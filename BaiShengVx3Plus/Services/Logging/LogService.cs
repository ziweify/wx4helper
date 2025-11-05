using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
using System.Collections.Concurrent;
using SQLite;

namespace BaiShengVx3Plus.Services.Logging
{
    /// <summary>
    /// 日志服务实现（ORM 精简版）
    /// </summary>
    public class LogService : ILogService, IDisposable
    {
        private readonly ConcurrentQueue<LogEntry> _memoryLogs;
        private readonly ConcurrentQueue<LogEntry> _pendingLogs;
        private readonly Thread _consumerThread;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly string _dbPath;
        private LogLevel _minimumLevel = LogLevel.Trace;
        private const int MaxMemoryLogs = 1000;
        private const int BatchSize = 100;
        private const int FlushIntervalMs = 1000;

        public event EventHandler<LogEntry>? LogAdded;

        public LogService()
        {
            _memoryLogs = new ConcurrentQueue<LogEntry>();
            _pendingLogs = new ConcurrentQueue<LogEntry>();
            _cancellationTokenSource = new CancellationTokenSource();

            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, "logs.db");

            InitializeDatabase();

            _consumerThread = new Thread(ConsumeLogsAsync)
            {
                IsBackground = true,
                Name = "LogConsumerThread"
            };
            _consumerThread.Start();

            Info("LogService", "日志服务已启动");
        }

        public void Trace(string source, string message, string? extraData = null) => 
            Log(new LogEntry { Level = LogLevel.Trace, Source = source, Message = message, ExtraData = extraData });

        public void Debug(string source, string message, string? extraData = null) => 
            Log(new LogEntry { Level = LogLevel.Debug, Source = source, Message = message, ExtraData = extraData });

        public void Info(string source, string message, string? extraData = null) => 
            Log(new LogEntry { Level = LogLevel.Info, Source = source, Message = message, ExtraData = extraData });

        public void Warning(string source, string message, string? extraData = null) => 
            Log(new LogEntry { Level = LogLevel.Warning, Source = source, Message = message, ExtraData = extraData });

        public void Error(string source, string message, Exception? exception = null, string? extraData = null) => 
            Log(new LogEntry { Level = LogLevel.Error, Source = source, Message = message, Exception = exception?.ToString(), ExtraData = extraData });

        public void Fatal(string source, string message, Exception? exception = null, string? extraData = null) => 
            Log(new LogEntry { Level = LogLevel.Fatal, Source = source, Message = message, Exception = exception?.ToString(), ExtraData = extraData });

        public void SetMinimumLevel(LogLevel level) => _minimumLevel = level;

        private void Log(LogEntry entry)
        {
            if (entry.Level < _minimumLevel) return;

            entry.Timestamp = DateTime.Now;
            _memoryLogs.Enqueue(entry);
            _pendingLogs.Enqueue(entry);

            // 🔥 内存日志限制
            while (_memoryLogs.Count > MaxMemoryLogs)
            {
                _memoryLogs.TryDequeue(out _);
            }

            LogAdded?.Invoke(this, entry);
        }

        private void InitializeDatabase()
        {
            try
            {
                using var connection = new SQLiteConnection(_dbPath);
                connection.CreateTable<LogEntry>();  // 🔥 ORM 自动建表
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化日志数据库失败: {ex.Message}");
            }
        }

        private void ConsumeLogsAsync()
        {
            var batch = new List<LogEntry>();
            var lastFlushTime = DateTime.Now;

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // 🔥 收集批量日志
                    while (batch.Count < BatchSize && _pendingLogs.TryDequeue(out var log))
                    {
                        batch.Add(log);
                    }

                    // 🔥 批量写入或超时写入
                    if (batch.Count >= BatchSize || (batch.Count > 0 && (DateTime.Now - lastFlushTime).TotalMilliseconds >= FlushIntervalMs))
                    {
                        WriteBatchToDatabase(batch);
                        batch.Clear();
                        lastFlushTime = DateTime.Now;
                    }

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"日志消费线程异常: {ex.Message}");
                }
            }

            // 🔥 关闭时写入剩余日志
            if (batch.Count > 0)
            {
                WriteBatchToDatabase(batch);
            }
        }

        private void WriteBatchToDatabase(List<LogEntry> logs)
        {
            if (logs.Count == 0) return;

            try
            {
                using var connection = new SQLiteConnection(_dbPath);
                connection.RunInTransaction(() =>
                {
                    foreach (var log in logs)
                    {
                        connection.Insert(log);  // 🔥 ORM 一行代码
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入日志失败: {ex.Message}");
            }
        }

        public List<LogEntry> GetRecentLogs(int count = 100)
        {
            return _memoryLogs.TakeLast(count).ToList();
        }

        public List<LogEntry> QueryLogs(LogLevel? level = null, string? source = null, DateTime? startTime = null, DateTime? endTime = null, int limit = 1000)
        {
            try
            {
                using var connection = new SQLiteConnection(_dbPath);
                var query = connection.Table<LogEntry>();

                if (level.HasValue)
                    query = query.Where(l => l.Level == level.Value);

                if (!string.IsNullOrEmpty(source))
                    query = query.Where(l => l.Source == source);

                if (startTime.HasValue)
                    query = query.Where(l => l.Timestamp >= startTime.Value);

                if (endTime.HasValue)
                    query = query.Where(l => l.Timestamp <= endTime.Value);

                return query.OrderByDescending(l => l.Timestamp).Take(limit).ToList();
            }
            catch
            {
                return new List<LogEntry>();
            }
        }

        public void ClearOldLogs(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                using var connection = new SQLiteConnection(_dbPath);
                connection.Execute("DELETE FROM LogEntry WHERE Timestamp < ?", cutoffDate);  // 🔥 ORM Execute
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理旧日志失败: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _consumerThread.Join(TimeSpan.FromSeconds(5));
            _cancellationTokenSource.Dispose();
        }
    }
}

