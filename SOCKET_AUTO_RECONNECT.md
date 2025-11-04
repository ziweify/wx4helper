# 🔄 Socket 自动重连策略

## 📋 **问题背景**

用户提出了3种Socket连接方案：
1. **程序启动时尝试连接** - 检测已运行的微信
2. **启动微信后再连接** - 注入成功后手动触发
3. **持续重试 + 自动重连** - 直到连接成功，掉线继续连接

---

## 🏆 **最终方案：混合策略（1 + 2 + 3）**

我们实现了**智能连接策略**，结合了所有3种方案的优点：

```
程序启动 (方案1)
   ↓
尝试连接 → 成功 → 工作状态 ✓
   ↓
   失败 → 启动自动重连 (方案3)
   ↓
后台每5秒重试一次
   ↓
用户点击"采集" (方案2)
   ↓
注入/启动微信 → 尝试连接
   ↓
连接成功 → 停止自动重连
   ↓
检测到断线 → 自动重新连接 (方案3)
```

---

## ✅ **实现细节**

### **1️⃣ 启动时自动连接（方案1）**

```csharp
// VxMain.cs - VxMain_Load
private async void VxMain_Load(object sender, EventArgs e)
{
    // 程序启动时尝试连接
    bool connected = await _socketClient.ConnectAsync("127.0.0.1", 6328, 2000);
    
    if (connected)
    {
        lblStatus.Text = "已连接到微信 ✓";
    }
    else
    {
        lblStatus.Text = "未连接（等待微信启动）";
        // 启动自动重连
        _socketClient.StartAutoReconnect(5000); // 每5秒重试一次
    }
}
```

**触发时机**：程序窗口加载时（Form_Load）

**好处**：
- ✅ 如果微信已经运行且已注入，立即可用
- ✅ 用户无需任何操作

---

### **2️⃣ 注入后手动连接（方案2）**

```csharp
// VxMain.cs - btnGetContactList_Click
private async void btnGetContactList_Click(object sender, EventArgs e)
{
    // 注入 WeixinX.dll 成功后
    if (_loaderService.InjectToProcess(...))
    {
        await Task.Delay(1000); // 等待服务器启动
        
        // 尝试连接
        await ConnectToSocketServerAsync();
    }
}
```

**触发时机**：用户点击"采集"按钮，注入成功后

**好处**：
- ✅ 确保在注入后立即连接
- ✅ 如果用户手动操作，提供即时反馈

---

### **3️⃣ 自动重连机制（方案3）**

#### **特性**

| 特性 | 说明 |
|------|------|
| **重连间隔** | 5秒（可配置） |
| **触发条件** | 1. 启动时连接失败<br>2. 连接断开时 |
| **停止条件** | 连接成功 |
| **线程安全** | 使用 CancellationToken |

#### **实现代码**

```csharp
// WeixinSocketClient.cs
public bool AutoReconnect { get; set; } = false;

public void StartAutoReconnect(int intervalMs = 5000)
{
    _reconnectTask = Task.Run(() => AutoReconnectLoop(...));
}

private async Task AutoReconnectLoop(CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        if (!IsConnected)
        {
            bool success = await ConnectAsync(_lastHost, _lastPort, 3000);
            
            if (success)
            {
                return; // 连接成功，退出循环
            }
        }
        
        await Task.Delay(_reconnectInterval, cancellationToken); // 等待5秒
    }
}
```

#### **断线自动重连**

```csharp
// WeixinSocketClient.cs - ReceiveLoop
finally
{
    Disconnect();
    
    // 如果启用了自动重连，开始重连
    if (_autoReconnect)
    {
        StartAutoReconnect(_reconnectInterval);
    }
}
```

---

## 📊 **状态流转图**

```
┌─────────────┐
│ 程序启动    │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│ 尝试连接        │◄───────────┐
│ (方案1)         │            │
└────┬────────────┘            │
     │                         │
     ├─成功──► [工作状态] ──断线┘
     │                         
     └─失败──► [自动重连状态]
                 (方案3)
                    │
                    │ 后台每5秒重试
                    │
                    ▼
            ┌──────────────┐
            │ 用户注入微信 │
            │ (方案2)      │
            └──────┬───────┘
                   │
                   ▼
            连接成功 → [工作状态]
```

---

## 🎯 **使用方式**

### **默认配置（推荐）**

```csharp
public VxMain(...)
{
    _socketClient = socketClient;
    
    // 启用自动重连（全局）
    _socketClient.AutoReconnect = true;
}
```

### **手动控制**

```csharp
// 启动自动重连（自定义间隔）
_socketClient.StartAutoReconnect(3000); // 每3秒重试

// 停止自动重连
_socketClient.StopAutoReconnect();

// 禁用自动重连
_socketClient.AutoReconnect = false;
```

---

## 📈 **性能优化**

### **1. 避免频繁重试**

- **重连间隔**: 5秒（平衡及时性和资源消耗）
- **连接超时**: 3秒（避免长时间阻塞）

### **2. 资源管理**

- 连接成功后**自动停止**重连循环
- 使用 `CancellationToken` 安全终止线程
- 窗口关闭时自动清理资源

### **3. 日志级别**

```csharp
// 调试模式：显示所有重连尝试
_logService.Info("WeixinSocketClient", "Attempting to reconnect...");

// 生产模式：只记录成功/失败
_logService.Info("WeixinSocketClient", "Reconnected successfully!");
_logService.Error("WeixinSocketClient", "Reconnect failed");
```

---

## 🚨 **常见问题**

### **Q1: 为什么要混合3种方案？**

**A**: 
- **方案1** - 快速连接已运行的微信（最佳用户体验）
- **方案2** - 确保注入后立即可用（手动操作反馈）
- **方案3** - 断线自动恢复（稳定性保证）

### **Q2: 会不会消耗太多资源？**

**A**: 不会
- 重连间隔5秒（不频繁）
- 连接超时3秒（快速失败）
- 连接成功后立即停止重连循环
- 使用异步模式，不阻塞UI

### **Q3: 用户需要做什么？**

**A**: **什么都不需要做！**
- 程序启动 → 自动尝试连接
- 连接失败 → 后台自动重试
- 断线 → 自动重连
- 完全无感知

### **Q4: 如何禁用自动重连？**

**A**: 
```csharp
_socketClient.AutoReconnect = false;
```

### **Q5: 如何查看连接状态？**

**A**: 
- **UI状态栏**: 显示实时连接状态
- **日志窗口**: 查看详细连接日志
- **代码检测**: `_socketClient.IsConnected`

---

## 🔧 **调试方法**

### **1. 测试启动时连接**

```
1. 先启动微信（已注入 WeixinX.dll）
2. 再启动 BaiShengVx3Plus
3. 观察状态栏：应显示 "已连接到微信 ✓"
```

### **2. 测试自动重连**

```
1. 启动 BaiShengVx3Plus（微信未运行）
2. 观察日志：每5秒显示 "Attempting to reconnect..."
3. 启动微信并注入 WeixinX.dll
4. 观察日志：显示 "Reconnected successfully!"
```

### **3. 测试断线重连**

```
1. BaiShengVx3Plus 已连接到微信
2. 关闭微信进程
3. 观察日志：显示 "Connection lost, starting reconnect..."
4. 重新启动微信并注入
5. 观察日志：显示 "Reconnected successfully!"
```

---

## 📝 **相关代码文件**

| 文件 | 说明 |
|------|------|
| `BaiShengVx3Plus/Services/IWeixinSocketClient.cs` | 接口定义（新增自动重连方法） |
| `BaiShengVx3Plus/Services/WeixinSocketClient.cs` | 实现自动重连逻辑 |
| `BaiShengVx3Plus/Views/VxMain.cs` | 启动时连接 + 注入后连接 |

---

## 🎉 **总结**

| 方案 | 场景 | 实现 | 状态 |
|------|------|------|------|
| **方案1** | 程序启动 | `VxMain_Load` | ✅ 已实现 |
| **方案2** | 用户注入 | `btnGetContactList_Click` | ✅ 已实现 |
| **方案3** | 断线重连 | `AutoReconnectLoop` | ✅ 已实现 |

**关键特性**：
- ✅ 启动时自动连接
- ✅ 注入后立即连接
- ✅ 断线自动重连
- ✅ 后台持续重试
- ✅ 无需用户干预
- ✅ 完全自动化

**用户体验**：
- 🚀 打开程序立即可用（如果微信已运行）
- 🔄 微信启动时自动连接（无需手动操作）
- 💪 网络断开自动恢复（稳定可靠）

---

**文档创建时间**: 2025-11-04  
**最后更新**: 2025-11-04

