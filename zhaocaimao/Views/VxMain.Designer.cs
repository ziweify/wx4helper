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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VxMain));
            ucBinggoDataCur = new zhaocaimao.UserControls.UcBinggoDataCur();
            ucBinggoDataLast = new zhaocaimao.UserControls.UcBinggoDataLast();
            lblSealSeconds = new Label();
            txtSealSeconds = new Sunny.UI.UITextBox();
            lblMinBet = new Label();
            txtMinBet = new Sunny.UI.UITextBox();
            lblMaxBet = new Label();
            txtMaxBet = new Sunny.UI.UITextBox();
            lblPlatform = new Label();
            cbxPlatform = new Sunny.UI.UIComboBox();
            lblAutoBetUsername = new Label();
            txtAutoBetUsername = new Sunny.UI.UITextBox();
            lblAutoBetPassword = new Label();
            txtAutoBetPassword = new Sunny.UI.UITextBox();
            lblOdds = new Label();
            txtOdds = new Sunny.UI.UIDoubleUpDown();
            btnStartBrowser = new Sunny.UI.UIButton();
            btnConfigManager = new Sunny.UI.UIButton();
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
            splitContainerMain = new Sunny.UI.UISplitContainer();
            pnlLeft = new Sunny.UI.UIPanel();
            pnl_fastsetting = new Sunny.UI.UIPanel();
            swi_OrdersTasking = new Sunny.UI.UISwitch();
            swiAutoOrdersBet = new Sunny.UI.UISwitch();
            pnl_opendata = new Sunny.UI.UIPanel();
            dgvContacts = new Sunny.UI.UIDataGridView();
            pnlLeftTop = new Sunny.UI.UIPanel();
            btnRefreshContacts = new Sunny.UI.UIButton();
            btnBindingContacts = new Sunny.UI.UIButton();
            txtCurrentContact = new Sunny.UI.UITextBox();
            pnlRight = new Sunny.UI.UIPanel();
            splitContainerData = new Sunny.UI.UISplitContainer();
            pnlMembers = new Sunny.UI.UIPanel();
            dgvMembers = new Sunny.UI.UIDataGridView();
            pnlMembersTop = new Sunny.UI.UIPanel();
            lblMemberInfo = new Sunny.UI.UILabel();
            pnlOrders = new Sunny.UI.UIPanel();
            dgvOrders = new Sunny.UI.UIDataGridView();
            pnlOrdersTop = new Sunny.UI.UIPanel();
            lblOrderInfo = new Sunny.UI.UILabel();
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
            cmsMembers.SuspendLayout();
            (splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            pnlLeft.SuspendLayout();
            pnl_fastsetting.SuspendLayout();
            pnl_opendata.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvContacts).BeginInit();
            pnlLeftTop.SuspendLayout();
            pnlRight.SuspendLayout();
            (splitContainerData).BeginInit();
            splitContainerData.Panel1.SuspendLayout();
            splitContainerData.Panel2.SuspendLayout();
            splitContainerData.SuspendLayout();
            pnlMembers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMembers).BeginInit();
            pnlMembersTop.SuspendLayout();
            pnlOrders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOrders).BeginInit();
            pnlOrdersTop.SuspendLayout();
            pnlTopButtons.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // ucBinggoDataCur
            // 
            ucBinggoDataCur.BackColor = Color.FromArgb(245, 247, 250);
            ucBinggoDataCur.BorderStyle = BorderStyle.FixedSingle;
            ucBinggoDataCur.Location = new Point(0, 5);
            ucBinggoDataCur.Name = "ucBinggoDataCur";
            ucBinggoDataCur.Size = new Size(239, 85);
            ucBinggoDataCur.TabIndex = 0;
            // 
            // ucBinggoDataLast
            // 
            ucBinggoDataLast.BackColor = Color.FromArgb(248, 250, 252);
            ucBinggoDataLast.Location = new Point(3, 89);
            ucBinggoDataLast.Name = "ucBinggoDataLast";
            ucBinggoDataLast.Size = new Size(239, 107);
            ucBinggoDataLast.TabIndex = 1;
            // 
            // lblSealSeconds
            // 
            lblSealSeconds.Font = new Font("微软雅黑", 10F);
            lblSealSeconds.ForeColor = Color.FromArgb(90, 122, 138);
            lblSealSeconds.Location = new Point(5, 25);
            lblSealSeconds.Name = "lblSealSeconds";
            lblSealSeconds.Size = new Size(90, 23);
            lblSealSeconds.TabIndex = 0;
            lblSealSeconds.Text = "封盘提前(秒)";
            lblSealSeconds.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSealSeconds
            // 
            txtSealSeconds.DoubleValue = 49D;
            txtSealSeconds.FillColor = Color.FromArgb(245, 247, 250);
            txtSealSeconds.Font = new Font("微软雅黑", 10F);
            txtSealSeconds.IntValue = 49;
            txtSealSeconds.Location = new Point(100, 23);
            txtSealSeconds.Margin = new Padding(4, 5, 4, 5);
            txtSealSeconds.MinimumSize = new Size(1, 16);
            txtSealSeconds.Name = "txtSealSeconds";
            txtSealSeconds.Padding = new Padding(5);
            txtSealSeconds.RectColor = Color.FromArgb(107, 143, 166);
            txtSealSeconds.ShowText = false;
            txtSealSeconds.Size = new Size(130, 29);
            txtSealSeconds.TabIndex = 1;
            txtSealSeconds.Text = "49";
            txtSealSeconds.TextAlignment = ContentAlignment.MiddleLeft;
            txtSealSeconds.Watermark = "封盘提前秒数";
            txtSealSeconds.TextChanged += TxtSealSeconds_TextChanged;
            // 
            // lblMinBet
            // 
            lblMinBet.Font = new Font("微软雅黑", 10F);
            lblMinBet.ForeColor = Color.FromArgb(90, 122, 138);
            lblMinBet.Location = new Point(5, 58);
            lblMinBet.Name = "lblMinBet";
            lblMinBet.Size = new Size(90, 23);
            lblMinBet.TabIndex = 2;
            lblMinBet.Text = "最小投注";
            lblMinBet.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtMinBet
            // 
            txtMinBet.DoubleValue = 1D;
            txtMinBet.FillColor = Color.FromArgb(245, 247, 250);
            txtMinBet.Font = new Font("微软雅黑", 10F);
            txtMinBet.IntValue = 1;
            txtMinBet.Location = new Point(100, 56);
            txtMinBet.Margin = new Padding(4, 5, 4, 5);
            txtMinBet.MinimumSize = new Size(1, 16);
            txtMinBet.Name = "txtMinBet";
            txtMinBet.Padding = new Padding(5);
            txtMinBet.RectColor = Color.FromArgb(107, 143, 166);
            txtMinBet.ShowText = false;
            txtMinBet.Size = new Size(130, 29);
            txtMinBet.TabIndex = 3;
            txtMinBet.Text = "1";
            txtMinBet.TextAlignment = ContentAlignment.MiddleLeft;
            txtMinBet.Watermark = "最小投注金额";
            txtMinBet.TextChanged += TxtMinBet_TextChanged;
            // 
            // lblMaxBet
            // 
            lblMaxBet.Font = new Font("微软雅黑", 10F);
            lblMaxBet.ForeColor = Color.FromArgb(90, 122, 138);
            lblMaxBet.Location = new Point(5, 91);
            lblMaxBet.Name = "lblMaxBet";
            lblMaxBet.Size = new Size(90, 23);
            lblMaxBet.TabIndex = 4;
            lblMaxBet.Text = "最大投注";
            lblMaxBet.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtMaxBet
            // 
            txtMaxBet.DoubleValue = 10000D;
            txtMaxBet.FillColor = Color.FromArgb(245, 247, 250);
            txtMaxBet.Font = new Font("微软雅黑", 10F);
            txtMaxBet.IntValue = 10000;
            txtMaxBet.Location = new Point(100, 89);
            txtMaxBet.Margin = new Padding(4, 5, 4, 5);
            txtMaxBet.MinimumSize = new Size(1, 16);
            txtMaxBet.Name = "txtMaxBet";
            txtMaxBet.Padding = new Padding(5);
            txtMaxBet.RectColor = Color.FromArgb(107, 143, 166);
            txtMaxBet.ShowText = false;
            txtMaxBet.Size = new Size(130, 29);
            txtMaxBet.TabIndex = 5;
            txtMaxBet.Text = "10000";
            txtMaxBet.TextAlignment = ContentAlignment.MiddleLeft;
            txtMaxBet.Watermark = "最大投注金额";
            txtMaxBet.TextChanged += TxtMaxBet_TextChanged;
            // 
            // lblPlatform
            // 
            lblPlatform.Font = new Font("微软雅黑", 9F);
            lblPlatform.ForeColor = Color.FromArgb(90, 122, 138);
            lblPlatform.Location = new Point(5, 124);
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
            cbxPlatform.FillColor = Color.FromArgb(245, 247, 250);
            cbxPlatform.Font = new Font("微软雅黑", 9F);
            cbxPlatform.ItemHoverColor = Color.FromArgb(143, 168, 194);
            // 🔥 不在设计器中添加平台，改为在代码中动态加载（确保和 BetPlatform 枚举一致）
            // cbxPlatform.Items.AddRange(new object[] { "云顶", "海峡", "红海", "通宝" });
            cbxPlatform.ItemSelectForeColor = Color.FromArgb(245, 247, 250);
            cbxPlatform.Location = new Point(60, 122);
            cbxPlatform.Margin = new Padding(4, 5, 4, 5);
            cbxPlatform.MinimumSize = new Size(63, 0);
            cbxPlatform.Name = "cbxPlatform";
            cbxPlatform.Padding = new Padding(0, 0, 30, 2);
            cbxPlatform.RectColor = Color.FromArgb(107, 143, 166);
            cbxPlatform.Size = new Size(170, 25);
            cbxPlatform.SymbolSize = 24;
            cbxPlatform.TabIndex = 8;
            cbxPlatform.TextAlignment = ContentAlignment.MiddleLeft;
            cbxPlatform.Watermark = "";
            // 
            // lblAutoBetUsername
            // 
            lblAutoBetUsername.Font = new Font("微软雅黑", 9F);
            lblAutoBetUsername.ForeColor = Color.FromArgb(90, 122, 138);
            lblAutoBetUsername.Location = new Point(5, 154);
            lblAutoBetUsername.Name = "lblAutoBetUsername";
            lblAutoBetUsername.Size = new Size(50, 20);
            lblAutoBetUsername.TabIndex = 9;
            lblAutoBetUsername.Text = "账号:";
            lblAutoBetUsername.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtAutoBetUsername
            // 
            txtAutoBetUsername.FillColor = Color.FromArgb(245, 247, 250);
            txtAutoBetUsername.Font = new Font("微软雅黑", 9F);
            txtAutoBetUsername.Location = new Point(60, 152);
            txtAutoBetUsername.Margin = new Padding(4, 5, 4, 5);
            txtAutoBetUsername.MinimumSize = new Size(1, 16);
            txtAutoBetUsername.Name = "txtAutoBetUsername";
            txtAutoBetUsername.Padding = new Padding(5);
            txtAutoBetUsername.RectColor = Color.FromArgb(107, 143, 166);
            txtAutoBetUsername.ShowText = false;
            txtAutoBetUsername.Size = new Size(170, 25);
            txtAutoBetUsername.TabIndex = 10;
            txtAutoBetUsername.TextAlignment = ContentAlignment.MiddleLeft;
            txtAutoBetUsername.Watermark = "投注账号";
            // 
            // lblAutoBetPassword
            // 
            lblAutoBetPassword.Font = new Font("微软雅黑", 9F);
            lblAutoBetPassword.ForeColor = Color.FromArgb(90, 122, 138);
            lblAutoBetPassword.Location = new Point(5, 184);
            lblAutoBetPassword.Name = "lblAutoBetPassword";
            lblAutoBetPassword.Size = new Size(50, 20);
            lblAutoBetPassword.TabIndex = 11;
            lblAutoBetPassword.Text = "密码:";
            lblAutoBetPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtAutoBetPassword
            // 
            txtAutoBetPassword.FillColor = Color.FromArgb(245, 247, 250);
            txtAutoBetPassword.Font = new Font("微软雅黑", 9F);
            txtAutoBetPassword.Location = new Point(60, 182);
            txtAutoBetPassword.Margin = new Padding(4, 5, 4, 5);
            txtAutoBetPassword.MinimumSize = new Size(1, 16);
            txtAutoBetPassword.Name = "txtAutoBetPassword";
            txtAutoBetPassword.Padding = new Padding(5);
            txtAutoBetPassword.PasswordChar = '*';
            txtAutoBetPassword.RectColor = Color.FromArgb(107, 143, 166);
            txtAutoBetPassword.ShowText = false;
            txtAutoBetPassword.Size = new Size(170, 25);
            txtAutoBetPassword.TabIndex = 12;
            txtAutoBetPassword.TextAlignment = ContentAlignment.MiddleLeft;
            txtAutoBetPassword.Watermark = "投注密码";
            // 
            // lblOdds
            // 
            lblOdds.Font = new Font("微软雅黑", 9F);
            lblOdds.Location = new Point(5, 214);
            lblOdds.Name = "lblOdds";
            lblOdds.Size = new Size(50, 20);
            lblOdds.TabIndex = 13;
            lblOdds.Text = "赔率:";
            lblOdds.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtOdds
            // 
            txtOdds.DecimalPlaces = 2;
            txtOdds.Font = new Font("微软雅黑", 9F);
            txtOdds.Location = new Point(60, 212);
            txtOdds.Margin = new Padding(4, 5, 4, 5);
            txtOdds.Maximum = 2.5D;
            txtOdds.Minimum = 1D;
            txtOdds.MinimumSize = new Size(1, 16);
            txtOdds.Name = "txtOdds";
            txtOdds.Padding = new Padding(5);
            txtOdds.ShowText = false;
            txtOdds.Size = new Size(170, 25);
            txtOdds.TabIndex = 14;
            txtOdds.Text = null;
            txtOdds.TextAlignment = ContentAlignment.MiddleLeft;
            txtOdds.Value = 1.97D;
            // 
            // btnStartBrowser
            // 
            btnStartBrowser.FillColor = Color.FromArgb(107, 143, 166);
            btnStartBrowser.FillHoverColor = Color.FromArgb(143, 168, 194);
            btnStartBrowser.FillPressColor = Color.FromArgb(91, 127, 166);
            btnStartBrowser.Font = new Font("微软雅黑", 9F);
            btnStartBrowser.Location = new Point(120, 243);
            btnStartBrowser.MinimumSize = new Size(1, 1);
            btnStartBrowser.Name = "btnStartBrowser";
            btnStartBrowser.RectColor = Color.FromArgb(107, 143, 166);
            btnStartBrowser.RectHoverColor = Color.FromArgb(143, 168, 194);
            btnStartBrowser.RectPressColor = Color.FromArgb(91, 127, 166);
            btnStartBrowser.Size = new Size(110, 30);
            btnStartBrowser.TabIndex = 14;
            btnStartBrowser.Text = "启动浏览器";
            btnStartBrowser.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnStartBrowser.Click += btnStartBrowser_Click;
            // 
            // btnConfigManager
            // 
            btnConfigManager.FillColor = Color.FromArgb(107, 143, 166);
            btnConfigManager.FillHoverColor = Color.FromArgb(143, 168, 194);
            btnConfigManager.FillPressColor = Color.FromArgb(91, 127, 166);
            btnConfigManager.Font = new Font("微软雅黑", 9F);
            btnConfigManager.Location = new Point(120, 279);
            btnConfigManager.MinimumSize = new Size(1, 1);
            btnConfigManager.Name = "btnConfigManager";
            btnConfigManager.RectColor = Color.FromArgb(107, 143, 166);
            btnConfigManager.RectHoverColor = Color.FromArgb(143, 168, 194);
            btnConfigManager.RectPressColor = Color.FromArgb(91, 127, 166);
            btnConfigManager.Size = new Size(110, 30);
            btnConfigManager.TabIndex = 15;
            btnConfigManager.Text = "配置管理";
            btnConfigManager.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnConfigManager.Click += btnConfigManager_Click;
            // 
            // cmsMembers
            // 
            cmsMembers.BackColor = Color.FromArgb(245, 247, 250);
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
            // splitContainerMain
            // 
            splitContainerMain.Cursor = Cursors.VSplit;
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(0, 95);
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
            splitContainerMain.Size = new Size(1200, 688);
            splitContainerMain.SplitterDistance = 244;
            splitContainerMain.SplitterWidth = 5;
            splitContainerMain.TabIndex = 0;
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(pnl_fastsetting);
            pnlLeft.Controls.Add(pnl_opendata);
            pnlLeft.Controls.Add(dgvContacts);
            pnlLeft.Controls.Add(pnlLeftTop);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Font = new Font("微软雅黑", 12F);
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Margin = new Padding(4, 5, 4, 5);
            pnlLeft.MinimumSize = new Size(1, 1);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(244, 688);
            pnlLeft.TabIndex = 0;
            pnlLeft.Text = null;
            pnlLeft.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // pnl_fastsetting
            // 
            pnl_fastsetting.AutoScroll = true;
            pnl_fastsetting.Controls.Add(swi_OrdersTasking);
            pnl_fastsetting.Controls.Add(swiAutoOrdersBet);
            pnl_fastsetting.Controls.Add(lblSealSeconds);
            pnl_fastsetting.Controls.Add(txtSealSeconds);
            pnl_fastsetting.Controls.Add(lblMinBet);
            pnl_fastsetting.Controls.Add(txtMinBet);
            pnl_fastsetting.Controls.Add(lblMaxBet);
            pnl_fastsetting.Controls.Add(txtMaxBet);
            pnl_fastsetting.Controls.Add(lblPlatform);
            pnl_fastsetting.Controls.Add(cbxPlatform);
            pnl_fastsetting.Controls.Add(lblAutoBetUsername);
            pnl_fastsetting.Controls.Add(txtAutoBetUsername);
            pnl_fastsetting.Controls.Add(lblAutoBetPassword);
            pnl_fastsetting.Controls.Add(txtAutoBetPassword);
            pnl_fastsetting.Controls.Add(lblOdds);
            pnl_fastsetting.Controls.Add(txtOdds);
            pnl_fastsetting.Controls.Add(btnStartBrowser);
            pnl_fastsetting.Controls.Add(btnConfigManager);
            pnl_fastsetting.FillColor = Color.FromArgb(245, 247, 250);
            pnl_fastsetting.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            pnl_fastsetting.Location = new Point(5, 378);
            pnl_fastsetting.Margin = new Padding(4, 5, 4, 5);
            pnl_fastsetting.MinimumSize = new Size(1, 1);
            pnl_fastsetting.Name = "pnl_fastsetting";
            pnl_fastsetting.RectColor = Color.FromArgb(107, 143, 166);
            pnl_fastsetting.Size = new Size(237, 345);
            pnl_fastsetting.TabIndex = 3;
            pnl_fastsetting.Text = "快速设置";
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
            // pnl_opendata
            // 
            pnl_opendata.Controls.Add(ucBinggoDataLast);
            pnl_opendata.Controls.Add(ucBinggoDataCur);
            pnl_opendata.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            pnl_opendata.Location = new Point(3, 174);
            pnl_opendata.Margin = new Padding(4, 5, 4, 5);
            pnl_opendata.MinimumSize = new Size(1, 1);
            pnl_opendata.Name = "pnl_opendata";
            pnl_opendata.Size = new Size(239, 199);
            pnl_opendata.TabIndex = 2;
            pnl_opendata.Text = null;
            pnl_opendata.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvContacts
            // 
            dgvContacts.AllowUserToAddRows = false;
            dgvContacts.AllowUserToDeleteRows = false;
            dgvContacts.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(248, 250, 252);
            dgvContacts.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvContacts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvContacts.BackgroundColor = Color.White;
            dgvContacts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(107, 143, 166);
            dataGridViewCellStyle2.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvContacts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvContacts.ColumnHeadersHeight = 32;
            dgvContacts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvContacts.EnableHeadersVisualStyles = false;
            dgvContacts.Font = new Font("微软雅黑", 12F);
            dgvContacts.GridColor = Color.FromArgb(107, 143, 166);
            dgvContacts.Location = new Point(3, 43);
            dgvContacts.MultiSelect = false;
            dgvContacts.Name = "dgvContacts";
            dgvContacts.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(248, 250, 252);
            dataGridViewCellStyle3.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(107, 143, 166);
            dataGridViewCellStyle3.SelectionForeColor = Color.White;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dgvContacts.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgvContacts.RowHeadersVisible = false;
            dgvContacts.RowHeadersWidth = 51;
            dataGridViewCellStyle4.BackColor = Color.White;
            dataGridViewCellStyle4.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvContacts.RowsDefaultCellStyle = dataGridViewCellStyle4;
            dgvContacts.RowTemplate.Height = 29;
            dgvContacts.SelectedIndex = -1;
            dgvContacts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvContacts.Size = new Size(244, 147);
            dgvContacts.StripeOddColor = Color.FromArgb(248, 250, 252);
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
            pnlLeftTop.Location = new Point(0, 0);
            pnlLeftTop.Margin = new Padding(4, 5, 4, 5);
            pnlLeftTop.MinimumSize = new Size(1, 1);
            pnlLeftTop.Name = "pnlLeftTop";
            pnlLeftTop.Size = new Size(244, 40);
            pnlLeftTop.TabIndex = 0;
            pnlLeftTop.Text = null;
            pnlLeftTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnRefreshContacts
            // 
            btnRefreshContacts.Cursor = Cursors.Hand;
            btnRefreshContacts.Dock = DockStyle.Right;
            btnRefreshContacts.FillColor = Color.FromArgb(107, 143, 166);
            btnRefreshContacts.FillHoverColor = Color.FromArgb(143, 168, 194);
            btnRefreshContacts.FillPressColor = Color.FromArgb(91, 127, 166);
            btnRefreshContacts.Font = new Font("微软雅黑", 9F);
            btnRefreshContacts.Location = new Point(169, 0);
            btnRefreshContacts.MinimumSize = new Size(1, 1);
            btnRefreshContacts.Name = "btnRefreshContacts";
            btnRefreshContacts.RectColor = Color.FromArgb(107, 143, 166);
            btnRefreshContacts.RectHoverColor = Color.FromArgb(143, 168, 194);
            btnRefreshContacts.RectPressColor = Color.FromArgb(91, 127, 166);
            btnRefreshContacts.Size = new Size(39, 40);
            btnRefreshContacts.TabIndex = 2;
            btnRefreshContacts.Text = "刷新";
            btnRefreshContacts.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnRefreshContacts.Click += btnRefreshContacts_Click;
            // 
            // btnBindingContacts
            // 
            btnBindingContacts.Cursor = Cursors.Hand;
            btnBindingContacts.Dock = DockStyle.Right;
            btnBindingContacts.FillColor = Color.FromArgb(107, 143, 166);
            btnBindingContacts.FillHoverColor = Color.FromArgb(143, 168, 194);
            btnBindingContacts.FillPressColor = Color.FromArgb(91, 127, 166);
            btnBindingContacts.Font = new Font("微软雅黑", 9F);
            btnBindingContacts.Location = new Point(208, 0);
            btnBindingContacts.MinimumSize = new Size(1, 1);
            btnBindingContacts.Name = "btnBindingContacts";
            btnBindingContacts.RectColor = Color.FromArgb(107, 143, 166);
            btnBindingContacts.RectHoverColor = Color.FromArgb(143, 168, 194);
            btnBindingContacts.RectPressColor = Color.FromArgb(91, 127, 166);
            btnBindingContacts.Size = new Size(36, 40);
            btnBindingContacts.TabIndex = 1;
            btnBindingContacts.Text = "绑定";
            btnBindingContacts.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnBindingContacts.Click += btnBindingContacts_Click;
            // 
            // txtCurrentContact
            // 
            txtCurrentContact.Cursor = Cursors.IBeam;
            txtCurrentContact.Dock = DockStyle.Fill;
            txtCurrentContact.FillColor = Color.FromArgb(245, 247, 250);
            txtCurrentContact.Font = new Font("微软雅黑", 10F);
            txtCurrentContact.Location = new Point(0, 0);
            txtCurrentContact.Margin = new Padding(4, 5, 4, 5);
            txtCurrentContact.MinimumSize = new Size(1, 16);
            txtCurrentContact.Name = "txtCurrentContact";
            txtCurrentContact.Padding = new Padding(5);
            txtCurrentContact.ReadOnly = true;
            txtCurrentContact.ShowText = false;
            txtCurrentContact.Size = new Size(244, 40);
            txtCurrentContact.TabIndex = 0;
            txtCurrentContact.Text = "未绑定联系人";
            txtCurrentContact.TextAlignment = ContentAlignment.MiddleLeft;
            txtCurrentContact.Watermark = "点击绑定按钮选择联系人";
            txtCurrentContact.KeyDown += TxtCurrentContact_KeyDown;
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(splitContainerData);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Font = new Font("微软雅黑", 12F);
            pnlRight.Location = new Point(0, 0);
            pnlRight.Margin = new Padding(4, 5, 4, 5);
            pnlRight.MinimumSize = new Size(1, 1);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(951, 688);
            pnlRight.TabIndex = 0;
            pnlRight.Text = null;
            pnlRight.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // splitContainerData
            // 
            splitContainerData.Cursor = Cursors.HSplit;
            splitContainerData.Dock = DockStyle.Fill;
            splitContainerData.Location = new Point(0, 0);
            splitContainerData.MinimumSize = new Size(20, 20);
            splitContainerData.Name = "splitContainerData";
            splitContainerData.Orientation = Orientation.Horizontal;
            // 
            // splitContainerData.Panel1
            // 
            splitContainerData.Panel1.Controls.Add(pnlMembers);
            // 
            // splitContainerData.Panel2
            // 
            splitContainerData.Panel2.Controls.Add(pnlOrders);
            splitContainerData.Size = new Size(951, 688);
            splitContainerData.SplitterDistance = 340;
            splitContainerData.SplitterWidth = 5;
            splitContainerData.TabIndex = 0;
            // 
            // pnlMembers
            // 
            pnlMembers.Controls.Add(dgvMembers);
            pnlMembers.Controls.Add(pnlMembersTop);
            pnlMembers.Dock = DockStyle.Fill;
            pnlMembers.Font = new Font("微软雅黑", 12F);
            pnlMembers.Location = new Point(0, 0);
            pnlMembers.Margin = new Padding(4, 5, 4, 5);
            pnlMembers.MinimumSize = new Size(1, 1);
            pnlMembers.Name = "pnlMembers";
            pnlMembers.Size = new Size(951, 340);
            pnlMembers.TabIndex = 0;
            pnlMembers.Text = null;
            pnlMembers.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvMembers
            // 
            dgvMembers.AllowUserToAddRows = false;
            dgvMembers.AllowUserToDeleteRows = false;
            dgvMembers.AllowUserToResizeRows = false;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(248, 250, 252);
            dgvMembers.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            dgvMembers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMembers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvMembers.BackgroundColor = Color.White;
            dgvMembers.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = Color.FromArgb(107, 143, 166);
            dataGridViewCellStyle6.Font = new Font("微软雅黑", 10F);
            dataGridViewCellStyle6.ForeColor = Color.White;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
            dgvMembers.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            dgvMembers.ColumnHeadersHeight = 28;
            dgvMembers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvMembers.ContextMenuStrip = cmsMembers;
            dgvMembers.EnableHeadersVisualStyles = false;
            dgvMembers.Font = new Font("微软雅黑", 9F);
            dgvMembers.GridColor = Color.FromArgb(107, 143, 166);
            dgvMembers.Location = new Point(0, 30);
            dgvMembers.MultiSelect = false;
            dgvMembers.Name = "dgvMembers";
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = Color.FromArgb(248, 250, 252);
            dataGridViewCellStyle7.Font = new Font("微软雅黑", 9F);
            dataGridViewCellStyle7.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle7.SelectionBackColor = Color.FromArgb(107, 143, 166);
            dataGridViewCellStyle7.SelectionForeColor = Color.White;
            dataGridViewCellStyle7.WrapMode = DataGridViewTriState.True;
            dgvMembers.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            dgvMembers.RowHeadersVisible = false;
            dgvMembers.RowHeadersWidth = 51;
            dataGridViewCellStyle8.BackColor = Color.White;
            dataGridViewCellStyle8.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvMembers.RowsDefaultCellStyle = dataGridViewCellStyle8;
            dgvMembers.RowTemplate.Height = 24;
            dgvMembers.SelectedIndex = -1;
            dgvMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMembers.Size = new Size(951, 310);
            dgvMembers.StripeOddColor = Color.FromArgb(248, 250, 252);
            dgvMembers.TabIndex = 1;
            dgvMembers.SelectionChanged += dgvMembers_SelectionChanged;
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
            pnlMembersTop.Size = new Size(951, 30);
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
            lblMemberInfo.Size = new Size(951, 30);
            lblMemberInfo.TabIndex = 0;
            lblMemberInfo.Text = "会员列表 (共0人)";
            lblMemberInfo.TextAlign = ContentAlignment.MiddleLeft;
            lblMemberInfo.Paint += lblMemberInfo_Paint;
            // 
            // pnlOrders
            // 
            pnlOrders.Controls.Add(dgvOrders);
            pnlOrders.Controls.Add(pnlOrdersTop);
            pnlOrders.Dock = DockStyle.Fill;
            pnlOrders.Font = new Font("微软雅黑", 12F);
            pnlOrders.Location = new Point(0, 0);
            pnlOrders.Margin = new Padding(4, 5, 4, 5);
            pnlOrders.MinimumSize = new Size(1, 1);
            pnlOrders.Name = "pnlOrders";
            pnlOrders.Size = new Size(951, 343);
            pnlOrders.TabIndex = 0;
            pnlOrders.Text = null;
            pnlOrders.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvOrders
            // 
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.AllowUserToDeleteRows = false;
            dgvOrders.AllowUserToResizeRows = false;
            dataGridViewCellStyle9.BackColor = Color.FromArgb(248, 250, 252);
            dgvOrders.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle9;
            dgvOrders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvOrders.BackgroundColor = Color.White;
            dgvOrders.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle10.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.BackColor = Color.FromArgb(107, 143, 166);
            dataGridViewCellStyle10.Font = new Font("微软雅黑", 10F);
            dataGridViewCellStyle10.ForeColor = Color.White;
            dataGridViewCellStyle10.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = DataGridViewTriState.True;
            dgvOrders.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
            dgvOrders.ColumnHeadersHeight = 28;
            dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.Font = new Font("微软雅黑", 9F);
            dgvOrders.GridColor = Color.FromArgb(107, 143, 166);
            dgvOrders.Location = new Point(0, 30);
            dgvOrders.Name = "dgvOrders";
            dataGridViewCellStyle11.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = Color.FromArgb(248, 250, 252);
            dataGridViewCellStyle11.Font = new Font("微软雅黑", 9F);
            dataGridViewCellStyle11.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle11.SelectionBackColor = Color.FromArgb(107, 143, 166);
            dataGridViewCellStyle11.SelectionForeColor = Color.White;
            dataGridViewCellStyle11.WrapMode = DataGridViewTriState.True;
            dgvOrders.RowHeadersDefaultCellStyle = dataGridViewCellStyle11;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.RowHeadersWidth = 51;
            dataGridViewCellStyle12.BackColor = Color.White;
            dataGridViewCellStyle12.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvOrders.RowsDefaultCellStyle = dataGridViewCellStyle12;
            dgvOrders.RowTemplate.Height = 24;
            dgvOrders.SelectedIndex = -1;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.Size = new Size(951, 313);
            dgvOrders.StripeOddColor = Color.FromArgb(248, 250, 252);
            dgvOrders.TabIndex = 1;
            dgvOrders.MouseDown += DgvOrders_MouseDown;
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
            pnlOrdersTop.Size = new Size(951, 30);
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
            lblOrderInfo.Size = new Size(951, 30);
            lblOrderInfo.TabIndex = 0;
            lblOrderInfo.Text = "订单列表 (共0单)";
            lblOrderInfo.TextAlign = ContentAlignment.MiddleLeft;
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
            pnlTopButtons.Font = new Font("微软雅黑", 12F);
            pnlTopButtons.Location = new Point(0, 35);
            pnlTopButtons.Margin = new Padding(4, 5, 4, 5);
            pnlTopButtons.MinimumSize = new Size(1, 1);
            pnlTopButtons.Name = "pnlTopButtons";
            pnlTopButtons.Padding = new Padding(5);
            pnlTopButtons.Size = new Size(1200, 60);
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
            btnClearData.FillColor = Color.FromArgb(90, 122, 138);
            btnClearData.FillHoverColor = Color.FromArgb(91, 127, 166);
            btnClearData.FillPressColor = Color.FromArgb(74, 95, 122);
            btnClearData.Font = new Font("微软雅黑", 10F);
            btnClearData.Location = new Point(1039, 12);
            btnClearData.MinimumSize = new Size(1, 1);
            btnClearData.Name = "btnClearData";
            btnClearData.RectColor = Color.FromArgb(90, 122, 138);
            btnClearData.RectHoverColor = Color.FromArgb(91, 127, 166);
            btnClearData.RectPressColor = Color.FromArgb(74, 95, 122);
            btnClearData.Size = new Size(75, 40);
            btnClearData.TabIndex = 4;
            btnClearData.Text = "清空数据";
            btnClearData.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClearData.Click += btnClearData_Click;
            // 
            // btnCreditWithdrawManage
            // 
            btnCreditWithdrawManage.Cursor = Cursors.Hand;
            btnCreditWithdrawManage.FillColor = Color.FromArgb(107, 143, 166);
            btnCreditWithdrawManage.FillHoverColor = Color.FromArgb(143, 168, 194);
            btnCreditWithdrawManage.FillPressColor = Color.FromArgb(91, 127, 166);
            btnCreditWithdrawManage.Font = new Font("微软雅黑", 10F);
            btnCreditWithdrawManage.Location = new Point(359, 12);
            btnCreditWithdrawManage.MinimumSize = new Size(1, 1);
            btnCreditWithdrawManage.Name = "btnCreditWithdrawManage";
            btnCreditWithdrawManage.RectColor = Color.FromArgb(107, 143, 166);
            btnCreditWithdrawManage.RectHoverColor = Color.FromArgb(143, 168, 194);
            btnCreditWithdrawManage.RectPressColor = Color.FromArgb(91, 127, 166);
            btnCreditWithdrawManage.Size = new Size(120, 40);
            btnCreditWithdrawManage.TabIndex = 4;
            btnCreditWithdrawManage.Text = "上下分管理";
            btnCreditWithdrawManage.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnCreditWithdrawManage.Click += btnCreditWithdrawManage_Click;
            // 
            // btnOpenLotteryResult
            // 
            btnOpenLotteryResult.Cursor = Cursors.Hand;
            btnOpenLotteryResult.FillColor = Color.FromArgb(107, 143, 166);
            btnOpenLotteryResult.FillHoverColor = Color.FromArgb(143, 168, 194);
            btnOpenLotteryResult.FillPressColor = Color.FromArgb(91, 127, 166);
            btnOpenLotteryResult.Font = new Font("微软雅黑", 10F);
            btnOpenLotteryResult.Location = new Point(485, 12);
            btnOpenLotteryResult.MinimumSize = new Size(1, 1);
            btnOpenLotteryResult.Name = "btnOpenLotteryResult";
            btnOpenLotteryResult.RectColor = Color.FromArgb(107, 143, 166);
            btnOpenLotteryResult.RectHoverColor = Color.FromArgb(143, 168, 194);
            btnOpenLotteryResult.RectPressColor = Color.FromArgb(91, 127, 166);
            btnOpenLotteryResult.Size = new Size(100, 40);
            btnOpenLotteryResult.TabIndex = 3;
            btnOpenLotteryResult.Text = "数据记录";
            btnOpenLotteryResult.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnOpenLotteryResult.Click += btnOpenLotteryResult_Click;
            // 
            // btnConnect
            // 
            btnConnect.Cursor = Cursors.Hand;
            btnConnect.FillColor = Color.FromArgb(107, 143, 166);
            btnConnect.FillHoverColor = Color.FromArgb(143, 168, 194);
            btnConnect.FillPressColor = Color.FromArgb(91, 127, 166);
            btnConnect.FillSelectedColor = Color.FromArgb(91, 127, 166);
            btnConnect.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            btnConnect.Location = new Point(253, 12);
            btnConnect.MinimumSize = new Size(1, 1);
            btnConnect.Name = "btnConnect";
            btnConnect.Radius = 6;
            btnConnect.RectColor = Color.FromArgb(107, 143, 166);
            btnConnect.RectHoverColor = Color.FromArgb(143, 168, 194);
            btnConnect.RectPressColor = Color.FromArgb(91, 127, 166);
            btnConnect.RectSelectedColor = Color.FromArgb(91, 127, 166);
            btnConnect.Size = new Size(100, 40);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "启动微信";
            btnConnect.TipsFont = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnConnect.Click += btnConnect_Click;
            // 
            // btnLog
            // 
            btnLog.Cursor = Cursors.Hand;
            btnLog.FillColor = Color.FromArgb(107, 143, 166);
            btnLog.FillHoverColor = Color.FromArgb(143, 168, 194);
            btnLog.FillPressColor = Color.FromArgb(91, 127, 166);
            btnLog.Font = new Font("微软雅黑", 10F);
            btnLog.Location = new Point(591, 12);
            btnLog.MinimumSize = new Size(1, 1);
            btnLog.Name = "btnLog";
            btnLog.RectColor = Color.FromArgb(107, 143, 166);
            btnLog.RectHoverColor = Color.FromArgb(143, 168, 194);
            btnLog.RectPressColor = Color.FromArgb(91, 127, 166);
            btnLog.Size = new Size(95, 40);
            btnLog.TabIndex = 1;
            btnLog.Text = "日志";
            btnLog.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnLog.Click += btnLog_Click;
            // 
            // btnSettings
            // 
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.FillColor = Color.FromArgb(107, 143, 166);
            btnSettings.FillHoverColor = Color.FromArgb(143, 168, 194);
            btnSettings.FillPressColor = Color.FromArgb(91, 127, 166);
            btnSettings.Font = new Font("微软雅黑", 10F);
            btnSettings.Location = new Point(1120, 12);
            btnSettings.MinimumSize = new Size(1, 1);
            btnSettings.Name = "btnSettings";
            btnSettings.RectColor = Color.FromArgb(107, 143, 166);
            btnSettings.RectHoverColor = Color.FromArgb(143, 168, 194);
            btnSettings.RectPressColor = Color.FromArgb(91, 127, 166);
            btnSettings.Size = new Size(75, 40);
            btnSettings.TabIndex = 0;
            btnSettings.Text = "设置";
            btnSettings.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSettings.Click += btnSettings_Click;
            // 
            // statusStrip
            // 
            statusStrip.BackColor = Color.FromArgb(245, 247, 250);
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
            Controls.Add(splitContainerMain);
            Controls.Add(pnlTopButtons);
            Controls.Add(statusStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "VxMain";
            Text = "智能管理系统";
            ZoomScaleRect = new Rectangle(15, 15, 980, 762);
            Load += VxMain_Load;
            cmsMembers.ResumeLayout(false);
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            (splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            pnl_fastsetting.ResumeLayout(false);
            pnl_opendata.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvContacts).EndInit();
            pnlLeftTop.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            splitContainerData.Panel1.ResumeLayout(false);
            splitContainerData.Panel2.ResumeLayout(false);
            (splitContainerData).EndInit();
            splitContainerData.ResumeLayout(false);
            pnlMembers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMembers).EndInit();
            pnlMembersTop.ResumeLayout(false);
            pnlOrders.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvOrders).EndInit();
            pnlOrdersTop.ResumeLayout(false);
            pnlTopButtons.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Sunny.UI.UISplitContainer splitContainerMain;
        private Sunny.UI.UIPanel pnlLeft;
        private Sunny.UI.UIDataGridView dgvContacts;
        private Sunny.UI.UIPanel pnlLeftTop;
        private Sunny.UI.UIButton btnRefreshContacts;
        private Sunny.UI.UIButton btnBindingContacts;
        private Sunny.UI.UITextBox txtCurrentContact;
        private Sunny.UI.UIPanel pnlRight;
        private Sunny.UI.UISplitContainer splitContainerData;
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
        private Sunny.UI.UITextBox txtSealSeconds;
        private System.Windows.Forms.Label lblMinBet;
        private Sunny.UI.UITextBox txtMinBet;
        private System.Windows.Forms.Label lblMaxBet;
        private Sunny.UI.UITextBox txtMaxBet;
        private System.Windows.Forms.Label lblPlatform;
        private Sunny.UI.UIComboBox cbxPlatform;
        private System.Windows.Forms.Label lblAutoBetUsername;
        private Sunny.UI.UITextBox txtAutoBetUsername;
        private System.Windows.Forms.Label lblAutoBetPassword;
        private Sunny.UI.UITextBox txtAutoBetPassword;
        private System.Windows.Forms.Label lblOdds;
        private Sunny.UI.UIDoubleUpDown txtOdds;
        private Sunny.UI.UIButton btnStartBrowser;
        private Sunny.UI.UIButton btnConfigManager;
        private Sunny.UI.UISwitch swi_OrdersTasking;
        private Sunny.UI.UISwitch swiAutoOrdersBet;
    }
}
