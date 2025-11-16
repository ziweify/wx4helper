using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using zhaocaimao.Contracts;
using zhaocaimao.Contracts.Games;
using zhaocaimao.Models;
using zhaocaimao.Models.Games.Binggo;
using zhaocaimao.Models.Games.Binggo.Events;
using zhaocaimao.Core;
using zhaocaimao.Helpers;
using SQLite;

namespace zhaocaimao.Services.Games.Binggo
{
    /// <summary>
    /// 炳狗开奖服务实现
    /// 
    /// 核心功能：
    /// 1. 定时获取开奖数据（每秒）
    /// 2. 期号变更检测
    /// 3. 倒计时计算
    /// 4. 状态变更事件触发
    /// 5. 本地缓存管理（先查本地，没有再请求网络）
    /// </summary>
    /// <summary>
    /// 炳狗开奖服务实现
    /// 
    /// 🔥 核心设计（参考 F5BotV2 的 BoterServices）：
    /// 1. 统一管理开奖、结算、发送微信消息等所有逻辑
    /// 2. 事件驱动：期号变更、状态变更、开奖等事件统一在这里分发
    /// 3. 封盘、开盘、开奖、结算、期号变更等事件都在这个类中处理
    /// 4. 高内聚、低耦合，便于复用和维护
    /// </summary>
    public class BinggoLotteryService : IBinggoLotteryService
    {
        private readonly ILogService _logService;
        private readonly BinggoGameSettings _settings;
        private readonly IConfigurationService _configService;
        private SQLiteConnection? _db;
        private Core.BinggoLotteryDataBindingList? _bindingList;  // 🔥 UI 数据绑定
        
        // 🔥 业务依赖（用于结算和发送微信消息）
        private IBinggoOrderService? _orderService;
        private IGroupBindingService? _groupBindingService;
        private IWeixinSocketClient? _socketClient;
        private Core.V2OrderBindingList? _ordersBindingList;
        private Core.V2MemberBindingList? _membersBindingList;
        private Core.V2CreditWithdrawBindingList? _creditWithdrawsBindingList;  // 🔥 上下分 BindingList
        private BinggoStatisticsService? _statisticsService;  // 🔥 统计服务（用于更新统计）
        
        private System.Threading.Timer? _timer;
        private int _currentIssueId;
        private BinggoLotteryStatus _currentStatus = BinggoLotteryStatus.等待中;
        private int _secondsToSeal;
        private bool _isRunning;
        private readonly object _lock = new object();
        
        // 🔥 时间提醒标志（防止重复触发，参考 F5BotV2）
        private bool _reminded30Seconds = false;
        private bool _reminded15Seconds = false;
        
        // 🔥 开盘消息发送标志（防止同一期号重复发送"线下开始"消息）
        private int _lastOpeningIssueId = 0;
        
        // 🔥 上一期结算完成标志（确保"线下开始"消息在"留~名单"消息之后发送）
        private int _lastSettledIssueId = 0;
        
        // 🔥 开奖队列（参考 F5BotV2 的 itemUpdata）
        // 期号变更时，上期要开奖的期号进入队列，后台线程永远拿最新一条消息来开奖（处理卡奖情况）
        private readonly ConcurrentDictionary<int, BinggoLotteryData> _lotteryQueue = new ConcurrentDictionary<int, BinggoLotteryData>();
        private CancellationTokenSource? _queueCheckCts;
        private Task? _queueCheckTask;
        
        // 事件
        public event EventHandler<BinggoIssueChangedEventArgs>? IssueChanged;
        public event EventHandler<BinggoStatusChangedEventArgs>? StatusChanged;
        public event EventHandler<BinggoCountdownEventArgs>? CountdownTick;
        public event EventHandler<BinggoLotteryOpenedEventArgs>? LotteryOpened;
        
        // 属性
        public int CurrentIssueId => _currentIssueId;
        public BinggoLotteryStatus CurrentStatus => _currentStatus;
        public int SecondsToSeal => _secondsToSeal;
        public bool IsRunning => _isRunning;
        
        public BinggoLotteryService(
            ILogService logService,
            BinggoGameSettings settings,
            IConfigurationService configService)
        {
            _logService = logService;
            _settings = settings;
            _configService = configService;
        }
        
        /// <summary>
        /// 🔥 检查是否应该发送系统消息
        /// </summary>
        /// <returns>true = 应该发送，false = 不应该发送</returns>
        private bool ShouldSendSystemMessage()
        {
            // 如果收单开关关闭，且设置了"收单关闭时不发送系统消息"
            if (!_configService.GetIsOrdersTaskingEnabled() && 
                _configService.Get收单关闭时不发送系统消息())
            {
                _logService.Debug("BinggoLotteryService", "⏸️ 收单已关闭且设置了不发送系统消息，跳过发送");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 🔥 设置业务依赖（用于结算和发送微信消息）
        /// </summary>
        public void SetBusinessDependencies(
            IBinggoOrderService? orderService,
            IGroupBindingService? groupBindingService,
            IWeixinSocketClient? socketClient,
            Core.V2OrderBindingList? ordersBindingList,
            Core.V2MemberBindingList? membersBindingList,
            Core.V2CreditWithdrawBindingList? creditWithdrawsBindingList,
            BinggoStatisticsService? statisticsService = null)  // 🔥 统计服务（可选）
        {
            _orderService = orderService;
            _groupBindingService = groupBindingService;
            _socketClient = socketClient;
            _ordersBindingList = ordersBindingList;
            _membersBindingList = membersBindingList;
            _creditWithdrawsBindingList = creditWithdrawsBindingList;  // 🔥 设置上下分 BindingList
            _statisticsService = statisticsService;  // 🔥 设置统计服务
            _logService.Info("BinggoLotteryService", "✅ 业务依赖已设置");
        }
        
        /// <summary>
        /// 🔥 设置数据库连接（用于上下分申请）
        /// </summary>
        public void SetDatabaseForCreditWithdraw(SQLiteConnection? db)
        {
            _db = db;
            if (_db != null)
            {
                _db.CreateTable<Models.V2CreditWithdraw>();
                _logService.Info("BinggoLotteryService", "✅ 上下分数据库已设置");
            }
        }
        
        /// <summary>
        /// 设置数据库连接（用于本地缓存）
        /// </summary>
        public void SetDatabase(SQLiteConnection? db)
        {
            _db = db;
            _db?.CreateTable<BinggoLotteryData>();
            _logService.Info("BinggoLotteryService", "数据库已设置，开奖数据表已创建");
        }
        
        /// <summary>
        /// 设置 BindingList 用于自动 UI 更新
        /// </summary>
        public void SetBindingList(BinggoLotteryDataBindingList? bindingList)
        {
            _bindingList = bindingList;
            _logService.Info("BinggoLotteryService", "BindingList 已设置，开奖数据将自动更新到 UI");
        }
        
        public async Task StartAsync()
        {
            if (_isRunning)
            {
                _logService.Warning("BinggoLotteryService", "服务已在运行中");
                return;
            }
            
            // 🔥 防御性检查：确保数据库已设置
            if (_db == null)
            {
                _logService.Error("BinggoLotteryService", "❌ 数据库未设置，无法启动服务！请先调用 SetDatabase()");
                return;
            }
            
            _logService.Info("BinggoLotteryService", "🚀 开奖服务启动");
            _isRunning = true;
            
            // 立即执行一次
            await OnTimerTickAsync();
            
            // 启动定时器（每 1 秒）
            _timer = new System.Threading.Timer(
                callback: async _ => await OnTimerTickAsync(),
                state: null,
                dueTime: TimeSpan.FromSeconds(1),
                period: TimeSpan.FromSeconds(1)
            );
            
            // 🔥 启动开奖队列检查线程（参考 F5BotV2）
            _queueCheckCts = new CancellationTokenSource();
            _queueCheckTask = Task.Run(() => CheckLotteryQueueAsync(_queueCheckCts.Token), _queueCheckCts.Token);
            _logService.Info("BinggoLotteryService", "✅ 开奖队列检查线程已启动");
        }
        
        public Task StopAsync()
        {
            _logService.Info("BinggoLotteryService", "🛑 开奖服务停止");
            _isRunning = false;
            _timer?.Dispose();
            _timer = null;
            
            // 🔥 停止开奖队列检查线程
            _queueCheckCts?.Cancel();
            if (_queueCheckTask != null)
            {
                try
                {
                    _queueCheckTask.Wait(TimeSpan.FromSeconds(2));
                }
                catch (Exception ex)
                {
                    _logService.Warning("BinggoLotteryService", $"停止队列检查线程异常: {ex.Message}");
                }
            }
            _queueCheckCts?.Dispose();
            _queueCheckCts = null;
            _queueCheckTask = null;
            
            return Task.CompletedTask;
        }
        
        // ========================================
        // 🔥 核心定时器逻辑
        // ========================================
        
        private async Task OnTimerTickAsync()
        {
            if (!_isRunning) return;
            
            try
            {
                // ========================================
                // 🔥 步骤1: 使用本地计算获取当前期号（始终可用）
                // ========================================
                int localIssueId = BinggoTimeHelper.GetCurrentIssueId();
                
                // 🔥 关键区分：
                // 1. secondsToOpen = 距离开奖的真实倒计时（用于显示）
                // 2. secondsToSeal = 距离封盘的倒计时（用于状态判断）
                int secondsToOpen = BinggoTimeHelper.GetSecondsToOpen(localIssueId);
                int secondsToSeal = secondsToOpen - _settings.SealSecondsAhead;
                
                lock (_lock)
                {
                    // 🔥 检查期号变更（首次初始化也走统一流程）
                    if (localIssueId != _currentIssueId)
                    {
                        int previousIssueId = _currentIssueId;
                        
                        if (_currentIssueId == 0)
                        {
                            // 🔥 首次初始化：计算上一期
                            previousIssueId = BinggoTimeHelper.GetPreviousIssueId(localIssueId);
                            _logService.Info("BinggoLotteryService", $"✅ 首次初始化: 当前期号={localIssueId}, 上期期号={previousIssueId}");
                        }
                        else
                        {
                            _logService.Info("BinggoLotteryService", $"🔄 期号变更: {previousIssueId} → {localIssueId}");
                        }
                        
                        // 🔥 统一的期号切换流程（首次初始化和期号变更都走这里）
                        _currentIssueId = localIssueId;
                        _ = HandleIssueChangeAsync(previousIssueId, localIssueId);
                    }
                    
                    // 🔥 更新倒计时（存储真实的到开奖时间）
                    _secondsToSeal = secondsToOpen;  // 实际上应该改名为 _secondsToOpen
                    
                    // 🔥 检查状态变更（使用到封盘的时间）
                    UpdateStatus(secondsToSeal);
                    
                    // 🔥 触发倒计时事件（显示真实的到开奖时间）
                    CountdownTick?.Invoke(this, new BinggoCountdownEventArgs
                    {
                        Seconds = secondsToOpen,  // 显示到开奖的时间
                        IssueId = _currentIssueId
                    });
                }
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"定时器执行异常: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 处理期号变更（新版 - 异步）
        /// 🔥 重要：只维护当前期号的状态，上期数据只是异步加载显示
        /// 参考 F5BotV2 第983行：期号变更时，如果上一期还没开奖，状态设置为"开奖中"
        /// </summary>
        private async Task HandleIssueChangeAsync(int oldIssueId, int newIssueId)
        {
            try
            {
                _logService.Info("BinggoLotteryService", $"🔄 期号变更: {oldIssueId} → {newIssueId}");
                
                // 🔥 期号变更时，重置开盘消息发送标志（新期号可以发送"线下开始"消息）
                _lastOpeningIssueId = 0;
                
                // 🔥 创建上期数据（用于 UcBinggoDataLast 显示）
                var dataLast = new BinggoLotteryData
                {
                    IssueId = oldIssueId,
                    OpenTime = BinggoTimeHelper.GetIssueOpenTime(oldIssueId).ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                _logService.Info("BinggoLotteryService", $"📢 期号变更事件: 当期={newIssueId}, 上期={oldIssueId}");
                _logService.Info("BinggoLotteryService", $"   当期开奖时间: {BinggoTimeHelper.GetIssueOpenTime(newIssueId):HH:mm:ss}");
                _logService.Info("BinggoLotteryService", $"   上期开奖时间: {BinggoTimeHelper.GetIssueOpenTime(oldIssueId):HH:mm:ss}");
                
                // 🔥 触发期号变更事件（同时传递当期和上期数据）
                IssueChanged?.Invoke(this, new BinggoIssueChangedEventArgs
                {
                    OldIssueId = oldIssueId,
                    NewIssueId = newIssueId,
                    LastLotteryData = dataLast  // 上期数据（号码为空，显示为 ✱）
                });
                
                // 🔥 期号变更时，上期要开奖的期号进入开奖队列（参考 F5BotV2）
                // 创建一个空的 BinggoLotteryData 对象，IssueId 为 0 表示还未获取到开奖数据
                var queueData = new BinggoLotteryData { IssueId = 0 };
                _lotteryQueue.AddOrUpdate(oldIssueId, queueData, (key, oldValue) => queueData);
                _logService.Info("BinggoLotteryService", $"📥 期号 {oldIssueId} 已加入开奖队列");
                
                // 🔥 期号变更时，如果上一期还没开奖，状态设置为"开奖中"（参考 F5BotV2 第983行 On开奖中）
                // 检查上一期是否已开奖
                var lastData = await GetLotteryDataAsync(oldIssueId, forceRefresh: false);
                if (lastData == null || !lastData.IsOpened)
                {
                    // 上一期还没开奖，状态设置为"开奖中"
                    var oldStatus = _currentStatus;
                    _currentStatus = BinggoLotteryStatus.开奖中;
                    _logService.Info("BinggoLotteryService", $"🎲 上一期({oldIssueId})尚未开奖，状态设置为: 开奖中");
                    
                    StatusChanged?.Invoke(this, new BinggoStatusChangedEventArgs
                    {
                        OldStatus = oldStatus,
                        NewStatus = BinggoLotteryStatus.开奖中,
                        IssueId = oldIssueId,
                        Message = "等待上期开奖"
                    });
                }
                
                // 🔥 异步加载上期开奖数据（作为备用方案）
                // 当数据到达时，会触发 LotteryOpened 事件，UI 会再次更新
                await LoadPreviousLotteryDataAsync(oldIssueId);
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"期号变更处理异常: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 开奖队列检查线程（参考 F5BotV2 的更新队列线程）
        /// 永远拿最新一条消息来开奖，处理官方卡奖情况
        /// </summary>
        private async Task CheckLotteryQueueAsync(CancellationToken cancellationToken)
        {
            _logService.Info("BinggoLotteryService", "🔄 开奖队列检查线程已启动");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_lotteryQueue.Count > 0)
                    {
                        // 🔥 永远拿最新一条消息（参考 F5BotV2: itemUpdata.LastOrDefault()）
                        var lastItem = _lotteryQueue.OrderByDescending(kvp => kvp.Key).FirstOrDefault();
                        
                        if (lastItem.Key > 0 && lastItem.Value != null)
                        {
                            int queueIssueId = lastItem.Key;
                            BinggoLotteryData queueData = lastItem.Value;
                            
                            // 🔥 如果队列中的期号和实际获取到的开奖数据的期号不一致，说明还没有获取到开奖数据
                            // 参考 F5BotV2: if(item.Key != item.Value.IssueId)
                            if (queueData.IssueId == 0 || queueData.IssueId != queueIssueId || !queueData.IsOpened)
                            {
                                _logService.Info("BinggoLotteryService", $"📡 检查开奖队列: 期号 {queueIssueId} 尚未开奖，请求API...");
                                
                                // 🔥 调用API获取开奖数据
                                var api = Services.Api.BoterApi.GetInstance();
                                var response = await api.GetBgDataAsync(queueIssueId);
                                
                                if (response.Code == 0 && response.Data != null && response.Data.IsOpened)
                                {
                                    var openedData = response.Data;
                                    
                                    _logService.Info("BinggoLotteryService", $"✅ 获取到开奖数据: {queueIssueId} - {openedData.ToLotteryString()}");
                                    
                                    // 从队列中移除
                                    _lotteryQueue.TryRemove(queueIssueId, out _);
                                    
                                    // 保存到数据库
                                    if (_db != null)
                                    {
                                        try
                                        {
                                            _db.InsertOrReplace(openedData);
                                            _bindingList?.LoadFromDatabase(100);
                                        }
                                        catch (SQLite.SQLiteException ex) when (ex.Message.Contains("no such table"))
                                        {
                                            // 🔥 表不存在（可能是数据库刚初始化），忽略错误
                                            _logService.Warning("BinggoLotteryService", $"保存开奖数据失败，表不存在: {ex.Message}");
                                        }
                                    }
                                    
                    // 🔥 处理开奖（参考 F5BotV2: On已开奖(bgData)）
                    // 统一在这里处理：结算、发送微信消息、清空投注金额等
                    await OnLotteryOpenedAsync(openedData);
                    
                    // 🔥 开奖后，状态变为"等待中"（参考 F5BotV2 第1076行）
                    // 然后在状态循环中，当满足条件时会变成"开盘中"
                    if (_currentStatus == BinggoLotteryStatus.开奖中)
                    {
                        var oldStatus = _currentStatus;
                        _currentStatus = BinggoLotteryStatus.等待中;
                        _logService.Info("BinggoLotteryService", $"✅ 开奖完成，状态从'开奖中'变为'等待中'");
                        
                        StatusChanged?.Invoke(this, new BinggoStatusChangedEventArgs
                        {
                            OldStatus = oldStatus,
                            NewStatus = BinggoLotteryStatus.等待中,
                            IssueId = openedData.IssueId,
                            Message = "开奖完成，等待下一期"
                        });
                        
                        // 🔥 开奖完成后，检查是否需要发送下一期的开盘提示
                        // 只有在结算、发送中~名单、留~名单完成后，才发送下一期的"线下开始"
                        // 参考 F5BotV2：开奖后状态变为"等待中"，然后在状态循环中变为"开盘中"时发送
                        // 这里不立即发送，让状态循环自然触发
                    }
                                }
                                else
                                {
                                    _logService.Debug("BinggoLotteryService", $"⏳ 期号 {queueIssueId} 尚未开奖，等待下次检查...");
                                }
                            }
                            else
                            {
                                // 已经开奖，从队列中移除
                                _lotteryQueue.TryRemove(queueIssueId, out _);
                                _logService.Info("BinggoLotteryService", $"✅ 期号 {queueIssueId} 已开奖，从队列中移除");
                            }
                        }
                    }
                    
                    // 每1秒检查一次
                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logService.Info("BinggoLotteryService", "🛑 开奖队列检查线程已取消");
                    break;
                }
                catch (Exception ex)
                {
                    _logService.Error("BinggoLotteryService", $"开奖队列检查异常: {ex.Message}", ex);
                    await Task.Delay(1000, cancellationToken); // 异常时等待1秒后继续
                }
            }
            
            _logService.Info("BinggoLotteryService", "🛑 开奖队列检查线程已退出");
        }
        
        /// <summary>
        /// 加载上期数据（本地优先 + API补充）
        /// 🔥 如果未开奖，自动轮询直到获取到结果
        /// </summary>
        private async Task LoadPreviousLotteryDataAsync(int issueId)
        {
            try
            {
                // 步骤1: 先查本地
                BinggoLotteryData? data = null;
                if (_db != null)
                {
                    data = _db.Table<BinggoLotteryData>()
                        .Where(d => d.IssueId == issueId)
                        .FirstOrDefault();
                }
                
                // 步骤2: 如果本地有完整数据，直接返回
                // 🔥 注意：不在这里触发开奖事件，开奖事件只由队列检查线程触发（参考 F5BotV2）
                if (data != null && !string.IsNullOrEmpty(data.LotteryData))
                {
                    _logService.Info("BinggoLotteryService", $"💾 本地已有开奖数据: {issueId}");
                    // 如果该期号在队列中，队列检查线程会自动处理并触发开奖事件
                    return;
                }
                
                // 步骤3: 🔥 自动轮询获取开奖数据（参考 F5BotV2）
                int retryCount = 0;
                int maxRetries = 12;  // 最多重试12次（约60秒）
                int retryIntervalSeconds = 5;  // 每5秒重试一次
                
                while (retryCount < maxRetries)
                {
                    _logService.Info("BinggoLotteryService", $"📡 第 {retryCount + 1}/{maxRetries} 次请求开奖数据: {issueId}");
                    
                    // 🔥 使用 BoterApi 单例
                    var api = Services.Api.BoterApi.GetInstance();
                    var response = await api.GetBgDataAsync(issueId);
                    
                    // 🔥 BoterApi 已经返回解析好的 BinggoLotteryData
                    if (response.Code == 0 && response.Data != null && response.Data.IsOpened)
                    {
                        data = response.Data;
                        
                        // 保存到数据库
                        if (_db != null)
                        {
                            _db.InsertOrReplace(data);
                            _bindingList?.LoadFromDatabase(100);
                            _logService.Info("BinggoLotteryService", $"✅ 开奖数据已保存: {issueId} - {data.ToLotteryString()}");
                        }
                        
                        // 🔥 注意：不在这里触发开奖事件，开奖事件只由队列检查线程触发（参考 F5BotV2）
                        // 如果该期号在队列中，队列检查线程会自动处理并触发开奖事件
                        
                        return;  // 成功获取，退出轮询
                    }
                    
                    // 未获取到数据，等待后重试
                    retryCount++;
                    if (retryCount < maxRetries)
                    {
                        _logService.Info("BinggoLotteryService", $"⏳ 暂无开奖数据，{retryIntervalSeconds}秒后重试...");
                        await Task.Delay(retryIntervalSeconds * 1000);
                    }
                }
                
                _logService.Warning("BinggoLotteryService", $"❌ 轮询超时，未能获取开奖数据: {issueId}");
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"加载开奖数据异常: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 状态更新（基于倒计时）
        /// 🔥 完全参考 F5BotV2 的实现逻辑
        /// </summary>
        private void UpdateStatus(int secondsToSeal)
        {
            var oldStatus = _currentStatus;
            BinggoLotteryStatus newStatus;
            
            // ========================================
            // 🔥 根据倒计时判断状态（本地计算）
            // ========================================
            
            // 🔥 如果当前状态是"开奖中"，不能直接变成"开盘中"，必须先变成"等待中"
            // 只有在"等待中"状态时，才能根据倒计时变成"开盘中"
            if (oldStatus == BinggoLotteryStatus.开奖中)
            {
                // 开奖中状态时，不更新状态，等待开奖完成后再更新
                return;
            }
            
            if (secondsToSeal > 30)
            {
                // 开盘中（距离封盘超过 30 秒）
                newStatus = BinggoLotteryStatus.开盘中;
                
                // 🔥 只在第一次进入"开盘中"状态时执行 On开盘中 逻辑（参考 F5BotV2 第1139-1178行）
                if (oldStatus != BinggoLotteryStatus.开盘中)
                {
                    _ = Task.Run(async () => await OnOpeningAsync(_currentIssueId));
                }
            }
            else if (secondsToSeal > 0)
            {
                // 即将封盘（0-30 秒）
                newStatus = BinggoLotteryStatus.即将封盘;
                
                // ========================================
                // 🔥 30 秒提醒（参考 F5BotV2: sec < 30 && !b30）
                // ========================================
                if (secondsToSeal < 30 && !_reminded30Seconds)
                {
                    _reminded30Seconds = true;
                    _logService.Info("BinggoLotteryService", $"⏰ 30秒提醒: 期号 {_currentIssueId}");
                    
                    // 🔥 直接发送提醒消息到群（参考 F5BotV2 第1008行）- 异步执行
                    _ = Task.Run(async () => await SendSealingReminderAsync(_currentIssueId, 30));
                }
                
                // ========================================
                // 🔥 15 秒提醒（参考 F5BotV2: sec < 15 && !b15）
                // ========================================
                if (secondsToSeal < 15 && !_reminded15Seconds)
                {
                    _reminded15Seconds = true;
                    _logService.Info("BinggoLotteryService", $"⏰ 15秒提醒: 期号 {_currentIssueId}");
                    
                    // 🔥 直接发送提醒消息到群（参考 F5BotV2 第1013行）- 异步执行
                    _ = Task.Run(async () => await SendSealingReminderAsync(_currentIssueId, 15));
                }
            }
            else if (secondsToSeal > -_settings.SealSecondsAhead)
            {
                // 封盘中（0 到 -配置的封盘秒数，等待开奖）
                newStatus = BinggoLotteryStatus.封盘中;
                
                // 🔥 只在第一次进入封盘状态时发送封盘消息（参考 F5BotV2 第1205行 On封盘中）
                if (oldStatus != BinggoLotteryStatus.封盘中)
                {
                    _ = Task.Run(async () => await SendSealingMessageAsync(_currentIssueId));
                }
            }
            else
            {
                // 等待中（开奖后，等待下一期）
                newStatus = BinggoLotteryStatus.等待中;
            }
            
            // ========================================
            // 🔥 只在状态真正变更时触发事件
            // ========================================
            if (newStatus != oldStatus)
            {
                _currentStatus = newStatus;
                _logService.Info("BinggoLotteryService", $"🔔 状态变更: {oldStatus} → {newStatus}");
                
                StatusChanged?.Invoke(this, new BinggoStatusChangedEventArgs
                {
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    IssueId = _currentIssueId,
                    Message = GetStatusMessage(newStatus)
                });
            }
        }
        
        private string GetStatusMessage(BinggoLotteryStatus status)
        {
            return status switch
            {
                BinggoLotteryStatus.开盘中 => "开盘中",
                BinggoLotteryStatus.即将封盘 => "即将封盘",
                BinggoLotteryStatus.封盘中 => "封盘中",
                BinggoLotteryStatus.等待中 => "等待中",
                _ => "未知状态"
            };
        }
        
        

        
        
        // ========================================
        // 🔥 开奖数据查询（缓存优先策略）
        // ========================================
        
        /// <summary>
        /// 获取指定期号的开奖数据
        /// 
        /// 🔥 策略：先查本地缓存，没有再请求网络
        /// </summary>
        public async Task<BinggoLotteryData?> GetLotteryDataAsync(int issueId, bool forceRefresh = false)
        {
            try
            {
                // 步骤1: 如果不强制刷新，先查本地数据库
                if (!forceRefresh && _db != null)
                {
                    try
                    {
                        // 🔥 IsOpened 是计算属性（[Ignore]），不能在 SQLite 查询中直接使用
                        // 先查询 LotteryData 不为空的记录，然后在内存中过滤 IsOpened
                        var local = _db.Table<BinggoLotteryData>()
                            .Where(d => d.IssueId == issueId && !string.IsNullOrEmpty(d.LotteryData))
                            .ToList()
                            .FirstOrDefault(d => d.IsOpened);
                        
                        if (local != null)
                        {
                            _logService.Info("BinggoLotteryService", $"✓ 从本地缓存获取期号 {issueId} 数据");
                            return local;
                        }
                    }
                    catch (SQLite.SQLiteException ex) when (ex.Message.Contains("no such table"))
                    {
                        // 🔥 表不存在（可能是数据库刚初始化），忽略错误，直接从网络获取
                        _logService.Warning("BinggoLotteryService", $"本地数据库表不存在，跳过本地查询，从网络获取: {ex.Message}");
                    }
                }
                
                // 步骤2: 本地没有，从网络获取
                _logService.Info("BinggoLotteryService", $"🌐 从网络获取期号 {issueId} 数据");
                
                // 🔥 使用 BoterApi 单例
                var api = Services.Api.BoterApi.GetInstance();
                var response = await api.GetBgDataAsync(issueId);
                
                if (response.Code == 0 && response.Data != null && response.Data.IsOpened)
                {
                    // 步骤3: 保存到本地缓存
                    await SaveLotteryDataAsync(response.Data);
                    return response.Data;
                }
                
                _logService.Warning("BinggoLotteryService", $"期号 {issueId} 数据不存在或未开奖");
                return null;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"获取期号 {issueId} 数据失败: {ex.Message}", ex);
                return null;
            }
        }
        
        /// <summary>
        /// 获取最近 N 期的开奖数据
        /// </summary>
        public async Task<List<BinggoLotteryData>> GetRecentLotteryDataAsync(int count = 10)
        {
            try
            {
                _logService.Info("BinggoLotteryService", $"开始从 API 获取最近 {count} 期数据...");
                
                // 🔥 直接使用 BoterApi 单例（完全参考 F5BotV2）
                var api = Services.Api.BoterApi.GetInstance();
                var response = await api.GetBgDayAsync("", count, true);
                
                // 🔥 BoterApi 已经返回解析好的 List<BinggoLotteryData>，无需再转换
                if (response.Code == 0 && response.Data != null && response.Data.Count > 0)
                {
                    _logService.Info("BinggoLotteryService", $"✅ API 返回 {response.Data.Count} 期数据");
                    
                    // 保存到本地缓存
                    await SaveLotteryDataListAsync(response.Data);
                    
                    // 🔥 检查上期数据是否已开奖，如果是，触发 LotteryOpened 事件（参考 F5BotV2）
                    CheckAndNotifyLastIssue(response.Data);
                    
                    return response.Data;
                }
                else
                {
                    _logService.Warning("BinggoLotteryService", 
                        $"❌ API 返回失败: Code={response.Code}, Msg={response.Msg}");
                }
                
                // 如果网络失败，从本地读取
                if (_db != null)
                {
                    // 🔥 修复：IsOpened 是计算属性，SQLite-net 无法转换为 SQL
                    var local = _db.Table<BinggoLotteryData>()
                        .Where(d => !string.IsNullOrEmpty(d.LotteryData))
                        .OrderByDescending(d => d.IssueId)
                        .Take(count * 2) // 多取一些，因为可能有些记录 LotteryData 不完整
                        .ToList()
                        .Where(d => d.IsOpened) // 在内存中过滤，确保已开奖
                        .Take(count)
                        .ToList();
                    
                    _logService.Info("BinggoLotteryService", $"📂 从本地缓存获取 {local.Count} 期数据");
                    
                    // 🔥 同样检查上期数据
                    CheckAndNotifyLastIssue(local);
                    
                    return local;
                }
                
                return new List<BinggoLotteryData>();
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"获取最近 {count} 期数据失败: {ex.Message}", ex);
                
                // 异常时尝试从本地读取
                if (_db != null)
                {
                    try
                    {
                        // 🔥 修复：IsOpened 是计算属性，SQLite-net 无法转换为 SQL
                        var local = _db.Table<BinggoLotteryData>()
                            .Where(d => !string.IsNullOrEmpty(d.LotteryData))
                            .OrderByDescending(d => d.IssueId)
                            .Take(count * 2) // 多取一些，因为可能有些记录 LotteryData 不完整
                            .ToList()
                            .Where(d => d.IsOpened) // 在内存中过滤，确保已开奖
                            .Take(count)
                            .ToList();
                        
                        _logService.Info("BinggoLotteryService", $"📂 异常恢复：从本地缓存获取 {local.Count} 期数据");
                        
                        // 🔥 同样检查上期数据
                        CheckAndNotifyLastIssue(local);
                        
                        return local;
                    }
                    catch (Exception dbEx)
                    {
                        _logService.Error("BinggoLotteryService", $"从本地读取也失败: {dbEx.Message}", dbEx);
                    }
                }
                
                return new List<BinggoLotteryData>();
            }
        }
        
        /// <summary>
        /// 🔥 检查并通知上期开奖数据（参考 F5BotV2）
        /// 注意：不在这里触发开奖事件，开奖事件只由队列检查线程触发
        /// </summary>
        private void CheckAndNotifyLastIssue(List<BinggoLotteryData> dataList)
        {
            if (dataList == null || dataList.Count == 0)
                return;
            
            try
            {
                // 计算上期期号
                int currentIssueId = BinggoTimeHelper.GetCurrentIssueId();
                int lastIssueId = BinggoTimeHelper.GetPreviousIssueId(currentIssueId);
                
                // 🔥 在返回的数据中查找上期数据
                var lastData = dataList.FirstOrDefault(d => d.IssueId == lastIssueId);
                
                if (lastData != null && lastData.IsOpened)
                {
                    _logService.Info("BinggoLotteryService", 
                        $"🎲 发现上期已开奖数据: {lastIssueId} - {lastData.ToLotteryString()}");
                    
                    // 🔥 注意：不在这里触发开奖事件，开奖事件只由队列检查线程触发（参考 F5BotV2）
                    // 如果该期号在队列中，队列检查线程会自动处理并触发开奖事件
                }
                else
                {
                    _logService.Info("BinggoLotteryService", 
                        $"⏳ 上期数据未开奖或未找到: {lastIssueId}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"检查上期数据异常: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 处理开奖（参考 F5BotV2: On已开奖）
        /// 统一处理：结算、发送微信消息、清空投注金额等
        /// </summary>
        private async Task OnLotteryOpenedAsync(BinggoLotteryData data)
        {
            try
            {
                int issueId = data.IssueId;
                int issueidLite = issueId % 1000;  // 期号后3位（参考 F5BotV2: issueid_lite = data.IssueId % 1000）
                
                _logService.Info("BinggoLotteryService", $"🎲 开奖处理: {issueId} - {data.ToLotteryString()}");
                
                // 🔥 1. 获取当期所有订单（参考 F5BotV2 第 1420 行）
                // 🔥 查询条件：期号匹配，且不是已取消/未知状态，且不是托单
                var allOrders = _ordersBindingList?.ToList() ?? new List<V2MemberOrder>();
                _logService.Info("BinggoLotteryService", $"📋 订单列表总数: {allOrders.Count}");
                
                var orders = allOrders
                    .Where(o => o.IssueId == issueId 
                        && o.OrderStatus != OrderStatus.已取消 
                        && o.OrderStatus != OrderStatus.未知
                        && o.OrderType != OrderType.托)  // 托单不显示
                    .ToList();
                
                _logService.Info("BinggoLotteryService", $"📋 期号 {issueId} 的待结算订单数: {orders.Count}");
                if (orders.Count > 0)
                {
                    foreach (var o in orders)
                    {
                        _logService.Info("BinggoLotteryService", 
                            $"  订单ID={o.Id}, 状态={o.OrderStatus}, 类型={o.OrderType}, 期号={o.IssueId}, 金额={o.AmountTotal}");
                    }
                }
                
                // 🔥 2. 结算订单并统计（参考 F5BotV2 第 1429-1450 行）
                var ordersReports = new Dictionary<string, (string nickname, float balance, float totalAmount, float profit)>();

                if (orders != null && _orderService != null)
                {
                    foreach (var order in orders)
                    {
                        // 结算单个订单
                        await _orderService.SettleSingleOrderAsync(order, data);

                        // 统计输赢数据，整合显示给会员看的（参考 F5BotV2 第 1436-1449 行）
                        var member = _membersBindingList?.FirstOrDefault(m => m.Wxid == order.Wxid);
                        if (member == null || string.IsNullOrEmpty(order.Wxid)) continue;

                        // 🔥 使用订单中的昵称（参考 F5BotV2: order.nickname）
                        string nickname = order.Nickname ?? member.Nickname ?? member.DisplayName ?? "未知";

                        // 🔥 注意：这里不缓存余额，因为余额在结算过程中会被更新
                        // 余额将在发送消息时重新获取最新值（参考 F5BotV2 第 1454 行）
                        if (!ordersReports.ContainsKey(order.Wxid))
                        {
                            ordersReports[order.Wxid] = (
                                nickname,
                                0f,  // 🔥 不缓存余额，将在发送时重新获取
                                order.AmountTotal,
                                order.Profit
                            );
                        }
                        else
                        {
                            var existing = ordersReports[order.Wxid];
                            ordersReports[order.Wxid] = (
                                existing.nickname,
                                0f,  // 🔥 不缓存余额，将在发送时重新获取
                                existing.totalAmount + order.AmountTotal,
                                existing.profit + order.Profit
                            );
                        }
                    }

                    if (orders.Count > 0)
                    {
                        _logService.Info("BinggoLotteryService", $"✅ 结算完成: {orders.Count} 单");
                    }
                }

                // 🔥 转换为列表格式（用于发送消息）
                var ordersReportsList = ordersReports.Select(kvp => (
                    wxid: kvp.Key,
                    nickname: kvp.Value.nickname,
                    balance: kvp.Value.balance,
                    totalAmount: kvp.Value.totalAmount,
                    profit: kvp.Value.profit
                )).ToList();

                // 🔥 3. 发送中奖名单和留分名单到微信群（参考 F5BotV2 第 1415-1474 行）
                // 🔥 重要：无论是否有订单，都要发送这两个名单（参考 F5BotV2 第 1462、1474 行）
                string? groupWxId = _groupBindingService?.CurrentBoundGroup?.Wxid;
                if (!string.IsNullOrEmpty(groupWxId) && _socketClient != null && _socketClient.IsConnected)
                {
                    await SendSettlementMessagesAsync(data, groupWxId, issueidLite, ordersReportsList);
                    
                    // 🔥 标记该期号已结算完成（发送了中~名单和留~名单）
                    _lastSettledIssueId = issueId;
                    _logService.Info("BinggoLotteryService", $"✅ 期号 {issueId} 结算完成，已发送中~名单和留~名单");
                }
                else
                {
                    _logService.Info("BinggoLotteryService", "未绑定群或微信未登录，跳过发送结算消息");
                }

                
                // 🔥 4. 清空会员表当期投注金额（参考 F5BotV2 第 1477-1480 行）
                ClearMembersBetCur();
                
                // 🔥 5. 触发开奖事件（通知 UI 更新）
                LotteryOpened?.Invoke(this, new BinggoLotteryOpenedEventArgs
                {
                    LotteryData = data
                });
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"开奖处理失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 发送结算消息到微信群（参考 F5BotV2: On已开奖）
        /// 格式：第{issueid_lite}队\r{开奖号码}\r----中~名单----\r{会员名}[余额] 纯利\r
        /// 🔥 重要：无论是否有订单，都要发送这两个名单（参考 F5BotV2 第 1462、1474 行）
        /// </summary>
        private async Task SendSettlementMessagesAsync(
            BinggoLotteryData lotteryData, 
            string groupWxId, 
            int issueidLite,
            List<(string wxid, string nickname, float balance, float totalAmount, float profit)> ordersReports)
        {
            try
            {
                // 🔥 检查是否应该发送系统消息
                if (!ShouldSendSystemMessage())
                {
                    return;
                }
                
                int issueId = lotteryData.IssueId;
                
                // 🔥 发送中奖名单（参考 F5BotV2 第 1415-1462 行）
                // 格式：第{issueid_lite}队\r{开奖号码}\r----中~名单----\r{会员名}[余额] 纯利\r
                // 🔥 重要：即使没有订单，也要发送中奖名单（只是没有会员数据）
                var winningMessage = new System.Text.StringBuilder();
                winningMessage.Append($"第{issueidLite}队\r");
                winningMessage.Append($"{lotteryData.ToLotteryString()}\r");
                winningMessage.Append($"----中~名单----\r");
                
                if (ordersReports != null && ordersReports.Count > 0)
                {
                    foreach (var report in ordersReports)
                    {
                        // 🔥 重新获取最新余额（参考 F5BotV2 第 1454 行：var m = v2Memberbindlite.FirstOrDefault(...)）
                        // 因为同一个会员可能有多个订单，每次结算都会更新余额
                        // 所以必须在发送消息时重新获取最新余额，不能使用ordersReports中缓存的余额
                        var member = _membersBindingList?.FirstOrDefault(m => m.Wxid == report.wxid);
                        if (member == null) continue;  // 会员不存在，跳过
                        
                        // 🔥 使用结算后的最新余额（参考 F5BotV2 第 1458 行：[{(int)m.Balance}]）
                        float currentBalance = member.Balance;
                        
                        // 🔥 格式完全一致：{nickname}[{(int)balance}] {(int)profit - totalAmount}\r
                        // 盈利 = 总赢金额 - 投注总额 = 纯利（参考 F5BotV2 第 1458 行：{(int)order.Profit- order.AmountTotal}）
                        float netProfit = report.profit - report.totalAmount;  // 纯利 = 总赢 - 投注额
                        winningMessage.Append($"{report.nickname}[{(int)currentBalance}] {(int)netProfit}\r");
                    }
                }
                
                _logService.Info("BinggoLotteryService", $"📤 发送中奖名单到群: {groupWxId}");
                var response1 = await _socketClient!.SendAsync<object>("SendMessage", groupWxId, winningMessage.ToString());
                if (response1 != null)
                {
                    _logService.Info("BinggoLotteryService", "✅ 中奖名单已发送");
                }
                
                // 🔥 发送留分名单（参考 F5BotV2 第 1464-1474 行）
                // 格式：第{issueid_lite}队\r{开奖号码}\r----留~名单----\r{会员名} 余额\r
                // 🔥 重要：无论是否有订单，都要发送留分名单
                var balanceMessage = new System.Text.StringBuilder();
                balanceMessage.Append($"第{issueidLite}队\r");
                balanceMessage.Append($"{lotteryData.ToLotteryString()}\r");
                balanceMessage.Append($"----留~名单----\r");
                
                if (_membersBindingList != null)
                {
                    foreach (var member in _membersBindingList)
                    {
                        // 🔥 格式完全一致：{nickname} {(int)Balance}\r
                        if ((int)member.Balance >= 1)  // 余额 >= 1 才显示
                        {
                            balanceMessage.Append($"{member.Nickname ?? member.DisplayName ?? "未知"} {(int)member.Balance}\r");
                        }
                    }
                }
                
                _logService.Info("BinggoLotteryService", $"📤 发送留分名单到群: {groupWxId}");
                var response2 = await _socketClient!.SendAsync<object>("SendMessage", groupWxId, balanceMessage.ToString());
                if (response2 != null)
                {
                    _logService.Info("BinggoLotteryService", "✅ 留分名单已发送");
                }
                
                // 🔥 重要：增加延迟，确保消息真正发送到微信群（参考 F5BotV2 的消息发送机制）
                // 这样可以确保下一期的"线下开始"消息不会在"留~名单"之前发送
                await Task.Delay(1000);  // 延迟1秒，确保消息顺序正确
                _logService.Info("BinggoLotteryService", "✅ 结算消息发送完成，已等待1秒确保消息顺序");
                
                // 🔥 检查是否是今日最后一期（参考 F5BotV2 第 1482-1488 行）
                int dayIndex = Helpers.BinggoHelper.GetDayIndex(issueId);
                if (dayIndex == 203)
                {
                    _logService.Info("BinggoLotteryService", "今日最后一期，发送结束消息");
                    var endMessage = "各位客官今日份结束咯。\r";
                    await _socketClient!.SendAsync<object>("SendMessage", groupWxId, endMessage);
                }
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"发送结算消息失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 处理所有微信消息（统一入口：查、上分、下分、取消、投注）
        /// 所有炳狗相关的业务逻辑都通过这个方法处理
        /// 回复消息格式完全按照 F5BotV2（字节级别一致）
        /// </summary>
        public async Task<(bool handled, string? replyMessage, V2MemberOrder? order)> ProcessMessageAsync(
            V2Member member,
            string messageContent)
        {
            try
            {
                string msg = messageContent.Trim();
                
                // 🔥 1. 处理查询命令（查、流水、货单）- 参考 F5BotV2 第2174行
                if (msg == "查" || msg == "流水" || msg == "货单")
                {
                    // 🔥 格式完全按照 F5BotV2 第2177-2180行（字节级别一致）
                    // @{member.nickname}\r流~~记录\r今日/本轮进货:{BetToday}/{BetCur}\r今日上/下:{CreditToday}/{WithdrawToday}\r今日盈亏:{IncomeToday}\r
                    string sendTxt = $"@{member.Nickname}\r流~~记录\r";
                    sendTxt += $"今日/本轮进货:{member.BetToday}/{member.BetCur}\r";
                    sendTxt += $"今日上/下:{member.CreditToday}/{member.WithdrawToday}\r";
                    // 🔥 F5BotV2 使用 Zsxs 配置决定是否显示整数，这里默认使用整数格式（与 F5BotV2 默认一致）
                    sendTxt = sendTxt + $"今日盈亏:" + ((int)member.IncomeToday).ToString() + "\r";
                    
                    _logService.Info("BinggoLotteryService", 
                        $"查询命令: {member.Nickname} - 今日下注:{member.BetToday}, 盈亏:{member.IncomeToday}");
                    
                    return (true, sendTxt, null);
                }
                
                // 🔥 2. 处理上分/下分命令 - 参考 F5BotV2 第2564行
                string regexStr = "(上|下){1}(\\d+)(分*){1}";  // 🔥 修复：使用 \\d+ 而不是 \\d*，确保至少有一个数字
                if (Regex.IsMatch(msg, regexStr, RegexOptions.IgnoreCase))
                {
                    var match = Regex.Match(msg, regexStr, RegexOptions.IgnoreCase);
                    string st1 = match.Groups[1].Value;  // 上/下
                    string st2 = match.Groups[2].Value;  // 金额
                    
                    _logService.Info("BinggoLotteryService", 
                        $"解析上下分命令: 原始消息='{msg}', 动作='{st1}', 金额字符串='{st2}'");
                    
                    int money = 0;
                    try
                    {
                        money = Convert.ToInt32(st2);
                    }
                    catch
                    {
                        _logService.Warning("BinggoLotteryService", $"金额解析失败: '{st2}'");
                        return (true, "请输入正确的金额，例如：上1000 或 下500", null);
                    }
                    
                    if (money <= 0)
                    {
                        _logService.Warning("BinggoLotteryService", $"金额必须大于0: {money}");
                        return (true, "金额必须大于0", null);
                    }
                    
                    _logService.Info("BinggoLotteryService", 
                        $"✅ 上下分命令解析成功: 动作={st1}, 金额={money}");
                    
                    // 判断是上分还是下分
                    bool isCredit = st1 == "上";
                    
                    // 🔥 下分需要检查余额 - 参考 F5BotV2 第2430行
                    if (!isCredit && member.Balance < money)
                    {
                        string balanceReply = $"@{member.Nickname} 客官你的荷包是否不足!";
                        return (true, balanceReply, null);
                    }
                    
                    // 🔥 创建上下分申请
                    if (_db == null)
                    {
                        _logService.Warning("BinggoLotteryService", "数据库未初始化，无法创建上下分申请");
                        return (true, "系统错误，请联系管理员", null);
                    }
                    
                    _db.CreateTable<Models.V2CreditWithdraw>();
                    
                    var request = new Models.V2CreditWithdraw
                    {
                        GroupWxId = member.GroupWxId,
                        Wxid = member.Wxid,
                        Nickname = member.Nickname,
                        Amount = money,  // 🔥 使用解析出的金额
                        Action = isCredit ? CreditWithdrawAction.上分 : CreditWithdrawAction.下分,
                        Status = CreditWithdrawStatus.等待处理,
                        TimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Notes = $"会员申请{(isCredit ? "上分" : "下分")}"
                    };
                    
                    _logService.Info("BinggoLotteryService", 
                        $"创建上下分申请: 会员={member.Nickname}, 动作={(isCredit ? "上分" : "下分")}, 金额={money}");
                    
                    // 🔥 添加到 BindingList（会自动保存到数据库，并触发 UI 更新）
                    if (_creditWithdrawsBindingList != null)
                    {
                        _creditWithdrawsBindingList.Add(request);
                        _logService.Info("BinggoLotteryService", 
                            $"{(isCredit ? "上分" : "下分")}申请已创建并添加到 BindingList: {member.Nickname} - {money}");
                    }
                    else
                    {
                        // 如果没有 BindingList，直接保存到数据库（兼容旧逻辑）
                        _db.Insert(request);
                        _logService.Warning("BinggoLotteryService", 
                            $"上下分 BindingList 未设置，直接保存到数据库: {member.Nickname} - {money}");
                    }
                    
                    // 🔥 回复格式 - 参考 F5BotV2 第2605行：@{m.nickname}\r[{m.id}]请等待
                    string reply = $"@{member.Nickname}\r[{member.Id}]请等待";
                    return (true, reply, null);
                }
                
                // 🔥 3. 处理取消命令 - 参考 F5BotV2 第2190行
                if (msg == "取消" || msg == "qx")
                {
                    if (_currentIssueId == 0)
                    {
                        return (true, "系统初始化中，请稍后...", null);
                    }
                    
                    // 🔥 检查是否已封盘（只能在开盘中取消）- 参考 F5BotV2 第2216行
                    if (_currentStatus != BinggoLotteryStatus.开盘中)
                    {
                        return (true, $"@{member.Nickname} 时间到!不能取消!", null);
                    }
                    
                    // 查找订单
                    if (_orderService == null)
                    {
                        return (true, "系统错误，请稍后重试", null);
                    }
                    
                    var orders = _orderService.GetPendingOrdersForMemberAndIssue(member.Wxid, _currentIssueId)
                        .Where(o => o.OrderStatus != OrderStatus.已取消 && o.OrderStatus != OrderStatus.已完成)
                        .ToList();
                    
                    if (orders == null || orders.Count == 0)
                    {
                        return (true, $"@{member.Nickname}\r当前期号无待处理订单", null);
                    }
                    
                    // 🔥 取消最后一个订单（参考 F5BotV2 第2215行）
                    var ods = orders.Last();
                    
                    // 执行取消逻辑
                    ods.OrderStatus = OrderStatus.已取消;
                    _orderService.UpdateOrder(ods);
                    
                    // 🔥 退款给会员（参考 F5BotV2 第2301行）
                    member.Balance += ods.AmountTotal;
                    
                    // 🔥 减掉会员统计（参考 F5BotV2 第2302-2304行：OrderCancel 方法）
                    // 注意：托单不计算在内，但这里已经通过订单类型判断了
                    if (ods.OrderType != OrderType.托)
                    {
                        member.BetCur -= ods.AmountTotal;
                        member.BetToday -= ods.AmountTotal;
                        member.BetTotal -= ods.AmountTotal;
                        member.BetWait -= ods.AmountTotal;  // 减掉待结算金额
                        
                        _logService.Info("BinggoLotteryService", 
                            $"📊 统计更新: {member.Nickname} - 减掉投注 {ods.AmountTotal:F2} - 今日下注 {member.BetToday:F2}");
                    }
                    
                    _logService.Info("BinggoLotteryService", 
                        $"✅ 取消订单: {member.Nickname} - 期号:{_currentIssueId} - 订单ID:{ods.Id}");
                    
                    // 🔥 更新全局统计（参考 F5BotV2 第680-709行：OnMemberOrderCancel）
                    if (_statisticsService != null && ods.OrderType != OrderType.托)
                    {
                        _logService.Info("BinggoLotteryService", 
                            $"📊 调用统计服务减掉订单: 订单ID={ods.Id} 金额={ods.AmountTotal} 期号={ods.IssueId}");
                        _statisticsService.OnOrderCanceled(ods);
                        _logService.Info("BinggoLotteryService", 
                            $"✅ 统计服务已调用: 总注={_statisticsService.BetMoneyTotal} 今投={_statisticsService.BetMoneyToday} 当前={_statisticsService.BetMoneyCur}");
                    }
                    else
                    {
                        _logService.Warning("BinggoLotteryService", 
                            $"⚠️ 未调用统计服务: _statisticsService={(_statisticsService != null ? "已设置" : "null")} 订单类型={ods.OrderType}");
                    }
                    
                    // 🔥 回复格式 - 参考 F5BotV2 第2221行：@{m.nickname} {BetContentOriginal}\r已取消!\r+{AmountTotal}|留:{(int)Balance}
                    string cancelReply = $"@{member.Nickname} {ods.BetContentOriginal}\r已取消!\r+{ods.AmountTotal}|留:{(int)member.Balance}";
                    return (true, cancelReply, ods);
                }
                
                // 🔥 4. 处理投注消息
                // 简单判断是否可能是下注消息
                if (!messageContent.Any(char.IsDigit))
                {
                    return (false, null, null);  // 不是下注消息，不处理
                }
                
                string[] keywords = { "大", "小", "单", "双", "龙", "虎", 
                                     "尾大", "尾小", "合单", "合双",
                                     "一", "二", "三", "四", "五", "六", "总" };
                
                bool looksLikeBet = keywords.Any(k => messageContent.Contains(k));
                if (!looksLikeBet)
                {
                    return (false, null, null);  // 不是下注消息，不处理
                }
                
                // 🔥 检查期号是否初始化
                if (_currentIssueId == 0)
                {
                    _logService.Warning("BinggoLotteryService", "当前期号未初始化");
                    return (true, "系统初始化中，请稍后...", null);
                }
                
                // 🔥 检查状态（只有"开盘中"和"即将封盘"可以下注）
                if (_currentStatus == BinggoLotteryStatus.封盘中 || 
                    _currentStatus == BinggoLotteryStatus.开奖中)
                {
                    _logService.Info("BinggoLotteryService", 
                        $"❌ 封盘状态拒绝下注: {member.Nickname} - 期号: {_currentIssueId} - 状态: {_currentStatus}");
                    // 🔥 格式完全按照 F5BotV2 第2425行：{m.nickname}\r时间未到!不收货!
                    return (true, $"{member.Nickname}\r时间未到!不收货!", null);
                }
                
                // 🔥 调用订单服务创建订单
                if (_orderService == null)
                {
                    _logService.Error("BinggoLotteryService", "订单服务未初始化");
                    return (true, "系统错误，请稍后重试", null);
                }
                
                _logService.Info("BinggoLotteryService", 
                    $"📝 处理下注请求: {member.Nickname} ({member.Wxid}) - 期号: {_currentIssueId} - 状态: {_currentStatus}");
                
                var (success, message, order) = await _orderService.CreateOrderAsync(
                    member,
                    messageContent,
                    _currentIssueId,
                    _currentStatus);
                
                if (success)
                {
                    _logService.Info("BinggoLotteryService", 
                        $"✅ 下注成功: {member.Nickname} - 期号: {_currentIssueId} - 订单ID: {order?.Id}");
                }
                else
                {
                    _logService.Warning("BinggoLotteryService", 
                        $"❌ 下注失败: {member.Nickname} - {message}");
                }
                
                return (true, message, order);
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", 
                    $"处理消息失败: {ex.Message}", ex);
                return (true, "系统错误，请稍后重试", null);
            }
        }
        
        /// <summary>
        /// 🔥 开盘处理（参考 F5BotV2 第1139-1178行 On开盘中）
        /// 只在状态变为"开盘中"时执行一次
        /// 🔥 重要：必须确保上一期的中~名单和留~名单已发送，才能发送本期的"线下开始"
        /// </summary>
        private async Task OnOpeningAsync(int issueId)
        {
            try
            {
                // 🔥 防止同一期号重复发送"线下开始"消息
                if (_lastOpeningIssueId == issueId)
                {
                    _logService.Warning("BinggoLotteryService", $"⚠️ 期号 {issueId} 的'线下开始'消息已发送过，跳过重复发送");
                    return;
                }
                
                _logService.Info("BinggoLotteryService", $"📢 开盘处理: 期号 {issueId}");
                
                // 🔥 检查上一期是否已结算完成（发送了中~名单和留~名单）
                // 参考 F5BotV2：开奖后状态变为"等待中"，然后在状态循环中变为"开盘中"时发送
                // 🔥 关键：只有在上一期真正结算完成后，才发送本期的"线下开始"消息
                // 🔥 如果上一期还没结算完成，直接 return，不发送"线下开始"消息
                int previousIssueId = Helpers.BinggoTimeHelper.GetPreviousIssueId(issueId);
                if (_lastSettledIssueId < previousIssueId)
                {
                    _logService.Warning("BinggoLotteryService", 
                        $"⚠️ 上一期 {previousIssueId} 尚未结算完成（已结算期号：{_lastSettledIssueId}），跳过发送本期 {issueId} 的'线下开始'消息");
                    _logService.Warning("BinggoLotteryService", 
                        $"⚠️ 等待上一期开奖并结算完成后，下次 tick 时再发送'线下开始'消息");
                    
                    // 🔥 直接返回，不发送"线下开始"消息
                    // 下次 tick 时会再次检查，如果上一期已结算完成，就会发送
                    return;
                }
                
                // 🔥 重置提醒标志（参考 F5BotV2 第1157-1158行）
                _reminded30Seconds = false;
                _reminded15Seconds = false;
                
                // 🔥 发送开盘提示消息（参考 F5BotV2 第1159行）
                // 格式：第{issueid % 1000}队\r{Reply_开盘提示}
                string? groupWxId = _groupBindingService?.CurrentBoundGroup?.Wxid;
                if (!string.IsNullOrEmpty(groupWxId) && _socketClient != null && _socketClient.IsConnected)
                {
                    // 🔥 检查是否应该发送系统消息
                    if (ShouldSendSystemMessage())
                    {
                        int issueShort = issueId % 1000;
                        string message = $"第{issueShort}队\r---------线下开始---------";
                        
                        _logService.Info("BinggoLotteryService", $"📢 发送开盘提示: {groupWxId} - {message}");
                        
                        var response = await _socketClient.SendAsync<object>("SendMessage", groupWxId, message);
                        if (response != null)
                        {
                            // 🔥 标记该期号已发送过"线下开始"消息
                            _lastOpeningIssueId = issueId;
                            _logService.Info("BinggoLotteryService", $"✅ 开盘提示已发送: {message}");
                        }
                        
                        // 🔥 重要：增加延迟，确保"线下开始"消息先到达微信群，然后再发送图片
                        await Task.Delay(500);  // 延迟500ms
                        
                        // 🔥 发送历史记录图片（参考 F5BotV2 第1162行 On开盘发送历史记录图片）
                        await SendHistoryLotteryImageAsync(issueId, groupWxId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"开盘处理失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 发送历史记录图片（参考 F5BotV2 第1180-1202行）
        /// 生成最近32期开奖走势图并发送到微信群
        /// </summary>
        private async Task SendHistoryLotteryImageAsync(int issueId, string? groupWxId)
        {
            try
            {
                if (string.IsNullOrEmpty(groupWxId) || _socketClient == null || !_socketClient.IsConnected)
                {
                    _logService.Debug("BinggoLotteryService", "未绑定群或微信未登录，跳过发送历史记录图片");
                    return;
                }
                
                // 🔥 检查是否应该发送系统消息
                if (!ShouldSendSystemMessage())
                {
                    return;
                }
                
                _logService.Info("BinggoLotteryService", $"📊 开始生成历史记录图片: 期号 {issueId}");
                
                // 🔥 最多重试5次生成图片（参考 F5BotV2 第1185行）
                for (int retry = 0; retry < 5; retry++)
                {
                    try
                    {
                        // 🔥 1. 获取最近32期开奖数据（参考 F5BotV2 第1623行）
                        var api = Services.Api.BoterApi.GetInstance();
                        var response = await api.GetBgDayAsync(DateTime.Now.ToString("yyyy-MM-dd"), 32, true);
                        
                        if (response.Code != 0 || response.Data == null || response.Data.Count == 0)
                        {
                            _logService.Warning("BinggoLotteryService", $"获取历史数据失败，重试 {retry + 1}/5");
                            await Task.Delay(500);  // 等待500ms后重试
                            continue;
                        }
                        
                        // 🔥 2. 生成图片到 C:\images\ 目录（避免中文路径问题）
                        // 不使用用户文件夹，避免中文用户名导致的路径问题
                        // 需要管理员权限才能在C盘根目录创建文件夹
                        var dataDir = @"C:\images";
                        
                        // 🔥 防御性编程：确保目录存在
                        if (!Directory.Exists(dataDir))
                        {
                            try
                            {
                                Directory.CreateDirectory(dataDir);
                                _logService.Info("BinggoLotteryService", $"创建图片目录: {dataDir}");
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                _logService.Error("BinggoLotteryService", $"创建目录失败，需要管理员权限: {ex.Message}");
                                throw new Exception("需要管理员权限才能在C盘根目录创建文件夹，请以管理员身份运行程序");
                            }
                        }
                        
                        string imagePath = Path.Combine(dataDir, $"img_{issueId}.jpg");
                        _logService.Info("BinggoLotteryService", $"图片保存路径: {imagePath}");
                        
                        bool imageCreated = await CreateLotteryImageAsync(response.Data, imagePath);
                        
                        // 🔥 防御性编程：严格检查图片是否生成成功
                        if (!imageCreated)
                        {
                            _logService.Warning("BinggoLotteryService", $"图片生成失败（返回false），重试 {retry + 1}/5");
                            await Task.Delay(500);
                            continue;
                        }
                        
                        if (!File.Exists(imagePath))
                        {
                            _logService.Warning("BinggoLotteryService", $"图片文件不存在: {imagePath}，重试 {retry + 1}/5");
                            await Task.Delay(500);
                            continue;
                        }
                        
                        // 🔥 防御性编程：检查文件大小
                        var fileInfo = new FileInfo(imagePath);
                        if (fileInfo.Length == 0)
                        {
                            _logService.Warning("BinggoLotteryService", $"图片文件为空: {imagePath}，重试 {retry + 1}/5");
                            await Task.Delay(500);
                            continue;
                        }
                        
                        _logService.Info("BinggoLotteryService", $"✅ 图片生成成功: {imagePath}，大小: {fileInfo.Length} 字节");
                        
                        // 🔥 防御性编程：等待文件完全写入磁盘（避免"文件找不到"错误）
                        await Task.Delay(300);  // 等待300ms确保文件系统完全刷新
                        
                        // 🔥 再次验证文件是否可以访问
                        if (!File.Exists(imagePath))
                        {
                            _logService.Warning("BinggoLotteryService", $"等待后文件仍不存在: {imagePath}，重试 {retry + 1}/5");
                            await Task.Delay(500);
                            continue;
                        }
                        
                        // 🔥 3. 发送图片到微信群（直接使用纯英文路径 C:\images\）
                        _logService.Info("BinggoLotteryService", $"📤 发送历史记录图片到群: {groupWxId}，文件路径: {imagePath}");
                        var sendResponse = await _socketClient.SendAsync<object>("SendImage", groupWxId, imagePath);
                        
                        if (sendResponse != null)
                        {
                            _logService.Info("BinggoLotteryService", $"✅ 历史记录图片已发送: {imagePath}");
                            return;  // 成功，退出重试循环
                        }
                        else
                        {
                            _logService.Warning("BinggoLotteryService", $"图片发送失败（返回null），重试 {retry + 1}/5");
                            await Task.Delay(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Warning("BinggoLotteryService", $"生成/发送图片异常，重试 {retry + 1}/5: {ex.Message}");
                        await Task.Delay(500);
                    }
                }
                
                _logService.Error("BinggoLotteryService", "历史记录图片发送失败：已达最大重试次数");
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"发送历史记录图片失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 生成开奖走势图（参考 F5BotV2 第1616-1693行）
        /// </summary>
        private async Task<bool> CreateLotteryImageAsync(List<BinggoLotteryData> historyData, string outputPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (historyData == null || historyData.Count == 0)
                    {
                        _logService.Warning("BinggoLotteryService", "历史数据为空，无法生成图片");
                        return false;
                    }
                    
                    // 🔥 模板图片路径（在 libs 目录下）
                    string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "bgzst.png");
                    if (!File.Exists(templatePath))
                    {
                        _logService.Error("BinggoLotteryService", $"模板文件不存在: {templatePath}");
                        _logService.Error("BinggoLotteryService", $"请确保 libs/bgzst.png 文件存在");
                        return false;
                    }
                    
                    // 🔥 加载模板图片（参考 F5BotV2 第1635-1636行）
                    using (System.Drawing.Image templateImage = System.Drawing.Image.FromFile(templatePath))
                    using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(templateImage, templateImage.Width, templateImage.Height))
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        // 🔥 字体设置（参考 F5BotV2 第1643-1644行）
                        using (System.Drawing.Font font = new System.Drawing.Font("微软雅黑", 16.0f))
                        using (System.Drawing.Font fontBold = new System.Drawing.Font("微软雅黑", 16.0f, System.Drawing.FontStyle.Bold))
                        {
                            float rectY = 71;  // 🔥 起始Y坐标（参考 F5BotV2 第1641行）
                            
                            // 🔥 排序（参考 F5BotV2 第1647-1650行）
                            var sortedData = historyData.OrderBy(d => d.IssueId).ToList();
                            
                            // 🔥 绘制每一期数据（参考 F5BotV2 第1652-1676行）
                            for (int i = 0; i < sortedData.Count && i < 32; i++)  // 最多32期
                            {
                                var item = sortedData[i];
                                float currentY = rectY + (i * 28);  // 每行高度28像素
                                
                                // 🔥 绘制期号（参考 F5BotV2 第1655、1659行）
                                int issueShort = item.IssueId % 1000;
                                DrawText(g, issueShort.ToString(), 2, currentY, font, System.Drawing.Color.Black);
                                
                                // 🔥 绘制时间（参考 F5BotV2 第1656、1660行）
                                string time = DateTime.Parse(item.OpenTime).ToString("HH:mm");
                                DrawText(g, time, 60, currentY, font, System.Drawing.Color.Black);
                                
                                // 🔥 绘制5个开奖号码（参考 F5BotV2 第1664-1668行）
                                DrawLotteryNumber(g, 126, (int)currentY, fontBold, item.P1.Number);
                                DrawLotteryNumber(g, 226 + 3, (int)currentY, fontBold, item.P2.Number);
                                DrawLotteryNumber(g, 326 + 3, (int)currentY, fontBold, item.P3.Number);
                                DrawLotteryNumber(g, 428 + 3, (int)currentY, fontBold, item.P4.Number);
                                DrawLotteryNumber(g, 530 + 3, (int)currentY, fontBold, item.P5.Number);
                                
                                // 🔥 绘制和值（参考 F5BotV2 第1669-1670行）
                                int sum = item.P1.Number + item.P2.Number + item.P3.Number + item.P4.Number + item.P5.Number;
                                DrawLotterySum(g, 635, (int)currentY, fontBold, sum);
                                
                                // 🔥 绘制龙虎（参考 F5BotV2 第1671-1674行）
                                string dragonTiger = item.DragonTiger == Models.Games.Binggo.DragonTigerType.Dragon ? "龙" : "虎";
                                DrawLotteryDragonTiger(g, 750, (int)currentY, fontBold, dragonTiger);
                            }
                        }
                        
                        // 🔥 防御性编程：确保输出目录存在
                        string? outputDir = Path.GetDirectoryName(outputPath);
                        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                        {
                            Directory.CreateDirectory(outputDir);
                            _logService.Info("BinggoLotteryService", $"创建输出目录: {outputDir}");
                        }
                        
                        // 🔥 保存图片（参考 F5BotV2 第1680行）
                        bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        
                        // 🔥 防御性编程：验证文件是否真的生成了
                        if (!File.Exists(outputPath))
                        {
                            _logService.Error("BinggoLotteryService", $"图片保存失败，文件不存在: {outputPath}");
                            return false;
                        }
                        
                        var fileInfo = new FileInfo(outputPath);
                        _logService.Info("BinggoLotteryService", $"✅ 图片生成成功: {outputPath}，大小: {fileInfo.Length} 字节");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error("BinggoLotteryService", $"生成图片失败: {ex.Message}", ex);
                    return false;
                }
            });
        }
        
        /// <summary>
        /// 🔥 绘制文本（参考 F5BotV2 第1695-1715行）
        /// 完全复制 F5BotV2 的实现：使用 RectangleF 绘制区域
        /// </summary>
        private void DrawText(System.Drawing.Graphics g, string text, float x, float y, System.Drawing.Font font, System.Drawing.Color color, float fontSize = 10.0f)
        {
            try
            {
                // 🔥 定义矩形区域（参考 F5BotV2 第1701-1704行）
                float rectWidth = text.Length * (fontSize + 40);
                float rectHeight = fontSize + 40;
                System.Drawing.RectangleF textArea = new System.Drawing.RectangleF(x, y, rectWidth, rectHeight);
                
                // 🔥 使用画笔绘制文字（参考 F5BotV2 第1706-1707行）
                using (System.Drawing.Brush brush = new System.Drawing.SolidBrush(color))
                {
                    g.DrawString(text, font, brush, textArea);
                }
            }
            catch (Exception ex)
            {
                _logService.Warning("BinggoLotteryService", $"绘制文本失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔥 绘制开奖号码（参考 F5BotV2 SetLotteryData 第1722-1747行）
        /// 完全复制 F5BotV2 的逻辑：
        /// - 1-40: 黑色数字 + "小"(黑) + "单/双"
        /// - 41-80: 红色数字 + "大"(红) + "单/双"
        /// - 单数: "单"(黑)
        /// - 双数: "双"(红)
        /// </summary>
        private void DrawLotteryNumber(System.Drawing.Graphics g, int x, int y, System.Drawing.Font font, int number)
        {
            float tmpX = x;
            
            // 🔥 如果是个位数(1-9)，数字位置要右移8像素（参考 F5BotV2 第1727-1730行）
            if (number >= 1 && number <= 40)
            {
                if (number >= 1 && number <= 9)
                {
                    tmpX += 8;
                }
                DrawText(g, number.ToString(), tmpX, y, font, System.Drawing.Color.Black);    // 数字（黑色）
                DrawText(g, "小", x + 38, y, font, System.Drawing.Color.Black);               // 小（黑色）
            }
            if (number > 40 && number <= 80)
            {
                DrawText(g, number.ToString(), x, y, font, System.Drawing.Color.Red);         // 数字（红色）
                DrawText(g, "大", x + 38, y, font, System.Drawing.Color.Red);                 // 大（红色）
            }
            
            // 🔥 单双（参考 F5BotV2 第1739-1746行）
            if (number % 2 == 1)
            {
                DrawText(g, "单", x + 68 + 3, y, font, System.Drawing.Color.Black);           // 单（黑色）
            }
            if (number % 2 == 0)
            {
                DrawText(g, "双", x + 68 + 3, y, font, System.Drawing.Color.Red);             // 双（红色）
            }
        }
        
        /// <summary>
        /// 🔥 绘制和值（参考 F5BotV2 SetLotterySum 第1750-1775行）
        /// 完全复制 F5BotV2 的逻辑：
        /// - 15-202: 黑色数字 + "小"(黑) + "单/双"
        /// - 203-390: 红色数字 + "大"(红) + "单/双"
        /// </summary>
        private void DrawLotterySum(System.Drawing.Graphics g, int x, int y, System.Drawing.Font font, int sum)
        {
            float tmpX = x;
            
            // 🔥 如果是个位数(1-9)，数字位置要右移8像素（参考 F5BotV2 第1755-1758行）
            if (sum >= 15 && sum <= 202)
            {
                if (sum >= 1 && sum <= 9)
                {
                    tmpX += 8;
                }
                DrawText(g, sum.ToString(), tmpX, y, font, System.Drawing.Color.Black);       // 数字（黑色）
                DrawText(g, "小", x + 50, y, font, System.Drawing.Color.Black);               // 小（黑色）
            }
            if (sum >= 203 && sum <= 390)
            {
                DrawText(g, sum.ToString(), x, y, font, System.Drawing.Color.Red);            // 数字（红色）
                DrawText(g, "大", x + 50, y, font, System.Drawing.Color.Red);                 // 大（红色）
            }
            
            // 🔥 单双（参考 F5BotV2 第1767-1774行）
            if (sum % 2 == 1)
            {
                DrawText(g, "单", x + 80, y, font, System.Drawing.Color.Black);               // 单（黑色）
            }
            if (sum % 2 == 0)
            {
                DrawText(g, "双", x + 80, y, font, System.Drawing.Color.Red);                 // 双（红色）
            }
        }
        
        /// <summary>
        /// 🔥 绘制龙虎（参考 F5BotV2 SetLotteryLh 第1778-1789行）
        /// 龙：红色
        /// 虎：黑色
        /// </summary>
        private void DrawLotteryDragonTiger(System.Drawing.Graphics g, int x, int y, System.Drawing.Font font, string text)
        {
            if (text == "龙")
            {
                DrawText(g, text, x, y, font, System.Drawing.Color.Red);     // 龙（红色）
            }
            else if (text == "虎")
            {
                DrawText(g, text, x, y, font, System.Drawing.Color.Black);   // 虎（黑色）
            }
        }
        
        /// <summary>
        /// 🔥 发送封盘提醒消息（30秒/15秒）- 参考 F5BotV2 第1008/1013行
        /// 格式：{issueid % 1000} 还剩30秒 或 {issueid % 1000} 还剩15秒
        /// </summary>
        private async Task SendSealingReminderAsync(int issueId, int seconds)
        {
            try
            {
                string? groupWxId = _groupBindingService?.CurrentBoundGroup?.Wxid;
                if (string.IsNullOrEmpty(groupWxId) || _socketClient == null || !_socketClient.IsConnected)
                {
                    _logService.Debug("BinggoLotteryService", "未绑定群或微信未登录，跳过发送封盘提醒");
                    return;
                }
                
                // 🔥 检查是否应该发送系统消息
                if (!ShouldSendSystemMessage())
                {
                    return;
                }
                
                // 🔥 格式完全按照 F5BotV2：{issueid%1000} 还剩30秒 或 {issueid%1000} 还剩15秒
                int issueShort = issueId % 1000;
                string message = $"{issueShort} 还剩{seconds}秒";
                
                _logService.Info("BinggoLotteryService", $"📢 发送封盘提醒: {groupWxId} - {message}");
                
                var response = await _socketClient.SendAsync<object>("SendMessage", groupWxId, message);
                if (response != null)
                {
                    _logService.Info("BinggoLotteryService", $"✅ 封盘提醒已发送: {message}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"发送封盘提醒失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 发送封盘消息（参考 F5BotV2 第1205-1238行 On封盘中）
        /// 格式：{issueid % 1000} 时间到! 停止进仓! 以此为准!\r{订单列表}\r------线下无效------
        /// 即使没有订单也要发送
        /// </summary>
        private async Task SendSealingMessageAsync(int issueId)
        {
            try
            {
                string? groupWxId = _groupBindingService?.CurrentBoundGroup?.Wxid;
                if (string.IsNullOrEmpty(groupWxId) || _socketClient == null || !_socketClient.IsConnected)
                {
                    _logService.Debug("BinggoLotteryService", "未绑定群或微信未登录，跳过发送封盘消息");
                    return;
                }
                
                // 🔥 检查是否应该发送系统消息
                if (!ShouldSendSystemMessage())
                {
                    return;
                }
                
                _logService.Info("BinggoLotteryService", $"📢 发送封盘消息: 期号 {issueId}");
                
                // 🔥 格式完全按照 F5BotV2 第1226-1238行
                var sbTxt = new StringBuilder();
                int issueShort = issueId % 1000;
                sbTxt.Append($"{issueShort} 时间到! 停止进仓! 以此为准!\r");
                
                // 🔥 获取当期所有订单（参考 F5BotV2 第1228行）
                var orders = _ordersBindingList?
                    .Where(p => p.IssueId == issueId && p.OrderStatus != OrderStatus.已取消)
                    .OrderBy(o => o.Id)  // 排序（参考 F5BotV2 第1230行）
                    .ToList();
                
                if (orders != null && orders.Count > 0)
                {
                    // 🔥 格式：{nickname}[{(int)BetFronMoney}]:{BetContentStandar}|计:{AmountTotal}\r
                    foreach (var ods in orders)
                    {
                        sbTxt.Append($"{ods.Nickname ?? "未知"}[{(int)ods.BetFronMoney}]:{ods.BetContentStandar ?? ""}|计:{ods.AmountTotal}\r");
                    }
                }
                
                // 🔥 即使没有订单也要发送（参考 F5BotV2 第1237行）
                sbTxt.Append("------线下无效------");
                
                _logService.Info("BinggoLotteryService", $"📤 发送封盘消息到群: {groupWxId}");
                
                var response = await _socketClient.SendAsync<object>("SendMessage", groupWxId, sbTxt.ToString());
                if (response != null)
                {
                    _logService.Info("BinggoLotteryService", $"✅ 封盘消息已发送: 期号 {issueId}, 订单数 {orders?.Count ?? 0}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"发送封盘消息失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 清空会员表当期投注金额（参考 F5BotV2 第 1477-1480 行）
        /// </summary>
        private void ClearMembersBetCur()
        {
            try
            {
                if (_membersBindingList == null) return;
                
                int clearedCount = 0;
                foreach (var member in _membersBindingList)
                {
                    member.BetCur = 0;  // 清空当期投注金额
                    clearedCount++;
                }
                
                _logService.Info("BinggoLotteryService", $"✅ 已清空 {clearedCount} 个会员的当期投注金额");
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"清空会员投注金额失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 获取指定日期的所有开奖数据
        /// </summary>
        public async Task<List<BinggoLotteryData>> GetLotteryDataByDateAsync(DateTime date)
        {
            try
            {
                // 🔥 使用 BoterApi 单例
                var api = Services.Api.BoterApi.GetInstance();
                string dateStr = date.ToString("yyyy-MM-dd");
                var response = await api.GetBgDayAsync(dateStr, 203, false);
                
                if (response.Code == 0 && response.Data != null)
                {
                    await SaveLotteryDataListAsync(response.Data);
                    return response.Data;
                }
                
                return new List<BinggoLotteryData>();
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"获取 {date:yyyy-MM-dd} 数据失败: {ex.Message}", ex);
                return new List<BinggoLotteryData>();
            }
        }
        
        /// <summary>
        /// 保存开奖数据到本地缓存
        /// </summary>
        public async Task SaveLotteryDataAsync(BinggoLotteryData data)
        {
            await Task.Run(() =>
            {
                if (_db == null || !data.IsOpened) return;
                
                try
                {
                    var existing = _db.Table<BinggoLotteryData>()
                        .FirstOrDefault(d => d.IssueId == data.IssueId);
                    
                    if (existing == null)
                    {
                        _db.Insert(data);
                        _logService.Info("BinggoLotteryService", $"💾 保存开奖数据: {data.IssueId}");
                    }
                    else
                    {
                        data.Id = existing.Id;
                        _db.Update(data);
                        _logService.Info("BinggoLotteryService", $"🔄 更新开奖数据: {data.IssueId}");
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error("BinggoLotteryService", $"保存开奖数据失败: {ex.Message}", ex);
                }
            });
        }
        
        /// <summary>
        /// 批量保存开奖数据到本地缓存并更新 BindingList
        /// </summary>
        public async Task SaveLotteryDataListAsync(List<BinggoLotteryData> dataList)
        {
            await Task.Run(() =>
            {
                if (_db == null) return;
                
                int savedCount = 0;
                int updatedCount = 0;
                
                foreach (var data in dataList.Where(d => d.IsOpened))
                {
                    try
                    {
                        var existing = _db.Table<BinggoLotteryData>()
                            .FirstOrDefault(d => d.IssueId == data.IssueId);
                        
                        if (existing == null)
                        {
                            _db.Insert(data);
                            savedCount++;
                        }
                        else
                        {
                            data.Id = existing.Id;
                            _db.Update(data);
                            updatedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Warning("BinggoLotteryService", 
                            $"保存期号 {data.IssueId} 数据失败: {ex.Message}");
                    }
                }
                
                _logService.Info("BinggoLotteryService", 
                    $"💾 批量保存到数据库: 新增 {savedCount} 期，更新 {updatedCount} 期");
            });
            
            // 🔥 更新 BindingList（在主线程上执行，BindingList 会自动通知 UI）
            if (_bindingList != null)
            {
                foreach (var data in dataList.Where(d => d.IsOpened))
                {
                    try
                    {
                        var existingInList = _bindingList.FirstOrDefault(d => d.IssueId == data.IssueId);
                        if (existingInList == null)
                        {
                            _bindingList.Add(data);
                        }
                        else
                        {
                            // 更新现有项
                            int index = _bindingList.IndexOf(existingInList);
                            _bindingList[index] = data;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Warning("BinggoLotteryService", 
                            $"更新 BindingList 期号 {data.IssueId} 失败: {ex.Message}");
                    }
                }
                
                _logService.Info("BinggoLotteryService", 
                    $"✅ BindingList 更新完成，共 {dataList.Count} 期数据");
            }
        }
    }
}

