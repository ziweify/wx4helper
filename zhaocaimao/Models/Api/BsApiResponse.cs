using Newtonsoft.Json;

namespace zhaocaimao.Models.Api
{
    /// <summary>
    /// 白胜系统 API 响应
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class BsApiResponse<T>
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

