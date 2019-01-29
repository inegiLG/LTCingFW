using log4net;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

        public DbTransaction Transaction { set; get; }

        private DBSession() { }
        public static DBSession OpenSession(String dbAlias,bool OpenTransaction)
        {
            try
            {
                DBSession session = new DBSession();
                session.DbAlias = dbAlias;
                session.Connection = DbConnectionFactory.GetConnectionFromPool(dbAlias);
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

    }

}
