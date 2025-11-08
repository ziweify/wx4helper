using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using BsBrowserClient.Models;
using BsBrowserClient.Services;
using BsBrowserClient.PlatformScripts;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BaiShengVx3Plus.Shared.Platform;

namespace BsBrowserClient;

public partial class Form1 : Form
{
    private readonly string _configId;
    private readonly int _port;
    private readonly string _platform;
    private readonly string _platformUrl;
    
    private SocketServer? _socketServer;
    private IPlatformScript? _platformScript;
    private WebView2? _webView;
    private WebView2ResourceHandler? _resourceHandler;
    
    public Form1() : this("0", 9527, "YunDing28", "")
    {
    }
    
    public Form1(string configId, int port, string platform, string platformUrl)
    {
        InitializeComponent();
        
        _configId = configId;
        _port = port;
        _platform = platform;
        _platformUrl = string.IsNullOrEmpty(platformUrl) ? GetDefaultUrl(platform) : platformUrl;
        
        // 设置窗口标题（包含配置ID用于识别）
        this.Text = $"BsBrowser-{configId}";
        
        // 更新状态栏
        lblPort.Text = $"端口: {port} | 平台: {platform} | 配置: {configId}";
        txtUrl.Text = _platformUrl;
    }
    
    private async void Form1_Load(object sender, EventArgs e)
    {
        try
        {
            // 初始化 WebView2
            await InitializeWebView2Async();
            
            // 初始化平台脚本
            InitializePlatformScript();
            
            // 初始化 Socket 服务器
            InitializeSocketServer();
            
            lblStatus.Text = "✅ 初始化成功";
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"❌ 初始化失败: {ex.Message}";
            MessageBox.Show($"初始化失败: {ex.Message}", "错误", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    /// <summary>
    /// 初始化 WebView2 浏览器
    /// </summary>
    private async Task InitializeWebView2Async()
    {
        try
        {
            // 创建 WebView2 控件
            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            
            pnlBrowser.Controls.Add(_webView);
            
            // 等待 WebView2 初始化完成
            await _webView.EnsureCoreWebView2Async(null);
            
            // 初始化资源拦截器
            _resourceHandler = new WebView2ResourceHandler(OnResponseReceived);
            await _resourceHandler.InitializeAsync(_webView.CoreWebView2);
            
            // 启用 DevTools
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            
            // 导航到目标 URL
            _webView.CoreWebView2.Navigate(_platformUrl);
            txtUrl.Text = _platformUrl;
            
            // 绑定导航事件
            _webView.CoreWebView2.NavigationCompleted += async (s, e) =>
            {
                txtUrl.Text = _webView.CoreWebView2.Source;
                if (e.IsSuccess)
                {
                    lblStatus.Text = "✅ 页面加载完成";
                    OnLogMessage($"✅ 页面加载完成: {_webView.CoreWebView2.Source}");
                    
                    // 触发自动登录
                    await TryAutoLoginAsync();
                    
                    // 🔥 获取Cookie并回传到VxMain
                    await GetAndSendCookieToVxMain();
                }
                else
                {
                    lblStatus.Text = "❌ 页面加载失败";
                    OnLogMessage($"❌ 页面加载失败");
                }
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"WebView2 初始化失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 初始化平台脚本
    /// </summary>
    private void InitializePlatformScript()
    {
        // 使用共享库统一转换
        var platform = BetPlatformHelper.Parse(_platform);
        
        _platformScript = platform switch
        {
            BetPlatform.云顶 => new YunDing28Script(_webView!, OnLogMessage),
            BetPlatform.通宝 => new TongBaoScript(_webView!, OnLogMessage),
            BetPlatform.海峡 => new YunDing28Script(_webView!, OnLogMessage), // 暂用云顶脚本
            BetPlatform.红海 => new YunDing28Script(_webView!, OnLogMessage), // 暂用云顶脚本
            _ => new YunDing28Script(_webView!, OnLogMessage)
        };
    }
    
    private bool _isAutoLoginTriggered = false;
    
    /// <summary>
    /// 尝试自动登录（页面加载完成后触发）
    /// 参考 F5BotV2 的 LoginAsync 和 FrameLoadEnd 实现
    /// </summary>
    private async Task TryAutoLoginAsync()
    {
        if (_isAutoLoginTriggered || _platformScript == null)
            return;
        
        try
        {
            // 防止重复触发
            _isAutoLoginTriggered = true;
            
            OnLogMessage("🔍 检测页面状态，准备自动登录...");
            
            // 延迟一下，确保页面完全加载
            await Task.Delay(1000);
            
            // 从VxMain获取账号密码（通过Socket或HTTP）
            // 这里先用配置ID从HTTP API获取
            var username = "";
            var password = "";
            
            try
            {
                var httpClient = new System.Net.Http.HttpClient();
                var response = await httpClient.GetAsync($"http://127.0.0.1:8888/api/config?configId={_configId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    OnLogMessage($"📄 收到配置响应: {json.Substring(0, Math.Min(200, json.Length))}...");
                    
                    var config = Newtonsoft.Json.Linq.JObject.Parse(json);
                    if (config["success"]?.Value<bool>() ?? false)
                    {
                        username = config["data"]?["Username"]?.ToString() ?? "";
                        password = config["data"]?["Password"]?.ToString() ?? "";
                        
                        OnLogMessage($"✅ 获取到配置:");
                        OnLogMessage($"   用户名: {(string.IsNullOrEmpty(username) ? "(空)" : username)}");
                        OnLogMessage($"   密码: {(string.IsNullOrEmpty(password) ? "(空)" : "******")}");
                    }
                    else
                    {
                        OnLogMessage($"⚠️ API 返回 success=false");
                    }
                }
                else
                {
                    OnLogMessage($"⚠️ HTTP 请求失败: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                OnLogMessage($"⚠️ 获取配置异常: {ex.Message}");
            }
            
            // 如果没有账号密码，不自动登录
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                OnLogMessage("⚠️ 未配置账号密码，跳过自动登录");
                return;
            }
            
            // 调用平台脚本的登录方法
            OnLogMessage($"🔐 开始自动登录: {username}");
            var success = await _platformScript.LoginAsync(username, password);
            
            if (success)
            {
                OnLogMessage("✅ 自动登录成功！");
                
                // 通知VxMain登录成功（通过Socket）
                var message = new
                {
                    type = "login_success",
                    configId = _configId,
                    username = username,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                await _socketServer.SendToVxMain(message);
            }
            else
            {
                OnLogMessage("⚠️ 自动登录失败或超时，可能需要手动登录");
            }
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 自动登录异常: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 获取Cookie并发送到VxMain
    /// </summary>
    private async Task GetAndSendCookieToVxMain()
    {
        try
        {
            if (_webView?.CoreWebView2 == null)
            {
                OnLogMessage("⚠️ WebView2未初始化，无法获取Cookie");
                return;
            }
            
            // 获取当前页面的所有Cookie
            var cookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(_webView.CoreWebView2.Source);
            
            if (cookies == null || cookies.Count == 0)
            {
                OnLogMessage("ℹ️ 当前页面没有Cookie");
                return;
            }
            
            // 将Cookie格式化为字符串
            var cookieDict = new Dictionary<string, string>();
            foreach (var cookie in cookies)
            {
                cookieDict[cookie.Name] = cookie.Value;
            }
            
            // 通知VxMain（通过Socket）
            var message = new
            {
                type = "cookie_update",
                configId = _configId,
                url = _webView.CoreWebView2.Source,
                cookies = cookieDict,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            await _socketServer.SendToVxMain(message);
            
            OnLogMessage($"📤 Cookie已回传到VxMain:共{cookies.Count}个Cookie");
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 获取Cookie异常: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 初始化 Socket 服务器
    /// </summary>
    private void InitializeSocketServer()
    {
        // 解析配置ID
        if (!int.TryParse(_configId, out var configIdInt))
        {
            configIdInt = 0;
        }
        
        _socketServer = new SocketServer(configIdInt, OnCommandReceived, OnLogMessage);
        _socketServer.Start();
        
        lblPort.Text = $"配置: {_configId} | 平台: {_platform}";
    }
    
    /// <summary>
    /// 响应接收回调 - 处理拦截到的数据
    /// </summary>
    private void OnResponseReceived(ResponseEventArgs args)
    {
        try
        {
            // 只处理感兴趣的 URL
            if (string.IsNullOrEmpty(args.Url))
                return;
            
            // 记录日志
            OnLogMessage($"[拦截] {args.Url}");
            
            if (!string.IsNullOrEmpty(args.PostData))
            {
                OnLogMessage($"[POST] {args.PostData.Substring(0, Math.Min(100, args.PostData.Length))}...");
            }
            
            if (!string.IsNullOrEmpty(args.Context))
            {
                OnLogMessage($"[Response] Status={args.StatusCode}, Length={args.Context.Length}");
                
                // 可以在这里解析响应，提取投注结果等
                // 例如：如果是投注结果，可以通过 Socket 发送给主程序
            }
            
            // 让平台脚本处理响应
            _platformScript?.HandleResponse(args);
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 响应处理失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Socket 命令接收回调
    /// </summary>
    private async void OnCommandReceived(CommandRequest command)
    {
        try
        {
            OnLogMessage($"[命令] {command.Command}");
            
            var response = new CommandResponse
            {
                ConfigId = _configId,
                Success = false
            };
            
            switch (command.Command)
            {
                case "显示窗口":
                    // 显示窗口
                    if (InvokeRequired)
                    {
                        Invoke(() =>
                        {
                            this.Show();
                            this.WindowState = FormWindowState.Normal;
                            this.Activate();
                        });
                    }
                    else
                    {
                        this.Show();
                        this.WindowState = FormWindowState.Normal;
                        this.Activate();
                    }
                    response.Success = true;
                    response.Message = "窗口已显示";
                    break;
                    
                case "隐藏窗口":
                    // 隐藏窗口
                    if (InvokeRequired)
                    {
                        Invoke(() => this.Hide());
                    }
                    else
                    {
                        this.Hide();
                    }
                    response.Success = true;
                    response.Message = "窗口已隐藏";
                    break;
                    
                case "心跳检测":
                    // 心跳检测
                    response.Success = true;
                    response.Message = "Pong";
                    response.Data = new 
                    { 
                        configId = _configId,
                        platform = _platform,
                        processId = Environment.ProcessId
                    };
                    break;
                    
                case "封盘通知":
                    // 封盘通知 - 拉取订单并投注
                    var notifyData = command.Data as JObject;
                    var issueId = notifyData?["issueId"]?.ToString() ?? "";
                    var secondsRemaining = notifyData?["secondsRemaining"]?.ToObject<int>() ?? 0;
                    
                    OnLogMessage($"⏰ 封盘通知:期号{issueId} 剩余{secondsRemaining}秒");
                    
                    // 通过 HTTP 拉取订单并投注
                    var betResult = await FetchOrdersAndBetAsync(issueId);
                    response.Success = betResult.success;
                    response.Message = betResult.message;
                    break;
                    
                case "登录":
                    var loginData = command.Data as JObject;
                    var username = loginData?["username"]?.ToString() ?? "";
                    var password = loginData?["password"]?.ToString() ?? "";
                    
                    response.Success = await _platformScript!.LoginAsync(username, password);
                    response.Message = response.Success ? "登录成功" : "登录失败";
                    break;
                    
                case "获取余额":
                    var balance = await _platformScript!.GetBalanceAsync();
                    response.Success = balance >= 0;
                    response.Data = new { balance };
                    response.Message = response.Success ? $"余额: {balance}" : "获取余额失败";
                    break;
                    
                case "获取Cookie":
                    // 获取Cookie命令
                    try
                    {
                        if (_webView?.CoreWebView2 == null)
                        {
                            response.Message = "WebView2未初始化";
                            break;
                        }
                        
                        var allCookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(_webView.CoreWebView2.Source);
                        var cookieDict = new Dictionary<string, string>();
                        
                        foreach (var cookie in allCookies)
                        {
                            cookieDict[cookie.Name] = cookie.Value;
                        }
                        
                        response.Success = true;
                        response.Data = new 
                        { 
                            url = _webView.CoreWebView2.Source,
                            cookies = cookieDict,
                            count = allCookies.Count
                        };
                        response.Message = $"获取成功,共{allCookies.Count}个Cookie";
                        
                        OnLogMessage($"📤 获取Cookie完成:共{allCookies.Count}个");
                    }
                    catch (Exception cookieEx)
                    {
                        response.Success = false;
                        response.Message = "获取Cookie失败";
                        response.ErrorMessage = cookieEx.Message;
                        OnLogMessage($"❌ 获取Cookie失败:{cookieEx.Message}");
                    }
                    break;
                    
                case "获取盘口额度":
                    // 获取盘口额度命令
                    try
                    {
                        var quotaBalance = await _platformScript!.GetBalanceAsync();
                        response.Success = quotaBalance >= 0;
                        response.Data = new { balance = quotaBalance, quota = quotaBalance };
                        response.Message = response.Success ? $"盘口额度: {quotaBalance}元" : "获取额度失败";
                        
                        OnLogMessage($"📊 盘口额度:{quotaBalance}元");
                    }
                    catch (Exception quotaEx)
                    {
                        response.Success = false;
                        response.Message = "获取额度失败";
                        response.ErrorMessage = quotaEx.Message;
                        OnLogMessage($"❌ 获取额度失败:{quotaEx.Message}");
                    }
                    break;
                    
                case "投注":
                    // 新的投注流程：接收标准投注内容，执行投注，返回详细结果
                    var betData = command.Data as JObject;
                    var betIssueId = betData?["issueId"]?.ToString() ?? "";
                    var betContent = betData?["betContent"]?.ToString() ?? "";
                    
                    OnLogMessage($"📝 收到投注命令:期号{betIssueId} 内容:{betContent}");
                    
                    if (string.IsNullOrEmpty(betContent))
                    {
                        response.Message = "投注内容为空";
                        break;
                    }
                    
                    // 记录POST前时间
                    var postStartTime = DateTime.Now;
                    
                    try
                    {
                        OnLogMessage($"📦 准备投注:期号={betIssueId} 内容={betContent}");
                        
                        // 🔥 参考F5BotV2：将所有投注项组装成一个包，一次性POST
                        // betContent格式："1大10,2大10,3大10,4大10"
                        // 不需要拆分逐个投注，而是整体发送给平台脚本
                        // 平台脚本内部会将betContent解析并组装成一个POST请求
                        
                        var betOrder = new BetOrder
                        {
                            IssueId = betIssueId,
                            BetContent = betContent,  // 🔥 完整的投注内容，不拆分
                            Amount = 0  // 金额已包含在内容中
                        };
                        
                        // 🔥 平台脚本内部会：
                        //    1. 解析 betContent："1大10,2大10,3大10,4大10"
                        //    2. 组装成 JSON数组：[{id:1,money:10},{id:2,money:10},...]
                        //    3. 一次性POST请求到平台
                        var (success, orderId) = await _platformScript!.PlaceBetAsync(betOrder);
                        
                        // 记录POST后时间
                        var postEndTime = DateTime.Now;
                        var durationMs = (int)(postEndTime - postStartTime).TotalMilliseconds;
                        
                        response.Success = success;
                        response.Message = success ? "投注成功" : "投注失败";
                        response.Data = new
                        {
                            postStartTime = postStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            postEndTime = postEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            durationMs = durationMs,
                            orderNo = orderId
                        };
                        
                        OnLogMessage($"✅ 投注完成:成功={success} 耗时={durationMs}ms 订单号={orderId}");
                        OnLogMessage($"📊 返回数据:postStartTime={postStartTime:yyyy-MM-dd HH:mm:ss.fff}, postEndTime={postEndTime:yyyy-MM-dd HH:mm:ss.fff}");
                    }
                    catch (Exception betEx)
                    {
                        var postEndTime = DateTime.Now;
                        var durationMs = (int)(postEndTime - postStartTime).TotalMilliseconds;
                        
                        response.Success = false;
                        response.Message = "投注异常";
                        response.ErrorMessage = betEx.Message;
                        response.Data = new
                        {
                            postStartTime = postStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            postEndTime = postEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            durationMs = durationMs
                        };
                        
                        OnLogMessage($"❌ 投注异常:{betEx.Message}");
                    }
                    break;
                    
                default:
                    response.Message = $"未知命令: {command.Command}";
                    OnLogMessage($"⚠️ 未知命令: {command.Command}");
                    break;
            }
            
            // 发送响应
            _socketServer?.SendResponse(response);
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 命令处理失败: {ex.Message}");
            
            var errorResponse = new CommandResponse
            {
                ConfigId = _configId,
                Success = false,
                Message = ex.Message
            };
            
            _socketServer?.SendResponse(errorResponse);
        }
    }
    
    /// <summary>
    /// 日志回调
    /// </summary>
    private void OnLogMessage(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnLogMessage(message));
            return;
        }
        
        // 输出到状态栏
        lblStatus.Text = message;
        
        // 输出到日志文本框
        var time = DateTime.Now.ToString("HH:mm:ss.fff");
        txtLog.AppendText($"[{time}] {message}\r\n");
        
        // 自动滚动到底部
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
        
        // 输出到控制台（用于调试）
        Console.WriteLine($"[{time}] {message}");
    }
    
    /// <summary>
    /// 获取默认 URL
    /// </summary>
    private string GetDefaultUrl(string platform)
    {
        // 使用共享库统一获取URL
        return BetPlatformHelper.GetDefaultUrl(platform);
    }
    
    #region UI 事件处理
    
    private void btnNavigate_Click(object sender, EventArgs e)
    {
        if (_webView?.CoreWebView2 != null && !string.IsNullOrWhiteSpace(txtUrl.Text))
        {
            _webView.CoreWebView2.Navigate(txtUrl.Text);
        }
    }
    
    private void btnRefresh_Click(object sender, EventArgs e)
    {
        _webView?.CoreWebView2?.Reload();
    }
    
    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        // 拦截关闭事件，改为隐藏窗口
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true; // 取消关闭
            this.Hide();     // 隐藏窗口
            OnLogMessage($"窗口已隐藏（进程仍在运行）");
        }
        else
        {
            // 程序退出时才真正清理资源
            _socketServer?.Stop();
            _webView?.Dispose();
        }
    }
    
    /// <summary>
    /// 拉取订单并投注
    /// </summary>
    private async Task<(bool success, string message)> FetchOrdersAndBetAsync(string issueId)
    {
        try
        {
            OnLogMessage($"📥 开始拉取订单:期号{issueId}");
            
            // 1. 通过 HTTP 拉取订单列表
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://127.0.0.1:8888/api/order?issueId={issueId}");
            
            if (!response.IsSuccessStatusCode)
            {
                OnLogMessage($"❌ 拉取订单失败:HTTP {response.StatusCode}");
                return (false, $"HTTP请求失败:{response.StatusCode}");
            }
            
            var json = await response.Content.ReadAsStringAsync();
            OnLogMessage($"📦 收到响应:{json.Substring(0, Math.Min(200, json.Length))}...");
            
            var data = JsonConvert.DeserializeObject<JObject>(json);
            var success = data?["success"]?.ToObject<bool>() ?? false;
            var count = data?["count"]?.ToObject<int>() ?? 0;
            
            if (!success || count == 0)
            {
                OnLogMessage($"📭 没有待投注订单:期号{issueId}");
                return (true, "没有待投注订单");
            }
            
            // 2. 解析订单列表
            var orders = data?["data"]?.ToObject<List<JObject>>();
            if (orders == null || orders.Count == 0)
            {
                OnLogMessage($"❌ 订单数据解析失败");
                return (false, "订单数据解析失败");
            }
            
            OnLogMessage($"✅ 获取到 {orders.Count} 个待投注订单");
            
            // 3. 调用平台脚本投注
            // TODO: 需要实现订单合并逻辑，参考 F5BotV2
            foreach (var order in orders)
            {
                var orderType = order["OrderType"]?.ToString() ?? "";
                var betContent = order["BetContentStandar"]?.ToString() ?? "";
                var amount = order["Amount"]?.ToObject<float>() ?? 0;
                var memberName = order["MemberName"]?.ToString() ?? "";
                
                OnLogMessage($"📝 订单:{memberName} {orderType} {betContent} {amount}元");
            }
            
            OnLogMessage($"⚠️ 投注功能待实现，需要参考 F5BotV2 实现订单合并和组装");
            return (true, $"收到{orders.Count}个订单，投注功能待实现");
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 拉取订单异常:{ex.Message}");
            return (false, $"拉取订单异常:{ex.Message}");
        }
    }
    
    #endregion
    
    #region 测试按钮
    
    /// <summary>
    /// 测试Cookie按钮 - 获取并显示当前Cookie
    /// </summary>
    private async void btnTestCookie_Click(object? sender, EventArgs e)
    {
        try
        {
            OnLogMessage("🍪 【测试】开始获取Cookie...");
            
            if (_webView?.CoreWebView2 == null)
            {
                OnLogMessage("❌ WebView2未初始化");
                return;
            }
            
            // 方法1：通过WebView2 API获取Cookie
            OnLogMessage("📋 方法1：WebView2 API");
            var cookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(_webView.CoreWebView2.Source);
            OnLogMessage($"   获取到{cookies.Count}个Cookie:");
            
            foreach (var cookie in cookies)
            {
                OnLogMessage($"   - {cookie.Name}={cookie.Value.Substring(0, Math.Min(20, cookie.Value.Length))}...");
            }
            
            // 方法2：通过JavaScript获取document.cookie
            OnLogMessage("📋 方法2：JavaScript document.cookie");
            var script = @"
                (function() {
                    return document.cookie;
                })();
            ";
            
            var jsCookie = await _webView.CoreWebView2.ExecuteScriptAsync(script);
            jsCookie = jsCookie.Trim('"').Replace("\\", "");
            OnLogMessage($"   document.cookie={jsCookie.Substring(0, Math.Min(100, jsCookie.Length))}...");
            
            // 方法3：通过拦截获取的Cookie（显示当前已拦截的参数）
            OnLogMessage("📋 方法3：拦截到的关键参数");
            if (_platformScript != null)
            {
                var tongBaoScript = _platformScript as PlatformScripts.TongBaoScript;
                if (tongBaoScript != null)
                {
                    // 通过反射获取私有字段（用于测试）
                    var typeInfo = tongBaoScript.GetType();
                    var sidField = typeInfo.GetField("_sid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var uuidField = typeInfo.GetField("_uuid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var tokenField = typeInfo.GetField("_token", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    var sid = sidField?.GetValue(tongBaoScript)?.ToString() ?? "";
                    var uuid = uuidField?.GetValue(tongBaoScript)?.ToString() ?? "";
                    var token = tokenField?.GetValue(tongBaoScript)?.ToString() ?? "";
                    
                    OnLogMessage($"   sid={sid.Substring(0, Math.Min(20, sid.Length))}... ({sid.Length}字符)");
                    OnLogMessage($"   uuid={uuid}");
                    OnLogMessage($"   token={token.Substring(0, Math.Min(20, token.Length))}... ({token.Length}字符)");
                    
                    if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(uuid) || string.IsNullOrEmpty(token))
                    {
                        OnLogMessage("⚠️ 警告：关键参数未拦截到！请刷新页面或执行操作触发拦截。");
                    }
                    else
                    {
                        OnLogMessage("✅ 关键参数已拦截，可以进行投注");
                    }
                }
            }
            
            OnLogMessage("🍪 【测试】Cookie获取完成");
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 获取Cookie失败:{ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试投注按钮 - 固定投注"1大10"
    /// </summary>
    private async void btnTestBet_Click(object? sender, EventArgs e)
    {
        try
        {
            OnLogMessage("🎲 【测试】开始投注测试...");
            OnLogMessage("   固定投注内容:1大10");
            
            if (_platformScript == null)
            {
                OnLogMessage("❌ 平台脚本未初始化");
                return;
            }
            
            // 测试投注"1大10"
            var testBetContent = "1大10";
            var betOrder = new BetOrder
            {
                IssueId = "0",  // 测试用，期号为0
                BetContent = testBetContent,
                Amount = 0
            };
            
            OnLogMessage($"📤 调用PlaceBetAsync:内容={testBetContent}");
            var startTime = DateTime.Now;
            
            var (success, orderId) = await _platformScript.PlaceBetAsync(betOrder);
            
            var endTime = DateTime.Now;
            var duration = (int)(endTime - startTime).TotalMilliseconds;
            
            if (success)
            {
                OnLogMessage($"✅ 【测试】投注成功！");
                OnLogMessage($"   订单号:{orderId}");
                OnLogMessage($"   耗时:{duration}ms");
            }
            else
            {
                OnLogMessage($"❌ 【测试】投注失败");
                OnLogMessage($"   耗时:{duration}ms");
            }
            
            OnLogMessage("🎲 【测试】投注测试完成");
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 投注测试失败:{ex.Message}");
            OnLogMessage($"   堆栈:{ex.StackTrace}");
        }
    }
    
    #endregion
}

