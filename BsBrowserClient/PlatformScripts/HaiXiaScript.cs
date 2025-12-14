using BaiShengVx3Plus.Shared.Helpers;
using BaiShengVx3Plus.Shared.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 海峡666 平台脚本 - 参考 F5BotV2/HX666.cs
    /// </summary>
    public class HaiXiaScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ModernHttpHelper _httpHelper;
        private bool _isLoggedIn = false;
        private string _sid = "";
        private string _uuid = "";
        private decimal _currentBalance = 0;
        private string _baseUrl = "";  // 缓存的base URL
        private string _p_type = "";  // 盘口类型（A/B/C/D）
        
        // 赔率映射表（参考 F5BotV2 HX666Odds.cs）
        private readonly Dictionary<string, OddsInfo> _oddsMap = new Dictionary<string, OddsInfo>();
        
        public HaiXiaScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            _httpHelper = new ModernHttpHelper(_httpClient);  // 🔥 初始化 ModernHttpHelper
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表（参考 F5BotV2 HX666Odds.cs）
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[海峡] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（参考 F5BotV2 HX666Odds.cs）
        /// </summary>
        private void InitializeOddsMap()
        {
            // P1
            _oddsMap["P1大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", "B1DX_D", 1.97f);
            _oddsMap["P1小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.小, "平码一", "B1DX_X", 1.97f);
            _oddsMap["P1单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.单, "平码一", "B1DS_D", 1.97f);
            _oddsMap["P1双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.双, "平码一", "B1DS_S", 1.97f);
            _oddsMap["P1尾大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾大, "平码一", "B1WDX_D", 1.97f);
            _oddsMap["P1尾小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾小, "平码一", "B1WDX_X", 1.97f);
            _oddsMap["P1合单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合单, "平码一", "B1HDS_D", 1.97f);
            _oddsMap["P1合双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合双, "平码一", "B1HDS_S", 1.97f);
            
            // P2
            _oddsMap["P2大"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.大, "平码二", "B2DX_D", 1.97f);
            _oddsMap["P2小"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.小, "平码二", "B2DX_X", 1.97f);
            _oddsMap["P2单"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.单, "平码二", "B2DS_D", 1.97f);
            _oddsMap["P2双"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.双, "平码二", "B2DS_S", 1.97f);
            _oddsMap["P2尾大"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.尾大, "平码二", "B2WDX_D", 1.97f);
            _oddsMap["P2尾小"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.尾小, "平码二", "B2WDX_X", 1.97f);
            _oddsMap["P2合单"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.合单, "平码二", "B2HDS_D", 1.97f);
            _oddsMap["P2合双"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.合双, "平码二", "B2HDS_S", 1.97f);
            
            // P3
            _oddsMap["P3大"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.大, "平码三", "B3DX_D", 1.97f);
            _oddsMap["P3小"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.小, "平码三", "B3DX_X", 1.97f);
            _oddsMap["P3单"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.单, "平码三", "B3DS_D", 1.97f);
            _oddsMap["P3双"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.双, "平码三", "B3DS_S", 1.97f);
            _oddsMap["P3尾大"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.尾大, "平码三", "B3WDX_D", 1.97f);
            _oddsMap["P3尾小"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.尾小, "平码三", "B3WDX_X", 1.97f);
            _oddsMap["P3合单"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.合单, "平码三", "B3HDS_D", 1.97f);
            _oddsMap["P3合双"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.合双, "平码三", "B3HDS_S", 1.97f);
            
            // P4
            _oddsMap["P4大"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.大, "平码四", "B4DX_D", 1.97f);
            _oddsMap["P4小"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.小, "平码四", "B4DX_X", 1.97f);
            _oddsMap["P4单"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.单, "平码四", "B4DS_D", 1.97f);
            _oddsMap["P4双"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.双, "平码四", "B4DS_S", 1.97f);
            _oddsMap["P4尾大"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.尾大, "平码四", "B4WDX_D", 1.97f);
            _oddsMap["P4尾小"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.尾小, "平码四", "B4WDX_X", 1.97f);
            _oddsMap["P4合单"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.合单, "平码四", "B4HDS_D", 1.97f);
            _oddsMap["P4合双"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.合双, "平码四", "B4HDS_S", 1.97f);
            
            // P5
            _oddsMap["P5大"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.大, "平码五", "B5DX_D", 1.97f);
            _oddsMap["P5小"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.小, "平码五", "B5DX_X", 1.97f);
            _oddsMap["P5单"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.单, "平码五", "B5DS_D", 1.97f);
            _oddsMap["P5双"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.双, "平码五", "B5DS_S", 1.97f);
            _oddsMap["P5尾大"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.尾大, "平码五", "B5WDX_D", 1.97f);
            _oddsMap["P5尾小"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.尾小, "平码五", "B5WDX_X", 1.97f);
            _oddsMap["P5合单"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.合单, "平码五", "B5HDS_D", 1.97f);
            _oddsMap["P5合双"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.合双, "平码五", "B5HDS_S", 1.97f);
            
            // P总
            _oddsMap["P总大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.大, "总和", "ZHDX_D", 1.97f);
            _oddsMap["P总小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.小, "总和", "ZHDX_X", 1.97f);
            _oddsMap["P总单"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.单, "总和", "ZHDS_D", 1.97f);
            _oddsMap["P总双"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.双, "总和", "ZHDS_S", 1.97f);
            _oddsMap["P总尾大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾大, "总和", "HWDX_D", 1.97f);
            _oddsMap["P总尾小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾小, "总和", "HWDX_X", 1.97f);
            _oddsMap["P总龙"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.龙, "总和", "LH_L", 1.97f);
            _oddsMap["P总虎"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.虎, "总和", "LH_H", 1.97f);
        }
        
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Log("🔐 开始登录海峡...");
                
                // 参考 F5BotV2 HX666.cs 的登录逻辑
                var script = $@"
                    (function() {{
                        try {{
                            // 查找登录表单
                            const usernameInput = document.querySelector('input[name=""username""]') ||
                                                  document.querySelector('input[type=""text""]');
                            const passwordInput = document.querySelector('input[name=""password""]') ||
                                                  document.querySelector('input[type=""password""]');
                            
                            if (usernameInput && passwordInput) {{
                                usernameInput.value = '{username}';
                                passwordInput.value = '{password}';
                                
                                // 触发事件
                                usernameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                
                                return {{ success: true, message: '表单已填充，请输入验证码并点击登录' }};
                            }} else {{
                                return {{ success: false, message: '未找到登录表单' }};
                            }}
                        }} catch (error) {{
                            return {{ success: false, message: error.message }};
                        }}
                    }})();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                var json = JObject.Parse(result);
                
                var success = json["success"]?.Value<bool>() ?? false;
                var message = json["message"]?.ToString() ?? "";
                
                Log(success ? $"✅ {message}" : $"❌ {message}");
                
                if (success)
                {
                    // 等待登录完成（通过拦截响应获取 sid, uuid）
                    Log("⏳ 等待登录完成（请输入验证码并点击登录）...");
                    var waitCount = 0;
                    while (string.IsNullOrEmpty(_sid) && waitCount < 300)
                    {
                        await Task.Delay(100);
                        waitCount++;
                    }
                    
                    if (!string.IsNullOrEmpty(_sid))
                    {
                        Log($"✅ 登录成功！SID: {_sid.Substring(0, Math.Min(10, _sid.Length))}...");
                        _isLoggedIn = true;
                        return true;
                    }
                    else
                    {
                        Log("❌ 登录超时或失败");
                        return false;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ 登录失败: {ex.Message}");
                return false;
            }
        }
        
        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                if (!_isLoggedIn || string.IsNullOrEmpty(_baseUrl))
                {
                    Log("❌ 未登录或未获取到base URL，无法获取余额");
                    return -1;
                }
                
                Log("💰 获取余额...");
                
                // 参考 F5BotV2 HX666.cs GetUserInfoUpdata 方法
                string url = $"{_baseUrl}/PlaceBet/QueryResult?lotteryType=TWBINGO";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                // 解析响应（参考 F5BotV2 Line 417-419）
                var json = JObject.Parse(responseText);
                var accountLimit = json["accountLimit"]?.Value<decimal>() ?? 0;
                var unResult = json["UnResult"]?.Value<decimal>() ?? 0;
                
                _currentBalance = accountLimit;
                Log($"💰 余额: {accountLimit}, 未结算: {unResult}");
                
                return accountLimit;
            }
            catch (Exception ex)
            {
                Log($"❌ 获取余额失败: {ex.Message}");
                return -1;
            }
        }
        
        /// <summary>
        /// 获取Cookie字符串
        /// </summary>
        private async Task<string> GetCookiesAsync()
        {
            try
            {
                if (_webView?.CoreWebView2 == null)
                    return "";
                
                var cookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(_webView.CoreWebView2.Source);
                var cookieList = new List<string>();
                foreach (var cookie in cookies)
                {
                    cookieList.Add($"{cookie.Name}={cookie.Value}");
                }
                return string.Join("; ", cookieList);
            }
            catch
            {
                return "";
            }
        }
        
        public async Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BetStandardOrderList orders)
        {
            try
            {
                if (!_isLoggedIn || string.IsNullOrEmpty(_baseUrl))
                {
                    Log("❌ 未登录或未获取到base URL，无法投注");
                    return (false, "", "未登录");
                }
                
                if (orders == null || orders.Count == 0)
                {
                    Log("❌ 订单列表为空");
                    return (false, "", "订单列表为空");
                }
                
                Log($"🎲 开始投注，订单数: {orders.Count}");
                
                // 🔥 参考 F5BotV2 HX666.cs Bet 方法（Line 441-557）
                
                // 1. 获取赔率（如果未获取）
                if (string.IsNullOrEmpty(_p_type))
                {
                    await GetOddsAsync();
                }
                
                // 2. 合并订单（相同车号和玩法的订单合并）
                var betItems = new BetStandardOrderList();
                foreach (var order in orders.OrderBy(o => o.Car).ThenBy(o => o.Play))
                {
                    var last = betItems.LastOrDefault();
                    if (last == null)
                    {
                        betItems.Add(order);
                    }
                    else
                    {
                        if (last.Car == order.Car && last.Play == order.Play)
                        {
                            last.MoneySum += order.MoneySum;
                        }
                        else
                        {
                            betItems.Add(order);
                        }
                    }
                }
                
                Log($"📦 合并后订单数: {betItems.Count}, 总金额: {betItems.GetTotalAmount()}");
                
                // 3. 构建POST数据（参考 F5BotV2 Line 477-509）
                var sbPost = new StringBuilder();
                try
                {
                    for (int i = 0; i < betItems.Count; i++)
                    {
                        if (sbPost.Length > 0)
                            sbPost.Append("&");
                        
                        var order = betItems[i];
                        var key = $"{order.Car}{order.Play}";
                        
                        if (!_oddsMap.ContainsKey(key))
                        {
                            Log($"❌ 未找到赔率映射: {key}");
                            return (false, "", $"未找到赔率映射: {key}");
                        }
                        
                        var oddsInfo = _oddsMap[key];
                        sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][Amount]") + $"={order.MoneySum}&");
                        sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][KeyCode]") + $"={oddsInfo.OddsId}&");
                        sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][Odds]") + $"={oddsInfo.Odds}");
                        
                        Log($"   {key}: 金额={order.MoneySum}, ID={oddsInfo.OddsId}, 赔率={oddsInfo.Odds}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"❌ 构建投注数据失败: {ex.Message}");
                    return (false, "", $"构建投注数据失败: {ex.Message}");
                }
                
                // 4. 添加其他参数（参考 F5BotV2 Line 503-509）
                if (sbPost.Length > 0)
                {
                    sbPost.Append($"&lotteryType=TWBINGO");
                    sbPost.Append($"&betNum=10{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
                    sbPost.Append($"&prompt=true");
                    sbPost.Append($"&gt={_p_type}");
                }
                
                var postData = sbPost.ToString();
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 5. 发送POST请求（参考 F5BotV2 Line 514-526）
                string url = $"{_baseUrl}/PlaceBet/Confirmbet?lotteryType=TWBINGO";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 6. 解析响应（参考 F5BotV2 Line 533-556）
                var json = JObject.Parse(responseText);
                var succeed = json["succeed"]?.Value<int>() ?? 0;
                
                if (succeed == 1)
                {
                    var bettingNumber = json["BettingNumber"]?.ToString() ?? "";
                    Log($"✅ 投注成功: {bettingNumber}");
                    return (true, bettingNumber, responseText);
                }
                else
                {
                    var msg = json["msg"]?.ToString() ?? "未知错误";
                    Log($"❌ 投注失败: {msg}");
                    return (false, "", responseText);
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 投注异常: {ex.Message}");
                Log($"   堆栈: {ex.StackTrace}");
                return (false, "", $"投注异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取赔率（参考 F5BotV2 HX666.cs GetOdds 和 GetIndex 方法）
        /// </summary>
        private async Task<bool> GetOddsAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    Log("❌ 未获取到base URL，无法获取赔率");
                    return false;
                }
                
                // 1. 获取盘口类型（参考 F5BotV2 Line 289-307）
                string indexUrl = $"{_baseUrl}/Home/Index";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Get, indexUrl);
                request.Headers.Add("Cookie", cookies);
                
                var response = await _httpClient.SendAsync(request);
                var html = await response.Content.ReadAsStringAsync();
                
                // 解析盘口类型：<li class="p_curr">A</li>
                var match = Regex.Match(html, @"p_curr[^>]*>([A-D])<");
                if (match.Success)
                {
                    _p_type = match.Groups[1].Value;
                    Log($"📊 盘口类型: {_p_type}");
                }
                else
                {
                    _p_type = "A";  // 默认A盘
                    Log($"⚠️ 未找到盘口类型，使用默认值: {_p_type}");
                }
                
                // 2. 获取赔率数据（参考 F5BotV2 HX666Odds.cs GetUpdata 方法）
                int issueid = GetNextIssueId(DateTime.Now);
                string oddsUrl = $"{_baseUrl}/PlaceBet/Loaddata?lotteryType=TWBINGO";
                string postData = $"itype=-1&settingCode=LM%2CWH%2CFLSX%2CLH&oddstype={_p_type}&lotteryType=TWBINGO&install={issueid}";
                
                var oddsRequest = new HttpRequestMessage(HttpMethod.Post, oddsUrl);
                oddsRequest.Headers.Add("Cookie", cookies);
                oddsRequest.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var oddsResponse = await _httpClient.SendAsync(oddsRequest);
                var oddsText = await oddsResponse.Content.ReadAsStringAsync();
                
                var oddsJson = JObject.Parse(oddsText);
                var state = oddsJson["State"]?.Value<int>() ?? 0;
                
                if (state == 1)
                {
                    var data = oddsJson["data"] as JObject;
                    if (data != null)
                    {
                        foreach (var prop in data.Properties())
                        {
                            var keyCode = prop.Name;  // 如 "B1DX_D"
                            var oddsValue = prop.Value.Value<float>();
                            
                            // 更新赔率映射表中的赔率值
                            var oddsInfo = _oddsMap.Values.FirstOrDefault(o => o.OddsId == keyCode);
                            if (oddsInfo != null)
                            {
                                oddsInfo.Odds = oddsValue;
                                Log($"📊 赔率更新: {keyCode} = {oddsValue}");
                            }
                        }
                        Log($"✅ 赔率获取成功，共更新 {data.Properties().Count()} 项");
                        return true;
                    }
                }
                
                Log("❌ 赔率获取失败");
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ 获取赔率异常: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 获取下一期期号（参考 F5BotV2 BinGouHelper.getNextIssueId）
        /// </summary>
        private int GetNextIssueId(DateTime now)
        {
            // 简化实现：返回当前时间戳（实际应该根据开奖规则计算）
            return int.Parse(now.ToString("yyyyMMddHHmm"));
        }
        
        public void HandleResponse(ResponseEventArgs response)
        {
            try
            {
                // 拦截登录响应，提取 sid, uuid（参考 F5BotV2）
                if (response.Url.Contains("user/login") || response.Url.Contains("login"))
                {
                    var json = JObject.Parse(response.Context);
                    if (json["Msg"] != null)
                    {
                        var msg = JObject.Parse(json["Msg"].ToString());
                        if (msg["Error_code"]?.Value<int>() == 0)
                        {
                            _sid = msg["Sid"]?.ToString() ?? "";
                            _uuid = msg["Uuid"]?.ToString() ?? "";
                            
                            // 设置 baseUrl
                            if (string.IsNullOrEmpty(_baseUrl) && !string.IsNullOrEmpty(response.Url))
                            {
                                try
                                {
                                    _baseUrl = new Uri(response.Url).GetLeftPart(UriPartial.Authority);
                                    Log($"✅ Base URL 已设置: {_baseUrl}");
                                }
                                catch { }
                            }
                            
                            Log($"✅ 拦截到登录参数: SID={_sid.Substring(0, Math.Min(10, _sid.Length))}...");
                            _isLoggedIn = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 处理响应失败: {ex.Message}");
            }
        }
        
        public List<OddsInfo> GetOddsList()
        {
            // 返回赔率映射表中的所有赔率信息
            return _oddsMap.Values.ToList();
        }
    }
}

