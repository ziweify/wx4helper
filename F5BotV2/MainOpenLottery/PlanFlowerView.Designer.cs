namespace F5BotV2.MainOpenLottery
{
    partial class PlanFlowerView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvLotteryData = new CCWin.SkinControl.SkinCaptionPanel();
            this.grb_plan = new System.Windows.Forms.GroupBox();
            this.dgv_plan = new CCWin.SkinControl.SkinDataGridView();
            this.skinLabel1 = new System.Windows.Forms.Label();
            this.tbxIssueid = new CCWin.SkinControl.SkinTextBox();
            this.btnGetIssueCur = new CCWin.SkinControl.SkinButton();
            this.dgview = new CCWin.SkinControl.SkinDataGridView();
            this.chk_lotteryDataUpdata = new CCWin.SkinControl.SkinCheckBox();
            this.btn_queryLotteryDataDay = new CCWin.SkinControl.SkinButton();
            this.datetimePick = new System.Windows.Forms.DateTimePicker();
            this.dgvLotteryData.SuspendLayout();
            this.grb_plan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_plan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgview)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvLotteryData
            // 
            this.dgvLotteryData.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.dgvLotteryData.Controls.Add(this.datetimePick);
            this.dgvLotteryData.Controls.Add(this.grb_plan);
            this.dgvLotteryData.Controls.Add(this.skinLabel1);
            this.dgvLotteryData.Controls.Add(this.tbxIssueid);
            this.dgvLotteryData.Controls.Add(this.btnGetIssueCur);
            this.dgvLotteryData.Controls.Add(this.dgview);
            this.dgvLotteryData.Controls.Add(this.chk_lotteryDataUpdata);
            this.dgvLotteryData.Controls.Add(this.btn_queryLotteryDataDay);
            this.dgvLotteryData.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvLotteryData.Location = new System.Drawing.Point(4, 28);
            this.dgvLotteryData.Name = "dgvLotteryData";
            this.dgvLotteryData.Size = new System.Drawing.Size(1272, 1039);
            this.dgvLotteryData.TabIndex = 0;
            this.dgvLotteryData.Text = "开奖数据";
            // 
            // grb_plan
            // 
            this.grb_plan.Controls.Add(this.dgv_plan);
            this.grb_plan.Location = new System.Drawing.Point(0, 733);
            this.grb_plan.Name = "grb_plan";
            this.grb_plan.Size = new System.Drawing.Size(1261, 306);
            this.grb_plan.TabIndex = 16;
            this.grb_plan.TabStop = false;
            this.grb_plan.Text = "投注方案";
            // 
            // dgv_plan
            // 
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.dgv_plan.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_plan.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_plan.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgv_plan.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgv_plan.ColumnFont = null;
            this.dgv_plan.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(246)))), ((int)(((byte)(239)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_plan.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgv_plan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_plan.ColumnSelectForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(188)))), ((int)(((byte)(240)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_plan.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgv_plan.EnableHeadersVisualStyles = false;
            this.dgv_plan.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgv_plan.HeadFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgv_plan.HeadSelectForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgv_plan.Location = new System.Drawing.Point(0, 20);
            this.dgv_plan.Name = "dgv_plan";
            this.dgv_plan.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgv_plan.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgv_plan.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgv_plan.RowTemplate.Height = 23;
            this.dgv_plan.Size = new System.Drawing.Size(1029, 286);
            this.dgv_plan.TabIndex = 0;
            this.dgv_plan.TitleBack = null;
            this.dgv_plan.TitleBackColorBegin = System.Drawing.Color.White;
            this.dgv_plan.TitleBackColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(196)))), ((int)(((byte)(242)))));
            // 
            // skinLabel1
            // 
            this.skinLabel1.AutoSize = true;
            this.skinLabel1.BackColor = System.Drawing.Color.Transparent;
            //this.skinLabel1.BorderColor = System.Drawing.Color.White;
            this.skinLabel1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel1.Location = new System.Drawing.Point(336, 28);
            this.skinLabel1.Name = "skinLabel1";
            this.skinLabel1.Size = new System.Drawing.Size(56, 17);
            this.skinLabel1.TabIndex = 15;
            this.skinLabel1.Text = "开奖期号";
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
            this.tbxIssueid.Location = new System.Drawing.Point(401, 22);
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
            this.tbxIssueid.Size = new System.Drawing.Size(146, 28);
            // 
            // 
            // 
            this.tbxIssueid.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxIssueid.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxIssueid.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbxIssueid.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbxIssueid.SkinTxt.Name = "BaseText";
            this.tbxIssueid.SkinTxt.Size = new System.Drawing.Size(136, 18);
            this.tbxIssueid.SkinTxt.TabIndex = 0;
            this.tbxIssueid.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbxIssueid.SkinTxt.WaterText = "";
            this.tbxIssueid.TabIndex = 14;
            this.tbxIssueid.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxIssueid.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbxIssueid.WaterText = "";
            this.tbxIssueid.WordWrap = true;
            // 
            // btnGetIssueCur
            // 
            this.btnGetIssueCur.BackColor = System.Drawing.Color.Transparent;
            this.btnGetIssueCur.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnGetIssueCur.DownBack = null;
            this.btnGetIssueCur.Location = new System.Drawing.Point(550, 24);
            this.btnGetIssueCur.MouseBack = null;
            this.btnGetIssueCur.Name = "btnGetIssueCur";
            this.btnGetIssueCur.NormlBack = null;
            this.btnGetIssueCur.Size = new System.Drawing.Size(76, 23);
            this.btnGetIssueCur.TabIndex = 13;
            this.btnGetIssueCur.Text = "当前期号";
            this.btnGetIssueCur.UseVisualStyleBackColor = false;
            this.btnGetIssueCur.Click += new System.EventHandler(this.btnGetIssueCur_Click);
            // 
            // dgview
            // 
            this.dgview.AllowUserToAddRows = false;
            this.dgview.AllowUserToDeleteRows = false;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.dgview.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgview.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgview.ColumnFont = null;
            this.dgview.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(246)))), ((int)(((byte)(239)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgview.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dgview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgview.ColumnSelectForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(188)))), ((int)(((byte)(240)))));
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgview.DefaultCellStyle = dataGridViewCellStyle7;
            this.dgview.EnableHeadersVisualStyles = false;
            this.dgview.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgview.HeadFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgview.HeadSelectForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgview.Location = new System.Drawing.Point(-5, 54);
            this.dgview.Name = "dgview";
            this.dgview.ReadOnly = true;
            this.dgview.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgview.RowHeadersVisible = false;
            this.dgview.RowHeadersWidth = 62;
            this.dgview.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgview.RowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dgview.RowTemplate.Height = 23;
            this.dgview.Size = new System.Drawing.Size(1266, 673);
            this.dgview.TabIndex = 9;
            this.dgview.TitleBack = null;
            this.dgview.TitleBackColorBegin = System.Drawing.Color.White;
            this.dgview.TitleBackColorEnd = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(196)))), ((int)(((byte)(242)))));
            this.dgview.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgview_CellFormatting);
            this.dgview.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgview_CellPainting);
            // 
            // chk_lotteryDataUpdata
            // 
            this.chk_lotteryDataUpdata.AutoSize = true;
            this.chk_lotteryDataUpdata.BackColor = System.Drawing.Color.Transparent;
            this.chk_lotteryDataUpdata.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.chk_lotteryDataUpdata.DownBack = null;
            this.chk_lotteryDataUpdata.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chk_lotteryDataUpdata.Location = new System.Drawing.Point(212, 26);
            this.chk_lotteryDataUpdata.MouseBack = null;
            this.chk_lotteryDataUpdata.Name = "chk_lotteryDataUpdata";
            this.chk_lotteryDataUpdata.NormlBack = null;
            this.chk_lotteryDataUpdata.SelectedDownBack = null;
            this.chk_lotteryDataUpdata.SelectedMouseBack = null;
            this.chk_lotteryDataUpdata.SelectedNormlBack = null;
            this.chk_lotteryDataUpdata.Size = new System.Drawing.Size(75, 21);
            this.chk_lotteryDataUpdata.TabIndex = 8;
            this.chk_lotteryDataUpdata.Text = "自动更新";
            this.chk_lotteryDataUpdata.UseVisualStyleBackColor = false;
            // 
            // btn_queryLotteryDataDay
            // 
            this.btn_queryLotteryDataDay.BackColor = System.Drawing.Color.Transparent;
            this.btn_queryLotteryDataDay.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_queryLotteryDataDay.DownBack = null;
            this.btn_queryLotteryDataDay.Location = new System.Drawing.Point(132, 25);
            this.btn_queryLotteryDataDay.MouseBack = null;
            this.btn_queryLotteryDataDay.Name = "btn_queryLotteryDataDay";
            this.btn_queryLotteryDataDay.NormlBack = null;
            this.btn_queryLotteryDataDay.Size = new System.Drawing.Size(75, 23);
            this.btn_queryLotteryDataDay.TabIndex = 6;
            this.btn_queryLotteryDataDay.Text = "日期查询";
            this.btn_queryLotteryDataDay.UseVisualStyleBackColor = false;
            this.btn_queryLotteryDataDay.Click += new System.EventHandler(this.btn_queryLotteryDataDay_Click);
            // 
            // datetimePick
            // 
            this.datetimePick.Location = new System.Drawing.Point(7, 29);
            this.datetimePick.Name = "datetimePick";
            this.datetimePick.Size = new System.Drawing.Size(119, 21);
            this.datetimePick.TabIndex = 17;
            // 
            // PlanFlowerView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.ClientSize = new System.Drawing.Size(1280, 1077);
            this.Controls.Add(this.dgvLotteryData);
            this.Name = "PlanFlowerView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "计划策略";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpenLotteryView_FormClosing);
            this.Load += new System.EventHandler(this.OpenLotteryView_Load);
            this.Shown += new System.EventHandler(this.OpenLotteryView_Shown);
            this.dgvLotteryData.ResumeLayout(false);
            this.dgvLotteryData.PerformLayout();
            this.grb_plan.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_plan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinCaptionPanel dgvLotteryData;
        private CCWin.SkinControl.SkinDataGridView dgview;
        private CCWin.SkinControl.SkinCheckBox chk_lotteryDataUpdata;
        private CCWin.SkinControl.SkinButton btn_queryLotteryDataDay;
        private CCWin.SkinControl.SkinTextBox tbxIssueid;
        private System.Windows.Forms.Label skinLabel1;
        private CCWin.SkinControl.SkinButton btnGetIssueCur;
        private System.Windows.Forms.GroupBox grb_plan;
        private CCWin.SkinControl.SkinDataGridView dgv_plan;
        private System.Windows.Forms.DateTimePicker datetimePick;
    }
}