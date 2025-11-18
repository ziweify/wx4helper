using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Services.Games.Binggo;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 群组绑定服务接口
    /// 
    /// 🔥 职责：
    /// 1. 管理当前绑定的群组
    /// 2. 智能加载和合并群成员数据
    /// 3. 检测退群成员并更新状态
    /// 4. 编排完整的群组绑定流程（业务逻辑层）
    /// </summary>
    public interface IGroupBindingService
    {
        /// <summary>
        /// 当前绑定的群组
        /// </summary>
        WxContact? CurrentBoundGroup { get; }
        
        /// <summary>
        /// 设置数据库连接
        /// </summary>
        void SetDatabase(SQLiteConnection db);
        
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
        
        /// <summary>
        /// 🔥 完整的群组绑定流程（核心业务逻辑）
        /// 
        /// 职责：
        /// 1. 绑定群组
        /// 2. 创建 BindingList
        /// 3. 设置各种服务依赖
        /// 4. 加载数据库数据（订单、上下分）
        /// 5. 获取服务器数据并智能合并会员
        /// 6. 更新统计
        /// 7. 返回结果 DTO
        /// 
        /// 🔥 关键修复 2025-11-18：支持传入已有 BindingList（避免引用断裂）
        /// </summary>
        /// <param name="contact">要绑定的群组</param>
        /// <param name="db">数据库连接</param>
        /// <param name="socketClient">Socket 客户端</param>
        /// <param name="orderService">订单服务</param>
        /// <param name="statisticsService">统计服务</param>
        /// <param name="memberDataService">会员数据服务</param>
        /// <param name="lotteryService">开奖服务</param>
        /// <param name="existingMembersBindingList">已有的会员 BindingList（可选）</param>
        /// <param name="existingOrdersBindingList">已有的订单 BindingList（可选）</param>
        /// <param name="existingCreditWithdrawsBindingList">已有的上下分 BindingList（可选）</param>
        /// <returns>绑定结果</returns>
        Task<GroupBindingResult> BindGroupCompleteAsync(
            WxContact contact,
            SQLiteConnection db,
            IWeixinSocketClient socketClient,
            IBinggoOrderService orderService,
            BinggoStatisticsService statisticsService,
            IMemberDataService memberDataService,
            IBinggoLotteryService lotteryService,
            Core.V2MemberBindingList? existingMembersBindingList = null,
            Core.V2OrderBindingList? existingOrdersBindingList = null,
            Core.V2CreditWithdrawBindingList? existingCreditWithdrawsBindingList = null);
    }
}

