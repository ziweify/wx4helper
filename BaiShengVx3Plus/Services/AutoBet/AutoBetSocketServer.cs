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
        private readonly Action<int, TcpClient> _onBrowserConnected;
        private readonly Action<int, JObject>? _onMessageReceived; // 🔥 新增消息处理回调
        
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _listenerTask;
        private readonly Dictionary<int, ClientConnection> _connections = new();
        
        public bool IsRunning { get; private set; }
        
        public AutoBetSocketServer(
            ILogService log, 
            Action<int, TcpClient> onBrowserConnected,
            Action<int, JObject>? onMessageReceived = null) // 🔥 新增参数
        {
            _log = log;
            _onBrowserConnected = onBrowserConnected;
            _onMessageReceived = onMessageReceived; // 🔥 保存回调
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
                if (configId <= 0)
                {
                    _log.Warning("AutoBetServer", "握手失败：配置ID无效");
                    return;
                }
                
                _log.Info("AutoBetServer", $"✅ 浏览器握手成功，配置ID: {configId}");
                
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
                
                // 4. 通知 AutoBetService 有新连接
                _onBrowserConnected(configId, client);
                
                // 5. 持续读取命令（保持连接）
                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (string.IsNullOrEmpty(line))
                    {
                        _log.Warning("AutoBetServer", $"配置 {configId} 连接已断开");
                        break;
                    }
                    
                    _log.Info("AutoBetServer", $"📩 [{configId}] {line}");
                    
                    // 🔥 解析并处理消息
                    try
                    {
                        var message = JsonConvert.DeserializeObject<JObject>(line);
                        if (message != null)
                        {
                            var messageType = message["type"]?.ToString();
                            
                            // 分发消息给处理器
                            switch (messageType)
                            {
                                case "cookie_update":
                                    _log.Info("AutoBetServer", $"🍪 收到Cookie更新:配置{configId}");
                                    _onMessageReceived?.Invoke(configId, message);
                                    break;
                                    
                                case "login_success":
                                    _log.Info("AutoBetServer", $"✅ 收到登录成功通知:配置{configId}");
                                    _onMessageReceived?.Invoke(configId, message);
                                    break;
                                    
                                default:
                                    _log.Info("AutoBetServer", $"📨 收到消息:类型={messageType}");
                                    _onMessageReceived?.Invoke(configId, message);
                                    break;
                            }
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
                }
                
                reader?.Dispose();
                writer?.Dispose();
                client.Close();
            }
        }
        
        /// <summary>
        /// 获取指定配置的连接
        /// </summary>
        public ClientConnection? GetConnection(int configId)
        {
            lock (_connections)
            {
                return _connections.TryGetValue(configId, out var conn) ? conn : null;
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
            
            public bool IsConnected => Client?.Connected ?? false;
            
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

