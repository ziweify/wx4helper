using Sunny.UI;

namespace zhaocaimao.Views
{
    partial class BalanceChangeViewerForm
    {
        private System.ComponentModel.IContainer components = null;
        private UIDataGridView dgvBalanceChanges;
        private UIPanel pnlTop;
        private UITextBox txtSearch;
        private UIComboBox cmbReason;
        private UIButton btnSearch;
        private UIButton btnReset;
        private UILabel lblStats;
        private UILabel lblSearchLabel;
        private UILabel lblReasonLabel;

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
            this.dgvBalanceChanges = new Sunny.UI.UIDataGridView();
            this.pnlTop = new Sunny.UI.UIPanel();
            this.lblSearchLabel = new Sunny.UI.UILabel();
            this.txtSearch = new Sunny.UI.UITextBox();
            this.lblReasonLabel = new Sunny.UI.UILabel();
            this.cmbReason = new Sunny.UI.UIComboBox();
            this.btnSearch = new Sunny.UI.UIButton();
            this.btnReset = new Sunny.UI.UIButton();
            this.lblStats = new Sunny.UI.UILabel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBalanceChanges)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvBalanceChanges
            // 
            this.dgvBalanceChanges.AllowUserToAddRows = false;
            this.dgvBalanceChanges.AllowUserToDeleteRows = false;
            this.dgvBalanceChanges.AllowUserToResizeRows = false;
            this.dgvBalanceChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvBalanceChanges.BackgroundColor = System.Drawing.Color.White;
            this.dgvBalanceChanges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBalanceChanges.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.dgvBalanceChanges.Location = new System.Drawing.Point(12, 100);
            this.dgvBalanceChanges.MultiSelect = false;
            this.dgvBalanceChanges.Name = "dgvBalanceChanges";
            this.dgvBalanceChanges.ReadOnly = true;
            this.dgvBalanceChanges.RowHeadersVisible = false;
            this.dgvBalanceChanges.RowTemplate.Height = 29;
            this.dgvBalanceChanges.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBalanceChanges.Size = new System.Drawing.Size(1160, 510);
            this.dgvBalanceChanges.TabIndex = 0;
            // 
            // pnlTop
            // 
            this.pnlTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTop.Controls.Add(this.lblSearchLabel);
            this.pnlTop.Controls.Add(this.txtSearch);
            this.pnlTop.Controls.Add(this.lblReasonLabel);
            this.pnlTop.Controls.Add(this.cmbReason);
            this.pnlTop.Controls.Add(this.btnSearch);
            this.pnlTop.Controls.Add(this.btnReset);
            this.pnlTop.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.pnlTop.Location = new System.Drawing.Point(12, 35);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1160, 60);
            this.pnlTop.TabIndex = 1;
            this.pnlTop.Text = null;
            this.pnlTop.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblSearchLabel.Location = new System.Drawing.Point(10, 20);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(65, 20);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Text = "搜索：";
            this.lblSearchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSearch
            // 
            this.txtSearch.ButtonSymbol = 61761;
            this.txtSearch.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtSearch.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.txtSearch.Location = new System.Drawing.Point(80, 15);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtSearch.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.ShowText = false;
            this.txtSearch.Size = new System.Drawing.Size(250, 30);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtSearch.Watermark = "搜索会员名字或微信ID...";
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtSearch_KeyDown);
            // 
            // lblReasonLabel
            // 
            this.lblReasonLabel.AutoSize = true;
            this.lblReasonLabel.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblReasonLabel.Location = new System.Drawing.Point(350, 20);
            this.lblReasonLabel.Name = "lblReasonLabel";
            this.lblReasonLabel.Size = new System.Drawing.Size(93, 20);
            this.lblReasonLabel.TabIndex = 2;
            this.lblReasonLabel.Text = "变动原因：";
            this.lblReasonLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbReason
            // 
            this.cmbReason.DataSource = null;
            this.cmbReason.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cmbReason.FillColor = System.Drawing.Color.White;
            this.cmbReason.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cmbReason.Location = new System.Drawing.Point(450, 15);
            this.cmbReason.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbReason.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbReason.Name = "cmbReason";
            this.cmbReason.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbReason.Size = new System.Drawing.Size(150, 30);
            this.cmbReason.TabIndex = 3;
            this.cmbReason.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbReason.Watermark = "";
            this.cmbReason.SelectedIndexChanged += new System.EventHandler(this.CmbReason_SelectedIndexChanged);
            // 
            // btnSearch
            // 
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnSearch.Location = new System.Drawing.Point(620, 15);
            this.btnSearch.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 30);
            this.btnSearch.TabIndex = 4;
            this.btnSearch.Text = "搜索";
            this.btnSearch.TipsFont = new System.Drawing.Font("微软雅黑", 9F);
            this.btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // btnReset
            // 
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnReset.Location = new System.Drawing.Point(720, 15);
            this.btnReset.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(80, 30);
            this.btnReset.TabIndex = 5;
            this.btnReset.Text = "重置";
            this.btnReset.TipsFont = new System.Drawing.Font("微软雅黑", 9F);
            this.btnReset.Click += new System.EventHandler(this.BtnReset_Click);
            // 
            // lblStats
            // 
            this.lblStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStats.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblStats.Location = new System.Drawing.Point(12, 620);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(1160, 30);
            this.lblStats.TabIndex = 2;
            this.lblStats.Text = "共 0 条记录 | 增加: 0.00 | 减少: 0.00 | 净变化: 0.00";
            this.lblStats.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BalanceChangeViewerForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1184, 661);
            this.Controls.Add(this.lblStats);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.dgvBalanceChanges);
            this.Name = "BalanceChangeViewerForm";
            this.Text = "资金变动记录";
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 800, 450);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBalanceChanges)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}

