using System;
using System.Threading;
using zhaocaimao.Contracts;
using zhaocaimao.Services.AutoBet;

namespace zhaocaimao.Models.AutoBet
{
    /// <summary>
    /// BetConfig 的浏览器自管理功能
    /// 每个配置独立管理自己的浏览器和监控线程
    /// </summary>
    public partial class BetConfig
    {
        #region 私有字段（运行时状态，不持久化）
        
        private bool _isStartingBrowser; // 🔥 正在启动浏览器的标志，防止重复启动
        private readonly object _browserLock = new object();
        private ILogService? _logService;
        // 🔥 控件方式：不再需要 Socket 服务器和监控线程
        
        #endregion
        
        #region 初始化和依赖注入
        
        /// <summary>
        /// 设置依赖服务（在 AutoBetService 中调用）
        /// </summary>
        public void SetDependencies(ILogService logService)
        {
            _logService = logService;
            // 🔥 内部 WebView2 控件方式：不需要 Socket Server
            
            // 🔥 诊断日志：记录依赖注入
            _logService?.Info("BetConfig", $"✅ [{ConfigName}] 依赖服务已注入");
        }
        
        #endregion
        
        #region 公共方法：生命周期管理
        
        /// <summary>
        /// 启动浏览器（公共方法，供用户手动调用或 IsEnabled 变化时自动调用）
        /// </summary>
        public async Task StartBrowserManuallyAsync()
        {
            _logService?.Info("BetConfig", $"🚀 [{ConfigName}] 启动浏览器");
            await StartBrowserInternalAsync();
        }
        
        /// <summary>
        /// 停止浏览器（公共方法，供用户手动调用）
        /// </summary>
        public void StopBrowserManually()
        {
            lock (_browserLock)
            {
                if (Browser == null)
                {
                    _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 浏览器未运行");
                    return;
                }
                
                _logService?.Info("BetConfig", $"🛑 [{ConfigName}] 用户手动停止浏览器");
                Browser?.Dispose();
                Browser = null;
                ProcessId = 0;
            }
        }
        
        /// <summary>
        /// 显示浏览器控件（如果 ShowBrowserWindow 为 true）
        /// </summary>
        private void ShowBrowserControl()
        {
            if (!ShowBrowserWindow)
            {
                _logService?.Info("BetConfig", $"ℹ️ [{ConfigName}] ShowBrowserWindow=false，不显示浏览器窗口");
                return;
            }
            
            lock (_browserLock)
            {
                if (Browser == null || !Browser.IsInitialized)
                {
                    _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 浏览器未初始化，无法显示");
                    return;
                }
                
                // 🔥 显示浏览器控件
                // 注意：这里需要将控件添加到某个窗体中才能显示
                // 如果控件已经在窗体中，只需要设置 Visible = true
                if (Browser.Parent == null)
                {
                    _logService?.Info("BetConfig", $"ℹ️ [{ConfigName}] 浏览器控件未添加到窗体，需要手动添加到窗体才能显示");
                    // TODO: 如果需要自动显示，可以创建一个浏览器窗口窗体
                }
                else
                {
                    Browser.Visible = true;
                    Browser.BringToFront();
                    _logService?.Info("BetConfig", $"✅ [{ConfigName}] 浏览器控件已显示");
                }
            }
        }
        
        #endregion
        
        #region 私有方法：浏览器启动
        
        /// <summary>
        /// 内部方法：实际启动浏览器
        /// </summary>
        private async Task StartBrowserInternalAsync()
        {
            // 🔥 检查是否正在启动（防止重复启动）
            if (_isStartingBrowser)
            {
                _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 浏览器正在启动中，跳过重复启动");
                return;
            }
            
            // 🔥 检查浏览器是否已存在
            lock (_browserLock)
            {
                if (Browser != null && Browser.IsInitialized)
                {
                    _logService?.Info("BetConfig", $"✅ [{ConfigName}] 浏览器已存在且已初始化，无需重复启动");
                    return;
                }
                
                // 如果浏览器存在但未初始化，清理它
                if (Browser != null && !Browser.IsInitialized)
                {
                    _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 浏览器存在但未初始化，清理后重新创建");
                    Browser.Dispose();
                    Browser = null;
                }
                
                _isStartingBrowser = true;  // 标记正在启动
            }
            
            try
            {
                _logService?.Info("BetConfig", $"🚀 [{ConfigName}] 开始启动浏览器...");
                _logService?.Info("BetConfig", $"   配置ID: {Id}");
                _logService?.Info("BetConfig", $"   平台: {Platform}");
                _logService?.Info("BetConfig", $"   URL: {PlatformUrl}");
                _logService?.Info("BetConfig", $"   显示窗口: {ShowBrowserWindow}");
                
                // 控件方式：不需要清理 ProcessId
                
                // 🔥 直接创建浏览器控件对象
                var newBrowserControl = new UserControls.BetBrowserControl();
                
                // 订阅日志事件
                newBrowserControl.OnLog += (msg) => _logService?.Info("BetConfig", $"[{ConfigName}] {msg}");
                
                // 🔥 初始化浏览器控件（异步调用）
                try
                {
                    await newBrowserControl.InitializeAsync(Id, ConfigName, Platform, PlatformUrl);
                    
                    // 🔥 启动成功后再设置到 Browser 属性（立即设置，不等待完全初始化）
                    lock (_browserLock)
                    {
                        Browser = newBrowserControl;
                    }
                    
                    _logService?.Info("BetConfig", $"✅ [{ConfigName}] 浏览器控件已创建并开始初始化");
                    
                    // 🔥 立即完成，不等待初始化循环
                    // 浏览器控件初始化是异步的，但不需要等待，可以立即使用
                    // 如果需要显示窗口，会在 ShowBrowserControl 中处理
                    
                    // 🔥 自动登录（如果配置了账号密码，在后台执行，不阻塞启动）
                    _ = Task.Run(async () =>
                    {
                        // 等待浏览器引擎初始化完成（最多等待3秒）
                        for (int i = 0; i < 6; i++)
                        {
                            await Task.Delay(500);
                            if (newBrowserControl.IsInitialized)
                            {
                                break;
                            }
                        }
                        
                        if (AutoLogin && !string.IsNullOrEmpty(Username))
                        {
                            _logService?.Info("BetConfig", $"🔐 [{ConfigName}] 自动登录: {Username}");
                            try
                            {
                                var loginResult = await newBrowserControl.ExecuteCommandAsync("Login", new
                                {
                                    username = Username,
                                    password = Password
                                });
                                
                                if (loginResult.Success)
                                {
                                    _logService?.Info("BetConfig", $"✅ [{ConfigName}] 登录成功");
                                    Status = "已登录";
                                }
                                else
                                {
                                    _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 登录失败: {loginResult.ErrorMessage}");
                                    Status = "登录失败";
                                }
                            }
                            catch (Exception ex)
                            {
                                _logService?.Error("BetConfig", $"❌ [{ConfigName}] 自动登录异常", ex);
                                Status = "登录异常";
                            }
                        }
                    });
                }
                catch (Exception initEx)
                {
                    _logService?.Error("BetConfig", $"❌ [{ConfigName}] 浏览器控件初始化失败", initEx);
                    newBrowserControl?.Dispose();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logService?.Error("BetConfig", $"❌ [{ConfigName}] 启动浏览器时发生异常", ex);
                lock (_browserLock)
                {
                    Browser?.Dispose();
                    Browser = null;
                }
            }
            finally
            {
                // 清除启动标志
                _isStartingBrowser = false;
            }
        }
        
        /// <summary>
        /// 浏览器断开连接的事件处理（控件方式下不再需要）
        /// </summary>
        private void OnBrowserDisconnected(object? sender, EventArgs e)
        {
            _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 浏览器控件断开连接");
            
            lock (_browserLock)
            {
                // 清空浏览器对象引用，监控线程会自动重启
                Browser = null;
            }
        }
        
        #endregion
        
        #region 资源清理
        
        /// <summary>
        /// 清理资源（在 Dispose 中调用）
        /// </summary>
        partial void DisposeBrowserManagement()
        {
            // 关闭浏览器
            lock (_browserLock)
            {
                if (Browser != null)
                {
                    _logService?.Info("BetConfig", $"🧹 [{ConfigName}] 清理浏览器资源");
                    Browser?.Dispose();
                    Browser = null;
                }
            }
        }
        
        #endregion
    }
}

