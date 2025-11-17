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
    /// 连接状态枚举
    /// </summary>
    public enum ConnectionStatus
    {
        断开,
        连接中,
        已连接,
        重连中
    }
    
    /// <summary>
    /// Socket 服务器 - 接收主程序的命令
    /// </summary>
    public class SocketServer : IDisposable
    {
        private const int VXMAIN_SERVER_PORT = 19527; // VxMain 监听的固定端口
        
        private readonly int _configId;
        private readonly string _configName;  // 🔥 新增配置名
        private readonly Action<CommandRequest> _onCommandReceived;
        private readonly Action<string> _onLog;
        
        private TcpClient? _client;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private CancellationTokenSource? _cts;
        private Task? _listenerTask;
        
        // 🔥 心跳定时器
        private System.Threading.Timer? _heartbeatTimer;
        private readonly object _heartbeatLock = new object();
        
        public bool IsRunning { get; private set; }
        public ConnectionStatus Status { get; private set; } = ConnectionStatus.断开;
        
        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        public event EventHandler<ConnectionStatus>? StatusChanged;
        
        public SocketServer(int configId, string configName, Action<CommandRequest> onCommandReceived, Action<string> onLog)
        {
            _configId = configId;
            _configName = configName;  // 🔥 保存配置名
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
                
                UpdateStatus(ConnectionStatus.连接中);
                _onLog($"🔗 尝试连接到 VxMain (端口: {VXMAIN_SERVER_PORT})...");
                
                _listenerTask = Task.Run(() => ConnectAndListenAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                UpdateStatus(ConnectionStatus.断开);
                _onLog($"❌ 连接失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 更新连接状态并触发事件
        /// </summary>
        private void UpdateStatus(ConnectionStatus newStatus)
        {
            if (Status != newStatus)
            {
                Status = newStatus;
                StatusChanged?.Invoke(this, newStatus);
            }
        }
        
        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            if (!IsRunning) return;
            
            IsRunning = false;
            UpdateStatus(ConnectionStatus.断开);
            
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
                    // 🔥 使用不带BOM的UTF8编码
                    var utf8NoBom = new System.Text.UTF8Encoding(false);
                    _reader = new StreamReader(stream, utf8NoBom);
                    _writer = new StreamWriter(stream, utf8NoBom) { AutoFlush = true };
                    
                    // 2. 发送握手消息（包含配置ID和配置名）
                    var handshake = new
                    {
                        type = "hello",
                        configId = _configId,
                        configName = _configName,  // 🔥 同时发送配置名
                        processId = System.Diagnostics.Process.GetCurrentProcess().Id  // 🔥 传递进程ID
                    };
                    await _writer.WriteLineAsync(JsonConvert.SerializeObject(handshake));
                    _onLog($"📤 已发送握手，配置ID: {_configId}，配置名: {_configName}");
                    
                    // 3. 等待确认消息
                    var welcomeLine = await _reader.ReadLineAsync(cancellationToken);
                    if (!string.IsNullOrEmpty(welcomeLine))
                    {
                        var welcome = JsonConvert.DeserializeObject<JObject>(welcomeLine);
                        if (welcome?["type"]?.ToString() == "welcome")
                        {
                            UpdateStatus(ConnectionStatus.已连接);
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
                    UpdateStatus(ConnectionStatus.断开);
                    
                    // 清理连接
                    _reader?.Dispose();
                    _writer?.Dispose();
                    _client?.Close();
                    
                    // 等待后重试连接（快速重连，确保主程序重启后能快速连上）
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        UpdateStatus(ConnectionStatus.重连中);
                        // 🔥 改为200毫秒快速重连，避免错过主程序启动时机
                        // 🔥 只在第一次失败时记录日志，避免日志刷屏
                        if (Status == ConnectionStatus.断开)
                        {
                            _onLog($"❌ 连接失败: {ex.Message}，开始快速重连...");
                        }
                        await Task.Delay(200, cancellationToken);
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
                        UpdateStatus(ConnectionStatus.断开);
                        _onLog("⚠️ 连接已断开");
                        break;
                    }
                    
                    // 🔥 移除BOM字符（UTF-8 BOM: 0xEF 0xBB 0xBF）
                    line = line.Trim('\uFEFF', '\u200B');  // \uFEFF是BOM，\u200B是零宽空格
                    
                    _onLog($"📩 收到命令: {line.Substring(0, Math.Min(50, line.Length))}...");
                    
                    // 解析命令
                    var command = JsonConvert.DeserializeObject<CommandRequest>(line);
                    if (command != null)
                    {
                        // 🔥 同步调用命令处理器，等待响应发送完成后再读取下一条
                        // 这样可以避免：
                        // 1. 读取位置错乱（响应被误读为命令）
                        // 2. 响应丢失（ReadLineAsync 吞掉了响应）
                        _onCommandReceived(command);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (System.IO.IOException ioEx) when (ioEx.InnerException is System.Net.Sockets.SocketException)
                {
                    // 连接被远程主机强制关闭，正常退出循环
                    UpdateStatus(ConnectionStatus.断开);
                    _onLog("⚠️ 连接已断开（远程主机关闭）");
                    break;
                }
                catch (System.IO.IOException ioEx)
                {
                    // 其他 IO 异常，也认为连接断开
                    UpdateStatus(ConnectionStatus.断开);
                    _onLog($"⚠️ 连接异常: {ioEx.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    _onLog($"❌ 命令处理错误: {ex.Message}");
                    // 其他异常，休息一下避免快速循环
                    await Task.Delay(100, cancellationToken);
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
                _writer.Flush();  // 🔥 立即刷新缓冲区，确保数据发送
                
                _onLog($"📤 已发送响应: {response.Message}");
            }
            catch (Exception ex)
            {
                _onLog($"❌ 发送响应失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 发送任意数据到 VxMain
        /// </summary>
        public async Task SendToVxMain(object data)
        {
            try
            {
                if (_writer == null)
                {
                    _onLog("❌ 无法发送数据：连接未建立");
                    return;
                }
                
                var json = JsonConvert.SerializeObject(data);
                await _writer.WriteLineAsync(json);
                
                _onLog($"📤 已发送数据到 VxMain");
            }
            catch (Exception ex)
            {
                _onLog($"❌ 发送数据失败: {ex.Message}");
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
