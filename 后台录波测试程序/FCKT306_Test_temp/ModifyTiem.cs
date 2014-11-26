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

namespace FCK303_Monitor
{
    public partial class Form1 : Form
    {
        private int ShowTabNo;//分批次显示信息量
       // private int ShowTabNoV;//分批次显示信息量
       // private int ShowTabNoC;//分批次显示信息量
        private int SetTabNoC;//分批次显示信息量
        private int JixiangNo;//当前所选择机箱号
        private ushort [,] DataArr_V;//存储接收数值量
        //private ushort[,] DataArr_Other;//存储统计电压及版本号
        private ushort[,] DataArr_S;//存储接收状态量
        private byte[,] DataArr_C;//存储接收控制指令
        private string[] Status_Show;//存储控制指令名
        private int VorS;//需要显示电压值还是状态值
        public string[,] COMM_Data;//存储下位机IP地址和端口号
        private int[] Comm_state;//记录18个机箱的通信状态
        private byte[] myReadBuffer;//从套接字读取数据的缓冲区
        private byte[] SendControl;//设置控制指令的报文
        private int SendLen;//下发指令报文的长度
        public string Rec_Data;//存储接收到的原始报文
        //private bool ShowStatus;
        private double factor;//显示电压值的比例系数
        private bool SendFlg;//标志是否有控制指令下发
        private bool SaveFlg;//标志是否需要保存报文
        public string FileString;//报文存入文件前转换为字符串
        private int Pici;//控制数据处理的频率
        Socket skt;
        EndPoint endPoint;
        IPEndPoint ipEndPoint;
        StreamWriter[] sw;
 
        public Form1()
        {
            InitializeComponent();

            Pici = 0;
            ShowTabNo = 0;
          //  ShowTabNoV = 0;
          //  ShowTabNoC = 0;
            SetTabNoC = 0;
            JixiangNo = 1;
            SendLen = 0;
            VorS = 0;
            Rec_Data = "";
            //ShowStatus = false;
            SendFlg = false;
            SaveFlg = false;
            factor = 1.0;
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
            Status_Show[0] = "召唤状态和电容电压";
            Status_Show[1] = "召唤过欠压设定值";
            Status_Show[2] = "复位";
            Status_Show[3] = "闭锁子模块";
            Status_Show[4] = "投入子模块";
            Status_Show[5] = "切除子模块";
            Status_Show[6] = "晶闸管使能";
            Status_Show[7] = "晶闸管禁止";
            Status_Show[8] = "旁路开关使能";
            Status_Show[9] = "旁路开关禁止";

            sw = new StreamWriter[18];
           
            ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            endPoint = (EndPoint)ipEndPoint;
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            skt.ReceiveBufferSize = 24 * 1024;
            IPEndPoint tempEndPoint = new IPEndPoint(IPAddress.Any, 6001);
            skt.Bind(tempEndPoint);

            for (int i = 0; i < 16; i++)
            {
                dataGridView1.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                dataGridView2.Rows.Add("", "", "", "", "", "", "", "");
                dataGridView3.Rows.Add("", "", "", "", "");
            }
        }
                
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int length;
            string strss;
            byte[] SendByte = {0x68, 0x9C, 0x00, 0x00, 0xBB, 0xCC };
            bool COMM_Deplay = false;
            for (int i = 0; i < 18; i++)
            {
                Comm_state[i]++;
            }
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
                //if (form3 != null && form3.Visible == true && form3.ShowFlg == true)
                //{
                //    Rec_Data += strss;
                //    Rec_Data += ":" + System.Convert.ToString(myReadBuffer[4]) + " ";
                //    for (int i = 0; i < length - 1; i++)
                //        Rec_Data = Rec_Data + System.Convert.ToString(myReadBuffer[i]) + " ";
                //    Rec_Data += System.Convert.ToString(myReadBuffer[length - 1]) + "\r\n";
                //}
            }
        }

        private bool Deal_Data(Byte[] Rec, int Length, int jxNo)
        {
            int i, j;
            int sum = 0;
            bool Flg = false;
            string str = "";

            for (i = 0; i < Length - 177; i++)
            {
                if (Rec[i] == 0xAA && Rec[i + 1] == 0x55)
                {
                    for (j = 0; j < 176; j++)
                    { 
                        sum+=Rec[i+j];
                    }
                    sum &= 0xFFFF;
                    if (sum == (Rec[176] << 8) + Rec[177])
                    {
                        Flg = true;
                        for (j = 0; j < 32; j++)
                        {
                            DataArr_V[jxNo, j] = (ushort)((Rec[i + 2*j + 4] << 8) + Rec[i + 2*j + 5]);
                            DataArr_S[jxNo, j] = (ushort)((Rec[i + 2*j + 75] << 8) + Rec[i + 2*j + 76]);
                            DataArr_C[jxNo, j] = Rec[i + j + 140];
                        }
                        DataArr_V[jxNo, 32] = (ushort)((Rec[i + 68] << 8) + Rec[i + 69]);
                        DataArr_V[jxNo, 33] = (ushort)((Rec[i + 70] << 8) + Rec[i + 71]);
                        DataArr_V[jxNo, 34] = (ushort)((Rec[i + 72] << 8) + Rec[i + 73]);
                        DataArr_V[jxNo, 35] = (ushort)((Rec[i + 172] << 8) + Rec[i + 173]);
                        DataArr_V[jxNo, 36] = (ushort)((Rec[i + 174] << 8) + Rec[i + 175]);
                    }
                }
                if (SaveFlg == true)
                {//报头
                    if (Rec[i + 842] == 0)
                        Pici++;
                    str += " , ," + string.Format("{0:x}", Rec[i]) + ",";
                    str += string.Format("{0:x}", Rec[i + 1]) + ",";
                    str += string.Format("{0:x}", Rec[i + 2]) + ",";
                    str += string.Format("{0:x}", Rec[i + 3]) + ",";
                    str += "\r\n" + System.Convert.ToString(Rec[i + 2]) + "," + System.Convert.ToString(Rec[i + 3]) + ",";
                    //32个电压值
                    for (j = 0; j < 32; j++)
                    {
                        str += System.Convert.ToString((Rec[i + 2 * j + 4] << 8) + Rec[i + 2 * j + 5]) + ",";
                    }
                    //32个状态值
                    str += "\r\n" + System.Convert.ToString(Rec[i + 2]) + "," + System.Convert.ToString(Rec[i + 3]) + ",";
                    for (j = 0; j < 32; j++)
                    {
                        str += "0x";
                        if (Rec[i + 2 * j + 75] < 0x10)
                            str += "0";
                        str += string.Format("{0:x}", Rec[i + 2 * j + 75]);
                        if (Rec[i + 2 * j + 76] < 0x10)
                            str += "0";
                        str += string.Format("{0:x}", Rec[i + 2 * j + 76]) + ",";
                    }
                    //32个控制指令
                    str += "\r\n" + System.Convert.ToString(Rec[i + 2]) + "," + System.Convert.ToString(Rec[i + 3]) + ",";
                    for (j = 0; j < 16; j++)
                    {
                        str += "0x";                       
                        str += string.Format("{0:x}", (Rec[i + j + 140]>>4)&0xF) + ",";
                        str += "0x";
                        str += string.Format("{0:x}", Rec[i + j + 140] & 0xF) + ",";
                    }
                    //其他
                    str += "\r\n" + System.Convert.ToString(Rec[i + 2]) + "," + System.Convert.ToString(Rec[i + 3]) + ",";
                    //平均电压
                    str += "AVG-V,";
                    str += System.Convert.ToString((Rec[i + j + 72] << 8) + Rec[i + j + 73]) + ",";
                    //总状态
                    str += "Total-State,0x";
                    if (Rec[i + j + 156] < 0x10)
                        str += "0";
                    str += string.Format("{0:x}", Rec[i + j + 156]) + ",";
                    //运行模式
                    str += "Run-Mode,0x";
                    if (Rec[i + j + 157] < 0x10)
                        str += "0";
                    str += string.Format("{0:x}", Rec[i + j + 157])+",";
                    //拨码开关
                    str += "DIP-Switch,0x";
                    if (Rec[i + j + 158] < 0x10)
                        str += "0";
                    str += string.Format("{0:x}", Rec[i + j + 158]);
                    str += "\r\n";
                    if (sw[jxNo] != null)
                    {
                        sw[jxNo].Write(str);
                        sw[jxNo].Flush();
                    }
                }
            }
            return Flg;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Text.Contains("桥臂"))//当选择桥臂时，默认显示所选桥臂下机箱1的状态值
            {
                JixiangNo = (System.Convert.ToInt16(e.Node.Name) - 1) *5 + 1;
                toolStripLabel4.Text = "当前显示:" + e.Node.Text + "机箱1";
                VorS = 0;
            }
            else if (e.Node.Text.Contains("机箱"))//当选择机箱时，默认显示所选机箱状态值
            {
                JixiangNo = System.Convert.ToInt16(e.Node.Name) - 100;
                toolStripLabel4.Text = "当前显示:" + e.Node.Parent.Text + e.Node.Text;
                VorS = 0;
            }            
            
            if ((Comm_state[JixiangNo - 1] < 20)&&(timer1.Enabled==true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();

        }

        private void Show_for_ONE(ushort[,] Data_VC, ushort[,] Data_State)
        {
            int VCIndex, StateIndex;
            ulong tempData;
            string tempstr = "";
            int templen = 0;
           
            if (tabControl1.SelectedIndex == 0)
            {
                #region 按编号显示电压值和状态值
                VCIndex = ShowTabNo * 16;
                StateIndex = ShowTabNo * 16;
                for (int i = 0; i < 16; i++)
                {
                    tempData = (ushort)Data_VC[JixiangNo - 1, VCIndex + i];
                    //tempData = tempData * 700000 / 32768;
                    //if (tempData % 10 >= 5)
                    //    tempData += 10;
                    //tempData /= 10;
                    //tempf = tempData / 1000.0;
                    //str = String.Format("{0:f3}", tempf);
                    //dataGridView1.Rows[i].Cells[16].Value = str;
                    dataGridView1.Rows[i].Cells[23].Value = tempData*factor;
                    dataGridView1.Rows[i].Cells[0].Value = StateIndex + i + 1;

                }
                for (int i = 0; i < 16; i++)
                {
                    tempData = (ushort)Data_State[JixiangNo - 1, StateIndex + i];
                    tempstr = String.Format("{0:x}", tempData);
                    templen = tempstr.Length;
                    while (4 - templen > 0)
                    {
                        tempstr = "0" + tempstr;
                        templen++;
                    }
                    dataGridView1.Rows[i].Cells[22].Value = tempstr;
                    if ((tempData & 0x0004) != 0x0004)//旁路开关状态
                        dataGridView1.Rows[i].Cells[2].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[2].Style.BackColor = Color.Black;
                    if ((tempData & 0x0008) != 0x0008)//旁路开关误动
                        dataGridView1.Rows[i].Cells[3].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[3].Style.BackColor = Color.Black;
                    //dataGridView1.Rows[i].Cells[3].Style.BackColor = Color.Green;
                    if ((tempData & 0x0010) != 0x0010)//旁路开关误动
                        dataGridView1.Rows[i].Cells[4].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[4].Style.BackColor = Color.Black;
                    //dataGridView1.Rows[i].Cells[8].Style.BackColor = Color.Red;
                    if ((tempData & 0x0020) != 0x0020)//上电状态
                        dataGridView1.Rows[i].Cells[6].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[6].Style.BackColor = Color.Black;
                    //dataGridView1.Rows[i].Cells[14].Style.BackColor = Color.Red;
                    if ((tempData & 0x0040) != 0x0040)//软启状态
                        dataGridView1.Rows[i].Cells[7].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[7].Style.BackColor = Color.Black;
                    //dataGridView1.Rows[i].Cells[13].Style.BackColor = Color.Red;
                    if ((tempData & 0x0080) != 0x0080)//欠压
                        dataGridView1.Rows[i].Cells[9].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[9].Style.BackColor = Color.Black;
                    //dataGridView1.Rows[i].Cells[12].Style.BackColor = Color.Red;
                    if ((tempData & 0x0100) != 0x0100)//过压
                        dataGridView1.Rows[i].Cells[10].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[10].Style.BackColor = Color.Black;
                    //dataGridView1.Rows[i].Cells[6].Style.BackColor = Color.Green;
                    if ((tempData & 0x0200) != 0x0200)//上行通信状态
                        dataGridView1.Rows[i].Cells[12].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[12].Style.BackColor = Color.Black;
                    //dataGridView1.Rows[i].Cells[11].Style.BackColor = Color.Red;
                    if ((tempData & 0x0400) != 0x0400)//下行通信状态
                        dataGridView1.Rows[i].Cells[13].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[13].Style.BackColor = Color.Black;
                    //dataGridView1.Rows[i].Cells[10].Style.BackColor = Color.Red;
                    if ((tempData & 0x0800) != 0x0800)//晶闸管状态
                        dataGridView1.Rows[i].Cells[15].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[15].Style.BackColor = Color.Black;
                    //dataGridView1.Rows[i].Cells[5].Style.BackColor = Color.Green;
                    if ((tempData & 0x1000) != 0x1000)//驱动二反馈信号
                        dataGridView1.Rows[i].Cells[17].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[17].Style.BackColor = Color.Black;
                    //dataGridView1.Rows[i].Cells[4].Style.BackColor = Color.Green;
                    if ((tempData & 0x2000) != 0x2000)//驱动一反馈信号
                        dataGridView1.Rows[i].Cells[18].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[18].Style.BackColor = Color.Black;
                    if ((tempData & 0x4000) != 0x4000)//驱动二故障信号
                        dataGridView1.Rows[i].Cells[19].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[19].Style.BackColor = Color.Black;
                    if ((tempData & 0x8000) != 0x8000)//驱动一故障信号
                        dataGridView1.Rows[i].Cells[20].Style.BackColor = Color.White;
                    else
                        dataGridView1.Rows[i].Cells[20].Style.BackColor = Color.Black;
                }
                #endregion
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                #region 单独显示所有电压值
                for (int j = 0; j < 2; j++)
                {
                    //VCIndex = 5 + j * 32;
                    for (int i = 0; i < 16; i++)
                    {
                        tempData = (ushort)Data_VC[JixiangNo - 1,  j * 16 + i];
                        dataGridView2.Rows[i].Cells[j * 3 + 1].Value = tempData*factor;
                        dataGridView2.Rows[i].Cells[j * 3].Value = j * 16 + i + 1;
                    }
                }
                dataGridView2.Rows[0].Cells[7].Value = Data_VC[JixiangNo - 1, 34] * factor;
             //   dataGridView2.Rows[1].Cells[7].Value = DataArr_C[JixiangNo - 1, 16];
                dataGridView2.Rows[1].Cells[7].Value = DataArr_C[JixiangNo - 1, 17];
            //    dataGridView2.Rows[3].Cells[7].Value = DataArr_C[JixiangNo - 1, 18];
                #endregion
            }
            else if (tabControl1.SelectedIndex == 2)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int index = 0; index < 8; index++)
                    {
                        try
                        {
                            tempData = DataArr_C[JixiangNo - 1, j * 8 + index];
                            tempstr = String.Format("{0:x}", tempData&0x0f);
                            dataGridView3.Rows[2 * index].Cells[j * 3].Value = 16 * j + 2 * index + 1;
                            dataGridView3.Rows[2*index].Cells[j * 3 + 1].Value = tempstr;
                            tempstr = String.Format("{0:x}", (tempData>>4) & 0x0f);
                            dataGridView3.Rows[2 * index+1].Cells[j * 3].Value = 16 * j + 2 * index + 2;
                            dataGridView3.Rows[2 * index+1].Cells[j * 3 + 1].Value = tempstr;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("收到错误状态编码！", "警告", MessageBoxButtons.OK);
                        }
                    }
                }
            }
            else if (tabControl1.SelectedIndex == 3)
            { 
            }
            toolStripStatusLabel1.Text = "当前显示的是CPU板" + System.Convert.ToString(JixiangNo)+ "的数据";
            toolStripStatusLabel4.Text = "当前CPU板程序版本号：V" + String.Format("{0:f2}", DataArr_V[JixiangNo-1, 35] / 100.0);
            tempstr = String.Format("{0:x}", (DataArr_V[JixiangNo-1,36] & 0xffff));
            templen = tempstr.Length;
            while (4 - templen > 0)
            {
                tempstr = "0" + tempstr;
                templen++;
            }
            toolStripStatusLabel5.Text =  "当前CPU板程序校验码：0x" + tempstr;
        }

        private void Show_for_ONE()
        {
            for (int i = 0; i < 16; i++)
            {
                dataGridView1.Rows[i].Cells[22].Value = "";
                dataGridView1.Rows[i].Cells[23].Value = "";
                dataGridView1.Rows[i].Cells[0].Value = i + ShowTabNo * 16+1;
                for (int j = 2; j < 22; j++)
                    if (j != 5 && j != 8 && j != 11 && j != 14 && j != 16 && j != 21)
                        dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.White;
                for (int j = 0; j < 2; j++)
                {
                    dataGridView2.Rows[i].Cells[3 * j].Value = 16 * j + i + 1;
                    dataGridView2.Rows[i].Cells[3 * j + 1].Value = "";
                    dataGridView3.Rows[i].Cells[3 * j].Value = 16 * j + i + 1;
                    dataGridView3.Rows[i].Cells[3 * j + 1].Value = "";
                }
            }
            dataGridView2.Rows[0].Cells[6].Value = "平均电压";
         //   dataGridView2.Rows[1].Cells[6].Value = "汇总状态";
            dataGridView2.Rows[1].Cells[6].Value = "运行模式";
            //dataGridView2.Rows[3].Cells[6].Value = "拨码开关";
            dataGridView2.Rows[0].Cells[7].Value = "";
            dataGridView2.Rows[1].Cells[7].Value = "";
        //    dataGridView2.Rows[2].Cells[7].Value = "";
        //    dataGridView2.Rows[3].Cells[7].Value = "";
            toolStripStatusLabel1.Text = "当前无连接";
            toolStripStatusLabel4.Text = "当前机箱程序版本号：";
            toolStripStatusLabel5.Text = "当前机箱程序校验码：";
        }
        #region 翻页操作
        private void button1_Click(object sender, EventArgs e)
        {
            ShowTabNo = 0;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ShowTabNo = 1;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }
/*
        private void button3_Click(object sender, EventArgs e)
        {
            ShowTabNo = 2;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ShowTabNo = 3;
            if ((Comm_state[JixiangNo - 1] < 10)&&(timer1.Enabled==true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ShowTabNo = 4;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ShowTabNo = 5;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ShowTabNo = 6;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ShowTabNo = 7;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ShowTabNoV = 0;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ShowTabNoV = 1;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ShowTabNoC = 0;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            ShowTabNoC = 1;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            SetTabNoC = 0;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            SetTabNoC = 1;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }
        */
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            //判断当前机箱的通信状态，并设置指示颜色
            if (Comm_state[JixiangNo-1] >= 20)
                toolStripLabel2.BackColor = System.Drawing.Color.Gray;
            else
                toolStripLabel2.BackColor = System.Drawing.Color.Green;
            for (int i = 0; i < 18; i++)
            {
                if (Comm_state[i] > 200)
                    Comm_state[i] = 20;
            }

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
                    skt.SendTo(SendControl, 27, SocketFlags.None, (EndPoint)ipEndPoint);
                }
                catch (Exception)
                {
                }
                #endregion
            }
            
            //显示数据
            if (Comm_state[JixiangNo - 1] == 0)
                Show_for_ONE(DataArr_V, DataArr_S);
            else if (Comm_state[JixiangNo - 1] == 10)
                Show_for_ONE();

            //显示下位机校验码，本程序暂时不用
            //label2.Text = "DSP程序版本号：V" + String.Format("{0:f2}", (((DataQiaobi_VC[0, 781] << 8) + DataQiaobi_VC[0, 782]) & 0xffff) / 100.0);
            //string tempstr = String.Format("{0:x}", (((DataQiaobi_VC[0, 783] << 8) + DataQiaobi_VC[0, 784]) & 0xffff));
            //int templen = tempstr.Length;
            //while (4 - templen > 0)
            //{
            //    tempstr = "0" + tempstr;
            //    templen++;
            //}
            //label3.Text = "DSP程序校验码：0x" + tempstr;

            //string SortResult = "";
            //for (int i = 0; i < 25; i++)
            //{
            //    for (int j = i; j < 128; j += 25)
            //    {
            //        SortResult += String.Format("模块{0,3:d}:{1,7:f1}  ", (SortVC[QiaobiNO - 1, j * 4] << 8) + SortVC[QiaobiNO - 1, j * 4 + 1], (SortVC[QiaobiNO - 1, j * 4 + 2] << 8) + SortVC[QiaobiNO - 1, j * 4 + 3]);
            //    }
            //    SortResult += "\r\n";
            //}
            //richTextBox2.Text = SortResult;

            if (backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync(Rec_Data);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }//打开连接

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            for (int i = 0; i < 18; i++)
                Comm_state[i] = 15;
            toolStripLabel2.BackColor = System.Drawing.Color.Gray;
            if ((Comm_state[JixiangNo - 1] < 10) && (timer1.Enabled == true))
                Show_for_ONE(DataArr_V, DataArr_S);
            else
                Show_for_ONE();
        }//关闭连接

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            skt.Shutdown(SocketShutdown.Both);
            skt.Close();
            for (int i = 0; i < 18; i++)
            {
                if (sw[i] != null)
                {
                    sw[i].Close();
                    sw[i].Dispose();
                    sw[i] = null;
                }
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox2.Text == "")
                {
                    MessageBox.Show("请设定电压显示比例！", "提示", MessageBoxButtons.OK);
                    textBox2.Text = textBox1.Text;
                }
                else
                {                    
                    factor = System.Convert.ToDouble(textBox2.Text);
                    textBox1.Text = textBox2.Text;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("电压显示比例设定非法！", "警告", MessageBoxButtons.OK);
                textBox2.Text = textBox1.Text;
                //factor = 1.0;
            }
        }//设定电压值显示比例系数

        private void button15_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text == "")
                {
                    MessageBox.Show("请设定电压显示比例！", "提示", MessageBoxButtons.OK);
                    textBox1.Text = textBox2.Text;
                }
                else
                {
                    factor = System.Convert.ToDouble(textBox1.Text);
                    textBox2.Text = textBox1.Text;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("电压显示比例设定非法！", "警告", MessageBoxButtons.OK);
                textBox1.Text = textBox2.Text;
                //factor = 1.0;
            }
        }//设定电压值显示比例系数

        private void dataGridView4_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //uint sum = 0;
            ////MessageBox.Show(e.ColumnIndex.ToString()+","+e.RowIndex.ToString());
            //if (e.RowIndex >= 0 && ((e.ColumnIndex - 2) % 4 == 0))
            //{
            //    SendFlg = true;
            //    SendControl[0] = 0xBB;
            //    SendControl[1] = 0x44;
            //    SendControl[2] = (byte)(JixiangNo&0xff);
            //    SendControl[3] = (byte)(SetTabNoC * 80 + (e.ColumnIndex - 2) * 5 + e.RowIndex + 1);
            //    try
            //    {
            //        if (dataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value.ToString() == "")
            //            return;
            //        else
            //            SendControl[4] = System.Convert.ToByte(dataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value);
            //        sum = (uint)(SendControl[0] + SendControl[1] + SendControl[2] + SendControl[3] + SendControl[4]);
            //        SendControl[5] = (byte)((sum >> 8) & 0xFF);
            //        SendControl[6] = (byte)(sum & 0xFF);
            //        SendLen = 7;
            //        SendFlg = true;
            //    }
            //    catch (Exception)
            //    {
            //        MessageBox.Show("输入值非法！请重新输入！","警告",MessageBoxButtons.OK);
            //    }
            //}            
        }//生成控制指令报文

        private void dataGridView4_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            uint sum = 0;
            if (e.RowIndex >= 0 && ((e.ColumnIndex - 2) % 4 == 0))
            {
                SendFlg = true;
                SendControl[0] = 0xBB;
                SendControl[1] = 0x44;
                SendControl[2] = (byte)(JixiangNo & 0xff);
                SendControl[3] = 20;
                SendControl[4] = (byte)(SetTabNoC * 80 + (e.ColumnIndex - 2) * 5 + 1);
                try
                {
                    for (int i = 0; i < 25;i++ )
                        sum += SendControl[i ];
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

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (SaveFlg == true)
            {
                SaveFlg = false;
                toolStripButton4.Text = "保存报文";
                toolStripButton5.Visible = true;
                toolStripSeparator5.Visible = true;
                for (int i = 0; i < 18; i++)
                {
                    if (sw[i] != null)
                    {
                        sw[i].Close();
                        sw[i].Dispose();
                        sw[i] = null;
                    }
                }
            }
            else
            {
                toolStripButton5.Visible = false;
                toolStripSeparator5.Visible = false;
                try
                {
                    FileString = "Group,Number,";
                    for (int i = 0; i < 33; i++)
                        FileString += System.Convert.ToString(i) + ",";
                    FileString += "\r\n";
                    for (int i = 0; i < 18; i++)
                    {
                        string path = Path.GetFullPath("SaveFile\\机箱" + System.Convert.ToString(i) + ".csv");
                        sw[i] = new StreamWriter(path, true);
                        sw[i].Write(FileString);

                    }
                    SaveFlg = true;
                    toolStripButton4.Text = "取消保存";
                }
                catch (Exception)
                {
                    MessageBox.Show("文件打开失败！");
                    toolStripButton5.Visible = true;
                    toolStripSeparator5.Visible = true;
                }
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            int i = 0;
            //, FileMode.Append);
            // Open the file to read from.
            try
            {
                for (i = 0; i < 18; i++)
                {
                    string path = Path.GetFullPath("SaveFile\\机箱" + System.Convert.ToString(i) + ".csv");
                    sw[i] = new StreamWriter(path, false);
                    sw[i].Write("");
                    sw[i].Close();
                    sw[i].Dispose();
                    sw[i] = null;
                }
                if (i == 18)
                    MessageBox.Show("数据清除成功！");
            }
            catch (Exception)
            {
                MessageBox.Show("文件打开失败！");
            }
        }

        private void button3_Click(object sender, EventArgs e)
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
