using System;
using System.Collections.Generic;
using System.Linq;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Models.AutoBet;  // 🔥 使用统一的 BetItem

namespace BaiShengVx3Plus.Services.AutoBet
{
    /// <summary>
    /// 订单合并器
    /// 将多个订单合并为投注内容
    /// 参考 F5BotV2 的订单合并逻辑
    /// </summary>
    public class OrderMerger
    {
        private readonly ILogService _log;
        
        public OrderMerger(ILogService log)
        {
            _log = log;
        }
        
        /// <summary>
        /// 合并订单结果
        /// </summary>
        public class MergeResult
        {
            /// <summary>
            /// 合并后的标准投注内容："1大50,2大30,3大60"
            /// </summary>
            public string BetContentStandard { get; set; } = "";
            
            /// <summary>
            /// 总金额
            /// </summary>
            public decimal TotalAmount { get; set; }
            
            /// <summary>
            /// 关联的订单ID列表
            /// </summary>
            public List<long> OrderIds { get; set; } = new();
            
            /// <summary>
            /// 合并后的投注项列表
            /// </summary>
            public BetStandardOrderList BetItems { get; set; } = new();
        }
        
        // 🔥 BetItem 已移到 Models.AutoBet.BetItem，统一使用
        
        /// <summary>
        /// 合并订单
        /// </summary>
        public MergeResult Merge(IEnumerable<V2MemberOrder> orders)
        {
            var orderList = orders.ToList();
            
            if (!orderList.Any())
            {
                _log.Warning("OrderMerger", "⚠️ 没有订单需要合并");
                return new MergeResult();
            }
            
            _log.Info("OrderMerger", $"开始合并订单:共{orderList.Count}个");
            
            // 解析所有订单为投注项
            var allItems = new BetStandardOrderList();
            var orderIds = new List<long>();
            
            foreach (var order in orderList)
            {
                orderIds.Add(order.Id);
                
                // 解析 BetContentStandar（已经是标准格式，如 "1大20"）
                var items = ParseBetContent(order.BetContentStandar, order.AmountTotal);
                foreach (var item in items)
                {
                    allItems.Add(item);
                }
                
                _log.Info("OrderMerger", 
                    $"  订单{order.Id}:{order.BetContentStandar} {order.AmountTotal}元 → {items.Count}项");
            }
            
            // 合并相同号码和玩法的投注项（参考 F5BotV2 逻辑）
            var mergedItems = MergeBetItems(allItems);
            
            // 生成标准投注内容（格式："P1大50,P2小30"）
            var betContentStandard = string.Join(",", mergedItems.Select(item => $"{item.Car}{GetPlayName(item.Play)}{item.MoneySum}"));
            var totalAmount = mergedItems.Sum(item => item.MoneySum);
            
            _log.Info("OrderMerger", 
                $"✅ 合并完成:{orderList.Count}个订单 → {mergedItems.Count}项投注 总额{totalAmount}元");
            _log.Info("OrderMerger", $"   内容:{betContentStandard}");
            
            return new MergeResult
            {
                BetContentStandard = betContentStandard,
                TotalAmount = totalAmount,
                OrderIds = orderIds,
                BetItems = mergedItems
            };
        }
        
        /// <summary>
        /// 解析投注内容为投注项列表
        /// </summary>
        private BetStandardOrderList ParseBetContent(string? betContentStandar, float amount)
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
                    
                    // 解析单个投注项："1大20"
                    // 提取：号码、玩法、金额
                    var number = "";
                    var playType = "";
                    var amountStr = "";
                    
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
                _log.Error("OrderMerger", $"解析投注内容失败:{betContentStandar}", ex);
            }
            
            return items;
        }
        
        /// <summary>
        /// 合并投注项（参考 F5BotV2 逻辑）
        /// </summary>
        private BetStandardOrderList MergeBetItems(BetStandardOrderList items)
        {
            var merged = new BetStandardOrderList();
            
            // 按号码和玩法分组，累加金额
            var groups = items.GroupBy(item => new { item.Car, item.Play });
            
            foreach (var group in groups)
            {
                merged.Add(new BetStandardOrder(
                    0, 
                    group.Key.Car, 
                    group.Key.Play, 
                    group.Sum(item => item.MoneySum)
                ));
            }
            
            // 排序（按car枚举值）
            var sorted = merged.OrderBy(item => item.Car).ToList();
            var result = new BetStandardOrderList();
            foreach (var item in sorted)
            {
                result.Add(item);
            }
            return result;
        }
        
        /// <summary>
        /// 获取玩法名称
        /// 🔥 注意：只包含实际使用的玩法，移除不存在的"豹子"和"顺子"
        /// </summary>
        private string GetPlayName(BetPlayEnum play)
        {
            return play switch
            {
                BetPlayEnum.大 => "大",
                BetPlayEnum.小 => "小",
                BetPlayEnum.单 => "单",
                BetPlayEnum.双 => "双",
                BetPlayEnum.尾大 => "尾大",
                BetPlayEnum.尾小 => "尾小",
                BetPlayEnum.合单 => "合单",
                BetPlayEnum.合双 => "合双",
                BetPlayEnum.龙 => "龙",
                BetPlayEnum.虎 => "虎",
                BetPlayEnum.寿 => "寿",
                BetPlayEnum.喜 => "喜",
                _ => ""
            };
        }
    }
}

