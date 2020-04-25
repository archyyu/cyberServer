
using CashierLibrary.Util;
using CashierServer.Forms;
using CashierServer.Forms.Tips;
using CashierServer.HttpServer;
using CashierServer.Logic;
using CashierServer.Logic.Surf;
using CashierServer.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Timers;
using System.Windows.Forms;


namespace CashierServer
{
	public partial class MainForm : Form
    {

        private System.Timers.Timer deamonMysqlTimer;
        
        private Thread httpThread;

        private Thread surfHttpThread;

        private Thread billingThread;

        private Thread checkThread;

        private Thread asyncThread;

        private SurfLogic surfLogic = new SurfLogic();
        
        private delegate void ShowTips();

        public bool m_isNeedUpdate = false;

        private UpdateTipForm m_frm = new UpdateTipForm();

        // 新同步机制调整
        private SyncLogic syncThread = new SyncLogic();

        private AsynLogic asyncLogic = new AsynLogic();

        public UInt32 state { get; set; }

        public delegate void InvokeDelegate();
        
        public MainForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Normal;
            this.MaximizeBox = false;
            Control.CheckForIllegalCrossThreadCalls = false;
            state = 0;
        }
        

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private void MainForm_Load(object sender, EventArgs e)
        {
            string version = Application.ProductVersion.ToString();

            this.Text = ":V "+ version;

            LogHelper.WriteLog("当前服务器版本号:" + version);

            //判断是否应该进入停电模式
            UInt32 lastTick = IniUtil.getLastTick();
            if (lastTick != 0)
            {
                UInt32 currentTime = (UInt32)HttpUtil.GetTimeStamp();

                if ((currentTime - lastTick) > 30 * 60)
                {
                    DialogResult dr = MessageBox.Show("进入停电模式，将不再扣费，并且需要给所有人结账", "是否进入停电模式", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        state = 1;
                    }
                    else
                    {
                        state = 0;
                    }
                }

            }

            if (state == 0)
            {
                this.runStateLbl.Text = "正常模式";
            }
            else
            {
                this.runStateLbl.Text = "停电模式中";
            }

            

            string configStr = "";

            try
            {
                CashierUtil.initMysql();
                CashierUtil.CheckIfExistsTableSyncNetbarGoodsOrder();

                // 检测数据库是否更新
                if (CashierUtil.CheckIfUpdateMysql())
                {   
                    CashierUtil.closeMysql();
                    LogHelper.WriteLog("数据库更新有异常,请检查");
                    MessageBox.Show("数据库更新有异常,请联系技术");
                    Application.Exit();
                }

                configStr = CashierUtil.loadConfig();
                CashierUtil.processPcList();
                CashierUtil.closeMysql();
                LogHelper.WriteLog("加载配置完成");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("更新配置出现错误", ex);
            }

            try
            {
                this.surfLogic.gid =(UInt32)ServerUtil.currentUser.shopid;
                this.surfLogic.InitSurfLogic(configStr);
                this.surfLogic.mainForm = this;
                this.surfLogic.state = state;
                
                SurfHttpServer surfHttpServer = new SurfHttpServer(18000);
                surfHttpServer.surfLogic = this.surfLogic;
                surfHttpThread = new Thread(surfHttpServer.listen);
                surfHttpThread.Start();

                LogHelper.WriteLog("启动SurfHttp Server");

                CashierHttpServer httpServer = new CashierHttpServer(17000);
                httpServer.surfLogic = this.surfLogic;
                httpThread = new Thread(httpServer.listen);
                httpThread.IsBackground = true;
                httpThread.Start();

                LogHelper.WriteLog("启动 Cashier Http Server");

                billingThread = new Thread(this.surfLogic.tick);
                billingThread.Start();

                LogHelper.WriteLog("启动 扣费线程");

                checkThread = new Thread(this.surfLogic.check);
                checkThread.Start();

                LogHelper.WriteLog("启动定时下机 线程");

                //// 原同步线程
                //asyncThread = new Thread(this.asyncLogic.tick);
                //asyncThread.Start();

                // 开启新同步线程
                syncThread.start();

                LogHelper.WriteLog("启动同步线程");
                
            }
            catch (System.Exception ex)
            {
                LogHelper.WriteLog("err",ex);
            }

            this.netbarNameLbl.Text = this.surfLogic.billingRate.name;

            this.initTimer();
            
		}

        public void setNetStateLbl(String content)
        {
            this.netStateLbl.Text = content;
        }

        private void initTimer()
        {
            //守候mysql定时器
            this.deamonMysqlTimer = new System.Timers.Timer();
            this.deamonMysqlTimer.Enabled = true;
            this.deamonMysqlTimer.Interval = 3 * 1000;
            this.deamonMysqlTimer.Elapsed += this.deamonMysqlService;
            this.deamonMysqlTimer.Start();

        }
        
        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.notifyIcon.Visible = false;
            this.ShowInTaskbar = true;
            this.Activate();
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
            {
                return;
            }
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            this.notifyIcon.Visible = true;
            this.Hide();
            return;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon.Visible = true;
            }
        }

        
        private void deamonMysqlService(object sender, EventArgs e)
        {
            this.BeginInvoke(new InvokeDelegate(this.checkMysqlService));
		}
        
        private void checkMysqlService()
        {
            try
            {
                CashierUtil.BisStartSqlService();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("Server fail", ex);
            }
        }

        private void aboutBtn_Click(object sender, EventArgs e)
        {

            string dir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            System.Diagnostics.Process update = new System.Diagnostics.Process();
            update.StartInfo.FileName = dir + "\\AutoUpdate.exe";
            update.Start();
        }
        
		
        
        private String GetUpdateServerTime()
        {
            try
            {

                IDictionary<String, Object> requestBody = new Dictionary<String, Object>();
                IDictionary<string, string> checkParams = HttpUtil.initParams();
                foreach (KeyValuePair<string, string> kv in checkParams)
                {
                    requestBody.Add(kv.Key, kv.Value);
                }

                String body = JsonUtil.SerializeObject(requestBody);
                // 拉取配置
                String response = HttpUtil.doPost(IniUtil.UpdateConfigUrl(), body);

                return response;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("error", ex);
            }
            return "";
        }

        /// <summary>
        /// 显示提示
        /// </summary>
        private void ShowUpdateTip()
        {  
            m_frm.Show();
            this.m_isNeedUpdate = false;
        }

        private DateTime GetLocalUpDateTime()
        {
            string localXmlFile = Application.StartupPath + "\\UpdateList.xml";
            XmlFiles localXmlFiles = new XmlFiles(localXmlFile);
            String strData = localXmlFiles.GetNodeValue("//LastUpdateTime");
            DateTime dt = DateTime.Parse(strData);
            return dt;
        }
        
        private void cashierConfig_Click(object sender, EventArgs e)
        {

            System.Diagnostics.Process[] processList = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process process in processList)
            {
                if (process.ProcessName.ToUpper() == "网费收银端")
                {
                    process.Kill();
                }
            }
            
            string dir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "";
            System.Diagnostics.Process cashier = new System.Diagnostics.Process();
            cashier.StartInfo.FileName = dir + "\\Cashier\\网费收银端.exe";
            cashier.Start();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
            DialogResult dr = MessageBox.Show("退出则无法享受上网服务器，确定要退出吗?", "退出系统", messButton);
            if (dr == DialogResult.OK)//如果点击“确定”按钮
            {
                this.Dispose();
                Application.Exit();
                System.Environment.Exit(0);
            }
            else//如果点击“取消”按钮
            {
                
            }
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.notifyIcon.Visible = false;
            this.ShowInTaskbar = true;
            this.Activate();
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
    }
}
