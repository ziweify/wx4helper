using System;

namespace BaiShengVx3Plus.Models.AutoBet
{
    /// <summary>
    /// 投注订单
    /// </summary>
    public class BetOrder
    {
        public string? IssueId { get; set; }
        public string PlayType { get; set; } = "";
        public string BetContent { get; set; } = "";
        public decimal Amount { get; set; }
    }
}

