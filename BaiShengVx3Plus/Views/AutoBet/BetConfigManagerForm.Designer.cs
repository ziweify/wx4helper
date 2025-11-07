namespace BaiShengVx3Plus.Views.AutoBet
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            splitContainer = new Sunny.UI.UISplitContainer();
            pnlLeft = new Sunny.UI.UIPanel();
            dgvConfigs = new Sunny.UI.UIDataGridView();
            pnlLeftTop = new Sunny.UI.UIPanel();
            btnDelete = new Sunny.UI.UIButton();
            btnEdit = new Sunny.UI.UIButton();
            btnAdd = new Sunny.UI.UIButton();
            pnlRight = new Sunny.UI.UIPanel();
            tabControl = new Sunny.UI.UITabControl();
            tabBasic = new System.Windows.Forms.TabPage();
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
            tabAdvanced = new System.Windows.Forms.TabPage();
            pnlAdvancedContent = new Sunny.UI.UIPanel();
            txtBetScript = new Sunny.UI.UITextBox();
            lblBetScript = new Sunny.UI.UILabel();
            txtCookies = new Sunny.UI.UITextBox();
            lblCookies = new Sunny.UI.UILabel();
            txtNotes = new Sunny.UI.UITextBox();
            lblNotes = new Sunny.UI.UILabel();
            tabRecords = new System.Windows.Forms.TabPage();
            pnlRecordsContent = new Sunny.UI.UIPanel();
            dgvRecords = new Sunny.UI.UIDataGridView();
            pnlRecordsTop = new Sunny.UI.UIPanel();
            btnRefreshRecords = new Sunny.UI.UIButton();
            dtpEndDate = new System.Windows.Forms.DateTimePicker();
            lblTo = new Sunny.UI.UILabel();
            dtpStartDate = new System.Windows.Forms.DateTimePicker();
            lblDateRange = new Sunny.UI.UILabel();
            pnlRightTop = new Sunny.UI.UIPanel();
            btnClose = new Sunny.UI.UIButton();
            btnStopBrowser = new Sunny.UI.UIButton();
            btnStartBrowser = new Sunny.UI.UIButton();
            btnSave = new Sunny.UI.UIButton();
            lblStatus = new Sunny.UI.UILabel();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
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
            tabRecords.SuspendLayout();
            pnlRecordsContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).BeginInit();
            pnlRecordsTop.SuspendLayout();
            pnlRightTop.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer
            // 
            splitContainer.Cursor = System.Windows.Forms.Cursors.VSplit;
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Location = new System.Drawing.Point(0, 0);
            splitContainer.MinimumSize = new System.Drawing.Size(20, 20);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(pnlLeft);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(pnlRight);
            splitContainer.Size = new System.Drawing.Size(1400, 800);
            splitContainer.SplitterDistance = 400;
            splitContainer.SplitterWidth = 6;
            splitContainer.TabIndex = 0;
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(dgvConfigs);
            pnlLeft.Controls.Add(pnlLeftTop);
            pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlLeft.Font = new System.Drawing.Font("微软雅黑", 12F);
            pnlLeft.Location = new System.Drawing.Point(0, 0);
            pnlLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            pnlLeft.MinimumSize = new System.Drawing.Size(1, 1);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new System.Drawing.Size(400, 800);
            pnlLeft.TabIndex = 0;
            pnlLeft.Text = "配置列表";
            pnlLeft.TextAlignment = System.Drawing.ContentAlignment.TopCenter;
            // 
            // dgvConfigs
            // 
            dgvConfigs.AllowUserToAddRows = false;
            dgvConfigs.AllowUserToDeleteRows = false;
            dgvConfigs.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(235, 243, 255);
            dgvConfigs.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvConfigs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvConfigs.BackgroundColor = System.Drawing.Color.White;
            dgvConfigs.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvConfigs.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvConfigs.ColumnHeadersHeight = 32;
            dgvConfigs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 12F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(155, 200, 255);
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgvConfigs.DefaultCellStyle = dataGridViewCellStyle3;
            dgvConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvConfigs.EnableHeadersVisualStyles = false;
            dgvConfigs.Font = new System.Drawing.Font("宋体", 12F);
            dgvConfigs.GridColor = System.Drawing.Color.FromArgb(80, 160, 255);
            dgvConfigs.Location = new System.Drawing.Point(0, 75);
            dgvConfigs.MultiSelect = false;
            dgvConfigs.Name = "dgvConfigs";
            dgvConfigs.ReadOnly = true;
            dgvConfigs.RectColor = System.Drawing.Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 12F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvConfigs.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvConfigs.RowHeadersVisible = false;
            dgvConfigs.RowHeadersWidth = 51;
            dgvConfigs.RowTemplate.Height = 29;
            dgvConfigs.SelectedIndex = -1;
            dgvConfigs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvConfigs.Size = new System.Drawing.Size(400, 725);
            dgvConfigs.StripeOddColor = System.Drawing.Color.FromArgb(235, 243, 255);
            dgvConfigs.TabIndex = 1;
            dgvConfigs.SelectionChanged += dgvConfigs_SelectionChanged;
            // 
            // pnlLeftTop
            // 
            pnlLeftTop.Controls.Add(btnDelete);
            pnlLeftTop.Controls.Add(btnEdit);
            pnlLeftTop.Controls.Add(btnAdd);
            pnlLeftTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlLeftTop.Font = new System.Drawing.Font("微软雅黑", 12F);
            pnlLeftTop.Location = new System.Drawing.Point(0, 35);
            pnlLeftTop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            pnlLeftTop.MinimumSize = new System.Drawing.Size(1, 1);
            pnlLeftTop.Name = "pnlLeftTop";
            pnlLeftTop.Size = new System.Drawing.Size(400, 40);
            pnlLeftTop.TabIndex = 0;
            pnlLeftTop.Text = null;
            pnlLeftTop.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnDelete
            // 
            btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDelete.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnDelete.Location = new System.Drawing.Point(220, 5);
            btnDelete.MinimumSize = new System.Drawing.Size(1, 1);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(100, 30);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "删除";
            btnDelete.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            btnDelete.Click += btnDelete_Click;
            // 
            // btnEdit
            // 
            btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            btnEdit.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnEdit.Location = new System.Drawing.Point(115, 5);
            btnEdit.MinimumSize = new System.Drawing.Size(1, 1);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new System.Drawing.Size(100, 30);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "编辑";
            btnEdit.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            btnEdit.Click += btnEdit_Click;
            // 
            // btnAdd
            // 
            btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAdd.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnAdd.Location = new System.Drawing.Point(10, 5);
            btnAdd.MinimumSize = new System.Drawing.Size(1, 1);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new System.Drawing.Size(100, 30);

            btnAdd.TabIndex = 0;
            btnAdd.Text = "新增";
            btnAdd.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            btnAdd.Click += btnAdd_Click;
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(tabControl);
            pnlRight.Controls.Add(pnlRightTop);
            pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlRight.Font = new System.Drawing.Font("微软雅黑", 12F);
            pnlRight.Location = new System.Drawing.Point(0, 0);
            pnlRight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            pnlRight.MinimumSize = new System.Drawing.Size(1, 1);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new System.Drawing.Size(994, 800);
            pnlRight.TabIndex = 0;
            pnlRight.Text = "配置详情";
            pnlRight.TextAlignment = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabBasic);
            tabControl.Controls.Add(tabAdvanced);
            tabControl.Controls.Add(tabRecords);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            tabControl.Font = new System.Drawing.Font("微软雅黑", 10F);
            tabControl.ItemSize = new System.Drawing.Size(150, 40);
            tabControl.Location = new System.Drawing.Point(0, 85);
            tabControl.MainPage = "";
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(994, 715);
            tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            tabControl.TabIndex = 1;
            // 
            // tabBasic
            // 
            tabBasic.BackColor = System.Drawing.Color.White;
            tabBasic.Controls.Add(pnlBasicContent);
            tabBasic.Location = new System.Drawing.Point(0, 40);
            tabBasic.Name = "tabBasic";
            tabBasic.Size = new System.Drawing.Size(994, 675);
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
            pnlBasicContent.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlBasicContent.Font = new System.Drawing.Font("微软雅黑", 12F);
            pnlBasicContent.Location = new System.Drawing.Point(0, 0);
            pnlBasicContent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            pnlBasicContent.MinimumSize = new System.Drawing.Size(1, 1);
            pnlBasicContent.Name = "pnlBasicContent";
            pnlBasicContent.Size = new System.Drawing.Size(994, 675);
            pnlBasicContent.TabIndex = 0;
            pnlBasicContent.Text = null;
            pnlBasicContent.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnTestConnection
            // 
            btnTestConnection.Cursor = System.Windows.Forms.Cursors.Hand;
            btnTestConnection.Font = new System.Drawing.Font("微软雅黑", 10F);
            btnTestConnection.Location = new System.Drawing.Point(150, 420);
            btnTestConnection.MinimumSize = new System.Drawing.Size(1, 1);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new System.Drawing.Size(150, 35);

            btnTestConnection.TabIndex = 17;
            btnTestConnection.Text = "测试连接";
            btnTestConnection.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            btnTestConnection.Click += btnTestConnection_Click;
            // 
            // chkShowBrowser
            // 
            chkShowBrowser.Cursor = System.Windows.Forms.Cursors.Hand;
            chkShowBrowser.Font = new System.Drawing.Font("微软雅黑", 10F);
            chkShowBrowser.Location = new System.Drawing.Point(150, 370);
            chkShowBrowser.MinimumSize = new System.Drawing.Size(1, 1);
            chkShowBrowser.Name = "chkShowBrowser";
            chkShowBrowser.Size = new System.Drawing.Size(200, 29);
            chkShowBrowser.TabIndex = 16;
            chkShowBrowser.Text = "显示浏览器窗口";
            // 
            // chkAutoLogin
            // 
            chkAutoLogin.Checked = true;
            chkAutoLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            chkAutoLogin.Font = new System.Drawing.Font("微软雅黑", 10F);
            chkAutoLogin.Location = new System.Drawing.Point(380, 335);
            chkAutoLogin.MinimumSize = new System.Drawing.Size(1, 1);
            chkAutoLogin.Name = "chkAutoLogin";
            chkAutoLogin.Size = new System.Drawing.Size(150, 29);
            chkAutoLogin.TabIndex = 15;
            chkAutoLogin.Text = "自动登录";
            // 
            // chkEnabled
            // 
            chkEnabled.Checked = true;
            chkEnabled.Cursor = System.Windows.Forms.Cursors.Hand;
            chkEnabled.Font = new System.Drawing.Font("微软雅黑", 10F);
            chkEnabled.Location = new System.Drawing.Point(150, 335);
            chkEnabled.MinimumSize = new System.Drawing.Size(1, 1);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new System.Drawing.Size(150, 29);
            chkEnabled.TabIndex = 14;
            chkEnabled.Text = "启用此配置";
            // 
            // txtMaxBetAmount
            // 
            txtMaxBetAmount.Font = new System.Drawing.Font("微软雅黑", 10F);
            txtMaxBetAmount.Location = new System.Drawing.Point(480, 285);
            txtMaxBetAmount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtMaxBetAmount.MinimumSize = new System.Drawing.Size(1, 16);
            txtMaxBetAmount.Name = "txtMaxBetAmount";
            txtMaxBetAmount.Padding = new System.Windows.Forms.Padding(5);
            txtMaxBetAmount.ShowText = false;
            txtMaxBetAmount.Size = new System.Drawing.Size(200, 30);
            txtMaxBetAmount.TabIndex = 13;
            txtMaxBetAmount.Text = "10000";
            txtMaxBetAmount.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            txtMaxBetAmount.Watermark = "";
            // 
            // lblMaxBetAmount
            // 
            lblMaxBetAmount.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblMaxBetAmount.Location = new System.Drawing.Point(370, 285);
            lblMaxBetAmount.Name = "lblMaxBetAmount";
            lblMaxBetAmount.Size = new System.Drawing.Size(100, 30);
            lblMaxBetAmount.TabIndex = 12;
            lblMaxBetAmount.Text = "最大金额:";
            lblMaxBetAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtMinBetAmount
            // 
            txtMinBetAmount.Font = new System.Drawing.Font("微软雅黑", 10F);
            txtMinBetAmount.Location = new System.Drawing.Point(150, 285);
            txtMinBetAmount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtMinBetAmount.MinimumSize = new System.Drawing.Size(1, 16);
            txtMinBetAmount.Name = "txtMinBetAmount";
            txtMinBetAmount.Padding = new System.Windows.Forms.Padding(5);
            txtMinBetAmount.ShowText = false;
            txtMinBetAmount.Size = new System.Drawing.Size(200, 30);
            txtMinBetAmount.TabIndex = 11;
            txtMinBetAmount.Text = "1";
            txtMinBetAmount.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            txtMinBetAmount.Watermark = "";
            // 
            // lblMinBetAmount
            // 
            lblMinBetAmount.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblMinBetAmount.Location = new System.Drawing.Point(40, 285);
            lblMinBetAmount.Name = "lblMinBetAmount";
            lblMinBetAmount.Size = new System.Drawing.Size(100, 30);
            lblMinBetAmount.TabIndex = 10;
            lblMinBetAmount.Text = "最小金额:";
            lblMinBetAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPassword
            // 
            txtPassword.Font = new System.Drawing.Font("微软雅黑", 10F);
            txtPassword.Location = new System.Drawing.Point(150, 235);
            txtPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtPassword.MinimumSize = new System.Drawing.Size(1, 16);
            txtPassword.Name = "txtPassword";
            txtPassword.Padding = new System.Windows.Forms.Padding(5);
            txtPassword.PasswordChar = '*';
            txtPassword.ShowText = false;
            txtPassword.Size = new System.Drawing.Size(530, 30);
            txtPassword.TabIndex = 9;
            txtPassword.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            txtPassword.Watermark = "投注账号密码";
            // 
            // lblPassword
            // 
            lblPassword.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblPassword.Location = new System.Drawing.Point(40, 235);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(100, 30);
            lblPassword.TabIndex = 8;
            lblPassword.Text = "密码:";
            lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtUsername
            // 
            txtUsername.Font = new System.Drawing.Font("微软雅黑", 10F);
            txtUsername.Location = new System.Drawing.Point(150, 185);
            txtUsername.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtUsername.MinimumSize = new System.Drawing.Size(1, 16);
            txtUsername.Name = "txtUsername";
            txtUsername.Padding = new System.Windows.Forms.Padding(5);
            txtUsername.ShowText = false;
            txtUsername.Size = new System.Drawing.Size(530, 30);
            txtUsername.TabIndex = 7;
            txtUsername.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            txtUsername.Watermark = "投注账号";
            // 
            // lblUsername
            // 
            lblUsername.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblUsername.Location = new System.Drawing.Point(40, 185);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new System.Drawing.Size(100, 30);
            lblUsername.TabIndex = 6;
            lblUsername.Text = "账号:";
            lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPlatformUrl
            // 
            txtPlatformUrl.Font = new System.Drawing.Font("微软雅黑", 10F);
            txtPlatformUrl.Location = new System.Drawing.Point(150, 135);
            txtPlatformUrl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtPlatformUrl.MinimumSize = new System.Drawing.Size(1, 16);
            txtPlatformUrl.Name = "txtPlatformUrl";
            txtPlatformUrl.Padding = new System.Windows.Forms.Padding(5);
            txtPlatformUrl.ShowText = false;
            txtPlatformUrl.Size = new System.Drawing.Size(530, 30);
            txtPlatformUrl.TabIndex = 5;
            txtPlatformUrl.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            txtPlatformUrl.Watermark = "https://www.example.com";
            // 
            // lblPlatformUrl
            // 
            lblPlatformUrl.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblPlatformUrl.Location = new System.Drawing.Point(40, 135);
            lblPlatformUrl.Name = "lblPlatformUrl";
            lblPlatformUrl.Size = new System.Drawing.Size(100, 30);
            lblPlatformUrl.TabIndex = 4;
            lblPlatformUrl.Text = "平台URL:";
            lblPlatformUrl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbxPlatform
            // 
            cbxPlatform.DataSource = null;
            cbxPlatform.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            cbxPlatform.FillColor = System.Drawing.Color.White;
            cbxPlatform.Font = new System.Drawing.Font("微软雅黑", 10F);
            cbxPlatform.ItemHoverColor = System.Drawing.Color.FromArgb(155, 200, 255);
            cbxPlatform.Items.AddRange(BaiShengVx3Plus.Shared.Platform.BetPlatformHelper.GetAllPlatformNames());
            cbxPlatform.ItemSelectForeColor = System.Drawing.Color.FromArgb(235, 243, 255);
            cbxPlatform.Location = new System.Drawing.Point(150, 85);
            cbxPlatform.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cbxPlatform.MinimumSize = new System.Drawing.Size(63, 0);
            cbxPlatform.Name = "cbxPlatform";
            cbxPlatform.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            cbxPlatform.Size = new System.Drawing.Size(530, 30);
            cbxPlatform.SymbolSize = 24;
            cbxPlatform.TabIndex = 3;
            cbxPlatform.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            cbxPlatform.Watermark = "";
            cbxPlatform.SelectedIndexChanged += cbxPlatform_SelectedIndexChanged;
            // 
            // lblPlatform
            // 
            lblPlatform.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblPlatform.Location = new System.Drawing.Point(40, 85);
            lblPlatform.Name = "lblPlatform";
            lblPlatform.Size = new System.Drawing.Size(100, 30);
            lblPlatform.TabIndex = 2;
            lblPlatform.Text = "平台:";
            lblPlatform.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtConfigName
            // 
            txtConfigName.Font = new System.Drawing.Font("微软雅黑", 10F);
            txtConfigName.Location = new System.Drawing.Point(150, 35);
            txtConfigName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtConfigName.MinimumSize = new System.Drawing.Size(1, 16);
            txtConfigName.Name = "txtConfigName";
            txtConfigName.Padding = new System.Windows.Forms.Padding(5);
            txtConfigName.ShowText = false;
            txtConfigName.Size = new System.Drawing.Size(530, 30);
            txtConfigName.TabIndex = 1;
            txtConfigName.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            txtConfigName.Watermark = "例如：云�?8-账号1";
            // 
            // lblConfigName
            // 
            lblConfigName.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblConfigName.Location = new System.Drawing.Point(40, 35);
            lblConfigName.Name = "lblConfigName";
            lblConfigName.Size = new System.Drawing.Size(100, 30);
            lblConfigName.TabIndex = 0;
            lblConfigName.Text = "配置名称:";
            lblConfigName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabAdvanced
            // 
            tabAdvanced.BackColor = System.Drawing.Color.White;
            tabAdvanced.Controls.Add(pnlAdvancedContent);
            tabAdvanced.Location = new System.Drawing.Point(0, 40);
            tabAdvanced.Name = "tabAdvanced";
            tabAdvanced.Size = new System.Drawing.Size(994, 675);
            tabAdvanced.TabIndex = 1;
            tabAdvanced.Text = "高级设置";
            // 
            // pnlAdvancedContent
            // 
            pnlAdvancedContent.Controls.Add(txtBetScript);
            pnlAdvancedContent.Controls.Add(lblBetScript);
            pnlAdvancedContent.Controls.Add(txtCookies);
            pnlAdvancedContent.Controls.Add(lblCookies);
            pnlAdvancedContent.Controls.Add(txtNotes);
            pnlAdvancedContent.Controls.Add(lblNotes);
            pnlAdvancedContent.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlAdvancedContent.Font = new System.Drawing.Font("微软雅黑", 12F);
            pnlAdvancedContent.Location = new System.Drawing.Point(0, 0);
            pnlAdvancedContent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            pnlAdvancedContent.MinimumSize = new System.Drawing.Size(1, 1);
            pnlAdvancedContent.Name = "pnlAdvancedContent";
            pnlAdvancedContent.Size = new System.Drawing.Size(994, 675);
            pnlAdvancedContent.TabIndex = 0;
            pnlAdvancedContent.Text = null;
            pnlAdvancedContent.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtBetScript
            // 
            txtBetScript.Font = new System.Drawing.Font("微软雅黑", 9F);
            txtBetScript.Location = new System.Drawing.Point(150, 280);
            txtBetScript.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtBetScript.MinimumSize = new System.Drawing.Size(1, 16);
            txtBetScript.Multiline = true;
            txtBetScript.Name = "txtBetScript";
            txtBetScript.Padding = new System.Windows.Forms.Padding(5);
            txtBetScript.ShowText = false;
            txtBetScript.Size = new System.Drawing.Size(800, 350);
            txtBetScript.TabIndex = 5;
            txtBetScript.TextAlignment = System.Drawing.ContentAlignment.TopLeft;
            txtBetScript.Watermark = "自定义投注脚本（高级）";
            // 
            // lblBetScript
            // 
            lblBetScript.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblBetScript.Location = new System.Drawing.Point(40, 280);
            lblBetScript.Name = "lblBetScript";
            lblBetScript.Size = new System.Drawing.Size(100, 30);
            lblBetScript.TabIndex = 4;
            lblBetScript.Text = "投注脚本:";
            lblBetScript.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtCookies
            // 
            txtCookies.Font = new System.Drawing.Font("微软雅黑", 9F);
            txtCookies.Location = new System.Drawing.Point(150, 140);
            txtCookies.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtCookies.MinimumSize = new System.Drawing.Size(1, 16);
            txtCookies.Multiline = true;
            txtCookies.Name = "txtCookies";
            txtCookies.Padding = new System.Windows.Forms.Padding(5);
            txtCookies.ShowText = false;
            txtCookies.Size = new System.Drawing.Size(800, 120);
            txtCookies.TabIndex = 3;
            txtCookies.TextAlignment = System.Drawing.ContentAlignment.TopLeft;
            txtCookies.Watermark = "浏览器Cookies（可选，用于免登录）";
            // 
            // lblCookies
            // 
            lblCookies.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblCookies.Location = new System.Drawing.Point(40, 140);
            lblCookies.Name = "lblCookies";
            lblCookies.Size = new System.Drawing.Size(100, 30);
            lblCookies.TabIndex = 2;
            lblCookies.Text = "Cookies:";
            lblCookies.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtNotes
            // 
            txtNotes.Font = new System.Drawing.Font("微软雅黑", 10F);
            txtNotes.Location = new System.Drawing.Point(150, 35);
            txtNotes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtNotes.MinimumSize = new System.Drawing.Size(1, 16);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.Padding = new System.Windows.Forms.Padding(5);
            txtNotes.ShowText = false;
            txtNotes.Size = new System.Drawing.Size(800, 80);
            txtNotes.TabIndex = 1;
            txtNotes.TextAlignment = System.Drawing.ContentAlignment.TopLeft;
            txtNotes.Watermark = "备注信息";
            // 
            // lblNotes
            // 
            lblNotes.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblNotes.Location = new System.Drawing.Point(40, 35);
            lblNotes.Name = "lblNotes";
            lblNotes.Size = new System.Drawing.Size(100, 30);
            lblNotes.TabIndex = 0;
            lblNotes.Text = "备注:";
            lblNotes.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tabRecords
            // 
            tabRecords.BackColor = System.Drawing.Color.White;
            tabRecords.Controls.Add(pnlRecordsContent);
            tabRecords.Location = new System.Drawing.Point(0, 40);
            tabRecords.Name = "tabRecords";
            tabRecords.Size = new System.Drawing.Size(994, 675);
            tabRecords.TabIndex = 2;
            tabRecords.Text = "投注记录";
            // 
            // pnlRecordsContent
            // 
            pnlRecordsContent.Controls.Add(dgvRecords);
            pnlRecordsContent.Controls.Add(pnlRecordsTop);
            pnlRecordsContent.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlRecordsContent.Font = new System.Drawing.Font("微软雅黑", 12F);
            pnlRecordsContent.Location = new System.Drawing.Point(0, 0);
            pnlRecordsContent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            pnlRecordsContent.MinimumSize = new System.Drawing.Size(1, 1);
            pnlRecordsContent.Name = "pnlRecordsContent";
            pnlRecordsContent.Size = new System.Drawing.Size(994, 675);
            pnlRecordsContent.TabIndex = 0;
            pnlRecordsContent.Text = null;
            pnlRecordsContent.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dgvRecords
            // 
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToResizeRows = false;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(235, 243, 255);
            dgvRecords.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            dgvRecords.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvRecords.BackgroundColor = System.Drawing.Color.White;
            dgvRecords.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle6.Font = new System.Drawing.Font("微软雅黑", 12F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvRecords.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            dgvRecords.ColumnHeadersHeight = 32;
            dgvRecords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.Font = new System.Drawing.Font("宋体", 12F);
            dgvRecords.GridColor = System.Drawing.Color.FromArgb(80, 160, 255);
            dgvRecords.Location = new System.Drawing.Point(0, 45);
            dgvRecords.MultiSelect = false;
            dgvRecords.Name = "dgvRecords";
            dgvRecords.ReadOnly = true;
            dgvRecords.RectColor = System.Drawing.Color.FromArgb(80, 160, 255);
            dgvRecords.RowHeadersVisible = false;
            dgvRecords.RowHeadersWidth = 51;
            dgvRecords.RowTemplate.Height = 29;
            dgvRecords.SelectedIndex = -1;
            dgvRecords.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.Size = new System.Drawing.Size(994, 630);
            dgvRecords.StripeOddColor = System.Drawing.Color.FromArgb(235, 243, 255);
            dgvRecords.TabIndex = 1;
            // 
            // pnlRecordsTop
            // 
            pnlRecordsTop.Controls.Add(btnRefreshRecords);
            pnlRecordsTop.Controls.Add(dtpEndDate);
            pnlRecordsTop.Controls.Add(lblTo);
            pnlRecordsTop.Controls.Add(dtpStartDate);
            pnlRecordsTop.Controls.Add(lblDateRange);
            pnlRecordsTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlRecordsTop.Font = new System.Drawing.Font("微软雅黑", 12F);
            pnlRecordsTop.Location = new System.Drawing.Point(0, 0);
            pnlRecordsTop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            pnlRecordsTop.MinimumSize = new System.Drawing.Size(1, 1);
            pnlRecordsTop.Name = "pnlRecordsTop";
            pnlRecordsTop.Size = new System.Drawing.Size(994, 45);
            pnlRecordsTop.TabIndex = 0;
            pnlRecordsTop.Text = null;
            pnlRecordsTop.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnRefreshRecords
            // 
            btnRefreshRecords.Cursor = System.Windows.Forms.Cursors.Hand;
            btnRefreshRecords.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnRefreshRecords.Location = new System.Drawing.Point(595, 7);
            btnRefreshRecords.MinimumSize = new System.Drawing.Size(1, 1);
            btnRefreshRecords.Name = "btnRefreshRecords";
            btnRefreshRecords.Size = new System.Drawing.Size(100, 30);

            btnRefreshRecords.TabIndex = 4;
            btnRefreshRecords.Text = "刷新";
            btnRefreshRecords.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            btnRefreshRecords.Click += btnRefreshRecords_Click;
            // 
            // dtpEndDate
            // 
            dtpEndDate.Font = new System.Drawing.Font("微软雅黑", 9F);
            dtpEndDate.Location = new System.Drawing.Point(430, 10);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new System.Drawing.Size(150, 27);
            dtpEndDate.TabIndex = 3;
            // 
            // lblTo
            // 
            lblTo.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblTo.Location = new System.Drawing.Point(390, 7);
            lblTo.Name = "lblTo";
            lblTo.Size = new System.Drawing.Size(30, 30);
            lblTo.TabIndex = 2;
            lblTo.Text = "至";
            lblTo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dtpStartDate
            // 
            dtpStartDate.Font = new System.Drawing.Font("微软雅黑", 9F);
            dtpStartDate.Location = new System.Drawing.Point(230, 10);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new System.Drawing.Size(150, 27);
            dtpStartDate.TabIndex = 1;
            // 
            // lblDateRange
            // 
            lblDateRange.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblDateRange.Location = new System.Drawing.Point(10, 7);
            lblDateRange.Name = "lblDateRange";
            lblDateRange.Size = new System.Drawing.Size(200, 30);
            lblDateRange.TabIndex = 0;
            lblDateRange.Text = "时间范围:";
            lblDateRange.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlRightTop
            // 
            pnlRightTop.Controls.Add(btnClose);
            pnlRightTop.Controls.Add(btnStopBrowser);
            pnlRightTop.Controls.Add(btnStartBrowser);
            pnlRightTop.Controls.Add(btnSave);
            pnlRightTop.Controls.Add(lblStatus);
            pnlRightTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlRightTop.Font = new System.Drawing.Font("微软雅黑", 12F);
            pnlRightTop.Location = new System.Drawing.Point(0, 35);
            pnlRightTop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            pnlRightTop.MinimumSize = new System.Drawing.Size(1, 1);
            pnlRightTop.Name = "pnlRightTop";
            pnlRightTop.Size = new System.Drawing.Size(994, 50);
            pnlRightTop.TabIndex = 0;
            pnlRightTop.Text = null;
            pnlRightTop.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.Font = new System.Drawing.Font("微软雅黑", 10F);
            btnClose.Location = new System.Drawing.Point(870, 10);
            btnClose.MinimumSize = new System.Drawing.Size(1, 1);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(100, 35);

            btnClose.TabIndex = 4;
            btnClose.Text = "关闭";
            btnClose.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            btnClose.Click += btnClose_Click;
            // 
            // btnStopBrowser
            // 
            btnStopBrowser.Cursor = System.Windows.Forms.Cursors.Hand;
            btnStopBrowser.Font = new System.Drawing.Font("微软雅黑", 10F);
            btnStopBrowser.Location = new System.Drawing.Point(390, 10);
            btnStopBrowser.MinimumSize = new System.Drawing.Size(1, 1);
            btnStopBrowser.Name = "btnStopBrowser";
            btnStopBrowser.Size = new System.Drawing.Size(120, 35);

            btnStopBrowser.TabIndex = 3;
            btnStopBrowser.Text = "停止浏览器";
            btnStopBrowser.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            btnStopBrowser.Click += btnStopBrowser_Click;
            // 
            // btnStartBrowser
            // 
            btnStartBrowser.Cursor = System.Windows.Forms.Cursors.Hand;
            btnStartBrowser.Font = new System.Drawing.Font("微软雅黑", 10F);
            btnStartBrowser.Location = new System.Drawing.Point(260, 10);
            btnStartBrowser.MinimumSize = new System.Drawing.Size(1, 1);
            btnStartBrowser.Name = "btnStartBrowser";
            btnStartBrowser.Size = new System.Drawing.Size(120, 35);

            btnStartBrowser.TabIndex = 2;
            btnStartBrowser.Text = "启动浏览器";
            btnStartBrowser.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            btnStartBrowser.Click += btnStartBrowser_Click;
            // 
            // btnSave
            // 
            btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSave.Font = new System.Drawing.Font("微软雅黑", 10F);
            btnSave.Location = new System.Drawing.Point(130, 10);
            btnSave.MinimumSize = new System.Drawing.Size(1, 1);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(120, 35);

            btnSave.TabIndex = 1;
            btnSave.Text = "保存配置";
            btnSave.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            btnSave.Click += btnSave_Click;
            // 
            // lblStatus
            // 
            lblStatus.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblStatus.Location = new System.Drawing.Point(10, 10);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(110, 35);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "状态: 未运行";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BetConfigManagerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1400, 800);
            Controls.Add(splitContainer);
            Font = new System.Drawing.Font("微软雅黑", 10F);
            Name = "BetConfigManagerForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "自动投注配置管理器";
            ZoomScaleRect = new System.Drawing.Rectangle(19, 19, 1400, 800);
            Load += BetConfigManagerForm_Load;
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
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
        private Sunny.UI.UITextBox txtBetScript;
        private Sunny.UI.UILabel lblBetScript;
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
    }
}

