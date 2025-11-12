using BaiShengVx3Plus.Core;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BaiShengVx3Plus.ViewModels
{
    /// <summary>
    /// 设置界面ViewModel
    /// 🔥 简化：直接使用 BoterApi 单例
    /// 🔥 运行模式：使用 IConfigurationService 管理（支持自动保存）
    /// </summary>
    public partial class SettingViewModel : ViewModelBase
    {
        private readonly IInsUserService _insUserService;
        private readonly IConfigurationService _configService;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private ObservableCollection<InsUser> _insUsers = new();

        [ObservableProperty]
        private InsUser? _selectedInsUser;

        [ObservableProperty]
        private string _statusMessage = "就绪";

        [ObservableProperty]
        private int _onlineCount;

        [ObservableProperty]
        private int _totalCount;
        
        // ========================================
        // 运行模式配置（绑定到 ConfigurationService）
        // ========================================
        
        /// <summary>
        /// 管理模式（UI 双向绑定）
        /// </summary>
        public bool Is管理模式
        {
            get => _configService.GetIsRunModeAdmin();
            set
            {
                if (_configService.GetIsRunModeAdmin() != value)
                {
                    _configService.SetIsRunModeAdmin(value);
                    OnPropertyChanged(nameof(Is管理模式));
                }
            }
        }
        
        /// <summary>
        /// 开发模式（UI 双向绑定）
        /// </summary>
        public bool Is开发模式
        {
            get => _configService.GetIsRunModeDev();
            set
            {
                if (_configService.GetIsRunModeDev() != value)
                {
                    _configService.SetIsRunModeDev(value);
                    OnPropertyChanged(nameof(Is开发模式));
                }
            }
        }
        
        /// <summary>
        /// 老板模式（UI 双向绑定）
        /// </summary>
        public bool Is老板模式
        {
            get => _configService.GetIsRunModeBoss();
            set
            {
                if (_configService.GetIsRunModeBoss() != value)
                {
                    _configService.SetIsRunModeBoss(value);
                    OnPropertyChanged(nameof(Is老板模式));
                }
            }
        }

        public SettingViewModel(
            IInsUserService insUserService,
            IConfigurationService configService)
        {
            _insUserService = insUserService;
            _configService = configService;
            
            // 🔥 订阅配置变更事件（同步 UI）
            _configService.ConfigurationChanged += OnConfigurationChanged;
            
            // 🔥 从 BoterApi 获取当前登录用户信息
            var api = Services.Api.BoterApi.GetInstance();
            if (api.LoginApiResponse != null && api.LoginApiResponse.Data != null)
            {
                CurrentUser = new User
                {
                    UserName = api.LoginApiResponse.Data.Username ?? "未知用户",
                    IsOnline = true
                };
            }
            
            _ = LoadDataAsync();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private async Task LoadDataAsync()
        {
            IsBusy = true;
            BusyMessage = "正在加载数据...";

            try
            {
                var users = await _insUserService.GetAllUsersAsync();
                InsUsers = new ObservableCollection<InsUser>(users);
                TotalCount = InsUsers.Count;
                OnlineCount = InsUsers.Count(u => u.IsOnline);
                StatusMessage = $"共 {TotalCount} 个用户，在线 {OnlineCount} 个";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载数据失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                BusyMessage = string.Empty;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        [RelayCommand]
        private void AddInsUser()
        {
            // TODO: 打开添加用户对话框
            StatusMessage = "添加用户功能开发中...";
        }

        [RelayCommand]
        private async Task DeleteInsUserAsync(InsUser? user)
        {
            if (user == null) return;

            try
            {
                var result = await _insUserService.DeleteUserAsync(user.Id);
                if (result)
                {
                    InsUsers.Remove(user);
                    StatusMessage = $"已删除用户: {user.UserName}";
                    TotalCount = InsUsers.Count;
                    OnlineCount = InsUsers.Count(u => u.IsOnline);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"删除失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            StatusMessage = "设置功能开发中...";
        }

        [RelayCommand]
        private void OpenSubscriptionManagement()
        {
            StatusMessage = "订单管理功能开发中...";
        }

        [RelayCommand]
        private void OpenWeChatDataCard()
        {
            StatusMessage = "微信数据卡管理功能开发中...";
        }

        [RelayCommand]
        private void OpenHelp()
        {
            StatusMessage = "帮助功能开发中...";
        }

        [RelayCommand]
        private void ModifyPassword()
        {
            StatusMessage = "修改密码功能开发中...";
        }

        [RelayCommand]
        private void Recharge()
        {
            StatusMessage = "充值功能开发中...";
        }

        [RelayCommand]
        private void TransferPoints()
        {
            StatusMessage = "转分功能开发中...";
        }

        [RelayCommand]
        private void Logout()
        {
            // 🔥 清除 BoterApi 的登录状态（需要添加 public setter）
            // 暂时无法直接清除，BoterApi 需要添加 Logout 方法
            
            // 触发登出事件
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? LogoutRequested;
        
        // ========================================
        // 配置变更事件处理
        // ========================================
        
        /// <summary>
        /// 当 ConfigurationService 中的配置变更时，通知 UI 更新
        /// </summary>
        private void OnConfigurationChanged(object? sender, ConfigurationChangedEventArgs e)
        {
            // 根据变更的属性名，触发对应的属性通知
            switch (e.PropertyName)
            {
                case "IsRunModeAdmin":
                    OnPropertyChanged(nameof(Is管理模式));
                    break;
                    
                case "IsRunModeDev":
                    OnPropertyChanged(nameof(Is开发模式));
                    break;
                    
                case "IsRunModeBoss":
                    OnPropertyChanged(nameof(Is老板模式));
                    break;
            }
        }
    }
}

