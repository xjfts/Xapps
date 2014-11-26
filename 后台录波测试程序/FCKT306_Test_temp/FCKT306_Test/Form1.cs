using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FCKT306_Test
{
    public partial class Form1 : Form
    {
        FCK302_Lubo WinFCK302_Lubo;
        FCKT306 WinFCKT306;
        ModifyTime WinModifyTime;
        public Form1()
        {
            InitializeComponent();
        }

        private void fCKT306StripMenuItem_Click(object sender, EventArgs e)
        {
            WinFCKT306 = new FCKT306();
            WinFCKT306.MdiParent = this;
            WinFCKT306.toolstrbtn = this.toolStripButton1;
            WinFCKT306.tstrmenuitem = this.fCKT306StripMenuItem;
            WinFCKT306.Visible = true;//(); 
            WinFCKT306.WindowState = FormWindowState.Maximized;
            toolStripButton1.Visible = true;
            fCKT306StripMenuItem.Enabled = false;
        }

        private void fCK302StripMenuItem_Click(object sender, EventArgs e)
        {
            WinFCK302_Lubo = new FCK302_Lubo();
            WinFCK302_Lubo.MdiParent = this;
            WinFCK302_Lubo.toolstrbtn = this.toolStripButton2;
            WinFCK302_Lubo.tstrmenuitem = this.fCK302StripMenuItem;
            WinFCK302_Lubo.Visible = true;//(); 
            WinFCK302_Lubo.WindowState = FormWindowState.Maximized;
            toolStripButton2.Visible = true;
            fCK302StripMenuItem.Enabled = false;
        }

        private void ModifyTStripMenuItem_Click(object sender, EventArgs e)
        {
            WinModifyTime = new ModifyTime();
            WinModifyTime.MdiParent = this;
            WinModifyTime.toolstrbtn = this.toolStripButton3;
            WinModifyTime.tstrmenuitem = this.ModifyTStripMenuItem;
            WinModifyTime.Visible = true;//(); 
            WinModifyTime.WindowState = FormWindowState.Maximized;
            toolStripButton3.Visible = true;
            ModifyTStripMenuItem.Enabled = false;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            WinFCKT306.Focus();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            WinFCK302_Lubo.Focus();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            WinModifyTime.Focus();
        }
    }
}
