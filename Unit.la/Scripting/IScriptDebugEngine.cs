using System;
using System.Collections.Generic;

namespace Unit.La.Scripting
{
    /// <summary>
    /// 脚本调试引擎接口
    /// 扩展 IScriptEngine，提供调试功能
    /// </summary>
    public interface IScriptDebugEngine : IScriptEngine
    {
        /// <summary>
        /// 步进（Step Into）- 遇到函数自动进入
        /// </summary>
        void StepInto();

        /// <summary>
        /// 步过（Step Over）- 遇到函数就步过
        /// </summary>
        void StepOver();

        /// <summary>
        /// 继续执行（Continue）- 继续运行到下一个断点
        /// </summary>
        void Continue();

        /// <summary>
        /// 停止调试
        /// </summary>
        void Stop();

        /// <summary>
        /// 获取当前变量
        /// </summary>
        Dictionary<string, object>? GetVariables();

        /// <summary>
        /// 获取调用堆栈
        /// </summary>
        List<string>? GetCallStack();
    }
}
