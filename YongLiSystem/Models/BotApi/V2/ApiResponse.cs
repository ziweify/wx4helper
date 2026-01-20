using Newtonsoft.Json;

namespace 永利系统.Models.BotApi.V2
{
    /// <summary>
    /// BotApi V2 版本 - API 响应基类
    /// 
    /// 注意：V2 版本的数据结构可能与 V1 不同
    /// 如果 V2 的 JSON 字段不同，请修改 JsonProperty 特性
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class ApiResponse<T>
    {
        // TODO: 根据 V2 API 的实际字段结构修改以下属性
        
        /// <summary>
        /// 响应代码 (0=成功, 其他=失败)
        /// V2 可能使用不同的字段名，如 "status", "error_code" 等
        /// </summary>
        [JsonProperty("code")]  // 如果 V2 使用不同字段名，请修改
        public int Code { get; set; }
        
        /// <summary>
        /// 响应消息
        /// V2 可能使用不同的字段名，如 "message", "error_msg" 等
        /// </summary>
        [JsonProperty("msg")]  // 如果 V2 使用不同字段名，请修改
        public string Msg { get; set; } = string.Empty;
        
        /// <summary>
        /// 响应数据
        /// V2 可能使用不同的字段名，如 "result", "payload" 等
        /// </summary>
        [JsonProperty("data")]  // 如果 V2 使用不同字段名，请修改
        public T? Data { get; set; }
        
        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonIgnore]
        public bool IsSuccess => Code == 0;
    }
}

