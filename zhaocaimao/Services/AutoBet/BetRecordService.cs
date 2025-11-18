using System;
using System.Collections.Generic;
using System.Linq;
using zhaocaimao.Contracts;
using zhaocaimao.Core;
using zhaocaimao.Models.AutoBet;

namespace zhaocaimao.Services.AutoBet
{
    /// <summary>
    /// 投注记录服务（重构版 - 遵循 F5BotV2 设计）
    /// 
    /// 核心改变：
    /// 1. ❌ 不再直接访问数据库
    /// 2. ✅ 只操作 BetRecordBindingList
    /// 3. ✅ 只负责业务逻辑，数据库操作由 BindingList 自动处理
    /// </summary>
    public class BetRecordService
    {
        private readonly ILogService _log;
        private BetRecordBindingList? _records;
        
        public BetRecordService(ILogService log)
        {
            _log = log;
        }
        
        /// <summary>
        /// 设置 BindingList（不再需要数据库连接）
        /// </summary>
        public void SetBindingList(BetRecordBindingList records)
        {
            _records = records;
            _log.Info("BetRecordService", "✅ 投注记录服务已初始化（使用 BindingList）");
        }
        
        /// <summary>
        /// 创建投注记录
        /// 🔥 只需添加到 BindingList，自动保存到数据库
        /// </summary>
        public BetRecord? Create(BetRecord record)
        {
            if (_records == null)
            {
                _log.Error("BetRecordService", "❌ BindingList 未初始化，无法创建投注记录");
                return null;
            }
            
            // 🔥 直接添加到 BindingList，自动触发数据库保存
            _records.Add(record);
            
            _log.Info("BetRecordService", $"📝 创建投注记录:ID={record.Id} 来源={record.Source} 内容={record.BetContentStandard}");
            
            return record;
        }
        
        /// <summary>
        /// 更新投注记录
        /// 🔥 直接修改对象属性，自动触发 PropertyChanged → 自动保存
        /// </summary>
        public void Update(BetRecord record)
        {
            if (_records == null)
            {
                _log.Error("BetRecordService", "❌ BindingList 未初始化，无法更新投注记录");
                return;
            }
            
            // 🔥 修改属性会自动触发 PropertyChanged → 自动保存
            record.UpdateTime = DateTime.Now;
            
            // 触发通知（如果需要强制刷新 UI）
            _records.NotifyItemChanged(record);
            
            _log.Info("BetRecordService", 
                $"✅ 更新投注记录:ID={record.Id} 成功={record.Success} 耗时={record.DurationMs}ms");
        }
        
        /// <summary>
        /// 更新投注记录（投注结果返回后）
        /// </summary>
        public void UpdateResult(int recordId, bool success, string? result, string? errorMessage, 
            DateTime? postStartTime, DateTime? postEndTime, string? orderNo)
        {
            if (_records == null)
            {
                _log.Error("BetRecordService", "❌ BindingList 未初始化，无法更新投注结果");
                return;
            }
            
            var record = _records.GetById(recordId);
            if (record == null)
            {
                _log.Warning("BetRecordService", $"⚠️ 投注记录不存在:ID={recordId}");
                return;
            }
            
            // 🔥 直接修改属性，自动触发保存
            record.Success = success;
            record.Result = result;
            record.ErrorMessage = errorMessage;
            record.PostStartTime = postStartTime;
            record.PostEndTime = postEndTime;
            record.OrderNo = orderNo;
            
            // Update 方法会触发 UpdateTime 和通知
            Update(record);
        }
        
        /// <summary>
        /// 获取投注记录
        /// 🔥 直接从 BindingList 查询（内存），不访问数据库
        /// </summary>
        public BetRecord? GetById(int id)
        {
            return _records?.GetById(id);
        }
        
        /// <summary>
        /// 获取指定期号的投注记录
        /// 🔥 直接从 BindingList 查询（内存），不访问数据库
        /// </summary>
        public BetRecord[] GetByIssueId(int issueId)
        {
            if (_records == null) return Array.Empty<BetRecord>();
            
            return _records.GetByIssueId(issueId);
        }
        
        /// <summary>
        /// 获取指定配置的投注记录
        /// 🔥 直接从 BindingList 查询（内存），不访问数据库
        /// </summary>
        public BetRecord[] GetByConfigId(int configId, int limit = 100)
        {
            if (_records == null) return Array.Empty<BetRecord>();
            
            return _records.GetByConfigId(configId)
                .OrderByDescending(r => r.CreateTime)
                .Take(limit)
                .ToArray();
        }
        
        /// <summary>
        /// 获取指定配置和日期范围的投注记录
        /// 🔥 直接从 BindingList 查询（内存），不访问数据库
        /// </summary>
        public BetRecord[] GetByConfigAndDateRange(int configId, DateTime startDate, DateTime endDate)
        {
            if (_records == null) return Array.Empty<BetRecord>();
            
            return _records.GetByConfigId(configId)
                .Where(r => r.CreateTime >= startDate && r.CreateTime <= endDate)
                .OrderByDescending(r => r.CreateTime)
                .ToArray();
        }
        
        /// <summary>
        /// 检查是否存在待处理的投注（用于防重复）
        /// 🔥 直接从 BindingList 查询（内存），不访问数据库
        /// </summary>
        public bool HasPendingBet(int configId, int issueId)
        {
            if (_records == null) return false;
            
            return _records.HasPendingBet(configId, issueId);
        }
    }
}
