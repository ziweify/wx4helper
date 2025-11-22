using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;
using zhaocaimao.UserControls;
using zhaocaimao.Models.AutoBet;
using zhaocaimao.Services.AutoBet.Browser;

namespace zhaocaimao.Views.AutoBet
{
    /// <summary>
    /// 浏览器窗口 - 使用内置 WebView2 控件
    /// 设计和 BaiShengV3Plus 的浏览器界面一样
    /// </summary>
    public partial class BetBrowserForm : UIForm
    {
        private BetBrowserControl? _browserControl;
        private readonly int _configId;
        private readonly string _configName;
        private readonly string _platform;
        private readonly string _platformUrl;
        private readonly Action<string>? _onLog;
        
        public event Action<string>? OnLog;
        
        /// <summary>
        /// 浏览器控件（供外部访问）
        /// </summary>
        public BetBrowserControl? BrowserControl => _browserControl;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _browserControl?.IsInitialized ?? false;
        
        public BetBrowserForm(int configId, string configName, string platform, string platformUrl, Action<string>? onLog = null)
        {
            _configId = configId;
            _configName = configName;
            _platform = platform;
            _platformUrl = platformUrl;
            _onLog = onLog;
            
            InitializeComponent();
            // 浏览器初始化在 Load 事件中异步执行
        }
        
        private void InitializeComponent()
        {
            // 窗口设置
            this.Text = $"自动投注 - {_configName}";
            this.Size = new System.Drawing.Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowTitle = true;
            this.ShowRadius = true;
            this.Style = UIStyle.Blue;
            this.BackColor = System.Drawing.Color.FromArgb(245, 248, 255);
            
            // 创建浏览器控件容器
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            this.Controls.Add(container);
            
            // 异步初始化浏览器（在窗口加载后）
            this.Load += async (s, e) => await InitializeBrowserAsync();
        }
        
        private async Task InitializeBrowserAsync()
        {
            try
            {
                LogMessage("🚀 正在初始化浏览器窗口...");
                
                // 创建浏览器控件
                _browserControl = new BetBrowserControl();
                _browserControl.OnLog += (msg) => LogMessage(msg);
                
                // 添加到容器
                if (this.Controls.Count > 0 && this.Controls[0] is Panel container)
                {
                    container.Controls.Add(_browserControl);
                }
                
                // 初始化浏览器
                await _browserControl.InitializeAsync(_configId, _configName, _platform, _platformUrl);
                
                LogMessage("✅ 浏览器窗口初始化成功");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 浏览器窗口初始化失败: {ex.Message}");
                MessageBox.Show($"浏览器初始化失败:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 执行命令（与 BrowserClient 接口保持一致）
        /// </summary>
        public async Task<BetResult> ExecuteCommandAsync(string command, object? data = null)
        {
            if (_browserControl == null || !_browserControl.IsInitialized)
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "浏览器未初始化"
                };
            }
            
            return await _browserControl.ExecuteCommandAsync(command, data);
        }
        
        /// <summary>
        /// 记录日志
        /// </summary>
        private void LogMessage(string message)
        {
            _onLog?.Invoke(message);
            OnLog?.Invoke(message);
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 清理浏览器控件
            if (_browserControl != null)
            {
                _browserControl.Dispose();
                _browserControl = null;
            }
            
            base.OnFormClosing(e);
        }
    }
}

