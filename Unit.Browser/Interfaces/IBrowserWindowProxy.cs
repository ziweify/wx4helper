using System;
using System.Threading.Tasks;
using Unit.Browser.Models;

namespace Unit.Browser.Interfaces
{
    /// <summary>
    /// 浏览器窗口代理接口
    /// </summary>
    public interface IBrowserWindowProxy : IDisposable
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 窗口是否可见
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// 日志事件
        /// </summary>
        event EventHandler<string>? OnLog;

        /// <summary>
        /// 初始化浏览器窗口
        /// </summary>
        Task InitializeAsync(string windowTitle, string initialUrl);

        /// <summary>
        /// 执行命令
        /// </summary>
        Task<BrowserCommandResult> ExecuteCommandAsync(string commandName, object? parameters = null, int timeoutMs = 30000);

        /// <summary>
        /// 显示窗口
        /// </summary>
        void ShowWindow();

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        void HideWindow();

        /// <summary>
        /// 关闭窗口
        /// </summary>
        void CloseWindow();
    }
}

