using log4net;
using LTCingFW;
using LTCingFW.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;


namespace LTCingFW
{
    public class OrmBaseDao
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(OrmBaseDao));

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private String GetTableName(DBSession session, OrmBaseModel model)
        {
            try
            {
                if (model == null && this is OrmBaseModel)
                {
                    model = (OrmBaseModel)this;
                }
                if (session == null)
                {
                    session = LTCingFWSet.GetThreadContext().DBSession;
                }
                OrmTableAttribute[] attrs = model.GetType().GetCustomAttributes(typeof(OrmTableAttribute), true) as OrmTableAttribute[];
                foreach (OrmTableAttribute attr in attrs)
                {
                    if (attr.DbAlias != null && attr.DbAlias == session.DbAlias)
                    {
                        return attr.TableName;
                    }
                }
                return attrs[0].TableName;
            }
            catch (Exception e)
            {
                throw new LTCingFWException("获取表名错误！", e);
            }

        }


        /// <summary>
        /// 获取查询时所有的列名的SQL字符串
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private String GetAllColumnNameStr(DBSession session, OrmBaseModel model)
        {
            try
            {
                if (model == null && this is OrmBaseModel)
                {
                    model = (OrmBaseModel)this;
                }
                if (session == null)
                {
                    session = LTCingFWSet.GetThreadContext().DBSession;
                }
                StringBuilder sb = new StringBuilder();
                foreach (OrmColumnBean bean in model.OrmList)
                {
                    OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                    sb.Append(attr.ColName).Append(',');
                }
                sb.Remove(sb.Length - 1, 1);//去掉最后一个逗号
                return sb.ToString();
            }
            catch (Exception e)
            {
                throw new LTCingFWException("获取所有列名错误！", e);
            }

        }
        /// <summary>
        /// 获取查询时所有的列名的SQL字符串
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private String GetInsertAllColumnNameStr(DBSession session, OrmBaseModel model)
        {
            try
            {
                if (model == null && this is OrmBaseModel)
                {
                    model = (OrmBaseModel)this;
                }
                if (session == null)
                {
                    session = LTCingFWSet.GetThreadContext().DBSession;
                }
                StringBuilder sb = new StringBuilder();
                foreach (OrmColumnBean bean in model.OrmList)
                {
                    OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                    if (attr.PrimaryKey && bean.Value == null)
                    {
                        continue;
                    }
                    sb.Append(attr.ColName).Append(',');
                }
                sb.Remove(sb.Length - 1, 1);//去掉最后一个逗号
                return sb.ToString();
            }
            catch (Exception e)
            {
                throw new LTCingFWException("获取所有列名错误！", e);
            }

        }

        /// <summary>
        /// 获取插入的值SQL和ValueList
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <param name="ValueList"></param>
        /// <param name="sqlText"></param>
        private void GetInsertColumnValues(DBSession session, OrmBaseModel model, List<DbParameter> ValueList, StringBuilder sqlText)
        {
            try
            {
                if (model == null && this is OrmBaseModel)
                {
                    model = (OrmBaseModel)this;
                }
                if (session == null)
                {
                    session = LTCingFWSet.GetThreadContext().DBSession;
                }
                sqlText.Append("(");
                foreach (OrmColumnBean bean in model.OrmList)
                {
                    DbParameter param = null;
                    OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                    if (attr.PrimaryKey && bean.Value == null)
                    {
                        continue;
                    }
                    string mark = "@";
                    if (session.ProviderName == DBSession.Oracle_ProviderName)
                    {
                        mark = ":";
                    }
                    if (session.DbFactory != null)
                    {
                        param = session.DbFactory.CreateParameter();
                    }
                    else
                    {
                        throw new LTCingFWException("不支持该数据库！");
                    }
                    if (attr.ColSize == 0)
                    {
                        param.Size = attr.ColSize;
                    }
                    param.ParameterName = mark + attr.ColName;
                    param.Value = getProperDbParameterValue(bean.Value, attr.ColType);
                    ValueList.Add(param);
                    sqlText.Append(mark).Append(attr.ColName).Append(",");
                }
                sqlText.Remove(sqlText.Length - 1, 1);
                sqlText.Append(")");
            }
            catch (Exception e)
            {
                throw new LTCingFWException("插入式中获取所有列名以及值错误！", e);
            }

        }
        /// <summary>
        /// 获取更新时的SET语句的SQL字符串
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private String GetUpdateSetColumnStr(DBSession session, OrmBaseModel model)
        {
            try
            {
                if (model == null && this is OrmBaseModel)
                {
                    model = (OrmBaseModel)this;
                }
                if (session == null)
                {
                    session = LTCingFWSet.GetThreadContext().DBSession;
                }
                StringBuilder sb = new StringBuilder();
                foreach (OrmColumnBean bean in model.OrmList)
                {
                    OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                    if (!attr.PrimaryKey)
                    {
                        if (bean.Value == null)
                        {
                            continue;
                        }
                        if (session.ProviderName == DBSession.Oracle_ProviderName)
                        {
                            sb.Append(" ").Append(attr.ColName).Append('=').Append(':').Append(attr.ColName).Append(",");
                        }
                        else
                        {
                            sb.Append(" ").Append(attr.ColName).Append('=').Append('@').Append(attr.ColName).Append(",");
                        }

                    }
                }
                if (sb.Length == 0)
                {
                    throw new LTCingFWException("非主键列无更新值！");
                }
                sb.Remove(sb.Length - 1, 1);//去掉最后一个逗号
                return sb.ToString();
            }
            catch (Exception e)
            {
                throw new LTCingFWException("更新式中获取SET所有列名错误！", e);
            }

        }

        /// <summary>
        /// 获取更新时的SET值列表
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private void GetUpdateSetColumnValues(DBSession session, OrmBaseModel model, List<DbParameter> ValueList)
        {
            try
            {
                if (model == null && this is OrmBaseModel)
                {
                    model = (OrmBaseModel)this;
                }
                if (session == null)
                {
                    session = LTCingFWSet.GetThreadContext().DBSession;
                }
                foreach (OrmColumnBean bean in model.OrmList)
                {
                    DbParameter param = null;
                    OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                    if (!attr.PrimaryKey)
                    {
                        if (bean.Value == null)
                        {
                            continue;
                        }
                        string mark = "@";
                        if (session.ProviderName == DBSession.Oracle_ProviderName)
                        {
                            mark = ":";
                        }
                        if (session.DbFactory != null)
                        {
                            param = session.DbFactory.CreateParameter();
                        }
                        else
                        {
                            throw new LTCingFWException("不支持该数据库！");
                        }
                        if (attr.ColSize == 0)
                        {
                            param.Size = attr.ColSize;
                        }
                        param.ParameterName = mark + attr.ColName;
                        param.Value = getProperDbParameterValue(bean.Value, attr.ColType);
                        ValueList.Add(param);
                    }
                }
            }
            catch (Exception e)
            {
                throw new LTCingFWException("更新式中获取SET所有列名的值错误！", e);
            }

        }
        /// <summary>
        /// DbParameter值转换
        /// </summary>
        /// <param name="value">原值，多为字符</param>
        /// <param name="colType">db数据类型</param>
        /// <returns></returns>
        private object getProperDbParameterValue(object value, int colType)
        {
            try
            {
                //空
                if (value == null || value is System.DBNull)
                {
                    return System.DBNull.Value;
                }
                //以字符串模拟其他类型


                if (value is string && value.ToString().Trim() == "")
                {
                    return System.DBNull.Value;
                }
                if (colType > 0 && colType <= 100)
                {
                    //SqlServer Type
                    if (FwUtilFunc.sqlserverTypeIsString(colType))
                    {
                        return value;
                    }
                    else if (FwUtilFunc.sqlserverTypeIsNumber(colType))
                    {
                        return Decimal.Parse((string)value);
                    }
                    else if (FwUtilFunc.sqlserverTypeIsDate(colType))
                    {
                        return Convert.ToDateTime((string)value);//使用DateTime
                    }
                    else
                    {
                        throw new LTCingFWException(String.Format("不可处理{0}的SqlServer数据类型", colType));
                    }
                }
                else if (colType > 100 && colType <= 200)
                {
                    //Oracle Type
                    if (FwUtilFunc.oracleTypeIsString(colType))
                    {
                        return value;
                    }
                    else if (FwUtilFunc.oracleTypeIsNumber(colType))
                    {
                        return Decimal.Parse((string)value);
                    }
                    else if (FwUtilFunc.oracleTypeIsDate(colType))
                    {
                        return Convert.ToDateTime((string)value);//使用DateTime
                    }
                    else
                    {
                        throw new LTCingFWException(String.Format("不可处理{0}的Oracle数据类型", colType));
                    }
                }
                else if (colType > 200 && colType <= 300)
                {
                    //MySql
                    if (FwUtilFunc.mysqlTypeIsString(colType))
                    {
                        return value;
                    }
                    if (FwUtilFunc.mysqlTypeIsNumber(colType))
                    {
                        return Decimal.Parse((string)value);
                    }
                    if (FwUtilFunc.mysqlTypeIsDate(colType))
                    {
                        return Convert.ToDateTime((string)value);//使用DateTime
                    }
                    else
                    {
                        throw new LTCingFWException(String.Format("不可处理{0}的MySql数据类型", colType));
                    }
                }
                else if (colType > 1000 && colType <= 1100)
                {
                    object res = null;
                    switch (colType)
                    {
                        case (int)OrmDataType.CommonType.INT:
                            res = Convert.ToInt32(value);
                            break;
                        case (int)OrmDataType.CommonType.STRING:
                            res = Convert.ToString(value);
                            break;
                        case (int)OrmDataType.CommonType.DECIMAL:
                            res = Convert.ToDecimal(value);
                            break;
                        case (int)OrmDataType.CommonType.BOOL:
                            if (value is bool)
                            {
                                res = value;
                            }
                            else if(value is string)
                            {
                                if (Convert.ToString(value).Trim() == "0" || Convert.ToString(value).Trim().ToUpper() == "FALSE")
                                {
                                    res = false;
                                }
                                else if (Convert.ToString(value).Trim() == "1" || Convert.ToString(value).Trim().ToUpper() == "TRUE")
                                {
                                    res = true;
                                }
                                else
                                {
                                    throw new LTCingFWException("非法的BOOL类型，请使用0/1/false/true!");
                                }
                            }
                            else
                            {
                                res = Convert.ToBoolean(value);
                            }
                            break;
                        case (int)OrmDataType.CommonType.DATE:
                            res = Convert.ToDateTime(value);
                            if ((DateTime)res == new DateTime())
                            {
                                res = System.DBNull.Value;
                            }
                            break;
                        case (int)OrmDataType.CommonType.BINARY:
                            if (value is byte[])
                            {
                                res = value;
                            }
                            else
                            {
                                throw new LTCingFWException("BINARY 类型必须是byte[]类型！");
                            }
                            break;
                        default:
                            break;
                    }
                    return res;
                }
                else
                {
                    throw new LTCingFWException(String.Format("没有{0}数据类型", colType));
                }
            }
            catch (Exception e)
            {
                throw new LTCingFWException("参数赋值前根据ColType类型进行数据类型强转时错误！类型[" + colType + "] 值为["+ value.ToString() + "]", e);
            }


        }
        ////默认与主键无关，用于查询
        //private void SetModelWhereSqlTextAndValues(DBSession session, OrmBaseModel model, StringBuilder sqlText, List<DbParameter> ValueList, bool onlyPrimaryKey)
        //{
        //    SetModelWhereSqlTextAndValues(session, model, sqlText, ValueList, onlyPrimaryKey);
        //}

        /// <summary>
        /// 获取查询的WHERE部分sql语句和Values
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <param name="sqlText"></param>
        /// <param name="ValueList"></param>
        /// <param name="onlyPrimaryKey">是否只按照主键查询</param>
        /// <param name="fuzzy">是否为模糊查询</param>
        private void SetModelWhereSqlTextAndValues(DBSession session, OrmBaseModel model, StringBuilder sqlText, List<DbParameter> ValueList, bool onlyPrimaryKey)
        {
            try
            {
                if (model == null && this is OrmBaseModel)
                {
                    model = (OrmBaseModel)this;
                }
                if (session == null)
                {
                    session = LTCingFWSet.GetThreadContext().DBSession;
                }
                bool hasPrimaryKey = false;
                //遍历属性值
                foreach (OrmColumnBean bean in model.OrmList)
                {

                    if (bean.Value != null)
                    {
                        DbParameter param = null;
                        OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                        if (attr.PrimaryKey && bean.Value != null)
                        {
                            hasPrimaryKey = true;
                        }

                        if (onlyPrimaryKey && !attr.PrimaryKey) { }
                        else
                        {
                            string mark = "@";
                            if (session.ProviderName == DBSession.Oracle_ProviderName)
                            {
                                mark = ":";
                            }
                            if (session.DbFactory != null)
                            {
                                param = session.DbFactory.CreateParameter();
                            }
                            else
                            {
                                throw new LTCingFWException("只接受Oracle SqlServer MySql 的连接！");
                            }

                            param.ParameterName = mark + attr.ColName;
                            if (attr.ColSize != 0)
                            {
                                param.Size = attr.ColSize;
                            }
                            if (model.FuzzyColumnNames.Contains(bean.ColumnName))
                            {
                                sqlText.Append(" AND ").Append(attr.ColName).Append(" LIKE ").Append(mark).Append(attr.ColName);
                                param.Value = "%" + getProperDbParameterValue(bean.Value, attr.ColType) + "%";
                                ValueList.Add(param);
                            }
                            else
                            {
                                sqlText.Append(" AND ").Append(attr.ColName).Append(" = ").Append(mark).Append(attr.ColName);
                                param.Value = getProperDbParameterValue(bean.Value, attr.ColType);
                                ValueList.Add(param);
                            }

                        }

                    }
                }
                if (onlyPrimaryKey && !hasPrimaryKey)
                {
                    throw new LTCingFWException("按照主键查询未找到主键或主键值为空！");
                }
            }
            catch (Exception e)
            {
                throw new LTCingFWException("生成WHERE条件语句以及设置值出错！", e);
            }


        }




        /// <summary>
        /// 为普通的sql语句加上分页信息
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private String addPaginationSql(String sql, DBSession session, OrmBaseModel model)
        {
            try
            {
                if (model == null && this is OrmBaseModel)
                {
                    model = (OrmBaseModel)this;
                }
                if (session == null)
                {
                    session = LTCingFWSet.GetThreadContext().DBSession;
                }
                int max_limit = model.UpLimitNumber;
                int min_limit = model.LowLimitNumber;
                String allColumnString = GetAllColumnNameStr(session, model);
                StringBuilder sb = new StringBuilder();
                String orderby = "";
                if (FwUtilFunc.StringIsEmpty(model.OrderBy))
                {
                    foreach (OrmColumnBean bean in model.OrmList)
                    {
                        if (bean.OrmColumnAttributeDic[session.DbAlias].PrimaryKey)
                        {
                            orderby = " " + bean.ColumnName + " DESC ";
                        }
                    }
                }
                else
                {
                    orderby = model.OrderBy;
                }

                //oracle
                if (session.ProviderName == DBSession.Oracle_ProviderName)
                {
                    sql = sql + " ORDER BY " + orderby;
                    sb.Append(" SELECT ").Append(allColumnString).Append(" FROM ");
                    sb.Append(" ( SELECT ").Append(" ROWNUM RN , ").Append(allColumnString);
                    sb.Append(" FROM ( ").Append(sql).Append(" ) ").Append(" WHERE ROWNUM <= ").Append(max_limit).Append(" ) ");
                    sb.Append(" WHERE RN >= ").Append(min_limit);

                }
                //sqlserver
                if (session.ProviderName == DBSession.SqlServer_ProviderName)
                {
                    sb.Append(" SELECT ").Append(allColumnString).Append(" FROM ");
                    sb.Append(" ( SELECT TOP ").Append(max_limit).Append(" ROW_NUMBER() OVER( ORDER BY ").Append(orderby).Append(" ) RN, ");
                    sb.Append(allColumnString).Append(" FROM ( ").Append(sql).Append(" ) t ) w2 ");
                    sb.Append(" WHERE w2.RN >= ").Append(min_limit);
                    sb.Append(" ORDER BY w2.RN ASC ");

                }
                //mysql
                if (session.ProviderName == DBSession.MySql_ProviderName)
                {
                    sql = sql + " ORDER BY " + orderby;
                    sb.Append(sql).Append(" LIMIT ").Append(min_limit - 1).Append(",").Append(model.PageItemCount);
                }

                return sb.ToString();
            }
            catch (Exception e)
            {

                throw new LTCingFWException("生成分页语句出错！", e);
            }

        }

        /// <summary>
        /// 获取条目数(OrmModel使用)
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int GetItemCount(DBSession session)
        {
            if (!(this is OrmBaseModel))
            {
                throw new LTCingFWException("此方法仅供OrmModel使用！");
            }
            return GetItemCount(session, null);
        }

        /// <summary>
        /// 获取条目数
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int GetItemCount(DBSession session, OrmBaseModel model)
        {
            if (model == null && this is OrmBaseModel)
            {
                model = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT COUNT(1) FROM ").Append(GetTableName(session, model)).Append(" WHERE 1=1 ");
            List<DbParameter> ValueList = new List<DbParameter>();
            SetModelWhereSqlTextAndValues(session, model, sql, ValueList, false);
            DbCommand cmd = session.Connection.CreateCommand();
            if (FwUtilFunc.StringIsNotEmpty(model.Where))
            {
                sql.Append(model.Where);
            }
            cmd.CommandText = sql.ToString();
            logger.Debug(sql.ToString());
            cmd.Parameters.AddRange(ValueList.ToArray());
            object result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// 分页查询(OrmModel使用)
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public DataTable SelectPage(DBSession session)
        {
            if (!(this is OrmBaseModel))
            {
                throw new LTCingFWException("此方法仅供OrmModel使用！");
            }
            return SelectPage(session, null);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns>DataTable</returns>
        public DataTable SelectPage(DBSession session, OrmBaseModel model)
        {
            if (model == null && this is OrmBaseModel)
            {
                model = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DbConnection conn = session.Connection;
            List<DbParameter> ValueList = new List<DbParameter>();
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" SELECT ");
            if (model.Distinct)
            {
                sqlText.Append(" DISTINCT ");
            }
            sqlText.Append(GetAllColumnNameStr(session, model)).Append(" FROM ").Append(GetTableName(session, model)).Append(" WHERE 1=1 ");
            SetModelWhereSqlTextAndValues(session, model, sqlText, ValueList, false);
            if (FwUtilFunc.StringIsNotEmpty(model.Where))
            {
                sqlText.Append(" AND ").Append(model.Where);
            }

            String pageSql = addPaginationSql(sqlText.ToString(), session, model);
            #region 缓存(分页不用)
            //if (CacheFactory.IsCached(session, model))
            //{
            //   DataTable cache = CacheFactory.GetTabelCache(pageSql);
            //    if (cache != null)
            //    {
            //        return cache;
            //    }
            //    else {
            //        DataTable table = Select(conn, pageSql, ValueList.ToArray()).Copy();
            //        table.TableName = GetTableName(session, model);
            //        CacheFactory.SetTableCache(pageSql, table);
            //        return table;
            //    }
            //}
            #endregion
            return Select( session, pageSql, ValueList.ToArray());
        }

        /// <summary>
        /// 分页查询(OrmModel使用)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <returns></returns>
        public List<T> SelectPage<T>(DBSession session) where T : OrmBaseModel
        {
            if (!(this is OrmBaseModel))
            {
                throw new LTCingFWException("此方法仅供OrmModel使用！");
            }
            return SelectPage<T>(session, null);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns>List<T></returns>
        public List<T> SelectPage<T>(DBSession session, OrmBaseModel model) where T : OrmBaseModel
        {
            if (model == null && this is OrmBaseModel)
            {
                model = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DataTable res = SelectPage(session, model);
            List<T> resT = FwUtilFunc.LoadOrmModelListFromDataTable<T>(res);
            foreach (T ormModel in resT)
            {
                QueryForForeignOrmModel(session, ormModel);
            }
            return resT;
        }

        /// <summary>
        /// 默认选择,根据有值列(OrmModel使用)
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public DataTable Select(DBSession session)
        {
            if (!(this is OrmBaseModel))
            {
                throw new LTCingFWException("此方法仅供OrmModel使用！");
            }
            return Select(session, null);
        }

        /// <summary>
        /// 默认选择,根据有值列
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public DataTable Select(DBSession session, OrmBaseModel model)
        {
            if (model == null && this is OrmBaseModel)
            {
                model = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DbConnection conn = session.Connection;
            List<DbParameter> ValueList = new List<DbParameter>();
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" SELECT ");
            if (model.Distinct)
            {
                sqlText.Append(" DISTINCT ");
            }
            if (model.SqlServerTop != 0)
            {
                sqlText.Append(" TOP ").Append(model.SqlServerTop).Append(" ");
            }
            sqlText.Append(GetAllColumnNameStr(session, model)).Append(" FROM ").Append(GetTableName(session, model)).Append(" WHERE 1=1 ");
            SetModelWhereSqlTextAndValues(session, model, sqlText, ValueList, false);
            if (FwUtilFunc.StringIsNotEmpty(model.Where))
            {
                sqlText.Append(" AND ").Append(model.Where);
            }
            if (FwUtilFunc.StringIsNotEmpty(model.OrderBy))
            {
                sqlText.Append(" ORDER BY ").Append(model.OrderBy);
            }
            #region 缓存
            if (CacheFactory.IsCached(session, model))
            {
                DataTable cache = CacheFactory.GetTabelCache(sqlText.ToString());
                if (cache != null)
                {
                    return cache;
                }
                else
                {
                    DataTable table = Select( session, sqlText.ToString(), ValueList.ToArray()).Copy();
                    CacheFactory.SetTableCache(sqlText.ToString(), table);
                    return table;
                }
            }
            #endregion
            return Select( session, sqlText.ToString(), ValueList.ToArray());
        }

        /// <summary>
        /// 查询，返回List<T>(OrmModel使用)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <returns></returns>
        public List<T> SelectT<T>(DBSession session) where T : OrmBaseModel
        {
            if (!(this is OrmBaseModel))
            {
                throw new LTCingFWException("此方法仅供OrmModel使用！");
            }
            return SelectT<T>(session);
        }

        /// <summary>
        /// 查询，返回List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<T> SelectT<T>(DBSession session, OrmBaseModel model) where T : OrmBaseModel
        {
            if (model == null && this is OrmBaseModel)
            {
                model = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DataTable res = Select(session, model);
            List<T> resT = FwUtilFunc.LoadOrmModelListFromDataTable<T>(res);
            foreach (T ormModel in resT)
            {
                QueryForForeignOrmModel(session, ormModel);
            }
            return resT;
        }

        /// <summary>
        /// 通过主键查询(OrmModel使用)
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public DataTable SelectByPrimaryKey(DBSession session)
        {
            if (!(this is OrmBaseModel))
            {
                throw new LTCingFWException("此方法仅供OrmModel使用！");
            }
            return SelectByPrimaryKey(session, null);
        }

        /// <summary>
        /// 通过主键查询
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public DataTable SelectByPrimaryKey(DBSession session, OrmBaseModel model)
        {
            if (model == null && this is OrmBaseModel)
            {
                model = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DbConnection conn = session.Connection;
            List<DbParameter> ValueList = new List<DbParameter>();
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" SELECT ");
            if (model.Distinct)
            {
                sqlText.Append(" DISTINCT ");
            }
            sqlText.Append(GetAllColumnNameStr(session, model)).Append(" FROM ").Append(GetTableName(session, model)).Append(" WHERE 1=1 ");
            SetModelWhereSqlTextAndValues(session, model, sqlText, ValueList, true);
            if (FwUtilFunc.StringIsNotEmpty(model.Where))
            {
                sqlText.Append(" AND ").Append(model.Where);
            }
            if (FwUtilFunc.StringIsNotEmpty(model.OrderBy))
            {
                sqlText.Append(" ORDER BY ").Append(model.OrderBy);
            }
            return Select( session, sqlText.ToString(), ValueList.ToArray());
        }

        /// <summary>
        /// 通过主键查询,返回List<T>(OrmModel使用)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <returns></returns>
        public List<T> SelectByPrimaryKey<T>(DBSession session) where T : OrmBaseModel
        {
            if (!(this is OrmBaseModel))
            {
                throw new LTCingFWException("此方法仅供OrmModel使用！");
            }
            return SelectByPrimaryKey<T>(session, null);
        }

        /// <summary>
        /// 通过主键查询,返回List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<T> SelectByPrimaryKey<T>(DBSession session, OrmBaseModel model) where T : OrmBaseModel
        {
            if (model == null && this is OrmBaseModel)
            {
                model = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DataTable res = SelectByPrimaryKey(session, model);
            List<T> resT = FwUtilFunc.LoadOrmModelListFromDataTable<T>(res);
            foreach (T resModel in resT)
            {
                QueryForForeignOrmModel(session, resModel);
            }
            return resT;
        }

        /// <summary>
        /// 针对查询完成的OrmModel，查询其关联的表内容
        /// </summary>
        /// <param name="session">DBSession</param>
        /// <param name="model">含有其他OrmModel查询的OrmModel</param>
        private void QueryForForeignOrmModel(DBSession session, OrmBaseModel resModel)
        {
            if (resModel == null && resModel is OrmBaseModel)
            {
                resModel = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            PropertyInfo[] infos = resModel.GetType().GetProperties();
            foreach (PropertyInfo info in infos)
            {
                //1.检查是否含有【OrmForeignAttribute】特性,未设置DbAlise
                object[] attrs = info.GetCustomAttributes(typeof(OrmForeignAttribute), true);
                if (attrs.Length == 0)
                {
                    continue;
                }

                //2.根据【OrmForeignAttribute】特性查找特定表

                //2.1 此表的存值属性和关联表的属性对应关系
                OrmForeignAttribute attr = attrs[0] as OrmForeignAttribute;
                Type infoType = null;//关联目标OrmModel类型
                string[] localColumnNames = attr.LocalColumnName.Split(',');
                string[] foreignColumnNames = attr.ForeignColumnName.Split(',');
                //2.2 检查返回类型是否为ForeignOrmModel，且载有一个OrmModel
                if (info.PropertyType.Name == "ForeignOrmModel`1")
                {
                    infoType = info.PropertyType.GenericTypeArguments[0];
                    if (infoType.BaseType.FullName != "LTCingFW.OrmBaseModel")
                    {
                        throw new Exception(resModel.GetType().Name + "为非法类型，请检查关联OrmModel的类型");
                    }
                }
                else
                {
                    throw new Exception(resModel.GetType().Name + "为非法类型，请检查关联OrmModel的类型");
                }
                //2.3 创建一个空的ForeignOrmModel并设置进去
                object foreignOrmModel = Assembly.GetExecutingAssembly().CreateInstance(info.PropertyType.FullName);
                FwUtilFunc.SetObjectPropertyValue(resModel, info.Name, foreignOrmModel);
                FwUtilFunc.SetObjectPropertyValue(foreignOrmModel, "OwnerModel", resModel);
                FwUtilFunc.SetObjectPropertyValue(foreignOrmModel, "Attr", attr);
                FwUtilFunc.SetObjectPropertyValue(foreignOrmModel, "DbAlias", session.DbAlias);
                //2.4 检查是否为LAZY或者EAGER模式
                if (attr.LZModel == LZModelEnum.LAZY)
                {
                    continue;
                }
                //3.查询

                //3.1 查询所需的实例
                object inst = infoType.Assembly.CreateInstance(infoType.FullName);
                //3.2 查询所需的条件变量读取，外键
                for (int i = 0; i < localColumnNames.Length; i++)
                {
                    object value = FwUtilFunc.GetObjectPropertyValue(resModel, localColumnNames[i]);
                    FwUtilFunc.SetObjectPropertyValue(inst, foreignColumnNames[i], value);
                }
                //3.3 查询
                MethodInfo Select_Method = typeof(OrmBaseDao).GetMethod("SelectT", new Type[] { typeof(DBSession), typeof(OrmBaseModel) });
                MethodInfo Cur_Select_Method = Select_Method.MakeGenericMethod(infoType);
                object Final_Result = Cur_Select_Method.Invoke(this, new object[] { session, inst as OrmBaseModel });
                //DataTable result = Select(session, inst as OrmBaseModel);

                //3.4 通过反射调用LoadOrmModelListFromDataTable<T>
                //MethodInfo Load_Method = typeof(FwUtilFunc).GetMethod("LoadOrmModelListFromDataTable", new Type[] { typeof(DataTable) });
                //MethodInfo Cur_Load_Method = Load_Method.MakeGenericMethod(infoType);
                //Final_Result is List<OrmBaseModel>
                //object Final_Result = Cur_Load_Method.Invoke(null, new object[] { result });

                //4.设置List进如ForeignOrmModel
                FwUtilFunc.SetObjectPropertyValue(foreignOrmModel, "Result", Final_Result);

            }
        }

        /// <summary>
        /// 自定义查询
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable Select(DBSession session, String sql, DbParameter[] parameters)
        {
            logger.Debug(sql);
            DbDataAdapter adapter = DBSession.GetDataAdapter(session, sql);
            if (session.Transaction != null)
            {
                adapter.SelectCommand.Transaction = session.Transaction;
            }
            adapter.SelectCommand.Parameters.AddRange(parameters);
            DataTable resultTable = new DataTable();
            adapter.Fill(resultTable);
            return resultTable;
        }

        /// <summary>
        /// 默认插入,全列插入(OrmModel使用)
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int Insert()
        {
            if (!(this is OrmBaseModel))
            {
                throw new LTCingFWException("此方法仅供OrmModel使用！");
            }
            return Insert(null, null);
        }

        /// <summary>
        /// 默认插入,全列插入
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public int Insert(DBSession session, OrmBaseModel model)
        {
            if (model == null && this is OrmBaseModel)
            {
                model = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DbConnection conn = session.Connection;
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" INSERT INTO ").Append(GetTableName(session, model)).Append("(").Append(GetInsertAllColumnNameStr(session, model)).Append(" ) ").Append(" VALUES ");
            List<DbParameter> ValueList = new List<DbParameter>();

            GetInsertColumnValues(session, model, ValueList, sqlText);

            //清除缓存
            if (CacheFactory.IsCached(session, model))
            {
                CacheFactory.RemoveAllTableCache(GetTableName(session, model));
            }
            //执行
            return Insert( session, sqlText.ToString(), ValueList.ToArray());
        }

        /// <summary>
        /// 自定义插入
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Insert(DBSession session, String sql, DbParameter[] parameters)
        {
            return executeSqlNotQuery(session.Connection, session.Transaction, sql, parameters);
        }

        /// <summary>
        /// 依据有值列删除
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int Delete(DBSession session)
        {
            return Delete(session, false);
        }

        /// <summary>
        /// 根据有值列删除(OrmModel使用)
        /// </summary>
        /// <param name="session"></param>
        /// <param name="onlyByPk">是否只依靠主键</param>
        /// <returns></returns>
        public int Delete(DBSession session, bool onlyByPk)
        {
            if (!(this is OrmBaseModel))
            {
                throw new LTCingFWException("此方法仅供OrmModel使用！");
            }
            return Delete(session, null, onlyByPk);
        }

        /// <summary>
        /// 依据有值列删除
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Delete(DBSession session, OrmBaseModel model)
        {
            return Delete( session,  model, false);
        }

        /// <summary>
        /// 根据有值列删除
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <param name="onlyByPk">是否只依据主键删除</param>
        /// <returns></returns>
        public int Delete(DBSession session, OrmBaseModel model, bool onlyByPk)
        {
            if (model == null && this is OrmBaseModel)
            {
                model = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DbConnection conn = session.Connection;
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" DELETE FROM ").Append(GetTableName(session, model)).Append(" WHERE 1=1 ");
            List<DbParameter> ValueList = new List<DbParameter>();
            SetModelWhereSqlTextAndValues(session, model, sqlText, ValueList, onlyByPk);
            //清除缓存
            if (CacheFactory.IsCached(session, model))
            {
                CacheFactory.RemoveAllTableCache(GetTableName(session, model));
            }
            return Delete(conn, session.Transaction, sqlText.ToString(), ValueList.ToArray());
        }
        /// <summary>
        /// 自定义删除
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Delete(DbConnection conn, DbTransaction dbTransaction, String sql, DbParameter[] parameters)
        {
            return executeSqlNotQuery(conn, dbTransaction, sql, parameters);
        }

        /// <summary>
        /// 默认以主键更新
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int Update(DBSession session)
        {
            return Update(session, true);
        }

        /// <summary>
        /// 默认修改,通过主键修改其他列(OrmModel使用)
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int Update(DBSession session, bool onlyByPk)
        {
            if (!(this is OrmBaseModel))
            {
                throw new LTCingFWException("此方法仅供OrmModel使用！");
            }
            return Update(session, null, onlyByPk);
        }

        /// <summary>
        /// 依据主键更新
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(DBSession session, OrmBaseModel model)
        {
            return Update( session,  model, true);
        }

        /// <summary>
        /// 默认修改,通过主键修改其他列
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public int Update(DBSession session, OrmBaseModel model, bool onlyByPk)
        {
            if (model == null && this is OrmBaseModel)
            {
                model = (OrmBaseModel)this;
            }
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DbConnection conn = session.Connection;
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" UPDATE ").Append(GetTableName(session, model)).Append(" SET ").Append(GetUpdateSetColumnStr(session, model));
            List<DbParameter> ValueList = new List<DbParameter>();
            GetUpdateSetColumnValues(session, model, ValueList);
            sqlText.Append(" WHERE 1=1 ");
            SetModelWhereSqlTextAndValues(session, model, sqlText, ValueList, onlyByPk);
            //清除缓存
            if (CacheFactory.IsCached(session, model))
            {
                CacheFactory.RemoveAllTableCache(GetTableName(session, model));
            }
            return Update(session, sqlText.ToString(), ValueList.ToArray());
        }
        /// <summary>
        /// 自定义修改
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Update(DBSession session, String sql, DbParameter[] parameters)
        {
            return executeSqlNotQuery(session.Connection, session.Transaction, sql, parameters);
        }

        /// <summary>
        /// 执行非查询的SQL语句
        /// </summary>
        /// <param name="conn">连接</param>
        /// <param name="sql">SQL语句</param>
        public int executeSqlNotQuery(DbConnection conn, DbTransaction dbTransaction, String sql, DbParameter[] parameters)
        {
            logger.Debug(sql);
            DbCommand cmd = conn.CreateCommand();
            if (dbTransaction != null)
            {
                cmd.Transaction = dbTransaction;
            }
            cmd.Parameters.AddRange(parameters);
            cmd.CommandText = sql;
            int n = cmd.ExecuteNonQuery();
            return n;
        }

        /// <summary>
        /// 执行非查询的SQL语句
        /// </summary>
        /// <param name="conn">连接</param>
        /// <param name="sql">SQL语句</param>
        public int executeSqlNotQuery(DBSession session, String sql)
        {
            logger.Debug(sql);
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DbCommand cmd = session.Connection.CreateCommand();
            if (session.Transaction != null)
            {
                cmd.Transaction = session.Transaction;
            }
            cmd.CommandText = sql;
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 根据完整的SQL进行查询
        /// </summary>
        /// <param name="session"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable executeSqlQuery(DBSession session, string sql)
        {
            logger.Debug(sql);
            if (session == null)
            {
                session = LTCingFWSet.GetThreadContext().DBSession;
            }
            DbDataAdapter adapter = DBSession.GetDataAdapter(session, sql);
            DbCommand cmd = session.Connection.CreateCommand();
            if (session.Transaction != null)
            {
                cmd.Transaction = session.Transaction;
            }
            DataTable resultTable = new DataTable();
            adapter.Fill(resultTable);
            return resultTable;
        }
    }
}
