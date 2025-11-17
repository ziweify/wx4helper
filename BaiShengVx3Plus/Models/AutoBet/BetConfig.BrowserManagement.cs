using System;
using System.Threading;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Services.AutoBet;

namespace BaiShengVx3Plus.Models.AutoBet
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
        private AutoBetSocketServer? _socketServer;
        
        #endregion
        
        #region 初始化和依赖注入
        
        /// <summary>
        /// 设置依赖服务（在 AutoBetService 中调用）
        /// </summary>
        public void SetDependencies(ILogService logService, AutoBetSocketServer socketServer)
        {
            _logService = logService;
            _socketServer = socketServer;
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
                Browser.Dispose(killProcess: true);
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
            
            // 4. 检查进程是否还在运行
            if (ProcessId > 0)
            {
                if (IsProcessRunning(ProcessId))
                {
                    _logService?.Debug("BetConfig", $"⏳ [{ConfigName}] 浏览器进程 {ProcessId} 仍在运行，等待重连...");
                    return false;
                }
                else
                {
                    // 进程已结束，清除 ProcessId
                    _logService?.Info("BetConfig", $"🔧 [{ConfigName}] 浏览器进程 {ProcessId} 已结束，清除 ProcessId");
                    ProcessId = 0;
                }
            }
            
            return true;  // 所有条件都满足，应该启动浏览器
        }
        
        /// <summary>
        /// 检查进程是否还在运行
        /// </summary>
        private bool IsProcessRunning(int processId)
        {
            try
            {
                var process = System.Diagnostics.Process.GetProcessById(processId);
                bool hasExited = process.HasExited;
                
                if (!hasExited)
                {
                    try
                    {
                        var _ = process.ProcessName;  // 尝试访问进程名，验证进程是否真实存在
                    }
                    catch
                    {
                        return false;
                    }
                }
                return !hasExited;
            }
            catch (ArgumentException)
            {
                return false;  // 进程不存在
            }
            catch (InvalidOperationException)
            {
                return false;  // 进程已退出
            }
            catch (Exception ex)
            {
                _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 检查进程 {processId} 时发生异常: {ex.Message}");
                return false;
            }
        }
        
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
                
                // 清理旧的 ProcessId
                if (ProcessId > 0)
                {
                    _logService?.Info("BetConfig", $"🔧 [{ConfigName}] 清除旧的 ProcessId: {ProcessId}");
                    ProcessId = 0;
                }
                
                // 创建浏览器客户端
                var newBrowser = new BrowserClient(configId: Id);
                
                // 🔥 启动浏览器进程（异步调用）
                bool started = await newBrowser.StartAsync(
                    port: 0,  // 0 = 使用默认端口
                    configName: ConfigName,
                    platform: Platform,
                    platformUrl: PlatformUrl
                );
                
                if (started)
                {
                    // 🔥 启动成功后再设置到 Browser 属性
                    lock (_browserLock)
                    {
                        Browser = newBrowser;
                    }
                    
                    // 获取进程ID并保存
                    if (newBrowser.IsProcessRunning)
                    {
                        try
                        {
                            var processField = newBrowser.GetType().GetField("_process", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (processField != null)
                            {
                                var process = processField.GetValue(newBrowser) as System.Diagnostics.Process;
                                if (process != null && !process.HasExited)
                                {
                                    ProcessId = process.Id;
                                    _logService?.Info("BetConfig", $"✅ [{ConfigName}] 浏览器进程已启动: PID={ProcessId}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 获取进程ID失败: {ex.Message}");
                        }
                    }
                    
                    // 🔥 等待浏览器连接到 Socket 服务器（最多等待5秒）
                    _logService?.Info("BetConfig", $"⏳ [{ConfigName}] 等待浏览器连接到 Socket 服务器...");
                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(500);  // 每500ms检查一次
                        if (IsConnected)
                        {
                            _logService?.Info("BetConfig", $"✅ [{ConfigName}] 浏览器已连接！等待时间: {i * 0.5}秒");
                            break;
                        }
                    }
                    
                    // 🔥 自动登录（如果配置了账号密码）
                    if (AutoLogin && !string.IsNullOrEmpty(Username))
                    {
                        _logService?.Info("BetConfig", $"🔐 [{ConfigName}] 自动登录: {Username}");
                        try
                        {
                            var loginResult = await newBrowser.SendCommandAsync("Login", new
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
                else
                {
                    _logService?.Error("BetConfig", $"❌ [{ConfigName}] 浏览器启动失败");
                    newBrowser.Dispose(killProcess: true);
                }
            }
            catch (Exception ex)
            {
                _logService?.Error("BetConfig", $"❌ [{ConfigName}] 启动浏览器时发生异常", ex);
                lock (_browserLock)
                {
                    Browser?.Dispose(killProcess: true);
                    Browser = null;
                }
            }
        }
        
        /// <summary>
        /// 浏览器断开连接的事件处理
        /// </summary>
        private void OnBrowserDisconnected(object? sender, EventArgs e)
        {
            _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 浏览器断开连接");
            
            lock (_browserLock)
            {
                // 检查进程是否真的结束了
                if (ProcessId > 0)
                {
                    if (IsProcessRunning(ProcessId))
                    {
                        _logService?.Warning("BetConfig", $"⚠️ [{ConfigName}] 浏览器进程 {ProcessId} 仍在运行，但连接已断开");
                        _logService?.Info("BetConfig", $"   保留 ProcessId，监控线程将等待重连或进程退出");
                    }
                    else
                    {
                        _logService?.Info("BetConfig", $"🔧 [{ConfigName}] 浏览器进程 {ProcessId} 已结束，清除 ProcessId");
                        ProcessId = 0;
                    }
                }
                
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
                    Browser.Dispose(killProcess: true);
                    Browser = null;
                }
            }
        }
        
        #endregion
    }
}

