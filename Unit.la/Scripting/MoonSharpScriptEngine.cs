using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Unit.La.Scripting
{
    /// <summary>
    /// MoonSharp Lua 脚本引擎实现
    /// </summary>
    public class MoonSharpScriptEngine : IScriptEngine
    {
        private readonly Script _script;
        private readonly HashSet<int> _breakpoints = new();

        public MoonSharpScriptEngine()
        {
            _script = new Script();
            
            // 允许CLR类型访问
            UserData.RegisterAssembly();
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

                // 执行脚本
                var result = _script.DoString(scriptCode);

                return new ScriptResult
                {
                    Success = true,
                    Data = result.ToObject(),
                    Output = result.ToString()
                };
            }
            catch (ScriptRuntimeException ex)
            {
                // MoonSharp 2.0 中的行号获取
                int lineNumber = 0;
                // 简化实现：直接使用异常信息，不依赖可能不存在的 API
                
                OnError?.Invoke(this, new ScriptErrorEventArgs
                {
                    Error = ex.DecoratedMessage ?? ex.Message,
                    LineNumber = lineNumber
                });

                return new ScriptResult
                {
                    Success = false,
                    Error = ex.DecoratedMessage ?? ex.Message,
                    LineNumber = lineNumber
                };
            }
            catch (SyntaxErrorException ex)
            {
                // MoonSharp 2.0 中的行号获取
                int lineNumber = 0;
                // 简化实现：从异常消息中解析行号或使用0
                
                OnError?.Invoke(this, new ScriptErrorEventArgs
                {
                    Error = ex.DecoratedMessage ?? ex.Message,
                    LineNumber = lineNumber
                });

                return new ScriptResult
                {
                    Success = false,
                    Error = ex.DecoratedMessage ?? ex.Message,
                    LineNumber = lineNumber
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
                    Error = ex.Message
                };
            }
        }

        public ScriptValidationResult Validate(string scriptCode)
        {
            try
            {
                // 尝试解析脚本
                _script.LoadString(scriptCode);
                return new ScriptValidationResult { IsValid = true };
            }
            catch (SyntaxErrorException ex)
            {
                // MoonSharp 2.0 中的行号获取
                int lineNumber = 0;
                int columnNumber = 0;
                // 简化实现：使用异常的装饰消息
                
                return new ScriptValidationResult
                {
                    IsValid = false,
                    Error = ex.DecoratedMessage ?? ex.Message,
                    LineNumber = lineNumber,
                    ColumnNumber = columnNumber
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

        public event EventHandler<ScriptDebugEventArgs>? OnBreakpoint;
        public event EventHandler<ScriptErrorEventArgs>? OnError;
    }
}
