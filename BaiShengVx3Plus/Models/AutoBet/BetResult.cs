using System;

namespace BaiShengVx3Plus.Models.AutoBet
{
    /// <summary>
    /// 投注结果
    /// </summary>
    public class BetResult
    {
        public bool Success { get; set; }
        public string? OrderId { get; set; }          // 兼容旧代码
        public string? Result { get; set; }           // 平台返回的原始数据
        public string? ErrorMessage { get; set; }
        public object? Data { get; set; }             // 兼容旧代码
        public DateTime? PostStartTime { get; set; }  // POST前时间
        public DateTime? PostEndTime { get; set; }    // POST后时间
        public int? DurationMs { get; set; }          // 耗时（毫秒）
        public string? OrderNo { get; set; }          // 平台订单号
    }
}
