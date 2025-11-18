using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using zhaocaimao.Shared.Models;

namespace zhaocaimao.Shared.Parsers
{
    /// <summary>
    /// 投注内容解析器
    /// 🔥 完全照搬 F5BotV2/Boter/BoterBetContent.cs 的实现（第66-287行）
    /// 不进行任何自作主张的修改
    /// </summary>
    public static class BetContentParser
    {
        /// <summary>
        /// 解析投注内容
        /// 🔥 完全参照 F5BotV2/Boter/BoterBetContent.cs 第66-156行
        /// 支持格式：
        /// - "123大100" => 1大100, 2大100, 3大100
        /// - "123大45单龙100" => 1大100, 2大100, 3大100, 4单100, 5单100, 龙100
        /// - "1大50,2小60" => 1大50, 2小60
        /// </summary>
        public static BetStandardOrderList ParseBetContent(string? message, int issueId)
        {
            var items = new BetStandardOrderList();
            
            if (string.IsNullOrWhiteSpace(message))
            {
                return items;
            }
            
            try
            {
                // 🔥 预处理（参考 F5BotV2 第73-74行）
                message = message.Replace('，', ',');
                message = message.Replace('\r', ',');
                
                // 🔥 按逗号分割（参考 F5BotV2 第79行）
                string[] betStrings = message.Split(',');
                
                foreach (string s in betStrings)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        continue;
                    
                    // 🔥 字符串类型2：特殊格式 123大4小2双龙虎100 => 1大100,2大100,3大100,4小100,2双100,龙100,虎100
                    // 参考 F5BotV2 第120-148行
                    string regexHead = @"(([123456一二三四五六总和]+)?([大小单双尾大尾小合单合双龙虎]+))+(\d+)";
                    var rgxHead = Regex.Match(s, regexHead);
                    var h1 = rgxHead.Groups[1].Value;
                    var h2 = rgxHead.Groups[2].Value;
                    var h3 = rgxHead.Groups[3].Value;
                    var h4 = rgxHead.Groups[4].Value;
                    
                    string regex = @"([123456一二三四五六总和]+)?([大小单双尾大尾小合单合双龙虎]+)";
                    var matches = Regex.Matches(s, regex);
                    var len = matches.Count;
                    if (len == 0)
                        throw new Exception("无效货单!");
                    
                    foreach (Match item in matches)
                    {
                        string message2 = item + h4;
                        // 🔥 字符串类型2：通用格式 12345大单双100  这种单一金额格式的
                        regex = @"([123456一二三四五六总和]*){1}([大小单双尾大尾小合单合双龙虎]*)(\d*$)";
                        if (Regex.IsMatch(message2, regex))
                        {
                            ParseBetStandardString(message2, issueId, items);
                        }
                    }
                }
            }
            catch
            {
                // 解析失败返回空列表
            }
            
            return items;
        }
        
        /// <summary>
        /// 解析标准投注字符串（格式：123456大单双100）
        /// 🔥 完全参照 F5BotV2/Boter/BoterBetContent.cs 第164-287行
        /// </summary>
        private static int ParseBetStandardString(string betString, int issueId, BetStandardOrderList items)
        {
            int reponse = 0;
            
            // 🔥 正则解析（参考 F5BotV2 第167-174行）
            string text = betString.Replace(" ", "");
            string regex = @"([123456一二三四五六总和]*){1}([大小单双尾大尾小合单合双龙虎]*)(\d*)([^#]*)";
            var match = Regex.Match(text, regex);
            var s0 = match.Groups[0].Value;
            var s1 = match.Groups[1].Value;
            var s2 = match.Groups[2].Value;
            var s3 = match.Groups[3].Value;
            var s4 = match.Groups[4].Value;
            
            // 🔥 解析车号（参考 F5BotV2 第179-205行）
            List<CarNumEnum> cars = new List<CarNumEnum>();
            string strCars = s1.Replace("总和", "6").Replace("总", "6")
                .Replace("一", "1")
                .Replace("二", "2")
                .Replace("三", "3")
                .Replace("四", "4")
                .Replace("五", "5")
                .Replace("六", "6");
            
            if (string.IsNullOrEmpty(strCars))
            {
                strCars = "6";
            }
            
            try
            {
                foreach (char c in strCars)
                {
                    string s_tmp = c.ToString();
                    CarNumEnum betcar = (CarNumEnum)Convert.ToInt32(s_tmp);
                    cars.Add(betcar);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"校验失败,{s1},请检查后重新开始!");
            }
            
            // 🔥 解析玩法（参考 F5BotV2 第207-241行）
            List<BetPlayEnum> plays = new List<BetPlayEnum>();
            string strPlays = s2.Replace("尾大", "5")
                .Replace("尾小", "6")
                .Replace("合单", "7")
                .Replace("合双", "8");
            
            try
            {
                foreach (var strPlay in strPlays)
                {
                    BetPlayEnum play = BetPlayEnum.未知;
                    string tmp = strPlay.ToString();
                    if (Regex.IsMatch(tmp, @"^\d+$"))
                    {
                        play = (BetPlayEnum)Convert.ToInt32(tmp);
                    }
                    else
                    {
                        play = (BetPlayEnum)Enum.Parse(typeof(BetPlayEnum), tmp);
                    }
                    if (play == BetPlayEnum.未知)
                        throw new Exception($"校验失败,{s2},请检查后重新开始!");
                    plays.Add(play);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"校验失败,{s2},请检查后重新开始!");
            }
            
            // 🔥 解析金额（参考 F5BotV2 第242-247行）
            int money = 0;
            try
            {
                money = Convert.ToInt32(s3);
            }
            catch
            {
                throw new Exception($"货单校验失败,{s3}");
            }
            
            // 🔥 开始组装订单（参考 F5BotV2 第249-284行）
            foreach (var play in plays)
            {
                // 🔥 龙虎特殊处理（参考 F5BotV2 第252-266行）
                if (play == BetPlayEnum.龙 || play == BetPlayEnum.虎)
                {
                    var item = items.FirstOrDefault(p => p.Car == CarNumEnum.P总 && p.Play == play);
                    if (item != null)
                    {
                        // 🔥 已存在，累加金额（参考 F5BotV2 第257行：numberAdd）
                        item.MoneySum += money;
                        reponse++;
                    }
                    else
                    {
                        items.Add(new BetStandardOrder(issueId, CarNumEnum.P总, play, money));
                        reponse++;
                    }
                    continue;
                }
                
                // 🔥 其他玩法（参考 F5BotV2 第270-283行）
                foreach (var c in cars)
                {
                    var item = items.FirstOrDefault(p => p.Car == c && p.Play == play);
                    if (item != null)
                    {
                        // 🔥 已存在，累加金额（参考 F5BotV2 第274行：numberAdd）
                        item.MoneySum += money;
                        reponse++;
                    }
                    else
                    {
                        items.Add(new BetStandardOrder(issueId, c, play, money));
                        reponse++;
                    }
                }
            }
            
            return reponse;
        }
        
        /// <summary>
        /// 将解析结果转换为标准字符串
        /// 🔥 参考 F5BotV2/Boter/BoterBetContent.cs 第289-299行：ToStandarString
        /// </summary>
        public static string ToStandardString(BetStandardOrderList items)
        {
            if (items == null || items.Count == 0)
                return string.Empty;
            
            var parts = new List<string>();
            foreach (var item in items)
            {
                // 格式: "1大100"
                parts.Add($"{(int)item.Car}{item.Play.ToString()}{item.MoneySum}");
            }
            return string.Join(",", parts);
        }
        
        /// <summary>
        /// 便捷方法：解析投注内容并返回标准字符串
        /// 用于替代原来的 ParseBetContent(string input)
        /// </summary>
        public static string ParseBetContentToString(string input, int issueId = 0)
        {
            var items = ParseBetContent(input, issueId);
            return ToStandardString(items);
        }
        
        /// <summary>
        /// 便捷方法：解析已标准化的投注内容（如 "1大10,2大20"）
        /// 用于 OrderMerger 等场景
        /// </summary>
        public static BetStandardOrderList ParseBetContentToOrderList(string betContentStandard, int issueId)
        {
            // 🔥 已标准化的格式（如 "1大10,2大20"）可以直接使用主解析方法
            // F5BotV2 的解析器支持所有格式，包括已标准化的
            return ParseBetContent(betContentStandard, issueId);
        }
    }
}
