using LxLib.LxSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Game.BinGou
{
    public static class BinGouHelper
    {
        const int count_real = 203;
        const int count_full = 288;
        const int firstNumber = 1;
        //const int firstIssueld = 111024361; //基准期号
        //const int firstTimestamp = 1651359900; // 基准时间戳
        //const int firstIssueld = 113000001; //基准期号
        //const int firstTimestamp = 1704063900; // 基准时间戳
                                               //2025
        const int firstIssueld = 114000001; //基准期号
        const int firstTimestamp = 1735686300; // 基准时间戳
        //以某一期的基准时间作为开奖参照物。来计算当前期的开奖时间
        //以2022.5.1号为参照物
        /*
         *   当天第一期时间
         *     "open_data": "03,18,29,57,26",
                "action_no": "111024361",
                "open_time": 1651359900,
                "date": "2022-05-01",
                "number": 1,
                "open_time_value": "2022-05-01 07:05:00",
                open_time_v": "07:05"
         */
        /*
         * ------------------------------------------
         *    当天最后一期时间
         *    {
                     "open_data": "45,53,26,78,49",
                     "action_no": "111024563",
                     "open_time": 1651420500,
                     "date": "2022-05-01",
                     "number": 203,
                     "open_time_value": "2022-05-01 23:55:00",
                     "open_time_v": "23:55"
               }
         * 
         */


        /// <summary>
        ///     通过时间。计算期号。
        /// </summary>
        /// <param name="time"></param>
        /// <returns>得到该时间的前一期数据, 也就该时间的最后一次开奖期号</returns>
        public static int getNextIssueId(DateTime time)
        {
            //time = DateTime.Parse("2022-06-27 06:11:00");
            int result = 0;
            DateTime firstDatetime = LxTimestampHelper.GetDateTime(firstTimestamp);
            var tmp_time = time;
            var ts = tmp_time - firstDatetime;
            //得到输入时间, 相对于起始时间, 过了多少天。
            var days = ts.Days;
            //firstDatetime = firstDatetime.AddDays(days);
            int temp_issue = firstIssueld + Convert.ToInt32(days) * count_real;
            int temp_count = 0;
            //得到该时间最后一期是当天第几期
            for (int i = 0; i < count_real; i++)
            {
                var f_timestamp = getOpenTimestamp(temp_issue + i);
                DateTime f_time = LxTimestampHelper.GetDateTime(f_timestamp);
                if (tmp_time > f_time)
                {
                    temp_count++;
                }
                else
                {
                    break;
                }
            }

            result = temp_issue + temp_count;


            return result;
        }

        /// <summary>
        ///     通过期号计算出改期的开奖时间戳
        /// </summary>
        public static long getOpenTimestamp(int issueld)
        {
            DateTime fristDatetime = LxTimestampHelper.GetDateTime(firstTimestamp);
            int days = getDays(issueld);
            int number = getNumber(issueld);
            DateTime nowDay = fristDatetime.AddDays(days);
            DateTime nowResult = nowDay.AddMinutes(5 * (number - 1));
            long timestamp = LxTimestampHelper.ToLong(nowResult);
            return timestamp;
        }

        public static DateTime getOpenDatetime(int issueId)
        {
            long timestamp = getOpenTimestamp(issueId);
            return GetDateTime(timestamp);
        }

        private static DateTime GetDateTime(long timeStamp)
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
        ///     通过期号. 得到该期号是当天的第几期, 
        ///     当天卡后, 从1开始 - 203结束
        /// </summary>
        /// <param name="issueld"></param>
        /// <returns></returns>
        public static int getNumber(int issueld)
        {
            //firstIssueId = 111024361;
            int result = 0;
            int value = issueld - firstIssueld;
            if (value >= 0)
            {
                // 0 % 203 = 0
                result = value % 203 + 1;
            }
            else
            {
                result = value % 203 + 1;
                result = 203 - Math.Abs(result);
            }
            return result;
        }

        /// <summary>
        ///     得到当前期号相对于基准期号的 天数
        /// </summary>
        /// <param name="issueld"></param>
        /// <returns></returns>
        public static int getDays(int issueld)
        {
            int result = 0;
            int value = issueld - firstIssueld;
            if (value >= 0)
            {
                result = value / 203;
            }
            else
            {
                result = value / 203;
            }
            return result;
        }
    }
}
