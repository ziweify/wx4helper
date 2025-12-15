using Unit.Shared.Models;
using zhaocaimao.Services.AutoBet.Browser.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace zhaocaimao.Services.AutoBet.Browser.PlatformScripts
{
    /// <summary>
    /// 通宝PC 平台脚本 - 参考 F5BotV2/TongBaoPcMember.cs
    /// </summary>
    public class TongBaoPcScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private bool _isLoggedIn = false;
        private string _sid = "";
        private string _uuid = "";
        private string _token = "";
        private string _region = "A";  // A,B,C,D盘类型
        private decimal _currentBalance = 0;
        
        public TongBaoPcScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
        }
        
        private void Log(string message) => _logCallback($"[通宝PC] {message}");
        
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Log("🔐 开始登录通宝PC...");
                // 通宝PC 与通宝类似，但使用不同的URL和参数提取方式
                // 参考 TongBaoScript，但使用 TongBaoPcMember 的参数提取逻辑
                return await Task.FromResult(false); // TODO: 实现登录逻辑
            }
            catch (Exception ex)
            {
                Log($"❌ 登录失败: {ex.Message}");
                return false;
            }
        }
        
        public async Task<decimal> GetBalanceAsync()
        {
            if (!_isLoggedIn) return -1;
            return _currentBalance;
        }
        
        public async Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BetStandardOrderList orders)
        {
            if (!_isLoggedIn) return (false, "", "未登录");
            return (false, "", "投注功能待实现");
        }
        
        public void HandleResponse(Services.ResponseEventArgs response)
        {
            try
            {
                // 参考 F5BotV2 TongBaoPcMember.cs 的响应拦截逻辑
                if (response.Url.Contains("/gettodaywinlost"))
                {
                    if (!string.IsNullOrEmpty(response.PostData))
                    {
                        var tokenMatch = Regex.Match(response.PostData, "token=([^&]+)");
                        var uuidMatch = Regex.Match(response.PostData, "uuid=([^&]+)");
                        if (tokenMatch.Success) _token = tokenMatch.Groups[1].Value;
                        if (uuidMatch.Success) _uuid = uuidMatch.Groups[1].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ 处理响应失败: {ex.Message}");
            }
        }
        
        public List<Models.OddsInfo> GetOddsList() => new List<Models.OddsInfo>();
    }
}

