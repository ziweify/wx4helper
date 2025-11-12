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
        
        public int GetSealSecondsAhead() => _configuration.N封盘提前秒数;
        
        public bool GetIsRunModeAdmin() => _configuration.IsRunModeAdmin;
        
        public bool GetIsRunModeDev() => _configuration.IsRunModeDev;
        
        public bool GetIsRunModeBoss() => _configuration.IsRunModeBoss;
        
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
        
        public void SetSealSecondsAhead(int value)
        {
            if (_configuration.N封盘提前秒数 != value)
            {
                var oldValue = _configuration.N封盘提前秒数;
                _configuration.N封盘提前秒数 = value;
                
                _logService.Info("ConfigurationService", $"封盘秒数已更新: {oldValue} → {value}");
                
                // 自动保存
                SaveConfiguration();
                
                // 触发变更事件（使用 ViewModel 的属性名，不是 Model 的属性名）
                OnConfigurationChanged("SealSecondsAhead", oldValue, value);
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

