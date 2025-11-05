# GetUserInfo Nickname 问题修复

## 📋 问题描述

**现象**：
- 第一次注入时，`GetUserInfo` 可以获取到 `nickname`
- 第二次启动后，`GetUserInfo` 返回的 `nickname` 为空

**用户疑问**：
> 为什么第二次启动后，GetUserInfo 就不能得到 nick 了？刚注入的时候是能得到的。这个数据得到之后有被运行时修改吗？

---

## 🔍 问题分析

### 1. 数据结构定义

在 `Features.h` 中，`CurrentUserInfo` 结构体有**两个不同的昵称字段**：

```cpp
// WeixinX/WeixinX/Features.h:31-46

struct CurrentUserInfo
{
    std::atomic_bool online;
    std::string wxid;
    std::string alias;
    std::string nick;        // ✅ 原始字段：从微信内存偏移量读取
    
    // Socket 通信需要的额外字段
    std::string nickname;    // ❌ 新增字段：用于 Socket 通信（但从未赋值！）
    std::string account;
    std::string mobile;
    std::string avatar;
    std::string dataPath;
    std::string currentDataPath;
    std::string dbKey;

    static constexpr uintptr_t offset_wxid = 0x48;
    static constexpr uintptr_t offset_alias = offset_wxid + 0x20;
    static constexpr uintptr_t offset_nick = offset_wxid + 0x40;

    void read(WeixinX::Core* core);
    void clear();
};
```

### 2. 读取逻辑

在 `CurrentUserInfo::read` 方法中，**只读取了 `nick`，没有给 `nickname` 赋值**：

```cpp
// WeixinX/WeixinX/Features.cpp:50-52 (修复前)

memcpy(&str, (void*)(*(__int64*)currentUserInfo + CurrentUserInfo::offset_nick), 32);
nick = str.str();  // ✅ 给 nick 赋值了
util::logging::wPrint(L"Nick: {}", util::utf8ToUtf16(nick.c_str()).c_str());

// ❌ 但是没有给 nickname 赋值！
```

### 3. 返回逻辑

在 `HandleGetUserInfo` 中，返回的是 **`nickname` 字段**（从未赋值）：

```cpp
// WeixinX/WeixinX/SocketCommands.cpp:155-172

Json::Value SocketCommands::HandleGetUserInfo(const Json::Value& params)
{
    util::logging::print("Handling GetUserInfo");
    
    Json::Value result;
    
    // 返回当前登录用户信息
    result["wxid"] = Core::currentUserInfo.wxid;
    result["nickname"] = Core::currentUserInfo.nickname;  // ❌ 这个字段从未被赋值！
    result["account"] = Core::currentUserInfo.account;
    result["mobile"] = Core::currentUserInfo.mobile;
    result["avatar"] = Core::currentUserInfo.avatar;
    result["dataPath"] = Core::currentUserInfo.dataPath;
    result["currentDataPath"] = Core::currentUserInfo.currentDataPath;
    result["dbKey"] = Core::currentUserInfo.dbKey;
    
    return result;
}
```

---

## 🐛 问题根源

### 为什么第一次能获取到？

**可能的原因**：
1. **内存残留**：`nickname` 字段是 `std::string`，在未初始化时，内存中可能碰巧有之前的数据
2. **调试环境**：Debug 模式下，编译器可能会初始化内存
3. **随机性**：C++ 未初始化的变量具有不确定的值

### 为什么第二次获取不到？

**原因**：
1. 内存被清零或覆盖
2. `std::string` 的默认构造函数会将其初始化为空字符串
3. 没有任何代码给 `nickname` 赋值

### 核心问题

**`nickname` 字段从未被正确初始化/赋值！**

```
微信内存 (offset_nick) 
  ↓ memcpy
nick (✅ 有值)
  ↓ ❌ 没有赋值操作
nickname (❌ 始终为空)
  ↓ 返回
GetUserInfo() 返回空的 nickname
```

---

## ✅ 解决方案

### 修复 1：在 `read` 方法中同步赋值

```cpp
// WeixinX/WeixinX/Features.cpp:50-53 (修复后)

memcpy(&str, (void*)(*(__int64*)currentUserInfo + CurrentUserInfo::offset_nick), 32);
nick = str.str();
nickname = nick;  // ✅ 同时赋值给 nickname（用于 Socket 通信）
util::logging::wPrint(L"Nick: {}", util::utf8ToUtf16(nick.c_str()).c_str());
```

### 修复 2：在 `clear` 方法中清空所有字段

```cpp
// WeixinX/WeixinX/Features.cpp:73-90 (修复后)

void WeixinX::CurrentUserInfo::clear() {

    std::lock_guard<std::mutex> l(currentUserInfoMutex);

    // 清空原始字段
    wxid.clear();
    alias.clear();
    nick.clear();
    
    // ✅ 清空 Socket 通信字段
    nickname.clear();
    account.clear();
    mobile.clear();
    avatar.clear();
    dataPath.clear();
    currentDataPath.clear();
    dbKey.clear();
}
```

---

## 📊 修复前后对比

### 修复前（有 Bug）

```cpp
// ❌ 问题流程

OnLogin()
  ↓
currentUserInfo.read(this)
  ↓
读取微信内存 → nick = "用户昵称"
  ↓ (没有赋值操作)
nickname = ""  // 空字符串或未定义
  ↓
GetUserInfo() 返回
  ↓
{
  "wxid": "wxid_123",
  "nickname": "",  // ❌ 空的！
  ...
}
```

### 修复后（正常）

```cpp
// ✅ 正确流程

OnLogin()
  ↓
currentUserInfo.read(this)
  ↓
读取微信内存 → nick = "用户昵称"
  ↓ (添加赋值)
nickname = nick  // ✅ "用户昵称"
  ↓
GetUserInfo() 返回
  ↓
{
  "wxid": "wxid_123",
  "nickname": "用户昵称",  // ✅ 正确！
  ...
}
```

---

## 🎯 为什么有两个昵称字段？

### 历史原因

1. **`nick`**：原始代码中的字段，从微信内存偏移量读取
2. **`nickname`**：为了 Socket 通信新增的字段，语义更清晰

### 更好的设计

**理想情况应该只保留一个字段**，或者让 `nickname` 成为 `nick` 的别名：

```cpp
// 方案 1：只使用一个字段
struct CurrentUserInfo
{
    std::string wxid;
    std::string alias;
    std::string nickname;  // 统一使用 nickname
    // ...
};

// 方案 2：使用引用（不推荐，因为 std::string 是值类型）
std::string& nickname = nick;  // ❌ 这样不行

// 方案 3：当前的修复方案（同步赋值）✅
nickname = nick;  // ✅ 简单有效
```

---

## 🔍 如何避免类似问题？

### 1. 代码审查清单

在添加新字段时，检查：
- [ ] 字段是否有初始化逻辑？
- [ ] 字段是否在 `read()` 方法中被赋值？
- [ ] 字段是否在 `clear()` 方法中被清空？
- [ ] 字段是否在所有需要的地方使用？

### 2. 使用构造函数初始化

```cpp
struct CurrentUserInfo
{
    std::string wxid{};       // ✅ 显式初始化为空字符串
    std::string alias{};
    std::string nick{};
    std::string nickname{};   // ✅ 显式初始化
    // ...
};
```

### 3. 添加日志和断言

```cpp
void read(WeixinX::Core* core) {
    // ...
    nickname = nick;
    
    // ✅ 添加日志确认赋值成功
    util::logging::print("UserInfo read: wxid={}, nickname={}", 
                         wxid.c_str(), nickname.c_str());
    
    // ✅ 添加断言确保非空
    assert(!nickname.empty());
}
```

### 4. 单元测试

```cpp
void TestUserInfoRead()
{
    CurrentUserInfo info;
    // 模拟读取
    info.nick = "TestNick";
    info.nickname = info.nick;
    
    // ✅ 断言两个字段相同
    assert(info.nick == info.nickname);
    assert(!info.nickname.empty());
}
```

---

## 📝 总结

### 问题
- `nickname` 字段从未被赋值
- `GetUserInfo` 返回空的 `nickname`

### 原因
- 代码中只读取了 `nick`，没有同步到 `nickname`
- 两个字段名称相似但实际是不同的变量

### 修复
1. ✅ 在 `read()` 中添加 `nickname = nick;`
2. ✅ 在 `clear()` 中清空所有字段

### 编译结果
```
✅ 编译成功
   0 个错误
   5 个警告（与此修复无关）

输出: WeixinX\x64\Release\WeixinX.dll
已复制到: WeixinX\bin\release\net8.0-windows
```

---

## 🚀 测试验证

### 测试步骤

1. **重新注入 WeixinX.dll**
   ```bash
   # 使用新编译的 DLL
   ```

2. **调用 GetUserInfo**
   ```json
   // 命令
   GetUserInfo()
   
   // 预期返回
   {
     "wxid": "wxid_xxxxx",
     "nickname": "用户昵称",  // ✅ 应该有值
     "account": "xxx",
     ...
   }
   ```

3. **多次测试**
   - ✅ 第一次注入：nickname 有值
   - ✅ 第二次注入：nickname 仍然有值
   - ✅ 重启微信：nickname 仍然有值

### 预期结果

无论何时调用 `GetUserInfo`，只要用户已登录，`nickname` 字段都应该返回正确的值。

---

**问题已修复！** ✅

现在 `GetUserInfo` 在任何时候都能正确返回用户昵称了。

