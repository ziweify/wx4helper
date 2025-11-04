# 📋 WeixinX JSON 日志功能

## ✨ 新功能说明

在 `WeixinX/Features.cpp` 的 `Received` 函数中添加了 **JSON 格式日志输出**功能。

### 📍 修改位置

- **文件**: `WeixinX/WeixinX/Features.cpp`
- **函数**: `WeixinX::MsgReceived::Received`
- **行号**: 第 553-566 行

### 🎯 功能描述

每当接收到微信消息时，会将 `msgReceived` 对象转换为 **格式化的 JSON** 并打印到日志中。

### 📝 JSON 输出格式

```json
{
  "receiver1": "xxx@chatroom",
  "receiver2": "wxid_xxx",
  "sender": "wxid_xxx",
  "ts": 1234567890,
  "fromChatroom": true,
  "content": "消息内容"
}
```

### 🔧 实现代码

```cpp
std::string rawContent = util::trim(msg->content.str().substr(pos + 1).c_str());

MsgReceived msgReceived;
msgReceived.receiver1 = msg->receiver1.str();
msgReceived.receiver2 = msg->receiver2.str();
msgReceived.sender = msg->sender.str();
msgReceived.ts = msg->ts;
msgReceived.fromChatroom = msg->receiver1.str().find("@chatroom") != std::string::npos;
msgReceived.content = rawContent;

// 将 msgReceived 转换为 JSON 并打印（不转义中文字符）
Json::Value j;
j["receiver1"] = msgReceived.receiver1;
j["receiver2"] = msgReceived.receiver2;
j["sender"] = msgReceived.sender;
j["ts"] = (Json::Int64)msgReceived.ts;
j["fromChatroom"] = msgReceived.fromChatroom;
j["content"] = msgReceived.content;

Json::StreamWriterBuilder builder;
builder["indentation"] = "  ";
builder["emitUTF8"] = true;  // 启用 UTF-8 输出，不转义中文
const std::string jsonString = Json::writeString(builder, j);

util::logging::wPrint(L"MsgReceived JSON:\n{}", util::utf8ToUtf16(jsonString.c_str()));
```

### 📊 字段说明

| 字段 | 类型 | 说明 |
|------|------|------|
| `receiver1` | string | 接收者1 (如果是群聊，包含 "@chatroom") |
| `receiver2` | string | 接收者2 |
| `sender` | string | 发送者微信ID |
| `ts` | int64 | 消息时间戳 |
| `fromChatroom` | boolean | 是否来自群聊 |
| `content` | string | 消息内容（已处理过的） |

### 🚀 编译方法

#### 方法1: 使用批处理脚本
```bash
cd WeixinX
.\build_weixinx.bat
```

#### 方法2: 使用 Visual Studio
1. 打开 `WeixinX.sln`
2. 选择 `Release` | `x64` 配置
3. 右键点击 `WeixinX` 项目 → 生成

### 📂 输出位置

- **DLL 文件**: `WeixinX\x64\Release\WeixinX.dll`
- **自动复制到**: `BaiShengVx3Plus\bin\Release\net8.0-windows\WeixinX.dll`

### ✅ 编译结果

- ✅ 编译成功
- ⚠️ 5 个警告（size_t 转换警告，不影响功能）
- ✅ 0 个错误
- ✅ DLL 已生成

### 🔍 日志输出示例

当接收到微信消息时，会在控制台/日志文件中看到：

```
MsgReceived JSON:
{
  "content": "/投注 大 100",
  "fromChatroom": true,
  "receiver1": "12345678@chatroom",
  "receiver2": "wxid_abc123",
  "sender": "wxid_xyz789",
  "ts": 1699123456789
}
```

### 🎨 特性

- ✅ **格式化输出** - 使用 2 空格缩进美化 JSON
- ✅ **UTF-8 支持** - `emitUTF8 = true` 确保中文不会被转义成 `\uxxxx` 格式
- ✅ **完整信息** - 包含所有关键字段
- ✅ **易于解析** - 标准 JSON 格式
- ✅ **无冗余代码** - 复用 rawContent，避免重复打印

### ⚡ 优化说明

**问题**: 原始实现中，中文字符会被转义为 `\u1111` 格式，不便于阅读。

**解决方案**:
1. **移除冗余日志** - 删除了单独打印 `rawContent` 的代码
2. **启用 UTF-8 输出** - 配置 `builder["emitUTF8"] = true`，确保中文正常显示
3. **复用变量** - 所有内容使用同一个 `rawContent` 变量，避免重复转换

**效果对比**:

❌ **优化前**: 
```json
{
  "content": "\u6295\u6ce8 \u5927 100"  // 中文被转义
}
```

✅ **优化后**:
```json
{
  "content": "投注 大 100"  // 中文正常显示
}
```

### 📌 使用场景

1. **调试消息接收** - 快速查看接收到的消息详情
2. **日志分析** - 便于后续日志解析和数据分析
3. **问题排查** - 出现问题时可以准确定位消息内容
4. **监控运行** - 实时监控微信消息流

### 🔧 依赖库

- **jsoncpp** - JSON 序列化/反序列化
- **C++20** - 标准库支持

---

**修改时间**: 2025-11-04  
**编译状态**: ✅ 成功  
**功能状态**: ✅ 可用

