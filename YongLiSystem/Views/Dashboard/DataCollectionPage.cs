using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using YongLiSystem.Models.Dashboard;
using YongLiSystem.Services.Dashboard;
using YongLiSystem.ViewModels.Dashboard;
using YongLiSystem.Views.Dashboard.Monitors;
using YongLiSystem.Views.Dashboard.Controls; // For MonitorConfigContainerControl
using YongLiSystem.Helpers;
using Unit.La.Controls;
using Unit.La.Models;
using Unit.La.Scripting;

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
        private readonly Dictionary<int, (BrowserTaskCardControl card, BrowserTaskControl? window)> _taskControls
            = new Dictionary<int, (BrowserTaskCardControl, BrowserTaskControl?)>();

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
                // 🔥 生成唯一的任务ID和脚本目录
                var taskId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var taskName = $"任务_{DateTime.Now:HHmmss}";
                var scriptDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Scripts",
                    $"Task_{taskId}"
                );

                // 🔥 自动创建脚本目录和模板文件
                Unit.La.Scripting.LocalScriptLoader.CreateDefaultScripts(scriptDirectory);

                // 创建新任务（使用默认值）
                var task = new ScriptTask
                {
                    Name = taskName,
                    Url = "https://www.baidu.com",
                    Username = "",
                    Password = "",
                    AutoLogin = false,
                    Script = scriptDirectory, // 🔥 存储脚本目录路径
                    CreatedTime = DateTime.Now,
                    Status = "待启动"
                };

                // 保存到数据库
                if (_dataCollectionService.SaveScriptTask(task))
                {
                    // 添加到界面
                    AddScriptTaskCard(task);
                    
                    // 立即打开编辑窗口（这样用户可以修改配置）
                    OnEditTask(task, _taskControls[task.Id].card);
                    
                    MessageBox.Show($"脚本任务已创建！\n脚本目录: {scriptDirectory}\n已自动生成 main.lua 和 functions.lua 模板。", 
                        "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("保存脚本任务失败！", "错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var card = new BrowserTaskCardControl
            {
                TaskInfo = task.ToBrowserTaskInfo(), // 使用扩展方法转换
                Width = 280,
                Height = 240,  // 增加高度以容纳缩略图
                Margin = new Padding(5)
            };

            // 订阅事件
            card.DeleteClicked += (s, e) => OnDeleteTask(task, card);
            card.StartStopClicked += (s, e) => OnStartStopTask(task, card);
            card.EditClicked += (s, e) => OnEditTask(task, card);
            card.CloseClicked += (s, e) => OnCloseTask(task, card);
            card.ThumbnailClicked += (s, e) => OnEditTask(task, card); // 缩略图点击同"编辑"

            flowLayoutTasks.Controls.Add(card);

            // 保存到字典
            _taskControls[task.Id] = (card, null);
            _scriptTasks.Add(task);
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        private void OnDeleteTask(ScriptTask task, BrowserTaskCardControl card)
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
        private void OnStartStopTask(ScriptTask task, BrowserTaskCardControl card)
        {
            try
            {
                if (task.IsRunning)
                {
                    StopTask(task);
                }
                else
                {
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
        /// 启动任务 - 后台执行脚本（不显示窗口）
        /// </summary>
        private void StartTask(ScriptTask task, BrowserTaskCardControl card)
        {
            try
            {
                // 🔥 确保浏览器任务控件已初始化
                if (!_taskControls.TryGetValue(task.Id, out var controlPair) || controlPair.window == null)
                {
                    // 初始化浏览器任务控件（但不显示窗口）
                    InitializeBrowserTaskControl(task, card);
                }

                // 获取控件
                var browserControl = _taskControls[task.Id].window;
                if (browserControl == null)
                {
                    throw new InvalidOperationException("浏览器任务控件初始化失败");
                }

                // 🔥 后台执行脚本（不显示窗口）
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        // 在独立线程启动，避免阻塞 UI
                        browserControl.Invoke(new Action(() =>
                        {
                            browserControl.ExecuteScript(browserControl.Config.Script);
                        }));
                    }
                    catch (Exception ex)
                    {
                        browserControl.Invoke(new Action(() =>
                        {
                            MessageBox.Show($"脚本执行失败: {ex.Message}", "错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                });
                
                // 更新状态
                task.IsRunning = true;
                task.Status = "运行中";
                task.LastRunTime = DateTime.Now;
                card.TaskInfo = task.ToBrowserTaskInfo();
                
                // 保存状态
                _dataCollectionService.SaveScriptTask(task);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动任务失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 回滚状态
                task.IsRunning = false;
                task.Status = "启动失败";
                card.TaskInfo = task.ToBrowserTaskInfo();
            }
        }

        /// <summary>
        /// 编辑任务 - 显示窗口（浏览器+编辑器+日志）
        /// </summary>
        private void OnEditTask(ScriptTask task, BrowserTaskCardControl card)
        {
            try
            {
                // 🔥 确保浏览器任务控件已初始化
                if (!_taskControls.TryGetValue(task.Id, out var controlPair) || controlPair.window == null)
                {
                    InitializeBrowserTaskControl(task, card);
                }

                // 获取控件
                var browserControl = _taskControls[task.Id].window;
                if (browserControl == null)
                {
                    throw new InvalidOperationException("浏览器任务控件初始化失败");
                }

                // 🔥 显示窗口（恢复透明度和任务栏显示）
                browserControl.Opacity = 1.0;         // 恢复不透明
                browserControl.ShowInTaskbar = true;  // 显示在任务栏
                browserControl.Show();
                browserControl.WindowState = FormWindowState.Normal; // 正常大小
                browserControl.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开编辑窗口失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 初始化浏览器任务控件（隐藏运行）
        /// </summary>
        private void InitializeBrowserTaskControl(ScriptTask task, BrowserTaskCardControl card)
        {
            try
            {
                // 转换配置
                var config = task.ToScriptTaskConfig();
                
                // 创建浏览器任务控件
                var browserControl = new BrowserTaskControl(config);
                
                // 🔥 设置为隐藏模式（透明 + 不显示任务栏）
                browserControl.Opacity = 0;           // 完全透明
                browserControl.ShowInTaskbar = false; // 不显示在任务栏
                browserControl.Show();                // 显示窗口（但透明，所以看不见）
                
                // 订阅缩略图更新事件
                browserControl.ThumbnailUpdated += (s, thumbnail) =>
                {
                    card.UpdateThumbnail(thumbnail);
                };
                
                // 🔥 订阅配置变更事件，保存到数据库
                browserControl.ConfigChanged += (s, changedConfig) =>
                {
                    try
                    {
                        // 更新任务对象
                        task.UpdateFromConfig(changedConfig);
                        
                        // 保存到数据库
                        if (_dataCollectionService.SaveScriptTask(task))
                        {
                            // 更新卡片显示
                            card.TaskInfo = task.ToBrowserTaskInfo();
                            System.Diagnostics.Debug.WriteLine($"[DataCollectionPage] 配置已保存到数据库: {task.Name}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[DataCollectionPage] 保存配置到数据库失败: {task.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DataCollectionPage] 保存配置异常: {ex.Message}");
                    }
                };
                
                // 保存到字典
                _taskControls[task.Id] = (card, browserControl);
            }
            catch (Exception ex)
            {
                throw new Exception($"初始化浏览器任务控件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        private void StopTask(ScriptTask task)
        {
            if (_taskControls.TryGetValue(task.Id, out var control))
            {
                // 关闭浏览器窗口（隐藏，不释放）
                control.window?.Hide();

                // 更新状态
                task.IsRunning = false;
                task.Status = "已停止";
                control.card.TaskInfo = task.ToBrowserTaskInfo(); // 触发UI更新
                
                // 🔥 真正停止任务时，重置缩略图为"未启动"
                control.card.ResetThumbnail();

                // 更新字典
                _taskControls[task.Id] = (control.card, control.window);
            }
        }

        /// <summary>
        /// 关闭任务（真正释放资源）
        /// </summary>
        private void OnCloseTask(ScriptTask task, BrowserTaskCardControl card)
        {
            try
            {
                var result = MessageBox.Show(
                    $"确定要关闭任务 \"{task.Name}\" 吗？\n\n关闭后将释放浏览器资源，需要重新启动。", 
                    "确认关闭",
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (_taskControls.TryGetValue(task.Id, out var control))
                    {
                        // 真正关闭并释放资源
                        control.window?.CloseAndDispose();

                        // 更新状态
                        task.IsRunning = false;
                        task.Status = "已关闭";
                        card.TaskInfo = task.ToBrowserTaskInfo();
                        
                        // 🔥 真正关闭任务时，重置缩略图为"未启动"
                        card.ResetThumbnail();
                        
                        _dataCollectionService.SaveScriptTask(task);

                        // 更新字典
                        _taskControls[task.Id] = (control.card, null);

                        MessageBox.Show("任务已关闭并释放资源！", "成功",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"关闭任务失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 缩略图点击 - 显示隐藏的窗口
        /// </summary>
        private void OnThumbnailClicked(ScriptTask task)
        {
            if (_taskControls.TryGetValue(task.Id, out var control))
            {
                if (control.window != null && !control.window.IsDisposed)
                {
                    if (control.window.Visible)
                    {
                        control.window.Activate();
                        control.window.BringToFront();
                    }
                    else
                    {
                        control.window.Show();
                        control.window.Activate();
                    }
                }
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

