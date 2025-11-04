using CefSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.CefBrowser
{
    public static  class CefPost
    {
        /// <summary>
        ///     这个方法有问题, 以后有时间要看看，通过这里的post后, 原来登录好的页面就退出了
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="url"></param>
        /// <param name="postDataBytes"></param>
        /// <param name="contentType"></param>
        public static void Post(this IWebBrowser browser, string url, byte[] postDataBytes, string contentType)
        {
            System.Diagnostics.Debug.WriteLine("Post::begin");
            IFrame frame = browser.GetBrowser().MainFrame;
            IRequest request = frame.CreateRequest();

            request.Url = url;
            request.Method = "POST";

            request.InitializePostData();
            var element = request.PostData.CreatePostDataElement();
            element.Bytes = postDataBytes;
            request.PostData.AddElement(element);

            NameValueCollection headers = new NameValueCollection();
            headers.Add("Content-Type", contentType);
            request.Headers = headers;

            frame.GetTextAsync().ContinueWith(taskHtml =>
            {
                var html = taskHtml.Result;
                System.Diagnostics.Debug.WriteLine(html);
            });
            frame.LoadRequest(request);
            System.Diagnostics.Debug.WriteLine("Post::finish");
        }
    }
}
