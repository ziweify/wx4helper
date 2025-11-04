namespace F5BotV2.CefBrowser
{
    partial class XCefBrowserView
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
            this.airForm1 = new ReaLTaiizor.Forms.AirForm();
            this.pnl_top = new CCWin.SkinControl.SkinPanel();
            this.foxLabel1 = new ReaLTaiizor.Controls.FoxLabel();
            this.skinButton2 = new CCWin.SkinControl.SkinButton();
            this.skinButton1 = new CCWin.SkinControl.SkinButton();
            this.btn_go = new CCWin.SkinControl.SkinButton();
            this.btnGetCookie = new CCWin.SkinControl.SkinButton();
            this.tbx_url = new CCWin.SkinControl.SkinTextBox();
            this.pnl_chromeBrowser = new CCWin.SkinControl.SkinPanel();
            this.airForm1.SuspendLayout();
            this.pnl_top.SuspendLayout();
            this.SuspendLayout();
            // 
            // airForm1
            // 
            this.airForm1.BackColor = System.Drawing.Color.White;
            this.airForm1.BorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.airForm1.Controls.Add(this.pnl_top);
            this.airForm1.Controls.Add(this.pnl_chromeBrowser);
            this.airForm1.Customization = "AAAA/1paWv9ycnL/";
            this.airForm1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.airForm1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.airForm1.Image = null;
            this.airForm1.Location = new System.Drawing.Point(0, 0);
            this.airForm1.MinimumSize = new System.Drawing.Size(112, 35);
            this.airForm1.Movable = true;
            this.airForm1.Name = "airForm1";
            this.airForm1.NoRounding = false;
            this.airForm1.Sizable = true;
            this.airForm1.Size = new System.Drawing.Size(1154, 803);
            this.airForm1.SmartBounds = true;
            this.airForm1.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.airForm1.TabIndex = 0;
            this.airForm1.Text = "迷你";
            this.airForm1.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.airForm1.Transparent = false;
            // 
            // pnl_top
            // 
            this.pnl_top.BackColor = System.Drawing.Color.White;
            this.pnl_top.Controls.Add(this.foxLabel1);
            this.pnl_top.Controls.Add(this.skinButton2);
            this.pnl_top.Controls.Add(this.skinButton1);
            this.pnl_top.Controls.Add(this.btn_go);
            this.pnl_top.Controls.Add(this.btnGetCookie);
            this.pnl_top.Controls.Add(this.tbx_url);
            this.pnl_top.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.pnl_top.DownBack = null;
            this.pnl_top.Location = new System.Drawing.Point(3, 30);
            this.pnl_top.MouseBack = null;
            this.pnl_top.Name = "pnl_top";
            this.pnl_top.NormlBack = null;
            this.pnl_top.Size = new System.Drawing.Size(1154, 33);
            this.pnl_top.TabIndex = 2;
            // 
            // foxLabel1
            // 
            this.foxLabel1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.foxLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(88)))), ((int)(((byte)(100)))));
            this.foxLabel1.Location = new System.Drawing.Point(88, 8);
            this.foxLabel1.Name = "foxLabel1";
            this.foxLabel1.Size = new System.Drawing.Size(34, 19);
            this.foxLabel1.TabIndex = 6;
            this.foxLabel1.Text = "链接";
            // 
            // skinButton2
            // 
            this.skinButton2.BackColor = System.Drawing.Color.Transparent;
            this.skinButton2.BaseColor = System.Drawing.Color.LightGray;
            this.skinButton2.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.skinButton2.DownBack = null;
            this.skinButton2.Location = new System.Drawing.Point(43, 4);
            this.skinButton2.MouseBack = null;
            this.skinButton2.Name = "skinButton2";
            this.skinButton2.NormlBack = null;
            this.skinButton2.Size = new System.Drawing.Size(42, 23);
            this.skinButton2.TabIndex = 5;
            this.skinButton2.Text = "前进";
            this.skinButton2.UseVisualStyleBackColor = false;
            this.skinButton2.Click += new System.EventHandler(this.skinButton2_Click);
            // 
            // skinButton1
            // 
            this.skinButton1.BackColor = System.Drawing.Color.Transparent;
            this.skinButton1.BaseColor = System.Drawing.Color.LightGray;
            this.skinButton1.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.skinButton1.DownBack = null;
            this.skinButton1.Location = new System.Drawing.Point(0, 4);
            this.skinButton1.MouseBack = null;
            this.skinButton1.Name = "skinButton1";
            this.skinButton1.NormlBack = null;
            this.skinButton1.Size = new System.Drawing.Size(42, 23);
            this.skinButton1.TabIndex = 5;
            this.skinButton1.Text = "后退";
            this.skinButton1.UseVisualStyleBackColor = false;
            this.skinButton1.Click += new System.EventHandler(this.skinButton1_Click);
            // 
            // btn_go
            // 
            this.btn_go.BackColor = System.Drawing.Color.Transparent;
            this.btn_go.BaseColor = System.Drawing.Color.LightGray;
            this.btn_go.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_go.DownBack = null;
            this.btn_go.Location = new System.Drawing.Point(1030, 4);
            this.btn_go.MouseBack = null;
            this.btn_go.Name = "btn_go";
            this.btn_go.NormlBack = null;
            this.btn_go.Size = new System.Drawing.Size(42, 23);
            this.btn_go.TabIndex = 5;
            this.btn_go.Text = "转到";
            this.btn_go.UseVisualStyleBackColor = false;
            this.btn_go.Click += new System.EventHandler(this.btn_go_Click);
            // 
            // btnGetCookie
            // 
            this.btnGetCookie.BackColor = System.Drawing.Color.Transparent;
            this.btnGetCookie.BaseColor = System.Drawing.Color.LightGray;
            this.btnGetCookie.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnGetCookie.DownBack = null;
            this.btnGetCookie.Location = new System.Drawing.Point(1075, 4);
            this.btnGetCookie.MouseBack = null;
            this.btnGetCookie.Name = "btnGetCookie";
            this.btnGetCookie.NormlBack = null;
            this.btnGetCookie.Size = new System.Drawing.Size(43, 23);
            this.btnGetCookie.TabIndex = 5;
            this.btnGetCookie.Text = "信息";
            this.btnGetCookie.UseVisualStyleBackColor = false;
            this.btnGetCookie.Click += new System.EventHandler(this.btnGetCookie_Click);
            // 
            // tbx_url
            // 
            this.tbx_url.BackColor = System.Drawing.Color.Transparent;
            this.tbx_url.DownBack = null;
            this.tbx_url.Icon = null;
            this.tbx_url.IconIsButton = false;
            this.tbx_url.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_url.IsPasswordChat = '\0';
            this.tbx_url.IsSystemPasswordChar = false;
            this.tbx_url.Lines = new string[0];
            this.tbx_url.Location = new System.Drawing.Point(125, 2);
            this.tbx_url.Margin = new System.Windows.Forms.Padding(0);
            this.tbx_url.MaxLength = 32767;
            this.tbx_url.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbx_url.MouseBack = null;
            this.tbx_url.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbx_url.Multiline = false;
            this.tbx_url.Name = "tbx_url";
            this.tbx_url.NormlBack = null;
            this.tbx_url.Padding = new System.Windows.Forms.Padding(5);
            this.tbx_url.ReadOnly = false;
            this.tbx_url.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbx_url.Size = new System.Drawing.Size(902, 28);
            // 
            // 
            // 
            this.tbx_url.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_url.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_url.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_url.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_url.SkinTxt.Name = "BaseText";
            this.tbx_url.SkinTxt.Size = new System.Drawing.Size(892, 18);
            this.tbx_url.SkinTxt.TabIndex = 0;
            this.tbx_url.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_url.SkinTxt.WaterText = "";
            this.tbx_url.TabIndex = 0;
            this.tbx_url.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbx_url.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbx_url.WaterText = "";
            this.tbx_url.WordWrap = true;
            // 
            // pnl_chromeBrowser
            // 
            this.pnl_chromeBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_chromeBrowser.BackColor = System.Drawing.Color.Transparent;
            this.pnl_chromeBrowser.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.pnl_chromeBrowser.DownBack = null;
            this.pnl_chromeBrowser.Location = new System.Drawing.Point(0, 63);
            this.pnl_chromeBrowser.MouseBack = null;
            this.pnl_chromeBrowser.Name = "pnl_chromeBrowser";
            this.pnl_chromeBrowser.NormlBack = null;
            this.pnl_chromeBrowser.Size = new System.Drawing.Size(1154, 740);
            this.pnl_chromeBrowser.TabIndex = 3;
            // 
            // XCefBrowserView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1154, 803);
            this.Controls.Add(this.airForm1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(112, 35);
            this.Name = "XCefBrowserView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CefBrowserView_FormClosing);
            this.airForm1.ResumeLayout(false);
            this.pnl_top.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ReaLTaiizor.Forms.AirForm airForm1;
        private CCWin.SkinControl.SkinPanel pnl_top;
        public CCWin.SkinControl.SkinButton btn_go;
        public CCWin.SkinControl.SkinButton btnGetCookie;
        private CCWin.SkinControl.SkinTextBox tbx_url;
        private CCWin.SkinControl.SkinPanel pnl_chromeBrowser;
        private ReaLTaiizor.Controls.FoxLabel foxLabel1;
        public CCWin.SkinControl.SkinButton skinButton1;
        public CCWin.SkinControl.SkinButton skinButton2;
    }
}