using System;

namespace 永利系统.Models.Games.Bingo.Exceptions
{
    /// <summary>
    /// Bingo 游戏服务异常类
    /// </summary>
    public class BingoGameException : Exception
    {
        /// <summary>
        /// 当前期号（如果有）
        /// </summary>
        public int? IssueId { get; }

        /// <summary>
        /// 当前状态（如果有）
        /// </summary>
        public string? CurrentStatus { get; }

        public BingoGameException(string message) : base(message)
        {
        }

        public BingoGameException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public BingoGameException(string message, int issueId) : base(message)
        {
            IssueId = issueId;
        }

        public BingoGameException(string message, int issueId, string currentStatus) : base(message)
        {
            IssueId = issueId;
            CurrentStatus = currentStatus;
        }

        public BingoGameException(string message, Exception innerException, int issueId) : base(message, innerException)
        {
            IssueId = issueId;
        }
    }
}

