using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Shared.Platform
{
    /// <summary>
    /// 平台URL管理器（统一管理所有平台的默认URL）
    /// 支持从服务器接口获取URL，支持强制更新
    /// </summary>
    public static class PlatformUrlManager
    {
        /// <summary>
        /// 平台URL映射表（唯一数据源）
        /// </summary>
        private static readonly Dictionary<string, string> _platformUrls = new()
        {
            // 中文平台名称
            { "通宝", "https://yb666.fr.win2000.cc" },
            { "云顶", "https://www.yunding28.com" },
            { "海峡", "https://www.haixia28.com" },
            { "红海", "https://www.honghai28.com" },
            
            // 兼容旧平台名称
            { "TongBao", "https://yb666.fr.win2000.cc" },
            { "YunDing", "https://www.yunding28.com" },
            { "YunDing28", "https://www.yunding28.com" },
            { "HaiXia", "https://www.haixia28.com" },
            { "HaiXia28", "https://www.haixia28.com" },
            { "HongHai", "https://www.honghai28.com" },
            { "HongHai28", "https://www.honghai28.com" }
        };

        /// <summary>
        /// 强制更新的URL映射（优先级高于默认URL，用于服务器推送的更新）
        /// </summary>
        private static readonly Dictionary<string, string> _forcedUrls = new();

        /// <summary>
        /// 服务器URL更新事件
        /// </summary>
        public static event EventHandler<PlatformUrlUpdatedEventArgs>? UrlUpdated;

        /// <summary>
        /// 根据平台名称获取默认URL
        /// </summary>
        /// <param name="platformName">平台名称（支持中文名和旧英文名）</param>
        /// <returns>平台URL，如果不存在返回空字符串</returns>
        public static string GetDefaultUrl(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                return "";

            // 1. 优先检查强制更新的URL
            if (_forcedUrls.TryGetValue(platformName, out var forcedUrl))
                return forcedUrl;

            // 2. 检查默认URL映射
            if (_platformUrls.TryGetValue(platformName, out var defaultUrl))
                return defaultUrl;

            // 3. 尝试通过 BetPlatform 枚举解析
            var platform = BetPlatformHelper.Parse(platformName);
            if (_platformUrls.TryGetValue(platform.ToString(), out var enumUrl))
                return enumUrl;

            return "";
        }

        /// <summary>
        /// 根据平台枚举获取默认URL
        /// </summary>
        /// <param name="platform">平台枚举</param>
        /// <returns>平台URL</returns>
        public static string GetDefaultUrl(BetPlatform platform)
        {
            return GetDefaultUrl(platform.ToString());
        }

        /// <summary>
        /// 强制更新平台URL（用于服务器推送的更新）
        /// </summary>
        /// <param name="platformName">平台名称</param>
        /// <param name="url">新的URL</param>
        public static void ForceUpdateUrl(string platformName, string url)
        {
            if (string.IsNullOrWhiteSpace(platformName) || string.IsNullOrWhiteSpace(url))
                return;

            _forcedUrls[platformName] = url;
            
            // 触发更新事件
            UrlUpdated?.Invoke(null, new PlatformUrlUpdatedEventArgs
            {
                PlatformName = platformName,
                NewUrl = url,
                IsForced = true
            });
        }

        /// <summary>
        /// 清除强制更新的URL（恢复为默认URL）
        /// </summary>
        /// <param name="platformName">平台名称</param>
        public static void ClearForcedUrl(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                return;

            if (_forcedUrls.Remove(platformName))
            {
                UrlUpdated?.Invoke(null, new PlatformUrlUpdatedEventArgs
                {
                    PlatformName = platformName,
                    NewUrl = GetDefaultUrl(platformName),
                    IsForced = false
                });
            }
        }

        /// <summary>
        /// 清除所有强制更新的URL
        /// </summary>
        public static void ClearAllForcedUrls()
        {
            var platformNames = _forcedUrls.Keys.ToList();
            _forcedUrls.Clear();

            foreach (var platformName in platformNames)
            {
                UrlUpdated?.Invoke(null, new PlatformUrlUpdatedEventArgs
                {
                    PlatformName = platformName,
                    NewUrl = GetDefaultUrl(platformName),
                    IsForced = false
                });
            }
        }

        /// <summary>
        /// 从服务器接口获取平台URL（扩展方法）
        /// </summary>
        /// <param name="apiUrl">服务器API地址</param>
        /// <param name="timeoutSeconds">超时时间（秒）</param>
        /// <returns>是否成功更新</returns>
        public static async Task<bool> UpdateFromServerAsync(string apiUrl, int timeoutSeconds = 10)
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(timeoutSeconds)
                };

                var response = await httpClient.GetStringAsync(apiUrl);
                var urlData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(response);

                if (urlData == null || urlData.Count == 0)
                    return false;

                // 更新强制URL映射
                foreach (var kvp in urlData)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
                    {
                        ForceUpdateUrl(kvp.Key, kvp.Value);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出异常
                System.Diagnostics.Debug.WriteLine($"从服务器获取平台URL失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取所有平台URL映射（用于调试和显示）
        /// </summary>
        /// <returns>平台URL字典</returns>
        public static Dictionary<string, string> GetAllUrls()
        {
            var result = new Dictionary<string, string>(_platformUrls);
            
            // 合并强制更新的URL（覆盖默认URL）
            foreach (var kvp in _forcedUrls)
            {
                result[kvp.Key] = kvp.Value;
            }

            return result;
        }

        /// <summary>
        /// 检查URL是否为强制更新
        /// </summary>
        /// <param name="platformName">平台名称</param>
        /// <returns>是否为强制更新</returns>
        public static bool IsForcedUrl(string platformName)
        {
            return !string.IsNullOrWhiteSpace(platformName) && _forcedUrls.ContainsKey(platformName);
        }
    }

    /// <summary>
    /// 平台URL更新事件参数
    /// </summary>
    public class PlatformUrlUpdatedEventArgs : EventArgs
    {
        public string PlatformName { get; set; } = "";
        public string NewUrl { get; set; } = "";
        public bool IsForced { get; set; }
    }
}

