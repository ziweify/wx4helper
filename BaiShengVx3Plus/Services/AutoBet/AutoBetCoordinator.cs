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
        private readonly IConfigurationService _configService;  // 🔥 配置服务（全局游戏规则）
        private readonly ILogService _log;
        
        private bool _isAutoBetEnabled = false;
        private int _currentConfigId = -1;
        private BinggoLotteryStatus _lastStatus = BinggoLotteryStatus.等待中;  // 🔥 记录上次状态，防止重复触发
        private bool _hasProcessedCurrentIssue = false;  // 🔥 记录当前期号是否已处理投注
        
        public bool IsEnabled => _isAutoBetEnabled;
        
        public AutoBetCoordinator(
            AutoBetService autoBetService,
            IBinggoLotteryService lotteryService,
            IBinggoOrderService orderService,
            BetRecordService betRecordService,
            OrderMerger orderMerger,
            BetQueueManager betQueueManager,
            IConfigurationService configService,
            ILogService log)
        {
            _autoBetService = autoBetService;
            _lotteryService = lotteryService;
            _orderService = orderService;
            _betRecordService = betRecordService;
            _orderMerger = orderMerger;
            _betQueueManager = betQueueManager;
            _configService = configService;
            _log = log;
        }
        
        /// <summary>
        /// 启动自动投注
        /// </summary>
        public Task<bool> StartAsync(int configId)
        {
            try
            {
                _log.Info("AutoBet", $"🚀 启动自动投注，配置ID: {configId}");
                
                // 🔥 1. 设置 IsEnabled = true（触发监控任务启动浏览器）
                var config = _autoBetService.GetConfig(configId);
                if (config == null)
                {
                    _log.Error("AutoBet", $"❌ 配置不存在: {configId}");
                    return Task.FromResult(false);
                }
                
                if (!config.IsEnabled)
                {
                    _log.Info("AutoBet", $"📌 设置配置 [{config.ConfigName}] 为启用状态");
                    config.IsEnabled = true;  // PropertyChanged 自动保存，监控任务会检测到并启动浏览器
                    _autoBetService.SaveConfig(config);
                }
                
                // 🔥 2. 订阅开奖事件（这才是 Coordinator 的职责）
                _lotteryService.IssueChanged += LotteryService_IssueChanged;
                _lotteryService.StatusChanged += LotteryService_StatusChanged;
                
                _currentConfigId = configId;
                _isAutoBetEnabled = true;
                
                _log.Info("AutoBet", $"✅ 自动投注已启动");
                _log.Info("AutoBet", $"   _currentConfigId = {_currentConfigId}");
                _log.Info("AutoBet", $"   浏览器将由监控线程自动管理（检测间隔：2秒）");
                _log.Info("AutoBet", $"   ⚠️ 封盘投注时将使用此 configId 查找浏览器");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", "启动自动投注失败", ex);
                return Task.FromResult(false);
            }
        }
        
        /// <summary>
        /// 停止自动投注（只设置状态，不关闭浏览器）
        /// 
        /// 🔥 设计原则：
        /// - 飞单开关只是状态标识，表示是否准备好进行飞单
        /// - 关闭飞单：只停止处理订单，浏览器保持运行
        /// - 浏览器只在用户明确要求停止时才关闭（如配置管理器中的"停止浏览器"按钮）
        /// </summary>
        public void Stop()
        {
            _log.Info("AutoBet", "⏹️ 停止自动投注（只设置状态，不关闭浏览器）");
            
            _isAutoBetEnabled = false;
            
            // 取消订阅事件（不再处理封盘投注）
            _lotteryService.IssueChanged -= LotteryService_IssueChanged;
            _lotteryService.StatusChanged -= LotteryService_StatusChanged;
            
            // 🔥 只设置配置状态为禁用，不关闭浏览器
            if (_currentConfigId > 0)
            {
                var config = _autoBetService.GetConfig(_currentConfigId);
                if (config != null)
                {
                    config.IsEnabled = false;  // 设置状态，监控任务会看到
                    _autoBetService.SaveConfig(config);
                    _log.Info("AutoBet", $"✅ 配置 [{config.ConfigName}] 已设置为禁用状态（浏览器保持运行）");
                }
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
            
            // 🔥 新一期开始，重置状态和投注标记
            _lastStatus = BinggoLotteryStatus.等待中;
            _hasProcessedCurrentIssue = false;
            _log.Info("AutoBet", $"🔓 已重置状态和投注标记，允许新期号投注");
            
            // TODO: 可以在这里做一些准备工作
            // 例如：检查浏览器状态、刷新余额等
        }
        
        /// <summary>
        /// 状态变更事件 - 封盘时处理订单和推送投注命令
        /// 🔥 投注时机：在"封盘中"状态时投注（参考 F5BotV2 第1205行 On封盘中 → 第1244行 BetOrder）
        /// </summary>
        private async void LotteryService_StatusChanged(object? sender, BinggoStatusChangedEventArgs e)
        {
            _log.Info("AutoBet", $"📢 状态变更事件触发: {e.OldStatus} → {e.NewStatus}, 期号:{e.IssueId}");
            
            if (!_isAutoBetEnabled)
            {
                _log.Warning("AutoBet", "⚠️ 自动投注未启用，跳过处理");
                return;
            }
            
            // 🔥 投注时机：在"封盘中"状态时投注（参考 F5BotV2 第1205行 On封盘中）
            // 防止重复投注：双重检查
            // 1. 检查状态是否真正变化（从非"封盘中"变为"封盘中"）
            // 2. 检查当前期号是否已经处理过投注
            if (e.NewStatus == BinggoLotteryStatus.封盘中)
            {
                // 如果已经处理过当前期号，直接跳过
                if (_hasProcessedCurrentIssue)
                {
                    _log.Warning("AutoBet", $"⚠️ 期号{e.IssueId}已处理过投注，跳过重复处理");
                    return;
                }
                
                // 只在第一次进入"封盘中"状态时处理
                if (_lastStatus == BinggoLotteryStatus.封盘中)
                {
                    _log.Warning("AutoBet", $"⚠️ 已经在'封盘中'状态，跳过重复触发");
                    return;
                }
                
                _log.Info("AutoBet", $"   上次状态: {_lastStatus}");
                _log.Info("AutoBet", $"   当前状态: {e.NewStatus}");
                _log.Info("AutoBet", $"   配置ID: {_currentConfigId}");
                _log.Info("AutoBet", $"🎯 触发封盘投注: 期号={e.IssueId}");
                
                // 🔥 更新状态标记
                _lastStatus = e.NewStatus;
                _hasProcessedCurrentIssue = true;  // 标记已处理
                
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
                    
                    // 🔥 移除冗余验证：订单已在下单时通过 BinggoOrderValidator 验证过金额限制
                    // 订单表中的订单都是合法的，封盘时直接投注即可
                    // 参考 F5BotV2：订单进入订单表前已验证（单注金额、单期总金额），封盘时不再验证
                    
                    var config = _autoBetService.GetConfig(_currentConfigId);
                    if (config == null)
                    {
                        _log.Error("AutoBet", "❌ 配置不存在");
                        return;
                    }
                    
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
                    
                    if (betRecord == null)
                    {
                        _log.Error("AutoBet", "❌ 创建投注记录失败，数据库未初始化");
                        return;
                    }
                    
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
                        
                        // 🔥 根据POST结果更新订单状态（参考F5BotV2逻辑）
                        if (result.Success)
                        {
                            _log.Info("AutoBet", $"✅ POST成功，更新订单状态为【盘内+待结算】");
                            
                            // POST成功 → 盘内 + 待结算（等待开奖后计算盈利）
                            foreach (var orderId in mergeResult.OrderIds)
                            {
                                var order = pendingOrders.FirstOrDefault(o => o.Id == orderId);
                                if (order != null)
                                {
                                    order.OrderStatus = OrderStatus.待结算;  // 等待开奖结算
                                    order.OrderType = OrderType.盘内;      // 成功进入网盘
                                    _orderService.UpdateOrder(order);
                                }
                            }
                            _log.Info("AutoBet", $"✅ 已更新{mergeResult.OrderIds.Count}个订单为【盘内+待结算】");
                        }
                        else
                        {
                            _log.Warning("AutoBet", $"❌ POST失败，更新订单状态为【盘外+待结算】");
                            
                            // POST失败 → 盘外 + 待结算（开奖后仍需处理，如退款）
                            foreach (var orderId in mergeResult.OrderIds)
                            {
                                var order = pendingOrders.FirstOrDefault(o => o.Id == orderId);
                                if (order != null)
                                {
                                    order.OrderStatus = OrderStatus.待结算;  // 仍需开奖后处理
                                    order.OrderType = OrderType.盘外;      // 未进入网盘
                                    _orderService.UpdateOrder(order);
                                }
                            }
                            _log.Info("AutoBet", $"✅ 已更新{mergeResult.OrderIds.Count}个订单为【盘外+待结算】");
                        }
                        
                        return result;
                    });
                }
                catch (Exception ex)
                {
                    _log.Error("AutoBet", $"处理封盘事件失败:期号{e.IssueId}", ex);
                }
            }
            else
            {
                // 🔥 其他状态变更时，仅更新状态标记（不触发投注）
                if (_lastStatus != e.NewStatus)
                {
                    _log.Debug("AutoBet", $"状态变更: {_lastStatus} → {e.NewStatus}");
                    _lastStatus = e.NewStatus;
                }
            }
        }
    }
}

