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
                
                // 5. 保存订单（通过 BindingList 自动保存）
                _ordersBindingList?.Add(order);
                
                _logService.Info("BinggoOrderService", 
                    $"✅ 订单创建成功: {member.Nickname} - {betContent.ToStandardString()} - {betContent.TotalAmount:F2}元");
                
                // 6. 生成回复消息
                string replyMessage = $"{_settings.ReplySuccess}\n" +
                                    $"期号: {issueId}\n" +
                                    $"内容: {betContent.ToReplyString()}\n" +
                                    $"金额: {betContent.TotalAmount:F2}\n" +
                                    $"余额: {member.Balance:F2}";
                
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
                
                // 5. 保存订单
                _ordersBindingList?.Add(order);
                
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
        /// 结算单个订单
        /// </summary>
        private async Task SettleSingleOrderAsync(V2MemberOrder order, BinggoLotteryData lotteryData)
        {
            try
            {
                // 1. 解析下注内容
                var betContent = BinggoHelper.ParseBetContent(order.BetContent ?? string.Empty, order.IssueId);
                
                if (betContent.Code != 0)
                {
                    _logService.Warning("BinggoOrderService", 
                        $"订单解析失败，无法结算: {order.BetContent}");
                    order.IsSettled = true;
                    order.Profit = -(float)order.BetAmount; // 视为输
                    return;
                }
                
                // 2. 获取赔率（简化：统一赔率）
                decimal odds = 1.95m;
                if (_settings.Odds.ContainsKey("大"))
                {
                    odds = (decimal)_settings.Odds["大"];
                }
                
                // 3. 计算盈利
                decimal profit = BinggoHelper.CalculateTotalProfit(betContent, lotteryData, odds, isIntegerSettle: false);
                
                // 4. 更新订单
                order.Profit = (float)profit;
                order.IsSettled = true;
                
                // 5. 更新会员余额
                var member = _membersBindingList?.FirstOrDefault(m => m.Wxid == order.Wxid);
                if (member != null)
                {
                    // 退还本金 + 盈利
                    float betAmountFloat = (float)order.BetAmount;
                    float profitFloat = (float)profit;
                    member.Balance += betAmountFloat + profitFloat;
                    
                    _logService.Info("BinggoOrderService", 
                        $"结算订单: {order.Nickname} - 盈利: {profit:F2}，余额: {member.Balance:F2}");
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", 
                    $"结算单个订单失败: {ex.Message}", ex);
                order.IsSettled = true;
                order.Profit = -(float)order.BetAmount; // 异常视为输
            }
        }
    }
}

