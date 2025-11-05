using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Messages;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    /// <summary>
    /// 聊天消息处理器
    /// </summary>
    public class ChatMessageHandler : IMessageHandler
    {
        private readonly ILogService _logService;

        public ServerMessageType MessageType => ServerMessageType.OnMessage;

        public ChatMessageHandler(ILogService logService)
        {
            _logService = logService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                // 反序列化为具体类型
                var message = JsonSerializer.Deserialize<ChatMessageData>(data.GetRawText());
                if (message == null)
                {
                    _logService.Error("ChatMessageHandler", "Failed to deserialize message");
                    return;
                }

                _logService.Info("ChatMessageHandler", 
                    $"💬 收到消息 | 发送者: {message.Sender} | 接收者: {message.Receiver} | 内容: {message.Content}");

                // TODO: 在这里处理聊天消息
                // 1. 保存到数据库
                // 2. 更新 UI 显示
                // 3. 触发业务逻辑（如自动回复）

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("ChatMessageHandler", "Error handling chat message", ex);
            }
        }
    }
}

