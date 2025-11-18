using System;
using zhaocaimao.Models;
using zhaocaimao.Contracts;
using System.Collections.Concurrent;
using SQLite;

namespace zhaocaimao.Services.Logging
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
        private const int MaxMemoryLogs = 3000;  // 🔥 内存日志最大3000条
        private const int BatchSize = 100;
        private const int FlushIntervalMs = 1000;

        public event EventHandler<LogEntry>? LogAdded;

        public LogService()
        {
            _memoryLogs = new ConcurrentQueue<LogEntry>();
            _pendingLogs = new ConcurrentQueue<LogEntry>();
            _cancellationTokenSource = new CancellationTokenSource();

            // 🔥 使用 AppData\Local 目录，无需管理员权限
            var dataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "zhaocaimao",
                "Data");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, "logs.db");

            // 🔥 LogService 自己初始化数据库（避免循环依赖）
            // DatabaseInitializer 只用于其他数据库的初始化
            InitializeDatabase();

            _consumerThread = new Thread(ConsumeLogsAsync)
            {
                IsBackground = true,
                Name = "LogConsumerThread"
            };
            _consumerThread.Start();

            Info("LogService", "日志服务已启动");
        }

        // ========================================
        // 写入方法
        // ========================================

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

        public void Log(LogEntry entry)
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

        // ========================================
        // 查询方法
        // ========================================

        public IReadOnlyList<LogEntry> GetRecentLogs(int count = 100)
        {
            // 🔥 优化：使用 ToArray 再反向取，比 TakeLast 快
            var array = _memoryLogs.ToArray();
            if (array.Length <= count)
                return array;
            
            // 只返回最后 count 个
            var result = new LogEntry[count];
            Array.Copy(array, array.Length - count, result, 0, count);
            return result;
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
            int limit = 3000)  // 🔥 查询日志默认最多3000条
        {
            try
            {
                using var connection = new SQLiteConnection(_dbPath);
                var query = connection.Table<LogEntry>();

                if (startTime.HasValue)
                    query = query.Where(l => l.Timestamp >= startTime.Value);

                if (endTime.HasValue)
                    query = query.Where(l => l.Timestamp <= endTime.Value);

                if (minLevel.HasValue)
                    query = query.Where(l => l.Level >= minLevel.Value);

                if (!string.IsNullOrEmpty(source))
                    query = query.Where(l => l.Source == source);

                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(l => l.Message.Contains(keyword));

                return query.OrderByDescending(l => l.Timestamp).Take(limit).ToList();
            }
            catch
            {
                return new List<LogEntry>();
            }
        }

        public LogStatistics GetStatistics()
        {
            try
            {
                using var connection = new SQLiteConnection(_dbPath);
                
                // 🔥 性能优化：使用 SQL COUNT 而不是加载所有数据到内存
                // 避免加载100万+条数据导致卡顿
                return new LogStatistics
                {
                    TotalCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM LogEntry"),
                    TraceCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM LogEntry WHERE Level = ?", (int)LogLevel.Trace),
                    DebugCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM LogEntry WHERE Level = ?", (int)LogLevel.Debug),
                    InfoCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM LogEntry WHERE Level = ?", (int)LogLevel.Info),
                    WarningCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM LogEntry WHERE Level = ?", (int)LogLevel.Warning),
                    ErrorCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM LogEntry WHERE Level = ?", (int)LogLevel.Error),
                    FatalCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM LogEntry WHERE Level = ?", (int)LogLevel.Fatal),
                    FirstLogTime = connection.ExecuteScalar<DateTime?>("SELECT MIN(Timestamp) FROM LogEntry"),
                    LastLogTime = connection.ExecuteScalar<DateTime?>("SELECT MAX(Timestamp) FROM LogEntry")
                };
            }
            catch
            {
                return new LogStatistics();
            }
        }

        // ========================================
        // 管理方法
        // ========================================

        public void ClearMemoryLogs()
        {
            _memoryLogs.Clear();
        }

        public void ClearDatabaseLogs()
        {
            try
            {
                using var connection = new SQLiteConnection(_dbPath);
                connection.Execute("DELETE FROM LogEntry");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清空数据库日志失败: {ex.Message}");
            }
        }

        public void SetMinimumLevel(LogLevel level)
        {
            _minimumLevel = level;
        }

        public async Task ExportToFileAsync(string filePath, DateTime? startTime = null, DateTime? endTime = null, int limit = 10000)
        {
            await Task.Run(() =>
            {
                try
                {
                    // 🔥 性能优化：默认只导出最新10000条日志，避免100万+条数据卡顿
                    // 如果需要导出所有日志，可以传入 limit = int.MaxValue
                    Console.WriteLine($"[LogService] 开始导出日志，限制条数: {limit}");
                    var logs = QueryLogs(startTime, endTime, limit: limit);
                    var lines = logs.Select(log => 
                        $"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] [{log.Level}] [{log.Source}] {log.Message}");
                    File.WriteAllLines(filePath, lines);
                    Console.WriteLine($"[LogService] 导出完成，共 {logs.Count} 条日志");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"导出日志失败: {ex.Message}");
                }
            });
        }

        // ========================================
        // 私有方法
        // ========================================

        private void InitializeDatabase()
        {
            try
            {
                // 🔥 确保数据库目录存在
                var dbDir = Path.GetDirectoryName(_dbPath);
                if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                    Console.WriteLine($"创建日志数据库目录: {dbDir}");
                }
                
                Console.WriteLine($"初始化日志数据库: {_dbPath}");
                
                // 🔥 如果数据库文件存在但可能损坏，尝试修复或重建
                if (File.Exists(_dbPath))
                {
                    try
                    {
                        // 尝试打开数据库，检查是否损坏
                        using var testConnection = new SQLiteConnection(_dbPath);
                        var testResult = testConnection.ExecuteScalar<int>("SELECT 1");
                        Console.WriteLine("✅ 现有数据库文件可正常访问");
                    }
                    catch (Exception testEx)
                    {
                        Console.WriteLine($"⚠️ 数据库文件可能损坏，尝试重建: {testEx.Message}");
                        try
                        {
                            // 备份旧文件（如果可能）
                            var backupPath = _dbPath + ".backup." + DateTime.Now.ToString("yyyyMMddHHmmss");
                            File.Copy(_dbPath, backupPath, overwrite: true);
                            Console.WriteLine($"📦 已备份旧数据库到: {backupPath}");
                        }
                        catch
                        {
                            // 备份失败，继续删除
                        }
                        
                        // 删除损坏的数据库文件
                        File.Delete(_dbPath);
                        Console.WriteLine("🗑️ 已删除损坏的数据库文件，将重新创建");
                    }
                }
                
                // 🔥 创建或打开数据库连接
                using var connection = new SQLiteConnection(_dbPath);
                
                // 🔥 一次性创建所有表（确保表存在）
                connection.CreateTable<LogEntry>();
                
                Console.WriteLine("✅ 日志数据库表初始化成功");
                
                // 验证表是否创建成功
                var tableCount = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='LogEntry'");
                
                if (tableCount == 0)
                {
                    throw new Exception("LogEntry 表创建失败！");
                }
                
                Console.WriteLine($"✅ LogEntry 表验证成功");
            }
            catch (SQLiteException sqlEx)
            {
                var errorMsg = $"❌ 初始化日志数据库失败（SQLite错误）:\n" +
                              $"   错误: {sqlEx.Message}\n" +
                              $"   数据库路径: {_dbPath}\n" +
                              $"   错误代码: {sqlEx.Result}\n\n" +
                              $"   建议：\n" +
                              $"   1. 检查数据库文件是否被其他程序占用\n" +
                              $"   2. 检查目录权限\n" +
                              $"   3. 尝试手动删除数据库文件后重新启动程序";
                Console.WriteLine(errorMsg);
                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                var errorMsg = $"❌ 初始化日志数据库失败:\n" +
                              $"   错误: {ex.Message}\n" +
                              $"   数据库路径: {_dbPath}\n\n" +
                              $"   建议：\n" +
                              $"   1. 检查目录权限\n" +
                              $"   2. 检查磁盘空间\n" +
                              $"   3. 尝试手动删除数据库文件后重新启动程序";
                Console.WriteLine(errorMsg);
                Console.WriteLine($"   堆栈: {ex.StackTrace}");
                throw new Exception(errorMsg, ex);
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
                
                // 🔥 表已经在 InitializeDatabase 中创建，直接写入
                connection.RunInTransaction(() =>
                {
                    foreach (var log in logs)
                    {
                        connection.Insert(log);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 写入日志失败: {ex.Message}");
                Console.WriteLine($"   数据库路径: {_dbPath}");
                Console.WriteLine($"   待写入日志数: {logs.Count}");
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
