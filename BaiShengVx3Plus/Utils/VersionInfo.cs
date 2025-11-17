using System;

namespace BaiShengVx3Plus.Utils
{
    /// <summary>
    /// 版本信息管理
    /// </summary>
    public static class VersionInfo
    {
        /// <summary>
        /// 当前版本号
        /// 格式：主版本.次版本.修订号.构建号
        /// </summary>
        public const string Version = "31.8.1";  // 2025-11-14 构建
        
        /// <summary>
        /// 版本名称
        /// </summary>
        public const string VersionName = "百盛Vx3 Plus";
        
        /// <summary>
        /// 完整版本字符串
        /// </summary>
        public static string FullVersion => $"{VersionName} v{Version}";
        
        /// <summary>
        /// 构建日期
        /// </summary>
        public static string BuildDate => "2025-11-14";
        
        /// <summary>
        /// 更新日志
        /// </summary>
        public static string ChangeLog => @"
v3.1.0.1114 (2025-11-14)
─────────────────────────
✅ 修复：开奖数据库表不存在问题（防御性编程）
✅ 修复：浏览器连接状态检测问题
✅ 新增：管理员命令（刷新、管理上下分）
✅ 优化：日志性能（虚拟加载、SQL COUNT）
✅ 优化：图片生成路径（C:\images\，需管理员权限）
✅ 新增：记住密码功能（Base64加密）
✅ 新增：版本信息显示
";
    }
}

