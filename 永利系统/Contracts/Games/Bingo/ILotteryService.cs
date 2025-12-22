using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using 永利系统.Models.Games.Bingo;
using 永利系统.Models.Games.Bingo.Events;

namespace 永利系统.Contracts.Games.Bingo
{
    /// <summary>
    /// 开奖服务接口
    /// 
    /// 核心功能：
    /// 1. 定时获取当前期号和状态
    /// 2. 计算倒计时
    /// 3. 触发状态变更事件（开奖中、封盘中、等待中、开盘中、即将封盘）
    /// 4. 管理开奖数据（本地缓存优先）
    /// </summary>
    public interface ILotteryService
    {
        // ========================================
        // 属性
        // ========================================

        /// <summary>
        /// 当前期号
        /// </summary>
        int CurrentIssueId { get; }

        /// <summary>
        /// 当前状态
        /// </summary>
        LotteryStatus CurrentStatus { get; }

        /// <summary>
        /// 距离封盘秒数
        /// </summary>
        int SecondsToSeal { get; }

        /// <summary>
        /// 服务是否运行中
        /// </summary>
        bool IsRunning { get; }

        // ========================================
        // 方法
        // ========================================

        /// <summary>
        /// 获取状态快照（线程安全，原子性）
        /// </summary>
        /// <returns>(当前状态, 当前期号, 是否可下注)</returns>
        (LotteryStatus status, int issueId, bool canBet) GetStatusSnapshot();

        /// <summary>
        /// 启动开奖服务
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 停止开奖服务
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 获取指定期号的开奖数据
        /// </summary>
        /// <param name="issueId">期号</param>
        /// <param name="forceRefresh">是否强制从网络刷新</param>
        Task<LotteryData?> GetLotteryDataAsync(int issueId, bool forceRefresh = false);

        /// <summary>
        /// 获取最近 N 期的开奖数据
        /// </summary>
        /// <param name="count">数量</param>
        Task<List<LotteryData>> GetRecentLotteryDataAsync(int count = 10);

        /// <summary>
        /// 获取指定日期的所有开奖数据
        /// </summary>
        /// <param name="date">日期</param>
        Task<List<LotteryData>> GetLotteryDataByDateAsync(DateTime date);

        /// <summary>
        /// 保存开奖数据到本地缓存
        /// </summary>
        /// <param name="data">开奖数据</param>
        Task SaveLotteryDataAsync(LotteryData data);

        // ========================================
        // 事件
        // ========================================

        /// <summary>
        /// 期号变更事件
        /// </summary>
        event EventHandler<BingoLotteryIssueChangedEventArgs>? IssueChanged;

        /// <summary>
        /// 状态变更事件 (开盘/封盘/开奖)
        /// </summary>
        event EventHandler<BingoLotteryStatusChangedEventArgs>? StatusChanged;

        /// <summary>
        /// 倒计时更新事件 (每秒触发)
        /// </summary>
        event EventHandler<BingoLotteryCountdownEventArgs>? CountdownTick;

        /// <summary>
        /// 开奖数据到达事件
        /// </summary>
        event EventHandler<BingoLotteryOpenedEventArgs>? LotteryOpened;
    }
}

