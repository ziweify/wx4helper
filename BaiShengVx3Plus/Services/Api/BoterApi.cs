using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BaiShengVx3Plus.Models.Api;
using BaiShengVx3Plus.Models.Games.Binggo;
using Unit.Shared.Helpers;
using System.Net.Http;

namespace BaiShengVx3Plus.Services.Api
{
    /// <summary>
    /// 白胜 API 客户端（完全参考 F5BotV2 的 BoterApi）
    /// 
    /// 🔥 设计原则：
    /// 1. 单例模式（Singleton）
    /// 2. 登录后保存 c_sign，后续请求自动使用
    /// 3. 简单直接，不过度设计
    /// </summary>
    public class BoterApi
    {
        // 🔥 参考 F5BotV2 BoterApi.cs Line 20-22
        public static int VERIFY_SIGN_OFFTIME = 10000;  // 账户过期
        public static int VERIFY_SIGN_INVALID = 10001;  // 无效令牌（账号被其他地方登录）
        public static int VERIFY_SIGN_SUCCESS = 0;      // 成功
        
        private static BoterApi? _instance;
        private static readonly object _lock = new object();
        
        private readonly string _urlRoot = "http://8.134.71.102:789";
        private readonly HttpClient _httpClient;
        private readonly ModernHttpHelper _httpHelper;
        
        public BsApiResponse<BsApiUser>? LoginApiResponse { get; private set; }
        public string User { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;  // 🔥 保存密码（用于数据库备份加密）
        public DateTime OffTime { get; set; }
        
        // 🔥 账号失效事件（供外部订阅）
        public event Action<string>? OnAccountInvalid;
        public event Action<string>? OnAccountOffTime;
        
        private BoterApi()
        {
            _httpClient = new HttpClient();
            _httpHelper = new ModernHttpHelper(_httpClient);  // 🔥 复用 HttpClient 连接池
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
        /// 🔥 完全参考 F5BotV2
        /// </summary>
        public async Task<BsApiResponse<BsApiUser>> LoginAsync(string user, string pwd)
        {
            User = user;
            Password = pwd;  // 🔥 保存密码
            
            string funcUrl = $"{_urlRoot}/api/boter/login?user={user}&pwd={pwd}";
            
            try
            {
                Console.WriteLine($"📡 登录请求: {funcUrl}");
                
                // 🎯 使用 ModernHttpHelper
                var result = await _httpHelper.GetAsync(new HttpRequestItem
                {
                    Url = funcUrl,
                    Timeout = 10
                });
                
                if (!result.Success)
                {
                    Console.WriteLine($"❌ 登录请求失败: {result.ErrorMessage}");
                    return new BsApiResponse<BsApiUser>
                    {
                        Code = -1,
                        Msg = result.ErrorMessage ?? "请求失败"
                    };
                }
                
                var json = result.Html;
                Console.WriteLine($"📡 登录响应: {json}");
                
                LoginApiResponse = JsonConvert.DeserializeObject<BsApiResponse<BsApiUser>>(json);
                
                if (LoginApiResponse != null && LoginApiResponse.Code == 0)
                {
                    Console.WriteLine($"✅ 登录成功: {user}");
                    Console.WriteLine($"   c_sign: {LoginApiResponse.Data?.Token}");
                    Console.WriteLine($"   c_soft_name: {LoginApiResponse.Data?.SoftName}");
                    Console.WriteLine($"   c_off_time: {LoginApiResponse.Data?.ValidUntil}");
                    
                    // 🔥 验证 c_sign 是否正确解析
                    if (string.IsNullOrEmpty(LoginApiResponse.Data?.Token))
                    {
                        Console.WriteLine("⚠️ 警告: c_sign 为空！");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ 登录失败: Code={LoginApiResponse?.Code}, Msg={LoginApiResponse?.Msg}");
                    
                    // 🔥 检查账号失效（参考 F5BotV2 BoterServices.cs Line 1081-1088）
                    if (LoginApiResponse != null)
                    {
                        if (LoginApiResponse.Code == VERIFY_SIGN_OFFTIME)
                        {
                            // 账号过期
                            OnAccountOffTime?.Invoke("账号过期");
                        }
                        else if (LoginApiResponse.Code == VERIFY_SIGN_INVALID)
                        {
                            // 账号失效（被其他地方登录）
                            OnAccountInvalid?.Invoke("账号失效! 请重新登录\r\n请检查是否有在其他地方登录导致本次失效!");
                        }
                    }
                }
                
                return LoginApiResponse ?? new BsApiResponse<BsApiUser>
                {
                    Code = -1,
                    Msg = "登录响应为空"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 登录异常: {ex.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
                return new BsApiResponse<BsApiUser>
                {
                    Code = -1,
                    Msg = $"登录异常: {ex.Message}"
                };
            }
        }
        
        /// <summary>
        /// 获取炳狗日数据
        /// 🔥 完全参考 F5BotV2 的 getbgday
        /// </summary>
        public async Task<BsApiResponse<List<BinggoLotteryData>>> GetBgDayAsync(string date, int limit, bool fill)
        {
            var response = new BsApiResponse<List<BinggoLotteryData>>();
            
            // 🔥 检查是否已登录
            if (LoginApiResponse == null || LoginApiResponse.Data == null)
            {
                Console.WriteLine("❌ 未登录，无法获取数据");
                return new BsApiResponse<List<BinggoLotteryData>>
                {
                    Code = -1,
                    Msg = "请先登录"
                };
            }
            
            // 🔥 构建参数（完全参考 F5BotV2）
            string param = "";
            if (!string.IsNullOrEmpty(date))
                param += $"date={date}";
            if (!string.IsNullOrEmpty(param))
                param += "&";
            param += $"limit={limit}";
            if (!string.IsNullOrEmpty(param))
                param += "&";
            param += $"sign={LoginApiResponse.Data.Token}";  // 🔥 使用登录时保存的 c_sign
            if (fill)
                param += $"&fill=1";
                
            string funcUrl = $"{_urlRoot}/api/boter/getbgday?{param}";
            
            Console.WriteLine($"📡 API 请求: {funcUrl}");
            
            try
            {
                // 🎯 使用 ModernHttpHelper
                var result = await _httpHelper.GetAsync(new HttpRequestItem
                {
                    Url = funcUrl,
                    Timeout = 15
                });
                
                if (!result.Success)
                {
                    Console.WriteLine($"❌ API 请求失败: {result.ErrorMessage}");
                    return new BsApiResponse<List<BinggoLotteryData>>
                    {
                        Code = -1,
                        Msg = result.ErrorMessage ?? "请求失败"
                    };
                }
                
                var json = result.Html;
                Console.WriteLine($"📡 API 响应: {json.Substring(0, Math.Min(200, json.Length))}...");
                
                // 🔥 解析数据（参考 F5BotV2）
                var hret = JsonConvert.DeserializeObject<BsApiResponse<List<object>>>(json);
                if (hret != null)
                {
                    response.Code = hret.Code;
                    response.Msg = hret.Msg;
                    
                    // 🔥 检查账号失效（参考 F5BotV2 BoterServices.cs Line 1081-1088）
                    if (hret.Code == VERIFY_SIGN_OFFTIME)
                    {
                        // 账号过期
                        OnAccountOffTime?.Invoke("账号过期");
                    }
                    else if (hret.Code == VERIFY_SIGN_INVALID)
                    {
                        // 账号失效（被其他地方登录）
                        OnAccountInvalid?.Invoke("账号失效! 请重新登录\r\n请检查是否有在其他地方登录导致本次失效!");
                    }
                    
                    if (hret.Data != null)
                    {
                        response.Data = new List<BinggoLotteryData>();
                        foreach (var obj in hret.Data)
                        {
                            try
                            {
                                JObject d = JObject.Parse(obj.ToString()!);
                                
                                string p1 = d["p1"]?.ToString() ?? "-1";
                                string p2 = d["p2"]?.ToString() ?? "-1";
                                string p3 = d["p3"]?.ToString() ?? "-1";
                                string p4 = d["p4"]?.ToString() ?? "-1";
                                string p5 = d["p5"]?.ToString() ?? "-1";
                                string lotteryTime = d["lottery_time"]?.ToString() ?? "";
                                int issueId = d["issueid"]?.ToObject<int>() ?? 0;
                                
                                // 🔥 使用 FillLotteryData（完全参考 F5BotV2）
                                var bgData = new BinggoLotteryData().FillLotteryData(
                                    issueId, 
                                    $"{p1},{p2},{p3},{p4},{p5}", 
                                    lotteryTime
                                );
                                
                                response.Data.Add(bgData);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ 解析单条数据失败: {ex.Message}");
                            }
                        }
                        
                        Console.WriteLine($"✅ 成功获取 {response.Data.Count} 期数据");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API 请求失败: {ex.Message}");
                response.Code = -1;
                response.Msg = $"请求异常: {ex.Message}";
            }
            
            return response;
        }
        
        /// <summary>
        /// 获取指定期号的炳狗数据
        /// 🔥 完全参考 F5BotV2 的 getbgData
        /// </summary>
        public async Task<BsApiResponse<BinggoLotteryData>> GetBgDataAsync(int issueId)
        {
            // 🔥 检查是否已登录
            if (LoginApiResponse == null || LoginApiResponse.Data == null)
            {
                return new BsApiResponse<BinggoLotteryData>
                {
                    Code = -1,
                    Msg = "请先登录"
                };
            }
            
            string funcUrl = $"{_urlRoot}/api/boter/getbgData?issueid={issueId}&sign={LoginApiResponse.Data.Token}";
            
            try
            {
                // 🎯 使用 ModernHttpHelper
                var result = await _httpHelper.GetAsync(new HttpRequestItem
                {
                    Url = funcUrl,
                    Timeout = 10
                });
                
                if (!result.Success)
                {
                    Console.WriteLine($"❌ GetBgData({issueId}) 请求失败: {result.ErrorMessage}");
                    return new BsApiResponse<BinggoLotteryData>
                    {
                        Code = -1,
                        Msg = result.ErrorMessage ?? "请求失败"
                    };
                }
                
                var json = result.Html;
                Console.WriteLine($"📡 GetBgData({issueId}) API 响应: {json}");  // 🔥 添加日志
                
                var apiResponse = JsonConvert.DeserializeObject<BsApiResponse<object>>(json);
                
                // 🔥 修复：参考 F5BotV2，先检查 code
                if (apiResponse == null || apiResponse.Code != 0)
                {
                    Console.WriteLine($"⚠️ API 返回失败: Code={apiResponse?.Code}, Msg={apiResponse?.Msg}");
                    return new BsApiResponse<BinggoLotteryData>
                    {
                        Code = apiResponse?.Code ?? -1,
                        Msg = apiResponse?.Msg ?? "API调用失败"
                    };
                }
                
                // 🔥 修复：检查 data 是否为空
                if (apiResponse.Data == null)
                {
                    Console.WriteLine($"⚠️ 期号 {issueId} 数据为空（data=null）");
                    return new BsApiResponse<BinggoLotteryData>
                    {
                        Code = -1,
                        Msg = "数据为空"
                    };
                }
                
                JObject d = JObject.Parse(apiResponse.Data.ToString()!);
                
                // 🔥 修复：参考 F5BotV2，优先检查 lotteryData 字段
                string lotteryDataStr = d["lotteryData"]?.ToString() ?? "";
                
                if (!string.IsNullOrEmpty(lotteryDataStr))
                {
                    // 🔥 方式1：使用 lotteryData 字段（与 F5BotV2 完全一致）
                    string lotteryTime = d["lottery_time"]?.ToString() ?? "";
                    
                    Console.WriteLine($"📊 解析 lotteryData: {lotteryDataStr}, time={lotteryTime}");
                    
                    var bgData = new BinggoLotteryData().FillLotteryData(
                        issueId, 
                        lotteryDataStr, 
                        lotteryTime
                    );
                    
                    if (bgData.IsOpened)
                    {
                        Console.WriteLine($"✅ 开奖数据解析成功: {issueId} - {bgData.ToLotteryString()}");
                        return new BsApiResponse<BinggoLotteryData>
                        {
                            Code = 0,
                            Msg = "成功",
                            Data = bgData
                        };
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ 期号 {issueId} 未开奖（IsOpened=false）");
                        return new BsApiResponse<BinggoLotteryData>
                        {
                            Code = -1,
                            Msg = "未开奖"
                        };
                    }
                }
                else
                {
                    // 🔥 方式2：兜底，尝试解析 p1-p5 字段
                    string p1 = d["p1"]?.ToString() ?? "";
                    string p2 = d["p2"]?.ToString() ?? "";
                    string p3 = d["p3"]?.ToString() ?? "";
                    string p4 = d["p4"]?.ToString() ?? "";
                    string p5 = d["p5"]?.ToString() ?? "";
                    string lotteryTime = d["lottery_time"]?.ToString() ?? "";
                    
                    Console.WriteLine($"📊 解析 p1-p5: {p1},{p2},{p3},{p4},{p5}, time={lotteryTime}");
                    
                    // 🔥 检查是否所有号码都为空或无效（表示未开奖）
                    if (string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2) || 
                        string.IsNullOrEmpty(p3) || string.IsNullOrEmpty(p4) || 
                        string.IsNullOrEmpty(p5))
                    {
                        Console.WriteLine($"⚠️ 期号 {issueId} 未开奖（号码为空）");
                        return new BsApiResponse<BinggoLotteryData>
                        {
                            Code = -1,
                            Msg = "未开奖"
                        };
                    }
                    
                    var bgData = new BinggoLotteryData().FillLotteryData(
                        issueId, 
                        $"{p1},{p2},{p3},{p4},{p5}", 
                        lotteryTime
                    );
                    
                    if (bgData.IsOpened)
                    {
                        Console.WriteLine($"✅ 开奖数据解析成功: {issueId} - {bgData.ToLotteryString()}");
                        return new BsApiResponse<BinggoLotteryData>
                        {
                            Code = 0,
                            Msg = "成功",
                            Data = bgData
                        };
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ 期号 {issueId} 未开奖（FillLotteryData 后 IsOpened=false）");
                        return new BsApiResponse<BinggoLotteryData>
                        {
                            Code = -1,
                            Msg = "未开奖"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new BsApiResponse<BinggoLotteryData>
                {
                    Code = -1,
                    Msg = $"请求异常: {ex.Message}"
                };
            }
        }
    }
}

