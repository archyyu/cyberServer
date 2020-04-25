using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierServer.Model
{
    class RechargeOrderDTO
    {

        public long rechargeOrderID { get; set; }

        public int rechargeCompaignID { get; set; }

        public long memberID { get; set; }

        public byte rechargeWay { get; set; }

        public byte rechargeType { get; set; }

        public decimal cashBalance { get; set; }

        public decimal rechargeFee { get; set; }

        public decimal awardFee { get; set; }

        public byte state { get; set; }

        public int posAccount { get; set; }

        public DateTime rechargeDate { get; set; }

        public int dataVersion { get; set; }

        public byte rechargeSource { get; set; }

        public int gid { get; set; }

        public DateTime lastUpdateDate { get; set; }

        public long oldRechargeOrderID { get; set; }

        public int orderSource { get; set; }

        public int eventID { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("memberid:");
            sb.Append(memberID);
            sb.Append(",account:");
            sb.Append(posAccount);
            sb.Append(",Cash:");
            sb.Append(cashBalance);
            sb.Append(",base:");
            sb.Append(rechargeFee);
            sb.Append(",award:");
            sb.Append(awardFee);

            return sb.ToString();
        }
        
    }
}
