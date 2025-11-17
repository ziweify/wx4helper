using System;
using System.IO;
using System.Text.Json;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Configuration
{
    /// <summary>
    /// 配置服务实现
    /// 职责：管理应用程序级别的配置（读取、保存、变更通知）
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configFilePath;
        private readonly ILogService _logService;
        private AppConfiguration _configuration;
        
        // ========================================
        // 构造函数（支持依赖注入）
        // ========================================
        
        public ConfigurationService(ILogService logService)
        {
            _logService = logService;
            
            // 配置文件路径
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            
            // 加载配置
            _configuration = LoadConfiguration();
            
            _logService.Info("ConfigurationService", "✅ 配置服务已初始化");
        }
        
        // ========================================
        // 事件
        // ========================================
        
        public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
        
        // ========================================
        // 读取配置（公共接口）
        // ========================================
        
        public bool GetIsOrdersTaskingEnabled() => _configuration.Is收单开关;
        
        public bool GetIsAutoBetEnabled() => _configuration.Is飞单开关;
        
        /// <summary>
        /// 🔥 获取提前封盘秒数（统一使用 SealSecondsAhead）
        /// </summary>
        public int GetSealSecondsAhead() => _configuration.SealSecondsAhead;
        
        public bool Get收单关闭时不发送系统消息() => _configuration.收单关闭时不发送系统消息;
        
        // ========================================
        // 🔥 游戏规则配置访问（从 BinggoGameSettings 迁移过来）
        // ========================================
        
        public float GetMinBet() => _configuration.MinBet;
        public float GetMaxBet() => _configuration.MaxBet;
        public float GetMaxBetPerIssue() => _configuration.MaxBetPerIssue;
        public Dictionary<string, float> GetOdds() => _configuration.Odds;
        
        public bool GetIsRunModeAdmin() => _configuration.IsRunModeAdmin;
        
        public bool GetIsRunModeDev() => _configuration.IsRunModeDev;
        
        public bool GetIsRunModeBoss() => _configuration.IsRunModeBoss;
        
        /// <summary>
        /// 🔧 获取开发模式：当前会员
        /// </summary>
        public string GetRunDevCurrentMember() => _configuration.RunDevCurrentMember;
        
        /// <summary>
        /// 🔧 获取开发模式：发送消息内容
        /// </summary>
        public string GetRunDevSendMessage() => _configuration.RunDevSendMessage;
        
        // ========================================
        // 保存配置（公共接口）
        // ========================================
        
        public void SetIsOrdersTaskingEnabled(bool value)
        {
            if (_configuration.Is收单开关 != value)
            {
                var oldValue = _configuration.Is收单开关;
                _configuration.Is收单开关 = value;
                
                _logService.Info("ConfigurationService", $"收单开关已更新: {oldValue} → {value}");
                
                // 自动保存
                SaveConfiguration();
                
                // 触发变更事件（使用 ViewModel 的属性名，不是 Model 的属性名）
                OnConfigurationChanged("IsOrdersTaskingEnabled", oldValue, value);
            }
        }
        
        public void SetIsAutoBetEnabled(bool value)
        {
            if (_configuration.Is飞单开关 != value)
            {
                var oldValue = _configuration.Is飞单开关;
                _configuration.Is飞单开关 = value;
                
                _logService.Info("ConfigurationService", $"自动投注开关已更新: {oldValue} → {value}");
                
                // 自动保存
                SaveConfiguration();
                
                // 触发变更事件（使用 ViewModel 的属性名，不是 Model 的属性名）
                OnConfigurationChanged("IsAutoBetEnabled", oldValue, value);
            }
        }
        
        /// <summary>
        /// 🔥 设置提前封盘秒数（统一使用 SealSecondsAhead）
        /// </summary>
        public void SetSealSecondsAhead(int value)
        {
            if (_configuration.SealSecondsAhead != value)
            {
                var oldValue = _configuration.SealSecondsAhead;
                _configuration.SealSecondsAhead = value;
                
                _logService.Info("ConfigurationService", $"封盘秒数已更新: {oldValue} → {value}");
                
                // 自动保存
                SaveConfiguration();
                
                // 触发变更事件
                OnConfigurationChanged("SealSecondsAhead", oldValue, value);
            }
        }
        
        // ========================================
        // 🔥 游戏规则配置设置（从 BinggoGameSettings 迁移过来）
        // ========================================
        
        public void SetMinBet(float value)
        {
            if (_configuration.MinBet != value)
            {
                var oldValue = _configuration.MinBet;
                _configuration.MinBet = value;
                
                _logService.Info("ConfigurationService", $"最小投注已更新: {oldValue} → {value}");
                SaveConfiguration();
                OnConfigurationChanged("MinBet", oldValue, value);
            }
        }
        
        public void SetMaxBet(float value)
        {
            if (_configuration.MaxBet != value)
            {
                var oldValue = _configuration.MaxBet;
                _configuration.MaxBet = value;
                
                _logService.Info("ConfigurationService", $"最大投注已更新: {oldValue} → {value}");
                SaveConfiguration();
                OnConfigurationChanged("MaxBet", oldValue, value);
            }
        }
        
        public void SetMaxBetPerIssue(float value)
        {
            if (_configuration.MaxBetPerIssue != value)
            {
                var oldValue = _configuration.MaxBetPerIssue;
                _configuration.MaxBetPerIssue = value;
                
                _logService.Info("ConfigurationService", $"单期最大投注已更新: {oldValue} → {value}");
                SaveConfiguration();
                OnConfigurationChanged("MaxBetPerIssue", oldValue, value);
            }
        }
        
        public void Set收单关闭时不发送系统消息(bool value)
        {
            if (_configuration.收单关闭时不发送系统消息 != value)
            {
                var oldValue = _configuration.收单关闭时不发送系统消息;
                _configuration.收单关闭时不发送系统消息 = value;
                
                _logService.Info("ConfigurationService", $"收单关闭时不发送系统消息已更新: {oldValue} → {value}");
                
                // 自动保存
                SaveConfiguration();
                
                // 触发变更事件
                OnConfigurationChanged("收单关闭时不发送系统消息", oldValue, value);
            }
        }
        
        public void SetIsRunModeAdmin(bool value)
        {
            if (_configuration.IsRunModeAdmin != value)
            {
                var oldValue = _configuration.IsRunModeAdmin;
                _configuration.IsRunModeAdmin = value;
                
                _logService.Info("ConfigurationService", $"管理模式已更新: {oldValue} → {value}");
                
                // 自动保存
                SaveConfiguration();
                
                // 触发变更事件
                OnConfigurationChanged("IsRunModeAdmin", oldValue, value);
            }
        }
        
        public void SetIsRunModeDev(bool value)
        {
            if (_configuration.IsRunModeDev != value)
            {
                var oldValue = _configuration.IsRunModeDev;
                _configuration.IsRunModeDev = value;
                
                _logService.Info("ConfigurationService", $"开发模式已更新: {oldValue} → {value}");
                
                // 自动保存
                SaveConfiguration();
                
                // 触发变更事件
                OnConfigurationChanged("IsRunModeDev", oldValue, value);
            }
        }
        
        public void SetIsRunModeBoss(bool value)
        {
            if (_configuration.IsRunModeBoss != value)
            {
                var oldValue = _configuration.IsRunModeBoss;
                _configuration.IsRunModeBoss = value;
                
                _logService.Info("ConfigurationService", $"老板模式已更新: {oldValue} → {value}");
                
                // 自动保存
                SaveConfiguration();
                
                // 触发变更事件
                OnConfigurationChanged("IsRunModeBoss", oldValue, value);
            }
        }
        
        /// <summary>
        /// 🔧 设置开发模式：当前会员
        /// </summary>
        public void SetRunDevCurrentMember(string value)
        {
            if (_configuration.RunDevCurrentMember != value)
            {
                var oldValue = _configuration.RunDevCurrentMember;
                _configuration.RunDevCurrentMember = value;
                
                _logService.Info("ConfigurationService", $"开发模式-当前会员已更新: {oldValue} → {value}");
                
                // 自动保存
                SaveConfiguration();
                
                // 触发变更事件
                OnConfigurationChanged("RunDevCurrentMember", oldValue, value);
            }
        }
        
        /// <summary>
        /// 🔧 设置开发模式：发送消息内容
        /// </summary>
        public void SetRunDevSendMessage(string value)
        {
            if (_configuration.RunDevSendMessage != value)
            {
                var oldValue = _configuration.RunDevSendMessage;
                _configuration.RunDevSendMessage = value;
                
                _logService.Info("ConfigurationService", $"开发模式-发送消息已更新: {oldValue} → {value}");
                
                // 自动保存
                SaveConfiguration();
                
                // 触发变更事件
                OnConfigurationChanged("RunDevSendMessage", oldValue, value);
            }
        }
        
        // ========================================
        // 登录信息管理（记住密码功能）
        // ========================================
        
        /// <summary>
        /// 获取保存的用户名
        /// </summary>
        public string GetBsUserName() => _configuration.BsUserName;
        
        /// <summary>
        /// 获取保存的密码（解密）
        /// </summary>
        public string GetBsUserPassword()
        {
            if (string.IsNullOrEmpty(_configuration.BsUserPwd))
                return string.Empty;
            
            return Utils.PasswordHelper.Decrypt(_configuration.BsUserPwd);
        }
        
        /// <summary>
        /// 获取是否记住密码
        /// </summary>
        public bool GetIsRememberPassword() => _configuration.IsRememberPassword;
        
        /// <summary>
        /// 保存登录信息（记住密码）
        /// </summary>
        public void SaveLoginInfo(string username, string password, bool rememberPassword)
        {
            _configuration.BsUserName = username;
            _configuration.IsRememberPassword = rememberPassword;
            
            if (rememberPassword && !string.IsNullOrEmpty(password))
            {
                // 加密保存密码
                _configuration.BsUserPwd = Utils.PasswordHelper.Encrypt(password);
                _logService.Info("ConfigurationService", $"登录信息已保存: 用户名={username}, 记住密码=是");
            }
            else
            {
                // 不记住密码，清空
                _configuration.BsUserPwd = string.Empty;
                _logService.Info("ConfigurationService", $"登录信息已保存: 用户名={username}, 记住密码=否");
            }
            
            // 自动保存
            SaveConfiguration();
        }
        
        /// <summary>
        /// 清除保存的登录信息
        /// </summary>
        public void ClearLoginInfo()
        {
            _configuration.BsUserName = string.Empty;
            _configuration.BsUserPwd = string.Empty;
            _configuration.IsRememberPassword = false;
            
            SaveConfiguration();
            
            _logService.Info("ConfigurationService", "登录信息已清除");
        }
        
        // ========================================
        // 配置管理（公共接口）
        // ========================================
        
        public void ReloadConfiguration()
        {
            _logService.Info("ConfigurationService", "重新加载配置...");
            _configuration = LoadConfiguration();
            _logService.Info("ConfigurationService", "✅ 配置已重新加载");
        }
        
        public void SaveConfiguration()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                var json = JsonSerializer.Serialize(_configuration, options);
                File.WriteAllText(_configFilePath, json);
                
                _logService.Debug("ConfigurationService", $"配置已保存: {_configFilePath}");
            }
            catch (Exception ex)
            {
                _logService.Error("ConfigurationService", "保存配置失败", ex);
            }
        }
        
        // ========================================
        // 私有辅助方法
        // ========================================
        
        /// <summary>
        /// 从文件加载配置
        /// </summary>
        private AppConfiguration LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize<AppConfiguration>(json);
                    
                    if (config != null)
                    {
                        _logService.Info("ConfigurationService", $"配置已从文件加载: {_configFilePath}");
                        return config;
                    }
                }
                
                _logService.Warning("ConfigurationService", $"配置文件不存在，使用默认配置: {_configFilePath}");
                return new AppConfiguration();
            }
            catch (Exception ex)
            {
                _logService.Error("ConfigurationService", "加载配置失败，使用默认配置", ex);
                return new AppConfiguration();
            }
        }
        
        /// <summary>
        /// 触发配置变更事件
        /// </summary>
        private void OnConfigurationChanged(string propertyName, object? oldValue, object? newValue)
        {
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
            {
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue
            });
        }
    }
}

