namespace YongLiSystem.Views.Dashboard
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
            groupPending = new DevExpress.XtraEditors.GroupControl();
            gridPending = new DevExpress.XtraGrid.GridControl();
            gridViewPending = new DevExpress.XtraGrid.Views.Grid.GridView();
            colPendingId = new DevExpress.XtraGrid.Columns.GridColumn();
            colPendingIssueId = new DevExpress.XtraGrid.Columns.GridColumn();
            colPendingOpenData = new DevExpress.XtraGrid.Columns.GridColumn();
            colPendingAttemptCount = new DevExpress.XtraGrid.Columns.GridColumn();
            colPendingCreatedTime = new DevExpress.XtraGrid.Columns.GridColumn();
            gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            groupCompleted = new DevExpress.XtraEditors.GroupControl();
            gridCompleted = new DevExpress.XtraGrid.GridControl();
            gridViewCompleted = new DevExpress.XtraGrid.Views.Grid.GridView();
            colCompletedId = new DevExpress.XtraGrid.Columns.GridColumn();
            colCompletedIssueId = new DevExpress.XtraGrid.Columns.GridColumn();
            colCompletedOpenData = new DevExpress.XtraGrid.Columns.GridColumn();
            colCompletedCollectionTime = new DevExpress.XtraGrid.Columns.GridColumn();
            groupIssueInfo = new DevExpress.XtraEditors.GroupControl();
            btnGetIssueInfo = new DevExpress.XtraEditors.SimpleButton();
            txtCountdown = new DevExpress.XtraEditors.TextEdit();
            labelControl5 = new DevExpress.XtraEditors.LabelControl();
            btnStopAuto = new DevExpress.XtraEditors.SimpleButton();
            txtNextTime = new DevExpress.XtraEditors.TextEdit();
            btnStartAuto = new DevExpress.XtraEditors.SimpleButton();
            labelControl4 = new DevExpress.XtraEditors.LabelControl();
            txtNextIssue = new DevExpress.XtraEditors.TextEdit();
            labelControl3 = new DevExpress.XtraEditors.LabelControl();
            txtCurrentTime = new DevExpress.XtraEditors.TextEdit();
            labelControl2 = new DevExpress.XtraEditors.LabelControl();
            txtCurrentIssue = new DevExpress.XtraEditors.TextEdit();
            labelControl1 = new DevExpress.XtraEditors.LabelControl();
            groupOpenOrderPostAddr = new DevExpress.XtraEditors.GroupControl();
            groupCustTask = new DevExpress.XtraEditors.GroupControl();
            buttonAddScriptTask = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerLeft).BeginInit();
            splitContainerLeft.Panel1.SuspendLayout();
            splitContainerLeft.Panel2.SuspendLayout();
            splitContainerLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupPending).BeginInit();
            groupPending.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridPending).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridViewPending).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupCompleted).BeginInit();
            groupCompleted.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridCompleted).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridViewCompleted).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupIssueInfo).BeginInit();
            groupIssueInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtCountdown.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtNextTime.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtNextIssue.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCurrentTime.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCurrentIssue.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupOpenOrderPostAddr).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupCustTask).BeginInit();
            groupCustTask.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainerMain.Location = new System.Drawing.Point(0, 0);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(splitContainerLeft);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(groupCustTask);
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
            splitContainerLeft.Panel2.Controls.Add(groupOpenOrderPostAddr);
            splitContainerLeft.Panel2.Controls.Add(groupCompleted);
            splitContainerLeft.Panel2.Controls.Add(groupIssueInfo);
            splitContainerLeft.Size = new System.Drawing.Size(326, 756);
            splitContainerLeft.SplitterDistance = 131;
            splitContainerLeft.TabIndex = 0;
            // 
            // groupPending
            // 
            groupPending.Controls.Add(gridPending);
            groupPending.Dock = System.Windows.Forms.DockStyle.Fill;
            groupPending.Location = new System.Drawing.Point(0, 0);
            groupPending.Name = "groupPending";
            groupPending.Size = new System.Drawing.Size(326, 131);
            groupPending.TabIndex = 1;
            groupPending.Text = "[自动]待采集任务";
            // 
            // gridPending
            // 
            gridPending.Dock = System.Windows.Forms.DockStyle.Fill;
            gridPending.Location = new System.Drawing.Point(2, 23);
            gridPending.MainView = gridViewPending;
            gridPending.Name = "gridPending";
            gridPending.Size = new System.Drawing.Size(322, 106);
            gridPending.TabIndex = 0;
            gridPending.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridViewPending, gridView1 });
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
            // gridView1
            // 
            gridView1.GridControl = gridPending;
            gridView1.Name = "gridView1";
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
            groupIssueInfo.Size = new System.Drawing.Size(317, 143);
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
            // txtNextTime
            // 
            txtNextTime.Location = new System.Drawing.Point(71, 115);
            txtNextTime.Name = "txtNextTime";
            txtNextTime.Properties.ReadOnly = true;
            txtNextTime.Size = new System.Drawing.Size(147, 20);
            txtNextTime.TabIndex = 7;
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
            // groupOpenOrderPostAddr
            // 
            groupOpenOrderPostAddr.Location = new System.Drawing.Point(5, 442);
            groupOpenOrderPostAddr.Name = "groupOpenOrderPostAddr";
            groupOpenOrderPostAddr.Size = new System.Drawing.Size(320, 176);
            groupOpenOrderPostAddr.TabIndex = 1;
            groupOpenOrderPostAddr.Text = "提交地址";
            // 
            // groupCustTask
            // 
            groupCustTask.Controls.Add(buttonAddScriptTask);
            groupCustTask.Dock = System.Windows.Forms.DockStyle.Fill;
            groupCustTask.Location = new System.Drawing.Point(0, 0);
            groupCustTask.Name = "groupCustTask";
            groupCustTask.Size = new System.Drawing.Size(1058, 756);
            groupCustTask.TabIndex = 0;
            groupCustTask.Text = "[半自动]定制采集任务";
            // 
            // buttonAddScriptTask
            // 
            buttonAddScriptTask.Location = new System.Drawing.Point(145, 0);
            buttonAddScriptTask.Name = "buttonAddScriptTask";
            buttonAddScriptTask.Size = new System.Drawing.Size(46, 23);
            buttonAddScriptTask.TabIndex = 0;
            buttonAddScriptTask.Text = "增加";
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
            ((System.ComponentModel.ISupportInitialize)groupPending).EndInit();
            groupPending.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridPending).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridViewPending).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupCompleted).EndInit();
            groupCompleted.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridCompleted).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridViewCompleted).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupIssueInfo).EndInit();
            groupIssueInfo.ResumeLayout(false);
            groupIssueInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtCountdown.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtNextTime.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtNextIssue.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCurrentTime.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCurrentIssue.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupOpenOrderPostAddr).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupCustTask).EndInit();
            groupCustTask.ResumeLayout(false);
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
        private DevExpress.XtraEditors.SimpleButton btnStopAuto;
        private DevExpress.XtraEditors.SimpleButton btnStartAuto;
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
        private DevExpress.XtraEditors.GroupControl groupPending;
        private DevExpress.XtraGrid.GridControl gridPending;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewPending;
        private DevExpress.XtraGrid.Columns.GridColumn colPendingId;
        private DevExpress.XtraGrid.Columns.GridColumn colPendingIssueId;
        private DevExpress.XtraGrid.Columns.GridColumn colPendingOpenData;
        private DevExpress.XtraGrid.Columns.GridColumn colPendingAttemptCount;
        private DevExpress.XtraGrid.Columns.GridColumn colPendingCreatedTime;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.GroupControl groupOpenOrderPostAddr;
        private DevExpress.XtraEditors.GroupControl groupCustTask;
        private DevExpress.XtraEditors.SimpleButton buttonAddScriptTask;
    }
}
