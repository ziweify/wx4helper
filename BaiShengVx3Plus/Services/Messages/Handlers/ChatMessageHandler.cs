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
        private readonly IUserInfoService _userInfoService; // 🔥 新增：获取当前用户信息

        public ServerMessageType MessageType => ServerMessageType.OnMessage;

        public ChatMessageHandler(
            ILogService logService,
            IWeixinSocketClient socketClient,
            BinggoMessageHandler binggoMessageHandler,
            IMemberDataService memberDataService,
            IUserInfoService userInfoService) // 🔥 新增
        {
            _logService = logService;
            _socketClient = socketClient;
            _binggoMessageHandler = binggoMessageHandler;
            _memberDataService = memberDataService;
            _userInfoService = userInfoService; // 🔥 新增
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
                if (!message.FromChatroom || !message.Receiver1.Contains("@chatroom"))
                {
                    _logService.Debug("ChatMessageHandler", "非群消息，跳过炳狗处理");
                    return;
                }
                
                // 2. 🔥 检查收单开关（必须先检查！）
                _logService.Debug("ChatMessageHandler", $"🔍 检查收单开关: IsOrdersTaskingEnabled = {BinggoMessageHandler.IsOrdersTaskingEnabled}");
                if (!BinggoMessageHandler.IsOrdersTaskingEnabled)
                {
                    _logService.Info("ChatMessageHandler", "⏸️ 收单已关闭，忽略群消息");
                    return;
                }
                
                // 3. 获取发送者会员信息（从 dgvMembers 中查找）
                var member = GetMemberByWxid(message.Sender);
                if (member == null)
                {
                    _logService.Debug("ChatMessageHandler", $"未找到会员: {message.Sender}，跳过炳狗处理");
                    return;
                }
                
                // 🔥 获取当前用户 wxid（用于管理员判断）
                string currentUserWxid = _userInfoService.GetCurrentWxid();
                
                // 4. 调用炳狗消息处理器（传递群ID和当前用户ID）
                var (handled, replyMessage) = await _binggoMessageHandler.HandleMessageAsync(
                    member, 
                    message.Content,
                    message.Receiver1,  // 🔥 群ID
                    currentUserWxid);   // 🔥 当前用户ID
                
                // 4. 如果已处理，发送回复消息
                if (handled && !string.IsNullOrEmpty(replyMessage))
                {
                    // 🔥 修复：使用 Receiver1（群ID）而不是 Receiver
                    string replyTo = message.Receiver1;  // 群ID
                    
                    _logService.Info("ChatMessageHandler", 
                        $"准备回复到群: {replyTo}, 消息: {replyMessage.Substring(0, Math.Min(50, replyMessage.Length))}...");
                    
                    await SendWeChatReplyAsync(replyTo, replyMessage);
                    
                    _logService.Info("ChatMessageHandler", 
                        $"✅ 已发送回复");
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
        /// 🔥 修复：使用正确的 JSON-RPC 调用方式
        /// </summary>
        private async Task SendWeChatReplyAsync(string toWxid, string message)
        {
            try
            {
                _logService.Info("ChatMessageHandler", 
                    $"🔥 开始发送回复 | 目标: {toWxid} | 消息: {message}");
                
                // 🔥 修复：使用正确的方法签名
                // SendAsync<TResult>(string method, params object[] parameters)
                // C++ 端注册的命令是 "SendMessage"，参数是 (wxid, message)
                var response = await _socketClient.SendAsync<object>("SendMessage", toWxid, message);
                
                if (response != null)
                {
                    _logService.Info("ChatMessageHandler", $"✅ 消息已成功发送到微信");
                }
                else
                {
                    _logService.Warning("ChatMessageHandler", "⚠️ 消息发送返回 null");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("ChatMessageHandler", $"❌ 发送消息失败: {ex.Message}", ex);
            }
        }
    }
}
