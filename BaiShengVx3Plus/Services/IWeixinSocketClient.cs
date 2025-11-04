using System;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Services
{
    /// <summary>
    /// 微信 Socket 客户端接口
    /// </summary>
    public interface IWeixinSocketClient : IDisposable
    {
        /// <summary>
        /// 是否已连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 是否启用自动重连
        /// </summary>
        bool AutoReconnect { get; set; }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        Task<bool> ConnectAsync(string host = "127.0.0.1", int port = 6328, int timeoutMs = 5000);

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 启动自动重连
        /// </summary>
        void StartAutoReconnect(int intervalMs = 5000);

        /// <summary>
        /// 停止自动重连
        /// </summary>
        void StopAutoReconnect();

        /// <summary>
        /// 发送请求并等待响应
        /// </summary>
        Task<TResult?> SendAsync<TResult>(string method, params object[] parameters) where TResult : class;

        /// <summary>
        /// 发送请求并等待响应（带超时）
        /// </summary>
        Task<TResult?> SendAsync<TResult>(string method, int timeoutMs, params object[] parameters) where TResult : class;

        /// <summary>
        /// 服务器主动推送事件
        /// </summary>
        event EventHandler<ServerPushEventArgs>? OnServerPush;
    }

    /// <summary>
    /// 服务器推送事件参数
    /// </summary>
    public class ServerPushEventArgs : EventArgs
    {
        public string Method { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}

