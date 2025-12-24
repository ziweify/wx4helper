using System;
using System.ComponentModel;
using System.Threading.Tasks;
using SQLite;

namespace zhaocaimao.Models.AutoBet
{
    /// <summary>
    /// 自动投注配置
    /// </summary>
    [Table("AutoBetConfigs")]
    public partial class BetConfig : INotifyPropertyChanged, IDisposable
    {
    // ========================================
    // 🔥 持久化字段（需要保存到数据库）
    // ========================================
    private string _configName = "";
    private string _platform = "YunDing28";
    private string _platformUrl = "";
    private string _username = "";
    private string _password = "";
    private bool _isEnabled = true;
    private bool _showBrowserWindow = false;
    private bool _autoLogin = true;
    private string? _notes;
    private bool _isDefault = false;
    private string? _cookies;
    private DateTime? _cookieUpdateTime;
    
    // ========================================
    // 🔥 持久化属性（带 PropertyChanged 通知）
    // ========================================
    
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string ConfigName
    {
        get => _configName;
        set
        {
            if (_configName != value)
            {
                _configName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConfigName)));
            }
        }
    }
    
    /// <summary>
    /// 🔥 平台名称（如：通宝、云顶28等）
    /// </summary>
    public string Platform
    {
        get => _platform;
        set
        {
            if (_platform != value)
            {
                _platform = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Platform)));
            }
        }
    }
    
    /// <summary>
    /// 🔥 盘口URL（平台访问地址）
    /// </summary>
    public string PlatformUrl
    {
        get => _platformUrl;
        set
        {
            if (_platformUrl != value)
            {
                _platformUrl = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlatformUrl)));
            }
        }
    }
    
    /// <summary>
    /// 🔥 账号
    /// </summary>
    public string Username
    {
        get => _username;
        set
        {
            if (_username != value)
            {
                _username = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Username)));
            }
        }
    }
    
    /// <summary>
    /// 🔥 密码
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
            }
        }
    }
    
    /// <summary>
    /// 🔥 是否启用自动投注
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                var oldValue = _isEnabled;
                _isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
                
                // 🔥 配置自管理模式：当 IsEnabled 变为 true 时，启动监控线程
                if (value && !oldValue)
                {
                    // 从 false 变为 true，启动监控线程
                    StartMonitoring();
                    
                    // 立即检查是否需要启动浏览器
                    if (ShouldStartBrowser())
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                _isStartingBrowser = true;
                                await StartBrowserInternalAsync();
                            }
                            catch (Exception ex)
                            {
                                // 🔥 增强异常日志记录
                                _logService?.Error("BetConfig", $"❌ [{ConfigName}] 监控任务启动浏览器时异常", ex);
                                System.Diagnostics.Debug.WriteLine($"[BetConfig] 启动浏览器时异常: {ex.Message}\n{ex.StackTrace}");
                            }
                            finally
                            {
                                _isStartingBrowser = false;
                            }
                        });
                    }
                }
                else if (!value && oldValue)
                {
                    // 从 true 变为 false，停止监控线程
                    StopMonitoring();
                }
            }
        }
    }
    
    /// <summary>
    /// 🔥 是否显示浏览器窗口
    /// </summary>
    public bool ShowBrowserWindow
    {
        get => _showBrowserWindow;
        set
        {
            if (_showBrowserWindow != value)
            {
                _showBrowserWindow = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowBrowserWindow)));
            }
        }
    }
    
    /// <summary>
    /// 🔥 是否自动登录
    /// </summary>
    public bool AutoLogin
    {
        get => _autoLogin;
        set
        {
            if (_autoLogin != value)
            {
                _autoLogin = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoLogin)));
            }
        }
    }
    
    /// <summary>
    /// 🔥 备注信息
    /// </summary>
    public string? Notes
    {
        get => _notes;
        set
        {
            if (_notes != value)
            {
                _notes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
            }
        }
    }
    
    /// <summary>
    /// 🔥 是否为默认配置
    /// </summary>
    public bool IsDefault
    {
        get => _isDefault;
        set
        {
            if (_isDefault != value)
            {
                _isDefault = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDefault)));
            }
        }
    }
    
    /// <summary>
    /// 🔥 Cookie信息（字符串格式，如：key1=value1; key2=value2）
    /// </summary>
    public string? Cookies
    {
        get => _cookies;
        set
        {
            if (_cookies != value)
            {
                _cookies = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cookies)));
            }
        }
    }
    
    /// <summary>
    /// 🔥 Cookie 更新时间
    /// </summary>
    public DateTime? CookieUpdateTime
    {
        get => _cookieUpdateTime;
        set
        {
            if (_cookieUpdateTime != value)
            {
                _cookieUpdateTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CookieUpdateTime)));
            }
        }
    }
    
    // ========================================
    // ❌ 运行时数据（不需要 PropertyChanged，不需要持久化）
    // ========================================
    
    /// <summary>
    /// 当前余额（运行时从平台获取，不持久化）
    /// </summary>
    public decimal CurrentBalance { get; set; }
    
    /// <summary>
    /// 状态（运行时状态，如"已连接"、"未连接"，不持久化）
    /// 🔥 添加 PropertyChanged 通知，确保 UI 能自动刷新
    /// </summary>
    private string _status = "未启动";
    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }
    }
    
    /// <summary>
    /// 最后更新时间（自动更新，不需要通知）
    /// </summary>
    public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 浏览器进程ID（运行时数据，不持久化）
    /// </summary>
    public int ProcessId { get; set; } = 0;
        
        /// <summary>
        /// 🔥 浏览器客户端（运行时对象，不保存到数据库）
        /// 配置对象自己管理与浏览器的连接！
        /// </summary>
        [Ignore]
        public Services.AutoBet.BrowserClient? Browser { get; set; }
        
        /// <summary>
        /// 🔥 是否已连接到浏览器
        /// </summary>
        [Ignore]
        public bool IsConnected => Browser?.IsConnected ?? false;
        
    /// <summary>
    /// 显示浏览器窗口（兼容属性）
    /// </summary>
    [Ignore]
    public bool ShowBrowser
    {
        get => ShowBrowserWindow;
        set => ShowBrowserWindow = value;
    }
    
    // ========================================
    // INotifyPropertyChanged 实现
    // ========================================
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    // ========================================
    // IDisposable 实现
    // ========================================
    
    private bool _disposed = false;
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        DisposeBrowserManagement();  // 调用部分类中的清理方法
        GC.SuppressFinalize(this);
    }
    
    // 部分方法：由 BetConfig.BrowserManagement.cs 实现
    partial void DisposeBrowserManagement();
    }
}

