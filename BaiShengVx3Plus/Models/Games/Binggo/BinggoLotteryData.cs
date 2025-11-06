using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SQLite;

namespace BaiShengVx3Plus.Models.Games.Binggo
{
    /// <summary>
    /// 炳狗开奖数据
    /// 
    /// 存储每一期的开奖号码、统计信息等
    /// </summary>
    [Table("BinggoLotteryData")]
    public class BinggoLotteryData : INotifyPropertyChanged
    {
        private int _id;
        private int _issueId;
        private string _numbersString = string.Empty;
        private DateTime _issueStartTime;
        private DateTime? _openTime;
        
        /// <summary>
        /// 主键 ID
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        
        /// <summary>
        /// 期号 (例如：20251106001)
        /// </summary>
        [Indexed]
        public int IssueId
        {
            get => _issueId;
            set => SetProperty(ref _issueId, value);
        }
        
        /// <summary>
        /// 开奖号码字符串 (格式："1,2,3,4,5")
        /// </summary>
        public string NumbersString
        {
            get => _numbersString;
            set
            {
                if (SetProperty(ref _numbersString, value))
                {
                    // 号码变更后，通知所有计算属性
                    OnPropertyChanged(nameof(Numbers));
                    OnPropertyChanged(nameof(P1));
                    OnPropertyChanged(nameof(P2));
                    OnPropertyChanged(nameof(P3));
                    OnPropertyChanged(nameof(P4));
                    OnPropertyChanged(nameof(P5));
                    OnPropertyChanged(nameof(Sum));
                    OnPropertyChanged(nameof(BigSmall));
                    OnPropertyChanged(nameof(OddEven));
                    OnPropertyChanged(nameof(DragonTiger));
                }
            }
        }
        
        /// <summary>
        /// 期号开始时间
        /// </summary>
        public DateTime IssueStartTime
        {
            get => _issueStartTime;
            set => SetProperty(ref _issueStartTime, value);
        }
        
        /// <summary>
        /// 开奖时间
        /// </summary>
        public DateTime? OpenTime
        {
            get => _openTime;
            set => SetProperty(ref _openTime, value);
        }
        
        // ========================================
        // 🔥 计算属性 (不存储到数据库)
        // ========================================
        
        /// <summary>
        /// 开奖号码数组
        /// </summary>
        [Ignore]
        public int[] Numbers
        {
            get
            {
                if (string.IsNullOrEmpty(NumbersString))
                    return Array.Empty<int>();
                
                try
                {
                    return NumbersString.Split(',')
                        .Select(s => int.TryParse(s.Trim(), out int n) ? n : 0)
                        .ToArray();
                }
                catch
                {
                    return Array.Empty<int>();
                }
            }
        }
        
        /// <summary>
        /// 第1球
        /// </summary>
        [Ignore]
        public int P1 => Numbers.Length > 0 ? Numbers[0] : 0;
        
        /// <summary>
        /// 第2球
        /// </summary>
        [Ignore]
        public int P2 => Numbers.Length > 1 ? Numbers[1] : 0;
        
        /// <summary>
        /// 第3球
        /// </summary>
        [Ignore]
        public int P3 => Numbers.Length > 2 ? Numbers[2] : 0;
        
        /// <summary>
        /// 第4球
        /// </summary>
        [Ignore]
        public int P4 => Numbers.Length > 3 ? Numbers[3] : 0;
        
        /// <summary>
        /// 第5球
        /// </summary>
        [Ignore]
        public int P5 => Numbers.Length > 4 ? Numbers[4] : 0;
        
        /// <summary>
        /// 总和
        /// </summary>
        [Ignore]
        public int Sum => P1 + P2 + P3 + P4 + P5;
        
        /// <summary>
        /// 大小 (总和 >= 15 为大，< 15 为小)
        /// </summary>
        [Ignore]
        public string BigSmall => Sum >= 15 ? "大" : "小";
        
        /// <summary>
        /// 单双 (总和为奇数=单，偶数=双)
        /// </summary>
        [Ignore]
        public string OddEven => Sum % 2 == 0 ? "双" : "单";
        
        /// <summary>
        /// 龙虎 (P1 > P5 为龙，P1 < P5 为虎，P1 == P5 为和)
        /// </summary>
        [Ignore]
        public string DragonTiger
        {
            get
            {
                if (P1 > P5) return "龙";
                if (P1 < P5) return "虎";
                return "和";
            }
        }
        
        /// <summary>
        /// 是否已开奖
        /// </summary>
        [Ignore]
        public bool IsOpened => !string.IsNullOrEmpty(NumbersString) && Numbers.Length == 5;
        
        // ========================================
        // 🔥 INotifyPropertyChanged 实现
        // ========================================
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

