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
        private TcpClient? _socket;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        
        public bool IsConnected => _socket != null && _socket.Connected;
        
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
        /// 附加已建立的连接（用于浏览器主动连接的情况）
        /// </summary>
        public void AttachConnection(TcpClient socket)
        {
            _socket = socket;
            var stream = _socket.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        }
        
        /// <summary>
        /// 启动浏览器进程并连接
        /// </summary>
        public async Task<bool> StartAsync(int port, string platform, string platformUrl)
        {
            try
            {
                // 1. 启动浏览器进程
                var browserDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "BrowserClient");
                
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
                        Arguments = $"--config-id {_configId} --port {port} --platform {platform} --url {platformUrl}",
                        WorkingDirectory = browserDirectory, // 设置工作目录为浏览器所在目录
                        UseShellExecute = false,
                        CreateNoWindow = false // 显示浏览器窗口
                    }
                };
                
                _process.Start();
                
                // 2. 等待一下让浏览器启动
                await Task.Delay(2000);
                
                // 3. 连接 Socket
                _socket = new TcpClient();
                await _socket.ConnectAsync("127.0.0.1", port);
                
                var stream = _socket.GetStream();
                _reader = new StreamReader(stream, Encoding.UTF8);
                _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                
                return true;
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }
        
        /// <summary>
        /// 发送命令并等待响应
        /// </summary>
        public async Task<BetResult> SendCommandAsync(string command, object? data = null)
        {
            if (!IsConnected)
            {
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
                
                // 发送 JSON
                var json = JsonConvert.SerializeObject(request);
                await _writer!.WriteLineAsync(json);
                
                // 接收响应
                var responseLine = await _reader!.ReadLineAsync();
                if (string.IsNullOrEmpty(responseLine))
                {
                    return new BetResult
                    {
                        Success = false,
                        ErrorMessage = "未收到响应"
                    };
                }
                
                // 解析响应
                dynamic response = JsonConvert.DeserializeObject(responseLine)!;
                
                return new BetResult
                {
                    Success = response.success ?? false,
                    OrderId = response.data?.orderId?.ToString(),
                    ErrorMessage = response.errorMessage?.ToString()
                };
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
                var result = await SendCommandAsync("show");
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
                
                var result = await SendCommandAsync("ping");
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
        /// 重新连接（用于 VxMain 重启后恢复连接）
        /// </summary>
        public async Task<bool> ReconnectAsync(int port)
        {
            try
            {
                // 如果已连接，先断开
                if (_socket != null)
                {
                    _reader?.Dispose();
                    _writer?.Dispose();
                    _socket?.Close();
                    _socket?.Dispose();
                }
                
                // 重新连接
                _socket = new TcpClient();
                await _socket.ConnectAsync("127.0.0.1", port);
                
                var stream = _socket.GetStream();
                _reader = new StreamReader(stream, Encoding.UTF8);
                _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 停止并清理资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                // 关闭 Socket
                _reader?.Dispose();
                _writer?.Dispose();
                _socket?.Close();
                _socket?.Dispose();
                
                // 关闭进程
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                }
                _process?.Dispose();
            }
            catch
            {
                // 忽略清理错误
            }
        }
    }
}
