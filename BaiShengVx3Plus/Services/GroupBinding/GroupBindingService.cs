using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models;
using SQLite;

namespace BaiShengVx3Plus.Services.GroupBinding
{
    /// <summary>
    /// 群组绑定服务实现
    /// 
    /// 🔥 现代化、精简、易维护的设计：
    /// 1. 单一职责：只负责群组绑定和成员数据合并
    /// 2. 智能合并：对比数据库和服务器数据，自动处理新增/退群
    /// 3. 无副作用：不直接操作 UI，只返回处理后的数据
    /// </summary>
    public class GroupBindingService : IGroupBindingService
    {
        private readonly ILogService _logService;
        private SQLiteConnection? _db;
        
        public WxContact? CurrentBoundGroup { get; private set; }
        
        public GroupBindingService(ILogService logService)
        {
            _logService = logService;
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
    }
}

