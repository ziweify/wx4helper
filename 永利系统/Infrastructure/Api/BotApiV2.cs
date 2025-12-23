using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using 永利系统.Models.BotApi.V2;  // 使用 BotApi V2 版本的模型
using 永利系统.Services;
using 永利系统.Infrastructure.Helpers;

namespace 永利系统.Infrastructure.Api
{
    /// <summary>
    /// BotApi V2 版本客户端
    /// 
    /// 注意：V2 版本的 API 端点、请求格式、响应格式可能与 V1 不同
    /// 请根据实际的 V2 API 文档修改以下内容：
    /// 1. API 端点 URL
    /// 2. 请求参数格式
    /// 3. 响应处理逻辑
    /// </summary>
    public class BotApiV2
    {
        // 账号状态码（V2 可能使用不同的状态码）
        // TODO: 根据 V2 API 文档修改状态码
        public static int VERIFY_SIGN_OFFTIME = 10000;  // 账户过期
        public static int VERIFY_SIGN_INVALID = 10001;  // 无效令牌
        public static int VERIFY_SIGN_SUCCESS = 0;      // 成功
        
        private static BotApiV2? _instance;
        private static readonly object _lock = new object();
        
        // TODO: 根据 V2 API 的实际地址修改
        private readonly string _urlRoot = "http://8.134.71.102:789";  // V2 可能使用不同的地址
        private readonly ModernHttpHelper _httpHelper;
        
        public ApiResponse<ApiUser>? LoginApiResponse { get; private set; }
        public string User { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public DateTime OffTime { get; set; }
        
        // 账号失效事件（供外部订阅）
        public event Action<string>? OnAccountInvalid;
        public event Action<string>? OnAccountOffTime;
        
        private BotApiV2()
        {
            _httpHelper = new ModernHttpHelper();
        }
        
        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static BotApiV2 GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new BotApiV2();
                }
            }
            return _instance;
        }
        
        /// <summary>
        /// 登录
        /// TODO: 根据 V2 API 的实际接口修改登录逻辑
        /// </summary>
        public async Task<ApiResponse<ApiUser>> LoginAsync(string user, string pwd)
        {
            User = user;
            Password = pwd;
            
            // TODO: 根据 V2 API 的实际端点修改 URL
            string funcUrl = $"{_urlRoot}/api/boter/v2/login?user={user}&pwd={pwd}";
            
            try
            {
                var result = await _httpHelper.GetAsync(new HttpRequestItem
                {
                    Url = funcUrl,
                    Timeout = 10
                });
                
                if (!result.Success)
                {
                    return new ApiResponse<ApiUser>
                    {
                        Code = -1,
                        Msg = $"HTTP 错误: {result.StatusCode} - {result.ErrorMessage ?? result.Html}"
                    };
                }
                
                LoginApiResponse = JsonConvert.DeserializeObject<ApiResponse<ApiUser>>(result.Html);
                
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

