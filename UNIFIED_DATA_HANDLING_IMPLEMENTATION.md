# 统一数据处理架构实现总结

## 📋 实现内容

### 问题回顾

用户提出了一个非常重要的架构问题：

> **问题**：如何避免"主动请求"和"服务器推送"维护两份相同的处理代码？
> 
> ```
> 场景1：主动请求（点击刷新按钮）
> UI → SendAsync("GetContacts") → 收到响应 → ❓如何处理数据
> 
> 场景2：服务器推送
> Server → OnServerPush("OnContactsUpdated") → Handler → ❓如何处理数据
> ```

### 解决方案：统一数据处理服务层

```
┌─────────────────────────────────────────────────────┐
│          ContactDataService (数据处理服务层)         │
│  ┌─────────────────────────────────────────────┐   │
│  │  ProcessContactsAsync(JsonElement data)      │   │
│  │  - 解析数据                                   │   │
│  │  - 保存到数据库（business-{wxid}.db）        │   │
│  │  - 触发事件通知 UI                            │   │
│  └─────────────────────────────────────────────┘   │
└──────────────┬──────────────────┬───────────────────┘
               │                  │
    ┌──────────▼──────┐    ┌─────▼──────────────┐
    │  Handler 调用   │    │  UI 主动调用        │
    │  (服务器推送)   │    │  (点击刷新按钮)     │
    └─────────────────┘    └────────────────────┘
```

**核心思想**：
- ✅ 只写一份数据处理代码
- ✅ Handler 和 UI 都调用同一个 Service
- ✅ 代码复用，逻辑统一

---

## 🎯 实现的三个需求

### 需求 1：删除联系人测试数据 + 服务端推送 GetUserInfo

#### 1.1 BaiShengVx3Plus 端

- ✅ 删除了 `VxMain.cs` 中的 `LoadTestData()` 联系人测试数据生成代码
- ✅ 创建了 `ContactDataService` 服务，统一处理联系人数据
- ✅ 添加了 `ContactsUpdated` 事件，通知 UI 更新
- ✅ 注册了 `ContactDataService` 到 DI 容器

#### 1.2 WeixinX 服务端

- ✅ 在客户端连接成功后，自动推送 `GetUserInfo` 数据
- ✅ 检查 `wxid` 是否为空，如果为空则不处理
- ✅ 推送格式：`{"method": "OnLogin", "params": {...}}`

**实现位置**：
```cpp
// WeixinX/WeixinX/SocketServer.cpp
void SocketServer::AcceptThread()
{
    // ...
    auto client = std::make_unique<ClientConnection>(clientSocket, this);
    ClientConnection* clientPtr = client.get();
    client->Start();
    m_clients.push_back(std::move(client));
    
    // 推送 GetUserInfo 数据给新连接的客户端
    PushUserInfoToClient(clientPtr);
}

void SocketServer::PushUserInfoToClient(ClientConnection* client)
{
    // 1. 调用 GetUserInfo 获取用户信息
    Json::Value result = HandleCommand("GetUserInfo", emptyParams);
    
    // 2. 检查 wxid 是否为空
    if (result["wxid"].asString().empty()) {
        return; // 不处理
    }
    
    // 3. 推送到客户端
    Json::Value message;
    message["method"] = "OnLogin";
    message["params"] = result;
    client->Send(messageStr);
}
```

#### 1.3 GetContacts 数据库句柄检查

- ✅ 检查句柄是否存在于 map 中
- ✅ 检查句柄值是否为空（0）
- ✅ 如果任一检查失败，返回错误 JSON，避免崩溃

**实现位置**：
```cpp
// WeixinX/WeixinX/Features.cpp
string WeixinX::Core::GetContacts()
{
    // 1. 检查数据库句柄是否存在
    if (WeixinX::Features::DBHandles.find("contact.db") == WeixinX::Features::DBHandles.end())
    {
        return "{\"error\": \"contact.db handle not found\"}";
    }
    
    // 2. 检查数据库句柄值是否为空（0）
    uintptr_t dbHandle = WeixinX::Features::DBHandles["contact.db"];
    if (dbHandle == 0)
    {
        return "{\"error\": \"contact.db handle is null, WeChat may not be logged in\"}";
    }
    
    // 3. 安全查询
    // ...
}
```

---

### 需求 2：添加刷新按钮 + 统一数据处理

#### 2.1 UI 层修改

**添加刷新按钮**：

```csharp
// VxMain.Designer.cs
private Sunny.UI.UIButton btnRefreshContacts;

// 在 pnlLeftTop 中添加（绑定按钮左边）
pnlLeftTop.Controls.Add(btnRefreshContacts);
pnlLeftTop.Controls.Add(btnBindingContacts);
pnlLeftTop.Controls.Add(lblContactList);
```

**刷新按钮事件处理**：

```csharp
// VxMain.cs
private async void btnRefreshContacts_Click(object sender, EventArgs e)
{
    _logService.Info("VxMain", "🔄 刷新联系人列表");
    lblStatus.Text = "正在获取联系人...";

    // 1. 主动请求联系人数据
    var contactsData = await _socketClient.SendAsync<JsonDocument>("GetContacts", 10000);

    if (contactsData != null)
    {
        // 2. 统一调用 ContactDataService 处理（和服务器推送一样的处理逻辑）
        await _contactDataService.ProcessContactsAsync(contactsData.RootElement);
        _logService.Info("VxMain", "✓ 联系人刷新成功");
    }
    else
    {
        _logService.Warning("VxMain", "获取联系人失败");
        UIMessageBox.ShowWarning("获取联系人失败\n请检查微信是否已登录");
    }
}
```

#### 2.2 服务层统一处理

**创建了 `IContactDataService` 接口**：

```csharp
// BaiShengVx3Plus/Services/IContactDataService.cs
public interface IContactDataService
{
    /// <summary>
    /// 处理联系人数据（统一入口，无论是主动请求还是服务器推送）
    /// </summary>
    Task<List<WxContact>> ProcessContactsAsync(JsonElement data);
    
    /// <summary>
    /// 保存联系人到数据库（business-{wxid}.db）
    /// </summary>
    Task SaveContactsAsync(List<WxContact> contacts);
    
    /// <summary>
    /// 从数据库加载联系人
    /// </summary>
    Task<List<WxContact>> LoadContactsAsync();
    
    /// <summary>
    /// 联系人数据更新事件
    /// </summary>
    event EventHandler<ContactsUpdatedEventArgs>? ContactsUpdated;
}
```

**实现了 `ContactDataService`**：

```csharp
// BaiShengVx3Plus/Services/ContactDataService.cs
public class ContactDataService : IContactDataService
{
    private string? _currentWxid; // 当前登录的微信 ID

    public async Task<List<WxContact>> ProcessContactsAsync(JsonElement data)
    {
        // 1. 解析联系人数据
        var contacts = ParseContacts(data);
        
        // 2. 保存到数据库（表名：contacts_{wxid}）
        await SaveContactsAsync(contacts);
        
        // 3. 触发事件通知 UI
        ContactsUpdated?.Invoke(this, new ContactsUpdatedEventArgs
        {
            Contacts = contacts,
            UpdateTime = DateTime.Now,
            Source = "Process"
        });
        
        return contacts;
    }
    
    public async Task SaveContactsAsync(List<WxContact> contacts)
    {
        // 保存到 business.db 的 contacts_{_currentWxid} 表
        // CREATE TABLE IF NOT EXISTS contacts_{wxid} (...)
        // DELETE FROM contacts_{wxid}
        // INSERT INTO contacts_{wxid} VALUES (...)
    }
}
```

#### 2.3 使用方式对比

**方式 1：UI 主动请求（点击刷新）**

```csharp
// 主动请求
var contactsData = await _socketClient.SendAsync<JsonDocument>("GetContacts");

// 统一处理
await _contactDataService.ProcessContactsAsync(contactsData.RootElement);
```

**方式 2：服务器推送（Handler）**

```csharp
// ContactsUpdateHandler.cs
public class ContactsUpdateHandler : IMessageHandler
{
    public async Task HandleAsync(JsonElement data)
    {
        // 统一处理（和主动请求一样的代码）
        await _contactDataService.ProcessContactsAsync(data);
    }
}
```

**结果**：
- ✅ 只写一份数据处理代码
- ✅ UI 和 Handler 都调用同一个 Service
- ✅ 代码复用，逻辑统一

---

### 需求 3：联系人数据显示和保存

#### 3.1 显示在 dgvContacts

**事件订阅**：

```csharp
// VxMain.cs 构造函数
_contactDataService.ContactsUpdated += ContactDataService_ContactsUpdated;

// 事件处理
private void ContactDataService_ContactsUpdated(object? sender, ContactsUpdatedEventArgs e)
{
    _logService.Info("VxMain", $"📇 联系人数据已更新，共 {e.Contacts.Count} 个");

    // 切换到 UI 线程更新
    if (InvokeRequired)
    {
        Invoke(new Action(() => UpdateContactsList(e.Contacts)));
    }
    else
    {
        UpdateContactsList(e.Contacts);
    }
}

// 更新列表
private void UpdateContactsList(List<WxContact> contacts)
{
    // 清空现有数据
    _contactsBindingList.Clear();

    // 添加新数据
    foreach (var contact in contacts)
    {
        _contactsBindingList.Add(contact);
    }

    lblStatus.Text = $"✓ 已更新 {contacts.Count} 个联系人";
}
```

#### 3.2 保存到 SQLite

**数据库表结构**：

```sql
CREATE TABLE IF NOT EXISTS contacts_{wxid} (
    wxid TEXT PRIMARY KEY,
    account TEXT,
    nickname TEXT,
    remark TEXT,
    avatar TEXT,
    sex INTEGER DEFAULT 0,
    province TEXT,
    city TEXT,
    country TEXT,
    is_group INTEGER DEFAULT 0,
    update_time INTEGER DEFAULT 0
)
```

**保存逻辑**：

```csharp
// ContactDataService.cs
public async Task SaveContactsAsync(List<WxContact> contacts)
{
    // 1. 创建表（如果不存在）
    // CREATE TABLE IF NOT EXISTS contacts_{_currentWxid} (...)
    
    // 2. 清空旧数据
    // DELETE FROM contacts_{_currentWxid}
    
    // 3. 批量插入新数据（使用事务）
    using (var transaction = conn.BeginTransaction())
    {
        foreach (var contact in contacts)
        {
            // INSERT INTO contacts_{_currentWxid} VALUES (...)
        }
        transaction.Commit();
    }
}
```

**特点**：
- ✅ 刷新一次记录一次（全量替换）
- ✅ 不是实时保存，而是批量保存
- ✅ 使用事务保证数据一致性

---

## 🚀 使用流程

### 流程 1：程序启动 → 自动获取用户信息

```
1. BaiShengVx3Plus 启动
2. 点击"采集"按钮，注入 WeixinX.dll
3. Socket 客户端连接成功
   ↓
4. WeixinX 服务端自动推送 GetUserInfo
   {"method": "OnLogin", "params": {"wxid": "xxx", ...}}
   ↓
5. LoginEventHandler 处理
   - 检查 wxid 是否为空
   - 如果不为空，初始化数据库（business-{wxid}.db）
   - ContactDataService.SetCurrentWxid(wxid)
   ↓
6. 数据库已就绪，可以开始保存联系人
```

### 流程 2：点击刷新按钮 → 获取联系人

```
1. 用户点击"刷新"按钮
   ↓
2. UI 主动请求
   var contacts = await _socketClient.SendAsync<JsonDocument>("GetContacts")
   ↓
3. 统一处理
   await _contactDataService.ProcessContactsAsync(contacts.RootElement)
   ↓
4. ContactDataService 处理
   - 解析数据
   - 保存到 contacts_{wxid} 表
   - 触发 ContactsUpdated 事件
   ↓
5. UI 更新
   - 清空 dgvContacts
   - 添加新数据
   - 显示状态
```

### 流程 3：服务器推送联系人更新

```
1. WeixinX 服务端检测到联系人变化
   ↓
2. 服务端推送
   Broadcast("OnContactsUpdated", contactsData)
   ↓
3. MessageDispatcher 分发
   ↓
4. ContactsUpdateHandler 处理
   await _contactDataService.ProcessContactsAsync(data)
   ↓
5. ContactDataService 处理（和主动请求一样的逻辑）
   - 解析数据
   - 保存到数据库
   - 触发事件
   ↓
6. UI 更新
```

---

## 📁 创建的文件

### C# 文件（BaiShengVx3Plus）

```
BaiShengVx3Plus/
├── Services/
│   ├── IContactDataService.cs           # 联系人数据服务接口
│   ├── ContactDataService.cs            # 联系人数据服务实现
│   └── Messages/
│       └── Handlers/
│           └── ContactsUpdateHandler.cs # 联系人更新处理器
└── Views/
    ├── VxMain.cs                        # 主窗口（修改）
    └── VxMain.Designer.cs               # Designer（添加刷新按钮）
```

### C++ 文件（WeixinX）

```
WeixinX/WeixinX/
├── SocketServer.h                       # 声明 PushUserInfoToClient
├── SocketServer.cpp                     # 实现 PushUserInfoToClient
└── Features.cpp                         # GetContacts 添加句柄检查
```

---

## 🎯 核心优势

### 1. 代码复用

**问题**：主动请求和服务器推送需要两份代码

**解决**：
```csharp
// ✅ 只写一份代码
public class ContactDataService
{
    public async Task<List<WxContact>> ProcessContactsAsync(JsonElement data)
    {
        // 统一的数据处理逻辑
    }
}

// UI 调用
await _contactDataService.ProcessContactsAsync(data);

// Handler 调用
await _contactDataService.ProcessContactsAsync(data);
```

### 2. 职责清晰

| 组件 | 职责 |
|------|------|
| `WeixinSocketClient` | Socket 通信、消息接收 |
| `MessageDispatcher` | 消息路由、分发 |
| `IMessageHandler` | 接收服务器推送 |
| `ContactDataService` | **统一数据处理（核心）** |
| `VxMain` | UI 更新、用户交互 |

### 3. 易于测试

```csharp
// 单元测试
[Test]
public async Task ContactDataService_Should_ParseAndSave()
{
    // Arrange
    var mockDb = new Mock<IDatabaseService>();
    var service = new ContactDataService(mockLog.Object, mockDb.Object);
    service.SetCurrentWxid("test_wxid");
    
    var jsonData = JsonDocument.Parse("[{\"username\":\"wxid_1\",\"nick_name\":\"张三\"}]");
    
    // Act
    var contacts = await service.ProcessContactsAsync(jsonData.RootElement);
    
    // Assert
    Assert.AreEqual(1, contacts.Count);
    Assert.AreEqual("wxid_1", contacts[0].Wxid);
    Assert.AreEqual("张三", contacts[0].Nickname);
}
```

### 4. 易于扩展

添加新的数据类型处理：

```csharp
// 1. 创建接口
public interface IGroupDataService
{
    Task<List<WxGroup>> ProcessGroupsAsync(JsonElement data);
}

// 2. 创建实现
public class GroupDataService : IGroupDataService
{
    public async Task<List<WxGroup>> ProcessGroupsAsync(JsonElement data)
    {
        // 和 ContactDataService 一样的结构
    }
}

// 3. Handler 和 UI 都调用同一个 Service
```

---

## 🔍 关键技术点

### 1. 数据库表名动态化

```csharp
// ContactDataService.cs
private string? _currentWxid;

public void SetCurrentWxid(string wxid)
{
    _currentWxid = wxid;
}

public async Task SaveContactsAsync(List<WxContact> contacts)
{
    // 表名：contacts_{wxid}
    var tableName = $"contacts_{_currentWxid}";
    var sql = $"CREATE TABLE IF NOT EXISTS {tableName} (...)";
    // ...
}
```

**优势**：
- ✅ 每个微信号使用独立的表
- ✅ 多开微信时数据不冲突
- ✅ 符合用户需求（business-{wxid}.db）

### 2. 事件驱动 UI 更新

```csharp
// Service 层
public event EventHandler<ContactsUpdatedEventArgs>? ContactsUpdated;

// 触发事件
ContactsUpdated?.Invoke(this, new ContactsUpdatedEventArgs
{
    Contacts = contacts,
    UpdateTime = DateTime.Now,
    Source = "Process"
});

// UI 层订阅
_contactDataService.ContactsUpdated += ContactDataService_ContactsUpdated;

// 线程安全更新
private void ContactDataService_ContactsUpdated(object? sender, ContactsUpdatedEventArgs e)
{
    if (InvokeRequired)
    {
        Invoke(new Action(() => UpdateContactsList(e.Contacts)));
    }
    else
    {
        UpdateContactsList(e.Contacts);
    }
}
```

### 3. 服务端安全检查

```cpp
// 检查句柄是否存在
if (DBHandles.find("contact.db") == DBHandles.end()) {
    return error;
}

// 检查句柄值是否为空
uintptr_t dbHandle = DBHandles["contact.db"];
if (dbHandle == 0) {
    return error;
}

// 安全查询
rc = get_table(dbHandle, sql, &result, &row, &col, &err);
```

**优势**：
- ✅ 避免空指针崩溃
- ✅ 友好的错误提示
- ✅ 不影响其他功能

---

## ✅ 总结

### 实现完成

- ✅ 删除联系人测试数据
- ✅ 服务端推送 GetUserInfo（检查 wxid）
- ✅ 添加刷新按钮
- ✅ 统一数据处理服务层
- ✅ 联系人数据显示和保存（business-{wxid}.db）
- ✅ GetContacts 数据库句柄检查

### 核心价值

1. **解决了代码重复问题**
   - 主动请求和服务器推送共用一份代码
   - 通过 Service 层统一处理

2. **符合 SOLID 原则**
   - 单一职责：每个类只负责一件事
   - 开闭原则：添加新功能无需修改现有代码
   - 依赖倒置：依赖接口而不是实现

3. **易于维护和扩展**
   - 清晰的分层架构
   - 统一的数据处理流程
   - 完善的错误处理

---

**统一数据处理架构已完整实现！** 🚀

**下一步**：
1. 编译 WeixinX.dll
2. 测试刷新按钮功能
3. 测试服务器推送 GetUserInfo
4. 验证数据库保存逻辑

