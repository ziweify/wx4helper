using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using SQLite;
using zhaocaimao.Attributes;

namespace zhaocaimao.Models
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
        // 属性（带变化通知 + DataGridView 列配置）
        // ========================================

        [PrimaryKey, AutoIncrement]
        [DataGridColumn(HeaderText = "ID", Width = 50, Order = 0)]  // 🔥 显示 ID（用于上下分命令）
        public long Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        [Indexed]
        [Browsable(false)]  // 🔥 不在 DataGridView 中显示
        public string GroupWxId
        {
            get => _groupWxId;
            set => SetField(ref _groupWxId, value);
        }

        [Indexed]
        [DataGridColumn(HeaderText = "微信ID", Width = 150, Order = 1, Visible = false)]
        public string? Wxid
        {
            get => _wxid;
            set => SetField(ref _wxid, value);
        }

        [DataGridColumn(HeaderText = "账号", Width = 120, Order = 2, Visible = false)]
        public string? Account
        {
            get => _account;
            set => SetField(ref _account, value);
        }

        [DataGridColumn(HeaderText = "昵称", Width = 120, Order = 3)]
        public string? Nickname
        {
            get => _nickname;
            set => SetField(ref _nickname, value);
        }

        [DataGridColumn(HeaderText = "群昵称", Width = 120, Order = 4)]
        public string? DisplayName
        {
            get => _displayName;
            set => SetField(ref _displayName, value);
        }

        // ========================================
        // 🔥 业务统计属性（对应 F5BotV2 + DataGridView 列配置）
        // ========================================

        [DataGridColumn(HeaderText = "余额", Width = 100, Order = 5, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float Balance
        {
            get => _balance;
            set => SetField(ref _balance, value);
        }

        [DataGridColumn(HeaderText = "状态", Width = 80, Order = 6)]
        public MemberState State
        {
            get => _state;
            set => SetField(ref _state, value);
        }

        [DataGridColumn(HeaderText = "本期下注", Width = 90, Order = 7, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float BetCur
        {
            get => _betCur;
            set => SetField(ref _betCur, value);
        }

        [DataGridColumn(HeaderText = "待结算", Width = 90, Order = 8, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float BetWait
        {
            get => _betWait;
            set => SetField(ref _betWait, value);
        }

        [DataGridColumn(HeaderText = "今日盈亏", Width = 100, Order = 9, 
                        Format = "{0:+0.00;-0.00;0.00}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float IncomeToday
        {
            get => _incomeToday;
            set => SetField(ref _incomeToday, value);
        }

        [DataGridColumn(HeaderText = "今日上分", Width = 100, Order = 10, 
                        Format = "{0:N2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float CreditToday
        {
            get => _creditToday;
            set => SetField(ref _creditToday, value);
        }

        [DataGridColumn(HeaderText = "今日下注", Width = 90, Order = 11, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float BetToday
        {
            get => _betToday;
            set => SetField(ref _betToday, value);
        }

        [DataGridColumn(HeaderText = "今日下分", Width = 100, Order = 12, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float WithdrawToday
        {
            get => _withdrawToday;
            set => SetField(ref _withdrawToday, value);
        }

        [DataGridColumn(HeaderText = "总下注", Width = 100, Order = 13, 
                        Format = "{0:N2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float BetTotal
        {
            get => _betTotal;
            set => SetField(ref _betTotal, value);
        }

        [DataGridColumn(HeaderText = "总上分", Width = 100, Order = 14, 
                        Format = "{0:N2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float CreditTotal
        {
            get => _creditTotal;
            set => SetField(ref _creditTotal, value);
        }

        [DataGridColumn(HeaderText = "总下分", Width = 100, Order = 15, 
                        Format = "{0:N2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float WithdrawTotal
        {
            get => _withdrawTotal;
            set => SetField(ref _withdrawTotal, value);
        }

        [DataGridColumn(HeaderText = "总盈亏", Width = 100, Order = 16, 
                        Format = "{0:+0.00;-0.00;0.00}", Alignment = DataGridViewContentAlignment.MiddleRight)]
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
