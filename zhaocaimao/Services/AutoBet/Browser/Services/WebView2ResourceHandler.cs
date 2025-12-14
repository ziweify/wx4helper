using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using zhaocaimao.Services.AutoBet.Browser.Models;

namespace zhaocaimao.Services.AutoBet.Browser.Services
{
    /// <summary>
    /// WebView2 资源拦截器 - 复用 BsBrowserClient 的逻辑
    /// </summary>
    public class WebView2ResourceHandler
    {
        private readonly Action<ResponseEventArgs>? _responseCallback;
        private readonly ConcurrentDictionary<string, (string method, string postData)> _requestCache = new();

        public WebView2ResourceHandler(Action<ResponseEventArgs>? responseCallback)
        {
            _responseCallback = responseCallback;
        }

        /// <summary>
        /// 初始化拦截器
        /// </summary>
        public async Task InitializeAsync(CoreWebView2 coreWebView2)
        {
            // 1. 启用网络监控（DevTools Protocol）
            await coreWebView2.CallDevToolsProtocolMethodAsync("Network.enable", "{}");

            // 2. 监听请求发送事件（获取 POST data）
            coreWebView2.GetDevToolsProtocolEventReceiver("Network.requestWillBeSent")
                .DevToolsProtocolEventReceived += OnRequestWillBeSent;

            // 3. 监听响应接收事件
            coreWebView2.WebResourceResponseReceived += OnWebResourceResponseReceived;
        }

        /// <summary>
        /// 请求发送事件 - 获取 Method 和 POST data
        /// </summary>
        private void OnRequestWillBeSent(object? sender, CoreWebView2DevToolsProtocolEventReceivedEventArgs args)
        {
            try
            {
                var json = JObject.Parse(args.ParameterObjectAsJson);
                var request = json["request"];
                var url = request?["url"]?.ToString();
                var method = request?["method"]?.ToString() ?? "GET";
                var postData = request?["postData"]?.ToString() ?? "";

                if (!string.IsNullOrEmpty(url))
                {
                    // 缓存 Method 和 POST data，在响应时使用
                    _requestCache[url] = (method, postData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnRequestWillBeSent Error: {ex.Message}");
            }
        }

        /// <summary>
        /// 响应接收事件 - 获取 Response 内容
        /// </summary>
        private async void OnWebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs args)
        {
            try
            {
                var request = args.Request;
                var response = args.Response;
                
                // 获取 Method 和 POST data（如果有）
                string method = request.Method;  // 从 request 直接获取
                string postData = "";
                
                if (_requestCache.TryRemove(request.Uri, out var cachedRequest))
                {
                    method = cachedRequest.method;  // 使用缓存的 method（更准确）
                    postData = cachedRequest.postData;
                }

                // 获取响应内容
                string content = "";
                try
                {
                    var stream = await response.GetContentAsync();
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            content = await reader.ReadToEndAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    content = $"内容获取失败: {ex.Message}";
                }

                // 回调给外部处理
                _responseCallback?.Invoke(new ResponseEventArgs
                {
                    SenderName = nameof(WebView2ResourceHandler),
                    Url = request.Uri,
                    ReferrerUrl = "", // WebView2 不直接提供，需要从 DevTools Protocol 获取
                    Context = content,
                    PostData = postData,
                    Method = method,  // 🔥 添加 HTTP 方法
                    StatusCode = response.StatusCode,
                    ContentType = response.Headers.Contains("Content-Type") 
                        ? response.Headers.GetHeader("Content-Type") 
                        : ""
                });
            }
            catch (Exception ex)
            {
                _responseCallback?.Invoke(new ResponseEventArgs
                {
                    SenderName = nameof(WebView2ResourceHandler),
                    Url = args.Request.Uri,
                    Method = args.Request.Method,  // 🔥 异常时也包含 Method
                    ErrorMessage = $"OnWebResourceResponseReceived Error: {ex.Message}"
                });
            }
        }
    }

    /// <summary>
    /// 响应事件参数 - 与 F5BotV2 兼容
    /// </summary>
    public class ResponseEventArgs : EventArgs
    {
        public string SenderName { get; set; } = "";
        public string Url { get; set; } = "";
        public string ReferrerUrl { get; set; } = "";
        public string Context { get; set; } = "";
        public string PostData { get; set; } = "";
        public string Method { get; set; } = "";  // HTTP 方法: GET, POST, OPTIONS, etc.
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = "";
        public string? ErrorMessage { get; set; }
    }
}

