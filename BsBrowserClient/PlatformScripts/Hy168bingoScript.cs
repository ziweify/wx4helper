using BaiShengVx3Plus.Shared.Helpers;
using BaiShengVx3Plus.Shared.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// HY168/bingo168 平台脚本 - 参考 F5BotV2/Hy168bingoMember.cs
    /// </summary>
    public class Hy168bingoScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ModernHttpHelper _httpHelper;
        private bool _isLoggedIn = false;
        private string _abcd = "";
        private string _ab = "";
        private string _gid = "";
        private decimal _currentBalance = 0;
        private string _baseUrl = "";
        
        // 赔率映射表（参考 F5BotV2 Hy168bingoOdds.cs）
        private readonly Dictionary<string, OddsInfo> _oddsMap = new Dictionary<string, OddsInfo>();
        
        public Hy168bingoScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            _httpHelper = new ModernHttpHelper(_httpClient);  // 🔥 初始化 ModernHttpHelper
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表（参考 F5BotV2 Hy168bingoOdds.cs）
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[HY168] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（参考 F5BotV2 Hy168bingoOdds.cs）
        /// </summary>
        private void InitializeOddsMap()
        {
            // 使用默认赔率映射（参考 F5BotV2）
            // P1
            _oddsMap["P1大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", "1", 1.97f);
            _oddsMap["P1小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.小, "平码一", "2", 1.97f);
            _oddsMap["P1单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.单, "平码一", "3", 1.97f);
            _oddsMap["P1双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.双, "平码一", "4", 1.97f);
            _oddsMap["P1尾大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾大, "平码一", "5", 1.97f);
            _oddsMap["P1尾小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾小, "平码一", "6", 1.97f);
            _oddsMap["P1合单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合单, "平码一", "7", 1.97f);
            _oddsMap["P1合双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合双, "平码一", "8", 1.97f);
            
            // P总
            _oddsMap["P总大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.大, "和值", "9", 1.97f);
            _oddsMap["P总小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.小, "和值", "10", 1.97f);
            _oddsMap["P总单"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.单, "和值", "11", 1.97f);
            _oddsMap["P总双"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.双, "和值", "12", 1.97f);
            _oddsMap["P总尾大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾大, "和值", "13", 1.97f);
            _oddsMap["P总尾小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾小, "和值", "14", 1.97f);
            _oddsMap["P总龙"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.龙, "和值", "15", 1.97f);
            _oddsMap["P总虎"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.虎, "和值", "16", 1.97f);
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
                Log("🔐 开始登录HY168...");
                
                // 参考 F5BotV2 Hy168bingoMember.cs 的登录逻辑
                // HY168的登录逻辑：等待URL变化，从登录页变为首页
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
                    // 等待登录完成（通过URL变化判断）
                    Log("⏳ 等待登录完成（请输入验证码并点击登录）...");
                    var waitCount = 0;
                    while (waitCount < 300)
                    {
                        await Task.Delay(100);
                        var currentUrl = _webView.CoreWebView2.Source;
                        if (!string.IsNullOrEmpty(currentUrl) && !currentUrl.Contains("Member/Login"))
                        {
                            Log("✅ 登录成功！");
                            _isLoggedIn = true;
                            
                            // 设置 baseUrl
                            try
                            {
                                _baseUrl = new Uri(currentUrl).GetLeftPart(UriPartial.Authority);
                                Log($"✅ Base URL 已设置: {_baseUrl}");
                            }
                            catch { }
                            
                            return true;
                        }
                        waitCount++;
                    }
                    
                    Log("❌ 登录超时或失败");
                    return false;
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
                
                if (string.IsNullOrEmpty(_abcd) || string.IsNullOrEmpty(_ab))
                {
                    Log("❌ 投注参数不完整，无法投注");
                    return (false, "", "投注参数不完整");
                }
                
                Log($"🎲 开始投注，订单数: {orders.Count}");
                
                // 🔥 参考 F5BotV2 Hy168bingoMember.cs Bet 方法（Line 966-1107）
                
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
                
                // 2. 构建POST数据（参考 F5BotV2 Line 1006-1037）
                var postPacket = new StringBuilder();
                postPacket.Append($"xtype=make");
                
                List<dynamic> bets = new List<dynamic>();
                for (int i = 0; i < betItems.Count; i++)
                {
                    var order = betItems[i];
                    var key = $"{order.Car}{order.Play}";
                    
                    if (!_oddsMap.ContainsKey(key))
                    {
                        Log($"❌ 未找到赔率映射: {key}");
                        return (false, "", $"未找到赔率映射: {key}");
                    }
                    
                    var odds = _oddsMap[key];
                    dynamic betdata = new ExpandoObject();
                    betdata.pid = odds.OddsId;
                    betdata.je = order.MoneySum.ToString();
                    betdata.name = order.Play.ToString();
                    betdata.peilv1 = odds.Odds.ToString("F2");
                    betdata.classx = odds.CarName;
                    betdata.con = "";
                    betdata.bz = "";
                    bets.Add(betdata);
                }
                
                string arrbet = JsonConvert.SerializeObject(bets);
                postPacket.Append($"&pstr={arrbet}");
                postPacket.Append($"&abcd={_abcd}");
                postPacket.Append($"&ab={_ab}");
                postPacket.Append($"&bid=1-6");
                
                var postData = postPacket.ToString();
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 3. 发送POST请求（参考 F5BotV2 Line 1039-1050）
                string url = $"{_baseUrl}/uxj/makelib.php";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 4. 解析响应（参考 F5BotV2 Line 1056-1084）
                var json = JObject.Parse(responseText);
                var succeed = json["status"]?.ToString() ?? "";
                
                if (succeed == "success")
                {
                    Log("✅ 投注成功");
                    return (true, "", responseText);
                }
                else
                {
                    var msg = json["message"]?.ToString() ?? "未知错误";
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
                // 拦截获取投注参数响应，提取 abcd, ab（参考 F5BotV2 Line 136-171）
                if (response.Url.Contains("/makelib.php"))
                {
                    if (!string.IsNullOrEmpty(response.PostData))
                    {
                        var parameters = new Dictionary<string, string>();
                        string[] pairs = response.PostData.Split('&');
                        foreach (string pair in pairs)
                        {
                            string[] keyValue = pair.Split('=');
                            if (keyValue.Length == 2)
                            {
                                string key = keyValue[0].Trim().ToLower();
                                string value = keyValue[1].Trim();
                                parameters[key] = value;
                            }
                        }
                        
                        if (parameters.ContainsKey("xtype") && parameters["xtype"] == "getatt")
                        {
                            if (parameters.ContainsKey("abcd"))
                                _abcd = parameters["abcd"];
                            if (parameters.ContainsKey("ab"))
                                _ab = parameters["ab"];
                            
                            if (!string.IsNullOrEmpty(_abcd) && !string.IsNullOrEmpty(_ab))
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
                                
                                Log($"✅ 拦截到投注参数: ABCD={_abcd}, AB={_ab}");
                            }
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

