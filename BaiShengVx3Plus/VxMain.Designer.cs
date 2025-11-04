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
            splitContainer = new Sunny.UI.UISplitContainer();
            pnlLeft = new Sunny.UI.UIPanel();
            dgvInsUsers = new Sunny.UI.UIDataGridView();
            pnlLeftTop = new Sunny.UI.UIPanel();
            btnRefresh = new Sunny.UI.UIButton();
            lblUserList = new Sunny.UI.UILabel();
            pnlRight = new Sunny.UI.UIPanel();
            tabControl = new Sunny.UI.UITabControl();
            tabPageMain = new TabPage();
            pnlMainContent = new Sunny.UI.UIPanel();
            grpUserInfo = new Sunny.UI.UIGroupBox();
            btnSubmit = new Sunny.UI.UIButton();
            lblBalance = new Sunny.UI.UILabel();
            uiLabel10 = new Sunny.UI.UILabel();
            numSeconds = new Sunny.UI.UIIntegerUpDown();
            uiLabel9 = new Sunny.UI.UILabel();
            lblCurrentTime = new Sunny.UI.UILabel();
            uiLabel8 = new Sunny.UI.UILabel();
            lblLastLoginTime = new Sunny.UI.UILabel();
            uiLabel7 = new Sunny.UI.UILabel();
            txtAddress = new Sunny.UI.UITextBox();
            uiLabel6 = new Sunny.UI.UILabel();
            txtPassword = new Sunny.UI.UITextBox();
            uiLabel5 = new Sunny.UI.UILabel();
            txtAccount = new Sunny.UI.UITextBox();
            uiLabel4 = new Sunny.UI.UILabel();
            lblUserName = new Sunny.UI.UILabel();
            uiLabel3 = new Sunny.UI.UILabel();
            lblUserId = new Sunny.UI.UILabel();
            uiLabel1 = new Sunny.UI.UILabel();
            pnlProgress = new Sunny.UI.UIPanel();
            progressBar = new Sunny.UI.UIProcessBar();
            lblProgressText = new Sunny.UI.UILabel();
            pnlButtons = new Sunny.UI.UIPanel();
            btnTransfer = new Sunny.UI.UIButton();
            btnRecharge = new Sunny.UI.UIButton();
            btnModifyPassword = new Sunny.UI.UIButton();
            btnSubscription = new Sunny.UI.UIButton();
            btnWeChatCard = new Sunny.UI.UIButton();
            btnSettings = new Sunny.UI.UIButton();
            btnAddUser = new Sunny.UI.UIButton();
            tabPageLog = new TabPage();
            txtLog = new Sunny.UI.UIRichTextBox();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            (splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvInsUsers).BeginInit();
            pnlLeftTop.SuspendLayout();
            pnlRight.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageMain.SuspendLayout();
            pnlMainContent.SuspendLayout();
            grpUserInfo.SuspendLayout();
            pnlProgress.SuspendLayout();
            pnlButtons.SuspendLayout();
            tabPageLog.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer
            // 
            splitContainer.Cursor = Cursors.VSplit;
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 35);
            splitContainer.MinimumSize = new Size(20, 20);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(pnlLeft);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(pnlRight);
            splitContainer.Size = new Size(980, 702);
            splitContainer.SplitterDistance = 199;
            splitContainer.SplitterWidth = 5;
            splitContainer.TabIndex = 0;
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(dgvInsUsers);
            pnlLeft.Controls.Add(pnlLeftTop);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Font = new Font("微软雅黑", 12F);
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Margin = new Padding(4, 5, 4, 5);
            pnlLeft.MinimumSize = new Size(1, 1);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(199, 702);
            pnlLeft.TabIndex = 0;
            pnlLeft.Text = null;
            pnlLeft.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvInsUsers
            // 
            dgvInsUsers.AllowUserToAddRows = false;
            dgvInsUsers.AllowUserToDeleteRows = false;
            dgvInsUsers.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            dgvInsUsers.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvInsUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInsUsers.BackgroundColor = Color.White;
            dgvInsUsers.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvInsUsers.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvInsUsers.ColumnHeadersHeight = 32;
            dgvInsUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Window;
            dataGridViewCellStyle3.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvInsUsers.DefaultCellStyle = dataGridViewCellStyle3;
            dgvInsUsers.Dock = DockStyle.Fill;
            dgvInsUsers.EnableHeadersVisualStyles = false;
            dgvInsUsers.Font = new Font("微软雅黑", 12F);
            dgvInsUsers.GridColor = Color.FromArgb(80, 160, 255);
            dgvInsUsers.Location = new Point(0, 50);
            dgvInsUsers.MultiSelect = false;
            dgvInsUsers.Name = "dgvInsUsers";
            dgvInsUsers.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle4.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.White;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgvInsUsers.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvInsUsers.RowHeadersVisible = false;
            dgvInsUsers.RowHeadersWidth = 51;
            dataGridViewCellStyle5.BackColor = Color.White;
            dataGridViewCellStyle5.Font = new Font("微软雅黑", 12F);
            dgvInsUsers.RowsDefaultCellStyle = dataGridViewCellStyle5;
            dgvInsUsers.RowTemplate.Height = 29;
            dgvInsUsers.SelectedIndex = -1;
            dgvInsUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInsUsers.Size = new Size(199, 652);
            dgvInsUsers.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvInsUsers.TabIndex = 1;
            dgvInsUsers.SelectionChanged += dgvInsUsers_SelectionChanged;
            // 
            // pnlLeftTop
            // 
            pnlLeftTop.Controls.Add(btnRefresh);
            pnlLeftTop.Controls.Add(lblUserList);
            pnlLeftTop.Dock = DockStyle.Top;
            pnlLeftTop.Font = new Font("微软雅黑", 12F);
            pnlLeftTop.Location = new Point(0, 0);
            pnlLeftTop.Margin = new Padding(4, 5, 4, 5);
            pnlLeftTop.MinimumSize = new Size(1, 1);
            pnlLeftTop.Name = "pnlLeftTop";
            pnlLeftTop.Size = new Size(199, 50);
            pnlLeftTop.TabIndex = 0;
            pnlLeftTop.Text = null;
            pnlLeftTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnRefresh
            // 
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Dock = DockStyle.Right;
            btnRefresh.Font = new Font("微软雅黑", 10F);
            btnRefresh.Location = new Point(139, 0);
            btnRefresh.MinimumSize = new Size(1, 1);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(60, 50);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "刷新";
            btnRefresh.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnRefresh.Click += btnRefresh_Click;
            // 
            // lblUserList
            // 
            lblUserList.Dock = DockStyle.Fill;
            lblUserList.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            lblUserList.ForeColor = Color.FromArgb(48, 48, 48);
            lblUserList.Location = new Point(0, 0);
            lblUserList.Name = "lblUserList";
            lblUserList.Size = new Size(199, 50);
            lblUserList.TabIndex = 0;
            lblUserList.Text = "用户列表";
            lblUserList.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(tabControl);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Font = new Font("微软雅黑", 12F);
            pnlRight.Location = new Point(0, 0);
            pnlRight.Margin = new Padding(4, 5, 4, 5);
            pnlRight.MinimumSize = new Size(1, 1);
            pnlRight.Name = "pnlRight";
            pnlRight.Padding = new Padding(5);
            pnlRight.Size = new Size(776, 702);
            pnlRight.TabIndex = 0;
            pnlRight.Text = null;
            pnlRight.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageMain);
            tabControl.Controls.Add(tabPageLog);
            tabControl.Dock = DockStyle.Fill;
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.Font = new Font("微软雅黑", 12F);
            tabControl.ItemSize = new Size(150, 40);
            tabControl.Location = new Point(5, 5);
            tabControl.MainPage = "";
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(766, 692);
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.TabIndex = 0;
            tabControl.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            tabControl.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // tabPageMain
            // 
            tabPageMain.BackColor = Color.FromArgb(243, 249, 255);
            tabPageMain.Controls.Add(pnlMainContent);
            tabPageMain.Controls.Add(pnlProgress);
            tabPageMain.Controls.Add(pnlButtons);
            tabPageMain.Location = new Point(0, 40);
            tabPageMain.Name = "tabPageMain";
            tabPageMain.Size = new Size(766, 652);
            tabPageMain.TabIndex = 0;
            tabPageMain.Text = "开发测试中";
            // 
            // pnlMainContent
            // 
            pnlMainContent.Controls.Add(grpUserInfo);
            pnlMainContent.Dock = DockStyle.Fill;
            pnlMainContent.Font = new Font("微软雅黑", 12F);
            pnlMainContent.Location = new Point(0, 60);
            pnlMainContent.Margin = new Padding(4, 5, 4, 5);
            pnlMainContent.MinimumSize = new Size(1, 1);
            pnlMainContent.Name = "pnlMainContent";
            pnlMainContent.Padding = new Padding(10);
            pnlMainContent.Size = new Size(766, 522);
            pnlMainContent.TabIndex = 2;
            pnlMainContent.Text = null;
            pnlMainContent.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // grpUserInfo
            // 
            grpUserInfo.Controls.Add(btnSubmit);
            grpUserInfo.Controls.Add(lblBalance);
            grpUserInfo.Controls.Add(uiLabel10);
            grpUserInfo.Controls.Add(numSeconds);
            grpUserInfo.Controls.Add(uiLabel9);
            grpUserInfo.Controls.Add(lblCurrentTime);
            grpUserInfo.Controls.Add(uiLabel8);
            grpUserInfo.Controls.Add(lblLastLoginTime);
            grpUserInfo.Controls.Add(uiLabel7);
            grpUserInfo.Controls.Add(txtAddress);
            grpUserInfo.Controls.Add(uiLabel6);
            grpUserInfo.Controls.Add(txtPassword);
            grpUserInfo.Controls.Add(uiLabel5);
            grpUserInfo.Controls.Add(txtAccount);
            grpUserInfo.Controls.Add(uiLabel4);
            grpUserInfo.Controls.Add(lblUserName);
            grpUserInfo.Controls.Add(uiLabel3);
            grpUserInfo.Controls.Add(lblUserId);
            grpUserInfo.Controls.Add(uiLabel1);
            grpUserInfo.Dock = DockStyle.Top;
            grpUserInfo.Font = new Font("微软雅黑", 12F);
            grpUserInfo.Location = new Point(10, 10);
            grpUserInfo.Margin = new Padding(4, 5, 4, 5);
            grpUserInfo.MinimumSize = new Size(1, 1);
            grpUserInfo.Name = "grpUserInfo";
            grpUserInfo.Padding = new Padding(10, 32, 10, 10);
            grpUserInfo.Size = new Size(746, 400);
            grpUserInfo.TabIndex = 0;
            grpUserInfo.Text = "InsUser - 真号VIP";
            grpUserInfo.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnSubmit
            // 
            btnSubmit.Cursor = Cursors.Hand;
            btnSubmit.Font = new Font("微软雅黑", 12F);
            btnSubmit.Location = new Point(320, 340);
            btnSubmit.MinimumSize = new Size(1, 1);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(100, 40);
            btnSubmit.TabIndex = 18;
            btnSubmit.Text = "提交";
            btnSubmit.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // lblBalance
            // 
            lblBalance.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            lblBalance.ForeColor = Color.FromArgb(220, 155, 40);
            lblBalance.Location = new Point(100, 290);
            lblBalance.Name = "lblBalance";
            lblBalance.Size = new Size(150, 30);
            lblBalance.TabIndex = 17;
            lblBalance.Text = "0.00";
            lblBalance.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // uiLabel10
            // 
            uiLabel10.Font = new Font("微软雅黑", 12F);
            uiLabel10.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel10.Location = new Point(30, 290);
            uiLabel10.Name = "uiLabel10";
            uiLabel10.Size = new Size(70, 30);
            uiLabel10.TabIndex = 16;
            uiLabel10.Text = "余额:";
            uiLabel10.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numSeconds
            // 
            numSeconds.Font = new Font("微软雅黑", 12F);
            numSeconds.Location = new Point(500, 240);
            numSeconds.Margin = new Padding(4, 5, 4, 5);
            numSeconds.Maximum = 999999;
            numSeconds.Minimum = 0;
            numSeconds.MinimumSize = new Size(100, 0);
            numSeconds.Name = "numSeconds";
            numSeconds.ShowText = false;
            numSeconds.Size = new Size(150, 30);
            numSeconds.TabIndex = 15;
            numSeconds.Text = "uiIntegerUpDown1";
            numSeconds.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiLabel9
            // 
            uiLabel9.Font = new Font("微软雅黑", 12F);
            uiLabel9.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel9.Location = new Point(430, 240);
            uiLabel9.Name = "uiLabel9";
            uiLabel9.Size = new Size(70, 30);
            uiLabel9.TabIndex = 14;
            uiLabel9.Text = "秒数:";
            uiLabel9.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCurrentTime
            // 
            lblCurrentTime.Font = new Font("微软雅黑", 12F);
            lblCurrentTime.ForeColor = Color.FromArgb(48, 48, 48);
            lblCurrentTime.Location = new Point(100, 240);
            lblCurrentTime.Name = "lblCurrentTime";
            lblCurrentTime.Size = new Size(250, 30);
            lblCurrentTime.TabIndex = 13;
            lblCurrentTime.Text = "2024-01-01 00:00:00";
            lblCurrentTime.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // uiLabel8
            // 
            uiLabel8.Font = new Font("微软雅黑", 12F);
            uiLabel8.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel8.Location = new Point(30, 240);
            uiLabel8.Name = "uiLabel8";
            uiLabel8.Size = new Size(70, 30);
            uiLabel8.TabIndex = 12;
            uiLabel8.Text = "当前:";
            uiLabel8.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLastLoginTime
            // 
            lblLastLoginTime.Font = new Font("微软雅黑", 12F);
            lblLastLoginTime.ForeColor = Color.FromArgb(48, 48, 48);
            lblLastLoginTime.Location = new Point(500, 190);
            lblLastLoginTime.Name = "lblLastLoginTime";
            lblLastLoginTime.Size = new Size(200, 30);
            lblLastLoginTime.TabIndex = 11;
            lblLastLoginTime.Text = "2024-01-01 00:00:00";
            lblLastLoginTime.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // uiLabel7
            // 
            uiLabel7.Font = new Font("微软雅黑", 12F);
            uiLabel7.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel7.Location = new Point(430, 190);
            uiLabel7.Name = "uiLabel7";
            uiLabel7.Size = new Size(70, 30);
            uiLabel7.TabIndex = 10;
            uiLabel7.Text = "上一次:";
            uiLabel7.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtAddress
            // 
            txtAddress.Font = new Font("微软雅黑", 12F);
            txtAddress.Location = new Point(100, 190);
            txtAddress.Margin = new Padding(4, 5, 4, 5);
            txtAddress.MinimumSize = new Size(1, 16);
            txtAddress.Name = "txtAddress";
            txtAddress.Padding = new Padding(5);
            txtAddress.ShowText = false;
            txtAddress.Size = new Size(250, 30);
            txtAddress.TabIndex = 9;
            txtAddress.TextAlignment = ContentAlignment.MiddleLeft;
            txtAddress.Watermark = "请输入地址";
            // 
            // uiLabel6
            // 
            uiLabel6.Font = new Font("微软雅黑", 12F);
            uiLabel6.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel6.Location = new Point(30, 190);
            uiLabel6.Name = "uiLabel6";
            uiLabel6.Size = new Size(70, 30);
            uiLabel6.TabIndex = 8;
            uiLabel6.Text = "地址:";
            uiLabel6.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("微软雅黑", 12F);
            txtPassword.Location = new Point(500, 140);
            txtPassword.Margin = new Padding(4, 5, 4, 5);
            txtPassword.MinimumSize = new Size(1, 16);
            txtPassword.Name = "txtPassword";
            txtPassword.Padding = new Padding(5);
            txtPassword.PasswordChar = '●';
            txtPassword.ShowText = false;
            txtPassword.Size = new Size(200, 30);
            txtPassword.TabIndex = 7;
            txtPassword.TextAlignment = ContentAlignment.MiddleLeft;
            txtPassword.Watermark = "";
            // 
            // uiLabel5
            // 
            uiLabel5.Font = new Font("微软雅黑", 12F);
            uiLabel5.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel5.Location = new Point(430, 140);
            uiLabel5.Name = "uiLabel5";
            uiLabel5.Size = new Size(70, 30);
            uiLabel5.TabIndex = 6;
            uiLabel5.Text = "密码:";
            uiLabel5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtAccount
            // 
            txtAccount.Font = new Font("微软雅黑", 12F);
            txtAccount.Location = new Point(100, 140);
            txtAccount.Margin = new Padding(4, 5, 4, 5);
            txtAccount.MinimumSize = new Size(1, 16);
            txtAccount.Name = "txtAccount";
            txtAccount.Padding = new Padding(5);
            txtAccount.ShowText = false;
            txtAccount.Size = new Size(250, 30);
            txtAccount.TabIndex = 5;
            txtAccount.TextAlignment = ContentAlignment.MiddleLeft;
            txtAccount.Watermark = "请输入账号";
            // 
            // uiLabel4
            // 
            uiLabel4.Font = new Font("微软雅黑", 12F);
            uiLabel4.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel4.Location = new Point(30, 140);
            uiLabel4.Name = "uiLabel4";
            uiLabel4.Size = new Size(70, 30);
            uiLabel4.TabIndex = 4;
            uiLabel4.Text = "账号:";
            uiLabel4.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUserName
            // 
            lblUserName.Font = new Font("微软雅黑", 12F);
            lblUserName.ForeColor = Color.FromArgb(48, 48, 48);
            lblUserName.Location = new Point(500, 90);
            lblUserName.Name = "lblUserName";
            lblUserName.Size = new Size(200, 30);
            lblUserName.TabIndex = 3;
            lblUserName.Text = "-";
            lblUserName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // uiLabel3
            // 
            uiLabel3.Font = new Font("微软雅黑", 12F);
            uiLabel3.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel3.Location = new Point(430, 90);
            uiLabel3.Name = "uiLabel3";
            uiLabel3.Size = new Size(70, 30);
            uiLabel3.TabIndex = 2;
            uiLabel3.Text = "名称:";
            uiLabel3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUserId
            // 
            lblUserId.Font = new Font("微软雅黑", 12F);
            lblUserId.ForeColor = Color.FromArgb(48, 48, 48);
            lblUserId.Location = new Point(100, 90);
            lblUserId.Name = "lblUserId";
            lblUserId.Size = new Size(250, 30);
            lblUserId.TabIndex = 1;
            lblUserId.Text = "-";
            lblUserId.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("微软雅黑", 12F);
            uiLabel1.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel1.Location = new Point(30, 90);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(70, 30);
            uiLabel1.TabIndex = 0;
            uiLabel1.Text = "ID:";
            uiLabel1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlProgress
            // 
            pnlProgress.Controls.Add(progressBar);
            pnlProgress.Controls.Add(lblProgressText);
            pnlProgress.Dock = DockStyle.Bottom;
            pnlProgress.Font = new Font("微软雅黑", 12F);
            pnlProgress.Location = new Point(0, 582);
            pnlProgress.Margin = new Padding(4, 5, 4, 5);
            pnlProgress.MinimumSize = new Size(1, 1);
            pnlProgress.Name = "pnlProgress";
            pnlProgress.Padding = new Padding(10);
            pnlProgress.Size = new Size(766, 70);
            pnlProgress.TabIndex = 1;
            pnlProgress.Text = null;
            pnlProgress.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // progressBar
            // 
            progressBar.Dock = DockStyle.Fill;
            progressBar.FillColor = Color.FromArgb(235, 243, 255);
            progressBar.Font = new Font("微软雅黑", 12F);
            progressBar.Location = new Point(10, 40);
            progressBar.MinimumSize = new Size(70, 1);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(746, 20);
            progressBar.TabIndex = 1;
            progressBar.Text = "uiProcessBar1";
            // 
            // lblProgressText
            // 
            lblProgressText.Dock = DockStyle.Top;
            lblProgressText.Font = new Font("微软雅黑", 10F);
            lblProgressText.ForeColor = Color.FromArgb(48, 48, 48);
            lblProgressText.Location = new Point(10, 10);
            lblProgressText.Name = "lblProgressText";
            lblProgressText.Size = new Size(746, 30);
            lblProgressText.TabIndex = 0;
            lblProgressText.Text = "上一次:  123456789   当前时间: 13:19:23  剩  3000";
            lblProgressText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnTransfer);
            pnlButtons.Controls.Add(btnRecharge);
            pnlButtons.Controls.Add(btnModifyPassword);
            pnlButtons.Controls.Add(btnSubscription);
            pnlButtons.Controls.Add(btnWeChatCard);
            pnlButtons.Controls.Add(btnSettings);
            pnlButtons.Controls.Add(btnAddUser);
            pnlButtons.Dock = DockStyle.Top;
            pnlButtons.Font = new Font("微软雅黑", 12F);
            pnlButtons.Location = new Point(0, 0);
            pnlButtons.Margin = new Padding(4, 5, 4, 5);
            pnlButtons.MinimumSize = new Size(1, 1);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Padding = new Padding(5);
            pnlButtons.Size = new Size(766, 60);
            pnlButtons.TabIndex = 0;
            pnlButtons.Text = null;
            pnlButtons.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnTransfer
            // 
            btnTransfer.Cursor = Cursors.Hand;
            btnTransfer.Font = new Font("微软雅黑", 10F);
            btnTransfer.Location = new Point(545, 10);
            btnTransfer.MinimumSize = new Size(1, 1);
            btnTransfer.Name = "btnTransfer";
            btnTransfer.Size = new Size(80, 40);
            btnTransfer.TabIndex = 6;
            btnTransfer.Text = "转分";
            btnTransfer.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnTransfer.Click += btnTransfer_Click;
            // 
            // btnRecharge
            // 
            btnRecharge.Cursor = Cursors.Hand;
            btnRecharge.Font = new Font("微软雅黑", 10F);
            btnRecharge.Location = new Point(455, 10);
            btnRecharge.MinimumSize = new Size(1, 1);
            btnRecharge.Name = "btnRecharge";
            btnRecharge.Size = new Size(80, 40);
            btnRecharge.TabIndex = 5;
            btnRecharge.Text = "充值";
            btnRecharge.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnRecharge.Click += btnRecharge_Click;
            // 
            // btnModifyPassword
            // 
            btnModifyPassword.Cursor = Cursors.Hand;
            btnModifyPassword.Font = new Font("微软雅黑", 10F);
            btnModifyPassword.Location = new Point(345, 10);
            btnModifyPassword.MinimumSize = new Size(1, 1);
            btnModifyPassword.Name = "btnModifyPassword";
            btnModifyPassword.Size = new Size(100, 40);
            btnModifyPassword.TabIndex = 4;
            btnModifyPassword.Text = "修改密码";
            btnModifyPassword.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnModifyPassword.Click += btnModifyPassword_Click;
            // 
            // btnSubscription
            // 
            btnSubscription.Cursor = Cursors.Hand;
            btnSubscription.Font = new Font("微软雅黑", 10F);
            btnSubscription.Location = new Point(235, 10);
            btnSubscription.MinimumSize = new Size(1, 1);
            btnSubscription.Name = "btnSubscription";
            btnSubscription.Size = new Size(100, 40);
            btnSubscription.TabIndex = 3;
            btnSubscription.Text = "订单管理";
            btnSubscription.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSubscription.Click += btnSubscription_Click;
            // 
            // btnWeChatCard
            // 
            btnWeChatCard.Cursor = Cursors.Hand;
            btnWeChatCard.Font = new Font("微软雅黑", 10F);
            btnWeChatCard.Location = new Point(95, 10);
            btnWeChatCard.MinimumSize = new Size(1, 1);
            btnWeChatCard.Name = "btnWeChatCard";
            btnWeChatCard.Size = new Size(130, 40);
            btnWeChatCard.TabIndex = 2;
            btnWeChatCard.Text = "微信数据卡管理";
            btnWeChatCard.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnWeChatCard.Click += btnWeChatCard_Click;
            // 
            // btnSettings
            // 
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.Dock = DockStyle.Right;
            btnSettings.Font = new Font("微软雅黑", 10F);
            btnSettings.Location = new Point(661, 5);
            btnSettings.MinimumSize = new Size(1, 1);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(100, 50);
            btnSettings.TabIndex = 1;
            btnSettings.Text = "设置";
            btnSettings.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSettings.Click += btnSettings_Click;
            // 
            // btnAddUser
            // 
            btnAddUser.Cursor = Cursors.Hand;
            btnAddUser.Font = new Font("微软雅黑", 10F);
            btnAddUser.Location = new Point(5, 10);
            btnAddUser.MinimumSize = new Size(1, 1);
            btnAddUser.Name = "btnAddUser";
            btnAddUser.Size = new Size(80, 40);
            btnAddUser.TabIndex = 0;
            btnAddUser.Text = "添加";
            btnAddUser.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnAddUser.Click += btnAddUser_Click;
            // 
            // tabPageLog
            // 
            tabPageLog.BackColor = Color.FromArgb(243, 249, 255);
            tabPageLog.Controls.Add(txtLog);
            tabPageLog.Location = new Point(0, 40);
            tabPageLog.Name = "tabPageLog";
            tabPageLog.Size = new Size(766, 652);
            tabPageLog.TabIndex = 1;
            tabPageLog.Text = "日志";
            // 
            // txtLog
            // 
            txtLog.Dock = DockStyle.Fill;
            txtLog.FillColor = Color.White;
            txtLog.Font = new Font("Consolas", 10F);
            txtLog.Location = new Point(0, 0);
            txtLog.Margin = new Padding(4, 5, 4, 5);
            txtLog.MinimumSize = new Size(1, 1);
            txtLog.Name = "txtLog";
            txtLog.Padding = new Padding(10);
            txtLog.ReadOnly = true;
            txtLog.ShowText = false;
            txtLog.Size = new Size(766, 652);
            txtLog.TabIndex = 0;
            txtLog.Text = "系统日志...\n";
            txtLog.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // statusStrip
            // 
            statusStrip.BackColor = Color.FromArgb(243, 249, 255);
            statusStrip.Font = new Font("微软雅黑", 10F);
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 737);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(980, 25);
            statusStrip.TabIndex = 1;
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
            ClientSize = new Size(980, 762);
            Controls.Add(splitContainer);
            Controls.Add(statusStrip);
            Name = "VxMain";
            Text = "百胜VX3Plus - 管理系统";
            ZoomScaleRect = new Rectangle(15, 15, 980, 762);
            Load += VxMain_Load;
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            (splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvInsUsers).EndInit();
            pnlLeftTop.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabPageMain.ResumeLayout(false);
            pnlMainContent.ResumeLayout(false);
            grpUserInfo.ResumeLayout(false);
            pnlProgress.ResumeLayout(false);
            pnlButtons.ResumeLayout(false);
            tabPageLog.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Sunny.UI.UISplitContainer splitContainer;
        private Sunny.UI.UIPanel pnlLeft;
        private Sunny.UI.UIDataGridView dgvInsUsers;
        private Sunny.UI.UIPanel pnlLeftTop;
        private Sunny.UI.UIButton btnRefresh;
        private Sunny.UI.UILabel lblUserList;
        private Sunny.UI.UIPanel pnlRight;
        private Sunny.UI.UITabControl tabControl;
        private TabPage tabPageMain;
        private TabPage tabPageLog;
        private Sunny.UI.UIRichTextBox txtLog;
        private Sunny.UI.UIPanel pnlButtons;
        private Sunny.UI.UIButton btnSettings;
        private Sunny.UI.UIButton btnAddUser;
        private Sunny.UI.UIButton btnWeChatCard;
        private Sunny.UI.UIButton btnSubscription;
        private Sunny.UI.UIButton btnModifyPassword;
        private Sunny.UI.UIButton btnRecharge;
        private Sunny.UI.UIButton btnTransfer;
        private Sunny.UI.UIPanel pnlProgress;
        private Sunny.UI.UIProcessBar progressBar;
        private Sunny.UI.UILabel lblProgressText;
        private Sunny.UI.UIPanel pnlMainContent;
        private Sunny.UI.UIGroupBox grpUserInfo;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel lblUserId;
        private Sunny.UI.UILabel lblUserName;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UITextBox txtAccount;
        private Sunny.UI.UILabel uiLabel4;
        private Sunny.UI.UITextBox txtPassword;
        private Sunny.UI.UILabel uiLabel5;
        private Sunny.UI.UITextBox txtAddress;
        private Sunny.UI.UILabel uiLabel6;
        private Sunny.UI.UILabel lblLastLoginTime;
        private Sunny.UI.UILabel uiLabel7;
        private Sunny.UI.UILabel lblCurrentTime;
        private Sunny.UI.UILabel uiLabel8;
        private Sunny.UI.UIIntegerUpDown numSeconds;
        private Sunny.UI.UILabel uiLabel9;
        private Sunny.UI.UILabel lblBalance;
        private Sunny.UI.UILabel uiLabel10;
        private Sunny.UI.UIButton btnSubmit;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
    }
}
