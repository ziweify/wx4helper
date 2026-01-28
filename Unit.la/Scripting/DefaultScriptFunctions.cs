using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MoonSharp.Interpreter;
using Unit.La.Scripting;

namespace Unit.La.Scripting
{
    /// <summary>
    /// 默认的 Lua 脚本函数库
    /// 提供常用的工具函数，可在任何项目中使用
    /// </summary>
    public static class DefaultScriptFunctions
    {
        private static Action<string>? _logCallback;
        private static readonly HttpClient _httpClient = new HttpClient();
        private static CancellationToken? _cancellationToken;

        /// <summary>
        /// 设置日志回调函数
        /// </summary>
        public static void SetLogCallback(Action<string> logCallback)
        {
            _logCallback = logCallback;
        }

        /// <summary>
        /// 设置取消令牌（用于检查脚本是否停止）
        /// </summary>
        public static void SetCancellationToken(CancellationToken token)
        {
            _cancellationToken = token;
        }

        /// <summary>
        /// 注册所有默认函数到脚本引擎
        /// </summary>
        public static void RegisterAll(IScriptEngine engine, Action<string>? logCallback = null)
        {
            _logCallback = logCallback;

            // 日志函数
            engine.BindFunction("log", new Action<string>(Log));
            engine.BindFunction("log_info", new Action<string>(LogInfo));
            engine.BindFunction("log_warn", new Action<string>(LogWarn));
            engine.BindFunction("log_error", new Action<string>(LogError));

            // 延迟函数
            engine.BindFunction("sleep", new Action<int>(Sleep));
            engine.BindFunction("wait", new Action<int>(Sleep)); // 别名

            // 🔥 UI 友好的循环函数（自动包含 10ms 延时）
            engine.BindFunction("loop", new Func<MoonSharp.Interpreter.DynValue, MoonSharp.Interpreter.DynValue, bool>(Loop));

            // 脚本控制函数
            engine.BindFunction("is_stopped", new Func<bool>(IsStopped));

            // HTTP 请求函数
            engine.BindFunction("http_get", new Func<string, string>(HttpGet));
            engine.BindFunction("http_post", new Func<string, string, string>(HttpPost));

            // 时间函数
            engine.BindFunction("get_timestamp", new Func<long>(GetTimestamp));
            engine.BindFunction("get_datetime", new Func<string>(GetDateTime));
            engine.BindFunction("format_time", new Func<string, string>(FormatTime));

            // 随机数函数
            engine.BindFunction("random_number", new Func<int, int, int>(RandomNumber));
            engine.BindFunction("random_string", new Func<int, string>(RandomString));

            // JSON 函数
            engine.BindFunction("parse_json", new Func<string, object>(ParseJson));
            engine.BindFunction("to_json", new Func<object, string>(ToJson));

            // 字符串函数
            engine.BindFunction("string_contains", new Func<string, string, bool>(StringContains));
            engine.BindFunction("string_replace", new Func<string, string, string, string>(StringReplace));
            engine.BindFunction("string_split", new Func<string, string, string[]>(StringSplit));
        }

        #region 日志函数

        /// <summary>
        /// 输出日志（脚本中的 log() 函数）
        /// </summary>
        public static void Log(string message)
        {
            // 🔥 添加 [SCRIPT] 标记，用于识别脚本日志
            var msg = $"[SCRIPT] {message}";
            Console.WriteLine(msg);
            _logCallback?.Invoke(msg); // 🔥 传递带标记的消息
        }

        public static void LogInfo(string message)
        {
            // 🔥 添加 [SCRIPT] 标记，用于识别脚本日志
            var msg = $"[SCRIPT] {message}";
            Console.WriteLine(msg);
            _logCallback?.Invoke(msg); // 🔥 传递带标记的消息
        }

        public static void LogWarn(string message)
        {
            var msg = $"[WARN] {message}";
            Console.WriteLine(msg);
            _logCallback?.Invoke($"[WARN] {message}"); // 🔥 保留前缀，用于识别警告类型
        }

        public static void LogError(string message)
        {
            var msg = $"[ERROR] {message}";
            Console.WriteLine(msg);
            _logCallback?.Invoke($"[ERROR] {message}"); // 🔥 保留前缀，用于识别错误类型
        }

        #endregion

        #region 延迟函数

        /// <summary>
        /// 延迟（毫秒）- UI 友好版本
        /// 🔥 使用 DoEvents 保持界面响应，避免卡死
        /// 用法: sleep(1000) -- 等待1秒，界面不卡
        /// </summary>
        public static void Sleep(int milliseconds)
        {
            if (milliseconds <= 0) return;

            var startTime = DateTime.Now;
            var targetTime = startTime.AddMilliseconds(milliseconds);

            // 🔥 使用 DoEvents 循环，保持 UI 响应
            while (DateTime.Now < targetTime)
            {
                // 检查是否已停止
                if (_cancellationToken?.IsCancellationRequested == true)
                {
                    return; // 提前退出
                }

                // 🔥 处理 UI 消息，保持界面响应
                Application.DoEvents();

                // 短暂休眠，避免 CPU 100%
                var remaining = (targetTime - DateTime.Now).TotalMilliseconds;
                if (remaining > 0)
                {
                    Thread.Sleep(Math.Min(50, (int)remaining)); // 每次最多休眠 50ms
                }
            }
        }

        #endregion

        #region 循环函数

        /// <summary>
        /// UI 友好的循环函数 - 自动包含 10ms 延时
        /// 🔥 替代 while 循环，自动保持界面响应
        /// 用法: 
        ///   loop(function() return true end, function() 
        ///       -- 循环体
        ///   end)
        /// </summary>
        public static bool Loop(MoonSharp.Interpreter.DynValue conditionFunc, MoonSharp.Interpreter.DynValue bodyFunc)
        {
            if (conditionFunc == null || conditionFunc.Type != MoonSharp.Interpreter.DataType.Function)
            {
                throw new ArgumentException("loop 的第一个参数必须是函数（条件函数）");
            }

            if (bodyFunc == null || bodyFunc.Type != MoonSharp.Interpreter.DataType.Function)
            {
                throw new ArgumentException("loop 的第二个参数必须是函数（循环体）");
            }

            // 获取脚本引擎（从 conditionFunc 中获取）
            var script = conditionFunc.Function.OwnerScript;
            if (script == null)
            {
                throw new InvalidOperationException("无法获取脚本引擎实例");
            }

            // 🔥 循环执行，自动包含 10ms 延时
            while (true)
            {
                // 检查是否已停止
                if (_cancellationToken?.IsCancellationRequested == true)
                {
                    return false; // 提前退出
                }

                // 执行条件函数（使用 Script.Call 方法，传递空参数数组）
                MoonSharp.Interpreter.DynValue conditionResult;
                try
                {
                    conditionResult = script.Call(conditionFunc);
                }
                catch (Exception ex)
                {
                    LogError($"条件函数执行错误: {ex.Message}");
                    break; // 发生错误，退出循环
                }

                if (conditionResult.Type == MoonSharp.Interpreter.DataType.Boolean && !conditionResult.Boolean)
                {
                    break; // 条件为 false，退出循环
                }

                // 执行循环体（使用 Script.Call 方法）
                try
                {
                    script.Call(bodyFunc);
                }
                catch (Exception ex)
                {
                    LogError($"循环体执行错误: {ex.Message}");
                    break; // 发生错误，退出循环
                }

                // 🔥 自动延时 10ms，保持界面响应
                Application.DoEvents();
                Thread.Sleep(10);
            }

            return true;
        }

        #endregion

        #region 脚本控制函数

        /// <summary>
        /// 检查脚本是否已停止
        /// 用法: if is_stopped() then return false end
        /// </summary>
        public static bool IsStopped()
        {
            return _cancellationToken?.IsCancellationRequested == true;
        }

        #endregion

        #region HTTP 函数

        /// <summary>
        /// HTTP GET 请求
        /// </summary>
        public static string HttpGet(string url)
        {
            try
            {
                var response = _httpClient.GetStringAsync(url).Result;
                return response;
            }
            catch (Exception ex)
            {
                LogError($"HTTP GET 失败: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// HTTP POST 请求
        /// </summary>
        public static string HttpPost(string url, string data)
        {
            try
            {
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = _httpClient.PostAsync(url, content).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                LogError($"HTTP POST 失败: {ex.Message}");
                return "";
            }
        }

        #endregion

        #region 时间函数

        /// <summary>
        /// 获取当前时间戳（毫秒）
        /// </summary>
        public static long GetTimestamp()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 获取当前日期时间字符串
        /// </summary>
        public static string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        public static string FormatTime(string format)
        {
            return DateTime.Now.ToString(format);
        }

        #endregion

        #region 随机数函数

        private static readonly Random _random = new Random();

        /// <summary>
        /// 生成随机整数（包含min和max）
        /// </summary>
        public static int RandomNumber(int min, int max)
        {
            return _random.Next(min, max + 1);
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[_random.Next(chars.Length)]);
            }
            return result.ToString();
        }

        #endregion

        #region JSON 函数

        /// <summary>
        /// 解析 JSON 字符串
        /// </summary>
        public static object ParseJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<object>(json) ?? new object();
            }
            catch (Exception ex)
            {
                LogError($"JSON 解析失败: {ex.Message}");
                return new object();
            }
        }

        /// <summary>
        /// 对象转 JSON 字符串
        /// </summary>
        public static string ToJson(object obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj);
            }
            catch (Exception ex)
            {
                LogError($"JSON 序列化失败: {ex.Message}");
                return "{}";
            }
        }

        #endregion

        #region 字符串函数

        /// <summary>
        /// 判断字符串是否包含子串
        /// </summary>
        public static bool StringContains(string str, string substring)
        {
            return str.Contains(substring);
        }

        /// <summary>
        /// 替换字符串
        /// </summary>
        public static string StringReplace(string str, string oldValue, string newValue)
        {
            return str.Replace(oldValue, newValue);
        }

        /// <summary>
        /// 分割字符串
        /// </summary>
        public static string[] StringSplit(string str, string separator)
        {
            return str.Split(new[] { separator }, StringSplitOptions.None);
        }

        #endregion
    }
}
