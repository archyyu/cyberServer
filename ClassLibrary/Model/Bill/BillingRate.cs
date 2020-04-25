using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class BillingRate
    {

        public BillingRate()
        {
            this.weekPrices = new List<WeekPrice>();
            this.PeriodHourPrices = new List<PeriodPrice>();
            this.DurationHourPrices = new List<DurationPrice>();
            this.ExtraPrices = new List<ExtraPrice>();

            this.RatioBase = 1;
            this.RatioAward = 1;

            this.isChain = 2;

        }

        public bool IsChain()
        {
            return this.isChain == 1;
        }

        //基本配置信息
        public int gid { get; set; }
        public string name { get; set; }
        public int isChain { get; set; } //是否连锁，1：连锁 2：单店
        
        public int cashierflag { get; set; }

        //费率部分
        
        public UInt16 RatioBase { get; set; }
        public UInt16 RatioAward { get; set; }

        public float GirlRate { get; set; }

        public UInt32 LockTime { get; set; }
        public UInt32 ActiveTime { get; set; }
        public UInt32 ReLogin { get; set; }
        public UInt32 NeedActive { get; set; }
        public UInt32 SubmitAction { get; set; }
        public UInt32 DurationAction { get; set; }
        public UInt32 PeriodAction { get; set; }

        //自增id
        public Int64 memberId { get; set; }
        public Int64 rechargeOrderId { get; set; }
        public Int64 onlineId { get; set; }
        public Int64 dutyId { get; set; }

        public List<WeekPrice> weekPrices { get; set; }
        public List<PeriodPrice> PeriodHourPrices { get; set; }
        public List<DurationPrice> DurationHourPrices { get; set; }

        public List<ExtraPrice> ExtraPrices { get; set; }

    }
}
