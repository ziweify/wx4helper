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
        public string Category { get; set; } = "其他"; // 功能分类
    }

    /// <summary>
    /// 脚本功能注册表
    /// 管理所有可绑定到脚本的功能
    /// 支持默认函数和自定义函数
    /// </summary>
    public class ScriptFunctionRegistry
    {
        private readonly Dictionary<string, ScriptFunctionInfo> _functions = new();
        private readonly Dictionary<string, object> _objects = new();

        /// <summary>
        /// 注册功能
        /// </summary>
        public void RegisterFunction(string name, Delegate function, string description = "", string example = "", string category = "其他")
        {
            _functions[name] = new ScriptFunctionInfo
            {
                Name = name,
                Function = function,
                Description = description,
                Example = example,
                Category = category
            };
        }

        /// <summary>
        /// 注册对象
        /// </summary>
        public void RegisterObject(string name, object obj)
        {
            _objects[name] = obj;
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

            foreach (var kvp in _objects)
            {
                engine.BindObject(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 注册默认函数库
        /// </summary>
        /// <param name="logCallback">日志回调</param>
        /// <param name="webViewProvider">WebView2 提供者函数（动态引用，确保重新创建时仍然有效）</param>
        /// <param name="cancellationToken">取消令牌（用于停止脚本）</param>
        public void RegisterDefaults(Action<string>? logCallback = null, 
            Func<Microsoft.Web.WebView2.WinForms.WebView2?>? webViewProvider = null,
            System.Threading.CancellationToken? cancellationToken = null)
        {
            // 🔧 设置日志回调到 DefaultScriptFunctions
            if (logCallback != null)
            {
                DefaultScriptFunctions.SetLogCallback(logCallback);
            }
            
            // 🔧 设置取消令牌到 DefaultScriptFunctions
            if (cancellationToken != null)
            {
                DefaultScriptFunctions.SetCancellationToken(cancellationToken.Value);
            }
            
            // 日志函数
            RegisterFunction("log", new Action<string>(DefaultScriptFunctions.Log), 
                "输出日志", "log('Hello World')", "日志");
            RegisterFunction("log_info", new Action<string>(DefaultScriptFunctions.LogInfo), 
                "输出信息日志", "log_info('信息')", "日志");
            RegisterFunction("log_warn", new Action<string>(DefaultScriptFunctions.LogWarn), 
                "输出警告日志", "log_warn('警告')", "日志");
            RegisterFunction("log_error", new Action<string>(DefaultScriptFunctions.LogError), 
                "输出错误日志", "log_error('错误')", "日志");

            // 延迟函数
            RegisterFunction("sleep", new Action<int>(DefaultScriptFunctions.Sleep), 
                "延迟指定毫秒数", "sleep(1000)", "工具");
            RegisterFunction("wait", new Action<int>(DefaultScriptFunctions.Sleep), 
                "延迟指定毫秒数（sleep别名）", "wait(1000)", "工具");

            // 脚本控制函数
            RegisterFunction("is_stopped", new Func<bool>(DefaultScriptFunctions.IsStopped), 
                "检查脚本是否已停止", "if is_stopped() then return false end", "控制");

            // HTTP 函数
            RegisterFunction("http_get", new Func<string, string>(DefaultScriptFunctions.HttpGet), 
                "发送 HTTP GET 请求", "local response = http_get('https://api.example.com')", "网络");
            RegisterFunction("http_post", new Func<string, string, string>(DefaultScriptFunctions.HttpPost), 
                "发送 HTTP POST 请求", "local response = http_post('https://api.example.com', '{}')", "网络");

            // 时间函数
            RegisterFunction("get_timestamp", new Func<long>(DefaultScriptFunctions.GetTimestamp), 
                "获取当前时间戳（毫秒）", "local ts = get_timestamp()", "时间");
            RegisterFunction("get_datetime", new Func<string>(DefaultScriptFunctions.GetDateTime), 
                "获取当前日期时间字符串", "local dt = get_datetime()", "时间");
            RegisterFunction("format_time", new Func<string, string>(DefaultScriptFunctions.FormatTime), 
                "格式化当前时间", "local t = format_time('yyyy-MM-dd')", "时间");

            // 随机数函数
            RegisterFunction("random_number", new Func<int, int, int>(DefaultScriptFunctions.RandomNumber), 
                "生成随机整数", "local n = random_number(1, 100)", "随机");
            RegisterFunction("random_string", new Func<int, string>(DefaultScriptFunctions.RandomString), 
                "生成随机字符串", "local s = random_string(10)", "随机");

            // JSON 函数
            RegisterFunction("parse_json", new Func<string, object>(DefaultScriptFunctions.ParseJson), 
                "解析 JSON 字符串", "local obj = parse_json(jsonStr)", "JSON");
            RegisterFunction("to_json", new Func<object, string>(DefaultScriptFunctions.ToJson), 
                "对象转 JSON 字符串", "local json = to_json(obj)", "JSON");

            // 字符串函数
            RegisterFunction("string_contains", new Func<string, string, bool>(DefaultScriptFunctions.StringContains), 
                "判断字符串是否包含子串", "local b = string_contains('hello', 'ell')", "字符串");
            RegisterFunction("string_replace", new Func<string, string, string, string>(DefaultScriptFunctions.StringReplace), 
                "替换字符串", "local s = string_replace('hello', 'l', 'L')", "字符串");
            RegisterFunction("string_split", new Func<string, string, string[]>(DefaultScriptFunctions.StringSplit), 
                "分割字符串", "local arr = string_split('a,b,c', ',')", "字符串");
            
            // 🌐 注册 WebView2 桥接对象（使用动态引用）
            if (webViewProvider != null)
            {
                var webBridge = new WebBridge(webViewProvider, logCallback);
                
                // 传递取消令牌到 WebBridge
                if (cancellationToken != null)
                {
                    webBridge.SetCancellationToken(cancellationToken.Value);
                }
                
                RegisterObject("web", webBridge);
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
        /// 按分类获取功能
        /// </summary>
        public Dictionary<string, List<ScriptFunctionInfo>> GetFunctionsByCategory()
        {
            return _functions.Values
                .GroupBy(f => f.Category)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// 获取功能名称列表（用于自动完成）
        /// </summary>
        public string GetAutoCompleteKeywords()
        {
            return string.Join(" ", _functions.Keys);
        }

        /// <summary>
        /// 清空所有注册
        /// </summary>
        public void Clear()
        {
            _functions.Clear();
            _objects.Clear();
        }

        /// <summary>
        /// 生成帮助文档
        /// </summary>
        public string GenerateHelpText()
        {
            var help = new System.Text.StringBuilder();
            help.AppendLine("=== Lua 脚本函数帮助 ===\n");

            var categories = GetFunctionsByCategory();
            foreach (var category in categories.OrderBy(c => c.Key))
            {
                help.AppendLine($"【{category.Key}】");
                foreach (var func in category.Value.OrderBy(f => f.Name))
                {
                    help.AppendLine($"  {func.Name}");
                    if (!string.IsNullOrEmpty(func.Description))
                        help.AppendLine($"    说明: {func.Description}");
                    if (!string.IsNullOrEmpty(func.Example))
                        help.AppendLine($"    示例: {func.Example}");
                    help.AppendLine();
                }
            }

            return help.ToString();
        }
    }
}
