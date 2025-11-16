using zhaocaimao.Models;

namespace zhaocaimao.Contracts
{
    /// <summary>
    /// 会员数据访问服务接口
    /// 
    /// 提供全局访问会员数据的能力
    /// </summary>
    public interface IMemberDataService
    {
        /// <summary>
        /// 根据微信ID获取会员
        /// </summary>
        V2Member? GetMemberByWxid(string wxid);
        
        /// <summary>
        /// 设置当前群组ID
        /// </summary>
        void SetCurrentGroupWxid(string groupWxid);
        
        /// <summary>
        /// 获取当前群组ID
        /// </summary>
        string? GetCurrentGroupWxid();
    }
}

