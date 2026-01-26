using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Unit.La.Models;

namespace Unit.La.Services
{
    /// <summary>
    /// 配置服务 - 管理配置的保存和加载
    /// 支持本地文件保存和 HTTP 远程读取
    /// </summary>
    public class ConfigService
    {
        private readonly string _configDirectory;
        private readonly HttpClient? _httpClient;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configDirectory">配置目录路径，如果为 null 则使用默认路径（%LocalAppData%\永利系统）</param>
        /// <param name="httpClient">HTTP 客户端（用于远程读取），如果为 null 则不支持远程读取</param>
        public ConfigService(string? configDirectory = null, HttpClient? httpClient = null)
        {
            // 🔥 默认配置目录：%LocalAppData%\永利系统（与 AppPaths.ConfigDirectory 保持一致）
            if (string.IsNullOrEmpty(configDirectory))
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                _configDirectory = Path.Combine(localAppData, "永利系统");
            }
            else
            {
                _configDirectory = configDirectory;
            }

            // 确保配置目录存在
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }

            _httpClient = httpClient;
        }

        /// <summary>
        /// 获取配置文件的完整路径
        /// </summary>
        /// <param name="configName">配置名称（不含扩展名）</param>
        /// <returns>配置文件路径</returns>
        public string GetConfigFilePath(string configName)
        {
            // 清理文件名，移除非法字符
            var safeName = string.Join("_", configName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_configDirectory, $"{safeName}.json");
        }

        /// <summary>
        /// 保存配置到本地文件
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="configName">配置名称（如果不提供，则使用 config.Name）</param>
        public void SaveConfig(ScriptTaskConfig config, string? configName = null)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var name = configName ?? config.Name;
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("配置名称不能为空", nameof(configName));

            var filePath = GetConfigFilePath(name);
            config.SaveToFile(filePath);
        }

        /// <summary>
        /// 从本地文件加载配置
        /// </summary>
        /// <param name="configName">配置名称（不含扩展名）</param>
        /// <returns>配置对象，如果文件不存在则返回 null</returns>
        public ScriptTaskConfig? LoadConfig(string configName)
        {
            if (string.IsNullOrEmpty(configName))
                throw new ArgumentException("配置名称不能为空", nameof(configName));

            var filePath = GetConfigFilePath(configName);
            return ScriptTaskConfig.LoadFromFile(filePath);
        }

        /// <summary>
        /// 从 HTTP 远程 URL 加载配置
        /// </summary>
        /// <param name="url">配置文件的 HTTP URL</param>
        /// <returns>配置对象</returns>
        public async Task<ScriptTaskConfig?> LoadConfigFromRemoteAsync(string url)
        {
            if (_httpClient == null)
                throw new InvalidOperationException("未提供 HttpClient，无法从远程加载配置");

            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL 不能为空", nameof(url));

            try
            {
                var json = await _httpClient.GetStringAsync(url);
                return ScriptTaskConfig.FromJson(json);
            }
            catch (Exception ex)
            {
                throw new Exception($"从远程加载配置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 保存配置到 HTTP 远程 URL（POST）
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="url">目标 URL</param>
        /// <returns>是否成功</returns>
        public async Task<bool> SaveConfigToRemoteAsync(ScriptTaskConfig config, string url)
        {
            if (_httpClient == null)
                throw new InvalidOperationException("未提供 HttpClient，无法保存到远程");

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL 不能为空", nameof(url));

            try
            {
                var json = config.ToJson();
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"保存配置到远程失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查配置文件是否存在
        /// </summary>
        /// <param name="configName">配置名称</param>
        /// <returns>是否存在</returns>
        public bool ConfigExists(string configName)
        {
            if (string.IsNullOrEmpty(configName))
                return false;

            var filePath = GetConfigFilePath(configName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// 删除配置文件
        /// </summary>
        /// <param name="configName">配置名称</param>
        /// <returns>是否成功删除</returns>
        public bool DeleteConfig(string configName)
        {
            if (string.IsNullOrEmpty(configName))
                return false;

            var filePath = GetConfigFilePath(configName);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取所有配置文件的名称列表
        /// </summary>
        /// <returns>配置名称列表</returns>
        public string[] GetAllConfigNames()
        {
            if (!Directory.Exists(_configDirectory))
                return Array.Empty<string>();

            var files = Directory.GetFiles(_configDirectory, "*.json");
            var names = new System.Collections.Generic.List<string>();

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                names.Add(name);
            }

            return names.ToArray();
        }
    }
}
