# BaiShengVx3Plus 架构设计文档

## 🏗️ 总体架构

```
┌─────────────────────────────────────────────────────────────┐
│                         Presentation Layer                   │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐   │
│  │  LoginForm   │  │   VxMain     │  │  Other Views    │   │
│  └──────┬───────┘  └──────┬───────┘  └────────┬────────┘   │
│         │                 │                    │             │
│         └─────────────────┼────────────────────┘             │
│                           │                                  │
├───────────────────────────┼──────────────────────────────────┤
│                     ViewModel Layer                          │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐   │
│  │ LoginViewModel│  │VxMainViewModel│  │ Other VMs      │   │
│  └──────┬───────┘  └──────┬───────┘  └────────┬────────┘   │
│         │                 │                    │             │
│         └─────────────────┼────────────────────┘             │
│                           │                                  │
├───────────────────────────┼──────────────────────────────────┤
│                      Service Layer                           │
│  ┌────────────┐  ┌──────────────┐  ┌───────────────────┐   │
│  │   Auth     │  │   Lottery    │  │    Browser        │   │
│  │  Service   │  │   Service    │  │    Service        │   │
│  └────────────┘  └──────────────┘  └───────────────────┘   │
│  ┌────────────┐  ┌──────────────┐  ┌───────────────────┐   │
│  │   Bet      │  │  Settlement  │  │    Message        │   │
│  │  Service   │  │   Service    │  │    Service        │   │
│  └────────────┘  └──────────────┘  └───────────────────┘   │
├──────────────────────────────────────────────────────────────┤
│                      Data Layer                              │
│  ┌────────────┐  ┌──────────────┐  ┌───────────────────┐   │
│  │ Repository │  │  SQLite DB   │  │   BindingList     │   │
│  │   Layer    │  │   Context    │  │   (Auto-Save)     │   │
│  └────────────┘  └──────────────┘  └───────────────────┘   │
└──────────────────────────────────────────────────────────────┘
```

## 📦 项目结构

```
BaiShengVx3Plus/
├── Models/                        # 数据模型层
│   ├── Member.cs                  # 会员模型
│   ├── MemberOrder.cs             # 订单模型
│   ├── LotteryData.cs             # 开奖数据模型
│   ├── BetContent.cs              # 投注内容模型
│   ├── Enums.cs                   # 枚举定义
│   └── Interfaces/                # 模型接口
│       └── INotifyEntity.cs       # 通知接口
│
├── Data/                          # 数据访问层
│   ├── Repositories/              # 仓储模式
│   │   ├── IRepository.cs         # 仓储接口
│   │   ├── MemberRepository.cs    # 会员仓储
│   │   └── OrderRepository.cs     # 订单仓储
│   ├── SqliteContext.cs           # SQLite上下文
│   └── BindingLists/              # 修改即保存的BindingList
│       ├── AutoSaveBindingList.cs # 基础自动保存列表
│       ├── MemberBindingList.cs   # 会员绑定列表
│       └── OrderBindingList.cs    # 订单绑定列表
│
├── Services/                      # 服务层
│   ├── Interfaces/                # 服务接口
│   │   ├── ILotteryService.cs     # 开奖服务接口
│   │   ├── IBetService.cs         # 投注服务接口
│   │   ├── ISettlementService.cs  # 结算服务接口
│   │   ├── IBrowserService.cs     # 浏览器服务接口
│   │   └── IMessageService.cs     # 消息服务接口
│   ├── LotteryService.cs          # 开奖服务实现
│   ├── BetService.cs              # 投注服务实现
│   ├── SettlementService.cs       # 结算服务实现
│   ├── BrowserService.cs          # 浏览器服务实现
│   └── MessageService.cs          # 消息服务实现（预留Socket）
│
├── ViewModels/                    # 视图模型层
│   ├── VxMainViewModel.cs         # 主界面VM
│   ├── LoginViewModel.cs          # 登录VM
│   └── ...
│
├── Views/                         # 视图层
│   ├── VxMain.cs                  # 主界面（重新设计）
│   ├── LoginForm.cs               # 登录窗体
│   └── ...
│
├── Core/                          # 核心基础设施
│   ├── ViewModelBase.cs
│   ├── RelayCommand.cs
│   └── ServiceLocator.cs
│
└── Config/                        # 配置文件
    └── AppSettings.cs
```

## 🎯 核心功能设计

### 1. 修改即保存机制

**原理：**
- 使用 `INotifyPropertyChanged` 监听属性变化
- `BindingList<T>` 监听集合项的 `PropertyChanged` 事件
- 事件触发时立即写入SQLite数据库
- 使用事务确保数据一致性

**实现流程：**
```
用户修改 → PropertyChanged事件 → 自动触发保存 → SQLite更新 → Commit
```

### 2. 服务层接口设计

#### 2.1 开奖服务 (ILotteryService)
```csharp
interface ILotteryService
{
    Task<LotteryData> GetLatestLotteryAsync();
    Task<LotteryData> GetLotteryByIssueAsync(string issueId);
    Task<bool> UpdateLotteryDataAsync(LotteryData data);
    event EventHandler<LotteryData> LotteryOpened;
}
```

#### 2.2 投注服务 (IBetService)
```csharp
interface IBetService
{
    Task<BetResult> PlaceBetAsync(MemberOrder order);
    Task<List<MemberOrder>> GetPendingBetsAsync();
    Task<bool> CancelBetAsync(string orderId);
    decimal CalculateBetAmount(string betContent);
}
```

#### 2.3 结算服务 (ISettlementService)
```csharp
interface ISettlementService
{
    Task<SettlementResult> SettleOrderAsync(MemberOrder order, LotteryData lottery);
    Task<List<SettlementResult>> SettleAllPendingAsync(string issueId);
    decimal CalculateProfit(MemberOrder order, LotteryData lottery);
}
```

#### 2.4 浏览器服务 (IBrowserService)
```csharp
interface IBrowserService
{
    Task LoginAsync(string url, string username, string password);
    Task NavigateAsync(string url);
    Task<string> ExecuteScriptAsync(string script);
    Task<BrowserState> GetStateAsync();
    event EventHandler<BrowserEvent> BrowserEvent;
}
```

#### 2.5 消息服务 (IMessageService) - 预留Socket
```csharp
interface IMessageService
{
    Task ConnectAsync(string serverUrl);
    Task DisconnectAsync();
    Task SendMessageAsync(Message message);
    event EventHandler<Message> MessageReceived;
    bool IsConnected { get; }
}
```

### 3. 数据模型设计

#### Member (会员)
- 基础信息：wxid, account, nickname, avatar
- 状态：State (会员/托/管理)
- 余额：Balance
- 统计：今日/总 的 上分/下分/投注/盈亏

#### MemberOrder (订单)
- 会员信息：wxid, nickname
- 期号：IssueId
- 投注内容：BetContentOriginal, BetContentStandard
- 金额：AmountTotal, BetFronMoney, BetAfterMoney
- 结果：Profit, NetProfit, Odds
- 状态：OrderStatus, OrderType

## 🔥 关键技术点

### 1. 修改即保存的实现

```csharp
public class AutoSaveBindingList<T> : BindingList<T> 
    where T : INotifyPropertyChanged
{
    private readonly IRepository<T> _repository;
    
    protected override void OnListChanged(ListChangedEventArgs e)
    {
        base.OnListChanged(e);
        
        if (e.ListChangedType == ListChangedType.ItemChanged)
        {
            var item = this[e.NewIndex];
            // 立即保存到数据库
            _repository.UpdateAsync(item).Wait();
        }
    }
    
    protected override void InsertItem(int index, T item)
    {
        item.PropertyChanged += Item_PropertyChanged;
        base.InsertItem(index, item);
        _repository.InsertAsync(item).Wait();
    }
    
    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var item = (T)sender;
        // 属性修改立即保存
        _repository.UpdateAsync(item).Wait();
    }
}
```

### 2. DataGridView 绑定

```csharp
// 设置数据源
dgvMembers.DataSource = memberBindingList;

// 配置列
dgvMembers.AutoGenerateColumns = false;
dgvMembers.ReadOnly = true;  // UI只读
dgvMembers.AllowUserToAddRows = false;

// 通过ViewModel修改数据
viewModel.UpdateMemberBalance(member, newBalance);
// 自动触发保存，自动刷新UI
```

### 3. MVVM + 修改即保存

```csharp
// ViewModel
public void UpdateMember(Member member, Action<Member> updateAction)
{
    updateAction(member);
    // member的属性改变会自动触发：
    // PropertyChanged → BindingList → 自动保存 → UI更新
}
```

## 📊 数据库设计

### 表结构

#### Members 表
```sql
CREATE TABLE Members (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    GroupWxId TEXT,
    wxid TEXT UNIQUE,
    account TEXT,
    nickname TEXT,
    display_name TEXT,
    Balance REAL DEFAULT 0,
    State INTEGER DEFAULT 0,
    BetToday REAL DEFAULT 0,
    BetTotal REAL DEFAULT 0,
    IncomeToday REAL DEFAULT 0,
    IncomeTotal REAL DEFAULT 0,
    CreditToday REAL DEFAULT 0,
    CreditTotal REAL DEFAULT 0,
    WithdrawToday REAL DEFAULT 0,
    WithdrawTotal REAL DEFAULT 0
);
```

#### Orders 表
```sql
CREATE TABLE Orders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    GroupWxId TEXT,
    wxid TEXT,
    nickname TEXT,
    IssueId TEXT,
    BetContentOriginal TEXT,
    BetContentStandard TEXT,
    AmountTotal REAL,
    Nums INTEGER,
    Profit REAL DEFAULT 0,
    NetProfit REAL DEFAULT 0,
    Odds REAL DEFAULT 1.97,
    OrderStatus INTEGER DEFAULT 0,
    OrderType INTEGER DEFAULT 0,
    TimeStampBet INTEGER,
    TimeString TEXT
);
```

## 🎨 UI 设计（参考F5BotV2）

### 主界面布局 (980 x 762)

```
┌────────────────────────────────────────────────────────────┐
│  百胜VX3Plus                                         [_][□][X]│
├────────────────────────────────────────────────────────────┤
│  [开奖结果] [当期数据] [历史数据] [盘口管理]                 │
├──────┬─────────────────────────────────────────────────────┤
│ 联系 │  ┌─会员列表────────────────────────────────────┐    │
│ 人   │  │ ID │ 昵称 │ 状态 │ 余额 │ 今日盈亏 │ 总盈亏 │    │
│ /    │  ├────┼──────┼──────┼──────┼──────────┼────────┤    │
│ 群   │  │ 1  │ 张三 │ 会员 │ 100  │ -20      │ 50     │    │
│      │  │ 2  │ 李四 │ 会员 │ 200  │ 30       │ 100    │    │
│ [刷新]│  └─────────────────────────────────────────────┘    │
│      │  ┌─订单列表────────────────────────────────────┐    │
│      │  │期号│昵称│注前│注后│数量│盈利│总额│状态│时间│    │
│      │  ├────┼────┼────┼────┼────┼────┼────┼────┼────┤    │
│      │  │001 │张三│100 │80  │2   │0   │20  │待结│14:20│   │
│      │  └─────────────────────────────────────────────┘    │
├──────┴─────────────────────────────────────────────────────┤
│ 状态: 就绪 | 在线用户: 10 | 待结算: 5 | 余额总计: 1000     │
└────────────────────────────────────────────────────────────┘
```

## 🚀 实现路线图

### Phase 1: 基础架构 (Current)
- ✅ 创建数据模型
- ✅ 创建修改即保存的BindingList
- ✅ 创建Repository层
- ✅ 创建服务接口

### Phase 2: 核心功能
- 实现开奖服务
- 实现投注服务
- 实现结算服务
- 实现浏览器服务

### Phase 3: UI重构
- 重新设计VxMain界面
- 实现DataGridView修改即保存
- 绑定ViewModel

### Phase 4: 高级功能
- 实现消息服务（Socket）
- 添加日志系统
- 添加配置管理
- 性能优化

## 📝 开发规范

### 命名约定
- 模型：PascalCase (Member, MemberOrder)
- 服务：I前缀接口 + Service后缀 (ILotteryService)
- ViewModel：ViewModel后缀 (VxMainViewModel)
- 私有字段：_camelCase (_repository)

### 异步规范
- 所有IO操作使用async/await
- 方法名以Async结尾
- 避免 .Result 和 .Wait()

### 数据库操作
- 使用事务确保一致性
- 立即提交（修改即保存）
- 异常处理和日志记录

## 🔒 安全考虑

1. **数据验证**：所有输入数据进行验证
2. **SQL注入防护**：使用参数化查询
3. **并发控制**：SQLite使用事务锁
4. **错误处理**：完善的异常捕获和日志

---

📅 创建日期: 2024-11-04
🔧 技术栈: .NET 8.0 + SunnyUI + SQLite + MVVM
✅ 状态: 设计完成，开发中

