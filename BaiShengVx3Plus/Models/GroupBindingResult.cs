using BaiShengVx3Plus.Core;

namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 群组绑定结果 DTO
    /// 用于从服务层返回到 UI 层
    /// </summary>
    public class GroupBindingResult
    {
        /// <summary>
        /// 绑定的群组信息
        /// </summary>
        public WxContact Group { get; set; } = null!;
        
        /// <summary>
        /// 会员 BindingList
        /// </summary>
        public V2MemberBindingList? MembersBindingList { get; set; }
        
        /// <summary>
        /// 订单 BindingList
        /// </summary>
        public V2OrderBindingList? OrdersBindingList { get; set; }
        
        /// <summary>
        /// 上下分 BindingList
        /// </summary>
        public V2CreditWithdrawBindingList? CreditWithdrawsBindingList { get; set; }
        
        /// <summary>
        /// 会员数量
        /// </summary>
        public int MemberCount { get; set; }
        
        /// <summary>
        /// 订单数量
        /// </summary>
        public int OrderCount { get; set; }
        
        /// <summary>
        /// 上下分记录数量
        /// </summary>
        public int CreditWithdrawCount { get; set; }
        
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 错误消息（失败时）
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}

