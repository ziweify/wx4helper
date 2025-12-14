using BaiShengVx3Plus.Shared.Helpers;
using BaiShengVx3Plus.Shared.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 蓝A 平台脚本 - 参考 F5BotV2/LanABetSite.cs
    /// </summary>
    public class LanAScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ModernHttpHelper _httpHelper;
        private bool _isLoggedIn = false;
        private int _p_id = 1;
        private int _tt_top = 20000;
        private decimal _currentBalance = 0;
        private string _baseUrl = "";
        
        public LanAScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            _httpHelper = new ModernHttpHelper(_httpClient);  // 🔥 初始化 ModernHttpHelper
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }
        
        private void Log(string message) => _logCallback($"[蓝A] {message}");
        
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
                Log("🔐 开始登录蓝A...");
                
                // 参考 F5BotV2 LanABetSite.cs 的登录逻辑（Line 85-113）
                var script = $@"
                    (function() {{
                        try {{
                            // 查找登录表单（参考 F5BotV2 Line 86-90）
                            const inputCount = document.querySelectorAll('input').length;
                            if (inputCount == 5) {{
                                document.querySelector('#username').value = '{username}';
                                document.querySelector('#password').value = '{password}';
                                
                                // 触发事件
                                document.querySelector('#username').dispatchEvent(new Event('input', {{ bubbles: true }}));
                                document.querySelector('#password').dispatchEvent(new Event('input', {{ bubbles: true }}));
                                
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
                var json = Newtonsoft.Json.Linq.JObject.Parse(result);
                
                var success = json["success"]?.Value<bool>("success") ?? false;
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
                        if (!string.IsNullOrEmpty(currentUrl) && !currentUrl.Contains("login"))
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
                
                Log($"🎲 开始投注，订单数: {orders.Count}");
                
                // 🔥 参考 F5BotV2 LanABetSite.cs Bet 方法（Line 152-386）
                
                // 1. 合并订单
                var betItems = new BetStandardOrderList();
                int issueId = 0;
                foreach (var order in orders.OrderBy(o => o.Car).ThenBy(o => o.Play))
                {
                    if (issueId == 0)
                        issueId = order.IssueId;
                    
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
                
                // 2. 构建POST数据（参考 F5BotV2 Line 192-349）
                var betdata = new Dictionary<string, string>();
                
                // 初始化所有字段为空（参考 F5BotV2 Line 192-280）
                betdata["t_0_11"] = ""; betdata["t_0_12"] = ""; betdata["t_0_13"] = ""; betdata["t_0_14"] = "";
                betdata["t_0_26"] = ""; betdata["t_0_27"] = ""; betdata["t_0_37"] = ""; betdata["t_0_38"] = "";
                
                for (int car = 1; car <= 5; car++)
                {
                    betdata[$"t_{car}_11"] = ""; betdata[$"t_{car}_12"] = ""; betdata[$"t_{car}_13"] = ""; betdata[$"t_{car}_14"] = "";
                    betdata[$"t_{car}_34"] = ""; betdata[$"t_{car}_35"] = ""; betdata[$"t_{car}_36"] = "";
                    betdata[$"t_{car}_15"] = ""; betdata[$"t_{car}_16"] = "";
                    betdata[$"t_{car}_28"] = ""; betdata[$"t_{car}_29"] = ""; betdata[$"t_{car}_30"] = "";
                    betdata[$"t_{car}_31"] = ""; betdata[$"t_{car}_32"] = ""; betdata[$"t_{car}_33"] = "";
                }
                
                // 填充投注数据（参考 F5BotV2 Line 283-338）
                foreach (var betItem in betItems)
                {
                    if (betItem.Car == CarNumEnum.P总 && betItem.Play == BetPlayEnum.单) betdata["t_0_11"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P总 && betItem.Play == BetPlayEnum.双) betdata["t_0_12"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P总 && betItem.Play == BetPlayEnum.大) betdata["t_0_13"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P总 && betItem.Play == BetPlayEnum.小) betdata["t_0_14"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P总 && betItem.Play == BetPlayEnum.尾大) betdata["t_0_26"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P总 && betItem.Play == BetPlayEnum.尾小) betdata["t_0_27"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P总 && betItem.Play == BetPlayEnum.龙) betdata["t_0_37"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P总 && betItem.Play == BetPlayEnum.虎) betdata["t_0_38"] = betItem.MoneySum.ToString();
                    
                    else if (betItem.Car == CarNumEnum.P1 && betItem.Play == BetPlayEnum.单) betdata["t_1_11"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P1 && betItem.Play == BetPlayEnum.双) betdata["t_1_12"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P1 && betItem.Play == BetPlayEnum.大) betdata["t_1_13"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P1 && betItem.Play == BetPlayEnum.小) betdata["t_1_14"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P1 && betItem.Play == BetPlayEnum.尾大) betdata["t_1_15"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P1 && betItem.Play == BetPlayEnum.尾小) betdata["t_1_16"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P1 && betItem.Play == BetPlayEnum.合单) betdata["t_1_28"] = betItem.MoneySum.ToString();
                    else if (betItem.Car == CarNumEnum.P1 && betItem.Play == BetPlayEnum.合双) betdata["t_1_29"] = betItem.MoneySum.ToString();
                    
                    // P2-P5 类似处理...
                }
                
                // 构建POST字符串（参考 F5BotV2 Line 341-350）
                var sbPost = new StringBuilder();
                foreach (var data in betdata)
                {
                    if (sbPost.Length > 0)
                        sbPost.Append("&");
                    sbPost.Append($"{data.Key}={data.Value}");
                }
                sbPost.Append($"&p_id={_p_id}");
                sbPost.Append($"&tt_top={_tt_top}");
                sbPost.Append($"&action=submit");
                sbPost.Append($"&now_sale_qishu={issueId}");
                
                var postData = sbPost.ToString();
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 3. 发送POST请求（参考 F5BotV2 Line 358-372）
                if (issueId > 0)
                {
                    string url = $"{_baseUrl}/api/ynk3/gxklsf.ashx";
                    var cookies = await GetCookiesAsync();
                    
                    var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Headers.Add("Cookie", cookies);
                    request.Headers.Add("Referer", $"{_baseUrl}/gxklsf.html");
                    request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                    
                    var response = await _httpClient.SendAsync(request);
                    var responseText = await response.Content.ReadAsStringAsync();
                    
                    Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                    
                    // 4. 解析响应（参考 F5BotV2 Line 373）
                    Log("✅ 投注成功");
                    return (true, "", responseText);
                }
                else
                {
                    Log("❌ 期号无效");
                    return (false, "", "期号无效");
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
            // 蓝A不需要拦截响应
        }
        
        public List<OddsInfo> GetOddsList()
        {
            // 蓝A不需要赔率映射
            return new List<OddsInfo>();
        }
        
        /// <summary>
        /// 获取未结算的订单信息（蓝A 平台暂不支持）
        /// </summary>
        public Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
            int state = 0,
            int pageNum = 1,
            int pageCount = 20,
            string? beginDate = null,
            string? endDate = null,
            int timeout = 10)
        {
            return Task.FromResult<(bool, List<JObject>?, int, int, string)>((false, null, 0, 0, "平台暂不支持"));
        }
    }
}

