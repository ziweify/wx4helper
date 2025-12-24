using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;
using zhaocaimao.UserControls;
using zhaocaimao.Models.AutoBet;
using zhaocaimao.Services.AutoBet.Browser;
using zhaocaimao.Helpers;

namespace zhaocaimao.Views.AutoBet
{
    /// <summary>
    /// 浏览器窗口 - 使用内置 WebView2 控件
    /// 设计和 BaiShengV3Plus 的浏览器界面完全一样
    /// </summary>
    public partial class BetBrowserForm : UIForm
    {
        private BetBrowserControl? _browserControl;
        private readonly int _configId;
        private readonly string _configName;
        private readonly string _platform;
        private readonly string _platformUrl;
        private readonly Action<string>? _onLog;
        
        // UI 控件
        private StatusStrip? statusStrip1;
        private ToolStripStatusLabel? lblStatus;
        private ToolStripStatusLabel? lblBalance;
        private ToolStripStatusLabel? lblOddsInfo;
        private Panel? pnlTop;
        private Button? btnTestBet;
        private Button? btnTestCookie;
        private Button? btnRefresh;
        private Button? btnNavigate;
        private TextBox? txtUrl;
        private Label? lblUrl;
        private SplitContainer? splitContainer;
        private Panel? pnlBrowser;
        private Panel? pnlLog;
        private RichTextBox? txtLog;
        private Panel? pnlLogButtons;
        private CheckBox? chkLogSocket;
        private CheckBox? chkLogBet;
        private CheckBox? chkLogHttp;
        private CheckBox? chkLogSystem;
        private Label? lblLogStatus;
        private Button? btnSaveLog;
        private Button? btnClearLog;
        
        // 日志系统
        private readonly Queue<string> _logBuffer = new Queue<string>();
        private const int MAX_LOG_LINES = 1000;
        private bool _isUserScrolling = false;
        private System.Windows.Forms.Timer? _logTimer;
        
        // 日志类型
        private enum LogType
        {
            Socket,   // Socket通信
            Bet,      // 投注相关
            Http,     // HTTP拦截
            System    // 系统消息
        }
        
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
            
            // 🔥 设置窗口标题（必须在 InitializeComponent 之后）
            this.Text = $"自动投注 - {configName}";
            
            // 🔥 设置初始URL（必须在 InitializeComponent 之后）
            if (txtUrl != null)
            {
                txtUrl.Text = platformUrl;
            }
            
            // 🔥 显式设置窗口大小调整属性（确保 UIForm 基类不会覆盖）
            EnsureResizable();
            this.MinimumSize = new Size(1000, 900); // 设置最小尺寸，确保可以调整
            
            InitializeLogSystem();
            
            // 🔥 浏览器初始化在 Load 事件中异步执行
            // 注意：不能使用 async lambda，否则设计器无法解析
            this.Load += BetBrowserForm_Load;
        }
        
        /// <summary>
        /// 窗体加载事件（异步初始化浏览器）
        /// </summary>
        private async void BetBrowserForm_Load(object? sender, EventArgs e)
        {
            // 🔥 再次确保窗口大小调整属性已设置（防止 UIForm 基类覆盖）
            EnsureResizable();
            
            await InitializeBrowserAsync();
        }
        
        /// <summary>
        /// 窗体显示后再次确保窗口可调整大小（防止 UIForm 基类在显示时覆盖）
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            // 🔥 在窗体显示后再次强制设置，确保 UIForm 基类不会覆盖
            EnsureResizable();
        }
        
        /// <summary>
        /// 确保窗口可以调整大小（在多个地方调用，防止被覆盖）
        /// </summary>
        private void EnsureResizable()
        {
            try
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.SizeGripStyle = SizeGripStyle.Show;
                this.MaximizeBox = true;
                this.MinimizeBox = true;
                this.ControlBox = true;
            }
            catch
            {
                // 忽略设置失败的情况
            }
        }
        
        /// <summary>
        /// 窗体激活时确保窗口可调整大小
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            EnsureResizable();
        }
        
        private async Task InitializeBrowserAsync()
        {
            try
            {
                OnLogMessage("🚀 正在初始化浏览器窗口...");
                
                // 创建浏览器控件
                _browserControl = new BetBrowserControl();
                _browserControl.OnLog += (msg) => OnLogMessage(msg);
                
                // 添加到浏览器面板
                if (pnlBrowser != null)
                {
                    _browserControl.Dock = DockStyle.Fill;
                    pnlBrowser.Controls.Add(_browserControl);
                }
                
                // 初始化浏览器
                await _browserControl.InitializeAsync(_configId, _configName, _platform, _platformUrl);
                
                OnLogMessage("✅ 浏览器窗口初始化成功");
                UpdateStatus("✅ 初始化成功");
            }
            catch (Exception ex)
            {
                OnLogMessage($"❌ 浏览器窗口初始化失败: {ex.Message}");
                UpdateStatus($"❌ 初始化失败: {ex.Message}");
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
        /// 更新状态栏
        /// </summary>
        private void UpdateStatus(string status)
        {
            if (lblStatus != null)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => lblStatus.Text = status));
                }
                else
                {
                    lblStatus.Text = status;
                }
            }
        }
        
        /// <summary>
        /// 更新余额显示
        /// </summary>
        public void UpdateBalance(decimal balance)
        {
            if (lblBalance != null)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => lblBalance.Text = $"余额: ¥{balance:F2}"));
                }
                else
                {
                    lblBalance.Text = $"余额: ¥{balance:F2}";
                }
            }
        }
        
        #region 日志系统
        
        /// <summary>
        /// 初始化日志系统
        /// </summary>
        private void InitializeLogSystem()
        {
            if (txtLog == null) return;
            
            // 创建日志更新定时器（每100ms批量更新一次，避免频繁UI刷新）
            _logTimer = new System.Windows.Forms.Timer();
            _logTimer.Interval = 100;  // 100ms
            _logTimer.Tick += LogTimer_Tick;
            _logTimer.Start();
            
            // 监听滚动条事件
            txtLog.VScroll += TxtLog_VScroll;
            txtLog.MouseWheel += TxtLog_MouseWheel;
        }
        
        /// <summary>
        /// 日志定时器 - 批量更新UI
        /// </summary>
        private void LogTimer_Tick(object? sender, EventArgs e)
        {
            if (txtLog == null) return;
            
            int bufferCount = 0;
            lock (_logBuffer)
            {
                bufferCount = _logBuffer.Count;
            }
            
            if (bufferCount == 0)
            {
                UpdateLogStatus();
                return;
            }
            
            // 批量处理日志
            var logs = new List<string>();
            lock (_logBuffer)
            {
                while (_logBuffer.Count > 0 && logs.Count < 50)  // 每次最多处理50条
                {
                    logs.Add(_logBuffer.Dequeue());
                }
            }
            
            if (logs.Count == 0) return;
            
            // 检查是否需要自动滚动
            bool shouldAutoScroll = !_isUserScrolling && IsScrollAtBottom();
            
            // 批量添加日志
            txtLog.SuspendLayout();
            try
            {
                foreach (var log in logs)
                {
                    txtLog.AppendText(log);
                }
                
                // 限制日志行数（保持性能）
                int lineCount = txtLog.Lines.Length;
                if (lineCount > MAX_LOG_LINES)
                {
                    // 删除前面的旧日志
                    int removeLines = lineCount - MAX_LOG_LINES;
                    int removePos = 0;
                    for (int i = 0; i < removeLines; i++)
                    {
                        removePos = txtLog.Text.IndexOf('\n', removePos) + 1;
                    }
                    txtLog.Text = txtLog.Text.Substring(removePos);
                }
                
                // 自动滚动到底部
                if (shouldAutoScroll)
                {
                    txtLog.SelectionStart = txtLog.Text.Length;
                    txtLog.ScrollToCaret();
                }
            }
            finally
            {
                txtLog.ResumeLayout();
            }
            
            // 更新日志状态
            UpdateLogStatus();
        }
        
        /// <summary>
        /// 更新日志状态显示
        /// </summary>
        private void UpdateLogStatus()
        {
            if (lblLogStatus == null || txtLog == null) return;
            
            int bufferCount = 0;
            lock (_logBuffer)
            {
                bufferCount = _logBuffer.Count;
            }
            
            int lineCount = txtLog.Lines.Length;
            string autoScrollStatus = _isUserScrolling ? "关" : "开";
            
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => 
                    lblLogStatus.Text = $"📊 日志: {lineCount}行 | 缓冲: {bufferCount} | 自动滚动: {autoScrollStatus}"));
            }
            else
            {
                lblLogStatus.Text = $"📊 日志: {lineCount}行 | 缓冲: {bufferCount} | 自动滚动: {autoScrollStatus}";
            }
        }
        
        /// <summary>
        /// 检查滚动条是否在底部
        /// </summary>
        private bool IsScrollAtBottom()
        {
            if (txtLog == null || txtLog.Lines.Length == 0) return true;
            
            // 获取可见行数
            int visibleLines = txtLog.Height / txtLog.Font.Height;
            int totalLines = txtLog.Lines.Length;
            
            // 获取第一个可见字符的行号
            int firstVisibleLine = txtLog.GetLineFromCharIndex(txtLog.GetCharIndexFromPosition(new Point(0, 0)));
            
            // 如果底部可见，则认为在底部
            return (firstVisibleLine + visibleLines >= totalLines - 2);
        }
        
        /// <summary>
        /// 滚动条滚动事件
        /// </summary>
        private void TxtLog_VScroll(object? sender, EventArgs e)
        {
            _isUserScrolling = !IsScrollAtBottom();
        }
        
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        private void TxtLog_MouseWheel(object? sender, MouseEventArgs e)
        {
            _isUserScrolling = !IsScrollAtBottom();
        }
        
        /// <summary>
        /// 写入日志（带类型过滤，自动识别日志类型）
        /// </summary>
        private void OnLogMessage(string message, LogType? explicitType = null)
        {
            // 自动识别日志类型
            LogType type = explicitType ?? DetectLogType(message);
            
            // 根据复选框状态过滤日志
            bool shouldLog = type switch
            {
                LogType.Socket => chkLogSocket?.Checked ?? true,
                LogType.Bet => chkLogBet?.Checked ?? true,
                LogType.Http => chkLogHttp?.Checked ?? false,
                LogType.System => chkLogSystem?.Checked ?? true,
                _ => true
            };
            
            if (!shouldLog) return;
            
            // 输出到状态栏（只显示简短消息）
            if (message.Length > 50)
            {
                UpdateStatus(message.Substring(0, 50) + "...");
            }
            else
            {
                UpdateStatus(message);
            }
            
            // 添加到日志缓冲区（异步处理，不阻塞）
            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            var typeIcon = type switch
            {
                LogType.Socket => "🔌",
                LogType.Bet => "🎲",
                LogType.Http => "🌐",
                LogType.System => "⚙️",
                _ => "📝"
            };
            var logLine = $"[{time}] {typeIcon} {message}\r\n";
            
            lock (_logBuffer)
            {
                _logBuffer.Enqueue(logLine);
                
                // 如果缓冲区过大，丢弃旧日志（防止内存溢出）
                while (_logBuffer.Count > MAX_LOG_LINES * 2)
                {
                    _logBuffer.Dequeue();
                }
            }
            
            // 外部日志回调
            _onLog?.Invoke(message);
            OnLog?.Invoke(message);
        }
        
        /// <summary>
        /// 自动识别日志类型
        /// </summary>
        private LogType DetectLogType(string message)
        {
            if (string.IsNullOrEmpty(message)) return LogType.System;
            
            var lowerMessage = message.ToLower();
            
            // 投注相关
            if (lowerMessage.Contains("投注") || lowerMessage.Contains("bet") || 
                lowerMessage.Contains("订单") || lowerMessage.Contains("order") ||
                lowerMessage.Contains("🎲"))
            {
                return LogType.Bet;
            }
            
            // HTTP相关
            if (lowerMessage.Contains("拦截") || lowerMessage.Contains("http") || 
                lowerMessage.Contains("post") || lowerMessage.Contains("get") ||
                lowerMessage.Contains("🌐") || lowerMessage.Contains("response"))
            {
                return LogType.Http;
            }
            
            // Socket相关
            if (lowerMessage.Contains("socket") || lowerMessage.Contains("连接") ||
                lowerMessage.Contains("🔌") || lowerMessage.Contains("命令"))
            {
                return LogType.Socket;
            }
            
            // 默认为系统日志
            return LogType.System;
        }
        
        #endregion
        
        #region UI 事件处理
        
        private void BtnNavigate_Click(object? sender, EventArgs e)
        {
            if (_browserControl?.WebView != null && !string.IsNullOrWhiteSpace(txtUrl?.Text))
            {
                try
                {
                    _browserControl.Navigate(txtUrl.Text);
                    OnLogMessage($"导航到: {txtUrl.Text}");
                }
                catch (Exception ex)
                {
                    OnLogMessage($"❌ 导航失败: {ex.Message}");
                }
            }
        }
        
        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            if (_browserControl?.WebView != null)
            {
                try
                {
                    _browserControl.Refresh();
                    OnLogMessage("刷新页面");
                }
                catch (Exception ex)
                {
                    OnLogMessage($"❌ 刷新失败: {ex.Message}");
                }
            }
        }
        
        private async void BtnTestCookie_Click(object? sender, EventArgs e)
        {
            try
            {
                OnLogMessage("🍪 【测试】开始获取Cookie...");
                
                if (_browserControl?.WebView?.CoreWebView2 == null)
                {
                    OnLogMessage("❌ WebView2未初始化");
                    return;
                }
                
                // 通过命令接口获取Cookie
                var result = await _browserControl.ExecuteCommandAsync("获取Cookie");
                
                if (result.Success && result.Data != null)
                {
                    OnLogMessage($"✅ 获取到Cookie信息");
                    
                    // 尝试从 Data 中提取 cookies 信息
                    if (result.Data is Newtonsoft.Json.Linq.JObject jobj && jobj["cookies"] is Newtonsoft.Json.Linq.JObject cookiesObj)
                    {
                        OnLogMessage($"   Cookie数量: {cookiesObj.Count}");
                        foreach (var cookie in cookiesObj)
                        {
                            var value = cookie.Value?.ToString() ?? "";
                            OnLogMessage($"   - {cookie.Key}={value.Substring(0, Math.Min(20, value.Length))}...");
                        }
                    }
                    else
                    {
                        OnLogMessage($"   数据: {result.Data}");
                    }
                }
                else
                {
                    OnLogMessage($"❌ 获取Cookie失败: {result.ErrorMessage}");
                }
                
                // 方法2：直接通过WebView2 API获取
                OnLogMessage("📋 方法2：直接通过 WebView2 API");
                var cookies = await _browserControl.WebView.CoreWebView2.CookieManager.GetCookiesAsync(_browserControl.WebView.CoreWebView2.Source);
                OnLogMessage($"   获取到{cookies.Count}个Cookie:");
                
                foreach (var cookie in cookies)
                {
                    OnLogMessage($"   - {cookie.Name}={cookie.Value.Substring(0, Math.Min(20, cookie.Value.Length))}...");
                }
                
                OnLogMessage("🍪 【测试】Cookie获取完成");
            }
            catch (Exception ex)
            {
                OnLogMessage($"❌ 获取Cookie失败:{ex.Message}");
            }
        }
        
        private async void BtnTestBet_Click(object? sender, EventArgs e)
        {
            try
            {
                OnLogMessage("🎲 【测试】开始投注测试...");
                
                if (_browserControl == null || !_browserControl.IsInitialized)
                {
                    OnLogMessage("❌ 浏览器控件未初始化");
                    return;
                }
                
                // 🔥 获取当前期号
                int currentIssueId = zhaocaimao.Helpers.BinggoHelper.GetCurrentIssueId();
                OnLogMessage($"📊 当前期号: {currentIssueId}");
                OnLogMessage($"   固定投注内容: P1大10元");
                
                // 测试投注"P1大10元"
                var testOrders = new Unit.Shared.Models.BetStandardOrderList
                {
                    new Unit.Shared.Models.BetStandardOrder(
                        currentIssueId,  // 🔥 使用当前期号
                        Unit.Shared.Models.CarNumEnum.P1, 
                        Unit.Shared.Models.BetPlayEnum.大, 
                        10)
                };
                
                OnLogMessage($"📤 直接调用投注接口...");
                var startTime = DateTime.Now;
                
                var betResult = await _browserControl.ExecuteCommandAsync("投注", testOrders);
                
                var endTime = DateTime.Now;
                var duration = (int)(endTime - startTime).TotalMilliseconds;
                
                if (betResult.Success)
                {
                    OnLogMessage($"✅ 【测试】投注成功！");
                    OnLogMessage($"   订单号:{betResult.OrderId ?? "N/A"}");
                    OnLogMessage($"   耗时:{duration}ms");
                }
                else
                {
                    OnLogMessage($"❌ 【测试】投注失败");
                    OnLogMessage($"   耗时:{duration}ms");
                    OnLogMessage($"   错误:{betResult.ErrorMessage}");
                    OnLogMessage($"💡 提示:错误\"单笔下注范围0~0\"通常表示:");
                    OnLogMessage($"   1. 当前没有开盘（未到投注时间）");
                    OnLogMessage($"   2. 这个玩法被禁用或限制");
                    OnLogMessage($"   3. 需要等待下一期开盘后再投注");
                }
                
                OnLogMessage("🎲 【测试】投注测试完成");
            }
            catch (Exception ex)
            {
                OnLogMessage($"❌ 投注测试失败:{ex.Message}");
                OnLogMessage($"   堆栈:{ex.StackTrace}");
            }
        }
        
        private void BtnClearLog_Click(object? sender, EventArgs e)
        {
            try
            {
                // 清空日志缓冲区
                lock (_logBuffer)
                {
                    _logBuffer.Clear();
                }
                
                // 清空日志文本框
                if (txtLog != null)
                {
                    txtLog.Clear();
                }
                
                // 更新状态
                UpdateLogStatus();
                
                OnLogMessage("🗑️ 日志已清空");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清空日志失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnSaveLog_Click(object? sender, EventArgs e)
        {
            try
            {
                if (txtLog == null) return;
                
                // 生成日志文件名
                var fileName = $"BrowserClient_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var saveDialog = new SaveFileDialog
                {
                    FileName = fileName,
                    Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                    Title = "保存日志"
                };
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // 保存日志
                    File.WriteAllText(saveDialog.FileName, txtLog.Text, System.Text.Encoding.UTF8);
                    
                    OnLogMessage($"💾 日志已保存: {saveDialog.FileName}");
                    MessageBox.Show($"日志已成功保存到:\n{saveDialog.FileName}", "保存成功", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                OnLogMessage($"❌ 保存日志失败: {ex.Message}");
                MessageBox.Show($"保存日志失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async void LblOddsInfo_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_browserControl == null || !_browserControl.IsInitialized)
                {
                    MessageBox.Show("浏览器未初始化", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 🔥 获取赔率列表（通过执行命令）
                var oddsResult = await _browserControl.ExecuteCommandAsync("获取赔率", null);
                
                if (!oddsResult.Success)
                {
                    MessageBox.Show($"获取赔率失败: {oddsResult.ErrorMessage ?? "未知错误"}", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 解析赔率数据
                var oddsList = oddsResult.Data as List<zhaocaimao.Services.AutoBet.Browser.Models.OddsInfo>;
                if (oddsList == null || oddsList.Count == 0)
                {
                    MessageBox.Show("赔率数据尚未加载，请先登录并等待赔率更新", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 创建并显示赔率窗口
                var oddsForm = new OddsDisplayForm();
                oddsForm.SetOddsData(oddsList);
                oddsForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                OnLogMessage($"❌ 打开赔率窗口失败: {ex.Message}");
                MessageBox.Show($"打开赔率窗口失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        #endregion
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 停止日志定时器
            if (_logTimer != null)
            {
                _logTimer.Stop();
                _logTimer.Dispose();
                _logTimer = null;
            }
            
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
