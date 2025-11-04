using CCWin.SkinClass;
using CsQuery.Utility;
using F5BotV2.Game.BinGou;
using F5BotV2.Model;
using LxLib.LxNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.BetSite.yyz168
{
    public class HX666Odds
        : List<OddsBingo>
    {

        public HX666Odds()
        {
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.单, "", "B1DS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.双, "", "B1DS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.大, "", "B1DX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.小, "", "B1DX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.尾大, "", "B1WDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.尾小, "", "B1WDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.合单, "", "B1HDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.合双, "", "B1HDS_S", 0f));

            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.单, "", "B2DS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.双, "", "B2DS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.大, "", "B2DX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.小, "", "B2DX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.尾大, "", "B2WDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.尾小, "", "B2WDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.合单, "", "B2HDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.合双, "", "B2HDS_S", 0f));

            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.单, "", "B3DS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.双, "", "B3DS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.大, "", "B3DX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.小, "", "B3DX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.尾大, "", "B3WDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.尾小, "", "B3WDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.合单, "", "B3HDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.合双, "", "B3HDS_S", 0f));

            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.单, "", "B4DS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.双, "", "B4DS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.大, "", "B4DX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.小, "", "B4DX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.尾大, "", "B4WDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.尾小, "", "B4WDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.合单, "", "B4HDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.合双, "", "B4HDS_S", 0f));

            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.单, "", "B5DS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.双, "", "B5DS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.大, "", "B5DX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.小, "", "B5DX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.尾大, "", "B5WDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.尾小, "", "B5WDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.合单, "", "B5HDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.合双, "", "B5HDS_S", 0f));

            //总和:ZHDS_D
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.单, "", "ZHDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.双, "", "ZHDS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.大, "", "ZHDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.小, "", "ZHDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.尾大, "", "HWDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.尾小, "", "HWDX_X", 0f));
            //LH_L
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.龙, "", "LH_L", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.虎, "", "LH_H", 0f));
        }

        private bool _isUpdata = false;  //是否已经更新
        public bool isUpdata
        {
            get { return _isUpdata; }
            set
            {
                _isUpdata = value;
            }
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="car"></param>
        /// <param name="play"></param>
        /// <param name="safemode">安全模式, 如果没有找到元素 true表示不会抛出异常 </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play, bool safemode = false)
        {
            var odds = this.FirstOrDefault(p=>p.car == car && p.play == play);
            if (odds.odds == 0)
            {
                if(!safemode)
                {
                    throw new Exception();
                }
            }
               
            return odds;
        }


        public OddsBingo GetOdds(string odsName, bool safemode = false)
        {
            var odds = this.FirstOrDefault(p => p.oddsName == odsName);
            if (!safemode)
            {
                if(odds.odds == 0)
                    throw new Exception();
            }
            return odds;
        }

       /// <summary>
       ///  获取数据
       /// </summary>
       /// <param name="cookie"></param>
       /// <param name="p_type">盘类型:A | B | C | D</param>
        public  bool GetUpdata(string url, string cookie, string p_type)
        {
            //https://8575517633-cj.mm666.co/PlaceBet/Loaddata?lotteryType=TWBINGO
            //string url = $"{urlRoot}/PlaceBet/Loaddata?lotteryType=TWBINGO";
            //itype=-1&settingCode=LM%2CWH%2CFLSX%2CLH&oddstype=A&lotteryType=TWBINGO&install=114018263
            bool response = false;
            try
            {
                int issueid = BinGouHelper.getNextIssueId(DateTime.Now);
                LxHttpHelper http = new LxHttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Cookie = cookie,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                    Postdata = $"itype=-1&settingCode=LM%2CWH%2CFLSX%2CLH&oddstype=A&lotteryType=TWBINGO&install={issueid}",
                };
                item.Header.Add("Sec-Ch-Ua-Mobile:?0");
                item.Header.Add("Sec-Ch-Ua-Platform:\"Windows\"");
                item.Header.Add("Sec-Fetch-Dest:document");
                item.Header.Add("Sec-Fetch-Mode:navigate");
                item.Header.Add("Sec-Fetch-Site:none");
                item.Header.Add("Sec-Fetch-User:?1");
                item.Header.Add("Upgrade-Insecure-Requests:1");
                var hr1 = http.GetHtml(item);

                JObject jRet = JObject.Parse(hr1.Html);
                var jState = jRet["State"].ToInt32();
                if (jState == 1)
                {
                    var jData = jRet["data"];
                    var obj = JsonConvert.DeserializeObject(jData.ToString());
                    foreach (var p in obj as JObject)
                    {
                        //"B1FLSX_F"
                        string key = p.Key;
                        float value = (float)p.Value;
                        var OddsObj = this.GetOdds(key, true);
                        if(OddsObj != null)
                        {
                            OddsObj.odds = value;
                        }
                    }
                    response = true;
                    _isUpdata = true;
                }
            }
            catch(Exception ex)
            {
                response = false;
            }
            return response;
        }
    }
}
