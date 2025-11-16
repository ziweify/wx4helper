using zhaocaimao.Core;
using zhaocaimao.Models;
using zhaocaimao.Models.Games.Binggo;
using SQLite;
using System.Threading.Tasks;

namespace zhaocaimao.Contracts.Games
{
    public interface IBinggoOrderService
    {
        /// <summary>
        /// 设置数据库连接
        /// </summary>
        void SetDatabase(SQLiteConnection? db);
        
        /// <summary>
        /// 设置订单 BindingList（用于 UI 自动更新）
        /// </summary>
        void SetOrdersBindingList(V2OrderBindingList? bindingList);
        
        /// <summary>
        /// 设置会员 BindingList（用于更新余额）
        /// </summary>
        void SetMembersBindingList(V2MemberBindingList? bindingList);
        
        /// <summary>
        /// 设置统计服务（用于自动更新统计）
        /// </summary>
        void SetStatisticsService(Services.Games.Binggo.BinggoStatisticsService? statisticsService);
        
        /// <summary>
        /// 创建订单（从微信消息）
        /// </summary>
        /// <returns>(成功, 回复消息, 订单对象)</returns>
        Task<(bool success, string message, V2MemberOrder? order)> CreateOrderAsync(
            V2Member member,
            string messageContent,
            int issueId,
            BinggoLotteryStatus currentStatus);
        
        /// <summary>
        /// 补单（手动创建）
        /// </summary>
        /// <returns>(成功, 消息, 订单对象)</returns>
        Task<(bool success, string message, V2MemberOrder? order)> CreateManualOrderAsync(
            V2Member member,
            int issueId,
            string betContent,
            decimal amount);
        
        /// <summary>
        /// 结算指定期号的所有订单
        /// </summary>
        /// <returns>(结算数量, 汇总消息)</returns>
        Task<(int settledCount, string summary)> SettleOrdersAsync(
            int issueId,
            BinggoLotteryData? lotteryData);
        
        /// <summary>
        /// 结算单个订单（用于开奖处理）
        /// </summary>
        Task SettleSingleOrderAsync(V2MemberOrder order, BinggoLotteryData lotteryData);
        
        /// <summary>
        /// 获取指定期号的待投注订单（用于自动投注）
        /// </summary>
        IEnumerable<V2MemberOrder> GetPendingOrdersForIssue(int issueId);
        
        /// <summary>
        /// 获取指定会员、指定期号的待处理订单（用于取消命令）
        /// </summary>
        IEnumerable<V2MemberOrder> GetPendingOrdersForMemberAndIssue(string wxid, int issueId);
        
        /// <summary>
        /// 更新订单（用于投注后更新状态）
        /// </summary>
        void UpdateOrder(V2MemberOrder order);
    }
}
