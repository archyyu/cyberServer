using CashierLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashierServer.Util
{
    class ServerUtil
    {

        

#if DEBUG
        public static string ServerAddr = "weixin.10000ja.net";
        public static string YunAddr = "yun.aida58.com";
#else
        public static string ServerAddr = "weixin.10000ja.net";
        public static string YunAddr = "yun.aida58.com";
#endif

        // public static string testAddr = "127.0.0.18";

        public static string token = "";

        

        public static bool Debug = true;

        public static String VersionType()
        {
            if (Debug == true)
            {
				ServerAddr = "weixin1.10000ja.net";
				return "测试版本";
				
			}
            else
            {
				ServerAddr = "weixin.10000ja.net";
				return "发布版本";
				
			}
        }
        public static String LoginUrl = "http://" + ServerAddr + "/waterbar/cashier/user/login";

        public static String CashierMemberUrl = "http://" + ServerAddr + "/waterbar/cashier/member";

        #region 新增参数

        public static String syncNewUrl = "http://" + YunAddr + "/cashier/syncnew";

        public static String userUrl = "http://" + YunAddr + "/cashier/user";

        public static int yunErrorCount = 0;

        #endregion

        public static User currentUser;

    }
}
