using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using CashierLibrary.Util;

namespace CashierLibrary.Util
{
    public class RegistryUtil
    {

        public static void setValue(String key, String value)
        {
            try
            {
                RegistryKey rkey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                RegistryKey software = rkey.OpenSubKey("software\\aida", true); //该项必须已存在
                if (software == null)
                {
                    software = rkey.CreateSubKey("software\\aida");
                }
                software.SetValue(key, value);
                rkey.Close();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("没有读写注册表的权限",ex);
            }
         
        }

        public static string getValue(String k)
        {
            string info = "";
            RegistryKey Key;
            Key = Registry.LocalMachine;
            RegistryKey myreg = Key.OpenSubKey("software\\aida"); 
            info = myreg.GetValue(k).ToString();
            myreg.Close();
            return info;
        }

    }
}
