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
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            panelBrowser = new System.Windows.Forms.Panel();
            panelBrowserContent = new System.Windows.Forms.Panel();
            toolStripBrowser = new System.Windows.Forms.ToolStrip();
            btnBack = new System.Windows.Forms.ToolStripButton();
            btnForward = new System.Windows.Forms.ToolStripButton();
            btnRefresh = new System.Windows.Forms.ToolStripButton();
            btnHome = new System.Windows.Forms.ToolStripButton();
            txtUrl = new System.Windows.Forms.ToolStripTextBox();
            btnNavigate = new System.Windows.Forms.ToolStripButton();
            btnHistory = new System.Windows.Forms.ToolStripDropDownButton();
            toolStripMain = new System.Windows.Forms.ToolStrip();
            btnSaveConfig = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            btnClearLog = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            btnDockPosition = new System.Windows.Forms.ToolStripDropDownButton();
            menuDockRight = new System.Windows.Forms.ToolStripMenuItem();
            menuDockBottom = new System.Windows.Forms.ToolStripMenuItem();
            menuDockLeft = new System.Windows.Forms.ToolStripMenuItem();
            btnTogglePanel = new System.Windows.Forms.ToolStripButton();
            tabControlTools = new System.Windows.Forms.TabControl();
            tabPageConfig = new System.Windows.Forms.TabPage();
            tabPageLog = new System.Windows.Forms.TabPage();
            tabPageScript = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            panelBrowser.SuspendLayout();
            toolStripBrowser.SuspendLayout();
            toolStripMain.SuspendLayout();
            tabControlTools.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.Location = new System.Drawing.Point(0, 0);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(panelBrowser);
            splitContainerMain.Panel1.Controls.Add(toolStripMain);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(tabControlTools);
            splitContainerMain.Size = new System.Drawing.Size(1440, 980);
            splitContainerMain.SplitterDistance = 960;
            splitContainerMain.TabIndex = 0;
            // 
            // panelBrowser
            // 
            panelBrowser.Controls.Add(panelBrowserContent);
            panelBrowser.Controls.Add(toolStripBrowser);
            panelBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            panelBrowser.Location = new System.Drawing.Point(0, 25);
            panelBrowser.Name = "panelBrowser";
            panelBrowser.Size = new System.Drawing.Size(960, 955);
            panelBrowser.TabIndex = 1;
            // 
            // panelBrowserContent
            // 
            panelBrowserContent.Dock = System.Windows.Forms.DockStyle.Fill;
            panelBrowserContent.Location = new System.Drawing.Point(0, 25);
            panelBrowserContent.Name = "panelBrowserContent";
            panelBrowserContent.Size = new System.Drawing.Size(960, 930);
            panelBrowserContent.TabIndex = 1;
            // 
            // toolStripBrowser
            // 
            toolStripBrowser.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnBack, btnForward, btnRefresh, btnHome, txtUrl, btnNavigate, btnHistory });
            toolStripBrowser.Location = new System.Drawing.Point(0, 0);
            toolStripBrowser.Name = "toolStripBrowser";
            toolStripBrowser.Size = new System.Drawing.Size(960, 25);
            toolStripBrowser.TabIndex = 0;
            // 
            // btnBack
            // 
            btnBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnBack.Name = "btnBack";
            btnBack.Size = new System.Drawing.Size(23, 22);
            btnBack.Text = "◀";
            btnBack.ToolTipText = "后退";
            btnBack.Click += OnGoBack;
            // 
            // btnForward
            // 
            btnForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnForward.Name = "btnForward";
            btnForward.Size = new System.Drawing.Size(23, 22);
            btnForward.Text = "▶";
            btnForward.ToolTipText = "前进";
            btnForward.Click += OnGoForward;
            // 
            // btnRefresh
            // 
            btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(23, 22);
            btnRefresh.Text = "🔄";
            btnRefresh.ToolTipText = "刷新";
            btnRefresh.Click += OnRefresh;
            // 
            // btnHome
            // 
            btnHome.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnHome.Name = "btnHome";
            btnHome.Size = new System.Drawing.Size(23, 22);
            btnHome.Text = "🏠";
            btnHome.ToolTipText = "主页";
            btnHome.Click += OnGoHome;
            // 
            // txtUrl
            // 
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new System.Drawing.Size(400, 25);
            txtUrl.KeyDown += OnUrlKeyDown;
            // 
            // btnNavigate
            // 
            btnNavigate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnNavigate.Name = "btnNavigate";
            btnNavigate.Size = new System.Drawing.Size(36, 22);
            btnNavigate.Text = "转到";
            btnNavigate.Click += OnNavigate;
            // 
            // btnHistory
            // 
            btnHistory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnHistory.Name = "btnHistory";
            btnHistory.Size = new System.Drawing.Size(48, 22);
            btnHistory.Text = "📜";
            btnHistory.ToolTipText = "历史记录";
            // 
            // toolStripMain
            // 
            toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnSaveConfig, toolStripSeparator1, btnClearLog, toolStripSeparator2, btnDockPosition, btnTogglePanel });
            toolStripMain.Location = new System.Drawing.Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Size = new System.Drawing.Size(960, 25);
            toolStripMain.TabIndex = 0;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new System.Drawing.Size(60, 22);
            btnSaveConfig.Text = "💾 保存";
            btnSaveConfig.Click += OnSaveConfig;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnClearLog
            // 
            btnClearLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new System.Drawing.Size(72, 22);
            btnClearLog.Text = "🗑️ 清空日志";
            btnClearLog.Click += OnClearLog;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnDockPosition
            // 
            btnDockPosition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnDockPosition.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuDockRight, menuDockBottom, menuDockLeft });
            btnDockPosition.Name = "btnDockPosition";
            btnDockPosition.Size = new System.Drawing.Size(90, 22);
            btnDockPosition.Text = "📐 面板位置 ▼";
            // 
            // menuDockRight
            // 
            menuDockRight.Name = "menuDockRight";
            menuDockRight.Size = new System.Drawing.Size(180, 22);
            menuDockRight.Text = "➡️ 停靠在右侧";
            menuDockRight.Click += OnDockRight;
            // 
            // menuDockBottom
            // 
            menuDockBottom.Name = "menuDockBottom";
            menuDockBottom.Size = new System.Drawing.Size(180, 22);
            menuDockBottom.Text = "⬇️ 停靠在底部";
            menuDockBottom.Click += OnDockBottom;
            // 
            // menuDockLeft
            // 
            menuDockLeft.Name = "menuDockLeft";
            menuDockLeft.Size = new System.Drawing.Size(180, 22);
            menuDockLeft.Text = "⬅️ 停靠在左侧";
            menuDockLeft.Click += OnDockLeft;
            // 
            // btnTogglePanel
            // 
            btnTogglePanel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnTogglePanel.Name = "btnTogglePanel";
            btnTogglePanel.Size = new System.Drawing.Size(60, 22);
            btnTogglePanel.Text = "👁️ 隐藏";
            btnTogglePanel.Click += OnTogglePanel;
            // 
            // tabControlTools
            // 
            tabControlTools.Controls.Add(tabPageConfig);
            tabControlTools.Controls.Add(tabPageLog);
            tabControlTools.Controls.Add(tabPageScript);
            tabControlTools.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControlTools.Location = new System.Drawing.Point(0, 0);
            tabControlTools.Name = "tabControlTools";
            tabControlTools.SelectedIndex = 0;
            tabControlTools.Size = new System.Drawing.Size(476, 980);
            tabControlTools.TabIndex = 0;
            // 
            // tabPageConfig
            // 
            tabPageConfig.Location = new System.Drawing.Point(4, 24);
            tabPageConfig.Name = "tabPageConfig";
            tabPageConfig.Padding = new System.Windows.Forms.Padding(3);
            tabPageConfig.Size = new System.Drawing.Size(468, 952);
            tabPageConfig.TabIndex = 0;
            tabPageConfig.Text = "⚙️ 配置";
            tabPageConfig.UseVisualStyleBackColor = true;
            // 
            // tabPageLog
            // 
            tabPageLog.Location = new System.Drawing.Point(4, 24);
            tabPageLog.Name = "tabPageLog";
            tabPageLog.Padding = new System.Windows.Forms.Padding(3);
            tabPageLog.Size = new System.Drawing.Size(468, 952);
            tabPageLog.TabIndex = 1;
            tabPageLog.Text = "📋 日志";
            tabPageLog.UseVisualStyleBackColor = true;
            // 
            // tabPageScript
            // 
            tabPageScript.Location = new System.Drawing.Point(4, 24);
            tabPageScript.Name = "tabPageScript";
            tabPageScript.Size = new System.Drawing.Size(468, 952);
            tabPageScript.TabIndex = 2;
            tabPageScript.Text = "📝 脚本";
            tabPageScript.UseVisualStyleBackColor = true;
            // 
            // BrowserTaskControl
            // 
            ClientSize = new System.Drawing.Size(1440, 980);
            Controls.Add(splitContainerMain);
            Name = "BrowserTaskControl";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
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
            tabControlTools.ResumeLayout(false);
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
        private System.Windows.Forms.TabControl tabControlTools;
        private System.Windows.Forms.TabPage tabPageConfig;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.TabPage tabPageScript;
    }
}
