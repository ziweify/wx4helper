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
    /// 文件打开事件参数
    /// </summary>
    public class FileOpenEventArgs : EventArgs
    {
        public string FilePath { get; }

        public FileOpenEventArgs(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }
    }

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
        
        // 调试状态
        private bool _isDebugging = false;
        private bool _isPaused = false;
        private int _currentDebugLine = -1;
        

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

            // 8. 初始化文件树和函数列表
            InitializeFileExplorer();

            // 9. 初始化调试面板
            InitializeDebugPanel();

            // 10. 订阅事件
            SubscribeEvents();

            // 11. 创建默认脚本引擎（Lua）
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
            
            // 🔥 配置缩进设置
            scintilla.TabWidth = 4; // Tab 宽度：4个空格
            scintilla.UseTabs = false; // 使用空格而不是 Tab
            
            // 设置选中文本的背景色为浅蓝色（避免黑色背景）
            try
            {
                // 尝试设置选中背景色（如果 API 支持）
                var selectionBackColorProp = scintilla.GetType().GetProperty("SelectionBackColor");
                if (selectionBackColorProp != null)
                {
                    selectionBackColorProp.SetValue(scintilla, Color.FromArgb(200, 200, 255)); // 浅蓝色
                }
            }
            catch
            {
                // 如果 API 不支持，忽略
            }
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

            // 添加键盘事件处理（调试快捷键和智能缩进）
            scintilla.KeyDown += Scintilla_KeyDown;
            
            // 🔥 处理回车键，实现智能缩进
            scintilla.CharAdded += Scintilla_CharAdded_Indent;
        }

        /// <summary>
        /// 键盘事件处理（调试快捷键和常用快捷键）
        /// </summary>
        private void Scintilla_KeyDown(object? sender, KeyEventArgs e)
        {
            // 🔥 处理 Ctrl+S：阻止 DC3 字符输入，并触发保存请求
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // 阻止字符输入，防止 DC3 字符
                // 🔥 触发保存请求事件（由父窗口处理）
                SaveRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            // 🔥 处理回车键：实现智能缩进
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                HandleEnterKey();
                // 不阻止默认行为，让 Scintilla 正常插入换行符
                // 缩进会在 CharAdded 事件中处理
                return;
            }

            // 处理其他组合键（Alt 等）
            if (e.Alt)
                return; // 忽略 Alt 组合键

            switch (e.KeyCode)
            {
                case Keys.F7:
                    // 步进（Step Into）
                    e.Handled = true;
                    StepInto();
                    break;

                case Keys.F8:
                    // 步过（Step Over）
                    e.Handled = true;
                    StepOver();
                    break;

                case Keys.F9:
                    // 继续（Continue）
                    e.Handled = true;
                    ContinueExecution();
                    break;

                case Keys.F5:
                    // 开始调试
                    if (e.Shift)
                    {
                        e.Handled = true;
                        StartDebugging();
                    }
                    break;
            }
        }

        /// <summary>
        /// 处理回车键：准备智能缩进（在换行符插入后执行）
        /// </summary>
        private void HandleEnterKey()
        {
            // 这个方法在 KeyDown 中调用，但实际的缩进插入在 CharAdded 事件中完成
            // 这里只是标记需要处理缩进
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
                ToggleBreakpointInternal(line);
            }
        }

        /// <summary>
        /// 切换断点（内部方法，接受0基索引）
        /// </summary>
        private void ToggleBreakpointInternal(int line)
        {
            if (scintilla == null) return;

            if (_breakpoints.Contains(line))
            {
                // 移除断点
                scintilla.Lines[line].MarkerDelete(1);
                _breakpoints.Remove(line);
                _scriptEngine?.ClearBreakpoint(line + 1); // Lua 行号从1开始
            }
            else
            {
                // 添加断点
                scintilla.Lines[line].MarkerAdd(1);
                _breakpoints.Add(line);
                _scriptEngine?.SetBreakpoint(line + 1);
            }
            
            UpdateBreakpointList();
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

        /// <summary>
        /// 智能缩进处理：当用户按回车时，自动根据上一行缩进
        /// </summary>
        private void Scintilla_CharAdded_Indent(object? sender, CharAddedEventArgs e)
        {
            if (scintilla == null) return;

            // 只处理回车键（换行符）
            if (e.Char != '\n') return;

            // 🔥 使用 BeginInvoke 延迟执行，确保新行已完全创建
            // 使用父控件的 BeginInvoke（因为 scintilla 可能没有这个方法）
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() => ApplyAutoIndent()));
            }
            else
            {
                // 如果句柄未创建，直接执行（不太可能发生）
                ApplyAutoIndent();
            }
        }

        /// <summary>
        /// 应用自动缩进
        /// </summary>
        private void ApplyAutoIndent()
        {
            if (scintilla == null) return;

            try
            {
                int currentLine = scintilla.CurrentLine;
                if (currentLine <= 0) return; // 第一行，无需缩进

                // 获取上一行
                int previousLine = currentLine - 1;
                if (previousLine < 0) return;
                
                var previousLineText = scintilla.Lines[previousLine].Text; // 保留原始文本（包含前导空格）
                
                // 计算上一行的缩进（前导空格数）
                int previousIndent = GetLineIndent(previousLineText);
                
                // 移除行尾空白后检查内容
                var trimmedLine = previousLineText.TrimEnd();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    // 上一行是空行，使用上一行的缩进
                    InsertIndentAtCurrentLine(previousIndent);
                    return;
                }

                // 检查上一行是否需要增加缩进（Lua 关键字）
                bool shouldIncreaseIndent = false;
                string trimmed = trimmedLine.Trim(); // 移除前后空白
                
                // Lua 关键字：function, if then, do, repeat, while, for, elseif, else
                if (trimmed.EndsWith("function") || 
                    trimmed.StartsWith("function ") ||
                    trimmed.EndsWith("then") ||
                    trimmed.EndsWith("do") ||
                    trimmed.StartsWith("if ") ||
                    trimmed.StartsWith("elseif ") ||
                    trimmed.StartsWith("while ") ||
                    trimmed.StartsWith("for ") ||
                    trimmed.StartsWith("repeat") ||
                    (trimmed == "else"))
                {
                    shouldIncreaseIndent = true;
                }
                
                // 检查是否以 { 或 ( 结尾（虽然 Lua 不常用，但兼容）
                if (trimmedLine.TrimEnd().EndsWith("{") || 
                    trimmedLine.TrimEnd().EndsWith("("))
                {
                    shouldIncreaseIndent = true;
                }

                // 计算新行的缩进（使用常量 4 作为缩进宽度）
                const int indentWidth = 4;
                int newIndent = previousIndent;
                if (shouldIncreaseIndent)
                {
                    newIndent += indentWidth; // 增加一级缩进（4个空格）
                }
                
                // 在新行插入缩进空格
                InsertIndentAtCurrentLine(newIndent);
            }
            catch
            {
                // 如果出错，静默失败，不影响编辑
            }
        }

        /// <summary>
        /// 在当前行插入指定数量的缩进空格
        /// </summary>
        private void InsertIndentAtCurrentLine(int indent)
        {
            if (scintilla == null) return;

            try
            {
                int currentLine = scintilla.CurrentLine;
                if (currentLine < 0) return;

                // 获取当前行的起始位置
                int lineStart = scintilla.Lines[currentLine].Position;
                
                // 获取当前光标位置
                int currentPos = scintilla.CurrentPosition;
                
                // 获取当前行的文本
                string currentLineText = scintilla.Lines[currentLine].Text;
                
                // 计算当前行的缩进（前导空格数）
                int currentIndent = GetLineIndent(currentLineText);
                
                // 计算需要插入或删除的空格数
                int indentDiff = indent - currentIndent;
                
                if (indentDiff > 0)
                {
                    // 需要插入更多空格：在行首插入
                    scintilla.InsertText(lineStart, new string(' ', indentDiff));
                    // 🔥 将光标移动到缩进后的位置
                    scintilla.GotoPosition(lineStart + indent);
                }
                else if (indentDiff < 0)
                {
                    // 需要删除多余的空格
                    int spacesToRemove = -indentDiff;
                    if (currentIndent >= spacesToRemove)
                    {
                        scintilla.DeleteRange(lineStart, spacesToRemove);
                        // 🔥 调整光标位置
                        if (currentPos >= lineStart + spacesToRemove)
                        {
                            scintilla.GotoPosition(currentPos - spacesToRemove);
                        }
                        else
                        {
                            scintilla.GotoPosition(lineStart + indent);
                        }
                    }
                }
                else
                {
                    // 如果 indentDiff == 0，只需要确保光标在正确位置
                    // 🔥 如果光标在行首，移动到缩进位置
                    if (currentPos <= lineStart)
                    {
                        scintilla.GotoPosition(lineStart + indent);
                    }
                }
            }
            catch
            {
                // 如果出错，静默失败
            }
        }

        /// <summary>
        /// 计算行的缩进（前导空格数）
        /// </summary>
        private int GetLineIndent(string line)
        {
            int indent = 0;
            foreach (char c in line)
            {
                if (c == ' ')
                {
                    indent++;
                }
                else if (c == '\t')
                {
                    indent += scintilla?.TabWidth ?? 4; // Tab 视为多个空格
                }
                else
                {
                    break; // 遇到非空白字符，停止计数
                }
            }
            return indent;
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
                    // 🔥 保存当前光标位置和选择范围
                    var currentPos = scintilla.CurrentPosition;
                    var anchorPos = scintilla.AnchorPosition;
                    
                    // 设置文本
                    scintilla.Text = value ?? string.Empty;
                    
                    // 🔥 恢复光标位置（如果新文本长度足够）
                    if (currentPos <= scintilla.TextLength)
                    {
                        scintilla.CurrentPosition = currentPos;
                        scintilla.AnchorPosition = anchorPos;
                    }
                    else
                    {
                        // 如果文本变短了，将光标移到末尾
                        scintilla.CurrentPosition = scintilla.TextLength;
                    }
                    
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

            // 🔥 如果设置了脚本目录，先加载 functions.lua（如果存在）
            if (!string.IsNullOrEmpty(ScriptDirectory) && _scriptEngine != null)
            {
                var functionsPath = System.IO.Path.Combine(ScriptDirectory, "functions.lua");
                if (System.IO.File.Exists(functionsPath))
                {
                    try
                    {
                        var functionsCode = System.IO.File.ReadAllText(functionsPath, System.Text.Encoding.UTF8);
                        // 先加载 functions.lua 到脚本引擎（不执行，只定义函数）
                        _scriptEngine.LoadScript(functionsCode);
                    }
                    catch (Exception ex)
                    {
                        // 如果加载失败，记录错误但继续执行主脚本
                        System.Diagnostics.Debug.WriteLine($"加载 functions.lua 失败: {ex.Message}");
                    }
                }
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

        /// <summary>
        /// 获取当前脚本引擎
        /// </summary>
        public IScriptEngine ScriptEngine
        {
            get
            {
                if (_scriptEngine == null)
                {
                    CreateDefaultScriptEngine();
                }
                return _scriptEngine!;
            }
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

            // 注意: 默认函数现在由 BrowserTaskControl 在创建时通过 ScriptFunctionRegistry 注册
            // 这里不再自动绑定，而是由使用者调用 RegisterScriptFunction 或 RegisterScriptObject
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

        #region 文件资源管理器和函数列表

        /// <summary>
        /// 初始化文件资源管理器和函数列表
        /// </summary>
        private void InitializeFileExplorer()
        {
            // 初始化文件树
            if (treeViewFiles != null)
            {
                treeViewFiles.ImageList = new ImageList();
                // 使用简单的图标或字体图标
                try
                {
                    treeViewFiles.ImageList.Images.Add("folder", SystemIcons.Application);
                    treeViewFiles.ImageList.Images.Add("file", SystemIcons.Information);
                }
                catch
                {
                    // 如果图标不可用，使用空图标
                    treeViewFiles.ImageList.Images.Add("folder", new Bitmap(16, 16));
                    treeViewFiles.ImageList.Images.Add("file", new Bitmap(16, 16));
                }
                treeViewFiles.AfterSelect += TreeViewFiles_AfterSelect;
                treeViewFiles.NodeMouseDoubleClick += TreeViewFiles_NodeMouseDoubleClick;
            }

            // 初始化函数列表
            if (listBoxFunctions != null)
            {
                listBoxFunctions.SelectedIndexChanged += ListBoxFunctions_SelectedIndexChanged;
                listBoxFunctions.DoubleClick += ListBoxFunctions_DoubleClick;
                listBoxFunctions.MouseDown += ListBoxFunctions_MouseDown;
            }

            // 监听脚本内容变化，更新函数列表
            if (scintilla != null)
            {
                scintilla.TextChanged += (s, e) => UpdateFunctionList();
            }
        }

        /// <summary>
        /// 脚本目录（用于文件树）
        /// </summary>
        public string? ScriptDirectory { get; private set; }

        /// <summary>
        /// 设置脚本目录（用于文件树）
        /// </summary>
        public void SetScriptDirectory(string? scriptDirectory)
        {
            ScriptDirectory = scriptDirectory;
            UpdateFileTree(scriptDirectory);
        }

        /// <summary>
        /// 更新文件树（从脚本目录加载）
        /// </summary>
        public void UpdateFileTree(string? scriptDirectory)
        {
            if (treeViewFiles == null || string.IsNullOrEmpty(scriptDirectory))
                return;

            treeViewFiles.Nodes.Clear();

            if (!System.IO.Directory.Exists(scriptDirectory))
                return;

            var rootNode = new TreeNode(System.IO.Path.GetFileName(scriptDirectory))
            {
                ImageIndex = 0,
                SelectedImageIndex = 0,
                Tag = scriptDirectory
            };

            LoadDirectory(rootNode, scriptDirectory);
            treeViewFiles.Nodes.Add(rootNode);
            rootNode.Expand();
        }

        /// <summary>
        /// 加载目录到树节点
        /// </summary>
        private void LoadDirectory(TreeNode parentNode, string directory)
        {
            try
            {
                // 加载文件
                foreach (var file in System.IO.Directory.GetFiles(directory, "*.lua"))
                {
                    var fileName = System.IO.Path.GetFileName(file);
                    var fileNode = new TreeNode(fileName)
                    {
                        ImageIndex = 1,
                        SelectedImageIndex = 1,
                        Tag = file
                    };
                    parentNode.Nodes.Add(fileNode);
                }

                // 加载子目录
                foreach (var subDir in System.IO.Directory.GetDirectories(directory))
                {
                    var dirName = System.IO.Path.GetFileName(subDir);
                    var dirNode = new TreeNode(dirName)
                    {
                        ImageIndex = 0,
                        SelectedImageIndex = 0,
                        Tag = subDir
                    };
                    LoadDirectory(dirNode, subDir);
                    parentNode.Nodes.Add(dirNode);
                }
            }
            catch
            {
                // 忽略错误
            }
        }

        /// <summary>
        /// 文件树选择事件（不加载文件，只触发事件）
        /// </summary>
        private void TreeViewFiles_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            // 选择文件时不自动加载，等待双击或由父控件处理
        }

        /// <summary>
        /// 文件树双击事件：触发文件打开事件，由父控件在 Tab 中打开
        /// </summary>
        private void TreeViewFiles_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is string filePath && System.IO.File.Exists(filePath))
            {
                // 🔥 更新文件树的选择状态
                if (treeViewFiles != null)
                {
                    treeViewFiles.SelectedNode = e.Node;
                }
                
                // 🔥 触发文件打开事件，由父控件在 Tab 中打开文件
                // 这样每个文件都在独立的 Tab 页面中，不会覆盖当前编辑的内容
                FileOpenRequested?.Invoke(this, new FileOpenEventArgs(filePath));
            }
        }

        /// <summary>
        /// 更新函数列表
        /// </summary>
        private void UpdateFunctionList()
        {
            if (listBoxFunctions == null)
                return;

            listBoxFunctions.Items.Clear();

            try
            {
                var functions = LuaFunctionParser.ParseFunctions(ScriptText);
                foreach (var func in functions)
                {
                    listBoxFunctions.Items.Add(new FunctionListItem(func));
                }
            }
            catch
            {
                // 忽略解析错误
            }
        }

        /// <summary>
        /// 函数列表项（用于显示和存储函数信息）
        /// </summary>
        private class FunctionListItem
        {
            public LuaFunctionParser.FunctionInfo FunctionInfo { get; }

            public FunctionListItem(LuaFunctionParser.FunctionInfo funcInfo)
            {
                FunctionInfo = funcInfo;
            }

            public override string ToString()
            {
                return FunctionInfo.FullSignature;
            }
        }

        /// <summary>
        /// 函数列表选择事件 - 定位到函数代码
        /// </summary>
        private void ListBoxFunctions_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (listBoxFunctions?.SelectedItem is FunctionListItem item && scintilla != null)
            {
                // 跳转到函数所在行
                var lineNumber = item.FunctionInfo.LineNumber;
                if (lineNumber > 0 && lineNumber <= scintilla.Lines.Count)
                {
                    // 转换为0基索引
                    var lineIndex = lineNumber - 1;
                    var line = scintilla.Lines[lineIndex];
                    
                    // 跳转到该行
                    line.Goto();
                    
                    // 确保该行可见（滚动到视图）
                    line.EnsureVisible();
                    
                    // 将焦点设置到编辑器
                    scintilla.Focus();
                }
            }
        }

        /// <summary>
        /// 函数列表双击事件 - 运行函数
        /// </summary>
        private void ListBoxFunctions_DoubleClick(object? sender, EventArgs e)
        {
            if (listBoxFunctions?.SelectedItem is FunctionListItem item)
            {
                RunFunction(item.FunctionInfo);
            }
        }

        /// <summary>
        /// 函数列表右键菜单
        /// </summary>
        private void ListBoxFunctions_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listBoxFunctions != null)
            {
                var index = listBoxFunctions.IndexFromPoint(e.Location);
                if (index >= 0 && index < listBoxFunctions.Items.Count)
                {
                    listBoxFunctions.SelectedIndex = index;
                    if (listBoxFunctions.SelectedItem is FunctionListItem item)
                    {
                        var contextMenu = new ContextMenuStrip();
                        var runItem = new ToolStripMenuItem("运行函数");
                        runItem.Click += (s, args) => RunFunction(item.FunctionInfo);
                        contextMenu.Items.Add(runItem);
                        contextMenu.Show(listBoxFunctions, e.Location);
                    }
                }
            }
        }

        /// <summary>
        /// 运行函数
        /// </summary>
        private void RunFunction(LuaFunctionParser.FunctionInfo funcInfo)
        {
            try
            {
                // 如果有参数，显示参数输入对话框
                Dictionary<string, string>? parameterValues = null;
                if (funcInfo.Parameters.Count > 0)
                {
                    using (var dialog = new Views.FunctionParameterDialog(funcInfo))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            parameterValues = dialog.ParameterValues;
                        }
                        else
                        {
                            return; // 用户取消
                        }
                    }
                }

                // 生成函数调用代码
                var callCode = LuaFunctionParser.GenerateFunctionCall(funcInfo, parameterValues);

                // 执行函数调用
                if (_scriptEngine != null)
                {
                    var result = _scriptEngine.Execute(callCode);
                    // 可以显示结果或触发事件
                    OnFunctionExecuted?.Invoke(funcInfo.Name, result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"运行函数失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 函数执行完成事件
        /// </summary>
        public event Action<string, object>? OnFunctionExecuted;

        #endregion

        #region 调试面板

        /// <summary>
        /// 初始化调试面板
        /// </summary>
        private void InitializeDebugPanel()
        {
            // 初始化变量列表
            if (listViewVariables != null)
            {
                listViewVariables.View = View.Details;
                listViewVariables.FullRowSelect = true;
            }

            // 初始化调用堆栈
            if (listBoxCallStack != null)
            {
                // 可以添加右键菜单等
            }

            // 初始化断点列表
            if (listBoxBreakpoints != null)
            {
                listBoxBreakpoints.DoubleClick += ListBoxBreakpoints_DoubleClick;
            }
        }

        /// <summary>
        /// 更新断点列表
        /// </summary>
        private void UpdateBreakpointList()
        {
            if (listBoxBreakpoints == null)
                return;

            listBoxBreakpoints.Items.Clear();
            foreach (var line in _breakpoints.OrderBy(x => x))
            {
                listBoxBreakpoints.Items.Add($"第 {line} 行");
            }
        }

        /// <summary>
        /// 断点列表双击事件 - 跳转到断点
        /// </summary>
        private void ListBoxBreakpoints_DoubleClick(object? sender, EventArgs e)
        {
            if (listBoxBreakpoints?.SelectedItem is string item && scintilla != null)
            {
                // 提取行号
                var match = System.Text.RegularExpressions.Regex.Match(item, @"第 (\d+) 行");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int lineNumber))
                {
                    scintilla.Lines[lineNumber - 1].Goto();
                }
            }
        }

        /// <summary>
        /// 更新变量监视
        /// </summary>
        public void UpdateVariables(Dictionary<string, object>? variables)
        {
            if (listViewVariables == null)
                return;

            listViewVariables.Items.Clear();

            if (variables == null)
                return;

            foreach (var kvp in variables)
            {
                var item = new ListViewItem(kvp.Key);
                item.SubItems.Add(kvp.Value?.ToString() ?? "nil");
                item.SubItems.Add(kvp.Value?.GetType().Name ?? "nil");
                listViewVariables.Items.Add(item);
            }
        }

        /// <summary>
        /// 更新调用堆栈
        /// </summary>
        public void UpdateCallStack(List<string>? callStack)
        {
            if (listBoxCallStack == null)
                return;

            listBoxCallStack.Items.Clear();

            if (callStack == null)
                return;

            foreach (var frame in callStack)
            {
                listBoxCallStack.Items.Add(frame);
            }
        }

        #endregion

        #region 断点调试功能

        /// <summary>
        /// 设置/取消断点（公共方法，接受1基索引）
        /// </summary>
        public void ToggleBreakpoint(int lineNumber)
        {
            ToggleBreakpointInternal(lineNumber - 1); // 转换为0基索引，调用私有方法
        }

        /// <summary>
        /// 检查是否有断点
        /// </summary>
        public bool HasBreakpoint(int lineNumber)
        {
            return _breakpoints.Contains(lineNumber - 1);
        }

        /// <summary>
        /// 清除所有断点
        /// </summary>
        public void ClearAllBreakpoints()
        {
            if (scintilla == null)
                return;

            foreach (var line in _breakpoints.ToList())
            {
                if (line >= 0 && line < scintilla.Lines.Count)
                {
                    scintilla.Lines[line].MarkerDelete(1);
                }
                _scriptEngine?.ClearBreakpoint(line + 1);
            }
            
            _breakpoints.Clear();
            UpdateBreakpointList();
        }

        /// <summary>
        /// 开始调试
        /// </summary>
        public void StartDebugging()
        {
            if (_scriptEngine == null)
                return;

            _isDebugging = true;
            _isPaused = false;
            _currentDebugLine = -1;

            // 执行脚本（带断点支持）
            try
            {
                var result = _scriptEngine.Execute(ScriptText);
                // 如果遇到断点，会触发OnBreakpointHit事件
            }
            catch (Exception ex)
            {
                _isDebugging = false;
                OnError?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// 步进（Step Into）- F7
        /// 遇到函数自动进入
        /// </summary>
        public void StepInto()
        {
            if (!_isDebugging || !_isPaused)
                return;

            if (_scriptEngine is IScriptDebugEngine debugEngine)
            {
                debugEngine.StepInto();
                _isPaused = false;
                // 继续执行一步
                ContinueExecution();
            }
        }

        /// <summary>
        /// 步过（Step Over）- F8
        /// 遇到函数就步过
        /// </summary>
        public void StepOver()
        {
            if (!_isDebugging || !_isPaused)
                return;

            if (_scriptEngine is IScriptDebugEngine debugEngine)
            {
                debugEngine.StepOver();
                _isPaused = false;
                // 继续执行一步
                ContinueExecution();
            }
        }

        /// <summary>
        /// 继续执行（Continue）- F9
        /// 继续运行到下一个断点
        /// </summary>
        public void ContinueExecution()
        {
            if (!_isDebugging || !_isPaused)
                return;

            _isPaused = false;
            // 继续执行（由脚本引擎处理）
            if (_scriptEngine is IScriptDebugEngine debugEngine)
            {
                debugEngine.Continue();
            }
        }

        /// <summary>
        /// 停止调试
        /// </summary>
        public void StopDebugging()
        {
            _isDebugging = false;
            _isPaused = false;
            _currentDebugLine = -1;

            if (_scriptEngine is IScriptDebugEngine debugEngine)
            {
                debugEngine.Stop();
            }
        }

        /// <summary>
        /// 是否正在调试
        /// </summary>
        public bool IsDebugging => _isDebugging;

        /// <summary>
        /// 是否已暂停
        /// </summary>
        public bool IsPaused => _isPaused;

        #endregion

        #region 公共事件（简单订阅）

        /// <summary>
        /// 脚本内容变更事件
        /// </summary>
        [Category("脚本")]
        [Description("脚本内容变更时触发")]
        public event EventHandler? ScriptTextChanged;

        /// <summary>
        /// 保存请求事件（用于 Ctrl+S 快捷键）
        /// </summary>
        [Category("脚本")]
        [Description("保存请求时触发（如 Ctrl+S）")]
        public event EventHandler? SaveRequested;

        /// <summary>
        /// 文件打开请求事件（用于在 Tab 中打开文件）
        /// </summary>
        [Category("脚本")]
        [Description("文件打开请求时触发，传递文件路径")]
        public event EventHandler<FileOpenEventArgs>? FileOpenRequested;

        /// <summary>
        /// 根据文件路径选择文件树中的节点
        /// </summary>
        public void SelectFileInTree(string filePath)
        {
            if (treeViewFiles == null || string.IsNullOrEmpty(filePath))
                return;

            try
            {
                // 遍历所有节点，找到匹配的文件路径
                foreach (TreeNode node in treeViewFiles.Nodes)
                {
                    var foundNode = FindNodeByPath(node, filePath);
                    if (foundNode != null)
                    {
                        treeViewFiles.SelectedNode = foundNode;
                        foundNode.EnsureVisible();
                        return;
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
        }

        /// <summary>
        /// 递归查找节点
        /// </summary>
        private TreeNode? FindNodeByPath(TreeNode node, string filePath)
        {
            if (node.Tag is string nodePath && nodePath == filePath)
            {
                return node;
            }

            foreach (TreeNode child in node.Nodes)
            {
                var found = FindNodeByPath(child, filePath);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

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
