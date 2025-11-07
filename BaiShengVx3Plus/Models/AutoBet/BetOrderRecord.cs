using System;
using SQLite;

namespace BaiShengVx3Plus.Models.AutoBet
{
    /// <summary>
    /// 投注订单记录
    /// </summary>
    [Table("AutoBetOrders")]
    public class BetOrderRecord
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        
        public string ConfigName { get; set; } = "";
        
        public int ConfigId { get; set; }
        
        public string Platform { get; set; } = "";
        
        public string IssueId { get; set; } = "";
        
        public string PlayType { get; set; } = "";
        
        public string BetContent { get; set; } = "";
        
        public decimal Amount { get; set; }
        
        public string? PlatformOrderId { get; set; }
        
        public string Status { get; set; } = "待处理";
        
        public string? ErrorMessage { get; set; }
        
        public DateTime CreateTime { get; set; } = DateTime.Now;
        
        public DateTime? SettleTime { get; set; }
    }
}
