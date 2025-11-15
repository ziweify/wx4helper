using BaiShengVx3Plus.Shared.Models;

namespace BsBrowserClient.Models
{
    /// <summary>
    /// 赔率信息（用于赔率显示窗口）
    /// </summary>
    public class OddsInfo
    {
        /// <summary>
        /// 标准车号（程序定义的）
        /// </summary>
        public CarNumEnum Car { get; set; }
        
        /// <summary>
        /// 标准玩法（程序定义的）
        /// </summary>
        public BetPlayEnum Play { get; set; }
        
        /// <summary>
        /// 完整名称（车号+玩法）
        /// </summary>
        public string FullName => $"{Car}{Play}";
        
        /// <summary>
        /// 网站使用的名称（如："平一大"）
        /// </summary>
        public string CarName { get; set; } = "";
        
        /// <summary>
        /// 赔率（网站的赔率）
        /// </summary>
        public float Odds { get; set; }
        
        /// <summary>
        /// 网站使用的ID（如："5370"）
        /// </summary>
        public string OddsId { get; set; } = "";
        
        public OddsInfo()
        {
        }
        
        public OddsInfo(CarNumEnum car, BetPlayEnum play, string carName, float odds, string oddsId)
        {
            Car = car;
            Play = play;
            CarName = carName;
            Odds = odds;
            OddsId = oddsId;
        }
    }
}

