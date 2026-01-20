using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using YongLiSystem.Infrastructure.Api;
using YongLiSystem.Models.BotApi.V1;  // 使用 BotApi V1 版本
using YongLiSystem.Services;
using YongLiSystem.Views;

namespace YongLiSystem.Services.Auth
{
    /// <summary>
    /// 认证服务
    /// 封装登录逻辑，提供高级认证功能
    /// </summary>
    public class AuthService
    {
        private readonly LoggingService _loggingService;
        private readonly BotApiV1 _botApi;
        
        public AuthService(LoggingService loggingService)
        {
            _loggingService = loggingService;
            _botApi = BotApiV1.GetInstance();
            
            // 订阅账号失效事件
            _botApi.OnAccountInvalid += HandleAccountInvalid;
            _botApi.OnAccountOffTime += HandleAccountOffTime;
        }
        
        /// <summary>
        /// 显示登录窗口并执行登录
        /// </summary>
        public async Task<bool> ShowLoginDialogAsync()
        {
            try
            {
                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        var viewModel = loginForm.ViewModel;
                        if (viewModel != null)
                        {
                            return await LoginAsync(viewModel.Username, viewModel.Password);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error("认证服务", $"显示登录窗口失败: {ex.Message}");
                MessageBox.Show($"登录失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return false;
        }
        
        /// <summary>
        /// 执行登录
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _loggingService.Warn("认证服务", "用户名或密码为空");
                return false;
            }
            
            try
            {
                _loggingService.Info("认证服务", $"正在登录: {username}");
                
                var response = await _botApi.LoginAsync(username, password);
                
                if (response.Code == 0 && response.Data != null)
                {
                    _loggingService.Info("认证服务", $"登录成功: {username}");
                    
                    // 验证 Token 是否存在
                    if (string.IsNullOrEmpty(response.Data.Token))
                    {
                        _loggingService.Error("认证服务", "登录成功但 Token 为空");
                        AuthGuard.ClearAuthentication();
                        return false;
                    }
                    
                    _loggingService.Info("认证服务", $"Token 已获取，长度: {response.Data.Token.Length}");
                    
                    // 设置认证标记（防破解）
                    AuthGuard.SetAuthenticated(response.Data.Token);
                    
                    _loggingService.Info("认证服务", "认证标记已设置");
                    
                    return true;
                }
                else
                {
                    _loggingService.Error("认证服务", $"登录失败: {response.Msg}");
                    AuthGuard.ClearAuthentication();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error("认证服务", $"登录异常: {ex.Message}");
                AuthGuard.ClearAuthentication();
                return false;
            }
        }
        
        /// <summary>
        /// 检查是否已登录，如果未登录则显示登录窗口
        /// </summary>
        public async Task<bool> EnsureLoggedInAsync()
        {
            if (_botApi.IsLoggedIn())
            {
                return true;
            }
            
            return await ShowLoginDialogAsync();
        }
        
        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        public ApiUser? GetCurrentUser()
        {
            return _botApi.LoginApiResponse?.Data;
        }
        
        /// <summary>
        /// 获取当前 Token
        /// </summary>
        public string? GetToken()
        {
            return _botApi.GetToken();
        }
        
        /// <summary>
        /// 登出
        /// </summary>
        public void Logout()
        {
            _botApi.Logout();
            AuthGuard.ClearAuthentication();
            _loggingService.Info("认证服务", "已登出");
        }
        
        /// <summary>
        /// 处理账号失效事件
        /// </summary>
        private void HandleAccountInvalid(string message)
        {
            _loggingService.Error("认证服务", $"账号失效: {message}");
            
            // 必须在 UI 线程显示对话框
            if (Application.MessageLoop)
            {
                MessageBox.Show(message, "账号失效", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (Application.OpenForms.Count > 0)
            {
                Application.OpenForms[0].Invoke(new Action(() =>
                {
                    MessageBox.Show(message, "账号失效", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }
        
        /// <summary>
        /// 处理账号过期事件
        /// </summary>
        private void HandleAccountOffTime(string message)
        {
            _loggingService.Error("认证服务", $"账号过期: {message}");
            
            // 必须在 UI 线程显示对话框并停止程序运行
            if (Application.MessageLoop)
            {
                MessageBox.Show(message, "账号过期", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            else if (Application.OpenForms.Count > 0)
            {
                Application.OpenForms[0].Invoke(new Action(() =>
                {
                    MessageBox.Show(message, "账号过期", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }));
            }
        }
    }
}
