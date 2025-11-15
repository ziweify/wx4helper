using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BaiShengVx3Plus.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaiShengVx3Plus.Services.AutoBet
{
    /// <summary>
    /// 自动投注 Socket 服务器 - 接收浏览器客户端的主动连接
    /// </summary>
    public class AutoBetSocketServer : IDisposable
    {
        private const int SERVER_PORT = 19527; // VxMain 监听的固定端口
        
        private readonly ILogService _log;
        private readonly Action<string, int, int> _onBrowserConnected;  // 🔥 改为 (string configName, int configId, int processId)
        private readonly Action<int, JObject>? _onMessageReceived; // 🔥 新增消息处理回调
        private readonly Action<int>? _onBrowserDisconnected; // 🔥 新增连接断开回调
        
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _listenerTask;
        private readonly Dictionary<int, ClientConnection> _connections = new();
        
        public bool IsRunning { get; private set; }
        
        public AutoBetSocketServer(
            ILogService log, 
            Action<string, int, int> onBrowserConnected,  // 🔥 改为 (string configName, int configId, int processId)
            Action<int, JObject>? onMessageReceived = null, // 🔥 新增参数
            Action<int>? onBrowserDisconnected = null) // 🔥 新增连接断开回调
        {
            _log = log;
            _onBrowserConnected = onBrowserConnected;
            _onMessageReceived = onMessageReceived; // 🔥 保存回调
            _onBrowserDisconnected = onBrowserDisconnected; // 🔥 保存连接断开回调
        }
        
        /// <summary>
        /// 🔥 获取指定配置的连接（供 BrowserClient 使用）
        /// </summary>
        public ClientConnection? GetConnection(int configId)
        {
            lock (_connections)
            {
                return _connections.TryGetValue(configId, out var conn) ? conn : null;
            }
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
                _listener = new TcpListener(IPAddress.Loopback, SERVER_PORT);
                _listener.Start();
                
                IsRunning = true;
                _log.Info("AutoBetServer", $"✅ Socket 服务器已启动，端口: {SERVER_PORT}");
                
                _listenerTask = Task.Run(() => ListenAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                _log.Error("AutoBetServer", $"Socket 服务器启动失败", ex);
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
            
            // 关闭所有连接
            lock (_connections)
            {
                foreach (var conn in _connections.Values)
                {
                    conn.Dispose();
                }
                _connections.Clear();
            }
            
            _log.Info("AutoBetServer", "⏹️ Socket 服务器已停止");
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
                    
                    _log.Info("AutoBetServer", "⏳ 等待浏览器连接...");
                    
                    // 接受连接
                    var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                    _log.Info("AutoBetServer", $"✅ 浏览器已连接: {client.Client.RemoteEndPoint}");
                    
                    // 启动新任务处理此连接
                    _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _log.Error("AutoBetServer", $"监听错误", ex);
                    await Task.Delay(1000, cancellationToken); // 延迟后重试
                }
            }
        }
        
        /// <summary>
        /// 处理客户端连接
        /// </summary>
        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            StreamReader? reader = null;
            StreamWriter? writer = null;
            int configId = -1;
            
            try
            {
                var stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                
                // 1. 接收浏览器的握手消息（包含配置ID）
                var handshakeLine = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(handshakeLine))
                {
                    _log.Warning("AutoBetServer", "握手失败：未收到数据");
                    return;
                }
                
                _log.Info("AutoBetServer", $"📩 收到握手: {handshakeLine}");
                
                var handshake = JsonConvert.DeserializeObject<JObject>(handshakeLine);
                if (handshake == null || handshake["type"]?.ToString() != "hello")
                {
                    _log.Warning("AutoBetServer", "握手失败：消息格式错误");
                    return;
                }
                
                configId = handshake["configId"]?.ToObject<int>() ?? -1;
                var configName = handshake["configName"]?.ToString() ?? "";  // 🔥 解析配置名
                var processId = handshake["processId"]?.ToObject<int>() ?? 0;  // 🔥 解析进程ID
                
                // 🔥 配置名是必须的，用于匹配配置
                if (string.IsNullOrEmpty(configName))
                {
                    _log.Warning("AutoBetServer", "握手失败：配置名为空");
                    return;
                }
                
                _log.Info("AutoBetServer", $"✅ 浏览器握手成功");
                _log.Info("AutoBetServer", $"   配置ID: {configId}");
                _log.Info("AutoBetServer", $"   配置名: {configName}");
                _log.Info("AutoBetServer", $"   进程ID: {processId}");
                
                // 2. 发送确认消息
                var response = new
                {
                    type = "welcome",
                    success = true,
                    message = "连接成功"
                };
                await writer.WriteLineAsync(JsonConvert.SerializeObject(response));
                
                // 3. 保存连接
                var connection = new ClientConnection
                {
                    ConfigId = configId,
                    Client = client,
                    Reader = reader,
                    Writer = writer
                };
                
                lock (_connections)
                {
                    if (_connections.ContainsKey(configId))
                    {
                        _log.Warning("AutoBetServer", $"配置ID {configId} 已存在连接，关闭旧连接");
                        _connections[configId].Dispose();
                    }
                    _connections[configId] = connection;
                }
                
                // 4. 🔥 通知 AutoBetService 有新连接（传递配置名、配置ID 和 进程ID）
                _onBrowserConnected(configName, configId, processId);
                
                // 5. 持续读取消息（包括主动通知和命令响应）
                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (string.IsNullOrEmpty(line))
                    {
                        _log.Warning("AutoBetServer", $"配置 {configId} 连接已断开");
                        break;
                    }
                    
                    _log.Info("AutoBetServer", $"📩 [{configId}] {line.Substring(0, Math.Min(100, line.Length))}...");
                    
                    // 🔥 解析并分发所有消息
                    try
                    {
                        var message = JsonConvert.DeserializeObject<JObject>(line);
                        if (message != null)
                        {
                            // 所有消息都通过回调分发
                            _onMessageReceived?.Invoke(configId, message);
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _log.Error("AutoBetServer", "解析消息失败", parseEx);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                _log.Error("AutoBetServer", $"处理客户端连接失败 (配置ID: {configId})", ex);
            }
            finally
            {
                // 清理连接
                if (configId > 0)
                {
                    lock (_connections)
                    {
                        if (_connections.TryGetValue(configId, out var conn))
                        {
                            conn.Dispose();
                            _connections.Remove(configId);
                        }
                    }
                    _log.Info("AutoBetServer", $"❌ 配置 {configId} 连接已关闭");
                    
                    // 🔥 通知连接断开（事件驱动）
                    _onBrowserDisconnected?.Invoke(configId);
                }
                
                reader?.Dispose();
                writer?.Dispose();
                client.Close();
            }
        }
        
        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
            _listenerTask?.Dispose();
        }
        
        /// <summary>
        /// 客户端连接封装
        /// </summary>
        public class ClientConnection : IDisposable
        {
            public int ConfigId { get; set; }
            public TcpClient Client { get; set; } = null!;
            public StreamReader Reader { get; set; } = null!;
            public StreamWriter Writer { get; set; } = null!;
            
            /// <summary>
            /// 🔥 可靠的连接状态检测
            /// TcpClient.Connected 不可靠，必须使用 Socket.Poll 检测
            /// </summary>
            public bool IsConnected
            {
                get
                {
                    try
                    {
                        if (Client == null || Client.Client == null)
                            return false;
                        
                        // 🔥 使用 Socket.Poll 进行可靠的连接检测
                        // Poll(1, SelectMode.SelectRead) 检查是否有可读数据
                        // Available == 0 表示连接已关闭（有可读事件但无数据）
                        if (Client.Client.Poll(1, SelectMode.SelectRead) && Client.Client.Available == 0)
                            return false; // 连接已关闭
                        
                        return Client.Connected;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            
            /// <summary>
            /// 发送命令到浏览器
            /// </summary>
            public async Task<bool> SendCommandAsync(string command, object? data = null)
            {
                try
                {
                    if (!IsConnected) return false;
                    
                    var request = new
                    {
                        command = command,
                        data = data
                    };
                    
                    var json = JsonConvert.SerializeObject(request);
                    await Writer.WriteLineAsync(json);
                    
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            
            public void Dispose()
            {
                Reader?.Dispose();
                Writer?.Dispose();
                Client?.Close();
            }
        }
    }
}

