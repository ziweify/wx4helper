using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using zhaocaimao.Models.AutoBet;
using SharedModels = zhaocaimao.Shared.Models;
using zhaocaimao.Shared.Platform;
using zhaocaimao.Services.AutoBet.Browser.PlatformScripts;
using zhaocaimao.Services.AutoBet.Browser.Services;

namespace zhaocaimao.Services.AutoBet.Browser
{
    /// <summary>
    /// 浏览器引擎实现 - 复用 BsBrowserClient 的核心逻辑
    /// 直接使用 BsBrowserClient 的代码，通过反射或直接引用
    /// </summary>
    public class BetBrowserEngine : IBetBrowserEngine
    {
        private readonly WebView2 _webView;
        private int _configId;
        private string _configName = "";
        private string _platform = "";
        private string _platformUrl = "";
        private bool _isInitialized = false;
        
        // 🔥 复用 BsBrowserClient 的代码
        private PlatformScripts.IPlatformScript? _platformScript;
        private Services.WebView2ResourceHandler? _resourceHandler;
        
        public event Action<string>? OnLog;
        
        public bool IsInitialized => _isInitialized;
        
        public BetBrowserEngine(WebView2 webView)
        {
            _webView = webView ?? throw new ArgumentNullException(nameof(webView));
        }
        
        /// <summary>
        /// 初始化浏览器
        /// </summary>
        public async Task InitializeAsync(int configId, string configName, string platform, string platformUrl)
        {
            try
            {
                _configId = configId;
                _configName = configName;
                _platform = platform;
                _platformUrl = string.IsNullOrEmpty(platformUrl) ? PlatformUrlManager.GetDefaultUrl(platform) : platformUrl;
                
                OnLog?.Invoke("🚀 正在初始化浏览器引擎...");
                
                // 1. 初始化 WebView2
                await InitializeWebView2Async();
                OnLog?.Invoke("✅ WebView2 初始化完成");
                
                // 2. 初始化平台脚本（复用 BsBrowserClient 的代码）
                InitializePlatformScript();
                OnLog?.Invoke($"✅ 平台脚本初始化完成: {platform}");
                
                // 3. 初始化资源拦截器
                await InitializeResourceHandlerAsync();
                OnLog?.Invoke("✅ 资源拦截器初始化完成");
                
                _isInitialized = true;
                OnLog?.Invoke("🎉 浏览器引擎初始化成功");
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 初始化失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化 WebView2
        /// </summary>
        private async Task InitializeWebView2Async()
        {
            // 🔥 为每个实例创建独立的用户数据文件夹
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "zhaocaimao",
                "WebView2Data",
                $"Config_{_configId}");
            
            Directory.CreateDirectory(userDataFolder);
            
            // 使用自定义用户数据文件夹初始化 WebView2
            var environment = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: userDataFolder,
                options: null);
            
            // 等待 WebView2 初始化完成
            await _webView.EnsureCoreWebView2Async(environment);
            
            // 启用 DevTools
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            
            // 导航到目标 URL
            _webView.CoreWebView2.Navigate(_platformUrl);
            
            // 绑定导航事件
            _webView.CoreWebView2.NavigationCompleted += async (s, e) =>
            {
                if (e.IsSuccess)
                {
                    OnLog?.Invoke($"✅ 页面加载完成: {_webView.CoreWebView2.Source}");
                    
                    // 触发自动登录
                    await TryAutoLoginAsync();
                }
                else
                {
                    OnLog?.Invoke($"❌ 页面加载失败");
                }
            };
        }
        
        /// <summary>
        /// 初始化平台脚本（复用 BsBrowserClient 的代码）
        /// </summary>
        private void InitializePlatformScript()
        {
            var platformEnum = BetPlatformHelper.Parse(_platform);
            Action<string> logCallback = (msg) => OnLog?.Invoke(msg);
            
            // 🔥 根据平台创建对应的脚本实例
            _platformScript = platformEnum switch
            {
                BetPlatform.云顶 => CreateYunDing28Script(logCallback),
                BetPlatform.通宝 => CreateTongBaoScript(logCallback),
                BetPlatform.海峡 => CreateYunDing28Script(logCallback), // 暂用云顶脚本
                BetPlatform.红海 => CreateYunDing28Script(logCallback), // 暂用云顶脚本
                _ => CreateYunDing28Script(logCallback)
            };
        }
        
        /// <summary>
        /// 创建云顶28脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateYunDing28Script(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.YunDing28Script(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建云顶28脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建通宝脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateTongBaoScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.TongBaoScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建通宝脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 初始化资源拦截器
        /// </summary>
        private async Task InitializeResourceHandlerAsync()
        {
            try
            {
                _resourceHandler = new Services.WebView2ResourceHandler(OnResponseReceived);
                await _resourceHandler.InitializeAsync(_webView.CoreWebView2);
                OnLog?.Invoke("✅ 资源拦截器初始化完成");
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 资源拦截器初始化失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 响应接收回调
        /// </summary>
        private void OnResponseReceived(Services.ResponseEventArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(args.Url))
                    return;
                
                OnLog?.Invoke($"拦截:{args.Url}");
                
                if (!string.IsNullOrEmpty(args.PostData))
                {
                    OnLog?.Invoke($"[POST] {args.PostData.Substring(0, Math.Min(100, args.PostData.Length))}...");
                }
                
                // 让平台脚本处理响应
                _platformScript?.HandleResponse(args);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 响应处理失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 尝试自动登录
        /// </summary>
        private async Task TryAutoLoginAsync()
        {
            try
            {
                // 从 HTTP API 获取账号密码
                var username = "";
                var password = "";
                
                try
                {
                    var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync($"http://127.0.0.1:8888/api/config?configId={_configId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var config = JObject.Parse(json);
                        if (config["success"]?.Value<bool>() ?? false)
                        {
                            username = config["data"]?["Username"]?.ToString() ?? "";
                            password = config["data"]?["Password"]?.ToString() ?? "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnLog?.Invoke($"⚠️ 获取配置异常: {ex.Message}");
                }
                
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    OnLog?.Invoke("⚠️ 未配置账号密码，跳过自动登录");
                    return;
                }
                
                // 调用平台脚本的登录方法
                OnLog?.Invoke($"🔐 开始自动登录: {username}");
                var result = await ExecuteCommandAsync("Login", new
                {
                    username = username,
                    password = password
                });
                
                if (result.Success)
                {
                    OnLog?.Invoke("✅ 自动登录成功！");
                }
                else
                {
                    OnLog?.Invoke($"⚠️ 自动登录失败: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 自动登录异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 执行命令（复用 BsBrowserClient 的命令处理逻辑）
        /// </summary>
        public async Task<BetResult> ExecuteCommandAsync(string command, object? data = null)
        {
            try
            {
                OnLog?.Invoke($"收到命令: {command}");
                
                var result = new BetResult
                {
                    Success = false
                };
                
                // 🔥 复用 BsBrowserClient 的命令处理逻辑
                switch (command)
                {
                    case "Login":
                    case "登录":
                        if (_platformScript == null)
                        {
                            result.Success = false;
                            result.ErrorMessage = "平台脚本未初始化";
                            break;
                        }
                        
                        var loginData = data as Newtonsoft.Json.Linq.JObject;
                        var username = loginData?["username"]?.ToString() ?? "";
                        var password = loginData?["password"]?.ToString() ?? "";
                        
                        // WebView2 操作必须在 UI 线程执行
                        if (_webView.InvokeRequired)
                        {
                            var loginResult = await Task.Run(async () =>
                            {
                                var tcs = new TaskCompletionSource<bool>();
                                _webView.Invoke(async () =>
                                {
                                    try
                                    {
                                        var r = await _platformScript.LoginAsync(username, password);
                                        tcs.SetResult(r);
                                    }
                                    catch (Exception ex)
                                    {
                                        OnLog?.Invoke($"❌ 登录失败: {ex.Message}");
                                        tcs.SetResult(false);
                                    }
                                });
                                return await tcs.Task;
                            });
                            result.Success = loginResult;
                        }
                        else
                        {
                            result.Success = await _platformScript.LoginAsync(username, password);
                        }
                        result.ErrorMessage = result.Success ? null : "登录失败";
                        break;
                        
                    case "投注":
                        if (_platformScript == null)
                        {
                            result.Success = false;
                            result.ErrorMessage = "平台脚本未初始化";
                            break;
                        }
                        
                        SharedModels.BetStandardOrderList? betOrders = null;
                        if (data is Newtonsoft.Json.Linq.JArray jArray)
                        {
                            betOrders = jArray.ToObject<SharedModels.BetStandardOrderList>();
                        }
                        else if (data is Newtonsoft.Json.Linq.JObject betData)
                        {
                            betOrders = betData.ToObject<SharedModels.BetStandardOrderList>();
                        }
                        
                        if (betOrders == null || betOrders.Count == 0)
                        {
                            result.Success = false;
                            result.ErrorMessage = "投注内容为空";
                            break;
                        }
                        
                        var postStartTime = DateTime.Now;
                        try
                        {
                            bool success;
                            string orderId;
                            string platformResponse;
                            
                            if (_webView.InvokeRequired)
                            {
                                var betResult = await Task.Run(async () =>
                                {
                                    var tcs = new TaskCompletionSource<(bool, string, string)>();
                                    _webView.Invoke(async () =>
                                    {
                                        try
                                        {
                                            var r = await _platformScript.PlaceBetAsync(betOrders);
                                            tcs.SetResult(r);
                                        }
                                        catch (Exception ex)
                                        {
                                            OnLog?.Invoke($"❌ 投注失败: {ex.Message}");
                                            tcs.SetResult((false, "", $"#投注异常: {ex.Message}"));
                                        }
                                    });
                                    return await tcs.Task;
                                });
                                success = betResult.Item1;
                                orderId = betResult.Item2;
                                platformResponse = betResult.Item3;
                            }
                            else
                            {
                                var betResult = await _platformScript.PlaceBetAsync(betOrders);
                                success = betResult.success;
                                orderId = betResult.orderId;
                                platformResponse = betResult.platformResponse;
                            }
                            
                            var postEndTime = DateTime.Now;
                            var durationMs = (int)(postEndTime - postStartTime).TotalMilliseconds;
                            
                            result.Success = success;
                            result.ErrorMessage = success ? null : "投注失败";
                            result.Data = new
                            {
                                postStartTime = postStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                postEndTime = postEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                durationMs = durationMs,
                                orderNo = orderId,
                                platformResponse = platformResponse
                            };
                        }
                        catch (Exception betEx)
                        {
                            result.Success = false;
                            result.ErrorMessage = betEx.Message;
                        }
                        break;
                        
                    case "获取余额":
                        if (_platformScript == null)
                        {
                            result.Success = false;
                            result.ErrorMessage = "平台脚本未初始化";
                            break;
                        }
                        
                        decimal balance = -1;
                        if (_webView.InvokeRequired)
                        {
                            balance = await Task.Run(async () =>
                            {
                                var tcs = new TaskCompletionSource<decimal>();
                                _webView.Invoke(async () =>
                                {
                                    try
                                    {
                                        var r = await _platformScript.GetBalanceAsync();
                                        tcs.SetResult(r);
                                    }
                                    catch (Exception ex)
                                    {
                                        OnLog?.Invoke($"❌ 获取余额失败: {ex.Message}");
                                        tcs.SetResult(-1);
                                    }
                                });
                                return await tcs.Task;
                            });
                        }
                        else
                        {
                            balance = await _platformScript.GetBalanceAsync();
                        }
                        
                        result.Success = balance >= 0;
                        result.Data = new { balance };
                        result.ErrorMessage = result.Success ? null : "获取余额失败";
                        break;
                        
                    case "心跳检测":
                        result.Success = true;
                        result.Data = new
                        {
                            configId = _configId,
                            platform = _platform
                        };
                        break;
                        
                    default:
                        result.Success = false;
                        result.ErrorMessage = $"未知命令: {command}";
                        break;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}

