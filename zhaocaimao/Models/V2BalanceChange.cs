using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;
using zhaocaimao.Attributes;

namespace zhaocaimao.Models
{
    /// <summary>
    /// 资金变动记录模型
    /// 记录所有会员的资金变动情况
    /// </summary>
    public class V2BalanceChange : INotifyPropertyChanged
    {
        private long _id;
        private string _groupWxId = "";
        private string? _wxid;
        private string? _nickname;
        private float _balanceBefore;  // 变动前余额
        private float _balanceAfter;   // 变动后余额
        private float _changeAmount;   // 变动金额（正数=增加，负数=减少）
        private ChangeReason _reason;  // 变动原因
        private long _relatedOrderId;  // 关联订单ID（如果是订单导致的变动）
        private string? _relatedOrderInfo;  // 关联订单信息（如：123大10）
        private string? _relatedOrderTime;  // 关联订单时间（冗余记录）
        private int _issueId;  // 期号
        private string _timeString = "";  // 时间字符串
        private long _timestamp;  // 时间戳
        private string? _notes;  // 备注

        [PrimaryKey, AutoIncrement]
        [DataGridColumn(HeaderText = "ID", Width = 50, Order = 0)]
        public long Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        [Indexed]
        [Browsable(false)]
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

        [DataGridColumn(HeaderText = "昵称", Width = 100, Order = 2)]
        public string? Nickname
        {
            get => _nickname;
            set => SetField(ref _nickname, value);
        }

        [DataGridColumn(HeaderText = "变动前", Width = 80, Order = 3, 
                        Format = "{0:F2}", Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight)]
        public float BalanceBefore
        {
            get => _balanceBefore;
            set => SetField(ref _balanceBefore, value);
        }

        [DataGridColumn(HeaderText = "变动后", Width = 80, Order = 4, 
                        Format = "{0:F2}", Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight)]
        public float BalanceAfter
        {
            get => _balanceAfter;
            set => SetField(ref _balanceAfter, value);
        }

        [DataGridColumn(HeaderText = "变动金额", Width = 80, Order = 5, 
                        Format = "{0:F2}", Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight)]
        public float ChangeAmount
        {
            get => _changeAmount;
            set => SetField(ref _changeAmount, value);
        }

        [DataGridColumn(HeaderText = "原因", Width = 100, Order = 6, 
                        Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter)]
        public ChangeReason Reason
        {
            get => _reason;
            set => SetField(ref _reason, value);
        }

        /// <summary>
        /// 变动原因文本（用于显示）
        /// </summary>
        [Browsable(false)]
        public string ReasonText => Reason switch
        {
            ChangeReason.未知 => "未知",
            ChangeReason.下注 => "下注",
            ChangeReason.订单结算 => "订单结算",
            ChangeReason.订单取消 => "订单取消",
            ChangeReason.上分 => "上分",
            ChangeReason.下分 => "下分",
            ChangeReason.清空数据 => "清空数据",
            ChangeReason.手动调整 => "手动调整",
            ChangeReason.补单 => "补单",
            _ => "未知"
        };

        [DataGridColumn(HeaderText = "关联订单ID", Width = 80, Order = 7, Visible = false)]
        public long RelatedOrderId
        {
            get => _relatedOrderId;
            set => SetField(ref _relatedOrderId, value);
        }

        [DataGridColumn(HeaderText = "订单信息", Width = 150, Order = 8)]
        public string? RelatedOrderInfo
        {
            get => _relatedOrderInfo;
            set => SetField(ref _relatedOrderInfo, value);
        }

        [DataGridColumn(HeaderText = "订单时间", Width = 140, Order = 9)]
        public string? RelatedOrderTime
        {
            get => _relatedOrderTime;
            set => SetField(ref _relatedOrderTime, value);
        }

        [DataGridColumn(HeaderText = "期号", Width = 90, Order = 10, 
                        Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter)]
        public int IssueId
        {
            get => _issueId;
            set => SetField(ref _issueId, value);
        }

        [DataGridColumn(HeaderText = "变动时间", Width = 140, Order = 11)]
        public string TimeString
        {
            get => _timeString;
            set => SetField(ref _timeString, value);
        }

        [Browsable(false)]
        public long Timestamp
        {
            get => _timestamp;
            set => SetField(ref _timestamp, value);
        }

        [DataGridColumn(HeaderText = "备注", Width = 200, Order = 12)]
        public string? Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        // ========================================
        // INotifyPropertyChanged 实现
        // ========================================

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 资金变动原因枚举
    /// </summary>
    public enum ChangeReason
    {
        未知 = 0,
        下注 = 1,        // 会员下注（余额减少）
        订单结算 = 2,    // 订单结算（余额增加或减少）
        订单取消 = 3,    // 订单取消（退回余额）
        上分 = 4,        // 充值/上分（余额增加）
        下分 = 5,        // 提现/下分（余额减少）
        清空数据 = 6,    // 清空数据（余额归零）
        手动调整 = 7,    // 管理员手动调整
        补单 = 8         // 补单
    }
}

