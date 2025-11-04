namespace F5Bot.Main.X
{
    partial class UcXBuySellCoins
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
            this.components = new System.ComponentModel.Container();
            this.btn_agree = new CCWin.SkinControl.SkinButton();
            this.btn_ignore = new CCWin.SkinControl.SkinButton();
            this.btn_AllOrder = new CCWin.SkinControl.SkinButton();
            this.btn_NikeName = new CCWin.SkinControl.SkinButton();
            this.btn_PayAction = new CCWin.SkinControl.SkinButton();
            this.lbl_money = new System.Windows.Forms.Label();
            this.lbl_Datetime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_agree
            // 
            this.btn_agree.BackColor = System.Drawing.Color.Transparent;
            this.btn_agree.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btn_agree.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_agree.DownBack = null;
            this.btn_agree.Location = new System.Drawing.Point(0, 65);
            this.btn_agree.MouseBack = null;
            this.btn_agree.Name = "btn_agree";
            this.btn_agree.NormlBack = null;
            this.btn_agree.Size = new System.Drawing.Size(44, 23);
            this.btn_agree.TabIndex = 0;
            this.btn_agree.Text = "同意";
            this.btn_agree.UseVisualStyleBackColor = false;
            this.btn_agree.Click += new System.EventHandler(this.btn_agree_Click);
            // 
            // btn_ignore
            // 
            this.btn_ignore.BackColor = System.Drawing.Color.Transparent;
            this.btn_ignore.BaseColor = System.Drawing.Color.Tomato;
            this.btn_ignore.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_ignore.DownBack = null;
            this.btn_ignore.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_ignore.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btn_ignore.Location = new System.Drawing.Point(48, 65);
            this.btn_ignore.MouseBack = null;
            this.btn_ignore.Name = "btn_ignore";
            this.btn_ignore.NormlBack = null;
            this.btn_ignore.Size = new System.Drawing.Size(46, 23);
            this.btn_ignore.TabIndex = 1;
            this.btn_ignore.Text = "忽略";
            this.btn_ignore.UseVisualStyleBackColor = false;
            this.btn_ignore.Click += new System.EventHandler(this.btn_ignore_Click);
            // 
            // btn_AllOrder
            // 
            this.btn_AllOrder.BackColor = System.Drawing.Color.Transparent;
            this.btn_AllOrder.BaseColor = System.Drawing.Color.Yellow;
            this.btn_AllOrder.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_AllOrder.DownBack = null;
            this.btn_AllOrder.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_AllOrder.ForeColor = System.Drawing.Color.OrangeRed;
            this.btn_AllOrder.Location = new System.Drawing.Point(0, 94);
            this.btn_AllOrder.MouseBack = null;
            this.btn_AllOrder.Name = "btn_AllOrder";
            this.btn_AllOrder.NormlBack = null;
            this.btn_AllOrder.Size = new System.Drawing.Size(166, 20);
            this.btn_AllOrder.TabIndex = 3;
            this.btn_AllOrder.Text = "上下分管理";
            this.btn_AllOrder.UseVisualStyleBackColor = false;
            this.btn_AllOrder.Click += new System.EventHandler(this.btn_AllOrder_Click);
            // 
            // btn_NikeName
            // 
            this.btn_NikeName.BackColor = System.Drawing.Color.Transparent;
            this.btn_NikeName.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btn_NikeName.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_NikeName.DownBack = null;
            this.btn_NikeName.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_NikeName.ForeColor = System.Drawing.Color.Black;
            this.btn_NikeName.GlowColor = System.Drawing.Color.Transparent;
            this.btn_NikeName.InnerBorderColor = System.Drawing.Color.Transparent;
            this.btn_NikeName.Location = new System.Drawing.Point(-1, -1);
            this.btn_NikeName.Margin = new System.Windows.Forms.Padding(1);
            this.btn_NikeName.MouseBack = null;
            this.btn_NikeName.Name = "btn_NikeName";
            this.btn_NikeName.NormlBack = null;
            this.btn_NikeName.Size = new System.Drawing.Size(167, 21);
            this.btn_NikeName.TabIndex = 4;
            this.btn_NikeName.Text = "会员:@昵称啊啊啊啊";
            this.btn_NikeName.UseVisualStyleBackColor = false;
            // 
            // btn_PayAction
            // 
            this.btn_PayAction.BackColor = System.Drawing.Color.Transparent;
            this.btn_PayAction.BaseColor = System.Drawing.Color.Lime;
            this.btn_PayAction.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_PayAction.DownBack = null;
            this.btn_PayAction.Location = new System.Drawing.Point(-1, 21);
            this.btn_PayAction.Margin = new System.Windows.Forms.Padding(1);
            this.btn_PayAction.MouseBack = null;
            this.btn_PayAction.Name = "btn_PayAction";
            this.btn_PayAction.NormlBack = null;
            this.btn_PayAction.Size = new System.Drawing.Size(46, 21);
            this.btn_PayAction.TabIndex = 5;
            this.btn_PayAction.Text = "未知";
            this.btn_PayAction.UseVisualStyleBackColor = false;
            // 
            // lbl_money
            // 
            this.lbl_money.AutoSize = true;
            this.lbl_money.BackColor = System.Drawing.Color.Transparent;
            this.lbl_money.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_money.Location = new System.Drawing.Point(47, 24);
            this.lbl_money.Name = "lbl_money";
            this.lbl_money.Size = new System.Drawing.Size(48, 17);
            this.lbl_money.TabIndex = 6;
            this.lbl_money.Text = "********";
            // 
            // lbl_Datetime
            // 
            this.lbl_Datetime.BackColor = System.Drawing.Color.Lime;
            this.lbl_Datetime.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_Datetime.Location = new System.Drawing.Point(-1, 44);
            this.lbl_Datetime.Name = "lbl_Datetime";
            this.lbl_Datetime.Size = new System.Drawing.Size(63, 18);
            this.lbl_Datetime.TabIndex = 7;
            this.lbl_Datetime.Text = "23:59:59";
            this.lbl_Datetime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UcBuySellCoins
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.Controls.Add(this.lbl_Datetime);
            this.Controls.Add(this.lbl_money);
            this.Controls.Add(this.btn_PayAction);
            this.Controls.Add(this.btn_NikeName);
            this.Controls.Add(this.btn_AllOrder);
            this.Controls.Add(this.btn_ignore);
            this.Controls.Add(this.btn_agree);
            this.Name = "UcBuySellCoins";
            this.Size = new System.Drawing.Size(173, 123);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CCWin.SkinControl.SkinButton btn_agree;
        private CCWin.SkinControl.SkinButton btn_ignore;
        private CCWin.SkinControl.SkinButton btn_AllOrder;
        private CCWin.SkinControl.SkinButton btn_NikeName;
        private CCWin.SkinControl.SkinButton btn_PayAction;
        private System.Windows.Forms.Label lbl_money;
        private System.Windows.Forms.Label lbl_Datetime;
    }
}
