using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models.Games.Binggo;
using BaiShengVx3Plus.Models.Games.Binggo.Events;
using BaiShengVx3Plus.Core;
using BaiShengVx3Plus.Helpers;
using SQLite;

namespace BaiShengVx3Plus.Services.Games.Binggo
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
    public class BinggoLotteryService : IBinggoLotteryService
    {
        private readonly ILogService _logService;
        private readonly BinggoGameSettings _settings;
        private SQLiteConnection? _db;
        private Core.BinggoLotteryDataBindingList? _bindingList;  // 🔥 UI 数据绑定
        
        private System.Threading.Timer? _timer;
        private int _currentIssueId;
        private BinggoLotteryStatus _currentStatus = BinggoLotteryStatus.等待中;
        private int _secondsToSeal;
        private bool _isRunning;
        private readonly object _lock = new object();
        
        // 🔥 时间提醒标志（防止重复触发，参考 F5BotV2）
        private bool _reminded30Seconds = false;
        private bool _reminded15Seconds = false;
        
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
            BinggoGameSettings settings)
        {
            _logService = logService;
            _settings = settings;
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
        }
        
        public Task StopAsync()
        {
            _logService.Info("BinggoLotteryService", "🛑 开奖服务停止");
            _isRunning = false;
            _timer?.Dispose();
            _timer = null;
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
        /// 🔥 完全参考 F5BotV2 的逻辑：期号变更时同时设置当期和上期数据
        /// </summary>
        private async Task HandleIssueChangeAsync(int oldIssueId, int newIssueId)
        {
            try
            {
                _logService.Info("BinggoLotteryService", $"🔄 期号变更: {oldIssueId} → {newIssueId}");
                
                // 🔥 参考 F5BotV2: 同时创建当期和上期数据对象
                // 1. 创建上期数据（用于 UcBinggoDataLast 显示）
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
                
                // 🔥 异步加载上期开奖数据
                // 当数据到达时，会触发 LotteryOpened 事件，UI 会再次更新
                await LoadPreviousLotteryDataAsync(oldIssueId);
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"期号变更处理异常: {ex.Message}", ex);
            }
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
                if (data != null && !string.IsNullOrEmpty(data.LotteryData))
                {
                    _logService.Info("BinggoLotteryService", $"💾 本地已有开奖数据: {issueId}");
                    LotteryOpened?.Invoke(this, new BinggoLotteryOpenedEventArgs
                    {
                        LotteryData = data
                    });
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
                        
                        // 触发开奖事件
                        LotteryOpened?.Invoke(this, new BinggoLotteryOpenedEventArgs
                        {
                            LotteryData = data
                        });
                        
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
            
            if (secondsToSeal > 30)
            {
                // 开盘中（距离封盘超过 30 秒）
                newStatus = BinggoLotteryStatus.开盘中;
                
                // 重置提醒标志（新一期开始）
                _reminded30Seconds = false;
                _reminded15Seconds = false;
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
                    
                    // 触发状态变更事件（带提醒消息）
                    StatusChanged?.Invoke(this, new BinggoStatusChangedEventArgs
                    {
                        OldStatus = oldStatus,
                        NewStatus = newStatus,
                        IssueId = _currentIssueId,
                        Message = $"还剩 30 秒封盘"
                    });
                }
                
                // ========================================
                // 🔥 15 秒提醒（参考 F5BotV2: sec < 15 && !b15）
                // ========================================
                if (secondsToSeal < 15 && !_reminded15Seconds)
                {
                    _reminded15Seconds = true;
                    _logService.Info("BinggoLotteryService", $"⏰ 15秒提醒: 期号 {_currentIssueId}");
                    
                    // 触发状态变更事件（带提醒消息）
                    StatusChanged?.Invoke(this, new BinggoStatusChangedEventArgs
                    {
                        OldStatus = oldStatus,
                        NewStatus = newStatus,
                        IssueId = _currentIssueId,
                        Message = $"还剩 15 秒封盘"
                    });
                }
            }
            else if (secondsToSeal > -_settings.SealSecondsAhead)
            {
                // 封盘中（0 到 -配置的封盘秒数，等待开奖）
                newStatus = BinggoLotteryStatus.封盘中;
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
        
        /// <summary>
        /// 处理期号变更（旧版 - 保留兼容）
        /// </summary>
        private void OnIssueChanged(BinggoLotteryData newData)
        {
            _logService.Info("BinggoLotteryService", $"📢 期号变更: {_currentIssueId} → {newData.IssueId}");
            
            // 获取上期开奖数据（先查本地缓存）
            var lastDataTask = GetLotteryDataAsync(_currentIssueId, forceRefresh: false);
            lastDataTask.Wait();
            var lastData = lastDataTask.Result;
            
            // 触发期号变更事件
            IssueChanged?.Invoke(this, new BinggoIssueChangedEventArgs
            {
                OldIssueId = _currentIssueId,
                NewIssueId = newData.IssueId,
                LastLotteryData = lastData
            });
            
            // 重置状态为开盘
            var oldStatus = _currentStatus;
            _currentStatus = BinggoLotteryStatus.开盘中;
            
            StatusChanged?.Invoke(this, new BinggoStatusChangedEventArgs
            {
                OldStatus = oldStatus,
                NewStatus = BinggoLotteryStatus.开盘中,
                IssueId = newData.IssueId,
                Data = newData
            });
        }
        
        /// <summary>
        /// 检查状态变更
        /// </summary>
        private void CheckStatusChange(BinggoLotteryData data)
        {
            var oldStatus = _currentStatus;
            
            // 检查封盘
            if (_secondsToSeal <= 0 && _currentStatus == BinggoLotteryStatus.开盘中)
            {
                _currentStatus = BinggoLotteryStatus.封盘中;
                _logService.Info("BinggoLotteryService", $"🔒 封盘: 期号 {_currentIssueId}");
                
                StatusChanged?.Invoke(this, new BinggoStatusChangedEventArgs
                {
                    OldStatus = oldStatus,
                    NewStatus = BinggoLotteryStatus.封盘中,
                    IssueId = _currentIssueId,
                    Data = data
                });
            }
            
            // 检查开奖
            if (data.IsOpened && _currentStatus != BinggoLotteryStatus.开奖中)
            {
                _currentStatus = BinggoLotteryStatus.开奖中;
                _logService.Info("BinggoLotteryService", $"🎲 开奖: {data.ToLotteryString()}");
                
                // 保存到本地缓存
                var saveTask = SaveLotteryDataAsync(data);
                saveTask.Wait();
                
                // 🔥 更新 UI BindingList（线程安全）
                if (_bindingList != null)
                {
                    _bindingList.AddOrUpdate(data);
                }
                
                // 触发开奖事件
                LotteryOpened?.Invoke(this, new BinggoLotteryOpenedEventArgs
                {
                    LotteryData = data
                });
            }
        }
        
        /// <summary>
        /// 计算距离封盘的秒数
        /// 🔥 已废弃：改用 BinggoTimeHelper 的本地计算
        /// </summary>
        private int CalculateSecondsToSeal(BinggoLotteryData data)
        {
            // 这个方法已不再使用，保留仅用于兼容性
            return 0;
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
                    var local = _db.Table<BinggoLotteryData>()
                        .FirstOrDefault(d => d.IssueId == issueId && d.IsOpened);
                    
                    if (local != null)
                    {
                        _logService.Info("BinggoLotteryService", $"✓ 从本地缓存获取期号 {issueId} 数据");
                        return local;
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
                    var local = _db.Table<BinggoLotteryData>()
                        .Where(d => d.IsOpened)
                        .OrderByDescending(d => d.IssueId)
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
                        var local = _db.Table<BinggoLotteryData>()
                            .Where(d => d.IsOpened)
                            .OrderByDescending(d => d.IssueId)
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
                    
                    // 触发开奖事件，通知 UI 更新
                    LotteryOpened?.Invoke(this, new BinggoLotteryOpenedEventArgs
                    {
                        LotteryData = lastData
                    });
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

