using System;

namespace 永利系统.Models.Dashboard
{
    /// <summary>
    /// 数据采集配置
    /// </summary>
    public class DataCollectionConfig
    {
        /// <summary>
        /// 数据源URL
        /// </summary>
        public string DataSourceUrl { get; set; } = "https://www.taiwanlottery.com.tw/lotto/BingoBingo/OEHLStatistic.htm";

        /// <summary>
        /// 采集间隔(秒)
        /// </summary>
        public int CollectionInterval { get; set; } = 5;

        /// <summary>
        /// 是否使用代理
        /// </summary>
        public bool UseProxy { get; set; }

        /// <summary>
        /// 代理地址 (格式: IP:Port, 如: 127.0.0.1:7890)
        /// </summary>
        public string ProxyAddress { get; set; } = "127.0.0.1:7890";

        /// <summary>
        /// 提交地址 (多行,每行一个地址)
        /// 格式: [标识]URL
        /// 示例: [w168]http://api.example.com/upload
        /// </summary>
        public string SubmitAddresses { get; set; } = string.Empty;

        /// <summary>
        /// 当期期号
        /// </summary>
        public string CurrentIssue { get; set; } = string.Empty;

        /// <summary>
        /// 当期开奖时间
        /// </summary>
        public string CurrentOpenTime { get; set; } = string.Empty;

        /// <summary>
        /// 下期期号
        /// </summary>
        public string NextIssue { get; set; } = string.Empty;

        /// <summary>
        /// 下期开奖时间
        /// </summary>
        public string NextOpenTime { get; set; } = string.Empty;

        /// <summary>
        /// 倒计时(秒)
        /// </summary>
        public int Countdown { get; set; }

        /// <summary>
        /// 是否自动采集
        /// </summary>
        public bool IsAutoCollecting { get; set; }
    }
}

