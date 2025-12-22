using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace 永利系统.Models.Games.Bingo
{
    /// <summary>
    /// 开奖数据模型（用于数据绑定）
    /// </summary>
    public class LotteryData : INotifyPropertyChanged
    {
        private int _issueId;
        private string? _lotteryNumber;
        private DateTime _lotteryTime;
        private DateTime _openTime;
        private DateTime _sealTime;
        private LotteryStatus _status;
        private int _secondsToSeal;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        /// <summary>
        /// 期号
        /// </summary>
        // [Indexed] // TODO: 添加 SQLite 特性
        public int IssueId
        {
            get => _issueId;
            set => SetField(ref _issueId, value);
        }

        /// <summary>
        /// 开奖号码
        /// </summary>
        public string? LotteryNumber
        {
            get => _lotteryNumber;
            set => SetField(ref _lotteryNumber, value);
        }

        /// <summary>
        /// 开奖时间
        /// </summary>
        public DateTime LotteryTime
        {
            get => _lotteryTime;
            set => SetField(ref _lotteryTime, value);
        }

        /// <summary>
        /// 开盘时间
        /// </summary>
        public DateTime OpenTime
        {
            get => _openTime;
            set => SetField(ref _openTime, value);
        }

        /// <summary>
        /// 封盘时间
        /// </summary>
        public DateTime SealTime
        {
            get => _sealTime;
            set => SetField(ref _sealTime, value);
        }

        /// <summary>
        /// 状态
        /// </summary>
        public LotteryStatus Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        /// <summary>
        /// 距离封盘秒数
        /// </summary>
        public int SecondsToSeal
        {
            get => _secondsToSeal;
            set => SetField(ref _secondsToSeal, value);
        }
        
        /// <summary>
        /// 是否已开奖（用于判断号码是否可见）
        /// </summary>
        [Ignore]
        public bool IsOpened => !string.IsNullOrEmpty(LotteryNumber);

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
}

