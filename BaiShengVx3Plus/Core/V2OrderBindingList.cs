using System;
using System.ComponentModel;
using System.Linq;
using BaiShengVx3Plus.Models;
using SQLite;

namespace BaiShengVx3Plus.Core
{
    /// <summary>
    /// 订单 BindingList（精简版，使用 ORM 自动增删改）
    /// 继承自 BindingList，自动处理数据库操作
    /// 
    /// 核心优势：
    /// 1. 零 SQL：Insert/Update/Delete 一行代码
    /// 2. 自动追踪：PropertyChanged 自动保存
    /// </summary>
    public class V2OrderBindingList : BindingList<V2MemberOrder>
    {
        private readonly SQLiteConnection _db;

        public V2OrderBindingList(SQLiteConnection db)
        {
            _db = db;
            
            // 🔥 自动建表（零 SQL）
            _db.CreateTable<V2MemberOrder>();
        }

        /// <summary>
        /// 重写 InsertItem：添加时自动保存到数据库
        /// </summary>
        protected override void InsertItem(int index, V2MemberOrder item)
        {
            if (item.Id == 0)
            {
                // 🔥 插入新记录（一行代码）
                _db.Insert(item);
                item.Id = _db.ExecuteScalar<long>("SELECT last_insert_rowid()");
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
        /// 从数据库加载所有订单
        /// </summary>
        public void LoadFromDatabase()
        {
            var orders = _db.Table<V2MemberOrder>()
                .OrderByDescending(o => o.TimeStampBet)
                .ToList();

            foreach (var order in orders)
            {
                base.InsertItem(Count, order);
                
                // 订阅属性变化
                order.PropertyChanged += (s, e) =>
                {
                    if (order.Id > 0)
                    {
                        _db.Update(order);
                    }
                };
            }
        }
    }
}

