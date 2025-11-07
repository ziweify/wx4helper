using Newtonsoft.Json;

namespace BsBrowserClient.Models
{
    /// <summary>
    /// Socket 命令请求
    /// </summary>
    public class CommandRequest
    {
        /// <summary>
        /// 命令类型: Navigate, Login, GetBalance, PlaceBet, GetStatus, Stop
        /// </summary>
        [JsonProperty("command")]
        public string Command { get; set; } = "";
        
        /// <summary>
        /// 命令数据 (JSON 对象)
        /// </summary>
        [JsonProperty("data")]
        public object? Data { get; set; }
    }
}

