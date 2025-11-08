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
        private readonly BinggoGameSettings _settings;
        
        public BinggoOrderValidator(ILogService logService, BinggoGameSettings settings)
        {
            _logService = logService;
            _settings = settings;
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
                
                //// 2. 检查是否封盘（只有"开盘中"和"即将封盘"可以下注）
                //// 要在消息监控源头检测, 
                //if (currentStatus == BinggoLotteryStatus.封盘中 || currentStatus == BinggoLotteryStatus.开奖中)
                //{
                //    errorMessage = "已封盘，请等待下期！";
                //    return false;
                //}
                
                // 3. 检查下注内容是否有效
                if (betContent.Code != 0 || betContent.Items.Count == 0)
                {
                    errorMessage = betContent.ErrorMessage ?? "无效的下注内容";
                    return false;
                }
                
                // 4. 验证单注金额
                foreach (var item in betContent.Items)
                {
                    if (item.Amount < (decimal)_settings.MinBet)
                    {
                        errorMessage = $"单注金额不能小于 {_settings.MinBet} 元";
                        return false;
                    }
                    
                    if (item.Amount > (decimal)_settings.MaxBet)
                    {
                        errorMessage = $"单注金额不能超过 {_settings.MaxBet} 元";
                        return false;
                    }
                }
                
                // 5. 验证总金额
                decimal totalAmount = betContent.TotalAmount;
                
                if (totalAmount > (decimal)_settings.MaxBetPerIssue)
                {
                    errorMessage = $"单期总投注不能超过 {_settings.MaxBetPerIssue} 元";
                    return false;
                }
                
                // 6. 验证余额（如果不是托或管理）
                if (member.State != MemberState.托 && member.State != MemberState.管理)
                {
                    if ((decimal)member.Balance < totalAmount)
                    {
                        errorMessage = $"余额不足！当前余额: {member.Balance:F2}，需要: {totalAmount:F2}";
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
                
                if (amount > (decimal)_settings.MaxBet * 100) // 补单最大金额限制
                {
                    errorMessage = $"补单金额过大，最多 {_settings.MaxBet * 100} 元";
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

