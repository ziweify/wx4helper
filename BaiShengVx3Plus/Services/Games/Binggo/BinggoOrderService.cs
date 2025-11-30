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
        private readonly IConfigurationService _configService; // 🔥 配置服务（用于获取赔率）
        private BinggoStatisticsService? _statisticsService; // 🔥 统计服务（可选，通过 SetStatisticsService 设置）
        private SQLiteConnection? _db;
        private V2OrderBindingList? _ordersBindingList;
        private V2MemberBindingList? _membersBindingList;
        
        // 🔥 应用级别的锁：保护会员余额、订单、资金相关表的同步写入
        // 参考用户要求："所有会员表，订单表的操作，要变成同步操作。而且是应用级别的同步"
        //
        // 🔥 重要变更：使用全局锁管理类（Core.ResourceLocks）
        // 原因：不同类中的 static readonly object 是独立的对象，无法互相保护
        // 例如：BinggoOrderService._memberBalanceLock != CreditWithdrawService._memberBalanceLock
        // 解决：所有服务使用 ResourceLocks.MemberBalanceLock 和 ResourceLocks.OrderLimitCheckLock
        // 
        // 🔥 不再定义本地锁对象，直接使用 Core.ResourceLocks.MemberBalanceLock
        // 🔥 不再定义本地锁对象，直接使用 Core.ResourceLocks.OrderLimitCheckLock
        
        public BinggoOrderService(
            ILogService logService,
            IBinggoLotteryService lotteryService,
            BinggoOrderValidator validator,
            IConfigurationService configService) // 🔥 注入配置服务
        {
            _logService = logService;
            _lotteryService = lotteryService;
            _validator = validator;
            _configService = configService;
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
                
                // 🔥 关键修复：在保存订单前，获取实时状态和期号（模仿 F5BotV2 第2393行设计）
                // F5BotV2 在保存订单前会再次检查 _status，确保状态未变化
                // 这是 F5BotV2 稳定性的关键！
                int realTimeIssueId = _lotteryService.CurrentIssueId;
                BinggoLotteryStatus realTimeStatus = _lotteryService.CurrentStatus;
                
                // 🔥 检查期号是否一致（防止期号在处理过程中变化）
                if (realTimeIssueId != issueId)
                {
                    _logService.Warning("BinggoOrderService", 
                        $"❌ 期号已变化，拒绝下注: {member.Nickname} - 传入期号: {issueId} - 当前期号: {realTimeIssueId}");
                    return (false, $"{member.Nickname}\r时间未到!不收货!", null);
                }
                
                // 🔥 再次检查状态（防止状态在处理过程中变化）- 参考 F5BotV2 第2393行
                // F5BotV2: if(_status == BoterStatus.开盘中) { /* 保存订单 */ } else { /* 拒绝 */ }
                // 🔥 重要：使用白名单模式（只允许明确的状态），而不是黑名单模式
                // 这样即使将来新增状态，也会默认拒绝，更安全（防御性编程）
                if (realTimeStatus != BinggoLotteryStatus.开盘中 && 
                    realTimeStatus != BinggoLotteryStatus.即将封盘)
                {
                    _logService.Warning("BinggoOrderService", 
                        $"❌ 状态已变化，拒绝下注: {member.Nickname} - 期号: {realTimeIssueId} - 状态: {realTimeStatus}");
                    return (false, $"{member.Nickname}\r时间未到!不收货!", null);
                }
                
                _logService.Info("BinggoOrderService", 
                    $"✅ 状态和期号验证通过: 期号={realTimeIssueId}, 状态={realTimeStatus}");
                
                // 1. 解析下注内容
                var betContent = BinggoHelper.ParseBetContent(messageContent, issueId);
                
                if (betContent.Code != 0)
                {
                    _logService.Warning("BinggoOrderService", 
                        $"解析下注失败: {betContent.ErrorMessage}");
                    return (false, betContent.ErrorMessage, null);
                }
                
                // 2. 🔥 验证下注 + 创建订单（使用锁保护，防止并发竞态）
                // 🎯 并发场景：两用户同时投注同一项（如 1大）
                //   - 不加锁：都查到累计未超限 → 都通过验证 → 都保存成功 → 总额超限！
                //   - 加锁后：线程A验证+保存 → 线程B验证时查到A的订单 → 超限被拒绝 ✅
                V2MemberOrder? order = null;
                lock (Core.ResourceLocks.OrderLimitCheckLock)
                {
                    // 2.1 查询当期所有投注项的累计金额（🔥 在锁内查询，确保是最新数据）
                    var accumulatedAmounts = new Dictionary<string, decimal>();
                    foreach (var item in betContent.Items)
                    {
                        string key = $"{item.CarNumber}{item.PlayType}";
                        if (!accumulatedAmounts.ContainsKey(key))
                        {
                            accumulatedAmounts[key] = GetIssueBetAmountByItem(issueId, item.CarNumber, item.PlayType.ToString());
                        }
                    }
                    
                    // 2.2 验证下注（传入累计金额字典）
                    if (!_validator.ValidateBet(member, betContent, currentStatus, accumulatedAmounts, out string errorMessage))
                    {
                        _logService.Warning("BinggoOrderService", 
                            $"验证下注失败: {errorMessage}");
                        
                        // 🔥 余额不足消息格式完全按照 F5BotV2 第2430行：@{m.nickname} {Reply_余额不足}
                        // Reply_余额不足 = "客官你的荷包是否不足!"（F5BotV2 第194行）
                        if (errorMessage == "余额不足")
                        {
                            return (false, $"@{member.Nickname} 客官你的荷包是否不足!", null);
                        }
                        
                        // 🔥 限额超限消息（参考 F5BotV2 第2458、2475行）
                        return (false, $"@{member.Nickname} {errorMessage}", null);
                    }
                    
                    // 2.3 验证通过，立即创建订单对象（在锁内，确保原子性）
                    long timestampBet = DateTimeOffset.Now.ToUnixTimeSeconds();
                    
                    // 🔥 记录注前金额和注后金额
                    float betFronMoney = member.Balance;  // 下注前余额
                    float betAfterMoney = member.Balance - (float)betContent.TotalAmount;  // 下注后余额（暂存）
                    
                    order = new V2MemberOrder
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
                    // 🔥 从配置服务获取赔率（VxMain 中设置的统一赔率）
                    // 由于 SetAllOdds 设置的是统一赔率，所有玩法都是同一个值，所以获取"大"的赔率即可
                    Odds = GetOddsFromConfig(betContent),
                    OrderStatus = OrderStatus.待处理,  // 🔥 初始状态为待处理，等待自动投注
                    // 🔥 订单类型根据会员等级初始化（参考 F5BotV2）
                    // 托单：不投注到平台，但正常扣钱、正常结算
                    OrderType = member.State == MemberState.托 ? OrderType.托 : OrderType.待定,
                    MemberState = member.State,  // 🔥 记录会员等级快照（订单创建时的会员状态）
                    IsSettled = false,
                    
                    // 🔥 开奖服务专用字段（保留兼容）
                    BetContent = betContent.ToStandardString(),
                    BetAmount = betContent.TotalAmount
                };
                
                // 🔥 2.4 使用应用级别的锁保护会员余额和订单的同步更新
                // 参考用户要求："锁要注意时机，不能锁定太长时间，只锁定写入数据库数据这里"
                // 参考 F5BotV2: V2Member.AddOrder 方法（第430-439行）
                lock (Core.ResourceLocks.MemberBalanceLock)
                {
                    // 🔥 关键修复1：先检查 BindingList 是否可用（在扣除余额之前）
                    // 防止：余额扣除成功，但订单保存失败（因为 BindingList 为 null）
                    if (_ordersBindingList == null)
                    {
                        string errorCode = Constants.ErrorCodes.Order.OrderListNotInitialized;
                        _logService.Error("BinggoOrderService", 
                            $"❌ [{errorCode}] 严重错误：订单列表未初始化！" +
                            $"会员: {member.Nickname}({member.Wxid}), " +
                            $"期号: {issueId}, " +
                            $"金额: {betContent.TotalAmount:F2}");
                        return (false, Constants.ErrorCodes.FormatUserMessage(errorCode), null);
                    }
                    
                    if (_membersBindingList == null)
                    {
                        string errorCode = Constants.ErrorCodes.Order.MemberListNotInitialized;
                        _logService.Error("BinggoOrderService", 
                            $"❌ [{errorCode}] 严重错误：会员列表未初始化！" +
                            $"会员: {member.Nickname}({member.Wxid}), " +
                            $"期号: {issueId}, " +
                            $"金额: {betContent.TotalAmount:F2}");
                        return (false, Constants.ErrorCodes.FormatUserMessage(errorCode), null);
                    }
                    
                    // 🔥 关键修复2：在保存订单前的最后时刻，再次检查实时状态（模仿 F5BotV2 第2393行）
                    // F5BotV2 的关键设计：在锁内、保存订单前，最后一次检查状态
                    // 这是防止状态变化的最后一道防线！
                    var finalStatus = _lotteryService.CurrentStatus;
                    var finalIssueId = _lotteryService.CurrentIssueId;
                    
                    if (finalStatus != BinggoLotteryStatus.开盘中 && 
                        finalStatus != BinggoLotteryStatus.即将封盘)
                    {
                        _logService.Warning("BinggoOrderService", 
                            $"❌ [锁内检查] 状态已变化，拒绝下单: {member.Nickname} - 期号: {finalIssueId} - 状态: {finalStatus}");
                        return (false, $"{member.Nickname}\r时间未到!不收货!", null);
                    }
                    
                    if (finalIssueId != issueId)
                    {
                        _logService.Warning("BinggoOrderService", 
                            $"❌ [锁内检查] 期号已变化，拒绝下单: {member.Nickname} - 原期号: {issueId} - 当前期号: {finalIssueId}");
                        return (false, $"{member.Nickname}\r时间未到!不收货!", null);
                    }
                    
                    _logService.Info("BinggoOrderService", 
                        $"✅ [锁内检查] 最终状态和期号验证通过: 期号={finalIssueId}, 状态={finalStatus}");
                    
                    // 🔥 关键修复3：记录原始值（用于异常回滚）
                    float balanceBefore = member.Balance;
                    float betWaitBefore = member.BetWait;
                    float betTodayBefore = member.BetToday;
                    float betTotalBefore = member.BetTotal;
                    float betCurBefore = member.BetCur;
                    
                    try
                    {
                        // 2.4.1 扣除余额
                        // 🔥 重要：托单也要扣钱！（用户要求："托照样正常结算，什么都是走正常流程的，仅仅不投注而已"）
                        // 只有管理员不扣钱
                        if (member.State != MemberState.管理)
                        {
                            member.Balance -= (float)betContent.TotalAmount;
                            
                            _logService.Info("BinggoOrderService", 
                                $"🔒 [下单] {member.Nickname} - 扣除 {betContent.TotalAmount:F2}，余额: {balanceBefore:F2} → {member.Balance:F2}");
                        }
                        
                        // 2.4.2 增加待结算金额和统计（参考 F5BotV2 第 546 行）
                        // 🔥 重要：托单也要增加统计！（会员个人统计）
                        member.BetWait += (float)betContent.TotalAmount;
                        member.BetToday += (float)betContent.TotalAmount;
                        member.BetTotal += (float)betContent.TotalAmount;
                        member.BetCur += (float)betContent.TotalAmount;  // 本期下注
                        
                        _logService.Info("BinggoOrderService", 
                            $"🔒 [下单] {member.Nickname} - 待结算: {member.BetWait:F2}, 今日下注: {member.BetToday:F2}");
                        
                        // 2.4.3 保存订单（插入到列表顶部，保持"最新在上"）
                        // 🔥 已在前面检查过 _ordersBindingList != null，所以这里不需要 ?. 了
                        if (_ordersBindingList.Count > 0)
                        {
                            _ordersBindingList.Insert(0, order);  // 🔥 插入到顶部
                        }
                        else
                        {
                            _ordersBindingList.Add(order);  // 🔥 空列表时使用 Add
                        }
                        
                        _logService.Info("BinggoOrderService", 
                            $"✅ [下单] {member.Nickname} - 订单已保存: OrderId={order.Id}, 金额={betContent.TotalAmount:F2}");
                    }
                    catch (Exception ex)
                    {
                        // 🔥 关键修复4：异常时回滚所有修改
                        member.Balance = balanceBefore;
                        member.BetWait = betWaitBefore;
                        member.BetToday = betTodayBefore;
                        member.BetTotal = betTotalBefore;
                        member.BetCur = betCurBefore;
                        
                        string errorCode = Constants.ErrorCodes.Order.OrderSaveFailed;
                        _logService.Error("BinggoOrderService", 
                            $"❌ [{errorCode}] 订单保存失败，已回滚余额和统计！\n" +
                            $"  会员: {member.Nickname}({member.Wxid})\n" +
                            $"  期号: {issueId}\n" +
                            $"  金额: {betContent.TotalAmount:F2}\n" +
                            $"  余额: {balanceBefore:F2} (已回滚)\n" +
                            $"  异常: {ex.GetType().Name}\n" +
                            $"  消息: {ex.Message}\n" +
                            $"  堆栈: {ex.StackTrace}", ex);
                        
                        return (false, Constants.ErrorCodes.FormatUserMessage(errorCode), null);
                    }
                }
                // 🔥 锁释放：会员余额、订单数据已同步写入
                
                } // 🔥 关闭 _orderLimitCheckLock 锁
                // 锁释放：限额验证-订单创建的原子操作完成
                
                _logService.Info("BinggoOrderService", 
                    $"✅ 订单创建成功: {member.Nickname} - {betContent.ToStandardString()} - {betContent.TotalAmount:F2}元");
                
                // 🔥 7. 更新全局统计（实时增减，参考 F5BotV2 第 538-573 行：OnMemberOrderCreate）
                // 注意：托单不计入统计（参考 F5BotV2 Line 548）
                if (_statisticsService != null && order.OrderType != OrderType.托 && order.OrderStatus != OrderStatus.已取消)
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
        /// 补单（在原订单上操作，参考 F5BotV2 第 599-673 行）
        /// </summary>
        /// <param name="order">原订单对象</param>
        /// <param name="member">会员对象</param>
        /// <param name="sendToWeChat">是否发送到微信（线上补单=true，离线补单=false）</param>
        /// <returns>(成功, 微信消息内容, 订单对象)</returns>
        public async Task<(bool success, string message, V2MemberOrder? order)> SettleManualOrderAsync(
            V2MemberOrder order,
            V2Member member,
            bool sendToWeChat = true)
        {
            string type = sendToWeChat ? "线上补单" : "离线补单";
            
            try
            {
                _logService.Info("BinggoOrderService", 
                    $"{type}: {member.Nickname} ({member.Wxid}) - 订单ID: {order.Id} - 期号: {order.IssueId}");
                
                // 🔥 1. 检查订单状态（参考 F5BotV2 第 599-640 行）
                if (order.OrderStatus == OrderStatus.已完成)
                {
                    return (false, "已完成的订单无法补单", null);
                }
                
                if (order.OrderStatus == OrderStatus.已取消)
                {
                    return (false, "已取消的订单无法补单", null);
                }
                
                // 🔥 2. 获取开奖数据（优先从本地缓存）
                var lotteryData = await _lotteryService.GetLotteryDataAsync(order.IssueId, forceRefresh: false);
                
                if (lotteryData == null || !lotteryData.IsOpened)
                {
                    return (false, $"期号 {order.IssueId} 未开奖，请先在开奖页面手动录入开奖数据！", null);
                }
                
                // 🔥 3. 在原订单上结算（参考 F5BotV2 第 622-624 行）
                await SettleSingleOrderAsync(order, lotteryData);
                
                // 🔥 4. 添加备注（参考 F5BotV2：记录补单信息）
                string notePrefix = string.IsNullOrEmpty(order.Notes) ? "" : $"{order.Notes}\r";
                string noteSuffix = $"{type} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                order.Notes = $"{notePrefix}{noteSuffix}";
                
                // 🔥 5. 更新订单到数据库（备注已更新）
                // 🔥 使用全局锁：虽然这里只更新订单，但保持一致性
                lock (Core.ResourceLocks.MemberBalanceLock)
                {
                    UpdateOrder(order);
                }
                
                // 🔥 6. 生成补单微信消息（完全参考 F5BotV2 第 1402 行和 284 行）
                // 格式：
                //   ----补分名单----
                //   {nickname}|{期号后3位}|{开奖号码}|{投注内容}|{返奖金额}
                //   ------补完留分------
                //   {nickname} | {余额}
                // 注意：F5BotV2 中显示的是 Profit（返奖金额，总赢金额包含本金），不是纯利
                int issueShort = order.IssueId % 1000;
                string lotteryStr = lotteryData.ToLotteryString();  // "7,14,21,8,2 大单 龙"
                string betContentForMessage = order.BetContentOriginal ?? order.BetContentStandar ?? order.BetContent ?? "";
                string weChatMessage = $"----补分名单----\r" +
                    $"{member.Nickname}|{issueShort}|{lotteryStr}|{betContentForMessage}|{order.Profit}\r" +
                    $"------补完留分------\r" +
                    $"{member.Nickname} | {(int)member.Balance}";
                
                _logService.Info("BinggoOrderService", 
                    $"✅ {type}成功: {member.Nickname} - 订单ID: {order.Id} - 盈利: {order.NetProfit:F2} - 余额: {member.Balance:F2}");
                
                // 🔥 返回微信消息（如果是线上补单，调用者需要发送到微信；离线补单则只做记录）
                return (true, weChatMessage, order);
            }
            catch (Exception ex)
            {
                string errorCode = Constants.ErrorCodes.Order.ManualOrderFailed;
                _logService.Error("BinggoOrderService", 
                    $"❌ [{errorCode}] 补单失败！\n" +
                    $"  订单ID: {order.Id}\n" +
                    $"  期号: {order.IssueId}\n" +
                    $"  会员: {member.Nickname}({member.Wxid})\n" +
                    $"  金额: {order.AmountTotal:F2}\n" +
                    $"  类型: {type}\n" +
                    $"  异常: {ex.GetType().Name}\n" +
                    $"  消息: {ex.Message}", ex);
                return (false, Constants.ErrorCodes.FormatUserMessage(errorCode), null);
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
                
                // 🔥 6. 使用应用级别的锁保护会员余额和订单的同步更新
                // 参考用户要求："锁要注意时机，不能锁定太长时间，只锁定写入数据库数据这里"
                // 参考 F5BotV2: V2Member.OpenLottery 方法（第446-457行）
                // 🔥 使用全局锁：确保与下注、上下分等操作互斥
                lock (Core.ResourceLocks.MemberBalanceLock)
                {
                    // 6.1 显式更新订单到数据库（确保状态保存）
                    // 虽然 PropertyChanged 会自动保存，但为了确保可靠性，这里显式调用 UpdateOrder
                    UpdateOrder(order);
                    
                    _logService.Info("BinggoOrderService", 
                        $"🔒 [开奖] 订单已更新: OrderId={order.Id}, Profit={order.Profit:F2}, NetProfit={order.NetProfit:F2}");
                    
                    // 6.2 更新会员数据（参考 F5BotV2: m.OpenLottery(order) 第 451-454 行）
                    var member = _membersBindingList?.FirstOrDefault(m => m.Wxid == order.Wxid);
                    if (member != null)  // 🔥 托单也正常结算（更新余额）
                    {
                        _logService.Info("BinggoOrderService", 
                            $"🔒 [开奖] {member.Nickname} - 结算前余额: {member.Balance:F2}, 待结算: {member.BetWait:F2}");
                        
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
                            $"🔒 [开奖] {member.Nickname} - 结算后余额: {member.Balance:F2}, 待结算: {member.BetWait:F2}, 今日盈亏: {member.IncomeToday:F2}");
                        
                        // 🔥 更新全局盈利统计（参考 F5BotV2 第 626-635 行：OnMemberOrderFinish）
                        // 注意：托单不计入统计（参考 F5BotV2 Line 626）
                        if (_statisticsService != null && order.OrderType != OrderType.托)
                        {
                            _statisticsService.OnOrderSettled(order);
                        }
                    }
                }
                // 🔥 锁释放：订单状态、会员余额已同步更新
                
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
        /// 🔥 关键：
        /// 1. 从 BindingList（内存表）查询，而不是数据库
        /// 2. 排除托单（OrderType.托）
        /// 参考 F5BotV2：托单不应该投注到平台，只用于显示和统计
        /// </summary>
        public IEnumerable<V2MemberOrder> GetPendingOrdersForIssue(int issueId)
        {
            if (_ordersBindingList == null) return Enumerable.Empty<V2MemberOrder>();
            
            try
            {
                // 🔥 从 BindingList（内存表）查询，而不是数据库
                // 用户要求："订单只能从内存表中拿，改数据都改内存表，内存表修改即保存"
                var allOrders = _ordersBindingList
                    .Where(o => o.IssueId == issueId && o.OrderStatus == OrderStatus.待处理)
                    .ToList();
                
                // 🔥 排除托单（参考 F5BotV2）
                var validOrders = allOrders
                    .Where(o => o.OrderType != OrderType.托)
                    .ToList();
                
                int tuoOrders = allOrders.Count - validOrders.Count;
                
                _logService.Info("BinggoOrderService", 
                    $"📋 查询待投注订单（从内存表）:期号{issueId} 总计{allOrders.Count}个，有效{validOrders.Count}个，托单{tuoOrders}个（已排除）");
                
                return validOrders;
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
        /// 🔥 从 BindingList（内存表）查询，而不是数据库
        /// </summary>
        public IEnumerable<V2MemberOrder> GetPendingOrdersForMemberAndIssue(string wxid, int issueId)
        {
            if (_ordersBindingList == null) return Enumerable.Empty<V2MemberOrder>();
            
            try
            {
                // 🔥 从 BindingList（内存表）查询
                var orders = _ordersBindingList
                    .Where(o => o.Wxid == wxid && o.IssueId == issueId && o.OrderStatus == OrderStatus.待处理)
                    .ToList();
                
                _logService.Info("BinggoOrderService", $"📋 查询待处理订单（从内存表）:会员{wxid} 期号{issueId} 找到{orders.Count}个");
                
                return orders;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", $"查询待处理订单失败:会员{wxid} 期号{issueId}", ex);
                return Enumerable.Empty<V2MemberOrder>();
            }
        }
        
        /// <summary>
        /// 🔥 获取当期指定投注项的累计金额（用于限额验证）
        /// 参考 F5BotV2 第2447-2480行的 _OrderLimitDic 机制
        /// 
        /// 🎯 重要：
        /// 1. 只统计当期订单（期号变更后自动重置）
        /// 2. 排除已取消的订单（F5BotV2 第2517-2544行：取消时恢复额度）
        /// 3. 实时查询，无需手动清空（F5BotV2 第1258行：封盘后 Clear）
        /// </summary>
        public decimal GetIssueBetAmountByItem(int issueId, int carNumber, string playType)
        {
            if (_ordersBindingList == null) return 0;
            
            try
            {
                // 🔥 从 BindingList（内存表）查询当期所有订单
                // 🔥 排除已取消的订单（参考 F5BotV2 第2517-2544行：OnOrderMoneyLimitCacel 恢复额度）
                var orders = _ordersBindingList
                    .Where(o => o.IssueId == issueId && o.OrderStatus != OrderStatus.已取消)
                    .ToList();
                
                if (!orders.Any())
                    return 0;
                
                decimal total = 0;
                
                // 遍历所有订单，累计指定投注项的金额
                foreach (var order in orders)
                {
                    if (string.IsNullOrEmpty(order.BetContent))
                        continue;
                    
                    // 解析投注内容（格式:"1大10,2小20,3单30"）
                    var betContent = BaiShengVx3Plus.Shared.Parsers.BetContentParser.ParseBetContent(order.BetContent, issueId);
                    
                    if (betContent == null || betContent.Count == 0)
                        continue;
                    
                    // 查找匹配的投注项（betContent 本身就是 List<BetStandardOrder>）
                    foreach (var item in betContent)
                    {
                        // 比较车号（枚举转int）和玩法（枚举转字符串）
                        if ((int)item.Car == carNumber && item.Play.ToString() == playType)
                        {
                            total += item.MoneySum;
                        }
                    }
                }
                
                return total;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", 
                    $"获取当期投注项累计金额失败: 期号{issueId} 车{carNumber}{playType}", ex);
                return 0;
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
        
        /// <summary>
        /// 从配置服务获取微信订单统一赔率（用于订单结算）
        /// 🔥 这是独立于网站投注赔率的配置，专门用于微信下单时的订单结算计算
        /// </summary>
        private float GetOddsFromConfig(BinggoBetContent betContent)
        {
            try
            {
                // 🔥 获取微信订单统一赔率（所有玩法都使用同一个值）
                var odds = _configService.GetWechatOrderOdds();
                if (odds > 0)
                {
                    return odds;
                }
                
                // 🔥 如果配置中没有或为0，使用默认值
                _logService.Debug("BinggoOrderService", "微信订单赔率配置未设置或为0，使用默认值 1.97");
                return 1.97f;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoOrderService", "获取微信订单赔率配置失败，使用默认值 1.97", ex);
                return 1.97f;
            }
        }
    }
}

