using System;

namespace 永利系统.Models.Games.Bingo.Events
{
    /// <summary>
    /// Bingo 期号变更事件参数
    /// </summary>
    public class BingoLotteryIssueChangedEventArgs : EventArgs
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
        /// 上期开奖数据（可选）
        /// </summary>
        public LotteryData? LastLotteryData { get; set; }

        public BingoLotteryIssueChangedEventArgs(int oldIssueId, int newIssueId, LotteryData? lastLotteryData = null)
        {
            OldIssueId = oldIssueId;
            NewIssueId = newIssueId;
            LastLotteryData = lastLotteryData;
        }
    }
}

