using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using zhaocaimao.Models;

namespace zhaocaimao.Contracts
{
    /// <summary>
    /// 微信应用服务接口（Application Service）
    /// 负责编排业务流程，协调多个领域服务
    /// </summary>
    public interface IWeChatService
    {
        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        ConnectionState CurrentState { get; }

        /// <summary>
        /// 连接并初始化（智能判断是否需要启动/注入微信）
        /// </summary>
        /// <param name="forceRestart">是否强制重新启动/注入（默认 false，会先尝试直接连接）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功</returns>
        Task<bool> ConnectAndInitializeAsync(bool forceRestart = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// 刷新用户信息（带重试机制）
        /// </summary>
        /// <param name="maxRetries">最大重试次数，-1表示无限重试</param>
        /// <param name="retryInterval">重试间隔（毫秒）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="isRunModeDev">是否为开发模式（返回模拟数据）</param>
        /// <returns>用户信息，失败返回null</returns>
        Task<WxUserInfo?> RefreshUserInfoAsync(
            int maxRetries = 10, 
            int retryInterval = 2000, 
            CancellationToken cancellationToken = default,
            bool isRunModeDev = false);

        /// <summary>
        /// 刷新联系人列表（带重试机制和过滤）
        /// </summary>
        /// <param name="maxRetries">最大重试次数（默认1次，不重试）</param>
        /// <param name="retryInterval">重试间隔（毫秒，默认2000ms）</param>
        /// <param name="filterType">过滤类型（默认全部）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>过滤后的联系人列表</returns>
        Task<List<WxContact>> RefreshContactsAsync(
            int maxRetries = 1, 
            int retryInterval = 2000, 
            ContactFilterType filterType = ContactFilterType.全部,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 断开连接
        /// </summary>
        Task DisconnectAsync();
    }

    /// <summary>
    /// 连接状态
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// 未连接
        /// </summary>
        Disconnected,

        /// <summary>
        /// 正在连接（通用）
        /// </summary>
        Connecting,

        /// <summary>
        /// 正在启动微信
        /// </summary>
        LaunchingWeChat,

        /// <summary>
        /// 正在注入DLL
        /// </summary>
        InjectingDll,

        /// <summary>
        /// 正在连接Socket
        /// </summary>
        ConnectingSocket,

        /// <summary>
        /// 正在获取用户信息
        /// </summary>
        FetchingUserInfo,

        /// <summary>
        /// 正在初始化数据库
        /// </summary>
        InitializingDatabase,

        /// <summary>
        /// 正在获取联系人
        /// </summary>
        FetchingContacts,

        /// <summary>
        /// 已连接（完全初始化）
        /// </summary>
        Connected,

        /// <summary>
        /// 连接失败
        /// </summary>
        Failed
    }

    /// <summary>
    /// 连接状态变化事件参数
    /// </summary>
    public class ConnectionStateChangedEventArgs : EventArgs
    {
        public ConnectionState OldState { get; set; }
        public ConnectionState NewState { get; set; }
        public string? Message { get; set; }
        public Exception? Error { get; set; }
    }
}

