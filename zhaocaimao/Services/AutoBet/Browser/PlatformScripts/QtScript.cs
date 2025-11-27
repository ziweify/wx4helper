using zhaocaimao.Shared.Models;
using zhaocaimao.Services.AutoBet.Browser.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace zhaocaimao.Services.AutoBet.Browser.PlatformScripts
{
    /// <summary>
    /// QT/蓝B 平台脚本 - 参考 F5BotV2/QtBet.cs
    /// </summary>
    public class QtScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isLoggedIn = false;
        private decimal _currentBalance = 0;
        private string _baseUrl = "";  // 缓存的base URL
        private bool _oddsUpdated = false;  // 赔率是否已更新
        
        // 赔率映射表（参考 F5BotV2 QtOdds.cs）
        private readonly Dictionary<string, Models.OddsInfo> _oddsMap = new Dictionary<string, Models.OddsInfo>();
        
        public QtScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表（参考 F5BotV2 QtOdds.cs）
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[QT] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（参考 F5BotV2 QtOdds.cs）
        /// </summary>
        private void InitializeOddsMap()
        {
            // 初始化基本赔率映射（实际赔率值需要从服务器获取）
            // P1
            _oddsMap["P1大"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", 1.97f, "44");
            _oddsMap["P1小"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.小, "平码一", 1.97f, "45");
            _oddsMap["P1单"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.单, "平码一", 1.97f, "44");
            _oddsMap["P1双"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.双, "平码一", 1.97f, "45");
            _oddsMap["P1尾大"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.尾大, "平码一", 1.97f, "46");
            _oddsMap["P1尾小"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.尾小, "平码一", 1.97f, "47");
            _oddsMap["P1合单"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.合单, "平码一", 1.97f, "48");
            _oddsMap["P1合双"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.合双, "平码一", 1.97f, "49");
            
            // P2-P5 类似...
            // P总
            _oddsMap["P总大"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.大, "总和", 1.97f, "22");
            _oddsMap["P总小"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.小, "总和", 1.97f, "23");
            _oddsMap["P总单"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.单, "总和", 1.97f, "24");
            _oddsMap["P总双"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.双, "总和", 1.97f, "25");
            _oddsMap["P总尾大"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.尾大, "总和", 1.97f, "26");
            _oddsMap["P总尾小"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.尾小, "总和", 1.97f, "27");
            _oddsMap["P总龙"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.龙, "总和", 1.97f, "37");
            _oddsMap["P总虎"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.虎, "总和", 1.97f, "38");
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
                Log("🔐 开始登录QT...");
                
                // 参考 F5BotV2 QtBet.cs 的登录逻辑
                var script = $@"
                    (function() {{
                        try {{
                            // 查找登录表单（QT登录表单选择器）
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
                        
                        // 检查是否已登录（通过URL判断）
                        var currentUrl = await _webView.CoreWebView2.ExecuteScriptAsync("window.location.href");
                        if (currentUrl != null && currentUrl.Contains("index/index"))
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
                
                // 🔥 参考 F5BotV2 QtBet.cs Bet 方法（Line 249-337）
                
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
                
                // 2. 构建POST数据（参考 F5BotV2 Line 281-295）
                List<dynamic> bets = new List<dynamic>();
                try
                {
                    for (int i = 0; i < betItems.Count; i++)
                    {
                        var order = betItems[i];
                        var key = $"{order.Car}{order.Play}";
                        
                        if (!_oddsMap.ContainsKey(key))
                        {
                            Log($"❌ 未找到赔率映射: {key}");
                            return (false, "", $"未找到赔率映射: {key}");
                        }
                        
                        var oddsInfo = _oddsMap[key];
                        dynamic betdata = new ExpandoObject();
                        betdata.action_no = order.IssueId.ToString();
                        betdata.bonus_prop_id = oddsInfo.CarName;  // 如 "平码一"
                        betdata.single_money = order.MoneySum.ToString();
                        betdata.action_data = oddsInfo.OddsId;  // 如 "44"
                        bets.Add(betdata);
                        
                        Log($"   {key}: 金额={order.MoneySum}, ID={oddsInfo.OddsId}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"❌ 构建投注数据失败: {ex.Message}");
                    return (false, "", $"构建投注数据失败: {ex.Message}");
                }
                
                string arrbet = JsonConvert.SerializeObject(bets);
                string arrbet_encode = WebUtility.UrlEncode(arrbet);
                string postData = $"lbrJson={arrbet_encode}";
                
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 3. 发送POST请求（参考 F5BotV2 Line 298-315）
                string url = $"{_baseUrl}/ntwbg/bet";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Headers.Add("Referer", $"{_baseUrl}/us/ntwbg/index");
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 4. 解析响应（参考 F5BotV2 Line 317-336）
                var json = JObject.Parse(responseText);
                var code = json["code"]?.Value<int>() ?? 0;
                
                if (code == 1)
                {
                    var actionNo = json["action_no"]?.ToString() ?? "";
                    Log($"✅ 投注成功: {actionNo}");
                    return (true, actionNo, responseText);
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
        /// 获取赔率（参考 F5BotV2 QtOdds.cs GetUpdata 方法）
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
                
                // 参考 F5BotV2 Line 368-509
                string url = $"{_baseUrl}/ntwbg/get_play";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                var json = JObject.Parse(responseText);
                var code = json["code"]?.Value<int>() ?? 0;
                
                if (code == 1)
                {
                    var jdata = json["data"];
                    var dataArray = JArray.Parse(jdata.ToString());
                    
                    foreach (var data in dataArray)
                    {
                        var id = data["id"]?.Value<int>() ?? 0;
                        var played_name = data["played_name"]?.ToString() ?? "";
                        var odds = data["odds"]?.Value<float>() ?? 1.97f;
                        
                        // 根据 played_name 和 id 更新赔率映射表
                        // 这里需要根据实际数据结构来映射
                        Log($"📊 赔率: ID={id}, 名称={played_name}, 赔率={odds}");
                    }
                    
                    _oddsUpdated = true;
                    Log($"✅ 赔率获取成功");
                    return true;
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
        
        public void HandleResponse(Services.ResponseEventArgs response)
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
        
        public List<Models.OddsInfo> GetOddsList()
        {
            // 返回赔率映射表中的所有赔率信息
            return _oddsMap.Values.ToList();
        }
    }
}

