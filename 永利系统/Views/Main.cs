using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using 永利系统.ViewModels;
using 永利系统.Views.Pages;

namespace 永利系统.Views
{
    public partial class Main : RibbonForm
    {
        private readonly MainViewModel _viewModel;
        private readonly Dictionary<string, UserControl> _pages = new();
        private UserControl? _currentPage;

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
            // 注册页面（使用简单的 Dictionary 管理）
            _pages["Dashboard"] = new DashboardPage();
            _pages["DataManagement"] = new DataManagementPage();
            _pages["Reports"] = new DashboardPage(); // 可以替换为实际的报表页面
            _pages["Settings"] = new DashboardPage(); // 可以替换为实际的设置页面

            // 设置所有页面为 Fill
            foreach (var page in _pages.Values)
            {
                page.Dock = DockStyle.Fill;
            }

            // 导航到默认页面
            NavigateToPage("Dashboard");
        }

        private void NavigateToPage(string pageKey)
        {
            if (!_pages.TryGetValue(pageKey, out var page))
                return;

            if (_currentPage == page)
                return;

            // 移除当前页面
            if (_currentPage != null)
            {
                contentPanel.Controls.Remove(_currentPage);
            }

            // 添加新页面
            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(page);
            _currentPage = page;

            // 更新按钮状态
            UpdateNavigationButtons(pageKey);
        }

        private void UpdateNavigationButtons(string pageKey)
        {
            // 重置所有按钮
            barButtonItemDashboard.Down = false;
            barButtonItemDataManagement.Down = false;
            barButtonItemReports.Down = false;
            barButtonItemSettings.Down = false;

            // 设置当前按钮
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

        #region Ribbon Button Click Events

        private void barButtonItemDashboard_ItemClick(object sender, ItemClickEventArgs e)
        {
            NavigateToPage("Dashboard");
        }

        private void barButtonItemDataManagement_ItemClick(object sender, ItemClickEventArgs e)
        {
            NavigateToPage("DataManagement");
        }

        private void barButtonItemReports_ItemClick(object sender, ItemClickEventArgs e)
        {
            NavigateToPage("Reports");
        }

        private void barButtonItemSettings_ItemClick(object sender, ItemClickEventArgs e)
        {
            NavigateToPage("Settings");
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
            _viewModel.Initialize();
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
