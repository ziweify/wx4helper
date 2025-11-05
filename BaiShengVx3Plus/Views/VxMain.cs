using Sunny.UI;
using BaiShengVx3Plus.ViewModels;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Services.Messages;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace BaiShengVx3Plus
{
    public partial class VxMain : UIForm
    {
        private readonly VxMainViewModel _viewModel;
        private readonly IContactBindingService _contactBindingService;
        private readonly ILogService _logService;
        private readonly IWeixinSocketClient _socketClient; // Socket 客户端
        private readonly MessageDispatcher _messageDispatcher; // 消息分发器
        private readonly IContactDataService _contactDataService; // 联系人数据服务
        private readonly IUserInfoService _userInfoService; // 用户信息服务
        private readonly IWeChatService _wechatService; // 微信应用服务（Application Service）
        private readonly IMemberService _memberService; // 🔥 会员服务（自动追踪）
        private readonly IOrderService _orderService; // 🔥 订单服务（自动追踪）
        private BindingList<WxContact> _contactsBindingList;
        private BindingList<V2Member> _membersBindingList;
        private BindingList<V2MemberOrder> _ordersBindingList;
        
        // 设置窗口单实例
        private Views.SettingsForm? _settingsForm;
        
        // 当前绑定的联系人对象
        private WxContact? _currentBoundContact;
        
        // 连接取消令牌
        private CancellationTokenSource? _connectCts;

        public VxMain(
            VxMainViewModel viewModel,
            IContactBindingService contactBindingService,
            ILogService logService,
            IWeixinSocketClient socketClient,
            MessageDispatcher messageDispatcher,
            IContactDataService contactDataService, // 注入联系人数据服务
            IUserInfoService userInfoService, // 注入用户信息服务
            IWeChatService wechatService, // 注入微信应用服务
            IMemberService memberService, // 🔥 注入会员服务（自动追踪）
            IOrderService orderService) // 🔥 注入订单服务（自动追踪）
        {
            InitializeComponent();
            _viewModel = viewModel;
            _contactBindingService = contactBindingService;
            _logService = logService;
            _socketClient = socketClient;
            _messageDispatcher = messageDispatcher;
            _contactDataService = contactDataService;
            _userInfoService = userInfoService;
            _wechatService = wechatService;
            _memberService = memberService;
            _orderService = orderService;
            
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
            
            // 绑定用户信息到用户控件
            ucUserInfo1.UserInfo = _userInfoService.CurrentUser;
            
            // 订阅用户控件的连接按钮事件
            ucUserInfo1.CollectButtonClick += UcUserInfo_CollectButtonClick;
            
            // 记录主窗口打开
            _logService.Info("VxMain", "主窗口已打开");

            // 🔥 初始化数据绑定列表（从服务加载，自动追踪属性变化）
            _contactsBindingList = new BindingList<WxContact>(); // 联系人稍后异步加载
            _membersBindingList = _memberService.GetAllMembers();  // 会员立即加载（自动追踪）
            _ordersBindingList = _orderService.GetAllOrders();     // 订单立即加载（自动追踪）

            // 联系人列表手动配置（异步加载）
            _contactsBindingList.AllowEdit = true;
            _contactsBindingList.AllowNew = false;
            _contactsBindingList.AllowRemove = false;

            _logService.Info("VxMain", $"✓ 加载 {_membersBindingList.Count} 个会员，{_ordersBindingList.Count} 个订单（已自动追踪）");

            InitializeDataBindings();
        }

        private void InitializeDataBindings()
        {
            // 绑定联系人列表
            dgvContacts.DataSource = _contactsBindingList;
            dgvContacts.AutoGenerateColumns = true;
            dgvContacts.ReadOnly = true;

            // 绑定会员列表
            dgvMembers.DataSource = _membersBindingList;
            dgvMembers.AutoGenerateColumns = true;
            dgvMembers.EditMode = DataGridViewEditMode.EditOnEnter;

            // 设置会员表字段可见性和顺序
            dgvMembers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            // 绑定订单列表
            dgvOrders.DataSource = _ordersBindingList;
            dgvOrders.AutoGenerateColumns = true;
            dgvOrders.EditMode = DataGridViewEditMode.EditOnEnter;

            // 设置订单表字段可见性和顺序
            dgvOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 添加测试数据
            LoadTestData();
        }

        private void LoadTestData()
        {
            // ✅ 联系人数据已删除，改为从服务器获取

            // 添加测试会员数据
            for (int i = 1; i <= 10; i++)
            {
                var member = new V2Member
                {
                    Id = i,
                    Wxid = $"wxid_{i:D3}",
                    Account = $"13800138{i:D3}",
                    Nickname = $"会员{i}",
                    DisplayName = $"群昵称{i}",
                    Balance = 1000 + i * 100,
                    State = i % 3 == 0 ? MemberState.管理 : (i % 2 == 0 ? MemberState.托 : MemberState.会员),
                    BetCur = i * 50,
                    BetWait = i * 20,
                    IncomeToday = i * 10 - 50,
                    CreditToday = i * 100,
                    BetToday = i * 80,
                    WithdrawToday = i * 30,
                    BetTotal = i * 1000,
                    CreditTotal = i * 2000,
                    WithdrawTotal = i * 500,
                    IncomeTotal = i * 200 - 100
                };
                _membersBindingList.Add(member);
            }

            // 添加测试订单数据
            for (int i = 1; i <= 20; i++)
            {
                var order = new V2MemberOrder
                {
                    Id = i,
                    Wxid = $"wxid_{(i % 10) + 1:D3}",
                    Account = $"13800138{(i % 10) + 1:D3}",
                    Nickname = $"会员{(i % 10) + 1}",
                    IssueId = 241104001 + i,
                    BetContentOriginal = $"1,2,3,4,5*10",
                    BetContentStandar = $"1,大,10;2,小,10;3,单,10",
                    Nums = 3,
                    AmountTotal = 30,
                    Profit = i % 2 == 0 ? 59.1f : 0,
                    NetProfit = i % 2 == 0 ? 29.1f : -30,
                    Odds = 1.97f,
                    OrderStatus = i % 3 == 0 ? OrderStatus.已完成 : (i % 2 == 0 ? OrderStatus.待结算 : OrderStatus.待处理),
                    OrderType = i % 2 == 0 ? OrderType.盘内 : OrderType.待定,
                    TimeStampBet = (long)DateTimeOffset.Now.AddMinutes(-i).ToUnixTimeSeconds(),
                    TimeString = DateTime.Now.AddMinutes(-i).ToString("yyyy-MM-dd HH:mm:ss"),
                    Notes = i % 5 == 0 ? "重要订单" : ""
                };
                _ordersBindingList.Add(order);
            }

            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            //lblContactList.Text = $"联系人列表({_contactsBindingList.Count})";
            lblMemberInfo.Text = $"会员列表 (共{_membersBindingList.Count}人)";
            lblOrderInfo.Text = $"订单列表 (共{_ordersBindingList.Count}单)";
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

                if (dgvMembers.Columns.Count > 0)
                {
                    HideMemberColumns();
                }

                if (dgvOrders.Columns.Count > 0)
                {
                    HideOrderColumns();
                }
                
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
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "窗口加载时发生错误", ex);
                lblStatus.Text = "初始化失败";
            }
        }

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

        private void HideMemberColumns()
        {
            // 隐藏Id列
            if (dgvMembers.Columns["Id"] != null)
                dgvMembers.Columns["Id"].Visible = false;

            if (dgvMembers.Columns["GroupWxId"] != null)
                dgvMembers.Columns["GroupWxId"].Visible = false;
        }

        private void HideOrderColumns()
        {
            // 隐藏Id列
            if (dgvOrders.Columns["Id"] != null)
                dgvOrders.Columns["Id"].Visible = false;

            if (dgvOrders.Columns["GroupWxId"] != null)
                dgvOrders.Columns["GroupWxId"].Visible = false;

            if (dgvOrders.Columns["TimeStampBet"] != null)
                dgvOrders.Columns["TimeStampBet"].Visible = false;
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

        private void btnBindingContacts_Click(object sender, EventArgs e)
        {
            if (dgvContacts.CurrentRow?.DataBoundItem is WxContact contact)
            {
                // 保存当前绑定的联系人对象
                _currentBoundContact = contact;
                
                // 调用服务保存绑定
                _contactBindingService.BindContact(contact);
                
                // 更新联系人列表编辑框显示
                if (this.Controls.Find("txtCurrentContact", true).FirstOrDefault() is Sunny.UI.UITextBox txt)
                {
                    txt.Text = $"{contact.Nickname} ({contact.Wxid})";
                }
                
                lblStatus.Text = $"已绑定联系人: {contact.Nickname} ({contact.Wxid})";
                _logService.Info("VxMain", $"绑定联系人: {contact.Nickname} ({contact.Wxid}), IsGroup: {contact.IsGroup}");
                UIMessageBox.ShowSuccess($"成功绑定联系人: {contact.Nickname}");
            }
            else
            {
                _logService.Warning("VxMain", "绑定联系人失败: 未选择联系人");
                UIMessageBox.ShowWarning("请先选择一个联系人");
            }
        }

        /// <summary>
        /// 用户控件的连接按钮点击事件
        /// 功能：启动微信（如果未启动）→ 注入 DLL → 连接 Socket → 自动获取用户信息和联系人
        /// </summary>
        /// <summary>
        /// 用户控件的连接按钮点击事件（使用新的 WeChatService）
        /// </summary>
        private async void UcUserInfo_CollectButtonClick(object? sender, EventArgs e)
        {
            try
            {
                // 取消之前的连接（如果有）
                _connectCts?.Cancel();
                _connectCts = new CancellationTokenSource();

                _logService.Info("VxMain", "用户点击连接按钮");

                // 调用微信应用服务进行连接和初始化
                // forceRestart = false，让服务自动判断
                // 状态更新由 WeChatService_ConnectionStateChanged 事件处理
                var success = await _wechatService.ConnectAndInitializeAsync(forceRestart: false, _connectCts.Token);
                
                _logService.Info("VxMain", $">>> 连接和初始化完成，结果: {success}");
                
                // 如果成功，检查联系人列表
                if (success)
                {
                    _logService.Info("VxMain", $">>> dgvContacts 行数: {dgvContacts.Rows.Count}");
                    _logService.Info("VxMain", $">>> _contactsBindingList 数量: {_contactsBindingList.Count}");
                }
            }
            catch (OperationCanceledException)
            {
                _logService.Info("VxMain", "连接被用户取消");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "连接失败", ex);
                UIMessageBox.ShowError($"连接失败:\n{ex.Message}");
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

            // 连接中时禁用按钮，其他状态启用
            ucUserInfo1.SetCollectButtonEnabled(!isConnecting);

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

        private void btnOpenLotteryResult_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "打开开奖结果窗口...";
            // TODO: 实现开奖结果窗口
        }

        private void btnClearData_Click(object sender, EventArgs e)
        {
            if (UIMessageBox.ShowAsk("确定要清空所有数据吗？"))
            {
                _contactsBindingList.Clear();
                _membersBindingList.Clear();
                _ordersBindingList.Clear();
                UpdateStatistics();
                lblStatus.Text = "数据已清空";
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
                _settingsForm = new Views.SettingsForm(_socketClient, _logService);
                
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

                // 线程安全地更新 UI
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        // ✅ 更新用户信息显示
                        ucUserInfo1.UserInfo = e.UserInfo;
                    }));
                }
                else
                {
                    // ✅ 更新用户信息显示
                    ucUserInfo1.UserInfo = e.UserInfo;
                }

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
    }
}
