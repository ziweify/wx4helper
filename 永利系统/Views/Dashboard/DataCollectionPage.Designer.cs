namespace 永利系统.Views.Dashboard
{
    partial class DataCollectionPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            splitContainerLeft = new System.Windows.Forms.SplitContainer();
            groupCompleted = new DevExpress.XtraEditors.GroupControl();
            gridCompleted = new DevExpress.XtraGrid.GridControl();
            gridViewCompleted = new DevExpress.XtraGrid.Views.Grid.GridView();
            colCompletedId = new DevExpress.XtraGrid.Columns.GridColumn();
            colCompletedIssueId = new DevExpress.XtraGrid.Columns.GridColumn();
            colCompletedOpenData = new DevExpress.XtraGrid.Columns.GridColumn();
            colCompletedCollectionTime = new DevExpress.XtraGrid.Columns.GridColumn();
            xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            xtraTabPageConfig = new DevExpress.XtraTab.XtraTabPage();
            groupControl_Monitor_config = new DevExpress.XtraEditors.GroupControl();
            btnStopAuto = new DevExpress.XtraEditors.SimpleButton();
            btnStartAuto = new DevExpress.XtraEditors.SimpleButton();
            groupSubmitAddress = new DevExpress.XtraEditors.GroupControl();
            memoSubmitAddresses = new DevExpress.XtraEditors.MemoEdit();
            groupDataSource = new DevExpress.XtraEditors.GroupControl();
            btnTestConnection = new DevExpress.XtraEditors.SimpleButton();
            txtProxyAddress = new DevExpress.XtraEditors.TextEdit();
            chkUseProxy = new DevExpress.XtraEditors.CheckEdit();
            txtDataSourceUrl = new DevExpress.XtraEditors.TextEdit();
            labelControl6 = new DevExpress.XtraEditors.LabelControl();
            groupIssueInfo = new DevExpress.XtraEditors.GroupControl();
            btnGetIssueInfo = new DevExpress.XtraEditors.SimpleButton();
            txtCountdown = new DevExpress.XtraEditors.TextEdit();
            labelControl5 = new DevExpress.XtraEditors.LabelControl();
            txtNextTime = new DevExpress.XtraEditors.TextEdit();
            labelControl4 = new DevExpress.XtraEditors.LabelControl();
            txtNextIssue = new DevExpress.XtraEditors.TextEdit();
            labelControl3 = new DevExpress.XtraEditors.LabelControl();
            txtCurrentTime = new DevExpress.XtraEditors.TextEdit();
            labelControl2 = new DevExpress.XtraEditors.LabelControl();
            txtCurrentIssue = new DevExpress.XtraEditors.TextEdit();
            labelControl1 = new DevExpress.XtraEditors.LabelControl();
            xtraTabPageMonitorA = new DevExpress.XtraTab.XtraTabPage();
            xtraTabPageMonitorB = new DevExpress.XtraTab.XtraTabPage();
            xtraTabPageMonitorC = new DevExpress.XtraTab.XtraTabPage();
            groupPending = new DevExpress.XtraEditors.GroupControl();
            gridPending = new DevExpress.XtraGrid.GridControl();
            gridViewPending = new DevExpress.XtraGrid.Views.Grid.GridView();
            colPendingId = new DevExpress.XtraGrid.Columns.GridColumn();
            colPendingIssueId = new DevExpress.XtraGrid.Columns.GridColumn();
            colPendingOpenData = new DevExpress.XtraGrid.Columns.GridColumn();
            colPendingAttemptCount = new DevExpress.XtraGrid.Columns.GridColumn();
            colPendingCreatedTime = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerLeft).BeginInit();
            splitContainerLeft.Panel1.SuspendLayout();
            splitContainerLeft.Panel2.SuspendLayout();
            splitContainerLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupCompleted).BeginInit();
            groupCompleted.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridCompleted).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridViewCompleted).BeginInit();
            ((System.ComponentModel.ISupportInitialize)xtraTabControl1).BeginInit();
            xtraTabControl1.SuspendLayout();
            xtraTabPageConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupControl_Monitor_config).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupSubmitAddress).BeginInit();
            groupSubmitAddress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)memoSubmitAddresses.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupDataSource).BeginInit();
            groupDataSource.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtProxyAddress.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkUseProxy.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtDataSourceUrl.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupIssueInfo).BeginInit();
            groupIssueInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtCountdown.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtNextTime.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtNextIssue.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCurrentTime.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCurrentIssue.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupPending).BeginInit();
            groupPending.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridPending).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridViewPending).BeginInit();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainerMain.Location = new System.Drawing.Point(0, 0);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(splitContainerLeft);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(xtraTabControl1);
            splitContainerMain.Size = new System.Drawing.Size(1388, 756);
            splitContainerMain.SplitterDistance = 326;
            splitContainerMain.TabIndex = 0;
            // 
            // splitContainerLeft
            // 
            splitContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerLeft.Location = new System.Drawing.Point(0, 0);
            splitContainerLeft.Name = "splitContainerLeft";
            splitContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerLeft.Panel1
            // 
            splitContainerLeft.Panel1.Controls.Add(groupPending);
            // 
            // splitContainerLeft.Panel2
            // 
            splitContainerLeft.Panel2.Controls.Add(groupCompleted);
            splitContainerLeft.Panel2.Controls.Add(groupIssueInfo);
            splitContainerLeft.Size = new System.Drawing.Size(326, 756);
            splitContainerLeft.SplitterDistance = 131;
            splitContainerLeft.TabIndex = 0;
            // 
            // groupCompleted
            // 
            groupCompleted.Controls.Add(gridCompleted);
            groupCompleted.Location = new System.Drawing.Point(3, 8);
            groupCompleted.Name = "groupCompleted";
            groupCompleted.Size = new System.Drawing.Size(321, 279);
            groupCompleted.TabIndex = 0;
            groupCompleted.Text = "已完成任务";
            // 
            // gridCompleted
            // 
            gridCompleted.Dock = System.Windows.Forms.DockStyle.Fill;
            gridCompleted.Location = new System.Drawing.Point(2, 23);
            gridCompleted.MainView = gridViewCompleted;
            gridCompleted.Name = "gridCompleted";
            gridCompleted.Size = new System.Drawing.Size(317, 254);
            gridCompleted.TabIndex = 0;
            gridCompleted.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridViewCompleted });
            // 
            // gridViewCompleted
            // 
            gridViewCompleted.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colCompletedId, colCompletedIssueId, colCompletedOpenData, colCompletedCollectionTime });
            gridViewCompleted.DetailHeight = 331;
            gridViewCompleted.GridControl = gridCompleted;
            gridViewCompleted.Name = "gridViewCompleted";
            gridViewCompleted.OptionsBehavior.Editable = false;
            gridViewCompleted.OptionsBehavior.ReadOnly = true;
            gridViewCompleted.OptionsEditForm.PopupEditFormWidth = 700;
            gridViewCompleted.OptionsView.ShowGroupPanel = false;
            // 
            // colCompletedId
            // 
            colCompletedId.Caption = "ID";
            colCompletedId.FieldName = "Id";
            colCompletedId.MinWidth = 26;
            colCompletedId.Name = "colCompletedId";
            colCompletedId.Visible = true;
            colCompletedId.VisibleIndex = 0;
            colCompletedId.Width = 52;
            // 
            // colCompletedIssueId
            // 
            colCompletedIssueId.Caption = "期号";
            colCompletedIssueId.FieldName = "IssueId";
            colCompletedIssueId.MinWidth = 26;
            colCompletedIssueId.Name = "colCompletedIssueId";
            colCompletedIssueId.Visible = true;
            colCompletedIssueId.VisibleIndex = 1;
            colCompletedIssueId.Width = 105;
            // 
            // colCompletedOpenData
            // 
            colCompletedOpenData.Caption = "开奖号码";
            colCompletedOpenData.FieldName = "OpenData";
            colCompletedOpenData.MinWidth = 26;
            colCompletedOpenData.Name = "colCompletedOpenData";
            colCompletedOpenData.Visible = true;
            colCompletedOpenData.VisibleIndex = 2;
            colCompletedOpenData.Width = 131;
            // 
            // colCompletedCollectionTime
            // 
            colCompletedCollectionTime.Caption = "采集时间";
            colCompletedCollectionTime.DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            colCompletedCollectionTime.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            colCompletedCollectionTime.FieldName = "CollectionTime";
            colCompletedCollectionTime.MinWidth = 26;
            colCompletedCollectionTime.Name = "colCompletedCollectionTime";
            colCompletedCollectionTime.Visible = true;
            colCompletedCollectionTime.VisibleIndex = 3;
            colCompletedCollectionTime.Width = 157;
            // 
            // xtraTabControl1
            // 
            xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            xtraTabControl1.Location = new System.Drawing.Point(0, 0);
            xtraTabControl1.Name = "xtraTabControl1";
            xtraTabControl1.SelectedTabPage = xtraTabPageConfig;
            xtraTabControl1.Size = new System.Drawing.Size(1058, 756);
            xtraTabControl1.TabIndex = 0;
            xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { xtraTabPageConfig, xtraTabPageMonitorA, xtraTabPageMonitorB, xtraTabPageMonitorC });
            // 
            // xtraTabPageConfig
            // 
            xtraTabPageConfig.Controls.Add(groupControl_Monitor_config);
            xtraTabPageConfig.Controls.Add(groupSubmitAddress);
            xtraTabPageConfig.Controls.Add(groupDataSource);
            xtraTabPageConfig.Name = "xtraTabPageConfig";
            xtraTabPageConfig.Size = new System.Drawing.Size(1056, 730);
            xtraTabPageConfig.Text = "配置";
            // 
            // groupControl_Monitor_config
            // 
            groupControl_Monitor_config.Location = new System.Drawing.Point(5, 3);
            groupControl_Monitor_config.Name = "groupControl_Monitor_config";
            groupControl_Monitor_config.Size = new System.Drawing.Size(330, 716);
            groupControl_Monitor_config.TabIndex = 5;
            groupControl_Monitor_config.Text = "监控配置";
            // 
            // btnStopAuto
            // 
            btnStopAuto.Appearance.BackColor = System.Drawing.Color.FromArgb(192, 0, 0);
            btnStopAuto.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            btnStopAuto.Appearance.Options.UseBackColor = true;
            btnStopAuto.Appearance.Options.UseFont = true;
            btnStopAuto.Location = new System.Drawing.Point(226, 111);
            btnStopAuto.Name = "btnStopAuto";
            btnStopAuto.Size = new System.Drawing.Size(94, 24);
            btnStopAuto.TabIndex = 1;
            btnStopAuto.Text = "■ 停止采集";
            // 
            // btnStartAuto
            // 
            btnStartAuto.Appearance.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
            btnStartAuto.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            btnStartAuto.Appearance.Options.UseBackColor = true;
            btnStartAuto.Appearance.Options.UseFont = true;
            btnStartAuto.Location = new System.Drawing.Point(227, 86);
            btnStartAuto.Name = "btnStartAuto";
            btnStartAuto.Size = new System.Drawing.Size(94, 21);
            btnStartAuto.TabIndex = 0;
            btnStartAuto.Text = "▶ 开始采集";
            // 
            // groupSubmitAddress
            // 
            groupSubmitAddress.Controls.Add(memoSubmitAddresses);
            groupSubmitAddress.Location = new System.Drawing.Point(339, 296);
            groupSubmitAddress.Name = "groupSubmitAddress";
            groupSubmitAddress.Size = new System.Drawing.Size(345, 189);
            groupSubmitAddress.TabIndex = 2;
            groupSubmitAddress.Text = "提交地址 (多行, 格式: [标识]URL)";
            // 
            // memoSubmitAddresses
            // 
            memoSubmitAddresses.Dock = System.Windows.Forms.DockStyle.Fill;
            memoSubmitAddresses.Location = new System.Drawing.Point(2, 23);
            memoSubmitAddresses.Name = "memoSubmitAddresses";
            memoSubmitAddresses.Size = new System.Drawing.Size(341, 164);
            memoSubmitAddresses.TabIndex = 0;
            // 
            // groupDataSource
            // 
            groupDataSource.Controls.Add(btnTestConnection);
            groupDataSource.Controls.Add(txtProxyAddress);
            groupDataSource.Controls.Add(chkUseProxy);
            groupDataSource.Controls.Add(txtDataSourceUrl);
            groupDataSource.Controls.Add(labelControl6);
            groupDataSource.Location = new System.Drawing.Point(339, 173);
            groupDataSource.Name = "groupDataSource";
            groupDataSource.Size = new System.Drawing.Size(345, 123);
            groupDataSource.TabIndex = 1;
            groupDataSource.Text = "数据源";
            // 
            // btnTestConnection
            // 
            btnTestConnection.Location = new System.Drawing.Point(260, 84);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new System.Drawing.Size(68, 22);
            btnTestConnection.TabIndex = 4;
            btnTestConnection.Text = "测试连接";
            // 
            // txtProxyAddress
            // 
            txtProxyAddress.Location = new System.Drawing.Point(123, 84);
            txtProxyAddress.Name = "txtProxyAddress";
            txtProxyAddress.Size = new System.Drawing.Size(131, 20);
            txtProxyAddress.TabIndex = 3;
            // 
            // chkUseProxy
            // 
            chkUseProxy.Location = new System.Drawing.Point(13, 84);
            chkUseProxy.Name = "chkUseProxy";
            chkUseProxy.Properties.Caption = "使用代理 (HTTP)";
            chkUseProxy.Size = new System.Drawing.Size(105, 20);
            chkUseProxy.TabIndex = 2;
            // 
            // txtDataSourceUrl
            // 
            txtDataSourceUrl.Location = new System.Drawing.Point(13, 56);
            txtDataSourceUrl.Name = "txtDataSourceUrl";
            txtDataSourceUrl.Size = new System.Drawing.Size(315, 20);
            txtDataSourceUrl.TabIndex = 1;
            // 
            // labelControl6
            // 
            labelControl6.Location = new System.Drawing.Point(13, 33);
            labelControl6.Name = "labelControl6";
            labelControl6.Size = new System.Drawing.Size(40, 14);
            labelControl6.TabIndex = 0;
            labelControl6.Text = "数据源:";
            // 
            // groupIssueInfo
            // 
            groupIssueInfo.Controls.Add(btnGetIssueInfo);
            groupIssueInfo.Controls.Add(txtCountdown);
            groupIssueInfo.Controls.Add(labelControl5);
            groupIssueInfo.Controls.Add(btnStopAuto);
            groupIssueInfo.Controls.Add(txtNextTime);
            groupIssueInfo.Controls.Add(btnStartAuto);
            groupIssueInfo.Controls.Add(labelControl4);
            groupIssueInfo.Controls.Add(txtNextIssue);
            groupIssueInfo.Controls.Add(labelControl3);
            groupIssueInfo.Controls.Add(txtCurrentTime);
            groupIssueInfo.Controls.Add(labelControl2);
            groupIssueInfo.Controls.Add(txtCurrentIssue);
            groupIssueInfo.Controls.Add(labelControl1);
            groupIssueInfo.Location = new System.Drawing.Point(5, 293);
            groupIssueInfo.Name = "groupIssueInfo";
            groupIssueInfo.Size = new System.Drawing.Size(317, 325);
            groupIssueInfo.TabIndex = 0;
            groupIssueInfo.Text = "期号信息";
            // 
            // btnGetIssueInfo
            // 
            btnGetIssueInfo.Location = new System.Drawing.Point(241, 56);
            btnGetIssueInfo.Name = "btnGetIssueInfo";
            btnGetIssueInfo.Size = new System.Drawing.Size(77, 24);
            btnGetIssueInfo.TabIndex = 10;
            btnGetIssueInfo.Text = "获取期号信息";
            // 
            // txtCountdown
            // 
            txtCountdown.Location = new System.Drawing.Point(260, 30);
            txtCountdown.Name = "txtCountdown";
            txtCountdown.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            txtCountdown.Properties.Appearance.ForeColor = System.Drawing.Color.Red;
            txtCountdown.Properties.Appearance.Options.UseFont = true;
            txtCountdown.Properties.Appearance.Options.UseForeColor = true;
            txtCountdown.Properties.Appearance.Options.UseTextOptions = true;
            txtCountdown.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            txtCountdown.Properties.ReadOnly = true;
            txtCountdown.Size = new System.Drawing.Size(52, 22);
            txtCountdown.TabIndex = 9;
            // 
            // labelControl5
            // 
            labelControl5.Location = new System.Drawing.Point(223, 33);
            labelControl5.Name = "labelControl5";
            labelControl5.Size = new System.Drawing.Size(36, 14);
            labelControl5.TabIndex = 8;
            labelControl5.Text = "倒计时";
            // 
            // txtNextTime
            // 
            txtNextTime.Location = new System.Drawing.Point(71, 115);
            txtNextTime.Name = "txtNextTime";
            txtNextTime.Properties.ReadOnly = true;
            txtNextTime.Size = new System.Drawing.Size(147, 20);
            txtNextTime.TabIndex = 7;
            // 
            // labelControl4
            // 
            labelControl4.Location = new System.Drawing.Point(13, 118);
            labelControl4.Name = "labelControl4";
            labelControl4.Size = new System.Drawing.Size(52, 14);
            labelControl4.TabIndex = 6;
            labelControl4.Text = "开奖时间:";
            // 
            // txtNextIssue
            // 
            txtNextIssue.Location = new System.Drawing.Point(71, 87);
            txtNextIssue.Name = "txtNextIssue";
            txtNextIssue.Properties.ReadOnly = true;
            txtNextIssue.Size = new System.Drawing.Size(105, 20);
            txtNextIssue.TabIndex = 5;
            // 
            // labelControl3
            // 
            labelControl3.Location = new System.Drawing.Point(13, 90);
            labelControl3.Name = "labelControl3";
            labelControl3.Size = new System.Drawing.Size(52, 14);
            labelControl3.TabIndex = 4;
            labelControl3.Text = "下期期号:";
            // 
            // txtCurrentTime
            // 
            txtCurrentTime.Location = new System.Drawing.Point(71, 59);
            txtCurrentTime.Name = "txtCurrentTime";
            txtCurrentTime.Properties.ReadOnly = true;
            txtCurrentTime.Size = new System.Drawing.Size(147, 20);
            txtCurrentTime.TabIndex = 3;
            // 
            // labelControl2
            // 
            labelControl2.Location = new System.Drawing.Point(13, 61);
            labelControl2.Name = "labelControl2";
            labelControl2.Size = new System.Drawing.Size(52, 14);
            labelControl2.TabIndex = 2;
            labelControl2.Text = "开奖时间:";
            // 
            // txtCurrentIssue
            // 
            txtCurrentIssue.Location = new System.Drawing.Point(71, 30);
            txtCurrentIssue.Name = "txtCurrentIssue";
            txtCurrentIssue.Properties.ReadOnly = true;
            txtCurrentIssue.Size = new System.Drawing.Size(105, 20);
            txtCurrentIssue.TabIndex = 1;
            // 
            // labelControl1
            // 
            labelControl1.Location = new System.Drawing.Point(13, 33);
            labelControl1.Name = "labelControl1";
            labelControl1.Size = new System.Drawing.Size(52, 14);
            labelControl1.TabIndex = 0;
            labelControl1.Text = "当期期号:";
            // 
            // xtraTabPageMonitorA
            // 
            xtraTabPageMonitorA.Name = "xtraTabPageMonitorA";
            xtraTabPageMonitorA.Size = new System.Drawing.Size(1056, 730);
            xtraTabPageMonitorA.Text = "监控A";
            // 
            // xtraTabPageMonitorB
            // 
            xtraTabPageMonitorB.Name = "xtraTabPageMonitorB";
            xtraTabPageMonitorB.Size = new System.Drawing.Size(1056, 730);
            xtraTabPageMonitorB.Text = "监控B";
            // 
            // xtraTabPageMonitorC
            // 
            xtraTabPageMonitorC.Name = "xtraTabPageMonitorC";
            xtraTabPageMonitorC.Size = new System.Drawing.Size(1056, 730);
            xtraTabPageMonitorC.Text = "监控C";
            // 
            // groupPending
            // 
            groupPending.Controls.Add(gridPending);
            groupPending.Dock = System.Windows.Forms.DockStyle.Fill;
            groupPending.Location = new System.Drawing.Point(0, 0);
            groupPending.Name = "groupPending";
            groupPending.Size = new System.Drawing.Size(326, 131);
            groupPending.TabIndex = 1;
            groupPending.Text = "待采集任务";
            // 
            // gridPending
            // 
            gridPending.Dock = System.Windows.Forms.DockStyle.Fill;
            gridPending.Location = new System.Drawing.Point(2, 23);
            gridPending.MainView = gridViewPending;
            gridPending.Name = "gridPending";
            gridPending.Size = new System.Drawing.Size(322, 106);
            gridPending.TabIndex = 0;
            gridPending.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridViewPending });
            // 
            // gridViewPending
            // 
            gridViewPending.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colPendingId, colPendingIssueId, colPendingOpenData, colPendingAttemptCount, colPendingCreatedTime });
            gridViewPending.DetailHeight = 331;
            gridViewPending.GridControl = gridPending;
            gridViewPending.Name = "gridViewPending";
            gridViewPending.OptionsBehavior.Editable = false;
            gridViewPending.OptionsBehavior.ReadOnly = true;
            gridViewPending.OptionsEditForm.PopupEditFormWidth = 700;
            gridViewPending.OptionsView.ShowGroupPanel = false;
            // 
            // colPendingId
            // 
            colPendingId.Caption = "ID";
            colPendingId.FieldName = "Id";
            colPendingId.MinWidth = 26;
            colPendingId.Name = "colPendingId";
            colPendingId.Visible = true;
            colPendingId.VisibleIndex = 0;
            colPendingId.Width = 52;
            // 
            // colPendingIssueId
            // 
            colPendingIssueId.Caption = "期号";
            colPendingIssueId.FieldName = "IssueId";
            colPendingIssueId.MinWidth = 26;
            colPendingIssueId.Name = "colPendingIssueId";
            colPendingIssueId.Visible = true;
            colPendingIssueId.VisibleIndex = 1;
            colPendingIssueId.Width = 105;
            // 
            // colPendingOpenData
            // 
            colPendingOpenData.Caption = "开奖号码";
            colPendingOpenData.FieldName = "OpenData";
            colPendingOpenData.MinWidth = 26;
            colPendingOpenData.Name = "colPendingOpenData";
            colPendingOpenData.Visible = true;
            colPendingOpenData.VisibleIndex = 2;
            colPendingOpenData.Width = 131;
            // 
            // colPendingAttemptCount
            // 
            colPendingAttemptCount.Caption = "尝试次数";
            colPendingAttemptCount.FieldName = "AttemptCount";
            colPendingAttemptCount.MinWidth = 26;
            colPendingAttemptCount.Name = "colPendingAttemptCount";
            colPendingAttemptCount.Visible = true;
            colPendingAttemptCount.VisibleIndex = 3;
            colPendingAttemptCount.Width = 87;
            // 
            // colPendingCreatedTime
            // 
            colPendingCreatedTime.Caption = "创建时间";
            colPendingCreatedTime.DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            colPendingCreatedTime.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            colPendingCreatedTime.FieldName = "CreatedTime";
            colPendingCreatedTime.MinWidth = 26;
            colPendingCreatedTime.Name = "colPendingCreatedTime";
            colPendingCreatedTime.Visible = true;
            colPendingCreatedTime.VisibleIndex = 4;
            colPendingCreatedTime.Width = 157;
            // 
            // DataCollectionPage
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1388, 756);
            Controls.Add(splitContainerMain);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "DataCollectionPage";
            Text = "数据采集";
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            splitContainerLeft.Panel1.ResumeLayout(false);
            splitContainerLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerLeft).EndInit();
            splitContainerLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupCompleted).EndInit();
            groupCompleted.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridCompleted).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridViewCompleted).EndInit();
            ((System.ComponentModel.ISupportInitialize)xtraTabControl1).EndInit();
            xtraTabControl1.ResumeLayout(false);
            xtraTabPageConfig.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupControl_Monitor_config).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupSubmitAddress).EndInit();
            groupSubmitAddress.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)memoSubmitAddresses.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupDataSource).EndInit();
            groupDataSource.ResumeLayout(false);
            groupDataSource.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtProxyAddress.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkUseProxy.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtDataSourceUrl.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupIssueInfo).EndInit();
            groupIssueInfo.ResumeLayout(false);
            groupIssueInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtCountdown.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtNextTime.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtNextIssue.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCurrentTime.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCurrentIssue.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupPending).EndInit();
            groupPending.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridPending).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridViewPending).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.SplitContainer splitContainerLeft;
        private DevExpress.XtraEditors.GroupControl groupCompleted;
        private DevExpress.XtraGrid.GridControl gridCompleted;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewCompleted;
        private DevExpress.XtraGrid.Columns.GridColumn colCompletedId;
        private DevExpress.XtraGrid.Columns.GridColumn colCompletedIssueId;
        private DevExpress.XtraGrid.Columns.GridColumn colCompletedOpenData;
        private DevExpress.XtraGrid.Columns.GridColumn colCompletedCollectionTime;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageConfig;
        private DevExpress.XtraEditors.SimpleButton btnStopAuto;
        private DevExpress.XtraEditors.SimpleButton btnStartAuto;
        private DevExpress.XtraEditors.GroupControl groupSubmitAddress;
        private DevExpress.XtraEditors.MemoEdit memoSubmitAddresses;
        private DevExpress.XtraEditors.GroupControl groupDataSource;
        private DevExpress.XtraEditors.SimpleButton btnTestConnection;
        private DevExpress.XtraEditors.TextEdit txtProxyAddress;
        private DevExpress.XtraEditors.CheckEdit chkUseProxy;
        private DevExpress.XtraEditors.TextEdit txtDataSourceUrl;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.GroupControl groupIssueInfo;
        private DevExpress.XtraEditors.SimpleButton btnGetIssueInfo;
        private DevExpress.XtraEditors.TextEdit txtCountdown;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.TextEdit txtNextTime;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.TextEdit txtNextIssue;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.TextEdit txtCurrentTime;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.TextEdit txtCurrentIssue;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageMonitorA;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageMonitorB;
        private DevExpress.XtraTab.XtraTabPage xtraTabPageMonitorC;
        private DevExpress.XtraEditors.GroupControl groupControl_Monitor_config;
        private DevExpress.XtraEditors.GroupControl groupPending;
        private DevExpress.XtraGrid.GridControl gridPending;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewPending;
        private DevExpress.XtraGrid.Columns.GridColumn colPendingId;
        private DevExpress.XtraGrid.Columns.GridColumn colPendingIssueId;
        private DevExpress.XtraGrid.Columns.GridColumn colPendingOpenData;
        private DevExpress.XtraGrid.Columns.GridColumn colPendingAttemptCount;
        private DevExpress.XtraGrid.Columns.GridColumn colPendingCreatedTime;
    }
}
