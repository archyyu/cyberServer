using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CashierLibrary.Model.Bill;

namespace CashierLibrary.Model.Bill
{
    public class SurfPc
    {

        public SurfPc()
        {
            lpSurfUser = null;
            AreaTypeId = 1;
            PcState = PcState.PC_ON_NOUSER;
            PcHeartTime = 0;
        }

        public string PcName { get; set; }
        public UInt32 AreaTypeId { get; set; }
        public PcState PcState { get; set; }
        public PcInfo PcInfo { get; set; }
        public SurfUser lpSurfUser { get; set; }                // 有人上机
        public UInt32 PcHeartTime { get; set; }

        public override string ToString()
        {
            return "pcName:" + PcName + ",areaType:" + AreaTypeId;
        }

    }
}
