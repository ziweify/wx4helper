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
        
        /// <summary>
        /// 内存中的日志历史（用于在日志窗口打开时显示）
        /// </summary>
        private readonly List<LogEntry> _logHistory = new List<LogEntry>();
        private readonly object _historyLock = new object();
        private const int MAX_HISTORY_COUNT = 1000; // 最多保留1000条内存日志

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

            System.Diagnostics.Debug.WriteLine($"LoggingService.Log: [{module}] {message}");

            // 添加到内存历史
            lock (_historyLock)
            {
                _logHistory.Add(logEntry);
                System.Diagnostics.Debug.WriteLine($"LoggingService.Log: 内存历史数量 = {_logHistory.Count}");
                
                // 限制内存日志数量
                if (_logHistory.Count > MAX_HISTORY_COUNT)
                {
                    _logHistory.RemoveAt(0);
                }
            }

            // 触发事件
            System.Diagnostics.Debug.WriteLine($"LoggingService.Log: 触发 LogReceived 事件，订阅者数量 = {LogReceived?.GetInvocationList().Length ?? 0}");
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
        /// 获取内存中的日志历史
        /// </summary>
        /// <returns>日志条目列表</returns>
        public List<LogEntry> GetMemoryHistory()
        {
            lock (_historyLock)
            {
                System.Diagnostics.Debug.WriteLine($"LoggingService.GetMemoryHistory: 返回 {_logHistory.Count} 条日志");
                
                // 输出前3条日志内容
                for (int i = 0; i < Math.Min(3, _logHistory.Count); i++)
                {
                    System.Diagnostics.Debug.WriteLine($"  内存历史[{i}]: {_logHistory[i].Timestamp:HH:mm:ss.fff} [{_logHistory[i].Module}] {_logHistory[i].Message}");
                }
                
                return new List<LogEntry>(_logHistory);
            }
        }
        
        /// <summary>
        /// 加载历史日志（从文件）
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>日志条目列表</returns>
        public List<LogEntry> LoadHistory(DateTime date)
        {
            var logFile = Path.Combine(_logDirectory, $"log_{date:yyyyMMdd}.txt");
            System.Diagnostics.Debug.WriteLine($"LoadHistory: 尝试加载文件: {logFile}");
            System.Diagnostics.Debug.WriteLine($"LoadHistory: _logDirectory = {_logDirectory}");
            System.Diagnostics.Debug.WriteLine($"LoadHistory: File.Exists = {File.Exists(logFile)}");
            
            if (!File.Exists(logFile))
            {
                System.Diagnostics.Debug.WriteLine($"LoadHistory: 文件不存在，返回空列表");
                return new List<LogEntry>();
            }

            var entries = new List<LogEntry>();
            
            try
            {
                var lines = File.ReadAllLines(logFile);
                System.Diagnostics.Debug.WriteLine($"LoadHistory: 从文件读取了 {lines.Length} 行");
                
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
                
                System.Diagnostics.Debug.WriteLine($"LoadHistory: 成功解析 {entries.Count} 条日志");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadHistory: 读取失败: {ex.Message}");
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
                // 使用正则表达式或更健壮的解析方式
                line = line.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    return null;

                // 查找第一个 [ 的位置（时间戳后）
                var firstBracket = line.IndexOf('[');
                if (firstBracket < 0)
                    return null;

                // 提取时间戳
                var timestampStr = line.Substring(0, firstBracket).Trim();
                if (!DateTime.TryParse(timestampStr, out var timestamp))
                    return null;

                // 提取模块和级别
                var remaining = line.Substring(firstBracket);
                // 格式: [模块] [级别] 消息
                var parts = remaining.Split(new[] { "] [" }, StringSplitOptions.None);
                if (parts.Length < 2)
                    return null;

                // 提取模块（去掉开头的 [）
                var modulePart = parts[0].TrimStart('[').Trim();
                
                // 提取级别和消息
                var levelAndMessage = parts[1];
                var levelEnd = levelAndMessage.IndexOf(']');
                if (levelEnd < 0)
                    return null;

                var levelPart = levelAndMessage.Substring(0, levelEnd).Trim();
                var message = levelAndMessage.Substring(levelEnd + 1).Trim();

                if (Enum.TryParse<LogLevel>(levelPart, true, out var level))
                {
                    return new LogEntry
                    {
                        Timestamp = timestamp,
                        Module = modulePart,
                        Level = level,
                        Message = message
                    };
                }
            }
            catch (Exception ex)
            {
                // 解析失败，记录错误但不影响其他日志
                System.Diagnostics.Debug.WriteLine($"解析日志行失败: {line}, 错误: {ex.Message}");
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

