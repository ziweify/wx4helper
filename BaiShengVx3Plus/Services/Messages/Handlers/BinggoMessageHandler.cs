using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Models.Games.Binggo;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SQLite;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    /// <summary>
    /// 炳狗下注消息处理器
    /// 
    /// 功能：
    /// 1. 接收微信群消息
    /// 2. 判断是否为下注消息
    /// 3. 调用订单服务创建订单
    /// 4. 返回回复消息
    /// </summary>
    public class BinggoMessageHandler
    {
        private readonly ILogService _logService;
        private readonly IBinggoLotteryService _lotteryService;
        private readonly IBinggoOrderService _orderService;
        private readonly AdminCommandHandler _adminCommandHandler; // 🔥 新增：管理员命令处理器
        
        /// <summary>
        /// 全局开关：是否启用订单处理（收单开关）
        /// </summary>
        public static bool IsOrdersTaskingEnabled { get; set; } = true;
        
        public BinggoMessageHandler(
            ILogService logService,
            IBinggoLotteryService lotteryService,
            IBinggoOrderService orderService,
            AdminCommandHandler adminCommandHandler) // 🔥 新增：注入管理员命令处理器
        {
            _logService = logService;
            _lotteryService = lotteryService;
            _orderService = orderService;
            _adminCommandHandler = adminCommandHandler; // 🔥 新增
        }
        
        /// <summary>
        /// 处理群消息，判断是否为下注消息
        /// </summary>
        /// <param name="member">发送消息的会员</param>
        /// <param name="messageContent">消息内容</param>
        /// <param name="groupWxid">群wxid（用于管理员命令）</param>
        /// <param name="currentUserWxid">当前登录用户的wxid（用于判断是否自己发送）</param>
        /// <returns>(是否处理, 回复消息)</returns>
        public async Task<(bool handled, string? replyMessage)> HandleMessageAsync(
            V2Member member, 
            string messageContent,
            string groupWxid = "",
            string currentUserWxid = "")
        {
            try
            {
                // 1. 基础检查
                if (member == null || string.IsNullOrWhiteSpace(messageContent))
                {
                    return (false, null);
                }
                
                // 🔥 检查是否为上下分命令（上下分不受收单开关影响）
                // 参考 F5BotV2：收单开关只影响投注订单，不影响上下分、查询等操作
                string trimmedMsg = messageContent.Trim();
                bool isCreditWithdrawCommand = Regex.IsMatch(trimmedMsg, @"^[上下]\d+$");
                
                // ✅ 检查是否开启收单（使用静态属性，由 VxMain 同步更新）
                // 🔥 注意：上下分、查询命令不受收单开关影响
                // 🔥 取消命令受收单开关影响：关闭后不能取消，开启才能接收取消命令
                if(!IsOrdersTaskingEnabled && !isCreditWithdrawCommand)
                {
                    // 🔥 如果是查询命令，允许通过
                    if (trimmedMsg != "查" && trimmedMsg != "流水" && trimmedMsg != "货单")
                    {
                        return (false, null);
                    }
                }
               
                
                // 🔥 2. 管理员权限检查 - 参考 F5BotV2 Line 2014-2075
                bool isAdmin = member.State == MemberState.管理 || member.Wxid == currentUserWxid;
                
                if (isAdmin)
                {
                    _logService.Info("BinggoMessageHandler", $"检测到管理员消息: {member.Nickname} ({member.Wxid})");
                    
                    // 🔥 2.1 处理刷新命令
                    var (refreshCode, refreshReply, refreshError) = await _adminCommandHandler.HandleRefreshCommand(groupWxid, messageContent);
                    if (refreshCode != -1)
                    {
                        // 🔥 刷新命令已在内部发送消息，如果成功（code==0）且没有错误，返回已处理但不需要回复
                        if (refreshCode == 0)
                        {
                            if (!string.IsNullOrEmpty(refreshReply))
                            {
                                return (true, refreshReply);
                            }
                            else
                            {
                                // 🔥 消息已在内部发送，返回已处理但不需要回复
                                return (true, null);
                            }
                        }
                        else if (!string.IsNullOrEmpty(refreshError))
                        {
                            return (true, refreshError);
                        }
                    }
                    
                    // 🔥 2.2 处理管理上下分命令
                    var (creditCode, creditReply, creditError) = await _adminCommandHandler.HandleCreditWithdrawCommand(groupWxid, messageContent);
                    if (creditCode != -1)
                    {
                        if (creditCode == 0 && !string.IsNullOrEmpty(creditReply))
                        {
                            return (true, creditReply);
                        }
                        else if (!string.IsNullOrEmpty(creditError))
                        {
                            return (true, creditError);
                        }
                    }
                    
                    // 🔥 管理员的其他消息不处理（不当作普通命令）
                    // 这样管理员可以在群里正常聊天，不会触发投注等命令
                    _logService.Info("BinggoMessageHandler", "管理员消息不匹配任何管理命令，忽略");
                    return (false, null);
                }
                
                // 3. 过滤不需要处理的消息（普通会员）
                if (ShouldIgnoreMessage(messageContent))
                {
                    return (false, null);
                }
                
                // 🔥 4. 统一通过 BinggoLotteryService 处理所有普通会员消息
                // 包括：查、上分、下分、取消、投注
                // 所有状态验证、订单创建、回复消息生成都在服务中统一处理
                var (handled, replyMessage, order) = await _lotteryService.ProcessMessageAsync(
                    member,
                    messageContent);
                
                return (handled, replyMessage);
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoMessageHandler", 
                    $"处理消息失败: {ex.Message}", ex);
                return (true, "系统错误，请稍后重试");
            }
        }
        
        /// <summary>
        /// 判断是否应该忽略此消息（普通会员）
        /// </summary>
        private bool ShouldIgnoreMessage(string message)
        {
            // 过滤系统消息
            if (message.StartsWith("["))
            {
                return true;
            }
            
            // 🔥 注意：不过滤 @ 开头的消息（管理员命令需要）
            // 管理员命令会在前面的 isAdmin 分支中处理
            // 这里只是普通会员的消息过滤
            
            // 过滤表情和图片
            if (message.Contains("<msg>") || message.Contains("<img"))
            {
                return true;
            }
            
            // 🔥 允许单字符命令（如"查"），但过滤其他太短的消息（少于1个字符）
            // "查"命令是合法的单字符命令，不应该被过滤
            if (message.Length < 1)
            {
                return true;
            }
            
            // 🔥 特殊处理：如果消息是"查"、"流水"、"货单"等查询命令，不应该被过滤
            // 这些命令会在 ProcessMessageAsync 中处理
            string trimmedMsg = message.Trim();
            if (trimmedMsg == "查" || trimmedMsg == "流水" || trimmedMsg == "货单")
            {
                return false;  // 不过滤查询命令
            }
            
            // 过滤其他太短的消息（少于2个字符，但查询命令除外）
            if (message.Length < 2)
            {
                return true;
            }
            
            return false;
        }
        
        // 🔥 所有命令处理逻辑已移至 BinggoLotteryService.ProcessMessageAsync
        // 这里只保留消息过滤逻辑
    }
}

