using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BaiShengVx3Plus.Models.Api;
using BaiShengVx3Plus.Models.Games.Binggo;
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
        private static BoterApi? _instance;
        private static readonly object _lock = new object();
        
        private readonly string _urlRoot = "http://8.134.71.102:789";
        private readonly HttpClient _httpClient;
        
        public BsApiResponse<BsApiUser>? LoginApiResponse { get; private set; }
        public string User { get; private set; } = string.Empty;
        public DateTime OffTime { get; set; }
        
        private BoterApi()
        {
            _httpClient = new HttpClient();
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
            
            string funcUrl = $"{_urlRoot}/api/boter/login?user={user}&pwd={pwd}";
            
            try
            {
                var response = await _httpClient.GetAsync(funcUrl);
                var json = await response.Content.ReadAsStringAsync();
                
                LoginApiResponse = JsonConvert.DeserializeObject<BsApiResponse<BsApiUser>>(json);
                
                if (LoginApiResponse != null && LoginApiResponse.Code == 0)
                {
                    Console.WriteLine($"✅ 登录成功: {user}");
                    Console.WriteLine($"   c_sign: {LoginApiResponse.Data?.Token}");
                    Console.WriteLine($"   有效期: {LoginApiResponse.Data?.ValidUntil}");
                }
                
                return LoginApiResponse ?? new BsApiResponse<BsApiUser>
                {
                    Code = -1,
                    Msg = "登录响应为空"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 登录失败: {ex.Message}");
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
                var httpResponse = await _httpClient.GetAsync(funcUrl);
                var json = await httpResponse.Content.ReadAsStringAsync();
                
                Console.WriteLine($"📡 API 响应: {json.Substring(0, Math.Min(200, json.Length))}...");
                
                // 🔥 解析数据（参考 F5BotV2）
                var hret = JsonConvert.DeserializeObject<BsApiResponse<List<object>>>(json);
                if (hret != null)
                {
                    response.Code = hret.Code;
                    response.Msg = hret.Msg;
                    
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
                var httpResponse = await _httpClient.GetAsync(funcUrl);
                var json = await httpResponse.Content.ReadAsStringAsync();
                
                var apiResponse = JsonConvert.DeserializeObject<BsApiResponse<object>>(json);
                if (apiResponse != null && apiResponse.Code == 0 && apiResponse.Data != null)
                {
                    JObject d = JObject.Parse(apiResponse.Data.ToString()!);
                    
                    string p1 = d["p1"]?.ToString() ?? "-1";
                    string p2 = d["p2"]?.ToString() ?? "-1";
                    string p3 = d["p3"]?.ToString() ?? "-1";
                    string p4 = d["p4"]?.ToString() ?? "-1";
                    string p5 = d["p5"]?.ToString() ?? "-1";
                    string lotteryTime = d["lottery_time"]?.ToString() ?? "";
                    
                    var bgData = new BinggoLotteryData().FillLotteryData(
                        issueId, 
                        $"{p1},{p2},{p3},{p4},{p5}", 
                        lotteryTime
                    );
                    
                    return new BsApiResponse<BinggoLotteryData>
                    {
                        Code = 0,
                        Msg = "成功",
                        Data = bgData
                    };
                }
                
                return new BsApiResponse<BinggoLotteryData>
                {
                    Code = apiResponse?.Code ?? -1,
                    Msg = apiResponse?.Msg ?? "获取失败"
                };
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

