using System;
using System.Collections.Generic;

namespace zhaocaimao.Models.AutoBet
{
    /// <summary>
    /// 标准投注订单（参考 F5BotV2）
    /// ✅ 使用枚举表示，无需浏览器再次解析
    /// ✅ 主程序统一解析，浏览器直接投注
    /// </summary>
    public class BetStandardOrder
    {
        /// <summary>
        /// 期号
        /// </summary>
        public int IssueId { get; set; }
        
        /// <summary>
        /// 位置：P1/P2/P3/P4/P5/P总
        /// </summary>
        public CarNumEnum Car { get; set; }
        
        /// <summary>
        /// 玩法：大/小/单/双/豹子...
        /// </summary>
        public BetPlayEnum Play { get; set; }
        
        /// <summary>
        /// 金额
        /// </summary>
        public int MoneySum { get; set; }
        
        /// <summary>
        /// 赔率（可选）
        /// </summary>
        public float Odds { get; set; }
        
        public BetStandardOrder()
        {
        }
        
        public BetStandardOrder(int issueId, CarNumEnum car, BetPlayEnum play, int moneySum)
        {
            IssueId = issueId;
            Car = car;
            Play = play;
            MoneySum = moneySum;
        }
        
        public override string ToString()
        {
            return $"{Car}{Play}{MoneySum}";
        }
    }
    
    /// <summary>
    /// 标准投注订单列表
    /// </summary>
    public class BetStandardOrderList : List<BetStandardOrder>
    {
        /// <summary>
        /// 获取总金额
        /// </summary>
        public int GetTotalAmount()
        {
            int result = 0;
            foreach (var item in this)
            {
                result += item.MoneySum;
            }
            return result;
        }
    }
    
    /// <summary>
    /// 位置枚举：P1/P2/P3/P4/P5/P总
    /// </summary>
    public enum CarNumEnum
    {
        未知 = 0,
        P1 = 1,
        P2 = 2,
        P3 = 3,
        P4 = 4,
        P5 = 5,
        P总 = 6
    }
    
    /// <summary>
    /// 玩法枚举
    /// </summary>
    public enum BetPlayEnum
    {
        未知 = 0,
        单 = 1,
        双 = 2,
        大 = 3,
        小 = 4,
        尾大 = 5,
        尾小 = 6,
        合单 = 7,
        合双 = 8,
        龙 = 9,
        虎 = 10,
        豹子 = 11,
        顺子 = 12,
        寿 = 13,
        喜 = 14
    }
}

