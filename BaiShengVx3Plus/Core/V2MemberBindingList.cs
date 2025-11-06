using System;
using System.ComponentModel;
using System.Linq;
using BaiShengVx3Plus.Models;
using SQLite;

namespace BaiShengVx3Plus.Core
{
    /// <summary>
    /// 会员 BindingList（精简版，使用 ORM 自动增删改）
    /// 继承自 BindingList，自动处理数据库操作
    /// 
    /// 核心优势：
    /// 1. 零 SQL：Insert/Update/Delete 一行代码
    /// 2. 自动追踪：PropertyChanged 自动保存
    /// 3. 自动去重：检查 GroupWxId + Wxid
    /// </summary>
    public class V2MemberBindingList : BindingList<V2Member>
    {
        private readonly SQLiteConnection _db;
        private readonly string _groupWxId;

        public V2MemberBindingList(SQLiteConnection db, string groupWxId)
        {
            _db = db;
            _groupWxId = groupWxId;
            
            // 🔥 自动建表（零 SQL）
            _db.CreateTable<V2Member>();
        }

        /// <summary>
        /// 重写 InsertItem：添加时自动保存到数据库
        /// </summary>
        protected override void InsertItem(int index, V2Member item)
        {
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

            base.InsertItem(index, item);

            // 🔥 订阅属性变化：自动保存（一行代码）
            item.PropertyChanged += (s, e) =>
            {
                if (item.Id > 0)
                {
                    _db.Update(item);  // 🔥 自动更新
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
        /// </summary>
        public void LoadFromDatabase()
        {
            var members = _db.Table<V2Member>()
                .Where(m => m.GroupWxId == _groupWxId)
                .ToList();

            foreach (var member in members)
            {
                base.InsertItem(Count, member);
                
                // 订阅属性变化
                member.PropertyChanged += (s, e) =>
                {
                    if (member.Id > 0)
                    {
                        _db.Update(member);
                    }
                };
            }
        }
    }
}

