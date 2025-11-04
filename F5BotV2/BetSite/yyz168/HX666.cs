using CCWin.SkinClass;
using CefSharp;
using CefSharp.WinForms;
using F5BotV2.Boter;
using F5BotV2.CefBrowser;
using F5BotV2.Game.BinGou;
using F5BotV2.Model;
using LxLib.LxNet;
using LxLib.LxSys;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace F5BotV2.BetSite.yyz168
{
    /// <summary>
    ///     海峡666
    ///     搜索页:166y.net
    ///     搜索码:93226
    ///     //会员,代理,线路页面
    ///     //http://166.tt/Site/Show?Token=1c3c13a84b0df2400c9aa0788c54d67a
    /// </summary>
    public class HX666
        : IBetApi
        , INotifyPropertyChanged        //这里的属性监控, 不是为了属性绑定, 所以不传递窗口进来了, 是为了窗口外部监听变化, 而进行处理
    {
        IBetApi betApi; //只要这里面方法, 不要数据, 数据要冲洗你自己



        private float _amount = 0f;
        public float amount
        {
            get { return _amount; }
            set
            {
                if (_amount == value)
                    return;
                _amount = value;
                NotifyPropertyChanged(() => amount);
            }
        }


        private float _unamount = 0f;
        public float unamount {
            get { return _unamount; }
            set
            {
                if (_unamount == value)
                    return;
                _unamount = value;
            }
        }


        private int _Issueid;
        public int Issueid
        {
            get { return _Issueid; }
            set
            {
                if (_Issueid == value)
                    return;
                _Issueid = value;
            }
        }

        private int _PreIssueid;
        public int PreIssueid
        {
            get { return _PreIssueid; }
            set
            {
                if (_PreIssueid == value)
                    return;
                _PreIssueid = value;
            }
        }

        private string _PreLotteryCode = "";
        public string PreLotteryCode
        {
            get { return _PreLotteryCode; }
            set
            {
                if (_PreLotteryCode == value)
                    return;
                _PreLotteryCode = value;
            }
        }



        CancellationTokenSource cts = null;
        IBetBrowserBase browser;
        private string _p_type = "";    //盘口类型
        HX666Odds _Odds = new HX666Odds();

        public HX666(IBetBrowserBase browser)
        {
            betApi = new BetApi(BetSiteType.海峡);
            //betApi.SetRootUrl("https://4921031761-cj.mm666.co");
            betApi.SetRootUrl("https://4921031761-cj.mm666.co/");
            this.browser = browser;
            this.browser.chromeBroser.FrameLoadEnd += ChromeBroser_FrameLoadEnd;
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

        /// <summary>
        ///     处理页面加载事件...
        ///     可以动态处理页面跳转过程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if(e.Url.IndexOf($"/Member/Agreement") >= 0)
            {
                //自动点击同意
                //document.querySelectorAll('.submit_btn')[0].text == 同意
                //document.querySelectorAll('.submit_btn')[0].click()
                Task.Factory.StartNew(() => {
                    var p = browser.chromeBroser;
                    var js_tongyi = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelectorAll('.submit_btn')[0].text");
                    js_tongyi.Wait();
                    var jsText = (string)js_tongyi.Result.Result;
                    if (jsText != null)
                        if (jsText == "同意")
                            p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('.submit_btn')[0].click()");
                });
            }
            else if(e.Url.IndexOf($"{urlRoot}/Home/Index") >= 0)
            {
                //处理弹窗
                Task.Factory.StartNew(() => {
                    var p = browser.chromeBroser;
                    for (int i = 1; i <= 4; i++)
                    {
                        //document.querySelectorAll('#ui-id-4')[0].parentNode.childNodes[1].textContent;
                        string jsquery_textContent = $"document.querySelectorAll('#ui-id-{i}')[0].parentNode.childNodes[1].textContent";
                        string jsquery_click = $"document.querySelectorAll('#ui-id-{i}')[0].parentNode.childNodes[1].click()";
                        var js_tongyi = p.GetBrowser().MainFrame.EvaluateScriptAsync(jsquery_textContent);
                        js_tongyi.Wait();
                        var jsText = (string)js_tongyi.Result.Result;
                        if (jsText != null)
                            if (jsText == "Close")
                                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync(jsquery_click);
                    }


                });
            }
            else if(e.Url.IndexOf($"?ReturnUrl=/Home/Index") >= 0)
            {
                //被挤下线了，需要重新登录

            }
        }

        public string urlRoot => betApi.urlRoot;

        public string cookie => betApi.cookie;

        public BetSiteType betSiteType => betApi.betSiteType;

        public bool isLoginSuccess { get => betApi.isLoginSuccess; set => betApi.isLoginSuccess = value; }



        /// <summary>
        ///     登录海峡官网
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <param name="browser"></param>
        /// <returns></returns>
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
                                    if(jsInput != null)
                                        if(jsInput.Result != null)
                                        {
                                            if((int)jsInput.Result.Result == 4)
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

        /// <summary>
        ///     获取页面数据.得到 盘口类型
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     获取站点赔率, 赔率用字典来保存
        /// </summary>
        /// <returns></returns>
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
            if(_p_type != "A" && _p_type != "B" && _p_type != "C" && _p_type != "D")
            {
                _p_type = GetIndex();
            }

            if (!_Odds.isUpdata)
            {
                //_p_type = "A";
                if (_Odds.GetUpdata($"{urlRoot}/PlaceBet/Loaddata?lotteryType=TWBINGO", browser.Cookies, _p_type))
                {
                    _Odds.isUpdata = true;
                }
            }
               

            //开奖数据
            //GetCurrentOpenLotteryData();
            return _Odds.isUpdata;
        }

        /// <summary>
        ///     得到当期开奖数据, 期号
        /// </summary>
        public bool GetCurrentOpenLotteryData()
        {
            /*
             *  {
                    "Installments": "112052324",
                    "State": 1,
                    "CloseTimeStamp": 12,
                    "OpenTimeStamp": 42,
                    "PreLotteryResult": "28,70,10,60,52",
                    "PreInstallments": "112052323",
                    "TemplateCode": "BINGO"}
             */
            //https://8575517633-cj.mm666.co/PlaceBet/GetCurrentInstall?lotteryType=TWBINGO
            bool respone = true;
            try
            {
                string url = $"{urlRoot}/PlaceBet/GetCurrentInstall?lotteryType=TWBINGO";
                LxHttpHelper http = new LxHttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Cookie = browser.Cookies,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                    //Postdata = $"lotteryType=TWBINGO",
                };
                var hr1 = http.GetHtml(item);
                JObject jRet = JObject.Parse(hr1.Html);
                Issueid = Convert.ToInt32(jRet["Installments"]);
                PreIssueid = Convert.ToInt32(jRet["PreInstallments"]);
                PreLotteryCode = jRet["PreLotteryResult"].ToString();
            }
            catch(Exception ex)
            {
                respone = false;
            }
            return respone;
        }


        public bool GetUserInfoUpdata()
        {
            //https://8575517633-cj.mm666.co/PlaceBet/QueryResult?lotteryType=TWBINGO
            /* //返回值
{
    "Result": 0.0,
    "UnResult": 81.0,           //未结算金额
    "accountLimit": 49919.0,
    "AccType": 0,
    "accountLimitList": {
        "accountLimit_2": 0.0,
        "accountLimit_0": 49919.0 }}
             */
            bool response = true;
            HttpResult hr1 = null;
            try
            {
                string url = $"{urlRoot}/PlaceBet/QueryResult?lotteryType=TWBINGO";
                LxHttpHelper http = new LxHttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Cookie = browser.Cookies,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                    //Postdata = $"lotteryType=TWBINGO",
                };
                hr1 = http.GetHtml(item);
                //解析, 更新到类变量中
                JObject jRet = JObject.Parse(hr1.Html);
                amount = Convert.ToSingle(jRet["accountLimit"]);
                unamount = Convert.ToSingle(jRet["UnResult"]);
            }
            catch(Exception ex)
            {
                if(hr1 != null)
                    loger(Log.Create($"Bet::更新数据::{betSiteType}返回异常", $"hr={hr1.Html}"));
                response = false;
            }
            return response;
        }


        public void SetCookie(string cookie)
        {
            betApi.SetCookie(cookie);
        }

        public bool SetRootUrl(string url)
        {
            return betApi.SetRootUrl(url);
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
            foreach(var bsOrder in items)
            {
                var last = betitems.LastOrDefault();
                if(last == null)
                {
                    betitems.Add(bsOrder);
                }
                else
                {
                    if(last.car == bsOrder.car && last.play == bsOrder.play)
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
            catch(Exception ex)
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
                sbPost.Append($"&gt={_p_type}");
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
            catch(Exception ex)
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

        public void Cancel()
        {
            try
            {
                cts.Cancel();
            }
            catch
            {

            }
            
        }

        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play)
        {
            return betApi.GetOdds(car, play);
        }
    }


    /// <summary>
    ///     降序排列
    /// </summary>
    public class BetStandardOrderComparer : IComparer<BetStandardOrder>
    {
        public int Compare(BetStandardOrder x, BetStandardOrder y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return 0;
            }
            //从1号车在前面, 大的在后面
            if (x.car > y.car)
                return 1;
            if(x.car < y.car)
                return -1;

            //如果等于
            if (x.play > y.play)
                return 1;
            if (x.play < y.play)
                return -1;


            return 0;
        }
    }

}
