using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M80Scada
{
    public class SqlFunction
    {

        public string GetConnStr()
        {
            var connectionString = new MySqlConnectionStringBuilder
            {
                Server = "localhost",
                Port = 3306,
                UserID = "root",
                Password = "Aa111111",
                Database = "db_scadabrother",
                CharacterSet = "utf8mb4", // 解决乱码，支持emoji
                SslMode = MySqlSslMode.None, // 本地测试关闭SSL，生产环境根据实际设置为Required/VerifyCA
                ConnectionTimeout = 30,
                AllowPublicKeyRetrieval = true // 可选：如果遇到公钥检索错误时启用
            }.ToString();

            return connectionString;
        }


        //增删查改-新增
        public int insert_of_sql(string sql, string connstr)
        {
            int result = -1;
            using (var conn = new MySqlConnection(connstr))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                }
                catch { }
                finally { conn.Close(); }
            }
            return result;
        }
        //增删查改-删除
        public int del_of_sql(string sql, string connstr)
        {
            int result = -1;
            using (var conn = new MySqlConnection(connstr))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    result = cmd.ExecuteNonQuery();
                }
                catch { }
                finally { conn.Close(); }
            }
            return result;
        }
        //增删查改-查询
        public DataTable slc_of_sql(string sql, string connstr)
        {
            DataTable result = new DataTable();
            using (var conn = new MySqlConnection(connstr))
            {
                try
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    DataSet ds = new DataSet();
                    da.Fill(ds, "slc_of_sql");
                    result = ds.Tables["slc_of_sql"] ?? new DataTable();
                }
                catch { }
                finally { conn.Close(); }
            }
            return result;
        }
        //增删查改-更新
        public int update_of_sql(string sql, string connstr)
        {
            int result = -1;
            using (var conn = new MySqlConnection(connstr))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    result = cmd.ExecuteNonQuery();
                }
                catch { }
                finally { conn.Close(); }
            }
            return result;
        }

    }
}
