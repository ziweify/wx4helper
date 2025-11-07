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
        private readonly Action<CommandRequest> _onCommandReceived;
        private readonly Action<string> _onLog;
        
        private TcpListener? _listener;
        private TcpClient? _client;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private CancellationTokenSource? _cts;
        private Task? _listenerTask;
        
        public bool IsRunning { get; private set; }
        
        public SocketServer(int port, Action<CommandRequest> onCommandReceived, Action<string> onLog)
        {
            _port = port;
            _onCommandReceived = onCommandReceived;
            _onLog = onLog;
        }
        
        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;
            
            try
            {
                _cts = new CancellationTokenSource();
                _listener = new TcpListener(IPAddress.Loopback, _port);
                _listener.Start();
                
                IsRunning = true;
                _onLog($"✅ Socket 服务器已启动，端口: {_port}");
                
                _listenerTask = Task.Run(() => ListenAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                _onLog($"❌ Socket 服务器启动失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            if (!IsRunning) return;
            
            IsRunning = false;
            
            _cts?.Cancel();
            _listener?.Stop();
            
            _reader?.Dispose();
            _writer?.Dispose();
            _client?.Close();
            
            _onLog("⏹️ Socket 服务器已停止");
        }
        
        /// <summary>
        /// 监听连接
        /// </summary>
        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null) break;
                    
                    _onLog("⏳ 等待主程序连接...");
                    
                    // 接受连接
                    _client = await _listener.AcceptTcpClientAsync();
                    _onLog("✅ 主程序已连接");
                    
                    var stream = _client.GetStream();
                    _reader = new StreamReader(stream, Encoding.UTF8);
                    _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                    
                    // 处理命令
                    await ProcessCommandsAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _onLog($"❌ 监听错误: {ex.Message}");
                    await Task.Delay(1000, cancellationToken); // 延迟后重试
                }
            }
        }
        
        /// <summary>
        /// 处理命令
        /// </summary>
        private async Task ProcessCommandsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _reader != null)
            {
                try
                {
                    var line = await _reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line))
                    {
                        _onLog("⚠️ 连接已断开");
                        break;
                    }
                    
                    _onLog($"📩 收到命令: {line.Substring(0, Math.Min(50, line.Length))}...");
                    
                    // 解析命令
                    var command = JsonConvert.DeserializeObject<CommandRequest>(line);
                    if (command != null)
                    {
                        _onCommandReceived(command);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _onLog($"❌ 命令处理错误: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 发送响应
        /// </summary>
        public void SendResponse(CommandResponse response)
        {
            try
            {
                if (_writer == null)
                {
                    _onLog("❌ 无法发送响应：连接未建立");
                    return;
                }
                
                var json = JsonConvert.SerializeObject(response);
                _writer.WriteLine(json);
                
                _onLog($"📤 已发送响应: {response.Message}");
            }
            catch (Exception ex)
            {
                _onLog($"❌ 发送响应失败: {ex.Message}");
            }
        }
        
        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
            _listenerTask?.Dispose();
        }
    }
}
