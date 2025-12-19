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
            barButtonItemDashboard = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemDataManagement = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemReports = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemSettings = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemRefresh = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemSave = new DevExpress.XtraBars.BarButtonItem();
            barButtonItemExit = new DevExpress.XtraBars.BarButtonItem();
            barStaticItemStatus = new DevExpress.XtraBars.BarStaticItem();
            barStaticItemUser = new DevExpress.XtraBars.BarStaticItem();
            ribbonPageMain = new DevExpress.XtraBars.Ribbon.RibbonPage();
            ribbonPageGroupNavigation = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            ribbonPageGroupActions = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            ribbonStatusBar1 = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            contentPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)ribbonControl1).BeginInit();
            SuspendLayout();
            // 
            // ribbonControl1
            // 
            ribbonControl1.ExpandCollapseItem.Id = 0;
            ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] { ribbonControl1.ExpandCollapseItem, barButtonItemDashboard, barButtonItemDataManagement, barButtonItemReports, barButtonItemSettings, barButtonItemRefresh, barButtonItemSave, barButtonItemExit, barStaticItemStatus, barStaticItemUser });
            ribbonControl1.Location = new System.Drawing.Point(0, 0);
            ribbonControl1.MaxItemId = 10;
            ribbonControl1.Name = "ribbonControl1";
            ribbonControl1.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] { ribbonPageMain });
            ribbonControl1.Size = new System.Drawing.Size(1438, 160);
            ribbonControl1.StatusBar = ribbonStatusBar1;
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
            ribbonPageGroupActions.ItemLinks.Add(barButtonItemExit);
            ribbonPageGroupActions.Name = "ribbonPageGroupActions";
            ribbonPageGroupActions.Text = "操作";
            // 
            // ribbonStatusBar1
            // 
            ribbonStatusBar1.ItemLinks.Add(barStaticItemStatus);
            ribbonStatusBar1.ItemLinks.Add(barStaticItemUser);
            ribbonStatusBar1.Location = new System.Drawing.Point(0, 875);
            ribbonStatusBar1.Name = "ribbonStatusBar1";
            ribbonStatusBar1.Ribbon = ribbonControl1;
            ribbonStatusBar1.Size = new System.Drawing.Size(1438, 24);
            // 
            // contentPanel
            // 
            contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            contentPanel.Location = new System.Drawing.Point(0, 160);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new System.Drawing.Size(1438, 715);
            contentPanel.TabIndex = 2;
            // 
            // Main
            // 
            AllowFormGlass = DevExpress.Utils.DefaultBoolean.False;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1438, 899);
            Controls.Add(contentPanel);
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
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
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
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar1;
        private DevExpress.XtraBars.BarStaticItem barStaticItemStatus;
        private DevExpress.XtraBars.BarStaticItem barStaticItemUser;
        private System.Windows.Forms.Panel contentPanel;
    }
}
