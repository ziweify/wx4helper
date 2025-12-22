using System;

namespace 永利系统.Models.Games.Bingo.Events
{
    /// <summary>
    /// Bingo 游戏警告事件参数（用于30秒/15秒提醒）
    /// </summary>
    public class BingoGameWarningEventArgs : EventArgs
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
        /// 警告消息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 警告类型（30秒或15秒）
        /// </summary>
        public WarningType WarningType { get; }

        public BingoGameWarningEventArgs(int remainingSeconds, int issueId, string message, WarningType warningType)
        {
            RemainingSeconds = remainingSeconds;
            IssueId = issueId;
            Message = message;
            WarningType = warningType;
        }
    }

    /// <summary>
    /// 警告类型枚举
    /// </summary>
    public enum WarningType
    {
        /// <summary>
        /// 30秒警告
        /// </summary>
        Warning30Seconds = 30,

        /// <summary>
        /// 15秒警告
        /// </summary>
        Warning15Seconds = 15
    }
}

