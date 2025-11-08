using System;
using System.ComponentModel;
using SQLite;

namespace BaiShengVx3Plus.Models.AutoBet
{
    /// <summary>
    /// 自动投注配置
    /// </summary>
    [Table("AutoBetConfigs")]
    public class BetConfig : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string ConfigName { get; set; } = "";
        
        public string Platform { get; set; } = "YunDing28";
        
        public string PlatformUrl { get; set; } = "";
        
        public string Username { get; set; } = "";
        
        public string Password { get; set; } = "";
        
        public bool IsEnabled { get; set; } = true;
        
        public bool ShowBrowserWindow { get; set; } = false;
        
        public decimal MinBetAmount { get; set; } = 1;
        
        public decimal MaxBetAmount { get; set; } = 10000;
        
        public bool AutoLogin { get; set; } = true;
        
        public decimal CurrentBalance { get; set; }
        
        public string Status { get; set; } = "未启动";
        
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
        
        public string? Notes { get; set; }
        
        public bool IsDefault { get; set; } = false;
        
        /// <summary>
        /// 显示浏览器窗口（兼容属性）
        /// </summary>
        [Ignore]
        public bool ShowBrowser
        {
            get => ShowBrowserWindow;
            set => ShowBrowserWindow = value;
        }
        
        /// <summary>
        /// Cookie信息（字符串格式，如：key1=value1; key2=value2）
        /// </summary>
        public string? Cookies { get; set; }
        
        /// <summary>
        /// Cookie 更新时间
        /// </summary>
        public DateTime? CookieUpdateTime { get; set; }
        
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

