using log4net;
using LTCingFW.utils;
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
    public class DBSession
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DBSession));

        public String DbAlias { get; set; }

        public DbConnection Connection { set; get; }

        public string ConnectionString { get; set; }

        public DbTransaction Transaction { set; get; }

        public DbProviderFactory DbFactory { get; set; }

        private DBSession() { }
        public static DBSession OpenSession(String dbAlias,bool OpenTransaction)
        {
            try
            {
                DBSession session = new DBSession();
                session.GetConnection(dbAlias);
                session.Connection.Open();
                if (OpenTransaction) {
                    session.BeginTransaction();
                }
                return session;
            }
            catch (Exception e)
            {
                throw new LTCingFWException("创建DBSession错误："+e.Message);
            }
            
        }

        public void BeginTransaction()
        {
            if (Connection == null)
            {
                throw new LTCingFWException("无DB连接，请先打开DB连接！");
            }
            if (Transaction != null)
            {
                throw new LTCingFWException("连接事务未结束，不可开启新事务！");
            }
            Transaction = Connection.BeginTransaction();
        }

        public void EndTransaction()
        {
            
            if (Transaction != null)
            {
                Transaction.Commit();
                Transaction.Dispose();
                Transaction = null;
            }
        }

        public void Close()
        {
            try
            {
                EndTransaction();
                if (Connection != null)
                {
                    Connection.Close();
                    Connection = null;
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message+e.StackTrace);
            }
            

        }

        public void Commit()
        {
            if (Transaction != null)
            {
                Transaction.Commit();
            }
        }

        public void RollBack()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
            }
        }


        private void GetConnection(string dbAlias)
        {
            DB_Leaf node = LTCingFWSet.dbDic[dbAlias];
            String dbtype = node.DbType.Trim().ToLower();
            this.DbAlias = dbAlias;

            if (FwUtilFunc.StringIsNotEmpty(node.ProviderName)) {
                this.DbFactory = DbProviderFactories.GetFactory(node.ProviderName);
                this.Connection = DbFactory.CreateConnection();
                if (FwUtilFunc.StringIsNotEmpty(node.ConnectionString))
                {
                    this.Connection.ConnectionString = node.ConnectionString;
                    return;
                }
            }
            if (dbtype == "oracle")
            {
                OracleConnectionStringBuilder info = new OracleConnectionStringBuilder();
                if (node.ConnectionString != null && node.ConnectionString.Trim() != "")
                {
                    info.ConnectionString = node.ConnectionString.Trim();
                }
                else
                {
                    info.DataSource = node.DataSource.Trim();
                    info.UserID = node.UserID.Trim();
                    info.Password = node.Password.Trim();
                    info.ConnectionTimeout = int.Parse(node.ConnectionTimeout.Trim());
                    info.Pooling = node.Pooling.Trim().ToLower() == "true" ? true : false;
                    info.MaxPoolSize = int.Parse(node.MaxPoolSize.Trim());
                    info.MinPoolSize = int.Parse(node.MinPoolSize.Trim());
                    info.IncrPoolSize = int.Parse(node.IncrPoolSize.Trim());
                    info.DecrPoolSize = int.Parse(node.DecrPoolSize.Trim());
                }
                this.Connection = new OracleConnection();
                this.Connection.ConnectionString = info.ConnectionString;
            }
            else if (dbtype == "sqlserver")
            {
                SqlConnectionStringBuilder info = new SqlConnectionStringBuilder();
                if (node.ConnectionString != null && node.ConnectionString.Trim() != "")
                {
                    info.ConnectionString = node.ConnectionString.Trim();
                }
                else
                {
                    info.DataSource = node.DataSource.Trim();
                    if (node.InitialCatalog.Trim() != "")
                    {
                        info.InitialCatalog = node.InitialCatalog.Trim();
                    }
                    info.UserID = node.UserID.Trim();
                    info.Password = node.Password.Trim();
                    info.ConnectTimeout = int.Parse(node.ConnectionTimeout.Trim());
                    info.Pooling = node.Pooling.Trim().ToLower() == "true" ? true : false;
                    info.MaxPoolSize = int.Parse(node.MaxPoolSize.Trim());
                    info.MinPoolSize = int.Parse(node.MinPoolSize.Trim());
                }
                this.Connection = new SqlConnection();
                this.Connection.ConnectionString = info.ConnectionString;
            }
            else if (dbtype == "mysql")
            {

                MySqlConnectionStringBuilder info = new MySqlConnectionStringBuilder();
                if (node.ConnectionString != null && node.ConnectionString.Trim() != "")
                {
                    info.ConnectionString = node.ConnectionString.Trim();
                }
                else
                {
                    info.Server = node.DataSource.Trim();
                    info.Database = node.Database.Trim();
                    info.UserID = node.UserID.Trim();
                    info.Password = node.Password.Trim();
                    info.ConnectionTimeout = uint.Parse(node.ConnectionTimeout.Trim());
                    info.Pooling = node.Pooling.Trim().ToLower() == "true" ? true : false;
                    info.MaximumPoolSize = uint.Parse(node.MaxPoolSize.Trim());
                    info.MinimumPoolSize = uint.Parse(node.MinPoolSize.Trim());
                }
                this.Connection = new MySqlConnection();
                this.Connection.ConnectionString = info.ConnectionString;
            }
            else
            {
                throw new LTCingFWException(dbtype + "为不支持的数据库类型。");
            }
        }

        /// <summary>
        /// 获取Oracle或者SqlServer的数据适配器
        /// </summary>
        /// <param name="conn">连接</param>
        /// <param name="sql">sql语句</param>
        /// <returns>DbDataAdapter</returns>
        public static DbDataAdapter GetDataAdapter(DbConnection conn, String sql)
        {
            if (conn is OracleConnection)
            {
                OracleDataAdapter adapter = new OracleDataAdapter(sql, (OracleConnection)conn);
                return adapter;
            }
            else if (conn is SqlConnection)
            {
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

    }

}
