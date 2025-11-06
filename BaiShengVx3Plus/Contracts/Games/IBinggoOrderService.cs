using BaiShengVx3Plus.Core;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Models.Games.Binggo;
using SQLite;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Contracts.Games
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
    }
}
