using System;
using System.Linq;
using zhaocaimao.Contracts;
using zhaocaimao.Models;
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
                float balanceBefore = member.Balance;
                float balanceAfter;

                // 🔥 2. 根据动作类型处理
                if (request.Action == CreditWithdrawAction.上分)
                {
                    // 上分：增加余额
                    balanceAfter = balanceBefore + request.Amount;
                    member.Balance = balanceAfter;
                    member.CreditToday += request.Amount;
                    member.CreditTotal += request.Amount;
                    
                    // 🔥 播放上分声音（参考 F5BotV2 第2597行：PlayMp3("mp3_shang.mp3")）
                    if (!isLoading)
                    {
                        _soundService?.PlayCreditUpSound();
                    }
                }
                else if (request.Action == CreditWithdrawAction.下分)
                {
                    // 下分：检查余额并扣除
                    if (member.Balance < request.Amount)
                    {
                        // 余额不足
                        if (!isLoading && _socketClient != null)
                        {
                            string errorMsg = $"@{member.Nickname} 存储不足!";
                            _ = _socketClient.SendAsync<object>("SendMessage", member.GroupWxId, errorMsg);
                        }
                        return (false, "余额不足");
                    }

                    balanceAfter = balanceBefore - request.Amount;
                    member.Balance = balanceAfter;
                    member.WithdrawToday += request.Amount;
                    member.WithdrawTotal += request.Amount;
                    
                    // 🔥 播放下分声音（参考 F5BotV2 第2599行：PlayMp3("mp3_xia.mp3")）
                    if (!isLoading)
                    {
                        _soundService?.PlayCreditDownSound();
                    }
                }
                else
                {
                    return (false, "未知操作类型");
                }

                // 🔥 3. 更新申请状态（仅非加载模式）
                if (!isLoading)
                {
                    request.Status = CreditWithdrawStatus.已同意;
                    request.ProcessedBy = Services.Api.BoterApi.GetInstance().User;
                    request.ProcessedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                // 🔥 4. 记录资金变动
                var balanceChange = new V2BalanceChange
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

                // 🔥 5. 保存到数据库（统一事务 + 锁保护）
                Services.Database.DatabaseLockService.Instance.ExecuteWrite(() =>
                {
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
                    }
                    catch
                    {
                        _db.Rollback();
                        throw;
                    }
                });

                // 🔥 6. 发送微信通知（仅非加载模式）
                if (!isLoading && _socketClient != null)
                {
                    string notifyMessage = $"@{member.Nickname}\r[{member.Id}]{actionName}{(int)request.Amount}完成|余:{(int)member.Balance}";
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
        /// </summary>
        public void LoadGroupCreditWithdraws(string groupWxid, Core.V2MemberBindingList membersBindingList)
        {
            try
            {
                _logService.Info("CreditWithdrawService", $"📊 开始加载群 {groupWxid} 的上下分数据...");

                // 🔥 1. 加载已同意的上下分记录
                var creditWithdraws = _db.Table<V2CreditWithdraw>()
                    .Where(cw => cw.GroupWxId == groupWxid && cw.Status == CreditWithdrawStatus.已同意)
                    .OrderBy(cw => cw.Timestamp)
                    .ToList();

                _logService.Info("CreditWithdrawService", $"📊 找到 {creditWithdraws.Count} 条已同意的上下分记录");

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

                // 🔥 4. 更新会员统计（批量更新，高效）
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
                        
                        // 保存到数据库
                        _db.Update(member);
                        updatedCount++;
                    }
                }

                _logService.Info("CreditWithdrawService", 
                    $"✅ 上下分数据加载完成\n" +
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

