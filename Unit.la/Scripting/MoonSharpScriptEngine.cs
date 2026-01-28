using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Loaders;

namespace Unit.La.Scripting
{
    /// <summary>
    /// MoonSharp Lua 脚本引擎实现
    /// </summary>
    public class MoonSharpScriptEngine : IScriptDebugEngine
    {
        internal readonly Script _script; // 🔥 改为 internal，供 CustomScriptLoader 访问
        private readonly HashSet<int> _breakpoints = new();
        private bool _isDebugging = false;
        private bool _isPaused = false;
        private int _currentLine = -1;
        private Dictionary<string, object>? _currentVariables = null;
        private List<string>? _currentCallStack = null;
        
        // 🔥 脚本目录（用于 require 加载文件）
        private string? _scriptDirectory;

        public MoonSharpScriptEngine()
        {
            _script = new Script();
            
            // 🔥 注册自定义类型，让 MoonSharp 能够识别
            // WebBridge 用于 Lua 中的 web 对象
            UserData.RegisterType<WebBridge>();
            // ConfigBridge 用于 Lua 中的 config 对象（支持双向绑定）
            UserData.RegisterType<ConfigBridge>();
            
            // .NET 8 不支持 Assembly.GetCallingAssembly()，所以不调用 RegisterAssembly
            // 其他类型将按需自动注册
            
            // 🔥 设置自定义脚本加载器，支持 require 功能
            _script.Options.ScriptLoader = new CustomScriptLoader(this);
        }
        
        /// <summary>
        /// 设置脚本目录（用于 require 加载文件）
        /// </summary>
        public void SetScriptDirectory(string? scriptDirectory)
        {
            _scriptDirectory = scriptDirectory;
        }
        
        /// <summary>
        /// 获取脚本目录
        /// </summary>
        public string? GetScriptDirectory()
        {
            return _scriptDirectory;
        }
        
        /// <summary>
        /// 自定义脚本加载器，支持从脚本目录加载文件
        /// </summary>
        private class CustomScriptLoader : IScriptLoader
        {
            private readonly MoonSharpScriptEngine _engine;
            
            public CustomScriptLoader(MoonSharpScriptEngine engine)
            {
                _engine = engine;
            }
            
            public object LoadFile(string file, Table globalContext)
            {
                // 🔥 从脚本目录加载文件
                var scriptDir = _engine.GetScriptDirectory();
                if (string.IsNullOrEmpty(scriptDir))
                {
                    throw new ScriptRuntimeException($"无法加载文件 '{file}'：脚本目录未设置");
                }
                
                // 处理不同的文件路径格式
                string filePath;
                if (System.IO.Path.IsPathRooted(file))
                {
                    // 绝对路径
                    filePath = file;
                }
                else if (file.Contains(System.IO.Path.DirectorySeparatorChar) || file.Contains('/'))
                {
                    // 相对路径（相对于脚本目录）
                    filePath = System.IO.Path.Combine(scriptDir, file);
                }
                else
                {
                    // 简单文件名，添加 .lua 扩展名（如果没有）
                    var fileName = file;
                    if (!fileName.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += ".lua";
                    }
                    filePath = System.IO.Path.Combine(scriptDir, fileName);
                }
                
                // 检查文件是否存在
                if (!System.IO.File.Exists(filePath))
                {
                    throw new ScriptRuntimeException($"无法加载文件 '{file}'：文件不存在 ({filePath})");
                }
                
                // 读取文件内容
                try
                {
                    // 🔥 关键：LoadFile 应该返回文件内容（字符串），而不是执行代码
                    // MoonSharp 会自动执行返回的内容，并使用我们传递的源文件名
                    var content = System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    
                    // 🔥 返回文件内容，MoonSharp 会自动使用源文件名执行
                    // 但是我们需要确保源文件名被正确传递
                    // 由于 LoadFile 只返回字符串，我们需要通过其他方式传递文件名
                    // 实际上，MoonSharp 会使用 ResolveFileName 返回的文件名作为源文件名
                    return content;
                }
                catch (Exception ex)
                {
                    throw new ScriptRuntimeException($"加载文件 '{filePath}' 失败: {ex.Message}", ex);
                }
            }
            
            public string ResolveModuleName(string modname, Table globalContext)
            {
                // 🔥 解析模块名（支持 require "module" 或 require "module.lua"）
                // 如果模块名不包含路径分隔符，直接返回
                if (!modname.Contains(System.IO.Path.DirectorySeparatorChar) && 
                    !modname.Contains('/'))
                {
                    return modname;
                }
                
                // 如果包含路径，返回相对路径
                return modname;
            }
            
            public string ResolveFileName(string filename, Table globalContext)
            {
                // 🔥 解析文件名，返回完整的文件路径
                // 这样 MoonSharp 可以在错误报告中显示正确的文件名
                var scriptDir = _engine.GetScriptDirectory();
                if (string.IsNullOrEmpty(scriptDir))
                {
                    return filename;
                }
                
                // 处理不同的文件路径格式
                string filePath;
                if (System.IO.Path.IsPathRooted(filename))
                {
                    // 绝对路径
                    filePath = filename;
                }
                else if (filename.Contains(System.IO.Path.DirectorySeparatorChar) || filename.Contains('/'))
                {
                    // 相对路径（相对于脚本目录）
                    filePath = System.IO.Path.Combine(scriptDir, filename);
                }
                else
                {
                    // 简单文件名，添加 .lua 扩展名（如果没有）
                    var fileName = filename;
                    if (!fileName.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += ".lua";
                    }
                    filePath = System.IO.Path.Combine(scriptDir, fileName);
                }
                
                // 🔥 返回文件名（不含路径），这样错误信息会更清晰
                // 或者返回完整路径，取决于 MoonSharp 如何处理
                return System.IO.Path.GetFileName(filePath);
            }
        }

        /// <summary>
        /// 加载脚本（不执行，只定义函数和变量）
        /// 用于加载 functions.lua 等库文件
        /// 接口实现
        /// </summary>
        public void LoadScript(string scriptCode)
        {
            LoadScript(scriptCode, null);
        }

        // 🔥 存储当前加载的脚本文件名映射（用于错误报告）
        private readonly Dictionary<string, string> _scriptFileNames = new Dictionary<string, string>();
        
        // 🔥 存储函数名到文件名的映射（用于错误报告）
        // 当加载 functions.lua 时，记录其中的函数名
        private readonly Dictionary<string, string> _functionToFileMap = new Dictionary<string, string>();

        /// <summary>
        /// 加载脚本（不执行，只定义函数和变量）
        /// 用于加载 functions.lua 等库文件
        /// 重载版本，支持源文件名（内部使用）
        /// </summary>
        /// <param name="scriptCode">脚本代码</param>
        /// <param name="sourceFileName">源文件名（用于错误报告），例如 "functions.lua"</param>
        internal void LoadScript(string scriptCode, string? sourceFileName)
        {
            try
            {
                // 🔥 关键改进：使用 DoString 的重载版本，传递源文件名
                // 这样 MoonSharp 就能在错误报告中包含正确的文件名
                if (!string.IsNullOrEmpty(sourceFileName))
                {
                    // 使用源文件名作为代码标识符
                    _script.DoString(scriptCode, null, sourceFileName);
                }
                else
                {
                    // 如果没有提供文件名，使用默认方式
                    _script.DoString(scriptCode);
                }
                
                // 🔥 如果提供了源文件名，存储映射（用于后续错误报告）
                if (!string.IsNullOrEmpty(sourceFileName))
                {
                    // 使用脚本代码的哈希作为键，存储文件名
                    var scriptHash = scriptCode.GetHashCode().ToString();
                    _scriptFileNames[scriptHash] = sourceFileName;
                    
                    // 🔥 如果加载的是 functions.lua，解析其中的函数名并建立映射
                    if (sourceFileName == "functions.lua" || sourceFileName.EndsWith("functions.lua"))
                    {
                        // 简单的函数名提取：查找 "function functionName(" 模式
                        var functionMatches = System.Text.RegularExpressions.Regex.Matches(scriptCode, 
                            @"function\s+(\w+)\s*\(");
                        foreach (System.Text.RegularExpressions.Match match in functionMatches)
                        {
                            if (match.Groups.Count > 1)
                            {
                                var funcName = match.Groups[1].Value;
                                _functionToFileMap[funcName] = sourceFileName;
                                System.Diagnostics.Debug.WriteLine($"映射函数 {funcName} -> {sourceFileName}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果加载失败，抛出异常
                throw new InvalidOperationException($"加载脚本失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 执行脚本
        /// 接口实现
        /// </summary>
        public ScriptResult Execute(string scriptCode, Dictionary<string, object>? context = null)
        {
            return Execute(scriptCode, context, null);
        }

        /// <summary>
        /// 执行脚本
        /// 重载版本，支持源文件名
        /// </summary>
        public ScriptResult Execute(string scriptCode, Dictionary<string, object>? context, string? sourceFileName)
        {
            try
            {
                // 加载上下文
                if (context != null)
                {
                    foreach (var kvp in context)
                    {
                        _script.Globals[kvp.Key] = DynValue.FromObject(_script, kvp.Value);
                    }
                }

                // 🔥 使用新的生命周期执行方式，传递源文件名
                return ExecuteWithLifecycle(scriptCode, sourceFileName);
            }
            catch (ScriptRuntimeException ex)
            {
                // 运行时错误 - 提取详细信息
                var errorInfo = ExtractErrorInfo(ex, null);
                
                OnError?.Invoke(this, new ScriptErrorEventArgs
                {
                    Error = errorInfo.Message,
                    LineNumber = errorInfo.LineNumber
                });

                return new ScriptResult
                {
                    Success = false,
                    Error = errorInfo.Message,
                    LineNumber = errorInfo.LineNumber,
                    Output = errorInfo.FullMessage
                };
            }
            catch (SyntaxErrorException ex)
            {
                // 语法错误 - 提取详细信息
                var errorInfo = ExtractErrorInfo(ex, null);
                
                OnError?.Invoke(this, new ScriptErrorEventArgs
                {
                    Error = errorInfo.Message,
                    LineNumber = errorInfo.LineNumber
                });

                return new ScriptResult
                {
                    Success = false,
                    Error = errorInfo.Message,
                    LineNumber = errorInfo.LineNumber,
                    Output = errorInfo.FullMessage
                };
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ScriptErrorEventArgs
                {
                    Error = ex.Message,
                    LineNumber = 0
                });

                return new ScriptResult
                {
                    Success = false,
                    Error = ex.Message,
                    Output = ex.ToString()
                };
            }
        }

        /// <summary>
        /// 使用完整生命周期执行脚本：main() -> error() -> exit()
        /// 强制要求脚本必须包含3个函数：main, error, exit
        /// </summary>
        /// <param name="scriptCode">脚本代码</param>
        /// <param name="sourceFileName">源文件名（用于错误报告），例如 "main.lua"</param>
        private ScriptResult ExecuteWithLifecycle(string scriptCode, string? sourceFileName = null)
        {
            bool hasError = false;
            string? errorMessage = null;
            int errorLineNumber = 0;
            string? errorTrace = null;
            object? result = null;
            DynValue? exitFunc = null;  // 在外层声明，供 finally 使用

            // 🔥 存储当前执行的脚本文件名（用于错误报告）
            string? currentSourceFileName = sourceFileName;
            
            // 🔥 存储当前执行的脚本代码（用于错误分析）
            string? currentScriptCode = scriptCode;
            
            try
            {
                // 1. 🔥 关键改进：使用 DoString 的重载版本，传递源文件名
                // 这样 MoonSharp 就能在错误报告中包含正确的文件名和行号
                if (!string.IsNullOrEmpty(currentSourceFileName))
                {
                    _script.DoString(scriptCode, null, currentSourceFileName);
                }
                else
                {
                    _script.DoString(scriptCode);
                }
            }
            catch (IndexOutOfRangeException indexEx)
            {
                // 🔥 捕获数组越界异常，这通常是 MoonSharp 内部错误或函数绑定问题
                hasError = true;
                errorMessage = $"脚本加载错误: 数组越界异常\n" +
                              $"   这可能是由于函数参数不匹配或函数绑定问题导致的\n" +
                              $"   原始错误: {indexEx.Message}\n" +
                              $"   堆栈: {indexEx.StackTrace}";
                errorTrace = indexEx.ToString();
                
                // 尝试从堆栈中提取更多信息
                var stackTrace = indexEx.StackTrace ?? "";
                if (stackTrace.Contains("Processing_Loop"))
                {
                    errorMessage += "\n   提示: 错误发生在循环处理中，请检查循环函数（如 loop）的使用是否正确";
                }
                
                return new ScriptResult
                {
                    Success = false,
                    Error = errorMessage,
                    LineNumber = 0,
                    Output = errorTrace
                };
            }
            catch (Exception loadEx)
            {
                // 脚本加载阶段的其他错误
                hasError = true;
                errorMessage = $"脚本加载错误: {loadEx.Message}";
                errorTrace = loadEx.ToString();
                
                return new ScriptResult
                {
                    Success = false,
                    Error = errorMessage,
                    LineNumber = 0,
                    Output = errorTrace
                };
            }
            
            try
            {

                // 2. 🔥 验证3个必须函数是否都存在
                var mainFunc = _script.Globals.Get("main");
                var errorFunc = _script.Globals.Get("error");
                exitFunc = _script.Globals.Get("exit");  // 赋值给外层变量

                var missingFunctions = new System.Text.StringBuilder();
                
                if (mainFunc.IsNil() || mainFunc.Type != DataType.Function)
                {
                    missingFunctions.AppendLine("  - function main()");
                }
                
                if (errorFunc.IsNil() || errorFunc.Type != DataType.Function)
                {
                    missingFunctions.AppendLine("  - function error(errorInfo)");
                }
                
                if (exitFunc.IsNil() || exitFunc.Type != DataType.Function)
                {
                    missingFunctions.AppendLine("  - function exit()");
                }

                // 如果有缺失的函数，返回错误
                if (missingFunctions.Length > 0)
                {
                    var errorMsg = $"❌ 脚本不符合规范！必须包含以下3个函数：\n{missingFunctions}\n" +
                                   "标准脚本结构：\n" +
                                   "function main()\n" +
                                   "    -- 主业务逻辑\n" +
                                   "end\n\n" +
                                   "function error(errorInfo)\n" +
                                   "    -- 异常处理\n" +
                                   "    return true  -- 或 false\n" +
                                   "end\n\n" +
                                   "function exit()\n" +
                                   "    -- 清理工作\n" +
                                   "end";
                    
                    return new ScriptResult
                    {
                        Success = false,
                        Error = errorMsg,
                        LineNumber = 0,
                        Output = errorMsg
                    };
                }

                // 3. 执行 main() 函数
                try
                {
                    // 🔥 检查 mainFunc 是否为 null
                    if (mainFunc == null || mainFunc.IsNil())
                    {
                        throw new InvalidOperationException("main() 函数未找到或为 null");
                    }
                    
                    // 🔥 检查 _script 是否为 null
                    if (_script == null)
                    {
                        throw new InvalidOperationException("脚本引擎未初始化");
                    }
                    
                    var mainResult = _script.Call(mainFunc);
                    result = mainResult.ToObject();
                }
                catch (NullReferenceException nullEx)
                {
                    // 🔥 捕获空引用异常，提供更详细的错误信息
                    hasError = true;
                    errorMessage = $"❌ 空引用异常 (NullReferenceException)\n" +
                                  $"   错误位置: main() 函数执行时\n" +
                                  $"   可能原因:\n" +
                                  $"     1. 脚本中调用了 nil 值（函数或变量未定义）\n" +
                                  $"     2. 函数参数传递错误\n" +
                                  $"     3. 对象方法调用时对象为 nil\n" +
                                  $"   原始错误: {nullEx.Message}\n" +
                                  $"   堆栈跟踪:\n{nullEx.StackTrace}";
                    errorTrace = nullEx.ToString();
                    
                    // 尝试从堆栈中提取更多信息
                    var stackTrace = nullEx.StackTrace ?? "";
                    if (stackTrace.Contains("Processing_Loop"))
                    {
                        errorMessage += "\n   提示: 错误发生在循环处理中，请检查循环函数（如 loop、while）的使用是否正确";
                    }
                    if (stackTrace.Contains("Call"))
                    {
                        errorMessage += "\n   提示: 错误发生在函数调用中，请检查函数是否存在、参数是否正确";
                    }
                    
                    // 调用 error() 回调（如果存在）
                    try
                    {
                        if (errorFunc != null && !errorFunc.IsNil() && errorFunc.Type == DataType.Function)
                        {
                            var errorInfoTable = new Table(_script);
                            errorInfoTable["message"] = DynValue.NewString(errorMessage);
                            errorInfoTable["lineNumber"] = DynValue.NewNumber(0);
                            errorInfoTable["type"] = DynValue.NewString("NullReferenceException");
                            
                            var errorResult = _script.Call(errorFunc, errorInfoTable);
                            
                            if (errorResult.Type == DataType.Boolean && errorResult.Boolean)
                            {
                                // error() 返回 true，表示忽略异常，继续执行
                                hasError = false;
                                errorMessage = null;
                            }
                        }
                    }
                    catch
                    {
                        // error() 调用失败，忽略
                    }
                }
                catch (ScriptRuntimeException ex)
                {
                    // main() 执行时发生异常
                    hasError = true;
                    var errorInfo = ExtractErrorInfo(ex, currentSourceFileName, currentScriptCode);
                    errorMessage = errorInfo.Message;
                    errorLineNumber = errorInfo.LineNumber;
                    errorTrace = errorInfo.FullMessage;

                    // 4. 调用 error() 回调（已强制要求，必定存在）
                    try
                    {
                        // 创建错误信息表
                        var errorInfoTable = new Dictionary<string, object>
                        {
                            { "message", errorMessage },
                            { "lineNumber", errorLineNumber },
                            { "trace", errorTrace ?? "" }
                        };

                        // 调用 error() 函数
                        var errorResult = _script.Call(errorFunc, errorInfoTable);
                        
                        // 检查返回值
                        if (errorResult.Type == DataType.Boolean && errorResult.Boolean)
                        {
                            // 返回 true，忽略异常，继续执行
                            hasError = false;
                            errorMessage = null;
                        }
                        // 返回 false 或其他值，停止执行（保持 hasError = true）
                    }
                    catch (Exception errorHandlerEx)
                    {
                        // error() 函数本身出错，记录但不影响原始错误
                        errorMessage = $"原始错误: {errorMessage}\nerror() 函数执行失败: {errorHandlerEx.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                // 脚本加载或其他阶段的错误
                hasError = true;
                errorMessage = ex.Message;
                errorTrace = ex.ToString();
            }
            finally
            {
                // 5. 无论如何，调用 exit() 函数（如果已成功加载）
                try
                {
                    // 🔥 检查 exitFunc 是否为 null 且是有效的函数
                    if (exitFunc != null && !exitFunc.IsNil() && exitFunc.Type == DataType.Function)
                    {
                        _script.Call(exitFunc);
                    }
                }
                catch (Exception exitEx)
                {
                    // exit() 函数出错，记录但不影响最终结果
                    if (hasError)
                    {
                        errorMessage = $"{errorMessage}\nexit() 函数执行失败: {exitEx.Message}";
                    }
                    else
                    {
                        hasError = true;
                        errorMessage = $"exit() 函数执行失败: {exitEx.Message}";
                    }
                }
            }

            // 6. 返回最终结果
            if (hasError)
            {
                return new ScriptResult
                {
                    Success = false,
                    Error = errorMessage,
                    LineNumber = errorLineNumber,
                    Output = errorTrace
                };
            }
            else
            {
                return new ScriptResult
                {
                    Success = true,
                    Data = result,
                    Output = result?.ToString() ?? "null"
                };
            }
        }

        /// <summary>
        /// 从异常中提取详细的错误信息
        /// 改进：遍历整个调用栈，找到实际出错的位置（最深层），并构建完整的调用链
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="sourceFileName">源文件名（如果已知），例如 "main.lua"</param>
        /// <param name="scriptCode">脚本代码（用于分析错误行的函数调用），例如 main.lua 的完整代码</param>
        private (string Message, int LineNumber, int ColumnNumber, string FullMessage) ExtractErrorInfo(Exception ex, string? sourceFileName = null, string? scriptCode = null)
        {
            string fullMessage = ex.ToString();
            string message = ex.Message;
            int lineNumber = 0;
            int columnNumber = 0;
            var callStackDetails = new System.Text.StringBuilder();

            // 🔥 处理 NullReferenceException
            if (ex is NullReferenceException nullEx)
            {
                message = $"空引用异常: {nullEx.Message}";
                fullMessage = nullEx.ToString();
                
                // 尝试从堆栈中提取更多信息
                var stackTrace = nullEx.StackTrace ?? "";
                if (stackTrace.Contains("Processing_Loop"))
                {
                    message += "\n   错误发生在循环处理中，可能是循环函数（如 loop、while）使用错误";
                }
                if (stackTrace.Contains("Call"))
                {
                    message += "\n   错误发生在函数调用中，可能是函数不存在或参数错误";
                }
                
                // 构建详细的错误信息
                callStackDetails.AppendLine("📋 调用栈信息：");
                callStackDetails.AppendLine($"   错误类型: NullReferenceException");
                callStackDetails.AppendLine($"   错误消息: {nullEx.Message}");
                if (!string.IsNullOrEmpty(stackTrace))
                {
                    callStackDetails.AppendLine($"   堆栈跟踪:\n{stackTrace}");
                }
                
                fullMessage = $"❌ 错误: {message}\n\n{callStackDetails}\n=== 完整堆栈跟踪 ===\n{fullMessage}";
                return (message, lineNumber, columnNumber, fullMessage);
            }

            // MoonSharp 异常通常包含 DecoratedMessage
            if (ex is ScriptRuntimeException runtimeEx)
            {
                message = runtimeEx.DecoratedMessage ?? runtimeEx.Message;
                fullMessage = runtimeEx.ToString();
                
                // 🔥 改进：从错误消息中提取更多信息
                // 尝试解析 "chunk_2:(10,10-30): attempt to call a nil value" 或 "main.lua:(10,10-30): attempt to call a nil value" 这样的格式
                // 如果 MoonSharp 使用了源文件名，我们可以从 DecoratedMessage 中提取
                var messageMatch = System.Text.RegularExpressions.Regex.Match(message, 
                    @"([\w\.]+):\((\d+),(\d+)-(\d+)\):\s*(.+)");
                if (messageMatch.Success)
                {
                    // 提取源文件名（如果可用）
                    var extractedFileName = messageMatch.Groups[1].Value;
                    // 如果提取的文件名不是 "chunk_X" 格式，说明 MoonSharp 使用了我们传递的源文件名
                    if (!extractedFileName.StartsWith("chunk_") && !string.IsNullOrEmpty(extractedFileName))
                    {
                        // 更新 sourceFileName（如果之前没有提供）
                        if (string.IsNullOrEmpty(sourceFileName))
                        {
                            sourceFileName = extractedFileName;
                        }
                    }
                    
                    // 如果从消息中解析到行号和列号，使用它们
                    if (int.TryParse(messageMatch.Groups[2].Value, out int msgLine))
                    {
                        lineNumber = msgLine;
                    }
                    if (int.TryParse(messageMatch.Groups[3].Value, out int msgCol))
                    {
                        columnNumber = msgCol;
                    }
                    // 提取错误描述（去掉位置信息）
                    var errorDesc = messageMatch.Groups[5].Value.Trim();
                    if (!string.IsNullOrEmpty(errorDesc))
                    {
                        message = errorDesc;
                    }
                }
                
                // 🔥 尝试从错误消息中提取被调用的函数名
                // "attempt to call a nil value" 通常意味着调用了一个不存在的函数
                var nilCallMatch = System.Text.RegularExpressions.Regex.Match(message, 
                    @"attempt to call a nil value", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (nilCallMatch.Success)
                {
                    // 🔥 改进：从 DecoratedMessage 中提取函数名
                    // MoonSharp 的 DecoratedMessage 格式可能是: "chunk_2:(10,10-30): attempt to call a nil value"
                    // 我们需要从代码中推断，而不是从错误消息中提取（因为 "a" 可能是 "a nil value" 的一部分）
                    
                    // 尝试从原始消息中提取位置信息，然后提示用户检查该位置的代码
                    var positionMatch = System.Text.RegularExpressions.Regex.Match(runtimeEx.DecoratedMessage ?? runtimeEx.Message, 
                        @"\((\d+),(\d+)-(\d+)\)");
                    if (positionMatch.Success)
                    {
                        message = $"尝试调用一个 nil 值（函数或变量不存在）\n" +
                                 $"   位置: 第 {positionMatch.Groups[1].Value} 行，第 {positionMatch.Groups[2].Value}-{positionMatch.Groups[3].Value} 列\n" +
                                 $"   提示: 请检查该位置的代码，确认函数或变量是否已正确定义";
                    }
                    else
                    {
                        message = "尝试调用一个 nil 值（函数或变量不存在）\n" +
                                 "   提示: 请检查代码，确认函数或变量是否已正确定义";
                    }
                }
                
                // 🔥 改进：遍历整个调用栈，找到实际出错的位置
                if (runtimeEx.CallStack != null && runtimeEx.CallStack.Count > 0)
                {
                    // 构建完整的调用栈信息
                    callStackDetails.AppendLine("📋 调用栈信息：");
                    
                    // 🔥 调试：先打印所有调用栈帧的详细信息（用于调试）
                    System.Diagnostics.Debug.WriteLine($"调用栈帧数量: {runtimeEx.CallStack.Count}");
                    for (int debugIdx = 0; debugIdx < runtimeEx.CallStack.Count; debugIdx++)
                    {
                        var debugFrame = runtimeEx.CallStack[debugIdx];
                        var debugLoc = debugFrame.Location;
                        string debugFuncName = "未知";
                        try { if (debugFrame.Name != null) debugFuncName = debugFrame.Name; } catch { }
                        System.Diagnostics.Debug.WriteLine($"  帧[{debugIdx}]: 函数={debugFuncName}, 行={debugLoc?.FromLine ?? 0}, 列={debugLoc?.FromChar ?? 0}");
                    }
                    
                    // 🔥 关键改进：MoonSharp 的调用栈顺序可能是从外层到内层
                    // 第一个栈帧（索引0）可能是调用点，最后一个栈帧才是实际出错的位置
                    // 我们需要找到最深层（最后一个有有效位置的）栈帧作为错误位置
                    
                    int errorFrameIndex = -1;
                    int deepestLine = 0;
                    
                    // 策略1：从后往前找，最后一个有位置的栈帧通常是实际出错的位置
                    for (int i = runtimeEx.CallStack.Count - 1; i >= 0; i--)
                    {
                        var frame = runtimeEx.CallStack[i];
                        var location = frame.Location;
                        
                        if (location != null && location.FromLine > 0)
                        {
                            // 找到第一个有有效位置的栈帧（从后往前），这通常是实际出错的位置
                            if (errorFrameIndex == -1)
                            {
                                errorFrameIndex = i;
                                deepestLine = location.FromLine;
                                lineNumber = location.FromLine;
                                columnNumber = location.FromChar;
                            }
                            // 如果找到更深的栈帧（行号更大），更新错误位置
                            else if (location.FromLine > deepestLine)
                            {
                                errorFrameIndex = i;
                                deepestLine = location.FromLine;
                                lineNumber = location.FromLine;
                                columnNumber = location.FromChar;
                            }
                        }
                    }
                    
                    // 策略2：如果从后往前没找到，尝试从前往后找第一个有位置的
                    if (errorFrameIndex == -1)
                    {
                        for (int i = 0; i < runtimeEx.CallStack.Count; i++)
                        {
                            var frame = runtimeEx.CallStack[i];
                            var location = frame.Location;
                            if (location != null && location.FromLine > 0)
                            {
                                errorFrameIndex = i;
                                lineNumber = location.FromLine;
                                columnNumber = location.FromChar;
                                break;
                            }
                        }
                    }
                    
                    // 策略3：如果调用栈只有一个帧（通常是 main），且错误发生在函数调用中
                    // 这可能意味着错误发生在被调用的函数内部，但调用栈没有包含该函数的栈帧
                    if (runtimeEx.CallStack.Count == 1)
                    {
                        var singleFrame = runtimeEx.CallStack[0];
                        string singleFrameFuncName = "未知函数";
                        try
                        {
                            if (singleFrame.Name != null)
                            {
                                singleFrameFuncName = singleFrame.Name;
                            }
                        }
                        catch { }
                        
                        // 如果调用栈只有 main 函数，但错误是 "attempt to call a nil value"
                        // 这可能意味着错误发生在被调用的函数内部
                        if (singleFrameFuncName == "main" && 
                            (message.Contains("attempt to call a nil value") || 
                             message.Contains("尝试调用") || 
                             message.Contains("nil 值")))
                        {
                            // 添加提示信息，说明错误可能发生在被调用的函数中
                            callStackDetails.AppendLine();
                            callStackDetails.AppendLine("⚠️ 注意：调用栈信息不完整，错误可能发生在被调用的函数内部。");
                            
                            // 🔥 改进：动态分析错误行的代码，提取被调用的函数名
                            string? calledFunctionName = null;
                            if (!string.IsNullOrEmpty(scriptCode) && lineNumber > 0)
                            {
                                try
                                {
                                    var lines = scriptCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                                    if (lineNumber <= lines.Length)
                                    {
                                        var errorLine = lines[lineNumber - 1]; // 行号从1开始，数组从0开始
                                        
                                        // 尝试提取函数调用：匹配 "functionName(" 或 "functionName ("
                                        var functionCallMatch = System.Text.RegularExpressions.Regex.Match(errorLine, 
                                            @"(\w+)\s*\(");
                                        if (functionCallMatch.Success)
                                        {
                                            calledFunctionName = functionCallMatch.Groups[1].Value;
                                        }
                                    }
                                }
                                catch
                                {
                                    // 如果分析失败，忽略
                                }
                            }
                            
                            // 根据提取的函数名提供更准确的提示
                            if (!string.IsNullOrEmpty(calledFunctionName))
                            {
                                // 检查函数名是否在映射表中
                                string? functionFile = null;
                                if (_functionToFileMap.ContainsKey(calledFunctionName))
                                {
                                    functionFile = _functionToFileMap[calledFunctionName];
                                }
                                else if (calledFunctionName == "login" || calledFunctionName == "getData" || 
                                        calledFunctionName == "queryOrder" || calledFunctionName == "placeBet")
                                {
                                    functionFile = "functions.lua";
                                }
                                
                                if (!string.IsNullOrEmpty(functionFile))
                                {
                                    callStackDetails.AppendLine($"   💡 提示：{sourceFileName ?? "main.lua"} 第{lineNumber}行调用了 {calledFunctionName}() 函数。");
                                    callStackDetails.AppendLine($"   请检查 {functionFile} 中的 {calledFunctionName} 函数实现，错误可能发生在该函数内部。");
                                    callStackDetails.AppendLine("   常见问题：");
                                    callStackDetails.AppendLine("   - 函数内部调用了未定义的函数或变量");
                                    callStackDetails.AppendLine("   - 函数内部使用了未绑定的 web 或 config 对象");
                                    callStackDetails.AppendLine("   - 函数内部有语法错误或逻辑错误");
                                    callStackDetails.AppendLine($"   - 函数参数不匹配或传递了 nil 值");
                                }
                                else
                                {
                                    callStackDetails.AppendLine($"   💡 提示：{sourceFileName ?? "main.lua"} 第{lineNumber}行调用了 {calledFunctionName}() 函数。");
                                    callStackDetails.AppendLine($"   请检查 {calledFunctionName} 函数的实现，错误可能发生在该函数内部。");
                                }
                            }
                            else
                            {
                                callStackDetails.AppendLine($"   💡 提示：错误发生在 {sourceFileName ?? "main.lua"} 第{lineNumber}行。");
                                callStackDetails.AppendLine("   如果该行调用了其他函数，请检查该函数的实现。");
                            }
                        }
                    }
                    
                    // 第二遍遍历：构建完整的调用栈信息
                    for (int i = 0; i < runtimeEx.CallStack.Count; i++)
                    {
                        var frame = runtimeEx.CallStack[i];
                        var location = frame.Location;
                        
                        if (location != null)
                        {
                            var frameLine = location.FromLine;
                            var frameCol = location.FromChar;
                            
                            // 获取函数名（如果可用）
                            string functionName = "未知函数";
                            try
                            {
                                if (frame.Name != null)
                                {
                                    functionName = frame.Name;
                                }
                            }
                            catch { }
                            
                            // 🔥 改进：根据函数名推断文件名
                            // 优先使用函数名到文件名的映射
                            string sourceFile = sourceFileName ?? "未知文件";
                            if (functionName != "main" && functionName != "未知函数")
                            {
                                // 检查函数名映射
                                if (_functionToFileMap.ContainsKey(functionName))
                                {
                                    sourceFile = _functionToFileMap[functionName];
                                }
                                // 如果没有映射，但函数名是常见的库函数名，推断为 functions.lua
                                else if (functionName == "login" || functionName == "getData" || 
                                        functionName == "queryOrder" || functionName == "placeBet")
                                {
                                    sourceFile = "functions.lua";
                                }
                            }
                            
                            // 判断这是错误位置还是调用位置
                            if (i == errorFrameIndex)
                            {
                                // 这是实际出错的位置
                                callStackDetails.AppendLine($"❌ 错误位置: {sourceFile}:{frameLine}:{frameCol} (函数: {functionName})");
                            }
                            else
                            {
                                // 其他栈帧是调用链
                                callStackDetails.AppendLine($"   ↳ 调用位置: {sourceFile}:{frameLine}:{frameCol} (函数: {functionName})");
                            }
                        }
                        else
                        {
                            // 即使没有位置信息，也显示函数名
                            string functionName = "未知函数";
                            try
                            {
                                if (frame.Name != null)
                                {
                                    functionName = frame.Name;
                                }
                            }
                            catch { }
                            
                            string sourceFile = sourceFileName ?? "未知文件";
                            if (functionName != "main" && functionName != "未知函数")
                            {
                                // 检查函数名映射
                                if (_functionToFileMap.ContainsKey(functionName))
                                {
                                    sourceFile = _functionToFileMap[functionName];
                                }
                                else
                                {
                                    sourceFile = "functions.lua";
                                }
                            }
                            
                            if (i == errorFrameIndex)
                            {
                                callStackDetails.AppendLine($"❌ 错误位置: {sourceFile} (函数: {functionName}, 位置信息不可用)");
                            }
                            else
                            {
                                callStackDetails.AppendLine($"   ↳ 调用位置: {sourceFile} (函数: {functionName}, 位置信息不可用)");
                            }
                        }
                    }
                }
            }
            else if (ex is SyntaxErrorException syntaxEx)
            {
                message = syntaxEx.DecoratedMessage ?? syntaxEx.Message;
                fullMessage = syntaxEx.ToString();
                
                // 尝试从消息中解析行号（格式通常是 "...:line X:..."）
                var match = System.Text.RegularExpressions.Regex.Match(message, @"line\s+(\d+)", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int parsedLine))
                {
                    lineNumber = parsedLine;
                }
                
                // 尝试解析列号
                match = System.Text.RegularExpressions.Regex.Match(message, @"column\s+(\d+)", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int parsedCol))
                {
                    columnNumber = parsedCol;
                }
            }

            // 将调用栈信息添加到完整消息中
            if (callStackDetails.Length > 0)
            {
                // 🔥 改进：构建更友好的错误消息格式
                var friendlyMessage = new System.Text.StringBuilder();
                friendlyMessage.AppendLine($"❌ 错误: {message}");
                friendlyMessage.AppendLine();
                friendlyMessage.Append(callStackDetails);
                friendlyMessage.AppendLine();
                friendlyMessage.AppendLine("=== 完整堆栈跟踪 ===");
                friendlyMessage.Append(fullMessage);
                
                fullMessage = friendlyMessage.ToString();
            }
            else
            {
                // 即使没有调用栈信息，也格式化错误消息
                fullMessage = $"❌ 错误: {message}\n\n=== 完整堆栈跟踪 ===\n{fullMessage}";
            }

            return (message, lineNumber, columnNumber, fullMessage);
        }

        public ScriptValidationResult Validate(string scriptCode)
        {
            try
            {
                // 尝试解析脚本
                var tempScript = new Script();
                tempScript.DoString(scriptCode);
                
                // 🔥 验证3个必须函数是否都存在
                var mainFunc = tempScript.Globals.Get("main");
                var errorFunc = tempScript.Globals.Get("error");
                var exitFunc = tempScript.Globals.Get("exit");

                var missingFunctions = new System.Text.StringBuilder();
                
                if (mainFunc.IsNil() || mainFunc.Type != DataType.Function)
                {
                    missingFunctions.AppendLine("  - function main()");
                }
                
                if (errorFunc.IsNil() || errorFunc.Type != DataType.Function)
                {
                    missingFunctions.AppendLine("  - function error(errorInfo)");
                }
                
                if (exitFunc.IsNil() || exitFunc.Type != DataType.Function)
                {
                    missingFunctions.AppendLine("  - function exit()");
                }

                // 如果有缺失的函数，返回验证失败
                if (missingFunctions.Length > 0)
                {
                    var errorMsg = $"脚本不符合规范！必须包含以下3个函数：\n{missingFunctions}";
                    
                    return new ScriptValidationResult
                    {
                        IsValid = false,
                        Error = errorMsg,
                        LineNumber = 0,
                        ColumnNumber = 0
                    };
                }
                
                // 语法正确且3个函数都存在
                return new ScriptValidationResult { IsValid = true };
            }
            catch (SyntaxErrorException ex)
            {
                // 语法错误 - 提取详细信息
                var errorInfo = ExtractErrorInfo(ex, null);
                
                return new ScriptValidationResult
                {
                    IsValid = false,
                    Error = errorInfo.Message,
                    LineNumber = errorInfo.LineNumber,
                    ColumnNumber = errorInfo.ColumnNumber
                };
            }
            catch (Exception ex)
            {
                return new ScriptValidationResult
                {
                    IsValid = false,
                    Error = ex.Message,
                    LineNumber = 0,
                    ColumnNumber = 0
                };
            }
        }

        public void BindFunction(string name, Delegate function)
        {
            _script.Globals[name] = DynValue.FromObject(_script, function);
        }

        public void BindObject(string name, object obj)
        {
            _script.Globals[name] = DynValue.FromObject(_script, obj);
        }

        public void SetBreakpoint(int lineNumber)
        {
            _breakpoints.Add(lineNumber);
            // MoonSharp 的断点功能需要额外的调试器支持
            // 这里先记录断点，实际调试功能可以在后续版本中实现
        }

        public void ClearBreakpoint(int lineNumber)
        {
            _breakpoints.Remove(lineNumber);
        }

        #region IScriptDebugEngine 实现

        /// <summary>
        /// 步进（Step Into）- 遇到函数自动进入
        /// 注意：MoonSharp 的完整调试支持需要更复杂的实现
        /// 这里提供基础框架，实际功能可以在后续版本中完善
        /// </summary>
        public void StepInto()
        {
            if (!_isDebugging || !_isPaused)
                return;

            _isPaused = false;
            // TODO: 实现真正的步进功能（需要 MoonSharp 调试器支持）
        }

        /// <summary>
        /// 步过（Step Over）- 遇到函数就步过
        /// </summary>
        public void StepOver()
        {
            if (!_isDebugging || !_isPaused)
                return;

            _isPaused = false;
            // TODO: 实现真正的步过功能（需要 MoonSharp 调试器支持）
        }

        /// <summary>
        /// 继续执行（Continue）- 继续运行到下一个断点
        /// </summary>
        public void Continue()
        {
            if (!_isDebugging || !_isPaused)
                return;

            _isPaused = false;
            // 继续执行（由 Execute 方法中的断点检查处理）
        }

        /// <summary>
        /// 停止调试
        /// </summary>
        public void Stop()
        {
            _isDebugging = false;
            _isPaused = false;
            _currentLine = -1;
            _currentVariables = null;
            _currentCallStack = null;
        }

        /// <summary>
        /// 获取当前变量
        /// </summary>
        public Dictionary<string, object>? GetVariables()
        {
            if (!_isDebugging || !_isPaused)
                return null;

            // 尝试从脚本全局变量中提取
            var variables = new Dictionary<string, object>();
            try
            {
                foreach (var pair in _script.Globals.Pairs)
                {
                    if (pair.Key.Type == DataType.String)
                    {
                        var key = pair.Key.String;
                        var value = pair.Value.ToObject();
                        variables[key] = value ?? "nil";
                    }
                }
            }
            catch
            {
                // 忽略错误
            }

            return variables;
        }

        /// <summary>
        /// 获取调用堆栈
        /// </summary>
        public List<string>? GetCallStack()
        {
            if (!_isDebugging || !_isPaused)
                return null;

            // TODO: 实现真正的调用堆栈（需要 MoonSharp 调试器支持）
            var stack = new List<string>();
            if (_currentLine > 0)
            {
                stack.Add($"第 {_currentLine} 行");
            }
            return stack;
        }

        #endregion

        public event EventHandler<ScriptDebugEventArgs>? OnBreakpoint;
        public event EventHandler<ScriptErrorEventArgs>? OnError;
    }
}
