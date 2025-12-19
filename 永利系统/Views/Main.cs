using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using 永利系统.ViewModels;
using 永利系统.Core;
using 永利系统.Views.Pages;

namespace 永利系统.Views
{
    public partial class Main : RibbonForm
    {
        private readonly MainViewModel _viewModel;
        private NavigationService? _navigationService;

        public Main()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            InitializeNavigation();
            BindViewModel();
            ApplyModernTheme();
        }

        private void InitializeNavigation()
        {
            // 初始化导航服务
            _navigationService = new NavigationService(contentPanel);

            // 注册页面
            _navigationService.RegisterPage("Dashboard", new DashboardPage());
            _navigationService.RegisterPage("DataManagement", new DataManagementPage());
            _navigationService.RegisterPage("Reports", new DashboardPage()); // 可以替换为实际的报表页面
            _navigationService.RegisterPage("Settings", new DashboardPage()); // 可以替换为实际的设置页面

            // 导航到默认页面
            _navigationService.NavigateTo("Dashboard");

            // 订阅导航变更事件
            _navigationService.NavigationChanged += OnNavigationChanged;
        }

        private void BindViewModel()
        {
            // 绑定状态栏
            barStaticItemStatus.Caption = _viewModel.StatusMessage;
            barStaticItemUser.Caption = $"当前用户: {_viewModel.CurrentUser}";

            // 监听属性变更
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.StatusMessage))
                {
                    barStaticItemStatus.Caption = _viewModel.StatusMessage;
                }
            };
        }

        private void ApplyModernTheme()
        {
            // 应用现代化主题
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = "Office 2019 Colorful";
        }

        private void OnNavigationChanged(object? sender, string pageKey)
        {
            // 更新 Ribbon 按钮选中状态
            ResetRibbonButtonsState();
            
            switch (pageKey)
            {
                case "Dashboard":
                    barButtonItemDashboard.Down = true;
                    break;
                case "DataManagement":
                    barButtonItemDataManagement.Down = true;
                    break;
                case "Reports":
                    barButtonItemReports.Down = true;
                    break;
                case "Settings":
                    barButtonItemSettings.Down = true;
                    break;
            }
        }

        private void ResetRibbonButtonsState()
        {
            barButtonItemDashboard.Down = false;
            barButtonItemDataManagement.Down = false;
            barButtonItemReports.Down = false;
            barButtonItemSettings.Down = false;
        }

        #region Ribbon Button Click Events

        private void barButtonItemDashboard_ItemClick(object sender, ItemClickEventArgs e)
        {
            _navigationService?.NavigateTo("Dashboard");
        }

        private void barButtonItemDataManagement_ItemClick(object sender, ItemClickEventArgs e)
        {
            _navigationService?.NavigateTo("DataManagement");
        }

        private void barButtonItemReports_ItemClick(object sender, ItemClickEventArgs e)
        {
            _navigationService?.NavigateTo("Reports");
        }

        private void barButtonItemSettings_ItemClick(object sender, ItemClickEventArgs e)
        {
            _navigationService?.NavigateTo("Settings");
        }

        private void barButtonItemRefresh_ItemClick(object sender, ItemClickEventArgs e)
        {
            _viewModel.RefreshCommand?.Execute(null);
        }

        private void barButtonItemSave_ItemClick(object sender, ItemClickEventArgs e)
        {
            _viewModel.SaveCommand?.Execute(null);
        }

        private void barButtonItemExit_ItemClick(object sender, ItemClickEventArgs e)
        {
            Close();
        }

        #endregion

        private void Main_Load(object sender, EventArgs e)
        {
            _viewModel.OnLoaded();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show(
                "确定要退出系统吗？",
                "确认退出",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }
    }
}
