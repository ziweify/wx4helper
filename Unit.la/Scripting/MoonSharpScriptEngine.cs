using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Unit.La.Scripting
{
    /// <summary>
    /// MoonSharp Lua 脚本引擎实现
    /// </summary>
    public class MoonSharpScriptEngine : IScriptDebugEngine
    {
        private readonly Script _script;
        private readonly HashSet<int> _breakpoints = new();
        private bool _isDebugging = false;
        private bool _isPaused = false;
        private int _currentLine = -1;
        private Dictionary<string, object>? _currentVariables = null;
        private List<string>? _currentCallStack = null;

        public MoonSharpScriptEngine()
        {
            _script = new Script();
            
            // 🔥 注册自定义类型，让 MoonSharp 能够识别
            // WebBridge 用于 Lua 中的 web 对象
            UserData.RegisterType<WebBridge>();
            
            // .NET 8 不支持 Assembly.GetCallingAssembly()，所以不调用 RegisterAssembly
            // 其他类型将按需自动注册
        }

        public ScriptResult Execute(string scriptCode, Dictionary<string, object>? context = null)
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

                // 🔥 使用新的生命周期执行方式
                return ExecuteWithLifecycle(scriptCode);
            }
            catch (ScriptRuntimeException ex)
            {
                // 运行时错误 - 提取详细信息
                var errorInfo = ExtractErrorInfo(ex);
                
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
                var errorInfo = ExtractErrorInfo(ex);
                
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
        private ScriptResult ExecuteWithLifecycle(string scriptCode)
        {
            bool hasError = false;
            string? errorMessage = null;
            int errorLineNumber = 0;
            string? errorTrace = null;
            object? result = null;
            DynValue? exitFunc = null;  // 在外层声明，供 finally 使用

            try
            {
                // 1. 先加载脚本，定义所有函数
                _script.DoString(scriptCode);

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
                    var mainResult = _script.Call(mainFunc);
                    result = mainResult.ToObject();
                }
                catch (ScriptRuntimeException ex)
                {
                    // main() 执行时发生异常
                    hasError = true;
                    var errorInfo = ExtractErrorInfo(ex);
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
        /// </summary>
        private (string Message, int LineNumber, int ColumnNumber, string FullMessage) ExtractErrorInfo(Exception ex)
        {
            string fullMessage = ex.ToString();
            string message = ex.Message;
            int lineNumber = 0;
            int columnNumber = 0;

            // MoonSharp 异常通常包含 DecoratedMessage
            if (ex is ScriptRuntimeException runtimeEx)
            {
                message = runtimeEx.DecoratedMessage ?? runtimeEx.Message;
                fullMessage = runtimeEx.ToString();
                
                // 尝试从堆栈中获取行号
                if (runtimeEx.CallStack != null && runtimeEx.CallStack.Count > 0)
                {
                    var frame = runtimeEx.CallStack[0];
                    lineNumber = frame.Location?.FromLine ?? 0;
                    columnNumber = frame.Location?.FromChar ?? 0;
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
                var errorInfo = ExtractErrorInfo(ex);
                
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
