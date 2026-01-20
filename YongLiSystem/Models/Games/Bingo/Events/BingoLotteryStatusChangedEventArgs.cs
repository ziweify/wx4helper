using System;
using YongLiSystem.Models.Games.Bingo;

namespace YongLiSystem.Models.Games.Bingo.Events
{
    /// <summary>
    /// Bingo 开奖状态变更事件参数
    /// </summary>
    public class BingoLotteryStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 旧状态
        /// </summary>
        public LotteryStatus OldStatus { get; set; }

        /// <summary>
        /// 新状态
        /// </summary>
        public LotteryStatus NewStatus { get; set; }

        /// <summary>
        /// 当前期号
        /// </summary>
        public int IssueId { get; set; }

        /// <summary>
        /// 距离封盘秒数
        /// </summary>
        public int SecondsToSeal { get; set; }

        public BingoLotteryStatusChangedEventArgs(LotteryStatus oldStatus, LotteryStatus newStatus, int issueId, int secondsToSeal)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
            IssueId = issueId;
            SecondsToSeal = secondsToSeal;
        }
    }
}

