namespace Unit.Shared.Models.Games.Binggo.Statistics
{
    /// <summary>
    /// 位置玩法结果（某个位置的某个玩法结果）
    /// </summary>
    public class PositionPlayResult
    {
        /// <summary>
        /// 位置（P1-P5, Sum）
        /// </summary>
        public BallPosition Position { get; set; }

        /// <summary>
        /// 玩法类型
        /// </summary>
        public GamePlayType PlayType { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public PlayResult Result { get; set; }

        public PositionPlayResult(BallPosition position, GamePlayType playType, PlayResult result)
        {
            Position = position;
            PlayType = playType;
            Result = result;
        }
    }
}

