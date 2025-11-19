using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Utils;
using BaiShengVx3Plus.Models.Config;

namespace BaiShengVx3Plus.Services.Sound
{
    /// <summary>
    /// 声音播放服务（参考 F5BotV2）
    /// 播放时机：
    /// 1. 封盘时播放 mp3_fp.mp3
    /// 2. 开奖时播放 mp3_kj.mp3
    /// 3. 上分时播放 mp3_shang.mp3
    /// 4. 下分时播放 mp3_xia.mp3
    /// 
    /// 🔥 关键修复：MCI API 需要在 UI 线程中调用，否则可能播放不完整
    /// </summary>
    public class SoundService
    {
        private readonly ILogService _logService;
        private readonly string _soundDirectory;
        
        // 🔥 关键修复：保持 MP3Play 对象的引用列表，防止被垃圾回收（参考 F5BotV2 实际运行机制）
        // MCI 的 play 命令是异步的，如果对象被回收，MCI 会自动关闭，导致声音只播放开头
        // 注意：F5BotV2 每次都创建新对象，不调用 StopT()（StopT() 会关闭 MCI 设备）
        // 我们使用列表保存最近的播放器对象，避免被 GC 回收
        private readonly System.Collections.Generic.List<MP3Play> _recentPlayers = new();
        
        // 🔊 声音设置
        private SoundSettings? _soundSettings;
        
        // 🔥 UI 线程同步上下文（用于将声音播放切换到 UI 线程）
        private SynchronizationContext? _uiContext;
        
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
        /// 🔥 设置 UI 线程同步上下文（应在主窗口创建后调用）
        /// </summary>
        public void SetUIContext(SynchronizationContext uiContext)
        {
            _uiContext = uiContext;
            _logService.Info("SoundService", $"✅ UI 线程同步上下文已设置: {uiContext?.GetType().Name ?? "null"}");
        }
        
        /// <summary>
        /// 设置声音配置
        /// </summary>
        public void SetSoundSettings(SoundSettings? settings)
        {
            _soundSettings = settings;
            _logService.Info("SoundService", $"声音设置已更新: {(settings?.EnableSound == true ? "已启用" : "已禁用")}");
        }
        
        /// <summary>
        /// 播放 MP3 文件（参考 F5BotV2 第 2550-2555 行）
        /// 🔥 关键修复1：保持 MP3Play 对象引用，防止被垃圾回收导致声音中断
        /// 🔥 关键修复2：MCI API 需要在 UI 线程中调用，否则可能播放不完整
        /// </summary>
        /// <param name="fileName">文件名（如：mp3_fp.mp3）</param>
        /// <param name="volume">音量 (0-100)，注意：MCI 音量范围是 0-1000</param>
        public void PlayMp3(string fileName, int volume = 100)
        {
            // 🔥 如果声音未启用，直接返回
            if (_soundSettings != null && !_soundSettings.EnableSound)
            {
                _logService.Debug("SoundService", $"声音已禁用，跳过播放: {fileName}");
                return;
            }
            
            // 🔥 关键修复：如果设置了 UI 上下文，切换到 UI 线程播放
            // MCI API 在某些情况下需要在有消息泵的线程中调用，否则可能播放不完整
            if (_uiContext != null)
            {
                _logService.Info("SoundService", $"🔊 切换到 UI 线程播放声音: {fileName}");
                _uiContext.Post(_ => PlayMp3Internal(fileName, volume), null);
                return;
            }
            
            // 如果没有设置 UI 上下文，直接在当前线程播放（兼容旧代码）
            _logService.Debug("SoundService", $"🔊 在当前线程播放声音: {fileName}（未设置 UI 上下文）");
            PlayMp3Internal(fileName, volume);
        }
        
        /// <summary>
        /// 内部播放方法（实际执行播放逻辑）
        /// </summary>
        private void PlayMp3Internal(string fileName, int volume)
        {
            try
            {
                string filePath = Path.Combine(_soundDirectory, fileName);
                
                _logService.Info("SoundService", $"🔊 准备播放声音: {fileName}");
                _logService.Info("SoundService", $"   完整路径: {filePath}");
                _logService.Info("SoundService", $"   文件存在: {File.Exists(filePath)}");
                _logService.Info("SoundService", $"   当前线程: {Thread.CurrentThread.ManagedThreadId} (IsUIThread: {SynchronizationContext.Current != null})");
                
                if (!File.Exists(filePath))
                {
                    _logService.Warning("SoundService", $"⚠️ 声音文件不存在: {filePath}");
                    return;
                }
                
                // 🔥 关键修复：每次都创建新的 MP3Play 对象（完全参考 F5BotV2 第 2552 行）
                // 不调用 StopT()！因为 StopT() 会调用 "close media"，关闭 MCI 设备
                // MCI 的 "play media" 命令是异步的，需要保持对象引用直到播放完成
                var player = new MP3Play();
                
                _logService.Info("SoundService", $"   1. MP3Play 对象已创建");
                
                player.FileName = filePath;
                
                _logService.Info("SoundService", $"   2. FileName 已设置");
                _logService.Info("SoundService", $"   3. 播放状态: {player.mc.state}");
                
                // 🔥 设置音量（0-100）
                player.SetVolume(volume);
                _logService.Info("SoundService", $"   4. SetVolume({volume}) 已调用");
                
                player.play();
                
                _logService.Info("SoundService", $"   5. play() 已调用");
                _logService.Info("SoundService", $"   6. 播放状态: {player.mc.state}");
                
                // 🔥 保存到列表，防止被垃圾回收（保留最近 10 个播放器对象）
                _recentPlayers.Add(player);
                if (_recentPlayers.Count > 10)
                {
                    _recentPlayers.RemoveAt(0);  // 移除最早的
                }
                
                _logService.Info("SoundService", $"   7. 对象已保存到列表（总数: {_recentPlayers.Count}）");
                _logService.Info("SoundService", $"✅ 播放声音完成: {fileName}, 音量: {volume}%");
                
                // 🔥 关键修复：异步等待一段时间后移除旧的播放器对象
                // MCI 播放是异步的，需要保持对象引用直到播放完成
                // 根据文件时长，等待足够的时间后再允许对象被回收
                try
                {
                    int duration = player.Duration;  // 获取总时长（秒）
                    if (duration > 0 && duration < 60)  // 合理的时长范围
                    {
                        _logService.Info("SoundService", $"   8. 声音时长: {duration} 秒");
                        
                        // 🔥 在后台线程中等待播放完成后再清理（不阻塞当前线程）
                        var playerToKeep = player;
                        System.Threading.Tasks.Task.Run(async () =>
                        {
                            await System.Threading.Tasks.Task.Delay((duration + 1) * 1000);  // 多等待1秒，确保播放完成
                            
                            // 播放完成后，允许从列表中移除（但不主动移除，让新声音自动挤出去）
                            _logService.Info("SoundService", $"   ✅ [{fileName}] 播放完成（{duration}秒）");
                        });
                    }
                }
                catch (Exception durationEx)
                {
                    _logService.Warning("SoundService", $"获取声音时长失败: {durationEx.Message}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("SoundService", $"播放声音失败: {fileName}", ex);
            }
        }
        
        /// <summary>
        /// 播放封盘声音
        /// </summary>
        public void PlaySealingSound()
        {
            string fileName = _soundSettings?.SealingSound ?? "mp3_fp.mp3";
            int volume = _soundSettings?.SealingVolume ?? 100;
            PlayMp3(fileName, volume);
        }
        
        /// <summary>
        /// 播放开奖声音
        /// </summary>
        public void PlayLotterySound()
        {
            string fileName = _soundSettings?.LotterySound ?? "mp3_kj.mp3";
            int volume = _soundSettings?.LotteryVolume ?? 100;
            PlayMp3(fileName, volume);
        }
        
        /// <summary>
        /// 播放上分声音
        /// </summary>
        public void PlayCreditUpSound()
        {
            string fileName = _soundSettings?.CreditUpSound ?? "mp3_shang.mp3";
            int volume = _soundSettings?.CreditUpVolume ?? 100;
            PlayMp3(fileName, volume);
        }
        
        /// <summary>
        /// 播放下分声音
        /// </summary>
        public void PlayCreditDownSound()
        {
            string fileName = _soundSettings?.CreditDownSound ?? "mp3_xia.mp3";
            int volume = _soundSettings?.CreditDownVolume ?? 100;
            PlayMp3(fileName, volume);
        }
        
        /// <summary>
        /// 播放指定的声音文件（用于测试）
        /// </summary>
        /// <param name="fileName">文件名（相对路径）</param>
        /// <param name="volume">音量 (0-100)</param>
        public void PlayTestSound(string fileName, int volume = 100)
        {
            PlayMp3(fileName, volume);
        }
    }
}

