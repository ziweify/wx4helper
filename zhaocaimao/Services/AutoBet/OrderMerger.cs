using System;
using System.Collections.Generic;
using System.Linq;
using zhaocaimao.Contracts;
using zhaocaimao.Models;
using zhaocaimao.Shared.Models;  // 🔥 使用共享的模型

namespace zhaocaimao.Services.AutoBet
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
                
                // 🔥 解析 BetContentStandar（已经是标准格式，如 "1大20"）
                // 使用完全照搬 F5BotV2 的解析逻辑
                var items = zhaocaimao.Shared.Parsers.BetContentParser.ParseBetContentToOrderList(order.BetContentStandar, order.IssueId);
                foreach (var item in items)
                {
                    allItems.Add(item);
                }
                
                _log.Info("OrderMerger", 
                    $"  订单{order.Id}:{order.BetContentStandar} {order.AmountTotal}元 → {items.Count}项");
            }
            
            // 合并相同号码和玩法的投注项（参考 F5BotV2 逻辑）
            var mergedItems = MergeBetItems(allItems);
            
            // 🔥 生成标准投注内容（格式："1大50,2小30"）- 注意：不带P前缀
            // 将 CarNumEnum.P1 转换为 "1"，而不是 "P1"
            var betContentStandard = string.Join(",", mergedItems.Select(item => 
            {
                var carNumber = item.Car switch
                {
                    CarNumEnum.P1 => "1",
                    CarNumEnum.P2 => "2",
                    CarNumEnum.P3 => "3",
                    CarNumEnum.P4 => "4",
                    CarNumEnum.P5 => "5",
                    CarNumEnum.P总 => "6",
                    _ => "1"
                };
                return $"{carNumber}{GetPlayName(item.Play)}{item.MoneySum}";
            }));
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

