using System.Data.SQLite;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 数据库服务接口
    /// 管理业务数据库（business.db）
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// 获取数据库连接
        /// </summary>
        SQLiteConnection GetConnection();

        /// <summary>
        /// 执行查询
        /// </summary>
        T? ExecuteScalar<T>(string sql, object? parameters = null);

        /// <summary>
        /// 执行命令（INSERT, UPDATE, DELETE）
        /// </summary>
        int ExecuteNonQuery(string sql, object? parameters = null);

        /// <summary>
        /// 查询单条记录
        /// </summary>
        T? QuerySingle<T>(string sql, object? parameters = null) where T : class, new();

        /// <summary>
        /// 查询多条记录
        /// </summary>
        List<T> Query<T>(string sql, object? parameters = null) where T : class, new();

        /// <summary>
        /// 执行事务
        /// </summary>
        void ExecuteTransaction(Action<SQLiteConnection, SQLiteTransaction> action);

        /// <summary>
        /// 初始化数据库
        /// </summary>
        void InitializeDatabase();

        /// <summary>
        /// 初始化业务数据库（使用 wxid 组合表名）
        /// </summary>
        Task InitializeBusinessDatabaseAsync(string wxid);
        
        /// <summary>
        /// 切换到指定用户的数据库 (business_{wxid}.db)
        /// </summary>
        void SwitchDatabase(string wxid);
        
        /// <summary>
        /// 获取当前数据库路径
        /// </summary>
        string GetCurrentDatabasePath();
        
        /// <summary>
        /// 获取当前用户的 wxid
        /// </summary>
        string? GetCurrentWxid();
        
        /// <summary>
        /// 清空数据库连接（用户登出时调用）
        /// </summary>
        void ClearDatabase();
    }
}

