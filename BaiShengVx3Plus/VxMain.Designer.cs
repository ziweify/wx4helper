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
            splitContainerMain = new Sunny.UI.UISplitContainer();
            pnlLeft = new Sunny.UI.UIPanel();
            dgvContacts = new Sunny.UI.UIDataGridView();
            pnlLeftTop = new Sunny.UI.UIPanel();
            btnRefreshContacts = new Sunny.UI.UIButton();
            lblContactList = new Sunny.UI.UILabel();
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
            btnClearData = new Sunny.UI.UIButton();
            btnOpenLotteryResult = new Sunny.UI.UIButton();
            btnLog = new Sunny.UI.UIButton();
            btnSettings = new Sunny.UI.UIButton();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            (splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            pnlLeft.SuspendLayout();
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
            dgvContacts.Location = new Point(0, 40);
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
            dgvContacts.Size = new Size(244, 286);
            dgvContacts.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvContacts.TabIndex = 1;
            dgvContacts.SelectionChanged += dgvContacts_SelectionChanged;
            // 
            // pnlLeftTop
            // 
            pnlLeftTop.Controls.Add(btnRefreshContacts);
            pnlLeftTop.Controls.Add(lblContactList);
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
            btnRefreshContacts.Location = new Point(189, 0);
            btnRefreshContacts.MinimumSize = new Size(1, 1);
            btnRefreshContacts.Name = "btnRefreshContacts";
            btnRefreshContacts.Size = new Size(55, 40);
            btnRefreshContacts.TabIndex = 1;
            btnRefreshContacts.Text = "刷新";
            btnRefreshContacts.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnRefreshContacts.Click += btnRefreshContacts_Click;
            // 
            // lblContactList
            // 
            lblContactList.Dock = DockStyle.Fill;
            lblContactList.Font = new Font("微软雅黑", 11F, FontStyle.Bold);
            lblContactList.ForeColor = Color.FromArgb(48, 48, 48);
            lblContactList.Location = new Point(0, 0);
            lblContactList.Name = "lblContactList";
            lblContactList.Size = new Size(244, 40);
            lblContactList.TabIndex = 0;
            lblContactList.Text = "联系人列表";
            lblContactList.TextAlign = ContentAlignment.MiddleCenter;
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
            dgvMembers.CellValueChanged += dgvMembers_CellValueChanged;
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
            dgvOrders.CellValueChanged += dgvOrders_CellValueChanged;
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
            pnlTopButtons.Controls.Add(btnClearData);
            pnlTopButtons.Controls.Add(btnOpenLotteryResult);
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
            // btnClearData
            // 
            btnClearData.Cursor = Cursors.Hand;
            btnClearData.Font = new Font("微软雅黑", 10F);
            btnClearData.Location = new Point(215, 10);
            btnClearData.MinimumSize = new Size(1, 1);
            btnClearData.Name = "btnClearData";
            btnClearData.Size = new Size(100, 40);
            btnClearData.TabIndex = 3;
            btnClearData.Text = "清空数据";
            btnClearData.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClearData.Click += btnClearData_Click;
            // 
            // btnOpenLotteryResult
            // 
            btnOpenLotteryResult.Cursor = Cursors.Hand;
            btnOpenLotteryResult.Font = new Font("微软雅黑", 10F);
            btnOpenLotteryResult.Location = new Point(110, 10);
            btnOpenLotteryResult.MinimumSize = new Size(1, 1);
            btnOpenLotteryResult.Name = "btnOpenLotteryResult";
            btnOpenLotteryResult.Size = new Size(100, 40);
            btnOpenLotteryResult.TabIndex = 2;
            btnOpenLotteryResult.Text = "开奖结果";
            btnOpenLotteryResult.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnOpenLotteryResult.Click += btnOpenLotteryResult_Click;
            // 
            // btnLog
            // 
            btnLog.Cursor = Cursors.Hand;
            btnLog.Font = new Font("微软雅黑", 10F);
            btnLog.Location = new Point(5, 10);
            btnLog.MinimumSize = new Size(1, 1);
            btnLog.Name = "btnLog";
            btnLog.Size = new Size(100, 40);
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
        private Sunny.UI.UILabel lblContactList;
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
        private Sunny.UI.UIButton btnLog;
        private Sunny.UI.UIButton btnOpenLotteryResult;
        private Sunny.UI.UIButton btnClearData;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
    }
}
