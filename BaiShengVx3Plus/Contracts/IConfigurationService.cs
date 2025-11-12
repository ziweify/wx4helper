namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 配置服务接口
    /// 提供统一的配置读取和保存功能
    /// </summary>
    public interface IConfigurationService
    {
        // ========================================
        // 读取配置
        // ========================================
        
        /// <summary>
        /// 获取收单开关状态
        /// </summary>
        bool GetIsOrdersTaskingEnabled();
        
        /// <summary>
        /// 获取自动投注开关状态
        /// </summary>
        bool GetIsAutoBetEnabled();
        
        /// <summary>
        /// 获取提前封盘秒数
        /// </summary>
        int GetSealSecondsAhead();
        
        // ========================================
        // 保存配置
        // ========================================
        
        /// <summary>
        /// 设置收单开关
        /// </summary>
        void SetIsOrdersTaskingEnabled(bool value);
        
        /// <summary>
        /// 设置自动投注开关
        /// </summary>
        void SetIsAutoBetEnabled(bool value);
        
        /// <summary>
        /// 设置提前封盘秒数
        /// </summary>
        void SetSealSecondsAhead(int value);
        
        // ========================================
        // 配置管理
        // ========================================
        
        /// <summary>
        /// 重新加载配置（从文件）
        /// </summary>
        void ReloadConfiguration();
        
        /// <summary>
        /// 立即保存配置到文件
        /// </summary>
        void SaveConfiguration();
        
        // ========================================
        // 事件通知
        // ========================================
        
        /// <summary>
        /// 配置变更事件（用于 UI 绑定和响应）
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
    }
    
    /// <summary>
    /// 配置变更事件参数
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        public string PropertyName { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }
}

