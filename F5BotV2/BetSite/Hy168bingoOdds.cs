using F5BotV2.Game.BinGou;
using LxLib.LxNet;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LxLib.LxSys;
using F5BotV2.Model;
using System.Runtime.InteropServices.WindowsRuntime;

namespace F5BotV2.BetSite.HongHai
{
    public class Hy168bingoOdds
        : OddsBingGouBase
    {
        public override bool GetUpdata(string urlRoot, string cookie, string p_type)
        {
            return true;
        }

        public Hy168bingoOdds() {
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.单, true).SetValue("平码一", "23378907", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.双, true).SetValue("平码一", "23378908", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.大, true).SetValue("平码一", "23378909", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.小, true).SetValue("平码一", "23378910", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.尾大, true).SetValue("平码一", "23378911", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.尾小, true).SetValue("平码一", "23378912", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.合单, true).SetValue("平码一", "23378915", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.合双, true).SetValue("平码一", "23378916", 1.97f);

            //二车
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.单, true).SetValue("平码二", "23378969", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.双, true).SetValue("平码二", "23378970", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.大, true).SetValue("平码二", "23378971", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.小, true).SetValue("平码二", "23378972", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.尾大, true).SetValue("平码二", "23378973", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.尾小, true).SetValue("平码二", "23378974", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.合单, true).SetValue("平码二", "23378977", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.合双, true).SetValue("平码二", "23378978", 1.97f);
  
            //三车
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.单, true).SetValue("平码三", "23379031", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.双, true).SetValue("平码三", "23379032", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.大, true).SetValue("平码三", "23379033", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.小, true).SetValue("平码三", "23379034", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.尾大, true).SetValue("平码三", "23379035", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.尾小, true).SetValue("平码三", "23379036", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.合单, true).SetValue("平码三", "23379039", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.合双, true).SetValue("平码三", "23379040", 1.97f);

            //四车
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.单, true).SetValue("平码四", "23379093", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.双, true).SetValue("平码四", "23379094", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.大, true).SetValue("平码四", "23379095", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.小, true).SetValue("平码四", "23379096", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.尾大, true).SetValue("平码四", "23379097", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.尾小, true).SetValue("平码四", "23379098", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.合单, true).SetValue("平码四", "23379101", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.合双, true).SetValue("平码四", "23379102", 1.97f);

            //五车
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.单, true).SetValue("特码(第五球)", "23379155", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.双, true).SetValue("特码(第五球)", "23379156", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.大, true).SetValue("特码(第五球)", "23379157", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.小, true).SetValue("特码(第五球)", "23379158", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.尾大, true).SetValue("特码(第五球)", "23379159", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.尾小, true).SetValue("特码(第五球)", "23379160", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.合单, true).SetValue("特码(第五球)", "23379163", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.合双, true).SetValue("特码(第五球)", "23379164", 1.97f);
 
            //总车
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.单, true).SetValue("前五和值", "23379193", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.双, true).SetValue("前五和值", "23379194", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.大, true).SetValue("前五和值", "23379195", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.小, true).SetValue("前五和值", "23379196", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.尾大, true).SetValue("前五和值", "23379197", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.尾小, true).SetValue("前五和值", "23379198", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.龙, true).SetValue("前五和值", "23379199", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.虎, true).SetValue("前五和值", "23379200", 1.97f);
        }
    }
}
