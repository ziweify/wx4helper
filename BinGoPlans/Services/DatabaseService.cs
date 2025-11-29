using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaiShengVx3Plus.Shared.Models.Games.Binggo;
using BinGoPlans.Models;
using SQLite;

namespace BinGoPlans.Services
{
    /// <summary>
    /// 数据库服务（添加即保存）
    /// 参考 BaiShengVx3Plus 的 BinggoLotteryDataBindingList
    /// </summary>
    public class DatabaseService
    {
        private readonly SQLiteConnection _db;
        private readonly object _lock = new object();

        public DatabaseService(string dbPath)
        {
            try
            {
            // 确保目录存在
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 打开数据库连接
            _db = new SQLiteConnection(dbPath);

            // 配置数据库为最可靠模式（数据完整性优先，不使用 WAL）
                // 注意：PRAGMA 命令可能会抛出 "not an error" 异常（sqlite-net 已知问题），需要捕获
                // 使用辅助方法安全执行 PRAGMA 命令
                SafeExecutePragma("PRAGMA journal_mode = DELETE");
                SafeExecutePragma("PRAGMA synchronous = FULL");
                SafeExecutePragma("PRAGMA foreign_keys = ON");
                SafeExecutePragma("PRAGMA cache_size = 10000");
                SafeExecutePragma("PRAGMA temp_store = MEMORY");

            // 自动创建表（使用 BinGoDataEntity，它有 SQLite 特性）
            _db.CreateTable<BinGoDataEntity>();
            }
            catch (Exception ex)
            {
                // 如果异常是 "not an error"，忽略它（这是 sqlite-net 的已知问题）
                if (IsNotAnErrorException(ex))
                {
                    // 忽略此异常，继续执行
                    return;
                }
                
                // 其他异常需要重新抛出，因为数据库可能无法正常初始化
                throw new Exception("数据库初始化失败: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 检查异常是否是 "not an error" 异常（sqlite-net 已知问题）
        /// </summary>
        private bool IsNotAnErrorException(Exception ex)
        {
            if (ex == null) return false;
            
            // 检查 HResult（最可靠的方式）
            if (ex.HResult == 0x80131500)
            {
                return true;
            }
            
            // 检查异常消息
            if (ex.Message != null && ex.Message.Contains("not an error", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            // 检查是否是 SQLiteException 且消息包含 "not an error" 或 "SQLITE_OK"
            if (ex is SQLiteException sqliteEx)
            {
                if (sqliteEx.Message != null && 
                    (sqliteEx.Message.Contains("not an error", StringComparison.OrdinalIgnoreCase) || 
                     sqliteEx.Message.Contains("SQLITE_OK", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 安全执行 PRAGMA 命令（忽略 "not an error" 异常）
        /// sqlite-net 已知问题：PRAGMA 命令有时会抛出 "not an error" 异常，即使命令成功执行
        /// </summary>
        private void SafeExecutePragma(string pragmaCommand)
        {
            try
            {
                _db.Execute(pragmaCommand);
            }
            catch (Exception ex)
            {
                // 检查是否是 "not an error" 异常
                if (IsNotAnErrorException(ex))
                {
                    // 忽略此异常，继续执行（命令实际上已成功）
                    return;
                }
                
                // 其他异常记录但不阻止初始化（数据库可能仍然可用）
                System.Diagnostics.Debug.WriteLine($"执行 PRAGMA 命令时发生异常 ({pragmaCommand}): {ex.GetType().Name} - {ex.Message} (HResult: 0x{ex.HResult:X8})");
            }
        }

        /// <summary>
        /// 添加或更新数据（添加即保存）
        /// IssueId 作为主键，如果已存在则更新，否则插入
        /// </summary>
        public void SaveData(BinGoData data)
        {
            if (data == null || !data.IsOpened) return;

            lock (_lock)
            {
                try
                {
                    BinGoDataEntity entity;
                    
                    // 如果已经是 BinGoDataEntity，直接使用；否则转换
                    if (data is BinGoDataEntity entityData)
                    {
                        entity = entityData;
                    }
                    else
                    {
                        entity = BinGoDataEntity.FromBinGoData(data);
                    }
                    
                    // 检查是否已存在
                    var existing = _db.Table<BinGoDataEntity>()
                        .FirstOrDefault(d => d.IssueId == entity.IssueId);

                    if (existing == null)
                    {
                        // 插入新记录
                        _db.Insert(entity);
                    }
                    else
                    {
                        // 更新现有记录
                        _db.Update(entity);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("保存数据失败 (IssueId=" + data.IssueId + "): " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// 批量添加或更新数据（添加即保存）
        /// </summary>
        public void SaveDataRange(IEnumerable<BinGoData> dataList)
        {
            if (dataList == null) return;

            lock (_lock)
            {
                try
                {
                    int savedCount = 0;
                    int updatedCount = 0;

                    foreach (var data in dataList.Where(d => d != null && d.IsOpened))
                    {
                        BinGoDataEntity entity;
                        
                        // 如果已经是 BinGoDataEntity，直接使用；否则转换
                        if (data is BinGoDataEntity entityData)
                        {
                            entity = entityData;
                        }
                        else
                        {
                            entity = BinGoDataEntity.FromBinGoData(data);
                        }
                        
                        var existing = _db.Table<BinGoDataEntity>()
                            .FirstOrDefault(d => d.IssueId == entity.IssueId);

                        if (existing == null)
                        {
                            _db.Insert(entity);
                            savedCount++;
                        }
                        else
                        {
                            _db.Update(entity);
                            updatedCount++;
                        }
                    }

                    // 提交事务（sqlite-net-pcl 自动管理事务）
                }
                catch (Exception ex)
                {
                    throw new Exception("批量保存数据失败: " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// 从数据库加载指定日期的数据
        /// 返回 BinGoDataEntity（继承自 BinGoData），可以直接用于显示和计算
        /// </summary>
        public List<BinGoDataEntity> LoadDataByDate(DateTime date)
        {
            lock (_lock)
            {
                try
                {
                    var startDate = date.Date;
                    var endDate = startDate.AddDays(1);

                    var entityList = _db.Table<BinGoDataEntity>()
                        .Where(d => d.OpenTime >= startDate && d.OpenTime < endDate)
                        .Where(d => !string.IsNullOrEmpty(d.LotteryData))
                        .OrderBy(d => d.IssueId)
                        .ToList();

                    // BinGoDataEntity 继承自 BinGoData，可以直接使用
                    // 从数据库加载后，需要重新解析 LotteryData（因为计算属性没有被保存）
                    foreach (var entity in entityList)
                    {
                        // 重新解析 LotteryData（这会设置所有计算属性）
                        if (!string.IsNullOrEmpty(entity.LotteryData))
                        {
                            // 通过 FillLotteryData 重新解析，这会触发 ParseLotteryData
                            entity.FillLotteryData(entity.IssueId, entity.LotteryData, entity.OpenTime);
                        }
                    }

                    // 过滤已开奖的数据
                    return entityList.Where(d => d.IsOpened).ToList();
                }
                catch (Exception ex)
                {
                    throw new Exception("加载数据失败 (Date=" + date.ToString("yyyy-MM-dd") + "): " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// 从数据库加载所有数据
        /// 返回 BinGoDataEntity（继承自 BinGoData），可以直接用于显示和计算
        /// </summary>
        public List<BinGoDataEntity> LoadAllData()
        {
            lock (_lock)
            {
                try
                {
                    var entityList = _db.Table<BinGoDataEntity>()
                        .Where(d => !string.IsNullOrEmpty(d.LotteryData))
                        .OrderBy(d => d.IssueId)
                        .ToList();

                    // BinGoDataEntity 继承自 BinGoData，可以直接使用
                    // 从数据库加载后，需要重新解析 LotteryData（因为计算属性没有被保存）
                    foreach (var entity in entityList)
                    {
                        // 重新解析 LotteryData（这会设置所有计算属性）
                        if (!string.IsNullOrEmpty(entity.LotteryData))
                        {
                            // 通过 FillLotteryData 重新解析，这会触发 ParseLotteryData
                            entity.FillLotteryData(entity.IssueId, entity.LotteryData, entity.OpenTime);
                        }
                    }

                    // 过滤已开奖的数据
                    return entityList.Where(d => d.IsOpened).ToList();
                }
                catch (Exception ex)
                {
                    throw new Exception("加载所有数据失败: " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// 检查指定日期是否有数据
        /// </summary>
        public bool HasDataForDate(DateTime date)
        {
            lock (_lock)
            {
                try
                {
                    var startDate = date.Date;
                    var endDate = startDate.AddDays(1);

                    var count = _db.Table<BinGoDataEntity>()
                        .Where(d => d.OpenTime >= startDate && d.OpenTime < endDate)
                        .Where(d => !string.IsNullOrEmpty(d.LotteryData))
                        .Count();

                    return count > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void Close()
        {
            lock (_lock)
            {
                _db?.Close();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}
