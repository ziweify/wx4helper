using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Models.Games.Binggo;
using System;

namespace BaiShengVx3Plus.Services.Games.Binggo
{
    /// <summary>
    /// 炳狗订单验证器
    /// 
    /// 功能：
    /// 1. 验证会员余额
    /// 2. 验证下注金额限额
    /// 3. 验证下注状态（是否封盘）
    /// </summary>
    public class BinggoOrderValidator
    {
        private readonly ILogService _logService;
        private readonly IConfigurationService _configService;
        
        public BinggoOrderValidator(ILogService logService, IConfigurationService configService)
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
        /// <param name="errorMessage">错误信息（验证失败时）</param>
        /// <returns>是否验证通过</returns>
        public bool ValidateBet(
            V2Member member, 
            BinggoBetContent betContent, 
            BinggoLotteryStatus currentStatus,
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
                
                // 4. 验证单注金额
                float minBet = _configService.GetMinBet();
                float maxBet = _configService.GetMaxBet();
                _logService.Info("BinggoOrderValidator", $"🔍 开始验证单注金额限制: MinBet={minBet}, MaxBet={maxBet}");
                
                foreach (var item in betContent.Items)
                {
                    _logService.Info("BinggoOrderValidator", $"   - 检查投注项: 车{item.CarNumber} {item.PlayType}, 金额={item.Amount}");
                    
                    if (item.Amount < (decimal)minBet)
                    {
                        errorMessage = $"单注金额不能小于 {minBet} 元";
                        _logService.Warning("BinggoOrderValidator", $"❌ {errorMessage}（实际: {item.Amount}）");
                        return false;
                    }
                    
                    if (item.Amount > (decimal)maxBet)
                    {
                        errorMessage = $"单注金额不能超过 {maxBet} 元";
                        _logService.Warning("BinggoOrderValidator", $"❌ {errorMessage}（实际: {item.Amount}）");
                        return false;
                    }
                }
                
                _logService.Info("BinggoOrderValidator", "✅ 单注金额验证通过");
                
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

