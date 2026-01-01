using System;
using System.Windows.Forms;
using 永利系统.ViewModels.Dashboard;
using 永利系统.Views.Dashboard.Monitors;
using 永利系统.Views.Dashboard.Controls;
using static 永利系统.Views.Dashboard.Controls.MonitorConfigContainerControl;

namespace 永利系统.Views.Dashboard
{
    /// <summary>
    /// 数据采集页面
    /// </summary>
    public partial class DataCollectionPage : Form
    {
        private readonly DataCollectionViewModel _viewModel;
        private MonitorA_Control? _monitorAControl;
        private MonitorB_Control? _monitorBControl;
        private MonitorC_Control? _monitorCControl;
        private MonitorConfigContainerControl? _monitorConfigContainer;

        public DataCollectionPage()
        {
            InitializeComponent();
            
            // 设置为非顶级窗口，可以嵌入到 TabPage 中
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            
            _viewModel = new DataCollectionViewModel();
            InitializeBindings();
            InitializeMonitorControls();
            InitializeMonitorConfig();
            ConnectMonitorsWithConfig();
        }

        /// <summary>
        /// 初始化监控配置
        /// </summary>
        private void InitializeMonitorConfig()
        {
            // 创建监控配置容器并添加到 groupControl_Monitor_config
            _monitorConfigContainer = new MonitorConfigContainerControl
            {
                Dock = DockStyle.Fill
            };
            groupControl_Monitor_config.Controls.Add(_monitorConfigContainer);

            // 设置默认配置
            SetDefaultConfigs();
        }

        /// <summary>
        /// 设置默认配置
        /// </summary>
        private void SetDefaultConfigs()
        {
            if (_monitorConfigContainer == null) return;

            // 监控A默认配置
            if (_monitorConfigContainer.MonitorAConfig != null)
            {
                _monitorConfigContainer.MonitorAConfig.Url = "https://www.taiwanlottery.com.tw/lotto/BingoBingo/OEHLStatistic.htm";
                _monitorConfigContainer.MonitorAConfig.Username = "";
                _monitorConfigContainer.MonitorAConfig.Password = "";
                _monitorConfigContainer.MonitorAConfig.AutoLogin = false;
                _monitorConfigContainer.MonitorAConfig.Script = @"
(function() {
    try {
        var issueEl = document.querySelector('#right_overflow_hinet > div');
        if (issueEl) {
            return {
                success: true,
                text: issueEl.innerText
            };
        }
        return { success: false, message: '未找到元素' };
    } catch(e) {
        return { success: false, message: e.message };
    }
})();";
            }

            // 监控B默认配置
            if (_monitorConfigContainer.MonitorBConfig != null)
            {
                _monitorConfigContainer.MonitorBConfig.Url = "https://example.com/monitor-b";
                _monitorConfigContainer.MonitorBConfig.AutoLogin = false;
            }

            // 监控C默认配置
            if (_monitorConfigContainer.MonitorCConfig != null)
            {
                _monitorConfigContainer.MonitorCConfig.Url = "https://example.com/monitor-c";
                _monitorConfigContainer.MonitorCConfig.AutoLogin = false;
            }
        }

        /// <summary>
        /// 连接监控控件与配置
        /// </summary>
        private async void ConnectMonitorsWithConfig()
        {
            if (_monitorConfigContainer == null) return;

            // 订阅监控命令事件
            _monitorConfigContainer.MonitorCommand += OnMonitorCommand;

            // 连接监控A - 异步初始化浏览器
            if (_monitorAControl != null && _monitorConfigContainer.MonitorAConfig != null)
            {
                await _monitorAControl.SetConfigAsync(_monitorConfigContainer.MonitorAConfig);
            }

            // 连接监控B - 异步初始化浏览器
            if (_monitorBControl != null && _monitorConfigContainer.MonitorBConfig != null)
            {
                await _monitorBControl.SetConfigAsync(_monitorConfigContainer.MonitorBConfig);
            }

            // 连接监控C - 异步初始化浏览器
            if (_monitorCControl != null && _monitorConfigContainer.MonitorCConfig != null)
            {
                await _monitorCControl.SetConfigAsync(_monitorConfigContainer.MonitorCConfig);
            }
        }

        /// <summary>
        /// 处理监控命令
        /// </summary>
        private async void OnMonitorCommand(object? sender, MonitorCommandEventArgs e)
        {
            try
            {
                switch (e.MonitorName)
                {
                    case "MonitorA":
                        await _monitorAControl?.ExecuteMonitorCommand(e.CommandName)!;
                        break;
                    case "MonitorB":
                        await _monitorBControl?.ExecuteMonitorCommand(e.CommandName)!;
                        break;
                    case "MonitorC":
                        await _monitorCControl?.ExecuteMonitorCommand(e.CommandName)!;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行监控命令失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 初始化监控控件
        /// </summary>
        private void InitializeMonitorControls()
        {
            // 创建监控A控件并添加到对应的TabPage
            _monitorAControl = new MonitorA_Control
            {
                Dock = DockStyle.Fill
            };
            xtraTabPageMonitorA.Controls.Add(_monitorAControl);

            // 创建监控B控件
            _monitorBControl = new MonitorB_Control
            {
                Dock = DockStyle.Fill
            };
            xtraTabPageMonitorB.Controls.Add(_monitorBControl);

            // 创建监控C控件
            _monitorCControl = new MonitorC_Control
            {
                Dock = DockStyle.Fill
            };
            xtraTabPageMonitorC.Controls.Add(_monitorCControl);
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
            btnTestConnection.Click += (s, e) => _viewModel.TestConnectionCommand?.Execute(null);
            
            // 注释掉不存在的按钮
            // btnManualCollect.Click += (s, e) => _viewModel.ManualCollectCommand?.Execute(null);
            // btnClearCompleted.Click += (s, e) => _viewModel.ClearCompletedCommand?.Execute(null);
            // btnExportData.Click += (s, e) => _viewModel.ExportDataCommand?.Execute(null);

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

