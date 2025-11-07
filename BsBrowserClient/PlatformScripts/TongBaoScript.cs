using BsBrowserClient.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 通宝28 平台脚本 - 参考 F5BotV2/TongBaoMember.cs
    /// </summary>
    public class TongBaoScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        
        // 关键参数（从拦截中获取）
        private string _sid = "";
        private string _uuid = "";
        private string _token = "";
        private string _region = "A";  // A,B,C,D盘类型
        private decimal _currentBalance = 0;
        
        // 测试账号（来自F5BotV2注释）
        // 账号: wwww11
        // 密码: Aaa123
        
        public TongBaoScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
        }
        
        /// <summary>
        /// 登录 - 辅助填充表单，用户手动点击登录
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                _logCallback($"🔐 开始登录通宝: {username}");
                
                // 方法1：辅助填充表单，用户手动点击登录
                // F5BotV2 也是手动登录，因为通宝有验证码
                var script = $@"
                    (function() {{
                        try {{
                            // 查找用户名输入框
                            const usernameInput = document.querySelector('input[name=""username""]') ||
                                                  document.querySelector('input[type=""text""]') ||
                                                  document.querySelector('#login input[type=""text""]');
                            
                            // 查找密码输入框
                            const passwordInput = document.querySelector('input[name=""password""]') ||
                                                  document.querySelector('input[type=""password""]') ||
                                                  document.querySelector('#login input[type=""password""]');
                            
                            if (usernameInput && passwordInput) {{
                                usernameInput.value = '{username}';
                                passwordInput.value = '{password}';
                                
                                // 触发事件
                                usernameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                
                                return {{ success: true, message: '表单已填充，请输入验证码并点击登录' }};
                            }} else {{
                                return {{ success: false, message: '找不到登录表单' }};
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
                
                _logCallback(success ? $"✅ {message}" : $"❌ {message}");
                
                if (success)
                {
                    // 等待拦截到登录成功的参数（sid, uuid, token）
                    _logCallback("⏳ 等待登录完成（请输入验证码并点击登录）...");
                    var waitCount = 0;
                    while (string.IsNullOrEmpty(_sid) && string.IsNullOrEmpty(_uuid) && waitCount < 300)  // 30秒超时
                    {
                        await Task.Delay(100);
                        waitCount++;
                    }
                    
                    if (!string.IsNullOrEmpty(_sid) && !string.IsNullOrEmpty(_uuid))
                    {
                        _logCallback($"✅ 登录成功！UUID: {_uuid}, SID: {_sid.Substring(0, 10)}...");
                        return true;
                    }
                    else
                    {
                        _logCallback("❌ 登录超时或失败");
                        return false;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 登录失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 获取余额
        /// </summary>
        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_sid) || string.IsNullOrEmpty(_uuid))
                {
                    _logCallback("❌ 未登录，无法获取余额");
                    return -1;
                }
                
                _logCallback("💰 获取余额...");
                
                // 通宝的余额通常会在拦截的响应中
                // 如果当前余额为0，尝试从页面读取
                if (_currentBalance == 0)
                {
                    var script = @"
                        (function() {
                            try {
                                // 常见的余额显示元素
                                const balanceElement = document.querySelector('.balance') ||
                                                      document.querySelector('.user-balance') ||
                                                      document.querySelector('[class*=""balance""]') ||
                                                      document.querySelector('[class*=""money""]');
                                
                                if (balanceElement) {
                                    const text = balanceElement.innerText || balanceElement.textContent;
                                    const match = text.match(/[\d,.]+/);
                                    if (match) {
                                        return { success: true, balance: parseFloat(match[0].replace(/,/g, '')) };
                                    }
                                }
                                
                                return { success: false, balance: 0, message: '找不到余额元素' };
                            } catch (error) {
                                return { success: false, balance: 0, message: error.message };
                            }
                        })();
                    ";
                    
                    var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    var json = JObject.Parse(result);
                    
                    if (json["success"]?.Value<bool>() ?? false)
                    {
                        _currentBalance = json["balance"]?.Value<decimal>() ?? 0;
                    }
                }
                
                _logCallback($"💰 当前余额: {_currentBalance}");
                return _currentBalance;
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 获取余额失败: {ex.Message}");
                return -1;
            }
        }
        
        /// <summary>
        /// 下注 - 使用HTTP POST
        /// 参考 F5BotV2 的 Bet 方法
        /// </summary>
        public async Task<(bool success, string orderId)> PlaceBetAsync(BetOrder order)
        {
            try
            {
                if (string.IsNullOrEmpty(_sid) || string.IsNullOrEmpty(_uuid) || string.IsNullOrEmpty(_token))
                {
                    _logCallback("❌ 未登录，无法下注");
                    return (false, "");
                }
                
                _logCallback($"🎲 开始投注: {order.BetContent} {order.Amount}");
                
                // 构造POST数据（参考F5BotV2 Line 358-391）
                var postData = new StringBuilder();
                postData.Append($"uuid={_uuid}");
                postData.Append($"&sid={_sid}");
                postData.Append($"&roomeng=twbingo");
                postData.Append($"&pan={_region}");
                postData.Append($"&shuitype=0");
                
                // 构造投注数组
                // 注意：这里需要根据实际的赔率ID映射来设置 id
                // 示例：大=1, 小=2, 单=3, 双=4 等
                var betId = GetBetId(order.BetContent);
                var bets = new[]
                {
                    new { id = betId, money = order.Amount }
                };
                
                var arrbet = JsonConvert.SerializeObject(bets);
                var arrbet_encoded = System.Web.HttpUtility.UrlEncode(arrbet);
                var userdata = $"{order.BetContent}";
                var userdata_encoded = System.Web.HttpUtility.UrlEncode(userdata);
                
                postData.Append($"&arrbet={arrbet_encoded}");
                postData.Append($"&grouplabel=");
                postData.Append($"&userdata={userdata_encoded}");
                postData.Append($"&token={_token}");
                postData.Append($"&timestamp={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
                
                // 获取当前URL的域名（因为通宝域名会变化）
                var currentUrl = _webView.CoreWebView2.Source;
                var baseUrl = new Uri(currentUrl).GetLeftPart(UriPartial.Authority);
                
                // 发送POST请求（参考F5BotV2 Line 408-420）
                var url = $"{baseUrl}/frcomgame/createmainorder";
                var content = new StringContent(postData.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
                
                _logCallback($"📤 发送投注请求: {url}");
                
                var response = await _httpClient.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();
                
                _logCallback($"📥 投注响应: {responseText.Substring(0, Math.Min(100, responseText.Length))}...");
                
                // 解析响应（参考F5BotV2 Line 430-441）
                var json = JObject.Parse(responseText);
                var succeed = json["status"]?.Value<bool>() ?? false;
                
                if (succeed)
                {
                    var orderId = json["BettingNumber"]?.ToString() ?? $"TB{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                    _logCallback($"✅ 投注成功: {orderId}");
                    return (true, orderId);
                }
                else
                {
                    var msg = json["msg"]?.ToString() ?? "未知错误";
                    _logCallback($"❌ 投注失败: {msg}");
                    return (false, "");
                }
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 投注异常: {ex.Message}");
                return (false, "");
            }
        }
        
        /// <summary>
        /// 处理拦截到的响应
        /// 参考 F5BotV2 的 ChromeBroser_ResponseComplete 方法
        /// </summary>
        public void HandleResponse(ResponseEventArgs response)
        {
            try
            {
                // 1. 拦截 gettodaywinlost - 获取 sid, uuid, token
                // 参考 F5BotV2 Line 96-102
                if (response.Url.Contains("/gettodaywinlost"))
                {
                    if (!string.IsNullOrEmpty(response.PostData))
                    {
                        // uuid=10014139&sid=9cbc377084ec37b28bc1d1d64a55210d0034174&token=640006705068482b6ca1b089c29a8eb1&timestamp=1744376513
                        _token = Regex.Match(response.PostData, @"token=([^&]+)").Groups[1].Value;
                        _uuid = Regex.Match(response.PostData, @"uuid=([^&]+)").Groups[1].Value;
                        _sid = Regex.Match(response.PostData, @"sid=([^&]+)").Groups[1].Value;
                        
                        if (!string.IsNullOrEmpty(_sid) && !string.IsNullOrEmpty(_uuid))
                        {
                            _logCallback($"✅ 拦截到登录参数 - UUID: {_uuid}, Token: {_token.Substring(0, 10)}...");
                        }
                    }
                    
                    // 解析响应中的余额
                    if (!string.IsNullOrEmpty(response.Context))
                    {
                        try
                        {
                            var json = JObject.Parse(response.Context);
                            var balance = json["balance"]?.Value<decimal>() ?? 0;
                            if (balance > 0)
                            {
                                _currentBalance = balance;
                                _logCallback($"💰 余额更新: {balance}");
                            }
                        }
                        catch { }
                    }
                }
                
                // 2. 拦截 getcommongroupodds - 获取盘口类型（A/B/C/D）
                // 参考 F5BotV2 Line 103-107
                else if (response.Url.Contains("/getcommongroupodds"))
                {
                    if (!string.IsNullOrEmpty(response.PostData))
                    {
                        // uuid=10014139&sid=ba4b32d0d4b5c0f66c3dca90234611540034124&groupnames=qwlm&pan=A&roomeng=twbingo
                        var region = Regex.Match(response.PostData, @"pan=([^&]+)").Groups[1].Value;
                        if (!string.IsNullOrEmpty(region))
                        {
                            _region = region;
                            _logCallback($"📊 盘口类型: {_region}");
                        }
                    }
                }
                
                // 3. 拦截投注响应
                else if (response.Url.Contains("/createmainorder"))
                {
                    if (!string.IsNullOrEmpty(response.Context))
                    {
                        try
                        {
                            var json = JObject.Parse(response.Context);
                            var succeed = json["status"]?.Value<bool>() ?? false;
                            var msg = json["msg"]?.ToString() ?? "";
                            
                            if (succeed)
                            {
                                var bettingNumber = json["BettingNumber"]?.ToString() ?? "";
                                _logCallback($"✅ 投注成功: {bettingNumber} - {msg}");
                            }
                            else
                            {
                                _logCallback($"❌ 投注失败: {msg}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logCallback($"⚠️ 解析投注响应失败: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 响应处理失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 根据投注内容获取对应的ID
        /// 注意：实际ID需要从赔率接口中获取
        /// 这里提供一个简化的映射
        /// </summary>
        private int GetBetId(string betContent)
        {
            // 这个映射需要根据实际的赔率接口返回的数据来调整
            // F5BotV2 中是从 _Odds.GetOdds() 获取的
            return betContent.ToLower() switch
            {
                "大" => 1,
                "小" => 2,
                "单" => 3,
                "双" => 4,
                "大单" => 5,
                "大双" => 6,
                "小单" => 7,
                "小双" => 8,
                "极大" => 9,
                "极小" => 10,
                _ => 1 // 默认值
            };
        }
    }
}

