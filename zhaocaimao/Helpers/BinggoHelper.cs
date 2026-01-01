using zhaocaimao.Models.Games.Binggo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace zhaocaimao.Helpers
{
    /// <summary>
    /// 炳狗游戏辅助类
    /// 
    /// 功能：
    /// 1. 时间和期号计算
    /// 2. 解析下注文本
    /// 3. 判断中奖逻辑
    /// 4. 计算盈利
    /// 
    /// 参考: F5BotV2/Boter/BoterBetContent.cs
    /// </summary>
    public static class BinggoHelper
    {
        // ========================================
        // 🔥 第一部分：时间和期号计算（原 BinggoTimeHelper）
        // ========================================
        
        // 核心常量
        private const int ISSUES_PER_DAY = 203;           // 每天期数
        private const int FIRST_ISSUE_ID = 115000001;     // 基准期号 (2025-12-31 第1期)
        private const long FIRST_TIMESTAMP = 1767222300;  // 基准时间戳 (2025-12-31 07:05:00)
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
                result = value % ISSUES_PER_DAY + 1;
                result = ISSUES_PER_DAY - Math.Abs(result);
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取上一期期号
        /// </summary>
        public static int GetPreviousIssueId(int issueId)
        {
            return issueId - 1;
        }
        
        /// <summary>
        /// 获取指定期号往前推 n 期的期号
        /// </summary>
        public static int GetPreviousIssueId(int issueId, int offset)
        {
            return issueId - offset;
        }
        
        /// <summary>
        /// 获取下一期期号
        /// </summary>
        public static int GetNextIssueId(int issueId)
        {
            return issueId + 1;
        }
        
        /// <summary>
        /// 格式化倒计时显示（MM:SS）
        /// </summary>
        public static string FormatCountdown(int seconds)
        {
            if (seconds < 0) return "00:00";
            
            int minutes = seconds / 60;
            int secs = seconds % 60;
            return $"{minutes:D2}:{secs:D2}";
        }
        
        /// <summary>
        /// 格式化期号显示（只显示后3位）
        /// </summary>
        public static string FormatIssueId(int issueId)
        {
            return (issueId % 1000).ToString("D3");
        }
        
        /// <summary>
        /// 计算期号相对于基准日期的天数差
        /// </summary>
        private static int GetDaysDiff(int issueId)
        {
            return (issueId - FIRST_ISSUE_ID) / ISSUES_PER_DAY;
        }
        
        /// <summary>
        /// 🔥 获取期号的日索引（参考 F5BotV2: BinGouHelper.getNumber）
        /// 返回当天是第几期（1-203）
        /// </summary>
        public static int GetDayIndex(int issueId)
        {
            // 直接使用 GetIssueNumber 方法
            return GetIssueNumber(issueId);
        }
        
        // ========================================
        // 🔥 第二部分：下注解析和结算
        // ========================================
        
        /// <summary>
        /// 解析下注内容
        /// 
        /// 支持格式:
        /// - "123大100" => P1大100, P2大100, P3大100
        /// - "1大50,2小60" => P1大50, P2小60
        /// - "123大4小5单龙100" => P1大100, P2大100, P3大100, P4小100, P5单100, 龙100
        /// - "总和大100" 或 "总大100" => P6(总和)大100
        /// - "龙100" => 龙100
        /// - "一二三大100" => P1大100, P2大100, P3大100
        /// </summary>
        public static BinggoBetContent ParseBetContent(string message, int issueId)
        {
            var result = new BinggoBetContent(issueId, message);
            
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    result.ErrorMessage = "下注内容不能为空";
                    return result;
                }
                
                // 预处理：替换标点符号和换行
                message = message.Replace('，', ',')
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace(" ", "");
                
                // 按逗号分割成多个下注语句
                string[] betStrings = message.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string betString in betStrings)
                {
                    if (string.IsNullOrWhiteSpace(betString))
                        continue;
                    
                    ParseSingleBetString(betString, result);
                }
                
                if (result.Items.Count == 0)
                {
                    result.ErrorMessage = "未识别到有效的下注内容";
                    result.Code = -1;
                }
                else
                {
                    result.Code = 0; // 成功
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.Code = -1;
            }
            
            return result;
        }
        
        /// <summary>
        /// 解析单个下注字符串
        /// 例如: "123大4小5单龙100" => 多个下注项
        /// </summary>
        private static void ParseSingleBetString(string betString, BinggoBetContent result)
        {
            // 正则: 匹配车号+玩法组合+金额
            // 例如: "123大4小5单龙100"
            // 匹配: ([123456一二三四五六总和]+)?([大小单双尾大尾小合单合双龙虎]+) 重复多次 + (\d+) 金额
            string pattern = @"(([123456一二三四五六总和]+)?([大小单双尾大尾小合单合双龙虎]+))+(\d+)";
            var match = Regex.Match(betString, pattern);
            
            if (!match.Success)
            {
                throw new Exception($"无法识别的下注格式: {betString}");
            }
            
            // 提取金额 (最后的数字)
            string amountStr = match.Groups[4].Value;
            if (!decimal.TryParse(amountStr, out decimal amount))
            {
                throw new Exception($"金额格式错误: {amountStr}");
            }
            
            // 提取所有车号+玩法组合
            string pattern2 = @"([123456一二三四五六总和]+)?([大小单双尾大尾小合单合双龙虎]+)";
            var matches = Regex.Matches(betString, pattern2);
            
            foreach (Match m in matches)
            {
                string carStr = m.Groups[1].Value;    // 车号部分 (可能为空)
                string playStr = m.Groups[2].Value;   // 玩法部分
                
                // 解析玩法
                var playTypes = ParsePlayTypes(playStr);
                
                // 解析车号
                var carNumbers = ParseCarNumbers(carStr, playTypes);
                
                // 为每个车号和玩法创建下注项
                foreach (var play in playTypes)
                {
                    // 龙虎特殊处理：始终用车号6(总和)
                    if (play == BinggoPlayType.龙 || play == BinggoPlayType.虎)
                    {
                        AddOrIncrementBetItem(result, 6, play, amount);
                        continue;
                    }
                    
                    // 其他玩法：为每个车号创建
                    foreach (var carNumber in carNumbers)
                    {
                        AddOrIncrementBetItem(result, carNumber, play, amount);
                    }
                }
            }
        }
        
        /// <summary>
        /// 解析车号字符串
        /// 例如: "123" => [1,2,3]
        /// 例如: "一二三" => [1,2,3]
        /// 例如: "总和" => [6]
        /// 例如: "" => [6] (默认为总和)
        /// </summary>
        private static List<int> ParseCarNumbers(string carStr, List<BinggoPlayType> playTypes)
        {
            var result = new List<int>();
            
            // 如果车号为空，且玩法包含龙虎，默认为6(总和)
            if (string.IsNullOrEmpty(carStr))
            {
                if (playTypes.Any(p => p == BinggoPlayType.龙 || p == BinggoPlayType.虎))
                {
                    return new List<int> { 6 };
                }
                // 否则默认为总和
                return new List<int> { 6 };
            }
            
            // 中文转换
            carStr = carStr.Replace("总和", "6")
                          .Replace("总", "6")
                          .Replace("一", "1")
                          .Replace("二", "2")
                          .Replace("三", "3")
                          .Replace("四", "4")
                          .Replace("五", "5")
                          .Replace("六", "6");
            
            // 🔥 提取所有数字（允许重复，参考 F5BotV2）
            // 例如: "23333" → [2, 3, 3, 3, 3]（不去重！）
            foreach (char c in carStr)
            {
                if (char.IsDigit(c))
                {
                    int num = int.Parse(c.ToString());
                    if (num >= 1 && num <= 6)
                    {
                        result.Add(num);  // 🔥 去除 !result.Contains(num) 条件，允许重复
                    }
                }
            }
            
            if (result.Count == 0)
            {
                throw new Exception($"无效的车号: {carStr}");
            }
            
            return result;
        }
        
        /// <summary>
        /// 解析玩法字符串
        /// 例如: "大单" => [大, 单]
        /// </summary>
        private static List<BinggoPlayType> ParsePlayTypes(string playStr)
        {
            var result = new List<BinggoPlayType>();
            
            // 特殊玩法优先匹配（防止"尾大"被识别为"大"）
            playStr = playStr.Replace("尾大", "5")
                            .Replace("尾小", "6")
                            .Replace("合单", "7")
                            .Replace("合双", "8");
            
            // 逐字符解析
            foreach (char c in playStr)
            {
                BinggoPlayType playType = BinggoPlayType.未知;
                
                if (char.IsDigit(c))
                {
                    // 数字代表特殊玩法
                    int num = int.Parse(c.ToString());
                    playType = (BinggoPlayType)num;
                }
                else
                {
                    // 文字转换
                    switch (c)
                    {
                        case '大': playType = BinggoPlayType.大; break;
                        case '小': playType = BinggoPlayType.小; break;
                        case '单': playType = BinggoPlayType.单; break;
                        case '双': playType = BinggoPlayType.双; break;
                        case '龙': playType = BinggoPlayType.龙; break;
                        case '虎': playType = BinggoPlayType.虎; break;
                    }
                }
                
                if (playType != BinggoPlayType.未知 && !result.Contains(playType))
                {
                    result.Add(playType);
                }
            }
            
            if (result.Count == 0)
            {
                throw new Exception($"无效的玩法: {playStr}");
            }
            
            return result;
        }
        
        /// <summary>
        /// 添加或累加下注项
        /// 🔥 参考 F5BotV2 第 272-283 行：只要车号和玩法相同就累加数量，不管金额
        /// 这样 1233333大10 会解析为: 1大10(1注), 2大10(1注), 3大10×5(5注)
        /// </summary>
        private static void AddOrIncrementBetItem(BinggoBetContent result, int carNumber, BinggoPlayType playType, decimal amount)
        {
            // 🔥 关键修复：只匹配车号和玩法，不匹配金额（参考 F5BotV2 第272行）
            var existing = result.Items.FirstOrDefault(item => 
                item.CarNumber == carNumber && item.PlayType == playType);
            
            if (existing != null)
            {
                // 🔥 已存在相同车号+玩法：累加数量（参考 F5BotV2 第275行：item.numberAdd()）
                existing.AddQuantity();
            }
            else
            {
                // 🔥 首次出现：新增（参考 F5BotV2 第280行）
                result.Items.Add(new BinggoBetItem(carNumber, playType, amount));
            }
        }
        
        /// <summary>
        /// 判断单个下注项是否中奖（🔥 完全参考 F5BotV2 LotteryNumber.cs 第 71-89 行）
        /// </summary>
        public static bool IsWin(BinggoBetItem betItem, BinggoLotteryData lotteryData)
        {
            if (lotteryData == null || !lotteryData.IsOpened)
                return false;
            
            // 龙虎特殊处理
            if (betItem.PlayType == BinggoPlayType.龙)
            {
                return lotteryData.DragonTiger == DragonTigerType.Dragon;
            }
            if (betItem.PlayType == BinggoPlayType.虎)
            {
                return lotteryData.DragonTiger == DragonTigerType.Tiger;
            }
            
            // 获取对应车号的值
            int value = 0;
            bool isPSum = false; // 是否为总和
            switch (betItem.CarNumber)
            {
                case 1: value = lotteryData.P1?.Number ?? 0; break;
                case 2: value = lotteryData.P2?.Number ?? 0; break;
                case 3: value = lotteryData.P3?.Number ?? 0; break;
                case 4: value = lotteryData.P4?.Number ?? 0; break;
                case 5: value = lotteryData.P5?.Number ?? 0; break;
                case 6: 
                    value = lotteryData.PSum?.Number ?? 0;
                    isPSum = true; // 标记为总和
                    break;
                default: return false;
            }
            
            // 判断玩法是否中奖
            switch (betItem.PlayType)
            {
                case BinggoPlayType.大:
                    // 🔥 参考 F5BotV2: LotteryNumber.cs 第 71-89 行
                    if (isPSum)
                    {
                        // P总（总和）：203 <= number <= 390 为大
                        return value >= 203 && value <= 390;
                    }
                    else
                    {
                        // P1-P5（单个车号）：number > 40 为大
                        return value > 40;
                    }
                    
                case BinggoPlayType.小:
                    // 🔥 参考 F5BotV2: LotteryNumber.cs 第 71-89 行
                    if (isPSum)
                    {
                        // P总（总和）：15 <= number <= 202 为小
                        return value >= 15 && value <= 202;
                    }
                    else
                    {
                        // P1-P5（单个车号）：number <= 40 为小
                        return value <= 40;
                    }
                case BinggoPlayType.单:
                    return value % 2 == 1;
                case BinggoPlayType.双:
                    return value % 2 == 0;
                case BinggoPlayType.尾大:
                    return (value % 10) >= 5;
                case BinggoPlayType.尾小:
                    return (value % 10) < 5;
                case BinggoPlayType.合单:
                    int sum = (value / 10) + (value % 10);
                    return sum % 2 == 1;
                case BinggoPlayType.合双:
                    sum = (value / 10) + (value % 10);
                    return sum % 2 == 0;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 计算单个下注项的盈利
        /// </summary>
        /// <param name="betItem">下注项</param>
        /// <param name="lotteryData">开奖数据</param>
        /// <param name="odds">赔率</param>
        /// <param name="isIntegerSettle">是否整数结算</param>
        /// <returns>盈利金额（正数=赢，负数=输）</returns>
        public static decimal CalculateProfit(BinggoBetItem betItem, BinggoLotteryData lotteryData, decimal odds, bool isIntegerSettle = false)
        {
            if (IsWin(betItem, lotteryData))
            {
                // 中奖: 盈利 = 总投注额 × 赔率
                decimal profit = betItem.TotalAmount * odds;
                
                if (isIntegerSettle)
                {
                    profit = Math.Floor(profit); // 取整
                }
                
                return profit;
            }
            else
            {
                // 未中奖: 损失 = -总投注额
                return -betItem.TotalAmount;
            }
        }
        
        /// <summary>
        /// 计算整个下注内容的总盈利
        /// </summary>
        public static decimal CalculateTotalProfit(BinggoBetContent betContent, BinggoLotteryData lotteryData, decimal odds, bool isIntegerSettle = false)
        {
            decimal totalProfit = 0;
            
            foreach (var item in betContent.Items)
            {
                totalProfit += CalculateProfit(item, lotteryData, odds, isIntegerSettle);
            }
            
            return totalProfit;
        }
    }
}
