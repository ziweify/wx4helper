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
        云顶 = 21,  // 🔥 保留云顶（现有项目使用）
        yyds = 22   // 🔥 YYDS 平台
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
        /// 🔥 所有平台配置（唯一数据源，参考 F5BotV2 BetSiteFactory）
        /// 维护此文件即可同步更新"配置管理"和"快速设置"的盘口选项
        /// </summary>
        private static readonly Dictionary<BetPlatform, PlatformInfo> _platforms = new()
        {
            {
                BetPlatform.不使用盘口, new PlatformInfo
                {
                    Platform = BetPlatform.不使用盘口,
                    DefaultUrl = "about:blank",
                    LegacyNames = new[] { "None", "NoneSite" }
                }
            },
            {
                BetPlatform.元宇宙2, new PlatformInfo
                {
                    Platform = BetPlatform.元宇宙2,
                    DefaultUrl = "http://yyz.168app.net/2/",
                    LegacyNames = new[] { "YYZ2", "YuanYuZhou2", "YuanYuZhou" }
                }
            },
            {
                BetPlatform.海峡, new PlatformInfo
                {
                    Platform = BetPlatform.海峡,
                    DefaultUrl = "https://4921031761-cj.mm666.co/",
                    LegacyNames = new[] { "HaiXia", "HaiXia28", "HX", "HX666" }
                }
            },
            {
                BetPlatform.QT, new PlatformInfo
                {
                    Platform = BetPlatform.QT,
                    DefaultUrl = "http://119.23.246.81/qt/",
                    LegacyNames = new[] { "Qt", "QTBet" }
                }
            },
            {
                BetPlatform.茅台, new PlatformInfo
                {
                    Platform = BetPlatform.茅台,
                    DefaultUrl = "https://8912794526-tky.c4ux0uslgd.com/",
                    LegacyNames = new[] { "MaoTai", "Mt168", "MT" }
                }
            },
            {
                BetPlatform.太平洋, new PlatformInfo
                {
                    Platform = BetPlatform.太平洋,
                    DefaultUrl = "https://8912794526-tky.c4ux0uslgd.com/",  // 🔥 复用茅台URL（F5BotV2中两者共用）
                    LegacyNames = new[] { "TaiPingYang", "TPY" }
                }
            },
            {
                BetPlatform.蓝A, new PlatformInfo
                {
                    Platform = BetPlatform.蓝A,
                    DefaultUrl = "https://lana.example.com/",  // 🔥 需要用户手动配置（F5BotV2中未设置默认URL）
                    LegacyNames = new[] { "LanA", "BlueA" }
                }
            },
            {
                BetPlatform.红海, new PlatformInfo
                {
                    Platform = BetPlatform.红海,
                    DefaultUrl = "https://pjpctreyoobvf.6f888.net/#/",
                    LegacyNames = new[] { "HongHai", "HH" }
                }
            },
            {
                BetPlatform.S880, new PlatformInfo
                {
                    Platform = BetPlatform.S880,
                    DefaultUrl = "http://47.106.119.141:880/",
                    LegacyNames = new[] { "s880" }
                }
            },
            {
                BetPlatform.ADK, new PlatformInfo
                {
                    Platform = BetPlatform.ADK,
                    DefaultUrl = "https://yk.adkdkdkd.com/",
                    LegacyNames = new[] { "adk" }
                }
            },
            {
                BetPlatform.红海无名, new PlatformInfo
                {
                    Platform = BetPlatform.红海无名,
                    DefaultUrl = "https://pjpctreyoobvf.6f888.net/#/",
                    LegacyNames = new[] { "HongHaiWuMing", "HHWM" }
                }
            },
            {
                BetPlatform.果然, new PlatformInfo
                {
                    Platform = BetPlatform.果然,
                    DefaultUrl = "https://kk888.link/",
                    LegacyNames = new[] { "GuoRan", "Kk888" }
                }
            },
            {
                BetPlatform.蓝B, new PlatformInfo
                {
                    Platform = BetPlatform.蓝B,
                    DefaultUrl = "http://119.23.246.81/qt/",  // 🔥 复用QT的URL（F5BotV2中两者共用QtBet脚本）
                    LegacyNames = new[] { "LanB", "BlueB" }
                }
            },
            {
                BetPlatform.AC, new PlatformInfo
                {
                    Platform = BetPlatform.AC,
                    DefaultUrl = "https://3151135604-acyl.yy777.co/",
                    LegacyNames = new[] { "ac", "Ac" }
                }
            },
            {
                BetPlatform.通宝, new PlatformInfo
                {
                    Platform = BetPlatform.通宝,
                    DefaultUrl = "https://tbfowenb.fr.cvv66.top/",
                    LegacyNames = new[] { "TongBao", "TB" }
                }
            },
            {
                BetPlatform.通宝PC, new PlatformInfo
                {
                    Platform = BetPlatform.通宝PC,
                    DefaultUrl = "https://tbfowenb.fr.cvv66.top/",
                    LegacyNames = new[] { "TongBaoPC", "TBPC" }
                }
            },
            {
                BetPlatform.HY168, new PlatformInfo
                {
                    Platform = BetPlatform.HY168,
                    DefaultUrl = "http://hy.168bingo.top/",
                    LegacyNames = new[] { "hy168", "HY" }
                }
            },
            {
                BetPlatform.bingo168, new PlatformInfo
                {
                    Platform = BetPlatform.bingo168,
                    DefaultUrl = "http://hy.168bingo.top/",  // 🔥 复用HY168的URL（F5BotV2中两者共用）
                    LegacyNames = new[] { "Bingo168", "bingo" }
                }
            },
            {
                BetPlatform.云顶, new PlatformInfo
                {
                    Platform = BetPlatform.云顶,
                    DefaultUrl = "https://yd28.vip",
                    LegacyNames = new[] { "YunDing", "YunDing28", "YD" }
                }
            },
            {
                BetPlatform.yyds, new PlatformInfo
                {
                    Platform = BetPlatform.yyds,
                    DefaultUrl = "https://client.06n.yyds666.me/",  // 🔥 YYDS 平台
                    LegacyNames = new[] { "YYDS", "Yyds" }
                }
            }
        };
        
        /// <summary>
        /// 获取所有平台名称（用于UI下拉框）
        /// 🔥 修复：必须和 GetAllPlatforms() 保持相同顺序（按枚举值排序）
        /// </summary>
        public static string[] GetAllPlatformNames()
        {
            return GetAllPlatforms().Select(p => p.ToString()).ToArray();
        }
        
        /// <summary>
        /// 根据名称获取枚举（兼容旧数据）
        /// </summary>
        public static BetPlatform Parse(string name)
        {
            // 1. 尝试直接解析中文名
            if (Enum.TryParse<BetPlatform>(name, out var platform))
                return platform;
            
            // 2. 兼容旧的英文名（支持 F5BotV2 的旧数据格式）
            foreach (var kvp in _platforms)
            {
                if (kvp.Value.LegacyNames.Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    return kvp.Key;
            }
            
            // 3. 默认返回"不使用盘口"（避免误操作）
            return BetPlatform.不使用盘口;
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
        /// 获取所有平台列表（按枚举值顺序）
        /// </summary>
        private static BetPlatform[]? _allPlatforms;
        
        public static BetPlatform[] GetAllPlatforms()
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
