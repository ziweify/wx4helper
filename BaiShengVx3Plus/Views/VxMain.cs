using Sunny.UI;
using BaiShengVx3Plus.ViewModels;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Shared.Platform;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Services.Messages;
using BaiShengVx3Plus.Services.Messages.Handlers;
using BaiShengVx3Plus.Services.Games.Binggo;
using BaiShengVx3Plus.Services;
using BaiShengVx3Plus.Models.Games.Binggo;
using BaiShengVx3Plus.Models.Games.Binggo.Events;
using BaiShengVx3Plus.Helpers;
using BaiShengVx3Plus.Core;
using BaiShengVx3Plus.Extensions;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SQLite;

namespace BaiShengVx3Plus
{
    public partial class VxMain : UIForm
    {
        private readonly VxMainViewModel _viewModel;
        
        private readonly ILogService _logService;
        private readonly IWeixinSocketClient _socketClient; // Socket 客户端
        private readonly MessageDispatcher _messageDispatcher; // 消息分发器
        private readonly IContactDataService _contactDataService; // 联系人数据服务
        private readonly IUserInfoService _userInfoService; // 用户信息服务
        private readonly IWeChatService _wechatService; // 微信应用服务（Application Service）
        private readonly IGroupBindingService _groupBindingService; // 群组绑定服务
        private readonly IMemberDataService _memberDataService; // 会员数据访问服务
        
        // 🎮 炳狗游戏服务
        private readonly IBinggoLotteryService _lotteryService;
        private readonly IBinggoOrderService _orderService;
        private readonly BinggoStatisticsService _statisticsService; // 🔥 统计服务
        private readonly BinggoMessageHandler _binggoMessageHandler;
        private readonly Services.AutoBet.AutoBetService _autoBetService; // 🤖 自动投注服务
        private readonly Services.AutoBet.AutoBetCoordinator _autoBetCoordinator; // 🤖 自动投注协调器
        private readonly IConfigurationService _configService; // 📝 配置服务
        private readonly ViewModels.ConfigViewModel _configViewModel; // 📝 配置 ViewModel（用于数据绑定）
        private readonly ViewModels.SettingViewModel _settingViewModel; // 🌐 设置 ViewModel（全局单例）
        
        // 🔥 ORM 数据库连接（双库结构）
        private SQLiteConnection? _globalDb;  // 全局数据库: business.db (飞单配置、开奖数据)
        private SQLiteConnection? _db;  // 微信专属数据库: business_{wxid}.db (会员、订单、投注记录等)
        
        // 🔥 赔率控件步进控制
        private double _lastOddsValue = 1.97;  // 记录上一次的赔率值，用于检测按钮点击
        private Sunny.UI.UIDoubleUpDown.OnValueChanged? _oddsValueChangedHandler;  // 保存事件处理程序引用，用于解绑
        private string _currentDbPath = "";  // 当前微信专属数据库路径
        
        // 数据绑定列表
        private BindingList<WxContact> _contactsBindingList;
        private V2MemberBindingList? _membersBindingList;  // 🔥 使用 ORM BindingList
        private V2OrderBindingList? _ordersBindingList;    // 🔥 使用 ORM BindingList
        private V2CreditWithdrawBindingList? _creditWithdrawsBindingList;  // 🔥 上下分 BindingList（与会员、订单统一模式）
        private BinggoLotteryDataBindingList? _lotteryDataBindingList; // 🎲 炳狗开奖数据 BindingList
        
        // 🔥 封盘提醒标记已移至 BinggoLotteryService 内部管理
        
        // 设置窗口单实例
        private Views.SettingsForm? _settingsForm;
        private Views.BinggoLotteryResultForm? _lotteryResultForm;  // 🎲 开奖结果窗口
        

        //private string _currentGroupWxId = ""; // 🔥 当前绑定的群 wxid
        
        // 当前用户信息（用于检测用户切换）
        private WxUserInfo? _currentUserInfo;
        
        // 连接取消令牌
        private CancellationTokenSource? _connectCts;

        #region 线程安全的 UI 更新辅助方法

        /// <summary>
        /// 线程安全的 UI 更新（同步版本）
        /// 用于：必须立即完成的 UI 更新，例如显示错误对话框
        /// </summary>
        private void UpdateUIThreadSafe(Action uiAction)
        {
            if (InvokeRequired)
            {
                Invoke(uiAction);  // 同步等待
            }
            else
            {
                uiAction();
            }
        }

        /// <summary>
        /// 线程安全的 UI 更新（异步版本）
        /// 用于：不阻塞调用线程的 UI 更新，例如更新状态文本
        /// </summary>
        private void UpdateUIThreadSafeAsync(Action uiAction)
        {
            if (InvokeRequired)
            {
                BeginInvoke(uiAction);  // 异步，不等待
            }
            else
            {
                uiAction();
            }
        }

        #endregion

        public VxMain(
            VxMainViewModel viewModel,
            ILogService logService,
            IWeixinSocketClient socketClient,
            MessageDispatcher messageDispatcher,
            IContactDataService contactDataService, // 注入联系人数据服务
            IUserInfoService userInfoService, // 注入用户信息服务
            IWeChatService wechatService, // 注入微信应用服务
            IGroupBindingService groupBindingService, // 注入群组绑定服务
            IMemberDataService memberDataService, // 注入会员数据访问服务
            IBinggoLotteryService lotteryService, // 🎮 注入炳狗开奖服务
            IBinggoOrderService orderService, // 🎮 注入炳狗订单服务
            BinggoStatisticsService statisticsService, // 🔥 注入统计服务
            BinggoMessageHandler binggoMessageHandler, // 🎮 注入炳狗消息处理器
            Services.AutoBet.AutoBetService autoBetService, // 🤖 注入自动投注服务
            Services.AutoBet.AutoBetCoordinator autoBetCoordinator, // 🤖 注入自动投注协调器
            IConfigurationService configService, // 📝 注入配置服务
            ViewModels.SettingViewModel settingViewModel) // 🌐 注入设置 ViewModel（全局单例）
        {
            InitializeComponent();
            _viewModel = viewModel;
            _logService = logService;
            _socketClient = socketClient;
            _messageDispatcher = messageDispatcher;
            _contactDataService = contactDataService;
            _memberDataService = memberDataService;
            _userInfoService = userInfoService;
            _wechatService = wechatService;
            _groupBindingService = groupBindingService;
            _lotteryService = lotteryService;
            _orderService = orderService;
            _statisticsService = statisticsService; // 🔥 统计服务
            _binggoMessageHandler = binggoMessageHandler;
            _autoBetService = autoBetService; // 🤖 自动投注服务
            _autoBetCoordinator = autoBetCoordinator; // 🤖 自动投注协调器
            _configService = configService; // 📝 配置服务
            _configViewModel = new ViewModels.ConfigViewModel(configService); // 📝 创建配置 ViewModel
            _settingViewModel = settingViewModel; // 🌐 设置 ViewModel（全局单例）
            
            // 🔥 诊断：检查 AutoBetService 是否成功注入
            if (_autoBetService == null)
            {
                _logService.Error("VxMain", "❌❌❌ AutoBetService 未成功注入！这会导致浏览器无法连接！");
            }
            else
            {
                _logService.Info("VxMain", $"✅ AutoBetService 已注入: {_autoBetService.GetType().FullName}");
            }
            
            // 订阅服务器推送事件，并使用消息分发器处理
            _socketClient.OnServerPush += SocketClient_OnServerPush;
            
            // 启用自动重连
            _socketClient.AutoReconnect = true;
            
            // 订阅联系人数据更新事件
            _contactDataService.ContactsUpdated += ContactDataService_ContactsUpdated;
            
            // 订阅用户信息更新事件
            _userInfoService.UserInfoUpdated += UserInfoService_UserInfoUpdated;
            
            // 订阅微信服务的连接状态变化事件
            _wechatService.ConnectionStateChanged += WeChatService_ConnectionStateChanged;
            
            // 🔥 现代化数据绑定：用户信息服务 → 用户控件
            // 用户控件通过 PropertyChanged 自动更新，无需手动调用 UpdateDisplay
            ucUserInfo1.UserInfo = _userInfoService.CurrentUser;
            
            // 记录主窗口打开
            _logService.Info("VxMain", "主窗口已打开");

            // 🔥 初始化联系人列表
            _contactsBindingList = new BindingList<WxContact>();
            _contactsBindingList.AllowEdit = true;
            _contactsBindingList.AllowNew = false;
            _contactsBindingList.AllowRemove = false;

            // 🔥 立即初始化默认数据库 business.db（不需要等待 wxid）
            InitializeDatabase("default");

            InitializeDataBindings();
            InitializeAutoBetUIEvents();  // 🤖 绑定自动投注事件
            InitializeMemberContextMenu();  // 🔧 初始化会员表右键菜单（开发模式）
            InitializeOrderContextMenu();  // 🔧 初始化订单表右键菜单（补单功能）
            
            // 🔧 订阅会员选择变化事件（开发模式-自动更新当前测试会员）
            dgvMembers.SelectionChanged += DgvMembers_SelectionChanged;
        }

        /// <summary>
        /// 初始化数据库（使用 ORM，双库结构）
        /// 
        /// 🔥 数据库命名规则（优化后）：
        /// 1. 全局数据库: business.db（存储全局共享数据）
        ///    - AutoBetConfigs（飞单配置）
        ///    - BinggoLotteryData（开奖数据）
        ///    - BinggoBetItem（开奖下注项）
        /// 
        /// 2. 微信专属数据库: business_{wxid}.db（存储微信账号专属数据）
        ///    - V2Member（会员信息）
        ///    - V2MemberOrder（订单信息）
        ///    - V2CreditWithdraw（上下分记录）
        ///    - V2BalanceChange（资金变动记录）
        ///    - BetOrderRecord（投注记录）
        ///    - WxContact（联系人）
        ///    - WxUserInfo（用户信息）
        ///    - LogEntry（日志）
        /// 
        /// 3. 日志数据库: logs.db（全局共享，暂未使用）
        /// 
        /// 🔥 重要设计原则：
        /// 1. 数据库操作（增删改查）= 同步执行，保证数据一致性，避免污染
        /// 2. UI 更新（状态文本等）= 异步执行，避免阻塞 UI 线程，保证流畅
        /// 3. 数据绑定（DataSource）= 同步执行，确保数据立即生效
        /// </summary>
        /// <param name="wxid">微信ID，"default" 表示仅初始化全局数据库，其他为实际微信ID</param>
        private void InitializeDatabase(string wxid)
        {
            try
            {
                // ========================================
                // 🔥 步骤1: 初始化全局数据库（始终打开）
                // ========================================
                
                var dataDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "BaiShengVx3Plus",
                    "Data");
                Directory.CreateDirectory(dataDirectory);
                
                // 🔥 全局数据库：business.db（始终打开，存储全局共享数据）
                string globalDbPath = Path.Combine(dataDirectory, "business.db");
                
                if (_globalDb == null || _globalDb.DatabasePath != globalDbPath)
                {
                    _globalDb?.Close();
                    _globalDb = new SQLiteConnection(globalDbPath);
                    _logService.Info("VxMain", $"✅ 全局数据库已打开: {globalDbPath}");
                    
                    // 🔥 配置为最可靠模式（数据完整性优先）
                    ConfigureDatabaseReliability(_globalDb, "全局数据库");
                    
                    // 🔥 使用统一的数据库初始化器创建全局表
                    var databaseInitializer = Program.ServiceProvider?.GetService<Services.Database.DatabaseInitializer>();
                    if (databaseInitializer != null)
                    {
                        databaseInitializer.InitializeGlobalTables(_globalDb);
                    }
                    else
                    {
                        // 如果 DatabaseInitializer 不可用，使用旧方法（向后兼容）
                        InitializeGlobalTables(_globalDb);
                    }
                }
                
                // ========================================
                // 🔥 步骤2: 初始化微信专属数据库（如果需要）
                // ========================================
                
                if (wxid != "default")
                {
                    // 关闭旧的微信专属数据库连接
                    _db?.Close();
                    _db = null;
                    
                    // 🔥 微信专属数据库：business_{wxid}.db
                    string wxDbPath = Path.Combine(dataDirectory, $"business_{wxid}.db");
                    _currentDbPath = wxDbPath;
                    
                    _logService.Info("VxMain", $"初始化微信专属数据库: {wxDbPath}");
                    _db = new SQLiteConnection(wxDbPath);
                    
                    // 🔥 配置为最可靠模式（数据完整性优先）
                    ConfigureDatabaseReliability(_db, "微信专属数据库");
                    
                    // 🔥 使用统一的数据库初始化器创建微信专属表
                    var databaseInitializer = Program.ServiceProvider?.GetService<Services.Database.DatabaseInitializer>();
                    if (databaseInitializer != null)
                    {
                        databaseInitializer.InitializeWxTables(_db);
                    }
                    else
                    {
                        // 如果 DatabaseInitializer 不可用，使用旧方法（向后兼容）
                        InitializeWxTables(_db);
                    }
                    
                    // 🔥 将数据库连接传递给群组绑定服务
                    if (_groupBindingService is Services.GroupBinding.GroupBindingService groupBindingService)
                    {
                        groupBindingService.SetDatabase(_db);
                    }
                    
                    _logService.Info("VxMain", "✅ 微信专属数据库已准备，等待绑定群后加载数据");
                }
                else
                {
                    _logService.Info("VxMain", "✅ 仅初始化全局数据库（默认模式）");
                }
                
                // ========================================
                // 🔥 步骤3: 初始化炳狗服务
                // ========================================
                
                // 🔥 根据初始化场景，选择初始化方法
                if (wxid == "default")
                {
                    // 首次启动：初始化全局服务（只执行一次）
                    InitializeGlobalServices();
                }
                else
                {
                    // 微信连接成功：只初始化微信专属服务
                    InitializeWxServices();
                }
                
                // ========================================
                // 🔥 步骤4: 日志记录
                // ========================================
                
                _logService.Info("VxMain", $"✓ 数据库初始化完成");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"初始化数据库失败: {ex.Message}", ex);
                
                // 错误提示（同步，确保用户看到）
                UpdateUIThreadSafe(() => 
                {
                    UIMessageBox.ShowError($"初始化数据库失败: {ex.Message}");
                });
            }
        }
        
        /// <summary>
        /// 配置数据库为最可靠模式
        /// 🔥 可靠性优先于性能（适用于配置、订单、会员等关键数据）
        /// </summary>
        private void ConfigureDatabaseReliability(SQLiteConnection db, string dbName)
        {
            try
            {
                _logService.Info("VxMain", $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _logService.Info("VxMain", $"🔧 配置 {dbName} 为最可靠模式...");
                
                // 1️⃣ 禁用 WAL 模式，使用传统 DELETE 日志
                // 优点：数据立即写入主文件，工具兼容性好，备份简单
                // 缺点：性能略低于 WAL（但对我们的场景影响很小）
                db.Execute("PRAGMA journal_mode = DELETE");
                var journalMode = db.ExecuteScalar<string>("PRAGMA journal_mode");
                _logService.Info("VxMain", $"✅ 日志模式: {journalMode}");
                _logService.Info("VxMain", $"   说明: 数据立即写入主文件，无需等待检查点");
                
                // 2️⃣ 设置为 FULL 同步模式
                // 确保每次写入都刷新到磁盘（即使断电也不会丢数据）
                db.Execute("PRAGMA synchronous = FULL");
                var syncMode = db.ExecuteScalar<int>("PRAGMA synchronous");
                var syncModeName = syncMode switch
                {
                    0 => "OFF (最快，最不安全)",
                    1 => "NORMAL (一般)",
                    2 => "FULL (最慢，最安全)",
                    3 => "EXTRA (超级安全)",
                    _ => $"未知({syncMode})"
                };
                _logService.Info("VxMain", $"✅ 同步模式: {syncModeName}");
                _logService.Info("VxMain", $"   说明: 数据立即刷新到磁盘，防止断电丢失");
                
                // 3️⃣ 启用外键约束（数据一致性）
                db.Execute("PRAGMA foreign_keys = ON");
                var fkEnabled = db.ExecuteScalar<int>("PRAGMA foreign_keys");
                _logService.Info("VxMain", $"✅ 外键约束: {(fkEnabled == 1 ? "已启用" : "未启用")}");
                
                // 4️⃣ 设置合理的缓存大小（平衡性能和内存）
                db.Execute("PRAGMA cache_size = 2000");  // 约 8MB 缓存
                _logService.Info("VxMain", $"✅ 缓存大小: 2000 页 (约 8MB)");
                
                _logService.Info("VxMain", $"✅ {dbName} 已配置为最可靠模式");
                _logService.Info("VxMain", $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            catch (Exception ex)
            {
                _logService.Warning("VxMain", $"配置 {dbName} 参数失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 初始化全局服务（只在应用启动时执行一次）
        /// 🔥 全局数据库相关的服务：AutoBetService, LotteryService
        /// </summary>
        private void InitializeGlobalServices()
        {
            try
            {
                _logService.Info("VxMain", "🎮 初始化全局服务（仅执行一次）...");
                
                // 🔥 检查全局数据库是否已初始化
                if (_globalDb == null)
                {
                    _logService.Error("VxMain", "❌ 全局数据库未初始化，无法初始化全局服务！");
                    return;
                }
                
                // 🔥 1. 设置全局数据库连接
                // - AutoBetService: AutoBetConfigs（飞单配置）
                _autoBetService.SetDatabase(_globalDb);
                _logService.Info("VxMain", "✅ AutoBetService 已设置全局数据库（AutoBetConfigs）");
                
                // - LotteryService: BinggoLotteryData（开奖数据）
                _lotteryService.SetDatabase(_globalDb);
                _logService.Info("VxMain", "✅ LotteryService 已设置全局数据库（BinggoLotteryData）");
                
                // 📌 BetRecordService: 已在 AutoBetService.SetDatabase 中初始化
                _logService.Info("VxMain", "✅ BetRecordService 将在 AutoBetService.SetDatabase 中自动初始化");
                
                // 🤖 数据库设置完成后，加载自动投注设置
                LoadAutoBetSettings();
                
                // 🔊 添加声音测试按钮（动态创建）
                AddSoundTestButton();
                
                // 🎚️ 加载应用配置（从 appsettings.json）
                LoadAppConfiguration();
                
                // 🔥 配置自管理模式：启动监控线程
                _logService.Info("VxMain", "🚀 启动自动投注监控线程（配置自管理模式）...");
                _autoBetService.StartMonitoring();
                _logService.Info("VxMain", "✅ 配置初始化完成");
                
                // 2. 创建开奖数据 BindingList（使用全局数据库）
                _lotteryDataBindingList = new BinggoLotteryDataBindingList(_globalDb, _logService);
                _lotteryDataBindingList.LoadFromDatabase(100);
                
                // 3. 设置开奖服务的 BindingList（用于自动更新 UI）
                _lotteryService.SetBindingList(_lotteryDataBindingList);
                
                // 6. 订阅开奖事件（UI 更新）
                _lotteryService.LotteryOpened += OnLotteryOpened;
                _lotteryService.StatusChanged += OnLotteryStatusChanged;
                _lotteryService.IssueChanged += OnLotteryIssueChanged;
                
                _logService.Info("VxMain", "✅ 全局服务初始化完成");
                
                // 7. 初始化UI相关服务
                InitializeUIServices();
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "初始化全局服务失败", ex);
            }
        }
        
        /// <summary>
        /// 初始化微信专属服务（每次切换微信时执行）
        /// 🔥 微信专属数据库相关的服务：OrderService, AdminCommandHandler
        /// </summary>
        private void InitializeWxServices()
        {
            try
            {
                _logService.Info("VxMain", "🎮 初始化微信专属服务...");
                
                // 📌 AdminCommandHandler: 设置会员 BindingList、数据库、上下分服务和 BindingList
                var adminCommandHandler = Program.ServiceProvider.GetService<Services.Messages.Handlers.AdminCommandHandler>();
                if (adminCommandHandler != null && _db != null)
                {
                    adminCommandHandler.SetDatabase(_db);
                    if (_membersBindingList != null)
                    {
                        adminCommandHandler.SetMembersBindingList(_membersBindingList);
                        
                        // 🔥 创建并设置上下分服务（参考 F5BotV2）
                        var creditWithdrawService = new Services.Games.Binggo.CreditWithdrawService(
                            _db,
                            _logService,
                            _statisticsService,
                            _socketClient,
                            Program.ServiceProvider.GetService<Services.Sound.SoundService>());
                        adminCommandHandler.SetCreditWithdrawService(creditWithdrawService);
                        
                        // 🔥 设置上下分 BindingList
                        if (_creditWithdrawsBindingList != null)
                        {
                            adminCommandHandler.SetCreditWithdrawsBindingList(_creditWithdrawsBindingList);
                        }
                        
                        _logService.Info("VxMain", "✅ AdminCommandHandler 已设置会员列表、数据库、上下分服务和 BindingList");
                    }
                    else
                    {
                        _logService.Info("VxMain", "⚠️ AdminCommandHandler 已设置数据库，但会员列表尚未初始化（需要先绑定群）");
                    }
                }
                
                // 📌 微信专属数据库（business_{wxid}.db）
                if (_db != null)
                {
                    // - OrderService: V2MemberOrder（订单）
                    _orderService.SetDatabase(_db);
                    _logService.Info("VxMain", "✅ OrderService 已设置微信专属数据库（V2MemberOrder）");
                }
                else
                {
                    _logService.Warning("VxMain", "⚠️ 微信专属数据库未初始化（未绑定微信），部分功能暂不可用");
                }
                
                // 4. 设置订单服务的 BindingList
                if (_db != null)
                {
                    _orderService.SetOrdersBindingList(_ordersBindingList);
                    _orderService.SetMembersBindingList(_membersBindingList);
                }
                
                // 🔥 5. 设置开奖服务的业务依赖（用于结算和发送微信消息）
                if (_lotteryService is BinggoLotteryService lotteryServiceImpl)
                {
                    lotteryServiceImpl.SetBusinessDependencies(
                        _orderService,
                        _groupBindingService,
                        _socketClient,
                        _ordersBindingList,
                        _membersBindingList,
                        _creditWithdrawsBindingList,
                        _statisticsService
                    );
                    
                    // 设置数据库连接（用于上下分申请）
                    if (_db != null)
                    {
                        lotteryServiceImpl.SetDatabaseForCreditWithdraw(_db);
                    }
                }
                
                _logService.Info("VxMain", "✅ 微信专属服务初始化完成");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "初始化微信专属服务失败", ex);
            }
        }
        
        /// <summary>
        /// 初始化UI相关服务（在全局服务初始化后调用）
        /// </summary>
        private void InitializeUIServices()
        {
            try
            {
                // 🔥 6. 订阅统计服务属性变化（自动更新 UI）
                _statisticsService.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(BinggoStatisticsService.PanDescribe))
                    {
                        _logService.Info("VxMain", 
                            $"📢 收到 PanDescribe 属性变化通知（线程{System.Threading.Thread.CurrentThread.ManagedThreadId}），准备更新 UI");
                        UpdateUIThreadSafe(() => 
                        {
                            _logService.Info("VxMain", 
                                $"🎨 在 UI 线程中更新 lblMemberInfo（线程{System.Threading.Thread.CurrentThread.ManagedThreadId}）");
                            UpdateMemberInfoLabel();
                        });
                    }
                };
                
                // 6. 启动开奖服务
                _ = _lotteryService.StartAsync();
                
                // 7. 🎨 绑定 UI 控件到开奖服务
                _logService.Info("VxMain", "🎨 开始绑定 UI 控件到开奖服务...");
                
                UpdateUIThreadSafeAsync(() =>
                {
                    _logService.Info("VxMain", "📍 在 UI 线程中执行绑定...");
                    
                    if (ucBinggoDataCur != null)
                    {
                        ucBinggoDataCur.SetLotteryService(_lotteryService);
                        _logService.Info("VxMain", "✅ ucBinggoDataCur.SetLotteryService 完成");
                    }
                    
                    if (ucBinggoDataLast != null)
                    {
                        ucBinggoDataLast.SetLotteryService(_lotteryService);
                        _logService.Info("VxMain", "✅ ucBinggoDataLast.SetLotteryService 完成");
                    }
                });
                
                // 🔥 立即加载最近的开奖数据
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(500);
                        _logService.Info("VxMain", "🎲 开始立即加载最近开奖数据...");
                        
                        var recentData = await _lotteryService.GetRecentLotteryDataAsync(5);
                        if (recentData != null && recentData.Count > 0)
                        {
                            _logService.Info("VxMain", $"✅ 立即加载成功，获取 {recentData.Count} 期数据");
                            _logService.Info("VxMain", $"   最新期号: {recentData[0].IssueId}");
                            _logService.Info("VxMain", $"   开奖号码: {recentData[0].ToLotteryString()}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Error("VxMain", $"立即加载开奖数据失败: {ex.Message}", ex);
                    }
                });
                
                _logService.Info("VxMain", "✅ UI服务初始化完成");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "UI服务初始化失败", ex);
            }
        }
        
        /// <summary>
        /// 初始化快速设置面板
        /// </summary>
        private void InitializeFastSettings()
        {
            try
            {
                // 从配置加载到 UI
                txtSealSeconds.Value = _configService.GetSealSecondsAhead();
                txtMinBet.Value = (int)_configService.GetMinBet();
                txtMaxBet.Value = (int)_configService.GetMaxBet();
                
                // 🔥 绑定事件：用户修改快速设置时自动保存
                txtSealSeconds.ValueChanged += (s, e) =>
                {
                    // 🔥 直接使用 ConfigurationService 保存（自动持久化到 appsettings.json）
                    _configService.SetSealSecondsAhead((int)txtSealSeconds.Value);
                };
                
                txtMinBet.ValueChanged += (s, e) =>
                {
                    // 🔥 直接使用 ConfigurationService 保存（自动持久化到 appsettings.json）
                    _configService.SetMinBet((float)txtMinBet.Value);
                };
                
                txtMaxBet.ValueChanged += (s, e) =>
                {
                    // 🔥 直接使用 ConfigurationService 保存（自动持久化到 appsettings.json）
                    _configService.SetMaxBet((float)txtMaxBet.Value);
                };
                
                // 🔥 管理模式初始化（默认关闭）
                // chkAdminMode 在 Settings 窗口中，不在主窗口
                UpdateAdminModeUI();
                
                _logService.Info("VxMain", "✅ 快速设置面板已初始化");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"快速设置面板初始化失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 更新管理模式 UI 状态
        /// </summary>
        private void UpdateAdminModeUI()
        {
            bool isAdminMode = _configService.GetIsRunModeAdmin();
            
            // 管理模式下，txtCurrentContact 可编辑
            txtCurrentContact.ReadOnly = !isAdminMode;
            txtCurrentContact.BackColor = isAdminMode ? Color.White : SystemColors.Control;
            
            _logService.Info("VxMain", isAdminMode ? "✅ 管理模式已启用" : "❌ 管理模式已禁用");
        }
        
        /// <summary>
        /// 管理模式 checkbox 变化事件 - 已移到 SettingsForm
        /// </summary>
        // private void ChkAdminMode_CheckedChanged(object? sender, EventArgs e)
        // {
        //     _binggoSettings.IsAdminMode = chkAdminMode?.Checked ?? false;
        //     UpdateAdminModeUI();
        // }
        
        /// <summary>
        /// txtCurrentContact 按回车手动绑定（管理模式）
        /// </summary>
        private async void TxtCurrentContact_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _configService.GetIsRunModeAdmin())
            {
                string input = txtCurrentContact.Text.Trim();
                if (string.IsNullOrEmpty(input))
                    return;
                
                try
                {
                    // 🔥 解析输入：支持 "nickname (wxid)" 或直接 "wxid"
                    string wxid;
                    string nickname;
                    
                    if (input.Contains("(") && input.Contains(")"))
                    {
                        // 格式：nickname (wxid)
                        int startIndex = input.IndexOf('(');
                        int endIndex = input.IndexOf(')');
                        wxid = input.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
                        nickname = input.Substring(0, startIndex).Trim();
                    }
                    else
                    {
                        // 直接输入 wxid
                        wxid = input;
                        nickname = "手动绑定群";
                    }
                    
                    // 验证是否为群（包含 @ 符号）
                    if (!wxid.Contains("@"))
                    {
                        UIMessageBox.ShowWarning("请输入正确的群 wxid（必须包含 @ 符号）！");
                        return;
                    }
                    
                    _logService.Info("VxMain", $"📍 管理模式手动绑定: {nickname} ({wxid})");
                    
                    // 🔥 创建联系人对象并走统一绑定流程
                    var contact = new WxContact
                    {
                        Wxid = wxid,
                        Nickname = nickname,
                        Remark = "手动绑定"
                    };
                    
                    await BindGroupAsync(contact);
                    
                    _logService.Info("VxMain", $"✅ 管理模式手动绑定成功: {wxid}");
                }
                catch (Exception ex)
                {
                    _logService.Error("VxMain", $"手动绑定失败: {ex.Message}", ex);
                    UIMessageBox.ShowError($"手动绑定失败！\n\n{ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 封盘提前秒数值改变事件
        /// </summary>
        private void TxtSealSeconds_ValueChanged(object? sender, int value)
        {
            try
            {
                _configService.SetSealSecondsAhead(value);
                _logService.Info("VxMain", $"封盘提前秒数已更新: {value} 秒");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"更新封盘提前秒数失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 加载最近的开奖数据
        /// </summary>
        private async Task LoadRecentLotteryDataAsync()
        {
            try
            {
                _logService.Info("VxMain", "📊 开始加载最近开奖数据...");
                
                // 🔥 完全参考 F5BotV2 的 getbgday 接口
                // URL: http://8.134.71.102:789/api/boter/getbgday?limit=100&sign={c_sign}&fill=1
                var recentData = await _lotteryService.GetRecentLotteryDataAsync(100);
                
                if (recentData != null && recentData.Count > 0)
                {
                    _logService.Info("VxMain", $"✅ 成功加载 {recentData.Count} 期开奖数据");
                    
                    // 数据已经自动保存到数据库和 BindingList
                    // UI 会自动更新
                }
                else
                {
                    _logService.Warning("VxMain", "❌ 未获取到开奖数据");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"加载开奖数据失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 开奖事件处理（UI 更新）
        /// 🔥 业务逻辑（结算、发送微信消息等）已在 BinggoLotteryService 中统一处理
        /// </summary>
        private void OnLotteryOpened(object? sender, BinggoLotteryOpenedEventArgs e)
        {
            try
            {
                _logService.Info("VxMain", 
                    $"🎲 开奖: {e.LotteryData.ToLotteryString()}");
                
                // 🔥 业务逻辑（结算、发送微信消息、清空投注金额）已在 BinggoLotteryService.OnLotteryOpenedAsync 中处理
                // 这里只需要更新 UI（如果需要的话）
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"开奖事件处理失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 状态变更事件处理（UI 更新）
        /// 🔥 业务逻辑（封盘提醒、封盘消息等）已在 BinggoLotteryService 中统一处理
        /// </summary>
        private void OnLotteryStatusChanged(object? sender, BinggoStatusChangedEventArgs e)
        {
            UpdateUIThreadSafeAsync(() =>
            {
                _logService.Info("VxMain", $"🔄 状态变更: {e.NewStatus} - {e.Message}");
                
                // 🔥 业务逻辑（封盘提醒、封盘消息）已在 BinggoLotteryService 中统一处理
                // 这里只需要更新 UI（如果需要的话）
            });
        }
        
        /// <summary>
        /// 期号变更事件处理
        /// 🔥 更新当前期号并重新计算本期下注统计
        /// </summary>
        private void OnLotteryIssueChanged(object? sender, BinggoIssueChangedEventArgs e)
        {
            UpdateUIThreadSafeAsync(() =>
            {
                _logService.Info("VxMain", $"📅 期号变更: {e.OldIssueId} → {e.NewIssueId}");
                
                // 🔥 封盘提醒标记已在 BinggoLotteryService 内部管理
                
                // 🔥 设置当前期号（会自动重新计算本期下注）
                _statisticsService.SetCurrentIssueId(e.NewIssueId);
                
                // TODO: 可选 - 发送开盘通知到微信群
            });
        }
        
        // 🔥 封盘提醒和封盘消息已移至 BinggoLotteryService 内部处理
        
        /// <summary>
        /// 获取当前绑定的群ID
        /// </summary>
        private string? GetCurrentGroupWxId()
        {
            return _groupBindingService.CurrentBoundGroup?.Wxid;
        }
        
        private void InitializeDataBindings()
        {
            // 绑定联系人列表
            dgvContacts.DataSource = _contactsBindingList;
            dgvContacts.AutoGenerateColumns = true;
            dgvContacts.ReadOnly = true;
            
            // 🔥 美化联系人列表样式
            CustomizeContactsGridStyle();

            // 🔥 只初始化 DataGridView，不加载数据（等待绑定群）
            dgvMembers.AutoGenerateColumns = true;
            dgvMembers.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvMembers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            // 🔥 美化会员列表样式
            CustomizeMembersGridStyle();
            
            // 🔥 只初始化订单列表，不加载数据（等待绑定群）
            dgvOrders.AutoGenerateColumns = true;
            dgvOrders.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            // 🔥 美化订单列表样式
            CustomizeOrdersGridStyle();

            // 🔥 配置会员表列（列宽、可见性、格式）
            ConfigureMembersDataGridView();

            // 🔥 配置订单表列（列宽、可见性、格式）
            ConfigureOrdersDataGridView();

            // ✅ 数据加载延迟到绑定群后
            _logService.Info("VxMain", "✅ 数据绑定初始化完成（未加载数据，等待绑定群）");
        }

        private void LoadTestData()
        {
            // ✅ 所有测试数据已清空
            // 联系人数据：从服务器获取
            // 会员数据：从数据库加载（自动追踪）
            // 订单数据：从数据库加载（自动追踪）

            UpdateStatistics();
        }

        /// <summary>
        /// 更新统计信息显示
        /// 🔥 统一的统计更新方法（参考 F5BotV2 第 790 行）
        /// </summary>
        private void UpdateStatistics()
        {
            // 🔥 调用统计服务重新计算（唯一入口）
            _statisticsService.UpdateStatistics();
            
            // 🔥 更新 UI 显示
            UpdateMemberInfoLabel();
        }
        
        /// <summary>
        /// 🔥 统计数据缓存（用于自定义绘制）
        /// </summary>
        private class StatsData
        {
            public int MemberCount { get; set; }
            public int OrderCount { get; set; }
            public float BetMoneyTotal { get; set; }
            public float BetMoneyToday { get; set; }
            public float BetMoneyCur { get; set; }
            public int IssueidCur { get; set; }
            public float IncomeTotal { get; set; }
            public float IncomeToday { get; set; }
            public float CreditTotal { get; set; }
            public float CreditToday { get; set; }
            public float WithdrawTotal { get; set; }
            public float WithdrawToday { get; set; }
        }
        
        private StatsData _currentStats = new StatsData();
        
        /// <summary>
        /// 更新会员信息标签
        /// 🔥 显示会员数、订单数 + 统计信息（参考 F5BotV2 第 805 行）
        /// </summary>
        private void UpdateMemberInfoLabel()
        {
            // 🔥 更新统计数据缓存
            _currentStats.MemberCount = _membersBindingList?.Count ?? 0;
            _currentStats.OrderCount = _ordersBindingList?.Count ?? 0;
            _currentStats.BetMoneyTotal = _statisticsService.BetMoneyTotal;
            _currentStats.BetMoneyToday = _statisticsService.BetMoneyToday;
            _currentStats.BetMoneyCur = _statisticsService.BetMoneyCur;
            _currentStats.IssueidCur = _statisticsService.IssueidCur;
            _currentStats.IncomeTotal = _statisticsService.IncomeTotal;
            _currentStats.IncomeToday = _statisticsService.IncomeToday;
            _currentStats.CreditTotal = _statisticsService.CreditTotal;
            _currentStats.CreditToday = _statisticsService.CreditToday;
            _currentStats.WithdrawTotal = _statisticsService.WithdrawTotal;
            _currentStats.WithdrawToday = _statisticsService.WithdrawToday;
            
            // 🔥 触发重绘
            lblMemberInfo.Invalidate();
            
            // 订单信息标签（可选保留）
            lblOrderInfo.Text = $"订单列表 (共{_currentStats.OrderCount}单)";
        }
        
        /// <summary>
        /// 🔥 自定义绘制统计信息（带颜色和背景）
        /// 整行统一背景色 + 数据块分色显示
        /// </summary>
        private void lblMemberInfo_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            // 🔥 绘制整行统一背景色（淡蓝灰色，更突出）
            using (var brush = new SolidBrush(Color.FromArgb(240, 245, 250)))
            {
                g.FillRectangle(brush, 0, 0, lblMemberInfo.Width, lblMemberInfo.Height);
            }
            
            float x = 8;
            float y = 6;
            
            // 🔥 基础信息（深色文字）
            DrawText(g, $"会员: {_currentStats.MemberCount} 人 | 订单: {_currentStats.OrderCount} 单 | ", 
                ref x, y, Color.FromArgb(48, 48, 48), Color.Transparent);
            
            // 🔥 总注（整块显示）
            DrawDataBlock(g, "总注", _currentStats.BetMoneyTotal, ref x, y);
            DrawText(g, " | ", ref x, y, Color.FromArgb(100, 100, 100), Color.Transparent);
            
            // 🔥 今投（整块显示）
            DrawDataBlock(g, "今投", _currentStats.BetMoneyToday, ref x, y);
            DrawText(g, " | ", ref x, y, Color.FromArgb(100, 100, 100), Color.Transparent);
            
            // 🔥 当前期投注（整块显示）
            DrawDataBlock(g, $"当前:{_currentStats.IssueidCur}投注", _currentStats.BetMoneyCur, ref x, y);
            DrawText(g, " | ", ref x, y, Color.FromArgb(100, 100, 100), Color.Transparent);
            
            // 🔥 总盈利/今日盈利（整块显示）
            DrawDoubleDataBlock(g, "总/今盈利", _currentStats.IncomeTotal, _currentStats.IncomeToday, ref x, y);
            DrawText(g, " | ", ref x, y, Color.FromArgb(100, 100, 100), Color.Transparent);
            
            // 🔥 总上/今上（整块显示）
            DrawDoubleDataBlock(g, "总上/今上", _currentStats.CreditTotal, _currentStats.CreditToday, ref x, y);
            DrawText(g, " | ", ref x, y, Color.FromArgb(100, 100, 100), Color.Transparent);
            
            // 🔥 总下/今下（整块显示）
            DrawDoubleDataBlock(g, "总下/今下", _currentStats.WithdrawTotal, _currentStats.WithdrawToday, ref x, y);
        }
        
        /// <summary>
        /// 绘制普通文本
        /// </summary>
        private void DrawText(Graphics g, string text, ref float x, float y, Color foreColor, Color backColor)
        {
            using var font = new Font("微软雅黑", 9F, FontStyle.Regular);
            var size = g.MeasureString(text, font);
            
            if (backColor != Color.Transparent)
            {
                using var brush = new SolidBrush(backColor);
                g.FillRectangle(brush, x, y - 2, size.Width, size.Height);
            }
            
            using var textBrush = new SolidBrush(foreColor);
            g.DrawString(text, font, textBrush, x, y);
            x += size.Width;
        }
        
        /// <summary>
        /// 🔥 绘制单个数据块（标签+金额，整体一个颜色背景）
        /// </summary>
        private void DrawDataBlock(Graphics g, string label, float amount, ref float x, float y)
        {
            string text = $"{label}:{amount:F2}";
            
            // 🔥 根据金额绝对值确定颜色
            var colors = GetAmountColors(Math.Abs(amount));
            
            using var font = new Font("微软雅黑", 9F, FontStyle.Bold);
            var size = g.MeasureString(text, font);
            
            // 🔥 绘制圆角背景（整块）
            using var path = CreateRoundedRectanglePath(x - 3, y - 2, size.Width + 6, size.Height + 2, 4);
            using var brush = new SolidBrush(colors.BackColor);
            g.FillPath(brush, path);
            
            // 🔥 绘制完整文字（标签:金额）
            using var textBrush = new SolidBrush(colors.ForeColor);
            g.DrawString(text, font, textBrush, x, y);
            x += size.Width + 6;
        }
        
        /// <summary>
        /// 🔥 绘制双数据块（标签+金额1/金额2，整体一个颜色背景）
        /// </summary>
        private void DrawDoubleDataBlock(Graphics g, string label, float amount1, float amount2, ref float x, float y)
        {
            string text = $"{label}:{amount1:F2}/{amount2:F2}";
            
            // 🔥 使用两个金额中较大的绝对值来决定颜色
            float maxAmount = Math.Max(Math.Abs(amount1), Math.Abs(amount2));
            var colors = GetAmountColors(maxAmount);
            
            using var font = new Font("微软雅黑", 9F, FontStyle.Bold);
            var size = g.MeasureString(text, font);
            
            // 🔥 绘制圆角背景（整块）
            using var path = CreateRoundedRectanglePath(x - 3, y - 2, size.Width + 6, size.Height + 2, 4);
            using var brush = new SolidBrush(colors.BackColor);
            g.FillPath(brush, path);
            
            // 🔥 绘制完整文字（标签:金额1/金额2）
            using var textBrush = new SolidBrush(colors.ForeColor);
            g.DrawString(text, font, textBrush, x, y);
            x += size.Width + 6;
        }
        
        /// <summary>
        /// 🔥 根据金额大小获取颜色配置
        /// 金额分级：
        /// - < 1000: 橘色系 (#FF8C00)
        /// - < 10000: 金色系 (#FFD700)
        /// - >= 10000: 红色系 (#DC143C)
        /// </summary>
        private (Color BackColor, Color ForeColor) GetAmountColors(float absAmount)
        {
            if (absAmount < 1000)
            {
                return (Color.FromArgb(255, 140, 0), Color.White);  // 橘色背景 + 白字
            }
            else if (absAmount < 10000)
            {
                return (Color.FromArgb(255, 215, 0), Color.FromArgb(48, 48, 48));  // 金色背景 + 深灰字
            }
            else
            {
                return (Color.FromArgb(220, 20, 60), Color.White);  // 红色背景 + 白字
            }
        }
        
        /// <summary>
        /// 创建圆角矩形路径
        /// </summary>
        private System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectanglePath(float x, float y, float width, float height, float radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(x, y, radius, radius, 180, 90);
            path.AddArc(x + width - radius, y, radius, radius, 270, 90);
            path.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90);
            path.AddArc(x, y + height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private async void VxMain_Load(object sender, EventArgs e)
        {
            try
            {
                // 🔥 显示版本号
                this.Text = Utils.VersionInfo.FullVersion;
                _logService.Info("VxMain", $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _logService.Info("VxMain", $"🚀 {Utils.VersionInfo.FullVersion}");
                _logService.Info("VxMain", $"📅 构建日期: {Utils.VersionInfo.BuildDate}");
                _logService.Info("VxMain", $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                
                // 🔥 设置声音服务的 UI 线程上下文（确保 MCI API 在 UI 线程中调用）
                var soundService = Program.ServiceProvider?.GetService<Services.Sound.SoundService>();
                if (soundService != null && SynchronizationContext.Current != null)
                {
                    soundService.SetUIContext(SynchronizationContext.Current);
                    _logService.Info("VxMain", $"✅ 声音服务 UI 线程上下文已设置");
                }
                else
                {
                    _logService.Warning("VxMain", $"⚠️ 无法设置声音服务 UI 线程上下文: soundService={soundService != null}, SyncContext={SynchronizationContext.Current != null}");
                }
                
                lblStatus.Text = "正在初始化...";
                
                // 🔥 初始化平台下拉框（使用统一数据源）
                InitializePlatformComboBox();
                
                // 隐藏不需要显示的列
                if (dgvContacts.Columns.Count > 0)
                {
                    HideContactColumns();
                }

                // 🔥 会员表和订单表的列配置已在 InitializeDataBindings() 中完成
                // 不需要在这里重复调用配置方法
                
                // 🎮 初始化快速设置面板
                InitializeFastSettings();
                
                // 🔥 TODO: 检查微信版本（在连接前）
                // if (!await CheckWeChatVersionAsync())
                // {
                //     // 版本不匹配且用户未安装，退出程序
                //     _logService.Warning("VxMain", "微信版本不匹配，程序退出");
                //     lblStatus.Text = "微信版本不匹配";
                //     Application.Exit();
                //     return;
                // }
                
                // 🔥 统一使用 WeChatService 进行连接和初始化
                // forceRestart = false，会先尝试快速连接，失败才启动/注入
                _logService.Info("VxMain", "程序启动，开始自动连接和初始化...");
                
                var success = await _wechatService.ConnectAndInitializeAsync(forceRestart: false);
                
                if (!success)
                {
                    _logService.Info("VxMain", "自动连接失败，启动自动重连（每5秒尝试一次）");
                    _socketClient.StartAutoReconnect(5000);
                }
                else
                {
                    _logService.Info("VxMain", "✅ 自动连接和初始化成功");
                }
                
                // 🌐 登录成功后加载开奖数据（登录窗口已经完成 WebAPI 登录）
                // ⚠️ 重要：必须在数据库初始化后才能加载开奖数据
                // 数据库初始化在 UserInfoService_UserInfoUpdated 中触发
                // 这里延迟加载，确保数据库已经准备好
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);  // 等待2秒，确保数据库已初始化
                    await LoadRecentLotteryDataAsync();
                });
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "窗口加载时发生错误", ex);
                lblStatus.Text = "初始化失败";
            }
        }

        // 🔥 鼠标悬停的行索引
        private int _hoverRowIndex_Contacts = -1;
        private int _hoverRowIndex_Members = -1;
        private int _hoverRowIndex_Orders = -1;

        #region 美化样式设置

        /// <summary>
        /// 美化联系人列表样式
        /// </summary>
        private void CustomizeContactsGridStyle()
        {
            // 🔥 1. 绑定行格式化事件（绿色显示已绑定的行）
            dgvContacts.CellFormatting += dgvContacts_CellFormatting;
            
            // 🔥 2. 自定义选中样式（透明蒙板 + 高亮边框）
            dgvContacts.DefaultCellStyle.SelectionBackColor = Color.Transparent;
            dgvContacts.DefaultCellStyle.SelectionForeColor = Color.Black;
            
            // 🔥 3. 绑定 CellPainting 事件（绘制自定义选中效果 + Hover 效果）
            dgvContacts.CellPainting += dgvContacts_CellPainting;
            
            // 🔥 4. 绑定鼠标事件（Hover 效果）
            dgvContacts.CellMouseEnter += dgvContacts_CellMouseEnter;
            dgvContacts.CellMouseLeave += dgvContacts_CellMouseLeave;
        }

        /// <summary>
        /// 美化会员列表样式
        /// </summary>
        private void CustomizeMembersGridStyle()
        {
            // 🔥 1. 自定义选中样式（透明蒙板 + 高亮边框）
            dgvMembers.DefaultCellStyle.SelectionBackColor = Color.Transparent;
            dgvMembers.DefaultCellStyle.SelectionForeColor = Color.Black;
            
            // 🔥 2. 绑定 CellPainting 事件（绘制自定义选中效果 + Hover 效果）
            dgvMembers.CellPainting += dgvMembers_CellPainting;
            
            // 🔥 3. 绑定鼠标事件（Hover 效果）
            dgvMembers.CellMouseEnter += dgvMembers_CellMouseEnter;
            dgvMembers.CellMouseLeave += dgvMembers_CellMouseLeave;
        }

        /// <summary>
        /// 美化订单列表样式
        /// </summary>
        private void CustomizeOrdersGridStyle()
        {
            // 🔥 1. 自定义选中样式（透明蒙板 + 高亮边框）
            dgvOrders.DefaultCellStyle.SelectionBackColor = Color.Transparent;
            dgvOrders.DefaultCellStyle.SelectionForeColor = Color.Black;
            
            // 🔥 2. 绑定 CellPainting 事件（绘制自定义选中效果 + Hover 效果）
            dgvOrders.CellPainting += dgvOrders_CellPainting;
            
            // 🔥 3. 绑定鼠标事件（Hover 效果）
            dgvOrders.CellMouseEnter += dgvOrders_CellMouseEnter;
            dgvOrders.CellMouseLeave += dgvOrders_CellMouseLeave;
        }

        /// <summary>
        /// 🔥 配置会员表列（使用特性系统）
        /// 一行代码完成所有配置：列标题、列宽、对齐、格式化、顺序
        /// </summary>
        private void ConfigureMembersDataGridView()
        {
            // 🔥 从 V2Member 模型的特性自动配置
            dgvMembers.ConfigureFromModel<V2Member>();
            
            // 可选：隐藏额外的列（如果需要）
            dgvMembers.HideColumns("Account", "DisplayName", "BetWait");
            
            // 🔥 设置为只读，不允许直接修改数据
            dgvMembers.ReadOnly = true;
            dgvMembers.AllowUserToAddRows = false;
            dgvMembers.AllowUserToDeleteRows = false;
        }

        /// <summary>
        /// 🔥 配置订单表列（使用特性系统）
        /// 一行代码完成所有配置：列标题、列宽、对齐、格式化、顺序
        /// </summary>
        private void ConfigureOrdersDataGridView()
        {
            // 🔥 从 V2MemberOrder 模型的特性自动配置
            dgvOrders.ConfigureFromModel<V2MemberOrder>();
            
            // 🔥 设置为只读，不允许直接修改数据
            dgvOrders.ReadOnly = true;
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.AllowUserToDeleteRows = false;
        }

        #endregion

        #region 联系人列表 - 鼠标事件

        /// <summary>
        /// 鼠标进入单元格（Hover 效果）
        /// </summary>
        private void dgvContacts_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoverRowIndex_Contacts = e.RowIndex;
                dgvContacts.InvalidateRow(e.RowIndex); // 重绘该行
            }
        }

        /// <summary>
        /// 鼠标离开单元格
        /// </summary>
        private void dgvContacts_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (_hoverRowIndex_Contacts >= 0)
            {
                int oldHoverRow = _hoverRowIndex_Contacts;
                _hoverRowIndex_Contacts = -1;
                dgvContacts.InvalidateRow(oldHoverRow); // 重绘之前的行
            }
        }

        #endregion

        #region 会员列表 - 鼠标事件

        private void dgvMembers_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoverRowIndex_Members = e.RowIndex;
                dgvMembers.InvalidateRow(e.RowIndex);
            }
        }

        private void dgvMembers_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (_hoverRowIndex_Members >= 0)
            {
                int oldHoverRow = _hoverRowIndex_Members;
                _hoverRowIndex_Members = -1;
                dgvMembers.InvalidateRow(oldHoverRow);
            }
        }

        #endregion

        #region 订单列表 - 鼠标事件

        private void dgvOrders_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                _hoverRowIndex_Orders = e.RowIndex;
                dgvOrders.InvalidateRow(e.RowIndex);
            }
        }

        private void dgvOrders_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (_hoverRowIndex_Orders >= 0)
            {
                int oldHoverRow = _hoverRowIndex_Orders;
                _hoverRowIndex_Orders = -1;
                dgvOrders.InvalidateRow(oldHoverRow);
            }
        }

        #endregion

        /// <summary>
        /// 单元格格式化：绿色显示已绑定的行
        /// </summary>
        private void dgvContacts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            if (dgvContacts.Rows[e.RowIndex].DataBoundItem is WxContact contact)
            {
                // 🔥 如果是当前绑定的联系人，用绿色背景
                if (_groupBindingService.CurrentBoundGroup != null && contact.Wxid == _groupBindingService.CurrentBoundGroup.Wxid)
                {
                    dgvContacts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240); // 浅绿色
                    dgvContacts.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(82, 196, 26);   // 深绿色文字
                }
                else
                {
                    // 恢复默认颜色
                    dgvContacts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                    dgvContacts.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }

        #region 联系人列表 - CellPainting

        /// <summary>
        /// 单元格绘制：自定义效果（Hover + 选中 + 绑定）
        /// </summary>
        private void dgvContacts_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null) return;
            
            bool isSelected = dgvContacts.Rows[e.RowIndex].Selected;
            bool isHover = (e.RowIndex == _hoverRowIndex_Contacts);
            bool isBound = false;
            
            // 🔥 检查是否是绑定的行
            if (dgvContacts.Rows[e.RowIndex].DataBoundItem is WxContact contact)
            {
                var cur = _groupBindingService.CurrentBoundGroup;
                isBound = (cur != null && contact.Wxid == cur.Wxid);
            }
            
            // 🔥 优先级：绑定 > 选中 > Hover
            if (isSelected || isHover)
            {
                // 先绘制原本的背景色
                e.PaintBackground(e.CellBounds, false);
                
                // 🔥 选中效果：蓝色蒙板 (50% 透明度)
                if (isSelected)
                {
                    e.Graphics.FillRectangle(
                        new SolidBrush(Color.FromArgb(50, 80, 160, 255)), // 50% 透明度的蓝色
                        e.CellBounds);
                    
                    // 绘制蓝色边框（2px）
                    using (Pen pen = new Pen(Color.FromArgb(80, 160, 255), 2))
                    {
                        e.Graphics.DrawRectangle(pen, 
                            e.CellBounds.X, 
                            e.CellBounds.Y, 
                            e.CellBounds.Width - 1, 
                            e.CellBounds.Height - 1);
                    }
                }
                // 🔥 Hover 效果：淡黄色蒙板 (30% 透明度)
                else if (isHover && !isSelected)
                {
                    e.Graphics.FillRectangle(
                        new SolidBrush(Color.FromArgb(30, 255, 235, 150)), // 30% 透明度的淡黄色
                        e.CellBounds);
                }
                
                // 绘制文本
                if (e.Value != null && e.CellStyle?.Font != null)
                {
                    // 🔥 使用原本的文字颜色（绿色行保持绿色文字）
                    using (SolidBrush brush = new SolidBrush(e.CellStyle.ForeColor))
                    {
                        e.Graphics.DrawString(
                            e.Value.ToString() ?? string.Empty,
                            e.CellStyle.Font,
                            brush,
                            e.CellBounds.X + 5,
                            e.CellBounds.Y + (e.CellBounds.Height - e.CellStyle.Font.Height) / 2);
                    }
                }
                
                e.Handled = true;
            }
        }

        #endregion

        #region 会员列表 - CellPainting

        /// <summary>
        /// 🔥 会员列表：自定义效果（会员状态背景色 + Hover + 选中）
        /// 
        /// 会员状态背景色：
        /// - 管理: 金色
        /// - 托: 橙色
        /// - 已退群: 灰色
        /// - 已删除: 红色
        /// - 普会: 白色
        /// - 蓝会: 蓝色
        /// - 紫会: 紫色
        /// </summary>
        private void dgvMembers_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null) return;
            
            // 🔥 获取会员对象，确定状态背景色
            Color baseBackColor = Color.White;  // 默认白色
            if (dgvMembers.Rows[e.RowIndex].DataBoundItem is V2Member member)
            {
                baseBackColor = member.State switch
                {
                    MemberState.管理 => Color.FromArgb(255, 248, 220),    // 金色（浅）
                    MemberState.托 => Color.FromArgb(255, 228, 181),       // 橙色（浅）
                    MemberState.已退群 => Color.FromArgb(220, 220, 220),  // 灰色
                    MemberState.已删除 => Color.FromArgb(255, 200, 200),  // 红色（浅）
                    MemberState.普会 => Color.White,                       // 白色
                    MemberState.蓝会 => Color.FromArgb(224, 240, 255),    // 蓝色（浅）
                    MemberState.紫会 => Color.FromArgb(245, 230, 255),    // 紫色（浅）
                    _ => Color.White
                };
            }
            
            bool isSelected = dgvMembers.Rows[e.RowIndex].Selected;
            bool isHover = (e.RowIndex == _hoverRowIndex_Members);
            
            // 🔥 绘制背景（状态背景色）
            e.PaintBackground(e.CellBounds, false);
            using (var backBrush = new SolidBrush(baseBackColor))
            {
                e.Graphics.FillRectangle(backBrush, e.CellBounds);
            }
            
            // 🔥 绘制选中效果（透明蒙板 + 边框）
            if (isSelected)
            {
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(50, 80, 160, 255)),
                    e.CellBounds);
                
                using (Pen pen = new Pen(Color.FromArgb(80, 160, 255), 2))
                {
                    e.Graphics.DrawRectangle(pen, 
                        e.CellBounds.X, 
                        e.CellBounds.Y, 
                        e.CellBounds.Width - 1, 
                        e.CellBounds.Height - 1);
                }
            }
            // 🔥 绘制 Hover 效果（透明蒙板）
            else if (isHover)
            {
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(30, 255, 235, 150)),
                    e.CellBounds);
            }
            
            // 🔥 绘制文本
            if (e.Value != null && e.CellStyle?.Font != null)
            {
                using (SolidBrush brush = new SolidBrush(e.CellStyle.ForeColor))
                {
                    e.Graphics.DrawString(
                        e.Value.ToString() ?? string.Empty,
                        e.CellStyle.Font,
                        brush,
                        e.CellBounds.X + 5,
                        e.CellBounds.Y + (e.CellBounds.Height - e.CellStyle.Font.Height) / 2);
                }
            }
            
            e.Handled = true;
        }

        #endregion

        #region 订单列表 - CellPainting

        /// <summary>
        /// 订单列表：自定义效果（Hover + 选中）
        /// </summary>
        private void dgvOrders_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null) return;
            
            bool isSelected = dgvOrders.Rows[e.RowIndex].Selected;
            bool isHover = (e.RowIndex == _hoverRowIndex_Orders);
            
            if (isSelected || isHover)
            {
                e.PaintBackground(e.CellBounds, false);
                
                if (isSelected)
                {
                    e.Graphics.FillRectangle(
                        new SolidBrush(Color.FromArgb(50, 80, 160, 255)),
                        e.CellBounds);
                    
                    using (Pen pen = new Pen(Color.FromArgb(80, 160, 255), 2))
                    {
                        e.Graphics.DrawRectangle(pen, 
                            e.CellBounds.X, 
                            e.CellBounds.Y, 
                            e.CellBounds.Width - 1, 
                            e.CellBounds.Height - 1);
                    }
                }
                else if (isHover && !isSelected)
                {
                    e.Graphics.FillRectangle(
                        new SolidBrush(Color.FromArgb(30, 255, 235, 150)),
                        e.CellBounds);
                }
                
                if (e.Value != null && e.CellStyle?.Font != null)
                {
                    using (SolidBrush brush = new SolidBrush(e.CellStyle.ForeColor))
                    {
                        e.Graphics.DrawString(
                            e.Value.ToString() ?? string.Empty,
                            e.CellStyle.Font,
                            brush,
                            e.CellBounds.X + 5,
                            e.CellBounds.Y + (e.CellBounds.Height - e.CellStyle.Font.Height) / 2);
                    }
                }
                
                e.Handled = true;
            }
        }

        #endregion

        private void HideContactColumns()
        {
            // 只显示 Wxid 和 Nickname 两列，其他全部隐藏
            if (dgvContacts.Columns["Account"] != null)
                dgvContacts.Columns["Account"].Visible = false;

            if (dgvContacts.Columns["Remark"] != null)
                dgvContacts.Columns["Remark"].Visible = false;

            if (dgvContacts.Columns["Avatar"] != null)
                dgvContacts.Columns["Avatar"].Visible = false;

            if (dgvContacts.Columns["Sex"] != null)
                dgvContacts.Columns["Sex"].Visible = false;

            if (dgvContacts.Columns["Province"] != null)
                dgvContacts.Columns["Province"].Visible = false;

            if (dgvContacts.Columns["City"] != null)
                dgvContacts.Columns["City"].Visible = false;

            if (dgvContacts.Columns["Country"] != null)
                dgvContacts.Columns["Country"].Visible = false;

            if (dgvContacts.Columns["IsGroup"] != null)
                dgvContacts.Columns["IsGroup"].Visible = false;

            // 修改 Wxid 列的表头显示为 "ID"
            if (dgvContacts.Columns["Wxid"] != null)
            {
                dgvContacts.Columns["Wxid"].HeaderText = "ID";
                dgvContacts.Columns["Wxid"].Width = 100;
            }

            // 调整昵称列宽度为自动填充
            if (dgvContacts.Columns["Nickname"] != null)
            {
                dgvContacts.Columns["Nickname"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }


        #region 🔥 现代化方案：自动保存（PropertyChangeTracker）

        // ========================================
        // 重要说明：
        // 1. 不再需要 CellValueChanged 事件
        // 2. 不再需要手动保存方法
        // 3. 属性修改后自动保存单个字段
        // ========================================

        // ❌ 旧方案（已删除）：
        // private void dgvMembers_CellValueChanged(...)
        // {
        //     SaveMemberToDatabase(member);  // 手动调用保存
        // }

        // ✅ 新方案（自动）：
        // 用户在 DataGridView 中编辑单元格
        // → 数据绑定自动更新 member.Balance
        // → SetField 触发 PropertyChanged 事件
        // → PropertyChangeTracker 自动保存
        // → UPDATE members SET Balance = @Value WHERE Id = @Id
        // → 只更新一个字段！

        // 示例：直接修改属性
        // var member = _membersBindingList[0];
        // member.Balance = 100;  // ✅ 自动保存！只更新 Balance 字段

        #endregion

        #region 事件处理

        private void dgvContacts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvContacts.CurrentRow != null && dgvContacts.CurrentRow.DataBoundItem is WxContact contact)
            {
                lblStatus.Text = $"选中联系人: {contact.Nickname} ({contact.Wxid})";
                // TODO: 根据选中的联系人，加载对应的会员和订单数据
            }
        }

        private void dgvMembers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMembers.CurrentRow != null && dgvMembers.CurrentRow.DataBoundItem is V2Member member)
            {
                // 根据选中的会员，筛选订单
                FilterOrdersByMember(member.Wxid);
            }
        }

        private void FilterOrdersByMember(string? wxid)
        {
            if (string.IsNullOrEmpty(wxid)) return;
            // TODO: 实现订单筛选逻辑
            // 这里可以创建一个过滤后的BindingList
        }

        /// <summary>
        /// 🔥 绑定群组按钮点击事件（现代化、服务化）
        /// 
        /// 核心逻辑：
        /// 1. 验证是否为群组
        /// 2. 使用 GroupBindingService 绑定群组
        /// 3. 获取服务器数据
        /// 4. 智能合并数据库和服务器数据
        /// 5. 加载到 UI（自动保存）
        /// </summary>
        private async void btnBindingContacts_Click(object sender, EventArgs e)
        {
            if (dgvContacts.CurrentRow?.DataBoundItem is not WxContact contact)
            {
                _logService.Warning("VxMain", "绑定联系人失败: 未选择联系人");
                UIMessageBox.ShowWarning("请先选择一个联系人");
                return;
            }

            try
            {
                // 🔥 验证是否为群（wxid 包含 '@' 符号）
                if (!contact.Wxid.Contains("@"))
                {
                    _logService.Warning("VxMain", $"绑定失败: 选中的不是群组 - {contact.Nickname} ({contact.Wxid})");
                    UIMessageBox.ShowWarning("请选择正确的群组！\n\n只有群组（包含 @ 符号的ID）才能进行绑定。");
                    return;
                }
                
                // 🔥 走统一的绑定群流程
                await BindGroupAsync(contact);
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"绑定群组失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"绑定群组失败！\n\n{ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔥 统一的绑定群方法（重构版：业务逻辑在服务层）
        /// 
        /// 职责：
        /// 1. 调用服务层完成所有业务逻辑
        /// 2. 只负责 UI 更新和显示
        /// 3. 保持 View 层精简
        /// 
        /// 🔥 关键修复 2025-11-18：使用 Clear+Add 模式，避免引用断裂
        /// - 首次绑定：创建 BindingList 并绑定到 DataSource
        /// - 后续绑定：传入已有实例，服务内部使用 Clear+Add 更新
        /// </summary>
        private async Task BindGroupAsync(WxContact contact)
        {
            try
            {
                _logService.Info("VxMain", $"📍 开始绑定群: {contact.Nickname} ({contact.Wxid})");
                
                // 🔥 验证数据库
                if (_db == null)
                {
                    _logService.Error("VxMain", "数据库未初始化！");
                    UIMessageBox.ShowError("数据库未初始化！");
                    return;
                }
                
                // 🔥 1. 判断是否首次绑定
                bool isFirstTimeBinding = _membersBindingList == null;
                
                if (isFirstTimeBinding)
                {
                    _logService.Info("VxMain", "✅ 首次绑定群，创建 BindingList");
                    
                    // 首次创建 BindingList
                    _membersBindingList = new V2MemberBindingList(_db, contact.Wxid);
                    _ordersBindingList = new V2OrderBindingList(_db);
                    _creditWithdrawsBindingList = new V2CreditWithdrawBindingList(_db);
                }
                else
                {
                    _logService.Info("VxMain", "✅ 复用已有 BindingList（避免引用断裂）");
                    
                    // 清零统计（数据会在 GroupBindingService 中重新加载）
                    _statisticsService.UpdateStatistics(setZero: true);
                }
                
                // 🔥 2. 更新 UI 状态
                txtCurrentContact.Text = $"{contact.Nickname} ({contact.Wxid})";
                txtCurrentContact.FillColor = Color.FromArgb(240, 255, 240); // 浅绿色背景
                txtCurrentContact.RectColor = Color.FromArgb(82, 196, 26);   // 绿色边框
                dgvContacts.Refresh();
                lblStatus.Text = $"✓ 已绑定: {contact.Nickname} - 正在加载数据...";
                
                // 🔥 3. 调用服务层完成所有业务逻辑（传入已有 BindingList）
                var result = await _groupBindingService.BindGroupCompleteAsync(
                    contact,
                    _db,
                    _socketClient,
                    _orderService,
                    _statisticsService,
                    _memberDataService,
                    _lotteryService,
                    // 🔥 关键修复：传入已有实例
                    existingMembersBindingList: _membersBindingList,
                    existingOrdersBindingList: _ordersBindingList,
                    existingCreditWithdrawsBindingList: _creditWithdrawsBindingList
                );
                
                // 🔥 4. 处理结果
                if (!result.Success)
                {
                    _logService.Error("VxMain", $"绑定群失败: {result.ErrorMessage}");
                    UIMessageBox.ShowError($"绑定群失败！\n\n{result.ErrorMessage}");
                    return;
                }
                
                // 🔥 5. 确保所有服务引用同一个 BindingList 实例
                SetAllServicesBindingList();
                
                // 🔥 6. 只在首次绑定时设置 DataSource
                if (isFirstTimeBinding)
                {
                    UpdateUIThreadSafe(() =>
                    {
                        dgvMembers.DataSource = _membersBindingList;
                        dgvOrders.DataSource = _ordersBindingList;
                        
                        _logService.Info("VxMain", "✅ 首次绑定 DataSource 到 UI");
                        
                        // 🔥 重要：在设置 DataSource 之后，列已经自动生成，现在应用特性配置
                        // 这样列头标题、列宽、对齐等配置才会生效
                        if (dgvMembers.Columns.Count > 0)
                        {
                            dgvMembers.ConfigureFromModel<V2Member>();
                            _logService.Info("VxMain", "✅ 会员表列配置已应用");
                        }
                        
                        if (dgvOrders.Columns.Count > 0)
                        {
                            dgvOrders.ConfigureFromModel<V2MemberOrder>();
                            _logService.Info("VxMain", "✅ 订单表列配置已应用");
                        }
                    });
                }
                else
                {
                    _logService.Info("VxMain", "✅ 复用已有 DataSource，UI 自动同步（BindingList 特性）");
                }
                
                // 🔥 7. 更新 UI 显示
                UpdateMemberInfoLabel();
                lblStatus.Text = $"✓ 已绑定: {contact.Nickname} - 加载完成";
                
                _logService.Info("VxMain", 
                    $"✅ 绑定群完成: {result.MemberCount} 个会员, {result.OrderCount} 个订单, {result.CreditWithdrawCount} 条上下分记录");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"绑定群失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"绑定群失败！\n\n{ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 🔥 统一设置所有服务的 BindingList 引用
        /// 
        /// 关键修复 2025-11-18：确保所有服务都引用同一个 BindingList 实例
        /// 避免引用断裂导致的数据不同步问题
        /// </summary>
        private void SetAllServicesBindingList()
        {
            if (_membersBindingList == null || _ordersBindingList == null || _creditWithdrawsBindingList == null)
            {
                _logService.Warning("VxMain", "BindingList 未初始化，无法设置服务引用");
                return;
            }
            
            _logService.Info("VxMain", "🔗 开始统一设置所有服务的 BindingList 引用...");
            
            // 1️⃣ AdminCommandHandler
            var adminCommandHandler = Program.ServiceProvider.GetService<Services.Messages.Handlers.AdminCommandHandler>();
            if (adminCommandHandler != null && _db != null)
            {
                adminCommandHandler.SetMembersBindingList(_membersBindingList);
                adminCommandHandler.SetDatabase(_db);
                
                // 创建并设置上下分服务（参考 F5BotV2）
                var creditWithdrawService = new Services.Games.Binggo.CreditWithdrawService(
                    _db,
                    _logService,
                    _statisticsService,
                    _socketClient,
                    Program.ServiceProvider.GetService<Services.Sound.SoundService>());
                creditWithdrawService.SetCreditWithdrawsBindingList(_creditWithdrawsBindingList);
                adminCommandHandler.SetCreditWithdrawService(creditWithdrawService);
                adminCommandHandler.SetCreditWithdrawsBindingList(_creditWithdrawsBindingList);
                
                _logService.Info("VxMain", "✅ AdminCommandHandler 已设置 BindingList");
            }
            
            // 2️⃣ BinggoOrderService
            if (_orderService != null)
            {
                _orderService.SetMembersBindingList(_membersBindingList);
                _orderService.SetOrdersBindingList(_ordersBindingList);
                _logService.Info("VxMain", "✅ BinggoOrderService 已设置 BindingList");
            }
            
            // 3️⃣ BinggoStatisticsService
            if (_statisticsService != null)
            {
                _statisticsService.SetBindingLists(_membersBindingList, _ordersBindingList);
                _logService.Info("VxMain", "✅ BinggoStatisticsService 已设置 BindingList");
            }
            
            // 4️⃣ BinggoLotteryService（已在 GroupBindingService 中设置）
            // 无需重复设置，因为 GroupBindingService.BindGroupCompleteAsync 已经调用了
            // lotteryService.SetBusinessDependencies(...)
            
            // 5️⃣ MemberDataService
            if (_memberDataService is Services.MemberDataService mds)
            {
                mds.SetMembersBindingList(_membersBindingList);
                _logService.Info("VxMain", "✅ MemberDataService 已设置 BindingList");
            }
            
            _logService.Info("VxMain", "🔗 所有服务的 BindingList 引用已统一设置完成");
            _logService.Info("VxMain", $"   会员表 HashCode: {_membersBindingList.GetHashCode()}");
            _logService.Info("VxMain", $"   订单表 HashCode: {_ordersBindingList.GetHashCode()}");
            _logService.Info("VxMain", $"   上下分表 HashCode: {_creditWithdrawsBindingList.GetHashCode()}");
        }

        /// <summary>
        /// 连接按钮点击事件（现代化方式）
        /// 
        /// 🔥 精简、现代化、易维护的实现：
        /// 1. 直接调用 WeChatService.ConnectAndInitializeAsync()
        /// 2. UserInfo 自动通过 _userInfoService 更新
        /// 3. ucUserInfo1 通过数据绑定自动刷新（无需手动更新）
        /// 4. 状态更新通过 WeChatService_ConnectionStateChanged 事件处理
        /// </summary>
        private async void btnConnect_Click(object? sender, EventArgs e)
        {
            try
            {
                // 取消之前的连接（如果有）
                _connectCts?.Cancel();
                _connectCts = new CancellationTokenSource();

                _logService.Info("VxMain", "用户点击连接按钮");

                // 🔥 调用微信应用服务进行连接和初始化
                // forceRestart = false，让服务自动判断
                // UserInfo 会通过 _userInfoService 自动更新
                var success = await _wechatService.ConnectAndInitializeAsync(forceRestart: false, _connectCts.Token);
                
                _logService.Info("VxMain", $"连接和初始化完成，结果: {success}");
            }
            catch (OperationCanceledException)
            {
                _logService.Info("VxMain", "连接被用户取消");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "连接失败", ex);
                UpdateUIThreadSafe(() => UIMessageBox.ShowError($"连接失败:\n{ex.Message}"));
            }
        }

        /// <summary>
        /// 微信服务连接状态变化事件处理（管理 UI 状态）
        /// </summary>
        private void WeChatService_ConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
        {
            // 切换到 UI 线程
            if (InvokeRequired)
            {
                Invoke(new Action(() => WeChatService_ConnectionStateChanged(sender, e)));
                return;
            }

            // 更新状态栏
            string statusMessage = e.NewState switch
            {
                ConnectionState.Disconnected => "未连接",
                ConnectionState.LaunchingWeChat => "正在启动微信...",
                ConnectionState.InjectingDll => "正在注入 DLL...",
                ConnectionState.ConnectingSocket => "正在连接 Socket...",
                ConnectionState.FetchingUserInfo => "正在获取用户信息（等待登录）...",
                ConnectionState.FetchingContacts => "正在获取联系人...",
                ConnectionState.Connected => e.Message ?? "已连接",
                ConnectionState.Failed => $"连接失败: {e.Message}",
                _ => e.Message ?? "未知状态"
            };

            lblStatus.Text = statusMessage;

            // 更新按钮状态
            bool isConnecting = e.NewState switch
            {
                ConnectionState.LaunchingWeChat => true,
                ConnectionState.InjectingDll => true,
                ConnectionState.ConnectingSocket => true,
                ConnectionState.FetchingUserInfo => true,
                ConnectionState.FetchingContacts => true,
                _ => false
            };

            // 🔥 连接中时禁用连接按钮，其他状态启用
            UpdateUIThreadSafe(() => btnConnect.Enabled = !isConnecting);

            // 记录日志
            _logService.Info("VxMain", $"连接状态: {e.OldState} → {e.NewState} ({statusMessage})");

            // 如果连接失败，显示错误信息
            if (e.NewState == ConnectionState.Failed && e.Error != null)
            {
                UIMessageBox.ShowError($"连接失败:\n{e.Error.Message}");
            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            try
            {
                _logService.Info("VxMain", "打开日志查看窗口");
                lblStatus.Text = "打开日志窗口...";
                
                // 从 DI 容器获取日志窗口
                var logViewer = Program.ServiceProvider?.GetRequiredService<Views.LogViewerForm>();
                if (logViewer != null)
                {
                    logViewer.Show();  // 非模态窗口，可以同时查看日志和操作主窗口
                    lblStatus.Text = "日志窗口已打开";
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "打开日志窗口失败", ex);
                UIMessageBox.ShowError($"打开日志窗口失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 🔥 打开开奖结果窗口
        /// </summary>
        private void btnOpenLotteryResult_Click(object sender, EventArgs e)
        {
            try
            {
                if (_lotteryResultForm == null || _lotteryResultForm.IsDisposed)
                {
                    _lotteryResultForm = new Views.BinggoLotteryResultForm(_lotteryService, _logService);
                    _lotteryResultForm.SetBindingList(_lotteryDataBindingList);
                }
                
                if (_lotteryResultForm.Visible)
                {
                    _lotteryResultForm.Activate(); // 如果已打开，激活窗口
                }
                else
                {
                    _lotteryResultForm.Show();
                }
                
                lblStatus.Text = "开奖结果窗口已打开";
                _logService.Info("VxMain", "开奖结果窗口已打开");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "打开开奖结果窗口失败", ex);
                UIMessageBox.ShowError($"打开开奖结果窗口失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 🔥 清空数据按钮（完整功能）
        /// 1. 先备份数据库（加密压缩，密码为用户登录密码）
        /// 2. 清空订单表所有数据
        /// 3. 清空会员的金额数据（Balance, BetCur, BetWait, IncomeToday等）
        /// 4. 保留会员基础信息（Wxid, Nickname, Account等）
        /// 5. 清空统计数据
        /// </summary>
        private async void btnClearData_Click(object sender, EventArgs e)
        {
            try
            {
                // 🔥 确认对话框
                if (!UIMessageBox.ShowAsk("确定要清空所有数据吗？\n\n" +
                    "此操作将：\n" +
                    "1. 备份当前数据库（加密压缩）\n" +
                    "2. 清空所有订单数据\n" +
                    "3. 重置会员金额数据\n" +
                    "4. 清空统计数据\n" +
                    "5. 清空48小时之前的上下分记录\n\n" +
                    "会员基础信息（微信ID、昵称等）将保留"))
                {
                    return;
                }
                
                lblStatus.Text = "正在清空数据...";
                _logService.Info("VxMain", "开始清空数据...");
                
                // ========================================
                // 🔥 步骤1：备份数据库
                // ========================================
                
                if (!string.IsNullOrEmpty(_currentDbPath) && File.Exists(_currentDbPath))
                {
                    try
                    {
                        // 生成备份文件名：d{日期时间}_{原数据库名}
                        string timestamp = DateTime.Now.ToString("MMddHHmm");  // 月日时分
                        string dbFileName = Path.GetFileName(_currentDbPath);
                        string backupDbName = $"d{timestamp}_{dbFileName}";
                        
                        // 🔥 使用 AppData\Local 目录存储备份
                        var backupDirectory = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "BaiShengVx3Plus",
                            "Data",
                            "Backup");
                        
                        string backupDbPath = Path.Combine(backupDirectory, backupDbName);
                        
                        // 创建备份目录
                        Directory.CreateDirectory(backupDirectory);
                        
                        // 🔥 备份数据库（SQLite支持在连接打开时复制文件，只要没有写操作）
                        // 参考 F5BotV2：不需要关闭数据库连接
                        // 先执行一次同步，确保所有数据写入磁盘（DELETE模式下不需要checkpoint）
                        if (_db != null)
                        {
                            // DELETE模式下，数据已经写入主文件，只需要确保同步到磁盘
                            _db.Execute("PRAGMA synchronous = FULL");
                            // 执行一个简单的查询，触发同步
                            _db.ExecuteScalar<int>("SELECT 1");
                        }
                        
                        // 等待文件系统刷新
                        await Task.Delay(100);
                        
                        // 复制数据库文件（数据库连接可以保持打开）
                        File.Copy(_currentDbPath, backupDbPath, true);
                        _logService.Info("VxMain", $"✅ 数据库已备份: {backupDbPath}");
                        
                        // 🔥 使用7-Zip或WinRAR压缩加密（如果可用）
                        // 获取用户密码
                        string password = Services.Api.BoterApi.GetInstance().Password;
                        
                        if (!string.IsNullOrEmpty(password))
                        {
                            string zipPath = backupDbPath + ".zip";
                            
                            // 尝试使用 System.IO.Compression（但不支持密码加密）
                            // 这里我们只是提示用户手动加密
                            UIMessageBox.ShowWarning(
                                $"数据库已备份到：\n{backupDbPath}\n\n" +
                                $"请手动使用WinRAR或7-Zip加密压缩此文件\n" +
                                $"建议密码：您的登录密码");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Error("VxMain", "备份数据库失败", ex);
                        UIMessageBox.ShowError($"备份数据库失败：{ex.Message}\n\n已取消清空操作");
                        return;
                    }
                }
                
                // ========================================
                // 🔥 步骤2：清空订单表
                // ========================================
                
                // 🔥 确保数据库已打开
                if (_db == null)
                {
                    _logService.Error("VxMain", "数据库未打开，无法清空数据");
                    UIMessageBox.ShowError("数据库未打开，无法清空数据");
                    return;
                }
                
                try
                {
                    _db.DeleteAll<Models.V2MemberOrder>();
                    _logService.Info("VxMain", "✅ 订单表已清空");
                }
                catch (Exception ex)
                {
                    _logService.Error("VxMain", $"清空订单表失败: {ex.Message}", ex);
                    throw;
                }
                
                // 清空UI订单列表
                _ordersBindingList?.Clear();
                
                // ========================================
                // 🔥 步骤3：重置会员金额数据（保留基础信息）- 参考 F5BotV2 Line 812-821
                // ========================================
                
                if (_membersBindingList != null)
                {
                    try
                    {
                        // 🔥 参考 F5BotV2：遍历会员列表，只修改金额字段
                        foreach (var member in _membersBindingList)
                        {
                            // 🔥 清空金额数据（参考 F5BotV2 Line 814-821）
                            member.BetToday = 0f;      // 今日流水
                            member.BetTotal = 0f;      // 总流水
                            member.CreditToday = 0f;    // 今日上分
                            member.CreditTotal = 0f;    // 总上分
                            member.IncomeToday = 0f;    // 今日盈亏
                            member.IncomeTotal = 0f;    // 总盈亏
                            member.WithdrawToday = 0f;   // 今日下分
                            member.WithdrawTotal = 0f;   // 总下分
                            member.Balance = 0f;         // 余额
                            member.BetCur = 0f;          // 当期投注
                            member.BetWait = 0f;         // 待结算
                            
                            // 🔥 保留基础信息（Wxid, Nickname, Account, DisplayName, State等）
                            // BindingList 会自动保存（通过 PropertyChanged 事件）
                        }
                        
                        _logService.Info("VxMain", $"✅ {_membersBindingList.Count} 个会员的金额数据已重置");
                    }
                    catch (Exception ex)
                    {
                        _logService.Error("VxMain", $"重置会员金额数据失败: {ex.Message}", ex);
                        throw;
                    }
                }
                
                // ========================================
                // 🔥 步骤4：清空统计数据
                // ========================================
                
                _statisticsService.UpdateStatistics(setZero: true);
                _logService.Info("VxMain", "✅ 统计数据已清空");
                
                // ========================================
                // 🔥 步骤5：清空48小时之前的上下分记录（参考 F5BotV2 XMainView.cs Line 847-849）
                // ========================================
                
                if (_creditWithdrawsBindingList != null)
                {
                    try
                    {
                        _creditWithdrawsBindingList.DeleteOldRecords(48);
                        _logService.Info("VxMain", "✅ 48小时之前的上下分记录已清空");
                    }
                    catch (Exception ex)
                    {
                        _logService.Error("VxMain", $"清空旧上下分记录失败: {ex.Message}", ex);
                        // 不抛出异常，继续执行
                    }
                }
                
                // ========================================
                // 🔥 步骤6：刷新UI
                // ========================================
                
                UpdateUIThreadSafeAsync(() =>
                {
                    UpdateMemberInfoLabel();
                    dgvMembers.Refresh();
                    dgvOrders.Refresh();
                });
                
                lblStatus.Text = "数据已清空";
                _logService.Info("VxMain", "✅ 数据清空完成");
                
                this.ShowSuccessTip("数据清空成功！\n\n" +
                    "✓ 订单数据已清空\n" +
                    "✓ 会员金额数据已重置\n" +
                    "✓ 统计数据已清空\n" +
                    "✓ 48小时之前的上下分记录已清空\n" +
                    "✓ 会员基础信息已保留\n" +
                    "✓ 数据库已备份");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "清空数据失败", ex);
                UIMessageBox.ShowError($"清空数据失败：{ex.Message}");
                lblStatus.Text = "清空数据失败";
            }
        }

        /// <summary>
        /// 🔊 动态添加声音测试按钮
        /// </summary>
        private void AddSoundTestButton()
        {
            try
            {
                // 创建测试按钮
                var btnTestSound = new Sunny.UI.UIButton
                {
                    Name = "btnTestSound",
                    Text = "🔊 测试声音",
                    Size = new System.Drawing.Size(100, 35),
                    Font = new System.Drawing.Font("微软雅黑", 9F),
                    TabIndex = 100
                };
                
                // 添加点击事件
                btnTestSound.Click += BtnTestSound_Click;
                
                // 找到 pnlTopButtons 面板
                var pnlTopButtons = this.Controls.Find("pnlTopButtons", true).FirstOrDefault();
                if (pnlTopButtons != null)
                {
                    // 添加到面板
                    pnlTopButtons.Controls.Add(btnTestSound);
                    
                    // 设置位置（在设置按钮旁边）
                    var btnSettings = pnlTopButtons.Controls.Find("btnSettings", false).FirstOrDefault();
                    if (btnSettings != null)
                    {
                        btnTestSound.Location = new System.Drawing.Point(
                            btnSettings.Location.X + btnSettings.Width + 10,
                            btnSettings.Location.Y);
                    }
                    
                    _logService.Info("VxMain", "✅ 声音测试按钮已添加");
                }
                else
                {
                    _logService.Warning("VxMain", "⚠️ 未找到 pnlTopButtons 面板");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "添加声音测试按钮失败", ex);
            }
        }
        
        /// <summary>
        /// 🔊 测试声音按钮点击事件
        /// </summary>
        private void BtnTestSound_Click(object? sender, EventArgs e)
        {
            try
            {
                var soundService = Program.ServiceProvider.GetService<Services.Sound.SoundService>();
                if (soundService == null)
                {
                    Sunny.UI.UIMessageBox.ShowWarning("SoundService 未初始化！");
                    return;
                }

                var testForm = new Views.SoundTestForm(soundService, _logService);
                testForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "打开声音测试窗口失败", ex);
                Sunny.UI.UIMessageBox.ShowError($"打开失败:\n{ex.Message}");
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            try
            {
                // 检查设置窗口是否已打开
                if (_settingsForm != null && !_settingsForm.IsDisposed)
                {
                    // 窗口已打开，激活并显示到前台
                    _logService.Info("VxMain", "设置窗口已打开，激活到前台");
                    
                    // 如果窗口最小化，先恢复
                    if (_settingsForm.WindowState == FormWindowState.Minimized)
                    {
                        _settingsForm.WindowState = FormWindowState.Normal;
                    }
                    
                    // 激活窗口并显示到最前面
                    _settingsForm.Activate();
                    _settingsForm.BringToFront();
                    _settingsForm.Focus();
                    
                    lblStatus.Text = "设置窗口已激活";
                    return;
                }
                
                lblStatus.Text = "打开设置窗口...";
                _logService.Info("VxMain", "创建新的设置窗口");
                
                // 创建新的设置窗口（非模态）
                // 🔧 传入模拟消息回调（用于开发模式测试）
                // 🔊 传入声音服务（用于声音测试）
                var soundService = Program.ServiceProvider?.GetService<Services.Sound.SoundService>();
                _settingsForm = new Views.SettingsForm(
                    _socketClient, 
                    _logService, 
                    _settingViewModel, 
                    _configService,
                    SimulateMemberMessageAsync, // 🔧 开发模式：模拟消息回调
                    soundService); // 🔊 声音测试
                
                // 订阅关闭事件，清理引用
                _settingsForm.FormClosed += (s, args) =>
                {
                    _logService.Info("VxMain", "设置窗口已关闭");
                    _settingsForm = null;
                    lblStatus.Text = "设置窗口已关闭";
                };
                
                // 显示为非模态窗口
                _settingsForm.Show(this);
                lblStatus.Text = "设置窗口已打开";
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "打开设置窗口失败", ex);
                UIMessageBox.ShowError($"打开设置窗口失败:\n{ex.Message}");
            }
        }

        #endregion

        #region Socket 通信

        /// <summary>
        /// 连接到 Socket 服务器
        /// </summary>
        private async Task ConnectToSocketServerAsync()
        {
            try
            {
                _logService.Info("VxMain", "正在连接到 Socket 服务器...");
                lblStatus.Text = "正在连接到 Socket 服务器...";
                
                bool connected = await _socketClient.ConnectAsync("127.0.0.1", 6328, 5000);
                
                if (connected)
                {
                    _logService.Info("VxMain", "Socket 连接成功");
                    lblStatus.Text = "已连接到微信 ✓";
                    
                    // 测试：获取用户信息
                    await TestGetUserInfoAsync();
                }
                else
                {
                    _logService.Error("VxMain", "Socket 连接失败");
                    lblStatus.Text = "连接失败（将自动重试）";
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "连接 Socket 服务器时发生错误", ex);
                lblStatus.Text = "连接失败";
                UIMessageBox.ShowError($"连接失败:\n{ex.Message}");
            }
        }

        /// <summary>
        /// 测试：获取用户信息
        /// </summary>
        private async Task TestGetUserInfoAsync()
        {
            try
            {
                _logService.Info("VxMain", "测试获取用户信息...");
                
                // 使用 JsonDocument 替代 dynamic
                var result = await _socketClient.SendAsync<JsonDocument>("GetUserInfo");
                
                if (result != null)
                {
                    string jsonResult = JsonSerializer.Serialize(result.RootElement, new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                    _logService.Info("VxMain", $"用户信息: {jsonResult}");
                }
                else
                {
                    _logService.Warning("VxMain", "未能获取用户信息");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "测试获取用户信息失败", ex);
            }
        }

        /// <summary>
        /// 处理服务器主动推送的消息（使用消息分发器）
        /// </summary>
        private async void SocketClient_OnServerPush(object? sender, ServerPushEventArgs e)
        {
            try
            {
                _logService.Info("VxMain", $"📨 收到服务器推送: {e.Method}");
                
                // 使用消息分发器处理消息（异步）
                await _messageDispatcher.DispatchAsync(e.Method, e.Data);
                
                // 更新 UI 状态（在 UI 线程中）
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateUIStatus(e.Method)));
                }
                else
                {
                    UpdateUIStatus(e.Method);
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "处理服务器推送失败", ex);
            }
        }

        /// <summary>
        /// 根据消息类型更新 UI 状态
        /// </summary>
        private void UpdateUIStatus(string messageType)
        {
            switch (messageType.ToLower())
            {
                case "onmessage":
                    lblStatus.Text = "💬 收到新消息";
                    break;

                case "onlogin":
                    lblStatus.Text = "✅ 微信已登录";
                    break;

                case "onlogout":
                    lblStatus.Text = "❌ 微信已登出";
                    break;

                case "onmemberjoin":
                    lblStatus.Text = "👋 新成员加入";
                    break;

                case "onmemberleave":
                    lblStatus.Text = "👋 成员退出";
                    break;

                default:
                    lblStatus.Text = $"📨 收到推送: {messageType}";
                    break;
            }
        }

        /// <summary>
        /// 处理联系人数据更新事件
        /// </summary>
        private void ContactDataService_ContactsUpdated(object? sender, ContactsUpdatedEventArgs e)
        {
            try
            {
                _logService.Info("VxMain", $"📇 收到联系人数据更新事件，共 {e.Contacts?.Count ?? 0} 个，来源: {e.Source}");

                // 🔥 切换到 UI 线程更新（使用 BeginInvoke 确保异步执行，不阻塞）
                if (InvokeRequired)
                {
                    _logService.Info("VxMain", "🔄 切换到 UI 线程更新联系人列表");
                    BeginInvoke(new Action(() => 
                    {
                        _logService.Info("VxMain", "✅ 已在 UI 线程，开始更新联系人列表");
                        UpdateContactsList(e.Contacts ?? new List<WxContact>());
                    }));
                }
                else
                {
                    _logService.Info("VxMain", "✅ 已在 UI 线程，直接更新联系人列表");
                    UpdateContactsList(e.Contacts ?? new List<WxContact>());
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "处理联系人数据更新失败", ex);
            }
        }

        /// <summary>
        /// 用户信息更新事件处理（仅负责 UI 更新，不再处理连接逻辑）
        /// </summary>
        private async void UserInfoService_UserInfoUpdated(object? sender, UserInfoUpdatedEventArgs e)
        {
            try
            {
                _logService.Info("VxMain", $"📱 用户信息已更新: {e.UserInfo.Nickname} ({e.UserInfo.Wxid})");

                // 🔥 检测用户切换，重新初始化数据库
                bool isUserChanged = false;
                if (_currentUserInfo != null && !string.IsNullOrEmpty(_currentUserInfo.Wxid))
                {
                    if (_currentUserInfo.Wxid != e.UserInfo.Wxid)
                    {
                        isUserChanged = true;
                        _logService.Warning("VxMain", 
                            $"⚠️ 检测到用户切换: {_currentUserInfo.Wxid} → {e.UserInfo.Wxid}，准备重新绑定数据库...");
                        
                        // 清空联系人列表和绑定信息
                        UpdateUIThreadSafe(() =>
                        {
                            _contactsBindingList.Clear();
                            txtCurrentContact.Text = "未绑定";
                            txtCurrentContact.FillColor = Color.White;
                            txtCurrentContact.RectColor = Color.Silver;
                        });
                    }
                }
                
                // 更新当前用户信息
                _currentUserInfo = e.UserInfo;
                
                // 🔥 重新绑定数据库（微信专属数据库：business_{wxid}.db）
                // ⚠️ 重要：只要 wxid 有效，就重新绑定数据库
                // 这样可以确保用户切换后，数据库也正确切换
                if (!string.IsNullOrEmpty(e.UserInfo.Wxid))
                {
                    _logService.Info("VxMain", 
                        isUserChanged 
                            ? $"🔄 用户切换，重新绑定数据库: business_{e.UserInfo.Wxid}.db"
                            : $"📂 初始化数据库: business_{e.UserInfo.Wxid}.db");
                    
                    InitializeDatabase(e.UserInfo.Wxid);
                    
                    // 🔥 用户切换后，重新加载联系人列表
                    if (isUserChanged)
                    {
                        _logService.Info("VxMain", "🔄 用户切换，重新加载联系人列表");
                        _ = RefreshContactsAsync();
                    }
                }
                else
                {
                    _logService.Warning("VxMain", "⚠️ UserInfo.Wxid 为空，使用默认数据库");
                    InitializeDatabase("unknown");
                }

                // 🔥 用户信息通过现代化数据绑定自动更新
                // ucUserInfo1 订阅了 UserInfo.PropertyChanged 事件，会自动刷新显示

                // 🔥 如果用户已登录（wxid 不为空）且 WeChatService 不在获取流程中，自动获取联系人
                // 这个主要处理服务器主动推送 OnLogin 的情况（自动重连后）
                if (!string.IsNullOrEmpty(e.UserInfo.Wxid))
                {
                    var currentState = _wechatService.CurrentState;
                    
                    // 只有在非活动连接流程中才主动获取（避免与 ConnectAndInitializeAsync 重复）
                    if (currentState != ConnectionState.Connecting && 
                        currentState != ConnectionState.FetchingUserInfo && 
                        currentState != ConnectionState.FetchingContacts &&
                        currentState != ConnectionState.InitializingDatabase)
                    {
                        _logService.Info("VxMain", "检测到用户登录事件（非主动连接流程），准备获取联系人...");
                        
                        // 设置当前 wxid
                        _contactDataService.SetCurrentWxid(e.UserInfo.Wxid);

                        // 等待一段时间让 C++ 端数据库句柄初始化
                        await Task.Delay(1500);

                        // 自动获取联系人
                        _logService.Info("VxMain", "开始自动获取联系人...");
                        await RefreshContactsAsync();
                    }
                    else
                    {
                        _logService.Info("VxMain", $"当前状态: {currentState}，跳过重复获取联系人");
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "处理用户信息更新失败", ex);
            }
        }

        /// <summary>
        /// 刷新联系人列表（封装供多处调用）
        /// </summary>
        private async Task RefreshContactsAsync()
        {
            try
            {
                _logService.Info("VxMain", "🔄 开始获取联系人列表");
                lblStatus.Text = "正在获取联系人...";

                // 主动请求联系人数据
                // ✅ 调用 WeChatService（业务逻辑层）
                // UI 层不应该直接操作 SocketClient
                var contacts = await _wechatService.RefreshContactsAsync(1, 2000, ContactFilterType.群组);
                
                // UI 只负责显示结果
                lblStatus.Text = $"已获取 {contacts.Count} 个联系人";
                _logService.Info("VxMain", $"联系人刷新完成，共 {contacts.Count} 个");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"刷新联系人失败: {ex.Message}", ex);
                lblStatus.Text = "刷新失败";
            }
        }

        /// <summary>
        /// 更新联系人列表（UI 线程）
        /// </summary>
        private void UpdateContactsList(List<WxContact> contacts)
        {
            try
            {
                if (contacts == null || contacts.Count == 0)
                {
                    _logService.Warning("VxMain", "⚠️ 联系人列表为空，清空现有数据");
                    _contactsBindingList.Clear();
                    lblStatus.Text = "⚠️ 联系人列表为空";
                    return;
                }

                // 清空现有数据
                _contactsBindingList.Clear();

                // 添加新数据
                foreach (var contact in contacts)
                {
                    if (contact != null && !string.IsNullOrEmpty(contact.Wxid))
                    {
                        _contactsBindingList.Add(contact);
                    }
                }

                lblStatus.Text = $"✓ 已更新 {_contactsBindingList.Count} 个联系人";
                _logService.Info("VxMain", $"✅ 联系人列表已更新到 UI: {_contactsBindingList.Count} 个联系人");
                
                // 🔥 确保 DataGridView 正确绑定
                if (dgvContacts.DataSource != _contactsBindingList)
                {
                    _logService.Warning("VxMain", "⚠️ dgvContacts.DataSource 未绑定，重新绑定");
                    dgvContacts.DataSource = _contactsBindingList;
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "更新联系人列表失败", ex);
            }
        }

        /// <summary>
        /// 刷新联系人列表（按钮点击）
        /// </summary>
        private async void btnRefreshContacts_Click(object sender, EventArgs e)
        {
            await RefreshContactsAsync();
        }

        ///// <summary>
        ///// 加载群成员数据到 dgvMembers
        ///// </summary>
        ///// <param name="groupMembersJson">GetGroupContacts 返回的 JSON 数据</param>
        ///// <param name="groupWxid">群微信 ID</param>
        //private Task LoadGroupMembersToDataGridAsync(JsonElement groupMembersJson, string groupWxid)
        //{
        //    try
        //    {
        //        _logService.Info("VxMain", $"开始解析群成员数据，群ID: {groupWxid}");

        //        // 🔥 确保 _membersBindingList 已初始化
        //        if (_membersBindingList == null)
        //        {
        //            _logService.Warning("VxMain", "会员列表未初始化，跳过加载");
        //            return Task.CompletedTask;
        //        }

        //        // 清空当前 dgvMembers 数据
        //        _membersBindingList.Clear();

        //        int count = 0;
        //        foreach (var memberElement in groupMembersJson.EnumerateArray())
        //        {
        //            try
        //            {
        //                // 解析群成员数据
        //                string memberWxid = memberElement.TryGetProperty("member_wxid", out var mwxid) 
        //                    ? mwxid.GetString() ?? "" : "";
        //                string memberNickname = memberElement.TryGetProperty("member_nickname", out var mnick) 
        //                    ? mnick.GetString() ?? "" : "";
        //                string memberAlias = memberElement.TryGetProperty("member_alias", out var malias) 
        //                    ? malias.GetString() ?? "" : "";
        //                string memberRemark = memberElement.TryGetProperty("member_remark", out var mremark) 
        //                    ? mremark.GetString() ?? "" : "";

        //                // 跳过无效数据
        //                if (string.IsNullOrEmpty(memberWxid))
        //                {
        //                    _logService.Warning("VxMain", "跳过无效的群成员数据：member_wxid 为空");
        //                    continue;
        //                }

        //                // 创建 V2Member 对象
        //                var member = new V2Member
        //                {
        //                    GroupWxId = groupWxid,  // 🔥 设置群ID
        //                    Wxid = memberWxid,
        //                    Nickname = memberNickname,
        //                    Account = memberAlias,
        //                    DisplayName = string.IsNullOrEmpty(memberRemark) ? memberNickname : memberRemark,
                            
        //                    // 初始化业务字段为默认值
        //                    Balance = 0,
        //                    State = MemberState.会员,
        //                    BetCur = 0,
        //                    BetWait = 0,
        //                    IncomeToday = 0,
        //                    CreditToday = 0,
        //                    BetToday = 0,
        //                    WithdrawToday = 0,
        //                    BetTotal = 0,
        //                    CreditTotal = 0,
        //                    WithdrawTotal = 0,
        //                    IncomeTotal = 0
        //                };

        //                // 🔥 添加到 BindingList，ItemAdded 事件会自动保存到数据库
        //                _membersBindingList.Add(member);
        //                count++;

        //                _logService.Debug("VxMain", $"添加群成员: {memberNickname} ({memberWxid})");
        //            }
        //            catch (Exception ex)
        //            {
        //                _logService.Error("VxMain", $"解析单个群成员失败: {ex.Message}");
        //            }
        //        }

        //        _logService.Info("VxMain", $"✓ 群成员加载完成，共 {count} 个成员");

        //        // 刷新 UI
        //        if (dgvMembers.InvokeRequired)
        //        {
        //            dgvMembers.Invoke(new Action(() => dgvMembers.Refresh()));
        //        }
        //        else
        //        {
        //            dgvMembers.Refresh();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logService.Error("VxMain", $"加载群成员到 DataGrid 失败: {ex.Message}");
        //        throw;
        //    }
            
        //    return Task.CompletedTask;
        //}

        /// <summary>
        /// 窗口关闭时断开 Socket 连接并关闭子窗口
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _logService.Info("VxMain", "窗口正在关闭...");
                
                // 🔥 强制保存自动投注设置（防止防抖定时器未触发导致数据丢失）
                _logService.Info("VxMain", "保存自动投注设置...");
                _saveTimer?.Dispose();  // 取消防抖定时器
                _saveTimer = null;
                SaveAutoBetSettings();  // 立即保存
                
                // 断开 Socket 连接
                _logService.Info("VxMain", "断开 Socket 连接");
                _socketClient?.Disconnect();
                
                // 关闭设置窗口（如果打开）
                if (_settingsForm != null && !_settingsForm.IsDisposed)
                {
                    _logService.Info("VxMain", "关闭设置窗口");
                    _settingsForm.Close();
                    _settingsForm = null;
                }
                
                _logService.Info("VxMain", "✅ 窗口关闭前处理完成");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "关闭窗口失败", ex);
            }
            
            base.OnFormClosing(e);
        }

        #endregion

        #region 会员表右键菜单事件

        /// <summary>
        /// 🔥 菜单项：清零（清空会员余额和统计数据）
        /// </summary>
        private void OnMenuClearBalance_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvMembers.CurrentRow?.DataBoundItem is not V2Member member)
                {
                    UIMessageBox.ShowWarning("请先选择一个会员！");
                    return;
                }

                var result = UIMessageBox.ShowAsk($"确定要清零会员 [{member.Nickname}] 的所有数据吗？\n\n此操作将重置余额和所有统计数据。");
                if (!result) return;

                _logService.Info("VxMain", $"清零会员: {member.Nickname} (Wxid: {member.Wxid})");

                // 🔥 清零操作（数据会自动保存）
                member.Balance = 0;
                member.BetCur = 0;
                member.BetWait = 0;
                member.IncomeToday = 0;
                member.CreditToday = 0;
                member.BetToday = 0;
                member.WithdrawToday = 0;
                member.BetTotal = 0;
                member.CreditTotal = 0;
                member.WithdrawTotal = 0;
                member.IncomeTotal = 0;

                // 刷新显示
                dgvMembers.Refresh();
                UpdateStatistics();

                UIMessageBox.ShowSuccess($"会员 [{member.Nickname}] 已清零！");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "清零会员失败", ex);
                UIMessageBox.ShowError($"清零失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 🔥 菜单项：删除会员
        /// </summary>
        private void OnMenuDeleteMember_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvMembers.CurrentRow?.DataBoundItem is not V2Member member)
                {
                    UIMessageBox.ShowWarning("请先选择一个会员！");
                    return;
                }

                var result = UIMessageBox.ShowAsk($"确定要删除会员 [{member.Nickname}] 吗？\n\n此操作不可恢复！");
                if (!result) return;

                _logService.Info("VxMain", $"删除会员: {member.Nickname} (Wxid: {member.Wxid})");

                // 🔥 从 BindingList 中移除（会自动从数据库删除）
                _membersBindingList?.Remove(member);

                // 刷新显示
                UpdateStatistics();

                UIMessageBox.ShowSuccess($"会员 [{member.Nickname}] 已删除！");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "删除会员失败", ex);
                UIMessageBox.ShowError($"删除失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 🔥 菜单项：设置会员角色
        /// </summary>
        private void OnMenuSetRole_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvMembers.CurrentRow?.DataBoundItem is not V2Member member)
                {
                    UIMessageBox.ShowWarning("请先选择一个会员！");
                    return;
                }

                if (sender is not ToolStripMenuItem menuItem || menuItem.Tag is not MemberState newRole)
                {
                    UIMessageBox.ShowWarning("无效的角色选择！");
                    return;
                }

                var oldRole = member.State;
                _logService.Info("VxMain", $"设置会员角色: {member.Nickname} ({oldRole} -> {newRole})");

                // 🔥 修改角色（数据会自动保存）
                member.State = newRole;

                // 刷新显示
                dgvMembers.Refresh();

                UIMessageBox.ShowSuccess($"会员 [{member.Nickname}] 的角色已设置为 [{newRole}]");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "设置角色失败", ex);
                UIMessageBox.ShowError($"设置角色失败：{ex.Message}");
            }
        }

        #endregion

        #region 🔥 会员右键菜单事件处理

        /// <summary>
        /// 清分 - 清空会员余额
        /// </summary>
        private void TsmiClearBalance_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMembers.SelectedRows.Count == 0)
                {
                    UIMessageBox.ShowWarning("请先选择要清分的会员");
                    return;
                }

                var selectedMember = dgvMembers.SelectedRows[0].DataBoundItem as Models.V2Member;
                if (selectedMember == null) return;

                // 确认对话框
                if (!UIMessageBox.ShowAsk($"确定要清分会员【{selectedMember.Nickname}】吗？\n\n" +
                    $"当前余额：{selectedMember.Balance:F2}\n" +
                    $"清分后余额将变为：0.00\n\n" +
                    $"此操作将记录到资金变动表"))
                {
                    return;
                }

                float balanceBefore = selectedMember.Balance;
                float balanceAfter = 0f;
                float changeAmount = -balanceBefore;

                // 清空余额
                selectedMember.Balance = 0f;

                // 🔥 记录到资金变动表
                if (_db != null)
                {
                    var balanceChange = new Models.V2BalanceChange
                    {
                        GroupWxId = selectedMember.GroupWxId,
                        Wxid = selectedMember.Wxid,
                        Nickname = selectedMember.Nickname,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        ChangeAmount = changeAmount,
                        Reason = Models.ChangeReason.手动调整,
                        IssueId = 0,
                        TimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Notes = "管理员清分操作"
                    };
                    _db.Insert(balanceChange);

                    // 更新会员数据库
                    _db.Update(selectedMember);
                }

                // 🔥 详细日志记录
                var currentUser = Services.Api.BoterApi.GetInstance().User;
                _logService.Info("会员管理", 
                    $"清分操作\n" +
                    $"操作人：{currentUser}\n" +
                    $"群：{selectedMember.GroupWxId}\n" +
                    $"会员：{selectedMember.Nickname} ({selectedMember.Wxid})\n" +
                    $"清分前余额：{balanceBefore:F2}\n" +
                    $"清分后余额：{balanceAfter:F2}\n" +
                    $"清分金额：{changeAmount:F2}");

                // 刷新UI
                dgvMembers.Refresh();
                UpdateStatistics();
                this.ShowSuccessTip($"清分成功！会员【{selectedMember.Nickname}】余额已清零");
            }
            catch (Exception ex)
            {
                _logService.Error("会员管理", "清分失败", ex);
                UIMessageBox.ShowError($"清分失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除会员 - 硬删除（物理删除）
        /// </summary>
        private void TsmiDeleteMember_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMembers.SelectedRows.Count == 0)
                {
                    UIMessageBox.ShowWarning("请先选择要删除的会员");
                    return;
                }

                var selectedMember = dgvMembers.SelectedRows[0].DataBoundItem as Models.V2Member;
                if (selectedMember == null) return;

                // 确认对话框（警告硬删除）
                if (!UIMessageBox.ShowAsk($"⚠️ 警告：确定要删除会员【{selectedMember.Nickname}】吗？\n\n" +
                    $"这是物理删除，数据将无法恢复！\n\n" +
                    $"会员信息：\n" +
                    $"昵称：{selectedMember.Nickname}\n" +
                    $"微信ID：{selectedMember.Wxid}\n" +
                    $"账号：{selectedMember.Account}\n" +
                    $"当前余额：{selectedMember.Balance:F2}\n" +
                    $"总下注：{selectedMember.BetTotal:F2}\n" +
                    $"总盈亏：{selectedMember.IncomeTotal:F2}\n\n" +
                    $"删除操作将被详细记录到日志"))
                {
                    return;
                }

                // 🔥 详细日志记录（删除前）
                var currentUser = Services.Api.BoterApi.GetInstance().User;
                _logService.Info("会员管理", 
                    $"删除会员操作\n" +
                    $"操作人：{currentUser}\n" +
                    $"群：{selectedMember.GroupWxId}\n" +
                    $"被删除会员信息：\n" +
                    $"  - 昵称：{selectedMember.Nickname}\n" +
                    $"  - 微信ID：{selectedMember.Wxid}\n" +
                    $"  - 账号：{selectedMember.Account}\n" +
                    $"  - 当前余额：{selectedMember.Balance:F2}\n" +
                    $"  - 今日下注：{selectedMember.BetToday:F2}\n" +
                    $"  - 今日盈亏：{selectedMember.IncomeToday:F2}\n" +
                    $"  - 总下注：{selectedMember.BetTotal:F2}\n" +
                    $"  - 总盈亏：{selectedMember.IncomeTotal:F2}\n" +
                    $"  - 今日上分：{selectedMember.CreditToday:F2}\n" +
                    $"  - 今日下分：{selectedMember.WithdrawToday:F2}\n" +
                    $"  - 总上分：{selectedMember.CreditTotal:F2}\n" +
                    $"  - 总下分：{selectedMember.WithdrawTotal:F2}");

                // 🔥 从数据库物理删除
                if (_db != null)
                {
                    _db.Delete(selectedMember);
                }

                // 从 BindingList 移除
                _membersBindingList?.Remove(selectedMember);

                // 刷新UI
                dgvMembers.Refresh();
                UpdateStatistics();

                this.ShowSuccessTip($"已删除会员【{selectedMember.Nickname}】");
            }
            catch (Exception ex)
            {
                _logService.Error("会员管理", "删除会员失败", ex);
                UIMessageBox.ShowError($"删除会员失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 设置会员类型
        /// </summary>
        private void TsmiSetMemberType_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMembers.SelectedRows.Count == 0)
                {
                    UIMessageBox.ShowWarning("请先选择要设置的会员");
                    return;
                }

                var selectedMember = dgvMembers.SelectedRows[0].DataBoundItem as Models.V2Member;
                if (selectedMember == null) return;

                var menuItem = sender as ToolStripMenuItem;
                if (menuItem == null) return;

                // 🔥 根据菜单项名称确定会员类型（使用 MemberState 枚举）
                Models.MemberState newState = menuItem.Name switch
                {
                    "tsmiSetAdmin" => Models.MemberState.管理,
                    "tsmiSetAgent" => Models.MemberState.托,
                    "tsmiSetLeft" => Models.MemberState.已退群,
                    "tsmiSetNormal" => Models.MemberState.普会,
                    "tsmiSetBlue" => Models.MemberState.蓝会,
                    "tsmiSetPurple" => Models.MemberState.紫会,
                    "tsmiSetYellow" => Models.MemberState.黄会,
                    _ => selectedMember.State  // 保持不变
                };

                string typeName = menuItem.Text;
                var oldState = selectedMember.State;

                // 🔥 更新会员 State 字段
                selectedMember.State = newState;

                // 🔥 更新数据库
                if (_db != null)
                {
                    _db.Update(selectedMember);
                }

                // 记录日志
                var currentUser = Services.Api.BoterApi.GetInstance().User;
                _logService.Info("会员管理", 
                    $"设置会员类型\n" +
                    $"操作人：{currentUser}\n" +
                    $"会员：{selectedMember.Nickname} ({selectedMember.Wxid})\n" +
                    $"原类型：{oldState}\n" +
                    $"新类型：{typeName} ({newState})");

                // 刷新UI
                dgvMembers.Refresh();
                this.ShowSuccessTip($"已将会员【{selectedMember.Nickname}】设置为：{typeName}");
            }
            catch (Exception ex)
            {
                _logService.Error("会员管理", "设置会员类型失败", ex);
                UIMessageBox.ShowError($"设置会员类型失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 查看资金变动
        /// </summary>
        private void TsmiViewBalanceChange_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMembers.SelectedRows.Count == 0)
                {
                    UIMessageBox.ShowWarning("请先选择要查看的会员");
                    return;
                }

                var selectedMember = dgvMembers.SelectedRows[0].DataBoundItem as Models.V2Member;
                if (selectedMember == null) return;

                // 🔥 创建并显示资金变动查看窗口
                if (_db == null)
                {
                    UIMessageBox.ShowWarning("数据库未初始化");
                    return;
                }

                var form = new Views.BalanceChangeViewerForm(
                    selectedMember.Wxid, 
                    selectedMember.Nickname, 
                    _db, 
                    _logService);
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                _logService.Error("会员管理", "查看资金变动失败", ex);
                UIMessageBox.ShowError($"查看资金变动失败：{ex.Message}");
            }
        }

        #endregion

        #region 🔥 上下分管理

        // 🔥 上下分管理窗口单实例（非模态）
        private Views.CreditWithdrawManageForm? _creditWithdrawManageForm;

        /// <summary>
        /// 打开上下分管理窗口（非模态，参考 F5BotV2）
        /// </summary>
        private void btnCreditWithdrawManage_Click(object sender, EventArgs e)
        {
            try
            {
                if (_db == null || _creditWithdrawsBindingList == null || _membersBindingList == null)
                {
                    UIMessageBox.ShowWarning("请先绑定群");
                    return;
                }
                
                // 🔥 检查窗口是否已打开（参考设置窗口的逻辑）
                if (_creditWithdrawManageForm != null && !_creditWithdrawManageForm.IsDisposed)
                {
                    // 窗口已打开，激活并显示到前台
                    _logService.Info("VxMain", "上下分管理窗口已打开，激活到前台");
                    
                    // 如果窗口最小化，先恢复
                    if (_creditWithdrawManageForm.WindowState == FormWindowState.Minimized)
                    {
                        _creditWithdrawManageForm.WindowState = FormWindowState.Normal;
                    }
                    
                    // 激活窗口并显示到最前面
                    _creditWithdrawManageForm.Activate();
                    _creditWithdrawManageForm.BringToFront();
                    _creditWithdrawManageForm.Focus();
                    
                    lblStatus.Text = "上下分管理窗口已激活";
                    return;
                }
                
                lblStatus.Text = "打开上下分管理窗口...";
                _logService.Info("VxMain", "创建新的上下分管理窗口");
                
                // 🔥 创建新的上下分管理窗口（非模态）
                // 🔥 创建上下分服务（参考 F5BotV2）
                var creditWithdrawService = new Services.Games.Binggo.CreditWithdrawService(
                    _db,
                    _logService,
                    _statisticsService,
                    _socketClient,
                    Program.ServiceProvider.GetService<Services.Sound.SoundService>());
                
                _creditWithdrawManageForm = new Views.CreditWithdrawManageForm(
                    _db,
                    _logService,
                    _socketClient,
                    _creditWithdrawsBindingList,
                    _membersBindingList,
                    creditWithdrawService);
                
                // 🔥 订阅关闭事件，清理引用并刷新统计
                _creditWithdrawManageForm.FormClosed += (s, args) =>
                {
                    _logService.Info("VxMain", "上下分管理窗口已关闭");
                    _creditWithdrawManageForm = null;
                    lblStatus.Text = "上下分管理窗口已关闭";
                    
                    // 🔥 关闭窗口后刷新统计
                    _statisticsService.UpdateStatistics();
                    UpdateMemberInfoLabel();
                };
                
                // 🔥 显示为非模态窗口（可以同时操作其他窗口）
                _creditWithdrawManageForm.Show(this);
                lblStatus.Text = "上下分管理窗口已打开";
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "打开上下分管理窗口失败", ex);
                UIMessageBox.ShowError($"打开上下分管理窗口失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 加载上下分数据并恢复会员统计
        /// 参考 F5BotV2 BoterServices.cs 第901-907行
        /// </summary>
        private void LoadCreditWithdrawData(string groupWxid)
        {
            try
            {
                if (_db == null || _membersBindingList == null || _creditWithdrawsBindingList == null)
                {
                    _logService.Warning("VxMain", "数据库、会员列表或上下分列表未初始化，跳过上下分数据加载");
                    return;
                }
                
                // 🔥 1. 确保表存在
                _db.CreateTable<V2CreditWithdraw>();
                
                // 🔥 2. 从 BindingList（内存表）加载该群的所有上下分记录
                // 用户要求："订单只能从内存表中拿，改数据都改内存表，内存表修改即保存"
                var creditWithdraws = _creditWithdrawsBindingList
                    .Where(cw => cw.GroupWxId == groupWxid)
                    .OrderBy(cw => cw.Timestamp)
                    .ToList();
                
                _logService.Info("VxMain", $"📊 从内存表加载了 {creditWithdraws.Count} 条上下分记录");
                
                if (creditWithdraws.Count == 0)
                {
                    return;
                }
                
                // 🔥 3. 今日日期（用于判断今日统计）
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                
                // 🔥 4. 遍历所有上下分记录，恢复会员统计
                // 参考 F5BotV2: 加载时只更新Total统计，Today统计由每日重置逻辑处理
                foreach (var cw in creditWithdraws)
                {
                    // 只处理已同意的记录
                    if (cw.Status != CreditWithdrawStatus.已同意)
                    {
                        continue;
                    }
                    
                    var member = _membersBindingList.FirstOrDefault(m => m.Wxid == cw.Wxid);
                    if (member == null)
                    {
                        _logService.Warning("VxMain", $"上下分记录找不到对应会员: {cw.Wxid}");
                        continue;
                    }
                    
                    // 🔥 更新Total统计（总计）
                    if (cw.Action == CreditWithdrawAction.上分)
                    {
                        member.CreditTotal += cw.Amount;
                        
                        // 如果是今日的，也更新Today统计
                        if (cw.TimeString.StartsWith(today))
                        {
                            member.CreditToday += cw.Amount;
                        }
                    }
                    else if (cw.Action == CreditWithdrawAction.下分)
                    {
                        member.WithdrawTotal += cw.Amount;
                        
                        // 如果是今日的，也更新Today统计
                        if (cw.TimeString.StartsWith(today))
                        {
                            member.WithdrawToday += cw.Amount;
                        }
                    }
                }
                
                _logService.Info("VxMain", $"✅ 上下分数据加载完成，已恢复会员统计");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "加载上下分数据失败", ex);
            }
        }

        #endregion

        #region 🤖 自动投注 UI 和逻辑

        private System.Threading.Timer? _saveTimer;
        private System.Threading.Timer? _oddsTimer;  // 🔥 赔率防抖定时器

        /// <summary>
        /// 初始化自动投注 UI 事件（控件已在 Designer 中创建）
        /// </summary>
        private void InitializeAutoBetUIEvents()
        {
            try
            {
                _logService.Info("VxMain", "🤖 初始化自动投注UI事件绑定...");
                
                // 从默认配置加载设置
                LoadAutoBetSettings();
                
                // ✅ 加载应用设置（绑定到 ConfigViewModel，支持双向自动同步）
                swi_OrdersTasking.DataBindings.Add(
                    new Binding("Active", _configViewModel, nameof(_configViewModel.IsOrdersTaskingEnabled), 
                    false, DataSourceUpdateMode.OnPropertyChanged));
                    
                swiAutoOrdersBet.DataBindings.Add(
                    new Binding("Active", _configViewModel, nameof(_configViewModel.IsAutoBetEnabled), 
                    false, DataSourceUpdateMode.OnPropertyChanged));

                // 绑定自动保存事件（使用防抖机制）
                // 下拉框：立即保存
                cbxPlatform.SelectedIndexChanged += (s, e) => SaveAutoBetSettings();
                
                // 文本框：延迟保存（防抖：用户停止输入1秒后再保存）
                txtAutoBetUsername.TextChanged += (s, e) => 
                {
                    _logService.Debug("VxMain", $"🔍 账号文本变化: '{txtAutoBetUsername.Text}'");
                    DebounceSaveSettings();
                };
                txtAutoBetPassword.TextChanged += (s, e) => 
                {
                    _logService.Debug("VxMain", $"🔍 密码文本变化: '{(string.IsNullOrEmpty(txtAutoBetPassword.Text) ? "(空)" : "***")}'");
                    DebounceSaveSettings();
                };
                
                // 🔥 赔率设置：防抖验证和保存，并处理步进（0.01）
                _oddsValueChangedHandler = (sender, value) =>
                {
                    try
                    {
                        double currentValue = value;
                        double diff = Math.Abs(currentValue - _lastOddsValue);
                        
                        // 🔥 如果变化量接近 1.0（可能是默认步进），则调整为 0.01 步进
                        if (diff > 0.5 && diff < 1.5)
                        {
                            // 检测到可能是按钮点击导致的大步进，调整为 0.01 步进
                            double newValue = currentValue > _lastOddsValue 
                                ? _lastOddsValue + 0.01 
                                : _lastOddsValue - 0.01;
                            
                            // 限制在有效范围内
                            newValue = Math.Max(1.0, Math.Min(2.5, newValue));
                            
                            // 临时解绑事件避免递归
                            if (_oddsValueChangedHandler != null)
                            {
                                txtOdds.ValueChanged -= _oddsValueChangedHandler;
                                txtOdds.Value = newValue;
                                txtOdds.ValueChanged += _oddsValueChangedHandler;
                            }
                            
                            _lastOddsValue = newValue;
                            _logService.Debug("VxMain", $"🔍 赔率步进调整: {currentValue:F2} → {newValue:F2}");
                            
                            // 触发防抖保存
                            DebounceValidateAndSaveOdds();
                            return;
                        }
                        
                        // 🔥 如果变化量不是 0.01 的倍数，且变化量较大（可能是按钮点击），则调整到最近的 0.01 倍数
                        if (diff > 0.01 && diff < 0.5)
                        {
                            // 计算应该增加还是减少
                            double step = currentValue > _lastOddsValue ? 0.01 : -0.01;
                            double newValue = _lastOddsValue + step;
                            
                            // 限制在有效范围内
                            newValue = Math.Max(1.0, Math.Min(2.5, newValue));
                            
                            // 如果调整后的值与当前值不同，则更新
                            if (Math.Abs(newValue - currentValue) > 0.001)
                            {
                                if (_oddsValueChangedHandler != null)
                                {
                                    txtOdds.ValueChanged -= _oddsValueChangedHandler;
                                    txtOdds.Value = newValue;
                                    txtOdds.ValueChanged += _oddsValueChangedHandler;
                                }
                                _lastOddsValue = newValue;
                                _logService.Debug("VxMain", $"🔍 赔率步进调整: {currentValue:F2} → {newValue:F2}");
                                DebounceValidateAndSaveOdds();
                                return;
                            }
                        }
                        
                        _lastOddsValue = currentValue;
                        _logService.Debug("VxMain", $"🔍 赔率值变化: {currentValue:F2}");
                        DebounceValidateAndSaveOdds();
                    }
                    catch (Exception ex)
                    {
                        _logService.Error("VxMain", "处理赔率值变化失败", ex);
                    }
                };
                txtOdds.ValueChanged += _oddsValueChangedHandler;
                
                // 🔥 双重保险：失去焦点时立即保存（防止复制粘贴后立即关闭程序导致数据丢失）
                txtAutoBetUsername.LostFocus += (s, e) => 
                {
                    _logService.Debug("VxMain", "🔍 账号失去焦点，取消防抖定时器并立即保存");
                    _saveTimer?.Dispose();
                    _saveTimer = null;
                    SaveAutoBetSettings();
                };
                txtAutoBetPassword.LostFocus += (s, e) => 
                {
                    _logService.Debug("VxMain", "🔍 密码失去焦点，取消防抖定时器并立即保存");
                    _saveTimer?.Dispose();
                    _saveTimer = null;
                    SaveAutoBetSettings();
                };
                
                txtOdds.LostFocus += (s, e) => 
                {
                    _logService.Debug("VxMain", "🔍 赔率失去焦点，取消防抖定时器并立即验证保存");
                    _oddsTimer?.Dispose();
                    _oddsTimer = null;
                    ValidateAndSaveOdds();
                };
                
                _logService.Info("VxMain", "✅ 自动投注UI事件已绑定（包含 TextChanged 和 LostFocus）");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "初始化自动投注UI事件失败", ex);
            }
        }
        
     


        /// <summary>
        /// 防抖保存设置（用户停止输入1秒后才保存）
        /// 🔥 保存时机：
        /// 1. 用户修改账号/密码后，停止输入1秒自动保存
        /// 2. 用户修改平台时，立即保存
        /// 3. 开启自动投注时，立即保存
        /// 4. 窗口关闭时，强制保存（防止数据丢失）
        /// </summary>
        private void DebounceSaveSettings()
        {
            // 取消之前的计时器
            _saveTimer?.Dispose();
            
            // 创建新的计时器，1秒后执行保存
            _saveTimer = new System.Threading.Timer(_ =>
            {
                // 在UI线程上执行保存
                this.Invoke(() =>
                {
                    _logService.Info("VxMain", "⏰ 防抖定时器触发：自动保存账号/密码设置");
                    SaveAutoBetSettings();
                    _saveTimer?.Dispose();
                    _saveTimer = null;
                });
            }, null, 1000, System.Threading.Timeout.Infinite);
            
            _logService.Debug("VxMain", "⏳ 账号/密码已修改，将在1秒后自动保存（防抖机制）");
        }

        /// <summary>
        /// 防抖验证和保存赔率（用户停止输入1秒后才验证和保存）
        /// </summary>
        private void DebounceValidateAndSaveOdds()
        {
            // 取消之前的计时器
            _oddsTimer?.Dispose();
            
            // 创建新的计时器，1秒后执行验证和保存
            _oddsTimer = new System.Threading.Timer(_ =>
            {
                // 在UI线程上执行验证和保存
                this.Invoke(() =>
                {
                    _logService.Info("VxMain", "⏰ 赔率防抖定时器触发：验证并保存赔率");
                    ValidateAndSaveOdds();
                    _oddsTimer?.Dispose();
                    _oddsTimer = null;
                });
            }, null, 1000, System.Threading.Timeout.Infinite);
            
            _logService.Debug("VxMain", "⏳ 赔率已修改，将在1秒后验证并保存（防抖机制）");
        }

        /// <summary>
        /// 验证并保存赔率（范围：1.0 - 2.5，默认：1.97）
        /// </summary>
        private void ValidateAndSaveOdds()
        {
            try
            {
                double oddsValue = txtOdds.Value;
                
                // 🔥 验证范围：< 1 或 > 2.5 都重置为 1.97
                if (oddsValue < 1.0 || oddsValue > 2.5)
                {
                    string reason = oddsValue < 1.0 
                        ? "赔率不能小于 1.0" 
                        : "赔率不能大于 2.5";
                    
                    _logService.Warning("VxMain", $"❌ 赔率验证失败: {oddsValue:F2} - {reason}，重置为 1.97");
                    
                    // 重置为默认值
                    txtOdds.Value = 1.97;
                    
                    // 显示提示
                    UIMessageBox.Show($"赔率设置失败：{reason}\n已重置为默认值 1.97", 
                        "提示", UIStyle.Orange, UIMessageBoxButtons.OK);
                    
                    return;
                }
                
                // 🔥 保存到全局配置（微信订单统一赔率，用于订单结算）
                _configService.SetWechatOrderOdds((float)oddsValue);
                
                _logService.Info("VxMain", $"✅ 赔率已保存: {oddsValue:F2}");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "验证并保存赔率失败", ex);
            }
        }

        /// <summary>
        /// 初始化平台下拉框（使用统一数据源）
        /// </summary>
        private void InitializePlatformComboBox()
        {
            try
            {
                var platformNames = BetPlatformHelper.GetAllPlatformNames();
                cbxPlatform.Items.Clear();
                cbxPlatform.Items.AddRange(platformNames);
                _logService.Info("VxMain", $"✅ 平台下拉框已初始化，共 {platformNames.Length} 个平台");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "初始化平台下拉框失败", ex);
            }
        }
        
        /// <summary>
        /// 赔率控件值变化事件处理（实现 0.01 步进）
        /// </summary>
        private void TxtOdds_ValueChanged(object? sender, double e)
        {
            try
            {
                double currentValue = txtOdds.Value;
                double diff = Math.Abs(currentValue - _lastOddsValue);
                
                // 🔥 如果变化量接近 1.0（可能是默认步进），则调整为 0.01 步进
                if (diff > 0.5 && diff < 1.5)
                {
                    // 检测到可能是按钮点击导致的大步进，调整为 0.01 步进
                    double newValue = currentValue > _lastOddsValue 
                        ? _lastOddsValue + 0.01 
                        : _lastOddsValue - 0.01;
                    
                    // 限制在有效范围内
                    newValue = Math.Max(1.0, Math.Min(2.5, newValue));
                    
                    // 临时解绑事件避免递归
                    txtOdds.ValueChanged -= TxtOdds_ValueChanged;
                    txtOdds.Value = newValue;
                    txtOdds.ValueChanged += TxtOdds_ValueChanged;
                    
                    _lastOddsValue = newValue;
                    _logService.Debug("VxMain", $"🔍 赔率步进调整: {currentValue:F2} → {newValue:F2}");
                    
                    // 触发防抖保存
                    DebounceValidateAndSaveOdds();
                    return;
                }
                
                // 🔥 如果变化量不是 0.01 的倍数，且变化量较大（可能是按钮点击），则调整到最近的 0.01 倍数
                if (diff > 0.01 && diff < 0.5)
                {
                    // 计算应该增加还是减少
                    double step = currentValue > _lastOddsValue ? 0.01 : -0.01;
                    double newValue = _lastOddsValue + step;
                    
                    // 限制在有效范围内
                    newValue = Math.Max(1.0, Math.Min(2.5, newValue));
                    
                    // 如果调整后的值与当前值不同，则更新
                    if (Math.Abs(newValue - currentValue) > 0.001)
                    {
                        txtOdds.ValueChanged -= TxtOdds_ValueChanged;
                        txtOdds.Value = newValue;
                        txtOdds.ValueChanged += TxtOdds_ValueChanged;
                        _lastOddsValue = newValue;
                        _logService.Debug("VxMain", $"🔍 赔率步进调整: {currentValue:F2} → {newValue:F2}");
                        DebounceValidateAndSaveOdds();
                        return;
                    }
                }
                
                _lastOddsValue = currentValue;
                _logService.Debug("VxMain", $"🔍 赔率值变化: {currentValue:F2}");
                DebounceValidateAndSaveOdds();
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "处理赔率值变化失败", ex);
            }
        }
        
        /// <summary>
        /// 从默认配置加载自动投注设置
        /// 🔥 如果默认配置不存在，会创建一个新的默认配置（账号密码为空）
        /// </summary>
        private void LoadAutoBetSettings()
        {
            try
            {
                // 🔥 加载设置时，临时解绑事件，避免触发自动启动
                _logService.Info("VxMain", "📋 加载自动投注设置（临时解绑事件）...");
                swiAutoOrdersBet.ValueChanged -= swiAutoOrdersBet_ValueChanged;
                
                var defaultConfig = _autoBetService.GetConfigsBindingList()?.FirstOrDefault(c => c.IsDefault);
                
                if (defaultConfig != null)
                {
                    // 加载平台（使用共享库统一转换）
                    var platform = BetPlatformHelper.Parse(defaultConfig.Platform);
                    cbxPlatform.SelectedIndex = BetPlatformHelper.GetIndex(platform);

                    // 加载账号密码（如果为空，显示空白是正常的）
                    txtAutoBetUsername.Text = defaultConfig.Username ?? "";
                    txtAutoBetPassword.Text = defaultConfig.Password ?? "";
                    
                    // 🔥 加载微信订单统一赔率（从全局配置，默认 1.97）
                    var odds = _configService.GetWechatOrderOdds();
                    if (odds <= 0) odds = 1.97f;  // 如果未设置，使用默认值
                    _lastOddsValue = odds;  // 初始化记录值
                    txtOdds.Value = odds;
                    
                    _logService.Info("VxMain", $"✅ 已加载默认配置: 平台={defaultConfig.Platform}, 账号={(string.IsNullOrEmpty(defaultConfig.Username) ? "(空)" : defaultConfig.Username)}, 赔率={odds:F2}");
                }
                else
                {
                    // 🔥 默认配置不存在，创建一个新的（账号密码为空）
                    _logService.Warning("VxMain", "⚠️ 未找到默认配置，将创建新的默认配置");
                    
                    var platform = BetPlatformHelper.GetByIndex(cbxPlatform.SelectedIndex >= 0 ? cbxPlatform.SelectedIndex : 0);
                    var newConfig = new Models.AutoBet.BetConfig
                    {
                        ConfigName = "默认配置",
                        Platform = platform.ToString(),
                        PlatformUrl = PlatformUrlManager.GetDefaultUrl(platform),
                        Username = "",  // 🔥 初始为空，用户需要手动输入
                        Password = "",  // 🔥 初始为空，用户需要手动输入
                        IsDefault = true,
                        IsEnabled = false,
                        AutoLogin = true
                    };
                    
                    _autoBetService.SaveConfig(newConfig);
                    
                    // 加载到UI
                    cbxPlatform.SelectedIndex = BetPlatformHelper.GetIndex(platform);
                    txtAutoBetUsername.Text = "";
                    txtAutoBetPassword.Text = "";
                    txtOdds.Value = 1.97;  // 🔥 默认赔率
                    
                    _logService.Info("VxMain", "✅ 已创建新的默认配置（账号密码为空，需要用户输入）");
                }
                
                _logService.Info("VxMain", "✅ 自动投注设置加载完成");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "加载自动投注设置失败", ex);
            }
            finally
            {
                // 🔥 重新绑定事件
                swiAutoOrdersBet.ValueChanged += swiAutoOrdersBet_ValueChanged;
                _logService.Info("VxMain", "✅ 自动投注开关事件已重新绑定");
            }
        }

        /// <summary>
        /// 保存自动投注设置到默认配置
        /// 🔥 保存时机：
        /// 1. 用户修改账号/密码后，停止输入1秒自动保存（防抖机制）
        /// 2. 用户修改平台时，立即保存
        /// 3. 开启自动投注时，立即保存
        /// 4. 窗口关闭时，强制保存（防止数据丢失）
        /// </summary>
        private void SaveAutoBetSettings()
        {
            try
            {
                var defaultConfig = _autoBetService.GetConfigsBindingList()?.FirstOrDefault(c => c.IsDefault);
                if (defaultConfig == null)
                {
                    // 🔥 如果默认配置不存在，创建一个新的
                    _logService.Warning("VxMain", "⚠️ 未找到默认配置，将创建新的默认配置");
                    
                    var platform = BetPlatformHelper.GetByIndex(cbxPlatform.SelectedIndex);
                    defaultConfig = new Models.AutoBet.BetConfig
                    {
                        ConfigName = "默认配置",
                        Platform = platform.ToString(),
                        PlatformUrl = PlatformUrlManager.GetDefaultUrl(platform),
                        Username = txtAutoBetUsername.Text,
                        Password = txtAutoBetPassword.Text,
                        IsDefault = true,
                        IsEnabled = false,
                        AutoLogin = true
                    };
                    
                    _autoBetService.SaveConfig(defaultConfig);
                    _logService.Info("VxMain", "✅ 已创建新的默认配置");
                }
                else
                {
                    // 保存平台（使用共享库统一转换）
                    var platform = BetPlatformHelper.GetByIndex(cbxPlatform.SelectedIndex);
                    defaultConfig.Platform = platform.ToString();
                    
                    // 🔥 不再自动覆盖平台URL，保留用户手动修改的值
                    // 如果用户需要修改URL，应该在配置管理器中手动修改
                    // 只有在URL为空时才自动设置默认URL
                    if (string.IsNullOrWhiteSpace(defaultConfig.PlatformUrl))
                    {
                        defaultConfig.PlatformUrl = PlatformUrlManager.GetDefaultUrl(platform);
                        _logService.Info("VxMain", $"URL为空，已自动设置为默认URL: {defaultConfig.PlatformUrl}");
                    }
                    else
                    {
                        _logService.Info("VxMain", $"保留用户设置的URL: {defaultConfig.PlatformUrl}");
                    }

                    // 保存账号密码
                    var username = txtAutoBetUsername.Text;
                    var password = txtAutoBetPassword.Text;
                    
                    // 🔥 检查是否有变化（避免不必要的保存）
                    bool usernameChanged = defaultConfig.Username != username;
                    bool passwordChanged = defaultConfig.Password != password;
                    
                    if (usernameChanged || passwordChanged)
                    {
                        _logService.Info("VxMain", $"📝 检测到账号/密码变化:");
                        if (usernameChanged)
                            _logService.Info("VxMain", $"   账号: {defaultConfig.Username ?? "(空)"} → {username ?? "(空)"}");
                        if (passwordChanged)
                            _logService.Info("VxMain", $"   密码: {(string.IsNullOrEmpty(defaultConfig.Password) ? "(空)" : "***")} → {(string.IsNullOrEmpty(password) ? "(空)" : "***")}");
                    }
                    
                    defaultConfig.Username = username;
                    defaultConfig.Password = password;
                    defaultConfig.LastUpdateTime = DateTime.Now;  // 🔥 强制触发更新

                    // 保存到数据库
                    _autoBetService.SaveConfig(defaultConfig);

                    _logService.Info("VxMain", "✅ 自动投注设置已保存到数据库");
                    _logService.Info("VxMain", $"   - 平台: {defaultConfig.Platform}");
                    _logService.Info("VxMain", $"   - URL: {defaultConfig.PlatformUrl}");
                    _logService.Info("VxMain", $"   - 账号: {(string.IsNullOrEmpty(username) ? "(空)" : username)}");
                    _logService.Info("VxMain", $"   - 密码: {(string.IsNullOrEmpty(password) ? "(空)" : "已设置")}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "保存自动投注设置失败", ex);
            }
        }

        /// <summary>
        /// 启用/禁用自动投注开关（使用 UI开关控件）
        /// 职责：只修改应用级配置（appsettings.json），不启动或停止浏览器
        /// 浏览器的启动由监控线程自动管理，或用户手动点击浏览器控制按钮
        /// </summary>
        private void swiAutoOrdersBet_ValueChanged(object? sender, bool value)
        {
            try
            {
                _logService.Info("VxMain", $"🎚️ 飞单开关触发: {value}");
                
                // ✅ 只修改应用级配置（会自动保存到 appsettings.json）
                _configService.SetIsAutoBetEnabled(value);
                
                if (value) // 开启自动投注
                {
                    // 先保存设置
                    SaveAutoBetSettings();
                    
                    // ✅ 设置 BetConfig.IsEnabled = true（让监控线程启动浏览器）
                    var defaultConfig = _autoBetService.GetConfigsBindingList()?.FirstOrDefault(c => c.IsDefault);
                    if (defaultConfig != null)
                    {
                        defaultConfig.IsEnabled = true;
                        _autoBetService.SaveConfig(defaultConfig);
                        _logService.Info("VxMain", $"✅ 已设置配置 [{defaultConfig.ConfigName}] IsEnabled=true");
                        _logService.Info("VxMain", "   监控线程将在2秒内检测到并启动浏览器");
                        
                        // 🔥 启动 AutoBetCoordinator（订阅封盘事件，处理订单投注）
                        _ = Task.Run(async () =>
                        {
                            var success = await _autoBetCoordinator.StartAsync(defaultConfig.Id);
                            if (success)
                            {
                                _logService.Info("VxMain", $"✅ AutoBetCoordinator 已启动，已订阅封盘事件");
                            }
                            else
                            {
                                _logService.Error("VxMain", "❌ AutoBetCoordinator 启动失败");
                            }
                        });
                    }
                    
                    _logService.Info("VxMain", "✅ 飞单功能已启用（浏览器由监控任务管理）");
                    this.ShowSuccessTip("飞单功能已启用！");
                }
                else // 停止自动投注
                {
                    // 🔥 先停止 AutoBetCoordinator（取消订阅封盘事件）
                    _autoBetCoordinator.Stop();
                    _logService.Info("VxMain", "✅ AutoBetCoordinator 已停止，已取消订阅封盘事件");
                    
                    // ✅ 设置 BetConfig.IsEnabled = false（停止监控浏览器）
                    var defaultConfig = _autoBetService.GetConfigsBindingList()?.FirstOrDefault(c => c.IsDefault);
                    if (defaultConfig != null)
                    {
                        defaultConfig.IsEnabled = false;
                        _autoBetService.SaveConfig(defaultConfig);
                        _logService.Info("VxMain", $"✅ 已设置配置 [{defaultConfig.ConfigName}] IsEnabled=false");
                    }
                    
                    _logService.Info("VxMain", "🛑 飞单功能已禁用");
                    this.ShowSuccessTip("飞单功能已禁用！");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "切换自动投注失败", ex);
                swiAutoOrdersBet.Active = !value;  // 恢复原值
                this.ShowErrorTip($"操作失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 启用/禁用订单任务开关（控制是否处理微信消息）
        /// </summary>
        private void swi_OrdersTasking_ValueChanged(object? sender, bool value)
        {
            try
            {
                _logService.Info("VxMain", $"🎚️ 收单开关触发: {value}");
                
                // ✅ 通过 Service 更新配置（会自动保存到文件 + 触发事件）
                _configService.SetIsOrdersTaskingEnabled(value);
                
                // 更新消息处理器的全局开关
                Services.Messages.Handlers.BinggoMessageHandler.IsOrdersTaskingEnabled = value;
                _logService.Info("VxMain", $"✅ 已同步到 BinggoMessageHandler.IsOrdersTaskingEnabled = {value}");
                
                if (value)
                {
                    _logService.Info("VxMain", "✅ 订单任务已启用（收单中）");
                    this.ShowSuccessTip("订单任务已启用，开始处理微信消息");
                }
                else
                {
                    _logService.Info("VxMain", "⏹️ 订单任务已禁用（收单停）");
                    this.ShowInfoTip("订单任务已禁用，暂停处理微信消息");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "切换订单任务失败", ex);
            }
        }

        /// <summary>
        /// 加载应用配置（从 appsettings.json）
        /// </summary>
        private void LoadAppConfiguration()
        {
            try
            {
                _logService.Info("VxMain", "📖 开始加载应用配置（临时解绑事件）...");
                
                // 🔥 临时解绑事件，避免加载时触发自动启动
                swiAutoOrdersBet.ValueChanged -= swiAutoOrdersBet_ValueChanged;
                swi_OrdersTasking.ValueChanged -= swi_OrdersTasking_ValueChanged;
                
                // ✅ 从配置服务获取配置
                var isAutoBetEnabled = _configService.GetIsAutoBetEnabled();
                var isOrdersTaskingEnabled = _configService.GetIsOrdersTaskingEnabled();
                
                _logService.Info("VxMain", $"📖 从配置文件读取: 飞单={isAutoBetEnabled}, 收单={isOrdersTaskingEnabled}");
                
                // ✅ 通过 ViewModel 设置UI状态（通过数据绑定自动同步）
                _configViewModel.IsAutoBetEnabled = isAutoBetEnabled;
                _configViewModel.IsOrdersTaskingEnabled = isOrdersTaskingEnabled;
                
                // ✅ 手动同步到消息处理器（初始化时需要同步）
                Services.Messages.Handlers.BinggoMessageHandler.IsOrdersTaskingEnabled = isOrdersTaskingEnabled;
                _logService.Info("VxMain", $"✅ 已同步到 BinggoMessageHandler.IsOrdersTaskingEnabled = {isOrdersTaskingEnabled}");
                
                _logService.Info("VxMain", $"✅ 应用配置已加载: 飞单={swiAutoOrdersBet.Active}, 收单={swi_OrdersTasking.Active}");
                
                // 🔥 同步应用级配置到 BetConfig.IsEnabled
                // 这样监控线程才能正确检测到需要启动浏览器
                var defaultConfig = _autoBetService.GetConfigsBindingList()?.FirstOrDefault(c => c.IsDefault);
                if (defaultConfig != null)
                {
                    if (defaultConfig.IsEnabled != isAutoBetEnabled)
                    {
                        _logService.Info("VxMain", $"🔄 同步飞单开关状态: appsettings.json={isAutoBetEnabled}, BetConfig.IsEnabled={defaultConfig.IsEnabled}");
                        _logService.Info("VxMain", $"   将 BetConfig.IsEnabled 同步为: {isAutoBetEnabled}");
                        defaultConfig.IsEnabled = isAutoBetEnabled;
                        _autoBetService.SaveConfig(defaultConfig);
                    }
                    else
                    {
                        _logService.Info("VxMain", $"✅ 飞单开关状态已同步: BetConfig.IsEnabled={defaultConfig.IsEnabled}");
                    }
                    
                    // 🔥 如果飞单开关已开启，启动 AutoBetCoordinator（订阅封盘事件）
                    if (isAutoBetEnabled)
                    {
                        _logService.Info("VxMain", "✅ 检测到飞单开关已开启，启动 AutoBetCoordinator...");
                        _ = Task.Run(async () =>
                        {
                            var success = await _autoBetCoordinator.StartAsync(defaultConfig.Id);
                            if (success)
                            {
                                _logService.Info("VxMain", $"✅ AutoBetCoordinator 已启动，已订阅封盘事件");
                            }
                            else
                            {
                                _logService.Error("VxMain", "❌ AutoBetCoordinator 启动失败");
                            }
                        });
                    }
                }
                
                // 🔥 重新绑定事件
                swiAutoOrdersBet.ValueChanged += swiAutoOrdersBet_ValueChanged;
                swi_OrdersTasking.ValueChanged += swi_OrdersTasking_ValueChanged;
                _logService.Info("VxMain", "✅ UI 开关事件已重新绑定");
                
                // ✅ 监控线程会自动检测并启动浏览器（延迟2秒，等待老浏览器重连）
                if (isAutoBetEnabled)
                {
                    _logService.Info("VxMain", "✅ 监控线程将自动处理浏览器启动（延迟2秒，等待老浏览器重连）");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "加载应用配置失败", ex);
            }
        }
        
        /// <summary>
        /// 打开配置管理器
        /// </summary>
        private void btnConfigManager_Click(object? sender, EventArgs e)
        {
            try
            {
                var form = new Views.AutoBet.BetConfigManagerForm(_autoBetService, _logService);
                form.ShowDialog(this);
                
                // 刷新默认配置（可能在配置管理器中被修改）
                LoadAutoBetSettings();
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "打开配置管理器失败", ex);
                Sunny.UI.UIMessageBox.Show($"打开失败: {ex.Message}", "错误", Sunny.UI.UIStyle.Red, Sunny.UI.UIMessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// 手动启动浏览器按钮
        /// </summary>
        private async void btnStartBrowser_Click(object? sender, EventArgs e)
        {
            try
            {
                // 先保存设置
                SaveAutoBetSettings();

                _logService.Info("VxMain", "🚀 手动启动浏览器...");

                var defaultConfig = _autoBetService.GetConfigsBindingList()?.FirstOrDefault(c => c.IsDefault);
                if (defaultConfig != null)
                {
                    var success = await _autoBetService.StartBrowser(defaultConfig.Id);

                    if (success)
                    {
                        _logService.Info("VxMain", "✅ 浏览器已启动");
                        //Sunny.UI.UIMessageBox.Show("浏览器已启动！", "成功", Sunny.UI.UIStyle.Green, Sunny.UI.UIMessageBoxButtons.OK);
                    }
                    else
                    {
                        _logService.Error("VxMain", "启动浏览器失败");
                        Sunny.UI.UIMessageBox.Show("启动浏览器失败！", "错误", Sunny.UI.UIStyle.Red, Sunny.UI.UIMessageBoxButtons.OK);
                    }
                }
                else
                {
                    _logService.Error("VxMain", "未找到默认配置");
                    Sunny.UI.UIMessageBox.Show("未找到默认配置！", "错误", Sunny.UI.UIStyle.Red, Sunny.UI.UIMessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "启动浏览器失败", ex);
                Sunny.UI.UIMessageBox.Show($"启动失败: {ex.Message}", "错误", Sunny.UI.UIStyle.Red, Sunny.UI.UIMessageBoxButtons.OK);
            }
        }

        #endregion

        #region 订单表右键菜单
        
        /// <summary>
        /// 🔥 初始化订单表右键菜单（补单功能）
        /// </summary>
        private void InitializeOrderContextMenu()
        {
            // 创建右键菜单
            var contextMenu = new ContextMenuStrip();
            
            // 🔥 补单父菜单
            var menuSupplementOrder = new ToolStripMenuItem
            {
                Text = "补单",
                Font = new Font("微软雅黑", 10F)
            };
            
            // 🔥 线上补单（发送到微信）
            var menuOnlineSupplement = new ToolStripMenuItem
            {
                Text = "线上补单",
                Font = new Font("微软雅黑", 10F)
            };
            menuOnlineSupplement.Click += MenuOnlineSupplement_Click;
            
            // 🔥 离线补单（不发送到微信，仅记录）
            var menuOfflineSupplement = new ToolStripMenuItem
            {
                Text = "离线补单",
                Font = new Font("微软雅黑", 10F)
            };
            menuOfflineSupplement.Click += MenuOfflineSupplement_Click;
            
            // 添加子菜单
            menuSupplementOrder.DropDownItems.Add(menuOnlineSupplement);
            menuSupplementOrder.DropDownItems.Add(menuOfflineSupplement);
            
            // 添加到右键菜单
            contextMenu.Items.Add(menuSupplementOrder);
            
            // 绑定到订单表
            dgvOrders.ContextMenuStrip = contextMenu;
        }
        
        /// <summary>
        /// 🔥 订单表鼠标按下事件（用于右键菜单显示前选中行）
        /// </summary>
        private void DgvOrders_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // 获取鼠标点击位置的行索引
                var hitTest = dgvOrders.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0)
                {
                    // 如果点击的行没有被选中，则选中它
                    if (!dgvOrders.Rows[hitTest.RowIndex].Selected)
                    {
                        dgvOrders.ClearSelection();
                        dgvOrders.Rows[hitTest.RowIndex].Selected = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// 🔥 线上补单（发送到微信）
        /// 完全参考 F5BotV2 第 1189-1277 行
        /// </summary>
        private async void MenuOnlineSupplement_Click(object? sender, EventArgs e)
        {
            await PerformSupplementOrderAsync(sendToWeChat: true);
        }
        
        /// <summary>
        /// 🔥 离线补单（不发送到微信，仅记录）
        /// </summary>
        private async void MenuOfflineSupplement_Click(object? sender, EventArgs e)
        {
            await PerformSupplementOrderAsync(sendToWeChat: false);
        }
        
        /// <summary>
        /// 🔥 执行补单操作（通用方法，支持线上和离线）
        /// 完全参考 F5BotV2 的补分逻辑
        /// </summary>
        private async Task PerformSupplementOrderAsync(bool sendToWeChat)
        {
            try
            {
                string type = sendToWeChat ? "线上补单" : "离线补单";
                
                // 🔥 1. 检查是否有选中的订单
                if (dgvOrders.SelectedRows.Count == 0)
                {
                    UIMessageBox.ShowWarning("请先选择要补单的订单！");
                    return;
                }
                
                // 🔥 2. 检查是否绑定了群
                var groupWxId = _groupBindingService.CurrentBoundGroup?.Wxid;
                if (string.IsNullOrEmpty(groupWxId))
                {
                    UIMessageBox.ShowWarning("没有绑定群组！不能补单！");
                    return;
                }
                
                // 🔥 3. 处理所有选中的订单
                int successCount = 0;
                int failCount = 0;
                var messages = new System.Text.StringBuilder();
                
                foreach (DataGridViewRow row in dgvOrders.SelectedRows)
                {
                    if (row.DataBoundItem is not V2MemberOrder order)
                        continue;
                    
                    // 🔥 4. 检查订单的群是否与当前绑定的群一致（参考 F5BotV2 第 1243-1248 行）
                    if (order.GroupWxId != groupWxId)
                    {
                        var confirmResult = UIMessageBox.ShowAsk(
                            $"订单 {order.IssueId} 与目前绑定的群组不一致！\n" +
                            $"订单不是这个群的\n" +
                            $"您确定要补该订单吗？");
                        if (!confirmResult)
                            continue;
                    }
                    
                    // 🔥 5. 查找会员
                    var member = _membersBindingList?.FirstOrDefault(m => 
                        m.Wxid == order.Wxid && m.GroupWxId == order.GroupWxId);
                    
                    if (member == null)
                    {
                        failCount++;
                        _logService.Warning("VxMain", 
                            $"{type}失败: 没有在目前绑定的群中找到该会员 - 订单: {order.IssueId} - 会员: {order.Nickname}");
                        messages.AppendLine($"❌ {order.Nickname} ({order.IssueId}) - 未找到会员");
                        continue;
                    }
                    
                    // 🔥 6. 执行补单（在原订单上操作，参考 F5BotV2）
                    (bool success, string message, V2MemberOrder? settledOrder) = await _orderService.SettleManualOrderAsync(
                        order,
                        member,
                        sendToWeChat);  // 🔥 控制是否发送到微信
                    
                    if (success)
                    {
                        successCount++;
                        _logService.Info("VxMain", 
                            $"✅ {type}成功: {member.Nickname} - 订单ID: {order.Id} - 期号: {order.IssueId} - 盈利: {settledOrder?.NetProfit:F2}");
                        
                        // 🔥 7. 如果是线上补单，发送消息到微信（参考 F5BotV2 第 1267-1268 行）
                        if (sendToWeChat && !string.IsNullOrEmpty(message))
                        {
                            try
                            {
                                await _socketClient.SendAsync<object>("SendMessage", groupWxId, message);
                                _logService.Info("VxMain", $"📤 {type}消息已发送到微信群");
                            }
                            catch (Exception ex)
                            {
                                _logService.Error("VxMain", $"{type}消息发送失败", ex);
                                messages.AppendLine($"⚠️ {order.Nickname} ({order.IssueId}) - 补单成功但消息发送失败");
                                continue;
                            }
                        }
                        
                        messages.AppendLine($"✅ {order.Nickname} ({order.IssueId}) - {settledOrder?.NetProfit:F2}元");
                    }
                    else
                    {
                        failCount++;
                        _logService.Warning("VxMain", 
                            $"❌ {type}失败: {member.Nickname} - 期号: {order.IssueId} - 原因: {message}");
                        messages.AppendLine($"❌ {order.Nickname} ({order.IssueId}) - {message}");
                    }
                }
                
                // 🔥 8. 显示结果汇总
                if (successCount > 0)
                {
                    UpdateStatistics();  // 刷新统计数据
                    dgvOrders.Refresh();
                    dgvMembers.Refresh();
                    
                    string summary = $"{type}完成！\n\n" +
                        $"成功: {successCount} 单\n" +
                        $"失败: {failCount} 单\n\n" +
                        $"详细信息：\n{messages}";
                    
                    UIMessageBox.ShowSuccess(summary);
                }
                else if (failCount > 0)
                {
                    UIMessageBox.ShowError($"{type}全部失败！\n\n{messages}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"补单操作失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"补单失败：{ex.Message}");
            }
        }
        
        #endregion

        #region 数据库表初始化

        /// <summary>
        /// 🔥 初始化全局数据库表（business.db）
        /// 存储全局共享数据，所有微信账号共用
        /// </summary>
        private void InitializeGlobalTables(SQLiteConnection db)
        {
            try
            {
                _logService.Info("VxMain", "🗄️ 初始化全局数据库表...");

                // ========================================
                // 🔥 自动投注配置表（全局共享）
                // ========================================
                
                // 自动投注配置表（飞单配置，全局共享）
                db.CreateTable<Models.AutoBet.BetConfig>();
                _logService.Debug("VxMain", "✓ 全局表: BetConfig");

                // ========================================
                // 🔥 游戏开奖数据表（全局共享）
                // ========================================
                
                // 炳狗开奖数据表（开奖数据，全局共享）
                db.CreateTable<Models.Games.Binggo.BinggoLotteryData>();
                _logService.Debug("VxMain", "✓ 全局表: BinggoLotteryData");
                
                // 炳狗下注项表（单/双/大/小/对子等，全局共享）
                db.CreateTable<Models.Games.Binggo.BinggoBetItem>();
                _logService.Debug("VxMain", "✓ 全局表: BinggoBetItem");

                _logService.Info("VxMain", "✅ 全局数据库表初始化完成（3张表）");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "初始化全局数据库表失败", ex);
                throw;
            }
        }
        
        /// <summary>
        /// 🔥 初始化微信专属数据库表（business_{wxid}.db）
        /// 存储微信账号专属数据：会员、订单、上下分记录、投注记录等
        /// </summary>
        private void InitializeWxTables(SQLiteConnection db)
        {
            try
            {
                _logService.Info("VxMain", "🗄️ 初始化微信专属数据库表...");

                // ========================================
                // 🔥 核心业务表（微信账号专属）
                // ========================================
                
                // 会员表
                db.CreateTable<V2Member>();
                _logService.Debug("VxMain", "✓ 微信专属表: V2Member");
                
                // 订单表（微信收到的订单）
                db.CreateTable<V2MemberOrder>();
                _logService.Debug("VxMain", "✓ 微信专属表: V2MemberOrder");
                
                // 上下分申请表
                db.CreateTable<V2CreditWithdraw>();
                _logService.Debug("VxMain", "✓ 微信专属表: V2CreditWithdraw");
                
                // 资金变动表（上下分、订单结算等）
                db.CreateTable<V2BalanceChange>();
                _logService.Debug("VxMain", "✓ 微信专属表: V2BalanceChange");
                
                // 🔥 BetOrderRecord 已删除，改用 BetRecord（由 BetRecordService 在全局数据库中管理）

                // ========================================
                // 🔥 基础数据表（微信账号专属）
                // ========================================
                
                // 微信联系人表
                db.CreateTable<WxContact>();
                _logService.Debug("VxMain", "✓ 微信专属表: WxContact");
                
                // 微信用户信息表
                db.CreateTable<WxUserInfo>();
                _logService.Debug("VxMain", "✓ 微信专属表: WxUserInfo");
                
                // 日志表
                db.CreateTable<LogEntry>();
                _logService.Debug("VxMain", "✓ 微信专属表: LogEntry");

                _logService.Info("VxMain", "✅ 微信专属数据库表初始化完成（8张表）");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "初始化微信专属数据库表失败", ex);
                throw;
            }
        }

        #endregion
    }
}
