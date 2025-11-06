using System;

namespace BaiShengVx3Plus.Models.Api
{
    /// <summary>
    /// 白胜系统 API 用户
    /// 
    /// 用于系统级认证，包含登录凭证和权限信息
    /// </summary>
    public class BsApiUser
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 密码（仅用于登录，不持久化）
        /// </summary>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// 认证 Token
        /// </summary>
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// Token 过期时间
        /// </summary>
        public DateTime TokenExpiry { get; set; }
        
        /// <summary>
        /// 账号有效期
        /// </summary>
        public DateTime ValidUntil { get; set; }
        
        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin { get; set; }
        
        /// <summary>
        /// 用户 ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Token 是否有效
        /// </summary>
        public bool IsTokenValid => !string.IsNullOrEmpty(Token) && DateTime.Now < TokenExpiry;
        
        /// <summary>
        /// 账号是否在有效期内
        /// </summary>
        public bool IsAccountValid => DateTime.Now < ValidUntil;
    }
}

