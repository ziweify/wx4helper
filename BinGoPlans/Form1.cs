using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaiShengVx3Plus.Shared.Models.Games.Binggo;
using BaiShengVx3Plus.Shared.Models.Games.Binggo.Statistics;
using BaiShengVx3Plus.Shared.Services;
using BinGoPlans.Controls;
using BinGoPlans.Services;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using Statistics = BaiShengVx3Plus.Shared.Models.Games.Binggo.Statistics;
using GameSizeType = BaiShengVx3Plus.Shared.Models.Games.Binggo.SizeType;
using GameOddEvenType = BaiShengVx3Plus.Shared.Models.Games.Binggo.OddEvenType;
using GameTailSizeType = BaiShengVx3Plus.Shared.Models.Games.Binggo.TailSizeType;
using GameSumOddEvenType = BaiShengVx3Plus.Shared.Models.Games.Binggo.SumOddEvenType;
using GameDragonTigerType = BaiShengVx3Plus.Shared.Models.Games.Binggo.DragonTigerType;

namespace BinGoPlans
{
    public partial class Form1 : XtraForm
    {
        private BinggoStatisticsService _statisticsService;
        private DataService _dataService;
        private XtraTabControl _mainTabControl;
        private ComboBoxEdit _positionCombo;
        private ComboBoxEdit _playTypeCombo;
        private ComboBoxEdit _trendPeriodCombo;
        private DateEdit _datePicker;
        private TextEdit _usernameEdit;
        private TextEdit _passwordEdit;
        private CheckEdit _autoLoginCheck;
        private bool _isLoggedIn = false;
        private DevExpress.XtraGrid.GridControl _lotteryDataGrid;
        private DevExpress.XtraGrid.Views.Grid.GridView _lotteryDataGridView;

        public Form1()
        {
            InitializeComponent();
            InitializeServices();
            InitializeUI();
        }

        private void InitializeServices()
        {
            _statisticsService = new BinggoStatisticsService();
            _dataService = new DataService(_statisticsService);
        }

        private void InitializeUI()
        {
            Text = "宾果数据统计系统";
            Size = new Size(1600, 1000);
            StartPosition = FormStartPosition.CenterScreen;

            // 创建工具栏
            CreateToolbar();

            // 创建主选项卡
            _mainTabControl = new XtraTabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            // 路珠统计选项卡
            CreateRoadBeadTab();
            // 走势图选项卡
            CreateTrendTab();
            // 连续统计选项卡
            CreateConsecutiveStatsTab();
            // 数量统计选项卡
            CreateCountStatsTab();

            // 添加到现有控件之后
            Controls.Add(_mainTabControl);
            Controls.SetChildIndex(_mainTabControl, 0);
        }

        private void CreateToolbar()
        {
            // 使用Designer中已定义的barManager1
            // 创建一个工具栏Panel
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(5)
            };

            // 位置选择
            var positionLabel = new Label
            {
                Text = "位置:",
                Location = new Point(10, 15),
                AutoSize = true
            };
            _positionCombo = new ComboBoxEdit
            {
                Location = new Point(60, 12),
                Width = 100
            };
            _positionCombo.Properties.Items.AddRange(new[] { "P1", "P2", "P3", "P4", "P5", "总和" });
            _positionCombo.SelectedIndex = 0;
            _positionCombo.SelectedIndexChanged += (s, e) => RefreshAllTabs();

            // 玩法选择
            var playTypeLabel = new Label
            {
                Text = "玩法:",
                Location = new Point(180, 15),
                AutoSize = true
            };
            _playTypeCombo = new ComboBoxEdit
            {
                Location = new Point(230, 12),
                Width = 100
            };
            _playTypeCombo.Properties.Items.AddRange(new[] { "大小", "单双", "尾大小", "合单双", "龙虎" });
            _playTypeCombo.SelectedIndex = 0;
            _playTypeCombo.SelectedIndexChanged += (s, e) => RefreshAllTabs();

            // 走势周期选择
            var trendPeriodLabel = new Label
            {
                Text = "走势周期:",
                Location = new Point(350, 15),
                AutoSize = true
            };
            _trendPeriodCombo = new ComboBoxEdit
            {
                Location = new Point(430, 12),
                Width = 120
            };
            _trendPeriodCombo.Properties.Items.AddRange(new[] { "10期", "50期", "100期", "203期(日)", "3日", "一周", "一月", "5日线" });
            _trendPeriodCombo.SelectedIndex = 2;
            _trendPeriodCombo.SelectedIndexChanged += (s, e) => RefreshTrendTab();

            // 日期选择
            var dateLabel = new Label
            {
                Text = "日期:",
                Location = new Point(570, 15),
                AutoSize = true
            };
            _datePicker = new DateEdit
            {
                Location = new Point(620, 12),
                Width = 120,
                EditValue = DateTime.Today
            };

            // 登录配置
            var usernameLabel = new Label
            {
                Text = "账号:",
                Location = new Point(760, 15),
                AutoSize = true
            };
            _usernameEdit = new TextEdit
            {
                Location = new Point(810, 12),
                Width = 100
            };

            var passwordLabel = new Label
            {
                Text = "密码:",
                Location = new Point(920, 15),
                AutoSize = true
            };
            _passwordEdit = new TextEdit
            {
                Location = new Point(970, 12),
                Width = 100
            };
            _passwordEdit.Properties.PasswordChar = '*';

            _autoLoginCheck = new CheckEdit
            {
                Text = "自动登录",
                Location = new Point(1080, 12),
                Width = 80
            };

            // 登录按钮
            var loginBtn = new SimpleButton
            {
                Text = "登录",
                Location = new Point(1170, 10),
                Width = 60
            };
            loginBtn.Click += async (s, e) => await LoginAsync();

            // 加载数据按钮
            var loadDataBtn = new SimpleButton
            {
                Text = "加载数据",
                Location = new Point(1240, 10),
                Width = 100
            };
            loadDataBtn.Click += async (s, e) => await LoadDataAsync();

            toolbarPanel.Controls.AddRange(new Control[] 
            { 
                positionLabel, _positionCombo, 
                playTypeLabel, _playTypeCombo,
                trendPeriodLabel, _trendPeriodCombo,
                dateLabel, _datePicker,
                usernameLabel, _usernameEdit,
                passwordLabel, _passwordEdit,
                _autoLoginCheck,
                loginBtn,
                loadDataBtn
            });

            // 加载保存的配置
            LoadConfig();
            
            // 自动登录
            if (_autoLoginCheck.Checked && !string.IsNullOrEmpty(_usernameEdit.Text) && !string.IsNullOrEmpty(_passwordEdit.Text))
            {
                _ = LoginAsync(); // 异步执行，不等待
            }

            Controls.Add(toolbarPanel);
            Controls.SetChildIndex(toolbarPanel, 0);
        }

        private void CreateRoadBeadTab()
        {
            var tab = new XtraTabPage { Text = "路珠统计" };
            var mainSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400
            };

            // 上半部分：路珠显示（带滚动条）
            var roadBeadPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };
            
            var roadBeadControl = new RoadBeadControl
            {
                BackColor = Color.White,
                Location = new Point(0, 0)
            };
            roadBeadControl.Tag = roadBeadControl;
            
            roadBeadPanel.Controls.Add(roadBeadControl);

            // 下半部分：开奖数据表格
            var bottomSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 200
            };

            // 开奖数据表格
            _lotteryDataGrid = new DevExpress.XtraGrid.GridControl
            {
                Dock = DockStyle.Fill
            };
            _lotteryDataGridView = new DevExpress.XtraGrid.Views.Grid.GridView(_lotteryDataGrid);
            _lotteryDataGrid.MainView = _lotteryDataGridView;
            _lotteryDataGridView.OptionsView.ShowGroupPanel = false;
            _lotteryDataGridView.OptionsBehavior.Editable = false;
            _lotteryDataGridView.OptionsSelection.MultiSelect = false;
            _lotteryDataGridView.OptionsView.ShowIndicator = true;

            // 初始化列
            InitializeLotteryDataGridColumns();

            var infoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };

            bottomSplitContainer.Panel1.Controls.Add(_lotteryDataGrid);
            bottomSplitContainer.Panel2.Controls.Add(infoPanel);

            mainSplitContainer.Panel1.Controls.Add(roadBeadPanel);
            mainSplitContainer.Panel2.Controls.Add(bottomSplitContainer);
            tab.Controls.Add(mainSplitContainer);
            _mainTabControl.TabPages.Add(tab);
        }

        private void InitializeLotteryDataGridColumns()
        {
            _lotteryDataGridView.Columns.Clear();

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "IssueId",
                Caption = "期号",
                Width = 100,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "LotteryData",
                Caption = "开奖号码",
                Width = 150,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "P1Number",
                Caption = "P1",
                Width = 60,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "P2Number",
                Caption = "P2",
                Width = 60,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "P3Number",
                Caption = "P3",
                Width = 60,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "P4Number",
                Caption = "P4",
                Width = 60,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "P5Number",
                Caption = "P5",
                Width = 60,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "SumNumber",
                Caption = "总和",
                Width = 80,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "SumSize",
                Caption = "大小",
                Width = 60,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "SumOddEven",
                Caption = "单双",
                Width = 60,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "DragonTiger",
                Caption = "龙虎",
                Width = 60,
                Visible = true
            });

            _lotteryDataGridView.Columns.Add(new DevExpress.XtraGrid.Columns.GridColumn
            {
                FieldName = "OpenTime",
                Caption = "开奖时间",
                Width = 150,
                Visible = true
            });
        }

        private void CreateTrendTab()
        {
            var tab = new XtraTabPage { Text = "走势图" };
            var trendChart = new TrendChartControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            trendChart.Tag = trendChart;
            tab.Controls.Add(trendChart);
            _mainTabControl.TabPages.Add(tab);
        }

        private void CreateConsecutiveStatsTab()
        {
            var tab = new XtraTabPage { Text = "连续统计" };
            var consecutiveControl = new ConsecutiveStatsControl
            {
                Dock = DockStyle.Fill
            };
            consecutiveControl.Tag = consecutiveControl;
            tab.Controls.Add(consecutiveControl);
            _mainTabControl.TabPages.Add(tab);
        }

        private void CreateCountStatsTab()
        {
            var tab = new XtraTabPage { Text = "数量统计" };
            var grid = new DevExpress.XtraGrid.GridControl
            {
                Dock = DockStyle.Fill
            };
            var view = new DevExpress.XtraGrid.Views.Grid.GridView(grid);
            grid.MainView = view;
            tab.Controls.Add(grid);
            _mainTabControl.TabPages.Add(tab);
        }

        private void RefreshAllTabs()
        {
            RefreshRoadBeadTab();
            RefreshConsecutiveStatsTab();
            RefreshCountStatsTab();
            RefreshLotteryDataGrid();
        }

        private void RefreshRoadBeadTab()
        {
            var tab = _mainTabControl.TabPages[0];
            var splitContainer = tab.Controls.OfType<SplitContainer>().FirstOrDefault();
            if (splitContainer == null) return;

            var roadBeadPanel = splitContainer.Panel1.Controls.OfType<Panel>().FirstOrDefault();
            if (roadBeadPanel == null) return;
            
            var roadBeadControl = roadBeadPanel.Controls.OfType<RoadBeadControl>().FirstOrDefault();
            if (roadBeadControl == null) return;

            var position = GetSelectedPosition();
            var playType = GetSelectedPlayType();
            
            // 获取所有数据并按时间排序
            var allData = _statisticsService.GetAllData().ToList();
            
            // 根据数据生成路珠数据和期号列表（保持顺序一致）
            var roadBeadData = new List<Statistics.PositionPlayResult>();
            var issueIds = new List<int>();
            
            foreach (var lotteryData in allData)
            {
                var ball = lotteryData.GetBallNumber(position);
                if (ball == null) continue;

                Statistics.PlayResult result = playType switch
                {
                    Statistics.GamePlayType.Size => ball.Size == GameSizeType.Big 
                        ? Statistics.PlayResult.Big 
                        : Statistics.PlayResult.Small,
                    Statistics.GamePlayType.OddEven => ball.OddEven == GameOddEvenType.Odd 
                        ? Statistics.PlayResult.Odd 
                        : Statistics.PlayResult.Even,
                    Statistics.GamePlayType.TailSize => ball.TailSize == GameTailSizeType.TailBig 
                        ? Statistics.PlayResult.TailBig 
                        : Statistics.PlayResult.TailSmall,
                    Statistics.GamePlayType.SumOddEven => ball.SumOddEven == GameSumOddEvenType.SumOdd 
                        ? Statistics.PlayResult.SumOdd 
                        : Statistics.PlayResult.SumEven,
                    Statistics.GamePlayType.DragonTiger => position == BallPosition.Sum
                        ? (lotteryData.DragonTiger == GameDragonTigerType.Dragon 
                            ? Statistics.PlayResult.Dragon
                            : lotteryData.DragonTiger == GameDragonTigerType.Tiger 
                            ? Statistics.PlayResult.Tiger
                            : Statistics.PlayResult.Draw)
                        : Statistics.PlayResult.Unknown,
                    _ => Statistics.PlayResult.Unknown
                };

                if (result != Statistics.PlayResult.Unknown)
                {
                    roadBeadData.Add(new Statistics.PositionPlayResult(position, playType, result));
                    issueIds.Add(lotteryData.IssueId);
                }
            }
            
            roadBeadControl.SetData(roadBeadData, issueIds);
        }

        private void RefreshTrendTab()
        {
            var tab = _mainTabControl.TabPages[1];
            var trendChart = tab.Controls.OfType<TrendChartControl>().FirstOrDefault();
            if (trendChart == null) return;

            var period = _trendPeriodCombo.SelectedIndex;
            List<TrendDataPoint> data;

            switch (period)
            {
                case 0: // 10期
                    data = _statisticsService.GetTrendDataByCount(10);
                    break;
                case 1: // 50期
                    data = _statisticsService.GetTrendDataByCount(50);
                    break;
                case 2: // 100期
                    data = _statisticsService.GetTrendDataByCount(100);
                    break;
                case 3: // 203期(日)
                    data = _statisticsService.GetTrendDataByCount(203);
                    break;
                case 4: // 3日
                    data = _statisticsService.GetTrendData(TimeSpan.FromDays(3));
                    break;
                case 5: // 一周
                    data = _statisticsService.GetTrendData(TimeSpan.FromDays(7));
                    break;
                case 6: // 一月
                    data = _statisticsService.GetTrendData(TimeSpan.FromDays(30));
                    break;
                case 7: // 5日线
                    data = _statisticsService.GetTrendData(TimeSpan.FromDays(5));
                    break;
                default:
                    data = _statisticsService.GetTrendDataByCount(100);
                    break;
            }

            trendChart.PlayType = GetSelectedPlayType();
            trendChart.SetData(data);
        }

        private void RefreshConsecutiveStatsTab()
        {
            var tab = _mainTabControl.TabPages[2];
            var consecutiveControl = tab.Controls.OfType<ConsecutiveStatsControl>().FirstOrDefault();
            if (consecutiveControl == null) return;

            var position = GetSelectedPosition();
            var playType = GetSelectedPlayType();
            var data = _statisticsService.GetConsecutiveStats(position, playType);
            consecutiveControl.SetData(data);
        }

        private void RefreshCountStatsTab()
        {
            var tab = _mainTabControl.TabPages[3];
            var grid = tab.Controls.OfType<DevExpress.XtraGrid.GridControl>().FirstOrDefault();
            if (grid == null) return;

            var position = GetSelectedPosition();
            var playType = GetSelectedPlayType();
            var stats = _statisticsService.GetCountStats(position, playType);

            // 这里可以绑定到Grid，暂时简化处理
        }

        private BallPosition GetSelectedPosition()
        {
            return _positionCombo.SelectedIndex switch
            {
                0 => BallPosition.P1,
                1 => BallPosition.P2,
                2 => BallPosition.P3,
                3 => BallPosition.P4,
                4 => BallPosition.P5,
                5 => BallPosition.Sum,
                _ => BallPosition.P1
            };
        }

        private GamePlayType GetSelectedPlayType()
        {
            return _playTypeCombo.SelectedIndex switch
            {
                0 => GamePlayType.Size,
                1 => GamePlayType.OddEven,
                2 => GamePlayType.TailSize,
                3 => GamePlayType.SumOddEven,
                4 => GamePlayType.DragonTiger,
                _ => GamePlayType.Size
            };
        }

        private async Task LoginAsync()
        {
            var username = _usernameEdit.Text.Trim();
            var password = _passwordEdit.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                XtraMessageBox.Show("请输入账号和密码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var result = await _dataService.LoginAsync(username, password);
                if (result)
                {
                    _isLoggedIn = true;
                    XtraMessageBox.Show("登录成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // 保存配置
                    SaveConfig();
                }
                else
                {
                    XtraMessageBox.Show("登录失败，请检查账号密码", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"登录失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadDataAsync()
        {
            if (!_isLoggedIn)
            {
                XtraMessageBox.Show("请先登录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedDate = _datePicker.DateTime.Date;

            try
            {
                await _dataService.LoadFromApiAsync(selectedDate);
                RefreshAllTabs();
                RefreshTrendTab();
                RefreshLotteryDataGrid();
                XtraMessageBox.Show($"数据加载成功！共加载 {selectedDate:yyyy-MM-dd} 的数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshLotteryDataGrid()
        {
            if (_lotteryDataGrid == null) return;

            var allData = _statisticsService.GetAllData();
            var displayData = allData.Select(d => new
            {
                IssueId = d.IssueId,
                LotteryData = d.LotteryData,
                P1Number = d.P1?.Number ?? 0,
                P2Number = d.P2?.Number ?? 0,
                P3Number = d.P3?.Number ?? 0,
                P4Number = d.P4?.Number ?? 0,
                P5Number = d.P5?.Number ?? 0,
                SumNumber = d.PSum?.Number ?? 0,
                SumSize = d.PSum?.GetSizeText() ?? "",
                SumOddEven = d.PSum?.GetOddEvenText() ?? "",
                DragonTiger = d.GetDragonTigerText(),
                OpenTime = d.OpenTime.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            _lotteryDataGrid.DataSource = displayData;
            _lotteryDataGridView.BestFitColumns();
        }

        private void LoadConfig()
        {
            try
            {
                // 从配置文件或注册表加载
                var configFile = Path.Combine(Application.StartupPath, "config.json");
                if (File.Exists(configFile))
                {
                    var json = File.ReadAllText(configFile);
                    var config = System.Text.Json.JsonSerializer.Deserialize<LoginConfig>(json);
                    if (config != null)
                    {
                        _usernameEdit.Text = config.Username ?? "";
                        _passwordEdit.Text = config.Password ?? "";
                        _autoLoginCheck.Checked = config.AutoLogin;
                        if (config.LastDate.HasValue)
                        {
                            _datePicker.DateTime = config.LastDate.Value;
                        }
                    }
                }
            }
            catch
            {
                // 忽略配置加载错误
            }
        }

        private void SaveConfig()
        {
            try
            {
                var config = new LoginConfig
                {
                    Username = _usernameEdit.Text,
                    Password = _autoLoginCheck.Checked ? _passwordEdit.Text : "",
                    AutoLogin = _autoLoginCheck.Checked,
                    LastDate = _datePicker.DateTime
                };

                var configFile = Path.Combine(Application.StartupPath, "config.json");
                var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFile, json);
            }
            catch
            {
                // 忽略配置保存错误
            }
        }

        private class LoginConfig
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public bool AutoLogin { get; set; }
            public DateTime? LastDate { get; set; }
        }

        /// <summary>
        /// 添加开奖数据（数据入口）
        /// </summary>
        public void AddLotteryData(int issueId, string lotteryData, DateTime openTime)
        {
            _dataService.AddLotteryData(issueId, lotteryData, openTime);
            RefreshAllTabs();
            RefreshTrendTab();
        }

        /// <summary>
        /// 批量添加开奖数据（数据入口）
        /// </summary>
        public void AddLotteryDataRange(System.Collections.Generic.IEnumerable<(int issueId, string lotteryData, DateTime openTime)> dataList)
        {
            _dataService.AddLotteryDataRange(dataList);
            RefreshAllTabs();
            RefreshTrendTab();
        }
    }
}
