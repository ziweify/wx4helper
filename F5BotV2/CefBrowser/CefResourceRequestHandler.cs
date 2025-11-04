using CefSharp.Handler;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.CefBrowser
{

    public class CefResourceRequestHandler
        : ResourceRequestHandler
    {
        private readonly System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
        private CefCallBack<ResponseEventArgs> _ResponseCompletion;

        public CefResourceRequestHandler(CefCallBack<ResponseEventArgs> responseCompletion)
        {
            this._ResponseCompletion = responseCompletion;
        }

        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            //"http://game.cj7799.com/api/getRecode.go?a=a&t=1705458168054&gameid=TWBG&no=113003289"
            //string url = "http://game.cj7799.com/api/getRecode.go?a=a&t=1705458168054&gameid=TWBG&no=113003289";
            string postdata = "";
            string Method = "";

            try
            {
                Method = request.Method;

                //获取请求数据
                if (request.PostData != null)
                {
                    var emements = request.PostData.Elements;
                    foreach (var emement in emements)
                    {
                        var postdata_bytes = emement.Bytes;
                        if (!string.IsNullOrEmpty(postdata))
                            postdata += "|";
                        postdata += System.Text.Encoding.UTF8.GetString(postdata_bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                postdata = $"POST获取错误::{ex.Message}";
            }

            try
            {
                var bytes = memoryStream.ToArray();
                string data = "";
                if (response.Charset == "utf-8" || response.Charset == "")
                {
                    //得到数据1
                    data = System.Text.Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    //Deal with different encoding here
                    //得到数据1
                    var encoding = Encoding.GetEncoding(response.Charset);
                    data = encoding.GetString(bytes);
                }

                //解析数据1
                //把数据回调给外部程序处理
                _ResponseCompletion?.Invoke(this, new ResponseEventArgs()
                {
                    SenderName = this.GetType().Name,
                    Context = data,
                    ReferrerUrl = request.ReferrerUrl,
                    Url = request.Url,
                    PostData = postdata
                });
            }
            catch (Exception ex)
            {
                try
                {
                    _ResponseCompletion?.Invoke(this, new ResponseEventArgs()
                    {
                        SenderName = this.GetType().Name,
                        ReferrerUrl = request.ReferrerUrl,
                        Url = request.Url,
                        ErrorMessage = "OnResourceLoadComplete::ERROR" + ex.Message,
                    });
                }
                catch
                {

                }

            }

            //哪个url
            //Debug.WriteLine(request.Url);
            ////对应地址的Headers
            //Debug.WriteLine(response.Headers["date"]);
        }

        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return new CefSharp.ResponseFilter.StreamResponseFilter(memoryStream);
        }
    }
}
