using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LxLib.LxSys
{
    public static class LxTimestampHelper
    {
        /// <summary>
        /// DateTime时间格式转换为13位带毫秒的Unix时间戳
        /// </summary>
        /// <param name="time">DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public static long ConvertDateTimeLong(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalMilliseconds;
        }

        /// <summary>
        /// DateTime时间格式转换为10位不带毫秒的Unix时间戳
        /// </summary>
        /// <param name="time">DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public static long ConvertDateTimeInt(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalSeconds;
        }



        /// <summary>
        ///     时间格式字符串，转换成时间
        ///     参数参考 strDatetime = 2023-09-23 59:59:59
        /// </summary>
        /// <returns></returns>
        public static DateTime strtodatetime(string strDatetime)
        {
            return DateTime.Parse(strDatetime);
        }

        public static string datetimetostr(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Unix时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式,例如:1482115779, 或long类型</param>
        /// <returns>C#格式时间</returns>
        public static DateTime GetDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
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
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ToLong(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalSeconds;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public static string GetTimeStamp13()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }


        public static int GetTimeStampToInt32()
        {
            try
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return Convert.ToInt32(ts.TotalSeconds);
            }
            catch
            {
                return -1;
            }
        }
    }
}
