using CashierServer.Util;

namespace CashierServer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.显示ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.netbarNameLbl = new System.Windows.Forms.Label();
            this.runStateLbl = new System.Windows.Forms.Label();
            this.aboutBtn = new System.Windows.Forms.Button();
            this.cashierConfig = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.netStateLbl = new System.Windows.Forms.Label();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "智慧网咖服务器";
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.显示ToolStripMenuItem,
            this.退出ToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(137, 48);
            // 
            // 显示ToolStripMenuItem
            // 
            this.显示ToolStripMenuItem.Name = "显示ToolStripMenuItem";
            this.显示ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.显示ToolStripMenuItem.Text = "显示主页面";
            this.显示ToolStripMenuItem.Click += new System.EventHandler(this.显示ToolStripMenuItem_Click);
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.退出ToolStripMenuItem.Text = "退出";
            this.退出ToolStripMenuItem.Click += new System.EventHandler(this.退出ToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(149, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "网吧名称：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(137, 130);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "服务器模式：";
            // 
            // netbarNameLbl
            // 
            this.netbarNameLbl.AutoSize = true;
            this.netbarNameLbl.BackColor = System.Drawing.Color.Transparent;
            this.netbarNameLbl.ForeColor = System.Drawing.Color.White;
            this.netbarNameLbl.Location = new System.Drawing.Point(233, 48);
            this.netbarNameLbl.Name = "netbarNameLbl";
            this.netbarNameLbl.Size = new System.Drawing.Size(89, 12);
            this.netbarNameLbl.TabIndex = 12;
            this.netbarNameLbl.Text = "北京吧友南门店";
            // 
            // runStateLbl
            // 
            this.runStateLbl.AutoSize = true;
            this.runStateLbl.BackColor = System.Drawing.Color.Transparent;
            this.runStateLbl.ForeColor = System.Drawing.Color.White;
            this.runStateLbl.Location = new System.Drawing.Point(233, 130);
            this.runStateLbl.Name = "runStateLbl";
            this.runStateLbl.Size = new System.Drawing.Size(29, 12);
            this.runStateLbl.TabIndex = 13;
            this.runStateLbl.Text = "正常";
            // 
            // aboutBtn
            // 
            this.aboutBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(173)))), ((int)(((byte)(221)))));
            this.aboutBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.aboutBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(186)))), ((int)(((byte)(224)))));
            this.aboutBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(162)))), ((int)(((byte)(212)))));
            this.aboutBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(173)))), ((int)(((byte)(221)))));
            this.aboutBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.aboutBtn.ForeColor = System.Drawing.Color.White;
            this.aboutBtn.Location = new System.Drawing.Point(247, 223);
            this.aboutBtn.Name = "aboutBtn";
            this.aboutBtn.Size = new System.Drawing.Size(75, 23);
            this.aboutBtn.TabIndex = 17;
            this.aboutBtn.Text = "在线升级";
            this.aboutBtn.UseVisualStyleBackColor = false;
            this.aboutBtn.Click += new System.EventHandler(this.aboutBtn_Click);
            // 
            // cashierConfig
            // 
            this.cashierConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(173)))), ((int)(((byte)(221)))));
            this.cashierConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cashierConfig.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(186)))), ((int)(((byte)(224)))));
            this.cashierConfig.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(162)))), ((int)(((byte)(212)))));
            this.cashierConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(173)))), ((int)(((byte)(221)))));
            this.cashierConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cashierConfig.ForeColor = System.Drawing.Color.White;
            this.cashierConfig.Location = new System.Drawing.Point(105, 223);
            this.cashierConfig.Name = "cashierConfig";
            this.cashierConfig.Size = new System.Drawing.Size(75, 23);
            this.cashierConfig.TabIndex = 21;
            this.cashierConfig.Text = "启动收银台";
            this.cashierConfig.UseVisualStyleBackColor = false;
            this.cashierConfig.Click += new System.EventHandler(this.cashierConfig_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(147, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 22;
            this.label3.Text = "网络连接：";
            // 
            // netStateLbl
            // 
            this.netStateLbl.AutoSize = true;
            this.netStateLbl.ForeColor = System.Drawing.Color.Transparent;
            this.netStateLbl.Location = new System.Drawing.Point(233, 89);
            this.netStateLbl.Name = "netStateLbl";
            this.netStateLbl.Size = new System.Drawing.Size(29, 12);
            this.netStateLbl.TabIndex = 23;
            this.netStateLbl.Text = "正常";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(168)))), ((int)(((byte)(214)))));
            this.BackgroundImage = global::CashierServer.Properties.Resources.bj;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(431, 274);
            this.Controls.Add(this.netStateLbl);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cashierConfig);
            this.Controls.Add(this.aboutBtn);
            this.Controls.Add(this.runStateLbl);
            this.Controls.Add(this.netbarNameLbl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "V2.0727.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label netbarNameLbl;
        private System.Windows.Forms.Label runStateLbl;
        private System.Windows.Forms.Button aboutBtn;
        private System.Windows.Forms.Button cashierConfig;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem 退出ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 显示ToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label netStateLbl;
    }
}

