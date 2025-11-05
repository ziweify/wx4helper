using System;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Services.UserInfo
{
    /// <summary>
    /// 用户信息服务实现（线程安全）
    /// </summary>
    public class UserInfoService : IUserInfoService
    {
        private readonly ILogService _logService;
        private readonly object _lockObject = new object();
        private WxUserInfo _currentUser = new WxUserInfo();

        /// <summary>
        /// 当前用户信息
        /// </summary>
        public WxUserInfo CurrentUser
        {
            get
            {
                lock (_lockObject)
                {
                    return _currentUser;
                }
            }
        }

        /// <summary>
        /// 用户信息更新事件
        /// </summary>
        public event EventHandler<UserInfoUpdatedEventArgs>? UserInfoUpdated;

        public UserInfoService(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// 更新用户信息（线程安全）
        /// </summary>
        public void UpdateUserInfo(WxUserInfo userInfo)
        {
            lock (_lockObject)
            {
                _currentUser.Wxid = userInfo.Wxid;
                _currentUser.Nickname = userInfo.Nickname;
                _currentUser.Account = userInfo.Account;
                _currentUser.Mobile = userInfo.Mobile;
                _currentUser.Avatar = userInfo.Avatar;
                _currentUser.DataPath = userInfo.DataPath;
                _currentUser.CurrentDataPath = userInfo.CurrentDataPath;
                _currentUser.DbKey = userInfo.DbKey;

                _logService.Info("UserInfoService", 
                    $"用户信息已更新: {_currentUser.Nickname} ({_currentUser.Wxid})");
            }

            // 触发事件（在锁外）
            OnUserInfoUpdated(new UserInfoUpdatedEventArgs(_currentUser));
        }

        /// <summary>
        /// 清空用户信息
        /// </summary>
        public void ClearUserInfo()
        {
            lock (_lockObject)
            {
                _currentUser = new WxUserInfo();
                _logService.Info("UserInfoService", "用户信息已清空");
            }

            // 触发事件
            OnUserInfoUpdated(new UserInfoUpdatedEventArgs(_currentUser));
        }

        /// <summary>
        /// 触发用户信息更新事件
        /// </summary>
        protected virtual void OnUserInfoUpdated(UserInfoUpdatedEventArgs e)
        {
            UserInfoUpdated?.Invoke(this, e);
        }
    }
}

