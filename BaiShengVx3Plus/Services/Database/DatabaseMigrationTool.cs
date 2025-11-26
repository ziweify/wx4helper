using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SQLite;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Models.Games.Binggo;

namespace BaiShengVx3Plus.Services.Database
{
    /// <summary>
    /// 数据库迁移工具
    /// 将旧的微信专属数据库（business_{wxid}.db）迁移到新的共享数据库（business.db）
    /// 
    /// 使用方法：
    /// 1. 备份现有数据
    /// 2. 创建 DatabaseMigrationTool 实例
    /// 3. 调用 MigrateFromWxDbToSharedDb() 方法
    /// 4. 验证迁移结果
    /// </summary>
    public class DatabaseMigrationTool
    {
        private readonly ILogService? _logService;

        public DatabaseMigrationTool(ILogService? logService = null)
        {
            _logService = logService;
        }

        /// <summary>
        /// 日志输出
        /// </summary>
        private void Log(string level, string message)
        {
            if (_logService != null)
            {
                switch (level.ToLower())
                {
                    case "info":
                        _logService.Info("DatabaseMigration", message);
                        break;
                    case "warning":
                        _logService.Warning("DatabaseMigration", message);
                        break;
                    case "error":
                        _logService.Error("DatabaseMigration", message);
                        break;
                    default:
                        _logService.Debug("DatabaseMigration", message);
                        break;
                }
            }
            else
            {
                Console.WriteLine($"[DatabaseMigration] {message}");
            }
        }

        /// <summary>
        /// 🔥 从微信专属数据库迁移到共享数据库
        /// </summary>
        /// <returns>(成功, 错误消息)</returns>
        public (bool success, string message) MigrateFromWxDbToSharedDb()
        {
            try
            {
                Log("info", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Log("info", "🔄 开始数据库迁移（微信专属DB → 共享DB）");
                Log("info", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                var dataDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "BaiShengVx3Plus",
                    "Data");

                if (!Directory.Exists(dataDirectory))
                {
                    return (false, "数据目录不存在");
                }

                // 1. 查找所有旧的微信专属数据库
                var wxDbFiles = Directory.GetFiles(dataDirectory, "business_*.db");
                if (wxDbFiles.Length == 0)
                {
                    Log("info", "✅ 未找到旧的微信专属数据库，无需迁移");
                    return (true, "未找到需要迁移的数据库");
                }

                Log("info", $"📁 找到 {wxDbFiles.Length} 个微信专属数据库需要迁移");

                // 2. 打开或创建新的共享数据库
                string sharedDbPath = Path.Combine(dataDirectory, "business.db");
                using var sharedDb = new SQLiteConnection(sharedDbPath);

                // 初始化表结构
                Log("info", "📋 初始化共享数据库表结构...");
                var initializer = new DatabaseInitializer(_logService);
                initializer.InitializeAllTables(sharedDb);

                // 3. 迁移统计
                int totalMembers = 0;
                int totalOrders = 0;
                int totalCredits = 0;
                int totalBalanceChanges = 0;
                int skippedMembers = 0;
                int skippedOrders = 0;

                // 4. 逐个迁移微信专属数据库
                foreach (var wxDbFile in wxDbFiles)
                {
                    string wxid = Path.GetFileNameWithoutExtension(wxDbFile).Replace("business_", "");
                    Log("info", $"\n📦 迁移数据库: {Path.GetFileName(wxDbFile)} (wxid: {wxid})");

                    using var wxDb = new SQLiteConnection(wxDbFile);

                    // 4.1 迁移会员数据
                    var members = wxDb.Table<V2Member>().ToList();
                    Log("info", $"   会员数据: {members.Count} 条");

                    foreach (var member in members)
                    {
                        try
                        {
                            // 检查是否已存在（按 GroupWxId + Wxid）
                            var existing = sharedDb.Table<V2Member>()
                                .FirstOrDefault(m => m.GroupWxId == member.GroupWxId && m.Wxid == member.Wxid);

                            if (existing == null)
                            {
                                // 新增
                                member.Id = 0;  // 重置 ID，让数据库自动分配
                                sharedDb.Insert(member);
                                totalMembers++;
                            }
                            else
                            {
                                // 已存在，合并数据（保留余额更大的）
                                if (member.Balance > existing.Balance ||
                                    member.BetTotal > existing.BetTotal ||
                                    member.IncomeTotal > existing.IncomeTotal)
                                {
                                    existing.Balance = Math.Max(existing.Balance, member.Balance);
                                    existing.BetTotal = Math.Max(existing.BetTotal, member.BetTotal);
                                    existing.IncomeTotal = Math.Max(existing.IncomeTotal, member.IncomeTotal);
                                    existing.CreditTotal = Math.Max(existing.CreditTotal, member.CreditTotal);
                                    existing.WithdrawTotal = Math.Max(existing.WithdrawTotal, member.WithdrawTotal);
                                    sharedDb.Update(existing);
                                    Log("info", $"   ⚠️ 会员已存在，已合并数据: {member.Nickname} ({member.Wxid})");
                                }
                                skippedMembers++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("error", $"   ❌ 迁移会员失败: {member.Nickname} - {ex.Message}");
                        }
                    }

                    // 4.2 迁移订单数据
                    var orders = wxDb.Table<V2MemberOrder>().ToList();
                    Log("info", $"   订单数据: {orders.Count} 条");

                    foreach (var order in orders)
                    {
                        try
                        {
                            // 检查是否已存在（按 GroupWxId + Wxid + TimeStampBet + AmountTotal）
                            var existing = sharedDb.Table<V2MemberOrder>()
                                .FirstOrDefault(o =>
                                    o.GroupWxId == order.GroupWxId &&
                                    o.Wxid == order.Wxid &&
                                    o.TimeStampBet == order.TimeStampBet &&
                                    Math.Abs(o.AmountTotal - order.AmountTotal) < 0.01);

                            if (existing == null)
                            {
                                order.Id = 0;  // 重置 ID
                                sharedDb.Insert(order);
                                totalOrders++;
                            }
                            else
                            {
                                skippedOrders++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("error", $"   ❌ 迁移订单失败: {order.Id} - {ex.Message}");
                        }
                    }

                    // 4.3 迁移上下分记录
                    var credits = wxDb.Table<V2CreditWithdraw>().ToList();
                    Log("info", $"   上下分记录: {credits.Count} 条");

                    foreach (var credit in credits)
                    {
                        try
                        {
                            var existing = sharedDb.Table<V2CreditWithdraw>()
                                .FirstOrDefault(c =>
                                    c.GroupWxId == credit.GroupWxId &&
                                    c.Wxid == credit.Wxid &&
                                    c.Timestamp == credit.Timestamp);

                            if (existing == null)
                            {
                                credit.Id = 0;
                                sharedDb.Insert(credit);
                                totalCredits++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("error", $"   ❌ 迁移上下分记录失败: {credit.Id} - {ex.Message}");
                        }
                    }

                    // 4.4 迁移余额变动记录
                    var balanceChanges = wxDb.Table<V2BalanceChange>().ToList();
                    Log("info", $"   余额变动记录: {balanceChanges.Count} 条");

                    foreach (var change in balanceChanges)
                    {
                        try
                        {
                            var existing = sharedDb.Table<V2BalanceChange>()
                                .FirstOrDefault(c =>
                                    c.GroupWxId == change.GroupWxId &&
                                    c.Wxid == change.Wxid &&
                                    c.Timestamp == change.Timestamp);

                            if (existing == null)
                            {
                                change.Id = 0;
                                sharedDb.Insert(change);
                                totalBalanceChanges++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("error", $"   ❌ 迁移余额变动记录失败: {change.Id} - {ex.Message}");
                        }
                    }

                    Log("info", $"✅ 完成迁移: {Path.GetFileName(wxDbFile)}");
                }

                // 5. 迁移完成
                Log("info", "\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Log("info", "✅ 数据库迁移完成！");
                Log("info", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Log("info", $"📊 迁移统计：");
                Log("info", $"   会员数据: {totalMembers} 条（跳过 {skippedMembers} 条重复）");
                Log("info", $"   订单数据: {totalOrders} 条（跳过 {skippedOrders} 条重复）");
                Log("info", $"   上下分记录: {totalCredits} 条");
                Log("info", $"   余额变动记录: {totalBalanceChanges} 条");
                Log("info", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                // 6. 备份旧数据库
                string backupDir = Path.Combine(dataDirectory, "backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                Directory.CreateDirectory(backupDir);

                foreach (var wxDbFile in wxDbFiles)
                {
                    string backupPath = Path.Combine(backupDir, Path.GetFileName(wxDbFile));
                    File.Copy(wxDbFile, backupPath, overwrite: true);
                    Log("info", $"📦 已备份: {Path.GetFileName(wxDbFile)} → {backupPath}");
                }

                Log("info", $"✅ 旧数据库已备份到: {backupDir}");
                Log("info", "💡 提示：验证迁移成功后，可以手动删除旧数据库文件");

                return (true, $"迁移成功：会员 {totalMembers} 条，订单 {totalOrders} 条，上下分 {totalCredits} 条，余额变动 {totalBalanceChanges} 条");
            }
            catch (Exception ex)
            {
                string error = $"迁移失败: {ex.Message}";
                Log("error", error);
                return (false, error);
            }
        }

        /// <summary>
        /// 🔥 验证迁移结果
        /// </summary>
        public (bool success, string message) VerifyMigration()
        {
            try
            {
                var dataDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "BaiShengVx3Plus",
                    "Data");

                string sharedDbPath = Path.Combine(dataDirectory, "business.db");
                if (!File.Exists(sharedDbPath))
                {
                    return (false, "共享数据库不存在");
                }

                using var sharedDb = new SQLiteConnection(sharedDbPath);

                int memberCount = sharedDb.Table<V2Member>().Count();
                int orderCount = sharedDb.Table<V2MemberOrder>().Count();
                int creditCount = sharedDb.Table<V2CreditWithdraw>().Count();
                int balanceChangeCount = sharedDb.Table<V2BalanceChange>().Count();

                string result = $"共享数据库数据统计：\n" +
                               $"会员: {memberCount} 条\n" +
                               $"订单: {orderCount} 条\n" +
                               $"上下分: {creditCount} 条\n" +
                               $"余额变动: {balanceChangeCount} 条";

                Log("info", result);
                return (true, result);
            }
            catch (Exception ex)
            {
                return (false, $"验证失败: {ex.Message}");
            }
        }
    }
}

