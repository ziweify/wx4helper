using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Core;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Games.Binggo
{
    /// <summary>
    /// Binggo 游戏统计服务
    /// 🔥 完全参考 F5BotV2 的 BoterServices 统计逻辑（第 790-807 行）
    /// 统一管理所有统计数据的计算和更新
    /// </summary>
    public class BinggoStatisticsService : INotifyPropertyChanged
    {
        private readonly ILogService _logService;
        private V2MemberBindingList? _membersBindingList;
        private V2OrderBindingList? _ordersBindingList;
        
        // ========================================
        // 🔥 统计字段（参考 F5BotV2 第 266-360 行）
        // ========================================
        
        private int _betMoneyTotal;     // 总下注
        private int _betMoneyToday;     // 今日下注
        private int _betMoneyCur;       // 本期下注
        private float _incomeTotal;     // 总盈亏
        private float _incomeToday;     // 今日盈亏
        private int _creditTotal;       // 总上分
        private int _creditToday;       // 今日上分
        private int _withdrawTotal;     // 总下分
        private int _withdrawToday;     // 今日下分
        private int _issueidCur;        // 当前期号
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public BinggoStatisticsService(ILogService logService)
        {
            _logService = logService;
        }
        
        // ========================================
        // 属性（参考 F5BotV2）
        // ========================================
        
        public int BetMoneyTotal
        {
            get => _betMoneyTotal;
            set => SetField(ref _betMoneyTotal, value);
        }
        
        public int BetMoneyToday
        {
            get => _betMoneyToday;
            set => SetField(ref _betMoneyToday, value);
        }
        
        public int BetMoneyCur
        {
            get => _betMoneyCur;
            set => SetField(ref _betMoneyCur, value);
        }
        
        public float IncomeTotal
        {
            get => _incomeTotal;
            set => SetField(ref _incomeTotal, value);
        }
        
        public float IncomeToday
        {
            get => _incomeToday;
            set => SetField(ref _incomeToday, value);
        }
        
        public int CreditTotal
        {
            get => _creditTotal;
            set => SetField(ref _creditTotal, value);
        }
        
        public int CreditToday
        {
            get => _creditToday;
            set => SetField(ref _creditToday, value);
        }
        
        public int WithdrawTotal
        {
            get => _withdrawTotal;
            set => SetField(ref _withdrawTotal, value);
        }
        
        public int WithdrawToday
        {
            get => _withdrawToday;
            set => SetField(ref _withdrawToday, value);
        }
        
        public int IssueidCur
        {
            get => _issueidCur;
            set => SetField(ref _issueidCur, value);
        }
        
        /// <summary>
        /// 盘口描述字符串
        /// 🔥 完全参考 F5BotV2 第 805 行
        /// 🔥 所有金额显示小数点后 2 位
        /// </summary>
        public string PanDescribe => 
            $"总注:{BetMoneyTotal:F2}|今投:{BetMoneyToday:F2}|当前:{IssueidCur}投注:{BetMoneyCur:F2} | 总/今盈利:{IncomeTotal:F2}/{IncomeToday:F2} | 总上/今上:{CreditTotal:F2}/{CreditToday:F2} 总下/今下:{WithdrawTotal:F2}/{WithdrawToday:F2}";
        
        // ========================================
        // 方法
        // ========================================
        
        /// <summary>
        /// 设置绑定列表
        /// </summary>
        public void SetBindingLists(V2MemberBindingList? membersBindingList, V2OrderBindingList? ordersBindingList)
        {
            _membersBindingList = membersBindingList;
            _ordersBindingList = ordersBindingList;
        }
        
        /// <summary>
        /// 更新统计数据
        /// 🔥 完全参考 F5BotV2 的 UpdataPanDescribe 方法（第 790-807 行）
        /// 这是唯一的统计更新方法，所有地方都调用它
        /// </summary>
        /// <param name="setZero">是否清零（切换群时使用）</param>
        public void UpdateStatistics(bool setZero = false)
        {
            try
            {
                if (setZero)
                {
                    // 🔥 清零所有统计（参考 F5BotV2 第 793-804 行）
                    BetMoneyTotal = 0;
                    BetMoneyToday = 0;
                    BetMoneyCur = 0;
                    IncomeTotal = 0f;
                    IncomeToday = 0f;
                    CreditTotal = 0;
                    WithdrawTotal = 0;
                    CreditToday = 0;
                    WithdrawToday = 0;
                    
                    _logService.Info("BinggoStatistics", "统计数据已清零");
                    return;
                }
                
                // 🔥 从订单列表重新计算所有统计（参考 F5BotV2 第 548-570 行）
                if (_ordersBindingList == null || _ordersBindingList.Count == 0)
                {
                    UpdateStatistics(setZero: true);
                    return;
                }
                
                DateTime today = DateTime.Now.Date;
                int totalBet = 0;
                int todayBet = 0;
                int curBet = 0;
                float totalIncome = 0f;
                float todayIncome = 0f;
                
                foreach (var order in _ordersBindingList)
                {
                    // 🔥 跳过托单和已取消订单（参考 F5BotV2 第 548 行）
                    if (order.OrderType == OrderType.托 || order.OrderStatus == OrderStatus.已取消)
                        continue;
                    
                    // 总下注
                    totalBet += (int)order.AmountTotal;
                    
                    // 今日下注
                    if (order.CreatedAt.Date == today)
                    {
                        todayBet += (int)order.AmountTotal;
                    }
                    
                    // 当期下注
                    if (order.IssueId == IssueidCur)
                    {
                        curBet += (int)order.AmountTotal;
                    }
                    
                    // 总盈亏和今日盈亏（已结算的订单）
                    if (order.OrderStatus == OrderStatus.已完成)
                    {
                        totalIncome += order.NetProfit;
                        if (order.CreatedAt.Date == today)
                        {
                            todayIncome += order.NetProfit;
                        }
                    }
                }
                
                // 🔥 从会员列表计算上下分（如果有的话）
                if (_membersBindingList != null)
                {
                    int totalCredit = 0;
                    int todayCredit = 0;
                    int totalWithdraw = 0;
                    int todayWithdraw = 0;
                    
                    foreach (var member in _membersBindingList)
                    {
                        totalCredit += (int)member.CreditTotal;
                        todayCredit += (int)member.CreditToday;
                        totalWithdraw += (int)member.WithdrawTotal;
                        todayWithdraw += (int)member.WithdrawToday;
                    }
                    
                    CreditTotal = totalCredit;
                    CreditToday = todayCredit;
                    WithdrawTotal = totalWithdraw;
                    WithdrawToday = todayWithdraw;
                }
                
                // 更新统计数据
                BetMoneyTotal = totalBet;
                BetMoneyToday = todayBet;
                BetMoneyCur = curBet;
                IncomeTotal = totalIncome;
                IncomeToday = todayIncome;
                
                _logService.Info("BinggoStatistics", 
                    $"统计更新: 总注{totalBet} 今投{todayBet} 当前{curBet} 总盈{totalIncome:F2} 今盈{todayIncome:F2}");
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoStatistics", $"更新统计失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 设置当前期号
        /// </summary>
        public void SetCurrentIssueId(int issueId)
        {
            if (IssueidCur != issueId)
            {
                IssueidCur = issueId;
                // 期号变更后重新计算本期下注
                UpdateStatistics();
            }
        }
        
        // ========================================
        // INotifyPropertyChanged 实现
        // ========================================
        
        protected void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                
                // 🔥 任何字段变化都触发 PanDescribe 更新
                if (propertyName != nameof(PanDescribe))
                {
                    OnPropertyChanged(nameof(PanDescribe));
                }
            }
        }
        
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

