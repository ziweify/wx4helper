using System;
using System.ComponentModel;
using SQLite;

namespace zhaocaimao.Models.AutoBet
{
    /// <summary>
    /// 投注记录表
    /// 记录所有投注行为（自动和手动）
    /// 🔥 实现 INotifyPropertyChanged 以支持 BindingList 自动保存
    /// </summary>
    [Table("BetRecords")]
    public class BetRecord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        /// <summary>
        /// 配置ID
        /// </summary>
        public int ConfigId { get; set; }
        
        /// <summary>
        /// 期号
        /// </summary>
        public int IssueId { get; set; }
        
        /// <summary>
        /// 来源：订单|命令
        /// </summary>
        public BetRecordSource Source { get; set; }
        
        /// <summary>
        /// 关联的订单ID列表（逗号分隔）
        /// 仅当 Source=订单 时有值
        /// </summary>
        public string? OrderIds { get; set; }
        
        /// <summary>
        /// 投注内容（标准格式）
        /// 格式：1大50,2大30,3大60
        /// 必须是已解析的标准格式，可直接投注
        /// </summary>
        public string BetContentStandard { get; set; } = "";
        
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// VxMain发送命令时间
        /// </summary>
        public DateTime SendTime { get; set; }
        
        /// <summary>
        /// Client POST前时间（从Client返回）
        /// </summary>
        public DateTime? PostStartTime { get; set; }
        
        /// <summary>
        /// Client POST后时间（从Client返回）
        /// </summary>
        public DateTime? PostEndTime { get; set; }
        
        /// <summary>
        /// 耗时（毫秒）
        /// </summary>
        public int? DurationMs { get; set; }
        
        /// <summary>
        /// 是否成功（null=未返回）
        /// </summary>
        public bool? Success { get; set; }
        
        /// <summary>
        /// 返回结果（平台返回的原始数据）
        /// </summary>
        public string? Result { get; set; }
        
        /// <summary>
        /// 异常信息
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// 平台订单号
        /// </summary>
        public string? OrderNo { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
    
    /// <summary>
    /// 投注记录来源
    /// </summary>
    public enum BetRecordSource
    {
        /// <summary>
        /// 来源于订单表，会更新订单状态
        /// </summary>
        订单 = 1,
        
        /// <summary>
        /// 来源于手动命令，不更新订单状态
        /// </summary>
        命令 = 2
    }
}

