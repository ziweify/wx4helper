using BaiShengVx3Plus.Shared.Models;
using BsBrowserClient.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
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
        private string _region = "C";  // A,B,C,D盘类型
        private decimal _currentBalance = 0;
        private string _baseUrl = "";  // 缓存的base URL
        
        // 赔率ID映射表：key="平一大", value="5370"
        private readonly Dictionary<string, string> _oddsMap = new Dictionary<string, string>();
        
        // 赔率值映射表：key="平一大", value=1.97f （实际赔率）
        private readonly Dictionary<string, float> _oddsValues = new Dictionary<string, float>();
        
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
                
                // 🔥 先检查是否在登录页，如果不是则尝试导航到登录页
                var checkLoginPageScript = @"
                    (function() {
                        try {
                            // 检查是否有登录表单
                            const usernameInput = document.querySelector('input[name=""username""]') ||
                                                  document.querySelector('input[type=""text""]');
                            const passwordInput = document.querySelector('input[name=""password""]') ||
                                                  document.querySelector('input[type=""password""]');
                            
                            if (usernameInput && passwordInput) {
                                return { isLoginPage: true };
                            }
                            
                            // 没有登录表单，尝试找到登录按钮或链接
                            const loginBtn = document.querySelector('a[href*=""login""]') ||
                                           document.querySelector('button:contains(""登录"")') ||
                                           document.querySelector('[class*=""login""]') ||
                                           Array.from(document.querySelectorAll('a')).find(a => a.textContent.includes('登录'));
                            
                            if (loginBtn) {
                                loginBtn.click();
                                return { isLoginPage: false, clickedLogin: true };
                            }
                            
                            return { isLoginPage: false, clickedLogin: false };
                        } catch (error) {
                            return { isLoginPage: false, error: error.message };
                        }
                    })();
                ";
                
                var checkResult = await _webView.CoreWebView2.ExecuteScriptAsync(checkLoginPageScript);
                var checkJson = JObject.Parse(checkResult);
                var isLoginPage = checkJson["isLoginPage"]?.Value<bool>() ?? false;
                
                // 如果不在登录页，且点击了登录按钮，等待页面跳转
                if (!isLoginPage && (checkJson["clickedLogin"]?.Value<bool>() ?? false))
                {
                    _logCallback("🔄 已点击登录按钮，等待跳转到登录页...");
                    await Task.Delay(1500);  // 等待SPA路由切换
                }
                
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
                                return {{ success: false, message: '当前页面不是登录页，请手动点击【登录】按钮' }};
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
                        // 🔥 缓存base URL（从WebView2获取，避免在投注时跨线程访问）
                        var currentUrl = _webView.CoreWebView2?.Source ?? "";
                        if (!string.IsNullOrEmpty(currentUrl))
                        {
                            _baseUrl = new Uri(currentUrl).GetLeftPart(UriPartial.Authority);
                        }
                        
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
        public async Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BaiShengVx3Plus.Shared.Models.BetStandardOrderList orders)
        {
            try
            {
                if (string.IsNullOrEmpty(_sid) || string.IsNullOrEmpty(_uuid) || string.IsNullOrEmpty(_token))
                {
                    _logCallback("❌ 未登录，无法下注");
                    return (false, "", "#未登录，无法下注");  // 🔥 #前缀表示客户端校验错误
                }
                
                var issueId = orders.Count > 0 ? orders[0].IssueId : 0;
                var totalAmount = orders.GetTotalAmount();
                _logCallback($"🎲 开始投注: 期号{issueId} 共{orders.Count}项 {totalAmount}元");
                
                // 🔥 使用标准化订单列表，不需要再解析文本
                var betList = new List<object>();
                var userdataList = new List<string>();
                
                foreach (var order in orders)
                {
                    // 🔥 直接从 BetPlayEnum 映射到玩法名称
                    var playType = order.Play switch
                    {
                        BetPlayEnum.大 => "大",
                        BetPlayEnum.小 => "小",
                        BetPlayEnum.单 => "单",
                        BetPlayEnum.双 => "双",
                        BetPlayEnum.尾大 => "尾大",
                        BetPlayEnum.尾小 => "尾小",
                        BetPlayEnum.合单 => "合单",
                        BetPlayEnum.合双 => "合双",
                        BetPlayEnum.龙 => "龙",
                        BetPlayEnum.虎 => "虎",
                        _ => "大"
                    };
                    
                    // 🔥 特殊处理：龙虎的 carName 为空（参照 F5BotV2）
                    string carName;
                    if (order.Play == BetPlayEnum.龙 || order.Play == BetPlayEnum.虎)
                    {
                        carName = "";  // 🔥 龙虎没有车号前缀（F5BotV2 中 carName 为空字符串）
                    }
                    else
                    {
                        // 其他玩法正常映射车号
                        carName = order.Car switch
                        {
                            CarNumEnum.P1 => "平一",
                            CarNumEnum.P2 => "平二",
                            CarNumEnum.P3 => "平三",
                            CarNumEnum.P4 => "平四",
                            CarNumEnum.P5 => "平五",
                            CarNumEnum.P总 => "和值",  // 🔥 和值（大小单双尾大尾小）
                            _ => "平一"
                        };
                    }
                    
                    var money = order.MoneySum;
                    
                    // 🔥 从赔率映射表中获取ID
                    var oddsKey = $"{carName}{playType}";  // 如："平一大"
                    var betIdStr = _oddsMap.ContainsKey(oddsKey) ? _oddsMap[oddsKey] : "0";
                    
                    // 输出调试信息
                    _logCallback($"   🔍 查找ID: {oddsKey} → ID={betIdStr}, 映射表数量={_oddsMap.Count}");
                    
                    var betId = int.TryParse(betIdStr, out var id) ? id : 0;
                    betList.Add(new { id = betId, money = money });
                    userdataList.Add(oddsKey);
                    
                    _logCallback($"   解析:{oddsKey} 金额:{money} ID:{betId}");
                }
                
                if (betList.Count == 0)
                {
                    _logCallback("❌ 没有有效的投注项");
                    return (false, "", "#没有有效的投注项");  // 🔥 #前缀表示客户端校验错误
                }
                
                // 构造POST数据（完全按照F5BotV2 Line 358-391的方式）
                // 🔥 手动编码，手动拼接字符串，不让HttpClient自动处理！
                
                var arrbet = JsonConvert.SerializeObject(betList);
                var arrbet_encoded = WebUtility.UrlEncode(arrbet);
                
                var userdata = string.Join(" ", userdataList) + " ";
                var userdata_encoded = WebUtility.UrlEncode(userdata);
                
                _logCallback($"📦 投注包:arrbet={arrbet}, userdata={userdata.Trim()}");
                _logCallback($"   uuid={_uuid}, sid={_sid.Substring(0, Math.Min(10, _sid.Length))}..., region={_region}");
                
                // 🔥 完全按照F5BotV2的方式拼接POST字符串
                var postData = new StringBuilder();
                postData.Append($"uuid={_uuid}");
                postData.Append($"&sid={_sid}");
                postData.Append($"&roomeng=twbingo");
                postData.Append($"&pan={_region}");
                postData.Append($"&shuitype=0");
                postData.Append($"&arrbet={arrbet_encoded}");
                postData.Append($"&grouplabel=");
                postData.Append($"&userdata={userdata_encoded}");
                postData.Append($"&kuaiyidata=");
                postData.Append($"&token={_token}");
                postData.Append($"&timestamp={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
                
                var fullPostData = postData.ToString();
                
                // 🔥 使用缓存的base URL（避免跨线程访问WebView2）
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    _logCallback("❌ 未获取到base URL，可能未登录");
                    return (false, "", "#未获取到base URL，可能未登录");  // 🔥 #前缀表示客户端校验错误
                }
                
                // 发送POST请求（参考F5BotV2 Line 408-420）
                var url = $"{_baseUrl}/frcomgame/createmainorder";
                
                _logCallback($"📤 发送投注请求: {url}");
                _logCallback($"📋 POST数据（完整）:");
                _logCallback($"   {fullPostData}");
                
                // 🔥 使用ByteArrayContent直接发送字节，避免HttpClient的任何自动处理
                var bytes = Encoding.UTF8.GetBytes(fullPostData);
                var content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                
                var response = await _httpClient.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();
                
                _logCallback($"📥 投注响应（完整）:");
                _logCallback($"   {responseText}");
                
                // 🔥 解析响应（参考F5BotV2 Line 430-441）
                var json = JObject.Parse(responseText);
                var succeed = json["status"]?.Value<bool>() ?? false;
                
                if (succeed)
                {
                    var orderId = json["BettingNumber"]?.ToString() ?? $"TB{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                    _logCallback($"✅ 投注成功: {orderId}");
                    return (true, orderId, responseText);  // 🔥 返回完整响应
                }
                else
                {
                    var msg = json["msg"]?.ToString() ?? "未知错误";
                    var errcode = json["errcode"]?.ToString() ?? "";
                    _logCallback($"❌ 投注失败: {msg} (errcode={errcode})");
                    return (false, "", responseText);  // 🔥 返回完整响应（包含错误信息）
                }
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 投注异常: {ex.Message}");
                _logCallback($"   堆栈: {ex.StackTrace}");
                return (false, "", $"#投注异常: {ex.Message}");  // 🔥 #前缀表示客户端异常
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
                            // 🔥 同时设置 _baseUrl（手动登录时也能获取到）
                            if (string.IsNullOrEmpty(_baseUrl) && !string.IsNullOrEmpty(response.Url))
                            {
                                try
                                {
                                    _baseUrl = new Uri(response.Url).GetLeftPart(UriPartial.Authority);
                                    _logCallback($"✅ Base URL 已设置: {_baseUrl}");
                                }
                                catch { }
                            }
                            
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
                
                // 2. 拦截 getcommongroupodds - 获取盘口类型（A/B/C/D）和赔率ID
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
                    
                    // 解析响应数据，获取赔率ID和赔率值（参考 F5BotV2）
                    if (!string.IsNullOrEmpty(response.Context))
                    {
                        try
                        {
                            var json = JObject.Parse(response.Context);
                            var msg = json["msg"]; // 🔥 正确的字段是msg，不是datas！
                            if (msg != null && msg.Type == JTokenType.Array)
                            {
                                _oddsMap.Clear();
                                _oddsValues.Clear();  // 🔥 清空赔率值
                                int count = 0;
                                
                                // ResultID从5370开始，对应"平一大"
                                // 5370=平一大, 5371=平一小, 5372=平一单, 5373=平一双...
                                var resultArray = msg.ToArray();
                                foreach (var item in resultArray)
                                {
                                    var resultId = item["ResultID"]?.ToString(); // 🔥 字段是ResultID
                                    var odds = item["Odds"]?.Value<float>() ?? 1.97f; // 🔥 获取实际赔率
                                    
                                    if (!string.IsNullOrEmpty(resultId))
                                    {
                                        // 根据ResultID推算name
                                        var id = int.Parse(resultId);
                                        string name = GetNameFromResultId(id);
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            _oddsMap[name] = resultId;
                                            _oddsValues[name] = odds;  // 🔥 存储实际赔率
                                            count++;
                                        }
                                    }
                                }
                                _logCallback($"✅ 赔率数据已更新，共{_oddsMap.Count}项（ID+实际赔率值）");
                            }
                            else
                            {
                                _logCallback($"⚠️ 响应中没有找到msg数组");
                                _logCallback($"   响应内容: {response.Context.Substring(0, Math.Min(200, response.Context.Length))}...");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logCallback($"⚠️ 解析赔率数据失败: {ex.Message}");
                            _logCallback($"   响应内容: {response.Context.Substring(0, Math.Min(200, response.Context.Length))}...");
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
        /// 根据ResultID推算名称
        /// 🔥 完全参照 F5BotV2/BetSite/HongHai/TongBaoOdds.cs
        /// 规律：
        /// - 5364-5369: 前五和值（6个玩法：大小单双尾大尾小）
        /// - 5370-5377: 平码一（8个玩法：大小单双尾大尾小合单合双）
        /// - 5378-5385: 平码二（8个玩法）
        /// - 5386-5393: 平码三（8个玩法）
        /// - 5394-5401: 平码四（8个玩法）
        /// - 5402-5409: 特码/平五（8个玩法）
        /// </summary>
        private string GetNameFromResultId(int resultId)
        {
            // 🔥 龙虎: 5418-5419（参照 F5BotV2，carName 为空字符串）
            if (resultId == 5418)
            {
                return "龙";  // 🔥 龙虎没有车号前缀
            }
            else if (resultId == 5419)
            {
                return "虎";  // 🔥 龙虎没有车号前缀
            }
            
            // 🔥 前五和值（总和）: 5364-5369（参照 F5BotV2）
            if (resultId >= 5364 && resultId <= 5369)
            {
                int playIndex = resultId - 5364;
                string playName = playIndex switch
                {
                    0 => "大",      // 5364
                    1 => "小",      // 5365
                    2 => "单",      // 5366
                    3 => "双",      // 5367
                    4 => "尾大",    // 5368
                    5 => "尾小",    // 5369
                    _ => ""
                };
                return $"和值{playName}";  // 🔥 修正：和值而不是总和
            }
            
            // 🔥 平码一到平五: 5370-5409
            if (resultId >= 5370 && resultId <= 5409)
            {
                int offset = resultId - 5370;
                int carIndex = offset / 8;  // 🔥 每个号码8个玩法
                int playIndex = offset % 8;
                
                string carName = carIndex switch
                {
                    0 => "平一",    // 5370-5377
                    1 => "平二",    // 5378-5385
                    2 => "平三",    // 5386-5393
                    3 => "平四",    // 5394-5401
                    4 => "平五",    // 5402-5409
                    _ => ""
                };
                
                string playName = playIndex switch
                {
                    0 => "大",      // +0
                    1 => "小",      // +1
                    2 => "单",      // +2
                    3 => "双",      // +3
                    4 => "尾大",    // +4
                    5 => "尾小",    // +5
                    6 => "合单",    // +6
                    7 => "合双",    // +7
                    _ => ""
                };
                
                return $"{carName}{playName}";
            }
            
            return "";
        }
        
        /// <summary>
        /// 根据投注内容获取对应的ID
        /// 从拦截的赔率数据中查找
        /// </summary>
        private string GetBetId(string number, string playType)
        {
            // 组合成赔率名称，如："平一大"（网站显示用）
            // number: "1" → "平一", "2" → "平二", ...
            var carName = number switch
            {
                "1" => "平一",
                "2" => "平二",
                "3" => "平三",
                "4" => "平四",
                "5" => "平五",
                "6" or "总" => "和值",  // 🔥 修正：和值而不是总和
                _ => "平一"
            };
            
            var oddsName = $"{carName}{playType}"; // 如："平一大"
            
            if (_oddsMap.TryGetValue(oddsName, out var id))
            {
                return id;
            }
            
            _logCallback($"⚠️ 未找到赔率ID: {oddsName}，使用默认值0");
            return "0";
        }
        
        /// <summary>
        /// 获取赔率列表（用于赔率显示窗口）
        /// </summary>
        public List<BsBrowserClient.Models.OddsInfo> GetOddsList()
        {
            var oddsList = new List<BsBrowserClient.Models.OddsInfo>();
            
            if (_oddsMap.Count == 0)
            {
                _logCallback("⚠️ 赔率数据尚未加载");
                return oddsList;
            }
            
            // 遍历赔率映射表，生成 OddsInfo 列表
            foreach (var kvp in _oddsMap)
            {
                var name = kvp.Key;      // 如："平一大"
                var oddsId = kvp.Value;  // 如："5370"
                
                // 解析名称，提取车号和玩法
                if (!TryParseName(name, out var car, out var play))
                {
                    continue;  // 跳过无法解析的项
                }
                
                // 🔥 获取实际赔率值（如果没有则使用默认值1.97）
                var odds = _oddsValues.ContainsKey(name) ? _oddsValues[name] : 1.97f;
                
                oddsList.Add(new BsBrowserClient.Models.OddsInfo(car, play, name, odds, oddsId));
            }
            
            return oddsList;
        }
        
        /// <summary>
        /// 解析名称，提取车号和玩法
        /// </summary>
        private bool TryParseName(string name, out CarNumEnum car, out BetPlayEnum play)
        {
            car = CarNumEnum.P1;
            play = BetPlayEnum.大;
            
            // 🔥 特殊处理：龙虎没有车号前缀（F5BotV2 中龙虎的 carName 为空字符串）
            // 必须放在最前面，因为龙虎不匹配任何 StartsWith 条件！
            if (name == "龙")
            {
                car = CarNumEnum.P总;
                play = BetPlayEnum.龙;
                return true;
            }
            else if (name == "虎")
            {
                car = CarNumEnum.P总;
                play = BetPlayEnum.虎;
                return true;
            }
            
            // 解析车号
            if (name.StartsWith("平一"))
            {
                car = CarNumEnum.P1;
            }
            else if (name.StartsWith("平二"))
            {
                car = CarNumEnum.P2;
            }
            else if (name.StartsWith("平三"))
            {
                car = CarNumEnum.P3;
            }
            else if (name.StartsWith("平四"))
            {
                car = CarNumEnum.P4;
            }
            else if (name.StartsWith("平五"))
            {
                car = CarNumEnum.P5;
            }
            else if (name.StartsWith("和值"))
            {
                car = CarNumEnum.P总;
            }
            else
            {
                return false;  // 无法识别车号
            }
            
            // 解析玩法（从后往前匹配，因为车号长度不固定）
            if (name.EndsWith("大"))
            {
                play = BetPlayEnum.大;
            }
            else if (name.EndsWith("小"))
            {
                play = BetPlayEnum.小;
            }
            else if (name.EndsWith("单"))
            {
                play = BetPlayEnum.单;
            }
            else if (name.EndsWith("双"))
            {
                play = BetPlayEnum.双;
            }
            else if (name.EndsWith("尾大"))
            {
                play = BetPlayEnum.尾大;
            }
            else if (name.EndsWith("尾小"))
            {
                play = BetPlayEnum.尾小;
            }
            else if (name.EndsWith("合单"))
            {
                play = BetPlayEnum.合单;
            }
            else if (name.EndsWith("合双"))
            {
                play = BetPlayEnum.合双;
            }
            else
            {
                return false;  // 无法识别玩法
            }
            
            return true;
        }
    }
}

