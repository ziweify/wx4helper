namespace BinGoPlans
{
    partial class Form1
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
            toolbarPanel = new System.Windows.Forms.Panel();
            positionLabel = new System.Windows.Forms.Label();
            _positionCombo = new DevExpress.XtraEditors.ComboBoxEdit();
            playTypeLabel = new System.Windows.Forms.Label();
            _playTypeCombo = new DevExpress.XtraEditors.ComboBoxEdit();
            trendPeriodLabel = new System.Windows.Forms.Label();
            _trendPeriodCombo = new DevExpress.XtraEditors.ComboBoxEdit();
            dateLabel = new System.Windows.Forms.Label();
            _datePicker = new DevExpress.XtraEditors.DateEdit();
            usernameLabel = new System.Windows.Forms.Label();
            _usernameEdit = new DevExpress.XtraEditors.TextEdit();
            passwordLabel = new System.Windows.Forms.Label();
            _passwordEdit = new DevExpress.XtraEditors.TextEdit();
            _autoLoginCheck = new DevExpress.XtraEditors.CheckEdit();
            loginBtn = new DevExpress.XtraEditors.SimpleButton();
            loadDataBtn = new DevExpress.XtraEditors.SimpleButton();
            _mainTabControl = new DevExpress.XtraTab.XtraTabControl();
            tabRoadBead = new DevExpress.XtraTab.XtraTabPage();
            mainSplitContainer = new System.Windows.Forms.SplitContainer();
            roadBeadPanel = new System.Windows.Forms.Panel();
            roadBeadControl = new BinGoPlans.Controls.RoadBeadControl();
            bottomSplitContainer = new System.Windows.Forms.SplitContainer();
            _lotteryDataGrid = new DevExpress.XtraGrid.GridControl();
            _lotteryDataGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            infoPanel = new System.Windows.Forms.Panel();
            tabTrend = new DevExpress.XtraTab.XtraTabPage();
            trendChartControl = new BinGoPlans.Controls.TrendChartControl();
            tabConsecutive = new DevExpress.XtraTab.XtraTabPage();
            consecutiveStatsControl = new BinGoPlans.Controls.ConsecutiveStatsControl();
            tabCountStats = new DevExpress.XtraTab.XtraTabPage();
            countStatsGrid = new DevExpress.XtraGrid.GridControl();
            countStatsGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            toolbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_positionCombo.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_playTypeCombo.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_trendPeriodCombo.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_datePicker.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_datePicker.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_usernameEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_passwordEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_autoLoginCheck.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_mainTabControl).BeginInit();
            _mainTabControl.SuspendLayout();
            tabRoadBead.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            roadBeadPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bottomSplitContainer).BeginInit();
            bottomSplitContainer.Panel1.SuspendLayout();
            bottomSplitContainer.Panel2.SuspendLayout();
            bottomSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_lotteryDataGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_lotteryDataGridView).BeginInit();
            tabTrend.SuspendLayout();
            tabConsecutive.SuspendLayout();
            tabCountStats.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)countStatsGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)countStatsGridView).BeginInit();
            SuspendLayout();
            // 
            // toolbarPanel
            // 
            toolbarPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            toolbarPanel.Controls.Add(positionLabel);
            toolbarPanel.Controls.Add(_positionCombo);
            toolbarPanel.Controls.Add(playTypeLabel);
            toolbarPanel.Controls.Add(_playTypeCombo);
            toolbarPanel.Controls.Add(trendPeriodLabel);
            toolbarPanel.Controls.Add(_trendPeriodCombo);
            toolbarPanel.Controls.Add(dateLabel);
            toolbarPanel.Controls.Add(_datePicker);
            toolbarPanel.Controls.Add(usernameLabel);
            toolbarPanel.Controls.Add(_usernameEdit);
            toolbarPanel.Controls.Add(passwordLabel);
            toolbarPanel.Controls.Add(_passwordEdit);
            toolbarPanel.Controls.Add(_autoLoginCheck);
            toolbarPanel.Controls.Add(loginBtn);
            toolbarPanel.Controls.Add(loadDataBtn);
            toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            toolbarPanel.Location = new System.Drawing.Point(0, 0);
            toolbarPanel.Name = "toolbarPanel";
            toolbarPanel.Padding = new System.Windows.Forms.Padding(5);
            toolbarPanel.Size = new System.Drawing.Size(1600, 50);
            toolbarPanel.TabIndex = 0;
            // 
            // positionLabel
            // 
            positionLabel.AutoSize = true;
            positionLabel.Location = new System.Drawing.Point(10, 15);
            positionLabel.Name = "positionLabel";
            positionLabel.Size = new System.Drawing.Size(35, 14);
            positionLabel.TabIndex = 0;
            positionLabel.Text = "位置:";
            // 
            // _positionCombo
            // 
            _positionCombo.Location = new System.Drawing.Point(60, 12);
            _positionCombo.Name = "_positionCombo";
            _positionCombo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            _positionCombo.Properties.Items.AddRange(new object[] { "P1", "P2", "P3", "P4", "P5", "总和" });
            _positionCombo.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _positionCombo.Size = new System.Drawing.Size(100, 20);
            _positionCombo.TabIndex = 1;
            // 
            // playTypeLabel
            // 
            playTypeLabel.AutoSize = true;
            playTypeLabel.Location = new System.Drawing.Point(180, 15);
            playTypeLabel.Name = "playTypeLabel";
            playTypeLabel.Size = new System.Drawing.Size(35, 14);
            playTypeLabel.TabIndex = 2;
            playTypeLabel.Text = "玩法:";
            // 
            // _playTypeCombo
            // 
            _playTypeCombo.Location = new System.Drawing.Point(230, 12);
            _playTypeCombo.Name = "_playTypeCombo";
            _playTypeCombo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            _playTypeCombo.Properties.Items.AddRange(new object[] { "大小", "单双", "尾大小", "合单双", "龙虎" });
            _playTypeCombo.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _playTypeCombo.Size = new System.Drawing.Size(100, 20);
            _playTypeCombo.TabIndex = 3;
            // 
            // trendPeriodLabel
            // 
            trendPeriodLabel.AutoSize = true;
            trendPeriodLabel.Location = new System.Drawing.Point(350, 15);
            trendPeriodLabel.Name = "trendPeriodLabel";
            trendPeriodLabel.Size = new System.Drawing.Size(59, 14);
            trendPeriodLabel.TabIndex = 4;
            trendPeriodLabel.Text = "走势周期:";
            // 
            // _trendPeriodCombo
            // 
            _trendPeriodCombo.Location = new System.Drawing.Point(430, 12);
            _trendPeriodCombo.Name = "_trendPeriodCombo";
            _trendPeriodCombo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            _trendPeriodCombo.Properties.Items.AddRange(new object[] { "10期", "50期", "100期", "203期(日)", "3日", "一周", "一月", "5日线" });
            _trendPeriodCombo.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _trendPeriodCombo.Size = new System.Drawing.Size(120, 20);
            _trendPeriodCombo.TabIndex = 5;
            // 
            // dateLabel
            // 
            dateLabel.AutoSize = true;
            dateLabel.Location = new System.Drawing.Point(570, 15);
            dateLabel.Name = "dateLabel";
            dateLabel.Size = new System.Drawing.Size(35, 14);
            dateLabel.TabIndex = 6;
            dateLabel.Text = "日期:";
            // 
            // _datePicker
            // 
            _datePicker.EditValue = null;
            _datePicker.Location = new System.Drawing.Point(620, 12);
            _datePicker.Name = "_datePicker";
            _datePicker.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            _datePicker.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            _datePicker.Size = new System.Drawing.Size(120, 20);
            _datePicker.TabIndex = 7;
            // 
            // usernameLabel
            // 
            usernameLabel.AutoSize = true;
            usernameLabel.Location = new System.Drawing.Point(760, 15);
            usernameLabel.Name = "usernameLabel";
            usernameLabel.Size = new System.Drawing.Size(35, 14);
            usernameLabel.TabIndex = 8;
            usernameLabel.Text = "账号:";
            // 
            // _usernameEdit
            // 
            _usernameEdit.Location = new System.Drawing.Point(810, 12);
            _usernameEdit.Name = "_usernameEdit";
            _usernameEdit.Size = new System.Drawing.Size(100, 20);
            _usernameEdit.TabIndex = 9;
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.Location = new System.Drawing.Point(920, 15);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new System.Drawing.Size(35, 14);
            passwordLabel.TabIndex = 10;
            passwordLabel.Text = "密码:";
            // 
            // _passwordEdit
            // 
            _passwordEdit.Location = new System.Drawing.Point(970, 12);
            _passwordEdit.Name = "_passwordEdit";
            _passwordEdit.Properties.PasswordChar = '*';
            _passwordEdit.Size = new System.Drawing.Size(100, 20);
            _passwordEdit.TabIndex = 11;
            // 
            // _autoLoginCheck
            // 
            _autoLoginCheck.Location = new System.Drawing.Point(1080, 12);
            _autoLoginCheck.Name = "_autoLoginCheck";
            _autoLoginCheck.Properties.Caption = "自动登录";
            _autoLoginCheck.Size = new System.Drawing.Size(80, 20);
            _autoLoginCheck.TabIndex = 12;
            // 
            // loginBtn
            // 
            loginBtn.Location = new System.Drawing.Point(1170, 10);
            loginBtn.Name = "loginBtn";
            loginBtn.Size = new System.Drawing.Size(60, 23);
            loginBtn.TabIndex = 13;
            loginBtn.Text = "登录";
            // 
            // loadDataBtn
            // 
            loadDataBtn.Location = new System.Drawing.Point(1240, 10);
            loadDataBtn.Name = "loadDataBtn";
            loadDataBtn.Size = new System.Drawing.Size(100, 23);
            loadDataBtn.TabIndex = 14;
            loadDataBtn.Text = "加载数据";
            // 
            // _mainTabControl
            // 
            _mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            _mainTabControl.Location = new System.Drawing.Point(0, 50);
            _mainTabControl.Name = "_mainTabControl";
            _mainTabControl.Padding = new System.Windows.Forms.Padding(5);
            _mainTabControl.SelectedTabPage = tabRoadBead;
            _mainTabControl.Size = new System.Drawing.Size(1600, 950);
            _mainTabControl.TabIndex = 1;
            _mainTabControl.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { tabRoadBead, tabTrend, tabConsecutive, tabCountStats });
            // 
            // tabRoadBead
            // 
            tabRoadBead.Controls.Add(mainSplitContainer);
            tabRoadBead.Name = "tabRoadBead";
            tabRoadBead.Size = new System.Drawing.Size(1598, 924);
            tabRoadBead.Text = "路珠统计";
            // 
            // mainSplitContainer
            // 
            mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            mainSplitContainer.Name = "mainSplitContainer";
            mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mainSplitContainer.Panel1
            // 
            mainSplitContainer.Panel1.Controls.Add(roadBeadPanel);
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.Controls.Add(bottomSplitContainer);
            mainSplitContainer.Size = new System.Drawing.Size(1598, 924);
            mainSplitContainer.SplitterDistance = 401;
            mainSplitContainer.TabIndex = 0;
            // 
            // roadBeadPanel
            // 
            roadBeadPanel.AutoScroll = true;
            roadBeadPanel.BackColor = System.Drawing.Color.White;
            roadBeadPanel.Controls.Add(roadBeadControl);
            roadBeadPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            roadBeadPanel.Location = new System.Drawing.Point(0, 0);
            roadBeadPanel.Name = "roadBeadPanel";
            roadBeadPanel.Size = new System.Drawing.Size(1598, 401);
            roadBeadPanel.TabIndex = 0;
            // 
            // roadBeadControl
            // 
            roadBeadControl.BackColor = System.Drawing.Color.White;
            roadBeadControl.CellSize = 25;
            roadBeadControl.Location = new System.Drawing.Point(0, 0);
            roadBeadControl.Name = "roadBeadControl";
            roadBeadControl.Size = new System.Drawing.Size(100, 100);
            roadBeadControl.TabIndex = 0;
            // 
            // bottomSplitContainer
            // 
            bottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            bottomSplitContainer.Location = new System.Drawing.Point(0, 0);
            bottomSplitContainer.Name = "bottomSplitContainer";
            bottomSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // bottomSplitContainer.Panel1
            // 
            bottomSplitContainer.Panel1.Controls.Add(_lotteryDataGrid);
            // 
            // bottomSplitContainer.Panel2
            // 
            bottomSplitContainer.Panel2.Controls.Add(infoPanel);
            bottomSplitContainer.Size = new System.Drawing.Size(1598, 519);
            bottomSplitContainer.SplitterDistance = 201;
            bottomSplitContainer.TabIndex = 0;
            // 
            // _lotteryDataGrid
            // 
            _lotteryDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            _lotteryDataGrid.Location = new System.Drawing.Point(0, 0);
            _lotteryDataGrid.MainView = _lotteryDataGridView;
            _lotteryDataGrid.Name = "_lotteryDataGrid";
            _lotteryDataGrid.Size = new System.Drawing.Size(1598, 201);
            _lotteryDataGrid.TabIndex = 0;
            _lotteryDataGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { _lotteryDataGridView });
            // 
            // _lotteryDataGridView
            // 
            _lotteryDataGridView.GridControl = _lotteryDataGrid;
            _lotteryDataGridView.Name = "_lotteryDataGridView";
            _lotteryDataGridView.OptionsBehavior.Editable = false;
            _lotteryDataGridView.OptionsView.ShowGroupPanel = false;
            // 
            // infoPanel
            // 
            infoPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            infoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            infoPanel.Location = new System.Drawing.Point(0, 0);
            infoPanel.Name = "infoPanel";
            infoPanel.Size = new System.Drawing.Size(1598, 314);
            infoPanel.TabIndex = 0;
            // 
            // tabTrend
            // 
            tabTrend.Controls.Add(trendChartControl);
            tabTrend.Name = "tabTrend";
            tabTrend.Size = new System.Drawing.Size(1598, 924);
            tabTrend.Text = "走势图";
            // 
            // trendChartControl
            // 
            trendChartControl.BackColor = System.Drawing.Color.White;
            trendChartControl.Dock = System.Windows.Forms.DockStyle.Fill;
            trendChartControl.Location = new System.Drawing.Point(0, 0);
            trendChartControl.Name = "trendChartControl";
            trendChartControl.PlayType = BaiShengVx3Plus.Shared.Models.Games.Binggo.Statistics.GamePlayType.Size;
            trendChartControl.Size = new System.Drawing.Size(1598, 924);
            trendChartControl.TabIndex = 0;
            // 
            // tabConsecutive
            // 
            tabConsecutive.Controls.Add(consecutiveStatsControl);
            tabConsecutive.Name = "tabConsecutive";
            tabConsecutive.Size = new System.Drawing.Size(1598, 924);
            tabConsecutive.Text = "连续统计";
            // 
            // consecutiveStatsControl
            // 
            consecutiveStatsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            consecutiveStatsControl.Location = new System.Drawing.Point(0, 0);
            consecutiveStatsControl.Name = "consecutiveStatsControl";
            consecutiveStatsControl.Size = new System.Drawing.Size(1598, 924);
            consecutiveStatsControl.TabIndex = 0;
            // 
            // tabCountStats
            // 
            tabCountStats.Controls.Add(countStatsGrid);
            tabCountStats.Name = "tabCountStats";
            tabCountStats.Size = new System.Drawing.Size(1598, 924);
            tabCountStats.Text = "数量统计";
            // 
            // countStatsGrid
            // 
            countStatsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            countStatsGrid.Location = new System.Drawing.Point(0, 0);
            countStatsGrid.MainView = countStatsGridView;
            countStatsGrid.Name = "countStatsGrid";
            countStatsGrid.Size = new System.Drawing.Size(1598, 924);
            countStatsGrid.TabIndex = 0;
            countStatsGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { countStatsGridView });
            // 
            // countStatsGridView
            // 
            countStatsGridView.GridControl = countStatsGrid;
            countStatsGridView.Name = "countStatsGridView";
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1600, 1000);
            Controls.Add(_mainTabControl);
            Controls.Add(toolbarPanel);
            Name = "Form1";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "宾果数据统计系统";
            toolbarPanel.ResumeLayout(false);
            toolbarPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_positionCombo.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)_playTypeCombo.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)_trendPeriodCombo.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)_datePicker.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)_datePicker.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)_usernameEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)_passwordEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)_autoLoginCheck.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)_mainTabControl).EndInit();
            _mainTabControl.ResumeLayout(false);
            tabRoadBead.ResumeLayout(false);
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            roadBeadPanel.ResumeLayout(false);
            bottomSplitContainer.Panel1.ResumeLayout(false);
            bottomSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bottomSplitContainer).EndInit();
            bottomSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_lotteryDataGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)_lotteryDataGridView).EndInit();
            tabTrend.ResumeLayout(false);
            tabConsecutive.ResumeLayout(false);
            tabCountStats.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)countStatsGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)countStatsGridView).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel toolbarPanel;
        private System.Windows.Forms.Label positionLabel;
        private DevExpress.XtraEditors.ComboBoxEdit _positionCombo;
        private System.Windows.Forms.Label playTypeLabel;
        private DevExpress.XtraEditors.ComboBoxEdit _playTypeCombo;
        private System.Windows.Forms.Label trendPeriodLabel;
        private DevExpress.XtraEditors.ComboBoxEdit _trendPeriodCombo;
        private System.Windows.Forms.Label dateLabel;
        private DevExpress.XtraEditors.DateEdit _datePicker;
        private System.Windows.Forms.Label usernameLabel;
        private DevExpress.XtraEditors.TextEdit _usernameEdit;
        private System.Windows.Forms.Label passwordLabel;
        private DevExpress.XtraEditors.TextEdit _passwordEdit;
        private DevExpress.XtraEditors.CheckEdit _autoLoginCheck;
        private DevExpress.XtraEditors.SimpleButton loginBtn;
        private DevExpress.XtraEditors.SimpleButton loadDataBtn;
        private DevExpress.XtraTab.XtraTabControl _mainTabControl;
        private DevExpress.XtraTab.XtraTabPage tabRoadBead;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.Panel roadBeadPanel;
        private BinGoPlans.Controls.RoadBeadControl roadBeadControl;
        private System.Windows.Forms.SplitContainer bottomSplitContainer;
        private DevExpress.XtraGrid.GridControl _lotteryDataGrid;
        private DevExpress.XtraGrid.Views.Grid.GridView _lotteryDataGridView;
        private System.Windows.Forms.Panel infoPanel;
        private DevExpress.XtraTab.XtraTabPage tabTrend;
        private BinGoPlans.Controls.TrendChartControl trendChartControl;
        private DevExpress.XtraTab.XtraTabPage tabConsecutive;
        private BinGoPlans.Controls.ConsecutiveStatsControl consecutiveStatsControl;
        private DevExpress.XtraTab.XtraTabPage tabCountStats;
        private DevExpress.XtraGrid.GridControl countStatsGrid;
        private DevExpress.XtraGrid.Views.Grid.GridView countStatsGridView;
    }
}
