namespace Unit.La.Controls
{
    partial class BrowserTaskControl
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
            splitContainerMain = new SplitContainer();
            panelBrowser = new Panel();
            panelBrowserContent = new Panel();
            toolStripBrowser = new ToolStrip();
            btnBack = new ToolStripButton();
            btnForward = new ToolStripButton();
            btnRefresh = new ToolStripButton();
            btnHome = new ToolStripButton();
            txtUrl = new ToolStripTextBox();
            btnNavigate = new ToolStripButton();
            btnHistory = new ToolStripDropDownButton();
            toolStripMain = new ToolStrip();
            btnSaveConfig = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnClearLog = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnDockPosition = new ToolStripDropDownButton();
            menuDockRight = new ToolStripMenuItem();
            menuDockBottom = new ToolStripMenuItem();
            menuDockLeft = new ToolStripMenuItem();
            btnTogglePanel = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            btnToggleScriptWindow = new ToolStripButton();
            tabControlConfigLog = new TabControl();
            tabPageConfig = new TabPage();
            tabPageLog = new TabPage();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            panelBrowser.SuspendLayout();
            toolStripBrowser.SuspendLayout();
            toolStripMain.SuspendLayout();
            tabControlConfigLog.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(0, 0);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(panelBrowser);
            splitContainerMain.Panel1.Controls.Add(toolStripMain);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(tabControlConfigLog);
            splitContainerMain.Size = new Size(1484, 961);
            splitContainerMain.SplitterDistance = 1024;
            splitContainerMain.TabIndex = 0;
            // 
            // panelBrowser
            // 
            panelBrowser.Controls.Add(panelBrowserContent);
            panelBrowser.Controls.Add(toolStripBrowser);
            panelBrowser.Dock = DockStyle.Fill;
            panelBrowser.Location = new Point(0, 25);
            panelBrowser.Name = "panelBrowser";
            panelBrowser.Size = new Size(1024, 936);
            panelBrowser.TabIndex = 1;
            // 
            // panelBrowserContent
            // 
            panelBrowserContent.Dock = DockStyle.Fill;
            panelBrowserContent.Location = new Point(0, 25);
            panelBrowserContent.Name = "panelBrowserContent";
            panelBrowserContent.Size = new Size(1024, 911);
            panelBrowserContent.TabIndex = 1;
            // 
            // toolStripBrowser
            // 
            toolStripBrowser.Items.AddRange(new ToolStripItem[] { btnBack, btnForward, btnRefresh, btnHome, txtUrl, btnNavigate, btnHistory });
            toolStripBrowser.Location = new Point(0, 0);
            toolStripBrowser.Name = "toolStripBrowser";
            toolStripBrowser.Size = new Size(1024, 25);
            toolStripBrowser.TabIndex = 0;
            // 
            // btnBack
            // 
            btnBack.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(23, 22);
            btnBack.Text = "◀";
            btnBack.ToolTipText = "后退";
            btnBack.Click += OnGoBack;
            // 
            // btnForward
            // 
            btnForward.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnForward.Name = "btnForward";
            btnForward.Size = new Size(23, 22);
            btnForward.Text = "▶";
            btnForward.ToolTipText = "前进";
            btnForward.Click += OnGoForward;
            // 
            // btnRefresh
            // 
            btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(28, 22);
            btnRefresh.Text = "🔄";
            btnRefresh.ToolTipText = "刷新";
            btnRefresh.Click += OnRefresh;
            // 
            // btnHome
            // 
            btnHome.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(28, 22);
            btnHome.Text = "🏠";
            btnHome.ToolTipText = "主页";
            btnHome.Click += OnGoHome;
            // 
            // txtUrl
            // 
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(400, 25);
            txtUrl.KeyDown += OnUrlKeyDown;
            // 
            // btnNavigate
            // 
            btnNavigate.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnNavigate.Name = "btnNavigate";
            btnNavigate.Size = new Size(36, 22);
            btnNavigate.Text = "转到";
            btnNavigate.Click += OnNavigate;
            // 
            // btnHistory
            // 
            btnHistory.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnHistory.Name = "btnHistory";
            btnHistory.Size = new Size(37, 22);
            btnHistory.Text = "📜";
            btnHistory.ToolTipText = "历史记录";
            // 
            // toolStripMain
            // 
            toolStripMain.Items.AddRange(new ToolStripItem[] { btnSaveConfig, toolStripSeparator2, btnTogglePanel, toolStripSeparator3, btnToggleScriptWindow });
            toolStripMain.Location = new Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Size = new Size(1024, 25);
            toolStripMain.TabIndex = 0;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(56, 22);
            btnSaveConfig.Text = "💾 保存";
            btnSaveConfig.Click += OnSaveConfig;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // btnClearLog
            // 
            btnClearLog.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(80, 22);
            btnClearLog.Text = "🗑️ 清空日志";
            btnClearLog.Click += OnClearLog;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // btnDockPosition
            // 
            btnDockPosition.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDockPosition.DropDownItems.AddRange(new ToolStripItem[] { menuDockRight, menuDockBottom, menuDockLeft });
            btnDockPosition.Name = "btnDockPosition";
            btnDockPosition.Size = new Size(105, 22);
            btnDockPosition.Text = "📐 面板位置 ▼";
            // 
            // menuDockRight
            // 
            menuDockRight.Name = "menuDockRight";
            menuDockRight.Size = new Size(156, 22);
            menuDockRight.Text = "➡️ 停靠在右侧";
            menuDockRight.Click += OnDockRight;
            // 
            // menuDockBottom
            // 
            menuDockBottom.Name = "menuDockBottom";
            menuDockBottom.Size = new Size(156, 22);
            menuDockBottom.Text = "⬇️ 停靠在底部";
            menuDockBottom.Click += OnDockBottom;
            // 
            // menuDockLeft
            // 
            menuDockLeft.Name = "menuDockLeft";
            menuDockLeft.Size = new Size(156, 22);
            menuDockLeft.Text = "⬅️ 停靠在左侧";
            menuDockLeft.Click += OnDockLeft;
            // 
            // btnTogglePanel
            // 
            btnTogglePanel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnTogglePanel.Name = "btnTogglePanel";
            btnTogglePanel.Size = new Size(56, 22);
            btnTogglePanel.Text = "👁️ 隐藏";
            btnTogglePanel.Click += OnTogglePanel;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // btnToggleScriptWindow
            // 
            btnToggleScriptWindow.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnToggleScriptWindow.Name = "btnToggleScriptWindow";
            btnToggleScriptWindow.Size = new Size(80, 22);
            btnToggleScriptWindow.Text = "📝 显示脚本";
            btnToggleScriptWindow.ToolTipText = "显示/隐藏脚本编辑器窗口";
            btnToggleScriptWindow.Click += OnToggleScriptWindow;
            // 
            // tabControlConfigLog
            // 
            tabControlConfigLog.Controls.Add(tabPageConfig);
            tabControlConfigLog.Controls.Add(tabPageLog);
            tabControlConfigLog.Dock = DockStyle.Fill;
            tabControlConfigLog.Location = new Point(0, 0);
            tabControlConfigLog.Name = "tabControlConfigLog";
            tabControlConfigLog.SelectedIndex = 0;
            tabControlConfigLog.Size = new Size(456, 961);
            tabControlConfigLog.TabIndex = 0;
            // 
            // tabPageConfig
            // 
            tabPageConfig.Location = new Point(4, 26);
            tabPageConfig.Name = "tabPageConfig";
            tabPageConfig.Padding = new Padding(3);
            tabPageConfig.Size = new Size(448, 931);
            tabPageConfig.TabIndex = 0;
            tabPageConfig.Text = "⚙️ 配置";
            tabPageConfig.UseVisualStyleBackColor = true;
            // 
            // tabPageLog
            // 
            tabPageLog.Location = new Point(4, 26);
            tabPageLog.Name = "tabPageLog";
            tabPageLog.Padding = new Padding(3);
            tabPageLog.Size = new Size(779, 931);
            tabPageLog.TabIndex = 1;
            tabPageLog.Text = "📋 日志";
            tabPageLog.UseVisualStyleBackColor = true;
            // 
            // BrowserTaskControl
            // 
            ClientSize = new Size(1484, 961);
            Controls.Add(splitContainerMain);
            Name = "BrowserTaskControl";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "浏览器任务";
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel1.PerformLayout();
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            panelBrowser.ResumeLayout(false);
            panelBrowser.PerformLayout();
            toolStripBrowser.ResumeLayout(false);
            toolStripBrowser.PerformLayout();
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            tabControlConfigLog.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.Panel panelBrowser;
        private System.Windows.Forms.Panel panelBrowserContent;
        private System.Windows.Forms.ToolStrip toolStripBrowser;
        private System.Windows.Forms.ToolStripButton btnBack;
        private System.Windows.Forms.ToolStripButton btnForward;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripButton btnHome;
        private System.Windows.Forms.ToolStripTextBox txtUrl;
        private System.Windows.Forms.ToolStripButton btnNavigate;
        private System.Windows.Forms.ToolStripDropDownButton btnHistory;
        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripButton btnSaveConfig;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnClearLog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton btnDockPosition;
        private System.Windows.Forms.ToolStripMenuItem menuDockRight;
        private System.Windows.Forms.ToolStripMenuItem menuDockBottom;
        private System.Windows.Forms.ToolStripMenuItem menuDockLeft;
        private System.Windows.Forms.ToolStripButton btnTogglePanel;
        private System.Windows.Forms.TabControl tabControlConfigLog;
        private System.Windows.Forms.TabPage tabPageConfig;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnToggleScriptWindow;
    }
}
