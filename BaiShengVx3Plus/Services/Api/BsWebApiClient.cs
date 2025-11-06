using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BaiShengVx3Plus.Services.Api
{
    /// <summary>
    /// 白胜系统 WebAPI HTTP 客户端实现
    /// 使用 Newtonsoft.Json 进行 JSON 序列化/反序列化
    /// </summary>
    public class BsWebApiClient : IBsWebApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BsWebApiClient> _logger;
        private string _baseUrl = "http://8.134.71.102:789"; // Default from F5BotV2
        private string _sign = string.Empty;

        public BsWebApiClient(HttpClient httpClient, ILogger<BsWebApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
            _logger.LogInformation($"WebAPI Base URL set to: {_baseUrl}");
        }

        public void SetSign(string sign)
        {
            _sign = sign;
            _logger.LogInformation("WebAPI Sign updated.");
        }

        public async Task<BsApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string>? parameters = null)
        {
            var uriBuilder = new UriBuilder($"{_baseUrl}/api/boter/{endpoint}");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    query[param.Key] = param.Value;
                }
            }

            if (!string.IsNullOrEmpty(_sign))
            {
                query["sign"] = _sign;
            }

            uriBuilder.Query = query.ToString();
            string url = uriBuilder.ToString();

            _logger.LogDebug($"Sending GET request to: {url}");

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Received GET response: {content}");
                
                // 使用 Newtonsoft.Json 反序列化
                var result = JsonConvert.DeserializeObject<BsApiResponse<T>>(content);
                return result ?? new BsApiResponse<T> { Code = -1, Msg = "Deserialization failed" };
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, $"HTTP request failed for GET {url}: {httpEx.Message}");
                return new BsApiResponse<T> { Code = -1, Msg = $"网络请求失败: {httpEx.Message}" };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, $"JSON deserialization failed for GET {url}: {jsonEx.Message}");
                return new BsApiResponse<T> { Code = -1, Msg = $"数据解析失败: {jsonEx.Message}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred for GET {url}: {ex.Message}");
                return new BsApiResponse<T> { Code = -1, Msg = $"未知错误: {ex.Message}" };
            }
        }

        public async Task<BsApiResponse<T>> PostAsync<T>(string endpoint, object? data = null)
        {
            var url = $"{_baseUrl}/api/boter/{endpoint}";
            _logger.LogDebug($"Sending POST request to: {url}");

            try
            {
                StringContent? content = null;
                if (data != null)
                {
                    // 使用 Newtonsoft.Json 序列化
                    var json = JsonConvert.SerializeObject(data);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                    _logger.LogDebug($"POST request body: {json}");
                }

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Received POST response: {responseContent}");
                
                // 使用 Newtonsoft.Json 反序列化
                var result = JsonConvert.DeserializeObject<BsApiResponse<T>>(responseContent);
                return result ?? new BsApiResponse<T> { Code = -1, Msg = "Deserialization failed" };
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, $"HTTP request failed for POST {url}: {httpEx.Message}");
                return new BsApiResponse<T> { Code = -1, Msg = $"网络请求失败: {httpEx.Message}" };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, $"JSON deserialization failed for POST {url}: {jsonEx.Message}");
                return new BsApiResponse<T> { Code = -1, Msg = $"数据解析失败: {jsonEx.Message}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred for POST {url}: {ex.Message}");
                return new BsApiResponse<T> { Code = -1, Msg = $"未知错误: {ex.Message}" };
            }
        }
        
        // ========================================
        // 🎲 炳狗游戏专用 API 方法
        // ========================================
        
        public async Task<BsApiResponse<T>> GetCurrentBinggoDataAsync<T>()
        {
            return await GetAsync<T>("bingguo/current");
        }
        
        public async Task<BsApiResponse<T>> GetBinggoDataAsync<T>(int issueId)
        {
            return await GetAsync<T>("bingguo/issue", new Dictionary<string, string>
            {
                ["issue_id"] = issueId.ToString()
            });
        }
        
        public async Task<BsApiResponse<T>> GetRecentBinggoDataAsync<T>(int count = 10)
        {
            return await GetAsync<T>("bingguo/recent", new Dictionary<string, string>
            {
                ["count"] = count.ToString()
            });
        }
        
        public async Task<BsApiResponse<T>> GetBinggoDataListAsync<T>(System.DateTime date)
        {
            return await GetAsync<T>("bingguo/list", new Dictionary<string, string>
            {
                ["date"] = date.ToString("yyyy-MM-dd")
            });
        }
    }
}
