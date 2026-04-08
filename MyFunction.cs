using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace M80Scada
{
   
    public class MyFunction
    {
        public static string _configPath = Application.StartupPath + @"\Config\ShinewaySCADA.config";
            


        #region 新版 文件读写函数(创建、写入、读取)

        public bool CreateFlie(string PathFile)
        {
            bool result = true;

            if (!File.Exists(PathFile))
            {
                try
                {
                    // 创建文件夹
                    string folderPath = Path.GetDirectoryName(PathFile);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    // 创建文件
                    File.Create(PathFile).Close();
                }
                catch (Exception ex)
                {
                    result = false;
                    MessageBox.Show("CreateFlie错误：\r\n" + ex.Message);
                }
            }
            return result;
        }

        public bool WriteFlie(string path, string[] strArr)
        {
            bool result = false;
            try
            {
                FileStream stream = new FileStream(path, FileMode.Create);//fileMode指定是读取还是写入
                StreamWriter writer = new StreamWriter(stream);
                int rows = strArr.Length;
                for (int i = 0; i < rows; i++)
                {
                    if ((i + 1) < rows)
                    {
                        writer.WriteLine(strArr[i]);//写入一行，写完后会自动换行
                    }
                    else
                    {
                        writer.Write(strArr[i]);//写完后不会换行
                    }
                }
                writer.Close();//释放内存
                stream.Close();//释放内存
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("WriteFlie错误：\r\n" + ex.Message + "\r\n" + path);
            }

            return result;
        }

        public string[] ReadFile(string path, int rows)
        {
            string[] myStrArr = new string[rows];
            try
            {
                FileStream stream = new FileStream(path, FileMode.Open);
                StreamReader reader = new StreamReader(stream);
                for (int i = 0; i < rows; i++)
                {
                    myStrArr[i] = reader.ReadLine();
                }
                reader.Close();
                stream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ReadFile错误：\r\n" + ex.Message + "\r\n" + path);
            }
            return myStrArr;
        }

        #endregion


        #region 加密解密
        //可逆加密解密
        public string encodeText(string str)
        {
            string htext = "";
            if (str != null)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    htext = htext + (char)(str[i] + 10 - 1 * 2);
                }
            }

            return htext;
        }
        //可逆加密解密
        public string decodeText(string str)
        {
            string dtext = "";
            if (str != null)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    dtext = dtext + (char)(str[i] - 10 + 1 * 2);
                }
            }

            return dtext;
        }

        #endregion

        public void LoadConstr()
        {
            string[] serverArr = ReadFile(_configPath, 4);
            string str1 = serverArr[0];
            string str2 = serverArr[1];
            string str3 = encodeText(serverArr[2]);
            string str4 = serverArr[3];
           
            var connectionString = new MySqlConnectionStringBuilder
            {
                Server = str1,
                Port = 3306,
                UserID = str2,
                Password =str3,
                Database = str4,
                CharacterSet = "utf8mb4", // 解决乱码，支持emoji
                SslMode = MySqlSslMode.None, // 本地测试关闭SSL，生产环境根据实际设置为Required/VerifyCA
                ConnectionTimeout = 30,
                AllowPublicKeyRetrieval = true // 可选：如果遇到公钥检索错误时启用
            }.ToString();

            GlobalCommon.ConnStr = connectionString;
            //Common.ConnStr = "Server=localhost;Port=3306;User ID=root;Password=Aa111111;Database=db_scadabrother;SSL Mode=None;Allow Public Key Retrieval=True;Character Set=utf8mb4;Connection Timeout=30";
        }
        

        public bool CheckPingIP(string ip)
        {
            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(ip, 120);
            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 获取计数
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="prot"></param>
        /// <returns></returns>
        public int GetWkcntrNum(string ip, int prot)
        {
            int Count = -1;//链接异常
            TcpClient client = new TcpClient();
            client.SendTimeout = client.ReceiveTimeout = 2000;
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), prot);
                client.SendTimeout = client.ReceiveTimeout = 2000;
                client.Connect(remoteEP);
                if (client.Connected)
                {
                    var networkStream = client.GetStream();
                    var buffer = Encoding.UTF8.GetBytes("%CLOD    WkcntrNum    \r\n00%");
                    networkStream.Write(buffer, 0, buffer.Length);
                    networkStream.ReadTimeout = 3000;
                    var readBuffer = new byte[1024];
                    var len = networkStream.Read(readBuffer, 0, readBuffer.Length);
                    var result = Encoding.UTF8.GetString(readBuffer, 0, len);
                    networkStream.Close(3000);
                    var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("A01"))
                        {
                            var fields = line.Split(',');
                            Count = Convert.ToInt32(fields[2].Trim());
                            break;
                        }
                    }
                }
                else
                {  
                }
            }
            catch
            {                          
            }
            finally
            {
                if (client.Connected)
                {
                    client.Close();
                }
            }
            return Count;
        }

        public int GetColorStatus(string ip, int prot, string color)
        {
            // -1 通讯错误 1=NO  0=OFF
            int status = -1;           
            TcpClient client = new TcpClient();
            client.SendTimeout = client.ReceiveTimeout = 2000;           
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), prot);
                client.Connect(remoteEP);
                if (client.Connected)
                {
                    var networkStream = client.GetStream();
                    var buffer = Encoding.UTF8.GetBytes("%CIOCREF " + color + "       \r\n00%");
                    networkStream.Write(buffer, 0, buffer.Length);
                    networkStream.ReadTimeout = 3000;
                    var readBuffer = new byte[1024];
                    var len = networkStream.Read(readBuffer, 0, readBuffer.Length);
                    var result = Encoding.UTF8.GetString(readBuffer, 0, len);
                    networkStream.Close(3000);
                    var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    int lines_count = lines.Length;                    
                    if (lines_count > 2)
                    {
                        string statusstr = lines[1].Trim();
                        status = statusstr == "ON" ? 1 : 0;
                    }                   
                }
            }
            catch {}
            finally
            {
                if (client.Connected)
                {
                    client.Close();
                }
            }

            return status;
        }


        private int SetLed(int ColorStatus, string ColorStr)
        {  
            //-1 通讯错误 1=NO  0=OFF
            int result = -1;           
            if (ColorStatus == 1&& ColorStr== "GRN")
            {
                result = 1;
            }
            if (ColorStatus == 1 && ColorStr == "YEL")
            {
                result = 2;
            }
            if (ColorStatus == 1 && ColorStr == "RED")
            {
                result = 3;
            }
            return result;

        }

        //获取LED状态 最终存档的
        public int GetLedStatus(string ip, int prot)
        {
            // GetColorStatus：-1 通讯错误 1=NO  0=OFF
            //-1异常 1绿 2黄 3红 4其他
            int result = -99;
            string[]arr=new string[3] { "GRN", "YEL", "RED" }; //绿灯后不再判断 GRN置前  
            for (int i = 0; i < 3; i++)
            {
                int color = GetColorStatus(ip, prot, arr[i]); //-1 通讯错误 1=NO  0=OFF
                if (color != 0)
                {                    
                    result = SetLed(color, arr[i]);
                    break;
                }
            }
            if (result == -99)
            {
                result = 4;
            }

          

            return result;
        }

        //获取正在运行的程序名称（NC程序名称）
        public string GetNcName(string ip, int prot)
        {
            string str = string.Empty;
            TcpClient client = new TcpClient();
            client.SendTimeout = client.ReceiveTimeout = 2000;
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), prot);
                client.SendTimeout = client.ReceiveTimeout = 2000;
                client.Connect(remoteEP);
                if (client.Connected)
                {
                    var networkStream = client.GetStream();
                    var buffer = Encoding.UTF8.GetBytes("%CREDPRGN          \r\n00%");
                    networkStream.Write(buffer, 0, buffer.Length);
                    networkStream.ReadTimeout = 3000;
                    var readBuffer = new byte[1024];
                    var len = networkStream.Read(readBuffer, 0, readBuffer.Length);
                    var result = Encoding.UTF8.GetString(readBuffer, 0, len);
                    networkStream.Close(3000);
                    var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    if (lines.Length > 1)
                    {
                        string line2str = lines[1].Trim();
                        if (line2str.Length > 7)
                        {
                            str = "O" + line2str.Substring(4, 4);
                        }
                    }
                }
            }
            catch { }
            finally
            {
                if (client.Connected)
                {
                    client.Close();
                }
            }
            return str;
        }
        //获取正在运行的程序内容（NC程序内容）
        public string GetNcText(string ip, int prot, string NcName)
        {
            string str = string.Empty;
            TcpClient client = new TcpClient();
            client.SendTimeout = client.ReceiveTimeout = 2000;
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), prot);
                client.SendTimeout = client.ReceiveTimeout = 2000;
                client.Connect(remoteEP);
                if (client.Connected)
                {
                    var networkStream = client.GetStream();
                    var buffer = Encoding.UTF8.GetBytes("%CLOD    " + NcName + "     \r\n00%");
                    networkStream.Write(buffer, 0, buffer.Length);
                    networkStream.ReadTimeout = 3000;
                    var readBuffer = new byte[1024];
                    var len = networkStream.Read(readBuffer, 0, readBuffer.Length);
                    //var result = Encoding.UTF8.GetString(readBuffer, 0, len);
                    var result = Encoding.Default.GetString(readBuffer, 0, len);
                    networkStream.Close(3000);
                    var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        str += line;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (client.Connected)
                {
                    client.Close();
                }
            }
            return str;
        }

        public string GetSubstring(string str, string sstr, string estr)
        {
            int subindex1 = str.IndexOf(sstr);//查找第一个字符的位置
            int sublength = 0;
            if (subindex1 != -1) //第一个字符存在
            {
                int subindex2 = str.IndexOf(estr, (subindex1 + (sstr.Length)));//查找第二个字符的位置，需从第一个字符的位置开始查找
                if (subindex2 != -1)
                {
                    sublength = subindex2 - subindex1 - sstr.Length;
                    return str.Substring(subindex1 + sstr.Length, sublength);
                }
                else { return null; }
            }
            else { return null; }
        }







    }
}
