using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using SQLite;
using BaiShengVx3Plus.Attributes;

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
        // 属性（带变化通知 + DataGridView 列配置）
        // ========================================

        [PrimaryKey, AutoIncrement]
        [Browsable(false)]  // 🔥 不在 DataGridView 中显示
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
        [DataGridColumn(HeaderText = "微信ID", Width = 120, Order = 1)]
        public string? Wxid
        {
            get => _wxid;
            set => SetField(ref _wxid, value);
        }

        [Indexed]
        [DataGridColumn(HeaderText = "期号", Width = 80, Order = 2, 
                        Alignment = DataGridViewContentAlignment.MiddleCenter)]
        public int IssueId
        {
            get => _issueId;
            set => SetField(ref _issueId, value);
        }

        [DataGridColumn(HeaderText = "账号", Width = 100, Order = 3)]
        public string? Account
        {
            get => _account;
            set => SetField(ref _account, value);
        }

        [DataGridColumn(HeaderText = "昵称", Width = 100, Order = 4)]
        public string? Nickname
        {
            get => _nickname;
            set => SetField(ref _nickname, value);
        }

        [Browsable(false)]  // 🔥 不在 DataGridView 中显示（时间戳）
        public long TimeStampBet
        {
            get => _timeStampBet;
            set => SetField(ref _timeStampBet, value);
        }

        // ========================================
        // 🔥 业务订单属性（对应 F5BotV2 + DataGridView 列配置）
        // ========================================

        [DataGridColumn(HeaderText = "投注内容", Width = 200, Order = 5)]
        public string? BetContentOriginal
        {
            get => _betContentOriginal;
            set => SetField(ref _betContentOriginal, value);
        }

        [Browsable(false)]  // 🔥 不显示标准内容（给业务逻辑用）
        public string? BetContentStandar
        {
            get => _betContentStandar;
            set => SetField(ref _betContentStandar, value);
        }

        [DataGridColumn(HeaderText = "注数", Width = 60, Order = 6, 
                        Alignment = DataGridViewContentAlignment.MiddleRight)]
        public int Nums
        {
            get => _nums;
            set => SetField(ref _nums, value);
        }

        [DataGridColumn(HeaderText = "金额", Width = 80, Order = 7, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float AmountTotal
        {
            get => _amountTotal;
            set => SetField(ref _amountTotal, value);
        }

        [DataGridColumn(HeaderText = "盈利", Width = 80, Order = 8, 
                        Format = "{0:+0.00;-0.00;0.00}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float Profit
        {
            get => _profit;
            set => SetField(ref _profit, value);
        }

        [DataGridColumn(HeaderText = "纯利", Width = 80, Order = 9, 
                        Format = "{0:+0.00;-0.00;0.00}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float NetProfit
        {
            get => _netProfit;
            set => SetField(ref _netProfit, value);
        }

        [DataGridColumn(HeaderText = "赔率", Width = 60, Order = 10, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleCenter)]
        public float Odds
        {
            get => _odds;
            set => SetField(ref _odds, value);
        }

        [DataGridColumn(HeaderText = "状态", Width = 80, Order = 11, 
                        Alignment = DataGridViewContentAlignment.MiddleCenter)]
        public OrderStatus OrderStatus
        {
            get => _orderStatus;
            set => SetField(ref _orderStatus, value);
        }

        [DataGridColumn(HeaderText = "类型", Width = 60, Order = 12, 
                        Alignment = DataGridViewContentAlignment.MiddleCenter)]
        public OrderType OrderType
        {
            get => _orderType;
            set => SetField(ref _orderType, value);
        }

        [DataGridColumn(HeaderText = "时间", Width = 150, Order = 13)]
        public string? TimeString
        {
            get => _timeString;
            set => SetField(ref _timeString, value);
        }

        [DataGridColumn(HeaderText = "备注", Width = 100, Order = 14)]
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
