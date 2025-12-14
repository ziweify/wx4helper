using BaiShengVx3Plus.Shared.Helpers;
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
    /// ADK 平台脚本 - 参考 F5BotV2/ADKMember.cs
    /// </summary>
    public class ADKScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ModernHttpHelper _httpHelper;
        private bool _isLoggedIn = false;
        private string _baseUrl = "";  // 缓存的base URL
        private string _qihaoid = "";  // 期号ID
        private string _mysession = "";  // session
        private decimal _currentBalance = 0;
        
        // 赔率映射表（参考 F5BotV2，ADK使用茅台的赔率映射）
        private readonly Dictionary<string, OddsInfo> _oddsMap = new Dictionary<string, OddsInfo>();
        
        public ADKScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            _httpHelper = new ModernHttpHelper(_httpClient);  // 🔥 初始化 ModernHttpHelper
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表（参考 F5BotV2，ADK使用茅台的赔率映射）
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[ADK] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（参考 F5BotV2，ADK使用茅台的赔率映射）
        /// </summary>
        private void InitializeOddsMap()
        {
            // 使用茅台的赔率映射（参考 F5BotV2 ADKMember.cs Line 35）
            // P1
            _oddsMap["P1大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", "DX1_D", 1.97f);
            _oddsMap["P1小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.小, "平码一", "DX1_X", 1.97f);
            _oddsMap["P1单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.单, "平码一", "DS1_D", 1.97f);
            _oddsMap["P1双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.双, "平码一", "DS1_S", 1.97f);
            _oddsMap["P1尾大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾大, "平码一", "WDX1_D", 1.97f);
            _oddsMap["P1尾小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾小, "平码一", "WDX1_X", 1.97f);
            _oddsMap["P1合单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合单, "平码一", "HDS1_D", 1.97f);
            _oddsMap["P1合双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合双, "平码一", "HDS1_S", 1.97f);
            
            // P2-P5 类似...
            // P总
            _oddsMap["P总大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.大, "和值", "ZDX_D", 1.97f);
            _oddsMap["P总小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.小, "和值", "ZDX_X", 1.97f);
            _oddsMap["P总单"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.单, "和值", "ZDS_D", 1.97f);
            _oddsMap["P总双"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.双, "和值", "ZDS_S", 1.97f);
            _oddsMap["P总尾大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾大, "和值", "ZWDX_D", 1.97f);
            _oddsMap["P总尾小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾小, "和值", "ZWDX_X", 1.97f);
            _oddsMap["P总龙"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.龙, "和值", "LH_L", 1.97f);
            _oddsMap["P总虎"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.虎, "和值", "LH_H", 1.97f);
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
        /// 获取投注金额字符串（参考 F5BotV2 ADKMember.cs GetBetString 方法）
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
                Log("🔐 开始登录ADK...");
                
                // 参考 F5BotV2 ADKMember.cs 的登录逻辑（Line 88-234）
                var script = $@"
                    (function() {{
                        try {{
                            // 查找登录表单（ADK登录表单选择器）
                            const usernameInput = document.querySelector('#txtUsername') ||
                                                  document.querySelector('input[name=""username""]') ||
                                                  document.querySelector('input[type=""text""]');
                            const passwordInput = document.querySelector('#txtPass') ||
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
                        
                        // 检查是否已登录（通过URL判断，参考 F5BotV2 Line 150-165）
                        var currentUrl = await _webView.CoreWebView2.ExecuteScriptAsync("window.location.href");
                        if (currentUrl != null && !currentUrl.Contains("login") && currentUrl.Contains("default.html"))
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
                
                Log($"🎲 开始投注，订单数: {orders.Count}");
                
                // 🔥 参考 F5BotV2 ADKMember.cs Bet 方法（Line 286-488）
                
                // 1. 获取期号ID和mysession（参考 F5BotV2 Line 318-349）
                int issueid = orders[0].IssueId;
                await GetQihaoIdAndSessionAsync(issueid);
                
                if (string.IsNullOrEmpty(_qihaoid) || string.IsNullOrEmpty(_mysession))
                {
                    Log("❌ 未获取到期号ID或mysession，无法投注");
                    return (false, "", "未获取到投注参数");
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
                
                // 3. 构建POST数据（参考 F5BotV2 Line 356-426）
                var postPacket = new StringBuilder();
                try
                {
                    // P总
                    postPacket.Append($"txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.尾大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.龙)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.尾小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.虎)}");
                    
                    // P1-P5（参考 F5BotV2 Line 371-418）
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.大)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.小)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.单)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.双)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.尾大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.尾大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.尾大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.尾大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.尾大)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.尾小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.尾小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.尾小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.尾小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.尾小)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.合单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.合单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.合单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.合单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.合单)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.合双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.合双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.合双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.合双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.合双)}");
                    
                    // 其他玩法的空值（参考 F5BotV2 Line 420-423）
                    for (int i = 0; i < 200; i++)
                    {
                        postPacket.Append($"&txtnumber=");
                    }
                    
                    postPacket.Append($"&txtnumber2=&txtvalue_258=&txtnumber=&txtvalue_248=&txtnumber=&txtvalue_249=&txtnumber=&txtvalue_250=&txtnumber=&txtvalue_251=&txtnumber=&txtvalue_252=&txtnumber=&txtvalue_253=&txtnumber=&txtvalue_254=&txtnumber=&txtvalue_255=&txtnumber=&txtvalue_256=&txtnumber=&txtvalue_257=");
                    postPacket.Append($"&qihaoid={_qihaoid}&mysession={_mysession}");
                }
                catch (Exception ex)
                {
                    Log($"❌ 构建投注数据失败: {ex.Message}");
                    return (false, "", $"构建投注数据失败: {ex.Message}");
                }
                
                var postData = postPacket.ToString();
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 4. 发送POST请求（参考 F5BotV2 Line 443-455）
                string url = $"{_baseUrl}/default.html?method=add";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Headers.Add("Referer", $"{_baseUrl}/default.html");
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 5. 解析响应（参考 F5BotV2 Line 458-488）
                var json = JObject.Parse(responseText);
                var error = json["error"]?.Value<bool>() ?? true;
                
                if (!error)
                {
                    Log($"✅ 投注成功");
                    return (true, $"ADK{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}", responseText);
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
        /// 获取期号ID和mysession（参考 F5BotV2 ADKMember.cs Line 318-349）
        /// </summary>
        private async Task<bool> GetQihaoIdAndSessionAsync(int issueid)
        {
            try
            {
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    Log("❌ 未获取到base URL，无法获取期号ID和session");
                    return false;
                }
                
                var cookies = await GetCookiesAsync();
                
                // 1. 获取期号ID（参考 F5BotV2 Line 323-338）
                string qihaoUrl = $"{_baseUrl}/source/AjaxShiFen.ashx";
                var qihaoRequest = new HttpRequestMessage(HttpMethod.Post, qihaoUrl);
                qihaoRequest.Headers.Add("Cookie", cookies);
                qihaoRequest.Headers.Add("Referer", $"{_baseUrl}/default.html");
                qihaoRequest.Headers.Add("Accept", "text/plain, */*; q=0.01");
                
                var qihaoResponse = await _httpClient.SendAsync(qihaoRequest);
                var qihaoText = await qihaoResponse.Content.ReadAsStringAsync();
                
                string[] sfens = qihaoText.Split('|');
                if (sfens.Length > 2)
                {
                    if (sfens[2].Replace(" ", "") == issueid.ToString().Replace(" ", ""))
                    {
                        _qihaoid = sfens[4];
                        Log($"✅ 获取到期号ID: {_qihaoid}");
                    }
                }
                
                // 2. 获取mysession（参考 F5BotV2 Line 342-349）
                string paramUrl = $"{_baseUrl}/default.html";
                var paramRequest = new HttpRequestMessage(HttpMethod.Get, paramUrl);
                paramRequest.Headers.Add("Cookie", cookies);
                paramRequest.Headers.Add("Referer", $"{_baseUrl}/default.html");
                
                var paramResponse = await _httpClient.SendAsync(paramRequest);
                var paramHtml = await paramResponse.Content.ReadAsStringAsync();
                
                var match = Regex.Match(paramHtml, @"name=""mysession""[^#]+value=""([^#]+)""");
                if (match.Success)
                {
                    _mysession = match.Groups[1].Value;
                    Log($"✅ 获取到mysession: {_mysession.Substring(0, Math.Min(10, _mysession.Length))}...");
                }
                
                return !string.IsNullOrEmpty(_qihaoid) && !string.IsNullOrEmpty(_mysession);
            }
            catch (Exception ex)
            {
                Log($"❌ 获取期号ID和session异常: {ex.Message}");
                return false;
            }
        }
        
        public void HandleResponse(ResponseEventArgs response)
        {
            try
            {
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

