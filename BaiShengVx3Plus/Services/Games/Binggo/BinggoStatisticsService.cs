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
        /// 🔥 订单创建时立即增加统计（参考 F5BotV2 第 538-573 行：OnMemberOrderCreate）
        /// 实时增减，而不是重新计算
        /// </summary>
        public void OnOrderCreated(V2MemberOrder order)
        {
            try
            {
                // 🔥 跳过托单和已取消订单（参考 F5BotV2 第 548 行）
                if (order.OrderType == OrderType.托 || order.OrderStatus == OrderStatus.已取消)
                    return;
                
                // 🔥 使用 TimeStampBet 获取订单日期（参考 F5BotV2 第 550 行：LxTimestampHelper.GetDateTime(order.TimeStampBet)）
                // 注意：TimeStampBet 是下注时间戳，CreatedAt 是数据库记录创建时间，两者可能不同
                DateTime orderDate;
                try
                {
                    orderDate = LxLib.LxSys.LxTimestampHelper.GetDateTime(order.TimeStampBet).Date;
                }
                catch
                {
                    // 如果时间戳转换失败，使用 CreatedAt 作为后备
                    orderDate = order.CreatedAt.Date;
                    _logService.Warning("BinggoStatistics", $"订单 {order.Id} 时间戳转换失败，使用 CreatedAt: {order.TimeStampBet}");
                }
                
                DateTime today = DateTime.Now.Date;
                int amount = (int)order.AmountTotal;
                
                // 🔥 总下注（总是增加，参考 F5BotV2 第 555、565 行）
                BetMoneyTotal += amount;
                
                // 🔥 今日下注（如果是今天的订单，参考 F5BotV2 第 552-555 行）
                if (orderDate == today)
                {
                    BetMoneyToday += amount;
                    
                    // 🔥 当期下注（如果是当前期号，参考 F5BotV2 第 557-560 行）
                    if (order.IssueId == IssueidCur)
                    {
                        BetMoneyCur += amount;
                    }
                }
                // 🔥 如果不是今天的订单，只增加总注，不增加今日和当前（参考 F5BotV2 第 563-565 行）
                
                _logService.Debug("BinggoStatistics", 
                    $"📊 统计增加: 订单 {order.Id} - 金额 {amount} - 总注 {BetMoneyTotal} 今投 {BetMoneyToday} 当前 {BetMoneyCur} - 期号 {order.IssueId} 当前期号 {IssueidCur} 订单日期 {orderDate:yyyy-MM-dd} 今天 {today:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoStatistics", $"OnOrderCreated 失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 订单取消时立即减掉统计（参考 F5BotV2 第 680-709 行：OnMemberOrderCancel）
        /// 实时增减，而不是重新计算
        /// </summary>
        public void OnOrderCanceled(V2MemberOrder order)
        {
            try
            {
                // 🔥 跳过托单（参考 F5BotV2 第 688 行）
                if (order.OrderType == OrderType.托)
                {
                    _logService.Debug("BinggoStatistics", $"跳过托单取消统计: 订单 {order.Id}");
                    return;
                }
                
                // 🔥 使用 TimeStampBet 获取订单日期（参考 F5BotV2 第 690 行：LxTimestampHelper.GetDateTime(order.TimeStampBet)）
                // 注意：TimeStampBet 是下注时间戳，CreatedAt 是数据库记录创建时间，两者可能不同
                DateTime orderDate;
                try
                {
                    orderDate = LxLib.LxSys.LxTimestampHelper.GetDateTime(order.TimeStampBet).Date;
                }
                catch
                {
                    // 如果时间戳转换失败，使用 CreatedAt 作为后备
                    orderDate = order.CreatedAt.Date;
                    _logService.Warning("BinggoStatistics", $"订单 {order.Id} 时间戳转换失败，使用 CreatedAt: {order.TimeStampBet}");
                }
                
                DateTime today = DateTime.Now.Date;
                int amount = (int)order.AmountTotal;
                
                // 🔥 总下注（总是减掉，参考 F5BotV2 第 694、703 行）
                BetMoneyTotal -= amount;
                
                // 🔥 今日下注（如果是今天的订单，参考 F5BotV2 第 692-695 行）
                if (orderDate == today)
                {
                    BetMoneyToday -= amount;
                    
                    // 🔥 当期下注（如果是当前期号，参考 F5BotV2 第 696-699 行）
                    if (order.IssueId == IssueidCur)
                    {
                        BetMoneyCur -= amount;
                    }
                }
                // 🔥 如果不是今天的订单，只减掉总注，不减今日和当前（参考 F5BotV2 第 701-704 行）
                
                _logService.Info("BinggoStatistics", 
                    $"📊 统计减少: 订单 {order.Id} - 金额 {amount} - 总注 {BetMoneyTotal} 今投 {BetMoneyToday} 当前 {BetMoneyCur} - 期号 {order.IssueId} 当前期号 {IssueidCur} 订单日期 {orderDate:yyyy-MM-dd} 今天 {today:yyyy-MM-dd} 时间戳 {order.TimeStampBet}");
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoStatistics", $"OnOrderCanceled 失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔥 订单结算时更新盈利统计（参考 F5BotV2 第 626-635 行：OnMemberOrderFinish）
        /// 只更新盈利，不更新投注金额（投注金额在下单时已更新）
        /// </summary>
        public void OnOrderSettled(V2MemberOrder order)
        {
            try
            {
                // 🔥 跳过托单（参考 F5BotV2 第 626 行）
                if (order.OrderType == OrderType.托)
                    return;
                
                // 🔥 只更新已结算的订单（参考 F5BotV2 第 599 行）
                if (order.OrderStatus != OrderStatus.已完成)
                    return;
                
                // 🔥 使用 TimeStampBet 获取订单日期（参考 F5BotV2 第 550 行：LxTimestampHelper.GetDateTime(order.TimeStampBet)）
                // 注意：TimeStampBet 是下注时间戳，CreatedAt 是数据库记录创建时间，两者可能不同
                DateTime orderDate;
                try
                {
                    orderDate = LxLib.LxSys.LxTimestampHelper.GetDateTime(order.TimeStampBet).Date;
                }
                catch
                {
                    // 如果时间戳转换失败，使用 CreatedAt 作为后备
                    orderDate = order.CreatedAt.Date;
                    _logService.Warning("BinggoStatistics", $"订单 {order.Id} 时间戳转换失败，使用 CreatedAt: {order.TimeStampBet}");
                }
                
                DateTime today = DateTime.Now.Date;
                float netProfit = order.NetProfit;  // 纯利
                
                // 🔥 总盈亏和今日盈亏（参考 F5BotV2 第 630-631 行）
                // 注意：F5BotV2 使用 -= order.NetProfit，但我们的系统 NetProfit 已经是纯利（正数=盈利，负数=亏损）
                // 所以直接 += 即可
                IncomeTotal += netProfit;
                
                if (orderDate == today)
                {
                    IncomeToday += netProfit;
                }
                
                _logService.Debug("BinggoStatistics", 
                    $"📊 盈利统计更新: 订单 {order.Id} - 纯利 {netProfit:F2} - 总盈 {IncomeTotal:F2} 今盈 {IncomeToday:F2}");
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoStatistics", $"OnOrderSettled 失败: {ex.Message}", ex);
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
                // 期号变更后重新计算本期下注（因为期号变了，需要重新计算）
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

