using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Core;
using BaiShengVx3Plus.Services.Games.Binggo;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BaiShengVx3Plus.Services.GroupBinding
{
    /// <summary>
    /// 群组绑定服务实现
    /// 
    /// 🔥 现代化、精简、易维护的设计：
    /// 1. 单一职责：负责群组绑定和成员数据合并
    /// 2. 智能合并：对比数据库和服务器数据，自动处理新增/退群
    /// 3. 业务逻辑编排：完整的绑定流程（BindGroupCompleteAsync）
    /// 4. 无副作用：不直接操作 UI，只返回处理后的数据
    /// </summary>
    public class GroupBindingService : IGroupBindingService
    {
        private readonly ILogService _logService;
        private readonly IConfigurationService _configService;
        private SQLiteConnection? _db;
        
        public WxContact? CurrentBoundGroup { get; private set; }
        
        public GroupBindingService(
            ILogService logService,
            IConfigurationService configService)
        {
            _logService = logService;
            _configService = configService;
        }
        
        /// <summary>
        /// 设置数据库连接（由外部管理）
        /// </summary>
        public void SetDatabase(SQLiteConnection db)
        {
            _db = db;
        }
        
        /// <summary>
        /// 绑定群组
        /// </summary>
        public void BindGroup(WxContact group)
        {
            CurrentBoundGroup = group;
            _logService.Info("GroupBindingService", $"绑定群组: {group.Nickname} ({group.Wxid})");
        }
        
        /// <summary>
        /// 取消绑定
        /// </summary>
        public void UnbindGroup()
        {
            if (CurrentBoundGroup != null)
            {
                _logService.Info("GroupBindingService", $"取消绑定群组: {CurrentBoundGroup.Nickname}");
            }
            CurrentBoundGroup = null;
        }
        
        /// <summary>
        /// 🔥 刷新当前绑定群的成员数据（供外部调用）
        /// 
        /// 使用场景：
        /// 1. 点击"刷新会员"按钮
        /// 2. 管理命令"刷新"
        /// 
        /// 功能：
        /// - 从服务器重新获取群成员列表
        /// - 自动检测并更新昵称变化
        /// - 记录变化日志
        /// - 自动保存到数据库
        /// </summary>
        public async Task<(bool success, int memberCount)> RefreshCurrentGroupMembersAsync(
            IWeixinSocketClient socketClient,
            V2MemberBindingList membersBindingList)
        {
            try
            {
                if (CurrentBoundGroup == null)
                {
                    _logService.Warning("GroupBindingService", "当前未绑定群组，无法刷新");
                    return (false, 0);
                }
                
                _logService.Info("GroupBindingService", $"🔄 刷新群成员: {CurrentBoundGroup.Nickname}");
                
                // 🔥 调用内部刷新方法
                bool success = await RefreshGroupMembersInternalAsync(
                    CurrentBoundGroup.Wxid,
                    socketClient,
                    membersBindingList,
                    clearBeforeLoad: true);
                
                return (success, membersBindingList.Count);
            }
            catch (Exception ex)
            {
                _logService.Error("GroupBindingService", "刷新群成员失败", ex);
                return (false, 0);
            }
        }
        
        /// <summary>
        /// 🔥 智能加载和合并群成员数据
        /// 
        /// 核心逻辑：
        /// 1. 从数据库加载当前群的所有会员（GroupWxId == groupWxId）
        /// 2. 对比服务器返回的会员列表
        /// 3. 数据库中存在 → 保留（使用数据库数据，保留历史统计）
        /// 4. 数据库中不存在 → 新增（使用服务器数据）
        /// 5. 数据库有但服务器没返回 → 标记为"已退群"
        /// </summary>
        public List<V2Member> LoadAndMergeMembers(List<V2Member> serverMembers, string groupWxId)
        {
            if (_db == null)
            {
                _logService.Error("GroupBindingService", "数据库未初始化");
                return serverMembers;
            }
            
            try
            {
                _logService.Info("GroupBindingService", $"开始智能合并群成员数据: {groupWxId}");
                
                // 🔥 步骤1: 从数据库加载当前群的所有会员
                var dbMembers = _db.Table<V2Member>()
                    .Where(m => m.GroupWxId == groupWxId)
                    .ToList();
                
                _logService.Info("GroupBindingService", 
                    $"数据库中找到 {dbMembers.Count} 个会员，服务器返回 {serverMembers.Count} 个会员");
                
                // 🔥 步骤2: 创建服务器会员的 Wxid 集合（用于快速查找）
                var serverWxids = new HashSet<string>(
                    serverMembers.Where(m => !string.IsNullOrEmpty(m.Wxid))
                                 .Select(m => m.Wxid!)
                );
                
                // 🔥 步骤3: 合并结果列表
                var mergedMembers = new List<V2Member>();
                
                // 🔥 步骤4: 处理服务器返回的会员
                foreach (var serverMember in serverMembers)
                {
                    if (string.IsNullOrEmpty(serverMember.Wxid))
                        continue;
                    
                    // 在数据库中查找
                    var dbMember = dbMembers.FirstOrDefault(m => m.Wxid == serverMember.Wxid);
                    
                    if (dbMember != null)
                    {
                        // 情况1: 数据库中存在 → 使用数据库数据（保留历史统计）
                        // 🔥 检查并更新基本信息（昵称、群昵称可能变化）
                        
                        bool nicknameChanged = false;
                        bool displayNameChanged = false;
                        string oldNickname = dbMember.Nickname;
                        string oldDisplayName = dbMember.DisplayName;
                        
                        // 🔥 检查昵称是否变化
                        if (!string.IsNullOrEmpty(serverMember.Nickname) && 
                            serverMember.Nickname != dbMember.Nickname)
                        {
                            dbMember.Nickname = serverMember.Nickname;
                            nicknameChanged = true;
                        }
                        
                        // 🔥 检查DisplayName（群昵称/备注）是否变化
                        if (!string.IsNullOrEmpty(serverMember.DisplayName) && 
                            serverMember.DisplayName != dbMember.DisplayName)
                        {
                            dbMember.DisplayName = serverMember.DisplayName;
                            displayNameChanged = true;
                        }
                        
                        // 🔥 记录变化日志
                        if (nicknameChanged || displayNameChanged)
                        {
                            _logService.Warning("GroupBindingService", 
                                $"🔄 会员信息已更新 - ID={dbMember.Id}, 微信ID={dbMember.Wxid}");
                            
                            if (nicknameChanged)
                            {
                                _logService.Warning("GroupBindingService", 
                                    $"   ✏️ 昵称变更: [{oldNickname}] → [{dbMember.Nickname}]");
                            }
                            
                            if (displayNameChanged)
                            {
                                _logService.Warning("GroupBindingService", 
                                    $"   ✏️ 群昵称变更: [{oldDisplayName}] → [{dbMember.DisplayName}]" +
                                    $" （留分名单将使用新名称）");
                            }
                        }
                        
                        // 🔥 如果之前是"已退群"，重新加入时全部复位
                        if (dbMember.State == MemberState.已退群)
                        {
                            string oldState = dbMember.State.ToString();
                            
                            // 🔥 关键修复：重新加入时，所有数据复位（包括状态）
                            // 记录之前的完整数据（用于审计）
                            _logService.Warning("GroupBindingService", 
                                $"📋 会员重新加入（数据复位）: " +
                                $"Wxid={dbMember.Wxid}, " +
                                $"昵称={dbMember.Nickname}, " +
                                $"原状态={oldState}, " +
                                $"原余额={dbMember.Balance:F2}, " +
                                $"原待结算={dbMember.BetWait:F2}, " +
                                $"原总下注={dbMember.BetTotal:F2}, " +
                                $"原总盈利={dbMember.IncomeTotal:F2}, " +
                                $"原总上分={dbMember.CreditTotal:F2}, " +
                                $"原总下分={dbMember.WithdrawTotal:F2}");
                            
                            // 🔥 复位所有数据（财务 + 状态）
                            dbMember.State = MemberState.会员;  // ← 强制复位为"会员"
                            dbMember.Balance = 0;
                            dbMember.BetWait = 0;
                            dbMember.BetToday = 0;
                            dbMember.BetTotal = 0;
                            dbMember.BetCur = 0;
                            dbMember.IncomeToday = 0;
                            dbMember.IncomeTotal = 0;
                            dbMember.CreditToday = 0;
                            dbMember.CreditTotal = 0;
                            dbMember.WithdrawToday = 0;
                            dbMember.WithdrawTotal = 0;
                            
                            _logService.Info("GroupBindingService", 
                                $"✅ 会员 {dbMember.Nickname} 重新加入群组，所有数据已复位（状态=会员，余额=0）");
                        }
                        
                        mergedMembers.Add(dbMember);
                    }
                    else
                    {
                        // 情况2: 数据库中不存在 → 新增会员
                        // 🔥 关键：新会员的财务数据全部清0（初始化状态）
                serverMember.GroupWxId = groupWxId;
                serverMember.State = MemberState.会员;  // 默认状态
                serverMember.Balance = 0;  // 余额清0
                serverMember.BetWait = 0;  // 待结算清0
                serverMember.BetToday = 0;
                serverMember.BetTotal = 0;
                serverMember.BetCur = 0;
                serverMember.IncomeToday = 0;
                serverMember.IncomeTotal = 0;
                serverMember.CreditToday = 0;
                serverMember.CreditTotal = 0;
                serverMember.WithdrawToday = 0;
                serverMember.WithdrawTotal = 0;
                        
                        mergedMembers.Add(serverMember);
                        
                        _logService.Info("GroupBindingService", 
                            $"新增会员（初始化状态）: {serverMember.Nickname} ({serverMember.Wxid})");
                    }
                }
                
                // 🔥 步骤5: 处理已退群的会员（数据库有但服务器没返回）
                foreach (var dbMember in dbMembers)
                {
                    if (string.IsNullOrEmpty(dbMember.Wxid))
                        continue;
                    
                    if (!serverWxids.Contains(dbMember.Wxid))
                    {
                        // 情况3: 数据库有但服务器没返回 → 标记为"已退群"
                        if (dbMember.State != MemberState.已退群 && dbMember.State != MemberState.已删除)
                        {
                            // 🔥 记录会员退群前的完整数据（用于审计）
                            _logService.Warning("GroupBindingService", 
                                $"📋 会员退群（完整数据记录）: " +
                                $"Wxid={dbMember.Wxid}, " +
                                $"昵称={dbMember.Nickname}, " +
                                $"备注={dbMember.Nickname ?? "无"}, " +
                                $"原状态={dbMember.State}, " +
                                $"余额={dbMember.Balance:F2}, " +
                                $"待结算={dbMember.BetWait:F2}, " +
                                $"今日下注={dbMember.BetToday:F2}, " +
                                $"总下注={dbMember.BetTotal:F2}, " +
                                $"今日盈利={dbMember.IncomeToday:F2}, " +
                                $"总盈利={dbMember.IncomeTotal:F2}, " +
                                $"今日上分={dbMember.CreditToday:F2}, " +
                                $"总上分={dbMember.CreditTotal:F2}, " +
                                $"今日下分={dbMember.WithdrawToday:F2}, " +
                                $"总下分={dbMember.WithdrawTotal:F2}, " +
                                $"群ID={dbMember.GroupWxId}");
                            
                            dbMember.State = MemberState.已退群;
                            mergedMembers.Add(dbMember);
                        }
                        else if (dbMember.State == MemberState.已退群)
                        {
                            // 仍然是已退群状态，保留
                            mergedMembers.Add(dbMember);
                        }
                    }
                }
                
                _logService.Info("GroupBindingService", 
                    $"✅ 合并完成: 共 {mergedMembers.Count} 个会员");
                
                return mergedMembers;
            }
            catch (Exception ex)
            {
                _logService.Error("GroupBindingService", "合并群成员数据失败", ex);
                return serverMembers;
            }
        }
        
        /// <summary>
        /// 🔥 刷新群成员数据（公共方法）
        /// 
        /// 职责：
        /// 1. 从服务器获取群成员列表
        /// 2. 智能合并数据库和服务器数据
        /// 3. 更新 BindingList
        /// 4. 记录昵称变化日志
        /// 
        /// 用途：
        /// - 绑定群时调用（BindGroupCompleteAsync）
        /// - 刷新会员时调用（RefreshCurrentGroupMembersAsync）
        /// </summary>
        private async Task<bool> RefreshGroupMembersInternalAsync(
            string groupWxid,
            IWeixinSocketClient socketClient,
            V2MemberBindingList membersBindingList,
            bool clearBeforeLoad)
        {
            try
            {
                _logService.Info("GroupBindingService", $"🔄 开始刷新群成员: {groupWxid}");
                
                // 🔥 1. 获取服务器数据
                var serverResult = await socketClient.SendAsync<JsonDocument>("GetGroupContacts", groupWxid);
                
                if (serverResult == null)
                {
                    _logService.Warning("GroupBindingService", "获取群成员失败: 返回 null");
                    
                    // 服务器获取失败，只加载数据库数据
                    await Task.Run(() =>
                    {
                        if (clearBeforeLoad)
                        {
                            membersBindingList.Clear();
                        }
                        membersBindingList.LoadFromDatabase();
                    });
                    _logService.Info("GroupBindingService", $"从数据库加载: {membersBindingList.Count} 个会员");
                    return false;
                }
                
                _logService.Info("GroupBindingService", $"获取成功，类型: {serverResult.RootElement.ValueKind}");
                
                // 🔥 2. 开发模式：使用模拟数据
                if (_configService.GetIsRunModeDev())
                {
                    _logService.Info("GroupBindingService", "🔧 开发模式：使用模拟群成员数据");
                    
                    var mockMembers = new[]
                    {
                        new { username = "M100", Balance = 100f, wxid = "wxid_m100", nick_name = "nick100" },
                        new { username = "M200", Balance = 200f, wxid = "wxid_m200", nick_name = "nick200" },
                        new { username = "M300", Balance = 300f, wxid = "wxid_m300", nick_name = "nick300"},
                        new { username = "M400", Balance = 400f, wxid = "wxid_m400", nick_name = "nick400" },
                        new { username = "M500", Balance = 500f, wxid = "wxid_m500", nick_name = "nick500" }
                    };
                    
                    serverResult = JsonDocument.Parse(JsonConvert.SerializeObject(mockMembers));
                    _logService.Info("GroupBindingService", $"✅ 模拟数据: {mockMembers.Length} 个会员");
                }
                
                if (serverResult.RootElement.ValueKind != JsonValueKind.Array)
                {
                    _logService.Warning("GroupBindingService", $"格式错误，只加载数据库数据，ValueKind={serverResult.RootElement.ValueKind}");
                    
                    await Task.Run(() =>
                    {
                        if (clearBeforeLoad)
                        {
                            membersBindingList.Clear();
                        }
                        membersBindingList.LoadFromDatabase();
                    });
                    _logService.Info("GroupBindingService", $"从数据库加载: {membersBindingList.Count} 个会员");
                    return false;
                }
                
                // 🔥 3. 解析服务器返回的会员数据
                int arrayLength = serverResult.RootElement.GetArrayLength();
                _logService.Info("GroupBindingService", $"开始解析 {arrayLength} 个群成员");
                
                var serverMembers = ParseServerMembers(serverResult.RootElement, groupWxid);
                _logService.Info("GroupBindingService", $"解析完成: {serverMembers.Count} 个");
                
                // 🔥 4. 智能合并数据（数据库 + 服务器，会记录昵称变化日志）
                var mergedMembers = LoadAndMergeMembers(serverMembers, groupWxid);
                _logService.Info("GroupBindingService", $"合并完成: {mergedMembers.Count} 个会员");
                
                // 🔥 5. 更新 BindingList
                // 🔥 关键修复：使用锁保护 Clear/Add 操作，防止并发问题
                lock (Core.ResourceLocks.BindingListUpdateLock)
                {
                    if (clearBeforeLoad)
                    {
                        // 切换到不同的群：清空旧数据，添加新数据
                        membersBindingList.Clear();
                        _logService.Info("GroupBindingService", "切换群：已清空会员列表，准备重新加载");
                        
                        foreach (var member in mergedMembers)
                        {
                            membersBindingList.Add(member);
                        }
                    }
                    else
                    {
                        // 刷新同一个群：采用更新模式（不清空，避免引用失效）
                        _logService.Info("GroupBindingService", "刷新同一个群：采用更新模式（逐个更新，避免引用失效）");
                        
                        // 🔥 更新模式：更新现有会员，添加新会员，移除已退群的
                        foreach (var newMember in mergedMembers)
                        {
                            var existingMember = membersBindingList.FirstOrDefault(m => m.Wxid == newMember.Wxid);
                            if (existingMember != null)
                            {
                                // 更新现有会员的数据（保持引用不变）
                                existingMember.Nickname = newMember.Nickname;
                                existingMember.State = newMember.State;
                                existingMember.DisplayName = newMember.DisplayName;
                                // 不更新余额等财务数据（从数据库加载的是最新的）
                                existingMember.Balance = newMember.Balance;
                                existingMember.BetWait = newMember.BetWait;
                                existingMember.BetToday = newMember.BetToday;
                                existingMember.BetTotal = newMember.BetTotal;
                                existingMember.IncomeToday = newMember.IncomeToday;
                                existingMember.IncomeTotal = newMember.IncomeTotal;
                                existingMember.CreditToday = newMember.CreditToday;
                                existingMember.CreditTotal = newMember.CreditTotal;
                                existingMember.WithdrawToday = newMember.WithdrawToday;
                                existingMember.WithdrawTotal = newMember.WithdrawTotal;
                            }
                            else
                            {
                                // 新成员：添加到列表
                                membersBindingList.Add(newMember);
                                _logService.Info("GroupBindingService", $"新成员: {newMember.Nickname}");
                            }
                        }
                        
                        // 移除已退群的会员
                        var mergedWxids = mergedMembers.Select(m => m.Wxid).ToHashSet();
                        var toRemove = membersBindingList.Where(m => !mergedWxids.Contains(m.Wxid)).ToList();
                        foreach (var member in toRemove)
                        {
                            // 🔥 关键修复：移除前记录完整的会员数据（用于审计和恢复）
                            _logService.Warning("GroupBindingService", 
                                $"📋 移除已退群会员（完整数据记录）: " +
                                $"Wxid={member.Wxid}, " +
                                $"昵称={member.Nickname}, " +
                                $"备注={member.Nickname ?? "无"}, " +
                                $"状态={member.State}, " +
                                $"余额={member.Balance:F2}, " +
                                $"待结算={member.BetWait:F2}, " +
                                $"今日下注={member.BetToday:F2}, " +
                                $"总下注={member.BetTotal:F2}, " +
                                $"今日盈利={member.IncomeToday:F2}, " +
                                $"总盈利={member.IncomeTotal:F2}, " +
                                $"今日上分={member.CreditToday:F2}, " +
                                $"总上分={member.CreditTotal:F2}, " +
                                $"今日下分={member.WithdrawToday:F2}, " +
                                $"总下分={member.WithdrawTotal:F2}, " +
                                $"群ID={member.GroupWxId}, " +
                                $"创建时间={DateTime.Now:yyyy-MM-dd HH:mm:ss}, " +
                                $"更新时间={DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                            
                            membersBindingList.Remove(member);
                        }
                    }
                    
                    _logService.Info("GroupBindingService", $"✅ 会员列表已更新: {membersBindingList.Count} 个会员");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logService.Error("GroupBindingService", $"刷新群成员失败: {ex.Message}", ex);
                return false;
            }
        }
        
        /// <summary>
        /// 🔥 完整的群组绑定流程（核心业务逻辑）
        /// 
        /// 职责：编排所有业务逻辑，返回结果 DTO，View 层只负责 UI 更新
        /// 
        /// 🔥 关键修复 2025-11-18：支持传入已有 BindingList（避免引用断裂）
        /// - 如果传入已有实例 → 使用 Clear() + Add() 更新数据
        /// - 如果传入 null → 创建新实例（首次绑定）
        /// </summary>
        public async Task<GroupBindingResult> BindGroupCompleteAsync(
            WxContact contact,
            SQLiteConnection db,
            IWeixinSocketClient socketClient,
            IBinggoOrderService orderService,
            BinggoStatisticsService statisticsService,
            IMemberDataService memberDataService,
            IBinggoLotteryService lotteryService,
            V2MemberBindingList? existingMembersBindingList = null,
            V2OrderBindingList? existingOrdersBindingList = null,
            V2CreditWithdrawBindingList? existingCreditWithdrawsBindingList = null,
            bool isSameGroup = false)
        {
            var result = new GroupBindingResult { Group = contact };
            
            try
            {
                _logService.Info("GroupBindingService", $"📍 开始完整绑定群: {contact.Nickname} ({contact.Wxid})");
                
                // 🔥 1. 绑定群组
                BindGroup(contact);
                SetDatabase(db);
                
                // 🔥 2. 复用已有 BindingList 或创建新实例
                bool isFirstTimeBinding = existingMembersBindingList == null;
                
                var membersBindingList = existingMembersBindingList ?? new V2MemberBindingList(db, contact.Wxid);
                var ordersBindingList = existingOrdersBindingList ?? new V2OrderBindingList(db, contact.Wxid);
                var creditWithdrawsBindingList = existingCreditWithdrawsBindingList ?? new V2CreditWithdrawBindingList(db);
                
                if (isFirstTimeBinding)
                {
                    _logService.Info("GroupBindingService", "✅ BindingList 首次创建");
                }
                else
                {
                    _logService.Info("GroupBindingService", "✅ 复用已有 BindingList（避免引用断裂）");
                }
                
                // 🔥 3. 设置各种服务依赖
                orderService.SetMembersBindingList(membersBindingList);
                orderService.SetOrdersBindingList(ordersBindingList);
                orderService.SetStatisticsService(statisticsService);
                statisticsService.SetBindingLists(membersBindingList, ordersBindingList);
                
                if (memberDataService is MemberDataService mds)
                {
                    mds.SetMembersBindingList(membersBindingList);
                }
                
                // 🔥 3.5. 更新开奖服务的 BindingList 引用
                if (lotteryService is BinggoLotteryService lotteryServiceImpl)
                {
                    lotteryServiceImpl.SetBusinessDependencies(
                        orderService,
                        this,
                        socketClient,
                        ordersBindingList,
                        membersBindingList,
                        creditWithdrawsBindingList,
                        statisticsService  // 🔥 传入统计服务！
                    );
                }
                
                _logService.Info("GroupBindingService", "✅ 服务依赖已设置");
                
                // 🔥 4. 从数据库加载订单数据（订单不需要与服务器同步）
                await Task.Run(() =>
                {
                    // 🔥 关键修复：如果是复用已有实例，先 Clear() 再加载
                    if (!isFirstTimeBinding)
                    {
                        ordersBindingList.Clear();
                    }
                    ordersBindingList.LoadFromDatabase();
                });
                
                _logService.Info("GroupBindingService", $"✅ 从数据库加载: {ordersBindingList.Count} 个订单");
                
                // 🔥 4.5. 从数据库加载上下分数据
                await Task.Run(() =>
                {
                    // 🔥 关键修复：如果是复用已有实例，先 Clear() 再加载
                    if (!isFirstTimeBinding)
                    {
                        creditWithdrawsBindingList.Clear();
                    }
                    creditWithdrawsBindingList.LoadFromDatabase(contact.Wxid);
                });
                
                _logService.Info("GroupBindingService", $"✅ 从数据库加载: {creditWithdrawsBindingList.Count} 条上下分记录");
                
                // 🔥 6. 刷新群成员数据（调用提取的公共方法）
                // 🔥 关键优化：只有切换到不同的群时才清空列表
                // 如果是同一个群（刷新），采用更新模式，避免 member 引用失效
                bool clearBeforeLoad = !isSameGroup;
                
                _logService.Info("GroupBindingService", 
                    $"刷新模式: 同一个群={isSameGroup}, 清空列表={clearBeforeLoad}");
                
                await RefreshGroupMembersInternalAsync(
                    contact.Wxid,
                    socketClient,
                    membersBindingList,
                    clearBeforeLoad: clearBeforeLoad);
                
                // 🔥 9. 更新会员的上下分统计
                creditWithdrawsBindingList.UpdateMemberStatistics(membersBindingList);
                _logService.Info("GroupBindingService", "✅ 会员上下分统计已更新");
                
                // 🔥 10. 更新统计
                statisticsService.UpdateStatistics();
                _logService.Info("GroupBindingService", "✅ 统计数据已更新");
                
                // 🔥 11. 返回结果 DTO
                result.MembersBindingList = membersBindingList;
                result.OrdersBindingList = ordersBindingList;
                result.CreditWithdrawsBindingList = creditWithdrawsBindingList;
                result.MemberCount = membersBindingList.Count;
                result.OrderCount = ordersBindingList.Count;
                result.CreditWithdrawCount = creditWithdrawsBindingList.Count;
                result.Success = true;
                
                _logService.Info("GroupBindingService", 
                    $"✅ 绑定群完成: {result.MemberCount} 个会员, {result.OrderCount} 个订单, {result.CreditWithdrawCount} 条上下分记录");
                
                return result;
            }
            catch (Exception ex)
            {
                _logService.Error("GroupBindingService", $"绑定群失败: {ex.Message}", ex);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
        
        /// <summary>
        /// 解析服务器返回的会员数据（从 VxMain 移过来）
        /// </summary>
        private List<V2Member> ParseServerMembers(JsonElement data, string groupWxId)
        {
            var members = new List<V2Member>();
            
            try
            {
                if (data.ValueKind != JsonValueKind.Array)
                {
                    _logService.Warning("GroupBindingService", "服务器返回的数据不是数组");
                    return members;
                }
                
                foreach (var item in data.EnumerateArray())
                {
                    try
                    {
                        var member = new V2Member
                        {
                            GroupWxId = groupWxId,
                            State = MemberState.会员
                        };
                        
                        // 🔥 解析 wxid（支持多种字段名）
                        if (item.TryGetProperty("member_wxid", out var memberWxid))
                        {
                            member.Wxid = memberWxid.GetString() ?? string.Empty;
                        }
                        else if (item.TryGetProperty("username", out var username))
                        {
                            member.Wxid = username.GetString() ?? string.Empty;
                        }
                        else if (item.TryGetProperty("wxid", out var wxid))
                        {
                            member.Wxid = wxid.GetString() ?? string.Empty;
                        }
                        
                        if (string.IsNullOrEmpty(member.Wxid))
                        {
                            _logService.Warning("GroupBindingService", "跳过无效会员：wxid 为空");
                            continue;
                        }
                        
                        // 🔥 解析昵称（支持多种字段名）
                        if (item.TryGetProperty("member_nickname", out var memberNickname))
                        {
                            member.Nickname = memberNickname.GetString() ?? string.Empty;
                        }
                        else if (item.TryGetProperty("nick_name", out var nickName))
                        {
                            member.Nickname = nickName.GetString() ?? string.Empty;
                        }
                        else if (item.TryGetProperty("nickname", out var nickname))
                        {
                            member.Nickname = nickname.GetString() ?? string.Empty;
                        }
                        
                        // 🔥 解析备注名（作为群昵称）
                        if (item.TryGetProperty("member_remark", out var memberRemark))
                        {
                            string remark = memberRemark.GetString() ?? string.Empty;
                            if (!string.IsNullOrEmpty(remark))
                            {
                                member.DisplayName = remark;
                            }
                            else
                            {
                                member.DisplayName = member.Nickname; // 备注为空时使用昵称
                            }
                        }
                        else if (item.TryGetProperty("display_name", out var displayName))
                        {
                            member.DisplayName = displayName.GetString() ?? string.Empty;
                        }
                        else
                        {
                            member.DisplayName = member.Nickname; // 默认使用昵称
                        }
                        
                        // 🔥 解析微信号（支持多种字段名）
                        if (item.TryGetProperty("member_alias", out var memberAlias))
                        {
                            member.Account = memberAlias.GetString() ?? string.Empty;
                        }
                        else if (item.TryGetProperty("alias", out var alias))
                        {
                            member.Account = alias.GetString() ?? string.Empty;
                        }
                        
                        members.Add(member);
                        _logService.Debug("GroupBindingService", $"解析会员: {member.Nickname} ({member.Wxid})");
                    }
                    catch (Exception ex)
                    {
                        _logService.Error("GroupBindingService", $"解析单个会员数据失败: {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error("GroupBindingService", $"解析群成员数据失败: {ex.Message}", ex);
            }
            
            _logService.Info("GroupBindingService", $"✅ 解析完成: 共 {members.Count} 个会员");
            return members;
        }
    }
}

