using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using zhaocaimao.Models;
using zhaocaimao.Contracts;

namespace zhaocaimao.Services.WeChat
{
    /// <summary>
    /// 微信应用服务实现（Application Service）
    /// 负责编排业务流程：启动微信→注入DLL→连接Socket→获取用户信息→获取联系人
    /// </summary>
    public class WeChatService : IWeChatService
    {
        private readonly IWeChatLoaderService _loaderService;
        private readonly IWeixinSocketClient _socketClient;
        private readonly IUserInfoService _userInfoService;
        private readonly IContactDataService _contactDataService;
        private readonly ILogService _logService;
        private readonly IConfigurationService _configService; // 🔥 配置服务（用于读取系统设置）

        private ConnectionState _currentState = ConnectionState.Disconnected;
        private readonly object _stateLock = new object();

        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

        /// <summary>
        /// 当前连接状态
        /// </summary>
        public ConnectionState CurrentState
        {
            get
            {
                lock (_stateLock)
                {
                    return _currentState;
                }
            }
        }

        public WeChatService(
            IWeChatLoaderService loaderService,
            IWeixinSocketClient socketClient,
            IUserInfoService userInfoService,
            IContactDataService contactDataService,
            ILogService logService,
            IConfigurationService configService) // 🔥 注入配置服务
        {
            _loaderService = loaderService;
            _socketClient = socketClient;
            _userInfoService = userInfoService;
            _contactDataService = contactDataService;
            _logService = logService;
            _configService = configService; // 🔥 存储配置服务引用
        }

        /// <summary>
        /// 连接并初始化（完整流程，智能判断是否需要启动/注入微信）
        /// </summary>
        /// <param name="forceRestart">是否强制重新启动/注入（默认 false，会先尝试直接连接）</param>
        public async Task<bool> ConnectAndInitializeAsync(bool forceRestart = false, CancellationToken cancellationToken = default)
        {
            try
            {
                _logService.Info("WeChatService", "========== 开始连接和初始化流程 ==========");
                // ✅ 示例：获取管理模式设置
                bool isRunModeAdin = _configService.GetIsRunModeAdmin();
                bool IsRunModeDev = _configService.GetIsRunModeDev();
                _logService.Info("WeChatService", $"📋 当前运行模式: 管理模式={isRunModeAdin}, 开发模式={IsRunModeDev}");
                

                UpdateState(ConnectionState.Connecting, "正在连接...");

                bool needsLaunchOrInject = forceRestart;

                // 🔥 智能判断：先尝试直接连接（最快）
                if (!forceRestart)
                {
                    _logService.Info("WeChatService", "🔍 步骤1: 尝试直接连接（假设微信已运行且已注入）");
                    
                    bool quickConnected = await _socketClient.ConnectAsync("127.0.0.1", 6328, 2000);
                    
                    if (quickConnected)
                    {
                        _logService.Info("WeChatService", "✓ 快速连接成功！微信已就绪");
                        needsLaunchOrInject = false;
                    }
                    else
                    {
                        _logService.Info("WeChatService", "✗ 快速连接失败，需要启动或注入微信");
                        needsLaunchOrInject = true;
                    }
                }

                // 🔥 如果需要，启动或注入微信
                if (needsLaunchOrInject)
                {
                    _logService.Info("WeChatService", "🚀 步骤2: 启动或注入微信");
                    UpdateState(ConnectionState.LaunchingWeChat, "正在启动微信...");

                    if (!await LaunchOrInjectWeChatAsync(cancellationToken))
                    {
                        UpdateState(ConnectionState.Failed, "启动或注入微信失败");
                        return false;
                    }

                    // 启动/注入后，连接 Socket
                    _logService.Info("WeChatService", "🔌 步骤3: 连接 Socket 服务器");
                    UpdateState(ConnectionState.Connecting, "正在连接 Socket...");

                    if (!await ConnectSocketAsync(cancellationToken))
                    {
                        UpdateState(ConnectionState.Failed, "Socket 连接失败");
                        return false;
                    }
                }

                // 3. 获取用户信息（带重试）
                _logService.Info("WeChatService", "👤 步骤4: 获取用户信息");
                UpdateState(ConnectionState.FetchingUserInfo, "正在获取用户信息...");
                
                // 🔥 传递开发模式参数，用于模拟返回数据
                WxUserInfo? userInfo = await RefreshUserInfoAsync(
                    maxRetries: -1, 
                    retryInterval: 2000, 
                    cancellationToken, 
                    isRunModeDev: IsRunModeDev);
                
                if (userInfo == null || string.IsNullOrEmpty(userInfo.Wxid))
                {
                    UpdateState(ConnectionState.Failed, "获取用户信息失败");
                    return false;
                }

                // 4. 初始化数据库
                _logService.Info("WeChatService", $"💾 步骤5: 更新用户信息: {userInfo.Wxid}");
                UpdateState(ConnectionState.InitializingDatabase, "正在初始化...");

                // 🔥 更新用户信息（会自动同步 wxid 到 ContactDataService）
                // 🔥 数据库初始化由 VxMain 的 UserInfoService_UserInfoUpdated 事件自动处理
                _userInfoService.UpdateUserInfo(userInfo);

                // 5. 获取联系人（智能选择：快速连接用单次尝试，新启动用重试）
                _logService.Info("WeChatService", "📇 步骤6: 获取联系人列表");
                UpdateState(ConnectionState.FetchingContacts, "正在获取联系人列表...");
                
                List<WxContact> contacts;

                // 🔥 如果快速连接成功（微信已打开），使用快速重试（最多2次，间隔500ms）
                // 如果启动/注入了微信，使用重试机制（等待数据库句柄初始化）
                if (needsLaunchOrInject)
                {
                    //如果是开发模式
                    _logService.Info("WeChatService", "🚀 快速连接模式：使用快速重试获取联系人（微信已就绪）");
                    // 🔥 快速连接模式下，如果第一次失败，快速重试一次（不等待太久）
                    contacts = await RefreshContactsAsync(
                        maxRetries: 2,  // 🔥 快速连接模式：最多重试2次（第一次 + 1次重试）
                        retryInterval: 500,  // 🔥 快速重试间隔：500ms（不等待太久）
                        filterType: ContactFilterType.群组,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    _logService.Info("WeChatService", "⏳ 新启动模式：使用重试机制获取联系人（等待数据库句柄初始化）");
                    // 🔥 等待并重试获取联系人（直到数据库句柄初始化完成）
                    if(!IsRunModeDev)
                    {
                        contacts = await RefreshContactsAsync(
                                                maxRetries: 5,  // 🔥 减少重试次数（从10次减少到5次）
                                                retryInterval: 1000,  // 🔥 减少重试间隔（从2000ms减少到1000ms）
                                                filterType: ContactFilterType.群组,
                                                cancellationToken: cancellationToken);
                    }
                    else
                    {
                        contacts = new List<WxContact>();
                        contacts.Add(new WxContact() { Wxid = "wxid_111111", Account = "111111", Nickname = "n111111" });
                        contacts.Add(new WxContact() { Wxid = "wxid_222222", Account = "222222", Nickname = "n222222" });
                        contacts.Add(new WxContact() { Wxid = "wxid_333333", Account = "333333", Nickname = "n333333" });
                        contacts.Add(new WxContact() { Wxid = "wxid_444444", Account = "444444", Nickname = "n444444" });
                        contacts.Add(new WxContact() { Wxid = "wxid_555555", Account = "555555", Nickname = "n555555" });
                    }
                    
                }
                
                _logService.Info("WeChatService", $"✓ 联系人获取成功，共 {contacts.Count} 个");

                // 6. 完成
                UpdateState(ConnectionState.Connected, "连接成功");
                _logService.Info("WeChatService", "========== ✅ 连接和初始化完成 ==========");

                return true;
            }
            catch (OperationCanceledException)
            {
                _logService.Info("WeChatService", "连接被用户取消");
                UpdateState(ConnectionState.Disconnected, "连接已取消");
                return false;
            }
            catch (Exception ex)
            {
                _logService.Error("WeChatService", "连接和初始化失败", ex);
                UpdateState(ConnectionState.Failed, $"发生错误: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 刷新用户信息（带重试机制）
        /// </summary>
        public async Task<WxUserInfo?> RefreshUserInfoAsync(
            int maxRetries = 10,
            int retryInterval = 2000,
            CancellationToken cancellationToken = default,
            bool isRunModeDev = false)
        {
            UpdateState(ConnectionState.FetchingUserInfo, "正在获取用户信息（等待登录）...");

            int attempt = 0;
            while (maxRetries == -1 || attempt < maxRetries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _logService.Info("WeChatService", $"尝试获取用户信息 (尝试 {attempt + 1}{(maxRetries == -1 ? "" : $"/{maxRetries}")})");

                    // 🔥 开发模式：返回模拟数据
                    if (isRunModeDev)
                    {
                        var userInfo = new WxUserInfo()
                        {
                            Account = "kaice",
                            Nickname = "开测",
                            Mobile = "111111",
                            Wxid = "wxid_kaice"
                        };
                        _logService.Info("WeChatService", $"✓ [开发模式]用户信息获取成功: {userInfo.Nickname} ({userInfo.Wxid})");
                        return userInfo;
                    }

                    var userInfoDoc = await _socketClient.SendAsync<JsonDocument>("GetUserInfo", 10000);
                    if (userInfoDoc != null)
                    {
                        var root = userInfoDoc.RootElement;
                        var wxid = root.GetProperty("wxid").GetString();

                        if (!string.IsNullOrEmpty(wxid))
                        {
                            var userInfo = new WxUserInfo
                            {
                                Wxid = wxid,
                                Nickname = root.TryGetProperty("nickname", out var nick) ? nick.GetString() ?? "" : "",
                                Account = root.TryGetProperty("account", out var acc) ? acc.GetString() ?? "" : "",
                                Mobile = root.TryGetProperty("mobile", out var mob) ? mob.GetString() ?? "" : "",
                                Avatar = root.TryGetProperty("avatar", out var ava) ? ava.GetString() ?? "" : "",
                                DataPath = root.TryGetProperty("data_path", out var dp) ? dp.GetString() ?? "" : "",
                                CurrentDataPath = root.TryGetProperty("current_data_path", out var cdp) ? cdp.GetString() ?? "" : "",
                                DbKey = root.TryGetProperty("db_key", out var dbk) ? dbk.GetString() ?? "" : ""
                            };

                            // ✅ 只返回用户信息，由调用者决定是否更新（避免重复调用）
                            _logService.Info("WeChatService", $"✓ 用户信息获取成功: {userInfo.Nickname} ({userInfo.Wxid})");
                            return userInfo;
                        }
                    }

                    _logService.Warning("WeChatService", "用户信息为空，等待登录...");
                }
                catch (Exception ex)
                {
                    _logService.Warning("WeChatService", $"获取用户信息失败 (尝试 {attempt + 1}): {ex.Message}");
                }

                attempt++;
                await Task.Delay(retryInterval, cancellationToken);
            }

            _logService.Error("WeChatService", "获取用户信息失败：超过最大重试次数");
            return null;
        }

        /// <summary>
        /// 刷新联系人列表（带重试机制和过滤）
        /// </summary>
        /// <param name="maxRetries">最大重试次数（默认1次，不重试）</param>
        /// <param name="retryInterval">重试间隔（毫秒，默认2000ms）</param>
        /// <param name="filterType">过滤类型（默认全部）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task<List<WxContact>> RefreshContactsAsync(
            int maxRetries = 1,
            int retryInterval = 2000,
            ContactFilterType filterType = ContactFilterType.全部,
            CancellationToken cancellationToken = default)
        {
            List<WxContact>? result = null;
            Exception? lastException = null;
            
            try
            {
                /// 如果是开发模式
                if(_configService.GetIsRunModeDev())
                {
                    result = new List<WxContact>();
                    result.Add(new WxContact() { Wxid = "wxid_111111@wx.com", Account = "111111", Nickname = "n111111" });
                    result.Add(new WxContact() { Wxid = "wxid_222222@wx.com", Account = "222222", Nickname = "n222222" });
                    result.Add(new WxContact() { Wxid = "wxid_333333@wx.com", Account = "333333", Nickname = "n333333" });
                    result.Add(new WxContact() { Wxid = "wxid_444444", Account = "444444", Nickname = "n444444" });
                    result.Add(new WxContact() { Wxid = "wxid_555555", Account = "555555", Nickname = "n555555" });
                    return result;
                }

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    try
                    {
                        string retryInfo = maxRetries > 1 ? $" (尝试 {attempt}/{maxRetries})" : "";
                        _logService.Info("WeChatService", $"开始刷新联系人列表{retryInfo}");

                        var contactsDoc = await _socketClient.SendAsync<JsonDocument>("GetContacts", 30000);
                        
                        if (contactsDoc != null)
                        {
                            _logService.Debug("WeChatService", $"收到数据类型: {contactsDoc.RootElement.ValueKind}");
                            
                            // 🔥 使用静态方法解析 JSON
                            result = Services.Contact.ContactDataService.ParseContactsFromJson(contactsDoc.RootElement);
                            _logService.Info("WeChatService", $"✓ 联系人解析成功，共 {result.Count} 个");
                            
                            break; // 成功，退出重试循环
                        }

                        _logService.Warning("WeChatService", $"联系人数据为空{retryInfo}");
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        string retryInfo = maxRetries > 1 ? $" (尝试 {attempt}/{maxRetries})" : "";
                        _logService.Warning("WeChatService", $"刷新联系人失败{retryInfo}: {ex.Message}");
                    }
                    
                    // 🔥 如果不是最后一次尝试，等待后重试
                    if (attempt < maxRetries)
                    {
                        _logService.Info("WeChatService", $"等待 {retryInterval}ms 后重试...");
                        await Task.Delay(retryInterval, cancellationToken);
                    }
                }
                
                // 🔥 所有重试都失败
                if (result == null)
                {
                    if (lastException != null)
                    {
                        _logService.Error("WeChatService", "刷新联系人失败：超过最大重试次数", lastException);
                    }
                    else
                    {
                        _logService.Error("WeChatService", "获取联系人失败：数据为空（超过最大重试次数）");
                    }
                    
                    result = new List<WxContact>();
                }
            }
            finally
            {
                // 🔥 无论成功或失败，都触发事件通知 UI（应用过滤）
                if (result != null)
                {
                    result = await _contactDataService.ProcessContactsAsync(result, filterType);
                }
            }
            
            return result ?? new List<WxContact>();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                _logService.Info("WeChatService", "正在断开连接...");

                _socketClient.Disconnect();
                _userInfoService.ClearUserInfo();

                UpdateState(ConnectionState.Disconnected, "已断开连接");
                _logService.Info("WeChatService", "✓ 已断开连接");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("WeChatService", "断开连接时发生错误", ex);
            }
        }

        // ========================================
        // 私有辅助方法
        // ========================================

        /// <summary>
        /// 启动或注入微信
        /// </summary>
        private async Task<bool> LaunchOrInjectWeChatAsync(CancellationToken cancellationToken)
        {
            UpdateState(ConnectionState.LaunchingWeChat, "正在启动微信...");

            // 🔥 使用固定路径：bin\release\net8.0-windows\WeixinX.dll
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            
            if (string.IsNullOrEmpty(basePath))
            {
                _logService.Error("WeChatService", "无法获取应用程序基础路径");
                return false;
            }
            
            var dllPath = Path.Combine(basePath, "WeixinX.dll");
            
            _logService.Info("WeChatService", $"DLL 路径: {dllPath}");
            
            if (!File.Exists(dllPath))
            {
                _logService.Error("WeChatService", $"找不到 WeixinX.dll: {dllPath}");
                return false;
            }

            var (success, errorMessage) = await _loaderService.LaunchOrInjectAsync(dllPath, cancellationToken);

            if (!success)
            {
                _logService.Error("WeChatService", $"启动或注入微信失败: {errorMessage}");
                return false;
            }

            _logService.Info("WeChatService", "✓ 微信启动/注入成功");
            return true;
        }

        /// <summary>
        /// 连接 Socket 服务器
        /// </summary>
        private async Task<bool> ConnectSocketAsync(CancellationToken cancellationToken)
        {
            UpdateState(ConnectionState.ConnectingSocket, "正在连接 Socket 服务器...");

            // 等待 DLL 初始化 Socket 服务器
            await Task.Delay(2000, cancellationToken);

            var connected = await _socketClient.ConnectAsync("127.0.0.1", 6328, 5000);

            if (!connected)
            {
                _logService.Error("WeChatService", "Socket 连接失败");
                return false;
            }

            _logService.Info("WeChatService", "✓ Socket 连接成功");
            return true;
        }

        /// <summary>
        /// 更新状态并触发事件
        /// </summary>
        private void UpdateState(ConnectionState newState, string? message = null, Exception? error = null)
        {
            ConnectionState oldState;
            lock (_stateLock)
            {
                oldState = _currentState;
                _currentState = newState;
            }

            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
            {
                OldState = oldState,
                NewState = newState,
                Message = message,
                Error = error
            });
        }
    }
}

