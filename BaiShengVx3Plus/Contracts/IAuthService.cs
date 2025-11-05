using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 认证服务接口
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 登录
        /// </summary>
        Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password);

        /// <summary>
        /// 登出
        /// </summary>
        void Logout();

        /// <summary>
        /// 获取当前用户
        /// </summary>
        User? GetCurrentUser();

        /// <summary>
        /// 是否已登录
        /// </summary>
        bool IsAuthenticated { get; }
    }
}

