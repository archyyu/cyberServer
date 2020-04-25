using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashierLibrary.Model
{
    public class Member
    {

        public long memberID { get; set; }

        public String account { get; set; }

        public String activeTime { get; set; }

        public String address { get; set; }

        public String areaName { get; set; }

        public Int32 areaType { get; set; }

        public String awardBalance { get; set; }

        public String bRoomOwner { get; set; }

        public String baseBalance { get; set; }

        public String birthday { get; set; }

        public String cashBalance { get; set; }
        
        public String identifyNum { get; set; }

        public String machineName { get; set; }

        public String openID { get; set; }
        

        public String memberName { get; set; }

        public Int32 memberType { get; set; }

        public String memberTypeDesc { get; set; }

        public String onlineFee { get; set; }

        public Int64 onlineStartTime { get; set; }

        public String phone { get; set; }

        public String maxEndTime { get; set; }

    }
}
