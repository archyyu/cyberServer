﻿using CashierLibrary.Util;
using CashierServer.Forms;
using CashierServer.HttpServer;
using CashierServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CashierServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
         
            Boolean createdNew;
            System.Threading.Mutex instance = new System.Threading.Mutex(true, "CashierServer", out createdNew);
            if (createdNew)
            {
                log4net.Config.XmlConfigurator.Configure();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                //处理未捕获的异常   
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                //处理UI线程异常   
                Application.ThreadException += Application_ThreadException;
                //处理非UI线程异常   
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                try
                {

                    //MysqlUtil.DeleteSqlProcess();
                    // 注册Mysq服务
                    CashierUtil.InstallMysql();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("注册mysql服务出错,错误：\n" + ex.Message);
                    return;
                }

                LoginForm loginForm = new LoginForm();
                if (loginForm.ShowDialog() == DialogResult.OK)
                {

                    Application.Run(new MainForm());
                }
                instance.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("您已经打开一个爱达服务器，请退出后打开");
                LogHelper.WriteLog("您已经打开一个爱达服务器，请退出后打开");
                Application.Exit();
            }


           
            
        }

        /// <summary>
        ///错误弹窗
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            string str;
            var strDateInfo = "出现应用程序未处理的异常：" + DateTime.Now + "\r\n";
            var error = e.Exception;
            if (error != null)
            {
                str = string.Format(strDateInfo + "异常类型：{0}\r\n异常消息：{1}\r\n异常信息：{2}\r\n",
                     error.GetType().Name, error.Message, error.StackTrace);
            }
            else
            {
                str = string.Format("应用程序线程错误:{0}", e);
            }

            LogHelper.WriteLog("【Application】:"+str,e.Exception);

            DataReportUtil dr = new DataReportUtil(ServerUtil.currentUser.shopid, "Server");
            dr.Report("【Application】:" + str, error.ToString());

            MessageBox.Show("发生错误，请查看程序日志！", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var error = e.ExceptionObject as Exception;
            var strDateInfo = "出现应用程序未处理的异常：" + DateTime.Now + "\r\n";
            var str = error != null ? string.Format(strDateInfo + "Application UnhandledException:{0};\n\r堆栈信息:{1}", error.Message, error.StackTrace) : string.Format("Application UnhandledError:{0}", e);
            LogHelper.WriteLog("【Application】:" + str,error);

            DataReportUtil dr = new DataReportUtil(ServerUtil.currentUser.shopid, "Server");
            dr.Report("【Application】:" + str, error.ToString());

            MessageBox.Show("发生错误，请查看程序日志！", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
