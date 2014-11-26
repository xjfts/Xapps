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
    public partial class ModifyTime : Form
    {
        public ToolStripMenuItem tstrmenuitem;
        public ToolStripButton toolstrbtn;

        private int JixiangNo;//当前所选择机箱号
        private ushort [,] DataArr_V;//存储接收数值量
        private ushort[,] DataArr_S;//存储接收状态量
        private byte[,] DataArr_C;//存储接收控制指令
        private string[] Status_Show;//存储控制指令名
        public string[,] COMM_Data;//存储下位机IP地址和端口号
        private int[] Comm_state;//记录18个机箱的通信状态
        private byte[] myReadBuffer;//从套接字读取数据的缓冲区
        private byte[] SendControl;//设置控制指令的报文
        public string Rec_Data;//存储接收到的原始报文
        //private bool ShowStatus;
        private bool SendFlg;//标志是否有控制指令下发
       // private bool SaveFlg;//标志是否需要保存报文
        public string FileString;//报文存入文件前转换为字符串
        private int SendLen;
        Socket skt;
        EndPoint endPoint;
        IPEndPoint ipEndPoint;

        public ModifyTime()
        {
            InitializeComponent();

            SendLen = 0;
            JixiangNo = 1;
            Rec_Data = "";
            //ShowStatus = false;
            SendFlg = false;
            //SaveFlg = false;
            SendControl=new byte[30];
            DataArr_V = new ushort[18, 40];
            DataArr_S = new ushort[18, 40];
            DataArr_C = new byte[18, 40];
            myReadBuffer = new byte[1024];
            Status_Show = new string[10];
            for (int i = 0; i < 1024; i++)
                myReadBuffer[i] = 0;
            for (int i = 0; i < 18; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    DataArr_V[i, j] = 0;
                    DataArr_S[i, j] = 0;
                    DataArr_C[i, j] = 0;
                }
            }
            COMM_Data = new string[18, 2];
            Comm_state = new int[18];
            for (int i = 0; i < 18; i++)
            {
                COMM_Data[i, 0] = "192.168.1." + System.Convert.ToString(101 + i);
                COMM_Data[i, 1] = System.Convert.ToString(2101 + i);
                Comm_state[i]=15;
            }
                       
            ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            endPoint = (EndPoint)ipEndPoint;
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            skt.ReceiveBufferSize = 24 * 1024;
            IPEndPoint tempEndPoint = new IPEndPoint(IPAddress.Any, 6001);
            skt.Bind(tempEndPoint);

        }
                
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int length;
            string strss;
            byte[] SendByte = {0x68, 0x9C, 0x00, 0x00, 0xBB, 0xCC };
            bool COMM_Deplay = false;
            if (skt.Available > 0)
            {
                length = skt.ReceiveFrom(myReadBuffer, ref endPoint);
                strss = endPoint.ToString();
                for (int i = 0; i < 18; i++)
                {
                    if (strss.Contains(COMM_Data[i, 0]))
                    {
                        Comm_state[i] = 0;
                        COMM_Deplay = Deal_Data(myReadBuffer, length, i);
                        break;
                    }
                }
                if (COMM_Deplay == true)
                {
                    try
                    {
                        skt.SendTo(SendByte, endPoint);
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show("发送确认报文失败失败！","提示",MessageBoxButtons.OK);
                    }
                }
            }
        }

        private bool Deal_Data(Byte[] Rec, int Length, int jxNo)
        {
            return true;
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (SendFlg==true)
            {
                #region 发送至网关
                SendFlg = false;
                try
                {
                    byte[] ipbyte = new byte[4];
                    string[] sByte = COMM_Data[JixiangNo-1, 0].Split('.');
                    ipbyte[0] = System.Convert.ToByte(sByte[0]);
                    ipbyte[1] = System.Convert.ToByte(sByte[1]);
                    ipbyte[2] = System.Convert.ToByte(sByte[2]);
                    ipbyte[3] = System.Convert.ToByte(sByte[3]);//(byte)(201 + NetIndex);
                    IPAddress ipaddr1 = new IPAddress(ipbyte);
                    ipEndPoint.Address = ipaddr1;
                    ipEndPoint.Port = System.Convert.ToInt16(COMM_Data[JixiangNo-1, 1]);//2401 + NetIndex;
                    skt.SendTo(SendControl, SendLen, SocketFlags.None, (EndPoint)ipEndPoint);
                }
                catch (Exception)
                {
                }
                #endregion
            }
            if (backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync(Rec_Data);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            toolStripButton1.BackColor = Color.Green;
        }//打开连接

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            toolStripButton1.BackColor = System.Drawing.SystemColors.Control;
        }//关闭连接

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            tstrmenuitem.Enabled = true;
            toolstrbtn.Visible = false;

            skt.Shutdown(SocketShutdown.Both);
            skt.Close();
        }
        private void button3_Click(object sender, EventArgs e)//设置事件
        {
            uint sum = 0;
             try
             {
                SendControl[0] = 0xBB;
                SendControl[1] = 0x44;
                SendControl[2] = 0x55;
                SendControl[3] = System.Convert.ToByte(textBox3.Text);
                SendControl[4] = System.Convert.ToByte(textBox4.Text);
                SendControl[5] = System.Convert.ToByte(textBox5.Text);
                SendControl[6] = System.Convert.ToByte(textBox6.Text);
                SendControl[7] = System.Convert.ToByte(textBox7.Text);
                SendControl[8] = System.Convert.ToByte(textBox8.Text);
               
                for (int i = 0; i < 25; i++)
                    sum += SendControl[i];
                SendControl[25] = (byte)((sum >> 8) & 0xFF);
                SendControl[26] = (byte)(sum & 0xFF);
                SendLen = 27;
                SendFlg = true;
             }
            catch (Exception)
            {
                MessageBox.Show("设置有非法字符，请修改后再重发！", "警告", MessageBoxButtons.OK);
            }
        }
    }
}
