using System;
using System.Drawing;
using System.Windows.Forms;

namespace 永利系统.Views
{
    /// <summary>
    /// 浮动日志窗口 - 可以从主窗口分离的独立日志窗口
    /// </summary>
    public partial class FloatingLogWindow : Form
    {
        private readonly LogWindow _logWindow;
        private readonly Action? _onClosing;
        private bool _isAttaching = false; // 防止循环调用的标志位

        public LogWindow LogWindowControl => _logWindow;

        public FloatingLogWindow(LogWindow logWindow, Action? onClosing = null)
        {
            _logWindow = logWindow;
            _onClosing = onClosing;

            InitializeComponent();
            
            // 设置窗口属性
            this.Text = "📋 日志查看器";
            this.Size = new Size(1200, 600);
            this.MinimumSize = new Size(800, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = true;
            this.Icon = this.Owner?.Icon; // 使用主窗口的图标
            
            // 添加 LogWindow 控件
            _logWindow.Dock = DockStyle.Fill;
            this.Controls.Add(_logWindow);
            
            // 窗口关闭事件
            this.FormClosing += FloatingLogWindow_FormClosing;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // FloatingLogWindow
            this.AutoScaleDimensions = new SizeF(7F, 14F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 600);
            this.Name = "FloatingLogWindow";
            this.Text = "日志查看器";
            
            this.ResumeLayout(false);
        }

        private void FloatingLogWindow_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 如果是附加操作触发的关闭，不再重复调用
            if (_isAttaching)
            {
                return;
            }

            // 通知主窗口，日志窗口即将关闭（附加回主窗口）
            _onClosing?.Invoke();
        }

        /// <summary>
        /// 标记为正在附加，防止 FormClosing 事件重复调用
        /// </summary>
        public void MarkAsAttaching()
        {
            _isAttaching = true;
        }
    }
}

