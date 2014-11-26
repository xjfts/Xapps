using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace FCKT306_Test
{
    public partial class FCKT306 : Form
    {
        public ToolStripMenuItem tstrmenuitem;
        public ToolStripButton toolstrbtn;

        private byte[] RecData;
        private byte[] SendData;
        private byte[] RecBuffer;
        private string[,] CommAddr;
        private int CommState;
        private bool SendFlg;

        Socket skt;
        EndPoint endPoint;
        IPEndPoint ipEndPoint;

        public FCKT306()
        {
           // testindex = 0;
            InitializeComponent();
            Initialize_DataGridView();

            SendFlg = false;
            RecData=new byte[200];
            SendData = new byte[100];
            RecBuffer = new byte[1024];
            CommState = 25;
            CommAddr = new string[1, 2];
            CommAddr[0, 0] = "192.168.1.136";
            CommAddr[0, 1] = "3106";

            ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            endPoint = (EndPoint)ipEndPoint;
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            skt.ReceiveBufferSize = 24 * 1024;
            IPEndPoint tempEndPoint = new IPEndPoint(IPAddress.Any, 7006);
            skt.Bind(tempEndPoint);
        }
        public void Initialize_DataGridView()
        {
            dataGridView1.Rows.Add("A上子模块电压和", "", "阀正在充电", "");
            dataGridView1.Rows.Add("A下子模块电压和", "", "阀预检", "");
            dataGridView1.Rows.Add("B上子模块电压和", "", "阀解锁", "");
            dataGridView1.Rows.Add("B下子模块电压和", "", "换流阀闭锁", "");
            dataGridView1.Rows.Add("C上子模块电压和", "", "充电完成", "");
            dataGridView1.Rows.Add("C下子模块电压和", "", "MVCE_RDY", "");
            dataGridView1.Rows.Add("保留", "", "MVCE_ALARM", "");
            dataGridView1.Rows.Add("保留", "", "保留", "");
            dataGridView1.Rows.Add("保留", "", "保护闭锁换流阀", "");
            dataGridView1.Rows.Add("FPGA版本", "", "IEC校验正确", "");
            dataGridView1.Rows.Add("FPGA校验码", "", "跳闸信号识别", "");
            dataGridView1.Rows.Add("DSP版本", "", "跳闸故障信号", "");
            dataGridView1.Rows.Add("DSP校验码", "", "保留", "");

            dataGridView3.Rows.Add("A上子模块电压和", "", "阀正在充电", "");
            dataGridView3.Rows.Add("A下子模块电压和", "", "阀预检", "");
            dataGridView3.Rows.Add("B上子模块电压和", "", "阀解锁", "");
            dataGridView3.Rows.Add("B下子模块电压和", "", "换流阀闭锁", "");
            dataGridView3.Rows.Add("C上子模块电压和", "", "充电完成", "");
            dataGridView3.Rows.Add("C下子模块电压和", "", "MVCE_RDY", "");
            dataGridView3.Rows.Add("保留", "", "MVCE_ALARM", "");
            dataGridView3.Rows.Add("保留", "", "保留", "");
            dataGridView3.Rows.Add("保留", "", "保护闭锁换流阀", "");
            dataGridView3.Rows.Add("FPGA版本", "", "IEC校验正确", "");
            dataGridView3.Rows.Add("FPGA校验码", "", "跳闸信号识别", "");
            dataGridView3.Rows.Add("DSP版本", "", "跳闸故障信号", "");
            dataGridView3.Rows.Add("DSP校验码", "", "保留", "");

            dataGridView2.Rows.Add("A上调制波", "", "设置");
            dataGridView2.Rows.Add("A下调制波", "", "设置");
            dataGridView2.Rows.Add("B上调制波", "", "设置");
            dataGridView2.Rows.Add("B下调制波", "", "设置");
            dataGridView2.Rows.Add("C上调制波", "", "设置");
            dataGridView2.Rows.Add("C下调制波", "", "设置");
            dataGridView2.Rows.Add("A上桥臂电流", "", "设置");
            dataGridView2.Rows.Add("B上桥臂电流", "", "设置");
            dataGridView2.Rows.Add("C上桥臂电流", "", "设置");
            dataGridView2.Rows.Add("A下桥臂电流", "", "设置");
            dataGridView2.Rows.Add("B下桥臂电流", "", "设置");
            dataGridView2.Rows.Add("C下桥臂电流", "", "设置");
            dataGridView2.Rows.Add("主备系统选择", "", "设置");
            DataGridViewComboBoxCell cell1 = new DataGridViewComboBoxCell();
            cell1.Items.Add("启用主动");
            cell1.Items.Add("启用备用");
            dataGridView2.Rows[12].Cells[1] = cell1;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (CommState < 20)
                toolStripButton6.BackColor = System.Drawing.Color.Green;
            if (CommState >= 20)
                toolStripButton6.BackColor = System.Drawing.Color.Gray;
            if (CommState > 200)
                CommState = 25;
            if (CommState < 20)
                Show_for_ONE(RecData);
            else if (CommState >= 20)
                Show_for_ONE();

            if (SendFlg == true)
            {
                string[] SByte = CommAddr[0, 0].Split('.');
                byte[] ipbyte = new byte[4];
                if (SByte.Length == 4)
                    for (int i = 0; i < 4; i++)
                        ipbyte[i] = System.Convert.ToByte(SByte[i]);

                IPAddress ipaddr = new IPAddress(ipbyte);
                ipEndPoint.Address = ipaddr;
                ipEndPoint.Port = System.Convert.ToInt16(CommAddr[0, 1]);
                
                    try
                    {
                        SendFlg = false;
                        skt.SendTo(SendData, SendData[3], SocketFlags.None, (EndPoint)ipEndPoint);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("指令发送失败！", "提示", MessageBoxButtons.OK);
                    }
            }

            if (backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync(RecBuffer);
            }
        }
        //开启子线程接收数据
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int length;
            string strss;    
            CommState++;
            for (int i = 0; i < 6; i++)
            {
                if (skt.Available > 0)
                {
                    length = skt.ReceiveFrom(RecBuffer, ref endPoint);
                    strss = endPoint.ToString();
                    if (strss.Contains(CommAddr[0, 0]))
                    {
                        CommState = 0;
                        Deal_Data(RecBuffer, length);
                        //break;
                    }
                }
                else
                    break;
            }
        }
        //启动通信
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            toolStripButton1.BackColor = Color.Green;
        }
        //关闭通信
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            toolStripButton1.BackColor = SystemColors.Control;
            Show_for_ONE();
        }
        //处理接收报文
        private void Deal_Data(Byte[] Rec, int Length)
        {
            for (int i = 0; i < Length; i++)
            {
                if (Rec[i] == 0xAA && Rec[i + 1] == 0x55 && i + Length >= 178)
                {
                    Int16 sum = 0;
                    for (int j = 0; j < 176; j++)
                        sum += Rec[i + j];
                    if (sum == ((Rec[i + 176] << 8) + Rec[i + 177]))
                    {
                        for (int j = 0; j < 178; j++)
                            RecData[j] = Rec[i + j];
                        break;                       
                    }
                }
                else if (Rec[i] == 0xBB && Rec[i + 1] == 0xCC && i + Length >= ((Rec[i + 2] << 8) + Rec[i + 3]))
                { 
                    Int16 sum = 0;
                    int tlength = (Rec[i + 2] << 8) + Rec[i + 3];
                    for (int j = 0; j < tlength-2; j++)
                        sum += Rec[i + j];
                    if (sum == ((Rec[i + tlength - 2] << 8) + Rec[i + tlength - 1]))
                    {
                        MessageBox.Show("指令下发成功！","提示",MessageBoxButtons.OK);
                        break;
                    }
                }
            }
        }
        //显示数据到窗体上
        private void Show_for_ONE(byte[] Rec)
        {
            int fData = 0;
            for (int i = 0; i < 6; i++)
            {
                fData= (Rec[2 * i + 4] << 8) + Rec[2* i + 5];
                dataGridView1.Rows[i].Cells[1].Value = (fData / 10).ToString() + "." + (fData % 10).ToString();
                fData = (Rec[2* i + 55] << 8) + Rec[2* i + 56];
                dataGridView3.Rows[i].Cells[1].Value = (fData / 10).ToString() + "." + (fData % 10).ToString();
            }
            fData = (Rec[16] << 8) + Rec[17];
            for (int i = 0; i < 8; i++)
            {
                if (((fData >> (2 * i)) & 0x03) == 0x03)
                    dataGridView1.Rows[i].Cells[3].Style.BackColor = Color.Black;
                else
                    dataGridView1.Rows[i].Cells[3].Style.BackColor = Color.White;
            }
            fData = (Rec[67] << 8) + Rec[68];
            for (int i = 0; i < 8; i++)
            {
                if (((fData >> (2 * i)) & 0x03) == 0x03)
                    dataGridView3.Rows[i].Cells[3].Style.BackColor = Color.Black;
                else
                    dataGridView3.Rows[i].Cells[3].Style.BackColor = Color.White;
            }
            if ((Rec[19] & 0x03) == 0x03)
                dataGridView1.Rows[8].Cells[3].Style.BackColor = Color.Black;
            else
                dataGridView1.Rows[8].Cells[3].Style.BackColor = Color.White;
            if ((Rec[70] & 0x03) == 0x03)
                dataGridView3.Rows[8].Cells[3].Style.BackColor = Color.Black;
            else
                dataGridView3.Rows[8].Cells[3].Style.BackColor = Color.White;
            if (Rec[106] == 0xAA && Rec[107] == 0xAA)
                dataGridView1.Rows[9].Cells[3].Style.BackColor = Color.Black;
            else
                dataGridView1.Rows[9].Cells[3].Style.BackColor = Color.White;
            if (Rec[108] == 0xAA && Rec[109] == 0xAA)
                dataGridView3.Rows[9].Cells[3].Style.BackColor = Color.Black;
            else
                dataGridView3.Rows[9].Cells[3].Style.BackColor = Color.White;
            if (Rec[110] == 0xAA && Rec[111] == 0xAA)
                dataGridView1.Rows[10].Cells[3].Style.BackColor = Color.Black;
            else
                dataGridView1.Rows[10].Cells[3].Style.BackColor = Color.White;
            if (Rec[112] == 0xAA && Rec[113] == 0xAA)
                dataGridView3.Rows[10].Cells[3].Style.BackColor = Color.Black;
            else
                dataGridView3.Rows[10].Cells[3].Style.BackColor = Color.White;
            if (Rec[114] == 0xAA && Rec[115] == 0xAA)
                dataGridView1.Rows[11].Cells[3].Style.BackColor = Color.Black;
            else
                dataGridView1.Rows[11].Cells[3].Style.BackColor = Color.White;
            if (Rec[116] == 0xAA && Rec[117] == 0xAA)
                dataGridView3.Rows[11].Cells[3].Style.BackColor = Color.Black;
            else
                dataGridView3.Rows[11].Cells[3].Style.BackColor = Color.White;

            fData = (Rec[168] << 8) + Rec[169];
            string fpgaVer = "";
            if (((fData >> 12) & 0x0F) != 0)
                fpgaVer += ((fData >> 12) & 0x0F).ToString();
            fpgaVer += ((fData >> 8) & 0x0F).ToString() + "." + ((fData >> 4) & 0x0F).ToString() + (fData & 0x0F).ToString();
            dataGridView1.Rows[9].Cells[1].Value = fpgaVer;//((fData >> 12) & 0x0F).ToString() + ((fData >> 8) & 0x0F).ToString() + "." + ((fData >> 4) & 0x0F).ToString() + (fData & 0x0F).ToString();
            dataGridView3.Rows[9].Cells[1].Value = fpgaVer;//((fData >> 12) & 0x0F).ToString() + ((fData >> 8) & 0x0F).ToString() + "." + ((fData >> 4) & 0x0F).ToString() + (fData & 0x0F).ToString();
            fData = (Rec[172] << 8) + Rec[173];
            if ((fData % 100) < 10)
            {
                dataGridView1.Rows[11].Cells[1].Value = (fData / 100).ToString() + ".0" + (fData % 100).ToString();
                dataGridView3.Rows[11].Cells[1].Value = (fData / 100).ToString() + ".0" + (fData % 100).ToString();
            }
            else
            {
                dataGridView1.Rows[11].Cells[1].Value = (fData / 100).ToString() + "." + (fData % 100).ToString();
                dataGridView3.Rows[11].Cells[1].Value = (fData / 100).ToString() + "." + (fData % 100).ToString();
            }
            string str = string.Format("{0:x}", (Rec[170] << 8) + Rec[171]);
            while (str.Length < 4)
                str = "0" + str;
            dataGridView1.Rows[10].Cells[1].Value = "0x" + str;
            dataGridView3.Rows[10].Cells[1].Value = "0x" + str;

            str = string.Format("{0:x}", (Rec[174] << 8) + Rec[175]);
            while (str.Length < 4)
                str = "0" + str;
            dataGridView1.Rows[12].Cells[1].Value = "0x" + str;
            dataGridView3.Rows[12].Cells[1].Value = "0x" + str;
        }
        //通信中断，清除窗体数据
        private void Show_for_ONE()
        {
            for (int i = 0; i < 13; i++)
            {
                dataGridView1.Rows[i].Cells[1].Value = "";
                dataGridView3.Rows[i].Cells[1].Value = "";
                dataGridView1.Rows[i].Cells[3].Style.BackColor = Color.White;
                dataGridView3.Rows[i].Cells[3].Style.BackColor = Color.White;
            }
            //for (int i = 0; i < 17; i++)
            //{
            //    dataGridView2.Rows[i].Cells[1].Value = "";
            //}
        }
        //指令全部下发
        private void button1_Click(object sender, EventArgs e)
        {
            SendData[0] = 0xBB;
            SendData[1] = 0xCC;
            SendData[2] = 0x00;
            SendData[3] = 70;
            //SendData[4] = 0x00;
            //SendData[5] = 0x00;
            try
            {
                int senddata = 0;
                for (int i = 0; i < 12; i++)
                {
                    senddata = System.Convert.ToInt16(dataGridView2.Rows[i].Cells[1].Value.ToString());
                    SendData[4 * i + 4] = (byte)(((i + 1) >> 8) & 0xFF);
                    SendData[4 * i + 5] = (byte)((i + 1) & 0xFF);
                    SendData[4 * i + 6] = (byte)((senddata >> 8) & 0xFF);
                    SendData[4 * i + 7] = (byte)(senddata & 0xFF);
                }
            }
            catch(Exception)
            {
                MessageBox.Show("设定数据非法。", "警告", MessageBoxButtons.OK);
                return;
            }
            if (dataGridView2.Rows[12].Cells[1].Value.ToString() == "启用主动")
            {
                SendData[53] = 13;
                SendData[54] = 0xAA;
                SendData[55] = 0xAA;
            }
            else if (dataGridView2.Rows[12].Cells[1].Value.ToString() == "启用备用")
            {
                SendData[53] = 13;
                SendData[54] = 0x55;
                SendData[55] = 0x55;
            }
            else
            {
                MessageBox.Show("请选择主备模式。", "警告", MessageBoxButtons.OK);
                return;
            }
            SendData[57] = 14;
            if (checkBox1.Checked == true)
                SendData[58] = 0xAA;
            else
                SendData[58] = 0x55;
            if (checkBox2.Checked == true)
                SendData[59] = 0xAA;
            else
                SendData[59] = 0x55;
            SendData[61] = 15;
            if (checkBox4.Checked == true)
                SendData[62] = 0xAA;
            else
                SendData[62] = 0x55;
            if (checkBox3.Checked == true)
                SendData[63] = 0xAA;
            else
                SendData[63] = 0x55;
            SendData[65] = 16;
            if (checkBox6.Checked == true)
                SendData[66] = 0xAA;
            else
                SendData[66] = 0x55;
            if (checkBox5.Checked == true)
                SendData[67] = 0xAA;
            else
                SendData[67] = 0x55;

            int sum = 0;
            for (int i = 0; i < 68; i++)
                sum += SendData[i];
            SendData[68] = (byte)((sum >> 8) & 0xFF);
            SendData[69] = (byte)(sum & 0xFF);
            SendFlg = true;
        }
        //启用备用系统指令
        private void button2_Click(object sender, EventArgs e)
        {
            SendData[4] = 0x00;
            SendFlg = true;
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            SendData[0] = 0xBB;
            SendData[1] = 0xCC;
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                SendData[2] = 0x00;
                SendData[3] = 0x0A;
                SendData[4] = 0x00;
                SendData[5] = (byte)(e.RowIndex+1);
                if (e.RowIndex == 12)
                {
                    if (dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString() == "启用主动")
                    {
                        SendData[6] = 0xAA;
                        SendData[7] = 0xAA;
                    }
                    else if (dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString() == "启用备用")
                    {
                        SendData[6] = 0x55;
                        SendData[7] = 0x55;
                    }
                    else
                    {
                        MessageBox.Show("请选择主备模式。", "警告", MessageBoxButtons.OK);
                        return;
                    }
                }
                else
                {
                    try
                    {
                        int senddata = System.Convert.ToInt16(dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString());
                        SendData[6] = (byte)((senddata>>8)&0xFF);
                        SendData[7] = (byte)(senddata & 0xFF);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("设置的数据非法！", "警告", MessageBoxButtons.OK);
                        return;
                    }
                }
                int sum = 0;
                for (int i = 0; i < 8; i++)
                    sum += SendData[i];
                SendData[8] = (byte)((sum >> 8) & 0xFF);
                SendData[9] = (byte)(sum & 0xFF);
                SendFlg = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SendData[0] = 0xBB;
            SendData[1] = 0xCC;
            SendData[2] = 0x00;
            SendData[3] = 0x0A;
            SendData[4] = 0x00;
            SendData[5] = 14;
            if (checkBox1.Checked == true)
                SendData[6] = 0xAA;
            else
                SendData[6] = 0x55;
            if (checkBox2.Checked == true)
                SendData[7] = 0xAA;
            else
                SendData[7] = 0x55;
                   
            int sum = 0;
            for (int i = 0; i < 8; i++)
                sum += SendData[i];
            SendData[8] = (byte)((sum >> 8) & 0xFF);
            SendData[9] = (byte)(sum & 0xFF);
            SendFlg = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SendData[0] = 0xBB;
            SendData[1] = 0xCC;
            SendData[2] = 0x00;
            SendData[3] = 0x0A;
            SendData[4] = 0x00;
            SendData[5] = 15;
            if (checkBox4.Checked == true)
                SendData[6] = 0xAA;
            else
                SendData[6] = 0x55;
            if (checkBox3.Checked == true)
                SendData[7] = 0xAA;
            else
                SendData[7] = 0x55;

            int sum = 0;
            for (int i = 0; i < 8; i++)
                sum += SendData[i];
            SendData[8] = (byte)((sum >> 8) & 0xFF);
            SendData[9] = (byte)(sum & 0xFF);
            SendFlg = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SendData[0] = 0xBB;
            SendData[1] = 0xCC;
            SendData[2] = 0x00;
            SendData[3] = 0x0A;
            SendData[4] = 0x00;
            SendData[5] = 16;
            if (checkBox6.Checked == true)
                SendData[6] = 0xAA;
            else
                SendData[6] = 0x55;
            if (checkBox5.Checked == true)
                SendData[7] = 0xAA;
            else
                SendData[7] = 0x55;

            int sum = 0;
            for (int i = 0; i < 8; i++)
                sum += SendData[i];
            SendData[8] = (byte)((sum >> 8) & 0xFF);
            SendData[9] = (byte)(sum & 0xFF);
            SendFlg = true;
        }

        private void FCKT306_FormClosing(object sender, FormClosingEventArgs e)
        {
            tstrmenuitem.Enabled = true;
            toolstrbtn.Visible = false;
        }
    }
}
