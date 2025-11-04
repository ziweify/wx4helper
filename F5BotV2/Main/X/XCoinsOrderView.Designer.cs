namespace F5BotV2.Main
{
    partial class XCoinsOrderView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgv_coins_order = new CCWin.SkinControl.SkinDataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_coins_order)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_coins_order
            // 
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.dgv_coins_order.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_coins_order.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgv_coins_order.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgv_coins_order.ColumnFont = null;
            this.dgv_coins_order.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(246)))), ((int)(((byte)(239)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_coins_order.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgv_coins_order.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_coins_order.ColumnSelectForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(188)))), ((int)(((byte)(240)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_coins_order.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgv_coins_order.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_coins_order.EnableHeadersVisualStyles = false;
            this.dgv_coins_order.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgv_coins_order.HeadFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgv_coins_order.HeadSelectForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgv_coins_order.Location = new System.Drawing.Point(4, 28);
            this.dgv_coins_order.Name = "dgv_coins_order";
            this.dgv_coins_order.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgv_coins_order.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgv_coins_order.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgv_coins_order.RowTemplate.Height = 23;
            this.dgv_coins_order.Size = new System.Drawing.Size(614, 700);
            this.dgv_coins_order.TabIndex = 0;
            this.dgv_coins_order.TitleBack = null;
            this.dgv_coins_order.TitleBackColorBegin = System.Drawing.Color.White;
            this.dgv_coins_order.TitleBackColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(196)))), ((int)(((byte)(242)))));
            this.dgv_coins_order.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_coins_order_CellContentClick);
            this.dgv_coins_order.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_coins_order_CellMouseDown);
            this.dgv_coins_order.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgv_coins_order_CellPainting);
            // 
            // XCoinsOrderView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gold;
            this.ClientSize = new System.Drawing.Size(622, 732);
            this.ControlBoxActive = System.Drawing.Color.Gold;
            this.ControlBoxDeactive = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.Controls.Add(this.dgv_coins_order);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "XCoinsOrderView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "上下分";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CoinsOrder_FormClosing);
            this.Load += new System.EventHandler(this.CoinsOrder_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_coins_order)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinDataGridView dgv_coins_order;
    }
}