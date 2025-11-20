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
    /// 会员 BindingList（精简版，使用 ORM 自动增删改）
    /// 继承自 BindingList，自动处理数据库操作
    /// 
    /// 核心优势：
    /// 1. 零 SQL：Insert/Update/Delete 一行代码
    /// 2. 自动追踪：PropertyChanged 自动保存
    /// 3. 自动去重：检查 GroupWxId + Wxid
    /// 4. 🔥 线程安全：数据库操作立即执行，UI 更新在 UI 线程执行
    /// </summary>
    public class V2MemberBindingList : BindingList<V2Member>
    {
        private readonly SQLiteConnection _db;
        private readonly string _groupWxId;
        private readonly SynchronizationContext? _syncContext;

        public V2MemberBindingList(SQLiteConnection db, string groupWxId)
        {
            _db = db;
            _groupWxId = groupWxId;
            
            // 🔥 捕获 UI 线程的 SynchronizationContext
            _syncContext = SynchronizationContext.Current;
            
            // 🔥 自动建表（零 SQL）
            _db.CreateTable<V2Member>();
        }

        /// <summary>
        /// 重写 InsertItem：添加时自动保存到数据库
        /// 🔥 线程安全：数据库操作立即执行，UI 更新在 UI 线程执行
        /// </summary>
        protected override void InsertItem(int index, V2Member item)
        {
            // ========================================
            // 🔥 步骤1: 数据库操作（在当前线程立即执行，保证可靠写入）
            // ========================================
            
            // 🔥 修复：只在 GroupWxId 为空时才设置，否则保留原值
            // 这样可以支持在同一个数据库中存储多个群的会员
            if (string.IsNullOrEmpty(item.GroupWxId))
            {
                item.GroupWxId = _groupWxId;
            }

            // 🔥 检查是否已存在（去重）
            var existing = _db.Table<V2Member>()
                .FirstOrDefault(m => m.GroupWxId == item.GroupWxId && m.Wxid == item.Wxid);

            if (existing == null)
            {
                // 🔥 插入新记录（一行代码）
                _db.Insert(item);
                item.Id = _db.ExecuteScalar<long>("SELECT last_insert_rowid()");
            }
            else
            {
                // 🔥 更新现有记录（保留业务数据，更新基本信息）
                item.Id = existing.Id;
                item.Balance = existing.Balance;
                item.State = existing.State;
                item.BetCur = existing.BetCur;
                item.BetWait = existing.BetWait;
                item.IncomeToday = existing.IncomeToday;
                item.CreditToday = existing.CreditToday;
                item.BetToday = existing.BetToday;
                item.WithdrawToday = existing.WithdrawToday;
                item.BetTotal = existing.BetTotal;
                item.CreditTotal = existing.CreditTotal;
                item.WithdrawTotal = existing.WithdrawTotal;
                item.IncomeTotal = existing.IncomeTotal;
                
                // 更新基本信息（昵称、备注等）
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
        /// 🔥 线程安全：数据库更新立即执行，UI 刷新在 UI 线程执行
        /// </summary>
        private void SubscribePropertyChanged(V2Member item)
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
        /// 从数据库加载所有会员
        /// 🔥 必须在 UI 线程调用
        /// </summary>
        public void LoadFromDatabase()
        {
            var members = _db.Table<V2Member>()
                .Where(m => m.GroupWxId == _groupWxId)
                .ToList();

            foreach (var member in members)
            {
                base.InsertItem(Count, member);
                SubscribePropertyChanged(member);
            }
        }
        
        /// <summary>
        /// 通知指定会员的数据已更新
        /// 🔥 线程安全：触发 UI 刷新
        /// </summary>
        public void NotifyItemChanged(V2Member member)
        {
            var index = IndexOf(member);
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
    }
}

