# 🐛 Socket 服务器调试指南

## 问题诊断

根据日志，发现了以下问题：

### 客户端日志
```
2025-11-04 23:27:46.026  调试  WeixinSocketClient  Sending: {"id":3,"method":"GetContacts","params":[]}
2025-11-04 23:27:56.038  错误  WeixinSocketClient  Request timeout: GetContacts
2025-11-04 23:27:56.052  警告  SettingsForm  收到空响应
```

**问题**：请求已发送，但 10 秒后超时，没有收到响应。

---

## 解决方案

我已经在服务器端添加了详细的调试日志，现在需要查看 C++ 端的日志输出。

### ✅ 步骤 1：下载 DebugView 工具

`WeixinX.dll` 的日志输出使用 `OutputDebugString`，需要使用 DebugView 工具查看。

**下载地址**：
- [Sysinternals DebugView](https://learn.microsoft.com/en-us/sysinternals/downloads/debugview)
- 或者搜索：`Sysinternals DebugView`

### ✅ 步骤 2：启动 DebugView

1. 解压并运行 `Dbgview.exe`（以管理员权限运行）
2. 点击菜单：`Capture` -> `Capture Global Win32`
3. 点击菜单：`Capture` -> `Capture Events`
4. 点击菜单：`Edit` -> `Clear Display` 清空现有日志

### ✅ 步骤 3：重新测试

1. **关闭所有微信进程**
2. 启动 `BaiShengVx3Plus`
3. 点击 **"采集"** 按钮重新注入新编译的 DLL
4. 观察 DebugView 窗口，应该看到：
   ```
   [WeixinX] Initializing Socket Server...
   [WeixinX] Registered handler for method: GetContacts
   [WeixinX] Registered handler for method: GetGroupContacts
   [WeixinX] Registered handler for method: SendMessage
   [WeixinX] Registered handler for method: GetUserInfo
   [WeixinX] All socket commands registered
   [WeixinX] Socket Server started successfully on port 6328
   ```

5. 打开 **"设置"** 窗口，发送命令 `GetContacts()`
6. 观察 DebugView 中的输出

---

## 新增的调试日志

我已经添加了以下调试日志：

### 1. 命令注册
```cpp
[WeixinX] Registered handler for method: GetContacts
[WeixinX] Registered handler for method: GetGroupContacts
[WeixinX] Registered handler for method: SendMessage
[WeixinX] Registered handler for method: GetUserInfo
[WeixinX] All socket commands registered
```

### 2. 接收请求
```cpp
[WeixinX] Received: {"id":3,"method":"GetContacts","params":[]}
[WeixinX] Processing command: GetContacts (id=3)
```

### 3. 处理命令
```cpp
[WeixinX] HandleCommand called for method: GetContacts
[WeixinX] Registered handlers count: 4
[WeixinX] Found handler for method: GetContacts
[WeixinX] Handling GetContacts
[WeixinX] Handler executed successfully for: GetContacts
[WeixinX] Command GetContacts executed, preparing response
```

### 4. 发送响应
```cpp
[WeixinX] Sending response: {"id":3,"result":[{"wxid":"wxid_example123","nickname":"示例联系人","remark":"备注名","avatar":"http://example.com/avatar.jpg"}],"error":null}
[WeixinX] Response sent: success
```

### 5. 错误信息（如果有）
```cpp
[WeixinX] Unknown method: SomeInvalidMethod
[WeixinX] Available methods:
[WeixinX]   - GetContacts
[WeixinX]   - GetGroupContacts
[WeixinX]   - SendMessage
[WeixinX]   - GetUserInfo
```

---

## 可能的问题和解决方案

### 问题 1：DebugView 中没有任何输出

**可能原因**：
1. DLL 没有正确注入
2. DebugView 没有以管理员权限运行
3. `Capture Global Win32` 没有启用

**解决方案**：
1. 确认微信进程存在：`tasklist | findstr WeChat.exe`
2. 以管理员权限运行 DebugView
3. 确认 `Capture` 菜单中的 `Capture Global Win32` 和 `Capture Events` 都已勾选

### 问题 2：看到 "Initializing Socket Server..." 但没有 "Socket Server started successfully"

**可能原因**：
1. 端口 6328 被占用
2. WinSock 初始化失败

**解决方案**：
1. 检查端口占用：`netstat -ano | findstr 6328`
2. 如果被占用，结束占用端口的进程或修改端口号

### 问题 3：看到 "Received: ..." 但没有后续的 "Processing command"

**可能原因**：
1. JSON 解析失败
2. 线程异常

**解决方案**：
1. 检查 DebugView 中是否有 "Failed to parse JSON" 错误
2. 检查是否有 "Exception in ProcessMessage" 错误

### 问题 4：看到 "Processing command" 但没有 "HandleCommand called"

**可能原因**：
1. 在调用 `HandleCommand` 之前发生了异常

**解决方案**：
1. 检查 DebugView 中的完整日志
2. 查看是否有异常信息

### 问题 5：看到 "Unknown method"

**可能原因**：
1. 命令名称拼写错误（大小写敏感！）
2. 命令处理器没有正确注册

**解决方案**：
1. 确认命令名称完全匹配（`GetContacts` 不等于 `getContacts`）
2. 检查 DebugView 中的 "Available methods" 列表

### 问题 6：看到 "Response sent: success" 但客户端仍然超时

**可能原因**：
1. 客户端接收缓冲区问题
2. 消息格式不匹配

**解决方案**：
1. 在 C# 客户端的 `ReceiveLoop` 方法中添加更多日志
2. 检查接收到的字节数

---

## 完整测试流程（带 DebugView）

### 准备阶段
1. ✅ 启动 DebugView（管理员权限）
2. ✅ 启用 `Capture Global Win32`
3. ✅ 清空显示缓冲区

### 测试阶段
1. ✅ 关闭所有微信进程
2. ✅ 启动 `BaiShengVx3Plus`
3. ✅ 点击"采集"，观察 DebugView
   - 应该看到：`Initializing Socket Server...`
   - 应该看到：`Socket Server started successfully on port 6328`
4. ✅ 打开设置窗口
5. ✅ 发送命令：`GetContacts()`
6. ✅ 同时观察：
   - DebugView 窗口（C++ 端日志）
   - BaiShengVx3Plus 日志窗口（C# 端日志）

### 分析阶段
根据 DebugView 的输出，定位问题：

| DebugView 输出 | 说明 | 下一步 |
|---------------|------|--------|
| 无任何输出 | DLL 未注入或 DebugView 配置错误 | 检查注入状态和 DebugView 设置 |
| 看到 "Initializing..." 但无 "started successfully" | Socket 服务器启动失败 | 检查端口占用 |
| 看到 "started successfully" 但无 "Received:" | 客户端连接失败 | 检查客户端连接代码 |
| 看到 "Received:" 但无 "Processing" | JSON 解析失败 | 检查消息格式 |
| 看到 "Processing" 但无 "HandleCommand" | 异常发生 | 查看异常信息 |
| 看到 "Unknown method" | 命令未注册或名称错误 | 检查命令名称 |
| 看到 "Response sent: success" 但客户端超时 | 接收端问题 | 检查客户端接收逻辑 |

---

## 下一步

### 如果 DebugView 中看到完整的日志流程

说明服务器端工作正常，问题在客户端接收。需要：
1. 在 C# 客户端的 `ReceiveLoop` 中添加更多日志
2. 检查客户端的响应等待逻辑

### 如果 DebugView 中日志中断

根据最后一条日志，定位问题：
1. 如果在 "Processing command" 后中断，检查 `HandleCommand` 的调用
2. 如果在 "Handler executed" 后中断，检查响应构建逻辑
3. 如果在 "Sending response" 后中断，检查 `Send` 方法

---

## 编译时间

**最新编译时间**：2025/11/4 23:41:55  
**输出位置**：`D:\gitcode\wx4helper\WeixinX\bin\release\net8.0-windows\WeixinX.dll`

---

## 总结

现在我们有了非常详细的调试日志，可以精确定位问题所在。

**请按照上述步骤重新测试，并告诉我 DebugView 中的输出内容！** 🔍

这样我们就可以快速找到问题的根源并修复它。

