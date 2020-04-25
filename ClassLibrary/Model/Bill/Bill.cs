using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class Bill
    {
        public Int32 billingID { get; set; }
        public UInt32 gid { get; set; }
        public UInt32 lastCostTimestamp { get; set; }
        public UInt64 memberID { get; set; }
        
        public float currentCostBase { get; set; }
        public float currentCostAward { get; set; }
        public float currentCostTemp { get; set; }
        
    }
}
