using Unit.La.Models;
using YongLiSystem.Models.Dashboard;

namespace YongLiSystem.Helpers
{
    /// <summary>
    /// ScriptTask 和 BrowserTaskInfo 之间的转换扩展方法
    /// 用于适配项目特定的 ScriptTask 模型到 Unit.la 的通用模型
    /// </summary>
    public static class ScriptTaskExtensions
    {
        /// <summary>
        /// 将 ScriptTask 转换为 BrowserTaskInfo
        /// </summary>
        public static BrowserTaskInfo ToBrowserTaskInfo(this ScriptTask scriptTask)
        {
            return new BrowserTaskInfo
            {
                Id = scriptTask.Id,
                Name = scriptTask.Name,
                Url = scriptTask.Url,
                Status = scriptTask.Status,
                IsRunning = scriptTask.IsRunning,
                LastRunTime = scriptTask.LastRunTime,
                Tag = scriptTask // 将原始 ScriptTask 存储在 Tag 中
            };
        }

        /// <summary>
        /// 从 BrowserTaskInfo 更新 ScriptTask
        /// </summary>
        public static void UpdateFromBrowserTaskInfo(this ScriptTask scriptTask, BrowserTaskInfo taskInfo)
        {
            // 只更新UI相关的字段，不更新脚本、配置等业务数据
            scriptTask.Name = taskInfo.Name;
            scriptTask.Url = taskInfo.Url;
            scriptTask.Status = taskInfo.Status;
            scriptTask.IsRunning = taskInfo.IsRunning;
            scriptTask.LastRunTime = taskInfo.LastRunTime;
        }

        /// <summary>
        /// 从 BrowserTaskInfo 获取原始 ScriptTask
        /// </summary>
        public static ScriptTask? GetScriptTask(this BrowserTaskInfo taskInfo)
        {
            return taskInfo.Tag as ScriptTask;
        }
    }
}
