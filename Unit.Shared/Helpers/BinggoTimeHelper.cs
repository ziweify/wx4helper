using System;

namespace Unit.Shared.Helpers
{
    /// <summary>
    /// Binggo 时间计算辅助类（共享库版本）
    /// 
    /// 功能：期号与时间的相互转换
    /// 适用于：通宝28、Binggo等5分钟一期的游戏
    /// </summary>
    public static class BinggoTimeHelper
    {
        // 核心常量
        private const int ISSUES_PER_DAY = 203;           // 每天期数
        private const int FIRST_ISSUE_ID = 115000001;     // 基准期号 (2026-01-01 第1期)
        private const long FIRST_TIMESTAMP = 1767222300;  // 基准时间戳 (2026-01-01 07:05:00)
        private const int MINUTES_PER_ISSUE = 5;          // 每期间隔（分钟）
        
        /// <summary>
        /// 获取指定时间的当前期号（完全参考 F5BotV2 的 getNextIssueId）
        /// </summary>
        /// <param name="time">查询时间（默认为当前时间）</param>
        /// <returns>得到该时间的前一期数据, 也就该时间的最后一次开奖期号</returns>
        public static int GetCurrentIssueId(DateTime? time = null)
        {
            var currentTime = time ?? DateTime.Now;
            var firstTime = DateTimeOffset.FromUnixTimeSeconds(FIRST_TIMESTAMP).LocalDateTime;
            
            // 计算天数差
            var timeSpan = currentTime - firstTime;
            var days = timeSpan.Days;
            
            // 当天的基础期号
            int baseDayIssueId = FIRST_ISSUE_ID + days * ISSUES_PER_DAY;
            
            // 🔥 关键：计算当天已经过了多少期（参考 F5BotV2 逻辑）
            int issueCount = 0;
            for (int i = 0; i < ISSUES_PER_DAY; i++)
            {
                var issueTimestamp = GetIssueOpenTimestamp(baseDayIssueId + i);
                var issueTime = DateTimeOffset.FromUnixTimeSeconds(issueTimestamp).LocalDateTime;
                
                // 🔥 关键判断：如果当前时间 > 该期开奖时间，说明该期已过
                if (currentTime > issueTime)
                {
                    issueCount++;
                }
                else
                {
                    break;
                }
            }
            
            return baseDayIssueId + issueCount;
        }
        
        /// <summary>
        /// 根据期号计算开奖时间戳（完全参考 F5BotV2 的 getOpenTimestamp）
        /// </summary>
        /// <param name="issueId">期号</param>
        /// <returns>Unix 时间戳（秒）</returns>
        public static long GetIssueOpenTimestamp(int issueId)
        {
            var firstTime = DateTimeOffset.FromUnixTimeSeconds(FIRST_TIMESTAMP).LocalDateTime;
            
            // 计算天数差
            int days = GetDaysDiff(issueId);
            
            // 计算当天第几期（1-203）
            int number = GetIssueNumber(issueId);
            
            // 计算开奖时间
            var nowDay = firstTime.AddDays(days);
            var openTime = nowDay.AddMinutes(MINUTES_PER_ISSUE * (number - 1));
            
            // 转换为 Unix 时间戳
            return new DateTimeOffset(openTime).ToUnixTimeSeconds();
        }
        
        /// <summary>
        /// 根据期号计算开奖时间（完全参考 F5BotV2 的 getOpenDatetime）
        /// </summary>
        /// <param name="issueId">期号</param>
        /// <returns>开奖时间</returns>
        public static DateTime GetIssueOpenTime(int issueId)
        {
            long timestamp = GetIssueOpenTimestamp(issueId);
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
        }
        
        /// <summary>
        /// 计算距离指定期号开奖还有多少秒
        /// </summary>
        /// <param name="issueId">期号</param>
        /// <param name="currentTime">当前时间（默认为当前时间）</param>
        /// <returns>剩余秒数（负数表示已开奖）</returns>
        public static int GetSecondsToOpen(int issueId, DateTime? currentTime = null)
        {
            var now = currentTime ?? DateTime.Now;
            var openTime = GetIssueOpenTime(issueId);
            var seconds = (int)(openTime - now).TotalSeconds;
            return seconds;
        }
        
        /// <summary>
        /// 计算距离封盘还有多少秒
        /// </summary>
        /// <param name="issueId">期号</param>
        /// <param name="sealSecondsBeforeOpen">提前封盘秒数（默认45秒）</param>
        /// <param name="currentTime">当前时间（默认为当前时间）</param>
        /// <returns>剩余秒数（负数表示已封盘）</returns>
        public static int GetSecondsToSeal(int issueId, int sealSecondsBeforeOpen = 45, DateTime? currentTime = null)
        {
            var secondsToOpen = GetSecondsToOpen(issueId, currentTime);
            return secondsToOpen - sealSecondsBeforeOpen;
        }
        
        /// <summary>
        /// 获取期号在当天是第几期（1-203）
        /// 🔥 完全参考 F5BotV2 的 getNumber 方法
        /// </summary>
        public static int GetIssueNumber(int issueId)
        {
            int result = 0;
            int value = issueId - FIRST_ISSUE_ID;
            
            if (value >= 0)
            {
                // 🔥 关键：result = value % 203 + 1
                // 例如：value = 0, result = 1 (第1期)
                //      value = 202, result = 203 (第203期)
                //      value = 203, result = 1 (第2天第1期)
                result = value % ISSUES_PER_DAY + 1;
            }
            else
            {
                // 处理负数（历史期号）
                result = ((value % ISSUES_PER_DAY) + ISSUES_PER_DAY) % ISSUES_PER_DAY + 1;
            }
            
            return result;
        }
        
        /// <summary>
        /// 计算期号距离第一期的天数差
        /// </summary>
        private static int GetDaysDiff(int issueId)
        {
            int value = issueId - FIRST_ISSUE_ID;
            return value / ISSUES_PER_DAY;
        }
        
        /// <summary>
        /// 获取上一期期号
        /// </summary>
        public static int GetPreviousIssueId(int issueId)
        {
            return issueId - 1;
        }
        
        /// <summary>
        /// 获取下一期期号
        /// </summary>
        public static int GetNextIssueId(int issueId)
        {
            return issueId + 1;
        }
        
        /// <summary>
        /// 格式化倒计时（秒数转为 mm:ss 格式）
        /// </summary>
        public static string FormatCountdown(int seconds)
        {
            if (seconds < 0) return "00:00";
            
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            return $"{minutes:D2}:{remainingSeconds:D2}";
        }
        
        /// <summary>
        /// 格式化期号（114070636 → 114-070636）
        /// </summary>
        public static string FormatIssueId(int issueId)
        {
            var str = issueId.ToString();
            if (str.Length >= 7)
            {
                return $"{str.Substring(0, 3)}-{str.Substring(3)}";
            }
            return str;
        }
    }
}

