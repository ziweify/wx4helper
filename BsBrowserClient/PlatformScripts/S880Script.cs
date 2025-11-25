using BaiShengVx3Plus.Shared.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
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
    /// S880 平台脚本 - 参考 F5BotV2/S880Member.cs
    /// </summary>
    public class S880Script : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isLoggedIn = false;
        private string _p_id = "";
        private string _tt_top = "";
        private string _baseUrl = "";  // 缓存的base URL
        private decimal _currentBalance = 0;
        
        // 赔率映射表（参考 F5BotV2，S880使用红海的赔率映射）
        private readonly Dictionary<string, OddsInfo> _oddsMap = new Dictionary<string, OddsInfo>();
        
        public S880Script(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表（参考 F5BotV2，S880使用红海的赔率映射）
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[S880] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（参考 F5BotV2，S880使用红海的赔率映射）
        /// </summary>
        private void InitializeOddsMap()
        {
            // 使用红海的赔率映射（参考 F5BotV2 S880Member.cs Line 31）
            // P1
            _oddsMap["P1大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", "5370", 1.97f);
            _oddsMap["P1小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.小, "平码一", "5371", 1.97f);
            _oddsMap["P1单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.单, "平码一", "5372", 1.97f);
            _oddsMap["P1双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.双, "平码一", "5373", 1.97f);
            _oddsMap["P1尾大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾大, "平码一", "5374", 1.97f);
            _oddsMap["P1尾小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾小, "平码一", "5375", 1.97f);
            _oddsMap["P1合单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合单, "平码一", "5376", 1.97f);
            _oddsMap["P1合双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合双, "平码一", "5377", 1.97f);
            
            // P2-P5 类似...
            // P总
            _oddsMap["P总大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.大, "和值", "5364", 1.97f);
            _oddsMap["P总小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.小, "和值", "5365", 1.97f);
            _oddsMap["P总单"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.单, "和值", "5366", 1.97f);
            _oddsMap["P总双"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.双, "和值", "5367", 1.97f);
            _oddsMap["P总尾大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾大, "和值", "5368", 1.97f);
            _oddsMap["P总尾小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾小, "和值", "5369", 1.97f);
            _oddsMap["P总龙"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.龙, "和值", "5418", 1.97f);
            _oddsMap["P总虎"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.虎, "和值", "5419", 1.97f);
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
        /// 获取投注金额字符串（参考 F5BotV2 S880Member.cs GetBetString 方法）
        /// </summary>
        private string GetBetString(BetStandardOrderList items, CarNumEnum car, BetPlayEnum play)
        {
            var item = items.FirstOrDefault(p => p.Car == car && p.Play == play);
            return item != null ? item.MoneySum.ToString() : "";
        }
        
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Log("🔐 开始登录S880...");
                
                // 参考 F5BotV2 S880Member.cs 的登录逻辑（Line 123-225）
                var script = $@"
                    (function() {{
                        try {{
                            // 查找登录表单（S880登录表单选择器）
                            const usernameInput = document.querySelector('#username') ||
                                                  document.querySelector('input[name=""username""]') ||
                                                  document.querySelector('input[type=""text""]');
                            const passwordInput = document.querySelector('#password') ||
                                                  document.querySelector('input[name=""password""]') ||
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
                    // 等待登录完成
                    Log("⏳ 等待登录完成（请输入验证码并点击登录）...");
                    var waitCount = 0;
                    while (!_isLoggedIn && waitCount < 300)
                    {
                        await Task.Delay(100);
                        waitCount++;
                        
                        // 检查是否已登录（通过URL判断，参考 F5BotV2 Line 189）
                        var currentUrl = await _webView.CoreWebView2.ExecuteScriptAsync("window.location.href");
                        if (currentUrl != null && currentUrl.Contains("user/mail.asp"))
                        {
                            _isLoggedIn = true;
                            
                            // 设置 baseUrl
                            if (string.IsNullOrEmpty(_baseUrl))
                            {
                                try
                                {
                                    var urlStr = currentUrl.Trim('"');
                                    _baseUrl = new Uri(urlStr).GetLeftPart(UriPartial.Authority);
                                    Log($"✅ Base URL 已设置: {_baseUrl}");
                                }
                                catch { }
                            }
                            
                            break;
                        }
                    }
                    
                    if (_isLoggedIn)
                    {
                        Log("✅ 登录成功！");
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
                // 通过页面或API获取余额（需要根据实际平台实现）
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
                
                if (string.IsNullOrEmpty(_p_id) || string.IsNullOrEmpty(_tt_top))
                {
                    Log("❌ 未获取到 p_id 或 tt_top，无法投注");
                    return (false, "", "未获取到投注参数");
                }
                
                Log($"🎲 开始投注，订单数: {orders.Count}");
                
                // 🔥 参考 F5BotV2 S880Member.cs Bet 方法（Line 316-516）
                
                // 1. 合并订单（相同车号和玩法的订单合并）
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
                
                // 2. 构建POST数据（参考 F5BotV2 Line 349-420）
                var postPacket = new StringBuilder();
                try
                {
                    // P总
                    postPacket.Append($"t_0_22={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.单)}");
                    postPacket.Append($"&t_0_23={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.双)}");
                    postPacket.Append($"&t_0_24={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.大)}");
                    postPacket.Append($"&t_0_25={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.小)}");
                    postPacket.Append($"&t_0_26={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.尾大)}");
                    postPacket.Append($"&t_0_27={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.尾小)}");
                    postPacket.Append($"&t_0_37={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.龙)}");
                    postPacket.Append($"&t_0_38={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.虎)}");
                    
                    // P1-P5（参考 F5BotV2 Line 363-420）
                    for (int car = 1; car <= 5; car++)
                    {
                        var carEnum = (CarNumEnum)car;
                        postPacket.Append($"&t_{car}_22={GetBetString(betItems, carEnum, BetPlayEnum.单)}");
                        postPacket.Append($"&t_{car}_23={GetBetString(betItems, carEnum, BetPlayEnum.双)}");
                        postPacket.Append($"&t_{car}_24={GetBetString(betItems, carEnum, BetPlayEnum.大)}");
                        postPacket.Append($"&t_{car}_25={GetBetString(betItems, carEnum, BetPlayEnum.小)}");
                        postPacket.Append($"&t_{car}_26={GetBetString(betItems, carEnum, BetPlayEnum.尾大)}");
                        postPacket.Append($"&t_{car}_27={GetBetString(betItems, carEnum, BetPlayEnum.尾小)}");
                        postPacket.Append($"&t_{car}_28={GetBetString(betItems, carEnum, BetPlayEnum.合单)}");
                        postPacket.Append($"&t_{car}_29={GetBetString(betItems, carEnum, BetPlayEnum.合双)}");
                        // 注意：BetPlayEnum 中没有"福"和"禄"，使用其他枚举值或跳过
                        // postPacket.Append($"&t_{car}_30={GetBetString(betItems, carEnum, BetPlayEnum.福)}");
                        // postPacket.Append($"&t_{car}_31={GetBetString(betItems, carEnum, BetPlayEnum.禄)}");
                        postPacket.Append($"&t_{car}_32={GetBetString(betItems, carEnum, BetPlayEnum.寿)}");
                        postPacket.Append($"&t_{car}_33={GetBetString(betItems, carEnum, BetPlayEnum.喜)}");
                    }
                    
                    // 添加 p_id 和 tt_top（参考 F5BotV2）
                    postPacket.Append($"&p_id={_p_id}");
                    postPacket.Append($"&tt_top={_tt_top}");
                }
                catch (Exception ex)
                {
                    Log($"❌ 构建投注数据失败: {ex.Message}");
                    return (false, "", $"构建投注数据失败: {ex.Message}");
                }
                
                var postData = postPacket.ToString();
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 3. 发送POST请求（参考 F5BotV2 Line 440-456）
                string url = $"{_baseUrl}/user/bet.asp";  // 需要根据实际URL调整
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 4. 解析响应（需要根据实际响应格式调整）
                if (responseText.Contains("成功") || responseText.Contains("success"))
                {
                    Log($"✅ 投注成功");
                    return (true, $"S880{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}", responseText);
                }
                else
                {
                    Log($"❌ 投注失败");
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
        
        public void HandleResponse(ResponseEventArgs response)
        {
            try
            {
                // 参考 F5BotV2 S880Member.cs 的响应拦截逻辑（Line 74-108）
                if (response.Url.Contains("user/mail.asp"))
                {
                    _p_id = Regex.Match(response.Context, "id=\"p_id\" value=\"([^#> ]+)\"").Groups[1].Value;
                    _tt_top = Regex.Match(response.Context, "id=\"tt_top\" value=\"([^#> ]+)\"").Groups[1].Value;
                    
                    if (!string.IsNullOrEmpty(_p_id) && !string.IsNullOrEmpty(_tt_top))
                    {
                        Log($"✅ 拦截到投注参数: p_id={_p_id}, tt_top={_tt_top}");
                    }
                }
                
                // 设置 baseUrl（如果还未设置）
                if (string.IsNullOrEmpty(_baseUrl) && !string.IsNullOrEmpty(response.Url))
                {
                    try
                    {
                        _baseUrl = new Uri(response.Url).GetLeftPart(UriPartial.Authority);
                        Log($"✅ Base URL 已设置: {_baseUrl}");
                    }
                    catch { }
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

