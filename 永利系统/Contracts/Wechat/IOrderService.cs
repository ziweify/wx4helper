using System.Collections.Generic;
using System.Threading.Tasks;
using 永利系统.Models.Wechat;
using 永利系统.Models.Games.Bingo;

namespace 永利系统.Contracts.Wechat
{
    /// <summary>
    /// 订单服务接口
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
        /// </summary>
        /// <param name="member">会员</param>
        /// <param name="messageContent">消息内容</param>
        /// <param name="issueId">期号</param>
        /// <param name="currentStatus">当前开奖状态</param>
        /// <returns>(成功, 回复消息, 订单对象)</returns>
        Task<(bool success, string message, Order? order)> CreateOrderAsync(
            Member member,
            string messageContent,
            int issueId,
            LotteryStatus currentStatus);

        /// <summary>
        /// 补单（手动创建订单）
        /// </summary>
        /// <param name="member">会员</param>
        /// <param name="betContent">投注内容</param>
        /// <param name="issueId">期号</param>
        /// <param name="amount">投注金额</param>
        /// <returns>(成功, 消息, 订单对象)</returns>
        Task<(bool success, string message, Order? order)> CreateManualOrderAsync(
            Member member,
            string betContent,
            int issueId,
            decimal amount);

        /// <summary>
        /// 结算指定期号的所有订单
        /// </summary>
        /// <param name="issueId">期号</param>
        /// <param name="lotteryData">开奖数据</param>
        /// <returns>(结算数量, 汇总消息)</returns>
        Task<(int settledCount, string summary)> SettleOrdersAsync(
            int issueId,
            LotteryData? lotteryData);

        /// <summary>
        /// 结算单个订单
        /// </summary>
        /// <param name="order">订单</param>
        /// <param name="lotteryData">开奖数据</param>
        Task SettleSingleOrderAsync(Order order, LotteryData lotteryData);

        /// <summary>
        /// 获取指定期号的待投注订单
        /// </summary>
        /// <param name="issueId">期号</param>
        IEnumerable<Order> GetPendingOrdersForIssue(int issueId);

        /// <summary>
        /// 获取指定会员、指定期号的待处理订单
        /// </summary>
        /// <param name="wxid">微信ID</param>
        /// <param name="issueId">期号</param>
        IEnumerable<Order> GetPendingOrdersForMemberAndIssue(string wxid, int issueId);

        /// <summary>
        /// 获取当期指定投注项的累计金额（用于限额验证）
        /// </summary>
        /// <param name="issueId">期号</param>
        /// <param name="carNumber">车号</param>
        /// <param name="playType">玩法（如"大"、"小"）</param>
        /// <returns>累计金额</returns>
        decimal GetIssueBetAmountByItem(int issueId, int carNumber, string playType);

        /// <summary>
        /// 更新订单
        /// </summary>
        /// <param name="order">订单</param>
        void UpdateOrder(Order order);
    }
}

