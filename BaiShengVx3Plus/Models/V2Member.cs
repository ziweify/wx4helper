using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 会员数据模型（实现 INotifyPropertyChanged，支持属性变化通知）
    /// 使用 SQLite-net ORM 特性，自动建表和增删改
    /// 
    /// 🔥 字段对照（参考 F5BotV2）：
    /// - Id = 主键（自增）
    /// - GroupWxId = 群ID
    /// - Wxid = 微信ID
    /// - Account = 微信号（对应 F5BotV2 的 account）
    /// - Nickname = 昵称（对应 F5BotV2 的 nickname）
    /// - DisplayName = 群昵称（对应 F5BotV2 的 display_name）
    /// - Balance = 余额（对应 F5BotV2 的 Balance）
    /// - State = 状态（对应 F5BotV2 的 State）
    /// - 其他业务字段与 F5BotV2 完全一致
    /// </summary>
    public class V2Member : INotifyPropertyChanged
    {
        // ========================================
        // 主键和基础字段
        // ========================================

        private long _id;

        // ========================================
        // 🔥 联系人信息字段（对应 F5BotV2 的 IWxContacts）
        // ========================================
        private string _groupWxId = "";
        private string? _wxid;
        private string? _account;
        private string? _nickname;
        private string? _displayName;

        // ========================================
        // 🔥 业务统计字段（对应 F5BotV2 的业务字段）
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
        // 🔥 业务统计属性（对应 F5BotV2）
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
