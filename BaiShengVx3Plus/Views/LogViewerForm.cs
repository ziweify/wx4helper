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
        
        // 🔥 智能滚动控制：用户是否在底部查看
        private bool _isUserScrolledToBottom = true;

        public LogViewerForm(ILogService logService)
        {
            _logService = logService;
            InitializeComponent();
            
            // 订阅实时日志事件
            _logService.LogAdded += OnLogAdded;
            
            // 🔥 订阅滚动事件，检测用户是否手动滚动
            dgvLogs.Scroll += DgvLogs_Scroll;
            
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
                // 🔥 保存当前滚动位置（用于判断是否在底部）
                int lastVisibleRow = -1;
                if (dgvLogs.Rows.Count > 0 && dgvLogs.DisplayedRowCount(false) > 0)
                {
                    lastVisibleRow = dgvLogs.FirstDisplayedScrollingRowIndex + dgvLogs.DisplayedRowCount(false) - 1;
                }
                
                // 添加到表格底部（最新的在下面，更符合日志习惯）
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

                // 限制显示行数（保留最新1000条，删除顶部旧数据）
                if (dgvLogs.Rows.Count > 1000)
                {
                    dgvLogs.Rows.RemoveAt(0);  // 删除最旧的（顶部）
                }

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
                // 🔥 暂停绘制，提升性能
                dgvLogs.SuspendLayout();
                dgvLogs.Rows.Clear();
                
                // 🔥 只加载最近100条，避免卡顿
                var logs = _logService.GetRecentLogs(100);
                
                foreach (var log in logs)
                {
                    AddLogToGrid(log);
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

