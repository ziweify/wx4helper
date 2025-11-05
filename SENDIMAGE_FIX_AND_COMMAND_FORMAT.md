# 🖼️ SendImage 修复和命令格式说明

## ✅ 问题 1：SendImage 文件检查（防止崩溃）

### 问题描述
用户发送了不存在的图片路径，导致微信崩溃：
```
SendImage(27206515609@chatroom, d:/1.png)  // 文件不存在 → 微信崩溃
```

### 修复方案

已在 `Core::SendImage` 函数中添加了**三重检查**：

#### 1. 检查文件是否存在

```cpp
DWORD fileAttr = GetFileAttributesA(which.c_str());
if (fileAttr == INVALID_FILE_ATTRIBUTES)
{
    util::logging::print("SendImage: File not found: {}", which);
    throw std::runtime_error(std::format("Image file not found: {}", which));
}
```

**作用**：
- 使用 Windows API `GetFileAttributesA` 检查文件
- 如果文件不存在，返回 `INVALID_FILE_ATTRIBUTES`
- 抛出异常，阻止后续调用，防止崩溃

#### 2. 检查是否是目录

```cpp
if (fileAttr & FILE_ATTRIBUTE_DIRECTORY)
{
    util::logging::print("SendImage: Path is a directory, not a file: {}", which);
    throw std::runtime_error(std::format("Path is a directory, not a file: {}", which));
}
```

**作用**：
- 防止用户误传目录路径
- 例如：`d:/images/` 而不是 `d:/images/1.png`

#### 3. 检查文件扩展名

```cpp
std::string lowerPath = which;
std::transform(lowerPath.begin(), lowerPath.end(), lowerPath.begin(), ::tolower);

bool isValidImageExt = 
    lowerPath.ends_with(".jpg") || 
    lowerPath.ends_with(".jpeg") || 
    lowerPath.ends_with(".png") || 
    lowerPath.ends_with(".gif") || 
    lowerPath.ends_with(".bmp");

if (!isValidImageExt)
{
    util::logging::print("SendImage: Invalid image format: {}", which);
    throw std::runtime_error(std::format("Invalid image format (must be jpg/jpeg/png/gif/bmp): {}", which));
}
```

**作用**：
- 确保文件是支持的图片格式
- 支持的格式：`.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`
- 防止发送非图片文件

#### 4. 成功日志

```cpp
util::logging::print("SendImage: File validated successfully: {}", which);
// ... 发送图片 ...
util::logging::print("SendImage: Image sent successfully to {}", who);
```

---

## ✅ 问题 2：命令参数是否需要引号？

### 答案：**可以使用引号，也可以不使用**

### 解析逻辑

命令解析器 (`ParseCommand`) 支持以下格式：

#### 格式 1：不使用引号（推荐简单参数）

```
SendImage(27206515609@chatroom, d:/1.png)
```

**解析结果**：
- 参数 1: `"27206515609@chatroom"` (字符串)
- 参数 2: `"d:/1.png"` (字符串)

#### 格式 2：使用引号（推荐包含特殊字符）

```
SendImage("27206515609@chatroom", "d:/1.png")
```

**解析结果**：
- 参数 1: `"27206515609@chatroom"` (字符串)
- 参数 2: `"d:/1.png"` (字符串)

**效果相同！**

---

## 📋 参数解析规则

### 代码逻辑（ParseCommand）

```csharp
foreach (var part in parts)
{
    string trimmed = part.Trim();
    
    // 1. 如果有引号，作为字符串处理（去除引号）
    if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
    {
        paramList.Add(trimmed.Trim('"'));
    }
    // 2. 尝试解析为整数
    else if (int.TryParse(trimmed, out int intValue))
    {
        paramList.Add(intValue);
    }
    // 3. 尝试解析为浮点数
    else if (double.TryParse(trimmed, out double doubleValue))
    {
        paramList.Add(doubleValue);
    }
    // 4. 尝试解析为布尔值
    else if (bool.TryParse(trimmed, out bool boolValue))
    {
        paramList.Add(boolValue);
    }
    // 5. 默认作为字符串
    else
    {
        paramList.Add(trimmed);
    }
}
```

### 解析示例

| 输入 | 解析结果 | 类型 |
|------|---------|------|
| `wxid_123` | `"wxid_123"` | string |
| `"wxid_123"` | `"wxid_123"` | string |
| `123` | `123` | int |
| `"123"` | `"123"` | string |
| `3.14` | `3.14` | double |
| `"3.14"` | `"3.14"` | string |
| `true` | `true` | bool |
| `"true"` | `"true"` | string |
| `d:/1.png` | `"d:/1.png"` | string |
| `"d:/1.png"` | `"d:/1.png"` | string |

---

## 🎯 推荐使用方式

### 简单路径（不包含空格、逗号）

```
✅ SendImage(wxid_123, d:/images/photo.png)
✅ SendImage(wxid_123, "d:/images/photo.png")
```

两种都可以！

### 包含空格的路径

```
❌ SendImage(wxid_123, d:/my photos/photo.png)     // 错误：空格会被解析为分隔符
✅ SendImage(wxid_123, "d:/my photos/photo.png")   // 正确：引号保护空格
```

**必须使用引号！**

### 包含逗号的参数

```
❌ SendMessage(wxid_123, Hello, world!)            // 错误：逗号会被解析为参数分隔符
✅ SendMessage(wxid_123, "Hello, world!")          // 正确：引号保护逗号
```

**必须使用引号！**

### 包含@符号（群聊ID）

```
✅ SendImage(27206515609@chatroom, d:/1.png)       // 不需要引号
✅ SendImage("27206515609@chatroom", "d:/1.png")   // 使用引号也可以
```

两种都可以！

---

## 🧪 测试场景

### 场景 1：文件不存在

**命令**：
```
SendImage(wxid_123, d:/not_exist.png)
```

**预期结果**：
```json
{
  "error": "Failed to send message: Image file not found: d:/not_exist.png"
}
```

**DebugView 日志**：
```
[WeixinX] SendImage: File not found: d:/not_exist.png
```

**微信状态**：✅ 不会崩溃

---

### 场景 2：路径是目录

**命令**：
```
SendImage(wxid_123, d:/images/)
```

**预期结果**：
```json
{
  "error": "Failed to send message: Path is a directory, not a file: d:/images/"
}
```

**微信状态**：✅ 不会崩溃

---

### 场景 3：无效的图片格式

**命令**：
```
SendImage(wxid_123, d:/document.pdf)
```

**预期结果**：
```json
{
  "error": "Failed to send message: Invalid image format (must be jpg/jpeg/png/gif/bmp): d:/document.pdf"
}
```

**微信状态**：✅ 不会崩溃

---

### 场景 4：正确的图片

**命令**：
```
SendImage(wxid_123, d:/photo.jpg)
```

**预期结果**：
```json
{
  "success": true,
  "messageId": "msg_1730738993"
}
```

**DebugView 日志**：
```
[WeixinX] SendImage: File validated successfully: d:/photo.jpg
[WeixinX] SendImage: Image sent successfully to wxid_123
```

**微信状态**：✅ 图片发送成功

---

## 📊 编译信息

```
编译时间: 2025/11/5 8:59:40
输出位置: D:\gitcode\wx4helper\WeixinX\bin\release\net8.0-windows\WeixinX.dll
编译结果: ✅ 成功（5 个警告，0 个错误）
```

---

## 🎯 命令格式总结

### 基本格式

```
MethodName(param1, param2, param3, ...)
```

### 参数类型

| 类型 | 不使用引号 | 使用引号 | 推荐 |
|------|----------|---------|------|
| **wxid** | `wxid_123` | `"wxid_123"` | 两者都可 |
| **群ID** | `123@chatroom` | `"123@chatroom"` | 两者都可 |
| **简单路径** | `d:/1.png` | `"d:/1.png"` | 两者都可 |
| **包含空格的路径** | ❌ 错误 | `"d:/my photos/1.png"` | **必须引号** |
| **包含逗号的文本** | ❌ 错误 | `"Hello, world!"` | **必须引号** |
| **纯数字** | `123` → int | `"123"` → string | 看需求 |

---

## 💡 最佳实践

### ✅ 推荐做法

```
// 1. 简单参数可以不用引号
SendMessage(wxid_123, Hello)
GetContacts()
GetUserInfo()

// 2. 复杂参数使用引号
SendMessage(wxid_123, "Hello, world!")
SendImage(wxid_123, "d:/my photos/photo.jpg")

// 3. 养成习惯，统一使用引号（更安全）
SendMessage("wxid_123", "Hello, world!")
SendImage("27206515609@chatroom", "d:/images/photo.png")
```

### ❌ 避免的错误

```
// 路径包含空格但没有引号
SendImage(wxid_123, d:/my photos/photo.jpg)  // ❌ 解析错误

// 消息包含逗号但没有引号
SendMessage(wxid_123, Hello, world!)          // ❌ 参数过多

// 拼写错误
sendImage(wxid_123, d:/1.png)                 // ❌ 方法名大小写敏感
```

---

## 📋 支持的命令列表

### 1. GetContacts()
```
GetContacts()
```
获取所有联系人列表（无参数）

### 2. GetUserInfo()
```
GetUserInfo()
```
获取当前登录用户信息（无参数）

### 3. GetGroupContacts(groupId)
```
GetGroupContacts(123456789@chatroom)
GetGroupContacts("123456789@chatroom")
```
获取群成员列表

### 4. SendMessage(wxid, message)
```
SendMessage(wxid_123, Hello)
SendMessage(wxid_123, "Hello, world!")
SendMessage("27206515609@chatroom", "大家好！")
```
发送文本消息

### 5. SendImage(wxid, imagePath)
```
SendImage(wxid_123, d:/photo.jpg)
SendImage(wxid_123, "d:/photo.jpg")
SendImage("27206515609@chatroom", "d:/images/photo.png")
```
发送图片消息（现在带文件检查，不会崩溃）

---

## 🚀 测试步骤

### 1. 准备测试文件

```bash
# 创建一个测试图片
# 例如：d:/test.png
```

### 2. 测试文件不存在（验证不崩溃）

```
SendImage(filehelper, d:/not_exist.png)
```

预期：返回错误信息，微信不崩溃 ✅

### 3. 测试正确的文件

```
SendImage(filehelper, d:/test.png)
SendImage(filehelper, "d:/test.png")
```

预期：图片发送成功 ✅

### 4. 测试带空格的路径

```
SendImage(filehelper, "d:/test folder/test.png")
```

预期：正常工作（如果路径存在）✅

---

## 📄 相关修改

### 修改的文件
- ✅ `WeixinX/WeixinX/Features.cpp`
  - 在 `SendImage` 函数中添加了三重检查
  - 文件存在性检查
  - 目录检查
  - 格式检查

---

## ⚠️ 注意事项

### 1. 路径格式

Windows 路径可以使用：
- 正斜杠：`d:/images/photo.png` ✅
- 反斜杠：`d:\images\photo.png` ✅（但需要引号）
- UNC 路径：`\\server\share\photo.png` ✅（需要引号）

### 2. 文件格式限制

支持的图片格式：
- ✅ `.jpg` / `.jpeg`
- ✅ `.png`
- ✅ `.gif`
- ✅ `.bmp`

不支持的格式：
- ❌ `.webp`
- ❌ `.svg`
- ❌ `.pdf`
- ❌ `.doc`

### 3. 文件大小

建议：
- 图片大小不超过 10MB
- 尺寸不超过 4096x4096

---

## 🎉 总结

### 问题 1：SendImage 崩溃 - 已修复 ✅
- 添加了文件存在性检查
- 添加了目录检查
- 添加了格式检查
- 现在发送不存在的文件会返回错误，不会崩溃

### 问题 2：命令参数引号 - 已解答 ✅
- **可以使用引号，也可以不使用**
- 简单参数（无空格、无逗号）：两种都可以
- 复杂参数（有空格、有逗号）：**必须使用引号**
- **推荐**：养成习惯统一使用引号，更安全

---

**状态**：✅ **已完成并编译成功**

**测试建议**：
1. 先测试文件不存在的情况（验证不崩溃）
2. 再测试正确的文件
3. 测试带引号和不带引号的命令
4. 测试包含空格的路径

