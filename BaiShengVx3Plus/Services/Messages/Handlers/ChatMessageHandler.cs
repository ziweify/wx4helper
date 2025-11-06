using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Messages;
using BaiShengVx3Plus.Services.Games.Binggo;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    /// <summary>
    /// 聊天消息处理器
    /// 
    /// 功能：
    /// 1. 接收微信群消息
    /// 2. 调用 BinggoMessageHandler 处理下注消息
    /// 3. 发送回复消息到微信
    /// </summary>
    public class ChatMessageHandler : IMessageHandler
    {
        private readonly ILogService _logService;
        private readonly IWeixinSocketClient _socketClient;
        private readonly BinggoMessageHandler _binggoMessageHandler;
        private readonly IMemberDataService _memberDataService;

        public ServerMessageType MessageType => ServerMessageType.OnMessage;

        public ChatMessageHandler(
            ILogService logService,
            IWeixinSocketClient socketClient,
            BinggoMessageHandler binggoMessageHandler,
            IMemberDataService memberDataService)
        {
            _logService = logService;
            _socketClient = socketClient;
            _binggoMessageHandler = binggoMessageHandler;
            _memberDataService = memberDataService;
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

                // ========================================
                // 🎮 炳狗下注消息处理
                // ========================================
                
                // 1. 检查是否为群消息
                if (string.IsNullOrEmpty(message.Receiver) || !message.Receiver.Contains("@chatroom"))
                {
                    _logService.Debug("ChatMessageHandler", "非群消息，跳过炳狗处理");
                    return;
                }
                
                // 2. 获取发送者会员信息（从 dgvMembers 中查找）
                var member = GetMemberByWxid(message.Sender);
                if (member == null)
                {
                    _logService.Debug("ChatMessageHandler", $"未找到会员: {message.Sender}，跳过炳狗处理");
                    return;
                }
                
                // 3. 调用炳狗消息处理器
                var (handled, replyMessage) = await _binggoMessageHandler.HandleMessageAsync(
                    member, 
                    message.Content);
                
                // 4. 如果已处理，发送回复消息
                if (handled && !string.IsNullOrEmpty(replyMessage))
                {
                    await SendWeChatReplyAsync(message.Receiver, replyMessage);
                    _logService.Info("ChatMessageHandler", 
                        $"✅ 已回复: {replyMessage.Substring(0, Math.Min(50, replyMessage.Length))}...");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("ChatMessageHandler", "Error handling chat message", ex);
            }
        }
        
        /// <summary>
        /// 根据 wxid 获取会员信息
        /// </summary>
        private V2Member? GetMemberByWxid(string wxid)
        {
            return _memberDataService.GetMemberByWxid(wxid);
        }
        
        /// <summary>
        /// 发送回复消息到微信群
        /// </summary>
        private async Task SendWeChatReplyAsync(string toWxid, string message)
        {
            try
            {
                // 构造 SendText 命令
                var command = new
                {
                    command = "SendText",
                    wxid = toWxid,
                    message = message
                };
                
                var commandJson = JsonSerializer.Serialize(command);
                
                // 通过 Socket 发送（使用默认超时）
                var response = await _socketClient.SendAsync<string>(commandJson);
                
                if (response != "(null)")
                {
                    _logService.Info("ChatMessageHandler", $"✅ 消息已发送: {response}");
                }
                else
                {
                    _logService.Warning("ChatMessageHandler", "消息发送返回 null");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("ChatMessageHandler", $"发送消息失败: {ex.Message}", ex);
            }
        }
    }
}
