using DevExpress.XtraEditors;

namespace 永利系统.Views.Dashboard.Controls
{
    partial class MonitorConfigControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblUrl = new LabelControl();
            txtUrl = new TextEdit();
            lblUsername = new LabelControl();
            txtUsername = new TextEdit();
            lblPassword = new LabelControl();
            txtPassword = new TextEdit();
            chkAutoLogin = new CheckEdit();
            lblScript = new LabelControl();
            memoScript = new MemoEdit();
            groupDataUpdate = new GroupControl();
            txtLatestIssueData = new TextEdit();
            lblLatestIssueData = new LabelControl();
            groupCommands = new GroupControl();
            btnGetCookie = new SimpleButton();
            btnCollect = new SimpleButton();
            btnLogin = new SimpleButton();
            ((System.ComponentModel.ISupportInitialize)txtUrl.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtUsername.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPassword.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkAutoLogin.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memoScript.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDataUpdate).BeginInit();
            groupDataUpdate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtLatestIssueData.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupCommands).BeginInit();
            groupCommands.SuspendLayout();
            SuspendLayout();
            // 
            // lblUrl
            // 
            lblUrl.Location = new System.Drawing.Point(15, 15);
            lblUrl.Name = "lblUrl";
            lblUrl.Size = new System.Drawing.Size(36, 14);
            lblUrl.TabIndex = 0;
            lblUrl.Text = "网址:";
            // 
            // txtUrl
            // 
            txtUrl.Location = new System.Drawing.Point(65, 12);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new System.Drawing.Size(240, 20);
            txtUrl.TabIndex = 1;
            // 
            // lblUsername
            // 
            lblUsername.Location = new System.Drawing.Point(15, 45);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new System.Drawing.Size(48, 14);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "用户名:";
            // 
            // txtUsername
            // 
            txtUsername.Location = new System.Drawing.Point(65, 42);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new System.Drawing.Size(240, 20);
            txtUsername.TabIndex = 3;
            // 
            // lblPassword
            // 
            lblPassword.Location = new System.Drawing.Point(15, 75);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(36, 14);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "密码:";
            // 
            // txtPassword
            // 
            txtPassword.Location = new System.Drawing.Point(65, 72);
            txtPassword.Name = "txtPassword";
            txtPassword.Properties.PasswordChar = '●';
            txtPassword.Properties.UseSystemPasswordChar = true;
            txtPassword.Size = new System.Drawing.Size(240, 20);
            txtPassword.TabIndex = 5;
            // 
            // chkAutoLogin
            // 
            chkAutoLogin.Location = new System.Drawing.Point(65, 102);
            chkAutoLogin.Name = "chkAutoLogin";
            chkAutoLogin.Properties.Caption = "自动登录";
            chkAutoLogin.Size = new System.Drawing.Size(75, 20);
            chkAutoLogin.TabIndex = 6;
            // 
            // lblScript
            // 
            lblScript.Location = new System.Drawing.Point(15, 135);
            lblScript.Name = "lblScript";
            lblScript.Size = new System.Drawing.Size(36, 14);
            lblScript.TabIndex = 7;
            lblScript.Text = "脚本:";
            // 
            // memoScript
            // 
            memoScript.Location = new System.Drawing.Point(15, 155);
            memoScript.Name = "memoScript";
            memoScript.Properties.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            memoScript.Size = new System.Drawing.Size(290, 150);
            memoScript.TabIndex = 8;
            // 
            // groupDataUpdate
            // 
            groupDataUpdate.Controls.Add(txtLatestIssueData);
            groupDataUpdate.Controls.Add(lblLatestIssueData);
            groupDataUpdate.Location = new System.Drawing.Point(15, 315);
            groupDataUpdate.Name = "groupDataUpdate";
            groupDataUpdate.Size = new System.Drawing.Size(290, 80);
            groupDataUpdate.TabIndex = 9;
            groupDataUpdate.Text = "数据更新时间";
            // 
            // lblLatestIssueData
            // 
            lblLatestIssueData.Location = new System.Drawing.Point(15, 35);
            lblLatestIssueData.Name = "lblLatestIssueData";
            lblLatestIssueData.Size = new System.Drawing.Size(60, 14);
            lblLatestIssueData.TabIndex = 0;
            lblLatestIssueData.Text = "最新期号:";
            // 
            // txtLatestIssueData
            // 
            txtLatestIssueData.Location = new System.Drawing.Point(85, 32);
            txtLatestIssueData.Name = "txtLatestIssueData";
            txtLatestIssueData.Properties.ReadOnly = true;
            txtLatestIssueData.Size = new System.Drawing.Size(190, 20);
            txtLatestIssueData.TabIndex = 1;
            // 
            // groupCommands
            // 
            groupCommands.Controls.Add(btnGetCookie);
            groupCommands.Controls.Add(btnCollect);
            groupCommands.Controls.Add(btnLogin);
            groupCommands.Location = new System.Drawing.Point(15, 405);
            groupCommands.Name = "groupCommands";
            groupCommands.Size = new System.Drawing.Size(290, 100);
            groupCommands.TabIndex = 10;
            groupCommands.Text = "命令";
            // 
            // btnLogin
            // 
            btnLogin.Location = new System.Drawing.Point(15, 35);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new System.Drawing.Size(80, 28);
            btnLogin.TabIndex = 0;
            btnLogin.Text = "登录";
            btnLogin.Click += BtnLogin_Click;
            // 
            // btnCollect
            // 
            btnCollect.Location = new System.Drawing.Point(105, 35);
            btnCollect.Name = "btnCollect";
            btnCollect.Size = new System.Drawing.Size(80, 28);
            btnCollect.TabIndex = 1;
            btnCollect.Text = "采集";
            btnCollect.Click += BtnCollect_Click;
            // 
            // btnGetCookie
            // 
            btnGetCookie.Location = new System.Drawing.Point(195, 35);
            btnGetCookie.Name = "btnGetCookie";
            btnGetCookie.Size = new System.Drawing.Size(80, 28);
            btnGetCookie.TabIndex = 2;
            btnGetCookie.Text = "获取Cookie";
            btnGetCookie.Click += BtnGetCookie_Click;
            // 
            // MonitorConfigControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(groupCommands);
            Controls.Add(groupDataUpdate);
            Controls.Add(memoScript);
            Controls.Add(lblScript);
            Controls.Add(chkAutoLogin);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtUsername);
            Controls.Add(lblUsername);
            Controls.Add(txtUrl);
            Controls.Add(lblUrl);
            Name = "MonitorConfigControl";
            Size = new System.Drawing.Size(320, 520);
            ((System.ComponentModel.ISupportInitialize)txtUrl.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtUsername.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPassword.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkAutoLogin.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memoScript.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupDataUpdate).EndInit();
            groupDataUpdate.ResumeLayout(false);
            groupDataUpdate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtLatestIssueData.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupCommands).EndInit();
            groupCommands.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private LabelControl lblUrl;
        private TextEdit txtUrl;
        private LabelControl lblUsername;
        private TextEdit txtUsername;
        private LabelControl lblPassword;
        private TextEdit txtPassword;
        private CheckEdit chkAutoLogin;
        private LabelControl lblScript;
        private MemoEdit memoScript;
        private GroupControl groupDataUpdate;
        private TextEdit txtLatestIssueData;
        private LabelControl lblLatestIssueData;
        private GroupControl groupCommands;
        private SimpleButton btnGetCookie;
        private SimpleButton btnCollect;
        private SimpleButton btnLogin;
    }
}

