using System;
using System.Collections.Generic;

namespace Unit.La.Models
{
    /// <summary>
    /// 脚本源配置
    /// </summary>
    public class ScriptSourceConfig
    {
        /// <summary>
        /// 脚本源模式
        /// </summary>
        public ScriptSourceMode Mode { get; set; } = ScriptSourceMode.Local;

        /// <summary>
        /// 本地文件夹路径（本地模式）
        /// </summary>
        public string LocalDirectory { get; set; } = string.Empty;

        /// <summary>
        /// 远程URL（远程模式）
        /// </summary>
        public string RemoteUrl { get; set; } = string.Empty;

        /// <summary>
        /// 远程认证Token（可选）
        /// </summary>
        public string? RemoteAuthToken { get; set; }

        /// <summary>
        /// 自动刷新间隔（秒，0=不自动刷新）
        /// </summary>
        public int AutoRefreshInterval { get; set; } = 0;

        /// <summary>
        /// 最后刷新时间
        /// </summary>
        public DateTime LastRefreshTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 验证配置
        /// </summary>
        public bool IsValid()
        {
            return Mode switch
            {
                ScriptSourceMode.Local => !string.IsNullOrEmpty(LocalDirectory),
                ScriptSourceMode.Remote => !string.IsNullOrEmpty(RemoteUrl) && Uri.IsWellFormedUriString(RemoteUrl, UriKind.Absolute),
                _ => false
            };
        }
    }

    /// <summary>
    /// 脚本源模式
    /// </summary>
    public enum ScriptSourceMode
    {
        /// <summary>
        /// 本地文件模式
        /// </summary>
        Local,

        /// <summary>
        /// 远程URL模式
        /// </summary>
        Remote
    }
}
