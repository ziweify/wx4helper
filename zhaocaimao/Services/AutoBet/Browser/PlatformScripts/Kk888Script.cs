using zhaocaimao.Shared.Models;
using zhaocaimao.Services.AutoBet.Browser.Services;
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

namespace zhaocaimao.Services.AutoBet.Browser.PlatformScripts
{
    /// <summary>
    /// 果然 平台脚本 - 参考 F5BotV2/Kk888Member.cs
    /// </summary>
    public class Kk888Script : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isLoggedIn = false;
        private string _uid = "";
        private string _grpid = "";
        private decimal _currentBalance = 0;
        private string _baseUrl = "";
        private bool _isOddsStatus = false;  // 是否已获取赔率
        
        // 赔率映射表（参考 F5BotV2 Kk888Odds.cs）
        private readonly Dictionary<string, Models.OddsInfo> _oddsMap = new Dictionary<string, Models.OddsInfo>();
        
        public Kk888Script(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表（参考 F5BotV2 Kk888Odds.cs）
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[果然] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（参考 F5BotV2 Kk888Odds.cs）
        /// </summary>
        private void InitializeOddsMap()
        {
            // P1
            _oddsMap["P1大"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", 1.97f, "3110101");
            _oddsMap["P1小"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.小, "平码一", 1.97f, "3110102");
            _oddsMap["P1单"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.单, "平码一", 1.97f, "3120101");
            _oddsMap["P1双"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.双, "平码一", 1.97f, "3120102");
            _oddsMap["P1尾大"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.尾大, "平码一", 1.97f, "3130101");
            _oddsMap["P1尾小"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.尾小, "平码一", 1.97f, "3130102");
            _oddsMap["P1合单"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.合单, "平码一", 1.97f, "3140101");
            _oddsMap["P1合双"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.合双, "平码一", 1.97f, "3140102");
            
            // P2-P5 类似...
            // P总
            _oddsMap["P总大"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.大, "和值", 1.97f, "3110001");
            _oddsMap["P总小"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.小, "和值", 1.97f, "3110002");
            _oddsMap["P总单"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.单, "和值", 1.97f, "3120001");
            _oddsMap["P总双"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.双, "和值", 1.97f, "3120002");
            _oddsMap["P总尾大"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.尾大, "和值", 1.97f, "3130001");
            _oddsMap["P总尾小"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.尾小, "和值", 1.97f, "3130002");
            _oddsMap["P总龙"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.龙, "和值", 1.97f, "3150001");
            _oddsMap["P总虎"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.虎, "和值", 1.97f, "3150002");
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
        
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Log("🔐 开始登录果然...");
                
                // 参考 F5BotV2 Kk888Member.cs 的登录逻辑
                // 果然的登录逻辑：等待URL变化，从 /user/login 变为包含 /User/Bet/?gt=BINGO
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
                    // 等待登录完成（通过拦截响应获取 uid, grpid）
                    Log("⏳ 等待登录完成（请输入验证码并点击登录）...");
                    var waitCount = 0;
                    while (string.IsNullOrEmpty(_uid) && waitCount < 300)
                    {
                        await Task.Delay(100);
                        waitCount++;
                    }
                    
                    if (!string.IsNullOrEmpty(_uid) && !string.IsNullOrEmpty(_grpid))
                    {
                        Log($"✅ 登录成功！UID: {_uid}, GRPID: {_grpid}");
                        _isLoggedIn = true;
                        
                        // 获取赔率
                        await UpdateOddsAsync();
                        
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
        
        /// <summary>
        /// 更新赔率（参考 F5BotV2 Kk888Member.cs GetOdds 方法）
        /// </summary>
        private async Task UpdateOddsAsync()
        {
            try
            {
                if (_isOddsStatus || string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_uid) || string.IsNullOrEmpty(_grpid))
                    return;
                
                Log("📊 获取赔率...");
                
                var cookies = await GetCookiesAsync();
                var random = new Random();
                var issueId = DateTime.Now.ToString("yyyyMMddHHmm");  // 当前期数
                var postData = $"gt=BINGO&grpid={_grpid}&prekjqs={issueId}&r={random.NextDouble()}&uid={_uid}";
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/User/Bet/getplinfo");
                request.Headers.Add("Cookie", cookies);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                var json = JObject.Parse(responseText);
                var jdata = json["data"]?.ToArray() ?? new JToken[0];
                
                foreach (var odd in jdata)
                {
                    var name = odd["gid"]?.ToString() ?? "";
                    var ov = odd["ov"]?.Value<float>() ?? 0;
                    
                    // 更新赔率值
                    var key = _oddsMap.Keys.FirstOrDefault(k => _oddsMap[k].OddsId == name);
                    if (key != null)
                    {
                        _oddsMap[key] = new Models.OddsInfo(_oddsMap[key].Car, _oddsMap[key].Play, _oddsMap[key].CarName, ov, name);
                    }
                }
                
                _isOddsStatus = true;
                Log("✅ 赔率更新完成");
            }
            catch (Exception ex)
            {
                Log($"❌ 更新赔率失败: {ex.Message}");
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
                
                if (string.IsNullOrEmpty(_uid) || string.IsNullOrEmpty(_grpid))
                {
                    Log("❌ 登录参数不完整，无法投注");
                    return (false, "", "登录参数不完整");
                }
                
                Log($"🎲 开始投注，订单数: {orders.Count}");
                
                // 🔥 参考 F5BotV2 Kk888Member.cs Bet 方法
                
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
                
                // 2. 构建POST数据（参考 F5BotV2 Line 201-229）
                string uPI_ID = "";
                string uPI_P = "";
                string uPI_M = "";
                
                foreach (var item in betItems)
                {
                    var key = $"{item.Car}{item.Play}";
                    if (!_oddsMap.ContainsKey(key))
                    {
                        Log($"❌ 未找到赔率映射: {key}");
                        return (false, "", $"未找到赔率映射: {key}");
                    }
                    
                    var odds = _oddsMap[key];
                    if (!string.IsNullOrEmpty(odds.CarName) && odds.Odds > 0.5 && item.MoneySum > 0)
                    {
                        if (!string.IsNullOrEmpty(uPI_ID)) uPI_ID += ",";
                        if (!string.IsNullOrEmpty(uPI_P)) uPI_P += ",";
                        if (!string.IsNullOrEmpty(uPI_M)) uPI_M += ",";
                        
                        uPI_ID += odds.OddsId;
                        uPI_P += odds.Odds.ToString("F2");
                        uPI_M += item.MoneySum;
                    }
                }
                
                var issueId = orders[0].IssueId;
                var random = new Random();
                var postData = $"gt=BINGO&qs={issueId}" +
                              $"&uPI_ID={WebUtility.UrlEncode(uPI_ID)}" +
                              $"&uPI_P={WebUtility.UrlEncode(uPI_P)}" +
                              $"&uPI_M={WebUtility.UrlEncode(uPI_M)}" +
                              $"&r={random.NextDouble()}" +
                              $"&uid={_uid}";
                
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 3. 发送POST请求
                string url = $"{_baseUrl}/User/Bet/Betsave";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 4. 解析响应（参考 F5BotV2 Line 246-254）
                if (responseText.IndexOf("投注成功") != -1)
                {
                    Log("✅ 投注成功");
                    return (true, "", responseText);
                }
                else
                {
                    Log($"❌ 投注失败: {responseText}");
                    return (false, "", responseText);
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 投注异常: {ex.Message}");
                return (false, "", $"投注异常: {ex.Message}");
            }
        }
        
        public void HandleResponse(Services.ResponseEventArgs response)
        {
            try
            {
                // 拦截获取投注页面响应，提取 uid, grpid（参考 F5BotV2 Line 333-340）
                if (response.Url.Contains("/User/Bet/?gt=BINGO"))
                {
                    _uid = Regex.Match(response.Url, "UID=([^&]+)").Groups[1].Value;
                    _grpid = Regex.Match(response.Url, "grpid=([^&]+)").Groups[1].Value;
                    
                    if (!string.IsNullOrEmpty(_uid) && !string.IsNullOrEmpty(_grpid))
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
                        
                        Log($"✅ 拦截到登录参数: UID={_uid}, GRPID={_grpid}");
                        _isLoggedIn = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 处理响应失败: {ex.Message}");
            }
        }
        
        public List<Models.OddsInfo> GetOddsList()
        {
            // 返回赔率映射表中的所有赔率信息
            return _oddsMap.Values.ToList();
        }
    }
}

