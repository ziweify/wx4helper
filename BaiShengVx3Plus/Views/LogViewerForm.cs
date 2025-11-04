using BaiShengVx3Plus.Services;
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

        public LogViewerForm(ILogService logService)
        {
            _logService = logService;
            InitializeComponent();
            
            // 订阅实时日志事件
            _logService.LogAdded += OnLogAdded;
            
            // 加载历史日志
            LoadRecentLogs();
            
            // 启动定时刷新（备用方案）
            StartRefreshTimer();
        }

        private void OnLogAdded(object? sender, LogEntry entry)
        {
            // 跨线程更新 UI
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddLogToGrid(entry)));
            }
            else
            {
                AddLogToGrid(entry);
            }
        }

        private void AddLogToGrid(LogEntry entry)
        {
            try
            {
                // 添加到表格顶部（最新的在上面）
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

                // 限制显示行数（保留最新1000条）
                if (dgvLogs.Rows.Count > 1000)
                {
                    dgvLogs.Rows.RemoveAt(dgvLogs.Rows.Count - 1);
                }

                // 自动滚动到顶部（如果启用）
                if (chkAutoScroll.Checked && dgvLogs.Rows.Count > 0)
                {
                    dgvLogs.FirstDisplayedScrollingRowIndex = 0;
                }

                // 更新统计
                UpdateStatistics();
            }
            catch
            {
                // 忽略UI更新错误
            }
        }

        private void LoadRecentLogs()
        {
            try
            {
                dgvLogs.Rows.Clear();
                var logs = _logService.GetRecentLogs(500);
                
                foreach (var log in logs)
                {
                    AddLogToGrid(log);
                }
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError($"加载日志失败: {ex.Message}");
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
                Interval = 5000 // 5秒刷新一次（备用）
            };
            _refreshTimer.Tick += (s, e) => UpdateStatistics();
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

                foreach (var log in logs)
                {
                    AddLogToGrid(log);
                }
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
            
            // 停止定时器
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }
    }
}

