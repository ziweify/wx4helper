using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BaiShengVx3Plus.Shared.Models.Games.Binggo
{
    /// <summary>
    /// 宾果开奖数据（标准化模型）
    /// 用于统计系统的标准数据源
    /// </summary>
    public class BinggoLotteryData : INotifyPropertyChanged
    {
        private int _id;
        private int _issueId;
        private string _lotteryData = string.Empty;
        private DateTime _openTime;
        private string _lastError = string.Empty;

        // ========================================
        // 数据库字段
        // ========================================

        /// <summary>
        /// 主键 ID
        /// </summary>
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// 期号（例如：114062884）
        /// </summary>
        public int IssueId
        {
            get => _issueId;
            set => SetProperty(ref _issueId, value);
        }

        /// <summary>
        /// 开奖号码字符串（格式："7,14,21,8,2"）
        /// </summary>
        public string LotteryData
        {
            get => _lotteryData;
            set
            {
                if (SetProperty(ref _lotteryData, value))
                {
                    // 号码变更后，重新解析并通知所有计算属性
                    ParseLotteryData();
                    OnPropertyChanged(nameof(P1));
                    OnPropertyChanged(nameof(P2));
                    OnPropertyChanged(nameof(P3));
                    OnPropertyChanged(nameof(P4));
                    OnPropertyChanged(nameof(P5));
                    OnPropertyChanged(nameof(PSum));
                    OnPropertyChanged(nameof(DragonTiger));
                    OnPropertyChanged(nameof(IsOpened));
                }
            }
        }

        /// <summary>
        /// 开奖时间
        /// </summary>
        public DateTime OpenTime
        {
            get => _openTime;
            set => SetProperty(ref _openTime, value);
        }

        /// <summary>
        /// 最后一次错误信息
        /// </summary>
        public string LastError
        {
            get => _lastError;
            set => SetProperty(ref _lastError, value);
        }

        // ========================================
        // 计算属性（不存储到数据库）
        // ========================================

        /// <summary>
        /// 号码列表（解析后的 LotteryNumber 对象）
        /// </summary>
        public List<LotteryNumber> Items { get; private set; } = new List<LotteryNumber>();

        /// <summary>
        /// 第1球
        /// </summary>
        public LotteryNumber? P1 { get; private set; }

        /// <summary>
        /// 第2球
        /// </summary>
        public LotteryNumber? P2 { get; private set; }

        /// <summary>
        /// 第3球
        /// </summary>
        public LotteryNumber? P3 { get; private set; }

        /// <summary>
        /// 第4球
        /// </summary>
        public LotteryNumber? P4 { get; private set; }

        /// <summary>
        /// 第5球
        /// </summary>
        public LotteryNumber? P5 { get; private set; }

        /// <summary>
        /// 总和
        /// </summary>
        public LotteryNumber? PSum { get; private set; }

        /// <summary>
        /// 龙虎
        /// </summary>
        public DragonTigerType DragonTiger { get; private set; } = DragonTigerType.Unknown;

        /// <summary>
        /// 是否已开奖
        /// </summary>
        public bool IsOpened => !string.IsNullOrEmpty(LotteryData) && Items.Count >= 5;

        // ========================================
        // 核心方法
        // ========================================

        /// <summary>
        /// 填充开奖数据
        /// </summary>
        public BinggoLotteryData FillLotteryData(int issueId, string lotteryData, DateTime openTime)
        {
            try
            {
                IssueId = issueId;
                LotteryData = lotteryData;
                OpenTime = openTime;
                return this;
            }
            catch (Exception ex)
            {
                LastError = $"issueId={issueId}, lotteryData={lotteryData}, openTime={openTime}, msg={ex.Message}";
                return this;
            }
        }

        /// <summary>
        /// 解析开奖号码字符串
        /// </summary>
        private void ParseLotteryData()
        {
            try
            {
                Items.Clear();
                P1 = P2 = P3 = P4 = P5 = PSum = null;
                DragonTiger = DragonTigerType.Unknown;

                if (string.IsNullOrEmpty(LotteryData))
                    return;

                string[] data = LotteryData.Split(',');
                if (data.Length < 5)
                    return;

                // 解析 P1-P5
                for (int i = 0; i < 5; i++)
                {
                    if (int.TryParse(data[i].Trim(), out int number))
                    {
                        Items.Add(new LotteryNumber((BallPosition)(i + 1), number));
                    }
                }

                if (Items.Count == 5)
                {
                    P1 = Items[0];
                    P2 = Items[1];
                    P3 = Items[2];
                    P4 = Items[3];
                    P5 = Items[4];

                    // 计算总和
                    int sum = P1.Number + P2.Number + P3.Number + P4.Number + P5.Number;
                    PSum = new LotteryNumber(BallPosition.Sum, sum);
                    Items.Add(PSum);

                    // 计算龙虎
                    if (P1.Number > P5.Number)
                    {
                        DragonTiger = DragonTigerType.Dragon;
                    }
                    else if (P1.Number < P5.Number)
                    {
                        DragonTiger = DragonTigerType.Tiger;
                    }
                    else
                    {
                        DragonTiger = DragonTigerType.Draw;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = $"解析号码失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 根据位置获取号码
        /// </summary>
        public LotteryNumber? GetBallNumber(BallPosition position)
        {
            return position switch
            {
                BallPosition.P1 => P1,
                BallPosition.P2 => P2,
                BallPosition.P3 => P3,
                BallPosition.P4 => P4,
                BallPosition.P5 => P5,
                BallPosition.Sum => PSum,
                _ => null
            };
        }

        /// <summary>
        /// 转换为开奖字符串
        /// </summary>
        public string ToLotteryString()
        {
            try
            {
                if (P1 == null || P2 == null || P3 == null || P4 == null || P5 == null || PSum == null)
                    return "0,0,0,0,0 * * *";

                return $"{P1.Number},{P2.Number},{P3.Number},{P4.Number},{P5.Number} " +
                       $"{PSum.GetSizeText()}{PSum.GetOddEvenText()} " +
                       $"{GetDragonTigerText()}";
            }
            catch
            {
                return "0,0,0,0,0 * * *";
            }
        }

        /// <summary>
        /// 获取龙虎文本
        /// </summary>
        public string GetDragonTigerText()
        {
            return DragonTiger switch
            {
                DragonTigerType.Dragon => "龙",
                DragonTigerType.Tiger => "虎",
                DragonTigerType.Draw => "和",
                _ => "?"
            };
        }

        // ========================================
        // INotifyPropertyChanged 实现
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

