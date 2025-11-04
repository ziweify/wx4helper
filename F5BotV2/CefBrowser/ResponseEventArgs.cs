using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.CefBrowser
{
    /// <summary>
    ///     有请求的消息, 自己处理过, 需要返回的消息
    /// </summary>
    public class ResponseEventArgs
    {
        public string SenderName { get; set; }  //从哪个对象返回的消息, 对象名
        public string ReferrerUrl { get; set; } //可空
        public string Url { get; set; }         //这才是主要的，这个请求的url
        public string Context { get; set; }     //response返回的字符串, 默认uft-8
        public string ErrorMessage { get; set; } //附加内容..描述改消息的问题
        public string PostData { get; set; }
        //public ResponseContextType Cagetor { get; set; }

        public ResponseEventArgs() { }
    }
}
