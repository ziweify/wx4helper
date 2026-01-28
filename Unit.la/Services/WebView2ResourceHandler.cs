using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Unit.La.Services
{
    /// <summary>
    /// WebView2 资源拦截器 - 拦截 HTTP 请求和响应
    /// </summary>
    public class WebView2ResourceHandler
    {
        private readonly Action<ResponseEventArgs>? _responseCallback;
        private readonly ConcurrentDictionary<string, string> _postDataCache = new();

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
        /// 请求发送事件 - 获取 POST data
        /// </summary>
        private void OnRequestWillBeSent(object? sender, CoreWebView2DevToolsProtocolEventReceivedEventArgs args)
        {
            try
            {
                // 使用 System.Text.Json 解析 JSON
                using var doc = JsonDocument.Parse(args.ParameterObjectAsJson);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("request", out var request))
                {
                    var url = request.TryGetProperty("url", out var urlProp) ? urlProp.GetString() : null;
                    var method = request.TryGetProperty("method", out var methodProp) ? methodProp.GetString() : null;
                    var postData = request.TryGetProperty("postData", out var postDataProp) ? postDataProp.GetString() : null;

                    if (method == "POST" && !string.IsNullOrEmpty(postData) && !string.IsNullOrEmpty(url))
                    {
                        // 缓存 POST data，在响应时使用
                        _postDataCache[url] = postData;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnRequestWillBeSent Error: {ex.Message}");
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
                
                // 获取 POST data（如果有）
                string? postData = null;
                if (_postDataCache.TryRemove(request.Uri, out var cachedPostData))
                {
                    postData = cachedPostData;
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
                    PostData = postData ?? "",
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
                    ErrorMessage = $"OnWebResourceResponseReceived Error: {ex.Message}"
                });
            }
        }
    }
}
