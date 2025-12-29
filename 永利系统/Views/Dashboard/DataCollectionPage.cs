using System;
using System.Windows.Forms;
using 永利系统.ViewModels.Dashboard;

namespace 永利系统.Views.Dashboard
{
    /// <summary>
    /// 数据采集页面
    /// </summary>
    public partial class DataCollectionPage : Form
    {
        private readonly DataCollectionViewModel _viewModel;

        public DataCollectionPage()
        {
            InitializeComponent();
            
            // 设置为非顶级窗口，可以嵌入到 TabPage 中
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            
            _viewModel = new DataCollectionViewModel();
            InitializeBindings();
        }

        /// <summary>
        /// 初始化数据绑定
        /// </summary>
        private void InitializeBindings()
        {
            // 绑定待采集列表
            gridPending.DataSource = _viewModel.PendingTasks;

            // 绑定已完成列表
            gridCompleted.DataSource = _viewModel.CompletedTasks;

            // 绑定期号信息
            txtCurrentIssue.DataBindings.Add("EditValue", _viewModel.Config, 
                nameof(_viewModel.Config.CurrentIssue), true, DataSourceUpdateMode.OnPropertyChanged);
            txtCurrentTime.DataBindings.Add("EditValue", _viewModel.Config, 
                nameof(_viewModel.Config.CurrentOpenTime), true, DataSourceUpdateMode.OnPropertyChanged);
            txtNextIssue.DataBindings.Add("EditValue", _viewModel.Config, 
                nameof(_viewModel.Config.NextIssue), true, DataSourceUpdateMode.OnPropertyChanged);
            txtNextTime.DataBindings.Add("EditValue", _viewModel.Config, 
                nameof(_viewModel.Config.NextOpenTime), true, DataSourceUpdateMode.OnPropertyChanged);
            txtCountdown.DataBindings.Add("EditValue", _viewModel, 
                nameof(_viewModel.CountdownText), false, DataSourceUpdateMode.Never);

            // 绑定数据源配置
            txtDataSourceUrl.DataBindings.Add("EditValue", _viewModel.Config, 
                nameof(_viewModel.Config.DataSourceUrl), true, DataSourceUpdateMode.OnPropertyChanged);
            chkUseProxy.DataBindings.Add("Checked", _viewModel.Config, 
                nameof(_viewModel.Config.UseProxy), true, DataSourceUpdateMode.OnPropertyChanged);
            txtProxyAddress.DataBindings.Add("EditValue", _viewModel.Config, 
                nameof(_viewModel.Config.ProxyAddress), true, DataSourceUpdateMode.OnPropertyChanged);
            txtProxyAddress.DataBindings.Add("Enabled", _viewModel.Config, 
                nameof(_viewModel.Config.UseProxy), false, DataSourceUpdateMode.Never);

            // 绑定提交地址
            memoSubmitAddresses.DataBindings.Add("EditValue", _viewModel.Config, 
                nameof(_viewModel.Config.SubmitAddresses), true, DataSourceUpdateMode.OnPropertyChanged);

            // 绑定按钮命令
            btnGetIssueInfo.Click += (s, e) => _viewModel.GetIssueInfoCommand?.Execute(null);
            btnStartAuto.Click += (s, e) => _viewModel.StartAutoCollectionCommand?.Execute(null);
            btnStopAuto.Click += (s, e) => _viewModel.StopAutoCollectionCommand?.Execute(null);
            btnManualCollect.Click += (s, e) => _viewModel.ManualCollectCommand?.Execute(null);
            btnTestConnection.Click += (s, e) => _viewModel.TestConnectionCommand?.Execute(null);
            btnClearCompleted.Click += (s, e) => _viewModel.ClearCompletedCommand?.Execute(null);
            btnExportData.Click += (s, e) => _viewModel.ExportDataCommand?.Execute(null);

            // 初始化默认值
            InitializeDefaultValues();
        }

        /// <summary>
        /// 初始化默认值
        /// </summary>
        private void InitializeDefaultValues()
        {
            // 设置默认提交地址
            var defaultAddresses = @"[w168]http://8.134.71.102/api/api/upload_result.do,
[boter]http://8.134.71.102:789/api/boter/uploadbg,
[wold]http://8.138.183.44/api/task/upload_twbgone?token=asdrv24n33323brf";

            if (string.IsNullOrWhiteSpace(_viewModel.Config.SubmitAddresses))
            {
                _viewModel.Config.SubmitAddresses = defaultAddresses;
            }
        }
    }
}

