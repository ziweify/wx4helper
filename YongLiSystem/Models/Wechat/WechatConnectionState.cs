namespace YongLiSystem.Models.Wechat
{
    /// <summary>
    /// 微信连接状态枚举
    /// </summary>
    public enum WechatConnectionState
    {
        /// <summary>
        /// 未连接
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// 连接中
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// 已连接
        /// </summary>
        Connected = 2,

        /// <summary>
        /// 连接失败
        /// </summary>
        Failed = 3
    }
}

