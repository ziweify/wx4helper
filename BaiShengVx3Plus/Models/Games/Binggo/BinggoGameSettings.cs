using System.Collections.Generic;

namespace BaiShengVx3Plus.Models.Games.Binggo
{
    /// <summary>
    /// 炳狗游戏配置
    /// </summary>
    public class BinggoGameSettings
    {
        /// <summary>
        /// 赔率配置 (例如：{"大": 1.95, "小": 1.95, "单": 1.95, "双": 1.95})
        /// </summary>
        public Dictionary<string, float> Odds { get; set; } = new()
        {
            { "大", 1.95f },
            { "小", 1.95f },
            { "单", 1.95f },
            { "双", 1.95f },
            { "龙", 1.95f },
            { "虎", 1.95f }
        };
        
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
        /// 提前封盘秒数（参考 F5BotV2: reduceCloseSeconds，默认 49 秒）
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
        // 🔥 回复消息配置
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
    }
}

