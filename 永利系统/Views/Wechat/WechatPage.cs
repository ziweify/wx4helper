using System;
using System.ComponentModel;
using System.Windows.Forms;
using DevExpress.XtraEditors;  // ✅ 添加 DevExpress 命名空间
using 永利系统.Services;
using 永利系统.Services.Wechat;
using 永利系统.Views.Wechat.Controls;

namespace 永利系统.Views.Wechat
{
    /// <summary>
    /// 微信助手页面 - 使用 XtraForm 实现，支持后台自动刷新
    /// 复刻 BaiShengVx3Plus 的 VxMain 界面设计
    /// ✅ 修复：改用 XtraForm 以支持 DevExpress 控件在设计器中正常工作
    /// </summary>
    public partial class WechatPage : XtraForm  // ✅ 修改：Form → XtraForm
    {
        private readonly LoggingService? _loggingService;
        private System.Windows.Forms.Timer? _refreshTimer;
        private WechatBingoGameService? _gameService;
        private WechatSettingsForm? _settingsForm; // 设置窗口引用

        public WechatPage()
        {
            InitializeComponent();
            
            // ⚠️ 设计器模式下不执行运行时初始化代码（使用更可靠的检查方法）
            if (IsDesignMode())
            {
                TopLevel = true;
                return;
            }
            
            // 🔥 临时：生成工具栏图标文件（只运行一次，然后删除此代码）
            // GenerateToolbarIconFiles();

            
            // 设置为非顶级窗口，可以嵌入到 TabPage 中
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            
            _loggingService = LoggingService.Instance;
            InitializeUI();
            InitializeGameService();
            StartAutoRefresh();
            
            // 订阅 FormClosing 事件以清理资源
            FormClosing += WechatPage_FormClosing;
        }

        /// <summary>
        /// 🔥 临时方法：生成工具栏图标PNG文件
        /// 运行一次后请删除此方法和构造函数中的调用
        /// </summary>
        private void GenerateToolbarIconFiles()
        {
            try
            {
                // 获取项目根目录（向上3层：bin/Debug/net8.0-windows -> bin/Debug -> bin -> 项目根目录）
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, "..", "..", ".."));
                string iconsDir = System.IO.Path.Combine(projectDir, "Resources", "Icons");
                
                // 如果目录不存在则创建
                if (!System.IO.Directory.Exists(iconsDir))
                {
                    System.IO.Directory.CreateDirectory(iconsDir);
                }
                
                // 生成图标
                Helpers.ToolbarIconGenerator.GenerateAllIcons(iconsDir);
            }
            catch (Exception ex)
            {
                _loggingService?.Error("WechatPage", $"生成图标文件失败: {ex.Message}");
            }
        }

        private void InitializeUI()
        {
            // 注意：工具栏图标已在 Designer.cs 的 InitializeComponent() 中初始化
            // 这样设计器可以直接显示图标占位
            
            // 🔥 Bingo 数据控件已在设计器中添加，这里只需要绑定服务即可
            // 不再需要动态创建控件
            
            // 初始化界面
            _loggingService.Info("微信助手", "微信助手页面已初始化");
        }
        
        /// <summary>
        /// 🔥 初始化游戏服务
        /// </summary>
        private void InitializeGameService()
        {
            try
            {
                // 创建游戏服务（WechatBingoGameService 继承自 BingoGameServiceBase，也实现了 ILotteryService）
                _gameService = new WechatBingoGameService(_loggingService);
                
                // 将游戏服务绑定到 Bingo 数据控件（WechatBingoGameService 实现了 ILotteryService）
                // 控件已在设计器中创建，这里直接使用
                if (ucBingoDataCur != null && _gameService != null)
                {
                    ucBingoDataCur.SetLotteryService(_gameService);
                    _loggingService.Info("微信助手", "当前期控件已绑定游戏服务");
                }
                
                if (ucBingoDataLast != null && _gameService != null)
                {
                    ucBingoDataLast.SetLotteryService(_gameService);
                    _loggingService.Info("微信助手", "上期控件已绑定游戏服务");
                }
                
                // 启动游戏服务
                _ = _gameService?.StartAsync(); // 使用 _ = 忽略未等待警告
                _loggingService.Info("微信助手", "游戏服务已启动");
            }
            catch (Exception ex)
            {
                _loggingService.Error("微信助手", $"初始化游戏服务失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 启动自动刷新（即使页面不可见也会运行）
        /// </summary>
        private void StartAutoRefresh()
        {
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 3000 // 每3秒刷新一次
            };
            _refreshTimer.Tick += (s, e) =>
            {
                // 后台自动刷新微信数据
                _loggingService.Debug("微信助手", "后台自动刷新中...");
                // TODO: 实现微信数据刷新逻辑
            };
            _refreshTimer.Start();
        }

        private void WechatPage_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 停止游戏服务
            if (_gameService != null)
            {
                try
                {
                    _ = _gameService.StopAsync(); // 使用 _ = 忽略未等待警告
                    _loggingService.Info("微信助手", "游戏服务已停止");
                }
                catch (Exception ex)
                {
                    _loggingService.Error("微信助手", $"停止游戏服务失败: {ex.Message}");
                }
            }
            
            // 清理 Timer
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }

        // 工具栏按钮事件处理（框架，不包含逻辑）
        private void ToolStripButton_Connect_Click(object sender, EventArgs e)
        {
            _loggingService.Info("微信助手", "连接按钮被点击");
        }

        private void ToolStripButton_Log_Click(object sender, EventArgs e)
        {
            _loggingService.Info("微信助手", "日志按钮被点击");
        }

        private void ToolStripButton_OpenLotteryResult_Click(object sender, EventArgs e)
        {
            _loggingService.Info("微信助手", "开奖结果按钮被点击");
        }

        private void ToolStripButton_CreditWithdrawManage_Click(object sender, EventArgs e)
        {
            _loggingService.Info("微信助手", "上下分管理按钮被点击");
        }

        private void ToolStripButton_ClearData_Click(object sender, EventArgs e)
        {
            _loggingService.Info("微信助手", "清空数据按钮被点击");
        }

        private void ToolStripButton_Settings_Click(object sender, EventArgs e)
        {
            try
            {
                // 检查设置窗口是否已打开
                if (_settingsForm != null && !_settingsForm.IsDisposed)
                {
                    // 窗口已打开，激活并显示到前台
                    _loggingService?.Info("微信助手", "设置窗口已打开，激活到前台");
                    
                    // 如果窗口最小化，先恢复
                    if (_settingsForm.WindowState == FormWindowState.Minimized)
                    {
                        _settingsForm.WindowState = FormWindowState.Normal;
                    }
                    
                    // 激活窗口并显示到最前面
                    _settingsForm.Activate();
                    _settingsForm.BringToFront();
                    _settingsForm.Focus();
                    
                    return;
                }
                
                _loggingService?.Info("微信助手", "创建新的设置窗口");
                
                // 创建新的设置窗口（非模态）
                _settingsForm = new WechatSettingsForm();
                
                // 🔥 获取真正的主窗口（MainTabs）
                // WechatPage 是 XtraForm，被嵌入到 MainTabs 的 TabPage 中
                // 需要通过 TopLevelControl 或 Application.OpenForms 获取主窗口
                Form? mainForm = null;
                
                // 方法1：使用 TopLevelControl 属性（最可靠）
                mainForm = this.TopLevelControl as Form;
                
                // 方法2：如果方法1失败，遍历所有打开的窗体查找 MainTabs
                if (mainForm == null)
                {
                    foreach (Form form in Application.OpenForms)
                    {
                        if (form.GetType().Name == "MainTabs")
                        {
                            mainForm = form;
                            break;
                        }
                    }
                }
                
                _loggingService?.Info("微信助手", $"找到主窗口: {mainForm?.GetType().Name ?? "null"}, WindowState={mainForm?.WindowState}, Location=({mainForm?.Location.X}, {mainForm?.Location.Y}), DesktopLocation=({mainForm?.DesktopLocation.X}, {mainForm?.DesktopLocation.Y})");
                
                // 手动设置窗口位置为主窗口正中间
                if (mainForm != null)
                {
                    _settingsForm.StartPosition = FormStartPosition.Manual;
                    
                    // 🔥 使用 DesktopLocation 获取窗口在屏幕上的绝对坐标
                    // Location 可能是相对于父容器的，DesktopLocation 才是屏幕坐标
                    int mainX = mainForm.DesktopLocation.X;
                    int mainY = mainForm.DesktopLocation.Y;
                    int mainWidth = mainForm.Width;
                    int mainHeight = mainForm.Height;
                    
                    int x = mainX + (mainWidth - _settingsForm.Width) / 2;
                    int y = mainY + (mainHeight - _settingsForm.Height) / 2;
                    
                    _settingsForm.Location = new System.Drawing.Point(x, y);
                    _loggingService?.Info("微信助手", $"设置窗口位置: 主窗口DesktopLocation({mainX}, {mainY}, {mainWidth}x{mainHeight}), 设置窗口({x}, {y}, {_settingsForm.Width}x{_settingsForm.Height})");
                }
                else
                {
                    // 找不到主窗口，使用屏幕中心
                    _settingsForm.StartPosition = FormStartPosition.CenterScreen;
                    _loggingService?.Warn("微信助手", "未找到主窗口，设置窗口将在屏幕中央显示");
                }
                
                // 订阅关闭事件，清理引用
                _settingsForm.FormClosed += (s, args) =>
                {
                    _settingsForm = null;
                    _loggingService?.Info("微信助手", "设置窗口已关闭");
                };
                
                // 显示窗口（非模态，TopMost 已在窗口构造函数中设置）
                if (mainForm != null)
                {
                    _settingsForm.Show(mainForm); // 传递父窗口参数
                }
                else
                {
                    _settingsForm.Show();
                }
                
                _settingsForm.Activate();
                _settingsForm.Focus();
            }
            catch (Exception ex)
            {
                _loggingService?.Error("微信助手", $"打开设置窗口失败: {ex.Message}", ex);
                MessageBox.Show($"打开设置窗口失败:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 判断是否处于设计器模式（更可靠的方法）
        /// </summary>
        private bool IsDesignMode()
        {
            // 方法1：检查 DesignMode 属性
            if (DesignMode)
                return true;
            
            // 方法2：检查 LicenseManager（更可靠）
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;
            
            // 方法3：检查 Site
            if (Site != null && Site.DesignMode)
                return true;
            
            // 方法4：检查是否有 Handle（设计器模式下通常没有 Handle）
            // 注意：这个检查需要在 HandleCreated 之后才准确，所以只作为辅助检查
            try
            {
                if (!IsHandleCreated)
                {
                    // 如果还没有 Handle，可能是设计器模式
                    // 但这不是绝对可靠的，因为运行时也可能还没有 Handle
                    return false; // 不依赖这个检查
                }
            }
            catch
            {
                // 如果检查 Handle 时出错，可能是设计器模式
                return true;
            }
            
            return false;
        }
    }
}

