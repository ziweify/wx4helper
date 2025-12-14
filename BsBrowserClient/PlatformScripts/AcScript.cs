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
    /// AC 平台脚本 - 参考 F5BotV2/AcMember.cs（与海峡类似）
    /// </summary>
    public class AcScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ModernHttpHelper _httpHelper;
        private bool _isLoggedIn = false;
        private string _sid = "";
        private string _uuid = "";
        private decimal _currentBalance = 0;
        private string _baseUrl = "";
        private string _pk = "";  // 盘口类型（A/B/C/D）
        
        // 赔率映射表（参考 F5BotV2 HX666Odds.cs，AC使用海峡的赔率映射）
        private readonly Dictionary<string, OddsInfo> _oddsMap = new Dictionary<string, OddsInfo>();
        
        public AcScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            _httpHelper = new ModernHttpHelper(_httpClient);  // 🔥 初始化 ModernHttpHelper
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表（参考 F5BotV2，AC使用海峡的赔率映射）
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[AC] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（参考 F5BotV2，AC使用海峡的赔率映射）
        /// </summary>
        private void InitializeOddsMap()
        {
            // 使用海峡的赔率映射（参考 F5BotV2 AcMember.cs Line 36）
            // P1
            _oddsMap["P1大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", "B1DX_D", 1.97f);
            _oddsMap["P1小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.小, "平码一", "B1DX_X", 1.97f);
            _oddsMap["P1单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.单, "平码一", "B1DS_D", 1.97f);
            _oddsMap["P1双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.双, "平码一", "B1DS_S", 1.97f);
            _oddsMap["P1尾大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾大, "平码一", "B1WDX_D", 1.97f);
            _oddsMap["P1尾小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾小, "平码一", "B1WDX_X", 1.97f);
            _oddsMap["P1合单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合单, "平码一", "B1HDS_D", 1.97f);
            _oddsMap["P1合双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合双, "平码一", "B1HDS_S", 1.97f);
            
            // P总
            _oddsMap["P总大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.大, "和值", "B0DX_D", 1.97f);
            _oddsMap["P总小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.小, "和值", "B0DX_X", 1.97f);
            _oddsMap["P总单"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.单, "和值", "B0DS_D", 1.97f);
            _oddsMap["P总双"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.双, "和值", "B0DS_S", 1.97f);
            _oddsMap["P总尾大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾大, "和值", "B0WDX_D", 1.97f);
            _oddsMap["P总尾小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾小, "和值", "B0WDX_X", 1.97f);
            _oddsMap["P总龙"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.龙, "和值", "B0LH_L", 1.97f);
            _oddsMap["P总虎"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.虎, "和值", "B0LH_H", 1.97f);
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
        
        /// <summary>
        /// 获取盘口类型（参考 F5BotV2 AcMember.cs GetIndex 方法）
        /// </summary>
        private async Task<string> GetIndexAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_baseUrl))
                    return "A";
                
                var cookies = await GetCookiesAsync();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/Home/Index");
                request.Headers.Add("Cookie", cookies);
                
                var response = await _httpClient.SendAsync(request);
                var html = await response.Content.ReadAsStringAsync();
                
                // 解析HTML获取盘口类型（参考 F5BotV2 Line 253-256）
                var match = Regex.Match(html, @"p_curr.>(.*?)<");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 获取盘口类型失败: {ex.Message}");
            }
            
            return "A";
        }
        
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Log("🔐 开始登录AC...");
                
                // 参考 F5BotV2 AcMember.cs 的登录逻辑（与海峡类似）
                var script = $@"
                    (function() {{
                        try {{
                            // 查找登录表单（参考 F5BotV2 Line 144-148）
                            const inputs = document.querySelectorAll('input');
                            if (inputs.length >= 4) {{
                                inputs[0].value = '{username}';
                                inputs[1].value = '{password}';
                                
                                // 触发事件
                                inputs[0].dispatchEvent(new Event('input', {{ bubbles: true }}));
                                inputs[1].dispatchEvent(new Event('input', {{ bubbles: true }}));
                                
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
                    
                    if (!string.IsNullOrEmpty(_sid) && !string.IsNullOrEmpty(_uuid))
                    {
                        Log($"✅ 登录成功！UUID: {_uuid}, SID: {_sid.Substring(0, Math.Min(10, _sid.Length))}...");
                        _isLoggedIn = true;
                        
                        // 获取盘口类型
                        _pk = await GetIndexAsync();
                        Log($"✅ 盘口类型: {_pk}");
                        
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
                return _currentBalance;
            }
            catch (Exception ex)
            {
                Log($"❌ 获取余额失败: {ex.Message}");
                return -1;
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
                
                if (string.IsNullOrEmpty(_pk))
                {
                    _pk = await GetIndexAsync();
                }
                
                Log($"🎲 开始投注，订单数: {orders.Count}");
                
                // 🔥 参考 F5BotV2 AcMember.cs Bet 方法（与海峡类似）
                
                // 1. 合并订单
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
                
                // 2. 构建POST数据（参考 F5BotV2 Line 296-329）
                var sbPost = new StringBuilder();
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
                    
                    var odds = _oddsMap[key];
                    sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][Amount]") + $"={order.MoneySum}&");
                    sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][KeyCode]") + $"={odds.OddsId}&");
                    sbPost.Append(WebUtility.UrlEncode($"betdata[{i}][Odds]") + $"={odds.Odds}");
                }
                
                if (sbPost.Length > 0)
                {
                    sbPost.Append($"&lotteryType=TWBINGO");
                    sbPost.Append($"&betNum=10{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
                    sbPost.Append($"&prompt=true");
                    sbPost.Append($"&gt={_pk}");
                }
                
                var postData = sbPost.ToString();
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 3. 发送POST请求
                string url = $"{_baseUrl}/PlaceBet/Confirmbet?lotteryType=TWBINGO";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 4. 解析响应（参考 F5BotV2 Line 352-365）
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
                return (false, "", $"投注异常: {ex.Message}");
            }
        }
        
        public void HandleResponse(ResponseEventArgs response)
        {
            try
            {
                // 拦截登录响应，提取 sid, uuid（参考 F5BotV2 AcMember.cs，与海峡类似）
                if (response.Url.Contains("/user/login") || response.Url.Contains("login"))
                {
                    if (!string.IsNullOrEmpty(response.Context))
                    {
                        try
                        {
                            var json = JObject.Parse(response.Context);
                            var msg = json["Msg"]?.ToString();
                            if (!string.IsNullOrEmpty(msg))
                            {
                                var jMsg = JObject.Parse(msg);
                                var errorCode = jMsg["Error_code"]?.Value<int>() ?? -1;
                                if (errorCode == 0)
                                {
                                    _sid = jMsg["Sid"]?.ToString() ?? "";
                                    _uuid = jMsg["Uuid"]?.ToString() ?? "";
                                    
                                    if (!string.IsNullOrEmpty(_sid) && !string.IsNullOrEmpty(_uuid))
                                    {
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
                                        
                                        Log($"✅ 拦截到登录参数: UUID={_uuid}, SID={_sid.Substring(0, Math.Min(10, _sid.Length))}...");
                                        _isLoggedIn = true;
                                    }
                                }
                            }
                        }
                        catch { }
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
        
        public Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
            int state = 0, int pageNum = 1, int pageCount = 20, string? beginDate = null, string? endDate = null, int timeout = 10)
        {
            Log("⚠️ Ac 平台暂不支持获取订单列表");
            return Task.FromResult<(bool, List<JObject>?, int, int, string)>((false, null, 0, 0, "平台暂不支持"));
        }
    }
}

