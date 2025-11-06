using Sunny.UI;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models.Games.Binggo;
using BaiShengVx3Plus.Models.Games.Binggo.Events;
using BaiShengVx3Plus.Helpers;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BaiShengVx3Plus.UserControls
{
    /// <summary>
    /// 上期开奖数据显示控件
    /// 🔥 完全参考 F5BotV2 的显示逻辑
    /// 
    /// 功能：
    /// - 显示上期期号
    /// - 显示上期开奖号码（P1-P5 + 总和）
    /// - 显示大小单双、龙虎
    /// - 显示开奖时间
    /// - 实时更新
    /// </summary>
    public partial class UcBinggoDataLast : UserControl
    {
        private IBinggoLotteryService? _lotteryService;
        private BinggoLotteryData? _lastData;
        private UILabel[] numberLabels = new UILabel[6];  // P1-P5 + Sum
        
        public UcBinggoDataLast()
        {
            InitializeUI();
        }
        
        /// <summary>
        /// 设置开奖服务并订阅事件
        /// </summary>
        public void SetLotteryService(IBinggoLotteryService lotteryService)
        {
            // 取消订阅旧服务
            if (_lotteryService != null)
            {
                _lotteryService.LotteryOpened -= OnLotteryOpened;
            }
            
            _lotteryService = lotteryService;
            
            // 订阅新服务
            if (_lotteryService != null)
            {
                _lotteryService.LotteryOpened += OnLotteryOpened;
                
                // 🔥 立即加载上期数据
                LoadLastLotteryData();
            }
        }
        
        private void InitializeUI()
        {
            // 🔥 设置控件大小和样式（压缩高度）
            this.Size = new Size(239, 110);
            this.BackColor = Color.FromArgb(255, 248, 225);
            
            // 标题标签（压缩高度）
            var lblTitle = new UILabel
            {
                Text = "上期开奖",
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(48, 48, 48),
                Location = new Point(5, 3),
                Size = new Size(229, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);
            
            // 期号标签（压缩高度）
            lblLastIssue = new UILabel
            {
                Text = "期号: -",
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.FromArgb(48, 48, 48),
                Location = new Point(5, 23),
                Size = new Size(120, 16),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblLastIssue);
            
            // 🔥 开奖时间标签（新增，右对齐）
            lblOpenTime = new UILabel
            {
                Text = "-",
                Font = new Font("微软雅黑", 8F),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(130, 23),
                Size = new Size(104, 16),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(lblOpenTime);
            
            // 🔥 号码显示区域（6个圆形号码：P1-P5 + Sum）
            int startX = 9;
            int startY = 42;
            int spacing = 37;
            int ballSize = 32;
            
            for (int i = 0; i < 6; i++)
            {
                var lblNumber = new UILabel
                {
                    Text = "-",
                    Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(startX + i * spacing, startY),
                    Size = new Size(ballSize, ballSize),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Gray
                };
                
                // 🔥 自定义绘制圆形背景
                lblNumber.Paint += (s, e) =>
                {
                    var lbl = s as UILabel;
                    if (lbl != null)
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (var brush = new SolidBrush(lbl.BackColor))
                        {
                            e.Graphics.FillEllipse(brush, 0, 0, lbl.Width - 1, lbl.Height - 1);
                        }
                        
                        // 绘制文字
                        using (var format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        })
                        {
                            e.Graphics.DrawString(lbl.Text, lbl.Font, new SolidBrush(lbl.ForeColor), 
                                new RectangleF(0, 0, lbl.Width, lbl.Height), format);
                        }
                    }
                };
                
                numberLabels[i] = lblNumber;
                this.Controls.Add(lblNumber);
            }
            
            // 统计信息标签（大小单双、龙虎）
            lblStatistics = new UILabel
            {
                Text = "大小单双: -",
                Font = new Font("微软雅黑", 8F),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(5, 78),
                Size = new Size(229, 28),
                TextAlign = ContentAlignment.TopCenter
            };
            this.Controls.Add(lblStatistics);
        }
        
        /// <summary>
        /// 🔥 加载上期数据
        /// </summary>
        private async void LoadLastLotteryData()
        {
            try
            {
                if (_lotteryService == null) return;
                
                // 🔥 获取最近1期数据
                var recentData = await _lotteryService.GetRecentLotteryDataAsync(1);
                if (recentData != null && recentData.Count > 0)
                {
                    _lastData = recentData[0];
                    UpdateDisplay();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载上期数据失败: {ex.Message}");
            }
        }
        
        private void UpdateDisplay()
        {
            if (_lastData == null || !_lastData.IsOpened)
            {
                UpdateUIThreadSafe(() =>
                {
                    lblLastIssue.Text = "期号: -";
                    lblOpenTime.Text = "-";
                    lblStatistics.Text = "暂无数据";
                    for (int i = 0; i < 6; i++)
                    {
                        numberLabels[i].Text = "-";
                        numberLabels[i].BackColor = Color.Gray;
                        numberLabels[i].Invalidate(); // 触发重绘
                    }
                });
                return;
            }
            
            UpdateUIThreadSafe(() =>
            {
                // 更新期号
                lblLastIssue.Text = $"期号: {_lastData?.IssueId ?? 0}";
                
                // 🔥 更新开奖时间
                if (!string.IsNullOrEmpty(_lastData?.OpenTime))
                {
                    if (DateTime.TryParse(_lastData.OpenTime, out DateTime openTime))
                    {
                        lblOpenTime.Text = openTime.ToString("HH:mm:ss");
                    }
                    else
                    {
                        lblOpenTime.Text = _lastData.OpenTime;
                    }
                }
                else
                {
                    lblOpenTime.Text = "-";
                }
                
                // 🔥 使用新的 P1-P5 和 PSum 属性
                var balls = new[] { _lastData.P1, _lastData.P2, _lastData.P3, _lastData.P4, _lastData.P5, _lastData.PSum };
                
                for (int i = 0; i < 6; i++)
                {
                    var ball = balls[i];
                    if (ball != null)
                    {
                        int number = ball.Number;
                        numberLabels[i].Text = number.ToString();
                        
                        // 🔥 根据号码设置颜色
                        if (i < 5)  // P1-P5
                        {
                            if (number >= 1 && number <= 10)
                            {
                                numberLabels[i].BackColor = Color.FromArgb(33, 150, 243);  // 蓝色
                            }
                            else if (number >= 11 && number <= 20)
                            {
                                numberLabels[i].BackColor = Color.FromArgb(76, 175, 80);  // 绿色
                            }
                            else if (number >= 21 && number <= 28)
                            {
                                numberLabels[i].BackColor = Color.FromArgb(244, 67, 54);  // 红色
                            }
                            else
                            {
                                numberLabels[i].BackColor = Color.FromArgb(158, 158, 158);  // 灰色
                            }
                        }
                        else  // PSum（总和）
                        {
                            numberLabels[i].BackColor = Color.FromArgb(255, 152, 0);  // 橙色
                        }
                        
                        numberLabels[i].Invalidate(); // 触发重绘
                    }
                    else
                    {
                        numberLabels[i].Text = "-";
                        numberLabels[i].BackColor = Color.Gray;
                    }
                }
                
                // 🔥 使用新的 PSum 属性更新统计
                if (_lastData.PSum != null)
                {
                    string bigSmall = _lastData.PSum.GetSizeText();
                    string oddEven = _lastData.PSum.GetOddEvenText();
                    string dragonTiger = _lastData.GetDragonTigerText();
                    lblStatistics.Text = $"{bigSmall} {oddEven} | {dragonTiger} | 总和: {_lastData.PSum.Number}";
                }
            });
        }
        
        private void OnLotteryOpened(object? sender, BinggoLotteryOpenedEventArgs e)
        {
            _lastData = e.LotteryData;
            UpdateDisplay();
        }
        
        private void UpdateUIThreadSafe(Action action)
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            // 取消订阅事件
            if (_lotteryService != null)
            {
                _lotteryService.LotteryOpened -= OnLotteryOpened;
            }
            
            base.Dispose(disposing);
        }
        
        #region 设计器生成的字段
        
        private UILabel lblLastIssue = null!;
        private UILabel lblOpenTime = null!;
        private UILabel lblStatistics = null!;
        
        #endregion
    }
}
