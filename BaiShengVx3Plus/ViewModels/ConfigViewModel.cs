using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace BaiShengVx3Plus.ViewModels
{
    /// <summary>
    /// 配置视图模型（专门用于 UI 数据绑定）
    /// 职责：
    /// 1. 继承 ViewModelBase（已实现 INotifyPropertyChanged）
    /// 2. 作为 UI 和 Service 之间的桥梁
    /// 3. UI 绑定到这个 ViewModel，ViewModel 调用 Service
    /// </summary>
    public partial class ConfigViewModel : ViewModelBase
    {
        private readonly IConfigurationService _configService;
        
        // ========================================
        // 构造函数
        // ========================================
        
        public ConfigViewModel(IConfigurationService configService)
        {
            _configService = configService;
            
            // 订阅 Service 的变更事件，自动更新 UI
            _configService.ConfigurationChanged += OnConfigurationChanged;
            
            // 🔥 订阅账号失效事件（参考 F5BotV2 BoterServices.cs Line 1117-1136）
            var api = Services.Api.BoterApi.GetInstance();
            api.OnAccountInvalid += HandleAccountInvalid;
            api.OnAccountOffTime += HandleAccountOffTime;
        }

        // ========================================
        // 可绑定属性（UI 双向绑定到这些属性）
        // ========================================

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _bsUserName = string.Empty;

        [ObservableProperty]
        private string _bsUserPass = string.Empty;

        [ObservableProperty]
        private bool _isRememberPassword;

        /// <summary>
        /// 收单开关（UI 双向绑定）
        /// </summary>
        public bool IsOrdersTaskingEnabled
        {
            get => _configService.GetIsOrdersTaskingEnabled();
            set
            {
                if (_configService.GetIsOrdersTaskingEnabled() != value)
                {
                    _configService.SetIsOrdersTaskingEnabled(value);
                    // Service 会触发事件，然后调用 OnConfigurationChanged
                }
            }
        }
        
        /// <summary>
        /// 自动投注开关（UI 双向绑定）
        /// </summary>
        public bool IsAutoBetEnabled
        {
            get => _configService.GetIsAutoBetEnabled();
            set
            {
                if (_configService.GetIsAutoBetEnabled() != value)
                {
                    _configService.SetIsAutoBetEnabled(value);
                    // Service 会触发事件，然后调用 OnConfigurationChanged
                }
            }
        }
        
        /// <summary>
        /// 提前封盘秒数（UI 双向绑定）
        /// </summary>
        public int SealSecondsAhead
        {
            get => _configService.GetSealSecondsAhead();
            set
            {
                if (_configService.GetSealSecondsAhead() != value)
                {
                    _configService.SetSealSecondsAhead(value);
                    // Service 会触发事件，然后调用 OnConfigurationChanged
                }
            }
        }
        
        /// <summary>
        /// 🔧 开发模式：当前会员（UI 双向绑定）
        /// </summary>
        public string RunDevCurrentMember
        {
            get => _configService.GetRunDevCurrentMember();
            set
            {
                if (_configService.GetRunDevCurrentMember() != value)
                {
                    _configService.SetRunDevCurrentMember(value);
                }
            }
        }
        
        /// <summary>
        /// 🔧 开发模式：发送消息内容（UI 双向绑定）
        /// </summary>
        public string RunDevSendMessage
        {
            get => _configService.GetRunDevSendMessage();
            set
            {
                if (_configService.GetRunDevSendMessage() != value)
                {
                    _configService.SetRunDevSendMessage(value);
                }
            }
        }
        
        // ========================================
        // 事件处理（ViewModelBase 已实现 INotifyPropertyChanged）
        // ========================================
        
        /// <summary>
        /// 当 Service 的配置变更时，通知 UI 更新
        /// </summary>
        private void OnConfigurationChanged(object? sender, ConfigurationChangedEventArgs e)
        {
            // 根据变更的属性名，触发对应的属性通知
            switch (e.PropertyName)
            {
                case nameof(IsOrdersTaskingEnabled):
                    OnPropertyChanged(nameof(IsOrdersTaskingEnabled));
                    break;
                    
                case nameof(IsAutoBetEnabled):
                    OnPropertyChanged(nameof(IsAutoBetEnabled));
                    break;
                    
                case nameof(SealSecondsAhead):
                    OnPropertyChanged(nameof(SealSecondsAhead));
                    break;
                    
                case "RunDevCurrentMember":
                    OnPropertyChanged(nameof(RunDevCurrentMember));
                    break;
                    
                case "RunDevSendMessage":
                    OnPropertyChanged(nameof(RunDevSendMessage));
                    break;
            }
        }

        //--绑定名利给--
        public event EventHandler? LoginSucceeded;
        
        /// <summary>
        /// 🔥 处理账号失效（被其他地方登录）- 参考 F5BotV2 OnBoterInvalid
        /// </summary>
        private void HandleAccountInvalid(string message)
        {
            // 🔥 必须在 UI 线程显示对话框并停止程序运行（参考 F5BotV2 BoterServices.cs Line 1130-1136）
            if (Application.MessageLoop)
            {
                MessageBox.Show(message, "账号失效", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                // 🔥 停止程序运行（参考 F5BotV2，返回1表示停止）
                Application.Exit();
            }
            else
            {
                // 如果不在 UI 线程，使用同步方式
                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0].Invoke(new Action(() =>
                    {
                        MessageBox.Show(message, "账号失效", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
                        Application.Exit();
                    }));
                }
                else
                {
                    // 如果没有打开的窗口，直接退出
                    MessageBox.Show(message, "账号失效", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
        }
        
        /// <summary>
        /// 🔥 处理账号过期 - 参考 F5BotV2 OnBoterOffTime
        /// </summary>
        private void HandleAccountOffTime(string message)
        {
            // 🔥 必须在 UI 线程显示对话框并停止程序运行（参考 F5BotV2 BoterServices.cs Line 1117-1125）
            if (Application.MessageLoop)
            {
                MessageBox.Show(message, "账号过期", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                // 🔥 停止程序运行（参考 F5BotV2，返回1表示停止）
                Application.Exit();
            }
            else
            {
                // 如果不在 UI 线程，使用同步方式
                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0].Invoke(new Action(() =>
                    {
                        MessageBox.Show(message, "账号过期", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
                        Application.Exit();
                    }));
                }
                else
                {
                    // 如果没有打开的窗口，直接退出
                    MessageBox.Show(message, "账号过期", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
        }

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
                var response = await api.LoginAsync(BsUserName, BsUserPass);

                if (response.Code == 0)
                {
                    Console.WriteLine($"✅ 登录成功: {BsUserName}");
                    // 触发登录成功事件
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    // 🔥 检查账号失效（事件已在 BoterApi 中触发，这里只显示错误消息）
                    if (response.Code == Services.Api.BoterApi.VERIFY_SIGN_OFFTIME)
                    {
                        ErrorMessage = "账号过期";
                    }
                    else if (response.Code == Services.Api.BoterApi.VERIFY_SIGN_INVALID)
                    {
                        ErrorMessage = "账号失效! 请重新登录\r\n请检查是否有在其他地方登录导致本次失效!";
                    }
                    else
                    {
                        ErrorMessage = $"登录失败: {response.Msg}";
                    }
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
            return !string.IsNullOrWhiteSpace(BsUserName) &&
                   !string.IsNullOrWhiteSpace(BsUserPass) &&
                   !IsBusy;
        }
    }
}

