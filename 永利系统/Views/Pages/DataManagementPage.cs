using System;
using System.Windows.Forms;
using 永利系统.ViewModels;

namespace 永利系统.Views.Pages
{
    /// <summary>
    /// 数据管理页面 - 使用 Form 实现，支持后台自动刷新
    /// </summary>
    public partial class DataManagementPage : Form
    {
        private readonly DataManagementViewModel _viewModel;
        private System.Windows.Forms.Timer? _refreshTimer;

        public DataManagementPage()
        {
            InitializeComponent();
            
            // 设置为非顶级窗口，可以嵌入到 TabPage 中
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            
            _viewModel = new DataManagementViewModel();
            BindViewModel();
            StartAutoRefresh();
            
            // 订阅 FormClosing 事件以清理资源
            FormClosing += DataManagementPage_FormClosing;
        }

        private void BindViewModel()
        {
            // 绑定数据
            if (gridControl1 != null)
                gridControl1.DataSource = _viewModel.DataItems;
            if (txtSearch != null)
                txtSearch.DataBindings.Add("Text", _viewModel, nameof(_viewModel.SearchText));
            
            // 绑定命令
            if (btnAdd != null)
                btnAdd.Click += (s, e) => _viewModel.AddCommand?.Execute(null);
            if (btnEdit != null)
                btnEdit.Click += (s, e) => _viewModel.EditCommand?.Execute(null);
            if (btnDelete != null)
                btnDelete.Click += (s, e) => _viewModel.DeleteCommand?.Execute(null);
            if (btnSearch != null)
                btnSearch.Click += (s, e) => _viewModel.SearchCommand?.Execute(null);
        }

        /// <summary>
        /// 启动自动刷新（即使页面不可见也会运行）
        /// </summary>
        private void StartAutoRefresh()
        {
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 10000 // 每10秒刷新一次
            };
            _refreshTimer.Tick += (s, e) =>
            {
                // 后台自动刷新数据
                _viewModel.RefreshData();
            };
            _refreshTimer.Start();
        }

        private void DataManagementPage_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 清理 Timer
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }

        private void DataManagementPage_Load(object sender, EventArgs e)
        {
            // 数据加载已在 ViewModel 构造函数中完成
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (view != null && view.FocusedRowHandle >= 0)
            {
                _viewModel.SelectedItem = view.GetRow(view.FocusedRowHandle) as 永利系统.Models.DataItem;
            }
        }
    }
}
