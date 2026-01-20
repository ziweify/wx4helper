using System;
using YongLiSystem.Models.Games.Bingo;

namespace YongLiSystem.Models.Games.Bingo.Events
{
    /// <summary>
    /// Bingo 开奖数据到达事件参数
    /// </summary>
    public class BingoLotteryOpenedEventArgs : EventArgs
    {
        /// <summary>
        /// 开奖数据
        /// </summary>
        public LotteryData LotteryData { get; }

        public BingoLotteryOpenedEventArgs(LotteryData lotteryData)
        {
            LotteryData = lotteryData;
        }
    }
}

