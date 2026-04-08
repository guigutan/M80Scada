using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZSockets
{
    class AgreeMent
    {
    }
    /// <summary>
    /// 系统型号协议
    /// </summary>
    public enum sysType
    {
        EZNC_SYS_MAGICCARD64 = 0,
        EZNC_SYS_MAGICBOARD64 = 1,
        EZNC_SYS_MELDAS6X5L = 2,
        EZNC_SYS_MELDAS6X5M = 3,
        EZNC_SYS_MELDASC6C64 = 4,
        EZNC_SYS_MELDAS700L = 5,
        EZNC_SYS_MELDAS700M = 6,
        EZNC_SYS_MELDASC70 = 7,
        EZNC_SYS_MELDAS800L = 8,
        EZNC_SYS_MELDAS800M = 9
    }
    /// <summary>
    /// 报警类型协议
    /// </summary>
    enum alarmType
    {
        M_ALM_ALL_ALARM = (0x0),
        M_ALM_NC_ALARM = (0x100),
        M_ALM_STOP_CODE = (0x200),
        M_ALM_PLC_ALARM = (0x300),
        M_ALM_OPE_MSG = (0x400),
        M_ALM_WARNING = (0x500)
    }
    /// <summary>
    /// 程序相关协议
    /// </summary>
    enum progType
    {
        EZNC_MAINPRG = 0,
        EZNC_SUBPRG = 1
    }
    /// <summary>
    /// 程序号
    /// </summary>
    enum proNum
    {
        EZNC_PRG_MAXNUM = 0,
        EZNC_PRG_CURNUM = 1,
        EZNC_PRG_RESTNUM = 2,
        EZNC_PRG_CHARNUM = 3,
        EZNC_PRG_RESTCHARNUM = 4
    }

    /// <summary>
    /// 打开文件模式
    /// </summary>
    enum openFileMode
    {
        EZNC_FILE_INIT = 0,
        EZNC_FILE_READ = 1,        //读模式
        EZNC_FILE_WRITE = 2,       //写模式
        EZNC_FILE_OVERWRITE = 3  //覆盖模式（即使指定文件存在，也会写入）。
    }

    enum fileDir
    {
        EZNC_DISK_DIRTYPE =0x10000,   //读取目录信息
        EZNC_DISK_COMMENT=0x4 ,           //读取的注释信息（仅在NC控制单元一侧）
        EZNC_DISK_DATE=0x2,//读取日期信息（仅在个人计算机端）
        EZNC_DISK_SIZE=0x1//,读取大小信息
    } 
}
