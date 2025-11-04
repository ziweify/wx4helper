using CefSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.CefBrowser
{
    public class F5JsDialogHandler : CefSharp.IJsDialogHandler
    {
        private CefSharp.IJsDialogHandler handler;
        private OnJSDialogHandler JSDialorHandler;

        public F5JsDialogHandler(OnJSDialogHandler JSDialorHandler)
        {
            handler = new CefSharp.Handler.JsDialogHandler();
            if (JSDialorHandler != null)
                this.JSDialorHandler = JSDialorHandler;
        }

        public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback)
        {
            return handler.OnBeforeUnloadDialog(chromiumWebBrowser, browser, messageText, isReload, callback);
        }

        public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            handler.OnDialogClosed(chromiumWebBrowser, browser);
        }

        public bool OnJSDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
        {
            Debug.WriteLine(string.Format("OnJSDialog::{0}=>{1}", dialogType.ToString(), messageText));
            //suppressMessage = true, return false 这样输入页面不弹窗了, 而且也能操作。
            //suppressMessage = true;
            //return false;
            //正常处理
            var result = handler.OnJSDialog(chromiumWebBrowser, browser, originUrl, dialogType, messageText, defaultPromptText, callback, ref suppressMessage);
            //使用回调函数
            if (this.JSDialorHandler != null)
                this.JSDialorHandler(chromiumWebBrowser, browser, originUrl, dialogType, messageText, defaultPromptText);

            return result;
        }

        public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            handler.OnResetDialogState(chromiumWebBrowser, browser);
        }
    }
}
