using F5BotV2.CefBrowser;
using LxLib.LxNet;
using F5BotV2.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using LxLib.LxSys;
using System.Diagnostics;
using System.Text.RegularExpressions;
using F5BotV2.BetSite.HongHai;
using System.Threading;
using F5BotV2.BetSite.mt168;
using F5BotV2.BetSite.yyz168;
using System.Dynamic;
using System.Net;
using System.Security.Policy;
using CCWin.SkinClass;

namespace F5BotV2.BetSite.Qt
{
    public class QtBet
        : IBetApi
    {
        CancellationTokenSource cts = null;
        private IBetApi betApi = null;
        private QtOdds _Odds = new QtOdds();

        public string urlRoot => betApi.urlRoot;

        public string cookie => betApi.cookie;

        public BetSiteType betSiteType => betApi.betSiteType;

        public bool isLoginSuccess { get => betApi.isLoginSuccess; set => betApi.isLoginSuccess = value; }

        public float amount { get; set; }

        IBetBrowserBase browser;

        public QtBet(IBetBrowserBase browser)
        {

            betApi = new BetApi( BetSiteType.QT);
            SetRootUrl("http://119.23.246.81/qt/");
            this.browser = browser;
            this.browser.chromeBroser.FrameLoadEnd += ChromeBroser_FrameLoadEnd;
            this.browser.chromeBroser.RequestHandler = new CefRequestHandler(ChromeBroser_ResponseComplete);

        }

        public int LoginAsync(string name, string pass, IBetBrowserBase browser)
        {
            Debug.WriteLine("LoginAsync::进入");
            if (cts == null)
            {
                cts = new CancellationTokenSource();

                Task.Factory.StartNew(async (object hx666) => {
                    await browser.LoadUrlAsyn(urlRoot, new Func<ChromiumWebBrowserExtend, bool>((p) =>
                    {
                        QtBet hx = hx666 as QtBet;
                        bool response = true;
                        try
                        {
                            //由于加载了验证页面, 判断是否是正确登录页
                            using (cts.Token.Register((Thread.CurrentThread.Abort)))
                            {
                                //这个线程就一直给他跑, 不返回了
                                while (true)
                                {
                                    //自动登录代码
                                    var jsInput = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelectorAll('input').length");
                                    if (jsInput != null)
                                        if (jsInput.Result != null)
                                        {
                                            if ((int)jsInput.Result.Result == 3)
                                            {
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#username\").value = '" + name + "';");
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#password\").value = '" + pass + "';");
                                                var jsYzm = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelector(\"#seccode\").value");
                                                jsYzm.Wait();
                                                var jsYzmResult = jsYzm.Result.Result;
                                                if (jsYzmResult != null)
                                                    if (jsYzmResult.ToString().Count() >= 4)
                                                    {
                                                        p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#btn-submit\").click()");

                                                        //判断是否登录成功, 判断title有没有错误信息
                                                        Thread.Sleep(1000);

                                                        //通知UI登录成功
                                                        //hx.isLoginSuccess = true;
                                                        //获取赔率
                                                        //hx.GetOdds();
                                                        //break;
                                                    }
                                                //获取验证码
                                                Debug.Write("等待输入验证码!");
                                                //点击登录

                                                //退出过程
                                            }
                                        }
                                    try
                                    {
                                        if (p.UrlCurrent != null)
                                        {

                                            if (p.UrlCurrent.IndexOf("index/index") != -1)
                                            {
                                                //获取赔率
                                               bool bupdata = _Odds.GetUpdata(urlRoot, browser.Cookies, "");
                                                if (bupdata)
                                                    break;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {


                                    }
                                    Thread.Sleep(2000);
                                    Debug.Write("等待登录完成!");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Write("LoginAsync::异常!退出");
                            response = false;
                        }
                        finally
                        {
                            cts = null;
                            //p.OnJSDialog = null;
                        }

                        return response;
                    }));

                    //释放线程参数
                    cts = null;
                }, this);
            }
            Debug.WriteLine("LoginAsync::退出");
            return 0;
        }

        private void ChromeBroser_ResponseComplete(object sender, ResponseEventArgs args)
        {
            //处理资源
            try
            {

                if (args.Url.IndexOf("user/login") != -1)
                {
                    //取数据 Msg

                    JObject jobj = JObject.Parse(args.Context);
                    JObject jMsg = JObject.Parse(jobj["Msg"].ToString());
                    int Error_code = Convert.ToInt32(jMsg["Error_code"]);
                    if (Error_code == 0)
                    {
                        //sid = jMsg["Sid"].ToString();
                        //Uuid = jMsg["Uuid"].ToInt32();
                    }
                }
                else if (args.Url.IndexOf("user/gettodaywinlost") != -1)
                {
                    //token = Regex.Match(args.PostData, "token=([^&]+)").Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR::ChromeBroser_ResponseComplete::" + ex.Message);
            }

        }


        public  List<BgLotteryData> getLotteryData(DateTime date, string strCookie)
        {
            this.SetCookie(strCookie);
            return getLotteryData(date);
        }

        public List<BgLotteryData> getLotteryData(DateTime date)
        {

            //URL::http://120.77.34.11/qt/ntwbg/get_list?page=1&limit=90&date=2022-05-18
            List<BgLotteryData> result = new List<BgLotteryData>();
            try
            {
                string dateStr = date.ToString("yyyy-MM-dd");
                LxHttpHelper http = new LxHttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = $"{urlRoot}/ntwbg/get_list?page=1&limit=256&date={dateStr}",
                    Cookie = this.cookie,
                };
                HttpResult hr = http.GetHtml(item);
                //解析结果
                JObject jsonHtmlResult = (JObject)JsonConvert.DeserializeObject(hr.Html);
                if (jsonHtmlResult == null)
                {
                    return result;
                }
                JToken jsonData = jsonHtmlResult["data"];
                JToken jsonCount = jsonHtmlResult["count"];
                //遍历数据, 从当前开奖, 遍历到当天第一期
                for (int i = jsonData.Count() - 1; i >= 0; i--)
                {
                    string open_data = jsonData[i]["open_data"].ToString();
                    string action_no = jsonData[i]["action_no"].ToString();
                    string open_timestamp = jsonData[i]["open_time"].ToString();
                    int l_open_timestamp = Convert.ToInt32(open_timestamp);
                    DateTime opentime = LxTimestampHelper.GetDateTime(l_open_timestamp);
                    result.Add(new BgLotteryData().FillLotteryData(Convert.ToInt32(action_no), open_data, LxTimestampHelper.datetimetostr(opentime)));
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public bool SetRootUrl(string url)
        {
            return betApi.SetRootUrl(url);
        }

        public void SetCookie(string cookie)
        {
            betApi.SetCookie(cookie);
        }

        public void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            betApi.ChromeBroser_FrameLoadEnd(sender, e);
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
            BetStatus status = BetStatus.未知;
            if (items.Count <= 0)
                return BetStatus.没有数据;

            Debug.WriteLine($"::Bet::");
            items.Sort(new BetStandardOrderComparer());
            Debug.WriteLine($"Bet::Sort::{JsonConvert.SerializeObject(items)}");

            //合并订单
            BetStandardOrderList betitems = new BetStandardOrderList();
            foreach (var bsOrder in items)
            {
                var last = betitems.LastOrDefault();
                if (last == null)
                {
                    betitems.Add(bsOrder);
                }
                else
                {
                    if (last.car == bsOrder.car && last.play == bsOrder.play)
                    {
                        last.moneySum += bsOrder.moneySum;
                    }
                    else
                    {
                        betitems.Add(bsOrder);
                    }
                }
            }

            List<dynamic> bets = new List<dynamic>();
            for (int i = 0; i < betitems.Count; i++)
            {
                var ods = _Odds.GetOdds(betitems[i].car, betitems[i].play);
                var ibettem = betitems[i];
                dynamic betdata = new ExpandoObject();
                betdata.action_no = Convert.ToString(betitems[i].IssueId);
                betdata.bonus_prop_id = ods.carName; //单都是44
                betdata.single_money = Conversion.ToString(ibettem.moneySum);
                betdata.action_data = ods.oddsName;
                bets.Add(betdata);
            }
            string arrbet = JsonConvert.SerializeObject(bets);
            string arrbet_encode = WebUtility.UrlEncode(arrbet);

            //投递结果
            LxHttpHelper http = new LxHttpHelper();
            string url = $"{urlRoot}/ntwbg/bet";
            string postdata = $"lbrJson={arrbet_encode}";
            Debug.WriteLine($"POST={postdata}");
            HttpItem item = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Cookie = browser.Cookies,
                Postdata = postdata,
                Accept = "application/json, text/javascript, */*; q=0.01", //"application/json, text/javascript, */*; q=0.01",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",   //这个很重要, 没这个, 服务器就无法解析注单
                Referer = "http://47.99.138.38/us/ntwbg/index",
                
               
            };
            item.Header.Add("X-Requested-With:XMLHttpRequest");
            var hr = http.GetHtml(item);
            //{"code":1,"msg":"投注成功！","tz_time":"2024-06-23 07:33:50","rj_date":"2024-06-23","action_no":"113035329","name":"cshh001"}
            try
            {
                JObject jResult = JObject.Parse(hr.Html);
                int code = jResult["code"].ToInt32();
                if(code == 1)
                {
                    loger(Log.Create($"Bet::POST::{betSiteType}成功", $"hr={hr.Html}"));
                    status = BetStatus.成功;
                }
                else
                { 
                    loger(Log.Create($"Bet::POST::{betSiteType}失败", $"hr={hr.Html}"));
                    status = BetStatus.失败;
                }
            }
            catch
            {
                status = BetStatus.异常;
            }
            return status;
        }

        public void loger(Log log)
        {
            betApi.loger(log);
        }

        public bool GetCurrentOpenLotteryData()
        {
            return true;
        }

        public bool GetUserInfoUpdata()
        {
            return true;
        }

        public void Cancel()
        {
             
        }

        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play)
        {
            return betApi.GetOdds(car, play);
        }
    }

    public class QtOdds
        : OddsBingGouBase
    {
        public override bool GetUpdata(string urlRoot, string cookie, string p_type)
        {
            LxHttpHelper http = new LxHttpHelper();
            string url = $"{urlRoot}/ntwbg/get_play";
            HttpItem item = new HttpItem()
            {
                Method = "POST",
                Cookie = cookie,
                Accept = "application/json, text/javascript, */*; q=0.01",
                URL = url,
            };
            var hr = http.GetHtml(item);
            JObject joret = JObject.Parse(hr.Html);
            int code = joret["code"].ToInt32();
            if(code == 1)
            {
                var jdata = joret["data"];
                JArray dataArry  = JArray.Parse(jdata.ToString());
                foreach(var data in dataArry)
                {
                    var id = data["id"].ToInt32();
                    var played_name = data["played_name"].ToString();
                    //查找修改对应的数据
                    if(played_name == "大" || played_name == "小" || played_name == "单" || played_name == "双"
                        || played_name == "尾大" || played_name == "尾小" || played_name == "合单" || played_name == "合双"
                        || played_name == "福" || played_name == "禄" || played_name == "寿" || played_name == "喜")
                    {
                        for(int i = 0; i < this.Count; i++)
                        {
                            if (this[i].car != CarNumEnum.P总 && this[i].car != CarNumEnum.未知)
                            {
                                if (this[i].play.ToString() == played_name)
                                    this[i].carName = Convert.ToString(id);
                            }
                        }
                    }
                    else if(played_name == "总和单" || played_name == "总和双" || played_name == "总和大" || played_name == "总和小"
                        || played_name == "龙" || played_name == "虎"
                        || played_name == "总尾大" || played_name == "总尾小")
                    {
                        played_name = played_name.Replace("总和", "");
                        played_name = played_name.Replace("总", "");
                        for (int i = 0; i < this.Count; i++)
                        {
                            if (this[i].car == CarNumEnum.P总)
                            {
                                if (this[i].play.ToString() == played_name)
                                    this[i].carName = Convert.ToString(id);
                            }
                        }
                    }
                }
                isUpdata = true;
            }
            else
            {
                isUpdata = false;
            }
            return isUpdata;
        }

        public QtOdds()
        {
            //carname = 盘口的 bonus_prop_id
            //odsname = 盘口的s action_data
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.单, true).SetValue("44", "0.3", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.双, true).SetValue("48", "0.4", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.大, true).SetValue("36", "0.2", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.小, true).SetValue("40", "0.1", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.尾大, true).SetValue("52", "0.2", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.尾小, true).SetValue("56", "0.1");
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.合单, true).SetValue("60", "0.3");
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.合双, true).SetValue("64", "0.4");
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.福, true).SetValue("84", "0.1", 2.5f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.禄, true).SetValue("88", "0.2", 2.5f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.寿, true).SetValue("92", "0.3", 2.5f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.喜, true).SetValue("96", "0.4", 2.5f);
            //二车
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.单, true).SetValue("44", "1.3", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.双, true).SetValue("48", "1.4", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.大, true).SetValue("36", "1.2", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.小, true).SetValue("40", "1.1", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.尾大, true).SetValue("52", "1.2", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.尾小, true).SetValue("56", "1.1");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.合单, true).SetValue("60", "1.3");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.合双, true).SetValue("64", "1.4");
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.福, true).SetValue("84", "1.1", 2.5f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.禄, true).SetValue("88", "1.2", 2.5f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.寿, true).SetValue("92", "1.3", 2.5f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.喜, true).SetValue("96", "1.4", 2.5f);
            //三车
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.单, true).SetValue("44", "2.3", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.双, true).SetValue("48", "2.4", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.大, true).SetValue("36", "2.2", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.小, true).SetValue("40", "2.1", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.尾大, true).SetValue("52", "2.2", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.尾小, true).SetValue("56", "2.1");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.合单, true).SetValue("60", "2.3");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.合双, true).SetValue("64", "2.4");
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.福, true).SetValue("84", "2.1", 2.5f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.禄, true).SetValue("88", "2.2", 2.5f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.寿, true).SetValue("92", "2.3", 2.5f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.喜, true).SetValue("96", "2.4", 2.5f);
            //四车
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.单, true).SetValue("44", "3.3", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.双, true).SetValue("48", "3.4", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.大, true).SetValue("36", "3.2", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.小, true).SetValue("40", "3.1", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.尾大, true).SetValue("52", "3.2", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.尾小, true).SetValue("56", "3.1");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.合单, true).SetValue("60", "3.3");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.合双, true).SetValue("64", "3.4");
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.福, true).SetValue("84", "3.1", 2.5f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.禄, true).SetValue("88", "3.2", 2.5f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.寿, true).SetValue("92", "3.3", 2.5f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.喜, true).SetValue("96", "3.4", 2.5f);
            //五车
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.单, true).SetValue("44", "4.3", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.双, true).SetValue("48", "4.4", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.大, true).SetValue("36", "4.2", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.小, true).SetValue("40", "4.1", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.尾大, true).SetValue("52", "4.2", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.尾小, true).SetValue("56", "4.1");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.合单, true).SetValue("60", "4.3");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.合双, true).SetValue("64", "4.4");
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.福, true).SetValue("84", "4.1", 2.5f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.禄, true).SetValue("88", "4.2", 2.5f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.寿, true).SetValue("92", "4.3", 2.5f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.喜, true).SetValue("96", "4.4", 2.5f);
            //总车
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.单, true).SetValue("4", "3");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.双, true).SetValue("8", "4");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.大, true).SetValue("12", "2");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.小, true).SetValue("16", "1");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.尾大, true).SetValue("20", "2");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.尾小, true).SetValue("24", "1");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.龙, true).SetValue("28", "1");
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.虎, true).SetValue("32", "2");
        }
    }
}
