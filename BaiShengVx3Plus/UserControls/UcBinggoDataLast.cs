using Sunny.UI;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models.Games.Binggo;
using BaiShengVx3Plus.Models.Games.Binggo.Events;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace BaiShengVx3Plus.UserControls
{
    /// <summary>
    /// 上期开奖数据显示控件
    /// 
    /// 功能：
    /// - 显示上期期号
    /// - 显示上期开奖结果（6 个号码）
    /// - 显示大小、单双统计
    /// </summary>
    public partial class UcBinggoDataLast : UserControl
    {
        private IBinggoLotteryService? _lotteryService;
        private BinggoLotteryData? _lastData;
        
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
                
                // 加载最近一期数据
                LoadLastLotteryData();
            }
        }
        
        private void InitializeUI()
        {
            // 设置控件大小和样式
            this.Size = new Size(239, 140);
            this.BackColor = Color.FromArgb(255, 248, 225);
            
            // 标题标签
            var lblTitle = new UILabel
            {
                Text = "上期开奖",
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(48, 48, 48),
                Location = new Point(10, 10),
                Size = new Size(219, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);
            
            // 期号标签
            lblLastIssue = new UILabel
            {
                Text = "期号: -",
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.FromArgb(48, 48, 48),
                Location = new Point(10, 40),
                Size = new Size(219, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblLastIssue);
            
            // 号码面板
            pnlNumbers = new UIPanel
            {
                Location = new Point(10, 65),
                Size = new Size(219, 40),
                FillColor = Color.Transparent,
                RectColor = Color.Transparent
            };
            this.Controls.Add(pnlNumbers);
            
            // 创建 6 个号码标签（使用简单的 Label，手动绘制圆形）
            numberLabels = new UILabel[6];
            for (int i = 0; i < 6; i++)
            {
                numberLabels[i] = new UILabel
                {
                    Text = "-",
                    Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Gray,
                    Location = new Point(i * 36 + 2, 5),
                    Size = new Size(32, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.None
                };
                // 添加圆形外观
                numberLabels[i].Paint += (s, e) =>
                {
                    var lbl = s as UILabel;
                    if (lbl != null)
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (var brush = new SolidBrush(lbl.BackColor))
                        {
                            e.Graphics.FillEllipse(brush, 0, 0, lbl.Width - 1, lbl.Height - 1);
                        }
                        TextRenderer.DrawText(e.Graphics, lbl.Text, lbl.Font, 
                            lbl.ClientRectangle, lbl.ForeColor,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                };
                pnlNumbers.Controls.Add(numberLabels[i]);
            }
            
            // 统计标签
            lblStatistics = new UILabel
            {
                Text = "大小单双: -",
                Font = new Font("微软雅黑", 8F),
                ForeColor = Color.FromArgb(96, 96, 96),
                Location = new Point(10, 110),
                Size = new Size(219, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblStatistics);
        }
        
        private async void LoadLastLotteryData()
        {
            if (_lotteryService == null) return;
            
            try
            {
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
            if (_lastData == null || _lastData.Numbers == null || _lastData.Numbers.Length == 0)
            {
                UpdateUIThreadSafe(() =>
                {
                    lblLastIssue.Text = "期号: -";
                    lblStatistics.Text = "大小单双: -";
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
                
                // 更新号码
                int numCount = Math.Min(6, _lastData?.Numbers?.Length ?? 0);
                for (int i = 0; i < numCount; i++)
                {
                    int number = _lastData!.Numbers![i];
                    numberLabels[i].Text = number.ToString();
                    
                    // 根据号码设置颜色（1-10 蓝色，11-20 绿色，21-28 红色）
                    if (number >= 1 && number <= 10)
                    {
                        numberLabels[i].BackColor = Color.FromArgb(33, 150, 243);
                    }
                    else if (number >= 11 && number <= 20)
                    {
                        numberLabels[i].BackColor = Color.FromArgb(76, 175, 80);
                    }
                    else
                    {
                        numberLabels[i].BackColor = Color.FromArgb(244, 67, 54);
                    }
                    numberLabels[i].Invalidate(); // 触发重绘
                }
                
                // 更新统计
                var numbers = _lastData?.Numbers;
                if (numbers != null && numbers.Length >= 6)
                {
                    int sum = numbers.Take(6).Sum();
                    string bigSmall = sum >= 84 ? "大" : "小";
                    string oddEven = (sum % 2 == 0) ? "双" : "单";
                    lblStatistics.Text = $"{bigSmall} {oddEven} | 总和: {sum}";
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
        private UIPanel pnlNumbers = null!;
        private UILabel[] numberLabels = null!;
        private UILabel lblStatistics = null!;
        
        #endregion
    }
}

