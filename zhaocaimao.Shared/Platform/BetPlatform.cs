using System;
using System.Collections.Generic;
using System.Linq;

namespace zhaocaimao.Shared.Platform
{
    /// <summary>
    /// 投注平台枚举（直接使用中文名，参考 F5BotV2 BetSiteType）
    /// </summary>
    public enum BetPlatform
    {
        不使用盘口 = 0,
        元宇宙2 = 1,
        海峡 = 2,
        QT = 3,
        茅台 = 5,
        太平洋 = 6,
        蓝A = 7,
        红海 = 8,
        S880 = 9,
        ADK = 10,
        红海无名 = 11,
        果然 = 12,
        蓝B = 15,
        AC = 16,
        通宝 = 17,
        通宝PC = 18,
        HY168 = 19,
        bingo168 = 20,
        云顶 = 21  // 🔥 保留云顶（现有项目使用）
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
            // 注意：其他新增平台（元宇宙2、QT、茅台等）的URL配置需要在 PlatformUrlManager 中配置
            // 这里只配置有明确URL的平台
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
        /// 获取所有平台列表（按枚举顺序）
        /// </summary>
        private static BetPlatform[]? _allPlatforms;
        
        private static BetPlatform[] GetAllPlatforms()
        {
            if (_allPlatforms == null)
            {
                _allPlatforms = Enum.GetValues(typeof(BetPlatform))
                    .Cast<BetPlatform>()
                    .OrderBy(p => (int)p)
                    .ToArray();
            }
            return _allPlatforms;
        }
        
        /// <summary>
        /// 根据索引获取平台（索引对应下拉框中的位置）
        /// </summary>
        public static BetPlatform GetByIndex(int index)
        {
            var platforms = GetAllPlatforms();
            if (index >= 0 && index < platforms.Length)
                return platforms[index];
            return BetPlatform.不使用盘口; // 默认返回第一个
        }
        
        /// <summary>
        /// 获取平台索引（对应下拉框中的位置）
        /// </summary>
        public static int GetIndex(BetPlatform platform)
        {
            var platforms = GetAllPlatforms();
            for (int i = 0; i < platforms.Length; i++)
            {
                if (platforms[i] == platform)
                    return i;
            }
            return 0; // 默认返回第一个索引
        }
    }
}
