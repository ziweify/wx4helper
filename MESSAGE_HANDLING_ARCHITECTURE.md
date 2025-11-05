# BaiShengVx3Plus 消息处理架构设计

## 📋 目录
1. [问题分析](#问题分析)
2. [架构设计](#架构设计)
3. [核心概念](#核心概念)
4. [实现方案](#实现方案)
5. [代码示例](#代码示例)
6. [最佳实践](#最佳实践)

---

## 🔍 问题分析

### 现有机制

#### 1. 请求-响应模式（Request-Response）
```csharp
// 客户端发起请求，等待响应
var contacts = await _socketClient.SendAsync<JsonDocument>("GetContacts");
```

**流程**：
```
Client                          Server
  |--- {id:1, method:"GetContacts"} -->|
  |                                     | (处理请求)
  |<-- {id:1, result:{...}, error:null}|
```

**机制**：
- 使用 `_pendingRequests` 字典存储待响应请求
- 通过 `id` 匹配请求和响应
- 支持超时机制

#### 2. 服务器推送模式（Server Push）
```csharp
// 服务器主动推送消息
_socketClient.OnServerPush += (sender, e) => { ... };
```

**流程**：
```
Client                          Server
  |                                     |
  |<-- {method:"OnMessage", params:{...}}| (主动推送)
  |                                     |
```

**机制**：
- 没有 `id` 字段（或 id 为特殊值）
- 有 `method` 字段标识消息类型
- 触发 `OnServerPush` 事件

### 关键问题

1. **会冲突吗？**
   ❌ **不会冲突**
   - 两种消息在 `ReceiveLoop` 中有明确区分
   - 有 `id` 的是响应 → 写入 `TaskCompletionSource`
   - 有 `method` 的是推送 → 触发 `OnServerPush` 事件

2. **如何优雅处理推送消息？**
   - 当前：所有消息都通过 `OnServerPush` 事件
   - 问题：需要在 UI 层手动判断 `method` 类型
   - 改进：使用消息处理器模式（Message Handler Pattern）

3. **可扩展性？**
   - 当前：添加新消息类型需要修改 UI 代码
   - 改进：每种消息类型一个处理器类
   - 符合：开闭原则（OCP）

---

## 🏗️ 架构设计

### 设计原则

1. **单一职责原则（SRP）**
   - 每个消息处理器只处理一种消息类型

2. **开闭原则（OCP）**
   - 添加新消息类型时，无需修改现有代码

3. **依赖倒置原则（DIP）**
   - 依赖于接口，而不是具体实现

### 架构图

```
┌─────────────────────────────────────────────────────────────┐
│                      WeixinSocketClient                      │
│  (底层 Socket 通信，区分请求-响应 vs 推送)                   │
└────────────────────────┬────────────────────────────────────┘
                         │ OnServerPush 事件
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                     MessageDispatcher                        │
│  (消息分发器，根据 method 路由到具体 Handler)                │
└────────────────────────┬────────────────────────────────────┘
                         │
        ┌────────────────┼────────────────┐
        ▼                ▼                ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ ChatMessage  │  │ LoginMessage │  │ MemberLeave  │
│   Handler    │  │   Handler    │  │   Handler    │
└──────────────┘  └──────────────┘  └──────────────┘
        │                │                │
        └────────────────┼────────────────┘
                         ▼
              ┌───────────────────┐
              │   UI 层 / 服务层  │
              │  (通过事件通知)   │
              └───────────────────┘
```

---

## 🎯 核心概念

### 1. 消息类型定义

```csharp
/// <summary>
/// 服务器推送消息类型
/// </summary>
public enum ServerMessageType
{
    /// <summary>
    /// 聊天消息
    /// </summary>
    OnMessage,
    
    /// <summary>
    /// 用户登录
    /// </summary>
    OnLogin,
    
    /// <summary>
    /// 用户登出
    /// </summary>
    OnLogout,
    
    /// <summary>
    /// 群成员加入
    /// </summary>
    OnMemberJoin,
    
    /// <summary>
    /// 群成员退出
    /// </summary>
    OnMemberLeave,
    
    /// <summary>
    /// 心跳
    /// </summary>
    OnHeartbeat,
    
    /// <summary>
    /// 未知消息
    /// </summary>
    Unknown
}
```

### 2. 消息数据模型

```csharp
/// <summary>
/// 聊天消息数据
/// </summary>
public class ChatMessageData
{
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public bool FromChatroom { get; set; }
}

/// <summary>
/// 登录/登出消息数据
/// </summary>
public class LoginEventData
{
    public string Wxid { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public long Timestamp { get; set; }
}

/// <summary>
/// 群成员变动数据
/// </summary>
public class MemberEventData
{
    public string GroupId { get; set; } = string.Empty;
    public string MemberWxid { get; set; } = string.Empty;
    public string MemberNickname { get; set; } = string.Empty;
    public long Timestamp { get; set; }
}
```

### 3. 消息处理器接口

```csharp
/// <summary>
/// 消息处理器接口
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// 处理器支持的消息类型
    /// </summary>
    ServerMessageType MessageType { get; }
    
    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="data">消息数据（JSON）</param>
    Task HandleAsync(JsonElement data);
}
```

### 4. 消息分发器

```csharp
/// <summary>
/// 消息分发器（单例，负责路由消息到具体处理器）
/// </summary>
public class MessageDispatcher
{
    private readonly Dictionary<ServerMessageType, List<IMessageHandler>> _handlers = new();
    private readonly ILogService _logService;

    public MessageDispatcher(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// 注册消息处理器
    /// </summary>
    public void RegisterHandler(IMessageHandler handler)
    {
        if (!_handlers.ContainsKey(handler.MessageType))
        {
            _handlers[handler.MessageType] = new List<IMessageHandler>();
        }
        _handlers[handler.MessageType].Add(handler);
        _logService.Info("MessageDispatcher", $"Registered handler for {handler.MessageType}");
    }

    /// <summary>
    /// 分发消息到对应的处理器
    /// </summary>
    public async Task DispatchAsync(string method, JsonElement data)
    {
        // 将 method 字符串转换为枚举
        if (!Enum.TryParse<ServerMessageType>(method, out var messageType))
        {
            messageType = ServerMessageType.Unknown;
            _logService.Warning("MessageDispatcher", $"Unknown message type: {method}");
        }

        // 查找对应的处理器
        if (_handlers.TryGetValue(messageType, out var handlers))
        {
            _logService.Info("MessageDispatcher", $"Dispatching {method} to {handlers.Count} handler(s)");
            
            // 并行执行所有处理器
            var tasks = handlers.Select(h => h.HandleAsync(data));
            await Task.WhenAll(tasks);
        }
        else
        {
            _logService.Warning("MessageDispatcher", $"No handler registered for {messageType}");
        }
    }
}
```

---

## 💻 实现方案

### 步骤 1: 创建消息模型和枚举

**文件**: `BaiShengVx3Plus/Models/ServerMessages.cs`

```csharp
using System.Text.Json.Serialization;

namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 服务器推送消息类型
    /// </summary>
    public enum ServerMessageType
    {
        OnMessage,       // 聊天消息
        OnLogin,         // 登录
        OnLogout,        // 登出
        OnMemberJoin,    // 群成员加入
        OnMemberLeave,   // 群成员退出
        OnHeartbeat,     // 心跳
        Unknown          // 未知
    }

    /// <summary>
    /// 聊天消息数据
    /// </summary>
    public class ChatMessageData
    {
        [JsonPropertyName("sender")]
        public string Sender { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("receiver")]
        public string Receiver { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("fromChatroom")]
        public bool FromChatroom { get; set; }
    }

    /// <summary>
    /// 登录/登出事件数据
    /// </summary>
    public class LoginEventData
    {
        [JsonPropertyName("wxid")]
        public string Wxid { get; set; } = string.Empty;

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// 群成员变动数据
    /// </summary>
    public class MemberEventData
    {
        [JsonPropertyName("groupId")]
        public string GroupId { get; set; } = string.Empty;

        [JsonPropertyName("memberWxid")]
        public string MemberWxid { get; set; } = string.Empty;

        [JsonPropertyName("memberNickname")]
        public string MemberNickname { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }
}
```

### 步骤 2: 创建消息处理器接口

**文件**: `BaiShengVx3Plus/Services/Messages/IMessageHandler.cs`

```csharp
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Messages
{
    /// <summary>
    /// 消息处理器接口
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// 处理器支持的消息类型
        /// </summary>
        ServerMessageType MessageType { get; }

        /// <summary>
        /// 处理消息（异步）
        /// </summary>
        /// <param name="data">消息数据（JSON）</param>
        Task HandleAsync(JsonElement data);
    }
}
```

### 步骤 3: 创建消息分发器

**文件**: `BaiShengVx3Plus/Services/Messages/MessageDispatcher.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Messages
{
    /// <summary>
    /// 消息分发器（负责路由消息到具体处理器）
    /// </summary>
    public class MessageDispatcher
    {
        private readonly Dictionary<ServerMessageType, List<IMessageHandler>> _handlers = new();
        private readonly ILogService _logService;

        public MessageDispatcher(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// 注册消息处理器
        /// </summary>
        public void RegisterHandler(IMessageHandler handler)
        {
            if (!_handlers.ContainsKey(handler.MessageType))
            {
                _handlers[handler.MessageType] = new List<IMessageHandler>();
            }

            _handlers[handler.MessageType].Add(handler);
            _logService.Info("MessageDispatcher", $"✓ Registered handler for {handler.MessageType}: {handler.GetType().Name}");
        }

        /// <summary>
        /// 分发消息到对应的处理器
        /// </summary>
        public async Task DispatchAsync(string method, object? data)
        {
            try
            {
                // 将 method 字符串转换为枚举
                if (!Enum.TryParse<ServerMessageType>(method, ignoreCase: true, out var messageType))
                {
                    messageType = ServerMessageType.Unknown;
                    _logService.Warning("MessageDispatcher", $"Unknown message type: {method}");
                }

                // 转换 data 为 JsonElement
                JsonElement jsonData;
                if (data == null)
                {
                    jsonData = JsonDocument.Parse("{}").RootElement;
                }
                else if (data is JsonElement element)
                {
                    jsonData = element;
                }
                else
                {
                    // 其他类型，先序列化再反序列化
                    string json = JsonSerializer.Serialize(data);
                    jsonData = JsonDocument.Parse(json).RootElement;
                }

                // 查找对应的处理器
                if (_handlers.TryGetValue(messageType, out var handlers))
                {
                    _logService.Info("MessageDispatcher", $"📨 Dispatching {method} to {handlers.Count} handler(s)");

                    // 并行执行所有处理器
                    var tasks = handlers.Select(h => h.HandleAsync(jsonData));
                    await Task.WhenAll(tasks);
                }
                else
                {
                    _logService.Warning("MessageDispatcher", $"⚠ No handler registered for {messageType}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("MessageDispatcher", $"Error dispatching message: {method}", ex);
            }
        }
    }
}
```

### 步骤 4: 创建具体的消息处理器

#### 4.1 聊天消息处理器

**文件**: `BaiShengVx3Plus/Services/Messages/Handlers/ChatMessageHandler.cs`

```csharp
using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    /// <summary>
    /// 聊天消息处理器
    /// </summary>
    public class ChatMessageHandler : IMessageHandler
    {
        private readonly ILogService _logService;

        public ServerMessageType MessageType => ServerMessageType.OnMessage;

        public ChatMessageHandler(ILogService logService)
        {
            _logService = logService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                // 反序列化为具体类型
                var message = JsonSerializer.Deserialize<ChatMessageData>(data.GetRawText());
                if (message == null)
                {
                    _logService.Error("ChatMessageHandler", "Failed to deserialize message");
                    return;
                }

                _logService.Info("ChatMessageHandler", 
                    $"💬 收到消息 | 发送者: {message.Sender} | 内容: {message.Content}");

                // TODO: 在这里处理聊天消息
                // 1. 保存到数据库
                // 2. 更新 UI 显示
                // 3. 触发业务逻辑（如自动回复）

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("ChatMessageHandler", "Error handling chat message", ex);
            }
        }
    }
}
```

#### 4.2 登录/登出消息处理器

**文件**: `BaiShengVx3Plus/Services/Messages/Handlers/LoginEventHandler.cs`

```csharp
using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    /// <summary>
    /// 登录事件处理器
    /// </summary>
    public class LoginEventHandler : IMessageHandler
    {
        private readonly ILogService _logService;

        public ServerMessageType MessageType => ServerMessageType.OnLogin;

        public LoginEventHandler(ILogService logService)
        {
            _logService = logService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                var loginData = JsonSerializer.Deserialize<LoginEventData>(data.GetRawText());
                if (loginData == null) return;

                _logService.Info("LoginEventHandler", 
                    $"✅ 微信登录 | Wxid: {loginData.Wxid} | 昵称: {loginData.Nickname}");

                // TODO: 处理登录事件
                // 1. 更新用户状态
                // 2. 刷新联系人列表
                // 3. 通知 UI 更新

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("LoginEventHandler", "Error handling login event", ex);
            }
        }
    }

    /// <summary>
    /// 登出事件处理器
    /// </summary>
    public class LogoutEventHandler : IMessageHandler
    {
        private readonly ILogService _logService;

        public ServerMessageType MessageType => ServerMessageType.OnLogout;

        public LogoutEventHandler(ILogService logService)
        {
            _logService = logService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                var logoutData = JsonSerializer.Deserialize<LoginEventData>(data.GetRawText());
                if (logoutData == null) return;

                _logService.Info("LogoutEventHandler", 
                    $"❌ 微信登出 | Wxid: {logoutData.Wxid} | 昵称: {logoutData.Nickname}");

                // TODO: 处理登出事件
                // 1. 清空用户数据
                // 2. 断开连接
                // 3. 通知 UI 更新

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("LogoutEventHandler", "Error handling logout event", ex);
            }
        }
    }
}
```

#### 4.3 群成员变动处理器

**文件**: `BaiShengVx3Plus/Services/Messages/Handlers/MemberEventHandler.cs`

```csharp
using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    /// <summary>
    /// 群成员加入处理器
    /// </summary>
    public class MemberJoinHandler : IMessageHandler
    {
        private readonly ILogService _logService;

        public ServerMessageType MessageType => ServerMessageType.OnMemberJoin;

        public MemberJoinHandler(ILogService logService)
        {
            _logService = logService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                var memberData = JsonSerializer.Deserialize<MemberEventData>(data.GetRawText());
                if (memberData == null) return;

                _logService.Info("MemberJoinHandler", 
                    $"👋 新成员加入 | 群: {memberData.GroupId} | 成员: {memberData.MemberNickname}");

                // TODO: 处理成员加入事件
                // 1. 更新群成员列表
                // 2. 发送欢迎消息
                // 3. 通知 UI 更新

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("MemberJoinHandler", "Error handling member join", ex);
            }
        }
    }

    /// <summary>
    /// 群成员退出处理器
    /// </summary>
    public class MemberLeaveHandler : IMessageHandler
    {
        private readonly ILogService _logService;

        public ServerMessageType MessageType => ServerMessageType.OnMemberLeave;

        public MemberLeaveHandler(ILogService logService)
        {
            _logService = logService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                var memberData = JsonSerializer.Deserialize<MemberEventData>(data.GetRawText());
                if (memberData == null) return;

                _logService.Info("MemberLeaveHandler", 
                    $"👋 成员退出 | 群: {memberData.GroupId} | 成员: {memberData.MemberNickname}");

                // TODO: 处理成员退出事件
                // 1. 更新群成员列表
                // 2. 记录退群日志
                // 3. 通知 UI 更新

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("MemberLeaveHandler", "Error handling member leave", ex);
            }
        }
    }
}
```

### 步骤 5: 集成到 DI 容器

**文件**: `BaiShengVx3Plus/Program.cs`

```csharp
// 在 ConfigureServices 方法中添加

// 注册消息分发器（单例）
services.AddSingleton<MessageDispatcher>();

// 注册消息处理器（瞬时）
services.AddTransient<IMessageHandler, ChatMessageHandler>();
services.AddTransient<IMessageHandler, LoginEventHandler>();
services.AddTransient<IMessageHandler, LogoutEventHandler>();
services.AddTransient<IMessageHandler, MemberJoinHandler>();
services.AddTransient<IMessageHandler, MemberLeaveHandler>();
```

### 步骤 6: 在启动时注册处理器

**文件**: `BaiShengVx3Plus/Program.cs` 的 `Main` 方法

```csharp
// 在显示主窗口之前
var logService = ServiceProvider.GetRequiredService<ILogService>();
var dispatcher = ServiceProvider.GetRequiredService<MessageDispatcher>();

// 注册所有消息处理器
var handlers = ServiceProvider.GetServices<IMessageHandler>();
foreach (var handler in handlers)
{
    dispatcher.RegisterHandler(handler);
}

logService.Info("Program", "Message handlers registered successfully");
```

### 步骤 7: 连接 Socket 客户端和分发器

**文件**: `BaiShengVx3Plus/Views/VxMain.cs`

```csharp
// 在构造函数中注入 MessageDispatcher
private readonly MessageDispatcher _messageDispatcher;

public VxMain(
    VxMainViewModel viewModel,
    IContactBindingService contactBindingService,
    IWeChatLoaderService loaderService,
    ILogService logService,
    IWeixinSocketClient socketClient,
    MessageDispatcher messageDispatcher) // 👈 新增
{
    InitializeComponent();
    _viewModel = viewModel;
    _contactBindingService = contactBindingService;
    _loaderService = loaderService;
    _logService = logService;
    _socketClient = socketClient;
    _messageDispatcher = messageDispatcher; // 👈 保存

    // 订阅服务器推送事件，并分发到消息处理器
    _socketClient.OnServerPush += async (sender, e) =>
    {
        try
        {
            await _messageDispatcher.DispatchAsync(e.Method, e.Data);
        }
        catch (Exception ex)
        {
            _logService.Error("VxMain", "Error dispatching message", ex);
        }
    };

    _logService.Info("VxMain", "主窗口已打开");
    // ... 其他代码 ...
}
```

---

## 🎨 最佳实践

### 1. 添加新消息类型

**示例：添加"好友请求"消息**

```csharp
// 1. 在枚举中添加
public enum ServerMessageType
{
    // ... 已有类型 ...
    OnFriendRequest,  // 👈 新增
}

// 2. 创建数据模型
public class FriendRequestData
{
    public string FromWxid { get; set; } = string.Empty;
    public string FromNickname { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long Timestamp { get; set; }
}

// 3. 创建处理器
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
        var request = JsonSerializer.Deserialize<FriendRequestData>(data.GetRawText());
        if (request == null) return;

        _logService.Info("FriendRequestHandler", 
            $"👥 好友请求 | 来自: {request.FromNickname} | 消息: {request.Message}");

        // 处理逻辑...
        await Task.CompletedTask;
    }
}

// 4. 注册到 DI 容器
services.AddTransient<IMessageHandler, FriendRequestHandler>();
```

### 2. 处理器之间的通信

如果一个处理器需要调用其他服务：

```csharp
public class ChatMessageHandler : IMessageHandler
{
    private readonly ILogService _logService;
    private readonly IDatabaseService _dbService;  // 👈 注入其他服务
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

        // 保存到数据库
        await _dbService.SaveMessageAsync(message);

        // 获取联系人信息
        var contact = await _contactService.GetContactByWxidAsync(message.Sender);

        // 处理逻辑...
    }
}
```

### 3. UI 更新（线程安全）

在处理器中更新 UI 需要注意线程切换：

```csharp
public class ChatMessageHandler : IMessageHandler
{
    private readonly ILogService _logService;
    private readonly SynchronizationContext _uiContext;

    public ChatMessageHandler(ILogService logService)
    {
        _logService = logService;
        _uiContext = SynchronizationContext.Current!; // 捕获 UI 线程上下文
    }

    public async Task HandleAsync(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<ChatMessageData>(data.GetRawText());
        if (message == null) return;

        // 切换到 UI 线程更新界面
        _uiContext.Post(_ =>
        {
            // 在 UI 线程中执行
            // 例如：更新 DataGridView
        }, null);

        await Task.CompletedTask;
    }
}
```

或者在 VxMain 中处理：

```csharp
// 在 VxMain.cs 中创建一个事件
public event EventHandler<ChatMessageData>? OnChatMessageReceived;

// 在 ChatMessageHandler 中触发事件
public class ChatMessageHandler : IMessageHandler
{
    private readonly VxMain _mainForm;

    public async Task HandleAsync(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<ChatMessageData>(data.GetRawText());
        if (message == null) return;

        // 触发事件（会自动在 UI 线程中处理）
        _mainForm.OnChatMessageReceived?.Invoke(this, message);
    }
}

// 在 VxMain 中订阅
private void VxMain_Load(object sender, EventArgs e)
{
    this.OnChatMessageReceived += (s, data) =>
    {
        // 在 UI 线程中安全更新
        lblLastMessage.Text = $"{data.Sender}: {data.Content}";
    };
}
```

### 4. 错误处理和重试

```csharp
public class ChatMessageHandler : IMessageHandler
{
    public async Task HandleAsync(JsonElement data)
    {
        int retryCount = 3;
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                var message = JsonSerializer.Deserialize<ChatMessageData>(data.GetRawText());
                // 处理逻辑...
                break; // 成功，退出循环
            }
            catch (Exception ex)
            {
                _logService.Error("ChatMessageHandler", $"Attempt {i + 1} failed", ex);
                
                if (i == retryCount - 1)
                {
                    // 最后一次尝试失败，记录到死信队列
                    await _dlqService.AddAsync("ChatMessage", data.GetRawText(), ex.Message);
                }
                else
                {
                    await Task.Delay(1000 * (i + 1)); // 指数退避
                }
            }
        }
    }
}
```

---

## 📊 与现有架构的关系

### 消息流程图

```
微信服务器
    │
    ▼
WeixinX.dll (C++)
    │ (Socket Server)
    ▼
WeixinSocketClient.cs
    │
    ├─ 有 id？
    │   ├─ Yes → _pendingRequests[id].SetResult(message)
    │   │           ↓
    │   │       SendAsync<T>() 返回结果
    │   │
    │   └─ No → OnServerPush 事件触发
    │               ↓
    │           MessageDispatcher.DispatchAsync(method, data)
    │               ↓
    │           IMessageHandler.HandleAsync(data)
    │               ↓
    │           ┌───────────┬───────────┬───────────┐
    │           ▼           ▼           ▼           ▼
    │       ChatMessage  LoginEvent  LogoutEvent  MemberEvent
    │        Handler      Handler     Handler      Handler
    │           │           │           │           │
    │           └───────────┴───────────┴───────────┘
    │                       ▼
    │                  业务逻辑处理
    │                  (保存数据库、更新UI等)
```

### 不会冲突的原因

1. **请求-响应**：
   ```json
   // 客户端发送
   {"id": 1, "method": "GetContacts", "params": []}
   
   // 服务器响应
   {"id": 1, "result": [...], "error": null}
   ```
   - ✅ 有 `id` 字段
   - ✅ 通过 `_pendingRequests` 匹配
   - ✅ 不会触发 `OnServerPush`

2. **服务器推送**：
   ```json
   // 服务器主动推送
   {"method": "OnMessage", "params": {...}}
   ```
   - ✅ 没有 `id` 字段
   - ✅ 触发 `OnServerPush` 事件
   - ✅ 通过 `MessageDispatcher` 分发

---

## 🚀 下一步

1. **创建消息模型和处理器**
   - ✅ 枚举和数据模型
   - ✅ 接口和分发器
   - ✅ 具体处理器实现

2. **集成到 DI 容器**
   - ✅ 注册服务
   - ✅ 连接 Socket 客户端

3. **实现业务逻辑**
   - 保存消息到数据库
   - 更新 UI 显示
   - 触发自动回复等

4. **测试**
   - 模拟服务器推送
   - 验证消息路由
   - 检查 UI 更新

---

## 📚 总结

### 优点

1. ✅ **解耦**：UI 层不需要关心消息类型判断
2. ✅ **可扩展**：添加新消息类型只需创建新处理器
3. ✅ **可测试**：每个处理器可以独立测试
4. ✅ **职责清晰**：每个类只负责一种消息
5. ✅ **不冲突**：请求-响应和推送完全分离

### 核心思想

- **请求-响应**：用于主动查询数据（`SendAsync`）
- **服务器推送**：用于被动接收事件（`OnServerPush` + `MessageDispatcher`）
- **消息处理器**：每种消息类型一个处理器类
- **依赖注入**：所有组件通过 DI 容器管理

---

**这就是一个优雅、可扩展的消息处理架构！** 🎉

