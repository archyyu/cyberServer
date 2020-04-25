using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class Billing
    {

        public static readonly UInt32 DB_DEFAULT_USERTYPE_ID = 1;   //临时用户
        public static readonly UInt32 PAYMODE_ACCOUNT = 5;   //账户支付
        
        public static readonly string WX_LOGIN_KEY =  "f#SDTgvlVc7WZOCq522D$OsW";
        
        public static readonly UInt32 LOGIN_TYPE_PASSPORT = 1;
        public static readonly UInt32 LOGIN_TYPE_WX = 0;
        

        public static readonly UInt32 PC_CHECK_INTERVAL = 30000;   // pc检测周期间隔（s）
        public static readonly UInt32 PC_ALIVE_TIME_THRESHOLD = 180;     // pc存活心跳的最大间隔 3min（s）
        public static readonly UInt32 PING_TIMEOUT = 50;      // ping 超时设置(ms)
    }
}
