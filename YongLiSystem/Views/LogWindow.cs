using System;
using System.Collections.Generic;
using System.ComponentModel; // BindingList
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using YongLiSystem.Models;
using YongLiSystem.Services;

namespace YongLiSystem.Views
{
    /// <summary>
    /// 日志窗口 - 使用 UserControl 实现
    /// </summary>
    public partial class LogWindow : UserControl
    {
        private readonly LoggingService _loggingService;
        private readonly BindingList<LogEntry> _logEntries = new BindingList<LogEntry>(); // 改用 BindingList
        private readonly object _logEntriesLock = new object();
        
        // 过滤条件
        private string _selectedModule = "全部";
        private LogLevel _selectedLevel = LogLevel.Debug;
        private bool _isPaused = false;
        private bool _isHistoryLoaded = false; // 标记是否已加载历史日志
        
        // 分离状态
        private bool _isDetached = false; // 是否已分离
        public event EventHandler? DetachRequested; // 分离请求事件
        public event EventHandler? AttachRequested; // 附加请求事件
        
        // UI 控件
        private GridControl? _gridControl;
        private GridView? _gridView;
        private SimpleButton? _btnClear;
        private SimpleButton? _btnPause;
        private SimpleButton? _btnExport;
        private SimpleButton? _btnDetach; // 分离/附加按钮
        private ComboBoxEdit? _cmbModule;
        private ComboBoxEdit? _cmbLevel;
        private TextEdit? _txtSearch;

        public LogWindow()
        {
            _loggingService = LoggingService.Instance;
            InitializeComponent();
            InitializeUI();
            SubscribeToLogEvents();
            
            System.Diagnostics.Debug.WriteLine("LogWindow 构造函数完成");
            
            // 在窗口首次显示时加载历史日志
            VisibleChanged += LogWindow_VisibleChanged;
            
            // 订阅 HandleCreated 事件，确保在控件重新创建后重新订阅日志事件
            HandleCreated += LogWindow_HandleCreated;
            
            // 立即尝试加载历史日志（如果控件已创建）
            if (IsHandleCreated || Visible)
            {
                System.Diagnostics.Debug.WriteLine("LogWindow 构造函数：立即加载历史日志");
                _isHistoryLoaded = true;
                LoadHistoricalLogs();
            }
        }
        
        private void LogWindow_HandleCreated(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("LogWindow_HandleCreated: 控件句柄已创建，重新订阅日志事件");
            // 确保日志事件已订阅
            SubscribeToLogEvents();
        }
        
        private void LogWindow_VisibleChanged(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"LogWindow_VisibleChanged: Visible={Visible}, _isHistoryLoaded={_isHistoryLoaded}");
            
            // 确保日志事件已订阅（防止在移动控件时丢失订阅）
            SubscribeToLogEvents();
            
            if (Visible && !_isHistoryLoaded)
            {
                _isHistoryLoaded = true;
                System.Diagnostics.Debug.WriteLine("LogWindow_VisibleChanged: 开始加载历史日志");
                LoadHistoricalLogs();
            }
            else if (Visible)
            {
                // 如果已加载过历史日志，只需刷新显示
                System.Diagnostics.Debug.WriteLine("LogWindow_VisibleChanged: 控件变为可见，刷新显示");
                RefreshDisplay();
            }
        }

        private void InitializeComponent()
        {
            // 设置 UserControl 属性
            Name = "LogWindow";
            Dock = DockStyle.Bottom;
            Height = 250;
        }

        private void InitializeUI()
        {
            // 创建容器面板
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            // 创建工具栏
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35
            };

            // 创建模块过滤下拉框
            _cmbModule = new ComboBoxEdit
            {
                Location = new Point(5, 5),
                Width = 120
            };
            _cmbModule.Properties.Items.AddRange(new[] { "全部", "系统", "主页", "微信助手", "开奖管理", "方案管理", "系统设置" });
            _cmbModule.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _cmbModule.EditValue = "全部"; // 使用 EditValue 而不是 Text
            _cmbModule.SelectedIndex = 0; // 确保选中第一项
            _cmbModule.SelectedIndexChanged += CmbModule_SelectedIndexChanged; // 订阅事件
            toolbar.Controls.Add(_cmbModule);

            // 创建级别过滤下拉框
            _cmbLevel = new ComboBoxEdit
            {
                Location = new Point(130, 5),
                Width = 100
            };
            _cmbLevel.Properties.Items.AddRange(new[] { "全部", "DEBUG", "INFO", "WARN", "ERROR" });
            _cmbLevel.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _cmbLevel.EditValue = "全部"; // 使用 EditValue 而不是 Text
            _cmbLevel.SelectedIndex = 0; // 确保选中第一项
            _cmbLevel.SelectedIndexChanged += CmbLevel_SelectedIndexChanged; // 订阅事件
            toolbar.Controls.Add(_cmbLevel);

            // 创建搜索框
            _txtSearch = new TextEdit
            {
                Location = new Point(235, 5),
                Width = 200
            };
            _txtSearch.Properties.NullText = "搜索日志...";
            _txtSearch.KeyDown += TxtSearch_KeyDown;
            toolbar.Controls.Add(_txtSearch);

            // 创建清空按钮
            _btnClear = new SimpleButton
            {
                Location = new Point(440, 5),
                Width = 60,
                Text = "清空"
            };
            _btnClear.Click += BtnClear_Click;
            toolbar.Controls.Add(_btnClear);

            // 创建暂停按钮
            _btnPause = new SimpleButton
            {
                Location = new Point(505, 5),
                Width = 60,
                Text = "暂停"
            };
            _btnPause.Click += BtnPause_Click;
            toolbar.Controls.Add(_btnPause);

            // 创建导出按钮
            _btnExport = new SimpleButton
            {
                Location = new Point(570, 5),
                Width = 60,
                Text = "导出"
            };
            _btnExport.Click += BtnExport_Click;
            toolbar.Controls.Add(_btnExport);

            // 创建分离/附加按钮
            _btnDetach = new SimpleButton
            {
                Location = new Point(635, 5),
                Width = 80,
                Text = "🔓 分离"
            };
            _btnDetach.Click += BtnDetach_Click;
            toolbar.Controls.Add(_btnDetach);

            // 创建 GridControl
            _gridControl = new GridControl
            {
                Dock = DockStyle.Fill
            };

            _gridView = new GridView(_gridControl);
            _gridControl.MainView = _gridView;

            // 配置列 - 使用 FieldName 而不是 AddField
            var colTimestamp = _gridView.Columns.Add();
            colTimestamp.FieldName = "Timestamp";
            colTimestamp.Caption = "时间";
            colTimestamp.Width = 120;
            colTimestamp.Visible = true;
            colTimestamp.OptionsColumn.AllowEdit = false;
            
            // 不使用 DisplayFormat，而是使用 CustomColumnDisplayText 事件来格式化时间

            var colModule = _gridView.Columns.Add();
            colModule.FieldName = "Module";
            colModule.Caption = "模块";
            colModule.Width = 100;
            colModule.Visible = true;

            var colLevel = _gridView.Columns.Add();
            colLevel.FieldName = "Level";
            colLevel.Caption = "级别";
            colLevel.Width = 80;
            colLevel.Visible = true;

            var colMessage = _gridView.Columns.Add();
            colMessage.FieldName = "Message";
            colMessage.Caption = "消息";
            colMessage.Width = 500;
            colMessage.Visible = true;

            // 设置GridView选项
            _gridView.OptionsView.RowAutoHeight = true;
            _gridView.OptionsView.ShowGroupPanel = false;
            _gridView.OptionsView.ShowIndicator = true;
            _gridView.OptionsView.ShowColumnHeaders = true;
            _gridView.OptionsBehavior.Editable = false; // 只读
            _gridView.OptionsBehavior.ReadOnly = true;

            // 添加自定义列显示文本事件（用于格式化时间）
            _gridView.CustomColumnDisplayText += GridView_CustomColumnDisplayText;

            // 🔥 初始化时设置空的数据源（确保 GridControl 可以正常绑定）
            _gridControl.DataSource = new List<LogEntry>();

            // 添加行样式
            _gridView.RowStyle += GridView_RowStyle;

            // 添加到面板
            panel.Controls.Add(_gridControl);
            panel.Controls.Add(toolbar);

            // 添加到 DockPanel
            Controls.Add(panel);
        }

        /// <summary>
        /// 自定义列显示文本（用于格式化时间）
        /// </summary>
        private void GridView_CustomColumnDisplayText(object? sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column?.FieldName == "Timestamp" && e.Value is DateTime dt)
            {
                e.DisplayText = dt.ToString("HH:mm:ss.fff");
            }
        }

        private void GridView_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (e.RowHandle < 0) return;

            var view = sender as GridView;
            if (view == null) return;

            var entry = view.GetRow(e.RowHandle) as LogEntry;
            if (entry == null) return;

            // 根据日志级别设置颜色
            switch (entry.Level)
            {
                case LogLevel.Error:
                    e.Appearance.BackColor = Color.LightCoral;
                    break;
                case LogLevel.Warn:
                    e.Appearance.BackColor = Color.LightYellow;
                    break;
                case LogLevel.Info:
                    e.Appearance.BackColor = Color.LightBlue;
                    break;
                case LogLevel.Debug:
                    e.Appearance.BackColor = Color.White;
                    break;
            }
        }

        private void SubscribeToLogEvents()
        {
            System.Diagnostics.Debug.WriteLine("SubscribeToLogEvents: 订阅日志事件");
            // 先取消订阅，避免重复订阅
            _loggingService.LogReceived -= OnLogReceived;
            _loggingService.LogReceived += OnLogReceived;
        }
        
        /// <summary>
        /// 加载历史日志
        /// </summary>
        private void LoadHistoricalLogs()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"========== LoadHistoricalLogs 开始 (调用堆栈跟踪) ==========");
                System.Diagnostics.Debug.WriteLine(new System.Diagnostics.StackTrace().ToString());
                
                // **临时方案：不加载历史文件，只显示当前会话的日志**
                // 1. 先从内存加载（包含当前会话的所有日志）
                var memoryHistory = _loggingService.GetMemoryHistory();
                System.Diagnostics.Debug.WriteLine($"从内存加载历史日志: 找到 {memoryHistory.Count} 条日志");
                
                // 输出前几条日志内容
                for (int i = 0; i < Math.Min(5, memoryHistory.Count); i++)
                {
                    System.Diagnostics.Debug.WriteLine($"  [{i}] {memoryHistory[i].Timestamp:HH:mm:ss.fff} [{memoryHistory[i].Module}] {memoryHistory[i].Message}");
                }
                
                // **暂时禁用文件加载，避免重复日志问题**
                /*
                // 2. 再从文件加载（包含之前保存的日志）
                var fileHistory = _loggingService.LoadHistory(DateTime.Now);
                System.Diagnostics.Debug.WriteLine($"从文件加载历史日志: 找到 {fileHistory.Count} 条日志");
                
                // 3. 合并日志（去重，按时间排序）
                var allHistory = new List<LogEntry>();
                allHistory.AddRange(memoryHistory);
                
                // 添加文件中的日志（排除内存中已存在的）
                var memoryTimestamps = new HashSet<string>(memoryHistory.Select(e => $"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff}"));
                foreach (var entry in fileHistory)
                {
                    var key = $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}";
                    if (!memoryTimestamps.Contains(key))
                    {
                        allHistory.Add(entry);
                    }
                }
                
                // 按时间排序
                allHistory = allHistory.OrderBy(e => e.Timestamp).ToList();
                */
                
                var allHistory = memoryHistory; // 只使用内存历史
                
                System.Diagnostics.Debug.WriteLine($"合并后共 {allHistory.Count} 条日志");
                
                lock (_logEntriesLock)
                {
                    System.Diagnostics.Debug.WriteLine($"清空前 _logEntries.Count = {_logEntries.Count}");
                    _logEntries.Clear();
                    foreach (var entry in allHistory)
                    {
                        _logEntries.Add(entry);
                    }
                    System.Diagnostics.Debug.WriteLine($"日志列表已更新: {_logEntries.Count} 条");
                }
                
                // 刷新显示（在UI线程中）
                if (InvokeRequired)
                {
                    System.Diagnostics.Debug.WriteLine("在UI线程中刷新显示...");
                    BeginInvoke(new Action(() => 
                    {
                        RefreshDisplay();
                        System.Diagnostics.Debug.WriteLine($"刷新显示完成，GridControl 行数: {_gridView?.RowCount ?? 0}");
                        System.Diagnostics.Debug.WriteLine("========== LoadHistoricalLogs 结束 ==========");
                    }));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("直接刷新显示...");
                    RefreshDisplay();
                    System.Diagnostics.Debug.WriteLine($"刷新显示完成，GridControl 行数: {_gridView?.RowCount ?? 0}");
                    System.Diagnostics.Debug.WriteLine("========== LoadHistoricalLogs 结束 ==========");
                }
            }
            catch (Exception ex)
            {
                // 加载历史日志失败，不影响新日志的显示
                System.Diagnostics.Debug.WriteLine($"加载历史日志失败: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"异常堆栈: {ex.StackTrace}");
                MessageBox.Show($"加载历史日志失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnLogReceived(object? sender, LogEventArgs e)
        {
            if (_isPaused)
            {
                System.Diagnostics.Debug.WriteLine("OnLogReceived: 已暂停，忽略日志");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"OnLogReceived: 收到日志 [{e.LogEntry.Module}] {e.LogEntry.Message}");

            // 在主线程中更新UI
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddLogEntry(e.LogEntry)));
            }
            else
            {
                AddLogEntry(e.LogEntry);
            }
        }

        private void AddLogEntry(LogEntry entry)
        {
            System.Diagnostics.Debug.WriteLine($"AddLogEntry: 添加日志 [{entry.Module}] {entry.Message}");
            
            lock (_logEntriesLock)
            {
                _logEntries.Add(entry);
                
                // 限制日志数量（最多保留10000条）
                if (_logEntries.Count > 10000)
                {
                    _logEntries.RemoveAt(0);
                }
                
                System.Diagnostics.Debug.WriteLine($"AddLogEntry: 当前日志数量 = {_logEntries.Count}");
            }

            // 刷新显示
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            System.Diagnostics.Debug.WriteLine($"========== RefreshDisplay 开始 ==========");
            System.Diagnostics.Debug.WriteLine($"GridControl: {(_gridControl != null ? "存在" : "null")}");
            System.Diagnostics.Debug.WriteLine($"GridView: {(_gridView != null ? "存在" : "null")}");
            
            if (_gridControl == null || _gridView == null)
            {
                System.Diagnostics.Debug.WriteLine("RefreshDisplay: GridControl 或 GridView 为 null");
                MessageBox.Show("GridControl 或 GridView 为 null，请检查初始化", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 应用过滤
            var filtered = ApplyFilters();
            System.Diagnostics.Debug.WriteLine($"RefreshDisplay: 过滤后 {filtered.Count} 条日志");
            
            // 输出前几条过滤后的日志
            for (int i = 0; i < Math.Min(3, filtered.Count); i++)
            {
                System.Diagnostics.Debug.WriteLine($"  [{i}] {filtered[i].Timestamp:HH:mm:ss} [{filtered[i].Module}] {filtered[i].Message}");
            }

            // 更新数据源
            try
            {
                _gridControl.BeginUpdate();
                _gridControl.DataSource = null;
                _gridControl.DataSource = filtered;
                _gridControl.RefreshDataSource();
                _gridView.RefreshData();
                _gridControl.EndUpdate();
                
                System.Diagnostics.Debug.WriteLine($"RefreshDisplay: GridView 行数 = {_gridView.RowCount}");
                System.Diagnostics.Debug.WriteLine($"RefreshDisplay: GridControl.DataSource 类型 = {_gridControl.DataSource?.GetType().Name ?? "null"}");
                System.Diagnostics.Debug.WriteLine($"RefreshDisplay: GridView.Columns.Count = {_gridView.Columns.Count}");
                
                // 输出列信息
                foreach (DevExpress.XtraGrid.Columns.GridColumn col in _gridView.Columns)
                {
                    System.Diagnostics.Debug.WriteLine($"  列: {col.FieldName} ({col.Caption}) Visible={col.Visible}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshDisplay 异常: {ex.Message}");
                MessageBox.Show($"刷新显示失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 自动滚动到底部
            if (_gridView.RowCount > 0)
            {
                _gridView.MakeRowVisible(_gridView.RowCount - 1);
                _gridView.FocusedRowHandle = _gridView.RowCount - 1;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("警告: GridView.RowCount = 0");
            }
            
            System.Diagnostics.Debug.WriteLine($"========== RefreshDisplay 结束 ==========");
        }

        private List<LogEntry> ApplyFilters()
        {
            List<LogEntry> entries;
            lock (_logEntriesLock)
            {
                entries = _logEntries.ToList();
            }
            
            System.Diagnostics.Debug.WriteLine($"========== ApplyFilters 开始 ==========");
            System.Diagnostics.Debug.WriteLine($"原始日志: {entries.Count} 条");
            System.Diagnostics.Debug.WriteLine($"_selectedModule = '{_selectedModule}'");
            
            var filtered = entries.AsEnumerable();

            // 模块过滤
            if (!string.IsNullOrEmpty(_selectedModule) && _selectedModule != "全部")
            {
                System.Diagnostics.Debug.WriteLine($"应用模块过滤: '{_selectedModule}'");
                var beforeCount = filtered.Count();
                filtered = filtered.Where(e => e.Module == _selectedModule);
                System.Diagnostics.Debug.WriteLine($"模块过滤: {beforeCount} -> {filtered.Count()} 条");
            }

            // 级别过滤
            var levelText = _cmbLevel?.EditValue?.ToString() ?? "全部";
            if (levelText != "全部" && !string.IsNullOrEmpty(levelText))
            {
                if (Enum.TryParse<LogLevel>(levelText, true, out var level))
                {
                    System.Diagnostics.Debug.WriteLine($"应用级别过滤: {level}");
                    var beforeCount = filtered.Count();
                    filtered = filtered.Where(e => e.Level == level);
                    System.Diagnostics.Debug.WriteLine($"级别过滤: {beforeCount} -> {filtered.Count()} 条");
                }
            }

            // 搜索过滤
            // 🔥 使用 EditValue 而不是 Text，避免 NullText 干扰
            var searchText = _txtSearch?.EditValue?.ToString();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                System.Diagnostics.Debug.WriteLine($"应用搜索过滤: '{searchText}'");
                var beforeCount = filtered.Count();
                filtered = filtered.Where(e => 
                    e.Message.ToLower().Contains(searchText) ||
                    e.Module.ToLower().Contains(searchText));
                System.Diagnostics.Debug.WriteLine($"搜索过滤: {beforeCount} -> {filtered.Count()} 条");
            }

            var result = filtered.ToList();
            System.Diagnostics.Debug.WriteLine($"最终返回: {result.Count} 条");
            System.Diagnostics.Debug.WriteLine($"========== ApplyFilters 结束 ==========");
            
            return result;
        }

        /// <summary>
        /// 设置模块过滤（公开方法，供外部调用）
        /// </summary>
        public void FilterByModule(string module)
        {
            if (_cmbModule != null)
            {
                _selectedModule = module;
                _cmbModule.EditValue = module;
                
                // 如果模块不在列表中，添加它
                if (!_cmbModule.Properties.Items.Contains(module))
                {
                    _cmbModule.Properties.Items.Add(module);
                }
                
                RefreshDisplay();
            }
        }

        /// <summary>
        /// 强制刷新显示（公开方法，供外部调用）
        /// </summary>
        public void ForceRefresh()
        {
            System.Diagnostics.Debug.WriteLine("========== ForceRefresh 开始 ==========");
            System.Diagnostics.Debug.WriteLine($"ForceRefresh: _logEntries.Count = {_logEntries.Count}");
            System.Diagnostics.Debug.WriteLine($"ForceRefresh: IsHandleCreated = {IsHandleCreated}");
            System.Diagnostics.Debug.WriteLine($"ForceRefresh: Visible = {Visible}");
            System.Diagnostics.Debug.WriteLine($"ForceRefresh: InvokeRequired = {InvokeRequired}");
            
            // 重新订阅事件（确保不会丢失）
            SubscribeToLogEvents();
            
            // 如果需要跨线程调用
            if (InvokeRequired)
            {
                System.Diagnostics.Debug.WriteLine("ForceRefresh: 使用 Invoke 调用刷新");
                BeginInvoke(new Action(() => 
                {
                    RefreshDisplay();
                    System.Diagnostics.Debug.WriteLine($"ForceRefresh 完成: GridView 行数 = {_gridView?.RowCount ?? 0}");
                }));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ForceRefresh: 直接调用刷新");
                RefreshDisplay();
                System.Diagnostics.Debug.WriteLine($"ForceRefresh 完成: GridView 行数 = {_gridView?.RowCount ?? 0}");
            }
            
            System.Diagnostics.Debug.WriteLine("========== ForceRefresh 结束 ==========");
        }

        private void CmbModule_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var newModule = _cmbModule?.EditValue?.ToString() ?? "全部";
            System.Diagnostics.Debug.WriteLine($"CmbModule_SelectedIndexChanged: {_selectedModule} -> {newModule}");
            _selectedModule = newModule;
            RefreshDisplay();
        }

        private void CmbLevel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var levelText = _cmbLevel?.EditValue?.ToString() ?? "全部";
            System.Diagnostics.Debug.WriteLine($"CmbLevel_SelectedIndexChanged: {levelText}");
            
            if (levelText != "全部" && Enum.TryParse<LogLevel>(levelText, true, out var level))
            {
                _selectedLevel = level;
            }
            RefreshDisplay();
        }

        private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e != null && e.KeyCode == Keys.Enter)
            {
                RefreshDisplay();
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            lock (_logEntriesLock)
            {
                _logEntries.Clear();
            }
            RefreshDisplay();
        }

        private void BtnPause_Click(object? sender, EventArgs e)
        {
            _isPaused = !_isPaused;
            _btnPause!.Text = _isPaused ? "继续" : "暂停";
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "文本文件|*.txt|所有文件|*.*";
                dialog.FileName = $"日志_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var logs = ApplyFilters();
                        var content = string.Join(Environment.NewLine, logs.Select(l => l?.ToString() ?? ""));
                        System.IO.File.WriteAllText(dialog.FileName, content);
                        MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDetach_Click(object? sender, EventArgs e)
        {
            if (_isDetached)
            {
                // 请求附加回主窗口
                AttachRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // 请求分离到独立窗口
                DetachRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 设置分离状态（由主窗口调用）
        /// </summary>
        public void SetDetachedState(bool isDetached)
        {
            _isDetached = isDetached;
            if (_btnDetach != null)
            {
                _btnDetach.Text = isDetached ? "🔒 附加" : "🔓 分离";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _loggingService.LogReceived -= OnLogReceived;
            }
            base.Dispose(disposing);
        }
    }
}

