using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace M80Scada
{
    public partial class ServerSetting : Form
    {
        public static string _configPath = Application.StartupPath + @"\Config\ShinewaySCADA.config";
        MyFunction mf=new MyFunction();
     
        public ServerSetting()
        {
            InitializeComponent();
            LoadConfig();
        }
        public void LoadConfig() 
        {
            mf.CreateFlie(_configPath);
            string[] ServerArr = mf.ReadFile(_configPath,4);
            textBox1.Text = ServerArr[0];
            textBox2.Text = ServerArr[1];
            textBox3.Text = mf.encodeText(ServerArr[2]);
            textBox4.Text = ServerArr[3];
        }
        public void SevaConfig() 
        {
            //只对密码加密
            string str1 = textBox1.Text.Trim();
            string str2 = textBox2.Text.Trim();
            string str3 = mf.decodeText(textBox3.Text.Trim());
            string str4 = textBox4.Text.Trim();
            string[] ServerArr = new string[4] { str1, str2, str3, str4 };
            bool result = mf.WriteFlie(_configPath, ServerArr);
            if (result)
            {
                var connectionString = new MySqlConnectionStringBuilder
                {
                    Server = str1,
                    Port = 3306,
                    UserID = str2,
                    Password = mf.encodeText(str3),
                    Database = str4,
                    CharacterSet = "utf8mb4", // 解决乱码，支持emoji
                    SslMode = MySqlSslMode.None, // 本地测试关闭SSL，生产环境根据实际设置为Required/VerifyCA
                    ConnectionTimeout = 30,
                    AllowPublicKeyRetrieval = true // 可选：如果遇到公钥检索错误时启用
                }.ToString();
                
                GlobalCommon.ConnStr = connectionString;

                MessageBox.Show("保存成功");
            }
            else
            {
                MessageBox.Show("保存 ShinewaySCADA.comfig 时错误！");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SevaConfig();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
