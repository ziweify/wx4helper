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
            this.components = new System.ComponentModel.Container();
            this.toolbarPanel = new System.Windows.Forms.Panel();
            this.positionLabel = new System.Windows.Forms.Label();
            this._positionCombo = new DevExpress.XtraEditors.ComboBoxEdit();
            this.playTypeLabel = new System.Windows.Forms.Label();
            this._playTypeCombo = new DevExpress.XtraEditors.ComboBoxEdit();
            this.trendPeriodLabel = new System.Windows.Forms.Label();
            this._trendPeriodCombo = new DevExpress.XtraEditors.ComboBoxEdit();
            this.dateLabel = new System.Windows.Forms.Label();
            this._datePicker = new DevExpress.XtraEditors.DateEdit();
            this.usernameLabel = new System.Windows.Forms.Label();
            this._usernameEdit = new DevExpress.XtraEditors.TextEdit();
            this.passwordLabel = new System.Windows.Forms.Label();
            this._passwordEdit = new DevExpress.XtraEditors.TextEdit();
            this._autoLoginCheck = new DevExpress.XtraEditors.CheckEdit();
            this.loginBtn = new DevExpress.XtraEditors.SimpleButton();
            this.loadDataBtn = new DevExpress.XtraEditors.SimpleButton();
            this._mainTabControl = new DevExpress.XtraTab.XtraTabControl();
            this.tabRoadBead = new DevExpress.XtraTab.XtraTabPage();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.roadBeadPanel = new System.Windows.Forms.Panel();
            this.roadBeadControl = new BinGoPlans.Controls.RoadBeadControl();
            this.bottomSplitContainer = new System.Windows.Forms.SplitContainer();
            this._lotteryDataGrid = new DevExpress.XtraGrid.GridControl();
            this._lotteryDataGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.infoPanel = new System.Windows.Forms.Panel();
            this.tabTrend = new DevExpress.XtraTab.XtraTabPage();
            this.trendChartControl = new BinGoPlans.Controls.TrendChartControl();
            this.tabConsecutive = new DevExpress.XtraTab.XtraTabPage();
            this.consecutiveStatsControl = new BinGoPlans.Controls.ConsecutiveStatsControl();
            this.tabCountStats = new DevExpress.XtraTab.XtraTabPage();
            this.countStatsGrid = new DevExpress.XtraGrid.GridControl();
            this.countStatsGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this._positionCombo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._playTypeCombo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._trendPeriodCombo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._datePicker.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._datePicker.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._usernameEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._passwordEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._autoLoginCheck.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._mainTabControl)).BeginInit();
            this._mainTabControl.SuspendLayout();
            this.tabRoadBead.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).BeginInit();
            this.bottomSplitContainer.Panel1.SuspendLayout();
            this.bottomSplitContainer.Panel2.SuspendLayout();
            this.bottomSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._lotteryDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._lotteryDataGridView)).BeginInit();
            this.tabTrend.SuspendLayout();
            this.tabConsecutive.SuspendLayout();
            this.tabCountStats.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.countStatsGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.countStatsGridView)).BeginInit();
            this.toolbarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolbarPanel
            // 
            this.toolbarPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.toolbarPanel.Controls.Add(this.positionLabel);
            this.toolbarPanel.Controls.Add(this._positionCombo);
            this.toolbarPanel.Controls.Add(this.playTypeLabel);
            this.toolbarPanel.Controls.Add(this._playTypeCombo);
            this.toolbarPanel.Controls.Add(this.trendPeriodLabel);
            this.toolbarPanel.Controls.Add(this._trendPeriodCombo);
            this.toolbarPanel.Controls.Add(this.dateLabel);
            this.toolbarPanel.Controls.Add(this._datePicker);
            this.toolbarPanel.Controls.Add(this.usernameLabel);
            this.toolbarPanel.Controls.Add(this._usernameEdit);
            this.toolbarPanel.Controls.Add(this.passwordLabel);
            this.toolbarPanel.Controls.Add(this._passwordEdit);
            this.toolbarPanel.Controls.Add(this._autoLoginCheck);
            this.toolbarPanel.Controls.Add(this.loginBtn);
            this.toolbarPanel.Controls.Add(this.loadDataBtn);
            this.toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarPanel.Location = new System.Drawing.Point(0, 0);
            this.toolbarPanel.Name = "toolbarPanel";
            this.toolbarPanel.Padding = new System.Windows.Forms.Padding(5);
            this.toolbarPanel.Size = new System.Drawing.Size(1600, 50);
            this.toolbarPanel.TabIndex = 0;
            // 
            // positionLabel
            // 
            this.positionLabel.AutoSize = true;
            this.positionLabel.Location = new System.Drawing.Point(10, 15);
            this.positionLabel.Name = "positionLabel";
            this.positionLabel.Size = new System.Drawing.Size(44, 14);
            this.positionLabel.TabIndex = 0;
            this.positionLabel.Text = "位置:";
            // 
            // _positionCombo
            // 
            this._positionCombo.Location = new System.Drawing.Point(60, 12);
            this._positionCombo.Name = "_positionCombo";
            this._positionCombo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this._positionCombo.Properties.Items.AddRange(new object[] {
            "P1",
            "P2",
            "P3",
            "P4",
            "P5",
            "总和"});
            this._positionCombo.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this._positionCombo.Size = new System.Drawing.Size(100, 20);
            this._positionCombo.TabIndex = 1;
            // 
            // playTypeLabel
            // 
            this.playTypeLabel.AutoSize = true;
            this.playTypeLabel.Location = new System.Drawing.Point(180, 15);
            this.playTypeLabel.Name = "playTypeLabel";
            this.playTypeLabel.Size = new System.Drawing.Size(44, 14);
            this.playTypeLabel.TabIndex = 2;
            this.playTypeLabel.Text = "玩法:";
            // 
            // _playTypeCombo
            // 
            this._playTypeCombo.Location = new System.Drawing.Point(230, 12);
            this._playTypeCombo.Name = "_playTypeCombo";
            this._playTypeCombo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this._playTypeCombo.Properties.Items.AddRange(new object[] {
            "大小",
            "单双",
            "尾大小",
            "合单双",
            "龙虎"});
            this._playTypeCombo.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this._playTypeCombo.Size = new System.Drawing.Size(100, 20);
            this._playTypeCombo.TabIndex = 3;
            // 
            // trendPeriodLabel
            // 
            this.trendPeriodLabel.AutoSize = true;
            this.trendPeriodLabel.Location = new System.Drawing.Point(350, 15);
            this.trendPeriodLabel.Name = "trendPeriodLabel";
            this.trendPeriodLabel.Size = new System.Drawing.Size(68, 14);
            this.trendPeriodLabel.TabIndex = 4;
            this.trendPeriodLabel.Text = "走势周期:";
            // 
            // _trendPeriodCombo
            // 
            this._trendPeriodCombo.Location = new System.Drawing.Point(430, 12);
            this._trendPeriodCombo.Name = "_trendPeriodCombo";
            this._trendPeriodCombo.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this._trendPeriodCombo.Properties.Items.AddRange(new object[] {
            "10期",
            "50期",
            "100期",
            "203期(日)",
            "3日",
            "一周",
            "一月",
            "5日线"});
            this._trendPeriodCombo.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this._trendPeriodCombo.Size = new System.Drawing.Size(120, 20);
            this._trendPeriodCombo.TabIndex = 5;
            // 
            // dateLabel
            // 
            this.dateLabel.AutoSize = true;
            this.dateLabel.Location = new System.Drawing.Point(570, 15);
            this.dateLabel.Name = "dateLabel";
            this.dateLabel.Size = new System.Drawing.Size(44, 14);
            this.dateLabel.TabIndex = 6;
            this.dateLabel.Text = "日期:";
            // 
            // _datePicker
            // 
            this._datePicker.EditValue = null;
            this._datePicker.Location = new System.Drawing.Point(620, 12);
            this._datePicker.Name = "_datePicker";
            this._datePicker.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this._datePicker.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this._datePicker.Size = new System.Drawing.Size(120, 20);
            this._datePicker.TabIndex = 7;
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Location = new System.Drawing.Point(760, 15);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(44, 14);
            this.usernameLabel.TabIndex = 8;
            this.usernameLabel.Text = "账号:";
            // 
            // _usernameEdit
            // 
            this._usernameEdit.Location = new System.Drawing.Point(810, 12);
            this._usernameEdit.Name = "_usernameEdit";
            this._usernameEdit.Size = new System.Drawing.Size(100, 20);
            this._usernameEdit.TabIndex = 9;
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(920, 15);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(44, 14);
            this.passwordLabel.TabIndex = 10;
            this.passwordLabel.Text = "密码:";
            // 
            // _passwordEdit
            // 
            this._passwordEdit.Location = new System.Drawing.Point(970, 12);
            this._passwordEdit.Name = "_passwordEdit";
            this._passwordEdit.Properties.PasswordChar = '*';
            this._passwordEdit.Size = new System.Drawing.Size(100, 20);
            this._passwordEdit.TabIndex = 11;
            // 
            // _autoLoginCheck
            // 
            this._autoLoginCheck.Location = new System.Drawing.Point(1080, 12);
            this._autoLoginCheck.Name = "_autoLoginCheck";
            this._autoLoginCheck.Properties.Caption = "自动登录";
            this._autoLoginCheck.Size = new System.Drawing.Size(80, 20);
            this._autoLoginCheck.TabIndex = 12;
            // 
            // loginBtn
            // 
            this.loginBtn.Location = new System.Drawing.Point(1170, 10);
            this.loginBtn.Name = "loginBtn";
            this.loginBtn.Size = new System.Drawing.Size(60, 23);
            this.loginBtn.TabIndex = 13;
            this.loginBtn.Text = "登录";
            // 
            // loadDataBtn
            // 
            this.loadDataBtn.Location = new System.Drawing.Point(1240, 10);
            this.loadDataBtn.Name = "loadDataBtn";
            this.loadDataBtn.Size = new System.Drawing.Size(100, 23);
            this.loadDataBtn.TabIndex = 14;
            this.loadDataBtn.Text = "加载数据";
            // 
            // _mainTabControl
            // 
            this._mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainTabControl.Location = new System.Drawing.Point(0, 50);
            this._mainTabControl.Name = "_mainTabControl";
            this._mainTabControl.Padding = new System.Windows.Forms.Padding(5);
            this._mainTabControl.SelectedTabPage = this.tabRoadBead;
            this._mainTabControl.Size = new System.Drawing.Size(1600, 950);
            this._mainTabControl.TabIndex = 1;
            this._mainTabControl.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.tabRoadBead,
            this.tabTrend,
            this.tabConsecutive,
            this.tabCountStats});
            // 
            // tabRoadBead
            // 
            this.tabRoadBead.Controls.Add(this.mainSplitContainer);
            this.tabRoadBead.Name = "tabRoadBead";
            this.tabRoadBead.Size = new System.Drawing.Size(1590, 920);
            this.tabRoadBead.Text = "路珠统计";
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.mainSplitContainer.Name = "mainSplitContainer";
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.roadBeadPanel);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.bottomSplitContainer);
            this.mainSplitContainer.Size = new System.Drawing.Size(1590, 920);
            this.mainSplitContainer.SplitterDistance = 400;
            this.mainSplitContainer.TabIndex = 0;
            // 
            // roadBeadPanel
            // 
            this.roadBeadPanel.AutoScroll = true;
            this.roadBeadPanel.BackColor = System.Drawing.Color.White;
            this.roadBeadPanel.Controls.Add(this.roadBeadControl);
            this.roadBeadPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.roadBeadPanel.Location = new System.Drawing.Point(0, 0);
            this.roadBeadPanel.Name = "roadBeadPanel";
            this.roadBeadPanel.Size = new System.Drawing.Size(1590, 400);
            this.roadBeadPanel.TabIndex = 0;
            // 
            // roadBeadControl
            // 
            this.roadBeadControl.BackColor = System.Drawing.Color.White;
            this.roadBeadControl.Location = new System.Drawing.Point(0, 0);
            this.roadBeadControl.Name = "roadBeadControl";
            this.roadBeadControl.Size = new System.Drawing.Size(100, 100);
            this.roadBeadControl.TabIndex = 0;
            // 
            // bottomSplitContainer
            // 
            this.bottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.bottomSplitContainer.Name = "bottomSplitContainer";
            this.bottomSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // bottomSplitContainer.Panel1
            // 
            this.bottomSplitContainer.Panel1.Controls.Add(this._lotteryDataGrid);
            // 
            // bottomSplitContainer.Panel2
            // 
            this.bottomSplitContainer.Panel2.Controls.Add(this.infoPanel);
            this.bottomSplitContainer.Size = new System.Drawing.Size(1590, 516);
            this.bottomSplitContainer.SplitterDistance = 200;
            this.bottomSplitContainer.TabIndex = 0;
            // 
            // _lotteryDataGrid
            // 
            this._lotteryDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lotteryDataGrid.Location = new System.Drawing.Point(0, 0);
            this._lotteryDataGrid.MainView = this._lotteryDataGridView;
            this._lotteryDataGrid.Name = "_lotteryDataGrid";
            this._lotteryDataGrid.Size = new System.Drawing.Size(1590, 200);
            this._lotteryDataGrid.TabIndex = 0;
            this._lotteryDataGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this._lotteryDataGridView});
            // 
            // _lotteryDataGridView
            // 
            this._lotteryDataGridView.GridControl = this._lotteryDataGrid;
            this._lotteryDataGridView.Name = "_lotteryDataGridView";
            this._lotteryDataGridView.OptionsBehavior.Editable = false;
            this._lotteryDataGridView.OptionsSelection.MultiSelect = false;
            this._lotteryDataGridView.OptionsView.ShowGroupPanel = false;
            this._lotteryDataGridView.OptionsView.ShowIndicator = true;
            // 
            // infoPanel
            // 
            this.infoPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.infoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoPanel.Location = new System.Drawing.Point(0, 0);
            this.infoPanel.Name = "infoPanel";
            this.infoPanel.Size = new System.Drawing.Size(1590, 312);
            this.infoPanel.TabIndex = 0;
            // 
            // tabTrend
            // 
            this.tabTrend.Controls.Add(this.trendChartControl);
            this.tabTrend.Name = "tabTrend";
            this.tabTrend.Size = new System.Drawing.Size(1590, 920);
            this.tabTrend.Text = "走势图";
            // 
            // trendChartControl
            // 
            this.trendChartControl.BackColor = System.Drawing.Color.White;
            this.trendChartControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trendChartControl.Location = new System.Drawing.Point(0, 0);
            this.trendChartControl.Name = "trendChartControl";
            this.trendChartControl.Size = new System.Drawing.Size(1590, 920);
            this.trendChartControl.TabIndex = 0;
            // 
            // tabConsecutive
            // 
            this.tabConsecutive.Controls.Add(this.consecutiveStatsControl);
            this.tabConsecutive.Name = "tabConsecutive";
            this.tabConsecutive.Size = new System.Drawing.Size(1590, 920);
            this.tabConsecutive.Text = "连续统计";
            // 
            // consecutiveStatsControl
            // 
            this.consecutiveStatsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consecutiveStatsControl.Location = new System.Drawing.Point(0, 0);
            this.consecutiveStatsControl.Name = "consecutiveStatsControl";
            this.consecutiveStatsControl.Size = new System.Drawing.Size(1590, 920);
            this.consecutiveStatsControl.TabIndex = 0;
            // 
            // tabCountStats
            // 
            this.tabCountStats.Controls.Add(this.countStatsGrid);
            this.tabCountStats.Name = "tabCountStats";
            this.tabCountStats.Size = new System.Drawing.Size(1590, 920);
            this.tabCountStats.Text = "数量统计";
            // 
            // countStatsGrid
            // 
            this.countStatsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.countStatsGrid.Location = new System.Drawing.Point(0, 0);
            this.countStatsGrid.MainView = this.countStatsGridView;
            this.countStatsGrid.Name = "countStatsGrid";
            this.countStatsGrid.Size = new System.Drawing.Size(1590, 920);
            this.countStatsGrid.TabIndex = 0;
            this.countStatsGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.countStatsGridView});
            // 
            // countStatsGridView
            // 
            this.countStatsGridView.GridControl = this.countStatsGrid;
            this.countStatsGridView.Name = "countStatsGridView";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 1000);
            this.Controls.Add(this._mainTabControl);
            this.Controls.Add(this.toolbarPanel);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "宾果数据统计系统";
            ((System.ComponentModel.ISupportInitialize)(this._positionCombo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._playTypeCombo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._trendPeriodCombo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._datePicker.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._datePicker.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._usernameEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._passwordEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._autoLoginCheck.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._mainTabControl)).EndInit();
            this._mainTabControl.ResumeLayout(false);
            this.tabRoadBead.ResumeLayout(false);
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.bottomSplitContainer.Panel1.ResumeLayout(false);
            this.bottomSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).EndInit();
            this.bottomSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._lotteryDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._lotteryDataGridView)).EndInit();
            this.tabTrend.ResumeLayout(false);
            this.tabConsecutive.ResumeLayout(false);
            this.tabCountStats.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.countStatsGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.countStatsGridView)).EndInit();
            this.toolbarPanel.ResumeLayout(false);
            this.toolbarPanel.PerformLayout();
            this.ResumeLayout(false);

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
