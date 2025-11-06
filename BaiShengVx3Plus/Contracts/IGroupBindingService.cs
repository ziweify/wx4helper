using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 群组绑定服务接口
    /// 
    /// 🔥 职责：
    /// 1. 管理当前绑定的群组
    /// 2. 智能加载和合并群成员数据
    /// 3. 检测退群成员并更新状态
    /// </summary>
    public interface IGroupBindingService
    {
        /// <summary>
        /// 当前绑定的群组
        /// </summary>
        WxContact? CurrentBoundGroup { get; }
        
        /// <summary>
        /// 绑定群组
        /// </summary>
        /// <param name="group">要绑定的群组</param>
        void BindGroup(WxContact group);
        
        /// <summary>
        /// 取消绑定
        /// </summary>
        void UnbindGroup();
        
        /// <summary>
        /// 智能加载群成员
        /// 
        /// 逻辑：
        /// 1. 对比服务器返回的数据和数据库中的数据
        /// 2. 数据库中存在 → 加载（保留历史数据）
        /// 3. 数据库中不存在 → 新增
        /// 4. 数据库有但服务器没返回 → 标记为"已退群"
        /// </summary>
        /// <param name="serverMembers">服务器返回的群成员列表</param>
        /// <param name="groupWxId">群微信ID</param>
        /// <returns>合并后的会员列表</returns>
        List<V2Member> LoadAndMergeMembers(List<V2Member> serverMembers, string groupWxId);
    }
}

