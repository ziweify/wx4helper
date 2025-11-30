using System;

namespace zhaocaimao.Core
{
    /// <summary>
    /// 全局资源锁 - 确保所有涉及资金和关键资源的操作使用同一个锁
    /// 
    /// 🔥 设计原则：
    /// 1. 所有修改会员余额的操作必须使用 MemberBalanceLock
    /// 2. 所有订单限额验证必须使用 OrderLimitCheckLock  
    /// 3. 避免死锁：按顺序获取锁（先 OrderLimitCheckLock，后 MemberBalanceLock）
    /// 
    /// 🔥 使用场景：
    /// - 下注扣款（BinggoOrderService）
    /// - 上下分（CreditWithdrawService）
    /// - 结算返还（BinggoOrderService）
    /// - 管理员操作（AdminCommandHandler）
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
        /// 
        /// 🔥 使用此锁的场景：
        /// 1. 下注扣款（member.Balance -= amount）
        /// 2. 上分（member.Balance += amount）
        /// 3. 下分（member.Balance -= amount）
        /// 4. 结算返还（member.Balance += profit）
        /// 5. 管理员直接修改余额
        /// 
        /// 🔥 锁的范围：
        /// lock (ResourceLocks.MemberBalanceLock)
        /// {
        ///     // 读取余额
        ///     // 修改余额
        ///     // 保存数据
        /// }
        /// 
        /// 🔥 注意事项：
        /// - 锁的范围要尽可能小，只锁写入数据的部分
        /// - 不要在锁内执行耗时操作（网络请求、文件读写等）
        /// - 不要在锁内调用可能阻塞的方法
        /// </summary>
        public static readonly object MemberBalanceLock = new object();
        
        /// <summary>
        /// 订单限额检查锁 - 保护订单限额验证的原子性
        /// 
        /// 🔥 使用此锁的场景：
        /// lock (ResourceLocks.OrderLimitCheckLock)
        /// {
        ///     // 查询当期累计金额
        ///     // 验证限额
        ///     // 创建订单对象
        /// }
        /// 
        /// 🔥 为什么需要？
        /// 防止并发验证导致的竞态条件：
        /// - 线程A查询累计=19000，验证通过（< 20000）
        /// - 线程B查询累计=19000，验证通过（< 20000）
        /// - 线程A保存订单，累计=20000
        /// - 线程B保存订单，累计=21000 ← 超限！
        /// 
        /// 使用锁后：
        /// - 线程A: lock { 查询=19000 → 验证通过 → 创建订单 }
        /// - 线程B: lock { 查询=20000 → 验证失败 → 拒绝 }
        /// </summary>
        public static readonly object OrderLimitCheckLock = new object();
        
        // 🔥 未来扩展：其他全局锁可以在这里添加
        // public static readonly object ConfigurationLock = new object();
        // public static readonly object StatisticsLock = new object();
    }
}

