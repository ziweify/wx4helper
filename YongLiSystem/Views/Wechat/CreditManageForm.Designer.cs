using System.Drawing;
using System.Windows.Forms;

namespace 永利系统.Views.Wechat
{
    partial class CreditManageForm
    {
        private System.ComponentModel.IContainer components = null;

        // 标题栏（可拖动区域）
        private DevExpress.XtraEditors.PanelControl panelControl_TitleBar;
        private DevExpress.XtraEditors.LabelControl labelControl_Title;
        private System.Windows.Forms.Label label_Minimize;
        private System.Windows.Forms.Label label_Close;

        // 工具栏区域
        private DevExpress.XtraEditors.PanelControl panelControl_Toolbar;
        private DevExpress.XtraEditors.LabelControl labelControl_StatusLabel;
        private DevExpress.XtraEditors.ComboBoxEdit comboBoxEdit_StatusFilter;
        private DevExpress.XtraEditors.SimpleButton simpleButton_Refresh;
        private DevExpress.XtraEditors.SimpleButton simpleButton_Export;

        // 主数据区域
        private DevExpress.XtraEditors.PanelControl panelControl_Main;
        private DevExpress.XtraGrid.GridControl gridControl_Requests;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_Requests;

        // 底部状态栏
        private DevExpress.XtraEditors.PanelControl panelControl_StatusBar;
        private DevExpress.XtraEditors.LabelControl labelControl_PendingCount;
        private DevExpress.XtraEditors.LabelControl labelControl_TodayCredit;
        private DevExpress.XtraEditors.LabelControl labelControl_TodayWithdraw;
        private DevExpress.XtraEditors.LabelControl labelControl_TotalBalance;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            
            this.panelControl_TitleBar = new DevExpress.XtraEditors.PanelControl();
            this.labelControl_Title = new DevExpress.XtraEditors.LabelControl();
            this.label_Minimize = new System.Windows.Forms.Label();
            this.label_Close = new System.Windows.Forms.Label();
            
            this.panelControl_Toolbar = new DevExpress.XtraEditors.PanelControl();
            this.labelControl_StatusLabel = new DevExpress.XtraEditors.LabelControl();
            this.comboBoxEdit_StatusFilter = new DevExpress.XtraEditors.ComboBoxEdit();
            this.simpleButton_Refresh = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton_Export = new DevExpress.XtraEditors.SimpleButton();
            
            this.panelControl_Main = new DevExpress.XtraEditors.PanelControl();
            this.gridControl_Requests = new DevExpress.XtraGrid.GridControl();
            this.gridView_Requests = new DevExpress.XtraGrid.Views.Grid.GridView();
            
            this.panelControl_StatusBar = new DevExpress.XtraEditors.PanelControl();
            this.labelControl_PendingCount = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_TodayCredit = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_TodayWithdraw = new DevExpress.XtraEditors.LabelControl();
            this.labelControl_TotalBalance = new DevExpress.XtraEditors.LabelControl();
            
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_TitleBar)).BeginInit();
            this.panelControl_TitleBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_Toolbar)).BeginInit();
            this.panelControl_Toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit_StatusFilter.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_Main)).BeginInit();
            this.panelControl_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_Requests)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_Requests)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_StatusBar)).BeginInit();
            this.panelControl_StatusBar.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // panelControl_TitleBar
            // 
            this.panelControl_TitleBar.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.panelControl_TitleBar.Appearance.Options.UseBackColor = true;
            this.panelControl_TitleBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl_TitleBar.Controls.Add(this.labelControl_Title);
            this.panelControl_TitleBar.Controls.Add(this.label_Minimize);
            this.panelControl_TitleBar.Controls.Add(this.label_Close);
            this.panelControl_TitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl_TitleBar.Location = new System.Drawing.Point(0, 0);
            this.panelControl_TitleBar.Name = "panelControl_TitleBar";
            this.panelControl_TitleBar.Size = new System.Drawing.Size(1200, 35);
            this.panelControl_TitleBar.TabIndex = 0;
            this.panelControl_TitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelControl_TitleBar_MouseDown);
            
            // 
            // labelControl_Title
            // 
            this.labelControl_Title.Appearance.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold);
            this.labelControl_Title.Appearance.ForeColor = System.Drawing.Color.White;
            this.labelControl_Title.Appearance.Options.UseFont = true;
            this.labelControl_Title.Appearance.Options.UseForeColor = true;
            this.labelControl_Title.Location = new System.Drawing.Point(15, 9);
            this.labelControl_Title.Name = "labelControl_Title";
            this.labelControl_Title.Size = new System.Drawing.Size(108, 19);
            this.labelControl_Title.TabIndex = 0;
            this.labelControl_Title.Text = "💰 上下分管理";
            this.labelControl_Title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelControl_TitleBar_MouseDown);
            
            // 
            // label_Minimize
            // 
            this.label_Minimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_Minimize.BackColor = System.Drawing.Color.Transparent;
            this.label_Minimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label_Minimize.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.label_Minimize.ForeColor = System.Drawing.Color.White;
            this.label_Minimize.Location = new System.Drawing.Point(1120, 0);
            this.label_Minimize.Name = "label_Minimize";
            this.label_Minimize.Size = new System.Drawing.Size(40, 35);
            this.label_Minimize.TabIndex = 1;
            this.label_Minimize.Text = "─";
            this.label_Minimize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Minimize.Click += new System.EventHandler(this.Label_Minimize_Click);
            this.label_Minimize.MouseEnter += new System.EventHandler(this.Label_Minimize_MouseEnter);
            this.label_Minimize.MouseLeave += new System.EventHandler(this.Label_Minimize_MouseLeave);
            
            // 
            // label_Close
            // 
            this.label_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_Close.BackColor = System.Drawing.Color.Transparent;
            this.label_Close.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label_Close.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.label_Close.ForeColor = System.Drawing.Color.White;
            this.label_Close.Location = new System.Drawing.Point(1160, 0);
            this.label_Close.Name = "label_Close";
            this.label_Close.Size = new System.Drawing.Size(40, 35);
            this.label_Close.TabIndex = 2;
            this.label_Close.Text = "✕";
            this.label_Close.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Close.Click += new System.EventHandler(this.Label_Close_Click);
            this.label_Close.MouseEnter += new System.EventHandler(this.Label_Close_MouseEnter);
            this.label_Close.MouseLeave += new System.EventHandler(this.Label_Close_MouseLeave);
            
            // 
            // panelControl_Toolbar
            // 
            this.panelControl_Toolbar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl_Toolbar.Controls.Add(this.labelControl_StatusLabel);
            this.panelControl_Toolbar.Controls.Add(this.comboBoxEdit_StatusFilter);
            this.panelControl_Toolbar.Controls.Add(this.simpleButton_Refresh);
            this.panelControl_Toolbar.Controls.Add(this.simpleButton_Export);
            this.panelControl_Toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl_Toolbar.Location = new System.Drawing.Point(0, 35);
            this.panelControl_Toolbar.Name = "panelControl_Toolbar";
            this.panelControl_Toolbar.Size = new System.Drawing.Size(1200, 60);
            this.panelControl_Toolbar.TabIndex = 1;
            
            // 
            // labelControl_StatusLabel
            // 
            this.labelControl_StatusLabel.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.labelControl_StatusLabel.Appearance.Options.UseFont = true;
            this.labelControl_StatusLabel.Location = new System.Drawing.Point(20, 20);
            this.labelControl_StatusLabel.Name = "labelControl_StatusLabel";
            this.labelControl_StatusLabel.Size = new System.Drawing.Size(42, 20);
            this.labelControl_StatusLabel.TabIndex = 0;
            this.labelControl_StatusLabel.Text = "状态:";
            
            // 
            // comboBoxEdit_StatusFilter
            // 
            this.comboBoxEdit_StatusFilter.Location = new System.Drawing.Point(70, 18);
            this.comboBoxEdit_StatusFilter.Name = "comboBoxEdit_StatusFilter";
            this.comboBoxEdit_StatusFilter.Properties.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.comboBoxEdit_StatusFilter.Properties.Appearance.Options.UseFont = true;
            this.comboBoxEdit_StatusFilter.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxEdit_StatusFilter.Properties.DropDownRows = 5;
            this.comboBoxEdit_StatusFilter.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.comboBoxEdit_StatusFilter.Size = new System.Drawing.Size(150, 24);
            this.comboBoxEdit_StatusFilter.TabIndex = 1;
            this.comboBoxEdit_StatusFilter.SelectedIndexChanged += new System.EventHandler(this.ComboBoxEdit_StatusFilter_SelectedIndexChanged);
            
            // 
            // simpleButton_Refresh
            // 
            this.simpleButton_Refresh.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.simpleButton_Refresh.Appearance.Options.UseFont = true;
            this.simpleButton_Refresh.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
            this.simpleButton_Refresh.Location = new System.Drawing.Point(240, 15);
            this.simpleButton_Refresh.Name = "simpleButton_Refresh";
            this.simpleButton_Refresh.Size = new System.Drawing.Size(90, 30);
            this.simpleButton_Refresh.TabIndex = 2;
            this.simpleButton_Refresh.Text = "🔄 刷新";
            this.simpleButton_Refresh.Click += new System.EventHandler(this.SimpleButton_Refresh_Click);
            
            // 
            // simpleButton_Export
            // 
            this.simpleButton_Export.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.simpleButton_Export.Appearance.Options.UseFont = true;
            this.simpleButton_Export.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
            this.simpleButton_Export.Location = new System.Drawing.Point(350, 15);
            this.simpleButton_Export.Name = "simpleButton_Export";
            this.simpleButton_Export.Size = new System.Drawing.Size(90, 30);
            this.simpleButton_Export.TabIndex = 3;
            this.simpleButton_Export.Text = "📁 导出";
            
            // 
            // panelControl_Main
            // 
            this.panelControl_Main.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl_Main.Controls.Add(this.gridControl_Requests);
            this.panelControl_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl_Main.Location = new System.Drawing.Point(0, 95);
            this.panelControl_Main.Name = "panelControl_Main";
            this.panelControl_Main.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.panelControl_Main.Size = new System.Drawing.Size(1200, 555);
            this.panelControl_Main.TabIndex = 2;
            
            // 
            // gridControl_Requests
            // 
            this.gridControl_Requests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl_Requests.Location = new System.Drawing.Point(15, 10);
            this.gridControl_Requests.MainView = this.gridView_Requests;
            this.gridControl_Requests.Name = "gridControl_Requests";
            this.gridControl_Requests.Size = new System.Drawing.Size(1170, 520);
            this.gridControl_Requests.TabIndex = 0;
            this.gridControl_Requests.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_Requests});
            
            // 
            // gridView_Requests
            // 
            this.gridView_Requests.Appearance.HeaderPanel.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.gridView_Requests.Appearance.HeaderPanel.Options.UseFont = true;
            this.gridView_Requests.Appearance.HeaderPanel.Options.UseTextOptions = true;
            this.gridView_Requests.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridView_Requests.Appearance.Row.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.gridView_Requests.Appearance.Row.Options.UseFont = true;
            this.gridView_Requests.GridControl = this.gridControl_Requests;
            this.gridView_Requests.Name = "gridView_Requests";
            this.gridView_Requests.OptionsBehavior.Editable = false;
            this.gridView_Requests.OptionsBehavior.ReadOnly = true;
            this.gridView_Requests.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gridView_Requests.OptionsView.EnableAppearanceEvenRow = true;
            this.gridView_Requests.OptionsView.ShowGroupPanel = false;
            this.gridView_Requests.RowHeight = 35;
            
            // 
            // panelControl_StatusBar
            // 
            this.panelControl_StatusBar.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panelControl_StatusBar.Appearance.Options.UseBackColor = true;
            this.panelControl_StatusBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl_StatusBar.Controls.Add(this.labelControl_PendingCount);
            this.panelControl_StatusBar.Controls.Add(this.labelControl_TodayCredit);
            this.panelControl_StatusBar.Controls.Add(this.labelControl_TodayWithdraw);
            this.panelControl_StatusBar.Controls.Add(this.labelControl_TotalBalance);
            this.panelControl_StatusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelControl_StatusBar.Location = new System.Drawing.Point(0, 650);
            this.panelControl_StatusBar.Name = "panelControl_StatusBar";
            this.panelControl_StatusBar.Size = new System.Drawing.Size(1200, 50);
            this.panelControl_StatusBar.TabIndex = 3;
            
            // 
            // labelControl_PendingCount
            // 
            this.labelControl_PendingCount.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.labelControl_PendingCount.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(79)))));
            this.labelControl_PendingCount.Appearance.Options.UseFont = true;
            this.labelControl_PendingCount.Appearance.Options.UseForeColor = true;
            this.labelControl_PendingCount.Location = new System.Drawing.Point(20, 15);
            this.labelControl_PendingCount.Name = "labelControl_PendingCount";
            this.labelControl_PendingCount.Size = new System.Drawing.Size(98, 19);
            this.labelControl_PendingCount.TabIndex = 0;
            this.labelControl_PendingCount.Text = "⏳ 待处理: 0 笔";
            
            // 
            // labelControl_TodayCredit
            // 
            this.labelControl_TodayCredit.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.labelControl_TodayCredit.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(207)))), ((int)(((byte)(124)))));
            this.labelControl_TodayCredit.Appearance.Options.UseFont = true;
            this.labelControl_TodayCredit.Appearance.Options.UseForeColor = true;
            this.labelControl_TodayCredit.Location = new System.Drawing.Point(200, 15);
            this.labelControl_TodayCredit.Name = "labelControl_TodayCredit";
            this.labelControl_TodayCredit.Size = new System.Drawing.Size(127, 19);
            this.labelControl_TodayCredit.TabIndex = 1;
            this.labelControl_TodayCredit.Text = "⬆️ 今日上分: 0.00";
            
            // 
            // labelControl_TodayWithdraw
            // 
            this.labelControl_TodayWithdraw.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.labelControl_TodayWithdraw.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.labelControl_TodayWithdraw.Appearance.Options.UseFont = true;
            this.labelControl_TodayWithdraw.Appearance.Options.UseForeColor = true;
            this.labelControl_TodayWithdraw.Location = new System.Drawing.Point(420, 15);
            this.labelControl_TodayWithdraw.Name = "labelControl_TodayWithdraw";
            this.labelControl_TodayWithdraw.Size = new System.Drawing.Size(127, 19);
            this.labelControl_TodayWithdraw.TabIndex = 2;
            this.labelControl_TodayWithdraw.Text = "⬇️ 今日下分: 0.00";
            
            // 
            // labelControl_TotalBalance
            // 
            this.labelControl_TotalBalance.Appearance.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.labelControl_TotalBalance.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.labelControl_TotalBalance.Appearance.Options.UseFont = true;
            this.labelControl_TotalBalance.Appearance.Options.UseForeColor = true;
            this.labelControl_TotalBalance.Location = new System.Drawing.Point(640, 15);
            this.labelControl_TotalBalance.Name = "labelControl_TotalBalance";
            this.labelControl_TotalBalance.Size = new System.Drawing.Size(127, 19);
            this.labelControl_TotalBalance.TabIndex = 3;
            this.labelControl_TotalBalance.Text = "💰 总余额: 0.00";
            
            // 
            // CreditManageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.panelControl_Main);
            this.Controls.Add(this.panelControl_StatusBar);
            this.Controls.Add(this.panelControl_Toolbar);
            this.Controls.Add(this.panelControl_TitleBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CreditManageForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "上下分管理";
            
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_TitleBar)).EndInit();
            this.panelControl_TitleBar.ResumeLayout(false);
            this.panelControl_TitleBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_Toolbar)).EndInit();
            this.panelControl_Toolbar.ResumeLayout(false);
            this.panelControl_Toolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit_StatusFilter.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_Main)).EndInit();
            this.panelControl_Main.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_Requests)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_Requests)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl_StatusBar)).EndInit();
            this.panelControl_StatusBar.ResumeLayout(false);
            this.panelControl_StatusBar.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}

