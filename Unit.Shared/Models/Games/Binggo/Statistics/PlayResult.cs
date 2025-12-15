namespace Unit.Shared.Models.Games.Binggo.Statistics
{
    /// <summary>
    /// 玩法结果（用于路珠和统计）
    /// </summary>
    public enum PlayResult
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = -1,

        // 大小
        Big = 1,
        Small = 0,

        // 单双
        Odd = 10,
        Even = 11,

        // 尾大小
        TailBig = 20,
        TailSmall = 21,

        // 合单双
        SumOdd = 30,
        SumEven = 31,

        // 龙虎
        Dragon = 40,
        Tiger = 41,
        Draw = 42
    }

    /// <summary>
    /// 玩法结果扩展方法
    /// </summary>
    public static class PlayResultExtensions
    {
        /// <summary>
        /// 获取玩法结果的显示文本
        /// </summary>
        public static string GetDisplayText(this PlayResult result)
        {
            return result switch
            {
                PlayResult.Big => "大",
                PlayResult.Small => "小",
                PlayResult.Odd => "单",
                PlayResult.Even => "双",
                PlayResult.TailBig => "尾大",
                PlayResult.TailSmall => "尾小",
                PlayResult.SumOdd => "合单",
                PlayResult.SumEven => "合双",
                PlayResult.Dragon => "龙",
                PlayResult.Tiger => "虎",
                PlayResult.Draw => "和",
                _ => "?"
            };
        }

        /// <summary>
        /// 获取玩法结果的颜色标识（用于路珠绘制）
        /// 注意：Shared项目不依赖System.Drawing，返回颜色名称字符串
        /// </summary>
        public static string GetColorName(this PlayResult result)
        {
            return result switch
            {
                PlayResult.Big => "Red",
                PlayResult.Small => "Blue",
                PlayResult.Odd => "Red",
                PlayResult.Even => "Blue",
                PlayResult.TailBig => "Red",
                PlayResult.TailSmall => "Blue",
                PlayResult.SumOdd => "Red",
                PlayResult.SumEven => "Blue",
                PlayResult.Dragon => "Red",
                PlayResult.Tiger => "Blue",
                PlayResult.Draw => "Gray",
                _ => "White"
            };
        }
    }
}

