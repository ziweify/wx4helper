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
            this.uiTabControl1 = new Sunny.UI.UITabControl();
            this.tabPageConnection = new System.Windows.Forms.TabPage();
            this.uiGroupBox1 = new Sunny.UI.UIGroupBox();
            this.btnRefreshStatus = new Sunny.UI.UIButton();
            this.lblConnectionStatus = new Sunny.UI.UILabel();
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.btnConnect = new Sunny.UI.UIButton();
            this.txtPort = new Sunny.UI.UITextBox();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.txtHost = new Sunny.UI.UITextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiGroupBox2 = new Sunny.UI.UIGroupBox();
            this.btnApplyReconnect = new Sunny.UI.UIButton();
            this.txtReconnectInterval = new Sunny.UI.UITextBox();
            this.uiLabel5 = new Sunny.UI.UILabel();
            this.chkAutoReconnect = new Sunny.UI.UICheckBox();
            this.tabPageTest = new System.Windows.Forms.TabPage();
            this.uiGroupBox3 = new Sunny.UI.UIGroupBox();
            this.btnClearResult = new Sunny.UI.UIButton();
            this.txtResult = new System.Windows.Forms.RichTextBox();
            this.uiLabel6 = new Sunny.UI.UILabel();
            this.btnSendCommand = new Sunny.UI.UIButton();
            this.txtCommand = new Sunny.UI.UITextBox();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.uiGroupBox4 = new Sunny.UI.UIGroupBox();
            this.btnQuickGetUserInfo = new Sunny.UI.UIButton();
            this.btnQuickGetContacts = new Sunny.UI.UIButton();
            this.btnQuickSendMessage = new Sunny.UI.UIButton();
            this.btnSave = new Sunny.UI.UIButton();
            this.btnCancel = new Sunny.UI.UIButton();
            this.uiTabControl1.SuspendLayout();
            this.tabPageConnection.SuspendLayout();
            this.uiGroupBox1.SuspendLayout();
            this.uiGroupBox2.SuspendLayout();
            this.tabPageTest.SuspendLayout();
            this.uiGroupBox3.SuspendLayout();
            this.uiGroupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiTabControl1
            // 
            this.uiTabControl1.Controls.Add(this.tabPageConnection);
            this.uiTabControl1.Controls.Add(this.tabPageTest);
            this.uiTabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiTabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.uiTabControl1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiTabControl1.ItemSize = new System.Drawing.Size(150, 40);
            this.uiTabControl1.Location = new System.Drawing.Point(0, 35);
            this.uiTabControl1.MainPage = "";
            this.uiTabControl1.Name = "uiTabControl1";
            this.uiTabControl1.SelectedIndex = 0;
            this.uiTabControl1.Size = new System.Drawing.Size(800, 510);
            this.uiTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.uiTabControl1.TabIndex = 0;
            // 
            // tabPageConnection
            // 
            this.tabPageConnection.BackColor = System.Drawing.Color.White;
            this.tabPageConnection.Controls.Add(this.uiGroupBox2);
            this.tabPageConnection.Controls.Add(this.uiGroupBox1);
            this.tabPageConnection.Location = new System.Drawing.Point(0, 40);
            this.tabPageConnection.Name = "tabPageConnection";
            this.tabPageConnection.Size = new System.Drawing.Size(800, 470);
            this.tabPageConnection.TabIndex = 0;
            this.tabPageConnection.Text = "连接设置";
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.btnRefreshStatus);
            this.uiGroupBox1.Controls.Add(this.lblConnectionStatus);
            this.uiGroupBox1.Controls.Add(this.uiLabel4);
            this.uiGroupBox1.Controls.Add(this.btnConnect);
            this.uiGroupBox1.Controls.Add(this.txtPort);
            this.uiGroupBox1.Controls.Add(this.uiLabel2);
            this.uiGroupBox1.Controls.Add(this.txtHost);
            this.uiGroupBox1.Controls.Add(this.uiLabel1);
            this.uiGroupBox1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiGroupBox1.Location = new System.Drawing.Point(20, 20);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.uiGroupBox1.Size = new System.Drawing.Size(760, 200);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "服务器设置";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnRefreshStatus
            // 
            this.btnRefreshStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefreshStatus.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnRefreshStatus.Location = new System.Drawing.Point(480, 140);
            this.btnRefreshStatus.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnRefreshStatus.Name = "btnRefreshStatus";
            this.btnRefreshStatus.Size = new System.Drawing.Size(120, 35);
            this.btnRefreshStatus.TabIndex = 7;
            this.btnRefreshStatus.Text = "刷新状态";
            this.btnRefreshStatus.Click += new System.EventHandler(this.btnRefreshStatus_Click);
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.lblConnectionStatus.Location = new System.Drawing.Point(160, 145);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(300, 23);
            this.lblConnectionStatus.TabIndex = 6;
            this.lblConnectionStatus.Text = "未连接 ✗";
            this.lblConnectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel4
            // 
            this.uiLabel4.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel4.Location = new System.Drawing.Point(30, 145);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(120, 23);
            this.uiLabel4.TabIndex = 5;
            this.uiLabel4.Text = "连接状态:";
            this.uiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnConnect
            // 
            this.btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConnect.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnConnect.Location = new System.Drawing.Point(620, 140);
            this.btnConnect.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(120, 35);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "连接";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtPort
            // 
            this.txtPort.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPort.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtPort.Location = new System.Drawing.Point(160, 90);
            this.txtPort.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPort.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtPort.Name = "txtPort";
            this.txtPort.ShowText = false;
            this.txtPort.Size = new System.Drawing.Size(200, 29);
            this.txtPort.TabIndex = 3;
            this.txtPort.Text = "6328";
            this.txtPort.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtPort.Watermark = "端口号";
            // 
            // uiLabel2
            // 
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel2.Location = new System.Drawing.Point(30, 90);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(120, 23);
            this.uiLabel2.TabIndex = 2;
            this.uiLabel2.Text = "端口:";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtHost
            // 
            this.txtHost.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtHost.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtHost.Location = new System.Drawing.Point(160, 45);
            this.txtHost.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtHost.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtHost.Name = "txtHost";
            this.txtHost.ShowText = false;
            this.txtHost.Size = new System.Drawing.Size(400, 29);
            this.txtHost.TabIndex = 1;
            this.txtHost.Text = "127.0.0.1";
            this.txtHost.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtHost.Watermark = "服务器地址";
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel1.Location = new System.Drawing.Point(30, 45);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(120, 23);
            this.uiLabel1.TabIndex = 0;
            this.uiLabel1.Text = "服务器地址:";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.btnApplyReconnect);
            this.uiGroupBox2.Controls.Add(this.txtReconnectInterval);
            this.uiGroupBox2.Controls.Add(this.uiLabel5);
            this.uiGroupBox2.Controls.Add(this.chkAutoReconnect);
            this.uiGroupBox2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiGroupBox2.Location = new System.Drawing.Point(20, 240);
            this.uiGroupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.uiGroupBox2.Size = new System.Drawing.Size(760, 150);
            this.uiGroupBox2.TabIndex = 1;
            this.uiGroupBox2.Text = "自动重连设置";
            this.uiGroupBox2.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnApplyReconnect
            // 
            this.btnApplyReconnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApplyReconnect.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnApplyReconnect.Location = new System.Drawing.Point(620, 85);
            this.btnApplyReconnect.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnApplyReconnect.Name = "btnApplyReconnect";
            this.btnApplyReconnect.Size = new System.Drawing.Size(120, 35);
            this.btnApplyReconnect.TabIndex = 3;
            this.btnApplyReconnect.Text = "应用";
            this.btnApplyReconnect.Click += new System.EventHandler(this.btnApplyReconnect_Click);
            // 
            // txtReconnectInterval
            // 
            this.txtReconnectInterval.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtReconnectInterval.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtReconnectInterval.Location = new System.Drawing.Point(220, 90);
            this.txtReconnectInterval.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtReconnectInterval.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtReconnectInterval.Name = "txtReconnectInterval";
            this.txtReconnectInterval.ShowText = false;
            this.txtReconnectInterval.Size = new System.Drawing.Size(200, 29);
            this.txtReconnectInterval.TabIndex = 2;
            this.txtReconnectInterval.Text = "5000";
            this.txtReconnectInterval.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtReconnectInterval.Watermark = "毫秒";
            // 
            // uiLabel5
            // 
            this.uiLabel5.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel5.Location = new System.Drawing.Point(30, 90);
            this.uiLabel5.Name = "uiLabel5";
            this.uiLabel5.Size = new System.Drawing.Size(180, 23);
            this.uiLabel5.TabIndex = 1;
            this.uiLabel5.Text = "重连间隔 (毫秒):";
            this.uiLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkAutoReconnect
            // 
            this.chkAutoReconnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkAutoReconnect.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.chkAutoReconnect.Location = new System.Drawing.Point(30, 45);
            this.chkAutoReconnect.MinimumSize = new System.Drawing.Size(1, 1);
            this.chkAutoReconnect.Name = "chkAutoReconnect";
            this.chkAutoReconnect.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.chkAutoReconnect.Size = new System.Drawing.Size(300, 29);
            this.chkAutoReconnect.TabIndex = 0;
            this.chkAutoReconnect.Text = "启用自动重连";
            this.chkAutoReconnect.CheckedChanged += new System.EventHandler(this.chkAutoReconnect_CheckedChanged);
            // 
            // tabPageTest
            // 
            this.tabPageTest.BackColor = System.Drawing.Color.White;
            this.tabPageTest.Controls.Add(this.uiGroupBox4);
            this.tabPageTest.Controls.Add(this.uiGroupBox3);
            this.tabPageTest.Location = new System.Drawing.Point(0, 40);
            this.tabPageTest.Name = "tabPageTest";
            this.tabPageTest.Size = new System.Drawing.Size(800, 470);
            this.tabPageTest.TabIndex = 1;
            this.tabPageTest.Text = "命令测试";
            // 
            // uiGroupBox3
            // 
            this.uiGroupBox3.Controls.Add(this.btnClearResult);
            this.uiGroupBox3.Controls.Add(this.txtResult);
            this.uiGroupBox3.Controls.Add(this.uiLabel6);
            this.uiGroupBox3.Controls.Add(this.btnSendCommand);
            this.uiGroupBox3.Controls.Add(this.txtCommand);
            this.uiGroupBox3.Controls.Add(this.uiLabel3);
            this.uiGroupBox3.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiGroupBox3.Location = new System.Drawing.Point(20, 140);
            this.uiGroupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox3.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox3.Name = "uiGroupBox3";
            this.uiGroupBox3.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.uiGroupBox3.Size = new System.Drawing.Size(760, 310);
            this.uiGroupBox3.TabIndex = 0;
            this.uiGroupBox3.Text = "Socket 命令测试";
            this.uiGroupBox3.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnClearResult
            // 
            this.btnClearResult.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearResult.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnClearResult.Location = new System.Drawing.Point(620, 95);
            this.btnClearResult.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnClearResult.Name = "btnClearResult";
            this.btnClearResult.Size = new System.Drawing.Size(120, 35);
            this.btnClearResult.TabIndex = 5;
            this.btnClearResult.Text = "清空结果";
            this.btnClearResult.Click += new System.EventHandler(this.btnClearResult_Click);
            // 
            // txtResult
            // 
            this.txtResult.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.txtResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtResult.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtResult.Location = new System.Drawing.Point(30, 140);
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.Size = new System.Drawing.Size(710, 150);
            this.txtResult.TabIndex = 4;
            this.txtResult.Text = "";
            // 
            // uiLabel6
            // 
            this.uiLabel6.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel6.Location = new System.Drawing.Point(30, 100);
            this.uiLabel6.Name = "uiLabel6";
            this.uiLabel6.Size = new System.Drawing.Size(120, 23);
            this.uiLabel6.TabIndex = 3;
            this.uiLabel6.Text = "执行结果:";
            this.uiLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSendCommand
            // 
            this.btnSendCommand.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSendCommand.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnSendCommand.Location = new System.Drawing.Point(620, 40);
            this.btnSendCommand.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSendCommand.Name = "btnSendCommand";
            this.btnSendCommand.Size = new System.Drawing.Size(120, 35);
            this.btnSendCommand.TabIndex = 2;
            this.btnSendCommand.Text = "发送";
            this.btnSendCommand.Click += new System.EventHandler(this.btnSendCommand_Click);
            // 
            // txtCommand
            // 
            this.txtCommand.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtCommand.Font = new System.Drawing.Font("Consolas", 12F);
            this.txtCommand.Location = new System.Drawing.Point(120, 45);
            this.txtCommand.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCommand.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.ShowText = false;
            this.txtCommand.Size = new System.Drawing.Size(480, 29);
            this.txtCommand.TabIndex = 1;
            this.txtCommand.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtCommand.Watermark = "例如: GetUserInfo() 或 SendMessage(\"wxid\", \"Hello\")";
            // 
            // uiLabel3
            // 
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel3.Location = new System.Drawing.Point(30, 45);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(80, 23);
            this.uiLabel3.TabIndex = 0;
            this.uiLabel3.Text = "命令:";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // uiGroupBox4
            // 
            this.uiGroupBox4.Controls.Add(this.btnQuickSendMessage);
            this.uiGroupBox4.Controls.Add(this.btnQuickGetContacts);
            this.uiGroupBox4.Controls.Add(this.btnQuickGetUserInfo);
            this.uiGroupBox4.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiGroupBox4.Location = new System.Drawing.Point(20, 20);
            this.uiGroupBox4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox4.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox4.Name = "uiGroupBox4";
            this.uiGroupBox4.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.uiGroupBox4.Size = new System.Drawing.Size(760, 100);
            this.uiGroupBox4.TabIndex = 1;
            this.uiGroupBox4.Text = "快捷命令";
            this.uiGroupBox4.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnQuickGetUserInfo
            // 
            this.btnQuickGetUserInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuickGetUserInfo.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnQuickGetUserInfo.Location = new System.Drawing.Point(30, 45);
            this.btnQuickGetUserInfo.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnQuickGetUserInfo.Name = "btnQuickGetUserInfo";
            this.btnQuickGetUserInfo.Size = new System.Drawing.Size(150, 35);
            this.btnQuickGetUserInfo.TabIndex = 0;
            this.btnQuickGetUserInfo.Tag = "GetUserInfo()";
            this.btnQuickGetUserInfo.Text = "获取用户信息";
            this.btnQuickGetUserInfo.Click += new System.EventHandler(this.btnQuickCommand_Click);
            // 
            // btnQuickGetContacts
            // 
            this.btnQuickGetContacts.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuickGetContacts.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnQuickGetContacts.Location = new System.Drawing.Point(200, 45);
            this.btnQuickGetContacts.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnQuickGetContacts.Name = "btnQuickGetContacts";
            this.btnQuickGetContacts.Size = new System.Drawing.Size(150, 35);
            this.btnQuickGetContacts.TabIndex = 1;
            this.btnQuickGetContacts.Tag = "GetContacts()";
            this.btnQuickGetContacts.Text = "获取联系人列表";
            this.btnQuickGetContacts.Click += new System.EventHandler(this.btnQuickCommand_Click);
            // 
            // btnQuickSendMessage
            // 
            this.btnQuickSendMessage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuickSendMessage.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnQuickSendMessage.Location = new System.Drawing.Point(370, 45);
            this.btnQuickSendMessage.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnQuickSendMessage.Name = "btnQuickSendMessage";
            this.btnQuickSendMessage.Size = new System.Drawing.Size(150, 35);
            this.btnQuickSendMessage.TabIndex = 2;
            this.btnQuickSendMessage.Tag = "SendMessage(\"wxid\", \"Hello\")";
            this.btnQuickSendMessage.Text = "发送消息";
            this.btnQuickSendMessage.Click += new System.EventHandler(this.btnQuickCommand_Click);
            // 
            // btnSave
            // 
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnSave.Location = new System.Drawing.Point(550, 560);
            this.btnSave.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 35);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "保存";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnCancel.Location = new System.Drawing.Point(680, 560);
            this.btnCancel.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "关闭";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(800, 610);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.uiTabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "设置";
            this.uiTabControl1.ResumeLayout(false);
            this.tabPageConnection.ResumeLayout(false);
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox2.ResumeLayout(false);
            this.tabPageTest.ResumeLayout(false);
            this.uiGroupBox3.ResumeLayout(false);
            this.uiGroupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

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
    }
}

