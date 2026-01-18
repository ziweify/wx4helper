using Unit.Shared.Models;
using Unit.Shared.Helpers;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 测试平台脚本 - 用于开发测试
    /// 所有操作都模拟成功，不发送真实请求
    /// </summary>
    public class TestPlatformScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        
        // 模拟登录状态
        private bool _isLoggedIn = false;
        private string _username = "";
        private decimal _currentBalance = 10000m;  // 模拟余额：1万元
        
        // 模拟订单号计数器
        private int _orderCounter = 1;
        
        // 模拟赔率数据
        private readonly Dictionary<string, float> _oddsValues = new Dictionary<string, float>();
        
        public TestPlatformScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            
            // 初始化模拟赔率数据
            InitializeOdds();
        }
        
        /// <summary>
        /// 初始化赔率数据（模拟）
        /// </summary>
        private void InitializeOdds()
        {
            // 平码赔率（大小单双尾大尾小合单合双）
            var cars = new[] { "平一", "平二", "平三", "平四", "平五" };
            var plays = new[] { "大", "小", "单", "双", "尾大", "尾小", "合单", "合双" };
            
            foreach (var car in cars)
            {
                foreach (var play in plays)
                {
                    _oddsValues[$"{car}{play}"] = 1.97f;
                }
            }
            
            // 和值赔率
            var sumPlays = new[] { "大", "小", "单", "双", "尾大", "尾小" };
            foreach (var play in sumPlays)
            {
                _oddsValues[$"和值{play}"] = 1.97f;
            }
            
            // 龙虎赔率
            _oddsValues["龙"] = 1.97f;
            _oddsValues["虎"] = 1.97f;
        }
        
        /// <summary>
        /// 登录 - 直接返回登录成功
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                _logCallback($"🔐 [测试平台] 开始登录: {username}");
                
                // 模拟登录延迟
                await Task.Delay(500);
                
                _isLoggedIn = true;
                _username = username;
                
                _logCallback($"✅ [测试平台] 登录成功！用户: {username}");
                _logCallback($"💰 [测试平台] 模拟余额: {_currentBalance:F2} 元");
                
                return true;
            }
            catch (Exception ex)
            {
                _logCallback($"❌ [测试平台] 登录失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 获取余额 - 返回模拟余额
        /// </summary>
        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                if (!_isLoggedIn)
                {
                    _logCallback("❌ [测试平台] 未登录，无法获取余额");
                    return -1;
                }
                
                _logCallback($"💰 [测试平台] 获取余额...");
                
                // 模拟查询延迟
                await Task.Delay(200);
                
                _logCallback($"💰 [测试平台] 当前余额: {_currentBalance:F2} 元");
                return _currentBalance;
            }
            catch (Exception ex)
            {
                _logCallback($"❌ [测试平台] 获取余额失败: {ex.Message}");
                return -1;
            }
        }
        
        /// <summary>
        /// 下注 - 直接返回投注成功（假投注）
        /// </summary>
        public async Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BetStandardOrderList orders)
        {
            try
            {
                if (!_isLoggedIn)
                {
                    _logCallback("❌ [测试平台] 未登录，无法下注");
                    return (false, "", "{\"status\":false,\"msg\":\"未登录\"}");
                }
                
                var issueId = orders.Count > 0 ? orders[0].IssueId : 0;
                var totalAmount = orders.GetTotalAmount();
                _logCallback($"🎲 [测试平台] 开始投注: 期号{issueId} 共{orders.Count}项 {totalAmount}元");
                
                // 打印投注内容
                foreach (var order in orders)
                {
                    var playType = order.Play.ToString();
                    var carName = order.Car.ToString();
                    var money = order.MoneySum;
                    _logCallback($"   - {carName} {playType}: {money}元");
                }
                
                // 检查余额
                if (_currentBalance < totalAmount)
                {
                    _logCallback($"❌ [测试平台] 余额不足: 当前{_currentBalance:F2}元 < 需要{totalAmount}元");
                    return (false, "", "{\"status\":false,\"msg\":\"余额不足\"}");
                }
                
                // 模拟投注延迟
                await Task.Delay(300);
                
                // 扣除余额
                _currentBalance -= totalAmount;
                
                // 生成模拟订单号
                var orderId = $"TEST{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{_orderCounter++}";
                
                _logCallback($"✅ [测试平台] 投注成功: {orderId}");
                _logCallback($"💰 [测试平台] 剩余余额: {_currentBalance:F2} 元");
                
                // 返回模拟响应（参考通宝格式）
                var response = new
                {
                    status = true,
                    BettingNumber = orderId,
                    msg = "投注成功",
                    balance = _currentBalance
                };
                
                return (true, orderId, JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                _logCallback($"❌ [测试平台] 投注异常: {ex.Message}");
                return (false, "", $"{{\"status\":false,\"msg\":\"投注异常: {ex.Message}\"}}");
            }
        }
        
        /// <summary>
        /// 处理响应 - 测试平台不需要拦截
        /// </summary>
        public void HandleResponse(ResponseEventArgs response)
        {
            // 测试平台不需要拦截，所有操作都是模拟的
            // 可以在这里记录日志，观察其他平台的响应
        }
        
        /// <summary>
        /// 获取赔率列表
        /// </summary>
        public List<OddsInfo> GetOddsList()
        {
            var oddsList = new List<OddsInfo>();
            
            // 平码赔率
            var cars = new[] { "平一", "平二", "平三", "平四", "平五" };
            var plays = new[] 
            { 
                ("大", BetPlayEnum.大), 
                ("小", BetPlayEnum.小), 
                ("单", BetPlayEnum.单), 
                ("双", BetPlayEnum.双),
                ("尾大", BetPlayEnum.尾大),
                ("尾小", BetPlayEnum.尾小),
                ("合单", BetPlayEnum.合单),
                ("合双", BetPlayEnum.合双)
            };
            
            for (int i = 0; i < cars.Length; i++)
            {
                var carEnum = (CarNumEnum)(i + 1);  // P1=1, P2=2, ...
                
                foreach (var (playName, playEnum) in plays)
                {
                    var name = $"{cars[i]}{playName}";
                    var odds = _oddsValues.ContainsKey(name) ? _oddsValues[name] : 1.97f;
                    oddsList.Add(new OddsInfo(carEnum, playEnum, name, $"TEST_{name}", odds));
                }
            }
            
            // 和值赔率
            var sumPlays = new[] 
            { 
                ("大", BetPlayEnum.大), 
                ("小", BetPlayEnum.小), 
                ("单", BetPlayEnum.单), 
                ("双", BetPlayEnum.双),
                ("尾大", BetPlayEnum.尾大),
                ("尾小", BetPlayEnum.尾小)
            };
            
            foreach (var (playName, playEnum) in sumPlays)
            {
                var name = $"和值{playName}";
                var odds = _oddsValues.ContainsKey(name) ? _oddsValues[name] : 1.97f;
                oddsList.Add(new OddsInfo(CarNumEnum.P总, playEnum, name, $"TEST_{name}", odds));
            }
            
            // 龙虎赔率
            oddsList.Add(new OddsInfo(CarNumEnum.P总, BetPlayEnum.龙, "龙", "TEST_龙", 1.97f));
            oddsList.Add(new OddsInfo(CarNumEnum.P总, BetPlayEnum.虎, "虎", "TEST_虎", 1.97f));
            
            _logCallback($"✅ [测试平台] 返回赔率列表: {oddsList.Count}项");
            
            return oddsList;
        }
        
        /// <summary>
        /// 获取未结算的订单信息 - 返回模拟订单
        /// </summary>
        public async Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
            int state = 0, 
            int pageNum = 1, 
            int pageCount = 20,
            string? beginDate = null,
            string? endDate = null,
            int timeout = 10)
        {
            try
            {
                if (!_isLoggedIn)
                {
                    _logCallback("❌ [测试平台] 未登录，无法获取订单");
                    return (false, null, 0, 0, "未登录");
                }
                
                _logCallback($"📋 [测试平台] 获取订单列表: state={state}, page={pageNum}");
                
                // 模拟查询延迟
                await Task.Delay(200);
                
                // 返回空订单列表（测试平台没有真实订单）
                var orders = new List<JObject>();
                
                _logCallback($"✅ [测试平台] 获取订单成功: 0条记录");
                
                return (true, orders, 0, 0, "");
            }
            catch (Exception ex)
            {
                _logCallback($"❌ [测试平台] 获取订单异常: {ex.Message}");
                return (false, null, 0, 0, $"异常: {ex.Message}");
            }
        }
    }
}
