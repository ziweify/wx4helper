using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.Mvvm;

namespace 永利系统.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private string _statusMessage = "就绪";
        private string _currentUser = "管理员";
        private string _selectedPageKey = "Dashboard";

        private bool _isBusy;

        public MainViewModel()
        {
            InitializeCommands();
        }

        #region 属性

        /// <summary>
        /// 状态栏消息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 当前用户
        /// </summary>
        public string CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 当前选中的页面键
        /// </summary>
        public string SelectedPageKey
        {
            get => _selectedPageKey;
            set
            {
                if (_selectedPageKey != value)
                {
                    _selectedPageKey = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region 命令

        public ICommand? NavigateToDashboardCommand { get; private set; }
        public ICommand? NavigateToDataManagementCommand { get; private set; }
        public ICommand? NavigateToReportsCommand { get; private set; }
        public ICommand? NavigateToSettingsCommand { get; private set; }
        public ICommand? RefreshCommand { get; private set; }
        public ICommand? SaveCommand { get; private set; }
        public ICommand? ExitCommand { get; private set; }

        private void InitializeCommands()
        {
            NavigateToDashboardCommand = new DelegateCommand(() => NavigateToPage("Dashboard"));
            NavigateToDataManagementCommand = new DelegateCommand(() => NavigateToPage("DataManagement"));
            NavigateToReportsCommand = new DelegateCommand(() => NavigateToPage("Reports"));
            NavigateToSettingsCommand = new DelegateCommand(() => NavigateToPage("Settings"));
            RefreshCommand = new DelegateCommand(() => RefreshData());
            SaveCommand = new DelegateCommand(() => SaveData(), () => !IsBusy);
            ExitCommand = new DelegateCommand(() => ExitApplication());
        }

        #endregion

        #region 方法

        private void NavigateToPage(string pageKey)
        {
            SelectedPageKey = pageKey;
            StatusMessage = $"已切换到 {GetPageDisplayName(pageKey)}";
        }

        private string GetPageDisplayName(string pageKey)
        {
            return pageKey switch
            {
                "Dashboard" => "首页",
                "DataManagement" => "数据管理",
                "Reports" => "报表分析",
                "Settings" => "系统设置",
                _ => pageKey
            };
        }

        private void RefreshData()
        {
            StatusMessage = "正在刷新数据...";
            // TODO: 实现刷新逻辑
            StatusMessage = "数据刷新完成";
        }

        private void SaveData()
        {
            IsBusy = true;
            StatusMessage = "正在保存...";
            
            try
            {
                // TODO: 实现保存逻辑
                System.Threading.Thread.Sleep(500); // 模拟保存操作
                StatusMessage = "保存成功";
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExitApplication()
        {
            System.Windows.Forms.Application.Exit();
        }

        // OnLoaded 方法在 DevExpress.Mvvm.ViewModelBase 中不存在
        // 初始化已在构造函数中完成
        public void Initialize()
        {
            StatusMessage = $"欢迎使用永利系统，{CurrentUser}";
        }

        #endregion
    }
}

