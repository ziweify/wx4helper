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
        
        private readonly object _lock = new object();
        
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
                                
                                // 控件方式：不需要检查 Socket 连接
                                // 浏览器控件在 StartAsync 时已经初始化
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
            
            _log.Info("AutoBet", "ℹ️ 监控线程已废弃，浏览器由 IsEnabled 属性直接管理");
            
            // 🔥 检查是否有启用的配置需要启动浏览器
            int enabledCount = 0;
            foreach (var config in _configs)
            {
                if (config.IsEnabled && !config.IsConnected)
                {
                    enabledCount++;
                    _log.Info("AutoBet", $"🚀 检测到配置 [{config.ConfigName}] 已启用但浏览器未启动，立即启动...");
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await config.StartBrowserManuallyAsync();
                        }
                        catch (Exception ex)
                        {
                            _log.Error("AutoBet", $"❌ 启动浏览器失败: {config.ConfigName}", ex);
                        }
                    });
                }
            }
            
            if (enabledCount > 0)
            {
                _log.Info("AutoBet", $"✅ 已为 {enabledCount} 个配置启动浏览器");
            }
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
                
                // 🔥 控件方式：不再需要 Socket 连接处理
                // 浏览器控件在 StartAsync 时已经初始化，直接检查状态即可
                _log.Info("AutoBet", $"ℹ️ 控件方式：浏览器已通过 StartAsync 初始化，无需 Socket 连接");
                
                var browserControl = config.Browser;
                if (browserControl != null && browserControl.IsInitialized)
                {
                    config.Status = "已连接";
                    SaveConfig(config);
                    _log.Info("AutoBet", $"✅ 浏览器控件已连接: {config.ConfigName}");
                }
                else
                {
                    _log.Warning("AutoBet", $"⚠️ 浏览器控件未连接: {config.ConfigName}");
                }
                
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
                    // 🔥 监控线程已移除，不再需要并发控制
                    // 浏览器由 IsEnabled 属性直接管理，内部已有防重复启动机制
                    
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
                // 控件方式：不需要消息处理，命令直接调用 ExecuteCommandAsync
                // if (config?.Browser != null)
                // {
                //     config.Browser.OnMessageReceived(message);
                // }
                
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
            var browserControl = config?.Browser;
            if (browserControl == null || !browserControl.IsInitialized)
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
                
                await browserControl.ExecuteCommandAsync("封盘通知", data);
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
            
            var browserControl = config.Browser;
            if (browserControl == null)
            {
                _log.Warning("AutoBet", $"❌ 浏览器未连接，无法推送投注命令: {config.ConfigName}");
                _log.Warning("AutoBet", $"   监控任务会在3秒内自动检查并重启浏览器");
                
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "浏览器未连接(监控任务将自动重连)"
                };
            }
            
            // 🔥 检查连接状态（控件方式）
            if (!browserControl.IsInitialized)
            {
                _log.Error("AutoBet", $"❌ 浏览器控件未初始化或已断开");
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "浏览器控件未初始化"
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
                
                // 🔥 直接调用浏览器控件的命令方法
                var result = await browserControl.ExecuteCommandAsync("投注", betOrders);
                
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
        /// 获取浏览器控件（供命令面板使用）
        /// </summary>
        public UserControls.BetBrowserControl? GetBrowserControl(int configId)
        {
            var config = GetConfig(configId);
            if (config == null)
            {
                _log.Warning("AutoBet", $"❌ GetBrowserControl: 配置不存在 ConfigId={configId}");
                return null;
            }
            
            var browserControl = config.Browser;
            if (browserControl == null)
            {
                _log.Warning("AutoBet", $"❌ GetBrowserControl: BrowserControl 为 null ConfigId={configId}");
                return null;
            }
            
            // 🔥 诊断连接状态（控件方式）
            _log.Info("AutoBet", $"📊 GetBrowserControl 诊断: ConfigId={configId}");
            _log.Info("AutoBet", $"   BrowserControl 存在: {browserControl != null}");
            _log.Info("AutoBet", $"   BrowserControl.IsInitialized: {browserControl.IsInitialized}");
            
            return browserControl;
        }
        
        /// <summary>
        /// 获取浏览器客户端（兼容方法，返回包装器）
        /// </summary>
        [Obsolete("使用 GetBrowserControl 代替")]
        public BrowserClient? GetBrowserClient(int configId)
        {
            var control = GetBrowserControl(configId);
            if (control == null)
                return null;
            
            // 返回一个包装器，保持兼容性
            return new BrowserClientWrapper(configId, control);
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
            // 🔥 监控线程已移除，不再需要并发控制
            // 浏览器由 IsEnabled 属性直接管理，内部已有防重复启动机制
            bool shouldStart = true;
            
            try
            {
                _log.Info("AutoBet", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _log.Info("AutoBet", $"🚀 启动浏览器控件 ConfigId={configId}");
                
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
                
                // 🔥 清理旧的浏览器控件（如果存在）
                if (config.Browser != null)
                {
                    _log.Info("AutoBet", $"🧹 清理旧的浏览器控件（准备启动新浏览器）");
                    try
                    {
                        config.Browser.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _log.Warning("AutoBet", $"清理旧浏览器控件时出错: {ex.Message}");
                    }
                    config.Browser = null;
                }
                
                // 控件方式：不需要 ProcessId
                
                _log.Info("AutoBet", $"📋 配置信息: {config.ConfigName} ({config.Platform})");
                _log.Info("AutoBet", $"🚀 启动新浏览器控件: {config.ConfigName}");
                _log.Info("AutoBet", $"   ConfigId: {configId}");
                _log.Info("AutoBet", $"   平台: {config.Platform}");
                _log.Info("AutoBet", $"   URL: {config.PlatformUrl}");
                
                // 🔥 直接创建浏览器控件
                var newBrowserControl = new UserControls.BetBrowserControl();
                
                // 订阅日志事件
                newBrowserControl.OnLog += (msg) => _log.Info("AutoBet", $"[{config.ConfigName}] {msg}");
                
                // 🔥 先设置到配置（在初始化之前，这样监控线程可以看到对象存在）
                config.Browser = newBrowserControl;
                _log.Info("AutoBet", $"✅ 浏览器控件已设置到配置对象");
                
                // 初始化浏览器控件
                await newBrowserControl.InitializeAsync(configId, config.ConfigName, config.Platform, config.PlatformUrl);
                _log.Info("AutoBet", $"✅ 浏览器控件已初始化");
                
                // 更新状态
                config.Status = "已启动";
                SaveConfig(config);
                
                // 🔥 控件方式：直接检查是否已初始化（不需要等待 Socket 连接）
                _log.Info("AutoBet", $"⏳ 等待浏览器控件初始化...");
                
                // 🔥 等待初始化完成，最多等待5秒
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(500);
                    
                    // 检查连接状态
                    if (config.IsConnected)
                    {
                        _log.Info("AutoBet", $"✅ 浏览器控件已初始化！等待时间: {i * 0.5}秒");
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
                    _log.Warning("AutoBet", $"⚠️ 浏览器控件尚未初始化（等待5秒后）");
                    _log.Warning("AutoBet", $"   当前 BrowserControl.IsInitialized: {newBrowserControl.IsInitialized}");
                }
                
                // 4️⃣ 自动登录
                if (config.AutoLogin && !string.IsNullOrEmpty(config.Username))
                {
                    _log.Info("AutoBet", $"🔐 自动登录: {config.Username}");
                    var loginResult = await newBrowserControl.ExecuteCommandAsync("Login", new
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
                // 🔥 监控线程已移除，不再需要移除启动标记
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
            
            var browserControl = config.Browser;
            if (browserControl == null || !browserControl.IsInitialized)
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
            var result = await browserControl.ExecuteCommandAsync("投注", orders);
            
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
                _log.Info("AutoBet", $"   正在关闭浏览器控件...");
                
                config!.Browser?.Dispose();  // 🔥 释放浏览器控件
                config.Browser = null;  // 🔥 配置对象清除 Browser 引用
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
        
        // 🔥 监控线程已移除，浏览器由 IsEnabled 属性直接管理
        
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
                
                // 🔥 监控线程已移除，无需停止
                
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

