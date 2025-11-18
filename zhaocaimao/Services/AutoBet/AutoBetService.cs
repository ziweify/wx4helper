using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using zhaocaimao.Contracts;
using zhaocaimao.Contracts.Games;
using zhaocaimao.Models.AutoBet;  // 🔥 BetConfig, BetResult
using zhaocaimao.Shared.Models;  // 🔥 使用共享的模型
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
        private BetRecordService? _betRecordService;  // 🔥 投注记录服务
        
        // 🔥 已删除字典！配置对象自己管理 Browser 连接
        // private readonly Dictionary<int, BrowserClient> _browsers = new();  // ❌ 不需要了
        
        // Socket 服务器（双向通信：心跳、状态推送、远程控制）
        private AutoBetSocketServer? _socketServer;
        
        // HTTP 服务器（主数据交互：配置、订单、结果）
        private AutoBetHttpServer? _httpServer;
        
        // 待投注订单队列（配置ID → 订单队列）
        private readonly Dictionary<int, Queue<zhaocaimao.Shared.Models.BetStandardOrderList>> _orderQueues = new();
        
        // 🔥 配置列表（内存管理，自动保存）- 参考 V2MemberBindingList
        // 每个配置对象通过 config.Browser 管理自己的浏览器连接
        private Core.BetConfigBindingList? _configs;
        
        // 🔥 后台监控任务：自动启动浏览器（如果配置需要但未连接）
        private Thread? _monitorThread;
        private bool _monitorRunning = false;
        private readonly object _lock = new object();
        
        // 🔥 记录正在启动的配置（防止重复启动）
        private readonly HashSet<int> _startingConfigs = new();
        
        // 🔥 用于取消异步任务的 CancellationTokenSource
        private CancellationTokenSource? _cancellationTokenSource;
        
        public AutoBetService(ILogService log, IBinggoOrderService orderService, BetRecordService betRecordService)
        {
            _log = log;
            _orderService = orderService;
            _betRecordService = betRecordService;
            
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
            _log.Info("AutoBet", $"📦 设置数据库: {_db.DatabasePath}");
            
            // 🔥 配置 SQLite 为最可靠模式（数据完整性优先）
            try
            {
                // 1️⃣ 禁用 WAL 模式，使用传统 DELETE 日志（数据立即写入主文件）
                _db.Execute("PRAGMA journal_mode = DELETE");
                var journalMode = _db.ExecuteScalar<string>("PRAGMA journal_mode");
                _log.Info("AutoBet", $"✅ 日志模式: {journalMode} (数据立即持久化)");
                
                // 2️⃣ 设置为 FULL 同步模式（确保每次写入都刷新到磁盘）
                _db.Execute("PRAGMA synchronous = FULL");
                var syncMode = _db.ExecuteScalar<int>("PRAGMA synchronous");
                _log.Info("AutoBet", $"✅ 同步模式: {syncMode} (FULL=2, 最高可靠性)");
                
                // 3️⃣ 启用外键约束（数据一致性）
                _db.Execute("PRAGMA foreign_keys = ON");
                _log.Info("AutoBet", "✅ 外键约束已启用");
            }
            catch (Exception ex)
            {
                _log.Warning("AutoBet", $"配置数据库参数失败: {ex.Message}");
            }
            
            // 🔥 创建配置 BindingList 并加载数据到内存
            _configs = new Core.BetConfigBindingList(_db, _log);
            _configs.LoadFromDatabase();
            _log.Info("AutoBet", "✅ BetConfig BindingList 已创建并加载");
            
            // 🔥 创建投注记录 BindingList 并注入到 BetRecordService
            var betRecordBindingList = new Core.BetRecordBindingList(_db);
            _betRecordService?.SetBindingList(betRecordBindingList);
            _log.Info("AutoBet", "✅ BetRecord BindingList 已创建并注入到服务");
            
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
                    
                    // 🔥 读取配置时也需要线程安全
                    List<BetConfig>? configsWithProcess = null;
                    lock (_lock)
                    {
                        configsWithProcess = _configs?.Where(c => c.ProcessId > 0 && IsProcessRunning(c.ProcessId)).ToList();
                    }
                    
                    if (configsWithProcess != null && configsWithProcess.Count > 0)
                    {
                        _log.Info("AutoBet", $"发现 {configsWithProcess.Count} 个配置的浏览器进程仍在运行");
                        
                        foreach (var config in configsWithProcess)
                        {
                            _log.Info("AutoBet", $"   - [{config.ConfigName}] 进程ID: {config.ProcessId}");
                            
                            // 等待浏览器重连（最多等待2秒，本机连接应该很快）
                            _log.Info("AutoBet", $"   ⏳ 等待浏览器重连到 Socket 服务器...");
                            
                            for (int i = 0; i < 4; i++)
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
                                _log.Warning("AutoBet", $"   ⚠️ [{config.ConfigName}] 浏览器进程在运行，但2秒内未重连");
                                _log.Warning("AutoBet", $"      保留 ProcessId={config.ProcessId}，让监控任务继续等待（避免重复启动）");
                                // 🔥 不清除 ProcessId！让 MonitorBrowsers 看到进程还在运行，避免重复启动
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
            
            // 🔥 给所有配置注入依赖（LogService 和 SocketServer）
            InjectDependenciesToConfigs();
            
            _log.Info("AutoBet", "✅ 数据库设置完成，等待配置同步后启动监控");
        }
        
        /// <summary>
        /// 给所有配置注入依赖服务
        /// </summary>
        private void InjectDependenciesToConfigs()
        {
            if (_configs == null) return;
            
            foreach (var config in _configs)
            {
                config.SetDependencies(_log, _socketServer);
            }
            
            _log.Info("AutoBet", $"✅ 已为 {_configs.Count} 个配置注入依赖服务");
        }
        
        /// <summary>
        /// 启动监控（在所有配置初始化完成后调用）
        /// 🔥 新架构：启动所有配置的监控线程（每个配置独立，监控线程内部检查 IsEnabled）
        /// </summary>
        public void StartMonitoring()
        {
            if (_configs == null)
            {
                _log.Warning("AutoBet", "⚠️ 配置列表为空，无法启动监控");
                return;
            }
            
            _log.Info("AutoBet", "🚀 开始启动配置监控（配置自管理模式）...");
            
            int startedCount = 0;
            foreach (var config in _configs)
            {
                // 🔥 无论 IsEnabled 状态如何，都启动监控线程
                // 监控线程内部会检查 IsEnabled，只有启用时才启动浏览器
                config.StartMonitoring();
                startedCount++;
            }
            
            _log.Info("AutoBet", $"✅ 已启动 {startedCount} 个配置的监控线程");
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
        /// 保存配置（兼容方法 - 遵循 F5BotV2 设计）
        /// 
        /// 🔥 F5BotV2 设计原则：
        /// 1. 新配置：添加到 BindingList → 自动保存
        /// 2. 修改配置：PropertyChanged → 自动保存
        /// 3. 不需要手动调用数据库操作！
        /// 
        /// 本方法仅作为向后兼容层保留
        /// </summary>
        public void SaveConfig(BetConfig config)
        {
            if (_configs == null)
            {
                _log.Error("AutoBet", "❌ SaveConfig 失败: _configs 为 null");
                return;
            }
            
            if (config.Id == 0)
            {
                // 🔥 新配置：添加到 BindingList（自动触发数据库保存）
                _configs.Add(config);
                _log.Info("AutoBet", $"✅ 配置已添加: {config.ConfigName} (新ID={config.Id})");
            }
            else
            {
                // 🔥 修改现有配置：BindingList 的 PropertyChanged 会自动保存
                // 这里只需要更新 LastUpdateTime，触发一次保存
                config.LastUpdateTime = DateTime.Now;
                
                _log.Info("AutoBet", $"✅ 配置已更新: {config.ConfigName} (ID={config.Id})");
                _log.Info("AutoBet", $"   说明：BindingList 已自动保存到数据库（F5BotV2 设计）");
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
                    lock (_configs) // 🔥 保护数据库操作
                    {
                        _db.Execute("DELETE FROM BetRecord WHERE ConfigId = ?", id);
                    }
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
                // ✅ 不再强制重置 IsEnabled，保留用户上次的设置
                // 原因：现在有"延迟2秒再次判断"机制和老浏览器重连机制，可以安全地保留 IsEnabled 状态
                _log.Info("AutoBet", $"加载默认配置 IsEnabled 状态: {defaultConfig.IsEnabled}");
                
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
                _log.Info("AutoBet", $"✅ 配置信息: {config.ConfigName} (ServerId={configId}, BrowserId={browserConfigId}, {config.Platform})");
                _log.Info("AutoBet", $"   说明：配置名固定，但数据库重建后配置ID可能变化");
                _log.Info("AutoBet", $"   当前连接状态: {(config.IsConnected ? "已连接" : "未连接")}");
                
                // 🔥 保存进程ID到配置
                config.ProcessId = processId;
                SaveConfig(config);
                _log.Info("AutoBet", $"✅ 已保存进程ID: {processId}");
                
                // 🔥 从 AutoBetSocketServer 获取 ClientConnection
                // 关键：必须使用浏览器握手时发送的 browserConfigId 去查找，因为 AutoBetSocketServer 是用它存储的
                _log.Info("AutoBet", $"   查找连接使用的 BrowserConfigId: {browserConfigId}");
                
                var connection = _socketServer?.GetConnection(browserConfigId);
                if (connection == null)
                {
                    _log.Error("AutoBet", $"❌ 无法获取 ClientConnection: BrowserConfigId={browserConfigId}");
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
                    _log.Info("AutoBet", $"📌 配置已有 Browser 实例，清理旧连接并附加新连接");
                    // 🔥 清理旧连接（但不杀进程）
                    try
                    {
                        var oldConnection = browserClient.GetConnection();
                        if (oldConnection != null && oldConnection != connection)
                        {
                            _log.Info("AutoBet", $"   清理旧连接（准备附加新连接）");
                            oldConnection.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Warning("AutoBet", $"   清理旧连接时出错: {ex.Message}");
                    }
                }
                
                // 🔥 如果浏览器和服务端的 configId 不同，需要在 AutoBetSocketServer 中更新映射
                // 🔥 注意：必须在 AttachConnection 之前更新映射，确保 GetConnection 能正确获取
                if (browserConfigId != configId)
                {
                    _log.Info("AutoBet", $"🔄 更新连接映射: BrowserId={browserConfigId} → ServerId={configId}");
                    _socketServer?.UpdateConnectionMapping(browserConfigId, configId);
                    
                    // 🔥 更新映射后，重新从服务器获取连接（因为映射已更新）
                    connection = _socketServer?.GetConnection(configId);
                    if (connection == null)
                    {
                        _log.Error("AutoBet", $"❌ 更新映射后无法获取连接: ConfigId={configId}");
                        _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                        return;
                    }
                    _log.Info("AutoBet", $"✅ 已重新获取连接（映射更新后）");
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
                
                // 🔥 检查并清除 ProcessId
                if (config.ProcessId > 0)
                {
                    // 检查进程是否真的结束了
                    if (IsProcessRunning(config.ProcessId))
                    {
                        _log.Warning("AutoBet", $"⚠️ 浏览器进程 {config.ProcessId} 仍在运行，但连接已断开");
                        _log.Info("AutoBet", $"   可能原因：浏览器崩溃、网络问题、手动关闭窗口但进程未退出");
                        _log.Info("AutoBet", $"   保留 ProcessId，监控任务将等待重连或进程自然退出");
                        // 不清除 ProcessId，让监控任务继续等待进程退出
                    }
                    else
                    {
                        _log.Info("AutoBet", $"🔧 浏览器进程 {config.ProcessId} 已结束，清除 ProcessId");
                        config.ProcessId = 0;
                    }
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
                var betOrders = zhaocaimao.Shared.Parsers.BetContentParser.ParseBetContentToOrderList(betContentStandard, int.Parse(issueId));
                
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
        public void QueueBetOrder(int configId, zhaocaimao.Shared.Models.BetStandardOrderList orders)
        {
            lock (_orderQueues)
            {
                if (!_orderQueues.ContainsKey(configId))
                {
                    _orderQueues[configId] = new Queue<zhaocaimao.Shared.Models.BetStandardOrderList>();
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
        public zhaocaimao.Shared.Models.BetStandardOrderList? GetPendingOrder(int configId, int? issueId)
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
            var config = GetConfig(configId);
            if (config == null)
            {
                _log.Warning("AutoBet", $"❌ GetBrowserClient: 配置不存在 ConfigId={configId}");
                return null;
            }
            
            var browserClient = config.Browser;
            if (browserClient == null)
            {
                _log.Warning("AutoBet", $"❌ GetBrowserClient: BrowserClient 为 null ConfigId={configId}");
                return null;
            }
            
            // 🔥 诊断连接状态
            var connection = browserClient.GetConnection();
            _log.Info("AutoBet", $"📊 GetBrowserClient 诊断: ConfigId={configId}");
            _log.Info("AutoBet", $"   BrowserClient 存在: {browserClient != null}");
            _log.Info("AutoBet", $"   Connection 存在: {connection != null}");
            _log.Info("AutoBet", $"   Connection.IsConnected: {connection?.IsConnected ?? false}");
            _log.Info("AutoBet", $"   BrowserClient.IsConnected: {browserClient.IsConnected}");
            
            return browserClient;
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
                zhaocaimao.Shared.Models.BetStandardOrderList? orders = null;
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
                
                // 🔥 直接调用内部启动方法（不通过监控任务，避免重复启动）
                _log.Info("AutoBet", $"🚀 直接启动浏览器...");
                bool startResult = await StartBrowserInternal(configId);
                
                _log.Info("AutoBet", $"   启动结果: {(startResult ? "✅ 成功" : "❌ 失败")}");
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return startResult;
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
            // 🔥 并发控制：防止同一配置被重复启动
            bool shouldStart = false;
            lock (_lock)
            {
                if (_startingConfigs.Contains(configId))
                {
                    _log.Warning("AutoBet", $"⏳ 配置 {configId} 正在启动中，跳过重复启动");
                    return false;
                }
                _startingConfigs.Add(configId);
                shouldStart = true;  // 标记已添加，需要在 finally 中移除
            }
            
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
                    _log.Info("AutoBet", $"   说明：清理连接但不杀进程，新浏览器启动后旧进程会被系统自动清理");
                    try
                    {
                        config.Browser.Dispose(killProcess: false);  // 🔥 不杀进程，只清理连接
                    }
                    catch (Exception ex)
                    {
                        _log.Warning("AutoBet", $"清理旧 BrowserClient 时出错: {ex.Message}");
                    }
                    config.Browser = null;
                }
                
                // 🔥 清除旧的 ProcessId（让新浏览器使用新的 ProcessId）
                if (config.ProcessId > 0)
                {
                    _log.Info("AutoBet", $"🧹 清除旧的 ProcessId: {config.ProcessId}（新浏览器将使用新的 ProcessId）");
                    config.ProcessId = 0;
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
            finally
            {
                // 🔥 移除启动标记（确保即使异常也能清除）
                if (shouldStart)
                {
                    lock (_lock)
                    {
                        _startingConfigs.Remove(configId);
                    }
                }
            }
        }
        
        /// <summary>
        /// 投注
        /// </summary>
        public async Task<BetResult> PlaceBet(int configId, zhaocaimao.Shared.Models.BetStandardOrderList orders)
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
                
                browserClient.Dispose(killProcess: true);  // 🔥 明确要求杀死进程
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
        /// 🔥 监控线程循环（使用专用线程 + while 循环，精确控制时机）
        /// </summary>
        private void MonitorBrowsersLoop()
        {
            try
            {
                _log.Info("AutoBet", "🚀 监控线程立即开始运行（用户需求：立即启动，但检测到需要启动浏览器时，先延迟2秒再次判断）");
                _log.Info("AutoBet", "✅ 监控线程已启动，开始循环检查...");
                
                // 🔥 主循环：每2秒检查一次
                while (_monitorRunning)
                {
                    try
                    {
                        // 🔥 执行监控任务
                        MonitorBrowsers();
                    }
                    catch (Exception ex)
                    {
                        _log.Error("AutoBet", "监控任务执行异常", ex);
                    }
                    
                    // 🔥 等待2秒再执行下一次
                    // 注意：这里是任务执行完后等待2秒，不是从上次开始计时
                    Thread.Sleep(2000);
                }
                
                _log.Info("AutoBet", "⏹️ 监控线程已停止");
            }
            catch (ThreadAbortException)
            {
                _log.Info("AutoBet", "⏹️ 监控线程被中止");
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", "监控线程异常退出", ex);
            }
        }
        
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
        /// 🔥 现在由专用线程调用，不再是 Timer 回调
        /// </summary>
        private void MonitorBrowsers()
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
                
                // 🔥 简化日志：只在有问题时才输出
                foreach (var config in enabledConfigs)
                {
                    // 🔥 检查连接状态（配置对象自己管理）
                    if (config.IsConnected)
                    {
                        // 已连接，跳过
                        continue;
                    }
                    
                    // 🔥 诊断日志：输出未连接配置的关键状态
                    _log.Debug("AutoBet", $"🔍 检查配置 [{config.ConfigName}]:");
                    _log.Debug("AutoBet", $"   IsEnabled={config.IsEnabled}, IsConnected={config.IsConnected}");
                    _log.Debug("AutoBet", $"   ProcessId={config.ProcessId}, Browser={(config.Browser != null ? "存在" : "null")}");
                    
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
                    if (config.ProcessId > 0)
                    {
                        if (IsProcessRunning(config.ProcessId))
                        {
                            _log.Info("AutoBet", $"⏳ 配置 [{config.ConfigName}] 浏览器进程 {config.ProcessId} 仍在运行，等待重连...");
                            continue;  // 🔥 进程还在，不启动新的
                        }
                        else
                        {
                            // 🔥 进程已结束，清除 ProcessId
                            _log.Info("AutoBet", $"🔧 配置 [{config.ConfigName}] 浏览器进程 {config.ProcessId} 已结束，清除 ProcessId");
                            config.ProcessId = 0;
                        }
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
                    
                    // 🔥 异步处理（不阻塞监控线程）
                    int configId = config.Id;
                    string configName = config.ConfigName;
                    int processId = config.ProcessId;
                    
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // 🔥 【核心优化】无论 ProcessId 是否为0，都先等待2秒给老浏览器重连的机会
                            // 这是用户的核心需求：先延时2秒，再次判断，再启动
                            _log.Info("AutoBet", $"⏳ [{configName}] 检测到未连接（ProcessId={processId}），延迟2秒再次检查连接状态...");
                            await Task.Delay(2000);
                            
                            // 🔥 【关键检查1】等待后再次检查连接状态
                            var cfgCheck = GetConfig(configId);
                            if (cfgCheck?.IsConnected == true)
                            {
                                _log.Info("AutoBet", $"✅ [{configName}] 浏览器已在2秒内重连成功，取消启动");
                                return;
                            }
                            
                            // 🔥 【关键检查2】再次检查 IsEnabled（可能用户在等待期间关闭了）
                            if (cfgCheck == null || !cfgCheck.IsEnabled)
                            {
                                _log.Info("AutoBet", $"   [{configName}] IsEnabled=false，取消启动");
                                return;
                            }
                            
                            // 🔥 【关键检查3】如果还有进程ID，再次检查进程是否真的已结束
                            if (cfgCheck.ProcessId > 0 && IsProcessRunning(cfgCheck.ProcessId))
                            {
                                _log.Warning("AutoBet", $"⚠️ [{configName}] 浏览器进程 {cfgCheck.ProcessId} 仍在运行但未连接");
                                _log.Warning("AutoBet", $"   保留 ProcessId，等待下次检查");
                                return;
                            }
                            
                            // 🔥 确认未连接且需要启动，启动浏览器
                            _log.Info("AutoBet", $"🚀 [{configName}] 延迟2秒后确认未连接，开始启动新浏览器");
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
        
        /// <summary>
        /// 🔥 检查进程是否还在运行
        /// 使用 Process.GetProcessById + HasExited 双重检查，确保准确性
        /// </summary>
        private bool IsProcessRunning(int processId)
        {
            try
            {
                // 🔥 第一步：通过 ProcessId 获取进程对象
                // 如果进程不存在，GetProcessById 会抛出 ArgumentException
                var process = System.Diagnostics.Process.GetProcessById(processId);
                
                // 🔥 第二步：检查进程是否已退出
                // HasExited 返回 true 表示进程已结束
                // 注意：这里也可能抛出异常（进程在获取后立即退出）
                bool hasExited = process.HasExited;
                
                // 🔥 第三步：额外检查进程名称（可选，增强可靠性）
                // 确保这不是一个被回收的 ProcessId（Windows 可能复用 PID）
                if (!hasExited)
                {
                    try
                    {
                        // 尝试访问进程名称，如果进程已死，会抛出异常
                        var _ = process.ProcessName;
                    }
                    catch
                    {
                        // 进程已死亡但 HasExited 未更新
                        return false;
                    }
                }
                
                return !hasExited;
            }
            catch (ArgumentException)
            {
                // ProcessId 不存在
                return false;
            }
            catch (InvalidOperationException)
            {
                // 进程已退出
                return false;
            }
            catch (Exception ex)
            {
                // 其他异常（例如权限问题）
                _log.Warning("AutoBet", $"⚠️ 检查进程 {processId} 时发生异常: {ex.Message}");
                return false;
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
                
                // 🔥 步骤2: 停止监控线程（防止新的任务启动）
                if (_monitorThread != null)
                {
                    _log.Info("AutoBet", "⏹️ 停止监控线程...");
                    _monitorRunning = false;  // 🔥 设置标志，让线程自然退出
                    
                    // 🔥 等待线程结束（最多等待3秒）
                    if (!_monitorThread.Join(3000))
                    {
                        _log.Warning("AutoBet", "⚠️ 监控线程未在3秒内结束，继续释放资源");
                    }
                    else
                    {
                        _log.Info("AutoBet", "✅ 监控线程已停止");
                    }
                    _monitorThread = null;
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

