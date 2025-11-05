using BaiShengVx3Plus.Core;
using BaiShengVx3Plus.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BaiShengVx3Plus.ViewModels
{
    /// <summary>
    /// 登录页面ViewModel
    /// </summary>
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _rememberPassword;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 登录成功事件
        /// </summary>
        public event EventHandler? LoginSucceeded;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            IsBusy = true;
            BusyMessage = "正在登录...";

            try
            {
                var (success, message, user) = await _authService.LoginAsync(Username, Password);

                if (success)
                {
                    // 触发登录成功事件
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"登录失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                BusyMessage = string.Empty;
            }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) && 
                   !string.IsNullOrWhiteSpace(Password) && 
                   !IsBusy;
        }

        partial void OnUsernameChanged(string value)
        {
            LoginCommand.NotifyCanExecuteChanged();
        }

        partial void OnPasswordChanged(string value)
        {
            LoginCommand.NotifyCanExecuteChanged();
        }
    }
}

