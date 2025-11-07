using System.Threading.Tasks;
using BsBrowserClient.Models;
//using CefSharp.WinForms;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 平台脚本接口
    /// </summary>
    public interface IPlatformScript
    {
        /// <summary>
        /// 平台名称
        /// </summary>
        string PlatformName { get; }
        
        /// <summary>
        /// 平台 URL
        /// </summary>
        string PlatformUrl { get; }
        
        /// <summary>
        /// 设置浏览器实例
        /// </summary>
        void SetBrowser(object browser); // TODO: ChromiumWebBrowser
        
        /// <summary>
        /// 登录
        /// </summary>
        Task<bool> LoginAsync(string username, string password);
        
        /// <summary>
        /// 获取余额
        /// </summary>
        Task<decimal> GetBalanceAsync();
        
        /// <summary>
        /// 投注
        /// </summary>
        Task<CommandResponse> PlaceBetAsync(BetOrder order);
    }
}

