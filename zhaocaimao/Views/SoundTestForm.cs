using System;
using System.Windows.Forms;
using zhaocaimao.Services.Sound;
using zhaocaimao.Contracts;
using Sunny.UI;

namespace zhaocaimao.Views
{
    /// <summary>
    /// 声音测试窗口
    /// </summary>
    public partial class SoundTestForm : UIForm
    {
        private readonly SoundService? _soundService;
        private readonly ILogService? _logService;

        public SoundTestForm(SoundService soundService, ILogService logService)
        {
            _soundService = soundService;
            _logService = logService;
            
            InitializeComponent();
            
            Text = "🔊 声音测试";
            Width = 400;
            Height = 300;
        }

        private void InitializeComponent()
        {
            var lblTitle = new UILabel
            {
                Text = "声音播放测试",
                Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(350, 30),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            var btnTestSealing = new UIButton
            {
                Text = "测试封盘声音",
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(150, 40),
                Font = new System.Drawing.Font("微软雅黑", 10F)
            };
            btnTestSealing.Click += (s, e) => TestSound("封盘", "mp3_fp.mp3");

            var btnTestLottery = new UIButton
            {
                Text = "测试开奖声音",
                Location = new System.Drawing.Point(210, 100),
                Size = new System.Drawing.Size(150, 40),
                Font = new System.Drawing.Font("微软雅黑", 10F)
            };
            btnTestLottery.Click += (s, e) => TestSound("开奖", "mp3_kj.mp3");

            var btnTestCreditUp = new UIButton
            {
                Text = "测试上分声音",
                Location = new System.Drawing.Point(20, 160),
                Size = new System.Drawing.Size(150, 40),
                Font = new System.Drawing.Font("微软雅黑", 10F)
            };
            btnTestCreditUp.Click += (s, e) => TestSound("上分", "mp3_shang.mp3");

            var btnTestCreditDown = new UIButton
            {
                Text = "测试下分声音",
                Location = new System.Drawing.Point(210, 160),
                Size = new System.Drawing.Size(150, 40),
                Font = new System.Drawing.Font("微软雅黑", 10F)
            };
            btnTestCreditDown.Click += (s, e) => TestSound("下分", "mp3_xia.mp3");

            Controls.Add(lblTitle);
            Controls.Add(btnTestSealing);
            Controls.Add(btnTestLottery);
            Controls.Add(btnTestCreditUp);
            Controls.Add(btnTestCreditDown);
        }

        private void TestSound(string name, string fileName)
        {
            try
            {
                _logService?.Info("SoundTest", $"========== 开始测试 {name} 声音 ==========");
                _logService?.Info("SoundTest", $"文件: {fileName}");
                
                if (_soundService == null)
                {
                    UIMessageBox.ShowError("SoundService 未初始化！");
                    return;
                }

                _soundService.PlayTestSound(fileName, 100);
                
                UIMessageTip.ShowOk($"正在播放: {name}");
                _logService?.Info("SoundTest", $"========== {name} 声音测试完成 ==========");
            }
            catch (Exception ex)
            {
                _logService?.Error("SoundTest", $"测试 {name} 声音失败", ex);
                UIMessageBox.ShowError($"测试失败:\n{ex.Message}");
            }
        }
    }
}

