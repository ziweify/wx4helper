# 🌐 BsBrowserClient - 百盛浏览器客户端

## 📝 项目说明

这是一个**独立的 CEF 浏览器工程**，用于自动化投注操作。

### 🎯 核心功能

1. **嵌入式浏览器**: 使用 CefSharp 提供完整浏览器功能
2. **Socket 通信**: 通过 TCP Socket 与主程序 `BaiShengVx3Plus` 通信
3. **多平台脚本**: 支持云顶28、海峡28等多个投注平台
4. **独立进程**: 每个配置可启动独立的浏览器进程

---

## 🏗️ 架构设计

```
BaiShengVx3Plus (主程序)
    │
    │ Socket 通信
    ↓
BsBrowserClient (独立工程)
    ├─ Form1.cs (主窗体 - Designer 设计)
    │   ├─ ChromiumWebBrowser (CEF 浏览器控件)
    │   ├─ 状态栏
    │   └─ 日志面板
    │
    ├─ SocketServer.cs (Socket 服务器)
    │   ├─ 监听连接
    │   ├─ 接收命令
    │   └─ 返回结果
    │
    └─ PlatformScripts/ (平台脚本)
        ├─ YunDing28Script.cs (云顶28)
        ├─ HaiXia28Script.cs (海峡28)
        └─ ...
```

---

## 📡 Socket 通信协议

### 命令格式 (JSON)

```json
{
  "Command": "Login|PlaceBet|GetBalance|Navigate",
  "Data": {
    "Username": "test001",
    "Password": "aaa111",
    "Platform": "YunDing28"
  }
}
```

### 返回格式 (JSON)

```json
{
  "Success": true,
  "Data": {
    "Balance": 1000.50,
    "OrderId": "202311070001"
  },
  "ErrorMessage": null
}
```

---

## 🚀 启动方式

### 方式1: 从主程序启动（推荐）
```csharp
// BaiShengVx3Plus 中启动
var process = Process.Start("BsBrowserClient.exe", $"--config-id {configId} --port {port}");
```

### 方式2: 独立启动（调试用）
```bash
BsBrowserClient.exe --config-id 1 --port 9527
```

---

## 📦 依赖包

- **CefSharp.WinForms**: 浏览器控件
- **Newtonsoft.Json**: JSON 序列化
- **System.Net.Sockets**: Socket 通信

---

## 🎨 界面设计

### 主窗体 (Form1)
- **顶部**: 地址栏、刷新按钮、配置信息
- **中间**: CEF 浏览器控件（占主要区域）
- **底部**: 状态栏（显示连接状态、余额等）
- **右侧**: 日志面板（可折叠）

---

## 🔧 开发步骤

### 1. 添加 NuGet 包
```xml
<PackageReference Include="CefSharp.WinForms" Version="119.4.30" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

### 2. 设计主窗体
- 使用 Visual Studio Designer 设计界面
- 添加 ChromiumWebBrowser 控件
- 添加状态栏、日志面板

### 3. 实现 Socket 服务器
- 监听指定端口
- 接收 JSON 命令
- 调用对应方法
- 返回 JSON 结果

### 4. 实现平台脚本
- 每个平台一个类
- 实现登录、投注、获取余额等方法
- 使用 JavaScript 注入实现自动化

---

## 📋 TODO

- [ ] 添加 CefSharp 包引用
- [ ] 设计主窗体界面
- [ ] 实现 Socket 服务器
- [ ] 实现云顶28脚本
- [ ] 实现海峡28脚本
- [ ] 添加日志功能
- [ ] 添加配置管理
- [ ] 测试通信协议

---

## 🤝 与主程序集成

主程序 `BaiShengVx3Plus` 通过以下方式使用：

```csharp
// 1. 启动浏览器进程
var browser = new BrowserClient(configId);
await browser.Start();

// 2. 发送命令
var result = await browser.SendCommand(new {
    Command = "Login",
    Data = new { Username = "test", Password = "123" }
});

// 3. 获取结果
if (result.Success)
{
    Console.WriteLine($"余额: {result.Data.Balance}");
}

// 4. 关闭浏览器
browser.Stop();
```

---

**设计原则**: 简单、独立、可扩展！

