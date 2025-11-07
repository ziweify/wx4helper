using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models.AutoBet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaiShengVx3Plus.Services.AutoBet
{
    /// <summary>
    /// 自动投注 HTTP 服务器
    /// 用于 VxMain 与 BsBrowserClient 之间的通信
    /// </summary>
    public class AutoBetHttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly ILogService _log;
        private readonly Func<int, BetConfig?> _getConfig;
        private readonly Action<BetConfig> _saveConfig;
        private readonly Func<int, string?, BetOrder?> _getOrder;
        private readonly Action<int, bool, string?, string?> _handleResult;
        
        private bool _isRunning;
        
        public AutoBetHttpServer(
            ILogService log,
            int port,
            Func<int, BetConfig?> getConfig,
            Action<BetConfig> saveConfig,
            Func<int, string?, BetOrder?> getOrder,
            Action<int, bool, string?, string?> handleResult)
        {
            _log = log;
            _getConfig = getConfig;
            _saveConfig = saveConfig;
            _getOrder = getOrder;
            _handleResult = handleResult;
            
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }
        
        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start()
        {
            try
            {
                _listener.Start();
                _isRunning = true;
                
                var prefix = _listener.Prefixes.First();
                _log.Info("HTTP", $"✅ HTTP 服务器已启动: {prefix}");
                _log.Info("HTTP", $"📖 API文档:");
                _log.Info("HTTP", $"  GET  {prefix}api/config?configId=1    - 获取配置和Cookie");
                _log.Info("HTTP", $"  GET  {prefix}api/order?configId=1     - 获取待投注订单");
                _log.Info("HTTP", $"  POST {prefix}api/result              - 提交投注结果");
                _log.Info("HTTP", $"  POST {prefix}api/cookie              - 更新Cookie");
                _log.Info("HTTP", $"  GET  {prefix}api/ping                - 心跳检测");
                
                Task.Run(() => ListenAsync());
            }
            catch (Exception ex)
            {
                _log.Error("HTTP", "服务器启动失败", ex);
                throw;
            }
        }
        
        /// <summary>
        /// 监听请求
        /// </summary>
        private async Task ListenAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context));
                }
                catch (HttpListenerException)
                {
                    // 服务器停止时会抛出异常，正常情况
                    if (_isRunning)
                    {
                        _log.Warning("HTTP", "监听器异常");
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("HTTP", "处理请求异常", ex);
                }
            }
        }
        
        /// <summary>
        /// 处理请求
        /// </summary>
        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            
            try
            {
                var path = request.Url?.AbsolutePath?.ToLower() ?? "";
                var method = request.HttpMethod.ToUpper();
                
                _log.Info("HTTP", $"📩 {method} {request.Url?.PathAndQuery}");
                
                // 路由分发
                if (path == "/api/config" && method == "GET")
                {
                    await HandleGetConfigAsync(request, response);
                }
                else if (path == "/api/order" && method == "GET")
                {
                    await HandleGetOrderAsync(request, response);
                }
                else if (path == "/api/result" && method == "POST")
                {
                    await HandlePostResultAsync(request, response);
                }
                else if (path == "/api/cookie" && method == "POST")
                {
                    await HandlePostCookieAsync(request, response);
                }
                else if (path == "/api/ping" && method == "GET")
                {
                    await HandlePingAsync(request, response);
                }
                else
                {
                    await RespondJsonAsync(response, 404, new
                    {
                        error = "Not Found",
                        path = path,
                        message = "接口不存在，请查看API文档"
                    });
                }
            }
            catch (Exception ex)
            {
                _log.Error("HTTP", $"请求处理失败: {request.Url?.PathAndQuery}", ex);
                await RespondJsonAsync(response, 500, new
                {
                    error = "Internal Server Error",
                    message = ex.Message
                });
            }
        }
        
        /// <summary>
        /// GET /api/config?configId=1
        /// 返回配置信息和Cookie（用于浏览器初始化）
        /// </summary>
        private async Task HandleGetConfigAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            var configId = int.Parse(request.QueryString["configId"] ?? "0");
            var config = _getConfig(configId);
            
            if (config != null)
            {
                await RespondJsonAsync(response, 200, new
                {
                    success = true,
                    data = new
                    {
                        config.Id,
                        config.ConfigName,
                        config.Platform,
                        config.PlatformUrl,
                        config.Username,
                        cookieData = config.CookieData,
                        cookieUpdateTime = config.CookieUpdateTime
                    }
                });
                
                _log.Info("HTTP", $"✅ 返回配置: {config.ConfigName}");
            }
            else
            {
                await RespondJsonAsync(response, 404, new
                {
                    success = false,
                    error = "Config Not Found",
                    configId = configId
                });
            }
        }
        
        /// <summary>
        /// GET /api/order?configId=1&issueId=114062940
        /// 返回待投注订单
        /// </summary>
        private async Task HandleGetOrderAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            var configId = int.Parse(request.QueryString["configId"] ?? "0");
            var issueId = request.QueryString["issueId"];
            
            var order = _getOrder(configId, issueId);
            
            if (order != null)
            {
                await RespondJsonAsync(response, 200, new
                {
                    success = true,
                    data = new
                    {
                        order.IssueId,
                        order.PlayType,
                        order.BetContent,
                        order.Amount
                    }
                });
                
                _log.Info("HTTP", $"✅ 返回订单: {order.IssueId} {order.BetContent} {order.Amount}元");
            }
            else
            {
                await RespondJsonAsync(response, 200, new
                {
                    success = false,
                    message = "No Pending Order",
                    configId = configId,
                    issueId = issueId
                });
            }
        }
        
        /// <summary>
        /// POST /api/result
        /// Body: { configId: 1, success: true, orderId: "xxx", errorMessage: "..." }
        /// 提交投注结果
        /// </summary>
        private async Task HandlePostResultAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var body = await reader.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<JObject>(body);
            
            var configId = data?["configId"]?.ToObject<int>() ?? 0;
            var success = data?["success"]?.ToObject<bool>() ?? false;
            var orderId = data?["orderId"]?.ToString();
            var errorMessage = data?["errorMessage"]?.ToString();
            
            // 调用回调处理结果
            _handleResult(configId, success, orderId, errorMessage);
            
            await RespondJsonAsync(response, 200, new
            {
                success = true,
                message = "Result received"
            });
            
            _log.Info("HTTP", $"✅ 收到投注结果: 配置{configId} {(success ? "成功" : "失败")} 订单号:{orderId}");
        }
        
        /// <summary>
        /// POST /api/cookie
        /// Body: { configId: 1, cookieData: "[...]" }
        /// 更新Cookie
        /// </summary>
        private async Task HandlePostCookieAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var body = await reader.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<JObject>(body);
            
            var configId = data?["configId"]?.ToObject<int>() ?? 0;
            var cookieData = data?["cookieData"]?.ToString();
            
            var config = _getConfig(configId);
            if (config != null)
            {
                config.CookieData = cookieData;
                config.CookieUpdateTime = DateTime.Now;
                _saveConfig(config);
                
                await RespondJsonAsync(response, 200, new
                {
                    success = true,
                    message = "Cookie updated"
                });
                
                _log.Info("HTTP", $"✅ Cookie已更新: 配置{configId} {config.ConfigName}");
            }
            else
            {
                await RespondJsonAsync(response, 404, new
                {
                    success = false,
                    error = "Config Not Found"
                });
            }
        }
        
        /// <summary>
        /// GET /api/ping
        /// 心跳检测
        /// </summary>
        private async Task HandlePingAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            await RespondJsonAsync(response, 200, new
            {
                success = true,
                message = "Pong",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        
        /// <summary>
        /// 返回 JSON 响应
        /// </summary>
        private async Task RespondJsonAsync(HttpListenerResponse response, int statusCode, object data)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.Headers.Add("Access-Control-Allow-Origin", "*"); // 允许跨域（浏览器访问）
            
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            response.Close();
        }
        
        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _listener?.Stop();
            _log.Info("HTTP", "⏹️ HTTP 服务器已停止");
        }
        
        public void Dispose()
        {
            Stop();
            _listener?.Close();
        }
    }
}

