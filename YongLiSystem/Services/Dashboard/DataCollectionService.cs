using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using YongLiSystem.Models.Dashboard;
using YongLiSystem.Services;
using YongLiSystem.Infrastructure.Paths;

namespace YongLiSystem.Services.Dashboard
{
    /// <summary>
    /// 数据采集服务 - 包含脚本任务的数据库操作
    /// </summary>
    public class DataCollectionService
    {
        private readonly LoggingService _loggingService;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _collectionTask;
        private SQLiteConnection? _db;
        private readonly string _dbPath;

        public DataCollectionService()
        {
            _loggingService = LoggingService.Instance;
            _dbPath = Path.Combine(AppPaths.DataDirectory, "app.db");
            InitializeDatabase();
        }

        /// <summary>
        /// 开始自动采集
        /// </summary>
        public void StartAutoCollection(int intervalSeconds, Action<DataCollectionTask> onDataCollected)
        {
            if (_collectionTask != null && !_collectionTask.IsCompleted)
            {
                _loggingService.Warn("数据采集", "采集任务已在运行中");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _collectionTask = Task.Run(async () =>
            {
                _loggingService.Info("数据采集", "自动采集任务已启动");

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // TODO: 实现实际的数据采集逻辑
                        _loggingService.Debug("数据采集", "执行自动采集...");

                        // 模拟采集延迟
                        await Task.Delay(intervalSeconds * 1000, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error("数据采集", $"自动采集异常: {ex.Message}", ex);
                    }
                }

                _loggingService.Info("数据采集", "自动采集任务已停止");
            }, token);
        }

        /// <summary>
        /// 停止自动采集
        /// </summary>
        public void StopAutoCollection()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _loggingService.Info("数据采集", "正在停止自动采集任务...");
            }
        }

        /// <summary>
        /// 手动采集指定期号
        /// </summary>
        public async Task<DataCollectionTask> CollectDataAsync(int issueId, string dataSource)
        {
            _loggingService.Info("数据采集", $"开始采集期号: {issueId}");

            var task = new DataCollectionTask
            {
                IssueId = issueId,
                DataSource = dataSource,
                Status = CollectionTaskStatus.Collecting,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now
            };

            try
            {
                // TODO: 实现实际的HTTP请求和HTML解析逻辑
                // 1. 发起HTTP请求到数据源
                // 2. 解析HTML,提取开奖数据
                // 3. 正则匹配期号和号码
                
                await Task.Delay(1000); // 模拟网络请求

                // 模拟数据
                task.OpenData = "01,02,03,04,05";
                task.CollectionTime = DateTime.Now;
                task.Status = CollectionTaskStatus.Completed;
                task.UpdatedTime = DateTime.Now;

                _loggingService.Info("数据采集", $"期号 {issueId} 采集成功: {task.OpenData}");
            }
            catch (Exception ex)
            {
                task.Status = CollectionTaskStatus.Failed;
                task.ErrorMessage = ex.Message;
                task.UpdatedTime = DateTime.Now;
                _loggingService.Error("数据采集", $"期号 {issueId} 采集失败: {ex.Message}", ex);
            }

            return task;
        }

        /// <summary>
        /// 获取期号信息 (当期/下期)
        /// </summary>
        public async Task<(string currentIssue, string currentTime, string nextIssue, string nextTime)> GetIssueInfoAsync(string dataSource)
        {
            _loggingService.Info("数据采集", "获取期号信息...");

            try
            {
                // TODO: 实现实际的API调用逻辑
                await Task.Delay(500); // 模拟网络请求

                // 模拟数据
                var currentIssue = "113004873";
                var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var nextIssue = "113004874";
                var nextTime = DateTime.Now.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss");

                _loggingService.Info("数据采集", $"当期: {currentIssue}, 下期: {nextIssue}");
                return (currentIssue, currentTime, nextIssue, nextTime);
            }
            catch (Exception ex)
            {
                _loggingService.Error("数据采集", $"获取期号信息失败: {ex.Message}", ex);
                return (string.Empty, string.Empty, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// 投递数据到多个API
        /// </summary>
        public async Task<bool> SubmitDataAsync(DataCollectionTask task, string submitAddresses)
        {
            if (string.IsNullOrWhiteSpace(submitAddresses))
            {
                _loggingService.Warn("数据采集", "未配置提交地址");
                return false;
            }

            try
            {
                // 解析提交地址
                // 格式: [标识]URL,\r\n[标识]URL,\r\n...
                var addresses = submitAddresses.Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                _loggingService.Info("数据采集", $"开始投递数据,目标数量: {addresses.Length}");

                foreach (var address in addresses)
                {
                    var trimmed = address.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    // TODO: 实现实际的POST请求逻辑
                    // 1. 解析 [标识]URL 格式
                    // 2. 根据不同标识,构造不同的POST数据格式
                    // 3. 发送HTTP POST请求

                    _loggingService.Debug("数据采集", $"投递到: {trimmed}");
                    await Task.Delay(100); // 模拟POST请求
                }

                _loggingService.Info("数据采集", "数据投递完成");
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error("数据采集", $"数据投递失败: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 测试数据源连接
        /// </summary>
        public async Task<bool> TestConnectionAsync(string dataSource)
        {
            _loggingService.Info("数据采集", $"测试连接: {dataSource}");

            try
            {
                // TODO: 实现实际的连接测试逻辑
                await Task.Delay(500); // 模拟网络请求

                _loggingService.Info("数据采集", "连接测试成功");
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error("数据采集", $"连接测试失败: {ex.Message}", ex);
                return false;
            }
        }

        #region 脚本任务数据库操作

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                // 确保数据目录存在
                var dataDir = Path.GetDirectoryName(_dbPath);
                if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }

                _db = new SQLiteConnection(_dbPath);
                
                // 创建脚本任务表
                _db.CreateTable<ScriptTask>();
                
                _loggingService.Info("DataCollectionService", $"数据库初始化成功: {_dbPath}");
            }
            catch (Exception ex)
            {
                _loggingService.Error("DataCollectionService", $"数据库初始化失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 加载所有脚本任务
        /// </summary>
        public List<ScriptTask> LoadAllScriptTasks()
        {
            try
            {
                if (_db == null)
                {
                    _loggingService.Warn("DataCollectionService", "数据库未初始化");
                    return new List<ScriptTask>();
                }

                var tasks = _db.Table<ScriptTask>().ToList();
                _loggingService.Info("DataCollectionService", $"加载了 {tasks.Count} 个脚本任务");
                return tasks;
            }
            catch (Exception ex)
            {
                _loggingService.Error("DataCollectionService", $"加载脚本任务失败: {ex.Message}", ex);
                return new List<ScriptTask>();
            }
        }

        /// <summary>
        /// 保存脚本任务
        /// </summary>
        public bool SaveScriptTask(ScriptTask task)
        {
            try
            {
                if (_db == null)
                {
                    _loggingService.Warn("DataCollectionService", "数据库未初始化");
                    return false;
                }

                if (task.Id == 0)
                {
                    // 新增
                    task.CreatedTime = DateTime.Now;
                    _db.Insert(task);
                    _loggingService.Info("DataCollectionService", $"新增脚本任务: {task.Name} (ID: {task.Id})");
                }
                else
                {
                    // 更新
                    _db.Update(task);
                    _loggingService.Info("DataCollectionService", $"更新脚本任务: {task.Name} (ID: {task.Id})");
                }

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error("DataCollectionService", $"保存脚本任务失败: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 删除脚本任务
        /// </summary>
        public bool DeleteScriptTask(int taskId)
        {
            try
            {
                if (_db == null)
                {
                    _loggingService.Warn("DataCollectionService", "数据库未初始化");
                    return false;
                }

                _db.Delete<ScriptTask>(taskId);
                _loggingService.Info("DataCollectionService", $"删除脚本任务 ID: {taskId}");
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error("DataCollectionService", $"删除脚本任务失败: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 根据ID获取脚本任务
        /// </summary>
        public ScriptTask? GetScriptTask(int taskId)
        {
            try
            {
                if (_db == null)
                {
                    _loggingService.Warn("DataCollectionService", "数据库未初始化");
                    return null;
                }

                return _db.Find<ScriptTask>(taskId);
            }
            catch (Exception ex)
            {
                _loggingService.Error("DataCollectionService", $"获取脚本任务失败: {ex.Message}", ex);
                return null;
            }
        }

        #endregion
    }
}

