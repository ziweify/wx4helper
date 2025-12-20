using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using 永利系统.Models;
using 永利系统.Services;

namespace 永利系统.Views
{
    /// <summary>
    /// 日志窗口 - 可停靠窗口
    /// </summary>
    public partial class LogWindow : DockPanel
    {
        private readonly LoggingService _loggingService;
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();
        private readonly object _logEntriesLock = new object();
        
        // 过滤条件
        private string _selectedModule = "全部";
        private LogLevel _selectedLevel = LogLevel.Debug;
        private bool _isPaused = false;
        
        // UI 控件
        private GridControl? _gridControl;
        private GridView? _gridView;
        private SimpleButton? _btnClear;
        private SimpleButton? _btnPause;
        private SimpleButton? _btnExport;
        private ComboBoxEdit? _cmbModule;
        private ComboBoxEdit? _cmbLevel;
        private TextEdit? _txtSearch;

        public LogWindow(DockingManager dockingManager)
        {
            _loggingService = LoggingService.Instance;
            InitializeComponent(dockingManager);
            InitializeUI();
            SubscribeToLogEvents();
        }

        private void InitializeComponent(DockingManager dockingManager)
        {
            // 设置 DockPanel 属性
            Name = "LogWindow";
            Text = "日志输出";
            Dock = DockStyle.Bottom;
            Height = 250;
            Visible = false; // 默认隐藏
            
            // 设置停靠属性
            DockManager = dockingManager;
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
                Width = 120,
                Text = "全部"
            };
            _cmbModule.Properties.Items.AddRange(new[] { "全部", "主页", "微信助手", "开奖管理", "方案管理", "系统设置" });
            _cmbModule.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _cmbModule.SelectedIndexChanged += CmbModule_SelectedIndexChanged;
            toolbar.Controls.Add(_cmbModule);

            // 创建级别过滤下拉框
            _cmbLevel = new ComboBoxEdit
            {
                Location = new Point(130, 5),
                Width = 100,
                Text = "全部"
            };
            _cmbLevel.Properties.Items.AddRange(new[] { "全部", "DEBUG", "INFO", "WARN", "ERROR" });
            _cmbLevel.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _cmbLevel.SelectedIndexChanged += CmbLevel_SelectedIndexChanged;
            toolbar.Controls.Add(_cmbLevel);

            // 创建搜索框
            _txtSearch = new TextEdit
            {
                Location = new Point(235, 5),
                Width = 200,
                NullText = "搜索日志..."
            };
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

            // 创建 GridControl
            _gridControl = new GridControl
            {
                Dock = DockStyle.Fill
            };

            _gridView = new GridView(_gridControl);
            _gridControl.MainView = _gridView;

            // 配置列
            _gridView.Columns.AddField("Timestamp").Caption = "时间";
            _gridView.Columns["Timestamp"].Width = 150;
            _gridView.Columns["Timestamp"].DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss.fff";

            _gridView.Columns.AddField("Module").Caption = "模块";
            _gridView.Columns["Module"].Width = 100;

            _gridView.Columns.AddField("Level").Caption = "级别";
            _gridView.Columns["Level"].Width = 80;

            _gridView.Columns.AddField("Message").Caption = "消息";
            _gridView.Columns["Message"].Width = 500;

            // 设置行高
            _gridView.OptionsView.RowAutoHeight = true;
            _gridView.OptionsView.ShowGroupPanel = false;

            // 设置数据源
            _gridControl.DataSource = _logEntries;

            // 添加行样式
            _gridView.RowStyle += GridView_RowStyle;

            // 添加到面板
            panel.Controls.Add(_gridControl);
            panel.Controls.Add(toolbar);

            // 添加到 DockPanel
            Controls.Add(panel);
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
            _loggingService.LogReceived += OnLogReceived;
        }

        private void OnLogReceived(object? sender, LogEventArgs e)
        {
            if (_isPaused) return;

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
            lock (_logEntriesLock)
            {
                _logEntries.Add(entry);
                
                // 限制日志数量（最多保留10000条）
                if (_logEntries.Count > 10000)
                {
                    _logEntries.RemoveAt(0);
                }
            }

            // 刷新显示
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_gridControl == null || _gridView == null) return;

            // 应用过滤
            var filtered = ApplyFilters(_logEntries);

            // 更新数据源
            _gridControl.DataSource = filtered;
            _gridView.RefreshData();

            // 自动滚动到底部（如果用户在底部）
            if (_gridView.RowCount > 0)
            {
                _gridView.MakeRowVisible(_gridView.RowCount - 1);
                _gridView.FocusedRowHandle = _gridView.RowCount - 1;
            }
        }

        private List<LogEntry> ApplyFilters(List<LogEntry> entries)
        {
            var filtered = entries.AsEnumerable();

            // 模块过滤
            if (_selectedModule != "全部")
            {
                filtered = filtered.Where(e => e.Module == _selectedModule);
            }

            // 级别过滤
            if (_cmbLevel?.Text != "全部")
            {
                if (Enum.TryParse<LogLevel>(_cmbLevel?.Text, true, out var level))
                {
                    filtered = filtered.Where(e => e.Level == level);
                }
            }

            // 搜索过滤
            if (!string.IsNullOrWhiteSpace(_txtSearch?.Text))
            {
                var searchText = _txtSearch.Text.ToLower();
                filtered = filtered.Where(e => 
                    e.Message.ToLower().Contains(searchText) ||
                    e.Module.ToLower().Contains(searchText));
            }

            return filtered.ToList();
        }

        private void CmbModule_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _selectedModule = _cmbModule?.Text ?? "全部";
            RefreshDisplay();
        }

        private void CmbLevel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_cmbLevel?.Text != "全部" && Enum.TryParse<LogLevel>(_cmbLevel.Text, true, out var level))
            {
                _selectedLevel = level;
            }
            RefreshDisplay();
        }

        private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
                        var logs = ApplyFilters(_logEntries);
                        var content = string.Join(Environment.NewLine, logs.Select(l => l.ToString()));
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

