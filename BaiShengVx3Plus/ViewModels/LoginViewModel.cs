using BaiShengVx3Plus.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BaiShengVx3Plus.ViewModels
{
    /// <summary>
    /// 登录页面ViewModel
    /// 🔥 简化：直接使用 BoterApi 单例（完全参考 F5BotV2）
    /// </summary>
    public partial class LoginViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _rememberPassword;

        public LoginViewModel()
        {
            // 🔥 不再需要依赖注入
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
                // 🔥 直接使用 BoterApi 单例（完全参考 F5BotV2）
                var api = Services.Api.BoterApi.GetInstance();
                var response = await api.LoginAsync(Username, Password);

                if (response.Code == 0)
                {
                    Console.WriteLine($"✅ 登录成功: {Username}");
                    // 触发登录成功事件
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = $"登录失败: {response.Msg}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"登录异常: {ex.Message}";
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

