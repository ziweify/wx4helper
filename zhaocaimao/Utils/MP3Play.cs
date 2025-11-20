using System;
using System.Runtime.InteropServices;

namespace zhaocaimao.Utils
{
    /// <summary>
    /// MP3 播放器（完全参考 F5BotV2）
    /// 使用 Windows MCI API 播放 MP3 文件
    /// 🔥 修复：使用唯一 alias 名称，避免多个实例互相干扰
    /// </summary>
    public class MP3Play
    {
        //定义API函数使用的字符串变量 
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        private string Name = "";
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        private string durLength = "";
        [MarshalAs(UnmanagedType.LPTStr, SizeConst = 128)]
        private string TemStr = "";
        int ilong;
        
        // 🔥 使用唯一 alias 名称，避免多个实例互相干扰
        private static int _instanceCounter = 0;
        private readonly string _alias;
        
        //定义播放状态枚举变量
        public enum State
        {
            mPlaying = 1,
            mPuase = 2,
            mStop = 3
        };
        
        //结构变量
        public struct structMCI
        {
            public bool bMut;
            public int iDur;
            public int iPos;
            public int iVol;
            public int iBal;
            public string iName;
            public State state;
        };
        public structMCI mc = new structMCI();
        
        /// <summary>
        /// 构造函数：生成唯一的 alias 名称
        /// </summary>
        public MP3Play()
        {
            _alias = $"media{System.Threading.Interlocked.Increment(ref _instanceCounter)}";
            System.Diagnostics.Debug.WriteLine($"[MP3Play] Created with alias: {_alias}");
        }
        
        //取得播放文件属性（完全参考 F5BotV2）
        public string FileName
        {
            get
            {
                return mc.iName;
            }
            set
            {
                try
                {
                    TemStr = "";
                    TemStr = TemStr.PadLeft(127, Convert.ToChar(" "));
                    Name = Name.PadLeft(260, Convert.ToChar(" "));
                    mc.iName = value;
                    ilong = APIClass.GetShortPathName(mc.iName, Name, Name.Length);
                    Name = GetCurrPath(Name);
                    Name = $"open {Convert.ToChar(34)}{Name}{Convert.ToChar(34)} alias {_alias}";  // 🔥 使用唯一 alias
                    
                    // 🔥 调试：记录 MCI 命令和返回值
                    // 注意：不再调用 close all，避免关闭其他正在播放的声音
                    System.Diagnostics.Debug.WriteLine($"[MP3Play] [{_alias}] 1. Open command: {Name}");
                    ilong = APIClass.mciSendString(Name, TemStr, TemStr.Length, 0);
                    System.Diagnostics.Debug.WriteLine($"[MP3Play] [{_alias}] 2. Open result: {ilong}");
                    ilong = APIClass.mciSendString($"set {_alias} time format milliseconds", TemStr, TemStr.Length, 0);  // 🔥 使用唯一 alias
                    System.Diagnostics.Debug.WriteLine($"[MP3Play] [{_alias}] 3. Set time format result: {ilong}");
                    
                    mc.state = State.mStop;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MP3Play] ❌ Exception in FileName setter: {ex.Message}");
                }
            }
        }
        
        //播放（完全参考 F5BotV2）
        public void play()
        {
            TemStr = "";
            TemStr = TemStr.PadLeft(127, Convert.ToChar(" "));
            int result = APIClass.mciSendString($"play {_alias}", TemStr, TemStr.Length, 0);  // 🔥 使用唯一 alias
            
            // 🔥 调试：记录播放命令的返回值
            System.Diagnostics.Debug.WriteLine($"[MP3Play] [{_alias}] Play command result: {result}");
            if (result != 0)
            {
                System.Diagnostics.Debug.WriteLine($"[MP3Play] [{_alias}] ❌ Play failed with error code: {result}");
            }
            
            mc.state = State.mPlaying;
        }
        
        /// <summary>
        /// 设置音量 (0-100)
        /// MCI 音量范围是 0-1000，需要转换
        /// 注意：很多 MCI 驱动不支持音量设置，这是正常的
        /// </summary>
        public void SetVolume(int volume)
        {
            try
            {
                // 限制音量范围 0-100
                volume = Math.Clamp(volume, 0, 100);
                
                // 转换为 MCI 音量 (0-1000)
                int mciVolume = volume * 10;
                
                TemStr = "";
                TemStr = TemStr.PadLeft(127, Convert.ToChar(" "));
                
                // 🔥 尝试多种 MCI 音量命令格式
                // 格式1: setaudio {alias} volume to 1000
                int result1 = APIClass.mciSendString($"setaudio {_alias} volume to {mciVolume}", TemStr, TemStr.Length, 0);
                System.Diagnostics.Debug.WriteLine($"[MP3Play] [{_alias}] Set volume format 1: {volume}% (MCI: {mciVolume}), result: {result1}");
                
                // 格式2: set {alias} audio volume to 1000 (某些驱动使用此格式)
                if (result1 != 0)
                {
                    int result2 = APIClass.mciSendString($"set {_alias} audio volume to {mciVolume}", TemStr, TemStr.Length, 0);
                    System.Diagnostics.Debug.WriteLine($"[MP3Play] [{_alias}] Set volume format 2: {volume}% (MCI: {mciVolume}), result: {result2}");
                }
                
                // 🔥 注意：即使音量设置失败（返回非0），播放也可能正常工作（使用系统默认音量）
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MP3Play] ❌ SetVolume exception: {ex.Message}");
            }
        }
        //停止（完全参考 F5BotV2）
        public void StopT()
        {
            TemStr = "";
            TemStr = TemStr.PadLeft(128, Convert.ToChar(" "));
            ilong = APIClass.mciSendString($"close {_alias}", TemStr, 128, 0);  // 🔥 只关闭当前 alias
            // 注意：不再调用 close all，避免关闭其他正在播放的声音
            mc.state = State.mStop;
        }
        public void Puase()
        {
            TemStr = "";
            TemStr = TemStr.PadLeft(128, Convert.ToChar(" "));
            ilong = APIClass.mciSendString($"pause {_alias}", TemStr, TemStr.Length, 0);  // 🔥 使用唯一 alias
            mc.state = State.mPuase;
        }
        
        private string GetCurrPath(string name)
        {
            if (name.Length < 1) return "";
            name = name.Trim();
            name = name.Substring(0, name.Length - 1);
            return name;
        }
        //总时间（完全参考 F5BotV2）
        public int Duration
        {
            get
            {
                durLength = "";
                durLength = durLength.PadLeft(128, Convert.ToChar(" "));
                APIClass.mciSendString($"status {_alias} length", durLength, durLength.Length, 0);  // 🔥 使用唯一 alias
                durLength = durLength.Trim();
                if (durLength == "") return 0;
                return (int)(Convert.ToDouble(durLength) / 1000f);
            }
        }
        //当前时间（完全参考 F5BotV2）
        public int CurrentPosition
        {
            get
            {
                durLength = "";
                durLength = durLength.PadLeft(128, Convert.ToChar(" "));
                APIClass.mciSendString($"status {_alias} position", durLength, durLength.Length, 0);  // 🔥 使用唯一 alias
                mc.iPos = (int)(Convert.ToDouble(durLength) / 1000f);
                return mc.iPos;
            }
        }
    }
    
    /// <summary>
    /// Windows API 调用类（完全参考 F5BotV2）
    /// </summary>
    public class APIClass
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(
         string lpszLongPath,
         string shortFile,
         int cchBuffer
      );
        [DllImport("winmm.dll", EntryPoint = "mciSendString", CharSet = CharSet.Auto)]
        public static extern int mciSendString(
           string lpstrCommand,
           string lpstrReturnString,
           int uReturnLength,
           int hwndCallback
          );
    }
}

