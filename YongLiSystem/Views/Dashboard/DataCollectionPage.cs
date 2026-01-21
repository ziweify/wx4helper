using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Unit.Browser.Controls;
using YongLiSystem.Models.Dashboard;
using YongLiSystem.Services.Dashboard;
using YongLiSystem.ViewModels.Dashboard;
using YongLiSystem.Views.Dashboard.Monitors;
using YongLiSystem.Views.Dashboard.Controls;
using static YongLiSystem.Views.Dashboard.Controls.MonitorConfigContainerControl;

namespace YongLiSystem.Views.Dashboard
{
    /// <summary>
    /// 数据采集页面
    /// </summary>
    public partial class DataCollectionPage : Form
    {
        private readonly DataCollectionViewModel _viewModel;
        private readonly DataCollectionService _dataCollectionService;
        private MonitorConfigContainerControl? _monitorConfigContainer;
        private readonly List<ScriptTask> _scriptTasks = new List<ScriptTask>();
        private readonly Dictionary<int, (ScriptTaskCardControl card, BrowserWindowProxy? proxy)> _taskControls 
            = new Dictionary<int, (ScriptTaskCardControl, BrowserWindowProxy?)>();

        public DataCollectionPage()
        {
            InitializeComponent();
            
            // 设置为非顶级窗口，可以嵌入到 TabPage 中
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            
            _viewModel = new DataCollectionViewModel();
            _dataCollectionService = new DataCollectionService();
            
            InitializeBindings();
            InitializeMonitorConfig();
            InitializeScriptTasks();
        }

        /// <summary>
        /// 初始化脚本任务功能
        /// </summary>
        private void InitializeScriptTasks()
        {
            // 绑定添加按钮事件
            buttonAddScriptTask.Click += OnAddScriptTaskClick;
            
            // 加载已保存的任务
            LoadScriptTasks();
        }

        /// <summary>
        /// 加载所有脚本任务
        /// </summary>
        private void LoadScriptTasks()
        {
            try
            {
                var tasks = _dataCollectionService.LoadAllScriptTasks();
                foreach (var task in tasks)
                {
                    AddScriptTaskCard(task);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载脚本任务失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 点击添加脚本任务按钮
        /// </summary>
        private void OnAddScriptTaskClick(object? sender, EventArgs e)
        {
            try
            {
                // 创建MonitorConfigControl作为配置对话框
                using var configDialog = new Form
                {
                    Text = "添加脚本任务",
                    Width = 800,
                    Height = 600,
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var configControl = new MonitorConfigControl
                {
                    Dock = DockStyle.Fill
                };

                var btnPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50
                };

                var btnOK = new DevExpress.XtraEditors.SimpleButton
                {
                    Text = "确定",
                    DialogResult = DialogResult.OK,
                    Width = 100,
                    Height = 30,
                    Location = new System.Drawing.Point(580, 10)
                };

                var btnCancel = new DevExpress.XtraEditors.SimpleButton
                {
                    Text = "取消",
                    DialogResult = DialogResult.Cancel,
                    Width = 100,
                    Height = 30,
                    Location = new System.Drawing.Point(690, 10)
                };

                btnPanel.Controls.Add(btnOK);
                btnPanel.Controls.Add(btnCancel);
                configDialog.Controls.Add(configControl);
                configDialog.Controls.Add(btnPanel);
                configDialog.AcceptButton = btnOK;
                configDialog.CancelButton = btnCancel;

                if (configDialog.ShowDialog(this) == DialogResult.OK)
                {
                    // 创建新任务
                    var task = new ScriptTask
                    {
                        Name = string.IsNullOrWhiteSpace(configControl.Url) 
                            ? $"任务 {DateTime.Now:HHmmss}" 
                            : $"任务 - {new Uri(configControl.Url).Host}",
                        Url = configControl.Url,
                        Username = configControl.Username,
                        Password = configControl.Password,
                        AutoLogin = configControl.AutoLogin,
                        Script = configControl.Script,
                        CreatedTime = DateTime.Now,
                        Status = "待启动"
                    };

                    // 保存到数据库
                    if (_dataCollectionService.SaveScriptTask(task))
                    {
                        // 添加到界面
                        AddScriptTaskCard(task);
                        MessageBox.Show("脚本任务添加成功！", "成功", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("保存脚本任务失败！", "错误", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加脚本任务失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 添加任务卡片到界面
        /// </summary>
        private void AddScriptTaskCard(ScriptTask task)
        {
            var card = new ScriptTaskCardControl
            {
                Task = task,
                Width = 280,
                Height = 220,
                Margin = new Padding(5)
            };

            // 订阅事件
            card.EditClicked += (s, e) => OnEditTask(task);
            card.DeleteClicked += (s, e) => OnDeleteTask(task, card);
            card.StartStopClicked += (s, e) => OnStartStopTask(task, card);

            flowLayoutTasks.Controls.Add(card);
            
            // 保存到字典
            _taskControls[task.Id] = (card, null);
            _scriptTasks.Add(task);
        }

        /// <summary>
        /// 编辑任务
        /// </summary>
        private void OnEditTask(ScriptTask task)
        {
            try
            {
                using var configDialog = new Form
                {
                    Text = "编辑脚本任务",
                    Width = 800,
                    Height = 600,
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var configControl = new MonitorConfigControl
                {
                    Dock = DockStyle.Fill,
                    Url = task.Url,
                    Username = task.Username,
                    Password = task.Password,
                    AutoLogin = task.AutoLogin,
                    Script = task.Script
                };

                var btnPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50
                };

                var btnOK = new DevExpress.XtraEditors.SimpleButton
                {
                    Text = "确定",
                    DialogResult = DialogResult.OK,
                    Width = 100,
                    Height = 30,
                    Location = new System.Drawing.Point(580, 10)
                };

                var btnCancel = new DevExpress.XtraEditors.SimpleButton
                {
                    Text = "取消",
                    DialogResult = DialogResult.Cancel,
                    Width = 100,
                    Height = 30,
                    Location = new System.Drawing.Point(690, 10)
                };

                btnPanel.Controls.Add(btnOK);
                btnPanel.Controls.Add(btnCancel);
                configDialog.Controls.Add(configControl);
                configDialog.Controls.Add(btnPanel);

                if (configDialog.ShowDialog(this) == DialogResult.OK)
                {
                    // 更新任务
                    task.Url = configControl.Url;
                    task.Username = configControl.Username;
                    task.Password = configControl.Password;
                    task.AutoLogin = configControl.AutoLogin;
                    task.Script = configControl.Script;

                    // 保存到数据库
                    if (_dataCollectionService.SaveScriptTask(task))
                    {
                        // 更新UI
                        if (_taskControls.TryGetValue(task.Id, out var control))
                        {
                            control.card.Task = task;
                        }
                        MessageBox.Show("脚本任务更新成功！", "成功", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"编辑脚本任务失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        private void OnDeleteTask(ScriptTask task, ScriptTaskCardControl card)
        {
            try
            {
                var result = MessageBox.Show($"确定要删除任务 \"{task.Name}\" 吗？", "确认删除", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // 如果正在运行，先停止
                    if (task.IsRunning)
                    {
                        StopTask(task);
                    }

                    // 从数据库删除
                    if (_dataCollectionService.DeleteScriptTask(task.Id))
                    {
                        // 从界面删除
                        flowLayoutTasks.Controls.Remove(card);
                        _taskControls.Remove(task.Id);
                        _scriptTasks.Remove(task);
                        card.Dispose();
                        
                        MessageBox.Show("脚本任务已删除！", "成功", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除脚本任务失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 启动/停止任务
        /// </summary>
        private async void OnStartStopTask(ScriptTask task, ScriptTaskCardControl card)
        {
            try
            {
                if (task.IsRunning)
                {
                    StopTask(task);
                }
                else
                {
                    // 使用 async void 可以直接调用 async 方法
                    StartTask(task, card);
                }

                // 保存状态
                _dataCollectionService.SaveScriptTask(task);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        private async void StartTask(ScriptTask task, ScriptTaskCardControl card)
        {
            try
            {
                // 创建浏览器窗口
                var proxy = new BrowserWindowProxy();
                
                // 初始化浏览器窗口
                var windowTitle = $"{task.Name} - {task.Id}";
                await proxy.InitializeAsync(windowTitle, task.Url);

                // 设置到卡片控件
                card.SetBrowserProxy(proxy);

                // 更新状态
                task.IsRunning = true;
                task.Status = "运行中";
                task.LastRunTime = DateTime.Now;
                card.Task = task; // 触发UI更新

                // 保存浏览器代理
                _taskControls[task.Id] = (card, proxy);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动任务失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 回滚状态
                task.IsRunning = false;
                task.Status = "启动失败";
                card.Task = task;
            }
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        private void StopTask(ScriptTask task)
        {
            if (_taskControls.TryGetValue(task.Id, out var control))
            {
                // 关闭浏览器窗口
                control.proxy?.CloseWindow();
                control.proxy?.Dispose();

                // 更新状态
                task.IsRunning = false;
                task.Status = "已停止";
                control.card.Task = task; // 触发UI更新

                // 更新字典
                _taskControls[task.Id] = (control.card, null);
            }
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



            // 绑定按钮命令
            btnGetIssueInfo.Click += (s, e) => _viewModel.GetIssueInfoCommand?.Execute(null);
            btnStartAuto.Click += (s, e) => _viewModel.StartAutoCollectionCommand?.Execute(null);
            btnStopAuto.Click += (s, e) => _viewModel.StopAutoCollectionCommand?.Execute(null);

            
            // 注释掉不存在的按钮
            // btnManualCollect.Click += (s, e) => _viewModel.ManualCollectCommand?.Execute(null);
            // btnClearCompleted.Click += (s, e) => _viewModel.ClearCompletedCommand?.Execute(null);
            // btnExportData.Click += (s, e) => _viewModel.ExportDataCommand?.Execute(null);

        }

    }
}

