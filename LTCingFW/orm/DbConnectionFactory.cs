using log4net;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class DbConnectionFactory
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DbConnectionFactory));
        /// <summary>
        /// 获得Oracle或者SqlServer连接
        /// </summary>
        /// <param name="dbAlias">oracle或者sqlserver</param>
        /// <returns>DbConnection</returns>
        public static DbConnection GetConnectionFromPool(String dbAlias) {
            try
            {
                DbConnectionStringBuilder info = LTCingFWSet.dbDic[dbAlias];
                //ORACLE
                if (info is OracleConnectionStringBuilder) {
                    OracleConnection conn = new OracleConnection();
                    conn.ConnectionString = ((OracleConnectionStringBuilder)info).ConnectionString;
                    conn.Open();
                    return conn;
                }
                //SQLSERVER
                if (info is SqlConnectionStringBuilder)
                {
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = ((SqlConnectionStringBuilder)info).ConnectionString;
                    conn.Open();
                    return conn;
                }
                //MYSQL未实现
                if (info is MySqlConnectionStringBuilder)
                {
                    MySqlConnection conn = new MySqlConnection();
                    conn.ConnectionString = ((MySqlConnectionStringBuilder)info).ConnectionString;
                    conn.Open();
                    return conn;
                }

            }
            catch (Exception e)
            {
                //logger.Warn(dbAlias+"连接异常："+e.Message);
                throw e;
            }
            return null;
        }
        /// <summary>
        /// 获取Oracle或者SqlServer的数据适配器
        /// </summary>
        /// <param name="conn">连接</param>
        /// <param name="sql">sql语句</param>
        /// <returns>DbDataAdapter</returns>
        public static DbDataAdapter GetDataAdapter(DbConnection conn,String sql) {
            if (conn is OracleConnection) {
                OracleDataAdapter adapter = new OracleDataAdapter(sql, (OracleConnection)conn);
                return adapter;
            }
            else if (conn is SqlConnection) {
                SqlDataAdapter adapter = new SqlDataAdapter(sql, (SqlConnection)conn);
                return adapter;
            }
            else if (conn is MySqlConnection)
            {
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, (MySqlConnection)conn);
                return adapter;
            }
            else
            {
                throw new LTCingFWException("不支持的数据适配器，不支持该数据库类型！");
            }
        }


        //私有化构造函数
        private DbConnectionFactory() { }
    }
}
