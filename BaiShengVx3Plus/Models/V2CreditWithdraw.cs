using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;
using BaiShengVx3Plus.Attributes;

namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 上下分申请记录模型（参考 F5BotV2 V2MemberCoinsBuySell）
    /// </summary>
    public class V2CreditWithdraw : INotifyPropertyChanged
    {
        private long _id;
        private string _groupWxId = "";
        private string? _wxid;
        private string? _nickname;
        private string? _account;
        private CreditWithdrawAction _action;  // 上分/下分
        private float _amount;  // 金额
        private CreditWithdrawStatus _status;  // 状态
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
        [Browsable(false)]
        public string? Wxid
        {
            get => _wxid;
            set => SetField(ref _wxid, value);
        }

        [DataGridColumn(HeaderText = "会员名", Width = 120, Order = 1)]
        public string? Nickname
        {
            get => _nickname;
            set => SetField(ref _nickname, value);
        }

        [Browsable(false)]
        public string? Account
        {
            get => _account;
            set => SetField(ref _account, value);
        }

        [DataGridColumn(HeaderText = "动作", Width = 80, Order = 2, 
                        Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter)]
        public CreditWithdrawAction Action
        {
            get => _action;
            set
            {
                if (SetField(ref _action, value))
                {
                    // 🔥 当 Action 改变时，通知 ActionText 也改变了
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionText)));
                }
            }
        }

        [DataGridColumn(HeaderText = "金额", Width = 100, Order = 3, 
                        Format = "{0:F2}", Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight)]
        public float Amount
        {
            get => _amount;
            set => SetField(ref _amount, value);
        }

        [DataGridColumn(HeaderText = "申请时间", Width = 140, Order = 4)]
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

        [DataGridColumn(HeaderText = "状态", Width = 80, Order = 5, 
                        Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter)]
        public CreditWithdrawStatus Status
        {
            get => _status;
            set
            {
                if (SetField(ref _status, value))
                {
                    // 🔥 状态变化时，通知 ActionText 也变化（因为 ActionText 依赖于 Status）
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionText)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
                }
            }
        }

        [DataGridColumn(HeaderText = "备注", Width = 200, Order = 6)]
        public string? Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        // 🔥 新增字段：处理人和处理时间（参考 F5BotV2）
        private string? _processedBy;
        private string? _processedTime;

        [DataGridColumn(HeaderText = "处理人", Width = 90, Order = 7)]
        public string? ProcessedBy
        {
            get => _processedBy;
            set => SetField(ref _processedBy, value);
        }

        [DataGridColumn(HeaderText = "处理时间", Width = 140, Order = 8)]
        public string? ProcessedTime
        {
            get => _processedTime;
            set => SetField(ref _processedTime, value);
        }

        // 🔥 辅助属性：动作文本（显示动作和处理结果）
        [Browsable(false)]
        public string ActionText
        {
            get
            {
                string actionText = Action switch
                {
                    CreditWithdrawAction.上分 => "上分",
                    CreditWithdrawAction.下分 => "下分",
                    _ => "未知"
                };
                
                // 🔥 如果已处理，显示处理结果（参考 F5BotV2）
                if (Status == CreditWithdrawStatus.已同意)
                {
                    return $"{actionText}-已同意";
                }
                else if (Status == CreditWithdrawStatus.已拒绝)
                {
                    return $"{actionText}-已拒绝";
                }
                else if (Status == CreditWithdrawStatus.忽略)
                {
                    return $"{actionText}-已忽略";
                }
                
                return actionText;
            }
        }

        // 🔥 辅助属性：状态文本（参考 F5BotV2）
        [Browsable(false)]
        public string StatusText => Status switch
        {
            CreditWithdrawStatus.等待处理 => "等待处理",
            CreditWithdrawStatus.已同意 => "已同意",
            CreditWithdrawStatus.已拒绝 => "已拒绝",
            CreditWithdrawStatus.忽略 => "忽略",
            _ => "未知"
        };

        // ========================================
        // INotifyPropertyChanged 实现
        // ========================================

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }

    /// <summary>
    /// 上下分动作枚举
    /// </summary>
    public enum CreditWithdrawAction
    {
        未知 = 0,
        上分 = 1,  // 充值
        下分 = 2   // 提现
    }

    /// <summary>
    /// 上下分状态枚举（参考 F5BotV2 V2MemberPayStatus）
    /// </summary>
    public enum CreditWithdrawStatus
    {
        等待处理 = 0,
        已同意 = 1,
        已拒绝 = 2,
        忽略 = 3  // 🔥 参考 F5BotV2 Line 1526-1542
    }
}

