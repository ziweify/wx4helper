using Unit.Shared.Models;
using Unit.Shared.Helpers;  // 🔥 引入共享库（ModernHttpHelper, BinggoTimeHelper）
using zhaocaimao.Services.AutoBet.Browser.Models;
using zhaocaimao.Services.AutoBet.Browser.Services;
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
using BrowserOddsInfo = zhaocaimao.Services.AutoBet.Browser.Models.OddsInfo;
using BrowserResponseEventArgs = zhaocaimao.Services.AutoBet.Browser.Services.ResponseEventArgs;

namespace zhaocaimao.Services.AutoBet.Browser.PlatformScripts
{
    /// <summary>
    /// 黄金海岸 平台脚本 - 基于通宝脚本复制
    /// </summary>
    public class HuangJinHaiAnScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ModernHttpHelper _httpHelper;  // 🔥 添加 ModernHttpHelper
        
        // 关键参数（从拦截中获取）
        private string _sid = "";
        private string _uuid = "";
        private string _token = "";
        private string _region = "C";  // A,B,C,D盘类型
        private decimal _currentBalance = 0;
        private string _baseUrl = "";  // 缓存的base URL
        
        // 🔥 动态API域名（从getmoneyinfo请求中自动提取和更新）
        private string DoMainApi = "";  // 投注使用的API域名，如 https://api.fr.win2000.vip
        
        // 赔率ID映射表：key="平一大", value="5370"
        private readonly Dictionary<string, string> _oddsMap = new Dictionary<string, string>();
        
        // 赔率值映射表：key="平一大", value=1.97f （实际赔率）
        private readonly Dictionary<string, float> _oddsValues = new Dictionary<string, float>();
        
        // 测试账号（来自F5BotV2注释）
        // 账号: wwww11
        // 密码: Aaa123
        
        public HuangJinHaiAnScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            _httpHelper = new ModernHttpHelper(_httpClient);  // 🔥 初始化 ModernHttpHelper
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
        }
        
        /// <summary>
        /// 登录 - 自动填充表单并尝试自动登录（黄金海岸平台）
        /// 根据页面结构：input.username 和 input.password
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                _logCallback($"🔐 开始登录黄金海岸: {username}");
                
                // 🔥 等待页面加载完成（Vue.js SPA需要时间）
                await Task.Delay(1000);
                
                // 🔥 黄金海岸平台登录脚本（使用 class="username" 和 class="password"）
                var script = $@"
                    (function() {{
                        try {{
                            // 🔥 黄金海岸平台：使用 class 选择器
                            const usernameInput = document.querySelector('input.username') ||
                                                  document.querySelector('input[placeholder*=""账号""]') ||
                                                  document.querySelector('input[type=""text""]');
                            
                            const passwordInput = document.querySelector('input.password') ||
                                                  document.querySelector('input[placeholder*=""密码""]') ||
                                                  document.querySelector('input[type=""password""]');
                            
                            if (!usernameInput || !passwordInput) {{
                                return {{ success: false, message: '未找到登录表单元素' }};
                            }}
                            
                            // 🔥 填充账号密码
                            usernameInput.value = '{username}';
                            passwordInput.value = '{password}';
                            
                            // 🔥 触发多种事件，确保Vue.js能检测到值变化
                            usernameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            usernameInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            usernameInput.dispatchEvent(new KeyboardEvent('keyup', {{ bubbles: true }}));
                            
                            passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            passwordInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            passwordInput.dispatchEvent(new KeyboardEvent('keyup', {{ bubbles: true }}));
                            
                            // 🔥 尝试查找并点击登录按钮（如果有验证码，需要手动输入）
                            // 注意：:contains() 不是有效的 CSS 选择器，需要使用 JavaScript 查找
                            const loginButton = document.querySelector('button[type=""submit""]') ||
                                              Array.from(document.querySelectorAll('button')).find(btn => 
                                                  btn.textContent.includes('登录') || 
                                                  btn.textContent.includes('登 录') ||
                                                  btn.textContent.includes('LOGIN')
                                              ) ||
                                              document.querySelector('a[href*=""login""]');
                            
                            let clickedLogin = false;
                            if (loginButton) {{
                                // 检查是否有验证码输入框
                                const captchaInput = document.querySelector('input[placeholder*=""验证码""]') ||
                                                   document.querySelector('input[type=""text""][placeholder*=""验证""]');
                                
                                if (captchaInput && captchaInput.value === '') {{
                                    // 有验证码且未填写，只填充账号密码，不自动点击
                                    return {{ 
                                        success: true, 
                                        message: '账号密码已填充，请手动输入验证码并点击登录',
                                        hasCaptcha: true,
                                        clickedLogin: false
                                    }};
                                }} else {{
                                    // 没有验证码或已填写，尝试自动点击登录
                                    loginButton.click();
                                    clickedLogin = true;
                                    return {{ 
                                        success: true, 
                                        message: '账号密码已填充，已自动点击登录按钮',
                                        hasCaptcha: false,
                                        clickedLogin: true
                                    }};
                                }}
                            }} else {{
                                return {{ 
                                    success: true, 
                                    message: '账号密码已填充，请手动点击登录按钮',
                                    hasCaptcha: true,
                                    clickedLogin: false
                                }};
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
                var hasCaptcha = json["hasCaptcha"]?.Value<bool>() ?? true;
                var clickedLogin = json["clickedLogin"]?.Value<bool>() ?? false;
                
                _logCallback(success ? $"✅ {message}" : $"❌ {message}");
                
                if (success)
                {
                    // 🔥 如果自动点击了登录按钮，等待登录完成
                    if (clickedLogin)
                    {
                        _logCallback("⏳ 已自动点击登录，等待登录完成...");
                        await Task.Delay(2000);  // 等待登录请求完成
                    }
                    else
                    {
                        // 有验证码，等待用户手动输入并点击登录
                        _logCallback("⏳ 等待用户输入验证码并点击登录...");
                    }
                    
                    // 🔥 等待拦截到登录成功的参数（sid, uuid, token）
                    // 或者通过URL变化判断登录成功（从 /#/ 变为其他路径）
                    var waitCount = 0;
                    var maxWait = hasCaptcha ? 600 : 300;  // 有验证码时等待60秒，无验证码30秒
                    
                    while (waitCount < maxWait)
                    {
                        await Task.Delay(100);
                        waitCount++;
                        
                        // 🔥 检查是否已登录成功（通过URL变化或拦截到参数）
                        var currentUrl = _webView.CoreWebView2?.Source ?? "";
                        
                        // 如果URL不再是登录页，可能已登录成功
                        if (!currentUrl.Contains("/#/") && !currentUrl.Contains("login"))
                        {
                            _logCallback($"✅ 检测到页面跳转，可能已登录成功: {currentUrl}");
                            // 继续等待拦截参数
                        }
                        
                        // 如果已拦截到登录参数，登录成功
                        if (!string.IsNullOrEmpty(_sid) && !string.IsNullOrEmpty(_uuid))
                        {
                            // 🔥 缓存base URL
                            if (string.IsNullOrEmpty(_baseUrl) && !string.IsNullOrEmpty(currentUrl))
                            {
                                try
                                {
                                    _baseUrl = new Uri(currentUrl).GetLeftPart(UriPartial.Authority);
                                }
                                catch { }
                            }
                            
                            _logCallback($"✅ 登录成功！UUID: {_uuid}, SID: {_sid.Substring(0, Math.Min(10, _sid.Length))}...");
                            
                            // 🔥 登录成功后，主动获取余额以更新状态（参考通宝脚本）
                            try
                            {
                                await Task.Delay(500); // 等待一下，确保登录完全完成
                                var balance = await GetBalanceAsync();
                                if (balance >= 0)
                                {
                                    _logCallback($"✅ 登录成功，余额: {balance}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logCallback($"⚠️ 获取余额失败（不影响登录）: {ex.Message}");
                            }
                            
                            return true;
                        }
                    }
                    
                    // 🔥 超时检查：如果URL已变化且不在登录页，可能已登录（但未拦截到参数）
                    var finalUrl = _webView.CoreWebView2?.Source ?? "";
                    if (!finalUrl.Contains("/#/") && !finalUrl.Contains("login") && !string.IsNullOrEmpty(finalUrl))
                    {
                        _logCallback($"⚠️ 登录可能已成功（URL已变化），但未拦截到登录参数");
                        _logCallback($"   当前URL: {finalUrl}");
                        // 🔥 尝试设置base URL
                        try
                        {
                            _baseUrl = new Uri(finalUrl).GetLeftPart(UriPartial.Authority);
                            _logCallback($"✅ 已设置Base URL: {_baseUrl}");
                            // 返回true，允许继续尝试（投注时会再次验证）
                            return true;
                        }
                        catch
                        {
                            _logCallback("❌ 登录超时或失败");
                            return false;
                        }
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
                _logCallback($"   堆栈: {ex.StackTrace}");
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
                // 🔥 如果未拦截到登录参数，但URL已变化（可能已登录），尝试从页面获取
                if (string.IsNullOrEmpty(_sid) || string.IsNullOrEmpty(_uuid))
                {
                    // 检查URL是否已跳转（不在登录页）
                    var currentUrl = _webView.CoreWebView2?.Source ?? "";
                    if (!currentUrl.Contains("/#/") && !currentUrl.Contains("login") && !string.IsNullOrEmpty(currentUrl))
                    {
                        _logCallback("⚠️ 未拦截到登录参数，但URL已变化，尝试从页面获取余额...");
                        // 继续执行，尝试从页面读取余额
                    }
                    else
                    {
                        _logCallback("❌ 未登录，无法获取余额");
                        return -1;
                    }
                }
                
                _logCallback("💰 获取余额...");
                
                // 黄金海岸的余额通常会在拦截的响应中
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
        public async Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(Unit.Shared.Models.BetStandardOrderList orders)
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
                
                // 🔥 优先使用DoMainApi（从getmoneyinfo动态获取），fallback到_baseUrl
                var apiDomain = !string.IsNullOrEmpty(DoMainApi) ? DoMainApi : _baseUrl;
                
                if (string.IsNullOrEmpty(apiDomain))
                {
                    _logCallback("❌ 未获取到API域名，可能未登录");
                    return (false, "", "#未获取到API域名，可能未登录");  // 🔥 #前缀表示客户端校验错误
                }
                
                // 发送POST请求（使用DoMainApi动态域名）
                var url = $"{apiDomain}/frcomgame/createmainorder";
                
                _logCallback($"🌐 投注API域名: {apiDomain}");
                
                _logCallback($"📤 发送投注请求: {url}");
                _logCallback($"📋 POST数据（完整）:");
                _logCallback($"   {fullPostData}");

                // 🎯 计算封盘时间（开奖时间 - 20秒）
                var openTime = BinggoTimeHelper.GetIssueOpenTime(issueId);
                var sealTime = openTime.AddSeconds(-20);  // 封盘时间
                _logCallback($"⏰ 期号{issueId} 开奖时间: {openTime:HH:mm:ss}, 封盘时间: {sealTime:HH:mm:ss}");
                
                // 🔥 重试机制：直到成功或超过封盘时间
                int retryCount = 0;
                const int maxRetries = 100;  // 最大重试次数（防止死循环）
                
                while (retryCount < maxRetries)
                {
                    var now = DateTime.Now;
                    
                    // 🔥 检查是否超过封盘时间
                    if (now > sealTime)
                    {
                        _logCallback($"⏰ 已超过封盘时间({sealTime:HH:mm:ss})，停止投注");
                        return (false, "", $"#已超过封盘时间，无法投注");
                    }
                    
                    retryCount++;
                    var remainingSeconds = (int)(sealTime - now).TotalSeconds;
                    _logCallback($"🔄 第{retryCount}次投注尝试 (距封盘还有{remainingSeconds}秒)");
                    
                    // 🎯 发送投注请求（2秒超时）
                    var result = await _httpHelper.PostAsync(new HttpRequestItem
                    {
                        Url = url,
                        PostData = fullPostData,
                        ContentType = "application/x-www-form-urlencoded",
                        Timeout = 2
                    });
                    
                    // ✅ 情况1：请求成功返回
                    if (result.Success)
                    {
                        var responseText = result.Html;
                        _logCallback($"📥 投注响应: {responseText.Substring(0, Math.Min(100, responseText.Length))}...");
                        
                        try
                        {
                            var json = JObject.Parse(responseText);
                            var succeed = json["status"]?.Value<bool>() ?? false;
                            
                            if (succeed)
                            {
                                var orderId = json["BettingNumber"]?.ToString() ?? $"HJHA{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                                _logCallback($"✅ 投注成功: {orderId} (第{retryCount}次尝试)");
                                return (true, orderId, responseText);
                            }
                            else
                            {
                                var msg = json["msg"]?.ToString() ?? "未知错误";
                                var errcode = json["errcode"]?.ToString() ?? "";
                                _logCallback($"❌ 投注失败: {msg} (errcode={errcode})");
                                
                                // 如果是明确的业务错误（如余额不足、已封盘等），不再重试
                                if (msg.Contains("余额不足") || msg.Contains("封盘") || msg.Contains("已结束"))
                                {
                                    return (false, "", responseText);
                                }
                                
                                // 其他错误继续重试
                                _logCallback($"⏳ 等待1秒后重试...");
                                await Task.Delay(1000);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logCallback($"⚠️ 解析响应失败: {ex.Message}，继续重试");
                            await Task.Delay(1000);
                            continue;
                        }
                    }
                    
                    // ⏰ 情况2：请求超时（2秒无响应）
                    _logCallback($"⏰ 投注请求超时，开始验证订单...");
                    
                    // 🔍 查询未结算订单，检查是否已投注成功
                    try
                    {
                        _logCallback($"🔍 查询未结算订单 (金额:{totalAmount}元)...");
                        var (success, orderList, _, _, errorMsg) = await GetLotMainOrderInfosAsync(
                            state: 0,           // 未结算
                            pageNum: 1,
                            pageCount: 20,
                            timeout: 3          // 查询订单超时3秒
                        );
                        
                        if (success && orderList != null && orderList.Count > 0)
                        {
                            _logCallback($"📋 查询到 {orderList.Count} 条未结算订单，开始匹配...");
                            
                            // 🔍 遍历订单，查找匹配的金额
                            foreach (var order in orderList)
                            {
                                var orderAmount = order["amount"]?.Value<int>() ?? 0;  // 订单总金额（整数）
                                var orderExpect = order["expect"]?.ToString() ?? "";    // 订单期号
                                var orderUserData = order["userdata"]?.ToString() ?? ""; // 订单内容
                                
                                // 🎯 匹配条件：金额相同 && 期号相同
                                if (orderAmount == (int)totalAmount && orderExpect == issueId.ToString())
                                {
                                    var orderId = order["orderid"]?.ToString() ?? $"HJHA{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                                    _logCallback($"✅ 找到匹配订单！金额:{orderAmount}元, 期号:{orderExpect}, 订单号:{orderId}");
                                    _logCallback($"   订单内容: {orderUserData.Trim()}");
                                    _logCallback($"🎉 投注已成功（通过订单验证确认，第{retryCount}次尝试）");
                                    
                                    // 返回成功（订单已存在）
                                    return (true, orderId, $"{{\"status\":true,\"BettingNumber\":\"{orderId}\",\"verified\":true}}");
                                }
                            }
                            
                            _logCallback($"⚠️ 未找到匹配的订单 (期号:{issueId}, 金额:{totalAmount}元)");
                        }
                        else
                        {
                            _logCallback($"⚠️ 查询订单失败或无订单: {errorMsg}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logCallback($"⚠️ 查询订单异常: {ex.Message}");
                    }
                    
                    // 没有找到匹配订单，继续重试投注
                    _logCallback($"⏳ 未找到订单，等待1秒后继续投注...");
                    await Task.Delay(1000);
                }
                
                // 超过最大重试次数
                _logCallback($"❌ 已达到最大重试次数({maxRetries})，投注失败");
                return (false, "", $"#投注失败：超过最大重试次数");
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 投注异常: {ex.Message}");
                _logCallback($"   堆栈: {ex.StackTrace}");
                return (false, "", $"#投注异常: {ex.Message}");  // 🔥 #前缀表示客户端异常
            }
        }
        
        /// <summary>
        /// 获取订单列表（未结算/已结算）
        /// 接口: /frclienthall/getlotmainorderinfos
        /// </summary>
        /// <param name="state">订单状态：0=未结算, 1=已结算</param>
        /// <param name="pageNum">页码（从1开始）</param>
        /// <param name="pageCount">每页数量</param>
        /// <param name="beginDate">开始日期（yyyyMMdd格式，如：20251214）</param>
        /// <param name="endDate">结束日期（yyyyMMdd格式，如：20251214）</param>
        /// <param name="timeout">超时时间（秒），默认2秒</param>
        /// <returns>(是否成功, 订单列表, 最大记录数, 最大页数, 错误消息)</returns>
        public async Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
            int state = 0, 
            int pageNum = 1, 
            int pageCount = 20,
            string? beginDate = null,
            string? endDate = null,
            int timeout = 2)
        {
            try
            {
                // 🔥 检查必要参数
                if (string.IsNullOrEmpty(_uuid) || string.IsNullOrEmpty(_sid))
                {
                    _logCallback("❌ 获取订单失败: 缺少 uuid 或 sid");
                    return (false, null, 0, 0, "缺少必要参数");
                }
                
                // 🔥 优先使用DoMainApi，fallback到_baseUrl
                var apiDomain = !string.IsNullOrEmpty(DoMainApi) ? DoMainApi : _baseUrl;
                
                if (string.IsNullOrEmpty(apiDomain))
                {
                    _logCallback("❌ 获取订单失败: API 域名未初始化");
                    return (false, null, 0, 0, "API域名未初始化");
                }
                
                // 🔥 使用当前日期（如果未指定）
                if (string.IsNullOrEmpty(beginDate))
                {
                    beginDate = DateTime.Now.ToString("yyyyMMdd");
                }
                if (string.IsNullOrEmpty(endDate))
                {
                    endDate = DateTime.Now.ToString("yyyyMMdd");
                }
                
                // 🔥 构建请求 URL
                string url = $"{apiDomain}/frclienthall/getlotmainorderinfos";
                
                // 🔥 构建 POST 参数
                string postData = $"uuid={_uuid}&sid={_sid}&state={state}&pagenum={pageNum}&pagecount={pageCount}&begindate={beginDate}&enddate={endDate}&roomeng=twbingo";
                
                _logCallback($"📤 获取订单列表: state={state}, page={pageNum}/{pageCount}, date={beginDate}~{endDate}, timeout={timeout}秒");
                
                // 🎯 使用 ModernHttpHelper（使用传入的超时参数）
                var result = await _httpHelper.PostAsync(new HttpRequestItem
                {
                    Url = url,
                    PostData = postData,
                    ContentType = "application/x-www-form-urlencoded",
                    Timeout = timeout  // 🔥 使用传入的超时参数
                });
                
                if (!result.Success)
                {
                    _logCallback($"❌ 获取订单请求失败: {result.ErrorMessage}");
                    return (false, null, 0, 0, result.ErrorMessage ?? "请求失败");
                }
                
                var responseText = result.Html;
                _logCallback($"📥 订单响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 🔥 解析响应
                var json = JObject.Parse(responseText);
                var status = json["status"]?.Value<bool>() ?? false;
                
                if (!status)
                {
                    var errcode = json["errcode"]?.Value<int>() ?? -1;
                    var msg = json["msg"]?.ToString() ?? "未知错误";
                    _logCallback($"❌ 获取订单失败: {msg} (errcode={errcode})");
                    return (status, null, 0, 0, msg);
                }
                
                // 🔥 提取订单数据
                var msgObj = json["msg"] as JObject;
                if (msgObj == null)
                {
                    _logCallback("✅ 获取订单成功: 但是msg为空, 无订单数据");
                    return (status, null, 0, 0, "无订单数据");
                }
                
                var maxRecordNum = msgObj["maxrecordnum"]?.Value<int>() ?? 0;
                var maxPageNum = msgObj["maxpagenum"]?.Value<int>() ?? 0;
                var dataArray = msgObj["data"] as JArray;
                
                if (dataArray == null || dataArray.Count == 0)
                {
                    _logCallback($"✅ 获取订单成功: 0条记录 (maxRecord={maxRecordNum}, maxPage={maxPageNum})");
                    return (status, new List<JObject>(), maxRecordNum, maxPageNum, "");
                }
                
                // 🔥 转换为 List<JObject>
                var orderList = new List<JObject>();
                foreach (var item in dataArray)
                {
                    if (item is JObject orderObj)
                    {
                        orderList.Add(orderObj);
                    }
                }
                
                _logCallback($"✅ 获取订单成功: {orderList.Count}条记录 (maxRecord={maxRecordNum}, maxPage={maxPageNum})");

                // 🔥 打印订单信息（用于调试）
                for (int i = 0; i < orderList.Count; i++)
                {
                    var order = orderList[i];
                    var orderId = order["orderid"]?.ToString() ?? "";
                    var expect = order["expect"]?.ToString() ?? ""; //期号
                    var amount = order["amount"]?.Value<int>() ?? 0; //金额 decimal 应该是 int没有小数点的
                    var userData = order["userdata"]?.ToString() ?? "";
                    var orderState = order["state"]?.Value<int>() ?? -1;
                    
                    _logCallback($"   [{i + 1}] {orderId} | 期号:{expect} | 金额:{amount}元 | 内容:{userData.Trim()} | 状态:{orderState}");
                }
                
                return (status, orderList, maxRecordNum, maxPageNum, "");
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 获取订单异常: {ex.Message}");
                _logCallback($"   堆栈: {ex.StackTrace}");
                return (false, null, 0, 0, $"异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理拦截到的响应
        /// 参考 F5BotV2 的 ChromeBroser_ResponseComplete 方法
        /// </summary>
        public void HandleResponse(BrowserResponseEventArgs response)
        {
            try
            {
                // 🔥 拦截 getmoneyinfo - 动态提取并更新API域名，同时解析余额
                // 参考通宝脚本：getmoneyinfo 用于更新金额
                if (response.Url.Contains("/getmoneyinfo"))
                {
                    try
                    {
                        var uri = new Uri(response.Url);
                        var currentDomain = $"{uri.Scheme}://{uri.Host}";
                        
                        // 如果域名和DoMainApi不一致，更新DoMainApi
                        if (currentDomain != DoMainApi)
                        {
                            var oldDomain = DoMainApi;
                            DoMainApi = currentDomain;
                            _logCallback($"🔄 API域名已更新: {oldDomain} → {DoMainApi}");
                            _logCallback($"🌐 投注将使用新域名: {DoMainApi}/frcomgame/createmainorder");
                        }
                        else
                        {
                            // 域名一致，输出确认日志（便于观察）
                            _logCallback($"✅ API域名确认: {DoMainApi}");
                        }
                        
                        // 🔥 解析响应中的余额（和通宝一样，从getmoneyinfo响应中获取余额）
                        if (!string.IsNullOrEmpty(response.Context))
                        {
                            try
                            {
                                var json = JObject.Parse(response.Context);
                                // 尝试多个可能的字段名
                                var balance = json["balance"]?.Value<decimal>() ?? 
                                             json["money"]?.Value<decimal>() ?? 
                                             json["amount"]?.Value<decimal>() ?? 0;
                                if (balance > 0)
                                {
                                    _currentBalance = balance;
                                    _logCallback($"💰 余额更新（来自getmoneyinfo）: {balance}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logCallback($"⚠️ 解析getmoneyinfo余额失败: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logCallback($"⚠️ 解析getmoneyinfo域名失败: {ex.Message}");
                    }
                }
                
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
        public List<BrowserOddsInfo> GetOddsList()
        {
            var oddsList = new List<BrowserOddsInfo>();
            
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
                
                oddsList.Add(new BrowserOddsInfo(car, play, name, odds, oddsId));
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

