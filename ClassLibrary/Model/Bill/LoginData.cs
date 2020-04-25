using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class LoginData
    {
        
        public LoginData()
        {
            LoginType = Billing.LOGIN_TYPE_PASSPORT;
            Timestamp = 0;
            MemberId = 0;
        }

        public string Account { get; set; }
        public string PcName { get; set; }
        public string Password { get; set; }
        public UInt32 LoginType { get; set; }
        public UInt32 Timestamp { get; set; }
        public UInt64 MemberId { get; set; }
    }
}
