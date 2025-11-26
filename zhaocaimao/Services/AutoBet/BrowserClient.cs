using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zhaocaimao.Models.AutoBet;
using zhaocaimao.Views.AutoBet;
using Newtonsoft.Json;

namespace zhaocaimao.Services.AutoBet
{
    /// <summary>
    /// 浏览器客户端 - 使用内置浏览器窗口（WebView2）
    /// 不再启动外部进程，直接使用进程内的浏览器窗口
    /// </summary>
    public class BrowserClient : IDisposable
    {
        // Windows API 用于显示窗口
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private readonly int _configId;
        private BetBrowserForm? _browserForm;  // 🔥 使用内置浏览器窗口
        private readonly object _browserLock = new object(); // 🔥 线程安全：保护浏览器窗口的访问
        
        /// <summary>
        /// 是否已连接（浏览器窗口已初始化）
        /// </summary>
        public bool IsConnected
        {
            get
            {
                lock (_browserLock)
                {
                    return _browserForm != null && _browserForm.IsInitialized && !_browserForm.IsDisposed;
                }
            }
        }
        
        /// <summary>
        /// 检查浏览器窗口是否还在运行
        /// </summary>
        public bool IsProcessRunning
        {
            get
            {
                lock (_browserLock)
                {
                    return _browserForm != null && !_browserForm.IsDisposed;
                }
            }
        }
        
        public BrowserClient(int configId)
        {
            _configId = configId;
        }
        
        /// <summary>
        /// 🔥 获取浏览器窗口（用于诊断）
        /// </summary>
        public BetBrowserForm? GetBrowserForm()
        {
            lock (_browserLock)
            {
                return _browserForm;
            }
        }
        
        /// <summary>
        /// 🔥 获取底层连接对象（用于诊断，兼容旧代码）
        /// 注意：使用内置窗口时，不再有 Socket 连接，返回 null
        /// </summary>
        [Obsolete("使用内置浏览器窗口，不再有 Socket 连接")]
        public AutoBetSocketServer.ClientConnection? GetConnection()
        {
            return null;  // 内置窗口不再有 Socket 连接
        }
        
        /// <summary>
        /// 启动浏览器窗口（使用内置 WebView2 控件）
        /// </summary>
        public async Task<bool> StartAsync(int port, string configName, string platform, string platformUrl)
        {
            try
            {
                lock (_browserLock)
                {
                    // 如果窗口已存在，直接返回
                    if (_browserForm != null && !_browserForm.IsDisposed)
                    {
                        // 激活现有窗口
                        if (_browserForm.WindowState == FormWindowState.Minimized)
                        {
                            _browserForm.WindowState = FormWindowState.Normal;
                        }
                        _browserForm.Activate();
                        _browserForm.BringToFront();
                        return true;
                    }
                }
                
                // 在 UI 线程中创建窗口
                BetBrowserForm? newForm = null;
                var tcs = new TaskCompletionSource<BetBrowserForm>();
                
                if (Application.OpenForms.Count > 0)
                {
                    var mainForm = Application.OpenForms[0];
                    mainForm.Invoke((MethodInvoker)(() =>
                    {
                        try
                        {
                            newForm = new BetBrowserForm(_configId, configName, platform, platformUrl, 
                                (msg) => Console.WriteLine($"[BrowserClient-{_configId}] {msg}"));
                            
                            // 订阅窗口关闭事件
                            newForm.FormClosed += (s, e) =>
                            {
                                lock (_browserLock)
                                {
                                    if (_browserForm == newForm)
                                    {
                                        Console.WriteLine($"[BrowserClient-{_configId}] ⚠️ 浏览器窗口已关闭");
                                        Console.WriteLine($"[BrowserClient-{_configId}] 💡 提示：如果需要继续飞单，请重新开启飞单开关或在配置管理器中启动浏览器");
                                        _browserForm = null;
                                        // 🔥 不自动重启浏览器，由监控线程检测 IsConnected=false 后自动重启
                                    }
                                }
                            };
                            
                            // 🔥 确保窗口显示
                            newForm.Show();
                            newForm.WindowState = FormWindowState.Normal;
                            newForm.Visible = true;
                            newForm.BringToFront();
                            newForm.Activate();
                            tcs.SetResult(newForm);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    }));
                }
                else
                {
                    // 如果没有主窗口，直接创建（需要在 UI 线程中）
                    System.Threading.SynchronizationContext? syncContext = null;
                    try
                    {
                        syncContext = System.Threading.SynchronizationContext.Current;
                    }
                    catch { }
                    
                    if (syncContext != null)
                    {
                        syncContext.Post(_ =>
                        {
                            try
                            {
                                newForm = new BetBrowserForm(_configId, configName, platform, platformUrl,
                                    (msg) => Console.WriteLine($"[BrowserClient-{_configId}] {msg}"));
                                
                                // 订阅窗口关闭事件
                                newForm.FormClosed += (s, e) =>
                                {
                                    lock (_browserLock)
                                    {
                                        if (_browserForm == newForm)
                                        {
                                            _browserForm = null;
                                        }
                                    }
                                };
                                
                                // 🔥 确保窗口显示
                                newForm.Show();
                                newForm.WindowState = FormWindowState.Normal;
                                newForm.Visible = true;
                                newForm.BringToFront();
                                newForm.Activate();
                                tcs.SetResult(newForm);
                            }
                            catch (Exception ex)
                            {
                                tcs.SetException(ex);
                            }
                        }, null);
                    }
                    else
                    {
                        throw new InvalidOperationException("无法在非 UI 线程中创建浏览器窗口");
                    }
                }
                
                // 等待窗口创建完成
                newForm = await tcs.Task;
                
                // 🔥 等待浏览器初始化（增加到 30 秒，首次安装 WebView2 可能需要更长时间）
                int retryCount = 0;
                int maxRetry = 60; // 30 秒 (60 * 500ms)
                while (retryCount < maxRetry && (newForm == null || !newForm.IsInitialized))
                {
                    await Task.Delay(500);
                    retryCount++;
                    
                    // 每 5 秒输出一次等待状态
                    if (retryCount % 10 == 0)
                    {
                        Console.WriteLine($"[BrowserClient] ⏳ 等待浏览器初始化... ({retryCount * 0.5}/{maxRetry * 0.5}秒)");
                    }
                }
                
                if (newForm == null || !newForm.IsInitialized)
                {
                    string formStatus = newForm == null ? "窗口未创建" : "窗口已创建但未初始化";
                    throw new Exception($"❌ 浏览器窗口初始化超时（等待了{retryCount * 0.5}秒）\n" +
                        $"📊 当前状态：{formStatus}\n" +
                        $"🔍 可能原因：\n" +
                        $"  1. WebView2 运行时未安装或首次初始化耗时较长\n" +
                        $"  2. 网络连接问题导致页面加载失败\n" +
                        $"  3. 防火墙或杀毒软件阻止了 WebView2\n" +
                        $"  4. 系统资源不足（内存/CPU占用过高）\n" +
                        $"💡 建议：\n" +
                        $"  - 检查 Windows 更新，确保 Edge 浏览器已安装\n" +
                        $"  - 前往 https://go.microsoft.com/fwlink/p/?LinkId=2124703 手动下载 WebView2 运行时\n" +
                        $"  - 检查防火墙/杀毒软件设置\n" +
                        $"  - 重启程序后重试");
                }
                
                lock (_browserLock)
                {
                    _browserForm = newForm;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BrowserClient-{_configId}] ❌ 启动浏览器窗口失败");
                Console.WriteLine($"[BrowserClient-{_configId}] 📋 异常类型: {ex.GetType().Name}");
                Console.WriteLine($"[BrowserClient-{_configId}] 📋 异常消息: {ex.Message}");
                Console.WriteLine($"[BrowserClient-{_configId}] 📍 堆栈跟踪:\n{ex.StackTrace}");
                
                // 检查内部异常
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[BrowserClient-{_configId}] 🔗 内部异常: {ex.InnerException.Message}");
                }
                
                Dispose();
                throw;
            }
        }
        
        /// <summary>
        /// 🔥 发送命令并等待响应（直接调用浏览器窗口的命令接口）
        /// </summary>
        public async Task<BetResult> SendCommandAsync(string command, object? data = null)
        {
            BetBrowserForm? browserForm;
            lock (_browserLock)
            {
                browserForm = _browserForm;
                
                // 🔥 增强诊断信息：准确说明失败原因
                if (browserForm == null)
                {
                    return new BetResult
                    {
                        Success = false,
                        ErrorMessage = "浏览器窗口未创建（browserForm == null）"
                    };
                }
                
                if (browserForm.IsDisposed)
                {
                    return new BetResult
                    {
                        Success = false,
                        ErrorMessage = "浏览器窗口已关闭（IsDisposed）"
                    };
                }
                
                if (!browserForm.IsInitialized)
                {
                    return new BetResult
                    {
                        Success = false,
                        ErrorMessage = "浏览器未完成初始化（WebView2 未就绪）"
                    };
                }
            }
            
            try
            {
                // 直接调用浏览器窗口的命令接口
                return await browserForm.ExecuteCommandAsync(command, data);
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
        /// 显示窗口
        /// </summary>
        public async Task<bool> ShowWindowAsync()
        {
            try
            {
                BetBrowserForm? browserForm;
                lock (_browserLock)
                {
                    browserForm = _browserForm;
                }
                
                if (browserForm == null || browserForm.IsDisposed)
                {
                    return false;
                }
                
                // 在 UI 线程中显示窗口
                if (browserForm.InvokeRequired)
                {
                    browserForm.Invoke((MethodInvoker)(() =>
                    {
                        if (browserForm.WindowState == FormWindowState.Minimized)
                        {
                            browserForm.WindowState = FormWindowState.Normal;
                        }
                        browserForm.Activate();
                        browserForm.BringToFront();
                    }));
                }
                else
                {
                    if (browserForm.WindowState == FormWindowState.Minimized)
                    {
                        browserForm.WindowState = FormWindowState.Normal;
                    }
                    browserForm.Activate();
                    browserForm.BringToFront();
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 检查浏览器状态（Ping）
        /// </summary>
        public async Task<(bool IsAlive, int ProcessId)> PingAsync()
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
                    // 返回进程ID为当前进程ID（因为使用内置窗口）
                    return (true, Process.GetCurrentProcess().Id);
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
        /// <param name="killProcess">是否关闭浏览器窗口（默认false，保持窗口运行）</param>
        public void Dispose(bool killProcess = false)
        {
            try
            {
                BetBrowserForm? browserFormToDispose = null;
                lock (_browserLock)
                {
                    browserFormToDispose = _browserForm;
                    _browserForm = null;
                }
                
                // 在锁外执行 Dispose（避免死锁）
                if (browserFormToDispose != null && killProcess)
                {
                    try
                    {
                        if (browserFormToDispose.InvokeRequired)
                        {
                            browserFormToDispose.Invoke((MethodInvoker)(() =>
                            {
                                browserFormToDispose.Close();
                                browserFormToDispose.Dispose();
                            }));
                        }
                        else
                        {
                            browserFormToDispose.Close();
                            browserFormToDispose.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BrowserClient] Dispose browser form 错误: {ex.Message}");
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
            // IDisposable 接口实现：默认不关闭窗口
            Dispose(killProcess: false);
        }
    }
}
