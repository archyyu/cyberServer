using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model.Bill
{
    public class PcInfo
    {

        public PcInfo()
        {
            Gid = 0;
            MachineId = 0;
            AreaId = 0;
            State = 0;
            DataVersion = 0;
            Mac = "";
            Ip = "";
            IpMask = "";
            MachineName = "";
        }

        public UInt32 Gid { get; set; }                   //## 网吧GID
        public int MachineId { get; set; }              //## 机器ID
        public int AreaId { get; set; }                 //## 区域ID
        public int State { get; set; }
        public int DataVersion { get; set; }
        public string Mac { get; set; }               //## 机器MAC 
        public string Ip { get; set; }                //## 机器IP
        public string IpMask { get; set; }            //## 子网掩码
        public string MachineName { get; set; }	    //## 机器名称
    }
}
