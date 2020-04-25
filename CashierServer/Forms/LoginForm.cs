using CashierLibrary.AutoRun;
using CashierLibrary.Model;
using CashierLibrary.Util;
using CashierServer.Util;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CashierServer.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            this.MaximizeBox = false;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            this.Login();
        }

        /// <summary>
        /// 查询是否存在mySql服务，若没有，退出。
        /// 若有，检测服务状态，启动服务
        /// </summary>
        /// <returns></returns>
        private bool InitMySqlDB()
        {
            var services = ServiceController.GetServices();

            var server = services.FirstOrDefault(s => s.ServiceName.Equals("mysqlaida"));
            if (null != server)
            {
                if (!server.Status.Equals(ServiceControllerStatus.Running))
                {
                    server.Start();
                    server.WaitForStatus(ServiceControllerStatus.Running);
                }
                return true;
            }

            return false;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

		/// <summary>
		/// 窗口加载事件处理函数
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void LoginForm_Load(object sender, EventArgs e)
        {
            // 设置自动启动
            //AutoRunHelper.SetAutoRun("CashierServer", Application.ExecutablePath);

            // 读取配置文件并登陆
            String loginName = IniHelper.INIGetStringValue(IniUtil.file,"AIDA", "loginName", "");
			String loginPwd = IniHelper.INIGetStringValue(IniUtil.file, "AIDA", "loginPwd", "");
			LogHelper.WriteLog("读取配置文件，用户:【"+loginName+"】 用户密码：【"+loginPwd+"】");
			if ((!string.IsNullOrEmpty(loginName))&&(!String.IsNullOrEmpty(loginPwd)))
			{

				this.txbLoginName.Text = AesUtil.Decrypt(loginName);
				this.txbPassword.Text = AesUtil.Decrypt(loginPwd);
				this.Login();
			}

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

		private void txbPassword_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{

				switch (e.KeyCode)
				{
					case Keys.Enter:
						this.Login();
						break;

					default:
						break;
				}
			}
			catch (Exception ex)
			{
				LogHelper.WriteLog("登录错误:\n"+ex.Message);
			}
		}

        private Object loginObject = new object();

		private void Login()
		{
            lock (this.loginObject)
            {
                if (!this.InitMySqlDB())
                {
                    MessageBox.Show("未注册Mysqlaida服务,请注册后登陆");
                    return;
                }

                string loginname = this.txbLoginName.Text.Trim();
                string password = this.txbPassword.Text.Trim();
                if (loginname.Length <= 0)
                {
                    MessageBox.Show("登录名不能为空");
                    return;
                }

                if (password.Length <= 0)
                {
                    MessageBox.Show("密码不能为空");
                    return;
                }

                this.btnLogin.Enabled = false;
                this.btnLogin.Text = "登录中";

                IDictionary<string, string> parameters = HttpUtil.initParams();

                parameters.Add("loginname", loginname);
                parameters.Add("password", password);

                string result = "";
                try
                {
                    result = HttpUtil.doPost(ServerUtil.LoginUrl, parameters);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("网络错误");
                    LogHelper.WriteLog("登录错误", ex);
                    this.btnLogin.Text = "登录";
                    this.btnLogin.Enabled = true;
                    return;
                }

                JObject json = JObject.Parse(result);

                if (json["result"].ToString() == "success")
                {
                    String strLogName = AesUtil.Encrypt(loginname);
                    String strPwd = AesUtil.Encrypt(password);
                    // 登陆成功后写入文件
                    IniHelper.INIWriteValue(IniUtil.file, "AIDA", "loginName", strLogName);
                    IniHelper.INIWriteValue(IniUtil.file, "AIDA", "loginPwd", strPwd);

                    User user = JsonUtil.DeserializeJsonToObject<User>(json["user"].ToString());
                    ServerUtil.currentUser = user;
                    
					try
					{
						LogHelper.WriteLog("写入注册表，shopId" + user.shopid.ToString());
						RegistryUtil.setValue("shopId", user.shopid.ToString());
					}
					catch (Exception ex)
					{
						LogHelper.WriteLog("CashierServer:注册表信息出错",ex);
					}
					
                    if (IniUtil.password().Length <= 0 && IniUtil.user().Length <= 0)
                    {
                        this.btnLogin.Text = "初始化中";
                        CashierUtil.changeMysqlPsd();
                        DialogResult dr = MessageBox.Show("初始化成功");
                        if (dr == DialogResult.OK)
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    }
                    else
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }

                }
                else
                {
                    MessageBox.Show("账号或者密码错误");
                    this.btnLogin.Text = "登录";
                    this.btnLogin.Enabled = true;
                    return;
                }
            }
		}

        /// <summary>
        /// 取消按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCance_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
