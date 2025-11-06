using System;

namespace BaiShengVx3Plus.Models.Games.Binggo.Events
{
    /// <summary>
    /// 炳狗期号变更事件参数
    /// </summary>
    public class BinggoIssueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 旧期号
        /// </summary>
        public int OldIssueId { get; set; }
        
        /// <summary>
        /// 新期号
        /// </summary>
        public int NewIssueId { get; set; }
        
        /// <summary>
        /// 上期开奖数据
        /// </summary>
        public BinggoLotteryData? LastLotteryData { get; set; }
    }
}

