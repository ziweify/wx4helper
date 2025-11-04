using F5BotV2.BetSite.HongHai;
using F5BotV2.BetSite.mt168;
using F5BotV2.BetSite.Qt;
using F5BotV2.BetSite.S880;
using F5BotV2.BetSite.yyz168;
using F5BotV2.CefBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.BetSite
{

    //不使用盘口
    //白海峡
    //白元宇宙2
    public enum BetSiteType {
        不使用盘口 = 0, 
        元宇宙2 = 1, 
        海峡 = 2,
        QT = 3,
        茅台 = 5,
        太平洋 = 6,
        蓝A = 7,
        红海 = 8,
        S880 = 9,
        ADK = 10,
        红海无名 = 11,
        果然 = 12,
        蓝B = 15,
        AC = 16,
        通宝=17,
        通宝PC = 18,
        HY168 = 19,
        bingo168 = 20
    }


    /// <summary>
    ///     站点工厂
    /// </summary>
    public class BetSiteFactory
    {
        public static IBetApi Create(BetSiteType site_type, IBetBrowserBase cefview)
        {
            IBetApi betapi = null;
            switch (site_type)
            {
                case BetSiteType.海峡:
                    betapi = new HX666(cefview);
                    break;
                case BetSiteType.元宇宙2:
                    betapi = new YYZ2Member();
                    break;
                case BetSiteType.不使用盘口:
                    betapi = new NoneSite();
                    break;
                case BetSiteType.茅台:
                    betapi = new Mt168Member(cefview);
                    break;
                case BetSiteType.太平洋:
                    betapi = new Mt168Member(cefview);
                    break;
                case BetSiteType.蓝A:
                    betapi = new LanABetSite(cefview);
                    break;
                case BetSiteType.红海:
                    betapi = new HongHaiMember(cefview);
                    break;
                case BetSiteType.通宝:
                    betapi = new TongBaoMember(cefview);
                    break;
                case BetSiteType.通宝PC:
                    betapi = new TongBaoPcMember(cefview);
                    break;
                case BetSiteType.红海无名:
                    betapi = new HongHaiWuMing(cefview);
                    break;
                case BetSiteType.S880:
                    betapi = new S880Member(cefview);
                    break;
                case BetSiteType.ADK:
                    betapi = new ADKMember(cefview);
                    break;
                case BetSiteType.QT:
                    betapi = new QtBet(cefview);
                    break;
                case BetSiteType.蓝B:
                    betapi = new QtBet(cefview);
                    break;
                case BetSiteType.果然:
                    betapi = new Kk888Member(cefview);
                    break;
                case BetSiteType.AC:
                    betapi = new AcMember(cefview);
                    break;
                case BetSiteType.HY168:
                    betapi = new Hy168bingoMember(cefview, BetSiteType.HY168);
                    break;
                case BetSiteType.bingo168:
                    betapi = new Hy168bingoMember(cefview, BetSiteType.bingo168);
                    break;
                default:
                    betapi = new NoneSite();
                    break;
            }
            return betapi;
        }
    }
}
