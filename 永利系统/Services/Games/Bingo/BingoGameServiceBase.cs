using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using 永利系统.Models.Games.Bingo;
using 永利系统.Models.Games.Bingo.Events;
using 永利系统.Models.Games.Bingo.Exceptions;
using 永利系统.Services;

namespace 永利系统.Services.Games.Bingo
{
    /// <summary>
    /// Bingo 游戏服务基类（混合模式：虚方法 + 事件）
    /// 
    /// 核心功能：
    /// 1. 管理游戏状态（开盘中、封盘中、开奖中等）
    /// 2. 期号计算和监控
    /// 3. 倒计时计算和提醒（30秒/15秒）
    /// 4. 开奖数据更新
    /// 5. 事件分发（虚方法给派生类 + 事件给外部订阅者）
    /// 
    /// 设计模式：
    /// - 模板方法模式：基类实现通用逻辑，派生类重写虚方法实现具体业务
    /// - 观察者模式：通过事件通知外部订阅者（UI、日志等）
    /// </summary>
    public abstract class BingoGameServiceBase
    {
        #region 字段

        protected readonly LoggingService _loggingService;
        protected readonly BingoGameServiceBase? _lotteryService;

        // 状态管理
        private volatile int _currentIssueId;
        private volatile LotteryStatus _currentStatus = LotteryStatus.等待中;
        private volatile bool _isRunning;
        private volatile bool _isEnabled;
        private int _betAheadSeconds = 30; // 默认提前30秒封盘

        // 线程管理
        private Task? _statusMonitorTask;
        private Task? _dataUpdateTask;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly object _statusLock = new object();

        // 数据更新队列
        private readonly ConcurrentDictionary<int, LotteryData> _pendingUpdates = new ConcurrentDictionary<int, LotteryData>();

        // 提醒标志
        private bool _warn30Seconds = false;
        private bool _warn15Seconds = false;

        #endregion

        #region 属性

        /// <summary>
        /// 当前期号
        /// </summary>
        public int CurrentIssueId
        {
            get
            {
                lock (_statusLock)
                {
                    return _currentIssueId;
                }
            }
            protected set
            {
                lock (_statusLock)
                {
                    _currentIssueId = value;
                }
            }
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        public LotteryStatus CurrentStatus
        {
            get
            {
                lock (_statusLock)
                {
                    return _currentStatus;
                }
            }
            protected set
            {
                lock (_statusLock)
                {
                    _currentStatus = value;
                }
            }
        }

        /// <summary>
        /// 距离封盘秒数
        /// </summary>
        public int SecondsToSeal { get; protected set; }

        /// <summary>
        /// 服务是否运行中
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 服务是否启用
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                    return;
                _isEnabled = value;
                _loggingService.Info("Bingo游戏服务", $"服务已{(value ? "启用" : "禁用")}");
            }
        }

        /// <summary>
        /// 提前封盘秒数（默认30秒）
        /// </summary>
        public int BetAheadSeconds
        {
            get => _betAheadSeconds;
            set
            {
                if (value < 0)
                    value = 0;
                _betAheadSeconds = value;
                _loggingService.Info("Bingo游戏服务", $"提前封盘时间设置为 {value} 秒");
            }
        }

        #endregion

        #region 事件（给外部订阅者：UI、日志等）

        /// <summary>
        /// 期号变更事件
        /// </summary>
        public event EventHandler<BingoLotteryIssueChangedEventArgs>? IssueChanged;

        /// <summary>
        /// 状态变更事件
        /// </summary>
        public event EventHandler<BingoLotteryStatusChangedEventArgs>? StatusChanged;

        /// <summary>
        /// 倒计时更新事件（每秒触发）
        /// </summary>
        public event EventHandler<BingoLotteryCountdownEventArgs>? CountdownTick;

        /// <summary>
        /// 开奖数据到达事件
        /// </summary>
        public event EventHandler<BingoLotteryOpenedEventArgs>? LotteryOpened;

        /// <summary>
        /// 警告事件（30秒/15秒提醒）
        /// </summary>
        public event EventHandler<BingoGameWarningEventArgs>? Warning;

        #endregion

        #region 构造函数

        protected BingoGameServiceBase(LoggingService loggingService, BingoGameServiceBase? lotteryService = null)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _lotteryService = lotteryService;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置提前封盘秒数
        /// </summary>
        /// <param name="seconds">提前秒数（默认30秒）</param>
        public void SetBetAheadSeconds(int seconds)
        {
            BetAheadSeconds = seconds;
        }

        /// <summary>
        /// 设置启用状态
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public Task StartAsync()
        {
            return StartAsync(CancellationToken.None);
        }

        /// <summary>
        /// 启动服务（带取消令牌）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        public virtual async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
            {
                _loggingService.Warn("Bingo游戏服务", "服务已在运行中");
                return;
            }

            _isRunning = true;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _loggingService.Info("Bingo游戏服务", "Bingo游戏服务已启动");

            // 启动状态监控任务
            _statusMonitorTask = Task.Run(() => StatusMonitorLoopAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            // 启动数据更新任务
            _dataUpdateTask = Task.Run(() => DataUpdateLoopAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public virtual async Task StopAsync()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _cancellationTokenSource?.Cancel();

            try
            {
                if (_statusMonitorTask != null)
                {
                    await _statusMonitorTask;
                }
                if (_dataUpdateTask != null)
                {
                    await _dataUpdateTask;
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，忽略
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _statusMonitorTask = null;
                _dataUpdateTask = null;
            }

            _loggingService.Info("Bingo游戏服务", "Bingo游戏服务已停止");
        }

        /// <summary>
        /// 获取状态快照（线程安全）
        /// </summary>
        public (LotteryStatus status, int issueId, bool canBet) GetStatusSnapshot()
        {
            lock (_statusLock)
            {
                var status = _currentStatus;
                var issueId = _currentIssueId;
                var canBet = status == LotteryStatus.开盘中 || status == LotteryStatus.即将封盘;
                return (status, issueId, canBet);
            }
        }

        #endregion

        #region 虚方法（给派生类重写实现具体业务逻辑）

        /// <summary>
        /// 期号变更时的处理（派生类重写实现具体业务逻辑）
        /// </summary>
        /// <param name="status">当前状态</param>
        /// <param name="newIssueId">新区号</param>
        protected virtual void On期号变更(LotteryStatus status, int newIssueId)
        {
            // 派生类重写实现具体业务逻辑
        }

        /// <summary>
        /// 状态变更时的处理（派生类重写实现具体业务逻辑）
        /// </summary>
        /// <param name="status">新状态</param>
        /// <param name="issueId">期号</param>
        protected virtual void On状态变更(LotteryStatus status, int issueId)
        {
            // 派生类重写实现具体业务逻辑
        }

        /// <summary>
        /// 开奖数据更新时的处理（派生类重写实现具体业务逻辑）
        /// </summary>
        /// <param name="data">开奖数据</param>
        protected virtual void On更新开奖数据(LotteryData data)
        {
            // 派生类重写实现具体业务逻辑
        }

        /// <summary>
        /// 提醒消息处理（派生类重写实现具体业务逻辑）
        /// </summary>
        /// <param name="message">提醒消息</param>
        protected virtual void On提醒消息(string message)
        {
            // 派生类重写实现具体业务逻辑
        }

        #endregion

        #region 内部方法（状态监控和数据更新）

        /// <summary>
        /// 状态监控循环（每秒执行）
        /// </summary>
        private async Task StatusMonitorLoopAsync(CancellationToken cancellationToken)
        {
            _loggingService.Debug("Bingo游戏服务", "状态监控任务已启动");

            while (!cancellationToken.IsCancellationRequested && _isRunning)
            {
                try
                {
                    if (!_isEnabled)
                    {
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    DateTime now = DateTime.Now;
                    int nextIssueId = CalculateNextIssueId(now);
                    DateTime issueOpenTime = CalculateIssueOpenTime(nextIssueId);
                    TimeSpan timeToOpen = issueOpenTime - now;
                    int totalSeconds = (int)timeToOpen.TotalSeconds;
                    int userSeconds = totalSeconds - _betAheadSeconds; // 用户可下注的剩余秒数

                    // 检查期号是否变更
                    if (CurrentIssueId != nextIssueId)
                    {
                        _期号变更(CurrentStatus, nextIssueId);
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    // 计算状态
                    LotteryStatus newStatus = CalculateStatus(userSeconds, totalSeconds);
                    SecondsToSeal = Math.Max(0, userSeconds);

                    // 更新状态
                    if (CurrentStatus != newStatus)
                    {
                        _状态变更(newStatus, nextIssueId);
                    }

                    // 触发倒计时事件
                    CountdownTick?.Invoke(this, new BingoLotteryCountdownEventArgs(SecondsToSeal, nextIssueId, CurrentStatus));

                    // 30秒提醒
                    if (userSeconds < 30 && userSeconds > 0 && !_warn30Seconds)
                    {
                        string message = $"封盘倒计时: {userSeconds}秒";
                        On提醒消息(message);
                        Warning?.Invoke(this, new BingoGameWarningEventArgs(userSeconds, nextIssueId, message, WarningType.Warning30Seconds));
                        _warn30Seconds = true;
                    }
                    else if (userSeconds >= 30)
                    {
                        _warn30Seconds = false;
                    }

                    // 15秒提醒
                    if (userSeconds < 15 && userSeconds > 0 && !_warn15Seconds)
                    {
                        string message = $"封盘倒计时: {userSeconds}秒";
                        On提醒消息(message);
                        Warning?.Invoke(this, new BingoGameWarningEventArgs(userSeconds, nextIssueId, message, WarningType.Warning15Seconds));
                        _warn15Seconds = true;
                    }
                    else if (userSeconds >= 15)
                    {
                        _warn15Seconds = false;
                    }

                    // 检查是否需要获取开奖数据
                    if (newStatus == LotteryStatus.开奖中)
                    {
                        int previousIssueId = nextIssueId - 1;
                        if (!_pendingUpdates.ContainsKey(previousIssueId))
                        {
                            _pendingUpdates.TryAdd(previousIssueId, new LotteryData { IssueId = previousIssueId });
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _loggingService.Error("Bingo游戏服务", $"状态监控异常: {ex.Message}");
                }

                await Task.Delay(1000, cancellationToken);
            }

            _loggingService.Debug("Bingo游戏服务", "状态监控任务已停止");
        }

        /// <summary>
        /// 数据更新循环（每秒执行）
        /// </summary>
        private async Task DataUpdateLoopAsync(CancellationToken cancellationToken)
        {
            _loggingService.Debug("Bingo游戏服务", "数据更新任务已启动");

            while (!cancellationToken.IsCancellationRequested && _isRunning)
            {
                try
                {
                    if (_pendingUpdates.Count > 0)
                    {
                        var lastItem = _pendingUpdates.LastOrDefault();
                        if (lastItem.Key == lastItem.Value.IssueId && lastItem.Value.IssueId > 0)
                        {
                            // 数据已更新，触发事件
                            if (_pendingUpdates.TryRemove(lastItem.Key, out var lotteryData))
                            {
                                _更新开奖数据(lotteryData);
                            }
                        }
                        else if (lastItem.Key > 0)
                        {
                            // 需要获取开奖数据
                            if (_lotteryService != null)
                            {
                                var data = await _lotteryService.GetLotteryDataAsync(lastItem.Key, forceRefresh: true);
                                if (data != null)
                                {
                                    _pendingUpdates.AddOrUpdate(lastItem.Key, data, (key, old) => data);
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _loggingService.Error("Bingo游戏服务", $"数据更新异常: {ex.Message}");
                }

                await Task.Delay(1000, cancellationToken);
            }

            _loggingService.Debug("Bingo游戏服务", "数据更新任务已停止");
        }

        /// <summary>
        /// 计算下一个期号（派生类可以重写实现不同的计算逻辑）
        /// </summary>
        protected virtual int CalculateNextIssueId(DateTime now)
        {
            // TODO: 实现期号计算逻辑
            // 这里应该根据实际游戏规则计算期号
            _loggingService.Debug("Bingo游戏服务", "计算期号（需要实现具体逻辑）");
            return 0;
        }

        /// <summary>
        /// 计算期号开盘时间（派生类可以重写实现不同的计算逻辑）
        /// </summary>
        protected virtual DateTime CalculateIssueOpenTime(int issueId)
        {
            // TODO: 实现开盘时间计算逻辑
            _loggingService.Debug("Bingo游戏服务", "计算开盘时间（需要实现具体逻辑）");
            return DateTime.Now;
        }

        /// <summary>
        /// 计算当前状态
        /// </summary>
        private LotteryStatus CalculateStatus(int userSeconds, int totalSeconds)
        {
            if (totalSeconds > 300) // 超过5分钟，可能是封盘中
            {
                return LotteryStatus.封盘中;
            }

            if (userSeconds > 0)
            {
                if (userSeconds <= 15)
                {
                    return LotteryStatus.即将封盘;
                }
                return LotteryStatus.开盘中;
            }
            else if (userSeconds <= 0 && userSeconds > -_betAheadSeconds)
            {
                return LotteryStatus.封盘中;
            }
            else
            {
                return LotteryStatus.开奖中;
            }
        }

        #endregion

        #region 私有方法（触发虚方法和事件）

        /// <summary>
        /// 处理期号变更（先调用虚方法，再触发事件）
        /// </summary>
        private void _期号变更(LotteryStatus status, int newIssueId)
        {
            if (CurrentIssueId == newIssueId)
                return;

            int oldIssueId = CurrentIssueId;
            CurrentIssueId = newIssueId;

            // 添加到待更新队列
            _pendingUpdates.TryAdd(newIssueId - 1, new LotteryData { IssueId = newIssueId - 1 });

            // 1. 先调用虚方法（派生类的业务逻辑）
            On期号变更(status, newIssueId);

            // 2. 再触发事件（外部订阅者：UI、日志等）
            IssueChanged?.Invoke(this, new BingoLotteryIssueChangedEventArgs(oldIssueId, newIssueId));
        }

        /// <summary>
        /// 处理状态变更（先调用虚方法，再触发事件）
        /// </summary>
        private void _状态变更(LotteryStatus newStatus, int issueId)
        {
            lock (_statusLock)
            {
                if (CurrentStatus == newStatus)
                    return;

                LotteryStatus oldStatus = CurrentStatus;
                CurrentStatus = newStatus;

                // 1. 先调用虚方法（派生类的业务逻辑）
                On状态变更(newStatus, issueId);

                // 2. 再触发事件（外部订阅者：UI、日志等）
                StatusChanged?.Invoke(this, new BingoLotteryStatusChangedEventArgs(oldStatus, newStatus, issueId, SecondsToSeal));
            }
        }

        /// <summary>
        /// 处理开奖数据更新（先调用虚方法，再触发事件）
        /// </summary>
        private void _更新开奖数据(LotteryData data)
        {
            // 1. 先调用虚方法（派生类的业务逻辑）
            On更新开奖数据(data);

            // 2. 再触发事件（外部订阅者：UI、日志等）
            LotteryOpened?.Invoke(this, new BingoLotteryOpenedEventArgs(data));

            // 如果下一期已开始，可以更新状态为开盘中
            if (data.IssueId + 1 == CurrentIssueId)
            {
                // 可以在这里更新状态
                // _状态变更(LotteryStatus.开盘中, CurrentIssueId);
            }
        }

        #endregion
        
        #region 数据查询方法（虚方法，派生类可重写）
        
        /// <summary>
        /// 获取指定期号的开奖数据（虚方法，派生类可重写）
        /// </summary>
        public virtual async Task<LotteryData?> GetLotteryDataAsync(int issueId, bool forceRefresh = false)
        {
            // 基类提供默认实现：返回 null，派生类应重写此方法实现实际逻辑
            await Task.CompletedTask;
            return null;
        }
        
        /// <summary>
        /// 获取最近 N 期的开奖数据（虚方法，派生类可重写）
        /// </summary>
        public virtual async Task<List<LotteryData>> GetRecentLotteryDataAsync(int count = 10)
        {
            // 基类提供默认实现：返回空列表，派生类应重写此方法实现实际逻辑
            await Task.CompletedTask;
            return new List<LotteryData>();
        }
        
        /// <summary>
        /// 获取指定日期的所有开奖数据（虚方法，派生类可重写）
        /// </summary>
        public virtual async Task<List<LotteryData>> GetLotteryDataByDateAsync(DateTime date)
        {
            // 基类提供默认实现：返回空列表，派生类应重写此方法实现实际逻辑
            await Task.CompletedTask;
            return new List<LotteryData>();
        }
        
        /// <summary>
        /// 保存开奖数据到本地缓存（虚方法，派生类可重写）
        /// </summary>
        public virtual async Task SaveLotteryDataAsync(LotteryData data)
        {
            // 基类提供默认实现：不执行任何操作，派生类应重写此方法实现实际逻辑
            await Task.CompletedTask;
        }
        
        #endregion
    }
}

