using System;
using System.Collections.Generic;
using System.Linq;

namespace BaiShengVx3Plus.Shared.Platform
{
    /// <summary>
    /// 投注平台枚举（直接使用中文名）
    /// </summary>
    public enum BetPlatform
    {
        云顶 = 0,
        海峡 = 1,
        红海 = 2,
        通宝 = 3
    }
    
    /// <summary>
    /// 平台配置信息
    /// </summary>
    public class PlatformInfo
    {
        public BetPlatform Platform { get; set; }
        public string DefaultUrl { get; set; } = "";
        
        // 兼容旧数据的内部名称
        public string[] LegacyNames { get; set; } = Array.Empty<string>();
    }
    
    /// <summary>
    /// 投注平台工具类
    /// </summary>
    public static class BetPlatformHelper
    {
        /// <summary>
        /// 所有平台配置（唯一数据源）
        /// </summary>
        private static readonly Dictionary<BetPlatform, PlatformInfo> _platforms = new()
        {
            {
                BetPlatform.云顶, new PlatformInfo
                {
                    Platform = BetPlatform.云顶,
                    DefaultUrl = "https://yd28.vip",
                    LegacyNames = new[] { "YunDing", "YunDing28", "yunding", "yunding28" }
                }
            },
            {
                BetPlatform.海峡, new PlatformInfo
                {
                    Platform = BetPlatform.海峡,
                    DefaultUrl = "https://hx28.vip",
                    LegacyNames = new[] { "HaiXia", "HaiXia28", "haixia", "haixia28" }
                }
            },
            {
                BetPlatform.红海, new PlatformInfo
                {
                    Platform = BetPlatform.红海,
                    DefaultUrl = "https://hh28.vip",
                    LegacyNames = new[] { "HongHai", "HongHai28", "honghai", "honghai28" }
                }
            },
            {
                BetPlatform.通宝, new PlatformInfo
                {
                    Platform = BetPlatform.通宝,
                    DefaultUrl = "https://tbfowenb.fr.cvv66.top/",
                    LegacyNames = new[] { "TongBao", "TongBao28", "tongbao", "tongbao28" }
                }
            }
        };
        
        /// <summary>
        /// 获取所有平台名称（用于UI下拉框）
        /// </summary>
        public static string[] GetAllPlatformNames()
        {
            return Enum.GetNames(typeof(BetPlatform));
        }
        
        /// <summary>
        /// 根据名称获取枚举（兼容旧数据）
        /// </summary>
        public static BetPlatform Parse(string name)
        {
            // 1. 尝试直接解析中文名
            if (Enum.TryParse<BetPlatform>(name, out var platform))
                return platform;
            
            // 2. 兼容旧的英文名
            foreach (var kvp in _platforms)
            {
                if (kvp.Value.LegacyNames.Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    return kvp.Key;
            }
            
            // 3. 默认返回云顶
            return BetPlatform.云顶;
        }
        
        /// <summary>
        /// 获取平台默认URL
        /// </summary>
        public static string GetDefaultUrl(BetPlatform platform)
        {
            return _platforms.TryGetValue(platform, out var info) 
                ? info.DefaultUrl 
                : "about:blank";
        }
        
        /// <summary>
        /// 根据名称获取默认URL（兼容旧数据）
        /// </summary>
        public static string GetDefaultUrl(string name)
        {
            return GetDefaultUrl(Parse(name));
        }
        
        /// <summary>
        /// 根据索引获取平台
        /// </summary>
        public static BetPlatform GetByIndex(int index)
        {
            return index switch
            {
                0 => BetPlatform.云顶,
                1 => BetPlatform.海峡,
                2 => BetPlatform.红海,
                3 => BetPlatform.通宝,
                _ => BetPlatform.云顶
            };
        }
        
        /// <summary>
        /// 获取平台索引
        /// </summary>
        public static int GetIndex(BetPlatform platform)
        {
            return (int)platform;
        }
    }
}
