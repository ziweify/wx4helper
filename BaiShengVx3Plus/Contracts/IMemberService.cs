using System;
using System.Collections.Generic;
using System.ComponentModel;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Core;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 会员服务接口（简化版，配合 PropertyChangeTracker 使用）
    /// 注意：修改会员属性后，PropertyChangeTracker 会自动保存单个字段
    /// </summary>
    public interface IMemberService
    {
        /// <summary>
        /// 获取所有会员（从数据库，自动追踪属性变化）
        /// </summary>
        TrackableBindingList<V2Member> GetAllMembers();

        /// <summary>
        /// 根据ID获取会员（自动追踪属性变化）
        /// </summary>
        V2Member? GetMemberById(long id);

        /// <summary>
        /// 添加会员（立即写入数据库，自动追踪属性变化）
        /// </summary>
        /// <returns>新增的会员ID</returns>
        long AddMember(V2Member member);

        /// <summary>
        /// 删除会员（立即从数据库删除，停止追踪）
        /// </summary>
        void DeleteMember(long id);

        /// <summary>
        /// 会员数据变化事件（通知 UI 更新）
        /// </summary>
        event EventHandler? MembersChanged;
    }
}

