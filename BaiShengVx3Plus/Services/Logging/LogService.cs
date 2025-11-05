using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
using System.Collections.Concurrent;
using SQLite;
using System.Text;

namespace BaiShengVx3Plus.Services.Logging
{
    /// <summary>
    /// 日志服务实现
    /// 
    /// 线程安全策略：
    /// 1. 写入日志：多线程可以同时调用 Log() 方法
    /// 2. 内存队列：使用 ConcurrentQueue（线程安全）
    /// 3. 持久化：后台单线程消费队列，写入 SQLite
    /// 4. 读取：内存读取无锁，数据库读取使用连接池
    /// 
    /// 性能优化：
    /// 1. 内存缓存最近1000条日志（快速查询）
    /// 2. 批量写入 SQLite（每100条或1秒）
    /// 3. 异步不阻塞主线程
    /// </summary>
    public class LogService : ILogService, IDisposable
    {
        private readonly ConcurrentQueue<LogEntry> _memoryLogs;
        private readonly ConcurrentQueue<LogEntry> _pendingLogs;
        private readonly Thread _consumerThread;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly string _dbPath;  // 🔥 改用文件路径（ORM）
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
            _dbPath = Path.Combine(dataDir, "logs.db");  // 🔥 ORM 直接使用文件路径

            InitializeDatabase();

            _consumerThread = new Thread(ConsumeLogsAsync)
            {
                IsBackground = true,
                Name = "LogConsumerThread"
            };
            _consumerThread.Start();

            Info("LogService", "日志服务已启动");
        }

        public void Trace(string source, string message, string? extraData = null)
        {
            Log(new LogEntry
            {
                Level = LogLevel.Trace,
                Source = source,
                Message = message,
                ExtraData = extraData
            });
        }

        public void Debug(string source, string message, string? extraData = null)
        {
            Log(new LogEntry
            {
                Level = LogLevel.Debug,
                Source = source,
                Message = message,
                ExtraData = extraData
            });
        }

        public void Info(string source, string message, string? extraData = null)
        {
            Log(new LogEntry
            {
                Level = LogLevel.Info,
                Source = source,
                Message = message,
                ExtraData = extraData
            });
        }

        public void Warning(string source, string message, string? extraData = null)
        {
            Log(new LogEntry
            {
                Level = LogLevel.Warning,
                Source = source,
                Message = message,
                ExtraData = extraData
            });
        }

        public void Error(string source, string message, Exception? exception = null, string? extraData = null)
        {
            Log(new LogEntry
            {
                Level = LogLevel.Error,
                Source = source,
                Message = message,
                Exception = exception?.ToString(),
                ExtraData = extraData
            });
        }

        public void Fatal(string source, string message, Exception? exception = null, string? extraData = null)
        {
            Log(new LogEntry
            {
                Level = LogLevel.Fatal,
                Source = source,
                Message = message,
                Exception = exception?.ToString(),
                ExtraData = extraData
            });
        }

        public void Log(LogEntry entry)
        {
            if (entry.Level < _minimumLevel)
                return;

            _memoryLogs.Enqueue(entry);
            
            while (_memoryLogs.Count > MaxMemoryLogs)
            {
                _memoryLogs.TryDequeue(out _);
            }

            _pendingLogs.Enqueue(entry);

            try
            {
                LogAdded?.Invoke(this, entry);
            }
            catch
            {
            }

            WriteToTextFile(entry);
        }

        public IReadOnlyList<LogEntry> GetRecentLogs(int count = 100)
        {
            return _memoryLogs.TakeLast(count).ToList();
        }

        public IReadOnlyList<LogEntry> GetAllMemoryLogs()
        {
            return _memoryLogs.ToList();
        }

        public List<LogEntry> QueryLogs(
            DateTime? startTime = null,
            DateTime? endTime = null,
            LogLevel? minLevel = null,
            string? source = null,
            string? keyword = null,
            int limit = 1000)
        {
            var logs = new List<LogEntry>();

            using var connection = new SQLiteConnection(_dbPath);  // 🔥 ORM
            connection.Open();

            var sql = new StringBuilder("SELECT * FROM Logs WHERE 1=1");
            var parameters = new List<SQLiteParameter>();

            if (startTime.HasValue)
            {
                sql.Append(" AND Timestamp >= @StartTime");
                parameters.Add(new SQLiteParameter("@StartTime", startTime.Value));
            }

            if (endTime.HasValue)
            {
                sql.Append(" AND Timestamp <= @EndTime");
                parameters.Add(new SQLiteParameter("@EndTime", endTime.Value));
            }

            if (minLevel.HasValue)
            {
                sql.Append(" AND Level >= @MinLevel");
                parameters.Add(new SQLiteParameter("@MinLevel", (int)minLevel.Value));
            }

            if (!string.IsNullOrEmpty(source))
            {
                sql.Append(" AND Source LIKE @Source");
                parameters.Add(new SQLiteParameter("@Source", $"%{source}%"));
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                sql.Append(" AND Message LIKE @Keyword");
                parameters.Add(new SQLiteParameter("@Keyword", $"%{keyword}%"));
            }

            sql.Append($" ORDER BY Timestamp DESC LIMIT {limit}");

            using var command = new SQLiteCommand(sql.ToString(), connection);
            command.Parameters.AddRange(parameters.ToArray());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                logs.Add(new LogEntry
                {
                    Id = reader.GetInt64(0),
                    Timestamp = reader.GetDateTime(1),
                    Level = (LogLevel)reader.GetInt32(2),
                    Source = reader.GetString(3),
                    Message = reader.GetString(4),
                    Exception = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ThreadId = reader.GetInt32(6),
                    UserId = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ExtraData = reader.IsDBNull(8) ? null : reader.GetString(8)
                });
            }

            return logs;
        }

        public LogStatistics GetStatistics()
        {
            using var connection = new SQLiteConnection(_dbPath);  // 🔥 ORM
            connection.Open();

            using var command = new SQLiteCommand(@"
                SELECT 
                    COUNT(*) as Total,
                    SUM(CASE WHEN Level = 0 THEN 1 ELSE 0 END) as TraceCount,
                    SUM(CASE WHEN Level = 1 THEN 1 ELSE 0 END) as DebugCount,
                    SUM(CASE WHEN Level = 2 THEN 1 ELSE 0 END) as InfoCount,
                    SUM(CASE WHEN Level = 3 THEN 1 ELSE 0 END) as WarningCount,
                    SUM(CASE WHEN Level = 4 THEN 1 ELSE 0 END) as ErrorCount,
                    SUM(CASE WHEN Level = 5 THEN 1 ELSE 0 END) as FatalCount,
                    MIN(Timestamp) as FirstLog,
                    MAX(Timestamp) as LastLog
                FROM Logs", connection);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new LogStatistics
                {
                    TotalCount = reader.GetInt32(0),
                    TraceCount = reader.GetInt32(1),
                    DebugCount = reader.GetInt32(2),
                    InfoCount = reader.GetInt32(3),
                    WarningCount = reader.GetInt32(4),
                    ErrorCount = reader.GetInt32(5),
                    FatalCount = reader.GetInt32(6),
                    FirstLogTime = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    LastLogTime = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
                };
            }

            return new LogStatistics();
        }

        public void ClearMemoryLogs()
        {
            while (_memoryLogs.TryDequeue(out _)) { }
        }

        public void ClearDatabaseLogs()
        {
            using var connection = new SQLiteConnection(_dbPath);  // 🔥 ORM
            connection.Open();
            using var command = new SQLiteCommand("DELETE FROM Logs", connection);
            command.ExecuteNonQuery();
        }

        public void SetMinimumLevel(LogLevel level)
        {
            _minimumLevel = level;
        }

        public async Task ExportToFileAsync(string filePath, DateTime? startTime = null, DateTime? endTime = null)
        {
            var logs = QueryLogs(startTime, endTime, limit: int.MaxValue);
            
            await Task.Run(() =>
            {
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
                foreach (var log in logs)
                {
                    writer.WriteLine(log.ToText());
                }
            });
        }

        private void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_dbPath);  // 🔥 ORM
            connection.Open();

            using var walCmd = new SQLiteCommand("PRAGMA journal_mode=WAL;", connection);
            walCmd.ExecuteNonQuery();

            using var command = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS Logs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp DATETIME NOT NULL,
                    Level INTEGER NOT NULL,
                    Source TEXT NOT NULL,
                    Message TEXT NOT NULL,
                    Exception TEXT,
                    ThreadId INTEGER NOT NULL,
                    UserId TEXT,
                    ExtraData TEXT
                );

                CREATE INDEX IF NOT EXISTS idx_timestamp ON Logs(Timestamp);
                CREATE INDEX IF NOT EXISTS idx_level ON Logs(Level);
                CREATE INDEX IF NOT EXISTS idx_source ON Logs(Source);
            ", connection);

            command.ExecuteNonQuery();
        }

        private void ConsumeLogsAsync()
        {
            var batch = new List<LogEntry>();
            var lastFlushTime = DateTime.Now;

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    while (_pendingLogs.TryDequeue(out var entry) && batch.Count < BatchSize)
                    {
                        batch.Add(entry);
                    }

                    if (batch.Count >= BatchSize || 
                        (batch.Count > 0 && (DateTime.Now - lastFlushTime).TotalMilliseconds >= FlushIntervalMs))
                    {
                        WriteBatchToDatabase(batch);
                        batch.Clear();
                        lastFlushTime = DateTime.Now;
                    }

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"日志消费线程错误: {ex.Message}");
                }
            }

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
                using var connection = new SQLiteConnection(_dbPath);  // 🔥 ORM
                connection.Open();

                using var transaction = connection.BeginTransaction();
                using var command = new SQLiteCommand(@"
                    INSERT INTO Logs (Timestamp, Level, Source, Message, Exception, ThreadId, UserId, ExtraData)
                    VALUES (@Timestamp, @Level, @Source, @Message, @Exception, @ThreadId, @UserId, @ExtraData)
                ", connection, transaction);

                foreach (var log in logs)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@Timestamp", log.Timestamp);
                    command.Parameters.AddWithValue("@Level", (int)log.Level);
                    command.Parameters.AddWithValue("@Source", log.Source);
                    command.Parameters.AddWithValue("@Message", log.Message);
                    command.Parameters.AddWithValue("@Exception", log.Exception ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ThreadId", log.ThreadId);
                    command.Parameters.AddWithValue("@UserId", log.UserId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ExtraData", log.ExtraData ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"写入数据库失败: {ex.Message}");
            }
        }

        private void WriteToTextFile(LogEntry entry)
        {
            try
            {
                var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                Directory.CreateDirectory(logDir);

                var logFile = Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd}.log");
                File.AppendAllText(logFile, entry.ToText() + Environment.NewLine);
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            Info("LogService", "日志服务正在停止...");

            _cancellationTokenSource.Cancel();
            _consumerThread.Join(TimeSpan.FromSeconds(5));
            _cancellationTokenSource.Dispose();
        }
    }
}

