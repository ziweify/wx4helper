using System;
using System.Threading.Tasks;
using Unit.Browser.Interfaces;
using Unit.Browser.Models;
using Unit.Browser.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Unit.Browser.Extensions
{
    /// <summary>
    /// 扩展命令执行器 - 支持业务命令（投注、登录等）
    /// </summary>
    public class ExtendedCommandExecutor : CommandExecutor
    {
        private WebView2? _webView;

        public new void SetWebView(WebView2 webView)
        {
            base.SetWebView(webView);
            _webView = webView;
        }

        public new bool SupportsCommand(string commandName)
        {
            // 先检查基类命令
            if (base.SupportsCommand(commandName))
                return true;

            // 再检查扩展命令
            return commandName switch
            {
                "投注" => true,
                "登录" => true,
                "获取余额" => true,
                "获取赔率" => true,
                "获取期号" => true,
                _ => false
            };
        }

        public new async Task<BrowserCommandResult> ExecuteAsync(BrowserCommand command)
        {
            // 先尝试基类命令
            if (base.SupportsCommand(command.Name))
            {
                return await base.ExecuteAsync(command);
            }

            // 执行扩展命令
            if (_webView?.CoreWebView2 == null)
            {
                return BrowserCommandResult.CreateFailure(
                    command.CommandId,
                    "WebView2 未初始化");
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                object? result = command.Name switch
                {
                    "投注" => await BetAsync(command.Parameters),
                    "登录" => await LoginAsync(command.Parameters),
                    "获取余额" => await GetBalanceAsync(),
                    "获取赔率" => await GetOddsAsync(),
                    "获取期号" => await GetCurrentIssueAsync(),
                    _ => throw new NotSupportedException($"不支持的命令: {command.Name}")
                };

                stopwatch.Stop();
                return BrowserCommandResult.CreateSuccess(
                    command.CommandId,
                    result,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return BrowserCommandResult.CreateFailure(
                    command.CommandId,
                    $"执行失败: {ex.Message}",
                    stopwatch.ElapsedMilliseconds);
            }
        }

        #region 业务命令实现

        /// <summary>
        /// 投注命令
        /// 参数格式: { "issueId": 123, "betType": "大", "amount": 10 }
        /// 或简化格式: "123大10"
        /// </summary>
        private async Task<object> BetAsync(object? parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentException("投注命令需要提供参数");
            }

            // 解析参数
            int issueId;
            string betType;
            decimal amount;

            if (parameters is string simpleFormat)
            {
                // 简化格式: "123大10"
                // 需要根据实际格式解析
                throw new NotImplementedException("简化格式投注暂未实现，请使用对象格式");
            }
            else if (parameters is JObject jobj)
            {
                issueId = jobj["issueId"]?.Value<int>() ?? 0;
                betType = jobj["betType"]?.Value<string>() ?? "";
                amount = jobj["amount"]?.Value<decimal>() ?? 0;
            }
            else
            {
                throw new ArgumentException("投注参数格式错误");
            }

            // 执行投注脚本（示例，需根据实际平台调整）
            var script = $@"
                (function() {{
                    // 这里是具体的投注逻辑
                    // 需要根据实际平台的DOM结构和API来实现
                    
                    // 示例：调用平台的投注API
                    if (window.platformBetApi) {{
                        return window.platformBetApi.bet({{
                            issueId: {issueId},
                            betType: '{betType}',
                            amount: {amount}
                        }});
                    }}
                    
                    // 如果没有API，通过DOM操作
                    // document.querySelector('.bet-button').click();
                    
                    return {{ success: false, message: '投注API未找到' }};
                }})();
            ";

            var resultJson = await _webView!.CoreWebView2.ExecuteScriptAsync(script);
            var result = JsonConvert.DeserializeObject<dynamic>(resultJson);

            return new
            {
                Success = result?.success ?? false,
                Message = result?.message ?? "投注完成",
                OrderId = result?.orderId,
                IssueId = issueId,
                BetType = betType,
                Amount = amount
            };
        }

        /// <summary>
        /// 登录命令
        /// 参数格式: { "username": "xxx", "password": "xxx" }
        /// </summary>
        private async Task<object> LoginAsync(object? parameters)
        {
            if (parameters is not JObject jobj)
            {
                throw new ArgumentException("登录命令需要提供用户名和密码");
            }

            var username = jobj["username"]?.Value<string>() ?? "";
            var password = jobj["password"]?.Value<string>() ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("用户名或密码不能为空");
            }

            // 执行登录脚本（示例）
            var script = $@"
                (function() {{
                    // 填充用户名密码
                    document.querySelector('#username').value = '{username}';
                    document.querySelector('#password').value = '{password}';
                    
                    // 点击登录按钮
                    document.querySelector('.login-btn').click();
                    
                    return {{ success: true }};
                }})();
            ";

            await _webView!.CoreWebView2.ExecuteScriptAsync(script);

            // 等待登录完成（简单等待，可以优化为检测登录状态）
            await Task.Delay(2000);

            return new { Success = true, Username = username };
        }

        /// <summary>
        /// 获取余额
        /// </summary>
        private async Task<object> GetBalanceAsync()
        {
            var script = @"
                (function() {
                    // 从页面获取余额（根据实际DOM调整）
                    var balanceEl = document.querySelector('.balance');
                    if (balanceEl) {
                        var text = balanceEl.innerText;
                        var match = text.match(/[\d\.]+/);
                        return match ? parseFloat(match[0]) : 0;
                    }
                    return 0;
                })();
            ";

            var resultJson = await _webView!.CoreWebView2.ExecuteScriptAsync(script);
            var balance = JsonConvert.DeserializeObject<decimal>(resultJson);

            return new { Balance = balance };
        }

        /// <summary>
        /// 获取赔率
        /// </summary>
        private async Task<object> GetOddsAsync()
        {
            var script = @"
                (function() {
                    // 获取赔率数据（根据实际DOM调整）
                    var odds = [];
                    document.querySelectorAll('.odds-item').forEach(function(item) {
                        odds.push({
                            name: item.querySelector('.name').innerText,
                            value: parseFloat(item.querySelector('.value').innerText)
                        });
                    });
                    return odds;
                })();
            ";

            var resultJson = await _webView!.CoreWebView2.ExecuteScriptAsync(script);
            var odds = JsonConvert.DeserializeObject<dynamic>(resultJson);

            return odds ?? new { };
        }

        /// <summary>
        /// 获取当前期号
        /// </summary>
        private async Task<object> GetCurrentIssueAsync()
        {
            var script = @"
                (function() {
                    // 获取期号（根据实际DOM调整）
                    var issueEl = document.querySelector('.current-issue');
                    if (issueEl) {
                        var text = issueEl.innerText;
                        var match = text.match(/\d+/);
                        return match ? parseInt(match[0]) : 0;
                    }
                    return 0;
                })();
            ";

            var resultJson = await _webView!.CoreWebView2.ExecuteScriptAsync(script);
            var issueId = JsonConvert.DeserializeObject<int>(resultJson);

            return new { IssueId = issueId };
        }

        #endregion
    }
}

