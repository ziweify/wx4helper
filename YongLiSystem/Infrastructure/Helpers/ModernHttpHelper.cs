using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YongLiSystem.Infrastructure.Helpers
{
    /// <summary>
    /// 现代化的 HTTP 辅助类（基于 HttpClient）
    /// 
    /// 🎯 设计目标：像老的 HttpHelper 一样简单易用，但使用现代 HttpClient
    /// 
    /// 使用示例:
    /// <code>
    /// var helper = new ModernHttpHelper();
    /// var result = await helper.PostAsync(new HttpRequestItem
    /// {
    ///     Url = "https://api.example.com/login",
    ///     PostData = jsonData,
    ///     ContentType = "application/json",
    ///     Headers = new[]
    ///     {
    ///         $"Authorization: {token}",
    ///         "X-Custom-Header: value"
    ///     }
    /// });
    /// </code>
    /// </summary>
    public class ModernHttpHelper
    {
        private readonly HttpClient _httpClient;
        
        public ModernHttpHelper()
        {
            _httpClient = new HttpClient();
            
            // 设置默认超时
            _httpClient.Timeout = TimeSpan.FromSeconds(100);
            
            // 设置默认请求头
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        }
        
        /// <summary>
        /// 使用自定义 HttpClient（可复用连接池）
        /// </summary>
        public ModernHttpHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        /// <summary>
        /// 发送 GET 请求
        /// </summary>
        public async Task<HttpResponseResult> GetAsync(HttpRequestItem item)
        {
            item.Method = "GET";
            return await SendRequestAsync(item);
        }
        
        /// <summary>
        /// 发送 POST 请求
        /// </summary>
        public async Task<HttpResponseResult> PostAsync(HttpRequestItem item)
        {
            item.Method = "POST";
            return await SendRequestAsync(item);
        }
        
        /// <summary>
        /// 发送请求（核心方法）
        /// </summary>
        private async Task<HttpResponseResult> SendRequestAsync(HttpRequestItem item)
        {
            var result = new HttpResponseResult();
            
            // 🔥 创建超时控制器（支持单个请求的超时设置）
            using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(item.Timeout)))
            {
                try
                {
                    // 创建请求
                    var request = new HttpRequestMessage(
                        item.Method == "POST" ? HttpMethod.Post : HttpMethod.Get,
                        item.Url
                    );
                    
                    // 添加请求头（自动过滤内容头）
                    var headers = item.GetHeadersDictionary();
                    foreach (var header in headers)
                    {
                        try
                        {
                            if (!request.Headers.Contains(header.Key))
                            {
                                request.Headers.Add(header.Key, header.Value);
                            }
                        }
                        catch
                        {
                            // 忽略无法添加的请求头（可能是内容头）
                        }
                    }
                    
                    // 添加 POST 数据
                    if (!string.IsNullOrEmpty(item.PostData))
                    {
                        var encoding = item.Encoding ?? Encoding.UTF8;
                        var contentType = item.ContentType ?? "application/x-www-form-urlencoded";
                        
                        request.Content = new StringContent(item.PostData, encoding, contentType);
                    }
                    else if (item.PostDataByte != null && item.PostDataByte.Length > 0)
                    {
                        request.Content = new ByteArrayContent(item.PostDataByte);
                        
                        if (!string.IsNullOrEmpty(item.ContentType))
                        {
                            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(item.ContentType);
                        }
                    }
                    
                    // 🔥 发送请求（带超时控制）
                    using (var response = await _httpClient.SendAsync(request, cts.Token))
                    {
                        result.StatusCode = (int)response.StatusCode;
                        result.StatusDescription = response.ReasonPhrase ?? "";
                        
                        // 读取响应内容
                        var contentBytes = await response.Content.ReadAsByteArrayAsync();
                        result.ResponseByte = contentBytes;
                        
                        // 转换为字符串
                        var responseEncoding = item.Encoding ?? Encoding.UTF8;
                        result.Html = responseEncoding.GetString(contentBytes);
                        
                        // 提取响应头
                        result.Headers = new Dictionary<string, string>();
                        foreach (var header in response.Headers)
                        {
                            result.Headers[header.Key] = string.Join(", ", header.Value);
                        }
                        
                        // 提取内容头
                        foreach (var header in response.Content.Headers)
                        {
                            result.Headers[header.Key] = string.Join(", ", header.Value);
                        }
                        
                        result.Success = response.IsSuccessStatusCode;
                    }
                }
                catch (TaskCanceledException)
                {
                    // 🔥 超时异常特殊处理
                    result.Success = false;
                    result.ErrorMessage = $"请求超时（{item.Timeout}秒）";
                    result.Html = result.ErrorMessage;
                }
                catch (OperationCanceledException)
                {
                    // 🔥 操作取消异常
                    result.Success = false;
                    result.ErrorMessage = $"请求被取消（超时: {item.Timeout}秒）";
                    result.Html = result.ErrorMessage;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    result.Html = ex.Message;
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// HTTP 请求参数类（类似老的 HttpItem）
    /// </summary>
    public class HttpRequestItem
    {
        /// <summary>
        /// 请求 URL（必填）
        /// </summary>
        public string Url { get; set; } = "";
        
        /// <summary>
        /// 请求方法（GET/POST，自动设置）
        /// </summary>
        public string Method { get; set; } = "GET";
        
        /// <summary>
        /// POST 数据（字符串）
        /// </summary>
        public string? PostData { get; set; }
        
        /// <summary>
        /// POST 数据（字节数组）
        /// </summary>
        public byte[]? PostDataByte { get; set; }
        
        /// <summary>
        /// 内容类型（默认 application/x-www-form-urlencoded）
        /// </summary>
        public string? ContentType { get; set; }
        
        /// <summary>
        /// 请求头数组（更简洁的方式）
        /// 
        /// 格式: ["Authorization: Bearer xxx", "Content-Type: application/json"]
        /// 如果有重复的 key，后面的值会覆盖前面的值
        /// </summary>
        public string[]? Headers { get; set; }
        
        /// <summary>
        /// 编码（默认 UTF8）
        /// </summary>
        public Encoding? Encoding { get; set; }
        
        /// <summary>
        /// 超时时间（秒，默认 100）
        /// </summary>
        public int Timeout { get; set; } = 100;
        
        /// <summary>
        /// 将请求头数组解析为字典（内部使用）
        /// </summary>
        internal Dictionary<string, string> GetHeadersDictionary()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            if (Headers == null || Headers.Length == 0)
            {
                return dict;
            }
            
            foreach (var header in Headers)
            {
                if (string.IsNullOrWhiteSpace(header))
                {
                    continue;
                }
                
                // 按第一个冒号分割
                var index = header.IndexOf(':');
                if (index > 0)
                {
                    var key = header.Substring(0, index).Trim();
                    var value = header.Substring(index + 1).Trim();
                    
                    // 🔥 后面的值覆盖前面的值
                    dict[key] = value;
                }
            }
            
            return dict;
        }
    }
    
    /// <summary>
    /// HTTP 响应结果类（类似老的 HttpResult）
    /// </summary>
    public class HttpResponseResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 响应内容（字符串）
        /// </summary>
        public string Html { get; set; } = "";
        
        /// <summary>
        /// 响应内容（字节数组）
        /// </summary>
        public byte[]? ResponseByte { get; set; }
        
        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }
        
        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDescription { get; set; } = "";
        
        /// <summary>
        /// 响应头字典
        /// </summary>
        public Dictionary<string, string>? Headers { get; set; }
        
        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}

