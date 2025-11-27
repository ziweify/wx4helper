using zhaocaimao.Shared.Models;
using zhaocaimao.Services.AutoBet.Browser.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace zhaocaimao.Services.AutoBet.Browser.PlatformScripts
{
    /// <summary>
    /// 元宇宙2 平台脚本 - 参考 F5BotV2/YYZ2Member.cs
    /// 注意：元宇宙2 需要获取会员线路，然后显示验证码输入窗口
    /// </summary>
    public class YYZ2Script : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isLoggedIn = false;
        private decimal _currentBalance = 0;
        private string _baseUrl = "";
        private string _memberLineUrl = "";  // 会员线路URL
        
        public YYZ2Script(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
        }
        
        private void Log(string message) => _logCallback($"[元宇宙2] {message}");
        
        /// <summary>
        /// 获取会员线路（参考 F5BotV2 YYZ2Member.cs getMemberLine 方法）
        /// </summary>
        private async Task<string> GetMemberLineAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_baseUrl))
                    return "";
                
                var response = await _httpClient.GetAsync(_baseUrl);
                var html = await response.Content.ReadAsStringAsync();
                
                // 解析HTML获取会员线路（参考 F5BotV2 Line 182-200）
                // 使用正则表达式提取会员线路1的URL
                var match = Regex.Match(html, @"会员线路1.*?accesskey=""([^""]+)""");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 获取会员线路失败: {ex.Message}");
            }
            
            return "";
        }
        
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Log("🔐 开始登录元宇宙2...");
                
                // 参考 F5BotV2 YYZ2Member.cs 的登录逻辑（Line 57-124）
                // 1. 获取会员线路
                if (string.IsNullOrEmpty(_memberLineUrl))
                {
                    _memberLineUrl = await GetMemberLineAsync();
                    if (string.IsNullOrEmpty(_memberLineUrl))
                    {
                        Log("❌ 无法获取会员线路");
                        return false;
                    }
                    Log($"✅ 获取到会员线路: {_memberLineUrl}");
                }
                
                // 2. 导航到会员线路
                _webView.CoreWebView2.Navigate(_memberLineUrl);
                await Task.Delay(2000);  // 等待页面加载
                
                // 3. 填充登录表单（参考 F5BotV2 Line 112-113）
                var script = $@"
                    (function() {{
                        try {{
                            // 查找登录表单
                            const inputs = document.querySelectorAll('input');
                            if (inputs.length >= 2) {{
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
                
                // 参考 F5BotV2 YYZ2Member.cs Bet 方法
                // 元宇宙2 使用 BetApi 的投注逻辑，这里先返回待实现
                Log("⚠️ 投注功能待实现（需要参考 F5BotV2 YYZ2Member.cs）");
                return (false, "", "投注功能待实现");
            }
            catch (Exception ex)
            {
                Log($"❌ 投注异常: {ex.Message}");
                return (false, "", $"投注异常: {ex.Message}");
            }
        }
        
        public void HandleResponse(Services.ResponseEventArgs response)
        {
            // 元宇宙2不需要拦截响应
        }
        
        public List<Models.OddsInfo> GetOddsList()
        {
            // 元宇宙2不需要赔率映射
            return new List<Models.OddsInfo>();
        }
    }
}

