using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace BaiShengVx3Plus.ViewModels
{
    /// <summary>
    /// 配置视图模型（专门用于 UI 数据绑定）
    /// 职责：
    /// 1. 实现 INotifyPropertyChanged（支持 WinForms 数据绑定）
    /// 2. 作为 UI 和 Service 之间的桥梁
    /// 3. UI 绑定到这个 ViewModel，ViewModel 调用 Service
    /// </summary>
    public partial class ConfigViewModel : ViewModelBase
        , INotifyPropertyChanged
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
        }

        // ========================================
        // 可绑定属性（UI 双向绑定到这些属性）
        // ========================================

        public string ErrorMessage;

        public string BsUserName
        { get; set; }

        public string BsUserPass
        { get; set; }

        public bool IsRememberPassword
        { get; set; }

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
        
        // ========================================
        // INotifyPropertyChanged 实现
        // ========================================
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        // ========================================
        // 事件处理
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
            }
        }

        //--绑定名利给--
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
                var response = await api.LoginAsync(BsUserName, BsUserPass);

                if (response.Code == 0)
                {
                    Console.WriteLine($"✅ 登录成功: {BsUserName}");
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
            return !string.IsNullOrWhiteSpace(BsUserName) &&
                   !string.IsNullOrWhiteSpace(BsUserPass) &&
                   !IsBusy;
        }
    }
}

