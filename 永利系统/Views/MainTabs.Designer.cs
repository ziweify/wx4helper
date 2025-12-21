using System.Windows.Forms;

namespace 永利系统.Views
{
    partial class MainTabs
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
            menuStrip1 = new MenuStrip();
            toolStripMenuItemFile = new ToolStripMenuItem();
            toolStripMenuItemNew = new ToolStripMenuItem();
            toolStripMenuItemOpen = new ToolStripMenuItem();
            toolStripMenuItemSave = new ToolStripMenuItem();
            toolStripMenuItemSaveAs = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItemExit = new ToolStripMenuItem();
            toolStripMenuItemTools = new ToolStripMenuItem();
            toolStripMenuItemOptions = new ToolStripMenuItem();
            toolStripMenuItemHelp = new ToolStripMenuItem();
            toolStripMenuItemAbout = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            toolStripButtonRefresh = new ToolStripButton();
            toolStripButtonSave = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripButtonLog = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripButtonWechatStart = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            toolStripButtonExit = new ToolStripButton();
            statusStrip1 = new StatusStrip();
            toolStripStatusStatus = new ToolStripStatusLabel();
            toolStripStatusUser = new ToolStripStatusLabel();
            toolStripStatusLog = new ToolStripStatusLabel();
            splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            logWindow1 = new LogWindow();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1.Panel1).BeginInit();
            splitContainerControl1.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1.Panel2).BeginInit();
            splitContainerControl1.Panel2.SuspendLayout();
            splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)xtraTabControl1).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] {
            toolStripMenuItemFile,
            toolStripMenuItemTools,
            toolStripMenuItemHelp});
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(1438, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItemFile
            // 
            toolStripMenuItemFile.DropDownItems.AddRange(new ToolStripItem[] {
            toolStripMenuItemNew,
            toolStripMenuItemOpen,
            toolStripMenuItemSave,
            toolStripMenuItemSaveAs,
            toolStripSeparator1,
            toolStripMenuItemExit});
            toolStripMenuItemFile.Name = "toolStripMenuItemFile";
            toolStripMenuItemFile.Size = new System.Drawing.Size(44, 20);
            toolStripMenuItemFile.Text = "文件";
            // 
            // toolStripMenuItemNew
            // 
            toolStripMenuItemNew.Name = "toolStripMenuItemNew";
            toolStripMenuItemNew.Size = new System.Drawing.Size(100, 22);
            toolStripMenuItemNew.Text = "新建";
            toolStripMenuItemNew.Click += ToolStripMenuItemNew_Click;
            // 
            // toolStripMenuItemOpen
            // 
            toolStripMenuItemOpen.Name = "toolStripMenuItemOpen";
            toolStripMenuItemOpen.Size = new System.Drawing.Size(100, 22);
            toolStripMenuItemOpen.Text = "打开";
            toolStripMenuItemOpen.Click += ToolStripMenuItemOpen_Click;
            // 
            // toolStripMenuItemSave
            // 
            toolStripMenuItemSave.Name = "toolStripMenuItemSave";
            toolStripMenuItemSave.Size = new System.Drawing.Size(100, 22);
            toolStripMenuItemSave.Text = "保存";
            toolStripMenuItemSave.Click += ToolStripMenuItemSave_Click;
            // 
            // toolStripMenuItemSaveAs
            // 
            toolStripMenuItemSaveAs.Name = "toolStripMenuItemSaveAs";
            toolStripMenuItemSaveAs.Size = new System.Drawing.Size(100, 22);
            toolStripMenuItemSaveAs.Text = "另存为";
            toolStripMenuItemSaveAs.Click += ToolStripMenuItemSaveAs_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(97, 6);
            // 
            // toolStripMenuItemExit
            // 
            toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            toolStripMenuItemExit.Size = new System.Drawing.Size(100, 22);
            toolStripMenuItemExit.Text = "退出";
            toolStripMenuItemExit.Click += ToolStripMenuItemExit_Click;
            // 
            // toolStripMenuItemTools
            // 
            toolStripMenuItemTools.DropDownItems.AddRange(new ToolStripItem[] {
            toolStripMenuItemOptions});
            toolStripMenuItemTools.Name = "toolStripMenuItemTools";
            toolStripMenuItemTools.Size = new System.Drawing.Size(44, 20);
            toolStripMenuItemTools.Text = "工具";
            // 
            // toolStripMenuItemOptions
            // 
            toolStripMenuItemOptions.Name = "toolStripMenuItemOptions";
            toolStripMenuItemOptions.Size = new System.Drawing.Size(100, 22);
            toolStripMenuItemOptions.Text = "选项";
            toolStripMenuItemOptions.Click += ToolStripMenuItemOptions_Click;
            // 
            // toolStripMenuItemHelp
            // 
            toolStripMenuItemHelp.DropDownItems.AddRange(new ToolStripItem[] {
            toolStripMenuItemAbout});
            toolStripMenuItemHelp.Name = "toolStripMenuItemHelp";
            toolStripMenuItemHelp.Size = new System.Drawing.Size(44, 20);
            toolStripMenuItemHelp.Text = "帮助";
            // 
            // toolStripMenuItemAbout
            // 
            toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
            toolStripMenuItemAbout.Size = new System.Drawing.Size(100, 22);
            toolStripMenuItemAbout.Text = "关于";
            toolStripMenuItemAbout.Click += ToolStripMenuItemAbout_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] {
            toolStripButtonRefresh,
            toolStripButtonSave,
            toolStripSeparator2,
            toolStripButtonLog,
            toolStripSeparator3,
            toolStripButtonWechatStart,
            toolStripSeparator4,
            toolStripButtonExit});
            toolStrip1.Location = new System.Drawing.Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1438, 49);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonRefresh
            // 
            toolStripButtonRefresh.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButtonRefresh.Image = CreateRefreshIcon();
            toolStripButtonRefresh.ImageScaling = ToolStripItemImageScaling.None;
            toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonRefresh.Name = "toolStripButtonRefresh";
            toolStripButtonRefresh.Size = new System.Drawing.Size(60, 46);
            toolStripButtonRefresh.Text = "刷新";
            toolStripButtonRefresh.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButtonRefresh.Click += toolStripButtonRefresh_Click;
            // 
            // toolStripButtonSave
            // 
            toolStripButtonSave.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButtonSave.Image = CreateSaveIcon();
            toolStripButtonSave.ImageScaling = ToolStripItemImageScaling.None;
            toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Size = new System.Drawing.Size(60, 46);
            toolStripButtonSave.Text = "保存";
            toolStripButtonSave.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButtonSave.Click += toolStripButtonSave_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonLog
            // 
            toolStripButtonLog.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButtonLog.Image = CreateLogIcon();
            toolStripButtonLog.ImageScaling = ToolStripItemImageScaling.None;
            toolStripButtonLog.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonLog.Name = "toolStripButtonLog";
            toolStripButtonLog.Size = new System.Drawing.Size(72, 46);
            toolStripButtonLog.Text = "查看日志";
            toolStripButtonLog.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButtonLog.Click += toolStripButtonLog_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonWechatStart
            // 
            toolStripButtonWechatStart.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButtonWechatStart.Image = CreateWechatIcon();
            toolStripButtonWechatStart.ImageScaling = ToolStripItemImageScaling.None;
            toolStripButtonWechatStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonWechatStart.Name = "toolStripButtonWechatStart";
            toolStripButtonWechatStart.Size = new System.Drawing.Size(84, 46);
            toolStripButtonWechatStart.Text = "启动微信";
            toolStripButtonWechatStart.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButtonWechatStart.Click += toolStripButtonWechatStart_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonExit
            // 
            toolStripButtonExit.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButtonExit.Image = CreateExitIcon();
            toolStripButtonExit.ImageScaling = ToolStripItemImageScaling.None;
            toolStripButtonExit.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonExit.Name = "toolStripButtonExit";
            toolStripButtonExit.Size = new System.Drawing.Size(60, 46);
            toolStripButtonExit.Text = "退出";
            toolStripButtonExit.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButtonExit.Click += toolStripButtonExit_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] {
            toolStripStatusStatus,
            toolStripStatusUser,
            toolStripStatusLog});
            statusStrip1.Location = new System.Drawing.Point(0, 875);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1438, 24);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusStatus
            // 
            toolStripStatusStatus.Name = "toolStripStatusStatus";
            toolStripStatusStatus.Size = new System.Drawing.Size(32, 19);
            toolStripStatusStatus.Text = "就绪";
            // 
            // toolStripStatusUser
            // 
            toolStripStatusUser.Name = "toolStripStatusUser";
            toolStripStatusUser.Size = new System.Drawing.Size(80, 19);
            toolStripStatusUser.Text = "当前用户: 管理员";
            // 
            // toolStripStatusLog
            // 
            toolStripStatusLog.Name = "toolStripStatusLog";
            toolStripStatusLog.Size = new System.Drawing.Size(1316, 19);
            toolStripStatusLog.Spring = true;
            toolStripStatusLog.Text = "日志信息";
            toolStripStatusLog.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            toolStripStatusLog.Click += ToolStripStatusLog_Click;
            // 
            // splitContainerControl1
            // 
            splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerControl1.Horizontal = false;
            splitContainerControl1.Location = new System.Drawing.Point(0, 73);
            splitContainerControl1.Name = "splitContainerControl1";
            // 
            // splitContainerControl1.Panel1
            // 
            splitContainerControl1.Panel1.Controls.Add(xtraTabControl1);
            splitContainerControl1.Panel1.Text = "Panel1";
            // 
            // splitContainerControl1.Panel2
            // 
            splitContainerControl1.Panel2.Controls.Add(logWindow1);
            splitContainerControl1.Panel2.Text = "Panel2";
            splitContainerControl1.ShowSplitGlyph = DevExpress.Utils.DefaultBoolean.True;
            splitContainerControl1.Size = new System.Drawing.Size(1438, 826);
            splitContainerControl1.SplitterPosition = 575;
            splitContainerControl1.TabIndex = 3;
            // 
            // xtraTabControl1
            // 
            xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            xtraTabControl1.Location = new System.Drawing.Point(0, 0);
            xtraTabControl1.Name = "xtraTabControl1";
            xtraTabControl1.Size = new System.Drawing.Size(1438, 575);
            xtraTabControl1.TabIndex = 0;
            // 
            // logWindow1
            // 
            logWindow1.Dock = System.Windows.Forms.DockStyle.Fill;
            logWindow1.Location = new System.Drawing.Point(0, 0);
            logWindow1.Name = "logWindow1";
            logWindow1.Size = new System.Drawing.Size(1438, 246);
            logWindow1.TabIndex = 0;
            // 
            // MainTabs
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1438, 899);
            Controls.Add(splitContainerControl1);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainTabs";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "永利系统 - 数据管理平台";
            FormClosing += MainTabs_FormClosing;
            Load += MainTabs_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1.Panel1).EndInit();
            splitContainerControl1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1.Panel2).EndInit();
            splitContainerControl1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerControl1).EndInit();
            splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)xtraTabControl1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem toolStripMenuItemFile;
        private ToolStripMenuItem toolStripMenuItemNew;
        private ToolStripMenuItem toolStripMenuItemOpen;
        private ToolStripMenuItem toolStripMenuItemSave;
        private ToolStripMenuItem toolStripMenuItemSaveAs;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem toolStripMenuItemExit;
        private ToolStripMenuItem toolStripMenuItemTools;
        private ToolStripMenuItem toolStripMenuItemOptions;
        private ToolStripMenuItem toolStripMenuItemHelp;
        private ToolStripMenuItem toolStripMenuItemAbout;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButtonRefresh;
        private ToolStripButton toolStripButtonSave;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolStripButtonLog;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton toolStripButtonWechatStart;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton toolStripButtonExit;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusStatus;
        private ToolStripStatusLabel toolStripStatusUser;
        private ToolStripStatusLabel toolStripStatusLog;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private LogWindow logWindow1;

        /// <summary>
        /// 创建刷新图标 (24x24)
        /// </summary>
        private System.Drawing.Image CreateRefreshIcon()
        {
            var bitmap = new System.Drawing.Bitmap(24, 24);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(System.Drawing.Color.Transparent);
                
                // 绘制圆形箭头（刷新图标）
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 120, 215), 2))
                {
                    // 绘制圆形
                    g.DrawArc(pen, 4, 4, 16, 16, 45, 270);
                    // 绘制箭头
                    g.DrawLine(pen, 6, 8, 2, 4);
                    g.DrawLine(pen, 6, 8, 10, 4);
                    g.DrawLine(pen, 18, 16, 22, 20);
                    g.DrawLine(pen, 18, 16, 14, 20);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 创建保存图标 (24x24)
        /// </summary>
        private System.Drawing.Image CreateSaveIcon()
        {
            var bitmap = new System.Drawing.Bitmap(24, 24);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(System.Drawing.Color.Transparent);
                
                // 绘制软盘图标（保存）
                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 120, 215)))
                {
                    // 软盘主体
                    g.FillRectangle(brush, 6, 4, 12, 16);
                    // 软盘标签
                    g.FillRectangle(System.Drawing.Brushes.White, 8, 6, 8, 4);
                    // 软盘中心孔
                    g.FillEllipse(System.Drawing.Brushes.White, 10, 12, 4, 4);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 创建日志图标 (24x24)
        /// </summary>
        private System.Drawing.Image CreateLogIcon()
        {
            var bitmap = new System.Drawing.Bitmap(24, 24);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(System.Drawing.Color.Transparent);
                
                // 绘制文档图标（日志）
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 120, 215), 2))
                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 120, 215)))
                {
                    // 文档主体
                    var path = new System.Drawing.Drawing2D.GraphicsPath();
                    path.AddLines(new System.Drawing.Point[] {
                        new System.Drawing.Point(4, 4),
                        new System.Drawing.Point(16, 4),
                        new System.Drawing.Point(20, 8),
                        new System.Drawing.Point(20, 20),
                        new System.Drawing.Point(4, 20)
                    });
                    path.CloseAllFigures();
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                    
                    // 文档折角
                    g.DrawLine(new System.Drawing.Pen(System.Drawing.Color.White, 2), 16, 4, 16, 8);
                    g.DrawLine(new System.Drawing.Pen(System.Drawing.Color.White, 2), 16, 8, 20, 8);
                    
                    // 绘制文本行
                    using (var textPen = new System.Drawing.Pen(System.Drawing.Color.White, 1.5f))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            g.DrawLine(textPen, 6, 11 + i * 3, 18, 11 + i * 3);
                        }
                    }
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 创建微信图标 (24x24)
        /// </summary>
        private System.Drawing.Image CreateWechatIcon()
        {
            var bitmap = new System.Drawing.Bitmap(24, 24);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(System.Drawing.Color.Transparent);
                
                // 绘制微信图标（绿色圆形 + 两个对话气泡）
                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(7, 193, 96)))
                {
                    // 左侧气泡
                    g.FillEllipse(brush, 2, 8, 8, 8);
                    // 右侧气泡
                    g.FillEllipse(brush, 14, 6, 8, 8);
                }
                
                // 绘制眼睛（两个小点）
                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
                {
                    g.FillEllipse(brush, 4, 10, 2, 2);
                    g.FillEllipse(brush, 16, 8, 2, 2);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 创建退出图标 (24x24)
        /// </summary>
        private System.Drawing.Image CreateExitIcon()
        {
            var bitmap = new System.Drawing.Bitmap(24, 24);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(System.Drawing.Color.Transparent);
                
                // 绘制门图标（退出）
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(220, 53, 69), 2))
                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(220, 53, 69)))
                {
                    // 门框
                    g.DrawRectangle(pen, 6, 4, 12, 16);
                    // 门把手
                    g.FillEllipse(brush, 16, 10, 2, 2);
                    // 门缝线
                    g.DrawLine(pen, 12, 4, 12, 20);
                }
            }
            return bitmap;
        }
    }
}
