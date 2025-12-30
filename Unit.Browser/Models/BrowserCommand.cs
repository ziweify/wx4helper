using System;

namespace Unit.Browser.Models
{
    /// <summary>
    /// 浏览器命令
    /// </summary>
    public class BrowserCommand
    {
        /// <summary>
        /// 命令唯一ID
        /// </summary>
        public string CommandId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 命令名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 命令参数
        /// </summary>
        public object? Parameters { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int TimeoutMs { get; set; } = 30000; // 默认30秒
    }
}

