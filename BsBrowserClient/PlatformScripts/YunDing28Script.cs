using BaiShengVx3Plus.Shared.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 云顶28 平台脚本 - 智能登录和自动投注
    /// </summary>
    public class YunDing28Script : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private bool _isLoggedIn = false;
        
        public YunDing28Script(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
        }
        
        private void Log(string message) => _logCallback($"[云顶] {message}");
        
        /// <summary>
        /// 智能登录 - 检测页面状态并自动登录
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Log("🔐 开始智能登录流程...");
                
                var maxAttempts = 60; // 最多等待60秒
                var attempt = 0;
                
                while (attempt < maxAttempts && !_isLoggedIn)
                {
                    await Task.Delay(1000);
                    attempt++;
                    
                    // 获取当前URL
                    var currentUrl = await _webView.CoreWebView2.ExecuteScriptAsync("window.location.href");
                    var url = currentUrl?.Replace("\"", "") ?? "";
                    
                    // 1. 检测是否在登录页面
                    var pageState = await DetectPageStateAsync();
                    
                    switch (pageState)
                    {
                        case "login":
                            Log("检测到登录页面，尝试自动填充...");
                            await AutoFillLoginFormAsync(username, password);
                            await Task.Delay(2000); // 等待跳转
                            break;
                            
                        case "home":
                        case "game":
                            Log("✅ 检测到已登录状态！");
                            _isLoggedIn = true;
                            return true;
                            
                        case "agreement":
                            Log("检测到协议页面，尝试自动同意...");
                            await AutoAgreeAgreementAsync();
                            break;
                            
                        case "notice":
                            Log("检测到公告弹窗，尝试关闭...");
                            await AutoCloseNoticeAsync();
                            break;
                            
                        default:
                            Log($"等待页面加载... ({attempt}/{maxAttempts})");
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
                    Log("⚠️ 登录超时或需要手动介入");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 登录异常: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 检测当前页面状态
        /// </summary>
        private async Task<string> DetectPageStateAsync()
        {
            var script = @"
                (function() {
                    try {
                        const url = window.location.href;
                        const pathname = window.location.pathname;
                        const hash = window.location.hash;
                        
                        // 检测登录页面
                        const loginForm = document.querySelector('form.login-form') ||
                                        document.querySelector('#login-form') ||
                                        document.querySelector('.login-box');
                        const usernameInput = document.querySelector('input[type=""text""]') ||
                                            document.querySelector('input[name=""username""]');
                        const passwordInput = document.querySelector('input[type=""password""]');
                        
                        if (loginForm || (usernameInput && passwordInput)) {
                            return 'login';
                        }
                        
                        // 检测协议页面
                        if (url.includes('agreement') || hash.includes('agreement') || 
                            document.querySelector('.agreement') || document.querySelector('#agreement')) {
                            return 'agreement';
                        }
                        
                        // 检测公告弹窗
                        const notice = document.querySelector('.notice-popup') ||
                                      document.querySelector('.modal.show') ||
                                      document.querySelector('[class*=""popup""][style*=""display: block""]');
                        if (notice) {
                            return 'notice';
                        }
                        
                        // 检测游戏页面
                        const balance = document.querySelector('.balance') ||
                                      document.querySelector('.user-balance') ||
                                      document.querySelector('[class*=""balance""]');
                        const betArea = document.querySelector('.bet-area') ||
                                      document.querySelector('.betting-panel') ||
                                      document.querySelector('[class*=""bet""]');
                        
                        if (balance || betArea) {
                            return 'game';
                        }
                        
                        // 检测首页
                        if (url.includes('/home') || hash.includes('/home') || pathname === '/') {
                            return 'home';
                        }
                        
                        return 'unknown';
                    } catch (error) {
                        return 'error';
                    }
                })();
            ";
            
            var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
            return result?.Replace("\"", "") ?? "unknown";
        }
        
        /// <summary>
        /// 自动填充登录表单
        /// </summary>
        private async Task AutoFillLoginFormAsync(string username, string password)
        {
            var script = $@"
                (function() {{
                    try {{
                        // 查找输入框
                        const usernameInput = document.querySelector('input[type=""text""]') ||
                                            document.querySelector('input[name=""username""]') ||
                                            document.querySelector('#username');
                        const passwordInput = document.querySelector('input[type=""password""]') ||
                                            document.querySelector('input[name=""password""]') ||
                                            document.querySelector('#password');
                        
                        if (usernameInput && passwordInput) {{
                            usernameInput.value = '{username}';
                            passwordInput.value = '{password}';
                            
                            // 触发事件
                            usernameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            usernameInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            passwordInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            
                            // 查找并点击登录按钮
                            setTimeout(() => {{
                                const loginBtn = document.querySelector('button[type=""submit""]') ||
                                               document.querySelector('.login-btn') ||
                                               document.querySelector('#login-btn') ||
                                               Array.from(document.querySelectorAll('button')).find(btn => 
                                                   btn.textContent.includes('登录') || 
                                                   btn.textContent.includes('登陆') ||
                                                   btn.textContent.includes('Login')
                                               );
                                
                                if (loginBtn) {{
                                    loginBtn.click();
                                    return true;
                                }}
                            }}, 500);
                            
                            return true;
                        }}
                        return false;
                    }} catch (error) {{
                        console.error(error);
                        return false;
                    }}
                }})();
            ";
            
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
        
        /// <summary>
        /// 自动同意协议
        /// </summary>
        private async Task AutoAgreeAgreementAsync()
        {
            var script = @"
                (function() {
                    try {
                        const agreeBtn = document.querySelector('.agree-btn') ||
                                       document.querySelector('#agree-btn') ||
                                       document.querySelector('button.agree') ||
                                       Array.from(document.querySelectorAll('button')).find(btn => 
                                           btn.textContent.includes('同意') || 
                                           btn.textContent.includes('确认') ||
                                           btn.textContent.includes('Agree')
                                       );
                        
                        if (agreeBtn) {
                            agreeBtn.click();
                            return true;
                        }
                        return false;
                    } catch (error) {
                        return false;
                    }
                })();
            ";
            
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
        
        /// <summary>
        /// 自动关闭公告弹窗
        /// </summary>
        private async Task AutoCloseNoticeAsync()
        {
            var script = @"
                (function() {
                    try {
                        const closeBtn = document.querySelector('.close-btn') ||
                                       document.querySelector('.modal-close') ||
                                       document.querySelector('[class*=""close""]') ||
                                       document.querySelector('.popup-close');
                        
                        if (closeBtn) {
                            closeBtn.click();
                            return true;
                        }
                        return false;
                    } catch (error) {
                        return false;
                    }
                })();
            ";
            
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
        
        /// <summary>
        /// 获取余额
        /// </summary>
        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                var script = @"
                    (function() {
                        try {
                            const balanceElement = document.querySelector('.balance') ||
                                                  document.querySelector('.user-balance') ||
                                                  document.querySelector('[class*=""balance""]') ||
                                                  document.querySelector('[class*=""money""]');
                            
                            if (balanceElement) {
                                const text = balanceElement.innerText || balanceElement.textContent;
                                const match = text.match(/[\d,.]+/);
                                if (match) {
                                    return parseFloat(match[0].replace(/,/g, ''));
                                }
                            }
                            return -1;
                        } catch (error) {
                            return -1;
                        }
                    })();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                if (decimal.TryParse(result, out var balance))
                {
                    Log($"💰 余额: {balance}");
                    return balance;
                }
                
                return -1;
            }
            catch (Exception ex)
            {
                Log($"❌ 获取余额失败: {ex.Message}");
                return -1;
            }
        }
        
        /// <summary>
        /// 下注
        /// </summary>
        public async Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BaiShengVx3Plus.Shared.Models.BetStandardOrderList orders)
        {
            try
            {
                var issueId = orders.Count > 0 ? orders[0].IssueId : 0;
                var totalAmount = orders.GetTotalAmount();
                Log($"🎲 投注: 期号{issueId} 共{orders.Count}项 {totalAmount}元");
                
                // 🔥 云顶28平台的投注逻辑需要根据实际平台实现
                // 目前返回未实现状态
                Log($"⚠️ 云顶28平台投注功能待实现，需要根据实际平台页面结构实现");
                await Task.CompletedTask;
                return (false, "", "#云顶28平台投注功能待实现");  // 🔥 #前缀表示客户端错误

            }
            catch (Exception ex)
            {
                Log($"❌ 投注异常: {ex.Message}");
                return (false, "", $"#投注异常: {ex.Message}");  // 🔥 #前缀表示客户端异常
            }
        }
        
        /// <summary>
        /// 处理响应
        /// </summary>
        public void HandleResponse(ResponseEventArgs response)
        {
            // 云顶平台可以通过拦截响应获取更多信息
            // 例如：余额、订单状态等
        }
        
        /// <summary>
        /// 设置Cookie
        /// </summary>
        public void SetCookie(string cookie)
        {
            // WebView2 使用 CoreWebView2.CookieManager
            Log($"设置 Cookie (长度: {cookie?.Length ?? 0})");
        }
        
        /// <summary>
        /// 获取赔率列表（用于赔率显示窗口）
        /// </summary>
        public List<OddsInfo> GetOddsList()
        {
            Log("⚠️ 云顶28平台赔率功能待实现");
            return new List<OddsInfo>();
        }
        
        public Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
            int state = 0, int pageNum = 1, int pageCount = 20, string? beginDate = null, string? endDate = null, int timeout = 10)
        {
            Log("⚠️ 云顶28 平台暂不支持获取订单列表");
            return Task.FromResult<(bool, List<JObject>?, int, int, string)>((false, null, 0, 0, "平台暂不支持"));
        }
    }
}

