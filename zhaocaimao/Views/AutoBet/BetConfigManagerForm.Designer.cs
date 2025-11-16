namespace zhaocaimao.Views.AutoBet
{
    partial class BetConfigManagerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
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
            splitContainer = new Sunny.UI.UISplitContainer();
            pnlLeft = new Sunny.UI.UIPanel();
            dgvConfigs = new Sunny.UI.UIDataGridView();
            pnlLeftTop = new Sunny.UI.UIPanel();
            btnDelete = new Sunny.UI.UIButton();
            btnEdit = new Sunny.UI.UIButton();
            btnAdd = new Sunny.UI.UIButton();
            pnlRight = new Sunny.UI.UIPanel();
            tabControl = new Sunny.UI.UITabControl();
            tabBasic = new TabPage();
            pnlBasicContent = new Sunny.UI.UIPanel();
            btnTestConnection = new Sunny.UI.UIButton();
            chkShowBrowser = new Sunny.UI.UICheckBox();
            chkAutoLogin = new Sunny.UI.UICheckBox();
            chkEnabled = new Sunny.UI.UICheckBox();
            txtMaxBetAmount = new Sunny.UI.UITextBox();
            lblMaxBetAmount = new Sunny.UI.UILabel();
            txtMinBetAmount = new Sunny.UI.UITextBox();
            lblMinBetAmount = new Sunny.UI.UILabel();
            txtPassword = new Sunny.UI.UITextBox();
            lblPassword = new Sunny.UI.UILabel();
            txtUsername = new Sunny.UI.UITextBox();
            lblUsername = new Sunny.UI.UILabel();
            txtPlatformUrl = new Sunny.UI.UITextBox();
            lblPlatformUrl = new Sunny.UI.UILabel();
            cbxPlatform = new Sunny.UI.UIComboBox();
            lblPlatform = new Sunny.UI.UILabel();
            txtConfigName = new Sunny.UI.UITextBox();
            lblConfigName = new Sunny.UI.UILabel();
            tabAdvanced = new TabPage();
            pnlAdvancedContent = new Sunny.UI.UIPanel();
            pnlCommandPanel = new Sunny.UI.UIPanel();
            lblCommandPanel = new Sunny.UI.UILabel();
            pnlQuickButtons = new Sunny.UI.UIPanel();
            btnBetCommand = new Sunny.UI.UIButton();
            btnGetQuotaCommand = new Sunny.UI.UIButton();
            btnGetCookieCommand = new Sunny.UI.UIButton();
            lblCommandInput = new Sunny.UI.UILabel();
            txtCommand = new Sunny.UI.UITextBox();
            btnSendCommand = new Sunny.UI.UIButton();
            lblCommandResult = new Sunny.UI.UILabel();
            txtCommandResult = new Sunny.UI.UITextBox();
            txtCookies = new Sunny.UI.UITextBox();
            lblCookies = new Sunny.UI.UILabel();
            txtNotes = new Sunny.UI.UITextBox();
            lblNotes = new Sunny.UI.UILabel();
            tabRecords = new TabPage();
            pnlRecordsContent = new Sunny.UI.UIPanel();
            tbxRecordsDetailed = new Sunny.UI.UIRichTextBox();
            dgvRecords = new Sunny.UI.UIDataGridView();
            pnlRecordsTop = new Sunny.UI.UIPanel();
            btnRefreshRecords = new Sunny.UI.UIButton();
            dtpEndDate = new DateTimePicker();
            lblTo = new Sunny.UI.UILabel();
            dtpStartDate = new DateTimePicker();
            lblDateRange = new Sunny.UI.UILabel();
            pnlRightTop = new Sunny.UI.UIPanel();
            btnClose = new Sunny.UI.UIButton();
            btnStopBrowser = new Sunny.UI.UIButton();
            btnStartBrowser = new Sunny.UI.UIButton();
            btnSave = new Sunny.UI.UIButton();
            lblStatus = new Sunny.UI.UILabel();
            (splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConfigs).BeginInit();
            pnlLeftTop.SuspendLayout();
            pnlRight.SuspendLayout();
            tabControl.SuspendLayout();
            tabBasic.SuspendLayout();
            pnlBasicContent.SuspendLayout();
            tabAdvanced.SuspendLayout();
            pnlAdvancedContent.SuspendLayout();
            pnlCommandPanel.SuspendLayout();
            pnlQuickButtons.SuspendLayout();
            tabRecords.SuspendLayout();
            pnlRecordsContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).BeginInit();
            pnlRecordsTop.SuspendLayout();
            pnlRightTop.SuspendLayout();
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
            splitContainer.Size = new Size(1400, 765);
            splitContainer.SplitterDistance = 400;
            splitContainer.SplitterWidth = 6;
            splitContainer.TabIndex = 0;
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(dgvConfigs);
            pnlLeft.Controls.Add(pnlLeftTop);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Font = new Font("微软雅黑", 12F);
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Margin = new Padding(4, 5, 4, 5);
            pnlLeft.MinimumSize = new Size(1, 1);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(400, 765);
            pnlLeft.TabIndex = 0;
            pnlLeft.Text = "配置列表";
            pnlLeft.TextAlignment = ContentAlignment.TopCenter;
            // 
            // dgvConfigs
            // 
            dgvConfigs.AllowUserToAddRows = false;
            dgvConfigs.AllowUserToDeleteRows = false;
            dgvConfigs.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            dgvConfigs.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvConfigs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvConfigs.BackgroundColor = Color.White;
            dgvConfigs.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvConfigs.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvConfigs.ColumnHeadersHeight = 32;
            dgvConfigs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Window;
            dataGridViewCellStyle3.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(155, 200, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvConfigs.DefaultCellStyle = dataGridViewCellStyle3;
            dgvConfigs.Dock = DockStyle.Fill;
            dgvConfigs.EnableHeadersVisualStyles = false;
            dgvConfigs.Font = new Font("宋体", 12F);
            dgvConfigs.GridColor = Color.FromArgb(80, 160, 255);
            dgvConfigs.Location = new Point(0, 40);
            dgvConfigs.MultiSelect = false;
            dgvConfigs.Name = "dgvConfigs";
            dgvConfigs.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle4.Font = new Font("宋体", 12F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.White;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgvConfigs.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvConfigs.RowHeadersVisible = false;
            dgvConfigs.RowHeadersWidth = 51;
            dataGridViewCellStyle5.BackColor = Color.White;
            dataGridViewCellStyle5.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvConfigs.RowsDefaultCellStyle = dataGridViewCellStyle5;
            dgvConfigs.RowTemplate.Height = 29;
            dgvConfigs.SelectedIndex = -1;
            dgvConfigs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvConfigs.Size = new Size(400, 725);
            dgvConfigs.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvConfigs.TabIndex = 1;
            dgvConfigs.SelectionChanged += dgvConfigs_SelectionChanged;
            // 
            // pnlLeftTop
            // 
            pnlLeftTop.Controls.Add(btnDelete);
            pnlLeftTop.Controls.Add(btnEdit);
            pnlLeftTop.Controls.Add(btnAdd);
            pnlLeftTop.Dock = DockStyle.Top;
            pnlLeftTop.Font = new Font("微软雅黑", 12F);
            pnlLeftTop.Location = new Point(0, 0);
            pnlLeftTop.Margin = new Padding(4, 5, 4, 5);
            pnlLeftTop.MinimumSize = new Size(1, 1);
            pnlLeftTop.Name = "pnlLeftTop";
            pnlLeftTop.Size = new Size(400, 40);
            pnlLeftTop.TabIndex = 0;
            pnlLeftTop.Text = null;
            pnlLeftTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnDelete
            // 
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Font = new Font("微软雅黑", 9F);
            btnDelete.Location = new Point(220, 5);
            btnDelete.MinimumSize = new Size(1, 1);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(100, 30);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "删除";
            btnDelete.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnDelete.Click += btnDelete_Click;
            // 
            // btnEdit
            // 
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Font = new Font("微软雅黑", 9F);
            btnEdit.Location = new Point(115, 5);
            btnEdit.MinimumSize = new Size(1, 1);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(100, 30);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "编辑";
            btnEdit.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnEdit.Click += btnEdit_Click;
            // 
            // btnAdd
            // 
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Font = new Font("微软雅黑", 9F);
            btnAdd.Location = new Point(10, 5);
            btnAdd.MinimumSize = new Size(1, 1);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(100, 30);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "新增";
            btnAdd.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnAdd.Click += btnAdd_Click;
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(tabControl);
            pnlRight.Controls.Add(pnlRightTop);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Font = new Font("微软雅黑", 12F);
            pnlRight.Location = new Point(0, 0);
            pnlRight.Margin = new Padding(4, 5, 4, 5);
            pnlRight.MinimumSize = new Size(1, 1);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(994, 765);
            pnlRight.TabIndex = 0;
            pnlRight.Text = "配置详情";
            pnlRight.TextAlignment = ContentAlignment.TopCenter;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabBasic);
            tabControl.Controls.Add(tabAdvanced);
            tabControl.Controls.Add(tabRecords);
            tabControl.Dock = DockStyle.Fill;
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.Font = new Font("微软雅黑", 10F);
            tabControl.ItemSize = new Size(150, 40);
            tabControl.Location = new Point(0, 50);
            tabControl.MainPage = "";
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(994, 715);
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.TabIndex = 1;
            tabControl.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            tabControl.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // tabBasic
            // 
            tabBasic.BackColor = Color.White;
            tabBasic.Controls.Add(pnlBasicContent);
            tabBasic.Location = new Point(0, 40);
            tabBasic.Name = "tabBasic";
            tabBasic.Size = new Size(994, 675);
            tabBasic.TabIndex = 0;
            tabBasic.Text = "基本设置";
            // 
            // pnlBasicContent
            // 
            pnlBasicContent.Controls.Add(btnTestConnection);
            pnlBasicContent.Controls.Add(chkShowBrowser);
            pnlBasicContent.Controls.Add(chkAutoLogin);
            pnlBasicContent.Controls.Add(chkEnabled);
            pnlBasicContent.Controls.Add(txtMaxBetAmount);
            pnlBasicContent.Controls.Add(lblMaxBetAmount);
            pnlBasicContent.Controls.Add(txtMinBetAmount);
            pnlBasicContent.Controls.Add(lblMinBetAmount);
            pnlBasicContent.Controls.Add(txtPassword);
            pnlBasicContent.Controls.Add(lblPassword);
            pnlBasicContent.Controls.Add(txtUsername);
            pnlBasicContent.Controls.Add(lblUsername);
            pnlBasicContent.Controls.Add(txtPlatformUrl);
            pnlBasicContent.Controls.Add(lblPlatformUrl);
            pnlBasicContent.Controls.Add(cbxPlatform);
            pnlBasicContent.Controls.Add(lblPlatform);
            pnlBasicContent.Controls.Add(txtConfigName);
            pnlBasicContent.Controls.Add(lblConfigName);
            pnlBasicContent.Dock = DockStyle.Fill;
            pnlBasicContent.Font = new Font("微软雅黑", 12F);
            pnlBasicContent.Location = new Point(0, 0);
            pnlBasicContent.Margin = new Padding(4, 5, 4, 5);
            pnlBasicContent.MinimumSize = new Size(1, 1);
            pnlBasicContent.Name = "pnlBasicContent";
            pnlBasicContent.Size = new Size(994, 675);
            pnlBasicContent.TabIndex = 0;
            pnlBasicContent.Text = null;
            pnlBasicContent.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnTestConnection
            // 
            btnTestConnection.Cursor = Cursors.Hand;
            btnTestConnection.Font = new Font("微软雅黑", 10F);
            btnTestConnection.Location = new Point(150, 420);
            btnTestConnection.MinimumSize = new Size(1, 1);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new Size(150, 35);
            btnTestConnection.TabIndex = 17;
            btnTestConnection.Text = "测试连接";
            btnTestConnection.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnTestConnection.Click += btnTestConnection_Click;
            // 
            // chkShowBrowser
            // 
            chkShowBrowser.Cursor = Cursors.Hand;
            chkShowBrowser.Font = new Font("微软雅黑", 10F);
            chkShowBrowser.ForeColor = Color.FromArgb(48, 48, 48);
            chkShowBrowser.Location = new Point(150, 370);
            chkShowBrowser.MinimumSize = new Size(1, 1);
            chkShowBrowser.Name = "chkShowBrowser";
            chkShowBrowser.Size = new Size(200, 29);
            chkShowBrowser.TabIndex = 16;
            chkShowBrowser.Text = "显示浏览器窗口";
            // 
            // chkAutoLogin
            // 
            chkAutoLogin.Checked = true;
            chkAutoLogin.Cursor = Cursors.Hand;
            chkAutoLogin.Font = new Font("微软雅黑", 10F);
            chkAutoLogin.ForeColor = Color.FromArgb(48, 48, 48);
            chkAutoLogin.Location = new Point(380, 335);
            chkAutoLogin.MinimumSize = new Size(1, 1);
            chkAutoLogin.Name = "chkAutoLogin";
            chkAutoLogin.Size = new Size(150, 29);
            chkAutoLogin.TabIndex = 15;
            chkAutoLogin.Text = "自动登录";
            // 
            // chkEnabled
            // 
            chkEnabled.Checked = true;
            chkEnabled.Cursor = Cursors.Hand;
            chkEnabled.Font = new Font("微软雅黑", 10F);
            chkEnabled.ForeColor = Color.FromArgb(48, 48, 48);
            chkEnabled.Location = new Point(150, 335);
            chkEnabled.MinimumSize = new Size(1, 1);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new Size(150, 29);
            chkEnabled.TabIndex = 14;
            chkEnabled.Text = "启用此配置";
            // 
            // txtMaxBetAmount
            // 
            txtMaxBetAmount.DoubleValue = 10000D;
            txtMaxBetAmount.Font = new Font("微软雅黑", 10F);
            txtMaxBetAmount.IntValue = 10000;
            txtMaxBetAmount.Location = new Point(480, 285);
            txtMaxBetAmount.Margin = new Padding(4, 5, 4, 5);
            txtMaxBetAmount.MinimumSize = new Size(1, 16);
            txtMaxBetAmount.Name = "txtMaxBetAmount";
            txtMaxBetAmount.Padding = new Padding(5);
            txtMaxBetAmount.ShowText = false;
            txtMaxBetAmount.Size = new Size(200, 30);
            txtMaxBetAmount.TabIndex = 13;
            txtMaxBetAmount.Text = "10000";
            txtMaxBetAmount.TextAlignment = ContentAlignment.MiddleLeft;
            txtMaxBetAmount.Watermark = "";
            // 
            // lblMaxBetAmount
            // 
            lblMaxBetAmount.Font = new Font("微软雅黑", 10F);
            lblMaxBetAmount.ForeColor = Color.FromArgb(48, 48, 48);
            lblMaxBetAmount.Location = new Point(370, 285);
            lblMaxBetAmount.Name = "lblMaxBetAmount";
            lblMaxBetAmount.Size = new Size(100, 30);
            lblMaxBetAmount.TabIndex = 12;
            lblMaxBetAmount.Text = "最大金额:";
            lblMaxBetAmount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtMinBetAmount
            // 
            txtMinBetAmount.DoubleValue = 1D;
            txtMinBetAmount.Font = new Font("微软雅黑", 10F);
            txtMinBetAmount.IntValue = 1;
            txtMinBetAmount.Location = new Point(150, 285);
            txtMinBetAmount.Margin = new Padding(4, 5, 4, 5);
            txtMinBetAmount.MinimumSize = new Size(1, 16);
            txtMinBetAmount.Name = "txtMinBetAmount";
            txtMinBetAmount.Padding = new Padding(5);
            txtMinBetAmount.ShowText = false;
            txtMinBetAmount.Size = new Size(200, 30);
            txtMinBetAmount.TabIndex = 11;
            txtMinBetAmount.Text = "1";
            txtMinBetAmount.TextAlignment = ContentAlignment.MiddleLeft;
            txtMinBetAmount.Watermark = "";
            // 
            // lblMinBetAmount
            // 
            lblMinBetAmount.Font = new Font("微软雅黑", 10F);
            lblMinBetAmount.ForeColor = Color.FromArgb(48, 48, 48);
            lblMinBetAmount.Location = new Point(40, 285);
            lblMinBetAmount.Name = "lblMinBetAmount";
            lblMinBetAmount.Size = new Size(100, 30);
            lblMinBetAmount.TabIndex = 10;
            lblMinBetAmount.Text = "最小金额:";
            lblMinBetAmount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("微软雅黑", 10F);
            txtPassword.Location = new Point(150, 235);
            txtPassword.Margin = new Padding(4, 5, 4, 5);
            txtPassword.MinimumSize = new Size(1, 16);
            txtPassword.Name = "txtPassword";
            txtPassword.Padding = new Padding(5);
            txtPassword.PasswordChar = '*';
            txtPassword.ShowText = false;
            txtPassword.Size = new Size(530, 30);
            txtPassword.TabIndex = 9;
            txtPassword.TextAlignment = ContentAlignment.MiddleLeft;
            txtPassword.Watermark = "投注账号密码";
            // 
            // lblPassword
            // 
            lblPassword.Font = new Font("微软雅黑", 10F);
            lblPassword.ForeColor = Color.FromArgb(48, 48, 48);
            lblPassword.Location = new Point(40, 235);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(100, 30);
            lblPassword.TabIndex = 8;
            lblPassword.Text = "密码:";
            lblPassword.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtUsername
            // 
            txtUsername.Font = new Font("微软雅黑", 10F);
            txtUsername.Location = new Point(150, 185);
            txtUsername.Margin = new Padding(4, 5, 4, 5);
            txtUsername.MinimumSize = new Size(1, 16);
            txtUsername.Name = "txtUsername";
            txtUsername.Padding = new Padding(5);
            txtUsername.ShowText = false;
            txtUsername.Size = new Size(530, 30);
            txtUsername.TabIndex = 7;
            txtUsername.TextAlignment = ContentAlignment.MiddleLeft;
            txtUsername.Watermark = "投注账号";
            // 
            // lblUsername
            // 
            lblUsername.Font = new Font("微软雅黑", 10F);
            lblUsername.ForeColor = Color.FromArgb(48, 48, 48);
            lblUsername.Location = new Point(40, 185);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(100, 30);
            lblUsername.TabIndex = 6;
            lblUsername.Text = "账号:";
            lblUsername.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtPlatformUrl
            // 
            txtPlatformUrl.Font = new Font("微软雅黑", 10F);
            txtPlatformUrl.Location = new Point(150, 135);
            txtPlatformUrl.Margin = new Padding(4, 5, 4, 5);
            txtPlatformUrl.MinimumSize = new Size(1, 16);
            txtPlatformUrl.Name = "txtPlatformUrl";
            txtPlatformUrl.Padding = new Padding(5);
            txtPlatformUrl.ShowText = false;
            txtPlatformUrl.Size = new Size(530, 30);
            txtPlatformUrl.TabIndex = 5;
            txtPlatformUrl.TextAlignment = ContentAlignment.MiddleLeft;
            txtPlatformUrl.Watermark = "https://www.example.com";
            // 
            // lblPlatformUrl
            // 
            lblPlatformUrl.Font = new Font("微软雅黑", 10F);
            lblPlatformUrl.ForeColor = Color.FromArgb(48, 48, 48);
            lblPlatformUrl.Location = new Point(40, 135);
            lblPlatformUrl.Name = "lblPlatformUrl";
            lblPlatformUrl.Size = new Size(100, 30);
            lblPlatformUrl.TabIndex = 4;
            lblPlatformUrl.Text = "平台URL:";
            lblPlatformUrl.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cbxPlatform
            // 
            cbxPlatform.DataSource = null;
            cbxPlatform.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            cbxPlatform.FillColor = Color.White;
            cbxPlatform.Font = new Font("微软雅黑", 10F);
            cbxPlatform.ItemHoverColor = Color.FromArgb(155, 200, 255);
            cbxPlatform.Items.AddRange(new object[] { "云顶", "海峡", "红海", "通宝" });
            cbxPlatform.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            cbxPlatform.Location = new Point(150, 85);
            cbxPlatform.Margin = new Padding(4, 5, 4, 5);
            cbxPlatform.MinimumSize = new Size(63, 0);
            cbxPlatform.Name = "cbxPlatform";
            cbxPlatform.Padding = new Padding(0, 0, 30, 2);
            cbxPlatform.Size = new Size(530, 30);
            cbxPlatform.SymbolSize = 24;
            cbxPlatform.TabIndex = 3;
            cbxPlatform.TextAlignment = ContentAlignment.MiddleLeft;
            cbxPlatform.Watermark = "";
            cbxPlatform.SelectedIndexChanged += cbxPlatform_SelectedIndexChanged;
            // 
            // lblPlatform
            // 
            lblPlatform.Font = new Font("微软雅黑", 10F);
            lblPlatform.ForeColor = Color.FromArgb(48, 48, 48);
            lblPlatform.Location = new Point(40, 85);
            lblPlatform.Name = "lblPlatform";
            lblPlatform.Size = new Size(100, 30);
            lblPlatform.TabIndex = 2;
            lblPlatform.Text = "平台:";
            lblPlatform.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtConfigName
            // 
            txtConfigName.Font = new Font("微软雅黑", 10F);
            txtConfigName.Location = new Point(150, 35);
            txtConfigName.Margin = new Padding(4, 5, 4, 5);
            txtConfigName.MinimumSize = new Size(1, 16);
            txtConfigName.Name = "txtConfigName";
            txtConfigName.Padding = new Padding(5);
            txtConfigName.ShowText = false;
            txtConfigName.Size = new Size(530, 30);
            txtConfigName.TabIndex = 1;
            txtConfigName.TextAlignment = ContentAlignment.MiddleLeft;
            txtConfigName.Watermark = "例如：云�?8-账号1";
            // 
            // lblConfigName
            // 
            lblConfigName.Font = new Font("微软雅黑", 10F);
            lblConfigName.ForeColor = Color.FromArgb(48, 48, 48);
            lblConfigName.Location = new Point(40, 35);
            lblConfigName.Name = "lblConfigName";
            lblConfigName.Size = new Size(100, 30);
            lblConfigName.TabIndex = 0;
            lblConfigName.Text = "配置名称:";
            lblConfigName.TextAlign = ContentAlignment.MiddleRight;
            // 
            // tabAdvanced
            // 
            tabAdvanced.BackColor = Color.White;
            tabAdvanced.Controls.Add(pnlAdvancedContent);
            tabAdvanced.Location = new Point(0, 40);
            tabAdvanced.Name = "tabAdvanced";
            tabAdvanced.Size = new Size(200, 60);
            tabAdvanced.TabIndex = 1;
            tabAdvanced.Text = "高级设置";
            // 
            // pnlAdvancedContent
            // 
            pnlAdvancedContent.Controls.Add(pnlCommandPanel);
            pnlAdvancedContent.Controls.Add(txtCookies);
            pnlAdvancedContent.Controls.Add(lblCookies);
            pnlAdvancedContent.Controls.Add(txtNotes);
            pnlAdvancedContent.Controls.Add(lblNotes);
            pnlAdvancedContent.Dock = DockStyle.Fill;
            pnlAdvancedContent.Font = new Font("微软雅黑", 12F);
            pnlAdvancedContent.Location = new Point(0, 0);
            pnlAdvancedContent.Margin = new Padding(4, 5, 4, 5);
            pnlAdvancedContent.MinimumSize = new Size(1, 1);
            pnlAdvancedContent.Name = "pnlAdvancedContent";
            pnlAdvancedContent.Size = new Size(200, 60);
            pnlAdvancedContent.TabIndex = 0;
            pnlAdvancedContent.Text = null;
            pnlAdvancedContent.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // pnlCommandPanel
            // 
            pnlCommandPanel.Controls.Add(lblCommandPanel);
            pnlCommandPanel.Controls.Add(pnlQuickButtons);
            pnlCommandPanel.Controls.Add(lblCommandInput);
            pnlCommandPanel.Controls.Add(txtCommand);
            pnlCommandPanel.Controls.Add(btnSendCommand);
            pnlCommandPanel.Controls.Add(lblCommandResult);
            pnlCommandPanel.Controls.Add(txtCommandResult);
            pnlCommandPanel.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            pnlCommandPanel.Location = new Point(40, 280);
            pnlCommandPanel.Margin = new Padding(4, 5, 4, 5);
            pnlCommandPanel.MinimumSize = new Size(1, 1);
            pnlCommandPanel.Name = "pnlCommandPanel";
            pnlCommandPanel.Size = new Size(910, 380);
            pnlCommandPanel.TabIndex = 4;
            pnlCommandPanel.Text = null;
            pnlCommandPanel.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // lblCommandPanel
            // 
            lblCommandPanel.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            lblCommandPanel.ForeColor = Color.FromArgb(48, 48, 48);
            lblCommandPanel.Location = new Point(0, 0);
            lblCommandPanel.Name = "lblCommandPanel";
            lblCommandPanel.Size = new Size(150, 30);
            lblCommandPanel.TabIndex = 0;
            lblCommandPanel.Text = "发送指令";
            lblCommandPanel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlQuickButtons
            // 
            pnlQuickButtons.Controls.Add(btnBetCommand);
            pnlQuickButtons.Controls.Add(btnGetQuotaCommand);
            pnlQuickButtons.Controls.Add(btnGetCookieCommand);
            pnlQuickButtons.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            pnlQuickButtons.Location = new Point(0, 35);
            pnlQuickButtons.Margin = new Padding(4, 5, 4, 5);
            pnlQuickButtons.MinimumSize = new Size(1, 1);
            pnlQuickButtons.Name = "pnlQuickButtons";
            pnlQuickButtons.Size = new Size(910, 45);
            pnlQuickButtons.TabIndex = 1;
            pnlQuickButtons.Text = null;
            pnlQuickButtons.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnBetCommand
            // 
            btnBetCommand.Cursor = Cursors.Hand;
            btnBetCommand.Font = new Font("微软雅黑", 9F);
            btnBetCommand.Location = new Point(0, 5);
            btnBetCommand.MinimumSize = new Size(1, 1);
            btnBetCommand.Name = "btnBetCommand";
            btnBetCommand.Size = new Size(100, 35);
            btnBetCommand.TabIndex = 0;
            btnBetCommand.Text = "投注";
            btnBetCommand.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnBetCommand.Click += BtnBetCommand_Click;
            // 
            // btnGetQuotaCommand
            // 
            btnGetQuotaCommand.Cursor = Cursors.Hand;
            btnGetQuotaCommand.Font = new Font("微软雅黑", 9F);
            btnGetQuotaCommand.Location = new Point(110, 5);
            btnGetQuotaCommand.MinimumSize = new Size(1, 1);
            btnGetQuotaCommand.Name = "btnGetQuotaCommand";
            btnGetQuotaCommand.Size = new Size(140, 35);
            btnGetQuotaCommand.TabIndex = 1;
            btnGetQuotaCommand.Text = "获取盘口额度";
            btnGetQuotaCommand.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnGetQuotaCommand.Click += BtnGetQuotaCommand_Click;
            // 
            // btnGetCookieCommand
            // 
            btnGetCookieCommand.Cursor = Cursors.Hand;
            btnGetCookieCommand.Font = new Font("微软雅黑", 9F);
            btnGetCookieCommand.Location = new Point(260, 5);
            btnGetCookieCommand.MinimumSize = new Size(1, 1);
            btnGetCookieCommand.Name = "btnGetCookieCommand";
            btnGetCookieCommand.Size = new Size(120, 35);
            btnGetCookieCommand.TabIndex = 2;
            btnGetCookieCommand.Text = "获取Cookie";
            btnGetCookieCommand.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnGetCookieCommand.Click += BtnGetCookieCommand_Click;
            // 
            // lblCommandInput
            // 
            lblCommandInput.Font = new Font("微软雅黑", 10F);
            lblCommandInput.ForeColor = Color.FromArgb(48, 48, 48);
            lblCommandInput.Location = new Point(0, 90);
            lblCommandInput.Name = "lblCommandInput";
            lblCommandInput.Size = new Size(150, 25);
            lblCommandInput.TabIndex = 2;
            lblCommandInput.Text = "命令输入:";
            lblCommandInput.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtCommand
            // 
            txtCommand.Font = new Font("微软雅黑", 9F);
            txtCommand.Location = new Point(0, 120);
            txtCommand.Margin = new Padding(4, 5, 4, 5);
            txtCommand.MinimumSize = new Size(1, 16);
            txtCommand.Name = "txtCommand";
            txtCommand.Padding = new Padding(5);
            txtCommand.ShowText = false;
            txtCommand.Size = new Size(710, 30);
            txtCommand.TabIndex = 3;
            txtCommand.TextAlignment = ContentAlignment.MiddleLeft;
            txtCommand.Watermark = "例如: 投注(1234大10) 或 获取盘口额度";
            // 
            // btnSendCommand
            // 
            btnSendCommand.Cursor = Cursors.Hand;
            btnSendCommand.Font = new Font("微软雅黑", 9F);
            btnSendCommand.Location = new Point(720, 120);
            btnSendCommand.MinimumSize = new Size(1, 1);
            btnSendCommand.Name = "btnSendCommand";
            btnSendCommand.Size = new Size(100, 30);
            btnSendCommand.TabIndex = 4;
            btnSendCommand.Text = "发送";
            btnSendCommand.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSendCommand.Click += BtnSendCommand_Click;
            // 
            // lblCommandResult
            // 
            lblCommandResult.Font = new Font("微软雅黑", 10F);
            lblCommandResult.ForeColor = Color.FromArgb(48, 48, 48);
            lblCommandResult.Location = new Point(0, 165);
            lblCommandResult.Name = "lblCommandResult";
            lblCommandResult.Size = new Size(150, 25);
            lblCommandResult.TabIndex = 5;
            lblCommandResult.Text = "执行结果:";
            lblCommandResult.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtCommandResult
            // 
            txtCommandResult.Font = new Font("Consolas", 9F);
            txtCommandResult.Location = new Point(0, 195);
            txtCommandResult.Margin = new Padding(4, 5, 4, 5);
            txtCommandResult.MinimumSize = new Size(1, 16);
            txtCommandResult.Multiline = true;
            txtCommandResult.Name = "txtCommandResult";
            txtCommandResult.Padding = new Padding(5);
            txtCommandResult.ReadOnly = true;
            txtCommandResult.ShowText = false;
            txtCommandResult.Size = new Size(820, 180);
            txtCommandResult.TabIndex = 6;
            txtCommandResult.TextAlignment = ContentAlignment.TopLeft;
            txtCommandResult.Watermark = "";
            // 
            // txtCookies
            // 
            txtCookies.Font = new Font("微软雅黑", 9F);
            txtCookies.Location = new Point(150, 140);
            txtCookies.Margin = new Padding(4, 5, 4, 5);
            txtCookies.MinimumSize = new Size(1, 16);
            txtCookies.Multiline = true;
            txtCookies.Name = "txtCookies";
            txtCookies.Padding = new Padding(5);
            txtCookies.ShowText = false;
            txtCookies.Size = new Size(800, 120);
            txtCookies.TabIndex = 3;
            txtCookies.TextAlignment = ContentAlignment.TopLeft;
            txtCookies.Watermark = "浏览器Cookies（可选，用于免登录）";
            // 
            // lblCookies
            // 
            lblCookies.Font = new Font("微软雅黑", 10F);
            lblCookies.ForeColor = Color.FromArgb(48, 48, 48);
            lblCookies.Location = new Point(40, 140);
            lblCookies.Name = "lblCookies";
            lblCookies.Size = new Size(100, 30);
            lblCookies.TabIndex = 2;
            lblCookies.Text = "Cookies:";
            lblCookies.TextAlign = ContentAlignment.TopRight;
            // 
            // txtNotes
            // 
            txtNotes.Font = new Font("微软雅黑", 10F);
            txtNotes.Location = new Point(150, 35);
            txtNotes.Margin = new Padding(4, 5, 4, 5);
            txtNotes.MinimumSize = new Size(1, 16);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.Padding = new Padding(5);
            txtNotes.ShowText = false;
            txtNotes.Size = new Size(800, 80);
            txtNotes.TabIndex = 1;
            txtNotes.TextAlignment = ContentAlignment.TopLeft;
            txtNotes.Watermark = "备注信息";
            // 
            // lblNotes
            // 
            lblNotes.Font = new Font("微软雅黑", 10F);
            lblNotes.ForeColor = Color.FromArgb(48, 48, 48);
            lblNotes.Location = new Point(40, 35);
            lblNotes.Name = "lblNotes";
            lblNotes.Size = new Size(100, 30);
            lblNotes.TabIndex = 0;
            lblNotes.Text = "备注:";
            lblNotes.TextAlign = ContentAlignment.TopRight;
            // 
            // tabRecords
            // 
            tabRecords.BackColor = Color.White;
            tabRecords.Controls.Add(pnlRecordsContent);
            tabRecords.Location = new Point(0, 40);
            tabRecords.Name = "tabRecords";
            tabRecords.Size = new Size(994, 675);
            tabRecords.TabIndex = 2;
            tabRecords.Text = "投注记录";
            // 
            // pnlRecordsContent
            // 
            pnlRecordsContent.Controls.Add(tbxRecordsDetailed);
            pnlRecordsContent.Controls.Add(dgvRecords);
            pnlRecordsContent.Controls.Add(pnlRecordsTop);
            pnlRecordsContent.Dock = DockStyle.Fill;
            pnlRecordsContent.Font = new Font("微软雅黑", 12F);
            pnlRecordsContent.Location = new Point(0, 0);
            pnlRecordsContent.Margin = new Padding(4, 5, 4, 5);
            pnlRecordsContent.MinimumSize = new Size(1, 1);
            pnlRecordsContent.Name = "pnlRecordsContent";
            pnlRecordsContent.Size = new Size(994, 675);
            pnlRecordsContent.TabIndex = 0;
            pnlRecordsContent.Text = null;
            pnlRecordsContent.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // tbxRecordsDetailed
            // 
            tbxRecordsDetailed.FillColor = Color.White;
            tbxRecordsDetailed.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            tbxRecordsDetailed.Location = new Point(711, 48);
            tbxRecordsDetailed.Margin = new Padding(4, 5, 4, 5);
            tbxRecordsDetailed.MinimumSize = new Size(1, 1);
            tbxRecordsDetailed.Name = "tbxRecordsDetailed";
            tbxRecordsDetailed.Padding = new Padding(2);
            tbxRecordsDetailed.ShowText = false;
            tbxRecordsDetailed.Size = new Size(279, 622);
            tbxRecordsDetailed.TabIndex = 2;
            tbxRecordsDetailed.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvRecords
            // 
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToResizeRows = false;
            dataGridViewCellStyle6.BackColor = Color.FromArgb(235, 243, 255);
            dgvRecords.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            dgvRecords.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRecords.BackgroundColor = Color.White;
            dgvRecords.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle7.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle7.ForeColor = Color.White;
            dataGridViewCellStyle7.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle7.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = DataGridViewTriState.True;
            dgvRecords.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            dgvRecords.ColumnHeadersHeight = 32;
            dgvRecords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvRecords.Dock = DockStyle.Left;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.Font = new Font("宋体", 12F);
            dgvRecords.GridColor = Color.FromArgb(80, 160, 255);
            dgvRecords.Location = new Point(0, 45);
            dgvRecords.MultiSelect = false;
            dgvRecords.Name = "dgvRecords";
            dgvRecords.ReadOnly = true;
            dataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle8.Font = new Font("宋体", 12F);
            dataGridViewCellStyle8.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle8.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle8.SelectionForeColor = Color.White;
            dataGridViewCellStyle8.WrapMode = DataGridViewTriState.True;
            dgvRecords.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
            dgvRecords.RowHeadersVisible = false;
            dgvRecords.RowHeadersWidth = 51;
            dataGridViewCellStyle9.BackColor = Color.White;
            dataGridViewCellStyle9.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvRecords.RowsDefaultCellStyle = dataGridViewCellStyle9;
            dgvRecords.RowTemplate.Height = 29;
            dgvRecords.SelectedIndex = -1;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.Size = new Size(706, 630);
            dgvRecords.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvRecords.TabIndex = 1;
            // 
            // pnlRecordsTop
            // 
            pnlRecordsTop.Controls.Add(btnRefreshRecords);
            pnlRecordsTop.Controls.Add(dtpEndDate);
            pnlRecordsTop.Controls.Add(lblTo);
            pnlRecordsTop.Controls.Add(dtpStartDate);
            pnlRecordsTop.Controls.Add(lblDateRange);
            pnlRecordsTop.Dock = DockStyle.Top;
            pnlRecordsTop.Font = new Font("微软雅黑", 12F);
            pnlRecordsTop.Location = new Point(0, 0);
            pnlRecordsTop.Margin = new Padding(4, 5, 4, 5);
            pnlRecordsTop.MinimumSize = new Size(1, 1);
            pnlRecordsTop.Name = "pnlRecordsTop";
            pnlRecordsTop.Size = new Size(994, 45);
            pnlRecordsTop.TabIndex = 0;
            pnlRecordsTop.Text = null;
            pnlRecordsTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnRefreshRecords
            // 
            btnRefreshRecords.Cursor = Cursors.Hand;
            btnRefreshRecords.Font = new Font("微软雅黑", 9F);
            btnRefreshRecords.Location = new Point(595, 7);
            btnRefreshRecords.MinimumSize = new Size(1, 1);
            btnRefreshRecords.Name = "btnRefreshRecords";
            btnRefreshRecords.Size = new Size(100, 30);
            btnRefreshRecords.TabIndex = 4;
            btnRefreshRecords.Text = "刷新";
            btnRefreshRecords.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnRefreshRecords.Click += btnRefreshRecords_Click;
            // 
            // dtpEndDate
            // 
            dtpEndDate.Font = new Font("微软雅黑", 9F);
            dtpEndDate.Location = new Point(430, 10);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(150, 23);
            dtpEndDate.TabIndex = 3;
            // 
            // lblTo
            // 
            lblTo.Font = new Font("微软雅黑", 10F);
            lblTo.ForeColor = Color.FromArgb(48, 48, 48);
            lblTo.Location = new Point(390, 7);
            lblTo.Name = "lblTo";
            lblTo.Size = new Size(30, 30);
            lblTo.TabIndex = 2;
            lblTo.Text = "至";
            lblTo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // dtpStartDate
            // 
            dtpStartDate.Font = new Font("微软雅黑", 9F);
            dtpStartDate.Location = new Point(230, 10);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(150, 23);
            dtpStartDate.TabIndex = 1;
            // 
            // lblDateRange
            // 
            lblDateRange.Font = new Font("微软雅黑", 10F);
            lblDateRange.ForeColor = Color.FromArgb(48, 48, 48);
            lblDateRange.Location = new Point(10, 7);
            lblDateRange.Name = "lblDateRange";
            lblDateRange.Size = new Size(200, 30);
            lblDateRange.TabIndex = 0;
            lblDateRange.Text = "时间范围:";
            lblDateRange.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlRightTop
            // 
            pnlRightTop.Controls.Add(btnClose);
            pnlRightTop.Controls.Add(btnStopBrowser);
            pnlRightTop.Controls.Add(btnStartBrowser);
            pnlRightTop.Controls.Add(btnSave);
            pnlRightTop.Controls.Add(lblStatus);
            pnlRightTop.Dock = DockStyle.Top;
            pnlRightTop.Font = new Font("微软雅黑", 12F);
            pnlRightTop.Location = new Point(0, 0);
            pnlRightTop.Margin = new Padding(4, 5, 4, 5);
            pnlRightTop.MinimumSize = new Size(1, 1);
            pnlRightTop.Name = "pnlRightTop";
            pnlRightTop.Size = new Size(994, 50);
            pnlRightTop.TabIndex = 0;
            pnlRightTop.Text = null;
            pnlRightTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            btnClose.Cursor = Cursors.Hand;
            btnClose.Font = new Font("微软雅黑", 10F);
            btnClose.Location = new Point(870, 10);
            btnClose.MinimumSize = new Size(1, 1);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(100, 35);
            btnClose.TabIndex = 4;
            btnClose.Text = "关闭";
            btnClose.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClose.Click += btnClose_Click;
            // 
            // btnStopBrowser
            // 
            btnStopBrowser.Cursor = Cursors.Hand;
            btnStopBrowser.Font = new Font("微软雅黑", 10F);
            btnStopBrowser.Location = new Point(390, 10);
            btnStopBrowser.MinimumSize = new Size(1, 1);
            btnStopBrowser.Name = "btnStopBrowser";
            btnStopBrowser.Size = new Size(120, 35);
            btnStopBrowser.TabIndex = 3;
            btnStopBrowser.Text = "停止浏览器";
            btnStopBrowser.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnStopBrowser.Click += btnStopBrowser_Click;
            // 
            // btnStartBrowser
            // 
            btnStartBrowser.Cursor = Cursors.Hand;
            btnStartBrowser.Font = new Font("微软雅黑", 10F);
            btnStartBrowser.Location = new Point(260, 10);
            btnStartBrowser.MinimumSize = new Size(1, 1);
            btnStartBrowser.Name = "btnStartBrowser";
            btnStartBrowser.Size = new Size(120, 35);
            btnStartBrowser.TabIndex = 2;
            btnStartBrowser.Text = "启动浏览器";
            btnStartBrowser.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnStartBrowser.Click += btnStartBrowser_Click;
            // 
            // btnSave
            // 
            btnSave.Cursor = Cursors.Hand;
            btnSave.Font = new Font("微软雅黑", 10F);
            btnSave.Location = new Point(130, 10);
            btnSave.MinimumSize = new Size(1, 1);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(120, 35);
            btnSave.TabIndex = 1;
            btnSave.Text = "保存配置";
            btnSave.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSave.Click += btnSave_Click;
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("微软雅黑", 10F);
            lblStatus.ForeColor = Color.FromArgb(48, 48, 48);
            lblStatus.Location = new Point(10, 10);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(110, 35);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "状态: 未运行";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // BetConfigManagerForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1400, 800);
            Controls.Add(splitContainer);
            Font = new Font("微软雅黑", 10F);
            Name = "BetConfigManagerForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "自动投注配置管理器";
            ZoomScaleRect = new Rectangle(19, 19, 1400, 800);
            Load += BetConfigManagerForm_Load;
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            (splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvConfigs).EndInit();
            pnlLeftTop.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabBasic.ResumeLayout(false);
            pnlBasicContent.ResumeLayout(false);
            tabAdvanced.ResumeLayout(false);
            pnlAdvancedContent.ResumeLayout(false);
            pnlCommandPanel.ResumeLayout(false);
            pnlQuickButtons.ResumeLayout(false);
            tabRecords.ResumeLayout(false);
            pnlRecordsContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvRecords).EndInit();
            pnlRecordsTop.ResumeLayout(false);
            pnlRightTop.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UISplitContainer splitContainer;
        private Sunny.UI.UIPanel pnlLeft;
        private Sunny.UI.UIDataGridView dgvConfigs;
        private Sunny.UI.UIPanel pnlLeftTop;
        private Sunny.UI.UIButton btnDelete;
        private Sunny.UI.UIButton btnEdit;
        private Sunny.UI.UIButton btnAdd;
        private Sunny.UI.UIPanel pnlRight;
        private Sunny.UI.UITabControl tabControl;
        private System.Windows.Forms.TabPage tabBasic;
        private Sunny.UI.UIPanel pnlBasicContent;
        private Sunny.UI.UIButton btnTestConnection;
        private Sunny.UI.UICheckBox chkShowBrowser;
        private Sunny.UI.UICheckBox chkAutoLogin;
        private Sunny.UI.UICheckBox chkEnabled;
        private Sunny.UI.UITextBox txtMaxBetAmount;
        private Sunny.UI.UILabel lblMaxBetAmount;
        private Sunny.UI.UITextBox txtMinBetAmount;
        private Sunny.UI.UILabel lblMinBetAmount;
        private Sunny.UI.UITextBox txtPassword;
        private Sunny.UI.UILabel lblPassword;
        private Sunny.UI.UITextBox txtUsername;
        private Sunny.UI.UILabel lblUsername;
        private Sunny.UI.UITextBox txtPlatformUrl;
        private Sunny.UI.UILabel lblPlatformUrl;
        private Sunny.UI.UIComboBox cbxPlatform;
        private Sunny.UI.UILabel lblPlatform;
        private Sunny.UI.UITextBox txtConfigName;
        private Sunny.UI.UILabel lblConfigName;
        private System.Windows.Forms.TabPage tabAdvanced;
        private Sunny.UI.UIPanel pnlAdvancedContent;
        private Sunny.UI.UIPanel pnlCommandPanel;
        private Sunny.UI.UILabel lblCommandPanel;
        private Sunny.UI.UIPanel pnlQuickButtons;
        private Sunny.UI.UIButton btnBetCommand;
        private Sunny.UI.UIButton btnGetQuotaCommand;
        private Sunny.UI.UIButton btnGetCookieCommand;
        private Sunny.UI.UILabel lblCommandInput;
        private Sunny.UI.UITextBox txtCommand;
        private Sunny.UI.UIButton btnSendCommand;
        private Sunny.UI.UILabel lblCommandResult;
        private Sunny.UI.UITextBox txtCommandResult;
        private Sunny.UI.UITextBox txtCookies;
        private Sunny.UI.UILabel lblCookies;
        private Sunny.UI.UITextBox txtNotes;
        private Sunny.UI.UILabel lblNotes;
        private System.Windows.Forms.TabPage tabRecords;
        private Sunny.UI.UIPanel pnlRecordsContent;
        private Sunny.UI.UIDataGridView dgvRecords;
        private Sunny.UI.UIPanel pnlRecordsTop;
        private Sunny.UI.UIButton btnRefreshRecords;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private Sunny.UI.UILabel lblTo;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private Sunny.UI.UILabel lblDateRange;
        private Sunny.UI.UIPanel pnlRightTop;
        private Sunny.UI.UIButton btnClose;
        private Sunny.UI.UIButton btnStopBrowser;
        private Sunny.UI.UIButton btnStartBrowser;
        private Sunny.UI.UIButton btnSave;
        private Sunny.UI.UILabel lblStatus;
        private Sunny.UI.UIRichTextBox tbxRecordsDetailed;
    }
}

