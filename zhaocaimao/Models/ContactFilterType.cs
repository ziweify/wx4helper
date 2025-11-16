namespace zhaocaimao.Models
{
    /// <summary>
    /// 联系人过滤类型
    /// </summary>
    public enum ContactFilterType
    {
        /// <summary>
        /// 全部联系人（不过滤）
        /// </summary>
        全部 = 0,
        
        /// <summary>
        /// 个人联系人（wxid 不含 @ 符号）
        /// </summary>
        联系人 = 1,
        
        /// <summary>
        /// 群组（wxid 包含 @ 符号）
        /// </summary>
        群组 = 2
    }
}

