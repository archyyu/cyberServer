using CashierLibrary.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class PeriodPrice
    {
        public UInt32 RuleId { get; set; }
        public UInt32 AreaId { get; set; }
        public UInt32 MemberType { get; set; }
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public float Price { get; set; }

        public UInt32 PeriodTime { get; set; }

        public UInt32 ByType { get; set; }

        public UInt32 TypeFlag { get; set; }
        public UInt32 Reserved { get; set; }

        public bool isIn(UInt32 now, bool isSmart = false)
        {

            // 是否智能包夜
            double begintime = (isSmart == true) ? StartTime : (StartTime - 0.2);

            float nowTime = this.timestampToFormat(now);

            if (StartTime < EndTime)
            {

                if (begintime < nowTime && nowTime < EndTime)
                {
                    return true;
                }
            }
            else
            {

                if (begintime < nowTime || nowTime < EndTime)
                {
                    return true;
                }
            }

            return false;
        }

        public UInt32 generateStartTime(UInt32 curTime)
        {

            float nowTime = this.timestampToFormat(curTime);

            if (StartTime - 0.2 <= nowTime)
            {
                return (UInt32)DateUtil.GetTimeStamp(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd ") + " 00:00:00").AddSeconds(60 * 60 * StartTime));
            }
            else
            {
                return (UInt32)DateUtil.GetTimeStamp(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd ") + " 00:00:00").AddSeconds(60 * 60 * EndTime)) - PeriodTime;
            }
            
            
        }

        private float timestampToFormat(UInt32 curTime)
        {
            DateTime dt = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddSeconds(curTime);
            float nowTime = dt.Hour + (float)dt.Minute / 60;
            return nowTime;
        }

        public UInt32 generateMaxEndTime(UInt32 now)
        {
            
            if (StartTime < EndTime)
            {
                return (UInt32)DateUtil.GetTimeStamp(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd ") + " 00:00:00").AddSeconds(60 * 60 * EndTime));
            }
            else
            {

                float nowTime = this.timestampToFormat(now);

                if (StartTime - 0.2 <= nowTime)
                {
                    return (UInt32)DateUtil.GetTimeStamp(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd ") + " 00:00:00").AddSeconds(60 * 60 * 24 +  60 * 60 * EndTime));
                }
                else
                {
                    return (UInt32)DateUtil.GetTimeStamp(DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd ") + " 00:00:00").AddSeconds(60 * 60 * EndTime));
                }

            }
            
        }


    }
}
