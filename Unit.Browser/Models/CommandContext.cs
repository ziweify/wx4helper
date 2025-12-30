using System.Threading.Tasks;

namespace Unit.Browser.Models
{
    /// <summary>
    /// 命令执行上下文（内部使用）
    /// </summary>
    internal class CommandContext
    {
        /// <summary>
        /// 命令对象
        /// </summary>
        public BrowserCommand Command { get; set; } = null!;

        /// <summary>
        /// 任务完成源（用于异步等待）
        /// </summary>
        public TaskCompletionSource<BrowserCommandResult> CompletionSource { get; set; } = new();
    }
}

