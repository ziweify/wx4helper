using System;
using System.IO;
using System.Windows.Forms;
using zhaocaimao.Models.Config;
using zhaocaimao.Services.Sound;
using zhaocaimao.Contracts;
using Sunny.UI;

namespace zhaocaimao.Views.Controls
{
    /// <summary>
    /// 声音设置面板
    /// </summary>
    public partial class SoundSettingsPanel : UserControl
    {
        private readonly SoundService? _soundService;
        private readonly ILogService? _logService;
        private SoundSettings? _soundSettings;

        public SoundSettingsPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 构造函数（用于运行时注入服务）
        /// </summary>
        public SoundSettingsPanel(SoundService soundService, ILogService logService, SoundSettings soundSettings) : this()
        {
            _soundService = soundService;
            _logService = logService;
            _soundSettings = soundSettings;
            
            LoadSettings();
        }

        /// <summary>
        /// 加载设置到 UI
        /// </summary>
        private void LoadSettings()
        {
            if (_soundSettings == null) return;

            chkEnableSound.Checked = _soundSettings.EnableSound;
            
            txtSealingSound.Text = _soundSettings.SealingSound;
            txtLotterySound.Text = _soundSettings.LotterySound;
            txtCreditUpSound.Text = _soundSettings.CreditUpSound;
            txtCreditDownSound.Text = _soundSettings.CreditDownSound;
            
            trbSealingVolume.Value = _soundSettings.SealingVolume;
            trbLotteryVolume.Value = _soundSettings.LotteryVolume;
            trbCreditUpVolume.Value = _soundSettings.CreditUpVolume;
            trbCreditDownVolume.Value = _soundSettings.CreditDownVolume;
            
            UpdateVolumeLabel(trbSealingVolume, lblSealingVolume);
            UpdateVolumeLabel(trbLotteryVolume, lblLotteryVolume);
            UpdateVolumeLabel(trbCreditUpVolume, lblCreditUpVolume);
            UpdateVolumeLabel(trbCreditDownVolume, lblCreditDownVolume);
        }

        /// <summary>
        /// 保存 UI 设置到模型
        /// </summary>
        public void SaveSettings()
        {
            if (_soundSettings == null) return;

            _soundSettings.EnableSound = chkEnableSound.Checked;
            
            _soundSettings.SealingSound = txtSealingSound.Text.Trim();
            _soundSettings.LotterySound = txtLotterySound.Text.Trim();
            _soundSettings.CreditUpSound = txtCreditUpSound.Text.Trim();
            _soundSettings.CreditDownSound = txtCreditDownSound.Text.Trim();
            
            _soundSettings.SealingVolume = trbSealingVolume.Value;
            _soundSettings.LotteryVolume = trbLotteryVolume.Value;
            _soundSettings.CreditUpVolume = trbCreditUpVolume.Value;
            _soundSettings.CreditDownVolume = trbCreditDownVolume.Value;
            
            // 更新 SoundService 的设置
            _soundService?.SetSoundSettings(_soundSettings);
            
            _logService?.Info("SoundSettings", "声音设置已保存");
        }

        /// <summary>
        /// 浏览声音文件
        /// </summary>
        private void BrowseSoundFile(UITextBox targetTextBox)
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    // 🔥 默认打开当前程序运行目录下的 sound 文件夹
                    string soundDir = Path.Combine(Application.StartupPath, "sound");
                    if (Directory.Exists(soundDir))
                    {
                        openFileDialog.InitialDirectory = soundDir;
                    }
                    else
                    {
                        openFileDialog.InitialDirectory = Application.StartupPath;
                    }
                    
                    openFileDialog.Filter = "MP3 文件 (*.mp3)|*.mp3|所有文件 (*.*)|*.*";
                    openFileDialog.Title = "选择声音文件";
                    
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFile = openFileDialog.FileName;
                        
                        // 🔥 转换为相对路径（相对于 sound 文件夹）
                        string relativePath = GetRelativePath(soundDir, selectedFile);
                        
                        targetTextBox.Text = relativePath;
                        
                        _logService?.Info("SoundSettings", $"选择声音文件: {relativePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logService?.Error("SoundSettings", $"浏览声音文件失败", ex);
                UIMessageBox.ShowError($"浏览文件失败:\n{ex.Message}");
            }
        }

        /// <summary>
        /// 获取相对路径（相对于 sound 文件夹）
        /// </summary>
        private string GetRelativePath(string soundDir, string fullPath)
        {
            try
            {
                // 如果文件在 sound 文件夹内，返回相对路径
                if (fullPath.StartsWith(soundDir, StringComparison.OrdinalIgnoreCase))
                {
                    string relativePath = fullPath.Substring(soundDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    return relativePath;
                }
                
                // 否则返回文件名
                return Path.GetFileName(fullPath);
            }
            catch
            {
                return Path.GetFileName(fullPath);
            }
        }

        /// <summary>
        /// 测试按钮点击事件
        /// </summary>
        private void BtnTest_Click(object? sender, EventArgs e)
        {
            try
            {
                if (sender is not UIButton btn) return;
                if (btn.Tag is not UITextBox textBox) return;

                string fileName = textBox.Text.Trim();
                if (string.IsNullOrEmpty(fileName))
                {
                    UIMessageBox.ShowWarning("请先输入声音文件路径！");
                    return;
                }

                // 获取对应的音量 TrackBar
                int volume = 100;
                if (btn == btnTestSealing)
                    volume = trbSealingVolume.Value;
                else if (btn == btnTestLottery)
                    volume = trbLotteryVolume.Value;
                else if (btn == btnTestCreditUp)
                    volume = trbCreditUpVolume.Value;
                else if (btn == btnTestCreditDown)
                    volume = trbCreditDownVolume.Value;

                _logService?.Info("SoundSettings", $"测试播放: {fileName}, 音量: {volume}%");

                // 🔥 播放声音
                if (_soundService != null)
                {
                    _soundService.PlayTestSound(fileName, volume);
                    UIMessageTip.ShowOk($"正在播放: {fileName}");
                }
                else
                {
                    UIMessageBox.ShowWarning("声音服务未初始化！");
                }
            }
            catch (Exception ex)
            {
                _logService?.Error("SoundSettings", $"测试播放失败", ex);
                UIMessageBox.ShowError($"播放失败:\n{ex.Message}");
            }
        }

        /// <summary>
        /// 更新音量标签
        /// </summary>
        private void UpdateVolumeLabel(UITrackBar trackBar, UILabel label)
        {
            label.Text = $"{trackBar.Value}%";
        }
    }
}

