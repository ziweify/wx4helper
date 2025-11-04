# 🚀 Socket 通信快速开始

## ✅ 已完成的工作

### 服务端 (WeixinX - C++)
- ✅ `SocketServer.h` / `SocketServer.cpp` - Socket 服务器核心
- ✅ `SocketCommands.h` / `SocketCommands.cpp` - 命令处理器
- ✅ 集成到 `Features.h` 和 `Features.cpp`
- ✅ 在 `dllmain.cpp` 中自动启动
- ✅ 添加到 `WeixinX.vcxproj` 项目文件

### 客户端 (BaiShengVx3Plus - C#)
- ✅ `IWeixinSocketClient.cs` - 客户端接口
- ✅ `WeixinSocketClient.cs` - 客户端实现
- ✅ 注册到 DI 容器 (`Program.cs`)

### 文档
- ✅ `SOCKET_COMMUNICATION_GUIDE.md` - 完整使用指南

---

## 🔧 测试步骤

### 1. 编译 WeixinX
```bash
cd WeixinX
.\build_weixinx.bat
```

**输出**: `WeixinX\x64\Release\WeixinX.dll`

### 2. 注入到微信
使用 `BaiShengVx3Plus` 中的注入功能，或者直接运行：
```bash
# 假设已启动微信
Loader.dll -> InjectDllToProcess(wechatPID, "WeixinX.dll")
```

**预期日志**:
```
Initializing Socket Server...
Socket Server started successfully on port 6328
All socket commands registered
```

### 3. 运行 BaiShengVx3Plus
```bash
cd BaiShengVx3Plus\bin\Debug\net8.0-windows
.\BaiShengVx3Plus.exe
```

### 4. 在 VxMain 中测试连接

#### 方法1: 在 VxMain_Load 中自动连接
```csharp
private async void VxMain_Load(object sender, EventArgs e)
{
    // 延迟一下，确保 Socket 服务器已启动
    await Task.Delay(1000);
    
    bool connected = await _socketClient.ConnectAsync();
    if (connected)
    {
        _logService.Info("VxMain", "Socket 连接成功");
        UIMessageBox.ShowSuccess("已连接到微信服务");
    }
    else
    {
        _logService.Error("VxMain", "Socket 连接失败");
        UIMessageBox.ShowError("无法连接到微信服务");
    }
}
```

#### 方法2: 添加测试按钮
```csharp
private async void btnTestSocket_Click(object sender, EventArgs e)
{
    try
    {
        // 1. 连接
        if (!_socketClient.IsConnected)
        {
            bool connected = await _socketClient.ConnectAsync();
            if (!connected)
            {
                UIMessageBox.ShowError("连接失败");
                return;
            }
        }
        
        // 2. 测试 GetUserInfo
        var userInfo = await _socketClient.SendAsync<UserInfo>("GetUserInfo");
        if (userInfo != null)
        {
            UIMessageBox.ShowSuccess($"连接成功！\n当前用户: {userInfo.Nickname}\nWXID: {userInfo.Wxid}");
        }
        
        // 3. 测试 GetContacts
        var contacts = await _socketClient.SendAsync<List<Contact>>("GetContacts");
        if (contacts != null)
        {
            UIMessageBox.ShowInfo($"获取到 {contacts.Count} 个联系人");
        }
    }
    catch (Exception ex)
    {
        _logService.Error("VxMain", "Socket 测试失败", ex);
        UIMessageBox.ShowError($"测试失败: {ex.Message}");
    }
}
```

---

## 📊 验证方法

### 1. 检查服务端日志
在微信进程的控制台输出中查看：
```
Initializing Socket Server...
Socket Server started successfully on port 6328
Registered handler for method: GetContacts
Registered handler for method: GetGroupContacts
Registered handler for method: SendMessage
Registered handler for method: GetUserInfo
All socket commands registered
```

### 2. 检查客户端日志
在 `BaiShengVx3Plus` 的日志窗口中查看：
```
[INFO] WeixinSocketClient: Connecting to 127.0.0.1:6328...
[INFO] WeixinSocketClient: Connected successfully
[INFO] WeixinSocketClient: Receive loop started
```

### 3. 测试命令
```csharp
// 获取用户信息
var userInfo = await _socketClient.SendAsync<UserInfo>("GetUserInfo");
// 应该返回当前登录的微信用户信息

// 获取联系人
var contacts = await _socketClient.SendAsync<List<Contact>>("GetContacts");
// 应该返回联系人列表（目前是示例数据）
```

---

## 🔥 常见问题

### Q1: 连接失败
**原因**:
- 微信未注入 WeixinX.dll
- Socket 服务器未启动
- 端口被占用

**解决**:
```bash
# 检查端口是否被占用
netstat -ano | findstr 6328

# 确认微信已注入 DLL
# 查看微信进程的控制台输出
```

### Q2: 请求超时
**原因**:
- 命令处理器异常
- 网络延迟

**解决**:
```csharp
// 增加超时时间
var result = await _socketClient.SendAsync<Result>("SlowMethod", 30000);
```

### Q3: 中文乱码
**已解决**: 使用 UTF-8 编码 + `emitUTF8 = true`

### Q4: 粘包问题
**已解决**: 使用 4字节长度头 + 消息体格式

---

## 📝 下一步

### 实现真实的命令处理

#### 1. GetContacts - 获取真实联系人
```cpp
// SocketCommands.cpp
Json::Value SocketCommands::HandleGetContacts(const Json::Value& params)
{
    Json::Value result(Json::arrayValue);
    
    // TODO: 调用微信API获取联系人列表
    // 可以参考 Features.cpp 中的数据库查询逻辑
    
    // 示例：从微信数据库查询
    // auto& dbHandles = Features::DBHandles;
    // if (dbHandles.find("MicroMsg.db") != dbHandles.end()) {
    //     // 执行SQL查询
    // }
    
    return result;
}
```

#### 2. SendMessage - 发送消息
```cpp
Json::Value SocketCommands::HandleSendMessage(const Json::Value& params)
{
    std::string wxid = params[0].asString();
    std::string message = params[1].asString();
    
    // TODO: 调用微信发送消息API
    // 可以参考 Features.cpp 中的 SendText 方法
    
    Json::Value result;
    result["success"] = true;
    result["messageId"] = "msg_123";
    return result;
}
```

#### 3. 添加服务器推送
```cpp
// 在 Features.cpp 的消息接收处理中
void WeixinX::MsgReceived::Received(weixin_dll::v41021::weixin_struct::MsgReceived* msg)
{
    // ... 现有逻辑 ...
    
    // 推送消息到客户端
    auto& core = WeixinX::util::Singleton<WeixinX::Core>::Get();
    auto* server = core.GetSocketServer();
    
    if (server && server->IsRunning()) {
        Json::Value data;
        data["wxid"] = msgReceived.sender;
        data["content"] = msgReceived.content;
        data["timestamp"] = msgReceived.ts;
        
        server->Broadcast("OnMessage", data);
    }
}
```

---

## ✨ 功能扩展示例

### 添加新命令

#### 服务端 (C++)
```cpp
// SocketCommands.cpp
Json::Value HandleGetGroupList(const Json::Value& params)
{
    Json::Value result(Json::arrayValue);
    
    // 实现获取群列表逻辑
    
    return result;
}

// 注册
void SocketCommands::RegisterAll(SocketServer* server)
{
    // ... 现有注册 ...
    server->RegisterHandler("GetGroupList", HandleGetGroupList);
}
```

#### 客户端 (C#)
```csharp
// 调用
var groups = await _socketClient.SendAsync<List<Group>>("GetGroupList");
```

---

## 📦 完整项目结构

```
WeixinX/
├── SocketServer.h          ✅ 服务器核心
├── SocketServer.cpp        ✅
├── SocketCommands.h        ✅ 命令处理器
├── SocketCommands.cpp      ✅
├── Features.h              ✅ (已集成)
├── Features.cpp            ✅ (已集成)
└── dllmain.cpp             ✅ (已集成)

BaiShengVx3Plus/
├── Services/
│   ├── IWeixinSocketClient.cs      ✅ 客户端接口
│   └── WeixinSocketClient.cs       ✅ 客户端实现
└── Program.cs                      ✅ (已注册)

Docs/
├── SOCKET_COMMUNICATION_GUIDE.md   ✅ 完整指南
└── SOCKET_QUICK_START.md           ✅ 快速开始
```

---

**🎉 Socket 通信框架已完成！开始测试吧！**

