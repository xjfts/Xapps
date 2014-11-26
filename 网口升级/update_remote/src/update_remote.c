/******* 客户端程序  TCPClient.c ************/
#include <stdlib.h>
#include <stdio.h>
#include <errno.h>
#include <string.h>
#include <netdb.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <fcntl.h>
#include <unistd.h>


int main(int argc, char *argv[])
{
	 int sockfd;
	 char end;
	 char start = 0x01;
	 struct sockaddr_in server_addr;
	 int portnumber,nbytes;


	 if(argc!=3)
	 {
	  fprintf(stderr,"Usage:%s hostname portnumber\a\n",argv[0]);
	  exit(1);
	 }

	 if((portnumber=atoi(argv[2]))<0)
	 {
			fprintf(stderr,"Usage:%s hostname portnumber\a\n",argv[0]);
			exit(1);
	 }

	/*客户程序开始建立 sockfd描述符*/
	 if((sockfd=socket(AF_INET,SOCK_STREAM,0))==-1)
	 {
			fprintf(stderr,"Socket Error:%s\a\n",strerror(errno));
			exit(1);
	 }

	/*客户程序填充服务端的资料*/
	 bzero(&server_addr,sizeof(server_addr));
	 server_addr.sin_family=AF_INET;
	 server_addr.sin_port=htons(portnumber);
	 server_addr.sin_addr.s_addr = inet_addr(argv[1]);

	/*客户程序发起连接请求*/
	//服务器忙则connect进入阻塞状态
	 if(connect(sockfd,(struct sockaddr *)(&server_addr),sizeof(struct sockaddr))==-1)
	{
	  fprintf(stderr,"Connect Error:%s\a\n",strerror(errno));
	  exit(1);
	}
	 fprintf(stdout,"zynq linking ok\n");

	 if((nbytes = send(sockfd, &start, 1,0)) == -1){
		 fprintf(stderr,"send Error:%s\n",strerror(errno));
		 exit(1);
	 }

	if((nbytes=recv(sockfd, &end, 1,0))==-1){
			fprintf(stderr,"update flash error\n");
			exit(1);
	 }
	 if(end == 0x02)
		 printf("update flash OK\n");
	 else
		 fprintf(stderr,"update flash error: %d\n", end);
	/*结束通讯*/
	 close(sockfd);
	 exit(0);
}
