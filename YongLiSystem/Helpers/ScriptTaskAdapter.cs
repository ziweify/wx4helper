using System;
using System.IO;
using Unit.La.Models;
using YongLiSystem.Models.Dashboard;

namespace YongLiSystem.Helpers
{
    /// <summary>
    /// ScriptTask 与 BrowserTaskConfig 的适配器
    /// </summary>
    public static class ScriptTaskAdapter
    {
        /// <summary>
        /// ScriptTask 转 BrowserTaskConfig
        /// </summary>
        public static BrowserTaskConfig ToBrowserTaskConfig(this ScriptTask task)
        {
            var config = new BrowserTaskConfig
            {
                Name = task.Name,
                Url = task.Url,
                Username = task.Username,
                Password = task.Password,
                Script = "", // 不再直接存储脚本内容
                AutoLogin = task.AutoLogin,
                CreatedTime = task.CreatedTime,
                LastModifiedTime = task.LastRunTime, // 映射到 LastRunTime
                ScriptSourceMode = ScriptSourceMode.Local
            };

            // 🔥 配置脚本目录：如果 Script 字段包含目录路径，则设置
            if (!string.IsNullOrEmpty(task.Script) && Directory.Exists(task.Script))
            {
                config.ScriptDirectory = task.Script;
            }
            else
            {
                // 兼容旧数据：如果是脚本内容，创建临时目录
                var tempDir = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Scripts",
                    $"Task_{task.Id}"
                );
                
                config.ScriptDirectory = tempDir;
                
                // 如果目录不存在且有旧脚本内容，创建目录并迁移
                if (!Directory.Exists(tempDir) && !string.IsNullOrEmpty(task.Script))
                {
                    Unit.La.Scripting.LocalScriptLoader.CreateDefaultScripts(tempDir);
                }
            }

            return config;
        }

        /// <summary>
        /// BrowserTaskConfig 更新到 ScriptTask
        /// </summary>
        public static void UpdateFromConfig(this ScriptTask task, BrowserTaskConfig config)
        {
            task.Name = config.Name;
            task.Url = config.Url;
            task.Username = config.Username;
            task.Password = config.Password;
            
            // 🔥 保存脚本目录路径而不是脚本内容
            if (!string.IsNullOrEmpty(config.ScriptDirectory))
            {
                task.Script = config.ScriptDirectory;
            }
            
            task.AutoLogin = config.AutoLogin;
            task.LastRunTime = config.LastModifiedTime; // 映射回 LastRunTime
        }
    }
}
