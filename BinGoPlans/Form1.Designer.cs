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
            roadBeadMainPanel = new System.Windows.Forms.Panel();
            roadBeadSplitContainer = new System.Windows.Forms.SplitContainer();
            roadBeadTopSplitContainer = new System.Windows.Forms.SplitContainer();
            roadBeadSizeControl = new BinGoPlans.Controls.RoadBeadControl();
            roadBeadSizeLabel = new System.Windows.Forms.Label();
            roadBeadOddEvenControl = new BinGoPlans.Controls.RoadBeadControl();
            roadBeadOddEvenLabel = new System.Windows.Forms.Label();
            roadBeadBottomSplitContainer = new System.Windows.Forms.SplitContainer();
            roadBeadTailSizeControl = new BinGoPlans.Controls.RoadBeadControl();
            roadBeadTailSizeLabel = new System.Windows.Forms.Label();
            roadBeadSumOddEvenControl = new BinGoPlans.Controls.RoadBeadControl();
            roadBeadSumOddEvenLabel = new System.Windows.Forms.Label();
            tabTrend = new DevExpress.XtraTab.XtraTabPage();
            trendChartControl = new BinGoPlans.Controls.TrendChartControl();
            tabConsecutive = new DevExpress.XtraTab.XtraTabPage();
            consecutiveStatsControl = new BinGoPlans.Controls.ConsecutiveStatsControl();
            tabCountStats = new DevExpress.XtraTab.XtraTabPage();
            countStatsGrid = new DevExpress.XtraGrid.GridControl();
            countStatsGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            bottomSplitContainer = new System.Windows.Forms.SplitContainer();
            infoPanel = new System.Windows.Forms.Panel();
            pnl_left = new DevExpress.XtraEditors.PanelControl();
            _lotteryDataGrid = new DevExpress.XtraGrid.GridControl();
            _lotteryDataGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
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
            roadBeadMainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)roadBeadSplitContainer).BeginInit();
            roadBeadSplitContainer.Panel1.SuspendLayout();
            roadBeadSplitContainer.Panel2.SuspendLayout();
            roadBeadSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)roadBeadTopSplitContainer).BeginInit();
            roadBeadTopSplitContainer.Panel1.SuspendLayout();
            roadBeadTopSplitContainer.Panel2.SuspendLayout();
            roadBeadTopSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)roadBeadBottomSplitContainer).BeginInit();
            roadBeadBottomSplitContainer.Panel1.SuspendLayout();
            roadBeadBottomSplitContainer.Panel2.SuspendLayout();
            roadBeadBottomSplitContainer.SuspendLayout();
            tabTrend.SuspendLayout();
            tabConsecutive.SuspendLayout();
            tabCountStats.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)countStatsGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)countStatsGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bottomSplitContainer).BeginInit();
            bottomSplitContainer.Panel2.SuspendLayout();
            bottomSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnl_left).BeginInit();
            pnl_left.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_lotteryDataGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_lotteryDataGridView).BeginInit();
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
            toolbarPanel.Size = new System.Drawing.Size(1798, 50);
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
            _mainTabControl.Location = new System.Drawing.Point(461, 52);
            _mainTabControl.Name = "_mainTabControl";
            _mainTabControl.Padding = new System.Windows.Forms.Padding(5);
            _mainTabControl.SelectedTabPage = tabRoadBead;
            _mainTabControl.Size = new System.Drawing.Size(1284, 950);
            _mainTabControl.TabIndex = 1;
            _mainTabControl.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { tabRoadBead, tabTrend, tabConsecutive, tabCountStats });
            // 
            // tabRoadBead
            // 
            tabRoadBead.Controls.Add(roadBeadMainPanel);
            tabRoadBead.Name = "tabRoadBead";
            tabRoadBead.Size = new System.Drawing.Size(1282, 924);
            tabRoadBead.Text = "路珠统计";
            // 
            // roadBeadMainPanel
            // 
            roadBeadMainPanel.Controls.Add(roadBeadSplitContainer);
            roadBeadMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            roadBeadMainPanel.Location = new System.Drawing.Point(0, 0);
            roadBeadMainPanel.Name = "roadBeadMainPanel";
            roadBeadMainPanel.Size = new System.Drawing.Size(1282, 924);
            roadBeadMainPanel.TabIndex = 0;
            // 
            // roadBeadSplitContainer
            // 
            roadBeadSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            roadBeadSplitContainer.Location = new System.Drawing.Point(0, 0);
            roadBeadSplitContainer.Name = "roadBeadSplitContainer";
            roadBeadSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // roadBeadSplitContainer.Panel1
            // 
            roadBeadSplitContainer.Panel1.Controls.Add(roadBeadTopSplitContainer);
            // 
            // roadBeadSplitContainer.Panel2
            // 
            roadBeadSplitContainer.Panel2.Controls.Add(roadBeadBottomSplitContainer);
            roadBeadSplitContainer.Size = new System.Drawing.Size(1282, 924);
            roadBeadSplitContainer.SplitterDistance = 462;
            roadBeadSplitContainer.TabIndex = 0;
            // 
            // roadBeadTopSplitContainer
            // 
            roadBeadTopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            roadBeadTopSplitContainer.Location = new System.Drawing.Point(0, 0);
            roadBeadTopSplitContainer.Name = "roadBeadTopSplitContainer";
            // 
            // roadBeadTopSplitContainer.Panel1
            // 
            roadBeadTopSplitContainer.Panel1.AutoScroll = true;
            roadBeadTopSplitContainer.Panel1.BackColor = System.Drawing.Color.White;
            roadBeadTopSplitContainer.Panel1.Controls.Add(roadBeadSizeControl);
            roadBeadTopSplitContainer.Panel1.Controls.Add(roadBeadSizeLabel);
            // 
            // roadBeadTopSplitContainer.Panel2
            // 
            roadBeadTopSplitContainer.Panel2.AutoScroll = true;
            roadBeadTopSplitContainer.Panel2.BackColor = System.Drawing.Color.White;
            roadBeadTopSplitContainer.Panel2.Controls.Add(roadBeadOddEvenControl);
            roadBeadTopSplitContainer.Panel2.Controls.Add(roadBeadOddEvenLabel);
            roadBeadTopSplitContainer.Size = new System.Drawing.Size(1282, 462);
            roadBeadTopSplitContainer.SplitterDistance = 641;
            roadBeadTopSplitContainer.TabIndex = 0;
            // 
            // roadBeadSizeControl
            // 
            roadBeadSizeControl.BackColor = System.Drawing.Color.White;
            roadBeadSizeControl.CellSize = 20;
            roadBeadSizeControl.Location = new System.Drawing.Point(0, 25);
            roadBeadSizeControl.Name = "roadBeadSizeControl";
            roadBeadSizeControl.Size = new System.Drawing.Size(100, 100);
            roadBeadSizeControl.TabIndex = 0;
            // 
            // roadBeadSizeLabel
            // 
            roadBeadSizeLabel.AutoSize = true;
            roadBeadSizeLabel.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            roadBeadSizeLabel.Location = new System.Drawing.Point(5, 5);
            roadBeadSizeLabel.Name = "roadBeadSizeLabel";
            roadBeadSizeLabel.Size = new System.Drawing.Size(37, 19);
            roadBeadSizeLabel.TabIndex = 1;
            roadBeadSizeLabel.Text = "大小";
            // 
            // roadBeadOddEvenControl
            // 
            roadBeadOddEvenControl.BackColor = System.Drawing.Color.White;
            roadBeadOddEvenControl.CellSize = 20;
            roadBeadOddEvenControl.Location = new System.Drawing.Point(0, 25);
            roadBeadOddEvenControl.Name = "roadBeadOddEvenControl";
            roadBeadOddEvenControl.Size = new System.Drawing.Size(100, 100);
            roadBeadOddEvenControl.TabIndex = 0;
            // 
            // roadBeadOddEvenLabel
            // 
            roadBeadOddEvenLabel.AutoSize = true;
            roadBeadOddEvenLabel.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            roadBeadOddEvenLabel.Location = new System.Drawing.Point(5, 5);
            roadBeadOddEvenLabel.Name = "roadBeadOddEvenLabel";
            roadBeadOddEvenLabel.Size = new System.Drawing.Size(37, 19);
            roadBeadOddEvenLabel.TabIndex = 1;
            roadBeadOddEvenLabel.Text = "单双";
            // 
            // roadBeadBottomSplitContainer
            // 
            roadBeadBottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            roadBeadBottomSplitContainer.Location = new System.Drawing.Point(0, 0);
            roadBeadBottomSplitContainer.Name = "roadBeadBottomSplitContainer";
            // 
            // roadBeadBottomSplitContainer.Panel1
            // 
            roadBeadBottomSplitContainer.Panel1.AutoScroll = true;
            roadBeadBottomSplitContainer.Panel1.BackColor = System.Drawing.Color.White;
            roadBeadBottomSplitContainer.Panel1.Controls.Add(roadBeadTailSizeControl);
            roadBeadBottomSplitContainer.Panel1.Controls.Add(roadBeadTailSizeLabel);
            // 
            // roadBeadBottomSplitContainer.Panel2
            // 
            roadBeadBottomSplitContainer.Panel2.AutoScroll = true;
            roadBeadBottomSplitContainer.Panel2.BackColor = System.Drawing.Color.White;
            roadBeadBottomSplitContainer.Panel2.Controls.Add(roadBeadSumOddEvenControl);
            roadBeadBottomSplitContainer.Panel2.Controls.Add(roadBeadSumOddEvenLabel);
            roadBeadBottomSplitContainer.Size = new System.Drawing.Size(1282, 458);
            roadBeadBottomSplitContainer.SplitterDistance = 641;
            roadBeadBottomSplitContainer.TabIndex = 0;
            // 
            // roadBeadTailSizeControl
            // 
            roadBeadTailSizeControl.BackColor = System.Drawing.Color.White;
            roadBeadTailSizeControl.CellSize = 20;
            roadBeadTailSizeControl.Location = new System.Drawing.Point(0, 25);
            roadBeadTailSizeControl.Name = "roadBeadTailSizeControl";
            roadBeadTailSizeControl.Size = new System.Drawing.Size(100, 100);
            roadBeadTailSizeControl.TabIndex = 0;
            // 
            // roadBeadTailSizeLabel
            // 
            roadBeadTailSizeLabel.AutoSize = true;
            roadBeadTailSizeLabel.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            roadBeadTailSizeLabel.Location = new System.Drawing.Point(5, 5);
            roadBeadTailSizeLabel.Name = "roadBeadTailSizeLabel";
            roadBeadTailSizeLabel.Size = new System.Drawing.Size(37, 19);
            roadBeadTailSizeLabel.TabIndex = 1;
            roadBeadTailSizeLabel.Text = "尾大小";
            // 
            // roadBeadSumOddEvenControl
            // 
            roadBeadSumOddEvenControl.BackColor = System.Drawing.Color.White;
            roadBeadSumOddEvenControl.CellSize = 20;
            roadBeadSumOddEvenControl.Location = new System.Drawing.Point(0, 25);
            roadBeadSumOddEvenControl.Name = "roadBeadSumOddEvenControl";
            roadBeadSumOddEvenControl.Size = new System.Drawing.Size(100, 100);
            roadBeadSumOddEvenControl.TabIndex = 0;
            // 
            // roadBeadSumOddEvenLabel
            // 
            roadBeadSumOddEvenLabel.AutoSize = true;
            roadBeadSumOddEvenLabel.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            roadBeadSumOddEvenLabel.Location = new System.Drawing.Point(5, 5);
            roadBeadSumOddEvenLabel.Name = "roadBeadSumOddEvenLabel";
            roadBeadSumOddEvenLabel.Size = new System.Drawing.Size(37, 19);
            roadBeadSumOddEvenLabel.TabIndex = 1;
            roadBeadSumOddEvenLabel.Text = "和单双";
            // 
            // tabTrend
            // 
            tabTrend.Controls.Add(trendChartControl);
            tabTrend.Name = "tabTrend";
            tabTrend.Size = new System.Drawing.Size(1282, 924);
            tabTrend.Text = "走势图";
            // 
            // trendChartControl
            // 
            trendChartControl.BackColor = System.Drawing.Color.White;
            trendChartControl.Dock = System.Windows.Forms.DockStyle.Fill;
            trendChartControl.Location = new System.Drawing.Point(0, 0);
            trendChartControl.Name = "trendChartControl";
            trendChartControl.PlayType = BaiShengVx3Plus.Shared.Models.Games.Binggo.Statistics.GamePlayType.Size;
            trendChartControl.Size = new System.Drawing.Size(1282, 924);
            trendChartControl.TabIndex = 0;
            // 
            // tabConsecutive
            // 
            tabConsecutive.Controls.Add(consecutiveStatsControl);
            tabConsecutive.Name = "tabConsecutive";
            tabConsecutive.Size = new System.Drawing.Size(1282, 924);
            tabConsecutive.Text = "连续统计";
            // 
            // consecutiveStatsControl
            // 
            consecutiveStatsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            consecutiveStatsControl.Location = new System.Drawing.Point(0, 0);
            consecutiveStatsControl.Name = "consecutiveStatsControl";
            consecutiveStatsControl.Size = new System.Drawing.Size(1282, 924);
            consecutiveStatsControl.TabIndex = 0;
            // 
            // tabCountStats
            // 
            tabCountStats.Controls.Add(countStatsGrid);
            tabCountStats.Name = "tabCountStats";
            tabCountStats.Size = new System.Drawing.Size(1282, 924);
            tabCountStats.Text = "数量统计";
            // 
            // countStatsGrid
            // 
            countStatsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            countStatsGrid.Location = new System.Drawing.Point(0, 0);
            countStatsGrid.MainView = countStatsGridView;
            countStatsGrid.Name = "countStatsGrid";
            countStatsGrid.Size = new System.Drawing.Size(1282, 924);
            countStatsGrid.TabIndex = 0;
            countStatsGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { countStatsGridView });
            // 
            // countStatsGridView
            // 
            countStatsGridView.GridControl = countStatsGrid;
            countStatsGridView.Name = "countStatsGridView";
            // 
            // bottomSplitContainer
            // 
            bottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            bottomSplitContainer.Location = new System.Drawing.Point(0, 0);
            bottomSplitContainer.Name = "bottomSplitContainer";
            bottomSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // bottomSplitContainer.Panel2
            // 
            bottomSplitContainer.Panel2.Controls.Add(infoPanel);
            bottomSplitContainer.Size = new System.Drawing.Size(1282, 519);
            bottomSplitContainer.SplitterDistance = 201;
            bottomSplitContainer.TabIndex = 0;
            // 
            // infoPanel
            // 
            infoPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            infoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            infoPanel.Location = new System.Drawing.Point(0, 0);
            infoPanel.Name = "infoPanel";
            infoPanel.Size = new System.Drawing.Size(1282, 314);
            infoPanel.TabIndex = 0;
            // 
            // pnl_left
            // 
            pnl_left.Controls.Add(_lotteryDataGrid);
            pnl_left.Dock = System.Windows.Forms.DockStyle.Left;
            pnl_left.Location = new System.Drawing.Point(0, 50);
            pnl_left.Name = "pnl_left";
            pnl_left.Size = new System.Drawing.Size(456, 950);
            pnl_left.TabIndex = 3;
            // 
            // _lotteryDataGrid
            // 
            _lotteryDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            _lotteryDataGrid.Location = new System.Drawing.Point(2, 2);
            _lotteryDataGrid.MainView = _lotteryDataGridView;
            _lotteryDataGrid.Name = "_lotteryDataGrid";
            _lotteryDataGrid.Size = new System.Drawing.Size(452, 946);
            _lotteryDataGrid.TabIndex = 3;
            _lotteryDataGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { _lotteryDataGridView });
            // 
            // _lotteryDataGridView
            // 
            _lotteryDataGridView.GridControl = _lotteryDataGrid;
            _lotteryDataGridView.Name = "_lotteryDataGridView";
            _lotteryDataGridView.OptionsBehavior.Editable = false;
            _lotteryDataGridView.OptionsView.ShowGroupPanel = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1798, 1000);
            Controls.Add(pnl_left);
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
            roadBeadMainPanel.ResumeLayout(false);
            roadBeadSplitContainer.Panel1.ResumeLayout(false);
            roadBeadSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)roadBeadSplitContainer).EndInit();
            roadBeadSplitContainer.ResumeLayout(false);
            roadBeadTopSplitContainer.Panel1.ResumeLayout(false);
            roadBeadTopSplitContainer.Panel1.PerformLayout();
            roadBeadTopSplitContainer.Panel2.ResumeLayout(false);
            roadBeadTopSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)roadBeadTopSplitContainer).EndInit();
            roadBeadTopSplitContainer.ResumeLayout(false);
            roadBeadBottomSplitContainer.Panel1.ResumeLayout(false);
            roadBeadBottomSplitContainer.Panel1.PerformLayout();
            roadBeadBottomSplitContainer.Panel2.ResumeLayout(false);
            roadBeadBottomSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)roadBeadBottomSplitContainer).EndInit();
            roadBeadBottomSplitContainer.ResumeLayout(false);
            tabTrend.ResumeLayout(false);
            tabConsecutive.ResumeLayout(false);
            tabCountStats.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)countStatsGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)countStatsGridView).EndInit();
            bottomSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bottomSplitContainer).EndInit();
            bottomSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnl_left).EndInit();
            pnl_left.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_lotteryDataGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)_lotteryDataGridView).EndInit();
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
        private System.Windows.Forms.Panel roadBeadMainPanel;
        private System.Windows.Forms.SplitContainer roadBeadSplitContainer;
        private System.Windows.Forms.SplitContainer roadBeadTopSplitContainer;
        private System.Windows.Forms.SplitContainer roadBeadBottomSplitContainer;
        private BinGoPlans.Controls.RoadBeadControl roadBeadSizeControl;
        private System.Windows.Forms.Label roadBeadSizeLabel;
        private BinGoPlans.Controls.RoadBeadControl roadBeadOddEvenControl;
        private System.Windows.Forms.Label roadBeadOddEvenLabel;
        private BinGoPlans.Controls.RoadBeadControl roadBeadTailSizeControl;
        private System.Windows.Forms.Label roadBeadTailSizeLabel;
        private BinGoPlans.Controls.RoadBeadControl roadBeadSumOddEvenControl;
        private System.Windows.Forms.Label roadBeadSumOddEvenLabel;
        private System.Windows.Forms.SplitContainer bottomSplitContainer;
        private System.Windows.Forms.Panel infoPanel;
        private DevExpress.XtraTab.XtraTabPage tabTrend;
        private BinGoPlans.Controls.TrendChartControl trendChartControl;
        private DevExpress.XtraTab.XtraTabPage tabConsecutive;
        private BinGoPlans.Controls.ConsecutiveStatsControl consecutiveStatsControl;
        private DevExpress.XtraTab.XtraTabPage tabCountStats;
        private DevExpress.XtraGrid.GridControl countStatsGrid;
        private DevExpress.XtraGrid.Views.Grid.GridView countStatsGridView;
        private DevExpress.XtraEditors.PanelControl pnl_left;
        private DevExpress.XtraGrid.GridControl _lotteryDataGrid;
        private DevExpress.XtraGrid.Views.Grid.GridView _lotteryDataGridView;
    }
}
