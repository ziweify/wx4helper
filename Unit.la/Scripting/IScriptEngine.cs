using System;
using System.Collections.Generic;

namespace Unit.La.Scripting
{
    /// <summary>
    /// 脚本执行结果
    /// </summary>
    public class ScriptResult
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Error { get; set; }
        public int LineNumber { get; set; }
        public string? Output { get; set; }
    }

    /// <summary>
    /// 脚本验证结果
    /// </summary>
    public class ScriptValidationResult
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
    }

    /// <summary>
    /// 脚本调试事件参数
    /// </summary>
    public class ScriptDebugEventArgs : EventArgs
    {
        public int LineNumber { get; set; }
        public Dictionary<string, object>? Variables { get; set; }
    }

    /// <summary>
    /// 脚本错误事件参数
    /// </summary>
    public class ScriptErrorEventArgs : EventArgs
    {
        public string Error { get; set; } = string.Empty;
        public int LineNumber { get; set; }
    }

    /// <summary>
    /// 脚本引擎接口
    /// </summary>
    public interface IScriptEngine
    {
        /// <summary>
        /// 加载脚本（不执行，只定义函数和变量）
        /// 用于加载 functions.lua 等库文件
        /// </summary>
        void LoadScript(string scriptCode);

        /// <summary>
        /// 执行脚本
        /// </summary>
        ScriptResult Execute(string script, Dictionary<string, object>? context = null);

        /// <summary>
        /// 验证脚本语法
        /// </summary>
        ScriptValidationResult Validate(string script);

        /// <summary>
        /// 绑定函数
        /// </summary>
        void BindFunction(string name, Delegate function);

        /// <summary>
        /// 绑定对象
        /// </summary>
        void BindObject(string name, object obj);

        /// <summary>
        /// 设置断点
        /// </summary>
        void SetBreakpoint(int lineNumber);

        /// <summary>
        /// 清除断点
        /// </summary>
        void ClearBreakpoint(int lineNumber);

        /// <summary>
        /// 调试事件
        /// </summary>
        event EventHandler<ScriptDebugEventArgs>? OnBreakpoint;
        event EventHandler<ScriptErrorEventArgs>? OnError;
    }
}
