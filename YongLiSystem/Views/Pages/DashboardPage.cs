using System;
using System.Windows.Forms;
using 永利系统.ViewModels;

namespace 永利系统.Views.Pages
{
    /// <summary>
    /// 主页 - 使用 Form 实现，支持后台自动刷新
    /// </summary>
    public partial class DashboardPage : Form
    {
        private readonly DashboardViewModel _viewModel;
        private System.Windows.Forms.Timer? _refreshTimer;

        public DashboardPage()
        {
            InitializeComponent();
            
            // 设置为非顶级窗口，可以嵌入到 TabPage 中
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            
            _viewModel = new DashboardViewModel();
            BindViewModel();
            StartAutoRefresh();
            
            // 订阅 FormClosing 事件以清理资源
            FormClosing += DashboardPage_FormClosing;
        }

        private void BindViewModel()
        {
            // 绑定数据到控件
            if (lblTotalRecords != null)
                lblTotalRecords.DataBindings.Add("Text", _viewModel, nameof(_viewModel.TotalRecords));
            if (lblTodayRecords != null)
                lblTodayRecords.DataBindings.Add("Text", _viewModel, nameof(_viewModel.TodayRecords));
            if (lblTotalAmount != null)
                lblTotalAmount.DataBindings.Add("Text", _viewModel, nameof(_viewModel.TotalAmount));
        }

        /// <summary>
        /// 启动自动刷新（即使页面不可见也会运行）
        /// </summary>
        private void StartAutoRefresh()
        {
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000 // 每5秒刷新一次
            };
            _refreshTimer.Tick += (s, e) =>
            {
                // 后台自动刷新数据
                _viewModel.RefreshData();
            };
            _refreshTimer.Start();
        }

        private void DashboardPage_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 清理 Timer
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }

        private void DashboardPage_Load(object sender, EventArgs e)
        {
            // 数据加载已在 ViewModel 构造函数中完成
        }
    }
}
