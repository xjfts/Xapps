/***************************************
 *
 * file iec104.h
 *
 */
#ifndef _IEC104_H_
#define _IEC104_H_

#define	STARTDT_ACT								0x7				// 104 protocol start data transmit act
#define	STARTDT_CONFIRM							0x0b			// 104 protocol start date transmit confirm

#define	STOPDT_ACT								0x13			// 104 protocol stop  data transmit act
#define	STOPDT_CONFIRM							0x23			// 104 protocol stop  data transmit confirm

#define	TESTFR_ACT								0x43			// 104 protocol test frame act
#define	TESTFR_CONFIRM							0x83			// 104 protocol test frame confirm


#define	IEC104_OFFSET_LEN						1
#define	IEC104_OFFSET_CODE						2
#define	IEC104_OFFSET_TI						6
#define	IEC104_OFFSET_VSQ						7
#define	IEC104_OFFSET_COT						8
#define	IEC104_OFFSET_SECT						10
#define IEC104_OFFSET_ADDR      				11				// sub address
#define IEC104_OFFSET_INF						12
#define IEC104_OFFSET_CONTEXT					15
#define MIN_IEC104_FRAMELEN						13







typedef struct
{
	unsigned short	millionsecond;
	unsigned char	minute;
	unsigned char	hour;
	unsigned char	day;
	unsigned char	month;
	unsigned char	year;
	unsigned char   reserve;
	unsigned char	second;
}systime_t;


#endif
