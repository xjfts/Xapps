#include <linux/config.h>
#include <linux/module.h>
#include <linux/moduleparam.h>
#include <linux/init.h>

#include <linux/sched.h>
#include <linux/kernel.h>	/* printk() */
#include <linux/fs.h>		/* everything... */
#include <linux/errno.h>	/* error codes */
#include <linux/delay.h>	/* udelay */
#include <linux/kdev_t.h>
#include <linux/slab.h>
#include <linux/mm.h>
#include <linux/ioport.h>
#include <linux/interrupt.h>
#include <linux/workqueue.h>
#include <linux/poll.h>
#include <linux/wait.h>

#include <asm/io.h>

#define MY_GPIO_REG_NUM	0x128	/* use 8 ports by default */
#define XPAR_AXI_GPIO_GIC_0_BASEADDR 0x41200000

#define XGPIO_TRI_OFFSET	0x4
#define XGPIO_GIE_OFFSET	0x11C /**< Glogal interrupt enable register */
#define XGPIO_ISR_OFFSET	0x120 /**< Interrupt status register */
#define XGPIO_IER_OFFSET	0x128 /**< Interrupt enable register */
#define GPIO_IRQ 31
#define DEVICE_NAME "gpio_inter"

static void __iomem *GPIO_Regs;

static int my_gpio_open(struct inode * inode , struct file * filp)
{
	return 0;
}
static int my_gpio_release(struct inode * inode, struct file *filp)
{
	return 0;
}
static long my_gpio_ioctl(struct file *filp, unsigned int reg_num, unsigned long arg)
{
	return 0;
}
static const struct file_operations my_gpio_fops =
{
     .owner = THIS_MODULE,
     .open = my_gpio_open,
     .release = my_gpio_release,
     .unlocked_ioctl = my_gpio_ioctl,
};

static struct miscdevice my_gpio_dev =
{
     .minor = MISC_DYNAMIC_MINOR,
     .name = DEVICE_NAME,
     .fops = &my_gpio_fops,
};
irqreturn_t short_interrupt(int irq, void *dev_id, struct pt_regs *regs)
{
	int Register;

	printk("my_gpio:this is a ISR for gpio\n");
	Register = ioread32(GPIO_Regs+XGPIO_ISR_OFFSET);
	iowrite32(Register & 0x01, GPIO_Regs+XGPIO_ISR_OFFSET);

	return IRQ_HANDLED;
}
static int __init gpio_init(void)
{	
	int ret;
	int Register;

	GPIO_Regs = ioremap(XPAR_AXI_GPIO_GIC_0_BASEADDR, MY_GPIO_REG_NUM); 
	printk("my_gpio: Access address to device is:0x%x\n", (unsigned int)GPIO_Regs);
	if(GPIO_Regs == NULL){
		printk("my_gpio:[ERROR] Access address is NULL!\n");
		return -EIO;
 	 } 	
	ret = misc_register(&my_gpio_dev);
	if (ret){
		printk("my_gpio:[ERROR] Misc device register failed\n");
		return ret;
  	}
  	ret = request_irq(GPIO_IRQ, gpio_interrupt,
				SA_INTERRUPT,"gpio_inter",
				NULL);
	if (ret) {
		printk(KERN_INFO "GPIO: can't get assigned irq %i\n", GPIO_IRQ);
		goto end;
	}
	else{
		iowrite32(0x01, GPIO_Regs+XGPIO_TRI_OFFSET);
		iowrite32(0x80000000, GPIO_Regs+XGPIO_GIE_OFFSET);
		Register=ioread32(GPIO_Regs+XGPIO_IER_OFFSET);
		iowrite32(Register | 0x01, GPIO_Regs+XGPIO_IER_OFFSET);
	}
	
	printk("XilinxGpio: OK! Module init complete\n");
end:
	return 0;
}

static void __exit gpio_exit(void)
{
	free_irq(GPIO_IRQ, NULL);
	iounmap(GPIO_Regs);
	misc_deregister(&my_gpio_dev);
	printk("my_gpio: Module exit\n");
}

module_init(gpio_init);
module_exit(gpio_exit);

MODULE_AUTHOR("teng");
MODULE_DESCRIPTION("GpioDriver");
MODULE_ALIAS("It's only a gpio test of Xilinx zynq");
MODULE_LICENSE("Dual BSD/GPL");




