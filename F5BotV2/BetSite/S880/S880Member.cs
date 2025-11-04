using CefSharp;
using F5BotV2.BetSite.HongHai;
using F5BotV2.BetSite.yyz168;
using F5BotV2.CefBrowser;
using LxLib.LxNet;
using LxLib.LxSys;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using F5BotV2.Model;
using CCWin.SkinClass;

namespace F5BotV2.BetSite.S880
{
    public class S880Member
        : IBetApi
        , INotifyPropertyChanged
    {
        CancellationTokenSource cts = null;
        private HongHaiBingouOdds _Odds = new HongHaiBingouOdds();
        public HongHaiBingouOdds Odds { get { return _Odds; } }

        public string p_id = "";
        public string tt_top = "";

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
        public S880Member(IBetBrowserBase browser)
        {
            betApi = new BetApi(BetSiteType.S880);
            SetRootUrl("http://47.106.119.141:880/");
            this.browser = browser;
            this.browser.chromeBroser.FrameLoadEnd += ChromeBroser_FrameLoadEnd;
            this.browser.chromeBroser.RequestHandler = new CefRequestHandler(ChromeBroser_ResponseComplete);
        }

        private void ChromeBroser_ResponseComplete(object sender, ResponseEventArgs args)
        {
            //处理资源
            try
            {

                if (args.Url.IndexOf("user/login") != -1)
                {
                    //取数据 Msg
                    //JObject jobj = JObject.Parse(args.Context);
                    //JObject jMsg = JObject.Parse(jobj["Msg"].ToString());
                    //int Error_code = jMsg["Error_code"].ToInt32();
                    //if (Error_code == 0)
                    //{
                    //    sid = jMsg["Sid"].ToString();
                    //    Uuid = jMsg["Uuid"].ToInt32();
                    //}
                }
                else if (args.Url.IndexOf("user/mail.asp") != -1)
                {
                    //取限额。post要用
                    /*
                     * <input name="p_id" type="hidden" id="p_id" value="1" />
                       <input name="tt_top" type="hidden" id="tt_top" value="20000" />
                     */
                     p_id = Regex.Match(args.Context, "id=\"p_id\" value=\"([^#> ]+)\"").Groups[1].Value; 
                     tt_top = Regex.Match(args.Context, "id=\"tt_top\" value=\"([^#> ]+)\"").Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR::ChromeBroser_ResponseComplete::" + ex.Message);
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
                        HongHaiMember hx = hx666 as HongHaiMember;
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
                                            if ((int)jsInput.Result.Result == 3)
                                            {
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#username\").value=\'" + name + "';");
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#password\").value=\'" + pass + "';");
                                                var jsYzm = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelector(\"#seccode\").value");
                                                jsYzm.Wait();
                                                var jsYzmResult = jsYzm.Result.Result;
                                                if (jsYzmResult != null)
                                                    if (jsYzmResult.ToString().Count() >= 4)
                                                    {
                                                        p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#gaia_table > tbody > tr:nth-child(8) > td:nth-child(2) > fieldset > a\").click()");

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
                                            //检测到成功登录页面。执行相关动作
                                            //https://pjpctreyoobvf.6f888.net/#/guide
                                            if (p.UrlCurrent.IndexOf("/#/guid") != -1)
                                            {
                                                // p.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.querySelector(\"#app > div:nth-child(1) > div > div > div > div.agree-info > div > div > div > div.tongyi-fan > a:nth-child(2)\").click()");
                                                continue;
                                            }
                                            //进入首页关闭弹窗
                                            //https://pjpctreyoobvf.6f888.net/#/tema
                                            if (p.UrlCurrent.IndexOf("user/mail.asp") != -1)
                                            {
                                                break;
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


        private string GetBetString(BetStandardOrderList items, CarNumEnum car, BetPlayEnum play)
        {
            var item = items.FirstOrDefault(p=>p.car == car && p.play == play);
            if(item != null)
            {
                return Convert.ToString(item.moneySum);
            }
            else
            {
                return "";
            }
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
            BetStatus status = BetStatus.未知;
            if (items.Count <= 0)
                return BetStatus.没有数据;

            int issueid = items[0].IssueId;
            Debug.WriteLine($"::Bet::begin::{issueid}");
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
            Debug.WriteLine($"Bet::合成包");
            int sum = betitems.GetAmountTatol();
            StringBuilder postPackect = new StringBuilder(128);

            //合成发送包
            try
            {
                postPackect.Append($"t_0_22={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.单)}");
                postPackect.Append($"&t_0_23={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.双)}");
                postPackect.Append($"&t_0_24={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.大)}");
                postPackect.Append($"&t_0_25={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.小)}");
                postPackect.Append($"&t_0_26={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.尾大)}");
                postPackect.Append($"&t_0_27={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.尾小)}");
                postPackect.Append($"&t_0_37={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.龙)}");
                postPackect.Append($"&t_0_38={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.虎)}");

                //1车
                postPackect.Append($"&t_1_22={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.单)}");
                postPackect.Append($"&t_1_23={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.双)}");
                postPackect.Append($"&t_1_24={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.大)}");
                postPackect.Append($"&t_1_25={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.小)}");
                postPackect.Append($"&t_1_26={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.尾大)}");
                postPackect.Append($"&t_1_27={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.尾小)}");
                postPackect.Append($"&t_1_28={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.合单)}");
                postPackect.Append($"&t_1_29={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.合双)}");
                postPackect.Append($"&t_1_30={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.福)}");
                postPackect.Append($"&t_1_31={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.禄)}");
                postPackect.Append($"&t_1_32={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.寿)}");
                postPackect.Append($"&t_1_33={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.喜)}");
                //2车
                postPackect.Append($"&t_2_22={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.单)}");
                postPackect.Append($"&t_2_23={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.双)}");
                postPackect.Append($"&t_2_24={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.大)}");
                postPackect.Append($"&t_2_25={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.小)}");
                postPackect.Append($"&t_2_26={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.尾大)}");
                postPackect.Append($"&t_2_27={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.尾小)}");
                postPackect.Append($"&t_2_28={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.合单)}");
                postPackect.Append($"&t_2_29={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.合双)}");
                postPackect.Append($"&t_2_30={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.福)}");
                postPackect.Append($"&t_2_31={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.禄)}");
                postPackect.Append($"&t_2_32={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.寿)}");
                postPackect.Append($"&t_2_33={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.喜)}");
                //3车
                postPackect.Append($"&t_3_22={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.单)}");
                postPackect.Append($"&t_3_23={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.双)}");
                postPackect.Append($"&t_3_24={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.大)}");
                postPackect.Append($"&t_3_25={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.小)}");
                postPackect.Append($"&t_3_26={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.尾大)}");
                postPackect.Append($"&t_3_27={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.尾小)}");
                postPackect.Append($"&t_3_28={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.合单)}");
                postPackect.Append($"&t_3_29={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.合双)}");
                postPackect.Append($"&t_3_30={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.福)}");
                postPackect.Append($"&t_3_31={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.禄)}");
                postPackect.Append($"&t_3_32={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.寿)}");
                postPackect.Append($"&t_3_33={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.喜)}");
                //4车
                postPackect.Append($"&t_4_22={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.单)}");
                postPackect.Append($"&t_4_23={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.双)}");
                postPackect.Append($"&t_4_24={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.大)}");
                postPackect.Append($"&t_4_25={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.小)}");
                postPackect.Append($"&t_4_26={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.尾大)}");
                postPackect.Append($"&t_4_27={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.尾小)}");
                postPackect.Append($"&t_4_28={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.合单)}");
                postPackect.Append($"&t_4_29={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.合双)}");
                postPackect.Append($"&t_4_30={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.福)}");
                postPackect.Append($"&t_4_31={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.禄)}");
                postPackect.Append($"&t_4_32={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.寿)}");
                postPackect.Append($"&t_4_33={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.喜)}");
                //5车
                postPackect.Append($"&t_5_22={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.单)}");
                postPackect.Append($"&t_5_23={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.双)}");
                postPackect.Append($"&t_5_24={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.大)}");
                postPackect.Append($"&t_5_25={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.小)}");
                postPackect.Append($"&t_5_26={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.尾大)}");
                postPackect.Append($"&t_5_27={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.尾小)}");
                postPackect.Append($"&t_5_28={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.合单)}");
                postPackect.Append($"&t_5_29={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.合双)}");
                postPackect.Append($"&t_5_30={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.福)}");
                postPackect.Append($"&t_5_31={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.禄)}");
                postPackect.Append($"&t_5_32={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.寿)}");
                postPackect.Append($"&t_5_33={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.喜)}");

                postPackect.Append($"&p_id={this.p_id}");
                postPackect.Append($"&tt_top={this.tt_top}");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Bet::合成包失败::msg={ex.Message}");
                //记录异常信息
                _Odds.isUpdata = false;
                string jsonitem = JsonConvert.SerializeObject(items);
                loger(Log.Create($"Bet::错误::{betSiteType}赔率获取失败", $"BetStandardOrderList={jsonitem},errmsg={ex.Message}"));
                return BetStatus.赔率获取失败;
            }


            string postdata = postPackect.ToString();

            //发送测试
            string url = $"{urlRoot}/function.asp?action=touzhu";
            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Cookie = browser.Cookies,
                Postdata = postdata,
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7", //"application/json, text/javascript, */*; q=0.01",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                ContentType = "application/x-www-form-urlencoded",   //这个很重要, 没这个, 服务器就无法解析注单
                Referer = $"{urlRoot}/user/mail.asp?p_id={p_id}",
            };
            HttpResult hr = null;
            //解析返回值 ToInt32
            try
            {
                hr = http.GetHtml(item);
                int succeed = hr.Html.IndexOf("投注成功");
                if (succeed > 0)
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
