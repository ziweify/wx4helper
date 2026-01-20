namespace YongLiSystem.Views.Wechat
{
    partial class WechatSettingsForm
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
            xtraTabControl_Settings = new DevExpress.XtraTab.XtraTabControl();
            xtraTabPageConnection = new DevExpress.XtraTab.XtraTabPage();
            groupControl_Connection = new DevExpress.XtraEditors.GroupControl();
            simpleButton_RefreshStatus = new DevExpress.XtraEditors.SimpleButton();
            labelControl_ConnectionStatusValue = new DevExpress.XtraEditors.LabelControl();
            labelControl_ConnectionStatus = new DevExpress.XtraEditors.LabelControl();
            xtraTabPageCommandTest = new DevExpress.XtraTab.XtraTabPage();
            groupControl_CommandTest = new DevExpress.XtraEditors.GroupControl();
            simpleButton_ClearResult = new DevExpress.XtraEditors.SimpleButton();
            memoEdit_Result = new DevExpress.XtraEditors.MemoEdit();
            labelControl_Result = new DevExpress.XtraEditors.LabelControl();
            simpleButton_SendCommand = new DevExpress.XtraEditors.SimpleButton();
            textEdit_Command = new DevExpress.XtraEditors.TextEdit();
            labelControl_Command = new DevExpress.XtraEditors.LabelControl();
            groupControl_QuickCommands = new DevExpress.XtraEditors.GroupControl();
            simpleButton_SendImage = new DevExpress.XtraEditors.SimpleButton();
            simpleButton_SendMessage = new DevExpress.XtraEditors.SimpleButton();
            simpleButton_GetGroupContacts = new DevExpress.XtraEditors.SimpleButton();
            simpleButton_GetContacts = new DevExpress.XtraEditors.SimpleButton();
            simpleButton_GetUserInfo = new DevExpress.XtraEditors.SimpleButton();
            xtraTabPageSoundTest = new DevExpress.XtraTab.XtraTabPage();
            groupControl_SoundTest = new DevExpress.XtraEditors.GroupControl();
            labelControl_SoundTestResult = new DevExpress.XtraEditors.LabelControl();
            simpleButton_TestCreditDown = new DevExpress.XtraEditors.SimpleButton();
            simpleButton_TestCreditUp = new DevExpress.XtraEditors.SimpleButton();
            simpleButton_TestLottery = new DevExpress.XtraEditors.SimpleButton();
            simpleButton_TestSealing = new DevExpress.XtraEditors.SimpleButton();
            xtraTabPageSystem = new DevExpress.XtraTab.XtraTabPage();
            groupControl_DevModeOptions = new DevExpress.XtraEditors.GroupControl();
            simpleButton_SendTestMessage = new DevExpress.XtraEditors.SimpleButton();
            textEdit_TestMessage = new DevExpress.XtraEditors.TextEdit();
            labelControl_TestMessage = new DevExpress.XtraEditors.LabelControl();
            textEdit_CurrentMember = new DevExpress.XtraEditors.TextEdit();
            labelControl_CurrentMember = new DevExpress.XtraEditors.LabelControl();
            groupControl_SystemModes = new DevExpress.XtraEditors.GroupControl();
            checkEdit_DevMode = new DevExpress.XtraEditors.CheckEdit();
            checkEdit_DisableSystemMessages = new DevExpress.XtraEditors.CheckEdit();
            simpleButton_Close = new DevExpress.XtraEditors.SimpleButton();
            checkEdit_AdminMode = new DevExpress.XtraEditors.CheckEdit();
            groupControl_AdminModeOptions = new DevExpress.XtraEditors.GroupControl();
            ((System.ComponentModel.ISupportInitialize)xtraTabControl_Settings).BeginInit();
            xtraTabControl_Settings.SuspendLayout();
            xtraTabPageConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupControl_Connection).BeginInit();
            groupControl_Connection.SuspendLayout();
            xtraTabPageCommandTest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupControl_CommandTest).BeginInit();
            groupControl_CommandTest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)memoEdit_Result.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)textEdit_Command.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupControl_QuickCommands).BeginInit();
            groupControl_QuickCommands.SuspendLayout();
            xtraTabPageSoundTest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupControl_SoundTest).BeginInit();
            groupControl_SoundTest.SuspendLayout();
            xtraTabPageSystem.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupControl_DevModeOptions).BeginInit();
            groupControl_DevModeOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)textEdit_TestMessage.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)textEdit_CurrentMember.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupControl_SystemModes).BeginInit();
            groupControl_SystemModes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)checkEdit_DevMode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)checkEdit_DisableSystemMessages.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)checkEdit_AdminMode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupControl_AdminModeOptions).BeginInit();
            groupControl_AdminModeOptions.SuspendLayout();
            SuspendLayout();
            // 
            // xtraTabControl_Settings
            // 
            xtraTabControl_Settings.Dock = System.Windows.Forms.DockStyle.Top;
            xtraTabControl_Settings.Location = new System.Drawing.Point(0, 0);
            xtraTabControl_Settings.Name = "xtraTabControl_Settings";
            xtraTabControl_Settings.SelectedTabPage = xtraTabPageSystem;
            xtraTabControl_Settings.Size = new System.Drawing.Size(800, 500);
            xtraTabControl_Settings.TabIndex = 0;
            xtraTabControl_Settings.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { xtraTabPageSystem, xtraTabPageConnection, xtraTabPageCommandTest, xtraTabPageSoundTest });
            // 
            // xtraTabPageConnection
            // 
            xtraTabPageConnection.Controls.Add(groupControl_Connection);
            xtraTabPageConnection.Name = "xtraTabPageConnection";
            xtraTabPageConnection.Size = new System.Drawing.Size(798, 474);
            xtraTabPageConnection.Text = "连接设置";
            // 
            // groupControl_Connection
            // 
            groupControl_Connection.Controls.Add(simpleButton_RefreshStatus);
            groupControl_Connection.Controls.Add(labelControl_ConnectionStatusValue);
            groupControl_Connection.Controls.Add(labelControl_ConnectionStatus);
            groupControl_Connection.Location = new System.Drawing.Point(20, 20);
            groupControl_Connection.Name = "groupControl_Connection";
            groupControl_Connection.Size = new System.Drawing.Size(750, 150);
            groupControl_Connection.TabIndex = 0;
            groupControl_Connection.Text = "连接状态";
            // 
            // simpleButton_RefreshStatus
            // 
            simpleButton_RefreshStatus.Location = new System.Drawing.Point(120, 80);
            simpleButton_RefreshStatus.Name = "simpleButton_RefreshStatus";
            simpleButton_RefreshStatus.Size = new System.Drawing.Size(100, 30);
            simpleButton_RefreshStatus.TabIndex = 2;
            simpleButton_RefreshStatus.Text = "刷新状态";
            // 
            // labelControl_ConnectionStatusValue
            // 
            labelControl_ConnectionStatusValue.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            labelControl_ConnectionStatusValue.Appearance.ForeColor = System.Drawing.Color.Red;
            labelControl_ConnectionStatusValue.Appearance.Options.UseFont = true;
            labelControl_ConnectionStatusValue.Appearance.Options.UseForeColor = true;
            labelControl_ConnectionStatusValue.Location = new System.Drawing.Point(120, 40);
            labelControl_ConnectionStatusValue.Name = "labelControl_ConnectionStatusValue";
            labelControl_ConnectionStatusValue.Size = new System.Drawing.Size(39, 14);
            labelControl_ConnectionStatusValue.TabIndex = 1;
            labelControl_ConnectionStatusValue.Text = "未连接";
            // 
            // labelControl_ConnectionStatus
            // 
            labelControl_ConnectionStatus.Location = new System.Drawing.Point(30, 40);
            labelControl_ConnectionStatus.Name = "labelControl_ConnectionStatus";
            labelControl_ConnectionStatus.Size = new System.Drawing.Size(60, 14);
            labelControl_ConnectionStatus.TabIndex = 0;
            labelControl_ConnectionStatus.Text = "连接状态：";
            // 
            // xtraTabPageCommandTest
            // 
            xtraTabPageCommandTest.Controls.Add(groupControl_CommandTest);
            xtraTabPageCommandTest.Controls.Add(groupControl_QuickCommands);
            xtraTabPageCommandTest.Name = "xtraTabPageCommandTest";
            xtraTabPageCommandTest.Size = new System.Drawing.Size(798, 474);
            xtraTabPageCommandTest.Text = "命令测试";
            // 
            // groupControl_CommandTest
            // 
            groupControl_CommandTest.Controls.Add(simpleButton_ClearResult);
            groupControl_CommandTest.Controls.Add(memoEdit_Result);
            groupControl_CommandTest.Controls.Add(labelControl_Result);
            groupControl_CommandTest.Controls.Add(simpleButton_SendCommand);
            groupControl_CommandTest.Controls.Add(textEdit_Command);
            groupControl_CommandTest.Controls.Add(labelControl_Command);
            groupControl_CommandTest.Location = new System.Drawing.Point(20, 140);
            groupControl_CommandTest.Name = "groupControl_CommandTest";
            groupControl_CommandTest.Size = new System.Drawing.Size(750, 310);
            groupControl_CommandTest.TabIndex = 1;
            groupControl_CommandTest.Text = "Socket 命令测试";
            // 
            // simpleButton_ClearResult
            // 
            simpleButton_ClearResult.Location = new System.Drawing.Point(620, 75);
            simpleButton_ClearResult.Name = "simpleButton_ClearResult";
            simpleButton_ClearResult.Size = new System.Drawing.Size(100, 30);
            simpleButton_ClearResult.TabIndex = 5;
            simpleButton_ClearResult.Text = "清空结果";
            // 
            // memoEdit_Result
            // 
            memoEdit_Result.Location = new System.Drawing.Point(30, 100);
            memoEdit_Result.Name = "memoEdit_Result";
            memoEdit_Result.Properties.ReadOnly = true;
            memoEdit_Result.Properties.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            memoEdit_Result.Size = new System.Drawing.Size(690, 180);
            memoEdit_Result.TabIndex = 4;
            // 
            // labelControl_Result
            // 
            labelControl_Result.Location = new System.Drawing.Point(30, 80);
            labelControl_Result.Name = "labelControl_Result";
            labelControl_Result.Size = new System.Drawing.Size(60, 14);
            labelControl_Result.TabIndex = 3;
            labelControl_Result.Text = "执行结果：";
            // 
            // simpleButton_SendCommand
            // 
            simpleButton_SendCommand.Location = new System.Drawing.Point(620, 35);
            simpleButton_SendCommand.Name = "simpleButton_SendCommand";
            simpleButton_SendCommand.Size = new System.Drawing.Size(100, 30);
            simpleButton_SendCommand.TabIndex = 2;
            simpleButton_SendCommand.Text = "发送";
            // 
            // textEdit_Command
            // 
            textEdit_Command.Location = new System.Drawing.Point(80, 37);
            textEdit_Command.Name = "textEdit_Command";
            textEdit_Command.Properties.NullValuePrompt = "例如: GetUserInfo() 或 SendMessage(\"wxid\", \"Hello\")";
            textEdit_Command.Size = new System.Drawing.Size(530, 20);
            textEdit_Command.TabIndex = 1;
            // 
            // labelControl_Command
            // 
            labelControl_Command.Location = new System.Drawing.Point(30, 40);
            labelControl_Command.Name = "labelControl_Command";
            labelControl_Command.Size = new System.Drawing.Size(36, 14);
            labelControl_Command.TabIndex = 0;
            labelControl_Command.Text = "命令：";
            // 
            // groupControl_QuickCommands
            // 
            groupControl_QuickCommands.Controls.Add(simpleButton_SendImage);
            groupControl_QuickCommands.Controls.Add(simpleButton_SendMessage);
            groupControl_QuickCommands.Controls.Add(simpleButton_GetGroupContacts);
            groupControl_QuickCommands.Controls.Add(simpleButton_GetContacts);
            groupControl_QuickCommands.Controls.Add(simpleButton_GetUserInfo);
            groupControl_QuickCommands.Location = new System.Drawing.Point(20, 20);
            groupControl_QuickCommands.Name = "groupControl_QuickCommands";
            groupControl_QuickCommands.Size = new System.Drawing.Size(750, 100);
            groupControl_QuickCommands.TabIndex = 0;
            groupControl_QuickCommands.Text = "快捷命令";
            // 
            // simpleButton_SendImage
            // 
            simpleButton_SendImage.Location = new System.Drawing.Point(530, 40);
            simpleButton_SendImage.Name = "simpleButton_SendImage";
            simpleButton_SendImage.Size = new System.Drawing.Size(100, 30);
            simpleButton_SendImage.TabIndex = 4;
            simpleButton_SendImage.Text = "发送图片";
            // 
            // simpleButton_SendMessage
            // 
            simpleButton_SendMessage.Location = new System.Drawing.Point(420, 40);
            simpleButton_SendMessage.Name = "simpleButton_SendMessage";
            simpleButton_SendMessage.Size = new System.Drawing.Size(100, 30);
            simpleButton_SendMessage.TabIndex = 3;
            simpleButton_SendMessage.Text = "发送消息";
            // 
            // simpleButton_GetGroupContacts
            // 
            simpleButton_GetGroupContacts.Location = new System.Drawing.Point(290, 40);
            simpleButton_GetGroupContacts.Name = "simpleButton_GetGroupContacts";
            simpleButton_GetGroupContacts.Size = new System.Drawing.Size(120, 30);
            simpleButton_GetGroupContacts.TabIndex = 2;
            simpleButton_GetGroupContacts.Text = "获取群成员列表";
            // 
            // simpleButton_GetContacts
            // 
            simpleButton_GetContacts.Location = new System.Drawing.Point(160, 40);
            simpleButton_GetContacts.Name = "simpleButton_GetContacts";
            simpleButton_GetContacts.Size = new System.Drawing.Size(120, 30);
            simpleButton_GetContacts.TabIndex = 1;
            simpleButton_GetContacts.Text = "获取联系人列表";
            // 
            // simpleButton_GetUserInfo
            // 
            simpleButton_GetUserInfo.Location = new System.Drawing.Point(30, 40);
            simpleButton_GetUserInfo.Name = "simpleButton_GetUserInfo";
            simpleButton_GetUserInfo.Size = new System.Drawing.Size(120, 30);
            simpleButton_GetUserInfo.TabIndex = 0;
            simpleButton_GetUserInfo.Text = "获取用户信息";
            // 
            // xtraTabPageSoundTest
            // 
            xtraTabPageSoundTest.Controls.Add(groupControl_SoundTest);
            xtraTabPageSoundTest.Name = "xtraTabPageSoundTest";
            xtraTabPageSoundTest.Size = new System.Drawing.Size(798, 474);
            xtraTabPageSoundTest.Text = "声音测试";
            // 
            // groupControl_SoundTest
            // 
            groupControl_SoundTest.Controls.Add(labelControl_SoundTestResult);
            groupControl_SoundTest.Controls.Add(simpleButton_TestCreditDown);
            groupControl_SoundTest.Controls.Add(simpleButton_TestCreditUp);
            groupControl_SoundTest.Controls.Add(simpleButton_TestLottery);
            groupControl_SoundTest.Controls.Add(simpleButton_TestSealing);
            groupControl_SoundTest.Location = new System.Drawing.Point(20, 20);
            groupControl_SoundTest.Name = "groupControl_SoundTest";
            groupControl_SoundTest.Size = new System.Drawing.Size(750, 250);
            groupControl_SoundTest.TabIndex = 0;
            groupControl_SoundTest.Text = "声音测试";
            // 
            // labelControl_SoundTestResult
            // 
            labelControl_SoundTestResult.Appearance.Options.UseFont = true;
            labelControl_SoundTestResult.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            labelControl_SoundTestResult.Location = new System.Drawing.Point(30, 120);
            labelControl_SoundTestResult.Name = "labelControl_SoundTestResult";
            labelControl_SoundTestResult.Size = new System.Drawing.Size(690, 14);
            labelControl_SoundTestResult.TabIndex = 4;
            labelControl_SoundTestResult.Text = "点击按钮测试声音播放...";
            // 
            // simpleButton_TestCreditDown
            // 
            simpleButton_TestCreditDown.Location = new System.Drawing.Point(540, 50);
            simpleButton_TestCreditDown.Name = "simpleButton_TestCreditDown";
            simpleButton_TestCreditDown.Size = new System.Drawing.Size(150, 40);
            simpleButton_TestCreditDown.TabIndex = 3;
            simpleButton_TestCreditDown.Text = "💸 测试下分声音";
            // 
            // simpleButton_TestCreditUp
            // 
            simpleButton_TestCreditUp.Location = new System.Drawing.Point(370, 50);
            simpleButton_TestCreditUp.Name = "simpleButton_TestCreditUp";
            simpleButton_TestCreditUp.Size = new System.Drawing.Size(150, 40);
            simpleButton_TestCreditUp.TabIndex = 2;
            simpleButton_TestCreditUp.Text = "💰 测试上分声音";
            // 
            // simpleButton_TestLottery
            // 
            simpleButton_TestLottery.Location = new System.Drawing.Point(200, 50);
            simpleButton_TestLottery.Name = "simpleButton_TestLottery";
            simpleButton_TestLottery.Size = new System.Drawing.Size(150, 40);
            simpleButton_TestLottery.TabIndex = 1;
            simpleButton_TestLottery.Text = "🎲 测试开奖声音";
            // 
            // simpleButton_TestSealing
            // 
            simpleButton_TestSealing.Location = new System.Drawing.Point(30, 50);
            simpleButton_TestSealing.Name = "simpleButton_TestSealing";
            simpleButton_TestSealing.Size = new System.Drawing.Size(150, 40);
            simpleButton_TestSealing.TabIndex = 0;
            simpleButton_TestSealing.Text = "🔔 测试封盘声音";
            // 
            // xtraTabPageSystem
            // 
            xtraTabPageSystem.Controls.Add(groupControl_AdminModeOptions);
            xtraTabPageSystem.Controls.Add(groupControl_DevModeOptions);
            xtraTabPageSystem.Controls.Add(groupControl_SystemModes);
            xtraTabPageSystem.Name = "xtraTabPageSystem";
            xtraTabPageSystem.Size = new System.Drawing.Size(798, 474);
            xtraTabPageSystem.Text = "系统设置";
            // 
            // groupControl_DevModeOptions
            // 
            groupControl_DevModeOptions.Controls.Add(simpleButton_SendTestMessage);
            groupControl_DevModeOptions.Controls.Add(checkEdit_DevMode);
            groupControl_DevModeOptions.Controls.Add(textEdit_TestMessage);
            groupControl_DevModeOptions.Controls.Add(labelControl_TestMessage);
            groupControl_DevModeOptions.Controls.Add(textEdit_CurrentMember);
            groupControl_DevModeOptions.Controls.Add(labelControl_CurrentMember);
            groupControl_DevModeOptions.Location = new System.Drawing.Point(20, 159);
            groupControl_DevModeOptions.Name = "groupControl_DevModeOptions";
            groupControl_DevModeOptions.Size = new System.Drawing.Size(530, 312);
            groupControl_DevModeOptions.TabIndex = 1;
            groupControl_DevModeOptions.Text = "开发模式设置";
            // 
            // simpleButton_SendTestMessage
            // 
            simpleButton_SendTestMessage.Location = new System.Drawing.Point(217, 104);
            simpleButton_SendTestMessage.Name = "simpleButton_SendTestMessage";
            simpleButton_SendTestMessage.Size = new System.Drawing.Size(44, 19);
            simpleButton_SendTestMessage.TabIndex = 4;
            simpleButton_SendTestMessage.Text = "发送";
            // 
            // textEdit_TestMessage
            // 
            textEdit_TestMessage.EditValue = "123大小20";
            textEdit_TestMessage.Location = new System.Drawing.Point(75, 78);
            textEdit_TestMessage.Name = "textEdit_TestMessage";
            textEdit_TestMessage.Size = new System.Drawing.Size(186, 20);
            textEdit_TestMessage.TabIndex = 3;
            // 
            // labelControl_TestMessage
            // 
            labelControl_TestMessage.Location = new System.Drawing.Point(9, 81);
            labelControl_TestMessage.Name = "labelControl_TestMessage";
            labelControl_TestMessage.Size = new System.Drawing.Size(60, 14);
            labelControl_TestMessage.TabIndex = 2;
            labelControl_TestMessage.Text = "消息内容：";
            labelControl_TestMessage.Click += labelControl_TestMessage_Click;
            // 
            // textEdit_CurrentMember
            // 
            textEdit_CurrentMember.Location = new System.Drawing.Point(75, 52);
            textEdit_CurrentMember.Name = "textEdit_CurrentMember";
            textEdit_CurrentMember.Properties.ReadOnly = true;
            textEdit_CurrentMember.Size = new System.Drawing.Size(186, 20);
            textEdit_CurrentMember.TabIndex = 1;
            // 
            // labelControl_CurrentMember
            // 
            labelControl_CurrentMember.Location = new System.Drawing.Point(9, 58);
            labelControl_CurrentMember.Name = "labelControl_CurrentMember";
            labelControl_CurrentMember.Size = new System.Drawing.Size(60, 14);
            labelControl_CurrentMember.TabIndex = 0;
            labelControl_CurrentMember.Text = "当前会员：";
            // 
            // groupControl_SystemModes
            // 
            groupControl_SystemModes.Controls.Add(checkEdit_DisableSystemMessages);
            groupControl_SystemModes.Location = new System.Drawing.Point(20, 3);
            groupControl_SystemModes.Name = "groupControl_SystemModes";
            groupControl_SystemModes.Size = new System.Drawing.Size(767, 150);
            groupControl_SystemModes.TabIndex = 0;
            groupControl_SystemModes.Text = "系统模式";
            // 
            // checkEdit_DevMode
            // 
            checkEdit_DevMode.Location = new System.Drawing.Point(9, 26);
            checkEdit_DevMode.Name = "checkEdit_DevMode";
            checkEdit_DevMode.Properties.Caption = "开发模式（允许手动绑定群、模拟各项数据）";
            checkEdit_DevMode.Size = new System.Drawing.Size(261, 20);
            checkEdit_DevMode.TabIndex = 2;
            // 
            // checkEdit_DisableSystemMessages
            // 
            checkEdit_DisableSystemMessages.Location = new System.Drawing.Point(30, 40);
            checkEdit_DisableSystemMessages.Name = "checkEdit_DisableSystemMessages";
            checkEdit_DisableSystemMessages.Properties.Caption = "收单关闭时不发送系统消息（开盘、封盘、开奖、结算等自动消息都不发送）";
            checkEdit_DisableSystemMessages.Size = new System.Drawing.Size(500, 20);
            checkEdit_DisableSystemMessages.TabIndex = 0;
            // 
            // simpleButton_Close
            // 
            simpleButton_Close.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            simpleButton_Close.Location = new System.Drawing.Point(710, 520);
            simpleButton_Close.Name = "simpleButton_Close";
            simpleButton_Close.Size = new System.Drawing.Size(80, 30);
            simpleButton_Close.TabIndex = 1;
            simpleButton_Close.Text = "关闭";
            simpleButton_Close.Click += SimpleButton_Cancel_Click;
            // 
            // checkEdit_AdminMode
            // 
            checkEdit_AdminMode.Location = new System.Drawing.Point(5, 26);
            checkEdit_AdminMode.Name = "checkEdit_AdminMode";
            checkEdit_AdminMode.Properties.Caption = "管理模式（允许手动绑定群）";
            checkEdit_AdminMode.Size = new System.Drawing.Size(223, 20);
            checkEdit_AdminMode.TabIndex = 1;
            // 
            // groupControl_AdminModeOptions
            // 
            groupControl_AdminModeOptions.Controls.Add(checkEdit_AdminMode);
            groupControl_AdminModeOptions.Location = new System.Drawing.Point(556, 159);
            groupControl_AdminModeOptions.Name = "groupControl_AdminModeOptions";
            groupControl_AdminModeOptions.Size = new System.Drawing.Size(233, 312);
            groupControl_AdminModeOptions.TabIndex = 2;
            groupControl_AdminModeOptions.Text = "管理模式设置";
            // 
            // WechatSettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 560);
            Controls.Add(simpleButton_Close);
            Controls.Add(xtraTabControl_Settings);
            Name = "WechatSettingsForm";
            Text = "微信助手设置";
            ((System.ComponentModel.ISupportInitialize)xtraTabControl_Settings).EndInit();
            xtraTabControl_Settings.ResumeLayout(false);
            xtraTabPageConnection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupControl_Connection).EndInit();
            groupControl_Connection.ResumeLayout(false);
            groupControl_Connection.PerformLayout();
            xtraTabPageCommandTest.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupControl_CommandTest).EndInit();
            groupControl_CommandTest.ResumeLayout(false);
            groupControl_CommandTest.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)memoEdit_Result.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)textEdit_Command.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupControl_QuickCommands).EndInit();
            groupControl_QuickCommands.ResumeLayout(false);
            xtraTabPageSoundTest.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupControl_SoundTest).EndInit();
            groupControl_SoundTest.ResumeLayout(false);
            xtraTabPageSystem.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupControl_DevModeOptions).EndInit();
            groupControl_DevModeOptions.ResumeLayout(false);
            groupControl_DevModeOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)textEdit_TestMessage.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)textEdit_CurrentMember.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupControl_SystemModes).EndInit();
            groupControl_SystemModes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)checkEdit_DevMode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)checkEdit_DisableSystemMessages.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)checkEdit_AdminMode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupControl_AdminModeOptions).EndInit();
            groupControl_AdminModeOptions.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraTab.XtraTabControl xtraTabControl_Settings;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageConnection;
        private DevExpress.XtraEditors.GroupControl groupControl_Connection;
        private DevExpress.XtraEditors.LabelControl labelControl_ConnectionStatus;
        private DevExpress.XtraEditors.LabelControl labelControl_ConnectionStatusValue;
        private DevExpress.XtraEditors.SimpleButton simpleButton_RefreshStatus;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageCommandTest;
        private DevExpress.XtraEditors.GroupControl groupControl_QuickCommands;
        private DevExpress.XtraEditors.SimpleButton simpleButton_GetUserInfo;
        private DevExpress.XtraEditors.SimpleButton simpleButton_GetContacts;
        private DevExpress.XtraEditors.SimpleButton simpleButton_GetGroupContacts;
        private DevExpress.XtraEditors.SimpleButton simpleButton_SendMessage;
        private DevExpress.XtraEditors.SimpleButton simpleButton_SendImage;
        private DevExpress.XtraEditors.GroupControl groupControl_CommandTest;
        private DevExpress.XtraEditors.LabelControl labelControl_Command;
        private DevExpress.XtraEditors.TextEdit textEdit_Command;
        private DevExpress.XtraEditors.SimpleButton simpleButton_SendCommand;
        private DevExpress.XtraEditors.LabelControl labelControl_Result;
        private DevExpress.XtraEditors.MemoEdit memoEdit_Result;
        private DevExpress.XtraEditors.SimpleButton simpleButton_ClearResult;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageSoundTest;
        private DevExpress.XtraEditors.GroupControl groupControl_SoundTest;
        private DevExpress.XtraEditors.SimpleButton simpleButton_TestSealing;
        private DevExpress.XtraEditors.SimpleButton simpleButton_TestLottery;
        private DevExpress.XtraEditors.SimpleButton simpleButton_TestCreditUp;
        private DevExpress.XtraEditors.SimpleButton simpleButton_TestCreditDown;
        private DevExpress.XtraEditors.LabelControl labelControl_SoundTestResult;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageSystem;
        private DevExpress.XtraEditors.GroupControl groupControl_SystemModes;
        private DevExpress.XtraEditors.CheckEdit checkEdit_DisableSystemMessages;
        private DevExpress.XtraEditors.CheckEdit checkEdit_DevMode;
        private DevExpress.XtraEditors.GroupControl groupControl_DevModeOptions;
        private DevExpress.XtraEditors.LabelControl labelControl_CurrentMember;
        private DevExpress.XtraEditors.TextEdit textEdit_CurrentMember;
        private DevExpress.XtraEditors.LabelControl labelControl_TestMessage;
        private DevExpress.XtraEditors.TextEdit textEdit_TestMessage;
        private DevExpress.XtraEditors.SimpleButton simpleButton_SendTestMessage;
        private DevExpress.XtraEditors.SimpleButton simpleButton_Close;
        private DevExpress.XtraEditors.GroupControl groupControl_AdminModeOptions;
        private DevExpress.XtraEditors.CheckEdit checkEdit_AdminMode;
    }
}
