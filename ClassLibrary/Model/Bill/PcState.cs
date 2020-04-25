using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public enum PcState
    {
        PC_ON_NOUSER = 0,
        PC_ON_USER = 1,
        PC_OFF = 2,
        PC_ORDER = 3,
    }
}
