using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.CefBrowser
{
    public delegate void OnJSDialogHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText);
    public delegate void OnUrlChangeHandler(string url);


    public class ChromiumWebBrowserExtend
        : ChromiumWebBrowser
    {
        /// <summary>
        ///     给外部回调函数处理
        /// </summary>
        public OnJSDialogHandler OnJSDialog { get; set; }
        public OnUrlChangeHandler OnUrlChange { get; set; }

        public string UrlCurrent { get; set; }


        //添加各种回调函数
        public ChromiumWebBrowserExtend(string address, IRequestContext requestContext = null)
            : base(address, requestContext) 
        {

            //注册自定义事件
            JsDialogHandler = new F5JsDialogHandler(OnJSDialog);                  //自定义弹窗事件.这里要做个委托。把弹窗内容输出到外部检测
            this.AddressChanged += ChromiumWebBrowserExtend_AddressChanged;
        }

        /// <summary>
        ///     页面变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChromiumWebBrowserExtend_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            UrlCurrent = e.Address;
            if (OnUrlChange != null)
                OnUrlChange(this.UrlCurrent);
        }
    }
}
