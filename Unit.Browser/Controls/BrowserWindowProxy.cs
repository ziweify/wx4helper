using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unit.Browser.Interfaces;
using Unit.Browser.Models;

namespace Unit.Browser.Controls
{
    /// <summary>
    /// 浏览器窗口代理（运行在主线程）
    /// </summary>
    public class BrowserWindowProxy : IBrowserWindowProxy
    {
        private Thread? _windowThread;
        private BrowserWindow? _browserWindow;
        private readonly SemaphoreSlim _initializationLock = new(1, 1);
        private bool _isInitialized;
        private bool _disposed;

        // 命令队列：使用 BlockingCollection 实现线程安全的命令传递
        private readonly BlockingCollection<CommandContext> _commandQueue = new();
        
        // 正在等待响应的命令：CommandId -> TaskCompletionSource
        private readonly ConcurrentDictionary<string, TaskCompletionSource<BrowserCommandResult>> _pendingCommands = new();

        public bool IsInitialized => _isInitialized;

        public bool IsVisible
        {
            get => _browserWindow?.Visible ?? false;
            set
            {
                if (_browserWindow != null)
                {
                    _browserWindow.Invoke(() => _browserWindow.Visible = value);
                }
            }
        }

        public event EventHandler<string>? OnLog;

        /// <summary>
        /// 初始化浏览器窗口（在独立线程中运行）
        /// </summary>
        public async Task InitializeAsync(string windowTitle, string initialUrl)
        {
            await _initializationLock.WaitAsync();
            try
            {
                if (_isInitialized)
                {
                    LogMessage("⚠️ 浏览器窗口已初始化");
                    return;
                }

                LogMessage($"🚀 正在初始化浏览器窗口: {windowTitle}");

                // 创建独立线程运行浏览器窗口
                var initCompletionSource = new TaskCompletionSource<bool>();
                
                _windowThread = new Thread(() =>
                {
                    try
                    {
                        // 设置为 STA 线程（WebView2 需要）
                        Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                        
                        // 创建浏览器窗口
                        _browserWindow = new BrowserWindow(windowTitle, initialUrl);
                        
                        // 订阅窗口日志
                        _browserWindow.OnLog += (s, msg) => LogMessage(msg);
                        
                        // 设置命令处理回调
                        _browserWindow.SetCommandHandler(ProcessCommandAsync);
                        
                        // 启动命令处理循环
                        Task.Run(() => CommandProcessingLoop());
                        
                        // 通知初始化完成
                        initCompletionSource.SetResult(true);
                        
                        // 启动消息循环（阻塞当前线程）
                        Application.Run(_browserWindow);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"❌ 浏览器窗口线程异常: {ex.Message}");
                        initCompletionSource.TrySetException(ex);
                    }
                })
                {
                    IsBackground = true,
                    Name = $"BrowserWindow-{windowTitle}"
                };

                _windowThread.Start();

                // 等待窗口初始化完成
                await initCompletionSource.Task;
                
                _isInitialized = true;
                LogMessage($"✅ 浏览器窗口初始化成功: {windowTitle}");
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        public async Task<BrowserCommandResult> ExecuteCommandAsync(string commandName, object? parameters = null, int timeoutMs = 30000)
        {
            if (!_isInitialized || _browserWindow == null)
            {
                return BrowserCommandResult.CreateFailure("", "浏览器窗口未初始化");
            }

            var command = new BrowserCommand
            {
                Name = commandName,
                Parameters = parameters,
                TimeoutMs = timeoutMs
            };

            var tcs = new TaskCompletionSource<BrowserCommandResult>();
            _pendingCommands[command.CommandId] = tcs;

            // 添加到命令队列
            var context = new CommandContext
            {
                Command = command,
                CompletionSource = tcs
            };
            
            _commandQueue.Add(context);

            // 设置超时
            using var cts = new CancellationTokenSource(timeoutMs);
            cts.Token.Register(() =>
            {
                if (_pendingCommands.TryRemove(command.CommandId, out var pendingTcs))
                {
                    pendingTcs.TrySetResult(BrowserCommandResult.CreateFailure(
                        command.CommandId,
                        $"命令执行超时 ({timeoutMs}ms)"));
                }
            });

            // 等待命令执行完成
            return await tcs.Task;
        }

        /// <summary>
        /// 命令处理循环（在后台线程中运行）
        /// </summary>
        private async Task CommandProcessingLoop()
        {
            LogMessage("📋 命令处理循环已启动");
            
            try
            {
                foreach (var context in _commandQueue.GetConsumingEnumerable())
                {
                    if (_disposed) break;

                    try
                    {
                        LogMessage($"🔄 处理命令: {context.Command.Name}");
                        
                        // 在浏览器窗口线程中执行命令
                        BrowserCommandResult? result = null;
                        
                        if (_browserWindow != null)
                        {
                            await _browserWindow.InvokeAsync(async () =>
                            {
                                result = await _browserWindow.ExecuteCommandAsync(context.Command);
                            });
                        }
                        
                        if (result == null)
                        {
                            result = BrowserCommandResult.CreateFailure(
                                context.Command.CommandId,
                                "命令执行失败：未返回结果");
                        }

                        // 设置结果
                        if (_pendingCommands.TryRemove(context.Command.CommandId, out var tcs))
                        {
                            tcs.TrySetResult(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"❌ 命令处理异常: {ex.Message}");
                        
                        if (_pendingCommands.TryRemove(context.Command.CommandId, out var tcs))
                        {
                            tcs.TrySetResult(BrowserCommandResult.CreateFailure(
                                context.Command.CommandId,
                                $"命令处理异常: {ex.Message}"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 命令处理循环异常: {ex.Message}");
            }
            
            LogMessage("📋 命令处理循环已停止");
        }

        /// <summary>
        /// 处理命令（由窗口调用）
        /// </summary>
        private Task ProcessCommandAsync(BrowserCommand command)
        {
            // 此方法由 BrowserWindow 调用，用于处理特殊命令
            // 大部分命令已在 CommandExecutor 中处理
            return Task.CompletedTask;
        }

        public void ShowWindow()
        {
            if (_browserWindow != null)
            {
                _browserWindow.Invoke(() =>
                {
                    _browserWindow.Show();
                    _browserWindow.BringToFront();
                });
            }
        }

        public void HideWindow()
        {
            if (_browserWindow != null)
            {
                _browserWindow.Invoke(() => _browserWindow.Hide());
            }
        }

        public void CloseWindow()
        {
            if (_browserWindow != null)
            {
                _browserWindow.Invoke(() => _browserWindow.Close());
            }
        }

        private void LogMessage(string message)
        {
            OnLog?.Invoke(this, message);
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            
            // 停止命令队列
            _commandQueue.CompleteAdding();
            
            // 关闭窗口
            CloseWindow();
            
            // 等待窗口线程结束
            if (_windowThread != null && _windowThread.IsAlive)
            {
                if (!_windowThread.Join(TimeSpan.FromSeconds(5)))
                {
                    LogMessage("⚠️ 窗口线程未能在5秒内结束");
                }
            }
            
            _commandQueue.Dispose();
            _initializationLock.Dispose();
            
            LogMessage("🧹 浏览器窗口代理已释放");
        }
    }
}

