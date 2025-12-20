using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using 永利系统.Models;

namespace 永利系统.Services
{
    /// <summary>
    /// 日志服务 - 单例模式
    /// </summary>
    public class LoggingService
    {
        private static LoggingService? _instance;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// 单例实例
        /// </summary>
        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LoggingService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 日志接收事件
        /// </summary>
        public event EventHandler<LogEventArgs>? LogReceived;

        /// <summary>
        /// 日志存储目录
        /// </summary>
        private readonly string _logDirectory;

        /// <summary>
        /// 当前日志文件路径
        /// </summary>
        private string CurrentLogFile => Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");

        /// <summary>
        /// 最小日志级别（低于此级别的日志将被忽略）
        /// </summary>
        public LogLevel MinLogLevel { get; set; } = LogLevel.Debug;

        private LoggingService()
        {
            // 设置日志目录
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "永利系统",
                "Logs"
            );
            
            _logDirectory = appDataPath;
            
            // 确保日志目录存在
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="module">模块名称</param>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        public void Log(string module, LogLevel level, string message, Exception? exception = null)
        {
            // 检查日志级别
            if (level < MinLogLevel)
            {
                return;
            }

            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Module = module,
                Level = level,
                Message = message,
                Exception = exception
            };

            // 触发事件
            LogReceived?.Invoke(this, new LogEventArgs(logEntry));

            // 异步保存到文件（不阻塞UI）
            Task.Run(() => SaveToFile(logEntry));
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        public void Debug(string module, string message)
        {
            Log(module, LogLevel.Debug, message);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        public void Info(string module, string message)
        {
            Log(module, LogLevel.Info, message);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public void Warn(string module, string message, Exception? exception = null)
        {
            Log(module, LogLevel.Warn, message, exception);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public void Error(string module, string message, Exception? exception = null)
        {
            Log(module, LogLevel.Error, message, exception);
        }

        /// <summary>
        /// 保存日志到文件
        /// </summary>
        private void SaveToFile(LogEntry entry)
        {
            try
            {
                var logLine = entry.ToString() + Environment.NewLine;
                File.AppendAllText(CurrentLogFile, logLine);
            }
            catch
            {
                // 忽略文件写入错误，避免影响主程序
            }
        }

        /// <summary>
        /// 加载历史日志
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>日志条目列表</returns>
        public List<LogEntry> LoadHistory(DateTime date)
        {
            var logFile = Path.Combine(_logDirectory, $"log_{date:yyyyMMdd}.txt");
            
            if (!File.Exists(logFile))
            {
                return new List<LogEntry>();
            }

            var entries = new List<LogEntry>();
            
            try
            {
                var lines = File.ReadAllLines(logFile);
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // 解析日志行（简单实现，可以根据实际格式调整）
                    var entry = ParseLogLine(line);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }
            catch
            {
                // 忽略读取错误
            }

            return entries;
        }

        /// <summary>
        /// 解析日志行（简单实现）
        /// </summary>
        private LogEntry? ParseLogLine(string line)
        {
            try
            {
                // 格式: yyyy-MM-dd HH:mm:ss.fff [模块] [级别] 消息
                // 这里使用简单的字符串解析，实际可以使用正则表达式
                var parts = line.Split(new[] { " [" }, StringSplitOptions.None);
                if (parts.Length < 3)
                    return null;

                var timestampStr = parts[0];
                var modulePart = parts[1].Split(']')[0];
                var levelPart = parts[2].Split(']')[0];
                var message = parts.Length > 3 ? string.Join(" [", parts.Skip(3)) : "";

                if (DateTime.TryParse(timestampStr, out var timestamp) &&
                    Enum.TryParse<LogLevel>(levelPart, true, out var level))
                {
                    return new LogEntry
                    {
                        Timestamp = timestamp,
                        Module = modulePart,
                        Level = level,
                        Message = message.Trim()
                    };
                }
            }
            catch
            {
                // 解析失败，返回null
            }

            return null;
        }

        /// <summary>
        /// 清空当前日志
        /// </summary>
        public void ClearCurrentLog()
        {
            try
            {
                if (File.Exists(CurrentLogFile))
                {
                    File.Delete(CurrentLogFile);
                }
            }
            catch
            {
                // 忽略删除错误
            }
        }
    }

    /// <summary>
    /// 日志事件参数
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        public LogEntry LogEntry { get; }

        public LogEventArgs(LogEntry logEntry)
        {
            LogEntry = logEntry;
        }
    }
}

