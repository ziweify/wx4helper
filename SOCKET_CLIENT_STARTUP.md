# 🔌 Socket 客户端启动流程说明

## 📋 **架构概述**

### **服务端（Server）**
- **项目**: `WeixinX.dll`（C++）
- **位置**: 运行在微信进程内（通过DLL注入）
- **端口**: 6328
- **启动时机**: DLL被注入时自动启动

### **客户端（Client）**
- **项目**: `BaiShengVx3Plus`（C# WinForms）
- **作用**: 与 WeixinX 通信，控制微信功能
- **启动时机**: 用户点击"采集"按钮，成功注入 WeixinX.dll 后

---

## 🚀 **启动流程详解**

### **1️⃣ 服务端启动（WeixinX）**

```cpp
// WeixinX/WeixinX/dllmain.cpp
case DLL_PROCESS_ATTACH:
{
    // ... 初始化代码 ...
    
    // 获取 Core 实例
    auto& core = WeixinX::util::Singleton<WeixinX::Core>::Get();
    
    // 🟢 启动 Socket 服务器（端口 6328）
    core.InitializeSocketServer();
    
    // 启动核心逻辑
    std::thread t(std::bind(&WeixinX::Core::Run, &core));
    t.detach();
}
```

**触发时机**: 当 `WeixinX.dll` 被注入到微信进程时（通过 `Loader.dll`）

---

### **2️⃣ 客户端启动（BaiShengVx3Plus）**

#### **Step 1: 依赖注入（Program.cs）**

```csharp
// BaiShengVx3Plus/Program.cs
services.AddSingleton<IWeixinSocketClient, WeixinSocketClient>();
```

#### **Step 2: 构造函数注入（VxMain.cs）**

```csharp
public VxMain(
    VxMainViewModel viewModel,
    Services.IContactBindingService contactBindingService,
    Services.IWeChatLoaderService loaderService,
    Services.ILogService logService,
    Services.IWeixinSocketClient socketClient) // 👈 注入 Socket 客户端
{
    _socketClient = socketClient;
    
    // 订阅服务器推送事件
    _socketClient.OnServerPush += SocketClient_OnServerPush;
}
```

#### **Step 3: 用户操作触发连接**

用户点击 **"采集"** 按钮 (`btnGetContactList`) 后：

```csharp
private async void btnGetContactList_Click(object sender, EventArgs e)
{
    // 1. 注入 WeixinX.dll 到微信进程
    if (_loaderService.InjectToProcess(processes[0], dllPath, out string error))
    {
        // 2. 等待服务器启动（1秒）
        await Task.Delay(1000);
        
        // 3. 🔵 连接到 Socket 服务器
        await ConnectToSocketServerAsync();
    }
}
```

#### **Step 4: 建立连接**

```csharp
private async Task ConnectToSocketServerAsync()
{
    // 连接到 127.0.0.1:6328
    bool connected = await _socketClient.ConnectAsync("127.0.0.1", 6328, 5000);
    
    if (connected)
    {
        // 连接成功！
        // 测试：获取用户信息
        await TestGetUserInfoAsync();
    }
}
```

---

## 📡 **通信流程**

### **客户端 → 服务端（请求-响应）**

```csharp
// 客户端发送请求
var result = await _socketClient.SendAsync<UserInfo>("GetUserInfo");
```

**消息格式**:
```json
{
  "id": 1,
  "method": "GetUserInfo",
  "params": []
}
```

### **服务端 → 客户端（主动推送）**

```cpp
// 服务端主动推送消息
m_socketServer->PushToAllClients("MessageReceived", messageData);
```

**处理推送**:
```csharp
private void SocketClient_OnServerPush(object? sender, ServerPushEventArgs e)
{
    switch (e.Method)
    {
        case "MessageReceived":
            // 处理新消息
            break;
        case "ContactListUpdated":
            // 处理联系人列表更新
            break;
    }
}
```

---

## 🔄 **完整流程时序图**

```
用户操作          BaiShengVx3Plus              Loader.dll         WeixinX.dll (微信进程)
   |                    |                          |                      |
   |--点击"采集"-------->|                          |                      |
   |                    |                          |                      |
   |                    |--InjectToProcess()------>|                      |
   |                    |                          |                      |
   |                    |                          |--DLL_PROCESS_ATTACH->|
   |                    |                          |                      |
   |                    |                          |<--注入成功-----------|
   |                    |<--注入成功---------------|                      |
   |                    |                          |                      |
   |                    |--Delay(1000ms)---------->|                      |
   |                    |                          |                      |
   |                    |--ConnectAsync()----------|-------------------->|
   |                    |                          |                      |
   |                    |<--连接成功----------------------------------------|
   |                    |                          |                      |
   |                    |--SendAsync("GetUserInfo")-------------------->|
   |                    |                          |                      |
   |                    |<--{result}----------------------------------------|
   |                    |                          |                      |
   |<--弹出连接成功提示--|                          |                      |
```

---

## ⚙️ **关键配置**

| 配置项 | 值 | 说明 |
|--------|-----|------|
| **服务端端口** | `6328` | WeixinX 监听端口 |
| **服务端地址** | `127.0.0.1` | 本地回环地址 |
| **连接超时** | `5000ms` | 5秒超时 |
| **延迟启动** | `1000ms` | 注入后等待1秒 |
| **协议格式** | `[4字节长度][JSON消息体]` | 自定义协议 |

---

## 🛠️ **调试方法**

### **1. 检查服务端是否启动**

在 WeixinX 的日志中查找：
```
Initializing Socket Server...
Socket Server started successfully on port 6328
```

### **2. 检查客户端连接**

在 BaiShengVx3Plus 的日志窗口查找：
```
正在连接到 Socket 服务器...
Socket 连接成功
```

### **3. 测试通信**

成功连接后会自动测试 `GetUserInfo`，检查日志：
```
测试获取用户信息...
用户信息: { ... }
```

---

## 🚨 **常见问题**

### **Q: 连接失败怎么办？**

**A**: 检查以下几点：
1. WeixinX.dll 是否成功注入到微信进程？
2. 微信进程是否正在运行？
3. Socket 服务器是否已启动（查看 WeixinX 日志）？
4. 防火墙是否阻止了端口 6328？

### **Q: 为什么要延迟 1 秒？**

**A**: 给 WeixinX.dll 足够的时间完成以下操作：
- 初始化 WeixinX::Core
- 启动 Socket 服务器
- 监听端口 6328

### **Q: 服务器在哪里启动？**

**A**: 
- **不是** BaiShengVx3Plus 启动服务器
- **是** WeixinX.dll（注入到微信进程后）启动服务器
- BaiShengVx3Plus 只是作为客户端连接到 WeixinX

---

## ✅ **总结**

| 角色 | 项目 | 启动时机 |
|------|------|----------|
| **服务端** | WeixinX.dll | DLL注入时自动启动 |
| **客户端** | BaiShengVx3Plus | 用户点击"采集"后手动连接 |

**关键点**:
1. ✅ 服务端在 `dllmain.cpp` 的 `DLL_PROCESS_ATTACH` 中启动
2. ✅ 客户端在 `VxMain.cs` 的 `btnGetContactList_Click` 中连接
3. ✅ 使用依赖注入管理 `IWeixinSocketClient`
4. ✅ 支持请求-响应和服务器主动推送两种模式
5. ✅ 窗口关闭时自动断开连接

---

## 📚 **相关文件**

### **服务端**
- `WeixinX/WeixinX/dllmain.cpp` - DLL入口点，启动Socket服务器
- `WeixinX/WeixinX/Features.cpp` - InitializeSocketServer() 实现
- `WeixinX/WeixinX/SocketServer.cpp` - Socket服务器实现
- `WeixinX/WeixinX/SocketCommands.cpp` - 命令处理器

### **客户端**
- `BaiShengVx3Plus/Program.cs` - 依赖注入配置
- `BaiShengVx3Plus/Views/VxMain.cs` - Socket连接逻辑
- `BaiShengVx3Plus/Services/WeixinSocketClient.cs` - Socket客户端实现
- `BaiShengVx3Plus/Services/IWeixinSocketClient.cs` - 接口定义

---

**文档创建时间**: 2025-11-04
**最后更新**: 2025-11-04

