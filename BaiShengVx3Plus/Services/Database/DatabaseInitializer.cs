using System;
using System.IO;
using SQLite;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Models.AutoBet;
using BaiShengVx3Plus.Models.Games.Binggo;

namespace BaiShengVx3Plus.Services.Database
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
        /// 🔥 初始化全局数据库表（business.db）
        /// 存储全局共享数据：自动投注配置、开奖数据等
        /// </summary>
        public void InitializeGlobalTables(SQLiteConnection db)
        {
            try
            {
                Log("info", "🗄️ 初始化全局数据库表...");

                // ========================================
                // 🔥 自动投注配置表（全局共享）
                // ========================================

                db.CreateTable<BetConfig>();
                Log("debug", "✓ 全局表: BetConfig");

                // ========================================
                // 🔥 游戏开奖数据表（全局共享）
                // ========================================

                db.CreateTable<BinggoLotteryData>();
                Log("debug", "✓ 全局表: BinggoLotteryData");

                db.CreateTable<BinggoBetItem>();
                Log("debug", "✓ 全局表: BinggoBetItem");

                // ========================================
                // 🔥 投注记录表（全局共享）
                // ========================================

                db.CreateTable<Models.AutoBet.BetRecord>();
                Log("debug", "✓ 全局表: BetRecord");

                Log("info", "✅ 全局数据库表初始化完成（4张表）");
            }
            catch (Exception ex)
            {
                Log("error", $"初始化全局数据库表失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 🔥 初始化微信专属数据库表（business_{wxid}.db）
        /// 存储微信账号专属数据：会员、订单、上下分记录等
        /// </summary>
        public void InitializeWxTables(SQLiteConnection db)
        {
            try
            {
                Log("info", "🗄️ 初始化微信专属数据库表...");

                // ========================================
                // 🔥 核心业务表（微信账号专属）
                // ========================================

                db.CreateTable<V2Member>();
                Log("debug", "✓ 微信专属表: V2Member");

                db.CreateTable<V2MemberOrder>();
                Log("debug", "✓ 微信专属表: V2MemberOrder");

                db.CreateTable<V2CreditWithdraw>();
                Log("debug", "✓ 微信专属表: V2CreditWithdraw");

                db.CreateTable<V2BalanceChange>();
                Log("debug", "✓ 微信专属表: V2BalanceChange");

                // ========================================
                // 🔥 基础数据表（微信账号专属）
                // ========================================

                db.CreateTable<WxContact>();
                Log("debug", "✓ 微信专属表: WxContact");

                db.CreateTable<WxUserInfo>();
                Log("debug", "✓ 微信专属表: WxUserInfo");

                // 🔥 注意：LogEntry 表在日志数据库中，不在微信专属数据库中
                // 微信专属数据库不需要 LogEntry 表

                Log("info", "✅ 微信专属数据库表初始化完成（6张表）");
            }
            catch (Exception ex)
            {
                Log("error", $"初始化微信专属数据库表失败: {ex.Message}");
                throw;
            }
        }
    }
}

