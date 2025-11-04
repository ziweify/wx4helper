namespace F5BotV2.Main
{
    partial class LoginView
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginView));
            this.tab_LoginMain = new CCWin.SkinControl.SkinTabControl();
            this.skinTabPage1 = new CCWin.SkinControl.SkinTabPage();
            this.chkAutoLogin = new CCWin.SkinControl.SkinCheckBox();
            this.btn_Login = new CCWin.SkinControl.SkinButton();
            this.tbx_LoginPwd = new CCWin.SkinControl.SkinTextBox();
            this.tbx_LoginUser = new CCWin.SkinControl.SkinTextBox();
            this.skinLabel2 = new System.Windows.Forms.Label();
            this.skinLabel1 = new System.Windows.Forms.Label();
            this.skinTabPage2 = new CCWin.SkinControl.SkinTabPage();
            this.btn_PwRecover = new CCWin.SkinControl.SkinButton();
            this.tbx_PwdRecoverConfirm = new CCWin.SkinControl.SkinTextBox();
            this.tbx_PwdRecoverNew = new CCWin.SkinControl.SkinTextBox();
            this.tbx_PwdRecoverMessage = new CCWin.SkinControl.SkinTextBox();
            this.tbx_PwdRecoverPass = new CCWin.SkinControl.SkinTextBox();
            this.tbx_PwdRecoverUser = new CCWin.SkinControl.SkinTextBox();
            this.skinLabel6 = new System.Windows.Forms.Label();
            this.skinLabel5 = new System.Windows.Forms.Label();
            this.skinLabel3 = new System.Windows.Forms.Label();
            this.skinLabel9 = new System.Windows.Forms.Label();
            this.skinLabel4 = new System.Windows.Forms.Label();
            this.skinTabPage3 = new CCWin.SkinControl.SkinTabPage();
            this.btnAddCredit = new CCWin.SkinControl.SkinButton();
            this.tbx_CarNumber = new CCWin.SkinControl.SkinTextBox();
            this.tbx_TbUser = new CCWin.SkinControl.SkinTextBox();
            this.skinLabel8 = new System.Windows.Forms.Label();
            this.skinLabel7 = new System.Windows.Forms.Label();
            this.tab_LoginMain.SuspendLayout();
            this.skinTabPage1.SuspendLayout();
            this.skinTabPage2.SuspendLayout();
            this.skinTabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab_LoginMain
            // 
            this.tab_LoginMain.AnimatorType = CCWin.SkinControl.AnimationType.HorizSlide;
            this.tab_LoginMain.CloseRect = new System.Drawing.Rectangle(2, 2, 12, 12);
            this.tab_LoginMain.Controls.Add(this.skinTabPage1);
            this.tab_LoginMain.Controls.Add(this.skinTabPage2);
            this.tab_LoginMain.Controls.Add(this.skinTabPage3);
            this.tab_LoginMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tab_LoginMain.HeadBack = null;
            this.tab_LoginMain.ImgTxtOffset = new System.Drawing.Point(0, 0);
            this.tab_LoginMain.ItemSize = new System.Drawing.Size(70, 23);
            this.tab_LoginMain.Location = new System.Drawing.Point(4, 28);
            this.tab_LoginMain.Name = "tab_LoginMain";
            this.tab_LoginMain.PageArrowDown = ((System.Drawing.Image)(resources.GetObject("tab_LoginMain.PageArrowDown")));
            this.tab_LoginMain.PageArrowHover = ((System.Drawing.Image)(resources.GetObject("tab_LoginMain.PageArrowHover")));
            this.tab_LoginMain.PageCloseHover = ((System.Drawing.Image)(resources.GetObject("tab_LoginMain.PageCloseHover")));
            this.tab_LoginMain.PageCloseNormal = ((System.Drawing.Image)(resources.GetObject("tab_LoginMain.PageCloseNormal")));
            this.tab_LoginMain.PageDown = ((System.Drawing.Image)(resources.GetObject("tab_LoginMain.PageDown")));
            this.tab_LoginMain.PageHover = ((System.Drawing.Image)(resources.GetObject("tab_LoginMain.PageHover")));
            this.tab_LoginMain.PageImagePosition = CCWin.SkinControl.SkinTabControl.ePageImagePosition.Left;
            this.tab_LoginMain.PageNorml = null;
            this.tab_LoginMain.SelectedIndex = 2;
            this.tab_LoginMain.Size = new System.Drawing.Size(428, 184);
            this.tab_LoginMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tab_LoginMain.TabIndex = 0;
            // 
            // skinTabPage1
            // 
            this.skinTabPage1.BackColor = System.Drawing.Color.White;
            this.skinTabPage1.Controls.Add(this.chkAutoLogin);
            this.skinTabPage1.Controls.Add(this.btn_Login);
            this.skinTabPage1.Controls.Add(this.tbx_LoginPwd);
            this.skinTabPage1.Controls.Add(this.tbx_LoginUser);
            this.skinTabPage1.Controls.Add(this.skinLabel2);
            this.skinTabPage1.Controls.Add(this.skinLabel1);
            this.skinTabPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.skinTabPage1.Location = new System.Drawing.Point(0, 23);
            this.skinTabPage1.Name = "skinTabPage1";
            this.skinTabPage1.Size = new System.Drawing.Size(428, 161);
            this.skinTabPage1.TabIndex = 0;
            this.skinTabPage1.TabItemImage = null;
            this.skinTabPage1.Text = "登录";
            // 
            // chkAutoLogin
            // 
            this.chkAutoLogin.AutoSize = true;
            this.chkAutoLogin.BackColor = System.Drawing.Color.Transparent;
            this.chkAutoLogin.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.chkAutoLogin.DownBack = null;
            this.chkAutoLogin.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chkAutoLogin.Location = new System.Drawing.Point(93, 108);
            this.chkAutoLogin.MouseBack = null;
            this.chkAutoLogin.Name = "chkAutoLogin";
            this.chkAutoLogin.NormlBack = null;
            this.chkAutoLogin.SelectedDownBack = null;
            this.chkAutoLogin.SelectedMouseBack = null;
            this.chkAutoLogin.SelectedNormlBack = null;
            this.chkAutoLogin.Size = new System.Drawing.Size(75, 21);
            this.chkAutoLogin.TabIndex = 4;
            this.chkAutoLogin.Text = "自动登录";
            this.chkAutoLogin.UseVisualStyleBackColor = false;
            // 
            // btn_Login
            // 
            this.btn_Login.BackColor = System.Drawing.Color.Transparent;
            this.btn_Login.BaseColor = System.Drawing.Color.Silver;
            this.btn_Login.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_Login.DownBack = null;
            this.btn_Login.Location = new System.Drawing.Point(174, 108);
            this.btn_Login.MouseBack = null;
            this.btn_Login.Name = "btn_Login";
            this.btn_Login.NormlBack = null;
            this.btn_Login.Size = new System.Drawing.Size(145, 23);
            this.btn_Login.TabIndex = 3;
            this.btn_Login.Text = "登录";
            this.btn_Login.UseVisualStyleBackColor = false;
            this.btn_Login.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // tbx_LoginPwd
            // 
            this.tbx_LoginPwd.BackColor = System.Drawing.Color.Transparent;
            this.tbx_LoginPwd.DownBack = null;
            this.tbx_LoginPwd.Icon = null;
            this.tbx_LoginPwd.IconIsButton = false;
            this.tbx_LoginPwd.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_LoginPwd.IsPasswordChat = '*';
            this.tbx_LoginPwd.IsSystemPasswordChar = false;
            this.tbx_LoginPwd.Lines = new string[0];
            this.tbx_LoginPwd.Location = new System.Drawing.Point(134, 66);
            this.tbx_LoginPwd.Margin = new System.Windows.Forms.Padding(0);
            this.tbx_LoginPwd.MaxLength = 32767;
            this.tbx_LoginPwd.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbx_LoginPwd.MouseBack = null;
            this.tbx_LoginPwd.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_LoginPwd.Multiline = false;
            this.tbx_LoginPwd.Name = "tbx_LoginPwd";
            this.tbx_LoginPwd.NormlBack = null;
            this.tbx_LoginPwd.Padding = new System.Windows.Forms.Padding(5);
            this.tbx_LoginPwd.ReadOnly = false;
            this.tbx_LoginPwd.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbx_LoginPwd.Size = new System.Drawing.Size(185, 28);
            // 
            // 
            // 
            this.tbx_LoginPwd.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_LoginPwd.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_LoginPwd.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_LoginPwd.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_LoginPwd.SkinTxt.Name = "BaseText";
            this.tbx_LoginPwd.SkinTxt.PasswordChar = '*';
            this.tbx_LoginPwd.SkinTxt.Size = new System.Drawing.Size(175, 18);
            this.tbx_LoginPwd.SkinTxt.TabIndex = 0;
            this.tbx_LoginPwd.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_LoginPwd.SkinTxt.WaterText = "";
            this.tbx_LoginPwd.TabIndex = 2;
            this.tbx_LoginPwd.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbx_LoginPwd.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_LoginPwd.WaterText = "";
            this.tbx_LoginPwd.WordWrap = true;
            // 
            // tbx_LoginUser
            // 
            this.tbx_LoginUser.BackColor = System.Drawing.Color.Transparent;
            this.tbx_LoginUser.DownBack = null;
            this.tbx_LoginUser.Icon = null;
            this.tbx_LoginUser.IconIsButton = false;
            this.tbx_LoginUser.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_LoginUser.IsPasswordChat = '\0';
            this.tbx_LoginUser.IsSystemPasswordChar = false;
            this.tbx_LoginUser.Lines = new string[0];
            this.tbx_LoginUser.Location = new System.Drawing.Point(134, 34);
            this.tbx_LoginUser.Margin = new System.Windows.Forms.Padding(0);
            this.tbx_LoginUser.MaxLength = 32767;
            this.tbx_LoginUser.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbx_LoginUser.MouseBack = null;
            this.tbx_LoginUser.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_LoginUser.Multiline = false;
            this.tbx_LoginUser.Name = "tbx_LoginUser";
            this.tbx_LoginUser.NormlBack = null;
            this.tbx_LoginUser.Padding = new System.Windows.Forms.Padding(5);
            this.tbx_LoginUser.ReadOnly = false;
            this.tbx_LoginUser.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbx_LoginUser.Size = new System.Drawing.Size(185, 28);
            // 
            // 
            // 
            this.tbx_LoginUser.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_LoginUser.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_LoginUser.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_LoginUser.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_LoginUser.SkinTxt.Name = "BaseText";
            this.tbx_LoginUser.SkinTxt.Size = new System.Drawing.Size(175, 18);
            this.tbx_LoginUser.SkinTxt.TabIndex = 0;
            this.tbx_LoginUser.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_LoginUser.SkinTxt.WaterText = "";
            this.tbx_LoginUser.TabIndex = 1;
            this.tbx_LoginUser.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbx_LoginUser.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_LoginUser.WaterText = "";
            this.tbx_LoginUser.WordWrap = true;
            // 
            // skinLabel2
            // 
            this.skinLabel2.AutoSize = true;
            this.skinLabel2.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel2.Location = new System.Drawing.Point(89, 66);
            this.skinLabel2.Name = "skinLabel2";
            this.skinLabel2.Size = new System.Drawing.Size(42, 21);
            this.skinLabel2.TabIndex = 1;
            this.skinLabel2.Text = "密码";
            // 
            // skinLabel1
            // 
            this.skinLabel1.AutoSize = true;
            this.skinLabel1.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel1.Location = new System.Drawing.Point(89, 34);
            this.skinLabel1.Name = "skinLabel1";
            this.skinLabel1.Size = new System.Drawing.Size(42, 21);
            this.skinLabel1.TabIndex = 0;
            this.skinLabel1.Text = "账号";
            // 
            // skinTabPage2
            // 
            this.skinTabPage2.BackColor = System.Drawing.Color.White;
            this.skinTabPage2.Controls.Add(this.btn_PwRecover);
            this.skinTabPage2.Controls.Add(this.tbx_PwdRecoverConfirm);
            this.skinTabPage2.Controls.Add(this.tbx_PwdRecoverNew);
            this.skinTabPage2.Controls.Add(this.tbx_PwdRecoverMessage);
            this.skinTabPage2.Controls.Add(this.tbx_PwdRecoverPass);
            this.skinTabPage2.Controls.Add(this.tbx_PwdRecoverUser);
            this.skinTabPage2.Controls.Add(this.skinLabel6);
            this.skinTabPage2.Controls.Add(this.skinLabel5);
            this.skinTabPage2.Controls.Add(this.skinLabel3);
            this.skinTabPage2.Controls.Add(this.skinLabel9);
            this.skinTabPage2.Controls.Add(this.skinLabel4);
            this.skinTabPage2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.skinTabPage2.Location = new System.Drawing.Point(0, 23);
            this.skinTabPage2.Name = "skinTabPage2";
            this.skinTabPage2.Size = new System.Drawing.Size(428, 161);
            this.skinTabPage2.TabIndex = 1;
            this.skinTabPage2.TabItemImage = null;
            this.skinTabPage2.Text = "密码找回";
            // 
            // btn_PwRecover
            // 
            this.btn_PwRecover.BackColor = System.Drawing.Color.Transparent;
            this.btn_PwRecover.BaseColor = System.Drawing.Color.Silver;
            this.btn_PwRecover.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_PwRecover.DownBack = null;
            this.btn_PwRecover.Location = new System.Drawing.Point(281, 133);
            this.btn_PwRecover.MouseBack = null;
            this.btn_PwRecover.Name = "btn_PwRecover";
            this.btn_PwRecover.NormlBack = null;
            this.btn_PwRecover.Size = new System.Drawing.Size(136, 23);
            this.btn_PwRecover.TabIndex = 7;
            this.btn_PwRecover.Text = "重置密码";
            this.btn_PwRecover.UseVisualStyleBackColor = false;
            this.btn_PwRecover.Click += new System.EventHandler(this.btn_PwRecover_Click);
            // 
            // tbx_PwdRecoverConfirm
            // 
            this.tbx_PwdRecoverConfirm.BackColor = System.Drawing.Color.Transparent;
            this.tbx_PwdRecoverConfirm.DownBack = null;
            this.tbx_PwdRecoverConfirm.Icon = null;
            this.tbx_PwdRecoverConfirm.IconIsButton = false;
            this.tbx_PwdRecoverConfirm.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_PwdRecoverConfirm.IsPasswordChat = '\0';
            this.tbx_PwdRecoverConfirm.IsSystemPasswordChar = false;
            this.tbx_PwdRecoverConfirm.Lines = new string[0];
            this.tbx_PwdRecoverConfirm.Location = new System.Drawing.Point(93, 130);
            this.tbx_PwdRecoverConfirm.Margin = new System.Windows.Forms.Padding(0);
            this.tbx_PwdRecoverConfirm.MaxLength = 32767;
            this.tbx_PwdRecoverConfirm.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbx_PwdRecoverConfirm.MouseBack = null;
            this.tbx_PwdRecoverConfirm.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_PwdRecoverConfirm.Multiline = false;
            this.tbx_PwdRecoverConfirm.Name = "tbx_PwdRecoverConfirm";
            this.tbx_PwdRecoverConfirm.NormlBack = null;
            this.tbx_PwdRecoverConfirm.Padding = new System.Windows.Forms.Padding(5);
            this.tbx_PwdRecoverConfirm.ReadOnly = false;
            this.tbx_PwdRecoverConfirm.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbx_PwdRecoverConfirm.Size = new System.Drawing.Size(185, 28);
            // 
            // 
            // 
            this.tbx_PwdRecoverConfirm.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_PwdRecoverConfirm.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_PwdRecoverConfirm.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_PwdRecoverConfirm.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_PwdRecoverConfirm.SkinTxt.Name = "BaseText";
            this.tbx_PwdRecoverConfirm.SkinTxt.Size = new System.Drawing.Size(175, 18);
            this.tbx_PwdRecoverConfirm.SkinTxt.TabIndex = 0;
            this.tbx_PwdRecoverConfirm.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_PwdRecoverConfirm.SkinTxt.WaterText = "";
            this.tbx_PwdRecoverConfirm.TabIndex = 5;
            this.tbx_PwdRecoverConfirm.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbx_PwdRecoverConfirm.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_PwdRecoverConfirm.WaterText = "";
            this.tbx_PwdRecoverConfirm.WordWrap = true;
            // 
            // tbx_PwdRecoverNew
            // 
            this.tbx_PwdRecoverNew.BackColor = System.Drawing.Color.Transparent;
            this.tbx_PwdRecoverNew.DownBack = null;
            this.tbx_PwdRecoverNew.Icon = null;
            this.tbx_PwdRecoverNew.IconIsButton = false;
            this.tbx_PwdRecoverNew.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_PwdRecoverNew.IsPasswordChat = '\0';
            this.tbx_PwdRecoverNew.IsSystemPasswordChar = false;
            this.tbx_PwdRecoverNew.Lines = new string[0];
            this.tbx_PwdRecoverNew.Location = new System.Drawing.Point(93, 96);
            this.tbx_PwdRecoverNew.Margin = new System.Windows.Forms.Padding(0);
            this.tbx_PwdRecoverNew.MaxLength = 32767;
            this.tbx_PwdRecoverNew.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbx_PwdRecoverNew.MouseBack = null;
            this.tbx_PwdRecoverNew.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_PwdRecoverNew.Multiline = false;
            this.tbx_PwdRecoverNew.Name = "tbx_PwdRecoverNew";
            this.tbx_PwdRecoverNew.NormlBack = null;
            this.tbx_PwdRecoverNew.Padding = new System.Windows.Forms.Padding(5);
            this.tbx_PwdRecoverNew.ReadOnly = false;
            this.tbx_PwdRecoverNew.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbx_PwdRecoverNew.Size = new System.Drawing.Size(185, 28);
            // 
            // 
            // 
            this.tbx_PwdRecoverNew.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_PwdRecoverNew.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_PwdRecoverNew.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_PwdRecoverNew.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_PwdRecoverNew.SkinTxt.Name = "BaseText";
            this.tbx_PwdRecoverNew.SkinTxt.Size = new System.Drawing.Size(175, 18);
            this.tbx_PwdRecoverNew.SkinTxt.TabIndex = 0;
            this.tbx_PwdRecoverNew.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_PwdRecoverNew.SkinTxt.WaterText = "";
            this.tbx_PwdRecoverNew.TabIndex = 5;
            this.tbx_PwdRecoverNew.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbx_PwdRecoverNew.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_PwdRecoverNew.WaterText = "";
            this.tbx_PwdRecoverNew.WordWrap = true;
            // 
            // tbx_PwdRecoverMessage
            // 
            this.tbx_PwdRecoverMessage.BackColor = System.Drawing.Color.Transparent;
            this.tbx_PwdRecoverMessage.DownBack = null;
            this.tbx_PwdRecoverMessage.Icon = null;
            this.tbx_PwdRecoverMessage.IconIsButton = false;
            this.tbx_PwdRecoverMessage.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_PwdRecoverMessage.IsPasswordChat = '\0';
            this.tbx_PwdRecoverMessage.IsSystemPasswordChar = false;
            this.tbx_PwdRecoverMessage.Lines = new string[0];
            this.tbx_PwdRecoverMessage.Location = new System.Drawing.Point(93, 65);
            this.tbx_PwdRecoverMessage.Margin = new System.Windows.Forms.Padding(0);
            this.tbx_PwdRecoverMessage.MaxLength = 32767;
            this.tbx_PwdRecoverMessage.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbx_PwdRecoverMessage.MouseBack = null;
            this.tbx_PwdRecoverMessage.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_PwdRecoverMessage.Multiline = false;
            this.tbx_PwdRecoverMessage.Name = "tbx_PwdRecoverMessage";
            this.tbx_PwdRecoverMessage.NormlBack = null;
            this.tbx_PwdRecoverMessage.Padding = new System.Windows.Forms.Padding(5);
            this.tbx_PwdRecoverMessage.ReadOnly = false;
            this.tbx_PwdRecoverMessage.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbx_PwdRecoverMessage.Size = new System.Drawing.Size(185, 28);
            // 
            // 
            // 
            this.tbx_PwdRecoverMessage.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_PwdRecoverMessage.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_PwdRecoverMessage.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_PwdRecoverMessage.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_PwdRecoverMessage.SkinTxt.Name = "BaseText";
            this.tbx_PwdRecoverMessage.SkinTxt.Size = new System.Drawing.Size(175, 18);
            this.tbx_PwdRecoverMessage.SkinTxt.TabIndex = 0;
            this.tbx_PwdRecoverMessage.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_PwdRecoverMessage.SkinTxt.WaterText = "";
            this.tbx_PwdRecoverMessage.TabIndex = 5;
            this.tbx_PwdRecoverMessage.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbx_PwdRecoverMessage.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_PwdRecoverMessage.WaterText = "";
            this.tbx_PwdRecoverMessage.WordWrap = true;
            // 
            // tbx_PwdRecoverPass
            // 
            this.tbx_PwdRecoverPass.BackColor = System.Drawing.Color.Transparent;
            this.tbx_PwdRecoverPass.DownBack = null;
            this.tbx_PwdRecoverPass.Icon = null;
            this.tbx_PwdRecoverPass.IconIsButton = false;
            this.tbx_PwdRecoverPass.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_PwdRecoverPass.IsPasswordChat = '*';
            this.tbx_PwdRecoverPass.IsSystemPasswordChar = false;
            this.tbx_PwdRecoverPass.Lines = new string[0];
            this.tbx_PwdRecoverPass.Location = new System.Drawing.Point(93, 32);
            this.tbx_PwdRecoverPass.Margin = new System.Windows.Forms.Padding(0);
            this.tbx_PwdRecoverPass.MaxLength = 32767;
            this.tbx_PwdRecoverPass.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbx_PwdRecoverPass.MouseBack = null;
            this.tbx_PwdRecoverPass.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_PwdRecoverPass.Multiline = false;
            this.tbx_PwdRecoverPass.Name = "tbx_PwdRecoverPass";
            this.tbx_PwdRecoverPass.NormlBack = null;
            this.tbx_PwdRecoverPass.Padding = new System.Windows.Forms.Padding(5);
            this.tbx_PwdRecoverPass.ReadOnly = false;
            this.tbx_PwdRecoverPass.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbx_PwdRecoverPass.Size = new System.Drawing.Size(185, 28);
            // 
            // 
            // 
            this.tbx_PwdRecoverPass.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_PwdRecoverPass.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_PwdRecoverPass.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_PwdRecoverPass.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_PwdRecoverPass.SkinTxt.Name = "BaseText";
            this.tbx_PwdRecoverPass.SkinTxt.PasswordChar = '*';
            this.tbx_PwdRecoverPass.SkinTxt.Size = new System.Drawing.Size(175, 18);
            this.tbx_PwdRecoverPass.SkinTxt.TabIndex = 0;
            this.tbx_PwdRecoverPass.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_PwdRecoverPass.SkinTxt.WaterText = "";
            this.tbx_PwdRecoverPass.TabIndex = 6;
            this.tbx_PwdRecoverPass.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbx_PwdRecoverPass.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_PwdRecoverPass.WaterText = "";
            this.tbx_PwdRecoverPass.WordWrap = true;
            // 
            // tbx_PwdRecoverUser
            // 
            this.tbx_PwdRecoverUser.BackColor = System.Drawing.Color.Transparent;
            this.tbx_PwdRecoverUser.DownBack = null;
            this.tbx_PwdRecoverUser.Icon = null;
            this.tbx_PwdRecoverUser.IconIsButton = false;
            this.tbx_PwdRecoverUser.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_PwdRecoverUser.IsPasswordChat = '\0';
            this.tbx_PwdRecoverUser.IsSystemPasswordChar = false;
            this.tbx_PwdRecoverUser.Lines = new string[0];
            this.tbx_PwdRecoverUser.Location = new System.Drawing.Point(93, 0);
            this.tbx_PwdRecoverUser.Margin = new System.Windows.Forms.Padding(0);
            this.tbx_PwdRecoverUser.MaxLength = 32767;
            this.tbx_PwdRecoverUser.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbx_PwdRecoverUser.MouseBack = null;
            this.tbx_PwdRecoverUser.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_PwdRecoverUser.Multiline = false;
            this.tbx_PwdRecoverUser.Name = "tbx_PwdRecoverUser";
            this.tbx_PwdRecoverUser.NormlBack = null;
            this.tbx_PwdRecoverUser.Padding = new System.Windows.Forms.Padding(5);
            this.tbx_PwdRecoverUser.ReadOnly = false;
            this.tbx_PwdRecoverUser.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbx_PwdRecoverUser.Size = new System.Drawing.Size(185, 28);
            // 
            // 
            // 
            this.tbx_PwdRecoverUser.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_PwdRecoverUser.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_PwdRecoverUser.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_PwdRecoverUser.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_PwdRecoverUser.SkinTxt.Name = "BaseText";
            this.tbx_PwdRecoverUser.SkinTxt.Size = new System.Drawing.Size(175, 18);
            this.tbx_PwdRecoverUser.SkinTxt.TabIndex = 0;
            this.tbx_PwdRecoverUser.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_PwdRecoverUser.SkinTxt.WaterText = "";
            this.tbx_PwdRecoverUser.TabIndex = 6;
            this.tbx_PwdRecoverUser.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbx_PwdRecoverUser.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_PwdRecoverUser.WaterText = "";
            this.tbx_PwdRecoverUser.WordWrap = true;
            // 
            // skinLabel6
            // 
            this.skinLabel6.AutoSize = true;
            this.skinLabel6.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel6.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel6.Location = new System.Drawing.Point(0, 131);
            this.skinLabel6.Name = "skinLabel6";
            this.skinLabel6.Size = new System.Drawing.Size(90, 21);
            this.skinLabel6.TabIndex = 4;
            this.skinLabel6.Text = "确认新密码";
            // 
            // skinLabel5
            // 
            this.skinLabel5.AutoSize = true;
            this.skinLabel5.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel5.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel5.Location = new System.Drawing.Point(14, 99);
            this.skinLabel5.Name = "skinLabel5";
            this.skinLabel5.Size = new System.Drawing.Size(58, 21);
            this.skinLabel5.TabIndex = 4;
            this.skinLabel5.Text = "新密码";
            // 
            // skinLabel3
            // 
            this.skinLabel3.AutoSize = true;
            this.skinLabel3.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel3.Location = new System.Drawing.Point(6, 68);
            this.skinLabel3.Name = "skinLabel3";
            this.skinLabel3.Size = new System.Drawing.Size(74, 21);
            this.skinLabel3.TabIndex = 4;
            this.skinLabel3.Text = "预留信息";
            // 
            // skinLabel9
            // 
            this.skinLabel9.AutoSize = true;
            this.skinLabel9.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel9.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel9.Location = new System.Drawing.Point(22, 34);
            this.skinLabel9.Name = "skinLabel9";
            this.skinLabel9.Size = new System.Drawing.Size(42, 21);
            this.skinLabel9.TabIndex = 3;
            this.skinLabel9.Text = "密码";
            // 
            // skinLabel4
            // 
            this.skinLabel4.AutoSize = true;
            this.skinLabel4.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel4.Location = new System.Drawing.Point(22, 4);
            this.skinLabel4.Name = "skinLabel4";
            this.skinLabel4.Size = new System.Drawing.Size(42, 21);
            this.skinLabel4.TabIndex = 3;
            this.skinLabel4.Text = "账号";
            // 
            // skinTabPage3
            // 
            this.skinTabPage3.BackColor = System.Drawing.Color.White;
            this.skinTabPage3.Controls.Add(this.btnAddCredit);
            this.skinTabPage3.Controls.Add(this.tbx_CarNumber);
            this.skinTabPage3.Controls.Add(this.tbx_TbUser);
            this.skinTabPage3.Controls.Add(this.skinLabel8);
            this.skinTabPage3.Controls.Add(this.skinLabel7);
            this.skinTabPage3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.skinTabPage3.Location = new System.Drawing.Point(0, 23);
            this.skinTabPage3.Name = "skinTabPage3";
            this.skinTabPage3.Size = new System.Drawing.Size(428, 161);
            this.skinTabPage3.TabIndex = 2;
            this.skinTabPage3.TabItemImage = null;
            this.skinTabPage3.Text = "特别";
            // 
            // btnAddCredit
            // 
            this.btnAddCredit.BackColor = System.Drawing.Color.Transparent;
            this.btnAddCredit.BaseColor = System.Drawing.Color.Silver;
            this.btnAddCredit.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnAddCredit.DownBack = null;
            this.btnAddCredit.Location = new System.Drawing.Point(80, 70);
            this.btnAddCredit.MouseBack = null;
            this.btnAddCredit.Name = "btnAddCredit";
            this.btnAddCredit.NormlBack = null;
            this.btnAddCredit.Size = new System.Drawing.Size(102, 23);
            this.btnAddCredit.TabIndex = 9;
            this.btnAddCredit.Text = "确认";
            this.btnAddCredit.UseVisualStyleBackColor = false;
            this.btnAddCredit.Click += new System.EventHandler(this.btnAddCredit_Click);
            // 
            // tbx_CarNumber
            // 
            this.tbx_CarNumber.BackColor = System.Drawing.Color.Transparent;
            this.tbx_CarNumber.DownBack = null;
            this.tbx_CarNumber.Icon = null;
            this.tbx_CarNumber.IconIsButton = false;
            this.tbx_CarNumber.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_CarNumber.IsPasswordChat = '\0';
            this.tbx_CarNumber.IsSystemPasswordChar = false;
            this.tbx_CarNumber.Lines = new string[0];
            this.tbx_CarNumber.Location = new System.Drawing.Point(80, 39);
            this.tbx_CarNumber.Margin = new System.Windows.Forms.Padding(0);
            this.tbx_CarNumber.MaxLength = 32767;
            this.tbx_CarNumber.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbx_CarNumber.MouseBack = null;
            this.tbx_CarNumber.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_CarNumber.Multiline = false;
            this.tbx_CarNumber.Name = "tbx_CarNumber";
            this.tbx_CarNumber.NormlBack = null;
            this.tbx_CarNumber.Padding = new System.Windows.Forms.Padding(5);
            this.tbx_CarNumber.ReadOnly = false;
            this.tbx_CarNumber.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbx_CarNumber.Size = new System.Drawing.Size(337, 28);
            // 
            // 
            // 
            this.tbx_CarNumber.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_CarNumber.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_CarNumber.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_CarNumber.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_CarNumber.SkinTxt.Name = "BaseText";
            this.tbx_CarNumber.SkinTxt.Size = new System.Drawing.Size(327, 18);
            this.tbx_CarNumber.SkinTxt.TabIndex = 0;
            this.tbx_CarNumber.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_CarNumber.SkinTxt.WaterText = "";
            this.tbx_CarNumber.TabIndex = 8;
            this.tbx_CarNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbx_CarNumber.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_CarNumber.WaterText = "";
            this.tbx_CarNumber.WordWrap = true;
            // 
            // tbx_TbUser
            // 
            this.tbx_TbUser.BackColor = System.Drawing.Color.Transparent;
            this.tbx_TbUser.DownBack = null;
            this.tbx_TbUser.Icon = null;
            this.tbx_TbUser.IconIsButton = false;
            this.tbx_TbUser.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_TbUser.IsPasswordChat = '\0';
            this.tbx_TbUser.IsSystemPasswordChar = false;
            this.tbx_TbUser.Lines = new string[0];
            this.tbx_TbUser.Location = new System.Drawing.Point(80, 6);
            this.tbx_TbUser.Margin = new System.Windows.Forms.Padding(0);
            this.tbx_TbUser.MaxLength = 32767;
            this.tbx_TbUser.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbx_TbUser.MouseBack = null;
            this.tbx_TbUser.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_TbUser.Multiline = false;
            this.tbx_TbUser.Name = "tbx_TbUser";
            this.tbx_TbUser.NormlBack = null;
            this.tbx_TbUser.Padding = new System.Windows.Forms.Padding(5);
            this.tbx_TbUser.ReadOnly = false;
            this.tbx_TbUser.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbx_TbUser.Size = new System.Drawing.Size(185, 28);
            // 
            // 
            // 
            this.tbx_TbUser.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_TbUser.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_TbUser.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_TbUser.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_TbUser.SkinTxt.Name = "BaseText";
            this.tbx_TbUser.SkinTxt.Size = new System.Drawing.Size(175, 18);
            this.tbx_TbUser.SkinTxt.TabIndex = 0;
            this.tbx_TbUser.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_TbUser.SkinTxt.WaterText = "";
            this.tbx_TbUser.TabIndex = 8;
            this.tbx_TbUser.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbx_TbUser.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_TbUser.WaterText = "";
            this.tbx_TbUser.WordWrap = true;
            // 
            // skinLabel8
            // 
            this.skinLabel8.AutoSize = true;
            this.skinLabel8.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel8.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel8.Location = new System.Drawing.Point(3, 43);
            this.skinLabel8.Name = "skinLabel8";
            this.skinLabel8.Size = new System.Drawing.Size(58, 21);
            this.skinLabel8.TabIndex = 7;
            this.skinLabel8.Text = "特别号";
            // 
            // skinLabel7
            // 
            this.skinLabel7.AutoSize = true;
            this.skinLabel7.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel7.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel7.Location = new System.Drawing.Point(18, 8);
            this.skinLabel7.Name = "skinLabel7";
            this.skinLabel7.Size = new System.Drawing.Size(42, 21);
            this.skinLabel7.TabIndex = 7;
            this.skinLabel7.Text = "账号";
            // 
            // LoginView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGray;
            this.ClientSize = new System.Drawing.Size(436, 216);
            this.Controls.Add(this.tab_LoginMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LoginView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "v - 3.61.**";
            this.Load += new System.EventHandler(this.LoginView_Load);
            this.tab_LoginMain.ResumeLayout(false);
            this.skinTabPage1.ResumeLayout(false);
            this.skinTabPage1.PerformLayout();
            this.skinTabPage2.ResumeLayout(false);
            this.skinTabPage2.PerformLayout();
            this.skinTabPage3.ResumeLayout(false);
            this.skinTabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinTabControl tab_LoginMain;
        private CCWin.SkinControl.SkinTabPage skinTabPage1;
        private CCWin.SkinControl.SkinTabPage skinTabPage2;
        private CCWin.SkinControl.SkinTabPage skinTabPage3;
        private System.Windows.Forms.Label skinLabel2;
        private System.Windows.Forms.Label skinLabel1;
        private CCWin.SkinControl.SkinTextBox tbx_LoginPwd;
        private CCWin.SkinControl.SkinTextBox tbx_LoginUser;
        private CCWin.SkinControl.SkinButton btn_Login;
        private CCWin.SkinControl.SkinCheckBox chkAutoLogin;
        private CCWin.SkinControl.SkinTextBox tbx_PwdRecoverMessage;
        private CCWin.SkinControl.SkinTextBox tbx_PwdRecoverUser;
        private System.Windows.Forms.Label skinLabel3;
        private System.Windows.Forms.Label skinLabel4;
        private CCWin.SkinControl.SkinButton btn_PwRecover;
        private CCWin.SkinControl.SkinTextBox tbx_PwdRecoverNew;
        private System.Windows.Forms.Label skinLabel5;
        private CCWin.SkinControl.SkinTextBox tbx_PwdRecoverConfirm;
        private System.Windows.Forms.Label skinLabel6;
        private CCWin.SkinControl.SkinButton btnAddCredit;
        private CCWin.SkinControl.SkinTextBox tbx_CarNumber;
        private CCWin.SkinControl.SkinTextBox tbx_TbUser;
        private System.Windows.Forms.Label skinLabel8;
        private System.Windows.Forms.Label skinLabel7;
        private CCWin.SkinControl.SkinTextBox tbx_PwdRecoverPass;
        private System.Windows.Forms.Label skinLabel9;
    }
}