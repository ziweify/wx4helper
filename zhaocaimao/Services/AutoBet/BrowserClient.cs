using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using zhaocaimao.Models.AutoBet;
using zhaocaimao.UserControls;

namespace zhaocaimao.Services.AutoBet
{
    /// <summary>
    /// 浏览器客户端包装器 - 用于兼容旧代码
    /// 内部使用 BetBrowserControl
    /// </summary>
    public class BrowserClientWrapper : BrowserClient
    {
        private readonly BetBrowserControl _control;
        
        public BrowserClientWrapper(int configId, BetBrowserControl control) : base(configId)
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));
        }
        
        public override bool IsConnected => _control.IsInitialized;
        
        public override BetBrowserControl? GetBrowserControl()
        {
            return _control;
        }
        
        public override async Task<BetResult> SendCommandAsync(string command, object? data = null)
        {
            if (!_control.IsInitialized)
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "浏览器未初始化"
                };
            }
            
            return await _control.ExecuteCommandAsync(command, data);
        }
        
        public override async Task<(bool IsAlive, int ProcessId)> PingAsync()
        {
            if (!_control.IsInitialized)
            {
                return (false, 0);
            }
            
            var result = await _control.ExecuteCommandAsync("心跳检测");
            return (result.Success, 0);
        }
        
        public override void Dispose(bool killProcess = false)
        {
            // 包装器不负责释放控件，控件由 BetConfig 管理
        }
    }
    
    /// <summary>
    /// 浏览器客户端 - 使用内部控件，不启动进程
    /// </summary>
    public class BrowserClient : IDisposable
    {
        private readonly int _configId;
        private BetBrowserControl? _browserControl;  // 🔥 使用内部控件
        private readonly object _controlLock = new object();
        
        public virtual bool IsConnected
        {
            get
            {
                lock (_controlLock)
                {
                    return _browserControl != null && _browserControl.IsInitialized;
                }
            }
        }
        
        /// <summary>
        /// 获取浏览器控件（用于嵌入到窗体）
        /// </summary>
        public virtual BetBrowserControl? GetBrowserControl()
        {
            lock (_controlLock)
            {
                return _browserControl;
            }
        }
        
        public BrowserClient(int configId)
        {
            _configId = configId;
        }
        
        /// <summary>
        /// 启动浏览器控件（不启动进程）
        /// </summary>
        public async Task<bool> StartAsync(int port, string configName, string platform, string platformUrl)
        {
            try
            {
                lock (_controlLock)
                {
                    if (_browserControl != null)
                    {
                        // 已存在，直接返回
                        return _browserControl.IsInitialized;
                    }
                    
                    // 创建浏览器控件
                    _browserControl = new BetBrowserControl();
                }
                
                // 初始化浏览器控件
                await _browserControl.InitializeAsync(_configId, configName, platform, platformUrl);
                
                return true;
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }
        
        /// <summary>
        /// 发送命令并等待响应（直接调用控件方法）
        /// </summary>
        public virtual async Task<BetResult> SendCommandAsync(string command, object? data = null)
        {
            BetBrowserControl? control;
            lock (_controlLock)
            {
                control = _browserControl;
                
                if (control == null || !control.IsInitialized)
                {
                    return new BetResult
                    {
                        Success = false,
                        ErrorMessage = "浏览器未初始化"
                    };
                }
            }
            
            try
            {
                // 直接调用控件的方法
                return await control.ExecuteCommandAsync(command, data);
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
        
        /// <summary>
        /// 检查浏览器状态（Ping）
        /// </summary>
        public virtual async Task<(bool IsAlive, int ProcessId)> PingAsync()
        {
            try
            {
                if (!IsConnected)
                {
                    return (false, 0);
                }
                
                var result = await SendCommandAsync("心跳检测");
                if (result.Success)
                {
                    return (true, 0);  // 控件方式没有进程ID
                }
                
                return (false, 0);
            }
            catch
            {
                return (false, 0);
            }
        }
        
        /// <summary>
        /// 停止并清理资源
        /// </summary>
        /// <param name="killProcess">是否终止浏览器（控件方式下此参数无效）</param>
        public virtual void Dispose(bool killProcess = false)
        {
            try
            {
                lock (_controlLock)
                {
                    if (_browserControl != null)
                    {
                        _browserControl.Dispose();
                        _browserControl = null;
                    }
                }
            }
            catch
            {
                // 忽略清理错误
            }
        }
        
        void IDisposable.Dispose()
        {
            Dispose(killProcess: false);
        }
        
        /// <summary>
        /// 获取连接对象（控件方式下不再需要，保留以兼容接口）
        /// </summary>
        public object? GetConnection()
        {
            return null;  // 控件方式没有 Socket 连接
        }
        
        /// <summary>
        /// 附加连接（控件方式下不再需要，保留以兼容接口）
        /// </summary>
        public void AttachConnection(object? connection)
        {
            // 控件方式不需要 Socket 连接
        }
        
        /// <summary>
        /// 处理消息（控件方式下不再需要，保留以兼容接口）
        /// </summary>
        public void OnMessageReceived(object message)
        {
            // 控件方式不需要消息处理
        }
        
        /// <summary>
        /// 检查进程是否运行（控件方式下始终返回 false）
        /// </summary>
        public bool IsProcessRunning => false;
    }
}

