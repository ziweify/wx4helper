using Sunny.UI;
using zhaocaimao.Contracts.Games;
using zhaocaimao.Models.Games.Binggo;
using zhaocaimao.Models.Games.Binggo.Events;
using zhaocaimao.Helpers;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace zhaocaimao.UserControls
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
        
        // 🔥 闪烁动画相关
        private System.Windows.Forms.Timer? _blinkTimer;
        private bool _isBlinking = false;
        private int _blinkCount = 0;
        
        public UcBinggoDataLast()
        {
            InitializeUI();
            InitializeBlinkTimer();
        }
        
        /// <summary>
        /// 🔥 初始化闪烁定时器（让"开奖中"更有动感）
        /// </summary>
        private void InitializeBlinkTimer()
        {
            _blinkTimer = new System.Windows.Forms.Timer();
            _blinkTimer.Interval = 500;  // 0.5秒闪烁一次
            _blinkTimer.Tick += (s, e) =>
            {
                if (_isBlinking && _lastData != null && !_lastData.IsOpened)
                {
                    _blinkCount++;
                    bool showBright = (_blinkCount % 2 == 0);
                    
                    for (int i = 0; i < 6; i++)
                    {
                        if (showBright)
                        {
                            numberLabels[i].BackColor = Color.FromArgb(255, 235, 59);  // 亮黄色
                        }
                        else
                        {
                            numberLabels[i].BackColor = Color.FromArgb(255, 193, 7);   // 金黄色
                        }
                        numberLabels[i].Invalidate();
                    }
                }
            };
        }
        
        /// <summary>
        /// 设置开奖服务并订阅事件
        /// 🔥 完全参考 F5BotV2 的逻辑：订阅期号变更和开奖事件
        /// </summary>
        public void SetLotteryService(IBinggoLotteryService lotteryService)
        {
            Console.WriteLine("========== UcBinggoDataLast.SetLotteryService 开始 ==========");
            
            // 取消订阅旧服务
            if (_lotteryService != null)
            {
                _lotteryService.IssueChanged -= OnIssueChanged;
                _lotteryService.LotteryOpened -= OnLotteryOpened;
            }
            
            _lotteryService = lotteryService;
            
            // 订阅新服务
            if (_lotteryService != null)
            {
                _lotteryService.IssueChanged += OnIssueChanged;
                _lotteryService.LotteryOpened += OnLotteryOpened;
                
                // 🔥 立即本地计算并显示上期期号和时间（不等待 API）
                int currentIssueId = BinggoTimeHelper.GetCurrentIssueId();
                int lastIssueId = BinggoTimeHelper.GetPreviousIssueId(currentIssueId);
                DateTime lastOpenTime = BinggoTimeHelper.GetIssueOpenTime(lastIssueId);
                
                Console.WriteLine($"🔥 SetLotteryService: 本地计算上期 - 当前期号={currentIssueId}, 上期期号={lastIssueId}, 开奖时间={lastOpenTime:HH:mm:ss}");
                
                // 🔥 立即创建空数据对象并显示
                _lastData = new BinggoLotteryData
                {
                    IssueId = lastIssueId,
                    OpenTime = lastOpenTime.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                Console.WriteLine("✅ SetLotteryService: 立即显示上期期号和时间");
                UpdateDisplay();  // 立即显示期号和时间（号码为 ✱）
                
                // 🔥 异步加载号码（不阻塞）
                LoadLastLotteryData();
            }
            
            Console.WriteLine("========== UcBinggoDataLast.SetLotteryService 完成 ==========");
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
            
            // 🔥 号码显示区域（6个号码：P1-P5 + Sum）
            // 优化：字体调小以显示两位数，根据单双显示圆形或方形
            int startX = 9;
            int startY = 42;
            int spacing = 37;
            int ballSize = 32;
            
            for (int i = 0; i < 6; i++)
            {
                var lblNumber = new UILabel
                {
                    Text = "-",
                    Font = new Font("微软雅黑", 10F, FontStyle.Bold),  // 🔥 字体调小
                    ForeColor = Color.White,
                    Location = new Point(startX + i * spacing, startY),
                    Size = new Size(ballSize, ballSize),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Gray
                };
                
                // 🔥 自定义绘制（圆形或方形，根据单双决定）
                lblNumber.Paint += (s, e) =>
                {
                    var lbl = s as UILabel;
                    if (lbl != null)
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        
                        // 🔥 判断是否为单数（通过 Tag 传递）
                        bool isOdd = lbl.Tag is bool odd && odd;
                        
                        using (var brush = new SolidBrush(lbl.BackColor))
                        {
                            if (isOdd)
                            {
                                // 单数：圆形背景
                                e.Graphics.FillEllipse(brush, 0, 0, lbl.Width - 1, lbl.Height - 1);
                            }
                            else
                            {
                                // 双数：方形背景（圆角矩形）
                                using (var path = GetRoundedRectPath(new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1), 6))
                                {
                                    e.Graphics.FillPath(brush, path);
                                }
                            }
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
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),  // 🔥 字体放大加粗
                ForeColor = Color.FromArgb(48, 48, 48),
                Location = new Point(5, 78),
                Size = new Size(229, 28),
                TextAlign = ContentAlignment.TopCenter
            };
            this.Controls.Add(lblStatistics);
        }
        
        /// <summary>
        /// 🔥 获取圆角矩形路径（用于绘制双数的方形背景）
        /// </summary>
        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int diameter = radius * 2;
            
            // 左上角
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // 右上角
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // 右下角
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // 左下角
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            
            path.CloseFigure();
            return path;
        }
        
        /// <summary>
        /// 🔥 加载上期数据（完全参考 F5BotV2 的逻辑）
        /// ⚠️ 注意：这个方法已经不需要了，因为期号切换时会自动设置上期数据
        /// 但保留用于初始化时的数据加载
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
                
                // 🔥 从 API 获取最近1期号码
                Console.WriteLine("📡 LoadLastLotteryData: 开始从 API 获取号码...");
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
                
                // 🔥 如果未开奖，号码显示为 "✱"（参考 F5BotV2）
                if (!_lastData.IsOpened)
                {
                    lblStatistics.Text = "🎲 开 奖 中 🎲";
                    for (int i = 0; i < 6; i++)
                    {
                        numberLabels[i].Text = "✱";  // 使用 Unicode 星号，更美观
                        numberLabels[i].Font = new Font("微软雅黑", 18F, FontStyle.Bold);
                        numberLabels[i].BackColor = Color.FromArgb(255, 193, 7);  // 金黄色，表示开奖中
                        numberLabels[i].ForeColor = Color.White;
                        numberLabels[i].Invalidate();
                    }
                    
                    // 🔥 启动闪烁动画
                    if (!_isBlinking)
                    {
                        _isBlinking = true;
                        _blinkCount = 0;
                        _blinkTimer?.Start();
                    }
                    
                    return;  // ⚠️ 这里返回，不再继续处理号码
                }
                
                // 🔥 已开奖，停止闪烁动画
                if (_isBlinking)
                {
                    _isBlinking = false;
                    _blinkTimer?.Stop();
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
                        numberLabels[i].Font = new Font("微软雅黑", 10F, FontStyle.Bold);
                        
                        // 🔥 新规则：大红色，小绿色（基于 Size 属性）
                        if (ball.Size == BaiShengVx3Plus.Models.Games.Binggo.SizeType.Big)
                        {
                            numberLabels[i].BackColor = Color.FromArgb(244, 67, 54);  // 红色（大）
                        }
                        else  // Small
                        {
                            numberLabels[i].BackColor = Color.FromArgb(76, 175, 80);  // 绿色（小）
                        }
                        
                        // 🔥 通过 Tag 传递单双信息（用于绘制圆形或方形）
                        numberLabels[i].Tag = (ball.OddEven == BaiShengVx3Plus.Models.Games.Binggo.OddEvenType.Odd);
                        
                        numberLabels[i].Invalidate(); // 触发重绘
                    }
                    else
                    {
                        numberLabels[i].Text = "-";
                        numberLabels[i].BackColor = Color.Gray;
                        numberLabels[i].Tag = false;
                        numberLabels[i].Invalidate();
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
        /// 🔥 处理期号变更事件（完全参考 F5BotV2 的逻辑）
        /// 期号切换时，Service 已经计算好了上期数据（期号、时间），直接显示
        /// 号码显示为 "✱"（开奖中）
        /// </summary>
        private void OnIssueChanged(object? sender, BinggoIssueChangedEventArgs e)
        {
            Console.WriteLine($"📢 UcBinggoDataLast 收到期号变更事件: {e.OldIssueId} → {e.NewIssueId}");
            
            if (e.LastLotteryData != null)
            {
                Console.WriteLine($"✅ 期号变更带来的上期数据: IssueId={e.LastLotteryData.IssueId}, OpenTime={e.LastLotteryData.OpenTime}");
                Console.WriteLine($"   IsOpened={e.LastLotteryData.IsOpened}（false表示号码显示为✱）");
                
                _lastData = e.LastLotteryData;
                UpdateDisplay();  // 立即显示：期号、时间、号码（✱）
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
