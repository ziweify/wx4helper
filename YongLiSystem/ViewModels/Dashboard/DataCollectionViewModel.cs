using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using DevExpress.Mvvm;
using 永利系统.Models.Dashboard;
using 永利系统.Services;
using 永利系统.Services.Dashboard;

namespace 永利系统.ViewModels.Dashboard
{
    /// <summary>
    /// 数据采集页面 ViewModel
    /// </summary>
    public class DataCollectionViewModel : ViewModelBase
    {
        private readonly DataCollectionService _collectionService;
        private readonly LoggingService _loggingService;
        private System.Windows.Forms.Timer? _countdownTimer;

        // 待采集任务列表
        private ObservableCollection<DataCollectionTask> _pendingTasks;

        // 已完成任务列表
        private ObservableCollection<DataCollectionTask> _completedTasks;

        // 配置
        private DataCollectionConfig _config;

        public DataCollectionViewModel()
        {
            _collectionService = new DataCollectionService();
            _loggingService = LoggingService.Instance;

            _pendingTasks = new ObservableCollection<DataCollectionTask>();
            _completedTasks = new ObservableCollection<DataCollectionTask>();
            _config = new DataCollectionConfig();

            // 初始化命令
            GetIssueInfoCommand = new DelegateCommand(GetIssueInfo);
            StartAutoCollectionCommand = new DelegateCommand(StartAutoCollection);
            StopAutoCollectionCommand = new DelegateCommand(StopAutoCollection);
            ManualCollectCommand = new DelegateCommand(ManualCollect);
            TestConnectionCommand = new DelegateCommand(TestConnection);
            ClearCompletedCommand = new DelegateCommand(ClearCompleted);
            ExportDataCommand = new DelegateCommand(ExportData);

            InitializeCountdownTimer();
        }

        #region 属性

        /// <summary>
        /// 待采集任务列表
        /// </summary>
        public ObservableCollection<DataCollectionTask> PendingTasks
        {
            get => _pendingTasks;
            set
            {
                if (_pendingTasks != value)
                {
                    _pendingTasks = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(PendingCount));
                }
            }
        }

        /// <summary>
        /// 已完成任务列表
        /// </summary>
        public ObservableCollection<DataCollectionTask> CompletedTasks
        {
            get => _completedTasks;
            set
            {
                if (_completedTasks != value)
                {
                    _completedTasks = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CompletedCount));
                }
            }
        }

        /// <summary>
        /// 配置
        /// </summary>
        public DataCollectionConfig Config
        {
            get => _config;
            set
            {
                if (_config != value)
                {
                    _config = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 待采集数量
        /// </summary>
        public int PendingCount => PendingTasks?.Count ?? 0;

        /// <summary>
        /// 已完成数量
        /// </summary>
        public int CompletedCount => CompletedTasks?.Count ?? 0;

        /// <summary>
        /// 倒计时文本
        /// </summary>
        public string CountdownText
        {
            get
            {
                if (Config.Countdown <= 0)
                    return "00:00";

                var minutes = Config.Countdown / 60;
                var seconds = Config.Countdown % 60;
                return $"{minutes:D2}:{seconds:D2}";
            }
        }

        #endregion

        #region 命令

        public ICommand? GetIssueInfoCommand { get; }
        public ICommand? StartAutoCollectionCommand { get; }
        public ICommand? StopAutoCollectionCommand { get; }
        public ICommand? ManualCollectCommand { get; }
        public ICommand? TestConnectionCommand { get; }
        public ICommand? ClearCompletedCommand { get; }
        public ICommand? ExportDataCommand { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化倒计时定时器
        /// </summary>
        private void InitializeCountdownTimer()
        {
            _countdownTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // 每秒更新
            };
            _countdownTimer.Tick += (s, e) =>
            {
                if (Config.Countdown > 0)
                {
                    Config.Countdown--;
                    RaisePropertyChanged(nameof(CountdownText));
                }
            };
            _countdownTimer.Start();
        }

        /// <summary>
        /// 获取期号信息
        /// </summary>
        private async void GetIssueInfo()
        {
            try
            {
                _loggingService.Info("数据采集", "获取期号信息...");

                var (currentIssue, currentTime, nextIssue, nextTime) = 
                    await _collectionService.GetIssueInfoAsync(Config.DataSourceUrl);

                Config.CurrentIssue = currentIssue;
                Config.CurrentOpenTime = currentTime;
                Config.NextIssue = nextIssue;
                Config.NextOpenTime = nextTime;

                // 添加下期到待采集列表
                if (!string.IsNullOrEmpty(nextIssue) && int.TryParse(nextIssue, out int issueId))
                {
                    var existingTask = PendingTasks.FirstOrDefault(t => t.IssueId == issueId);
                    if (existingTask == null)
                    {
                        PendingTasks.Add(new DataCollectionTask
                        {
                            Id = PendingTasks.Count + 1,
                            IssueId = issueId,
                            Status = CollectionTaskStatus.Pending,
                            DataSource = Config.DataSourceUrl,
                            CreatedTime = DateTime.Now,
                            UpdatedTime = DateTime.Now
                        });
                        RaisePropertyChanged(nameof(PendingCount));
                    }
                }

                MessageBox.Show($"期号信息获取成功!\n当期: {currentIssue}\n下期: {nextIssue}", 
                    "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Error("数据采集", $"获取期号信息失败: {ex.Message}", ex);
                MessageBox.Show($"获取期号信息失败:\n{ex.Message}", 
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 开始自动采集
        /// </summary>
        private void StartAutoCollection()
        {
            if (Config.IsAutoCollecting)
            {
                _loggingService.Warn("数据采集", "自动采集已在运行中");
                return;
            }

            Config.IsAutoCollecting = true;
            _collectionService.StartAutoCollection(Config.CollectionInterval, OnDataCollected);
            _loggingService.Info("数据采集", "自动采集已启动");
        }

        /// <summary>
        /// 停止自动采集
        /// </summary>
        private void StopAutoCollection()
        {
            if (!Config.IsAutoCollecting)
            {
                return;
            }

            Config.IsAutoCollecting = false;
            _collectionService.StopAutoCollection();
            _loggingService.Info("数据采集", "自动采集已停止");
        }

        /// <summary>
        /// 手动采集
        /// </summary>
        private async void ManualCollect()
        {
            if (string.IsNullOrEmpty(Config.NextIssue))
            {
                MessageBox.Show("请先获取期号信息", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(Config.NextIssue, out int issueId))
            {
                MessageBox.Show("期号格式不正确", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _loggingService.Info("数据采集", $"手动采集期号: {issueId}");

                var task = await _collectionService.CollectDataAsync(issueId, Config.DataSourceUrl);

                // 更新待采集列表中的任务
                var existingTask = PendingTasks.FirstOrDefault(t => t.IssueId == issueId);
                if (existingTask != null)
                {
                    PendingTasks.Remove(existingTask);
                }

                // 如果采集成功,添加到已完成列表
                if (task.Status == CollectionTaskStatus.Completed)
                {
                    CompletedTasks.Insert(0, task);
                    RaisePropertyChanged(nameof(CompletedCount));

                    // 投递数据
                    if (!string.IsNullOrWhiteSpace(Config.SubmitAddresses))
                    {
                        await _collectionService.SubmitDataAsync(task, Config.SubmitAddresses);
                    }
                }
                else
                {
                    // 失败则重新加回待采集列表
                    task.AttemptCount++;
                    PendingTasks.Add(task);
                }

                RaisePropertyChanged(nameof(PendingCount));

                MessageBox.Show($"采集完成!\n期号: {issueId}\n数据: {task.OpenData}", 
                    "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Error("数据采集", $"手动采集失败: {ex.Message}", ex);
                MessageBox.Show($"采集失败:\n{ex.Message}", 
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        private async void TestConnection()
        {
            if (string.IsNullOrWhiteSpace(Config.DataSourceUrl))
            {
                MessageBox.Show("请输入数据源地址", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var success = await _collectionService.TestConnectionAsync(Config.DataSourceUrl);

                if (success)
                {
                    MessageBox.Show($"连接成功!\n数据源: {Config.DataSourceUrl}", 
                        "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("连接失败,请检查数据源地址", 
                        "失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error("数据采集", $"测试连接异常: {ex.Message}", ex);
                MessageBox.Show($"连接异常:\n{ex.Message}", 
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 清空已完成列表
        /// </summary>
        private void ClearCompleted()
        {
            if (CompletedTasks.Count == 0)
            {
                MessageBox.Show("已完成列表为空", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show($"确定要清空 {CompletedTasks.Count} 条已完成记录吗?", 
                "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                CompletedTasks.Clear();
                RaisePropertyChanged(nameof(CompletedCount));
                _loggingService.Info("数据采集", "已清空已完成列表");
            }
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        private void ExportData()
        {
            // TODO: 实现导出功能
            MessageBox.Show($"准备导出 {CompletedTasks.Count} 条数据", 
                "导出", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 数据采集完成回调
        /// </summary>
        private void OnDataCollected(DataCollectionTask task)
        {
            // 在UI线程中更新
            Application.OpenForms[0]?.BeginInvoke(new Action(() =>
            {
                var existingTask = PendingTasks.FirstOrDefault(t => t.IssueId == task.IssueId);
                if (existingTask != null)
                {
                    PendingTasks.Remove(existingTask);
                }

                if (task.Status == CollectionTaskStatus.Completed)
                {
                    CompletedTasks.Insert(0, task);
                    RaisePropertyChanged(nameof(CompletedCount));
                }
                else
                {
                    task.AttemptCount++;
                    PendingTasks.Add(task);
                }

                RaisePropertyChanged(nameof(PendingCount));
            }));
        }

        #endregion
    }
}

