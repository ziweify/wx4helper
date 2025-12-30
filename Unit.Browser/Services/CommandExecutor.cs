using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Unit.Browser.Interfaces;
using Unit.Browser.Models;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;

namespace Unit.Browser.Services
{
    /// <summary>
    /// 默认命令执行器
    /// </summary>
    public class CommandExecutor : Interfaces.ICommandExecutor
    {
        private WebView2? _webView;
        
        public void SetWebView(WebView2 webView)
        {
            _webView = webView;
        }

        public bool SupportsCommand(string commandName)
        {
            return commandName switch
            {
                "导航" or "重新导航" => true,
                "刷新页面" => true,
                "当前网址" => true,
                "获取Cookie" => true,
                "执行脚本" => true,
                "获取HTML" => true,
                "获取标题" => true,
                _ => false
            };
        }

        public async Task<BrowserCommandResult> ExecuteAsync(BrowserCommand command)
        {
            if (_webView?.CoreWebView2 == null)
            {
                return BrowserCommandResult.CreateFailure(
                    command.CommandId, 
                    "WebView2 未初始化");
            }

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                object? result = command.Name switch
                {
                    "导航" or "重新导航" => await NavigateAsync(command.Parameters),
                    "刷新页面" => await RefreshAsync(),
                    "当前网址" => GetCurrentUrl(),
                    "获取Cookie" => await GetCookiesAsync(),
                    "执行脚本" => await ExecuteScriptAsync(command.Parameters),
                    "获取HTML" => await GetHtmlAsync(),
                    "获取标题" => await GetTitleAsync(),
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

        #region 命令实现

        private async Task<bool> NavigateAsync(object? parameters)
        {
            if (parameters is not string url || string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("导航命令需要提供URL参数");
            }

            _webView!.CoreWebView2.Navigate(url);
            
            // 等待导航完成（简单实现，可以优化）
            await Task.Delay(500);
            return true;
        }

        private async Task<bool> RefreshAsync()
        {
            _webView!.CoreWebView2.Reload();
            await Task.Delay(300);
            return true;
        }

        private string GetCurrentUrl()
        {
            return _webView!.CoreWebView2.Source;
        }

        private async Task<object> GetCookiesAsync()
        {
            var cookieManager = _webView!.CoreWebView2.CookieManager;
            var cookies = await cookieManager.GetCookiesAsync(_webView.CoreWebView2.Source);
            
            var cookieDict = cookies.ToDictionary(
                c => c.Name,
                c => c.Value
            );
            
            return cookieDict;
        }

        private async Task<string> ExecuteScriptAsync(object? parameters)
        {
            if (parameters is not string script || string.IsNullOrWhiteSpace(script))
            {
                throw new ArgumentException("执行脚本命令需要提供脚本内容");
            }

            var result = await _webView!.CoreWebView2.ExecuteScriptAsync(script);
            return result;
        }

        private async Task<string> GetHtmlAsync()
        {
            var html = await _webView!.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
            // ExecuteScriptAsync 返回的是 JSON 字符串，需要解析
            return JsonConvert.DeserializeObject<string>(html) ?? string.Empty;
        }

        private async Task<string> GetTitleAsync()
        {
            var title = await _webView!.CoreWebView2.ExecuteScriptAsync("document.title");
            return JsonConvert.DeserializeObject<string>(title) ?? string.Empty;
        }

        #endregion
    }
}

