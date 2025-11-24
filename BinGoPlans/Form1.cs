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
