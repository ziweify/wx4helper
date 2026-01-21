using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Unit.Browser.Controls;
using YongLiSystem.Models.Dashboard;

namespace YongLiSystem.Views.Dashboard.Controls
{
    /// <summary>
    /// 脚本任务卡片控件 - 显示任务信息和浏览器缩略图
    /// </summary>
    public partial class ScriptTaskCardControl : XtraUserControl
    {
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        private ScriptTask? _task;
        private BrowserWindowProxy? _browserProxy;
        private System.Windows.Forms.Timer? _thumbnailTimer;

        public event EventHandler? EditClicked;
        public event EventHandler? DeleteClicked;
        public event EventHandler? StartStopClicked;

        public ScriptTask? Task
        {
            get => _task;
            set
            {
                _task = value;
                UpdateUI();
            }
        }

        public ScriptTaskCardControl()
        {
            InitializeComponent();
            InitializeThumbnailTimer();
        }

        /// <summary>
        /// 设置浏览器窗口代理
        /// </summary>
        public void SetBrowserProxy(BrowserWindowProxy? proxy)
        {
            _browserProxy = proxy;
        }

        /// <summary>
        /// 初始化缩略图更新定时器
        /// </summary>
        private void InitializeThumbnailTimer()
        {
            _thumbnailTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // 1秒更新一次
            };
            _thumbnailTimer.Tick += OnThumbnailTimerTick;
        }

        /// <summary>
        /// 定时器事件 - 每秒更新浏览器缩略图
        /// </summary>
        private void OnThumbnailTimerTick(object? sender, EventArgs e)
        {
            if (_browserProxy != null && pictureBoxThumbnail != null)
            {
                try
                {
                    var thumbnail = CaptureBrowserWindow();
                    if (thumbnail != null)
                    {
                        // 释放旧图片
                        var oldImage = pictureBoxThumbnail.Image;
                        pictureBoxThumbnail.Image = thumbnail;
                        oldImage?.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    // 忽略截图错误
                    System.Diagnostics.Debug.WriteLine($"截图失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 捕获浏览器窗口截图
        /// </summary>
        private Bitmap? CaptureBrowserWindow()
        {
            if (_browserProxy?.WindowHandle == IntPtr.Zero)
                return null;

            try
            {
                // 获取窗口句柄
                var handle = _browserProxy.WindowHandle;
                if (handle == IntPtr.Zero)
                    return null;

                // 创建与pictureBox大小匹配的位图
                var width = pictureBoxThumbnail.Width;
                var height = pictureBoxThumbnail.Height;

                if (width <= 0 || height <= 0)
                    return null;

                var bitmap = new Bitmap(width, height);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    var hdc = graphics.GetHdc();
                    try
                    {
                        PrintWindow(handle, hdc, 0);
                    }
                    finally
                    {
                        graphics.ReleaseHdc(hdc);
                    }
                }

                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (_task == null) return;

            labelTaskName.Text = _task.Name;
            labelUrl.Text = _task.Url;
            labelStatus.Text = _task.Status;
            
            // 更新按钮状态
            if (btnStartStop != null)
            {
                if (_task.IsRunning)
                {
                    btnStartStop.Text = "■ 停止";
                    btnStartStop.Appearance.BackColor = Color.FromArgb(192, 0, 0);
                }
                else
                {
                    btnStartStop.Text = "▶ 启动";
                    btnStartStop.Appearance.BackColor = Color.FromArgb(0, 192, 0);
                }
            }

            // 根据运行状态控制定时器
            if (_task.IsRunning)
            {
                _thumbnailTimer?.Start();
            }
            else
            {
                _thumbnailTimer?.Stop();
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _thumbnailTimer?.Stop();
                _thumbnailTimer?.Dispose();
                pictureBoxThumbnail?.Image?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void OnEditButtonClick(object? sender, EventArgs e)
        {
            EditClicked?.Invoke(this, EventArgs.Empty);
        }

        private void OnDeleteButtonClick(object? sender, EventArgs e)
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }

        private void OnStartStopButtonClick(object? sender, EventArgs e)
        {
            StartStopClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
