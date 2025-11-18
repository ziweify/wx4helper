using System.Collections.Generic;
using System.Threading.Tasks;
using zhaocaimao.Shared.Models;
using BrowserOddsInfo = zhaocaimao.Services.AutoBet.Browser.Models.OddsInfo;
using BrowserResponseEventArgs = zhaocaimao.Services.AutoBet.Browser.Services.ResponseEventArgs;

namespace zhaocaimao.Services.AutoBet.Browser.PlatformScripts
{
    /// <summary>
    /// 平台脚本接口 - 复用 BsBrowserClient 的逻辑
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
        void HandleResponse(BrowserResponseEventArgs response);
        
        /// <summary>
        /// 获取赔率列表
        /// </summary>
        List<BrowserOddsInfo> GetOddsList();
    }
}

