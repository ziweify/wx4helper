using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 会员数据模型（实现 INotifyPropertyChanged，支持属性变化通知）
    /// </summary>
    public class V2Member : INotifyPropertyChanged
    {
        // ========================================
        // 主键和基础字段
        // ========================================

        private long _id;
        private long _memberId;
        private string? _memberName;
        private string? _memberAlias;
        private double _memberAmount;
        private MemberState _memberState;
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
        private string? _displayName;

        // ========================================
        // 🔥 业务统计字段
        // ========================================
        private float _balance;
        private MemberState _state;
        private float _betCur;
        private float _betWait;
        private float _incomeToday;
        private float _creditToday;
        private float _betToday;
        private float _withdrawToday;
        private float _betTotal;
        private float _creditTotal;
        private float _withdrawTotal;
        private float _incomeTotal;

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

        public string? MemberAlias
        {
            get => _memberAlias;
            set => SetField(ref _memberAlias, value);
        }

        public double MemberAmount
        {
            get => _memberAmount;
            set => SetField(ref _memberAmount, value);
        }

        public MemberState MemberState
        {
            get => _memberState;
            set => SetField(ref _memberState, value);
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

        [DisplayName("WxID")]
        public string? Wxid
        {
            get => _wxid;
            set => SetField(ref _wxid, value);
        }

        [DisplayName("号")]
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

        [DisplayName("群昵称")]
        public string? DisplayName
        {
            get => _displayName;
            set => SetField(ref _displayName, value);
        }

        // ========================================
        // 🔥 业务统计属性
        // ========================================

        [DisplayName("余额")]
        public float Balance
        {
            get => _balance;
            set => SetField(ref _balance, value);
        }

        [DisplayName("状态")]
        public MemberState State
        {
            get => _state;
            set => SetField(ref _state, value);
        }

        [DisplayName("本期下注")]
        public float BetCur
        {
            get => _betCur;
            set => SetField(ref _betCur, value);
        }

        [DisplayName("待结算")]
        public float BetWait
        {
            get => _betWait;
            set => SetField(ref _betWait, value);
        }

        [DisplayName("今日盈亏")]
        public float IncomeToday
        {
            get => _incomeToday;
            set => SetField(ref _incomeToday, value);
        }

        [DisplayName("今日上分")]
        public float CreditToday
        {
            get => _creditToday;
            set => SetField(ref _creditToday, value);
        }

        [DisplayName("今日下注")]
        public float BetToday
        {
            get => _betToday;
            set => SetField(ref _betToday, value);
        }

        [DisplayName("今日下分")]
        public float WithdrawToday
        {
            get => _withdrawToday;
            set => SetField(ref _withdrawToday, value);
        }

        [DisplayName("总下注")]
        public float BetTotal
        {
            get => _betTotal;
            set => SetField(ref _betTotal, value);
        }

        [DisplayName("总上分")]
        public float CreditTotal
        {
            get => _creditTotal;
            set => SetField(ref _creditTotal, value);
        }

        [DisplayName("总下分")]
        public float WithdrawTotal
        {
            get => _withdrawTotal;
            set => SetField(ref _withdrawTotal, value);
        }

        [DisplayName("总盈亏")]
        public float IncomeTotal
        {
            get => _incomeTotal;
            set => SetField(ref _incomeTotal, value);
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
