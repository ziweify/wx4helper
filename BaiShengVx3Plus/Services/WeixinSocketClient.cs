using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Services
{
    /// <summary>
    /// 微信 Socket 客户端实现
    /// </summary>
    public class WeixinSocketClient : IWeixinSocketClient
    {
        private readonly ILogService _logService;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;
        private Task? _receiveTask;
        
        private int _requestId = 0;
        private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> _pendingRequests = new();

        public bool IsConnected => _client?.Connected ?? false;

        public event EventHandler<ServerPushEventArgs>? OnServerPush;

        public WeixinSocketClient(ILogService logService)
        {
            _logService = logService;
        }

        public async Task<bool> ConnectAsync(string host = "127.0.0.1", int port = 6328, int timeoutMs = 5000)
        {
            try
            {
                if (IsConnected)
                {
                    _logService.Warning("WeixinSocketClient", "Already connected");
                    return true;
                }

                _logService.Info("WeixinSocketClient", $"Connecting to {host}:{port}...");

                _client = new TcpClient();
                
                using var cts = new CancellationTokenSource(timeoutMs);
                await _client.ConnectAsync(host, port, cts.Token);
                
                _stream = _client.GetStream();
                _cts = new CancellationTokenSource();
                _receiveTask = Task.Run(() => ReceiveLoop(_cts.Token), _cts.Token);

                _logService.Info("WeixinSocketClient", "Connected successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logService.Error("WeixinSocketClient", "Failed to connect", ex);
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                _logService.Info("WeixinSocketClient", "Disconnecting...");

                _cts?.Cancel();
                _stream?.Close();
                _client?.Close();

                // 清理所有待处理的请求
                foreach (var kvp in _pendingRequests)
                {
                    kvp.Value.TrySetCanceled();
                }
                _pendingRequests.Clear();

                _logService.Info("WeixinSocketClient", "Disconnected");
            }
            catch (Exception ex)
            {
                _logService.Error("WeixinSocketClient", "Error during disconnect", ex);
            }
        }

        public async Task<TResult?> SendAsync<TResult>(string method, params object[] parameters) where TResult : class
        {
            return await SendAsync<TResult>(method, 10000, parameters);
        }

        public async Task<TResult?> SendAsync<TResult>(string method, int timeoutMs, params object[] parameters) where TResult : class
        {
            if (!IsConnected || _stream == null)
            {
                _logService.Error("WeixinSocketClient", "Not connected");
                return null;
            }

            int id = Interlocked.Increment(ref _requestId);
            var tcs = new TaskCompletionSource<string>();
            _pendingRequests[id] = tcs;

            try
            {
                // 构建请求
                var request = new
                {
                    id,
                    method,
                    @params = parameters
                };

                string json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                _logService.Debug("WeixinSocketClient", $"Sending: {json}");

                // 发送消息（4字节长度 + 消息体）
                byte[] messageBytes = Encoding.UTF8.GetBytes(json);
                byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lengthBytes); // 转换为网络字节序（大端）
                }

                await _stream.WriteAsync(lengthBytes, 0, 4);
                await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                await _stream.FlushAsync();

                // 等待响应（带超时）
                using var cts = new CancellationTokenSource(timeoutMs);
                var timeoutTask = Task.Delay(timeoutMs, cts.Token);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    _logService.Error("WeixinSocketClient", $"Request timeout: {method}");
                    return null;
                }

                string responseJson = await tcs.Task;
                _logService.Debug("WeixinSocketClient", $"Received: {responseJson}");

                // 解析响应
                var response = JsonSerializer.Deserialize<JsonElement>(responseJson);
                if (response.TryGetProperty("error", out var errorElement) && errorElement.ValueKind != JsonValueKind.Null)
                {
                    string error = errorElement.GetString() ?? "Unknown error";
                    _logService.Error("WeixinSocketClient", $"Server error: {error}");
                    return null;
                }

                if (response.TryGetProperty("result", out var resultElement))
                {
                    string resultJson = resultElement.GetRawText();
                    return JsonSerializer.Deserialize<TResult>(resultJson);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logService.Error("WeixinSocketClient", $"SendAsync failed: {method}", ex);
                return null;
            }
            finally
            {
                _pendingRequests.TryRemove(id, out _);
            }
        }

        private async Task ReceiveLoop(CancellationToken cancellationToken)
        {
            _logService.Info("WeixinSocketClient", "Receive loop started");

            try
            {
                while (!cancellationToken.IsCancellationRequested && IsConnected && _stream != null)
                {
                    // 读取消息长度（4字节）
                    byte[] lengthBytes = new byte[4];
                    int bytesRead = await ReadExactAsync(_stream, lengthBytes, 4, cancellationToken);
                    if (bytesRead != 4)
                    {
                        _logService.Warning("WeixinSocketClient", "Failed to read message length");
                        break;
                    }

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(lengthBytes); // 转换为主机字节序
                    }
                    int length = BitConverter.ToInt32(lengthBytes, 0);

                    if (length <= 0 || length > 10 * 1024 * 1024) // 限制最大10MB
                    {
                        _logService.Error("WeixinSocketClient", $"Invalid message length: {length}");
                        break;
                    }

                    // 读取消息体
                    byte[] messageBytes = new byte[length];
                    bytesRead = await ReadExactAsync(_stream, messageBytes, length, cancellationToken);
                    if (bytesRead != length)
                    {
                        _logService.Warning("WeixinSocketClient", "Failed to read message body");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(messageBytes);
                    ProcessMessage(message);
                }
            }
            catch (OperationCanceledException)
            {
                _logService.Info("WeixinSocketClient", "Receive loop cancelled");
            }
            catch (Exception ex)
            {
                _logService.Error("WeixinSocketClient", "Receive loop error", ex);
            }
            finally
            {
                _logService.Info("WeixinSocketClient", "Receive loop stopped");
                Disconnect();
            }
        }

        private async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int count, CancellationToken cancellationToken)
        {
            int totalRead = 0;
            while (totalRead < count && !cancellationToken.IsCancellationRequested)
            {
                int read = await stream.ReadAsync(buffer, totalRead, count - totalRead, cancellationToken);
                if (read == 0)
                {
                    return totalRead; // 连接关闭
                }
                totalRead += read;
            }
            return totalRead;
        }

        private void ProcessMessage(string message)
        {
            try
            {
                var json = JsonSerializer.Deserialize<JsonElement>(message);

                // 判断是响应还是推送
                if (json.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.Number)
                {
                    // 响应消息
                    int id = idElement.GetInt32();
                    if (_pendingRequests.TryRemove(id, out var tcs))
                    {
                        tcs.TrySetResult(message);
                    }
                }
                else if (json.TryGetProperty("method", out var methodElement))
                {
                    // 服务器推送
                    string method = methodElement.GetString() ?? "";
                    object? data = null;
                    
                    if (json.TryGetProperty("params", out var paramsElement))
                    {
                        data = JsonSerializer.Deserialize<object>(paramsElement.GetRawText());
                    }

                    _logService.Info("WeixinSocketClient", $"Server push: {method}");
                    OnServerPush?.Invoke(this, new ServerPushEventArgs { Method = method, Data = data });
                }
            }
            catch (Exception ex)
            {
                _logService.Error("WeixinSocketClient", "Failed to process message", ex);
            }
        }

        public void Dispose()
        {
            Disconnect();
            _cts?.Dispose();
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}

