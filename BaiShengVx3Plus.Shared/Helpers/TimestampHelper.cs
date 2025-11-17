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
        
        /// <summary>
        /// DateTime时间格式转换为10位不带毫秒的Unix时间戳
        /// 🔥 参考 F5BotV2 LxTimestampHelper.ConvertDateTimeInt
        /// </summary>
        /// <param name="time">DateTime时间格式</param>
        /// <returns>Unix时间戳格式（秒）</returns>
        public static long ConvertDateTimeInt(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalSeconds;
        }
    }
}

