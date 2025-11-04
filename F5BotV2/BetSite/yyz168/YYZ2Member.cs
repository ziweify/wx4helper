using CCWin.SkinClass;
using CefSharp;
using CefSharp.WinForms;
using CsQuery;
using F5BotV2.CefBrowser;
using F5BotV2.Model;
using LxLib.LxNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LxHttpHelper = LxLib.LxNet.LxHttpHelper;

namespace F5BotV2.BetSite.yyz168
{
    public class YYZ2Member
        : IBetApi
    {
        IBetApi betApi { get; set; }

        public string urlRoot => betApi.urlRoot;

        public string cookie => betApi.cookie;

        public BetSiteType betSiteType => betApi.betSiteType;

        public bool isLoginSuccess { get => betApi.isLoginSuccess; set => betApi.isLoginSuccess = value; }

        public float amount => throw new NotImplementedException();

        //测试账号: wwww11
        //    密码: Aaa123
        public YYZ2Member()
        {
            betApi = new BetApi( BetSiteType.元宇宙2);
            SetRootUrl("http://yyz.168app.net/2/");
        }


        //这个是自动登录...
        //测试账号
        //账:ssww168
        //密:Ssww168
        //地址:http://yyz.168app.net/2/
        /// <summary>
        ///     返回0表示成功, 返回其他值, 表示其他错误
        ///     错误码 600开头
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public int LoginAsync(string name, string pwd, IBetBrowserBase browser)
        {
            //获取会员线路
            string p1MemberUrl = getMemberLine();
            if (string.IsNullOrEmpty(p1MemberUrl))
                return 600;
            Task.Factory.StartNew(async () => {
               await browser.LoadUrlAsyn(p1MemberUrl, new Func<ChromiumWebBrowser, bool>((p) =>
                {
                    //1、得到验证码
                    string yzmBase64 = getImageBase64(p);

                    //2、显示验证码处理窗口
                    var image = LxHttpHelperEx.Base64ToImage(yzmBase64, ImageFormat.Png);
                    string yzmCode = "";

                    //::弹窗输入验证码开始::
                    //var codeinputResult = browser.ShowCodeInput(image, out yzmCode, new Func<Image>(() =>
                    //{
                    //    //刷新验证码
                    //    Image imageRet = null;
                    //    string script = "getImg()";
                    //    var js = p.EvaluateScriptAsPromiseAsync(script);
                    //    js.Wait();

                    //    string imageStr = getImageBase64(p);
                    //    imageRet = LxHttpHelperEx.Base64ToImage(imageStr, ImageFormat.Png);
                    //    return imageRet;
                    //}));
                    //if (codeinputResult == System.Windows.Forms.DialogResult.OK)
                    //{
                    //    if (!string.IsNullOrEmpty(yzmCode))
                    //    {
                    //        p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('input')[2].value = '" + yzmCode + "';");
                    //        //点击登录按钮
                    //        try
                    //        {
                    //            var jsLogin = p.GetBrowser().MainFrame.EvaluateScriptAsync($"login()");
                    //            if (jsLogin != null)
                    //            {
                    //                //接管弹窗
                    //                jsLogin.Wait();
                    //                //释放接管弹窗

                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {

                    //        }
                    //    }
                    //}
                    //::弹窗输入验证码结束::

                    //自动登录代码
                    p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('input')[0].value = '" + name + "';");
                    p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('input')[1].value = '" + pwd + "';");



                    return true;
                }));
            });
            //打开会员线路1
           
            Debug.WriteLine("Login::退出");
            return 0;
        }

        private string getImageBase64(ChromiumWebBrowser p)
        {
            //处理验证码
            string yzmBase64 = "";
            var tsSource = p.GetBrowser().MainFrame.GetSourceAsync();
            tsSource.Wait();
            var htmlCode = tsSource.Result;
            //使用CsQuery库来解析html
            var jqHtml = CQ.CreateDocument(htmlCode);
            var jqLoginBox = jqHtml[".login_box>ul>li"];
            //遍历li,得到验证码行
            foreach (var li in jqLoginBox)
            {
                var html = li.OuterHTML;
                //继续分析子项
                var jqLi = CQ.Create(html);
                var text = jqLi["li>i"].Html();
                if (text.IndexOf("验证码") != -1)
                {
                    //提取验证码
                    try
                    {
                        var yzmCode = jqLi["li>.code>img"].Each(x =>
                        {
                            yzmBase64 = x.GetAttribute("src");
                        });
                    }
                    catch (Exception ex)
                    {

                    }

                    break;
                }
            }
            return yzmBase64;
        }

        private string getMemberLine()
        {
            string p1Url = "";  //返回值, 会员线路1

            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = urlRoot,
                Timeout = 10000
            };
            HttpResult hrLine = http.GetHtml(item);
            if (hrLine == null)
                return "";

            CsQuery.Config.HtmlEncoder = CsQuery.HtmlEncoders.None;
            CsQuery.Config.OutputFormatter = CsQuery.OutputFormatters.HtmlEncodingNone;

            //使用CsQuery库来解析html
            var jqLine = CQ.CreateDocument(hrLine.Html);
            var strLine = jqLine[".main_con>.url_list>.mtable>tbody>tr"];

            //遍历线路.会员线路
            foreach (var els in strLine.Elements)
            {
                var html = els.OuterHTML;
                //继续查找子项, 由于里面都是td, 所以需要重新创建一个查询对象, 再查询对象里面继续操作
                var jqTc = CQ.Create(els.OuterHTML);
                var tc = jqTc[".tc"].Html();
                if (tc == "会员线路1")
                {
                    p1Url = els.GetAttribute("accesskey");
                    break;
                }
            }


            return p1Url;
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
            betApi.ChromeBroser_FrameLoadEnd(sender, e);
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
            BetStatus status = BetStatus.未知;
            betApi.Bet(items);
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
