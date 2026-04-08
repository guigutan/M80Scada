using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common;
using System.IO;


namespace EZSockets
{
    /// <summary>
    /// 封装的三菱的通讯类
    /// </summary>
    public class MitCom
    {
        private EZNCAUTLib.DispEZNcCommunication EZNcCom;//通讯库变量
       
        private int lResult=1;
        private int lSystemType;
        private string szMessage;
        public event Common.MyDelegate<string, string> AlarmData;//声明事件
        Random x = new Random();

        /// <summary>
        /// 连接三菱设备
        /// </summary>
        public void GetSimConnect(string nCTypeCmb,string nCCardNo,string timeOut,string ipAdress)
        {
            int lResultOpen = -1;
            int lResultClose = -1;
            int lMachine;
            int lTimeOut;
            string strHostName;
            if (EZNcCom == null)
            {
                EZNcCom = new EZNCAUTLib.DispEZNcCommunication();
              
                switch (nCTypeCmb)
                {
                    case "MAGIC64":
                        lSystemType = (int)sysType.EZNC_SYS_MAGICCARD64;
                        break;
                    case "M6x5M":
                        lSystemType = (int)sysType.EZNC_SYS_MELDAS6X5M;
                        break;
                    case "M6x5L":
                        lSystemType = (int)sysType.EZNC_SYS_MELDAS6X5L;
                        break;
                    case "C6/C64":
                        lSystemType = (int)sysType.EZNC_SYS_MELDASC6C64;
                        break;
                    case "M700M":
                        lSystemType = (int)sysType.EZNC_SYS_MELDAS700M;
                        
                        break;
                    case "M700L":
                        lSystemType = (int)sysType.EZNC_SYS_MELDAS700L; 
                        break;
                    case "M800M":
                        lSystemType = (int)sysType.EZNC_SYS_MELDAS800M;
                        break;
                    case "M800L":
                        lSystemType = (int)sysType.EZNC_SYS_MELDAS800L;
                        break;
                    case "M80B":
                        break;
                    default:
                        MessageBox.Show("三菱NC 类型选取错误！");
                        return;
                }
                lMachine = Convert.ToInt32(nCCardNo);
                lTimeOut = Convert.ToInt32(timeOut);
                if (string.IsNullOrEmpty(ipAdress))
                {
                    strHostName = "EZNC_LOCALHOST";
                }
                else
                {
                    if (ipAdress == "连接前请设定")
                    {
                        strHostName = "EZNC_LOCALHOST";
                    }
                    else
                    {
                        strHostName = ipAdress;
                    }
                }
                if (lSystemType == (int)sysType.EZNC_SYS_MELDASC6C64 || lSystemType == (int)sysType.EZNC_SYS_MELDAS700M ||
                   lSystemType == (int)sysType.EZNC_SYS_MELDAS700L || lSystemType == (int)sysType.EZNC_SYS_MELDAS800M ||
                   lSystemType == (int)sysType.EZNC_SYS_MELDAS800L)
                {
                    if (lSystemType == (int)sysType.EZNC_SYS_MELDASC6C64)
                    {
                        lResult = EZNcCom.SetTCPIPProtocol(strHostName, 64758);
                        //64758
                    }
                    else
                    {
                        lResult = EZNcCom.SetTCPIPProtocol(strHostName, 683);
                        //683
                    }

                    ErrorCheck("SetTCPIPProtocol");

                    if (lResult == 0)
                    {
                        lResultOpen = EZNcCom.Open2(lSystemType, lMachine, lTimeOut, "EZNC_LOCALHOST");//EZNC_LOCALHOST
                        lResult = lResultOpen;
                        ErrorCheck("Open");
                    }
                }
                else
                {
                    lResultOpen = EZNcCom.Open2(lSystemType, lMachine, lTimeOut, strHostName);
                    ErrorCheck("Open");
                }
            }
            else
            {

                lResult = EZNcCom.Close();
                ErrorCheck("Close");
                EZNcCom = null;

            }
        }

        /// <summary>
        /// 获取版本号
        /// IAxistNo:设置轴号（从轴1 =从1）
        /// lIndex：设置参数编号。 
        /*lIndex=0: NC系统S / W编号，名称和PLC版本取决于系统规格。
         lIndex=1控制单元，扩展单元取决于系统规格。
         lIndex=2 RIO单元，终端RIO单元轴设置仅适用于C70。取决于系统规格。*/

        /* 以UNICODE字符串形式获取各种NC系统软件版本信息。
         0：获取NC系统的系统S / W编号，名称和PLC版本。 字符串数据的格式如下:
        NC系统软件编号\ tNC系统名称\ t可编程控制器系统编号\ 0在NC系统编号和NC系统的S / W名称之间插入TAB代码。数据的末尾变成NULL代码。输出示例：“BND-2005W000-A0三菱CNC 830WM”
        如果没有项目，将会出现一个TAB代码。如果终止项不存在，则为NULL.代码将遵循TAB代码。
         1：获取控制单元和扩展单元版本。
         字符串数据的格式如下。
        控制单元号\ t扩展单元号\ 0
        在控制单元号码和分机号码之间插入一个TAB代码。
        数据的末尾变成NULL代码。
         2：获取RIO单元和终端RIO单元版本。
        字符串数据的格式如下。
        RIO单元号\ tTerminal RIO单元号\ 0
        M700有RIO单元1 \ t RIO单元2 \ t ... \ 0有24个项目。
        M700 / M800系列有RIO单元1¥RIO单元2¥t5 ...¥0最多32项（*）。
       *确认与MTB的RIO单元的数量。
        在RIO单元号码和终端RIO单元号码之间插入TAB代码。
        数据的末尾变成NULL代码。
        由于在本产品中分配字符串区域内存，使用VC ++的客户端需要
        用CoTaskMemFree（）显式释放字符串区域内存。*/
        /// </summary>
        public void GetSimVersion(int lAxistNo, int lIndex, out string version)
        {
            version = "";
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.System_GetVersion(lAxistNo, lIndex, out version);//系统号码，名称，控制S/W版本取得
                    ErrorCheck("GetVersion");
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
                version = "";
            }
        }

        /// <summary>
        /// 获取自动运行状态：
        /// lindex=0：刀具测量。plStatus=0：刀具未测量；=1：已测量
        /// lindex=1:在自动运行模式下：获取系统自动运行的状态指示。plStatus=0：无自动运行；=1：自动运行
        /// lindex=2:自动运行开始：获取系统自动运行的状态指示,并且正在执行移动命令或M，S，T，B进程. 。plStatus=0：没有开始自动运行；=1：开始自动运行
        /// lindex=3：自动运行暂停：在执行移动指令或其他自动操作指令时，获取表示自动操作暂停的状态。plStatus=0：无暂停；=1：暂停
        /// 函数说明：
        /// 获取运行状态。自动运行暂停功能仅在M700 / M800系列中有效。指示PLC接口自动运行状态的信号包括“自动运行”，“自动运行启动”和“自动运行暂停”。
        /// 在每个状态下这三个信号的ON / OFF状态如下所示。
        /// </summary>
        public void GetRunStatus(int lindex,out int plStatus)
        {
            plStatus = 0;
            try
            {
                if (EZNcCom != null)
                {

                    lResult = EZNcCom.Status_GetRunStatus(lindex, out plStatus);
                    ErrorCheck("GetRunStatus");
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        public int CommonVRead(int addr, out double value)
        {
            double data = 0.0;
            int type;
            lResult = EZNcCom.CommonVariable_Read2(addr, out data, out type);

            value = data;

            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.CommonVariable_Read2(addr, out data, out type);
                    ErrorCheck("CommonVRead");
                    if (lResult == 0)
                        return 0;
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }

            return -1;
        }

        #region 轴坐标
        double dPosition = 0.0f;
        /* //注意这里的库用的是表示度为14的，下方有表示度282的另一个方法。别用错库。*/
        /// <summary>
        /// 获取机械坐标
        /// </summary>
        public void GetAxisPosition(int axistno,out Communicationcs<string ,string> DataContainer )
        {
            DataContainer = new Communicationcs<string, string>();
            try
            {
                if (EZNcCom != null)
                {
                    for (int i = 1; i <= axistno; i++)
                    {
                        #region 获取剩余行程
                        lResult = EZNcCom.Position_GetDistance2(i, out dPosition, 0);
                        ErrorCheck("GetDisTance");
                        if (lResult == 0)
                        {
                            DataContainer.simDisPos.Add(dPosition.ToString("#0.000"));
                        }
                        #endregion

                        #region 获取当前坐标(相对坐标)
                        lResult = EZNcCom.Position_GetCurrentPosition(i, out dPosition);
                        ErrorCheck("GetCurrentPosition");
                        if (lResult == 0)
                        {
                            DataContainer.simCurPos.Add(dPosition.ToString("#0.000"));
                        }
                        #endregion

                        #region 获取工件坐标
                        lResult = EZNcCom.Position_GetWorkPosition(i, out dPosition, 0);
                        ErrorCheck("GetWorkPosition");
                        if (lResult == 0)
                        {
                            DataContainer.simWorkPos.Add(dPosition.ToString("#0.000"));
                        }
                        #endregion

                        #region  获取机械坐标
                        lResult = EZNcCom.Position_GetMachinePosition(i, out dPosition, 0);
                        ErrorCheck("GetMachinePosition");
                        if (lResult == 0)
                        {
                            DataContainer.simMaPos.Add(dPosition.ToString("#0.000"));
                        }
                        #endregion
                    }
                }
            }
            catch 
            {
                ErrorCheck("通讯已关闭" );
            }
           
        }
        #endregion

        public void GetSpindleSpeedSet(out int pldata)
        {
            pldata = -1;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Command_GetCommand2(1, 1, out pldata);
                    ErrorCheck("GetSpindleSpeedSet");
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// 获取主轴信息
        /// </summary>
        public void GetSpindleInfo(int index,int lspindle,out int pldata, out string loadstr)
        {
            #region 接口描述
            /*** 主轴监视 
             * lIndex：设置主轴的参数编号。
             * lIndex    描述数据                                          范围                备注
             * 0         增益。 主轴位置回路增益。                         单位：1 / s
             * 1        下垂。 位置偏差量。                                单位：L
             * 2        主轴（SR，SF）转速。 主轴电机转速。 包括进给。     0[RPM]
             * 3        负载。 主轴电机负载。                              0 [％]
             * 4        LED显示屏。 驱动器上有7段LED显示屏。              从“00 \ 0”到“FF \ 0”输出一个3位数的字符串。
             * 5        报警1。                                           最多3个字母数字字符
             * 6        报警2                                             同上
             * 7        报警3                                             同上
             * 8        报警4                                             同上                 M700/M800 series onl
             * 10       周期计数器。
             * 11       控制输入1。
             * 12
             * 13
             * 14
             * 15       控制输出1。
             * 16
             * 17
             * 18
             * 19
             * **lSpindle：设定主轴编号。
             * ** plData：返回主轴状态。
             * ** lppwszBuffer：获取主轴信息作为UNICODE字符串。
             * ***/
            /*** 主轴诊断
             * AxisNo：设置轴号（从轴1 = 1）
             * lIndex：设置诊断信息。
             * lIndex    描述数据                                          范围                备注
             * 0         工作时长
             * 1         报警记录1（报警号码）。                           上一个主轴报警号码
             * 2                 2（报警号码）。
             * 3                 3（报警号码）。
             * 4                 4（报警号码）。
             * 5                 5（报警号码）。
             * 6                 6（报警号码）。
             * 7                 7（报警号码）。
             * 8                 8（报警号码）。
             * 11        报警记录1（时间）。                               上一个主轴报警时间
             * 12                2（时间）。   
             * 13                3（时间）。   
             * 14                4（时间）。   
             * 15                5（时间）。   
             * 16                6（时间）。   
             * 17                7（时间）。   
             * 18                8（时间）。   
             * ***/

            #endregion
            pldata=-1;
            loadstr="";
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Monitor_GetSpindleMonitor(index, lspindle, out pldata, out loadstr);
                    ErrorCheck("GetSpindleMonitor");
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// 获取伺服轴信息
        /// lAxisNo: Sets the axis No. (From Axis 1 = from 1)
        ///lindex：设置在所述一组系统中设定轴号的参数号。
        /******************
         * lIndex            Description                                   Data range                   Remarks
         * 0                 获得：位置增益回路显示                          1/s
         * 1                 下垂：跟踪延迟                                   i
         * 2                 实际的电机速度                                  rpm
         * 3                 负载电流,电机电流（停机时转换为连续电流显示）。  %
         * 4                 最大电流1                                        %
         * 5                 最大电流2                                        %
         * 6                 超负荷                                           %
         * 7                 重新加载，再生负载                               %
         * 10                周期计数器                                       pulse
         * 101               报警1                                            输出一个3位数字字符串。
         * ****************/
        ///plData：返回轴的状态。
        ///lppwszBuffer：输出数据（返回值）作为UNICODE字符串，当任何100到104为lIndex设置。
        ///对于M700 / M800系列，当lIndex设置为11到15，18到20或100到104中的任意一个时，将数据（返回值）作为UNICODE字符串输出。
        ///获取设定零件系统中设定轴号的伺服监控信息。当索引表中的数据范围为[单位：指令单位]时，需要根据三菱CNC的指令单位进行转换。 设定单位请参照三菱CNC的规格
        /// </summary>
        public void GetServoInfo(int lAxisNo,int index,out int plData,out string loaStr,string alarmCode)
        {
            plData = -1;
            loaStr = "";
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Monitor_GetServoMonitor(lAxisNo, index, out plData, out loaStr);
                    ErrorCheck(alarmCode);
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
            
        }

        /// <summary>
        /// Group: 设置参数的组号。这个似乎对读取数据没影响
        /// item:参数号
        /// Size：设置读取参数的数量。 范围从1开始。最多可取5个
        /// lAxis：设置要获取参数的轴。除非需要获取的参数取决于轴，否则不需要设置该参数
        /// 
        /// </summary>
        public void GetParaValue(int lGroup, int lItem, int lSize, int lAxis, out string overValue)
        {
            object  pvValue=null;
            overValue = "";
             try
             {
                 if (EZNcCom != null)
                 {
                     if (lGroup == 3)
                     {
                         for (int i = 1; i <= lAxis; i++)
                         {
                             lResult = EZNcCom.Parameter_GetData3(lGroup, lItem, lSize, i, out pvValue);
                         }
                     }
                     else
                     {
                         lResult = EZNcCom.Parameter_GetData3(lGroup, lItem, lSize, 1, out pvValue);
                     }
                     ErrorCheck("GetParameterData");
                     var value = pvValue as string[];
                     if (value != null&&value .Count() >=0)
                     {
                         overValue = value[0];
                     }

                 }

             }
             catch
             {
                 ErrorCheck("通讯已关闭");
             }

        }

        /// <summary>
        /// 获取进给速递
        ///FeedSpeedType:进给速度类型，设置要获得的进给速度的类型。
        /*FeedSpeedType=0;指令进给速度（FA）
         =1;手动有效进给速度（FM）
         =2;同步进给速度（FS）
         =3;攻丝（FE）
         pdspeed:返回指定系统的进给速度。*/
        /// </summary>
        public void GetFeedSpeed(int lFeedType,out double pdSpeed,string alarmMsg)
        {
            pdSpeed = 0.0;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Position_GetFeedSpeed(lFeedType, out pdSpeed);
                    ErrorCheck(alarmMsg);
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }

        }

        /// <summary>
        /// 获取报警消息
        /// lMessageNumber：设置要获取的消息的数量。 值：1至10（最大）
        /// lAlarmType：设置要获取的警报类型
        /// M_ALM_NC_ALARM: NC报警
        /// M_ALM_STOP_CODEL:停止代码
        /// M_ALM_PLC_ALARM :PLC报警消息
        /// M_ALM_OPE_MSG:操作员消息
        /// M_ALM_ALL_ALARM:不区分报警类型
        /// lppwszBuffer：以UNICODE字符串形式获取报警信息。消息格式包括CR，LF代码来区分消息。 此外，在消息的末尾插入NULL。
        ///获取设定NC控制单元中当前产生的报警的报警信息。该报警信息的语言符合NC参数（＃1043 lang）。
        ///消息按重要性降序排列。由于在本产品中分配字符串区域内存，使用VC ++的客户端需要用CoTaskMemFree（）显式释放字符串区域内存。
        ///M800系列不支持此功能。 （EZ_ERR_NOT_SUPPORT返回plRet。）
        /// </summary>
        public void GetAlarm(out string[] AlarmMsg)
        {
            AlarmMsg = new string[3];
            int[] lRes = new int[3];
            int tmp = 0;
            try
            {
                if (EZNcCom != null)
                {
                    if (lSystemType == (int)sysType.EZNC_SYS_MELDAS6X5M || lSystemType == (int)sysType.EZNC_SYS_MELDASC6C64)
                    {
                        lRes[0] = EZNcCom.System_GetAlarm2(3, (int)alarmType.M_ALM_NC_ALARM, out AlarmMsg[0]);
                        lResult = lRes[0];
                        ErrorCheck("GetAlarm");
                        lRes[1] = EZNcCom.System_GetAlarm2(3, (int)alarmType.M_ALM_PLC_ALARM, out AlarmMsg[1]);

                        lResult = lRes[1];
                        ErrorCheck("GetAlarm");
                        lRes[2] = EZNcCom.System_GetAlarm2(3, (int)alarmType.M_ALM_OPE_MSG, out AlarmMsg[2]);
                        lResult = lRes[2];
                        ErrorCheck("GetAlarm");

                        if (lRes[0] + lRes[1] + lRes[2] == 0)
                        {
                            for (int K = 0; K < 3; K++)
                            {
                                if (!string.IsNullOrEmpty(AlarmMsg[K]))
                                {

                                }
                            }
                        }
                    }

                    if (lSystemType == (int)sysType.EZNC_SYS_MAGICBOARD64 || lSystemType == (int)sysType.EZNC_SYS_MELDAS700M ||
                            lSystemType == (int)sysType.EZNC_SYS_MELDAS700L || lSystemType == (int)sysType.EZNC_SYS_MELDAS800M ||
                            lSystemType == (int)sysType.EZNC_SYS_MELDAS800L || lSystemType == (int)sysType.EZNC_SYS_MELDAS6X5L)
                    {
                        lResult = EZNcCom.System_GetAlarm2(10, (int)alarmType.M_ALM_ALL_ALARM, out AlarmMsg[0]);
                        ErrorCheck("GetAlarm");
                    }
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// 获取自动运行时间
        /// /plTime：返回自动运行开始的总处理时间（小时，分钟，秒），使用内存（磁带）或MDI模式到M02 / M30的终止或复位操作。
        ///值：0到99995959
        ///输出示例：9999：59：59 = 99995959
        ///从存储器（磁带）开始的自动运行开始或MDI模式到M02 / M30的终止或复位操作获取总处理时间（小时，分钟，秒）。当值达到最大值时停止积分，并保持最大值。
        /// </summary>
        public void GetRunTime(out int plTime)
        {
            plTime = 0;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Time_GetRunTime(out plTime);
                    ErrorCheck("Time_GetRunTime");
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// 读取PLC参数
        /// </summary>
        public void GetPLCPar(int lindex,out int value)
        {
            value = 0;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Parameter_GetPlcParameter(lindex, out value);
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// 读取程序号
        ///lProgramType：设置程序类型。
        ///EZNC_MAINPRG::主程序//EZNC_SUBPRG子程序
        ///ppwszProgramNo：返回已完成搜索的程序的编号.目前以UNICODE字符串自动运行。 程序编号作为M700 / M800系列的程序文件名。
        ///自动返回已完成搜索或当前搜索的程序的编号
        /// </summary>
        /// <param name="szPrgNo"></param>
        public void GetProgramNumber(out string PrgNo)
        {
            string szPrgNo;
            int szPrg;
            PrgNo=null ;
            try
            {
                if (EZNcCom != null)
                {
                    if (lSystemType == (int)sysType.EZNC_SYS_MELDAS700M || lSystemType == (int)sysType.EZNC_SYS_MELDAS700L || lSystemType == (int)sysType.EZNC_SYS_MELDAS800L)
                    {
                        lResult = EZNcCom.Program_GetProgramNumber2((int)progType.EZNC_MAINPRG, out szPrgNo);
                        ErrorCheck("GetProgramNumber2");
                        if (lResult == 0)
                        {
                            PrgNo = szPrgNo;
                        }
                    }
                    else
                    {
                        lResult = EZNcCom.Program_GetProgramNumber((int)progType.EZNC_MAINPRG, out szPrg);
                        ErrorCheck("GetProgramNumber");
                        if (lResult == 0)
                        {
                            PrgNo = szPrg.ToString ();
                        }
                    }
                }
                else
                {
                    ErrorCheck("获取GetProgramNumber2失败");
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// 程序序列号取得
        /// </summary>
        public void GetSequenceNo(out int lSequenceNo)
        {
            lSequenceNo = 0;
            int  sequenceNo = 0;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Program_GetSequenceNumber((int)progType.EZNC_MAINPRG, out sequenceNo);
                    ErrorCheck("GetsequenceNumber");
                    if (lResult == 0)
                    {
                        lSequenceNo = sequenceNo;
                    }
                    else
                    {

                        ErrorCheck("获取GetProgramNumber2失败");

                    }
                }
                else
                {
                    ErrorCheck("通讯对象未建立");
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
           
        }

        /// <summary>
        ///获取当前正在运行程序
        ///当前块读出
        ///lBlockNumber：设置要获取的块数。 值：1到10
        ///lppwszProgramData：以UNICODE字符串的形式获取程序块。 分开程序块，CR，LF代码插入它们之间。 另外，在NULL处插入NULL结束。
        ///plCurrentBlockNo：返回得到的块中正在执行的块号。
        ///0未运行
        ///1第一块
        ///2第二块
        ///获取已经完成操作搜索的程序或正在执行的程序。 读取正在运行的程序段或者运行中的程序段。
        /// </summary>
        public void GetProgram(out string PrgData)
        {
            string szPrgData = null;
            PrgData = "null";
            int lCurrentBlkNo;
            
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Program_CurrentBlockRead(10, out szPrgData, out lCurrentBlkNo);
                    ErrorCheck("CurrentBlockRead");
                   
                    if (lResult == 0)
                    {
                        PrgData = szPrgData;
                    }
                    else
                    {

                        ErrorCheck("获取GetProgramNumber2失败");

                    }
                }
                else
                {
                    ErrorCheck("通讯对象未建立");
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// 获取刀具偏置组数
        /// 返回设定零件系统的刀具偏置组数。组数由NC规格决定。值含义：200 = 200 [sets]
        /// </summary>
        public void GetToolSetSize(out int toolSetSize)
        {
            int toolCount;
            toolSetSize = 0;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Tool_GetToolSetSize(out toolCount);
                    ErrorCheck("Tool_GetToolSetSize");

                    if (lResult == 0)
                    {
                        toolSetSize = toolCount;
                    }
                    else
                    {

                        ErrorCheck("Tool_GetToolSetSize失败");

                    }
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }
        /// <summary>
        ///获取刀补信息
        ///lType：设置刀具补偿类型。 请参阅参数表。
        ///lKind：设置刀具补偿量的类型。 请参阅参数表。
        ///lToolSetNo：设置刀具偏置组号。可以通过GetToolSetSize（）获取组数。
        ///pdOffset：返回刀具偏置量。 请参阅参数表。
        ///plNo：返回假想的刀尖点数。 请参阅参数表。仅L系统类型。 除L系统类型外，不返回任何内容。
        ///Function:获取设定零件系统/轴号的刀具偏置量。参数表中显示的范围因三菱CNC的英制，公制等指令单位而异。 有关详细信息，请参阅每个三菱CNC的安装手册。
        /// </summary>
        public void GetToolOffSet(int lType,int lKind,int lToolSetNo,out double offSet,out int no)
        {
            offSet = -1;
            no = -1;
            double  pdOffSet;
            int plNo;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Tool_GetOffset(lType, lKind, lToolSetNo, out  pdOffSet, out  plNo);
                    ErrorCheck("GetOffset");
                    if (lResult == 0)
                    {
                        offSet = pdOffSet;
                        no = plNo;
                    }
                    else
                    {

                        ErrorCheck("获取GetOffset失败");

                    }
                }
                else
                {
                    ErrorCheck("通讯对象未建立");
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }

        }

        #region 文件相关

        /// <summary>
        /// 驱动信息取得
        ///lppwszDriveInfo：以UNICODE字符串形式获取驱动器信息。
        ///驱动器信息的格式如下所示：
        ///驱动器名称：CRLFDrive名称：CRLF ...驱动器名称：CRLF \ 0
        ///要分隔驱动器名称，将在它们之间插入CR，LF代码。 数据的结尾
        ///成为CR，LF代码和NULL代码。 数据的末尾变成NULL代码。
        /// </summary>
        public void GetDriveInfomation(out string driveInfo)
        {
            string lppwszDriveInfo;
            driveInfo = null;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.File_GetDriveInformation(out lppwszDriveInfo);
                    if (lResult != 0)
                    {
                        driveInfo = lppwszDriveInfo;
                    }
                    else
                    {

                        ErrorCheck(" When a drive does not exist");

                    }
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// 目录检索
        /// lpcwszDirectryName：将目录名称设置为UNICODE字符串。
        ///使用绝对路径指定目录，如下所示：
        ///驱动器名称+“：”+ \目录名称\文件名称...    获取设置的文件名称信息。 （注1）
        ///驱动器名称+“：”+“\ Directory name”...   获取设置的目录名称信息。 （注1）
        ///驱动器名称+“：”+ \目录名称\ ...           获取设置的目录信息。
        ///（注1）该设置适用于M700 / M800系列。
        ///FileType：设置要读取的数据的类型和格式。以下内容也可以用管道（|）设置。 当设置NULL时，读取文件信息。
        ///值含义
        ////EZNC_DISK_DIRTYPE   读取目录信息
        ///EZNC_DISK_COMMENT    读取的注释信息（仅在NC控制单元一侧）
        ///EZNC_DISK_DATE       读取日期信息（仅在个人计算机端）
        ///EZNC_DISK_SIZE       读取大小信息
        ///lppwszFileInfo：以UNICODE字符串的形式获取文件信息。
        ///文件信息的格式如下所示：文件名\大小\日期\评论\ 0.一个TAB代码插入文件名，大小，日期和评论之间。数据的末尾变成NULL代码。
        ///plRet：返回是否读取文件信息或返回错误代码。 （在自动化时，使用返回值。）
        ///0：当没有文件信息时
        ///1或更多：当存在文件信息时
        ///EZNC_FILE_DIR_DATASIZE：超出了最大数据大小
        ///EZNC_FILE_DIR_NOTOPEN：未打开
        ///EZNC_FILE_DIR_READ：文件信息读取错误
        ///EZNC_FILE_DIR_ALREADYOPENED：不同的目录已经打开
        ///EZNC_FILE_DIR_NODRIVE：驱动器不存在
        ///EZNC_FILE_DIR_NODIR：目录不存在
        ///（注2）如果个人计算机发生错误，错误代码EZNC_FILE_ ...将变为EZNC_PCFILE_...
        ///Function 说明：
        ///搜索目录。
        ///在FindDir2（）中，可以通过读取一次来读取一个文件的信息。要不断获取目录信息，可以通过重复调用FindNextDir2（）来获取设置目录中的文件名列表。要存储在由lpszFileInfo指示的区域中的文件信息的格式如下：
        ///文件名\ tSize \ tDate \ tComment \ 0
        ///TAB代码插入文件名称，大小，日期和注释之间。数据的末尾变成NULL代码。仅存储在读取类型中设置的文件名后面的信息。例如，如果设置了“EZNC_DISK_COMMENT | EZNC_DISK_DATE”，则信息如下：
        ///文件名\ tDate \ tComment \ 0
        ///如果为注释不能注释的文件设置了“EZNC_DISK_SIZE | EZNC_DISK_COMMENT”
        ///被添加，注释信息将不会被输出，注释将如下所示：文件名\ tSize \ t \ 0
        ///对于日期无法获取的文件，设置“EZNC_DISK_SIZE | EZNC_DISK_DATE | EZNC_DISK_COMMENT“变成如下，没有日期信息输出。文件名\ tSize \ t \ tComment \ 0
        ///*日期无法获得的文件是指NC控制单元上的文件侧。
        ///由于字符串区域内存在本产品中分配，使用VC ++的客户端需要
        ///用CoTaskMemFree（）显式释放字符串区域内存。
        ///（注1）读取NC侧紧凑型闪存（M700系列）的目录大小信息不支持SD卡（M800系列）。读取的目录大小信息无效。
        ///（注2）对于C70，指定个人计算机上的文件并指定0lFileType，\ t被添加到文件信息的结尾（文件名\ t \ 0）。要使用获取的文件信息，请在使用前删除\ t。
        ///说明：
        ///当使用FindDir2（）时，在执行ResetDir（）之前，不能执行FindDir2（），OpenFile3（），OpenNcFile2（）。
        ///执行时，出现“EZNC_FILE_DIR_ALREADYOPENED（0x80030101）不同目录已经打开”的错误。 使用它时，在执行FindDir2（）后立即执行ResetDir（）。
        /// </summary>
        public void GetFindDir(string item,out string  dirInfo)
        {
            item = null;
            dirInfo = null;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.File_FindDir2(item, (int)fileDir.EZNC_DISK_SIZE, out dirInfo);
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }

           // lResult = EZNcCom.File_FindNextDir( out sss);
        }

        /// <summary>
        /// 目录检索结束
        /// 终止目录搜索。要再次搜索目录，请执行FindDir2（）。
        /// </summary>
        public void ResertDir()
        {
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.File_ResetDir();
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }

        }

        /// <summary>
        /// 空余容量取得
        /// 单位bit
        /// </summary>
        public void GetDriveSize(string path, out int driveInfo)
        {
         int  lppwszDriveInfo;
         driveInfo = 0;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.File_GetDriveSize(path, out lppwszDriveInfo);
                    if (lResult == 0)
                    {
                        driveInfo = lppwszDriveInfo;
                    }
                    else
                    {

                        ErrorCheck("GetDriveInformation失败");

                    }
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// 开放式加工程序专用文件
        /// 打开NC程序
        /// lpcwszFileName：将包含路径的文件名设置为UNICODE字符串。
        /// lMode：设置打开模式。
        /// 以下路径以外的路径不能使用。
        /// M700系列
        /// M01：\ PRG \ USER \加工程序编号
        ///M01：\ PRG \ UMACRO \加工程序编号
        ///M01：\ PRG \ MMACRO \加工程序编号
        ///M01：\ PRG \ FIX \加工程序编号
        ///M01：\ PRG \ MDI \加工程序编号
        ///M800系列
        ///M01：\ PRG \ USER \加工程序名称（32或更少的字母数字字符包括扩展名）
        ///M01：\ PRG \ MMACRO \加工程序号（100010000-199999999）
        ///M01：\ PRG \ FIX \加工程序号（100000010 -100009999）
        ///M01：\ PRG\ MDI\ MDI.PRG
        ///function
        ///在设定模式下打开加工程序文件。 创建临时文件的目录按以下优先级顺序创建：
        ///使用环境变量TMP指定的目录
        ///安装产品的目录
        ///临时文件名称是“MELDASn”。 一个数字被放置在n中。
        ///OpenFile3（）不能同时使用。 C70不支持此功能。 （EZ_ERR_NOT_SUPPORT返回到plRet。）
        ///（注1）确保使用CloseNCFile2（）（或AbortNCFile2（））关闭打开的文件。 如果CloseNCFile2（）未使用，临时文件将保留。
        ///（注2）对于M700 / M800系列，在NC控制单元自动运行期间，可以执行写入或覆盖操作，除非正在执行用于操作的文件的自动操作。 读取操作可以在NC控制单元的自动操作期间执行。
        /// </summary>
        public void OpenNCFile(string fileName)
        {
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.File_OpenNCFile2(fileName, (int)openFileMode.EZNC_FILE_OVERWRITE);
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// Nc程序文件关闭
        /// 关闭加工程序文件。C70不支持此功能。 （EZ_ERR_NOT_SUPPORT返回到plRet。）
        ///（注1）对于M700 / M800系列，除非正在执行用于操作的文件的自动操作，否则可以在NC控制单元的自动操作期间执行操作。
        ///（注2）使用OpenNCFile2（）打开文件时，确保将其关闭CloseNCFile2（）（或AborNCtFile2（））。
        /// </summary>
        public void CloseNCFile()
        {
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.File_CloseNCFile2();
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }
        }

        /// <summary>
        /// 读取NC程序
        /// 长度：以字节数设置一次读取的数据大小。
        /// vData：返回VARIANT中的读取字节数据数组。
        ///数据从用OpenNCFile2（）打开的加工程序文件中读取。 要读取的数据返回一个字节数据数组及其字节数。 当pdwNumRead小于dwLength时，确定文件结束。
        ///设置一次读取的数据大小。 读取大文件时，可以分多个部分读取。 该文件可以按顺序读取，直到执行CloseNCFile2（）。
        ///C70不支持此功能。 （EZ_ERR_NOT_SUPPORT返回到plRet。）
        /// </summary>
        public void ReadNcFile(int lLength,out  string vData,string filename)
        {
            object Data = null;
            vData = null;
            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.File_OpenNCFile2(filename, (int)openFileMode.EZNC_FILE_READ);
                    lResult = EZNcCom.File_ReadNCFile2(lLength, out Data);
                    if (lResult == 0)
                    {
                        byte[] bytes = (byte[])Data;
                        vData = Encoding.UTF8.GetString(bytes);

                        ErrorCheck("ReadNCFile成功");
                    }
                    else
                    {

                        ErrorCheck("ReadNCFile失败");

                    }
                    EZNcCom.File_CloseNCFile2();
                }

                else
                {
                    ErrorCheck("通讯对象未建立");
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }

        }

        /// <summary>
        /// 写入NC程序
        /// 自动化的说法：
       ///vData：创建要写入字节数组的数据，并将其替换为vData（VARIANT类型），如下例所示。
       ///例）Dim vWriteFile As Variant
       ///Dim byteWrite（）As Byte
       ///vWriteFile = byteWrite
       ///function说明
       ///数据被写入用OpenNCFile2（）打开的加工程序文件中。 要写入的数据是字节数组中的数据。
      ///设置一次写入数据的大小。 当写入大量的数据时，它可以被写入多个部分。 数据可以按顺序写入，直到执行CloseNCFile2（）。
      ///C70不支持此功能。 （EZ_ERR_NOT_SUPPORT返回到plRet。）
      ///（注）对于M700 / M800系列，当编辑锁定B（＃8105）参数为1时，不能写入程序8000至9999。 当编辑锁定C（＃1121）参数为1时，程序9000至9999无法写入。
        /// </summary>
        public void WriteNcFile(string filename,string data)
        {
            object vata = data;

            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.File_OpenNCFile2(filename, (int)openFileMode.EZNC_FILE_OVERWRITE);
                    lResult = EZNcCom.File_WriteNCFile(vata);
                    EZNcCom.File_CloseNCFile2();
                }
            }
            catch
            {
                ErrorCheck("通讯已关闭");
            }

        }

        #endregion


        public void SetPrograme()
        {

            //try
            //{
                string targetPath = "T1.NC";

                lResult = EZNcCom.Operation_Search(targetPath,0,0);
              //  EZNcCom.File_OpenNCFile2(targetPath, (int)openFileMode.EZNC_FILE_OVERWRITE);
                EZNcCom.Operation_Run();
                
                // OpenNCFile(targetPath);
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.File_CloseNCFile2();
                }

            //}
            //catch(Exception ex)
            //{
                
            //}


            //return true;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// ///M800系列
        ///M01：\ PRG \ USER \加工程序名称（32或更少的字母数字字符包括扩展名）
        ///M01：\ PRG \ MMACRO \加工程序号（100010000-199999999）
        ///M01：\ PRG \ FIX \加工程序号（100000010 -100009999）
        ///M01：\ PRG\ MDI\ MDI.PRG
        public bool UpdateNCPrgToCNC(string filename)
        {
            string ErrorMsg = "";
            int ErrorCode;
            try
            {
                if (filename == "")
                {
                    ErrorMsg = "程序名不能为空.";
                    ErrorCode = -1;
                    return false;
                }

                if (!File.Exists(filename))
                {
                    ErrorMsg = "指定程序不存在.";
                    ErrorCode = -1;
                    return false;
                };

                //if (!isConnectCNC)
                //{
                //    ReConnected();
                //}
               // ReConnected();
                //Data update cnc program from PC to cnc
                //***************************************************************************
                //File_Copy2(lpcwszSrcFileName, lpcwszDstFileName)
                //lResult = dispEZNc_Com.File_Copy2("E:\\TEMP\\9005", "M01:\\PRG\\USER\\9005");
                int index = filename.LastIndexOf("\\") + 1;
                string ncfileName = filename.Substring(index, (filename.Length - index));
                string targetPath = "M01:\\PRG\\USER\\" + ncfileName;
                object content = "";

                //int nRet = EZNcCom.File_Copy2(targetPath, "d:\\test123.NC");

                //targetPath = "M01:\\PRG\\USER\\TEST.NC";
                //string line;
                //StringBuilder sb = new StringBuilder();
                //// 创建一个 StreamReader 的实例来读取文件 ,using 语句也能关闭 StreamReader
                //using (System.IO.StreamReader sr = new System.IO.StreamReader(filename))
                //{
                //    // 从文件读取并显示行，直到文件的末尾 
                //    while ((line = sr.ReadLine()) != null)
                //    {
                //        //Console.WriteLine(line);
                //        sb.Append(line);
                //    }
                //}

                //line = sb.ToString();
                //EZNcCom.File_OpenNCFile2(targetPath, (int)openFileMode.EZNC_FILE_OVERWRITE);
                //EZNcCom.File_WriteNCFile(line);
                //EZNcCom.File_CloseNCFile2();
                EZNcCom.File_Delete2(targetPath);

                int nRet = EZNcCom.File_Copy2(filename, targetPath);

                if (nRet == 0)
                {
                    ErrorMsg = "";
                    ErrorCode = 0;
                    return true;
                }
                else
                {
                    long retCode = (long)nRet;
                    //switch (retCode)
                    //{
                    //    case EZNcCom.EZNC_FILE_COPY_BUSY:
                    //        ErrorMsg = "Copy is disabled (during operation)";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_ENTRYOVER:
                    //        ErrorMsg = "Registration limit exceeded";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_FILEEXIST:
                    //        ErrorMsg = "Copy destination file already exists";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_FILESYSTEM:
                    //        ErrorMsg = "File system error";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_ILLEGALNAME:
                    //        ErrorMsg = "Invalid file name format";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_MEMORYOVER:
                    //        ErrorMsg = "Memory capacity exceeded";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_NODIR:
                    //        ErrorMsg = "Directory does not exist";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_NODRIVE:
                    //        ErrorMsg = "Drive does not exist";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_NOFILE:
                    //        ErrorMsg = "File does not exist";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_PLCRUN:
                    //        ErrorMsg = "Copy is disabled (programmable controller in operation)";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_READ:
                    //        ErrorMsg = "Transfer source file is not readable";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_WRITE:
                    //        ErrorMsg = "Transfer destination file is not writable";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_FILE_COPY_PROTECT:
                    //        ErrorMsg = "Copying is disabled (protected)";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_PCFILE_COPY_CREATE:
                    //        ErrorMsg = "File cannot be created (PC only)";
                    //        ErrorCode = nRet;
                    //        break;
                    //    case EZNcErr.EZNC_PCFILE_COPY_OPEN:
                    //        ErrorMsg = "File cannot be opened (personal computer only)";
                    //        ErrorCode = nRet;
                    //        break;
                    //    default:
                    //        ErrorMsg = "UpdateNCPrgToCNC Error";
                    //        ErrorCode = -1;
                    //        break;
                    //}
                    return false;
                }
            }
            catch (Exception ex)
            {
 
                ErrorMsg = ex.Message + ex.StackTrace;
                return false;
            }
            finally
            {
              //  FreeConnected();
            }
        }

        /// <summary>
        /// 错误代码提示
        /// </summary>
        /// <param name="szMethod"></param>
        private void ErrorCheck(string szMethod)
        {
            if (EZNcCom != null)
            {
                if (lResult != 0)
                {
                    szMessage = szMethod + " Failed. " + "0x" + Convert.ToString(lResult, 16) + szMessage + "\r\n";

                }
                else
                {
                    szMessage = szMethod + "通讯正常";
                }
            }
            else
            {
                szMessage = szMethod;
            }
            //发布事件
            if (AlarmData != null&&szMessage!=""&&szMessage!=null)
            {
                AlarmData(szMessage);
            }
            szMessage = "";

        }






        public  int ReadDeviceY(string addr, out int value)
        {
            string[] devices = new string[1];
            devices[0] = addr;

            // 数据类型数组
            int[] dataTypes = new int[1];
            dataTypes[0] = 0x11;  // EZNC_PLC_BYTE 

            // 值数组
            int[] values = new int[1];
            values[0] = 0;

            try
            {
                if (EZNcCom != null)
                {
                    lResult = EZNcCom.Device_SetDevice(devices, dataTypes, values);
                    ErrorCheck("SetDevice");
                    if (lResult == 0)
                    {
                        object readData;
                        lResult = EZNcCom.Device_Read(out readData);
                        if (lResult == 0)
                        {
                            if (readData == null)
                            {
                                value = 0;
                                return -1;
                            }

                            if (readData is int[])
                            {
                                int[] arr = readData as int[];
                                value = arr != null && arr.Length > 0 ? arr[0] : 0;

                                return 0;
                            }

                            value = 0;
                            return -1;
                        }
                    }
                }
            }
            catch
            {

            }

            value = 0;
            return -1;
        }

















    }
}
