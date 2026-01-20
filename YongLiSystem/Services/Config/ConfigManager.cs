using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using 永利系统.Models.Config;
using 永利系统.Services;
using 永利系统.Infrastructure.Paths;

namespace 永利系统.Services.Config
{
    /// <summary>
    /// 配置管理器（单例模式）
    /// 
    /// 功能：
    /// 1. 加载/保存配置到 JSON 文件
    /// 2. 延迟保存机制（3秒无修改后自动保存）
    /// 3. 密码加密/解密（DPAPI）
    /// </summary>
    public class ConfigManager
    {
        private static ConfigManager? _instance;
        private static readonly object _lock = new object();
        
        private readonly LoggingService _loggingService;
        private readonly System.Threading.Timer _autoSaveTimer;
        private bool _isDirty = false;
        
        public AppConfig Config { get; private set; }
        
        private ConfigManager()
        {
            _loggingService = LoggingService.Instance;
            
            // 初始化配置
            Config = new AppConfig();
            
            // 创建自动保存定时器（3秒延迟）
            _autoSaveTimer = new System.Threading.Timer(
                callback: _ => AutoSave(),
                state: null,
                dueTime: Timeout.Infinite,
                period: Timeout.Infinite
            );
        }
        
        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new ConfigManager();
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 加载配置
        /// </summary>
        public void Load()
        {
            try
            {
                if (File.Exists(AppPaths.ConfigFile))
                {
                    var json = File.ReadAllText(AppPaths.ConfigFile, Encoding.UTF8);
                    var config = JsonConvert.DeserializeObject<AppConfig>(json);
                    
                    if (config != null)
                    {
                        Config = config;
                        _loggingService.Info("配置管理", $"配置加载成功: {AppPaths.ConfigFile}");
                    }
                }
                else
                {
                    _loggingService.Info("配置管理", "配置文件不存在，使用默认配置");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error("配置管理", $"加载配置失败: {ex.Message}");
                Config = new AppConfig();
            }
        }
        
        /// <summary>
        /// 保存配置
        /// </summary>
        public void Save()
        {
            try
            {
                _loggingService.Debug("配置管理", $"Save() 开始，目标路径: {AppPaths.ConfigFile}");
                
                // 序列化配置
                var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                _loggingService.Debug("配置管理", $"配置已序列化，JSON长度: {json.Length}");
                _loggingService.Debug("配置管理", $"JSON内容预览:\n{json.Substring(0, Math.Min(200, json.Length))}...");
                
                // 确保目录存在
                var directory = Path.GetDirectoryName(AppPaths.ConfigFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    _loggingService.Debug("配置管理", $"创建配置目录: {directory}");
                    Directory.CreateDirectory(directory);
                }
                
                // 写入文件（使用 FileStream 并强制刷新）
                _loggingService.Debug("配置管理", $"准备写入文件: {AppPaths.ConfigFile}");
                using (var fileStream = new FileStream(AppPaths.ConfigFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    fileStream.Flush(true); // 强制刷新到磁盘
                }
                _loggingService.Debug("配置管理", "File.WriteAllText 调用完成");
                
                // 验证文件是否存在
                if (File.Exists(AppPaths.ConfigFile))
                {
                    var fileInfo = new FileInfo(AppPaths.ConfigFile);
                    _loggingService.Info("配置管理", $"✅ 配置保存成功: {AppPaths.ConfigFile}，文件大小: {fileInfo.Length} 字节");
                }
                else
                {
                    _loggingService.Error("配置管理", "❌ 文件写入后不存在！");
                }
                
                _isDirty = false;
            }
            catch (Exception ex)
            {
                _loggingService.Error("配置管理", $"❌ 保存配置失败: {ex.Message}\n堆栈:\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 标记配置已修改（启动延迟保存）
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
            
            // 重置定时器：3秒后自动保存
            _autoSaveTimer.Change(dueTime: 3000, period: Timeout.Infinite);
        }
        
        /// <summary>
        /// 立即保存（如果有修改）
        /// </summary>
        public void SaveNow()
        {
            _loggingService.Debug("配置管理", $"SaveNow() 被调用，_isDirty = {_isDirty}");
            
            // 停止定时器
            _autoSaveTimer.Change(dueTime: Timeout.Infinite, period: Timeout.Infinite);
            
            // 强制保存（即使 _isDirty 为 false）
            _loggingService.Debug("配置管理", "强制调用 Save()...");
            Save();
        }
        
        /// <summary>
        /// 自动保存（定时器回调）
        /// </summary>
        private void AutoSave()
        {
            if (_isDirty)
            {
                Save();
            }
        }
        
        /// <summary>
        /// 重置为默认配置
        /// </summary>
        public void Reset()
        {
            Config = new AppConfig();
            Save();
            _loggingService.Info("配置管理", "配置已重置为默认值");
        }
        
        #region 密码加密/解密（DPAPI）
        
        /// <summary>
        /// 加密密码（使用 DPAPI）
        /// </summary>
        public static string EncryptPassword(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;
            
            try
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = ProtectedData.Protect(
                    plainBytes,
                    null, // optionalEntropy
                    DataProtectionScope.CurrentUser
                );
                
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// 解密密码（使用 DPAPI）
        /// </summary>
        public static string DecryptPassword(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;
            
            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var plainBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    null, // optionalEntropy
                    DataProtectionScope.CurrentUser
                );
                
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
        
        #endregion
    }
}

