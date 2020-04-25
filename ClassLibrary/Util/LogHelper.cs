using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashierLibrary.Util
{
    public class LogHelper
    {

        public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");

        public static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("logerror");

        public static void WriteLog(string info)
        {

            if (info == null || loginfo == null)
            {
                return ;
            }

            try
            {

                if (loginfo.IsInfoEnabled)
                {
                    //Debug.WriteLine(info);
                    loginfo.Info(info);
                }
            }
            catch (Exception )
            {
                //Debug.WriteLine(ex.ToString());
            }
        }
        

        public static void WriteLog(string info, Exception se)
        {
            if (info == null || se == null || logerror == null)
            {
                return ;
            }

            try
            {
                if (logerror.IsErrorEnabled)
                { 
                    logerror.Error(info, se);
                }
            }
            catch (Exception )
            {
                //Debug.WriteLine(ex.ToString());
            }
        }
    }
}
