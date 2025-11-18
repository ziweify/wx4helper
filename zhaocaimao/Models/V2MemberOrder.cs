using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using SQLite;
using zhaocaimao.Attributes;

namespace zhaocaimao.Models
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
        private string? _betContent;  // 用于开奖服务
        private int _nums;
        private float _amountTotal;
        private decimal _betAmount;  // 用于开奖服务
        private float _betFronMoney;  // 注前金额（下注前余额）
        private float _betAfterMoney; // 注后金额（下注后余额）
        private float _profit;
        private float _netProfit;
        private float _odds;
        private OrderStatus _orderStatus;
        private OrderType _orderType;
        private MemberState _memberState;  // 🔥 会员等级快照（订单创建时的会员状态）
        private string? _timeString;
        private string? _notes;
        private bool _isSettled;  // 是否已结算
        private DateTime _createdAt;  // 创建时间

        // ========================================
        // 属性（带变化通知 + DataGridView 列配置）
        // ========================================

        [PrimaryKey, AutoIncrement]
        [DataGridColumn(HeaderText = "ID", Width = 50, Order = 0)]  // 🔥 显示订单 ID
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
        [Browsable(false)]  // 🔥 不显示微信ID（占用空间）
        public string? Wxid
        {
            get => _wxid;
            set => SetField(ref _wxid, value);
        }

        [Browsable(false)]  // 🔥 不在 DataGridView 中显示（时间戳）
        public long TimeStampBet
        {
            get => _timeStampBet;
            set => SetField(ref _timeStampBet, value);
        }

        [DataGridColumn(HeaderText = "账号", Width = 100, Order = 15, Visible = false)]
        public string? Account
        {
            get => _account;
            set => SetField(ref _account, value);
        }

        /// <summary>
        /// 🔥 格式化的时间字符串（仅显示时间，不显示日期）
        /// 用户要求：仅显示时间，记录日期但不显示，避免占用过多位置
        /// </summary>
        [DataGridColumn(HeaderText = "时间", Width = 80, Order = 1, ReadOnly = true)]
        public string TimeOnly
        {
            get
            {
                if (string.IsNullOrEmpty(_timeString))
                    return "";
                
                try
                {
                    // 从 "yyyy-MM-dd HH:mm:ss" 中提取 "HH:mm:ss"
                    if (_timeString.Length >= 19)
                    {
                        return _timeString.Substring(11, 8);  // 提取时间部分
                    }
                    return _timeString;
                }
                catch
                {
                    return _timeString;
                }
            }
        }

        [Indexed]
        [DataGridColumn(HeaderText = "期号", Width = 85, Order = 2, 
                        Alignment = DataGridViewContentAlignment.MiddleCenter)]
        public int IssueId
        {
            get => _issueId;
            set => SetField(ref _issueId, value);
        }

        [DataGridColumn(HeaderText = "昵称", Width = 100, Order = 3)]
        public string? Nickname
        {
            get => _nickname;
            set => SetField(ref _nickname, value);
        }

        // ========================================
        // 🔥 业务订单属性（按用户要求顺序排列）
        // 顺序：时间, 期号, 昵称, 原始内容, 标准内容, 注前金额, 注后金额, 单数, 赔率, 总金额, 纯利润, 状态, 类型, 会员
        // ========================================

        [DataGridColumn(HeaderText = "原始内容", Width = 120, Order = 4)]
        public string? BetContentOriginal
        {
            get => _betContentOriginal;
            set => SetField(ref _betContentOriginal, value);
        }

        [DataGridColumn(HeaderText = "标准内容", Width = 120, Order = 5)]
        public string? BetContentStandar
        {
            get => _betContentStandar;
            set => SetField(ref _betContentStandar, value);
        }

        [DataGridColumn(HeaderText = "注前金额", Width = 80, Order = 6, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float BetFronMoney
        {
            get => _betFronMoney;
            set => SetField(ref _betFronMoney, value);
        }

        [DataGridColumn(HeaderText = "注后金额", Width = 80, Order = 7, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float BetAfterMoney
        {
            get => _betAfterMoney;
            set => SetField(ref _betAfterMoney, value);
        }

        [DataGridColumn(HeaderText = "单数", Width = 60, Order = 8, 
                        Alignment = DataGridViewContentAlignment.MiddleRight)]
        public int Nums
        {
            get => _nums;
            set => SetField(ref _nums, value);
        }

        [DataGridColumn(HeaderText = "赔率", Width = 60, Order = 9, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleCenter)]
        public float Odds
        {
            get => _odds;
            set => SetField(ref _odds, value);
        }

        [DataGridColumn(HeaderText = "总金额", Width = 80, Order = 10, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float AmountTotal
        {
            get => _amountTotal;
            set => SetField(ref _amountTotal, value);
        }

        [DataGridColumn(HeaderText = "纯利", Width = 70, Order = 11, 
                        Format = "{0:F2}", Alignment = DataGridViewContentAlignment.MiddleRight)]
        public float NetProfit
        {
            get => _netProfit;
            set => SetField(ref _netProfit, value);
        }

        [DataGridColumn(HeaderText = "状态", Width = 70, Order = 12, 
                        Alignment = DataGridViewContentAlignment.MiddleCenter)]
        public OrderStatus OrderStatus
        {
            get => _orderStatus;
            set => SetField(ref _orderStatus, value);
        }

        [DataGridColumn(HeaderText = "类型", Width = 60, Order = 13, 
                        Alignment = DataGridViewContentAlignment.MiddleCenter)]
        public OrderType OrderType
        {
            get => _orderType;
            set => SetField(ref _orderType, value);
        }

        /// <summary>
        /// 🔥 会员等级快照（订单创建时的会员状态）
        /// 用于扩展业务规则：按会员等级做差异化处理
        /// </summary>
        [DataGridColumn(HeaderText = "会员等级", Width = 70, Order = 14, 
                        Alignment = DataGridViewContentAlignment.MiddleCenter)]
        public MemberState MemberState
        {
            get => _memberState;
            set => SetField(ref _memberState, value);
        }

        [DataGridColumn(HeaderText = "备注", Width = 100, Order = 16)]
        public string? Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        [Browsable(false)]  // 🔥 不显示完整时间字符串（已有 TimeOnly）
        public string? TimeString
        {
            get => _timeString;
            set => SetField(ref _timeString, value);
        }

        [Browsable(false)]  // 🔥 不显示 Profit（盈利），只显示 NetProfit（纯利润）
        public float Profit
        {
            get => _profit;
            set => SetField(ref _profit, value);
        }

        // ========================================
        // 🔥 开奖服务专用字段
        // ========================================

        [Browsable(false)]
        public string? BetContent
        {
            get => _betContent;
            set => SetField(ref _betContent, value);
        }

        [Browsable(false)]
        public decimal BetAmount
        {
            get => _betAmount;
            set => SetField(ref _betAmount, value);
        }

        [Browsable(false)]
        public bool IsSettled
        {
            get => _isSettled;
            set => SetField(ref _isSettled, value);
        }

        [Browsable(false)]
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetField(ref _createdAt, value);
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
    
    /// <summary>
    /// 🔥 订单排序比较器（参考 F5BotV2 V2MemberOrderComparerDefault）
    /// 排序规则：
    /// 1. 首先按 IssueId 排序
    /// 2. 然后按 Nickname.GetHashCode() 排序（名字的哈希值，确保同名订单在一起）
    /// 3. 最后按 TimeStampBet 排序（下注时间戳）
    /// </summary>
    public class V2MemberOrderComparerDefault : IComparer<V2MemberOrder>
    {
        public int Compare(V2MemberOrder? x, V2MemberOrder? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;
            
            if (object.ReferenceEquals(x, y))
            {
                return 0;
            }
            
            // 🔥 首先按 IssueId 排序（参考 F5BotV2 Line 40-43）
            if (x.IssueId > y.IssueId)
                return 1;
            if (x.IssueId < y.IssueId)
                return -1;

            // 🔥 然后按 Nickname.GetHashCode() 排序（参考 F5BotV2 Line 46-51）
            // 确保同名订单在一起
            int xHash = (x.Nickname ?? "").GetHashCode();
            int yHash = (y.Nickname ?? "").GetHashCode();
            if (xHash > yHash)
                return 1;
            if (xHash < yHash)
                return -1;

            // 🔥 最后按 TimeStampBet 排序（参考 F5BotV2 Line 53-56）
            if (x.TimeStampBet > y.TimeStampBet)
                return 1;
            if (x.TimeStampBet < y.TimeStampBet)
                return -1;

            return 0;
        }
    }
}
