using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Unit.Browser.Controls;
using YongLiSystem.Services;
using YongLiSystem.Models.Dashboard;

namespace YongLiSystem.Views.Dashboard.Monitors
{
    /// <summary>
    /// 监控控件基类 - 使用独立BrowserWindow并嵌入到控件中
    /// </summary>
    public abstract class MonitorControlBase : XtraUserControl
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        protected readonly LoggingService _loggingService;
        protected BrowserWindowProxy? _browserProxy;
        protected CancellationTokenSource? _monitoringCts;
        protected MonitorConfig? _config;
        private bool _isInitialized = false;
        private Panel? _browserPanel; // 用于承载嵌入的浏览器窗口

        protected abstract string MonitorName { get; }

        public MonitorControlBase()
        {
            _loggingService = LoggingService.Instance;
        }

        /// <summary>
        /// 初始化UI - 创建一个Panel用于承载浏览器窗口
        /// </summary>
        protected void InitializeUI()
        {
            this.Dock = DockStyle.Fill;

            // 创建Panel承载浏览器窗口
            _browserPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White
            };
            this.Controls.Add(_browserPanel);

            // 监听大小变化，调整嵌入窗口的大小
            _browserPanel.Resize += (s, e) =>
            {
                if (_browserProxy != null && _browserProxy.WindowHandle != IntPtr.Zero)
                {
                    MoveWindow(_browserProxy.WindowHandle, 0, 0, _browserPanel.Width, _browserPanel.Height, true);
                }
            };
        }

        /// <summary>
        /// 设置监控配置并初始化浏览器
        /// </summary>
        public async Task SetConfigAsync(MonitorConfig config)
        {
            _config = config;
            if (_config != null && !string.IsNullOrEmpty(_config.Url))
            {
                LogMessage($"✅ 配置已设置: {_config.Name}");
                await InitializeBrowserAsync();
            }
        }

        /// <summary>
        /// 初始化浏览器 - 使用BrowserWindowProxy创建独立窗口并嵌入
        /// </summary>
        protected async Task InitializeBrowserAsync()
        {
            if (_isInitialized)
            {
                LogMessage("⚠️ 浏览器已初始化");
                return;
            }

            if (_config == null || string.IsNullOrEmpty(_config.Url))
            {
                LogMessage("❌ 配置未设置或URL为空");
                return;
            }

            if (_browserPanel == null)
            {
                LogMessage("❌ 浏览器承载面板未初始化");
                return;
            }

            try
            {
                LogMessage($"🚀 正在初始化独立浏览器窗口: {_config.Url}");
                
                // 创建BrowserWindowProxy
                _browserProxy?.Dispose();
                _browserProxy = new BrowserWindowProxy();
                _browserProxy.OnLog += (s, msg) => LogMessage($"[浏览器] {msg}");

                // 初始化浏览器窗口（在独立STA线程中）
                await _browserProxy.InitializeAsync($"{_config.Name} - 浏览器", _config.Url);

                // 等待窗口句柄可用
                await WaitForWindowHandle();

                if (_browserProxy.WindowHandle != IntPtr.Zero)
                {
                    // 将浏览器窗口嵌入到Panel中
                    SetParent(_browserProxy.WindowHandle, _browserPanel.Handle);
                    
                    // 调整窗口大小以填满Panel
                    MoveWindow(_browserProxy.WindowHandle, 0, 0, _browserPanel.Width, _browserPanel.Height, true);

                    _isInitialized = true;
                    LogMessage("✅ 浏览器窗口已嵌入到页面");

                    // 如果配置了自动登录，则执行登录
                    if (_config.AutoLogin && !string.IsNullOrEmpty(_config.Username))
                    {
                        LogMessage("🔐 执行自动登录...");
                        await Task.Delay(2000); // 等待页面加载
                        await ExecuteLoginAsync(_config.Username, _config.Password);
                    }
                }
                else
                {
                    LogMessage("❌ 无法获取浏览器窗口句柄");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 浏览器初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 等待窗口句柄可用
        /// </summary>
        private async Task WaitForWindowHandle()
        {
            int retryCount = 0;
            while (_browserProxy != null && _browserProxy.WindowHandle == IntPtr.Zero && retryCount < 50)
            {
                await Task.Delay(100);
                retryCount++;
            }
        }

        /// <summary>
        /// 执行登录
        /// </summary>
        protected async Task ExecuteLoginAsync(string username, string password)
        {
            if (_browserProxy == null || !_browserProxy.IsInitialized)
            {
                LogMessage("⚠️ 浏览器未初始化");
                return;
            }

            try
            {
                LogMessage($"🔐 执行登录: 用户名={username}");
                var result = await _browserProxy.ExecuteCommandAsync("登录", new { username, password });
                
                if (result.Success)
                {
                    LogMessage($"✅ 登录成功: {result.Data}");
                }
                else
                {
                    LogMessage($"❌ 登录失败: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 登录异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行数据采集
        /// </summary>
        protected async Task ExecuteCollectAsync()
        {
            if (_browserProxy == null || !_browserProxy.IsInitialized)
            {
                LogMessage("⚠️ 浏览器未初始化");
                return;
            }

            try
            {
                LogMessage("📥 执行数据采集...");

                if (_config == null || string.IsNullOrEmpty(_config.Script))
                {
                    LogMessage("⚠️ 未配置采集脚本");
                    return;
                }

                var result = await _browserProxy.ExecuteCommandAsync("执行脚本", _config.Script);
                
                if (result.Success)
                {
                    LogMessage($"✅ 采集完成: {result.Data}");
                    if (_config != null)
                    {
                        _config.LatestIssueData = $"{DateTime.Now:HH:mm:ss} - {result.Data}";
                    }
                }
                else
                {
                    LogMessage($"❌ 采集失败: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 采集异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取Cookie
        /// </summary>
        protected async Task<string?> GetCookieAsync()
        {
            if (_browserProxy == null || !_browserProxy.IsInitialized)
            {
                LogMessage("⚠️ 浏览器未初始化");
                return null;
            }

            try
            {
                LogMessage("🍪 获取Cookie...");
                var result = await _browserProxy.ExecuteCommandAsync("获取Cookie");
                
                if (result.Success)
                {
                    LogMessage($"✅ Cookie获取成功");
                    return result.Data?.ToString();
                }
                else
                {
                    LogMessage($"❌ Cookie获取失败: {result.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Cookie获取异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 执行监控命令（从外部配置页面调用）
        /// </summary>
        public async Task ExecuteMonitorCommand(string commandName)
        {
            if (!_isInitialized)
            {
                LogMessage("⚠️ 浏览器未初始化");
                return;
            }

            try
            {
                switch (commandName)
                {
                    case "Login":
                        if (_config != null)
                        {
                            await ExecuteLoginAsync(_config.Username, _config.Password);
                        }
                        break;
                    case "Collect":
                        await ExecuteCollectAsync();
                        break;
                    case "GetCookie":
                        var cookie = await GetCookieAsync();
                        break;
                    default:
                        LogMessage($"⚠️ 未知命令: {commandName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 命令执行异常: {commandName} - {ex.Message}");
            }
        }

        /// <summary>
        /// 记录日志到主日志系统
        /// </summary>
        protected void LogMessage(string message)
        {
            _loggingService.Info(_config?.Name ?? MonitorName, message);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _monitoringCts?.Cancel();
                _monitoringCts?.Dispose();
                _browserProxy?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
