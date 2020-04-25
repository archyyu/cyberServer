using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CashierServer.Util
{
    public class AIDAServerUtil
    {

        [DllImport(@"AIDA_SERVER.dll",CallingConvention = CallingConvention.Cdecl,CharSet =CharSet.Unicode)]
        public static extern int WJStart(String gid,String pwd);
        

    }
}
