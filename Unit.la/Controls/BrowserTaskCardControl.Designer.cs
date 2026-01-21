namespace Unit.La.Controls
{
    partial class BrowserTaskCardControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupControl = new DevExpress.XtraEditors.GroupControl();
            pictureBoxThumbnail = new System.Windows.Forms.PictureBox();
            panelInfo = new DevExpress.XtraEditors.PanelControl();
            labelLastRunTime = new DevExpress.XtraEditors.LabelControl();
            labelStatus = new DevExpress.XtraEditors.LabelControl();
            labelUrl = new DevExpress.XtraEditors.LabelControl();
            labelTaskName = new DevExpress.XtraEditors.LabelControl();
            panelButtons = new DevExpress.XtraEditors.PanelControl();
            btnClose = new DevExpress.XtraEditors.SimpleButton();
            btnDelete = new DevExpress.XtraEditors.SimpleButton();
            btnStartStop = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)groupControl).BeginInit();
            groupControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxThumbnail).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelInfo).BeginInit();
            panelInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelButtons).BeginInit();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // groupControl
            // 
            groupControl.Controls.Add(pictureBoxThumbnail);
            groupControl.Controls.Add(panelInfo);
            groupControl.Controls.Add(panelButtons);
            groupControl.Dock = System.Windows.Forms.DockStyle.Fill;
            groupControl.Location = new System.Drawing.Point(0, 0);
            groupControl.Name = "groupControl";
            groupControl.Size = new System.Drawing.Size(280, 240);
            groupControl.TabIndex = 0;
            groupControl.Text = "浏览器任务";
            // 
            // pictureBoxThumbnail
            // 
            pictureBoxThumbnail.BackColor = System.Drawing.Color.Black;
            pictureBoxThumbnail.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxThumbnail.Location = new System.Drawing.Point(2, 23);
            pictureBoxThumbnail.Name = "pictureBoxThumbnail";
            pictureBoxThumbnail.Size = new System.Drawing.Size(276, 150);
            pictureBoxThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBoxThumbnail.TabIndex = 0;
            pictureBoxThumbnail.TabStop = false;
            pictureBoxThumbnail.Click += OnThumbnailClick;
            // 
            // panelInfo
            // 
            panelInfo.Controls.Add(labelLastRunTime);
            panelInfo.Controls.Add(labelStatus);
            panelInfo.Controls.Add(labelUrl);
            panelInfo.Controls.Add(labelTaskName);
            panelInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelInfo.Location = new System.Drawing.Point(2, 173);
            panelInfo.Name = "panelInfo";
            panelInfo.Size = new System.Drawing.Size(276, 35);
            panelInfo.TabIndex = 1;
            // 
            // labelLastRunTime
            // 
            labelLastRunTime.Appearance.ForeColor = System.Drawing.Color.Gray;
            labelLastRunTime.Appearance.Options.UseForeColor = true;
            labelLastRunTime.Location = new System.Drawing.Point(10, 18);
            labelLastRunTime.Name = "labelLastRunTime";
            labelLastRunTime.Size = new System.Drawing.Size(48, 14);
            labelLastRunTime.TabIndex = 3;
            labelLastRunTime.Text = "未运行";
            // 
            // labelStatus
            // 
            labelStatus.Appearance.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            labelStatus.Appearance.ForeColor = System.Drawing.Color.Green;
            labelStatus.Appearance.Options.UseFont = true;
            labelStatus.Appearance.Options.UseForeColor = true;
            labelStatus.Location = new System.Drawing.Point(220, 3);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(44, 13);
            labelStatus.TabIndex = 2;
            labelStatus.Text = "待启动";
            // 
            // labelUrl
            // 
            labelUrl.Appearance.ForeColor = System.Drawing.Color.Gray;
            labelUrl.Appearance.Options.UseForeColor = true;
            labelUrl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            labelUrl.Location = new System.Drawing.Point(75, 3);
            labelUrl.Name = "labelUrl";
            labelUrl.Size = new System.Drawing.Size(140, 14);
            labelUrl.TabIndex = 1;
            labelUrl.Text = "https://...";
            // 
            // labelTaskName
            // 
            labelTaskName.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            labelTaskName.Appearance.Options.UseFont = true;
            labelTaskName.Location = new System.Drawing.Point(10, 3);
            labelTaskName.Name = "labelTaskName";
            labelTaskName.Size = new System.Drawing.Size(52, 14);
            labelTaskName.TabIndex = 0;
            labelTaskName.Text = "任务名称";
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(btnClose);
            panelButtons.Controls.Add(btnDelete);
            panelButtons.Controls.Add(btnStartStop);
            panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelButtons.Location = new System.Drawing.Point(2, 208);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new System.Drawing.Size(276, 30);
            panelButtons.TabIndex = 2;
            // 
            // btnClose
            // 
            btnClose.Appearance.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
            btnClose.Appearance.Options.UseBackColor = true;
            btnClose.Location = new System.Drawing.Point(150, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(55, 24);
            btnClose.TabIndex = 3;
            btnClose.Text = "🔴 关闭";
            btnClose.Click += OnCloseButtonClick;
            btnClose.Visible = false;
            // 
            // btnDelete
            // 
            btnDelete.Appearance.BackColor = System.Drawing.Color.FromArgb(192, 0, 0);
            btnDelete.Appearance.Options.UseBackColor = true;
            btnDelete.Location = new System.Drawing.Point(210, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(60, 24);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "删除";
            btnDelete.Click += OnDeleteButtonClick;
            // 
            // btnStartStop
            // 
            btnStartStop.Appearance.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
            btnStartStop.Appearance.Options.UseBackColor = true;
            btnStartStop.Location = new System.Drawing.Point(10, 3);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new System.Drawing.Size(135, 24);
            btnStartStop.TabIndex = 0;
            btnStartStop.Text = "▶ 启动";
            btnStartStop.Click += OnStartStopButtonClick;
            // 
            // BrowserTaskCardControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(groupControl);
            Name = "BrowserTaskCardControl";
            Size = new System.Drawing.Size(280, 240);
            ((System.ComponentModel.ISupportInitialize)groupControl).EndInit();
            groupControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxThumbnail).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelInfo).EndInit();
            panelInfo.ResumeLayout(false);
            panelInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)panelButtons).EndInit();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.GroupControl groupControl;
        private System.Windows.Forms.PictureBox pictureBoxThumbnail;
        private DevExpress.XtraEditors.PanelControl panelInfo;
        private DevExpress.XtraEditors.LabelControl labelLastRunTime;
        private DevExpress.XtraEditors.LabelControl labelStatus;
        private DevExpress.XtraEditors.LabelControl labelUrl;
        private DevExpress.XtraEditors.LabelControl labelTaskName;
        private DevExpress.XtraEditors.PanelControl panelButtons;
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnStartStop;
    }
}
