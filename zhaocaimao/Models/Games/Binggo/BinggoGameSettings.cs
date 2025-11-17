using System.Collections.Generic;

namespace BaiShengVx3Plus.Models.Games.Binggo
{
    /// <summary>
    /// 🔥 炳狗游戏配置（包装类）
    /// 用于向后兼容，实际数据存储在 AppConfiguration 中
    /// 
    /// ⚠️ 注意：这个类不再独立保存到文件
    /// 所有配置统一在 ConfigurationService 中管理
    /// </summary>
    public class BinggoGameSettings
    {
        private readonly AppConfiguration _appConfig;
        
        /// <summary>
        /// 构造函数：注入 AppConfiguration
        /// </summary>
        public BinggoGameSettings(AppConfiguration appConfig)
        {
            _appConfig = appConfig;
        }
        
        /// <summary>
        /// 无参构造函数：用于反序列化（已弃用）
        /// </summary>
        [System.Obsolete("请使用依赖注入的构造函数")]
        public BinggoGameSettings()
        {
            _appConfig = new AppConfiguration();
        }
        
        // ========================================
        // 🔥 所有属性都转发到 AppConfiguration
        // ========================================
        
        public Dictionary<string, float> Odds
        {
            get => _appConfig.Odds;
            set => _appConfig.Odds = value;
        }
        
        public float MinBet
        {
            get => _appConfig.MinBet;
            set => _appConfig.MinBet = value;
        }
        
        public float MaxBet
        {
            get => _appConfig.MaxBet;
            set => _appConfig.MaxBet = value;
        }
        
        public float MaxBetPerIssue
        {
            get => _appConfig.MaxBetPerIssue;
            set => _appConfig.MaxBetPerIssue = value;
        }
        
        public int SealSecondsAhead
        {
            get => _appConfig.SealSecondsAhead;
            set => _appConfig.SealSecondsAhead = value;
        }
        
        public int IssueDuration
        {
            get => _appConfig.IssueDuration;
            set => _appConfig.IssueDuration = value;
        }
        
        public bool AutoSendOpenNotice
        {
            get => _appConfig.AutoSendOpenNotice;
            set => _appConfig.AutoSendOpenNotice = value;
        }
        
        public bool AutoSendLotteryResult
        {
            get => _appConfig.AutoSendLotteryResult;
            set => _appConfig.AutoSendLotteryResult = value;
        }
        
        public bool AutoSendSettlementNotice
        {
            get => _appConfig.AutoSendSettlementNotice;
            set => _appConfig.AutoSendSettlementNotice = value;
        }
        
        public string ReplySuccess
        {
            get => _appConfig.ReplySuccess;
            set => _appConfig.ReplySuccess = value;
        }
        
        public string ReplyFailed
        {
            get => _appConfig.ReplyFailed;
            set => _appConfig.ReplyFailed = value;
        }
        
        public string ReplyInsufficientBalance
        {
            get => _appConfig.ReplyInsufficientBalance;
            set => _appConfig.ReplyInsufficientBalance = value;
        }
        
        public string ReplySealed
        {
            get => _appConfig.ReplySealed;
            set => _appConfig.ReplySealed = value;
        }
        
        public string ReplyOpenNotice
        {
            get => _appConfig.ReplyOpenNotice;
            set => _appConfig.ReplyOpenNotice = value;
        }
        
        public string ReplySuspended
        {
            get => _appConfig.ReplySuspended;
            set => _appConfig.ReplySuspended = value;
        }
        
        public bool IsAdminMode
        {
            get => _appConfig.IsRunModeAdmin;
            set => _appConfig.IsRunModeAdmin = value;
        }
    }
}

