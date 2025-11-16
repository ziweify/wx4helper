using BaiShengVx3Plus.Contracts;
using Sunny.UI;
using LogLevel = BaiShengVx3Plus.Models.LogLevel;  // 明确使用我们的 LogLevel
using LogEntry = BaiShengVx3Plus.Models.LogEntry;

namespace BaiShengVx3Plus.Views
{
    /// <summary>
    /// 日志查看窗口
    /// 实时显示系统日志
    /// </summary>
    public partial class LogViewerForm : UIForm
    {
        private readonly ILogService _logService;
        private System.Windows.Forms.Timer? _refreshTimer;
        private System.Windows.Forms.Timer? _batchUpdateTimer;
        
        // 🔥 智能滚动控制：用户是否在底部查看
        private bool _isUserScrolledToBottom = true;
        
        // 🔥 批量更新缓冲区：防止大量日志卡死UI
        private readonly Queue<LogEntry> _pendingLogs = new Queue<LogEntry>();
        private readonly object _pendingLogsLock = new object();
        private const int MAX_BATCH_SIZE = 50; // 每次批量处理最多50条

        public LogViewerForm(ILogService logService)
        {
            _logService = logService;
            InitializeComponent();
            
            // 订阅实时日志事件
            _logService.LogAdded += OnLogAdded;
            
            // 🔥 订阅滚动事件，检测用户是否手动滚动
            dgvLogs.Scroll += DgvLogs_Scroll;
            
            // 🔥 启动批量更新定时器（每100ms批量处理日志，防止UI卡顿）
            _batchUpdateTimer = new System.Windows.Forms.Timer();
            _batchUpdateTimer.Interval = 100; // 100ms
            _batchUpdateTimer.Tick += BatchUpdateTimer_Tick;
            _batchUpdateTimer.Start();
            
            // 加载历史日志
            LoadRecentLogs();
            
            // 启动定时刷新（备用方案）
            StartRefreshTimer();
        }
        
        /// <summary>
        /// 🔥 检测用户是否滚动到底部（智能滚动核心）
        /// </summary>
        private void DgvLogs_Scroll(object? sender, ScrollEventArgs e)
        {
            if (dgvLogs.Rows.Count == 0)
            {
                _isUserScrolledToBottom = true;
                return;
            }
            
            try
            {
                // 🔥 检测是否接近底部（最后3行内）
                int lastVisibleRow = dgvLogs.FirstDisplayedScrollingRowIndex + dgvLogs.DisplayedRowCount(false) - 1;
                int totalRows = dgvLogs.Rows.Count;
                
                // 如果用户在最后3行内，认为在底部
                _isUserScrolledToBottom = (totalRows - lastVisibleRow) <= 3;
                
                // 调试日志
                Console.WriteLine($"滚动检测: lastRow={lastVisibleRow}, totalRows={totalRows}, isBottom={_isUserScrolledToBottom}");
            }
            catch
            {
                _isUserScrolledToBottom = true;
            }
        }

        private void OnLogAdded(object? sender, LogEntry entry)
        {
            // 🔥 不直接更新UI，而是放入缓冲队列，由定时器批量处理
            lock (_pendingLogsLock)
            {
                _pendingLogs.Enqueue(entry);
            }
        }
        
        /// <summary>
        /// 🔥 批量更新定时器：每100ms批量处理日志，防止UI卡顿
        /// </summary>
        private void BatchUpdateTimer_Tick(object? sender, EventArgs e)
        {
            List<LogEntry> logsToAdd;
            
            // 从缓冲队列中取出日志
            lock (_pendingLogsLock)
            {
                if (_pendingLogs.Count == 0)
                    return;
                
                // 每次最多处理 MAX_BATCH_SIZE 条
                int count = Math.Min(_pendingLogs.Count, MAX_BATCH_SIZE);
                logsToAdd = new List<LogEntry>(count);
                
                for (int i = 0; i < count; i++)
                {
                    logsToAdd.Add(_pendingLogs.Dequeue());
                }
            }
            
            // 批量添加到界面
            if (logsToAdd.Count > 0)
            {
                AddLogsToGridBatch(logsToAdd);
            }
        }
        
        /// <summary>
        /// 🔥 批量添加日志到表格（高性能版本）
        /// </summary>
        private void AddLogsToGridBatch(List<LogEntry> entries)
        {
            if (entries.Count == 0) return;
            
            try
            {
                // 🔥 暂停绘制，提升性能
                dgvLogs.SuspendLayout();
                
                foreach (var entry in entries)
                {
                    var index = dgvLogs.Rows.Add(
                        entry.FormattedTime,
                        entry.LevelName,
                        entry.Source,
                        entry.Message,
                        entry.ThreadId
                    );

                    // 根据级别设置行颜色
                    var row = dgvLogs.Rows[index];
                    switch (entry.Level)
                    {
                        case LogLevel.Error:
                        case LogLevel.Fatal:
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                            row.DefaultCellStyle.ForeColor = Color.DarkRed;
                            break;
                        case LogLevel.Warning:
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 230);
                            row.DefaultCellStyle.ForeColor = Color.DarkOrange;
                            break;
                        case LogLevel.Debug:
                            row.DefaultCellStyle.ForeColor = Color.Gray;
                            break;
                    }
                }
                
                // 限制显示行数（保留最新3000条，删除顶部旧数据）
                while (dgvLogs.Rows.Count > 3000)
                {
                    dgvLogs.Rows.RemoveAt(0);  // 删除最旧的（顶部）
                }

                // 🔥 恢复绘制
                dgvLogs.ResumeLayout();
                
                // 🔥 智能滚动：只有当用户在底部时才自动滚动到底部
                if (chkAutoScroll.Checked && _isUserScrolledToBottom && dgvLogs.Rows.Count > 0)
                {
                    try
                    {
                        dgvLogs.FirstDisplayedScrollingRowIndex = dgvLogs.Rows.Count - 1;
                    }
                    catch
                    {
                        // 忽略滚动错误
                    }
                }

                // 🔥 性能优化：移除这里的UpdateStatistics()调用
                // 改为在批量更新完成后或定时器中更新，避免频繁查询数据库（100万条记录）
                // UpdateStatistics(); // ❌ 每添加一条日志就查询一次数据库，太频繁！
            }
            catch
            {
                // 忽略UI更新错误
                dgvLogs.ResumeLayout();
            }
        }

        private async void LoadRecentLogs()
        {
            try
            {
                // 🔥 异步加载，不阻塞UI线程
                var logs = await Task.Run(() => _logService.GetRecentLogs(100));
                
                // 🔥 暂停绘制，提升性能
                dgvLogs.SuspendLayout();
                dgvLogs.Rows.Clear();
                
                // 🔥 批量添加，而不是逐条添加
                var rows = new List<object[]>();
                foreach (var log in logs)
                {
                    rows.Add(new object[]
                    {
                        log.FormattedTime,
                        log.LevelName,
                        log.Source,
                        log.Message,
                        log.ThreadId
                    });
                }
                
                // 一次性添加所有行
                foreach (var rowData in rows)
                {
                    var index = dgvLogs.Rows.Add(rowData);
                    
                    // 根据级别设置行颜色
                    var log = logs[index];
                    var row = dgvLogs.Rows[index];
                    switch (log.Level)
                    {
                        case LogLevel.Error:
                        case LogLevel.Fatal:
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                            row.DefaultCellStyle.ForeColor = Color.DarkRed;
                            break;
                        case LogLevel.Warning:
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 230);
                            row.DefaultCellStyle.ForeColor = Color.DarkOrange;
                            break;
                        case LogLevel.Debug:
                            row.DefaultCellStyle.ForeColor = Color.Gray;
                            break;
                    }
                }
                
                // 滚动到底部
                if (dgvLogs.Rows.Count > 0)
                {
                    dgvLogs.FirstDisplayedScrollingRowIndex = dgvLogs.Rows.Count - 1;
                }
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError($"加载日志失败: {ex.Message}");
            }
            finally
            {
                // 🔥 恢复绘制
                dgvLogs.ResumeLayout();
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                var stats = _logService.GetStatistics();
                lblStatistics.Text = $"总计: {stats.TotalCount} | " +
                                   $"错误: {stats.ErrorCount} | " +
                                   $"警告: {stats.WarningCount} | " +
                                   $"信息: {stats.InfoCount} | " +
                                   $"显示: {dgvLogs.Rows.Count}";
            }
            catch
            {
                // 忽略统计更新错误
            }
        }

        private void StartRefreshTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000 // 🔥 性能优化：5秒刷新一次统计（避免频繁查询100万条日志）
            };
            _refreshTimer.Tick += (s, e) =>
            {
                // 🔥 只在定时器中更新统计，降低查询频率
                UpdateStatistics();
            };
            _refreshTimer.Start();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadRecentLogs();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (UIMessageBox.ShowAsk("确定要清空内存中的日志吗？"))
            {
                dgvLogs.Rows.Clear();
                _logService.ClearMemoryLogs();
                UpdateStatistics();
            }
        }

        private void btnClearDatabase_Click(object sender, EventArgs e)
        {
            if (UIMessageBox.ShowAsk("确定要清空数据库中的所有日志吗？此操作不可恢复！"))
            {
                _logService.ClearDatabaseLogs();
                LoadRecentLogs();
                UIMessageBox.ShowSuccess("数据库日志已清空");
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "日志文件 (*.log)|*.log|文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.log"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _logService.ExportToFileAsync(dialog.FileName).Wait();
                    UIMessageBox.ShowSuccess($"日志已导出到：{dialog.FileName}");
                }
                catch (Exception ex)
                {
                    UIMessageBox.ShowError($"导出失败: {ex.Message}");
                }
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                var keyword = txtKeyword.Text.Trim();
                var source = cmbSource.Text;
                
                // 转换选择的级别（跳过"全部"选项）
                LogLevel? minLevel = cmbLevel.SelectedIndex > 0 
                    ? (LogLevel)(cmbLevel.SelectedIndex - 1) 
                    : null;

                if (string.IsNullOrEmpty(keyword) && string.IsNullOrEmpty(source) && !minLevel.HasValue)
                {
                    LoadRecentLogs();
                    return;
                }

                dgvLogs.Rows.Clear();
                var logs = _logService.QueryLogs(
                    keyword: string.IsNullOrEmpty(keyword) ? null : keyword,
                    source: string.IsNullOrEmpty(source) ? null : source,
                    minLevel: minLevel
                );

                // 🔥 使用批量添加提升性能
                AddLogsToGridBatch(logs);
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError($"查询失败: {ex.Message}");
            }
        }

        private void cmbLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 自动查询
            if (cmbLevel.SelectedIndex >= 0)
            {
                btnQuery_Click(sender, e);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            // 取消订阅事件
            _logService.LogAdded -= OnLogAdded;
            
            // 🔥 停止所有定时器，防止内存泄漏
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            _batchUpdateTimer?.Stop();
            _batchUpdateTimer?.Dispose();
        }
    }
}

