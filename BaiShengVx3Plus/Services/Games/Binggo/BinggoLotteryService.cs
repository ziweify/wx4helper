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
        private readonly IBsWebApiClient _apiClient;
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
            IBsWebApiClient apiClient, 
            ILogService logService,
            BinggoGameSettings settings)
        {
            _apiClient = apiClient;
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
                int secondsToSeal = BinggoTimeHelper.GetSecondsToSeal(localIssueId, _settings.SealSecondsAhead);
                
                lock (_lock)
                {
                    // 检查期号变更
                    if (localIssueId != _currentIssueId)
                    {
                        if (_currentIssueId != 0)
                        {
                            // 期号变更，触发开奖逻辑
                            var previousIssueId = _currentIssueId;
                            _currentIssueId = localIssueId;
                            _ = HandleIssueChangeAsync(previousIssueId, localIssueId);  // 异步处理开奖
                        }
                        else
                        {
                            // 首次初始化
                            _currentIssueId = localIssueId;
                            _logService.Info("BinggoLotteryService", $"✅ 初始化当前期号: {localIssueId}");
                            
                            // 立即加载上期数据
                            _ = LoadPreviousLotteryDataAsync(BinggoTimeHelper.GetPreviousIssueId(localIssueId));
                        }
                    }
                    
                    // 更新倒计时
                    _secondsToSeal = secondsToSeal;
                    
                    // 检查状态变更
                    UpdateStatus(secondsToSeal);
                    
                    // 触发倒计时事件
                    CountdownTick?.Invoke(this, new BinggoCountdownEventArgs
                    {
                        Seconds = _secondsToSeal,
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
        /// </summary>
        private async Task HandleIssueChangeAsync(int oldIssueId, int newIssueId)
        {
            try
            {
                _logService.Info("BinggoLotteryService", $"🔄 期号变更: {oldIssueId} → {newIssueId}");
                
                // 触发期号变更事件
                IssueChanged?.Invoke(this, new BinggoIssueChangedEventArgs
                {
                    OldIssueId = oldIssueId,
                    NewIssueId = newIssueId,
                    LastLotteryData = null
                });
                
                // 异步加载上期开奖数据
                await LoadPreviousLotteryDataAsync(oldIssueId);
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"期号变更处理异常: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 加载上期数据（本地优先 + API补充）
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
                
                // 步骤2: 如果本地没有开奖数据，从API获取
                if (data == null || string.IsNullOrEmpty(data.NumbersString))
                {
                    _logService.Info("BinggoLotteryService", $"📡 从API获取开奖数据: {issueId}");
                    var response = await _apiClient.GetBinggoDataAsync<BinggoLotteryData>(issueId);
                    
                    if (response.IsSuccess && response.Data != null)
                    {
                        data = response.Data;
                        data.OpenTime = BinggoTimeHelper.GetIssueOpenTime(issueId);
                        
                        // 保存到数据库
                        if (_db != null && !string.IsNullOrEmpty(data.NumbersString))
                        {
                            _db.InsertOrReplace(data);
                            _bindingList?.LoadFromDatabase(100);
                            _logService.Info("BinggoLotteryService", $"💾 开奖数据已保存: {issueId}");
                        }
                    }
                }
                
                // 步骤3: 如果有开奖数据，触发开奖事件
                if (data != null && !string.IsNullOrEmpty(data.NumbersString))
                {
                    _logService.Info("BinggoLotteryService", $"🎲 开奖: {issueId}, 号码: {data.NumbersString}");
                    LotteryOpened?.Invoke(this, new BinggoLotteryOpenedEventArgs
                    {
                        LotteryData = data
                    });
                }
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
            else if (secondsToSeal > -45)
            {
                // 封盘中（0 到 -45 秒，等待开奖）
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
                _logService.Info("BinggoLotteryService", 
                    $"🎲 开奖: {data.IssueId} - {data.NumbersString} (总:{data.Sum} {data.BigSmall}{data.OddEven} {data.DragonTiger})");
                
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
        /// </summary>
        private int CalculateSecondsToSeal(BinggoLotteryData data)
        {
            // 根据期号开始时间 + 期号时长 - 提前封盘时间
            var elapsed = (DateTime.Now - data.IssueStartTime).TotalSeconds;
            var totalDuration = _settings.IssueDuration;  // 默认 300 秒（5分钟）
            var sealAhead = _settings.SealSecondsAhead;   // 默认 30 秒
            
            var secondsRemaining = totalDuration - elapsed - sealAhead;
            return secondsRemaining > 0 ? (int)secondsRemaining : 0;
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
                var response = await _apiClient.GetBinggoDataAsync<BinggoLotteryData>(issueId);
                
                if (response.IsSuccess && response.Data != null && response.Data.IsOpened)
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
                // 优先从网络获取最新数据
                var response = await _apiClient.GetRecentBinggoDataAsync<List<BinggoLotteryData>>(count);
                
                if (response.IsSuccess && response.Data != null)
                {
                    // 保存到本地缓存
                    await SaveLotteryDataListAsync(response.Data);
                    return response.Data;
                }
                
                // 如果网络失败，从本地读取
                if (_db != null)
                {
                    var local = _db.Table<BinggoLotteryData>()
                        .Where(d => d.IsOpened)
                        .OrderByDescending(d => d.IssueId)
                        .Take(count)
                        .ToList();
                    
                    _logService.Info("BinggoLotteryService", $"从本地缓存获取最近 {count} 期数据");
                    return local;
                }
                
                return new List<BinggoLotteryData>();
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryService", $"获取最近 {count} 期数据失败: {ex.Message}", ex);
                return new List<BinggoLotteryData>();
            }
        }
        
        /// <summary>
        /// 获取指定日期的所有开奖数据
        /// </summary>
        public async Task<List<BinggoLotteryData>> GetLotteryDataByDateAsync(DateTime date)
        {
            try
            {
                var response = await _apiClient.GetBinggoDataListAsync<List<BinggoLotteryData>>(date);
                
                if (response.IsSuccess && response.Data != null)
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
        /// 批量保存开奖数据到本地缓存
        /// </summary>
        public async Task SaveLotteryDataListAsync(List<BinggoLotteryData> dataList)
        {
            await Task.Run(() =>
            {
                if (_db == null) return;
                
                foreach (var data in dataList.Where(d => d.IsOpened))
                {
                    try
                    {
                        var existing = _db.Table<BinggoLotteryData>()
                            .FirstOrDefault(d => d.IssueId == data.IssueId);
                        
                        if (existing == null)
                        {
                            _db.Insert(data);
                        }
                        else
                        {
                            data.Id = existing.Id;
                            _db.Update(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Warning("BinggoLotteryService", 
                            $"保存期号 {data.IssueId} 数据失败: {ex.Message}");
                    }
                }
                
                _logService.Info("BinggoLotteryService", $"💾 批量保存 {dataList.Count} 期数据");
            });
        }
    }
}

