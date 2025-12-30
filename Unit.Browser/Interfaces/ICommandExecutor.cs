using System.Threading.Tasks;
using Unit.Browser.Models;
using Microsoft.Web.WebView2.WinForms;

namespace Unit.Browser.Interfaces
{
    /// <summary>
    /// 命令执行器接口
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>
        /// 设置 WebView2 实例
        /// </summary>
        void SetWebView(WebView2 webView);

        /// <summary>
        /// 执行命令
        /// </summary>
        Task<BrowserCommandResult> ExecuteAsync(BrowserCommand command);

        /// <summary>
        /// 是否支持指定命令
        /// </summary>
        bool SupportsCommand(string commandName);
    }
}

