# 消息处理架构 - 快速上手指南

## 📋 概述

这个架构用于处理 WeixinX 服务器主动推送的消息，如聊天消息、登录/登出事件、群成员变动等。

---

## 🎯 核心概念

### 1. 消息流程

```
WeixinX (C++)  →  WeixinSocketClient  →  MessageDispatcher  →  具体 Handler
                        ↓
                  OnServerPush 事件
                        ↓
                    VxMain 处理
```

### 2. 两种通信模式

#### 模式 1: 请求-响应（Request-Response）

**用途**：主动查询数据

```csharp
// 客户端主动请求
var contacts = await _socketClient.SendAsync<JsonDocument>("GetContacts");

// 服务器响应
{
  "id": 1,
  "result": [...],
  "error": null
}
```

#### 模式 2: 服务器推送（Server Push）

**用途**：被动接收事件

```csharp
// 服务器主动推送
{
  "method": "OnMessage",
  "params": {
    "sender": "wxid_xxx",
    "content": "Hello"
  }
}

// 客户端通过 OnServerPush 事件接收
_socketClient.OnServerPush += (sender, e) => {
    // MessageDispatcher 自动分发到对应的 Handler
};
```

**两种模式不会冲突**！

---

## 🚀 添加新消息类型

### 步骤 1: 添加枚举

**文件**: `BaiShengVx3Plus/Models/ServerMessages.cs`

```csharp
public enum ServerMessageType
{
    OnMessage,
    OnLogin,
    OnLogout,
    OnMemberJoin,
    OnMemberLeave,
    OnHeartbeat,
    OnFriendRequest,  // 👈 新增
    Unknown
}
```

### 步骤 2: 创建数据模型（可选）

```csharp
public class FriendRequestData
{
    [JsonPropertyName("fromWxid")]
    public string FromWxid { get; set; } = string.Empty;

    [JsonPropertyName("fromNickname")]
    public string FromNickname { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}
```

### 步骤 3: 创建处理器

**文件**: `BaiShengVx3Plus/Services/Messages/Handlers/FriendRequestHandler.cs`

```csharp
using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    public class FriendRequestHandler : IMessageHandler
    {
        private readonly ILogService _logService;

        public ServerMessageType MessageType => ServerMessageType.OnFriendRequest;

        public FriendRequestHandler(ILogService logService)
        {
            _logService = logService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                var request = JsonSerializer.Deserialize<FriendRequestData>(data.GetRawText());
                if (request == null)
                {
                    _logService.Error("FriendRequestHandler", "Failed to deserialize data");
                    return;
                }

                _logService.Info("FriendRequestHandler", 
                    $"👥 收到好友请求 | 来自: {request.FromNickname} ({request.FromWxid})");

                // TODO: 处理好友请求
                // 1. 保存到数据库
                // 2. 通知 UI
                // 3. 自动同意（可选）

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("FriendRequestHandler", "Error handling friend request", ex);
            }
        }
    }
}
```

### 步骤 4: 注册到 DI 容器

**文件**: `BaiShengVx3Plus/Program.cs`

```csharp
// 在 ConfigureServices 中添加
services.AddTransient<IMessageHandler, FriendRequestHandler>();
```

**完成！** 无需修改任何其他代码。

---

## 💻 在处理器中使用其他服务

### 注入服务

```csharp
public class ChatMessageHandler : IMessageHandler
{
    private readonly ILogService _logService;
    private readonly IDatabaseService _dbService;  // 注入数据库服务
    private readonly IContactBindingService _contactService;

    public ChatMessageHandler(
        ILogService logService,
        IDatabaseService dbService,
        IContactBindingService contactService)
    {
        _logService = logService;
        _dbService = dbService;
        _contactService = contactService;
    }

    public async Task HandleAsync(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<ChatMessageData>(data.GetRawText());
        if (message == null) return;

        // 1. 保存到数据库
        await SaveMessageToDatabase(message);

        // 2. 获取联系人信息
        var contact = await _contactService.GetContactByWxidAsync(message.Sender);

        // 3. 处理业务逻辑
        await ProcessBusinessLogic(message, contact);
    }

    private async Task SaveMessageToDatabase(ChatMessageData message)
    {
        // 使用 _dbService 保存消息
        // ...
    }

    private async Task ProcessBusinessLogic(ChatMessageData message, WxContact? contact)
    {
        // 业务逻辑处理
        // ...
    }
}
```

---

## 🎨 更新 UI（线程安全）

### 方法 1: 在处理器中使用 SynchronizationContext

```csharp
public class ChatMessageHandler : IMessageHandler
{
    private readonly ILogService _logService;
    private readonly SynchronizationContext? _uiContext;

    public ChatMessageHandler(ILogService logService)
    {
        _logService = logService;
        _uiContext = SynchronizationContext.Current; // 捕获 UI 线程上下文
    }

    public async Task HandleAsync(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<ChatMessageData>(data.GetRawText());
        if (message == null) return;

        // 处理业务逻辑...

        // 更新 UI（切换到 UI 线程）
        _uiContext?.Post(_ =>
        {
            // 在 UI 线程中执行
            // 例如：更新 DataGridView
        }, null);

        await Task.CompletedTask;
    }
}
```

### 方法 2: 使用事件通知（推荐）

**在 VxMain 中定义事件**：

```csharp
public partial class VxMain : UIForm
{
    // 定义事件
    public event EventHandler<ChatMessageData>? OnChatMessageReceived;
}
```

**在处理器中触发事件**：

```csharp
public class ChatMessageHandler : IMessageHandler
{
    private readonly ILogService _logService;
    private VxMain? _mainForm;

    public ChatMessageHandler(ILogService logService)
    {
        _logService = logService;
        
        // 从 DI 容器获取主窗口引用
        _mainForm = Program.ServiceProvider?.GetService<VxMain>();
    }

    public async Task HandleAsync(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<ChatMessageData>(data.GetRawText());
        if (message == null) return;

        // 触发事件（会在 UI 线程中处理）
        _mainForm?.OnChatMessageReceived?.Invoke(this, message);

        await Task.CompletedTask;
    }
}
```

**在 VxMain 中订阅事件**：

```csharp
private void VxMain_Load(object sender, EventArgs e)
{
    // 订阅事件
    this.OnChatMessageReceived += (s, data) =>
    {
        // 在 UI 线程中安全更新
        lblLastMessage.Text = $"{data.Sender}: {data.Content}";
        
        // 添加到 DataGridView
        // dgvMessages.Rows.Add(...);
    };
}
```

---

## 🔍 调试技巧

### 1. 查看日志

所有消息处理都会记录到日志：

```
[MessageDispatcher] 📨 Dispatching OnMessage to 1 handler(s)
[ChatMessageHandler] 💬 收到消息 | 发送者: wxid_xxx | 内容: Hello
```

打开日志窗口（`btnLog` 按钮）查看详细日志。

### 2. 设置断点

在处理器的 `HandleAsync` 方法中设置断点：

```csharp
public async Task HandleAsync(JsonElement data)
{
    // 👈 在这里设置断点
    var message = JsonSerializer.Deserialize<ChatMessageData>(data.GetRawText());
    // ...
}
```

### 3. 查看消息内容

```csharp
public async Task HandleAsync(JsonElement data)
{
    // 打印原始 JSON
    string rawJson = data.GetRawText();
    _logService.Debug("Handler", $"Raw JSON: {rawJson}");
    
    // 反序列化
    var message = JsonSerializer.Deserialize<ChatMessageData>(rawJson);
    // ...
}
```

---

## ⚠️ 常见问题

### Q1: 消息没有被处理？

**检查**：
1. ✅ 处理器是否注册到 DI 容器？
   ```csharp
   services.AddTransient<IMessageHandler, YourHandler>();
   ```

2. ✅ 处理器的 `MessageType` 是否匹配？
   ```csharp
   public ServerMessageType MessageType => ServerMessageType.OnMessage;
   ```

3. ✅ 服务器发送的 `method` 字符串是否正确？
   - 服务器发送: `"OnMessage"`
   - 枚举: `ServerMessageType.OnMessage`
   - **大小写会自动匹配**（`ignoreCase: true`）

### Q2: 处理器抛出异常？

**解决**：
- 所有异常都会被捕获并记录到日志
- 检查日志窗口查看详细错误信息
- 在处理器中添加 try-catch

```csharp
public async Task HandleAsync(JsonElement data)
{
    try
    {
        // 你的代码...
    }
    catch (Exception ex)
    {
        _logService.Error("YourHandler", "Error details", ex);
    }
}
```

### Q3: UI 更新失败？

**原因**：处理器运行在后台线程

**解决**：
- 使用 `Invoke` 或 `SynchronizationContext`
- 或使用事件通知（推荐）

```csharp
// ✅ 正确
_uiContext?.Post(_ => {
    lblStatus.Text = "Updated";
}, null);

// ❌ 错误（会抛出异常）
lblStatus.Text = "Updated";  // 跨线程操作
```

### Q4: 如何处理多个消息类型？

**方案 1**：创建多个处理器（推荐）

```csharp
public class LoginHandler : IMessageHandler
{
    public ServerMessageType MessageType => ServerMessageType.OnLogin;
    // ...
}

public class LogoutHandler : IMessageHandler
{
    public ServerMessageType MessageType => ServerMessageType.OnLogout;
    // ...
}
```

**方案 2**：一个处理器处理多种消息

不推荐，因为违反单一职责原则。

### Q5: 如何测试处理器？

**单元测试示例**：

```csharp
[TestClass]
public class ChatMessageHandlerTests
{
    [TestMethod]
    public async Task HandleAsync_ValidMessage_ShouldLogCorrectly()
    {
        // Arrange
        var mockLog = new Mock<ILogService>();
        var handler = new ChatMessageHandler(mockLog.Object);
        
        var jsonData = JsonDocument.Parse(@"
        {
            ""sender"": ""test_user"",
            ""content"": ""Hello World"",
            ""receiver"": ""me"",
            ""timestamp"": 1234567890,
            ""fromChatroom"": false
        }").RootElement;
        
        // Act
        await handler.HandleAsync(jsonData);
        
        // Assert
        mockLog.Verify(x => x.Info(
            "ChatMessageHandler", 
            It.Is<string>(s => s.Contains("test_user") && s.Contains("Hello World"))
        ), Times.Once);
    }
}
```

---

## 📚 示例代码

### 完整示例：处理聊天消息并保存到数据库

```csharp
using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    public class ChatMessageHandler : IMessageHandler
    {
        private readonly ILogService _logService;
        private readonly IDatabaseService _dbService;
        private VxMain? _mainForm;

        public ServerMessageType MessageType => ServerMessageType.OnMessage;

        public ChatMessageHandler(
            ILogService logService,
            IDatabaseService dbService)
        {
            _logService = logService;
            _dbService = dbService;
            _mainForm = Program.ServiceProvider?.GetService<VxMain>();
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                // 1. 反序列化消息
                var message = JsonSerializer.Deserialize<ChatMessageData>(data.GetRawText());
                if (message == null)
                {
                    _logService.Error("ChatMessageHandler", "Failed to deserialize message");
                    return;
                }

                _logService.Info("ChatMessageHandler", 
                    $"💬 收到消息 | 发送者: {message.Sender} | 内容: {message.Content}");

                // 2. 保存到数据库
                await SaveMessageToDatabase(message);

                // 3. 触发 UI 更新事件
                _mainForm?.OnChatMessageReceived?.Invoke(this, message);

                // 4. 处理业务逻辑（如自动回复）
                await ProcessAutoReply(message);
            }
            catch (Exception ex)
            {
                _logService.Error("ChatMessageHandler", "Error handling chat message", ex);
            }
        }

        private async Task SaveMessageToDatabase(ChatMessageData message)
        {
            try
            {
                var conn = await _dbService.GetConnectionAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO messages (sender, content, receiver, timestamp, from_chatroom)
                    VALUES (@sender, @content, @receiver, @timestamp, @from_chatroom)";
                
                cmd.Parameters.AddWithValue("@sender", message.Sender);
                cmd.Parameters.AddWithValue("@content", message.Content);
                cmd.Parameters.AddWithValue("@receiver", message.Receiver);
                cmd.Parameters.AddWithValue("@timestamp", message.Timestamp);
                cmd.Parameters.AddWithValue("@from_chatroom", message.FromChatroom);
                
                await cmd.ExecuteNonQueryAsync();
                _logService.Debug("ChatMessageHandler", "Message saved to database");
            }
            catch (Exception ex)
            {
                _logService.Error("ChatMessageHandler", "Failed to save message", ex);
            }
        }

        private async Task ProcessAutoReply(ChatMessageData message)
        {
            // 如果消息以 "/" 开头，触发命令
            if (message.Content.StartsWith("/"))
            {
                _logService.Info("ChatMessageHandler", $"检测到命令: {message.Content}");
                // TODO: 处理命令逻辑
            }

            await Task.CompletedTask;
        }
    }
}
```

---

## 🎉 总结

### 核心要点

1. **两种通信模式不冲突**：
   - 请求-响应：`SendAsync` (有 `id`)
   - 服务器推送：`OnServerPush` (没有 `id`)

2. **添加新消息类型只需 3 步**：
   - 添加枚举
   - 创建处理器
   - 注册到 DI

3. **处理器可以注入任何服务**：
   - `ILogService`
   - `IDatabaseService`
   - `IContactBindingService`
   - 等等...

4. **UI 更新要注意线程安全**：
   - 使用 `Invoke` 或 `SynchronizationContext`
   - 或使用事件通知

### 参考文档

- **架构设计**: `MESSAGE_HANDLING_ARCHITECTURE.md`
- **实现总结**: `MESSAGE_HANDLING_IMPLEMENTATION.md`
- **快速上手**: `MESSAGE_HANDLING_QUICK_START.md` (本文档)

---

**开始使用吧！** 🚀

