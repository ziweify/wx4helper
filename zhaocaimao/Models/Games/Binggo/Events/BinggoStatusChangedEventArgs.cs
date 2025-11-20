using System;

namespace zhaocaimao.Models.Games.Binggo.Events
{
    /// <summary>
    /// 炳狗状态变更事件参数
    /// </summary>
    public class BinggoStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 旧状态
        /// </summary>
        public BinggoLotteryStatus OldStatus { get; set; }
        
        /// <summary>
        /// 新状态
        /// </summary>
        public BinggoLotteryStatus NewStatus { get; set; }
        
        /// <summary>
        /// 期号
        /// </summary>
        public int IssueId { get; set; }
        
        /// <summary>
        /// 开奖数据 (如果有)
        /// </summary>
        public BinggoLotteryData? Data { get; set; }
        
        /// <summary>
        /// 消息描述
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}

