using System;
using Newtonsoft.Json;

namespace 永利系统.Models.BotApi.V2
{
    /// <summary>
    /// BotApi V2 版本 - API 用户信息
    /// 
    /// 注意：V2 版本的数据结构可能与 V1 不同
    /// 如果 V2 的 JSON 字段不同，请修改 JsonProperty 特性
    /// </summary>
    public class ApiUser
    {
        // TODO: 根据 V2 API 的实际字段结构修改以下属性
        
        /// <summary>
        /// 软件名称（V2 可能使用不同的字段名）
        /// </summary>
        [JsonProperty("c_soft_name")]  // 如果 V2 使用不同字段名，请修改
        public string SoftName { get; set; } = string.Empty;
        
        /// <summary>
        /// 认证签名（V2 可能使用不同的字段名）
        /// </summary>
        [JsonProperty("c_sign")]  // 如果 V2 使用不同字段名，请修改
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// 公共 Token（V2 可能使用不同的字段名）
        /// </summary>
        [JsonProperty("c_token_public")]  // 如果 V2 使用不同字段名，请修改
        public string PublicToken { get; set; } = string.Empty;
        
        /// <summary>
        /// 账号过期时间（V2 可能使用不同的字段名）
        /// </summary>
        [JsonProperty("c_off_time")]  // 如果 V2 使用不同字段名，请修改
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

