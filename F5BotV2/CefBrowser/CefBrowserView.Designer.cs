namespace F5BotV2.CefBrowser
{
    partial class CefBrowserView
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
            this.pnl_top = new CCWin.SkinControl.SkinPanel();
            this.btnGetCookie = new CCWin.SkinControl.SkinButton();
            this.tbx_url = new CCWin.SkinControl.SkinTextBox();
            this.pnl_chromeBrowser = new CCWin.SkinControl.SkinPanel();
            this.btn_go = new CCWin.SkinControl.SkinButton();
            this.pnl_top.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnl_top
            // 
            this.pnl_top.BackColor = System.Drawing.Color.DarkGray;
            this.pnl_top.Controls.Add(this.btn_go);
            this.pnl_top.Controls.Add(this.btnGetCookie);
            this.pnl_top.Controls.Add(this.tbx_url);
            this.pnl_top.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.pnl_top.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl_top.DownBack = null;
            this.pnl_top.Location = new System.Drawing.Point(0, 0);
            this.pnl_top.MouseBack = null;
            this.pnl_top.Name = "pnl_top";
            this.pnl_top.NormlBack = null;
            this.pnl_top.Size = new System.Drawing.Size(1154, 33);
            this.pnl_top.TabIndex = 0;
            // 
            // btnGetCookie
            // 
            this.btnGetCookie.BackColor = System.Drawing.Color.Transparent;
            this.btnGetCookie.BaseColor = System.Drawing.Color.LightGray;
            this.btnGetCookie.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnGetCookie.DownBack = null;
            this.btnGetCookie.Location = new System.Drawing.Point(1123, 3);
            this.btnGetCookie.MouseBack = null;
            this.btnGetCookie.Name = "btnGetCookie";
            this.btnGetCookie.NormlBack = null;
            this.btnGetCookie.Size = new System.Drawing.Size(28, 23);
            this.btnGetCookie.TabIndex = 5;
            this.btnGetCookie.Text = "CK";
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
            this.tbx_url.Location = new System.Drawing.Point(75, 2);
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
            this.tbx_url.Size = new System.Drawing.Size(949, 28);
            // 
            // 
            // 
            this.tbx_url.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbx_url.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbx_url.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbx_url.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbx_url.SkinTxt.Name = "BaseText";
            this.tbx_url.SkinTxt.Size = new System.Drawing.Size(939, 18);
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
            this.pnl_chromeBrowser.BackColor = System.Drawing.Color.Transparent;
            this.pnl_chromeBrowser.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.pnl_chromeBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl_chromeBrowser.DownBack = null;
            this.pnl_chromeBrowser.Location = new System.Drawing.Point(0, 33);
            this.pnl_chromeBrowser.MouseBack = null;
            this.pnl_chromeBrowser.Name = "pnl_chromeBrowser";
            this.pnl_chromeBrowser.NormlBack = null;
            this.pnl_chromeBrowser.Size = new System.Drawing.Size(1154, 714);
            this.pnl_chromeBrowser.TabIndex = 1;
            // 
            // btn_go
            // 
            this.btn_go.BackColor = System.Drawing.Color.Transparent;
            this.btn_go.BaseColor = System.Drawing.Color.LightGray;
            this.btn_go.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_go.DownBack = null;
            this.btn_go.Location = new System.Drawing.Point(1027, 3);
            this.btn_go.MouseBack = null;
            this.btn_go.Name = "btn_go";
            this.btn_go.NormlBack = null;
            this.btn_go.Size = new System.Drawing.Size(28, 23);
            this.btn_go.TabIndex = 5;
            this.btn_go.Text = "GO";
            this.btn_go.UseVisualStyleBackColor = false;
            this.btn_go.Click += new System.EventHandler(this.btn_go_Click);
            // 
            // CefBrowserView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1154, 747);
            this.Controls.Add(this.pnl_chromeBrowser);
            this.Controls.Add(this.pnl_top);
            this.Name = "CefBrowserView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CefBrowserView";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CefBrowserView_FormClosing);
            this.pnl_top.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinPanel pnl_top;
        private CCWin.SkinControl.SkinPanel pnl_chromeBrowser;
        private CCWin.SkinControl.SkinTextBox tbx_url;
        public CCWin.SkinControl.SkinButton btnGetCookie;
        public CCWin.SkinControl.SkinButton btn_go;
    }
}