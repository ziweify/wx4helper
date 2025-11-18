using System;
using System.Threading;

namespace zhaocaimao.Services.Database
{
    /// <summary>
    /// 数据库同步锁服务
    /// 
    /// 用途：保护会员表、订单表、余额变动表等联动计算的数据库操作
    /// 原则：
    /// 1. 应用级别的同步，不是表级别的同步
    /// 2. 所有资金相关的表操作使用同一个锁
    /// 3. 锁只保护数据库写入操作，不保护业务逻辑计算
    /// 4. 避免锁定太长时间，只锁定写入数据库数据这里
    /// 
    /// 使用场景：
    /// - 会员余额更新（下单、取消、结算、上下分）
    /// - 订单创建、更新、删除
    /// - 余额变动记录插入
    /// </summary>
    public class DatabaseLockService
    {
        private static readonly object _lock = new object();
        private static DatabaseLockService? _instance;
        
        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static DatabaseLockService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseLockService();
                        }
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 数据库写入锁（用于保护所有资金相关表的写入操作）
        /// </summary>
        private readonly object _writeLock = new object();
        
        private DatabaseLockService()
        {
        }
        
        /// <summary>
        /// 执行受保护的数据库写入操作
        /// 
        /// 使用方式：
        /// DatabaseLockService.Instance.ExecuteWrite(() => {
        ///     _db.Update(member);
        ///     _db.Update(order);
        ///     _db.Insert(balanceChange);
        /// });
        /// </summary>
        public void ExecuteWrite(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            
            lock (_writeLock)
            {
                action();
            }
        }
        
        /// <summary>
        /// 执行受保护的数据库写入操作（带返回值）
        /// </summary>
        public T ExecuteWrite<T>(Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            
            lock (_writeLock)
            {
                return func();
            }
        }
        
        /// <summary>
        /// 尝试获取写入锁（用于需要判断是否可以获得锁的场景）
        /// </summary>
        public bool TryExecuteWrite(Action action, int timeoutMs = 0)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            
            if (timeoutMs <= 0)
            {
                // 立即尝试，不等待
                if (Monitor.TryEnter(_writeLock))
                {
                    try
                    {
                        action();
                        return true;
                    }
                    finally
                    {
                        Monitor.Exit(_writeLock);
                    }
                }
                return false;
            }
            else
            {
                // 等待指定时间
                if (Monitor.TryEnter(_writeLock, timeoutMs))
                {
                    try
                    {
                        action();
                        return true;
                    }
                    finally
                    {
                        Monitor.Exit(_writeLock);
                    }
                }
                return false;
            }
        }
    }
}

