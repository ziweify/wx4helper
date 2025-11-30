using System;

namespace zhaocaimao.Core
{
    /// <summary>
    /// 全局资源锁 - 确保所有涉及资金和关键资源的操作使用同一个锁
    /// 
    /// 🔥 设计原则：
    /// 1. 所有修改会员余额的操作必须使用 MemberBalanceLock
    /// 2. 所有订单限额验证必须使用 OrderLimitCheckLock  
    /// 3. 所有 BindingList 更新必须使用 BindingListUpdateLock
    /// 4. 避免死锁：按顺序获取锁（先 OrderLimitCheckLock，后 MemberBalanceLock）
    /// 
    /// 🔥 使用场景：
    /// - 下注扣款（BinggoOrderService）
    /// - 上下分（CreditWithdrawService）
    /// - 结算返还（BinggoOrderService）
    /// - 管理员操作（AdminCommandHandler）
    /// - 刷新/绑定群（GroupBindingService）
    /// 
    /// 🔥 为什么需要全局锁？
    /// 问题：不同类中的 static readonly object _memberBalanceLock 是独立的对象
    /// 结果：BinggoOrderService._memberBalanceLock != CreditWithdrawService._memberBalanceLock
    /// 影响：无法互相保护，可能导致余额错误（丢失更新）
    /// 
    /// 解决：创建全局锁管理类，所有服务使用同一个锁对象
    /// </summary>
    public static class ResourceLocks
    {
        /// <summary>
        /// 会员余额锁 - 保护所有会员余额的修改操作
        /// </summary>
        public static readonly object MemberBalanceLock = new object();
        
        /// <summary>
        /// 订单限额检查锁 - 保护订单限额验证的原子性
        /// </summary>
        public static readonly object OrderLimitCheckLock = new object();
        
        /// <summary>
        /// BindingList 更新锁 - 保护 BindingList 的 Clear/Add 操作和读取操作
        /// 
        /// 🔥 使用此锁的场景：
        /// 1. 刷新/绑定群时的 Clear() + Add() 操作
        /// 2. GetMemberByWxid() 读取会员数据
        /// 
        /// 🔥 为什么需要？
        /// 问题：绑定群时会 Clear() 清空列表，然后 Add() 新数据
        /// 风险：在 Clear() 和 Add() 之间，GetMemberByWxid() 会返回 null
        /// 更严重：即使在 Clear() 前获取了 member，Clear+Add 后这个引用失效！
        /// 
        /// 场景模拟：
        /// T1: [消息线程] member = GetMemberByWxid(wxid);  // 获取旧对象
        /// T2: [UI线程] Clear();  // 清空
        /// T3: [UI线程] Add(new V2Member);  // 添加新对象
        /// T4: [消息线程] member.Balance -= 500;  // 💥 修改的是旧对象！
        /// T5: [消息线程] _ordersBindingList.Insert(order);  // 订单保存到新列表
        /// 结果：订单已保存，但余额没扣（修改的是旧对象）！
        /// 
        /// 使用锁后：
        /// lock (BindingListUpdateLock)
        /// {
        ///     Clear();
        ///     Add(...);
        /// }
        /// 
        /// lock (BindingListUpdateLock)
        /// {
        ///     member = GetMemberByWxid(wxid);  // 保证读取的是最新对象
        /// }
        /// </summary>
        public static readonly object BindingListUpdateLock = new object();
    }
}
