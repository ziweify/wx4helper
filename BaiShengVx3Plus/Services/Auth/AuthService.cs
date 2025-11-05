using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Services.Auth
{
    /// <summary>
    /// 认证服务实现
    /// </summary>
    public class AuthService : IAuthService
    {
        private User? _currentUser;

        public bool IsAuthenticated => _currentUser != null;

        public User? GetCurrentUser() => _currentUser;

        public async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password)
        {
            // 模拟异步登录验证
            await Task.Delay(500);

            // TODO: 实现真实的登录逻辑
            // 这里是示例代码
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "用户名或密码不能为空", null);
            }

            // 模拟验证（实际应该调用API或数据库）
            if (username == "admin" && password == "admin")
            {
                _currentUser = new User
                {
                    Id = 1,
                    UserName = username,
                    RealName = "管理员",
                    Role = "Admin",
                    IsVip = true,
                    VipExpireTime = DateTime.Now.AddYears(1),
                    Balance = 2354.00m,
                    IsOnline = true
                };
                return (true, "登录成功", _currentUser);
            }

            return (false, "用户名或密码错误", null);
        }

        public void Logout()
        {
            _currentUser = null;
        }
    }
}

