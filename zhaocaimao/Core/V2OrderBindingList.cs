using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using zhaocaimao.Models;
using SQLite;

namespace zhaocaimao.Core
{
    /// <summary>
    /// 订单 BindingList（精简版，使用 ORM 自动增删改）
    /// 继承自 BindingList，自动处理数据库操作
    /// 
    /// 核心优势：
    /// 1. 零 SQL：Insert/Update/Delete 一行代码
    /// 2. 自动追踪：PropertyChanged 自动保存
    /// 3. 🔥 线程安全：数据库操作立即执行，UI 更新在 UI 线程执行
    /// </summary>
    public class V2OrderBindingList : BindingList<V2MemberOrder>
    {
        private readonly SQLiteConnection _db;
        private readonly SynchronizationContext? _syncContext;

        public V2OrderBindingList(SQLiteConnection db)
        {
            _db = db;
            
            // 🔥 捕获 UI 线程的 SynchronizationContext
            _syncContext = SynchronizationContext.Current;
            
            // 🔥 自动建表（零 SQL）
            _db.CreateTable<V2MemberOrder>();
        }

        /// <summary>
        /// 重写 InsertItem：添加时自动保存到数据库
        /// 🔥 线程安全：数据库操作立即执行，UI 更新在 UI 线程执行
        /// 🔥 新订单插入到列表顶部（索引0），保持与 LoadFromDatabase 一致（最新在上）
        /// </summary>
        protected override void InsertItem(int index, V2MemberOrder item)
        {
            // ========================================
            // 🔥 步骤1: 数据库操作（在当前线程立即执行，保证可靠写入）
            // ========================================
            if (item.Id == 0)
            {
                // 🔥 插入新记录（一行代码）
                _db.Insert(item);
                item.Id = _db.ExecuteScalar<long>("SELECT last_insert_rowid()");
            }

            // ========================================
            // 🔥 步骤2: UI 更新（在 UI 线程执行）
            // 🔥 强制将新订单插入到顶部（索引0），保持"最新在上"的一致性
            // ========================================
            if (_syncContext != null && SynchronizationContext.Current != _syncContext)
            {
                // 🔥 从非 UI 线程调用，切换到 UI 线程
                _syncContext.Post(_ =>
                {
                    base.InsertItem(0, item);  // 🔥 插入到顶部
                    SubscribePropertyChanged(item);
                }, null);
            }
            else
            {
                // 🔥 已在 UI 线程，直接执行
                base.InsertItem(0, item);  // 🔥 插入到顶部
                SubscribePropertyChanged(item);
            }
        }
        
        /// <summary>
        /// 订阅属性变化，自动保存到数据库
        /// 🔥 线程安全：数据库更新立即执行，UI 刷新在 UI 线程执行
        /// </summary>
        private void SubscribePropertyChanged(V2MemberOrder item)
        {
            item.PropertyChanged += (s, e) =>
            {
                if (item.Id > 0)
                {
                    // 🔥 立即保存到数据库（在当前线程执行）
                    _db.Update(item);
                    
                    // 🔥 线程安全地刷新 UI
                    NotifyItemChanged(item);
                }
            };
        }
        
        /// <summary>
        /// 通知指定订单的数据已更新
        /// 🔥 线程安全：触发 UI 刷新
        /// </summary>
        private void NotifyItemChanged(V2MemberOrder order)
        {
            var index = IndexOf(order);
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
        /// 重写 RemoveItem：删除时自动从数据库删除
        /// </summary>
        protected override void RemoveItem(int index)
        {
            var item = this[index];
            
            if (item.Id > 0)
            {
                _db.Delete(item);  // 🔥 自动删除（一行代码）
            }
            
            base.RemoveItem(index);
        }

        /// <summary>
        /// 从数据库加载所有订单
        /// 🔥 必须在 UI 线程调用
        /// </summary>
        public void LoadFromDatabase()
        {
            var orders = _db.Table<V2MemberOrder>()
                .OrderByDescending(o => o.TimeStampBet)
                .ToList();

            foreach (var order in orders)
            {
                base.InsertItem(Count, order);
                SubscribePropertyChanged(order);
            }
        }
    }
}

