using System;
using YongLiSystem.Models.Games.Bingo;

namespace YongLiSystem.Models.Games.Bingo.Events
{
    /// <summary>
    /// Bingo 开奖倒计时事件参数
    /// </summary>
    public class BingoLotteryCountdownEventArgs : EventArgs
    {
        /// <summary>
        /// 剩余秒数
        /// </summary>
        public int RemainingSeconds { get; }

        /// <summary>
        /// 当前期号
        /// </summary>
        public int IssueId { get; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public LotteryStatus CurrentStatus { get; }

        public BingoLotteryCountdownEventArgs(int remainingSeconds, int issueId, LotteryStatus currentStatus)
        {
            RemainingSeconds = remainingSeconds;
            IssueId = issueId;
            CurrentStatus = currentStatus;
        }
    }
}

