using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
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
        private readonly int _configId;
        private Process? _process;
        private TcpClient? _socket;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        
        public bool IsConnected => _socket != null && _socket.Connected;
        
        public BrowserClient(int configId)
        {
            _configId = configId;
        }
        
        /// <summary>
        /// 启动浏览器进程并连接
        /// </summary>
        public async Task<bool> StartAsync(int port, string platform, string platformUrl)
        {
            try
            {
                // 1. 启动浏览器进程
                var browserExePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "BsBrowserClient.exe");
                
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
