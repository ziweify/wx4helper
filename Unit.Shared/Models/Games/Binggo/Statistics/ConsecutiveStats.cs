namespace Unit.Shared.Models.Games.Binggo.Statistics
{
    /// <summary>
    /// 连续统计（例如：连续2个大、连续3个大等）
    /// </summary>
    public class ConsecutiveStats
    {
        /// <summary>
        /// 位置
        /// </summary>
        public BallPosition Position { get; set; }

        /// <summary>
        /// 玩法类型
        /// </summary>
        public GamePlayType PlayType { get; set; }

        /// <summary>
        /// 结果类型（大/小、单/双等）
        /// </summary>
        public PlayResult ResultType { get; set; }

        /// <summary>
        /// 连续次数（2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 11+）
        /// </summary>
        public int ConsecutiveCount { get; set; }

        /// <summary>
        /// 出现次数
        /// </summary>
        public int OccurrenceCount { get; set; }
    }
}

