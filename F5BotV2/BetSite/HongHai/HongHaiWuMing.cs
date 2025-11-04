using CefSharp;
using F5BotV2.BetSite.mt168;
using F5BotV2.BetSite.yyz168;
using F5BotV2.CefBrowser;
using LxLib.LxNet;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using F5BotV2.Model;
using CCWin.SkinClass;
using LxLib.LxSys;
using System.Web;
using System.Text.RegularExpressions;
using System.Net;
using System.Security.Cryptography;

namespace F5BotV2.BetSite.HongHai
{
    public class HongHaiWuMing
        : IBetApi
        , INotifyPropertyChanged
    {
        CancellationTokenSource cts = null;
        private HongHaiBingouOdds _Odds = new HongHaiBingouOdds();
        public HongHaiBingouOdds Odds { get { return _Odds; } }

        public string sid = "";
        public string Uuid = "";    //10016757
        public string token = "";

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (PropertyChanged == null)
                return;

            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }

        private IBetBrowserBase browser;
        IBetApi betApi { get; set; }

        public string urlRoot => betApi.urlRoot;

        public string cookie => betApi.cookie;

        public BetSiteType betSiteType => betApi.betSiteType;

        public bool isLoginSuccess { get => betApi.isLoginSuccess; set => betApi.isLoginSuccess = value; }

        public float amount => throw new NotImplementedException();

        //测试账号: wwww11
        //    密码: Aaa123
        public HongHaiWuMing(IBetBrowserBase browser)
        {
            betApi = new BetApi(BetSiteType.红海无名);
            SetRootUrl("https://pjpctreyoobvf.6f888.net/#/");
            this.browser = browser;
            this.browser.chromeBroser.FrameLoadEnd += ChromeBroser_FrameLoadEnd;
            this.browser.chromeBroser.RequestHandler = new CefRequestHandler(ChromeBroser_ResponseComplete);
        }

        private void ChromeBroser_ResponseComplete(object sender, ResponseEventArgs args)
        {
            //处理资源
            try
            {

                //https://frhrewobvower.da16888.top/frclienthall/getnoticeinfo
                //if (args.Url.IndexOf("/getnoticeinfo") != -1)
                //{
                //    //取数据 Msg
                   
                //   JObject jobj = JObject.Parse(args.Context);
                //   JObject jMsg = JObject.Parse(jobj["Msg"].ToString());
                //   int Error_code = jMsg["Error_code"].ToInt32();
                //    if(Error_code == 0)
                //    {
                //        sid = jMsg["Sid"].ToString();
                //        Uuid = jMsg["Uuid"].ToInt32();
                //    }
                //}
            //https://frhrewobvower.da16888.top/frclienthall/gettodaywinlost
                if(args.Url.IndexOf("/gettodaywinlost") != -1)
                {
                    sid = Regex.Match(args.PostData, "sid=([^&]+)").Groups[1].Value;
                    Uuid = Regex.Match(args.PostData, "uuid=([^&]+)").Groups[1].Value;
                    token = Regex.Match(args.PostData, "token=([^&]+)").Groups[1].Value;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("ERROR::ChromeBroser_ResponseComplete::"+ex.Message);
            }

        }


        //这个是自动登录...
        //测试账号
        //账:ssww168
        //密:Ssww168
        //地址:8912794526-tky.c4ux0uslgd.com
        /// <summary>
        ///     返回0表示成功, 返回其他值, 表示其他错误
        ///     错误码 600开头
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public int LoginAsync(string name, string pass, IBetBrowserBase browser)
        {
            Debug.WriteLine("LoginAsync::进入");
            if (cts == null)
            {
                cts = new CancellationTokenSource();

                Task.Factory.StartNew(async (object hx666) => {
                    await browser.LoadUrlAsyn(urlRoot, new Func<ChromiumWebBrowserExtend, bool>((p) =>
                    {
                        HongHaiWuMing hx = hx666 as HongHaiWuMing;
                        bool response = true;
                        try
                        {
                            p.OnJSDialog = OnJSDialogLogin;
                            //由于加载了验证页面, 判断是否是正确登录页
                            using (cts.Token.Register((Thread.CurrentThread.Abort)))
                            {
                                //这个线程就一直给他跑, 不返回了
                                while (true)
                                {
                                    //自动登录代码
                                    //var jsInput = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelectorAll('input').length");
                                    //if (jsInput != null)
                                    //    if (jsInput.Result != null)
                                    //    {
                                    //        if ((int)jsInput.Result.Result == 3)
                                    //        {
                                    //            p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#login > div.content > div > div.form > ul > li.l1 > input\").value = '" + name + "';");
                                    //            p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#login > div.content > div > div.form > ul > li.l2 > input\").value = '" + pass + "';");
                                    //            var jsYzm = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelector(\"#login > div.content > div > div.form > ul > li.l3 > input\").value");
                                    //            jsYzm.Wait();
                                    //            var jsYzmResult = jsYzm.Result.Result;
                                    //            if (jsYzmResult != null)
                                    //                if (jsYzmResult.ToString().Count() >= 4)
                                    //                {
                                    //                    p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#login > div.content > div > div.form > ul > li.l4 > span:nth-child(1)\").click()");

                                    //                    //判断是否登录成功, 判断title有没有错误信息
                                    //                    Thread.Sleep(1000);
                                    //                    //通知UI登录成功
                                    //                    //hx.isLoginSuccess = true;
                                    //                    //获取赔率
                                    //                    //hx.GetOdds();
                                    //                    //break;
                                    //                }
                                    //            //获取验证码
                                    //            Debug.Write("等待输入验证码!");
                                    //            //点击登录

                                    //            //退出过程
                                    //        }
                                    //    }
                                    try
                                    {
                                        if (p.UrlCurrent != null)
                                        {
                                            //检测到成功登录页面。执行相关动作
                                            //https://pjpctreyoobvf.6f888.net/#/guide
                                            if (p.UrlCurrent.IndexOf("/#/guid") != -1)
                                            {
                                               // p.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.querySelector(\"#app > div:nth-child(1) > div > div > div > div.agree-info > div > div > div > div.tongyi-fan > a:nth-child(2)\").click()");
                                                continue;
                                            }
                                            //进入首页关闭弹窗
                                            //https://pjpctreyoobvf.6f888.net/#/tema
                                            if (p.UrlCurrent.IndexOf("/#/tema") != -1)
                                            {
                                                //p.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.querySelector(\"#home > div.footer > div.notice-os > div > div.n-close > i\").click()");
                                            }
                                            //BG页面
                                            //https://pjpctreyoobvf.6f888.net/#/liangmianqw
                                            if (p.UrlCurrent.IndexOf("/#/liangmianqw") == -1)
                                            {
                                                hx.isLoginSuccess = true;
                                                if (!string.IsNullOrEmpty(Uuid) && !string.IsNullOrEmpty(sid))
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

        private void OnJSDialogLogin(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText)
        {
            if (dialogType == CefJsDialogType.Alert)
            {
                if (messageText.IndexOf("验证码错误") != -1)
                {

                }
                else if (messageText.IndexOf("账号或密码错误") != -1)
                {

                }
            }
        }


        //可以重新设置线路获取地址地址
        //里面是 线路1
        //       线路2.。。 等等等
        public bool SetRootUrl(string url)
        {
            //校验url.必须要带 / 结尾
            return betApi.SetRootUrl(url);
        }

        public void SetCookie(string cookie)
        {
            betApi.SetCookie(cookie);
        }

        public void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            //if (e.Url.IndexOf($"/Member/Agreement") >= 0)
            //{
            //    //自动点击同意
            //    //document.querySelectorAll('.submit_btn')[0].text == 同意
            //    //document.querySelectorAll('.submit_btn')[0].click()
            //    Task.Factory.StartNew(() => {
            //        var p = browser.chromeBroser;
            //        var js_tongyi = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelectorAll('.submit_btn')[0].text");
            //        js_tongyi.Wait();
            //        var jsText = (string)js_tongyi.Result.Result;
            //        if (jsText != null)
            //            if (jsText == "同意")
            //                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('.submit_btn')[0].click()");
            //    });
            //}
            //else if (e.Url.IndexOf($"{urlRoot}/Home/Index") >= 0)
            //{
            //    //处理弹窗
            //    Task.Factory.StartNew(() => {
            //        var p = browser.chromeBroser;
            //        for (int i = 1; i <= 4; i++)
            //        {
            //            //document.querySelectorAll('#ui-id-4')[0].parentNode.childNodes[1].textContent;
            //            string jsquery_textContent = $"document.querySelectorAll('#ui-id-{i}')[0].parentNode.childNodes[1].textContent";
            //            string jsquery_click = $"document.querySelectorAll('#ui-id-{i}')[0].parentNode.childNodes[1].click()";
            //            var js_tongyi = p.GetBrowser().MainFrame.EvaluateScriptAsync(jsquery_textContent);
            //            js_tongyi.Wait();
            //            var jsText = (string)js_tongyi.Result.Result;
            //            if (jsText != null)
            //                if (jsText == "Close")
            //                    p.GetBrowser().MainFrame.ExecuteJavaScriptAsync(jsquery_click);
            //        }


            //    });
            //}
            //else if (e.Url.IndexOf($"?ReturnUrl=/Home/Index") >= 0)
            //{
            //    //被挤下线了，需要重新登录

            //}
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
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
            Debug.WriteLine($"Bet::合成包");
            int sum = betitems.GetAmountTatol();
            //dynamic postPackect = new ExpandoObject();
            StringBuilder postPackect = new StringBuilder(128);

            //合成发送包
            try
            {
                postPackect.Append($"uuid={this.Uuid}");
                postPackect.Append($"&sid={this.sid}");
                postPackect.Append($"&roomeng=twbingo");
                postPackect.Append($"&pan=A");
                postPackect.Append($"shuitype=0");

                List<dynamic> bets = new List<dynamic>();
                for (int i = 0; i < betitems.Count; i++)
                {
                    var ibettem = betitems[i];
                    var ods = _Odds.GetOdds(ibettem.car, ibettem.play);     //填充赔率

                    //动态生成对象
                    dynamic betdata = new ExpandoObject();
                    betdata.id = Conversion.ToInt32(ods.oddsName);
                    betdata.money = ibettem.moneySum;
                    bets.Add(betdata);
                }

                //这里的token要用计算出来的
                string timestamp = LxTimestampHelper.GetTimeStamp();
                string token_source = $"{this.Uuid}{this.sid}{timestamp}WEOROCBS";
                string toekn_md5 = LxEncrypt.GetMD5_32(token_source, "utf-8");

                string arrbet = JsonConvert.SerializeObject(bets);
                string arrbet_encode = WebUtility.UrlEncode(arrbet);
                postPackect.Append($"&arrbet={arrbet_encode}");
                postPackect.Append($"&token={toekn_md5}");
                postPackect.Append($"&timestamp={timestamp}");

            }
            catch (Exception ex)
            {
                //记录异常信息
                _Odds.isUpdata = false;
                string jsonitem = JsonConvert.SerializeObject(items);
                loger(Log.Create($"Bet::错误::{betSiteType}赔率获取失败", $"BetStandardOrderList={jsonitem},errmsg={ex.Message}"));
                return BetStatus.赔率获取失败;
            }


            string postdata = postPackect.ToString();

            //发送测试
            string url = $"{urlRoot}/frcomgame/setneworder";
            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Cookie = browser.Cookies,
                Postdata = postdata,
                Accept = "application/json, text/javascript, */*; q=0.01", //"application/json, text/javascript, */*; q=0.01",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                ContentType = "application/x-www-form-urlencoded;charset=UTF-8",   //这个很重要, 没这个, 服务器就无法解析注单
            };
            HttpResult hr = null;
            //成功返回值
            //{\"TimeIniBet\":25,\"TimeSaveBet\":11,\"succeed\":1,\"msg\":\"下注成功!\",\"BettingNumber\":7692,\"betList\":[{\"OddNo\":\"N1706300089134\",\"MidType\":\"平码一\",\"DisplayName\":\"大\",\"Odds\":1.97,\"Amount\":20,\"ReturnValue\":1.3},{\"OddNo\":\"N1706300089135\",\"MidType\":\"平码一\",\"DisplayName\":\"小\",\"Odds\":1.97,\"Amount\":20,\"ReturnValue\":1.3}],\"installment\":\"112052657\"}
            //失败返回值
            //"{\"succeed\":2,\"msg\":\"无注单可投!\"}"

            //解析返回值 ToInt32
            try
            {
                hr = http.GetHtml(item);
                JObject jResult = JObject.Parse(hr.Html);
                bool succeed = jResult["Status"].ToBoolean(false);
                if (succeed)
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
            catch (Exception ex)
            {
                loger(Log.Create($"Bet::POST::{betSiteType}异常", $"ex={ex.Message}"));
                status = BetStatus.异常;
                string html = "";
                if (hr != null)
                    html = hr.Html;
                loger(Log.Create($"Bet::POST::{betSiteType}POST返回异常", $"hr={html},errmsg={ex.Message}"));
            }
            Debug.WriteLine($"::Bet::end::{issueid}");
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
}
