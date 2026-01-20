using System;
using System.Collections.Generic;
using System.Linq;

namespace Unit.La.Scripting
{
    /// <summary>
    /// 脚本功能信息
    /// </summary>
    public class ScriptFunctionInfo
    {
        public string Name { get; set; } = string.Empty;
        public Delegate Function { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;
    }

    /// <summary>
    /// 脚本功能注册表
    /// 管理所有可绑定到脚本的功能
    /// </summary>
    public class ScriptFunctionRegistry
    {
        private static ScriptFunctionRegistry? _instance;
        private readonly Dictionary<string, ScriptFunctionInfo> _functions = new();

        /// <summary>
        /// 单例实例
        /// </summary>
        public static ScriptFunctionRegistry Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScriptFunctionRegistry();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 注册功能
        /// </summary>
        public void RegisterFunction(string name, Delegate function, string description, string example)
        {
            _functions[name] = new ScriptFunctionInfo
            {
                Name = name,
                Function = function,
                Description = description,
                Example = example
            };
        }

        /// <summary>
        /// 绑定所有功能到脚本引擎
        /// </summary>
        public void BindToEngine(IScriptEngine engine)
        {
            foreach (var func in _functions.Values)
            {
                engine.BindFunction(func.Name, func.Function);
            }
        }

        /// <summary>
        /// 获取所有功能信息（用于自动完成和帮助）
        /// </summary>
        public IEnumerable<ScriptFunctionInfo> GetAllFunctions()
        {
            return _functions.Values;
        }

        /// <summary>
        /// 获取功能名称列表（用于自动完成）
        /// </summary>
        public string GetAutoCompleteKeywords()
        {
            return string.Join(" ", _functions.Keys);
        }
    }
}
