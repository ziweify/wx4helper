using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using zhaocaimao.Models;
using zhaocaimao.Shared.Helpers;
using SQLite;

namespace zhaocaimao.Core
{
    /// <summary>
    /// 上下分申请 BindingList（与 V2MemberBindingList、V2OrderBindingList 相同模式）
    /// 继承自 BindingList，自动处理数据库操作
    /// 
    /// 核心优势：
    /// 1. 零 SQL：Insert/Update/Delete 一行代码
    /// 2. 自动追踪：PropertyChanged 自动保存
    /// 3. 🔥 线程安全：数据库操作立即执行，UI 更新在 UI 线程执行
    /// 4. 统一模式：与会员表、订单表保持一致
    /// </summary>
    public class V2CreditWithdrawBindingList : BindingList<V2CreditWithdraw>
    {
        private readonly SQLiteConnection _db;
        private readonly SynchronizationContext? _syncContext;

        public V2CreditWithdrawBindingList(SQLiteConnection db)
        {
            _db = db;
            
            // 🔥 捕获 UI 线程的 SynchronizationContext
            _syncContext = SynchronizationContext.Current;
            
            // 🔥 自动建表（零 SQL）
            _db.CreateTable<V2CreditWithdraw>();
        }

        /// <summary>
        /// 重写 InsertItem：添加时自动保存到数据库
        /// 🔥 线程安全：数据库操作立即执行，UI 更新在 UI 线程执行
        /// 🔧 修复循环引用：避免重复订阅，正确处理线程切换
        /// </summary>
        protected override void InsertItem(int index, V2CreditWithdraw item)
        {
            // ========================================
            // 🔥 步骤1: 数据库操作（在当前线程立即执行，保证可靠写入）
            // ========================================
            
            // 🔥 插入到数据库（如果 Id == 0）
            if (item.Id == 0)
            {
                _db.Insert(item);
                item.Id = _db.ExecuteScalar<long>("SELECT last_insert_rowid()");
            }

            // ========================================
            // 🔥 步骤2: UI 更新（在 UI 线程执行）
            // ========================================
            
            if (_syncContext != null && SynchronizationContext.Current != _syncContext)
            {
                // 如果不在 UI 线程，切换到 UI 线程
                // 🔧 修复：使用 Send 而不是 Post，确保操作同步完成，避免竞态条件
                _syncContext.Send(_ =>
                {
                    // 🔧 修复：只在 UI 线程订阅一次，避免重复订阅
                    SubscribePropertyChanged(item);
                    // 🔧 修复：插入到顶部（index 0），保持一致性
                    base.InsertItem(0, item);
                }, null);
            }
            else
            {
                // 如果已在 UI 线程，直接插入
                // 🔧 修复：在 UI 线程订阅（只订阅一次）
                SubscribePropertyChanged(item);
                base.InsertItem(0, item);  // 🔥 插入到顶部（最新在上）
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
        /// 从数据库加载上下分申请
        /// 🔥 必须在 UI 线程调用
        /// 🔥 只加载当日的数据（参考用户需求）
        /// </summary>
        public void LoadFromDatabase(string? groupWxid = null)
        {
            // 🔥 先清空现有数据（避免重复加载）
            while (Count > 0)
            {
                base.RemoveItem(0);
            }
            
            // 🔥 计算今日0点的时间戳
            var todayStart = DateTime.Now.Date;
            var todayStartTimestamp = TimestampHelper.ConvertDateTimeInt(todayStart);
            
            var query = _db.Table<V2CreditWithdraw>()
                .Where(c => c.Timestamp >= todayStartTimestamp)  // 🔥 只加载当日数据
                .OrderByDescending(c => c.Timestamp);
            
            // 如果指定了群ID，只加载该群的数据
            var creditWithdraws = string.IsNullOrEmpty(groupWxid)
                ? query.ToList()
                : query.Where(c => c.GroupWxId == groupWxid).ToList();

            foreach (var item in creditWithdraws)
            {
                base.InsertItem(Count, item);
                SubscribePropertyChanged(item);
            }
        }
        
        /// <summary>
        /// 🔥 删除48小时之前的数据（参考 F5BotV2 XMainView.cs Line 847-849）
        /// </summary>
        public void DeleteOldRecords(int hoursBefore = 48)
        {
            try
            {
                // 🔥 计算48小时之前的时间戳（参考 F5BotV2）
                var timestampDeleteBefore = TimestampHelper.ConvertDateTimeInt(
                    DateTime.Now.AddHours(-hoursBefore));
                
                // 🔥 删除48小时之前的所有记录
                var deletedCount = _db.Execute(
                    "DELETE FROM V2CreditWithdraw WHERE Timestamp <= ?", 
                    timestampDeleteBefore);
                
                System.Diagnostics.Debug.WriteLine($"✅ 已删除 {deletedCount} 条48小时之前的上下分记录");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 删除旧记录失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 订阅属性变化事件，自动保存到数据库
        /// 🔥 核心功能：属性改变 → 自动保存
        /// </summary>
        private void SubscribePropertyChanged(V2CreditWithdraw item)
        {
            if (item is INotifyPropertyChanged notifyItem)
            {
                notifyItem.PropertyChanged -= OnItemPropertyChanged;
                notifyItem.PropertyChanged += OnItemPropertyChanged;
            }
        }

        /// <summary>
        /// 属性变化时自动更新数据库
        /// </summary>
        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is V2CreditWithdraw item && item.Id > 0)
            {
                try
                {
                    _db.Update(item);  // 🔥 自动更新（一行代码）
                }
                catch (Exception ex)
                {
                    // 日志记录（如果需要）
                    System.Diagnostics.Debug.WriteLine($"上下分记录更新失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 更新会员的上下分统计
        /// 🔥 统一计算逻辑：从已同意的记录中统计
        /// </summary>
        public void UpdateMemberStatistics(V2MemberBindingList membersBindingList)
        {
            try
            {
                // 🔥 1. 今日日期
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // 🔥 2. 获取所有已同意的记录
                var approvedRecords = this.Where(c => c.Status == CreditWithdrawStatus.已同意).ToList();

                // 🔥 3. 按会员分组统计
                var memberStats = approvedRecords
                    .GroupBy(c => c.Wxid)
                    .Select(g => new
                    {
                        Wxid = g.Key,
                        CreditTotal = g.Where(c => c.Action == CreditWithdrawAction.上分).Sum(c => c.Amount),
                        WithdrawTotal = g.Where(c => c.Action == CreditWithdrawAction.下分).Sum(c => c.Amount),
                        CreditToday = g.Where(c => c.Action == CreditWithdrawAction.上分 && c.TimeString.StartsWith(today)).Sum(c => c.Amount),
                        WithdrawToday = g.Where(c => c.Action == CreditWithdrawAction.下分 && c.TimeString.StartsWith(today)).Sum(c => c.Amount)
                    })
                    .ToList();

                // 🔥 4. 更新会员统计
                foreach (var stat in memberStats)
                {
                    var member = membersBindingList.FirstOrDefault(m => m.Wxid == stat.Wxid);
                    if (member != null)
                    {
                        member.CreditTotal = stat.CreditTotal;
                        member.WithdrawTotal = stat.WithdrawTotal;
                        member.CreditToday = stat.CreditToday;
                        member.WithdrawToday = stat.WithdrawToday;
                        
                        // 会员对象的 PropertyChanged 会触发自动保存到数据库
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新会员上下分统计失败: {ex.Message}");
            }
        }
    }
}
