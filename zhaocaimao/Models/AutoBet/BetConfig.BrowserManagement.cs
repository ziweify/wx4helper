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
        
        private Thread? _monitorThread;
        private bool _monitorRunning;
        private bool _isStartingBrowser; // 🔥 正在启动浏览器的标志，防止重复启动
        private readonly object _browserLock = new object();
        private ILogService? _logService;
        // 🔥 控件方式：不再需要 Socket 服务器
        
        #endregion
        
        #region 初始化和依赖注入
        
        /// <summary>
        /// 设置依赖服务（在 AutoBetService 中调用）
        /// </summary>
        public void SetDependencies(ILogService logService, AutoBetSocketServer? socketServer = null)
        {
            _logService = logService;
            // 🔥 控件方式：不再需要 Socket 服务器，保留参数以兼容接口
        }
        
        #endregion
        
        #region 公共方法：生命周期管理
        
        /// <summary>
        /// 启动监控线程（当 IsEnabled 变为 true 时自动调用）
        /// </summary>
        public void StartMonitoring()
        {
            lock (_browserLock)
            {
                if (_monitorThread != null && _monitorThread.IsAlive)
                {
                    _logService?.Info("BetConfig", $"⚠️ [{ConfigName}] 监控线程已在运行，无需重复启动");
                    return;
                }
                
                _monitorRunning = true;
                _monitorThread = new Thread(MonitorLoop)
                {
                    Name = $"BrowserMonitor-{ConfigName}-{Id}",
                    IsBackground = true
                };
                _monitorThread.Start();
                
                _logService?.Info("BetConfig", $"✅ [{ConfigName}] 监控线程已启动");
            }
        }
        
        /// <summary>
        /// 停止监控线程（当 IsEnabled 变为 false 时自动调用）
        /// </summary>
        public void StopMonitoring()
        {
            lock (_browserLock)
            {
                if (_monitorThread == null) return;
                
                _logService?.Info("BetConfig", $"⏹️ [{ConfigName}] 停止监控线程...");
                _monitorRunning = false;
                
                // 等待线程退出（最多3秒）
                if (_monitorThread.IsAlive)
                {
                    if (!_monitorThread.Join(3000))
                    {
                        _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 监控线程未在3秒内退出");
                    }
                }
                
                _monitorThread = null;
                _logService?.Info("BetConfig", $"✅ [{ConfigName}] 监控线程已停止");
            }
        }
        
        /// <summary>
        /// 启动浏览器（公共方法，供用户手动调用）
        /// </summary>
        public async Task StartBrowserManuallyAsync()
        {
            _logService?.Info("BetConfig", $"🖱️ [{ConfigName}] 用户手动启动浏览器");
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
        
        #endregion
        
        #region 私有方法：监控循环
        
        /// <summary>
        /// 监控循环：只监控自己的连接状态
        /// </summary>
        private void MonitorLoop()
        {
            try
            {
                _logService?.Info("BetConfig", $"🚀 [{ConfigName}] 监控线程开始运行（检查间隔：2秒）");
                
                while (_monitorRunning)
                {
                    try
                    {
                        // 检查是否需要启动浏览器
                        if (ShouldStartBrowser())
                        {
                            // 延迟2秒，给老浏览器重连的机会
                            _logService?.Info("BetConfig", $"⏳ [{ConfigName}] 检测到未连接，延迟2秒再次检查...");
                            Thread.Sleep(2000);
                            
                            // 再次检查（可能在等待期间已连接）
                            if (_monitorRunning && ShouldStartBrowser())
                            {
                                // 🔥 确认需要启动后，设置正在启动标志
                                _isStartingBrowser = true;
                                
                                // 🔥 在后台线程中调用异步方法
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        await StartBrowserInternalAsync();
                                    }
                                    catch (Exception ex)
                                    {
                                        _logService?.Error("BetConfig", $"❌ [{ConfigName}] 启动浏览器时异常", ex);
                                    }
                                    finally
                                    {
                                        // 🔥 清除正在启动标志
                                        _isStartingBrowser = false;
                                    }
                                });
                            }
                        }
                        
                        // 🔥 统一使用2秒间隔，给浏览器足够时间连接
                        Thread.Sleep(2000);
                    }
                    catch (ThreadInterruptedException)
                    {
                        break;  // 线程被中断，退出循环
                    }
                    catch (Exception ex)
                    {
                        _logService?.Error("BetConfig", $"[{ConfigName}] 监控任务执行异常", ex);
                        Thread.Sleep(2000);
                    }
                }
                
                _logService?.Info("BetConfig", $"⏹️ [{ConfigName}] 监控线程已退出");
            }
            catch (Exception ex)
            {
                _logService?.Error("BetConfig", $"[{ConfigName}] 监控线程异常退出", ex);
            }
        }
        
        /// <summary>
        /// 判断是否应该启动浏览器
        /// </summary>
        private bool ShouldStartBrowser()
        {
            // 0. 🔥 检查是否正在启动（防止重复启动）
            if (_isStartingBrowser)
            {
                return false;
            }
            
            // 1. 检查配置是否启用
            if (!IsEnabled)
            {
                return false;
            }
            
            // 2. 检查是否已连接
            if (IsConnected)
            {
                return false;
            }
            
            // 3. 检查浏览器对象是否存在
            lock (_browserLock)
            {
                if (Browser != null)
                {
                    return false;  // 浏览器对象存在，可能正在启动或连接中
                }
            }
            
            // 4. 控件方式：不需要检查进程，直接检查控件状态
            // ProcessId 在控件方式下不再使用
            
            return true;  // 所有条件都满足，应该启动浏览器
        }
        
        // 🔥 控件方式：不再需要检查进程
        
        #endregion
        
        #region 私有方法：浏览器启动
        
        /// <summary>
        /// 内部方法：实际启动浏览器
        /// </summary>
        private async Task StartBrowserInternalAsync()
        {
            // 🔥 不能在 lock 内使用 await，所以先检查再锁定
            bool shouldStart = false;
            lock (_browserLock)
            {
                if (Browser != null)
                {
                    _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 浏览器已存在，跳过重复启动");
                    return;
                }
                shouldStart = true;  // 标记需要启动
            }
            
            if (!shouldStart) return;
            
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
                    
                    // 🔥 启动成功后再设置到 Browser 属性
                    lock (_browserLock)
                    {
                        Browser = newBrowserControl;
                    }
                    
                    _logService?.Info("BetConfig", $"✅ [{ConfigName}] 浏览器控件已创建并初始化");
                    
                    // 🔥 控件方式：直接检查是否已初始化（不需要等待 Socket 连接）
                    _logService?.Info("BetConfig", $"⏳ [{ConfigName}] 等待浏览器控件完全初始化...");
                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(500);  // 每500ms检查一次
                        if (IsConnected)
                        {
                            _logService?.Info("BetConfig", $"✅ [{ConfigName}] 浏览器控件已初始化！等待时间: {i * 0.5}秒");
                            break;
                        }
                    }
                    
                    // 🔥 自动登录（如果配置了账号密码）
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
                    else
                    {
                        _logService?.Info("BetConfig", $"ℹ️ [{ConfigName}] 未配置账号密码，跳过自动登录");
                    }
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
            // 停止监控线程
            StopMonitoring();
            
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

