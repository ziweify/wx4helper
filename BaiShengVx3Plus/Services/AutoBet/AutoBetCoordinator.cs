using System;
using System.Linq;
using System.Threading.Tasks;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models;
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
        private readonly IBinggoOrderService _orderService;
        private readonly BetRecordService _betRecordService;
        private readonly OrderMerger _orderMerger;
        private readonly BetQueueManager _betQueueManager;
        private readonly ILogService _log;
        
        private bool _isAutoBetEnabled = false;
        private int _currentConfigId = -1;
        
        public bool IsEnabled => _isAutoBetEnabled;
        
        public AutoBetCoordinator(
            AutoBetService autoBetService,
            IBinggoLotteryService lotteryService,
            IBinggoOrderService orderService,
            BetRecordService betRecordService,
            OrderMerger orderMerger,
            BetQueueManager betQueueManager,
            ILogService log)
        {
            _autoBetService = autoBetService;
            _lotteryService = lotteryService;
            _orderService = orderService;
            _betRecordService = betRecordService;
            _orderMerger = orderMerger;
            _betQueueManager = betQueueManager;
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
        /// 状态变更事件 - 封盘时处理订单和推送投注命令
        /// </summary>
        private async void LotteryService_StatusChanged(object? sender, BinggoStatusChangedEventArgs e)
        {
            _log.Info("AutoBet", $"📢 状态变更事件触发: {e.OldStatus} → {e.NewStatus}, 期号:{e.IssueId}");
            _log.Info("AutoBet", $"   自动投注启用: {_isAutoBetEnabled}");
            _log.Info("AutoBet", $"   当前配置ID: {_currentConfigId}");
            
            if (!_isAutoBetEnabled)
            {
                _log.Warning("AutoBet", "⚠️ 自动投注未启用，跳过处理");
                return;
            }
            
            // 只在"即将封盘"状态时处理投注
            if (e.NewStatus == BinggoLotteryStatus.即将封盘)
            {
                _log.Info("AutoBet", $"🎯 触发封盘事件: 期号={e.IssueId}");
                
                try
                {
                    // 1. 查询待处理订单
                    _log.Info("AutoBet", $"📋 开始查询待投注订单...");
                    var pendingOrders = _orderService.GetPendingOrdersForIssue(e.IssueId);
                    
                    if (!pendingOrders.Any())
                    {
                        _log.Warning("AutoBet", $"⚠️ 期号{e.IssueId}没有待投注订单");
                        return;
                    }
                    
                    _log.Info("AutoBet", $"✅ 查询到{pendingOrders.Count()}个待投注订单");
                    
                    // 2. 扩展业务规则：按会员等级处理订单（示例）
                    // 例如：蓝会会员金额>500，多打到配置B
                    //var blueMemberLargeOrders = pendingOrders.Where(o =>
                    //    o.MemberState == MemberState.蓝会 &&
                    //    o.AmountTotal > 500 &&
                    //    o.OrderType != OrderType.托  // 排除托单
                    //).ToList();
                    
                    //if (blueMemberLargeOrders.Any())
                    //{
                    //    _log.Info("AutoBet", $"📢 检测到{blueMemberLargeOrders.Count}个蓝会大额订单(>500元)");
                    //    // TODO: 多打到配置B的逻辑
                    //    // await DuplicateOrdersToConfigB(blueMemberLargeOrders);
                    //}
                    
                    // 3. 合并订单
                    _log.Info("AutoBet", $"📦 开始合并订单...");
                    var mergeResult = _orderMerger.Merge(pendingOrders);
                    
                    if (string.IsNullOrEmpty(mergeResult.BetContentStandard))
                    {
                        _log.Warning("AutoBet", "⚠️ 订单合并失败或内容为空");
                        return;
                    }
                    
                    _log.Info("AutoBet", $"✅ 订单合并完成:");
                    _log.Info("AutoBet", $"   订单数量: {mergeResult.OrderIds.Count}个");
                    _log.Info("AutoBet", $"   合并内容: {mergeResult.BetContentStandard}");
                    _log.Info("AutoBet", $"   总金额: {mergeResult.TotalAmount:F2}元");
                    
                    // 4. 创建投注记录
                    _log.Info("AutoBet", $"📋 创建投注记录...");
                    var betRecord = new BetRecord
                    {
                        ConfigId = _currentConfigId,
                        IssueId = e.IssueId,
                        Source = BetRecordSource.订单,
                        OrderIds = string.Join(",", mergeResult.OrderIds),
                        BetContentStandard = mergeResult.BetContentStandard,
                        TotalAmount = mergeResult.TotalAmount,
                        SendTime = DateTime.Now
                    };
                    
                    betRecord = _betRecordService.Create(betRecord);
                    _log.Info("AutoBet", $"✅ 投注记录已创建: ID={betRecord.Id}");
                    
                    // 5. 通过 Socket 发送投注命令到浏览器
                    _log.Info("AutoBet", $"📤 发送投注命令到浏览器客户端:");
                    _log.Info("AutoBet", $"   配置ID: {_currentConfigId}");
                    _log.Info("AutoBet", $"   期号: {e.IssueId}");
                    _log.Info("AutoBet", $"   内容: {mergeResult.BetContentStandard}");
                    
                    _betQueueManager.EnqueueBet(betRecord.Id, async () =>
                    {
                        _log.Info("AutoBet", $"🚀 开始执行投注...");
                        
                        // 这里调用 Socket 发送"投注"命令
                        var result = await _autoBetService.SendBetCommandAsync(
                            _currentConfigId,
                            e.IssueId.ToString(),
                            mergeResult.BetContentStandard
                        );
                        
                        _log.Info("AutoBet", $"📥 投注命令返回: Success={result.Success}");
                        if (!string.IsNullOrEmpty(result.ErrorMessage))
                        {
                            _log.Warning("AutoBet", $"   错误信息: {result.ErrorMessage}");
                        }
                        
                        // 根据结果更新订单状态
                        if (result.Success)
                        {
                            _log.Info("AutoBet", $"✅ 投注成功，开始更新订单状态...");
                            
                            // 投注成功，更新订单为"盘内 + 待结算"
                            foreach (var orderId in mergeResult.OrderIds)
                            {
                                var order = pendingOrders.FirstOrDefault(o => o.Id == orderId);
                                if (order != null)
                                {
                                    order.OrderType = OrderType.盘内;  // 🔥 进盘成功
                                    order.OrderStatus = OrderStatus.待结算;
                                    _orderService.UpdateOrder(order);
                                }
                            }
                            _log.Info("AutoBet", $"✅ 已更新{mergeResult.OrderIds.Count}个订单为【盘内+待结算】状态");
                        }
                        else
                        {
                            _log.Warning("AutoBet", $"❌ 投注失败，开始更新订单状态...");
                            
                            // 投注失败，更新订单为"盘外"并标记为"已完成"
                            foreach (var orderId in mergeResult.OrderIds)
                            {
                                var order = pendingOrders.FirstOrDefault(o => o.Id == orderId);
                                if (order != null)
                                {
                                    order.OrderType = OrderType.盘外;  // 设置为盘外
                                    order.OrderStatus = OrderStatus.已完成;  // 标记完成（不需要结算）
                                    _orderService.UpdateOrder(order);
                                }
                            }
                            _log.Warning("AutoBet", $"❌ 已更新{mergeResult.OrderIds.Count}个订单为【盘外已完成】状态");
                        }
                        
                        return result;
                    });
                }
                catch (Exception ex)
                {
                    _log.Error("AutoBet", $"处理封盘事件失败:期号{e.IssueId}", ex);
                }
            }
        }
    }
}

