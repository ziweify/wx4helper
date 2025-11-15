using System;
using System.Collections.Generic;
using BaiShengVx3Plus.Shared.Models;

namespace BaiShengVx3Plus.Shared.Parsers
{
    /// <summary>
    /// 投注内容解析器（公共解析类）
    /// </summary>
    public static class BetContentParser
    {
        /// <summary>
        /// 解析投注内容为投注项列表
        /// 来源：OrderMerger.ParseBetContent
        /// </summary>
        public static BetStandardOrderList ParseBetContent(string? betContentStandar, float amount)
        {
            var items = new BetStandardOrderList();
            
            if (string.IsNullOrEmpty(betContentStandar))
            {
                return items;
            }
            
            // 🔥 BetContentStandar 格式：1大20,3大20,4大20（逗号分隔多个投注项）
            // 每个投注项格式：号码 + 玩法 + 金额
            
            try
            {
                // 🔥 先按逗号分割
                var parts = betContentStandar.Split(',', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var part in parts)
                {
                    var content = part.Trim();
                    if (string.IsNullOrEmpty(content)) continue;
                    
                    // 🔥 解析单个投注项："1大20" 或 "P1大20"
                    // 提取：号码、玩法、金额
                    var number = "";
                    var playType = "";
                    var amountStr = "";
                    
                    // 🔥 移除P前缀（如果存在）
                    if (content.StartsWith("P", StringComparison.OrdinalIgnoreCase) && content.Length > 1 && char.IsDigit(content[1]))
                    {
                        content = content.Substring(1);  // 移除 "P" 前缀
                    }
                    
                    foreach (var ch in content)
                    {
                        if (char.IsDigit(ch))
                        {
                            if (string.IsNullOrEmpty(playType))
                            {
                                // 还没有玩法，说明是号码
                                number += ch;
                            }
                            else
                            {
                                // 已经有玩法了，说明是金额
                                amountStr += ch;
                            }
                        }
                        else if (char.IsLetter(ch) || ch >= 0x4E00 && ch <= 0x9FA5)  // 汉字范围
                        {
                            playType += ch;
                        }
                    }
                    
                    // 解析金额
                    int itemAmount = string.IsNullOrEmpty(amountStr) ? 0 : int.Parse(amountStr);
                    
                    if (!string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(playType) && itemAmount > 0)
                    {
                        // 将号码和玩法转换为枚举
                        var carEnum = number switch
                        {
                            "1" => CarNumEnum.P1,
                            "2" => CarNumEnum.P2,
                            "3" => CarNumEnum.P3,
                            "4" => CarNumEnum.P4,
                            "5" => CarNumEnum.P5,
                            "总" or "6" => CarNumEnum.P总,
                            _ => CarNumEnum.P1
                        };
                        
                        var playEnum = playType switch
                        {
                            "大" => BetPlayEnum.大,
                            "小" => BetPlayEnum.小,
                            "单" => BetPlayEnum.单,
                            "双" => BetPlayEnum.双,
                            "尾大" => BetPlayEnum.尾大,
                            "尾小" => BetPlayEnum.尾小,
                            _ => BetPlayEnum.大
                        };
                        
                        items.Add(new BetStandardOrder(0, carEnum, playEnum, itemAmount));
                    }
                }
            }
            catch (Exception ex)
            {
                // 注意：原代码使用 _log.Error，但这里是静态类，不记录日志
                // 如果需要日志，可以在调用方处理
                throw new Exception($"解析投注内容失败:{betContentStandar}", ex);
            }
            
            return items;
        }
        
        /// <summary>
        /// 解析投注内容字符串为 BetStandardOrderList
        /// 格式："1大10,2大10,3大10,4大10"
        /// 来源：AutoBetService.ParseBetContentToOrderList
        /// </summary>
        public static BetStandardOrderList ParseBetContentToOrderList(string betContentStandard, int issueId)
        {
            var orders = new BetStandardOrderList();
            
            if (string.IsNullOrEmpty(betContentStandard))
            {
                return orders;
            }
            
            try
            {
                // 🔥 按逗号分割多个投注项
                var parts = betContentStandard.Split(',', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var part in parts)
                {
                    var content = part.Trim();
                    if (string.IsNullOrEmpty(content)) continue;
                    
                    // 🔥 解析单个投注项："1大10" 或 "P1大10"
                    // 提取：号码、玩法、金额
                    var number = "";
                    var playType = "";
                    var amountStr = "";
                    
                    // 🔥 移除P前缀（如果存在）
                    if (content.StartsWith("P", StringComparison.OrdinalIgnoreCase) && content.Length > 1 && char.IsDigit(content[1]))
                    {
                        content = content.Substring(1);  // 移除 "P" 前缀
                    }
                    
                    foreach (var ch in content)
                    {
                        if (char.IsDigit(ch))
                        {
                            if (string.IsNullOrEmpty(playType))
                            {
                                // 还没有玩法，说明是号码
                                number += ch;
                            }
                            else
                            {
                                // 已经有玩法了，说明是金额
                                amountStr += ch;
                            }
                        }
                        else if (char.IsLetter(ch) || (ch >= 0x4E00 && ch <= 0x9FA5))  // 汉字范围
                        {
                            playType += ch;
                        }
                    }
                    
                    // 解析金额
                    if (int.TryParse(amountStr, out int itemAmount) && itemAmount > 0)
                    {
                        // 解析号码
                        var carEnum = number switch
                        {
                            "1" => CarNumEnum.P1,
                            "2" => CarNumEnum.P2,
                            "3" => CarNumEnum.P3,
                            "4" => CarNumEnum.P4,
                            "5" => CarNumEnum.P5,
                            "总" or "6" => CarNumEnum.P总,
                            _ => CarNumEnum.P1
                        };
                        
                        // 解析玩法
                        var playEnum = playType switch
                        {
                            "大" => BetPlayEnum.大,
                            "小" => BetPlayEnum.小,
                            "单" => BetPlayEnum.单,
                            "双" => BetPlayEnum.双,
                            "尾大" => BetPlayEnum.尾大,
                            "尾小" => BetPlayEnum.尾小,
                            "合单" => BetPlayEnum.合单,
                            "合双" => BetPlayEnum.合双,
                            "龙" => BetPlayEnum.龙,
                            "虎" => BetPlayEnum.虎,
                            "寿" => BetPlayEnum.寿,
                            "喜" => BetPlayEnum.喜,
                            _ => BetPlayEnum.大
                        };
                        
                        orders.Add(new BetStandardOrder(issueId, carEnum, playEnum, itemAmount));
                    }
                }
            }
            catch (Exception ex)
            {
                // 注意：原代码使用 _log.Error，但这里是静态类，不记录日志
                // 如果需要日志，可以在调用方处理
                throw new Exception($"解析投注内容失败: {betContentStandard}", ex);
            }
            
            return orders;
        }
        
        /// <summary>
        /// 解析投注内容："1234大10" → "1大10,2大10,3大10,4大10"
        /// 来源：BetConfigManagerForm.ParseBetContent
        /// </summary>
        public static string ParseBetContent(string input)
        {
            try
            {
                var items = new List<string>();
                
                // 按空格或逗号分割
                var parts = input.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    
                    // 检查是否包含连续数字（如："1234大20"）
                    var match = System.Text.RegularExpressions.Regex.Match(
                        trimmed, 
                        @"^(\d+)(大|小|单|双)(\d+)$"
                    );
                    
                    if (match.Success)
                    {
                        var numbers = match.Groups[1].Value;  // "1234"
                        var type = match.Groups[2].Value;      // "大"
                        var amount = match.Groups[3].Value;    // "10"
                        
                        // 拆分为单个投注
                        foreach (var num in numbers)
                        {
                            items.Add($"{num}{type}{amount}");
                        }
                    }
                    else
                    {
                        // 已经是标准格式或无法解析，直接添加
                        items.Add(trimmed);
                    }
                }
                
                return string.Join(",", items);
            }
            catch
            {
                // 注意：原代码使用 _logService.Error，但这里是静态类，不记录日志
                // 如果需要日志，可以在调用方处理
                // 解析失败返回原内容
                return input;
            }
        }
    }
}

