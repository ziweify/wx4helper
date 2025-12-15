namespace Unit.Shared.Models.Games.Binggo
{
    /// <summary>
    /// 开奖号码类
    /// 每个号码包含：大小、单双、尾大小、合单双等属性
    /// </summary>
    public class LotteryNumber
    {
        /// <summary>
        /// 号码值（1-28 或总和）
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 大小
        /// 单个球：>40为大，≤40为小
        /// 总和：>=203为大，<203为小
        /// </summary>
        public SizeType Size { get; set; }

        /// <summary>
        /// 单双
        /// </summary>
        public OddEvenType OddEven { get; set; }

        /// <summary>
        /// 位置（第几球）
        /// </summary>
        public BallPosition Position { get; set; }

        /// <summary>
        /// 尾大小
        /// 尾数0-4为尾小，5-9为尾大
        /// </summary>
        public TailSizeType TailSize { get; set; }

        /// <summary>
        /// 合单双（十位+个位的和）
        /// </summary>
        public SumOddEvenType SumOddEven { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LotteryNumber(BallPosition position, int number)
        {
            Position = position;
            Number = number;

            if (number == 0)
            {
                Size = SizeType.Unknown;
                OddEven = OddEvenType.Unknown;
                TailSize = TailSizeType.Unknown;
                SumOddEven = SumOddEvenType.Unknown;
                return;
            }

            // 单双计算
            OddEven = (number % 2 == 0) ? OddEvenType.Even : OddEvenType.Odd;

            // 大小计算
            if (position == BallPosition.Sum)
            {
                // 总和的大小判断：>=203为大，15-202为小
                if (number >= 15 && number <= 202)
                {
                    Size = SizeType.Small;
                }
                else if (number >= 203 && number <= 390)
                {
                    Size = SizeType.Big;
                }
                else
                {
                    Size = SizeType.Unknown;
                }
            }
            else
            {
                // 单个球的大小判断：>40为大，≤40为小
                Size = (number > 40) ? SizeType.Big : SizeType.Small;
            }

            // 尾大小计算
            int tailDigit = number % 10;  // 尾数
            TailSize = (tailDigit >= 0 && tailDigit <= 4) ? TailSizeType.TailSmall : TailSizeType.TailBig;

            // 合单双计算（十位+个位的和）
            int tenDigit = number / 10;     // 十位
            int digitSum = tenDigit + tailDigit;  // 十位+个位
            SumOddEven = (digitSum % 2 == 0) ? SumOddEvenType.SumEven : SumOddEvenType.SumOdd;

            // 特殊规则：如果是总和，合单双等于单双
            if (position == BallPosition.Sum)
            {
                SumOddEven = (OddEven == OddEvenType.Even) ? SumOddEvenType.SumEven : SumOddEvenType.SumOdd;
            }
        }

        public override string ToString()
        {
            return Number.ToString();
        }

        /// <summary>
        /// 获取大小文本
        /// </summary>
        public string GetSizeText()
        {
            return Size switch
            {
                SizeType.Big => "大",
                SizeType.Small => "小",
                _ => "?"
            };
        }

        /// <summary>
        /// 获取单双文本
        /// </summary>
        public string GetOddEvenText()
        {
            return OddEven switch
            {
                OddEvenType.Odd => "单",
                OddEvenType.Even => "双",
                _ => "?"
            };
        }

        /// <summary>
        /// 获取尾大小文本
        /// </summary>
        public string GetTailSizeText()
        {
            return TailSize switch
            {
                TailSizeType.TailBig => "尾大",
                TailSizeType.TailSmall => "尾小",
                _ => "?"
            };
        }

        /// <summary>
        /// 获取合单双文本
        /// </summary>
        public string GetSumOddEvenText()
        {
            return SumOddEven switch
            {
                SumOddEvenType.SumOdd => "合单",
                SumOddEvenType.SumEven => "合双",
                _ => "?"
            };
        }
    }
}

