using System;
using System.Collections.Generic;

namespace Unit.La.Models
{
    /// <summary>
    /// 脚本信息 - 支持内存和文件两种模式
    /// </summary>
    public class ScriptInfo
    {
        /// <summary>
        /// 脚本ID（唯一标识）
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 脚本名称（如 main.lua, functions.lua）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 脚本显示名称（用于UI显示）
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 脚本内容（内存模式）
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 脚本文件路径（文件模式，可为空）
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// 脚本类型
        /// </summary>
        public ScriptType Type { get; set; } = ScriptType.Main;

        /// <summary>
        /// 是否为内存模式（true=内存，false=文件）
        /// </summary>
        public bool IsMemoryMode => string.IsNullOrEmpty(FilePath);

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否已修改（未保存）
        /// </summary>
        public bool IsModified { get; set; }

        /// <summary>
        /// 元数据（扩展字段，如网络URL、版本等）
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        public override string ToString()
        {
            return $"{DisplayName} ({Type})";
        }
    }

    /// <summary>
    /// 脚本类型
    /// </summary>
    public enum ScriptType
    {
        Main,           // 主脚本
        Functions,      // 功能库
        Test,           // 测试脚本
        Custom          // 自定义脚本
    }
}
