using System;
using System.Threading.Tasks;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models.AutoBet;
using BaiShengVx3Plus.Models.Games.Binggo;
using BaiShengVx3Plus.Models.Games.Binggo.Events;
using BaiShengVx3Plus.Services.Games.Binggo;

namespace BaiShengVx3Plus.Services.AutoBet
{
    /// <summary>
    /// 自动投注协调器 - 连接开奖服务和投注服务
    /// </summary>
    public class AutoBetCoordinator
    {
        private readonly AutoBetService _autoBetService;
        private readonly IBinggoLotteryService _lotteryService;
        private readonly ILogService _log;
        
        private bool _isAutoBetEnabled = false;
        private int _currentConfigId = -1;
        
        public bool IsEnabled => _isAutoBetEnabled;
        
        public AutoBetCoordinator(
            AutoBetService autoBetService,
            IBinggoLotteryService lotteryService,
            ILogService log)
        {
            _autoBetService = autoBetService;
            _lotteryService = lotteryService;
            _log = log;
        }
        
        /// <summary>
        /// 启动自动投注
        /// </summary>
        public async Task<bool> StartAsync(int configId)
        {
            try
            {
                _log.Info("AutoBet", $"🚀 启动自动投注，配置ID: {configId}");
                
                // 1. 启动浏览器
                var success = await _autoBetService.StartBrowser(configId);
                if (!success)
                {
                    _log.Error("AutoBet", "启动浏览器失败");
                    return false;
                }
                
                // 2. 订阅开奖事件
                _lotteryService.IssueChanged += LotteryService_IssueChanged;
                _lotteryService.StatusChanged += LotteryService_StatusChanged;
                
                _currentConfigId = configId;
                _isAutoBetEnabled = true;
                
                _log.Info("AutoBet", "✅ 自动投注已启动");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", "启动自动投注失败", ex);
                return false;
            }
        }
        
        /// <summary>
        /// 停止自动投注
        /// </summary>
        public void Stop()
        {
            _log.Info("AutoBet", "⏹️ 停止自动投注");
            
            _isAutoBetEnabled = false;
            
            // 取消订阅
            _lotteryService.IssueChanged -= LotteryService_IssueChanged;
            _lotteryService.StatusChanged -= LotteryService_StatusChanged;
            
            // 停止浏览器
            if (_currentConfigId > 0)
            {
                _autoBetService.StopBrowser(_currentConfigId);
                _currentConfigId = -1;
            }
        }
        
        /// <summary>
        /// 期号变更事件 - 新一期开始
        /// </summary>
        private void LotteryService_IssueChanged(object? sender, BinggoIssueChangedEventArgs e)
        {
            if (!_isAutoBetEnabled) return;
            
            _log.Info("AutoBet", $"🔔 新一期开始: {e.NewIssueId}");
            
            // TODO: 可以在这里做一些准备工作
            // 例如：检查浏览器状态、刷新余额等
        }
        
        /// <summary>
        /// 状态变更事件 - 封盘时自动投注
        /// </summary>
        private async void LotteryService_StatusChanged(object? sender, BinggoStatusChangedEventArgs e)
        {
            if (!_isAutoBetEnabled) return;
            
            // 只在"即将封盘"状态时执行投注
            if (e.NewStatus == BinggoLotteryStatus.即将封盘)
            {
                _log.Info("AutoBet", $"🎯 触发自动投注: {e.IssueId}");
                
                await ExecuteAutoBetAsync(e.IssueId);
            }
        }
        
        /// <summary>
        /// 执行自动投注
        /// </summary>
        private async Task ExecuteAutoBetAsync(int issueId)
        {
            try
            {
                // TODO: 这里需要根据实际业务逻辑决定投注内容
                // 目前先实现一个简单的测试投注
                
                var order = new BetOrder
                {
                    IssueId = issueId.ToString(),
                    PlayType = "大小",
                    BetContent = "大",
                    Amount = 1  // 测试金额
                };
                
                _log.Info("AutoBet", $"📤 自动投注: {order.PlayType} {order.BetContent} {order.Amount}元");
                
                var result = await _autoBetService.PlaceBet(_currentConfigId, order);
                
                if (result.Success)
                {
                    _log.Info("AutoBet", $"✅ 自动投注成功! 订单号: {result.OrderId}");
                }
                else
                {
                    _log.Warning("AutoBet", $"❌ 自动投注失败: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", "执行自动投注异常", ex);
            }
        }
        
        /// <summary>
        /// 手动投注
        /// </summary>
        public async Task<BetResult> PlaceBetManualAsync(BetOrder order)
        {
            if (!_isAutoBetEnabled || _currentConfigId <= 0)
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "自动投注未启动"
                };
            }
            
            return await _autoBetService.PlaceBet(_currentConfigId, order);
        }
    }
}

