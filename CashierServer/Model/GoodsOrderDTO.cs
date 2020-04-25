using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierServer.Model
{
    public class GoodsOrderDTO
    {

        private long goodsOrderID;

        private long memberID;

        private decimal goodsSum;
        

        private byte orderType;

        private decimal orderFee;

        private decimal couponDiscoutFee;

        private decimal paySum;

        private decimal basePayFee;

        private byte state;

        private DateTime createTime;

        private byte source;

        private int areaId;

        private int machineID;

        private int actorUser;

        private DateTime actTime;

        private String reason;

        private int returnActorUse;

        private DateTime returnTime;

        private int authorise;

        private int dataVersion;

        private int gid;

        private byte payWay;

        private int creator;

        private String goodsOrderDesc;

        public long GoodsOrderID
        {
            get
            {
                return goodsOrderID;
            }

            set
            {
                goodsOrderID = value;
            }
        }

        public long MemberID
        {
            get
            {
                return memberID;
            }

            set
            {
                memberID = value;
            }
        }

        public decimal GoodsSum
        {
            get
            {
                return goodsSum;
            }

            set
            {
                goodsSum = value;
            }
        }

        public byte OrderType
        {
            get
            {
                return orderType;
            }

            set
            {
                orderType = value;
            }
        }

        public decimal OrderFee
        {
            get
            {
                return orderFee;
            }

            set
            {
                orderFee = value;
            }
        }

        public decimal CouponDiscoutFee
        {
            get
            {
                return couponDiscoutFee;
            }

            set
            {
                couponDiscoutFee = value;
            }
        }

        public decimal PaySum
        {
            get
            {
                return paySum;
            }

            set
            {
                paySum = value;
            }
        }

        public decimal BasePayFee
        {
            get
            {
                return basePayFee;
            }

            set
            {
                basePayFee = value;
            }
        }

        public byte State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
            }
        }

        public DateTime CreateTime
        {
            get
            {
                return createTime;
            }

            set
            {
                createTime = value;
            }
        }

        public byte Source
        {
            get
            {
                return source;
            }

            set
            {
                source = value;
            }
        }

        public int AreaId
        {
            get
            {
                return areaId;
            }

            set
            {
                areaId = value;
            }
        }

        public int MachineID
        {
            get
            {
                return machineID;
            }

            set
            {
                machineID = value;
            }
        }

        public int ActorUser
        {
            get
            {
                return actorUser;
            }

            set
            {
                actorUser = value;
            }
        }

        public DateTime ActTime
        {
            get
            {
                return actTime;
            }

            set
            {
                actTime = value;
            }
        }

        public string Reason
        {
            get
            {
                return reason;
            }

            set
            {
                reason = value;
            }
        }

        public int ReturnActorUse
        {
            get
            {
                return returnActorUse;
            }

            set
            {
                returnActorUse = value;
            }
        }

        public DateTime ReturnTime
        {
            get
            {
                return returnTime;
            }

            set
            {
                returnTime = value;
            }
        }

        public int Authorise
        {
            get
            {
                return authorise;
            }

            set
            {
                authorise = value;
            }
        }

        public int DataVersion
        {
            get
            {
                return dataVersion;
            }

            set
            {
                dataVersion = value;
            }
        }

        public int Gid
        {
            get
            {
                return gid;
            }

            set
            {
                gid = value;
            }
        }

        public byte PayWay
        {
            get
            {
                return payWay;
            }

            set
            {
                payWay = value;
            }
        }

        public int Creator
        {
            get
            {
                return creator;
            }

            set
            {
                creator = value;
            }
        }

        public string GoodsOrderDesc
        {
            get
            {
                return goodsOrderDesc;
            }

            set
            {
                goodsOrderDesc = value;
            }
        }
    }
}
