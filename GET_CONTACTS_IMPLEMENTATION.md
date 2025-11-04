# 📇 真实联系人查询功能实现

## ✅ 实现完成

已实现从微信 `contact.db` 数据库查询所有联系人并返回 JSON 格式数据。

---

## 📊 详细代码分析

### 1. 关键变量说明

```cpp
int rc;           // return code，SQLite 返回值，0 = 成功
int row;          // 查询结果的行数
int col;          // 查询结果的列数
char** result;    // 二维字符串数组指针
char* err;        // 错误信息指针
int idx;          // 遍历索引
```

### 2. result 数组布局

```
result[0] ~ result[col-1]                    : 列名（第一行）
result[col] ~ result[col + row*col - 1]      : 数据行
```

**示例**：查询 3 列 2 行

```
result[0] = "username"     result[1] = "nick_name"     result[2] = "remark"
result[3] = "wxid_001"     result[4] = "张三"           result[5] = "朋友"
result[6] = "wxid_002"     result[7] = "李四"           result[8] = "同事"
```

### 3. 遍历逻辑

```cpp
// idx 从 col 开始，跳过列名行
int idx = col;

for (int x = 0; x < row; x++)        // 遍历每一行
{
    for (int y = 0; y < col; y++)    // 遍历每一列
    {
        const char* columnName = result[y];      // 列名（从前 col 个元素获取）
        const char* value = result[idx++];       // 数据（idx 自动递增）
        
        // 使用 columnName 和 value
    }
}
```

### 4. 资源释放（重要！）

```cpp
// 必须调用 free_table 释放内存，否则会内存泄漏
util::invokeCdecl<void>(
    (void*)(base + WeixinX::weixin_dll::v41021::offset::db::free_table), 
    result
);
```

---

## 📋 Contact 表结构

```sql
CREATE TABLE contact(
    id INTEGER PRIMARY KEY,
    username TEXT,              -- wxid（微信ID）
    local_type INTEGER,
    alias TEXT,                 -- 微信号
    encrypt_username TEXT,
    flag INTEGER,
    delete_flag INTEGER,        -- 0=正常，1=已删除
    verify_flag INTEGER,        -- 认证标志
    remark TEXT,                -- 备注名
    remark_quan_pin TEXT,
    remark_pin_yin_initial TEXT,
    nick_name TEXT,             -- 昵称
    pin_yin_initial TEXT,
    quan_pin TEXT,
    big_head_url TEXT,          -- 头像大图URL
    small_head_url TEXT,        -- 头像小图URL
    head_img_md5 TEXT,
    chat_room_notify INTEGER,
    is_in_chat_room INTEGER,
    description TEXT,           -- 个性签名
    extra_buffer BLOB,          -- 额外数据（二进制）
    chat_room_type INTEGER      -- 0=普通好友，1=群聊
)
```

---

## 🔧 实现细节

### Core::GetContacts() 函数

**位置**：`WeixinX/WeixinX/Features.cpp` (第 535-661 行)

**返回值**：`std::string` (JSON 格式)

### 实现步骤

#### 步骤 1：检查数据库句柄

```cpp
if (WeixinX::Features::DBHandles.find("contact.db") == WeixinX::Features::DBHandles.end())
{
    // 返回错误 JSON
    Json::Value error;
    error["error"] = "contact.db handle not found";
    return Json::writeString(builder, error);
}
```

#### 步骤 2：准备查询变量

```cpp
uintptr_t base = util::getWeixinDllBase();
char* err = nullptr;
char** result = nullptr;
int row = 0, col = 0;
int rc;
```

#### 步骤 3：构建 SQL 查询

```cpp
std::string sql = 
    "SELECT "
    "username, "           // wxid
    "nick_name, "          // 昵称
    "alias, "              // 微信号
    "remark, "             // 备注
    "small_head_url, "     // 头像
    "description, "        // 个性签名
    "verify_flag, "        // 认证标志
    "chat_room_type "      // 群聊类型
    "FROM contact "
    "WHERE delete_flag = 0 "  // 排除已删除的联系人
    "ORDER BY username";
```

**为什么选择这些字段？**
- ✅ 包含最常用的联系人信息
- ✅ 排除 BLOB 字段（`extra_buffer`）避免数据过大
- ✅ 排除不常用的拼音字段

#### 步骤 4：调用 get_table 查询

```cpp
rc = util::invokeCdecl<int>(
    (void*)(base + WeixinX::weixin_dll::v41021::offset::db::get_table),
    WeixinX::Features::DBHandles["contact.db"],
    sql.c_str(), 
    &result, 
    &row, 
    &col, 
    &err
);
```

**参数说明**：
1. `get_table` 函数地址
2. 数据库句柄
3. SQL 语句
4. 结果指针（输出）
5. 行数指针（输出）
6. 列数指针（输出）
7. 错误信息指针（输出）

#### 步骤 5：解析结果并构建 JSON

```cpp
Json::Value contacts(Json::arrayValue);

if (rc == 0 && row > 0 && col > 0)
{
    int idx = col;  // 跳过列名行
    
    for (int x = 0; x < row; x++)
    {
        Json::Value contact;
        
        for (int y = 0; y < col; y++)
        {
            const char* columnName = result[y];
            const char* value = result[idx++];
            
            // NULL 检查
            if (value != nullptr && strlen(value) > 0)
            {
                contact[columnName] = value;
            }
            else
            {
                contact[columnName] = "";
            }
        }
        
        contacts.append(contact);
    }
}
```

**关键点**：
- ✅ `idx` 从 `col` 开始（跳过列名）
- ✅ 检查 `value` 是否为 `nullptr`
- ✅ 使用列名作为 JSON 键

#### 步骤 6：释放资源

```cpp
if (result != nullptr)
{
    util::invokeCdecl<void>(
        (void*)(base + WeixinX::weixin_dll::v41021::offset::db::free_table), 
        result
    );
    util::logging::print("GetContacts: Resources freed");
}
```

**重要性**：
- ⚠️ 不释放会导致内存泄漏
- ⚠️ 即使查询失败也要检查并释放
- ⚠️ `result` 不为 `nullptr` 时才释放

#### 步骤 7：转换为 JSON 字符串

```cpp
Json::StreamWriterBuilder builder;
builder["indentation"] = "  ";       // 格式化输出
builder["emitUTF8"] = true;         // 支持中文
std::string jsonString = Json::writeString(builder, contacts);

return jsonString;
```

---

## 🌐 Socket 命令处理器

### HandleGetContacts 实现

**位置**：`WeixinX/WeixinX/SocketCommands.cpp` (第 19-56 行)

```cpp
Json::Value SocketCommands::HandleGetContacts(const Json::Value& params)
{
    try {
        // 1. 获取 Core 单例
        auto& core = util::Singleton<Core>::Get();
        
        // 2. 调用数据库查询
        std::string jsonString = core.GetContacts();
        
        // 3. 解析 JSON 字符串为 Json::Value
        Json::Value result;
        JSONCPP_STRING err;
        Json::CharReaderBuilder builder;
        const std::unique_ptr<Json::CharReader> reader(builder.newCharReader());
        
        if (reader->parse(jsonString.c_str(), 
                         jsonString.c_str() + jsonString.length(), 
                         &result, &err))
        {
            return result;
        }
        else
        {
            Json::Value error;
            error["error"] = "Failed to parse contacts JSON";
            return error;
        }
    }
    catch (const std::exception& e) {
        Json::Value error;
        error["error"] = std::format("Failed to get contacts: {}", e.what());
        return error;
    }
}
```

---

## 📊 响应格式

### 成功响应

```json
[
  {
    "username": "wxid_abc123",
    "nick_name": "张三",
    "alias": "zhangsan",
    "remark": "朋友",
    "small_head_url": "http://wx.qlogo.cn/...",
    "description": "这是我的个性签名",
    "verify_flag": "0",
    "chat_room_type": "0"
  },
  {
    "username": "123456789@chatroom",
    "nick_name": "技术交流群",
    "alias": "",
    "remark": "",
    "small_head_url": "http://wx.qlogo.cn/...",
    "description": "",
    "verify_flag": "0",
    "chat_room_type": "1"
  }
]
```

### 错误响应

```json
{
  "error": "contact.db handle not found"
}
```

或

```json
{
  "error": "Failed to get contacts: <exception message>"
}
```

---

## 🧪 测试步骤

### 1. 准备测试环境

```bash
# 关闭所有微信
taskkill /F /IM WeChat.exe
```

### 2. 启动并注入

1. 启动 BaiShengVx3Plus
2. 点击"采集"，启动微信并注入
3. 等待 Socket 连接建立

### 3. 查询联系人

在设置窗口输入：
```
GetContacts()
```

### 4. 预期结果

**DebugView 日志**：
```
[WeixinX] GetContacts: Starting to query contact database
[WeixinX] GetContacts: Executing SQL
[WeixinX] GetContacts: Query successful, rows=150, cols=8
[WeixinX] GetContacts: Parsed 150 contacts
[WeixinX] GetContacts: Resources freed
[WeixinX] GetContacts: Returning 25678 bytes of JSON
[WeixinX] GetContacts: Successfully parsed 150 contacts
```

**客户端响应**：
```json
[
  {
    "username": "wxid_001",
    "nick_name": "好友1",
    ...
  },
  {
    "username": "wxid_002",
    "nick_name": "好友2",
    ...
  },
  ...
]
```

---

## 🎯 字段说明

| 字段 | 类型 | 说明 |
|------|------|------|
| `username` | TEXT | 微信ID（wxid），唯一标识符 |
| `nick_name` | TEXT | 昵称（显示名称） |
| `alias` | TEXT | 微信号（可能为空） |
| `remark` | TEXT | 备注名（您设置的备注） |
| `small_head_url` | TEXT | 头像URL |
| `description` | TEXT | 个性签名 |
| `verify_flag` | INTEGER | 认证标志（公众号等） |
| `chat_room_type` | INTEGER | 0=普通好友，1=群聊 |

---

## ⚠️ 注意事项

### 1. 内存管理

```cpp
// ✅ 正确：检查 result 不为 nullptr
if (result != nullptr)
{
    util::invokeCdecl<void>(..., result);
}

// ❌ 错误：未检查就释放
util::invokeCdecl<void>(..., result);  // 如果 result 为 nullptr 会崩溃
```

### 2. NULL 值处理

```cpp
// ✅ 正确：检查 value 是否为 nullptr
if (value != nullptr && strlen(value) > 0)
{
    contact[columnName] = value;
}
else
{
    contact[columnName] = "";  // 空字符串
}

// ❌ 错误：未检查就使用
contact[columnName] = value;  // value 可能为 nullptr
```

### 3. 索引计算

```cpp
// ✅ 正确：idx 从 col 开始
int idx = col;

// ❌ 错误：idx 从 0 开始会读取列名
int idx = 0;
```

### 4. 数据库句柄检查

```cpp
// ✅ 正确：先检查句柄是否存在
if (DBHandles.find("contact.db") == DBHandles.end())
{
    return error_json;
}

// ❌ 错误：直接使用可能不存在的句柄
auto handle = DBHandles["contact.db"];  // 可能崩溃
```

---

## 🚀 扩展功能

### 1. 按条件筛选

```cpp
// 只查询好友（排除群聊）
std::string sql = 
    "SELECT ... FROM contact "
    "WHERE delete_flag = 0 AND chat_room_type = 0 "
    "ORDER BY username";
```

### 2. 模糊搜索

```cpp
// 按昵称搜索
std::string sql = std::format(
    "SELECT ... FROM contact "
    "WHERE delete_flag = 0 AND nick_name LIKE '%{}%' "
    "ORDER BY username",
    searchKeyword
);
```

### 3. 分页查询

```cpp
// 限制返回数量
std::string sql = std::format(
    "SELECT ... FROM contact "
    "WHERE delete_flag = 0 "
    "ORDER BY username "
    "LIMIT {} OFFSET {}",
    pageSize, offset
);
```

---

## 📊 编译信息

```
编译时间: 2025/11/5 1:25:39
输出位置: D:\gitcode\wx4helper\WeixinX\bin\release\net8.0-windows\WeixinX.dll
编译结果: ✅ 成功（1 个警告，0 个错误）
```

---

## 📋 总结

### 实现的功能
✅ 查询 contact.db 数据库  
✅ 解析查询结果为 JSON  
✅ 正确的资源管理（free_table）  
✅ NULL 值安全处理  
✅ 详细的日志记录  
✅ 异常处理  

### 技术要点
- 使用 `get_table` API 查询数据库
- 理解 result 数组的二维布局
- 正确管理内存（free_table）
- 处理 NULL 值
- JSON 序列化和反序列化

### 下一步
- 实现群成员查询（GetGroupContacts）
- 实现搜索功能
- 实现分页功能
- 缓存联系人列表

---

**状态**：✅ **已完成并编译成功**

**测试建议**：
1. 先查询少量联系人测试
2. 检查 DebugView 日志
3. 验证 JSON 格式正确
4. 确认没有内存泄漏

