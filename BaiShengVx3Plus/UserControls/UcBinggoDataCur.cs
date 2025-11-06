using Sunny.UI;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models.Games.Binggo;
using BaiShengVx3Plus.Models.Games.Binggo.Events;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BaiShengVx3Plus.UserControls
{
    /// <summary>
    /// 当前期开奖数据显示控件
    /// 
    /// 功能：
    /// - 显示当前期号
    /// - 显示距封盘倒计时
    /// - 显示当前状态（开盘/封盘/开奖）
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
            // 设置控件大小和样式
            this.Size = new Size(239, 140);
            this.BackColor = Color.FromArgb(243, 249, 255);
            
            // 标题标签
            var lblTitle = new UILabel
            {
                Text = "当前期数据",
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(48, 48, 48),
                Location = new Point(10, 10),
                Size = new Size(219, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);
            
            // 期号标签
            lblCurrentIssue = new UILabel
            {
                Text = "期号: -",
                Font = new Font("微软雅黑", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(48, 48, 48),
                Location = new Point(10, 45),
                Size = new Size(219, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblCurrentIssue);
            
            // 倒计时标签（大字体，醒目）
            lblCountdown = new UILabel
            {
                Text = "00:00",
                Font = new Font("微软雅黑", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 136),
                Location = new Point(10, 75),
                Size = new Size(219, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblCountdown);
            
            // 状态标签
            lblStatus = new UILabel
            {
                Text = "未开始",
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.Gray,
                Location = new Point(10, 115),
                Size = new Size(219, 20),
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
                lblCurrentIssue.Text = _lotteryService.CurrentIssueId > 0 
                    ? $"期号: {_lotteryService.CurrentIssueId}" 
                    : "期号: -";
                
                // 更新倒计时
                int seconds = _lotteryService.SecondsToSeal;
                int minutes = seconds / 60;
                int secs = seconds % 60;
                lblCountdown.Text = $"{minutes:D2}:{secs:D2}";
                
                // 根据倒计时调整颜色
                if (seconds <= 10)
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
                    default:
                        lblStatus.Text = "● 未开始";
                        lblStatus.ForeColor = Color.Gray;
                        break;
                }
            });
        }
        
        private void OnIssueChanged(object? sender, BinggoIssueChangedEventArgs e)
        {
            UpdateDisplay();
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
        
        private UILabel lblCurrentIssue = null!;
        private UILabel lblCountdown = null!;
        private UILabel lblStatus = null!;
        
        #endregion
    }
}

