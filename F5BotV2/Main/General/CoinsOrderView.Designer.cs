namespace F5BotV2.Main
{
    partial class CoinsOrderView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgv_coins_order = new CCWin.SkinControl.SkinDataGridView();
            this.cms_coins_order = new CCWin.SkinControl.SkinContextMenuStrip();
            this.刷新ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.同意ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.忽略ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_coins_order)).BeginInit();
            this.cms_coins_order.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgv_coins_order
            // 
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.dgv_coins_order.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgv_coins_order.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgv_coins_order.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgv_coins_order.ColumnFont = null;
            this.dgv_coins_order.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(246)))), ((int)(((byte)(239)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_coins_order.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dgv_coins_order.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_coins_order.ColumnSelectForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgv_coins_order.ContextMenuStrip = this.cms_coins_order;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(188)))), ((int)(((byte)(240)))));
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_coins_order.DefaultCellStyle = dataGridViewCellStyle7;
            this.dgv_coins_order.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_coins_order.EnableHeadersVisualStyles = false;
            this.dgv_coins_order.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgv_coins_order.HeadFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgv_coins_order.HeadSelectForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgv_coins_order.Location = new System.Drawing.Point(4, 28);
            this.dgv_coins_order.Name = "dgv_coins_order";
            this.dgv_coins_order.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgv_coins_order.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgv_coins_order.RowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dgv_coins_order.RowTemplate.Height = 23;
            this.dgv_coins_order.Size = new System.Drawing.Size(494, 583);
            this.dgv_coins_order.TabIndex = 0;
            this.dgv_coins_order.TitleBack = null;
            this.dgv_coins_order.TitleBackColorBegin = System.Drawing.Color.White;
            this.dgv_coins_order.TitleBackColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(196)))), ((int)(((byte)(242)))));
            this.dgv_coins_order.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_coins_order_CellMouseDown);
            this.dgv_coins_order.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgv_coins_order_CellPainting);
            // 
            // cms_coins_order
            // 
            this.cms_coins_order.Arrow = System.Drawing.Color.Black;
            this.cms_coins_order.Back = System.Drawing.Color.White;
            this.cms_coins_order.BackRadius = 4;
            this.cms_coins_order.Base = System.Drawing.Color.FromArgb(((int)(((byte)(105)))), ((int)(((byte)(200)))), ((int)(((byte)(254)))));
            this.cms_coins_order.DropDownImageSeparator = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(197)))), ((int)(((byte)(197)))));
            this.cms_coins_order.Fore = System.Drawing.Color.Black;
            this.cms_coins_order.HoverFore = System.Drawing.Color.White;
            this.cms_coins_order.ItemAnamorphosis = true;
            this.cms_coins_order.ItemBorder = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(148)))), ((int)(((byte)(212)))));
            this.cms_coins_order.ItemBorderShow = true;
            this.cms_coins_order.ItemHover = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(148)))), ((int)(((byte)(212)))));
            this.cms_coins_order.ItemPressed = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(148)))), ((int)(((byte)(212)))));
            this.cms_coins_order.ItemRadius = 4;
            this.cms_coins_order.ItemRadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.cms_coins_order.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.刷新ToolStripMenuItem,
            this.同意ToolStripMenuItem,
            this.忽略ToolStripMenuItem,
            this.退出ToolStripMenuItem});
            this.cms_coins_order.ItemSplitter = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(148)))), ((int)(((byte)(212)))));
            this.cms_coins_order.Name = "cms_coins_order";
            this.cms_coins_order.RadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.cms_coins_order.Size = new System.Drawing.Size(101, 92);
            this.cms_coins_order.SkinAllColor = true;
            this.cms_coins_order.TitleAnamorphosis = true;
            this.cms_coins_order.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(228)))), ((int)(((byte)(236)))));
            this.cms_coins_order.TitleRadius = 4;
            this.cms_coins_order.TitleRadiusStyle = CCWin.SkinClass.RoundStyle.All;
            // 
            // 刷新ToolStripMenuItem
            // 
            this.刷新ToolStripMenuItem.Name = "刷新ToolStripMenuItem";
            this.刷新ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.刷新ToolStripMenuItem.Text = "刷新";
            this.刷新ToolStripMenuItem.Click += new System.EventHandler(this.刷新ToolStripMenuItem_Click);
            // 
            // 同意ToolStripMenuItem
            // 
            this.同意ToolStripMenuItem.Name = "同意ToolStripMenuItem";
            this.同意ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.同意ToolStripMenuItem.Text = "同意";
            this.同意ToolStripMenuItem.Click += new System.EventHandler(this.同意ToolStripMenuItem_Click);
            // 
            // 忽略ToolStripMenuItem
            // 
            this.忽略ToolStripMenuItem.Name = "忽略ToolStripMenuItem";
            this.忽略ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.忽略ToolStripMenuItem.Text = "忽略";
            this.忽略ToolStripMenuItem.Click += new System.EventHandler(this.忽略ToolStripMenuItem_Click);
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.退出ToolStripMenuItem.Text = "退出";
            this.退出ToolStripMenuItem.Click += new System.EventHandler(this.退出ToolStripMenuItem_Click);
            // 
            // CoinsOrderView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(502, 615);
            this.Controls.Add(this.dgv_coins_order);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "CoinsOrderView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "上下分信息";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CoinsOrder_FormClosing);
            this.Load += new System.EventHandler(this.CoinsOrder_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_coins_order)).EndInit();
            this.cms_coins_order.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinDataGridView dgv_coins_order;
        private CCWin.SkinControl.SkinContextMenuStrip cms_coins_order;
        private System.Windows.Forms.ToolStripMenuItem 刷新ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 同意ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 忽略ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 退出ToolStripMenuItem;
    }
}