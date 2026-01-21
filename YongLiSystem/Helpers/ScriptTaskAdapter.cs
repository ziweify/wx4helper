using System;
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
            return new BrowserTaskConfig
            {
                Name = task.Name,
                Url = task.Url,
                Username = task.Username,
                Password = task.Password,
                Script = task.Script,
                AutoLogin = task.AutoLogin,
                CreatedTime = task.CreatedTime,
                LastModifiedTime = task.LastRunTime // 映射到 LastRunTime
            };
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
            task.Script = config.Script;
            task.AutoLogin = config.AutoLogin;
            task.LastRunTime = config.LastModifiedTime; // 映射回 LastRunTime
        }
    }
}
