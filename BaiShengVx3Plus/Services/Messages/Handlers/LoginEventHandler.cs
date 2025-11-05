using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Messages;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    /// <summary>
    /// 登录事件处理器
    /// </summary>
    public class LoginEventHandler : IMessageHandler
    {
        private readonly ILogService _logService;
        private readonly IUserInfoService _userInfoService;
        private readonly IDatabaseService _databaseService;

        public ServerMessageType MessageType => ServerMessageType.OnLogin;

        public LoginEventHandler(
            ILogService logService, 
            IUserInfoService userInfoService,
            IDatabaseService databaseService)
        {
            _logService = logService;
            _userInfoService = userInfoService;
            _databaseService = databaseService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                var loginData = JsonSerializer.Deserialize<LoginEventData>(data.GetRawText());
                if (loginData == null) 
                {
                    _logService.Error("LoginEventHandler", "Failed to deserialize login data");
                    return;
                }

                _logService.Info("LoginEventHandler", 
                    $"✅ 微信登录 | Wxid: {loginData.Wxid} | 昵称: {loginData.Nickname}");

                // 检查 wxid 是否为空
                if (string.IsNullOrEmpty(loginData.Wxid))
                {
                    _logService.Warning("LoginEventHandler", "Wxid is empty, skip processing");
                    return;
                }

                // 1. 更新用户信息
                var userInfo = new WxUserInfo
                {
                    Wxid = loginData.Wxid,
                    Nickname = loginData.Nickname ?? string.Empty,
                    Account = loginData.Account ?? string.Empty,
                    Mobile = loginData.Mobile ?? string.Empty,
                    Avatar = loginData.Avatar ?? string.Empty,
                    DataPath = loginData.DataPath ?? string.Empty,
                    CurrentDataPath = loginData.CurrentDataPath ?? string.Empty,
                    DbKey = loginData.DbKey ?? string.Empty
                };

                _userInfoService.UpdateUserInfo(userInfo);

                // 2. 初始化业务数据库（使用 wxid 组合表名）
                await _databaseService.InitializeBusinessDatabaseAsync(loginData.Wxid);
                _logService.Info("LoginEventHandler", $"Business database initialized for wxid: {loginData.Wxid}");

                // 注意：联系人列表的获取由 VxMain 的 UserInfoService_UserInfoUpdated 事件自动触发

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("LoginEventHandler", "Error handling login event", ex);
            }
        }
    }

    /// <summary>
    /// 登出事件处理器
    /// </summary>
    public class LogoutEventHandler : IMessageHandler
    {
        private readonly ILogService _logService;
        private readonly IUserInfoService _userInfoService;

        public ServerMessageType MessageType => ServerMessageType.OnLogout;

        public LogoutEventHandler(ILogService logService, IUserInfoService userInfoService)
        {
            _logService = logService;
            _userInfoService = userInfoService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                var logoutData = JsonSerializer.Deserialize<LoginEventData>(data.GetRawText());
                if (logoutData == null) 
                {
                    _logService.Error("LogoutEventHandler", "Failed to deserialize logout data");
                    return;
                }

                _logService.Info("LogoutEventHandler", 
                    $"❌ 微信登出 | Wxid: {logoutData.Wxid} | 昵称: {logoutData.Nickname}");

                // 1. 清空用户信息
                _userInfoService.ClearUserInfo();

                // 注意：UI 更新由 VxMain 的 UserInfoService_UserInfoUpdated 事件自动处理

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("LogoutEventHandler", "Error handling logout event", ex);
            }
        }
    }
}

