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
using System.Text.RegularExpressions;
using CCWin.SkinClass;

namespace F5BotV2.BetSite
{
    /// <summary>
    ///     测试账号
    ///     账:tt97
    ///     密:979797
    /// </summary>

    public class ADKMember
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
        public ADKMember(IBetBrowserBase browser)
        {
            betApi = new BetApi(BetSiteType.ADK);
            SetRootUrl("https://yk.adkdkdkd.com/");
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
                        ADKMember hx = hx666 as ADKMember;
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
                                            if ((int)jsInput.Result.Result == 4)
                                            {
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#txtUsername\").value = '" + name + "';");
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#txtPass\").value = '" + pass + "';");
                                                var jsYzm = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelector(\"#VerifyCode\").value");
                                                jsYzm.Wait();
                                                var jsYzmResult = jsYzm.Result.Result;
                                                if (jsYzmResult != null)
                                                    if (jsYzmResult.ToString().Count() >= 4)
                                                    {
                                                        p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#submit1\").click()");

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
            var item = items.FirstOrDefault(p => p.car == car && p.play == play);
            if (item != null)
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
           //发包参数
            string qihaoid = "";
            string mysession = "";

            //得到期号ID
            LxHttpHelper http = new LxHttpHelper();
            var hrQihao = http.GetHtml(new HttpItem()
            {
                //https://yk.adkdkdkd.com/source/AjaxShiFen.ashx
                Method = "POST",
                URL = $"{urlRoot}/source/AjaxShiFen.ashx",
                Cookie = browser.Cookies,
                Referer = $"{urlRoot}/default.html",
                Accept = "text/plain, */*; q=0.01"
            });
            string[] sfens = hrQihao.Html.Split('|');
            if(sfens.Length > 2) {
                if (sfens[2].Replace(" ", "") == issueid.ToString().Replace(" ", ""))
                {
                    qihaoid = sfens[4];
                }
            }
            //得到包参数
            Debug.WriteLine($"Bet::得到包参数");
            //https://yk.adkdkdkd.com/default.html
            var hrParam = http.GetHtml(new HttpItem() { 
                Method = "GET",
                URL = $"{urlRoot}/default.html",
                Cookie = browser.Cookies,
                Referer = $"{urlRoot}/default.html",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"
            });
            mysession = Regex.Match(hrParam.Html, @"name=""mysession""[^#]+value=""([^#]+)""").Groups[1].Value;




            Debug.WriteLine($"Bet::合成包");
            int sum = betitems.GetAmountTatol();
            StringBuilder postPackect = new StringBuilder(128);

            //合成发送包
            try
            {
                postPackect.Append($"txtnumber={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.大)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.单)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.尾大)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.龙)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.小)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.双)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.尾小)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P总, BetPlayEnum.虎)}");


                //大
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.大)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.大)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.大)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.大)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.大)}");
                //小
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.小)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.小)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.小)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.小)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.小)}");
                //单
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.单)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.单)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.单)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.单)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.单)}");
                //双
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.双)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.双)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.双)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.双)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.双)}");
                //尾大
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.尾大)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.尾大)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.尾大)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.尾大)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.尾大)}");
                //尾小
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.尾小)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.尾小)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.尾小)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.尾小)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.尾小)}");
                //合单
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.合单)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.合单)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.合单)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.合单)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.合单)}");
                //合双
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P1, BetPlayEnum.合双)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P2, BetPlayEnum.合双)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P3, BetPlayEnum.合双)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P4, BetPlayEnum.合双)}");
                postPackect.Append($"&txtnumber={GetBetString(betitems, CarNumEnum.P5, BetPlayEnum.合双)}");
                //其他玩法的
                for(int i = 0; i < 200; i++)
                {
                    postPackect.Append($"&txtnumber=");
                }
              
                postPackect.Append($"&txtnumber2=&txtvalue_258=&txtnumber=&txtvalue_248=&txtnumber=&txtvalue_249=&txtnumber=&txtvalue_250=&txtnumber=&txtvalue_251=&txtnumber=&txtvalue_252=&txtnumber=&txtvalue_253=&txtnumber=&txtvalue_254=&txtnumber=&txtvalue_255=&txtnumber=&txtvalue_256=&txtnumber=&txtvalue_257=");
                postPackect.Append($"&qihaoid=572258&mysession=638525358421932853");

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
            string url = $"{urlRoot}/default.html?method=add";
            //LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Cookie = browser.Cookies,
                Postdata = postdata,
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7", //"application/json, text/javascript, */*; q=0.01",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                ContentType = "application/x-www-form-urlencoded",   //这个很重要, 没这个, 服务器就无法解析注单
                Referer = $"{urlRoot}/default.html",
            }; 
            HttpResult hr = null;
            //解析返回值 ToInt32
            try
            {
                //投递成功返回 = {"error":false,"msg":null,"url":"default.html","Data":null}
                //投递失败返回 = {"error":true,"msg":"错误：投注金额必须大于等于20！","url":null,"Data":null}
                hr = http.GetHtml(item);
                JObject jBetRet = JObject.Parse(hr.Html);
                bool error = Convert.ToBoolean(jBetRet["error"].ToString());
                if (!error)
                {
                    loger(Log.Create($"Bet::POST::{betSiteType}成功", $"hr={hr.Html}"));
                    status = BetStatus.成功;
                }
                else
                {
                    //string msg = jBetRet["msg"].ToString();
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
