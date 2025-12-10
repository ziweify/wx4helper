using System;

namespace zhaocaimao.Utils
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
        public const string Version = "5.3.3";  

        /// <summary>
        /// 版本名称
        /// </summary>
        public const string VersionName = "智能管理系统";
        
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

    /*
     * 5.3.2
     *    修正封盘后还能下注的问题
     *    修正状态管理问题
     *    
     * 5.2.2
     *      修复ADK支持
     */

    /*
     * 修复完成总结
1. 管理员上分时创建记录
修改 AdminCommandHandler.ExecuteCreditWithdraw，创建 V2CreditWithdraw 记录并调用 CreditWithdrawService.ProcessCreditWithdraw（参考 F5BotV2 Line 2759-2762, 2814-2817）
在 VxMain.cs 中设置 CreditWithdrawService 和 V2CreditWithdrawBindingList 依赖
2. 添加忽略功能
在 CreditWithdrawStatus 枚举中添加 忽略 = 3
在 CreditWithdrawService 中添加 IgnoreCreditWithdraw 方法（参考 F5BotV2 Line 1526-1542）
在 CreditWithdrawManageForm 中添加忽略按钮和处理逻辑
3. 修复按钮状态显示
操作后按钮显示操作内容（"已同意"、"已忽略"、"已拒绝"）并禁用（参考 F5BotV2 Line 179-187, 194-204）
使用 CellPainting 事件控制按钮状态和文本
4. 添加颜色设置
动作颜色：上分绿色、下分红色（参考 F5BotV2 Line 147-168）
状态颜色：等待处理红色、已同意绿色、忽略浅灰色、已拒绝橙色（参考 F5BotV2 Line 169-209）
金额颜色：百元浅灰色、千元绿色、万元橙色（参考 F5BotV2 Line 211-237）
5. 修复字段显示
移除 ProcessedBy 和 ProcessedTime 的 [Browsable(false)] 特性，使其在 DataGridView 中显示
添加 [DataGridColumn] 特性配置列头标题和宽度
6. 更新状态筛选
在状态下拉框中添加"忽略"选项
更新筛选逻辑以支持忽略状态
     */
}

