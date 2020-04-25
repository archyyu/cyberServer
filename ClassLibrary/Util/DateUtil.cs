using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CashierLibrary.Util
{
    public class DateUtil
    {
        /// <summary>
        /// 字符串日期转DateTime
        /// </summary>
        /// <param name="strDateTime">字符串日期</param>
        /// <returns></returns>
        public static DateTime TransStrToDateTime(string strDateTime)
        {
            DateTime now;
            string[] format = new string[]
            {
            "yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss", "yyyy年MM月dd日 HH时mm分ss秒",
            "yyyyMdHHmmss","yyyy年M月d日 H时mm分ss秒", "yyyy.M.d H:mm:ss", "yyyy.MM.dd HH:mm:ss","yyyy-MM-dd","yyyyMMdd"
            ,"yyyy/MM/dd","yyyy/M/d"
            };
            if (DateTime.TryParseExact(strDateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out now))
            {
                return now;
            }
            return DateTime.MinValue;
        }

        public static string tmToFormat(UInt32 secs)
        {
            DateTime dt = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddSeconds(secs);
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetFormatByTime10(string timeStamp)
        {
            return GetDateTime(timeStamp.PadLeft(10, '0')).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetFormatByTime13(string timeStamp)
        {
            return GetDateTime(timeStamp.PadLeft(13, '0')).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 时间戳格式化
        /// </summary>
        /// <param name="timeStamp">10或13</param>
        /// <returns></returns>
        public static DateTime GetDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp.PadRight(17, '0'));
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        public static string dtToFormat(DateTime dt)
        {
            return String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

        }

        public static double GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ts.TotalSeconds;
        }

        public static double GetTimeStamp(DateTime cu)
        {
            TimeSpan ts = cu.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ts.TotalSeconds;
        }

        public static string GetTimeStampStr()
        {
            return ConvertDateTimeToInt(DateTime.Now).ToString();
        }

        public static long ConvertDateTimeToInt(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            return (time.Ticks - startTime.Ticks) / 10000;
        }
    }
}
