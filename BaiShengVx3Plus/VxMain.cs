using Sunny.UI;
using BaiShengVx3Plus.ViewModels;
using BaiShengVx3Plus.Models;
using System.ComponentModel;

namespace BaiShengVx3Plus
{
    public partial class VxMain : UIForm
    {
        private readonly VxMainViewModel _viewModel;
        private readonly Services.IContactBindingService _contactBindingService;
        private readonly Services.IWeChatLoaderService _loaderService;
        private BindingList<WxContact> _contactsBindingList;
        private BindingList<V2Member> _membersBindingList;
        private BindingList<V2MemberOrder> _ordersBindingList;

        public VxMain(
            VxMainViewModel viewModel,
            Services.IContactBindingService contactBindingService,
            Services.IWeChatLoaderService loaderService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _contactBindingService = contactBindingService;
            _loaderService = loaderService;

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

        private void VxMain_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "系统就绪";
            
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
                UIMessageBox.ShowSuccess($"成功绑定联系人: {contact.Nickname}");
            }
            else
            {
                UIMessageBox.ShowWarning("请先选择一个联系人");
            }
        }

        private void btnGetContactList_Click(object sender, EventArgs e)
        {
            try
            {
                var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                var dllPath = Path.Combine(currentDir, "WeixinX.dll");

                if (!File.Exists(dllPath))
                {
                    UIMessageBox.ShowError($"找不到 WeixinX.dll\n路径: {dllPath}");
                    return;
                }

                lblStatus.Text = "正在检查微信进程...";
                Application.DoEvents();

                // 获取现有微信进程
                var processes = _loaderService.GetWeChatProcesses();

                if (processes.Count > 0)
                {
                    lblStatus.Text = $"发现 {processes.Count} 个微信进程，正在注入...";
                    Application.DoEvents();

                    // 注入到第一个进程
                    if (_loaderService.InjectToProcess(processes[0], dllPath, out string error))
                    {
                        lblStatus.Text = "成功注入到微信进程";
                        UIMessageBox.ShowSuccess($"成功注入到微信进程 (PID: {processes[0]})");
                    }
                    else
                    {
                        lblStatus.Text = "注入失败";
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
                        lblStatus.Text = "成功启动微信并注入";
                        UIMessageBox.ShowSuccess("成功启动微信并注入 WeixinX.dll");
                    }
                    else
                    {
                        lblStatus.Text = "启动失败";
                        UIMessageBox.ShowError($"启动失败:\n{error}");
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "发生错误";
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
            lblStatus.Text = "打开日志窗口...";
            // TODO: 实现日志窗口
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
            lblStatus.Text = "打开设置窗口...";
            // TODO: 实现设置窗口
        }

        #endregion
    }
}
