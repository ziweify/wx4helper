namespace F5Bot.Main.X
{
    partial class UcXLotteryDataCur
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.skinLabel1 = new System.Windows.Forms.Label();
            this.lblOpenTime = new System.Windows.Forms.Label();
            this.lblIssue = new System.Windows.Forms.Label();
            this.lblNowTime = new System.Windows.Forms.Label();
            this.skinLabel4 = new System.Windows.Forms.Label();
            this.lblAlsoSecond = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // skinLabel1
            // 
            this.skinLabel1.AutoSize = true;
            this.skinLabel1.BackColor = System.Drawing.Color.MediumAquamarine;
            this.skinLabel1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel1.Location = new System.Drawing.Point(5, 2);
            this.skinLabel1.Name = "skinLabel1";
            this.skinLabel1.Size = new System.Drawing.Size(47, 17);
            this.skinLabel1.TabIndex = 0;
            this.skinLabel1.Text = "当前期:";
            // 
            // lblOpenTime
            // 
            this.lblOpenTime.AutoSize = true;
            this.lblOpenTime.BackColor = System.Drawing.Color.Goldenrod;
            this.lblOpenTime.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblOpenTime.ForeColor = System.Drawing.Color.Black;
            this.lblOpenTime.Location = new System.Drawing.Point(135, 2);
            this.lblOpenTime.Name = "lblOpenTime";
            this.lblOpenTime.Size = new System.Drawing.Size(47, 16);
            this.lblOpenTime.TabIndex = 1;
            this.lblOpenTime.Text = "23:54.00";
            // 
            // lblIssue
            // 
            this.lblIssue.AutoSize = true;
            this.lblIssue.BackColor = System.Drawing.Color.Gold;
            this.lblIssue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblIssue.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblIssue.Location = new System.Drawing.Point(59, 2);
            this.lblIssue.Name = "lblIssue";
            this.lblIssue.Size = new System.Drawing.Size(71, 17);
            this.lblIssue.TabIndex = 2;
            this.lblIssue.Text = "111063741";
            // 
            // lblNowTime
            // 
            this.lblNowTime.AutoSize = true;
            this.lblNowTime.BackColor = System.Drawing.Color.LightGreen;
            this.lblNowTime.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblNowTime.Location = new System.Drawing.Point(60, 23);
            this.lblNowTime.Name = "lblNowTime";
            this.lblNowTime.Size = new System.Drawing.Size(56, 17);
            this.lblNowTime.TabIndex = 1;
            this.lblNowTime.Text = "23:54.00";
            // 
            // skinLabel4
            // 
            this.skinLabel4.AutoSize = true;
            this.skinLabel4.BackColor = System.Drawing.Color.LightGreen;
            this.skinLabel4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel4.Location = new System.Drawing.Point(117, 23);
            this.skinLabel4.Name = "skinLabel4";
            this.skinLabel4.Size = new System.Drawing.Size(20, 17);
            this.skinLabel4.TabIndex = 1;
            this.skinLabel4.Text = "剩";
            // 
            // lblAlsoSecond
            // 
            this.lblAlsoSecond.AutoSize = true;
            this.lblAlsoSecond.BackColor = System.Drawing.Color.LightGreen;
            this.lblAlsoSecond.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAlsoSecond.Location = new System.Drawing.Point(138, 22);
            this.lblAlsoSecond.Name = "lblAlsoSecond";
            this.lblAlsoSecond.Size = new System.Drawing.Size(45, 19);
            this.lblAlsoSecond.TabIndex = 1;
            this.lblAlsoSecond.Text = "3000";
            this.lblAlsoSecond.Click += new System.EventHandler(this.skinLabel5_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.BackColor = System.Drawing.Color.LightGreen;
            this.lblStatus.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblStatus.Location = new System.Drawing.Point(6, 23);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(44, 17);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "投注中";
            // 
            // UcLotteryDataCur
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Green;
            this.Controls.Add(this.lblAlsoSecond);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.skinLabel4);
            this.Controls.Add(this.lblNowTime);
            this.Controls.Add(this.lblOpenTime);
            this.Controls.Add(this.lblIssue);
            this.Controls.Add(this.skinLabel1);
            this.Name = "UcLotteryDataCur";
            this.Size = new System.Drawing.Size(198, 52);
            this.Load += new System.EventHandler(this.UcLotteryData_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label skinLabel1;
        private System.Windows.Forms.Label lblOpenTime;
        private System.Windows.Forms.Label lblIssue;
        private System.Windows.Forms.Label lblNowTime;
        private System.Windows.Forms.Label skinLabel4;
        private System.Windows.Forms.Label lblAlsoSecond;
        private System.Windows.Forms.Label lblStatus;
    }
}
