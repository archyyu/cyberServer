using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class AreaItem
    {

        public AreaItem()
        {

        }

        public UInt32 AreaId { get; set; }
        public string AreaName { get; set; }
        public UInt32 RoomType { get; set; }
        public string memberTypeList { get; set; }

    }
}
