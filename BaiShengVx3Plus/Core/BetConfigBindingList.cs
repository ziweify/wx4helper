using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using BaiShengVx3Plus.Models.AutoBet;
using SQLite;

namespace BaiShengVx3Plus.Core
{
    /// <summary>
    /// 自动投注配置 BindingList（参考 V2MemberBindingList）
    /// 
    /// 核心优势：
    /// 1. 程序启动时从数据库加载到内存
    /// 2. 平时操作都在内存中进行（不访问数据库）
    /// 3. 配置修改时自动保存到数据库（通过 PropertyChanged 事件）
    /// 4. 监控任务直接从内存读取配置（高性能）
    /// </summary>
    public class BetConfigBindingList : BindingList<BetConfig>
    {
        private readonly SQLiteConnection _db;
        private readonly SynchronizationContext? _syncContext;

        public BetConfigBindingList(SQLiteConnection db)
        {
            _db = db;
            
            // 🔥 捕获 UI 线程的 SynchronizationContext
            _syncContext = SynchronizationContext.Current;
            
            // 🔥 自动建表
            _db.CreateTable<BetConfig>();
        }

        /// <summary>
        /// 重写 InsertItem：添加时自动保存到数据库
        /// </summary>
        protected override void InsertItem(int index, BetConfig item)
        {
            // ========================================
            // 🔥 步骤1: 数据库操作（立即执行，保证可靠写入）
            // ========================================
            
            // 🔥 检查是否已存在（去重）
            var existing = _db.Table<BetConfig>()
                .FirstOrDefault(c => c.Id == item.Id);

            if (existing == null)
            {
                // 🔥 插入新记录
                item.LastUpdateTime = DateTime.Now;
                _db.Insert(item);
                item.Id = (int)_db.ExecuteScalar<long>("SELECT last_insert_rowid()");
            }
            else
            {
                // 🔥 更新现有记录
                item.LastUpdateTime = DateTime.Now;
                _db.Update(item);
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
        /// 🔥 关键：配置修改后立即保存，监控任务读取的是最新数据
        /// </summary>
        private void SubscribePropertyChanged(BetConfig item)
        {
            item.PropertyChanged += (s, e) =>
            {
                if (item.Id > 0)
                {
                    // 🔥 立即保存到数据库（在当前线程执行）
                    item.LastUpdateTime = DateTime.Now;
                    _db.Update(item);
                    
                    // 🔥 线程安全地刷新 UI
                    NotifyItemChanged(item);
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
                _db.Delete(item);
            }
            
            base.RemoveItem(index);
        }

        /// <summary>
        /// 从数据库加载所有配置到内存
        /// 🔥 程序启动时调用一次
        /// </summary>
        public void LoadFromDatabase()
        {
            var configs = _db.Table<BetConfig>()
                .OrderBy(c => c.Id)
                .ToList();

            foreach (var config in configs)
            {
                base.InsertItem(Count, config);
                SubscribePropertyChanged(config);
            }
        }
        
        /// <summary>
        /// 通知指定配置的数据已更新
        /// 🔥 线程安全：触发 UI 刷新
        /// </summary>
        public void NotifyItemChanged(BetConfig config)
        {
            var index = IndexOf(config);
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
        /// 获取默认配置（从内存读取）
        /// </summary>
        public BetConfig? GetDefaultConfig()
        {
            return this.FirstOrDefault(c => c.IsDefault);
        }
        
        /// <summary>
        /// 获取所有启用的配置（从内存读取）
        /// 🔥 监控任务使用此方法，不访问数据库
        /// </summary>
        public BetConfig[] GetEnabledConfigs()
        {
            return this.Where(c => c.IsEnabled).ToArray();
        }
    }
}

