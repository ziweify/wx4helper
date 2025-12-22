using System;
using System.Windows.Forms;
using 永利系统.Services;
using 永利系统.Services.Wechat;
using 永利系统.Views.Wechat.Controls;

namespace 永利系统.Views.Wechat
{
    /// <summary>
    /// 微信助手页面 - 使用 Form 实现，支持后台自动刷新
    /// 复刻 BaiShengVx3Plus 的 VxMain 界面设计
    /// </summary>
    public partial class WechatPage : Form
    {
        private readonly LoggingService _loggingService;
        private System.Windows.Forms.Timer? _refreshTimer;
        private WechatBingoGameService? _gameService;
        
        // Bingo 数据控件
        private UcBingoDataCur? _ucBingoDataCur;
        private UcBingoDataLast? _ucBingoDataLast;

        public WechatPage()
        {
            InitializeComponent();
            
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

        private void InitializeUI()
        {
            // 设置工具栏按钮图标（必须在 InitializeComponent 之后调用）
            toolStripButton_Connect.Image = CreateConnectIcon();
            toolStripButton_Connect.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButton_Connect.Size = new System.Drawing.Size(60, 47);
            
            toolStripButton_Log.Image = CreateLogIcon();
            toolStripButton_Log.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButton_Log.Size = new System.Drawing.Size(60, 47);
            
            toolStripButton_OpenLotteryResult.Image = CreateLotteryIcon();
            toolStripButton_OpenLotteryResult.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButton_OpenLotteryResult.Size = new System.Drawing.Size(84, 47);
            
            toolStripButton_CreditWithdrawManage.Image = CreateCreditIcon();
            toolStripButton_CreditWithdrawManage.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButton_CreditWithdrawManage.Size = new System.Drawing.Size(96, 47);
            
            toolStripButton_ClearData.Image = CreateClearIcon();
            toolStripButton_ClearData.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButton_ClearData.Size = new System.Drawing.Size(84, 47);
            
            toolStripButton_Settings.Image = CreateSettingsIcon();
            toolStripButton_Settings.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            toolStripButton_Settings.Size = new System.Drawing.Size(60, 47);
            
            // 设置工具栏高度以容纳图片和文字
            toolStrip1.Size = new System.Drawing.Size(toolStrip1.Size.Width, 50);
            
            // 🔥 初始化 Bingo 数据控件并添加到 panelControl_OpenData
            InitializeBingoDataControls();
            
            // 初始化界面
            _loggingService.Info("微信助手", "微信助手页面已初始化");
        }
        
        /// <summary>
        /// 🔥 初始化 Bingo 数据控件
        /// </summary>
        private void InitializeBingoDataControls()
        {
            // 移除原有的 Label 控件
            panelControl_OpenData.Controls.Clear();
            
            // 创建当前期控件
            _ucBingoDataCur = new UcBingoDataCur
            {
                Location = new System.Drawing.Point(0, 0),
                Dock = DockStyle.Top
            };
            panelControl_OpenData.Controls.Add(_ucBingoDataCur);
            
            // 创建上期控件
            _ucBingoDataLast = new UcBingoDataLast
            {
                Location = new System.Drawing.Point(0, 90),
                Dock = DockStyle.Top
            };
            panelControl_OpenData.Controls.Add(_ucBingoDataLast);
            
            _loggingService.Info("微信助手", "Bingo数据控件已初始化");
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
                if (_ucBingoDataCur != null && _gameService != null)
                {
                    _ucBingoDataCur.SetLotteryService(_gameService);
                    _loggingService.Info("微信助手", "当前期控件已绑定游戏服务");
                }
                
                if (_ucBingoDataLast != null && _gameService != null)
                {
                    _ucBingoDataLast.SetLotteryService(_gameService);
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
            _loggingService.Info("微信助手", "设置按钮被点击");
        }
    }
}

