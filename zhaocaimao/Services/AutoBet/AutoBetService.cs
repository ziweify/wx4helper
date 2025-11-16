using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using zhaocaimao.Contracts;
using zhaocaimao.Contracts.Games;
using zhaocaimao.Models.AutoBet;  // 🔥 BetConfig, BetResult
using BaiShengVx3Plus.Shared.Models;  // 🔥 使用共享的模型
using SQLite;

namespace zhaocaimao.Services.AutoBet
{
    /// <summary>
    /// 自动投注服务 - 管理配置和浏览器
    /// </summary>
    public class AutoBetService : IDisposable
    {
        private SQLiteConnection? _db;
        private readonly ILogService _log;
        private IBinggoOrderService? _orderService;
        
        // 🔥 已删除字典！配置对象自己管理 Browser 连接
        // private readonly Dictionary<int, BrowserClient> _browsers = new();  // ❌ 不需要了
        
        // Socket 服务器（双向通信：心跳、状态推送、远程控制）
        private AutoBetSocketServer? _socketServer;
        
        // HTTP 服务器（主数据交互：配置、订单、结果）
        private AutoBetHttpServer? _httpServer;
        
        // 待投注订单队列（配置ID → 订单队列）
        private readonly Dictionary<int, Queue<BaiShengVx3Plus.Shared.Models.BetStandardOrderList>> _orderQueues = new();
        
        // 🔥 配置列表（内存管理，自动保存）- 参考 V2MemberBindingList
        // 每个配置对象通过 config.Browser 管理自己的浏览器连接
        private Core.BetConfigBindingList? _configs;
        
        // 🔥 后台监控任务：自动启动浏览器（如果配置需要但未连接）
        private System.Threading.Timer? _monitorTimer;
        private readonly object _lock = new object();
        
        // 🔥 记录正在启动的配置（防止重复启动）
        private readonly HashSet<int> _startingConfigs = new();
        
        // 🔥 监控任务执行标记（防止并发执行）
        private bool _isMonitoring = false;
        
        // 🔥 用于取消异步任务的 CancellationTokenSource
        private CancellationTokenSource? _cancellationTokenSource;
        
        public AutoBetService(ILogService log, IBinggoOrderService orderService)
        {
            _log = log;
            _orderService = orderService;
            
            _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _log.Info("AutoBet", "🚀 AutoBetService 构造函数执行");
            
            // 启动 Socket 服务器（端口 19527，用于双向通信）
            _socketServer = new AutoBetSocketServer(log, OnBrowserConnected, OnMessageReceived, OnBrowserDisconnected); // 🔥 添加连接断开回调
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
            // 🔥 BetOrderRecord 已删除，改用 BetRecord（由 BetRecordService 管理）
            
            // 🔥 创建配置 BindingList 并加载数据到内存
            _configs = new Core.BetConfigBindingList(_db);
            _configs.LoadFromDatabase();
            
            EnsureDefaultConfig();
            _log.Info("AutoBet", $"✅ 数据库已设置，已加载 {_configs.Count} 个配置到内存");
            
            // 🔥 创建 CancellationTokenSource（用于取消异步任务）
            _cancellationTokenSource = new CancellationTokenSource();
            
            // 🔥 主程序重启场景：检查是否有浏览器进程在运行，等待它们重连
            _ = Task.Run(async () =>
            {
                try
                {
                    // 🔥 使用 CancellationToken，如果已取消则立即返回
                    await Task.Delay(1000, _cancellationTokenSource.Token);  // 等待1秒，让 Socket 服务器完全启动
                    
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    _log.Info("AutoBet", "🔍 检查是否有浏览器进程在运行（主程序重启场景）...");
                    
                    var configsWithProcess = _configs?.Where(c => c.ProcessId > 0 && IsProcessRunning(c.ProcessId)).ToList();
                    
                    if (configsWithProcess != null && configsWithProcess.Count > 0)
                    {
                        _log.Info("AutoBet", $"发现 {configsWithProcess.Count} 个配置的浏览器进程仍在运行");
                        
                        foreach (var config in configsWithProcess)
                        {
                            _log.Info("AutoBet", $"   - [{config.ConfigName}] 进程ID: {config.ProcessId}");
                            
                            // 等待浏览器重连（最多等待5秒）
                            _log.Info("AutoBet", $"   ⏳ 等待浏览器重连到 Socket 服务器...");
                            
                            for (int i = 0; i < 10; i++)
                            {
                                // 🔥 检查是否已取消
                                if (_cancellationTokenSource?.Token.IsCancellationRequested == true)
                                {
                                    _log.Info("AutoBet", $"   ⏹️ [{config.ConfigName}] 任务已取消，停止等待重连");
                                    break;
                                }
                                
                                await Task.Delay(500, _cancellationTokenSource?.Token ?? CancellationToken.None);
                                
                                // 检查是否已连接
                                if (config.IsConnected)
                                {
                                    _log.Info("AutoBet", $"   ✅ [{config.ConfigName}] 浏览器已重连！等待时间: {i * 0.5}秒");
                                    break;
                                }
                                
                                // 检查 Socket 服务器是否有连接
                                var connection = _socketServer?.GetConnection(config.Id);
                                if (connection != null && connection.IsConnected)
                                {
                                    _log.Info("AutoBet", $"   📌 [{config.ConfigName}] 发现 Socket 连接，但未附加到 BrowserClient");
                                    
                                    // 创建或更新 BrowserClient
                                    if (config.Browser == null)
                                    {
                                        var browserClient = new BrowserClient(config.Id);
                                        config.Browser = browserClient;
                                        _log.Info("AutoBet", $"   ✅ 已创建 BrowserClient");
                                    }
                                    
                                    // 附加连接
                                    config.Browser.AttachConnection(connection);
                                    
                                    if (config.IsConnected)
                                    {
                                        _log.Info("AutoBet", $"   ✅ [{config.ConfigName}] 连接已附加，浏览器重连成功！");
                                        config.Status = "已连接";
                                        SaveConfig(config);
                                        break;
                                    }
                                }
                            }
                            
                            if (!config.IsConnected)
                            {
                                _log.Warning("AutoBet", $"   ⚠️ [{config.ConfigName}] 浏览器进程在运行，但5秒内未重连");
                                _log.Warning("AutoBet", $"      监控任务将继续检查，浏览器可能会稍后重连");
                            }
                        }
                    }
                    else
                    {
                        _log.Info("AutoBet", "没有发现运行中的浏览器进程");
                    }
                    
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                }
                catch (OperationCanceledException)
                {
                    _log.Info("AutoBet", "检查浏览器进程任务已取消");
                }
                catch (Exception ex)
                {
                    _log.Error("AutoBet", "检查浏览器进程时出错", ex);
                }
            }, _cancellationTokenSource.Token);
            
            // 🔥 配置加载完成后，再启动监控任务（主要机制，负责检查配置状态并启动浏览器）
            _monitorTimer = new System.Threading.Timer(MonitorBrowsers, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            _log.Info("AutoBet", "✅ 后台监控任务已启动（每2秒检查一次，主要机制）");
            _log.Info("AutoBet", "   说明：监控任务负责检查配置状态并启动浏览器，界面只设置状态");
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
                // 🔥 修改现有配置：强制更新到数据库
                // 因为某些字段（如 Username, Password）是自动属性，不会触发 PropertyChanged
                if (_db != null)
                {
                    config.LastUpdateTime = DateTime.Now;
                    _db.Update(config);  // 🔥 强制更新到数据库
                    _log.Info("AutoBet", $"配置已更新并保存到数据库: {config.ConfigName}");
                }
                else
                {
                    _log.Warning("AutoBet", $"配置已更新但数据库未初始化: {config.ConfigName}");
                }
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
        private void OnBrowserConnected(string configName, int browserConfigId, int processId)
        {
            try
            {
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _log.Info("AutoBet", $"🔗 浏览器已通过 Socket 连接，配置名: {configName}");
                _log.Info("AutoBet", $"   浏览器ConfigId: {browserConfigId}");
                _log.Info("AutoBet", $"   进程ID: {processId}");
                
                // 🔥 根据配置名查找配置（而不是配置ID）
                Models.AutoBet.BetConfig? config;
                lock (_lock)
                {
                    config = _configs?.FirstOrDefault(c => c.ConfigName == configName);
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
                _log.Info("AutoBet", $"   当前连接状态: {(config.IsConnected ? "已连接" : "未连接")}");
                
                // 🔥 保存进程ID到配置
                config.ProcessId = processId;
                SaveConfig(config);
                _log.Info("AutoBet", $"✅ 已保存进程ID: {processId}");
                
                // 🔥 从 AutoBetSocketServer 获取 ClientConnection
                var connection = _socketServer?.GetConnection(browserConfigId);
                if (connection == null)
                {
                    _log.Error("AutoBet", $"❌ 无法获取 ClientConnection: {browserConfigId}");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return;
                }
                
                _log.Info("AutoBet", $"✅ 已获取 ClientConnection，连接状态: {connection.IsConnected}");
                
                // 🔥 配置对象自己管理 Browser！
                BrowserClient? browserClient = config.Browser;
                
                if (browserClient == null)
                {
                    // 🔥 主程序重启或数据库重建场景：config.Browser 为 null，但浏览器在运行并重连了
                    _log.Info("AutoBet", $"📌 配置无 Browser 实例，自动创建");
                    _log.Info("AutoBet", $"   场景：主程序重启、数据库重建、或浏览器先于主程序启动");
                    
                    browserClient = new BrowserClient(configId);
                    config.Browser = browserClient;  // 🔥 先设置到配置，再附加连接
                }
                else
                {
                    _log.Info("AutoBet", $"📌 配置已有 Browser 实例，更新连接");
                }
                
                // 🔥 附加连接（无论新建还是已存在，都要更新连接）
                browserClient.AttachConnection(connection);
                
                // 🔥 验证连接状态
                if (browserClient.IsConnected)
                {
                    _log.Info("AutoBet", $"✅ BrowserClient 连接状态验证成功");
                }
                else
                {
                    _log.Warning("AutoBet", $"⚠️ BrowserClient 连接状态验证失败，但继续处理");
                    _log.Warning("AutoBet", $"   connection.IsConnected={connection.IsConnected}");
                    _log.Warning("AutoBet", $"   browserClient.IsConnected={browserClient.IsConnected}");
                }
                
                // 更新配置状态
                config.Status = "已连接";
                SaveConfig(config);
                
                _log.Info("AutoBet", $"✅ 浏览器 Socket 连接处理完成: {config.ConfigName}");
                _log.Info("AutoBet", $"   配置连接状态: {(config.IsConnected ? "已连接" : "未连接")}");
                _log.Info("AutoBet", $"   BrowserClient.IsConnected: {browserClient.IsConnected}");
                _log.Info("AutoBet", $"   ClientConnection.IsConnected: {connection.IsConnected}");
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"❌ 处理浏览器连接失败: {configName}", ex);
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
        }
        
        /// <summary>
        /// 🔥 浏览器连接断开回调（事件驱动）
        /// 当浏览器 Socket 连接断开时，自动触发恢复机制
        /// </summary>
        private void OnBrowserDisconnected(int configId)
        {
            try
            {
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _log.Info("AutoBet", $"🔌 浏览器连接断开事件: ConfigId={configId}");
                
                var config = GetConfig(configId);
                if (config == null)
                {
                    _log.Warning("AutoBet", $"配置不存在: {configId}");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return;
                }
                
                _log.Info("AutoBet", $"配置信息: {config.ConfigName}");
                _log.Info("AutoBet", $"   IsEnabled: {config.IsEnabled}");
                _log.Info("AutoBet", $"   IsConnected: {config.IsConnected}");
                
                // 🔥 清理失效的 Browser 引用
                if (config.Browser != null)
                {
                    _log.Info("AutoBet", $"清理失效的 Browser 引用");
                    config.Browser = null;
                }
                
                // 🔥 更新状态
                config.Status = "连接断开";
                SaveConfig(config);
                
                // 🔥 如果配置已启用，由监控任务统一处理恢复（避免重复启动）
                // 🔥 事件驱动只负责清理和标记，不直接启动浏览器
                if (config.IsEnabled)
                {
                    // 🔥 前置并发控制：检查是否已经在启动中
                    bool alreadyStarting = false;
                    lock (_lock)
                    {
                        alreadyStarting = _startingConfigs.Contains(configId);
                    }
                    
                    if (alreadyStarting)
                    {
                        _log.Info("AutoBet", $"⏳ [{config.ConfigName}] 配置已在启动中，跳过事件驱动恢复");
                        _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                        return;
                    }
                    
                    _log.Info("AutoBet", $"配置已启用，监控任务将在2秒内检查并恢复连接...");
                    _log.Info("AutoBet", "   说明：恢复由监控任务统一处理，避免与事件驱动重复启动");
                }
                else
                {
                    _log.Info("AutoBet", $"配置未启用，不自动恢复");
                }
                
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"处理连接断开事件失败: ConfigId={configId}", ex);
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
                var config = GetConfig(configId);
                if (config?.Browser != null)
                {
                    config.Browser.OnMessageReceived(message);
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
            var config = GetConfig(configId);
            var browserClient = config?.Browser;
            if (browserClient == null)
            {
                _log.Warning("AutoBet", $"浏览器未连接，无法推送封盘通知: {config?.ConfigName ?? configId.ToString()}");
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
            
            var config = GetConfig(configId);
            if (config == null)
            {
                _log.Error("AutoBet", $"❌ 配置不存在: configId={configId}");
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "配置不存在"
                };
            }
            
            var browserClient = config.Browser;
            if (browserClient == null)
            {
                _log.Warning("AutoBet", $"❌ 浏览器未连接，无法推送投注命令: {config.ConfigName}");
                _log.Warning("AutoBet", $"   监控任务会在3秒内自动检查并重启浏览器");
                
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "浏览器未连接(监控任务将自动重连)"
                };
            }
            
            // 🔥 检查连接状态
            if (!browserClient.IsConnected)
            {
                _log.Error("AutoBet", $"❌ 浏览器客户端存在但 IsConnected=false");
                
                // 🔥 详细诊断（仅在连接失败时输出）
                var connection = browserClient.GetConnection();
                _log.Error("AutoBet", $"   📊 诊断: connection={connection != null}");
                
                if (connection != null)
                {
                    _log.Error("AutoBet", $"   Client={connection.Client != null}, Connected={connection.Client?.Connected}");
                    
                    if (connection.Client?.Client != null)
                    {
                        try
                        {
                            var socket = connection.Client.Client;
                            bool pollResult = socket.Poll(1, System.Net.Sockets.SelectMode.SelectRead);
                            int available = socket.Available;
                            _log.Error("AutoBet", $"   Socket.Poll={pollResult}, Available={available}");
                        }
                        catch (Exception ex)
                        {
                            _log.Error("AutoBet", $"   Socket检查异常: {ex.Message}");
                        }
                    }
                }
                
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "连接状态异常(请查看日志)"
                };
            }
            
            try
            {
                // 🔥 将字符串格式的 betContentStandard 解析为 BetStandardOrderList
                // 格式："1大10,2大10,3大10,4大10"
                var betOrders = BaiShengVx3Plus.Shared.Parsers.BetContentParser.ParseBetContentToOrderList(betContentStandard, int.Parse(issueId));
                
                if (betOrders == null || betOrders.Count == 0)
                {
                    _log.Warning("AutoBet", $"❌ 解析投注内容失败或为空: {betContentStandard}");
                    return new BetResult
                    {
                        Success = false,
                        ErrorMessage = "投注内容解析失败"
                    };
                }
                
                // 🔥 发送 BetStandardOrderList 对象（浏览器端期望的格式）
                var result = await browserClient.SendCommandAsync("投注", betOrders);
                
                _log.Info("AutoBet", $"📥 投注结果:配置{configId} 成功={result.Success}");
                
                // 🔥 安全解析 result.Data（避免 JValue 错误）
                var betResult = new BetResult
                {
                    Success = result.Success,
                    ErrorMessage = result.ErrorMessage
                };
                
                // 🔥 解析返回数据（包含时间信息和平台完整响应）
                if (result.Data != null && result.Data is Newtonsoft.Json.Linq.JObject dataObj)
                {
                    // 🔥 提取平台完整响应（最重要的信息）
                    var platformResponse = dataObj["platformResponse"]?.ToString();
                    betResult.Result = platformResponse ?? dataObj.ToString();
                    
                    // 解析时间和耗时（用于性能监控）
                    var postStartStr = dataObj["postStartTime"]?.ToString();
                    var postEndStr = dataObj["postEndTime"]?.ToString();
                    
                    if (!string.IsNullOrEmpty(postStartStr) && DateTime.TryParse(postStartStr, out var postStart))
                    {
                        betResult.PostStartTime = postStart;
                    }
                    
                    if (!string.IsNullOrEmpty(postEndStr) && DateTime.TryParse(postEndStr, out var postEnd))
                    {
                        betResult.PostEndTime = postEnd;
                    }
                    
                    betResult.DurationMs = dataObj["durationMs"]?.ToObject<int?>();
                    betResult.OrderNo = dataObj["orderNo"]?.ToString();
                    betResult.OrderId = dataObj["orderId"]?.ToString();  // 兼容旧字段
                    
                    // 🔥 处理错误信息（区分客户端错误和平台错误）
                    if (!result.Success)
                    {
                        // 如果平台响应以#开头，说明是客户端校验错误
                        if (!string.IsNullOrEmpty(platformResponse) && platformResponse.StartsWith("#"))
                        {
                            betResult.ErrorMessage = platformResponse;  // 客户端错误（#未登录，无法下注）
                        }
                        // 否则尝试从平台API响应中提取错误信息
                        else if (!string.IsNullOrEmpty(platformResponse))
                        {
                            try
                            {
                                var platformJson = Newtonsoft.Json.Linq.JObject.Parse(platformResponse);
                                var msg = platformJson["msg"]?.ToString();
                                var errcode = platformJson["errcode"]?.ToString();
                                if (!string.IsNullOrEmpty(msg))
                                {
                                    // 平台API错误（格式化显示）
                                    betResult.ErrorMessage = string.IsNullOrEmpty(errcode) 
                                        ? $"[平台] {msg}" 
                                        : $"[平台] {msg} (errcode={errcode})";
                                }
                            }
                            catch
                            {
                                // JSON解析失败，可能是普通错误文本
                                if (string.IsNullOrEmpty(betResult.ErrorMessage))
                                {
                                    betResult.ErrorMessage = result.ErrorMessage;
                                }
                            }
                        }
                        // 如果还是没有ErrorMessage，使用CommandResponse的ErrorMessage
                        else if (string.IsNullOrEmpty(betResult.ErrorMessage))
                        {
                            betResult.ErrorMessage = result.ErrorMessage ?? "投注失败";
                        }
                    }
                }
                else if (result.Data != null)
                {
                    // 如果不是 JObject，直接转换为字符串
                    betResult.Result = result.Data.ToString();
                }
                
                return betResult;
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
        public void QueueBetOrder(int configId, BaiShengVx3Plus.Shared.Models.BetStandardOrderList orders)
        {
            lock (_orderQueues)
            {
                if (!_orderQueues.ContainsKey(configId))
                {
                    _orderQueues[configId] = new Queue<BaiShengVx3Plus.Shared.Models.BetStandardOrderList>();
                }
                
                _orderQueues[configId].Enqueue(orders);
                var issueId = orders.Count > 0 ? orders[0].IssueId : 0;
                var totalAmount = orders.GetTotalAmount();
                _log.Info("AutoBet", $"📝 订单已加入队列: 配置{configId} 期号{issueId} 共{orders.Count}项 {totalAmount}元");
            }
        }
        
        /// <summary>
        /// 获取待处理订单（HTTP API 调用）
        /// </summary>
        public BaiShengVx3Plus.Shared.Models.BetStandardOrderList? GetPendingOrder(int configId, int? issueId)
        {
            lock (_orderQueues)
            {
                if (!_orderQueues.TryGetValue(configId, out var queue) || queue.Count == 0)
                {
                    return null;
                }
                
                // 如果指定了期号，查找对应期号的订单
                if (issueId.HasValue)
                {
                    return queue.FirstOrDefault(o => o.Count > 0 && o[0].IssueId == issueId.Value);
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
            return GetConfig(configId)?.Browser;
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
                BaiShengVx3Plus.Shared.Models.BetStandardOrderList? orders = null;
                lock (_orderQueues)
                {
                    if (_orderQueues.TryGetValue(configId, out var queue) && queue.Count > 0)
                    {
                        orders = queue.Dequeue();
                    }
                }
                
                if (orders == null)
                {
                    _log.Warning("AutoBet", $"未找到对应订单: 配置{configId}");
                    return;
                }
                
                // 🔥 投注记录已由 BetRecordService 统一管理，此处不再重复记录
                
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
        /// 启动浏览器（界面调用：只设置状态，由监控任务负责实际启动）
        /// 
        /// 🔥 工作流程：
        /// 1. 检查是否已连接 → 已连接则直接返回
        /// 2. 设置 config.IsEnabled = true（触发监控任务）
        /// 3. 监控任务会在3秒内检测到并启动浏览器
        /// 
        /// 注意：监控任务是主要机制，负责检查配置状态并启动浏览器
        ///       界面只负责设置状态，不直接启动浏览器
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
                if (config.IsConnected)
                {
                    _log.Info("AutoBet", $"✅ 浏览器已连接，无需启动");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return true;
                }
                
                // 🔥 设置 IsEnabled = true（监控任务会检测到并启动浏览器）
                if (!config.IsEnabled)
                {
                    _log.Info("AutoBet", $"📌 设置配置为启用状态");
                    config.IsEnabled = true;
                    SaveConfig(config);
                }
                
                // 🔥 立即触发一次监控任务（即时响应，不等待定时器）
                _log.Info("AutoBet", $"🚀 立即触发监控任务检查...");
                MonitorBrowsers(null);
                
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return true; // 返回 true，表示状态已设置，监控任务会处理
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"❌ 启动浏览器失败: {configId}", ex);
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return false;
            }
        }
        
        /// <summary>
        /// 🔥 内部方法：实际启动浏览器进程
        /// 由 StartBrowser（用户主动调用）和 OnBrowserDisconnected（事件驱动恢复）调用
        /// </summary>
        private async Task<bool> StartBrowserInternal(int configId)
        {
            try
            {
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _log.Info("AutoBet", $"🚀 启动浏览器进程 ConfigId={configId}");
                
                var config = GetConfig(configId);
                if (config == null)
                {
                    _log.Error("AutoBet", $"❌ 配置不存在: {configId}");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return false;
                }
                
                // 再次检查是否已连接（避免重复启动）
                if (config.IsConnected)
                {
                    _log.Info("AutoBet", $"✅ 浏览器已连接，取消启动");
                    _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return true;
                }
                
                // 🔥 清理旧的 BrowserClient（如果存在）
                if (config.Browser != null)
                {
                    _log.Info("AutoBet", $"🧹 清理旧的 BrowserClient（准备启动新浏览器）");
                    try
                    {
                        config.Browser.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _log.Warning("AutoBet", $"清理旧 BrowserClient 时出错: {ex.Message}");
                    }
                    config.Browser = null;
                }
                
                _log.Info("AutoBet", $"📋 配置信息: {config.ConfigName} ({config.Platform})");
                _log.Info("AutoBet", $"🚀 启动新浏览器进程: {config.ConfigName}");
                _log.Info("AutoBet", $"   ConfigId: {configId}");
                _log.Info("AutoBet", $"   平台: {config.Platform}");
                _log.Info("AutoBet", $"   URL: {config.PlatformUrl}");
                
                // 创建浏览器客户端（Socket 服务器使用固定端口 19527）
                var newBrowserClient = new BrowserClient(configId);
                
                // 🔥 先设置到配置，这样 OnBrowserConnected 能找到它
                config.Browser = newBrowserClient;
                _log.Info("AutoBet", $"✅ BrowserClient 已设置到配置对象（等待连接）");
                
                // 启动浏览器进程
                await newBrowserClient.StartAsync(0, config.ConfigName, config.Platform, config.PlatformUrl);
                _log.Info("AutoBet", $"✅ 浏览器进程已启动");
                
                // 更新状态
                config.Status = "已启动";
                SaveConfig(config);
                
                // 3️⃣ 等待 Socket 连接建立（浏览器会主动连接到端口 19527）
                _log.Info("AutoBet", $"⏳ 等待浏览器连接到 Socket 服务器（端口 19527）...");
                
                // 🔥 等待连接建立，最多等待5秒
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(500);
                    
                    // 检查连接状态
                    if (config.IsConnected)
                    {
                        _log.Info("AutoBet", $"✅ Socket 连接已建立！等待时间: {i * 0.5}秒");
                        break;
                    }
                }
                
                // 最终检查连接状态
                if (config.IsConnected)
                {
                    _log.Info("AutoBet", $"✅ 浏览器连接成功，可以发送命令");
                }
                else
                {
                    _log.Warning("AutoBet", $"⚠️ Socket 连接尚未建立（等待5秒后）");
                    _log.Warning("AutoBet", $"   请检查日志中是否有 '浏览器握手成功，配置ID: {configId}' 的消息");
                    _log.Warning("AutoBet", $"   当前 BrowserClient.IsConnected: {newBrowserClient.IsConnected}");
                    
                    // 🔥 检查是否有连接但未附加
                    var connection = _socketServer?.GetConnection(configId);
                    if (connection != null)
                    {
                        _log.Warning("AutoBet", $"   ⚠️ 发现 Socket 连接存在但未附加到 BrowserClient");
                        _log.Warning("AutoBet", $"   连接状态: {connection.IsConnected}");
                        _log.Warning("AutoBet", $"   尝试手动附加连接...");
                        newBrowserClient.AttachConnection(connection);
                        
                        if (config.IsConnected)
                        {
                            _log.Info("AutoBet", $"✅ 手动附加连接成功！");
                        }
                    }
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
        public async Task<BetResult> PlaceBet(int configId, BaiShengVx3Plus.Shared.Models.BetStandardOrderList orders)
        {
            var config = GetConfig(configId);
            if (config == null)
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "配置不存在"
                };
            }
            
            var browserClient = config.Browser;
            if (browserClient == null)
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "浏览器未启动"
                };
            }
            
            var issueId = orders.Count > 0 ? orders[0].IssueId : 0;
            var totalAmount = orders.GetTotalAmount();
            _log.Info("AutoBet", $"📤 [{config.ConfigName}] 投注: 期号{issueId} 共{orders.Count}项 {totalAmount}元");
            
            // 发送投注命令
            var result = await browserClient.SendCommandAsync("PlaceBet", orders);
            
            // 🔥 投注记录已由 BetRecordService 统一管理，此处不再重复记录
            
            _log.Info("AutoBet", $"📥 [{config.ConfigName}] 投注结果: {(result.Success ? "✅ 成功" : "❌ 失败")}");
            
            return result;
        }
        
        /// <summary>
        /// 停止浏览器（用户明确要求停止时调用，如配置管理器中的"停止浏览器"按钮）
        /// 
        /// 🔥 注意：此方法会关闭浏览器进程
        /// - 关闭飞单开关时不应该调用此方法
        /// - 只在用户明确要求停止浏览器时调用
        /// </summary>
        public void StopBrowser(int configId)
        {
            _log.Info("AutoBet", $"⏹️ 请求停止浏览器: ConfigId={configId}");
            
            var config = GetConfig(configId);
            
            // 🔥 先设置 IsEnabled=false（防止监控任务重启）
            if (config != null)
            {
                config.IsEnabled = false;  // 🔥 自动保存，监控任务会立即看到
                _log.Info("AutoBet", $"   配置 [{config.ConfigName}] IsEnabled 已设置为 false");
            }
            
            // 然后关闭浏览器进程
            var browserClient = config?.Browser;
            if (browserClient != null)
            {
                _log.Info("AutoBet", $"   正在关闭浏览器进程...");
                
                browserClient.Dispose();
                config!.Browser = null;  // 🔥 配置对象清除 Browser 引用
                config.Status = "已停止";
                SaveConfig(config);
                
                _log.Info("AutoBet", $"✅ 浏览器已停止: {config.ConfigName}");
            }
            else
            {
                _log.Info("AutoBet", $"   浏览器未运行，无需停止");
            }
        }
        
        /// <summary>
        /// 停止所有浏览器
        /// </summary>
        public void StopAllBrowsers()
        {
            if (_configs == null) return;
            
            foreach (var config in _configs.Where(c => c.Browser != null).ToList())
            {
                StopBrowser(config.Id);
            }
        }
        
        #endregion
        
        /// <summary>
        /// 🔥 后台监控任务：主要机制（负责检查配置状态并启动浏览器）
        /// 
        /// 职责：
        /// 1. 从内存读取所有配置（不访问数据库）
        /// 2. 如果 IsEnabled=true 且 IsConnected=false
        /// 3. 检查进程是否还在运行（如果在运行，等待重连）
        /// 4. 如果进程不在运行，启动新浏览器
        /// 
        /// 工作流程：
        /// - 界面打开飞单开关 → 设置 config.IsEnabled = true
        /// - 监控任务检测到 IsEnabled=true 且 IsConnected=false → 启动浏览器
        /// - 事件驱动（OnBrowserDisconnected）作为辅助，处理连接断开后的自动恢复
        /// 
        /// 🔥 并发控制：使用 _isMonitoring 标记防止重复执行
        /// </summary>
        private void MonitorBrowsers(object? state)
        {
            // 🔥 并发控制：如果正在执行，直接返回（防止定时器重叠执行）
            lock (_lock)
            {
                if (_isMonitoring)
                {
                    _log.Debug("AutoBet", "⏳ 监控任务正在执行中，跳过本次触发");
                    return;
                }
                _isMonitoring = true;
            }
            
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
                
                // 🔥 简化日志：只在有问题时才输出
                foreach (var config in enabledConfigs)
                {
                    // 🔥 检查连接状态（配置对象自己管理）
                    if (config.IsConnected)
                    {
                        // 已连接，跳过
                        continue;
                    }
                    
                    // 🔥 如果 Browser 存在但未连接，移除它
                    if (config.Browser != null && !config.Browser.IsConnected)
                    {
                        _log.Warning("AutoBet", $"⚠️ 配置 [{config.ConfigName}] Browser存在但IsConnected=False");
                        
                        // 🔥 详细诊断
                        var connection = config.Browser.GetConnection();
                        _log.Warning("AutoBet", $"   诊断: connection={connection != null}, Client={connection?.Client != null}, Connected={connection?.Client?.Connected}");
                        
                        // 🔥 移除失效的 Browser，允许重新启动
                        config.Browser = null;
                        _log.Info("AutoBet", $"   🔧 已移除失效的 Browser");
                    }
                    
                    // 🔥 检查进程是否还在运行（简单方案）
                    if (config.ProcessId > 0 && IsProcessRunning(config.ProcessId))
                    {
                        _log.Info("AutoBet", $"⏳ 配置 [{config.ConfigName}] 浏览器进程 {config.ProcessId} 仍在运行，等待重连...");
                        continue;  // 🔥 进程还在，不启动新的
                    }
                    
                    // 🔥 前置并发控制：立即标记"正在启动"（在 Task.Run 之前）
                    bool shouldStart = false;
                    lock (_lock)
                    {
                        if (!_startingConfigs.Contains(config.Id))
                        {
                            _startingConfigs.Add(config.Id);  // 🔥 立即标记，防止竞态
                            shouldStart = true;
                        }
                    }
                    
                    if (!shouldStart)
                    {
                        _log.Debug("AutoBet", $"⏳ 配置 [{config.ConfigName}] 正在启动中，跳过");
                        continue;
                    }
                    
                    // 🔥 未连接，准备启动浏览器（已标记，不会重复）
                    _log.Info("AutoBet", $"📌 配置 [{config.ConfigName}] 飞单已开启但未连接");
                    
                    // 🔥 异步处理（不阻塞定时器）
                    int configId = config.Id;
                    string configName = config.ConfigName;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // 🔥 先等待2秒，给老浏览器重连的机会
                            _log.Info("AutoBet", $"⏳ [{configName}] 等待2秒，给老浏览器重连的机会...");
                            await Task.Delay(2000);
                            
                            // 🔥 等待后再次检查连接状态
                            var cfgCheck = GetConfig(configId);
                            if (cfgCheck?.IsConnected == true)
                            {
                                _log.Info("AutoBet", $"✅ [{configName}] 已在等待期间连接，无需启动浏览器");
                                return;
                            }
                            
                            // 🔥 再次检查 IsEnabled（可能用户在等待期间关闭了）
                            var cfg = GetConfig(configId);
                            if (cfg == null || !cfg.IsEnabled)
                            {
                                _log.Info("AutoBet", $"   [{configName}] IsEnabled=false，取消启动");
                                return;
                            }
                            
                            // 🔥 确认未连接且需要启动，启动浏览器
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
            finally
            {
                // 🔥 清除执行标记（确保即使异常也能清除）
                lock (_lock)
                {
                    _isMonitoring = false;
                }
            }
        }
        
        /// <summary>
        /// 🔥 检查进程是否还在运行
        /// </summary>
        private bool IsProcessRunning(int processId)
        {
            try
            {
                var process = System.Diagnostics.Process.GetProcessById(processId);
                return !process.HasExited;
            }
            catch
            {
                return false;  // 进程不存在
            }
        }
        
        /// <summary>
        /// 🔥 释放资源（主进程关闭时调用）
        /// 按正确顺序停止所有定时器和自动任务
        /// </summary>
        public void Dispose()
        {
            try
            {
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _log.Info("AutoBet", "🛑 开始释放 AutoBetService 资源...");
                
                // 🔥 步骤1: 取消所有异步任务
                if (_cancellationTokenSource != null)
                {
                    _log.Info("AutoBet", "⏹️ 取消所有异步任务...");
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                    _log.Info("AutoBet", "✅ 异步任务已取消");
                }
                
                // 🔥 步骤2: 停止监控定时器（防止新的任务启动）
                if (_monitorTimer != null)
                {
                    _log.Info("AutoBet", "⏹️ 停止监控定时器...");
                    _monitorTimer.Dispose();
                    _monitorTimer = null;
                    _log.Info("AutoBet", "✅ 监控定时器已停止");
                }
                
                // 🔥 步骤3: 等待正在执行的监控任务完成（最多等待2秒）
                if (_isMonitoring)
                {
                    _log.Info("AutoBet", "⏳ 等待正在执行的监控任务完成...");
                    int waitCount = 0;
                    while (_isMonitoring && waitCount < 20)  // 最多等待2秒
                    {
                        Thread.Sleep(100);
                        waitCount++;
                    }
                    if (_isMonitoring)
                    {
                        _log.Warning("AutoBet", "⚠️ 监控任务仍在执行，强制继续");
                    }
                    else
                    {
                        _log.Info("AutoBet", "✅ 监控任务已完成");
                    }
                }
                
                // 🔥 步骤4: 停止 Socket 服务器（停止接受新连接）
                if (_socketServer != null)
                {
                    _log.Info("AutoBet", "⏹️ 停止 Socket 服务器...");
                    _socketServer.Dispose();
                    _socketServer = null;
                    _log.Info("AutoBet", "✅ Socket 服务器已停止");
                }
                
                // 🔥 步骤5: 停止 HTTP 服务器
                if (_httpServer != null)
                {
                    _log.Info("AutoBet", "⏹️ 停止 HTTP 服务器...");
                    _httpServer.Dispose();
                    _httpServer = null;
                    _log.Info("AutoBet", "✅ HTTP 服务器已停止");
                }
                
                // 🔥 步骤6: 停止所有浏览器（最后停止，因为可能正在处理命令）
                _log.Info("AutoBet", "⏹️ 停止所有浏览器...");
                StopAllBrowsers();
                _log.Info("AutoBet", "✅ 所有浏览器已停止");
                
                _log.Info("AutoBet", "✅ AutoBetService 资源释放完成");
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", "释放资源时出错", ex);
            }
        }
    }
}

