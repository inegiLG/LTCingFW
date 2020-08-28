using log4net;
using LTCingFW.utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class DBSession
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DBSession));

        public const string Oracle_ProviderName = "Oracle.ManagedDataAccess.Client";
        public const string MySql_ProviderName = "MySql.Data.MySqlClient";
        public const string SqlServer_ProviderName = "System.Data.SqlClient";

        public String DbAlias { get; set; }

        public DbConnection Connection { set; get; }

        public string ConnectionString { get; set; }

        public DbTransaction Transaction { set; get; }

        public DbProviderFactory DbFactory { get; set; }

        public string ProviderName { get; set; }

        private DBSession() { }

        /// <summary>
        /// 开始Session
        /// </summary>
        /// <param name="dbAlias">连接名</param>
        /// <param name="OpenTransaction">是否开启事务</param>
        /// <returns></returns>
        public static DBSession OpenSession(String dbAlias, bool OpenTransaction)
        {
            try
            {
                DBSession session = new DBSession();
                session.GetConnection(dbAlias);
                session.Connection.Open();
                if (OpenTransaction)
                {
                    session.BeginTransaction();
                }
                logger.Debug(dbAlias + "创建新的DBSession！");
                return session;
            }
            catch (Exception e)
            {
                throw new LTCingFWException("数据库连接故障，创建DBSession[" + dbAlias + "]错误：" + e.Message);
            }
        }

        /// <summary>
        /// 判断session是否关闭
        /// </summary>
        /// <returns></returns>
        public Boolean IsClosed()
        {
            if (Connection == null) {
                return true;
            }
            if (Connection.State == ConnectionState.Closed) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 结束Session
        /// </summary>
        public void Close()
        {
            try
            {
                Commit();
                if (Connection != null)
                {
                    Connection.Close();
                    Connection = null;
                }
                logger.Debug(DbAlias + "DBSession结束！");
            }
            catch (Exception e)
            {
                throw new LTCingFWException("关闭DBSession[" + DbAlias + "]错误：" + e.Message + e.StackTrace);
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


        public void Commit()
        {
            if (Transaction != null)
            {
                Transaction.Commit();
                Transaction.Dispose();
                Transaction = null;
            }
        }

        public void RollBack()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
                Transaction.Dispose();
                Transaction = null;
            }
        }

        private void GetConnection(string dbAlias)
        {
            DB_Leaf node = LTCingFWSet.dbDic[dbAlias];
            //String dbtype = node.DbType.Trim().ToLower();
            this.DbAlias = dbAlias;

            if (FwUtilFunc.StringIsNotEmpty(node.ProviderName))
            {
                this.DbFactory = DbProviderFactories.GetFactory(node.ProviderName);
                this.Connection = DbFactory.CreateConnection();
                this.ProviderName = node.ProviderName;
                if (FwUtilFunc.StringIsNotEmpty(node.ConnectionString))
                {
                    this.Connection.ConnectionString = node.ConnectionString;
                    return;
                }
            }

        }

        public static DbDataAdapter GetDataAdapter(DBSession session, String sql)
        {
            DbDataAdapter adapter = session.DbFactory.CreateDataAdapter();
            if (adapter == null)
            {
                Assembly assem = session.DbFactory.GetType().Assembly;
                foreach (Type t in assem.GetTypes())
                {
                    if (t.BaseType == typeof(DbDataAdapter))
                    {
                        adapter = assem.CreateInstance(session.ProviderName + "." + t.Name) as DbDataAdapter;
                        break;
                    }
                }
            }
            DbCommand cmd = session.Connection.CreateCommand();
            cmd.CommandText = sql;
            if (session.Transaction != null) {
                cmd.Transaction = session.Transaction;
            }
            adapter.SelectCommand = cmd;
            return adapter;
        }

    }

}
