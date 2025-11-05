using System.Data.SQLite;
using System.Reflection;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Services.Database
{
    /// <summary>
    /// 数据库服务实现
    /// 管理业务数据库（business.db）
    /// 
    /// 特性：
    /// 1. WAL 模式：读写并发
    /// 2. 连接池：复用连接
    /// 3. 短事务：快速提交
    /// 4. 独立于日志数据库：零冲突
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private string _connectionString = "";
        private readonly ILogService? _logService;
        private string? _currentWxid;

        public DatabaseService(ILogService? logService = null)
        {
            _logService = logService;

            // 🔥 不再使用固定的 business.db
            // 在用户登录后调用 SwitchDatabase(wxid) 切换到用户专属数据库
            _logService?.Info("DatabaseService", "数据库服务已初始化，等待用户登录后切换数据库");
        }

        public SQLiteConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("数据库未初始化，请先调用 SwitchDatabase(wxid)");
            }
            
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            // 启用 WAL 模式（只需执行一次，但多次执行无害）
            using var cmd = new SQLiteCommand("PRAGMA journal_mode=WAL;", connection);
            cmd.ExecuteNonQuery();

            // 优化设置
            using var cmd2 = new SQLiteCommand(@"
                PRAGMA synchronous=NORMAL;
                PRAGMA cache_size=10000;
                PRAGMA temp_store=MEMORY;
            ", connection);
            cmd2.ExecuteNonQuery();

            return connection;
        }

        public T? ExecuteScalar<T>(string sql, object? parameters = null)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new SQLiteCommand(sql, connection);
                AddParameters(command, parameters);
                
                var result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                _logService?.Error("DatabaseService", $"ExecuteScalar 失败: {sql}", ex);
                throw;
            }
        }

        public int ExecuteNonQuery(string sql, object? parameters = null)
        {
            try
            {
                using var connection = GetConnection();
                using var command = new SQLiteCommand(sql, connection);
                AddParameters(command, parameters);
                
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logService?.Error("DatabaseService", $"ExecuteNonQuery 失败: {sql}", ex);
                throw;
            }
        }

        public T? QuerySingle<T>(string sql, object? parameters = null) where T : class, new()
        {
            try
            {
                using var connection = GetConnection();
                using var command = new SQLiteCommand(sql, connection);
                AddParameters(command, parameters);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapToObject<T>(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logService?.Error("DatabaseService", $"QuerySingle 失败: {sql}", ex);
                throw;
            }
        }

        public List<T> Query<T>(string sql, object? parameters = null) where T : class, new()
        {
            var results = new List<T>();

            try
            {
                using var connection = GetConnection();
                using var command = new SQLiteCommand(sql, connection);
                AddParameters(command, parameters);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(MapToObject<T>(reader));
                }
            }
            catch (Exception ex)
            {
                _logService?.Error("DatabaseService", $"Query 失败: {sql}", ex);
                throw;
            }

            return results;
        }

        public void ExecuteTransaction(Action<SQLiteConnection, SQLiteTransaction> action)
        {
            using var connection = GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                action(connection, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logService?.Error("DatabaseService", "事务执行失败", ex);
                throw;
            }
        }

        public void InitializeDatabase()
        {
            try
            {
                using var connection = GetConnection();

                // 创建会员表
                using var cmdMembers = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Members (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Wxid TEXT NOT NULL,
                        Nickname TEXT,
                        Phone TEXT,
                        Balance REAL DEFAULT 0,
                        State INTEGER DEFAULT 0,
                        Remark TEXT,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );
                    CREATE INDEX IF NOT EXISTS idx_members_wxid ON Members(Wxid);
                ", connection);
                cmdMembers.ExecuteNonQuery();

                // 创建订单表
                using var cmdOrders = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Orders (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        MemberId INTEGER NOT NULL,
                        OrderNo TEXT NOT NULL,
                        Amount REAL NOT NULL,
                        Status INTEGER DEFAULT 0,
                        OrderType INTEGER DEFAULT 0,
                        TimeStampBet INTEGER,
                        Remark TEXT,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (MemberId) REFERENCES Members(Id)
                    );
                    CREATE INDEX IF NOT EXISTS idx_orders_memberId ON Orders(MemberId);
                    CREATE INDEX IF NOT EXISTS idx_orders_orderNo ON Orders(OrderNo);
                ", connection);
                cmdOrders.ExecuteNonQuery();

                // 创建联系人表
                using var cmdContacts = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Contacts (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Wxid TEXT NOT NULL UNIQUE,
                        Account TEXT,
                        Nickname TEXT,
                        Remark TEXT,
                        Avatar TEXT,
                        Sex INTEGER DEFAULT 0,
                        Province TEXT,
                        City TEXT,
                        Country TEXT,
                        IsGroup INTEGER DEFAULT 0,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );
                    CREATE INDEX IF NOT EXISTS idx_contacts_wxid ON Contacts(Wxid);
                ", connection);
                cmdContacts.ExecuteNonQuery();

                _logService?.Info("DatabaseService", "业务数据库初始化成功");
            }
            catch (Exception ex)
            {
                _logService?.Error("DatabaseService", "数据库初始化失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 初始化业务数据库（使用 wxid 组合表名）
        /// </summary>
        public async Task InitializeBusinessDatabaseAsync(string wxid)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(wxid))
                    {
                        _logService?.Warning("DatabaseService", "wxid 为空，无法初始化业务数据库");
                        return;
                    }

                    using var connection = GetConnection();

                    // 创建带 wxid 后缀的联系人表
                    string tableName = $"contacts_{wxid}";
                    var createTableSql = $@"
                        CREATE TABLE IF NOT EXISTS {tableName} (
                            wxid TEXT PRIMARY KEY,
                            account TEXT,
                            nickname TEXT,
                            remark TEXT,
                            avatar TEXT,
                            sex INTEGER DEFAULT 0,
                            province TEXT,
                            city TEXT,
                            country TEXT,
                            is_group INTEGER DEFAULT 0,
                            update_time INTEGER DEFAULT 0
                        );
                        CREATE INDEX IF NOT EXISTS idx_{tableName}_nickname ON {tableName}(nickname);
                    ";

                    using var cmd = new SQLiteCommand(createTableSql, connection);
                    cmd.ExecuteNonQuery();

                    _logService?.Info("DatabaseService", $"业务数据库表 {tableName} 初始化成功");
                }
                catch (Exception ex)
                {
                    _logService?.Error("DatabaseService", $"初始化业务数据库失败（wxid: {wxid}）", ex);
                    throw;
                }
            });
        }

        private void AddParameters(SQLiteCommand command, object? parameters)
        {
            if (parameters == null) return;

            var properties = parameters.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(parameters);
                command.Parameters.AddWithValue($"@{prop.Name}", value ?? DBNull.Value);
            }
        }

        private T MapToObject<T>(SQLiteDataReader reader) where T : class, new()
        {
            var obj = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var property = properties.FirstOrDefault(p => 
                    p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                if (property != null && property.CanWrite)
                {
                    var value = reader.GetValue(i);
                    if (value != DBNull.Value)
                    {
                        // 类型转换
                        if (property.PropertyType == typeof(DateTime) && value is string dateStr)
                        {
                            if (DateTime.TryParse(dateStr, out var date))
                            {
                                property.SetValue(obj, date);
                            }
                        }
                        else if (property.PropertyType.IsEnum && value is long enumValue)
                        {
                            property.SetValue(obj, Enum.ToObject(property.PropertyType, enumValue));
                        }
                        else
                        {
                            try
                            {
                                property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                            }
                            catch
                            {
                                // 忽略类型转换失败
                            }
                        }
                    }
                }
            }

            return obj;
        }
        
        /// <summary>
        /// 切换到指定用户的数据库
        /// </summary>
        public void SwitchDatabase(string wxid)
        {
            if (string.IsNullOrEmpty(wxid))
            {
                throw new ArgumentException("wxid 不能为空", nameof(wxid));
            }
            
            _currentWxid = wxid;
            
            // 🔥 动态数据库路径：business_{wxid}.db
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", $"business_{wxid}.db");
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            _connectionString = $"Data Source={dbPath};Version=3;Pooling=true;Max Pool Size=10;";
            
            // 初始化新数据库
            InitializeDatabase();
            
            _logService?.Info("DatabaseService", $"✓ 已切换到用户数据库: business_{wxid}.db");
        }
        
        /// <summary>
        /// 获取当前数据库路径
        /// </summary>
        public string GetCurrentDatabasePath()
        {
            if (string.IsNullOrEmpty(_currentWxid))
            {
                return "未连接（等待用户登录）";
            }
            return $"business_{_currentWxid}.db";
        }
        
        /// <summary>
        /// 获取当前用户的 wxid
        /// </summary>
        public string? GetCurrentWxid()
        {
            return _currentWxid;
        }
        
        /// <summary>
        /// 清空数据库连接（用户登出时调用）
        /// </summary>
        public void ClearDatabase()
        {
            _currentWxid = null;
            _connectionString = "";
            
            _logService?.Info("DatabaseService", "✓ 数据库连接已清空，等待下次登录");
        }
    }
}

