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

namespace FCK302_Lubo
{
    public partial class Form1 : Form
    {
        private byte[][] RecData;
        private byte[] SendData;
        private byte[] RecBuffer;
        private string[,] CommAddr;
        private string LuboTime;
        //private int CommState;
        private int SendCounter;
        private byte SendCode;
        private int SendLength;
        private byte DeviceAddr;
       // private int CommStatus;
        private int testindex;
        private int[] StartTime;
        private int RedNum;
        private uint DataPointNum;
        private bool SendFlg;
        private bool ReGetFlg;
        private uint TestCounter1;
        private uint TestCounter2;
        private uint TestCounter3;
        private uint[] TestCounter;
        Socket skt;
        EndPoint endPoint;
        IPEndPoint ipEndPoint;

        DateTime[] datetimetest;
        int datetimeindex;

        public Form1()
        {
            InitializeComponent();

            datetimetest = new DateTime[100];
            datetimeindex = 0;

            SendFlg = false;
            ReGetFlg = false;
            SendCounter = 0;
            testindex = 0;
            LuboTime = "";
            RecData = new byte[10000][];
            //for (int i = 0; i < 12001; i++)
            //    RecData[i] = new byte[679];
            TestCounter = new uint[10000];
            SendData = new byte[100];
            RecBuffer = new byte[2048];
            StartTime = new int[7];
            RedNum = 0;
            //CommState = 25;

            CommAddr = new string[1, 2];
            CommAddr[0, 0] = "192.168.1.137";
            CommAddr[0, 1] = "3106";

            ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            endPoint = (EndPoint)ipEndPoint;
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            skt.ReceiveBufferSize = 100 * 1024;
            IPEndPoint tempEndPoint = new IPEndPoint(IPAddress.Any, 7006);
            skt.Bind(tempEndPoint);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (datetimeindex < 100)
            {
                datetimetest[datetimeindex] = DateTime.Now;
                datetimeindex++;
            }

            if (LuboTime != "" && richTextBox1.Text == "")
                richTextBox1.AppendText(LuboTime);

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
            while (index > 0 && skt.Available > 0)
            {
                index--;
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
                                            MessageBox.Show("数据接收完毕，没有数据丢失", "提示", MessageBoxButtons.OK);
                                            Thread tt = new Thread(SaveLubo);
                                            tt.Start();
                                        }
                                    }
                                    else
                                    {//接收时间信息
                                        for (int j = 0; j < 7; j++)
                                            StartTime[j] = BCDToDec((Rec[i + 2*j + 7] << 8) + Rec[i + 2*j + 6]);
                                    }
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
                            for (int j = 0; j < 4;j++ )
                            {
                                if (Rec[i + 10 * j + 6] == 1)
                                {
                                    LuboTime+="录波号:";
                                    LuboTime+=j.ToString();
                                    LuboTime+=",\t录波时间:";
                                    LuboTime+=BCDToDec((Rec[i + 10 * j + 8] << 8) + Rec[i + 10 * j + 7]).ToString() + "-";//年
                                    LuboTime+=BCDToDec(Rec[i + 10 * j + 9]).ToString()+"-";//月
                                    LuboTime+=BCDToDec(Rec[i + 10 * j + 10]).ToString() + " ";//日
                                    LuboTime+=BCDToDec(Rec[i + 10 * j + 11]).ToString() + ":";//时
                                    LuboTime+=BCDToDec(Rec[i + 10 * j + 12]).ToString() + ":";//分
                                    LuboTime += BCDToDec(Rec[i + 10 * j + 13]).ToString() + ":";//秒
                                    LuboTime += BCDToDec((Rec[i + 10 * j + 15] << 8) + Rec[i + 10 * j + 14]).ToString() + "\r\n";//毫秒
                                }
                            }
                        }
                    }
                    else if (Rec[i + 2] == 0x16)
                    {
                        MessageBox.Show("校时成功。", "提示", MessageBoxButtons.OK);
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
                                MessageBox.Show("当前录波号下无录波数据。", "提示", MessageBoxButtons.OK);
                            else if (Rec[i + 2] == 0x12)//&& Rec[i + 4] == 0x55)
                                MessageBox.Show("已清除录波号" + Rec[i + 4].ToString() + "的数据。", "提示", MessageBoxButtons.OK);
                            else if (Rec[i + 2] == 0x13)//&& Rec[i + 4] == 0x55)
                                MessageBox.Show("已触发数据录波，并存入录波号" + Rec[i + 4].ToString() + "。", "提示", MessageBoxButtons.OK);
                            else if (Rec[i + 2] == 0x17)//&& Rec[i + 4] == 0x55)
                                MessageBox.Show("触发模式设置成功。", "提示", MessageBoxButtons.OK);
                        }
                    }
                }
            }
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
            int index = 2*(RedNum+1);
            string sDataTime;
            byte[] packetsTime = new byte[8];
            StringBuilder str = new StringBuilder();
            StringBuilder sbReader = new StringBuilder();
            StreamWriter[] sw_Lubo;
            //StreamReader sr_Lubo = new StreamReader(Path.GetFullPath("Lubo302\\configer.CFG"), Encoding.ASCII);
            sw_Lubo = new StreamWriter[3];
            DateTime datatime = DateTime.Now;
            //for (int i = 0; i < 8; i++)
            //    packetsTime[i] = SaveLuboData[index][0, i + 1020];
            //StartTime[0] = datatime.Year;
            //StartTime[1] = datatime.Month;
            //StartTime[2] = datatime.Day;
            //StartTime[3] = datatime.Hour;
            //StartTime[4] = datatime.Minute;
            //StartTime[5] = datatime.Second;
            //StartTime[6] = datatime.Millisecond;
            sDataTime = StartTime[0].ToString() + "-" + StartTime[1].ToString() + "-" + StartTime[2].ToString() + "-" + StartTime[3].ToString() + "-" + StartTime[4].ToString() + "-" + StartTime[5].ToString() + "-" + packetsTime[6].ToString();
            sw_Lubo[0] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + ".DAT"), false);
            sw_Lubo[1] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + ".HDR"), false);
            sw_Lubo[2] = new StreamWriter(Path.GetFullPath("Lubo302\\" + sDataTime.ToString() + ".CFG"), false);
            //GetCFGFile(StartTime, sr_Lubo, sbReader);//
            GetCFGFile(StartTime, sbReader);//
            sw_Lubo[1].Write("");//sbReader.ToString());
            #region 处理FCK304机箱录波报文

            //sw_Lubo[1] = new StreamWriter(Path.GetFullPath("Lubo\\" + sDataTime + ".CFG"), false);
            //GetCFGFile(PacketsTime);
            //sw_Lubo[2] = new StreamWriter(Path.GetFullPath("Lubo304\\" + sDataTime + ".HDR"), false);                    
            //string strreader = sr_Lubo.ReadToEnd().ToString();
            sw_Lubo[2].Write(sbReader);
            sw_Lubo[2].Close();
            sw_Lubo[2] = null;
            for (int i = 0; i < index; i++)
            {//存储报文中25个时间点的数据
               // int tindex = j * 40 + 7;
                str.Append(i + 1);
                str.Append(",");
                str.Append(i * 30);
                str.Append(",");
               // if (RecData[i] != null)
                {
                    for (int k = 0; k < 264; k++)//264个子模块电压值
                    {
                        str.Append((RecData[i][3 + 2 * k] << 8) + RecData[i][2 + 2 * k]);
                        str.Append(",");
                    }
                    for (int k = 0; k < 132; k++)//264个子模块指令值
                    {
                        str.Append(RecData[i][531 + k] & 0x0F);
                        str.Append(",");
                        str.Append((RecData[i][531 + k] >> 4) & 0x0F);
                        str.Append(",");
                    }
                    for (int k = 0; k < 8; k++)//其他
                    {
                        str.Append((RecData[i][664 + 2 * k] << 8) + RecData[i][663 + 2 * k]);
                        str.Append(",");
                    }
                    RecData[i] = null;
                }               
                str.Append("\r\n");
                if (sw_Lubo[0] != null)
                {//数据写入DAT文件
                    sw_Lubo[0].Write(str);
                    sw_Lubo[0].Flush();
                    str.Clear();
                }
            }
            //存储完成，关闭文件
            if (sw_Lubo[0] != null)
            {
                sw_Lubo[0].Close();
                //sw_Lubo[0].Dispose();
                sw_Lubo[0] = null;
            }
            MessageBox.Show("数据处理完毕", "提示", MessageBoxButtons.OK);
            #endregion
        }

        private bool GetCFGFile(int[] packetstime, StringBuilder sbreader)
        {
            //string str;
            //while ((str = sr.ReadLine()) != null)
            //{
            //    sbreader.Append(str);
            //    sbreader.Append("\r\n");//_separator 设为空格   
            //}
            sbreader.Append("SS_S1P1PCPA2,COP\r\n536,536A,0D\r\n");
            for (int i = 0; i < 264;i++ )
                sbreader.Append((i+1).ToString()+",V"+(i+1).ToString()+",,,V,1,0,0,-32768,32767\r\n");
            for(int i=0;i<264;i++)
                sbreader.Append((i + 1).ToString() + ",C" + (i + 1).ToString() + ",,,V,1,0,0,-32768,32767\r\n");
            sbreader.Append("529,I_Dir,,,V,1,0,0,0,65535\r\n");
            sbreader.Append("530,Run_Mode,,,V,1,0,0,0,65535\r\n");
            sbreader.Append("531,C_Cmd,,,V,1,0,0,0,65535\r\n");
            sbreader.Append("532,Fault_Num,,,V,1,0,0,0,65535\r\n");
            sbreader.Append("533,Touru_Num,,,V,1,0,0,0,65535\r\n");
            sbreader.Append("534,Trip,,,V,1,0,0,0,65535\r\n");
            sbreader.Append("535,Alarm,,,V,1,0,0,0,65535\r\n");
            sbreader.Append("536,Self-Checking,,,V,1,0,0,0,65535\r\n");
            sbreader.Append("50\r\n1\r\n30,");
            sbreader.Append((2*RedNum+2).ToString()+"\r\n");
            string strtime = StartTime[2].ToString() + "/" + StartTime[1].ToString() + "/" + StartTime[0].ToString() + "," + StartTime[3].ToString() + ":" + StartTime[4].ToString() + ":";// +".";
            sbreader.Append(strtime + StartTime[5].ToString());
            sbreader.Append(".");
            sbreader.Append(StartTime[6] * 1000);
            sbreader.Append("\r\n");
            sbreader.Append(strtime + (packetstime[5]).ToString());
            sbreader.Append(".");
            sbreader.Append((packetstime[6] +30)* 1000);
            sbreader.Append("\r\n");
            sbreader.Append("ASCII");
            sbreader.Append("\r\n");
            //sw.Write(sbreader);
            return true;
        }
        //录波查询
        private void button1_Click(object sender, EventArgs e)
        {
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
                DeviceAddr = (byte)comboBox1.SelectedIndex;//System.Convert.ToByte(textBox1.Text);

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
            richTextBox1.Text = "";
            LuboTime = "";
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
        //测试BCD码转换，实际程序不用
        private void button6_Click(object sender, EventArgs e)
        {
            textBox2.Text = string.Format("{0:x}",GetBCD(System.Convert.ToInt32(textBox1.Text)));
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
                SendData[4] = (byte)(SendData[4]+0x0F);
            if (checkBox2.Checked == true)
                SendData[4] = (byte)(SendData[4] + 0xF0);
            SendData[5] = 0x00;
            if (checkBox3.Checked == true)
                SendData[5] = (byte)(SendData[5] + 0x0F);
            if (checkBox4.Checked == true)
                SendData[5] = (byte)(SendData[5] + 0xF0);
            int sum = SendData[0] + SendData[1] + SendData[2] + SendData[3] + SendData[4] + SendData[5];
            SendData[6] = (byte)(sum & 0xFF);
            SendData[7] = (byte)((sum >> 8) & 0xFF);
            SendFlg = true;
        }
    }
}
