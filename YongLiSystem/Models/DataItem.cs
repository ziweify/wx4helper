using System;

namespace 永利系统.Models
{
    /// <summary>
    /// 数据项模型
    /// </summary>
    public class DataItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsActive { get; set; }
    }
}

