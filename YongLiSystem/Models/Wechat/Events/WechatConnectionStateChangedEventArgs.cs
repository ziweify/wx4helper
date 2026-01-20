using System;

namespace YongLiSystem.Models.Wechat.Events
{
    /// <summary>
    /// 微信连接状态变更事件参数
    /// </summary>
    public class WechatConnectionStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 旧状态
        /// </summary>
        public WechatConnectionState OldState { get; set; }

        /// <summary>
        /// 新状态
        /// </summary>
        public WechatConnectionState NewState { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string? Message { get; set; }

        public WechatConnectionStateChangedEventArgs(WechatConnectionState oldState, WechatConnectionState newState, string? message = null)
        {
            OldState = oldState;
            NewState = newState;
            Message = message;
        }
    }
}

