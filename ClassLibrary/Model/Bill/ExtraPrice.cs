using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class ExtraPrice
    {
        public UInt32 RuleId { get; set; }
        public UInt32 AreaTypeId { get; set; }
        public UInt32 MemberTypeId { get; set; }
        public float AdditionalPrice { get; set; }
    }
}
