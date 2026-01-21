using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Unit.La.Models;

namespace Unit.La.Controls
{
    /// <summary>
    /// 浏览器任务卡片控件 - 显示缩略图、状态和操作按钮
    /// 通用组件，可在任何项目中使用，配合 BrowserTaskControl
    /// </summary>
    public partial class BrowserTaskCardControl : XtraUserControl
    {
        private BrowserTaskInfo? _taskInfo;
        private Image? _defaultThumbnail;

        /// <summary>
        /// 删除按钮点击事件
        /// </summary>
        public event EventHandler? DeleteClicked;

        /// <summary>
        /// 启动/停止按钮点击事件
        /// </summary>
        public event EventHandler? StartStopClicked;

        /// <summary>
        /// 关闭按钮点击事件（释放资源）
        /// </summary>
        public event EventHandler? CloseClicked;

        /// <summary>
        /// 缩略图点击事件（显示隐藏的窗口）
        /// </summary>
        public event EventHandler? ThumbnailClicked;

        /// <summary>
        /// 获取或设置任务信息
        /// </summary>
        public BrowserTaskInfo? TaskInfo
        {
            get => _taskInfo;
            set
            {
                _taskInfo = value;
                UpdateUI();
            }
        }

        public BrowserTaskCardControl()
        {
            InitializeComponent();
            InitializeDefaultThumbnail();
        }

        /// <summary>
        /// 初始化默认缩略图（黑色背景+提示文字）
        /// </summary>
        private void InitializeDefaultThumbnail()
        {
            _defaultThumbnail = new Bitmap(280, 150);
            using (var g = Graphics.FromImage(_defaultThumbnail))
            {
                g.Clear(Color.FromArgb(30, 30, 30));
                var text = "未启动";
                var font = new Font("Microsoft YaHei", 14, FontStyle.Bold);
                var brush = new SolidBrush(Color.Gray);
                var size = g.MeasureString(text, font);
                var x = (280 - size.Width) / 2;
                var y = (150 - size.Height) / 2;
                g.DrawString(text, font, brush, x, y);
            }
            pictureBoxThumbnail.Image = _defaultThumbnail;
        }

        /// <summary>
        /// 更新缩略图
        /// </summary>
        public void UpdateThumbnail(Image thumbnail)
        {
            if (pictureBoxThumbnail.InvokeRequired)
            {
                pictureBoxThumbnail.Invoke(new Action(() => UpdateThumbnail(thumbnail)));
                return;
            }

            if (pictureBoxThumbnail.Image != null && pictureBoxThumbnail.Image != _defaultThumbnail)
            {
                pictureBoxThumbnail.Image.Dispose();
            }
            pictureBoxThumbnail.Image = thumbnail;
        }

        /// <summary>
        /// 重置为默认缩略图
        /// </summary>
        public void ResetThumbnail()
        {
            if (pictureBoxThumbnail.Image != null && pictureBoxThumbnail.Image != _defaultThumbnail)
            {
                pictureBoxThumbnail.Image.Dispose();
            }
            pictureBoxThumbnail.Image = _defaultThumbnail;
        }

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (_taskInfo == null) return;

            labelTaskName.Text = _taskInfo.Name;
            labelUrl.Text = _taskInfo.Url.Length > 30 ? _taskInfo.Url.Substring(0, 27) + "..." : _taskInfo.Url;
            labelStatus.Text = _taskInfo.Status;
            labelLastRunTime.Text = _taskInfo.LastRunTime != DateTime.MinValue 
                ? $"上次运行: {_taskInfo.LastRunTime:HH:mm:ss}" 
                : "未运行";
            
            // 更新状态颜色
            labelStatus.Appearance.ForeColor = _taskInfo.IsRunning 
                ? Color.Green 
                : Color.Gray;
            
            // 更新按钮状态
            if (btnStartStop != null)
            {
                if (_taskInfo.IsRunning)
                {
                    btnStartStop.Text = "■ 停止";
                    btnStartStop.Appearance.BackColor = Color.FromArgb(192, 0, 0);
                    btnClose.Visible = true; // 运行中显示关闭按钮
                }
                else
                {
                    btnStartStop.Text = "▶ 启动";
                    btnStartStop.Appearance.BackColor = Color.FromArgb(0, 192, 0);
                    btnClose.Visible = false; // 未运行时隐藏关闭按钮
                    ResetThumbnail(); // 停止时重置缩略图
                }
            }
        }

        private void OnDeleteButtonClick(object? sender, EventArgs e)
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }

        private void OnStartStopButtonClick(object? sender, EventArgs e)
        {
            StartStopClicked?.Invoke(this, EventArgs.Empty);
        }

        private void OnCloseButtonClick(object? sender, EventArgs e)
        {
            CloseClicked?.Invoke(this, EventArgs.Empty);
        }

        private void OnThumbnailClick(object? sender, EventArgs e)
        {
            ThumbnailClicked?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (pictureBoxThumbnail.Image != null && pictureBoxThumbnail.Image != _defaultThumbnail)
                {
                    pictureBoxThumbnail.Image.Dispose();
                }
                _defaultThumbnail?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
