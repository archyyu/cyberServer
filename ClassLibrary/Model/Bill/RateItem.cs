using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class RateItem
    {

        public UInt32 AreaTypeId { get; set; }
        public UInt32 AreaOptions { get; set; }
        public UInt32 UserTypeId { get; set; }
        public float ExtraCharge { get; set; }
        public WeekPrice weekPrices { get; set; }
        public List<PeriodPrice> PeriodHourPrices { get; set; }
        public List<DurationPrice> DurationHourPrices{ get;set;}
    }
}
