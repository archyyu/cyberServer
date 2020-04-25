using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class DurationPrice
    {
        public UInt32 RuleId { get; set; }
        public UInt32 AreaId { get; set; }
        public UInt32 MemberType { get; set; }
        public UInt32 DurationTime { get; set; }
        public UInt32 ValidBeginTimestamp { get; set; }
        public UInt32 ValidEndTimestamp { get; set; }
        public float Price { get; set; }
        public UInt32 Reserved { get; set; }
    }
}
