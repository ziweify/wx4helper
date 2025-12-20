namespace 永利系统.Views
{
    partial class Main
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
            ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            applicationMenu1 = new DevExpress.XtraBars.Ribbon.ApplicationMenu();
            barButtonItemDashboard = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemDataManagement = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemReports = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemSettings = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemRefresh = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemSave = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemExit = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemLog = new DevExpress.XtraBars.BarButtonItem();
            barStaticItemStatus = new DevExpress.XtraBars.BarStaticItem();
            barStaticItemUser = new DevExpress.XtraBars.BarStaticItem();
            barStaticItemLog = new DevExpress.XtraBars.BarStaticItem();
            menuItemNew = new DevExpress.XtraBars.BarButtonItem();
            menuItemOpen = new DevExpress.XtraBars.BarButtonItem();
            menuItemSave = new DevExpress.XtraBars.BarButtonItem();
            menuItemSaveAs = new DevExpress.XtraBars.BarButtonItem();
            menuItemPrint = new DevExpress.XtraBars.BarButtonItem();
            menuItemOptions = new DevExpress.XtraBars.BarButtonItem();
            menuItemShowQATBelow = new DevExpress.XtraBars.BarCheckItem();
            menuItemExit = new DevExpress.XtraBars.BarButtonItem();
            ribbonPageMain = new DevExpress.XtraBars.Ribbon.RibbonPage();
            ribbonPageGroupNavigation = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            ribbonPageGroupActions = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            ribbonStatusBar1 = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            contentPanel = new System.Windows.Forms.Panel();
            logWindow1 = new LogWindow();
            ((System.ComponentModel.ISupportInitialize)ribbonControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)applicationMenu1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1.Panel1).BeginInit();
            splitContainerControl1.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1.Panel2).BeginInit();
            splitContainerControl1.Panel2.SuspendLayout();
            splitContainerControl1.SuspendLayout();
            SuspendLayout();
            // 
            // ribbonControl1
            // 
            ribbonControl1.ExpandCollapseItem.Id = 0;
            ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] { ribbonControl1.ExpandCollapseItem, barButtonItemDashboard, barButtonItemDataManagement, barButtonItemReports, barButtonItemSettings, barButtonItemRefresh, barButtonItemSave, barButtonItemExit, barButtonItemLog, barStaticItemStatus, barStaticItemUser, barStaticItemLog, menuItemNew, menuItemOpen, menuItemSave, menuItemSaveAs, menuItemPrint, menuItemOptions, menuItemShowQATBelow, menuItemExit });
            ribbonControl1.Location = new System.Drawing.Point(0, 0);
            ribbonControl1.MaxItemId = 19;
            ribbonControl1.Name = "ribbonControl1";
            ribbonControl1.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] { ribbonPageMain });
            ribbonControl1.Size = new System.Drawing.Size(1438, 160);
            ribbonControl1.StatusBar = ribbonStatusBar1;
            ribbonControl1.ApplicationButtonDropDownControl = applicationMenu1;
            ribbonControl1.ToolbarLocation = DevExpress.XtraBars.Ribbon.RibbonQuickAccessToolbarLocation.Below;
            // 
            // barButtonItemDashboard
            // 
            barButtonItemDashboard.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.CheckDropDown;
            barButtonItemDashboard.Caption = "首页";
            barButtonItemDashboard.Id = 1;
            barButtonItemDashboard.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            barButtonItemDashboard.Name = "barButtonItemDashboard";
            barButtonItemDashboard.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            barButtonItemDashboard.ItemClick += barButtonItemDashboard_ItemClick;
            // 
            // barButtonItemDataManagement
            // 
            barButtonItemDataManagement.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.CheckDropDown;
            barButtonItemDataManagement.Caption = "数据管理";
            barButtonItemDataManagement.Id = 2;
            barButtonItemDataManagement.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            barButtonItemDataManagement.Name = "barButtonItemDataManagement";
            barButtonItemDataManagement.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            barButtonItemDataManagement.ItemClick += barButtonItemDataManagement_ItemClick;
            // 
            // barButtonItemReports
            // 
            barButtonItemReports.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.CheckDropDown;
            barButtonItemReports.Caption = "报表分析";
            barButtonItemReports.Id = 3;
            barButtonItemReports.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            barButtonItemReports.Name = "barButtonItemReports";
            barButtonItemReports.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            barButtonItemReports.ItemClick += barButtonItemReports_ItemClick;
            // 
            // barButtonItemSettings
            // 
            barButtonItemSettings.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.CheckDropDown;
            barButtonItemSettings.Caption = "系统设置";
            barButtonItemSettings.Id = 4;
            barButtonItemSettings.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            barButtonItemSettings.Name = "barButtonItemSettings";
            barButtonItemSettings.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            barButtonItemSettings.ItemClick += barButtonItemSettings_ItemClick;
            // 
            // barButtonItemRefresh
            // 
            barButtonItemRefresh.Caption = "刷新";
            barButtonItemRefresh.Id = 5;
            barButtonItemRefresh.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            barButtonItemRefresh.Name = "barButtonItemRefresh";
            barButtonItemRefresh.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            barButtonItemRefresh.ItemClick += barButtonItemRefresh_ItemClick;
            // 
            // barButtonItemSave
            // 
            barButtonItemSave.Caption = "保存";
            barButtonItemSave.Id = 6;
            barButtonItemSave.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            barButtonItemSave.Name = "barButtonItemSave";
            barButtonItemSave.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            barButtonItemSave.ItemClick += barButtonItemSave_ItemClick;
            // 
            // barButtonItemExit
            // 
            barButtonItemExit.Caption = "退出";
            barButtonItemExit.Id = 7;
            barButtonItemExit.ImageOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            barButtonItemExit.Name = "barButtonItemExit";
            barButtonItemExit.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            barButtonItemExit.ItemClick += barButtonItemExit_ItemClick;
            // 
            // barButtonItemLog
            // 
            barButtonItemLog.Caption = "查看日志";
            barButtonItemLog.Id = 8;
            barButtonItemLog.Name = "barButtonItemLog";
            barButtonItemLog.ItemClick += barButtonItemLog_ItemClick;
            // 
            // barStaticItemStatus
            // 
            barStaticItemStatus.Caption = "就绪";
            barStaticItemStatus.Id = 8;
            barStaticItemStatus.Name = "barStaticItemStatus";
            // 
            // barStaticItemUser
            // 
            barStaticItemUser.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            barStaticItemUser.Caption = "当前用户: 管理员";
            barStaticItemUser.Id = 9;
            barStaticItemUser.Name = "barStaticItemUser";
            // 
            // barStaticItemLog
            // 
            barStaticItemLog.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            barStaticItemLog.Caption = "";
            barStaticItemLog.Id = 10;
            barStaticItemLog.Name = "barStaticItemLog";
            barStaticItemLog.ItemClick += barStaticItemLog_ItemClick;
            // 
            // applicationMenu1
            // 
            applicationMenu1.Name = "applicationMenu1";
            applicationMenu1.Ribbon = ribbonControl1;
            // 
            // menuItemNew
            // 
            menuItemNew.Caption = "新建";
            menuItemNew.Id = 10;
            menuItemNew.Name = "menuItemNew";
            menuItemNew.ItemClick += menuItemNew_ItemClick;
            // 
            // menuItemOpen
            // 
            menuItemOpen.Caption = "打开";
            menuItemOpen.Id = 11;
            menuItemOpen.Name = "menuItemOpen";
            menuItemOpen.ItemClick += menuItemOpen_ItemClick;
            // 
            // menuItemSave
            // 
            menuItemSave.Caption = "保存";
            menuItemSave.Id = 12;
            menuItemSave.Name = "menuItemSave";
            menuItemSave.ItemClick += menuItemSave_ItemClick;
            // 
            // menuItemSaveAs
            // 
            menuItemSaveAs.Caption = "另存为";
            menuItemSaveAs.Id = 13;
            menuItemSaveAs.Name = "menuItemSaveAs";
            menuItemSaveAs.ItemClick += menuItemSaveAs_ItemClick;
            // 
            // menuItemPrint
            // 
            menuItemPrint.Caption = "打印";
            menuItemPrint.Id = 14;
            menuItemPrint.Name = "menuItemPrint";
            menuItemPrint.ItemClick += menuItemPrint_ItemClick;
            // 
            // menuItemOptions
            // 
            menuItemOptions.Caption = "选项";
            menuItemOptions.Id = 15;
            menuItemOptions.Name = "menuItemOptions";
            menuItemOptions.ItemClick += menuItemOptions_ItemClick;
            // 
            // menuItemShowQATBelow
            // 
            menuItemShowQATBelow.Caption = "在功能区下方显示快速访问工具栏";
            menuItemShowQATBelow.Id = 16;
            menuItemShowQATBelow.Name = "menuItemShowQATBelow";
            menuItemShowQATBelow.Checked = true;  // 默认选中（QAT 在下方）
            menuItemShowQATBelow.ItemClick += menuItemShowQATBelow_ItemClick;
            // 
            // menuItemExit
            // 
            menuItemExit.Caption = "退出";
            menuItemExit.Id = 17;
            menuItemExit.Name = "menuItemExit";
            menuItemExit.ItemClick += menuItemExit_ItemClick;
            // 
            // 添加菜单项到 ApplicationMenu
            // 注意：在 DevExpress WinForms 中，ApplicationMenu 的菜单项通过 BarManager 管理
            // 菜单项将在 Main.cs 的构造函数中通过代码添加
            //
            // 
            // 添加常用按钮到 Quick Access Toolbar
            // 注意：在 DevExpress WinForms 中，QAT 可能需要通过其他方式访问
            // 暂时注释掉，如果需要可以后续实现
            // 
            // ribbonControl1.QuickAccessToolbar.ItemLinks.Add(barButtonItemSave);
            // ribbonControl1.QuickAccessToolbar.ItemLinks.Add(barButtonItemRefresh);
            // ribbonControl1.QuickAccessToolbar.ShowCustomizeItem = true;
            // 
            // ribbonPageMain
            // 
            ribbonPageMain.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] { ribbonPageGroupNavigation, ribbonPageGroupActions });
            ribbonPageMain.Name = "ribbonPageMain";
            ribbonPageMain.Text = "主页";
            // 
            // ribbonPageGroupNavigation
            // 
            ribbonPageGroupNavigation.ItemLinks.Add(barButtonItemDashboard);
            ribbonPageGroupNavigation.ItemLinks.Add(barButtonItemDataManagement);
            ribbonPageGroupNavigation.ItemLinks.Add(barButtonItemReports);
            ribbonPageGroupNavigation.ItemLinks.Add(barButtonItemSettings);
            ribbonPageGroupNavigation.Name = "ribbonPageGroupNavigation";
            ribbonPageGroupNavigation.Text = "导航";
            // 
            // ribbonPageGroupActions
            // 
            ribbonPageGroupActions.ItemLinks.Add(barButtonItemRefresh);
            ribbonPageGroupActions.ItemLinks.Add(barButtonItemSave);
            ribbonPageGroupActions.ItemLinks.Add(barButtonItemLog);
            ribbonPageGroupActions.ItemLinks.Add(barButtonItemExit);
            ribbonPageGroupActions.Name = "ribbonPageGroupActions";
            ribbonPageGroupActions.Text = "操作";
            // 
            // ribbonStatusBar1
            // 
            ribbonStatusBar1.ItemLinks.Add(barStaticItemStatus);
            ribbonStatusBar1.ItemLinks.Add(barStaticItemUser);
            ribbonStatusBar1.ItemLinks.Add(barStaticItemLog);
            ribbonStatusBar1.Location = new System.Drawing.Point(0, 875);
            ribbonStatusBar1.Name = "ribbonStatusBar1";
            ribbonStatusBar1.Ribbon = ribbonControl1;
            ribbonStatusBar1.Size = new System.Drawing.Size(1438, 24);
            // 
            // splitContainerControl1
            // 
            splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerControl1.Horizontal = false;
            splitContainerControl1.Location = new System.Drawing.Point(0, 160);
            splitContainerControl1.Name = "splitContainerControl1";
            // 
            // splitContainerControl1.Panel1
            // 
            splitContainerControl1.Panel1.Controls.Add(contentPanel);
            splitContainerControl1.Panel1.Text = "Panel1";
            // 
            // splitContainerControl1.Panel2
            // 
            splitContainerControl1.Panel2.Controls.Add(logWindow1);
            splitContainerControl1.Panel2.Text = "Panel2";
            splitContainerControl1.Size = new System.Drawing.Size(1438, 715);
            splitContainerControl1.SplitterPosition = 465;
            splitContainerControl1.TabIndex = 3;
            splitContainerControl1.ShowSplitGlyph = DevExpress.Utils.DefaultBoolean.True;
            // 
            // contentPanel
            // 
            contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            contentPanel.Location = new System.Drawing.Point(0, 0);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new System.Drawing.Size(1438, 465);
            contentPanel.TabIndex = 0;
            // 
            // logWindow1
            // 
            logWindow1.Dock = System.Windows.Forms.DockStyle.Fill;
            logWindow1.Location = new System.Drawing.Point(0, 0);
            logWindow1.Name = "logWindow1";
            logWindow1.Size = new System.Drawing.Size(1438, 245);
            logWindow1.TabIndex = 0;
            // 
            // Main
            // 
            AllowFormGlass = DevExpress.Utils.DefaultBoolean.False;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1438, 899);
            Controls.Add(splitContainerControl1);
            Controls.Add(ribbonStatusBar1);
            Controls.Add(ribbonControl1);
            Name = "Main";
            Ribbon = ribbonControl1;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            StatusBar = ribbonStatusBar1;
            Text = "永利系统 - 数据管理平台";
            FormClosing += Main_FormClosing;
            Load += Main_Load;
            ((System.ComponentModel.ISupportInitialize)ribbonControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)applicationMenu1).EndInit();
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1.Panel1).EndInit();
            splitContainerControl1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1.Panel2).EndInit();
            splitContainerControl1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1).EndInit();
            splitContainerControl1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraBars.Ribbon.ApplicationMenu applicationMenu1;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPageMain;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupNavigation;
        private DevExpress.XtraBars.BarButtonItem barButtonItemDashboard;
        private DevExpress.XtraBars.BarButtonItem barButtonItemDataManagement;
        private DevExpress.XtraBars.BarButtonItem barButtonItemReports;
        private DevExpress.XtraBars.BarButtonItem barButtonItemSettings;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupActions;
        private DevExpress.XtraBars.BarButtonItem barButtonItemRefresh;
        private DevExpress.XtraBars.BarButtonItem barButtonItemSave;
        private DevExpress.XtraBars.BarButtonItem barButtonItemExit;
        private DevExpress.XtraBars.BarButtonItem barButtonItemLog;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar1;
        private DevExpress.XtraBars.BarStaticItem barStaticItemStatus;
        private DevExpress.XtraBars.BarStaticItem barStaticItemUser;
        private DevExpress.XtraBars.BarStaticItem barStaticItemLog;
        private DevExpress.XtraBars.BarButtonItem menuItemNew;
        private DevExpress.XtraBars.BarButtonItem menuItemOpen;
        private DevExpress.XtraBars.BarButtonItem menuItemSave;
        private DevExpress.XtraBars.BarButtonItem menuItemSaveAs;
        private DevExpress.XtraBars.BarButtonItem menuItemPrint;
        private DevExpress.XtraBars.BarButtonItem menuItemOptions;
        private DevExpress.XtraBars.BarCheckItem menuItemShowQATBelow;
        private DevExpress.XtraBars.BarButtonItem menuItemExit;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private System.Windows.Forms.Panel contentPanel;
        private LogWindow logWindow1;
    }
}
