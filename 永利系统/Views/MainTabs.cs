using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraTab;
using 永利系统.Models;
using 永利系统.Services;
using 永利系统.ViewModels;
using 永利系统.Views.Pages;

namespace 永利系统.Views
{
    /// <summary>
    /// 使用传统工具栏和 TabControl 的主窗口
    /// </summary>
    public partial class MainTabs : Form
    {
        private readonly MainViewModel _viewModel;
        private readonly LoggingService _loggingService;

        public MainTabs()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            _loggingService = LoggingService.Instance;
            InitializeToolbar();
            InitializeLogging();
            InitializeTabs();
            BindViewModel();
            ApplyModernTheme();
            SetupKeyboardShortcuts();
        }

        private void InitializeToolbar()
        {
            // 工具栏已通过 Designer 配置
            // 这里可以添加额外的初始化逻辑
        }

        private void InitializeLogging()
        {
            // 默认隐藏日志面板
            splitContainerControl1.PanelVisibility = DevExpress.XtraEditors.SplitPanelVisibility.Panel1;
            
            // 订阅日志事件，更新状态栏
            _loggingService.LogReceived += OnLogReceived;

            // 启动日志
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
            
            toolStripStatusLog.Text = $"{timestamp} [{module}] [{level}] {message}";
            
            // 根据级别设置颜色
            switch (entry.Level)
            {
                case LogLevel.Error:
                    toolStripStatusLog.ForeColor = Color.Red;
                    break;
                case LogLevel.Warn:
                    toolStripStatusLog.ForeColor = Color.Orange;
                    break;
                case LogLevel.Info:
                    toolStripStatusLog.ForeColor = Color.Blue;
                    break;
                default:
                    toolStripStatusLog.ForeColor = Color.Black;
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

        private void InitializeTabs()
        {
            // 创建所有标签页（使用 Form 而不是 UserControl）
            CreateTabPage("主页", "Dashboard", new DashboardPage());
            CreateTabPage("数据管理", "DataManagement", new DataManagementPage());
            CreateTabPage("报表分析", "Reports", new DashboardPage()); // 可以替换为实际的报表页面
            CreateTabPage("系统设置", "Settings", new DashboardPage()); // 可以替换为实际的设置页面
            CreateTabPage("微信助手", "Wechat", new WechatPage());
            
            // 默认选中第一个标签页
            if (xtraTabControl1.TabPages.Count > 0)
            {
                xtraTabControl1.SelectedTabPageIndex = 0;
            }
        }

        private void CreateTabPage(string tabText, string tabName, Form pageForm)
        {
            var tabPage = new XtraTabPage
            {
                Text = tabText,
                Name = tabName
            };
            
            // 确保 Form 已设置为非顶级窗口
            if (pageForm.TopLevel)
            {
                pageForm.TopLevel = false;
            }
            
            // 设置 Form 为无边框并填充
            pageForm.FormBorderStyle = FormBorderStyle.None;
            pageForm.Dock = DockStyle.Fill;
            
            // 显示 Form（必须调用 Show，即使 TopLevel = false）
            pageForm.Show();
            
            // 添加 Form 到标签页
            tabPage.Controls.Add(pageForm);
            
            // 添加到 TabControl
            xtraTabControl1.TabPages.Add(tabPage);
        }

        private void BindViewModel()
        {
            // 绑定状态栏
            toolStripStatusStatus.Text = _viewModel.StatusMessage;
            toolStripStatusUser.Text = $"当前用户: {_viewModel.CurrentUser}";

            // 监听属性变更
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.StatusMessage))
                {
                    toolStripStatusStatus.Text = _viewModel.StatusMessage;
                }
            };
        }

        private void ApplyModernTheme()
        {
            // 应用现代化主题
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = "Office 2019 Colorful";
        }

        private void ToggleLogWindow()
        {
            // 切换日志面板的显示/隐藏
            if (splitContainerControl1.PanelVisibility == DevExpress.XtraEditors.SplitPanelVisibility.Panel1)
            {
                // 显示日志面板
                splitContainerControl1.PanelVisibility = DevExpress.XtraEditors.SplitPanelVisibility.Both;
                // 设置分隔位置（距离底部250像素）
                splitContainerControl1.SplitterPosition = splitContainerControl1.Height - 250;
            }
            else
            {
                // 隐藏日志面板
                splitContainerControl1.PanelVisibility = DevExpress.XtraEditors.SplitPanelVisibility.Panel1;
            }
        }

        #region Toolbar Button Click Events

        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            _viewModel.RefreshCommand?.Execute(null);
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            _viewModel.SaveCommand?.Execute(null);
        }

        private void toolStripButtonLog_Click(object sender, EventArgs e)
        {
            ToggleLogWindow();
        }

        private void toolStripButtonExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void toolStripButtonWechatStart_Click(object sender, EventArgs e)
        {
            _loggingService.Info("微信助手", "启动微信...");
            // TODO: 实现启动微信的逻辑
        }

        #endregion

        #region Menu Item Click Events

        private void ToolStripMenuItemNew_Click(object sender, EventArgs e)
        {
            MessageBox.Show("执行新建操作", "新建", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ToolStripMenuItemOpen_Click(object sender, EventArgs e)
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

        private void ToolStripMenuItemSave_Click(object sender, EventArgs e)
        {
            _viewModel.SaveCommand?.Execute(null);
        }

        private void ToolStripMenuItemSaveAs_Click(object sender, EventArgs e)
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

        private void ToolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ToolStripMenuItemOptions_Click(object sender, EventArgs e)
        {
            MessageBox.Show("打开选项对话框", "选项", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ToolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("永利系统 v1.0\n数据管理平台", "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ToolStripStatusLog_Click(object sender, EventArgs e)
        {
            // 点击状态栏日志项，切换日志窗口
            ToggleLogWindow();
        }

        #endregion

        #region Form Events

        private void MainTabs_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 保存设置等
        }

        private void MainTabs_Load(object sender, EventArgs e)
        {
            _viewModel.Initialize();
        }

        #endregion
    }
}
