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
            
            // 🔥 美化联系人列表样式
            CustomizeContactsGridStyle();

            // 绑定会员列表
            dgvMembers.DataSource = _membersBindingList;
            dgvMembers.AutoGenerateColumns = true;
            dgvMembers.EditMode = DataGridViewEditMode.EditOnEnter;

            // 设置会员表字段可见性和顺序
            dgvMembers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            // 🔥 美化会员列表样式
            CustomizeMembersGridStyle();
            
            // 绑定订单列表
            dgvOrders.DataSource = _ordersBindingList;
            dgvOrders.AutoGenerateColumns = true;
            dgvOrders.EditMode = DataGridViewEditMode.EditOnEnter;

            // 设置订单表字段可见性和顺序
            dgvOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            // 🔥 美化订单列表样式
            CustomizeOrdersGridStyle();

            // 添加测试数据
            LoadTestData();
        }

        private void LoadTestData()
        {
            // ✅ 所有测试数据已清空
            // 联系人数据：从服务器获取
            // 会员数据：从数据库加载（自动追踪）
            // 订单数据：从数据库加载（自动追踪）

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
        /// 会员列表：自定义效果（Hover + 选中）
        /// </summary>
        private void dgvMembers_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null) return;
            
            bool isSelected = dgvMembers.Rows[e.RowIndex].Selected;
            bool isHover = (e.RowIndex == _hoverRowIndex_Members);
            
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

        private async void btnBindingContacts_Click(object sender, EventArgs e)
        {
            if (dgvContacts.CurrentRow?.DataBoundItem is WxContact contact)
            {
                // 🔥 业务流程1：判断是否为群（wxid 包含 '@' 符号）
                if (!contact.Wxid.Contains("@"))
                {
                    _logService.Warning("VxMain", $"绑定失败: 选中的不是群组 - {contact.Nickname} ({contact.Wxid})");
                    UIMessageBox.ShowWarning("请选择正确的群组！\n\n只有群组（包含 @ 符号的ID）才能进行绑定。");
                    return;
                }
                
                // 保存当前绑定的联系人对象
                _currentBoundContact = contact;
                
                // 调用服务保存绑定
                _contactBindingService.BindContact(contact);
                
                // 🔥 更新文本框显示绑定的联系人
                txtCurrentContact.Text = $"{contact.Nickname} ({contact.Wxid})";
                txtCurrentContact.FillColor = Color.FromArgb(240, 255, 240); // 浅绿色背景
                txtCurrentContact.RectColor = Color.FromArgb(82, 196, 26);   // 绿色边框
                
                // 🔥 刷新 DataGridView，更新行颜色
                dgvContacts.Refresh();
                
                lblStatus.Text = $"✓ 已绑定: {contact.Nickname} ({contact.Wxid}) - 正在获取群成员...";
                _logService.Info("VxMain", $"绑定群组: {contact.Nickname} ({contact.Wxid})");
                
                // 🔥 业务流程2：调用 GetGroupContacts 获取群成员
                try
                {
                    _logService.Info("VxMain", $"开始获取群成员列表: {contact.Wxid}");
                    
                    var result = await _socketClient.SendAsync<JsonDocument>("GetGroupContacts", contact.Wxid);
                    
                    if (result == null || result.RootElement.ValueKind != JsonValueKind.Array)
                    {
                        _logService.Error("VxMain", "获取群成员失败: 返回数据为空或格式错误");
                        UIMessageBox.ShowError("获取群成员失败！");
                        return;
                    }
                    
                    // 🔥 业务流程3：解析数据并填充到 dgvMembers
                    await LoadGroupMembersToDataGridAsync(result.RootElement, contact.Wxid);
                    
                    lblStatus.Text = $"✓ 已绑定: {contact.Nickname} ({contact.Wxid}) - 群成员加载完成";
                    _logService.Info("VxMain", $"群成员加载完成: {contact.Wxid}");
                }
                catch (Exception ex)
                {
                    _logService.Error("VxMain", $"获取群成员异常: {ex.Message}");
                    UIMessageBox.ShowError($"获取群成员失败！\n\n{ex.Message}");
                }
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
        /// 加载群成员数据到 dgvMembers
        /// </summary>
        /// <param name="groupMembersJson">GetGroupContacts 返回的 JSON 数据</param>
        /// <param name="groupWxid">群微信 ID</param>
        private Task LoadGroupMembersToDataGridAsync(JsonElement groupMembersJson, string groupWxid)
        {
            try
            {
                _logService.Info("VxMain", $"开始解析群成员数据，群ID: {groupWxid}");

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

                        // 添加到 BindingList
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
    }
}
