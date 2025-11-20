using zhaocaimao.Models;
using System;
using System.Collections.Generic;

namespace zhaocaimao.Contracts
{
    /// <summary>
    /// 日志服务接口
    /// 
    /// 设计理念：
    /// 1. 线程安全：多线程可以同时写日志
    /// 2. 异步写入：不阻塞主线程
    /// 3. 实时通知：通过事件实时更新UI
    /// 4. 混合存储：内存（快速查询）+ SQLite（持久化）
    /// </summary>
    public interface ILogService
    {
        // ========================================
        // 事件：实时通知
        // ========================================

        /// <summary>
        /// 新日志添加事件（用于实时UI更新）
        /// </summary>
        event EventHandler<LogEntry>? LogAdded;

        // ========================================
        // 写入方法
        // ========================================

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        void Trace(string source, string message, string? extraData = null);

        /// <summary>
        /// 记录调试日志
        /// </summary>
        void Debug(string source, string message, string? extraData = null);

        /// <summary>
        /// 记录信息日志
        /// </summary>
        void Info(string source, string message, string? extraData = null);

        /// <summary>
        /// 记录警告日志
        /// </summary>
        void Warning(string source, string message, string? extraData = null);

        /// <summary>
        /// 记录错误日志
        /// </summary>
        void Error(string source, string message, Exception? exception = null, string? extraData = null);

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        void Fatal(string source, string message, Exception? exception = null, string? extraData = null);

        /// <summary>
        /// 记录日志（通用方法）
        /// </summary>
        void Log(LogEntry entry);

        // ========================================
        // 查询方法
        // ========================================

        /// <summary>
        /// 获取最近的N条日志（从内存）
        /// </summary>
        IReadOnlyList<LogEntry> GetRecentLogs(int count = 100);

        /// <summary>
        /// 获取所有内存中的日志
        /// </summary>
        IReadOnlyList<LogEntry> GetAllMemoryLogs();

        /// <summary>
        /// 从数据库查询日志
        /// </summary>
        List<LogEntry> QueryLogs(
            DateTime? startTime = null,
            DateTime? endTime = null,
            LogLevel? minLevel = null,
            string? source = null,
            string? keyword = null,
            int limit = 1000);

        /// <summary>
        /// 获取日志统计
        /// </summary>
        LogStatistics GetStatistics();

        // ========================================
        // 管理方法
        // ========================================

        /// <summary>
        /// 清空内存日志
        /// </summary>
        void ClearMemoryLogs();

        /// <summary>
        /// 清空数据库日志
        /// </summary>
        void ClearDatabaseLogs();

        /// <summary>
        /// 设置最小日志级别
        /// </summary>
        void SetMinimumLevel(LogLevel level);

        /// <summary>
        /// 导出日志到文件
        /// </summary>
        /// <param name="limit">导出数量限制，默认10000条，避免100万+数据卡顿</param>
        Task ExportToFileAsync(string filePath, DateTime? startTime = null, DateTime? endTime = null, int limit = 10000);
    }

    /// <summary>
    /// 日志统计信息
    /// </summary>
    public class LogStatistics
    {
        public int TotalCount { get; set; }
        public int TraceCount { get; set; }
        public int DebugCount { get; set; }
        public int InfoCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public int FatalCount { get; set; }
        public DateTime? FirstLogTime { get; set; }
        public DateTime? LastLogTime { get; set; }

        public override string ToString()
        {
            return $"总计: {TotalCount}, 错误: {ErrorCount}, 警告: {WarningCount}, 信息: {InfoCount}";
        }
    }
}

