using System;
using Newtonsoft.Json;

namespace 永利系统.Models.BotApi.V1
{
    /// <summary>
    /// BotApi V1 版本 - API 用户信息
    /// 用于系统级认证，包含登录凭证和权限信息
    /// 
    /// JSON 字段映射：
    /// - c_soft_name: 软件名称
    /// - c_sign: 认证签名（Token）
    /// - c_token_public: 公共 Token
    /// - c_off_time: 账号过期时间
    /// </summary>
    public class ApiUser
    {
        /// <summary>
        /// 软件名称
        /// </summary>
        [JsonProperty("c_soft_name")]
        public string SoftName { get; set; } = string.Empty;
        
        /// <summary>
        /// 认证签名（核心字段）
        /// </summary>
        [JsonProperty("c_sign")]
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// 公共 Token
        /// </summary>
        [JsonProperty("c_token_public")]
        public string PublicToken { get; set; } = string.Empty;
        
        /// <summary>
        /// 账号过期时间
        /// </summary>
        [JsonProperty("c_off_time")]
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// Token 是否有效
        /// </summary>
        [JsonIgnore]
        public bool IsTokenValid => !string.IsNullOrEmpty(Token);
        
        /// <summary>
        /// 账号是否在有效期内
        /// </summary>
        [JsonIgnore]
        public bool IsAccountValid => DateTime.Now < ValidUntil;
        
        /// <summary>
        /// 用户名（扩展字段，登录时设置）
        /// </summary>
        [JsonIgnore]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 密码（仅用于登录，不持久化）
        /// </summary>
        [JsonIgnore]
        public string Password { get; set; } = string.Empty;
    }
}

