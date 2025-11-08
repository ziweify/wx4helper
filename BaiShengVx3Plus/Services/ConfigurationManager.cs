using System;
using System.IO;
using System.Text.Json;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services
{
    /// <summary>
    /// 配置管理器（单例模式）
    /// 负责应用程序级别的设置管理
    /// </summary>
    public class ConfigurationManager
    {
        private static ConfigurationManager? _instance;
        private static readonly object _lock = new object();
        private readonly string _configFilePath;
        private AppConfiguration _configuration;
        
        /// <summary>
        /// 获取配置管理器单例
        /// </summary>
        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigurationManager();
                        }
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 获取当前配置
        /// </summary>
        public AppConfiguration Configuration => _configuration;
        
        /// <summary>
        /// 私有构造函数，防止外部实例化
        /// </summary>
        private ConfigurationManager()
        {
            // 配置文件路径：与程序在同一目录
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            
            // 加载配置
            _configuration = LoadConfiguration();
            
            // 订阅配置变更事件，自动保存
            _configuration.PropertyChanged += (s, e) => SaveConfiguration();
        }
        
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
                        Console.WriteLine($"✅ 配置已从文件加载: {_configFilePath}");
                        return config;
                    }
                }
                
                Console.WriteLine($"⚠️ 配置文件不存在，使用默认配置: {_configFilePath}");
                return new AppConfiguration();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 加载配置失败: {ex.Message}，使用默认配置");
                return new AppConfiguration();
            }
        }
        
        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true, // 格式化输出
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 支持中文
                };
                
                var json = JsonSerializer.Serialize(_configuration, options);
                File.WriteAllText(_configFilePath, json);
                
                Console.WriteLine($"✅ 配置已保存到文件: {_configFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 保存配置失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 重新加载配置（从文件）
        /// </summary>
        public void ReloadConfiguration()
        {
            _configuration = LoadConfiguration();
        }
    }
}

