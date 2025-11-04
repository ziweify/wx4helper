using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.BetSite.Boter
{
    public class BoterBgDataResponse
    {
        public int issueid { get; set; }
        public int issue_day_index { get; set; }
        /// <summary>
        ///     开奖日期
        /// </summary>
        public string date { get; set; }
        //开奖时间
        public string lottery_time { get; set; }
        public string lotteryData { get; set; }
        public int status { get; set; }
    }
}
