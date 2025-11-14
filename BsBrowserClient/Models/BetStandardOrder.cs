using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BsBrowserClient.Models
{
    // Enums from F5BotV2
    public enum BetPlayEnum
    {
        未知 = 0, 单 = 1, 双 = 2, 大 = 3, 小 = 4, 尾大 = 5, 尾小 = 6, 合单 = 7, 合双 = 8,
        龙 = 9, 虎 = 10, 和 = 11, 福 = 12, 寿 = 13, 喜 = 14
    }

    public enum CarNumEnum
    {
        未知 = 0, P1 = 1, P2 = 2, P3 = 3, P4 = 4, P5 = 5, P总 = 6
    }

    /// <summary>
    /// 标准化投注订单 (参考 F5BotV2)
    /// </summary>
    public class BetStandardOrder
    {
        [JsonProperty("issueId")]
        public int IssueId { get; set; }
        
        [JsonProperty("car")]
        public CarNumEnum car { get; set; }
        
        [JsonProperty("play")]
        public BetPlayEnum play { get; set; }
        
        [JsonProperty("moneySum")]
        public int moneySum { get; set; }
        
        [JsonProperty("odds")]
        public float Odds { get; set; } // 网站的赔率

        public BetStandardOrder(int IssueId, CarNumEnum car, BetPlayEnum play, int moneySum)
        {
            this.IssueId = IssueId;
            this.car = car;
            this.play = play;
            this.moneySum = moneySum;
        }

        // Parameterless constructor for deserialization
        public BetStandardOrder() { }
    }

    /// <summary>
    /// 标准化投注订单列表
    /// </summary>
    public class BetStandardOrderList : List<BetStandardOrder>
    {
        public int GetTotalAmount()
        {
            int result = 0;
            foreach (var item in this)
            {
                result += item.moneySum;
            }
            return result;
        }
    }
}


