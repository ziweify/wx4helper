using Newtonsoft.Json;

namespace YongLiSystem.Models.BotApi.V1
{
    /// <summary>
    /// BotApi V1 版本 - API 响应基类
    /// 
    /// JSON 字段映射：
    /// - code: 响应代码 (0=成功, 其他=失败)
    /// - msg: 响应消息
    /// - data: 响应数据
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 响应代码 (0=成功, 其他=失败)
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }
        
        /// <summary>
        /// 响应消息
        /// </summary>
        [JsonProperty("msg")]
        public string Msg { get; set; } = string.Empty;
        
        /// <summary>
        /// 响应数据
        /// </summary>
        [JsonProperty("data")]
        public T? Data { get; set; }
        
        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonIgnore]
        public bool IsSuccess => Code == 0;
    }
}

