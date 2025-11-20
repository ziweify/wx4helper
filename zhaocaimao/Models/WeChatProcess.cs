using System;

namespace zhaocaimao.Models
{
    /// <summary>
    /// 微信进程信息（领域模型）
    /// 这是一个完整的面向对象的数据模型
    /// </summary>
    public class WeChatProcess
    {
        /// <summary>
        /// 进程ID
        /// </summary>
        public uint ProcessId { get; set; }

        /// <summary>
        /// 进程句柄
        /// </summary>
        public IntPtr ProcessHandle { get; set; }

        /// <summary>
        /// 是否已注入
        /// </summary>
        public bool IsInjected { get; set; }

        /// <summary>
        /// 注入时间
        /// </summary>
        public DateTime? InjectedAt { get; set; }

        /// <summary>
        /// 注入的DLL路径
        /// </summary>
        public string? InjectedDllPath { get; set; }

        /// <summary>
        /// 进程启动时间
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// 进程状态
        /// </summary>
        public WeChatProcessStatus Status { get; set; }

        /// <summary>
        /// 最后心跳时间
        /// </summary>
        public DateTime? LastHeartbeat { get; set; }

        /// <summary>
        /// 进程是否存活
        /// </summary>
        public bool IsAlive => Status == WeChatProcessStatus.Running;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WeChatProcess(uint processId)
        {
            ProcessId = processId;
            StartedAt = DateTime.Now;
            Status = WeChatProcessStatus.Running;
        }

        /// <summary>
        /// 标记为已注入
        /// </summary>
        public void MarkAsInjected(string dllPath)
        {
            IsInjected = true;
            InjectedAt = DateTime.Now;
            InjectedDllPath = dllPath;
        }

        /// <summary>
        /// 更新心跳
        /// </summary>
        public void UpdateHeartbeat()
        {
            LastHeartbeat = DateTime.Now;
        }

        /// <summary>
        /// 检查心跳超时
        /// </summary>
        public bool IsHeartbeatTimeout(int timeoutSeconds = 60)
        {
            if (!LastHeartbeat.HasValue) return false;
            return (DateTime.Now - LastHeartbeat.Value).TotalSeconds > timeoutSeconds;
        }

        public override string ToString()
        {
            return $"Process {ProcessId} - {Status} - Injected: {IsInjected}";
        }
    }

    /// <summary>
    /// 微信进程状态枚举
    /// </summary>
    public enum WeChatProcessStatus
    {
        /// <summary>
        /// 启动中
        /// </summary>
        Starting,

        /// <summary>
        /// 运行中
        /// </summary>
        Running,

        /// <summary>
        /// 已停止
        /// </summary>
        Stopped,

        /// <summary>
        /// 异常
        /// </summary>
        Error
    }
}

