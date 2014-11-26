/******* ¿Í»§¶Ë³ÌÐò  TCPClient.c ************/
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
	 int socket;
	 char data;
	 ssize_t nbytes;

	 if(argc!=3)
	 {
		  fprintf(stderr,"Usage:%s socket data\a\n",argv[0]);
		  exit(1);
	 }

	 if((socket=atoi(argv[1]))<0)
	 {
			fprintf(stderr,"Usage:%d socket data\a\n",socket);
			exit(1);
	 }
	 data = atoi(argv[2]);

	 printf("socket:%d, data:%d\n", socket, data);

	 if((nbytes = send(socket, &data, 1, 0)) == -1){
		 fprintf(stderr,"send Error:%s\n",strerror(errno));
		 exit(1);
	 }

	 exit(0);
}
