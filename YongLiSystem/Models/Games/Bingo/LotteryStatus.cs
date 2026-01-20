namespace YongLiSystem.Models.Games.Bingo
{
    /// <summary>
    /// 开奖状态枚举
    /// </summary>
    public enum LotteryStatus
    {
        /// <summary>
        /// 等待中（系统初始化或停止）
        /// </summary>
        等待中 = 0,

        /// <summary>
        /// 开盘中（可以接受下注）
        /// </summary>
        开盘中 = 1,

        /// <summary>
        /// 即将封盘（倒计时小于设定秒数）
        /// </summary>
        即将封盘 = 2,

        /// <summary>
        /// 封盘中（停止接受下注，等待开奖）
        /// </summary>
        封盘中 = 3,

        /// <summary>
        /// 开奖中（开奖数据处理中）
        /// </summary>
        开奖中 = 4
    }
}

