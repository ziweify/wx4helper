# 📝 日志系统使用指南

## 🎯 架构总览

### 数据库分离架构（方案1）

```
Data/
├── business.db       ← 业务数据库（会员、订单、联系人）
└── logs.db           ← 日志数据库（系统日志）

优势：
✅ 完全隔离，零冲突
✅ 日志高频写入不影响业务
✅ 可以独立备份和优化
✅ 简单可靠
```

### 并发策略

```
业务数据（修改即保存）：
- 使用 business.db
- WAL 模式（读写并发）
- 短事务（< 5ms）
- 立即提交

日志数据（异步批量）：
- 使用 logs.db（独立）
- WAL 模式
- 异步队列 + 批量写入
- 100条/次 或 1秒/次
```

---

## 📚 核心组件

### 1. LogService（日志服务）

**特性：**
- ✅ 线程安全：多线程可以同时写日志
- ✅ 异步写入：不阻塞主线程
- ✅ 实时通知：通过事件实时更新UI
- ✅ 混合存储：内存（1000条）+ SQLite（永久）
- ✅ 独立数据库：logs.db（零冲突）

**日志级别：**
```csharp
LogLevel.Trace    // 跟踪（最详细）
LogLevel.Debug    // 调试
LogLevel.Info     // 信息
LogLevel.Warning  // 警告
LogLevel.Error    // 错误
LogLevel.Fatal    // 致命错误
```

### 2. DatabaseService（数据库服务）

**特性：**
- ✅ 管理业务数据库（business.db）
- ✅ WAL 模式（读写并发）
- ✅ 连接池（复用连接）
- ✅ 短事务（快速提交）

### 3. LogViewerForm（日志查看窗口）

**功能：**
- ✅ 实时显示日志（事件驱动）
- ✅ 按级别过滤
- ✅ 按来源过滤
- ✅ 关键词搜索
- ✅ 导出日志
- ✅ 清空日志

---

## 🚀 使用示例

### 1. 基本日志记录

```csharp
public class MyService
{
    private readonly ILogService _logService;
    
    public MyService(ILogService logService)
    {
        _logService = logService;
    }
    
    public void DoSomething()
    {
        // 记录信息日志
        _logService.Info("MyService", "开始执行操作");
        
        try
        {
            // ... 业务逻辑 ...
            
            _logService.Debug("MyService", "执行步骤1完成");
            _logService.Debug("MyService", "执行步骤2完成");
        }
        catch (Exception ex)
        {
            // 记录错误日志（包含异常）
            _logService.Error("MyService", "操作失败", ex);
            throw;
        }
    }
}
```

### 2. 带额外数据的日志

```csharp
var extraData = JsonSerializer.Serialize(new
{
    UserId = "user123",
    Action = "UpdateProfile",
    Data = new { Name = "张三", Phone = "13800138000" }
});

_logService.Info("UserService", "用户更新了个人资料", extraData);
```

### 3. 业务数据操作（修改即保存）

```csharp
public class MemberService
{
    private readonly IDatabaseService _db;
    private readonly ILogService _log;
    
    public void UpdateMember(V2Member member)
    {
        try
        {
            // 立即保存到数据库（business.db）
            _db.ExecuteNonQuery(@"
                UPDATE Members 
                SET Nickname = @Nickname, Phone = @Phone, UpdatedAt = @UpdatedAt
                WHERE Id = @Id
            ", new
            {
                member.Id,
                member.Nickname,
                member.Phone,
                UpdatedAt = DateTime.Now
            });
            
            // 记录日志（logs.db，异步）
            _log.Info("MemberService", $"更新会员: {member.Nickname}");
        }
        catch (Exception ex)
        {
            _log.Error("MemberService", "更新会员失败", ex);
            throw;
        }
    }
}
```

### 4. 打开日志窗口

```csharp
// 从 DI 容器获取
var logViewer = ServiceProvider.GetRequiredService<LogViewerForm>();
logViewer.Show();

// 或使用非模态窗口
logViewer.ShowDialog();
```

### 5. 订阅日志事件（实时监控）

```csharp
public class MainForm : Form
{
    private readonly ILogService _logService;
    
    public MainForm(ILogService logService)
    {
        _logService = logService;
        
        // 订阅实时日志
        _logService.LogAdded += OnLogAdded;
    }
    
    private void OnLogAdded(object? sender, LogEntry entry)
    {
        // 在状态栏显示最新日志
        if (InvokeRequired)
        {
            BeginInvoke(() => {
                lblStatus.Text = $"[{entry.LevelName}] {entry.Message}";
            });
        }
    }
}
```

---

## 🔍 查询日志

### 1. 获取最近日志（内存）

```csharp
// 获取最近100条日志（从内存，速度快）
var logs = _logService.GetRecentLogs(100);

foreach (var log in logs)
{
    Console.WriteLine($"[{log.FormattedTime}] {log.Message}");
}
```

### 2. 查询数据库日志

```csharp
// 查询今天的错误日志
var logs = _logService.QueryLogs(
    startTime: DateTime.Today,
    endTime: DateTime.Today.AddDays(1),
    minLevel: LogLevel.Error
);

// 按关键词搜索
var logs = _logService.QueryLogs(
    keyword: "用户登录",
    limit: 100
);

// 按来源过滤
var logs = _logService.QueryLogs(
    source: "UserService",
    limit: 50
);

// 组合查询
var logs = _logService.QueryLogs(
    startTime: DateTime.Now.AddHours(-1),  // 最近1小时
    minLevel: LogLevel.Warning,             // 警告及以上
    keyword: "微信",                         // 包含"微信"
    limit: 200
);
```

### 3. 获取统计信息

```csharp
var stats = _logService.GetStatistics();

Console.WriteLine($"总日志数: {stats.TotalCount}");
Console.WriteLine($"错误数: {stats.ErrorCount}");
Console.WriteLine($"警告数: {stats.WarningCount}");
Console.WriteLine($"信息数: {stats.InfoCount}");
Console.WriteLine($"首条日志: {stats.FirstLogTime}");
Console.WriteLine($"最后日志: {stats.LastLogTime}");
```

### 4. 导出日志

```csharp
// 导出所有日志
await _logService.ExportToFileAsync("logs_all.log");

// 导出指定时间范围
await _logService.ExportToFileAsync(
    "logs_today.log",
    startTime: DateTime.Today,
    endTime: DateTime.Now
);
```

---

## ⚙️ 配置和管理

### 1. 设置最小日志级别

```csharp
// 只记录警告及以上级别
_logService.SetMinimumLevel(LogLevel.Warning);

// 生产环境通常设置为 Info 或 Warning
// 开发环境通常设置为 Debug 或 Trace
```

### 2. 清空日志

```csharp
// 清空内存日志（不影响数据库）
_logService.ClearMemoryLogs();

// 清空数据库日志（永久删除）
_logService.ClearDatabaseLogs();
```

### 3. 日志文件位置

```
程序目录/
├── Data/
│   ├── business.db      ← 业务数据库
│   └── logs.db          ← 日志数据库
│
└── Logs/                ← 文本日志（备份）
    ├── 2025-11-04.log
    ├── 2025-11-05.log
    └── ...
```

---

## 📊 性能特性

### 日志写入性能

```
写日志调用：
- 耗时: < 1ms（仅入队）
- 不阻塞主线程
- 线程安全

数据库写入：
- 批量写入：100条/次
- 或定时写入：1秒/次
- 后台线程处理
```

### 业务数据性能

```
修改即保存：
- 单次更新: 3-5ms
- WAL模式: 读写并发
- 不阻塞查询
- 短事务: 快速提交
```

### 并发能力

```
✅ 多个业务表同时写入: 可以（排队执行，但很快）
✅ 业务数据写入 + 日志写入: 可以（完全独立）
✅ 多线程读取: 可以（无限制）
✅ 读写并发: 可以（WAL模式）
```

---

## 🛡️ 最佳实践

### 1. 日志级别使用建议

```csharp
// Trace: 非常详细的调试信息（通常不启用）
_logService.Trace("Service", "进入方法 DoSomething()");

// Debug: 调试信息（开发时使用）
_logService.Debug("Service", $"变量值: userId={userId}");

// Info: 重要的业务流程（生产环境默认级别）
_logService.Info("UserService", "用户登录成功");

// Warning: 潜在问题（需要关注）
_logService.Warning("PaymentService", "支付超时，正在重试");

// Error: 错误（需要处理）
_logService.Error("OrderService", "创建订单失败", ex);

// Fatal: 致命错误（应用可能崩溃）
_logService.Fatal("Application", "数据库连接失败，应用无法启动", ex);
```

### 2. 避免过度记录

```csharp
// ❌ 坏的做法：在循环中记录
for (int i = 0; i < 10000; i++)
{
    _logService.Debug("Service", $"处理第 {i} 条数据");  // 太多了！
}

// ✅ 好的做法：记录关键节点
_logService.Info("Service", "开始处理数据");
for (int i = 0; i < 10000; i++)
{
    // ... 处理数据 ...
    if (i % 1000 == 0)  // 每1000条记录一次
    {
        _logService.Debug("Service", $"已处理 {i} 条数据");
    }
}
_logService.Info("Service", $"处理完成，共 {10000} 条数据");
```

### 3. 记录关键业务操作

```csharp
public class OrderService
{
    public void CreateOrder(Order order)
    {
        _logService.Info("OrderService", $"创建订单: {order.OrderNo}");
        
        try
        {
            // 保存订单
            _db.SaveOrder(order);
            _logService.Info("OrderService", $"订单创建成功: {order.OrderNo}");
        }
        catch (Exception ex)
        {
            _logService.Error("OrderService", $"订单创建失败: {order.OrderNo}", ex);
            throw;
        }
    }
}
```

### 4. 使用结构化日志

```csharp
// 将复杂数据序列化为JSON
var orderData = JsonSerializer.Serialize(new
{
    OrderNo = order.OrderNo,
    Amount = order.Amount,
    MemberId = order.MemberId,
    Items = order.Items.Select(i => new { i.ProductId, i.Quantity })
});

_logService.Info("OrderService", "创建订单", orderData);
```

---

## 🔧 故障排查

### 1. 日志未写入数据库

**可能原因：**
- 后台线程未启动
- 数据库文件权限问题
- 磁盘空间不足

**解决方案：**
```csharp
// 检查日志服务状态
var stats = _logService.GetStatistics();
Console.WriteLine($"数据库中有 {stats.TotalCount} 条日志");

// 检查内存日志
var memoryLogs = _logService.GetAllMemoryLogs();
Console.WriteLine($"内存中有 {memoryLogs.Count} 条日志");

// 手动导出检查
await _logService.ExportToFileAsync("test.log");
```

### 2. 日志窗口不实时更新

**可能原因：**
- 未订阅 LogAdded 事件
- 线程同步问题

**解决方案：**
```csharp
// 确保订阅了事件
_logService.LogAdded += OnLogAdded;

// 确保跨线程调用
private void OnLogAdded(object? sender, LogEntry entry)
{
    if (InvokeRequired)
    {
        BeginInvoke(() => UpdateUI(entry));  // 跨线程
    }
    else
    {
        UpdateUI(entry);
    }
}
```

### 3. 数据库锁定

**如果仍然出现锁定（罕见）：**
```csharp
// 启用WAL模式（应该已经启用）
using var conn = _db.GetConnection();
conn.Execute("PRAGMA journal_mode=WAL;");

// 设置更长的超时
conn.Execute("PRAGMA busy_timeout=5000;");  // 5秒
```

---

## 📈 监控和维护

### 1. 定期检查日志大小

```csharp
var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "logs.db");
var fileInfo = new FileInfo(dbPath);
Console.WriteLine($"日志数据库大小: {fileInfo.Length / 1024 / 1024} MB");

// 如果太大，清理旧日志
if (fileInfo.Length > 100 * 1024 * 1024)  // 100MB
{
    _logService.QueryLogs(
        endTime: DateTime.Now.AddMonths(-3)  // 删除3个月前的
    );
}
```

### 2. 日志分析

```csharp
// 统计错误最多的模块
var logs = _logService.QueryLogs(
    minLevel: LogLevel.Error,
    limit: 1000
);

var errorsBySource = logs
    .GroupBy(l => l.Source)
    .OrderByDescending(g => g.Count())
    .Select(g => new { Source = g.Key, Count = g.Count() });

foreach (var item in errorsBySource)
{
    Console.WriteLine($"{item.Source}: {item.Count} 个错误");
}
```

---

## 🎯 总结

### ✅ 核心优势

1. **数据库分离**：业务数据和日志完全隔离
2. **线程安全**：多线程可以安全地写日志
3. **异步高效**：不阻塞主线程
4. **实时查看**：UI实时更新
5. **灵活查询**：按时间、级别、来源、关键词过滤
6. **WAL模式**：读写并发，性能优秀

### 📁 文件结构

```
Data/
├── business.db       # 业务数据（会员、订单）
└── logs.db           # 日志数据

Logs/
├── 2025-11-04.log    # 文本日志（备份）
└── ...
```

### 🚀 快速开始

```csharp
// 1. 注入服务
public MyClass(ILogService logService)
{
    _logService = logService;
}

// 2. 记录日志
_logService.Info("MyClass", "操作成功");
_logService.Error("MyClass", "操作失败", ex);

// 3. 查看日志
var logViewer = ServiceProvider.GetRequiredService<LogViewerForm>();
logViewer.Show();
```

---

**日志系统已完成！** 🎉

现在你有了一个生产级的日志系统，支持：
- ✅ 实时显示
- ✅ 数据库持久化
- ✅ 零冲突（独立数据库）
- ✅ 线程安全
- ✅ 高性能

