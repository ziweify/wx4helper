using System;
using System.IO;

namespace YongLiSystem.Infrastructure.Paths
{
    /// <summary>
    /// 应用程序路径管理器
    /// 
    /// 统一管理所有应用程序路径，确保路径一致性
    /// </summary>
    public static class AppPaths
    {
        /// <summary>
        /// 应用程序根目录（%LocalAppData%\永利系统）
        /// </summary>
        public static string AppRoot { get; }
        
        /// <summary>
        /// 配置文件目录
        /// </summary>
        public static string ConfigDirectory { get; }
        
        /// <summary>
        /// 日志文件目录
        /// </summary>
        public static string LogsDirectory { get; }
        
        /// <summary>
        /// 数据库文件目录
        /// </summary>
        public static string DataDirectory { get; }
        
        /// <summary>
        /// 缓存文件目录
        /// </summary>
        public static string CacheDirectory { get; }
        
        /// <summary>
        /// 配置文件完整路径
        /// </summary>
        public static string ConfigFile { get; }
        
        static AppPaths()
        {
            // 应用根目录：C:\Users\[用户名]\AppData\Local\永利系统\
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            AppRoot = Path.Combine(localAppData, "永利系统");
            
            // 子目录
            ConfigDirectory = AppRoot;
            LogsDirectory = Path.Combine(AppRoot, "Logs");
            DataDirectory = Path.Combine(AppRoot, "Data");
            CacheDirectory = Path.Combine(AppRoot, "Cache");
            
            // 配置文件路径
            ConfigFile = Path.Combine(ConfigDirectory, "config.json");
            
            // 确保所有目录存在
            EnsureDirectoriesExist();
        }
        
        /// <summary>
        /// 确保所有必需的目录存在
        /// </summary>
        private static void EnsureDirectoriesExist()
        {
            try
            {
                Directory.CreateDirectory(AppRoot);
                Directory.CreateDirectory(LogsDirectory);
                Directory.CreateDirectory(DataDirectory);
                Directory.CreateDirectory(CacheDirectory);
                
                // 验证目录是否真的存在
                System.Diagnostics.Debug.WriteLine($"[AppPaths] AppRoot 存在: {Directory.Exists(AppRoot)}");
                System.Diagnostics.Debug.WriteLine($"[AppPaths] AppRoot 路径: {AppRoot}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppPaths] 创建目录失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 获取日志文件路径（按日期）
        /// </summary>
        public static string GetLogFilePath(DateTime date)
        {
            return Path.Combine(LogsDirectory, $"{date:yyyy-MM-dd}.log");
        }
        
        /// <summary>
        /// 获取数据库文件路径
        /// </summary>
        public static string GetDatabasePath(string databaseName)
        {
            return Path.Combine(DataDirectory, databaseName);
        }
        
        /// <summary>
        /// 清理缓存目录
        /// </summary>
        public static void ClearCache()
        {
            try
            {
                if (Directory.Exists(CacheDirectory))
                {
                    Directory.Delete(CacheDirectory, true);
                    Directory.CreateDirectory(CacheDirectory);
                }
            }
            catch
            {
                // 忽略错误
            }
        }
        
        /// <summary>
        /// 清理过期日志文件
        /// </summary>
        public static void ClearOldLogs(int retentionDays)
        {
            try
            {
                if (!Directory.Exists(LogsDirectory))
                    return;
                
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                var logFiles = Directory.GetFiles(LogsDirectory, "*.log");
                
                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        File.Delete(logFile);
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
        }
    }
}

