using Sunny.UI;

namespace BaiShengVx3Plus.Views
{
    partial class CreditWithdrawManageForm
    {
        private System.ComponentModel.IContainer components = null;
        private UIDataGridView dgvRequests;
        private UIPanel pnlTop;
        private UIComboBox cmbStatus;
        private UIButton btnRefresh;
        private UILabel lblStats;
        private UILabel lblStatusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvRequests = new Sunny.UI.UIDataGridView();
            this.pnlTop = new Sunny.UI.UIPanel();
            this.lblStatusLabel = new Sunny.UI.UILabel();
            this.cmbStatus = new Sunny.UI.UIComboBox();
            this.btnRefresh = new Sunny.UI.UIButton();
            this.lblStats = new Sunny.UI.UILabel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRequests)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvRequests
            // 
            this.dgvRequests.AllowUserToAddRows = false;
            this.dgvRequests.AllowUserToDeleteRows = false;
            this.dgvRequests.AllowUserToResizeRows = false;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(235, 243, 255);
            this.dgvRequests.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvRequests.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvRequests.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvRequests.BackgroundColor = System.Drawing.Color.White;
            this.dgvRequests.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRequests.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvRequests.ColumnHeadersHeight = 32;
            this.dgvRequests.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvRequests.EnableHeadersVisualStyles = false;
            this.dgvRequests.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.dgvRequests.GridColor = System.Drawing.Color.FromArgb(80, 160, 255);
            this.dgvRequests.Location = new System.Drawing.Point(12, 100);
            this.dgvRequests.MultiSelect = false;
            this.dgvRequests.Name = "dgvRequests";
            this.dgvRequests.ReadOnly = true;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            this.dgvRequests.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvRequests.RowHeadersVisible = false;
            this.dgvRequests.RowTemplate.Height = 29;
            this.dgvRequests.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRequests.Size = new System.Drawing.Size(960, 450);
            this.dgvRequests.TabIndex = 0;
            // 
            // pnlTop
            // 
            this.pnlTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTop.Controls.Add(this.lblStatusLabel);
            this.pnlTop.Controls.Add(this.cmbStatus);
            this.pnlTop.Controls.Add(this.btnRefresh);
            this.pnlTop.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.pnlTop.Location = new System.Drawing.Point(12, 35);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(960, 60);
            this.pnlTop.TabIndex = 1;
            this.pnlTop.Text = null;
            this.pnlTop.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatusLabel
            // 
            this.lblStatusLabel.AutoSize = true;
            this.lblStatusLabel.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblStatusLabel.Location = new System.Drawing.Point(10, 20);
            this.lblStatusLabel.Name = "lblStatusLabel";
            this.lblStatusLabel.Size = new System.Drawing.Size(65, 20);
            this.lblStatusLabel.TabIndex = 0;
            this.lblStatusLabel.Text = "状态：";
            this.lblStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbStatus
            // 
            this.cmbStatus.DataSource = null;
            this.cmbStatus.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbStatus.FillColor = System.Drawing.Color.White;
            this.cmbStatus.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbStatus.Location = new System.Drawing.Point(80, 15);
            this.cmbStatus.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbStatus.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbStatus.Size = new System.Drawing.Size(150, 30);
            this.cmbStatus.TabIndex = 1;
            this.cmbStatus.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbStatus.Watermark = "";
            this.cmbStatus.SelectedIndexChanged += new System.EventHandler(this.CmbStatus_SelectedIndexChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnRefresh.Location = new System.Drawing.Point(250, 15);
            this.btnRefresh.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(80, 30);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.TipsFont = new System.Drawing.Font("微软雅黑", 9F);
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // lblStats
            // 
            this.lblStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStats.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblStats.Location = new System.Drawing.Point(12, 560);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(960, 30);
            this.lblStats.TabIndex = 2;
            this.lblStats.Text = "待处理: 0 笔 | 今日上分: 0.00 | 今日下分: 0.00";
            this.lblStats.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CreditWithdrawManageForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(984, 601);
            this.Controls.Add(this.lblStats);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.dgvRequests);
            this.Name = "CreditWithdrawManageForm";
            this.Text = "上下分管理";
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 800, 450);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRequests)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}

