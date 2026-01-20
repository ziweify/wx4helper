using System;

namespace 永利系统.Models
{
    /// <summary>
    /// 日志条目
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 模块名称（如：微信助手、开奖管理、方案管理等）
        /// </summary>
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// 日志消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 异常信息（如果有）
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// 格式化日志输出
        /// </summary>
        public override string ToString()
        {
            var timestamp = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var level = Level.ToString().ToUpper();
            var module = string.IsNullOrEmpty(Module) ? "系统" : Module;
            
            var logText = $"{timestamp} [{module}] [{level}] {Message}";
            
            if (Exception != null)
            {
                logText += $"\n异常: {Exception.Message}\n堆栈: {Exception.StackTrace}";
            }
            
            return logText;
        }
    }
}

