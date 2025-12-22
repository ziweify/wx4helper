using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using 永利系统.Models.Wechat;
using 永利系统.Models.Wechat.Events;

namespace 永利系统.Contracts.Wechat
{
    /// <summary>
    /// 微信服务接口
    /// 
    /// 核心功能：
    /// 1. 启动微信
    /// 2. 微信刷新（重新获取微信联系人等数据）
    /// 3. 微信绑定（绑定群组）
    /// </summary>
    public interface IWechatService
    {
        // ========================================
        // 属性
        // ========================================

        /// <summary>
        /// 当前连接状态
        /// </summary>
        WechatConnectionState CurrentState { get; }

        /// <summary>
        /// 当前绑定的群组ID
        /// </summary>
        string? CurrentGroupWxId { get; }

        // ========================================
        // 方法
        // ========================================

        /// <summary>
        /// 启动微信（完整流程：启动微信→注入DLL→连接Socket）
        /// </summary>
        /// <param name="forceRestart">是否强制重新启动</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task<bool> StartWechatAsync(bool forceRestart = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// 刷新微信数据（重新获取联系人、群组等数据）
        /// </summary>
        Task<bool> RefreshWechatDataAsync();

        /// <summary>
        /// 绑定群组
        /// </summary>
        /// <param name="groupWxId">群组微信ID</param>
        Task<bool> BindGroupAsync(string groupWxId);

        /// <summary>
        /// 获取联系人列表
        /// </summary>
        Task<List<Contact>> GetContactsAsync();

        /// <summary>
        /// 获取群组列表
        /// </summary>
        Task<List<Contact>> GetGroupsAsync();

        /// <summary>
        /// 断开连接
        /// </summary>
        Task DisconnectAsync();

        // ========================================
        // 事件
        // ========================================

        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        event EventHandler<WechatConnectionStateChangedEventArgs>? ConnectionStateChanged;

        /// <summary>
        /// 联系人更新事件
        /// </summary>
        event EventHandler<List<Contact>>? ContactsUpdated;

        /// <summary>
        /// 群组绑定事件
        /// </summary>
        event EventHandler<string>? GroupBound;
    }
}

