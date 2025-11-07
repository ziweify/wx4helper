using Newtonsoft.Json;

namespace BsBrowserClient.Models
{
    /// <summary>
    /// 投注订单
    /// </summary>
    public class BetOrder
    {
        /// <summary>
        /// 期号
        /// </summary>
        [JsonProperty("issueId")]
        public string? IssueId { get; set; }
        
        /// <summary>
        /// 玩法类型: 大小、单双、龙虎等
        /// </summary>
        [JsonProperty("playType")]
        public string PlayType { get; set; } = "";
        
        /// <summary>
        /// 投注内容: 大、小、单、双、龙、虎等
        /// </summary>
        [JsonProperty("betContent")]
        public string BetContent { get; set; } = "";
        
        /// <summary>
        /// 投注金额
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}

