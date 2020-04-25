using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public enum CostType
    {
        COST_TYPE_FREE = 0,
        COST_TYPE_WEEK = 1, //标准计费
        COST_TYPE_PERIOD = 2, //包时段
        COST_TYPE_DURATION = 3, //包时长

        COST_TYPE_ROOM_WEEK = 4,//包房标准
        COST_TYPE_ROOM_PERIOD = 5,//包房包时段
        COST_TYPE_ROOM_DURATION = 6,//包房包时长
        
    }
}
