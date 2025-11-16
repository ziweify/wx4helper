using System.Linq;
using zhaocaimao.Contracts;
using zhaocaimao.Models;
using zhaocaimao.Core;

namespace zhaocaimao.Services
{
    /// <summary>
    /// 会员数据访问服务实现
    /// 
    /// 提供全局访问会员数据的能力
    /// 注意：这是一个临时解决方案，后续应考虑更好的架构设计
    /// </summary>
    public class MemberDataService : IMemberDataService
    {
        private V2MemberBindingList? _membersBindingList;
        private string? _currentGroupWxid;
        
        /// <summary>
        /// 设置会员列表（由 VxMain 调用）
        /// </summary>
        public void SetMembersBindingList(V2MemberBindingList? bindingList)
        {
            _membersBindingList = bindingList;
        }
        
        /// <summary>
        /// 根据微信ID获取会员
        /// </summary>
        public V2Member? GetMemberByWxid(string wxid)
        {
            if (_membersBindingList == null || string.IsNullOrEmpty(wxid))
            {
                return null;
            }
            
            return _membersBindingList.FirstOrDefault(m => m.Wxid == wxid);
        }
        
        /// <summary>
        /// 设置当前群组ID
        /// </summary>
        public void SetCurrentGroupWxid(string groupWxid)
        {
            _currentGroupWxid = groupWxid;
        }
        
        /// <summary>
        /// 获取当前群组ID
        /// </summary>
        public string? GetCurrentGroupWxid()
        {
            return _currentGroupWxid;
        }
    }
}

