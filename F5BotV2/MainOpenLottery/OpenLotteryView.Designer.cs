namespace F5BotV2.MainOpenLottery
{
    partial class OpenLotteryView
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
            this.dgvLotteryData = new CCWin.SkinControl.SkinCaptionPanel();
            this.skinLabel1 = new System.Windows.Forms.Label();
            this.tbxIssueid = new CCWin.SkinControl.SkinTextBox();
            this.btnGetIssueCur = new CCWin.SkinControl.SkinButton();
            this.btnOpenLotteryForIssue = new CCWin.SkinControl.SkinButton();
            this.lbl_updata = new System.Windows.Forms.Label();
            this.lblCookie = new System.Windows.Forms.Label();
            this.tbxCookie = new CCWin.SkinControl.SkinTextBox();
            this.dgview = new CCWin.SkinControl.SkinDataGridView();
            this.chk_lotteryDataUpdata = new CCWin.SkinControl.SkinCheckBox();
            this.btn_getall_sql = new CCWin.SkinControl.SkinButton();
            this.btn_queryLotteryDataDay = new CCWin.SkinControl.SkinButton();
            this.datetimePick = new System.Windows.Forms.DateTimePicker();
            this.dgvLotteryData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgview)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvLotteryData
            // 
            this.dgvLotteryData.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.dgvLotteryData.Controls.Add(this.datetimePick);
            this.dgvLotteryData.Controls.Add(this.skinLabel1);
            this.dgvLotteryData.Controls.Add(this.tbxIssueid);
            this.dgvLotteryData.Controls.Add(this.btnGetIssueCur);
            this.dgvLotteryData.Controls.Add(this.btnOpenLotteryForIssue);
            this.dgvLotteryData.Controls.Add(this.lbl_updata);
            this.dgvLotteryData.Controls.Add(this.lblCookie);
            this.dgvLotteryData.Controls.Add(this.tbxCookie);
            this.dgvLotteryData.Controls.Add(this.dgview);
            this.dgvLotteryData.Controls.Add(this.chk_lotteryDataUpdata);
            this.dgvLotteryData.Controls.Add(this.btn_getall_sql);
            this.dgvLotteryData.Controls.Add(this.btn_queryLotteryDataDay);
            this.dgvLotteryData.Location = new System.Drawing.Point(7, 31);
            this.dgvLotteryData.Name = "dgvLotteryData";
            this.dgvLotteryData.Size = new System.Drawing.Size(1266, 1039);
            this.dgvLotteryData.TabIndex = 0;
            this.dgvLotteryData.Text = "开奖数据";
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
            // btnOpenLotteryForIssue
            // 
            this.btnOpenLotteryForIssue.BackColor = System.Drawing.Color.Transparent;
            this.btnOpenLotteryForIssue.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btnOpenLotteryForIssue.DownBack = null;
            this.btnOpenLotteryForIssue.Location = new System.Drawing.Point(629, 24);
            this.btnOpenLotteryForIssue.MouseBack = null;
            this.btnOpenLotteryForIssue.Name = "btnOpenLotteryForIssue";
            this.btnOpenLotteryForIssue.NormlBack = null;
            this.btnOpenLotteryForIssue.Size = new System.Drawing.Size(86, 23);
            this.btnOpenLotteryForIssue.TabIndex = 13;
            this.btnOpenLotteryForIssue.Text = "根据期号开奖";
            this.btnOpenLotteryForIssue.UseVisualStyleBackColor = false;
            this.btnOpenLotteryForIssue.Click += new System.EventHandler(this.btnOpenLotteryForIssue_Click);
            // 
            // lbl_updata
            // 
            this.lbl_updata.AutoSize = true;
            this.lbl_updata.Location = new System.Drawing.Point(476, 61);
            this.lbl_updata.Name = "lbl_updata";
            this.lbl_updata.Size = new System.Drawing.Size(71, 12);
            this.lbl_updata.TabIndex = 12;
            this.lbl_updata.Text = "正在更新{0}";
            // 
            // lblCookie
            // 
            this.lblCookie.AutoSize = true;
            this.lblCookie.BackColor = System.Drawing.Color.Transparent;
           // this.lblCookie.BorderColor = System.Drawing.Color.White;
            this.lblCookie.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCookie.Location = new System.Drawing.Point(7, 58);
            this.lblCookie.Name = "lblCookie";
            this.lblCookie.Size = new System.Drawing.Size(49, 17);
            this.lblCookie.TabIndex = 11;
            this.lblCookie.Text = "Cookie";
            // 
            // tbxCookie
            // 
            this.tbxCookie.BackColor = System.Drawing.Color.Transparent;
            this.tbxCookie.DownBack = null;
            this.tbxCookie.Icon = null;
            this.tbxCookie.IconIsButton = false;
            this.tbxCookie.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbxCookie.IsPasswordChat = '\0';
            this.tbxCookie.IsSystemPasswordChar = false;
            this.tbxCookie.Lines = new string[] {
        "PHPSESSID=b257e9b0f15e125b0e508bd00cfbdb6c; path=//"};
            this.tbxCookie.Location = new System.Drawing.Point(59, 51);
            this.tbxCookie.Margin = new System.Windows.Forms.Padding(0);
            this.tbxCookie.MaxLength = 32767;
            this.tbxCookie.MinimumSize = new System.Drawing.Size(28, 28);
            this.tbxCookie.MouseBack = null;
            this.tbxCookie.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.tbxCookie.Multiline = false;
            this.tbxCookie.Name = "tbxCookie";
            this.tbxCookie.NormlBack = null;
            this.tbxCookie.Padding = new System.Windows.Forms.Padding(5);
            this.tbxCookie.ReadOnly = false;
            this.tbxCookie.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbxCookie.Size = new System.Drawing.Size(414, 28);
            // 
            // 
            // 
            this.tbxCookie.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxCookie.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxCookie.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.tbxCookie.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.tbxCookie.SkinTxt.Name = "BaseText";
            this.tbxCookie.SkinTxt.Size = new System.Drawing.Size(404, 18);
            this.tbxCookie.SkinTxt.TabIndex = 0;
            this.tbxCookie.SkinTxt.Text = "PHPSESSID=b257e9b0f15e125b0e508bd00cfbdb6c; path=//";
            this.tbxCookie.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbxCookie.SkinTxt.WaterText = "";
            this.tbxCookie.TabIndex = 10;
            this.tbxCookie.Text = "PHPSESSID=b257e9b0f15e125b0e508bd00cfbdb6c; path=//";
            this.tbxCookie.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxCookie.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.tbxCookie.WaterText = "";
            this.tbxCookie.WordWrap = true;
            // 
            // dgview
            // 
            this.dgview.AllowUserToAddRows = false;
            this.dgview.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.dgview.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgview.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgview.ColumnFont = null;
            this.dgview.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(246)))), ((int)(((byte)(239)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgview.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgview.ColumnSelectForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(188)))), ((int)(((byte)(240)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgview.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgview.EnableHeadersVisualStyles = false;
            this.dgview.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgview.HeadFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dgview.HeadSelectForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgview.Location = new System.Drawing.Point(0, 82);
            this.dgview.Name = "dgview";
            this.dgview.ReadOnly = true;
            this.dgview.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgview.RowHeadersVisible = false;
            this.dgview.RowHeadersWidth = 62;
            this.dgview.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgview.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgview.RowTemplate.Height = 23;
            this.dgview.Size = new System.Drawing.Size(1266, 954);
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
            // btn_getall_sql
            // 
            this.btn_getall_sql.BackColor = System.Drawing.Color.Transparent;
            this.btn_getall_sql.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_getall_sql.DownBack = null;
            this.btn_getall_sql.Enabled = false;
            this.btn_getall_sql.Location = new System.Drawing.Point(1129, 50);
            this.btn_getall_sql.MouseBack = null;
            this.btn_getall_sql.Name = "btn_getall_sql";
            this.btn_getall_sql.NormlBack = null;
            this.btn_getall_sql.Size = new System.Drawing.Size(132, 23);
            this.btn_getall_sql.TabIndex = 6;
            this.btn_getall_sql.Text = "获取本地所有";
            this.btn_getall_sql.UseVisualStyleBackColor = false;
            this.btn_getall_sql.Click += new System.EventHandler(this.btn_getall_sql_Click);
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
            this.datetimePick.Location = new System.Drawing.Point(5, 27);
            this.datetimePick.Name = "datetimePick";
            this.datetimePick.Size = new System.Drawing.Size(121, 21);
            this.datetimePick.TabIndex = 16;
            // 
            // OpenLotteryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.ClientSize = new System.Drawing.Size(1280, 1077);
            this.Controls.Add(this.dgvLotteryData);
            this.Name = "OpenLotteryView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "开奖数据";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpenLotteryView_FormClosing);
            this.Load += new System.EventHandler(this.OpenLotteryView_Load);
            this.Shown += new System.EventHandler(this.OpenLotteryView_Shown);
            this.dgvLotteryData.ResumeLayout(false);
            this.dgvLotteryData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinCaptionPanel dgvLotteryData;
        private CCWin.SkinControl.SkinDataGridView dgview;
        private CCWin.SkinControl.SkinCheckBox chk_lotteryDataUpdata;
        private CCWin.SkinControl.SkinButton btn_queryLotteryDataDay;
        private System.Windows.Forms.Label lblCookie;
        private CCWin.SkinControl.SkinTextBox tbxCookie;
        private CCWin.SkinControl.SkinButton btn_getall_sql;
        private System.Windows.Forms.Label lbl_updata;
        private CCWin.SkinControl.SkinButton btnOpenLotteryForIssue;
        private CCWin.SkinControl.SkinTextBox tbxIssueid;
        private System.Windows.Forms.Label skinLabel1;
        private CCWin.SkinControl.SkinButton btnGetIssueCur;
        private System.Windows.Forms.DateTimePicker datetimePick;
    }
}