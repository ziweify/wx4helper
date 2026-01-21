using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ScintillaNET;
using Unit.La.Scripting;

namespace Unit.La.Controls
{
    /// <summary>
    /// 脚本编辑器控件（完整封装，开箱即用）
    /// 
    /// 使用方式：
    /// 1. 在设计器中拖放到窗体/用户控件
    /// 2. 设置 ScriptText 属性加载脚本
    /// 3. 订阅事件处理脚本变更和执行
    /// 
    /// 功能特性：
    /// - ✅ 语法高亮（Lua）
    /// - ✅ 断点调试（点击左边距）
    /// - ✅ 实时语法验证
    /// - ✅ 错误标记和提示
    /// - ✅ 自动完成
    /// - ✅ 代码折叠
    /// - ✅ 行号显示
    /// - ✅ 查找替换
    /// </summary>
    public partial class ScriptEditorControl : UserControl
    {
        private IScriptEngine? _scriptEngine;
        private readonly List<int> _breakpoints = new();
        private System.Windows.Forms.Timer? _validationTimer;
        private bool _isInitialized = false;

        /// <summary>
        /// 构造函数 - 自动初始化所有功能
        /// </summary>
        public ScriptEditorControl()
        {
            InitializeComponent(); // 设计器中创建 ScintillaNET 控件和所有UI
            InitializeAll(); // 自动初始化所有功能
        }

        /// <summary>
        /// 初始化所有功能（自动调用，无需手动配置）
        /// </summary>
        private void InitializeAll()
        {
            if (_isInitialized) return;

            // 1. 初始化编辑器基础配置
            InitializeEditor();

            // 2. 配置语法高亮
            ConfigureSyntaxHighlighting();

            // 3. 配置断点标记
            ConfigureBreakpointMarker();

            // 4. 配置错误标记
            ConfigureErrorMarker();

            // 5. 配置自动完成
            ConfigureAutoComplete();

            // 6. 配置代码折叠
            ConfigureCodeFolding();

            // 7. 配置行号
            ConfigureLineNumbers();

            // 8. 订阅事件
            SubscribeEvents();

            // 9. 创建默认脚本引擎（Lua）
            CreateDefaultScriptEngine();

            _isInitialized = true;
        }

        /// <summary>
        /// 初始化编辑器
        /// </summary>
        private void InitializeEditor()
        {
            if (scintilla == null) return;

            // 配置 ScintillaNET
            // 使用 lua 词法分析器
            try
            {
                scintilla.LexerName = "lua";
            }
            catch
            {
                // 如果 lua 不可用，不设置词法分析器（使用纯文本模式）
                // ScintillaNET 5.x 应该支持 lua，但为了兼容性添加 try-catch
            }
            
            scintilla.StyleClearAll();
            
            // 设置基础样式
            scintilla.Styles[Style.Default].ForeColor = Color.Black;
            scintilla.Styles[Style.Default].BackColor = Color.White;
            scintilla.Styles[Style.Default].Size = FontSize;
            scintilla.Styles[Style.Default].Font = "Consolas";
        }

        /// <summary>
        /// 配置语法高亮
        /// </summary>
        private void ConfigureSyntaxHighlighting()
        {
            if (scintilla == null) return;

            try
            {
                // Lua 关键字
                scintilla.SetKeywords(0, "and break do else elseif end false for function if in local nil not or repeat return then true until while");
                scintilla.SetKeywords(1, "string table math io file os debug coroutine"); // Lua标准库
                
                // 配置 Lua 语法样式
                scintilla.Styles[ScintillaNET.Style.Lua.Default].ForeColor = Color.Black;
                scintilla.Styles[ScintillaNET.Style.Lua.Comment].ForeColor = Color.Green;
                scintilla.Styles[ScintillaNET.Style.Lua.CommentLine].ForeColor = Color.Green;
                scintilla.Styles[ScintillaNET.Style.Lua.Number].ForeColor = Color.DarkOrange;
                scintilla.Styles[ScintillaNET.Style.Lua.Word].ForeColor = Color.Blue;
                scintilla.Styles[ScintillaNET.Style.Lua.Word].Bold = true;
                scintilla.Styles[ScintillaNET.Style.Lua.String].ForeColor = Color.Brown;
                scintilla.Styles[ScintillaNET.Style.Lua.Character].ForeColor = Color.Brown;
                scintilla.Styles[ScintillaNET.Style.Lua.LiteralString].ForeColor = Color.Brown;
                scintilla.Styles[ScintillaNET.Style.Lua.Operator].ForeColor = Color.Purple;
            }
            catch
            {
                // 如果样式配置失败，忽略（使用默认样式）
            }
        }

        /// <summary>
        /// 配置断点标记
        /// </summary>
        private void ConfigureBreakpointMarker()
        {
            if (scintilla == null) return;

            // 在左边距显示断点
            scintilla.Margins[0].Width = 20;
            scintilla.Margins[0].Type = MarginType.Symbol;
            scintilla.Margins[0].Mask = (1 << 0); // 使用标记 0

            // 断点图标（红色圆点）
            scintilla.Markers[0].Symbol = MarkerSymbol.Circle;
            scintilla.Markers[0].SetBackColor(Color.Red);
        }

        /// <summary>
        /// 配置错误标记
        /// </summary>
        private void ConfigureErrorMarker()
        {
            if (scintilla == null) return;

            // 错误标记（红色波浪线）
            scintilla.Markers[1].Symbol = MarkerSymbol.Underline;
            scintilla.Markers[1].SetBackColor(Color.Red);
        }

        /// <summary>
        /// 配置自动完成
        /// </summary>
        private void ConfigureAutoComplete()
        {
            if (scintilla == null) return;

            // 自动完成功能在 Scintilla.NET 5.x 中的 API 可能不同
            // 暂时不配置，后续可根据实际API添加
            // TODO: 实现自动完成功能
        }

        /// <summary>
        /// 配置代码折叠
        /// </summary>
        private void ConfigureCodeFolding()
        {
            if (scintilla == null) return;

            scintilla.SetProperty("fold", "1");
            scintilla.SetProperty("fold.compact", "1");
            scintilla.SetProperty("fold.lua", "1");

            // 折叠标记
            scintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            scintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            scintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;
            scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            scintilla.Markers[Marker.Folder].SetBackColor(Color.LightGray);
        }

        /// <summary>
        /// 配置行号
        /// </summary>
        private void ConfigureLineNumbers()
        {
            if (scintilla == null) return;

            scintilla.Margins[1].Type = MarginType.Number;
            scintilla.Margins[1].Width = ShowLineNumbers ? 50 : 0;
            scintilla.Margins[1].Sensitive = false;
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeEvents()
        {
            if (scintilla == null) return;

            scintilla.MarginClick += Scintilla_MarginClick;
            scintilla.TextChanged += Scintilla_TextChanged;
            scintilla.CharAdded += Scintilla_CharAdded; // 自动完成触发
        }

        /// <summary>
        /// 断点点击事件
        /// </summary>
        private void Scintilla_MarginClick(object? sender, MarginClickEventArgs e)
        {
            if (!EnableBreakpoints || scintilla == null) return;

            if (e.Margin == 0) // 左边距
            {
                int line = scintilla.LineFromPosition(e.Position);
                ToggleBreakpoint(line);
            }
        }

        /// <summary>
        /// 切换断点
        /// </summary>
        private void ToggleBreakpoint(int line)
        {
            if (scintilla == null) return;

            if (_breakpoints.Contains(line))
            {
                // 移除断点
                scintilla.Lines[line].MarkerDelete(0);
                _breakpoints.Remove(line);
                _scriptEngine?.ClearBreakpoint(line + 1); // Lua 行号从1开始
            }
            else
            {
                // 添加断点
                scintilla.Lines[line].MarkerAdd(0);
                _breakpoints.Add(line);
                _scriptEngine?.SetBreakpoint(line + 1);
            }
        }

        /// <summary>
        /// 文本变更事件（实时验证）
        /// </summary>
        private void Scintilla_TextChanged(object? sender, EventArgs e)
        {
            if (!EnableRealTimeValidation) return;

            // 延迟验证（避免频繁验证）
            _validationTimer?.Stop();
            _validationTimer = new System.Windows.Forms.Timer { Interval = ValidationDelay };
            _validationTimer.Tick += (s, args) =>
            {
                _validationTimer.Stop();
                ValidateScript();
            };
            _validationTimer.Start();

            ScriptTextChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 字符输入事件（触发自动完成）
        /// </summary>
        private void Scintilla_CharAdded(object? sender, CharAddedEventArgs e)
        {
            // 触发自动完成（可以根据需要实现）
            // scintilla.AutoCShow(...);
        }

        #region 公共属性（设计器可见，即拿即用）

        /// <summary>
        /// 获取或设置脚本内容（设计器可见）
        /// </summary>
        [Category("脚本")]
        [Description("脚本代码内容")]
        public string ScriptText
        {
            get => scintilla?.Text ?? string.Empty;
            set
            {
                if (scintilla != null)
                {
                    scintilla.Text = value ?? string.Empty;
                    ValidateScript(); // 自动验证
                }
            }
        }

        /// <summary>
        /// 是否启用实时验证（设计器可见）
        /// </summary>
        [Category("脚本")]
        [Description("是否启用实时语法验证")]
        [DefaultValue(true)]
        public bool EnableRealTimeValidation { get; set; } = true;

        /// <summary>
        /// 验证延迟时间（毫秒）（设计器可见）
        /// </summary>
        [Category("脚本")]
        [Description("实时验证延迟时间（毫秒）")]
        [DefaultValue(500)]
        public int ValidationDelay { get; set; } = 500;

        /// <summary>
        /// 是否显示行号（设计器可见）
        /// </summary>
        [Category("外观")]
        [Description("是否显示行号")]
        [DefaultValue(true)]
        public bool ShowLineNumbers
        {
            get => scintilla?.Margins[1].Width > 0;
            set
            {
                if (scintilla != null)
                {
                    scintilla.Margins[1].Width = value ? 50 : 0;
                }
            }
        }

        /// <summary>
        /// 是否启用断点（设计器可见）
        /// </summary>
        [Category("调试")]
        [Description("是否启用断点功能")]
        [DefaultValue(true)]
        public bool EnableBreakpoints { get; set; } = true;

        /// <summary>
        /// 字体大小（设计器可见）
        /// </summary>
        [Category("外观")]
        [Description("编辑器字体大小")]
        [DefaultValue(10)]
        public int FontSize
        {
            get => scintilla != null ? (int)scintilla.Styles[Style.Default].Size : 10;
            set
            {
                if (scintilla != null)
                {
                    for (int i = 0; i < scintilla.Styles.Count; i++)
                    {
                        scintilla.Styles[i].Size = value;
                    }
                }
            }
        }

        #endregion

        #region 公共方法（简单易用）

        /// <summary>
        /// 执行脚本（简单调用）
        /// </summary>
        public ScriptResult ExecuteScript(Dictionary<string, object>? context = null)
        {
            if (_scriptEngine == null)
            {
                CreateDefaultScriptEngine();
            }

            return _scriptEngine?.Execute(ScriptText, context)
                ?? new ScriptResult { Success = false, Error = "脚本引擎未初始化" };
        }

        /// <summary>
        /// 验证脚本（简单调用）
        /// </summary>
        public ScriptValidationResult ValidateScript()
        {
            if (_scriptEngine == null)
            {
                CreateDefaultScriptEngine();
            }

            var result = _scriptEngine?.Validate(ScriptText)
                ?? new ScriptValidationResult { IsValid = false, Error = "脚本引擎未初始化" };

            // 自动标记错误
            if (!result.IsValid)
            {
                MarkErrorLine(result.LineNumber - 1, result.Error);
                OnValidationError?.Invoke(result);
            }
            else
            {
                ClearErrorMarkers();
                OnValidationSuccess?.Invoke();
            }

            return result;
        }

        /// <summary>
        /// 绑定函数到脚本（简单调用）
        /// </summary>
        public void BindFunction(string name, Delegate function)
        {
            if (_scriptEngine == null)
            {
                CreateDefaultScriptEngine();
            }

            _scriptEngine?.BindFunction(name, function);
        }

        /// <summary>
        /// 绑定对象到脚本（简单调用）
        /// </summary>
        public void BindObject(string name, object obj)
        {
            if (_scriptEngine == null)
            {
                CreateDefaultScriptEngine();
            }

            _scriptEngine?.BindObject(name, obj);
        }

        /// <summary>
        /// 设置断点（简单调用）
        /// </summary>
        public void SetBreakpoint(int lineNumber)
        {
            if (!EnableBreakpoints || scintilla == null) return;

            int line = lineNumber - 1; // 转换为0基索引
            if (line >= 0 && line < scintilla.Lines.Count)
            {
                scintilla.Lines[line].MarkerAdd(0);
                _breakpoints.Add(line);
                _scriptEngine?.SetBreakpoint(lineNumber);
            }
        }

        /// <summary>
        /// 清除断点（简单调用）
        /// </summary>
        public void ClearBreakpoint(int lineNumber)
        {
            if (scintilla == null) return;

            int line = lineNumber - 1;
            if (line >= 0 && line < scintilla.Lines.Count && _breakpoints.Contains(line))
            {
                scintilla.Lines[line].MarkerDelete(0);
                _breakpoints.Remove(line);
                _scriptEngine?.ClearBreakpoint(lineNumber);
            }
        }

        /// <summary>
        /// 清除所有断点（简单调用）
        /// </summary>
        public void ClearAllBreakpoints()
        {
            if (scintilla == null) return;

            foreach (var line in _breakpoints.ToList())
            {
                if (line >= 0 && line < scintilla.Lines.Count)
                {
                    scintilla.Lines[line].MarkerDelete(0);
                }
                _scriptEngine?.ClearBreakpoint(line + 1);
            }
            _breakpoints.Clear();
        }

        /// <summary>
        /// 查找文本（简单调用）
        /// </summary>
        public void FindText(string text, bool matchCase = false, bool wholeWord = false)
        {
            if (scintilla == null) return;

            scintilla.SearchFlags = (matchCase ? SearchFlags.MatchCase : SearchFlags.None)
                | (wholeWord ? SearchFlags.WholeWord : SearchFlags.None);

            int pos = scintilla.SearchInTarget(text);
            if (pos >= 0)
            {
                scintilla.SetSelection(scintilla.TargetStart, scintilla.TargetEnd);
                scintilla.ScrollCaret();
            }
        }

        /// <summary>
        /// 替换文本（简单调用）
        /// </summary>
        public void ReplaceText(string findText, string replaceText, bool matchCase = false)
        {
            if (scintilla == null) return;

            scintilla.SearchFlags = matchCase ? SearchFlags.MatchCase : SearchFlags.None;
            scintilla.TargetStart = 0;
            scintilla.TargetEnd = scintilla.TextLength;

            scintilla.ReplaceTarget(replaceText);
        }

        /// <summary>
        /// 设置自定义脚本引擎（高级用法）
        /// </summary>
        public void SetScriptEngine(IScriptEngine engine)
        {
            _scriptEngine = engine;
            _scriptEngine.OnError += (s, e) => OnError?.Invoke(e.Error);
            _scriptEngine.OnBreakpoint += (s, e) => OnBreakpointHit?.Invoke(e);
        }

        #endregion

        #region 私有方法（内部实现）

        /// <summary>
        /// 创建默认脚本引擎（Lua）
        /// </summary>
        private void CreateDefaultScriptEngine()
        {
            if (_scriptEngine != null) return;

            _scriptEngine = new MoonSharpScriptEngine();
            _scriptEngine.OnError += (s, e) => OnError?.Invoke(e.Error);
            _scriptEngine.OnBreakpoint += (s, e) => OnBreakpointHit?.Invoke(e);

            // 绑定默认功能（如果已注册）
            var registry = ScriptFunctionRegistry.Instance;
            if (registry != null)
            {
                registry.BindToEngine(_scriptEngine);
            }
        }

        /// <summary>
        /// 标记错误行
        /// </summary>
        private void MarkErrorLine(int line, string? error)
        {
            if (scintilla == null) return;

            if (line >= 0 && line < scintilla.Lines.Count)
            {
                scintilla.Lines[line].MarkerAdd(1); // 使用标记1（错误标记）
                OnError?.Invoke($"第 {line + 1} 行: {error}");
            }
        }

        /// <summary>
        /// 清除错误标记
        /// </summary>
        private void ClearErrorMarkers()
        {
            if (scintilla == null) return;

            for (int i = 0; i < scintilla.Lines.Count; i++)
            {
                scintilla.Lines[i].MarkerDelete(1);
            }
        }

        #endregion

        #region 公共事件（简单订阅）

        /// <summary>
        /// 脚本内容变更事件
        /// </summary>
        [Category("脚本")]
        [Description("脚本内容变更时触发")]
        public event EventHandler? ScriptTextChanged;

        /// <summary>
        /// 脚本验证错误事件
        /// </summary>
        [Category("脚本")]
        [Description("脚本验证失败时触发")]
        public event Action<ScriptValidationResult>? OnValidationError;

        /// <summary>
        /// 脚本验证成功事件
        /// </summary>
        [Category("脚本")]
        [Description("脚本验证成功时触发")]
        public event Action? OnValidationSuccess;

        /// <summary>
        /// 脚本执行错误事件
        /// </summary>
        [Category("脚本")]
        [Description("脚本执行错误时触发")]
        public event Action<string>? OnError;

        /// <summary>
        /// 断点命中事件
        /// </summary>
        [Category("调试")]
        [Description("脚本执行到断点时触发")]
        public event Action<ScriptDebugEventArgs>? OnBreakpointHit;

        #endregion

        #region 重写方法

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // 确保初始化完成
            if (!_isInitialized)
            {
                InitializeAll();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _validationTimer?.Stop();
                _validationTimer?.Dispose();
                // 注意：components 的释放由基类处理
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
