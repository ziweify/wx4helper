using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommandRequest = BsBrowserClient.Models.CommandRequest;  // 🔥 使用别名避免类型冲突
using CommandResponse = BsBrowserClient.Models.CommandResponse;  // 🔥 命令响应
using BsBrowserClient.Services;
using BsBrowserClient.PlatformScripts;
using BsBrowserClient.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unit.Shared.Platform;
using Unit.Shared.Models;  // 🔥 使用共享的模型

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
        
        // 🔥 初始化日志文件夹路径
        _logFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BaiShengVx3Plus",
            "log"
        );
        Directory.CreateDirectory(_logFolder);
        
        // 🔥 初始化日志文件名（日期+时间，区分每次启动）
        var now = DateTime.Now;
        _currentLogFile = Path.Combine(
            _logFolder,
            $"BsBrowserClient_{now:yyyyMMdd_HHmmss}.log"
        );

        // 更新状态栏
        lblPort.Text = $"配置: {configName} (ID:{configId}) | 平台: {platform}";
        txtUrl.Text = _platformUrl;
    }
    
    private async void Form1_Load(object sender, EventArgs e)
    {
        try
        {
            // 🔥 默认隐藏日志面板（在 Load 事件中设置，确保控件已初始化）
            if (splitContainer != null)
            {
                splitContainer.Panel2Collapsed = true;  // 默认隐藏日志面板
            }
            
            // 🔥 初始化日志切换标签
            InitializeLogToggle();
            
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
            BetPlatform.通宝PC => new TongBaoPcScript(_webView!, betLogCallback),
            BetPlatform.海峡 => new HaiXiaScript(_webView!, betLogCallback),
            BetPlatform.红海 => new HongHaiScript(_webView!, betLogCallback),
            BetPlatform.红海无名 => new HongHaiWuMingScript(_webView!, betLogCallback),
            BetPlatform.茅台 => new Mt168Script(_webView!, betLogCallback),
            BetPlatform.太平洋 => new Mt168Script(_webView!, betLogCallback), // 使用茅台脚本
            BetPlatform.QT => new QtScript(_webView!, betLogCallback),
            BetPlatform.蓝B => new QtScript(_webView!, betLogCallback), // 使用QT脚本
            BetPlatform.S880 => new S880Script(_webView!, betLogCallback),
            BetPlatform.ADK => new ADKScript(_webView!, betLogCallback),
            BetPlatform.果然 => new Kk888Script(_webView!, betLogCallback),
            BetPlatform.AC => new AcScript(_webView!, betLogCallback),
            BetPlatform.HY168 => new Hy168bingoScript(_webView!, betLogCallback),
            BetPlatform.bingo168 => new Hy168bingoScript(_webView!, betLogCallback),
            BetPlatform.蓝A => new LanAScript(_webView!, betLogCallback),
            BetPlatform.元宇宙2 => new YYZ2Script(_webView!, betLogCallback),
            BetPlatform.不使用盘口 => new NoneSiteScript(_webView!, betLogCallback),
            _ => new YunDing28Script(_webView!, betLogCallback) // 默认使用云顶脚本
        };
    }

    private bool _isAutoLoginTriggered = false;

    // 🔥 从 VxMain 的 Login 命令中保存的账号密码（避免重复通过 HTTP API 获取）
    private string? _username;
    private string? _password;

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

            // 🔥 使用从 Login 命令中保存的账号密码（避免冗余的 HTTP API 调用）
            // VxMain 会在启动时主动发送 Login 命令，包含账号密码
            var username = _username;
            var password = _password;

            // 如果没有账号密码，说明 VxMain 还没发送 Login 命令
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                OnLogMessage("⚠️ 未收到登录凭据，等待 VxMain 发送 Login 命令...");
                OnLogMessage("ℹ️ 当前页面没有Cookie");
                return;
            }

            OnLogMessage($"✅ 使用已保存的登录凭据:");
            OnLogMessage($"   用户名: {username}");
            OnLogMessage($"   密码: ******");

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
                case "Login":  // 🔥 兼容英文命令名
                    var loginData = command.Data as JObject;
                    var username = loginData?["username"]?.ToString() ?? "";
                    var password = loginData?["password"]?.ToString() ?? "";

                    // 🔥 保存账号密码到成员变量（供自动登录使用，避免重复通过 HTTP API 获取）
                    _username = username;
                    _password = password;
                    OnLogMessage($"💾 已保存登录凭据: 用户名={username}");

                    // 🔥 WebView2 操作必须在 UI 线程执行
                    if (InvokeRequired)
                    {
                        var loginResult = await Task.Run(async () =>
                        {
                            var tcs = new TaskCompletionSource<bool>();
                            Invoke(async () =>
                            {
                                try
                                {
                                    var result = await _platformScript!.LoginAsync(username, password);
                                    tcs.SetResult(result);
                                }
                                catch (Exception ex)
                                {
                                    OnLogMessage($"❌ 登录失败: {ex.Message}");
                                    tcs.SetResult(false);
                                }
                            });
                            return await tcs.Task;
                        });
                        response.Success = loginResult;
                    }
                    else
                    {
                        response.Success = await _platformScript!.LoginAsync(username, password);
                    }
                    response.Message = response.Success ? "登录成功" : "登录失败";
                    break;

                case "获取余额":
                    // 🔥 WebView2 操作必须在 UI 线程执行
                    decimal balance = -1;
                    if (InvokeRequired)
                    {
                        balance = await Task.Run(async () =>
                        {
                            var tcs = new TaskCompletionSource<decimal>();
                            Invoke(async () =>
                            {
                                try
                                {
                                    var result = await _platformScript!.GetBalanceAsync();
                                    tcs.SetResult(result);
                                }
                                catch (Exception ex)
                                {
                                    OnLogMessage($"❌ 获取余额失败: {ex.Message}");
                                    tcs.SetResult(-1);
                                }
                            });
                            return await tcs.Task;
                        });
                    }
                    else
                    {
                        balance = await _platformScript!.GetBalanceAsync();
                    }
                    response.Success = balance >= 0;
                    response.Data = new { balance };
                    response.Message = response.Success ? $"余额: {balance}" : "获取余额失败";
                    break;

                case "获取Cookie":
                    // 获取Cookie命令
                    // 🔥 WebView2 操作必须在 UI 线程执行
                    try
                    {
                        if (_webView?.CoreWebView2 == null)
                        {
                            response.Message = "WebView2未初始化";
                            break;
                        }

                        if (InvokeRequired)
                        {
                            var cookieResult = await Task.Run(async () =>
                            {
                                var tcs = new TaskCompletionSource<(bool success, object? data, string message)>();
                                Invoke(async () =>
                                {
                                    try
                                    {
                                        var allCookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(_webView.CoreWebView2.Source);
                                        var cookieDict = new Dictionary<string, string>();

                                        foreach (var cookie in allCookies)
                                        {
                                            cookieDict[cookie.Name] = cookie.Value;
                                        }

                                        var data = new
                                        {
                                            url = _webView.CoreWebView2.Source,
                                            cookies = cookieDict,
                                            count = allCookies.Count
                                        };
                                        tcs.SetResult((true, data, $"获取成功,共{allCookies.Count}个Cookie"));
                                    }
                                    catch (Exception ex)
                                    {
                                        OnLogMessage($"❌ 获取Cookie失败: {ex.Message}");
                                        tcs.SetResult((false, null, "获取Cookie失败"));
                                    }
                                });
                                return await tcs.Task;
                            });
                            response.Success = cookieResult.success;
                            response.Data = cookieResult.data;
                            response.Message = cookieResult.message;
                        }
                        else
                        {
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
                        }

                        if (response.Success)
                        {
                            var count = (response.Data as dynamic)?.count ?? 0;
                            OnLogMessage($"📤 获取Cookie完成:共{count}个");
                        }
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
                    // 🔥 WebView2 操作必须在 UI 线程执行
                    try
                    {
                        decimal quotaBalance = -1;
                        if (InvokeRequired)
                        {
                            quotaBalance = await Task.Run(async () =>
                            {
                                var tcs = new TaskCompletionSource<decimal>();
                                Invoke(async () =>
                                {
                                    try
                                    {
                                        var result = await _platformScript!.GetBalanceAsync();
                                        tcs.SetResult(result);
                                    }
                                    catch (Exception ex)
                                    {
                                        OnLogMessage($"❌ 获取额度失败: {ex.Message}");
                                        tcs.SetResult(-1);
                                    }
                                });
                                return await tcs.Task;
                            });
                        }
                        else
                        {
                            quotaBalance = await _platformScript!.GetBalanceAsync();
                        }

                        response.Success = quotaBalance >= 0;
                        response.Data = new { balance = quotaBalance, quota = quotaBalance };
                        response.Message = response.Success ? $"盘口额度: {quotaBalance}元" : "获取额度失败";

                        if (response.Success)
                        {
                            OnLogMessage($"📊 盘口额度:{quotaBalance}元");
                        }
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
                    Unit.Shared.Models.BetStandardOrderList? betOrders = null;

                    // 🔥 BetStandardOrderList 序列化后可能是数组（JArray）或对象（JObject）
                    if (command.Data is Newtonsoft.Json.Linq.JArray jArray)
                    {
                        // 如果是数组，直接反序列化
                        betOrders = jArray.ToObject<Unit.Shared.Models.BetStandardOrderList>();
                    }
                    else if (command.Data is JObject betData)
                    {
                        // 如果是对象，尝试反序列化
                        betOrders = betData.ToObject<Unit.Shared.Models.BetStandardOrderList>();
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
                        // 🔥 WebView2 操作必须在 UI 线程执行
                        bool success;
                        string orderId;
                        string platformResponse;

                        if (InvokeRequired)
                        {
                            var result = await Task.Run(async () =>
                            {
                                var tcs = new TaskCompletionSource<(bool, string, string)>();
                                Invoke(async () =>
                                {
                                    try
                                    {
                                        var betResult = await _platformScript!.PlaceBetAsync(betOrders);
                                        tcs.SetResult(betResult);
                                    }
                                    catch (Exception ex)
                                    {
                                        OnLogMessage($"❌ 投注失败: {ex.Message}", LogType.Bet);
                                        tcs.SetResult((false, "", $"#投注异常: {ex.Message}"));
                                    }
                                });
                                return await tcs.Task;
                            });
                            success = result.Item1;
                            orderId = result.Item2;
                            platformResponse = result.Item3;
                        }
                        else
                        {
                            var result = await _platformScript!.PlaceBetAsync(betOrders);
                            success = result.success;
                            orderId = result.orderId;
                            platformResponse = result.platformResponse;
                        }

                        // 记录POST后时间
                        var postEndTime = DateTime.Now;
                        var durationMs = (int)(postEndTime - postStartTime).TotalMilliseconds;

                        // 🔥 解析 platformResponse（避免双重序列化导致转义）
                        object? platformResponseObj = null;
                        try
                        {
                            // 如果以#开头，说明是客户端错误（字符串）
                            if (!string.IsNullOrEmpty(platformResponse) && platformResponse.StartsWith("#"))
                            {
                                platformResponseObj = platformResponse;  // 保持字符串
                            }
                            // 否则尝试解析为JSON对象
                            else if (!string.IsNullOrEmpty(platformResponse))
                            {
                                platformResponseObj = Newtonsoft.Json.Linq.JToken.Parse(platformResponse);  // 解析为对象
                            }
                        }
                        catch
                        {
                            // 解析失败，保持原字符串
                            platformResponseObj = platformResponse;
                        }

                        response.Success = success;
                        response.Message = success ? "投注成功" : "投注失败";
                        response.Data = new
                        {
                            postStartTime = postStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            postEndTime = postEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            durationMs = durationMs,
                            orderNo = orderId,
                            platformResponse = platformResponseObj  // 🔥 直接放入对象，而不是字符串
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
    
    // 🔥 磁盘日志写入（新增）
    private readonly ConcurrentQueue<string> _diskWriteQueue = new ConcurrentQueue<string>();
    private readonly SemaphoreSlim _diskWriteSemaphore = new SemaphoreSlim(0);
    private CancellationTokenSource? _diskWriteCts;
    private Task? _diskWriteTask;
    private readonly string _logFolder;
    private readonly string _currentLogFile;  // 🔥 改为 readonly，启动时确定文件名
    private System.Threading.Timer? _diskFlushTimer;

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
        
        // 🔥 启动磁盘写入系统
        InitializeDiskWriter();
        
        // 🔥 清理旧日志（启动时清理一次）
        Task.Run(() => CleanOldLogs());
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

        // 🔥 关键修复：将取出的日志也加入磁盘写入队列（避免日志丢失）
        foreach (var log in logs)
        {
            EnqueueDiskWrite(log);
        }

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
    /// 初始化日志切换标签
    /// </summary>
    private void InitializeLogToggle()
    {
        if (lblLogToggle != null)
        {
            // 标签已在 Designer 中设置，这里只需要更新文本
            UpdateLogToggleText();
        }
    }
    
    /// <summary>
    /// 更新日志切换标签文本
    /// </summary>
    private void UpdateLogToggleText()
    {
        if (lblLogToggle != null && splitContainer != null)
        {
            if (splitContainer.Panel2Collapsed)
            {
                lblLogToggle.Text = "📋 当前日志";
            }
            else
            {
                lblLogToggle.Text = "📋 隐藏日志";
            }
        }
    }
    
    /// <summary>
    /// 日志切换标签点击事件 - 切换日志面板显示/隐藏
    /// </summary>
    private void LblLogToggle_Click(object? sender, EventArgs e)
    {
        ToggleLogPanel();
    }
    
    /// <summary>
    /// 切换日志面板显示/隐藏
    /// </summary>
    private void ToggleLogPanel()
    {
        if (splitContainer == null) return;
        
        try
        {
            if (splitContainer.Panel2Collapsed)
            {
                // 如果已隐藏，则显示
                splitContainer.Panel2Collapsed = false;
                // 设置分割器位置（日志面板占30%高度）
                if (splitContainer.Orientation == Orientation.Horizontal)
                {
                    splitContainer.SplitterDistance = (int)(splitContainer.Height * 0.7);
                }
                OnLogMessage("📋 日志面板已显示", LogType.System);
            }
            else
            {
                // 如果已显示，则隐藏
                splitContainer.Panel2Collapsed = true;
                OnLogMessage("📋 日志面板已隐藏", LogType.System);
            }
            
            UpdateLogToggleText();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"切换日志面板失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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

            // 🔥 如果缓冲区过大，移除旧日志（防止内存溢出）
            // LogTimer_Tick 会将所有日志自动写入磁盘，这里只是额外的保护
            while (_logBuffer.Count > MAX_LOG_LINES * 2)
            {
                _logBuffer.Dequeue();
            }
        }

        // 输出到控制台（用于调试）
        Console.WriteLine($"[{time}] [{type}] {message}");
    }

    #region 磁盘日志写入系统

    /// <summary>
    /// 初始化磁盘写入系统
    /// </summary>
    private void InitializeDiskWriter()
    {
        try
        {
            // 启动异步写入线程
            _diskWriteCts = new CancellationTokenSource();
            _diskWriteTask = Task.Run(() => DiskWriteWorker(), _diskWriteCts.Token);
            
            // 启动定时刷新（每 5 秒批量写入一次，防止缓冲区不满导致日志不写入）
            _diskFlushTimer = new System.Threading.Timer(
                callback: _ => FlushDiskWriteQueue(),
                state: null,
                dueTime: TimeSpan.FromSeconds(5),
                period: TimeSpan.FromSeconds(5)
            );
            
            OnLogMessage($"💾 日志持久化已启动: {Path.GetFileName(_currentLogFile)}", LogType.System);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"初始化磁盘写入失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 将日志加入磁盘写入队列
    /// </summary>
    private void EnqueueDiskWrite(string logLine)
    {
        _diskWriteQueue.Enqueue(logLine);
        _diskWriteSemaphore.Release();  // 通知后台线程
    }

    /// <summary>
    /// 磁盘写入工作线程
    /// </summary>
    private async Task DiskWriteWorker()
    {
        var batchBuffer = new List<string>(100);
        
        while (!_diskWriteCts?.Token.IsCancellationRequested ?? false)
        {
            try
            {
                // 等待新日志或超时（最多等待 1 秒）
                await _diskWriteSemaphore.WaitAsync(1000, _diskWriteCts.Token);
                
                // 批量收集日志（最多 100 条）
                batchBuffer.Clear();
                while (batchBuffer.Count < 100 && _diskWriteQueue.TryDequeue(out var log))
                {
                    batchBuffer.Add(log);
                }
                
                if (batchBuffer.Count == 0) continue;
                
                // 批量写入磁盘（异步 I/O）
                await WriteBatchToDiskAsync(batchBuffer);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"磁盘写入失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 批量写入日志到磁盘
    /// </summary>
    private async Task WriteBatchToDiskAsync(List<string> logs)
    {
        try
        {
            var logFile = GetLogFilePath();
            
            // 使用 StreamWriter 批量写入（高性能）
            using var writer = new StreamWriter(logFile, append: true, Encoding.UTF8, bufferSize: 65536);
            foreach (var log in logs)
            {
                await writer.WriteAsync(log);
            }
            await writer.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"写入日志文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取日志文件路径（启动时确定，不变）
    /// </summary>
    private string GetLogFilePath()
    {
        return _currentLogFile;
    }

    /// <summary>
    /// 强制刷新磁盘写入队列（定时触发点 2）
    /// </summary>
    private void FlushDiskWriteQueue()
    {
        // 如果队列有数据，通知写入线程
        if (_diskWriteQueue.Count > 0)
        {
            _diskWriteSemaphore.Release();
        }
    }

    /// <summary>
    /// 刷新所有日志到磁盘（程序退出时调用）
    /// </summary>
    private void FlushAllLogs()
    {
        try
        {
            var logFile = GetLogFilePath();
            var totalLogs = 0;
            
            // 1. 将内存缓冲区的日志也写入磁盘
            var bufferLogs = new List<string>();
            lock (_logBuffer)
            {
                while (_logBuffer.Count > 0)
                {
                    bufferLogs.Add(_logBuffer.Dequeue());
                }
            }
            
            Console.WriteLine($"📋 从 _logBuffer 取出 {bufferLogs.Count} 条日志");
            
            // 2. 从磁盘写入队列取出所有日志
            var queueLogs = new List<string>();
            while (_diskWriteQueue.TryDequeue(out var log))
            {
                queueLogs.Add(log);
            }
            
            Console.WriteLine($"📋 从 _diskWriteQueue 取出 {queueLogs.Count} 条日志");
            
            // 3. 合并所有日志并同步写入
            var allLogs = new List<string>();
            allLogs.AddRange(queueLogs);  // 先写入队列中的日志
            allLogs.AddRange(bufferLogs);  // 再写入缓冲区的日志
            
            if (allLogs.Count > 0)
            {
                using var writer = new StreamWriter(logFile, append: true, Encoding.UTF8, bufferSize: 65536);
                foreach (var log in allLogs)
                {
                    writer.Write(log);
                }
                writer.Flush();
                totalLogs = allLogs.Count;
            }
            
            Console.WriteLine($"💾 已刷新 {totalLogs} 条日志到磁盘: {Path.GetFileName(logFile)}");
            
            // 4. 输出文件完整路径（方便用户查找）
            Console.WriteLine($"📁 日志文件路径: {logFile}");
            Console.WriteLine($"📏 日志文件大小: {new FileInfo(logFile).Length / 1024.0:F2} KB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 刷新日志失败: {ex.Message}");
            Console.WriteLine($"   堆栈: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// 清理 7 天前的旧日志文件
    /// </summary>
    private void CleanOldLogs()
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-7);
            var logFiles = Directory.GetFiles(_logFolder, "BsBrowserClient_*.log");
            
            int cleanedCount = 0;
            long cleanedSize = 0;
            
            foreach (var file in logFiles)
            {
                var fileInfo = new FileInfo(file);
                
                // 跳过当前日志文件
                if (file == _currentLogFile)
                    continue;
                
                // 根据文件的最后修改时间判断是否需要清理
                if (fileInfo.LastWriteTime < cutoffDate)
                {
                    cleanedSize += fileInfo.Length;
                    File.Delete(file);
                    cleanedCount++;
                    Console.WriteLine($"已删除旧日志: {fileInfo.Name} ({fileInfo.Length / 1024.0:F2} KB)");
                }
            }
            
            if (cleanedCount > 0)
            {
                Console.WriteLine($"✅ 已清理 {cleanedCount} 个旧日志文件，释放 {cleanedSize / 1024.0 / 1024.0:F2} MB 空间");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清理旧日志失败: {ex.Message}");
        }
    }

    #endregion

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
                    Console.WriteLine($"════════════════════════════════════════");
                    Console.WriteLine($"开始关闭流程，准备刷新日志...");
                    
                    // 🔥 停止磁盘写入系统（用户建议的触发点 3）
                    _diskFlushTimer?.Dispose();
                    _diskWriteCts?.Cancel();
                    
                    // 🔥 刷新所有日志到磁盘（包括内存缓冲区和磁盘队列的所有日志）
                    FlushAllLogs();
                    
                    // 🔥 等待异步写入线程退出（最多 2 秒）
                    try
                    {
                        _diskWriteTask?.Wait(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ 等待异步线程退出失败: {ex.Message}");
                    }
                    
                    Console.WriteLine($"════════════════════════════════════════");
                    
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
            var testOrders = new Unit.Shared.Models.BetStandardOrderList
            {
                new Unit.Shared.Models.BetStandardOrder(0, Unit.Shared.Models.CarNumEnum.P1, Unit.Shared.Models.BetPlayEnum.大, 10)
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

    /// <summary>
    /// 点击赔率信息链接
    /// </summary>
    private void LblOddsInfo_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_platformScript == null)
            {
                MessageBox.Show("平台脚本未初始化", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 获取赔率列表
            var oddsList = _platformScript.GetOddsList();

            if (oddsList.Count == 0)
            {
                MessageBox.Show("赔率数据尚未加载，请先登录并等待赔率更新", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 创建并显示赔率窗口
            var oddsForm = new OddsDisplayForm();
            oddsForm.SetOddsData(oddsList);
            oddsForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 打开赔率窗口失败: {ex.Message}");
            MessageBox.Show($"打开赔率窗口失败: {ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    /// <summary>
    /// 获取未结算订单列表
    /// </summary>
    private async void btnGetLotMainOrderInfos_Click(object sender, EventArgs e)
    {
        try
        {
            OnLogMessage("📋 【测试】开始获取未结算订单列表...");

            if (_platformScript == null)
            {
                OnLogMessage("❌ 平台脚本未初始化");
                return;
            }

            // 🔥 调用平台脚本的获取订单方法（默认获取今天的未结算订单）
            OnLogMessage("📤 调用 GetLotMainOrderInfosAsync (state=0, pageNum=1, pageCount=20, timeout=15秒)");
            var startTime = DateTime.Now;

            var (success, orders, maxRecordNum, maxPageNum, errorMsg) = 
                await _platformScript.GetLotMainOrderInfosAsync(
                    state: 0,           // 未结算
                    pageNum: 1,         // 第1页
                    pageCount: 20,      // 每页20条
                    timeout: 15         // 🔥 超时时间15秒（可根据需要调整）
                    // beginDate 和 endDate 默认为今天
                );

            var elapsed = DateTime.Now - startTime;
            OnLogMessage($"⏱️ 请求耗时: {elapsed.TotalSeconds:F2}秒");

            if (success)
            {
                OnLogMessage($"✅ 获取订单成功:");
                OnLogMessage($"   📊 本页: {orders?.Count ?? 0}条");
                OnLogMessage($"   📊 总计: {maxRecordNum}条记录，共{maxPageNum}页");
                
                if (orders != null && orders.Count > 0)
                {
                    OnLogMessage($"\n📄 订单列表（前 {Math.Min(10, orders.Count)} 条）:");
                    
                    for (int i = 0; i < Math.Min(10, orders.Count); i++)
                    {
                        var order = orders[i];
                        
                        // 提取订单信息
                        var orderId = order["orderid"]?.ToString() ?? "N/A";
                        var expect = order["expect"]?.ToString() ?? "N/A";
                        var amount = order["amount"]?.Value<decimal>() ?? 0;
                        var userData = order["userdata"]?.ToString()?.Trim() ?? "";
                        var createTime = order["createtime"]?.ToString() ?? "";
                        var orderState = order["state"]?.Value<int>() ?? -1;
                        var subCount = order["subcount"]?.Value<int>() ?? 0;
                        
                        OnLogMessage($"   [{i + 1}] 订单ID: {orderId}");
                        OnLogMessage($"       期号: {expect} | 金额: {amount}元 | 子单数: {subCount}");
                        OnLogMessage($"       内容: {userData}");
                        OnLogMessage($"       时间: {createTime} | 状态: {(orderState == 0 ? "未结算" : "已结算")}");
                        
                        if (i < orders.Count - 1)
                        {
                            OnLogMessage("");  // 空行分隔
                        }
                    }
                    
                    if (orders.Count > 10)
                    {
                        OnLogMessage($"\n   ... 还有 {orders.Count - 10} 条订单未显示");
                    }
                }
                else
                {
                    OnLogMessage("   ℹ️ 暂无未结算订单");
                }
            }
            else
            {
                OnLogMessage($"❌ 获取订单失败: {errorMsg}");
            }
        }
        catch (Exception ex)
        {
            OnLogMessage($"❌ 获取订单异常: {ex.Message}");
            OnLogMessage($"   堆栈: {ex.StackTrace}");
        }
    }
}

