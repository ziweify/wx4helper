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
    public class HongHaiBingouOdds
        : OddsBingGouBase
    {
        public override bool GetUpdata(string urlRoot, string cookie, string p_type)
        {
            return true;
        }

        public HongHaiBingouOdds() {
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.单, true).SetValue("平码一", "5372", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.双, true).SetValue("平码一", "5373", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.大, true).SetValue("平码一", "5370", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.小, true).SetValue("平码一", "5371", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.尾大, true).SetValue("平码一", "5374", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.尾小, true).SetValue("平码一", "5375");
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.合单, true).SetValue("平码一", "5376");
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.合双, true).SetValue("平码一", "5377");
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.福, true).SetValue("平码一", "21831", 2.5f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.禄, true).SetValue("平码一", "21832", 2.5f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.寿, true).SetValue("平码一", "21833", 2.5f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.喜, true).SetValue("平码一", "21834", 2.5f);
            //二车
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.单, true).SetValue("平码二", "5380");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.双, true).SetValue("平码二", "5381");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.大, true).SetValue("平码二", "5378");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.小, true).SetValue("平码二", "5379");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.尾大, true).SetValue("平码二", "5382");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.尾小, true).SetValue("平码二", "5383");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.合单, true).SetValue("平码二", "5384");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.合双, true).SetValue("平码二", "5385");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.福, true).SetValue("平码二", "21835", 2.5f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.禄, true).SetValue("平码二", "21836", 2.5f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.寿, true).SetValue("平码二", "21837", 2.5f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.喜, true).SetValue("平码二", "21838", 2.5f);
            //三车
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.单, true).SetValue("平码三", "5388");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.双, true).SetValue("平码三", "5389");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.大, true).SetValue("平码三", "5386");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.小, true).SetValue("平码三", "5387");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.尾大, true).SetValue("平码三", "5390");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.尾小, true).SetValue("平码三", "5391");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.合单, true).SetValue("平码三", "5392");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.合双, true).SetValue("平码三", "5393");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.福, true).SetValue("平码三", "21839", 2.5f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.禄, true).SetValue("平码三", "21840", 2.5f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.寿, true).SetValue("平码三", "21841", 2.5f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.喜, true).SetValue("平码三", "21842", 2.5f);
            //四车
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.单, true).SetValue("平码四", "5396");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.双, true).SetValue("平码四", "5397");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.大, true).SetValue("平码四", "5394");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.小, true).SetValue("平码四", "5395");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.尾大, true).SetValue("平码四", "5398");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.尾小, true).SetValue("平码四", "5399");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.合单, true).SetValue("平码四", "5400");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.合双, true).SetValue("平码四", "5401");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.福, true).SetValue("平码四", "21843",  2.5f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.禄, true).SetValue("平码四", "21844", 2.5f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.寿, true).SetValue("平码四", "21845", 2.5f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.喜, true).SetValue("平码四", "21846", 2.5f);
            //五车
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.单, true).SetValue("特码", "5404");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.双, true).SetValue("特码", "5405");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.大, true).SetValue("特码", "5402");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.小, true).SetValue("特码", "5403");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.尾大, true).SetValue("特码", "5406");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.尾小, true).SetValue("特码", "5407");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.合单, true).SetValue("特码", "5408");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.合双, true).SetValue("特码", "5409");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.福, true).SetValue("特码", "21847", 2.5f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.禄, true).SetValue("特码", "21848", 2.5f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.寿, true).SetValue("特码", "21849", 2.5f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.喜, true).SetValue("特码", "21850", 2.5f);
            //总车
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.单, true).SetValue("和值", "5366");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.双, true).SetValue("和值", "5367");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.大, true).SetValue("和值", "5364");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.小, true).SetValue("和值", "5365");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.尾大, true).SetValue("和值", "5368");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.尾小, true).SetValue("和值", "5369");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.龙, true).SetValue("和值", "5418");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.虎, true).SetValue("和值", "5419");
        }
    }
}
