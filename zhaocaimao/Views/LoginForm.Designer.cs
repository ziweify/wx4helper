namespace zhaocaimao.Views
{
    partial class LoginForm
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
            pnlMain = new Sunny.UI.UIPanel();
            btnCancel = new Sunny.UI.UIButton();
            btnLogin = new Sunny.UI.UIButton();
            chkRememberPassword = new Sunny.UI.UICheckBox();
            txtPassword = new Sunny.UI.UITextBox();
            lblPassword = new Sunny.UI.UILabel();
            txtUsername = new Sunny.UI.UITextBox();
            lblUsername = new Sunny.UI.UILabel();
            lblTitle = new Sunny.UI.UILabel();
            pnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(btnCancel);
            pnlMain.Controls.Add(btnLogin);
            pnlMain.Controls.Add(chkRememberPassword);
            pnlMain.Controls.Add(txtPassword);
            pnlMain.Controls.Add(lblPassword);
            pnlMain.Controls.Add(txtUsername);
            pnlMain.Controls.Add(lblUsername);
            pnlMain.Controls.Add(lblTitle);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Font = new Font("微软雅黑", 12F);
            pnlMain.Location = new Point(0, 35);
            pnlMain.Margin = new Padding(4, 5, 4, 5);
            pnlMain.MinimumSize = new Size(1, 1);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(30);
            // 🔥 金色招财猫主题
            pnlMain.FillColor = Color.FromArgb(255, 248, 220);  // 淡金色背景
            pnlMain.RectColor = Color.FromArgb(255, 193, 7);   // 金色边框
            pnlMain.Size = new Size(450, 380);
            pnlMain.TabIndex = 0;
            pnlMain.Text = null;
            pnlMain.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnCancel
            // 
            btnCancel.Cursor = Cursors.Hand;
            // 🔥 金色主题 - 取消按钮使用深金色
            btnCancel.FillColor = Color.FromArgb(184, 134, 11);  // 深金色
            btnCancel.FillHoverColor = Color.FromArgb(218, 165, 32);  // 金色悬停
            btnCancel.FillPressColor = Color.FromArgb(153, 101, 21);  // 深金色按下
            btnCancel.Font = new Font("微软雅黑", 12F);
            btnCancel.Location = new Point(240, 295);
            btnCancel.MinimumSize = new Size(1, 1);
            btnCancel.Name = "btnCancel";
            btnCancel.RectColor = Color.FromArgb(184, 134, 11);
            btnCancel.RectHoverColor = Color.FromArgb(218, 165, 32);
            btnCancel.RectPressColor = Color.FromArgb(153, 101, 21);
            btnCancel.Size = new Size(120, 45);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "取消";
            btnCancel.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnCancel.Click += btnCancel_Click;
            // 
            // btnLogin
            // 
            btnLogin.Cursor = Cursors.Hand;
            // 🔥 金色主题 - 登录按钮使用亮金色
            btnLogin.FillColor = Color.FromArgb(255, 193, 7);  // 亮金色
            btnLogin.FillHoverColor = Color.FromArgb(255, 215, 0);  // 金色悬停
            btnLogin.FillPressColor = Color.FromArgb(218, 165, 32);  // 深金色按下
            btnLogin.RectColor = Color.FromArgb(255, 193, 7);
            btnLogin.RectHoverColor = Color.FromArgb(255, 215, 0);
            btnLogin.RectPressColor = Color.FromArgb(218, 165, 32);
            btnLogin.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            btnLogin.Location = new Point(90, 295);
            btnLogin.MinimumSize = new Size(1, 1);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(120, 45);
            btnLogin.TabIndex = 6;
            btnLogin.Text = "登录";
            btnLogin.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // chkRememberPassword
            // 
            chkRememberPassword.Font = new Font("微软雅黑", 10F);
            chkRememberPassword.ForeColor = Color.FromArgb(184, 134, 11);  // 金色文字
            chkRememberPassword.Location = new Point(130, 245);
            chkRememberPassword.MinimumSize = new Size(1, 1);
            chkRememberPassword.Name = "chkRememberPassword";
            chkRememberPassword.Size = new Size(150, 29);
            chkRememberPassword.TabIndex = 5;
            chkRememberPassword.Text = "记住密码";
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("微软雅黑", 12F);
            txtPassword.RectColor = Color.FromArgb(255, 193, 7);  // 金色边框
            txtPassword.Location = new Point(130, 195);
            txtPassword.Margin = new Padding(4, 5, 4, 5);
            txtPassword.MinimumSize = new Size(1, 16);
            txtPassword.Name = "txtPassword";
            txtPassword.Padding = new Padding(5);
            txtPassword.PasswordChar = '●';
            txtPassword.ShowText = false;
            txtPassword.Size = new Size(270, 35);
            txtPassword.TabIndex = 4;
            txtPassword.TextAlignment = ContentAlignment.MiddleLeft;
            txtPassword.Watermark = "请输入密码";
            // 
            // lblPassword
            // 
            lblPassword.Font = new Font("微软雅黑", 12F);
            lblPassword.ForeColor = Color.FromArgb(184, 134, 11);  // 金色文字
            lblPassword.Location = new Point(50, 195);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(80, 35);
            lblPassword.TabIndex = 3;
            lblPassword.Text = "密  码:";
            lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtUsername
            // 
            txtUsername.Font = new Font("微软雅黑", 12F);
            txtUsername.RectColor = Color.FromArgb(255, 193, 7);  // 金色边框
            txtUsername.Location = new Point(130, 140);
            txtUsername.Margin = new Padding(4, 5, 4, 5);
            txtUsername.MinimumSize = new Size(1, 16);
            txtUsername.Name = "txtUsername";
            txtUsername.Padding = new Padding(5);
            txtUsername.ShowText = false;
            txtUsername.Size = new Size(270, 35);
            txtUsername.TabIndex = 2;
            txtUsername.TextAlignment = ContentAlignment.MiddleLeft;
            txtUsername.Watermark = "请输入用户名";
            // 
            // lblUsername
            // 
            lblUsername.Font = new Font("微软雅黑", 12F);
            lblUsername.ForeColor = Color.FromArgb(184, 134, 11);  // 金色文字
            lblUsername.Location = new Point(50, 140);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(80, 35);
            lblUsername.TabIndex = 1;
            lblUsername.Text = "用户名:";
            lblUsername.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            // 🔥 金色招财猫主题 - 标题
            lblTitle.Font = new Font("微软雅黑", 22F, FontStyle.Bold, GraphicsUnit.Point, 134);
            lblTitle.ForeColor = Color.FromArgb(184, 134, 11);  // 深金色文字
            lblTitle.Location = new Point(30, 25);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(390, 70);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "💰 招财猫系统登录 💰";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LoginForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(450, 415);
            Controls.Add(pnlMain);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "招财猫 - 系统登录";
            ZoomScaleRect = new Rectangle(15, 15, 800, 450);
            Load += LoginForm_Load;
            pnlMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UIPanel pnlMain;
        private Sunny.UI.UILabel lblTitle;
        private Sunny.UI.UILabel lblUsername;
        private Sunny.UI.UITextBox txtUsername;
        private Sunny.UI.UILabel lblPassword;
        private Sunny.UI.UITextBox txtPassword;
        private Sunny.UI.UICheckBox chkRememberPassword;
        private Sunny.UI.UIButton btnLogin;
        private Sunny.UI.UIButton btnCancel;
    }
}

