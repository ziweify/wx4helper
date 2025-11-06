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
        /// 🔥 完全参考 F5BotV2 的逻辑：订阅期号变更和开奖事件
        /// </summary>
        public void SetLotteryService(IBinggoLotteryService lotteryService)
        {
            // 取消订阅旧服务
            if (_lotteryService != null)
            {
                _lotteryService.IssueChanged -= OnIssueChanged;  // 🔥 新增：订阅期号变更
                _lotteryService.LotteryOpened -= OnLotteryOpened;
            }
            
            _lotteryService = lotteryService;
            
            // 订阅新服务
            if (_lotteryService != null)
            {
                _lotteryService.IssueChanged += OnIssueChanged;  // 🔥 新增：订阅期号变更
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
                Console.WriteLine("========== UcBinggoDataLast.LoadLastLotteryData 开始 ==========");
                
                if (_lotteryService == null)
                {
                    Console.WriteLine("❌ LoadLastLotteryData: _lotteryService is null");
                    return;
                }
                
                Console.WriteLine("📡 LoadLastLotteryData: 开始获取最近1期数据...");
                Console.WriteLine($"📡 _lotteryService 类型: {_lotteryService.GetType().Name}");
                
                // 🔥 获取最近1期数据
                var recentData = await _lotteryService.GetRecentLotteryDataAsync(1);
                
                Console.WriteLine($"📡 API返回数据: recentData={recentData}, Count={recentData?.Count ?? 0}");
                
                if (recentData != null && recentData.Count > 0)
                {
                    _lastData = recentData[0];
                    Console.WriteLine($"✅ LoadLastLotteryData: 获取到数据");
                    Console.WriteLine($"   期号={_lastData.IssueId}");
                    Console.WriteLine($"   IsOpened={_lastData.IsOpened}");
                    Console.WriteLine($"   LotteryData={_lastData.LotteryData}");
                    Console.WriteLine($"   OpenTime={_lastData.OpenTime}");
                    
                    if (_lastData.P1 != null)
                    {
                        Console.WriteLine($"   号码: P1={_lastData.P1.Number}, P2={_lastData.P2?.Number}, P3={_lastData.P3?.Number}, P4={_lastData.P4?.Number}, P5={_lastData.P5?.Number}");
                        Console.WriteLine($"   总和: {_lastData.PSum?.Number}");
                        Console.WriteLine($"   龙虎: {_lastData.DragonTiger}");
                    }
                    else
                    {
                        Console.WriteLine("   ⚠️ P1 为 null（数据未解析？）");
                    }
                    
                    Console.WriteLine("📍 调用 UpdateDisplay...");
                    UpdateDisplay();
                    Console.WriteLine("✅ UpdateDisplay 完成");
                }
                else
                {
                    Console.WriteLine("⚠️ LoadLastLotteryData: 未获取到数据 (recentData is null or empty)");
                }
                
                Console.WriteLine("========== UcBinggoDataLast.LoadLastLotteryData 结束 ==========");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LoadLastLotteryData 失败: {ex.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            }
        }
        
        private void UpdateDisplay()
        {
            if (_lastData == null)
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
                        numberLabels[i].Invalidate();
                    }
                });
                return;
            }
            
            UpdateUIThreadSafe(() =>
            {
                // 🔥 期号和时间始终显示（即使未开奖）
                lblLastIssue.Text = $"期号: {_lastData.IssueId}";
                
                // 🔥 更新开奖时间
                if (!string.IsNullOrEmpty(_lastData.OpenTime))
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
                
                // 🔥 如果未开奖，号码显示为 "-"
                if (!_lastData.IsOpened)
                {
                    lblStatistics.Text = "等待开奖...";
                    for (int i = 0; i < 6; i++)
                    {
                        numberLabels[i].Text = "-";
                        numberLabels[i].BackColor = Color.Gray;
                        numberLabels[i].Invalidate();
                    }
                    return;  // ⚠️ 这里返回，不再继续处理号码
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
        
        /// <summary>
        /// 🔥 新增：处理期号变更事件（参考 F5BotV2 的逻辑）
        /// 期号变更时，立即显示上期的期号和时间（即使号码还未开出）
        /// </summary>
        private void OnIssueChanged(object? sender, BinggoIssueChangedEventArgs e)
        {
            Console.WriteLine($"📢 UcBinggoDataLast 收到期号变更事件: {e.OldIssueId} → {e.NewIssueId}");
            
            if (e.LastLotteryData != null)
            {
                Console.WriteLine($"✅ 期号变更带来的上期数据: IssueId={e.LastLotteryData.IssueId}, IsOpened={e.LastLotteryData.IsOpened}");
                _lastData = e.LastLotteryData;
                UpdateDisplay();  // 立即显示期号和时间（号码显示为 "-"）
            }
            else
            {
                Console.WriteLine("⚠️ 期号变更事件中的 LastLotteryData 为 null");
            }
        }
        
        private void OnLotteryOpened(object? sender, BinggoLotteryOpenedEventArgs e)
        {
            Console.WriteLine($"📢 UcBinggoDataLast 收到开奖事件: IssueId={e.LotteryData.IssueId}");
            _lastData = e.LotteryData;
            UpdateDisplay();  // 再次显示，这次包含号码
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
