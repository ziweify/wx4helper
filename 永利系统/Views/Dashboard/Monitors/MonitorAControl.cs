using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Unit.Browser.Controls;
using Unit.Browser.Models;
using 永利系统.Infrastructure;

namespace 永利系统.Views.Dashboard.Monitors
{
    /// <summary>
    /// 监控A - 示例浏览器监控控件
    /// </summary>
    public partial class MonitorAControl : XtraUserControl
    {
        private readonly LoggingService _loggingService;
        private BrowserWindowProxy? _browserProxy;
        private CancellationTokenSource? _monitoringCts;
        
        // UI Controls
        private GroupControl? grpControl;
        private SimpleButton? btnStart;
        private SimpleButton? btnStop;
        private SimpleButton? btnShowBrowser;
        private SimpleButton? btnTestCommand;
        private TextEdit? txtUrl;
        private LabelControl? lblUrl;
        private LabelControl? lblStatus;
        private MemoEdit? memoLog;

        public MonitorAControl()
        {
            _loggingService = LoggingService.Instance;
            
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            
            // 创建主容器
            grpControl = new GroupControl
            {
                Text = "监控A - 台湾彩票",
                Dock = DockStyle.Fill
            };
            this.Controls.Add(grpControl);

            // URL配置
            lblUrl = new LabelControl
            {
                Text = "监控URL:",
                Location = new System.Drawing.Point(20, 30)
            };
            grpControl.Controls.Add(lblUrl);

            txtUrl = new TextEdit
            {
                Location = new System.Drawing.Point(100, 28),
                Size = new System.Drawing.Size(500, 20),
                EditValue = "https://www.taiwanlottery.com.tw/lotto/BingoBingo/OEHLStatistic.htm"
            };
            grpControl.Controls.Add(txtUrl);

            // 控制按钮
            btnStart = new SimpleButton
            {
                Text = "启动监控",
                Location = new System.Drawing.Point(20, 70),
                Size = new System.Drawing.Size(100, 30)
            };
            btnStart.Click += BtnStart_Click;
            grpControl.Controls.Add(btnStart);

            btnStop = new SimpleButton
            {
                Text = "停止监控",
                Location = new System.Drawing.Point(130, 70),
                Size = new System.Drawing.Size(100, 30),
                Enabled = false
            };
            btnStop.Click += BtnStop_Click;
            grpControl.Controls.Add(btnStop);

            btnShowBrowser = new SimpleButton
            {
                Text = "显示浏览器",
                Location = new System.Drawing.Point(240, 70),
                Size = new System.Drawing.Size(100, 30),
                Enabled = false
            };
            btnShowBrowser.Click += BtnShowBrowser_Click;
            grpControl.Controls.Add(btnShowBrowser);

            btnTestCommand = new SimpleButton
            {
                Text = "测试命令",
                Location = new System.Drawing.Point(350, 70),
                Size = new System.Drawing.Size(100, 30),
                Enabled = false
            };
            btnTestCommand.Click += async (s, e) => await TestCommandsAsync();
            grpControl.Controls.Add(btnTestCommand);

            // 状态标签
            lblStatus = new LabelControl
            {
                Text = "状态: 未启动",
                Location = new System.Drawing.Point(20, 110),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new System.Drawing.Size(600, 20)
            };
            grpControl.Controls.Add(lblStatus);

            // 日志区域
            memoLog = new MemoEdit
            {
                Location = new System.Drawing.Point(20, 140),
                Size = new System.Drawing.Size(600, 400),
                Properties = { ReadOnly = true, ScrollBars = ScrollBars.Vertical }
            };
            grpControl.Controls.Add(memoLog);
        }

        private async void BtnStart_Click(object? sender, EventArgs e)
        {
            try
            {
                LogMessage("🚀 正在启动监控A...");
                UpdateStatus("初始化中...");

                // 创建浏览器代理
                _browserProxy = new BrowserWindowProxy();
                _browserProxy.OnLog += (s, msg) => LogMessage($"[浏览器] {msg}");

                // 初始化浏览器
                var url = txtUrl?.EditValue?.ToString() ?? "";
                await _browserProxy.InitializeAsync("监控A - 台湾彩票", url);

                // 显示浏览器窗口
                _browserProxy.ShowWindow();

                // 启动监控循环
                _monitoringCts = new CancellationTokenSource();
                _ = StartMonitoringLoopAsync(_monitoringCts.Token);

                // 更新UI状态
                btnStart!.Enabled = false;
                btnStop!.Enabled = true;
                btnShowBrowser!.Enabled = true;
                btnTestCommand!.Enabled = true;
                txtUrl!.Enabled = false;

                UpdateStatus("监控中...");
                LogMessage("✅ 监控A已启动");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 启动失败: {ex.Message}");
                UpdateStatus("启动失败");
                XtraMessageBox.Show($"启动失败:\n{ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            try
            {
                LogMessage("🛑 正在停止监控A...");

                // 停止监控循环
                _monitoringCts?.Cancel();
                _monitoringCts?.Dispose();
                _monitoringCts = null;

                // 关闭浏览器
                _browserProxy?.CloseWindow();
                _browserProxy?.Dispose();
                _browserProxy = null;

                // 更新UI状态
                btnStart!.Enabled = true;
                btnStop!.Enabled = false;
                btnShowBrowser!.Enabled = false;
                btnTestCommand!.Enabled = false;
                txtUrl!.Enabled = true;

                UpdateStatus("已停止");
                LogMessage("✅ 监控A已停止");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 停止失败: {ex.Message}");
            }
        }

        private void BtnShowBrowser_Click(object? sender, EventArgs e)
        {
            if (_browserProxy != null)
            {
                _browserProxy.ShowWindow();
                LogMessage("📺 浏览器窗口已显示");
            }
        }

        private async Task StartMonitoringLoopAsync(CancellationToken cancellationToken)
        {
            LogMessage("🔄 监控循环已启动");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // 获取当前期号
                        var issueResult = await _browserProxy!.ExecuteCommandAsync("获取期号");
                        if (issueResult.Success && issueResult.Data != null)
                        {
                            LogMessage($"📊 当前期号: {issueResult.Data}");
                        }

                        // 执行监控逻辑（示例）
                        var script = @"
                            (function() {
                                try {
                                    var issueEl = document.querySelector('#right_overflow_hinet > div');
                                    if (issueEl) {
                                        return {
                                            success: true,
                                            text: issueEl.innerText.substring(0, 100)
                                        };
                                    }
                                    return { success: false, message: '未找到期号元素' };
                                } catch(e) {
                                    return { success: false, message: e.message };
                                }
                            })();
                        ";

                        var scriptResult = await _browserProxy!.ExecuteCommandAsync("执行脚本", script);
                        if (scriptResult.Success)
                        {
                            LogMessage($"✅ 监控数据: {scriptResult.Data}");
                        }

                        // 等待10秒后再次监控
                        await Task.Delay(10000, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"⚠️ 监控异常: {ex.Message}");
                        await Task.Delay(5000, cancellationToken); // 错误后等待5秒
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 监控循环异常: {ex.Message}");
            }

            LogMessage("🔚 监控循环已停止");
        }

        private async Task TestCommandsAsync()
        {
            if (_browserProxy == null) return;

            try
            {
                LogMessage("🧪 开始测试命令...");

                // 测试1: 获取当前网址
                var urlResult = await _browserProxy.ExecuteCommandAsync("当前网址");
                LogMessage($"✅ 当前网址: {urlResult.Data}");

                // 测试2: 获取页面标题
                var titleResult = await _browserProxy.ExecuteCommandAsync("获取标题");
                LogMessage($"✅ 页面标题: {titleResult.Data}");

                // 测试3: 获取Cookie
                var cookieResult = await _browserProxy.ExecuteCommandAsync("获取Cookie");
                LogMessage($"✅ Cookie数量: {(cookieResult.Data as System.Collections.Generic.Dictionary<string, string>)?.Count ?? 0}");

                // 测试4: 执行简单脚本
                var scriptResult = await _browserProxy.ExecuteCommandAsync("执行脚本", 
                    "return document.body.innerText.length;");
                LogMessage($"✅ 页面文本长度: {scriptResult.Data}");

                LogMessage("🎉 命令测试完成");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 命令测试失败: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => LogMessage(message));
                return;
            }

            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            var logLine = $"[{time}] {message}";

            if (memoLog != null)
            {
                memoLog.EditValue = memoLog.EditValue?.ToString() + logLine + Environment.NewLine;
                
                // 自动滚动到底部
                memoLog.SelectionStart = memoLog.Text.Length;
                memoLog.ScrollToCaret();
            }

            _loggingService.Info("监控A", message);
        }

        private void UpdateStatus(string status)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => UpdateStatus(status));
                return;
            }

            if (lblStatus != null)
            {
                lblStatus.Text = $"状态: {status}";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _monitoringCts?.Cancel();
                _monitoringCts?.Dispose();
                _browserProxy?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

