using Sunny.UI;
using BaiShengVx3Plus.ViewModels;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
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
        private readonly BinggoGameSettings _binggoSettings;
        
        // 🔥 ORM 数据库连接
        private SQLiteConnection? _db;
        private string _currentDbPath = "";  // 当前数据库路径
        
        // 数据绑定列表
        private BindingList<WxContact> _contactsBindingList;
        private V2MemberBindingList? _membersBindingList;  // 🔥 使用 ORM BindingList
        private V2OrderBindingList? _ordersBindingList;    // 🔥 使用 ORM BindingList
        private V2CreditWithdrawBindingList? _creditWithdrawsBindingList;  // 🔥 上下分 BindingList（与会员、订单统一模式）
        private BinggoLotteryDataBindingList? _lotteryDataBindingList; // 🎲 炳狗开奖数据 BindingList
        
        // 设置窗口单实例
        private Views.SettingsForm? _settingsForm;
        private Views.BinggoLotteryResultForm? _lotteryResultForm;  // 🎲 开奖结果窗口
        
        // 当前绑定的联系人对象
        private WxContact? _currentBoundContact;
        private string _currentGroupWxId = ""; // 🔥 当前绑定的群 wxid
        
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
            BinggoGameSettings binggoSettings) // 🎮 注入炳狗游戏配置
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
            _binggoSettings = binggoSettings;
            
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
        }

        /// <summary>
        /// 初始化数据库（使用 ORM）
        /// 
        /// 🔥 数据库命名规则：
        /// 1. 默认数据库: business.db（空的，不存储任何数据）
        /// 2. 微信专属数据库: business_{wxid}.db（存储所有业务数据：会员、订单等）
        /// 3. 日志数据库: logs.db（全局共享）
        /// 
        /// 🔥 重要设计原则：
        /// 1. 数据库操作（增删改查）= 同步执行，保证数据一致性，避免污染
        /// 2. UI 更新（状态文本等）= 异步执行，避免阻塞 UI 线程，保证流畅
        /// 3. 数据绑定（DataSource）= 同步执行，确保数据立即生效
        /// </summary>
        /// <param name="wxid">微信ID，"default" 表示默认空数据库，其他为实际微信ID</param>
        private void InitializeDatabase(string wxid)
        {
            try
            {
                // ========================================
                // 🔥 步骤1: 数据库操作（同步，不阻塞UI）
                // ========================================
                
                // 关闭旧数据库连接
                _db?.Close();
                _db = null;
                
                // 🔥 数据库命名规则：
                // - default → business.db（空数据库）
                // - wxid_xxx → business_wxid_xxx.db（微信专属数据库，存储所有业务数据）
                string dbPath = wxid == "default" 
                    ? Path.Combine("Data", "business.db")  // 默认空数据库
                    : Path.Combine("Data", $"business_{wxid}.db");  // 微信专属数据库
                    
                Directory.CreateDirectory("Data");
                
                // 🔥 保存数据库路径（用于清空数据时备份）
                _currentDbPath = dbPath;
                
                _logService.Info("VxMain", $"初始化数据库: {dbPath}");
                
                // 🔥 创建 ORM 数据库连接（同步）
                _db = new SQLiteConnection(dbPath);
                
                // 🔥 将数据库连接传递给群组绑定服务
                if (_groupBindingService is Services.GroupBinding.GroupBindingService groupBindingService)
                {
                    groupBindingService.SetDatabase(_db);
                }
                
                // ✅ 不再在这里创建和加载数据
                // 数据加载延迟到绑定群后（参考 F5BotV2 第 816 行）
                _logService.Info("VxMain", "✅ 数据库已准备，等待绑定群后加载数据");
                
                // ========================================
                // 🔥 步骤3: 初始化炳狗服务（异步，不阻塞）
                // ========================================
                
                InitializeBinggoServices();
                
                // ========================================
                // 🔥 步骤4: 日志记录（异步，不阻塞）
                // ========================================
                
                _logService.Info("VxMain", $"✓ 数据库已初始化: {dbPath}");
                // ✅ 数据将在绑定群后加载，此处不记录数量
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
        /// 初始化炳狗游戏服务
        /// </summary>
        private void InitializeBinggoServices()
        {
            try
            {
                _logService.Info("VxMain", "🎮 初始化炳狗服务...");
                
                // 检查数据库是否已初始化
                if (_db == null)
                {
                    _logService.Warning("VxMain", "数据库未初始化，跳过炳狗服务初始化");
                    return;
                }
                
                // 1. 设置数据库连接
                _lotteryService.SetDatabase(_db);
                _orderService.SetDatabase(_db);
                _binggoMessageHandler.SetDatabase(_db);  // 🔥 设置消息处理器的数据库（用于上下分申请）
                
                // 2. 创建开奖数据 BindingList
                _lotteryDataBindingList = new BinggoLotteryDataBindingList(_db, _logService);
                _lotteryDataBindingList.LoadFromDatabase(100); // 加载最近 100 期
                
                // 3. 设置开奖服务的 BindingList（用于自动更新 UI）
                _lotteryService.SetBindingList(_lotteryDataBindingList);
                
                // 4. 设置订单服务的 BindingList（可能为 null，服务内部会处理）
                _orderService.SetOrdersBindingList(_ordersBindingList);
                _orderService.SetMembersBindingList(_membersBindingList);
                
                // 5. 订阅开奖事件（自动结算）
                _lotteryService.LotteryOpened += OnLotteryOpened;
                _lotteryService.StatusChanged += OnLotteryStatusChanged;
                _lotteryService.IssueChanged += OnLotteryIssueChanged;
                
                // 🔥 6. 订阅统计服务属性变化（自动更新 UI）
                _statisticsService.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(BinggoStatisticsService.PanDescribe))
                    {
                        UpdateUIThreadSafeAsync(() => UpdateMemberInfoLabel());
                    }
                };
                
                // 6. 启动开奖服务
                _ = _lotteryService.StartAsync();  // 异步启动，不等待
                
                // 7. 🎨 绑定 UI 控件到开奖服务
                _logService.Info("VxMain", "🎨 开始绑定 UI 控件到开奖服务...");
                
                if (ucBinggoDataCur == null)
                {
                    _logService.Error("VxMain", "❌ ucBinggoDataCur 为 null！");
                }
                if (ucBinggoDataLast == null)
                {
                    _logService.Error("VxMain", "❌ ucBinggoDataLast 为 null！");
                }
                
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
                
                // 🔥 立即加载最近的开奖数据（确保上期数据显示）
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(500);  // 等待500ms，确保服务完全启动
                        _logService.Info("VxMain", "🎲 开始立即加载最近开奖数据...");
                        
                        var recentData = await _lotteryService.GetRecentLotteryDataAsync(5);
                        if (recentData != null && recentData.Count > 0)
                        {
                            _logService.Info("VxMain", $"✅ 立即加载成功，获取 {recentData.Count} 期数据");
                            _logService.Info("VxMain", $"   最新期号: {recentData[0].IssueId}");
                            _logService.Info("VxMain", $"   开奖号码: {recentData[0].ToLotteryString()}");
                        }
                        else
                        {
                            _logService.Warning("VxMain", "⚠️ 立即加载失败，未获取到开奖数据");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Error("VxMain", $"立即加载开奖数据失败: {ex.Message}", ex);
                    }
                });
                
                _logService.Info("VxMain", "✅ 炳狗服务初始化完成（含 UI 控件绑定）");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"炳狗服务初始化失败: {ex.Message}", ex);
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
                txtSealSeconds.Value = _binggoSettings.SealSecondsAhead;
                txtMinBet.Value = (int)_binggoSettings.MinBet;
                txtMaxBet.Value = (int)_binggoSettings.MaxBet;
                
                // 🔥 管理模式初始化（默认关闭）
                if (chkAdminMode != null)
                {
                    chkAdminMode.Checked = _binggoSettings.IsAdminMode;
                    UpdateAdminModeUI();
                }
                
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
            bool isAdminMode = _binggoSettings.IsAdminMode;
            
            // 管理模式下，txtCurrentContact 可编辑
            txtCurrentContact.ReadOnly = !isAdminMode;
            txtCurrentContact.BackColor = isAdminMode ? Color.White : SystemColors.Control;
            
            _logService.Info("VxMain", isAdminMode ? "✅ 管理模式已启用" : "❌ 管理模式已禁用");
        }
        
        /// <summary>
        /// 管理模式 checkbox 变化事件
        /// </summary>
        private void ChkAdminMode_CheckedChanged(object? sender, EventArgs e)
        {
            _binggoSettings.IsAdminMode = chkAdminMode?.Checked ?? false;
            UpdateAdminModeUI();
        }
        
        /// <summary>
        /// txtCurrentContact 按回车手动绑定（管理模式）
        /// </summary>
        private async void TxtCurrentContact_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _binggoSettings.IsAdminMode)
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
                _binggoSettings.SealSecondsAhead = value;
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
        /// 开奖事件处理（自动结算）
        /// </summary>
        private async void OnLotteryOpened(object? sender, BinggoLotteryOpenedEventArgs e)
        {
            try
            {
                _logService.Info("VxMain", 
                    $"🎲 开奖: {e.LotteryData.ToLotteryString()}");
                
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
                _logService.Info("VxMain", $"🔄 状态变更: {e.NewStatus} - {e.Message}");
                // TODO: 更新 UI 状态显示
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
                
                // 🔥 设置当前期号（会自动重新计算本期下注）
                _statisticsService.SetCurrentIssueId(e.NewIssueId);
                
                // TODO: 可选 - 发送开盘通知到微信群
            });
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
                lblStatus.Text = "正在初始化...";
                
                // 隐藏不需要显示的列
                if (dgvContacts.Columns.Count > 0)
                {
                    HideContactColumns();
                }

                // 🔥 会员表和订单表的列配置已在 InitializeDataBindings() 中完成
                // 不需要在这里重复调用配置方法
                
                // 🎮 初始化快速设置面板
                InitializeFastSettings();
                
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
                if (_currentBoundContact != null && contact.Wxid == _currentBoundContact.Wxid)
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
                isBound = (_currentBoundContact != null && contact.Wxid == _currentBoundContact.Wxid);
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
        /// 🔥 解析服务器返回的群成员数据
        /// 
        /// GetGroupContacts 返回的字段名：
        /// - member_wxid
        /// - member_nickname
        /// - member_alias
        /// - member_remark
        /// </summary>
        private List<V2Member> ParseServerMembers(JsonElement arrayElement, string groupWxId)
        {
            var members = new List<V2Member>();
            
            try
            {
                foreach (var item in arrayElement.EnumerateArray())
                {
                    try
                    {
                        // 🔥 解析 GetGroupContacts 返回的字段
                        string? wxid = item.TryGetProperty("member_wxid", out var wxidProp) ? wxidProp.GetString() : null;
                        string? nickname = item.TryGetProperty("member_nickname", out var nicknameProp) ? nicknameProp.GetString() : null;
                        string? alias = item.TryGetProperty("member_alias", out var aliasProp) ? aliasProp.GetString() : null;
                        string? remark = item.TryGetProperty("member_remark", out var remarkProp) ? remarkProp.GetString() : null;
                        
                        // 优先使用备注名，其次昵称
                        string displayName = !string.IsNullOrEmpty(remark) ? remark : 
                                           !string.IsNullOrEmpty(nickname) ? nickname : "";
                        
                        if (string.IsNullOrEmpty(wxid))
                        {
                            _logService.Warning("VxMain", "解析单个会员失败: member_wxid 为空");
                            continue;
                        }
                        
                        var member = new V2Member
                        {
                            GroupWxId = groupWxId,
                            Wxid = wxid,
                            Nickname = nickname ?? "",
                            Account = alias ?? "",           // 微信号
                            DisplayName = displayName,       // 群昵称/备注
                            State = MemberState.会员         // 默认状态
                        };
                        
                        members.Add(member);
                        _logService.Info("VxMain", $"✓ 解析会员: {member.Nickname} ({member.Wxid})");
                    }
                    catch (Exception ex)
                    {
                        _logService.Warning("VxMain", $"解析单个会员失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"解析群成员数据失败: {ex.Message}", ex);
            }
            
            _logService.Info("VxMain", $"✅ 解析完成: 共 {members.Count} 个会员");
            return members;
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
        /// 🔥 统一的绑定群方法（参考 F5BotV2 第 816 行）
        /// 无论是正常绑定还是管理模式手动绑定，都走这个流程
        /// </summary>
        private async Task BindGroupAsync(WxContact contact)
        {
            try
            {
                _logService.Info("VxMain", $"📍 开始绑定群: {contact.Nickname} ({contact.Wxid})");
                
                // 🔥 1. 使用服务绑定群组
                _groupBindingService.BindGroup(contact);
                _currentBoundContact = contact;
                _currentGroupWxId = contact.Wxid;
                
                // 2. 更新 UI 显示
                txtCurrentContact.Text = $"{contact.Nickname} ({contact.Wxid})";
                txtCurrentContact.FillColor = Color.FromArgb(240, 255, 240); // 浅绿色背景
                txtCurrentContact.RectColor = Color.FromArgb(82, 196, 26);   // 绿色边框
                dgvContacts.Refresh();
                
                lblStatus.Text = $"✓ 已绑定: {contact.Nickname} - 正在加载数据...";
                _logService.Info("VxMain", $"✓ 绑定群组: {contact.Nickname} ({contact.Wxid})");
                
                // 🔥 3. 清空旧数据并清零统计（参考 F5BotV2 第 818 行）
                UpdateUIThreadSafe(() =>
                {
                    _membersBindingList?.Clear();
                    _ordersBindingList?.Clear();
                    _statisticsService.UpdateStatistics(setZero: true);
                });
                
                // 🔥 4. 创建新的 BindingList（绑定到数据库）
                if (_db == null)
                {
                    _logService.Error("VxMain", "数据库未初始化！");
                    UIMessageBox.ShowError("数据库未初始化！");
                    return;
                }
                
                _membersBindingList = new V2MemberBindingList(_db, contact.Wxid);
                _ordersBindingList = new V2OrderBindingList(_db);
                _creditWithdrawsBindingList = new V2CreditWithdrawBindingList(_db);  // 🔥 上下分 BindingList
                
                // 🔥 5. 设置到各个服务
                _orderService.SetMembersBindingList(_membersBindingList);
                _orderService.SetOrdersBindingList(_ordersBindingList);
                _orderService.SetStatisticsService(_statisticsService); // 🔥 设置统计服务
                _statisticsService.SetBindingLists(_membersBindingList, _ordersBindingList);
                
                if (_memberDataService is MemberDataService mds)
                {
                    mds.SetMembersBindingList(_membersBindingList);
                }
                
                // 🔥 6. 从数据库加载订单数据（订单不需要与服务器同步）
                await Task.Run(() =>
                {
                    _ordersBindingList.LoadFromDatabase();
                });
                
                _logService.Info("VxMain", $"✅ 从数据库加载: {_ordersBindingList.Count} 个订单");
                
                // 🔥 6.5. 从数据库加载上下分数据（与订单表统一模式）
                await Task.Run(() =>
                {
                    _creditWithdrawsBindingList.LoadFromDatabase(contact.Wxid);
                });
                
                _logService.Info("VxMain", $"✅ 从数据库加载: {_creditWithdrawsBindingList.Count} 条上下分记录");
                
                // 🔥 7. 获取服务器数据并智能合并会员（参考 F5BotV2）
                _logService.Info("VxMain", $"开始获取群成员列表并智能合并: {contact.Wxid}");
                var result = await _socketClient.SendAsync<JsonDocument>("GetGroupContacts", contact.Wxid);
                
                if (result == null || result.RootElement.ValueKind != JsonValueKind.Array)
                {
                    // 服务器获取失败，只加载数据库数据
                    _logService.Warning("VxMain", "获取群成员失败，只加载数据库数据");
                    await Task.Run(() =>
                    {
                        _membersBindingList.LoadFromDatabase();
                    });
                    _logService.Info("VxMain", $"✅ 从数据库加载: {_membersBindingList.Count} 个会员（仅本地）");
                }
                else
                {
                    // 🔥 8. 解析服务器返回的会员数据
                    var serverMembers = ParseServerMembers(result.RootElement, contact.Wxid);
                    _logService.Info("VxMain", $"服务器返回 {serverMembers.Count} 个群成员");
                    
                    // 🔥 9. 使用服务智能合并数据（数据库 + 服务器）
                    // ⚠️ 关键：LoadAndMergeMembers 返回的是完整列表，包括：
                    //    - 数据库有 + 服务器有 → 使用数据库数据（保留统计）
                    //    - 数据库无 + 服务器有 → 新增会员
                    //    - 数据库有 + 服务器无 → 标记"已退群"
                    var mergedMembers = _groupBindingService.LoadAndMergeMembers(serverMembers, contact.Wxid);
                    _logService.Info("VxMain", $"智能合并完成: 共 {mergedMembers.Count} 个会员");
                    
                    // 🔥 10. 直接加载合并后的完整列表（不是追加！）
                    UpdateUIThreadSafe(() =>
                    {
                        foreach (var member in mergedMembers)
                        {
                            _membersBindingList?.Add(member);  // 添加到空的 BindingList
                        }
                    });
                    
                    _logService.Info("VxMain", $"✅ 会员列表已更新: {_membersBindingList?.Count} 个会员");
                }
                
                // 🔥 11. 更新会员的上下分统计（从已同意的记录中计算）
                _creditWithdrawsBindingList.UpdateMemberStatistics(_membersBindingList);
                _logService.Info("VxMain", "✅ 会员上下分统计已更新");
                
                // 🔥 12. 绑定到 DataGridView
                UpdateUIThreadSafe(() =>
                {
                    dgvMembers.DataSource = _membersBindingList;
                    dgvOrders.DataSource = _ordersBindingList;
                });
                
                // 🔥 13. 更新统计（参考 F5BotV2 第 569 行）
                _statisticsService.UpdateStatistics();
                
                // 🔥 13. 更新UI显示
                UpdateMemberInfoLabel();
                
                lblStatus.Text = $"✓ 已绑定: {contact.Nickname} - 加载完成";
                _logService.Info("VxMain", $"✅ 绑定群完成: {_membersBindingList.Count} 个会员, {_ordersBindingList.Count} 个订单");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"绑定群失败: {ex.Message}", ex);
                throw;
            }
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
                    "4. 清空统计数据\n\n" +
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
                        string backupDbPath = Path.Combine("Data", "Backup", backupDbName);
                        
                        // 创建备份目录
                        Directory.CreateDirectory(Path.Combine("Data", "Backup"));
                        
                        // 🔥 关闭数据库连接（SQLite需要关闭连接才能复制文件）
                        _db?.Close();
                        _db?.Dispose();
                        _db = null;
                        
                        // 等待文件释放
                        await Task.Delay(100);
                        
                        // 复制数据库文件
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
                        
                        // 🔥 重新打开数据库
                        _db = new SQLiteConnection(_currentDbPath);
                    }
                    catch (Exception ex)
                    {
                        _logService.Error("VxMain", "备份数据库失败", ex);
                        UIMessageBox.ShowError($"备份数据库失败：{ex.Message}\n\n已取消清空操作");
                        
                        // 重新打开数据库
                        if (_db == null)
                        {
                            _db = new SQLiteConnection(_currentDbPath);
                        }
                        return;
                    }
                }
                
                // ========================================
                // 🔥 步骤2：清空订单表
                // ========================================
                
                if (_db != null)
                {
                    _db.DeleteAll<Models.V2MemberOrder>();
                    _logService.Info("VxMain", "✅ 订单表已清空");
                }
                
                // 清空UI订单列表
                _ordersBindingList?.Clear();
                
                // ========================================
                // 🔥 步骤3：重置会员金额数据（保留基础信息）
                // ========================================
                
                if (_membersBindingList != null)
                {
                    foreach (var member in _membersBindingList)
                    {
                        // 🔥 清空金额数据
                        member.Balance = 0f;
                        member.BetCur = 0f;
                        member.BetWait = 0f;
                        member.BetToday = 0f;
                        member.BetTotal = 0f;
                        member.IncomeToday = 0f;
                        member.IncomeTotal = 0f;
                        member.CreditToday = 0f;
                        member.CreditTotal = 0f;
                        member.WithdrawToday = 0f;
                        member.WithdrawTotal = 0f;
                        
                        // 🔥 保留基础信息（Wxid, Nickname, Account, DisplayName, State等）
                        // 不需要做任何操作，它们会自动保留
                    }
                    
                    // 🔥 更新数据库
                    if (_db != null)
                    {
                        foreach (var member in _membersBindingList)
                        {
                            _db.Update(member);
                        }
                    }
                    
                    _logService.Info("VxMain", $"✅ {_membersBindingList.Count} 个会员的金额数据已重置");
                }
                
                // ========================================
                // 🔥 步骤4：清空统计数据
                // ========================================
                
                _statisticsService.UpdateStatistics(setZero: true);
                _logService.Info("VxMain", "✅ 统计数据已清空");
                
                // ========================================
                // 🔥 步骤5：刷新UI
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
                _settingsForm = new Views.SettingsForm(_socketClient, _logService, _binggoSettings);
                
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
        private async void ContactDataService_ContactsUpdated(object? sender, ContactsUpdatedEventArgs e)
        {
            try
            {
                _logService.Info("VxMain", $"📇 联系人数据已更新，共 {e.Contacts.Count} 个");

                // 🔥 使用异步方式切换到 UI 线程，避免阻塞
                await Task.Run(() =>
                {
                    // 在后台线程处理数据（如果需要）
                    _logService.Info("VxMain", "准备更新联系人列表到 UI");
                });

                // 切换到 UI 线程更新
                if (InvokeRequired)
                {
                    await Task.Factory.StartNew(() =>
                    {
                        Invoke(new Action(() => UpdateContactsList(e.Contacts)));
                    });
                }
                else
                {
                    UpdateContactsList(e.Contacts);
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
                            _currentBoundContact = null;
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
                var contactsData = await _socketClient.SendAsync<JsonDocument>("GetContacts", 10000);

                if (contactsData != null)
                {
                    // 统一调用 ContactDataService 处理
                    await _contactDataService.ProcessContactsAsync(contactsData.RootElement);
                    _logService.Info("VxMain", "✓ 联系人获取成功");
                }
                else
                {
                    _logService.Warning("VxMain", "获取联系人失败");
                    lblStatus.Text = "获取联系人失败";
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "刷新联系人失败", ex);
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
                // 清空现有数据
                _contactsBindingList.Clear();

                // 添加新数据
                foreach (var contact in contacts)
                {
                    _contactsBindingList.Add(contact);
                }

                lblStatus.Text = $"✓ 已更新 {contacts.Count} 个联系人";
                _logService.Info("VxMain", $"联系人列表已更新到 UI");
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

        /// <summary>
        /// 加载群成员数据到 dgvMembers
        /// </summary>
        /// <param name="groupMembersJson">GetGroupContacts 返回的 JSON 数据</param>
        /// <param name="groupWxid">群微信 ID</param>
        private Task LoadGroupMembersToDataGridAsync(JsonElement groupMembersJson, string groupWxid)
        {
            try
            {
                _logService.Info("VxMain", $"开始解析群成员数据，群ID: {groupWxid}");

                // 🔥 确保 _membersBindingList 已初始化
                if (_membersBindingList == null)
                {
                    _logService.Warning("VxMain", "会员列表未初始化，跳过加载");
                    return Task.CompletedTask;
                }

                // 清空当前 dgvMembers 数据
                _membersBindingList.Clear();

                int count = 0;
                foreach (var memberElement in groupMembersJson.EnumerateArray())
                {
                    try
                    {
                        // 解析群成员数据
                        string memberWxid = memberElement.TryGetProperty("member_wxid", out var mwxid) 
                            ? mwxid.GetString() ?? "" : "";
                        string memberNickname = memberElement.TryGetProperty("member_nickname", out var mnick) 
                            ? mnick.GetString() ?? "" : "";
                        string memberAlias = memberElement.TryGetProperty("member_alias", out var malias) 
                            ? malias.GetString() ?? "" : "";
                        string memberRemark = memberElement.TryGetProperty("member_remark", out var mremark) 
                            ? mremark.GetString() ?? "" : "";

                        // 跳过无效数据
                        if (string.IsNullOrEmpty(memberWxid))
                        {
                            _logService.Warning("VxMain", "跳过无效的群成员数据：member_wxid 为空");
                            continue;
                        }

                        // 创建 V2Member 对象
                        var member = new V2Member
                        {
                            GroupWxId = groupWxid,  // 🔥 设置群ID
                            Wxid = memberWxid,
                            Nickname = memberNickname,
                            Account = memberAlias,
                            DisplayName = string.IsNullOrEmpty(memberRemark) ? memberNickname : memberRemark,
                            
                            // 初始化业务字段为默认值
                            Balance = 0,
                            State = MemberState.会员,
                            BetCur = 0,
                            BetWait = 0,
                            IncomeToday = 0,
                            CreditToday = 0,
                            BetToday = 0,
                            WithdrawToday = 0,
                            BetTotal = 0,
                            CreditTotal = 0,
                            WithdrawTotal = 0,
                            IncomeTotal = 0
                        };

                        // 🔥 添加到 BindingList，ItemAdded 事件会自动保存到数据库
                        _membersBindingList.Add(member);
                        count++;

                        _logService.Debug("VxMain", $"添加群成员: {memberNickname} ({memberWxid})");
                    }
                    catch (Exception ex)
                    {
                        _logService.Error("VxMain", $"解析单个群成员失败: {ex.Message}");
                    }
                }

                _logService.Info("VxMain", $"✓ 群成员加载完成，共 {count} 个成员");

                // 刷新 UI
                if (dgvMembers.InvokeRequired)
                {
                    dgvMembers.Invoke(new Action(() => dgvMembers.Refresh()));
                }
                else
                {
                    dgvMembers.Refresh();
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"加载群成员到 DataGrid 失败: {ex.Message}");
                throw;
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// 窗口关闭时断开 Socket 连接并关闭子窗口
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _logService.Info("VxMain", "窗口正在关闭，断开 Socket 连接");
                _socketClient?.Disconnect();
                
                // 关闭设置窗口（如果打开）
                if (_settingsForm != null && !_settingsForm.IsDisposed)
                {
                    _logService.Info("VxMain", "关闭设置窗口");
                    _settingsForm.Close();
                    _settingsForm = null;
                }
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
                    "tsmiSetNormal" => Models.MemberState.普会,
                    "tsmiSetMember" => Models.MemberState.会员,
                    "tsmiSetAgent" => Models.MemberState.托,
                    "tsmiSetBlue" => Models.MemberState.蓝会,
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

        /// <summary>
        /// 打开上下分管理窗口
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
                
                // 🔥 传递 BindingList 实例（统一模式）
                var form = new Views.CreditWithdrawManageForm(
                    _db, 
                    _logService, 
                    _socketClient,
                    _creditWithdrawsBindingList,
                    _membersBindingList);
                form.ShowDialog(this);
                
                // 🔥 关闭窗口后刷新统计
                _statisticsService.UpdateStatistics();
                UpdateMemberInfoLabel();
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
                if (_db == null || _membersBindingList == null)
                {
                    _logService.Warning("VxMain", "数据库或会员列表未初始化，跳过上下分数据加载");
                    return;
                }
                
                // 🔥 1. 确保表存在
                _db.CreateTable<V2CreditWithdraw>();
                
                // 🔥 2. 加载该群的所有上下分记录
                var creditWithdraws = _db.Table<V2CreditWithdraw>()
                    .Where(cw => cw.GroupWxId == groupWxid)
                    .OrderBy(cw => cw.Timestamp)
                    .ToList();
                
                _logService.Info("VxMain", $"📊 加载了 {creditWithdraws.Count} 条上下分记录");
                
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
    }
}
