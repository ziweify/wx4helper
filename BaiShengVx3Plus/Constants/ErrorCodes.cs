namespace BaiShengVx3Plus.Constants
{
    /// <summary>
    /// 系统错误代码
    /// 
    /// 🔥 设计原则：
    /// 1. 每个错误都有唯一的错误代码
    /// 2. 方便快速定位问题（搜索错误代码即可找到对应代码位置）
    /// 3. 给用户返回简略信息，详细信息记录到日志
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// 订单相关错误 (100-199)
        /// </summary>
        public static class Order
        {
            /// <summary>
            /// 订单列表未初始化（_ordersBindingList == null）
            /// 位置：BinggoOrderService.CreateOrderAsync
            /// </summary>
            public const string OrderListNotInitialized = "SYS-100";
            
            /// <summary>
            /// 会员列表未初始化（_membersBindingList == null）
            /// 位置：BinggoOrderService.CreateOrderAsync
            /// </summary>
            public const string MemberListNotInitialized = "SYS-101";
            
            /// <summary>
            /// 订单保存失败（Insert/Add 抛异常）
            /// 位置：BinggoOrderService.CreateOrderAsync - 锁内 try-catch
            /// </summary>
            public const string OrderSaveFailed = "SYS-102";
            
            /// <summary>
            /// 订单结算失败
            /// 位置：BinggoOrderService.SettleOrderAsync
            /// </summary>
            public const string OrderSettleFailed = "SYS-103";
            
            /// <summary>
            /// 补单失败
            /// 位置：BinggoOrderService.ManualOrderAsync
            /// </summary>
            public const string ManualOrderFailed = "SYS-104";
        }
        
        /// <summary>
        /// 上下分相关错误 (200-299)
        /// </summary>
        public static class CreditWithdraw
        {
            /// <summary>
            /// 上下分处理失败（通用）
            /// 位置：CreditWithdrawService.ProcessCreditWithdraw
            /// </summary>
            public const string ProcessFailed = "SYS-200";
            
            /// <summary>
            /// 余额不足
            /// 位置：CreditWithdrawService.ProcessCreditWithdraw - 下分
            /// </summary>
            public const string InsufficientBalance = "SYS-201";
            
            /// <summary>
            /// 数据库事务失败
            /// 位置：CreditWithdrawService.ProcessCreditWithdraw - 数据库操作
            /// </summary>
            public const string DatabaseTransactionFailed = "SYS-202";
            
            /// <summary>
            /// 申请已被处理
            /// 位置：CreditWithdrawService.ProcessCreditWithdraw - 验证
            /// </summary>
            public const string AlreadyProcessed = "SYS-203";
            
            /// <summary>
            /// 会员不存在
            /// 位置：CreditWithdrawService/AdminCommandHandler
            /// </summary>
            public const string MemberNotFound = "SYS-204";
        }
        
        /// <summary>
        /// 彩票服务相关错误 (300-399)
        /// </summary>
        public static class Lottery
        {
            /// <summary>
            /// 开奖数据获取失败
            /// 位置：BinggoLotteryService.LoadLotteryDataAsync
            /// </summary>
            public const string LotteryDataLoadFailed = "SYS-300";
            
            /// <summary>
            /// 结算处理失败
            /// 位置：BinggoLotteryService.ProcessLotteryAsync
            /// </summary>
            public const string SettlementFailed = "SYS-301";
        }
        
        /// <summary>
        /// 数据库相关错误 (400-499)
        /// </summary>
        public static class Database
        {
            /// <summary>
            /// 数据库连接失败
            /// </summary>
            public const string ConnectionFailed = "SYS-400";
            
            /// <summary>
            /// 数据保存失败
            /// </summary>
            public const string SaveFailed = "SYS-401";
        }
        
        /// <summary>
        /// 网络相关错误 (500-599)
        /// </summary>
        public static class Network
        {
            /// <summary>
            /// 微信消息发送失败
            /// </summary>
            public const string WeChatSendFailed = "SYS-500";
            
            /// <summary>
            /// API 请求失败
            /// </summary>
            public const string ApiRequestFailed = "SYS-501";
        }
        
        /// <summary>
        /// 格式化错误消息（给用户）
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <returns>用户友好的错误消息</returns>
        public static string FormatUserMessage(string errorCode)
        {
            return $"系统错误，请稍后重试 [{errorCode}]";
        }
    }
}

