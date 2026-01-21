namespace YongLiSystem.Views.Dashboard
{
    partial class BrowserTaskWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            panelAddressBar = new System.Windows.Forms.Panel();
            buttonBack = new System.Windows.Forms.Button();
            buttonForward = new System.Windows.Forms.Button();
            buttonRefreshBrowser = new System.Windows.Forms.Button();
            buttonHome = new System.Windows.Forms.Button();
            textBoxUrl = new System.Windows.Forms.TextBox();
            buttonGo = new System.Windows.Forms.Button();
            buttonHistory = new System.Windows.Forms.Button();
            toolStrip = new System.Windows.Forms.ToolStrip();
            toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonClearLog = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripDropDownDockPosition = new System.Windows.Forms.ToolStripDropDownButton();
            toolStripMenuItemDockRight = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItemDockBottom = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItemDockLeft = new System.Windows.Forms.ToolStripMenuItem();
            toolStripButtonTogglePanel = new System.Windows.Forms.ToolStripButton();
            xtraTabControl = new DevExpress.XtraTab.XtraTabControl();
            tabPageConfig = new DevExpress.XtraTab.XtraTabPage();
            tabPageLog = new DevExpress.XtraTab.XtraTabPage();
            tabPageScript = new DevExpress.XtraTab.XtraTabPage();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            panelBrowser.SuspendLayout();
            panelAddressBar.SuspendLayout();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)xtraTabControl).BeginInit();
            xtraTabControl.SuspendLayout();
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
            splitContainerMain.Panel1.Controls.Add(toolStrip);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(xtraTabControl);
            splitContainerMain.Size = new System.Drawing.Size(1200, 700);
            splitContainerMain.SplitterDistance = 800;
            splitContainerMain.TabIndex = 0;
            // 
            // panelBrowser
            // 
            panelBrowser.Controls.Add(panelBrowserContent);
            panelBrowser.Controls.Add(panelAddressBar);
            panelBrowser.Controls.Add(toolStrip);
            panelBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            panelBrowser.Location = new System.Drawing.Point(0, 0);
            panelBrowser.Name = "panelBrowser";
            panelBrowser.Size = new System.Drawing.Size(800, 675);
            panelBrowser.TabIndex = 1;
            // 
            // panelBrowserContent
            // 
            panelBrowserContent.Dock = System.Windows.Forms.DockStyle.Fill;
            panelBrowserContent.Location = new System.Drawing.Point(0, 65);
            panelBrowserContent.Name = "panelBrowserContent";
            panelBrowserContent.Size = new System.Drawing.Size(800, 610);
            panelBrowserContent.TabIndex = 2;
            // 
            // panelAddressBar
            // 
            panelAddressBar.Controls.Add(buttonHistory);
            panelAddressBar.Controls.Add(buttonGo);
            panelAddressBar.Controls.Add(textBoxUrl);
            panelAddressBar.Controls.Add(buttonHome);
            panelAddressBar.Controls.Add(buttonRefreshBrowser);
            panelAddressBar.Controls.Add(buttonForward);
            panelAddressBar.Controls.Add(buttonBack);
            panelAddressBar.Dock = System.Windows.Forms.DockStyle.Top;
            panelAddressBar.Location = new System.Drawing.Point(0, 25);
            panelAddressBar.Name = "panelAddressBar";
            panelAddressBar.Size = new System.Drawing.Size(800, 40);
            panelAddressBar.TabIndex = 1;
            // 
            // buttonBack
            // 
            buttonBack.Font = new System.Drawing.Font("Segoe UI", 12F);
            buttonBack.Location = new System.Drawing.Point(5, 5);
            buttonBack.Name = "buttonBack";
            buttonBack.Size = new System.Drawing.Size(40, 30);
            buttonBack.TabIndex = 0;
            buttonBack.Text = "◀";
            buttonBack.UseVisualStyleBackColor = true;
            buttonBack.Click += OnNavigateBack;
            // 
            // buttonForward
            // 
            buttonForward.Font = new System.Drawing.Font("Segoe UI", 12F);
            buttonForward.Location = new System.Drawing.Point(50, 5);
            buttonForward.Name = "buttonForward";
            buttonForward.Size = new System.Drawing.Size(40, 30);
            buttonForward.TabIndex = 1;
            buttonForward.Text = "▶";
            buttonForward.UseVisualStyleBackColor = true;
            buttonForward.Click += OnNavigateForward;
            // 
            // buttonRefreshBrowser
            // 
            buttonRefreshBrowser.Font = new System.Drawing.Font("Segoe UI", 10F);
            buttonRefreshBrowser.Location = new System.Drawing.Point(95, 5);
            buttonRefreshBrowser.Name = "buttonRefreshBrowser";
            buttonRefreshBrowser.Size = new System.Drawing.Size(40, 30);
            buttonRefreshBrowser.TabIndex = 2;
            buttonRefreshBrowser.Text = "🔄";
            buttonRefreshBrowser.UseVisualStyleBackColor = true;
            buttonRefreshBrowser.Click += OnRefreshBrowserClick;
            // 
            // buttonHome
            // 
            buttonHome.Font = new System.Drawing.Font("Segoe UI", 10F);
            buttonHome.Location = new System.Drawing.Point(140, 5);
            buttonHome.Name = "buttonHome";
            buttonHome.Size = new System.Drawing.Size(40, 30);
            buttonHome.TabIndex = 3;
            buttonHome.Text = "🏠";
            buttonHome.UseVisualStyleBackColor = true;
            buttonHome.Click += OnNavigateHome;
            // 
            // textBoxUrl
            // 
            textBoxUrl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxUrl.Font = new System.Drawing.Font("Consolas", 10F);
            textBoxUrl.Location = new System.Drawing.Point(185, 8);
            textBoxUrl.Name = "textBoxUrl";
            textBoxUrl.Size = new System.Drawing.Size(515, 23);
            textBoxUrl.TabIndex = 4;
            textBoxUrl.KeyDown += OnUrlKeyDown;
            // 
            // buttonGo
            // 
            buttonGo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonGo.Location = new System.Drawing.Point(705, 5);
            buttonGo.Name = "buttonGo";
            buttonGo.Size = new System.Drawing.Size(45, 30);
            buttonGo.TabIndex = 5;
            buttonGo.Text = "转到";
            buttonGo.UseVisualStyleBackColor = true;
            buttonGo.Click += OnNavigateGo;
            // 
            // buttonHistory
            // 
            buttonHistory.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonHistory.Location = new System.Drawing.Point(755, 5);
            buttonHistory.Name = "buttonHistory";
            buttonHistory.Size = new System.Drawing.Size(40, 30);
            buttonHistory.TabIndex = 6;
            buttonHistory.Text = "📜";
            buttonHistory.UseVisualStyleBackColor = true;
            buttonHistory.Click += OnShowHistory;
            // 
            // toolStrip
            // 
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonRefresh, toolStripSeparator1, toolStripButtonSave, toolStripSeparator2, toolStripButtonClearLog, toolStripSeparator3, toolStripDropDownDockPosition, toolStripButtonTogglePanel });
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new System.Drawing.Size(800, 25);
            toolStrip.TabIndex = 0;
            toolStrip.Text = "toolStrip1";
            // 
            // toolStripButtonRefresh
            // 
            toolStripButtonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRefresh.Name = "toolStripButtonRefresh";
            toolStripButtonRefresh.Size = new System.Drawing.Size(60, 22);
            toolStripButtonRefresh.Text = "🔄 刷新";
            toolStripButtonRefresh.Click += OnRefreshBrowser;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonSave
            // 
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Size = new System.Drawing.Size(60, 22);
            toolStripButtonSave.Text = "💾 保存";
            toolStripButtonSave.Click += OnSaveConfig;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonClearLog
            // 
            toolStripButtonClearLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonClearLog.Name = "toolStripButtonClearLog";
            toolStripButtonClearLog.Size = new System.Drawing.Size(72, 22);
            toolStripButtonClearLog.Text = "🗑️ 清空日志";
            toolStripButtonClearLog.Click += OnClearLog;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripDropDownDockPosition
            // 
            toolStripDropDownDockPosition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownDockPosition.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { 
                toolStripMenuItemDockRight, 
                toolStripMenuItemDockBottom, 
                toolStripMenuItemDockLeft 
            });
            toolStripDropDownDockPosition.Name = "toolStripDropDownDockPosition";
            toolStripDropDownDockPosition.Size = new System.Drawing.Size(90, 22);
            toolStripDropDownDockPosition.Text = "📐 面板位置 ▼";
            // 
            // toolStripMenuItemDockRight
            // 
            toolStripMenuItemDockRight.Name = "toolStripMenuItemDockRight";
            toolStripMenuItemDockRight.Size = new System.Drawing.Size(180, 22);
            toolStripMenuItemDockRight.Text = "➡️ 停靠在右侧";
            toolStripMenuItemDockRight.Click += OnDockRight;
            // 
            // toolStripMenuItemDockBottom
            // 
            toolStripMenuItemDockBottom.Name = "toolStripMenuItemDockBottom";
            toolStripMenuItemDockBottom.Size = new System.Drawing.Size(180, 22);
            toolStripMenuItemDockBottom.Text = "⬇️ 停靠在底部";
            toolStripMenuItemDockBottom.Click += OnDockBottom;
            // 
            // toolStripMenuItemDockLeft
            // 
            toolStripMenuItemDockLeft.Name = "toolStripMenuItemDockLeft";
            toolStripMenuItemDockLeft.Size = new System.Drawing.Size(180, 22);
            toolStripMenuItemDockLeft.Text = "⬅️ 停靠在左侧";
            toolStripMenuItemDockLeft.Click += OnDockLeft;
            // 
            // toolStripButtonTogglePanel
            // 
            toolStripButtonTogglePanel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonTogglePanel.Name = "toolStripButtonTogglePanel";
            toolStripButtonTogglePanel.Size = new System.Drawing.Size(60, 22);
            toolStripButtonTogglePanel.Text = "👁️ 隐藏";
            toolStripButtonTogglePanel.Click += OnTogglePanel;
            // 
            // xtraTabControl
            // 
            xtraTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            xtraTabControl.Location = new System.Drawing.Point(0, 0);
            xtraTabControl.Name = "xtraTabControl";
            xtraTabControl.SelectedTabPage = tabPageConfig;
            xtraTabControl.Size = new System.Drawing.Size(396, 700);
            xtraTabControl.TabIndex = 0;
            xtraTabControl.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { tabPageConfig, tabPageLog, tabPageScript });
            // 
            // tabPageConfig
            // 
            tabPageConfig.Name = "tabPageConfig";
            tabPageConfig.Size = new System.Drawing.Size(394, 675);
            tabPageConfig.Text = "⚙️ 配置";
            // 
            // tabPageLog
            // 
            tabPageLog.Name = "tabPageLog";
            tabPageLog.Size = new System.Drawing.Size(394, 675);
            tabPageLog.Text = "📋 日志";
            // 
            // tabPageScript
            // 
            tabPageScript.Name = "tabPageScript";
            tabPageScript.Size = new System.Drawing.Size(394, 675);
            tabPageScript.Text = "📝 脚本";
            // 
            // BrowserTaskWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1440, 980);
            Controls.Add(splitContainerMain);
            Name = "BrowserTaskWindow";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "浏览器任务窗口";
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel1.PerformLayout();
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            panelBrowser.ResumeLayout(false);
            panelBrowser.PerformLayout();
            panelAddressBar.ResumeLayout(false);
            panelAddressBar.PerformLayout();
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)xtraTabControl).EndInit();
            xtraTabControl.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.Panel panelBrowser;
        private System.Windows.Forms.Panel panelBrowserContent;
        private System.Windows.Forms.Panel panelAddressBar;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.Button buttonForward;
        private System.Windows.Forms.Button buttonRefreshBrowser;
        private System.Windows.Forms.Button buttonHome;
        private System.Windows.Forms.TextBox textBoxUrl;
        private System.Windows.Forms.Button buttonGo;
        private System.Windows.Forms.Button buttonHistory;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonClearLog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownDockPosition;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDockRight;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDockBottom;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDockLeft;
        private System.Windows.Forms.ToolStripButton toolStripButtonTogglePanel;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl;
        private DevExpress.XtraTab.XtraTabPage tabPageConfig;
        private DevExpress.XtraTab.XtraTabPage tabPageLog;
        private DevExpress.XtraTab.XtraTabPage tabPageScript;
    }
}
