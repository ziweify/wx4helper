using System;
using System.Windows.Forms;
using 永利系统.Services;

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

        public WechatPage()
        {
            InitializeComponent();
            
            // 设置为非顶级窗口，可以嵌入到 TabPage 中
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            
            _loggingService = LoggingService.Instance;
            InitializeUI();
            StartAutoRefresh();
            
            // 订阅 FormClosing 事件以清理资源
            FormClosing += WechatPage_FormClosing;
        }

        private void InitializeUI()
        {
            // 初始化界面
            _loggingService.Info("微信助手", "微信助手页面已初始化");
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

