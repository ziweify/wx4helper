using Unit.Shared.Models;
using zhaocaimao.Services.AutoBet.Browser.Services;
using Microsoft.Web.WebView2.WinForms;
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
    /// ADK 平台脚本 - 参考 F5BotV2/ADKMember.cs
    /// </summary>
    public class ADKScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isLoggedIn = false;
        private string _baseUrl = "";  // 缓存的base URL
        private string _qihaoid = "";  // 期号ID
        private string _mysession = "";  // session
        private decimal _currentBalance = 0;
        
        // 🔥 保存用户名和密码，用于页面刷新后自动重新填充
        private string _savedUsername = "";
        private string _savedPassword = "";
        private bool _isLoginAttempting = false; // 标记是否正在尝试登录
        
        // 赔率映射表（参考 F5BotV2，ADK使用茅台的赔率映射）
        private readonly Dictionary<string, Models.OddsInfo> _oddsMap = new Dictionary<string, Models.OddsInfo>();
        
        public ADKScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表（参考 F5BotV2，ADK使用茅台的赔率映射）
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[ADK] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（参考 F5BotV2，ADK使用茅台的赔率映射）
        /// </summary>
        private void InitializeOddsMap()
        {
            // 使用茅台的赔率映射（参考 F5BotV2 ADKMember.cs Line 35）
            // P1
            _oddsMap["P1大"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", 1.97f, "DX1_D");
            _oddsMap["P1小"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.小, "平码一", 1.97f, "DX1_X");
            _oddsMap["P1单"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.单, "平码一", 1.97f, "DS1_D");
            _oddsMap["P1双"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.双, "平码一", 1.97f, "DS1_S");
            _oddsMap["P1尾大"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.尾大, "平码一", 1.97f, "WDX1_D");
            _oddsMap["P1尾小"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.尾小, "平码一", 1.97f, "WDX1_X");
            _oddsMap["P1合单"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.合单, "平码一", 1.97f, "HDS1_D");
            _oddsMap["P1合双"] = new Models.OddsInfo(CarNumEnum.P1, BetPlayEnum.合双, "平码一", 1.97f, "HDS1_S");
            
            // P2-P5 类似...
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
        
        /// <summary>
        /// 获取投注金额字符串（参考 F5BotV2 ADKMember.cs GetBetString 方法）
        /// </summary>
        private string GetBetString(BetStandardOrderList items, CarNumEnum car, BetPlayEnum play)
        {
            var item = items.FirstOrDefault(p => p.Car == car && p.Play == play);
            return item != null ? item.MoneySum.ToString() : "";
        }
        
        /// <summary>
        /// 检查是否已登录（通过URL和页面内容判断）
        /// </summary>
        private async Task<bool> CheckLoginStatusAsync()
        {
            try
            {
                if (_webView?.CoreWebView2 == null) return false;
                
                // 检查URL
                var currentUrlScript = "window.location.href";
                var currentUrlResult = await _webView.CoreWebView2.ExecuteScriptAsync(currentUrlScript);
                if (currentUrlResult != null)
                {
                    var urlStr = currentUrlResult.Trim('"');
                    
                    // 如果URL包含 default.html 且不包含 login，说明已登录
                    if (!urlStr.Contains("login") && (urlStr.Contains("default.html") || urlStr.Contains("index")))
                    {
                        // 设置 baseUrl
                        if (string.IsNullOrEmpty(_baseUrl))
                        {
                            try
                            {
                                _baseUrl = new Uri(urlStr).GetLeftPart(UriPartial.Authority);
                                Log($"✅ 检测到已登录，Base URL: {_baseUrl}");
                            }
                            catch { }
                        }
                        return true;
                    }
                }
                
                // 检查页面内容（查找登录表单，如果找不到说明可能已登录）
                var checkScript = @"
                    (function() {
                        try {
                            // 查找登录表单元素
                            const loginForm = document.querySelector('form');
                            const usernameInput = document.querySelector('input[type=""text""]');
                            const passwordInput = document.querySelector('input[type=""password""]');
                            
                            // 如果找不到登录表单，或者URL不是登录页面，说明可能已登录
                            const currentUrl = window.location.href;
                            if (!currentUrl.includes('login') && (!loginForm || (!usernameInput && !passwordInput))) {
                                return { isLoggedIn: true, url: currentUrl };
                            }
                            
                            return { isLoggedIn: false, url: currentUrl };
                        } catch (error) {
                            return { isLoggedIn: false, error: error.message };
                        }
                    })();
                ";
                
                var checkResult = await _webView.CoreWebView2.ExecuteScriptAsync(checkScript);
                var checkJson = JObject.Parse(checkResult);
                var isLoggedIn = checkJson["isLoggedIn"]?.Value<bool>() ?? false;
                
                if (isLoggedIn)
                {
                    var url = checkJson["url"]?.ToString()?.Trim('"') ?? "";
                    if (!string.IsNullOrEmpty(url) && string.IsNullOrEmpty(_baseUrl))
                    {
                        try
                        {
                            _baseUrl = new Uri(url).GetLeftPart(UriPartial.Authority);
                            Log($"✅ 检测到已登录，Base URL: {_baseUrl}");
                        }
                        catch { }
                    }
                }
                
                return isLoggedIn;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Log("🔐 开始登录ADK...");
                
                // 🔥 保存用户名和密码，用于页面刷新后自动重新填充
                _savedUsername = username;
                _savedPassword = password;
                _isLoginAttempting = true;
                
                // 🔥 首先检查用户是否已经手动登录
                var alreadyLoggedIn = await CheckLoginStatusAsync();
                if (alreadyLoggedIn)
                {
                    _isLoggedIn = true;
                    _isLoginAttempting = false;
                    Log("✅ 检测到用户已手动登录，跳过自动登录流程");
                    return true;
                }
                
                // 等待页面加载完成（参考 F5BotV2 ADKMember.cs Line 88-234）
                Log("⏳ 等待登录页面加载完成...");
                await Task.Delay(1000); // 给页面1秒加载时间
                
                // 先检查页面上的input数量（F5BotV2检查是否为4个input）
                var checkInputCountScript = @"document.querySelectorAll('input').length";
                var inputCountResult = await _webView.CoreWebView2.ExecuteScriptAsync(checkInputCountScript);
                Log($"📊 页面input元素数量: {inputCountResult}");
                
                // 🔥 检查登录表单的属性，看是否有target="_blank"等导致新页面打开的设置
                var checkFormScript = @"
                    (function() {
                        const form = document.querySelector('form');
                        if (form) {
                            return {
                                action: form.action || '',
                                method: form.method || '',
                                target: form.target || '',
                                id: form.id || '',
                                name: form.name || ''
                            };
                        }
                        return { error: 'No form found' };
                    })();
                ";
                try
                {
                    var formCheckResult = await _webView.CoreWebView2.ExecuteScriptAsync(checkFormScript);
                    var formJson = JObject.Parse(formCheckResult);
                    Log($"📋 登录表单属性:");
                    Log($"   action: {formJson["action"]}");
                    Log($"   method: {formJson["method"]}");
                    Log($"   target: {formJson["target"]}");
                    Log($"   id: {formJson["id"]}");
                }
                catch (Exception ex)
                {
                    Log($"⚠️  检查表单属性失败: {ex.Message}");
                }
                
                // 修正登录表单选择器，参考 F5BotV2 使用简单直接的方式
                var script = $@"
                    (function() {{
                        try {{
                            // 获取所有input的调试信息
                            const allInputs = document.querySelectorAll('input');
                            const inputDebugInfo = Array.from(allInputs).map((input, idx) => ({{
                                index: idx,
                                type: input.type || 'unknown',
                                id: input.id || '',
                                name: input.name || '',
                                tagName: input.tagName
                            }}));
                            
                            // 直接使用 F5BotV2 中的选择器
                            const usernameInput = document.querySelector('#txtUsername');
                            const passwordInput = document.querySelector('#txtPass');
                            
                            if (usernameInput && passwordInput) {{
                                // 直接设置value，参考 F5BotV2 Line 116-117
                                usernameInput.value = '{username}';
                                passwordInput.value = '{password}';
                                
                                // 触发事件
                                usernameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                usernameInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                passwordInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                
                                // 🔥 强制修复表单属性，防止在新窗口打开
                                const loginForm = document.querySelector('form');
                                if (loginForm) {{
                                    // 移除target属性，防止打开新页面
                                    loginForm.removeAttribute('target');
                                    loginForm.target = '';
                                    
                                    // 确保表单在当前页面提交
                                    if (loginForm.action) {{
                                        // 如果action是空的或者是javascript:，设置为当前页面
                                        if (!loginForm.action || loginForm.action.startsWith('javascript:')) {{
                                            loginForm.action = window.location.href;
                                        }}
                                    }}
                                    
                                    // 标记：用户名和密码已填充，准备登录
                                    window._adkLoginAttempting = true;
                                    window._adkFormFixed = true;
                                    
                                    console.log('表单已修复: target=' + loginForm.target + ', action=' + loginForm.action);
                                }}
                                
                                return {{ 
                                    success: true, 
                                    message: '表单已填充并修复，请输入验证码（系统会自动提交）',
                                    usernameId: usernameInput.id,
                                    usernameName: usernameInput.name || '',
                                    passwordId: passwordInput.id,
                                    passwordName: passwordInput.name || '',
                                    inputCount: allInputs.length,
                                    inputDebugInfo: inputDebugInfo,
                                    formFixed: !!loginForm
                                }};
                            }} else {{
                                return {{ 
                                    success: false, 
                                    message: '未找到登录表单 (#txtUsername 或 #txtPass 不存在)', 
                                    usernameFound: !!usernameInput, 
                                    passwordFound: !!passwordInput,
                                    inputCount: allInputs.length,
                                    inputDebugInfo: inputDebugInfo
                                }};
                            }}
                        }} catch (error) {{
                            return {{ success: false, message: '执行错误: ' + error.message, error: error.stack }};
                        }}
                    }})();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                Log($"📄 登录脚本返回结果: {result}");
                
                var json = JObject.Parse(result);
                
                var success = json["success"]?.Value<bool>() ?? false;
                var message = json["message"]?.ToString() ?? "";
                
                // 输出调试信息
                if (json["inputDebugInfo"] != null)
                {
                    Log($"🔍 页面input元素调试信息 (总数: {json["inputCount"]}个):");
                    var debugInfo = json["inputDebugInfo"];
                    foreach (var item in debugInfo)
                    {
                        Log($"  [{item["index"]}] type={item["type"]}, id={item["id"]}, name={item["name"]}");
                    }
                }
                
                Log(success ? $"✅ {message}" : $"❌ {message}");
                
                if (success)
                {
                    if (json["usernameId"] != null)
                        Log($"  用户名输入框: id={json["usernameId"]}, name={json["usernameName"]}");
                    if (json["passwordId"] != null)
                        Log($"  密码输入框: id={json["passwordId"]}, name={json["passwordName"]}");
                }
                else
                {
                    Log($"  usernameFound={json["usernameFound"]}, passwordFound={json["passwordFound"]}");
                }
                
                if (success)
                {
                    // 等待登录完成（参考 F5BotV2 ADKMember.cs Line 107-173）
                    Log("⏳ 等待登录完成（请输入验证码）...");
                    var waitCount = 0;
                    var autoClicked = false;
                    
                    while (!_isLoggedIn && waitCount < 600) // 增加到60秒等待
                    {
                        await Task.Delay(100);
                        waitCount++;
                        
                        // 🔥 参考 F5BotV2 Line 118-124：检查验证码是否已填充，如果已填充则自动点击登录
                        if (!autoClicked && waitCount % 10 == 0) // 每1秒检查一次
                        {
                            var checkVerifyCodeScript = @"
                                (function() {
                                    try {
                                        const verifyCodeInput = document.querySelector('#VerifyCode') || 
                                                              document.querySelector('input[name=""validate""]') ||
                                                              document.querySelectorAll('input[type=""text""]')[1]; // 第二个text输入框通常是验证码
                                        const loginButton = document.querySelector('#submit1') ||
                                                          document.querySelector('input[type=""submit""]') ||
                                                          document.querySelector('button[type=""submit""]');
                                        
                                        if (verifyCodeInput && verifyCodeInput.value && verifyCodeInput.value.length >= 4) {
                                            if (loginButton) {
                                                loginButton.click();
                                                return { clicked: true, verifyCode: verifyCodeInput.value.length };
                                            }
                                        }
                                        return { clicked: false, verifyCode: verifyCodeInput ? verifyCodeInput.value.length : 0 };
                                    } catch (error) {
                                        return { clicked: false, error: error.message };
                                    }
                                })();
                            ";
                            
                            try
                            {
                                var checkResult = await _webView.CoreWebView2.ExecuteScriptAsync(checkVerifyCodeScript);
                                var checkJson = JObject.Parse(checkResult);
                                var clicked = checkJson["clicked"]?.Value<bool>() ?? false;
                                
                                if (clicked)
                                {
                                    autoClicked = true;
                                    Log("✅ 检测到验证码已填充，自动点击登录按钮");
                                    await Task.Delay(1000); // 等待登录请求
                                }
                            }
                            catch { }
                        }
                        
                        // 检查是否已登录（通过URL和页面内容判断）
                        var isLoggedIn = await CheckLoginStatusAsync();
                        if (isLoggedIn)
                        {
                            _isLoggedIn = true;
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
                        Log("❌ 登录超时或失败（请检查账号密码或验证码是否正确）");
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
        /// 自动重新填充登录表单（页面刷新后调用）
        /// </summary>
        public async Task AutoRefillLoginForm()
        {
            if (string.IsNullOrEmpty(_savedUsername) || string.IsNullOrEmpty(_savedPassword) || !_isLoginAttempting)
            {
                return;
            }
            
            try
            {
                Log("🔄 自动重新填充登录表单...");
                await Task.Delay(500); // 等待页面完全加载
                
                // 使用相同的填充脚本
                var script = $@"
                    (function() {{
                        try {{
                            const usernameInput = document.querySelector('#txtUsername');
                            const passwordInput = document.querySelector('#txtPass');
                            
                            if (usernameInput && passwordInput) {{
                                usernameInput.value = '{_savedUsername}';
                                passwordInput.value = '{_savedPassword}';
                                
                                // 触发事件
                                usernameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                usernameInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                passwordInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                
                                // 强制修复表单属性
                                const loginForm = document.querySelector('form');
                                if (loginForm) {{
                                    loginForm.removeAttribute('target');
                                    loginForm.target = '';
                                }}
                                
                                return {{ success: true }};
                            }}
                            return {{ success: false, message: '未找到输入框' }};
                        }} catch (error) {{
                            return {{ success: false, message: error.message }};
                        }}
                    }})();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                var json = JObject.Parse(result);
                var success = json["success"]?.Value<bool>() ?? false;
                
                if (success)
                {
                    Log("✅ 登录表单已自动重新填充，请重新输入验证码");
                }
                else
                {
                    Log($"❌ 重新填充失败: {json["message"]}");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 自动重新填充异常: {ex.Message}");
            }
        }
        
        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                // 🔥 如果_isLoggedIn为false，先检查是否已经登录
                if (!_isLoggedIn)
                {
                    var isLoggedIn = await CheckLoginStatusAsync();
                    if (isLoggedIn)
                    {
                        _isLoggedIn = true;
                        Log("✅ 检测到已登录，更新登录状态");
                    }
                    else
                    {
                        Log("❌ 未登录，无法获取余额");
                        return -1;
                    }
                }
                
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    Log("❌ 未获取到base URL，无法获取余额");
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
                // 🔥 如果_isLoggedIn为false，先检查是否已经登录
                if (!_isLoggedIn)
                {
                    var isLoggedIn = await CheckLoginStatusAsync();
                    if (isLoggedIn)
                    {
                        _isLoggedIn = true;
                        Log("✅ 检测到已登录，更新登录状态");
                    }
                    else
                    {
                        Log("❌ 未登录，无法投注");
                        return (false, "", "未登录");
                    }
                }
                
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    Log("❌ 未获取到base URL，无法投注");
                    return (false, "", "未获取到base URL");
                }
                
                if (orders == null || orders.Count == 0)
                {
                    Log("❌ 订单列表为空");
                    return (false, "", "订单列表为空");
                }
                
                Log($"🎲 开始投注，订单数: {orders.Count}");
                
                // 🔥 参考 F5BotV2 ADKMember.cs Bet 方法（Line 286-488）
                
                // 1. 获取期号ID和mysession（参考 F5BotV2 Line 318-349）
                int issueid = orders[0].IssueId;
                await GetQihaoIdAndSessionAsync(issueid);
                
                if (string.IsNullOrEmpty(_qihaoid) || string.IsNullOrEmpty(_mysession))
                {
                    Log("❌ 未获取到期号ID或mysession，无法投注");
                    return (false, "", "未获取到投注参数");
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
                
                // 3. 构建POST数据（参考 F5BotV2 Line 356-426）
                var postPacket = new StringBuilder();
                try
                {
                    // P总
                    postPacket.Append($"txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.尾大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.龙)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.尾小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P总, BetPlayEnum.虎)}");
                    
                    // P1-P5（参考 F5BotV2 Line 371-418）
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.大)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.小)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.单)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.双)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.尾大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.尾大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.尾大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.尾大)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.尾大)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.尾小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.尾小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.尾小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.尾小)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.尾小)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.合单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.合单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.合单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.合单)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.合单)}");
                    
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P1, BetPlayEnum.合双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P2, BetPlayEnum.合双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P3, BetPlayEnum.合双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P4, BetPlayEnum.合双)}");
                    postPacket.Append($"&txtnumber={GetBetString(betItems, CarNumEnum.P5, BetPlayEnum.合双)}");
                    
                    // 其他玩法的空值（参考 F5BotV2 Line 420-423）
                    for (int i = 0; i < 200; i++)
                    {
                        postPacket.Append($"&txtnumber=");
                    }
                    
                    postPacket.Append($"&txtnumber2=&txtvalue_258=&txtnumber=&txtvalue_248=&txtnumber=&txtvalue_249=&txtnumber=&txtvalue_250=&txtnumber=&txtvalue_251=&txtnumber=&txtvalue_252=&txtnumber=&txtvalue_253=&txtnumber=&txtvalue_254=&txtnumber=&txtvalue_255=&txtnumber=&txtvalue_256=&txtnumber=&txtvalue_257=");
                    postPacket.Append($"&qihaoid={_qihaoid}&mysession={_mysession}");
                }
                catch (Exception ex)
                {
                    Log($"❌ 构建投注数据失败: {ex.Message}");
                    return (false, "", $"构建投注数据失败: {ex.Message}");
                }
                
                var postData = postPacket.ToString();
                Log($"📤 POST数据: {postData.Substring(0, Math.Min(200, postData.Length))}...");
                
                // 4. 发送POST请求（参考 F5BotV2 Line 443-455）
                string url = $"{_baseUrl}/default.html?method=add";
                var cookies = await GetCookiesAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Cookie", cookies);
                request.Headers.Add("Referer", $"{_baseUrl}/default.html");
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                Log($"📥 投注响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");
                
                // 5. 解析响应（参考 F5BotV2 Line 458-488）
                var json = JObject.Parse(responseText);
                var error = json["error"]?.Value<bool>() ?? true;
                
                if (!error)
                {
                    Log($"✅ 投注成功");
                    return (true, $"ADK{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}", responseText);
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
        /// 获取期号ID和mysession（参考 F5BotV2 ADKMember.cs Line 318-349）
        /// </summary>
        private async Task<bool> GetQihaoIdAndSessionAsync(int issueid)
        {
            try
            {
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    Log("❌ 未获取到base URL，无法获取期号ID和session");
                    return false;
                }
                
                var cookies = await GetCookiesAsync();
                
                // 1. 获取期号ID（参考 F5BotV2 Line 323-338）
                string qihaoUrl = $"{_baseUrl}/source/AjaxShiFen.ashx";
                var qihaoRequest = new HttpRequestMessage(HttpMethod.Post, qihaoUrl);
                qihaoRequest.Headers.Add("Cookie", cookies);
                qihaoRequest.Headers.Add("Referer", $"{_baseUrl}/default.html");
                qihaoRequest.Headers.Add("Accept", "text/plain, */*; q=0.01");
                
                var qihaoResponse = await _httpClient.SendAsync(qihaoRequest);
                var qihaoText = await qihaoResponse.Content.ReadAsStringAsync();
                
                string[] sfens = qihaoText.Split('|');
                if (sfens.Length > 2)
                {
                    if (sfens[2].Replace(" ", "") == issueid.ToString().Replace(" ", ""))
                    {
                        _qihaoid = sfens[4];
                        Log($"✅ 获取到期号ID: {_qihaoid}");
                    }
                }
                
                // 2. 获取mysession（参考 F5BotV2 Line 342-349）
                string paramUrl = $"{_baseUrl}/default.html";
                var paramRequest = new HttpRequestMessage(HttpMethod.Get, paramUrl);
                paramRequest.Headers.Add("Cookie", cookies);
                paramRequest.Headers.Add("Referer", $"{_baseUrl}/default.html");
                
                var paramResponse = await _httpClient.SendAsync(paramRequest);
                var paramHtml = await paramResponse.Content.ReadAsStringAsync();
                
                var match = Regex.Match(paramHtml, @"name=""mysession""[^#]+value=""([^#]+)""");
                if (match.Success)
                {
                    _mysession = match.Groups[1].Value;
                    Log($"✅ 获取到mysession: {_mysession.Substring(0, Math.Min(10, _mysession.Length))}...");
                }
                
                return !string.IsNullOrEmpty(_qihaoid) && !string.IsNullOrEmpty(_mysession);
            }
            catch (Exception ex)
            {
                Log($"❌ 获取期号ID和session异常: {ex.Message}");
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
                
                // 🔥 检查登录状态（如果URL不是登录页面，说明可能已登录）
                if (!string.IsNullOrEmpty(response.Url) && !response.Url.Contains("login"))
                {
                    if (response.Url.Contains("default.html") || response.Url.Contains("index"))
                    {
                        if (!_isLoggedIn)
                        {
                            _isLoggedIn = true;
                            Log("✅ 检测到页面跳转，已登录状态已更新");
                        }
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

