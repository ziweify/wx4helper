# 简化方案 - 检查进程ID - 最终实现

## ✅ 实现完成

已按照用户建议实施**简单直接的方案**：不监听进程退出事件，只检查进程ID是否还在运行。

---

## 🔧 修改内容

### 1. BetConfig 添加 ProcessId 字段

```csharp
// BaiShengVx3Plus/Models/AutoBet/BetConfig.cs
public int ProcessId { get; set; } = 0;
```

---

### 2. 浏览器连接时传递进程ID

```csharp
// BsBrowserClient/Services/SocketServer.cs
var handshake = new
{
    type = "hello",
    configId = _configId,
    configName = _configName,
    processId = System.Diagnostics.Process.GetCurrentProcess().Id  // 🔥 传递进程ID
};
```

---

### 3. AutoBetSocketServer 解析进程ID

```csharp
// BaiShengVx3Plus/Services/AutoBet/AutoBetSocketServer.cs
var processId = handshake["processId"]?.ToObject<int>() ?? 0;

// 修改回调签名
private readonly Action<string, int, int> _onBrowserConnected;  // (configName, configId, processId)

// 触发事件时传递进程ID
_onBrowserConnected(configName, configId, processId);
```

---

### 4. AutoBetService 接收并存储进程ID

```csharp
// BaiShengVx3Plus/Services/AutoBet/AutoBetService.cs
private void OnBrowserConnected(string configName, int browserConfigId, int processId)
{
    // ... 查找配置 ...
    
    // 🔥 保存进程ID到配置
    config.ProcessId = processId;
    SaveConfig(config);
    _log.Info("AutoBet", $"✅ 已保存进程ID: {processId}");
    
    // ... 创建 BrowserClient ...
}
```

---

### 5. 监控任务启动前检查进程是否存在

```csharp
// BaiShengVx3Plus/Services/AutoBet/AutoBetService.cs
private void MonitorBrowsers(object? state)
{
    foreach (var config in enabledConfigs)
    {
        // ... 检查连接状态 ...
        
        if (!isConnected)
        {
            // 🔥 检查进程是否还在运行
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
        var process = System.Diagnostics.Process.GetProcessById(processId);
        return !process.HasExited;
    }
    catch
    {
        return false;  // 进程不存在
    }
}
```

---

### 6. 删除复杂的事件订阅代码

- ❌ 删除 `BrowserClient.OnProcessExited` 事件
- ❌ 删除 `_process.EnableRaisingEvents` 和 `_process.Exited` 订阅
- ❌ 删除 `StartBrowserInternal` 中的事件订阅逻辑

---

## 📊 完整流程

```
[T+0s] 浏览器连接 → 传递 ProcessId=1234
       AutoBetService: config.ProcessId = 1234

[T+3s] 用户手动关闭浏览器窗口
       进程 1234 被 Kill

[T+6s] 监控任务执行:
       1. 检查: IsConnected = false
       2. 检查: IsProcessRunning(1234) = false  ← 🔥 进程已不存在
       3. 检查: IsEnabled = true
       
       → 启动新浏览器 ✅（合理，因为进程已不存在）

[T+0s] 用户主动关闭（UI开关）:
       StopBrowser() → config.IsEnabled = false

[T+3s] 监控任务执行:
       1. 检查: IsEnabled = false
       
       → 跳过 ✅（合理，因为用户主动关闭）

[T+0s] 浏览器意外崩溃但连接未断:
       进程 1234 被 Kill
       但 Socket 连接可能还保持

[T+3s] 监控任务执行:
       1. 检查: IsConnected = false（Socket 最终断开）
       2. 检查: IsProcessRunning(1234) = false  ← 🔥 进程已不存在
       3. 检查: IsEnabled = true
       
       → 启动新浏览器 ✅（合理，自动恢复）

[T+0s] 用户关闭浏览器但进程卡住:
       浏览器窗口关闭，但进程 1234 仍在运行

[T+3s] 监控任务执行:
       1. 检查: IsConnected = false
       2. 检查: IsProcessRunning(1234) = true  ← 🔥 进程还在
       
       → 跳过，不启动新浏览器 ✅（等待进程退出或重连）
```

---

## ✅ 优点

1. **简单直接**：不需要事件、不需要监听
2. **逻辑清晰**：启动前检查进程是否存在
3. **无副作用**：不依赖事件触发时机
4. **鲁棒性强**：即使 Socket 连接断开，也能通过进程ID判断是否需要重启

---

## 🎯 核心原则

- **状态检查优于事件驱动**：定时检查进程状态，不依赖事件
- **简单优于复杂**：一个 `Process.GetProcessById()` 调用解决问题
- **防止重复启动**：进程还在就不启动新的

---

## 🚀 测试建议

1. **正常启动**：开启飞单 → 浏览器启动 → 连接成功
2. **手动关闭浏览器**：关闭浏览器窗口 → 监控任务检测到进程不存在 → 自动重启
3. **UI关闭飞单**：关闭飞单开关 → `IsEnabled=false` → 监控任务跳过
4. **主程序重启**：关闭主程序 → 浏览器继续运行 → 重启主程序 → 浏览器重连 → `ProcessId` 正确保存

**这是最简单、最直接、最鲁棒的方案！**

