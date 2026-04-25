using EZSockets;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace M80Scada
{
    public partial class Form1 : Form
    {
        MyFunction mf = new MyFunction();        
        SqlFunction sf = new SqlFunction();       
        public static string _ScadaNO;

        public Form1()
        {
            InitializeComponent();
            mf.LoadConstr();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1_Click(null, null);
        }

       
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Thread thread = new Thread(ThreadSlc);
            thread.IsBackground = true;
            thread.Start();
        }      

        public List<MyMachine> SlcMachine(string _ScadaNO)
        {           
            List<MyMachine> list = new List<MyMachine>();
            string sql = "select MachineID,MachineNO,IpAddr,PortNum,tempOneToMany,tempItem from t_Machine where Status=1 and Stype='M80' order by OrderBy";

            DataTable dt = sf.slc_of_sql(sql, GlobalCommon.ConnStr);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MyMachine mm = new MyMachine();
                    //mm.No=((i + 1) / 10).ToString();//分组
                    mm.No = i.ToString(); //不分组   
                    mm.MachineID = Convert.ToInt32(dt.Rows[i]["MachineID"]);
                    mm.MachineNO = dt.Rows[i]["MachineNO"].ToString();
                    mm.IpAddr = dt.Rows[i]["IpAddr"].ToString();

                    mm.PortNum = 683; 
                    try { if (dt.Rows[i]["PortNum"] != null && dt.Rows[i]["PortNum"].ToString() != "") { mm.PortNum =Convert.ToInt32(dt.Rows[i]["PortNum"]); } }
                    catch { }

                    mm.tempOneToMany = Convert.ToInt32(dt.Rows[i]["tempOneToMany"]);
                    mm.tempItem = dt.Rows[i]["tempItem"].ToString();
                    mm.ScadaNO = _ScadaNO;
                    list.Add(mm);
                }
            }
              
            return list;
        }
        private  void ThreadSlc()
        {
            while (true)
            {
                DateTime now = System.DateTime.Now;
                string str1 = now.ToString("yyyyMMddHHmm");               
                if (_ScadaNO != str1)
                {
                    _ScadaNO = str1;


                    List<MyMachine> list = SlcMachine(_ScadaNO);

                    this.Invoke((Action)(() =>
                    {
                        textBox1.Text = string.Format("每一分钟采集一次，当前正常采集：{0} 共计{1}台", _ScadaNO, list.Count);
                    }));


                    foreach (var MyMachine in list)
                    {
                        string sql = "insert into t_scadadata(MachineID,ScadaNO,WkcntrNum,WkcntrCount,OneToMany,WkcntrSum,LedStatus,ItemString) values  ";

                        int checkCount = 0;
                        bool checkIP = false;
                        while (!checkIP && checkCount < 3) { checkIP = mf.CheckPingIP(MyMachine.IpAddr); checkCount++; }
                        string MachineID = MyMachine.MachineID.ToString();
                        string ScadaNO = MyMachine.ScadaNO;
                        int WkcntrNum = -1;
                        int WkcntrCount = 0;
                        int OneToMany = 1;
                        int WkcntrSum = 0;
                        int LedStatus = -1;
                        string ItemString = "";
                        if (checkIP) 
                        {
                            EZSockets.MitCom MyMitCom = new MitCom();
                            MyMitCom.GetSimConnect("M800M", "1", "10", MyMachine.IpAddr); //连接/断开
                            Thread.Sleep(10);

                            //-----三色灯--------------------------------------
                            int Y40 = -1;
                            int Y41 = -1;
                            int Y42 = -1;
                            MyMitCom.ReadDeviceY("Y41", out Y41);
                            MyMitCom.ReadDeviceY("Y40", out Y40);
                            MyMitCom.ReadDeviceY("Y42", out Y42);
                            if (Y40 == 0 && Y41 == 1 && Y42 == 0) { LedStatus = 1; }//绿灯 0 1 0
                            if (Y40 == 1 && Y41 == 1 && Y42 == 0) { LedStatus = 2; }//黄灯 1 1 0
                            if (Y40 == 0 && Y41 == 0 && Y42 == 1) { LedStatus = 2; }//黄灯 0 0 1
                            if (Y40 == 1 && Y41 == 0 && Y42 == 0) { LedStatus = 3; }//红灯 1 0 0                            

                            if (LedStatus>0) 
                            {
                                string str = "断点专用";
                            }


                            //-----工件计数--------------------------------------
                            string M80count = "";
                            MyMitCom.GetParaValue(30, 8002, 1, 1, out M80count);
                            try { if (M80count != "") { WkcntrNum = Convert.ToInt32(M80count); } }
                            catch { }

                            //-----产量小计--------------------------------------
                            WkcntrCount = GetWkcntrCount(WkcntrNum, MachineID, MyMachine.ScadaNO);
                            WkcntrSum = WkcntrCount * OneToMany;

                            MyMitCom.GetSimConnect("M800M", "1", "10", MyMachine.IpAddr); //连接/断开
                        }

                        //IP通不通都需要记录采集结果 
                        sql += string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", MachineID, ScadaNO, WkcntrNum, WkcntrCount, OneToMany, WkcntrSum, LedStatus, ItemString);
                        sf.insert_of_sql(sql, GlobalCommon.ConnStr);
                    }                                        

                }
               
            }           
        }

        private void ThreadSCADA(MyMachine machine)
        {


            bool CkIp = false;
            for (int i = 0; i < 3; i++)
            {
                CkIp = mf.CheckPingIP(machine.IpAddr);
                if (CkIp)
                {
                    break;
                }
            }
            //----------------------         

            int WkcntrNum = -1;
            int WkcntrCount = 0;
            int LedStatus = -1;
            string ItemString = machine.tempItem;
            string MachineID = machine.MachineID.ToString();
            string ScadaNO = machine.ScadaNO;
            int OneToMany = machine.tempOneToMany;
            int WkcntrSum = 0;
            if (CkIp)
            {
                if (machine.IpAddr == "192.168.16.97")
                {
                    string str= "断点专用";
                }

                EZSockets.MitCom MyMitCom = new MitCom();
                MyMitCom.GetSimConnect("M800M", "1", "10", machine.IpAddr); //连接/断开

                Thread.Sleep(10);

                int Y40 = -1;
                int Y41 = -1;
                int Y42 = -1;

                MyMitCom.ReadDeviceY("Y41", out Y41);
                MyMitCom.ReadDeviceY("Y40", out Y40);
                MyMitCom.ReadDeviceY("Y42", out Y42);
                if (Y40 == 0 && Y41 == 1 && Y42 == 0) { LedStatus = 1; }//绿灯 0 1 0
                if (Y40 == 1 && Y41 == 1 && Y42 == 0) { LedStatus = 2; }//黄灯 1 1 0
                if (Y40 == 0 && Y41 == 0 && Y42 == 1) { LedStatus = 2; }//黄灯 0 0 1
                if (Y40 == 1 && Y41 == 0 && Y42 == 0) { LedStatus = 3; }//红灯 1 0 0

                if (machine.IpAddr == "192.168.6.55")
                {
                    string debugmsg = Y40 + "--" + Y41 + "--" + Y42;
                }

                string M80count = "";
                MyMitCom.GetParaValue(30, 8002, 1, 1, out M80count);

                

               

                try { if (M80count != "") { WkcntrNum = Convert.ToInt32(M80count); } }
                catch { }


                WkcntrCount = GetWkcntrCount(WkcntrNum, MachineID, machine.ScadaNO);
                WkcntrSum = WkcntrCount * OneToMany;


                MyMitCom.GetSimConnect("M800M", "1", "10", machine.IpAddr); //连接/断开



                string sqlInsert = "insert into t_scadadata(MachineID,ScadaNO,WkcntrNum,WkcntrCount,OneToMany,WkcntrSum,LedStatus,ItemString) values  ";

                sqlInsert += string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", MachineID, ScadaNO, WkcntrNum, WkcntrCount, OneToMany, WkcntrSum, LedStatus, ItemString);
                int count1 = sf.insert_of_sql(sqlInsert, GlobalCommon.ConnStr);



            }


        


            // int thisthread = Thread.CurrentThread.ManagedThreadId;
            // MyStr += "线程"+thisthread+":"+count + "\r\n";
            // Thread.CurrentThread.Abort(); 停止会遗漏
        }

        private void UpdAgant(int WkcntrNum, int WkcntrCount,int LedStatus, string MachineID, string ScadaNO)
        {
            string sqlUpdate = string.Format("update t_scadadata set WkcntrNum={0} ,WkcntrCount={1},LedStatus={2} where MachineID={3} and ScadaNO='{4}'", WkcntrNum, WkcntrCount, LedStatus,MachineID, ScadaNO);
            sf.update_of_sql(sqlUpdate, GlobalCommon.ConnStr);           
        }





        private bool IsScadaNO(string MachineID, string ScadaNO)
        {
            bool res = false;
            string sql = string.Format("select * from t_scadadata where MachineID={0},ScadaNO='{1}'", MachineID, ScadaNO);
            DataTable dt = sf.slc_of_sql(sql, GlobalCommon.ConnStr);
            if (dt != null && dt.Rows.Count > 0) { res = true; }
            return res;
        }


        private int GetWkcntrCount(int thisWkcntrNum ,string MachineID, string ScadaNO)
        {
            int result = 0;
            //2023 11 14   17 36
            string year = ScadaNO.Substring(0,4);
            string month = ScadaNO.Substring(4,2);
            string day = ScadaNO.Substring(6,2);
            string hour = ScadaNO.Substring(8,2);
            string minute = ScadaNO.Substring(10,2);
            string lastScadaNO = Convert.ToDateTime(year+"-"+ month+"-"+ day+" "+ hour+":"+ minute+":00").AddMinutes(-1).ToString("yyyyMMddHHmm");

            string sql = "select WkcntrNum from t_scadadata where ";
            sql += string.Format(" MachineID={0} and ScadaNO='{1}'", MachineID, lastScadaNO);

            DataTable dt = sf.slc_of_sql(sql,GlobalCommon.ConnStr);
            if(dt!=null&&dt.Rows.Count>0)
            {
                int lastWkcntrNum = Convert.ToInt32(dt.Rows[0][0]);
                if (thisWkcntrNum>0&&lastWkcntrNum > 0)
                {
                    int sum = thisWkcntrNum - lastWkcntrNum;
                    if (sum > 0)
                    {
                        result = sum;
                    }
                }
            }

            return result;
        }











        private void 服务器设置SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServerSetting ss = new ServerSetting();
            ss.ShowDialog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void 强制退出EToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
            this.Close();
        }
    }
}




