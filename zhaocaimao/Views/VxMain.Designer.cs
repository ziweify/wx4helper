using zhaocaimao.Models;

namespace zhaocaimao
{
    partial class VxMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle8 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle9 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle10 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle11 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle12 = new DataGridViewCellStyle();
            tabControlMain = new Sunny.UI.UITabControl();
            tabPageDataManagement = new TabPage();
            tabPageLottery = new TabPage();
            pnl_opendata = new Sunny.UI.UIPanel();
            tabPageSettings = new TabPage();
            pnl_fastsetting = new Sunny.UI.UIPanel();
            swi_OrdersTasking = new Sunny.UI.UISwitch();
            swiAutoOrdersBet = new Sunny.UI.UISwitch();
            lblSealSeconds = new Label();
            txtSealSeconds = new Sunny.UI.UIIntegerUpDown();
            lblMinBet = new Label();
            txtMinBet = new Sunny.UI.UIIntegerUpDown();
            lblMaxBet = new Label();
            txtMaxBet = new Sunny.UI.UIIntegerUpDown();
            lblAutoBetSeparator = new Label();
            lblPlatform = new Label();
            cbxPlatform = new Sunny.UI.UIComboBox();
            lblAutoBetUsername = new Label();
            txtAutoBetUsername = new Sunny.UI.UITextBox();
            lblAutoBetPassword = new Label();
            txtAutoBetPassword = new Sunny.UI.UITextBox();
            btnStartBrowser = new Sunny.UI.UIButton();
            btnConfigManager = new Sunny.UI.UIButton();
            splitContainerRight = new Sunny.UI.UISplitContainer();
            pnlMembers = new Sunny.UI.UIPanel();
            dgvMembers = new Sunny.UI.UIDataGridView();
            cmsMembers = new Sunny.UI.UIContextMenuStrip();
            tsmiClearBalance = new ToolStripMenuItem();
            tsmiDeleteMember = new ToolStripMenuItem();
            tsmiSetMemberType = new ToolStripMenuItem();
            tsmiSetAdmin = new ToolStripMenuItem();
            tsmiSetAgent = new ToolStripMenuItem();
            tsmiSetLeft = new ToolStripMenuItem();
            tsmiSetMemberSub = new ToolStripMenuItem();
            tsmiSetNormal = new ToolStripMenuItem();
            tsmiSetBlue = new ToolStripMenuItem();
            tsmiSetPurple = new ToolStripMenuItem();
            tsmiSetYellow = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            tsmiViewBalanceChange = new ToolStripMenuItem();
            pnlMembersTop = new Sunny.UI.UIPanel();
            lblMemberInfo = new Sunny.UI.UILabel();
            pnlOrders = new Sunny.UI.UIPanel();
            pnlOrdersTop = new Sunny.UI.UIPanel();
            lblOrderInfo = new Sunny.UI.UILabel();
            dgvOrders = new Sunny.UI.UIDataGridView();
            ucBinggoDataLast = new zhaocaimao.UserControls.UcBinggoDataLast();
            ucBinggoDataCur = new zhaocaimao.UserControls.UcBinggoDataCur();
            pnlRightSidebar = new Sunny.UI.UIPanel();
            dgvContacts = new Sunny.UI.UIDataGridView();
            pnlLeftTop = new Sunny.UI.UIPanel();
            btnRefreshContacts = new Sunny.UI.UIButton();
            btnBindingContacts = new Sunny.UI.UIButton();
            txtCurrentContact = new Sunny.UI.UITextBox();
            splitContainerMain = new Sunny.UI.UISplitContainer();
            pnlLeft = new Sunny.UI.UIPanel();
            pnlRight = new Sunny.UI.UIPanel();
            pnlTopButtons = new Sunny.UI.UIPanel();
            ucUserInfo1 = new zhaocaimao.Views.UcUserInfo();
            btnClearData = new Sunny.UI.UIButton();
            btnCreditWithdrawManage = new Sunny.UI.UIButton();
            btnOpenLotteryResult = new Sunny.UI.UIButton();
            btnConnect = new Sunny.UI.UIButton();
            btnLog = new Sunny.UI.UIButton();
            btnSettings = new Sunny.UI.UIButton();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            tabControlMain.SuspendLayout();
            tabPageDataManagement.SuspendLayout();  // 🔥 添加数据管理标签页的布局暂停
            tabPageLottery.SuspendLayout();
            tabPageSettings.SuspendLayout();
            pnl_fastsetting.SuspendLayout();
            (splitContainerRight).BeginInit();
            splitContainerRight.Panel1.SuspendLayout();
            splitContainerRight.Panel2.SuspendLayout();
            splitContainerRight.SuspendLayout();
            pnlMembers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMembers).BeginInit();
            cmsMembers.SuspendLayout();
            pnlMembersTop.SuspendLayout();
            pnlOrders.SuspendLayout();
            pnlOrdersTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOrders).BeginInit();
            pnlRightSidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvContacts).BeginInit();
            pnlLeftTop.SuspendLayout();
            (splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            pnlRight.SuspendLayout();
            pnlTopButtons.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabPageDataManagement);
            tabControlMain.Controls.Add(tabPageLottery);
            tabControlMain.Controls.Add(tabPageSettings);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControlMain.Font = new Font("微软雅黑", 13F, FontStyle.Bold);
            tabControlMain.ItemSize = new Size(140, 45);
            tabControlMain.Location = new Point(0, 105);
            tabControlMain.MainPage = "";
            tabControlMain.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(980, 678);
            tabControlMain.SizeMode = TabSizeMode.Fixed;
            tabControlMain.TabIndex = 0;
            tabControlMain.TabSelectedForeColor = Color.FromArgb(184, 134, 11);
            tabControlMain.TabUnSelectedForeColor = Color.FromArgb(160, 160, 160);
            tabControlMain.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // tabPageDataManagement
            // 
            tabPageDataManagement.BackColor = Color.FromArgb(255, 252, 240);
            tabPageDataManagement.Controls.Add(pnlRight);  // 🔥 添加包含 splitContainerRight 的 pnlRight
            tabPageDataManagement.Location = new Point(0, 45);
            tabPageDataManagement.Name = "tabPageDataManagement";
            tabPageDataManagement.Padding = new Padding(3);
            tabPageDataManagement.Size = new Size(980, 633);
            tabPageDataManagement.TabIndex = 0;
            tabPageDataManagement.Text = "📊 数据管理";
            tabPageDataManagement.UseVisualStyleBackColor = true;
            // 🔥 确保所有控件可见
            pnlRight.Visible = true;
            splitContainerRight.Visible = true;
            pnlMembers.Visible = true;
            pnlOrders.Visible = true;
            dgvMembers.Visible = true;
            dgvOrders.Visible = true;
            // 
            // tabPageLottery
            // 
            tabPageLottery.BackColor = Color.FromArgb(255, 252, 240);
            tabPageLottery.Controls.Add(pnl_opendata);
            tabPageLottery.Location = new Point(0, 45);
            tabPageLottery.Name = "tabPageLottery";
            tabPageLottery.Size = new Size(980, 633);
            tabPageLottery.TabIndex = 1;
            tabPageLottery.Text = "🎲 开奖数据";
            // 
            // pnl_opendata
            // 
            pnl_opendata.Dock = DockStyle.Fill;
            pnl_opendata.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            pnl_opendata.Location = new Point(0, 0);
            pnl_opendata.Margin = new Padding(10);
            pnl_opendata.MinimumSize = new Size(1, 1);
            pnl_opendata.Name = "pnl_opendata";
            pnl_opendata.Padding = new Padding(15);
            pnl_opendata.Size = new Size(980, 633);
            pnl_opendata.TabIndex = 2;
            pnl_opendata.Text = null;
            pnl_opendata.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // tabPageSettings
            // 
            tabPageSettings.BackColor = Color.FromArgb(255, 252, 240);
            tabPageSettings.Controls.Add(pnl_fastsetting);
            tabPageSettings.Location = new Point(0, 40);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Size = new Size(200, 60);
            tabPageSettings.TabIndex = 2;
            tabPageSettings.Text = "⚙️ 快速设置";
            // 
            // pnl_fastsetting
            // 
            pnl_fastsetting.Controls.Add(swi_OrdersTasking);
            pnl_fastsetting.Controls.Add(swiAutoOrdersBet);
            pnl_fastsetting.Controls.Add(lblSealSeconds);
            pnl_fastsetting.Controls.Add(txtSealSeconds);
            pnl_fastsetting.Controls.Add(lblMinBet);
            pnl_fastsetting.Controls.Add(txtMinBet);
            pnl_fastsetting.Controls.Add(lblMaxBet);
            pnl_fastsetting.Controls.Add(txtMaxBet);
            pnl_fastsetting.Controls.Add(lblAutoBetSeparator);
            pnl_fastsetting.Controls.Add(lblPlatform);
            pnl_fastsetting.Controls.Add(cbxPlatform);
            pnl_fastsetting.Controls.Add(lblAutoBetUsername);
            pnl_fastsetting.Controls.Add(txtAutoBetUsername);
            pnl_fastsetting.Controls.Add(lblAutoBetPassword);
            pnl_fastsetting.Controls.Add(txtAutoBetPassword);
            pnl_fastsetting.Controls.Add(btnStartBrowser);
            pnl_fastsetting.Controls.Add(btnConfigManager);
            pnl_fastsetting.Dock = DockStyle.Fill;
            pnl_fastsetting.FillColor = Color.White;
            pnl_fastsetting.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            pnl_fastsetting.Location = new Point(0, 0);
            pnl_fastsetting.Margin = new Padding(10);
            pnl_fastsetting.MinimumSize = new Size(1, 1);
            pnl_fastsetting.Name = "pnl_fastsetting";
            pnl_fastsetting.Padding = new Padding(15);
            pnl_fastsetting.Radius = 8;
            pnl_fastsetting.RectColor = Color.FromArgb(255, 193, 7);
            pnl_fastsetting.RectSize = 2;
            pnl_fastsetting.Size = new Size(200, 60);
            pnl_fastsetting.TabIndex = 3;
            pnl_fastsetting.Text = "💰 快速设置";
            pnl_fastsetting.TextAlignment = ContentAlignment.TopCenter;
            // 
            // swi_OrdersTasking
            // 
            swi_OrdersTasking.ActiveText = "收单中";
            swi_OrdersTasking.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            swi_OrdersTasking.InActiveText = "收单停";
            swi_OrdersTasking.Location = new Point(10, 243);
            swi_OrdersTasking.MinimumSize = new Size(1, 1);
            swi_OrdersTasking.Name = "swi_OrdersTasking";
            swi_OrdersTasking.Size = new Size(85, 29);
            swi_OrdersTasking.TabIndex = 6;
            swi_OrdersTasking.Text = "关单中";
            swi_OrdersTasking.ValueChanged += swi_OrdersTasking_ValueChanged;
            // 
            // swiAutoOrdersBet
            // 
            swiAutoOrdersBet.ActiveText = "飞单中";
            swiAutoOrdersBet.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            swiAutoOrdersBet.InActiveText = "飞单停";
            swiAutoOrdersBet.Location = new Point(10, 273);
            swiAutoOrdersBet.MinimumSize = new Size(1, 1);
            swiAutoOrdersBet.Name = "swiAutoOrdersBet";
            swiAutoOrdersBet.Size = new Size(85, 29);
            swiAutoOrdersBet.TabIndex = 6;
            swiAutoOrdersBet.Text = "关单中";
            swiAutoOrdersBet.ValueChanged += swiAutoOrdersBet_ValueChanged;
            // 
            // lblSealSeconds
            // 
            lblSealSeconds.Font = new Font("微软雅黑", 10F);
            lblSealSeconds.Location = new Point(5, 25);
            lblSealSeconds.Name = "lblSealSeconds";
            lblSealSeconds.Size = new Size(90, 23);
            lblSealSeconds.TabIndex = 0;
            lblSealSeconds.Text = "封盘提前(秒)";
            lblSealSeconds.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSealSeconds
            // 
            txtSealSeconds.Font = new Font("微软雅黑", 10F);
            txtSealSeconds.Location = new Point(100, 23);
            txtSealSeconds.Margin = new Padding(4, 5, 4, 5);
            txtSealSeconds.Maximum = 300;
            txtSealSeconds.Minimum = 10;
            txtSealSeconds.MinimumSize = new Size(100, 0);
            txtSealSeconds.Name = "txtSealSeconds";
            txtSealSeconds.ShowText = false;
            txtSealSeconds.Size = new Size(130, 29);
            txtSealSeconds.TabIndex = 1;
            txtSealSeconds.Text = "49";
            txtSealSeconds.TextAlignment = ContentAlignment.MiddleCenter;
            txtSealSeconds.ValueChanged += TxtSealSeconds_ValueChanged;
            // 
            // lblMinBet
            // 
            lblMinBet.Font = new Font("微软雅黑", 10F);
            lblMinBet.Location = new Point(5, 58);
            lblMinBet.Name = "lblMinBet";
            lblMinBet.Size = new Size(90, 23);
            lblMinBet.TabIndex = 2;
            lblMinBet.Text = "最小投注";
            lblMinBet.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtMinBet
            // 
            txtMinBet.Font = new Font("微软雅黑", 10F);
            txtMinBet.Location = new Point(100, 56);
            txtMinBet.Margin = new Padding(4, 5, 4, 5);
            txtMinBet.Maximum = 10000;
            txtMinBet.Minimum = 1;
            txtMinBet.MinimumSize = new Size(100, 0);
            txtMinBet.Name = "txtMinBet";
            txtMinBet.ShowText = false;
            txtMinBet.Size = new Size(130, 29);
            txtMinBet.TabIndex = 3;
            txtMinBet.Text = "1";
            txtMinBet.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // lblMaxBet
            // 
            lblMaxBet.Font = new Font("微软雅黑", 10F);
            lblMaxBet.Location = new Point(5, 91);
            lblMaxBet.Name = "lblMaxBet";
            lblMaxBet.Size = new Size(90, 23);
            lblMaxBet.TabIndex = 4;
            lblMaxBet.Text = "最大投注";
            lblMaxBet.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtMaxBet
            // 
            txtMaxBet.Font = new Font("微软雅黑", 10F);
            txtMaxBet.Location = new Point(100, 89);
            txtMaxBet.Margin = new Padding(4, 5, 4, 5);
            txtMaxBet.Maximum = 1000000;
            txtMaxBet.Minimum = 1;
            txtMaxBet.MinimumSize = new Size(100, 0);
            txtMaxBet.Name = "txtMaxBet";
            txtMaxBet.ShowText = false;
            txtMaxBet.Size = new Size(130, 29);
            txtMaxBet.TabIndex = 5;
            txtMaxBet.Text = "10000";
            txtMaxBet.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // lblAutoBetSeparator
            // 
            lblAutoBetSeparator.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            lblAutoBetSeparator.Location = new Point(5, 123);
            lblAutoBetSeparator.Name = "lblAutoBetSeparator";
            lblAutoBetSeparator.Size = new Size(225, 20);
            lblAutoBetSeparator.TabIndex = 6;
            lblAutoBetSeparator.Text = "━━━ 自动投注 ━━━";
            lblAutoBetSeparator.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblPlatform
            // 
            lblPlatform.Font = new Font("微软雅黑", 9F);
            lblPlatform.Location = new Point(5, 148);
            lblPlatform.Name = "lblPlatform";
            lblPlatform.Size = new Size(50, 20);
            lblPlatform.TabIndex = 7;
            lblPlatform.Text = "盘口:";
            lblPlatform.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cbxPlatform
            // 
            cbxPlatform.DataSource = null;
            cbxPlatform.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            cbxPlatform.FillColor = Color.White;
            cbxPlatform.Font = new Font("微软雅黑", 9F);
            cbxPlatform.ItemHoverColor = Color.FromArgb(155, 200, 255);
            cbxPlatform.Items.AddRange(new object[] { "云顶", "海峡", "红海", "通宝" });
            cbxPlatform.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            cbxPlatform.Location = new Point(60, 146);
            cbxPlatform.Margin = new Padding(4, 5, 4, 5);
            cbxPlatform.MinimumSize = new Size(63, 0);
            cbxPlatform.Name = "cbxPlatform";
            cbxPlatform.Padding = new Padding(0, 0, 30, 2);
            cbxPlatform.Size = new Size(170, 25);
            cbxPlatform.SymbolSize = 24;
            cbxPlatform.TabIndex = 8;
            cbxPlatform.TextAlignment = ContentAlignment.MiddleLeft;
            cbxPlatform.Watermark = "";
            // 
            // lblAutoBetUsername
            // 
            lblAutoBetUsername.Font = new Font("微软雅黑", 9F);
            lblAutoBetUsername.Location = new Point(5, 178);
            lblAutoBetUsername.Name = "lblAutoBetUsername";
            lblAutoBetUsername.Size = new Size(50, 20);
            lblAutoBetUsername.TabIndex = 9;
            lblAutoBetUsername.Text = "账号:";
            lblAutoBetUsername.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtAutoBetUsername
            // 
            txtAutoBetUsername.Font = new Font("微软雅黑", 9F);
            txtAutoBetUsername.Location = new Point(60, 176);
            txtAutoBetUsername.Margin = new Padding(4, 5, 4, 5);
            txtAutoBetUsername.MinimumSize = new Size(1, 16);
            txtAutoBetUsername.Name = "txtAutoBetUsername";
            txtAutoBetUsername.Padding = new Padding(5);
            txtAutoBetUsername.ShowText = false;
            txtAutoBetUsername.Size = new Size(170, 25);
            txtAutoBetUsername.TabIndex = 10;
            txtAutoBetUsername.TextAlignment = ContentAlignment.MiddleLeft;
            txtAutoBetUsername.Watermark = "投注账号";
            // 
            // lblAutoBetPassword
            // 
            lblAutoBetPassword.Font = new Font("微软雅黑", 9F);
            lblAutoBetPassword.Location = new Point(5, 208);
            lblAutoBetPassword.Name = "lblAutoBetPassword";
            lblAutoBetPassword.Size = new Size(50, 20);
            lblAutoBetPassword.TabIndex = 11;
            lblAutoBetPassword.Text = "密码:";
            lblAutoBetPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtAutoBetPassword
            // 
            txtAutoBetPassword.Font = new Font("微软雅黑", 9F);
            txtAutoBetPassword.Location = new Point(60, 206);
            txtAutoBetPassword.Margin = new Padding(4, 5, 4, 5);
            txtAutoBetPassword.MinimumSize = new Size(1, 16);
            txtAutoBetPassword.Name = "txtAutoBetPassword";
            txtAutoBetPassword.Padding = new Padding(5);
            txtAutoBetPassword.PasswordChar = '*';
            txtAutoBetPassword.ShowText = false;
            txtAutoBetPassword.Size = new Size(170, 25);
            txtAutoBetPassword.TabIndex = 12;
            txtAutoBetPassword.TextAlignment = ContentAlignment.MiddleLeft;
            txtAutoBetPassword.Watermark = "投注密码";
            // 
            // btnStartBrowser
            // 
            btnStartBrowser.FillColor = Color.FromArgb(255, 193, 7);
            btnStartBrowser.FillHoverColor = Color.FromArgb(255, 215, 0);
            btnStartBrowser.Font = new Font("微软雅黑", 9F);
            btnStartBrowser.Location = new Point(120, 240);
            btnStartBrowser.MinimumSize = new Size(1, 1);
            btnStartBrowser.Name = "btnStartBrowser";
            btnStartBrowser.RectColor = Color.FromArgb(255, 193, 7);
            btnStartBrowser.RectHoverColor = Color.FromArgb(255, 215, 0);
            btnStartBrowser.Size = new Size(110, 30);
            btnStartBrowser.TabIndex = 14;
            btnStartBrowser.Text = "启动浏览器";
            btnStartBrowser.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnStartBrowser.Click += btnStartBrowser_Click;
            // 
            // btnConfigManager
            // 
            btnConfigManager.FillColor = Color.FromArgb(255, 193, 7);
            btnConfigManager.FillHoverColor = Color.FromArgb(255, 215, 0);
            btnConfigManager.Font = new Font("微软雅黑", 9F);
            btnConfigManager.Location = new Point(120, 275);
            btnConfigManager.MinimumSize = new Size(1, 1);
            btnConfigManager.Name = "btnConfigManager";
            btnConfigManager.RectColor = Color.FromArgb(255, 193, 7);
            btnConfigManager.RectHoverColor = Color.FromArgb(255, 215, 0);
            btnConfigManager.Size = new Size(110, 30);
            btnConfigManager.TabIndex = 15;
            btnConfigManager.Text = "配置管理";
            btnConfigManager.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnConfigManager.Click += btnConfigManager_Click;
            // 
            // splitContainerRight
            // 
            splitContainerRight.Cursor = Cursors.HSplit;
            splitContainerRight.Dock = DockStyle.Fill;
            splitContainerRight.Location = new Point(0, 0);
            splitContainerRight.MinimumSize = new Size(20, 20);
            splitContainerRight.Name = "splitContainerRight";
            splitContainerRight.Orientation = Orientation.Horizontal;
            // 
            // splitContainerRight.Panel1
            // 
            splitContainerRight.Panel1.Controls.Add(pnlMembers);
            // 
            // splitContainerRight.Panel2
            // 
            splitContainerRight.Panel2.Controls.Add(pnlOrders);
            splitContainerRight.Size = new Size(745, 642);
            splitContainerRight.SplitterDistance = 321;
            splitContainerRight.SplitterWidth = 5;
            splitContainerRight.TabIndex = 0;
            splitContainerRight.Visible = true;  // 🔥 确保可见
            // 
            // pnlMembers
            // 
            // 🔥 重要：先添加顶部标签，再添加表格（这样表格会在标签下方）
            pnlMembers.Controls.Add(pnlMembersTop);
            pnlMembers.Controls.Add(dgvMembers);
            pnlMembers.Dock = DockStyle.Fill;
            pnlMembers.FillColor = Color.FromArgb(255, 248, 220);
            pnlMembers.Font = new Font("微软雅黑", 12F);
            pnlMembers.Location = new Point(0, 0);
            pnlMembers.Margin = new Padding(4, 5, 4, 5);
            pnlMembers.MinimumSize = new Size(1, 1);
            pnlMembers.Name = "pnlMembers";
            pnlMembers.RectColor = Color.FromArgb(255, 193, 7);
            pnlMembers.Size = new Size(745, 321);
            pnlMembers.TabIndex = 0;
            pnlMembers.Text = null;
            pnlMembers.TextAlignment = ContentAlignment.MiddleCenter;
            pnlMembers.Visible = true;  // 🔥 确保可见
            // 
            // dgvMembers
            // 
            dgvMembers.AllowUserToAddRows = false;
            dgvMembers.AllowUserToDeleteRows = false;
            dgvMembers.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            dgvMembers.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvMembers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMembers.BackgroundColor = Color.White;
            dgvMembers.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvMembers.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvMembers.ColumnHeadersHeight = 32;
            dgvMembers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvMembers.ContextMenuStrip = cmsMembers;
            dgvMembers.Dock = DockStyle.Fill;
            dgvMembers.EnableHeadersVisualStyles = false;
            dgvMembers.Font = new Font("微软雅黑", 10F);
            dgvMembers.GridColor = Color.FromArgb(80, 160, 255);
            dgvMembers.MultiSelect = false;
            dgvMembers.Name = "dgvMembers";
            dgvMembers.Visible = true;  // 🔥 确保可见
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle3.Font = new Font("微软雅黑", 10F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.White;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dgvMembers.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgvMembers.RowHeadersVisible = false;
            dgvMembers.RowHeadersWidth = 51;
            dataGridViewCellStyle4.BackColor = Color.White;
            dataGridViewCellStyle4.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvMembers.RowsDefaultCellStyle = dataGridViewCellStyle4;
            dgvMembers.RowTemplate.Height = 29;
            dgvMembers.SelectedIndex = -1;
            dgvMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMembers.Size = new Size(745, 291);
            dgvMembers.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvMembers.TabIndex = 1;
            dgvMembers.SelectionChanged += dgvMembers_SelectionChanged;
            // 
            // cmsMembers
            // 
            cmsMembers.BackColor = Color.FromArgb(243, 249, 255);
            cmsMembers.Font = new Font("微软雅黑", 10F);
            cmsMembers.ImageScalingSize = new Size(20, 20);
            cmsMembers.Items.AddRange(new ToolStripItem[] { tsmiClearBalance, tsmiDeleteMember, tsmiSetMemberType, toolStripSeparator1, tsmiViewBalanceChange });
            cmsMembers.Name = "cmsMembers";
            cmsMembers.Size = new Size(163, 106);
            // 
            // tsmiClearBalance
            // 
            tsmiClearBalance.Name = "tsmiClearBalance";
            tsmiClearBalance.Size = new Size(162, 24);
            tsmiClearBalance.Text = "清分";
            tsmiClearBalance.Click += TsmiClearBalance_Click;
            // 
            // tsmiDeleteMember
            // 
            tsmiDeleteMember.Name = "tsmiDeleteMember";
            tsmiDeleteMember.Size = new Size(162, 24);
            tsmiDeleteMember.Text = "删除会员";
            tsmiDeleteMember.Click += TsmiDeleteMember_Click;
            // 
            // tsmiSetMemberType
            // 
            tsmiSetMemberType.DropDownItems.AddRange(new ToolStripItem[] { tsmiSetAdmin, tsmiSetAgent, tsmiSetLeft, tsmiSetMemberSub });
            tsmiSetMemberType.Name = "tsmiSetMemberType";
            tsmiSetMemberType.Size = new Size(162, 24);
            tsmiSetMemberType.Text = "设置会员类型";
            // 
            // tsmiSetAdmin
            // 
            tsmiSetAdmin.Name = "tsmiSetAdmin";
            tsmiSetAdmin.Size = new Size(120, 24);
            tsmiSetAdmin.Text = "管理";
            tsmiSetAdmin.Click += TsmiSetMemberType_Click;
            // 
            // tsmiSetAgent
            // 
            tsmiSetAgent.Name = "tsmiSetAgent";
            tsmiSetAgent.Size = new Size(120, 24);
            tsmiSetAgent.Text = "托";
            tsmiSetAgent.Click += TsmiSetMemberType_Click;
            // 
            // tsmiSetLeft
            // 
            tsmiSetLeft.Name = "tsmiSetLeft";
            tsmiSetLeft.Size = new Size(120, 24);
            tsmiSetLeft.Text = "已退群";
            tsmiSetLeft.Click += TsmiSetMemberType_Click;
            // 
            // tsmiSetMemberSub
            // 
            tsmiSetMemberSub.DropDownItems.AddRange(new ToolStripItem[] { tsmiSetNormal, tsmiSetBlue, tsmiSetPurple, tsmiSetYellow });
            tsmiSetMemberSub.Name = "tsmiSetMemberSub";
            tsmiSetMemberSub.Size = new Size(120, 24);
            tsmiSetMemberSub.Text = "会员";
            // 
            // tsmiSetNormal
            // 
            tsmiSetNormal.Name = "tsmiSetNormal";
            tsmiSetNormal.Size = new Size(106, 24);
            tsmiSetNormal.Text = "普会";
            tsmiSetNormal.Click += TsmiSetMemberType_Click;
            // 
            // tsmiSetBlue
            // 
            tsmiSetBlue.Name = "tsmiSetBlue";
            tsmiSetBlue.Size = new Size(106, 24);
            tsmiSetBlue.Text = "蓝会";
            tsmiSetBlue.Click += TsmiSetMemberType_Click;
            // 
            // tsmiSetPurple
            // 
            tsmiSetPurple.Name = "tsmiSetPurple";
            tsmiSetPurple.Size = new Size(106, 24);
            tsmiSetPurple.Text = "紫会";
            tsmiSetPurple.Click += TsmiSetMemberType_Click;
            // 
            // tsmiSetYellow
            // 
            tsmiSetYellow.Name = "tsmiSetYellow";
            tsmiSetYellow.Size = new Size(106, 24);
            tsmiSetYellow.Text = "黄会";
            tsmiSetYellow.Click += TsmiSetMemberType_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(159, 6);
            // 
            // tsmiViewBalanceChange
            // 
            tsmiViewBalanceChange.Name = "tsmiViewBalanceChange";
            tsmiViewBalanceChange.Size = new Size(162, 24);
            tsmiViewBalanceChange.Text = "资金变动";
            tsmiViewBalanceChange.Click += TsmiViewBalanceChange_Click;
            // 
            // pnlMembersTop
            // 
            pnlMembersTop.Controls.Add(lblMemberInfo);
            pnlMembersTop.Dock = DockStyle.Top;
            pnlMembersTop.Font = new Font("微软雅黑", 12F);
            pnlMembersTop.Location = new Point(0, 0);
            pnlMembersTop.Margin = new Padding(4, 5, 4, 5);
            pnlMembersTop.MinimumSize = new Size(1, 1);
            pnlMembersTop.Name = "pnlMembersTop";
            pnlMembersTop.Size = new Size(745, 30);
            pnlMembersTop.TabIndex = 0;
            pnlMembersTop.Text = null;
            pnlMembersTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // lblMemberInfo
            // 
            lblMemberInfo.Dock = DockStyle.Fill;
            lblMemberInfo.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            lblMemberInfo.ForeColor = Color.FromArgb(48, 48, 48);
            lblMemberInfo.Location = new Point(0, 0);
            lblMemberInfo.Name = "lblMemberInfo";
            lblMemberInfo.Size = new Size(745, 30);
            lblMemberInfo.TabIndex = 0;
            lblMemberInfo.Text = "会员列表 (共0人)";
            lblMemberInfo.TextAlign = ContentAlignment.MiddleLeft;
            lblMemberInfo.Paint += lblMemberInfo_Paint;
            // 
            // pnlOrders
            // 
            // 🔥 重要：先添加顶部标签，再添加表格（这样表格会在标签下方）
            pnlOrders.Controls.Add(pnlOrdersTop);
            pnlOrders.Controls.Add(dgvOrders);
            pnlOrders.Dock = DockStyle.Fill;
            pnlOrders.FillColor = Color.FromArgb(255, 248, 220);
            pnlOrders.Font = new Font("微软雅黑", 12F);
            pnlOrders.Location = new Point(0, 0);
            pnlOrders.Margin = new Padding(4, 5, 4, 5);
            pnlOrders.MinimumSize = new Size(1, 1);
            pnlOrders.Name = "pnlOrders";
            pnlOrders.RectColor = Color.FromArgb(255, 193, 7);
            pnlOrders.Size = new Size(745, 316);
            pnlOrders.TabIndex = 0;
            pnlOrders.Text = null;
            pnlOrders.TextAlignment = ContentAlignment.MiddleCenter;
            pnlOrders.Visible = true;  // 🔥 确保可见
            // 
            // pnlOrdersTop
            // 
            pnlOrdersTop.Controls.Add(lblOrderInfo);
            pnlOrdersTop.Dock = DockStyle.Top;
            pnlOrdersTop.Font = new Font("微软雅黑", 12F);
            pnlOrdersTop.Location = new Point(0, 0);
            pnlOrdersTop.Margin = new Padding(4, 5, 4, 5);
            pnlOrdersTop.MinimumSize = new Size(1, 1);
            pnlOrdersTop.Name = "pnlOrdersTop";
            pnlOrdersTop.Size = new Size(745, 30);
            pnlOrdersTop.TabIndex = 0;
            pnlOrdersTop.Text = null;
            pnlOrdersTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // lblOrderInfo
            // 
            lblOrderInfo.Dock = DockStyle.Fill;
            lblOrderInfo.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            lblOrderInfo.ForeColor = Color.FromArgb(48, 48, 48);
            lblOrderInfo.Location = new Point(0, 0);
            lblOrderInfo.Name = "lblOrderInfo";
            lblOrderInfo.Size = new Size(745, 30);
            lblOrderInfo.TabIndex = 0;
            lblOrderInfo.Text = "订单列表 (共0单)";
            lblOrderInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgvOrders
            // 
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.AllowUserToDeleteRows = false;
            dgvOrders.AllowUserToResizeRows = false;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(235, 243, 255);
            dgvOrders.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrders.BackgroundColor = Color.White;
            dgvOrders.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle6.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle6.ForeColor = Color.White;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
            dgvOrders.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            dgvOrders.ColumnHeadersHeight = 32;
            dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvOrders.Dock = DockStyle.Fill;
            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.Font = new Font("微软雅黑", 10F);
            dgvOrders.GridColor = Color.FromArgb(80, 160, 255);
            dgvOrders.MultiSelect = true;
            dgvOrders.Name = "dgvOrders";
            dgvOrders.Visible = true;  // 🔥 确保可见
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle7.Font = new Font("微软雅黑", 10F);
            dataGridViewCellStyle7.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle7.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle7.SelectionForeColor = Color.White;
            dataGridViewCellStyle7.WrapMode = DataGridViewTriState.True;
            dgvOrders.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.RowHeadersWidth = 51;
            dataGridViewCellStyle8.BackColor = Color.White;
            dataGridViewCellStyle8.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvOrders.RowsDefaultCellStyle = dataGridViewCellStyle8;
            dgvOrders.RowTemplate.Height = 29;
            dgvOrders.SelectedIndex = -1;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.Size = new Size(745, 316);
            dgvOrders.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvOrders.TabIndex = 1;
            dgvOrders.MouseDown += DgvOrders_MouseDown;
            // 
            // ucBinggoDataLast
            // 
            ucBinggoDataLast.BackColor = Color.FromArgb(255, 248, 220);
            ucBinggoDataLast.Dock = DockStyle.Fill;
            ucBinggoDataLast.Location = new Point(10, 135);
            ucBinggoDataLast.Name = "ucBinggoDataLast";
            ucBinggoDataLast.Size = new Size(200, 533);
            ucBinggoDataLast.TabIndex = 1;
            // 
            // ucBinggoDataCur
            // 
            ucBinggoDataCur.BackColor = Color.FromArgb(255, 248, 220);
            ucBinggoDataCur.BorderStyle = BorderStyle.FixedSingle;
            ucBinggoDataCur.Dock = DockStyle.Top;
            ucBinggoDataCur.Location = new Point(10, 50);
            ucBinggoDataCur.Name = "ucBinggoDataCur";
            ucBinggoDataCur.Size = new Size(200, 85);
            ucBinggoDataCur.TabIndex = 0;
            // 
            // pnlRightSidebar
            // 
            pnlRightSidebar.Controls.Add(ucBinggoDataLast);
            pnlRightSidebar.Controls.Add(ucBinggoDataCur);
            pnlRightSidebar.Controls.Add(dgvContacts);
            pnlRightSidebar.Controls.Add(pnlLeftTop);
            pnlRightSidebar.Dock = DockStyle.Right;
            pnlRightSidebar.FillColor = Color.FromArgb(255, 248, 220);
            pnlRightSidebar.Font = new Font("微软雅黑", 12F);
            pnlRightSidebar.Location = new Point(980, 105);
            pnlRightSidebar.Margin = new Padding(0);
            pnlRightSidebar.MinimumSize = new Size(1, 1);
            pnlRightSidebar.Name = "pnlRightSidebar";
            pnlRightSidebar.Padding = new Padding(10);
            pnlRightSidebar.RectColor = Color.FromArgb(255, 193, 7);
            pnlRightSidebar.Size = new Size(220, 678);
            pnlRightSidebar.TabIndex = 2;
            pnlRightSidebar.Text = "💰 实时信息";
            pnlRightSidebar.TextAlignment = ContentAlignment.TopCenter;
            // 
            // dgvContacts
            // 
            dgvContacts.AllowUserToAddRows = false;
            dgvContacts.AllowUserToDeleteRows = false;
            dgvContacts.AllowUserToResizeRows = false;
            dataGridViewCellStyle9.BackColor = Color.FromArgb(235, 243, 255);
            dgvContacts.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle9;
            dgvContacts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvContacts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvContacts.BackgroundColor = Color.White;
            dgvContacts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle10.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle10.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle10.ForeColor = Color.White;
            dataGridViewCellStyle10.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = DataGridViewTriState.True;
            dgvContacts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
            dgvContacts.ColumnHeadersHeight = 32;
            dgvContacts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvContacts.EnableHeadersVisualStyles = false;
            dgvContacts.Font = new Font("微软雅黑", 12F);
            dgvContacts.GridColor = Color.FromArgb(80, 160, 255);
            dgvContacts.Location = new Point(20, 60);
            dgvContacts.MultiSelect = false;
            dgvContacts.Name = "dgvContacts";
            dgvContacts.ReadOnly = true;
            dataGridViewCellStyle11.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle11.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle11.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle11.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle11.SelectionForeColor = Color.White;
            dataGridViewCellStyle11.WrapMode = DataGridViewTriState.True;
            dgvContacts.RowHeadersDefaultCellStyle = dataGridViewCellStyle11;
            dgvContacts.RowHeadersVisible = false;
            dgvContacts.RowHeadersWidth = 51;
            dataGridViewCellStyle12.BackColor = Color.White;
            dataGridViewCellStyle12.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvContacts.RowsDefaultCellStyle = dataGridViewCellStyle12;
            dgvContacts.RowTemplate.Height = 29;
            dgvContacts.SelectedIndex = -1;
            dgvContacts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvContacts.Size = new Size(130, 628);
            dgvContacts.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvContacts.TabIndex = 1;
            dgvContacts.SelectionChanged += dgvContacts_SelectionChanged;
            // 
            // pnlLeftTop
            // 
            pnlLeftTop.Controls.Add(btnRefreshContacts);
            pnlLeftTop.Controls.Add(btnBindingContacts);
            pnlLeftTop.Controls.Add(txtCurrentContact);
            pnlLeftTop.Dock = DockStyle.Top;
            pnlLeftTop.Font = new Font("微软雅黑", 12F);
            pnlLeftTop.Location = new Point(10, 10);
            pnlLeftTop.Margin = new Padding(4, 5, 4, 5);
            pnlLeftTop.MinimumSize = new Size(1, 1);
            pnlLeftTop.Name = "pnlLeftTop";
            pnlLeftTop.Size = new Size(200, 40);
            pnlLeftTop.TabIndex = 0;
            pnlLeftTop.Text = null;
            pnlLeftTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnRefreshContacts
            // 
            btnRefreshContacts.Cursor = Cursors.Hand;
            btnRefreshContacts.Dock = DockStyle.Right;
            btnRefreshContacts.FillColor = Color.FromArgb(255, 248, 220);
            btnRefreshContacts.FillHoverColor = Color.FromArgb(255, 215, 0);
            btnRefreshContacts.FillPressColor = Color.FromArgb(184, 134, 11);
            btnRefreshContacts.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            btnRefreshContacts.ForeColor = Color.FromArgb(184, 134, 11);
            btnRefreshContacts.Location = new Point(100, 0);
            btnRefreshContacts.MinimumSize = new Size(1, 1);
            btnRefreshContacts.Name = "btnRefreshContacts";
            btnRefreshContacts.Radius = 4;
            btnRefreshContacts.RectColor = Color.FromArgb(255, 193, 7);
            btnRefreshContacts.Size = new Size(50, 40);
            btnRefreshContacts.TabIndex = 2;
            btnRefreshContacts.Text = "🔄 刷新";
            btnRefreshContacts.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnRefreshContacts.Click += btnRefreshContacts_Click;
            // 
            // btnBindingContacts
            // 
            btnBindingContacts.Cursor = Cursors.Hand;
            btnBindingContacts.Dock = DockStyle.Right;
            btnBindingContacts.FillColor = Color.FromArgb(255, 193, 7);
            btnBindingContacts.FillHoverColor = Color.FromArgb(255, 215, 0);
            btnBindingContacts.FillPressColor = Color.FromArgb(184, 134, 11);
            btnBindingContacts.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            btnBindingContacts.Location = new Point(150, 0);
            btnBindingContacts.MinimumSize = new Size(1, 1);
            btnBindingContacts.Name = "btnBindingContacts";
            btnBindingContacts.Radius = 4;
            btnBindingContacts.RectColor = Color.FromArgb(184, 134, 11);
            btnBindingContacts.Size = new Size(50, 40);
            btnBindingContacts.TabIndex = 1;
            btnBindingContacts.Text = "🔗 绑定";
            btnBindingContacts.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnBindingContacts.Click += btnBindingContacts_Click;
            // 
            // txtCurrentContact
            // 
            txtCurrentContact.Cursor = Cursors.IBeam;
            txtCurrentContact.Dock = DockStyle.Fill;
            txtCurrentContact.FillColor = Color.FromArgb(243, 249, 255);
            txtCurrentContact.Font = new Font("微软雅黑", 10F);
            txtCurrentContact.Location = new Point(0, 0);
            txtCurrentContact.Margin = new Padding(4, 5, 4, 5);
            txtCurrentContact.MinimumSize = new Size(1, 16);
            txtCurrentContact.Name = "txtCurrentContact";
            txtCurrentContact.Padding = new Padding(5);
            txtCurrentContact.ReadOnly = true;
            txtCurrentContact.ShowText = false;
            txtCurrentContact.Size = new Size(200, 40);
            txtCurrentContact.TabIndex = 0;
            txtCurrentContact.Text = "未绑定联系人";
            txtCurrentContact.TextAlignment = ContentAlignment.MiddleLeft;
            txtCurrentContact.Watermark = "点击绑定按钮选择联系人";
            txtCurrentContact.KeyDown += TxtCurrentContact_KeyDown;
            // 
            // splitContainerMain
            // 
            splitContainerMain.Cursor = Cursors.VSplit;
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(3, 3);
            splitContainerMain.MinimumSize = new Size(20, 20);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(pnlLeft);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(pnlRight);
            splitContainerMain.Size = new Size(994, 642);
            splitContainerMain.SplitterDistance = 244;
            splitContainerMain.SplitterWidth = 5;
            splitContainerMain.TabIndex = 0;
            // 
            // pnlLeft
            // 
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Font = new Font("微软雅黑", 12F);
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Margin = new Padding(4, 5, 4, 5);
            pnlLeft.MinimumSize = new Size(1, 1);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(244, 642);
            pnlLeft.TabIndex = 0;
            pnlLeft.Text = null;
            pnlLeft.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(splitContainerRight);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Font = new Font("微软雅黑", 12F);
            pnlRight.Location = new Point(0, 0);
            pnlRight.Margin = new Padding(4, 5, 4, 5);
            pnlRight.MinimumSize = new Size(1, 1);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(745, 642);
            pnlRight.TabIndex = 0;
            pnlRight.Text = null;
            pnlRight.TextAlignment = ContentAlignment.MiddleCenter;
            pnlRight.Visible = true;  // 🔥 确保可见
            // 
            // pnlTopButtons
            // 
            pnlTopButtons.Controls.Add(ucUserInfo1);
            pnlTopButtons.Controls.Add(btnClearData);
            pnlTopButtons.Controls.Add(btnCreditWithdrawManage);
            pnlTopButtons.Controls.Add(btnOpenLotteryResult);
            pnlTopButtons.Controls.Add(btnConnect);
            pnlTopButtons.Controls.Add(btnLog);
            pnlTopButtons.Controls.Add(btnSettings);
            pnlTopButtons.Dock = DockStyle.Top;
            pnlTopButtons.FillColor = Color.FromArgb(255, 215, 0);
            pnlTopButtons.Font = new Font("微软雅黑", 12F);
            pnlTopButtons.Location = new Point(0, 35);
            pnlTopButtons.Margin = new Padding(0);
            pnlTopButtons.MinimumSize = new Size(1, 1);
            pnlTopButtons.Name = "pnlTopButtons";
            pnlTopButtons.Padding = new Padding(15, 10, 15, 10);
            pnlTopButtons.RectColor = Color.FromArgb(184, 134, 11);
            pnlTopButtons.Size = new Size(1200, 70);
            pnlTopButtons.TabIndex = 1;
            pnlTopButtons.Text = null;
            pnlTopButtons.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // ucUserInfo1
            // 
            ucUserInfo1.BackColor = Color.White;
            ucUserInfo1.Location = new Point(1, 0);
            ucUserInfo1.Name = "ucUserInfo1";
            ucUserInfo1.Size = new Size(246, 60);
            ucUserInfo1.TabIndex = 5;
            // 
            // btnClearData
            // 
            btnClearData.Cursor = Cursors.Hand;
            btnClearData.FillColor = Color.FromArgb(255, 248, 220);
            btnClearData.FillHoverColor = Color.FromArgb(255, 215, 0);
            btnClearData.FillPressColor = Color.FromArgb(184, 134, 11);
            btnClearData.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            btnClearData.ForeColor = Color.FromArgb(184, 134, 11);
            btnClearData.Location = new Point(989, 15);
            btnClearData.MinimumSize = new Size(1, 1);
            btnClearData.Name = "btnClearData";
            btnClearData.Radius = 6;
            btnClearData.RectColor = Color.FromArgb(255, 193, 7);
            btnClearData.Size = new Size(100, 40);
            btnClearData.TabIndex = 4;
            btnClearData.Text = "🗑️ 清空数据";
            btnClearData.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClearData.Click += btnClearData_Click;
            // 
            // btnCreditWithdrawManage
            // 
            btnCreditWithdrawManage.Cursor = Cursors.Hand;
            btnCreditWithdrawManage.FillColor = Color.FromArgb(255, 248, 220);
            btnCreditWithdrawManage.FillHoverColor = Color.FromArgb(255, 215, 0);
            btnCreditWithdrawManage.FillPressColor = Color.FromArgb(184, 134, 11);
            btnCreditWithdrawManage.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            btnCreditWithdrawManage.ForeColor = Color.FromArgb(184, 134, 11);
            btnCreditWithdrawManage.Location = new Point(656, 15);
            btnCreditWithdrawManage.MinimumSize = new Size(1, 1);
            btnCreditWithdrawManage.Name = "btnCreditWithdrawManage";
            btnCreditWithdrawManage.Radius = 6;
            btnCreditWithdrawManage.RectColor = Color.FromArgb(255, 193, 7);
            btnCreditWithdrawManage.Size = new Size(120, 40);
            btnCreditWithdrawManage.TabIndex = 4;
            btnCreditWithdrawManage.Text = "💰 上下分管理";
            btnCreditWithdrawManage.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnCreditWithdrawManage.Click += btnCreditWithdrawManage_Click;
            // 
            // btnOpenLotteryResult
            // 
            btnOpenLotteryResult.Cursor = Cursors.Hand;
            btnOpenLotteryResult.FillColor = Color.FromArgb(255, 248, 220);
            btnOpenLotteryResult.FillHoverColor = Color.FromArgb(255, 215, 0);
            btnOpenLotteryResult.FillPressColor = Color.FromArgb(184, 134, 11);
            btnOpenLotteryResult.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            btnOpenLotteryResult.ForeColor = Color.FromArgb(184, 134, 11);
            btnOpenLotteryResult.Location = new Point(782, 15);
            btnOpenLotteryResult.MinimumSize = new Size(1, 1);
            btnOpenLotteryResult.Name = "btnOpenLotteryResult";
            btnOpenLotteryResult.Radius = 6;
            btnOpenLotteryResult.RectColor = Color.FromArgb(255, 193, 7);
            btnOpenLotteryResult.Size = new Size(100, 40);
            btnOpenLotteryResult.TabIndex = 3;
            btnOpenLotteryResult.Text = "🎲 开奖结果";
            btnOpenLotteryResult.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnOpenLotteryResult.Click += btnOpenLotteryResult_Click;
            // 
            // btnConnect
            // 
            btnConnect.Cursor = Cursors.Hand;
            btnConnect.FillColor = Color.FromArgb(255, 193, 7);
            btnConnect.FillHoverColor = Color.FromArgb(255, 215, 0);
            btnConnect.FillPressColor = Color.FromArgb(184, 134, 11);
            btnConnect.FillSelectedColor = Color.FromArgb(184, 134, 11);
            btnConnect.Font = new Font("微软雅黑", 11F, FontStyle.Bold);
            btnConnect.Location = new Point(253, 15);
            btnConnect.MinimumSize = new Size(1, 1);
            btnConnect.Name = "btnConnect";
            btnConnect.Radius = 8;
            btnConnect.RectColor = Color.FromArgb(184, 134, 11);
            btnConnect.RectHoverColor = Color.FromArgb(255, 215, 0);
            btnConnect.RectPressColor = Color.FromArgb(184, 134, 11);
            btnConnect.RectSelectedColor = Color.FromArgb(184, 134, 11);
            btnConnect.Size = new Size(110, 40);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "🔗 连接";
            btnConnect.TipsFont = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnConnect.Click += btnConnect_Click;
            // 
            // btnLog
            // 
            btnLog.Cursor = Cursors.Hand;
            btnLog.FillColor = Color.FromArgb(255, 248, 220);
            btnLog.FillHoverColor = Color.FromArgb(255, 215, 0);
            btnLog.FillPressColor = Color.FromArgb(184, 134, 11);
            btnLog.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            btnLog.ForeColor = Color.FromArgb(184, 134, 11);
            btnLog.Location = new Point(888, 15);
            btnLog.MinimumSize = new Size(1, 1);
            btnLog.Name = "btnLog";
            btnLog.Radius = 6;
            btnLog.RectColor = Color.FromArgb(255, 193, 7);
            btnLog.Size = new Size(95, 40);
            btnLog.TabIndex = 1;
            btnLog.Text = "📋 日志";
            btnLog.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnLog.Click += btnLog_Click;
            // 
            // btnSettings
            // 
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.Dock = DockStyle.Right;
            btnSettings.FillColor = Color.FromArgb(255, 248, 220);
            btnSettings.FillHoverColor = Color.FromArgb(255, 215, 0);
            btnSettings.FillPressColor = Color.FromArgb(184, 134, 11);
            btnSettings.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            btnSettings.ForeColor = Color.FromArgb(184, 134, 11);
            btnSettings.Location = new Point(1085, 10);
            btnSettings.MinimumSize = new Size(1, 1);
            btnSettings.Name = "btnSettings";
            btnSettings.Radius = 6;
            btnSettings.RectColor = Color.FromArgb(255, 193, 7);
            btnSettings.Size = new Size(100, 50);
            btnSettings.TabIndex = 0;
            btnSettings.Text = "⚙️ 设置";
            btnSettings.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSettings.Click += btnSettings_Click;
            // 
            // statusStrip
            // 
            statusStrip.BackColor = Color.FromArgb(255, 248, 220);
            statusStrip.Font = new Font("微软雅黑", 10F);
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 783);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1200, 25);
            statusStrip.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(37, 20);
            lblStatus.Text = "就绪";
            // 
            // VxMain
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1200, 808);
            Controls.Add(tabControlMain);
            Controls.Add(pnlRightSidebar);
            Controls.Add(pnlTopButtons);
            Controls.Add(statusStrip);
            Name = "VxMain";
            Text = "💰 招财猫 - 管理系统";
            ZoomScaleRect = new Rectangle(15, 15, 980, 762);
            Load += VxMain_Load;
            tabControlMain.ResumeLayout(false);
            tabPageDataManagement.ResumeLayout(false);  // 🔥 添加数据管理标签页的布局恢复
            tabPageLottery.ResumeLayout(false);
            tabPageSettings.ResumeLayout(false);
            pnl_fastsetting.ResumeLayout(false);
            splitContainerRight.Panel1.ResumeLayout(false);
            splitContainerRight.Panel2.ResumeLayout(false);
            (splitContainerRight).EndInit();
            splitContainerRight.ResumeLayout(false);
            pnlMembers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMembers).EndInit();
            cmsMembers.ResumeLayout(false);
            pnlMembersTop.ResumeLayout(false);
            pnlOrders.ResumeLayout(false);
            pnlOrdersTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvOrders).EndInit();
            pnlRightSidebar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvContacts).EndInit();
            pnlLeftTop.ResumeLayout(false);
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            (splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            pnlTopButtons.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        // 🔥 新增：标签页控件
        private Sunny.UI.UITabControl tabControlMain;
        private TabPage tabPageDataManagement;  // 🔥 合并的会员和订单管理页面
        private TabPage tabPageLottery;
        private TabPage tabPageSettings;
        private Sunny.UI.UIPanel pnlRightSidebar;
        
        private Sunny.UI.UISplitContainer splitContainerMain;
        private Sunny.UI.UIPanel pnlLeft;
        private Sunny.UI.UIDataGridView dgvContacts;
        private Sunny.UI.UIPanel pnlLeftTop;
        private Sunny.UI.UIButton btnRefreshContacts;
        private Sunny.UI.UIButton btnBindingContacts;
        private Sunny.UI.UITextBox txtCurrentContact;
        private Sunny.UI.UIPanel pnlRight;
        private Sunny.UI.UISplitContainer splitContainerRight;
        private Sunny.UI.UIPanel pnlMembers;
        private Sunny.UI.UIDataGridView dgvMembers;
        private Sunny.UI.UIPanel pnlMembersTop;
        private Sunny.UI.UILabel lblMemberInfo;
        private Sunny.UI.UIPanel pnlOrders;
        private Sunny.UI.UIDataGridView dgvOrders;
        private Sunny.UI.UIPanel pnlOrdersTop;
        private Sunny.UI.UILabel lblOrderInfo;
        private Sunny.UI.UIPanel pnlTopButtons;
        private Sunny.UI.UIButton btnSettings;
        private Sunny.UI.UIButton btnConnect;
        private Sunny.UI.UIButton btnLog;
        private Sunny.UI.UIButton btnOpenLotteryResult;
        private Sunny.UI.UIButton btnCreditWithdrawManage;  // 🔥 上下分管理按钮
        private Sunny.UI.UIButton btnClearData;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private Views.UcUserInfo ucUserInfo1;
        private Sunny.UI.UIContextMenuStrip cmsMembers;
        private ToolStripMenuItem tsmiClearBalance;       // 清分
        private ToolStripMenuItem tsmiDeleteMember;        // 删除
        private ToolStripMenuItem tsmiSetMemberType;       // 设置会员类型
        private ToolStripSeparator toolStripSeparator1;    // 分隔线
        private ToolStripMenuItem tsmiViewBalanceChange;   // 资金变动
        // 设置会员类型子菜单 - 顶级分类
        private ToolStripMenuItem tsmiSetAdmin;            // 管理
        private ToolStripMenuItem tsmiSetAgent;            // 托
        private ToolStripMenuItem tsmiSetLeft;             // 已退群
        private ToolStripMenuItem tsmiSetMemberSub;        // 会员（子菜单）
        // 会员子分类
        private ToolStripMenuItem tsmiSetNormal;           // 普会
        private ToolStripMenuItem tsmiSetBlue;             // 蓝会
        private ToolStripMenuItem tsmiSetPurple;           // 紫会
        private ToolStripMenuItem tsmiSetYellow;           // 黄会
        private Sunny.UI.UIPanel pnl_opendata;
        private UserControls.UcBinggoDataCur ucBinggoDataCur;
        private UserControls.UcBinggoDataLast ucBinggoDataLast;
        private Sunny.UI.UIPanel pnl_fastsetting;
        private System.Windows.Forms.Label lblSealSeconds;
        private Sunny.UI.UIIntegerUpDown txtSealSeconds;
        private System.Windows.Forms.Label lblMinBet;
        private Sunny.UI.UIIntegerUpDown txtMinBet;
        private System.Windows.Forms.Label lblMaxBet;
        private Sunny.UI.UIIntegerUpDown txtMaxBet;
        
        // 🤖 自动投注控件
        private System.Windows.Forms.Label lblAutoBetSeparator;
        private System.Windows.Forms.Label lblPlatform;
        private Sunny.UI.UIComboBox cbxPlatform;
        private System.Windows.Forms.Label lblAutoBetUsername;
        private Sunny.UI.UITextBox txtAutoBetUsername;
        private System.Windows.Forms.Label lblAutoBetPassword;
        private Sunny.UI.UITextBox txtAutoBetPassword;
        private Sunny.UI.UIButton btnStartBrowser;
        private Sunny.UI.UIButton btnConfigManager;
        private Sunny.UI.UISwitch swi_OrdersTasking;
        private Sunny.UI.UISwitch swiAutoOrdersBet;
    }
}
