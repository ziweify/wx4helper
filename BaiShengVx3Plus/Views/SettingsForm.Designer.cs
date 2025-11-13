namespace BaiShengVx3Plus.Views
{
    partial class SettingsForm
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
            uiTabControl1 = new Sunny.UI.UITabControl();
            tabPageSystem = new TabPage();
            grp开发模式选项 = new Sunny.UI.UIGroupBox();
            chk收单关闭时不发送系统消息 = new Sunny.UI.UICheckBox();
            chkRunModelDev = new Sunny.UI.UICheckBox();
            chkRunModeAdminSettings = new Sunny.UI.UICheckBox();
            tabPageConnection = new TabPage();
            uiGroupBox2 = new Sunny.UI.UIGroupBox();
            btnApplyReconnect = new Sunny.UI.UIButton();
            txtReconnectInterval = new Sunny.UI.UITextBox();
            uiLabel5 = new Sunny.UI.UILabel();
            chkAutoReconnect = new Sunny.UI.UICheckBox();
            uiGroupBox1 = new Sunny.UI.UIGroupBox();
            btnRefreshStatus = new Sunny.UI.UIButton();
            lblConnectionStatus = new Sunny.UI.UILabel();
            uiLabel4 = new Sunny.UI.UILabel();
            btnConnect = new Sunny.UI.UIButton();
            txtPort = new Sunny.UI.UITextBox();
            uiLabel2 = new Sunny.UI.UILabel();
            txtHost = new Sunny.UI.UITextBox();
            uiLabel1 = new Sunny.UI.UILabel();
            tabPageTest = new TabPage();
            uiGroupBox4 = new Sunny.UI.UIGroupBox();
            btnQuickSendImage = new Sunny.UI.UIButton();
            btnQuickSendMessage = new Sunny.UI.UIButton();
            btnQuickGetGroupContacts = new Sunny.UI.UIButton();
            btnQuickGetContacts = new Sunny.UI.UIButton();
            btnQuickGetUserInfo = new Sunny.UI.UIButton();
            uiGroupBox3 = new Sunny.UI.UIGroupBox();
            btnClearResult = new Sunny.UI.UIButton();
            txtResult = new RichTextBox();
            uiLabel6 = new Sunny.UI.UILabel();
            btnSendCommand = new Sunny.UI.UIButton();
            txtCommand = new Sunny.UI.UITextBox();
            uiLabel3 = new Sunny.UI.UILabel();
            btnSave = new Sunny.UI.UIButton();
            btnCancel = new Sunny.UI.UIButton();
            tbxRunDevCurrentMember = new Sunny.UI.UITextBox();
            uiLabel7 = new Sunny.UI.UILabel();
            uiLabel8 = new Sunny.UI.UILabel();
            tbxRunDevSendMessage = new Sunny.UI.UITextBox();
            btnRunDevSendCommand = new Sunny.UI.UIButton();
            uiCheckBox1 = new Sunny.UI.UICheckBox();
            uiTabControl1.SuspendLayout();
            tabPageSystem.SuspendLayout();
            grp开发模式选项.SuspendLayout();
            tabPageConnection.SuspendLayout();
            uiGroupBox2.SuspendLayout();
            uiGroupBox1.SuspendLayout();
            tabPageTest.SuspendLayout();
            uiGroupBox4.SuspendLayout();
            uiGroupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // uiTabControl1
            // 
            uiTabControl1.Controls.Add(tabPageSystem);
            uiTabControl1.Controls.Add(tabPageConnection);
            uiTabControl1.Controls.Add(tabPageTest);
            uiTabControl1.Dock = DockStyle.Top;
            uiTabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            uiTabControl1.Font = new Font("微软雅黑", 12F);
            uiTabControl1.ItemSize = new Size(150, 40);
            uiTabControl1.Location = new Point(0, 35);
            uiTabControl1.MainPage = "";
            uiTabControl1.Name = "uiTabControl1";
            uiTabControl1.SelectedIndex = 0;
            uiTabControl1.Size = new Size(800, 510);
            uiTabControl1.SizeMode = TabSizeMode.Fixed;
            uiTabControl1.TabIndex = 0;
            uiTabControl1.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            uiTabControl1.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // tabPageSystem
            // 
            tabPageSystem.BackColor = Color.White;
            tabPageSystem.Controls.Add(grp开发模式选项);
            tabPageSystem.Controls.Add(chk收单关闭时不发送系统消息);
            tabPageSystem.Controls.Add(chkRunModelDev);
            tabPageSystem.Controls.Add(uiCheckBox1);
            tabPageSystem.Controls.Add(chkRunModeAdminSettings);
            tabPageSystem.Location = new Point(0, 40);
            tabPageSystem.Name = "tabPageSystem";
            tabPageSystem.Size = new Size(800, 470);
            tabPageSystem.TabIndex = 0;
            tabPageSystem.Text = "系统设置";
            // 
            // grp开发模式选项
            // 
            grp开发模式选项.Controls.Add(btnRunDevSendCommand);
            grp开发模式选项.Controls.Add(uiLabel8);
            grp开发模式选项.Controls.Add(uiLabel7);
            grp开发模式选项.Controls.Add(tbxRunDevSendMessage);
            grp开发模式选项.Controls.Add(tbxRunDevCurrentMember);
            grp开发模式选项.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            grp开发模式选项.Location = new Point(14, 134);
            grp开发模式选项.Margin = new Padding(4, 5, 4, 5);
            grp开发模式选项.MinimumSize = new Size(1, 1);
            grp开发模式选项.Name = "grp开发模式选项";
            grp开发模式选项.Padding = new Padding(0, 32, 0, 0);
            grp开发模式选项.Size = new Size(338, 331);
            grp开发模式选项.TabIndex = 1;
            grp开发模式选项.Text = "开发模式选项";
            grp开发模式选项.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // chk收单关闭时不发送系统消息
            // 
            chk收单关闭时不发送系统消息.Font = new Font("微软雅黑", 11F);
            chk收单关闭时不发送系统消息.ForeColor = Color.FromArgb(48, 48, 48);
            chk收单关闭时不发送系统消息.Location = new Point(14, 14);
            chk收单关闭时不发送系统消息.MinimumSize = new Size(1, 1);
            chk收单关闭时不发送系统消息.Name = "chk收单关闭时不发送系统消息";
            chk收单关闭时不发送系统消息.Padding = new Padding(22, 0, 0, 0);
            chk收单关闭时不发送系统消息.Size = new Size(750, 30);
            chk收单关闭时不发送系统消息.TabIndex = 0;
            chk收单关闭时不发送系统消息.Text = "收单关闭时不发送系统消息（开盘、封盘、开奖、结算等自动消息都不发送）";
            // 
            // chkRunModelDev
            // 
            chkRunModelDev.Font = new Font("微软雅黑", 12F);
            chkRunModelDev.ForeColor = Color.FromArgb(48, 48, 48);
            chkRunModelDev.Location = new Point(14, 96);
            chkRunModelDev.MinimumSize = new Size(1, 1);
            chkRunModelDev.Name = "chkRunModelDev";
            chkRunModelDev.Padding = new Padding(22, 0, 0, 0);
            chkRunModelDev.Size = new Size(338, 30);
            chkRunModelDev.TabIndex = 0;
            chkRunModelDev.Text = "开发模式（允许手动绑定群, 模拟各项数据）";
            // 
            // chkRunModeAdminSettings
            // 
            chkRunModeAdminSettings.Font = new Font("微软雅黑", 12F);
            chkRunModeAdminSettings.ForeColor = Color.FromArgb(48, 48, 48);
            chkRunModeAdminSettings.Location = new Point(14, 60);
            chkRunModeAdminSettings.MinimumSize = new Size(1, 1);
            chkRunModeAdminSettings.Name = "chkRunModeAdminSettings";
            chkRunModeAdminSettings.Padding = new Padding(22, 0, 0, 0);
            chkRunModeAdminSettings.Size = new Size(300, 30);
            chkRunModeAdminSettings.TabIndex = 0;
            chkRunModeAdminSettings.Text = "管理模式（允许手动绑定群）";
            // 
            // tabPageConnection
            // 
            tabPageConnection.BackColor = Color.White;
            tabPageConnection.Controls.Add(uiGroupBox2);
            tabPageConnection.Controls.Add(uiGroupBox1);
            tabPageConnection.Location = new Point(0, 40);
            tabPageConnection.Name = "tabPageConnection";
            tabPageConnection.Size = new Size(200, 60);
            tabPageConnection.TabIndex = 0;
            tabPageConnection.Text = "连接设置";
            // 
            // uiGroupBox2
            // 
            uiGroupBox2.Controls.Add(btnApplyReconnect);
            uiGroupBox2.Controls.Add(txtReconnectInterval);
            uiGroupBox2.Controls.Add(uiLabel5);
            uiGroupBox2.Controls.Add(chkAutoReconnect);
            uiGroupBox2.Font = new Font("微软雅黑", 12F);
            uiGroupBox2.Location = new Point(20, 240);
            uiGroupBox2.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox2.MinimumSize = new Size(1, 1);
            uiGroupBox2.Name = "uiGroupBox2";
            uiGroupBox2.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox2.Size = new Size(760, 150);
            uiGroupBox2.TabIndex = 1;
            uiGroupBox2.Text = "自动重连设置";
            uiGroupBox2.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // btnApplyReconnect
            // 
            btnApplyReconnect.Cursor = Cursors.Hand;
            btnApplyReconnect.Font = new Font("微软雅黑", 12F);
            btnApplyReconnect.Location = new Point(620, 85);
            btnApplyReconnect.MinimumSize = new Size(1, 1);
            btnApplyReconnect.Name = "btnApplyReconnect";
            btnApplyReconnect.Size = new Size(120, 35);
            btnApplyReconnect.TabIndex = 3;
            btnApplyReconnect.Text = "应用";
            btnApplyReconnect.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnApplyReconnect.Click += btnApplyReconnect_Click;
            // 
            // txtReconnectInterval
            // 
            txtReconnectInterval.Cursor = Cursors.IBeam;
            txtReconnectInterval.DoubleValue = 5000D;
            txtReconnectInterval.Font = new Font("微软雅黑", 12F);
            txtReconnectInterval.IntValue = 5000;
            txtReconnectInterval.Location = new Point(220, 90);
            txtReconnectInterval.Margin = new Padding(4, 5, 4, 5);
            txtReconnectInterval.MinimumSize = new Size(1, 16);
            txtReconnectInterval.Name = "txtReconnectInterval";
            txtReconnectInterval.Padding = new Padding(5);
            txtReconnectInterval.ShowText = false;
            txtReconnectInterval.Size = new Size(200, 29);
            txtReconnectInterval.TabIndex = 2;
            txtReconnectInterval.Text = "5000";
            txtReconnectInterval.TextAlignment = ContentAlignment.MiddleLeft;
            txtReconnectInterval.Watermark = "毫秒";
            // 
            // uiLabel5
            // 
            uiLabel5.Font = new Font("微软雅黑", 12F);
            uiLabel5.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel5.Location = new Point(30, 90);
            uiLabel5.Name = "uiLabel5";
            uiLabel5.Size = new Size(180, 23);
            uiLabel5.TabIndex = 1;
            uiLabel5.Text = "重连间隔 (毫秒):";
            uiLabel5.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkAutoReconnect
            // 
            chkAutoReconnect.Cursor = Cursors.Hand;
            chkAutoReconnect.Font = new Font("微软雅黑", 12F);
            chkAutoReconnect.ForeColor = Color.FromArgb(48, 48, 48);
            chkAutoReconnect.Location = new Point(30, 45);
            chkAutoReconnect.MinimumSize = new Size(1, 1);
            chkAutoReconnect.Name = "chkAutoReconnect";
            chkAutoReconnect.Padding = new Padding(22, 0, 0, 0);
            chkAutoReconnect.Size = new Size(300, 29);
            chkAutoReconnect.TabIndex = 0;
            chkAutoReconnect.Text = "启用自动重连";
            chkAutoReconnect.CheckedChanged += chkAutoReconnect_CheckedChanged;
            // 
            // uiGroupBox1
            // 
            uiGroupBox1.Controls.Add(btnRefreshStatus);
            uiGroupBox1.Controls.Add(lblConnectionStatus);
            uiGroupBox1.Controls.Add(uiLabel4);
            uiGroupBox1.Controls.Add(btnConnect);
            uiGroupBox1.Controls.Add(txtPort);
            uiGroupBox1.Controls.Add(uiLabel2);
            uiGroupBox1.Controls.Add(txtHost);
            uiGroupBox1.Controls.Add(uiLabel1);
            uiGroupBox1.Font = new Font("微软雅黑", 12F);
            uiGroupBox1.Location = new Point(20, 20);
            uiGroupBox1.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox1.MinimumSize = new Size(1, 1);
            uiGroupBox1.Name = "uiGroupBox1";
            uiGroupBox1.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox1.Size = new Size(760, 200);
            uiGroupBox1.TabIndex = 0;
            uiGroupBox1.Text = "服务器设置";
            uiGroupBox1.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // btnRefreshStatus
            // 
            btnRefreshStatus.Cursor = Cursors.Hand;
            btnRefreshStatus.Font = new Font("微软雅黑", 12F);
            btnRefreshStatus.Location = new Point(480, 140);
            btnRefreshStatus.MinimumSize = new Size(1, 1);
            btnRefreshStatus.Name = "btnRefreshStatus";
            btnRefreshStatus.Size = new Size(120, 35);
            btnRefreshStatus.TabIndex = 7;
            btnRefreshStatus.Text = "刷新状态";
            btnRefreshStatus.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnRefreshStatus.Click += btnRefreshStatus_Click;
            // 
            // lblConnectionStatus
            // 
            lblConnectionStatus.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            lblConnectionStatus.ForeColor = Color.FromArgb(48, 48, 48);
            lblConnectionStatus.Location = new Point(160, 145);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.Size = new Size(300, 23);
            lblConnectionStatus.TabIndex = 6;
            lblConnectionStatus.Text = "未连接 ✗";
            lblConnectionStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // uiLabel4
            // 
            uiLabel4.Font = new Font("微软雅黑", 12F);
            uiLabel4.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel4.Location = new Point(30, 145);
            uiLabel4.Name = "uiLabel4";
            uiLabel4.Size = new Size(120, 23);
            uiLabel4.TabIndex = 5;
            uiLabel4.Text = "连接状态:";
            uiLabel4.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnConnect
            // 
            btnConnect.Cursor = Cursors.Hand;
            btnConnect.Font = new Font("微软雅黑", 12F);
            btnConnect.Location = new Point(620, 140);
            btnConnect.MinimumSize = new Size(1, 1);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(120, 35);
            btnConnect.TabIndex = 4;
            btnConnect.Text = "连接";
            btnConnect.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnConnect.Click += btnConnect_Click;
            // 
            // txtPort
            // 
            txtPort.Cursor = Cursors.IBeam;
            txtPort.DoubleValue = 6328D;
            txtPort.Font = new Font("微软雅黑", 12F);
            txtPort.IntValue = 6328;
            txtPort.Location = new Point(160, 90);
            txtPort.Margin = new Padding(4, 5, 4, 5);
            txtPort.MinimumSize = new Size(1, 16);
            txtPort.Name = "txtPort";
            txtPort.Padding = new Padding(5);
            txtPort.ShowText = false;
            txtPort.Size = new Size(200, 29);
            txtPort.TabIndex = 3;
            txtPort.Text = "6328";
            txtPort.TextAlignment = ContentAlignment.MiddleLeft;
            txtPort.Watermark = "端口号";
            // 
            // uiLabel2
            // 
            uiLabel2.Font = new Font("微软雅黑", 12F);
            uiLabel2.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel2.Location = new Point(30, 90);
            uiLabel2.Name = "uiLabel2";
            uiLabel2.Size = new Size(120, 23);
            uiLabel2.TabIndex = 2;
            uiLabel2.Text = "端口:";
            uiLabel2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtHost
            // 
            txtHost.Cursor = Cursors.IBeam;
            txtHost.Font = new Font("微软雅黑", 12F);
            txtHost.Location = new Point(160, 45);
            txtHost.Margin = new Padding(4, 5, 4, 5);
            txtHost.MinimumSize = new Size(1, 16);
            txtHost.Name = "txtHost";
            txtHost.Padding = new Padding(5);
            txtHost.ShowText = false;
            txtHost.Size = new Size(400, 29);
            txtHost.TabIndex = 1;
            txtHost.Text = "127.0.0.1";
            txtHost.TextAlignment = ContentAlignment.MiddleLeft;
            txtHost.Watermark = "服务器地址";
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("微软雅黑", 12F);
            uiLabel1.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel1.Location = new Point(30, 45);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(120, 23);
            uiLabel1.TabIndex = 0;
            uiLabel1.Text = "服务器地址:";
            uiLabel1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // tabPageTest
            // 
            tabPageTest.BackColor = Color.White;
            tabPageTest.Controls.Add(uiGroupBox4);
            tabPageTest.Controls.Add(uiGroupBox3);
            tabPageTest.Location = new Point(0, 40);
            tabPageTest.Name = "tabPageTest";
            tabPageTest.Size = new Size(200, 60);
            tabPageTest.TabIndex = 1;
            tabPageTest.Text = "命令测试";
            // 
            // uiGroupBox4
            // 
            uiGroupBox4.Controls.Add(btnQuickSendImage);
            uiGroupBox4.Controls.Add(btnQuickSendMessage);
            uiGroupBox4.Controls.Add(btnQuickGetGroupContacts);
            uiGroupBox4.Controls.Add(btnQuickGetContacts);
            uiGroupBox4.Controls.Add(btnQuickGetUserInfo);
            uiGroupBox4.Font = new Font("微软雅黑", 12F);
            uiGroupBox4.Location = new Point(20, 20);
            uiGroupBox4.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox4.MinimumSize = new Size(1, 1);
            uiGroupBox4.Name = "uiGroupBox4";
            uiGroupBox4.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox4.Size = new Size(760, 100);
            uiGroupBox4.TabIndex = 1;
            uiGroupBox4.Text = "快捷命令";
            uiGroupBox4.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // btnQuickSendImage
            // 
            btnQuickSendImage.Cursor = Cursors.Hand;
            btnQuickSendImage.Font = new Font("微软雅黑", 12F);
            btnQuickSendImage.Location = new Point(100, 68);
            btnQuickSendImage.MinimumSize = new Size(1, 1);
            btnQuickSendImage.Name = "btnQuickSendImage";
            btnQuickSendImage.Size = new Size(80, 29);
            btnQuickSendImage.TabIndex = 2;
            btnQuickSendImage.Tag = "SendImage(\"wxid\", \"d:/1.png\")";
            btnQuickSendImage.Text = "发送图片";
            btnQuickSendImage.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnQuickSendImage.Click += btnQuickCommand_Click;
            // 
            // btnQuickSendMessage
            // 
            btnQuickSendMessage.Cursor = Cursors.Hand;
            btnQuickSendMessage.Font = new Font("微软雅黑", 12F);
            btnQuickSendMessage.Location = new Point(14, 68);
            btnQuickSendMessage.MinimumSize = new Size(1, 1);
            btnQuickSendMessage.Name = "btnQuickSendMessage";
            btnQuickSendMessage.Size = new Size(80, 29);
            btnQuickSendMessage.TabIndex = 2;
            btnQuickSendMessage.Tag = "SendMessage(\"wxid\", \"Hello\")";
            btnQuickSendMessage.Text = "发送消息";
            btnQuickSendMessage.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnQuickSendMessage.Click += btnQuickCommand_Click;
            // 
            // btnQuickGetGroupContacts
            // 
            btnQuickGetGroupContacts.Cursor = Cursors.Hand;
            btnQuickGetGroupContacts.Font = new Font("微软雅黑", 12F);
            btnQuickGetGroupContacts.Location = new Point(280, 35);
            btnQuickGetGroupContacts.MinimumSize = new Size(1, 1);
            btnQuickGetGroupContacts.Name = "btnQuickGetGroupContacts";
            btnQuickGetGroupContacts.Size = new Size(131, 29);
            btnQuickGetGroupContacts.TabIndex = 1;
            btnQuickGetGroupContacts.Tag = "GetGroupContacts()";
            btnQuickGetGroupContacts.Text = "获取群成员列表";
            btnQuickGetGroupContacts.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnQuickGetGroupContacts.Click += btnQuickCommand_Click;
            // 
            // btnQuickGetContacts
            // 
            btnQuickGetContacts.Cursor = Cursors.Hand;
            btnQuickGetContacts.Font = new Font("微软雅黑", 12F);
            btnQuickGetContacts.Location = new Point(143, 35);
            btnQuickGetContacts.MinimumSize = new Size(1, 1);
            btnQuickGetContacts.Name = "btnQuickGetContacts";
            btnQuickGetContacts.Size = new Size(131, 29);
            btnQuickGetContacts.TabIndex = 1;
            btnQuickGetContacts.Tag = "GetContacts()";
            btnQuickGetContacts.Text = "获取联系人列表";
            btnQuickGetContacts.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnQuickGetContacts.Click += btnQuickCommand_Click;
            // 
            // btnQuickGetUserInfo
            // 
            btnQuickGetUserInfo.Cursor = Cursors.Hand;
            btnQuickGetUserInfo.Font = new Font("微软雅黑", 12F);
            btnQuickGetUserInfo.Location = new Point(14, 35);
            btnQuickGetUserInfo.MinimumSize = new Size(1, 1);
            btnQuickGetUserInfo.Name = "btnQuickGetUserInfo";
            btnQuickGetUserInfo.Size = new Size(123, 29);
            btnQuickGetUserInfo.TabIndex = 0;
            btnQuickGetUserInfo.Tag = "GetUserInfo()";
            btnQuickGetUserInfo.Text = "获取用户信息";
            btnQuickGetUserInfo.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnQuickGetUserInfo.Click += btnQuickCommand_Click;
            // 
            // uiGroupBox3
            // 
            uiGroupBox3.Controls.Add(btnClearResult);
            uiGroupBox3.Controls.Add(txtResult);
            uiGroupBox3.Controls.Add(uiLabel6);
            uiGroupBox3.Controls.Add(btnSendCommand);
            uiGroupBox3.Controls.Add(txtCommand);
            uiGroupBox3.Controls.Add(uiLabel3);
            uiGroupBox3.Font = new Font("微软雅黑", 12F);
            uiGroupBox3.Location = new Point(20, 140);
            uiGroupBox3.Margin = new Padding(4, 5, 4, 5);
            uiGroupBox3.MinimumSize = new Size(1, 1);
            uiGroupBox3.Name = "uiGroupBox3";
            uiGroupBox3.Padding = new Padding(0, 32, 0, 0);
            uiGroupBox3.Size = new Size(760, 310);
            uiGroupBox3.TabIndex = 0;
            uiGroupBox3.Text = "Socket 命令测试";
            uiGroupBox3.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // btnClearResult
            // 
            btnClearResult.Cursor = Cursors.Hand;
            btnClearResult.Font = new Font("微软雅黑", 12F);
            btnClearResult.Location = new Point(620, 95);
            btnClearResult.MinimumSize = new Size(1, 1);
            btnClearResult.Name = "btnClearResult";
            btnClearResult.Size = new Size(120, 35);
            btnClearResult.TabIndex = 5;
            btnClearResult.Text = "清空结果";
            btnClearResult.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClearResult.Click += btnClearResult_Click;
            // 
            // txtResult
            // 
            txtResult.BackColor = Color.FromArgb(243, 249, 255);
            txtResult.BorderStyle = BorderStyle.FixedSingle;
            txtResult.Font = new Font("Consolas", 10F);
            txtResult.Location = new Point(30, 140);
            txtResult.Name = "txtResult";
            txtResult.ReadOnly = true;
            txtResult.Size = new Size(710, 150);
            txtResult.TabIndex = 4;
            txtResult.Text = "";
            // 
            // uiLabel6
            // 
            uiLabel6.Font = new Font("微软雅黑", 12F);
            uiLabel6.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel6.Location = new Point(30, 100);
            uiLabel6.Name = "uiLabel6";
            uiLabel6.Size = new Size(120, 23);
            uiLabel6.TabIndex = 3;
            uiLabel6.Text = "执行结果:";
            uiLabel6.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnSendCommand
            // 
            btnSendCommand.Cursor = Cursors.Hand;
            btnSendCommand.Font = new Font("微软雅黑", 12F);
            btnSendCommand.Location = new Point(620, 40);
            btnSendCommand.MinimumSize = new Size(1, 1);
            btnSendCommand.Name = "btnSendCommand";
            btnSendCommand.Size = new Size(120, 35);
            btnSendCommand.TabIndex = 2;
            btnSendCommand.Text = "发送";
            btnSendCommand.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSendCommand.Click += btnSendCommand_Click;
            // 
            // txtCommand
            // 
            txtCommand.Cursor = Cursors.IBeam;
            txtCommand.Font = new Font("Consolas", 12F);
            txtCommand.Location = new Point(120, 45);
            txtCommand.Margin = new Padding(4, 5, 4, 5);
            txtCommand.MinimumSize = new Size(1, 16);
            txtCommand.Name = "txtCommand";
            txtCommand.Padding = new Padding(5);
            txtCommand.ShowText = false;
            txtCommand.Size = new Size(480, 29);
            txtCommand.TabIndex = 1;
            txtCommand.TextAlignment = ContentAlignment.MiddleLeft;
            txtCommand.Watermark = "例如: GetUserInfo() 或 SendMessage(\"wxid\", \"Hello\")";
            // 
            // uiLabel3
            // 
            uiLabel3.Font = new Font("微软雅黑", 12F);
            uiLabel3.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel3.Location = new Point(30, 45);
            uiLabel3.Name = "uiLabel3";
            uiLabel3.Size = new Size(80, 23);
            uiLabel3.TabIndex = 0;
            uiLabel3.Text = "命令:";
            uiLabel3.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnSave
            // 
            btnSave.Cursor = Cursors.Hand;
            btnSave.Font = new Font("微软雅黑", 12F);
            btnSave.Location = new Point(550, 560);
            btnSave.MinimumSize = new Size(1, 1);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 35);
            btnSave.TabIndex = 1;
            btnSave.Text = "保存";
            btnSave.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Font = new Font("微软雅黑", 12F);
            btnCancel.Location = new Point(680, 560);
            btnCancel.MinimumSize = new Size(1, 1);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 35);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "关闭";
            btnCancel.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnCancel.Click += btnCancel_Click;
            // 
            // tbxRunDevCurrentMember
            // 
            tbxRunDevCurrentMember.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            tbxRunDevCurrentMember.Location = new Point(104, 37);
            tbxRunDevCurrentMember.Margin = new Padding(4, 5, 4, 5);
            tbxRunDevCurrentMember.MinimumSize = new Size(1, 16);
            tbxRunDevCurrentMember.Name = "tbxRunDevCurrentMember";
            tbxRunDevCurrentMember.Padding = new Padding(5);
            tbxRunDevCurrentMember.ShowText = false;
            tbxRunDevCurrentMember.Size = new Size(219, 29);
            tbxRunDevCurrentMember.TabIndex = 0;
            tbxRunDevCurrentMember.TextAlignment = ContentAlignment.MiddleLeft;
            tbxRunDevCurrentMember.Watermark = "";
            // 
            // uiLabel7
            // 
            uiLabel7.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel7.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel7.Location = new Point(15, 42);
            uiLabel7.Name = "uiLabel7";
            uiLabel7.Size = new Size(82, 23);
            uiLabel7.TabIndex = 1;
            uiLabel7.Text = "当前会员";
            // 
            // uiLabel8
            // 
            uiLabel8.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiLabel8.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel8.Location = new Point(15, 79);
            uiLabel8.Name = "uiLabel8";
            uiLabel8.Size = new Size(82, 23);
            uiLabel8.TabIndex = 1;
            uiLabel8.Text = "消息内容";
            // 
            // tbxRunDevSendMessage
            // 
            tbxRunDevSendMessage.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            tbxRunDevSendMessage.Location = new Point(104, 76);
            tbxRunDevSendMessage.Margin = new Padding(4, 5, 4, 5);
            tbxRunDevSendMessage.MinimumSize = new Size(1, 16);
            tbxRunDevSendMessage.Name = "tbxRunDevSendMessage";
            tbxRunDevSendMessage.Padding = new Padding(5);
            tbxRunDevSendMessage.ShowText = false;
            tbxRunDevSendMessage.Size = new Size(219, 29);
            tbxRunDevSendMessage.TabIndex = 0;
            tbxRunDevSendMessage.TextAlignment = ContentAlignment.MiddleLeft;
            tbxRunDevSendMessage.Watermark = "";
            // 
            // btnRunDevSendCommand
            // 
            btnRunDevSendCommand.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnRunDevSendCommand.Location = new Point(244, 113);
            btnRunDevSendCommand.MinimumSize = new Size(1, 1);
            btnRunDevSendCommand.Name = "btnRunDevSendCommand";
            btnRunDevSendCommand.Size = new Size(79, 31);
            btnRunDevSendCommand.TabIndex = 2;
            btnRunDevSendCommand.Text = "发送";
            btnRunDevSendCommand.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // uiCheckBox1
            // 
            uiCheckBox1.Font = new Font("微软雅黑", 12F);
            uiCheckBox1.ForeColor = Color.FromArgb(48, 48, 48);
            uiCheckBox1.Location = new Point(14, 24);
            uiCheckBox1.MinimumSize = new Size(1, 1);
            uiCheckBox1.Name = "uiCheckBox1";
            uiCheckBox1.Padding = new Padding(22, 0, 0, 0);
            uiCheckBox1.Size = new Size(300, 30);
            uiCheckBox1.TabIndex = 0;
            uiCheckBox1.Text = "老板锁";
            // 
            // SettingsForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(800, 610);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(uiTabControl1);
            MaximizeBox = false;
            Name = "SettingsForm";
            Text = "设置";
            ZoomScaleRect = new Rectangle(15, 15, 800, 610);
            uiTabControl1.ResumeLayout(false);
            tabPageSystem.ResumeLayout(false);
            grp开发模式选项.ResumeLayout(false);
            tabPageConnection.ResumeLayout(false);
            uiGroupBox2.ResumeLayout(false);
            uiGroupBox1.ResumeLayout(false);
            tabPageTest.ResumeLayout(false);
            uiGroupBox4.ResumeLayout(false);
            uiGroupBox3.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITabControl uiTabControl1;
        private System.Windows.Forms.TabPage tabPageConnection;
        private System.Windows.Forms.TabPage tabPageTest;
        private Sunny.UI.UIGroupBox uiGroupBox1;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UITextBox txtHost;
        private Sunny.UI.UITextBox txtPort;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UIButton btnConnect;
        private Sunny.UI.UILabel lblConnectionStatus;
        private Sunny.UI.UILabel uiLabel4;
        private Sunny.UI.UIButton btnRefreshStatus;
        private Sunny.UI.UIGroupBox uiGroupBox2;
        private Sunny.UI.UICheckBox chkAutoReconnect;
        private Sunny.UI.UILabel uiLabel5;
        private Sunny.UI.UITextBox txtReconnectInterval;
        private Sunny.UI.UIButton btnApplyReconnect;
        private Sunny.UI.UIGroupBox uiGroupBox3;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UITextBox txtCommand;
        private Sunny.UI.UIButton btnSendCommand;
        private Sunny.UI.UILabel uiLabel6;
        private System.Windows.Forms.RichTextBox txtResult;
        private Sunny.UI.UIButton btnClearResult;
        private Sunny.UI.UIGroupBox uiGroupBox4;
        private Sunny.UI.UIButton btnQuickGetUserInfo;
        private Sunny.UI.UIButton btnQuickGetContacts;
        private Sunny.UI.UIButton btnQuickSendMessage;
        private Sunny.UI.UIButton btnSave;
        private Sunny.UI.UIButton btnCancel;
        private Sunny.UI.UIButton btnQuickSendImage;
        private Sunny.UI.UIButton btnQuickGetGroupContacts;
        private TabPage tabPageSystem; // 🔥 系统设置选项卡
        private Sunny.UI.UICheckBox chk收单关闭时不发送系统消息; // 🔥 收单关闭时不发送系统消息checkbox
        private Sunny.UI.UICheckBox chkRunModeAdminSettings; // 🔥 管理模式checkbox
        private Sunny.UI.UICheckBox chkRunModelDev;
        private Sunny.UI.UIGroupBox grp开发模式选项;
        private Sunny.UI.UILabel uiLabel7;
        private Sunny.UI.UITextBox tbxRunDevCurrentMember;
        private Sunny.UI.UILabel uiLabel8;
        private Sunny.UI.UITextBox tbxRunDevSendMessage;
        private Sunny.UI.UIButton btnRunDevSendCommand;
        private Sunny.UI.UICheckBox uiCheckBox1;
    }
}

