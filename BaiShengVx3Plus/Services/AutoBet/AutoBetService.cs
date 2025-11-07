using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models.AutoBet;
using SQLite;

namespace BaiShengVx3Plus.Services.AutoBet
{
    /// <summary>
    /// 自动投注服务 - 管理配置和浏览器
    /// </summary>
    public class AutoBetService : IDisposable
    {
        private SQLiteConnection? _db;
        private readonly ILogService _log;
        
        // 🔥 核心：配置ID → 浏览器客户端
        private readonly Dictionary<int, BrowserClient> _browsers = new();
        
        // Socket 服务器（双向通信：心跳、状态推送、远程控制）
        private AutoBetSocketServer? _socketServer;
        
        // HTTP 服务器（主数据交互：配置、订单、结果）
        private AutoBetHttpServer? _httpServer;
        
        // 待投注订单队列（配置ID → 订单队列）
        private readonly Dictionary<int, Queue<BetOrder>> _orderQueues = new();
        
        public AutoBetService(ILogService log)
        {
            _log = log;
            
            // 启动 Socket 服务器（端口 19527，用于双向通信）
            _socketServer = new AutoBetSocketServer(log, OnBrowserConnected);
            _socketServer.Start();
            
            // 启动 HTTP 服务器（端口 8888，用于数据交互和调试）
            _httpServer = new AutoBetHttpServer(
                log: log,
                port: 8888,
                getConfig: GetConfig,
                saveConfig: SaveConfig,
                getOrder: GetPendingOrder,
                handleResult: HandleBetResult
            );
            _httpServer.Start();
        }
        
        /// <summary>
        /// 设置数据库连接（延迟初始化）
        /// </summary>
        public void SetDatabase(SQLiteConnection db)
        {
            _db = db;
            _db.CreateTable<BetConfig>();
            _db.CreateTable<BetOrderRecord>();
            EnsureDefaultConfig();
            _log.Info("AutoBet", "✅ 数据库已设置");
        }
        
        #region 配置管理
        
        public List<BetConfig> GetConfigs()
        {
            if (_db == null) return new List<BetConfig>();
            return _db.Table<BetConfig>().OrderBy(c => c.Id).ToList();
        }
        
        public BetConfig? GetConfig(int id)
        {
            if (_db == null) return null;
            return _db.Find<BetConfig>(id);
        }
        
        public void SaveConfig(BetConfig config)
        {
            if (_db == null) return;
            
            config.LastUpdateTime = DateTime.Now;
            
            if (config.Id > 0)
                _db.Update(config);
            else
            {
                _db.Insert(config);
                config.Id = (int)_db.ExecuteScalar<long>("SELECT last_insert_rowid()");
            }
            
            _log.Info("AutoBet", $"配置已保存: {config.ConfigName}");
        }
        
        public void DeleteConfig(int id)
        {
            if (_db == null) return;
            
            var config = GetConfig(id);
            if (config != null && !config.IsDefault)
            {
                StopBrowser(id);
                
                // 删除配置
                _db.Execute("DELETE FROM AutoBetConfigs WHERE Id = ?", id);
                
                // 删除相关的投注记录（可选）
                _db.Execute("DELETE FROM BetOrderRecords WHERE ConfigId = ?", id);
                
                _log.Info("AutoBet", $"配置已删除: {config.ConfigName}");
            }
        }
        
        private void EnsureDefaultConfig()
        {
            if (_db == null) return;
            
            if (!_db.Table<BetConfig>().Any(c => c.IsDefault))
            {
                _db.Insert(new BetConfig
                {
                    ConfigName = "默认配置",
                    Platform = "YunDing28",
                    PlatformUrl = "https://www.yunding28.com",
                    IsDefault = true,
                    IsEnabled = true
                });
                _log.Info("AutoBet", "✅ 已创建默认配置");
            }
        }
        
        /// <summary>
        /// 浏览器连接回调（当浏览器通过 Socket 主动连接到 VxMain 时）
        /// </summary>
        private void OnBrowserConnected(int configId, System.Net.Sockets.TcpClient client)
        {
            try
            {
                _log.Info("AutoBet", $"🔗 浏览器已通过 Socket 连接，配置ID: {configId}");
                
                // 检查配置是否存在
                var config = GetConfig(configId);
                if (config == null)
                {
                    _log.Warning("AutoBet", $"配置不存在: {configId}");
                    return;
                }
                
                // 创建或更新 BrowserClient（使用已建立的连接）
                if (_browsers.ContainsKey(configId))
                {
                    _log.Info("AutoBet", $"更新现有浏览器连接: {config.ConfigName}");
                    _browsers[configId].Dispose();
                }
                
                var browserClient = new BrowserClient(configId);
                browserClient.AttachConnection(client); // 附加已建立的 Socket 连接
                _browsers[configId] = browserClient;
                
                // 更新配置状态
                config.Status = "已连接";
                SaveConfig(config);
                
                _log.Info("AutoBet", $"✅ 浏览器 Socket 连接成功: {config.ConfigName}");
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"处理浏览器连接失败: {configId}", ex);
            }
        }
        
        /// <summary>
        /// 通过 Socket 推送封盘通知到指定配置的浏览器
        /// </summary>
        public async Task NotifySealingAsync(int configId, string issueId, int secondsRemaining)
        {
            if (!_browsers.TryGetValue(configId, out var browserClient))
            {
                _log.Warning("AutoBet", $"浏览器未连接，无法推送封盘通知: 配置{configId}");
                return;
            }
            
            try
            {
                var data = new
                {
                    issueId = issueId,
                    secondsRemaining = secondsRemaining,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                await browserClient.SendCommandAsync("sealing_notify", data);
                _log.Info("AutoBet", $"📢 已推送封盘通知: 配置{configId} 期号{issueId} 剩余{secondsRemaining}秒");
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"推送封盘通知失败: 配置{configId}", ex);
            }
        }
        
        /// <summary>
        /// 通过 Socket 推送投注命令到指定配置的浏览器
        /// </summary>
        public async Task SendBetCommandAsync(int configId, BetOrder order)
        {
            if (!_browsers.TryGetValue(configId, out var browserClient))
            {
                _log.Warning("AutoBet", $"浏览器未连接，无法推送投注命令: 配置{configId}");
                return;
            }
            
            try
            {
                var data = new
                {
                    order.IssueId,
                    order.PlayType,
                    order.BetContent,
                    order.Amount
                };
                
                await browserClient.SendCommandAsync("place_bet", data);
                _log.Info("AutoBet", $"📤 已推送投注命令: 配置{configId} {order.IssueId} {order.BetContent} {order.Amount}元");
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"推送投注命令失败: 配置{configId}", ex);
            }
        }
        
        /// <summary>
        /// 添加订单到队列（供 HTTP 接口查询）
        /// </summary>
        public void QueueBetOrder(int configId, BetOrder order)
        {
            lock (_orderQueues)
            {
                if (!_orderQueues.ContainsKey(configId))
                {
                    _orderQueues[configId] = new Queue<BetOrder>();
                }
                
                _orderQueues[configId].Enqueue(order);
                _log.Info("AutoBet", $"📝 订单已加入队列: 配置{configId} {order.IssueId} {order.BetContent} {order.Amount}元");
            }
        }
        
        /// <summary>
        /// 获取待处理订单（HTTP API 调用）
        /// </summary>
        public BetOrder? GetPendingOrder(int configId, string? issueId)
        {
            lock (_orderQueues)
            {
                if (!_orderQueues.TryGetValue(configId, out var queue) || queue.Count == 0)
                {
                    return null;
                }
                
                // 如果指定了期号，查找对应期号的订单
                if (!string.IsNullOrEmpty(issueId))
                {
                    return queue.FirstOrDefault(o => o.IssueId == issueId);
                }
                
                // 否则返回队首订单
                return queue.Peek();
            }
        }
        
        /// <summary>
        /// 处理投注结果（HTTP API 回调）
        /// </summary>
        public void HandleBetResult(int configId, bool success, string? orderId, string? errorMessage)
        {
            try
            {
                var config = GetConfig(configId);
                if (config == null)
                {
                    _log.Warning("AutoBet", $"配置不存在: {configId}");
                    return;
                }
                
                // 从队列移除已处理的订单
                BetOrder? order = null;
                lock (_orderQueues)
                {
                    if (_orderQueues.TryGetValue(configId, out var queue) && queue.Count > 0)
                    {
                        order = queue.Dequeue();
                    }
                }
                
                if (order == null)
                {
                    _log.Warning("AutoBet", $"未找到对应订单: 配置{configId}");
                    return;
                }
                
                // 记录到数据库
                if (_db != null)
                {
                    _db.Insert(new BetOrderRecord
                    {
                        ConfigId = configId,
                        ConfigName = config.ConfigName,
                        Platform = config.Platform,
                        IssueId = order.IssueId ?? "",
                        PlayType = order.PlayType,
                        BetContent = order.BetContent,
                        Amount = order.Amount,
                        PlatformOrderId = orderId,
                        Status = success ? "成功" : "失败",
                        ErrorMessage = errorMessage,
                        CreateTime = DateTime.Now
                    });
                }
                
                _log.Info("AutoBet", $"📥 [{config.ConfigName}] 投注结果: {(success ? "✅ 成功" : "❌ 失败")} 订单号:{orderId}");
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"处理投注结果失败: 配置{configId}", ex);
            }
        }
        
        #endregion
        
        #region 浏览器控制
        
        /// <summary>
        /// 启动浏览器
        /// </summary>
        public async Task<bool> StartBrowser(int configId)
        {
            try
            {
                var config = GetConfig(configId);
                if (config == null)
                {
                    _log.Error("AutoBet", $"配置不存在: {configId}");
                    return false;
                }
                
                // 检查是否已有浏览器实例
                if (_browsers.TryGetValue(configId, out var existingBrowserClient))
                {
                    // 检查进程和连接状态
                    var (isAlive, processId) = await existingBrowserClient.PingAsync();
                    
                    if (isAlive && existingBrowserClient.IsProcessRunning)
                    {
                        // 浏览器在线且进程运行中，显示窗口
                        _log.Info("AutoBet", $"浏览器已运行（PID: {processId}），显示窗口: {config.ConfigName}");
                        await existingBrowserClient.ShowWindowAsync();
                        return true;
                    }
                else if (existingBrowserClient.IsProcessRunning)
                {
                    // 进程运行但连接断开，显示窗口并等待 Socket 服务器接收浏览器连接
                    _log.Warning("AutoBet", $"浏览器进程运行但连接断开，显示窗口并等待连接: {config.ConfigName}");
                    await existingBrowserClient.ShowWindowAsync();
                    return true;
                }
                    
                    // 进程已退出或无法恢复，清理并重启
                    _log.Warning("AutoBet", $"浏览器进程已退出，重新启动: {config.ConfigName}");
                    existingBrowserClient.Dispose();
                    _browsers.Remove(configId);
                }
                
                _log.Info("AutoBet", $"🚀 启动浏览器: {config.ConfigName}");
                
                // 创建浏览器客户端（Socket 服务器使用固定端口 19527）
                var browserClient = new BrowserClient(configId);
                await browserClient.StartAsync(0, config.Platform, config.PlatformUrl); // 端口参数不再使用，传 0
                
                _browsers[configId] = browserClient;
                
                // 更新状态
                config.Status = "已启动";
                SaveConfig(config);
                
                // 自动登录
                if (config.AutoLogin && !string.IsNullOrEmpty(config.Username))
                {
                    var loginResult = await browserClient.SendCommandAsync("Login", new
                    {
                        username = config.Username,
                        password = config.Password
                    });
                    
                    config.Status = loginResult.Success ? "已登录" : "登录失败";
                    SaveConfig(config);
                }
                
                _log.Info("AutoBet", $"✅ 浏览器启动成功: {config.ConfigName}");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"启动浏览器失败: {configId}", ex);
                return false;
            }
        }
        
        /// <summary>
        /// 投注
        /// </summary>
        public async Task<BetResult> PlaceBet(int configId, BetOrder order)
        {
            if (!_browsers.TryGetValue(configId, out var browserClient))
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "浏览器未启动"
                };
            }
            
            var config = GetConfig(configId);
            if (config == null)
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "配置不存在"
                };
            }
            
            _log.Info("AutoBet", $"📤 [{config.ConfigName}] 投注: {order.PlayType} {order.BetContent} {order.Amount}");
            
            // 发送投注命令
            var result = await browserClient.SendCommandAsync("PlaceBet", order);
            
            // 保存订单记录
            if (_db != null)
            {
                _db.Insert(new BetOrderRecord
                {
                    ConfigId = configId,
                    ConfigName = config.ConfigName,
                    Platform = config.Platform,
                    IssueId = order.IssueId ?? "",
                    PlayType = order.PlayType,
                    BetContent = order.BetContent,
                    Amount = order.Amount,
                    PlatformOrderId = result.OrderId,
                    Status = result.Success ? "成功" : "失败",
                    ErrorMessage = result.ErrorMessage,
                    CreateTime = DateTime.Now
                });
            }
            
            _log.Info("AutoBet", $"📥 [{config.ConfigName}] 投注结果: {(result.Success ? "✅ 成功" : "❌ 失败")}");
            
            return result;
        }
        
        /// <summary>
        /// 停止浏览器
        /// </summary>
        public void StopBrowser(int configId)
        {
            if (_browsers.TryGetValue(configId, out var browserClient))
            {
                browserClient.Dispose();
                _browsers.Remove(configId);
                
                var config = GetConfig(configId);
                if (config != null)
                {
                    config.Status = "已停止";
                    SaveConfig(config);
                }
                
                _log.Info("AutoBet", $"⏹️ 浏览器已停止: {config?.ConfigName}");
            }
        }
        
        /// <summary>
        /// 停止所有浏览器
        /// </summary>
        public void StopAllBrowsers()
        {
            foreach (var configId in _browsers.Keys.ToList())
            {
                StopBrowser(configId);
            }
        }
        
        #endregion
        
        public void Dispose()
        {
            StopAllBrowsers();
            _socketServer?.Dispose();
            _httpServer?.Dispose();
            _log.Info("AutoBet", "AutoBetService 已释放");
        }
    }
}
