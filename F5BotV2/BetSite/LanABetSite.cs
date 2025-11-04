using CefSharp;
using CefSharp.WinForms;
using F5BotV2.BetSite.yyz168;
using F5BotV2.CefBrowser;
using F5BotV2.Model;
using LxLib.LxNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace F5BotV2.BetSite
{
    /// <summary>
    ///     蓝盘A类
    /// </summary>
    public class LanABetSite
        : IBetApi
    {
        IBetBrowserBase browser;
        private CancellationTokenSource cts = null;
        protected IBetApi betApi { get; set; }

        public string urlRoot => betApi.urlRoot;

        public float amount => betApi.amount;

        public string cookie => betApi.cookie;

        public BetSiteType betSiteType => betApi.betSiteType;

        public bool isLoginSuccess { get => betApi.isLoginSuccess; set => betApi.isLoginSuccess = value; }

        public LanABetSite(IBetBrowserBase browser)
        {
            betApi = new BetApi(BetSiteType.蓝A);
            this.browser = browser;
            this.browser.chromeBroser.FrameLoadEnd += ChromeBroser_FrameLoadEnd;
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

        //要获取的几个私有变量才能投注
        private int p_id = 1;
        private int tt_top = 20000;

        public int LoginAsync(string name, string pass, IBetBrowserBase browser)
        {

            Debug.WriteLine("LoginAsync::进入");
            if (cts == null)
            {
                cts = new CancellationTokenSource();
                Task.Factory.StartNew(async (object betSite) => {
                    await browser.LoadUrlAsyn(urlRoot, new Func<ChromiumWebBrowser, bool>((p) =>
                    {
                        LanABetSite hx = betSite as LanABetSite;
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
                                            if ((int)jsInput.Result.Result == 5)
                                            {
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#username\").value = '" + name + "';");
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#password\").value = '" + pass + "';");
                                                var jsYzm = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelector(\"#seccode\").value");
                                                jsYzm.Wait();
                                                var jsYzmResult = jsYzm.Result.Result;
                                                if (jsYzmResult != null)
                                                    if (jsYzmResult.ToString().Count() >= 4)
                                                    {
                                                        p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelector(\"#gaia_table > tbody > tr:nth-child(8) > td:nth-child(2) > fieldset > input[type=button]\").click();");

                                                        //通知UI登录成功
                                                        hx.isLoginSuccess = true;
                                                        //获取赔率
                                                        //hx.GetOdds();
                                                        break;
                                                    }
                                                //获取验证码
                                                Debug.Write("等待输入验证码!");
                                                //点击登录

                                                //退出过程
                                            }
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
            return betApi.GetUserInfoUpdata();
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
            //投注
            //http://8.138.59.115:66/api/ynk3/gxklsf.ashx
            BetStatus status = BetStatus.未知;
            items.Sort(new BetStandardOrderComparer());

            int issueid = 0;

            //合并订单
            BetStandardOrderList betitems = new BetStandardOrderList();
            foreach (var bsOrder in items)
            {
                if (issueid == 0)
                    issueid = bsOrder.IssueId;
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

            //订单总金额.
            int sum = betitems.GetAmountTatol();
            StringBuilder sbPost = new StringBuilder(128);
            //合成发送封包
            try
            {
                //初始化包结构内容
                Dictionary<string, string> betdata = new Dictionary<string, string>();
                betdata.Add("t_0_11", "");
                betdata.Add("t_0_12", "");
                betdata.Add("t_0_13", "");
                betdata.Add("t_0_14", "");
                betdata.Add("t_0_26", "");
                betdata.Add("t_0_27", "");
                betdata.Add("t_0_37", "");
                betdata.Add("t_0_38", "");
                //1车
                betdata.Add("t_1_11", "");
                betdata.Add("t_1_12", "");
                betdata.Add("t_1_13", "");
                betdata.Add("t_1_14", "");
                betdata.Add("t_1_34", "");
                betdata.Add("t_1_35", "");
                betdata.Add("t_1_36", "");
                betdata.Add("t_1_15", "");
                betdata.Add("t_1_16", "");
                betdata.Add("t_1_28", "");
                betdata.Add("t_1_29", "");
                betdata.Add("t_1_30", "");
                betdata.Add("t_1_31", "");
                betdata.Add("t_1_32", "");
                betdata.Add("t_1_33", "");
                //2车
                betdata.Add("t_2_11", "");
                betdata.Add("t_2_12", "");
                betdata.Add("t_2_13", "");
                betdata.Add("t_2_14", "");
                betdata.Add("t_2_34", "");
                betdata.Add("t_2_35", "");
                betdata.Add("t_2_36", "");
                betdata.Add("t_2_15", "");
                betdata.Add("t_2_16", "");
                betdata.Add("t_2_28", "");
                betdata.Add("t_2_29", "");
                betdata.Add("t_2_30", "");
                betdata.Add("t_2_31", "");
                betdata.Add("t_2_32", "");
                betdata.Add("t_2_33", "");
                //3车
                betdata.Add("t_3_11", "");
                betdata.Add("t_3_12", "");
                betdata.Add("t_3_13", "");
                betdata.Add("t_3_14", "");
                betdata.Add("t_3_34", "");
                betdata.Add("t_3_35", "");
                betdata.Add("t_3_36", "");
                betdata.Add("t_3_15", "");
                betdata.Add("t_3_16", "");
                betdata.Add("t_3_28", "");
                betdata.Add("t_3_29", "");
                betdata.Add("t_3_30", "");
                betdata.Add("t_3_31", "");
                betdata.Add("t_3_32", "");
                betdata.Add("t_3_33", "");
                //4车
                betdata.Add("t_4_11", "");
                betdata.Add("t_4_12", "");
                betdata.Add("t_4_13", "");
                betdata.Add("t_4_14", "");
                betdata.Add("t_4_34", "");
                betdata.Add("t_4_35", "");
                betdata.Add("t_4_36", "");
                betdata.Add("t_4_15", "");
                betdata.Add("t_4_16", "");
                betdata.Add("t_4_28", "");
                betdata.Add("t_4_29", "");
                betdata.Add("t_4_30", "");
                betdata.Add("t_4_31", "");
                betdata.Add("t_4_32", "");
                betdata.Add("t_4_33", "");
                //5车
                betdata.Add("t_5_11", "");
                betdata.Add("t_5_12", "");
                betdata.Add("t_5_13", "");
                betdata.Add("t_5_14", "");
                betdata.Add("t_5_34", "");
                betdata.Add("t_5_35", "");
                betdata.Add("t_5_36", "");
                betdata.Add("t_5_15", "");
                betdata.Add("t_5_16", "");
                betdata.Add("t_5_28", "");
                betdata.Add("t_5_29", "");
                betdata.Add("t_5_30", "");
                betdata.Add("t_5_31", "");
                betdata.Add("t_5_32", "");
                betdata.Add("t_5_33", "");

                //开始和成包
                foreach(var betItem in betitems)
                {   
                    if (betItem.car == CarNumEnum.P总 && betItem.play == BetPlayEnum.单) betdata["t_0_11"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P总 && betItem.play == BetPlayEnum.双) betdata["t_0_12"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P总 && betItem.play == BetPlayEnum.大) betdata["t_0_13"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P总 && betItem.play == BetPlayEnum.小) betdata["t_0_14"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P总 && betItem.play == BetPlayEnum.尾大) betdata["t_0_26"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P总 && betItem.play == BetPlayEnum.尾小) betdata["t_0_27"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P总 && betItem.play == BetPlayEnum.龙) betdata["t_0_37"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P总 && betItem.play == BetPlayEnum.虎) betdata["t_0_38"] = Convert.ToString(betItem.moneySum);

                    else if(betItem.car == CarNumEnum.P1 && betItem.play == BetPlayEnum.单)  betdata["t_1_11"] = Convert.ToString(betItem.moneySum);
                    else if(betItem.car == CarNumEnum.P1 && betItem.play == BetPlayEnum.双) betdata["t_1_12"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P1 && betItem.play == BetPlayEnum.大) betdata["t_1_13"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P1 && betItem.play == BetPlayEnum.小) betdata["t_1_14"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P1 && betItem.play == BetPlayEnum.尾大) betdata["t_1_15"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P1 && betItem.play == BetPlayEnum.尾小) betdata["t_1_16"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P1 && betItem.play == BetPlayEnum.合单) betdata["t_1_28"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P1 && betItem.play == BetPlayEnum.合双) betdata["t_1_29"] = Convert.ToString(betItem.moneySum);

                    else if (betItem.car == CarNumEnum.P2 && betItem.play == BetPlayEnum.单) betdata["t_2_11"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P2 && betItem.play == BetPlayEnum.双) betdata["t_2_12"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P2 && betItem.play == BetPlayEnum.大) betdata["t_2_13"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P2 && betItem.play == BetPlayEnum.小) betdata["t_2_14"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P2 && betItem.play == BetPlayEnum.尾大) betdata["t_2_15"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P2 && betItem.play == BetPlayEnum.尾小) betdata["t_2_16"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P2 && betItem.play == BetPlayEnum.合单) betdata["t_2_28"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P2 && betItem.play == BetPlayEnum.合双) betdata["t_2_29"] = Convert.ToString(betItem.moneySum);

                    else if (betItem.car == CarNumEnum.P3 && betItem.play == BetPlayEnum.单) betdata["t_3_11"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P3 && betItem.play == BetPlayEnum.双) betdata["t_3_12"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P3 && betItem.play == BetPlayEnum.大) betdata["t_3_13"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P3 && betItem.play == BetPlayEnum.小) betdata["t_3_14"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P3 && betItem.play == BetPlayEnum.尾大) betdata["t_3_15"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P3 && betItem.play == BetPlayEnum.尾小) betdata["t_3_16"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P3 && betItem.play == BetPlayEnum.合单) betdata["t_3_28"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P3 && betItem.play == BetPlayEnum.合双) betdata["t_3_29"] = Convert.ToString(betItem.moneySum);

                    else if (betItem.car == CarNumEnum.P4 && betItem.play == BetPlayEnum.单) betdata["t_4_11"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P4 && betItem.play == BetPlayEnum.双) betdata["t_4_12"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P4 && betItem.play == BetPlayEnum.大) betdata["t_4_13"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P4 && betItem.play == BetPlayEnum.小) betdata["t_4_14"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P4 && betItem.play == BetPlayEnum.尾大) betdata["t_4_15"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P4 && betItem.play == BetPlayEnum.尾小) betdata["t_4_16"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P4 && betItem.play == BetPlayEnum.合单) betdata["t_4_28"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P4 && betItem.play == BetPlayEnum.合双) betdata["t_4_29"] = Convert.ToString(betItem.moneySum);

                    else if (betItem.car == CarNumEnum.P5 && betItem.play == BetPlayEnum.单) betdata["t_5_11"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P5 && betItem.play == BetPlayEnum.双) betdata["t_5_12"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P5 && betItem.play == BetPlayEnum.大) betdata["t_5_13"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P5 && betItem.play == BetPlayEnum.小) betdata["t_5_14"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P5 && betItem.play == BetPlayEnum.尾大) betdata["t_5_15"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P5 && betItem.play == BetPlayEnum.尾小) betdata["t_5_16"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P5 && betItem.play == BetPlayEnum.合单) betdata["t_5_28"] = Convert.ToString(betItem.moneySum);
                    else if (betItem.car == CarNumEnum.P5 && betItem.play == BetPlayEnum.合双) betdata["t_5_29"] = Convert.ToString(betItem.moneySum);

                }

                foreach(var data in betdata)
                {
                    if(sbPost.Length > 0)
                        sbPost.Append("&");
                    sbPost.Append($"{data.Key}={data.Value}");
                }
                sbPost.Append($"&p_id={p_id}");
                sbPost.Append($"&tt_top={tt_top}");
                sbPost.Append($"&action=submit");
                sbPost.Append($"&now_sale_qishu={issueid}");

                //发送包
                if(issueid > 0)
                {
                    string postdata = sbPost.ToString();

                    //发送测试
                    string url = $"{urlRoot}/api/ynk3/gxklsf.ashx";
                    string referer = $"{urlRoot}/gxklsf.html"; ;//http://8.138.59.115:66/gxklsf.html
                    LxHttpHelper http = new LxHttpHelper();
                    HttpItem item = new HttpItem()
                    {
                        URL = url,
                        Method = "POST",
                        Cookie = browser.Cookies,
                        Postdata = postdata,
                        Accept = "*/*", // Accept = "application/json, text/javascript, */*; q=0.01",
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                        ContentType = "application/x-www-form-urlencoded; charset=UTF-8",   //这个很重要, 没这个, 服务器就无法解析注单,
                        Referer = referer,
                    };
                    var hr = http.GetHtml(item);
                    return BetStatus.成功;
                }
            }
            catch (Exception ex)
            {
                //记录异常信息
                //_Odds.isUpdata = false;
                string jsonitem = JsonConvert.SerializeObject(items);
                loger(Log.Create($"Bet::错误::{betSiteType}可能赔率获取失败", $"BetStandardOrderList={jsonitem},errmsg={ex.Message}"));
                return BetStatus.失败;
            }

            return BetStatus.失败;
        }

        public void loger(Log log)
        {
            betApi.loger(log);
        }


        public void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Url.IndexOf($"/default.html") >= 0)
            {
                //点击获取
                //Task.Factory.StartNew(() => {
                //    var p = browser.chromeBroser;
                //    var js_tongyi = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelectorAll('.submit_btn')[0].text");
                //    js_tongyi.Wait();
                //    var jsText = (string)js_tongyi.Result.Result;
                //    if (jsText != null)
                //        if (jsText == "同意")
                //            p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('.submit_btn')[0].click()");
                //});
                //document.querySelector("#menu_s > ul > li:nth-child(2) > a > img")
                
            }
            else if(e.Url.IndexOf("gxklsf.html") >= 0)
            {
                //var victor = new TaskStringVisitor();
                //browser.chromeBroser.GetMainFrame().GetSource(victor);
                //victor.Task.Wait();
                //string src = victor.Task.Result;
            }
        }

        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play)
        {
            return betApi.GetOdds(car, play);
        }
    }
}
