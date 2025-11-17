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
        private BinggoStatisticsService? _statisticsService; // 🔥 统计服务（可选，通过 SetStatisticsService 设置）
        private SQLiteConnection? _db;
        private V2OrderBindingList? _ordersBindingList;
        private V2MemberBindingList? _membersBindingList;
        
        public BinggoOrderService(
            ILogService logService,
            IBinggoLotteryService lotteryService,
            BinggoOrderValidator validator)
        {
            _logService = logService;
            _lotteryService = lotteryService;
            _validator = validator;
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
                    
                    // 🔥 余额不足消息格式完全按照 F5BotV2 第2430行：@{m.nickname} {Reply_余额不足}
                    // Reply_余额不足 = "客官你的荷包是否不足!"（F5BotV2 第194行）
                    if (errorMessage == "余额不足")
                    {
                        return (false, $"@{member.Nickname} 客官你的荷包是否不足!", null);
                    }
                    
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
                    OrderStatus = OrderStatus.待处理,  // 🔥 初始状态为待处理，等待自动投注
                    OrderType = OrderType.待定,  // 🔥 初始类型为待定，投注后才确定盘内/盘外
                    MemberState = member.State,  // 🔥 记录会员等级快照（订单创建时的会员状态）
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
                // 注意：托单不计算在内（已在前面判断）
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
                
                // 🔥 7. 更新全局统计（实时增减，参考 F5BotV2 第 538-573 行：OnMemberOrderCreate）
                // 注意：托单不计算在内（已在前面判断）
                if (_statisticsService != null && order.OrderType != OrderType.托)
                {
                    _statisticsService.OnOrderCreated(order);
                }
                
                // 8. 生成回复消息（🔥 完全参考 F5BotV2 格式）
                // 格式：@昵称\r已进仓{注数}\r{投注内容}|扣:{金额}|留:{余额}
                // 🔥 F5BotV2 第2413行：扣:{member_order.AmountTotal}（不使用 (int) 转换）
                string replyMessage = $"@{member.Nickname}\r已进仓{order.Nums}\r{betContent.ToReplyString()}|扣:{order.AmountTotal}|留:{(int)member.Balance}";
                
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
        /// <param name="sendToWeChat">是否发送到微信（线上补单=true，离线补单=false）</param>
        /// <returns>(成功, 微信消息内容, 订单对象)</returns>
        public async Task<(bool success, string message, V2MemberOrder? order)> CreateManualOrderAsync(
            V2Member member,
            int issueId,
            string betContent,
            decimal amount,
            bool sendToWeChat = true)
        {
            try
            {
                string type = sendToWeChat ? "线上补单" : "离线补单";
                _logService.Info("BinggoOrderService", 
                    $"{type}: {member.Nickname} ({member.Wxid}) - 期号: {issueId}");
                
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
                    MemberState = member.State,  // 🔥 记录会员等级快照
                    Profit = 0,  // 稍后结算
                    IsSettled = false,
                    TimeStampBet = DateTimeOffset.Now.ToUnixTimeSeconds(),  // 🔥 设置下注时间戳
                    CreatedAt = DateTime.Now
                };
                
                // 4. 立即结算（与正常订单一样走结算流程）
                await SettleSingleOrderAsync(order, lotteryData);
                
                // 5. 更新会员余额（盈亏）
                member.Balance += order.NetProfit;  // 🔥 补单也要更新余额
                member.IncomeTotal += order.NetProfit;
                if (order.CreatedAt.Date == DateTime.Now.Date)
                {
                    member.IncomeToday += order.NetProfit;
                }
                
                // 6. 保存订单（插入到列表顶部，保持"最新在上"）
                if (_ordersBindingList != null && _ordersBindingList.Count > 0)
                {
                    _ordersBindingList.Insert(0, order);  // 🔥 插入到顶部
                }
                else
                {
                    _ordersBindingList?.Add(order);  // 🔥 空列表时使用 Add
                }
                
                // 🔥 7. 更新全局统计（完全参考 F5BotV2）
                if (_statisticsService != null && order.OrderType != OrderType.托)
                {
                    _statisticsService.OnOrderCreated(order);  // 增加总注、今投、当前
                    _statisticsService.OnOrderSettled(order);  // 增加总盈、今盈
                }
                
                // 🔥 8. 生成补单微信消息（完全参考 F5BotV2 第 1261-1268 行）
                // 格式：
                //   ----补分名单----
                //   {nickname}|{期号后3位}|{开奖号码}|{投注内容}|{押注金额}
                //   ------补完留分------
                //   {nickname} | {余额}
                int issueShort = issueId % 1000;
                string lotteryStr = lotteryData.ToLotteryString();  // "7,14,21,8,2 大单 龙"
                string weChatMessage = $"----补分名单----\r" +
                    $"{member.Nickname}|{issueShort}|{lotteryStr}|{betContent}|{order.AmountTotal - order.NetProfit}\r" +
                    $"------补完留分------\r" +
                    $"{member.Nickname} | {(int)member.Balance}";
                
                _logService.Info("BinggoOrderService", 
                    $"✅ {type}成功: {member.Nickname} - {betContent} - {amount:F2}元 - 盈利: {order.NetProfit:F2} - 余额: {member.Balance:F2}");
                
                // 🔥 返回微信消息（如果是线上补单，调用者需要发送到微信；离线补单则只做记录）
                return (true, weChatMessage, order);
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
                
                // 2. 查询当期所有订单（参考 F5BotV2: 排除已取消和未知状态）
                var unsetledOrders = _ordersBindingList?
                    .Where(o => o.IssueId == issueId 
                        && o.OrderStatus != OrderStatus.已取消 
                        && o.OrderStatus != OrderStatus.未知
                        && !o.IsSettled)
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
                
                // 🔥 5. 返回结算后的订单列表（用于生成中奖名单）
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
        public async Task SettleSingleOrderAsync(V2MemberOrder order, BinggoLotteryData lotteryData)
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
                
                // 🔥 6. 显式更新订单到数据库（确保状态保存）
                // 虽然 PropertyChanged 会自动保存，但为了确保可靠性，这里显式调用 UpdateOrder
                UpdateOrder(order);
                
                // 7. 更新会员数据（参考 F5BotV2: m.OpenLottery(order) 第 451-454 行）
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
                    
                    // 🔥 更新全局盈利统计（参考 F5BotV2 第 626-635 行：OnMemberOrderFinish）
                    // 注意：只更新盈利，不更新投注金额（投注金额在下单时已更新）
                    if (_statisticsService != null)
                    {
                        _statisticsService.OnOrderSettled(order);
                    }
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
        
        /// <summary>
        /// 获取指定期号的待投注订单（用于自动投注）
        /// </summary>
        public IEnumerable<V2MemberOrder> GetPendingOrdersForIssue(int issueId)
        {
            if (_db == null) return Enumerable.Empty<V2MemberOrder>();
            
            try
            {
                var orders = _db.Table<V2MemberOrder>()
                    .Where(o => o.IssueId == issueId && o.OrderStatus == OrderStatus.待处理)
                    .ToList();
                
                _logService.Info("BinggoOrderService", $"📋 查询待投注订单:期号{issueId} 找到{orders.Count}个");
                
                return orders;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", 
                    $"查询待投注订单失败: {ex.Message}", ex);
                return Enumerable.Empty<V2MemberOrder>();
            }
        }
        
        /// <summary>
        /// 获取指定会员、指定期号的待处理订单（用于取消命令）
        /// </summary>
        public IEnumerable<V2MemberOrder> GetPendingOrdersForMemberAndIssue(string wxid, int issueId)
        {
            if (_db == null) return Enumerable.Empty<V2MemberOrder>();
            
            try
            {
                var orders = _db.Table<V2MemberOrder>()
                    .Where(o => o.Wxid == wxid && o.IssueId == issueId && o.OrderStatus == OrderStatus.待处理)
                    .ToList();
                
                _logService.Info("BinggoOrderService", $"📋 查询待处理订单:会员{wxid} 期号{issueId} 找到{orders.Count}个");
                
                return orders;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", $"查询待处理订单失败:会员{wxid} 期号{issueId}", ex);
                return Enumerable.Empty<V2MemberOrder>();
            }
        }
        
        /// <summary>
        /// 更新订单（用于投注后更新状态）
        /// </summary>
        public void UpdateOrder(V2MemberOrder order)
        {
            if (_db == null || order == null) return;
            
            try
            {
                _db.Update(order);
                
                // 同步更新 BindingList（如果设置了）
                if (_ordersBindingList != null)
                {
                    var existing = _ordersBindingList.FirstOrDefault(o => o.Id == order.Id);
                    if (existing != null)
                    {
                        // 🔥 更新所有属性（包括 OrderStatus、OrderType、Profit、NetProfit、IsSettled）
                        existing.OrderStatus = order.OrderStatus;
                        existing.OrderType = order.OrderType;
                        existing.Profit = order.Profit;
                        existing.NetProfit = order.NetProfit;
                        existing.IsSettled = order.IsSettled;
                    }
                }
                
                _logService.Info("BinggoOrderService", 
                    $"✅ 订单已更新:ID={order.Id} 状态={order.OrderStatus} 类型={order.OrderType} 总赢={order.Profit:F2} 纯利={order.NetProfit:F2}");
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", $"更新订单失败:ID={order.Id}", ex);
            }
        }
    }
}

