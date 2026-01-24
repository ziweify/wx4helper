namespace Unit.La.Controls
{
    partial class BrowserConfigPanel
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
            lblName = new System.Windows.Forms.Label();
            txtName = new TraceableTextBox { TraceName = "txtName" };
            lblUrl = new System.Windows.Forms.Label();
            txtUrl = new TraceableTextBox { TraceName = "txtUrl" };
            lblUsername = new System.Windows.Forms.Label();
            txtUsername = new TraceableTextBox { TraceName = "txtUsername" };
            lblPassword = new System.Windows.Forms.Label();
            txtPassword = new TraceableTextBox { TraceName = "txtPassword" };
            chkAutoLogin = new System.Windows.Forms.CheckBox();
            groupBoxBasic = new System.Windows.Forms.GroupBox();
            groupBoxLogin = new System.Windows.Forms.GroupBox();
            groupBoxBasic.SuspendLayout();
            groupBoxLogin.SuspendLayout();
            SuspendLayout();
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new System.Drawing.Point(15, 25);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(56, 17);
            lblName.TabIndex = 0;
            lblName.Text = "任务名称";
            // 
            // txtName
            // 
            txtName.Location = new System.Drawing.Point(90, 22);
            txtName.Name = "txtName";
            txtName.Size = new System.Drawing.Size(300, 23);
            txtName.TabIndex = 1;
            // 
            // lblUrl
            // 
            lblUrl.AutoSize = true;
            lblUrl.Location = new System.Drawing.Point(15, 60);
            lblUrl.Name = "lblUrl";
            lblUrl.Size = new System.Drawing.Size(29, 17);
            lblUrl.TabIndex = 2;
            lblUrl.Text = "URL";
            // 
            // txtUrl
            // 
            txtUrl.Location = new System.Drawing.Point(90, 57);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new System.Drawing.Size(300, 23);
            txtUrl.TabIndex = 3;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new System.Drawing.Point(15, 25);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new System.Drawing.Size(44, 17);
            lblUsername.TabIndex = 4;
            lblUsername.Text = "用户名";
            // 
            // txtUsername
            // 
            txtUsername.Location = new System.Drawing.Point(90, 22);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new System.Drawing.Size(300, 23);
            txtUsername.TabIndex = 5;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new System.Drawing.Point(15, 60);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(32, 17);
            lblPassword.TabIndex = 6;
            lblPassword.Text = "密码";
            // 
            // txtPassword
            // 
            txtPassword.Location = new System.Drawing.Point(90, 57);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new System.Drawing.Size(300, 23);
            txtPassword.TabIndex = 7;
            // 
            // chkAutoLogin
            // 
            chkAutoLogin.AutoSize = true;
            chkAutoLogin.Location = new System.Drawing.Point(90, 92);
            chkAutoLogin.Name = "chkAutoLogin";
            chkAutoLogin.Size = new System.Drawing.Size(75, 21);
            chkAutoLogin.TabIndex = 8;
            chkAutoLogin.Text = "自动登录";
            chkAutoLogin.UseVisualStyleBackColor = true;
            // 
            // groupBoxBasic
            // 
            groupBoxBasic.Controls.Add(lblName);
            groupBoxBasic.Controls.Add(txtName);
            groupBoxBasic.Controls.Add(lblUrl);
            groupBoxBasic.Controls.Add(txtUrl);
            groupBoxBasic.Dock = System.Windows.Forms.DockStyle.Top;
            groupBoxBasic.Location = new System.Drawing.Point(0, 0);
            groupBoxBasic.Name = "groupBoxBasic";
            groupBoxBasic.Size = new System.Drawing.Size(420, 100);
            groupBoxBasic.TabIndex = 9;
            groupBoxBasic.TabStop = false;
            groupBoxBasic.Text = "基本信息";
            // 
            // groupBoxLogin
            // 
            groupBoxLogin.Controls.Add(lblUsername);
            groupBoxLogin.Controls.Add(txtUsername);
            groupBoxLogin.Controls.Add(lblPassword);
            groupBoxLogin.Controls.Add(txtPassword);
            groupBoxLogin.Controls.Add(chkAutoLogin);
            groupBoxLogin.Dock = System.Windows.Forms.DockStyle.Top;
            groupBoxLogin.Location = new System.Drawing.Point(0, 100);
            groupBoxLogin.Name = "groupBoxLogin";
            groupBoxLogin.Size = new System.Drawing.Size(420, 130);
            groupBoxLogin.TabIndex = 10;
            groupBoxLogin.TabStop = false;
            groupBoxLogin.Text = "登录信息（可选）";
            // 
            // BrowserConfigPanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(groupBoxLogin);
            Controls.Add(groupBoxBasic);
            Name = "BrowserConfigPanel";
            Size = new System.Drawing.Size(420, 240);
            groupBoxBasic.ResumeLayout(false);
            groupBoxBasic.PerformLayout();
            groupBoxLogin.ResumeLayout(false);
            groupBoxLogin.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblName;
        internal TraceableTextBox txtName;
        private System.Windows.Forms.Label lblUrl;
        internal TraceableTextBox txtUrl;
        private System.Windows.Forms.Label lblUsername;
        internal TraceableTextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        internal TraceableTextBox txtPassword;
        internal System.Windows.Forms.CheckBox chkAutoLogin;
        private System.Windows.Forms.GroupBox groupBoxBasic;
        private System.Windows.Forms.GroupBox groupBoxLogin;
    }
}
