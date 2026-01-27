using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using Unit.La.Models;
using Unit.La.Scripting;
using Unit.La.Services;

namespace Unit.La.Controls
{
    /// <summary>
    /// 浏览器任务控件 - 完整的浏览器+配置+脚本+日志集成界面
    /// 可在任何项目中独立使用，类似 Chrome 开发者工具的布局
    /// </summary>
    public partial class BrowserTaskControl : Form
    {
        /// <summary>
        /// 日志类型枚举
        /// </summary>
        public enum LogType
        {
            All,      // 全部
            System,   // 系统日志（程序内部操作）
            Error,    // 错误日志
            Warning,  // 警告日志
            Script    // 脚本日志（脚本中 log() 输出的）
        }

        /// <summary>
        /// 日志条目
        /// </summary>
        private class LogEntry
        {
            public string Message { get; set; } = string.Empty;
            public LogType Type { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private ScriptTaskConfig _config;
        private WebView2? _webView;
        private BrowserConfigPanel? _configPanel;
        private RichTextBox? _logTextBox;
        private ScriptEditorControl? _scriptEditor;
        private readonly ScriptFunctionRegistry _functionRegistry = new();
        private readonly List<string> _navigationHistory = new();
        private int _historyIndex = -1;
        private Action<string>? _customLogHandler;
        private System.Windows.Forms.Timer? _thumbnailTimer; // 缩略图更新定时器
        private TaskCompletionSource<bool>? _webViewInitTcs; // 🔥 WebView2 初始化完成信号
        private CancellationTokenSource? _scriptCancellation; // 🔥 脚本取消令牌
        private Form? _scriptFloatingWindow; // 🔥 脚本浮动窗口
        private ToolStripButton? _btnToggleScriptWindow; // 🔥 切换脚本窗口按钮
        private ConfigService? _configService; // 🔥 配置服务
        
        // 🔥 日志过滤相关
        private readonly List<LogEntry> _allLogs = new(); // 存储所有日志
        private LogType _currentFilter = LogType.All; // 当前过滤类型
        private ComboBox? _logFilterComboBox; // 日志过滤下拉框

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public event EventHandler<ScriptTaskConfig>? ConfigChanged;

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
        public ScriptTaskConfig Config => _config;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="configService">配置服务（可选，如果不提供则使用默认配置服务）</param>
        public BrowserTaskControl(ScriptTaskConfig config, ConfigService? configService = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            // 🔥 初始化配置服务
            _configService = configService ?? new ConfigService();
            
            InitializeComponent();
            
            // 🔥 先初始化 WebView2（异步，但会创建 _webView 对象）
            InitializeWebView();
            
            // 🔥 注册默认函数（使用动态 WebView 引用，确保关联始终有效）
            RegisterDefaultFunctions();
            
            // 最后初始化控件（会绑定所有注册的函数到引擎）
            InitializeControls();
            
            // 🔧 修改关闭行为：关闭时隐藏而不是真正关闭
            FormClosing += BrowserTaskControl_FormClosing;
            
            // 🔧 窗口显示时，立即让脚本编辑器获得焦点，修复全局焦点问题
            Shown += BrowserTaskControl_Shown;
            
            // 🔧 初始化缩略图定时器（每2秒更新一次）
            _thumbnailTimer = new System.Windows.Forms.Timer
            {
                Interval = 2000 // 2秒
            };
            _thumbnailTimer.Tick += ThumbnailTimer_Tick;
            _thumbnailTimer.Start();
            
            // 🔥 设置自动保存机制（数据驱动：配置对象属性变更时自动保存到数据库）
            SetupAutoSave();
            
            // 🔥 配置从数据库加载（由 DataCollectionPage 在创建 BrowserTaskControl 时传入）
            // 无需从 JSON 文件加载
        }

        /// <summary>
        /// 脚本浮动窗口显示事件
        /// 🔥 当脚本窗口显示时，触发代码编辑器滚动，激活消息泵
        /// </summary>
        private void ScriptFloatingWindow_Shown(object? sender, EventArgs e)
        {
            if (_scriptEditor == null) return;
            
            // 使用 BeginInvoke 确保在窗口完全显示后再触发
            BeginInvoke(new Action(() =>
            {
                // 触发滚动操作，激活消息泵
                TriggerScintillaScroll(_scriptEditor);
                
                // 将焦点设置到编辑器
                if (_scriptEditor.CanFocus && _scriptEditor.IsHandleCreated)
                {
                    _scriptEditor.Focus();
                    Application.DoEvents();
                }
            }));
        }

        /// <summary>
        /// 窗口显示事件
        /// 🔥 使用透明度临时显示脚本窗口，触发滚动操作激活消息泵，然后隐藏
        /// </summary>
        private void BrowserTaskControl_Shown(object? sender, EventArgs e)
        {
            // 🔥 使用 BeginInvoke 确保在窗口完全显示后再触发
            BeginInvoke(new Action(() =>
            {
                // 🔥 使用透明度临时显示脚本窗口，触发滚动操作，然后隐藏
                // 因为 Windows 不允许不可见控件获得焦点，所以必须临时显示
                // 使用 Opacity = 0 可以让窗口在视觉上隐藏，但控件仍然可见
                if (_scriptFloatingWindow != null && !_scriptFloatingWindow.IsDisposed && _scriptEditor != null)
                {
                    // 保存当前窗口状态
                    var wasVisible = _scriptFloatingWindow.Visible;
                    var originalOpacity = _scriptFloatingWindow.Opacity;
                    
                    // 如果窗口隐藏，使用透明度方式临时显示
                    if (!wasVisible)
                    {
                        // 设置透明度为 0（完全透明，用户看不到）
                        _scriptFloatingWindow.Opacity = 0;
                        // 显示窗口（虽然透明，但控件可见，可以触发滚动操作）
                        _scriptFloatingWindow.Show();
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(100); // 等待窗口显示完成
                    }
                    
                    // 触发滚动操作，激活消息泵
                    TriggerScintillaScroll(_scriptEditor);
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(50); // 等待滚动操作完成
                    
                    // 如果之前是隐藏的，现在隐藏回去并恢复透明度
                    if (!wasVisible)
                    {
                        _scriptFloatingWindow.Hide();
                        _scriptFloatingWindow.Opacity = originalOpacity; // 恢复原始透明度
                        Application.DoEvents();
                    }
                }
            }));
        }

        /// <summary>
        /// 触发 ScintillaNET 控件的滚动操作，激活消息泵
        /// 🔥 关键：模拟点击函数列表的操作，让焦点真正切换到函数列表或代码编辑器
        /// </summary>
        private void TriggerScintillaScroll(ScriptEditorControl scriptEditor)
        {
            try
            {
                // 🔥 关键：先让函数列表获得焦点（模拟手动点击函数列表的操作）
                // 通过反射访问 listBoxFunctions
                var listBoxField = scriptEditor.GetType().GetField("listBoxFunctions", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var listBox = listBoxField?.GetValue(scriptEditor) as System.Windows.Forms.ListBox;
                
                if (listBox != null && listBox.Items.Count > 0)
                {
                    // 如果有函数列表项，选择第一项（模拟点击函数列表）
                    if (listBox.SelectedIndex < 0)
                    {
                        listBox.SelectedIndex = 0;
                    }
                    // 让函数列表获得焦点
                    if (listBox.CanFocus)
                    {
                        listBox.Focus();
                        Application.DoEvents();
                    }
                }
                
                // 然后触发 ScintillaNET 控件的滚动操作
                var scintillaField = scriptEditor.GetType().GetField("scintilla", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var scintilla = scintillaField?.GetValue(scriptEditor);
                
                if (scintilla != null)
                {
                    // 获取 CurrentLine 属性（当前光标所在行）
                    var currentLineProp = scintilla.GetType().GetProperty("CurrentLine");
                    var currentLine = currentLineProp?.GetValue(scintilla);
                    
                    if (currentLine != null)
                    {
                        // 调用 Goto() 和 EnsureVisible()，触发滚动
                        // 这会激活消息泵，修复焦点状态
                        var gotoMethod = currentLine.GetType().GetMethod("Goto");
                        var ensureVisibleMethod = currentLine.GetType().GetMethod("EnsureVisible");
                        
                        gotoMethod?.Invoke(currentLine, null);
                        ensureVisibleMethod?.Invoke(currentLine, null);
                        
                        // 将焦点设置到编辑器（就像函数列表点击后那样）
                        var focusMethod = scintilla.GetType().GetMethod("Focus");
                        focusMethod?.Invoke(scintilla, null);
                        
                        Application.DoEvents();
                    }
                    else
                    {
                        // 如果 CurrentLine 为空，尝试触发第一行的滚动
                        var linesProp = scintilla.GetType().GetProperty("Lines");
                        var lines = linesProp?.GetValue(scintilla);
                        if (lines != null)
                        {
                            var countProp = lines.GetType().GetProperty("Count");
                            var count = countProp?.GetValue(lines) as int? ?? 0;
                            
                            if (count > 0)
                            {
                                var getItemMethod = lines.GetType().GetMethod("get_Item", new[] { typeof(int) });
                                if (getItemMethod != null)
                                {
                                    var firstLine = getItemMethod.Invoke(lines, new object[] { 0 });
                                    if (firstLine != null)
                                    {
                                        var gotoMethod = firstLine.GetType().GetMethod("Goto");
                                        var ensureVisibleMethod = firstLine.GetType().GetMethod("EnsureVisible");
                                        
                                        gotoMethod?.Invoke(firstLine, null);
                                        ensureVisibleMethod?.Invoke(firstLine, null);
                                        
                                        // 将焦点设置到编辑器
                                        var focusMethod = scintilla.GetType().GetMethod("Focus");
                                        focusMethod?.Invoke(scintilla, null);
                                        
                                        Application.DoEvents();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // 如果反射失败，忽略（不影响正常功能）
            }
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
        /// 执行脚本（同步方法，在 UI 线程执行）
        /// </summary>
        public object ExecuteScript(string script)
        {
            if (_scriptEditor == null)
            {
                throw new InvalidOperationException("脚本编辑器未初始化");
            }

            // 🔥 如果脚本窗口隐藏，自动显示它，确保脚本编辑器正常工作
            if (_scriptFloatingWindow != null && !_scriptFloatingWindow.IsDisposed && !_scriptFloatingWindow.Visible)
            {
                _scriptFloatingWindow.Show();
                _scriptFloatingWindow.BringToFront();
                
                // 更新按钮文本
                if (_btnToggleScriptWindow != null)
                {
                    _btnToggleScriptWindow.Text = "📝 隐藏脚本";
                }
                
                // 等待窗口显示完成
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                
                // 触发滚动操作，激活消息泵
                TriggerScintillaScroll(_scriptEditor);
                Application.DoEvents();
            }

            // 🔥 检查 WebView2 初始化状态，如果未完成则等待（最多30秒）
            if (_webViewInitTcs != null && !_webViewInitTcs.Task.IsCompleted)
            {
                LogMessage("⏳ WebView2 正在初始化，等待完成...");
                
                // 🔥 使用 DoEvents 循环等待，保持 UI 响应（最多30秒）
                var startTime = DateTime.Now;
                var timeout = TimeSpan.FromSeconds(30);
                
                while (!_webViewInitTcs.Task.IsCompleted && (DateTime.Now - startTime) < timeout)
                {
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(50); // 短暂休眠，避免 CPU 100%
                }
                
                if (!_webViewInitTcs.Task.IsCompleted)
                {
                    var error = "WebView2 初始化超时（30秒），请检查网络连接或重启应用";
                    LogMessage($"❌ {error}");
                    MessageBox.Show(error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return error;
                }
                
                // 检查是否初始化失败
                if (_webViewInitTcs.Task.IsFaulted)
                {
                    var error = $"WebView2 初始化失败: {_webViewInitTcs.Task.Exception?.GetBaseException().Message}";
                    LogMessage($"❌ {error}");
                    MessageBox.Show(error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return error;
                }
                
                LogMessage("✅ WebView2 初始化完成");
            }
            
            if (_webViewInitTcs != null && _webViewInitTcs.Task.IsFaulted)
            {
                var error = $"WebView2 初始化失败: {_webViewInitTcs.Task.Exception?.GetBaseException().Message}";
                LogMessage($"❌ {error}");
                MessageBox.Show(error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return error;
            }

            // 🔥 创建新的取消令牌
            _scriptCancellation = new CancellationTokenSource();

            try
            {
                LogMessage("▶️ 开始执行脚本...");
                
                // 🔥 ConfigBridge 已实现双向绑定，会自动同步，无需手动更新
                
                // 🔥 传递取消令牌到脚本执行环境
                _functionRegistry.RegisterDefaults(LogMessage, () => _webView, _scriptCancellation.Token);
                _functionRegistry.BindToEngine(_scriptEditor.ScriptEngine);
                
                // 🔥 直接在 UI 线程执行，不使用 Task.Run
                // 避免死锁：脚本需要访问 WebView2（必须在 UI 线程）
                var result = _scriptEditor.ExecuteScript();
                
                if (result.Success)
                {
                    LogMessage($"✅ 脚本执行成功");
                    if (!string.IsNullOrEmpty(result.Output))
                    {
                        LogMessage($"📤 输出: {result.Output}");
                    }
                    ScriptExecuted?.Invoke(this, result.Data ?? "null");
                    return result.Data ?? "null";
                }
                else
                {
                    // 显示友好的错误对话框
                    Views.ErrorDialog.ShowScriptError(result.Error ?? "未知错误", result.LineNumber, result.Output ?? "");
                    
                    // 同时记录到日志
                    LogMessage($"❌ 脚本执行失败");
                    LogMessage($"   💬 错误: {result.Error}");
                    
                    if (result.LineNumber > 0)
                    {
                        LogMessage($"   📍 位置: 第 {result.LineNumber} 行");
                    }
                    
                    if (!string.IsNullOrEmpty(result.Output) && result.Output != result.Error)
                    {
                        LogMessage($"   📋 详细信息:");
                        // 将详细信息分行显示
                        var lines = result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            LogMessage($"      {line}");
                        }
                    }
                    
                    return result.Error ?? "执行失败";
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 脚本执行异常: {ex.Message}");
                Views.ErrorDialog.ShowScriptError(ex.Message, 0, ex.StackTrace ?? "");
                return ex.Message;
            }
            finally
            {
                // 🔥 清理取消令牌
                _scriptCancellation?.Dispose();
                _scriptCancellation = null;
            }
        }
        
        /// <summary>
        /// 停止脚本执行
        /// </summary>
        public void StopScript()
        {
            if (_scriptCancellation != null && !_scriptCancellation.IsCancellationRequested)
            {
                LogMessage("⏹️ 停止脚本执行...");
                _scriptCancellation.Cancel();
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
            if (tabControlConfigLog != null)
            {
                tabControlConfigLog.SelectedTab = tabPageConfig;
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
            
            // 🔥 从 ScriptDirectory 加载脚本（如果需要）
            if (_scriptEditor != null && !string.IsNullOrEmpty(_config.ScriptDirectory))
            {
                _scriptEditor.SetScriptDirectory(_config.ScriptDirectory);
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

        #region 脚本管理辅助方法

        /// <summary>
        /// 在Tab中打开脚本
        /// </summary>
        private void OpenScriptInTab(TabControl tabControl, ScriptInfo script, int? lineNumber = null)
        {
            // 🔥 检查是否已经打开（通过文件路径判断，而不是 ID）
            foreach (TabPage tab in tabControl.TabPages)
            {
                if (tab.Tag is ScriptInfo existingScript && 
                    !string.IsNullOrEmpty(existingScript.FilePath) && 
                    !string.IsNullOrEmpty(script.FilePath) &&
                    existingScript.FilePath == script.FilePath)
                        {
                            // 已打开，切换到该 Tab
                            tabControl.SelectedTab = tab;
                            
                            // 🔥 同步文件树的选择状态
                            var tabEditor = tab.Controls.OfType<ScriptEditorControl>().FirstOrDefault();
                            if (tabEditor != null && !string.IsNullOrEmpty(script.FilePath))
                            {
                                tabEditor.SelectFileInTree(script.FilePath);
                                
                                // 🔥 如果指定了行号，跳转到该行（用于 Go to Definition）
                                if (lineNumber.HasValue && lineNumber.Value > 0)
                                {
                                    var scintillaField = tabEditor.GetType().GetField("scintilla", 
                                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                    var scintilla = scintillaField?.GetValue(tabEditor) as ScintillaNET.Scintilla;
                                    if (scintilla != null && lineNumber.Value > 0 && lineNumber.Value <= scintilla.Lines.Count)
                                    {
                                        var line = scintilla.Lines[lineNumber.Value - 1];
                                        if (line != null)
                                        {
                                            line.Goto();
                                            line.EnsureVisible();
                                            scintilla.Focus();
                                        }
                                    }
                                }
                            }
                            
                            LogMessage($"📄 切换到已打开的脚本: {script.DisplayName}");
                            return;
                        }
            }

            // 🔥 初始化 SavedContent（用于比较是否修改）
            if (string.IsNullOrEmpty(script.SavedContent))
            {
                script.SavedContent = script.Content ?? string.Empty;
            }

            // 创建新Tab
            var newTab = new TabPage(script.DisplayName)
            {
                Tag = script
            };

            var editor = new ScriptEditorControl
            {
                Dock = DockStyle.Fill,
                ScriptText = script.Content,
                EnableRealTimeValidation = true,
                ShowLineNumbers = true,
                EnableBreakpoints = true
            };

            // 🔥 设置脚本目录（用于文件树）
            if (!string.IsNullOrEmpty(_config.ScriptDirectory))
            {
                editor.SetScriptDirectory(_config.ScriptDirectory);
            }

            _functionRegistry.BindToEngine(editor.ScriptEngine);
            
            // 🔥 订阅文件打开事件（每个编辑器都需要订阅，以便从文件树打开新文件）
            editor.FileOpenRequested += (sender, e) =>
            {
                try
                {
                    var filePath = e.FilePath;
                    if (!System.IO.File.Exists(filePath))
                    {
                        LogMessage($"❌ 文件不存在: {filePath}");
                        return;
                    }

                    var fileName = System.IO.Path.GetFileName(filePath);
                    
                    // 检查是否已经在 Tab 中打开
                    foreach (TabPage tab in tabControl.TabPages)
                    {
                        if (tab.Tag is ScriptInfo existingScript && 
                            !string.IsNullOrEmpty(existingScript.FilePath) &&
                            existingScript.FilePath == filePath)
                        {
                            // 已打开，切换到该 Tab
                            tabControl.SelectedTab = tab;
                            
                            // 🔥 同步文件树的选择状态
                            var tabEditor = tab.Controls.OfType<ScriptEditorControl>().FirstOrDefault();
                            if (tabEditor != null && !string.IsNullOrEmpty(existingScript.FilePath))
                            {
                                tabEditor.SelectFileInTree(existingScript.FilePath);
                            }
                            
                            LogMessage($"📄 切换到已打开的脚本: {fileName}");
                            return;
                        }
                    }

                    // 更新当前 Tab 的 ScriptInfo 内容（保持修改状态）
                    var currentEditor = GetCurrentScriptEditor(tabControl);
                    if (currentEditor != null && tabControl.SelectedTab != null)
                    {
                        var currentTab = tabControl.SelectedTab;
                        if (currentTab.Tag is ScriptInfo currentScript)
                        {
                            currentScript.Content = currentEditor.ScriptText;
                        }
                    }

                    // 创建新的 ScriptInfo 并在 Tab 中打开
                    var fileContent = System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    var scriptInfo = new ScriptInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = fileName,
                        DisplayName = fileName,
                        FilePath = filePath,
                        Content = fileContent,
                        SavedContent = fileContent, // 🔥 初始化 SavedContent
                        Type = InferScriptType(fileName)
                    };

                    OpenScriptInTab(tabControl, scriptInfo, null);
                }
                catch (Exception ex)
                {
                    LogMessage($"❌ 打开文件失败: {ex.Message}");
                }
            };
            
            // 设置编辑器事件（从TabControl.Tag获取）
            if (tabControl.Tag != null)
            {
                var tagData = tabControl.Tag;
                var setupEvents = tagData.GetType().GetProperty("SetupEvents")?.GetValue(tagData) as Action<ScriptEditorControl, TabPage>;
                setupEvents?.Invoke(editor, newTab);
            }

            newTab.Controls.Add(editor);
            tabControl.TabPages.Add(newTab);
            tabControl.SelectedTab = newTab;

            // 🔥 同步文件树的选择状态：根据打开的文件路径，更新文件树的高亮
            if (!string.IsNullOrEmpty(script.FilePath))
            {
                editor.SelectFileInTree(script.FilePath);
            }

            // 🔥 如果指定了行号，跳转到该行（用于 Go to Definition）
            if (lineNumber.HasValue && lineNumber.Value > 0)
            {
                // 延迟执行，确保编辑器已完全加载
                if (editor.IsHandleCreated)
                {
                    editor.BeginInvoke(new Action(() =>
                    {
                        var scintillaField = editor.GetType().GetField("scintilla", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var scintilla = scintillaField?.GetValue(editor) as ScintillaNET.Scintilla;
                        if (scintilla != null && lineNumber.Value > 0 && lineNumber.Value <= scintilla.Lines.Count)
                        {
                            var line = scintilla.Lines[lineNumber.Value - 1];
                            if (line != null)
                            {
                                line.Goto();
                                line.EnsureVisible();
                                scintilla.Focus();
                            }
                        }
                    }));
                }
            }

            LogMessage($"📄 打开脚本: {script.DisplayName}");
        }

        /// <summary>
        /// 获取当前活动的脚本编辑器
        /// </summary>
        private ScriptEditorControl? GetCurrentScriptEditor(TabControl tabControl)
        {
            if (tabControl.SelectedTab != null && tabControl.SelectedTab.Controls.Count > 0)
            {
                return tabControl.SelectedTab.Controls[0] as ScriptEditorControl;
            }
            return _scriptEditor;
        }

        /// <summary>
        /// 浏览脚本目录
        /// </summary>
        private void OnBrowseScriptDirectory(TextBox txtPath)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择脚本目录",
                ShowNewFolderButton = true,
                SelectedPath = txtPath.Text
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = dialog.SelectedPath;
                _config.ScriptDirectory = dialog.SelectedPath;
                LogMessage($"📂 脚本目录已切换: {dialog.SelectedPath}");
            }
        }

        /// <summary>
        /// 推断脚本类型
        /// </summary>
        private ScriptType InferScriptType(string fileName)
        {
            var lowerName = fileName.ToLower();
            if (lowerName == "main.lua")
                return ScriptType.Main;
            else if (lowerName == "functions.lua" || lowerName == "lib.lua")
                return ScriptType.Functions;
            else if (lowerName.Contains("test"))
                return ScriptType.Test;
            else
                return ScriptType.Custom;
        }

        /// <summary>
        /// 保存当前脚本
        /// </summary>
        private void OnSaveCurrentScript(TabControl tabControl, ToolStripButton btnSave)
        {
            if (tabControl.SelectedTab == null)
                return;

            var currentTab = tabControl.SelectedTab;
            var scriptInfo = currentTab.Tag as ScriptInfo;

            if (scriptInfo == null)
            {
                LogMessage("❌ 无法获取脚本信息");
                return;
            }

            var editor = GetCurrentScriptEditor(tabControl);
            if (editor == null)
            {
                LogMessage("❌ 无法获取编辑器");
                return;
            }

            try
            {
                // 更新脚本内容
                scriptInfo.Content = editor.ScriptText ?? string.Empty;
                scriptInfo.ModifiedAt = DateTime.Now;

                // 保存到文件
                if (!string.IsNullOrEmpty(scriptInfo.FilePath))
                {
                    System.IO.File.WriteAllText(scriptInfo.FilePath, scriptInfo.Content, Encoding.UTF8);
                }
                else
                {
                    LogMessage("⚠️ 脚本未关联文件，仅保存到内存");
                }

                // 🔥 更新 SavedContent（用于比较是否修改）
                scriptInfo.SavedContent = scriptInfo.Content;
                scriptInfo.IsModified = false;

                // 移除修改标记
                if (currentTab.Text.EndsWith(" *"))
                {
                    currentTab.Text = currentTab.Text.Substring(0, currentTab.Text.Length - 2);
                }

                // 禁用保存按钮
                btnSave.Enabled = false;

                LogMessage($"✅ 已保存脚本: {scriptInfo.DisplayName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存脚本失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"❌ 保存脚本失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 新建脚本
        /// </summary>
        private void OnNewScript(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !System.IO.Directory.Exists(directory))
            {
                MessageBox.Show("请先选择脚本目录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new ScriptNameDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = System.IO.Path.Combine(directory, dialog.ScriptName);

                if (System.IO.File.Exists(filePath))
                {
                    MessageBox.Show("脚本文件已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var template = GetScriptTemplate(dialog.ScriptType);
                    System.IO.File.WriteAllText(filePath, template, Encoding.UTF8);
                    LogMessage($"✅ 已创建脚本: {dialog.ScriptDisplayName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"创建脚本失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 删除脚本
        /// </summary>
        private void OnDeleteScript(ScriptInfo script)
        {
            var result = MessageBox.Show(
                $"确定要删除脚本 \"{script.DisplayName}\" 吗？\n\n文件将被永久删除！",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (!string.IsNullOrEmpty(script.FilePath) && System.IO.File.Exists(script.FilePath))
                    {
                        System.IO.File.Delete(script.FilePath);
                    }

                    LogMessage($"✅ 已删除脚本: {script.DisplayName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除脚本失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 获取脚本模板
        /// </summary>
        private string GetScriptTemplate(ScriptType type)
        {
            return type switch
            {
                ScriptType.Main => @"-- ====================================
-- 主脚本 (main.lua)
-- ====================================

log('🚀 主脚本开始执行')

function main()
    -- 1. 导航到目标网站
    log('📍 步骤1: 导航到目标网站')
    web.Navigate(config.url or 'https://example.com')
    web.WaitForLoad(10000)  -- 等待页面加载完成
    
    -- 2. 登录示例
    log('🔐 步骤2: 登录')
    if web.Exists('#username') then
        web.Input('#username', config.username or 'admin')
        web.Input('#password', config.password or 'password')
        web.Click('#loginBtn')
        web.Wait(2000)
    end
    
    -- 3. 执行业务逻辑
    log('💼 步骤3: 执行业务逻辑')
    
    -- 4. 获取数据示例
    local title = web.GetTitle()
    log('📄 页面标题: ' .. title)
    
    local url = web.GetUrl()
    log('🔗 当前URL: ' .. url)
    
    log('✅ 主脚本执行完成')
    return true
end

-- 执行主逻辑
local success = main()
if success then
    log('✅ 执行成功')
else
    log('❌ 执行失败')
end
",
                ScriptType.Functions => @"-- ====================================
-- 功能库 (functions.lua)
-- ====================================

log('📚 功能库加载中...')

function login(username, password)
    log('🔐 登录: ' .. username)
    web.Navigate(config.url or 'https://example.com/login')
    web.WaitForLoad()
    web.Input('#username', username)
    web.Input('#password', password)
    web.Click('#loginBtn')
    web.Wait(2000)
    return true
end

function getData()
    log('📊 获取数据')
    if not web.WaitFor('.data-table', 5000) then
        log('⚠️ 数据表格未找到')
        return nil
    end
    local texts = web.GetAllText('.data-row .title')
    return texts
end

function queryOrder(orderId)
    log('🔍 查询订单: ' .. orderId)
    web.Input('#orderId', orderId)
    web.Click('#searchBtn')
    web.Wait(1000)
    if web.WaitFor('.order-result', 3000) then
        return web.GetElementText('.order-result')
    end
    return nil
end

function placeBet(betData)
    log('💰 投注')
    web.Input('#betAmount', tostring(betData.amount))
    web.Select('#betType', betData.type)
    web.Click('#betBtn')
    web.Wait(1000)
    return web.Exists('.bet-success')
end

log('✅ 功能库加载完成')
",
                ScriptType.Test => @"-- 测试脚本
log('🧪 测试脚本开始')

-- 测试 web 库功能
log('测试1: 导航')
web.Navigate('https://www.baidu.com')
web.WaitForLoad()

log('测试2: 获取页面信息')
local title = web.GetTitle()
log('页面标题: ' .. title)

if web.Exists('#kw') then
    log('✅ 找到搜索框')
end

log('🎉 测试完成')
",
                _ => @"-- 自定义脚本
log('脚本开始')

-- 在这里编写代码

log('脚本结束')
"
            };
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitializeControls()
        {
            // 🔥 配置和日志现在在 SplitContainer 中，脚本编辑器在浮动窗口中
            // 不再需要 TabControl 切换事件
            
            // 配置面板
            _configPanel = new BrowserConfigPanel
            {
                Dock = DockStyle.Fill,
                Config = _config
            };
            // 不再订阅 ConfigChanged 自动事件，改为在点击"保存"时手动触发
            tabPageConfig.Controls.Add(_configPanel);
            
            // 🔥 绑定 config 对象到脚本引擎（此时 _configPanel 已创建）
            BindConfigObject();

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

            // 🎨 编辑区域（ScriptEditorControl 内部已有文件树，无需额外的左侧列表）
            var panelEditor = new Panel { Dock = DockStyle.Fill };
            
            // 顶部工具栏（模式切换 + 操作 + 执行）
            var toolBarTop = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden,
                Padding = new Padding(5, 0, 0, 0)
            };
            
            // 模式切换
            var radioLocal = new ToolStripButton("● 本地")
            {
                Checked = true,
                CheckOnClick = true
            };
            var radioRemote = new ToolStripButton("○ 远程")
            {
                CheckOnClick = true
            };
            
            // 操作按钮
            var btnNew = new ToolStripButton("➕ 新建");
            var btnSave = new ToolStripButton("💾 保存") { Enabled = false };
            var btnDelete = new ToolStripButton("🗑 删除") { Enabled = false };
            
            // 执行按钮
            var btnExecute = new ToolStripButton("▶ 执行");
            var btnStop = new ToolStripButton("⏹ 停止") { Enabled = false };
            var btnDebug = new ToolStripButton("🐛 调试");
            var btnStepInto = new ToolStripButton("F7 步进") { Enabled = false };
            var btnStepOver = new ToolStripButton("F8 步过") { Enabled = false };
            var btnContinue = new ToolStripButton("F9 继续") { Enabled = false };
            var btnValidate = new ToolStripButton("✓ 验证");
            var btnHelp = new ToolStripButton("📖 帮助");
            
            toolBarTop.Items.Add(radioLocal);
            toolBarTop.Items.Add(radioRemote);
            toolBarTop.Items.Add(new ToolStripSeparator());
            toolBarTop.Items.Add(btnNew);
            toolBarTop.Items.Add(btnSave);
            toolBarTop.Items.Add(btnDelete);
            toolBarTop.Items.Add(new ToolStripSeparator());
            toolBarTop.Items.Add(btnExecute);
            toolBarTop.Items.Add(btnStop);
            toolBarTop.Items.Add(new ToolStripSeparator());
            toolBarTop.Items.Add(btnDebug);
            toolBarTop.Items.Add(btnStepInto);
            toolBarTop.Items.Add(btnStepOver);
            toolBarTop.Items.Add(btnContinue);
            toolBarTop.Items.Add(new ToolStripSeparator());
            toolBarTop.Items.Add(btnValidate);
            toolBarTop.Items.Add(btnHelp);
            
            // 路径显示栏
            var panelPath = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(5, 5, 5, 0)
            };
            
            var lblPathIcon = new Label
            {
                Text = "📂",
                AutoSize = true,
                Location = new Point(5, 6)
            };
            
            var txtScriptPath = new TextBox
            {
                Location = new Point(30, 5),
                Width = 400,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var btnBrowsePath = new Button
            {
                Text = "浏览",
                Location = new Point(440, 4),
                Width = 60,
                Height = 23
            };
            
            var btnRefreshPath = new Button
            {
                Text = "🔄 刷新",
                Location = new Point(510, 4),
                Width = 70,
                Height = 23
            };
            
            panelPath.Controls.Add(lblPathIcon);
            panelPath.Controls.Add(txtScriptPath);
            panelPath.Controls.Add(btnBrowsePath);
            panelPath.Controls.Add(btnRefreshPath);
            
            // VS风格的Tab标签页（用于切换多个打开的脚本）
            var tabControlScripts = new TabControl
            {
                Dock = DockStyle.Fill
            };
            
            // 默认添加一个标签页
            var tabPageMain = new TabPage("main.lua");
            
            _scriptEditor = new ScriptEditorControl
            {
                Dock = DockStyle.Fill,
                EnableRealTimeValidation = true,
                ShowLineNumbers = true,
                EnableBreakpoints = true
            };
            
            // 🔥 设置脚本目录（用于文件树）- 脚本内容从文件夹中加载
            if (!string.IsNullOrEmpty(_config.ScriptDirectory))
            {
                _scriptEditor.SetScriptDirectory(_config.ScriptDirectory);
            }
            
            // 🔥 为默认 Tab 创建 ScriptInfo（如果存在 main.lua 文件）
            ScriptInfo? mainScriptInfo = null;
            if (!string.IsNullOrEmpty(_config.ScriptDirectory))
            {
                var mainLuaPath = System.IO.Path.Combine(_config.ScriptDirectory, "main.lua");
                if (System.IO.File.Exists(mainLuaPath))
                {
                    try
                    {
                    var content = System.IO.File.ReadAllText(mainLuaPath, System.Text.Encoding.UTF8);
                    mainScriptInfo = new ScriptInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "main.lua",
                        DisplayName = "main.lua",
                        FilePath = mainLuaPath,
                        Content = content,
                        SavedContent = content, // 🔥 初始化 SavedContent
                        Type = ScriptType.Main
                    };
                    _scriptEditor.ScriptText = mainScriptInfo.Content;
                    tabPageMain.Tag = mainScriptInfo;
                    }
                    catch
                    {
                        // 如果读取失败，忽略
                    }
                }
            }
            
            tabPageMain.Controls.Add(_scriptEditor);
            tabControlScripts.TabPages.Add(tabPageMain);
            
            // 绑定所有注册的函数
            _functionRegistry.BindToEngine(_scriptEditor.ScriptEngine);
            
            panelEditor.Controls.Add(tabControlScripts);
            panelEditor.Controls.Add(panelPath);
            panelEditor.Controls.Add(toolBarTop);
            
            // ============ 事件绑定 ============
            
            // 注意：文件列表功能已移至 ScriptEditorControl 内部的文件树
            
            // 模式切换
            radioLocal.Click += (s, e) =>
            {
                radioLocal.Text = "● 本地";
                radioRemote.Text = "○ 远程";
                radioLocal.Checked = true;
                radioRemote.Checked = false;
                _config.ScriptSourceMode = ScriptSourceMode.Local;
                LogMessage("⚙️ 切换到本地模式");
            };
            
            radioRemote.Click += (s, e) =>
            {
                radioLocal.Text = "○ 本地";
                radioRemote.Text = "● 远程";
                radioLocal.Checked = false;
                radioRemote.Checked = true;
                _config.ScriptSourceMode = ScriptSourceMode.Remote;
                LogMessage("⚙️ 切换到远程模式");
                MessageBox.Show("远程模式功能开发中，敬请期待！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            
            // 新建脚本
            btnNew.Click += (s, e) =>
            {
                OnNewScript(txtScriptPath.Text);
                // 刷新文件树
                var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                if (currentEditor != null && !string.IsNullOrEmpty(txtScriptPath.Text))
                {
                    currentEditor.UpdateFileTree(txtScriptPath.Text);
                }
            };
            
            // 保存脚本
            btnSave.Click += (s, e) => OnSaveCurrentScript(tabControlScripts, btnSave);
            
            // 删除脚本（从当前Tab获取脚本信息）
            btnDelete.Click += (s, e) =>
            {
                var currentTab = tabControlScripts.SelectedTab;
                if (currentTab?.Tag is ScriptInfo script)
                {
                    OnDeleteScript(script);
                    // 刷新文件树
                    var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                    if (currentEditor != null && !string.IsNullOrEmpty(txtScriptPath.Text))
                    {
                        currentEditor.UpdateFileTree(txtScriptPath.Text);
                    }
                }
            };
            
            // 打开文件夹（添加到顶部工具栏）
            var btnOpenFolder = new ToolStripButton("📂 打开");
            btnOpenFolder.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtScriptPath.Text) && System.IO.Directory.Exists(txtScriptPath.Text))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = txtScriptPath.Text,
                        UseShellExecute = true
                    });
                }
            };
            
            // 将打开文件夹按钮添加到工具栏
            toolBarTop.Items.Insert(toolBarTop.Items.IndexOf(btnDelete) + 1, btnOpenFolder);
            
            // 监听脚本内容变化，启用保存按钮
            Action<ScriptEditorControl, TabPage> setupEditorEvents = (editor, tab) =>
            {
                // 🔥 订阅保存请求事件（Ctrl+S）
                editor.SaveRequested += (s, e) =>
                {
                    OnSaveCurrentScript(tabControlScripts, btnSave);
                };

                editor.ScriptTextChanged += (s, e) =>
                {
                    // 🔥 检查内容是否真的改变了（与已保存的内容比较）
                    if (tab.Tag is ScriptInfo scriptInfo)
                    {
                        var currentContent = editor.ScriptText ?? string.Empty;
                        var savedContent = scriptInfo.SavedContent ?? string.Empty;
                        
                        // 只有内容真正改变时才添加 * 标记
                        if (currentContent != savedContent)
                        {
                            if (!tab.Text.EndsWith(" *"))
                            {
                                tab.Text += " *"; // 标记为已修改
                            }
                            scriptInfo.IsModified = true;
                            if (tabControlScripts.SelectedTab == tab)
                            {
                                btnSave.Enabled = true;
                            }
                        }
                        else
                        {
                            // 内容与已保存的一致，移除 * 标记
                            if (tab.Text.EndsWith(" *"))
                            {
                                tab.Text = tab.Text.Substring(0, tab.Text.Length - 2);
                            }
                            scriptInfo.IsModified = false;
                            if (tabControlScripts.SelectedTab == tab)
                            {
                                btnSave.Enabled = false;
                            }
                        }
                    }
                };
            };
            
            // 为默认Tab设置事件
            setupEditorEvents(_scriptEditor, tabPageMain);
            
            // 🔥 订阅文件打开事件：当 ScriptEditorControl 的文件树双击时，在 Tab 中打开文件
            _scriptEditor.FileOpenRequested += (sender, e) =>
            {
                try
                {
                    var filePath = e.FilePath;
                    if (!System.IO.File.Exists(filePath))
                    {
                        LogMessage($"❌ 文件不存在: {filePath}");
                        return;
                    }

                    var fileName = System.IO.Path.GetFileName(filePath);
                    
                    // 检查是否已经在 Tab 中打开（通过文件路径）
                    foreach (TabPage tab in tabControlScripts.TabPages)
                    {
                        if (tab.Tag is ScriptInfo existingScript && 
                            !string.IsNullOrEmpty(existingScript.FilePath) &&
                            existingScript.FilePath == filePath)
                        {
                            // 已打开，切换到该 Tab
                            tabControlScripts.SelectedTab = tab;
                            
                            // 🔥 同步文件树的选择状态
                            var tabEditor = tab.Controls.OfType<ScriptEditorControl>().FirstOrDefault();
                            if (tabEditor != null && !string.IsNullOrEmpty(existingScript.FilePath))
                            {
                                tabEditor.SelectFileInTree(existingScript.FilePath);
                                
                                // 🔥 如果指定了行号，跳转到该行（用于 Go to Definition）
                                if (e.LineNumber.HasValue && e.LineNumber.Value > 0)
                                {
                                    var scintillaField = tabEditor.GetType().GetField("scintilla", 
                                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                    var scintilla = scintillaField?.GetValue(tabEditor) as ScintillaNET.Scintilla;
                                    if (scintilla != null && e.LineNumber.Value > 0 && e.LineNumber.Value <= scintilla.Lines.Count)
                                    {
                                        var line = scintilla.Lines[e.LineNumber.Value - 1];
                                        if (line != null)
                                        {
                                            line.Goto();
                                            line.EnsureVisible();
                                            scintilla.Focus();
                                        }
                                    }
                                }
                            }
                            
                            LogMessage($"📄 切换到已打开的脚本: {fileName}");
                            return;
                        }
                    }

                    // 🔥 更新当前 Tab 的 ScriptInfo 内容（但不保存到文件）
                    var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                    if (currentEditor != null && tabControlScripts.SelectedTab != null)
                    {
                        var currentTab = tabControlScripts.SelectedTab;
                        if (currentTab.Tag is ScriptInfo currentScript)
                        {
                            // 更新内存中的内容（保持修改状态）
                            currentScript.Content = currentEditor.ScriptText;
                        }
                    }

                    // 创建新的 ScriptInfo
                    var fileContent = System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    var scriptInfo = new ScriptInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = fileName,
                        DisplayName = fileName,
                        FilePath = filePath,
                        Content = fileContent,
                        SavedContent = fileContent, // 🔥 初始化 SavedContent
                        Type = InferScriptType(fileName)
                    };

                    // 在 Tab 中打开
                    OpenScriptInTab(tabControlScripts, scriptInfo, e.LineNumber); // 🔥 传递行号
                }
                catch (Exception ex)
                {
                    LogMessage($"❌ 打开文件失败: {ex.Message}");
                }
            };
            
            // Tab切换时：保存当前编辑内容，并更新保存按钮状态
            TabPage? _previousTab = null; // 记录之前的 Tab
            tabControlScripts.SelectedIndexChanged += (s, e) =>
            {
                // 🔥 保存之前 Tab 的编辑内容（如果有修改）
                if (_previousTab != null)
                {
                    var previousEditor = _previousTab.Controls.OfType<ScriptEditorControl>().FirstOrDefault();
                    if (previousEditor != null && _previousTab.Tag is ScriptInfo previousScript)
                    {
                        // 更新 ScriptInfo 的内容（保持修改状态，不保存到文件）
                        previousScript.Content = previousEditor.ScriptText;
                    }
                }

                // 🔥 切换到新 Tab 时，从 ScriptInfo 恢复内容（而不是从文件读取）
                if (tabControlScripts.SelectedTab != null)
                {
                    var currentTab = tabControlScripts.SelectedTab;
                    var currentEditor = currentTab.Controls.OfType<ScriptEditorControl>().FirstOrDefault();
                    if (currentEditor != null && currentTab.Tag is ScriptInfo currentScript)
                    {
                        // 从 ScriptInfo 恢复内容（保持编辑状态）
                        currentEditor.ScriptText = currentScript.Content;
                        
                        // 🔥 同步文件树的选择状态：根据当前 Tab 的文件路径，更新文件树的高亮
                        if (!string.IsNullOrEmpty(currentScript.FilePath))
                        {
                            currentEditor.SelectFileInTree(currentScript.FilePath);
                        }
                    }
                    
                    // 更新保存按钮状态
                    btnSave.Enabled = currentTab.Text.EndsWith(" *");
                }

                // 记录当前 Tab 为之前的 Tab
                _previousTab = tabControlScripts.SelectedTab;
            };
            
            // 保存 setupEditorEvents 和 btnSave 到字段，供后续使用
            tabControlScripts.Tag = new { SetupEvents = setupEditorEvents, SaveButton = btnSave };
            
            // 🔧 在窗体级别拦截 Ctrl+S 快捷键
            this.KeyPreview = true; // 启用按键预览
            this.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.S)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    OnSaveCurrentScript(tabControlScripts, btnSave);
                }
            };
            
            // 浏览文件夹
            btnBrowsePath.Click += (s, e) =>
            {
                OnBrowseScriptDirectory(txtScriptPath);
                // 刷新文件树
                var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                if (currentEditor != null && !string.IsNullOrEmpty(txtScriptPath.Text))
                {
                    currentEditor.SetScriptDirectory(txtScriptPath.Text);
                }
            };
            
            // 刷新脚本列表
            btnRefreshPath.Click += (s, e) =>
            {
                // 刷新文件树
                var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                if (currentEditor != null && !string.IsNullOrEmpty(txtScriptPath.Text))
                {
                    currentEditor.UpdateFileTree(txtScriptPath.Text);
                }
            };
            
            // 执行脚本
            btnExecute.Click += (s, e) =>
            {
                try
                {
                    // 禁用执行按钮，启用停止按钮
                    btnExecute.Enabled = false;
                    btnStop.Enabled = true;
                    
                    var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                    if (currentEditor != null)
                    {
                        ExecuteScript(currentEditor.ScriptText);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"❌ 执行脚本时发生错误: {ex.Message}");
                    Views.ErrorDialog.ShowScriptError(ex.Message, 0, ex.StackTrace ?? "");
                }
                finally
                {
                    // 恢复按钮状态
                    btnExecute.Enabled = true;
                    btnStop.Enabled = false;
                }
            };
            
            // 停止脚本
            btnStop.Click += (s, e) =>
            {
                StopScript();
            };
            
            // 调试按钮
            btnDebug.Click += (s, e) =>
            {
                var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                if (currentEditor != null)
                {
                    currentEditor.StartDebugging();
                    btnStepInto.Enabled = true;
                    btnStepOver.Enabled = true;
                    btnContinue.Enabled = true;
                    btnDebug.Enabled = false;
                }
            };
            
            // 步进（F7）
            btnStepInto.Click += (s, e) =>
            {
                var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                currentEditor?.StepInto();
            };
            
            // 步过（F8）
            btnStepOver.Click += (s, e) =>
            {
                var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                currentEditor?.StepOver();
            };
            
            // 继续（F9）
            btnContinue.Click += (s, e) =>
            {
                var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                currentEditor?.ContinueExecution();
            };
            
            // 验证脚本
            btnValidate.Click += (s, e) =>
            {
                var currentEditor = GetCurrentScriptEditor(tabControlScripts);
                if (currentEditor != null)
                {
                    LogMessage("🔍 开始验证脚本...");
                    var result = currentEditor.ValidateScript();
                    if (result.IsValid)
                    {
                        LogMessage("✅ 脚本验证通过 - 语法正确");
                    }
                    else
                    {
                        LogMessage($"❌ 脚本验证失败");
                        LogMessage($"   💬 错误: {result.Error}");
                        if (result.LineNumber > 0)
                        {
                            LogMessage($"   📍 位置: 第 {result.LineNumber} 行");
                            if (result.ColumnNumber > 0)
                            {
                                LogMessage($"           第 {result.ColumnNumber} 列");
                            }
                        }
                    }
                }
            };
            
            // 函数帮助
            btnHelp.Click += (s, e) =>
            {
                var helpText = _functionRegistry.GenerateHelpText();
                MessageBox.Show(helpText, "Lua 函数帮助", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            
            // 初始化脚本目录
            var defaultScriptDir = _config.ScriptDirectory ?? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", _config.Name ?? "Default");
            if (!System.IO.Directory.Exists(defaultScriptDir))
            {
                System.IO.Directory.CreateDirectory(defaultScriptDir);
            }
            
            // 🔧 检查目录是否为空，如果为空则创建默认脚本文件
            var existingFiles = System.IO.Directory.GetFiles(defaultScriptDir, "*.lua");
            if (existingFiles.Length == 0)
            {
                try
                {
                    // 创建 main.lua
                    var mainPath = System.IO.Path.Combine(defaultScriptDir, "main.lua");
                    System.IO.File.WriteAllText(mainPath, GetScriptTemplate(ScriptType.Main), Encoding.UTF8);
                    
                    // 创建 functions.lua
                    var functionsPath = System.IO.Path.Combine(defaultScriptDir, "functions.lua");
                    System.IO.File.WriteAllText(functionsPath, GetScriptTemplate(ScriptType.Functions), Encoding.UTF8);
                    
                    LogMessage($"✅ 已创建默认脚本模板: {defaultScriptDir}");
                }
                catch (Exception ex)
                {
                    LogMessage($"⚠️ 创建默认脚本失败: {ex.Message}");
                }
            }
            
            txtScriptPath.Text = defaultScriptDir;
            _config.ScriptDirectory = defaultScriptDir;
            
            // 🔧 如果存在 main.lua，默认打开它（如果之前没有加载）
            if (tabPageMain.Tag == null)
            {
                var defaultMainLuaPath = System.IO.Path.Combine(defaultScriptDir, "main.lua");
                if (System.IO.File.Exists(defaultMainLuaPath))
                {
                    try
                    {
                        var mainContent = System.IO.File.ReadAllText(defaultMainLuaPath, Encoding.UTF8);
                        var mainScript = new ScriptInfo
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "main.lua",
                            DisplayName = "main.lua",
                            FilePath = defaultMainLuaPath,
                            Content = mainContent,
                            SavedContent = mainContent, // 🔥 初始化 SavedContent
                            Type = ScriptType.Main
                        };
                        
                        _scriptEditor.ScriptText = mainScript.Content;
                        tabPageMain.Text = mainScript.DisplayName;
                        tabPageMain.Tag = mainScript;
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"⚠️ 加载 main.lua 失败: {ex.Message}");
                    }
                }
            }
            
            // 🔥 创建脚本浮动窗口
            CreateScriptFloatingWindow(panelEditor);
        }

        /// <summary>
        /// 初始化WebView2
        /// </summary>
        private async void InitializeWebView()
        {
            // 🔥 创建初始化完成信号
            _webViewInitTcs = new TaskCompletionSource<bool>();
            
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
                
                // 🔥 设置初始化完成
                _webViewInitTcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 浏览器初始化失败: {ex.Message}");
                
                // 🔥 设置初始化失败
                _webViewInitTcs.TrySetException(ex);
            }
        }

        /// <summary>
        /// 注册默认函数
        /// 🔥 使用动态 WebView 引用，确保在 WebView 重新创建时仍然有效
        /// </summary>
        private void RegisterDefaultFunctions()
        {
            // 🌐 使用动态 WebView 提供者，而不是直接传递 _webView 引用
            // 这样即使 _webView 被重新创建，web 对象仍然能获取最新的 WebView 实例
            _functionRegistry.RegisterDefaults(LogMessage, () => _webView);
            
            // 🔥 config 对象的绑定将在 InitializeControls() 中完成（此时 _configPanel 已创建）
        }

        /// <summary>
        /// 绑定 config 对象到脚本引擎
        /// 🔥 创建 ConfigBridge 实现双向绑定：Lua 可以读取和修改配置，修改后自动更新 UI
        /// 🔥 只需要创建一次，之后会自动同步
        /// </summary>
        private void BindConfigObject()
        {
            if (_configPanel == null) return;
            
            // 🔥 创建 ConfigBridge 对象（支持双向绑定）
            // 当 Lua 修改 config.username 时，会自动更新 _config 和 UI
            var configBridge = new Scripting.ConfigBridge(
                _config, 
                _configPanel, 
                LogMessage
            );
            
            // 🔥 注册 config 对象
            _functionRegistry.RegisterObject("config", configBridge);
            
            // 🔥 如果脚本引擎已初始化，立即绑定
            if (_scriptEditor?.ScriptEngine != null)
            {
                _scriptEditor.ScriptEngine.BindObject("config", configBridge);
            }
        }

        /// <summary>
        /// 输出日志（自动识别类型）
        /// </summary>
        private void LogMessage(string message)
        {
            // 🔥 自动识别日志类型
            LogType logType = LogType.System; // 默认为系统日志
            
            // 🔥 优先检查脚本日志标记（脚本中的 log() 函数会添加 [SCRIPT] 标记）
            if (message.StartsWith("[SCRIPT]"))
            {
                // 脚本日志（log() 或 log_info() 输出）
                logType = LogType.Script;
                // 移除标记，只保留消息内容
                message = message.Substring(9).TrimStart();
            }
            else if (message.StartsWith("[ERROR]"))
            {
                // 脚本错误日志（log_error() 输出）
                logType = LogType.Error;
                // 移除前缀，只保留消息内容
                message = message.Substring(7).TrimStart();
            }
            else if (message.StartsWith("[WARN]"))
            {
                // 脚本警告日志（log_warn() 输出）
                logType = LogType.Warning;
                // 移除前缀，只保留消息内容
                message = message.Substring(6).TrimStart();
            }
            // 检查系统错误日志
            else if (message.Contains("❌") || message.Contains("错误") || message.Contains("失败") || 
                     message.Contains("异常") || message.Contains("Exception") || message.Contains("Error"))
            {
                logType = LogType.Error;
            }
            // 检查系统警告日志
            else if (message.Contains("⚠️") || message.Contains("警告") || message.Contains("Warning"))
            {
                logType = LogType.Warning;
            }
            // 检查是否是脚本日志（脚本中的 log() 输出，不包含系统标记）
            else if (!message.Contains("✅") && !message.Contains("📄") && 
                     !message.Contains("⚙️") && !message.Contains("💾") && !message.Contains("🔄") &&
                     !message.Contains("▶️") && !message.Contains("⏹️") && !message.Contains("⏳") &&
                     !message.Contains("🌐") && !message.Contains("📂") && !message.Contains("🔍") &&
                     !message.Contains("初始化") && !message.Contains("保存") && !message.Contains("加载") &&
                     !message.Contains("执行") && !message.Contains("停止") && !message.Contains("切换到"))
            {
                // 可能是脚本日志
                logType = LogType.Script;
            }
            else
            {
                // 系统日志（包含系统操作标记）
                logType = LogType.System;
            }
            
            LogMessage(message, logType);
        }

        /// <summary>
        /// 输出日志（指定类型）
        /// </summary>
        private void LogMessage(string message, LogType type)
        {
            var timestamp = DateTime.Now;
            var timestampStr = timestamp.ToString("HH:mm:ss.fff");
            var logEntry = new LogEntry
            {
                Message = message,
                Type = type,
                Timestamp = timestamp
            };
            
            // 存储到日志列表
            _allLogs.Add(logEntry);
            
            // 限制日志数量（保留最近5000条）
            if (_allLogs.Count > 5000)
            {
                _allLogs.RemoveRange(0, _allLogs.Count - 5000);
            }

            // 格式化日志文本
            var logText = $"[{timestampStr}] {message}";

            // 调用自定义日志处理器
            _customLogHandler?.Invoke(logText);

            // 根据当前过滤条件决定是否显示
            if (ShouldDisplayLog(type))
            {
                AppendLogToTextBox(logText, type);
            }
        }

        /// <summary>
        /// 判断是否应该显示该日志
        /// </summary>
        private bool ShouldDisplayLog(LogType type)
        {
            if (_currentFilter == LogType.All)
                return true;
            return _currentFilter == type;
        }

        /// <summary>
        /// 添加日志到文本框（带颜色）
        /// </summary>
        private void AppendLogToTextBox(string logText, LogType type)
        {
            if (_logTextBox == null) return;

            var appendAction = new Action(() =>
            {
                // 根据类型设置颜色
                Color textColor = Color.White; // 默认白色
                switch (type)
                {
                    case LogType.Error:
                        textColor = Color.FromArgb(255, 100, 100); // 红色
                        break;
                    case LogType.Warning:
                        textColor = Color.FromArgb(255, 200, 100); // 橙色
                        break;
                    case LogType.Script:
                        textColor = Color.FromArgb(150, 200, 255); // 浅蓝色
                        break;
                    case LogType.System:
                        textColor = Color.White; // 白色
                        break;
                }

                _logTextBox.SelectionStart = _logTextBox.TextLength;
                _logTextBox.SelectionLength = 0;
                _logTextBox.SelectionColor = textColor;
                _logTextBox.AppendText(logText + Environment.NewLine);
                _logTextBox.SelectionColor = _logTextBox.ForeColor; // 恢复默认颜色
                _logTextBox.ScrollToCaret();
            });

            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(appendAction);
            }
            else
            {
                appendAction();
            }
        }

        /// <summary>
        /// 日志过滤下拉框选择改变事件
        /// </summary>
        private void LogFilterComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_logFilterComboBox == null) return;

            // 更新过滤类型
            _currentFilter = (LogType)_logFilterComboBox.SelectedIndex;

            // 重新显示日志
            RefreshLogDisplay();
        }

        /// <summary>
        /// 刷新日志显示（根据当前过滤条件）
        /// </summary>
        private void RefreshLogDisplay()
        {
            if (_logTextBox == null) return;

            var refreshAction = new Action(() =>
            {
                _logTextBox.Clear();

                foreach (var log in _allLogs)
                {
                    if (ShouldDisplayLog(log.Type))
                    {
                        var timestampStr = log.Timestamp.ToString("HH:mm:ss.fff");
                        var logText = $"[{timestampStr}] {log.Message}";
                        AppendLogToTextBox(logText, log.Type);
                    }
                }
            });

            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(refreshAction);
            }
            else
            {
                refreshAction();
            }
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
                // 🔥 使用数据绑定后，_config 对象已经自动同步了 UI 的值
                // 🔥 注意：脚本内容不保存到 JSON，只保存 ScriptDirectory 路径
                // 脚本内容存储在 ScriptDirectory 文件夹中，不需要在 JSON 中重复保存
                
                // 🔥 如果配置名称为空，使用默认名称
                if (string.IsNullOrEmpty(_config.Name))
                {
                    _config.Name = "默认配置";
                }
                
                // 🔍 添加详细日志
                LogMessage($"💾 准备保存配置:");
                LogMessage($"  - 名称: {_config.Name}");
                LogMessage($"  - URL: {_config.Url}");
                LogMessage($"  - 用户名: {_config.Username}");
                LogMessage($"  - 密码: {(_config.Password?.Length > 0 ? "***" : "空")}");
                LogMessage($"  - 自动登录: {_config.AutoLogin}");
                LogMessage($"  - 脚本目录: {_config.ScriptDirectory ?? "未设置"}");
                LogMessage($"  - 脚本模式: {_config.ScriptSourceMode}");
                
                // 🔥 触发配置变更事件（由订阅者保存到数据库）
                ConfigChanged?.Invoke(this, _config);
                LogMessage("✅ 配置已保存（ConfigChanged 事件已触发，已通知订阅者保存到数据库）");
                
                // 显示成功消息
                MessageBox.Show($"配置已保存到数据库！\n配置名称: {_config.Name}", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(error, "配置验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogMessage($"❌ 配置验证失败: {error}");
            }
        }

        private void OnClearLog(object? sender, EventArgs e)
        {
            _allLogs.Clear();
            _logTextBox?.Clear();
        }

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
        /// 窗口关闭时：隐藏而不是真正关闭，并自动保存配置
        /// </summary>
        private void BrowserTaskControl_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 🔥 自动保存配置到数据库（无论是否真正关闭）
            try
            {
                // 🔥 使用数据绑定后，_config 对象已经自动同步了 UI 的值
                // 🔥 注意：脚本内容保存在 ScriptDirectory 文件夹中，配置数据保存在数据库中
                
                // 如果配置名称为空，使用默认名称
                if (string.IsNullOrEmpty(_config.Name))
                {
                    _config.Name = "默认配置";
                }
                
                // 触发配置变更事件（由订阅者保存到数据库）
                ConfigChanged?.Invoke(this, _config);
            }
            catch (Exception ex)
            {
                // 保存失败不影响窗口关闭，只记录日志
                System.Diagnostics.Debug.WriteLine($"自动保存配置失败: {ex.Message}");
                if (IsHandleCreated)
                {
                    LogMessage($"❌ 自动保存配置失败: {ex.Message}");
                }
            }
            
            // 如果是用户点击关闭按钮（不是程序调用 Close()）
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // 取消关闭
                
                // 🔥 隐藏窗口（设置为透明 + 不显示任务栏）
                Opacity = 0;              // 完全透明
                ShowInTaskbar = false;    // 不显示在任务栏
                Hide();                   // 隐藏窗口
                
                LogMessage("ℹ️ 窗口已隐藏到后台运行（配置已自动保存）");
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
        /// 🔥 即使窗口隐藏（Visible = false），只要浏览器未释放，就继续更新缩略图
        /// 因为浏览器在后台运行，网页状态会持续更新，缩略图需要实时反映网页状态
        /// </summary>
        private async void ThumbnailTimer_Tick(object? sender, EventArgs e)
        {
            // 🔥 检查窗口是否真正关闭（IsDisposed），而不是检查 Visible
            // 因为窗口隐藏时 Visible = false，但浏览器仍在运行，应该继续更新缩略图
            if (_webView?.CoreWebView2 == null || IsDisposed || Disposing) return;

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

        #region 脚本浮动窗口

        /// <summary>
        /// 创建脚本浮动窗口
        /// </summary>
        private void CreateScriptFloatingWindow(Control scriptEditorPanel)
        {
            _scriptFloatingWindow = new Form
            {
                Text = "📝 脚本编辑器",
                Width = 900,  // 🔥 可调整：窗口初始宽度
                Height = 1200,   // 🔥 可调整：窗口初始高度
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.Sizable,
                ShowInTaskbar = false, // 不在任务栏显示
                Owner = this, // 设置为主窗口的子窗口
                Opacity = 1.0 // 默认完全不透明
            };

            // 将脚本编辑器面板添加到浮动窗口
            scriptEditorPanel.Dock = DockStyle.Fill;
            _scriptFloatingWindow.Controls.Add(scriptEditorPanel);

            // 订阅窗口关闭事件，恢复脚本编辑器到主窗口
            _scriptFloatingWindow.FormClosing += ScriptFloatingWindow_FormClosing;
            _scriptFloatingWindow.Shown += ScriptFloatingWindow_Shown;

            // 🔥 默认隐藏脚本窗口，用户点击按钮后才显示
            // 不调用 Show()，窗口保持隐藏状态
        }

        /// <summary>
        /// 脚本浮动窗口关闭事件
        /// </summary>
        private void ScriptFloatingWindow_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 取消关闭，改为隐藏
            e.Cancel = true;
            _scriptFloatingWindow?.Hide();
            
            // 更新按钮文本为"显示脚本"
            if (_btnToggleScriptWindow != null)
            {
                _btnToggleScriptWindow.Text = "📝 显示脚本";
            }
        }

        /// <summary>
        /// 切换脚本窗口显示/隐藏
        /// </summary>
        private void OnToggleScriptWindow(object? sender, EventArgs e)
        {
            if (_scriptFloatingWindow == null || _scriptFloatingWindow.IsDisposed)
            {
                // 如果窗口不存在，重新创建
                if (_scriptEditor != null)
                {
                    var parent = _scriptEditor.Parent;
                    if (parent != null)
                    {
                        var panelEditor = parent.Parent as Panel;
                        if (panelEditor != null)
                        {
                            // 从当前容器移除
                            panelEditor.Parent?.Controls.Remove(panelEditor);
                            // 重新创建浮动窗口
                            CreateScriptFloatingWindow(panelEditor);
                        }
                    }
                }
                return;
            }

            if (_scriptFloatingWindow.Visible)
            {
                // 当前是显示状态，隐藏窗口
                // 先设置透明度为 0（完全透明），然后隐藏
                _scriptFloatingWindow.Opacity = 0;
                Application.DoEvents();
                _scriptFloatingWindow.Hide();
                _scriptFloatingWindow.Opacity = 1.0; // 恢复透明度，为下次显示做准备
                if (_btnToggleScriptWindow != null)
                {
                    _btnToggleScriptWindow.Text = "📝 显示脚本";
                }
            }
            else
            {
                // 当前是隐藏状态，显示窗口
                // 恢复透明度为完全不透明
                _scriptFloatingWindow.Opacity = 1.0;
                _scriptFloatingWindow.Show();
                _scriptFloatingWindow.BringToFront();
                
                // 触发滚动操作，激活消息泵
                if (_scriptEditor != null)
                {
                    BeginInvoke(new Action(() =>
                    {
                        TriggerScintillaScroll(_scriptEditor);
                        Application.DoEvents();
                    }));
                }
                
                if (_btnToggleScriptWindow != null)
                {
                    _btnToggleScriptWindow.Text = "📝 隐藏脚本";
                }
            }
        }

        #endregion

        #region 配置保存和加载

        /// <summary>
        /// 设置自动保存机制（数据驱动）
        /// 🔥 监听配置对象属性变更，实现防抖自动保存到数据库
        /// </summary>
        private void SetupAutoSave()
        {
            // 🔥 订阅配置对象的属性变更事件
            _config.PropertyChanged += (s, e) =>
            {
                // 🔥 防抖：1秒无修改后自动保存
                // 使用 System.Threading.Timer 实现防抖
                var timer = new System.Threading.Timer(_ =>
                {
                    if (IsHandleCreated)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                // 🔥 注意：脚本内容保存在 ScriptDirectory 文件夹中，配置数据保存在数据库中
                                
                                // 如果配置名称为空，使用默认名称
                                if (string.IsNullOrEmpty(_config.Name))
                                {
                                    _config.Name = "默认配置";
                                }
                                
                                // 触发配置变更事件（由订阅者保存到数据库）
                                ConfigChanged?.Invoke(this, _config);
                                LogMessage($"💾 配置已自动保存到数据库（属性变更: {e.PropertyName}）");
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"❌ 自动保存失败: {ex.Message}");
                            }
                        }));
                    }
                }, null, 1000, Timeout.Infinite); // 1秒后执行，只执行一次
            };
        }


        // 🔥 已移除 SaveConfig() 和 LoadConfig() 方法
        // 配置数据保存在数据库中，由 DataCollectionPage 负责加载和保存
        // 通过 ConfigChanged 事件通知订阅者保存到数据库

        /// <summary>
        /// 从 HTTP 远程 URL 加载配置
        /// </summary>
        /// <param name="url">配置文件的 HTTP URL</param>
        public async Task LoadConfigFromRemoteAsync(string url)
        {
            try
            {
                if (_configService == null)
                {
                    LogMessage("❌ 配置服务未初始化，无法从远程加载");
                    return;
                }

                LogMessage($"🌐 正在从远程加载配置: {url}");
                var loadedConfig = await _configService.LoadConfigFromRemoteAsync(url);
                
                if (loadedConfig != null)
                {
                    // 合并配置
                    _config.Url = loadedConfig.Url;
                    _config.Username = loadedConfig.Username;
                    _config.Password = loadedConfig.Password;
                    _config.AutoLogin = loadedConfig.AutoLogin;
                    // 🔥 只保存脚本路径，不保存脚本内容
                    _config.ScriptDirectory = loadedConfig.ScriptDirectory;
                    _config.ScriptSourceMode = loadedConfig.ScriptSourceMode;
                    _config.CustomData = loadedConfig.CustomData;

                    // 更新 UI
                    if (_configPanel != null)
                    {
                        _configPanel.Config = _config;
                    }

                    // 🔥 从 ScriptDirectory 加载脚本（如果需要）
                    if (_scriptEditor != null && !string.IsNullOrEmpty(_config.ScriptDirectory))
                    {
                        _scriptEditor.SetScriptDirectory(_config.ScriptDirectory);
                    }

                    LogMessage($"✅ 配置已从远程加载成功");
                    
                    // 🔥 触发配置变更事件，通知订阅者保存到数据库
                    ConfigChanged?.Invoke(this, _config);
                    LogMessage($"💾 已通知订阅者保存配置到数据库");
                }
                else
                {
                    LogMessage($"❌ 从远程加载配置失败：返回 null");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 从远程加载配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存配置到 HTTP 远程 URL（未来功能：账号登录后同步到远端）
        /// 🔥 从数据库读取配置，转换为 JSON 发送到远程服务器
        /// </summary>
        /// <param name="url">目标 URL</param>
        public async Task SaveConfigToRemoteAsync(string url)
        {
            try
            {
                if (_configService == null)
                {
                    LogMessage("❌ 配置服务未初始化，无法保存到远程");
                    return;
                }

                // 确保配置是最新的
                if (_configPanel != null)
                {
                    _config = _configPanel.Config!;
                }
                // 🔥 注意：脚本内容不保存到 JSON，只保存 ScriptDirectory 路径
                // 脚本内容存储在 ScriptDirectory 文件夹中，不需要在 JSON 中重复保存

                LogMessage($"🌐 正在保存配置到远程: {url}");
                var success = await _configService.SaveConfigToRemoteAsync(_config, url);
                
                if (success)
                {
                    LogMessage($"✅ 配置已保存到远程成功");
                }
                else
                {
                    LogMessage($"❌ 保存配置到远程失败：HTTP 请求失败");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 保存配置到远程失败: {ex.Message}");
            }
        }

        #endregion
    }

}
