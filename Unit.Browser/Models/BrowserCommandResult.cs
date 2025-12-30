using System;

namespace Unit.Browser.Models
{
    /// <summary>
    /// 浏览器命令执行结果
    /// </summary>
    public class BrowserCommandResult
    {
        /// <summary>
        /// 对应的命令ID
        /// </summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 执行时长（毫秒）
        /// </summary>
        public long ExecutionTimeMs { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime CompletedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static BrowserCommandResult CreateSuccess(string commandId, object? data = null, long executionTimeMs = 0)
        {
            return new BrowserCommandResult
            {
                CommandId = commandId,
                Success = true,
                Data = data,
                ExecutionTimeMs = executionTimeMs
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static BrowserCommandResult CreateFailure(string commandId, string errorMessage, long executionTimeMs = 0)
        {
            return new BrowserCommandResult
            {
                CommandId = commandId,
                Success = false,
                ErrorMessage = errorMessage,
                ExecutionTimeMs = executionTimeMs
            };
        }
    }
}

