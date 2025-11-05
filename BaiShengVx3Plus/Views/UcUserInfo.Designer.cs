namespace BaiShengVx3Plus.Views
{
    partial class UcUserInfo
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
            pic_headimage = new PictureBox();
            tbx_wxnick = new Sunny.UI.UITextBox();
            lbl_wxid = new Sunny.UI.UIMarkLabel();
            btnGetContactList = new Sunny.UI.UIButton();
            ((System.ComponentModel.ISupportInitialize)pic_headimage).BeginInit();
            SuspendLayout();
            // 
            // pic_headimage
            // 
            pic_headimage.BackColor = Color.FromArgb(80, 160, 255);
            pic_headimage.BorderStyle = BorderStyle.FixedSingle;
            pic_headimage.Location = new Point(8, 5);
            pic_headimage.Name = "pic_headimage";
            pic_headimage.Size = new Size(50, 50);
            pic_headimage.SizeMode = PictureBoxSizeMode.Zoom;
            pic_headimage.TabIndex = 0;
            pic_headimage.TabStop = false;
            // 
            // tbx_wxnick
            // 
            tbx_wxnick.BackColor = Color.White;
            tbx_wxnick.Font = new Font("微软雅黑", 12F, FontStyle.Bold, GraphicsUnit.Point, 134);
            tbx_wxnick.ForeColor = Color.FromArgb(48, 48, 48);
            tbx_wxnick.Location = new Point(65, 8);
            tbx_wxnick.Margin = new Padding(4, 5, 4, 5);
            tbx_wxnick.MinimumSize = new Size(1, 16);
            tbx_wxnick.Name = "tbx_wxnick";
            tbx_wxnick.Padding = new Padding(5, 3, 5, 3);
            tbx_wxnick.Radius = 0;
            tbx_wxnick.ReadOnly = true;
            tbx_wxnick.RectColor = Color.Transparent;
            tbx_wxnick.RectSize = 0;
            tbx_wxnick.ShowText = false;
            tbx_wxnick.Size = new Size(200, 25);
            tbx_wxnick.TabIndex = 1;
            tbx_wxnick.Text = "未登录";
            tbx_wxnick.TextAlignment = ContentAlignment.MiddleLeft;
            tbx_wxnick.Watermark = "";
            // 
            // lbl_wxid
            // 
            lbl_wxid.BackColor = Color.Transparent;
            lbl_wxid.Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lbl_wxid.ForeColor = Color.FromArgb(100, 100, 100);
            lbl_wxid.Location = new Point(65, 35);
            lbl_wxid.Name = "lbl_wxid";
            lbl_wxid.Padding = new Padding(5, 0, 0, 0);
            lbl_wxid.Size = new Size(200, 18);
            lbl_wxid.TabIndex = 2;
            lbl_wxid.Text = "请先登录微信";
            // 
            // btnGetContactList
            // 
            btnGetContactList.Cursor = Cursors.Hand;
            btnGetContactList.FillColor = Color.FromArgb(80, 160, 255);
            btnGetContactList.FillHoverColor = Color.FromArgb(100, 180, 255);
            btnGetContactList.FillPressColor = Color.FromArgb(60, 140, 235);
            btnGetContactList.FillSelectedColor = Color.FromArgb(60, 140, 235);
            btnGetContactList.Font = new Font("微软雅黑", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 134);
            btnGetContactList.ForeColor = Color.White;
            btnGetContactList.Location = new Point(272, 10);
            btnGetContactList.MinimumSize = new Size(1, 1);
            btnGetContactList.Name = "btnGetContactList";
            btnGetContactList.Radius = 6;
            btnGetContactList.RectColor = Color.FromArgb(80, 160, 255);
            btnGetContactList.RectHoverColor = Color.FromArgb(100, 180, 255);
            btnGetContactList.RectPressColor = Color.FromArgb(60, 140, 235);
            btnGetContactList.RectSelectedColor = Color.FromArgb(60, 140, 235);
            btnGetContactList.Size = new Size(60, 40);
            btnGetContactList.TabIndex = 3;
            btnGetContactList.Text = "采集";
            btnGetContactList.TipsFont = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // UcUserInfo
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(btnGetContactList);
            Controls.Add(lbl_wxid);
            Controls.Add(tbx_wxnick);
            Controls.Add(pic_headimage);
            Name = "UcUserInfo";
            Size = new Size(340, 60);
            ((System.ComponentModel.ISupportInitialize)pic_headimage).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pic_headimage;
        private Sunny.UI.UITextBox tbx_wxnick;
        private Sunny.UI.UIMarkLabel lbl_wxid;
        private Sunny.UI.UIButton btnGetContactList;
    }
}
