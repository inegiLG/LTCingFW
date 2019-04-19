using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LTCingFW.unitdata;

namespace LTCingFW.unitdata
{
    [Service]
    public class UDConstructionService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UDConstructionService));
        private string mysql_create_ud_table = "CREATE DATABASE IF NOT EXISTS uddb default charset utf8 COLLATE utf8_bin;USE uddb;CREATE TABLE `project_info` (  `PROJECT_NAME` varchar(20) COLLATE utf8_bin NOT NULL DEFAULT '' COMMENT '项目名',  `UPDATE_TIME` datetime NOT NULL COMMENT '更新时间',  `PROJECT_DESC` varchar(50) COLLATE utf8_bin DEFAULT NULL COMMENT '项目描述',  PRIMARY KEY (`PROJECT_NAME`)) ;CREATE TABLE `table_info` (  `ID` int(11) NOT NULL AUTO_INCREMENT COMMENT 'ID',  `UPDATE_TIME` datetime NOT NULL COMMENT '更新时间',  `TABLE_NAME` varchar(20) COLLATE utf8_bin DEFAULT NULL COMMENT '表名',  `TABLE_DESC` varchar(50) COLLATE utf8_bin DEFAULT NULL COMMENT '表描述',  `OWNER_PROJECT` varchar(20) NOT NULL COMMENT '所属项目',  `USE_ROW` int(11) NOT NULL DEFAULT '0' COMMENT '是否使用行',  `USE_COLUMN` int(11) NOT NULL DEFAULT '1' COMMENT '是否使用列',  PRIMARY KEY (`ID`),  UNIQUE KEY `unique_table_info` (`OWNER_PROJECT`,`TABLE_NAME`) USING BTREE) ;CREATE TABLE `row_info` (  `OWNER_TABLE` int(11) NOT NULL COMMENT '所属表',  `ID` int(11) NOT NULL COMMENT 'ID',  `CREATE_TIME` datetime NOT NULL COMMENT '添加时间',  PRIMARY KEY (`OWNER_TABLE`,`ID`)) ;CREATE TABLE `column_info` (  `ID` int(11) NOT NULL AUTO_INCREMENT COMMENT 'ID',  `UPDATE_TIME` datetime NOT NULL COMMENT '更新时间',  `TABLE_ID` int(11) NOT NULL COMMENT '所属表',  `VALUE_TYPE` varchar(10) COLLATE utf8_bin NOT NULL COMMENT '列类型',  `COLUMN_NAME` varchar(20) COLLATE utf8_bin DEFAULT NULL COMMENT '列名',  `COLUMN_DESC` varchar(50) COLLATE utf8_bin DEFAULT NULL COMMENT '列描述',  PRIMARY KEY (`ID`),  UNIQUE KEY `INDEX_COLUMN_INFO` (`TABLE_ID`,`COLUMN_NAME`) USING BTREE) ;CREATE TABLE `value_info` (  `ID` int(11) NOT NULL AUTO_INCREMENT COMMENT 'ID',  `CREATE_TIME` datetime NOT NULL COMMENT '创建时间',  `TABLE_ID` int(11) NOT NULL COMMENT '所属表',  `ROW_ID` int(11) NULL COMMENT '行号',  `COLUMN_ID` int(11) NOT NULL COMMENT '列号',  `VALUE_TYPE` varchar(10) COLLATE utf8_bin DEFAULT NULL COMMENT '值类型',  `V_INT` int(11) DEFAULT NULL COMMENT '整数型实际值',  `V_DECIMAL` double DEFAULT NULL COMMENT '小数型实际值',  `V_STRING` varchar(50) COLLATE utf8_bin DEFAULT NULL COMMENT '字符串型实际值',  `V_BOOL` bit(1) DEFAULT NULL COMMENT '布尔型实际值',  `V_BINARY` mediumblob COMMENT '二进制型实际值',  `V_DATE` datetime DEFAULT NULL COMMENT '日期型实际值',  PRIMARY KEY (`ID`),  KEY `INDEX_VALUE_INFO` (`TABLE_ID`,`COLUMN_ID`,`CREATE_TIME`) USING BTREE,  KEY `ROW_VALUE_INFO` (`TABLE_ID`,`ROW_ID`) USING BTREE) ;";

        private string mysql_lock_table_write = "LOCK TABLE ROW_INFO WRITE";

        private string mysql_unlock_tables = "UNLOCK TABLES";

        private string mysql_get_max_row_id = "SELECT MAX(ID) FROM ROW_INFO";

        private string mysql_use_db = "USE UDDB";

        //查询所有数据库
        public string MYSQL_QUERY_DATABASE = " SELECT SCHEMA_NAME FROM information_schema.SCHEMATA ";

        [Injected(Name = "UDDao")]
        public UDDao udDao;


        /// <summary>
        /// 查询所有的database
        /// </summary>
        /// <returns></returns>
        [DBSession("uudb")]
        public virtual DataTable QueryAllDataBase()
        {
            DBSession session = LTCingFW.LTCingFWSet.GetThreadContext().DBSession;
            string sqlText = MYSQL_QUERY_DATABASE;
            return udDao.executeSqlQuery(session, sqlText);
        }

        /// <summary>
        /// 使用UDDB
        /// </summary>
        [DBSession("uudb")]
        public virtual int UseUDDB()
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.executeSqlNotQuery(session, mysql_use_db);
        }


        /// <summary>
        /// 创建MYSQL数据库表
        /// </summary>
        /// <param name="dbAlias"></param>
        [DBSession("uudb")]
        public virtual int CreateUDMySqlTable()
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.executeSqlNotQuery(session, mysql_create_ud_table);
        }


        //插入
        [DBSession("uudb",OpenTransaction = true)]
        public virtual int InsertUDData(OrmBaseModel tbModel)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.Insert(session, tbModel);
        }


        //更新
        [DBSession("uudb", OpenTransaction = true)]
        public virtual int UpdateUDData(OrmBaseModel tbModel)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.Update(session, tbModel,true);
        }

        //删除
        [DBSession("uudb", OpenTransaction = true)]
        public virtual int DeleteUDData(OrmBaseModel tbModel)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.Delete(session, tbModel,true);
        }

        //查询
        [DBSession("uudb")]
        public virtual DataTable QueryUDData(OrmBaseModel tbModel)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.Select(session, tbModel);
        }


        [DBSession("uudb")]
        public virtual void QueryValues(string columnID, string startTime, string endTime)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;

            ValueInfoOrmModel model = null;
            if (session.Connection is MySqlConnection)
            {
                model = new ValueInfoOrmModel();
            }

            if (model != null)
            {
                model.COLUMN_ID = columnID;
                OrmBaseModel m = (OrmBaseModel)model;

                m.Where = " UPDATE_TIME > '" + startTime + "' AND UPDATE_TIME < '" + endTime + "' ";
                udDao.Select(session, m);
            }

        }
        [DBSession("uudb")]
        public virtual List<ProjectInfoOrmModel> QueryProjectInfo(ProjectInfoOrmModel tbModel)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.SelectT<ProjectInfoOrmModel>(session, tbModel); 
        }
        [DBSession("uudb")]
        public virtual List<TableInfoOrmModel> QueryTableInfo(TableInfoOrmModel tbModel)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.SelectT<TableInfoOrmModel>(session, tbModel);
        }
        [DBSession("uudb")]
        public virtual List<ColumnInfoOrmModel> QueryColumnInfo(ColumnInfoOrmModel colModel)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.SelectT<ColumnInfoOrmModel>(session, colModel);
        }
        [DBSession("uudb")]
        public virtual List<RowInfoOrmModel> QueryRowInfo(RowInfoOrmModel colModel)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.SelectT<RowInfoOrmModel>(session, colModel);
        }
        [DBSession("uudb")]
        public virtual List<ValueInfoOrmModel> QueryValueInfo(ValueInfoOrmModel colModel)
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.SelectT<ValueInfoOrmModel>(session, colModel);
        }

        [DBSession("uudb")]
        public virtual int QueryNewestRowIndex()
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            DataTable res = udDao.executeSqlQuery(session, mysql_get_max_row_id);
            if (res.Rows[0][0] is System.DBNull)
            {
                return 0;
            }
            return Convert.ToInt32(res.Rows[0][0]);
        }

        [DBSession("uudb")]
        public virtual int LockRowInfoTable()
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.executeSqlNotQuery(session, mysql_lock_table_write);
        }

        [DBSession("uudb")]
        public virtual int UnLockTables()
        {
            DBSession session = LTCingFWSet.GetThreadContext().DBSession;
            return udDao.executeSqlNotQuery(session, mysql_unlock_tables);
        }
    }



}
