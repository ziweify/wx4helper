namespace YongLiSystem.Views.Dashboard.Controls
{
    partial class ScriptTaskCardControl
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
            labelStatus = new DevExpress.XtraEditors.LabelControl();
            labelUrl = new DevExpress.XtraEditors.LabelControl();
            labelTaskName = new DevExpress.XtraEditors.LabelControl();
            panelButtons = new DevExpress.XtraEditors.PanelControl();
            btnDelete = new DevExpress.XtraEditors.SimpleButton();
            btnEdit = new DevExpress.XtraEditors.SimpleButton();
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
            groupControl.Size = new System.Drawing.Size(280, 220);
            groupControl.TabIndex = 0;
            groupControl.Text = "脚本任务";
            // 
            // pictureBoxThumbnail
            // 
            pictureBoxThumbnail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxThumbnail.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxThumbnail.Location = new System.Drawing.Point(2, 63);
            pictureBoxThumbnail.Name = "pictureBoxThumbnail";
            pictureBoxThumbnail.Size = new System.Drawing.Size(276, 125);
            pictureBoxThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBoxThumbnail.TabIndex = 0;
            pictureBoxThumbnail.TabStop = false;
            // 
            // panelInfo
            // 
            panelInfo.Controls.Add(labelStatus);
            panelInfo.Controls.Add(labelUrl);
            panelInfo.Controls.Add(labelTaskName);
            panelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            panelInfo.Location = new System.Drawing.Point(2, 23);
            panelInfo.Name = "panelInfo";
            panelInfo.Size = new System.Drawing.Size(276, 40);
            panelInfo.TabIndex = 1;
            // 
            // labelStatus
            // 
            labelStatus.Appearance.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            labelStatus.Appearance.ForeColor = System.Drawing.Color.Green;
            labelStatus.Appearance.Options.UseFont = true;
            labelStatus.Appearance.Options.UseForeColor = true;
            labelStatus.Location = new System.Drawing.Point(220, 12);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(44, 13);
            labelStatus.TabIndex = 2;
            labelStatus.Text = "待启动";
            // 
            // labelUrl
            // 
            labelUrl.Appearance.ForeColor = System.Drawing.Color.Gray;
            labelUrl.Appearance.Options.UseForeColor = true;
            labelUrl.Location = new System.Drawing.Point(10, 23);
            labelUrl.Name = "labelUrl";
            labelUrl.Size = new System.Drawing.Size(80, 14);
            labelUrl.TabIndex = 1;
            labelUrl.Text = "https://...";
            // 
            // labelTaskName
            // 
            labelTaskName.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            labelTaskName.Appearance.Options.UseFont = true;
            labelTaskName.Location = new System.Drawing.Point(10, 5);
            labelTaskName.Name = "labelTaskName";
            labelTaskName.Size = new System.Drawing.Size(52, 14);
            labelTaskName.TabIndex = 0;
            labelTaskName.Text = "任务名称";
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(btnDelete);
            panelButtons.Controls.Add(btnEdit);
            panelButtons.Controls.Add(btnStartStop);
            panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelButtons.Location = new System.Drawing.Point(2, 188);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new System.Drawing.Size(276, 30);
            panelButtons.TabIndex = 2;
            // 
            // btnDelete
            // 
            btnDelete.Appearance.BackColor = System.Drawing.Color.FromArgb(192, 0, 0);
            btnDelete.Appearance.Options.UseBackColor = true;
            btnDelete.Location = new System.Drawing.Point(190, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(80, 24);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "删除";
            btnDelete.Click += OnDeleteButtonClick;
            // 
            // btnEdit
            // 
            btnEdit.Location = new System.Drawing.Point(100, 3);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new System.Drawing.Size(80, 24);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "编辑";
            btnEdit.Click += OnEditButtonClick;
            // 
            // btnStartStop
            // 
            btnStartStop.Appearance.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
            btnStartStop.Appearance.Options.UseBackColor = true;
            btnStartStop.Location = new System.Drawing.Point(10, 3);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new System.Drawing.Size(80, 24);
            btnStartStop.TabIndex = 0;
            btnStartStop.Text = "▶ 启动";
            btnStartStop.Click += OnStartStopButtonClick;
            // 
            // ScriptTaskCardControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(groupControl);
            Name = "ScriptTaskCardControl";
            Size = new System.Drawing.Size(280, 220);
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
        private DevExpress.XtraEditors.LabelControl labelStatus;
        private DevExpress.XtraEditors.LabelControl labelUrl;
        private DevExpress.XtraEditors.LabelControl labelTaskName;
        private DevExpress.XtraEditors.PanelControl panelButtons;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnEdit;
        private DevExpress.XtraEditors.SimpleButton btnStartStop;
    }
}
