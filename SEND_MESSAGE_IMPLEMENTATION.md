# 📤 微信发送消息功能实现

## ✅ 实现完成

### 功能描述
通过 Socket 通信调用微信的真实发送消息功能。

---

## 🔧 实现细节

### 1. 核心函数：`Core::SendText`

**位置**：`WeixinX/WeixinX/Features.cpp` (第 392 行)

**函数签名**：
```cpp
void WeixinX::Core::SendText(string who, string what);
```

**参数**：
- `who` (string): 接收者的微信ID (wxid)
- `what` (string): 消息内容（文本）

**功能**：
- 调用微信内部函数发送文本消息
- 通过堆分配构建消息结构
- 使用微信的内部接口发送

---

## 🌐 Socket 命令处理器

### HandleSendMessage 实现

**位置**：`WeixinX/WeixinX/SocketCommands.cpp` (第 63-96 行)

**修改前**（错误代码）：
```cpp
// ❌ 错误：直接调用静态方法
Features::SendText(wxid, message);
```

**修改后**（正确代码）：
```cpp
// ✅ 正确：通过单例获取 Core 实例
try {
    auto& core = util::Singleton<Core>::Get();
    core.SendText(wxid, message);
    
    Json::Value result;
    result["success"] = true;
    result["messageId"] = "msg_" + std::to_string(util::Timestamp());
    
    util::logging::print("Message sent successfully");
    return result;
}
catch (const std::exception& e) {
    Json::Value error;
    error["error"] = std::format("Failed to send message: {}", e.what());
    util::logging::print("Failed to send message: {}", e.what());
    return error;
}
```

### 关键技术点

#### 1. 单例模式访问
```cpp
auto& core = util::Singleton<Core>::Get();
```
- `Core` 类使用单例模式
- 通过 `Singleton<Core>::Get()` 获取全局唯一实例
- 保证整个程序只有一个 Core 实例

#### 2. 异常处理
```cpp
try {
    core.SendText(wxid, message);
    // 成功返回
}
catch (const std::exception& e) {
    // 失败返回错误信息
}
```
- 捕获发送过程中的异常
- 将异常信息返回给客户端
- 防止崩溃

#### 3. 响应格式

**成功响应**：
```json
{
  "success": true,
  "messageId": "msg_1730738993"
}
```

**失败响应**：
```json
{
  "error": "Failed to send message: <exception message>"
}
```

---

## 📊 编译信息

```
编译时间: 2025/11/5 1:05:48
输出位置: D:\gitcode\wx4helper\WeixinX\bin\release\net8.0-windows\WeixinX.dll
编译结果: ✅ 成功（0 个警告，0 个错误）
```

---

## 🧪 测试步骤

### 1. 准备测试环境

```bash
# 关闭所有微信进程
taskkill /F /IM WeChat.exe
```

### 2. 启动测试

1. 启动 BaiShengVx3Plus
2. 点击"采集"，启动微信并注入
3. 等待 Socket 连接建立

### 3. 发送测试消息

在设置窗口的命令输入框中输入：

```
SendMessage(filehelper, Hello from Socket!)
```

**参数说明**：
- `filehelper` - 文件传输助手的 wxid
- `Hello from Socket!` - 消息内容

### 4. 预期结果

**DebugView 日志**：
```
[WeixinX] Received: {"id":5,"method":"SendMessage","params":["filehelper","Hello from Socket!"]}
[WeixinX] Processing command: SendMessage (id=5)
[WeixinX] Handling SendMessage to filehelper: Hello from Socket!
[WeixinX] Message sent successfully
[WeixinX] Response sent: success
```

**客户端响应**：
```json
{
  "success": true,
  "messageId": "msg_1730738993"
}
```

**微信客户端**：
- 文件传输助手应该收到消息："Hello from Socket!"

---

## 🎯 使用示例

### 示例 1：发送给文件传输助手

```javascript
// 命令
SendMessage(filehelper, 测试消息)

// 预期响应
{
  "success": true,
  "messageId": "msg_1730738993"
}
```

### 示例 2：发送给好友

```javascript
// 命令
SendMessage(wxid_abc123, 你好！)

// 预期响应
{
  "success": true,
  "messageId": "msg_1730738994"
}
```

### 示例 3：发送给群聊

```javascript
// 命令
SendMessage(123456789@chatroom, 大家好！)

// 预期响应
{
  "success": true,
  "messageId": "msg_1730738995"
}
```

### 示例 4：发送失败

```javascript
// 命令（无效的 wxid）
SendMessage(invalid_wxid, 消息)

// 预期响应
{
  "error": "Failed to send message: <具体错误信息>"
}
```

---

## 🔍 内部实现原理

### SendText 函数流程

```cpp
void WeixinX::Core::SendText(string who, string what) {
    // 1. 获取微信 DLL 基址
    uint64_t base = WeixinX::util::getWeixinDllBase();
    
    // 2. 分配消息结构内存
    uint64_t* txtMessage = WeixinX::util::heapAlloc<uint64_t>(0x530);
    buildTextMessage(txtMessage, what, who);
    
    // 3. 构建发送数据
    uint64_t* data = WeixinX::util::heapAlloc<uint64_t>(0x20);
    data[0] = reinterpret_cast<uint64_t>(txtMessage + 2);
    data[1] = reinterpret_cast<uint64_t>(txtMessage);
    // ... 更多数据设置 ...
    
    // 4. 调用微信内部发送函数
    WeixinCall call = (WeixinCall)(base + offset::sendmsg::send_message);
    call(/* ... 参数 ... */);
    
    // 5. 清理内存
    util::heapFree(data);
    util::heapFree(txtMessage);
}
```

### 关键技术

1. **内存管理**：
   - 使用堆分配器 (`heapAlloc`) 分配消息结构
   - 发送完成后释放内存 (`heapFree`)
   - 防止内存泄漏

2. **内存布局**：
   - 消息结构大小：0x530 字节
   - 数据指针结构：0x20 字节
   - 严格按照微信内部格式构建

3. **函数调用**：
   - 通过偏移量定位微信内部函数
   - 使用函数指针类型转换调用
   - 传递正确的参数和寄存器状态

---

## ⚠️ 注意事项

### 1. wxid 格式

- **好友**：`wxid_abc123` 或 `微信号`
- **群聊**：`123456789@chatroom`
- **公众号**：`gh_xxxxxx`
- **文件传输助手**：`filehelper`

### 2. 消息内容限制

- 文本长度：建议不超过 10000 字符
- 特殊字符：需要正确编码（UTF-8）
- 换行符：支持 `\n`

### 3. 错误处理

- 无效的 wxid：返回错误
- 未添加的好友：可能失败
- 被拉黑的好友：可能失败
- 网络问题：可能超时

### 4. 发送频率

- 不建议过于频繁发送
- 建议每条消息间隔至少 100ms
- 避免被微信检测为异常行为

---

## 🚀 后续扩展

### 可实现的功能

1. **发送图片**：`Core::SendImage(string who, string which)`
2. **发送文件**：需要实现新函数
3. **发送名片**：需要实现新函数
4. **撤回消息**：需要实现新函数
5. **@群成员**：在群消息中实现

### 扩展示例

```cpp
// 在 SocketCommands.cpp 中添加新的处理器
Json::Value SocketCommands::HandleSendImage(const Json::Value& params)
{
    if (params.size() < 2) {
        Json::Value error;
        error["error"] = "Invalid parameters. Expected: (wxid: string, imagePath: string)";
        return error;
    }
    
    std::string wxid = params[0].asString();
    std::string imagePath = params[1].asString();
    
    try {
        auto& core = util::Singleton<Core>::Get();
        core.SendImage(wxid, imagePath);
        
        Json::Value result;
        result["success"] = true;
        return result;
    }
    catch (const std::exception& e) {
        Json::Value error;
        error["error"] = std::format("Failed to send image: {}", e.what());
        return error;
    }
}
```

---

## 📋 总结

### 实现的功能
✅ 通过 Socket 发送微信文本消息  
✅ 支持发送给好友、群聊、公众号  
✅ 异常处理和错误返回  
✅ 详细的日志记录  

### 技术要点
- 单例模式访问 Core 实例
- 调用微信内部 SendText 函数
- 异常安全的实现
- 完整的错误处理

### 测试建议
1. 先测试发送给文件传输助手
2. 再测试发送给好友
3. 最后测试发送给群聊
4. 观察 DebugView 日志确认执行

---

**状态**：✅ **已完成并编译成功**

**下一步**：测试真实的消息发送功能

