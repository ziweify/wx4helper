using Sunny.UI;
using zhaocaimao.Contracts.Games;
using zhaocaimao.Models.Games.Binggo;
using zhaocaimao.Models.Games.Binggo.Events;
using zhaocaimao.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace zhaocaimao.UserControls
{
    /// <summary>
    /// 当前期开奖数据显示控件
    /// 🔥 完全参考 F5BotV2 的简洁排版设计
    /// 
    /// 功能：
    /// - 显示当前期号
    /// - 显示开奖时间
    /// - 显示距封盘倒计时
    /// - 显示当前状态
    /// - 实时更新
    /// </summary>
    public partial class UcBinggoDataCur : UserControl
    {
        private IBinggoLotteryService? _lotteryService;
        
        public UcBinggoDataCur()
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
                _lotteryService.IssueChanged -= OnIssueChanged;
                _lotteryService.StatusChanged -= OnStatusChanged;
                _lotteryService.CountdownTick -= OnCountdownTick;
            }
            
            _lotteryService = lotteryService;
            
            // 订阅新服务
            if (_lotteryService != null)
            {
                _lotteryService.IssueChanged += OnIssueChanged;
                _lotteryService.StatusChanged += OnStatusChanged;
                _lotteryService.CountdownTick += OnCountdownTick;
                
                // 立即更新显示
                UpdateDisplay();
            }
        }
        
        private void InitializeUI()
        {
            // 🔥 F5BotV2 风格：紧凑、简洁、信息密集
            this.Size = new Size(239, 85);
            this.BackColor = Color.FromArgb(243, 249, 255);
            this.BorderStyle = BorderStyle.FixedSingle;
            
            // ========================================
            // 🔥 第一行：期号 + 开奖时间（左右布局）
            // ========================================
            
            var lblIssueTitle = new Label
            {
                Text = "期号:",
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(5, 5),
                Size = new Size(36, 18),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblIssueTitle);
            
            lblCurrentIssue = new Label
            {
                Text = "-",
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(48, 48, 48),
                Location = new Point(43, 5),
                Size = new Size(100, 18),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblCurrentIssue);
            
            lblOpenTime = new Label
            {
                Text = "-",
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(150, 5),
                Size = new Size(84, 18),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(lblOpenTime);
            
            // ========================================
            // 🔥 第二行：倒计时（大字体，居中）
            // ========================================
            
            lblCountdown = new Label
            {
                Text = "00:00",
                Font = new Font("微软雅黑", 26F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 136),
                Location = new Point(5, 25),
                Size = new Size(229, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblCountdown);
            
            // ========================================
            // 🔥 第三行：状态（底部居中）
            // ========================================
            
            lblStatus = new Label
            {
                Text = "未开始",
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.Gray,
                Location = new Point(5, 65),
                Size = new Size(229, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblStatus);
        }
        
        private void UpdateDisplay()
        {
            if (_lotteryService == null) return;
            
            UpdateUIThreadSafe(() =>
            {
                // 更新期号
                int issueId = _lotteryService.CurrentIssueId;
                lblCurrentIssue.Text = issueId > 0 ? issueId.ToString() : "-";
                
                // 🔥 更新开奖时间（F5BotV2 风格：简洁显示）
                if (issueId > 0)
                {
                    var openTime = BinggoTimeHelper.GetIssueOpenTime(issueId);
                    lblOpenTime.Text = openTime.ToString("HH:mm:ss");
                }
                else
                {
                    lblOpenTime.Text = "-";
                }
                
                // 更新倒计时
                int seconds = _lotteryService.SecondsToSeal;
                int minutes = seconds / 60;
                int secs = seconds % 60;
                lblCountdown.Text = $"{minutes:D2}:{secs:D2}";
                
                // 🔥 根据倒计时调整颜色（参考 F5BotV2）
                if (seconds <= 0)
                {
                    lblCountdown.ForeColor = Color.FromArgb(156, 39, 176); // 紫色（开奖中）
                }
                else if (seconds <= 10)
                {
                    lblCountdown.ForeColor = Color.FromArgb(244, 67, 54); // 红色警告
                }
                else if (seconds <= 30)
                {
                    lblCountdown.ForeColor = Color.FromArgb(255, 152, 0); // 橙色提示
                }
                else
                {
                    lblCountdown.ForeColor = Color.FromArgb(0, 150, 136); // 绿色正常
                }
                
                // 更新状态
                var status = _lotteryService.CurrentStatus;
                switch (status)
                {
                    case BinggoLotteryStatus.开盘中:
                        lblStatus.Text = "● 开盘中";
                        lblStatus.ForeColor = Color.FromArgb(76, 175, 80);
                        break;
                    case BinggoLotteryStatus.封盘中:
                        lblStatus.Text = "● 封盘中";
                        lblStatus.ForeColor = Color.FromArgb(244, 67, 54);
                        break;
                    case BinggoLotteryStatus.开奖中:
                        lblStatus.Text = "● 开奖中";
                        lblStatus.ForeColor = Color.FromArgb(156, 39, 176);
                        break;
                    case BinggoLotteryStatus.即将封盘:
                        lblStatus.Text = "● 即将封盘";
                        lblStatus.ForeColor = Color.FromArgb(255, 152, 0);
                        break;
                    default:
                        lblStatus.Text = "● 未开始";
                        lblStatus.ForeColor = Color.Gray;
                        break;
                }
            });
        }
        
        private void OnIssueChanged(object? sender, BinggoIssueChangedEventArgs e)
        {
            Console.WriteLine($"📢 UcBinggoDataCur 收到期号变更事件: {e.OldIssueId} → {e.NewIssueId}");
            
            // 🔥 立即显示新期号和开奖时间（参考 F5BotV2）
            if (_lotteryService != null)
            {
                UpdateUIThreadSafe(() =>
                {
                    lblCurrentIssue.Text = e.NewIssueId.ToString();
                    
                    // 计算并显示开奖时间
                    var openTime = BinggoTimeHelper.GetIssueOpenTime(e.NewIssueId);
                    lblOpenTime.Text = openTime.ToString("HH:mm:ss");
                    
                    Console.WriteLine($"✅ UcBinggoDataCur 已更新: 期号={e.NewIssueId}, 时间={openTime:HH:mm:ss}");
                });
            }
        }
        
        private void OnStatusChanged(object? sender, BinggoStatusChangedEventArgs e)
        {
            UpdateDisplay();
        }
        
        private void OnCountdownTick(object? sender, BinggoCountdownEventArgs e)
        {
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
                _lotteryService.IssueChanged -= OnIssueChanged;
                _lotteryService.StatusChanged -= OnStatusChanged;
                _lotteryService.CountdownTick -= OnCountdownTick;
            }
            
            base.Dispose(disposing);
        }
        
        #region 设计器生成的字段
        
        private Label lblCurrentIssue = null!;
        private Label lblOpenTime = null!;
        private Label lblCountdown = null!;
        private Label lblStatus = null!;
        
        #endregion
    }
}
