using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaiShengVx3Plus.Services;
using Sunny.UI;

namespace BaiShengVx3Plus.Views
{
    /// <summary>
    /// 微信版本检测和自动安装对话框
    /// </summary>
    public partial class WeChatVersionDialog : UIForm
    {
        private readonly string _currentVersion;
        private readonly string _requiredVersion;
        private CancellationTokenSource? _cts;
        
        public bool InstallationSuccess { get; private set; }
        
        public WeChatVersionDialog(string currentVersion, string requiredVersion)
        {
            _currentVersion = currentVersion;
            _requiredVersion = requiredVersion;
            
            InitializeComponent();
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // 设置对话框样式
            this.Text = "微信版本检测";
            this.Width = 550;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            
            // 设置主题色（使用 Sunny.UI 的蓝色）
            this.Style = UIStyle.Blue;
            this.StyleCustomMode = true;
            
            // 图标标签（大号警告图标）
            var lblIcon = new Label
            {
                Text = "⚠️",
                Font = new Font("Segoe UI Emoji", 36, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 155, 40),
                Location = new Point(40, 60),
                Size = new Size(80, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblIcon);
            
            // 标题标签
            var lblTitle = new UILabel
            {
                Text = "检测到微信版本不匹配",
                Font = new Font("微软雅黑", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(48, 48, 48),
                Location = new Point(140, 70),
                Size = new Size(350, 35),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblTitle);
            
            // 当前版本标签
            var lblCurrentLabel = new UILabel
            {
                Text = "当前版本:",
                Font = new Font("微软雅黑", 10),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(140, 115),
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblCurrentLabel);
            
            var lblCurrent = new UILabel
            {
                Text = _currentVersion,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 50, 50),
                Location = new Point(230, 115),
                Size = new Size(250, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblCurrent);
            
            // 需要版本标签
            var lblRequiredLabel = new UILabel
            {
                Text = "需要版本:",
                Font = new Font("微软雅黑", 10),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(140, 145),
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblRequiredLabel);
            
            var lblRequired = new UILabel
            {
                Text = _requiredVersion,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 180, 99),
                Location = new Point(230, 145),
                Size = new Size(250, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblRequired);
            
            // 说明文本
            var lblDescription = new UIRichTextBox
            {
                Text = "本程序仅支持微信 4.1.0.21 版本。\n" +
                       "您可以点击下方的"自动安装"按钮，程序将自动安装正确的版本。\n\n" +
                       "如果没有安装程序，请联系管理员获取。",
                Location = new Point(40, 185),
                Size = new Size(470, 80),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(245, 247, 250),
                Font = new Font("微软雅黑", 9),
                FillColor = Color.FromArgb(245, 247, 250),
                ScrollBarStyleInherited = false
            };
            this.Controls.Add(lblDescription);
            
            // 进度条（初始隐藏）
            var progressBar = new UIProgressBar
            {
                Location = new Point(40, 280),
                Size = new Size(470, 25),
                Visible = false,
                Style = UIStyle.Blue
            };
            this.Controls.Add(progressBar);
            
            // 日志文本框（初始隐藏）
            var txtLog = new UIRichTextBox
            {
                Location = new Point(40, 280),
                Size = new Size(470, 60),
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Visible = false,
                Font = new Font("Consolas", 9),
                FillColor = Color.FromArgb(250, 250, 250)
            };
            this.Controls.Add(txtLog);
            
            // 按钮面板
            var pnlButtons = new Panel
            {
                Location = new Point(0, 310),
                Size = new Size(550, 60),
                BackColor = Color.Transparent
            };
            this.Controls.Add(pnlButtons);
            
            // 自动安装按钮
            var btnInstall = new UIButton
            {
                Text = "🚀 自动安装",
                Size = new Size(140, 40),
                Location = new Point(150, 10),
                Style = UIStyle.Blue,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                Radius = 5,
                Enabled = WeChatVersionChecker.InstallerExists()
            };
            
            if (!btnInstall.Enabled)
            {
                btnInstall.Text = "❌ 安装程序不存在";
                btnInstall.ForeColor = Color.Gray;
            }
            
            pnlButtons.Controls.Add(btnInstall);
            
            // 退出按钮
            var btnExit = new UIButton
            {
                Text = "退出程序",
                Size = new Size(140, 40),
                Location = new Point(310, 10),
                Style = UIStyle.Gray,
                Font = new Font("微软雅黑", 10),
                Radius = 5
            };
            pnlButtons.Controls.Add(btnExit);
            
            // 事件处理
            btnInstall.Click += async (s, e) =>
            {
                await StartInstallationAsync(btnInstall, btnExit, progressBar, txtLog);
            };
            
            btnExit.Click += (s, e) =>
            {
                _cts?.Cancel();
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            
            // 窗口关闭事件
            this.FormClosing += (s, e) =>
            {
                if (!InstallationSuccess && e.CloseReason == CloseReason.UserClosing)
                {
                    var result = UIMessageBox.Show(
                        "微信版本不匹配，退出将无法使用本程序。\n\n是否确定退出？",
                        "确认退出",
                        UIStyle.Blue,
                        UIMessageBoxButtons.OKCancel);
                    
                    if (result != DialogResult.OK)
                    {
                        e.Cancel = true;
                    }
                }
            };
        }
        
        private async Task StartInstallationAsync(UIButton btnInstall, UIButton btnExit, UIProgressBar progressBar, UIRichTextBox txtLog)
        {
            try
            {
                // 禁用按钮
                btnInstall.Enabled = false;
                btnExit.Enabled = false;
                
                // 显示进度条和日志
                progressBar.Visible = true;
                progressBar.Value = 10;
                txtLog.Visible = true;
                txtLog.Clear();
                
                // 调整布局
                progressBar.Location = new Point(40, 275);
                txtLog.Location = new Point(40, 310);
                txtLog.Height = 40;
                
                _cts = new CancellationTokenSource();
                
                var progress = new Progress<string>(msg =>
                {
                    if (InvokeRequired)
                    {
                        Invoke(() =>
                        {
                            txtLog.AppendText($"{DateTime.Now:HH:mm:ss} {msg}\n");
                            txtLog.ScrollToCaret();
                            
                            // 更新进度条
                            if (msg.Contains("启动安装程序"))
                                progressBar.Value = 30;
                            else if (msg.Contains("等待安装完成"))
                                progressBar.Value = 50;
                            else if (msg.Contains("安装程序已退出"))
                                progressBar.Value = 80;
                            else if (msg.Contains("安装成功"))
                                progressBar.Value = 100;
                        });
                    }
                    else
                    {
                        txtLog.AppendText($"{DateTime.Now:HH:mm:ss} {msg}\n");
                        txtLog.ScrollToCaret();
                    }
                });
                
                // 执行安装
                var success = await WeChatVersionChecker.InstallWeChatAsync(progress, _cts.Token);
                
                if (success)
                {
                    InstallationSuccess = true;
                    progressBar.Value = 100;
                    
                    // 询问是否启动微信
                    var result = UIMessageBox.Show(
                        "微信安装成功！\n\n是否立即启动微信？",
                        "安装成功",
                        UIStyle.Blue,
                        UIMessageBoxButtons.OKCancel);
                    
                    if (result == DialogResult.OK)
                    {
                        await WeChatVersionChecker.LaunchWeChatAsync(progress);
                        await Task.Delay(2000);
                    }
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    progressBar.Value = 0;
                    btnExit.Enabled = true;
                    
                    UIMessageBox.ShowWarning(
                        "安装未完成或失败。\n\n请检查日志信息，或手动安装微信 4.1.0.21 后重启本程序。",
                        "安装失败");
                }
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError($"安装过程出错:\n{ex.Message}", "错误");
                btnExit.Enabled = true;
            }
        }
        
        // InitializeComponent 由 Designer 生成或手动实现
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}

