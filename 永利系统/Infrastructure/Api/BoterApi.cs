using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using 永利系统.Models.Api;
using 永利系统.Services;

namespace 永利系统.Infrastructure.Api
{
    /// <summary>
    /// Boter API 客户端（完全参考 BaiShengVx3Plus 的 BoterApi）
    /// 
    /// 设计原则：
    /// 1. 单例模式（Singleton）
    /// 2. 登录后保存 Token，后续请求自动使用
    /// 3. 简单直接，不过度设计
    /// </summary>
    public class BoterApi
    {
        // 账号状态码（参考 F5BotV2）
        public static int VERIFY_SIGN_OFFTIME = 10000;  // 账户过期
        public static int VERIFY_SIGN_INVALID = 10001;  // 无效令牌（账号被其他地方登录）
        public static int VERIFY_SIGN_SUCCESS = 0;      // 成功
        
        private static BoterApi? _instance;
        private static readonly object _lock = new object();
        
        private readonly string _urlRoot = "http://8.134.71.102:789";
        private readonly HttpClient _httpClient;
        
        public ApiResponse<ApiUser>? LoginApiResponse { get; private set; }
        public string User { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public DateTime OffTime { get; set; }
        
        // 账号失效事件（供外部订阅）
        public event Action<string>? OnAccountInvalid;
        public event Action<string>? OnAccountOffTime;
        
        private BoterApi()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        }
        
        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static BoterApi GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new BoterApi();
                }
            }
            return _instance;
        }
        
        /// <summary>
        /// 登录
        /// </summary>
        public async Task<ApiResponse<ApiUser>> LoginAsync(string user, string pwd)
        {
            User = user;
            Password = pwd;
            
            string funcUrl = $"{_urlRoot}/api/boter/login?user={user}&pwd={pwd}";
            
            try
            {
                var response = await _httpClient.GetAsync(funcUrl);
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                
                LoginApiResponse = JsonConvert.DeserializeObject<ApiResponse<ApiUser>>(json);
                
                if (LoginApiResponse != null && LoginApiResponse.Code == 0)
                {
                    if (LoginApiResponse.Data != null)
                    {
                        LoginApiResponse.Data.Username = user;
                        LoginApiResponse.Data.Password = pwd;
                    }
                }
                else
                {
                    // 检查账号失效
                    if (LoginApiResponse != null)
                    {
                        if (LoginApiResponse.Code == VERIFY_SIGN_OFFTIME)
                        {
                            OnAccountOffTime?.Invoke("账号过期");
                        }
                        else if (LoginApiResponse.Code == VERIFY_SIGN_INVALID)
                        {
                            OnAccountInvalid?.Invoke("账号失效! 请重新登录\r\n请检查是否有在其他地方登录导致本次失效!");
                        }
                    }
                }
                
                return LoginApiResponse ?? new ApiResponse<ApiUser>
                {
                    Code = -1,
                    Msg = "登录响应为空"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ApiUser>
                {
                    Code = -1,
                    Msg = $"登录异常: {ex.Message}"
                };
            }
        }
        
        /// <summary>
        /// 检查是否已登录
        /// </summary>
        public bool IsLoggedIn()
        {
            return LoginApiResponse != null && 
                   LoginApiResponse.Code == 0 && 
                   LoginApiResponse.Data != null &&
                   LoginApiResponse.Data.IsTokenValid &&
                   LoginApiResponse.Data.IsAccountValid;
        }
        
        /// <summary>
        /// 获取当前 Token
        /// </summary>
        public string? GetToken()
        {
            return LoginApiResponse?.Data?.Token;
        }
        
        /// <summary>
        /// 登出
        /// </summary>
        public void Logout()
        {
            LoginApiResponse = null;
            User = string.Empty;
            Password = string.Empty;
        }
    }
}

