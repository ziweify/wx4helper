using System;
using System.Collections.Generic;
using System.Linq;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models.AutoBet;
using SQLite;

namespace BaiShengVx3Plus.Services.AutoBet
{
    /// <summary>
    /// 投注记录服务
    /// </summary>
    public class BetRecordService
    {
        private readonly ILogService _log;
        private SQLiteConnection? _db;
        
        public BetRecordService(ILogService log)
        {
            _log = log;
        }
        
        /// <summary>
        /// 设置数据库连接
        /// </summary>
        public void SetDatabase(SQLiteConnection db)
        {
            _db = db;
            _db.CreateTable<BetRecord>();
            _log.Info("BetRecordService", "✅ 投注记录表已初始化");
        }
        
        /// <summary>
        /// 创建投注记录
        /// </summary>
        public BetRecord Create(BetRecord record)
        {
            if (_db == null) throw new InvalidOperationException("数据库未初始化");
            
            record.CreateTime = DateTime.Now;
            record.SendTime = DateTime.Now;
            
            _db.Insert(record);
            
            _log.Info("BetRecordService", $"📝 创建投注记录:ID={record.Id} 来源={record.Source} 内容={record.BetContentStandard}");
            
            return record;
        }
        
        /// <summary>
        /// 更新投注记录（投注结果返回后）
        /// </summary>
        public void UpdateResult(int recordId, bool success, string? result, string? errorMessage, 
            DateTime? postStartTime, DateTime? postEndTime, string? orderNo)
        {
            if (_db == null) throw new InvalidOperationException("数据库未初始化");
            
            var record = _db.Get<BetRecord>(recordId);
            if (record == null)
            {
                _log.Warning("BetRecordService", $"⚠️ 投注记录不存在:ID={recordId}");
                return;
            }
            
            record.Success = success;
            record.Result = result;
            record.ErrorMessage = errorMessage;
            record.PostStartTime = postStartTime;
            record.PostEndTime = postEndTime;
            record.OrderNo = orderNo;
            record.UpdateTime = DateTime.Now;
            
            // 计算耗时
            if (postStartTime.HasValue && postEndTime.HasValue)
            {
                record.DurationMs = (int)(postEndTime.Value - postStartTime.Value).TotalMilliseconds;
            }
            
            _db.Update(record);
            
            _log.Info("BetRecordService", 
                $"✅ 更新投注记录:ID={recordId} 成功={success} 耗时={record.DurationMs}ms");
        }
        
        /// <summary>
        /// 获取投注记录
        /// </summary>
        public BetRecord? GetById(int id)
        {
            if (_db == null) return null;
            
            try
            {
                return _db.Get<BetRecord>(id);
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// 获取指定期号的投注记录
        /// </summary>
        public List<BetRecord> GetByIssueId(int issueId)
        {
            if (_db == null) return new List<BetRecord>();
            
            return _db.Table<BetRecord>()
                .Where(r => r.IssueId == issueId)
                .OrderByDescending(r => r.CreateTime)
                .ToList();
        }
        
        /// <summary>
        /// 获取指定配置的投注记录
        /// </summary>
        public List<BetRecord> GetByConfigId(int configId, int limit = 100)
        {
            if (_db == null) return new List<BetRecord>();
            
            return _db.Table<BetRecord>()
                .Where(r => r.ConfigId == configId)
                .OrderByDescending(r => r.CreateTime)
                .Take(limit)
                .ToList();
        }
        
        /// <summary>
        /// 检查是否存在待处理的投注（用于防重复）
        /// </summary>
        public bool HasPendingBet(int configId, int issueId)
        {
            if (_db == null) return false;
            
            return _db.Table<BetRecord>()
                .Any(r => r.ConfigId == configId && 
                         r.IssueId == issueId && 
                         r.Success == null);  // Success=null 表示未返回结果
        }
    }
}

