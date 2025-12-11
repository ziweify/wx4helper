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
            
            // 🔥 配置 WebView2 设置，确保所有功能正常
            var settings = _webView.CoreWebView2.Settings;
            
            // 启用 DevTools（调试用）
            settings.AreDevToolsEnabled = true;
            
            // 🔥 确保 JavaScript 已启用（默认已启用，但显式设置以防万一）
            settings.IsScriptEnabled = true;
            
            // 🔥 确保允许执行 Web 消息（用于 JavaScript 与 C# 通信）
            settings.IsWebMessageEnabled = true;
            
            // 🔥 确保允许状态栏（某些网站可能需要）
            settings.IsStatusBarEnabled = true;
            
            // 🔥 确保允许缩放（提升用户体验）
            settings.IsZoomControlEnabled = true;
            
            // 🔥 确保允许内置错误页面（调试友好）
            settings.IsBuiltInErrorPageEnabled = true;
            
            // 🔥 启用通用自动填充（可能影响表单）
            settings.IsGeneralAutofillEnabled = true;
            
            // 🔥 启用密码自动填充保存提示（可能影响表单）
            settings.IsPasswordAutosaveEnabled = true;
            
            OnLog?.Invoke("✅ WebView2 设置已配置（JavaScript、表单、自动填充均已启用）");
            
            // 🔥 参考 F5BotV2 OpenPageSelf.cs：拦截新窗口请求，在当前窗口打开
            // F5BotV2 Line 23-29: OnBeforePopup 拦截弹出窗口并在当前窗口加载
            _webView.CoreWebView2.NewWindowRequested += (s, e) =>
            {
                OnLog?.Invoke($"🚫 拦截新窗口请求: {e.Uri}");
                OnLog?.Invoke($"   在当前窗口打开: {e.Uri}");
                
                // 取消新窗口打开
                e.Handled = true;
                
                // 在当前窗口加载目标URL（参考 F5BotV2 Line 27: chromiumWebBrowser.Load(targetUrl)）
                _webView.CoreWebView2.Navigate(e.Uri);
            };
            
            // 导航到目标 URL
            _webView.CoreWebView2.Navigate(_platformUrl);
            
            // 🔥 绑定NavigationStarting事件，监控页面导航
            _webView.CoreWebView2.NavigationStarting += (s, e) =>
            {
                OnLog?.Invoke($"🔄 检测到页面导航: {e.Uri}");
                OnLog?.Invoke($"   导航类型: {(e.IsUserInitiated ? "用户触发" : "脚本触发")}, IsRedirected: {e.IsRedirected}");
                
                // 如果是登录页面导航（可能是登录失败后返回），记录日志
                if (e.Uri.Contains("login"))
                {
                    OnLog?.Invoke($"⚠️  警告：正在导航回登录页面，可能是登录失败！");
                }
            };
            
            // 绑定导航事件
            _webView.CoreWebView2.NavigationCompleted += async (s, e) =>
            {
                if (e.IsSuccess)
                {
                    OnLog?.Invoke($"✅ 页面加载完成: {_webView.CoreWebView2.Source}");
                    
                    // 🔥 如果返回登录页面，调用平台脚本的自动重新填充方法
                    var currentUrl = _webView.CoreWebView2.Source;
                    if (currentUrl.Contains("login") && _platformScript != null)
                    {
                        OnLog?.Invoke("🔄 检测到登录页面，调用自动重新填充...");
                        try
                        {
                            // 调用平台脚本的自动重新填充方法
                            var refillMethod = _platformScript.GetType().GetMethod("AutoRefillLoginForm");
                            if (refillMethod != null)
                            {
                                await Task.Delay(500); // 等待页面完全加载
                                await (Task)refillMethod.Invoke(_platformScript, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            OnLog?.Invoke($"⚠️  自动重新填充失败: {ex.Message}");
                        }
                    }
                    else
                    {
                        // 🔥 不在这里触发自动登录，因为 AutoBetService.StartBrowserInternal 会主动发送 Login 命令
                        // 这里只记录页面加载完成，等待主程序发送登录命令
                        OnLog?.Invoke("⏳ 等待主程序发送登录命令...");
                    }
                }
                else
                {
                    OnLog?.Invoke($"❌ 页面加载失败: HttpStatusCode={e.HttpStatusCode}");
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
            
            // 🔥 根据平台创建对应的脚本实例（参考 F5BotV2 BetSiteFactory）
            _platformScript = platformEnum switch
            {
                BetPlatform.不使用盘口 => CreateNoneSiteScript(logCallback),
                BetPlatform.元宇宙2 => CreateYYZ2Script(logCallback),
                BetPlatform.海峡 => CreateHaiXiaScript(logCallback),
                BetPlatform.QT => CreateQtScript(logCallback),
                BetPlatform.茅台 => CreateMt168Script(logCallback),
                BetPlatform.太平洋 => CreateMt168Script(logCallback), // 🔥 复用茅台脚本（F5BotV2也是如此）
                BetPlatform.蓝A => CreateLanAScript(logCallback),
                BetPlatform.红海 => CreateHongHaiScript(logCallback),
                BetPlatform.S880 => CreateS880Script(logCallback),
                BetPlatform.ADK => CreateADKScript(logCallback),
                BetPlatform.红海无名 => CreateHongHaiWuMingScript(logCallback),
                BetPlatform.果然 => CreateKk888Script(logCallback), // 🔥 Kk888（F5BotV2中的Kk888Member）
                BetPlatform.蓝B => CreateQtScript(logCallback), // 🔥 修正：蓝B 使用QT脚本（F5BotV2也是如此）
                BetPlatform.AC => CreateAcScript(logCallback),
                BetPlatform.通宝 => CreateTongBaoScript(logCallback),
                BetPlatform.通宝PC => CreateTongBaoPcScript(logCallback),
                BetPlatform.HY168 => CreateHy168Script(logCallback),
                BetPlatform.bingo168 => CreateHy168Script(logCallback), // 🔥 bingo168 使用HY168脚本
                BetPlatform.云顶 => CreateYunDing28Script(logCallback),
                BetPlatform.yyds => CreateYydsScript(logCallback), // 🔥 YYDS 平台
                _ => CreateNoneSiteScript(logCallback) // 默认使用"不使用盘口"
            };
        }
        
        /// <summary>
        /// 创建YYDS脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateYydsScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.YydsScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建YYDS脚本失败: {ex.Message}");
                return null;
            }
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
        /// 创建"不使用盘口"脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateNoneSiteScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.NoneSiteScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建NoneSite脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建元宇宙2脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateYYZ2Script(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.YYZ2Script(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建元宇宙2脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建海峡脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateHaiXiaScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.HaiXiaScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建海峡脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建QT脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateQtScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.QtScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建QT脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建茅台脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateMt168Script(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.Mt168Script(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建茅台脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建蓝A脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateLanAScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.LanAScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建蓝A脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建红海脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateHongHaiScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.HongHaiScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建红海脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建S880脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateS880Script(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.S880Script(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建S880脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建ADK脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateADKScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.ADKScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建ADK脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建红海无名脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateHongHaiWuMingScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.HongHaiWuMingScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建红海无名脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建AC脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateAcScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.AcScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建AC脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建通宝PC脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateTongBaoPcScript(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.TongBaoPcScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建通宝PC脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建Kk888脚本（果然）
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateKk888Script(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.Kk888Script(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建Kk888脚本失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建HY168脚本
        /// </summary>
        private PlatformScripts.IPlatformScript? CreateHy168Script(Action<string> logCallback)
        {
            try
            {
                return new PlatformScripts.Hy168bingoScript(_webView, logCallback);
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"❌ 创建HY168脚本失败: {ex.Message}");
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
        /// 尝试自动登录（已废弃，改为由主程序主动发送 Login 命令）
        /// 保留此方法以防万一，但不再自动调用
        /// </summary>
        [Obsolete("自动登录已改为由主程序主动发送 Login 命令，此方法不再使用")]
        private async Task TryAutoLoginAsync()
        {
            // 🔥 不再自动登录，等待主程序发送 Login 命令
            // 这样可以确保使用正确的账号密码，并且避免重复登录
            OnLog?.Invoke("ℹ️ 自动登录已禁用，等待主程序发送登录命令");
            await Task.CompletedTask;
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
                        
                        // 🔥 支持多种数据格式
                        string username = "";
                        string password = "";
                        
                        if (data is Newtonsoft.Json.Linq.JObject loginData)
                        {
                            // 是 JObject，直接读取
                            username = loginData["username"]?.ToString() ?? "";
                            password = loginData["password"]?.ToString() ?? "";
                        }
                        else if (data != null)
                        {
                            // 尝试序列化后反序列化
                            try
                            {
                                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                                var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
                                username = obj["username"]?.ToString() ?? "";
                                password = obj["password"]?.ToString() ?? "";
                            }
                            catch (Exception ex)
                            {
                                OnLog?.Invoke($"❌ 解析登录数据失败: {ex.Message}");
                                result.Success = false;
                                result.ErrorMessage = $"解析登录数据失败: {ex.Message}";
                                break;
                            }
                        }
                        
                        // 🔥 记录日志以便调试
                        OnLog?.Invoke($"📝 登录数据: username={(string.IsNullOrEmpty(username) ? "(空)" : username)}, password={(string.IsNullOrEmpty(password) ? "(空)" : "******")}");
                        
                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                        {
                            result.Success = false;
                            result.ErrorMessage = "账号或密码为空";
                            OnLog?.Invoke($"❌ 登录失败: 账号或密码为空");
                            break;
                        }
                        
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
                        
                        // 🔥 支持多种数据格式
                        if (data is SharedModels.BetStandardOrderList orderList)
                        {
                            // 直接是 BetStandardOrderList 对象
                            betOrders = orderList;
                        }
                        else if (data is Newtonsoft.Json.Linq.JArray jArray)
                        {
                            // 是 JArray，尝试反序列化
                            betOrders = jArray.ToObject<SharedModels.BetStandardOrderList>();
                        }
                        else if (data is Newtonsoft.Json.Linq.JObject betData)
                        {
                            // 是 JObject，尝试反序列化
                            betOrders = betData.ToObject<SharedModels.BetStandardOrderList>();
                        }
                        else if (data is string betContentString)
                        {
                            // 🔥 如果是字符串，尝试解析（兼容旧格式）
                            // 格式："1大10,2大10,3大10,4大10" 或 "1234大10"
                            try
                            {
                                // 先尝试解析为标准格式
                                var standardContent = zhaocaimao.Shared.Parsers.BetContentParser.ParseBetContentToString(betContentString);
                                // 获取当前期号（如果没有，使用0）
                                var currentIssueId = 0; // TODO: 从上下文获取期号
                                betOrders = zhaocaimao.Shared.Parsers.BetContentParser.ParseBetContentToOrderList(standardContent, currentIssueId);
                            }
                            catch (Exception parseEx)
                            {
                                OnLog?.Invoke($"❌ 解析投注内容失败: {parseEx.Message}");
                                result.Success = false;
                                result.ErrorMessage = $"解析投注内容失败: {parseEx.Message}";
                                break;
                            }
                        }
                        
                        if (betOrders == null || betOrders.Count == 0)
                        {
                            result.Success = false;
                            result.ErrorMessage = "投注内容为空";
                            OnLog?.Invoke($"❌ 投注内容为空，数据类型: {data?.GetType().Name ?? "null"}");
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
                        
                    case "获取Cookie":
                        // 获取Cookie命令
                        // WebView2 操作必须在 UI 线程执行
                        try
                        {
                            if (_webView?.CoreWebView2 == null)
                            {
                                result.Success = false;
                                result.ErrorMessage = "WebView2未初始化";
                                break;
                            }
                            
                            if (_webView.InvokeRequired)
                            {
                                var cookieResult = await Task.Run(async () =>
                                {
                                    var tcs = new TaskCompletionSource<(bool success, object? data, string message)>();
                                    _webView.Invoke(async () =>
                                    {
                                        try
                                        {
                                            var allCookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(_webView.CoreWebView2.Source);
                                            var cookieDict = new System.Collections.Generic.Dictionary<string, string>();
                                            
                                            foreach (var cookie in allCookies)
                                            {
                                                cookieDict[cookie.Name] = cookie.Value;
                                            }
                                            
                                            var cookieData = new
                                            {
                                                url = _webView.CoreWebView2.Source,
                                                cookies = cookieDict,
                                                count = allCookies.Count
                                            };
                                            tcs.SetResult((true, cookieData, $"获取成功,共{allCookies.Count}个Cookie"));
                                        }
                                        catch (Exception ex)
                                        {
                                            OnLog?.Invoke($"❌ 获取Cookie失败: {ex.Message}");
                                            tcs.SetResult((false, null, "获取Cookie失败"));
                                        }
                                    });
                                    return await tcs.Task;
                                });
                                result.Success = cookieResult.success;
                                result.Data = cookieResult.data;
                                result.ErrorMessage = cookieResult.success ? null : cookieResult.message;
                            }
                            else
                            {
                                var allCookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(_webView.CoreWebView2.Source);
                                var cookieDict = new System.Collections.Generic.Dictionary<string, string>();
                                
                                foreach (var cookie in allCookies)
                                {
                                    cookieDict[cookie.Name] = cookie.Value;
                                }
                                
                                result.Success = true;
                                result.Data = new
                                {
                                    url = _webView.CoreWebView2.Source,
                                    cookies = cookieDict,
                                    count = allCookies.Count
                                };
                                result.ErrorMessage = null;
                            }
                            
                            if (result.Success)
                            {
                                var count = (result.Data as dynamic)?.count ?? 0;
                                OnLog?.Invoke($"📤 获取Cookie完成:共{count}个");
                            }
                        }
                        catch (Exception cookieEx)
                        {
                            result.Success = false;
                            result.ErrorMessage = "获取Cookie失败";
                            result.Data = new { error = cookieEx.Message };
                            OnLog?.Invoke($"❌ 获取Cookie失败:{cookieEx.Message}");
                        }
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

