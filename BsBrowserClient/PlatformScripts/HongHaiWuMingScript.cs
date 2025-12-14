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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 红海无名 平台脚本 - 参考 F5BotV2/HongHaiWuMing.cs
    /// </summary>
    public class HongHaiWuMingScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ModernHttpHelper _httpHelper;
        private bool _isLoggedIn = false;
        private string _sid = "";
        private string _uuid = "";
        private string _token = "";
        private decimal _currentBalance = 0;
        private string _baseUrl = "";  // 缓存的base URL
        
        // 赔率映射表（参考 F5BotV2，红海无名使用红海的赔率映射）
        private readonly Dictionary<string, OddsInfo> _oddsMap = new Dictionary<string, OddsInfo>();
        
        public HongHaiWuMingScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            _httpHelper = new ModernHttpHelper(_httpClient);  // 🔥 初始化 ModernHttpHelper
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表（参考 F5BotV2，红海无名使用红海的赔率映射）
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[红海无名] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（参考 F5BotV2，红海无名使用红海的赔率映射）
        /// </summary>
        private void InitializeOddsMap()
        {
            // 使用红海的赔率映射（参考 F5BotV2 HongHaiWuMing.cs Line 33）
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
        
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Log("🔐 开始登录红海无名...");
                
                // 参考 F5BotV2 HongHaiWuMing.cs 的登录逻辑
                // 红海无名的登录逻辑与红海类似，但需要等待特定URL
                var script = $@"
                    (function() {{
                        try {{
                            // 查找登录表单
                            const usernameInput = document.querySelector('#login > div.content > div > div.form > ul > li.l1 > input') ||
                                                  document.querySelector('input[name=""username""]') ||
                                                  document.querySelector('input[type=""text""]');
                            const passwordInput = document.querySelector('#login > div.content > div > div.form > ul > li.l2 > input') ||
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
                    // 等待登录完成（通过拦截响应获取 sid, uuid, token）
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
                
                if (string.IsNullOrEmpty(_sid) || string.IsNullOrEmpty(_uuid) || string.IsNullOrEmpty(_token))
                {
                    Log("❌ 登录参数不完整，无法投注");
                    return (false, "", "登录参数不完整");
                }
                
                Log($"🎲 开始投注，订单数: {orders.Count}");
                
                // 🔥 参考 F5BotV2 HongHaiWuMing.cs Bet 方法（与红海类似）
                
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
                
                // 2. 构建POST数据（参考 F5BotV2，与红海类似）
                var postPacket = new StringBuilder();
                try
                {
                    postPacket.Append($"uuid={_uuid}");
                    postPacket.Append($"&sid={_sid}");
                    postPacket.Append($"&roomeng=twbingo");
                    postPacket.Append($"&pan=A");
                    
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
                        
                        var oddsInfo = _oddsMap[key];
                        dynamic betdata = new ExpandoObject();
                        betdata.id = int.Parse(oddsInfo.OddsId);
                        betdata.money = order.MoneySum;
                        bets.Add(betdata);
                        
                        Log($"   {key}: 金额={order.MoneySum}, ID={oddsInfo.OddsId}");
                    }
                    
                    string arrbet = JsonConvert.SerializeObject(bets);
                    string arrbet_encode = WebUtility.UrlEncode(arrbet);
                    postPacket.Append($"&arrbet={arrbet_encode}");
                    postPacket.Append($"&token={_token}");
                    postPacket.Append($"&timestamp={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
                }
                catch (Exception ex)
                {
                    Log($"❌ 构建投注数据失败: {ex.Message}");
                    return (false, "", $"构建投注数据失败: {ex.Message}");
                }
                
                var postData = postPacket.ToString();
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 3. 发送POST请求
                string url = $"{_baseUrl}/comgame/setneworder";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 4. 解析响应
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
                // 拦截获取今日输赢响应，提取 sid, uuid, token（参考 F5BotV2 Line 98-103）
                if (response.Url.Contains("/gettodaywinlost"))
                {
                    if (!string.IsNullOrEmpty(response.PostData))
                    {
                        _sid = Regex.Match(response.PostData, "sid=([^&]+)").Groups[1].Value;
                        _uuid = Regex.Match(response.PostData, "uuid=([^&]+)").Groups[1].Value;
                        _token = Regex.Match(response.PostData, "token=([^&]+)").Groups[1].Value;
                        
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
            int state = 0, int pageNum = 1, int pageCount = 20, string? beginDate = null, string? endDate = null)
        {
            Log("⚠️ 红海无名 平台暂不支持获取订单列表");
            return Task.FromResult<(bool, List<JObject>?, int, int, string)>((false, null, 0, 0, "平台暂不支持"));
        }
    }
}

