using Unit.Shared.Models;
using Unit.Shared.Helpers;
using BsBrowserClient.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BsBrowserClient.PlatformScripts
{
    /// <summary>
    /// 测试平台脚本 - 完全模拟通宝的投注逻辑
    /// 用于测试超时处理、订单查询、重试机制等
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
        
        // 🔥 模拟订单存储（用于查询订单）
        private readonly List<JObject> _mockOrders = new List<JObject>();
        
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
        /// 下注 - 完全模拟通宝的投注重试逻辑
        /// 包括：重试循环、超时处理、订单查询、封盘检查
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
                
                // 🎯 计算封盘时间（开奖时间 - 20秒）
                var openTime = BinggoTimeHelper.GetIssueOpenTime(issueId);
                var sealTime = openTime.AddSeconds(-20);  // 封盘时间
                _logCallback($"⏰ 期号{issueId} 开奖时间: {openTime:HH:mm:ss}, 封盘时间: {sealTime:HH:mm:ss}");
                
                // 🔥 重试机制：直到成功或超过封盘时间（完全模拟通宝）
                int retryCount = 0;
                const int maxRetries = 100;  // 最大重试次数（防止死循环）
                
                while (retryCount < maxRetries)
                {
                    var now = DateTime.Now;
                    
                    // 🔥 检查是否超过封盘时间
                    if (now > sealTime)
                    {
                        _logCallback($"⏰ 已超过封盘时间({sealTime:HH:mm:ss})，停止投注");
                        return (false, "", $"#已超过封盘时间，无法投注");
                    }
                    
                    retryCount++;
                    var remainingSeconds = (int)(sealTime - now).TotalSeconds;
                    _logCallback($"🔄 第{retryCount}次投注尝试 (距封盘还有{remainingSeconds}秒)");
                    
                    // 🎯 模拟投注请求（2秒超时）
                    _logCallback($"⏳ [测试平台] 模拟投注请求...");
                    await Task.Delay(2000);  // 2秒超时
                    
                    // ⏰ 情况：请求超时（模拟通宝的超时场景）
                    _logCallback($"⏰ [测试平台] 投注请求超时，开始验证订单...");
                    
                    // 🔍 先生成模拟订单（模拟服务器实际已经处理成功）
                    var orderId = $"TEST{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{_orderCounter++}";
                    var mockOrder = CreateMockOrder(orderId, issueId, (int)totalAmount, orders);
                    _mockOrders.Add(mockOrder);
                    
                    // 扣除余额
                    _currentBalance -= totalAmount;
                    
                    _logCallback($"🎲 [测试平台] 实际已生成订单: {orderId}（模拟服务器已处理）");
                    
                    // 🔍 查询未结算订单，检查是否已投注成功（模拟通宝的验证逻辑）
                    try
                    {
                        _logCallback($"🔍 查询未结算订单 (金额:{totalAmount}元)...");
                        var (success, orderList, _, _, errorMsg) = await GetLotMainOrderInfosAsync(
                            state: 0,           // 未结算
                            pageNum: 1,
                            pageCount: 20,
                            timeout: 3          // 查询订单超时3秒
                        );
                        
                        if (success && orderList != null && orderList.Count > 0)
                        {
                            _logCallback($"📋 查询到 {orderList.Count} 条未结算订单，开始匹配...");
                            
                            // 🔍 遍历订单，查找匹配的金额
                            foreach (var order in orderList)
                            {
                                var orderAmount = order["amount"]?.Value<int>() ?? 0;
                                var orderExpect = order["expect"]?.ToString() ?? "";
                                var orderUserData = order["userdata"]?.ToString() ?? "";
                                var foundOrderId = order["orderid"]?.ToString() ?? "";
                                
                                // 🎯 匹配条件：金额相同 && 期号相同
                                if (orderAmount == (int)totalAmount && orderExpect == issueId.ToString())
                                {
                                    _logCallback($"✅ 找到匹配订单: {foundOrderId}");
                                    _logCallback($"   期号: {orderExpect}");
                                    _logCallback($"   金额: {orderAmount}元");
                                    _logCallback($"   内容: {orderUserData}");
                                    _logCallback($"✅ 投注成功: {foundOrderId} (第{retryCount}次尝试)");
                                    _logCallback($"💰 剩余余额: {_currentBalance:F2} 元");
                                    
                                    // 返回成功（模拟通宝格式）
                                    var response = new
                                    {
                                        status = true,
                                        BettingNumber = foundOrderId,
                                        msg = "投注成功（超时后验证成功）",
                                        balance = _currentBalance
                                    };
                                    
                                    return (true, foundOrderId, JsonConvert.SerializeObject(response));
                                }
                            }
                            
                            _logCallback($"⚠️ 未找到匹配订单（可能还未同步）");
                        }
                        else
                        {
                            _logCallback($"⚠️ 查询订单失败或无订单: {errorMsg}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logCallback($"❌ 查询订单异常: {ex.Message}");
                    }
                    
                    // 🔥 未找到订单，等待1秒后重试
                    _logCallback($"⏳ 等待1秒后重试...");
                    await Task.Delay(1000);
                }
                
                // 🔥 超过最大重试次数
                _logCallback($"❌ 投注失败：超过最大重试次数");
                return (false, "", $"#投注失败：超过最大重试次数");
            }
            catch (TimeoutException ex)
            {
                _logCallback($"❌ 网络超时: {ex.Message}");
                return (false, "", "{\"status\":false,\"msg\":\"网络请求超时，远程服务器无响应\"}");
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 投注异常: {ex.Message}");
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
        /// 获取未结算的订单信息 - 模拟查不到订单，触发系统重试投注
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
                
                _logCallback($"📤 [测试平台] 获取订单列表: state={state}, page={pageNum}/{pageCount}, timeout={timeout}秒");
                
                // 模拟网络延迟
                await Task.Delay(100);
                
                // 🔥 测试场景：故意返回空订单列表，让外部继续走投注流程
                // 这样可以测试系统的重试投注逻辑
                _logCallback($"📋 [测试平台] 实际已生成 {_mockOrders.Count} 个订单，但故意返回空列表（测试重试逻辑）");
                _logCallback($"✅ [测试平台] 获取订单成功: 0条记录 (实际订单数={_mockOrders.Count})");
                
                // 返回空订单列表（模拟查不到订单的场景）
                return (true, new List<JObject>(), 0, 0, "");
                
                #region 原始逻辑（已禁用，保留供后续测试使用）
                
                // // 🔥 过滤订单（按状态）
                // var filteredOrders = _mockOrders
                //     .Where(o => (o["state"]?.Value<int>() ?? 0) == state)
                //     .ToList();
                // 
                // // 🔥 分页
                // int totalRecords = filteredOrders.Count;
                // int totalPages = (int)Math.Ceiling((double)totalRecords / pageCount);
                // 
                // var pagedOrders = filteredOrders
                //     .Skip((pageNum - 1) * pageCount)
                //     .Take(pageCount)
                //     .ToList();
                // 
                // _logCallback($"✅ [测试平台] 获取订单成功: {pagedOrders.Count}条记录 (总记录={totalRecords}, 总页数={totalPages})");
                // 
                // // 🔥 打印订单信息（用于调试）
                // for (int i = 0; i < pagedOrders.Count; i++)
                // {
                //     var order = pagedOrders[i];
                //     var orderId = order["orderid"]?.ToString() ?? "";
                //     var expect = order["expect"]?.ToString() ?? "";
                //     var amount = order["amount"]?.Value<int>() ?? 0;
                //     var userData = order["userdata"]?.ToString() ?? "";
                //     var orderState = order["state"]?.Value<int>() ?? -1;
                //     
                //     _logCallback($"   [{i + 1}] {orderId} | 期号:{expect} | 金额:{amount}元 | 内容:{userData.Trim()} | 状态:{orderState}");
                // }
                // 
                // return (true, pagedOrders, totalRecords, totalPages, "");
                
                #endregion
            }
            catch (Exception ex)
            {
                _logCallback($"❌ [测试平台] 获取订单异常: {ex.Message}");
                return (false, null, 0, 0, $"异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 创建模拟订单 - 完全模拟通宝的订单格式
        /// </summary>
        private JObject CreateMockOrder(string orderId, int issueId, int amount, BetStandardOrderList orders)
        {
            // 构建投注内容（模拟 userdata 格式）
            var userData = string.Join(",", orders.Select(o => 
            {
                var carName = o.Car.ToString().Replace("P", "");  // P1 → 1
                var playType = o.Play.ToString();  // 大/小/单/双
                var money = o.MoneySum;
                return $"{carName}{playType}{money}";  // 例如：1大100
            }));
            
            // 创建订单对象（模拟通宝的订单结构）
            var order = new JObject
            {
                ["orderid"] = orderId,
                ["expect"] = issueId.ToString(),
                ["amount"] = amount,
                ["userdata"] = userData,
                ["state"] = 0,  // 0=未结算, 1=已结算
                ["createtime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ["updatetime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            _logCallback($"📝 [测试平台] 创建模拟订单: {orderId} | 期号:{issueId} | 金额:{amount}元 | 内容:{userData}");
            
            return order;
        }
    }
}
