using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public enum BillingEvent
    {
        BC_USER_COST = 1,
        BC_USER_LACKMONEY = 2,
        BC_USER_FORCE_REMOVE = 3,
        BC_USER_ISCHANGE_WEEK = 4,
        BC_SETTING_PERIOD_COMING = 5,
    }
}
