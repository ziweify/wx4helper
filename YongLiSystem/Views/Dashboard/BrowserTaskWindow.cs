using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using Microsoft.Web.WebView2.WinForms;
using YongLiSystem.Models.Dashboard;
using YongLiSystem.Views.Dashboard.Controls;
using Unit.La.Controls;

namespace YongLiSystem.Views.Dashboard
{
    /// <summary>
    /// 浏览器任务窗口 - 集成浏览器、配置、日志、脚本编辑
    /// 类似 Chrome 开发者工具的布局
    /// </summary>
    public partial class BrowserTaskWindow : XtraForm
    {
        private ScriptTask _task;
        private WebView2? _webView;
        private MonitorConfigControl? _configControl;
        private RichTextBox? _logTextBox;
        private ScriptEditorControl? _scriptEditor;
        private bool _isInitialized = false;
        private bool _isPanelVisible = true;
        private readonly List<string> _navigationHistory = new();
        private int _historyIndex = -1;
        private string _homeUrl = "";

        // 面板位置枚举
        public enum DockPosition
        {
            Right,   // 右侧（默认）
            Bottom,  // 底部
            Left     // 左侧
        }

        private DockPosition _currentDockPosition = DockPosition.Right;

        public event EventHandler<ScriptTask>? TaskConfigChanged;

        public ScriptTask Task
        {
            get => _task;
            set
            {
                _task = value;
                UpdateTaskInfo();
            }
        }

        public BrowserTaskWindow(ScriptTask task)
        {
            _task = task;
            InitializeComponent();
            InitializeWebView();
            InitializeToolPanel();
        }

        /// <summary>
        /// 初始化WebView2浏览器
        /// </summary>
        private async void InitializeWebView()
        {
            try
            {
                _webView = new WebView2
                {
                    Dock = DockStyle.Fill
                };

                panelBrowserContent.Controls.Add(_webView);

                // 初始化WebView2
                await _webView.EnsureCoreWebView2Async(null);

                // 设置主页
                _homeUrl = string.IsNullOrWhiteSpace(_task.Url) ? "https://www.baidu.com" : _task.Url;

                // 订阅导航事件
                _webView.NavigationStarting += (s, e) =>
                {
                    LogMessage($"🔄 导航到: {e.Uri}");
                    textBoxUrl.Text = e.Uri;
                };
                
                _webView.NavigationCompleted += (s, e) =>
                {
                    if (e.IsSuccess)
                    {
                        LogMessage($"✅ 页面加载成功");
                        var currentUrl = _webView.Source?.ToString() ?? "";
                        textBoxUrl.Text = currentUrl;
                        
                        // 添加到历史记录
                        AddToHistory(currentUrl);
                        
                        // 更新按钮状态
                        UpdateNavigationButtons();
                    }
                    else
                    {
                        LogMessage($"❌ 页面加载失败: {e.WebErrorStatus}");
                    }
                };

                // 订阅 URL 变更事件
                _webView.SourceChanged += (s, e) =>
                {
                    if (_webView.Source != null)
                    {
                        textBoxUrl.Text = _webView.Source.ToString();
                    }
                };

                _isInitialized = true;

                // 导航到URL
                if (!string.IsNullOrWhiteSpace(_task.Url))
                {
                    _webView.Source = new Uri(_task.Url);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ WebView2初始化失败: {ex.Message}");
                MessageBox.Show($"浏览器初始化失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 初始化工具面板（配置、日志、脚本）
        /// </summary>
        private void InitializeToolPanel()
        {
            try
            {
                // 配置页
                _configControl = new MonitorConfigControl
                {
                    Dock = DockStyle.Fill,
                    Url = _task.Url,
                    Username = _task.Username,
                    Password = _task.Password,
                    AutoLogin = _task.AutoLogin,
                    Script = _task.Script
                };

                // 订阅配置变更事件
                _configControl.UrlChanged += (s, e) => _task.Url = _configControl.Url;
                _configControl.UsernameChanged += (s, e) => _task.Username = _configControl.Username;
                _configControl.PasswordChanged += (s, e) => _task.Password = _configControl.Password;
                _configControl.AutoLoginChanged += (s, e) => _task.AutoLogin = _configControl.AutoLogin;
                _configControl.ScriptChanged += (s, e) => _task.Script = _configControl.Script;

                tabPageConfig.Controls.Add(_configControl);

                // 日志页
                _logTextBox = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
                    ForeColor = System.Drawing.Color.FromArgb(220, 220, 220),
                    Font = new System.Drawing.Font("Consolas", 9F)
                };
                tabPageLog.Controls.Add(_logTextBox);

                // 脚本编辑页
                LogMessage("正在初始化脚本编辑器...");
                _scriptEditor = new ScriptEditorControl
                {
                    Dock = DockStyle.Fill,
                    ScriptText = _task.Script
                };
                tabPageScript.Controls.Add(_scriptEditor);
                LogMessage("✅ 脚本编辑器初始化完成");

                // 添加脚本编辑器底部按钮面板
                var scriptButtonPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 40
                };

                var btnSaveScript = new SimpleButton
                {
                    Text = "💾 保存脚本",
                    Width = 100,
                    Height = 30,
                    Location = new System.Drawing.Point(10, 5)
                };
                btnSaveScript.Click += (s, e) =>
                {
                    _task.Script = _scriptEditor.ScriptText;
                    TaskConfigChanged?.Invoke(this, _task);
                    LogMessage("✅ 脚本已保存");
                };

                var btnExecuteScript = new SimpleButton
                {
                    Text = "▶ 执行脚本",
                    Width = 100,
                    Height = 30,
                    Location = new System.Drawing.Point(120, 5)
                };
                btnExecuteScript.Click += async (s, e) =>
                {
                    await ExecuteScriptAsync();
                };

                var btnValidateScript = new SimpleButton
                {
                    Text = "✓ 验证脚本",
                    Width = 100,
                    Height = 30,
                    Location = new System.Drawing.Point(230, 5)
                };
                btnValidateScript.Click += (s, e) =>
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

                scriptButtonPanel.Controls.Add(btnSaveScript);
                scriptButtonPanel.Controls.Add(btnExecuteScript);
                scriptButtonPanel.Controls.Add(btnValidateScript);
                tabPageScript.Controls.Add(scriptButtonPanel);
            }
            catch (Exception ex)
            {
                var errorMsg = $"工具面板初始化失败: {ex.Message}\n{ex.StackTrace}";
                MessageBox.Show(errorMsg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // 重新抛出异常，让调用者知道初始化失败
            }
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        private async System.Threading.Tasks.Task ExecuteScriptAsync()
        {
            if (!_isInitialized || _webView?.CoreWebView2 == null)
            {
                LogMessage("❌ 浏览器未初始化");
                return;
            }

            try
            {
                LogMessage("🔄 开始执行脚本...");
                
                var script = _scriptEditor?.ScriptText ?? _task.Script;
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                
                LogMessage($"✅ 脚本执行成功");
                LogMessage($"返回结果: {result}");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 脚本执行失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新任务信息
        /// </summary>
        private void UpdateTaskInfo()
        {
            Text = $"{_task.Name} - ID:{_task.Id}";
            
            if (_configControl != null)
            {
                _configControl.Url = _task.Url;
                _configControl.Username = _task.Username;
                _configControl.Password = _task.Password;
                _configControl.AutoLogin = _task.AutoLogin;
                _configControl.Script = _task.Script;
            }

            if (_scriptEditor != null)
            {
                _scriptEditor.ScriptText = _task.Script;
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        private void LogMessage(string message)
        {
            if (_logTextBox == null) return;

            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(() => LogMessage(message));
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _logTextBox.AppendText($"[{timestamp}] {message}\r\n");
            _logTextBox.ScrollToCaret();
        }

        /// <summary>
        /// 导航到指定URL
        /// </summary>
        public void NavigateToUrlPublic(string url)
        {
            NavigateToUrl(url);
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        private void OnSaveConfig(object? sender, EventArgs e)
        {
            if (_configControl != null)
            {
                _task.Url = _configControl.Url;
                _task.Username = _configControl.Username;
                _task.Password = _configControl.Password;
                _task.AutoLogin = _configControl.AutoLogin;
                _task.Script = _configControl.Script;

                TaskConfigChanged?.Invoke(this, _task);
                LogMessage("✅ 配置已保存");
            }
        }

        /// <summary>
        /// 刷新浏览器
        /// </summary>
        private void OnRefreshBrowser(object? sender, EventArgs e)
        {
            _webView?.Reload();
            LogMessage("🔄 刷新浏览器");
        }

        /// <summary>
        /// 浏览器刷新按钮点击
        /// </summary>
        private void OnRefreshBrowserClick(object? sender, EventArgs e)
        {
            _webView?.Reload();
            LogMessage("🔄 刷新浏览器");
        }

        /// <summary>
        /// 后退
        /// </summary>
        private void OnNavigateBack(object? sender, EventArgs e)
        {
            if (_webView?.CoreWebView2 != null && _webView.CoreWebView2.CanGoBack)
            {
                _webView.CoreWebView2.GoBack();
                LogMessage("◀ 后退");
            }
        }

        /// <summary>
        /// 前进
        /// </summary>
        private void OnNavigateForward(object? sender, EventArgs e)
        {
            if (_webView?.CoreWebView2 != null && _webView.CoreWebView2.CanGoForward)
            {
                _webView.CoreWebView2.GoForward();
                LogMessage("▶ 前进");
            }
        }

        /// <summary>
        /// 回到主页
        /// </summary>
        private void OnNavigateHome(object? sender, EventArgs e)
        {
            if (_webView != null && !string.IsNullOrWhiteSpace(_homeUrl))
            {
                try
                {
                    _webView.Source = new Uri(_homeUrl);
                    LogMessage($"🏠 回到主页: {_homeUrl}");
                }
                catch (Exception ex)
                {
                    LogMessage($"❌ 导航失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 转到指定URL
        /// </summary>
        private void OnNavigateGo(object? sender, EventArgs e)
        {
            NavigateToUrl(textBoxUrl.Text);
        }

        /// <summary>
        /// 地址栏回车键处理
        /// </summary>
        private void OnUrlKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NavigateToUrl(textBoxUrl.Text);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// 显示历史记录
        /// </summary>
        private void OnShowHistory(object? sender, EventArgs e)
        {
            if (_navigationHistory.Count == 0)
            {
                MessageBox.Show("暂无浏览历史", "历史记录", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var historyForm = new Form
            {
                Text = "浏览历史",
                Width = 600,
                Height = 400,
                StartPosition = FormStartPosition.CenterParent
            };

            var listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 9)
            };

            foreach (var url in _navigationHistory)
            {
                listBox.Items.Add(url);
            }

            listBox.DoubleClick += (s, args) =>
            {
                if (listBox.SelectedItem is string selectedUrl)
                {
                    NavigateToUrl(selectedUrl);
                    historyForm.Close();
                }
            };

            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var btnGo = new Button { Text = "转到", Width = 80, Height = 30, Location = new System.Drawing.Point(10, 5) };
            var btnClear = new Button { Text = "清空", Width = 80, Height = 30, Location = new System.Drawing.Point(100, 5) };
            var btnClose = new Button { Text = "关闭", Width = 80, Height = 30, Location = new System.Drawing.Point(190, 5) };

            btnGo.Click += (s, args) =>
            {
                if (listBox.SelectedItem is string selectedUrl)
                {
                    NavigateToUrl(selectedUrl);
                    historyForm.Close();
                }
            };

            btnClear.Click += (s, args) =>
            {
                _navigationHistory.Clear();
                _historyIndex = -1;
                listBox.Items.Clear();
                LogMessage("🗑️ 已清空浏览历史");
            };

            btnClose.Click += (s, args) => historyForm.Close();

            btnPanel.Controls.Add(btnGo);
            btnPanel.Controls.Add(btnClear);
            btnPanel.Controls.Add(btnClose);

            historyForm.Controls.Add(listBox);
            historyForm.Controls.Add(btnPanel);
            historyForm.ShowDialog();
        }

        /// <summary>
        /// 导航到指定URL
        /// </summary>
        private void NavigateToUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                // 如果不是完整的URL，自动添加 http://
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }

                if (_webView != null)
                {
                    _webView.Source = new Uri(url);
                    LogMessage($"🔗 导航到: {url}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 导航失败: {ex.Message}");
                MessageBox.Show($"无效的URL: {url}", "导航错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 添加到历史记录
        /// </summary>
        private void AddToHistory(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            // 避免重复添加相同的URL
            if (_navigationHistory.Count > 0 && _navigationHistory[_navigationHistory.Count - 1] == url)
                return;

            _navigationHistory.Add(url);
            _historyIndex = _navigationHistory.Count - 1;

            // 限制历史记录数量
            if (_navigationHistory.Count > 100)
            {
                _navigationHistory.RemoveAt(0);
                _historyIndex--;
            }
        }

        /// <summary>
        /// 更新导航按钮状态
        /// </summary>
        private void UpdateNavigationButtons()
        {
            if (_webView?.CoreWebView2 != null)
            {
                buttonBack.Enabled = _webView.CoreWebView2.CanGoBack;
                buttonForward.Enabled = _webView.CoreWebView2.CanGoForward;
            }
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        private void OnClearLog(object? sender, EventArgs e)
        {
            _logTextBox?.Clear();
        }

        /// <summary>
        /// 切换面板显示/隐藏
        /// </summary>
        private void OnTogglePanel(object? sender, EventArgs e)
        {
            _isPanelVisible = !_isPanelVisible;
            splitContainerMain.Panel2Collapsed = !_isPanelVisible;
            
            if (sender is ToolStripButton btn)
            {
                btn.Text = _isPanelVisible ? "👁️ 隐藏" : "👁️ 显示";
            }
        }

        /// <summary>
        /// 设置面板停靠位置
        /// </summary>
        private void SetDockPosition(DockPosition position)
        {
            _currentDockPosition = position;

            // 暂时移除控件
            var panel1Controls = new List<Control>();
            var panel2Controls = new List<Control>();
            
            foreach (Control ctrl in splitContainerMain.Panel1.Controls)
                panel1Controls.Add(ctrl);
            foreach (Control ctrl in splitContainerMain.Panel2.Controls)
                panel2Controls.Add(ctrl);

            splitContainerMain.Panel1.Controls.Clear();
            splitContainerMain.Panel2.Controls.Clear();

            switch (position)
            {
                case DockPosition.Right:
                    // 浏览器在左，面板在右（水平分割）
                    splitContainerMain.Orientation = Orientation.Vertical;
                    foreach (var ctrl in panel1Controls)
                        splitContainerMain.Panel1.Controls.Add(ctrl);
                    foreach (var ctrl in panel2Controls)
                        splitContainerMain.Panel2.Controls.Add(ctrl);
                    splitContainerMain.SplitterDistance = (int)(splitContainerMain.Width * 0.65);
                    break;

                case DockPosition.Bottom:
                    // 浏览器在上，面板在下（垂直分割）
                    splitContainerMain.Orientation = Orientation.Horizontal;
                    foreach (var ctrl in panel1Controls)
                        splitContainerMain.Panel1.Controls.Add(ctrl);
                    foreach (var ctrl in panel2Controls)
                        splitContainerMain.Panel2.Controls.Add(ctrl);
                    splitContainerMain.SplitterDistance = (int)(splitContainerMain.Height * 0.60);
                    break;

                case DockPosition.Left:
                    // 面板在左，浏览器在右（水平分割，交换位置）
                    splitContainerMain.Orientation = Orientation.Vertical;
                    foreach (var ctrl in panel2Controls)
                        splitContainerMain.Panel1.Controls.Add(ctrl);
                    foreach (var ctrl in panel1Controls)
                        splitContainerMain.Panel2.Controls.Add(ctrl);
                    splitContainerMain.SplitterDistance = (int)(splitContainerMain.Width * 0.35);
                    break;
            }

            LogMessage($"面板位置已切换到: {GetPositionName(position)}");
        }

        private string GetPositionName(DockPosition position)
        {
            return position switch
            {
                DockPosition.Right => "右侧",
                DockPosition.Bottom => "底部",
                DockPosition.Left => "左侧",
                _ => "未知"
            };
        }
    }
}
