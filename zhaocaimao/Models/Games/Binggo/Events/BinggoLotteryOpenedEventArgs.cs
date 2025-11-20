using System;

namespace zhaocaimao.Models.Games.Binggo.Events
{
    /// <summary>
    /// 炳狗开奖数据到达事件参数
    /// </summary>
    public class BinggoLotteryOpenedEventArgs : EventArgs
    {
        /// <summary>
        /// 开奖数据
        /// </summary>
        public BinggoLotteryData LotteryData { get; set; } = null!;
    }
}

