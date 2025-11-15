using System;

namespace BaiShengVx3Plus.Shared.Helpers
{
    /// <summary>
    /// 时间戳转换工具类
    /// 从 LxLib.LxSys.LxTimestampHelper 复制
    /// </summary>
    public static class TimestampHelper
    {
        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式（秒）</param>
        /// <returns>C#格式时间</returns>
        public static DateTime GetDateTime(long timeStamp)
        {
            DateTime time = new DateTime();
            try
            {
                DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                long lTime = long.Parse(timeStamp + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                time = dtStart.Add(toNow);
            }
            catch
            {
                time = DateTime.Now.AddDays(-30);
            }
            return time;
        }
    }
}

