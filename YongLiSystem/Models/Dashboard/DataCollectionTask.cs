using System;

namespace YongLiSystem.Models.Dashboard
{
    /// <summary>
    /// 数据采集任务项
    /// </summary>
    public class DataCollectionTask
    {
        /// <summary>
        /// 任务ID (自增)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 期号 (如: 113004873)
        /// </summary>
        public int IssueId { get; set; }

        /// <summary>
        /// 开奖数据 (格式: "01,02,03,04,05")
        /// </summary>
        public string OpenData { get; set; } = string.Empty;

        /// <summary>
        /// 采集次数
        /// </summary>
        public int AttemptCount { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime? CollectionTime { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public CollectionTaskStatus Status { get; set; } = CollectionTaskStatus.Pending;

        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSource { get; set; } = string.Empty;

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 采集任务状态枚举
    /// </summary>
    public enum CollectionTaskStatus
    {
        /// <summary>
        /// 待采集
        /// </summary>
        Pending,

        /// <summary>
        /// 采集中
        /// </summary>
        Collecting,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed,

        /// <summary>
        /// 失败
        /// </summary>
        Failed
    }
}

