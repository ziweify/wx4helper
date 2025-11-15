using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommandRequest = BsBrowserClient.Models.CommandRequest;  // 🔥 使用别名避免类型冲突
using CommandResponse = BsBrowserClient.Models.CommandResponse;  // 🔥 命令响应
using BsBrowserClient.Services;
using BsBrowserClient.PlatformScripts;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BaiShengVx3Plus.Shared.Platform;
using BaiShengVx3Plus.Shared.Models;  // 🔥 使用共享的模型

namespace BsBrowserClient;

public partial class Form1 : Form
{
    private readonly string _configId;
    private readonly string _configName;  // 🔥 新增配置名
    private readonly int _port;
    private readonly string _platform;
    private readonly string _platformUrl;
    
    private SocketServer? _socketServer;
    private IPlatformScript? _platformScript;
    private WebView2? _webView;
    private WebView2ResourceHandler? _resourceHandler;
    
    public Form1() : this("0", "未命名配置", 9527, "YunDing28", "")
    {
    }
    
    public Form1(string configId, string configName, int port, string platform, string platformUrl)
    {
        InitializeComponent();
        
        _configId = configId;
        _configName = configName;  // 🔥 保存配置名
        _port = port;
        _platform = platform;
        _platformUrl = string.IsNullOrEmpty(platformUrl) ? GetDefaultUrl(platform) : platformUrl;
        
        // 🔥 设置窗口标题（显示配置名，用于观察）
        this.Text = $"BsBrowser-{configName}";
        
        // 更新状态栏
        lblPort.Text = $"配置: {configName} (ID:{configId}) | 平台: {platform}";
        txtUrl.Text = _platformUrl;
    }
    
    private async void Form1_Load(object sender, EventArgs e)
    {
        try
        {
            // 初始化日志系统（优先初始化，以便记录后续日志）
            InitializeLogSystem();
            
            OnLogMessage("🚀 正在初始化 BrowserClient...");
            
            // 初始化 WebView2
            await InitializeWebView2Async();
            OnLogMessage("✅ WebView2 初始化完成");
            
            // 初始化平台脚本
            InitializePlatformScript();
            OnLogMessage($"✅ 平台脚本初始化完成: {_platform}");
            
            // 初始化 Socket 服务器
            InitializeSocketServer();
            OnLogMessage($"✅ Socket服务器启动: 端口{_port}", LogType.Socket);
            
            lblStatus.Text = "✅ 初始化成功";
            OnLogMessage("🎉 BrowserClient 初始化成功");
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"❌ 初始化失败: {ex.Message}";
            OnLogMessage($"❌ 初始化失败: {ex.Message}");
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
            
            // 🔥 为每个实例创建独立的用户数据文件夹，避免资源冲突
            // 使用 AppData\Local 目录，无需管理员权限
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BsBrowserClient",
                "WebView2Data",
                $"Config_{_configId}");
            
            // 确保目录存在
            Directory.CreateDirectory(userDataFolder);
            
            // 使用自定义用户数据文件夹初始化 WebView2
            var environment = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: userDataFolder,
                options: null);
            
            // 等待 WebView2 初始化完成
            await _webView.EnsureCoreWebView2Async(environment);
            
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
        
        // 创建一个兼容的日志回调（平台脚本的日志都视为投注类型）
        Action<string> betLogCallback = (msg) => OnLogMessage(msg, LogType.Bet);
        
        _platformScript = platform switch
        {
            BetPlatform.云顶 => new YunDing28Script(_webView!, betLogCallback),
            BetPlatform.通宝 => new TongBaoScript(_webView!, betLogCallback),
            BetPlatform.海峡 => new YunDing28Script(_webView!, betLogCallback), // 暂用云顶脚本
            BetPlatform.红海 => new YunDing28Script(_webView!, betLogCallback), // 暂用云顶脚本
            _ => new YunDing28Script(_webView!, betLogCallback)
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
            
            // 🔥 等待页面完全加载（包括 JavaScript 执行完成）
            await Task.Delay(2000);  // 增加到2秒
            
            // 🔥 额外等待 DOMContentLoaded
            try
            {
                await _webView!.CoreWebView2.ExecuteScriptAsync(@"
                    new Promise((resolve) => {
                        if (document.readyState === 'complete') {
                            resolve();
                        } else {
                            window.addEventListener('load', resolve);
                        }
                    });
                ");
                OnLogMessage("✅ 页面DOM已完全加载");
            }
            catch
            {
                OnLogMessage("⚠️ DOM检测失败，继续尝试登录");
            }
            
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
        
        // 创建一个兼容的日志回调（Socket服务器的日志视为Socket类型）
        Action<string> socketLogCallback = (msg) => OnLogMessage(msg, LogType.Socket);
        
        // 🔥 包装异步方法为同步调用（使用 .Wait()）
        void CommandReceivedWrapper(CommandRequest cmd)
        {
            // 同步等待异步方法完成，确保响应在返回前发送
            OnCommandReceivedAsync(cmd).Wait();
        }
        
        _socketServer = new SocketServer(configIdInt, _configName, CommandReceivedWrapper, socketLogCallback);  // 🔥 传入配置名
        
        // 订阅连接状态变化事件
        _socketServer.StatusChanged += OnSocketStatusChanged;
        
        _socketServer.Start();
        
        lblPort.Text = $"配置: {_configId} | 平台: {_platform}";
    }
    
    /// <summary>
    /// Socket 连接状态变化回调
    /// </summary>
    private void OnSocketStatusChanged(object? sender, Services.ConnectionStatus status)
    {
        // 跨线程更新 UI
        if (lblStatus.GetCurrentParent()?.InvokeRequired ?? false)
        {
            lblStatus.GetCurrentParent()?.Invoke(() => UpdateConnectionStatus(status));
        }
        else
        {
            UpdateConnectionStatus(status);
        }
    }
    
    /// <summary>
    /// 更新连接状态显示
    /// </summary>
    private void UpdateConnectionStatus(Services.ConnectionStatus status)
    {
        var (text, color) = status switch
        {
            Services.ConnectionStatus.断开 => ("● 未连接 VxMain", System.Drawing.Color.Red),
            Services.ConnectionStatus.连接中 => ("● 连接中...", System.Drawing.Color.Orange),
            Services.ConnectionStatus.已连接 => ("● 已连接 VxMain", System.Drawing.Color.Green),
            Services.ConnectionStatus.重连中 => ("● 重连中...", System.Drawing.Color.Orange),
            _ => ("● 未知状态", System.Drawing.Color.Gray)
        };
        
        lblStatus.Text = text;
        lblStatus.ForeColor = color;
        
        OnLogMessage($"🔄 连接状态: {text}", LogType.Socket);
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
            
            // 记录日志（HTTP拦截）
            OnLogMessage($"拦截:{args.Url}", LogType.Http);
            
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
    private async Task OnCommandReceivedAsync(CommandRequest command)
    {
        try
        {
            OnLogMessage($"收到命令:{command.Command}", LogType.Socket);
            
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
                    // 新的投注流程：接收标准化订单列表，执行投注，返回详细结果
                    BaiShengVx3Plus.Shared.Models.BetStandardOrderList? betOrders = null;
                    
                    // 🔥 BetStandardOrderList 序列化后可能是数组（JArray）或对象（JObject）
                    if (command.Data is Newtonsoft.Json.Linq.JArray jArray)
                    {
                        // 如果是数组，直接反序列化
                        betOrders = jArray.ToObject<BaiShengVx3Plus.Shared.Models.BetStandardOrderList>();
                    }
                    else if (command.Data is JObject betData)
                    {
                        // 如果是对象，尝试反序列化
                        betOrders = betData.ToObject<BaiShengVx3Plus.Shared.Models.BetStandardOrderList>();
                    }
                    
                    if (betOrders == null || betOrders.Count == 0)
                    {
                        response.Message = "投注内容为空";
                        response.ErrorMessage = "投注内容解析失败：无法将数据转换为 BetStandardOrderList";
                        OnLogMessage($"❌ 投注内容为空", LogType.Bet);
                        OnLogMessage($"   数据类型: {command.Data?.GetType().Name ?? "null"}", LogType.Bet);
                        break;
                    }
                    
                    var betIssueId = betOrders[0].IssueId;
                    var totalAmount = betOrders.GetTotalAmount();
                    
                    OnLogMessage($"📝 收到投注命令:期号{betIssueId} 共{betOrders.Count}项 {totalAmount}元", LogType.Bet);
                    
                    // 记录POST前时间
                    var postStartTime = DateTime.Now;
                    
                    try
                    {
                        OnLogMessage($"📦 准备投注:期号={betIssueId} 共{betOrders.Count}项 {totalAmount}元", LogType.Bet);
                        
                        // 🔥 使用标准化订单列表，平台脚本将其转换为平台特定的格式
                        var (success, orderId, platformResponse) = await _platformScript!.PlaceBetAsync(betOrders);
                        
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
                            orderNo = orderId,
                            platformResponse = platformResponse  // 🔥 包含平台完整响应
                        };
                        
                        OnLogMessage($"✅ 投注完成:成功={success} 耗时={durationMs}ms 订单号={orderId}", LogType.Bet);
                        OnLogMessage($"📊 平台响应:{platformResponse}");
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
    /// <summary>
    /// 日志缓冲区（高性能循环队列）
    /// </summary>
    private readonly Queue<string> _logBuffer = new Queue<string>();
    private const int MAX_LOG_LINES = 1000;  // 最大保留1000行日志
    private bool _isUserScrolling = false;   // 用户是否在查看历史
    private System.Windows.Forms.Timer? _logTimer;  // 日志批量更新定时器
    
    /// <summary>
    /// 初始化日志系统
    /// </summary>
    private void InitializeLogSystem()
    {
        // 创建日志更新定时器（每100ms批量更新一次，避免频繁UI刷新）
        _logTimer = new System.Windows.Forms.Timer();
        _logTimer.Interval = 100;  // 100ms
        _logTimer.Tick += LogTimer_Tick;
        _logTimer.Start();
        
        // 监听滚动条事件
        txtLog.VScroll += TxtLog_VScroll;
        txtLog.MouseWheel += TxtLog_MouseWheel;
    }
    
    /// <summary>
    /// 日志定时器 - 批量更新UI
    /// </summary>
    private void LogTimer_Tick(object? sender, EventArgs e)
    {
        int bufferCount = 0;
        lock (_logBuffer)
        {
            bufferCount = _logBuffer.Count;
        }
        
        if (bufferCount == 0)
        {
            // 更新日志状态（显示当前状态）
            UpdateLogStatus();
            return;
        }
        
        // 批量处理日志
        var logs = new List<string>();
        lock (_logBuffer)
        {
            while (_logBuffer.Count > 0 && logs.Count < 50)  // 每次最多处理50条
            {
                logs.Add(_logBuffer.Dequeue());
            }
        }
        
        if (logs.Count == 0) return;
        
        // 检查是否需要自动滚动
        bool shouldAutoScroll = !_isUserScrolling && IsScrollAtBottom();
        
        // 批量添加日志
        txtLog.SuspendLayout();
        try
        {
            foreach (var log in logs)
            {
                txtLog.AppendText(log);
            }
            
            // 限制日志行数（保持性能）
            int lineCount = txtLog.Lines.Length;
            if (lineCount > MAX_LOG_LINES)
            {
                // 删除前面的旧日志
                int removeLines = lineCount - MAX_LOG_LINES;
                int removePos = 0;
                for (int i = 0; i < removeLines; i++)
                {
                    removePos = txtLog.Text.IndexOf('\n', removePos) + 1;
                }
                txtLog.Text = txtLog.Text.Substring(removePos);
            }
            
            // 自动滚动到底部
            if (shouldAutoScroll)
            {
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.ScrollToCaret();
            }
        }
        finally
        {
            txtLog.ResumeLayout();
        }
        
        // 更新日志状态
        UpdateLogStatus();
    }
    
    /// <summary>
    /// 更新日志状态显示
    /// </summary>
    private void UpdateLogStatus()
    {
        int bufferCount = 0;
        lock (_logBuffer)
        {
            bufferCount = _logBuffer.Count;
        }
        
        int lineCount = txtLog.Lines.Length;
        string autoScrollStatus = _isUserScrolling ? "关" : "开";
        
        lblLogStatus.Text = $"📊 日志: {lineCount}行 | 缓冲: {bufferCount} | 自动滚动: {autoScrollStatus}";
    }
    
    /// <summary>
    /// 检查滚动条是否在底部
    /// </summary>
    private bool IsScrollAtBottom()
    {
        if (txtLog.Lines.Length == 0) return true;
        
        // 获取可见行数
        int visibleLines = txtLog.Height / txtLog.Font.Height;
        int totalLines = txtLog.Lines.Length;
        
        // 获取第一个可见字符的行号
        int firstVisibleLine = txtLog.GetLineFromCharIndex(txtLog.GetCharIndexFromPosition(new Point(0, 0)));
        
        // 如果底部可见，则认为在底部
        return (firstVisibleLine + visibleLines >= totalLines - 2);
    }
    
    /// <summary>
    /// 滚动条滚动事件
    /// </summary>
    private void TxtLog_VScroll(object? sender, EventArgs e)
    {
        // 用户手动滚动，标记为正在查看历史
        _isUserScrolling = !IsScrollAtBottom();
    }
    
    /// <summary>
    /// 鼠标滚轮事件
    /// </summary>
    private void TxtLog_MouseWheel(object? sender, MouseEventArgs e)
    {
        // 用户使用滚轮，标记为正在查看历史
        _isUserScrolling = !IsScrollAtBottom();
    }
    
    /// <summary>
    /// 日志回调（高性能版本）
    /// </summary>
    /// <summary>
    /// 日志类型枚举
    /// </summary>
    private enum LogType
    {
        Socket,   // Socket通信
        Bet,      // 投注相关
        Http,     // HTTP拦截
        System    // 系统消息
    }
    
    /// <summary>
    /// 写入日志（带类型过滤）
    /// </summary>
    private void OnLogMessage(string message, LogType type = LogType.System)
    {
        // 根据复选框状态过滤日志
        bool shouldLog = type switch
        {
            LogType.Socket => chkLogSocket?.Checked ?? true,
            LogType.Bet => chkLogBet?.Checked ?? true,
            LogType.Http => chkLogHttp?.Checked ?? false,
            LogType.System => chkLogSystem?.Checked ?? true,
            _ => true
        };
        
        if (!shouldLog) return;
        
        // 输出到状态栏
        if (InvokeRequired)
        {
            BeginInvoke(() => lblStatus.Text = message);
        }
        else
        {
            lblStatus.Text = message;
        }
        
        // 添加到日志缓冲区（异步处理，不阻塞）
        var time = DateTime.Now.ToString("HH:mm:ss.fff");
        var typeIcon = type switch
        {
            LogType.Socket => "🔌",
            LogType.Bet => "🎲",
            LogType.Http => "🌐",
            LogType.System => "⚙️",
            _ => "📝"
        };
        var logLine = $"[{time}] {typeIcon} {message}\r\n";
        
        lock (_logBuffer)
        {
            _logBuffer.Enqueue(logLine);
            
            // 如果缓冲区过大，丢弃旧日志（防止内存溢出）
            while (_logBuffer.Count > MAX_LOG_LINES * 2)
            {
                _logBuffer.Dequeue();
            }
        }
        
        // 输出到控制台（用于调试）
        Console.WriteLine($"[{time}] [{type}] {message}");
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
        // 拦截用户点击关闭按钮的事件
        if (e.CloseReason == CloseReason.UserClosing)
        {
            // 弹出确认对话框
            var result = MessageBox.Show(
                "请选择操作：\n\n" +
                "• 是(Y)：关闭浏览器（进程退出）\n" +
                "• 否(N)：最小化到任务栏\n" +
                "• 取消：继续使用",
                "关闭确认 - BsBrowser",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2  // 默认选择"否"(最小化)
            );
            
            switch (result)
            {
                case DialogResult.Yes:
                    // 用户选择关闭：允许关闭，清理资源
                    OnLogMessage($"用户选择关闭浏览器，进程即将退出");
                    _socketServer?.Stop();
                    _webView?.Dispose();
                    // 不取消关闭事件，允许窗口关闭
                    break;
                    
                case DialogResult.No:
                    // 用户选择最小化：取消关闭，隐藏窗口
                    e.Cancel = true;
                    this.WindowState = FormWindowState.Minimized;
                    OnLogMessage($"窗口已最小化（进程仍在运行）");
                    break;
                    
                case DialogResult.Cancel:
                default:
                    // 用户选择取消：取消关闭，保持窗口显示
                    e.Cancel = true;
                    OnLogMessage($"取消关闭");
                    break;
            }
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
            
            // 先获取余额，确认已登录
            OnLogMessage("📊 检查登录状态和余额...");
            var balance = await _platformScript.GetBalanceAsync();
            if (balance < 0)
            {
                OnLogMessage("❌ 未登录或获取余额失败，无法投注");
                return;
            }
            OnLogMessage($"✅ 当前余额: ¥{balance}");
            
            // 测试投注"1大10"
            var testOrders = new BaiShengVx3Plus.Shared.Models.BetStandardOrderList
            {
                new BaiShengVx3Plus.Shared.Models.BetStandardOrder(0, BaiShengVx3Plus.Shared.Models.CarNumEnum.P1, BaiShengVx3Plus.Shared.Models.BetPlayEnum.大, 10)
            };
            
            OnLogMessage($"📤 调用PlaceBetAsync:P1大10元");
            var startTime = DateTime.Now;
            
            var (success, orderId, platformResponse) = await _platformScript.PlaceBetAsync(testOrders);
            
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
                OnLogMessage($"💡 提示:错误\"单笔下注范围0~0\"通常表示:");
                OnLogMessage($"   1. 当前没有开盘（未到投注时间）");
                OnLogMessage($"   2. 这个玩法被禁用或限制");
                OnLogMessage($"   3. 需要等待下一期开盘后再投注");
            }
            
            OnLogMessage("🎲 【测试】投注测试完成");
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 投注测试失败:{ex.Message}");
            OnLogMessage($"   堆栈:{ex.StackTrace}");
        }
    }
    
    /// <summary>
    /// 清空日志按钮
    /// </summary>
    private void btnClearLog_Click(object? sender, EventArgs e)
    {
        try
        {
            // 清空日志缓冲区
            lock (_logBuffer)
            {
                _logBuffer.Clear();
            }
            
            // 清空日志文本框
            txtLog.Clear();
            
            // 更新状态
            UpdateLogStatus();
            
            OnLogMessage("🗑️ 日志已清空");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"清空日志失败: {ex.Message}", "错误", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    /// <summary>
    /// 保存日志按钮
    /// </summary>
    private void btnSaveLog_Click(object? sender, EventArgs e)
    {
        try
        {
            // 生成日志文件名
            var fileName = $"BrowserClient_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var saveDialog = new SaveFileDialog
            {
                FileName = fileName,
                Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                Title = "保存日志"
            };
            
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // 保存日志
                System.IO.File.WriteAllText(saveDialog.FileName, txtLog.Text, System.Text.Encoding.UTF8);
                
                OnLogMessage($"💾 日志已保存: {saveDialog.FileName}");
                MessageBox.Show($"日志已成功保存到:\n{saveDialog.FileName}", "保存成功", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 保存日志失败: {ex.Message}");
            MessageBox.Show($"保存日志失败: {ex.Message}", "错误", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    #endregion
}

