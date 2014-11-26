#include <sys/socket.h>
#include <sys/types.h>
#include <unistd.h>
#include <string.h>
#include <stdio.h>
#include <arpa/inet.h>
#include <stdlib.h>


#define MAXLINE 1400
#define SERV_PORT 8888
#define CISHU 12000

char sendline[MAXLINE];
char recvbuf[MAXLINE];

void do_cli(int sockfd,struct sockaddr *pservaddr,socklen_t *servlen)
{
	unsigned int i;
	int j = 1;
	int idx;
	int sum=0;
	ssize_t recv_len;

	sendline[0] = 0x55;
	sendline[1] = 0xAA;

	if(connect(sockfd,(struct sockaddr*)pservaddr, *servlen)==-1)
	{
		perror("connecterror");
		exit(1);
	}

	for(i=0; i<CISHU; i++){

		idx = j++;
		sendline[3] = idx & 0xFF;
		sendline[2] = (idx>>8) & 0xFF;

		sum = sendline[0]+sendline[1]+sendline[2]+sendline[3];
		sendline[1398] = sum & 0xFF;
		sendline[1399] = ((sum>>8) & 0xFF);

		sendto(sockfd, sendline, MAXLINE, 0, pservaddr, *servlen);
	}
}


int main(int argc,char **argv)
{
	int sockfd;
	struct sockaddr_in servaddr;
	socklen_t clt_len;

	if(argc!=2)
	{
		printf("usage:udpclient\n");
		exit(1);
	}

	bzero(&servaddr,sizeof(servaddr));
	servaddr.sin_family=AF_INET;
	servaddr.sin_port=htons(SERV_PORT);
	if(inet_pton(AF_INET,argv[1],&servaddr.sin_addr)<=0)
	{
		printf("[%s]isnotavalidIPaddress\n",argv[1]);
		exit(1);
	}

	sockfd=socket(AF_INET,SOCK_DGRAM,0);
	clt_len = sizeof(servaddr);
	do_cli(sockfd,(struct sockaddr*)&servaddr, &clt_len);
	return 0;
}
