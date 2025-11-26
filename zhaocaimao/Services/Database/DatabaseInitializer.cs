using System;
using System.IO;
using SQLite;
using zhaocaimao.Contracts;
using zhaocaimao.Models;
using zhaocaimao.Models.AutoBet;
using zhaocaimao.Models.Games.Binggo;

namespace zhaocaimao.Services.Database
{
    /// <summary>
    /// 🔥 统一的数据库初始化管理器
    /// 负责所有数据库和表的初始化，确保启动时所有表都已创建
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly ILogService? _logService;

        public DatabaseInitializer(ILogService? logService = null)
        {
            _logService = logService;
        }
        
        /// <summary>
        /// 记录日志（如果 LogService 可用）
        /// </summary>
        private void Log(string level, string message)
        {
            if (_logService != null)
            {
                switch (level.ToLower())
                {
                    case "info":
                        _logService.Info("DatabaseInitializer", message);
                        break;
                    case "debug":
                        _logService.Debug("DatabaseInitializer", message);
                        break;
                    case "warning":
                        _logService.Warning("DatabaseInitializer", message);
                        break;
                    case "error":
                        _logService.Error("DatabaseInitializer", message);
                        break;
                    default:
                        _logService.Info("DatabaseInitializer", message);
                        break;
                }
            }
            else
            {
                // 如果 LogService 不可用，输出到控制台
                Console.WriteLine($"[DatabaseInitializer] {message}");
            }
        }

        /// <summary>
        /// 🔥 初始化日志数据库（logs.db）
        /// 在 LogService 构造函数中调用
        /// </summary>
        public void InitializeLogDatabase(string dbPath)
        {
            try
            {
                Log("info", $"初始化日志数据库: {dbPath}");

                // 🔥 确保数据库目录存在
                var dbDir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                    Log("info", $"创建日志数据库目录: {dbDir}");
                }

                // 🔥 如果数据库文件存在但可能损坏，尝试修复或重建
                if (File.Exists(dbPath))
                {
                    try
                    {
                        // 尝试打开数据库，检查是否损坏
                        using var testConnection = new SQLiteConnection(dbPath);
                        var testResult = testConnection.ExecuteScalar<int>("SELECT 1");
                        Log("debug", "✅ 现有数据库文件可正常访问");
                    }
                    catch (Exception testEx)
                    {
                        Log("warning", $"⚠️ 数据库文件可能损坏，尝试重建: {testEx.Message}");
                        try
                        {
                            // 备份旧文件（如果可能）
                            var backupPath = dbPath + ".backup." + DateTime.Now.ToString("yyyyMMddHHmmss");
                            File.Copy(dbPath, backupPath, overwrite: true);
                            Log("info", $"📦 已备份旧数据库到: {backupPath}");
                        }
                        catch
                        {
                            // 备份失败，继续删除
                        }

                        // 删除损坏的数据库文件
                        File.Delete(dbPath);
                        Log("info", "🗑️ 已删除损坏的数据库文件，将重新创建");
                    }
                }

                // 🔥 创建或打开数据库连接
                using var connection = new SQLiteConnection(dbPath);

                // 🔥 创建日志表
                connection.CreateTable<LogEntry>();
                Log("debug", "✓ 日志表: LogEntry");

                // 🔥 验证表是否创建成功
                var tableCount = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='LogEntry'");

                if (tableCount == 0)
                {
                    throw new Exception("LogEntry 表创建失败！");
                }

                Log("info", "✅ 日志数据库初始化完成");
            }
            catch (SQLiteException sqlEx)
            {
                var errorMsg = $"❌ 初始化日志数据库失败（SQLite错误）:\n" +
                              $"   错误: {sqlEx.Message}\n" +
                              $"   数据库路径: {dbPath}\n" +
                              $"   错误代码: {sqlEx.Result}";
                Log("error", errorMsg);
                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                var errorMsg = $"❌ 初始化日志数据库失败:\n" +
                              $"   错误: {ex.Message}\n" +
                              $"   数据库路径: {dbPath}";
                Log("error", errorMsg);
                throw new Exception(errorMsg, ex);
            }
        }

        /// <summary>
        /// 🔥 初始化共享数据库所有表（business.db）
        /// 架构说明：所有微信号共享此数据库，数据按 GroupWxId 隔离
        /// </summary>
        public void InitializeAllTables(SQLiteConnection db)
        {
            try
            {
                Log("info", "🗄️ 初始化共享数据库表（business.db）...");

                // ========================================
                // 全局表（全局共享，不按 GroupWxId 隔离）
                // ========================================

                db.CreateTable<BetConfig>();
                Log("debug", "✓ 全局表: BetConfig");

                db.CreateTable<BinggoLotteryData>();
                Log("debug", "✓ 全局表: BinggoLotteryData");

                db.CreateTable<BinggoBetItem>();
                Log("debug", "✓ 全局表: BinggoBetItem");

                db.CreateTable<Models.AutoBet.BetRecord>();
                Log("debug", "✓ 全局表: BetRecord");

                // ========================================
                // 业务表（按 GroupWxId 隔离）
                // ========================================

                db.CreateTable<V2Member>();
                Log("debug", "✓ 业务表: V2Member (按 GroupWxId 隔离)");

                db.CreateTable<V2MemberOrder>();
                Log("debug", "✓ 业务表: V2MemberOrder (按 GroupWxId 隔离)");

                db.CreateTable<V2CreditWithdraw>();
                Log("debug", "✓ 业务表: V2CreditWithdraw (按 GroupWxId 隔离)");

                db.CreateTable<V2BalanceChange>();
                Log("debug", "✓ 业务表: V2BalanceChange (按 GroupWxId 隔离)");

                // ========================================
                // 基础表（所有微信号共享）
                // ========================================

                db.CreateTable<WxContact>();
                Log("debug", "✓ 基础表: WxContact");

                db.CreateTable<WxUserInfo>();
                Log("debug", "✓ 基础表: WxUserInfo");

                Log("info", "✅ 共享数据库表初始化完成（10张表）");
                Log("info", "📌 架构说明：所有微信号共享此数据库，数据按 GroupWxId 隔离");
            }
            catch (Exception ex)
            {
                Log("error", $"初始化共享数据库表失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 🔥 废弃：InitializeGlobalTables（已合并到 InitializeAllTables）
        /// 保留此方法仅为兼容旧代码，实际调用 InitializeAllTables
        /// </summary>
        [Obsolete("已废弃，请使用 InitializeAllTables")]
        public void InitializeGlobalTables(SQLiteConnection db)
        {
            InitializeAllTables(db);
        }

        /// <summary>
        /// 🔥 废弃：InitializeWxTables（已合并到 InitializeAllTables）
        /// 保留此方法仅为兼容旧代码，实际调用 InitializeAllTables
        /// </summary>
        [Obsolete("已废弃，请使用 InitializeAllTables")]
        public void InitializeWxTables(SQLiteConnection db)
        {
            // 不再需要微信专属表，所有表都在共享数据库中
            Log("warning", "⚠️ InitializeWxTables 已废弃，数据库架构已改为共享模式");
        }
    }
}

