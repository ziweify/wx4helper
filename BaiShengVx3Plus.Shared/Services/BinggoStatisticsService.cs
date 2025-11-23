using System;
using System.Collections.Generic;
using System.Linq;
using BaiShengVx3Plus.Shared.Models.Games.Binggo;
using BaiShengVx3Plus.Shared.Models.Games.Binggo.Statistics;

namespace BaiShengVx3Plus.Shared.Services
{
    /// <summary>
    /// 宾果统计服务
    /// 负责计算各种统计数据
    /// </summary>
    public class BinggoStatisticsService
    {
        private readonly List<BinggoLotteryData> _dataList = new List<BinggoLotteryData>();

        /// <summary>
        /// 添加开奖数据
        /// </summary>
        public void AddData(BinggoLotteryData data)
        {
            if (data != null && data.IsOpened)
            {
                _dataList.Add(data);
                // 按时间排序
                _dataList.Sort((a, b) => a.OpenTime.CompareTo(b.OpenTime));
            }
        }

        /// <summary>
        /// 批量添加开奖数据
        /// </summary>
        public void AddDataRange(IEnumerable<BinggoLotteryData> dataList)
        {
            foreach (var data in dataList)
            {
                AddData(data);
            }
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            _dataList.Clear();
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        public IReadOnlyList<BinggoLotteryData> GetAllData()
        {
            return _dataList.AsReadOnly();
        }

        /// <summary>
        /// 获取指定位置和玩法的路珠数据
        /// </summary>
        public List<PositionPlayResult> GetRoadBeadData(BallPosition position, GamePlayType playType, int? limit = null)
        {
            var results = new List<PositionPlayResult>();
            var data = limit.HasValue ? _dataList.TakeLast(limit.Value) : _dataList;

            foreach (var lotteryData in data)
            {
                var ball = lotteryData.GetBallNumber(position);
                if (ball == null) continue;

                PlayResult result = playType switch
                {
                    GamePlayType.Size => ball.Size == SizeType.Big ? PlayResult.Big : PlayResult.Small,
                    GamePlayType.OddEven => ball.OddEven == OddEvenType.Odd ? PlayResult.Odd : PlayResult.Even,
                    GamePlayType.TailSize => ball.TailSize == TailSizeType.TailBig ? PlayResult.TailBig : PlayResult.TailSmall,
                    GamePlayType.SumOddEven => ball.SumOddEven == SumOddEvenType.SumOdd ? PlayResult.SumOdd : PlayResult.SumEven,
                    GamePlayType.DragonTiger => position == BallPosition.Sum
                        ? (lotteryData.DragonTiger == DragonTigerType.Dragon ? PlayResult.Dragon
                           : lotteryData.DragonTiger == DragonTigerType.Tiger ? PlayResult.Tiger
                           : PlayResult.Draw)
                        : PlayResult.Unknown,
                    _ => PlayResult.Unknown
                };

                if (result != PlayResult.Unknown)
                {
                    results.Add(new PositionPlayResult(position, playType, result));
                }
            }

            return results;
        }

        /// <summary>
        /// 获取连续统计
        /// </summary>
        public List<ConsecutiveStats> GetConsecutiveStats(BallPosition position, GamePlayType playType)
        {
            var roadBeadData = GetRoadBeadData(position, playType);
            var stats = new Dictionary<int, int>(); // 连续次数 -> 出现次数

            if (roadBeadData.Count == 0) return new List<ConsecutiveStats>();

            // 统计连续出现
            int currentConsecutive = 1;
            PlayResult currentResult = roadBeadData[0].Result;

            for (int i = 1; i < roadBeadData.Count; i++)
            {
                if (roadBeadData[i].Result == currentResult)
                {
                    currentConsecutive++;
                }
                else
                {
                    // 记录当前连续
                    if (currentConsecutive >= 2)
                    {
                        int count = currentConsecutive > 11 ? 11 : currentConsecutive;
                        stats[count] = stats.GetValueOrDefault(count, 0) + 1;
                    }
                    currentConsecutive = 1;
                    currentResult = roadBeadData[i].Result;
                }
            }

            // 处理最后一组
            if (currentConsecutive >= 2)
            {
                int count = currentConsecutive > 11 ? 11 : currentConsecutive;
                stats[count] = stats.GetValueOrDefault(count, 0) + 1;
            }

            // 转换为结果列表
            var result = new List<ConsecutiveStats>();
            var resultType = playType switch
            {
                GamePlayType.Size => PlayResult.Big,
                GamePlayType.OddEven => PlayResult.Odd,
                GamePlayType.TailSize => PlayResult.TailBig,
                GamePlayType.SumOddEven => PlayResult.SumOdd,
                GamePlayType.DragonTiger => PlayResult.Dragon,
                _ => PlayResult.Unknown
            };

            // 统计所有连续次数（2-11, 11+）
            for (int i = 2; i <= 11; i++)
            {
                result.Add(new ConsecutiveStats
                {
                    Position = position,
                    PlayType = playType,
                    ResultType = resultType,
                    ConsecutiveCount = i,
                    OccurrenceCount = stats.GetValueOrDefault(i, 0)
                });
            }

            // 11+ 统计
            int count11Plus = stats.Where(kv => kv.Key > 11).Sum(kv => kv.Value);
            result.Add(new ConsecutiveStats
            {
                Position = position,
                PlayType = playType,
                ResultType = resultType,
                ConsecutiveCount = 12, // 用12表示11+
                OccurrenceCount = count11Plus
            });

            return result;
        }

        /// <summary>
        /// 获取走势数据点（按时间段分组）
        /// </summary>
        public List<TrendDataPoint> GetTrendData(TimeSpan period, int? limit = null)
        {
            if (_dataList.Count == 0) return new List<TrendDataPoint>();

            var data = limit.HasValue ? _dataList.TakeLast(limit.Value) : _dataList;
            var grouped = data.GroupBy(d => GetPeriodKey(d.OpenTime, period))
                .OrderBy(g => g.Key)
                .ToList();

            var result = new List<TrendDataPoint>();

            foreach (var group in grouped)
            {
                var point = new TrendDataPoint
                {
                    Time = group.Key,
                    IssueId = group.First().IssueId
                };

                foreach (var lotteryData in group)
                {
                    // 统计各个位置和玩法
                    for (int pos = 1; pos <= 5; pos++)
                    {
                        var ball = lotteryData.GetBallNumber((BallPosition)pos);
                        if (ball == null) continue;

                        if (ball.Size == SizeType.Big) point.BigCount++;
                        else point.SmallCount++;

                        if (ball.OddEven == OddEvenType.Odd) point.OddCount++;
                        else point.EvenCount++;

                        if (ball.TailSize == TailSizeType.TailBig) point.TailBigCount++;
                        else point.TailSmallCount++;

                        if (ball.SumOddEven == SumOddEvenType.SumOdd) point.SumOddCount++;
                        else point.SumEvenCount++;
                    }

                    // 龙虎统计
                    if (lotteryData.DragonTiger == DragonTigerType.Dragon) point.DragonCount++;
                    else if (lotteryData.DragonTiger == DragonTigerType.Tiger) point.TigerCount++;
                }

                result.Add(point);
            }

            return result;
        }

        /// <summary>
        /// 获取指定期数的走势数据
        /// </summary>
        public List<TrendDataPoint> GetTrendDataByCount(int count)
        {
            var data = _dataList.TakeLast(count).ToList();
            var result = new List<TrendDataPoint>();

            foreach (var lotteryData in data)
            {
                var point = new TrendDataPoint
                {
                    Time = lotteryData.OpenTime,
                    IssueId = lotteryData.IssueId
                };

                // 统计各个位置和玩法
                for (int pos = 1; pos <= 5; pos++)
                {
                    var ball = lotteryData.GetBallNumber((BallPosition)pos);
                    if (ball == null) continue;

                    if (ball.Size == SizeType.Big) point.BigCount++;
                    else point.SmallCount++;

                    if (ball.OddEven == OddEvenType.Odd) point.OddCount++;
                    else point.EvenCount++;

                    if (ball.TailSize == TailSizeType.TailBig) point.TailBigCount++;
                    else point.TailSmallCount++;

                    if (ball.SumOddEven == SumOddEvenType.SumOdd) point.SumOddCount++;
                    else point.SumEvenCount++;
                }

                // 龙虎统计
                if (lotteryData.DragonTiger == DragonTigerType.Dragon) point.DragonCount++;
                else if (lotteryData.DragonTiger == DragonTigerType.Tiger) point.TigerCount++;

                result.Add(point);
            }

            return result;
        }

        /// <summary>
        /// 获取时间段键值（用于分组）
        /// </summary>
        private DateTime GetPeriodKey(DateTime time, TimeSpan period)
        {
            if (period.TotalDays >= 1)
            {
                // 按天
                return new DateTime(time.Year, time.Month, time.Day);
            }
            else if (period.TotalHours >= 1)
            {
                // 按小时
                return new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
            }
            else
            {
                // 按分钟
                return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
            }
        }

        /// <summary>
        /// 获取数量统计（某个位置某个玩法的数量）
        /// </summary>
        public Dictionary<PlayResult, int> GetCountStats(BallPosition position, GamePlayType playType, int? limit = null)
        {
            var roadBeadData = GetRoadBeadData(position, playType, limit);
            var stats = new Dictionary<PlayResult, int>();

            foreach (var result in roadBeadData)
            {
                stats[result.Result] = stats.GetValueOrDefault(result.Result, 0) + 1;
            }

            return stats;
        }
    }
}

