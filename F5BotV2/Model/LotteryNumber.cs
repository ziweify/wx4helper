using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model
{

    public interface ILotteryNumber
    {
        int number { get; set; }
        NumberDX dx { get; set; }
        NumberDS ds { get; set; }
        CarNumEnum pos { get; set; }
        NumberWDX wdx { get; set; }
        NumberHDS hds { get; set; }
    }


    /// <summary>
    ///     开奖号码类, 代表每一个号码
    /// </summary>
    public class LotteryNumber
        : ILotteryNumber
    {
        public int number { get; set; }

        /// <summary>
        ///     大小
        /// </summary>
        public NumberDX dx { get; set; }

        public NumberDS ds { get; set; }

        public CarNumEnum pos { get; set; }

        public NumberWDX wdx { get; set; } //尾大小
        public NumberHDS hds { get; set; } //合单双


        // 辅助分析数据。通过函数写入,不提供直接修改
        // 都是插入数据后的统计数据
        //private num数量数据 _数量数据;
        //public num数量数据 数量数据 { get; set; }
        //public Lz大小 大小数据 { get; set; }
        //public Lz尾大小 尾大小数据 { get; set; }
        //public Lz单双 单双数据 { get; set; }
        //public Lz和值单双 和值单双数据 { get; set; }

        //辅助数据结束




        public LotteryNumber(CarNumEnum pos, int number)
        {
            this.pos = pos;
            this.number = number;
         

            if (number != 0)
            {
                //单双
                if (number % 2 == 0)
                    this.ds = NumberDS.双;
                else
                    this.ds = NumberDS.单;


                if (pos == CarNumEnum.P总)
                {
                    //总和
                    if ((number >= 15) && (number <= 202))
                    {
                        this.dx = NumberDX.小;
                    }
                    else if ((number >= 203) && (number <= 390))
                    {
                        this.dx = NumberDX.大;
                    }
                }
                else
                {
                    //大小
                    if ((number - 40) > 0)
                        this.dx = NumberDX.大;
                    else
                        this.dx = NumberDX.小;
                }

                //尾大小

                var ns = number % 10;   //得到尾数
                if (ns >= 0 && ns <= 4)
                    this.wdx = NumberWDX.尾小;
                else
                    this.wdx = NumberWDX.尾大;

                var n1 = number / 10;
                int nSum = n1 + ns;
                if (nSum % 2 == 0)
                    this.hds = NumberHDS.合双;
                else
                    this.hds = NumberHDS.合单;

                if(pos == CarNumEnum.P总)
                {
                    this.hds = (NumberHDS)((int)this.ds);
                }
            }
        }


        public override string ToString()
        {
            return this.number.ToString();
        }
    }
}
