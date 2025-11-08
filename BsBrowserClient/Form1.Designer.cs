namespace BsBrowserClient
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            lblBalance = new ToolStripStatusLabel();
            lblPort = new ToolStripStatusLabel();
            pnlTop = new Panel();
            btnTestBet = new Button();
            btnTestCookie = new Button();
            btnRefresh = new Button();
            btnNavigate = new Button();
            txtUrl = new TextBox();
            lblUrl = new Label();
            splitContainer = new SplitContainer();
            pnlBrowser = new Panel();
            pnlLog = new Panel();
            txtLog = new RichTextBox();
            pnlLogButtons = new Panel();
            chkLogSocket = new CheckBox();
            chkLogBet = new CheckBox();
            chkLogHttp = new CheckBox();
            chkLogSystem = new CheckBox();
            lblLogStatus = new Label();
            btnSaveLog = new Button();
            btnClearLog = new Button();
            statusStrip1.SuspendLayout();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            pnlLog.SuspendLayout();
            pnlLogButtons.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus, lblBalance, lblPort });
            statusStrip1.Location = new Point(0, 788);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1264, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(56, 17);
            lblStatus.Text = "● 未连接";
            // 
            // lblBalance
            // 
            lblBalance.Name = "lblBalance";
            lblBalance.Size = new Size(70, 17);
            lblBalance.Text = "余额: ¥0.00";
            // 
            // lblPort
            // 
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(46, 17);
            lblPort.Text = "端口: 0";
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(btnTestBet);
            pnlTop.Controls.Add(btnTestCookie);
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Controls.Add(btnNavigate);
            pnlTop.Controls.Add(txtUrl);
            pnlTop.Controls.Add(lblUrl);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1264, 45);
            pnlTop.TabIndex = 1;
            // 
            // btnTestBet
            // 
            btnTestBet.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTestBet.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            btnTestBet.Location = new Point(1085, 10);
            btnTestBet.Name = "btnTestBet";
            btnTestBet.Size = new Size(32, 25);
            btnTestBet.TabIndex = 5;
            btnTestBet.Text = "投";
            btnTestBet.UseVisualStyleBackColor = true;
            btnTestBet.Click += btnTestBet_Click;
            // 
            // btnTestCookie
            // 
            btnTestCookie.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTestCookie.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            btnTestCookie.Location = new Point(1047, 10);
            btnTestCookie.Name = "btnTestCookie";
            btnTestCookie.Size = new Size(32, 25);
            btnTestCookie.TabIndex = 4;
            btnTestCookie.Text = "C";
            btnTestCookie.UseVisualStyleBackColor = true;
            btnTestCookie.Click += btnTestCookie_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(1189, 10);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(60, 25);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "刷新";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnNavigate
            // 
            btnNavigate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNavigate.Location = new Point(1123, 10);
            btnNavigate.Name = "btnNavigate";
            btnNavigate.Size = new Size(60, 25);
            btnNavigate.TabIndex = 2;
            btnNavigate.Text = "Go";
            btnNavigate.UseVisualStyleBackColor = true;
            btnNavigate.Click += btnNavigate_Click;
            // 
            // txtUrl
            // 
            txtUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUrl.Location = new Point(60, 12);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(981, 23);
            txtUrl.TabIndex = 1;
            // 
            // lblUrl
            // 
            lblUrl.AutoSize = true;
            lblUrl.Location = new Point(12, 15);
            lblUrl.Name = "lblUrl";
            lblUrl.Size = new Size(34, 17);
            lblUrl.TabIndex = 0;
            lblUrl.Text = "URL:";
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 45);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(pnlBrowser);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(pnlLog);
            splitContainer.Size = new Size(1264, 743);
            splitContainer.SplitterDistance = 517;
            splitContainer.TabIndex = 3;
            // 
            // pnlBrowser
            // 
            pnlBrowser.Dock = DockStyle.Fill;
            pnlBrowser.Location = new Point(0, 0);
            pnlBrowser.Name = "pnlBrowser";
            pnlBrowser.Size = new Size(1264, 517);
            pnlBrowser.TabIndex = 0;
            // 
            // pnlLog
            // 
            pnlLog.Controls.Add(txtLog);
            pnlLog.Controls.Add(pnlLogButtons);
            pnlLog.Dock = DockStyle.Fill;
            pnlLog.Location = new Point(0, 0);
            pnlLog.Name = "pnlLog";
            pnlLog.Size = new Size(1264, 222);
            pnlLog.TabIndex = 0;
            // 
            // txtLog
            // 
            txtLog.BackColor = Color.Black;
            txtLog.Dock = DockStyle.Fill;
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.ForeColor = Color.Lime;
            txtLog.Location = new Point(0, 0);
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.Size = new Size(1264, 192);
            txtLog.TabIndex = 0;
            txtLog.Text = "";
            txtLog.WordWrap = false;
            // 
            // pnlLogButtons
            // 
            pnlLogButtons.Controls.Add(chkLogSocket);
            pnlLogButtons.Controls.Add(chkLogBet);
            pnlLogButtons.Controls.Add(chkLogHttp);
            pnlLogButtons.Controls.Add(chkLogSystem);
            pnlLogButtons.Controls.Add(lblLogStatus);
            pnlLogButtons.Controls.Add(btnSaveLog);
            pnlLogButtons.Controls.Add(btnClearLog);
            pnlLogButtons.Dock = DockStyle.Bottom;
            pnlLogButtons.Location = new Point(0, 192);
            pnlLogButtons.Name = "pnlLogButtons";
            pnlLogButtons.Size = new Size(1264, 30);
            pnlLogButtons.TabIndex = 1;
            // 
            // chkLogSocket
            // 
            chkLogSocket.AutoSize = true;
            chkLogSocket.Checked = true;
            chkLogSocket.CheckState = CheckState.Checked;
            chkLogSocket.Location = new Point(250, 6);
            chkLogSocket.Name = "chkLogSocket";
            chkLogSocket.Size = new Size(85, 21);
            chkLogSocket.TabIndex = 3;
            chkLogSocket.Text = "🔌 Socket";
            chkLogSocket.UseVisualStyleBackColor = true;
            // 
            // chkLogBet
            // 
            chkLogBet.AutoSize = true;
            chkLogBet.Checked = true;
            chkLogBet.CheckState = CheckState.Checked;
            chkLogBet.Location = new Point(350, 6);
            chkLogBet.Name = "chkLogBet";
            chkLogBet.Size = new Size(70, 21);
            chkLogBet.TabIndex = 4;
            chkLogBet.Text = "🎲 投注";
            chkLogBet.UseVisualStyleBackColor = true;
            // 
            // chkLogHttp
            // 
            chkLogHttp.AutoSize = true;
            chkLogHttp.Location = new Point(435, 6);
            chkLogHttp.Name = "chkLogHttp";
            chkLogHttp.Size = new Size(77, 21);
            chkLogHttp.TabIndex = 5;
            chkLogHttp.Text = "🌐 HTTP";
            chkLogHttp.UseVisualStyleBackColor = true;
            // 
            // chkLogSystem
            // 
            chkLogSystem.AutoSize = true;
            chkLogSystem.Checked = true;
            chkLogSystem.CheckState = CheckState.Checked;
            chkLogSystem.Location = new Point(535, 6);
            chkLogSystem.Name = "chkLogSystem";
            chkLogSystem.Size = new Size(71, 21);
            chkLogSystem.TabIndex = 6;
            chkLogSystem.Text = "⚙️ 系统";
            chkLogSystem.UseVisualStyleBackColor = true;
            // 
            // lblLogStatus
            // 
            lblLogStatus.AutoSize = true;
            lblLogStatus.ForeColor = Color.Gray;
            lblLogStatus.Location = new Point(5, 7);
            lblLogStatus.Name = "lblLogStatus";
            lblLogStatus.Size = new Size(205, 17);
            lblLogStatus.TabIndex = 2;
            lblLogStatus.Text = "📊 日志: 0行 | 缓冲: 0 | 自动滚动: 开";
            // 
            // btnSaveLog
            // 
            btnSaveLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveLog.Location = new Point(1106, 3);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new Size(75, 24);
            btnSaveLog.TabIndex = 1;
            btnSaveLog.Text = "保存日志";
            btnSaveLog.UseVisualStyleBackColor = true;
            btnSaveLog.Click += btnSaveLog_Click;
            // 
            // btnClearLog
            // 
            btnClearLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearLog.Location = new Point(1187, 3);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(75, 24);
            btnClearLog.TabIndex = 0;
            btnClearLog.Text = "清空日志";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1264, 860);
            Controls.Add(splitContainer);
            Controls.Add(pnlTop);
            Controls.Add(statusStrip1);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "百盛浏览器";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            pnlLog.ResumeLayout(false);
            pnlLogButtons.ResumeLayout(false);
            pnlLogButtons.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblBalance;
        private System.Windows.Forms.ToolStripStatusLabel lblPort;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Button btnTestBet;
        private System.Windows.Forms.Button btnTestCookie;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnNavigate;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Label lblUrl;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel pnlBrowser;
        private System.Windows.Forms.Panel pnlLog;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Panel pnlLogButtons;
        private System.Windows.Forms.CheckBox chkLogSocket;
        private System.Windows.Forms.CheckBox chkLogBet;
        private System.Windows.Forms.CheckBox chkLogHttp;
        private System.Windows.Forms.CheckBox chkLogSystem;
        private System.Windows.Forms.Button btnSaveLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Label lblLogStatus;
    }
}
