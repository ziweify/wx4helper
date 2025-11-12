using System.Text.Json.Serialization;

namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 应用程序配置（纯数据模型）
    /// 职责：存储应用级别的设置数据（不包含业务逻辑）
    /// </summary>
    public class AppConfiguration
    {
            public string BsUserName { get; set; } = string.Empty;   //百盛用户名
            public string BsUserPwd { get; set; } = string.Empty;    //百盛密码


        /// <summary>
        ///     运行模式
        /// </summary>
        public bool IsRunModeDev { get; set; }    //开发模式, 模拟联系人数据,模拟群数据,模拟恢复消息,可以控制界面显示模拟操作相关内容
        public bool IsRunModeAdmin { get; set; }  //管理模式(可以手动输入绑定群
        public bool IsRunModeBoss { get; set; }   //老板模式

        /// <summary>
        ///     软件模式
        /// </summary>
        public bool IsSoftModeVx { get; set; }
        public bool IsSoftModeFeitian { get; set; }


        /// <summary>
        /// 收单开关（是否接收微信下注消息）
        /// </summary>
        //[JsonPropertyName("isOrdersTaskingEnabled")]
        public bool Is收单开关 { get; set; } = false;
        
        /// <summary>
        /// 自动投注开关（飞单）
        /// </summary>
        //[JsonPropertyName("isAutoBetEnabled")]
        public bool Is飞单开关 { get; set; } = false;
        
        /// <summary>
        /// 提前封盘秒数
        /// </summary>
        //[JsonPropertyName("sealSecondsAhead")]
        public int N封盘提前秒数 { get; set; } = 45;
    }
}

