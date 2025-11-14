using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Games;
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
        private IBinggoOrderService? _orderService;
        
        // 🔥 核心：配置ID → 浏览器客户端
        private readonly Dictionary<int, BrowserClient> _browsers = new();
        
        // Socket 服务器（双向通信：心跳、状态推送、远程控制）
        private AutoBetSocketServer? _socketServer;
        
        // HTTP 服务器（主数据交互：配置、订单、结果）
        private AutoBetHttpServer? _httpServer;
        
        // 待投注订单队列（配置ID → 订单队列）
        private readonly Dictionary<int, Queue<BetOrder>> _orderQueues = new();
        
        // 🔥 配置列表（内存管理，自动保存）- 参考 V2MemberBindingList
        private Core.BetConfigBindingList? _configs;
        
        // 🔥 后台监控任务：自动启动浏览器（如果配置需要但未连接）
        private System.Threading.Timer? _monitorTimer;
        private readonly object _lock = new object();
        
        // 🔥 记录正在启动的配置（防止重复启动）
        private readonly HashSet<int> _startingConfigs = new();
        
        public AutoBetService(ILogService log, IBinggoOrderService orderService)
        {
            _log = log;
            _orderService = orderService;
            
            _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _log.Info("AutoBet", "🚀 AutoBetService 构造函数执行");
            
            // 启动 Socket 服务器（端口 19527，用于双向通信）
            _socketServer = new AutoBetSocketServer(log, OnBrowserConnected, OnMessageReceived); // 🔥 添加消息处理回调
            _socketServer.Start();
            
            // 🔥 监控任务暂不启动，等待 SetDatabase 完成后再启动
            _log.Info("AutoBet", "⏸️ 后台监控任务暂未启动（等待数据库初始化）");
            
            _log.Info("AutoBet", "✅ AutoBetService 初始化完成");
            _log.Info("AutoBet", $"   Socket 服务器状态: {(_socketServer.IsRunning ? "运行中" : "未运行")}");
            _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            // 启动 HTTP 服务器（端口 8888，用于数据交互和调试）
            _httpServer = new AutoBetHttpServer(
                log: log,
                port: 8888,
                getConfig: GetConfig,
                saveConfig: SaveConfig,
                orderService: orderService,
                handleResult: HandleBetResult
            );
            _httpServer.Start();
        }
        
        /// <summary>
        /// 设置数据库连接（延迟初始化）
        /// 🔥 从数据库加载配置到内存（仅加载一次）
        /// </summary>
        public void SetDatabase(SQLiteConnection db)
        {
            _db = db;
            _db.CreateTable<BetConfig>();
            _db.CreateTable<BetOrderRecord>();
            
            // 🔥 创建配置 BindingList 并加载数据到内存
            _configs = new Core.BetConfigBindingList(_db);
            _configs.LoadFromDatabase();
            
            EnsureDefaultConfig();
            _log.Info("AutoBet", $"✅ 数据库已设置，已加载 {_configs.Count} 个配置到内存");
            
            // 🔥 配置加载完成后，再启动监控任务
            _monitorTimer = new System.Threading.Timer(MonitorBrowsers, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
            _log.Info("AutoBet", "✅ 后台监控任务已启动（每3秒检查一次，首次延迟3秒）");
            _log.Info("AutoBet", "   说明：首次延迟3秒，给UI有时间完成初始化和事件绑定");
        }
        
        #region 配置管理（从内存读取，不访问数据库）
        
        /// <summary>
        /// 获取所有配置（从内存读取）
        /// 🔥 监控任务调用此方法，不访问数据库
        /// </summary>
        public List<BetConfig> GetConfigs()
        {
            if (_configs == null) return new List<BetConfig>();
            return _configs.ToList();
        }
        
        /// <summary>
        /// 获取指定配置（从内存读取）
        /// </summary>
        public BetConfig? GetConfig(int id)
        {
            if (_configs == null) return null;
            return _configs.FirstOrDefault(c => c.Id == id);
        }
        
        /// <summary>
        /// 保存配置（自动保存到数据库）
        /// 🔥 如果是新配置，添加到 BindingList（自动保存）
        /// 🔥 如果是修改配置，直接修改对象（PropertyChanged 自动保存）
        /// </summary>
        public void SaveConfig(BetConfig config)
        {
            if (_configs == null) return;
            
            if (config.Id == 0)
            {
                // 🔥 新配置：添加到 BindingList（自动保存到数据库）
                _configs.Add(config);
                _log.Info("AutoBet", $"配置已添加: {config.ConfigName}");
            }
            else
            {
                // 🔥 修改现有配置：直接修改对象属性即可
                // PropertyChanged 事件会自动保存到数据库
                _log.Info("AutoBet", $"配置已更新: {config.ConfigName}");
            }
        }
        
        /// <summary>
        /// 删除配置（从内存和数据库删除）
        /// </summary>
        public void DeleteConfig(int id)
        {
            if (_configs == null) return;
            
            var config = GetConfig(id);
            if (config != null && !config.IsDefault)
            {
                StopBrowser(id);
                
                // 🔥 从 BindingList 移除（自动从数据库删除）
                _configs.Remove(config);
                
                // 删除相关的投注记录（可选）
                if (_db != null)
                {
                    _db.Execute("DELETE FROM BetRecord WHERE ConfigId = ?", id);
                }
                
                _log.Info("AutoBet", $"配置已删除: {config.ConfigName}");
            }
        }
        
        private void EnsureDefaultConfig()
        {
            if (_configs == null) return;
            
            var defaultConfig = _configs.FirstOrDefault(c => c.IsDefault);
            
            if (defaultConfig == null)
            {
                // 🔥 不存在默认配置，创建新的
                var newConfig = new BetConfig
                {
                    ConfigName = "默认配置",
                    Platform = "通宝",
                    PlatformUrl = "https://yb666.fr.win2000.cc",
                    IsDefault = true,
                    IsEnabled = false  // 🔥 默认不启用，由用户手动开启
                };
                _configs.Add(newConfig);  // 自动保存到数据库
                _log.Info("AutoBet", "✅ 已创建默认配置（通宝平台）");
            }
            else
            {
                // 🔥 程序启动时，强制将所有配置的 IsEnabled 设置为 false
                // 避免上次异常退出时，配置状态遗留为 true，导致启动时自动启动浏览器
                _log.Info("AutoBet", $"检查默认配置 IsEnabled 状态: {defaultConfig.IsEnabled}");
                if (defaultConfig.IsEnabled)
                {
                    _log.Warning("AutoBet", "⚠️ 检测到默认配置 IsEnabled=true（可能是上次异常退出遗留）");
                    _log.Warning("AutoBet", "   强制设置为 false，避免启动时自动启动浏览器");
                    defaultConfig.IsEnabled = false;  // PropertyChanged 自动保存
                }
                
                // 🔥 检查并修复平台和URL的匹配
                _log.Info("AutoBet", $"检查默认配置: 平台={defaultConfig.Platform}, URL={defaultConfig.PlatformUrl}");
                
                bool needUpdate = false;
                string correctUrl = GetCorrectPlatformUrl(defaultConfig.Platform);
                
                // 如果URL不匹配平台，自动修正
                if (!string.IsNullOrEmpty(correctUrl) && defaultConfig.PlatformUrl != correctUrl)
                {
                    _log.Warning("AutoBet", $"⚠️ 检测到平台URL不匹配:");
                    _log.Warning("AutoBet", $"   平台: {defaultConfig.Platform}");
                    _log.Warning("AutoBet", $"   当前URL: {defaultConfig.PlatformUrl}");
                    _log.Warning("AutoBet", $"   正确URL: {correctUrl}");
                    
                    defaultConfig.PlatformUrl = correctUrl;  // 🔥 直接修改，PropertyChanged 自动保存
                    needUpdate = true;
                }
                
                // 兼容旧的平台名称（YunDing28 → 云顶）
                if (defaultConfig.Platform == "YunDing28")
                {
                    defaultConfig.Platform = "云顶";
                    defaultConfig.PlatformUrl = "https://www.yunding28.com";
                    needUpdate = true;
                    _log.Warning("AutoBet", "检测到旧的平台名称YunDing28，已更新为'云顶'");
                }
                
                if (needUpdate)
                {
                    // 🔥 无需手动调用 Update，PropertyChanged 自动保存
                    _log.Info("AutoBet", $"✅ 已修复默认配置: {defaultConfig.Platform} - {defaultConfig.PlatformUrl}（已自动保存到数据库）");
                }
                else
                {
                    _log.Info("AutoBet", $"✅ 默认配置正确: {defaultConfig.Platform} - {defaultConfig.PlatformUrl}");
                }
            }
        }
        
        /// <summary>
        /// 根据平台名称获取正确的URL
        /// </summary>
        private string GetCorrectPlatformUrl(string platform)
        {
            return platform switch
            {
                "通宝" or "TongBao" => "https://yb666.fr.win2000.cc",
                "云顶" or "YunDing" or "YunDing28" => "https://www.yunding28.com",
                "海峡" or "HaiXia" => "https://www.haixia28.com",
                "红海" or "HongHai" => "https://www.honghai28.com",
                _ => ""
            };
        }
        
        /// <summary>
        /// 🔥 浏览器连接回调（当浏览器通过 Socket 主动连接到 VxMain 时）
        /// 根据配置名匹配配置（而不是配置ID），解决数据库重建后ID变化的问题
        /// </summary>
        private void OnBrowserConnected(string configName, int browserConfigId)
        {
            try
            {
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _log.Info("AutoBet", $"🔗 浏览器已通过 Socket 连接，配置名: {configName}");
                _log.Info("AutoBet", $"   浏览器ConfigId: {browserConfigId}");
                _log.Info("AutoBet", $"   当前 _browsers 字典: [{string.Join(", ", _browsers.Keys)}]");
                
                // 🔥 根据配置名查找配置（而不是配置ID）
                Models.AutoBet.BetConfig? config;
                lock (_lock)
                {
                    config = _configs.FirstOrDefault(c => c.ConfigName == configName);
                }
                
                if (config == null)
                {
                    _log.Error("AutoBet", $"❌ 配置不存在: {configName}，拒绝连接");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return;
                }
                
                int configId = config.Id;
                _log.Info("AutoBet", $"✅ 配置信息: {config.ConfigName} (Id={configId}, {config.Platform})");
                _log.Info("AutoBet", $"   说明：配置名固定，但数据库重建后配置ID可能变化");
                
                // 🔥 从 AutoBetSocketServer 获取 ClientConnection
                var connection = _socketServer.GetConnection(browserConfigId);
                if (connection == null)
                {
                    _log.Error("AutoBet", $"❌ 无法获取 ClientConnection: {browserConfigId}");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return;
                }
                
                _log.Info("AutoBet", $"✅ 已获取 ClientConnection，连接状态: {connection.IsConnected}");
                
                // 🔥 创建或更新 BrowserClient（使用 ClientConnection）
                if (_browsers.TryGetValue(configId, out var existingBrowser))
                {
                    _log.Info("AutoBet", $"📌 _browsers 字典中已存在该配置，更新连接");
                    // ✅ 附加 ClientConnection（不是 TcpClient）
                    existingBrowser.AttachConnection(connection);
                }
                else
                {
                    // 🔥 主程序重启或数据库重建场景：_browsers 字典为空，但浏览器在运行并重连了
                    _log.Info("AutoBet", $"📌 _browsers 字典中无此配置，自动创建 BrowserClient");
                    _log.Info("AutoBet", $"   场景：主程序重启、数据库重建、或浏览器先于主程序启动");
                    
                    var browserClient = new BrowserClient(configId);
                    browserClient.AttachConnection(connection); // 🔥 附加 ClientConnection
                    
                    lock (_lock)
                    {
                        _browsers[configId] = browserClient;
                    }
                    
                    _log.Info("AutoBet", $"✅ BrowserClient 已创建并附加连接");
                }
                
                // 更新配置状态
                config.Status = "已连接";
                SaveConfig(config);
                
                _log.Info("AutoBet", $"✅ 浏览器 Socket 连接处理完成: {config.ConfigName}");
                _log.Info("AutoBet", $"   更新后 _browsers 字典: [{string.Join(", ", _browsers.Keys)}]");
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"❌ 处理浏览器连接失败: {configName}", ex);
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
        }
        
        /// <summary>
        /// 🔥 消息接收回调（当浏览器通过Socket主动发送消息时）
        /// 包括：命令响应、Cookie更新、登录成功通知等
        /// </summary>
        private void OnMessageReceived(int configId, Newtonsoft.Json.Linq.JObject message)
        {
            try
            {
                // 🔥 首先，将所有消息分发给对应的 BrowserClient
                //    这样 BrowserClient.SendCommandAsync 可以通过回调接收响应
                if (_browsers.TryGetValue(configId, out var browserClient))
                {
                    browserClient.OnMessageReceived(message);
                }
                
                // 然后，处理特定类型的消息（Cookie更新、登录成功等）
                var messageType = message["type"]?.ToString();
                
                switch (messageType)
                {
                    case "cookie_update":
                        HandleCookieUpdate(configId, message);
                        break;
                        
                    case "login_success":
                        HandleLoginSuccess(configId, message);
                        break;
                        
                    default:
                        _log.Info("AutoBet", $"未处理的消息类型:{messageType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", "处理消息失败", ex);
            }
        }
        
        /// <summary>
        /// 🔥 处理Cookie更新
        /// </summary>
        private void HandleCookieUpdate(int configId, Newtonsoft.Json.Linq.JObject message)
        {
            try
            {
                var url = message["url"]?.ToString();
                var cookies = message["cookies"]?.ToObject<Dictionary<string, string>>();
                
                if (cookies == null || cookies.Count == 0)
                {
                    _log.Warning("AutoBet", $"配置{configId} Cookie为空");
                    return;
                }
                
                // 转换为Cookie字符串
                var cookieString = string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));
                
                // 更新配置
                var config = GetConfig(configId);
                if (config != null)
                {
                    config.Cookies = cookieString;  // 🔥 统一使用Cookies字段
                    config.CookieUpdateTime = DateTime.Now;
                    SaveConfig(config);
                    
                    _log.Info("AutoBet", $"✅ 配置{configId}({config.ConfigName}) Cookie已更新:共{cookies.Count}个");
                }
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"更新Cookie失败:配置{configId}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 处理登录成功通知
        /// </summary>
        private void HandleLoginSuccess(int configId, Newtonsoft.Json.Linq.JObject message)
        {
            try
            {
                var username = message["username"]?.ToString();
                _log.Info("AutoBet", $"✅ 配置{configId} 登录成功:用户{username}");
                
                // 更新配置状态
                var config = GetConfig(configId);
                if (config != null)
                {
                    config.Status = "已登录";
                    SaveConfig(config);
                }
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", "处理登录成功失败", ex);
            }
        }
        
        /// <summary>
        /// 通过 Socket 推送封盘通知到指定配置的浏览器
        /// </summary>
        public async Task NotifySealingAsync(int configId, string issueId, int secondsRemaining)
        {
            if (!_browsers.TryGetValue(configId, out var browserClient))
            {
                _log.Warning("AutoBet", $"浏览器未连接，无法推送封盘通知:配置{configId}");
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
                
                await browserClient.SendCommandAsync("封盘通知", data);
                _log.Info("AutoBet", $"📢 已推送封盘通知:配置{configId} 期号{issueId} 剩余{secondsRemaining}秒");
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"推送封盘通知失败:配置{configId}", ex);
            }
        }
        
        /// <summary>
        /// 通过 Socket 发送投注命令到浏览器，并等待结果
        /// </summary>
        public async Task<BetResult> SendBetCommandAsync(int configId, string issueId, string betContentStandard)
        {
            _log.Info("AutoBet", $"📤 尝试发送投注命令: configId={configId}");
            _log.Info("AutoBet", $"   当前 _browsers 字典包含的 configId: [{string.Join(", ", _browsers.Keys)}]");
            
            if (!_browsers.TryGetValue(configId, out var browserClient))
            {
                _log.Warning("AutoBet", $"❌ 浏览器未连接，无法推送投注命令: configId={configId}");
                _log.Warning("AutoBet", $"   _browsers 中实际的 configId: [{string.Join(", ", _browsers.Keys)}]");
                _log.Warning("AutoBet", $"   ⚠️ configId 不匹配！请检查启动流程。");
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = $"浏览器未连接(configId={configId}不匹配)"
                };
            }
            
            _log.Info("AutoBet", $"✅ 找到浏览器客户端: configId={configId}");
            _log.Info("AutoBet", $"   BrowserClient.IsConnected: {browserClient.IsConnected}");  // 🔥 添加连接状态检查
            
            try
            {
                var data = new
                {
                    issueId = issueId,
                    betContent = betContentStandard
                };
                
                var result = await browserClient.SendCommandAsync("投注", data);
                
                _log.Info("AutoBet", $"📥 投注结果:配置{configId} 成功={result.Success}");
                
                return new BetResult
                {
                    Success = result.Success,
                    Result = result.Data?.ToString(),
                    ErrorMessage = result.ErrorMessage,
                    // 其他字段从 result.Data 解析
                    PostStartTime = result.Data != null && ((dynamic)result.Data).postStartTime != null ? 
                        DateTime.Parse(((dynamic)result.Data).postStartTime.ToString()) : null,
                    PostEndTime = result.Data != null && ((dynamic)result.Data).postEndTime != null ? 
                        DateTime.Parse(((dynamic)result.Data).postEndTime.ToString()) : null,
                    DurationMs = result.Data != null && ((dynamic)result.Data).durationMs != null ? 
                        (int)((dynamic)result.Data).durationMs : null,
                    OrderNo = result.Data != null && ((dynamic)result.Data).orderNo != null ? 
                        ((dynamic)result.Data).orderNo.ToString() : null
                };
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"推送投注命令失败:配置{configId}", ex);
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
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
        /// 获取浏览器客户端（供命令面板使用）
        /// </summary>
        public BrowserClient? GetBrowserClient(int configId)
        {
            return _browsers.TryGetValue(configId, out var client) ? client : null;
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
        /// 启动浏览器（新逻辑：只标记配置需要启动，由监控任务负责实际启动）
        /// </summary>
        public async Task<bool> StartBrowser(int configId)
        {
            try
            {
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _log.Info("AutoBet", $"🎯 请求启动浏览器: ConfigId={configId}");
                
                var config = GetConfig(configId);
                if (config == null)
                {
                    _log.Error("AutoBet", $"❌ 配置不存在: {configId}");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return false;
                }
                
                _log.Info("AutoBet", $"✅ 配置信息: {config.ConfigName} ({config.Platform})");
                _log.Info("AutoBet", $"   当前 IsEnabled 状态: {config.IsEnabled}");
                
                // 🔥 检查是否已连接
                bool isConnected;
                lock (_lock)
                {
                    isConnected = _browsers.ContainsKey(configId);
                }
                
                if (isConnected)
                {
                    _log.Info("AutoBet", $"✅ 浏览器已连接，无需启动");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return true;
                }
                
                _log.Info("AutoBet", $"📌 浏览器未连接，主动触发监控任务检查...");
                
                // 🔥 主动触发监控任务（不等待定时器）
                _log.Info("AutoBet", "   手动调用 MonitorBrowsers 立即检查并启动");
                MonitorBrowsers(null);
                
                // 🔥 等待浏览器连接（最多6秒，给监控任务足够时间）
                // 监控任务会等待2秒 + 启动浏览器需要2-3秒
                _log.Info("AutoBet", $"   等待浏览器启动并连接（最多6秒）...");
                for (int i = 1; i <= 6; i++)
                {
                    await Task.Delay(1000);
                    
                    bool connected;
                    lock (_lock)
                    {
                        connected = _browsers.ContainsKey(configId);
                    }
                    
                    if (connected)
                    {
                        _log.Info("AutoBet", $"✅ 浏览器已连接！（等待了 {i} 秒）");
                        _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                        return true;
                    }
                    
                    _log.Info("AutoBet", $"   ⏳ 等待浏览器连接... {i}/6 秒");
                }
                
                _log.Warning("AutoBet", $"⚠️ 等待6秒后浏览器仍未连接");
                _log.Warning("AutoBet", $"   可能原因：1) 浏览器进程启动失败  2) Socket连接失败  3) 配置ID不匹配");
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return false;
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"❌ 启动浏览器失败: {configId}", ex);
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return false;
            }
        }
        
        /// <summary>
        /// 🔥 内部方法：实际启动浏览器进程（由监控任务调用）
        /// </summary>
        private async Task<bool> StartBrowserInternal(int configId)
        {
            try
            {
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _log.Info("AutoBet", $"🚀 监控任务: 启动浏览器进程 ConfigId={configId}");
                
                var config = GetConfig(configId);
                if (config == null)
                {
                    _log.Error("AutoBet", $"❌ 配置不存在: {configId}");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return false;
                }
                
                // 再次检查是否已连接（避免重复启动）
                bool isConnected;
                lock (_lock)
                {
                    isConnected = _browsers.ContainsKey(configId);
                }
                
                if (isConnected)
                {
                    _log.Info("AutoBet", $"✅ 浏览器已连接，取消启动");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return true;
                }
                
                _log.Info("AutoBet", $"📋 配置信息: {config.ConfigName} ({config.Platform})");
                _log.Info("AutoBet", $"🚀 启动新浏览器进程: {config.ConfigName}");
                _log.Info("AutoBet", $"   ConfigId: {configId}");
                _log.Info("AutoBet", $"   平台: {config.Platform}");
                _log.Info("AutoBet", $"   URL: {config.PlatformUrl}");
                
                // 创建浏览器客户端（Socket 服务器使用固定端口 19527）
                var newBrowserClient = new BrowserClient(configId);
                await newBrowserClient.StartAsync(0, config.ConfigName, config.Platform, config.PlatformUrl);
                
                lock (_lock)
                {
                    _browsers[configId] = newBrowserClient;
                }
                _log.Info("AutoBet", $"✅ 浏览器进程已启动");
                _log.Info("AutoBet", $"   更新后 _browsers 字典: [{string.Join(", ", _browsers.Keys)}]");
                
                // 更新状态
                config.Status = "已启动";
                SaveConfig(config);
                
                // 3️⃣ 等待 Socket 连接建立
                _log.Info("AutoBet", $"⏳ 等待浏览器连接到 Socket 服务器（端口 19527）...");
                await Task.Delay(2000);  // 🔥 等待2秒让浏览器有时间连接
                
                // 检查连接状态
                var (connected, pid) = await newBrowserClient.PingAsync();
                if (connected)
                {
                    _log.Info("AutoBet", $"✅ Socket 连接已建立！进程ID: {pid}");
                }
                else
                {
                    _log.Warning("AutoBet", $"⚠️ Socket 连接尚未建立，可能需要更多时间");
                    _log.Warning("AutoBet", $"   请检查日志中是否有 '浏览器握手成功，配置ID: {configId}' 的消息");
                }
                
                // 4️⃣ 自动登录
                if (config.AutoLogin && !string.IsNullOrEmpty(config.Username))
                {
                    _log.Info("AutoBet", $"🔐 自动登录: {config.Username}");
                    var loginResult = await newBrowserClient.SendCommandAsync("Login", new
                    {
                        username = config.Username,
                        password = config.Password
                    });
                    
                    config.Status = loginResult.Success ? "已登录" : "登录失败";
                    SaveConfig(config);
                    
                    if (loginResult.Success)
                    {
                        _log.Info("AutoBet", $"✅ 登录成功");
                    }
                    else
                    {
                        _log.Warning("AutoBet", $"⚠️ 登录失败: {loginResult.ErrorMessage}");
                    }
                }
                
                _log.Info("AutoBet", $"✅ 浏览器启动流程完成: {config.ConfigName}");
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"❌ 启动浏览器失败: {configId}", ex);
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
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
            var config = GetConfig(configId);
            
            // 🔥 标记配置为禁用状态（PropertyChanged 自动保存到数据库）
            if (config != null && config.IsEnabled)
            {
                config.IsEnabled = false;  // 自动保存
                _log.Info("AutoBet", $"✅ 配置 [{config.ConfigName}] 飞单已关闭");
            }
            
            // 关闭浏览器进程
            if (_browsers.TryGetValue(configId, out var browserClient))
            {
                browserClient.Dispose();
                
                lock (_lock)
                {
                    _browsers.Remove(configId);
                }
                
                if (config != null)
                {
                    config.Status = "已停止";
                    // IsEnabled 已经在上面修改过，会自动保存
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
        
        /// <summary>
        /// 🔥 后台监控任务：自动管理浏览器生命周期
        /// 职责：
        /// 1. 从内存读取所有配置（不访问数据库）
        /// 2. 如果 IsEnabled=true 且 IsConnected=false
        /// 3. 先等待2秒（给老浏览器重连的机会）
        /// 4. 再次检查，如果仍未连接，启动浏览器
        /// </summary>
        private void MonitorBrowsers(object? state)
        {
            try
            {
                if (_configs == null) return;
                
                // 🔥 从内存读取所有启用的配置（不访问数据库）
                var enabledConfigs = _configs.Where(c => c.IsEnabled).ToList();
                
                if (enabledConfigs.Count == 0)
                {
                    // 优化：没有启用的配置，直接返回（避免无效检查）
                    return;
                }
                
                _log.Debug("AutoBet", $"🔍 监控任务检查: 发现 {enabledConfigs.Count} 个启用的配置");
                
                foreach (var config in enabledConfigs)
                {
                    // 🔥 检查连接状态（从 _browsers 字典查询）
                    bool isConnected;
                    lock (_lock)
                    {
                        isConnected = _browsers.ContainsKey(config.Id);
                    }
                    
                    if (isConnected)
                    {
                        _log.Debug("AutoBet", $"✅ 配置 [{config.ConfigName}] 已连接，跳过");
                        continue;
                    }
                    
                    // 🔥 检查是否正在启动（防止重复启动）
                    bool isStarting;
                    lock (_lock)
                    {
                        isStarting = _startingConfigs.Contains(config.Id);
                    }
                    
                    if (isStarting)
                    {
                        _log.Debug("AutoBet", $"⏳ 配置 [{config.ConfigName}] 正在启动中，跳过");
                        continue;
                    }
                    
                    // 🔥 未连接，准备启动浏览器
                    _log.Info("AutoBet", $"📌 配置 [{config.ConfigName}] 飞单已开启但未连接");
                    
                    // 🔥 异步处理（不阻塞定时器）
                    int configId = config.Id;
                    string configName = config.ConfigName;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // 🔥 先等待2秒，给老浏览器重连的机会（在标记"正在启动"之前）
                            _log.Info("AutoBet", $"⏳ [{configName}] 等待2秒，给老浏览器重连的机会...");
                            await Task.Delay(2000);
                            
                            // 🔥 等待后再次检查连接状态
                            bool isConnectedNow;
                            lock (_lock)
                            {
                                isConnectedNow = _browsers.ContainsKey(configId);
                            }
                            
                            if (isConnectedNow)
                            {
                                _log.Info("AutoBet", $"✅ [{configName}] 已在等待期间连接，无需启动浏览器");
                                return;
                            }
                            
                            // 🔥 确认未连接后，再标记为"正在启动"（防止重复）
                            bool shouldStart = false;
                            lock (_lock)
                            {
                                if (!_startingConfigs.Contains(configId))
                                {
                                    _startingConfigs.Add(configId);
                                    shouldStart = true;
                                }
                            }
                            
                            if (!shouldStart)
                            {
                                _log.Debug("AutoBet", $"⏳ [{configName}] 其他任务正在启动浏览器，跳过");
                                return;
                            }
                            
                            // 🔥 确认未连接，启动浏览器
                            _log.Info("AutoBet", $"🚀 [{configName}] 启动浏览器...");
                            await StartBrowserInternal(configId);
                        }
                        catch (Exception ex)
                        {
                            _log.Error("AutoBet", $"监控任务启动浏览器失败: ConfigId={configId}", ex);
                        }
                        finally
                        {
                            // 移除启动标记
                            lock (_lock)
                            {
                                _startingConfigs.Remove(configId);
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", "监控任务异常", ex);
            }
        }
        
        public void Dispose()
        {
            _monitorTimer?.Dispose();
            StopAllBrowsers();
            _socketServer?.Dispose();
            _httpServer?.Dispose();
            _log.Info("AutoBet", "AutoBetService 已释放");
        }
    }
}
