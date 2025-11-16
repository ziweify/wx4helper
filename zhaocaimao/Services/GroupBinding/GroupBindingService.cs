using zhaocaimao.Contracts;
using zhaocaimao.Contracts.Games;
using zhaocaimao.Models;
using zhaocaimao.Core;
using zhaocaimao.Services.Games.Binggo;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace zhaocaimao.Services.GroupBinding
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
                        // 但更新基本信息（昵称、群昵称可能变化）
                        dbMember.Nickname = serverMember.Nickname;
                        dbMember.DisplayName = serverMember.DisplayName;
                        
                        // 如果之前是"已退群"，现在恢复为原状态或"会员"
                        if (dbMember.State == MemberState.已退群)
                        {
                            dbMember.State = MemberState.会员;
                            _logService.Info("GroupBindingService", 
                                $"会员 {dbMember.Nickname} 重新加入群组");
                        }
                        
                        mergedMembers.Add(dbMember);
                    }
                    else
                    {
                        // 情况2: 数据库中不存在 → 新增会员
                        serverMember.GroupWxId = groupWxId;
                        serverMember.State = MemberState.会员;  // 默认状态
                        mergedMembers.Add(serverMember);
                        
                        _logService.Info("GroupBindingService", 
                            $"新增会员: {serverMember.Nickname} ({serverMember.Wxid})");
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
                            dbMember.State = MemberState.已退群;
                            mergedMembers.Add(dbMember);
                            
                            _logService.Warning("GroupBindingService", 
                                $"会员 {dbMember.Nickname} 已退群");
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
        /// 🔥 完整的群组绑定流程（核心业务逻辑）
        /// 
        /// 职责：编排所有业务逻辑，返回结果 DTO，View 层只负责 UI 更新
        /// </summary>
        public async Task<GroupBindingResult> BindGroupCompleteAsync(
            WxContact contact,
            SQLiteConnection db,
            IWeixinSocketClient socketClient,
            IBinggoOrderService orderService,
            BinggoStatisticsService statisticsService,
            IMemberDataService memberDataService,
            IBinggoLotteryService lotteryService)
        {
            var result = new GroupBindingResult { Group = contact };
            
            try
            {
                _logService.Info("GroupBindingService", $"📍 开始完整绑定群: {contact.Nickname} ({contact.Wxid})");
                
                // 🔥 1. 绑定群组
                BindGroup(contact);
                SetDatabase(db);
                
                // 🔥 2. 创建 BindingList（绑定到数据库）
                var membersBindingList = new V2MemberBindingList(db, contact.Wxid);
                var ordersBindingList = new V2OrderBindingList(db);
                var creditWithdrawsBindingList = new V2CreditWithdrawBindingList(db);
                
                _logService.Info("GroupBindingService", "✅ BindingList 已创建");
                
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
                    ordersBindingList.LoadFromDatabase();
                });
                
                _logService.Info("GroupBindingService", $"✅ 从数据库加载: {ordersBindingList.Count} 个订单");
                
                // 🔥 4.5. 从数据库加载上下分数据
                await Task.Run(() =>
                {
                    creditWithdrawsBindingList.LoadFromDatabase(contact.Wxid);
                });
                
                _logService.Info("GroupBindingService", $"✅ 从数据库加载: {creditWithdrawsBindingList.Count} 条上下分记录");
                
                // 🔥 4.5. 获取全局配置（从 IConfigurationService）
                bool isRunModeDev = _configService.GetIsRunModeDev();
                _logService.Info("GroupBindingService", $"✅ 全局配置: IsRunModeDev = {isRunModeDev}");
                
                // 🔥 5. 获取服务器数据并智能合并会员
                _logService.Info("GroupBindingService", $"开始获取群成员: {contact.Nickname} ({contact.Wxid})");
                
                var serverResult = await socketClient.SendAsync<JsonDocument>("GetGroupContacts", contact.Wxid);
                
                // 🔥 检查返回结果
                if (serverResult == null)
                {
                    _logService.Warning("GroupBindingService", "获取群成员失败: 返回 null");
                }
                else
                {
                    _logService.Info("GroupBindingService", $"获取成功，类型: {serverResult.RootElement.ValueKind}");
                    if (serverResult.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        _logService.Info("GroupBindingService", $"数组长度: {serverResult.RootElement.GetArrayLength()}");
                    }
                }
                
                // 🔥 5.1. 开发模式：使用模拟数据
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

                if (serverResult == null || serverResult.RootElement.ValueKind != JsonValueKind.Array)
                {
                    // 服务器获取失败，只加载数据库数据
                    _logService.Warning("GroupBindingService", $"获取失败或格式错误，只加载数据库数据");
                    if (serverResult != null)
                    {
                        _logService.Warning("GroupBindingService", $"ValueKind={serverResult.RootElement.ValueKind}");
                    }
                    
                    await Task.Run(() =>
                    {
                        membersBindingList.LoadFromDatabase();
                    });
                    _logService.Info("GroupBindingService", $"从数据库加载: {membersBindingList.Count} 个会员");
                }
                else
                {
                    // 🔥 6. 解析服务器返回的会员数据
                    int arrayLength = serverResult.RootElement.GetArrayLength();
                    _logService.Info("GroupBindingService", $"开始解析 {arrayLength} 个群成员");
                    
                    var serverMembers = ParseServerMembers(serverResult.RootElement, contact.Wxid);
                    _logService.Info("GroupBindingService", $"解析完成: {serverMembers.Count} 个");
                    
                    // 🔥 7. 智能合并数据（数据库 + 服务器）
                    var mergedMembers = LoadAndMergeMembers(serverMembers, contact.Wxid);
                    _logService.Info("GroupBindingService", $"合并完成: {mergedMembers.Count} 个会员");
                    
                    // 🔥 8. 加载合并后的完整列表
                    foreach (var member in mergedMembers)
                    {
                        membersBindingList.Add(member);
                    }
                    
                    _logService.Info("GroupBindingService", $"✅ 会员列表已更新: {membersBindingList.Count} 个会员");
                }
                
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

