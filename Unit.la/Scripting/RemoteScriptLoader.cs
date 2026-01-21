using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Unit.La.Models;

namespace Unit.La.Scripting
{
    /// <summary>
    /// 远程脚本加载器 - 从URL加载脚本
    /// </summary>
    public class RemoteScriptLoader
    {
        private readonly HttpClient _httpClient;
        private readonly ScriptSourceConfig _config;

        public RemoteScriptLoader(ScriptSourceConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = new HttpClient();

            // 设置认证Token
            if (!string.IsNullOrEmpty(_config.RemoteAuthToken))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.RemoteAuthToken}");
            }

            // 设置超时
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// 从远程URL加载脚本列表
        /// </summary>
        /// <returns>脚本字典 (脚本名 => 脚本内容)</returns>
        public async Task<Dictionary<string, string>> LoadScriptsAsync()
        {
            if (string.IsNullOrEmpty(_config.RemoteUrl))
            {
                throw new InvalidOperationException("远程URL未设置");
            }

            try
            {
                // 请求远程脚本数据
                var response = await _httpClient.GetAsync(_config.RemoteUrl);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();

                // 解析JSON: { "脚本a": "内容", "脚本b": "内容" }
                var scripts = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                if (scripts == null || scripts.Count == 0)
                {
                    throw new InvalidOperationException("远程脚本数据为空");
                }

                _config.LastRefreshTime = DateTime.Now;

                return scripts;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"加载远程脚本失败: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"解析远程脚本JSON失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 测试远程连接
        /// </summary>
        public async Task<(bool Success, string Message)> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_config.RemoteUrl, HttpCompletionOption.ResponseHeadersRead);
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, "连接成功");
                }
                else
                {
                    return (false, $"连接失败: HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"连接失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 转换为ScriptInfo列表
        /// </summary>
        public List<ScriptInfo> ConvertToScriptInfos(Dictionary<string, string> scripts)
        {
            var result = new List<ScriptInfo>();

            foreach (var kvp in scripts)
            {
                var scriptInfo = new ScriptInfo
                {
                    Name = kvp.Key,
                    DisplayName = kvp.Key,
                    Content = kvp.Value,
                    Type = InferScriptType(kvp.Key),
                    Metadata = new Dictionary<string, string>
                    {
                        ["source"] = "remote",
                        ["url"] = _config.RemoteUrl,
                        ["loaded_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                };

                result.Add(scriptInfo);
            }

            return result;
        }

        /// <summary>
        /// 根据文件名推断脚本类型
        /// </summary>
        private ScriptType InferScriptType(string fileName)
        {
            var lowerName = fileName.ToLower();

            if (lowerName.Contains("main"))
                return ScriptType.Main;
            else if (lowerName.Contains("function") || lowerName.Contains("lib"))
                return ScriptType.Functions;
            else if (lowerName.Contains("test"))
                return ScriptType.Test;
            else
                return ScriptType.Custom;
        }
    }
}
