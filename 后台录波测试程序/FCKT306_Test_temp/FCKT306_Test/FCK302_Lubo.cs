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
using System.Threading;

namespace FCKT306_Test
{
    public partial class FCK302_Lubo : Form
    {
        public ToolStripMenuItem tstrmenuitem;
        public ToolStripButton toolstrbtn;

        public delegate void AddListItem(int flg);
        public AddListItem myDelegate;
        private byte[][] RecData;
        private byte[] SendData;
        private byte[] RecBuffer;
        private string[,] CommAddr;
        private byte[,] LuboTime;
        private bool LuboTimeFlg;
        private int CommStatus;
        private int SendCounter;
       // private byte SendCode;
        private int SendLength;
        private byte DeviceAddr;
        //private int NumForOnceLubo;
        private int testindex;
        private int DataSelectFlg;
        private int[] StartTime;
        private int RedNum;
        private int GetRunNum;
        private double V_Factor;
        //private uint DataPointNum;
        private bool SendFlg;
        //private bool ReGetFlg;
        private uint TestCounter1;
        private uint TestCounter2;
        private uint TestCounter3;
        private uint[] TestCounter;
        private ushort[] DSPVerInfo;
        private int V_All;
        private int V_Group;
        private int V_Run;
        Socket skt;
        EndPoint endPoint;
        IPEndPoint ipEndPoint;
        private uint TestCounter4;
        private uint TestCounter5;
        private uint TestCounter6;
        DateTime[] datetimetest;
        int datetimeindex;
        DateTime datetimetest1;
        DateTime datetimetest2;
        StreamWriter sw_Lubo;
        public FCK302_Lubo()
        {
            InitializeComponent();
            
            
            TestCounter4 = 0;
            TestCounter5 = 0;
            TestCounter6 = 0;
            datetimetest = new DateTime[100];
            datetimeindex = 0;
            //NumForOnceLubo = 5000;
            //toolStripProgressBar1.Maximum = 5000;
            V_All = 0;
            V_Group = 0;
            V_Run = 0;
            GetRunNum = 0;
            SendFlg = false;
            LuboTimeFlg = false;
            //ReGetFlg = false;
            CommStatus = 0;
            SendCounter = 0;
            testindex = 0;
            V_Factor = 1.0;
            LuboTime = new byte[4,12];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 12; j++)
                    LuboTime[i, j] = 0;
            DSPVerInfo = new ushort[4];
            DSPVerInfo[0] = 0;
            DSPVerInfo[1] = 0;
            DSPVerInfo[2] = 0;
            DSPVerInfo[3] = 0;
            RecData = new byte[20000][];
            //for (int i = 0; i < 12001; i++)
            //    RecData[i] = new byte[679];
            TestCounter = new uint[10000];
            SendData = new byte[100];
            RecBuffer = new byte[2048];
            StartTime = new int[7];
            RedNum = 0;
            DataSelectFlg = 0;
            //CommState = 25;
            myDelegate = new AddListItem(WinShow);

            CommAddr = new string[1, 2];
            CommAddr[0, 0] = "192.168.1.60";
            CommAddr[0, 1] = "3106";

            ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            endPoint = (EndPoint)ipEndPoint;
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            skt.ReceiveBufferSize = 80000 * 1024;
            IPEndPoint tempEndPoint = new IPEndPoint(IPAddress.Any, 8888);
            skt.Bind(tempEndPoint);
            for (int i = 0; i < 20; i++)
                dataGridView1.Rows.Add("", "", "", "");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (toolStripProgressBar1.Visible == true)
            {
                toolStripProgressBar1.Value = (int)TestCounter3;
            }
            if (datetimeindex < 100)
            {
                datetimetest[datetimeindex] = DateTime.Now;
                datetimeindex++;
            }

            if (LuboTimeFlg == true)
                ShowInquireResult();

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
                    skt.SendTo(SendData, SendLength, SocketFlags.None, (EndPoint)ipEndPoint);
                    SendCounter++;
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

        private void ShowInquireResult()
        {
            for (int i = 0, k = 0; k < 4; k++)
            {
                if (LuboTime[k, 0] == 1)
                {
                    dataGridView1.Rows[i].Cells[0].Value = k;
                    string str = "";
                    str += ((LuboTime[k, 2] << 8) + LuboTime[k, 1]+2000).ToString() + "-";//年
                    str += (LuboTime[k, 3]).ToString() + "-";//月
                    str += (LuboTime[k, 4]).ToString() + " ";//日
                    str += (LuboTime[k, 5]).ToString() + ":";//时
                    str += (LuboTime[k, 6]).ToString() + ":";//分
                    str += (LuboTime[k, 7]).ToString() + ":";//秒
                    str += ((LuboTime[k, 9] << 8) + LuboTime[k, 8]).ToString();//毫秒
                    dataGridView1.Rows[i].Cells[1].Value = str;
                    switch (LuboTime[k, 10])
                    { 
                        case 1:
                            dataGridView1.Rows[i].Cells[2].Value = "告警触发";
                            break;
                        case 2:
                            dataGridView1.Rows[i].Cells[2].Value = "自检触发";
                            break;
                        case 3:
                            dataGridView1.Rows[i].Cells[2].Value = "跳闸触发";
                            break;
                        case 4:
                            dataGridView1.Rows[i].Cells[2].Value = "其他触发";
                            break;
                        case 5:
                            dataGridView1.Rows[i].Cells[2].Value = "后台触发";
                            break;
                        case 6:
                            dataGridView1.Rows[i].Cells[2].Value = "软启触发";
                            break;
                        case 7:
                            dataGridView1.Rows[i].Cells[2].Value = "预检触发";
                            break;
                        case 8:
                            dataGridView1.Rows[i].Cells[2].Value = "解锁触发";
                            break;
                        case 9:
                            dataGridView1.Rows[i].Cells[2].Value = "闭锁触发";
                            break;
                    }
                    switch (LuboTime[k, 11])
                    {
                        case 1:
                            dataGridView1.Rows[i].Cells[3].Value = "264个电压";
                            break;
                        case 2:
                            dataGridView1.Rows[i].Cells[3].Value = "264个状态";
                            break;
                        case 3:
                            dataGridView1.Rows[i].Cells[3].Value = "前132个电压和状态";
                            break;
                        case 4:
                            dataGridView1.Rows[i].Cells[3].Value = "后132个电压和状态";
                            break;
                        case 5:
                            dataGridView1.Rows[i].Cells[3].Value = "264个电压和状态";
                            break;
                    }
                    i++;
                }
                else
                    continue;
            }
            LuboTimeFlg = false;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)//建立通信连接
        {
            timer1.Enabled = true;
            toolStripButton1.BackColor = Color.Green;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)//断开通信连接
        {
            timer1.Enabled = false;
            toolStripButton1.BackColor = SystemColors.Control;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int length;
            string strss;
            //CommState++;
            int index = 5;
            TestCounter5++;
            while (index > 0 && skt.Available > 0)
            {
              //  index--;
                if (skt.Available > 0)
                {
                    length = skt.ReceiveFrom(RecBuffer, ref endPoint);
                    strss = endPoint.ToString();
                    if (strss.Contains(CommAddr[0, 0]))
                    {
                        //CommState = 0;
                        Deal_Data(RecBuffer, length);
                        //break;
                    }
                }
            }
        }

        private void Deal_Data(byte[] Rec, int length)
        {
            //     datetimetest1;
            //    datetimetest2;
            uint MAXcounter = 12000;
            if (TestCounter4 == 0)
            {
                datetimetest1 = DateTime.Now;
                sw_Lubo = new StreamWriter(Path.GetFullPath("Lubo302\\" + datetimetest1.ToString("yyyy-MM-dd hh-mm-ss") + ".csv"), false);
            }
            if (TestCounter4 == MAXcounter-1)
                datetimetest2 = DateTime.Now;
            if (length == 1400)
            {
                TestCounter4++;
                if (Rec[0] == 0x55 && Rec[0 + 1] == 0xAA)
                {
                    UInt16 sum = 0;
                    for (int j = 0; j < length - 2; j++)
                    {
                        sum += Rec[j];
                    }
                    int sum1 = ((Rec[length - 1] << 8) + Rec[length - 2]) & 0xFFFF;
                    if ((sum & 0xFFFF) == sum1)
                    {
                        StringBuilder str = new StringBuilder();
                        for (int i = 0; i < 700; i++)
                        {
                            str.Append((Rec[2 * i] << 8) + Rec[2 * i + 1]);
                            str.Append(",");
                        }
                        str.Append("\r\n");
                        if (sw_Lubo != null)
                        {//数据写入DAT文件
                            sw_Lubo.Write(str);
                            sw_Lubo.Flush();
                            str.Clear();
                        }
                        
                    }
                }
            }
            //存储完成，关闭文件
            if (TestCounter4 == MAXcounter)
            {
                if (sw_Lubo != null)
                {
                    sw_Lubo.Close();
                    //sw_Lubo[0].Dispose();
                    sw_Lubo = null;
                }
                MessageBox.Show("起始时间:" + datetimetest1.ToString() + ";结束时间:" + datetimetest2.ToString(), "提示", MessageBoxButtons.OK);
            }
            // else if (length < 1400)
            //   TestCounter5++;
            //  else 
            //     TestCounter6++;
            /*
            for (int i = 0; i < length; i++)
            {
                if (Rec[i] == 0x55 && Rec[i + 1] == 0xAA)
                {
                    if (Rec[i + 2] < 0x10)
                    {
                        int sendlength = (Rec[i + 5] << 8) + Rec[i + 4];
                        if (i + sendlength <= length)
                        {//接收录波数据                             
                            UInt16 sum = 0;
                            for (int j = 0; j < sendlength-2; j++)
                            {
                                sum += Rec[i + j];
                            }
                            int sum1 = ((Rec[i + sendlength - 1] << 8) + Rec[i + sendlength - 2]) & 0xFFFF;
                            if ((sum & 0xFFFF) == sum1)
                            {
                                if (sendlength == 1400)
                                {
                                    TestCounter3++;
                                    RedNum = (Rec[i + 7] << 8) + Rec[i + 6];
                                    RecData[2*RedNum] = new byte[694];
                                    RecData[2 * RedNum + 1] = new byte[694];
                                    if (TestCounter1 == RedNum)
                                        TestCounter1++;
                                    else if (TestCounter1 > RedNum)
                                    { 
                                        for(int j=0;j<testindex;j++)
                                            if (TestCounter[j] == RedNum)
                                            {
                                                TestCounter[j] = 0xFFFFFFFF;
                                                TestCounter2--;
                                                break;
                                            }
                                    }
                                    else
                                    {
                                        while (TestCounter1 <= RedNum)
                                        {
                                            TestCounter[testindex] = TestCounter1;
                                            testindex++;
                                            TestCounter2++;
                                            TestCounter1++;
                                        }
                                    }
                                    RecData[2 * RedNum][0] = (byte)((2 * RedNum)&0xFF);
                                    RecData[2 * RedNum][1] = (byte)(((2 * RedNum)>>8) & 0xFF);
                                    RecData[2 * RedNum + 1][0] = (byte)((2 * RedNum + 1) & 0xFF);
                                    RecData[2 * RedNum + 1][1] = (byte)(((2 * RedNum + 1) >> 8) & 0xFF);
                                    for (int j = 1; j < 694; j++)
                                    {
                                        RecData[2 * RedNum][j] = Rec[i + j + 6];
                                        RecData[2 * RedNum + 1][j] = Rec[i + j + 706];
                                    }
                                }
                                else if (sendlength == 22)
                                {
                                    if (Rec[i + 7] == 0xFF && Rec[i + 6] == 0xFF)
                                    {
                                        if (TestCounter2 > 0)
                                        {
                                            if(MessageBox.Show("数据接收完毕，共丢失"+TestCounter2.ToString()+"条数据，是否重传丢失数据？", "提示", MessageBoxButtons.YesNo)==DialogResult.Yes)
                                                ReGetCMD((int)TestCounter2, testindex, TestCounter, Rec, i);                                            
                                        }
                                        else
                                        {
                                            //MessageBox.Show("数据接收完毕，没有数据丢失", "提示", MessageBoxButtons.OK);
                                            Thread tt = new Thread(SaveLubo);
                                            tt.Start();
                                        }
                                        CommStatus = 2;
                                        //this.Invoke(myDelegate, new Object[] { CommStatus });
                                    }
                                }
                                else if (sendlength == 23)
                                {//接收时间信息
                                    for (int j = 0; j < 7; j++)
                                        StartTime[j] = (Rec[i + 2 * j + 7] << 8) + Rec[i + 2 * j + 6];//BCDToDec((Rec[i + 2*j + 7] << 8) + Rec[i + 2*j + 6]);
                                    StartTime[0] += 2000;
                                    CommStatus = 1;
                                    DataSelectFlg=Rec[i+ 20];
                                    //this.Invoke(myDelegate, new Object[] { CommStatus });
                                }
                            }
                        }
                    }
                    else if (Rec[i + 2] == 0x14)
                    { //查询可读录波数据
                        UInt16 sum = 0;
                        int sendlength = (Rec[i + 5] << 8) + Rec[i + 4];
                        for (int j = 0; j < sendlength-2; j++)
                        {
                            sum += Rec[i + j];
                        }
                        if ((sum&0xFFFF) == ((Rec[i + sendlength - 1] << 8) + Rec[i + sendlength - 2]))
                        {
                            LuboTimeFlg = false;
                            for (int j = 0; j < 4;j++ )
                            {
                                    if (Rec[i + 12 * j + 6] == 1)
                                    {
                                        LuboTimeFlg = true;
                                        for (int k = 0; k < 12; k++)
                                            LuboTime[j, k] = Rec[i + 12 * j + 6 + k];                                   
                                    }
                            }
                            if (LuboTimeFlg== false)
                                CommStatus = 9;
                        }
                    }
                    else if (Rec[i + 2] == 0x16)
                    {
                        CommStatus = 3;// MessageBox.Show("校时成功。", "提示", MessageBoxButtons.OK);
                    }
                    else if (Rec[i + 2] == 0x19)
                    {
                        UInt16 sum = 0;
                        for (int j = 0; j < 12; j++)
                        {
                            sum += Rec[i + j];
                        }
                        if (sum == ((Rec[i + 13] << 8) + Rec[i + 12]))
                        {
                            DSPVerInfo[0] = (ushort)((Rec[i + 5] << 8) + Rec[i + 4]);
                            DSPVerInfo[1] = (ushort)((Rec[i + 7] << 8) + Rec[i + 6]);
                            DSPVerInfo[2] = (ushort)((Rec[i + 9] << 8) + Rec[i + 8]);
                            DSPVerInfo[3] = (ushort)((Rec[i + 11] << 8) + Rec[i + 10]);
                            CommStatus = 19;// 返回下位机软件版本信息
                        }
                    }
                    else if (i + 8 <= length)
                    {
                        UInt16 sum = 0;
                        for (int j = 0; j < 6; j++)
                        {
                            sum += Rec[i + j];
                        }
                        if (sum == ((Rec[i + 7] << 8) + Rec[i + 6]))
                        {
                            if (Rec[i + 2] == 0x11 && Rec[i + 4] == 0x55)
                                CommStatus = 4;//MessageBox.Show("当前录波号下无录波数据。", "提示", MessageBoxButtons.OK);
                            else if (Rec[i + 2] == 0x12)//&& Rec[i + 4] == 0x55)
                                CommStatus = 5;//MessageBox.Show("已清除录波号" + Rec[i + 4].ToString() + "的数据。", "提示", MessageBoxButtons.OK);
                            else if (Rec[i + 2] == 0x13)//&& Rec[i + 4] == 0x55)
                                CommStatus = 6;//MessageBox.Show("已触发数据录波，并存入录波号" + Rec[i + 4].ToString() + "。", "提示", MessageBoxButtons.OK);
                            else if (Rec[i + 2] == 0x17)//&& Rec[i + 4] == 0x55)
                                CommStatus = 7; //MessageBox.Show("触发模式设置成功。", "提示", MessageBoxButtons.OK);
                            else if (Rec[i + 2] == 0x18)//&& Rec[i + 4] == 0x55)
                                CommStatus = 8;
                        }
                    }
                }
            }
            this.Invoke(myDelegate, new Object[] { CommStatus });
            */
        }
        private void ReGetCMD(int length,int testcounterlength,uint[] testcounter,byte[] rec,int index)
        {
            SendLength = (int)(2 * (length + 5));
            SendData[0] = 0x55;
            SendData[1] = 0xAA;
            SendData[2] = 0x15;
            SendData[3] = 0x00;
            SendData[4] = rec[index + 2];
            SendData[5] = rec[index + 3];
            SendData[6] = (byte)(SendLength & 0xFF);
            SendData[7] = (byte)((SendLength >> 8) & 0xFF);
            //int k = 0;
            for (int j = 0, k = 0; j < TestCounter2; k++)
            {
                if (testcounter[k] != 0xFFFFFFFF && k < testcounterlength)
                {
                    SendData[2 * j + 8] = (byte)(testcounter[k] & 0xFF);
                    SendData[2 * j + 9] = (byte)((testcounter[k] >> 8) & 0xFF);
                    j++;
                }
            }
            int sum = 0;
            for (int j = 0; j < SendLength - 2; j++)
                sum += SendData[j];
            SendData[SendLength - 2] = (byte)(sum & 0xFF);
            SendData[SendLength - 1] = (byte)((sum >> 8) & 0xFF);
            SendFlg = true;
        }
        private void SaveLubo()
        {
            int datanum = 2 * (RedNum + 1);
            GetRunNum = 0;
            if (radioButton9.Checked == true)
                AnalysisData(datanum);
            else
            {
                SaveDataToFile(datanum, true);
                SaveDataToFile(datanum, false);
            }
            for (int i = 0; i < datanum; i++)
                if (RecData[i] != null)
                    RecData[i] = null;

            MessageBox.Show("数据处理完毕", "提示", MessageBoxButtons.OK);
        }
        private void AnalysisData(int index)
        {
            string sDataTime = "";
            StringBuilder str = new StringBuilder();
            StringBuilder sbReader = new StringBuilder();
            DateTime datatime = DateTime.Now;

            for (int i = 0; i < 6; i++)
                sDataTime += StartTime[i].ToString() + "-"; //+StartTime[1].ToString() + "-" + StartTime[2].ToString() + "-" + StartTime[3].ToString() + "-" + StartTime[4].ToString() + "-" + StartTime[5].ToString() + "-" + packetsTime[6].ToString();
            sDataTime += StartTime[6].ToString();
            StreamWriter sw_Lubo = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + ".csv"), false);

            #region 处理FCK304机箱录波报文
            int []Cmd=new int[264];
            UInt16[] V264 = new ushort[264];
            for (int i = 0; i < index; i++)
            {//存储报文中25个时间点的数据
                str.Append(i + 1);
                str.Append(",");
                //str.Append(i * 30);
                //str.Append(",");
                    for (int k = 0; k < 264; k++)//264个子模块电压值
                    {
                        V264[k] = (ushort)((RecData[i][3 + 2 * k] << 8) + RecData[i][2 + 2 * k]);
                        str.Append((RecData[i][3 + 2 * k] << 8) + RecData[i][2 + 2 * k]);
                        str.Append(",");
                    }
                for (int k = 0; k < 132; k++)//264个子模块指令值
                {
                    Cmd[2*k]=RecData[i][531 + k] & 0x0F;
                    str.Append(RecData[i][531 + k] & 0x0F);
                    str.Append(",");
                    Cmd[2*k+1]=(RecData[i][531 + k] >> 4) & 0x0F;
                    str.Append((RecData[i][531 + k] >> 4) & 0x0F);
                    str.Append(",");
                }
                for (int k = 0; k < 9; k++)//其他
                {
                    str.Append((RecData[i][664 + 2 * k] << 8) + RecData[i][663 + 2 * k]);
                    str.Append(",");
                }
                //RecData[i] = null;
                UInt16[,] dealdataV = new ushort[2, V_Group];
                int[] dealdataN = new int[V_Group];
                UInt16 Idir = (ushort)((RecData[i][664] << 8) + RecData[i][663]);
                int result = 0;
                V_Run = (ushort)((RecData[i][672] << 8) + RecData[i][671]);//投入模块数
                int num_of_one_group = V_All / V_Group;
                int[] run_num_groups = new int[V_Group];
                int max_min_num = V_Run % V_Group;
                int run_num = V_Run / V_Group;
                int []tempcmd=new int[264];
                for (int ii = 0; ii < V_Group; ii++)
                {
                    UInt32 tempsum = 0;
                    for (int k = 0; k < num_of_one_group; k++)
                        tempsum += (ushort)((RecData[i][3 + 2 * k + ii * num_of_one_group] << 8) + RecData[i][2 + 2 * k + ii * num_of_one_group]);
                    dealdataV[0, ii] = (ushort)(tempsum / num_of_one_group);
                    dealdataV[1, ii] = (ushort)ii;
                    dealdataN[ii] = run_num;
                }
                SortData(ref dealdataV, V_Group);
               // dealdataN[ii] = (ushort)((RecData[i][3 + 2 * (ii + V_Group)] << 8) + RecData[i][2 + 2 * (ii + V_Group)]);
                if (Idir == 0xAAAA)
                {
                    for (int ii = 0; ii < max_min_num; ii++)
                    {
                        dealdataN[dealdataV[1, ii]] = run_num + 1;
                    }
                    
                    //    for (int ii = 0; ii < max_min_num; ii++)
                    //    {
                    //        if (dealdataN[dealdataV[1, ii]] != run_num + 1)
                    //            result = 1;
                    //    }
                    //for (int ii = max_min_num; ii < V_Group; ii++)
                    //{
                    //    if (dealdataN[dealdataV[1, ii]] != run_num)
                    //        result = 1;
                    //}
                }
                else if (Idir == 0x5555)
                {
                    max_min_num = V_Group - max_min_num;
                    for (int ii = max_min_num; ii < V_Group; ii++)
                    {
                        dealdataN[dealdataV[1, ii]] = run_num + 1;
                    }
                    //for (int ii = 0; ii < max_min_num; ii++)
                    //{
                    //    if (dealdataN[dealdataV[1, ii]] != run_num)
                    //        result = 1;
                    //}
                    //for (int ii = max_min_num; ii < V_Group; ii++)
                    //{
                    //    if (dealdataN[dealdataV[1, ii]] != run_num + 1)
                    //        result = 1;
                    //}
                }
                else
                {
                    result = 2;
                }
                if (result != 2)
                {
                    for (int ii = 0; ii < V_Group; ii++)
                    {
                        SortData(V264, ref tempcmd, dealdataN[ii], num_of_one_group, num_of_one_group *ii, Idir);
                    }
                    for (int ii = 0; ii < V_All; ii++)
                    {
                        if (tempcmd[ii] != Cmd[ii])
                        {
                            result = 1;
                            break;
                        }
                    }
                }
                str.Append(result);
                str.Append("\r\n");
                if (sw_Lubo != null)
                {//数据写入DAT文件
                    sw_Lubo.Write(str);
                    sw_Lubo.Flush();
                    str.Clear();
                }
            }
            //存储完成，关闭文件
            if (sw_Lubo != null)
            {
                sw_Lubo.Close();
                //sw_Lubo[0].Dispose();
                sw_Lubo = null;
            }
            #endregion
        }
        private void SortData(ref UInt16[,] data,int length)
        {
            ushort temp;
            for(int i=0;i<length-1;i++)
                for (int j = i+1; j < length; j++)
                {
                    if (data[0, i] > data[0, j])
                    {
                        temp = data[0, i];
                        data[0, i] = data[0, j];
                        data[0, j] = temp;
                        temp = data[1, i];
                        data[1, i] = data[1, j];
                        data[1, j] = temp;
                    }
                    else if(data[0, i] == data[0, j]&&data[1, i] > data[1, j])
                    {
                        temp = data[0, i];
                        data[0, i] = data[0, j];
                        data[0, j] = temp;
                        temp = data[1, i];
                        data[1, i] = data[1, j];
                        data[1, j] = temp;
                    }
                }
        }
        private void SortData(ushort[] vdata,ref int[] cdata, int run_num, int length, int offset, int dir)
        {
            ushort[,] temp_v = new ushort[2, length];
            for (int i = 0; i < length; i++)
            {
                temp_v[0, i] = vdata[offset + i];
                temp_v[1, i] = (ushort)i;
            }
            SortData(ref temp_v, length);
            if (dir == 0xAAAA)
            {
                for (int k = 0; k < run_num; k++)
                    cdata[offset + temp_v[1, k]] = 5;
                for (int k = run_num; k < length; k++)
                    cdata[offset + temp_v[1, k]] = 6;
            }
            else if (dir == 0x5555)
            {
                run_num = length - run_num;
                for (int k = 0; k < run_num; k++)
                    cdata[offset + temp_v[1, k]] = 6;
                for (int k = run_num; k < length; k++)
                    cdata[offset + temp_v[1, k]] = 5;
            }
        }
        private void SaveDataToFile(int index,bool flg)
        {
            string sDataTime="";
            int getRunNumResult = 2;
            int counterRunNum = 0;
            StringBuilder str = new StringBuilder();
            StringBuilder sbReader = new StringBuilder();
            StreamWriter[] sw_Lubo;
            sw_Lubo = new StreamWriter[3];
            DateTime datatime = DateTime.Now;

            for (int i = 0; i < 6; i++)
                sDataTime += StartTime[i].ToString() + "-"; //+StartTime[1].ToString() + "-" + StartTime[2].ToString() + "-" + StartTime[3].ToString() + "-" + StartTime[4].ToString() + "-" + StartTime[5].ToString() + "-" + packetsTime[6].ToString();
            sDataTime += StartTime[6].ToString();
            if (flg == true)
            {
                sw_Lubo[0] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + "t.DAT"), false);
                sw_Lubo[1] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + "t.HDR"), false);
                sw_Lubo[2] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + "t.CFG"), false);
            }
            else
            {
                sw_Lubo[0] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + ".DAT"), false);
                sw_Lubo[1] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + ".HDR"), false);
                sw_Lubo[2] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + ".CFG"), false);
            }
            GetCFGFile(StartTime, sbReader,1);//
            sw_Lubo[1].Write("");//sbReader.ToString());
            sw_Lubo[2].Write(sbReader);
            sw_Lubo[2].Close();
            sw_Lubo[2] = null;
            if (V_Factor != 1)
            {
                if (flg == true)
                    sw_Lubo[2] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + "Ft.CFG"), false);
                else
                    sw_Lubo[2] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + "F.CFG"), false);
                sbReader.Clear();
                GetCFGFile(StartTime, sbReader, V_Factor);
                sw_Lubo[2].Write(sbReader);
                sw_Lubo[2].Close();
                sw_Lubo[2] = null;
            }

            #region 处理FCK304机箱录波报文            
            if (DataSelectFlg == 5)
            {
                index /= 2;
                for (int i = 0; i < index; i++)
                {//存储报文中25个时间点的数据
                    str.Append(i + 1);
                    str.Append(",");
                    str.Append(i * 60);
                    str.Append(",");

                    for (int k = 0; k < 264; k++)//264个子模块电压值
                    {
                        str.Append((RecData[2*i][3 + 2 * k] << 8) + RecData[2*i][2 + 2 * k]);
                        str.Append(",");
                    }
                    for (int k = 0; k < 264; k++)//264个子模块状态值
                    {
                        if (flg == true)
                            str.Append((RecData[2 * i + 1][3 + 2 * k] << 8) + RecData[2 * i + 1][2 + 2 * k]);
                        else
                            str.Append(((RecData[2 * i + 1][3 + 2 * k] << 8) + RecData[2 * i+1][2 + 2 * k]) & 0x3FFF);
                        str.Append(",");
                    }
                    for (int k = 0; k < 132; k++)//264个子模块指令值
                    {
                        str.Append(RecData[2*i][531 + k] & 0x0F);
                        str.Append(",");
                        str.Append((RecData[2*i][531 + k] >> 4) & 0x0F);
                        str.Append(",");
                    }
                    for (int k = 0; k < 9; k++)//其他
                    {
                        str.Append((RecData[2*i][664 + 2 * k] << 8) + RecData[2*i][663 + 2 * k]);
                        str.Append(",");
                    }
                    //RecData[i] = null;

                    str.Append("\r\n");
                    if (sw_Lubo[0] != null)
                    {//数据写入DAT文件
                        sw_Lubo[0].Write(str);
                        sw_Lubo[0].Flush();
                        str.Clear();
                    }
                }
            }
            else
            {
                for (int i = 0; i < index; i++)
                {//存储报文中25个时间点的数据
                    counterRunNum = 0;
                    str.Append(i + 1);
                    str.Append(",");
                    str.Append(i * 30);
                    str.Append(",");

                    if (flg == true)
                    {
                        for (int k = 0; k < 264; k++)//264个子模块电压值
                        {
                            if (DataSelectFlg == 4 || DataSelectFlg == 3)
                            {
                                if (k > 131)
                                    str.Append(((RecData[i][3 + 2 * k] << 8) + RecData[i][2 + 2 * k]) & 0x3FFF);
                                else
                                    str.Append((RecData[i][3 + 2 * k] << 8) + RecData[i][2 + 2 * k]);
                            }
                            else if (DataSelectFlg == 2)
                            {
                                str.Append(((RecData[i][3 + 2 * k] << 8) + RecData[i][2 + 2 * k]) & 0x3FFF);
                                if ((RecData[i][3 + 2 * k] & 0x80) == 0x80)
                                    counterRunNum++;
                            }
                            else
                                str.Append((RecData[i][3 + 2 * k] << 8) + RecData[i][2 + 2 * k]);
                            str.Append(",");
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 264; k++)//264个子模块电压值
                        {
                            str.Append((RecData[i][3 + 2 * k] << 8) + RecData[i][2 + 2 * k]);
                            str.Append(",");
                            if ((RecData[i][3 + 2 * k] & 0x80) == 0x80)
                                counterRunNum++;
                        }
                    }
                    for (int k = 0; k < 132; k++)//264个子模块指令值
                    {
                        str.Append(RecData[i][531 + k] & 0x0F);
                        str.Append(",");
                        str.Append((RecData[i][531 + k] >> 4) & 0x0F);
                        str.Append(",");
                    }
                    for (int k = 0; k < 9; k++)//其他
                    {
                        str.Append((RecData[i][664 + 2 * k] << 8) + RecData[i][663 + 2 * k]);
                        str.Append(",");
                    }
                    if (DataSelectFlg == 2)
                    {
                        if (i > 0)
                        {
                            if (checkBox5.Checked == true && GetRunNum >= 208)
                                GetRunNum -= 208;
                            //if (GetRunNum == counterRunNum)
                            //    getRunNumResult = 0;
                            //else
                            //    getRunNumResult = 1;
                            getRunNumResult = GetRunNum - counterRunNum;
                        }
                        GetRunNum = (RecData[i][672] << 8) + RecData[i][671];
                        str.Append(counterRunNum);
                        str.Append(",");
                        str.Append(getRunNumResult);
                        str.Append(",");
                    }
                    //RecData[i] = null;

                    str.Append("\r\n");
                    if (sw_Lubo[0] != null)
                    {//数据写入DAT文件
                        sw_Lubo[0].Write(str);
                        sw_Lubo[0].Flush();
                        str.Clear();
                    }
                }
            }
            //存储完成，关闭文件
            if (sw_Lubo[0] != null)
            {
                sw_Lubo[0].Close();
                sw_Lubo[0] = null;
            }

            #endregion
        }

        private bool GetCFGFile(int[] packetstime, StringBuilder sbreader,double factor)
        {
            //string str;
            //while ((str = sr.ReadLine()) != null)
            //{
            //    sbreader.Append(str);
            //    sbreader.Append("\r\n");//_separator 设为空格   
            //}
           // sbreader.Append("SS_S1P1PCPA2,COP\r\n800,800A,0D\r\n");
            string Vstr = ",,,V," + factor.ToString() + ",0,0,-32768,32767\r\n";
            if (DataSelectFlg == 5)
            {
                sbreader.Append("SS_S1P1PCPA2,COP\r\n801,801A,0D\r\n");
                for (int i = 0; i < 264; i++)
                    sbreader.Append((i + 1).ToString() + ",V" + (i + 1).ToString() + Vstr);
                for (int i = 0; i < 264; i++)
                    sbreader.Append((i + 265).ToString() + ",S" + (i + 1).ToString() + ",,,V,1,0,0,-32768,32767\r\n");
                for (int i = 0; i < 264; i++)
                    sbreader.Append((i + 529).ToString() + ",C" + (i + 1).ToString() + ",,,V,1,0,0,-32768,32767\r\n");
                sbreader.Append("793,I_Dir,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("794,Run_Mode,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("795,C_Cmd,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("796,Fault_Num,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("797,Touru_Num,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("798,Trip,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("799,Alarm,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("800,Self-Checking,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("801,ZhuBei,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("50\r\n1\r\n60,");
                sbreader.Append((RedNum + 1).ToString() + "\r\n");
            }
            else
            {
                if (DataSelectFlg == 2)
                    sbreader.Append("SS_S1P1PCPA2,COP\r\n539,539A,0D\r\n");
                else
                    sbreader.Append("SS_S1P1PCPA2,COP\r\n537,537A,0D\r\n");
                if (DataSelectFlg == 1)
                {
                    for (int i = 0; i < 264; i++)
                        sbreader.Append((i + 1).ToString() + ",V" + (i + 1).ToString() + Vstr);
                }
                else if (DataSelectFlg == 2)
                {
                    for (int i = 0; i < 264; i++)
                        sbreader.Append((i + 1).ToString() + ",S" + (i + 1).ToString() + ",,,V,1,0,0,-32768,32767\r\n");
                }
                else if (DataSelectFlg == 3)
                {
                    for (int i = 0; i < 132; i++)
                        sbreader.Append((i + 1).ToString() + ",V" + (i + 1).ToString() + Vstr);
                    for (int i = 0; i < 132; i++)
                        sbreader.Append((i + 133).ToString() + ",S" + (i + 1).ToString() + ",,,V,1,0,0,-32768,32767\r\n");
                }
                else if (DataSelectFlg == 4)
                {
                    for (int i = 132; i < 264; i++)
                        sbreader.Append((i + 1).ToString() + ",V" + (i + 1).ToString() + Vstr);
                    for (int i = 132; i < 264; i++)
                        sbreader.Append((i + 133).ToString() + ",S" + (i + 1).ToString() + ",,,V,1,0,0,-32768,32767\r\n");
                }
                for (int i = 0; i < 264; i++)
                    sbreader.Append((i + 265).ToString() + ",C" + (i + 1).ToString() + ",,,V,1,0,0,-32768,32767\r\n");
                sbreader.Append("529,I_Dir,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("530,Run_Mode,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("531,C_Cmd,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("532,Fault_Num,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("533,Touru_Num,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("534,Trip,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("535,Alarm,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("536,Self-Checking,,,V,1,0,0,0,65535\r\n");
                sbreader.Append("537,Zhubei,,,V,1,0,0,0,65535\r\n");
                if (DataSelectFlg == 2)
                {
                    sbreader.Append("538,Get_Touru_Num,,,V,1,0,0,0,65535\r\n");
                    sbreader.Append("539,Counter_Touru_Result,,,V,1,0,0,-32768,32767\r\n");
                }
                sbreader.Append("50\r\n1\r\n30,");
                sbreader.Append((2 * RedNum + 2).ToString() + "\r\n");
            }
            string strtime = StartTime[2].ToString() + "/" + StartTime[1].ToString() + "/" + StartTime[0].ToString() + "," + StartTime[3].ToString() + ":" + StartTime[4].ToString() + ":";// +".";
            sbreader.Append(strtime + StartTime[5].ToString());
            sbreader.Append(".");
            sbreader.Append(StartTime[6] * 1000);
            sbreader.Append("\r\n");
            sbreader.Append(strtime + (packetstime[5]).ToString());
            sbreader.Append(".");
            if (DataSelectFlg == 5)
                sbreader.Append(packetstime[6] * 1000 + 60 * (RedNum + 1));
            else
                sbreader.Append(packetstime[6] * 1000 + 30 * (2 * RedNum + 2));
            sbreader.Append("\r\n");
            sbreader.Append("ASCII");
            sbreader.Append("\r\n");
            //sw.Write(sbreader);
            return true;
        }
        //录波查询
        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton9.Checked == true)
            {
                if (V_Group == 0)
                {
                    MessageBox.Show("数据分析需要的参数不完整，请设置！", "警告", MessageBoxButtons.OK);
                    return;
                }
            }
            testindex = 0;
            TestCounter1 = 0;
            TestCounter2 = 0;
            TestCounter3 = 0;
            for (int i = 0; i < TestCounter.Length; i++)
                TestCounter[i] = 0;
            SendLength = 8;
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("请选择要操作的录波号。", "警告", MessageBoxButtons.OK);
                return;
            }
            else
            {
                DeviceAddr = (byte)comboBox1.SelectedIndex;//System.Convert.ToByte(textBox1.Text);                
                if (LuboTime[comboBox1.SelectedIndex, 11] == 5)
                    toolStripProgressBar1.Maximum = 10000;
                else
                    toolStripProgressBar1.Maximum = 5000;
            }

            SendData[0] = 0x55;
            SendData[1] = 0xAA;
            SendData[2] = 0x11;// (byte)(SendCounter & 0xFF);
            SendData[3] = 0;// (byte)((SendCounter >> 8) & 0xFF);
            SendData[4] = DeviceAddr;
            SendData[5] = 0;
            int sum = SendData[0] + SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5];
            SendData[6] = (byte)(sum & 0xFF);
            SendData[7] = (byte)((sum >> 8) & 0xFF);
            SendFlg = true;
        }
        //清除录波数据
        private void button2_Click(object sender, EventArgs e)
        {
            SendLength = 8;
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("请选择要操作的录波号。", "警告", MessageBoxButtons.OK);
                return;
            }
            else
                DeviceAddr = (byte)comboBox1.SelectedIndex;//System.Convert.ToByte(textBox1.Text);

            SendData[0] = 0x55;
            SendData[1] = 0xAA;
            SendData[2] = 0x12;// (byte)(SendCounter & 0xFF);
            SendData[3] = 0;// (byte)((SendCounter >> 8) & 0xFF);
            SendData[4] = DeviceAddr;
            SendData[5] = 0;
            int sum = SendData[0] + SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5];
            SendData[6] = (byte)(sum & 0xFF);
            SendData[7] = (byte)((sum >> 8) & 0xFF);
            SendFlg = true;
        }
        //触发一次数据录波
        private void button3_Click(object sender, EventArgs e)
        {
            SendLength = 8;
            SendData[0] = 0x55;
            SendData[1] = 0xAA;
            SendData[2] = 0x13;// (byte)(SendCounter & 0xFF);
            SendData[3] = 0;// (byte)((SendCounter >> 8) & 0xFF);
            SendData[4] = 0xFF;
            SendData[5] = 0xFF;
            int sum = SendData[0] + SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5];
            SendData[6] = (byte)(sum & 0xFF);
            SendData[7] = (byte)((sum >> 8) & 0xFF);
            SendFlg = true;
        }
        //查询可读取的录波数据（0x14）
        private void button4_Click(object sender, EventArgs e)
        {
            SendLength = 8;            
            SendData[0] = 0x55;
            SendData[1] = 0xAA;
            SendData[2] = 0x14;// (byte)(SendCounter & 0xFF);
            SendData[3] = 0;// (byte)((SendCounter >> 8) & 0xFF);
            SendData[4] = 0xFF;
            SendData[5] = 0xFF;
            int sum = SendData[0] + SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5];
            SendData[6] = (byte)(sum & 0xFF);
            SendData[7] = (byte)((sum >> 8) & 0xFF);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4;j++ )
                    dataGridView1.Rows[i].Cells[j].Value = "";
                for (int j = 0; j < 12;j++ )
                    LuboTime[i,j] = 0;
            }
            SendFlg = true;
        }
        //校时报文（时间数据为BCD码）（0x16）
        private void button5_Click(object sender, EventArgs e)
        {
            DateTime setTime = DateTime.Now;
            SendLength = 22;
            SendData[0] = 0x55;
            SendData[1] = 0xAA;
            SendData[2] = 0x16;// (byte)(SendCounter & 0xFF);
            SendData[3] = 0;// (byte)((SendCounter >> 8) & 0xFF);
            SendData[4] = 22;
            SendData[5] = 0;
            SendData[6] = (byte)(GetBCD(setTime.Year) & 0xFF);
            SendData[7] = (byte)((GetBCD(setTime.Year) >> 8) & 0xFF);
            SendData[8] = (byte)(GetBCD(setTime.Month) & 0xFF);
            SendData[9] = (byte)((GetBCD(setTime.Month) >> 8) & 0xFF);
            SendData[10] = (byte)(GetBCD(setTime.Day) & 0xFF);
            SendData[11] = (byte)((GetBCD(setTime.Day) >> 8) & 0xFF);
            SendData[12] = (byte)(GetBCD(setTime.Hour) & 0xFF);
            SendData[13] = (byte)((GetBCD(setTime.Hour) >> 8) & 0xFF);
            SendData[14] = (byte)(GetBCD(setTime.Minute) & 0xFF);
            SendData[15] = (byte)((GetBCD(setTime.Minute) >> 8) & 0xFF);
            SendData[16] = (byte)(GetBCD(setTime.Second) & 0xFF);
            SendData[17] = (byte)((GetBCD(setTime.Second) >> 8) & 0xFF);
            SendData[18] = (byte)(GetBCD(setTime.Millisecond) & 0xFF);
            SendData[19] = (byte)((GetBCD(setTime.Millisecond) >> 8) & 0xFF);
            int sum = 0;
            for (int i = 0; i < SendLength-2; i++)
                sum += SendData[i];
            SendData[20] = (byte)(sum & 0xFF);
            SendData[21] = (byte)((sum >> 8) & 0xFF);
            //richTextBox1.Text = "";
            //LuboTime = "";
            SendFlg = true;
        }
        //转换为BCD码的功能函数
        private ushort GetBCD(int data)
        {
            ushort retdata=0;
            ushort dealdata=(ushort)(data&0xFFFF);
            int i = 0;
            while (dealdata > 0)
            {
                retdata += (ushort)((dealdata % 10) << (i * 4));
                dealdata = (ushort)(dealdata / 10);
                i++;
            }
            return retdata;
        }
        //转换为BCD码的功能函数
        private int BCDToDec(int data)
        {
            int retdata = 0;
            ushort dealdata = (ushort)(data & 0xFFFF);
            for(int i=3;i>=0;i--)
                retdata = (ushort)(retdata * 10 + ((dealdata >> (4 * i))&0x0F));
           
            return retdata;
        }
        //选择录波的数据
        private void button6_Click(object sender, EventArgs e)
        {
            //textBox2.Text = string.Format("{0:x}",GetBCD(System.Convert.ToInt32(textBox1.Text)));
            SendLength = 8;
            SendData[0] = 0x55;
            SendData[1] = 0xAA;
            SendData[2] = 0x18;// (byte)(SendCounter & 0xFF);
            SendData[3] = 0;// (byte)((SendCounter >> 8) & 0xFF);
            if (radioButton1.Checked == true)
            {
                //DataSelectFlg = 1;
                SendData[4] = 1;
            }
            else if (radioButton2.Checked == true)
            {
                //DataSelectFlg = 2;
                SendData[4] = 2;
            }
            else if (radioButton3.Checked == true)
            {
                //DataSelectFlg = 3;
                SendData[4] = 3;
            }
            else if (radioButton4.Checked == true)
            {
                //DataSelectFlg = 4;
                SendData[4] = 4;
            }
            else if (radioButton5.Checked == true)
            {
                //DataSelectFlg = 5;
                SendData[4] = 5;
            }
            else
                SendData[4] = 0;
            SendData[5] = 0x00;
            //if (checkBox7.Checked == true)
            //    SendData[5] = (byte)(SendData[5] + 0x0F);
            //if (checkBox4.Checked == true)
            //    SendData[5] = (byte)(SendData[5] + 0xF0);
            int sum = SendData[0] + SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5];
            SendData[6] = (byte)(sum & 0xFF);
            SendData[7] = (byte)((sum >> 8) & 0xFF);
            SendFlg = true;
        }
        //设置触发模式
        private void button7_Click(object sender, EventArgs e)
        {
            SendLength = 8;
            SendData[0] = 0x55;
            SendData[1] = 0xAA;
            SendData[2] = 0x17;// (byte)(SendCounter & 0xFF);
            SendData[3] = 0;// (byte)((SendCounter >> 8) & 0xFF);
            SendData[4] = 0x00;
            if(checkBox1.Checked==true)
                SendData[4] = (byte)(SendData[4]+0x03);
            if (checkBox2.Checked == true)
                SendData[4] = (byte)(SendData[4] + 0x0C);            
            if (checkBox3.Checked == true)
                SendData[4] = (byte)(SendData[4] + 0x30);
            if (checkBox4.Checked == true)
                SendData[4] = (byte)(SendData[4] + 0xC0);
            SendData[5] = 0x00;
            if (checkBox6.Checked == true)
                SendData[5] = (byte)(SendData[5] + 0x03);
            if (checkBox7.Checked == true)
                SendData[5] = (byte)(SendData[5] + 0x0C);
            if (checkBox8.Checked == true)
                SendData[5] = (byte)(SendData[5] + 0x30);
            if (checkBox9.Checked == true)
                SendData[5] = (byte)(SendData[5] + 0xC0);
            int sum = SendData[0] + SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5];
            SendData[6] = (byte)(sum & 0xFF);
            SendData[7] = (byte)((sum >> 8) & 0xFF);
            SendFlg = true;
        }

        private void WinShow(int flg)
        {
            if (flg == 1)
            {
                toolStripStatusLabel1.Text = "数据正在接收中";
                toolStripProgressBar1.Visible = true;
            }
            else if (flg == 2)
            {
                toolStripStatusLabel1.Text = "数据接收完毕";
                toolStripProgressBar1.Visible = false;
            }
            else if (flg == 3)
            {
                toolStripStatusLabel1.Text = "校时成功";
            }
            else if (flg == 4)
            {
                toolStripStatusLabel1.Text = "当前录波号下无录波数据";
            }
            else if (flg == 5)
            {
                toolStripStatusLabel1.Text = "已清除录波数据";
            }
            else if (flg == 6)
            {
                toolStripStatusLabel1.Text = "已触发数据录波";
            }
            else if (flg == 7)
            {
                toolStripStatusLabel1.Text = "触发模式设置成功";
            }
            else if (flg == 8)
            {
                toolStripStatusLabel1.Text = "录波数据选择指令下发成功";
            }
            else if (flg == 9)
            {
                toolStripStatusLabel1.Text = "没有可查询的录波数据";
            }
            else if (flg == 19)
            {
                toolStripStatusLabel1.Text = "DSP软件版本信息已返回";
                textBox4.Text = "V" + string.Format("{0:f2}", DSPVerInfo[1] / 100.0);
                string str1 = string.Format("{0:x}", DSPVerInfo[0]);
                string str2 = "0x";
                for (int i = 0; i < 4 - str1.Length; i++)
                    str2 += "0";
                textBox5.Text = str2+str1;

                textBox3.Text = "V" + string.Format("{0:f2}", BCDToDec(DSPVerInfo[3]) / 100.0);
                str1 = string.Format("{0:x}", DSPVerInfo[2]);
                str2 = "0x";
                for (int i = 0; i < 4 - str1.Length; i++)
                    str2 += "0";
                textBox1.Text = str2 + str1;
            }
        }

        private void FCK302_Lubo_FormClosing(object sender, FormClosingEventArgs e)
        {
            tstrmenuitem.Enabled = true;
            toolstrbtn.Visible = false;
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton10.Checked == true)
            {
                groupBox2.Visible = true;
                groupBox4.Visible = false;
                V_All = 0;
                V_Group = 0; 
                V_Run = 0;
                textBox6.Text = "";
                textBox2.Text = "";
                textBox7.Text = "";
            }
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton9.Checked == true)
            {
                groupBox4.Visible = true;
                groupBox2.Visible = false;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                V_All = System.Convert.ToInt16(textBox6.Text);
                V_Group = System.Convert.ToInt16(textBox2.Text);
                V_Run = System.Convert.ToInt16(textBox7.Text);
                if(V_All%V_Group!=0)
                {
                    MessageBox.Show("统计模块数无法分组！", "警告", MessageBoxButtons.OK);
                    return;
                }
            }
            catch(Exception)
            {
                MessageBox.Show("输入数据非法！", "警告", MessageBoxButtons.OK);
                return;
            }
            SendLength = 8;
            SendData[0] = 0x55;
            SendData[1] = 0xAA;
            SendData[2] = 0x18;// (byte)(SendCounter & 0xFF);
            SendData[3] = 0;// (byte)((SendCounter >> 8) & 0xFF);
            SendData[4] = 7;
            SendData[5] = 0x00;
            //if (checkBox7.Checked == true)
            //    SendData[5] = (byte)(SendData[5] + 0x0F);
            //if (checkBox4.Checked == true)
            //    SendData[5] = (byte)(SendData[5] + 0xF0);
            int sum = SendData[0] + SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5];
            SendData[6] = (byte)(sum & 0xFF);
            SendData[7] = (byte)((sum >> 8) & 0xFF);
            SendFlg = true;
        }
        //查询DSP软件信息指令
        private void button8_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            SendLength = 8;
            SendData[0] = 0x55;
            SendData[1] = 0xAA;
            SendData[2] = 0x19;// (byte)(SendCounter & 0xFF);
            SendData[3] = 0;// (byte)((SendCounter >> 8) & 0xFF);
            SendData[4] = 0x00;
            SendData[5] = 0x00;  
            int sum = SendData[0] + SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5];
            SendData[6] = (byte)(sum & 0xFF);
            SendData[7] = (byte)((sum >> 8) & 0xFF);
            SendFlg = true;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                V_Factor = System.Convert.ToDouble(textBox8.Text);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "错误", MessageBoxButtons.OK);
                V_Factor = 1;
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            textBox6.Text = TestCounter4.ToString();
            textBox2.Text = TestCounter5.ToString();
            textBox7.Text = TestCounter6.ToString();
            textBox3.Text = datetimetest1.ToString();
            textBox4.Text = datetimetest2.ToString();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            textBox6.Text = "";
            textBox2.Text = "";
            textBox7.Text = "";
            TestCounter4=0;
            TestCounter5=0;
            TestCounter6 = 0;
           // datetimetest1 = DateTime.Now;
        }

        private void FCK302_Lubo_Load(object sender, EventArgs e)
        {

        }

    }
}
