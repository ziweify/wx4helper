using System;
using System.Collections.Generic;
using BaiShengVx3Plus.Shared.Models.Games.Binggo;
using SQLite;

namespace BinGoPlans.Models
{
    /// <summary>
    /// BinGoData 的数据库实体类（用于 SQLite 自动建表）
    /// 继承 BinGoData，添加 SQLite 特性，可以直接用于显示和计算
    /// </summary>
    [Table("BinGoData")]
    public class BinGoDataEntity : BinGoData
    {
        /// <summary>
        /// 无参构造函数（用于 SQLite 反序列化）
        /// </summary>
        public BinGoDataEntity() : base()
        {
        }

        /// <summary>
        /// 构造函数：自动计算 DayIndex
        /// </summary>
        public BinGoDataEntity(int issueId, string lotteryData, DateTime openTime) 
            : base(issueId, lotteryData, openTime)
        {
        }

        /// <summary>
        /// 期号（主键）- 重写以添加 SQLite 特性
        /// </summary>
        [PrimaryKey]
        public new int IssueId
        {
            get => base.IssueId;
            set => base.IssueId = value;
        }

        /// <summary>
        /// 当天第几期（1-203）- 重写以添加 SQLite 特性
        /// </summary>
        [Indexed]
        public new int DayIndex
        {
            get => base.DayIndex;
            private set => base.DayIndex = value;
        }

        // ========================================
        // 注意：计算属性（Items, P1-P5, PSum, DragonTiger 等）不需要重新声明
        // SQLite 会自动忽略复杂类型和没有 [Column] 特性的属性
        // 这些属性在基类中定义，继承后可以直接使用
        // ========================================

        /// <summary>
        /// 从 BinGoData 创建实体（复制数据）
        /// </summary>
        public static BinGoDataEntity FromBinGoData(BinGoData data)
        {
            if (data == null) return null!;
            
            var entity = new BinGoDataEntity();
            entity.FillLotteryData(data.IssueId, data.LotteryData, data.OpenTime);
            if (!string.IsNullOrEmpty(data.LastError))
            {
                entity.LastError = data.LastError;
            }
            return entity;
        }

        /// <summary>
        /// 转换为 BinGoData（返回自身，因为已经继承）
        /// </summary>
        public BinGoData ToBinGoData()
        {
            return this;
        }
    }
}

