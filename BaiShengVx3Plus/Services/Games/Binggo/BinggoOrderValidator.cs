using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Models.Games.Binggo;
using System;
using System.Collections.Generic;

namespace BaiShengVx3Plus.Services.Games.Binggo
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
                if (member.State == MemberState.已删除 || member.State == MemberState.已退群)
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
                // 既限制单注，也限制当期累计总额
                float minBet = _configService.GetMinBet();
                float maxBet = _configService.GetMaxBet();
                _logService.Info("BinggoOrderValidator", $"🔍 开始验证单注金额限制: MinBet={minBet}, MaxBet={maxBet}");
                
                foreach (var item in betContent.Items)
                {
                    // 🔥 F5BotV2 第2446行：key = $"{betitem.car}{betitem.play}"
                    string key = $"{item.CarNumber}{item.PlayType}";
                    
                    _logService.Info("BinggoOrderValidator", $"   - 检查投注项: {key}, 金额={item.Amount}");
                    
                    // 4.1 检查单注最小金额（F5BotV2 第2450行）
                    if (item.Amount < (decimal)minBet)
                    {
                        // 🔥 F5BotV2 第2452行格式：@{nickname} 进仓失败!{key}不能小于{minBet}
                        errorMessage = $"进仓失败!{key}不能小于{minBet}";
                        _logService.Warning("BinggoOrderValidator", $"❌ {errorMessage}（实际: {item.Amount}）");
                        return false;
                    }
                    
                    // 🔥 4.2 检查当期累计金额（F5BotV2 第2447-2480行）
                    // 🔥 从传入的字典中获取累计金额（避免循环依赖）
                    decimal accumulatedAmount = 0;
                    if (accumulatedAmounts.TryGetValue(key, out var accumulated))
                    {
                        accumulatedAmount = accumulated;
                    }
                    
                    _logService.Info("BinggoOrderValidator", 
                        $"   - 当期已累计: {accumulatedAmount}, MaxBet: {maxBet}");
                    
                    if (accumulatedAmount == 0)
                    {
                        // 🔥 第一次投注：检查单注是否超过最大金额（F5BotV2 第2456-2460行）
                        if (item.Amount > (decimal)maxBet)
                        {
                            // 🔥 F5BotV2 第2458行格式：@{nickname} 进仓失败!{key}超限,当前{amount},剩:{maxBet}
                            errorMessage = $"进仓失败!{key}超限,当前{item.Amount},剩:{maxBet}";
                            _logService.Warning("BinggoOrderValidator", $"❌ {errorMessage}");
                            return false;
                        }
                    }
                    else
                    {
                        // 🔥 第二次及以后投注：检查是否超过剩余额度（F5BotV2 第2472-2477行）
                        decimal maxLimit = (decimal)maxBet - accumulatedAmount;
                        
                        _logService.Info("BinggoOrderValidator", 
                            $"   - 剩余额度: {maxLimit}, 当前投注: {item.Amount}");
                        
                        if (item.Amount > maxLimit)
                        {
                            // 🔥 F5BotV2 第2475行格式：@{nickname} 进仓失败!{key}超限,当前{amount},剩余:{maxLimit}
                            errorMessage = $"进仓失败!{key}超限,当前{item.Amount},剩余:{maxLimit}";
                            _logService.Warning("BinggoOrderValidator", $"❌ {errorMessage}");
                            _logService.Warning("BinggoOrderValidator", 
                                $"   详情: MaxBet={maxBet}, 已累计={accumulatedAmount}, 剩余={maxLimit}");
                            return false;
                        }
                    }
                }
                
                _logService.Info("BinggoOrderValidator", "✅ 单注金额验证通过（含当期累计限额检查）");
                
                // 5. 验证总金额
                decimal totalAmount = betContent.TotalAmount;
                float maxBetPerIssue = _configService.GetMaxBetPerIssue();
                
                if (totalAmount > (decimal)maxBetPerIssue)
                {
                    errorMessage = $"单期总投注不能超过 {maxBetPerIssue} 元";
                    return false;
                }
                
                // 6. 验证余额
                // 🔥 重要：托单也要验证余额！（托单是正常玩家，走正常流程）
                // 只有管理员不验证余额（管理员不扣钱）
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
                _logService.Error("BinggoOrderValidator", $"验证下注时发生异常: {ex.Message}", ex);
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
                _logService.Error("BinggoOrderValidator", $"验证补单时发生异常: {ex.Message}", ex);
                errorMessage = "系统错误，请稍后重试";
                return false;
            }
        }
    }
}

