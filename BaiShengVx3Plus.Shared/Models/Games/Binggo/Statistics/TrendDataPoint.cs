using System;

namespace BaiShengVx3Plus.Shared.Models.Games.Binggo.Statistics
{
    /// <summary>
    /// 走势数据点（用于绘制走势图）
    /// </summary>
    public class TrendDataPoint
    {
        /// <summary>
        /// 时间或期号
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 期号
        /// </summary>
        public int IssueId { get; set; }

        /// <summary>
        /// 大的数量
        /// </summary>
        public int BigCount { get; set; }

        /// <summary>
        /// 小的数量
        /// </summary>
        public int SmallCount { get; set; }

        /// <summary>
        /// 单的数量
        /// </summary>
        public int OddCount { get; set; }

        /// <summary>
        /// 双的数量
        /// </summary>
        public int EvenCount { get; set; }

        /// <summary>
        /// 尾大的数量
        /// </summary>
        public int TailBigCount { get; set; }

        /// <summary>
        /// 尾小的数量
        /// </summary>
        public int TailSmallCount { get; set; }

        /// <summary>
        /// 合单的数量
        /// </summary>
        public int SumOddCount { get; set; }

        /// <summary>
        /// 合双的数量
        /// </summary>
        public int SumEvenCount { get; set; }

        /// <summary>
        /// 龙的数量
        /// </summary>
        public int DragonCount { get; set; }

        /// <summary>
        /// 虎的数量
        /// </summary>
        public int TigerCount { get; set; }
    }
}

