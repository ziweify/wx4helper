using zhaocaimao.Contracts;
using zhaocaimao.Models;
using zhaocaimao.Models.Games.Binggo;
using System;
using System.Collections.Generic;

namespace zhaocaimao.Services.Games.Binggo
{
    /// <summary>
    /// 炳狗订单验证器
    /// 
    /// 功能：
    /// 1. 验证会员余额
    /// 2. 验证下注金额限额（单注 + 当期累计）
    /// 3. 验证下注状态（是否封盘）
    /// 
    /// 🔥 参考 F5BotV2 第2445-2509行：_OrderLimitDic 机制
    /// </summary>
    public class BinggoOrderValidator
    {
        private readonly ILogService _logService;
        private readonly IConfigurationService _configService;
        
        public BinggoOrderValidator(
            ILogService logService, 
            IConfigurationService configService)
        {
            _logService = logService;
            _configService = configService;
        }
        
        /// <summary>
        /// 验证下注是否有效
        /// </summary>
        /// <param name="member">会员信息</param>
        /// <param name="betContent">下注内容</param>
        /// <param name="currentStatus">当前开奖状态</param>
        /// <param name="accumulatedAmounts">当期已累计金额字典（key="{车号}{玩法}", value=累计金额）</param>
        /// <param name="errorMessage">错误信息（验证失败时）</param>
        /// <returns>是否验证通过</returns>
        public bool ValidateBet(
            V2Member member, 
            BinggoBetContent betContent, 
            BinggoLotteryStatus currentStatus,
            Dictionary<string, decimal> accumulatedAmounts,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                // 1. 检查会员状态
                // 🔥 只排除明确不能下注的状态：已删除、已退群
                // 参考 F5BotV2 和用户确认："非会员也可以下注"（不在联系人列表的人）
                // 所有其他状态（非会员、会员、托、管理、普会、蓝会、紫会、黄会）都可以下注
                if (member.State == MemberState.已删除 || 
                    member.State == MemberState.已退群)
                {
                    errorMessage = "您的账户状态异常，无法下注";
                    return false;
                }
                
                // 🔥 2. 封盘检查已移至 BinggoLotteryService.ProcessBetRequestAsync 统一处理
                // 这里不再检查，因为订单服务只负责业务验证（金额、内容等）
                
                // 3. 检查下注内容是否有效
                if (betContent.Code != 0 || betContent.Items.Count == 0)
                {
                    errorMessage = betContent.ErrorMessage ?? "无效的下注内容";
                    return false;
                }
                
                // 4. 验证单注金额 + 当期累计金额
                // 🔥 参考 F5BotV2 第2445-2480行：_OrderLimitDic 机制
                // 🔥 关键改进：先对当前订单内的投注项按 {车号}{玩法} 分组求和
                // 🔥 防止漏洞：用户输入 "11111大20000" 会被解析为 5 个 "1大20000"
                //             如果不分组，每个都单独检查，会绕过限额！
                float minBet = _configService.GetMinBet();
                float maxBet = _configService.GetMaxBet();
                _logService.Info("OrderValidator", $"🔍 开始验证单注金额限制: MinBet={minBet}, MaxBet={maxBet}");
                
                // 🔥 步骤1：对当前订单内的投注项分组求和
                // 🔥 关键修复：必须使用 TotalAmount（Amount × Quantity），而不是 Amount
                // 🔥 例如：5555大20000 会被解析为 1个 BinggoBetItem(5, 大, Amount=20000, Quantity=4, TotalAmount=80000)
                var currentOrderGrouped = new Dictionary<string, decimal>();
                foreach (var item in betContent.Items)
                {
                    string key = $"{item.CarNumber}{item.PlayType}";
                    if (!currentOrderGrouped.ContainsKey(key))
                    {
                        currentOrderGrouped[key] = 0;
                    }
                    currentOrderGrouped[key] += item.TotalAmount;  // 🔥 使用 TotalAmount，而不是 Amount
                }
                
                _logService.Info("OrderValidator", 
                    $"📊 当前订单分组后共 {currentOrderGrouped.Count} 个投注项（原始 {betContent.Items.Count} 个）");
                
                // 🔥 步骤2：对分组后的每个投注项进行限额检查
                foreach (var kvp in currentOrderGrouped)
                {
                    string key = kvp.Key;
                    decimal currentAmount = kvp.Value;
                    
                    _logService.Info("OrderValidator", $"   - 检查投注项: {key}, 本单金额={currentAmount}");
                    
                    // 4.1 检查单注最小金额（F5BotV2 第2450行）
                    if (currentAmount < (decimal)minBet)
                    {
                        // 🔥 F5BotV2 第2452行格式：@{nickname} 进仓失败!{key}不能小于{minBet}
                        // 🔥 数字格式：整数（不带小数点）
                        errorMessage = $"进仓失败!{key}不能小于{(int)minBet}";
                        _logService.Warning("OrderValidator", $"❌ {errorMessage}（实际: {currentAmount}）");
                        return false;
                    }
                    
                    // 🔥 4.2 检查当期累计金额（历史 + 本单）
                    decimal historicalAmount = 0;
                    if (accumulatedAmounts.TryGetValue(key, out var accumulated))
                    {
                        historicalAmount = accumulated;
                    }
                    
                    // 🔥 总累计 = 历史累计 + 本单金额
                    decimal totalAccumulated = historicalAmount + currentAmount;
                    
                    _logService.Info("OrderValidator", 
                        $"   - 历史累计: {historicalAmount}, 本单: {currentAmount}, 总计: {totalAccumulated}, 限额: {maxBet}");
                    
                    // 🔥 检查总累计是否超过限额
                    if (totalAccumulated > (decimal)maxBet)
                    {
                        // 🔥 计算剩余额度
                        decimal remaining = (decimal)maxBet - historicalAmount;
                        
                        // 🔥 F5BotV2 精确格式（参考第2458、2475行）
                        // 🔥 数字格式：整数（不带小数点）
                        // 🔥 第一次投注用"剩:"，后续投注用"剩余:"
                        if (historicalAmount == 0)
                        {
                            // 第一次投注，本单就超限（F5BotV2 第2458行）
                            errorMessage = $"进仓失败!{key}超限,当前{(int)currentAmount},剩:{(int)maxBet}";
                        }
                        else
                        {
                            // 已有历史投注，加上本单超限（F5BotV2 第2475行）
                            errorMessage = $"进仓失败!{key}超限,当前{(int)currentAmount},剩余:{(int)remaining}";
                        }
                        
                        _logService.Warning("OrderValidator", $"❌ {errorMessage}");
                        _logService.Warning("OrderValidator", 
                            $"   详情: MaxBet={maxBet}, 历史累计={historicalAmount}, 本单={currentAmount}, 总计={totalAccumulated}");
                        return false;
                    }
                }
                
                _logService.Info("OrderValidator", "✅ 单注金额验证通过（含当期累计限额检查）");
                
                // 5. 验证余额（自然限制，无需人为限制单期总金额）
                // 🔥 重要：托单也要验证余额！（托单是正常玩家，走正常流程）
                // 只有管理员不验证余额（管理员不扣钱）
                decimal totalAmount = betContent.TotalAmount;
                
                if (member.State != MemberState.管理)
                {
                    if ((decimal)member.Balance < totalAmount)
                    {
                        // 🔥 格式完全按照 F5BotV2 第194行 Reply_余额不足 = "客官你的荷包是否不足!"
                        // 注意：这里只返回错误标识，实际消息格式在 BinggoOrderService 中处理
                        errorMessage = "余额不足";
                        return false;
                    }
                }
                
                // 验证通过
                return true;
            }
            catch (Exception ex)
            {
                _logService.Error("OrderValidator", $"验证异常: {ex.Message}", ex);
                errorMessage = "系统错误，请稍后重试";
                return false;
            }
        }
        
        /// <summary>
        /// 验证补单是否有效
        /// </summary>
        public bool ValidateManualOrder(
            V2Member member,
            int issueId,
            decimal amount,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                // 1. 检查会员状态
                if (member.State == MemberState.已删除)
                {
                    errorMessage = "该会员已被删除，无法补单";
                    return false;
                }
                
                // 2. 验证期号（不能是未来的期号）
                // TODO: 可以根据当前期号验证
                
                // 3. 验证金额
                if (amount <= 0)
                {
                    errorMessage = "补单金额必须大于0";
                    return false;
                }
                
                float maxBet = _configService.GetMaxBet();
                if (amount > (decimal)maxBet * 100) // 补单最大金额限制
                {
                    errorMessage = $"补单金额过大，最多 {maxBet * 100} 元";
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logService.Error("OrderValidator", $"补单验证异常: {ex.Message}", ex);
                errorMessage = "系统错误，请稍后重试";
                return false;
            }
        }
    }
}

