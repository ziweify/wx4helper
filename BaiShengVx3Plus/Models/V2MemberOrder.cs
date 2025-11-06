using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 订单数据模型（实现 INotifyPropertyChanged，支持属性变化通知）
    /// 使用 SQLite-net ORM 特性，自动建表和增删改
    /// 
    /// 🔥 字段对照（参考 F5BotV2 的 V2MemberOrder）：
    /// - Id = 主键（自增）
    /// - GroupWxId = 群ID
    /// - Wxid = 会员微信ID
    /// - Account = 会员号码
    /// - Nickname = 会员昵称
    /// - IssueId = 期号
    /// - TimeStampBet = 下注时间戳
    /// - BetContentOriginal = 原始投注内容
    /// - BetContentStandar = 标准投注内容
    /// - Nums = 注码数量
    /// - AmountTotal = 投注总金额
    /// - Profit = 盈利
    /// - NetProfit = 纯利
    /// - Odds = 赔率
    /// - OrderStatus = 订单状态
    /// - OrderType = 订单类型
    /// - TimeString = 日期时间字符串
    /// - Notes = 备注
    /// </summary>
    public class V2MemberOrder : INotifyPropertyChanged
    {
        // ========================================
        // 主键和基础字段
        // ========================================

        private long _id;
        private long _timeStampBet;

        // ========================================
        // 🔥 联系人信息字段（对应 F5BotV2 的 IWxContacts）
        // ========================================
        private string _groupWxId = "";
        private string? _wxid;
        private string? _account;
        private string? _nickname;

        // ========================================
        // 🔥 业务订单字段（对应 F5BotV2）
        // ========================================
        private int _issueId;
        private string? _betContentOriginal;
        private string? _betContentStandar;
        private int _nums;
        private float _amountTotal;
        private float _profit;
        private float _netProfit;
        private float _odds;
        private OrderStatus _orderStatus;
        private OrderType _orderType;
        private string? _timeString;
        private string? _notes;

        // ========================================
        // 属性（带变化通知）
        // ========================================

        [PrimaryKey, AutoIncrement]
        public long Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        [Indexed, DisplayName("群ID")]
        public string GroupWxId
        {
            get => _groupWxId;
            set => SetField(ref _groupWxId, value);
        }

        [Indexed, DisplayName("WxID")]
        public string? Wxid
        {
            get => _wxid;
            set => SetField(ref _wxid, value);
        }

        [Indexed, DisplayName("期号")]
        public int IssueId
        {
            get => _issueId;
            set => SetField(ref _issueId, value);
        }

        [DisplayName("会员号码")]
        public string? Account
        {
            get => _account;
            set => SetField(ref _account, value);
        }

        [DisplayName("昵称")]
        public string? Nickname
        {
            get => _nickname;
            set => SetField(ref _nickname, value);
        }

        public long TimeStampBet
        {
            get => _timeStampBet;
            set => SetField(ref _timeStampBet, value);
        }

        // ========================================
        // 🔥 业务订单属性（对应 F5BotV2）
        // ========================================

        [DisplayName("原始内容")]
        public string? BetContentOriginal
        {
            get => _betContentOriginal;
            set => SetField(ref _betContentOriginal, value);
        }

        [DisplayName("标准内容")]
        public string? BetContentStandar
        {
            get => _betContentStandar;
            set => SetField(ref _betContentStandar, value);
        }

        [DisplayName("数量")]
        public int Nums
        {
            get => _nums;
            set => SetField(ref _nums, value);
        }

        [DisplayName("总金额")]
        public float AmountTotal
        {
            get => _amountTotal;
            set => SetField(ref _amountTotal, value);
        }

        [DisplayName("盈利")]
        public float Profit
        {
            get => _profit;
            set => SetField(ref _profit, value);
        }

        [DisplayName("纯利")]
        public float NetProfit
        {
            get => _netProfit;
            set => SetField(ref _netProfit, value);
        }

        [DisplayName("赔率")]
        public float Odds
        {
            get => _odds;
            set => SetField(ref _odds, value);
        }

        [DisplayName("状态")]
        public OrderStatus OrderStatus
        {
            get => _orderStatus;
            set => SetField(ref _orderStatus, value);
        }

        [DisplayName("类型")]
        public OrderType OrderType
        {
            get => _orderType;
            set => SetField(ref _orderType, value);
        }

        [DisplayName("日期时间")]
        public string? TimeString
        {
            get => _timeString;
            set => SetField(ref _timeString, value);
        }

        [DisplayName("备注")]
        public string? Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        // ========================================
        // INotifyPropertyChanged 实现
        // ========================================

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
