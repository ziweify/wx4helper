using System.Text.Json.Serialization;

namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 服务器推送消息类型
    /// </summary>
    public enum ServerMessageType
    {
        /// <summary>
        /// 聊天消息
        /// </summary>
        OnMessage,

        /// <summary>
        /// 用户登录
        /// </summary>
        OnLogin,

        /// <summary>
        /// 用户登出
        /// </summary>
        OnLogout,

        /// <summary>
        /// 群成员加入
        /// </summary>
        OnMemberJoin,

        /// <summary>
        /// 群成员退出
        /// </summary>
        OnMemberLeave,

        /// <summary>
        /// 心跳
        /// </summary>
        OnHeartbeat,

        /// <summary>
        /// 未知消息
        /// </summary>
        Unknown
    }

    /// <summary>
    /// 聊天消息数据
    /// </summary>
    public class ChatMessageData
    {
        [JsonPropertyName("sender")]
        public string Sender { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("receiver")]
        public string Receiver { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("fromChatroom")]
        public bool FromChatroom { get; set; }

        [JsonPropertyName("receiver1")]
        public string Receiver1 { get; set; } = string.Empty;

        [JsonPropertyName("receiver2")]
        public string Receiver2 { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录/登出事件数据
    /// </summary>
    public class LoginEventData
    {
        [JsonPropertyName("wxid")]
        public string Wxid { get; set; } = string.Empty;

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [JsonPropertyName("account")]
        public string Account { get; set; } = string.Empty;

        [JsonPropertyName("mobile")]
        public string Mobile { get; set; } = string.Empty;

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; } = string.Empty;

        [JsonPropertyName("dataPath")]
        public string DataPath { get; set; } = string.Empty;

        [JsonPropertyName("currentDataPath")]
        public string CurrentDataPath { get; set; } = string.Empty;

        [JsonPropertyName("dbKey")]
        public string DbKey { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// 群成员变动数据
    /// </summary>
    public class MemberEventData
    {
        [JsonPropertyName("groupId")]
        public string GroupId { get; set; } = string.Empty;

        [JsonPropertyName("memberWxid")]
        public string MemberWxid { get; set; } = string.Empty;

        [JsonPropertyName("memberNickname")]
        public string MemberNickname { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }
}

