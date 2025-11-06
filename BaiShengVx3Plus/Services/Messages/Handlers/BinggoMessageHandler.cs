using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Models.Games.Binggo;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                
                // 3. 简单判断是否可能是下注消息（包含数字和关键词）
                if (!LooksLikeBetMessage(messageContent))
                {
                    return (false, null);
                }
                
                _logService.Info("BinggoMessageHandler", 
                    $"收到可能的下注消息: {member.Nickname} - {messageContent}");
                
                // 4. 获取当前期号和状态
                int currentIssueId = _lotteryService.CurrentIssueId;
                var currentStatus = _lotteryService.CurrentStatus;
                
                if (currentIssueId == 0)
                {
                    _logService.Warning("BinggoMessageHandler", "当前期号未初始化");
                    return (true, "系统初始化中，请稍后...");
                }
                
                // 5. 检查是否封盘
                if (currentStatus != BinggoLotteryStatus.开盘中)
                {
                    _logService.Info("BinggoMessageHandler", 
                        $"当前状态: {currentStatus}，不接受下注");
                    return (true, _settings.ReplySealed);
                }
                
                // 6. 调用订单服务创建订单
                var (success, message, order) = await _orderService.CreateOrderAsync(
                    member,
                    messageContent,
                    currentIssueId,
                    currentStatus);
                
                if (success)
                {
                    _logService.Info("BinggoMessageHandler", 
                        $"✅ 下注成功: {member.Nickname} - 期号: {currentIssueId}");
                }
                else
                {
                    _logService.Warning("BinggoMessageHandler", 
                        $"❌ 下注失败: {member.Nickname} - {message}");
                }
                
                return (true, message);
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
        
        /// <summary>
        /// 简单判断是否看起来像下注消息
        /// </summary>
        private bool LooksLikeBetMessage(string message)
        {
            // 必须包含数字
            if (!message.Any(char.IsDigit))
            {
                return false;
            }
            
            // 包含关键词
            string[] keywords = { "大", "小", "单", "双", "龙", "虎", 
                                 "尾大", "尾小", "合单", "合双",
                                 "一", "二", "三", "四", "五", "六", "总" };
            
            foreach (var keyword in keywords)
            {
                if (message.Contains(keyword))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}

