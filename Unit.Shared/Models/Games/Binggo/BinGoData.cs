using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Unit.Shared.Models.Games.Binggo
{
    /// <summary>
    /// 宾果开奖数据（用于统计系统）
    /// 基于 BinggoLotteryData，但去掉 Id，使用 IssueId 作为主键，并添加 DayIndex 属性
    /// </summary>
    public class BinGoData : INotifyPropertyChanged
    {
        private int _issueId;
        private string _lotteryData = string.Empty;
        private DateTime _openTime;
        private int _dayIndex; // 当天第几期（1-203）
        private string _lastError = string.Empty;

        // ========================================
        // 数据库字段
        // ========================================

        /// <summary>
        /// 期号（主键，例如：114062884）
        /// </summary>
        public virtual int IssueId
        {
            get => _issueId;
            set
            {
                if (SetProperty(ref _issueId, value))
                {
                    // 期号变更时，重新计算 DayIndex
                    _dayIndex = CalculateDayIndex(_issueId);
                    OnPropertyChanged(nameof(DayIndex));
                }
            }
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
                    NotifyAllPropertiesChanged();
                }
            }
        }

        /// <summary>
        /// 开奖时间
        /// </summary>
        public DateTime OpenTime
        {
            get => _openTime;
            set
            {
                if (SetProperty(ref _openTime, value))
                {
                    OnPropertyChanged(nameof(OpenTimeString));
                    OnPropertyChanged(nameof(OpenDateString));
                }
            }
        }

        /// <summary>
        /// 当天第几期（1-203），自动计算
        /// </summary>
        public virtual int DayIndex
        {
            get => _dayIndex;
            protected set => SetProperty(ref _dayIndex, value);
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
        // 便捷属性（用于统计、走势图、算法）
        // ========================================

        /// <summary>
        /// P1 号码值
        /// </summary>
        public int P1Number => P1?.Number ?? 0;

        /// <summary>
        /// P2 号码值
        /// </summary>
        public int P2Number => P2?.Number ?? 0;

        /// <summary>
        /// P3 号码值
        /// </summary>
        public int P3Number => P3?.Number ?? 0;

        /// <summary>
        /// P4 号码值
        /// </summary>
        public int P4Number => P4?.Number ?? 0;

        /// <summary>
        /// P5 号码值
        /// </summary>
        public int P5Number => P5?.Number ?? 0;

        /// <summary>
        /// 总和号码值
        /// </summary>
        public int SumNumber => PSum?.Number ?? 0;

        /// <summary>
        /// P1 大小文本
        /// </summary>
        public string P1Size => P1?.GetSizeText() ?? "";

        /// <summary>
        /// P2 大小文本
        /// </summary>
        public string P2Size => P2?.GetSizeText() ?? "";

        /// <summary>
        /// P3 大小文本
        /// </summary>
        public string P3Size => P3?.GetSizeText() ?? "";

        /// <summary>
        /// P4 大小文本
        /// </summary>
        public string P4Size => P4?.GetSizeText() ?? "";

        /// <summary>
        /// P5 大小文本
        /// </summary>
        public string P5Size => P5?.GetSizeText() ?? "";

        /// <summary>
        /// 总和大小文本
        /// </summary>
        public string SumSize => PSum?.GetSizeText() ?? "";

        /// <summary>
        /// P1 单双文本
        /// </summary>
        public string P1OddEven => P1?.GetOddEvenText() ?? "";

        /// <summary>
        /// P2 单双文本
        /// </summary>
        public string P2OddEven => P2?.GetOddEvenText() ?? "";

        /// <summary>
        /// P3 单双文本
        /// </summary>
        public string P3OddEven => P3?.GetOddEvenText() ?? "";

        /// <summary>
        /// P4 单双文本
        /// </summary>
        public string P4OddEven => P4?.GetOddEvenText() ?? "";

        /// <summary>
        /// P5 单双文本
        /// </summary>
        public string P5OddEven => P5?.GetOddEvenText() ?? "";

        /// <summary>
        /// 总和单双文本
        /// </summary>
        public string SumOddEven => PSum?.GetOddEvenText() ?? "";

        /// <summary>
        /// P1 尾大小文本
        /// </summary>
        public string P1TailSize => P1?.GetTailSizeText() ?? "";

        /// <summary>
        /// P2 尾大小文本
        /// </summary>
        public string P2TailSize => P2?.GetTailSizeText() ?? "";

        /// <summary>
        /// P3 尾大小文本
        /// </summary>
        public string P3TailSize => P3?.GetTailSizeText() ?? "";

        /// <summary>
        /// P4 尾大小文本
        /// </summary>
        public string P4TailSize => P4?.GetTailSizeText() ?? "";

        /// <summary>
        /// P5 尾大小文本
        /// </summary>
        public string P5TailSize => P5?.GetTailSizeText() ?? "";

        /// <summary>
        /// P1 合单双文本
        /// </summary>
        public string P1SumOddEven => P1?.GetSumOddEvenText() ?? "";

        /// <summary>
        /// P2 合单双文本
        /// </summary>
        public string P2SumOddEven => P2?.GetSumOddEvenText() ?? "";

        /// <summary>
        /// P3 合单双文本
        /// </summary>
        public string P3SumOddEven => P3?.GetSumOddEvenText() ?? "";

        /// <summary>
        /// P4 合单双文本
        /// </summary>
        public string P4SumOddEven => P4?.GetSumOddEvenText() ?? "";

        /// <summary>
        /// P5 合单双文本
        /// </summary>
        public string P5SumOddEven => P5?.GetSumOddEvenText() ?? "";

        /// <summary>
        /// 龙虎文本
        /// </summary>
        public string DragonTigerText => GetDragonTigerText();

        /// <summary>
        /// 期号尾后3位（用于显示）
        /// </summary>
        public int IssueIdTail => IssueId % 1000;

        /// <summary>
        /// 期号尾后2位（用于显示）
        /// </summary>
        public int IssueIdTail2 => IssueId % 100;

        /// <summary>
        /// 开奖时间字符串（格式：yyyy-MM-dd HH:mm:ss）
        /// </summary>
        public string OpenTimeString => OpenTime.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// 开奖日期字符串（格式：yyyy-MM-dd）
        /// </summary>
        public string OpenDateString => OpenTime.ToString("yyyy-MM-dd");

        // ========================================
        // 构造函数
        // ========================================

        public BinGoData()
        {
        }

        /// <summary>
        /// 构造函数：自动计算 DayIndex
        /// </summary>
        public BinGoData(int issueId, string lotteryData, DateTime openTime)
        {
            _issueId = issueId;
            _dayIndex = CalculateDayIndex(issueId);
            _lotteryData = lotteryData;
            _openTime = openTime;
            ParseLotteryData();
        }

        // ========================================
        // 核心方法
        // ========================================

        /// <summary>
        /// 计算期号在当天是第几期（1-203）
        /// </summary>
        private static int CalculateDayIndex(int issueId)
        {
            const int FIRST_ISSUE_ID = 114000001;  // 基准期号 (2025-01-01 第1期)
            const int ISSUES_PER_DAY = 203;        // 每天203期
            
            int value = issueId - FIRST_ISSUE_ID;
            
            if (value >= 0)
            {
                // result = value % 203 + 1
                // 例如：value = 0, result = 1 (第1期)
                //      value = 202, result = 203 (第203期)
                //      value = 203, result = 1 (第2天第1期)
                return value % ISSUES_PER_DAY + 1;
            }
            else
            {
                // 处理负数（历史期号）
                int result = value % ISSUES_PER_DAY + 1;
                return ISSUES_PER_DAY - Math.Abs(result);
            }
        }

        /// <summary>
        /// 填充开奖数据（自动计算 DayIndex）
        /// </summary>
        public BinGoData FillLotteryData(int issueId, string lotteryData, DateTime openTime)
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

        /// <summary>
        /// 通知所有属性已变更（用于数据解析后）
        /// </summary>
        private void NotifyAllPropertiesChanged()
        {
            // 基础属性
            OnPropertyChanged(nameof(P1));
            OnPropertyChanged(nameof(P2));
            OnPropertyChanged(nameof(P3));
            OnPropertyChanged(nameof(P4));
            OnPropertyChanged(nameof(P5));
            OnPropertyChanged(nameof(PSum));
            OnPropertyChanged(nameof(DragonTiger));
            OnPropertyChanged(nameof(IsOpened));

            // 号码值属性
            OnPropertyChanged(nameof(P1Number));
            OnPropertyChanged(nameof(P2Number));
            OnPropertyChanged(nameof(P3Number));
            OnPropertyChanged(nameof(P4Number));
            OnPropertyChanged(nameof(P5Number));
            OnPropertyChanged(nameof(SumNumber));

            // 大小文本属性
            OnPropertyChanged(nameof(P1Size));
            OnPropertyChanged(nameof(P2Size));
            OnPropertyChanged(nameof(P3Size));
            OnPropertyChanged(nameof(P4Size));
            OnPropertyChanged(nameof(P5Size));
            OnPropertyChanged(nameof(SumSize));

            // 单双文本属性
            OnPropertyChanged(nameof(P1OddEven));
            OnPropertyChanged(nameof(P2OddEven));
            OnPropertyChanged(nameof(P3OddEven));
            OnPropertyChanged(nameof(P4OddEven));
            OnPropertyChanged(nameof(P5OddEven));
            OnPropertyChanged(nameof(SumOddEven));

            // 尾大小文本属性
            OnPropertyChanged(nameof(P1TailSize));
            OnPropertyChanged(nameof(P2TailSize));
            OnPropertyChanged(nameof(P3TailSize));
            OnPropertyChanged(nameof(P4TailSize));
            OnPropertyChanged(nameof(P5TailSize));

            // 合单双文本属性
            OnPropertyChanged(nameof(P1SumOddEven));
            OnPropertyChanged(nameof(P2SumOddEven));
            OnPropertyChanged(nameof(P3SumOddEven));
            OnPropertyChanged(nameof(P4SumOddEven));
            OnPropertyChanged(nameof(P5SumOddEven));

            // 其他属性
            OnPropertyChanged(nameof(DragonTigerText));
            OnPropertyChanged(nameof(IssueIdTail));
            OnPropertyChanged(nameof(IssueIdTail2));
            OnPropertyChanged(nameof(OpenTimeString));
            OnPropertyChanged(nameof(OpenDateString));
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

