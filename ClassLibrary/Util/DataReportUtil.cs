using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Util
{
    public class DataReportUtil
    {

        private string url = "http://yun.aida58.com/cashier/notice/jsreport";

        private int shopid = 0;

        private string module = "server";

        private string key = "common*@@WanJia";

        public DataReportUtil(int shopid, string module)
        {
            this.shopid = shopid;
            this.module = module;
        }

        public void Report(string data, string msg)
        {
            IDictionary<string, string> param = HttpUtil.initParams();
            param.Add("shopid", this.shopid.ToString());
            param.Add("modulename", this.module);
            param.Add("data", data);
            param.Add("msg", msg);

            String result = HttpUtil.doPost(this.url, param);
            LogHelper.WriteLog(result);
        }

    }
}
