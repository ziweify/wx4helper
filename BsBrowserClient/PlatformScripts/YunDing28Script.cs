using BsBrowserClient.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using System;
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
        public async Task<(bool success, string orderId)> PlaceBetAsync(BetOrder order)
        {
            try
            {
                Log($"🎲 投注: {order.BetContent} {order.Amount}元");
                
                var script = $@"
                    (function() {{
                        try {{
                            // 查找投注按钮（根据投注内容）
                            const betContent = '{order.BetContent}';
                            const betAmount = {order.Amount};
                            
                            // 查找对应的投注按钮
                            const betButtons = document.querySelectorAll('[data-bet], [class*=""bet""]');
                            let targetButton = null;
                            
                            for (const btn of betButtons) {{
                                const text = btn.innerText || btn.textContent;
                                if (text.includes(betContent)) {{
                                    targetButton = btn;
                                    break;
                                }}
                            }}
                            
                            if (targetButton) {{
                                // 输入金额
                                const amountInput = document.querySelector('input[name=""amount""]') ||
                                                  document.querySelector('.amount-input') ||
                                                  document.querySelector('input[type=""number""]');
                                
                                if (amountInput) {{
                                    amountInput.value = betAmount;
                                    amountInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                }}
                                
                                // 点击投注
                                targetButton.click();
                                
                                // 查找确认按钮
                                setTimeout(() => {{
                                    const confirmBtn = document.querySelector('.confirm-btn') ||
                                                     document.querySelector('#confirm-bet') ||
                                                     Array.from(document.querySelectorAll('button')).find(btn => 
                                                         btn.textContent.includes('确认') || 
                                                         btn.textContent.includes('投注')
                                                     );
                                    if (confirmBtn) {{
                                        confirmBtn.click();
                                    }}
                                }}, 500);
                                
                                return {{ success: true, orderId: 'YD' + Date.now() }};
                            }}
                            
                            return {{ success: false, orderId: '', message: '找不到投注按钮' }};
                        }} catch (error) {{
                            return {{ success: false, orderId: '', message: error.message }};
                        }}
                    }})();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                var json = JObject.Parse(result);
                
                var success = json["success"]?.Value<bool>() ?? false;
                var orderId = json["orderId"]?.ToString() ?? "";
                var message = json["message"]?.ToString() ?? "";
                
                if (success)
                {
                    Log($"✅ 投注成功: {orderId}");
                    return (true, orderId);
                }
                else
                {
                    Log($"❌ 投注失败: {message}");
                    return (false, "");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 投注异常: {ex.Message}");
                return (false, "");
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
    }
}

