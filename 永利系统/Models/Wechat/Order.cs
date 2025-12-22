using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;
using 永利系统.Models.Games.Bingo;

namespace 永利系统.Models.Wechat
{
    /// <summary>
    /// 订单数据模型（用于数据绑定）
    /// </summary>
    public class Order : INotifyPropertyChanged
    {
        private long _id;
        private string _groupWxId = "";
        private string? _wxid;
        private string? _account;
        private string? _nickname;
        private int _issueId;
        private string? _betContentOriginal;
        private string? _betContentStandard;
        private int _nums;
        private decimal _amountTotal;
        private decimal _betAmount;
        private decimal _betBeforeBalance;
        private decimal _betAfterBalance;
        private decimal _profit;
        private decimal _netProfit;
        private decimal _odds;
        private OrderStatus _orderStatus;
        private OrderType _orderType;
        private MemberState _memberState;
        private string? _timeString;
        private string? _notes;
        private bool _isSettled;
        private DateTime _createdAt;
        private DateTime _updatedAt;
        private long _timestampBet;

        // [PrimaryKey, AutoIncrement] // TODO: 添加 SQLite 特性
        public long Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        // [Indexed] // TODO: 添加 SQLite 特性
        public string GroupWxId
        {
            get => _groupWxId;
            set => SetField(ref _groupWxId, value);
        }

        // [Indexed] // TODO: 添加 SQLite 特性
        public string? Wxid
        {
            get => _wxid;
            set => SetField(ref _wxid, value);
        }

        public string? Account
        {
            get => _account;
            set => SetField(ref _account, value);
        }

        public string? Nickname
        {
            get => _nickname;
            set => SetField(ref _nickname, value);
        }

        /// <summary>
        /// 期号
        /// </summary>
        // [Indexed] // TODO: 添加 SQLite 特性
        public int IssueId
        {
            get => _issueId;
            set => SetField(ref _issueId, value);
        }

        /// <summary>
        /// 原始投注内容
        /// </summary>
        public string? BetContentOriginal
        {
            get => _betContentOriginal;
            set => SetField(ref _betContentOriginal, value);
        }

        /// <summary>
        /// 标准投注内容
        /// </summary>
        public string? BetContentStandard
        {
            get => _betContentStandard;
            set => SetField(ref _betContentStandard, value);
        }

        /// <summary>
        /// 注码数量
        /// </summary>
        public int Nums
        {
            get => _nums;
            set => SetField(ref _nums, value);
        }

        /// <summary>
        /// 投注总金额
        /// </summary>
        public decimal AmountTotal
        {
            get => _amountTotal;
            set => SetField(ref _amountTotal, value);
        }

        /// <summary>
        /// 投注金额
        /// </summary>
        public decimal BetAmount
        {
            get => _betAmount;
            set => SetField(ref _betAmount, value);
        }

        /// <summary>
        /// 投注前余额
        /// </summary>
        public decimal BetBeforeBalance
        {
            get => _betBeforeBalance;
            set => SetField(ref _betBeforeBalance, value);
        }

        /// <summary>
        /// 投注后余额
        /// </summary>
        public decimal BetAfterBalance
        {
            get => _betAfterBalance;
            set => SetField(ref _betAfterBalance, value);
        }

        /// <summary>
        /// 盈利
        /// </summary>
        public decimal Profit
        {
            get => _profit;
            set => SetField(ref _profit, value);
        }

        /// <summary>
        /// 纯利
        /// </summary>
        public decimal NetProfit
        {
            get => _netProfit;
            set => SetField(ref _netProfit, value);
        }

        /// <summary>
        /// 赔率
        /// </summary>
        public decimal Odds
        {
            get => _odds;
            set => SetField(ref _odds, value);
        }

        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderStatus OrderStatus
        {
            get => _orderStatus;
            set => SetField(ref _orderStatus, value);
        }

        /// <summary>
        /// 订单类型
        /// </summary>
        public OrderType OrderType
        {
            get => _orderType;
            set => SetField(ref _orderType, value);
        }

        /// <summary>
        /// 会员状态快照（订单创建时的会员状态）
        /// </summary>
        public MemberState MemberState
        {
            get => _memberState;
            set => SetField(ref _memberState, value);
        }

        /// <summary>
        /// 时间字符串
        /// </summary>
        public string? TimeString
        {
            get => _timeString;
            set => SetField(ref _timeString, value);
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        /// <summary>
        /// 是否已结算
        /// </summary>
        public bool IsSettled
        {
            get => _isSettled;
            set => SetField(ref _isSettled, value);
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetField(ref _createdAt, value);
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetField(ref _updatedAt, value);
        }

        /// <summary>
        /// 投注时间戳
        /// </summary>
        public long TimestampBet
        {
            get => _timestampBet;
            set => SetField(ref _timestampBet, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// 订单状态枚举
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// 待投注
        /// </summary>
        待投注 = 0,

        /// <summary>
        /// 已投注
        /// </summary>
        已投注 = 1,

        /// <summary>
        /// 已结算
        /// </summary>
        已结算 = 2,

        /// <summary>
        /// 已取消
        /// </summary>
        已取消 = 3
    }

    /// <summary>
    /// 订单类型枚举
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// 微信下单
        /// </summary>
        微信下单 = 0,

        /// <summary>
        /// 手动补单
        /// </summary>
        手动补单 = 1,

        /// <summary>
        /// 自动投注
        /// </summary>
        自动投注 = 2
    }
}

