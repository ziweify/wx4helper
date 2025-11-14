# 简化方案 - 检查进程ID

## 🎯 核心思路

**不需要监听进程退出事件**，只需要：

1. 浏览器连接时，传递 **进程ID**
2. `BetConfig` 存储 **进程ID**
3. 监控任务启动前，检查 **进程ID是否还存在**
4. 如果进程还在 → 不启动新浏览器

---

## 📝 实现步骤

### 步骤 1：BetConfig 添加 ProcessId 字段

```csharp
public class BetConfig : INotifyPropertyChanged
{
    // ... 现有字段 ...
    
    public int ProcessId { get; set; }  // 🔥 浏览器进程ID
    
    // ...
}
```

---

### 步骤 2：浏览器连接时传递进程ID

```csharp
// BsBrowserClient/Services/SocketServer.cs
public async Task ConnectAndListenAsync(CancellationToken cancellationToken)
{
    // ... 连接到 VxMain ...
    
    var handshake = new
    {
        type = "hello",
        configId = _configId,
        configName = _configName,
        processId = Process.GetCurrentProcess().Id  // 🔥 传递进程ID
    };
    
    await _writer.WriteLineAsync(JsonConvert.SerializeObject(handshake));
    await _writer.FlushAsync();
}
```

---

### 步骤 3：AutoBetService 接收并存储进程ID

```csharp
// AutoBetService.OnBrowserConnected
private void OnBrowserConnected(string configName, int browserConfigId)
{
    // ... 查找配置 ...
    
    var connection = _socketServer.GetConnection(browserConfigId);
    if (connection == null) return;
    
    // 🔥 从握手消息中提取进程ID
    // （需要修改 AutoBetSocketServer 在握手时保存进程ID）
    config.ProcessId = browserConfigId;  // 假设从握手消息中获取
    
    // ... 创建 BrowserClient ...
}
```

---

### 步骤 4：监控任务启动前检查进程是否存在

```csharp
private void MonitorBrowsers(object? state)
{
    foreach (var config in enabledConfigs)
    {
        // ... 检查连接状态 ...
        
        if (!isConnected)
        {
            // 🔥 在启动新浏览器前，检查进程是否还在
            if (config.ProcessId > 0 && IsProcessRunning(config.ProcessId))
            {
                _log.Info("AutoBet", $"⏳ 配置 [{config.ConfigName}] 浏览器进程 {config.ProcessId} 仍在运行，等待重连...");
                continue;  // 🔥 进程还在，不启动新的
            }
            
            // 进程不存在，可以启动新浏览器
            // ...
        }
    }
}

// 🔥 检查进程是否运行
private bool IsProcessRunning(int processId)
{
    try
    {
        var process = Process.GetProcessById(processId);
        return !process.HasExited;
    }
    catch
    {
        return false;  // 进程不存在
    }
}
```

---

## ✅ 优点

1. **简单直接**：不需要事件、不需要监听
2. **逻辑清晰**：启动前检查进程是否存在
3. **无副作用**：不依赖事件触发时机

---

## 📊 完整流程

```
[T+0s] 浏览器连接 → 传递 ProcessId=1234
       AutoBetService 存储: config.ProcessId = 1234

[T+3s] 用户手动关闭浏览器窗口
       进程 1234 被 Kill

[T+6s] 监控任务执行:
       检查: IsConnected = false
       检查: IsProcessRunning(1234) = false  ← 🔥 进程已不存在
       检查: IsEnabled = true
       
       → 启动新浏览器 ✅（合理，因为进程已不存在）

[T+6s] 用户主动关闭（UI开关）:
       StopBrowser() → config.IsEnabled = false
       
[T+9s] 监控任务执行:
       检查: IsEnabled = false
       
       → 跳过 ✅（合理，因为用户主动关闭）
```

---

## 🚀 修改计划

1. `BetConfig` 添加 `ProcessId` 字段
2. `BsBrowserClient` 握手时传递 `processId`
3. `AutoBetSocketServer` 解析并保存 `processId`
4. `AutoBetService.OnBrowserConnected` 更新 `config.ProcessId`
5. `MonitorBrowsers` 启动前检查 `IsProcessRunning(config.ProcessId)`

**这个方案更简单、更清晰！**

