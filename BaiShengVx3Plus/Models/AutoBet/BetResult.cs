using System;

namespace BaiShengVx3Plus.Models.AutoBet
{
    /// <summary>
    /// 投注结果
    /// </summary>
    public class BetResult
    {
        public bool Success { get; set; }
        public string? OrderId { get; set; }
        public string? ErrorMessage { get; set; }
        public object? Data { get; set; }
    }
}
