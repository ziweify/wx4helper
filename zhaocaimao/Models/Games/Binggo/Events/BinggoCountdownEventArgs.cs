using System;

namespace zhaocaimao.Models.Games.Binggo.Events
{
    /// <summary>
    /// 炳狗倒计时事件参数
    /// </summary>
    public class BinggoCountdownEventArgs : EventArgs
    {
        /// <summary>
        /// 距离封盘的秒数
        /// </summary>
        public int Seconds { get; set; }
        
        /// <summary>
        /// 当前期号
        /// </summary>
        public int IssueId { get; set; }
    }
}

