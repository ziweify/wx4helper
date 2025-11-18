using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using zhaocaimao.Models.AutoBet;
using zhaocaimao.Services.AutoBet.Browser;

namespace zhaocaimao.UserControls
{
    /// <summary>
    /// 浏览器控件 - 封装 WebView2，直接嵌入到主程序
    /// 复用 BsBrowserClient 的功能，但不启动进程
    /// </summary>
    public partial class BetBrowserControl : UserControl, IDisposable
    {
        private WebView2? _webView;
        private IBetBrowserEngine? _engine;
        private int _configId;
        private string _configName = "";
        private string _platform = "";
        private string _platformUrl = "";
        private bool _disposed = false;
        
        public event Action<string>? OnLog;
        
        public BetBrowserControl()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// 初始化浏览器（复用 BsBrowserClient 的逻辑）
        /// </summary>
        public async Task InitializeAsync(int configId, string configName, string platform, string platformUrl)
        {
            try
            {
                _configId = configId;
                _configName = configName;
                _platform = platform;
                _platformUrl = platformUrl;
                
                OnLog?.Invoke("🚀 正在初始化浏览器控件...");
                
                // 创建 WebView2 控件
                _webView = new WebView2
                {
                    Dock = DockStyle.Fill
                };
                
                this.Controls.Add(_webView);
                
                // 创建浏览器引擎
                _engine = new BetBrowserEngine(_webView);
                _engine.OnLog += (msg) => OnLog?.Invoke(msg);
                
                // 初始化引擎
                await _engine.InitializeAsync(configId, configName, platform, platformUrl);
                
                OnLog?.Invoke("✅ 浏览器控件初始化成功");
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 初始化失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 执行命令（复用 BsBrowserClient 的命令接口）
        /// </summary>
        public async Task<BetResult> ExecuteCommandAsync(string command, object? data = null)
        {
            if (_engine == null || !_engine.IsInitialized)
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "浏览器未初始化"
                };
            }
            
            return await _engine.ExecuteCommandAsync(command, data);
        }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _engine?.IsInitialized ?? false;
        
        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    OnLog?.Invoke("🧹 正在清理浏览器控件资源...");
                    
                    // 清理引擎
                    if (_engine != null)
                    {
                        // 引擎没有实现 IDisposable，只需要清空引用
                        _engine = null;
                    }
                    
                    // 清理 WebView2
                    if (_webView != null)
                    {
                        _webView.Dispose();
                        _webView = null;
                    }
                    
                    OnLog?.Invoke("✅ 浏览器控件资源已清理");
                }
                catch (Exception ex)
                {
                    OnLog?.Invoke($"⚠️ 清理资源时发生异常: {ex.Message}");
                }
                finally
                {
                    _disposed = true;
                    base.Dispose(disposing);
                }
            }
        }
    }
}

