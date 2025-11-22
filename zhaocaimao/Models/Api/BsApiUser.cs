using System;

namespace zhaocaimao.Models.Api
{
    /// <summary>
    /// API 用户信息
    /// 🔥 完全参考 F5BotV2 的 BoterLoginReponse
    /// 
    /// 用于系统级认证，包含登录凭证和权限信息
    /// </summary>
    public class BsApiUser
    {
        /// <summary>
        /// 软件名称
        /// 🔥 对应 F5BotV2 的 c_soft_name
        /// </summary>
        [Newtonsoft.Json.JsonProperty("c_soft_name")]
        public string SoftName { get; set; } = string.Empty;
        
        /// <summary>
        /// 认证签名（核心字段）
        /// 🔥 对应 F5BotV2 的 c_sign
        /// </summary>
        [Newtonsoft.Json.JsonProperty("c_sign")]
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// 公共 Token
        /// 🔥 对应 F5BotV2 的 c_token_public
        /// </summary>
        [Newtonsoft.Json.JsonProperty("c_token_public")]
        public string PublicToken { get; set; } = string.Empty;
        
        /// <summary>
        /// 账号过期时间
        /// 🔥 对应 F5BotV2 的 c_off_time
        /// </summary>
        [Newtonsoft.Json.JsonProperty("c_off_time")]
        public DateTime ValidUntil { get; set; }
        
        /// <summary>
        /// Token 是否有效
        /// </summary>
        public bool IsTokenValid => !string.IsNullOrEmpty(Token);
        
        /// <summary>
        /// 账号是否在有效期内
        /// </summary>
        public bool IsAccountValid => DateTime.Now < ValidUntil;
        
        // ============================================
        // 🔥 以下为兼容字段，保留用于其他地方引用
        // ============================================
        
        /// <summary>
        /// 用户名（扩展字段，F5BotV2 响应中没有）
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 密码（仅用于登录，不持久化）
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Token 过期时间（扩展字段）
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DateTime TokenExpiry { get; set; }
        
        /// <summary>
        /// 是否为管理员（扩展字段）
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsAdmin { get; set; }
        
        /// <summary>
        /// 用户 ID（扩展字段）
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public int UserId { get; set; }
    }
}

