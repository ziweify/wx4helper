using BaiShengVx3Plus.Shared.Models;
using BsBrowserClient.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 平台脚本接口
    /// </summary>
    public interface IPlatformScript
    {
        /// <summary>
        /// 登录
        /// </summary>
        Task<bool> LoginAsync(string username, string password);
        
        /// <summary>
        /// 获取余额
        /// </summary>
        Task<decimal> GetBalanceAsync();
        
        /// <summary>
        /// 下注
        /// </summary>
        Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BetStandardOrderList orders);
        
        /// <summary>
        /// 处理响应（拦截到的数据）
        /// </summary>
        void HandleResponse(ResponseEventArgs response);
        
        /// <summary>
        /// 获取赔率列表
        /// </summary>
        System.Collections.Generic.List<OddsInfo> GetOddsList();
        
        /// <summary>
        /// 获取未结算的订单信息
        /// </summary>
        /// <param name="state">订单状态：0=未结算, 1=已结算</param>
        /// <param name="pageNum">页码（从1开始）</param>
        /// <param name="pageCount">每页数量</param>
        /// <param name="beginDate">开始日期（yyyyMMdd格式，如：20251214）</param>
        /// <param name="endDate">结束日期（yyyyMMdd格式，如：20251214）</param>
        /// <returns>(是否成功, 订单列表, 最大记录数, 最大页数, 错误消息)</returns>
        Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
            int state = 0,
            int pageNum = 1,
            int pageCount = 20,
            string? beginDate = null,
            string? endDate = null);
    }
}
