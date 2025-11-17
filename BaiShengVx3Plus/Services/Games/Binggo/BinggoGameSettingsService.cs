using System;
using System.IO;
using System.Text.Json;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models.Games.Binggo;

namespace BaiShengVx3Plus.Services.Games.Binggo
{
    /// <summary>
    /// 炳狗游戏配置服务
    /// 负责加载和保存游戏配置
    /// </summary>
    public class BinggoGameSettingsService
    {
        private readonly ILogService _logService;
        private readonly BinggoGameSettings _settings;
        private readonly string _configFilePath;
        
        public BinggoGameSettingsService(ILogService logService, BinggoGameSettings settings)
        {
            _logService = logService;
            _settings = settings;
            
            // 🔥 配置文件路径：统一使用 LocalApplicationData（与数据库、日志、图片等放在一起）
            // LocalApplicationData = %AppData%\Local\BaiShengVx3Plus
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BaiShengVx3Plus"
            );
            
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            _configFilePath = Path.Combine(appDataPath, "BinggoGameSettings.json");
            
            // 启动时加载配置
            LoadSettings();
        }
        
        /// <summary>
        /// 加载游戏配置
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var loadedSettings = JsonSerializer.Deserialize<BinggoGameSettings>(json);
                    
                    if (loadedSettings != null)
                    {
                        // 将加载的值复制到单例对象
                        _settings.Odds = loadedSettings.Odds;
                        _settings.MinBet = loadedSettings.MinBet;
                        _settings.MaxBet = loadedSettings.MaxBet;
                        _settings.MaxBetPerIssue = loadedSettings.MaxBetPerIssue;
                        _settings.SealSecondsAhead = loadedSettings.SealSecondsAhead;
                        _settings.IssueDuration = loadedSettings.IssueDuration;
                        _settings.AutoSendOpenNotice = loadedSettings.AutoSendOpenNotice;
                        _settings.AutoSendLotteryResult = loadedSettings.AutoSendLotteryResult;
                        _settings.AutoSendSettlementNotice = loadedSettings.AutoSendSettlementNotice;
                        _settings.ReplySuccess = loadedSettings.ReplySuccess;
                        _settings.ReplyFailed = loadedSettings.ReplyFailed;
                        _settings.ReplyInsufficientBalance = loadedSettings.ReplyInsufficientBalance;
                        _settings.ReplySealed = loadedSettings.ReplySealed;
                        _settings.ReplyOpenNotice = loadedSettings.ReplyOpenNotice;
                        _settings.ReplySuspended = loadedSettings.ReplySuspended;
                        _settings.IsAdminMode = loadedSettings.IsAdminMode;
                        
                        _logService.Info("BinggoGameSettings", $"✅ 游戏配置已加载: MinBet={_settings.MinBet}, MaxBet={_settings.MaxBet}, SealSecondsAhead={_settings.SealSecondsAhead}");
                    }
                }
                else
                {
                    _logService.Info("BinggoGameSettings", "配置文件不存在，使用默认配置");
                    SaveSettings(); // 创建初始配置文件
                }
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoGameSettings", $"加载游戏配置失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 保存游戏配置
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                var json = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(_configFilePath, json);
                
                _logService.Info("BinggoGameSettings", $"✅ 游戏配置已保存: MinBet={_settings.MinBet}, MaxBet={_settings.MaxBet}, SealSecondsAhead={_settings.SealSecondsAhead}");
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoGameSettings", $"保存游戏配置失败: {ex.Message}", ex);
            }
        }
    }
}

