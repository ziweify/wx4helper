using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unit.Browser.Interfaces;
using Unit.Browser.Models;
using Unit.Browser.Services;
using Microsoft.Web.WebView2.WinForms;

namespace Unit.Browser.Controls
{
    /// <summary>
    /// 独立浏览器窗口（运行在独立线程）
    /// </summary>
    public partial class BrowserWindow : Form
    {
        private WebView2? _webView;
        private Interfaces.ICommandExecutor? _commandExecutor;
        private Func<BrowserCommand, Task>? _commandHandler;
        private readonly string _initialUrl;

        // UI 控件
        private SplitContainer? _splitContainer;
        private Panel? _pnlTop;
        private Panel? _pnlBrowser;
        private Panel? _pnlLog;
        private TextBox? _txtUrl;
        private Button? _btnNavigate;
        private Button? _btnRefresh;
        private RichTextBox? _txtLog;
        private Button? _btnClearLog;
        private CheckBox? _chkAutoScroll;
        private StatusStrip? _statusStrip;
        private ToolStripStatusLabel? _lblStatus;

        public event EventHandler<string>? OnLog;

        public BrowserWindow(string title, string initialUrl)
        {
            _initialUrl = initialUrl;
            
            InitializeComponent();
            
            this.Text = title;
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // 异步初始化 WebView2
            this.Load += async (s, e) => await InitializeWebViewAsync();
        }

        /// <summary>
        /// 设置命令处理器
        /// </summary>
        public void SetCommandHandler(Func<BrowserCommand, Task> handler)
        {
            _commandHandler = handler;
        }

        /// <summary>
        /// 初始化 WebView2
        /// </summary>
        private async Task InitializeWebViewAsync()
        {
            try
            {
                LogMessage("🚀 正在初始化 WebView2...");

                _webView = new WebView2
                {
                    Dock = DockStyle.Fill
                };

                _pnlBrowser?.Controls.Add(_webView);

                await _webView.EnsureCoreWebView2Async(null);

                // 初始化命令执行器
                _commandExecutor = new CommandExecutor();
                _commandExecutor.SetWebView(_webView);

                // 导航到初始URL
                if (!string.IsNullOrWhiteSpace(_initialUrl))
                {
                    _webView.CoreWebView2.Navigate(_initialUrl);
                    if (_txtUrl != null)
                    {
                        _txtUrl.Text = _initialUrl;
                    }
                }

                // 订阅导航完成事件
                _webView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    LogMessage($"✅ 导航完成: {_webView.CoreWebView2.Source}");
                    UpdateStatus("就绪");
                };

                LogMessage("✅ WebView2 初始化成功");
                UpdateStatus("就绪");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ WebView2 初始化失败: {ex.Message}");
                UpdateStatus($"初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        public async Task<BrowserCommandResult> ExecuteCommandAsync(BrowserCommand command)
        {
            if (_commandExecutor == null)
            {
                return BrowserCommandResult.CreateFailure(
                    command.CommandId,
                    "命令执行器未初始化");
            }

            LogMessage($"📤 执行命令: {command.Name}");
            UpdateStatus($"执行中: {command.Name}");

            var result = await _commandExecutor.ExecuteAsync(command);

            if (result.Success)
            {
                LogMessage($"✅ 命令成功: {command.Name} (耗时: {result.ExecutionTimeMs}ms)");
            }
            else
            {
                LogMessage($"❌ 命令失败: {command.Name} - {result.ErrorMessage}");
            }

            UpdateStatus("就绪");
            return result;
        }

        /// <summary>
        /// 辅助方法：在窗口线程中执行操作
        /// </summary>
        public async Task InvokeAsync(Func<Task> action)
        {
            if (InvokeRequired)
            {
                await Task.Run(() => Invoke(async () => await action()));
            }
            else
            {
                await action();
            }
        }

        #region UI 事件处理

        private void BtnNavigate_Click(object? sender, EventArgs e)
        {
            if (_webView?.CoreWebView2 != null && !string.IsNullOrWhiteSpace(_txtUrl?.Text))
            {
                _webView.CoreWebView2.Navigate(_txtUrl.Text);
                LogMessage($"🌐 导航到: {_txtUrl.Text}");
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            if (_webView?.CoreWebView2 != null)
            {
                _webView.CoreWebView2.Reload();
                LogMessage("🔄 刷新页面");
            }
        }

        private void BtnClearLog_Click(object? sender, EventArgs e)
        {
            _txtLog?.Clear();
            LogMessage("🗑️ 日志已清空");
        }

        #endregion

        #region 日志系统

        private void LogMessage(string message)
        {
            var logLine = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            
            // 输出到窗口日志
            if (_txtLog != null)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(() => AppendLog(logLine));
                }
                else
                {
                    AppendLog(logLine);
                }
            }

            // 触发日志事件（传递给代理）
            OnLog?.Invoke(this, message);
        }

        private void AppendLog(string logLine)
        {
            if (_txtLog == null) return;

            _txtLog.AppendText(logLine + Environment.NewLine);

            // 限制日志行数
            if (_txtLog.Lines.Length > 1000)
            {
                var lines = _txtLog.Lines;
                _txtLog.Lines = lines[^500..]; // 保留最后500行
            }

            // 自动滚动
            if (_chkAutoScroll?.Checked ?? true)
            {
                _txtLog.SelectionStart = _txtLog.Text.Length;
                _txtLog.ScrollToCaret();
            }
        }

        private void UpdateStatus(string status)
        {
            if (_lblStatus != null)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(() => _lblStatus.Text = status);
                }
                else
                {
                    _lblStatus.Text = status;
                }
            }
        }

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            LogMessage("🔚 浏览器窗口正在关闭...");
            
            if (_webView != null)
            {
                _webView.Dispose();
                _webView = null;
            }

            base.OnFormClosing(e);
        }
    }
}

