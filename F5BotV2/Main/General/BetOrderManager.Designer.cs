namespace F5BotV2.Main
{
    partial class BetOrderManager
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnl_top = new CCWin.SkinControl.SkinPanel();
            this.dgv_orders = new CCWin.SkinControl.SkinDataGridView();
            this.dpckSearch = new System.Windows.Forms.DateTimePicker();
            this.btnSearch = new CCWin.SkinControl.SkinButton();
            this.skinLabel1 = new System.Windows.Forms.Label();
            this.tbxIssueid = new CCWin.SkinControl.SkinTextBox();
            this.skinLabel2 = new System.Windows.Forms.Label();
            this.tbxGroupid = new CCWin.SkinControl.SkinTextBox();
            this.chkNotPay = new CCWin.SkinControl.SkinCheckBox();
            this.cmsOrdermanager = new CCWin.SkinControl.SkinContextMenuStrip();
            this.刷新ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.补分ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.全部补分ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnl_top.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_orders)).BeginInit();
            this.cmsOrdermanager.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnl_top
            // 
            this.pnl_top.BackColor = System.Drawing.Color.DarkCyan;
            this.pnl_top.Controls.Add(this.chkNotPay);
            this.pnl_top.Controls.Add(this.skinLabel2);
            this.pnl_top.Controls.Add(this.tbxGroupid);
            this.pnl_top.Controls.Add(this.tbxIssueid);
            this.pnl_top.Controls.Add(this.skinLabel1);
            this.pnl_top.Controls.Add(this.btnSearch);
            this.pnl_top.Controls.Add(this.dpckSearch);
            this.pnl_top.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.pnl_top.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl_top.DownBack = null;
            this.pnl_top.Location = new System.Drawing.Point(4, 28);
            this.pnl_top.MouseBack = null;
            this.pnl_top.Name = "pnl_top";
            this.pnl_top.NormlBack = null;
            this.pnl_top.Size = new System.Drawing.Size(1028, 43);
            this.pnl_top.TabIndex = 0;
            // 
            // dgv_orders
            // 
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.dgv_orders.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgv_orders.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgv_orders.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgv_orders.ColumnFont = null;
            this.dgv_orders.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(246)))), ((int)(((byte)(239)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_orders.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dgv_orders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_orders.ColumnSelectForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgv_orders.ContextMenuStrip = this.cmsOrdermanager;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(188)))), ((int)(((byte)(240)))));
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_orders.DefaultCellStyle = dataGridViewCellStyle7;
            this.dgv_orders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_orders.EnableHeadersVisualStyles = false;
            this.dgv_orders.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgv_orders.HeadFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgv_orders.HeadSelectForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgv_orders.Location = new System.Drawing.Point(4, 71);
            this.dgv_orders.Name = "dgv_orders";
            this.dgv_orders.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgv_orders.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgv_orders.RowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dgv_orders.RowTemplate.Height = 23;
            this.dgv_orders.Size = new System.Drawing.Size(1028, 699);
            this.dgv_orders.TabIndex = 1;
            this.dgv_orders.TitleBack = null;
            this.dgv_orders.TitleBackColorBegin = System.Drawing.Color.White;
            this.dgv_orders.TitleBackColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(196)))), ((int)(((byte)(242)))));
            this.dgv_orders.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_orders_CellMouseDown);
            // 
            // dpckSearch
            // 
            this.dpckSearch.Location = new System.Drawing.Point(3, 11);
            this.dpckSearch.Name = "dpckSearch";
            this.dpckSearch.Size = new System.Drawing.Size(140, 21);
            this.dpckSearch.TabIndex = 0;
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.Transparent;
            this.btnSearch.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnSearch.DownBack = null;
            this.btnSearch.Location = new System.Drawing.Point(558, 8);
            this.btnSearch.MouseBack = null;
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.NormlBack = null;
            this.btnSearch.Size = new System.Drawing.Size(86, 23);
            this.btnSearch.TabIndex = 1;
            this.btnSearch.Text = "查询";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // skinLabel1
            // 
            this.skinLabel1.AutoSize = true;
            this.skinLabel1.BackColor = System.Drawing.Color.Transparent;
           // this.skinLabel1.BorderColor = System.Drawing.Color.White;
            this.skinLabel1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel1.Location = new System.Drawing.Point(157, 14);
            this.skinLabel1.Name = "skinLabel1";
            this.skinLabel1.Size = new System.Drawing.Size(32, 17);
            this.skinLabel1.TabIndex = 2;
            this.skinLabel1.Text = "期号";
            // 
            // tbxIssueid
            // 
            this.tbxIssueid.BackColor = System.Drawing.Color.Transparent;
            this.tbxIssueid.DownBack = null;
            this.tbxIssueid.Icon = null;
            this.tbxIssueid.IconIsButton = false;
            this.tbxIssueid.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbxIssueid.IsPasswordChat = '\0';
            this.tbxIssueid.IsSystemPasswordChar = false;
            this.tbxIssueid.Lines = new string[0];
            this.tbxIssueid.Location = new System.Drawing.Point(192, 7);
            this.tbxIssueid.Margin = new System.Windows.Forms.Padding(0);
            this.tbxIssueid.MaxLength = 32767;
            this.tbxIssueid.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbxIssueid.MouseBack = null;
            this.tbxIssueid.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbxIssueid.Multiline = false;
            this.tbxIssueid.Name = "tbxIssueid";
            this.tbxIssueid.NormlBack = null;
            this.tbxIssueid.Padding = new System.Windows.Forms.Padding(5);
            this.tbxIssueid.ReadOnly = false;
            this.tbxIssueid.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbxIssueid.Size = new System.Drawing.Size(102, 28);
            // 
            // 
            // 
            this.tbxIssueid.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxIssueid.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxIssueid.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbxIssueid.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbxIssueid.SkinTxt.Name = "BaseText";
            this.tbxIssueid.SkinTxt.Size = new System.Drawing.Size(92, 18);
            this.tbxIssueid.SkinTxt.TabIndex = 0;
            this.tbxIssueid.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbxIssueid.SkinTxt.WaterText = "";
            this.tbxIssueid.TabIndex = 3;
            this.tbxIssueid.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxIssueid.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbxIssueid.WaterText = "";
            this.tbxIssueid.WordWrap = true;
            // 
            // skinLabel2
            // 
            this.skinLabel2.AutoSize = true;
            this.skinLabel2.BackColor = System.Drawing.Color.Transparent;
            //this.skinLabel2.BorderColor = System.Drawing.Color.White;
            this.skinLabel2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel2.Location = new System.Drawing.Point(306, 14);
            this.skinLabel2.Name = "skinLabel2";
            this.skinLabel2.Size = new System.Drawing.Size(34, 17);
            this.skinLabel2.TabIndex = 4;
            this.skinLabel2.Text = "群ID";
            // 
            // tbxGroupid
            // 
            this.tbxGroupid.BackColor = System.Drawing.Color.Transparent;
            this.tbxGroupid.DownBack = null;
            this.tbxGroupid.Icon = null;
            this.tbxGroupid.IconIsButton = false;
            this.tbxGroupid.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbxGroupid.IsPasswordChat = '\0';
            this.tbxGroupid.IsSystemPasswordChar = false;
            this.tbxGroupid.Lines = new string[0];
            this.tbxGroupid.Location = new System.Drawing.Point(342, 7);
            this.tbxGroupid.Margin = new System.Windows.Forms.Padding(0);
            this.tbxGroupid.MaxLength = 32767;
            this.tbxGroupid.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbxGroupid.MouseBack = null;
            this.tbxGroupid.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbxGroupid.Multiline = false;
            this.tbxGroupid.Name = "tbxGroupid";
            this.tbxGroupid.NormlBack = null;
            this.tbxGroupid.Padding = new System.Windows.Forms.Padding(5);
            this.tbxGroupid.ReadOnly = false;
            this.tbxGroupid.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbxGroupid.Size = new System.Drawing.Size(102, 28);
            // 
            // 
            // 
            this.tbxGroupid.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxGroupid.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxGroupid.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbxGroupid.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbxGroupid.SkinTxt.Name = "BaseText";
            this.tbxGroupid.SkinTxt.Size = new System.Drawing.Size(92, 18);
            this.tbxGroupid.SkinTxt.TabIndex = 0;
            this.tbxGroupid.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbxGroupid.SkinTxt.WaterText = "";
            this.tbxGroupid.TabIndex = 3;
            this.tbxGroupid.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxGroupid.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbxGroupid.WaterText = "";
            this.tbxGroupid.WordWrap = true;
            // 
            // chkNotPay
            // 
            this.chkNotPay.AutoSize = true;
            this.chkNotPay.BackColor = System.Drawing.Color.Transparent;
            this.chkNotPay.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.chkNotPay.DownBack = null;
            this.chkNotPay.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chkNotPay.Location = new System.Drawing.Point(473, 10);
            this.chkNotPay.MouseBack = null;
            this.chkNotPay.Name = "chkNotPay";
            this.chkNotPay.NormlBack = null;
            this.chkNotPay.SelectedDownBack = null;
            this.chkNotPay.SelectedMouseBack = null;
            this.chkNotPay.SelectedNormlBack = null;
            this.chkNotPay.Size = new System.Drawing.Size(63, 21);
            this.chkNotPay.TabIndex = 5;
            this.chkNotPay.Text = "未结算";
            this.chkNotPay.UseVisualStyleBackColor = false;
            // 
            // cmsOrdermanager
            // 
            this.cmsOrdermanager.Arrow = System.Drawing.Color.Black;
            this.cmsOrdermanager.Back = System.Drawing.Color.White;
            this.cmsOrdermanager.BackRadius = 4;
            this.cmsOrdermanager.Base = System.Drawing.Color.FromArgb(((int)(((byte)(105)))), ((int)(((byte)(200)))), ((int)(((byte)(254)))));
            this.cmsOrdermanager.DropDownImageSeparator = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(197)))), ((int)(((byte)(197)))));
            this.cmsOrdermanager.Fore = System.Drawing.Color.Black;
            this.cmsOrdermanager.HoverFore = System.Drawing.Color.White;
            this.cmsOrdermanager.ItemAnamorphosis = true;
            this.cmsOrdermanager.ItemBorder = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(148)))), ((int)(((byte)(212)))));
            this.cmsOrdermanager.ItemBorderShow = true;
            this.cmsOrdermanager.ItemHover = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(148)))), ((int)(((byte)(212)))));
            this.cmsOrdermanager.ItemPressed = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(148)))), ((int)(((byte)(212)))));
            this.cmsOrdermanager.ItemRadius = 4;
            this.cmsOrdermanager.ItemRadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.cmsOrdermanager.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.刷新ToolStripMenuItem,
            this.补分ToolStripMenuItem,
            this.全部补分ToolStripMenuItem,
            this.退出ToolStripMenuItem});
            this.cmsOrdermanager.ItemSplitter = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(148)))), ((int)(((byte)(212)))));
            this.cmsOrdermanager.Name = "cmsOrdermanager";
            this.cmsOrdermanager.RadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.cmsOrdermanager.Size = new System.Drawing.Size(125, 92);
            this.cmsOrdermanager.SkinAllColor = true;
            this.cmsOrdermanager.TitleAnamorphosis = true;
            this.cmsOrdermanager.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(228)))), ((int)(((byte)(236)))));
            this.cmsOrdermanager.TitleRadius = 4;
            this.cmsOrdermanager.TitleRadiusStyle = CCWin.SkinClass.RoundStyle.All;
            // 
            // 刷新ToolStripMenuItem
            // 
            this.刷新ToolStripMenuItem.Name = "刷新ToolStripMenuItem";
            this.刷新ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.刷新ToolStripMenuItem.Text = "刷新";
            // 
            // 补分ToolStripMenuItem
            // 
            this.补分ToolStripMenuItem.Name = "补分ToolStripMenuItem";
            this.补分ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.补分ToolStripMenuItem.Text = "补分";
            this.补分ToolStripMenuItem.Click += new System.EventHandler(this.补分ToolStripMenuItem_Click);
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.退出ToolStripMenuItem.Text = "退出";
            // 
            // 全部补分ToolStripMenuItem
            // 
            this.全部补分ToolStripMenuItem.Name = "全部补分ToolStripMenuItem";
            this.全部补分ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.全部补分ToolStripMenuItem.Text = "全部补分";
            this.全部补分ToolStripMenuItem.Click += new System.EventHandler(this.全部补分ToolStripMenuItem_Click);
            // 
            // BetOrderManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SeaGreen;
            this.ClientSize = new System.Drawing.Size(1036, 774);
            this.Controls.Add(this.dgv_orders);
            this.Controls.Add(this.pnl_top);
            this.Name = "BetOrderManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "订单管理";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BetOrderManager_FormClosing);
            this.Load += new System.EventHandler(this.BetOrderManager_Load);
            this.Shown += new System.EventHandler(this.BetOrderManager_Shown);
            this.pnl_top.ResumeLayout(false);
            this.pnl_top.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_orders)).EndInit();
            this.cmsOrdermanager.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinPanel pnl_top;
        private CCWin.SkinControl.SkinDataGridView dgv_orders;
        private CCWin.SkinControl.SkinButton btnSearch;
        private System.Windows.Forms.DateTimePicker dpckSearch;
        private System.Windows.Forms.Label skinLabel1;
        private CCWin.SkinControl.SkinTextBox tbxIssueid;
        private System.Windows.Forms.Label skinLabel2;
        private CCWin.SkinControl.SkinCheckBox chkNotPay;
        private CCWin.SkinControl.SkinTextBox tbxGroupid;
        private CCWin.SkinControl.SkinContextMenuStrip cmsOrdermanager;
        private System.Windows.Forms.ToolStripMenuItem 刷新ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 补分ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 退出ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 全部补分ToolStripMenuItem;
    }
}