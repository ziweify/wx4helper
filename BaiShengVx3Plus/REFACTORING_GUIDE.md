# 🚀 BaiShengVx3Plus 重构实施指南

## 📋 项目概述

基于 F5BotV2 的成熟架构，重构为现代化的 .NET 8.0 + MVVM + SunnyUI 应用程序。

**核心特性：**
- ✅ 修改即保存（无延时，无缓存，立即写入SQLite）
- ✅ MVVM架构 + 依赖注入
- ✅ 服务化设计（开奖、结算、投注、浏览器、消息）
- ✅ 预留Socket消息框架
- ✅ 现代化UI（参考F5BotV2布局）

## 📐 已完成文件列表

### ✅ 架构文档
- `ARCHITECTURE.md` - 完整架构设计文档
- `REFACTORING_GUIDE.md` - 本文档

### ✅ 核心模型
- `Models/Enums.cs` - 所有枚举定义

### 🔄 待创建文件

## 🎯 Phase 1: 数据模型层（优先级：最高）

### 1.1 创建 `Models/Member.cs`

```csharp
using System.ComponentModel;
using System.Linq.Expressions;
using SQLite;

namespace BaiShengVx3Plus.Models
{
    public class Member : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        private void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (PropertyChanged == null) return;
            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null) return;
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _groupWxId = string.Empty;
        public string GroupWxId
        {
            get => _groupWxId;
            set { _groupWxId = value; NotifyPropertyChanged(() => GroupWxId); }
        }

        private string _wxid = string.Empty;
        public string Wxid
        {
            get => _wxid;
            set { _wxid = value; NotifyPropertyChanged(() => Wxid); }
        }

        private string _account = string.Empty;
        public string Account
        {
            get => _account;
            set { _account = value; NotifyPropertyChanged(() => Account); }
        }

        private string _nickname = string.Empty;
        public string Nickname
        {
            get => _nickname;
            set { _nickname = value; NotifyPropertyChanged(() => Nickname); }
        }

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; NotifyPropertyChanged(() => DisplayName); }
        }

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set { _balance = value; NotifyPropertyChanged(() => Balance); }
        }

        private MemberState _state;
        public MemberState State
        {
            get => _state;
            set { _state = value; NotifyPropertyChanged(() => State); }
        }

        private decimal _incomeToday;
        public decimal IncomeToday
        {
            get => _incomeToday;
            set { _incomeToday = value; NotifyPropertyChanged(() => IncomeToday); }
        }

        private decimal _incomeTotal;
        public decimal IncomeTotal
        {
            get => _incomeTotal;
            set { _incomeTotal = value; NotifyPropertyChanged(() => IncomeTotal); }
        }

        // ... 其他财务字段（参考F5BotV2的V2Member）
    }
}
```

### 1.2 创建 `Models/MemberOrder.cs`

```csharp
// 参考 F5BotV2 的 V2MemberOrder.cs
// 包含所有订单字段：IssueId, BetContent, AmountTotal, Profit等
```

### 1.3 创建 `Models/LotteryData.cs`

```csharp
public class LotteryData
{
    public int Id { get; set; }
    public string IssueId { get; set; }
    public string Numbers { get; set; }  // 开奖号码
    public DateTime OpenTime { get; set; }
    public string Platform { get; set; }
}
```

## 🎯 Phase 2: 修改即保存基础设施（优先级：最高）

### 2.1 创建 `Data/AutoSaveBindingList.cs`

```csharp
using System.ComponentModel;
using SQLite;

namespace BaiShengVx3Plus.Data
{
    public class AutoSaveBindingList<T> : BindingList<T> where T : class, INotifyPropertyChanged, new()
    {
        private readonly SQLiteConnection _connection;
        private readonly string _tableName;

        public AutoSaveBindingList(SQLiteConnection connection)
        {
            _connection = connection;
            _tableName = typeof(T).Name;
            _connection.CreateTable<T>();
            LoadFromDatabase();
        }

        private void LoadFromDatabase()
        {
            var items = _connection.Table<T>().ToList();
            foreach (var item in items)
            {
                item.PropertyChanged += Item_PropertyChanged;
                base.Add(item);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            item.PropertyChanged += Item_PropertyChanged;
            base.InsertItem(index, item);
            
            // 立即保存到数据库
            try
            {
                _connection.Insert(item);
                _connection.Commit();
            }
            catch (Exception ex)
            {
                // 日志记录
                Console.WriteLine($"插入失败: {ex.Message}");
            }
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            item.PropertyChanged -= Item_PropertyChanged;
            base.RemoveItem(index);
            
            // 从数据库删除
            try
            {
                _connection.Delete(item);
                _connection.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除失败: {ex.Message}");
            }
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null || string.IsNullOrEmpty(e.PropertyName)) return;
            
            var item = (T)sender;
            
            // 立即保存到数据库
            try
            {
                _connection.Update(item);
                _connection.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新失败: {ex.Message}");
            }
        }
    }
}
```

### 2.2 创建 `Data/SqliteContext.cs`

```csharp
using SQLite;

namespace BaiShengVx3Plus.Data
{
    public class SqliteContext : IDisposable
    {
        private readonly SQLiteConnection _connection;
        
        public SqliteContext(string dbPath)
        {
            _connection = new SQLiteConnection(dbPath);
            InitializeTables();
        }

        private void InitializeTables()
        {
            _connection.CreateTable<Member>();
            _connection.CreateTable<MemberOrder>();
            _connection.CreateTable<LotteryData>();
        }

        public SQLiteConnection Connection => _connection;

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
```

## 🎯 Phase 3: 服务层接口（优先级：高）

### 3.1 创建服务接口

在 `Services/Interfaces/` 目录下创建：

#### `ILotteryService.cs`
```csharp
public interface ILotteryService
{
    Task<LotteryData?> GetLatestLotteryAsync();
    Task<LotteryData?> GetLotteryByIssueAsync(string issueId);
    Task<bool> UpdateLotteryDataAsync(LotteryData data);
    event EventHandler<LotteryData>? LotteryOpened;
}
```

#### `IBetService.cs`
```csharp
public interface IBetService
{
    Task<bool> PlaceBetAsync(MemberOrder order);
    Task<List<MemberOrder>> GetPendingBetsAsync();
    Task<bool> CancelBetAsync(int orderId);
    decimal CalculateBetAmount(string betContent);
    Task<bool> ValidateBetContentAsync(string content);
}
```

#### `ISettlementService.cs`
```csharp
public interface ISettlementService
{
    Task<decimal> SettleOrderAsync(MemberOrder order, LotteryData lottery);
    Task<List<SettlementResult>> SettleAllPendingAsync(string issueId);
    decimal CalculateProfit(MemberOrder order, LotteryData lottery, decimal odds);
}
```

#### `IBrowserService.cs`
```csharp
public interface IBrowserService
{
    Task<bool> InitializeAsync();
    Task<bool> LoginAsync(string url, string username, string password);
    Task NavigateAsync(string url);
    Task<string> ExecuteScriptAsync(string script);
    Task<BrowserState> GetStateAsync();
    Task<string> GetBalanceAsync();
    event EventHandler<BrowserEvent>? BrowserEvent;
}
```

#### `IMessageService.cs` (预留Socket)
```csharp
public interface IMessageService
{
    Task<bool> ConnectAsync(string serverUrl, int port);
    Task DisconnectAsync();
    Task<bool> SendMessageAsync(Message message);
    event EventHandler<Message>? MessageReceived;
    bool IsConnected { get; }
}
```

## 🎯 Phase 4: 重新设计 VxMain 界面

### 4.1 参考 F5BotV2 的布局

**关键DataGridView字段对照表：**

#### 会员表 (dgv_members)
| F5BotV2字段 | BaiShengVx3Plus字段 | 说明 |
|------------|-------------------|------|
| id | Id | 自增ID |
| wxid | Wxid | 微信ID |
| account | Account | 账号 |
| nickname | Nickname | 昵称 |
| display_name | DisplayName | 群昵称 |
| Balance | Balance | 余额 |
| State | State | 状态 |
| BetCur | BetCur | 本期下注 |
| BetToday | BetToday | 今日下注 |
| BetTotal | BetTotal | 总下注 |
| IncomeToday | IncomeToday | 今日盈亏 |
| IncomeTotal | IncomeTotal | 总盈亏 |
| CreditToday | CreditToday | 今日上分 |
| CreditTotal | CreditTotal | 总上分 |
| WithdrawToday | WithdrawToday | 今日下分 |
| WithdrawTotal | WithdrawTotal | 总下分 |

#### 订单表 (dgv_orders)
| F5BotV2字段 | BaiShengVx3Plus字段 | 说明 |
|------------|-------------------|------|
| id | Id | 自增ID |
| IssueId | IssueId | 期号 |
| nickname | Nickname | 昵称 |
| BetContentOriginal | BetContentOriginal | 原始内容 |
| BetContentStandar | BetContentStandard | 标准内容 |
| BetFronMoney | BetFronMoney | 注前金额 |
| BetAfterMoney | BetAfterMoney | 注后金额 |
| Nums | Nums | 数量 |
| AmountTotal | AmountTotal | 总金额 |
| Profit | Profit | 盈利 |
| NetProfit | NetProfit | 纯利 |
| OrderStatus | OrderStatus | 状态 |
| OrderType | OrderType | 类型 |
| TimeString | TimeString | 日期时间 |

### 4.2 VxMain.Designer.cs 布局

```csharp
// 主分割容器
private Sunny.UI.UISplitContainer splitMain;

// 左侧面板 - 联系人/群列表
private Sunny.UI.UIDataGridView dgvContacts;

// 右侧面板 - 上下分割
private Sunny.UI.UISplitContainer splitRight;

// 上半部分 - 会员列表
private Sunny.UI.UIDataGridView dgvMembers;

// 下半部分 - 订单列表
private Sunny.UI.UIDataGridView dgvOrders;

// 功能按钮区
private Sunny.UI.UIButton btnStart;
private Sunny.UI.UIButton btnStop;
private Sunny.UI.UIButton btnRefresh;
private Sunny.UI.UIButton btnClearData;
private Sunny.UI.UIButton btnOpenLottery;
private Sunny.UI.UIButton btnShowBrowser;

// 开奖数据显示区
private Sunny.UI.UIPanel pnlLotteryData;
```

## 📝 实施步骤

### Step 1: 创建数据模型 (今天完成)
1. ✅ 创建 `Models/Enums.cs`
2. ⏳ 创建 `Models/Member.cs`
3. ⏳ 创建 `Models/MemberOrder.cs`
4. ⏳ 创建 `Models/LotteryData.cs`

### Step 2: 创建数据层 (明天完成)
1. 创建 `Data/AutoSaveBindingList.cs`
2. 创建 `Data/SqliteContext.cs`
3. 创建 `Data/MemberBindingList.cs`
4. 创建 `Data/OrderBindingList.cs`

### Step 3: 创建服务接口 (后天完成)
1. 创建所有服务接口
2. 创建基础服务实现
3. 测试服务功能

### Step 4: 重构UI (第4天完成)
1. 修改 VxMain.Designer.cs
2. 绑定DataGridView
3. 测试修改即保存

### Step 5: 实现业务逻辑 (第5-7天)
1. 实现开奖服务
2. 实现投注服务
3. 实现结算服务
4. 实现浏览器服务

## 🔍 关键代码片段

### 修改即保存的核心逻辑

```csharp
// 在 VxMainViewModel 中
public class VxMainViewModel : ViewModelBase
{
    private readonly AutoSaveBindingList<Member> _members;
    private readonly AutoSaveBindingList<MemberOrder> _orders;

    public VxMainViewModel(SqliteContext context)
    {
        _members = new AutoSaveBindingList<Member>(context.Connection);
        _orders = new AutoSaveBindingList<MemberOrder>(context.Connection);
    }

    public IBindingList Members => _members;
    public IBindingList Orders => _orders;

    // 修改会员余额 - 自动保存
    public void UpdateMemberBalance(Member member, decimal newBalance)
    {
        member.Balance = newBalance;
        // 属性改变 → PropertyChanged事件 → 自动保存到数据库 → UI自动刷新
    }
}
```

### DataGridView 绑定示例

```csharp
// 在 VxMain.cs 中
private void InitializeDataGridViews()
{
    // 绑定会员列表
    dgvMembers.DataSource = _viewModel.Members;
    dgvMembers.AutoGenerateColumns = false;
    dgvMembers.ReadOnly = true;  // UI只读，通过ViewModel修改

    // 配置列
    dgvMembers.Columns["Id"].Width = 45;
    dgvMembers.Columns["Wxid"].Visible = false;
    dgvMembers.Columns["Balance"].DefaultCellStyle.Format = "0.00";
    dgvMembers.Columns["IncomeToday"].DefaultCellStyle.Format = "0.00";
    
    // 绑定订单列表
    dgvOrders.DataSource = _viewModel.Orders;
    dgvOrders.AutoGenerateColumns = false;
    dgvOrders.ReadOnly = true;
}
```

## 🎓 重要提示

### ✅ DO (推荐做法)
1. **立即保存**: 使用事件监听，属性改变立即保存
2. **只读UI**: DataGridView设为ReadOnly，通过ViewModel修改
3. **事务控制**: 每次保存后立即Commit
4. **异常处理**: 捕获所有数据库操作异常
5. **日志记录**: 记录所有保存操作

### ❌ DON'T (避免做法)
1. **不要延时保存**: 不使用Timer或批量保存
2. **不要缓存**: 不在内存中缓存，立即写入数据库
3. **不要可编辑UI**: 不让用户直接在DataGridView中编辑
4. **不要忽略异常**: 不忽略保存失败的情况
5. **不要阻塞UI**: 保存操作尽量快速

## 📦 NuGet 包依赖

```xml
<ItemGroup>
  <PackageReference Include="SunnyUI" Version="3.6.9" />
  <PackageReference Include="sqlite-net-pcl" Version="1.8.116" />
  <PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.6" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
  <PackageReference Include="CefSharp.WinForms" Version="120.1.110" />
</ItemGroup>
```

## 🔗 参考资源

- F5BotV2源码：`D:\gitcode\wx4helper\F5BotV2\`
- 架构文档：`ARCHITECTURE.md`
- SunnyUI文档：https://gitee.com/yhuse/SunnyUI
- SQLite.NET文档：https://github.com/praeclarum/sqlite-net

## 📞 后续支持

如果需要帮助，可以参考：
1. 查看 F5BotV2 对应功能的实现
2. 查阅 ARCHITECTURE.md 文档
3. 运行测试确保功能正常

---

📅 创建日期: 2024-11-04  
✅ 状态: 实施指南完成  
🎯 目标: 7天内完成基础架构

