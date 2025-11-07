using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BsBrowserClient.Models;
using BsBrowserClient.Services;
using BsBrowserClient.PlatformScripts;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
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
        
        // 设置标题
        this.Text = $"百盛浏览器 - {platform} (配置: {configId}, 端口: {port})";
        
        // 更新状态栏
        lblPort.Text = $"端口: {port}";
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
            _webView.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                txtUrl.Text = _webView.CoreWebView2.Source;
                lblStatus.Text = e.IsSuccess ? "✅ 页面加载完成" : "❌ 页面加载失败";
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
    
    /// <summary>
    /// 初始化 Socket 服务器
    /// </summary>
    private void InitializeSocketServer()
    {
        _socketServer = new SocketServer(_port, OnCommandReceived, OnLogMessage);
        _socketServer.Start();
        
        lblPort.Text = $"端口: {_port} ✅";
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
            
            switch (command.Command.ToLower())
            {
                case "login":
                    var loginData = command.Data as JObject;
                    var username = loginData?["username"]?.ToString() ?? "";
                    var password = loginData?["password"]?.ToString() ?? "";
                    
                    response.Success = await _platformScript!.LoginAsync(username, password);
                    response.Message = response.Success ? "登录成功" : "登录失败";
                    break;
                    
                case "getbalance":
                    var balance = await _platformScript!.GetBalanceAsync();
                    response.Success = balance >= 0;
                    response.Data = new { balance };
                    response.Message = response.Success ? $"余额: {balance}" : "获取余额失败";
                    break;
                    
                case "placebet":
                    var betData = command.Data as JObject;
                    var betOrder = betData?.ToObject<BetOrder>();
                    
                    if (betOrder != null)
                    {
                        var (success, orderId) = await _platformScript!.PlaceBetAsync(betOrder);
                        response.Success = success;
                        response.Data = new { orderId };
                        response.Message = success ? $"投注成功: {orderId}" : "投注失败";
                    }
                    break;
                    
                default:
                    response.Message = $"未知命令: {command.Command}";
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
        
        // 暂时输出到状态栏和控制台
        lblStatus.Text = message;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
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
        _socketServer?.Stop();
        _webView?.Dispose();
    }
    
    #endregion
}

