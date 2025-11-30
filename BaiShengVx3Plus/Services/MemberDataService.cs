using System.Linq;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Core;

namespace BaiShengVx3Plus.Services
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
        /// 🔥 关键修复：使用锁保护，防止刷新/绑定期间的并发问题
        /// </summary>
        public V2Member? GetMemberByWxid(string wxid)
        {
            // 🔥 使用 BindingListUpdateLock 保护读取操作
            // 防止在 Clear() 和 Add() 之间读取到 null 或旧对象
            lock (Core.ResourceLocks.BindingListUpdateLock)
            {
                if (_membersBindingList == null || string.IsNullOrEmpty(wxid))
                {
                    return null;
                }
                
                return _membersBindingList.FirstOrDefault(m => m.Wxid == wxid);
            }
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

