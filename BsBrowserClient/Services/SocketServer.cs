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
        private const int VXMAIN_SERVER_PORT = 19527; // VxMain 监听的固定端口
        
        private readonly int _configId;
        private readonly Action<CommandRequest> _onCommandReceived;
        private readonly Action<string> _onLog;
        
        private TcpClient? _client;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private CancellationTokenSource? _cts;
        private Task? _listenerTask;
        
        public bool IsRunning { get; private set; }
        
        public SocketServer(int configId, Action<CommandRequest> onCommandReceived, Action<string> onLog)
        {
            _configId = configId;
            _onCommandReceived = onCommandReceived;
            _onLog = onLog;
        }
        
        /// <summary>
        /// 启动服务器（主动连接 VxMain）
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;
            
            try
            {
                _cts = new CancellationTokenSource();
                IsRunning = true;
                
                _onLog($"🔗 尝试连接到 VxMain (端口: {VXMAIN_SERVER_PORT})...");
                
                _listenerTask = Task.Run(() => ConnectAndListenAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                _onLog($"❌ 连接失败: {ex.Message}");
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
            
            _reader?.Dispose();
            _writer?.Dispose();
            _client?.Close();
            
            _onLog("⏹️ Socket 已停止");
        }
        
        /// <summary>
        /// 连接到 VxMain 并持续监听命令
        /// </summary>
        private async Task ConnectAndListenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 1. 连接到 VxMain
                    _client = new TcpClient();
                    await _client.ConnectAsync("127.0.0.1", VXMAIN_SERVER_PORT, cancellationToken);
                    _onLog("✅ 已连接到 VxMain");
                    
                    var stream = _client.GetStream();
                    _reader = new StreamReader(stream, Encoding.UTF8);
                    _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                    
                    // 2. 发送握手消息（包含配置ID）
                    var handshake = new
                    {
                        type = "hello",
                        configId = _configId
                    };
                    await _writer.WriteLineAsync(JsonConvert.SerializeObject(handshake));
                    _onLog($"📤 已发送握手，配置ID: {_configId}");
                    
                    // 3. 等待确认消息
                    var welcomeLine = await _reader.ReadLineAsync(cancellationToken);
                    if (!string.IsNullOrEmpty(welcomeLine))
                    {
                        var welcome = JsonConvert.DeserializeObject<JObject>(welcomeLine);
                        if (welcome?["type"]?.ToString() == "welcome")
                        {
                            _onLog($"✅ 握手成功: {welcome["message"]}");
                        }
                    }
                    
                    // 4. 持续处理命令
                    await ProcessCommandsAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _onLog($"❌ 连接错误: {ex.Message}");
                    
                    // 清理连接
                    _reader?.Dispose();
                    _writer?.Dispose();
                    _client?.Close();
                    
                    // 等待后重试连接
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        _onLog("⏳ 5秒后重试连接...");
                        await Task.Delay(5000, cancellationToken);
                    }
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
