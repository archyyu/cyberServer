using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class WeekPrice
    {
        public UInt32 RuleId { get; set; }
        public UInt32 AreaId { get; set; }
        public UInt32 MemberType { get; set; }
        public UInt32 IgnoreTime { get; set; }
        public float StartPrice { get; set; }
        public float MinCostPrice { get; set; }
        public float[,] Price { get; set;}
        

        public float HourPrice(UInt32 now)
        {
            DateTime dt = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).AddSeconds(now);
            int day = dt.DayOfWeek == DayOfWeek.Sunday?6:(int)dt.DayOfWeek - 1;
            return this.Price[day,dt.Hour];
        }

        public void resetSurfUser(SurfUser surfUser,SurfPc surfPc,UInt32 now)
        {
            if (surfUser == null || surfPc == null)
            {
                return;
            }

            surfUser.AreaTypeId = surfPc.AreaTypeId;

            surfUser.RuleId = this.RuleId;
            surfUser.RuleValue = this.MinCostPrice;
            surfUser.IgnoreTime = this.IgnoreTime;
            surfUser.StartPrice = this.StartPrice;
            surfUser.HourPrice = this.HourPrice(now);
            surfUser.MinCostPrice = this.MinCostPrice;
            
        }

    }
}
