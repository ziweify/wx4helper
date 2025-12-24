namespace zhaocaimao.Views.AutoBet
{
    partial class BetBrowserForm
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
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            lblBalance = new ToolStripStatusLabel();
            lblOddsInfo = new ToolStripStatusLabel();
            lblLogToggle = new ToolStripStatusLabel();
            pnlTop = new Panel();
            lblUrl = new Label();
            txtUrl = new TextBox();
            btnNavigate = new Button();
            btnRefresh = new Button();
            btnTestCookie = new Button();
            btnTestBet = new Button();
            splitContainer = new SplitContainer();
            pnlBrowser = new Panel();
            pnlLog = new Panel();
            txtLog = new RichTextBox();
            pnlLogButtons = new Panel();
            lblLogStatus = new Label();
            chkLogSocket = new CheckBox();
            chkLogBet = new CheckBox();
            chkLogHttp = new CheckBox();
            chkLogSystem = new CheckBox();
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
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus, lblBalance, lblOddsInfo, lblLogToggle });
            statusStrip1.Location = new Point(0, 838);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1264, 22);
            statusStrip1.TabIndex = 2;
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
            // lblOddsInfo
            // 
            lblOddsInfo.IsLink = true;
            lblOddsInfo.Name = "lblOddsInfo";
            lblOddsInfo.Size = new Size(76, 17);
            lblOddsInfo.Text = "📊 查看赔率";
            lblOddsInfo.Click += LblOddsInfo_Click;
            // 
            // lblLogToggle
            // 
            lblLogToggle.IsLink = true;
            lblLogToggle.Name = "lblLogToggle";
            lblLogToggle.Size = new Size(56, 17);
            lblLogToggle.Text = "📋 当前日志";
            lblLogToggle.Click += LblLogToggle_Click;
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(lblUrl);
            pnlTop.Controls.Add(txtUrl);
            pnlTop.Controls.Add(btnNavigate);
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Controls.Add(btnTestCookie);
            pnlTop.Controls.Add(btnTestBet);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 35);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1264, 45);
            pnlTop.TabIndex = 1;
            // 
            // lblUrl
            // 
            lblUrl.AutoSize = true;
            lblUrl.Location = new Point(12, 15);
            lblUrl.Name = "lblUrl";
            lblUrl.Size = new Size(39, 16);
            lblUrl.TabIndex = 0;
            lblUrl.Text = "URL:";
            // 
            // txtUrl
            // 
            txtUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUrl.Location = new Point(60, 12);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(1019, 26);
            txtUrl.TabIndex = 1;
            // 
            // btnNavigate
            // 
            btnNavigate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNavigate.Location = new Point(1085, 13);
            btnNavigate.Name = "btnNavigate";
            btnNavigate.Size = new Size(49, 25);
            btnNavigate.TabIndex = 4;
            btnNavigate.Text = "转到";
            btnNavigate.Click += BtnNavigate_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(1216, 13);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(35, 25);
            btnRefresh.TabIndex = 5;
            btnRefresh.Text = "刷";
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // btnTestCookie
            // 
            btnTestCookie.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTestCookie.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            btnTestCookie.Location = new Point(1140, 12);
            btnTestCookie.Name = "btnTestCookie";
            btnTestCookie.Size = new Size(32, 25);
            btnTestCookie.TabIndex = 2;
            btnTestCookie.Text = "C";
            btnTestCookie.Click += BtnTestCookie_Click;
            // 
            // btnTestBet
            // 
            btnTestBet.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTestBet.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            btnTestBet.Location = new Point(1178, 12);
            btnTestBet.Name = "btnTestBet";
            btnTestBet.Size = new Size(32, 25);
            btnTestBet.TabIndex = 3;
            btnTestBet.Text = "投";
            btnTestBet.Click += BtnTestBet_Click;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 80);
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
            splitContainer.Size = new Size(1264, 758);
            splitContainer.SplitterDistance = 538;
            splitContainer.TabIndex = 0;
            // 
            // pnlBrowser
            // 
            pnlBrowser.Dock = DockStyle.Fill;
            pnlBrowser.Location = new Point(0, 0);
            pnlBrowser.Name = "pnlBrowser";
            pnlBrowser.Size = new Size(1264, 538);
            pnlBrowser.TabIndex = 0;
            // 
            // pnlLog
            // 
            pnlLog.Controls.Add(txtLog);
            pnlLog.Controls.Add(pnlLogButtons);
            pnlLog.Dock = DockStyle.Fill;
            pnlLog.Location = new Point(0, 0);
            pnlLog.Name = "pnlLog";
            pnlLog.Size = new Size(1264, 216);
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
            txtLog.ScrollBars = RichTextBoxScrollBars.Vertical; // 🔥 启用垂直滚动条
            txtLog.Size = new Size(1264, 186);
            txtLog.TabIndex = 0;
            txtLog.Text = "";
            txtLog.WordWrap = false;
            // 
            // pnlLogButtons
            // 
            pnlLogButtons.Controls.Add(lblLogStatus);
            pnlLogButtons.Controls.Add(chkLogSocket);
            pnlLogButtons.Controls.Add(chkLogBet);
            pnlLogButtons.Controls.Add(chkLogHttp);
            pnlLogButtons.Controls.Add(chkLogSystem);
            pnlLogButtons.Controls.Add(btnSaveLog);
            pnlLogButtons.Controls.Add(btnClearLog);
            pnlLogButtons.Dock = DockStyle.Bottom;
            pnlLogButtons.Location = new Point(0, 186);
            pnlLogButtons.Name = "pnlLogButtons";
            pnlLogButtons.Size = new Size(1264, 30);
            pnlLogButtons.TabIndex = 1;
            // 
            // lblLogStatus
            // 
            lblLogStatus.AutoSize = true;
            lblLogStatus.ForeColor = Color.Gray;
            lblLogStatus.Location = new Point(5, 7);
            lblLogStatus.Name = "lblLogStatus";
            lblLogStatus.Size = new Size(303, 16);
            lblLogStatus.TabIndex = 0;
            lblLogStatus.Text = "📊 日志: 0行 | 缓冲: 0 | 自动滚动: 开";
            // 
            // chkLogSocket
            // 
            chkLogSocket.AutoSize = true;
            chkLogSocket.Checked = true;
            chkLogSocket.CheckState = CheckState.Checked;
            chkLogSocket.Location = new Point(250, 6);
            chkLogSocket.Name = "chkLogSocket";
            chkLogSocket.Size = new Size(98, 20);
            chkLogSocket.TabIndex = 1;
            chkLogSocket.Text = "🔌 Socket";
            // 
            // chkLogBet
            // 
            chkLogBet.AutoSize = true;
            chkLogBet.Checked = true;
            chkLogBet.CheckState = CheckState.Checked;
            chkLogBet.Location = new Point(350, 6);
            chkLogBet.Name = "chkLogBet";
            chkLogBet.Size = new Size(82, 20);
            chkLogBet.TabIndex = 2;
            chkLogBet.Text = "🎲 投注";
            // 
            // chkLogHttp
            // 
            chkLogHttp.AutoSize = true;
            chkLogHttp.Location = new Point(435, 6);
            chkLogHttp.Name = "chkLogHttp";
            chkLogHttp.Size = new Size(82, 20);
            chkLogHttp.TabIndex = 3;
            chkLogHttp.Text = "🌐 HTTP";
            // 
            // chkLogSystem
            // 
            chkLogSystem.AutoSize = true;
            chkLogSystem.Checked = true;
            chkLogSystem.CheckState = CheckState.Checked;
            chkLogSystem.Location = new Point(535, 6);
            chkLogSystem.Name = "chkLogSystem";
            chkLogSystem.Size = new Size(82, 20);
            chkLogSystem.TabIndex = 4;
            chkLogSystem.Text = "⚙️ 系统";
            // 
            // btnSaveLog
            // 
            btnSaveLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveLog.Location = new Point(1106, 3);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new Size(75, 24);
            btnSaveLog.TabIndex = 5;
            btnSaveLog.Text = "保存日志";
            btnSaveLog.Click += BtnSaveLog_Click;
            // 
            // btnClearLog
            // 
            btnClearLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearLog.Location = new Point(1187, 3);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(75, 24);
            btnClearLog.TabIndex = 6;
            btnClearLog.Text = "清空日志";
            btnClearLog.Click += BtnClearLog_Click;
            // 
            // BetBrowserForm
            // 
            BackColor = Color.FromArgb(245, 248, 255);
            ClientSize = new Size(1264, 860);
            Controls.Add(splitContainer);
            Controls.Add(pnlTop);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.Sizable; // 🔥 允许调整窗口大小
            MaximizeBox = true; // 🔥 允许最大化
            MinimizeBox = true; // 🔥 允许最小化
            ControlBox = true; // 🔥 显示控制框
            Name = "BetBrowserForm";
            SizeGripStyle = SizeGripStyle.Show; // 🔥 显示右下角调整大小手柄
            Style = Sunny.UI.UIStyle.Custom;
            Text = "自动投注浏览器";
            ZoomScaleRect = new Rectangle(15, 15, 1264, 860);
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
    }
}

