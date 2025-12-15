using Unit.Shared.Helpers;
using Unit.Shared.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
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
    /// Yyds666 平台脚本 (Mail System风格登录)
    /// 登录地址: https://client.06n.yyds666.me/login?redirect=%2F
    /// </summary>
    public class Yyds666Script : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ModernHttpHelper _httpHelper;
        private bool _isLoggedIn = false;
        private decimal _currentBalance = 0;
        private string _baseUrl = "";  // 缓存的base URL
        
        // 赔率映射表
        private readonly Dictionary<string, OddsInfo> _oddsMap = new Dictionary<string, OddsInfo>();
        
        public Yyds666Script(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            _httpHelper = new ModernHttpHelper(_httpClient);  // 🔥 初始化 ModernHttpHelper
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 初始化赔率映射表
            InitializeOddsMap();
        }
        
        private void Log(string message) => _logCallback($"[Yyds666] {message}");
        
        /// <summary>
        /// 初始化赔率映射表（标准Binggo赔率）
        /// </summary>
        private void InitializeOddsMap()
        {
            // P1 平码一
            _oddsMap["P1大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", "1_big", 1.97f);
            _oddsMap["P1小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.小, "平码一", "1_small", 1.97f);
            _oddsMap["P1单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.单, "平码一", "1_odd", 1.97f);
            _oddsMap["P1双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.双, "平码一", "1_even", 1.97f);
            _oddsMap["P1尾大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾大, "平码一", "1_tail_big", 1.97f);
            _oddsMap["P1尾小"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.尾小, "平码一", "1_tail_small", 1.97f);
            _oddsMap["P1合单"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合单, "平码一", "1_sum_odd", 1.97f);
            _oddsMap["P1合双"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.合双, "平码一", "1_sum_even", 1.97f);
            
            // P2 平码二
            _oddsMap["P2大"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.大, "平码二", "2_big", 1.97f);
            _oddsMap["P2小"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.小, "平码二", "2_small", 1.97f);
            _oddsMap["P2单"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.单, "平码二", "2_odd", 1.97f);
            _oddsMap["P2双"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.双, "平码二", "2_even", 1.97f);
            _oddsMap["P2尾大"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.尾大, "平码二", "2_tail_big", 1.97f);
            _oddsMap["P2尾小"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.尾小, "平码二", "2_tail_small", 1.97f);
            _oddsMap["P2合单"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.合单, "平码二", "2_sum_odd", 1.97f);
            _oddsMap["P2合双"] = new OddsInfo(CarNumEnum.P2, BetPlayEnum.合双, "平码二", "2_sum_even", 1.97f);
            
            // P3 平码三
            _oddsMap["P3大"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.大, "平码三", "3_big", 1.97f);
            _oddsMap["P3小"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.小, "平码三", "3_small", 1.97f);
            _oddsMap["P3单"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.单, "平码三", "3_odd", 1.97f);
            _oddsMap["P3双"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.双, "平码三", "3_even", 1.97f);
            _oddsMap["P3尾大"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.尾大, "平码三", "3_tail_big", 1.97f);
            _oddsMap["P3尾小"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.尾小, "平码三", "3_tail_small", 1.97f);
            _oddsMap["P3合单"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.合单, "平码三", "3_sum_odd", 1.97f);
            _oddsMap["P3合双"] = new OddsInfo(CarNumEnum.P3, BetPlayEnum.合双, "平码三", "3_sum_even", 1.97f);
            
            // P4 平码四
            _oddsMap["P4大"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.大, "平码四", "4_big", 1.97f);
            _oddsMap["P4小"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.小, "平码四", "4_small", 1.97f);
            _oddsMap["P4单"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.单, "平码四", "4_odd", 1.97f);
            _oddsMap["P4双"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.双, "平码四", "4_even", 1.97f);
            _oddsMap["P4尾大"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.尾大, "平码四", "4_tail_big", 1.97f);
            _oddsMap["P4尾小"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.尾小, "平码四", "4_tail_small", 1.97f);
            _oddsMap["P4合单"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.合单, "平码四", "4_sum_odd", 1.97f);
            _oddsMap["P4合双"] = new OddsInfo(CarNumEnum.P4, BetPlayEnum.合双, "平码四", "4_sum_even", 1.97f);
            
            // P5 特码
            _oddsMap["P5大"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.大, "特码", "5_big", 1.97f);
            _oddsMap["P5小"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.小, "特码", "5_small", 1.97f);
            _oddsMap["P5单"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.单, "特码", "5_odd", 1.97f);
            _oddsMap["P5双"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.双, "特码", "5_even", 1.97f);
            _oddsMap["P5尾大"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.尾大, "特码", "5_tail_big", 1.97f);
            _oddsMap["P5尾小"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.尾小, "特码", "5_tail_small", 1.97f);
            _oddsMap["P5合单"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.合单, "特码", "5_sum_odd", 1.97f);
            _oddsMap["P5合双"] = new OddsInfo(CarNumEnum.P5, BetPlayEnum.合双, "特码", "5_sum_even", 1.97f);
            
            // P总 和值
            _oddsMap["P总大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.大, "和值", "sum_big", 1.97f);
            _oddsMap["P总小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.小, "和值", "sum_small", 1.97f);
            _oddsMap["P总单"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.单, "和值", "sum_odd", 1.97f);
            _oddsMap["P总双"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.双, "和值", "sum_even", 1.97f);
            _oddsMap["P总尾大"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾大, "和值", "sum_tail_big", 1.97f);
            _oddsMap["P总尾小"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.尾小, "和值", "sum_tail_small", 1.97f);
            _oddsMap["P总龙"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.龙, "和值", "sum_dragon", 1.97f);
            _oddsMap["P总虎"] = new OddsInfo(CarNumEnum.P总, BetPlayEnum.虎, "和值", "sum_tiger", 1.97f);
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
        /// 登录 Yyds666 平台
        /// 
        /// 登录流程：
        /// 1. 填充用户名和密码
        /// 2. 获取验证码（需要用户手动输入或OCR识别）
        /// 3. 点击登录按钮
        /// 4. 等待跳转到主页
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Log("🔐 开始登录 Yyds666...");
                
                // 步骤1: 填充用户名
                var fillUsernameScript = $@"
                    (function() {{
                        try {{
                            // 查找用户名输入框: name=""username""
                            var usernameInput = document.querySelector('input[name=""username""]');
                            if (!usernameInput) {{
                                return {{ success: false, error: '未找到用户名输入框' }};
                            }}
                            
                            // 清空并填充用户名
                            usernameInput.value = '{username}';
                            usernameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            usernameInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            
                            return {{ success: true, message: '用户名已填充' }};
                        }} catch (e) {{
                            return {{ success: false, error: e.toString() }};
                        }}
                    }})();
                ";
                
                var usernameResult = await _webView.CoreWebView2.ExecuteScriptAsync(fillUsernameScript);
                var usernameJson = JObject.Parse(usernameResult);
                
                if (usernameJson["success"]?.ToObject<bool>() != true)
                {
                    var error = usernameJson["error"]?.ToString() ?? "未知错误";
                    Log($"❌ 填充用户名失败: {error}");
                    return false;
                }
                
                Log($"✅ 用户名已填充: {username}");
                await Task.Delay(500);
                
                // 步骤2: 填充密码
                var fillPasswordScript = $@"
                    (function() {{
                        try {{
                            // 查找密码输入框: name=""password"" 或 id=""txtPass""
                            var passwordInput = document.querySelector('input[name=""password""]') 
                                             || document.querySelector('input#txtPass');
                            if (!passwordInput) {{
                                return {{ success: false, error: '未找到密码输入框' }};
                            }}
                            
                            // 清空并填充密码
                            passwordInput.value = '{password}';
                            passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            passwordInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            
                            return {{ success: true, message: '密码已填充' }};
                        }} catch (e) {{
                            return {{ success: false, error: e.toString() }};
                        }}
                    }})();
                ";
                
                var passwordResult = await _webView.CoreWebView2.ExecuteScriptAsync(fillPasswordScript);
                var passwordJson = JObject.Parse(passwordResult);
                
                if (passwordJson["success"]?.ToObject<bool>() != true)
                {
                    var error = passwordJson["error"]?.ToString() ?? "未知错误";
                    Log($"❌ 填充密码失败: {error}");
                    return false;
                }
                
                Log("✅ 密码已填充");
                await Task.Delay(500);
                
                // 步骤3: 提示用户输入验证码
                Log("⚠️ 请在浏览器中手动输入验证码，然后点击登录按钮！");
                Log("💡 验证码输入框: name=\"code\"");
                Log("💡 登录按钮: class=\"login_submit\"");
                
                // 🔥 这里不自动点击登录按钮，让用户手动输入验证码后点击
                // 如果需要自动化验证码识别，需要集成OCR服务（如百度OCR、腾讯OCR等）
                
                // 步骤4: 检测登录状态（通过URL变化或Cookie）
                Log("⏳ 等待用户登录...");
                
                // 等待最多60秒，检测是否登录成功
                for (int i = 0; i < 60; i++)
                {
                    await Task.Delay(1000);
                    
                    // 检查URL是否已跳转（登录成功后通常会跳转到首页）
                    var currentUrl = _webView.CoreWebView2.Source;
                    if (!currentUrl.Contains("/login"))
                    {
                        Log($"✅ 登录成功！当前URL: {currentUrl}");
                        _isLoggedIn = true;
                        _baseUrl = new Uri(currentUrl).GetLeftPart(UriPartial.Authority);
                        
                        // 尝试获取余额
                        await Task.Delay(2000); // 等待页面加载完成
                        await GetBalanceAsync();
                        
                        return true;
                    }
                }
                
                Log("⏱️ 登录超时（60秒内未检测到跳转）");
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ 登录异常: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 获取账户余额
        /// 🔥 需要根据实际页面结构修改
        /// </summary>
        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                Log("💰 正在获取账户余额...");
                
                // 🔥 这里需要根据实际登录后的页面结构来获取余额
                // 示例：从页面元素中提取余额
                var getBalanceScript = @"
                    (function() {
                        try {
                            // 🔥 示例：假设余额显示在 class=""balance"" 或 id=""userBalance"" 的元素中
                            var balanceElement = document.querySelector('.balance') 
                                              || document.querySelector('#userBalance')
                                              || document.querySelector('[data-balance]');
                            
                            if (balanceElement) {
                                var balanceText = balanceElement.innerText || balanceElement.textContent;
                                // 提取数字（去除货币符号等）
                                var balance = parseFloat(balanceText.replace(/[^0-9.]/g, ''));
                                return { success: true, balance: balance };
                            }
                            
                            return { success: false, error: '未找到余额元素' };
                        } catch (e) {
                            return { success: false, error: e.toString() };
                        }
                    })();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(getBalanceScript);
                var json = JObject.Parse(result);
                
                if (json["success"]?.ToObject<bool>() == true)
                {
                    _currentBalance = json["balance"]?.ToObject<decimal>() ?? 0;
                    Log($"✅ 账户余额: {_currentBalance:F2}");
                    return _currentBalance;
                }
                else
                {
                    Log($"⚠️ 获取余额失败: {json["error"]?.ToString() ?? "未知错误"}");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 获取余额异常: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// 下注
        /// 🔥 需要根据实际平台的投注接口实现
        /// </summary>
        public async Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BetStandardOrderList orders)
        {
            try
            {
                if (!_isLoggedIn)
                {
                    Log("❌ 未登录，无法下注");
                    return (false, "", "未登录");
                }
                
                if (orders == null || orders.Count == 0)
                {
                    Log("❌ 订单列表为空");
                    return (false, "", "订单列表为空");
                }
                
                Log($"📝 开始下注: 共 {orders.Count} 个投注项");
                
                // 🔥 这里需要根据实际平台的投注接口实现
                // 示例：构造投注数据
                var betItems = new List<object>();
                foreach (var order in orders)
                {
                    // 获取对应的赔率信息
                    var key = $"P{(int)order.Car}{order.Play}";
                    if (_oddsMap.TryGetValue(key, out var oddsInfo))
                    {
                        betItems.Add(new
                        {
                            playId = oddsInfo.OddsId,  // 玩法ID
                            amount = order.MoneySum,   // 金额
                            odds = oddsInfo.Odds       // 赔率
                        });
                        
                        Log($"  - {key}: {order.MoneySum} 元 (赔率 {oddsInfo.Odds})");
                    }
                    else
                    {
                        Log($"⚠️ 未找到 {key} 的赔率映射");
                    }
                }
                
                if (betItems.Count == 0)
                {
                    Log("❌ 没有有效的投注项");
                    return (false, "", "没有有效的投注项");
                }
                
                // 🔥 方法1: 通过HTTP API下注（需要抓包分析接口）
                // 示例：
                // var betData = new { items = betItems, issueId = orders.IssueId };
                // var jsonContent = JsonConvert.SerializeObject(betData);
                // var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                // var cookies = await GetCookiesAsync();
                // _httpClient.DefaultRequestHeaders.Clear();
                // _httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
                // var response = await _httpClient.PostAsync($"{_baseUrl}/api/bet/place", content);
                // var responseText = await response.Content.ReadAsStringAsync();
                
                // 🔥 方法2: 通过JavaScript在页面中下注
                var placeBetScript = $@"
                    (function() {{
                        try {{
                            // 🔥 这里需要根据实际页面的投注逻辑实现
                            // 示例：调用页面中的投注函数
                            var betData = {JsonConvert.SerializeObject(betItems)};
                            
                            // 假设页面有全局的投注函数 window.placeBet()
                            if (typeof window.placeBet === 'function') {{
                                var result = window.placeBet(betData);
                                return {{ success: true, result: result }};
                            }}
                            
                            return {{ success: false, error: '未找到投注函数' }};
                        }} catch (e) {{
                            return {{ success: false, error: e.toString() }};
                        }}
                    }})();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(placeBetScript);
                var json = JObject.Parse(result);
                
                if (json["success"]?.ToObject<bool>() == true)
                {
                    var orderId = json["result"]?["orderId"]?.ToString() ?? DateTime.Now.Ticks.ToString();
                    Log($"✅ 下注成功！订单号: {orderId}");
                    return (true, orderId, result);
                }
                else
                {
                    var error = json["error"]?.ToString() ?? "未知错误";
                    Log($"❌ 下注失败: {error}");
                    return (false, "", error);
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 下注异常: {ex.Message}");
                return (false, "", ex.Message);
            }
        }
        
        /// <summary>
        /// 处理HTTP响应拦截
        /// </summary>
        public void HandleResponse(ResponseEventArgs response)
        {
            try
            {
                var url = response.Url;
                var content = response.Context;
                
                // 🔥 拦截关键API响应，提取有用信息
                
                // 示例1: 拦截登录响应
                if (url.Contains("/api/login") || url.Contains("/login"))
                {
                    Log($"🔍 拦截登录响应: {content}");
                    // 可以从响应中提取token、用户信息等
                }
                
                // 示例2: 拦截余额查询响应
                if (url.Contains("/api/balance") || url.Contains("/balance"))
                {
                    try
                    {
                        var json = JObject.Parse(content);
                        if (json["balance"] != null)
                        {
                            _currentBalance = json["balance"].ToObject<decimal>();
                            Log($"💰 余额更新: {_currentBalance:F2}");
                        }
                    }
                    catch { }
                }
                
                // 示例3: 拦截投注响应
                if (url.Contains("/api/bet") || url.Contains("/bet/place"))
                {
                    Log($"📋 拦截投注响应: {content}");
                }
                
                // 示例4: 拦截赔率数据
                if (url.Contains("/api/odds") || url.Contains("/odds"))
                {
                    try
                    {
                        var json = JObject.Parse(content);
                        // 更新赔率映射表
                        Log($"📊 拦截赔率数据: {json.ToString().Substring(0, Math.Min(100, json.ToString().Length))}...");
                    }
                    catch { }
                }
            }
            catch
            {
                // 忽略拦截异常
            }
        }
        
        /// <summary>
        /// 获取赔率列表
        /// </summary>
        public List<OddsInfo> GetOddsList()
        {
            return _oddsMap.Values.ToList();
        }
        
        /// <summary>
        /// 获取未结算的订单信息（Yyds666 平台暂不支持）
        /// </summary>
        public Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
            int state = 0,
            int pageNum = 1,
            int pageCount = 20,
            string? beginDate = null,
            string? endDate = null,
            int timeout = 10)
        {
            // 🔥 Yyds666 平台暂不支持获取订单列表
            Log("⚠️ Yyds666 平台暂不支持获取订单列表");
            return Task.FromResult<(bool, List<JObject>?, int, int, string)>((false, null, 0, 0, "平台暂不支持"));
        }
    }
}

