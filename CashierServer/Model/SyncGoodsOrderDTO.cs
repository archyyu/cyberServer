using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierServer.Model
{
    public class SyncGoodsOrderDTO
    {
        private long goodsOrderID;

        private int sync;

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

        public int Sync
        {
            get
            {
                return sync;
            }

            set
            {
                sync = value;
            }
        }
    }
}
