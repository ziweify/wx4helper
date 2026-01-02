using System;
using System.Linq;
using zhaocaimao.Contracts;
using zhaocaimao.Models;
using zhaocaimao.Helpers;  // 🔥 添加 StringHelper.UnEscape()
using SQLite;

namespace zhaocaimao.Services.Games.Binggo
{
    /// <summary>
    /// 上下分服务 - 统一处理所有上下分相关逻辑
    /// 优化设计：
    /// 1. 统一的处理入口（ProcessCreditWithdraw）
    /// 2. 统一的数据保存
    /// 3. 统一的统计更新
    /// 4. 统一的通知发送
    /// 5. 加载时自动恢复统计
    /// </summary>
    public class CreditWithdrawService
    {
        private readonly SQLiteConnection _db;
        private readonly ILogService _logService;
        private readonly IWeixinSocketClient? _socketClient;
        private readonly BinggoStatisticsService _statisticsService;
        private readonly Services.Sound.SoundService? _soundService;  // 🔥 声音播放服务（可选）
        private Core.V2CreditWithdrawBindingList? _creditWithdrawsBindingList;  // 🔥 上下分内存表
        
        // 🔥 应用级别的锁：保护会员余额、上下分记录的同步写入
        // 参考用户要求："所有会员表，订单表的操作，要变成同步操作。而且是应用级别的同步"
        // 
        // 🔥 重要变更：使用全局锁管理类（Core.ResourceLocks）
        // 原因：不同类中的 static readonly object 是独立的对象，无法互相保护
        // 解决：使用 Core.ResourceLocks.MemberBalanceLock 确保与下注、结算等操作互斥
        // 
        // 🔥 不再定义本地锁对象，直接使用 Core.ResourceLocks.MemberBalanceLock

        public CreditWithdrawService(
            SQLiteConnection db,
            ILogService logService,
            BinggoStatisticsService statisticsService,
            IWeixinSocketClient? socketClient = null,
            Services.Sound.SoundService? soundService = null)  // 🔥 声音服务（可选）
        {
            _db = db;
            _logService = logService;
            _statisticsService = statisticsService;
            _socketClient = socketClient;
            _soundService = soundService;
            
            // 确保表存在
            _db.CreateTable<V2CreditWithdraw>();
            _db.CreateTable<V2BalanceChange>();
        }
        
        /// <summary>
        /// 设置上下分 BindingList（内存表）
        /// 🔥 用户要求："订单只能从内存表中拿，改数据都改内存表，内存表修改即保存"
        /// </summary>
        public void SetCreditWithdrawsBindingList(Core.V2CreditWithdrawBindingList? bindingList)
        {
            _creditWithdrawsBindingList = bindingList;
        }

        /// <summary>
        /// 🔥 统一的上下分处理入口
        /// </summary>
        /// <param name="request">上下分申请</param>
        /// <param name="member">会员对象</param>
        /// <param name="isLoading">是否是加载历史数据（true=加载，不发通知；false=实时处理，发通知）</param>
        /// <returns>处理结果</returns>
        public (bool success, string? errorMessage) ProcessCreditWithdraw(
            V2CreditWithdraw request,
            V2Member member,
            bool isLoading = false)
        {
            try
            {
                // 🔥 1. 验证
                if (request.Status != CreditWithdrawStatus.等待处理 && !isLoading)
                {
                    return (false, "该申请已处理");
                }

                string actionName = request.Action == CreditWithdrawAction.上分 ? "上分" : "下分";
                float balanceBefore;
                float balanceAfter;
                V2BalanceChange balanceChange;

                // 🔥 2. 使用应用级别的锁保护会员余额的同步更新（上下分）
                // 参考用户要求："锁要注意时机，不能锁定太长时间，只锁定写入数据库数据这里"
                lock (Core.ResourceLocks.MemberBalanceLock)
                {
                    balanceBefore = member.Balance;
                    
                    _logService.Info("CreditWithdrawService", 
                        $"🔒 [{actionName}] {member.Nickname} - 操作前余额: {balanceBefore:F2}");

                    // 2.1 根据动作类型处理
                    if (request.Action == CreditWithdrawAction.上分)
                    {
                        // 上分：增加余额
                        balanceAfter = balanceBefore + request.Amount;
                        member.Balance = balanceAfter;
                        member.CreditToday += request.Amount;
                        member.CreditTotal += request.Amount;
                        
                        // 🔥 声音播放已移至会员申请时（BinggoLotteryService.HandleMessageAsync）
                        // 管理员处理时不播放声音，避免重复
                    }
                    else if (request.Action == CreditWithdrawAction.下分)
                    {
                        // 下分：检查余额并扣除
                        if (member.Balance < request.Amount)
                        {
                            // 余额不足
                            _logService.Warning("CreditWithdrawService", 
                                $"🔒 [{actionName}] {member.Nickname} - 余额不足: {member.Balance:F2} < {request.Amount:F2}");
                            
                            if (!isLoading && _socketClient != null)
                            {
                                try
                                {
                                    // 🔥 使用群昵称（DisplayName，系统昵称）
                                    string displayName = member.DisplayName?.UnEscape() ?? member.Nickname?.UnEscape() ?? "未知";
                                    string errorMsg = $"@{displayName} 存储不足!";
                                    _ = _socketClient.SendAsync<object>("SendMessage", member.GroupWxId, errorMsg);
                                }
                                catch (Exception ex)
                                {
                                    _logService.Warning("CreditWithdrawService", $"发送余额不足消息失败: {ex.Message}");
                                    // 继续执行，不影响主流程
                                }
                            }
                            return (false, "余额不足");
                        }

                        balanceAfter = balanceBefore - request.Amount;
                        member.Balance = balanceAfter;
                        member.WithdrawToday += request.Amount;
                        member.WithdrawTotal += request.Amount;
                        
                        // 🔥 声音播放已移至会员申请时（BinggoLotteryService.HandleMessageAsync）
                        // 管理员处理时不播放声音，避免重复
                    }
                    else
                    {
                        return (false, "未知操作类型");
                    }
                    
                    _logService.Info("CreditWithdrawService", 
                        $"🔒 [{actionName}] {member.Nickname} - 操作后余额: {balanceAfter:F2}, 变动: {(balanceAfter - balanceBefore):F2}");

                    // 2.2 更新申请状态（仅非加载模式）
                    if (!isLoading)
                    {
                        request.Status = CreditWithdrawStatus.已同意;
                        request.ProcessedBy = Services.Api.BoterApi.GetInstance().User;
                        request.ProcessedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    // 2.3 记录资金变动
                    balanceChange = new V2BalanceChange
                    {
                        GroupWxId = member.GroupWxId,
                        Wxid = member.Wxid,
                        Nickname = member.Nickname,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        ChangeAmount = request.Action == CreditWithdrawAction.上分 ? request.Amount : -request.Amount,
                        Reason = request.Action == CreditWithdrawAction.上分 ? ChangeReason.上分 : ChangeReason.下分,
                        IssueId = 0,
                        TimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Notes = isLoading ? "加载历史记录" : $"管理员同意{actionName}申请"
                    };

                    // 2.4 保存到数据库（统一事务）
                    _db.BeginTransaction();
                    try
                    {
                        _db.Update(member);
                        _db.Update(request);
                        
                        // 加载模式不重复插入资金变动记录
                        if (!isLoading)
                        {
                            _db.Insert(balanceChange);
                        }
                        
                        _db.Commit();
                        
                        _logService.Info("CreditWithdrawService", 
                            $"🔒 [{actionName}] {member.Nickname} - 数据已保存到数据库");
                    }
                    catch
                    {
                        _db.Rollback();
                        _logService.Error("CreditWithdrawService", 
                            $"🔒 [{actionName}] {member.Nickname} - 数据库事务回滚");
                        throw;
                    }
                }
                // 🔥 锁释放：上下分数据已同步写入

                // 🔥 6. 发送微信通知（仅非加载模式）
                if (!isLoading && _socketClient != null)
                {
                    // 🔥 使用群昵称（DisplayName，系统昵称）
                    string displayName = member.DisplayName?.UnEscape() ?? member.Nickname?.UnEscape() ?? "未知";
                    string notifyMessage = $"@{displayName}\r[{member.Id}]{actionName}{(int)request.Amount}完成|余:{(int)member.Balance}";
                    _ = _socketClient.SendAsync<object>("SendMessage", member.GroupWxId, notifyMessage);
                }

                // 🔥 7. 更新统计（仅非加载模式）
                if (!isLoading)
                {
                    _statisticsService.UpdateStatistics();
                }

                // 🔥 8. 日志记录
                _logService.Info("CreditWithdrawService",
                    $"{(isLoading ? "加载" : "处理")}{actionName}\n" +
                    $"会员：{member.Nickname}\n" +
                    $"金额：{request.Amount:F2}\n" +
                    $"变动前：{balanceBefore:F2}\n" +
                    $"变动后：{balanceAfter:F2}");

                return (true, null);
            }
            catch (Exception ex)
            {
                _logService.Error("CreditWithdrawService", "处理上下分失败", ex);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// 🔥 加载群的所有上下分记录并恢复统计
        /// 优化：只恢复"已同意"的记录，避免重复计算
        /// 🔥 用户要求：从内存表（BindingList）查询，而不是数据库
        /// </summary>
        public void LoadGroupCreditWithdraws(string groupWxid, Core.V2MemberBindingList membersBindingList)
        {
            try
            {
                _logService.Info("CreditWithdrawService", $"📊 开始加载群 {groupWxid} 的上下分数据...");

                // 🔥 1. 从 BindingList（内存表）查询已同意的上下分记录
                // 用户要求："订单只能从内存表中拿，改数据都改内存表，内存表修改即保存"
                if (_creditWithdrawsBindingList == null)
                {
                    _logService.Warning("CreditWithdrawService", "上下分 BindingList 未设置，无法加载数据");
                    return;
                }

                var creditWithdraws = _creditWithdrawsBindingList
                    .Where(cw => cw.GroupWxId == groupWxid && cw.Status == CreditWithdrawStatus.已同意)
                    .OrderBy(cw => cw.Timestamp)
                    .ToList();

                _logService.Info("CreditWithdrawService", $"📊 从内存表找到 {creditWithdraws.Count} 条已同意的上下分记录");

                if (creditWithdraws.Count == 0)
                {
                    return;
                }

                // 🔥 2. 今日日期
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // 🔥 3. 统计每个会员的上下分总额
                var memberStats = creditWithdraws
                    .GroupBy(cw => cw.Wxid)
                    .Select(g => new
                    {
                        Wxid = g.Key,
                        CreditTotal = g.Where(cw => cw.Action == CreditWithdrawAction.上分).Sum(cw => cw.Amount),
                        WithdrawTotal = g.Where(cw => cw.Action == CreditWithdrawAction.下分).Sum(cw => cw.Amount),
                        CreditToday = g.Where(cw => cw.Action == CreditWithdrawAction.上分 && cw.TimeString.StartsWith(today)).Sum(cw => cw.Amount),
                        WithdrawToday = g.Where(cw => cw.Action == CreditWithdrawAction.下分 && cw.TimeString.StartsWith(today)).Sum(cw => cw.Amount)
                    })
                    .ToList();

                // 🔥 4. 更新会员统计（通过 BindingList 更新，自动保存）
                int updatedCount = 0;
                foreach (var stat in memberStats)
                {
                    var member = membersBindingList.FirstOrDefault(m => m.Wxid == stat.Wxid);
                    if (member != null)
                    {
                        member.CreditTotal = stat.CreditTotal;
                        member.WithdrawTotal = stat.WithdrawTotal;
                        member.CreditToday = stat.CreditToday;
                        member.WithdrawToday = stat.WithdrawToday;
                        
                        // 🔥 BindingList 的 PropertyChanged 会自动保存到数据库，不需要手动 _db.Update
                        updatedCount++;
                    }
                }

                _logService.Info("CreditWithdrawService", 
                    $"✅ 上下分数据加载完成（从内存表）\n" +
                    $"处理记录：{creditWithdraws.Count} 条\n" +
                    $"更新会员：{updatedCount} 个");
            }
            catch (Exception ex)
            {
                _logService.Error("CreditWithdrawService", "加载上下分数据失败", ex);
            }
        }

        /// <summary>
        /// 🔥 忽略上下分申请（参考 F5BotV2 Line 1526-1542）
        /// </summary>
        public (bool success, string? errorMessage) IgnoreCreditWithdraw(V2CreditWithdraw request)
        {
            try
            {
                if (request.Status != CreditWithdrawStatus.等待处理)
                {
                    return (false, "该申请已处理");
                }

                // 更新申请状态为忽略
                request.Status = CreditWithdrawStatus.忽略;
                request.ProcessedBy = Services.Api.BoterApi.GetInstance().User;
                request.ProcessedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                request.Notes = "管理员忽略";

                // 保存到数据库
                _db.Update(request);

                // 日志记录
                _logService.Info("CreditWithdrawService",
                    $"忽略申请\n" +
                    $"会员：{request.Nickname}\n" +
                    $"金额：{request.Amount:F2}\n" +
                    $"处理人：{request.ProcessedBy}");

                return (true, null);
            }
            catch (Exception ex)
            {
                _logService.Error("CreditWithdrawService", "忽略申请失败", ex);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// 🔥 拒绝上下分申请
        /// </summary>
        public (bool success, string? errorMessage) RejectCreditWithdraw(V2CreditWithdraw request)
        {
            try
            {
                if (request.Status != CreditWithdrawStatus.等待处理)
                {
                    return (false, "该申请已处理");
                }

                string actionName = request.Action == CreditWithdrawAction.上分 ? "上分" : "下分";

                // 更新申请状态
                request.Status = CreditWithdrawStatus.已拒绝;
                request.ProcessedBy = Services.Api.BoterApi.GetInstance().User;
                request.ProcessedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                request.Notes = "管理员拒绝";

                // 保存到数据库
                _db.Update(request);

                // 发送微信通知
                if (_socketClient != null)
                {
                    string notifyMessage = $"@{request.Nickname} {actionName}申请已被管理员拒绝";
                    _ = _socketClient.SendAsync<object>("SendMessage", request.GroupWxId, notifyMessage);
                }

                // 日志记录
                _logService.Info("CreditWithdrawService",
                    $"拒绝{actionName}申请\n" +
                    $"会员：{request.Nickname}\n" +
                    $"金额：{request.Amount:F2}\n" +
                    $"处理人：{request.ProcessedBy}");

                return (true, null);
            }
            catch (Exception ex)
            {
                _logService.Error("CreditWithdrawService", "拒绝申请失败", ex);
                return (false, ex.Message);
            }
        }
    }
}

