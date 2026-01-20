using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace YongLiSystem.Models.Wechat
{
    /// <summary>
    /// 会员数据模型（用于数据绑定）
    /// </summary>
    public class Member : INotifyPropertyChanged
    {
        private long _id;
        private string _groupWxId = "";
        private string? _wxid;
        private string? _account;
        private string? _nickname;
        private string? _displayName;
        private decimal _balance;
        private MemberState _state;
        private decimal _betCur;
        private decimal _betWait;
        private decimal _incomeToday;
        private decimal _creditToday;
        private decimal _betToday;
        private decimal _withdrawToday;
        private decimal _betTotal;
        private decimal _creditTotal;
        private decimal _withdrawTotal;
        private decimal _incomeTotal;
        private DateTime _createdAt;
        private DateTime _updatedAt;

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

        public string? DisplayName
        {
            get => _displayName;
            set => SetField(ref _displayName, value);
        }

        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance
        {
            get => _balance;
            set => SetField(ref _balance, value);
        }

        /// <summary>
        /// 会员状态
        /// </summary>
        public MemberState State
        {
            get => _state;
            set => SetField(ref _state, value);
        }

        /// <summary>
        /// 当前投注金额
        /// </summary>
        public decimal BetCur
        {
            get => _betCur;
            set => SetField(ref _betCur, value);
        }

        /// <summary>
        /// 等待投注金额
        /// </summary>
        public decimal BetWait
        {
            get => _betWait;
            set => SetField(ref _betWait, value);
        }

        /// <summary>
        /// 今日收入
        /// </summary>
        public decimal IncomeToday
        {
            get => _incomeToday;
            set => SetField(ref _incomeToday, value);
        }

        /// <summary>
        /// 今日上分
        /// </summary>
        public decimal CreditToday
        {
            get => _creditToday;
            set => SetField(ref _creditToday, value);
        }

        /// <summary>
        /// 今日投注
        /// </summary>
        public decimal BetToday
        {
            get => _betToday;
            set => SetField(ref _betToday, value);
        }

        /// <summary>
        /// 今日下分
        /// </summary>
        public decimal WithdrawToday
        {
            get => _withdrawToday;
            set => SetField(ref _withdrawToday, value);
        }

        /// <summary>
        /// 总投注
        /// </summary>
        public decimal BetTotal
        {
            get => _betTotal;
            set => SetField(ref _betTotal, value);
        }

        /// <summary>
        /// 总上分
        /// </summary>
        public decimal CreditTotal
        {
            get => _creditTotal;
            set => SetField(ref _creditTotal, value);
        }

        /// <summary>
        /// 总下分
        /// </summary>
        public decimal WithdrawTotal
        {
            get => _withdrawTotal;
            set => SetField(ref _withdrawTotal, value);
        }

        /// <summary>
        /// 总收入
        /// </summary>
        public decimal IncomeTotal
        {
            get => _incomeTotal;
            set => SetField(ref _incomeTotal, value);
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
    /// 会员状态枚举
    /// </summary>
    public enum MemberState
    {
        /// <summary>
        /// 普通会员
        /// </summary>
        普通会员 = 0,

        /// <summary>
        /// 管理员
        /// </summary>
        管理员 = 1,

        /// <summary>
        /// 代理
        /// </summary>
        代理 = 2,

        /// <summary>
        /// 已退群
        /// </summary>
        已退群 = 3
    }
}

