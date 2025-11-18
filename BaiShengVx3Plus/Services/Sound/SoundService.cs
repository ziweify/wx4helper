using System;
using System.IO;
using System.Windows.Forms;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Utils;

namespace BaiShengVx3Plus.Services.Sound
{
    /// <summary>
    /// 声音播放服务（参考 F5BotV2）
    /// 播放时机：
    /// 1. 封盘时播放 mp3_fp.mp3
    /// 2. 开奖时播放 mp3_kj.mp3
    /// 3. 上分时播放 mp3_shang.mp3
    /// 4. 下分时播放 mp3_xia.mp3
    /// </summary>
    public class SoundService
    {
        private readonly ILogService _logService;
        private readonly string _soundDirectory;
        
        public SoundService(ILogService logService)
        {
            _logService = logService;
            // 🔥 声音文件目录：EXE 所在目录下的 sound 文件夹（参考 F5BotV2）
            // 使用 Application.StartupPath 获取 EXE 所在的绝对路径
            _soundDirectory = Path.Combine(Application.StartupPath, "sound");
            
            // 确保目录存在
            if (!Directory.Exists(_soundDirectory))
            {
                Directory.CreateDirectory(_soundDirectory);
                _logService.Info("SoundService", $"✅ 创建声音文件目录: {_soundDirectory}");
            }
            else
            {
                _logService.Info("SoundService", $"✅ 声音文件目录: {_soundDirectory}");
            }
        }
        
        /// <summary>
        /// 播放 MP3 文件（参考 F5BotV2 第 2550-2555 行）
        /// </summary>
        /// <param name="fileName">文件名（如：mp3_fp.mp3）</param>
        public void PlayMp3(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_soundDirectory, fileName);
                
                if (!File.Exists(filePath))
                {
                    _logService.Warning("SoundService", $"⚠️ 声音文件不存在: {filePath}");
                    return;
                }
                
                // 🔥 参考 F5BotV2 第 2550-2555 行：播放一次性，不等待
                MP3Play player = new MP3Play();
                player.FileName = filePath;
                player.play();
                
                _logService.Info("SoundService", $"🔊 播放声音: {fileName}");
            }
            catch (Exception ex)
            {
                _logService.Error("SoundService", $"播放声音失败: {fileName}", ex);
            }
        }
        
        /// <summary>
        /// 播放封盘声音
        /// </summary>
        public void PlaySealingSound() => PlayMp3("mp3_fp.mp3");
        
        /// <summary>
        /// 播放开奖声音
        /// </summary>
        public void PlayLotterySound() => PlayMp3("mp3_kj.mp3");
        
        /// <summary>
        /// 播放上分声音
        /// </summary>
        public void PlayCreditUpSound() => PlayMp3("mp3_shang.mp3");
        
        /// <summary>
        /// 播放下分声音
        /// </summary>
        public void PlayCreditDownSound() => PlayMp3("mp3_xia.mp3");
    }
}

