using System;
using Unit.Shared.Models;

namespace Unit.Shared.Models
{
    /// <summary>
    /// 赔率信息（通用数据结构）
    /// </summary>
    public class OddsInfo
    {
        /// <summary>
        /// 车号（我们程序定义的）
        /// </summary>
        public CarNumEnum Car { get; set; }
        
        /// <summary>
        /// 玩法（我们程序定义的）
        /// </summary>
        public BetPlayEnum Play { get; set; }
        
        /// <summary>
        /// 显示名称（网站使用的名字）
        /// 例如："平一", "平二", "总和"
        /// </summary>
        public string CarName { get; set; } = "";
        
        /// <summary>
        /// 赔率（网站使用的赔率）
        /// 例如：1.97, 1.95
        /// </summary>
        public float Odds { get; set; }
        
        /// <summary>
        /// 赔率ID（网站使用的ID）
        /// 例如："5370", "5371" (TongBao平台)
        /// </summary>
        public string OddsId { get; set; } = "";
        
        /// <summary>
        /// 完整名称（用于显示）
        /// 例如："平一大", "平二小", "总和大"
        /// </summary>
        public string FullName => $"{CarName}{Play switch
        {
            BetPlayEnum.大 => "大",
            BetPlayEnum.小 => "小",
            BetPlayEnum.单 => "单",
            BetPlayEnum.双 => "双",
            BetPlayEnum.尾大 => "尾大",
            BetPlayEnum.尾小 => "尾小",
            BetPlayEnum.合单 => "合单",
            BetPlayEnum.合双 => "合双",
            _ => Play.ToString()
        }}";

        public OddsInfo()
        {
        }

        public OddsInfo(CarNumEnum car, BetPlayEnum play, string carName, string oddsId, float odds)
        {
            Car = car;
            Play = play;
            CarName = carName;
            OddsId = oddsId;
            Odds = odds;
        }
    }
}
