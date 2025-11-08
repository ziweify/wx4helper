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
        private readonly SQLiteConnection? _db;  // 🔥 数据库连接（用于上下分申请）
        
        /// <summary>
        /// 全局开关：是否启用订单处理（收单开关）
        /// </summary>
        public static bool IsOrdersTaskingEnabled { get; set; } = true;
        
        public BinggoMessageHandler(
            ILogService logService,
            IBinggoLotteryService lotteryService,
            IBinggoOrderService orderService,
            BinggoGameSettings settings,
            SQLiteConnection? db = null)  // 🔥 可选参数
        {
            _logService = logService;
            _lotteryService = lotteryService;
            _orderService = orderService;
            _settings = settings;
            _db = db;
        }
        
        /// <summary>
        /// 设置数据库连接（用于上下分申请）
        /// </summary>
        public void SetDatabase(SQLiteConnection db)
        {
            _db?.Close();
            // 使用反射设置私有字段
            var field = GetType().GetField("_db", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(this, db);
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
                // 0. 检查是否开启收单
                if(!ConfigurationManager.Instance.Configuration.IsOrdersTaskingEnabled)
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
                
                // 🔥 3. 优先处理查询命令（查、流水、货单）
                if (IsQueryCommand(messageContent))
                {
                    return (true, HandleQueryCommand(member));
                }
                
                // 🔥 4. 处理上分命令
                if (IsCreditCommand(messageContent))
                {
                    return (true, await HandleCreditCommandAsync(member, messageContent));
                }
                
                // 🔥 5. 处理下分命令
                if (IsWithdrawCommand(messageContent))
                {
                    return (true, await HandleWithdrawCommandAsync(member, messageContent));
                }
                
                // 🔥 6. 处理取消命令（取消当期待处理订单）
                if (IsCancelCommand(messageContent))
                {
                    return (true, await HandleCancelCommandAsync(member));
                }
                
                // 7. 简单判断是否可能是下注消息（包含数字和关键词）
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
                
                // 5. 调用订单服务创建订单
                // 🔥 封盘检查统一由 BinggoOrderValidator 处理，避免逻辑重复
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
        
        // ========================================
        // 🔥 命令处理方法
        // ========================================
        
        /// <summary>
        /// 判断是否是查询命令
        /// </summary>
        private bool IsQueryCommand(string message)
        {
            return message == "查" || message == "流水" || message == "货单";
        }
        
        /// <summary>
        /// 处理查询命令
        /// </summary>
        private string HandleQueryCommand(V2Member member)
        {
            try
            {
                // 参考 F5BotV2 (BoterServices.cs 第2174行)
                string reply = $"@{member.Nickname}\r流~~记录\r";
                reply += $"今日/本轮进货:{member.BetToday:F2}/{member.BetCur:F2}\r";
                reply += $"今日上/下:{member.CreditToday:F2}/{member.WithdrawToday:F2}\r";
                reply += $"今日盈亏:{member.IncomeToday:F2}\r";
                
                _logService.Info("BinggoMessageHandler", 
                    $"查询命令: {member.Nickname} - 今日下注:{member.BetToday:F2}, 盈亏:{member.IncomeToday:F2}");
                
                return reply;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoMessageHandler", "处理查询命令失败", ex);
                return "查询失败，请稍后重试";
            }
        }
        
        /// <summary>
        /// 判断是否是上分命令
        /// </summary>
        private bool IsCreditCommand(string message)
        {
            return Regex.IsMatch(message, @"^上(分)?(\d+)?$");
        }
        
        /// <summary>
        /// 处理上分命令
        /// </summary>
        private async Task<string> HandleCreditCommandAsync(V2Member member, string message)
        {
            try
            {
                // 解析金额
                var match = Regex.Match(message, @"^上(分)?(\d+)?$");
                if (!match.Groups[2].Success)
                {
                    return "请输入上分金额，例如：上1000";
                }
                
                float amount = float.Parse(match.Groups[2].Value);
                
                if (amount <= 0)
                {
                    return "上分金额必须大于0";
                }
                
                // 🔥 创建上分申请
                if (_db == null)
                {
                    _logService.Warning("BinggoMessageHandler", "数据库未初始化，无法创建上分申请");
                    return "系统错误，请联系管理员";
                }
                
                _db.CreateTable<V2CreditWithdraw>();
                
                var request = new V2CreditWithdraw
                {
                    GroupWxId = member.GroupWxId,
                    Wxid = member.Wxid,
                    Nickname = member.Nickname,
                    Amount = amount,
                    Action = CreditWithdrawAction.上分,
                    Status = CreditWithdrawStatus.等待处理,
                    TimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Notes = "会员申请上分"
                };
                
                _db.Insert(request);
                
                _logService.Info("BinggoMessageHandler", 
                    $"上分申请已创建: {member.Nickname} - {amount:F2}");
                
                // 🔥 回复格式参考 F5BotV2 (BoterServices.cs 第2605行)
                string reply = $"@{member.Nickname}\r[{member.Id}]请等待";
                
                return reply;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoMessageHandler", "处理上分命令失败", ex);
                return "上分申请失败，请稍后重试";
            }
        }
        
        /// <summary>
        /// 判断是否是下分命令
        /// </summary>
        private bool IsWithdrawCommand(string message)
        {
            return Regex.IsMatch(message, @"^下(分)?(\d+)?$");
        }
        
        /// <summary>
        /// 处理下分命令
        /// </summary>
        private async Task<string> HandleWithdrawCommandAsync(V2Member member, string message)
        {
            try
            {
                // 解析金额
                var match = Regex.Match(message, @"^下(分)?(\d+)?$");
                if (!match.Groups[2].Success)
                {
                    return "请输入下分金额，例如：下500";
                }
                
                float amount = float.Parse(match.Groups[2].Value);
                
                if (amount <= 0)
                {
                    return "下分金额必须大于0";
                }
                
                // 检查余额
                if (member.Balance < amount)
                {
                    return $"@{member.Nickname}\r余额不足！\r当前余额：{member.Balance:F2}";
                }
                
                // 🔥 创建下分申请
                if (_db == null)
                {
                    _logService.Warning("BinggoMessageHandler", "数据库未初始化，无法创建下分申请");
                    return "系统错误，请联系管理员";
                }
                
                _db.CreateTable<V2CreditWithdraw>();
                
                var request = new V2CreditWithdraw
                {
                    GroupWxId = member.GroupWxId,
                    Wxid = member.Wxid,
                    Nickname = member.Nickname,
                    Amount = amount,
                    Action = CreditWithdrawAction.下分,
                    Status = CreditWithdrawStatus.等待处理,
                    TimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Notes = "会员申请下分"
                };
                
                _db.Insert(request);
                
                _logService.Info("BinggoMessageHandler", 
                    $"下分申请已创建: {member.Nickname} - {amount:F2}");
                
                // 🔥 回复格式参考 F5BotV2 (BoterServices.cs 第2605行)
                string reply = $"@{member.Nickname}\r[{member.Id}]请等待";
                
                return reply;
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoMessageHandler", "处理下分命令失败", ex);
                return "下分申请失败，请稍后重试";
            }
        }
        
        /// <summary>
        /// 判断是否为取消命令
        /// </summary>
        private bool IsCancelCommand(string message)
        {
            message = message.Trim();
            return message == "取消" || message == "qx";
        }
        
        /// <summary>
        /// 处理取消命令
        /// 🔥 限制：只能取消当期、封盘前的待处理订单
        /// </summary>
        private async Task<string> HandleCancelCommandAsync(V2Member member)
        {
            try
            {
                // 1. 获取当前期号和状态
                int currentIssueId = _lotteryService.CurrentIssueId;
                var currentStatus = _lotteryService.CurrentStatus;
                
                if (currentIssueId == 0)
                {
                    return "系统初始化中，请稍后...";
                }
                
                // 2. 🔥 检查是否已封盘（只能在封盘前取消）
                if (currentStatus == BinggoLotteryStatus.封盘中 || currentStatus == BinggoLotteryStatus.开奖中)
                {
                    return $"@{member.Nickname}\r已封盘，无法取消订单";
                }
                
                // 3. 查找当期该会员的待处理订单
                var pendingOrders = _orderService.GetPendingOrdersForMemberAndIssue(member.Wxid, currentIssueId);
                
                if (pendingOrders == null || !pendingOrders.Any())
                {
                    return $"@{member.Nickname}\r当前期号无待处理订单";
                }
                
                // 4. 取消所有待处理订单
                int canceledCount = 0;
                foreach (var order in pendingOrders)
                {
                    order.OrderStatus = OrderStatus.已取消;
                    _orderService.UpdateOrder(order);
                    canceledCount++;
                }
                
                _logService.Info("BinggoMessageHandler", 
                    $"✅ 取消订单: {member.Nickname} - 期号:{currentIssueId} - 取消{canceledCount}个订单");
                
                // 5. 回复消息
                return $"@{member.Nickname}\r已取消{canceledCount}个订单";
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoMessageHandler", "处理取消命令失败", ex);
                return "取消订单失败，请联系管理员";
            }
        }
    }
}

