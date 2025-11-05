using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    /// <summary>
    /// 登录事件处理器
    /// </summary>
    public class LoginEventHandler : IMessageHandler
    {
        private readonly ILogService _logService;

        public ServerMessageType MessageType => ServerMessageType.OnLogin;

        public LoginEventHandler(ILogService logService)
        {
            _logService = logService;
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

                // TODO: 处理登录事件
                // 1. 更新用户状态
                // 2. 刷新联系人列表
                // 3. 通知 UI 更新

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

        public ServerMessageType MessageType => ServerMessageType.OnLogout;

        public LogoutEventHandler(ILogService logService)
        {
            _logService = logService;
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

                // TODO: 处理登出事件
                // 1. 清空用户数据
                // 2. 断开连接
                // 3. 通知 UI 更新

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("LogoutEventHandler", "Error handling logout event", ex);
            }
        }
    }
}

