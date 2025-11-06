# 🎮 VxMain 集成炳狗服务指南

## 📋 需要修改的内容

### 1. 添加 using 引用
```csharp
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Services.Games.Binggo;
using BaiShengVx3Plus.Models.Games.Binggo;
using BaiShengVx3Plus.Helpers;
```

### 2. 添加服务字段（在构造函数上方）
```csharp
// 🎮 炳狗游戏服务
private readonly IBinggoLotteryService _lotteryService;
private readonly IBinggoOrderService _orderService;
private readonly BinggoMessageHandler _binggoMessageHandler;
private readonly BinggoGameSettings _binggoSettings;

// 🎲 炳狗数据绑定
private Core.BinggoLotteryDataBindingList? _lotteryDataBindingList;
```

### 3. 修改构造函数签名，添加注入
```csharp
public VxMain(
    VxMainViewModel viewModel,
    ILogService logService,
    IWeixinSocketClient socketClient,
    MessageDispatcher messageDispatcher,
    IContactDataService contactDataService,
    IUserInfoService userInfoService,
    IWeChatService wechatService,
    IGroupBindingService groupBindingService,
    IBinggoLotteryService lotteryService,      // 🔥 新增
    IBinggoOrderService orderService,          // 🔥 新增
    BinggoMessageHandler binggoMessageHandler, // 🔥 新增
    BinggoGameSettings binggoSettings)         // 🔥 新增
{
    // ... 原有代码
    _lotteryService = lotteryService;
    _orderService = orderService;
    _binggoMessageHandler = binggoMessageHandler;
    _binggoSettings = binggoSettings;
    // ...
}
```

### 4. 在 `InitializeDatabase` 中初始化炳狗服务
```csharp
private void InitializeDatabase(string identifier)
{
    try
    {
        // ... 原有数据库初始化代码
        
        // 🎮 初始化炳狗服务
        InitializeBinggoServices();
        
        _logService.Info("VxMain", $"✅ 数据库初始化完成: {identifier}");
    }
    catch (Exception ex)
    {
        // ...
    }
}

/// <summary>
/// 初始化炳狗相关服务
/// </summary>
private void InitializeBinggoServices()
{
    try
    {
        _logService.Info("VxMain", "🎮 初始化炳狗服务...");
        
        // 1. 设置数据库连接
        _lotteryService.SetDatabase(_db);
        _orderService.SetDatabase(_db);
        
        // 2. 创建开奖数据 BindingList
        _lotteryDataBindingList = new Core.BinggoLotteryDataBindingList(_db, _logService);
        _lotteryDataBindingList.LoadFromDatabase(100); // 加载最近 100 期
        
        // 3. 设置开奖服务的 BindingList（用于自动更新 UI）
        _lotteryService.SetBindingList(_lotteryDataBindingList);
        
        // 4. 设置订单服务的 BindingList
        _orderService.SetOrdersBindingList(_ordersBindingList);
        _orderService.SetMembersBindingList(_membersBindingList);
        
        // 5. 订阅开奖事件（自动结算）
        _lotteryService.LotteryOpened += OnLotteryOpened;
        _lotteryService.StatusChanged += OnLotteryStatusChanged;
        _lotteryService.IssueChanged += OnLotteryIssueChanged;
        
        // 6. 启动开奖服务
        _lotteryService.StartAsync().Wait();
        
        _logService.Info("VxMain", "✅ 炳狗服务初始化完成");
    }
    catch (Exception ex)
    {
        _logService.Error("VxMain", $"炳狗服务初始化失败: {ex.Message}", ex);
        UIMessageBox.ShowError($"炳狗服务初始化失败：{ex.Message}");
    }
}
```

### 5. 添加开奖事件处理器
```csharp
/// <summary>
/// 开奖事件处理（自动结算）
/// </summary>
private async void OnLotteryOpened(object? sender, BinggoLotteryOpenedEventArgs e)
{
    try
    {
        _logService.Info("VxMain", 
            $"🎲 开奖: {e.LotteryData.IssueId} - {e.LotteryData.NumbersString}");
        
        // 自动结算订单
        var (settledCount, summary) = await _orderService.SettleOrdersAsync(
            e.LotteryData.IssueId, 
            e.LotteryData);
        
        _logService.Info("VxMain", 
            $"✅ 结算完成: {settledCount} 单");
        
        // TODO: 可选 - 发送结算通知到微信群
        // if (_binggoSettings.AutoSendSettlementNotice)
        // {
        //     await SendWeChatMessageAsync(summary);
        // }
    }
    catch (Exception ex)
    {
        _logService.Error("VxMain", $"开奖事件处理失败: {ex.Message}", ex);
    }
}

/// <summary>
/// 状态变更事件处理
/// </summary>
private void OnLotteryStatusChanged(object? sender, BinggoStatusChangedEventArgs e)
{
    UpdateUIThreadSafeAsync(() =>
    {
        // TODO: 更新 UI 状态显示
        _logService.Info("VxMain", $"🔄 状态变更: {e.NewStatus} - {e.Message}");
    });
}

/// <summary>
/// 期号变更事件处理
/// </summary>
private void OnLotteryIssueChanged(object? sender, BinggoIssueChangedEventArgs e)
{
    UpdateUIThreadSafeAsync(() =>
    {
        _logService.Info("VxMain", $"📅 期号变更: {e.NewIssueId}");
        
        // TODO: 可选 - 发送开盘通知到微信群
        // if (_binggoSettings.AutoSendOpenNotice)
        // {
        //     SendWeChatMessageAsync(_binggoSettings.ReplyOpenNotice);
        // }
    });
}
```

### 6. 集成消息处理（在 ChatMessageHandler 中）
在 `ChatMessageHandler.cs` 中添加炳狗消息处理：

```csharp
// ChatMessageHandler.cs
public class ChatMessageHandler : IMessageHandler
{
    private readonly BinggoMessageHandler _binggoMessageHandler;
    
    public ChatMessageHandler(..., BinggoMessageHandler binggoMessageHandler)
    {
        // ...
        _binggoMessageHandler = binggoMessageHandler;
    }
    
    public async Task HandleAsync(JsonElement data)
    {
        // ... 原有代码
        
        // 🎮 尝试处理炳狗下注消息
        var member = GetMemberByWxid(fromWxid);
        if (member != null)
        {
            var (handled, replyMessage) = await _binggoMessageHandler.HandleMessageAsync(
                member, 
                content);
            
            if (handled && !string.IsNullOrEmpty(replyMessage))
            {
                // 发送回复消息
                await SendWeChatReplyAsync(groupWxid, replyMessage);
                return; // 已处理，不再继续
            }
        }
        
        // ... 其他消息处理
    }
}
```

---

## 🎯 完整集成流程

### 流程图
```
应用启动
  ↓
Program.cs (注册所有服务) ✅
  ↓
VxMain 构造函数 (注入服务)
  ↓
用户登录成功
  ↓
InitializeDatabase("default")
  ├─ 创建 business.db
  └─ InitializeBinggoServices()
      ├─ 设置数据库连接
      ├─ 创建 BinggoLotteryDataBindingList
      ├─ 订阅开奖事件
      └─ 启动开奖服务
  ↓
用户连接微信
  ↓
InitializeDatabase(wxid)
  ├─ 创建 business_{wxid}.db
  └─ 重新初始化炳狗服务（使用新的数据库）
  ↓
用户绑定群组
  ↓
开始接收微信消息
  ↓
ChatMessageHandler
  ├─ BinggoMessageHandler.HandleMessageAsync()
  │   ├─ 判断是否下注消息
  │   ├─ 调用 OrderService.CreateOrderAsync()
  │   └─ 返回回复消息
  └─ 发送回复到微信群
  ↓
开奖定时器触发
  ↓
LotteryOpened 事件
  ↓
OnLotteryOpened()
  ├─ OrderService.SettleOrdersAsync()
  │   ├─ 查询未结算订单
  │   ├─ 计算盈利
  │   ├─ 更新余额
  │   └─ 标记已结算
  └─ 发送结算通知（可选）
```

---

## ✅ 集成检查清单

- [ ] Program.cs 添加服务注册
- [ ] VxMain.cs 添加 using 引用
- [ ] VxMain.cs 添加服务字段
- [ ] VxMain.cs 修改构造函数签名
- [ ] VxMain.cs 添加 InitializeBinggoServices()
- [ ] VxMain.cs 添加开奖事件处理器
- [ ] ChatMessageHandler.cs 集成 BinggoMessageHandler
- [ ] 测试完整流程
- [ ] 检查编译错误
- [ ] 检查运行时错误

---

## 🚀 下一步

1. 实现上述所有修改
2. 编译并测试
3. 添加简单的 UI 显示（可选）
4. 测试完整流程：登录 → 连接 → 绑定 → 下注 → 开奖 → 结算

---

**准备开始实施修改！** 🚀

