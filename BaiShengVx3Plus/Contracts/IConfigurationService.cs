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
        
        /// <summary>
        /// 🔥 获取最小投注金额
        /// </summary>
        float GetMinBet();
        
        /// <summary>
        /// 🔥 获取最大投注金额
        /// </summary>
        float GetMaxBet();
        
        /// <summary>
        /// 🔥 获取单期最大投注总额
        /// </summary>
        float GetMaxBetPerIssue();
        
        /// <summary>
        /// 🔥 获取微信订单统一赔率（用于订单结算）
        /// </summary>
        float GetWechatOrderOdds();
        
        /// <summary>
        /// 🔥 获取结算方式：true=整数结算，false=小数2位结算
        /// </summary>
        bool GetIsIntegerSettlement();
        
        /// <summary>
        /// 获取收单关闭时不发送系统消息状态
        /// </summary>
        bool Get收单关闭时不发送系统消息();
        
        /// <summary>
        /// 获取管理模式状态
        /// </summary>
        bool GetIsRunModeAdmin();
        
        /// <summary>
        /// 获取开发模式状态
        /// </summary>
        bool GetIsRunModeDev();
        
        /// <summary>
        /// 获取老板模式状态
        /// </summary>
        bool GetIsRunModeBoss();
        
        /// <summary>
        /// 🔧 获取开发模式：当前会员
        /// </summary>
        string GetRunDevCurrentMember();
        
        /// <summary>
        /// 🔧 获取开发模式：发送消息内容
        /// </summary>
        string GetRunDevSendMessage();
        
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
        
        /// <summary>
        /// 🔥 设置最小投注金额
        /// </summary>
        void SetMinBet(float value);
        
        /// <summary>
        /// 🔥 设置最大投注金额
        /// </summary>
        void SetMaxBet(float value);
        
        /// <summary>
        /// 🔥 设置微信订单统一赔率（用于订单结算）
        /// </summary>
        void SetWechatOrderOdds(float value);
        
        /// <summary>
        /// 🔥 设置结算方式：true=整数结算，false=小数2位结算
        /// </summary>
        void SetIsIntegerSettlement(bool value);
        
        /// <summary>
        /// 设置收单关闭时不发送系统消息状态
        /// </summary>
        void Set收单关闭时不发送系统消息(bool value);
        
        /// <summary>
        /// 设置管理模式
        /// </summary>
        void SetIsRunModeAdmin(bool value);
        
        /// <summary>
        /// 设置开发模式
        /// </summary>
        void SetIsRunModeDev(bool value);
        
        /// <summary>
        /// 设置老板模式
        /// </summary>
        void SetIsRunModeBoss(bool value);
        
        /// <summary>
        /// 🔧 设置开发模式：当前会员
        /// </summary>
        void SetRunDevCurrentMember(string value);
        
        /// <summary>
        /// 🔧 设置开发模式：发送消息内容
        /// </summary>
        void SetRunDevSendMessage(string value);
        
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

