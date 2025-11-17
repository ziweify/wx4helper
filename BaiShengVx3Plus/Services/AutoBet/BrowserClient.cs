using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models.AutoBet;
using Newtonsoft.Json;

namespace BaiShengVx3Plus.Services.AutoBet
{
    /// <summary>
    /// 浏览器客户端 - 启动进程并通过 Socket 通信
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
        private Process? _process;
        private AutoBetSocketServer.ClientConnection? _connection;  // 🔥 改为使用 ClientConnection
        
        // 🔥 响应等待机制
        private readonly Dictionary<string, TaskCompletionSource<Newtonsoft.Json.Linq.JObject>> _pendingResponses = new();
        private readonly object _responseLock = new();
        
        public bool IsConnected => _connection != null && _connection.IsConnected;
        
        /// <summary>
        /// 🔥 获取底层连接对象（用于诊断）
        /// </summary>
        public AutoBetSocketServer.ClientConnection? GetConnection() => _connection;
        
        /// <summary>
        /// 检查进程是否还在运行
        /// </summary>
        public bool IsProcessRunning
        {
            get
            {
                try
                {
                    return _process != null && !_process.HasExited;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        public BrowserClient(int configId)
        {
            _configId = configId;
        }
        
        /// <summary>
        /// 处理来自 AutoBetSocketServer 的响应消息
        /// </summary>
        public void OnMessageReceived(Newtonsoft.Json.Linq.JObject message)
        {
            try
            {
                // 检查是否是命令响应
                var success = message["success"]?.ToObject<bool>();
                var configId = message["configId"]?.ToString();
                
                if (success != null && configId == _configId.ToString())
                {
                    // 这是一个命令响应
                    var requestId = $"cmd_{_configId}";  // 简化版：每个配置同时只有一个pending命令
                    
                    lock (_responseLock)
                    {
                        if (_pendingResponses.TryGetValue(requestId, out var tcs))
                        {
                            _pendingResponses.Remove(requestId);
                            tcs.SetResult(message);
                            Console.WriteLine($"[BrowserClient] 响应已分发到等待的命令");
                        }
                        else
                        {
                            Console.WriteLine($"[BrowserClient] 收到响应，但没有等待的命令");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BrowserClient] OnMessageReceived 错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔥 附加已建立的连接（用于浏览器主动连接的情况）
        /// 改为接收 ClientConnection，避免 Socket 冲突
        /// </summary>
        public void AttachConnection(AutoBetSocketServer.ClientConnection? connection)
        {
            // 🔥 直接使用 ClientConnection，不再创建新的 reader/writer
            _connection = connection;
            
            // 🔥 连接已附加，IsConnected 属性会自动反映真实状态
        }
        
        /// <summary>
        /// 附加到已存在的浏览器进程（主程序重启场景）
        /// </summary>
        public void AttachToExistingProcess(Process process)
        {
            _process = process;
            // Socket 连接会在浏览器重连时由 AutoBetSocketServer.OnBrowserConnected 处理
        }
        
        /// <summary>
        /// 启动浏览器进程（浏览器会主动连接到 VxMain 的 Socket 服务器）
        /// </summary>
        public async Task<bool> StartAsync(int port, string configName, string platform, string platformUrl)
        {
            try
            {
                // 1. 启动浏览器进程
                // 🔥 修改：浏览器程序和主程序在同一文件夹
                var browserDirectory = AppDomain.CurrentDomain.BaseDirectory;
                
                var browserExePath = Path.Combine(browserDirectory, "BsBrowserClient.exe");
                
                if (!File.Exists(browserExePath))
                {
                    throw new FileNotFoundException($"浏览器程序不存在: {browserExePath}");
                }
                
                _process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = browserExePath,
                        // 🔥 传递 configId，用于HTTP API获取配置（账号、密码等）
                        Arguments = $"--config-id {_configId} --config-name \"{configName}\" --port {port} --platform {platform} --url {platformUrl}",
                        WorkingDirectory = browserDirectory, // 设置工作目录为浏览器所在目录
                        UseShellExecute = false,
                        CreateNoWindow = false // 显示浏览器窗口
                    }
                };
                
                _process.Start();
                
                // 2. 等待进程启动（浏览器会主动连接到 VxMain:19527）
                await Task.Delay(1000);
                
                // 3. 检查进程是否成功启动
                if (_process.HasExited)
                {
                    throw new Exception($"浏览器进程启动失败，退出代码: {_process.ExitCode}");
                }
                
                // ✅ 进程启动成功，Socket 连接由 AutoBetSocketServer.OnBrowserConnected 处理
                return true;
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }
        
        /// <summary>
        /// 🔥 发送命令并等待响应（通过 ClientConnection 发送，避免 Socket 冲突）
        /// </summary>
        public async Task<BetResult> SendCommandAsync(string command, object? data = null)
        {
            // 🔥 详细的连接状态检查
            Console.WriteLine($"[BrowserClient] SendCommandAsync 调用:");
            Console.WriteLine($"  - ConfigId: {_configId}");
            Console.WriteLine($"  - _connection == null: {_connection == null}");
            Console.WriteLine($"  - _connection?.IsConnected: {_connection?.IsConnected}");
            Console.WriteLine($"  - IsConnected: {IsConnected}");
            
            if (!IsConnected)
            {
                Console.WriteLine($"[BrowserClient] ❌ 连接检查失败，返回错误");
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "未连接到浏览器"
                };
            }
            
            try
            {
                // 构造请求
                var request = new
                {
                    command = command,
                    data = data
                };
                
                // 创建响应等待任务
                var requestId = $"cmd_{_configId}";
                var tcs = new TaskCompletionSource<Newtonsoft.Json.Linq.JObject>();
                
                lock (_responseLock)
                {
                    _pendingResponses[requestId] = tcs;
                }
                
                // 🔥 通过 ClientConnection 发送命令（避免 Socket 冲突）
                var json = JsonConvert.SerializeObject(request);
                Console.WriteLine($"[BrowserClient] 发送命令:{command} ConfigId:{_configId}");
                Console.WriteLine($"[BrowserClient] 发送数据:{json.Substring(0, Math.Min(200, json.Length))}...");
                
                var sendSuccess = await _connection!.SendCommandAsync(command, data);
                if (!sendSuccess)
                {
                    Console.WriteLine($"[BrowserClient] ❌ 发送命令失败");
                    lock (_responseLock)
                    {
                        _pendingResponses.Remove(requestId);
                    }
                    return new BetResult
                    {
                        Success = false,
                        ErrorMessage = "发送命令失败"
                    };
                }
                
                Console.WriteLine($"[BrowserClient] ✅ 命令已发送，等待响应...");
                
                // 🔥 等待响应（通过回调触发）
                Newtonsoft.Json.Linq.JObject? responseObj = null;
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                try
                {
                    // 等待 OnMessageReceived 设置结果
                    responseObj = await tcs.Task.WaitAsync(cts.Token);
                    Console.WriteLine($"[BrowserClient] 收到响应（通过回调）");
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"[BrowserClient] ⏱️ 超时！30秒未收到响应");
                    
                    // 清理
                    lock (_responseLock)
                    {
                        _pendingResponses.Remove(requestId);
                    }
                    
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BrowserClient] ❌ 等待响应异常: {ex.Message}");
                    
                    // 清理
                    lock (_responseLock)
                    {
                        _pendingResponses.Remove(requestId);
                    }
                    
                    throw;
                }
                
                if (responseObj == null)
                {
                    return new BetResult
                    {
                        Success = false,
                        ErrorMessage = "未收到响应"
                    };
                }
                
                // 解析响应
                var result = new BetResult
                {
                    Success = responseObj["success"]?.ToObject<bool>() ?? false,
                    ErrorMessage = responseObj["errorMessage"]?.ToString()
                };
                
                // 🔥 解析详细投注结果
                var responseData = responseObj["data"];
                if (responseData != null)
                {
                    result.Data = responseData;  // 保存原始数据
                    
                    // 解析时间和耗时
                    var postStartStr = responseData["postStartTime"]?.ToString();
                    var postEndStr = responseData["postEndTime"]?.ToString();
                    
                    if (!string.IsNullOrEmpty(postStartStr) && DateTime.TryParse(postStartStr, out var postStart))
                    {
                        result.PostStartTime = postStart;
                    }
                    
                    if (!string.IsNullOrEmpty(postEndStr) && DateTime.TryParse(postEndStr, out var postEnd))
                    {
                        result.PostEndTime = postEnd;
                    }
                    
                    result.DurationMs = responseData["durationMs"]?.ToObject<int?>();
                    result.OrderNo = responseData["orderNo"]?.ToString();
                    result.OrderId = responseData["orderId"]?.ToString();  // 兼容旧字段
                }
                
                return result;
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
        /// 显示窗口（通过 Socket 命令）
        /// </summary>
        public async Task<bool> ShowWindowAsync()
        {
            try
            {
                if (!IsConnected)
                {
                    return false;
                }
                
                // 发送显示命令
                var result = await SendCommandAsync("显示窗口");
                return result.Success;
            }
            catch
            {
                // 如果 Socket 失败，尝试使用 Windows API
                return ShowWindowByApi();
            }
        }
        
        /// <summary>
        /// 使用 Windows API 显示窗口（备用方法）
        /// </summary>
        private bool ShowWindowByApi()
        {
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    IntPtr hWnd = _process.MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_RESTORE);
                        SetForegroundWindow(hWnd);
                        return true;
                    }
                }
                return false;
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
                if (result.Success && result.Data != null)
                {
                    var data = result.Data as dynamic;
                    return (true, data?.processId ?? 0);
                }
                
                return (false, 0);
            }
            catch
            {
                return (false, 0);
            }
        }
        
        /// <summary>
        /// 🔥 重新连接已废弃 - 使用 ClientConnection 后由 AutoBetSocketServer 管理连接
        /// </summary>
        [Obsolete("不再使用，连接由 AutoBetSocketServer 管理")]
        public async Task<bool> ReconnectAsync(int port)
        {
            await Task.CompletedTask;
            return _connection != null && _connection.IsConnected;
        }
        
        /// <summary>
        /// 停止并清理资源
        /// </summary>
        /// <param name="killProcess">是否终止浏览器进程（默认false，保持浏览器运行以便主程序重启后重连）</param>
        public void Dispose(bool killProcess = false)
        {
            try
            {
                // 🔥 关闭 TCP 连接（通知 AutoBetSocketServer 清理）
                if (_connection != null)
                {
                    try
                    {
                        _connection.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BrowserClient] Dispose connection 错误: {ex.Message}");
                    }
                    _connection = null;
                }
                
                // 🔥 只有明确要求时才关闭进程（例如用户点击"停止浏览器"按钮）
                // 默认情况下保持浏览器运行，允许主程序重启后重连
                if (killProcess && _process != null && !_process.HasExited)
                {
                    _process.Kill();
                    _process?.Dispose();
                    _process = null;
                }
            }
            catch
            {
                // 忽略清理错误
            }
        }
        
        void IDisposable.Dispose()
        {
            // IDisposable 接口实现：默认不杀进程
            Dispose(killProcess: false);
        }
    }
}
