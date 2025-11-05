using System;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 用户信息服务接口
    /// </summary>
    public interface IUserInfoService
    {
        /// <summary>
        /// 当前用户信息
        /// </summary>
        WxUserInfo CurrentUser { get; }

        /// <summary>
        /// 更新用户信息（线程安全）
        /// </summary>
        void UpdateUserInfo(WxUserInfo userInfo);

        /// <summary>
        /// 清空用户信息
        /// </summary>
        void ClearUserInfo();

        /// <summary>
        /// 用户信息更新事件
        /// </summary>
        event EventHandler<UserInfoUpdatedEventArgs>? UserInfoUpdated;
    }

    /// <summary>
    /// 用户信息更新事件参数
    /// </summary>
    public class UserInfoUpdatedEventArgs : EventArgs
    {
        public WxUserInfo UserInfo { get; set; }

        public UserInfoUpdatedEventArgs(WxUserInfo userInfo)
        {
            UserInfo = userInfo;
        }
    }
}

