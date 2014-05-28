/*******IEC104 服务器程序  TCPServer.c ************/
#include <stdlib.h>
#include <stdio.h>
#include <errno.h>
#include <string.h>
#include <netdb.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <netinet/tcp.h>
#include <fcntl.h>
#include <unistd.h>
#include <math.h>
#include <time.h>
#include <signal.h>

#include "iec104.h"

#define WAITBUF 10

float TestValFloat=0;
int YT_int16_test=0;
unsigned int YT_unsignedint16_test=0;
unsigned char Index2=0;
unsigned char IEC104_YT_Valtest[4];
unsigned char IEC104_START01_CMD=0;
unsigned char IEC104_START01_READY=0;
unsigned short IEC104SendNum=0;
unsigned short IEC104RecvNum=0;
//IEC104_CommStart置位后才能启动数据传输
unsigned char IEC104_CommStart=0;
unsigned char ReseieData[256];
unsigned char SendData[256]={0x11,0x22,0x33,0x44,0x55};
systime_t sys_time;

void Delay(unsigned int LoopCounter)
{
   unsigned int i,j;
	for(i=0;i<LoopCounter;i++)
	{
	   j++;
	}
}

void set_systime(systime_t *time)
{
	struct tm my_tm;
	time_t t1;
//	char buf[128] = {0};
/**********************************************************
    sprintf(buf, "%04d-%02d-%02d  %02d:%02d:%02d",time->year,
    		time->month, time->day, time->hour, time->minute, time->millionsecond);
    printf("%s\n", buf);              //2012-12-12  12:12:12
***********************************************************/
	my_tm.tm_year = time->year + 2000 - 1900;
	my_tm.tm_mon  = time->month - 1;
	my_tm.tm_mday = time->day;
	my_tm.tm_hour = time->hour;
	my_tm.tm_min  = time->minute;
	my_tm.tm_sec  = time->millionsecond/1000;

/**********************************************************
    sprintf(buf, "%04d-%02d-%02d  %02d:%02d:%02d",my_tm.tm_year + 1900,
    		my_tm.tm_mon + 1, my_tm.tm_mday, my_tm.tm_hour, my_tm.tm_min, my_tm.tm_sec);
    printf("%s\n", buf);              //2012-12-12  12:12:12
*************************************************************/

	t1 = mktime(&my_tm);

	stime(&t1);
}
void get_systime(systime_t *mytime)
{
	struct tm *my_tm;
	time_t t1;

	t1 = time(&t1);
	my_tm = localtime(&t1);

	mytime->year  		= my_tm->tm_year + 1900 - 2000;
	mytime->month 		= my_tm->tm_mon  + 1;
	mytime->day   		= my_tm->tm_mday;
	mytime->hour  		= my_tm->tm_hour;
	mytime->minute		= my_tm->tm_min;
	mytime->millionsecond = my_tm->tm_sec * 1000;

}
int send_104frame(int sockfd, const void *msg, size_t len, int flags)
{
	if(send(sockfd,msg,len,flags)==-1){
		fprintf(stderr,"Write Error:%s\n",strerror(errno));
		close(sockfd);
		exit(1);
	}
	return 0;
}
void sconfirm_send(int sockfd, int i)
{
	SendData[0]=0x68;
	SendData[1]=0x04;
	SendData[2]=0x01;
	SendData[3]=0;
	IEC104RecvNum=ReseieData[i+2]+ReseieData[i+3]*256;
	IEC104RecvNum=IEC104RecvNum+2;
	SendData[4]=IEC104RecvNum&0xFF;
	SendData[5]=(IEC104RecvNum>>8)&0xFF;
	//S确认帧
	send_104frame(sockfd, SendData, 6, 0);
	Delay(0xFF);
}
void timeconfirm_send(int sockfd, int i)
{
	systime_t sys_time;

	get_systime(&sys_time);

	SendData[0]=0x68;
	SendData[1]=0x14;

	SendData[2]=IEC104SendNum&0xFF;
	SendData[3]=(IEC104SendNum>>8)&0xFF;
	SendData[4]=IEC104RecvNum&0xFF;
	SendData[5]=(IEC104RecvNum>>8)&0xFF;
	IEC104SendNum=IEC104SendNum+2;

	SendData[6]=0x67;
	SendData[7]=0x01;
	SendData[8]=0x07;
	SendData[9]=ReseieData[i+9];

	SendData[10]=ReseieData[i+10];
	SendData[11]=ReseieData[i+11];

	SendData[12]=ReseieData[i+12];
	SendData[13]=ReseieData[i+13];
	SendData[14]=ReseieData[i+14];

	SendData[15]=ReseieData[i+15];
	SendData[16]=ReseieData[i+16];
	SendData[17]=ReseieData[i+17];
	SendData[18]=ReseieData[i+18];
	SendData[19]=ReseieData[i+19];
	SendData[20]=ReseieData[i+20];
	SendData[21]=ReseieData[i+21];

	//发送对时确认
	send_104frame(sockfd, SendData, 22, 0);
}
void elecup_104frame(int sockfd)
{
	//上传电度值
	SendData[0]=0x68;
	SendData[1]=0x1A;

	SendData[2]=IEC104SendNum&0xFF;
	SendData[3]=(IEC104SendNum>>8)&0xFF;
	SendData[4]=IEC104RecvNum&0xFF;
	SendData[5]=(IEC104RecvNum>>8)&0xFF;
	IEC104SendNum=IEC104SendNum+2;
	//电度累积量
	SendData[6]= 0x0F;
	//2个电度值
	SendData[7]= 0x02;

	//突发事件 3 或请求、被请求 5 或响应电量总召唤 37
	SendData[8]= 0x05;
	SendData[9]= 0x00;
	//公共地址
	SendData[10]= 0x01;
	SendData[11]= 0x01;

	//0号电度，信息体地址
	SendData[12]= 0x01;
	SendData[13]= 0x64;//02基地址
	//SendData[13]= 0x0C;//97基地址
	SendData[14]= 0x00;

	//电度值
	SendData[15]= 0x11;
	SendData[16]= 0x00;
	SendData[17]= 0x00;
	SendData[18]= 0x00;
	//描述信息
	SendData[19]= 0x00;

	//1号电度，信息体地址
	SendData[20]= 0x02;
	SendData[21]= 0x64;//02基地址
	//SendData[21]= 0x0C;//97基地址
	SendData[22]= 0x00;
	//电度值
	SendData[23]= 0x22;
	SendData[24]= 0x00;
	SendData[25]= 0x00;
	SendData[26]= 0x00;
	//描述信息
	SendData[27]= 0x01;


	send_104frame(sockfd, SendData, 28, 0);
}

void dataup_104frame(int sockfd)
{
	int i;
	static int ndx=0;
	ndx++;
	//上传变化的遥信 数据
	SendData[0]=0x68;
	SendData[1]=0x17;

	SendData[2]=IEC104SendNum&0xFF;
	SendData[3]=(IEC104SendNum>>8)&0xFF;
	SendData[4]=IEC104RecvNum&0xFF;
	SendData[5]=(IEC104RecvNum>>8)&0xFF;
	IEC104SendNum=IEC104SendNum+2;
	//单点遥信
	SendData[6]= 0x01;
	//10个连续变位
	SendData[7]= 0x8A;

	//突发事件 3 或相应总召唤 20 或定时主动上传 1
	SendData[8]= 0x14;
	SendData[9]= 0x00;
	//公共地址
	SendData[10]= 0x01;
	SendData[11]= 0x01;

	//17号遥信，信息体地址
	SendData[12]= 0x01;
	SendData[13]= 0x00;
	SendData[14]= 0x00;
	//0:分位    1:合位
	if(ndx%2){
		for(i=0; i<10; i++){
			SendData[i+15]= 0x01;
		}
	}else{
		for(i=0; i<10; i++){
			SendData[i+15]= 0x00;
		}
	}
	send_104frame(sockfd, SendData, 25, 0);


	//上传变化的遥测数据
	SendData[0]=0x68;
	SendData[1]=0x16;

	SendData[2]=IEC104SendNum&0xFF;
	SendData[3]=(IEC104SendNum>>8)&0xFF;
	SendData[4]=IEC104RecvNum&0xFF;
	SendData[5]=(IEC104RecvNum>>8)&0xFF;
	IEC104SendNum=IEC104SendNum+2;
	//带品质描述的归一化测量值
	SendData[6]= 0x09;
	//2个变位
	SendData[7]= 0x02;

	//突发事件 3 或相应总召唤 20 或定时主动上传 1
	SendData[8]= 0x14;
	SendData[9]= 0x00;

	SendData[10]= 0x01;
	SendData[11]= 0x01;

	//1号遥测
	SendData[12]= 0x01;
	SendData[13]= 0x40;//02基地址
	//SendData[13]= 0x07;//97基地址
	SendData[14]= 0x00;
	//带品质描述的归一化报文
	//遥测值
	SendData[15]= 0xEE;
	SendData[16]= 0x10;
	SendData[17]= 0x00;

	//2号遥测
	SendData[18]= 0x02;
	SendData[19]= 0x40;//02基地址
	//SendData[18]= 0x07;//97基地址
	SendData[20]= 0x00;
	//不带品质描述的归一化报文
	//遥测值
	SendData[21]= 0xEE;
	SendData[22]= 0x10;
	SendData[23]= 0x00;

	send_104frame(sockfd, SendData, 25, 0);

}

int read_104frame(char *datarec, char *dataget, size_t len)
{
	int ret = -1;
	if((len >= 6) && (len < 256)){

	}

	return ret;
}
void process_104frame(int sockfd, char *data, size_t len)
{

	int i,j;

	for(j = 0; j < 256; j++){
		ReseieData[j]=data[j];
	}
	for(i=0;i<20;i++){
		if(ReseieData[i]==0x68){
			if(ReseieData[i+1]==0x04){
					if(ReseieData[i+2]==TESTFR_ACT)
					{
						//TESTFR
						SendData[0]=0x68;
						SendData[1]=0x04;
						SendData[2]=TESTFR_CONFIRM;
						SendData[3]=0;
						SendData[4]=0;
						SendData[5]=0;

						send_104frame(sockfd, SendData, 6, 0);

						SendData[0]=0x68;
						SendData[1]=0x04;
						SendData[2]=TESTFR_ACT;
						SendData[3]=0;
						SendData[4]=0;
						SendData[5]=0;

						send_104frame(sockfd, SendData, 6, 0);
						break;
					}
/************************************************************
					if(ReseieData[i+2]==0x83)
					{

						SendData[0]=0x68;
						SendData[1]=0x04;
						SendData[2]=0x43;
						SendData[3]=0;
						SendData[4]=0;
						SendData[5]=0;

						tcp_write(pcb,SendData , 6, 1);
						tcp_output(pcb);
						break;
					}
********************************************************************/
					if(ReseieData[i+2]==STOPDT_ACT)
					{
						//停止数据传输STOPDT
						SendData[0]=0x68;
						SendData[1]=0x04;
						SendData[2]=STOPDT_CONFIRM;
						SendData[3]=0;
						SendData[4]=0;
						SendData[5]=0;

						IEC104_CommStart=0;

						send_104frame(sockfd, SendData, 6, 0);
						break;
					}
					if(ReseieData[i+2]== STARTDT_ACT)
					{
						//开始数据传输STARTDT
						SendData[0]=0x68;
						SendData[1]=0x04;
						SendData[2]=STARTDT_CONFIRM;
						SendData[3]=0;
						SendData[4]=0;
						SendData[5]=0;

						IEC104_CommStart=1;

						send_104frame(sockfd, SendData, 6, 0);

						Delay(0xFF);

						SendData[0]=0x68;
	   					SendData[1]=0x15;
//						SendCommand[1]=0x7F;
						SendData[2]=IEC104SendNum&0xFF;
	   					SendData[3]=(IEC104SendNum>>8)&0xFF;
	   					SendData[4]=IEC104RecvNum&0xFF;
	   					SendData[5]=(IEC104RecvNum>>8)&0xFF;
						IEC104SendNum=IEC104SendNum+2;

						SendData[6]=0x1E;
						SendData[7]=0x01;
						SendData[8]=0x03;
						SendData[9]=0;
						///////////////////////
						SendData[10]=0x00;//sect
						SendData[11]=0x01;//addr
						/////////////////////
						SendData[12]=0x01;
						SendData[13]=0;
						SendData[14]=0;

						SendData[15]=0;
						//系统时间
						SendData[16]=0x1E;
						SendData[17]=0x3B;//毫秒
						SendData[18]=0;//分
						SendData[19]=0x00;//时
						SendData[20]=0x01;//日星期
						SendData[21]=0x01;//月
						SendData[22]=0x00;//年

						send_104frame(sockfd, SendData, 23, 0);

						break;
					}
/*					if(ReseieData[i+2]==0x01)
					{
//						//如果发送数据保存在一个缓冲区
//						//可以根据读取到的S报文删除已经
//						// 正确传输的报文
						SendData[0]=0;
						SendData[1]=0;
						SendData[2]=0;
						SendData[3]=0;
						SendData[4]=0;
						SendData[5]=0;

						tcp_write(pcb,SendData , 6, 1);
						tcp_output(pcb);
						break;
					} */
				}
				if(ReseieData[i+1]>=0x0E)
				{
					if(ReseieData[i+6]==0x64)
					{
						//总召唤
				   		if((ReseieData[i+7]==0x01)&&(ReseieData[i+8]==0x06))
				   		{
				   			//传输原因：激活
//				   		  	IEC104SendNum;
//							IEC104RecvNum;
							SendData[0]=0x68;
							SendData[1]=0x04;
							SendData[2]=0x01;
							SendData[3]=0;
							IEC104RecvNum=ReseieData[i+2]+ReseieData[i+3]*256;
							IEC104RecvNum=IEC104RecvNum+2;
							SendData[4]=IEC104RecvNum&0xFF;
							SendData[5]=(IEC104RecvNum>>8)&0xFF;
							//S 确认帧
							send_104frame(sockfd, SendData, 6, 0);

							Delay(0xFF);

							SendData[0]=0x68;
							SendData[1]=0x0e;

							SendData[2]=IEC104SendNum&0xFF;
	   						SendData[3]=(IEC104SendNum>>8)&0xFF;
	   						SendData[4]=IEC104RecvNum&0xFF;
	   						SendData[5]=(IEC104RecvNum>>8)&0xFF;
							IEC104SendNum=IEC104SendNum+2;

							SendData[6]=0x64;
							SendData[7]=0x01;
							SendData[8]=0x07;
							SendData[9]=ReseieData[i+9];
							SendData[10]=ReseieData[i+10];
							SendData[11]=ReseieData[i+11];
							SendData[12]=ReseieData[i+12];
							SendData[13]=ReseieData[i+13];
							SendData[14]=ReseieData[i+14];
							SendData[15]=ReseieData[i+15];
							//回答总召唤确认命令
							send_104frame(sockfd, SendData, 16, 0);
							Delay(0xFF);

							//////////////////////////////////////
							//
							//上传全遥信量、遥测量
							//
							/////////////////////////////////////
							dataup_104frame(sockfd);
							Delay(0xFF);


							SendData[0]=0x68;
							SendData[1]=0x0e;

							SendData[2]=IEC104SendNum&0xFF;
	   						SendData[3]=(IEC104SendNum>>8)&0xFF;
	   						SendData[4]=IEC104RecvNum&0xFF;
	   						SendData[5]=(IEC104RecvNum>>8)&0xFF;
							IEC104SendNum=IEC104SendNum+2;

							SendData[6]=0x64;
							SendData[7]=0x01;
							SendData[8]=0x0a;
							SendData[9]=ReseieData[i+9];
							SendData[10]=ReseieData[i+10];
							SendData[11]=ReseieData[i+11];
							SendData[12]=ReseieData[i+12];
							SendData[13]=ReseieData[i+13];
							SendData[14]=ReseieData[i+14];
							SendData[15]=ReseieData[i+15];
							//回答总召唤结束命令
							send_104frame(sockfd, SendData, 16, 0);
//							Delay(0xFF);
							break;
				   		}
					}
					if(ReseieData[i+6]==0x65)
					{
						//电能脉冲召唤命令
				   		if((ReseieData[i+7]==0x01)&&(ReseieData[i+8]==0x06))
				   		{
						 	SendData[0]=0x68;
							SendData[1]=0x04;
							SendData[2]=0x01;
							SendData[3]=0;
							IEC104RecvNum=ReseieData[i+2]+ReseieData[i+3]*256;
							IEC104RecvNum=IEC104RecvNum+2;
							SendData[4]=IEC104RecvNum&0xFF;
							SendData[5]=(IEC104RecvNum>>8)&0xFF;
							//S确认帧
							send_104frame(sockfd, SendData, 6, 0);

							SendData[0]=0x68;
							SendData[1]=0x0e;

							SendData[2]=IEC104SendNum&0xFF;
	   						SendData[3]=(IEC104SendNum>>8)&0xFF;
	   						SendData[4]=IEC104RecvNum&0xFF;
	   						SendData[5]=(IEC104RecvNum>>8)&0xFF;
							IEC104SendNum=IEC104SendNum+2;

							SendData[6]=0x65;
							SendData[7]=0x01;
							SendData[8]=0x07;
							SendData[9]=ReseieData[i+9];
							SendData[10]=ReseieData[i+10];
							SendData[11]=ReseieData[i+11];
							SendData[12]=ReseieData[i+12];
							SendData[13]=ReseieData[i+13];
							SendData[14]=ReseieData[i+14];
							SendData[15]=ReseieData[i+15];
							//回答电度召唤确认命令
							send_104frame(sockfd, SendData, 16, 0);
							Delay(0xFF);

							//////////////////////////////////////////////
							//
							//上传电度
							//
							////////////////////////////////////////////
							elecup_104frame(sockfd);
							Delay(0xFF);



							SendData[0]=0x68;
							SendData[1]=0x0e;

							SendData[2]=IEC104SendNum&0xFF;
	   						SendData[3]=(IEC104SendNum>>8)&0xFF;
	   						SendData[4]=IEC104RecvNum&0xFF;
	   						SendData[5]=(IEC104RecvNum>>8)&0xFF;
							IEC104SendNum=IEC104SendNum+2;

							SendData[6]=0x65;
							SendData[7]=0x01;
							SendData[8]=0x0A;
							SendData[9]=ReseieData[i+9];
							SendData[10]=ReseieData[i+10];
							SendData[11]=ReseieData[i+11];
							SendData[12]=ReseieData[i+12];
							SendData[13]=ReseieData[i+13];
							SendData[14]=ReseieData[i+14];
							SendData[15]=ReseieData[i+15];
							//回答电度召唤结束命令
							send_104frame(sockfd, SendData, 16, 0);
							break;
						}
					}
					if(ReseieData[i+6]==0x2E)
					{
						//遥控双命令  ,46
						//
				   		if((ReseieData[i+7]==0x01)&&(ReseieData[i+8]==0x06))
				   		{
						 	SendData[0]=0x68;
							SendData[1]=0x04;
							SendData[2]=0x01;
							SendData[3]=0;
							IEC104RecvNum=ReseieData[i+2]+ReseieData[i+3]*256;
							IEC104RecvNum=IEC104RecvNum+2;
							SendData[4]=IEC104RecvNum&0xFF;
							SendData[5]=(IEC104RecvNum>>8)&0xFF;
							//S确认帧
							send_104frame(sockfd, SendData, 6, 0);

							Delay(0xFF);

							if((ReseieData[i+12]==0x01)&&(ReseieData[i+13]==0x60)&&(ReseieData[i+14]==0x00))
							{
								//遥控返校
								if(ReseieData[i+15]==0x82)
								{
									IEC104_START01_READY=1;
									printf("006001 YC back\n\r");
								}
								//执行确认
								if(ReseieData[i+15]==0x02)
								{
								 	IEC104_START01_CMD=1;
								 	printf("006001 YC confirm\n\r");
								}

							}
//							if((ReseieData[i+12]==0x01)&&(ReseieData[i+13]==0x30)&&(ReseieData[i+14]==0x00))
//							{
//								if(ReseieData[i+15]==0x02)
//								{
//								 	IEC104_START_CMD=0;
//								}
//
//							}

							SendData[0]=0x68;
							SendData[1]=0x0e;

							SendData[2]=IEC104SendNum&0xFF;
	   						SendData[3]=(IEC104SendNum>>8)&0xFF;
	   						SendData[4]=IEC104RecvNum&0xFF;
	   						SendData[5]=(IEC104RecvNum>>8)&0xFF;
							IEC104SendNum=IEC104SendNum+2;

							SendData[6]=0x2e;
							SendData[7]=0x01;
							SendData[8]=0x07;
							SendData[9]=ReseieData[i+9];
							SendData[10]=ReseieData[i+10];
							SendData[11]=ReseieData[i+11];
							SendData[12]=ReseieData[i+12];
							SendData[13]=ReseieData[i+13];
							SendData[14]=ReseieData[i+14];
							SendData[15]=ReseieData[i+15];
							//执行确认
							send_104frame(sockfd, SendData, 16, 0);

//							Delay(0xFF);
//
//							SendData[0]=0x68;
//							SendData[1]=0x0e;
//
//							SendData[2]=IEC104SendNum&0xFF;
//	   						SendData[3]=(IEC104SendNum>>8)&0xFF;
//	   						SendData[4]=IEC104RecvNum&0xFF;
//	   						SendData[5]=(IEC104RecvNum>>8)&0xFF;
//							IEC104SendNum=IEC104SendNum+2;
//
//							SendData[6]=0x2e;
//							SendData[7]=0x01;
//							SendData[8]=0x0a;
//							SendData[9]=ReseieData[i+9];
//							SendData[10]=ReseieData[i+10];
//							SendData[11]=ReseieData[i+11];
//							SendData[12]=ReseieData[i+12];
//							SendData[13]=ReseieData[i+13];
//							SendData[14]=ReseieData[i+14];
//							SendData[15]=ReseieData[i+15];
//							//激活结束
//							send_104frame(sockfd, SendData, 16, 0);
							break;
						}
				   		///////////////////////////////
				   		//遥控撤消
				   		////////////////////////////////
				   		if((ReseieData[i+7]==0x01)&&(ReseieData[i+8]==0x08)){

							SendData[0]=0x68;
							SendData[1]=0x0e;

							SendData[2]=IEC104SendNum&0xFF;
	   						SendData[3]=(IEC104SendNum>>8)&0xFF;
	   						SendData[4]=IEC104RecvNum&0xFF;
	   						SendData[5]=(IEC104RecvNum>>8)&0xFF;
							IEC104SendNum=IEC104SendNum+2;

							SendData[6]=0x2e;
							SendData[7]=0x01;
							SendData[8]=0x09;
							SendData[9]=ReseieData[i+9];
							SendData[10]=ReseieData[i+10];
							SendData[11]=ReseieData[i+11];
							SendData[12]=ReseieData[i+12];
							SendData[13]=ReseieData[i+13];
							SendData[14]=ReseieData[i+14];
							SendData[15]=ReseieData[i+15];
							printf("006001 YC cancel\n\r");
							//撤销确认
							send_104frame(sockfd, SendData, 16, 0);
							break;
				   		}
					}
					if(ReseieData[i+6]==0x32)
					{
						//设点命令，短浮点数  ,50
				   		if((ReseieData[i+7]==0x01)&&(ReseieData[i+8]==0x06))
				   		{
						 	SendData[0]=0x68;
							SendData[1]=0x04;
							SendData[2]=0x01;
							SendData[3]=0;
							IEC104RecvNum=ReseieData[i+2]+ReseieData[i+3]*256;
							IEC104RecvNum=IEC104RecvNum+2;
							SendData[4]=IEC104RecvNum&0xFF;
							SendData[5]=(IEC104RecvNum>>8)&0xFF;
							//S确认帧
							send_104frame(sockfd, SendData, 6, 0);

							Delay(0xFF);

							if((ReseieData[i+12]==0x01)&&(ReseieData[i+13]==0x62)&&(ReseieData[i+14]==0x00))
							{

								printf("006201 YT confirm\n\r");
								IEC104_YT_Valtest[0]= ReseieData[i+15];
								IEC104_YT_Valtest[1]= ReseieData[i+16];
								IEC104_YT_Valtest[2]= ReseieData[i+17];
								IEC104_YT_Valtest[3]= ReseieData[i+18];
//								TestValFloat=(float)((IEC104_YT_Valtest[3]<<24)| (IEC104_YT_Valtest[2]<<16)|(IEC104_YT_Valtest[1]<<8)| IEC104_YT_Valtest[0])	;
//								YT_int16_test=(unsigned int) TestValFloat;
//								YT_int16_test=(unsigned int)(floor(TestValFloat));
								Index2=(ReseieData[i+18]&0x7F)*2+((ReseieData[i+17]&0x80)>>7);
								YT_unsignedint16_test=(IEC104_YT_Valtest[2]<<16)|(IEC104_YT_Valtest[1]<<8)| IEC104_YT_Valtest[0];
								YT_unsignedint16_test=0x800000|YT_unsignedint16_test;
								Index2=Index2-127;
								TestValFloat=0;
								for(j=0;j<(23-Index2);j++)
								{
				//					if(((YT_unsignedint16_test<<(1+j))&0x800000)==0x800000)
									if((YT_unsignedint16_test & (0x400000>>(Index2+j)))!=0x000000)
									{
										TestValFloat=TestValFloat+(float)(1.0/((float)(pow(2, j+1))));
									}
								}
								YT_unsignedint16_test=(YT_unsignedint16_test>>(23-Index2));
								if((IEC104_YT_Valtest[3]&0x80)==0x80)
							   		YT_int16_test=-YT_unsignedint16_test;
								else if((IEC104_YT_Valtest[3]&0x80)==0x00)
									YT_int16_test=YT_unsignedint16_test;

							}


							SendData[0]=0x68;
							SendData[1]=0x12;

							SendData[2]=IEC104SendNum&0xFF;
	   						SendData[3]=(IEC104SendNum>>8)&0xFF;
	   						SendData[4]=IEC104RecvNum&0xFF;
	   						SendData[5]=(IEC104RecvNum>>8)&0xFF;
							IEC104SendNum=IEC104SendNum+2;

							SendData[6]=0x32;
							SendData[7]=0x01;
							SendData[8]=0x07;
							SendData[9]=ReseieData[i+9];
							SendData[10]=ReseieData[i+10];
							SendData[11]=ReseieData[i+11];
							SendData[12]=ReseieData[i+12];
							SendData[13]=ReseieData[i+13];
							SendData[14]=ReseieData[i+14];
							SendData[15]=ReseieData[i+15];
							SendData[16]=ReseieData[i+16];
							SendData[17]=ReseieData[i+17];
							SendData[18]=ReseieData[i+18];
							SendData[19]=0;
							//激活确认
							send_104frame(sockfd, SendData, 20, 0);

//							Delay(0xFF);
//
//							SendData[0]=0x68;
//							SendData[1]=0x0e;
//
//							SendData[2]=IEC104SendNum&0xFF;
//	   						SendData[3]=(IEC104SendNum>>8)&0xFF;
//	   						SendData[4]=IEC104RecvNum&0xFF;
//	   						SendData[5]=(IEC104RecvNum>>8)&0xFF;
//							IEC104SendNum=IEC104SendNum+2;
//
//							SendData[6]=0x32;
//							SendData[7]=0x01;
//							SendData[8]=0x0a;
//							SendData[9]=ReseieData[i+9];
//							SendData[10]=ReseieData[i+10];
//							SendData[11]=ReseieData[i+11];
//							SendData[12]=ReseieData[i+12];
//							SendData[13]=ReseieData[i+13];
//							SendData[14]=ReseieData[i+14];
//							SendData[15]=ReseieData[i+15];
//							SendData[16]=ReseieData[i+16];
//							SendData[17]=ReseieData[i+17];
//							SendData[18]=ReseieData[i+18];
//							SendData[19]=0;
//							//激活结束
//							send_104frame(sockfd, SendData, 20, 0);
							break;
						}
					}
					if(ReseieData[i+6]==0x67)
					{
						//对时报文
				   		if((ReseieData[i+7]==0x01)&&(ReseieData[i+8]==0x06))
				   		{

				   			sconfirm_send(sockfd, i);
							//获取时间
				   			sys_time.millionsecond = ReseieData[i+IEC104_OFFSET_CONTEXT]
				   			          + ReseieData[i+IEC104_OFFSET_CONTEXT+1]*0x100;
				   			sys_time.minute = ReseieData[i+IEC104_OFFSET_CONTEXT+2] & 0x3F;
				   			sys_time.hour	= ReseieData[i+IEC104_OFFSET_CONTEXT+3] & 0x1f;
				   			sys_time.day	= ReseieData[i+IEC104_OFFSET_CONTEXT+4] & 0x1f;
				   			sys_time.month	= ReseieData[i+IEC104_OFFSET_CONTEXT+5] & 0x0f;
				   			sys_time.year	= ReseieData[i+IEC104_OFFSET_CONTEXT+6] & 0x7f;
				   			//设置时间
				   			set_systime(&sys_time);
				   			//发送对时确认
					   		timeconfirm_send(sockfd, i);
					   		break;
				   		}

					}
				}
		}
	}
}
/* Setting SO_TCP KEEPALIVE */
//int keep_alive = 1;//设定KeepAlive
//int keep_idle = 1;//开始首次KeepAlive探测前的TCP空闭时间
//如该连接在1秒内没有任何数据往来,则进行探测
//int keep_interval = 1;//两次KeepAlive探测间的时间间隔
// 探测时发包的时间间隔为1 秒
//int keep_count = 3;//判定断开前的KeepAlive探测次数
// 探测尝试的次数.如果第1次探测包就收到响应了,则后2次的不再发
void set_keepalive(int fd, int keep_alive, int keep_idle, int keep_interval, int keep_count)
{
	int opt = 1;
	if(keep_alive)
	{
		/*
		if(setsockopt(fd, SOL_SOCKET, SO_OOBINLINE,
                                        (void*)&opt, sizeof(opt)) == -1)
                {
                        fprintf(stderr,
                                "setsockopt SOL_SOCKET::SO_OOBINLINE failed, %s\n",strerror(errno));
                }
		*/
		if(setsockopt(fd, SOL_SOCKET, SO_KEEPALIVE,
					(void*)&keep_alive, sizeof(keep_alive)) == -1)
		{
			fprintf(stderr,
				"setsockopt SOL_SOCKET::SO_KEEPALIVE failed, %s\n",strerror(errno));
		}
		if(setsockopt(fd, SOL_TCP, TCP_KEEPIDLE,
					(void *)&keep_idle,sizeof(keep_idle)) == -1)
		{
			fprintf(stderr,
			 	"setsockopt SOL_TCP::TCP_KEEPIDLE failed, %s\n", strerror(errno));
		}
		if(setsockopt(fd,SOL_TCP,TCP_KEEPINTVL,
					(void *)&keep_interval, sizeof(keep_interval)) == -1)
		{
			fprintf(stderr,
				 "setsockopt SOL_tcp::TCP_KEEPINTVL failed, %s\n", strerror(errno));
		}
		if(setsockopt(fd,SOL_TCP,TCP_KEEPCNT,
					(void *)&keep_count,sizeof(keep_count)) == -1)
		{
			fprintf(stderr,
				"setsockopt SOL_TCP::TCP_KEEPCNT failed, %s\n", strerror(errno));
		}
	}
}
/* Net check Make sure you have not used OUT OF BAND DATA AND YOU CAN use OOB */
int netcheck(int fd)
{
	int buf_size = 1024;
	char buf[buf_size];
	int n = 0;
	//clear OOB DATA
	n = recv(fd, buf, buf_size, MSG_OOB);
	/*
	if(n > 0 )
        {
                fprintf(stdout, "Connection[%d] read OOB data length:%d\n", fd, n);
        }
	*/
	if(send(fd, (void *)"0", 1, MSG_OOB) < 0 )
	{
		fprintf(stderr, "Connection[%d] send OOB failed, %s\n", fd, strerror(errno));
                return -1;
	}
	return 0;
}
int main(int argc, char *argv[])
{
	int sockfd,new_fd;
	struct sockaddr_in server_addr;
	struct sockaddr_in client_addr;
	int sin_size,portnumber=2404;
	char buf[256];
	int recbytes;

 	 //服务器端开始建立socket描述符
 	 if((sockfd=socket(AF_INET,SOCK_STREAM,0))==-1){
 		 fprintf(stderr,"Socket error:%s\n\a",strerror(errno));
		// perror("call to socket");
 		 exit(1);
 	 }

 	 //服务器端填充 sockaddr结构
 	 bzero(&server_addr,sizeof(struct sockaddr_in));
 	 server_addr.sin_family=AF_INET;
 	 //自动填充主机IP
 	 server_addr.sin_addr.s_addr=htonl(INADDR_ANY);
 	 server_addr.sin_port=htons(portnumber);

 	 //捆绑sockfd描述符
 	 if(bind(sockfd,(struct sockaddr *)(&server_addr),sizeof(struct sockaddr))==-1){
        fprintf(stderr,"Bind error:%s\n\a",strerror(errno));
		// perror("call to socket");
        exit(1);
 	 }

 	 //监听sockfd描述符
	if(listen(sockfd, WAITBUF)==-1){
        fprintf(stderr,"Listen error:%s\n\a",strerror(errno));
        exit(1);
	}
 	fprintf(stdout,"Accepting connections ...\n");
	while(1){
        //服务器阻塞，直到客户程序建立连接
        sin_size=sizeof(struct sockaddr_in);
        if ((new_fd=accept(sockfd,(struct sockaddr *)(&client_addr),(socklen_t *)&sin_size))==-1)
        {
                fprintf(stderr,"Accept error:%s\n\a",strerror(errno));
                exit(1);
		}

        set_keepalive(new_fd, 1, 1, 1, 3);
        fprintf(stdout,"Server get connection from %s\n", inet_ntoa(client_addr.sin_addr));
        //////////////////////////////////
		//子进程处理接入的TCP连接
		//父进程继续监听
        /////////////////////////////////
		pid_t pid = fork();
		if (pid < 0){
			perror("call to fork");
			exit(1);
		}

		else if (0 == pid){
			close(sockfd);
			while(1){

				//////////////////////////////////////
				//主动上传变化的数据报文
				dataup_104frame(new_fd);
				//////////////////////////////////////

				///////////////////////////////////////////////
				//检查TCP连接状态，断开已中断的连接
				///////////////////////////////////////////////
				if(new_fd <= 0){
					exit(1);
				}
				//检查TCP连接的状态
				if(netcheck(new_fd) < 0){
                    shutdown(new_fd, SHUT_WR);
                    close(new_fd);
                    exit(1);
				}

				//读取104报文数据
				if((recbytes=recv(new_fd, buf, 1024, 0)) < 0){
					perror("call to recv");
					close(new_fd);
					exit(1);
				}
				//处理接收到的104报文
				process_104frame(new_fd, buf, recbytes);


			}
		}

		else{
			close(new_fd);
			//避免子进程成为僵尸进程
			signal(SIGCHLD,SIG_IGN);
			fprintf(stdout, "IEC104 server continues listening\n\r");
		}

	}

	exit(0);
}
