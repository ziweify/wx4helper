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
        private readonly BinggoGameSettings _settings;
        
        /// <summary>
        /// 全局开关：是否启用订单处理（收单开关）
        /// </summary>
        public static bool IsOrdersTaskingEnabled { get; set; } = true;
        
        public BinggoMessageHandler(
            ILogService logService,
            IBinggoLotteryService lotteryService,
            IBinggoOrderService orderService,
            BinggoGameSettings settings)
        {
            _logService = logService;
            _lotteryService = lotteryService;
            _orderService = orderService;
            _settings = settings;
        }
        
        /// <summary>
        /// 处理群消息，判断是否为下注消息
        /// </summary>
        /// <param name="member">发送消息的会员</param>
        /// <param name="messageContent">消息内容</param>
        /// <returns>(是否处理, 回复消息)</returns>
        public async Task<(bool handled, string? replyMessage)> HandleMessageAsync(
            V2Member member, 
            string messageContent)
        {
            try
            {
                // ✅ 检查是否开启收单（使用静态属性，由 VxMain 同步更新）
                if(!IsOrdersTaskingEnabled)
                {
                    return (false, null);
                }
               

                // 1. 基础检查
                if (member == null || string.IsNullOrWhiteSpace(messageContent))
                {
                    return (false, null);
                }
                
                // 2. 过滤不需要处理的消息
                if (ShouldIgnoreMessage(messageContent))
                {
                    return (false, null);
                }
                
                // 🔥 统一通过 BinggoLotteryService 处理所有消息
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
        /// 判断是否应该忽略此消息
        /// </summary>
        private bool ShouldIgnoreMessage(string message)
        {
            // 过滤系统消息
            if (message.StartsWith("[") || message.StartsWith("@"))
            {
                return true;
            }
            
            // 过滤表情和图片
            if (message.Contains("<msg>") || message.Contains("<img"))
            {
                return true;
            }
            
            // 过滤太短的消息（少于2个字符）
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

