using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models.AutoBet;
using SQLite;

namespace BaiShengVx3Plus.Services.AutoBet
{
    /// <summary>
    /// 自动投注服务 - 管理配置和浏览器
    /// </summary>
    public class AutoBetService : IDisposable
    {
        private SQLiteConnection? _db;
        private readonly ILogService _log;
        
        // 🔥 核心：配置ID → 浏览器客户端
        private readonly Dictionary<int, BrowserClient> _browsers = new();
        private int _nextPort = 9527; // 端口分配
        
        public AutoBetService(ILogService log)
        {
            _log = log;
        }
        
        /// <summary>
        /// 设置数据库连接（延迟初始化）
        /// </summary>
        public void SetDatabase(SQLiteConnection db)
        {
            _db = db;
            _db.CreateTable<BetConfig>();
            _db.CreateTable<BetOrderRecord>();
            EnsureDefaultConfig();
            _log.Info("AutoBet", "✅ 数据库已设置");
        }
        
        #region 配置管理
        
        public List<BetConfig> GetConfigs()
        {
            if (_db == null) return new List<BetConfig>();
            return _db.Table<BetConfig>().OrderBy(c => c.Id).ToList();
        }
        
        public BetConfig? GetConfig(int id)
        {
            if (_db == null) return null;
            return _db.Find<BetConfig>(id);
        }
        
        public void SaveConfig(BetConfig config)
        {
            if (_db == null) return;
            
            config.LastUpdateTime = DateTime.Now;
            
            if (config.Id > 0)
                _db.Update(config);
            else
            {
                _db.Insert(config);
                config.Id = (int)_db.ExecuteScalar<long>("SELECT last_insert_rowid()");
            }
            
            _log.Info("AutoBet", $"配置已保存: {config.ConfigName}");
        }
        
        public void DeleteConfig(int id)
        {
            if (_db == null) return;
            
            var config = GetConfig(id);
            if (config != null && !config.IsDefault)
            {
                StopBrowser(id);
                _db.Delete<BetConfig>(id);
                _log.Info("AutoBet", $"配置已删除: {config.ConfigName}");
            }
        }
        
        private void EnsureDefaultConfig()
        {
            if (_db == null) return;
            
            if (!_db.Table<BetConfig>().Any(c => c.IsDefault))
            {
                _db.Insert(new BetConfig
                {
                    ConfigName = "默认配置",
                    Platform = "YunDing28",
                    PlatformUrl = "https://www.yunding28.com",
                    IsDefault = true,
                    IsEnabled = true
                });
                _log.Info("AutoBet", "✅ 已创建默认配置");
            }
        }
        
        #endregion
        
        #region 浏览器控制
        
        /// <summary>
        /// 启动浏览器
        /// </summary>
        public async Task<bool> StartBrowser(int configId)
        {
            try
            {
                var config = GetConfig(configId);
                if (config == null)
                {
                    _log.Error("AutoBet", $"配置不存在: {configId}");
                    return false;
                }
                
                if (_browsers.ContainsKey(configId))
                {
                    _log.Warning("AutoBet", $"浏览器已启动: {config.ConfigName}");
                    return true;
                }
                
                _log.Info("AutoBet", $"🚀 启动浏览器: {config.ConfigName}");
                
                // 分配端口
                var port = _nextPort++;
                
                // 创建浏览器客户端
                var browserClient = new BrowserClient(configId);
                await browserClient.StartAsync(port, config.Platform, config.PlatformUrl);
                
                _browsers[configId] = browserClient;
                
                // 更新状态
                config.Status = "已启动";
                SaveConfig(config);
                
                // 自动登录
                if (config.AutoLogin && !string.IsNullOrEmpty(config.Username))
                {
                    var loginResult = await browserClient.SendCommandAsync("Login", new
                    {
                        username = config.Username,
                        password = config.Password
                    });
                    
                    config.Status = loginResult.Success ? "已登录" : "登录失败";
                    SaveConfig(config);
                }
                
                _log.Info("AutoBet", $"✅ 浏览器启动成功: {config.ConfigName}");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("AutoBet", $"启动浏览器失败: {configId}", ex);
                return false;
            }
        }
        
        /// <summary>
        /// 投注
        /// </summary>
        public async Task<BetResult> PlaceBet(int configId, BetOrder order)
        {
            if (!_browsers.TryGetValue(configId, out var browserClient))
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "浏览器未启动"
                };
            }
            
            var config = GetConfig(configId);
            if (config == null)
            {
                return new BetResult
                {
                    Success = false,
                    ErrorMessage = "配置不存在"
                };
            }
            
            _log.Info("AutoBet", $"📤 [{config.ConfigName}] 投注: {order.PlayType} {order.BetContent} {order.Amount}");
            
            // 发送投注命令
            var result = await browserClient.SendCommandAsync("PlaceBet", order);
            
            // 保存订单记录
            if (_db != null)
            {
                _db.Insert(new BetOrderRecord
                {
                    ConfigId = configId,
                    ConfigName = config.ConfigName,
                    Platform = config.Platform,
                    IssueId = order.IssueId ?? "",
                    PlayType = order.PlayType,
                    BetContent = order.BetContent,
                    Amount = order.Amount,
                    PlatformOrderId = result.OrderId,
                    Status = result.Success ? "成功" : "失败",
                    ErrorMessage = result.ErrorMessage,
                    CreateTime = DateTime.Now
                });
            }
            
            _log.Info("AutoBet", $"📥 [{config.ConfigName}] 投注结果: {(result.Success ? "✅ 成功" : "❌ 失败")}");
            
            return result;
        }
        
        /// <summary>
        /// 停止浏览器
        /// </summary>
        public void StopBrowser(int configId)
        {
            if (_browsers.TryGetValue(configId, out var browserClient))
            {
                browserClient.Dispose();
                _browsers.Remove(configId);
                
                var config = GetConfig(configId);
                if (config != null)
                {
                    config.Status = "已停止";
                    SaveConfig(config);
                }
                
                _log.Info("AutoBet", $"⏹️ 浏览器已停止: {config?.ConfigName}");
            }
        }
        
        /// <summary>
        /// 停止所有浏览器
        /// </summary>
        public void StopAllBrowsers()
        {
            foreach (var configId in _browsers.Keys.ToList())
            {
                StopBrowser(configId);
            }
        }
        
        #endregion
        
        public void Dispose()
        {
            StopAllBrowsers();
        }
    }
}
