using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraTab;
using 永利系统.Models;
using 永利系统.Services;
using 永利系统.Services.Auth;
using 永利系统.ViewModels;
using 永利系统.Views.Pages;
using 永利系统.Views.Wechat;

namespace 永利系统.Views
{
    /// <summary>
    /// 使用传统工具栏和 TabControl 的主窗口
    /// </summary>
    public partial class MainTabs : Form
    {
        private readonly MainViewModel _viewModel;
        private readonly LoggingService _loggingService;
        private readonly AuthGuard? _authGuard;
        private System.Windows.Forms.Timer? _authVerifyTimer;

        /// <summary>
        /// 构造函数（必须传入 AuthGuard，防止直接实例化）
        /// </summary>
        public MainTabs(AuthGuard? authGuard = null)
        {
            // 🔥 防破解：验证认证状态
            if (authGuard == null || !authGuard.VerifyAuthentication())
            {
                MessageBox.Show("未通过认证验证，无法启动主窗口", "安全验证", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                
                // 初始化必需字段（防止编译器警告）
                _viewModel = new MainViewModel();
                _loggingService = LoggingService.Instance;
                return;
            }
            
            _authGuard = authGuard;
            
            InitializeComponent();
            _viewModel = new MainViewModel();
            _loggingService = LoggingService.Instance;
            
            // 再次验证（双重验证）
            if (!_authGuard.VerifyAuthentication())
            {
                MessageBox.Show("认证验证失败，程序将退出", "安全验证", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            
            InitializeLogging();
            InitializeTabs();
            BindViewModel();
            ApplyModernTheme();
            SetupKeyboardShortcuts();
            StartPeriodicAuthVerify();
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
            // 🔥 防破解：关键操作前验证
            if (_authGuard != null && !_authGuard.VerifyAuthentication())
            {
                _loggingService.Error("主窗口", "初始化标签页时验证失败");
                return;
            }
            
            // 创建所有标签页（使用 Form 而不是 UserControl）
            CreateTabPage("主页", "Dashboard", new DashboardPage());
            CreateTabPage("数据管理", "DataManagement", new DataManagementPage());
            CreateTabPage("报表分析", "Reports", new ReportsPage());
            CreateTabPage("系统设置", "Settings", new SettingsPage());
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
            toolStripStatusStatus.Text = FormatStatusMessage(_viewModel.StatusMessage);
            toolStripStatusUser.Text = $"当前用户: {_viewModel.CurrentUser}";

            // 监听属性变更
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.StatusMessage))
                {
                    toolStripStatusStatus.Text = FormatStatusMessage(_viewModel.StatusMessage);
                }
            };
        }

        private string FormatStatusMessage(string message)
        {
            // 如果消息已经包含"永利系统"前缀，则直接返回
            if (message.StartsWith("永利系统"))
            {
                return message;
            }
            // 否则加上"永利系统"前缀
            return $"永利系统{message}";
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
                // 更新菜单项的选中状态
                toolStripMenuItemViewLog.Checked = true;
            }
            else
            {
                // 隐藏日志面板
                splitContainerControl1.PanelVisibility = DevExpress.XtraEditors.SplitPanelVisibility.Panel1;
                // 更新菜单项的选中状态
                toolStripMenuItemViewLog.Checked = false;
            }
        }


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
            // 🔥 防破解：关键操作前验证
            if (_authGuard != null && !_authGuard.VerifyOperation("保存数据"))
            {
                return;
            }
            
            _viewModel.SaveCommand?.Execute(null);
        }

        private void ToolStripMenuItemSaveAs_Click(object sender, EventArgs e)
        {
            // 🔥 防破解：关键操作前验证
            if (_authGuard != null && !_authGuard.VerifyOperation("另存为"))
            {
                return;
            }
            
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

        private void ToolStripMenuItemViewLog_Click(object sender, EventArgs e)
        {
            // 切换日志窗口显示/隐藏
            ToggleLogWindow();
        }

        private void ToolStripMenuItemOptions_Click(object sender, EventArgs e)
        {
            // 🔥 防破解：关键操作前验证
            if (_authGuard != null && !_authGuard.VerifyOperation("系统设置"))
            {
                return;
            }
            
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

        /// <summary>
        /// 启动定期认证验证（每5分钟验证一次）
        /// </summary>
        private void StartPeriodicAuthVerify()
        {
            if (_authGuard == null)
                return;
                
            _authVerifyTimer = new System.Windows.Forms.Timer();
            _authVerifyTimer.Interval = 5 * 60 * 1000; // 5分钟
            _authVerifyTimer.Tick += async (s, e) =>
            {
                var isValid = await _authGuard.PeriodicVerifyAsync();
                if (!isValid)
                {
                    _loggingService.Error("主窗口", "定期验证失败，程序将退出");
                    MessageBox.Show("认证验证失败，程序将退出", "安全验证", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            };
            _authVerifyTimer.Start();
        }
        
        private void MainTabs_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 清理定时器
            if (_authVerifyTimer != null)
            {
                _authVerifyTimer.Stop();
                _authVerifyTimer.Dispose();
                _authVerifyTimer = null;
            }
            
            // 清除认证状态
            if (_authGuard != null)
            {
                AuthGuard.ClearAuthentication();
            }
            
            // 🔥 保存配置（窗口位置、大小等）
            SaveWindowSettings();
            Services.Config.ConfigManager.Instance.SaveNow();
            
            _loggingService.Info("主窗口", "程序正常退出");
        }

        private void MainTabs_Load(object sender, EventArgs e)
        {
            // 加载窗口设置
            LoadWindowSettings();
            
            _viewModel.Initialize();
            _loggingService.Info("主窗口", "主窗口加载完成");
        }

        #endregion
        
        #region 窗口设置保存/加载
        
        /// <summary>
        /// 加载窗口设置
        /// </summary>
        private void LoadWindowSettings()
        {
            var config = Services.Config.ConfigManager.Instance.Config.Window;
            
            // 恢复窗口大小
            if (config.Width > 0 && config.Height > 0)
            {
                this.Size = new Size(config.Width, config.Height);
            }
            
            // 恢复窗口位置（-1 表示居中）
            if (config.X >= 0 && config.Y >= 0)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(config.X, config.Y);
            }
            else
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            
            // 恢复最大化状态
            if (config.Maximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }
        
        /// <summary>
        /// 保存窗口设置
        /// </summary>
        private void SaveWindowSettings()
        {
            var config = Services.Config.ConfigManager.Instance.Config.Window;
            
            // 保存窗口状态
            config.Maximized = (this.WindowState == FormWindowState.Maximized);
            
            // 只在正常状态下保存位置和大小
            if (this.WindowState == FormWindowState.Normal)
            {
                config.Width = this.Width;
                config.Height = this.Height;
                config.X = this.Location.X;
                config.Y = this.Location.Y;
            }
        }
        
        #endregion
    }
}
