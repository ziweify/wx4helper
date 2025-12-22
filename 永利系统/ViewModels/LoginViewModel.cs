using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Mvvm;
using 永利系统.Infrastructure.Api;
using 永利系统.Models.Api;
using 永利系统.Services;
using 永利系统.Services.Auth;

namespace 永利系统.ViewModels
{
    /// <summary>
    /// 登录 ViewModel
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly LoggingService _loggingService;
        
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _isRememberPassword;
        private string _errorMessage = string.Empty;
        private bool _isBusy;
        private string _busyMessage = string.Empty;
        
        public LoginViewModel(AuthService authService, LoggingService loggingService)
        {
            _authService = authService;
            _loggingService = loggingService;
            
            // 订阅账号失效事件
            var api = BoterApi.GetInstance();
            api.OnAccountInvalid += HandleAccountInvalid;
            api.OnAccountOffTime += HandleAccountOffTime;
        }
        
        #region 属性
        
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CanLogin));
                }
            }
        }
        
        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CanLogin));
                }
            }
        }
        
        /// <summary>
        /// 记住密码
        /// </summary>
        public bool IsRememberPassword
        {
            get => _isRememberPassword;
            set
            {
                if (_isRememberPassword != value)
                {
                    _isRememberPassword = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// 是否忙碌
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CanLogin));
                }
            }
        }
        
        /// <summary>
        /// 忙碌消息
        /// </summary>
        public string BusyMessage
        {
            get => _busyMessage;
            set
            {
                if (_busyMessage != value)
                {
                    _busyMessage = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// 是否可以登录
        /// </summary>
        public bool CanLogin => !string.IsNullOrWhiteSpace(Username) && 
                                !string.IsNullOrWhiteSpace(Password) && 
                                !IsBusy;
        
        #endregion
        
        #region 命令
        
        /// <summary>
        /// 执行登录（公开方法供外部调用）
        /// </summary>
        public async Task ExecuteLoginAsync()
        {
            await LoginAsync();
        }
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 登录成功事件
        /// </summary>
        public event EventHandler? LoginSucceeded;
        
        #endregion
        
        #region 方法
        
        /// <summary>
        /// 执行登录
        /// </summary>
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            IsBusy = true;
            BusyMessage = "正在登录...";
            
            try
            {
                var success = await _authService.LoginAsync(Username, Password);
                
                if (success)
                {
                    _loggingService.Info("登录", $"登录成功: {Username}");
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    var api = BoterApi.GetInstance();
                    var response = api.LoginApiResponse;
                    
                    if (response != null)
                    {
                        if (response.Code == BoterApi.VERIFY_SIGN_OFFTIME)
                        {
                            ErrorMessage = "账号过期";
                        }
                        else if (response.Code == BoterApi.VERIFY_SIGN_INVALID)
                        {
                            ErrorMessage = "账号失效! 请重新登录\r\n请检查是否有在其他地方登录导致本次失效!";
                        }
                        else
                        {
                            ErrorMessage = $"登录失败: {response.Msg}";
                        }
                    }
                    else
                    {
                        ErrorMessage = "登录失败: 未知错误";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"登录异常: {ex.Message}";
                _loggingService.Error("登录", $"登录异常: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                BusyMessage = string.Empty;
            }
        }
        
        /// <summary>
        /// 处理账号失效事件
        /// </summary>
        private void HandleAccountInvalid(string message)
        {
            ErrorMessage = message;
        }
        
        /// <summary>
        /// 处理账号过期事件
        /// </summary>
        private void HandleAccountOffTime(string message)
        {
            ErrorMessage = message;
        }
        
        #endregion
    }
}

