using CCWin.SkinClass;
using CefSharp;
using CefSharp.WinForms;
using CsQuery;
using CsQuery.Utility;
using F5BotV2.BetSite.yyz168;
using F5BotV2.CefBrowser;
using F5BotV2.Model;
using LxLib.LxNet;
using LxLib.LxSys;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LxHttpHelper = LxLib.LxNet.LxHttpHelper;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Dynamic;
using System.Web;

namespace F5BotV2.BetSite.mt168
{
    public class Mt168Member
        : IBetApi
        , INotifyPropertyChanged
    {
        CancellationTokenSource cts = null;
        private Mt168Odds _Odds = new Mt168Odds();
        public Mt168Odds Odds { get { return _Odds; } }

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
        public Mt168Member(IBetBrowserBase browser)
        {
            betApi = new BetApi(BetSiteType.茅台);
            SetRootUrl("https://8912794526-tky.c4ux0uslgd.com/");
            this.browser = browser;
            this.browser.chromeBroser.FrameLoadEnd += ChromeBroser_FrameLoadEnd;
            
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
                        Mt168Member hx = hx666 as Mt168Member;
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
                                    var jsInput = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelectorAll('input').length");
                                    if (jsInput != null)
                                        if (jsInput.Result != null)
                                        {
                                            if ((int)jsInput.Result.Result == 5)
                                            {
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"body > div.main > div > div.login > form > div.info.username > input\").value = '" + name + "';");
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"body > div.main > div > div.login > form > div.info.password > input\").value = '" + pass + "';");
                                                var jsYzm = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelector(\"body > div.main > div > div.login > form > div.info.code > input\").value");
                                                jsYzm.Wait();
                                                var jsYzmResult = jsYzm.Result.Result;
                                                if (jsYzmResult != null)
                                                    if (jsYzmResult.ToString().Count() >= 4)
                                                    {
                                                        p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"body > div.main > div > div.login > form > div.control > input[type=submit]\").click()");

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
                                        if(p.UrlCurrent != null)
                                        {
                                            if (p.UrlCurrent.IndexOf("member/agreement") != -1)
                                            {
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.querySelector(\"body > div > div > ul > li.user_winmain > div > ul > li.user_winbu > div > span > a.yes\").click()");
                                                continue;
                                            }

                                            if (p.UrlCurrent.IndexOf("member/index") != -1)
                                            {
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.querySelector(\"#l_TWBG > span\")");
                                            }
                                            //判断是否登录页面
                                            if (p.UrlCurrent.IndexOf("/login") == -1)
                                            {
                                                hx.isLoginSuccess = true;
                                                //获取赔率在这里
                                                hx.Odds.GetUpdata(this.MakeUrl(this.urlRoot, ""), browser.Cookies, "");
                                                break;
                                            }
                                        }
                                    }
                                    catch(Exception ex)
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
            if(dialogType == CefJsDialogType.Alert)
            {
                if(messageText.IndexOf("验证码错误") != -1)
                {

                }
                else if(messageText.IndexOf("账号或密码错误") != -1)
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

            int sum = betitems.GetAmountTatol();
            dynamic postPackect = new ExpandoObject();
            if(!_Odds.isUpdata)
            {
                _Odds.GetUpdata(this.MakeUrl(this.urlRoot, ""), browser.Cookies, "");
            }
            //合成发送包
            try
            {
                postPackect.lottery = "TWBG";
                postPackect.drawNumber = items[0].IssueId.ToString();
                //postPackect.bets = new List<dynamic>();

                List<dynamic> bets = new List<dynamic>();
                for (int i = 0; i < betitems.Count; i++)
                {
                    var ibettem = betitems[i];
                    var ods = _Odds.GetOdds(ibettem.car, ibettem.play);     //填充赔率
                    //betdata%5B0%5D%5BAmount%5D=10&betdata%5B0%5D%5BKeyCode%5D=B1DS_D&betdata%5B0%5D%5BOdds%5D=1.97
                    //sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][Amount]") + $"={betitems[i].moneySum}" + "&");
                    //sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][KeyCode]") + $"={ods.oddsName}" + "&");
                    //sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][Odds]") + $"={ods.odds}");
                    //动态生成对象
                    string[] odsName = ods.oddsName.Split('_');
                    dynamic betdata = new ExpandoObject();
                    betdata.game = odsName[0];
                    betdata.contents = odsName[1];
                    betdata.amount = ibettem.moneySum;
                    betdata.odds = ods.odds;
                    //betdata.title = HttpUtility.UrlEncode(ods.carName);

                    bets.Add(betdata);
                }
                postPackect.bets = bets;
                postPackect.fastBets = false;
                postPackect.ignore = false;
            }
            catch (Exception ex)
            {
                //记录异常信息
                _Odds.isUpdata = false;
                string jsonitem = JsonConvert.SerializeObject(items);
                loger(Log.Create($"Bet::错误::{betSiteType}赔率获取失败", $"BetStandardOrderList={jsonitem},errmsg={ex.Message}"));
                return BetStatus.赔率获取失败;
            }


            string postdata = JsonConvert.SerializeObject(postPackect);

            //发送测试
            string url = $"{urlRoot}/member/bet";
            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Cookie = browser.Cookies,
                Postdata = postdata,
                Accept = "application/json, text/javascript, */*; q=0.01", //"application/json, text/javascript, */*; q=0.01",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                ContentType = "application/json",   //这个很重要, 没这个, 服务器就无法解析注单
            };
            var hr = http.GetHtml(item);
            //成功返回值
            //{\"TimeIniBet\":25,\"TimeSaveBet\":11,\"succeed\":1,\"msg\":\"下注成功!\",\"BettingNumber\":7692,\"betList\":[{\"OddNo\":\"N1706300089134\",\"MidType\":\"平码一\",\"DisplayName\":\"大\",\"Odds\":1.97,\"Amount\":20,\"ReturnValue\":1.3},{\"OddNo\":\"N1706300089135\",\"MidType\":\"平码一\",\"DisplayName\":\"小\",\"Odds\":1.97,\"Amount\":20,\"ReturnValue\":1.3}],\"installment\":\"112052657\"}
            //失败返回值
            //"{\"succeed\":2,\"msg\":\"无注单可投!\"}"

            //解析返回值
            try
            {
                JObject jResult = JObject.Parse(hr.Html);
                int succeed = jResult["status"].ToInt32();
                if (succeed == 0)
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
                status = BetStatus.异常;
                string html = "";
                if (hr != null)
                    html = hr.Html;
                loger(Log.Create($"Bet::POST::{betSiteType}POST返回异常", $"hr={html},errmsg={ex.Message}"));
            }
            return status;
        }

        public void loger(Log log)
        {
            betApi.loger(log);
        }

        public bool GetCurrentOpenLotteryData()
        {
            throw new NotImplementedException();
        }

        public bool GetUserInfoUpdata()
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play)
        {
            return betApi.GetOdds(car, play);
        }
    }
}
