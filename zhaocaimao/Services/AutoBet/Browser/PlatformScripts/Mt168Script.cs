using Unit.Shared.Models;
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
    /// 茅台/太平洋 平台脚本 - 参考 F5BotV2/Mt168Member.cs
    /// </summary>
    public class Mt168Script : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isLoggedIn = false;
        private decimal _currentBalance = 0;
        private string _baseUrl = "";  // 缓存的base URL
        private bool _oddsUpdated = false;  // 赔率是否已更新
        
        // 赔率映射表（参考 F5BotV2 Mt168Odds.cs）
        private readonly Dictionary<string, Models.OddsInfo> _oddsMap = new Dictionary<string, Models.OddsInfo>();
        
        public Mt168Script(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表（参考 F5BotV2 Mt168Odds.cs）
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[茅台] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（参考 F5BotV2 Mt168Odds.cs）
        /// </summary>
        private void InitializeOddsMap()
        {
            // P1
            _oddsMap["P1大"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", 1.97f, "DX1_D");
            _oddsMap["P1小"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.小, "平码一", 1.97f, "DX1_X");
            _oddsMap["P1单"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.单, "平码一", 1.97f, "DS1_D");
            _oddsMap["P1双"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.双, "平码一", 1.97f, "DS1_S");
            _oddsMap["P1尾大"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.尾大, "平码一", 1.97f, "WDX1_D");
            _oddsMap["P1尾小"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.尾小, "平码一", 1.97f, "WDX1_X");
            _oddsMap["P1合单"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.合单, "平码一", 1.97f, "HDS1_D");
            _oddsMap["P1合双"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.合双, "平码一", 1.97f, "HDS1_S");
            
            // P2
            _oddsMap["P2大"] = new Models.OddsInfo(CarNumEnum.P2, BetPlayEnum.大, "平码二", 1.97f, "DX2_D");
            _oddsMap["P2小"] = new Models.OddsInfo(CarNumEnum.P2, BetPlayEnum.小, "平码二", 1.97f, "DX2_X");
            _oddsMap["P2单"] = new Models.OddsInfo(CarNumEnum.P2, BetPlayEnum.单, "平码二", 1.97f, "DS2_D");
            _oddsMap["P2双"] = new Models.OddsInfo(CarNumEnum.P2, BetPlayEnum.双, "平码二", 1.97f, "DS2_S");
            _oddsMap["P2尾大"] = new Models.OddsInfo(CarNumEnum.P2, BetPlayEnum.尾大, "平码二", 1.97f, "WDX2_D");
            _oddsMap["P2尾小"] = new Models.OddsInfo(CarNumEnum.P2, BetPlayEnum.尾小, "平码二", 1.97f, "WDX2_X");
            _oddsMap["P2合单"] = new Models.OddsInfo(CarNumEnum.P2, BetPlayEnum.合单, "平码二", 1.97f, "HDS2_D");
            _oddsMap["P2合双"] = new Models.OddsInfo(CarNumEnum.P2, BetPlayEnum.合双, "平码二", 1.97f, "HDS2_S");
            
            // P3
            _oddsMap["P3大"] = new Models.OddsInfo(CarNumEnum.P3, BetPlayEnum.大, "平码三", 1.97f, "DX3_D");
            _oddsMap["P3小"] = new Models.OddsInfo(CarNumEnum.P3, BetPlayEnum.小, "平码三", 1.97f, "DX3_X");
            _oddsMap["P3单"] = new Models.OddsInfo(CarNumEnum.P3, BetPlayEnum.单, "平码三", 1.97f, "DS3_D");
            _oddsMap["P3双"] = new Models.OddsInfo(CarNumEnum.P3, BetPlayEnum.双, "平码三", 1.97f, "DS3_S");
            _oddsMap["P3尾大"] = new Models.OddsInfo(CarNumEnum.P3, BetPlayEnum.尾大, "平码三", 1.97f, "WDX3_D");
            _oddsMap["P3尾小"] = new Models.OddsInfo(CarNumEnum.P3, BetPlayEnum.尾小, "平码三", 1.97f, "WDX3_X");
            _oddsMap["P3合单"] = new Models.OddsInfo(CarNumEnum.P3, BetPlayEnum.合单, "平码三", 1.97f, "HDS3_D");
            _oddsMap["P3合双"] = new Models.OddsInfo(CarNumEnum.P3, BetPlayEnum.合双, "平码三", 1.97f, "HDS3_S");
            
            // P4
            _oddsMap["P4大"] = new Models.OddsInfo(CarNumEnum.P4, BetPlayEnum.大, "平码四", 1.97f, "DX4_D");
            _oddsMap["P4小"] = new Models.OddsInfo(CarNumEnum.P4, BetPlayEnum.小, "平码四", 1.97f, "DX4_X");
            _oddsMap["P4单"] = new Models.OddsInfo(CarNumEnum.P4, BetPlayEnum.单, "平码四", 1.97f, "DS4_D");
            _oddsMap["P4双"] = new Models.OddsInfo(CarNumEnum.P4, BetPlayEnum.双, "平码四", 1.97f, "DS4_S");
            _oddsMap["P4尾大"] = new Models.OddsInfo(CarNumEnum.P4, BetPlayEnum.尾大, "平码四", 1.97f, "WDX4_D");
            _oddsMap["P4尾小"] = new Models.OddsInfo(CarNumEnum.P4, BetPlayEnum.尾小, "平码四", 1.97f, "WDX4_X");
            _oddsMap["P4合单"] = new Models.OddsInfo(CarNumEnum.P4, BetPlayEnum.合单, "平码四", 1.97f, "HDS4_D");
            _oddsMap["P4合双"] = new Models.OddsInfo(CarNumEnum.P4, BetPlayEnum.合双, "平码四", 1.97f, "HDS4_S");
            
            // P5
            _oddsMap["P5大"] = new Models.OddsInfo(CarNumEnum.P5, BetPlayEnum.大, "特码", 1.97f, "DX5_D");
            _oddsMap["P5小"] = new Models.OddsInfo(CarNumEnum.P5, BetPlayEnum.小, "特码", 1.97f, "DX5_X");
            _oddsMap["P5单"] = new Models.OddsInfo(CarNumEnum.P5, BetPlayEnum.单, "特码", 1.97f, "DS5_D");
            _oddsMap["P5双"] = new Models.OddsInfo(CarNumEnum.P5, BetPlayEnum.双, "特码", 1.97f, "DS5_S");
            _oddsMap["P5尾大"] = new Models.OddsInfo(CarNumEnum.P5, BetPlayEnum.尾大, "特码", 1.97f, "WDX5_D");
            _oddsMap["P5尾小"] = new Models.OddsInfo(CarNumEnum.P5, BetPlayEnum.尾小, "特码", 1.97f, "WDX5_X");
            _oddsMap["P5合单"] = new Models.OddsInfo(CarNumEnum.P5, BetPlayEnum.合单, "特码", 1.97f, "HDS5_D");
            _oddsMap["P5合双"] = new Models.OddsInfo(CarNumEnum.P5, BetPlayEnum.合双, "特码", 1.97f, "HDS5_S");
            
            // P总
            _oddsMap["P总大"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.大, "和值", 1.97f, "ZDX_D");
            _oddsMap["P总小"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.小, "和值", 1.97f, "ZDX_X");
            _oddsMap["P总单"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.单, "和值", 1.97f, "ZDS_D");
            _oddsMap["P总双"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.双, "和值", 1.97f, "ZDS_S");
            _oddsMap["P总尾大"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.尾大, "和值", 1.97f, "ZWDX_D");
            _oddsMap["P总尾小"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.尾小, "和值", 1.97f, "ZWDX_X");
            _oddsMap["P总龙"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.龙, "和值", 1.97f, "LH_L");
            _oddsMap["P总虎"] = new Models.OddsInfo(CarNumEnum.P总, BetPlayEnum.虎, "和值", 1.97f, "LH_H");
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
                Log("🔐 开始登录茅台...");
                
                // 参考 F5BotV2 Mt168Member.cs 的登录逻辑
                var script = $@"
                    (function() {{
                        try {{
                            // 查找登录表单（茅台登录表单选择器）
                            const usernameInput = document.querySelector('body > div.main > div > div.login > form > div.info.username > input') ||
                                                  document.querySelector('input[name=""username""]') ||
                                                  document.querySelector('input[type=""text""]');
                            const passwordInput = document.querySelector('body > div.main > div > div.login > form > div.info.password > input') ||
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
                        if (currentUrl != null && !currentUrl.Contains("login"))
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
                
                // 参考 F5BotV2 Mt168Member.cs GetUserInfoUpdata 方法
                // 通过页面或API获取余额（需要根据实际平台实现）
                // 这里先返回缓存的余额
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
                
                // 🔥 参考 F5BotV2 Mt168Member.cs Bet 方法（Line 275-400）
                
                // 1. 获取赔率（如果未获取）
                if (!_oddsUpdated)
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
                
                // 3. 构建POST数据（参考 F5BotV2 Line 308-343）
                dynamic postPacket = new ExpandoObject();
                try
                {
                    postPacket.lottery = "TWBG";
                    postPacket.drawNumber = orders[0].IssueId.ToString();
                    
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
                        string[] odsName = oddsInfo.OddsId.Split('_');
                        
                        dynamic betdata = new ExpandoObject();
                        betdata.game = odsName[0];
                        betdata.contents = odsName[1];
                        betdata.amount = order.MoneySum;
                        betdata.odds = oddsInfo.Odds;
                        bets.Add(betdata);
                        
                        Log($"   {key}: 金额={order.MoneySum}, ID={oddsInfo.OddsId}, 赔率={oddsInfo.Odds}");
                    }
                    
                    postPacket.bets = bets;
                    postPacket.fastBets = false;
                    postPacket.ignore = false;
                }
                catch (Exception ex)
                {
                    Log($"❌ 构建投注数据失败: {ex.Message}");
                    return (false, "", $"构建投注数据失败: {ex.Message}");
                }
                
                string postData = JsonConvert.SerializeObject(postPacket);
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 4. 发送POST请求（参考 F5BotV2 Line 357-368）
                string url = $"{_baseUrl}/member/bet";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 5. 解析响应（参考 F5BotV2 Line 376-399）
                var json = JObject.Parse(responseText);
                var status = json["status"]?.Value<int>() ?? -1;
                
                if (status == 0)
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
        /// 获取赔率（参考 F5BotV2 Mt168Odds.cs GetUpdata 方法）
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
                
                // 参考 F5BotV2 Line 18-147
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string fullUrl = $"{_baseUrl}/member/odds?lottery=TWBG&games=DX1%2CDX2%2CDX3%2CDX4%2CDX5%2CDX6%2CDX7%2CDX8%2CWDX1%2CWDX2%2CWDX3%2CWDX4%2CWDX5%2CWDX6%2CWDX7%2CWDX8%2CDS1%2CDS2%2CDS3%2CDS4%2CDS5%2CDS6%2CDS7%2CDS8%2CHDS1%2CHDS2%2CHDS3%2CHDS4%2CHDS5%2CHDS6%2CHDS7%2CHDS8%2CZDX%2CZDS%2CZWDX%2CLH1%2CLH2%2CLH3%2CLH4%2CB1%2CB2%2CB3%2CB4%2CB5%2CB6%2CB7%2CB8%2CZM%2CMP%2CZFB%2CWDX1%2CWDX2%2CWDX3%2CWDX4%2CWDX5%2CWDX6%2CWDX7%2CWDX8%2CHDS1%2CHDS2%2CHDS3%2CHDS4%2CHDS5%2CHDS6%2CHDS7%2CHDS8%2CFS%2CFW%2CFW1%2CFW2%2CFW3%2CFW4%2CFW5%2CFW6%2CFW7%2CFW8%2CZFB1%2CZFB2%2CZFB3%2CZFB4%2CZFB5%2CZFB6%2CZFB7%2CZFB8%2CLH1%2CLH2%2CLH3%2CLH4%2CLM2%2CLM22%2CLM3%2CLM32%2CLM4%2CLM5&_={timestamp}";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                request.Headers.Add("Cookie", cookies);
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                var json = JObject.Parse(responseText);
                
                // 更新赔率值（参考 F5BotV2 Line 47-119）
                _oddsMap["P1大"].Odds = json["DX1_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P1小"].Odds = json["DX1_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P1单"].Odds = json["DS1_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P1双"].Odds = json["DS1_S"]?.Value<float>() ?? 1.97f;
                _oddsMap["P1尾大"].Odds = json["WDX1_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P1尾小"].Odds = json["WDX1_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P1合单"].Odds = json["HDS1_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P1合双"].Odds = json["HDS1_S"]?.Value<float>() ?? 1.97f;
                
                _oddsMap["P2大"].Odds = json["DX2_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P2小"].Odds = json["DX2_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P2单"].Odds = json["DS2_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P2双"].Odds = json["DS2_S"]?.Value<float>() ?? 1.97f;
                _oddsMap["P2尾大"].Odds = json["WDX2_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P2尾小"].Odds = json["WDX2_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P2合单"].Odds = json["HDS2_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P2合双"].Odds = json["HDS2_S"]?.Value<float>() ?? 1.97f;
                
                _oddsMap["P3大"].Odds = json["DX3_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P3小"].Odds = json["DX3_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P3单"].Odds = json["DS3_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P3双"].Odds = json["DS3_S"]?.Value<float>() ?? 1.97f;
                _oddsMap["P3尾大"].Odds = json["WDX3_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P3尾小"].Odds = json["WDX3_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P3合单"].Odds = json["HDS3_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P3合双"].Odds = json["HDS3_S"]?.Value<float>() ?? 1.97f;
                
                _oddsMap["P4大"].Odds = json["DX4_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P4小"].Odds = json["DX4_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P4单"].Odds = json["DS4_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P4双"].Odds = json["DS4_S"]?.Value<float>() ?? 1.97f;
                _oddsMap["P4尾大"].Odds = json["WDX4_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P4尾小"].Odds = json["WDX4_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P4合单"].Odds = json["HDS4_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P4合双"].Odds = json["HDS4_S"]?.Value<float>() ?? 1.97f;
                
                _oddsMap["P5大"].Odds = json["DX5_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P5小"].Odds = json["DX5_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P5单"].Odds = json["DS5_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P5双"].Odds = json["DS5_S"]?.Value<float>() ?? 1.97f;
                _oddsMap["P5尾大"].Odds = json["WDX5_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P5尾小"].Odds = json["WDX5_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P5合单"].Odds = json["HDS5_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P5合双"].Odds = json["HDS5_S"]?.Value<float>() ?? 1.97f;
                
                _oddsMap["P总大"].Odds = json["ZDX_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P总小"].Odds = json["ZDX_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P总单"].Odds = json["ZDS_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P总双"].Odds = json["ZDS_S"]?.Value<float>() ?? 1.97f;
                _oddsMap["P总尾大"].Odds = json["ZWDX_D"]?.Value<float>() ?? 1.97f;
                _oddsMap["P总尾小"].Odds = json["ZWDX_X"]?.Value<float>() ?? 1.97f;
                _oddsMap["P总龙"].Odds = json["LH_L"]?.Value<float>() ?? 1.97f;
                _oddsMap["P总虎"].Odds = json["LH_H"]?.Value<float>() ?? 1.97f;
                
                _oddsUpdated = true;
                Log($"✅ 赔率获取成功，共更新 {_oddsMap.Count} 项");
                return true;
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

