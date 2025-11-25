using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace zhaocaimao.Models
{
    /// <summary>
    /// 应用程序配置（纯数据模型）
    /// 职责：存储应用级别的设置数据（不包含业务逻辑）
    /// </summary>
    public class AppConfiguration
    {
        // ========================================
        // 登录信息
        // ========================================
        
        public string BsUserName { get; set; } = string.Empty;   //百盛用户名
        public string BsUserPwd { get; set; } = string.Empty;    //百盛密码（Base64加密）
        public bool IsRememberPassword { get; set; } = false;    //记住密码

        // ========================================
        // 运行模式
        // ========================================
        
        public bool IsRunModeDev { get; set; }    //开发模式, 模拟联系人数据,模拟群数据,模拟恢复消息,可以控制界面显示模拟操作相关内容
        public bool IsRunModeAdmin { get; set; }  //管理模式(可以手动输入绑定群
        public bool IsRunModeBoss { get; set; }   //老板模式

        // ========================================
        // 软件模式
        // ========================================
        
        public bool IsSoftModeVx { get; set; }
        public bool IsSoftModeFeitian { get; set; }

        // ========================================
        // 业务开关
        // ========================================
        
        /// <summary>
        /// 收单开关（是否接收微信下注消息）
        /// </summary>
        public bool Is收单开关 { get; set; } = false;
        
        /// <summary>
        /// 自动投注开关（飞单）
        /// </summary>
        public bool Is飞单开关 { get; set; } = false;
        
        /// <summary>
        /// 收单关闭时不发送系统消息（开盘、封盘、开奖、结算消息）
        /// 默认 true = 收单关闭时也不发送系统消息
        /// </summary>
        public bool 收单关闭时不发送系统消息 { get; set; } = true;
        
        // ========================================
        // 🔥 游戏规则配置（从 BinggoGameSettings 迁移过来）
        // ========================================
        
        /// <summary>
        /// 🔥 当前选择的盘口（界面状态，保存在 appsettings.json）
        /// 用于主窗口显示当前使用的盘口，与配置管理中的配置数据不冲突
        /// 配置管理：管理多个配置（保存在数据库 AutoBetConfigs 表）
        /// 界面状态：显示当前使用的配置（保存在 appsettings.json）
        /// </summary>
        public string CurrentSelectedPlatform { get; set; } = "";
        
        /// <summary>
        /// 微信订单统一赔率（用于订单结算）
        /// 独立于网站投注赔率，用于微信下单时的订单结算计算
        /// 注意：网站投注赔率在浏览器端（BsBrowserClient）通过拦截网站响应动态获取，不在此配置中
        /// </summary>
        public float WechatOrderOdds { get; set; } = 1.97f;
        
        /// <summary>
        /// 🔥 赔率配置字典（用于 BinggoGameSettings 兼容）
        /// 注意：此属性主要用于向后兼容，实际使用中建议使用 WechatOrderOdds
        /// </summary>
        public Dictionary<string, float> Odds { get; set; } = new Dictionary<string, float>();
        
        /// <summary>
        /// 最小单注金额
        /// </summary>
        public float MinBet { get; set; } = 1.0f;
        
        /// <summary>
        /// 最大单注金额
        /// </summary>
        public float MaxBet { get; set; } = 10000.0f;
        
        /// <summary>
        /// 单期最大投注总额
        /// </summary>
        public float MaxBetPerIssue { get; set; } = 50000.0f;
        
        /// <summary>
        /// 🔥 提前封盘秒数（统一使用这个，删除旧的 N封盘提前秒数）
        /// 参考 F5BotV2: reduceCloseSeconds，默认 49 秒
        /// 用于计算：倒计时 = 开奖时间 - 当前时间 - 提前封盘秒数
        /// </summary>
        public int SealSecondsAhead { get; set; } = 49;
        
        /// <summary>
        /// 每期时长（秒）
        /// </summary>
        public int IssueDuration { get; set; } = 300;
        
        /// <summary>
        /// 是否启用自动开盘提示
        /// </summary>
        public bool AutoSendOpenNotice { get; set; } = true;
        
        /// <summary>
        /// 是否启用自动开奖通知
        /// </summary>
        public bool AutoSendLotteryResult { get; set; } = true;
        
        /// <summary>
        /// 是否启用自动结算通知
        /// </summary>
        public bool AutoSendSettlementNotice { get; set; } = true;
        
        // ========================================
        // 回复消息配置
        // ========================================
        
        /// <summary>
        /// 下注成功回复
        /// </summary>
        public string ReplySuccess { get; set; } = "已进仓！";
        
        /// <summary>
        /// 下注失败回复
        /// </summary>
        public string ReplyFailed { get; set; } = "客官我有点不明白！";
        
        /// <summary>
        /// 余额不足回复
        /// </summary>
        public string ReplyInsufficientBalance { get; set; } = "客官你的荷包是否不足！";
        
        /// <summary>
        /// 已封盘回复
        /// </summary>
        public string ReplySealed { get; set; } = "已封盘，请等待下期！";
        
        /// <summary>
        /// 开盘提示
        /// </summary>
        public string ReplyOpenNotice { get; set; } = "---------线下开始---------";
        
        /// <summary>
        /// 暂停服务回复
        /// </summary>
        public string ReplySuspended { get; set; } = "系统维护中，暂停服务！";
        
        // ========================================
        // 开发模式配置
        // ========================================
        
        /// <summary>
        /// 🔧 开发模式：当前选中的会员（用于测试）
        /// </summary>
        public string RunDevCurrentMember { get; set; } = string.Empty;
        
        /// <summary>
        /// 🔧 开发模式：发送的测试消息内容
        /// </summary>
        public string RunDevSendMessage { get; set; } = "大12310";
    }
}

