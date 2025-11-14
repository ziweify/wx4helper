# Socket 连接冲突问题 - 分析和修复

## 📋 问题描述

投注命令返回错误："未连接到浏览器"，但浏览器日志显示已成功连接。

```
✅ 找到浏览器客户端: configId=1
📥 投注结果:配置1 成功=False
投注命令返回:Success=False, Error=未连接到浏览器
```

浏览器日志：
```
[20:00:16.416] 🔌 ✅ 已连接到 VxMain
[20:00:16.485] 🔌 📤 已发送握手，配置ID: 1，配置名: 默认配置
[20:00:16.502] 🔌 ✅ 握手成功: 连接成功
```

---

## 🔍 问题根源

### Socket 连接冲突

```csharp
// 1. AutoBetSocketServer.HandleClientAsync:
private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
{
    var stream = client.GetStream();
    reader = new StreamReader(stream, Encoding.UTF8);  // ← 第一个 reader
    writer = new StreamWriter(stream, Encoding.UTF8);  // ← 第一个 writer
    
    // 保存连接
    _connections[configId] = new ClientConnection
    {
        Client = client,
        Reader = reader,
        Writer = writer
    };
    
    // 通知 AutoBetService
    _onBrowserConnected(configName, client);  // ← 传递 client
    
    // 持续读取消息
    while (!cancellationToken.IsCancellationRequested)
    {
        var line = await reader.ReadLineAsync(cancellationToken);  // ← 读取消息
        // ...
    }
}

// 2. BrowserClient.AttachConnection:
public void AttachConnection(TcpClient socket)
{
    _socket = socket;
    var stream = _socket.GetStream();
    _reader = new StreamReader(stream, utf8NoBom);  // ← 第二个 reader（冲突！）
    _writer = new StreamWriter(stream, utf8NoBom);  // ← 第二个 writer（冲突！）
}

// 3. BrowserClient.SendCommandAsync:
public async Task<BetResult> SendCommandAsync(string command, object? data = null)
{
    await _writer!.WriteLineAsync(json);  // ← 使用第二个 writer 发送命令
    // 等待响应...
}
```

### 问题分析

1. **同一个 Socket，两个 `StreamReader`**：
   - `AutoBetSocketServer` 创建了 `reader` 并在循环中读取消息
   - `BrowserClient` 又创建了 `_reader`，也想读取响应
   - **冲突！** 两个 `StreamReader` 从同一个流读取数据，会导致数据混乱

2. **命令发送可能成功，但响应丢失**：
   - `BrowserClient` 通过 `_writer` 发送命令
   - 浏览器响应
   - `AutoBetSocketServer` 的循环读取到响应，触发 `_onMessageReceived`
   - 但 `BrowserClient.SendCommandAsync` 在等待 `tcs.Task`，超时失败

3. **更严重的问题：`IsConnected` 检查失败**：
   - `BrowserClient.IsConnected => _socket != null && _socket.Connected`
   - 但问题可能是 `_socket.Connected` 返回 `false`
   - 或者 `_socket` 根本就是 `null`？

---

## ✅ 解决方案

### 方案1：`BrowserClient` 不直接使用 Socket

`BrowserClient` 不应该创建自己的 `reader/writer`，而应该通过 `AutoBetSocketServer` 发送命令。

```csharp
// BrowserClient.cs
public class BrowserClient
{
    private readonly int _configId;
    private Process? _process;
    private ClientConnection? _connection;  // ← 存储连接引用，而不是 Socket
    
    public bool IsConnected => _connection != null && _connection.IsConnected;
    
    public void AttachConnection(ClientConnection connection)  // ← 改为接收 ClientConnection
    {
        _connection = connection;
    }
    
    public async Task<BetResult> SendCommandAsync(string command, object? data = null)
    {
        if (!IsConnected)
        {
            return new BetResult
            {
                Success = false,
                ErrorMessage = "未连接到浏览器"
            };
        }
        
        // 通过 ClientConnection 发送命令
        var success = await _connection!.SendCommandAsync(command, data);
        
        // 等待响应（通过 OnMessageReceived 回调）
        // ...
    }
}
```

```csharp
// AutoBetService.cs
private void OnBrowserConnected(string configName, TcpClient client)
{
    // 查找配置...
    
    // 🔥 从 AutoBetSocketServer 获取 ClientConnection
    var connection = _socketServer.GetConnection(configId);
    
    if (_browsers.TryGetValue(configId, out var existingBrowser))
    {
        existingBrowser.AttachConnection(connection);  // ← 传递 ClientConnection
    }
    else
    {
        var browserClient = new BrowserClient(configId);
        browserClient.AttachConnection(connection);  // ← 传递 ClientConnection
        _browsers[configId] = browserClient;
    }
}
```

### 方案2：`AutoBetSocketServer` 提供 `GetConnection` 方法

```csharp
// AutoBetSocketServer.cs
public ClientConnection? GetConnection(int configId)
{
    lock (_connections)
    {
        return _connections.TryGetValue(configId, out var conn) ? conn : null;
    }
}
```

---

## 🎯 实施步骤

1. ✅ 修改 `BrowserClient.AttachConnection` 签名，接收 `ClientConnection` 而不是 `TcpClient`
2. ✅ 修改 `AutoBetService.OnBrowserConnected`，从 `_socketServer` 获取 `ClientConnection`
3. ✅ 修改 `AutoBetSocketServer.OnBrowserConnected` 回调签名，传递 `configId` 而不是 `TcpClient`
4. ✅ 在 `AutoBetSocketServer` 中添加 `GetConnection(int configId)` 方法

---

## 📝 待确认

添加调试日志后，需要确认：
1. `BrowserClient.AttachConnection` 是否被调用？
2. 传入的 `socket` 是否为 `null`？
3. 传入的 `socket.Connected` 是 `true` 还是 `false`？
4. `AttachConnection` 完成后，`IsConnected` 的值是什么？
5. `SendCommandAsync` 调用时，`IsConnected` 的值是什么？

**如果 `_socket.Connected` 是 `false`，说明 Socket 确实有问题。**
**如果 `_socket` 是 `null`，说明 `AttachConnection` 根本没被调用。**

等待用户提供新的日志输出后，再确定具体修复方案。

