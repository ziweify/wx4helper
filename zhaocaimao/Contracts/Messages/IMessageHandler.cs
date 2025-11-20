using System.Text.Json;
using System.Threading.Tasks;
using zhaocaimao.Models;

namespace zhaocaimao.Contracts.Messages
{
    /// <summary>
    /// 消息处理器接口
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// 处理器支持的消息类型
        /// </summary>
        ServerMessageType MessageType { get; }

        /// <summary>
        /// 处理消息（异步）
        /// </summary>
        /// <param name="data">消息数据（JSON）</param>
        Task HandleAsync(JsonElement data);
    }
}

