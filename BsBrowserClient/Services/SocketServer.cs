using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BsBrowserClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BsBrowserClient.Services
{
    /// <summary>
    /// Socket 服务器 - 接收主程序的命令
    /// </summary>
    public class SocketServer : IDisposable
    {
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _listenerTask;
        
        public event EventHandler<string>? OnLog;
        public event EventHandler<CommandRequest>? OnCommandReceived;
        
        public bool IsRunning { get; private set; }
        
        public SocketServer(int port)
        {
            _port = port;
        }
        
        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;
            
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();
            
            IsRunning = true;
            Log($"✅ Socket 服务器已启动，端口: {_port}");
            
            _listenerTask = Task.Run(() => ListenAsync(_cts.Token), _cts.Token);
        }
        
        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            if (!IsRunning) return;
            
            _cts?.Cancel();
            _listener?.Stop();
            IsRunning = false;
            
            Log("⏹️ Socket 服务器已停止");
        }
        
        /// <summary>
        /// 监听连接
        /// </summary>
        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                    Log($"📡 客户端已连接: {client.Client.RemoteEndPoint}");
                    
                    // 处理客户端（不等待，允许多个连接）
                    _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                Log($"❌ 监听异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理客户端连接
        /// </summary>
        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                using (client)
                {
                    var stream = client.GetStream();
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                    
                    while (!cancellationToken.IsCancellationRequested && client.Connected)
                    {
                        try
                        {
                            // 读取一行JSON
                            var line = await reader.ReadLineAsync(cancellationToken);
                            if (string.IsNullOrEmpty(line))
                            {
                                Log("⚠️ 客户端断开连接");
                                break;
                            }
                            
                            Log($"📥 收到命令: {line}");
                            
                            // 解析请求
                            var request = JsonConvert.DeserializeObject<CommandRequest>(line);
                            if (request == null)
                            {
                                await SendErrorResponseAsync(writer, "无效的请求格式");
                                continue;
                            }
                            
                            // 触发命令事件（由主窗体处理）
                            var response = await HandleCommandAsync(request);
                            
                            // 返回响应
                            var json = JsonConvert.SerializeObject(response);
                            await writer.WriteLineAsync(json);
                            Log($"📤 返回响应: {json}");
                        }
                        catch (JsonException ex)
                        {
                            Log($"❌ JSON 解析错误: {ex.Message}");
                            await SendErrorResponseAsync(writer, "JSON 格式错误");
                        }
                        catch (IOException)
                        {
                            Log("⚠️ 客户端连接已断开");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 处理客户端异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理命令（同步等待结果）
        /// </summary>
        private async Task<CommandResponse> HandleCommandAsync(CommandRequest request)
        {
            try
            {
                // 使用 TaskCompletionSource 等待 UI 线程处理
                var tcs = new TaskCompletionSource<CommandResponse>();
                
                // 在 UI 线程触发事件
                OnCommandReceived?.Invoke(this, request);
                
                // TODO: 这里需要改进，应该等待主窗体返回结果
                // 暂时返回成功
                return await Task.FromResult(new CommandResponse
                {
                    Success = true,
                    Data = new { Message = "命令已接收" }
                });
            }
            catch (Exception ex)
            {
                return new CommandResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        /// <summary>
        /// 发送错误响应
        /// </summary>
        private async Task SendErrorResponseAsync(StreamWriter writer, string errorMessage)
        {
            var response = new CommandResponse
            {
                Success = false,
                ErrorMessage = errorMessage
            };
            var json = JsonConvert.SerializeObject(response);
            await writer.WriteLineAsync(json);
        }
        
        private void Log(string message)
        {
            OnLog?.Invoke(this, $"[{DateTime.Now:HH:mm:ss}] {message}");
        }
        
        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
            _listener?.Stop();
        }
    }
}

