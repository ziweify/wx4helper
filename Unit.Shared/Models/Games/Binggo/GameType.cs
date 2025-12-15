namespace Unit.Shared.Models.Games.Binggo
{
    /// <summary>
    /// 大小枚举
    /// </summary>
    public enum SizeType
    {
        Unknown = -1,
        Small = 0,  // 小
        Big = 1     // 大
    }

    /// <summary>
    /// 单双枚举
    /// </summary>
    public enum OddEvenType
    {
        Unknown = -1,
        Even = 0,   // 双
        Odd = 1     // 单
    }

    /// <summary>
    /// 尾大小枚举
    /// </summary>
    public enum TailSizeType
    {
        Unknown = -1,
        TailSmall = 0,  // 尾小（0-4）
        TailBig = 1     // 尾大（5-9）
    }

    /// <summary>
    /// 合单双枚举（十位+个位的和）
    /// </summary>
    public enum SumOddEvenType
    {
        Unknown = -1,
        SumEven = 0,  // 合双
        SumOdd = 1    // 合单
    }

    /// <summary>
    /// 龙虎枚举
    /// </summary>
    public enum DragonTigerType
    {
        Unknown = -1,
        Tiger = 0,  // 虎（P1 < P5）
        Dragon = 1, // 龙（P1 > P5）
        Draw = 2    // 和（P1 = P5）
    }
}

