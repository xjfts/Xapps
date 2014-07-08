/*
 * Copyright (c) 2012 Xilinx, Inc.  All rights reserved.
 *
 * Xilinx, Inc.
 * XILINX IS PROVIDING THIS DESIGN, CODE, OR INFORMATION "AS IS" AS A
 * COURTESY TO YOU.  BY PROVIDING THIS DESIGN, CODE, OR INFORMATION AS
 * ONE POSSIBLE   IMPLEMENTATION OF THIS FEATURE, APPLICATION OR
 * STANDARD, XILINX IS MAKING NO REPRESENTATION THAT THIS IMPLEMENTATION
 * IS FREE FROM ANY CLAIMS OF INFRINGEMENT, AND YOU ARE RESPONSIBLE
 * FOR OBTAINING ANY RIGHTS YOU MAY REQUIRE FOR YOUR IMPLEMENTATION.
 * XILINX EXPRESSLY DISCLAIMS ANY WARRANTY WHATSOEVER WITH RESPECT TO
 * THE ADEQUACY OF THE IMPLEMENTATION, INCLUDING BUT NOT LIMITED TO
 * ANY WARRANTIES OR REPRESENTATIONS THAT THIS IMPLEMENTATION IS FREE
 * FROM CLAIMS OF INFRINGEMENT, IMPLIED WARRANTIES OF MERCHANTABILITY
 * AND FITNESS FOR A PARTICULAR PURPOSE.
 *
 */

#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <byteswap.h>
#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
#include <errno.h>
#include <sys/mman.h>
#include <string.h>

#define PAGE_SIZE ((size_t)getpagesize())
#define PAGE_MASK ((uint64_t)(long)~(PAGE_SIZE - 1))

int main(int argc, char **argv)
{
    int fd;
    uint64_t offset, base;
    volatile uint8_t *mm;
    int i;

    fd = open("/dev/mem", O_RDWR | O_SYNC);
    if(fd < 0) {
    	fprintf(stderr, "open (/dev/mem) failed (%d) \n", errno);
    	return 1;
    }

    offset = 0x3FF00000;
    base = offset & PAGE_MASK;
    offset &= ~PAGE_MASK;
    fprintf(stdout, "PAGE_SIZE (0x%x) \n", PAGE_SIZE);
    fprintf(stdout, "PAGE_MASK (0x%x) \n", PAGE_MASK);
    fprintf(stdout, "base (0x%x) \n", base);
    fprintf(stdout, "offset (0x%x) \n", offset);

    mm = mmap(NULL, PAGE_SIZE, PROT_READ | PROT_WRITE,
    		MAP_SHARED, fd, base);
    if(MAP_FAILED == mm) {
    	fprintf(stderr, "mmap64 (0x%x@0x%x) failed (%d) \n",
    			PAGE_SIZE, base, errno);
    }
    else {
    	fprintf(stdout, "mmap64 (0x%x) success!\n", base);
    }
    for(i=0; i<576; i++) {
    	*(volatile uint32_t *)(mm + offset + (i<<2)) = 0xffffffff;
    }

    munmap((void *)mm, PAGE_SIZE);
    close(fd);

    return 0;
}
