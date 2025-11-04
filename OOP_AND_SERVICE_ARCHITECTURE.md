# 面向对象编程 vs 服务架构

## 🎯 核心观点：服务就是面向对象！

**服务（Service）本身就是一个对象**，只是它遵循了更好的面向对象设计原则。

---

## 📚 理论基础

### 1. 什么是面向对象编程（OOP）？

面向对象的**三大特征**：
1. **封装（Encapsulation）** - 隐藏内部实现细节
2. **继承（Inheritance）** - 代码复用和扩展
3. **多态（Polymorphism）** - 同一接口，不同实现

面向对象的**五大原则**（SOLID）：
1. **单一职责原则（SRP）** - 一个类只负责一件事
2. **开放封闭原则（OCP）** - 对扩展开放，对修改关闭
3. **里氏替换原则（LSP）** - 子类可以替换父类
4. **接口隔离原则（ISP）** - 接口应该小而精
5. **依赖倒置原则（DIP）** - 依赖抽象，不依赖具体

### 2. 传统 OOP 的问题

```csharp
// ❌ 传统做法：上帝类（违反单一职责）
public class WeChatManager
{
    // 混合了太多职责
    private List<Process> processes;
    private Database db;
    private Socket socket;
    
    public void LaunchWeChat() { }      // 进程管理
    public void SaveToDb() { }          // 数据持久化
    public void SendMessage() { }       // 网络通信
    public void Log() { }               // 日志记录
    public void ValidateInput() { }     // 验证
    public void EncryptData() { }       // 加密
}

问题：
1. 职责不清晰
2. 难以测试（需要Database、Socket等依赖）
3. 难以维护（修改一个功能可能影响其他功能）
4. 难以复用（无法单独使用某个功能）
```

### 3. 现代 OOP：服务架构

```csharp
// ✅ 现代做法：职责分离（符合单一职责）

// 每个服务都是一个对象，只负责一件事
public class WeChatLoaderService      // 进程管理
{
    private Dictionary<uint, Process> _processes;  // 对象状态
    
    public void Launch() { }
    public void Inject() { }
}

public class WeChatDataService        // 数据持久化
{
    private IDatabase _db;
    
    public void Save() { }
    public void Load() { }
}

public class WeChatMessageService     // 网络通信
{
    private ISocket _socket;
    
    public void Send() { }
    public void Receive() { }
}

优势：
1. 职责清晰（单一职责）
2. 易于测试（可以Mock依赖）
3. 易于维护（修改一个服务不影响其他）
4. 易于复用（可以单独使用）
```

---

## 💡 服务中如何保存状态？

### 方式 1：使用私有字段（推荐）

```csharp
public class WeChatLoaderService
{
    // ========================================
    // 对象的状态（私有字段）
    // ========================================
    
    // 1. 集合状态
    private readonly ConcurrentDictionary<uint, WeChatProcess> _managedProcesses;
    
    // 2. 配置状态
    private readonly WeChatLoaderConfig _config;
    
    // 3. 运行时状态
    private DateTime _lastLaunchTime;
    private int _totalLaunchCount;
    
    // ========================================
    // 构造函数：初始化状态
    // ========================================
    
    public WeChatLoaderService()
    {
        _managedProcesses = new ConcurrentDictionary<uint, WeChatProcess>();
        _config = new WeChatLoaderConfig();
        _lastLaunchTime = DateTime.MinValue;
        _totalLaunchCount = 0;
    }
    
    // ========================================
    // 对象的行为（公共方法）
    // ========================================
    
    public bool LaunchWeChat(string dllPath, out string error)
    {
        // 修改状态
        _totalLaunchCount++;
        _lastLaunchTime = DateTime.Now;
        
        // 执行业务逻辑
        var result = LoaderNative.Launch(...);
        
        if (result)
        {
            // 保存新进程
            var process = new WeChatProcess(pid);
            _managedProcesses.TryAdd(pid, process);
        }
        
        return result;
    }
    
    // ========================================
    // 状态查询（读取状态的方法）
    // ========================================
    
    public IReadOnlyCollection<WeChatProcess> GetManagedProcesses()
    {
        return _managedProcesses.Values.ToList();
    }
    
    public int GetTotalLaunchCount()
    {
        return _totalLaunchCount;
    }
    
    public DateTime GetLastLaunchTime()
    {
        return _lastLaunchTime;
    }
}
```

### 方式 2：使用领域模型（Domain Model）

```csharp
// 领域模型：封装业务逻辑和数据
public class WeChatProcess
{
    // 数据
    public uint ProcessId { get; set; }
    public bool IsInjected { get; set; }
    public DateTime StartedAt { get; set; }
    
    // 行为
    public void MarkAsInjected(string dllPath)
    {
        IsInjected = true;
        InjectedAt = DateTime.Now;
    }
    
    public bool IsHeartbeatTimeout(int seconds)
    {
        return (DateTime.Now - LastHeartbeat).TotalSeconds > seconds;
    }
}

// 服务：管理领域模型
public class WeChatLoaderService
{
    private readonly Dictionary<uint, WeChatProcess> _processes;
    
    public void Launch()
    {
        var process = new WeChatProcess(pid);  // 创建领域对象
        _processes.Add(pid, process);          // 保存到服务状态
    }
    
    public WeChatProcess GetProcess(uint pid)
    {
        return _processes[pid];                // 返回领域对象
    }
}
```

---

## 🏗️ 完整架构示例

### 架构层次

```
┌─────────────────────────────────────────┐
│         UI Layer (表现层)                │
│  VxMain, LoginForm, Controls            │
│  - 处理用户交互                           │
│  - 数据绑定                               │
│  - 显示逻辑                               │
└──────────────┬──────────────────────────┘
               │ 依赖注入
               ▼
┌─────────────────────────────────────────┐
│      Application Layer (应用层)          │
│  ViewModels, Commands                    │
│  - 协调服务                               │
│  - 处理用户用例                           │
│  - 数据转换                               │
└──────────────┬──────────────────────────┘
               │ 调用
               ▼
┌─────────────────────────────────────────┐
│       Domain Layer (领域层)              │
│  Services, Models, Business Logic        │
│  - 核心业务逻辑                           │
│  - 领域对象                               │
│  - 业务规则                               │
└──────────────┬──────────────────────────┘
               │ 使用
               ▼
┌─────────────────────────────────────────┐
│   Infrastructure Layer (基础设施层)      │
│  Database, File System, Native APIs      │
│  - 数据持久化                             │
│  - 外部API调用                            │
│  - 系统资源访问                           │
└─────────────────────────────────────────┘
```

### 实际代码示例

```csharp
// ========================================
// 领域层：核心业务对象
// ========================================

// 领域模型
public class WeChatProcess
{
    public uint ProcessId { get; set; }
    public bool IsInjected { get; private set; }
    
    public void MarkAsInjected(string dllPath)
    {
        IsInjected = true;
        // 业务规则：记录注入时间
        InjectedAt = DateTime.Now;
    }
}

// 领域服务
public class WeChatLoaderService
{
    // 状态：管理的进程
    private readonly Dictionary<uint, WeChatProcess> _processes;
    
    // 依赖：基础设施
    private readonly IProcessRepository _repository;
    
    public WeChatLoaderService(IProcessRepository repository)
    {
        _processes = new Dictionary<uint, WeChatProcess>();
        _repository = repository;
    }
    
    // 业务逻辑：启动并注入
    public bool LaunchAndInject(string dllPath, out string error)
    {
        // 1. 调用Native API（基础设施层）
        var result = LoaderNative.Launch(dllPath, ...);
        
        if (result)
        {
            // 2. 创建领域对象
            var process = new WeChatProcess(pid);
            process.MarkAsInjected(dllPath);
            
            // 3. 保存到内存状态
            _processes.Add(pid, process);
            
            // 4. 持久化（基础设施层）
            _repository.Save(process);
        }
        
        return result;
    }
    
    // 查询：获取进程信息
    public WeChatProcess? GetProcess(uint pid)
    {
        return _processes.GetValueOrDefault(pid);
    }
}

// ========================================
// 应用层：协调多个服务
// ========================================

public class VxMainViewModel
{
    private readonly WeChatLoaderService _loaderService;
    private readonly IContactBindingService _contactService;
    
    public VxMainViewModel(
        WeChatLoaderService loaderService,
        IContactBindingService contactService)
    {
        _loaderService = loaderService;
        _contactService = contactService;
    }
    
    // 用例：启动微信并获取联系人
    public async Task LaunchAndLoadContacts()
    {
        // 1. 启动微信
        var success = _loaderService.LaunchAndInject("WeixinX.dll", out var error);
        
        if (success)
        {
            // 2. 等待微信启动
            await Task.Delay(2000);
            
            // 3. 获取联系人
            var contacts = await _contactService.GetContactsAsync();
            
            // 4. 更新UI
            Contacts = new ObservableCollection<Contact>(contacts);
        }
    }
}

// ========================================
// 表现层：UI
// ========================================

public partial class VxMain : Form
{
    private readonly VxMainViewModel _viewModel;
    
    public VxMain(VxMainViewModel viewModel)
    {
        _viewModel = viewModel;
    }
    
    private async void btnGetContactList_Click(object sender, EventArgs e)
    {
        // 调用应用层
        await _viewModel.LaunchAndLoadContacts();
    }
}
```

---

## 🔍 对比：传统 vs 现代

### 传统方式（耦合的对象）

```csharp
public class WeChatManager
{
    private Database db = new Database();        // 紧耦合
    private Logger logger = new Logger();        // 紧耦合
    
    public void Launch()
    {
        logger.Log("Starting...");
        var result = Native.Launch();
        db.Save(result);  // 直接依赖具体类
    }
}

// 使用
var manager = new WeChatManager();  // 无法控制依赖
manager.Launch();

// 测试困难
[Test]
public void TestLaunch()
{
    var manager = new WeChatManager();  // 必须使用真实的Database和Logger
    manager.Launch();                    // 难以隔离测试
}
```

### 现代方式（解耦的服务）

```csharp
public class WeChatLoaderService
{
    private readonly IDatabase _db;           // 依赖抽象
    private readonly ILogger _logger;         // 依赖抽象
    
    public WeChatLoaderService(IDatabase db, ILogger logger)
    {
        _db = db;
        _logger = logger;
    }
    
    public void Launch()
    {
        _logger.Log("Starting...");
        var result = Native.Launch();
        _db.Save(result);  // 通过接口调用
    }
}

// 使用（依赖注入）
services.AddSingleton<IDatabase, SqliteDatabase>();
services.AddSingleton<ILogger, FileLogger>();
services.AddSingleton<WeChatLoaderService>();

var service = serviceProvider.GetService<WeChatLoaderService>();
service.Launch();

// 测试简单
[Test]
public void TestLaunch()
{
    var mockDb = new MockDatabase();       // Mock依赖
    var mockLogger = new MockLogger();      // Mock依赖
    var service = new WeChatLoaderService(mockDb, mockLogger);
    
    service.Launch();                       // 隔离测试
    
    Assert.IsTrue(mockLogger.WasCalled);
}
```

---

## 🎯 回答你的问题

### Q1: 用服务没有了面向对象的感觉？

**A:** 恰恰相反！服务是**更好的面向对象**。

- ✅ **封装**：服务隐藏了实现细节，只暴露接口
- ✅ **多态**：通过接口可以有多种实现
- ✅ **组合**：服务之间通过依赖注入组合
- ✅ **单一职责**：每个服务只做一件事
- ✅ **依赖倒置**：依赖抽象而不是具体类

### Q2: 现代化编程都是这样的吗？

**A:** 是的！现代编程强调：

1. **SOLID 原则** - 更好的面向对象设计
2. **依赖注入** - 解耦，易于测试
3. **关注点分离** - 每个类职责单一
4. **测试驱动** - 可测试性是首要考虑

### Q3: 服务中如何保存状态？

**A:** 多种方式：

```csharp
// 1. 私有字段（内存状态）
public class WeChatLoaderService
{
    private readonly Dictionary<uint, WeChatProcess> _processes;
    
    public WeChatProcess GetProcess(uint pid)
    {
        return _processes[pid];  // 从内存读取
    }
}

// 2. 持久化到数据库（永久状态）
public class WeChatDataService
{
    private readonly IDatabase _db;
    
    public void SaveProcess(WeChatProcess process)
    {
        _db.Save(process);  // 保存到数据库
    }
    
    public WeChatProcess LoadProcess(uint pid)
    {
        return _db.Load<WeChatProcess>(pid);  // 从数据库加载
    }
}

// 3. 缓存（临时状态）
public class WeChatCacheService
{
    private readonly IMemoryCache _cache;
    
    public void CacheProcess(WeChatProcess process)
    {
        _cache.Set($"process_{process.ProcessId}", process, TimeSpan.FromMinutes(10));
    }
}
```

### Q4: 以前保存在Loader对象，现在怎么办？

**A:** 现在更灵活：

```csharp
// 以前：所有东西都在一个类
public class Loader
{
    public List<Process> Processes { get; set; }  // 状态
    public Config Config { get; set; }            // 配置
    public Database Db { get; set; }              // 数据库
    
    public void Launch() { }
    public void Save() { }
    public void Load() { }
}

// 现在：职责分离，更清晰
public class WeChatLoaderService      // 负责启动和管理
{
    private Dictionary<uint, WeChatProcess> _processes;  // 内存状态
    
    public void Launch() { }
    public WeChatProcess GetProcess(uint pid) { }
}

public class WeChatDataService        // 负责持久化
{
    private IDatabase _db;
    
    public void SaveProcess(WeChatProcess process) { }
    public WeChatProcess LoadProcess(uint pid) { }
}

public class WeChatConfigService      // 负责配置
{
    private Config _config;
    
    public Config GetConfig() { }
    public void UpdateConfig(Config config) { }
}
```

---

## 📊 总结

| 特性 | 传统OOP | 现代OOP（服务） |
|-----|---------|----------------|
| 职责 | 混合多个职责 | 单一职责 |
| 耦合度 | 紧耦合 | 松耦合 |
| 可测试性 | 难以测试 | 易于测试 |
| 可维护性 | 修改影响大 | 修改影响小 |
| 可扩展性 | 难以扩展 | 易于扩展 |
| 状态管理 | 混在一起 | 分层清晰 |
| 依赖管理 | new 创建依赖 | 依赖注入 |

## 🚀 最佳实践

1. **服务应该有状态** - 不要害怕在服务中保存状态
2. **状态分层** - 内存状态（快速）+ 持久化状态（可靠）
3. **单一职责** - 每个服务只负责一件事
4. **依赖注入** - 通过构造函数注入依赖
5. **接口优先** - 先定义接口，再实现
6. **领域驱动** - 使用领域模型封装业务逻辑

**服务架构 = 更好的面向对象！** 🎉

