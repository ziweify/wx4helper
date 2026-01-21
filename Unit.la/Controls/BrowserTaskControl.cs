using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using Unit.La.Models;
using Unit.La.Scripting;

namespace Unit.La.Controls
{
    /// <summary>
    /// 浏览器任务控件 - 完整的浏览器+配置+脚本+日志集成界面
    /// 可在任何项目中独立使用，类似 Chrome 开发者工具的布局
    /// </summary>
    public partial class BrowserTaskControl : Form
    {
        private BrowserTaskConfig _config;
        private WebView2? _webView;
        private BrowserConfigPanel? _configPanel;
        private RichTextBox? _logTextBox;
        private ScriptEditorControl? _scriptEditor;
        private readonly ScriptFunctionRegistry _functionRegistry = new();
        private readonly List<string> _navigationHistory = new();
        private int _historyIndex = -1;
        private Action<string>? _customLogHandler;
        private System.Windows.Forms.Timer? _thumbnailTimer; // 缩略图更新定时器

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public event EventHandler<BrowserTaskConfig>? ConfigChanged;

        /// <summary>
        /// 导航完成事件
        /// </summary>
        public event EventHandler<string>? NavigationCompleted;

        /// <summary>
        /// 脚本执行完成事件
        /// </summary>
        public event EventHandler<object>? ScriptExecuted;

        /// <summary>
        /// 缩略图更新事件
        /// </summary>
        public event EventHandler<Image>? ThumbnailUpdated;

        /// <summary>
        /// 获取当前配置
        /// </summary>
        public BrowserTaskConfig Config => _config;

        public BrowserTaskControl(BrowserTaskConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            InitializeComponent();
            InitializeControls();
            
            // 注册默认函数
            RegisterDefaultFunctions();
            
            // 初始化WebView2
            InitializeWebView();
            
            // 🔧 修改关闭行为：关闭时隐藏而不是真正关闭
            FormClosing += BrowserTaskControl_FormClosing;
            
            // 🔧 初始化缩略图定时器（每2秒更新一次）
            _thumbnailTimer = new System.Windows.Forms.Timer
            {
                Interval = 2000 // 2秒
            };
            _thumbnailTimer.Tick += ThumbnailTimer_Tick;
            _thumbnailTimer.Start();
        }

        /// <summary>
        /// 注册脚本函数
        /// </summary>
        public void RegisterScriptFunction(string name, Delegate function, string description = "", string example = "", string category = "自定义")
        {
            _functionRegistry.RegisterFunction(name, function, description, example, category);
            
            // 如果脚本编辑器已初始化，立即绑定
            if (_scriptEditor?.ScriptEngine != null)
            {
                _scriptEditor.ScriptEngine.BindFunction(name, function);
            }
        }

        /// <summary>
        /// 注册脚本对象
        /// </summary>
        public void RegisterScriptObject(string name, object obj)
        {
            _functionRegistry.RegisterObject(name, obj);
            
            // 如果脚本编辑器已初始化，立即绑定
            if (_scriptEditor?.ScriptEngine != null)
            {
                _scriptEditor.ScriptEngine.BindObject(name, obj);
            }
        }

        /// <summary>
        /// 设置自定义日志处理器
        /// </summary>
        public void SetCustomLogHandler(Action<string> handler)
        {
            _customLogHandler = handler;
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        public async Task<object> ExecuteScriptAsync(string script)
        {
            if (_scriptEditor == null)
            {
                throw new InvalidOperationException("脚本编辑器未初始化");
            }

            try
            {
                var result = await Task.Run(() => _scriptEditor.ExecuteScript());
                LogMessage($"✅ 脚本执行成功");
                ScriptExecuted?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 脚本执行失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 导航到指定URL
        /// </summary>
        public void NavigateTo(string url)
        {
            if (_webView?.CoreWebView2 != null)
            {
                string fullUrl = url;
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    fullUrl = "https://" + url;
                }
                _webView.CoreWebView2.Navigate(fullUrl);
            }
        }

        /// <summary>
        /// 刷新浏览器
        /// </summary>
        public void RefreshBrowser()
        {
            _webView?.Reload();
        }

        /// <summary>
        /// 选择配置选项卡
        /// </summary>
        public void SelectConfigTab()
        {
            if (tabControlTools != null)
            {
                tabControlTools.SelectedTab = tabPageConfig;
            }
        }

        /// <summary>
        /// 选择浏览器（隐藏工具面板）
        /// </summary>
        public void SelectBrowserTab()
        {
            if (splitContainerMain.Panel2Collapsed)
            {
                splitContainerMain.Panel2Collapsed = false;
            }
        }

        /// <summary>
        /// 更新任务信息
        /// </summary>
        public void UpdateTaskInfo()
        {
            Text = $"{_config.Name} - 浏览器任务";
            
            if (_configPanel != null)
            {
                _configPanel.Config = _config;
            }
            
            if (_scriptEditor != null)
            {
                _scriptEditor.ScriptText = _config.Script;
            }

            // 如果URL变了，导航到新URL
            if (_webView?.CoreWebView2 != null && !string.IsNullOrEmpty(_config.Url))
            {
                var currentUrl = _webView.Source?.ToString() ?? "";
                if (currentUrl != _config.Url)
                {
                    NavigateTo(_config.Url);
                }
            }
        }

        #region 私有方法

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitializeControls()
        {
            // 配置面板
            _configPanel = new BrowserConfigPanel
            {
                Dock = DockStyle.Fill,
                Config = _config
            };
            // 不再订阅 ConfigChanged 自动事件，改为在点击"保存"时手动触发
            tabPageConfig.Controls.Add(_configPanel);

            // 日志面板
            _logTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9)
            };
            tabPageLog.Controls.Add(_logTextBox);

            // 脚本编辑器
            _scriptEditor = new ScriptEditorControl
            {
                Dock = DockStyle.Fill,
                ScriptText = _config.Script,
                EnableRealTimeValidation = true,
                ShowLineNumbers = true,
                EnableBreakpoints = true
            };
            
            // 绑定所有注册的函数
            _functionRegistry.BindToEngine(_scriptEditor.ScriptEngine);
            
            // 创建脚本工具栏
            var scriptToolBar = new ToolStrip { Dock = DockStyle.Top };
            var btnExecute = new ToolStripButton("▶ 执行脚本");
            btnExecute.Click += async (s, e) => await ExecuteScriptAsync(_scriptEditor.ScriptText);
            var btnValidate = new ToolStripButton("✓ 验证脚本");
            btnValidate.Click += (s, e) =>
            {
                var result = _scriptEditor.ValidateScript();
                if (result.IsValid)
                {
                    LogMessage("✅ 脚本验证通过");
                }
                else
                {
                    LogMessage($"❌ 脚本验证失败: {result.Error}");
                }
            };
            var btnHelp = new ToolStripButton("📖 函数帮助");
            btnHelp.Click += (s, e) =>
            {
                var helpText = _functionRegistry.GenerateHelpText();
                MessageBox.Show(helpText, "Lua 函数帮助", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            scriptToolBar.Items.Add(btnExecute);
            scriptToolBar.Items.Add(btnValidate);
            scriptToolBar.Items.Add(new ToolStripSeparator());
            scriptToolBar.Items.Add(btnHelp);
            
            tabPageScript.Controls.Add(_scriptEditor);
            tabPageScript.Controls.Add(scriptToolBar);
        }

        /// <summary>
        /// 初始化WebView2
        /// </summary>
        private async void InitializeWebView()
        {
            try
            {
                _webView = new WebView2 { Dock = DockStyle.Fill };
                panelBrowserContent.Controls.Add(_webView);

                await _webView.EnsureCoreWebView2Async(null);

                // 订阅导航事件
                _webView.NavigationStarting += (s, e) =>
                {
                    LogMessage($"🔄 导航到: {e.Uri}");
                    txtUrl.Text = e.Uri;
                };

                _webView.NavigationCompleted += (s, e) =>
                {
                    if (e.IsSuccess)
                    {
                        var url = _webView.Source?.ToString() ?? "";
                        LogMessage($"✅ 页面加载成功");
                        txtUrl.Text = url;
                        AddToHistory(url);
                        UpdateNavigationButtons();
                        NavigationCompleted?.Invoke(this, url);
                    }
                    else
                    {
                        LogMessage($"❌ 页面加载失败");
                    }
                };

                // 导航到初始URL
                if (!string.IsNullOrEmpty(_config.Url))
                {
                    NavigateTo(_config.Url);
                }

                LogMessage("✅ 浏览器初始化成功");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 浏览器初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 注册默认函数
        /// </summary>
        private void RegisterDefaultFunctions()
        {
            _functionRegistry.RegisterDefaults(LogMessage);
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}";

            if (_logTextBox != null)
            {
                if (_logTextBox.InvokeRequired)
                {
                    _logTextBox.Invoke(new Action(() =>
                    {
                        _logTextBox.AppendText(logEntry + Environment.NewLine);
                        _logTextBox.ScrollToCaret();
                    }));
                }
                else
                {
                    _logTextBox.AppendText(logEntry + Environment.NewLine);
                    _logTextBox.ScrollToCaret();
                }
            }

            // 调用自定义日志处理器
            _customLogHandler?.Invoke(logEntry);
        }

        /// <summary>
        /// 添加到历史记录
        /// </summary>
        private void AddToHistory(string url)
        {
            if (_historyIndex >= 0 && _historyIndex < _navigationHistory.Count &&
                _navigationHistory[_historyIndex] == url)
            {
                return; // 避免重复
            }

            // 清除前进历史
            while (_navigationHistory.Count > _historyIndex + 1)
            {
                _navigationHistory.RemoveAt(_navigationHistory.Count - 1);
            }

            _navigationHistory.Add(url);
            _historyIndex = _navigationHistory.Count - 1;

            // 限制历史记录数量
            if (_navigationHistory.Count > 100)
            {
                _navigationHistory.RemoveAt(0);
                _historyIndex--;
            }

            UpdateNavigationButtons();
            UpdateHistoryMenu();
        }

        /// <summary>
        /// 更新导航按钮状态
        /// </summary>
        private void UpdateNavigationButtons()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateNavigationButtons));
                return;
            }

            btnBack.Enabled = _historyIndex > 0;
            btnForward.Enabled = _historyIndex < _navigationHistory.Count - 1;
        }

        /// <summary>
        /// 更新历史记录菜单
        /// </summary>
        private void UpdateHistoryMenu()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateHistoryMenu));
                return;
            }

            btnHistory.DropDownItems.Clear();
            
            for (int i = _navigationHistory.Count - 1; i >= 0; i--)
            {
                var url = _navigationHistory[i];
                var item = new ToolStripMenuItem(url);
                item.Tag = url;
                item.Click += (s, e) => NavigateTo((string)((ToolStripMenuItem)s!).Tag!);
                btnHistory.DropDownItems.Add(item);
            }

            if (_navigationHistory.Count > 0)
            {
                btnHistory.DropDownItems.Add(new ToolStripSeparator());
                var clearItem = new ToolStripMenuItem("清空历史记录");
                clearItem.Click += (s, e) =>
                {
                    _navigationHistory.Clear();
                    _historyIndex = -1;
                    UpdateNavigationButtons();
                    UpdateHistoryMenu();
                    LogMessage("✅ 历史记录已清空");
                };
                btnHistory.DropDownItems.Add(clearItem);
            }
        }

        /// <summary>
        /// 设置面板停靠位置
        /// </summary>
        private void SetDockPosition(DockPosition position)
        {
            splitContainerMain.SuspendLayout();

            switch (position)
            {
                case DockPosition.Right:
                    splitContainerMain.Orientation = Orientation.Vertical;
                    splitContainerMain.SplitterDistance = Width - 480;
                    break;
                case DockPosition.Bottom:
                    splitContainerMain.Orientation = Orientation.Horizontal;
                    splitContainerMain.SplitterDistance = Height - 400;
                    break;
                case DockPosition.Left:
                    splitContainerMain.Orientation = Orientation.Vertical;
                    splitContainerMain.SplitterDistance = 480;
                    break;
            }

            splitContainerMain.ResumeLayout();
        }

        #endregion

        #region 事件处理

        private void OnGoBack(object? sender, EventArgs e)
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
                _webView?.CoreWebView2.Navigate(_navigationHistory[_historyIndex]);
                UpdateNavigationButtons();
            }
        }

        private void OnGoForward(object? sender, EventArgs e)
        {
            if (_historyIndex < _navigationHistory.Count - 1)
            {
                _historyIndex++;
                _webView?.CoreWebView2.Navigate(_navigationHistory[_historyIndex]);
                UpdateNavigationButtons();
            }
        }

        private void OnRefresh(object? sender, EventArgs e) => RefreshBrowser();

        private void OnGoHome(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_config.Url))
            {
                NavigateTo(_config.Url);
            }
        }

        private void OnNavigate(object? sender, EventArgs e) => NavigateTo(txtUrl.Text);

        private void OnUrlKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OnNavigate(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void OnSaveConfig(object? sender, EventArgs e)
        {
            string error = "";
            if (_configPanel?.ValidateConfig(out error) == true)
            {
                _config = _configPanel.Config!;
                _config.Script = _scriptEditor?.ScriptText ?? "";
                
                // 🔍 添加详细日志
                LogMessage($"💾 准备保存配置:");
                LogMessage($"  - 名称: {_config.Name}");
                LogMessage($"  - URL: {_config.Url}");
                LogMessage($"  - 用户名: {_config.Username}");
                LogMessage($"  - 自动登录: {_config.AutoLogin}");
                LogMessage($"  - 脚本长度: {_config.Script?.Length ?? 0} 字符");
                
                ConfigChanged?.Invoke(this, _config);
                LogMessage("✅ 配置已保存（ConfigChanged 事件已触发）");
            }
            else
            {
                MessageBox.Show(error, "配置验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogMessage($"❌ 配置验证失败: {error}");
            }
        }

        private void OnClearLog(object? sender, EventArgs e) => _logTextBox?.Clear();

        private void OnDockRight(object? sender, EventArgs e) => SetDockPosition(DockPosition.Right);

        private void OnDockBottom(object? sender, EventArgs e) => SetDockPosition(DockPosition.Bottom);

        private void OnDockLeft(object? sender, EventArgs e) => SetDockPosition(DockPosition.Left);

        private void OnTogglePanel(object? sender, EventArgs e)
        {
            splitContainerMain.Panel2Collapsed = !splitContainerMain.Panel2Collapsed;
            btnTogglePanel.Text = splitContainerMain.Panel2Collapsed ? "👁️ 显示" : "👁️ 隐藏";
        }

        #endregion

        #region 窗口生命周期管理

        /// <summary>
        /// 窗口关闭时：隐藏而不是真正关闭
        /// </summary>
        private void BrowserTaskControl_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 如果是用户点击关闭按钮（不是程序调用 Close()）
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // 取消关闭
                Hide(); // 隐藏窗口
                LogMessage("ℹ️ 窗口已隐藏到后台运行");
            }
            // 如果是程序调用 Close()，正常关闭
        }

        /// <summary>
        /// 真正关闭窗口并释放资源
        /// </summary>
        public void CloseAndDispose()
        {
            _thumbnailTimer?.Stop();
            _thumbnailTimer?.Dispose();
            
            // 不取消关闭事件，允许真正关闭
            FormClosing -= BrowserTaskControl_FormClosing;
            
            LogMessage("🔴 窗口正在关闭并释放资源");
            Close();
            Dispose();
        }

        #endregion

        #region 缩略图生成

        /// <summary>
        /// 定时器触发：更新缩略图
        /// </summary>
        private async void ThumbnailTimer_Tick(object? sender, EventArgs e)
        {
            if (_webView?.CoreWebView2 == null || !Visible) return;

            try
            {
                var thumbnail = await CaptureThumbnailAsync();
                if (thumbnail != null)
                {
                    ThumbnailUpdated?.Invoke(this, thumbnail);
                }
            }
            catch (Exception ex)
            {
                // 静默失败，不影响主流程
                System.Diagnostics.Debug.WriteLine($"缩略图更新失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 捕获浏览器缩略图
        /// </summary>
        public async Task<Image?> CaptureThumbnailAsync()
        {
            if (_webView?.CoreWebView2 == null) return null;

            try
            {
                // 使用 WebView2 的截图 API
                using (var stream = new System.IO.MemoryStream())
                {
                    await _webView.CoreWebView2.CapturePreviewAsync(
                        CoreWebView2CapturePreviewImageFormat.Png,
                        stream);
                    
                    stream.Position = 0;
                    var fullImage = Image.FromStream(stream);
                    
                    // 生成缩略图（280x150，与卡片大小匹配）
                    var thumbnail = new Bitmap(280, 150);
                    using (var g = Graphics.FromImage(thumbnail))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(fullImage, 0, 0, 280, 150);
                    }
                    
                    fullImage.Dispose();
                    return thumbnail;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"截图失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 手动更新缩略图（立即触发）
        /// </summary>
        public async Task RefreshThumbnailAsync()
        {
            var thumbnail = await CaptureThumbnailAsync();
            if (thumbnail != null)
            {
                ThumbnailUpdated?.Invoke(this, thumbnail);
            }
        }

        #endregion

        /// <summary>
        /// 面板停靠位置
        /// </summary>
        public enum DockPosition
        {
            Right,
            Bottom,
            Left
        }
    }
}
