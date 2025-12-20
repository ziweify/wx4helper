using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars.Helpers.Docking; // DockingManager 在这个命名空间中
using DevExpress.XtraBars.Ribbon;
using 永利系统.Models;
using 永利系统.Services;
using 永利系统.ViewModels;
using 永利系统.Views.Pages;

namespace 永利系统.Views
{
    public partial class Main : RibbonForm
    {
        private readonly MainViewModel _viewModel;
        private readonly Dictionary<string, UserControl> _pages = new();
        private UserControl? _currentPage;
        private LogWindow? _logWindow;
        private readonly LoggingService _loggingService;

        public Main()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            _loggingService = LoggingService.Instance;
            InitializeApplicationMenu();
            InitializeLogging();
            InitializeNavigation();
            BindViewModel();
            ApplyModernTheme();
            SetupKeyboardShortcuts();
        }

        private void InitializeApplicationMenu()
        {
            // 在 DevExpress WinForms 中，ApplicationMenu 的菜单项需要通过设计器创建
            // 或者使用 PopupMenu 作为替代方案
            // 
            // 注意：ApplicationMenu.Items 属性在 WinForms 版本中可能不存在
            // 建议通过 Visual Studio 设计器来配置 ApplicationMenu 的菜单项
            //
            // 如果需要通过代码创建菜单，可以使用 PopupMenu：
            // var popupMenu = new DevExpress.XtraBars.PopupMenu();
            // popupMenu.Manager = ribbonControl1.Manager;
            // popupMenu.ItemLinks.Add(menuItemNew);
            // ribbonControl1.ApplicationButtonDropDownControl = popupMenu;
            
            // 当前实现：ApplicationMenu 已通过设计器创建并绑定
            // 菜单项需要在设计器中配置，或者使用 PopupMenu 替代
        }

        private void InitializeLogging()
        {
            // 创建日志窗口
            _logWindow = new LogWindow
            {
                Dock = DockStyle.Bottom,
                Height = 250,
                Visible = false // 默认隐藏
            };
            
            // 添加到内容面板
            contentPanel.Controls.Add(_logWindow);

            // 订阅日志事件，更新状态栏
            _loggingService.LogReceived += OnLogReceived;

            // 启动日志（只记录一次，表示主窗口已初始化）
            _loggingService.Info("系统", "主窗口初始化完成");
        }

        private void OnLogReceived(object? sender, LogEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateStatusBarLog(e.LogEntry)));
            }
            else
            {
                UpdateStatusBarLog(e.LogEntry);
            }
        }

        private void UpdateStatusBarLog(LogEntry entry)
        {
            var timestamp = entry.Timestamp.ToString("HH:mm:ss");
            var module = string.IsNullOrEmpty(entry.Module) ? "系统" : entry.Module;
            var level = entry.Level.ToString().ToUpper();
            var message = entry.Message.Length > 50 ? entry.Message.Substring(0, 50) + "..." : entry.Message;
            
            barStaticItemLog.Caption = $"{timestamp} [{module}] [{level}] {message}";
            
            // 根据级别设置颜色（使用 Appearance 属性）
            switch (entry.Level)
            {
                case LogLevel.Error:
                    barStaticItemLog.Appearance.ForeColor = Color.Red;
                    break;
                case LogLevel.Warn:
                    barStaticItemLog.Appearance.ForeColor = Color.Orange;
                    break;
                case LogLevel.Info:
                    barStaticItemLog.Appearance.ForeColor = Color.Blue;
                    break;
                default:
                    barStaticItemLog.Appearance.ForeColor = Color.Black;
                    break;
            }
        }

        private void SetupKeyboardShortcuts()
        {
            // F12 切换日志窗口
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F12)
                {
                    ToggleLogWindow();
                }
            };
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

            // 添加新页面（保留日志窗口）
            contentPanel.Controls.Add(page);
            _currentPage = page;
            
            // 确保日志窗口在最上层（如果存在且可见）
            if (_logWindow != null && _logWindow.Visible)
            {
                contentPanel.Controls.SetChildIndex(_logWindow, contentPanel.Controls.Count - 1);
            }

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

        private void barButtonItemLog_ItemClick(object sender, ItemClickEventArgs e)
        {
            ToggleLogWindow();
        }

        private void barStaticItemLog_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 点击状态栏日志项，切换日志窗口
            ToggleLogWindow();
        }
        
        private void ToggleLogWindow()
        {
            // 如果日志窗口未初始化，先初始化
            if (_logWindow == null)
            {
                InitializeLogging();
            }
            
            if (_logWindow != null)
            {
                _logWindow.Visible = !_logWindow.Visible;
                
                // 如果显示，确保在最上层
                if (_logWindow.Visible)
                {
                    contentPanel.Controls.SetChildIndex(_logWindow, contentPanel.Controls.Count - 1);
                }
            }
        }

        #endregion

        #region ApplicationMenu Event Handlers

        private void menuItemNew_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageBox.Show("执行新建操作", "新建", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void menuItemOpen_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "所有文件|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show($"打开文件: {dialog.FileName}", "打开", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void menuItemSave_ItemClick(object sender, ItemClickEventArgs e)
        {
            _viewModel.SaveCommand?.Execute(null);
        }

        private void menuItemSaveAs_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "所有文件|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show($"保存到: {dialog.FileName}", "另存为", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void menuItemPrint_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageBox.Show("执行打印操作", "打印", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void menuItemOptions_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageBox.Show("打开选项对话框", "选项", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void menuItemShowQATBelow_ItemClick(object sender, ItemClickEventArgs e)
        {
            var checkItem = sender as DevExpress.XtraBars.BarCheckItem;
            if (checkItem != null)
            {
                // 切换 QAT 位置
                if (checkItem.Checked)
                {
                    ribbonControl1.ToolbarLocation = DevExpress.XtraBars.Ribbon.RibbonQuickAccessToolbarLocation.Below;
                }
                else
                {
                    ribbonControl1.ToolbarLocation = DevExpress.XtraBars.Ribbon.RibbonQuickAccessToolbarLocation.Above;
                }
            }
        }

        private void menuItemExit_ItemClick(object sender, ItemClickEventArgs e)
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
