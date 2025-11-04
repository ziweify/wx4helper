# 📡 WeixinX Socket 通信方案

## 🎯 设计目标

- ✅ **轻量化** - 使用 Windows Socket API，无需额外 DLL
- ✅ **防粘包** - 4字节长度头 + JSON消息体
- ✅ **异步非阻塞** - 客户端不会卡死
- ✅ **支持重连** - 自动处理连接断开
- ✅ **双向通信** - 支持请求/响应和服务器推送
- ✅ **线程安全** - 多线程环境下安全使用

---

## 📋 通信协议

### 消息格式

```
[4字节长度（网络字节序）][JSON消息体（UTF-8）]
```

### JSON 协议

#### 1. 客户端请求
```json
{
  "id": 1,                    // 请求ID，用于匹配响应
  "method": "GetContacts",    // 方法名
  "params": []                // 参数数组（按顺序）
}
```

#### 2. 服务器响应
```json
{
  "id": 1,                    // 对应的请求ID
  "result": {...},            // 成功时的结果
  "error": null               // 错误信息
}
```

#### 3. 服务器主动推送
```json
{
  "method": "OnMessage",      // 事件名
  "params": {...}             // 事件数据
}
```

---

## 🔧 服务端实现 (WeixinX - C++)

### 1. 核心类

#### `SocketServer` - Socket 服务器
- 监听端口：6328
- 自动管理客户端连接
- 命令分发和处理

#### `ClientConnection` - 客户端连接
- 独立接收线程
- 防粘包机制
- 自动重连处理

#### `SocketCommands` - 命令处理器
- 注册和分发命令
- 统一的错误处理

### 2. 注册的命令

| 命令 | 参数 | 返回 | 说明 |
|------|------|------|------|
| `GetContacts` | 无 | 联系人数组 | 获取所有联系人 |
| `GetGroupContacts` | `[groupId]` | 群成员数组 | 获取群成员列表 |
| `SendMessage` | `[wxid, message]` | 发送结果 | 发送消息 |
| `GetUserInfo` | 无 | 用户信息 | 获取当前登录用户信息 |

### 3. 添加新命令

```cpp
// SocketCommands.cpp
Json::Value HandleMyCommand(const Json::Value& params)
{
    // 参数验证
    if (params.empty()) {
        Json::Value error;
        error["error"] = "Missing parameter";
        return error;
    }
    
    // 处理逻辑
    Json::Value result;
    result["data"] = "处理结果";
    return result;
}

// 注册
server->RegisterHandler("MyCommand", HandleMyCommand);
```

### 4. 服务器推送

```cpp
// 获取服务器实例
auto& core = WeixinX::util::Singleton<WeixinX::Core>::Get();
auto* server = core.GetSocketServer();

// 推送消息到所有客户端
Json::Value data;
data["type"] = "text";
data["content"] = "新消息";
server->Broadcast("OnMessage", data);
```

---

## 💻 客户端实现 (BaiShengVx3Plus - C#)

### 1. 核心接口

#### `IWeixinSocketClient`
```csharp
public interface IWeixinSocketClient : IDisposable
{
    bool IsConnected { get; }
    
    Task<bool> ConnectAsync(string host = "127.0.0.1", int port = 6328, int timeoutMs = 5000);
    void Disconnect();
    
    Task<TResult?> SendAsync<TResult>(string method, params object[] parameters) where TResult : class;
    Task<TResult?> SendAsync<TResult>(string method, int timeoutMs, params object[] parameters) where TResult : class;
    
    event EventHandler<ServerPushEventArgs>? OnServerPush;
}
```

### 2. 使用示例

#### 基本用法
```csharp
public class MyService
{
    private readonly IWeixinSocketClient _client;
    private readonly ILogService _logService;
    
    public MyService(IWeixinSocketClient client, ILogService logService)
    {
        _client = client;
        _logService = logService;
        
        // 订阅服务器推送事件
        _client.OnServerPush += OnServerPush;
    }
    
    // 连接到服务器
    public async Task<bool> ConnectAsync()
    {
        return await _client.ConnectAsync("127.0.0.1", 6328);
    }
    
    // 获取联系人列表
    public async Task<List<Contact>?> GetContactsAsync()
    {
        try
        {
            var contacts = await _client.SendAsync<List<Contact>>("GetContacts");
            return contacts;
        }
        catch (Exception ex)
        {
            _logService.Error("MyService", "获取联系人失败", ex);
            return null;
        }
    }
    
    // 获取群成员（带参数）
    public async Task<List<Member>?> GetGroupContactsAsync(string groupId)
    {
        try
        {
            var members = await _client.SendAsync<List<Member>>(
                "GetGroupContacts", 
                groupId  // 参数按顺序传递
            );
            return members;
        }
        catch (Exception ex)
        {
            _logService.Error("MyService", "获取群成员失败", ex);
            return null;
        }
    }
    
    // 发送消息（多个参数）
    public async Task<SendResult?> SendMessageAsync(string wxid, string message)
    {
        try
        {
            var result = await _client.SendAsync<SendResult>(
                "SendMessage",
                10000,  // 超时时间（毫秒）
                wxid,   // 参数1
                message // 参数2
            );
            return result;
        }
        catch (Exception ex)
        {
            _logService.Error("MyService", "发送消息失败", ex);
            return null;
        }
    }
    
    // 处理服务器推送
    private void OnServerPush(object? sender, ServerPushEventArgs e)
    {
        _logService.Info("MyService", $"收到推送: {e.Method}");
        
        switch (e.Method)
        {
            case "OnMessage":
                // 处理新消息
                break;
            case "OnStatusChange":
                // 处理状态变更
                break;
        }
    }
}
```

#### 在 VxMain 中使用
```csharp
public partial class VxMain : UIForm
{
    private readonly IWeixinSocketClient _socketClient;
    private readonly ILogService _logService;
    
    public VxMain(
        IWeixinSocketClient socketClient,
        ILogService logService)
    {
        InitializeComponent();
        _socketClient = socketClient;
        _logService = logService;
        
        // 订阅服务器推送
        _socketClient.OnServerPush += OnServerPush;
    }
    
    private async void VxMain_Load(object sender, EventArgs e)
    {
        // 连接到服务器
        bool connected = await _socketClient.ConnectAsync();
        if (connected)
        {
            _logService.Info("VxMain", "Socket连接成功");
            lblStatus.Text = "已连接到微信服务";
        }
        else
        {
            _logService.Error("VxMain", "Socket连接失败");
            UIMessageBox.ShowError("无法连接到微信服务，请确保微信已注入 WeixinX.dll");
        }
    }
    
    private async void btnGetContacts_Click(object sender, EventArgs e)
    {
        try
        {
            lblStatus.Text = "正在获取联系人...";
            
            // 发送请求（不会阻塞UI）
            var contacts = await _socketClient.SendAsync<List<WxContact>>("GetContacts");
            
            if (contacts != null)
            {
                // 更新UI
                _contactsBindingList.Clear();
                foreach (var contact in contacts)
                {
                    _contactsBindingList.Add(contact);
                }
                
                lblStatus.Text = $"获取到 {contacts.Count} 个联系人";
                _logService.Info("VxMain", $"获取联系人成功: {contacts.Count} 个");
            }
            else
            {
                lblStatus.Text = "获取联系人失败";
                UIMessageBox.ShowError("获取联系人失败");
            }
        }
        catch (Exception ex)
        {
            _logService.Error("VxMain", "获取联系人异常", ex);
            UIMessageBox.ShowError($"获取联系人异常: {ex.Message}");
        }
    }
    
    private async void btnGetGroupMembers_Click(object sender, EventArgs e)
    {
        if (dgvContacts.CurrentRow?.DataBoundItem is WxContact contact)
        {
            if (!contact.IsGroup)
            {
                UIMessageBox.ShowWarning("请选择一个群聊");
                return;
            }
            
            lblStatus.Text = $"正在获取群成员 {contact.Nickname}...";
            
            // 带参数的请求
            var members = await _socketClient.SendAsync<List<WxContact>>(
                "GetGroupContacts",
                contact.Wxid  // 群ID参数
            );
            
            if (members != null)
            {
                _membersBindingList.Clear();
                foreach (var member in members)
                {
                    _membersBindingList.Add(member);
                }
                
                lblStatus.Text = $"获取到 {members.Count} 个群成员";
            }
        }
    }
    
    // 处理服务器主动推送
    private void OnServerPush(object? sender, ServerPushEventArgs e)
    {
        // 使用 Invoke 更新 UI（因为是后台线程调用）
        this.Invoke(() =>
        {
            switch (e.Method)
            {
                case "OnMessage":
                    // 收到新消息
                    lblStatus.Text = "收到新消息";
                    _logService.Info("VxMain", "收到服务器推送的新消息");
                    break;
                    
                case "OnContactUpdate":
                    // 联系人更新
                    lblStatus.Text = "联系人已更新";
                    break;
            }
        });
    }
    
    private void VxMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        // 断开连接
        _socketClient.Disconnect();
    }
}
```

---

## 🎨 特性说明

### 1. 异步非阻塞
```csharp
// ✅ 正确：异步调用，不阻塞UI
var result = await _client.SendAsync<MyResult>("MyMethod");

// ❌ 错误：同步等待会阻塞UI
var result = _client.SendAsync<MyResult>("MyMethod").Result;
```

### 2. 超时处理
```csharp
// 默认超时 10 秒
var result = await _client.SendAsync<MyResult>("SlowMethod");

// 自定义超时 30 秒
var result = await _client.SendAsync<MyResult>("SlowMethod", 30000);
```

### 3. 错误处理
```csharp
try
{
    var result = await _client.SendAsync<MyResult>("MyMethod");
    if (result == null)
    {
        // 请求失败（超时、网络错误、服务器返回error）
    }
}
catch (Exception ex)
{
    // 异常处理
    _logService.Error("Service", "请求异常", ex);
}
```

### 4. 重连机制
```csharp
// 检查连接状态
if (!_client.IsConnected)
{
    // 重新连接
    await _client.ConnectAsync();
}
```

---

## 🔥 性能优化

### 服务端
- ✅ 多线程处理（每个客户端独立线程）
- ✅ 智能内存管理（unique_ptr）
- ✅ 消息大小限制（最大10MB）
- ✅ 优雅断开（不会卡死）

### 客户端
- ✅ 异步IO（不阻塞UI线程）
- ✅ 并发字典（ConcurrentDictionary）
- ✅ 自动超时处理
- ✅ 线程安全的事件分发

---

## 📌 注意事项

### 1. 线程安全
```csharp
// ❌ 错误：直接在后台线程更新UI
_client.OnServerPush += (s, e) => {
    lblStatus.Text = "更新";  // 会抛出异常
};

// ✅ 正确：使用 Invoke 更新UI
_client.OnServerPush += (s, e) => {
    this.Invoke(() => {
        lblStatus.Text = "更新";
    });
};
```

### 2. 生命周期管理
```csharp
// 窗口关闭时断开连接
protected override void OnFormClosing(FormClosingEventArgs e)
{
    _socketClient.Disconnect();
    base.OnFormClosing(e);
}
```

### 3. 参数顺序
```csharp
// 参数按定义顺序传递，没有参数名
await _client.SendAsync<Result>("SendMessage",
    "wxid_123",  // 第1个参数：wxid
    "Hello"      // 第2个参数：message
);
```

---

## 🚀 编译和部署

### WeixinX (C++)
1. 添加文件到项目：
   - `SocketServer.h` / `SocketServer.cpp`
   - `SocketCommands.h` / `SocketCommands.cpp`
2. 链接 `ws2_32.lib`
3. 编译生成 `WeixinX.dll`

### BaiShengVx3Plus (C#)
1. 添加服务到 DI 容器（已完成）
2. 在需要的地方注入 `IWeixinSocketClient`
3. 编译运行

---

## 📊 测试流程

1. ✅ 编译 WeixinX.dll
2. ✅ 注入到微信进程
3. ✅ 启动 BaiShengVx3Plus
4. ✅ 连接到 Socket 服务器
5. ✅ 测试命令调用
6. ✅ 测试服务器推送

---

**🎉 通信方案已完成！轻量、高效、易用！**

