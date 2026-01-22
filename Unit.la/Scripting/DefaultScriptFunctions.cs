using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
        /// 输出日志
        /// </summary>
        public static void Log(string message)
        {
            var msg = $"[LOG] {message}";
            Console.WriteLine(msg);
            _logCallback?.Invoke(msg);
        }

        public static void LogInfo(string message)
        {
            var msg = $"[INFO] {message}";
            Console.WriteLine(msg);
            _logCallback?.Invoke(msg);
        }

        public static void LogWarn(string message)
        {
            var msg = $"[WARN] {message}";
            Console.WriteLine(msg);
            _logCallback?.Invoke(msg);
        }

        public static void LogError(string message)
        {
            var msg = $"[ERROR] {message}";
            Console.WriteLine(msg);
            _logCallback?.Invoke(msg);
        }

        #endregion

        #region 延迟函数

        /// <summary>
        /// 延迟（毫秒）
        /// </summary>
        public static void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
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
