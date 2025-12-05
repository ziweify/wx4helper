using System;
using System.Windows.Forms;
using Sunny.UI;

namespace BaiShengVx3Plus.Views
{
    /// <summary>
    /// 非模态的数据错误通知窗口（单例模式）
    /// 用于显示 DataGridView 数据错误，不阻塞主程序
    /// </summary>
    public partial class DataErrorNotifyForm : UIForm
    {
        private static DataErrorNotifyForm? _instance;
        private static readonly object _lock = new object();
        
        private UIRichTextBox txtErrors;
        private UIButton btnClose;
        private UIButton btnClear;
        private int _errorCount = 0;

        private DataErrorNotifyForm()
        {
            InitializeComponents();
        }

        /// <summary>
        /// 获取或创建单例实例
        /// </summary>
        public static DataErrorNotifyForm Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null || _instance.IsDisposed)
                    {
                        _instance = new DataErrorNotifyForm();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// 显示错误信息（非模态）
        /// </summary>
        public static void ShowError(string source, string message)
        {
            var form = Instance;
            
            // 确保在 UI 线程执行
            if (form.InvokeRequired)
            {
                form.BeginInvoke(new Action(() => ShowErrorInternal(form, source, message)));
            }
            else
            {
                ShowErrorInternal(form, source, message);
            }
        }

        private static void ShowErrorInternal(DataErrorNotifyForm form, string source, string message)
        {
            form._errorCount++;
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string errorText = $"[{timestamp}] [{source}] {message}\r\n";
            
            form.txtErrors.AppendText(errorText);
            form.txtErrors.ScrollToCaret();
            form.Text = $"数据错误通知 ({form._errorCount})";
            
            // 如果窗口未显示，则显示（非模态）
            if (!form.Visible)
            {
                form.Show();
            }
            
            // 将窗口置于前台（但不抢夺焦点）
            form.TopMost = true;
            form.TopMost = false;
        }

        private void InitializeComponents()
        {
            this.Text = "数据错误通知";
            this.Size = new System.Drawing.Size(500, 300);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(
                Screen.PrimaryScreen.WorkingArea.Width - 520,
                Screen.PrimaryScreen.WorkingArea.Height - 320);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.ShowInTaskbar = false;
            this.MinimumSize = new System.Drawing.Size(300, 150);

            // 错误文本框
            txtErrors = new UIRichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
                ForeColor = System.Drawing.Color.FromArgb(255, 100, 100),
                Font = new System.Drawing.Font("Consolas", 9F)
            };

            // 底部按钮面板
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            btnClear = new UIButton
            {
                Text = "清空",
                Size = new System.Drawing.Size(80, 30),
                Location = new System.Drawing.Point(10, 5)
            };
            btnClear.Click += (s, e) =>
            {
                txtErrors.Clear();
                _errorCount = 0;
                Text = "数据错误通知";
            };

            btnClose = new UIButton
            {
                Text = "关闭",
                Size = new System.Drawing.Size(80, 30),
                Location = new System.Drawing.Point(100, 5)
            };
            btnClose.Click += (s, e) => this.Hide();

            pnlBottom.Controls.Add(btnClear);
            pnlBottom.Controls.Add(btnClose);

            this.Controls.Add(txtErrors);
            this.Controls.Add(pnlBottom);

            // 关闭时只隐藏，不销毁
            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            };
        }
    }
}

