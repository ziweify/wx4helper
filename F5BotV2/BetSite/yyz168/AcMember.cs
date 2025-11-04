using CCWin.SkinClass;
using CefSharp;
using CefSharp.WinForms;
using CsQuery.Utility;
using F5BotV2.BetSite.yyz168;
using F5BotV2.CefBrowser;
using F5BotV2.Model;
using LxLib.LxNet;
using LxLib.LxSys;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace F5BotV2.BetSite.yyz168
{
    /// <summary>
    ///     属于海峡的
    /// </summary>
    public class AcMember
        : IBetApi
        , INotifyPropertyChanged
    {
        private CancellationTokenSource cts = null;
        private IBetBrowserBase browser;
        private string _pk = "";    //盘口类型
        HX666Odds _Odds = new HX666Odds();


        IBetApi betApi { get; set; }

        public string urlRoot => betApi.urlRoot;

        public float amount => betApi.amount;

        public string cookie => betApi.cookie;

        public BetSiteType betSiteType => betApi.betSiteType;

        public bool isLoginSuccess { get => betApi.isLoginSuccess; set => betApi.isLoginSuccess = value; }
      

        public AcMember(IBetBrowserBase browser)
        {
            betApi = new BetApi(BetSiteType.AC);
            SetRootUrl("https://3151135604-acyl.yy777.co/");
            this.browser = browser;
            this.browser.chromeBroser.FrameLoadEnd += ChromeBroser_FrameLoadEnd;
            this.browser.chromeBroser.RequestHandler = new CefRequestHandler(ChromeBroser_ResponseComplete);

        }

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

        private void ChromeBroser_ResponseComplete(object sender, ResponseEventArgs args)
        {
            //处理资源
            try
            {
                //https://3151135604-acyl.yy777.co/PlaceBet/Index?lotteryType=TWBINGO&page=zp&pk=A
                if (args.Url.IndexOf("PlaceBet/Index?lotteryType") != -1)
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
                    await browser.LoadUrlAsyn(urlRoot, new Func<ChromiumWebBrowser, bool>((p) =>
                    {
                        HX666 hx = hx666 as HX666;
                        bool response = true;
                        try
                        {
                            //由于加载了验证页面, 判断是否是正确登录页
                            using (cts.Token.Register((Thread.CurrentThread.Abort)))
                            {
                                //这个线程就一直给他跑, 不返回了
                                while (true)
                                {
                                    //if (cts.Token.IsCancellationRequested)
                                    //    break;

                                    //自动登录代码
                                    var jsInput = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelectorAll('input').length");
                                    if (jsInput != null)
                                        if (jsInput.Result != null)
                                        {
                                            if ((int)jsInput.Result.Result == 4)
                                            {
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('input')[0].value = '" + name + "';");
                                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('input')[1].value = '" + pass + "';");
                                                var jsYzm = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelectorAll('input')[2].value");
                                                jsYzm.Wait();
                                                var jsYzmResult = jsYzm.Result.Result;
                                                if (jsYzmResult != null)
                                                    if (jsYzmResult.ToString().Count() >= 4)
                                                    {
                                                        p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('input')[3].click();");

                                                        //通知UI登录成功
                                                        hx.isLoginSuccess = true;
                                                        //获取赔率
                                                        hx.GetOdds();
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

        public bool GetOdds()
        {
            //执行需要返回的脚本指令
            //int issueid = BinGouHelper.getNextIssueId(DateTime.Now);
            //var p = browser.chromeBroser;
            //StringBuilder jsScript = new StringBuilder(64);
            //jsScript.Append("const hxp = new XMLHttpRequest();");
            //jsScript.Append("hxp.open('POST', 'https://4921031761-cj.mm666.co/PlaceBet/Loaddata?lotteryType=TWBINGO');");
            //jsScript.Append("hxp.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');");
            //jsScript.Append($"hxp.send('itype=-1&settingCode=LM%2CWH%2CFLSX%2CLH&oddstype=A&lotteryType=TWBINGO&install={issueid}');");
            if (_pk != "A" && _pk != "B" && _pk != "C" && _pk != "D")
            {
                _pk = GetIndex();
            }

            if (!_Odds.isUpdata)
            {
                //_p_type = "A";
                if (_Odds.GetUpdata($"{urlRoot}/PlaceBet/Loaddata?lotteryType=TWBINGO", browser.Cookies, _pk))
                {
                    _Odds.isUpdata = true;
                }
            }


            //开奖数据
            //GetCurrentOpenLotteryData();
            return _Odds.isUpdata;
        }

        private string GetIndex()
        {
            string url = $"{urlRoot}/Home/Index";
            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,
                Method = "GET",
                Cookie = browser.Cookies,
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
            };
            var hr = http.GetHtml(item);
            //<li class="p_curr">A</li>
            string rgxStr = @"p_curr.>(.*?)<";
            var rgxRet = Regex.Match(hr.Html, rgxStr);
            var p_leixing = rgxRet.Groups[1].Value;
            return p_leixing;
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
            BetStatus status = BetStatus.未知;
            if (!GetOdds())
            {
                loger(Log.Create($"Bet::错误::{urlRoot}::{betSiteType}::赔率获取失败", ""));
                return BetStatus.赔率获取失败;
            }


            //string jsItem = JsonConvert.SerializeObject(items);
            items.Sort(new BetStandardOrderComparer());
            //jsItem = JsonConvert.SerializeObject(items);
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
            StringBuilder sbPost = new StringBuilder(128);
            //合成发送包
            try
            {
                for (int i = 0; i < betitems.Count; i++)
                {
                    if (sbPost.Length > 0)
                        sbPost.Append("&");
                    var ibettem = betitems[i];
                    var ods = _Odds.GetOdds(ibettem.car, ibettem.play);     //填充赔率
                    //betdata%5B0%5D%5BAmount%5D=10&betdata%5B0%5D%5BKeyCode%5D=B1DS_D&betdata%5B0%5D%5BOdds%5D=1.97
                    sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][Amount]") + $"={betitems[i].moneySum}" + "&");
                    sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][KeyCode]") + $"={ods.oddsName}" + "&");
                    sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][Odds]") + $"={ods.odds}");
                }
            }
            catch (Exception ex)
            {
                //记录异常信息
                _Odds.isUpdata = false;
                string jsonitem = JsonConvert.SerializeObject(items);
                loger(Log.Create($"Bet::错误::{betSiteType}赔率获取失败", $"BetStandardOrderList={jsonitem},errmsg={ex.Message}"));
                return BetStatus.赔率获取失败;
            }


            if (sbPost.Length > 0)
            {
                sbPost.Append($"&lotteryType=TWBINGO");
                sbPost.Append($"&betNum=10{LxTimestampHelper.GetTimeStamp13()}");          //15位时间戳
                sbPost.Append($"&prompt=true");
                sbPost.Append($"&gt={_pk}");
            }

            string postdata = sbPost.ToString();

            //发送测试
            string url = $"{urlRoot}/PlaceBet/Confirmbet?lotteryType=TWBINGO";
            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Cookie = browser.Cookies,
                Postdata = postdata,
                Accept = "application/json, text/javascript, */*; q=0.01",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",   //这个很重要, 没这个, 服务器就无法解析注单
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
                int succeed = jResult["succeed"].ToInt32();
                if (succeed == 1)
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

        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play)
        {
            return betApi.GetOdds(car, play);
        }

        public void loger(Log log)
        {
            betApi.loger(log);
        }

        public void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            betApi.ChromeBroser_FrameLoadEnd(sender, e);
        }
    }
}
