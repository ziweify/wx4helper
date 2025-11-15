using BaiShengVx3Plus.Shared.Models;
using BsBrowserClient.Models;  // 🔥 OddsInfo 在 BsBrowserClient.Models 中
using BsBrowserClient.Services;
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
        Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BaiShengVx3Plus.Shared.Models.BetStandardOrderList orders);
        
        /// <summary>
        /// 处理响应（拦截到的数据）
        /// </summary>
        void HandleResponse(ResponseEventArgs response);
        
        /// <summary>
        /// 获取赔率列表
        /// </summary>
        System.Collections.Generic.List<BsBrowserClient.Models.OddsInfo> GetOddsList();
    }
}
