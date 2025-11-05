using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 订单数据模型（实现 INotifyPropertyChanged，支持属性变化通知）
    /// </summary>
    public class V2MemberOrder : INotifyPropertyChanged
    {
        // ========================================
        // 主键和基础字段
        // ========================================

        private long _id;
        private long _memberId;
        private string? _memberName;
        private string? _orderId;
        private OrderStatus _orderStatus;
        private OrderType _orderType;
        private double _orderAmountPlan;
        private double _orderAmount;
        private string? _orderResult;
        private string? _orderTarget;
        private string? _orderPlace;
        private long _timeStampCreate;
        private long _timeStampUpdate;
        private long _timeStampBet;
        private string? _extra;

        // ========================================
        // 🔥 联系人信息字段（从 IWxContacts）
        // ========================================
        private string _groupWxId = "";
        private string? _wxid;
        private string? _account;
        private string? _nickname;

        // ========================================
        // 🔥 业务订单字段
        // ========================================
        private int _issueId;
        private string? _betContentOriginal;
        private string? _betContentStandar;
        private int _nums;
        private float _amountTotal;
        private float _profit;
        private float _netProfit;
        private float _odds;
        private string? _timeString;
        private string? _notes;

        // ========================================
        // 属性（带变化通知）
        // ========================================

        [DisplayName("群ID")]
        public string GroupWxId
        {
            get => _groupWxId;
            set => SetField(ref _groupWxId, value);
        }

        public long Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        public long MemberId
        {
            get => _memberId;
            set => SetField(ref _memberId, value);
        }

        public string? MemberName
        {
            get => _memberName;
            set => SetField(ref _memberName, value);
        }

        public string? OrderId
        {
            get => _orderId;
            set => SetField(ref _orderId, value);
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

        public double OrderAmountPlan
        {
            get => _orderAmountPlan;
            set => SetField(ref _orderAmountPlan, value);
        }

        public double OrderAmount
        {
            get => _orderAmount;
            set => SetField(ref _orderAmount, value);
        }

        public string? OrderResult
        {
            get => _orderResult;
            set => SetField(ref _orderResult, value);
        }

        public string? OrderTarget
        {
            get => _orderTarget;
            set => SetField(ref _orderTarget, value);
        }

        public string? OrderPlace
        {
            get => _orderPlace;
            set => SetField(ref _orderPlace, value);
        }

        public long TimeStampCreate
        {
            get => _timeStampCreate;
            set => SetField(ref _timeStampCreate, value);
        }

        public long TimeStampUpdate
        {
            get => _timeStampUpdate;
            set => SetField(ref _timeStampUpdate, value);
        }

        public long TimeStampBet
        {
            get => _timeStampBet;
            set => SetField(ref _timeStampBet, value);
        }

        public string? Extra
        {
            get => _extra;
            set => SetField(ref _extra, value);
        }

        // ========================================
        // 🔥 联系人信息属性（从 IWxContacts）
        // ========================================

        [DisplayName("会员ID")]
        public string? Wxid
        {
            get => _wxid;
            set => SetField(ref _wxid, value);
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

        // ========================================
        // 🔥 业务订单属性
        // ========================================

        [DisplayName("期号")]
        public int IssueId
        {
            get => _issueId;
            set => SetField(ref _issueId, value);
        }

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
