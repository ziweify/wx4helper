using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BsBrowserClient.Models;
using BsBrowserClient.Services;
using BsBrowserClient.PlatformScripts;
using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json.Linq;

namespace BsBrowserClient;

public partial class Form1 : Form
{
    private readonly string _configId;
    private readonly int _port;
    private readonly string _platform;
    private readonly string _platformUrl;
    
    private SocketServer? _socketServer;
    private IPlatformScript? _platformScript;
    private ChromiumWebBrowser? _chromiumBrowser;
    
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
    
    private void Form1_Load(object sender, EventArgs e)
    {
        try
        {
            // 初始化 CEF
            InitializeCef();
            
            // 初始化平台脚本
            InitializePlatformScript();
            
            // 初始化 Socket 服务器
            InitializeSocketServer();
            
            UpdateStatus("✅ 已就绪", System.Drawing.Color.Green);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"初始化失败: {ex.Message}", "错误", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void InitializeCef()
    {
        // TODO: CEF 初始化需要根据实际 API 调整
        // 目前先创建一个简单的标签页显示URL
        var lblPlaceholder = new Label
        {
            Text = $"CEF 浏览器区域\n平台: {_platform}\nURL: {_platformUrl}\n\n" +
                   "⚠️ 需要配置 CefSharp 才能显示浏览器",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Font = new System.Drawing.Font("Microsoft YaHei", 12F),
            BackColor = System.Drawing.Color.WhiteSmoke
        };
        pnlBrowser.Controls.Add(lblPlaceholder);
        
        // 暂时不初始化 CEF，等解决引用问题后再启用
        /*
        if (!Cef.IsInitialized)
        {
            var settings = new CefSettings();
            Cef.Initialize(settings);
        }
        
        _chromiumBrowser = new ChromiumWebBrowser(_platformUrl)
        {
            Dock = DockStyle.Fill
        };
        pnlBrowser.Controls.Add(_chromiumBrowser);
        */
    }
    
    private void InitializePlatformScript()
    {
        // 创建平台脚本
        _platformScript = _platform.ToLower() switch
        {
            "yunding28" => new YunDing28Script(),
            _ => new YunDing28Script() // 默认
        };
        
        // 设置浏览器
        if (_chromiumBrowser != null)
        {
            _platformScript.SetBrowser(_chromiumBrowser);
        }
    }
    
    private void InitializeSocketServer()
    {
        _socketServer = new SocketServer(_port);
        _socketServer.OnLog += SocketServer_OnLog;
        _socketServer.OnCommandReceived += SocketServer_OnCommandReceived;
        _socketServer.Start();
        
        UpdateStatus($"● Socket 已启动 (端口: {_port})", System.Drawing.Color.Green);
    }
    
    private void SocketServer_OnLog(object? sender, string message)
    {
        // 可以输出到日志窗口（如果有）
        System.Diagnostics.Debug.WriteLine(message);
    }
    
    private async void SocketServer_OnCommandReceived(object? sender, CommandRequest request)
    {
        try
        {
            this.Invoke(() => UpdateStatus($"处理命令: {request.Command}", System.Drawing.Color.Blue));
            
            CommandResponse? response = null;
            
            switch (request.Command.ToLower())
            {
                case "navigate":
                    response = await HandleNavigateAsync(request);
                    break;
                    
                case "login":
                    response = await HandleLoginAsync(request);
                    break;
                    
                case "getbalance":
                    response = await HandleGetBalanceAsync(request);
                    break;
                    
                case "placebet":
                    response = await HandlePlaceBetAsync(request);
                    break;
                    
                case "getstatus":
                    response = HandleGetStatus();
                    break;
                    
                default:
                    response = new CommandResponse
                    {
                        Success = false,
                        ErrorMessage = $"未知命令: {request.Command}"
                    };
                    break;
            }
            
            // TODO: 需要改进 SocketServer 以支持返回响应
            // 目前只是处理了命令但没有返回给客户端
            
            this.Invoke(() => UpdateStatus("✅ 命令已处理", System.Drawing.Color.Green));
        }
        catch (Exception ex)
        {
            this.Invoke(() => UpdateStatus($"❌ 错误: {ex.Message}", System.Drawing.Color.Red));
        }
    }
    
    private async Task<CommandResponse> HandleNavigateAsync(CommandRequest request)
    {
        try
        {
            var data = request.Data as JObject;
            var url = data?["url"]?.ToString() ?? "";
            
            if (_chromiumBrowser != null)
            {
                this.Invoke(() => _chromiumBrowser.Load(url));
                await Task.Delay(1000);
                
                return new CommandResponse
                {
                    Success = true,
                    Data = new { Url = url }
                };
            }
            
            return new CommandResponse { Success = false, ErrorMessage = "浏览器未初始化" };
        }
        catch (Exception ex)
        {
            return new CommandResponse { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    private async Task<CommandResponse> HandleLoginAsync(CommandRequest request)
    {
        try
        {
            var data = request.Data as JObject;
            var username = data?["username"]?.ToString() ?? "";
            var password = data?["password"]?.ToString() ?? "";
            
            if (_platformScript != null)
            {
                var success = await _platformScript.LoginAsync(username, password);
                
                if (success)
                {
                    this.Invoke(() => UpdateStatus("✅ 已登录", System.Drawing.Color.Green));
                }
                
                return new CommandResponse
                {
                    Success = success,
                    Data = new { Username = username }
                };
            }
            
            return new CommandResponse { Success = false, ErrorMessage = "平台脚本未初始化" };
        }
        catch (Exception ex)
        {
            return new CommandResponse { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    private async Task<CommandResponse> HandleGetBalanceAsync(CommandRequest request)
    {
        try
        {
            if (_platformScript != null)
            {
                var balance = await _platformScript.GetBalanceAsync();
                
                this.Invoke(() => 
                {
                    lblBalance.Text = $"余额: ¥{balance:F2}";
                });
                
                return new CommandResponse
                {
                    Success = true,
                    Data = new { Balance = balance }
                };
            }
            
            return new CommandResponse { Success = false, ErrorMessage = "平台脚本未初始化" };
        }
        catch (Exception ex)
        {
            return new CommandResponse { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    private async Task<CommandResponse> HandlePlaceBetAsync(CommandRequest request)
    {
        try
        {
            var data = request.Data as JObject;
            var order = data?.ToObject<BetOrder>();
            
            if (order == null)
            {
                return new CommandResponse { Success = false, ErrorMessage = "订单数据无效" };
            }
            
            if (_platformScript != null)
            {
                var response = await _platformScript.PlaceBetAsync(order);
                return response;
            }
            
            return new CommandResponse { Success = false, ErrorMessage = "平台脚本未初始化" };
        }
        catch (Exception ex)
        {
            return new CommandResponse { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    private CommandResponse HandleGetStatus()
    {
        return new CommandResponse
        {
            Success = true,
            Data = new
            {
                ConfigId = _configId,
                Platform = _platform,
                Port = _port,
                IsReady = _chromiumBrowser != null && _platformScript != null
            }
        };
    }
    
    private void btnNavigate_Click(object sender, EventArgs e)
    {
        var url = txtUrl.Text.Trim();
        if (!string.IsNullOrEmpty(url) && _chromiumBrowser != null)
        {
            _chromiumBrowser.Load(url);
        }
    }
    
    private void btnRefresh_Click(object sender, EventArgs e)
    {
        // TODO: CEF 刷新功能，等 CEF 启用后实现
        // _chromiumBrowser?.GetBrowser()?.Reload();
        MessageBox.Show("刷新功能待实现", "提示");
    }
    
    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        // 清理资源
        _socketServer?.Stop();
        _chromiumBrowser?.Dispose();
        
        // 注意：不要在这里关闭 Cef，因为可能有其他实例
        // Cef.Shutdown();
    }
    
    private void UpdateStatus(string text, System.Drawing.Color color)
    {
        lblStatus.Text = text;
        lblStatus.ForeColor = color;
    }
    
    private string GetDefaultUrl(string platform)
    {
        return platform.ToLower() switch
        {
            "yunding28" => "https://www.yunding28.com",
            "haixia28" => "https://www.haixia28.com",
            _ => "about:blank"
        };
    }
}
