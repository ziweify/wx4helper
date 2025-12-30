using System.Drawing;
using System.Windows.Forms;

namespace Unit.Browser.Controls
{
    partial class BrowserWindow
    {
        private System.ComponentModel.IContainer components = null;

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
            _splitContainer = new SplitContainer();
            _pnlTop = new Panel();
            _pnlBrowser = new Panel();
            _pnlLog = new Panel();
            _txtUrl = new TextBox();
            _btnNavigate = new Button();
            _btnRefresh = new Button();
            _txtLog = new RichTextBox();
            _btnClearLog = new Button();
            _chkAutoScroll = new CheckBox();
            _statusStrip = new StatusStrip();
            _lblStatus = new ToolStripStatusLabel();

            ((System.ComponentModel.ISupportInitialize)_splitContainer).BeginInit();
            _splitContainer.Panel1.SuspendLayout();
            _splitContainer.Panel2.SuspendLayout();
            _splitContainer.SuspendLayout();
            _pnlTop.SuspendLayout();
            _pnlLog.SuspendLayout();
            _statusStrip.SuspendLayout();
            SuspendLayout();
            
            // 
            // _statusStrip
            // 
            _statusStrip.Items.AddRange(new ToolStripItem[] { _lblStatus });
            _statusStrip.Location = new Point(0, 858);
            _statusStrip.Name = "_statusStrip";
            _statusStrip.Size = new Size(1400, 22);
            _statusStrip.TabIndex = 2;
            
            // 
            // _lblStatus
            // 
            _lblStatus.Name = "_lblStatus";
            _lblStatus.Size = new Size(44, 17);
            _lblStatus.Text = "初始化中...";
            
            // 
            // _pnlTop
            // 
            _pnlTop.Controls.Add(_txtUrl);
            _pnlTop.Controls.Add(_btnNavigate);
            _pnlTop.Controls.Add(_btnRefresh);
            _pnlTop.Dock = DockStyle.Top;
            _pnlTop.Location = new Point(0, 0);
            _pnlTop.Name = "_pnlTop";
            _pnlTop.Size = new Size(1400, 40);
            _pnlTop.TabIndex = 0;
            
            // 
            // _txtUrl
            // 
            _txtUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtUrl.Location = new Point(12, 8);
            _txtUrl.Name = "_txtUrl";
            _txtUrl.Size = new Size(1235, 23);
            _txtUrl.TabIndex = 0;
            
            // 
            // _btnNavigate
            // 
            _btnNavigate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnNavigate.Location = new Point(1253, 7);
            _btnNavigate.Name = "_btnNavigate";
            _btnNavigate.Size = new Size(60, 25);
            _btnNavigate.TabIndex = 1;
            _btnNavigate.Text = "转到";
            _btnNavigate.UseVisualStyleBackColor = true;
            _btnNavigate.Click += BtnNavigate_Click;
            
            // 
            // _btnRefresh
            // 
            _btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnRefresh.Location = new Point(1319, 7);
            _btnRefresh.Name = "_btnRefresh";
            _btnRefresh.Size = new Size(60, 25);
            _btnRefresh.TabIndex = 2;
            _btnRefresh.Text = "刷新";
            _btnRefresh.UseVisualStyleBackColor = true;
            _btnRefresh.Click += BtnRefresh_Click;
            
            // 
            // _splitContainer
            // 
            _splitContainer.Dock = DockStyle.Fill;
            _splitContainer.Location = new Point(0, 40);
            _splitContainer.Name = "_splitContainer";
            _splitContainer.Orientation = Orientation.Horizontal;
            
            // 
            // _splitContainer.Panel1
            // 
            _splitContainer.Panel1.Controls.Add(_pnlBrowser);
            
            // 
            // _splitContainer.Panel2
            // 
            _splitContainer.Panel2.Controls.Add(_pnlLog);
            _splitContainer.Size = new Size(1400, 818);
            _splitContainer.SplitterDistance = 573;
            _splitContainer.TabIndex = 1;
            
            // 
            // _pnlBrowser
            // 
            _pnlBrowser.Dock = DockStyle.Fill;
            _pnlBrowser.Location = new Point(0, 0);
            _pnlBrowser.Name = "_pnlBrowser";
            _pnlBrowser.Size = new Size(1400, 573);
            _pnlBrowser.TabIndex = 0;
            
            // 
            // _pnlLog
            // 
            _pnlLog.Controls.Add(_txtLog);
            _pnlLog.Controls.Add(_btnClearLog);
            _pnlLog.Controls.Add(_chkAutoScroll);
            _pnlLog.Dock = DockStyle.Fill;
            _pnlLog.Location = new Point(0, 0);
            _pnlLog.Name = "_pnlLog";
            _pnlLog.Size = new Size(1400, 241);
            _pnlLog.TabIndex = 0;
            
            // 
            // _txtLog
            // 
            _txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _txtLog.BackColor = Color.Black;
            _txtLog.Font = new Font("Consolas", 9F);
            _txtLog.ForeColor = Color.Lime;
            _txtLog.Location = new Point(0, 0);
            _txtLog.Name = "_txtLog";
            _txtLog.ReadOnly = true;
            _txtLog.Size = new Size(1400, 210);
            _txtLog.TabIndex = 0;
            _txtLog.Text = "";
            _txtLog.WordWrap = false;
            
            // 
            // _chkAutoScroll
            // 
            _chkAutoScroll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _chkAutoScroll.AutoSize = true;
            _chkAutoScroll.Checked = true;
            _chkAutoScroll.CheckState = CheckState.Checked;
            _chkAutoScroll.Location = new Point(12, 216);
            _chkAutoScroll.Name = "_chkAutoScroll";
            _chkAutoScroll.Size = new Size(75, 19);
            _chkAutoScroll.TabIndex = 1;
            _chkAutoScroll.Text = "自动滚动";
            _chkAutoScroll.UseVisualStyleBackColor = true;
            
            // 
            // _btnClearLog
            // 
            _btnClearLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnClearLog.Location = new Point(1307, 213);
            _btnClearLog.Name = "_btnClearLog";
            _btnClearLog.Size = new Size(75, 23);
            _btnClearLog.TabIndex = 2;
            _btnClearLog.Text = "清空日志";
            _btnClearLog.UseVisualStyleBackColor = true;
            _btnClearLog.Click += BtnClearLog_Click;
            
            // 
            // BrowserWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 880);
            Controls.Add(_splitContainer);
            Controls.Add(_pnlTop);
            Controls.Add(_statusStrip);
            Name = "BrowserWindow";
            Text = "浏览器窗口";
            
            _splitContainer.Panel1.ResumeLayout(false);
            _splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_splitContainer).EndInit();
            _splitContainer.ResumeLayout(false);
            _pnlTop.ResumeLayout(false);
            _pnlTop.PerformLayout();
            _pnlLog.ResumeLayout(false);
            _pnlLog.PerformLayout();
            _statusStrip.ResumeLayout(false);
            _statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}

