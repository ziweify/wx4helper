using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Core;
using BaiShengVx3Plus.Helpers;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Models.Games.Binggo;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Services.Games.Binggo
{
    /// <summary>
    /// 炳狗订单服务
    /// 
    /// 功能：
    /// 1. 创建订单（微信下注）
    /// 2. 补单（手动创建）
    /// 3. 结算订单（批量+单个）
    /// 4. 查询订单
    /// </summary>
    public class BinggoOrderService : IBinggoOrderService
    {
        private readonly ILogService _logService;
        private readonly IBinggoLotteryService _lotteryService;
        private readonly BinggoOrderValidator _validator;
        private readonly BinggoGameSettings _settings;
        private BinggoStatisticsService? _statisticsService; // 🔥 统计服务（可选，通过 SetStatisticsService 设置）
        private SQLiteConnection? _db;
        private V2OrderBindingList? _ordersBindingList;
        private V2MemberBindingList? _membersBindingList;
        
        public BinggoOrderService(
            ILogService logService,
            IBinggoLotteryService lotteryService,
            BinggoOrderValidator validator,
            BinggoGameSettings settings)
        {
            _logService = logService;
            _lotteryService = lotteryService;
            _validator = validator;
            _settings = settings;
        }
        
        /// <summary>
        /// 设置统计服务
        /// </summary>
        public void SetStatisticsService(BinggoStatisticsService? statisticsService)
        {
            _statisticsService = statisticsService;
        }
        
        /// <summary>
        /// 设置数据库连接
        /// </summary>
        public void SetDatabase(SQLiteConnection? db)
        {
            _db = db;
        }
        
        /// <summary>
        /// 设置订单 BindingList（用于 UI 自动更新）
        /// </summary>
        public void SetOrdersBindingList(V2OrderBindingList? bindingList)
        {
            _ordersBindingList = bindingList;
        }
        
        /// <summary>
        /// 设置会员 BindingList（用于更新余额）
        /// </summary>
        public void SetMembersBindingList(V2MemberBindingList? bindingList)
        {
            _membersBindingList = bindingList;
        }
        
        /// <summary>
        /// 创建订单（从微信消息）
        /// </summary>
        public async Task<(bool success, string message, V2MemberOrder? order)> CreateOrderAsync(
            V2Member member,
            string messageContent,
            int issueId,
            BinggoLotteryStatus currentStatus)
        {
            try
            {
                _logService.Info("BinggoOrderService", 
                    $"处理下注: {member.Nickname} ({member.Wxid}) - 期号: {issueId}");
                
                // 1. 解析下注内容
                var betContent = BinggoHelper.ParseBetContent(messageContent, issueId);
                
                if (betContent.Code != 0)
                {
                    _logService.Warning("BinggoOrderService", 
                        $"解析下注失败: {betContent.ErrorMessage}");
                    return (false, betContent.ErrorMessage, null);
                }
                
                // 2. 验证下注
                if (!_validator.ValidateBet(member, betContent, currentStatus, out string errorMessage))
                {
                    _logService.Warning("BinggoOrderService", 
                        $"验证下注失败: {errorMessage}");
                    return (false, errorMessage, null);
                }
                
                // 3. 创建订单（完全参考 F5BotV2 的 V2MemberOrder 构造函数）
                long timestampBet = DateTimeOffset.Now.ToUnixTimeSeconds();
                
                // 🔥 记录注前金额和注后金额
                float betFronMoney = member.Balance;  // 下注前余额
                float betAfterMoney = member.Balance - (float)betContent.TotalAmount;  // 下注后余额（暂存）
                
                var order = new V2MemberOrder
                {
                    // 🔥 会员信息
                    Wxid = member.Wxid,
                    Account = member.Account,  // 🔥 修复：添加账号
                    Nickname = member.Nickname,
                    GroupWxId = member.GroupWxId,
                    
                    // 🔥 订单基础信息
                    IssueId = issueId,
                    TimeStampBet = timestampBet,
                    TimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    CreatedAt = DateTime.Now,
                    
                    // 🔥 投注内容（参考 F5BotV2）
                    BetContentOriginal = messageContent,  // 🔥 原始内容："6大50"
                    BetContentStandar = betContent.ToStandardString(),  // 🔥 标准内容："6,大,50"
                    Nums = betContent.Items.Count,  // 🔥 修复：注数
                    AmountTotal = (float)betContent.TotalAmount,  // 🔥 修复：总金额（float类型）
                    
                    // 🔥 金额记录（参考 F5BotV2）
                    BetFronMoney = betFronMoney,   // 注前金额
                    BetAfterMoney = betAfterMoney, // 注后金额
                    
                    // 🔥 结算信息
                    Profit = 0,  // 未结算
                    NetProfit = 0,  // 未结算
                    Odds = 1.97f,  // 🔥 修复：赔率（参考 F5BotV2 默认值）
                    OrderStatus = OrderStatus.待结算,
                    OrderType = OrderType.盘内,
                    IsSettled = false,
                    
                    // 🔥 开奖服务专用字段（保留兼容）
                    BetContent = betContent.ToStandardString(),
                    BetAmount = betContent.TotalAmount
                };
                
                // 4. 扣除余额（如果不是托或管理）
                if (member.State != MemberState.托 && member.State != MemberState.管理)
                {
                    member.Balance -= (float)betContent.TotalAmount;
                    _logService.Info("BinggoOrderService", 
                        $"扣除余额: {member.Nickname} - {betContent.TotalAmount:F2}，剩余: {member.Balance:F2}");
                }
                
                // 🔥 5. 增加待结算金额和统计（参考 F5BotV2 第 546 行）
                member.BetWait += (float)betContent.TotalAmount;
                member.BetToday += (float)betContent.TotalAmount;
                member.BetTotal += (float)betContent.TotalAmount;
                member.BetCur += (float)betContent.TotalAmount;  // 本期下注
                
                _logService.Info("BinggoOrderService", 
                    $"📊 统计更新: {member.Nickname} - 待结算 {member.BetWait:F2} - 今日下注 {member.BetToday:F2}");
                
                // 6. 保存订单（插入到列表顶部，保持"最新在上"）
                if (_ordersBindingList != null && _ordersBindingList.Count > 0)
                {
                    _ordersBindingList.Insert(0, order);  // 🔥 插入到顶部
                }
                else
                {
                    _ordersBindingList?.Add(order);  // 🔥 空列表时使用 Add
                }
                
                _logService.Info("BinggoOrderService", 
                    $"✅ 订单创建成功: {member.Nickname} - {betContent.ToStandardString()} - {betContent.TotalAmount:F2}元");
                
                // 🔥 7. 更新统计（参考 F5BotV2 第 569 行）
                _statisticsService?.UpdateStatistics();
                
                // 8. 生成回复消息（🔥 完全参考 F5BotV2 格式）
                // 格式：@昵称\r已进仓{注数}\r{投注内容}|扣:{金额}|留:{余额}
                string replyMessage = $"@{member.Nickname}\r已进仓{order.Nums}\r{betContent.ToReplyString()}|扣:{(int)order.AmountTotal}|留:{(int)member.Balance}";
                
                return (true, replyMessage, order);
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", 
                    $"创建订单失败: {ex.Message}", ex);
                return (false, "系统错误，请稍后重试", null);
            }
        }
        
        /// <summary>
        /// 补单（手动创建）
        /// </summary>
        public async Task<(bool success, string message, V2MemberOrder? order)> CreateManualOrderAsync(
            V2Member member,
            int issueId,
            string betContent,
            decimal amount)
        {
            try
            {
                _logService.Info("BinggoOrderService", 
                    $"补单: {member.Nickname} ({member.Wxid}) - 期号: {issueId}");
                
                // 1. 验证补单
                if (!_validator.ValidateManualOrder(member, issueId, amount, out string errorMessage))
                {
                    return (false, errorMessage, null);
                }
                
                // 2. 获取开奖数据（优先从本地缓存）
                var lotteryData = await _lotteryService.GetLotteryDataAsync(issueId, forceRefresh: false);
                
                if (lotteryData == null || !lotteryData.IsOpened)
                {
                    return (false, $"期号 {issueId} 未开奖，请先在开奖页面手动录入开奖数据！", null);
                }
                
                // 3. 创建订单
                var order = new V2MemberOrder
                {
                    Wxid = member.Wxid,
                    Nickname = member.Nickname,
                    GroupWxId = member.GroupWxId,
                    IssueId = issueId,
                    BetContent = betContent,
                    BetAmount = amount,
                    Profit = 0,  // 稍后结算
                    IsSettled = false,
                    CreatedAt = DateTime.Now
                };
                
                // 4. 立即结算
                await SettleSingleOrderAsync(order, lotteryData);
                
                // 5. 保存订单（插入到列表顶部，保持"最新在上"）
                if (_ordersBindingList != null && _ordersBindingList.Count > 0)
                {
                    _ordersBindingList.Insert(0, order);  // 🔥 插入到顶部
                }
                else
                {
                    _ordersBindingList?.Add(order);  // 🔥 空列表时使用 Add
                }
                
                _logService.Info("BinggoOrderService", 
                    $"✅ 补单成功: {member.Nickname} - {betContent} - {amount:F2}元 - 盈利: {order.Profit:F2}");
                
                return (true, $"补单成功，盈利: {order.Profit:F2}", order);
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", 
                    $"补单失败: {ex.Message}", ex);
                return (false, $"补单失败: {ex.Message}", null);
            }
        }
        
        /// <summary>
        /// 结算指定期号的所有订单
        /// </summary>
        public async Task<(int settledCount, string summary)> SettleOrdersAsync(
            int issueId,
            BinggoLotteryData? lotteryData)
        {
            try
            {
                _logService.Info("BinggoOrderService", 
                    $"开始结算期号: {issueId}");
                
                // 1. 获取开奖数据
                if (lotteryData == null)
                {
                    lotteryData = await _lotteryService.GetLotteryDataAsync(issueId, forceRefresh: true);
                }
                
                if (lotteryData == null || !lotteryData.IsOpened)
                {
                    _logService.Warning("BinggoOrderService", 
                        $"期号 {issueId} 未开奖，无法结算");
                    return (0, "开奖数据未找到");
                }
                
                // 2. 查询未结算的订单
                var unsetledOrders = _ordersBindingList?
                    .Where(o => o.IssueId == issueId && !o.IsSettled)
                    .ToList();
                
                if (unsetledOrders == null || unsetledOrders.Count == 0)
                {
                    _logService.Info("BinggoOrderService", 
                        $"期号 {issueId} 没有待结算订单");
                    return (0, "没有待结算订单");
                }
                
                // 3. 逐个结算
                int settledCount = 0;
                decimal totalProfit = 0;
                
                foreach (var order in unsetledOrders)
                {
                    await SettleSingleOrderAsync(order, lotteryData);
                    settledCount++;
                    totalProfit += (decimal)order.Profit;
                }
                
                _logService.Info("BinggoOrderService", 
                    $"✅ 结算完成: 期号 {issueId}，共 {settledCount} 单，总盈利: {totalProfit:F2}");
                
                // 🔥 4. 更新统计（参考 F5BotV2 第 635 行）
                _statisticsService?.UpdateStatistics();
                
                string summary = $"期号: {issueId}\n" +
                               $"订单数: {settledCount}\n" +
                               $"总盈利: {totalProfit:F2}\n" +
                               $"开奖: {lotteryData.ToLotteryString()}";
                
                return (settledCount, summary);
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", 
                    $"结算失败: {ex.Message}", ex);
                return (0, $"结算失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 结算单个订单（🔥 完全参考 F5BotV2 的 OnMemberOrderFinish 逻辑）
        /// </summary>
        private async Task SettleSingleOrderAsync(V2MemberOrder order, BinggoLotteryData lotteryData)
        {
            try
            {
                // 🔥 参考 F5BotV2: 第 599-640 行
                
                // 1. 检查订单状态
                if (order.OrderStatus == OrderStatus.已完成)
                {
                    _logService.Info("BinggoOrderService", $"订单已结算，跳过: {order.Id}");
                    return;
                }
                
                if (order.OrderStatus == OrderStatus.已取消)
                {
                    _logService.Info("BinggoOrderService", $"订单已取消，跳过: {order.Id}");
                    return;
                }
                
                // 2. 解析下注内容（使用 BetContentStandar 字段）
                var betContent = BinggoHelper.ParseBetContent(order.BetContentStandar ?? string.Empty, order.IssueId);
                
                if (betContent.Code != 0)
                {
                    _logService.Warning("BinggoOrderService", 
                        $"订单解析失败，无法结算: {order.BetContentStandar}");
                    order.IsSettled = true;
                    order.Profit = 0; // 解析失败视为输
                    order.NetProfit = -order.AmountTotal;
                    order.OrderStatus = OrderStatus.已完成;
                    return;
                }
                
                // 3. 获取赔率（参考 F5BotV2: _appSetting.wxOdds）
                float odds = order.Odds > 0 ? order.Odds : 1.97f;
                
                // 4. 调用 OpenLottery 计算盈利（参考 F5BotV2: order.OpenLottery(data, odds, zsjs)）
                float totalWin = 0f; // 总赢金额（包含本金）
                foreach (var item in betContent.Items)
                {
                    bool isWin = BinggoHelper.IsWin(item, lotteryData);
                    if (isWin)
                    {
                        // 🔥 参考 F5BotV2: 赢了返回 金额 × 赔率
                        totalWin += (float)item.TotalAmount * odds;
                    }
                }
                
                // 5. 更新订单状态（参考 F5BotV2: V2MemberOrder.OpenLottery 第 172-174 行）
                order.Profit = totalWin;  // 总赢金额（包含本金）
                order.NetProfit = totalWin - order.AmountTotal;  // 纯利 = 总赢 - 投注额
                order.OrderStatus = OrderStatus.已完成;
                order.IsSettled = true;
                
                _logService.Info("BinggoOrderService", 
                    $"📊 订单结算: {order.Wxid} - 期号 {order.IssueId} - 投注 {order.AmountTotal:F2} - 总赢 {order.Profit:F2} - 纯利 {order.NetProfit:F2}");
                
                // 6. 更新会员数据（参考 F5BotV2: m.OpenLottery(order) 第 451-454 行）
                var member = _membersBindingList?.FirstOrDefault(m => m.Wxid == order.Wxid);
                if (member != null && order.OrderType != OrderType.托)  // 🔥 托单不更新会员数据
                {
                    // 🔥 关键逻辑（参考 F5BotV2 V2Member.OpenLottery）：
                    // Balance += order.Profit (加上总赢金额，包含本金)
                    // IncomeToday += (order.Profit - order.AmountTotal)  (今日盈亏 = 纯利)
                    // IncomeTotal += (order.Profit - order.AmountTotal)  (总盈亏 = 纯利)
                    
                    member.Balance += order.Profit;  // 🔥 加上总赢金额
                    member.IncomeToday += order.NetProfit;  // 🔥 今日盈亏（纯利）
                    member.IncomeTotal += order.NetProfit;  // 🔥 总盈亏（纯利）
                    
                    // 🔥 扣除待结算金额（参考 F5BotV2 第 633 行: m.BetWait = m.BetWait - order.AmountTotal）
                    member.BetWait -= order.AmountTotal;
                    
                    _logService.Info("BinggoOrderService", 
                        $"✅ 会员更新: {member.Nickname} - 余额 {member.Balance:F2} - 今日盈亏 {member.IncomeToday:F2} - 待结算 {member.BetWait:F2}");
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", 
                    $"订单结算异常: {ex.Message}", ex);
                throw;
            }
        }
    }
}

