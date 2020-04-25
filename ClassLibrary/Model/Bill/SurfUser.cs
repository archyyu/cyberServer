using CashierLibrary.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class SurfUser
    {

        public SurfUser()
        {
            MemberId = 0; 
            LogoffTime = 0;
            Gid = 0;
            PcName = "";
            Password = "";
            Account = "";
            CashierID = 0;
            PayWay = Billing.PAYMODE_ACCOUNT;
            Flags = 0;
            Account = "";
            MemberId = 0;
            AreaTypeId = 0;
            MemberTypeId = 0;
            OnlineID = 0;


            this.CostType = CostType.COST_TYPE_FREE;
            this.TempBalance = 0;
            this.BaseBalance = 0;
            this.AwardBalance = 0;

            this.LogonTimestamp = 0;
            this.NextCostTimestamp = 0;
            this.WholeTimestamp = 0;

            this.CurrentCostBase = 0;
            this.CurrentCostAward = 0;
            this.CurrentCostTemp = 0;
            this.ExtraCharge = 0;

            this.Discount = 1;
            
        }

        public string Account{get;set;}
        public UInt64 MemberId{get;set;}

        public UInt32 LogoffTime{get;set;}
        public UInt32 ActiveTime{get;set;}                 // 开卡激活时间
        
        public string PcName{get;set;}
        public string MemberName{get;set;}

        public string MemberTypeName{get;set;}
        public string Password{get;set;}

        public UInt32 Gid { get; set; }

        public UInt32 OnlineID { get; set; }

        public string Phone{get;set;}
        public string QQ{get;set;}
        public UInt32 Sex{get;set;}
        public UInt32 IdType{get;set;}
        public string IdValue{get;set;}
        public string OpenID{get;set;}
        public string Birthday{get;set;}
        public int ProvinceId{get;set;}
        public int CityId{get;set;}
        public int DistrictID{get;set;}
        public string Address{get;set;}
        public long lOnlineRoomSeq{get;set;}
        public UInt32 CashierID{get;set;}
        public UInt32 PayWay{get;set;}


        public UInt32 Flags { get; set; }
        
        public UInt32 AreaTypeId { get; set; }                         // 上机区域id
        public UInt32 MemberTypeId { get; set; }                         // 用户类型id
        public CostType CostType { get; set; }                           // 扣费类型
        public float TempBalance { get; set; }                        // 临时账户（一次性消费，优惠券转换 或者 押金账户（找零））
        public float BaseBalance { get; set; }                        // 本金账户
        public float AwardBalance { get; set; }                       // 充送金账户
        public UInt16 RatioCostBase { get; set; }                      // 本金扣费占比
        public UInt16 RatioCostAward { get; set; }                     // 充送扣费占比
        public float Discount { get; set; }                           // 折扣
        public UInt32 LogonTimestamp { get; set; }                     // 上机时间

        public float PeriodStartTime { get; set; }                    // 包时段起始时间
        public float PeriodEndTime { get; set; }                      // 包时段结束时间
        public UInt32 RuleId { get; set; }                             // 包时段或包时长规则id标识
        public float RuleValue { get; set; }                          // 对应规则的取值

        public UInt32 DurationTime { get; set; }                    //包时段或者包时长，持续时间
        public UInt32 RoomId { get; set; }                             // 缺省 0 如果是包间用户, =AreaTypeId

        // out

        public CostState CostState { get; set; }                          // 扣费状态

        public UInt32 LastCostTimestamp { get; set; }                  // 上次扣费时间
        public UInt32 NextCostTimestamp { get; set; }                  // 下次扣费时间
        public UInt32 MaxEndTimestamp { get; set; }                    // 最大可上机时间

        public float CurrentCostBase { get; set; }                    // 当前本金扣费金额
        public float CurrentCostAward { get; set; }                   // 当前充送扣费金额
        public float CurrentCostTemp { get; set; }                    // 当前临时账户扣费金额

        public float ExtraCharge { get; set; }                        // 本次上机附加费

        public float AllHadCost { get; set; }                         // 本次上机累积扣费总额
        public UInt32 IgnoreTime { get; set; }                         // 忽略时间
        public float StartPrice { get; set; }                         // 起步价
        public float HourPrice { get; set; }                          // 标准单价
        public float MinCostPrice { get; set; }                   //最小扣费

        // assist, reload need
        public UInt32 WholeTimestamp { get; set; }                     // 整点标记

        public bool RoomOwner { get; set; }                          // 是否是包房主扣卡用户，依赖 RoomId
        public UInt32 BalanceMode { get; set; }                        // 账户模式
        public UInt32 Reserved { get; set; }                           // 保留

        private List<Bill> BillList = new List<Bill>();

        public void addBill(Bill bill)
        {
            BillList.Add(bill);
        }

        public void emptyBill()
        {
            BillList = new List<Bill>();
        }

        public float calculateCostSum(UInt32 from)
        {
            float sum = 0;
            foreach (Bill bill in BillList)
            {
                if (bill.lastCostTimestamp >= from)
                {
                    sum += (bill.currentCostBase + bill.currentCostAward + bill.currentCostTemp);
                }
            }
            return sum;
        }

        public float remain()
        {
            return (this.TempBalance + this.BaseBalance + this.AwardBalance);
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("memberid:");
            sb.Append(MemberId);
            sb.Append(",account:");
            sb.Append(Account);
            sb.Append(",costType:");
            sb.Append(this.CostType.ToString());
            sb.Append(",ignoreTime:");
            sb.Append(IgnoreTime);
            sb.Append(",pcName:");
            sb.Append(PcName);
            sb.Append(",startPrice:");
            sb.Append(StartPrice);
            sb.Append(",hasCost:");
            sb.Append(AllHadCost);
            sb.Append(",nowCost:");
            sb.Append("base:");
            sb.Append(CurrentCostBase);
            sb.Append(",award:");
            sb.Append(CurrentCostAward);
            sb.Append(",temp:");
            sb.Append(CurrentCostTemp);
            sb.Append(",discount:");
            sb.Append(Discount);
            sb.Append(",nextCostTime:");
            sb.Append(DateUtil.tmToFormat(NextCostTimestamp));
            sb.Append(",maxEndTime:");
            sb.Append(DateUtil.tmToFormat(MaxEndTimestamp));
            sb.Append(",remain:");
            sb.Append(TempBalance + BaseBalance + AwardBalance);

            return sb.ToString();
        }

    }
}
