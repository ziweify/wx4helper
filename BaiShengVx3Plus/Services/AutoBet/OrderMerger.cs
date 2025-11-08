using System;
using System.Collections.Generic;
using System.Linq;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models;

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
            public List<BetItem> BetItems { get; set; } = new();
        }
        
        /// <summary>
        /// 投注项
        /// </summary>
        public class BetItem
        {
            public string Number { get; set; } = "";      // 号码："1"
            public string PlayType { get; set; } = "";    // 玩法："大"
            public decimal Amount { get; set; }           // 金额：50
            
            public override string ToString()
            {
                return $"{Number}{PlayType}{Amount}";
            }
        }
        
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
            var allItems = new List<BetItem>();
            var orderIds = new List<long>();
            
            foreach (var order in orderList)
            {
                orderIds.Add(order.Id);
                
                // 解析 BetContentStandar（已经是标准格式，如 "1大20"）
                var items = ParseBetContent(order.BetContentStandar, order.AmountTotal);
                allItems.AddRange(items);
                
                _log.Info("OrderMerger", 
                    $"  订单{order.Id}:{order.BetContentStandar} {order.AmountTotal}元 → {items.Count}项");
            }
            
            // 合并相同号码和玩法的投注项（参考 F5BotV2 逻辑）
            var mergedItems = MergeBetItems(allItems);
            
            // 生成标准投注内容
            var betContentStandard = string.Join(",", mergedItems.Select(item => item.ToString()));
            var totalAmount = mergedItems.Sum(item => item.Amount);
            
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
        private List<BetItem> ParseBetContent(string? betContentStandar, float amount)
        {
            var items = new List<BetItem>();
            
            if (string.IsNullOrEmpty(betContentStandar))
            {
                return items;
            }
            
            // BetContentStandar 格式：1大20 或 1大
            // 需要提取：号码、玩法、金额
            
            try
            {
                var content = betContentStandar.Trim();
                
                // 尝试解析格式："1大20" 或 "1大"
                // 提取数字和汉字
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
                
                // 如果没有解析出金额，使用订单总金额
                decimal itemAmount = string.IsNullOrEmpty(amountStr) ? 
                    (decimal)amount : decimal.Parse(amountStr);
                
                if (!string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(playType))
                {
                    items.Add(new BetItem
                    {
                        Number = number,
                        PlayType = playType,
                        Amount = itemAmount
                    });
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
        private List<BetItem> MergeBetItems(List<BetItem> items)
        {
            var merged = new List<BetItem>();
            
            // 按号码和玩法分组，累加金额
            var groups = items.GroupBy(item => new { item.Number, item.PlayType });
            
            foreach (var group in groups)
            {
                merged.Add(new BetItem
                {
                    Number = group.Key.Number,
                    PlayType = group.Key.PlayType,
                    Amount = group.Sum(item => item.Amount)
                });
            }
            
            // 排序（按号码）
            return merged.OrderBy(item => item.Number).ToList();
        }
    }
}

