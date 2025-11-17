using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Services.Configuration;

namespace BaiShengVx3Plus.Services.Games.Binggo
{
    /// <summary>
    /// 🔥 炳狗游戏配置服务（兼容性包装）
    /// 实际配置存储在 ConfigurationService 中
    /// 
    /// ⚠️ 注意：此类仅用于向后兼容
    /// 新代码应该直接使用 ConfigurationService
    /// </summary>
    public class BinggoGameSettingsService
    {
        private readonly ILogService _logService;
        private readonly ConfigurationService _configService;
        
        public BinggoGameSettingsService(ILogService logService, ConfigurationService configService)
        {
            _logService = logService;
            _configService = configService;
            
            _logService.Info("BinggoGameSettings", "⚠️ BinggoGameSettingsService 已弃用，配置统一由 ConfigurationService 管理");
            _logService.Info("BinggoGameSettings", $"✅ 当前配置: MinBet={_configService.GetMinBet()}, MaxBet={_configService.GetMaxBet()}, SealSecondsAhead={_configService.GetSealSecondsAhead()}");
        }
        
        /// <summary>
        /// 加载游戏配置（已弃用，配置由 ConfigurationService 自动加载）
        /// </summary>
        [System.Obsolete("配置已统一由 ConfigurationService 管理，无需手动加载")]
        public void LoadSettings()
        {
            _logService.Info("BinggoGameSettings", "⚠️ LoadSettings() 已弃用，配置由 ConfigurationService 自动加载");
        }
        
        /// <summary>
        /// 保存游戏配置（已弃用，配置由 ConfigurationService 自动保存）
        /// </summary>
        [System.Obsolete("配置已统一由 ConfigurationService 管理，修改配置时会自动保存")]
        public void SaveSettings()
        {
            _configService.SaveConfiguration();
            _logService.Info("BinggoGameSettings", "⚠️ SaveSettings() 已弃用，配置由 ConfigurationService 自动保存");
        }
    }
}

