using Unit.Shared.Models;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 不使用盘口 脚本 - 参考 F5BotV2/NoneSite.cs
    /// 此脚本不执行任何操作，仅用于占位
    /// </summary>
    public class NoneSiteScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        
        public NoneSiteScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
        }
        
        private void Log(string message) => _logCallback($"[不使用盘口] {message}");
        
        public async Task<bool> LoginAsync(string username, string password)
        {
            Log("⚠️ 不使用盘口，跳过登录");
            return await Task.FromResult(true); // 直接返回成功
        }
        
        public async Task<decimal> GetBalanceAsync()
        {
            Log("⚠️ 不使用盘口，无法获取余额");
            return await Task.FromResult(0m);
        }
        
        public async Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BetStandardOrderList orders)
        {
            Log("⚠️ 不使用盘口，跳过投注");
            return await Task.FromResult((true, "NONE", "不使用盘口，已跳过"));
        }
        
        public void HandleResponse(ResponseEventArgs response)
        {
            // 不使用盘口，不处理任何响应
        }
        
        public List<OddsInfo> GetOddsList() => new List<OddsInfo>();
        
        public Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
            int state = 0, int pageNum = 1, int pageCount = 20, string? beginDate = null, string? endDate = null, int timeout = 10)
        {
            return Task.FromResult<(bool, List<JObject>?, int, int, string)>((false, null, 0, 0, "未使用盘口"));
        }
    }
}

