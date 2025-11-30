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
        private bool _isLoggedIn = false;

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
            // 设置默认值
            _positionCombo.SelectedIndex = 0;
            _playTypeCombo.SelectedIndex = 0;
            _trendPeriodCombo.SelectedIndex = 2;
            _datePicker.DateTime = DateTime.Today;

            // 绑定事件
            _positionCombo.SelectedIndexChanged += (s, e) => RefreshAllTabs();
            _playTypeCombo.SelectedIndexChanged += (s, e) => RefreshAllTabs();
            _trendPeriodCombo.SelectedIndexChanged += (s, e) => RefreshTrendTab();
            loginBtn.Click += async (s, e) => await LoginAsync();
            loadDataBtn.Click += async (s, e) => await LoadDataAsync();

            // 初始化表格列
            InitializeLotteryDataGridColumns();

            // 加载保存的配置
            LoadConfig();
            
            // 自动登录
            if (_autoLoginCheck.Checked && !string.IsNullOrEmpty(_usernameEdit.Text) && !string.IsNullOrEmpty(_passwordEdit.Text))
            {
                _ = LoginAsync(); // 异步执行，不等待
            }
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


        private void RefreshAllTabs()
        {
            RefreshRoadBeadTab();
            RefreshConsecutiveStatsTab();
            RefreshCountStatsTab();
            RefreshLotteryDataGrid();
        }

        private void RefreshRoadBeadTab()
        {
            var position = GetSelectedPosition();
            
            // 获取所有数据并按时间排序
            var allData = _statisticsService.GetAllData().ToList();
            
            // 生成4组路珠数据：大小、单双、尾大小、和单双
            var sizeData = new List<Statistics.PositionPlayResult>();
            var oddEvenData = new List<Statistics.PositionPlayResult>();
            var tailSizeData = new List<Statistics.PositionPlayResult>();
            var sumOddEvenData = new List<Statistics.PositionPlayResult>();
            var issueIds = new List<int>();
            
            foreach (var lotteryData in allData)
            {
                var ball = lotteryData.GetBallNumber(position);
                if (ball == null) continue;

                // 大小
                var sizeResult = ball.Size == GameSizeType.Big 
                    ? Statistics.PlayResult.Big 
                    : Statistics.PlayResult.Small;
                sizeData.Add(new Statistics.PositionPlayResult(position, Statistics.GamePlayType.Size, sizeResult));
                
                // 单双
                var oddEvenResult = ball.OddEven == GameOddEvenType.Odd 
                    ? Statistics.PlayResult.Odd 
                    : Statistics.PlayResult.Even;
                oddEvenData.Add(new Statistics.PositionPlayResult(position, Statistics.GamePlayType.OddEven, oddEvenResult));
                
                // 尾大小
                var tailSizeResult = ball.TailSize == GameTailSizeType.TailBig 
                    ? Statistics.PlayResult.TailBig 
                    : Statistics.PlayResult.TailSmall;
                tailSizeData.Add(new Statistics.PositionPlayResult(position, Statistics.GamePlayType.TailSize, tailSizeResult));
                
                // 和单双
                var sumOddEvenResult = ball.SumOddEven == GameSumOddEvenType.SumOdd 
                    ? Statistics.PlayResult.SumOdd 
                    : Statistics.PlayResult.SumEven;
                sumOddEvenData.Add(new Statistics.PositionPlayResult(position, Statistics.GamePlayType.SumOddEven, sumOddEvenResult));
                
                issueIds.Add(lotteryData.IssueId);
            }
            
            // 更新4个路珠控件
            if (roadBeadSizeControl != null)
            {
                roadBeadSizeControl.SetData(sizeData, issueIds);
            }
            
            if (roadBeadOddEvenControl != null)
            {
                roadBeadOddEvenControl.SetData(oddEvenData, issueIds);
            }
            
            if (roadBeadTailSizeControl != null)
            {
                roadBeadTailSizeControl.SetData(tailSizeData, issueIds);
            }
            
            if (roadBeadSumOddEvenControl != null)
            {
                roadBeadSumOddEvenControl.SetData(sumOddEvenData, issueIds);
            }
        }

        private void RefreshTrendTab()
        {
            if (trendChartControl == null) return;

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

            trendChartControl.PlayType = GetSelectedPlayType();
            trendChartControl.SetData(data);
        }

        private void RefreshConsecutiveStatsTab()
        {
            if (consecutiveStatsControl == null) return;

            var position = GetSelectedPosition();
            var playType = GetSelectedPlayType();
            var data = _statisticsService.GetConsecutiveStats(position, playType);
            consecutiveStatsControl.SetData(data);
        }

        private void RefreshCountStatsTab()
        {
            if (countStatsGrid == null) return;

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
            var selectedDate = _datePicker.DateTime.Date;

            try
            {
                // 使用新的加载逻辑：先从 SQLite 加载，如果没有再从网络获取
                await _dataService.LoadDataByDateAsync(selectedDate);
                RefreshAllTabs();
                RefreshTrendTab();
                RefreshLotteryDataGrid();
                
                var dataCount = _statisticsService.GetAllData().Count;
                XtraMessageBox.Show($"数据加载成功！共加载 {dataCount} 条数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshLotteryDataGrid()
        {
            if (_lotteryDataGrid == null) return;

            // 直接使用 BinGoDataEntity（继承自 BinGoData），避免不必要的转换
            var dataList = _dataService.GetCurrentDataList();

            // 直接使用 BinGoData 的便捷属性进行显示
            var displayData = dataList.Select(d => new
            {
                d.IssueId,
                d.DayIndex,
                d.OpenTimeString,
                d.OpenDateString,
                P1 = d.P1Number,
                P2 = d.P2Number,
                P3 = d.P3Number,
                P4 = d.P4Number,
                P5 = d.P5Number,
                Sum = d.SumNumber,
                P1Size = d.P1Size,
                P2Size = d.P2Size,
                P3Size = d.P3Size,
                P4Size = d.P4Size,
                P5Size = d.P5Size,
                SumSize = d.SumSize,
                P1OddEven = d.P1OddEven,
                P2OddEven = d.P2OddEven,
                P3OddEven = d.P3OddEven,
                P4OddEven = d.P4OddEven,
                P5OddEven = d.P5OddEven,
                SumOddEven = d.SumOddEven,
                P1TailSize = d.P1TailSize,
                P2TailSize = d.P2TailSize,
                P3TailSize = d.P3TailSize,
                P4TailSize = d.P4TailSize,
                P5TailSize = d.P5TailSize,
                P1SumOddEven = d.P1SumOddEven,
                P2SumOddEven = d.P2SumOddEven,
                P3SumOddEven = d.P3SumOddEven,
                P4SumOddEven = d.P4SumOddEven,
                P5SumOddEven = d.P5SumOddEven,
                DragonTiger = d.DragonTigerText
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
