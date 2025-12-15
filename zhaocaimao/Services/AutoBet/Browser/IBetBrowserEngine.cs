using System;
using System.Threading.Tasks;
using zhaocaimao.Models.AutoBet;
using Unit.Shared.Models;

namespace zhaocaimao.Services.AutoBet.Browser
{
    /// <summary>
    /// 浏览器引擎接口 - 封装 WebView2 和平台脚本的核心功能
    /// 复用 BsBrowserClient 的逻辑，但不包含 Form UI
    /// </summary>
    public interface IBetBrowserEngine
    {
        /// <summary>
        /// 初始化浏览器
        /// </summary>
        Task InitializeAsync(int configId, string configName, string platform, string platformUrl);
        
        /// <summary>
        /// 执行命令（复用 BsBrowserClient 的命令处理逻辑）
        /// </summary>
        Task<BetResult> ExecuteCommandAsync(string command, object? data = null);
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 日志事件
        /// </summary>
        event Action<string>? OnLog;
    }
}

