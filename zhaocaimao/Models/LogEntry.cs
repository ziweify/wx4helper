using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zhaocaimao.Models
{
    /// <summary>
    /// 日志条目模型
    /// </summary>
    public class LogEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 日志ID（自增）
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// 来源（哪个模块、类）
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 异常信息（可选）
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// 线程ID
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// 用户ID（可选）
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// 额外数据（JSON格式）
        /// </summary>
        public string? ExtraData { get; set; }

        /// <summary>
        /// 级别显示名称
        /// </summary>
        [DisplayName("级别")]
        public string LevelName => Level switch
        {
            LogLevel.Trace => "跟踪",
            LogLevel.Debug => "调试",
            LogLevel.Info => "信息",
            LogLevel.Warning => "警告",
            LogLevel.Error => "错误",
            LogLevel.Fatal => "致命",
            _ => "未知"
        };

        /// <summary>
        /// 格式化的时间
        /// </summary>
        [DisplayName("时间")]
        public string FormattedTime => Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

        /// <summary>
        /// 构造函数
        /// </summary>
        public LogEntry()
        {
            Timestamp = DateTime.Now;
            ThreadId = Environment.CurrentManagedThreadId;
        }

        /// <summary>
        /// 转换为文本格式
        /// </summary>
        public string ToText()
        {
            var exceptionInfo = string.IsNullOrEmpty(Exception) ? "" : $"\n异常: {Exception}";
            return $"[{FormattedTime}] [{LevelName}] [{Source}] [线程{ThreadId}] {Message}{exceptionInfo}";
        }

        public override string ToString()
        {
            return $"[{LevelName}] {Message}";
        }
    }

    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 跟踪（最详细）
        /// </summary>
        Trace = 0,

        /// <summary>
        /// 调试
        /// </summary>
        Debug = 1,

        /// <summary>
        /// 信息
        /// </summary>
        Info = 2,

        /// <summary>
        /// 警告
        /// </summary>
        Warning = 3,

        /// <summary>
        /// 错误
        /// </summary>
        Error = 4,

        /// <summary>
        /// 致命错误
        /// </summary>
        Fatal = 5
    }
}

