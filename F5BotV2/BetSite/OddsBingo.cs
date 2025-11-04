using F5BotV2.Model;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.BetSite
{
    /// <summary>
    ///     游戏赔率表
    /// </summary>
    public class OddsBingo
    {
        /// <summary>
        ///     未知, 车号
        /// </summary>
        public CarNumEnum car { get; set; } //这个是自己定义的名字

        public string carName { get; set; } //car别名- 对应网站那边的名字

        /// <summary>
        ///     玩法
        /// </summary>
        public BetPlayEnum play { get; set; }


        /// <summary>
        ///     赔率.通过网站数据获取填充得
        /// </summary>
        public float odds { get; set; }

        //B1DS_D/ 每个网站的玩法名不一样的
        public string oddsName { get; set; } 


        public bool SetValue(string carName, string odsName, float odds = 1.97f)
        {
            if(!string.IsNullOrEmpty(carName))
                this.carName = carName;
            this.odds = odds;
            if(!string.IsNullOrEmpty(odsName))
                this.oddsName = odsName;
            return true;
        }


        /// <summary>
        ///     
        /// </summary>
        /// <param name="car">我程序对应得</param>
        /// <param name="play"></param>
        /// <param name="odsName"></param>
        /// <param name="odsValue"></param>
        public OddsBingo(CarNumEnum car, BetPlayEnum play, string carName, string odsName, float odsValue)
        {
            this.car = car;
            this.carName = carName;
            this.play = play;
            this.oddsName = odsName;
            this.odds = odsValue;
        }
    }
}
