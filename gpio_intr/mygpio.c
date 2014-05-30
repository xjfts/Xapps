#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <poll.h>
#include <errno.h>
#include <unistd.h>
#include <string.h>

int main(int argc, char *argv[])
{
    int GpioNo, Exportfd, Directionfd, edgefd, ret;
    char buf[50], getbuf[10];
    struct pollfd fds[1];

    if(argc != 2){
         printf("usage: mygpio No. No is your gpio No.\n");
         exit(1);
    }
    printf("GPIO test is running...\n");

    Exportfd = open("/sys/class/gpio/export", O_WRONLY);
    if(Exportfd < 0){
         printf("Cannot open GPIO to export it\n");
         perror("OPEN EXPORT:");
         exit(1);
    }

    write(Exportfd, argv[1], 4);
    close(Exportfd);

    printf("GPIO exported successfully\n");

    sprintf(buf, "/sys/class/gpio/gpio%s/direction", argv[1]);
    Directionfd = open(buf, O_RDWR);
    if (Directionfd < 0){
         printf("Cannot open GPIO direction it\n");
         exit(1);
    }
    write(Directionfd, "in", 3);
    close(Directionfd);
    printf("GPIO direction set as input successfully\n");


    memset(buf, 0, 50);
    sprintf(buf, "/sys/class/gpio/gpio%s/edge", argv[1]);
    edgefd=open(buf, O_RDWR);
    if (edgefd < 0){
         printf("Cannot open GPIO edge\n");
         exit(1);
    }
    write(edgefd, "rising", 7);
    close(edgefd);
    printf("GPIO edgefd set as rising successfully\n");

    memset(buf, 0, 50);
    sprintf(buf, "/sys/class/gpio/gpio%s/value", argv[1]);
    fds[0].events = POLLPRI;

    while(1){
        if((fds[0].fd = open(buf, O_RDONLY)) < 0){
            fprintf(stderr, "open value error: %s", strerror(errno));
            exit(1);
        }
        ret=read(fds[0].fd, getbuf, 10);
        if(ret == -1)
            perror("read gpio error:");
        if(poll(fds, 1, -1) < 0){
            printf("poll error\n");
            exit(1);
        }
        printf("interrupt of GPIO occurs %d\n", fds[0].revents);
        close(fds[0].fd);
    }

    exit(0);
}
