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
        private TaskCompletionSource<bool>? _webViewInitTcs; // 🔥 WebView2 初始化完成信号

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
            
            // 🔥 先初始化 WebView2（异步，但会创建 _webView 对象）
            InitializeWebView();
            
            // 🔥 注册默认函数（使用动态 WebView 引用，确保关联始终有效）
            RegisterDefaultFunctions();
            
            // 最后初始化控件（会绑定所有注册的函数到引擎）
            InitializeControls();
            
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
        /// 执行脚本（同步方法，在 UI 线程执行）
        /// </summary>
        public object ExecuteScript(string script)
        {
            if (_scriptEditor == null)
            {
                throw new InvalidOperationException("脚本编辑器未初始化");
            }

            // 🔥 检查 WebView2 初始化状态
            if (_webViewInitTcs != null && !_webViewInitTcs.Task.IsCompleted)
            {
                LogMessage("⏳ WebView2 正在初始化，请稍候...");
                MessageBox.Show("WebView2 正在初始化中，请稍后再试", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return "WebView2 初始化中";
            }
            
            if (_webViewInitTcs != null && _webViewInitTcs.Task.IsFaulted)
            {
                var error = $"WebView2 初始化失败: {_webViewInitTcs.Task.Exception?.GetBaseException().Message}";
                LogMessage($"❌ {error}");
                MessageBox.Show(error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return error;
            }

            try
            {
                LogMessage("▶️ 开始执行脚本...");
                
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

        #region 脚本管理辅助方法

        /// <summary>
        /// 在Tab中打开脚本
        /// </summary>
        private void OpenScriptInTab(TabControl tabControl, ScriptInfo script)
        {
            // 检查是否已经打开
            foreach (TabPage tab in tabControl.TabPages)
            {
                if (tab.Tag is ScriptInfo existingScript && existingScript.Id == script.Id)
                {
                    tabControl.SelectedTab = tab;
                    LogMessage($"📄 切换到脚本: {script.DisplayName}");
                    return;
                }
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

            _functionRegistry.BindToEngine(editor.ScriptEngine);
            
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
        private void OnBrowseScriptDirectory(TextBox txtPath, ListBox listBox)
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
                LoadScriptsFromDirectory(dialog.SelectedPath, listBox);
                LogMessage($"📂 脚本目录已切换: {dialog.SelectedPath}");
            }
        }

        /// <summary>
        /// 从目录加载脚本列表
        /// </summary>
        private void LoadScriptsFromDirectory(string directory, ListBox listBox)
        {
            listBox.Items.Clear();

            if (string.IsNullOrEmpty(directory) || !System.IO.Directory.Exists(directory))
            {
                LogMessage($"❌ 脚本目录不存在: {directory}");
                return;
            }

            try
            {
                var luaFiles = System.IO.Directory.GetFiles(directory, "*.lua", System.IO.SearchOption.TopDirectoryOnly);

                foreach (var filePath in luaFiles)
                {
                    var fileName = System.IO.Path.GetFileName(filePath);
                    var script = new ScriptInfo
                    {
                        Name = fileName,
                        DisplayName = System.IO.Path.GetFileNameWithoutExtension(fileName),
                        FilePath = filePath,
                        Content = System.IO.File.ReadAllText(filePath, Encoding.UTF8),
                        Type = InferScriptType(fileName)
                    };

                    listBox.Items.Add(script);
                }

                LogMessage($"✅ 已加载 {luaFiles.Length} 个脚本");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 加载脚本失败: {ex.Message}");
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
                scriptInfo.Content = editor.ScriptText;
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
        private void OnNewScript(ListBox listBox, string directory)
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

                    var newScript = new ScriptInfo
                    {
                        Name = dialog.ScriptName,
                        DisplayName = dialog.ScriptDisplayName,
                        FilePath = filePath,
                        Content = template,
                        Type = dialog.ScriptType
                    };

                    listBox.Items.Add(newScript);
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
        private void OnDeleteScript(ListBox listBox)
        {
            if (listBox.SelectedItem is ScriptInfo script)
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

                        listBox.Items.Remove(script);
                        LogMessage($"✅ 已删除脚本: {script.DisplayName}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除脚本失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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

            // 🎨 新的VS风格布局：左侧脚本列表(100px) + 右侧编辑区域
            var splitContainerScript = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 100, // 左侧脚本列表宽度
                FixedPanel = FixedPanel.Panel1, // 固定左侧面板宽度
                BorderStyle = BorderStyle.Fixed3D
            };
            
            // ============ 左侧：脚本文件列表 ============
            var listBoxScripts = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F),
                IntegralHeight = false
            };
            
            splitContainerScript.Panel1.Controls.Add(listBoxScripts);
            
            // ============ 右侧：编辑区域 ============
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
                ScriptText = _config.Script,
                EnableRealTimeValidation = true,
                ShowLineNumbers = true,
                EnableBreakpoints = true
            };
            
            tabPageMain.Controls.Add(_scriptEditor);
            tabControlScripts.TabPages.Add(tabPageMain);
            
            // 绑定所有注册的函数
            _functionRegistry.BindToEngine(_scriptEditor.ScriptEngine);
            
            panelEditor.Controls.Add(tabControlScripts);
            panelEditor.Controls.Add(panelPath);
            panelEditor.Controls.Add(toolBarTop);
            
            splitContainerScript.Panel2.Controls.Add(panelEditor);
            
            // ============ 事件绑定 ============
            
            // 脚本列表选择事件
            listBoxScripts.SelectedIndexChanged += (s, e) =>
            {
                btnDelete.Enabled = listBoxScripts.SelectedIndex >= 0;
            };
            
            // 脚本列表双击事件（打开脚本到新Tab）
            listBoxScripts.DoubleClick += (s, e) =>
            {
                if (listBoxScripts.SelectedItem is ScriptInfo script)
                {
                    OpenScriptInTab(tabControlScripts, script);
                }
            };
            
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
            btnNew.Click += (s, e) => OnNewScript(listBoxScripts, txtScriptPath.Text);
            
            // 保存脚本
            btnSave.Click += (s, e) => OnSaveCurrentScript(tabControlScripts, btnSave);
            
            // 删除脚本
            btnDelete.Click += (s, e) => OnDeleteScript(listBoxScripts);
            
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
                editor.ScriptTextChanged += (s, e) =>
                {
                    if (!tab.Text.EndsWith(" *"))
                    {
                        tab.Text += " *"; // 标记为已修改
                    }
                    if (tabControlScripts.SelectedTab == tab)
                    {
                        btnSave.Enabled = true;
                    }
                };
            };
            
            // 为默认Tab设置事件
            setupEditorEvents(_scriptEditor, tabPageMain);
            
            // Tab切换时更新保存按钮状态
            tabControlScripts.SelectedIndexChanged += (s, e) =>
            {
                if (tabControlScripts.SelectedTab != null)
                {
                    btnSave.Enabled = tabControlScripts.SelectedTab.Text.EndsWith(" *");
                }
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
            btnBrowsePath.Click += (s, e) => OnBrowseScriptDirectory(txtScriptPath, listBoxScripts);
            
            // 刷新脚本列表
            btnRefreshPath.Click += (s, e) => LoadScriptsFromDirectory(txtScriptPath.Text, listBoxScripts);
            
            // 执行脚本
            btnExecute.Click += (s, e) =>
            {
                try
                {
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
            LoadScriptsFromDirectory(defaultScriptDir, listBoxScripts);
            
            // 🔧 如果加载到了脚本，默认打开 main.lua
            if (listBoxScripts.Items.Count > 0)
            {
                var mainScript = listBoxScripts.Items.Cast<ScriptInfo>().FirstOrDefault(s => s.Name.ToLower() == "main.lua");
                if (mainScript != null)
                {
                    listBoxScripts.SelectedItem = mainScript;
                    // 更新默认Tab的内容和Tag
                    _scriptEditor.ScriptText = mainScript.Content;
                    tabPageMain.Text = mainScript.DisplayName;
                    tabPageMain.Tag = mainScript;
                }
            }
            
            tabPageScript.Controls.Add(splitContainerScript);
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
            
            // 🔥 注册 config 对象
            UpdateLuaConfigObject();
        }

        /// <summary>
        /// 更新 Lua 中的 config 对象
        /// 🔥 配置修改后调用此方法，确保脚本中的 config 对象是最新的
        /// </summary>
        private void UpdateLuaConfigObject()
        {
            // 🔥 创建新的 config 字典
            var configObject = new Dictionary<string, object>
            {
                ["url"] = _config.Url ?? "",
                ["username"] = _config.Username ?? "",
                ["password"] = _config.Password ?? "",
                ["autoLogin"] = _config.AutoLogin,
                ["name"] = _config.Name ?? ""
            };
            
            // 🔥 重新注册（会覆盖旧的）
            _functionRegistry.RegisterObject("config", configObject);
            
            // 🔥 如果脚本引擎已初始化，立即绑定
            if (_scriptEditor?.ScriptEngine != null)
            {
                _scriptEditor.ScriptEngine.BindObject("config", configObject);
            }
            
            LogMessage($"🔄 已更新 Lua config 对象: URL={_config.Url}");
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
                
                // 🔥 更新 Lua 中的 config 对象
                UpdateLuaConfigObject();
                
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
                
                // 🔥 隐藏窗口（设置为透明 + 不显示任务栏）
                Opacity = 0;              // 完全透明
                ShowInTaskbar = false;    // 不显示在任务栏
                Hide();                   // 隐藏窗口
                
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
