using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using zhaocaimao.Models.AutoBet;
using SQLite;

namespace zhaocaimao.Core
{
    /// <summary>
    /// 投注记录 BindingList（遵循 F5BotV2 设计模式）
    /// 
    /// 核心优势：
    /// 1. 零 SQL：Insert/Update/Delete 一行代码
    /// 2. 自动追踪：PropertyChanged 自动保存
    /// 3. 🔥 线程安全：数据库操作带锁保护，UI 更新在 UI 线程执行
    /// 4. 集中管理：所有 BetRecord 的数据库操作都在这里，避免分散
    /// </summary>
    public class BetRecordBindingList : BindingList<BetRecord>
    {
        private readonly SQLiteConnection _db;
        private readonly SynchronizationContext? _syncContext;
        private readonly object _dbLock = new object(); // 🔥 数据库操作锁

        public BetRecordBindingList(SQLiteConnection db)
        {
            _db = db;
            
            // 🔥 捕获 UI 线程的 SynchronizationContext
            _syncContext = SynchronizationContext.Current;
            
            // 🔥 自动建表（零 SQL）
            lock (_dbLock)
            {
                _db.CreateTable<BetRecord>();
            }
        }

        /// <summary>
        /// 重写 InsertItem：添加时自动保存到数据库
        /// 🔥 线程安全：数据库操作立即执行，UI 更新在 UI 线程执行
        /// </summary>
        protected override void InsertItem(int index, BetRecord item)
        {
            // ========================================
            // 🔥 步骤1: 数据库操作（立即执行，保证可靠写入）
            // ========================================
            
            lock (_dbLock) // 🔥 保护数据库操作
            {
                if (item.Id == 0)
                {
                    // 设置时间戳
                    if (item.CreateTime == default)
                    {
                        item.CreateTime = DateTime.Now;
                    }
                    if (item.SendTime == default)
                    {
                        item.SendTime = DateTime.Now;
                    }
                    
                    // 🔥 插入新记录（一行代码）
                    _db.Insert(item);
                    item.Id = (int)_db.ExecuteScalar<long>("SELECT last_insert_rowid()");
                }
            }

            // ========================================
            // 🔥 步骤2: UI 更新（在 UI 线程执行）
            // ========================================
            if (_syncContext != null && SynchronizationContext.Current != _syncContext)
            {
                // 🔥 从非 UI 线程调用，切换到 UI 线程
                _syncContext.Post(_ =>
                {
                    base.InsertItem(index, item);
                    SubscribePropertyChanged(item);
                }, null);
            }
            else
            {
                // 🔥 已在 UI 线程，直接执行
                base.InsertItem(index, item);
                SubscribePropertyChanged(item);
            }
        }
        
        /// <summary>
        /// 订阅属性变化，自动保存到数据库
        /// 🔥 关键：记录修改后立即保存
        /// </summary>
        private void SubscribePropertyChanged(BetRecord item)
        {
            item.PropertyChanged += (s, e) =>
            {
                if (item.Id > 0)
                {
                    try
                    {
                        // 🔥 立即保存到数据库（在当前线程执行）
                        lock (_dbLock) // 🔥 保护数据库操作
                        {
                            item.UpdateTime = DateTime.Now;
                            
                            // 计算耗时
                            if (item.PostStartTime.HasValue && item.PostEndTime.HasValue)
                            {
                                item.DurationMs = (int)(item.PostEndTime.Value - item.PostStartTime.Value).TotalMilliseconds;
                            }
                            
                            _db.Update(item);
                        }
                        
                        // 🔥 线程安全地刷新 UI
                        NotifyItemChanged(item);
                    }
                    catch (Exception ex)
                    {
                        // 🔥 记录错误，但不阻止 UI 更新
                        System.Diagnostics.Debug.WriteLine($"❌ 保存投注记录失败: {ex.Message}");
                    }
                }
            };
        }

        /// <summary>
        /// 重写 RemoveItem：删除时自动从数据库删除
        /// </summary>
        protected override void RemoveItem(int index)
        {
            var item = this[index];
            
            if (item.Id > 0)
            {
                lock (_dbLock) // 🔥 保护数据库操作
                {
                    _db.Delete(item);
                }
            }
            
            base.RemoveItem(index);
        }

        /// <summary>
        /// 从数据库加载所有记录
        /// 🔥 必须在 UI 线程调用
        /// </summary>
        public void LoadFromDatabase(int? configId = null, int? issueId = null, int limit = 100)
        {
            lock (_dbLock) // 🔥 保护数据库读取
            {
                var query = _db.Table<BetRecord>();
                
                if (configId.HasValue)
                {
                    query = query.Where(r => r.ConfigId == configId.Value);
                }
                
                if (issueId.HasValue)
                {
                    query = query.Where(r => r.IssueId == issueId.Value);
                }
                
                var records = query
                    .OrderByDescending(r => r.CreateTime)
                    .Take(limit)
                    .ToList();

                foreach (var record in records)
                {
                    base.InsertItem(Count, record);
                    SubscribePropertyChanged(record);
                }
            }
        }
        
        /// <summary>
        /// 通知指定记录的数据已更新
        /// 🔥 线程安全：触发 UI 刷新
        /// </summary>
        public void NotifyItemChanged(BetRecord record)
        {
            var index = IndexOf(record);
            if (index >= 0)
            {
                if (_syncContext != null && SynchronizationContext.Current != _syncContext)
                {
                    _syncContext.Post(_ => ResetItem(index), null);
                }
                else
                {
                    ResetItem(index);
                }
            }
        }
        
        /// <summary>
        /// 根据 ID 查找记录
        /// 🔥 直接从内存查找，不访问数据库
        /// </summary>
        public BetRecord? GetById(int id)
        {
            return this.FirstOrDefault(r => r.Id == id);
        }
        
        /// <summary>
        /// 获取指定期号的记录
        /// 🔥 直接从内存查找，不访问数据库
        /// </summary>
        public BetRecord[] GetByIssueId(int issueId)
        {
            return this.Where(r => r.IssueId == issueId).ToArray();
        }
        
        /// <summary>
        /// 获取指定配置的记录
        /// 🔥 直接从内存查找，不访问数据库
        /// </summary>
        public BetRecord[] GetByConfigId(int configId)
        {
            return this.Where(r => r.ConfigId == configId).ToArray();
        }
        
        /// <summary>
        /// 检查是否存在待处理的投注（用于防重复）
        /// 🔥 直接从内存查找，不访问数据库
        /// </summary>
        public bool HasPendingBet(int configId, int issueId)
        {
            return this.Any(r => r.ConfigId == configId && 
                                r.IssueId == issueId && 
                                r.Success == null);
        }
    }
}

