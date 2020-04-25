using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class ActiveData
    {
        
        public ActiveData()
        {
            PayWay = Billing.PAYMODE_ACCOUNT;
            CashBalance = 0;
            CashierId = 0;
            PeriodStartTime = 0;
            PeriodEndTime = 0;
            RuleValue = 0;
            RuleId = 0;
            AreaTypeId = 0;
            CostType = CostType.COST_TYPE_WEEK;
            PcName = "";
            MemberId = 0;
        }


        public string Account { get; set; }
        public string PcName { get; set; }
        public CostType CostType { get; set; }
        public UInt32 AreaTypeId { get; set; }
        public bool RoomOwner { get; set; }
        public UInt32 RuleId { get; set; }
        public float RuleValue;
        public float PeriodStartTime;
        public float PeriodEndTime;
        public UInt32 PayWay;
        public float CashBalance;                  // 押金模式所付金额
        public UInt32 CashierId;
        public UInt64 MemberId;
    }
}
