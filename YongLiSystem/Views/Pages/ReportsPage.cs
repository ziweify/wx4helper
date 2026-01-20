using System;
using System.Windows.Forms;
using YongLiSystem.Services;

namespace YongLiSystem.Views.Pages
{
    /// <summary>
    /// 报表分析页面 - 使用 Form 实现，支持后台自动刷新
    /// </summary>
    public partial class ReportsPage : Form
    {
        private readonly LoggingService _loggingService;
        private System.Windows.Forms.Timer? _refreshTimer;

        public ReportsPage()
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
            FormClosing += ReportsPage_FormClosing;
        }

        private void InitializeUI()
        {
            // 初始化报表分析页面的UI
            _loggingService.Info("报表分析", "报表分析页面已初始化");
        }

        /// <summary>
        /// 启动自动刷新（即使页面不可见也会运行）
        /// </summary>
        private void StartAutoRefresh()
        {
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 15000 // 每15秒刷新一次报表数据
            };
            _refreshTimer.Tick += (s, e) =>
            {
                // 后台自动刷新报表数据
                _loggingService.Debug("报表分析", "后台自动刷新报表数据...");
                // TODO: 实现报表数据刷新逻辑
            };
            _refreshTimer.Start();
        }

        private void ReportsPage_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 清理 Timer
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }

        private void ReportsPage_Load(object sender, EventArgs e)
        {
            // 页面加载时的初始化
        }
    }
}

