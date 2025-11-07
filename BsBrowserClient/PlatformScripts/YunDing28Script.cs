using BsBrowserClient.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 云顶28 平台脚本 - 使用 WebView2
    /// </summary>
    public class YunDing28Script : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        
        public YunDing28Script(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
        }
        
        /// <summary>
        /// 登录
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                _logCallback($"开始登录: {username}");
                
                // 注入登录脚本
                var script = $@"
                    (function() {{
                        try {{
                            // 查找用户名输入框
                            const usernameInput = document.querySelector('input[name=""username""]') || 
                                                  document.querySelector('input[type=""text""]') ||
                                                  document.querySelector('#username');
                            
                            // 查找密码输入框
                            const passwordInput = document.querySelector('input[name=""password""]') || 
                                                  document.querySelector('input[type=""password""]') ||
                                                  document.querySelector('#password');
                            
                            // 查找登录按钮
                            const loginButton = document.querySelector('button[type=""submit""]') ||
                                               document.querySelector('.login-btn') ||
                                               document.querySelector('#login-btn');
                            
                            if (usernameInput && passwordInput && loginButton) {{
                                usernameInput.value = '{username}';
                                passwordInput.value = '{password}';
                                
                                // 触发事件
                                usernameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                
                                // 延迟点击登录按钮
                                setTimeout(() => loginButton.click(), 500);
                                
                                return {{ success: true, message: '登录脚本执行成功' }};
                            }} else {{
                                return {{ success: false, message: '找不到登录元素' }};
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
                
                return success;
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
                _logCallback("获取余额...");
                
                // 注入获取余额脚本
                var script = @"
                    (function() {
                        try {
                            // 常见的余额显示元素
                            const balanceElement = document.querySelector('.balance') ||
                                                  document.querySelector('.user-balance') ||
                                                  document.querySelector('#balance') ||
                                                  document.querySelector('[class*=""balance""]');
                            
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
                
                var success = json["success"]?.Value<bool>() ?? false;
                var balance = json["balance"]?.Value<decimal>() ?? 0;
                
                _logCallback(success ? $"✅ 余额: {balance}" : $"❌ 获取余额失败");
                
                return balance;
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 获取余额失败: {ex.Message}");
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
                _logCallback($"开始投注: {order.BetContent} {order.Amount}");
                
                // 注入投注脚本
                var script = $@"
                    (function() {{
                        try {{
                            // 1. 选择投注类型
                            const betTypeButton = document.querySelector('[data-type=""{order.BetContent}""]') ||
                                                 document.querySelector('.bet-option[data-value=""{order.BetContent}""]');
                            
                            if (!betTypeButton) {{
                                return {{ success: false, message: '找不到投注类型按钮: {order.BetContent}' }};
                            }}
                            
                            betTypeButton.click();
                            
                            // 2. 输入金额
                            const amountInput = document.querySelector('input[name=""amount""]') ||
                                               document.querySelector('.bet-amount') ||
                                               document.querySelector('#amount');
                            
                            if (amountInput) {{
                                amountInput.value = '{order.Amount}';
                                amountInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                            }}
                            
                            // 3. 点击确认按钮
                            const confirmButton = document.querySelector('.confirm-bet') ||
                                                 document.querySelector('#confirm-btn') ||
                                                 document.querySelector('button[type=""submit""]');
                            
                            if (confirmButton) {{
                                confirmButton.click();
                                return {{ success: true, orderId: 'ORDER_' + Date.now(), message: '投注成功' }};
                            }} else {{
                                return {{ success: false, message: '找不到确认按钮' }};
                            }}
                        }} catch (error) {{
                            return {{ success: false, message: error.message }};
                        }}
                    }})();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
                var json = JObject.Parse(result);
                
                var success = json["success"]?.Value<bool>() ?? false;
                var orderId = json["orderId"]?.ToString() ?? "";
                var message = json["message"]?.ToString() ?? "";
                
                _logCallback(success ? $"✅ {message}" : $"❌ {message}");
                
                return (success, orderId);
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 投注失败: {ex.Message}");
                return (false, "");
            }
        }
        
        /// <summary>
        /// 处理拦截到的响应
        /// </summary>
        public void HandleResponse(ResponseEventArgs response)
        {
            try
            {
                // 根据 URL 判断响应类型
                if (response.Url.Contains("/api/bet") || response.Url.Contains("/bet/submit"))
                {
                    // 投注响应
                    if (!string.IsNullOrEmpty(response.Context))
                    {
                        try
                        {
                            var json = JObject.Parse(response.Context);
                            var code = json["code"]?.Value<int>() ?? -1;
                            var message = json["message"]?.ToString() ?? "";
                            
                            if (code == 0 || code == 200)
                            {
                                var orderId = json["data"]?["orderId"]?.ToString() ?? "";
                                _logCallback($"✅ 投注成功: {orderId}");
                            }
                            else
                            {
                                _logCallback($"❌ 投注失败: {message}");
                            }
                        }
                        catch
                        {
                            // JSON 解析失败，忽略
                        }
                    }
                }
                else if (response.Url.Contains("/api/balance") || response.Url.Contains("/user/info"))
                {
                    // 余额响应
                    if (!string.IsNullOrEmpty(response.Context))
                    {
                        try
                        {
                            var json = JObject.Parse(response.Context);
                            var balance = json["data"]?["balance"]?.Value<decimal>() ?? 0;
                            _logCallback($"💰 余额更新: {balance}");
                        }
                        catch
                        {
                            // JSON 解析失败，忽略
                        }
                    }
                }
                else if (response.Url.Contains("/api/lottery") || response.Url.Contains("/api/issue"))
                {
                    // 开奖结果
                    if (!string.IsNullOrEmpty(response.Context))
                    {
                        try
                        {
                            var json = JObject.Parse(response.Context);
                            var issueId = json["data"]?["issueId"]?.ToString() ?? "";
                            var result = json["data"]?["result"]?.ToString() ?? "";
                            _logCallback($"🎲 开奖: {issueId} = {result}");
                        }
                        catch
                        {
                            // JSON 解析失败，忽略
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 响应处理失败: {ex.Message}");
            }
        }
    }
}
