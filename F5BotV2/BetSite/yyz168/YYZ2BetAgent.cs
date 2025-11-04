//using CefSharp;
//using CefSharp.WinForms;
//using CsQuery;
//using F5BotV2.CefBrowser;
//using LxLib.LxNet;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace F5BotV2.BetSite.yyz168
//{
//    /// <summary>
//    ///     代理端, 跟单用
//    /// </summary>
//    public class YYZ2BetAgent
//        : BetApi
//    {
//        //代理登录页面地址
//        private string _loginUrl = "http://yyz2.666.macao-lottery.com/";

//        public YYZ2BetAgent(string cookie)
//            :base( BetSiteType.元宇宙2)
//        {

//        }

//        /// <summary>
//        ///     实现浏览器的模拟登录
//        ///     这个让他手动登录i
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="pass"></param>
//        /// <param name="browser"></param>
//        /// <returns></returns>
//        public override async Task<int> LoginAsync(string name, string pwd, CefBrowserView browser)
//        {
//            //打开会员线路1
//            await browser.LoadUrlAsyn(_loginUrl, new Func<ChromiumWebBrowser, bool>((p) =>
//            {
//                //1、得到验证码
//                string yzmBase64 = getImageBase64(p);

//                //2、显示验证码处理窗口
//                var image = LxHttpHelperEx.Base64ToImage(yzmBase64, ImageFormat.Png);
//                string yzmCode = "";
//                var codeinputResult = browser.ShowCodeInput(image, out yzmCode, new Func<Image>(() => {
//                    //刷新验证码
//                    Image imageRet = null;
//                    string script = "getImg()";

//                    var js = p.EvaluateScriptAsPromiseAsync(script);
//                    js.Wait();

//                    string imageStr = getImageBase64(p);
//                    imageRet = LxHttpHelperEx.Base64ToImage(imageStr, ImageFormat.Png);
//                    return imageRet;
//                }));


//                //自动登录代码
//                //对方用的是VUE2.所以不能用这种语法
//                //p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('input')[0].value = '" + name + "';");
//                //p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('input')[1].value = '" + pwd + "';");
//                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"app.__vue__.$children[0].acc=\'{name}\'");
//                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"app.__vue__.$children[0].pwd=\'{pwd}\'");
//                if (codeinputResult == System.Windows.Forms.DialogResult.OK)
//                {
//                    if (!string.IsNullOrEmpty(yzmCode))
//                    {
//                        p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"app.__vue__.$children[0].code=\'{yzmCode}\'");
//                        //点击登录按钮
//                        try
//                        {
//                            var jsLogin = p.GetBrowser().MainFrame.EvaluateScriptAsync($"login()");
//                            if (jsLogin != null)
//                            {
//                                //接管弹窗
//                                jsLogin.Wait();
//                                //释放接管弹窗

//                            }
//                        }
//                        catch (Exception ex)
//                        {

//                        }
//                    }
//                }


//                return true;
//            }));
//            Debug.WriteLine("Login::退出");
//            return 0;
//        }

//        private string getImageBase64(ChromiumWebBrowser p)
//        {
//            //处理验证码
//            string yzmBase64 = "";
//            var tsSource = p.GetBrowser().MainFrame.GetSourceAsync();
//            tsSource.Wait();
//            var htmlCode = tsSource.Result;
//            //使用CsQuery库来解析html
//            var jqHtml = CQ.CreateDocument(htmlCode);
//            var jqLoginBox = jqHtml[".login_box>ul>li"];
//            //遍历li,得到验证码行
//            foreach (var li in jqLoginBox)
//            {
//                var html = li.OuterHTML;
//                //继续分析子项
//                var jqLi = CQ.Create(html);
//                var text = jqLi["li>i"].Html();
//                if (text.IndexOf("验证码") != -1)
//                {
//                    //提取验证码
//                    try
//                    {
//                        var yzmCode = jqLi["li>.code>img"].Each(x => {
//                            yzmBase64 = x.GetAttribute("src");
//                        });
//                    }
//                    catch (Exception ex)
//                    {

//                    }

//                    break;
//                }
//            }
//            return yzmBase64;
//        }

//        /// <summary>
//        ///     得到代理未结算的注单
//        /// </summary>
//        public void QurryBetingByCondiction()
//        {
//            //@查询代理下面的当前注单。。
//            //http://yyz2.666.macao-lottery.com/ServiceAPI/Betlist/QueryBetingByCondiction?uid=MMQUM1t4WSMYA3mLvNpWY&page=1
//            //@POST参数
//            //{
//            //"lotteryType":"TWBG",
//            //"search":"username",
//            //"like":"jkj115",
//            //"betType":-1,
//            //"status":0,
//            //"startDate":"2023-08-02",
//            //"endDate":"2023-08-02",
//            //"installments":""
//            //}
//            //@返回值



//        }  


//    }
//}
