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

namespace F5BotV2.BetSite.mt168
{
    public class Mt168Odds
        : OddsBingGouBase
    {
        public override bool GetUpdata(string urlRoot, string cookie, string p_type)
        {
            bool response = false;
            string t = $"{LxTimestampHelper.GetTimeStamp13()}";  //13位时间戳
            string fullUrl = $"{urlRoot}/member/odds?lottery=TWBG&games=DX1%2CDX2%2CDX3%2CDX4%2CDX5%2CDX6%2CDX7%2CDX8%2CWDX1%2CWDX2%2CWDX3%2CWDX4%2CWDX5%2CWDX6%2CWDX7%2CWDX8%2CDS1%2CDS2%2CDS3%2CDS4%2CDS5%2CDS6%2CDS7%2CDS8%2CHDS1%2CHDS2%2CHDS3%2CHDS4%2CHDS5%2CHDS6%2CHDS7%2CHDS8%2CZDX%2CZDS%2CZWDX%2CLH1%2CLH2%2CLH3%2CLH4%2CB1%2CB2%2CB3%2CB4%2CB5%2CB6%2CB7%2CB8%2CZM%2CMP%2CZFB%2CWDX1%2CWDX2%2CWDX3%2CWDX4%2CWDX5%2CWDX6%2CWDX7%2CWDX8%2CHDS1%2CHDS2%2CHDS3%2CHDS4%2CHDS5%2CHDS6%2CHDS7%2CHDS8%2CFS%2CFW%2CFW1%2CFW2%2CFW3%2CFW4%2CFW5%2CFW6%2CFW7%2CFW8%2CZFB1%2CZFB2%2CZFB3%2CZFB4%2CZFB5%2CZFB6%2CZFB7%2CZFB8%2CLH1%2CLH2%2CLH3%2CLH4%2CLM2%2CLM22%2CLM3%2CLM32%2CLM4%2CLM5&_={t}";
            try
            {
                int issueid = BinGouHelper.getNextIssueId(DateTime.Now);
                LxHttpHelper http = new LxHttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = fullUrl,
                    Method = "GET",
                    Cookie = cookie,
                    ContentType = "application/json",
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                };
                //item.Header.Add("Sec-Ch-Ua-Mobile:?0");
                //item.Header.Add("Sec-Ch-Ua-Platform:\"Windows\"");
                //item.Header.Add("Sec-Fetch-Dest:document");
                //item.Header.Add("Sec-Fetch-Mode:navigate");
                //item.Header.Add("Sec-Fetch-Site:none");
                //item.Header.Add("Sec-Fetch-User:?1");
                //item.Header.Add("Upgrade-Insecure-Requests:1");
                var hr1 = http.GetHtml(item);

                JObject jRet = JObject.Parse(hr1.Html);
                //一车
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.大, true).SetValue("平码一", "DX1_D", Convert.ToSingle(jRet["DX1_D"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.小, true).SetValue("平码一", "DX1_X", Convert.ToSingle(jRet["DX1_X"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.单, true).SetValue("平码一", "DS1_D", Convert.ToSingle(jRet["DS1_D"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.双, true).SetValue("平码一", "DS1_S", Convert.ToSingle(jRet["DS1_S"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.尾大, true).SetValue("平码一", "WDX1_D", Convert.ToSingle(jRet["WDX1_D"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.尾小, true).SetValue("平码一", "WDX1_X", Convert.ToSingle(jRet["WDX1_X"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.合单, true).SetValue("平码一", "HDS1_D", Convert.ToSingle(jRet["HDS1_D"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.合双, true).SetValue("平码一", "HDS1_S", Convert.ToSingle(jRet["HDS1_S"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.福, true).SetValue("平码一", "FLSX1_F", Convert.ToSingle(jRet["FLSX1_F"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.禄, true).SetValue("平码一", "FLSX1_L", Convert.ToSingle(jRet["FLSX1_L"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.寿, true).SetValue("平码一", "FLSX1_S", Convert.ToSingle(jRet["FLSX1_S"]));
                this.GetOdds(CarNumEnum.P1, BetPlayEnum.喜, true).SetValue("平码一", "FLSX1_X", Convert.ToSingle(jRet["FLSX1_X"]));
                //二车
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.大, true).SetValue("平码二", "DX2_D", Convert.ToSingle(jRet["DX2_D"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.小, true).SetValue("平码二", "DX2_X", Convert.ToSingle(jRet["DX2_X"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.单, true).SetValue("平码二", "DS2_D", Convert.ToSingle(jRet["DS2_D"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.双, true).SetValue("平码二", "DS2_S", Convert.ToSingle(jRet["DS2_S"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.尾大, true).SetValue("平码二", "WDX2_D", Convert.ToSingle(jRet["WDX2_D"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.尾小, true).SetValue("平码二", "WDX2_X", Convert.ToSingle(jRet["WDX2_X"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.合单, true).SetValue("平码二", "HDS2_D", Convert.ToSingle(jRet["HDS2_D"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.合双, true).SetValue("平码二", "HDS2_S", Convert.ToSingle(jRet["HDS2_S"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.福, true).SetValue("平码二", "FLSX2_F", Convert.ToSingle(jRet["FLSX2_F"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.禄, true).SetValue("平码二", "FLSX2_L", Convert.ToSingle(jRet["FLSX2_L"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.寿, true).SetValue("平码二", "FLSX2_S", Convert.ToSingle(jRet["FLSX2_S"]));
                this.GetOdds(CarNumEnum.P2, BetPlayEnum.喜, true).SetValue("平码二", "FLSX2_X", Convert.ToSingle(jRet["FLSX2_X"]));
                //三车
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.大, true).SetValue("平码三", "DX3_D", Convert.ToSingle(jRet["DX3_D"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.小, true).SetValue("平码三", "DX3_X", Convert.ToSingle(jRet["DX3_X"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.单, true).SetValue("平码三", "DS3_D", Convert.ToSingle(jRet["DS3_D"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.双, true).SetValue("平码三", "DS3_S", Convert.ToSingle(jRet["DS3_S"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.尾大, true).SetValue("平码三", "WDX3_D", Convert.ToSingle(jRet["WDX3_D"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.尾小, true).SetValue("平码三", "WDX3_X", Convert.ToSingle(jRet["WDX3_X"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.合单, true).SetValue("平码三", "HDS3_D", Convert.ToSingle(jRet["HDS3_D"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.合双, true).SetValue("平码三", "HDS3_S", Convert.ToSingle(jRet["HDS3_S"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.福, true).SetValue("平码三", "FLSX3_F", Convert.ToSingle(jRet["FLSX3_F"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.禄, true).SetValue("平码三", "FLSX3_L", Convert.ToSingle(jRet["FLSX3_L"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.寿, true).SetValue("平码三", "FLSX3_S", Convert.ToSingle(jRet["FLSX3_S"]));
                this.GetOdds(CarNumEnum.P3, BetPlayEnum.喜, true).SetValue("平码三", "FLSX3_X", Convert.ToSingle(jRet["FLSX3_X"]));
                //四车
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.大, true).SetValue("平码四", "DX4_D", Convert.ToSingle(jRet["DX4_D"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.小, true).SetValue("平码四", "DX4_X", Convert.ToSingle(jRet["DX4_X"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.单, true).SetValue("平码四", "DS4_D", Convert.ToSingle(jRet["DS4_D"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.双, true).SetValue("平码四", "DS4_S", Convert.ToSingle(jRet["DS4_S"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.尾大, true).SetValue("平码四", "WDX4_D", Convert.ToSingle(jRet["WDX4_D"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.尾小, true).SetValue("平码四", "WDX4_X", Convert.ToSingle(jRet["WDX4_X"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.合单, true).SetValue("平码四", "HDS4_D", Convert.ToSingle(jRet["HDS4_D"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.合双, true).SetValue("平码四", "HDS4_S", Convert.ToSingle(jRet["HDS4_S"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.福, true).SetValue("平码四", "FLSX4_F", Convert.ToSingle(jRet["FLSX4_F"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.禄, true).SetValue("平码四", "FLSX4_L", Convert.ToSingle(jRet["FLSX4_L"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.寿, true).SetValue("平码四", "FLSX4_S", Convert.ToSingle(jRet["FLSX4_S"]));
                this.GetOdds(CarNumEnum.P4, BetPlayEnum.喜, true).SetValue("平码四", "FLSX4_X", Convert.ToSingle(jRet["FLSX4_X"]));
                //五车
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.大, true).SetValue("特码", "DX5_D", Convert.ToSingle(jRet["DX5_D"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.小, true).SetValue("特码", "DX5_X", Convert.ToSingle(jRet["DX5_X"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.单, true).SetValue("特码", "DS5_D", Convert.ToSingle(jRet["DS5_D"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.双, true).SetValue("特码", "DS5_S", Convert.ToSingle(jRet["DS5_S"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.尾大, true).SetValue("特码", "WDX5_D", Convert.ToSingle(jRet["WDX5_D"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.尾小, true).SetValue("特码", "WDX5_X", Convert.ToSingle(jRet["WDX5_X"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.合单, true).SetValue("特码", "HDS5_D", Convert.ToSingle(jRet["HDS5_D"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.合双, true).SetValue("特码", "HDS5_S", Convert.ToSingle(jRet["HDS5_S"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.福, true).SetValue("特码", "FLSX5_F", Convert.ToSingle(jRet["FLSX5_F"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.禄, true).SetValue("特码", "FLSX5_L", Convert.ToSingle(jRet["FLSX5_L"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.寿, true).SetValue("特码", "FLSX5_S", Convert.ToSingle(jRet["FLSX5_S"]));
                this.GetOdds(CarNumEnum.P5, BetPlayEnum.喜, true).SetValue("特码", "FLSX5_X", Convert.ToSingle(jRet["FLSX5_X"]));
                //总车
                this.GetOdds(CarNumEnum.P总, BetPlayEnum.大, true).SetValue("和值", "ZDX_D", Convert.ToSingle(jRet["ZDX_D"]));
                this.GetOdds(CarNumEnum.P总, BetPlayEnum.小, true).SetValue("和值", "ZDX_X", Convert.ToSingle(jRet["ZDX_X"]));
                this.GetOdds(CarNumEnum.P总, BetPlayEnum.单, true).SetValue("和值", "ZDS_D",  Convert.ToSingle(jRet["ZDS_D"]));
                this.GetOdds(CarNumEnum.P总, BetPlayEnum.双, true).SetValue("和值", "ZDS_S", Convert.ToSingle(jRet["ZDS_S"]));
                this.GetOdds(CarNumEnum.P总, BetPlayEnum.尾大, true).SetValue("和值", "ZWDX_D", Convert.ToSingle(jRet["ZWDX_D"]));
                this.GetOdds(CarNumEnum.P总, BetPlayEnum.尾小, true).SetValue("和值", "ZWDX_X", Convert.ToSingle(jRet["ZWDX_X"]));
                this.GetOdds(CarNumEnum.P总, BetPlayEnum.龙, true).SetValue("和值", "LH_L", Convert.ToSingle(jRet["LH_L"]));
                this.GetOdds(CarNumEnum.P总, BetPlayEnum.虎, true).SetValue("和值", "LH_H",Convert.ToSingle(jRet["LH_H"]));

                response = true;
                isUpdata = true;
                //if (jState == 1)
                //{
                //    var jData = jRet["data"];
                //    var obj = JsonConvert.DeserializeObject(jData.ToString());
                //    foreach (var p in obj as JObject)
                //    {
                //        //"B1FLSX_F"
                //        string key = p.Key;
                //        float value = (float)p.Value;
                //        var OddsObj = this.GetOdds(key, true);
                //        if (OddsObj != null)
                //        {
                //            OddsObj.odds = value;
                //        }
                //    }
                //    response = true;
                //    _isUpdata = true;
                //}
            }
            catch (Exception ex)
            {
                response = false;
            }
            return response;
        }
    }
}
