using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Services.WeChat
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
        private readonly IDatabaseService _databaseService;
        private readonly ILogService _logService;

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
            IDatabaseService databaseService,
            ILogService logService)
        {
            _loaderService = loaderService;
            _socketClient = socketClient;
            _userInfoService = userInfoService;
            _contactDataService = contactDataService;
            _databaseService = databaseService;
            _logService = logService;
        }

        /// <summary>
        /// 连接并初始化（完整流程）
        /// </summary>
        public async Task<bool> ConnectAndInitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logService.Info("WeChatService", "========== 开始连接和初始化流程 ==========");

                // 1. 启动微信（或注入已运行的微信）
                if (!await LaunchOrInjectWeChatAsync(cancellationToken))
                {
                    UpdateState(ConnectionState.Failed, "启动或注入微信失败");
                    return false;
                }

                // 2. 连接 Socket
                if (!await ConnectSocketAsync(cancellationToken))
                {
                    UpdateState(ConnectionState.Failed, "Socket 连接失败");
                    return false;
                }

                // 3. 获取用户信息（带重试）
                var userInfo = await RefreshUserInfoAsync(maxRetries: -1, retryInterval: 2000, cancellationToken);
                if (userInfo == null || string.IsNullOrEmpty(userInfo.Wxid))
                {
                    UpdateState(ConnectionState.Failed, "获取用户信息失败");
                    return false;
                }

                // 4. 初始化数据库
                _logService.Info("WeChatService", $"初始化数据库，wxid: {userInfo.Wxid}");
                await _databaseService.InitializeBusinessDatabaseAsync(userInfo.Wxid);
                _contactDataService.SetCurrentWxid(userInfo.Wxid);

                // 5. 获取联系人（带重试，等待数据库句柄初始化）
                UpdateState(ConnectionState.FetchingContacts, "正在获取联系人列表...");
                
                // 等待一段时间让 C++ 端初始化数据库句柄
                await Task.Delay(1000, cancellationToken);
                
                var contacts = await RefreshContactsAsync(cancellationToken);
                _logService.Info("WeChatService", $"✓ 联系人获取成功，共 {contacts.Count} 个");

                // 6. 完成
                UpdateState(ConnectionState.Connected, "连接成功");
                _logService.Info("WeChatService", "========== 连接和初始化完成 ==========");

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
            CancellationToken cancellationToken = default)
        {
            UpdateState(ConnectionState.FetchingUserInfo, "正在获取用户信息（等待登录）...");

            int attempt = 0;
            while (maxRetries == -1 || attempt < maxRetries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _logService.Info("WeChatService", $"尝试获取用户信息 (尝试 {attempt + 1}{(maxRetries == -1 ? "" : $"/{maxRetries}")})");

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

                            _userInfoService.UpdateUserInfo(userInfo);
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
        /// 刷新联系人列表
        /// </summary>
        public async Task<List<WxContact>> RefreshContactsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logService.Info("WeChatService", "开始刷新联系人列表");

                var contactsDoc = await _socketClient.SendAsync<JsonDocument>("GetContacts", 30000);
                if (contactsDoc != null)
                {
                    var contacts = await _contactDataService.ProcessContactsAsync(contactsDoc.RootElement);
                    _logService.Info("WeChatService", $"✓ 联系人刷新成功，返回 {contacts.Count} 个联系人");
                    return contacts;
                }

                _logService.Warning("WeChatService", "联系人数据为空");
                return new List<WxContact>();
            }
            catch (Exception ex)
            {
                _logService.Error("WeChatService", "刷新联系人失败", ex);
                return new List<WxContact>();
            }
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

            var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WeixinX.dll");
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

