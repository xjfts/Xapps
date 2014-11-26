/*******服务器程序  TCPServer.c ************/
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

#define WAITBUF 10

int main(int argc, char *argv[])
{
	 int sockfd,new_fd;
	 struct sockaddr_in server_addr;
	 struct sockaddr_in client_addr;
	 int sin_size,portnumber;
	 int nbytes;
	 int status;
	 char start;
	 char end = 0 ;
	 char command[50];

	 if(argc!=2)
	 {
		  fprintf(stderr,"Usage:%s portnumber\a\n",argv[0]);
		  exit(1);
	 }
	 /*端口号不对，退出*/
	 if((portnumber=atoi(argv[1]))<0)
	 {
		  fprintf(stderr,"Usage:%s portnumber\a\n",argv[0]);
		  exit(1);
	 }

	/*服务器端开始建立socket描述符*/
	 if((sockfd=socket(AF_INET,SOCK_STREAM,0))==-1)
	 {
		 fprintf(stderr,"Socket error:%s\n\a",strerror(errno));
		// perror("call to socket");
		exit(1);
	 }

	/*服务器端填充 sockaddr结构*/
	 bzero(&server_addr,sizeof(struct sockaddr_in));
	 server_addr.sin_family=AF_INET;
	/*自动填充主机IP*/
	 server_addr.sin_addr.s_addr= inet_addr("10.12.82.10");
	 server_addr.sin_port=htons(portnumber);

 	 /*捆绑sockfd描述符*/
 	 if(bind(sockfd,(struct sockaddr *)(&server_addr),sizeof(struct sockaddr))==-1){
			fprintf(stderr,"Bind error:%s\n\a",strerror(errno));
			// perror("call to socket");
			exit(1);
 	 }

 	 /*监听sockfd描述符*/
 	 if(listen(sockfd, WAITBUF)==-1){
 		 fprintf(stderr,"Listen error:%s\n\a",strerror(errno));
 		 exit(1);
 	 }
 	 fprintf(stdout,"Accepting connections ...\n");
 	 while(1){
        /*服务器阻塞，直到客户程序建立连接*/
        sin_size=sizeof(struct sockaddr_in);
        if((new_fd=accept(sockfd,(struct sockaddr *)(&client_addr),(socklen_t *)&sin_size)) == -1){
                fprintf(stderr,"Accept error:%s\n\a",strerror(errno));
                exit(1);
		}

        fprintf(stdout,"Server get connection from %s\n", inet_ntoa(client_addr.sin_addr));

		if((nbytes = recv(new_fd, &start, 1, 0)) == -1){
			perror("call to recv");
			exit(1);
		}
		sprintf(command, "sh /update_flash.sh %d", new_fd);

		if(0x01 == start)
			status = system(command);
	    if(-1 == status){
	        printf("system error!");
	    }
	    else{
	        printf("exit status value = [0x%x]\n", status);

	        if (WIFEXITED(status))
	        {
	            if (0 == WEXITSTATUS(status))
	            {
					 if((nbytes = send(new_fd, &end, 1, 0)) == -1){
						 fprintf(stderr,"send Error:%s\n",strerror(errno));
						 exit(1);
					 }
	                printf("run update flash shell script successfully.\n");

	            }
	            else
	            {
	                printf("run update flash shell script fail, script exit code: 0x%x\n", WEXITSTATUS(status));
	            }
	        }
	        else
	        {
	            printf("shell script run error exit status = [%d]\n", WEXITSTATUS(status));
	        }
	    }

		if((nbytes = recv(new_fd, &start, 1, 0)) == -1){
			perror("call to recv");
			exit(1);
		}
		if(0x02 == start)
			close(new_fd);
		//循环下一个
 	 }
 	 close(sockfd);
 	 exit(0);
}
