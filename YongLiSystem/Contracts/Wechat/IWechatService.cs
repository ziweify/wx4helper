using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using 永利系统.Models.Wechat;
using 永利系统.Models.Wechat.Events;

namespace 永利系统.Contracts.Wechat
{
    /// <summary>
    /// 微信服务契约
    /// 
    /// 📋 契约说明：
    /// - 前置条件：服务必须正确初始化（通过构造函数注入依赖）
    /// - 后置条件：所有异步操作成功返回 true，失败返回 false 或抛出异常
    /// - 不变式：服务运行期间，CurrentState 必须准确反映实际连接状态
    /// 
    /// 核心功能：
    /// 1. 启动微信（完整流程：启动微信→注入DLL→连接Socket）
    /// 2. 微信刷新（重新获取微信联系人等数据）
    /// 3. 微信绑定（绑定群组）
    /// </summary>
    public interface IWechatService
    {
        // ========================================
        // 属性（不变式）
        // ========================================

        /// <summary>
        /// 当前连接状态
        /// 
        /// 📋 不变式：此属性必须始终准确反映实际连接状态，线程安全
        /// </summary>
        WechatConnectionState CurrentState { get; }

        /// <summary>
        /// 当前绑定的群组ID
        /// 
        /// 📋 不变式：
        /// - 未绑定时为 null
        /// - 绑定后不为空，且为有效的微信群组ID
        /// </summary>
        string? CurrentGroupWxId { get; }

        // ========================================
        // 方法（契约定义）
        // ========================================

        /// <summary>
        /// 启动微信（完整流程：启动微信→注入DLL→连接Socket）
        /// 
        /// 📋 契约：
        /// - 前置条件：无（可在任何状态下调用）
        /// - 后置条件：成功返回 true 且 CurrentState = Connected；失败返回 false 或抛出异常
        /// - 异常：操作失败时抛出 InvalidOperationException 或 TimeoutException
        /// </summary>
        /// <param name="forceRestart">是否强制重新启动（默认 false）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功启动</returns>
        /// <exception cref="InvalidOperationException">微信进程启动失败或DLL注入失败</exception>
        /// <exception cref="TimeoutException">连接超时</exception>
        Task<bool> StartWechatAsync(bool forceRestart = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// 刷新微信数据（重新获取联系人、群组等数据）
        /// 
        /// 📋 契约：
        /// - 前置条件：CurrentState 必须为 Connected
        /// - 后置条件：成功返回 true 并触发 ContactsUpdated 事件
        /// - 异常：未连接时抛出 InvalidOperationException
        /// </summary>
        /// <returns>是否成功刷新</returns>
        /// <exception cref="InvalidOperationException">微信未连接</exception>
        Task<bool> RefreshWechatDataAsync();

        /// <summary>
        /// 绑定群组
        /// 
        /// 📋 契约：
        /// - 前置条件：groupWxId 不能为 null 或空字符串，且 CurrentState 必须为 Connected
        /// - 后置条件：成功返回 true，CurrentGroupWxId 被设置，并触发 GroupBound 事件
        /// - 异常：参数无效或未连接时抛出异常
        /// </summary>
        /// <param name="groupWxId">群组微信ID（不能为空）</param>
        /// <returns>是否成功绑定</returns>
        /// <exception cref="ArgumentNullException">groupWxId 为 null 或空</exception>
        /// <exception cref="InvalidOperationException">微信未连接</exception>
        Task<bool> BindGroupAsync(string groupWxId);

        /// <summary>
        /// 获取联系人列表
        /// 
        /// 📋 契约：
        /// - 前置条件：CurrentState 必须为 Connected
        /// - 后置条件：永不返回 null，最坏情况返回空列表
        /// - 异常：未连接时抛出 InvalidOperationException
        /// </summary>
        /// <returns>联系人列表（永不为 null）</returns>
        /// <exception cref="InvalidOperationException">微信未连接</exception>
        Task<List<Contact>> GetContactsAsync();

        /// <summary>
        /// 获取群组列表
        /// 
        /// 📋 契约：
        /// - 前置条件：CurrentState 必须为 Connected
        /// - 后置条件：永不返回 null，最坏情况返回空列表
        /// - 异常：未连接时抛出 InvalidOperationException
        /// </summary>
        /// <returns>群组列表（永不为 null）</returns>
        /// <exception cref="InvalidOperationException">微信未连接</exception>
        Task<List<Contact>> GetGroupsAsync();

        /// <summary>
        /// 断开连接
        /// 
        /// 📋 契约：
        /// - 前置条件：无（可在任何状态下调用）
        /// - 后置条件：CurrentState = Disconnected，CurrentGroupWxId = null
        /// - 异常：不抛出异常（失败时仅记录日志）
        /// </summary>
        Task DisconnectAsync();

        // ========================================
        // 事件（契约定义）
        // ========================================

        /// <summary>
        /// 连接状态变化事件
        /// 
        /// 📋 契约：状态变化时必须触发此事件，且事件参数包含旧状态和新状态
        /// </summary>
        event EventHandler<WechatConnectionStateChangedEventArgs>? ConnectionStateChanged;

        /// <summary>
        /// 联系人更新事件
        /// 
        /// 📋 契约：调用 RefreshWechatDataAsync 成功后必须触发，参数永不为 null
        /// </summary>
        event EventHandler<List<Contact>>? ContactsUpdated;

        /// <summary>
        /// 群组绑定事件
        /// 
        /// 📋 契约：调用 BindGroupAsync 成功后必须触发，参数为绑定的群组ID
        /// </summary>
        event EventHandler<string>? GroupBound;
    }
}

