using Newtonsoft.Json;

namespace BsBrowserClient.Models
{
    /// <summary>
    /// Socket 命令响应
    /// </summary>
    public class CommandResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }
        
        /// <summary>
        /// 返回数据 (JSON 对象)
        /// </summary>
        [JsonProperty("data")]
        public object? Data { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        [JsonProperty("errorMessage")]
        public string? ErrorMessage { get; set; }
    }
}

