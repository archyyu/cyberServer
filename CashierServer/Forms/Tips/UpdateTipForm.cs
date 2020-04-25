using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CashierServer.Forms.Tips
{
    public partial class UpdateTipForm : Form
    {

        [DllImport("user32")]
        private static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);
        //下面是可用的常量，根据不同的动画效果声明自己需要的   
        private const int AW_HOR_POSITIVE = 0x0001;//自左向右显示窗口，该标志可以在滚动动画和滑动动画中使用。使用AW_CENTER标志时忽略该标志   
        private const int AW_HOR_NEGATIVE = 0x0002;//自右向左显示窗口，该标志可以在滚动动画和滑动动画中使用。使用AW_CENTER标志时忽略该标志   
        private const int AW_VER_POSITIVE = 0x0004;//自顶向下显示窗口，该标志可以在滚动动画和滑动动画中使用。使用AW_CENTER标志时忽略该标志   
        private const int AW_VER_NEGATIVE = 0x0008;//自下向上显示窗口，该标志可以在滚动动画和滑动动画中使用。使用AW_CENTER标志时忽略该标志该标志   
        private const int AW_CENTER = 0x0010;//若使用了AW_HIDE标志，则使窗口向内重叠；否则向外扩展   
        private const int AW_HIDE = 0x10000;//隐藏窗口   
        private const int AW_ACTIVE = 0x20000;//激活窗口，在使用了AW_HIDE标志后不要使用这个标志   
        private const int AW_SLIDE = 0x40000;//使用滑动类型动画效果，默认为滚动动画类型，当使用AW_CENTER标志时，这个标志就被忽略   
        private const int AW_BLEND = 0x80000;//使用淡入淡出效果 

        public UpdateTipForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 确认事件处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                string dir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                System.Diagnostics.Process update = new System.Diagnostics.Process();
                update.StartInfo.FileName = dir + "\\AutoUpdate.exe";
                update.Start();
                this.Hide();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("升级失败,失败原因:\n"+ex.Message);
            }
        }
        /// <summary>
        /// 取消升级
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        /// <summary>
        /// 加载右下角小图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTipForm_Load(object sender, EventArgs e)
        {
            int x = Screen.PrimaryScreen.WorkingArea.Right - this.Width;
            int y = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;
            this.Location = new Point(x, y);//设置窗体在屏幕右下角显示   
            AnimateWindow(this.Handle, 1000, AW_SLIDE | AW_ACTIVE | AW_VER_NEGATIVE);

            this.ShowInTaskbar = false;
            Control.CheckForIllegalCrossThreadCalls = false;

        }
    }
}
