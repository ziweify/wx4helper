using Sunny.UI;
using BaiShengVx3Plus.ViewModels;
using BaiShengVx3Plus.Models;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace BaiShengVx3Plus
{
    public partial class VxMain : UIForm
    {
        private readonly VxMainViewModel _viewModel;
        private readonly Services.IContactBindingService _contactBindingService;
        private readonly Services.IWeChatLoaderService _loaderService;
        private readonly Services.ILogService _logService;
        private readonly Services.IWeixinSocketClient _socketClient; // Socket 客户端
        private BindingList<WxContact> _contactsBindingList;
        private BindingList<V2Member> _membersBindingList;
        private BindingList<V2MemberOrder> _ordersBindingList;
        
        // 设置窗口单实例
        private Views.SettingsForm? _settingsForm;

        public VxMain(
            VxMainViewModel viewModel,
            Services.IContactBindingService contactBindingService,
            Services.IWeChatLoaderService loaderService,
            Services.ILogService logService,
            Services.IWeixinSocketClient socketClient) // 注入 Socket 客户端
        {
            InitializeComponent();
            _viewModel = viewModel;
            _contactBindingService = contactBindingService;
            _loaderService = loaderService;
            _logService = logService;
            _socketClient = socketClient;
            
            // 订阅服务器推送事件
            _socketClient.OnServerPush += SocketClient_OnServerPush;
            
            // 启用自动重连
            _socketClient.AutoReconnect = true;
            
            // 记录主窗口打开
            _logService.Info("VxMain", "主窗口已打开");

            // 初始化数据绑定列表
            _contactsBindingList = new BindingList<WxContact>();
            _membersBindingList = new BindingList<V2Member>();
            _ordersBindingList = new BindingList<V2MemberOrder>();

            // 启用数据绑定自动通知
            _contactsBindingList.AllowEdit = true;
            _contactsBindingList.AllowNew = false;
            _contactsBindingList.AllowRemove = false;

            _membersBindingList.AllowEdit = true;
            _membersBindingList.AllowNew = false;
            _membersBindingList.AllowRemove = false;

            _ordersBindingList.AllowEdit = true;
            _ordersBindingList.AllowNew = false;
            _ordersBindingList.AllowRemove = false;

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
            // 添加测试联系人数据
            for (int i = 1; i <= 15; i++)
            {
                var contact = new WxContact
                {
                    Wxid = $"wxid_{i:D3}",
                    Account = i % 3 == 0 ? $"wx{i:D5}" : "",
                    Nickname = $"联系人{i}",
                    Remark = i % 5 == 0 ? $"备注{i}" : "",
                    Sex = i % 2,
                    Province = "广东",
                    City = "深圳",
                    Country = "中国",
                    IsGroup = i % 4 == 0
                };
                _contactsBindingList.Add(contact);
            }

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
                    Profit = i % 2 == 0 ? 59.1m : 0,
                    NetProfit = i % 2 == 0 ? 29.1m : -30,
                    Odds = 1.97m,
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
            
            // 🔵 方案1：程序启动时尝试连接（检测已运行的微信）
            _logService.Info("VxMain", "程序启动，尝试连接到 Socket 服务器...");
            lblStatus.Text = "尝试连接到微信...";
            
            bool connected = await _socketClient.ConnectAsync("127.0.0.1", 6328, 2000);
            
            if (connected)
            {
                _logService.Info("VxMain", "连接成功！微信已在运行");
                lblStatus.Text = "已连接到微信 ✓";
            }
            else
            {
                _logService.Info("VxMain", "连接失败，微信可能未启动或未注入 WeixinX.dll");
                lblStatus.Text = "未连接（等待微信启动）";
                
                // 🔵 方案3：启动自动重连（后台持续尝试）
                _logService.Info("VxMain", "启动自动重连（每5秒尝试一次）");
                _socketClient.StartAutoReconnect(5000);
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

        #region 修改即保存逻辑

        private void dgvMembers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var member = dgvMembers.Rows[e.RowIndex].DataBoundItem as V2Member;
            if (member != null)
            {
                // 立即保存到数据库（这里先打印日志）
                SaveMemberToDatabase(member);
                lblStatus.Text = $"会员 {member.Nickname} 已更新";
            }
        }

        private void dgvOrders_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var order = dgvOrders.Rows[e.RowIndex].DataBoundItem as V2MemberOrder;
            if (order != null)
            {
                // 立即保存到数据库（这里先打印日志）
                SaveOrderToDatabase(order);
                lblStatus.Text = $"订单 {order.IssueId} 已更新";
            }
        }

        private void SaveMemberToDatabase(V2Member member)
        {
            // TODO: 实现数据库保存逻辑
            // _memberRepository.Update(member);
            System.Diagnostics.Debug.WriteLine($"保存会员: {member.Nickname}, 余额: {member.Balance}");
        }

        private void SaveOrderToDatabase(V2MemberOrder order)
        {
            // TODO: 实现数据库保存逻辑
            // _orderRepository.Update(order);
            System.Diagnostics.Debug.WriteLine($"保存订单: {order.IssueId}, 状态: {order.OrderStatus}");
        }

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

        private void FilterOrdersByMember(string wxid)
        {
            // TODO: 实现订单筛选逻辑
            // 这里可以创建一个过滤后的BindingList
        }

        private void btnBindingContacts_Click(object sender, EventArgs e)
        {
            if (dgvContacts.CurrentRow?.DataBoundItem is WxContact contact)
            {
                _contactBindingService.BindContact(contact);
                if (this.Controls.Find("txtCurrentContact", true).FirstOrDefault() is Sunny.UI.UITextBox txt)
                {
                    txt.Text = contact.Wxid;
                }
                lblStatus.Text = $"已绑定联系人: {contact.Nickname} ({contact.Wxid})";
                _logService.Info("VxMain", $"绑定联系人: {contact.Nickname} ({contact.Wxid})");
                UIMessageBox.ShowSuccess($"成功绑定联系人: {contact.Nickname}");
            }
            else
            {
                _logService.Warning("VxMain", "绑定联系人失败: 未选择联系人");
                UIMessageBox.ShowWarning("请先选择一个联系人");
            }
        }

        private async void btnGetContactList_Click(object sender, EventArgs e)
        {
            try
            {
                _logService.Info("VxMain", "开始采集联系人列表");
                
                //var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                var currentDir = "D:\\gitcode\\wx4helper\\BaiShengVx3Plus\\bin\\Release\\net8.0-windows\\";
                var dllPath = Path.Combine(currentDir, "WeixinX.dll");

                if (!File.Exists(dllPath))
                {
                    _logService.Error("VxMain", $"找不到 WeixinX.dll: {dllPath}");
                    UIMessageBox.ShowError($"找不到 WeixinX.dll\n路径: {dllPath}");
                    return;
                }

                lblStatus.Text = "正在检查微信进程...";
                Application.DoEvents();

                // 获取现有微信进程
                var processes = _loaderService.GetWeChatProcesses();
                _logService.Info("VxMain", $"检测到 {processes.Count} 个微信进程");

                if (processes.Count > 0)
                {
                    lblStatus.Text = $"发现 {processes.Count} 个微信进程，正在注入...";
                    Application.DoEvents();

                    // 注入到第一个进程
                    if (_loaderService.InjectToProcess(processes[0], dllPath, out string error))
                    {
                        lblStatus.Text = "成功注入到微信进程，正在连接 Socket...";
                        _logService.Info("VxMain", $"成功注入到微信进程 (PID: {processes[0]})");
                        
                        // 等待 Socket 服务器启动（延迟 1 秒）
                        await Task.Delay(1000);
                        
                        // 连接到 Socket 服务器
                        await ConnectToSocketServerAsync();
                    }
                    else
                    {
                        lblStatus.Text = "注入失败";
                        _logService.Error("VxMain", $"注入失败 (PID: {processes[0]}): {error}");
                        UIMessageBox.ShowError($"注入失败:\n{error}");
                    }
                }
                else
                {
                    lblStatus.Text = "未发现微信进程，正在启动...";
                    Application.DoEvents();

                    // 启动新微信并注入
                    if (_loaderService.LaunchWeChat("127.0.0.1", "5672", dllPath, out string error))
                    {
                        lblStatus.Text = "成功启动微信并注入，正在连接 Socket...";
                        _logService.Info("VxMain", "成功启动微信并注入 WeixinX.dll");
                        
                        // 等待微信启动和 Socket 服务器启动（延迟 2 秒）
                        await Task.Delay(2000);
                        
                        // 连接到 Socket 服务器
                        await ConnectToSocketServerAsync();
                    }
                    else
                    {
                        lblStatus.Text = "启动失败";
                        _logService.Error("VxMain", $"启动微信失败: {error}");
                        UIMessageBox.ShowError($"启动失败:\n{error}");
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "发生错误";
                _logService.Error("VxMain", "采集联系人列表失败", ex);
                UIMessageBox.ShowError($"发生错误:\n{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void btnRefreshContacts_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "刷新联系人列表...";
            // TODO: 从微信获取联系人列表
            UIMessageBox.ShowInfo("刷新功能待实现");
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
        /// 处理服务器主动推送的消息
        /// </summary>
        private void SocketClient_OnServerPush(object? sender, Services.ServerPushEventArgs e)
        {
            try
            {
                _logService.Info("VxMain", $"收到服务器推送: {e.Method}");
                
                // 使用 Invoke 确保在 UI 线程上更新
                if (InvokeRequired)
                {
                    Invoke(new Action(() => HandleServerPush(e)));
                }
                else
                {
                    HandleServerPush(e);
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", "处理服务器推送失败", ex);
            }
        }

        /// <summary>
        /// 实际处理服务器推送（在 UI 线程）
        /// </summary>
        private void HandleServerPush(Services.ServerPushEventArgs e)
        {
            switch (e.Method)
            {
                case "MessageReceived":
                    _logService.Info("VxMain", $"收到新消息: {e.Data}");
                    lblStatus.Text = $"收到新消息";
                    // TODO: 更新 UI 显示新消息
                    break;

                case "ContactListUpdated":
                    _logService.Info("VxMain", "联系人列表已更新");
                    lblStatus.Text = "联系人列表已更新";
                    // TODO: 刷新联系人列表
                    break;

                default:
                    _logService.Info("VxMain", $"未知推送类型: {e.Method}");
                    break;
            }
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
