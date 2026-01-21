using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using YongLiSystem.Models.Dashboard;

namespace YongLiSystem.Views.Dashboard.Controls
{
    /// <summary>
    /// 脚本任务卡片控件 - 简化版（不再显示缩略图）
    /// </summary>
    public partial class ScriptTaskCardControl : XtraUserControl
    {
        private ScriptTask? _task;

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
            labelLastRunTime.Text = _task.LastRunTime != DateTime.MinValue 
                ? $"上次运行: {_task.LastRunTime:HH:mm:ss}" 
                : "未运行";
            
            // 更新按钮状态
            if (btnStartStop != null)
            {
                if (_task.IsRunning)
                {
                    btnStartStop.Text = "■ 停止";
                    btnStartStop.Appearance.BackColor = System.Drawing.Color.FromArgb(192, 0, 0);
                }
                else
                {
                    btnStartStop.Text = "▶ 启动";
                    btnStartStop.Appearance.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
                }
            }
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
