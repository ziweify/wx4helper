namespace BaiShengVx3Plus.Views
{
    partial class LogViewerForm
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            pnlTop = new Sunny.UI.UIPanel();
            btnExport = new Sunny.UI.UIButton();
            btnClearDatabase = new Sunny.UI.UIButton();
            btnClear = new Sunny.UI.UIButton();
            btnRefresh = new Sunny.UI.UIButton();
            btnQuery = new Sunny.UI.UIButton();
            cmbLevel = new Sunny.UI.UIComboBox();
            uiLabel2 = new Sunny.UI.UILabel();
            cmbSource = new Sunny.UI.UIComboBox();
            uiLabel1 = new Sunny.UI.UILabel();
            txtKeyword = new Sunny.UI.UITextBox();
            uiLabel3 = new Sunny.UI.UILabel();
            chkAutoScroll = new Sunny.UI.UICheckBox();
            pnlBottom = new Sunny.UI.UIPanel();
            lblStatistics = new Sunny.UI.UILabel();
            dgvLogs = new Sunny.UI.UIDataGridView();
            colTime = new DataGridViewTextBoxColumn();
            colLevel = new DataGridViewTextBoxColumn();
            colSource = new DataGridViewTextBoxColumn();
            colMessage = new DataGridViewTextBoxColumn();
            colThreadId = new DataGridViewTextBoxColumn();
            pnlTop.SuspendLayout();
            pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLogs).BeginInit();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(btnExport);
            pnlTop.Controls.Add(btnClearDatabase);
            pnlTop.Controls.Add(btnClear);
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Controls.Add(btnQuery);
            pnlTop.Controls.Add(cmbLevel);
            pnlTop.Controls.Add(uiLabel2);
            pnlTop.Controls.Add(cmbSource);
            pnlTop.Controls.Add(uiLabel1);
            pnlTop.Controls.Add(txtKeyword);
            pnlTop.Controls.Add(uiLabel3);
            pnlTop.Controls.Add(chkAutoScroll);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Font = new Font("微软雅黑", 12F);
            pnlTop.Location = new Point(0, 35);
            pnlTop.Margin = new Padding(4, 5, 4, 5);
            pnlTop.MinimumSize = new Size(1, 1);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1200, 80);
            pnlTop.TabIndex = 0;
            pnlTop.Text = null;
            pnlTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnExport
            // 
            btnExport.Cursor = Cursors.Hand;
            btnExport.Font = new Font("微软雅黑", 10F);
            btnExport.Location = new Point(950, 42);
            btnExport.MinimumSize = new Size(1, 1);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(90, 30);
            btnExport.TabIndex = 12;
            btnExport.Text = "导出日志";
            btnExport.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnExport.Click += btnExport_Click;
            // 
            // btnClearDatabase
            // 
            btnClearDatabase.Cursor = Cursors.Hand;
            btnClearDatabase.Font = new Font("微软雅黑", 10F);
            btnClearDatabase.Location = new Point(1050, 42);
            btnClearDatabase.MinimumSize = new Size(1, 1);
            btnClearDatabase.Name = "btnClearDatabase";
            btnClearDatabase.Size = new Size(140, 30);
            btnClearDatabase.TabIndex = 11;
            btnClearDatabase.Text = "清空数据库日志";
            btnClearDatabase.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClearDatabase.Click += btnClearDatabase_Click;
            // 
            // btnClear
            // 
            btnClear.Cursor = Cursors.Hand;
            btnClear.Font = new Font("微软雅黑", 10F);
            btnClear.Location = new Point(840, 42);
            btnClear.MinimumSize = new Size(1, 1);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(100, 30);
            btnClear.TabIndex = 10;
            btnClear.Text = "清空显示";
            btnClear.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClear.Click += btnClear_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Font = new Font("微软雅黑", 10F);
            btnRefresh.Location = new Point(750, 42);
            btnRefresh.MinimumSize = new Size(1, 1);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(80, 30);
            btnRefresh.TabIndex = 9;
            btnRefresh.Text = "刷新";
            btnRefresh.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnQuery
            // 
            btnQuery.Cursor = Cursors.Hand;
            btnQuery.Font = new Font("微软雅黑", 10F);
            btnQuery.Location = new Point(660, 42);
            btnQuery.MinimumSize = new Size(1, 1);
            btnQuery.Name = "btnQuery";
            btnQuery.Size = new Size(80, 30);
            btnQuery.TabIndex = 8;
            btnQuery.Text = "查询";
            btnQuery.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnQuery.Click += btnQuery_Click;
            // 
            // cmbLevel
            // 
            cmbLevel.DataSource = null;
            cmbLevel.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            cmbLevel.FillColor = Color.White;
            cmbLevel.Font = new Font("微软雅黑", 10F);
            cmbLevel.ItemHoverColor = Color.FromArgb(155, 200, 255);
            cmbLevel.Items.AddRange(new object[] { "全部", "跟踪", "调试", "信息", "警告", "错误", "致命" });
            cmbLevel.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            cmbLevel.Location = new Point(520, 42);
            cmbLevel.Margin = new Padding(4, 5, 4, 5);
            cmbLevel.MinimumSize = new Size(63, 0);
            cmbLevel.Name = "cmbLevel";
            cmbLevel.Padding = new Padding(0, 0, 30, 2);
            cmbLevel.Size = new Size(130, 30);
            cmbLevel.TabIndex = 7;
            cmbLevel.Text = "全部";
            cmbLevel.TextAlignment = ContentAlignment.MiddleLeft;
            cmbLevel.Watermark = "";
            cmbLevel.SelectedIndexChanged += cmbLevel_SelectedIndexChanged;
            // 
            // uiLabel2
            // 
            uiLabel2.Font = new Font("微软雅黑", 10F);
            uiLabel2.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel2.Location = new Point(450, 42);
            uiLabel2.Name = "uiLabel2";
            uiLabel2.Size = new Size(70, 30);
            uiLabel2.TabIndex = 6;
            uiLabel2.Text = "级别:";
            uiLabel2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbSource
            // 
            cmbSource.DataSource = null;
            cmbSource.FillColor = Color.White;
            cmbSource.Font = new Font("微软雅黑", 10F);
            cmbSource.ItemHoverColor = Color.FromArgb(155, 200, 255);
            cmbSource.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            cmbSource.Location = new Point(300, 42);
            cmbSource.Margin = new Padding(4, 5, 4, 5);
            cmbSource.MinimumSize = new Size(63, 0);
            cmbSource.Name = "cmbSource";
            cmbSource.Padding = new Padding(0, 0, 30, 2);
            cmbSource.Size = new Size(140, 30);
            cmbSource.TabIndex = 5;
            cmbSource.TextAlignment = ContentAlignment.MiddleLeft;
            cmbSource.Watermark = "全部";
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("微软雅黑", 10F);
            uiLabel1.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel1.Location = new Point(230, 42);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(70, 30);
            uiLabel1.TabIndex = 4;
            uiLabel1.Text = "来源:";
            uiLabel1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtKeyword
            // 
            txtKeyword.Font = new Font("微软雅黑", 10F);
            txtKeyword.Location = new Point(90, 42);
            txtKeyword.Margin = new Padding(4, 5, 4, 5);
            txtKeyword.MinimumSize = new Size(1, 16);
            txtKeyword.Name = "txtKeyword";
            txtKeyword.Padding = new Padding(5);
            txtKeyword.ShowText = false;
            txtKeyword.Size = new Size(130, 30);
            txtKeyword.TabIndex = 3;
            txtKeyword.TextAlignment = ContentAlignment.MiddleLeft;
            txtKeyword.Watermark = "关键词";
            // 
            // uiLabel3
            // 
            uiLabel3.Font = new Font("微软雅黑", 10F);
            uiLabel3.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel3.Location = new Point(10, 42);
            uiLabel3.Name = "uiLabel3";
            uiLabel3.Size = new Size(80, 30);
            uiLabel3.TabIndex = 2;
            uiLabel3.Text = "关键词:";
            uiLabel3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkAutoScroll
            // 
            chkAutoScroll.Checked = true;
            chkAutoScroll.Font = new Font("微软雅黑", 10F);
            chkAutoScroll.ForeColor = Color.FromArgb(48, 48, 48);
            chkAutoScroll.Location = new Point(10, 10);
            chkAutoScroll.MinimumSize = new Size(1, 1);
            chkAutoScroll.Name = "chkAutoScroll";
            chkAutoScroll.Size = new Size(150, 25);
            chkAutoScroll.TabIndex = 1;
            chkAutoScroll.Text = "自动滚动";
            // 
            // pnlBottom
            // 
            pnlBottom.Controls.Add(lblStatistics);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Font = new Font("微软雅黑", 12F);
            pnlBottom.Location = new Point(0, 670);
            pnlBottom.Margin = new Padding(4, 5, 4, 5);
            pnlBottom.MinimumSize = new Size(1, 1);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1200, 30);
            pnlBottom.TabIndex = 1;
            pnlBottom.Text = null;
            pnlBottom.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // lblStatistics
            // 
            lblStatistics.Dock = DockStyle.Fill;
            lblStatistics.Font = new Font("微软雅黑", 10F);
            lblStatistics.ForeColor = Color.FromArgb(48, 48, 48);
            lblStatistics.Location = new Point(0, 0);
            lblStatistics.Name = "lblStatistics";
            lblStatistics.Padding = new Padding(10, 0, 0, 0);
            lblStatistics.Size = new Size(1200, 30);
            lblStatistics.TabIndex = 0;
            lblStatistics.Text = "总计: 0 | 错误: 0 | 警告: 0 | 信息: 0";
            lblStatistics.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgvLogs
            // 
            dgvLogs.AllowUserToAddRows = false;
            dgvLogs.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            dgvLogs.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLogs.BackgroundColor = Color.White;
            dgvLogs.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvLogs.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvLogs.ColumnHeadersHeight = 32;
            dgvLogs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvLogs.Columns.AddRange(new DataGridViewColumn[] { colTime, colLevel, colSource, colMessage, colThreadId });
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Window;
            dataGridViewCellStyle3.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvLogs.DefaultCellStyle = dataGridViewCellStyle3;
            dgvLogs.Dock = DockStyle.Fill;
            dgvLogs.EnableHeadersVisualStyles = false;
            dgvLogs.Font = new Font("微软雅黑", 12F);
            dgvLogs.GridColor = Color.FromArgb(80, 160, 255);
            dgvLogs.Location = new Point(0, 115);
            dgvLogs.Name = "dgvLogs";
            dgvLogs.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle4.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.White;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgvLogs.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvLogs.RowHeadersVisible = false;
            dgvLogs.RowTemplate.Height = 25;
            dgvLogs.SelectedIndex = -1;
            dgvLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLogs.Size = new Size(1200, 555);
            dgvLogs.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvLogs.TabIndex = 2;
            // 
            // colTime
            // 
            colTime.FillWeight = 20F;
            colTime.HeaderText = "时间";
            colTime.Name = "colTime";
            colTime.ReadOnly = true;
            // 
            // colLevel
            // 
            colLevel.FillWeight = 10F;
            colLevel.HeaderText = "级别";
            colLevel.Name = "colLevel";
            colLevel.ReadOnly = true;
            // 
            // colSource
            // 
            colSource.FillWeight = 20F;
            colSource.HeaderText = "来源";
            colSource.Name = "colSource";
            colSource.ReadOnly = true;
            // 
            // colMessage
            // 
            colMessage.FillWeight = 45F;
            colMessage.HeaderText = "消息";
            colMessage.Name = "colMessage";
            colMessage.ReadOnly = true;
            // 
            // colThreadId
            // 
            colThreadId.FillWeight = 10F;
            colThreadId.HeaderText = "线程";
            colThreadId.Name = "colThreadId";
            colThreadId.ReadOnly = true;
            // 
            // LogViewerForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1200, 700);
            Controls.Add(dgvLogs);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LogViewerForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "系统日志";
            ZoomScaleRect = new Rectangle(15, 15, 800, 450);
            pnlTop.ResumeLayout(false);
            pnlBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLogs).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UIPanel pnlTop;
        private Sunny.UI.UIPanel pnlBottom;
        private Sunny.UI.UIDataGridView dgvLogs;
        private Sunny.UI.UICheckBox chkAutoScroll;
        private Sunny.UI.UILabel lblStatistics;
        private Sunny.UI.UITextBox txtKeyword;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UIComboBox cmbLevel;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UIComboBox cmbSource;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIButton btnQuery;
        private Sunny.UI.UIButton btnRefresh;
        private Sunny.UI.UIButton btnClear;
        private Sunny.UI.UIButton btnClearDatabase;
        private Sunny.UI.UIButton btnExport;
        private DataGridViewTextBoxColumn colTime;
        private DataGridViewTextBoxColumn colLevel;
        private DataGridViewTextBoxColumn colSource;
        private DataGridViewTextBoxColumn colMessage;
        private DataGridViewTextBoxColumn colThreadId;
    }
}

