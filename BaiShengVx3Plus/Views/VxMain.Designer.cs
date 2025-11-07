using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus
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
            ucBinggoDataCur = new BaiShengVx3Plus.UserControls.UcBinggoDataCur();
            ucBinggoDataLast = new BaiShengVx3Plus.UserControls.UcBinggoDataLast();
            lblSealSeconds = new Label();
            txtSealSeconds = new Sunny.UI.UIIntegerUpDown();
            lblMinBet = new Label();
            txtMinBet = new Sunny.UI.UIIntegerUpDown();
            lblMaxBet = new Label();
            txtMaxBet = new Sunny.UI.UIIntegerUpDown();
            chkAdminMode = new Sunny.UI.UICheckBox(); // 🔥 管理模式checkbox
            cmsMembers = new Sunny.UI.UIContextMenuStrip();
            tsmiClearBalance = new ToolStripMenuItem();
            tsmiDeleteMember = new ToolStripMenuItem();
            tsmiSetMemberType = new ToolStripMenuItem();
            tsmiSetNormal = new ToolStripMenuItem();
            tsmiSetMember = new ToolStripMenuItem();
            tsmiSetAgent = new ToolStripMenuItem();
            tsmiSetBlue = new ToolStripMenuItem();
            tsmiSetYellow = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            tsmiViewBalanceChange = new ToolStripMenuItem();
            splitContainerMain = new Sunny.UI.UISplitContainer();
            pnlLeft = new Sunny.UI.UIPanel();
            pnl_fastsetting = new Sunny.UI.UIPanel();
            pnl_opendata = new Sunny.UI.UIPanel();
            dgvContacts = new Sunny.UI.UIDataGridView();
            pnlLeftTop = new Sunny.UI.UIPanel();
            btnRefreshContacts = new Sunny.UI.UIButton();
            btnBindingContacts = new Sunny.UI.UIButton();
            txtCurrentContact = new Sunny.UI.UITextBox();
            pnlRight = new Sunny.UI.UIPanel();
            splitContainerRight = new Sunny.UI.UISplitContainer();
            pnlMembers = new Sunny.UI.UIPanel();
            dgvMembers = new Sunny.UI.UIDataGridView();
            pnlMembersTop = new Sunny.UI.UIPanel();
            lblMemberInfo = new Sunny.UI.UILabel();
            pnlOrders = new Sunny.UI.UIPanel();
            dgvOrders = new Sunny.UI.UIDataGridView();
            pnlOrdersTop = new Sunny.UI.UIPanel();
            lblOrderInfo = new Sunny.UI.UILabel();
            pnlTopButtons = new Sunny.UI.UIPanel();
            ucUserInfo1 = new BaiShengVx3Plus.Views.UcUserInfo();
            btnClearData = new Sunny.UI.UIButton();
            btnOpenLotteryResult = new Sunny.UI.UIButton();
            btnConnect = new Sunny.UI.UIButton();
            btnLog = new Sunny.UI.UIButton();
            btnSettings = new Sunny.UI.UIButton();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
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
            (splitContainerRight).BeginInit();
            splitContainerRight.Panel1.SuspendLayout();
            splitContainerRight.Panel2.SuspendLayout();
            splitContainerRight.SuspendLayout();
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
            ucBinggoDataCur.BackColor = Color.FromArgb(243, 249, 255);
            ucBinggoDataCur.BorderStyle = BorderStyle.FixedSingle;
            ucBinggoDataCur.Location = new Point(0, 0);
            ucBinggoDataCur.Name = "ucBinggoDataCur";
            ucBinggoDataCur.Size = new Size(239, 101);
            ucBinggoDataCur.TabIndex = 0;
            // 
            // ucBinggoDataLast
            // 
            ucBinggoDataLast.BackColor = Color.FromArgb(255, 248, 225);
            ucBinggoDataLast.Location = new Point(0, 108);
            ucBinggoDataLast.Name = "ucBinggoDataLast";
            ucBinggoDataLast.Size = new Size(239, 118);
            ucBinggoDataLast.TabIndex = 1;
            // 
            // lblSealSeconds
            // 
            lblSealSeconds.Font = new Font("微软雅黑", 10F);
            lblSealSeconds.Location = new Point(5, 30);
            lblSealSeconds.Name = "lblSealSeconds";
            lblSealSeconds.Size = new Size(90, 23);
            lblSealSeconds.TabIndex = 0;
            lblSealSeconds.Text = "封盘提前(秒)";
            lblSealSeconds.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSealSeconds
            // 
            txtSealSeconds.Font = new Font("微软雅黑", 10F);
            txtSealSeconds.Location = new Point(100, 28);
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
            lblMinBet.Location = new Point(5, 63);
            lblMinBet.Name = "lblMinBet";
            lblMinBet.Size = new Size(90, 23);
            lblMinBet.TabIndex = 2;
            lblMinBet.Text = "最小投注";
            lblMinBet.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtMinBet
            // 
            txtMinBet.Font = new Font("微软雅黑", 10F);
            txtMinBet.Location = new Point(100, 61);
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
            lblMaxBet.Location = new Point(5, 96);
            lblMaxBet.Name = "lblMaxBet";
            lblMaxBet.Size = new Size(90, 23);
            lblMaxBet.TabIndex = 4;
            lblMaxBet.Text = "最大投注";
            lblMaxBet.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtMaxBet
            // 
            txtMaxBet.Font = new Font("微软雅黑", 10F);
            txtMaxBet.Location = new Point(100, 94);
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
            // chkAdminMode
            // 
            chkAdminMode.Font = new Font("微软雅黑", 9F);
            chkAdminMode.Location = new Point(100, 130);
            chkAdminMode.MinimumSize = new Size(1, 1);
            chkAdminMode.Name = "chkAdminMode";
            chkAdminMode.Padding = new Padding(22, 0, 0, 0);
            chkAdminMode.Size = new Size(130, 25);
            chkAdminMode.TabIndex = 6;
            chkAdminMode.Text = "管理模式";
            chkAdminMode.CheckedChanged += ChkAdminMode_CheckedChanged;
            // 
            // tsmiClearBalance
            // 
            tsmiClearBalance.Name = "tsmiClearBalance";
            tsmiClearBalance.Size = new Size(199, 26);
            tsmiClearBalance.Text = "清分";
            tsmiClearBalance.Click += TsmiClearBalance_Click;
            // 
            // tsmiDeleteMember
            // 
            tsmiDeleteMember.Name = "tsmiDeleteMember";
            tsmiDeleteMember.Size = new Size(199, 26);
            tsmiDeleteMember.Text = "删除会员";
            tsmiDeleteMember.Click += TsmiDeleteMember_Click;
            // 
            // tsmiSetMemberType
            // 
            tsmiSetMemberType.DropDownItems.AddRange(new ToolStripItem[] {
                tsmiSetNormal,
                tsmiSetMember,
                tsmiSetAgent,
                tsmiSetBlue,
                tsmiSetYellow
            });
            tsmiSetMemberType.Name = "tsmiSetMemberType";
            tsmiSetMemberType.Size = new Size(199, 26);
            tsmiSetMemberType.Text = "设置会员类型";
            // 
            // tsmiSetNormal
            // 
            tsmiSetNormal.Name = "tsmiSetNormal";
            tsmiSetNormal.Size = new Size(180, 26);
            tsmiSetNormal.Text = "普会";
            tsmiSetNormal.Click += TsmiSetMemberType_Click;
            // 
            // tsmiSetMember
            // 
            tsmiSetMember.Name = "tsmiSetMember";
            tsmiSetMember.Size = new Size(180, 26);
            tsmiSetMember.Text = "会员（盘内）";
            tsmiSetMember.Click += TsmiSetMemberType_Click;
            // 
            // tsmiSetAgent
            // 
            tsmiSetAgent.Name = "tsmiSetAgent";
            tsmiSetAgent.Size = new Size(180, 26);
            tsmiSetAgent.Text = "托";
            tsmiSetAgent.Click += TsmiSetMemberType_Click;
            // 
            // tsmiSetBlue
            // 
            tsmiSetBlue.Name = "tsmiSetBlue";
            tsmiSetBlue.Size = new Size(180, 26);
            tsmiSetBlue.Text = "蓝会（盘外）";
            tsmiSetBlue.Click += TsmiSetMemberType_Click;
            // 
            // tsmiSetYellow
            // 
            tsmiSetYellow.Name = "tsmiSetYellow";
            tsmiSetYellow.Size = new Size(180, 26);
            tsmiSetYellow.Text = "黄会";
            tsmiSetYellow.Click += TsmiSetMemberType_Click;
            // 
            // tsmiViewBalanceChange
            // 
            tsmiViewBalanceChange.Name = "tsmiViewBalanceChange";
            tsmiViewBalanceChange.Size = new Size(199, 26);
            tsmiViewBalanceChange.Text = "资金变动";
            tsmiViewBalanceChange.Click += TsmiViewBalanceChange_Click;
            // 
            // cmsMembers
            // 
            cmsMembers.BackColor = Color.FromArgb(243, 249, 255);
            cmsMembers.Font = new Font("微软雅黑", 10F);
            cmsMembers.ImageScalingSize = new Size(20, 20);
            cmsMembers.Items.AddRange(new ToolStripItem[] {
                tsmiClearBalance,
                tsmiDeleteMember,
                tsmiSetMemberType,
                toolStripSeparator1,
                tsmiViewBalanceChange
            });
            cmsMembers.Name = "cmsMembers";
            cmsMembers.Size = new Size(200, 120);
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
            splitContainerMain.Size = new Size(1200, 668);
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
            pnlLeft.Size = new Size(244, 668);
            pnlLeft.TabIndex = 0;
            pnlLeft.Text = null;
            pnlLeft.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // pnl_fastsetting
            // 
            pnl_fastsetting.Controls.Add(lblSealSeconds);
            pnl_fastsetting.Controls.Add(txtSealSeconds);
            pnl_fastsetting.Controls.Add(lblMinBet);
            pnl_fastsetting.Controls.Add(txtMinBet);
            pnl_fastsetting.Controls.Add(lblMaxBet);
            pnl_fastsetting.Controls.Add(txtMaxBet);
            pnl_fastsetting.Controls.Add(chkAdminMode); // 🔥 管理模式checkbox
            pnl_fastsetting.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            pnl_fastsetting.Location = new Point(4, 484);
            pnl_fastsetting.Margin = new Padding(4, 5, 4, 5);
            pnl_fastsetting.MinimumSize = new Size(1, 1);
            pnl_fastsetting.Name = "pnl_fastsetting";
            pnl_fastsetting.Size = new Size(237, 179);
            pnl_fastsetting.TabIndex = 3;
            pnl_fastsetting.Text = "快速设置";
            pnl_fastsetting.TextAlignment = ContentAlignment.TopCenter;
            // 
            // pnl_opendata
            // 
            pnl_opendata.Controls.Add(ucBinggoDataLast);
            pnl_opendata.Controls.Add(ucBinggoDataCur);
            pnl_opendata.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            pnl_opendata.Location = new Point(3, 246);
            pnl_opendata.Margin = new Padding(4, 5, 4, 5);
            pnl_opendata.MinimumSize = new Size(1, 1);
            pnl_opendata.Name = "pnl_opendata";
            pnl_opendata.Size = new Size(239, 233);
            pnl_opendata.TabIndex = 2;
            pnl_opendata.Text = null;
            pnl_opendata.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvContacts
            // 
            dgvContacts.AllowUserToAddRows = false;
            dgvContacts.AllowUserToDeleteRows = false;
            dgvContacts.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            dgvContacts.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvContacts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvContacts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvContacts.BackgroundColor = Color.White;
            dgvContacts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(80, 160, 255);
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
            dgvContacts.GridColor = Color.FromArgb(80, 160, 255);
            dgvContacts.Location = new Point(3, 43);
            dgvContacts.MultiSelect = false;
            dgvContacts.Name = "dgvContacts";
            dgvContacts.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle3.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(80, 160, 255);
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
            dgvContacts.Size = new Size(244, 200);
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
            btnRefreshContacts.Font = new Font("微软雅黑", 9F);
            btnRefreshContacts.Location = new Point(169, 0);
            btnRefreshContacts.MinimumSize = new Size(1, 1);
            btnRefreshContacts.Name = "btnRefreshContacts";
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
            btnBindingContacts.Font = new Font("微软雅黑", 9F);
            btnBindingContacts.Location = new Point(208, 0);
            btnBindingContacts.MinimumSize = new Size(1, 1);
            btnBindingContacts.Name = "btnBindingContacts";
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
            txtCurrentContact.FillColor = Color.FromArgb(243, 249, 255);
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
            txtCurrentContact.KeyDown += TxtCurrentContact_KeyDown; // 🔥 管理模式手动绑定
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
            pnlRight.Size = new Size(951, 668);
            pnlRight.TabIndex = 0;
            pnlRight.Text = null;
            pnlRight.TextAlignment = ContentAlignment.MiddleCenter;
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
            splitContainerRight.Size = new Size(951, 668);
            splitContainerRight.SplitterDistance = 312;
            splitContainerRight.SplitterWidth = 5;
            splitContainerRight.TabIndex = 0;
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
            pnlMembers.Size = new Size(951, 312);
            pnlMembers.TabIndex = 0;
            pnlMembers.Text = null;
            pnlMembers.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvMembers
            // 
            dgvMembers.AllowUserToAddRows = false;
            dgvMembers.AllowUserToDeleteRows = false;
            dgvMembers.AllowUserToResizeRows = false;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(235, 243, 255);
            dgvMembers.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            dgvMembers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMembers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvMembers.BackgroundColor = Color.White;
            dgvMembers.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle6.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle6.ForeColor = Color.White;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
            dgvMembers.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            dgvMembers.ColumnHeadersHeight = 32;
            dgvMembers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvMembers.ContextMenuStrip = cmsMembers;
            dgvMembers.EnableHeadersVisualStyles = false;
            dgvMembers.Font = new Font("微软雅黑", 10F);
            dgvMembers.GridColor = Color.FromArgb(80, 160, 255);
            dgvMembers.Location = new Point(0, 30);
            dgvMembers.MultiSelect = false;
            dgvMembers.Name = "dgvMembers";
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle7.Font = new Font("微软雅黑", 10F);
            dataGridViewCellStyle7.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle7.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle7.SelectionForeColor = Color.White;
            dataGridViewCellStyle7.WrapMode = DataGridViewTriState.True;
            dgvMembers.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            dgvMembers.RowHeadersVisible = false;
            dgvMembers.RowHeadersWidth = 51;
            dataGridViewCellStyle8.BackColor = Color.White;
            dataGridViewCellStyle8.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvMembers.RowsDefaultCellStyle = dataGridViewCellStyle8;
            dgvMembers.RowTemplate.Height = 29;
            dgvMembers.SelectedIndex = -1;
            dgvMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMembers.Size = new Size(951, 282);
            dgvMembers.StripeOddColor = Color.FromArgb(235, 243, 255);
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
            pnlOrders.Size = new Size(951, 351);
            pnlOrders.TabIndex = 0;
            pnlOrders.Text = null;
            pnlOrders.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvOrders
            // 
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.AllowUserToDeleteRows = false;
            dgvOrders.AllowUserToResizeRows = false;
            dataGridViewCellStyle9.BackColor = Color.FromArgb(235, 243, 255);
            dgvOrders.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle9;
            dgvOrders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvOrders.BackgroundColor = Color.White;
            dgvOrders.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle10.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle10.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle10.ForeColor = Color.White;
            dataGridViewCellStyle10.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = DataGridViewTriState.True;
            dgvOrders.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
            dgvOrders.ColumnHeadersHeight = 32;
            dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.Font = new Font("微软雅黑", 10F);
            dgvOrders.GridColor = Color.FromArgb(80, 160, 255);
            dgvOrders.Location = new Point(0, 30);
            dgvOrders.MultiSelect = false;
            dgvOrders.Name = "dgvOrders";
            dataGridViewCellStyle11.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle11.Font = new Font("微软雅黑", 10F);
            dataGridViewCellStyle11.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle11.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle11.SelectionForeColor = Color.White;
            dataGridViewCellStyle11.WrapMode = DataGridViewTriState.True;
            dgvOrders.RowHeadersDefaultCellStyle = dataGridViewCellStyle11;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.RowHeadersWidth = 51;
            dataGridViewCellStyle12.BackColor = Color.White;
            dataGridViewCellStyle12.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvOrders.RowsDefaultCellStyle = dataGridViewCellStyle12;
            dgvOrders.RowTemplate.Height = 29;
            dgvOrders.SelectedIndex = -1;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.Size = new Size(951, 321);
            dgvOrders.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvOrders.TabIndex = 1;
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
            btnClearData.Font = new Font("微软雅黑", 10F);
            btnClearData.Location = new Point(989, 12);
            btnClearData.MinimumSize = new Size(1, 1);
            btnClearData.Name = "btnClearData";
            btnClearData.Size = new Size(100, 40);
            btnClearData.TabIndex = 4;
            btnClearData.Text = "清空数据";
            btnClearData.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClearData.Click += btnClearData_Click;
            // 
            // btnOpenLotteryResult
            // 
            btnOpenLotteryResult.Cursor = Cursors.Hand;
            btnOpenLotteryResult.Font = new Font("微软雅黑", 10F);
            btnOpenLotteryResult.Location = new Point(460, 14);
            btnOpenLotteryResult.MinimumSize = new Size(1, 1);
            btnOpenLotteryResult.Name = "btnOpenLotteryResult";
            btnOpenLotteryResult.Size = new Size(100, 40);
            btnOpenLotteryResult.TabIndex = 3;
            btnOpenLotteryResult.Text = "开奖结果";
            btnOpenLotteryResult.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnOpenLotteryResult.Click += btnOpenLotteryResult_Click;
            // 
            // btnConnect
            // 
            btnConnect.Cursor = Cursors.Hand;
            btnConnect.FillHoverColor = Color.FromArgb(100, 180, 255);
            btnConnect.FillPressColor = Color.FromArgb(60, 140, 235);
            btnConnect.FillSelectedColor = Color.FromArgb(60, 140, 235);
            btnConnect.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            btnConnect.Location = new Point(253, 12);
            btnConnect.MinimumSize = new Size(1, 1);
            btnConnect.Name = "btnConnect";
            btnConnect.Radius = 6;
            btnConnect.RectHoverColor = Color.FromArgb(100, 180, 255);
            btnConnect.RectPressColor = Color.FromArgb(60, 140, 235);
            btnConnect.RectSelectedColor = Color.FromArgb(60, 140, 235);
            btnConnect.Size = new Size(100, 40);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "连接";
            btnConnect.TipsFont = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnConnect.Click += btnConnect_Click;
            // 
            // btnLog
            // 
            btnLog.Cursor = Cursors.Hand;
            btnLog.Font = new Font("微软雅黑", 10F);
            btnLog.Location = new Point(359, 14);
            btnLog.MinimumSize = new Size(1, 1);
            btnLog.Name = "btnLog";
            btnLog.Size = new Size(95, 40);
            btnLog.TabIndex = 1;
            btnLog.Text = "日志";
            btnLog.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnLog.Click += btnLog_Click;
            // 
            // btnSettings
            // 
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.Dock = DockStyle.Right;
            btnSettings.Font = new Font("微软雅黑", 10F);
            btnSettings.Location = new Point(1095, 5);
            btnSettings.MinimumSize = new Size(1, 1);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(100, 50);
            btnSettings.TabIndex = 0;
            btnSettings.Text = "设置";
            btnSettings.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSettings.Click += btnSettings_Click;
            // 
            // statusStrip
            // 
            statusStrip.BackColor = Color.FromArgb(243, 249, 255);
            statusStrip.Font = new Font("微软雅黑", 10F);
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 763);
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
            ClientSize = new Size(1200, 788);
            Controls.Add(splitContainerMain);
            Controls.Add(pnlTopButtons);
            Controls.Add(statusStrip);
            Name = "VxMain";
            Text = "百胜VX3Plus - 管理系统";
            ZoomScaleRect = new Rectangle(15, 15, 980, 762);
            Load += VxMain_Load;
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
            splitContainerRight.Panel1.ResumeLayout(false);
            splitContainerRight.Panel2.ResumeLayout(false);
            (splitContainerRight).EndInit();
            splitContainerRight.ResumeLayout(false);
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
        // 设置会员类型子菜单
        private ToolStripMenuItem tsmiSetNormal;           // 普会
        private ToolStripMenuItem tsmiSetMember;           // 会员（盘内）
        private ToolStripMenuItem tsmiSetAgent;            // 托
        private ToolStripMenuItem tsmiSetBlue;             // 蓝会（盘外）
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
        private Sunny.UI.UICheckBox chkAdminMode; // 🔥 管理模式checkbox
    }
}
