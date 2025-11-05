# 服务划分快速参考表 🚀

## 📊 当前项目服务一览

| 服务名称 | 类型 | 职责 | 代码行数 | 依赖数量 |
|---------|------|------|---------|----------|
| **WeChatLoaderService** | Infrastructure | 进程管理（启动、注入） | ~150 | 0 |
| **WeixinSocketClient** | Infrastructure | Socket通信 | ~300 | 1 |
| **DatabaseService** | Infrastructure | 数据库连接管理 | ~100 | 0 |
| **LogService** | Infrastructure | 日志记录 | ~200 | 1 |
| **ContactDataService** | Domain | 联系人业务逻辑 | ~300 | 2 |
| **UserInfoService** | Domain | 用户信息管理 | ~100 | 1 |
| **WeChatService** | Application | 微信流程编排 | ~250 | 5 |

**总计**: 7个服务，~1400行代码（平均每个服务200行）

---

## 🎯 服务职责对照表

### 如果我想...应该调用哪个服务？

| 需求 | 调用的服务 | 方法 |
|------|-----------|------|
| **启动微信** | WeChatLoaderService | `LaunchWeChat()` |
| **注入DLL** | WeChatLoaderService | `InjectToProcess()` |
| **连接Socket** | WeixinSocketClient | `ConnectAsync()` |
| **发送命令** | WeixinSocketClient | `SendAsync()` |
| **获取联系人数据** | WeChatService | `RefreshContactsAsync()` |
| **处理联系人JSON** | ContactDataService | `ProcessContactsAsync()` |
| **保存联系人到DB** | ContactDataService | `SaveContactsAsync()` |
| **更新用户信息** | UserInfoService | `UpdateUserInfo()` |
| **写日志** | LogService | `Info() / Error()` |
| **完整连接流程** | WeChatService | `ConnectAndInitializeAsync()` |

---

## 🔄 服务调用关系图

```
┌──────────────────────────────────────────────────────┐
│                    UI Layer                           │
│                    VxMain.cs                          │
│                                                       │
│  _wechatService.ConnectAndInitializeAsync()          │
└────────────────────┬─────────────────────────────────┘
                     │ 调用
┌────────────────────▼─────────────────────────────────┐
│              Application Service                      │
│                WeChatService                          │
│                                                       │
│  编排流程：                                            │
│  1. LaunchOrInjectWeChatAsync()                      │
│  2. ConnectSocketAsync()                             │
│  3. RefreshUserInfoAsync()                           │
│  4. RefreshContactsAsync()                           │
└──┬────────┬────────┬────────┬──────────────┬─────────┘
   │        │        │        │              │
   │ 调用   │ 调用   │ 调用   │ 调用         │ 调用
   ▼        ▼        ▼        ▼              ▼
┌──────┐ ┌─────┐ ┌───────┐ ┌──────────┐ ┌─────────┐
│Loader│ │Socket│ │UserInfo│ │Contact  │ │Database│
│Service│ │Client│ │Service │ │DataSvc  │ │Service │
└──────┘ └─────┘ └───────┘ └──────────┘ └─────────┘
   │        │                    │              │
   │        └─────────┬──────────┘              │
   │                  │ 调用                    │ 调用
   │                  ▼                         ▼
   │              ┌─────┐                   ┌──────┐
   │              │ Log │                   │SQLite│
   │              │Service│                   │  DB  │
   │              └─────┘                   └──────┘
   │
   └─► Native DLL (Loader.dll)
```

---

## 📋 服务分层速查表

### Infrastructure Layer（技术基础设施层）

| 服务 | 提供什么能力？ | 依赖什么？ |
|------|--------------|-----------|
| WeChatLoaderService | 进程管理能力 | Loader.dll |
| WeixinSocketClient | Socket通信能力 | WinSock API |
| DatabaseService | 数据库连接管理 | SQLite |
| LogService | 日志记录能力 | SQLite |

**特点**：
- ✅ 提供技术能力
- ✅ 不包含业务逻辑
- ✅ 可以在其他项目中复用

### Domain Layer（领域业务层）

| 服务 | 管理什么概念？ | 依赖什么？ |
|------|--------------|-----------|
| ContactDataService | "联系人"业务概念 | Database, Log |
| UserInfoService | "用户信息"业务概念 | Log |
| OrderService | "订单"业务概念 | Database, Log |
| MemberService | "会员"业务概念 | Database, Log |

**特点**：
- ✅ 包含业务规则
- ✅ 操作业务实体
- ✅ 触发领域事件

### Application Layer（应用编排层）

| 服务 | 编排什么流程？ | 依赖什么？ |
|------|--------------|-----------|
| WeChatService | 微信连接和初始化流程 | Loader, Socket, UserInfo, Contact, Database, Log |

**特点**：
- ✅ 编排业务流程
- ✅ 协调多个服务
- ✅ 管理事务边界
- ✅ 处理重试逻辑

---

## 🎯 何时添加新服务？决策树

```
新功能来了，应该怎么办？
  ↓
Step 1: 这是什么类型的功能？
  ├─ 技术能力（Socket、数据库、文件）
  │   ↓
  │   Step 2: 已有的技术服务能覆盖吗？
  │   ├─ 能 → 添加到现有的 Infrastructure Service
  │   └─ 不能 → 创建新的 Infrastructure Service
  │
  ├─ 业务逻辑（数据处理、规则验证）
  │   ↓
  │   Step 2: 属于哪个业务概念？
  │   ├─ 联系人 → 添加到 ContactDataService
  │   ├─ 订单 → 添加到 OrderService
  │   ├─ 会员 → 添加到 MemberService
  │   └─ 新概念 → 创建新的 Domain Service
  │
  └─ 流程编排（多服务协调）
      ↓
      Step 2: 属于哪个业务流程？
      ├─ 微信相关 → 添加到 WeChatService
      ├─ 支付相关 → 添加到 PaymentService
      └─ 新流程 → 创建新的 Application Service
```

---

## 💡 常见场景示例

### 场景1：添加"发送图片"功能

```
问：应该放在哪个服务？

分析：
- 发送图片 = Socket通信 + 文件处理
- 属于技术能力

答案：
✅ 在 WeixinSocketClient 添加 SendImageAsync() 方法

理由：
- 发送是Socket通信的一种
- 不涉及复杂业务逻辑
```

### 场景2：添加"联系人去重"功能

```
问：应该放在哪个服务？

分析：
- 联系人去重 = 联系人业务规则
- 属于业务逻辑

答案：
✅ 在 ContactDataService 添加私有方法 DeduplicateContacts()
✅ 在 ProcessContactsAsync() 中调用

理由：
- 去重是联系人的业务规则
- 不需要对外暴露（私有方法）
```

### 场景3：添加"订单结算"功能

```
问：应该放在哪个服务？

分析：
- 订单结算 = 计算订单总额 + 更新订单状态 + 更新会员积分
- 涉及多个业务概念（订单、会员）
- 需要编排多个服务

答案：
✅ 创建 OrderCheckoutService（Application Service）
✅ 调用 OrderService 和 MemberService

理由：
- 涉及多个领域服务的协调
- 需要事务管理
```

---

## 🚫 反模式警告

### ❌ God Object（上帝对象）

```csharp
// ❌ 一个服务做所有事情
public class WeChatService
{
    // 100+ 个方法
    // 2000+ 行代码
    // 无法维护
}
```

### ❌ Circular Dependency（循环依赖）

```csharp
// ❌ A依赖B，B依赖A
ContactService → OrderService → ContactService
```

### ❌ Feature Envy（特性依恋）

```csharp
// ❌ OrderService 总是访问 Contact 的数据
public class OrderService
{
    public void ProcessOrder(Order order)
    {
        var contact = _contactService.GetContact(order.ContactId);
        // 大量操作 contact 的数据
        // 这些逻辑应该在 ContactService 中
    }
}
```

---

## ✅ 最佳实践检查清单

### 新增服务时

- [ ] 服务名称清楚表明职责
- [ ] 服务代码不超过500行
- [ ] 服务依赖不超过5个
- [ ] 服务只有一个修改的理由
- [ ] 服务的所有方法都服务于同一个目标
- [ ] 服务可以独立测试
- [ ] 服务接口不超过10个方法

### 修改服务时

- [ ] 修改符合服务的职责
- [ ] 不会影响其他不相关的功能
- [ ] 有对应的单元测试
- [ ] 接口保持向后兼容

---

## 🎓 记住这些原则

### 1. 单一职责原则（SRP）
```
一个服务只做一件事，并把它做好
```

### 2. 开闭原则（OCP）
```
对扩展开放，对修改封闭
添加新功能时，不修改现有服务
```

### 3. 依赖倒置原则（DIP）
```
依赖抽象（接口），不依赖具体实现
```

### 4. 接口隔离原则（ISP）
```
不应该强迫客户端依赖它不使用的方法
保持接口小而精
```

### 5. 不要过度设计
```
先让功能跑起来，再逐步重构
不要一开始就创建一堆服务
```

---

## 📞 快速决策表

| 问题 | 答案 |
|------|------|
| 这个方法属于哪个层？ | 技术→Infrastructure<br>业务→Domain<br>流程→Application |
| 服务太大了（>500行）| 拆分！ |
| 服务太小了（<50行）| 考虑合并 |
| 两个服务循环依赖 | 重构！提取公共依赖 |
| 服务有10个依赖 | 太多了！拆分或重构 |
| UI直接调用Infrastructure | ❌ 应该通过Application Service |
| Application Service实现业务逻辑 | ❌ 应该委托给Domain Service |
| Domain Service做Socket通信 | ❌ 应该依赖Infrastructure Service |

---

## 🎯 总结：三句话记住服务划分

1. **Infrastructure（技术服务）**：提供技术能力，不管业务
2. **Domain（领域服务）**：实现业务规则，不管技术细节
3. **Application（应用服务）**：编排业务流程，不实现具体逻辑

---

**当你不确定时，问自己：**
> "这段代码的修改理由是什么？"
> 
> - 如果是技术变化（协议、数据库） → Infrastructure
> - 如果是业务规则变化 → Domain
> - 如果是流程变化 → Application

**祝你写出优雅的代码！** 🚀💯

