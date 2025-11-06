using System.Threading.Tasks;
using BaiShengVx3Plus.Models.Api;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 白胜系统 WebAPI 服务接口
    /// 
    /// 提供系统级认证和用户管理功能
    /// </summary>
    public interface IBsWebApiService
    {
        /// <summary>
        /// 是否已认证
        /// </summary>
        bool IsAuthenticated { get; }
        
        /// <summary>
        /// 当前用户
        /// </summary>
        BsApiUser? CurrentUser { get; }
        
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>是否成功</returns>
        Task<bool> LoginAsync(string username, string password);
        
        /// <summary>
        /// 登出
        /// </summary>
        void Logout();
        
        /// <summary>
        /// 获取最后一次错误消息
        /// </summary>
        string GetLastError();
        
        /// <summary>
        /// 刷新 Token
        /// </summary>
        Task<bool> RefreshTokenAsync();
    }
}

