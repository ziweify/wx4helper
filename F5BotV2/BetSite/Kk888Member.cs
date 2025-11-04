using CCWin.SkinClass;
using CefSharp;
using F5BotV2.BetSite.HongHai;
using F5BotV2.BetSite.Qt;
using F5BotV2.BetSite.yyz168;
using F5BotV2.CefBrowser;
using F5BotV2.Game.BinGou;
using F5BotV2.Model;
using HtmlAgilityPack;
using LxLib.LxNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace F5BotV2.BetSite
{
    /// <summary>
    ///     果然科技的
    /// </summary>
    public class Kk888Member
                : IBetApi
        , INotifyPropertyChanged
    {
        bool isOddsStatus = false;  //是否得到赔率了
        Kk888Odds kkodds = new Kk888Odds();
        CancellationTokenSource cts = null;
        private IBetBrowserBase browser;
        public event PropertyChangedEventHandler PropertyChanged;
        IBetApi betApi { get; set; }

        //---------抓取数据----------
        public string uid { get; set; }
        public string grpid { get; set; }
        //---------------------------

        public string urlRoot => betApi.urlRoot;

        public float amount => betApi.amount;

        public string cookie => betApi.cookie;

        public BetSiteType betSiteType => betApi.betSiteType;

        public bool isLoginSuccess { get => betApi.isLoginSuccess; set => betApi.isLoginSuccess = value; }

        public Kk888Member(IBetBrowserBase browser)
        {
            betApi = new BetApi(BetSiteType.果然);
            SetRootUrl("https://kk888.link/");
            this.browser = browser;
            this.browser.chromeBroser.FrameLoadEnd += ChromeBroser_FrameLoadEnd;
            this.browser.chromeBroser.RequestHandler = new CefRequestHandler(ChromeBroser_ResponseComplete);

        }

        public bool SetRootUrl(string url)
        {
            return betApi.SetRootUrl(url);
        }

        public void SetCookie(string cookie)
        {
            betApi.SetCookie(cookie);
        }

        public void Cancel()
        {
            betApi.Cancel();
        }

        public int LoginAsync(string name, string pass, IBetBrowserBase browser)
        {
            Debug.WriteLine("LoginAsync::进入");
            if (cts == null)
            {
                cts = new CancellationTokenSource();
                Task.Factory.StartNew(async (object hx666) => {
                    await browser.LoadUrlAsyn($"{urlRoot}/user/login", new Func<ChromiumWebBrowserExtend, bool>((p) =>
                    {
                        Kk888Member hx = hx666 as Kk888Member;
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
                                    //var jsInput = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelectorAll('input').length");
                                    try
                                    {
                                        if (p.UrlCurrent != null)
                                        {
                                            //BG页面
                                            //https://pjpctreyoobvf.6f888.net/#/liangmianqw
                                            if (p.UrlCurrent.IndexOf("/User/Bet/?gt=BINGO") == -1)
                                            {
                                                hx.isLoginSuccess = true;
                                                if (!string.IsNullOrEmpty(uid))
                                                {
                                                    break;
                                                }
                                                //获取赔率在这里
                                                //hx.Odds.GetUpdata(this.MakeUrl(this.urlRoot, ""), browser.Cookies, "");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {


                                    }
                                    Thread.Sleep(1000);
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

        public bool GetCurrentOpenLotteryData()
        {
            return betApi.GetCurrentOpenLotteryData();
        }

        public bool GetUserInfoUpdata()
        {
            return betApi.GetUserInfoUpdata(
                );
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
            //测试获得赔率
            //GetOdds(CarNumEnum.P1, BetPlayEnum.大);


            BetStatus status = BetStatus.未知;
            if (items.Count <= 0)
                return BetStatus.没有数据;

            int issueid = items[0].IssueId;
            Debug.WriteLine($"::Bet-honghai::begin::{issueid}");
            Debug.WriteLine($"Bet::Sort::{JsonConvert.SerializeObject(items)}");
            items.Sort(new BetStandardOrderComparer());

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

            //合成发送包
            string uPI_ID = "";
            string uPI_P = "";
            string uPI_M = "";
            foreach (var item in betitems)
            {
                var odds = this.GetOdds(item.car, item.play);
                if(!string.IsNullOrEmpty(odds.carName) && odds.odds > 0.5 && item.moneySum > 0)
                {
                    if (!string.IsNullOrEmpty(uPI_ID)) uPI_ID += ",";
                    if (!string.IsNullOrEmpty(uPI_P)) uPI_P += ",";
                    if (!string.IsNullOrEmpty(uPI_M)) uPI_M += ",";

                    uPI_ID += odds.oddsName;
                    uPI_P += odds.odds.ToString("F2");
                    uPI_M += item.moneySum;
                }
            }

            //开始组包
            Random rdm = new Random();
            StringBuilder postPackect = new StringBuilder(128);
            postPackect.Append($"gt=BINGO&qs={issueid}");
            postPackect.Append($"&uPI_ID={WebUtility.UrlEncode(uPI_ID)}");
            postPackect.Append($"&uPI_P={WebUtility.UrlEncode(uPI_P)}");
            postPackect.Append($"&uPI_M={WebUtility.UrlEncode(uPI_M)}");
            postPackect.Append($"&r={rdm.NextDouble()}");
            postPackect.Append($"&uid={this.uid}");
            string postdata = postPackect.ToString();

            LxHttpHelper http = new LxHttpHelper();
            string url = $"{this.urlRoot}/User/Bet/Betsave";
            string ccookie = browser.Cookies;
            HttpItem httpitem = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Cookie = ccookie,
                Postdata = postdata,
                Accept = "application/json, text/javascript, */*; q=0.01",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
            };

            HttpResult hr = http.GetHtml(httpitem);
            if(hr.Html.IndexOf("投注成功") != 0)
            {
                status = BetStatus.成功;
                loger(Log.Create($"Bet::POST::{betSiteType}成功", $"hr={hr.Html}"));
            }
            else
            {
                loger(Log.Create($"Bet::POST::{betSiteType}失败", $"hr={hr.Html}"));
            }
            //合并订单
            return status;
        }

        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play)
        {
            try
            {
                if(!isOddsStatus)
                {
                    LxHttpHelper http = new LxHttpHelper();
                    string url = $"{this.urlRoot}/User/Bet/getplinfo";
                    string ccookie = browser.Cookies;
                    Random rdm = new Random();
                    string qs = (BinGouHelper.getNextIssueId(DateTime.Now)).ToString(); //当前期数. 这里他放入sql里面查询了。可以通过这个字段，测试sql注入。
                    string postdata = $"gt=BINGO&grpid={grpid}&prekjqs={qs}&r={rdm.NextDouble()}&uid={uid}";
                    string referer = $"{this.urlRoot}/User/Bet/?gt=BINGO&grpid={grpid}&UID={uid}&r={rdm.NextDouble()}";
                    HttpItem item = new HttpItem()
                    {
                        URL = url,
                        Method = "POST",
                        Cookie = ccookie,
                        Postdata = postdata,
                        Accept = "application/json, text/javascript, */*; q=0.01",
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                        Referer = referer,
                        ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    };
                    HttpResult hr = http.GetHtml(item);
                    JObject jResult = JObject.Parse(hr.Html);
                    var jdata = jResult["data"].ToArray();
                    foreach (var odd in jdata)
                    {
                        string name = odd["gid"].ToString();
                        float ov = Convert.ToSingle(odd["ov"]);
                        var kk = kkodds.GetOdds(name, true);
                        if(kk != null)
                            kk.SetValue("", "", ov);
                    }
                    isOddsStatus = true;
                }

                return this.kkodds.GetOdds(car, play);
                //HtmlDocument doc = new HtmlDocument();
                //doc.LoadHtml(hr.Html);
                //var title = doc.DocumentNode.SelectSingleNode("//head/title");
                //var body = doc.DocumentNode.SelectSingleNode("//body/body/table/table");
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"GetOddsError::{ex.Message}");
                isOddsStatus = false;
            }
            return null;
        }


        public void loger(Log log)
        {
            betApi.loger(log);
        }

        public void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            //betApi.ChromeBroser_FrameLoadEnd(sender, e);
        }

        /// <summary>
        ///     资源加载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ChromeBroser_ResponseComplete(object sender, ResponseEventArgs args)
        {
            //处理资源
            try
            {

                if (args.Url.IndexOf("User/Bet/?gt=BINGO") != -1)
                {
                    //"https://kk888.link/User/Bet/?gt=BINGO&grpid=1&UID=8fa2a191b2a710e0b46e763ff6a00bf9&r=0.631514049107506"
                    //Regex.Match(args.PostData, "token=([^&]+)").Groups[1].Value;
                    uid = Regex.Match(args.Url, "UID=([^&]+)").Groups[1].Value;
                    grpid = Regex.Match(args.Url, "grpid=([^&]+)").Groups[1].Value;
                    //测试得到赔率，正式要注销以下
                    GetOdds( CarNumEnum.P2, BetPlayEnum.大);
                }
                //{
                //    //取数据 Msg

                //    JObject jobj = JObject.Parse(args.Context);
                //    JObject jMsg = JObject.Parse(jobj["Msg"].ToString());
                //    int Error_code = jMsg["Error_code"].ToInt32();
                //    if (Error_code == 0)
                //    {
                //        sid = jMsg["Sid"].ToString();
                //        Uuid = jMsg["Uuid"].ToInt32();
                //    }
                //}
                //else if (args.Url.IndexOf("user/gettodaywinlost") != -1)
                //{
                //    token = Regex.Match(args.PostData, "token=([^&]+)").Groups[1].Value;
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR::ChromeBroser_ResponseComplete::" + ex.Message);
            }

        }
    }

    public class Kk888Odds : OddsBingGouBase
    {
        public Kk888Odds()
        {
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.单, true).SetValue("平码一", "3120101", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.双, true).SetValue("平码一", "3120102", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.大, true).SetValue("平码一", "3110101", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.小, true).SetValue("平码一", "3110102", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.尾大, true).SetValue("平码一", "3130101", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.尾小, true).SetValue("平码一", "3130102", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.合单, true).SetValue("平码一", "3140101", 1.97f);
            this.GetOdds(CarNumEnum.P1, BetPlayEnum.合双, true).SetValue("平码一", "3140102", 1.97f);
            //二车
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.单, true).SetValue("平码二", "3120201", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.双, true).SetValue("平码二", "3120202", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.大, true).SetValue("平码二", "3110201", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.小, true).SetValue("平码二", "3110202", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.尾大, true).SetValue("平码二", "3130201", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.尾小, true).SetValue("平码二", "3130202", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.合单, true).SetValue("平码二", "3140201", 1.97f);
            this.GetOdds(CarNumEnum.P2, BetPlayEnum.合双, true).SetValue("平码二", "3140202", 1.97f);
            //三车
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.单, true).SetValue("平码三", "3120301", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.双, true).SetValue("平码三", "3120302", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.大, true).SetValue("平码三", "3110301", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.小, true).SetValue("平码三", "3110302", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.尾大, true).SetValue("平码三", "3130301", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.尾小, true).SetValue("平码三", "3130302", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.合单, true).SetValue("平码三", "3140301", 1.97f);
            this.GetOdds(CarNumEnum.P3, BetPlayEnum.合双, true).SetValue("平码三", "3140302", 1.97f);
            //四车
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.单, true).SetValue("平码四", "3120401", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.双, true).SetValue("平码四", "3120402", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.大, true).SetValue("平码四", "3110401", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.小, true).SetValue("平码四", "3110402", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.尾大, true).SetValue("平码四", "3130401", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.尾小, true).SetValue("平码四", "3130402", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.合单, true).SetValue("平码四", "3140401", 1.97f);
            this.GetOdds(CarNumEnum.P4, BetPlayEnum.合双, true).SetValue("平码四", "3140402", 1.97f);
            //五车
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.单, true).SetValue("特码", "3120501", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.双, true).SetValue("特码", "3120502", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.大, true).SetValue("特码", "3110501", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.小, true).SetValue("特码", "3110502", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.尾大, true).SetValue("特码", "3130501", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.尾小, true).SetValue("特码", "3130502", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.合单, true).SetValue("特码", "3140501", 1.97f);
            this.GetOdds(CarNumEnum.P5, BetPlayEnum.合双, true).SetValue("特码", "3140502", 1.97f);
            //总车
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.单, true).SetValue("和值", "3120001", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.双, true).SetValue("和值", "3120002", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.大, true).SetValue("和值", "3110001", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.小, true).SetValue("和值", "3110002", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.尾大, true).SetValue("和值", "3130001", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.尾小, true).SetValue("和值", "3130002", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.龙, true).SetValue("和值", "3150001", 1.97f);
            this.GetOdds(CarNumEnum.P总, BetPlayEnum.虎, true).SetValue("和值", "3150002", 1.97f);
        }

        public override bool GetUpdata(string url, string cookie, string p_type)
        {
            //更新赔率.. 
            return true;
        }
    }
}
