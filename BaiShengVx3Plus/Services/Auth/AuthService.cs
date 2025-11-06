using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Services.Auth
{
    /// <summary>
    /// 认证服务实现
    /// 🔥 完全调用真实的 WebAPI（F5BotV2）
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IBsWebApiService _webApiService;
        private readonly ILogService _logService;
        private User? _currentUser;

        public AuthService(IBsWebApiService webApiService, ILogService logService)
        {
            _webApiService = webApiService;
            _logService = logService;
        }

        public bool IsAuthenticated => _currentUser != null;

        public User? GetCurrentUser() => _currentUser;

        public async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password)
        {
            try
            {
                _logService.Info("AuthService", $"🔐 开始登录验证: {username}");
                
                // 🔥 调用真实的 WebAPI 登录接口（F5BotV2）
                bool success = await _webApiService.LoginAsync(username, password);
                
                if (success)
                {
                    var apiUser = _webApiService.CurrentUser;
                    
                    // 🔥 将 API 用户数据转换为本地 User 模型
                    _currentUser = new User
                    {
                        Id = 1,  // 本地 ID（可以从 API 获取）
                        UserName = username,
                        RealName = apiUser?.Username ?? username,
                        Role = "Admin",  // 从 API 获取角色
                        IsVip = true,
                        VipExpireTime = apiUser?.ValidUntil ?? DateTime.Now.AddYears(1),
                        Balance = 0,  // 从 API 获取余额（如果有）
                        IsOnline = true
                    };
                    
                    _logService.Info("AuthService", $"✅ 登录成功: {username}");
                    return (true, "登录成功", _currentUser);
                }
                else
                {
                    string error = _webApiService.GetLastError();
                    _logService.Warning("AuthService", $"❌ 登录失败: {error}");
                    return (false, error, null);
                }
            }
            catch (Exception ex)
            {
                _logService.Error("AuthService", $"登录异常: {ex.Message}", ex);
                return (false, $"登录异常: {ex.Message}", null);
            }
        }

        public void Logout()
        {
            _logService.Info("AuthService", $"用户登出: {_currentUser?.UserName}");
            _webApiService.Logout();
            _currentUser = null;
        }
    }
}

