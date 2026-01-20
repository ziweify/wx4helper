using System.Collections.Generic;
using System.Threading.Tasks;
using YongLiSystem.Models.Wechat;
using YongLiSystem.Models.Games.Bingo;

namespace YongLiSystem.Contracts.Wechat
{
    /// <summary>
    /// 订单服务契约
    /// 
    /// 📋 契约说明：
    /// - 前置条件：所有参数必须有效（不为 null，数值在合理范围内）
    /// - 后置条件：操作成功返回 (true, message, order)，失败返回 (false, errorMessage, null)
    /// - 不变式：服务运行期间，订单数据必须一致性（期号、金额、状态等）
    /// 
    /// 核心功能：
    /// 1. 创建订单（微信下注）
    /// 2. 补单（手动创建）
    /// 3. 结算订单（批量+单个）
    /// 4. 查询订单
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 创建订单（从微信消息）
        /// 
        /// 📋 契约：
        /// - 前置条件：member 不为 null，messageContent 不为空，issueId > 0
        /// - 后置条件：成功返回 (true, message, order)，其中 order 不为 null
        /// - 异常：参数无效时抛出 ArgumentException
        /// </summary>
        /// <param name="member">会员（不能为 null）</param>
        /// <param name="messageContent">消息内容（不能为空）</param>
        /// <param name="issueId">期号（必须 > 0）</param>
        /// <param name="currentStatus">当前开奖状态</param>
        /// <returns>(成功标志, 回复消息, 订单对象)</returns>
        /// <exception cref="ArgumentNullException">member 为 null</exception>
        /// <exception cref="ArgumentException">messageContent 为空或 issueId 无效</exception>
        Task<(bool success, string message, Order? order)> CreateOrderAsync(
            Member member,
            string messageContent,
            int issueId,
            LotteryStatus currentStatus);

        /// <summary>
        /// 补单（手动创建订单）
        /// 
        /// 📋 契约：
        /// - 前置条件：member 不为 null，betContent 不为空，issueId > 0，amount > 0
        /// - 后置条件：成功返回 (true, message, order)，其中 order 不为 null
        /// - 异常：参数无效时抛出 ArgumentException
        /// </summary>
        /// <param name="member">会员（不能为 null）</param>
        /// <param name="betContent">投注内容（不能为空）</param>
        /// <param name="issueId">期号（必须 > 0）</param>
        /// <param name="amount">投注金额（必须 > 0）</param>
        /// <returns>(成功标志, 消息, 订单对象)</returns>
        /// <exception cref="ArgumentNullException">member 为 null</exception>
        /// <exception cref="ArgumentException">betContent 为空、issueId 或 amount 无效</exception>
        Task<(bool success, string message, Order? order)> CreateManualOrderAsync(
            Member member,
            string betContent,
            int issueId,
            decimal amount);

        /// <summary>
        /// 结算指定期号的所有订单
        /// 
        /// 📋 契约：
        /// - 前置条件：issueId > 0
        /// - 后置条件：返回 (结算数量 >= 0, 汇总消息)
        /// - 异常：issueId 无效时抛出 ArgumentException
        /// </summary>
        /// <param name="issueId">期号（必须 > 0）</param>
        /// <param name="lotteryData">开奖数据（可为 null，表示未开奖）</param>
        /// <returns>(结算数量, 汇总消息)</returns>
        /// <exception cref="ArgumentException">issueId 无效</exception>
        Task<(int settledCount, string summary)> SettleOrdersAsync(
            int issueId,
            LotteryData? lotteryData);

        /// <summary>
        /// 结算单个订单
        /// 
        /// 📋 契约：
        /// - 前置条件：order 和 lotteryData 都不为 null，且 order.IssueId == lotteryData.IssueId
        /// - 后置条件：订单的 WinAmount 被正确计算并设置
        /// - 异常：参数无效时抛出 ArgumentException
        /// </summary>
        /// <param name="order">订单（不能为 null）</param>
        /// <param name="lotteryData">开奖数据（不能为 null）</param>
        /// <exception cref="ArgumentNullException">参数为 null</exception>
        /// <exception cref="ArgumentException">order.IssueId 与 lotteryData.IssueId 不匹配</exception>
        Task SettleSingleOrderAsync(Order order, LotteryData lotteryData);

        /// <summary>
        /// 获取指定期号的待投注订单
        /// 
        /// 📋 契约：
        /// - 前置条件：issueId > 0
        /// - 后置条件：永不返回 null，最坏情况返回空集合
        /// - 异常：issueId 无效时抛出 ArgumentException
        /// </summary>
        /// <param name="issueId">期号（必须 > 0）</param>
        /// <returns>订单集合（永不为 null）</returns>
        /// <exception cref="ArgumentException">issueId 无效</exception>
        IEnumerable<Order> GetPendingOrdersForIssue(int issueId);

        /// <summary>
        /// 获取指定会员、指定期号的待处理订单
        /// 
        /// 📋 契约：
        /// - 前置条件：wxid 不为空，issueId > 0
        /// - 后置条件：永不返回 null，最坏情况返回空集合
        /// - 异常：参数无效时抛出 ArgumentException
        /// </summary>
        /// <param name="wxid">微信ID（不能为空）</param>
        /// <param name="issueId">期号（必须 > 0）</param>
        /// <returns>订单集合（永不为 null）</returns>
        /// <exception cref="ArgumentException">wxid 为空或 issueId 无效</exception>
        IEnumerable<Order> GetPendingOrdersForMemberAndIssue(string wxid, int issueId);

        /// <summary>
        /// 获取当期指定投注项的累计金额（用于限额验证）
        /// 
        /// 📋 契约：
        /// - 前置条件：issueId > 0，carNumber >= 1 且 <= 10，playType 不为空
        /// - 后置条件：返回值 >= 0
        /// - 异常：参数无效时抛出 ArgumentException
        /// </summary>
        /// <param name="issueId">期号（必须 > 0）</param>
        /// <param name="carNumber">车号（1-10）</param>
        /// <param name="playType">玩法（如"大"、"小"，不能为空）</param>
        /// <returns>累计金额（>= 0）</returns>
        /// <exception cref="ArgumentException">参数无效</exception>
        decimal GetIssueBetAmountByItem(int issueId, int carNumber, string playType);

        /// <summary>
        /// 更新订单
        /// 
        /// 📋 契约：
        /// - 前置条件：order 不为 null 且 order.Id > 0
        /// - 后置条件：订单数据被保存到数据库
        /// - 异常：order 为 null 或 Id 无效时抛出 ArgumentException
        /// </summary>
        /// <param name="order">订单（不能为 null，且 Id > 0）</param>
        /// <exception cref="ArgumentNullException">order 为 null</exception>
        /// <exception cref="ArgumentException">order.Id 无效</exception>
        void UpdateOrder(Order order);
    }
}

