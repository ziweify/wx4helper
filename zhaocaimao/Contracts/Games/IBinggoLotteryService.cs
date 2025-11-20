using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using zhaocaimao.Models;
using zhaocaimao.Models.Games.Binggo;
using zhaocaimao.Models.Games.Binggo.Events;
using zhaocaimao.Core;

namespace zhaocaimao.Contracts.Games
{
    /// <summary>
    /// 炳狗开奖服务接口
    /// 
    /// 核心功能：
    /// 1. 定时获取当前期号和状态
    /// 2. 计算倒计时
    /// 3. 触发状态变更事件
    /// 4. 管理开奖数据（本地缓存优先）
    /// </summary>
    public interface IBinggoLotteryService
    {
        // ========================================
        // 🔥 属性
        // ========================================
        
        /// <summary>
        /// 当前期号
        /// </summary>
        int CurrentIssueId { get; }
        
        /// <summary>
        /// 当前状态
        /// </summary>
        BinggoLotteryStatus CurrentStatus { get; }
        
        /// <summary>
        /// 距离封盘秒数
        /// </summary>
        int SecondsToSeal { get; }
        
        /// <summary>
        /// 服务是否运行中
        /// </summary>
        bool IsRunning { get; }
        
        // ========================================
        // 🔥 方法
        // ========================================
        
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
        /// 
        /// 🔥 策略：先查本地缓存，没有再请求网络
        /// </summary>
        /// <param name="issueId">期号</param>
        /// <param name="forceRefresh">是否强制从网络刷新</param>
        Task<BinggoLotteryData?> GetLotteryDataAsync(int issueId, bool forceRefresh = false);
        
        /// <summary>
        /// 获取最近 N 期的开奖数据
        /// </summary>
        /// <param name="count">数量</param>
        Task<List<BinggoLotteryData>> GetRecentLotteryDataAsync(int count = 10);
        
        /// <summary>
        /// 获取指定日期的所有开奖数据
        /// </summary>
        /// <param name="date">日期</param>
        Task<List<BinggoLotteryData>> GetLotteryDataByDateAsync(DateTime date);
        
        /// <summary>
        /// 保存开奖数据到本地缓存
        /// </summary>
        /// <param name="data">开奖数据</param>
        Task SaveLotteryDataAsync(BinggoLotteryData data);
        
        /// <summary>
        /// 批量保存开奖数据到本地缓存
        /// </summary>
        /// <param name="dataList">开奖数据列表</param>
        Task SaveLotteryDataListAsync(List<BinggoLotteryData> dataList);
        
        /// <summary>
        /// 设置数据库连接 (ORM)
        /// </summary>
        void SetDatabase(SQLite.SQLiteConnection? db);
        
        /// <summary>
        /// 设置 BindingList 用于自动 UI 更新
        /// </summary>
        void SetBindingList(BinggoLotteryDataBindingList? bindingList);
        
        /// <summary>
        /// 🔥 处理所有微信消息（统一入口：查、上分、下分、取消、投注）
        /// 所有炳狗相关的业务逻辑都通过这个方法处理
        /// </summary>
        /// <param name="member">会员</param>
        /// <param name="messageContent">消息内容</param>
        /// <returns>(是否已处理, 回复消息, 订单对象)</returns>
        Task<(bool handled, string? replyMessage, V2MemberOrder? order)> ProcessMessageAsync(
            V2Member member,
            string messageContent);
        
        // ========================================
        // 🔥 事件
        // ========================================
        
        /// <summary>
        /// 期号变更事件
        /// </summary>
        event EventHandler<BinggoIssueChangedEventArgs>? IssueChanged;
        
        /// <summary>
        /// 状态变更事件 (开盘/封盘/开奖)
        /// </summary>
        event EventHandler<BinggoStatusChangedEventArgs>? StatusChanged;
        
        /// <summary>
        /// 倒计时更新事件 (每秒触发)
        /// </summary>
        event EventHandler<BinggoCountdownEventArgs>? CountdownTick;
        
        /// <summary>
        /// 开奖数据到达事件
        /// </summary>
        event EventHandler<BinggoLotteryOpenedEventArgs>? LotteryOpened;
    }
}

