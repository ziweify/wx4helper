using System;

namespace YongLiSystem.Models
{
    /// <summary>
    /// 数据采集项
    /// </summary>
    public class DataCollectionItem
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSource { get; set; } = string.Empty;

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// 数据内容
        /// </summary>
        public string DataValue { get; set; } = string.Empty;

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}

