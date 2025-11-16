using System;
using Newtonsoft.Json;

namespace zhaocaimao.Models.Api
{
    /// <summary>
    /// API 返回的开奖数据格式（F5BotV2）
    /// 
    /// 完全对应 F5BotV2 的 API 响应格式
    /// </summary>
    public class BsApiLotteryData
    {
        [JsonProperty("issueid")]
        public int IssueId { get; set; }
        
        [JsonProperty("p1")]
        public string P1 { get; set; } = string.Empty;
        
        [JsonProperty("p2")]
        public string P2 { get; set; } = string.Empty;
        
        [JsonProperty("p3")]
        public string P3 { get; set; } = string.Empty;
        
        [JsonProperty("p4")]
        public string P4 { get; set; } = string.Empty;
        
        [JsonProperty("p5")]
        public string P5 { get; set; } = string.Empty;
        
        [JsonProperty("date")]
        public string Date { get; set; } = string.Empty;
        
        [JsonProperty("lottery_time")]
        public string LotteryTime { get; set; } = string.Empty;
        
        [JsonProperty("issue_day_index")]
        public int IssueDayIndex { get; set; }
    }
}

