using log4net;
using Oracle.ManagedDataAccess.Client;
using LTCingFW;
using LTCingFW.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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


        /// <summary>
        /// 获取查询时所有的列名的SQL字符串
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private String GetAllColumnNameStr(DBSession session, OrmBaseModel model)
        {
            StringBuilder sb = new StringBuilder();
            foreach (OrmColumnBean bean in model.OrmList)
            {
                OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                sb.Append(attr.ColName).Append(',');
            }
            sb.Remove(sb.Length - 1, 1);//去掉最后一个逗号
            return sb.ToString();
        }


        /// <summary>
        /// 获取插入时的所有列名的SQL字符串
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private String GetInsertColumnValuesStr(DBSession session, OrmBaseModel model)
        {
            StringBuilder sb = new StringBuilder();
            foreach (OrmColumnBean bean in model.OrmList)
            {
                OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                if (session.Connection is OracleConnection)
                {
                        sb.Append(':').Append(attr.ColName).Append(',');
                }
                if (session.Connection is SqlConnection)
                {
                    sb.Append('@').Append(attr.ColName).Append(',');
                }

            }
            sb.Remove(sb.Length - 1, 1);//去掉最后一个逗号
            return sb.ToString();
        }

        /// <summary>
        /// 获取更新时的SET语句的SQL字符串
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private String GetUpdateSetColumnStr(DBSession session, OrmBaseModel model)
        {
            StringBuilder sb = new StringBuilder();
            foreach (OrmColumnBean bean in model.OrmList)
            {
                OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                if (!attr.PrimaryKey)
                {
                    if (session.Connection is OracleConnection)
                    {
                            sb.Append(" ").Append(attr.ColName).Append('=').Append(':').Append(attr.ColName).Append(",");
                    }
                    if (session.Connection is SqlConnection)
                    {
                        sb.Append(" ").Append(attr.ColName).Append('=').Append('@').Append(attr.ColName).Append(",");
                    }
                }
            }
            sb.Remove(sb.Length - 1, 1);//去掉最后一个逗号
            return sb.ToString();
        }

        /// <summary>
        /// 获取更新时的SET值列表
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private void GetUpdateSetColumnValues(DBSession session, OrmBaseModel model, List<DbParameter> ValueList)
        {
            foreach (OrmColumnBean bean in model.OrmList)
            {
                DbParameter param = null;
                OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];

                //oracle
                if (session.Connection is OracleConnection)
                {
                    if (!attr.PrimaryKey)
                    {
                        if (attr.ColSize == 0)
                        {
                            param = new OracleParameter(":" + attr.ColName, (OracleDbType)attr.ColType);
                        }
                        else
                        {
                            param = new OracleParameter(":" + attr.ColName, (OracleDbType)attr.ColType, attr.ColSize);
                        }
                        param.Value = getProperDbParameterValue( bean.Value, attr.ColType);
                        ValueList.Add(param);
                    }

                }
                //sqlserver
                if (session.Connection is SqlConnection)
                {
                    if (!attr.PrimaryKey)
                    {
                        if (attr.ColSize == 0)
                        {
                            param = new SqlParameter("@" + attr.ColName, (SqlDbType)attr.ColType);
                        }
                        else
                        {
                            param = new SqlParameter("@" + attr.ColName, (SqlDbType)attr.ColType, attr.ColSize);
                        }
                        param.Value = getProperDbParameterValue(bean.Value,attr.ColType);
                        ValueList.Add(param);
                    }
                }
                //mysql未实现


            }
        }
        /// <summary>
        /// DbParameter值转换
        /// </summary>
        /// <param name="value">原值，多为字符</param>
        /// <param name="colType">db数据类型</param>
        /// <returns></returns>
        private object getProperDbParameterValue( object value ,int colType) {
            //空
            if (value == null) {
                return System.DBNull.Value;
            }
            //以字符串模拟其他类型
            if (value is string)
            {
                string val = value as string;
                if (val.Trim() == "")
                {
                    return System.DBNull.Value;
                }
                //Oracle Type
                if (FwUtilFunc.oracleTypeIsString(colType))
                {
                    return value;
                }
                else if (FwUtilFunc.oracleTypeIsNumber(colType))
                {
                    return Decimal.Parse(val);
                }
                else if (FwUtilFunc.oracleTypeIsDate(colType))
                {
                    return Convert.ToDateTime(val);//使用DateTime
                }
                //SqlServer Type
                else if (FwUtilFunc.sqlserverTypeIsString(colType))
                {
                    return value;
                }
                else if (FwUtilFunc.sqlserverTypeIsNumber(colType))
                {
                    return Decimal.Parse(val);
                }
                else if (FwUtilFunc.sqlserverTypeIsDate(colType))
                {
                    return Convert.ToDateTime(val);//使用DateTime
                }
                //MySql未实现
                else {
                    throw new LTCingFWException(String.Format("无法将String类型的模拟值转换为数据库的{0}类型",colType));
                }
            }
            else {
                logger.Info(String.Format("对列类型{0}不做处理，请自行保证值可以使用!", colType));
                return value;
            }
        }
        //默认与主键无关，用于查询
        private void SetModelWhereSqlTextAndValues(DBSession session, OrmBaseModel model, StringBuilder sqlText, List<DbParameter> ValueList, bool onlyPrimaryKey)
        {
            SetModelWhereSqlTextAndValues(session, model, sqlText, ValueList, onlyPrimaryKey, false);
        }

        /// <summary>
        /// 获取查询的WHERE部分sql语句和Values
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <param name="sqlText"></param>
        /// <param name="ValueList"></param>
        /// <param name="onlyPrimaryKey">是否只按照主键查询</param>
        /// <param name="fuzzy">是否为模糊查询</param>
        private void SetModelWhereSqlTextAndValues(DBSession session, OrmBaseModel model, StringBuilder sqlText, List<DbParameter> ValueList, bool onlyPrimaryKey, bool fuzzy)
        {
            bool hasPrimaryKey = false;
            //遍历属性值
            foreach (OrmColumnBean bean in model.OrmList)
            {
                
                if (FwUtilFunc.StringIsNotEmpty((string)bean.Value))
                {
                    DbParameter param = null;
                    OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                    if (attr.PrimaryKey && bean.Value != null )
                    {
                        hasPrimaryKey = true;
                    }
                    //oracle
                    if (session.Connection is OracleConnection)
                    {
                        if (onlyPrimaryKey && !attr.PrimaryKey) { }
                        else 
                        {
                            if (attr.ColSize == 0)
                            {
                                param = new OracleParameter(":" + attr.ColName, (OracleDbType)attr.ColType);
                            }
                            else
                            {
                                param = new OracleParameter(":" + attr.ColName, (OracleDbType)attr.ColType, attr.ColSize);
                            }
                            if (FwUtilFunc.oracleTypeIsString(attr.ColType))
                            {
                                if (fuzzy)
                                {
                                    sqlText.Append(" AND ").Append(attr.ColName).Append(" LIKE :").Append(attr.ColName);
                                    param.Value = "%" + getProperDbParameterValue(bean.Value,attr.ColType) + "%";
                                    ValueList.Add(param);
                                }
                                else
                                {
                                    sqlText.Append(" AND ").Append(attr.ColName).Append(" = :").Append(attr.ColName);
                                    param.Value = getProperDbParameterValue(bean.Value, attr.ColType);
                                    ValueList.Add(param);
                                }
                            }
                            else //(oracleTypeIsNumber(attr.ColType))
                            {
                                sqlText.Append(" AND ").Append(attr.ColName).Append(" = :").Append(attr.ColName);
                                param.Value = getProperDbParameterValue(bean.Value, attr.ColType);
                                ValueList.Add(param);
                            }
                            
                        }

                    }
                    //sqlserver
                    if (session.Connection is SqlConnection)
                    {
                        if (onlyPrimaryKey && !attr.PrimaryKey) { }
                        else 
                        {
                            if (attr.ColSize == 0)
                            {
                                param = new SqlParameter("@" + attr.ColName, (SqlDbType)attr.ColType);
                            }
                            else
                            {
                                param = new SqlParameter("@" + attr.ColName, (SqlDbType)attr.ColType, attr.ColSize);
                            }
                            if (FwUtilFunc.sqlserverTypeIsString(attr.ColType))
                            {
                                if (fuzzy)
                                {
                                    sqlText.Append(" AND ").Append(attr.ColName).Append(" LIKE @").Append(attr.ColName);
                                    param.Value = "%" + getProperDbParameterValue(bean.Value, attr.ColType) + "%";
                                    ValueList.Add(param);
                                }
                                else
                                {
                                    sqlText.Append(" AND ").Append(attr.ColName).Append(" = @").Append(attr.ColName);
                                    param.Value = getProperDbParameterValue(bean.Value, attr.ColType);
                                    ValueList.Add(param);
                                }
                            }
                            else //(sqlserverTypeIsNumber(attr.ColType))
                            {
                                sqlText.Append(" AND ").Append(attr.ColName).Append(" = @").Append(attr.ColName);
                                param.Value = getProperDbParameterValue(bean.Value, attr.ColType);
                                ValueList.Add(param);
                            }
                            
                        }

                    }
                    //mysql未实现
                }
            }
            if (onlyPrimaryKey && !hasPrimaryKey) {
                throw new LTCingFWException("按照主键查询未找到主键或主键值为空！");
            }

        }

        public int GetItemCount(DBSession session, OrmBaseModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT COUNT(1) FROM ").Append(GetTableName(session, model)).Append(" WHERE 1=1 ");
            List<DbParameter> ValueList = new List<DbParameter>();
            SetModelWhereSqlTextAndValues(session, model, sql, ValueList, false,true);
            DbCommand cmd = session.Connection.CreateCommand();
            cmd.CommandText = sql.ToString();
            cmd.Parameters.AddRange(ValueList.ToArray());
            object result = cmd.ExecuteScalar();

            return (int)(Decimal)result;
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
            int max_limit = model.UpLimitNumber;
            int min_limit = model.LowLimitNumber;
            String allColumnString = GetAllColumnNameStr(session, model);
            StringBuilder sb = new StringBuilder();

            //oracle
            if (session.Connection is OracleConnection)
            {
                if (FwUtilFunc.StringIsNotEmpty(model.OrderBy))
                {
                    sql = sql + " ORDER BY " + model.OrderBy;
                }
                sb.Append(" SELECT ").Append(allColumnString).Append(" FROM ");
                sb.Append(" ( SELECT ").Append(" ROWNUM RN , ").Append(allColumnString);
                sb.Append(" FROM ( ").Append(sql).Append(" ) ").Append(" WHERE ROWNUM <= ").Append(max_limit).Append(" ) ");
                sb.Append(" WHERE RN >= ").Append(min_limit);

            }
            //sqlserver
            if (session.Connection is SqlConnection)
            {
                sb.Append(" SELECT ").Append(allColumnString).Append(" FROM ");
                sb.Append(" ( SELECT TOP ").Append(max_limit).Append(" ROW_NUMBER() OVER(").Append(model.OrderBy).Append(" ) RN, ");
                sb.Append(allColumnString).Append(" FROM ( ").Append(sql).Append(" ) ) w2 ");
                sb.Append(" WHERE w2.RN >= ").Append(min_limit);
                sb.Append(" ORDER BY w2.n ASC ");

            }

            return sb.ToString();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns>DataTable</returns>
        public DataTable SelectPage(DBSession session, OrmBaseModel model)
        {
            DbConnection conn = session.Connection;
            List<DbParameter> ValueList = new List<DbParameter>();
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" SELECT ");
            if (model.Distinct)
            {
                sqlText.Append(" DISTINCT ");
            }
            sqlText.Append(GetAllColumnNameStr(session, model)).Append(" FROM ").Append(GetTableName(session, model)).Append(" WHERE 1=1 ");
            SetModelWhereSqlTextAndValues(session, model, sqlText, ValueList, false, true);
            if (FwUtilFunc.StringIsNotEmpty(model.Where))
            {
                sqlText.Append(" AND ").Append(model.Where);
            }

            String pageSql = addPaginationSql(sqlText.ToString(), session, model);
            #region 缓存
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
            return Select(conn, session.Transaction, pageSql, ValueList.ToArray());
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns>List<T></returns>
        public List<T> SelectPage<T>(DBSession session, OrmBaseModel model) where T:OrmBaseModel{

            DataTable res = SelectPage( session,  model);
            List<T> resT = FwUtilFunc.LoadOrmModelListFromDataTable<T>(res);
            foreach (T ormModel in resT)
            {
                QueryForForeignOrmModel(session, ormModel);
            }
            return resT;
        }

        /// <summary>
        /// 默认选择,根据有值列
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public DataTable Select(DBSession session, OrmBaseModel model)
        {
            DbConnection conn = session.Connection;
            List<DbParameter> ValueList = new List<DbParameter>();
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" SELECT ");
            if (model.Distinct)
            {
                sqlText.Append(" DISTINCT ");
            }
            if (model.SqlServerTop != 0) {
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
                    DataTable table = Select(conn, session.Transaction, sqlText.ToString(), ValueList.ToArray()).Copy();
                    CacheFactory.SetTableCache(sqlText.ToString(), table);
                    return table;
                }
            }
            #endregion
            return Select(conn,session.Transaction, sqlText.ToString(), ValueList.ToArray());
        }

        /// <summary>
        /// 默认选择,根据有值列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<T> Select<T>(DBSession session, OrmBaseModel model) where T : OrmBaseModel
        {

            DataTable res = Select(session, model);
            List<T> resT = FwUtilFunc.LoadOrmModelListFromDataTable<T>(res);
            foreach (T ormModel in resT)
            {
                QueryForForeignOrmModel(session, ormModel);
            }
            return resT;
        }
        /// <summary>
        /// 通过主键查询
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public DataTable SelectByPrimaryKey(DBSession session, OrmBaseModel model)
        {
            DbConnection conn = session.Connection;
            List<DbParameter> ValueList = new List<DbParameter>();
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" SELECT ");
            if (model.Distinct) {
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
            return Select(conn,session.Transaction, sqlText.ToString(), ValueList.ToArray());
        }
        /// <summary>
        /// 通过主键查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<T> SelectByPrimaryKey<T>(DBSession session, OrmBaseModel model) where T : OrmBaseModel
        {
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
        public void QueryForForeignOrmModel(DBSession session, OrmBaseModel resModel) {

            PropertyInfo[] infos = resModel.GetType().GetProperties();
            foreach (PropertyInfo info in infos) {
                //1.检查是否含有【OrmForeignAttribute】特性,未设置DbAlise
                object[] attrs = info.GetCustomAttributes(typeof(OrmForeignAttribute), true);
                if (attrs.Length == 0) {
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
                    if (infoType.BaseType.FullName !="LTCingFW.OrmBaseModel")
                    {
                        throw new Exception(resModel.GetType().Name + "为非法类型，请检查关联OrmModel的类型");
                    }
                }
                else {
                    throw new Exception(resModel.GetType().Name+ "为非法类型，请检查关联OrmModel的类型");
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
                for (int i=0; i<localColumnNames.Length; i++)
                {
                    object value = FwUtilFunc.GetObjectPropertyValue(resModel, localColumnNames[i]);
                    FwUtilFunc.SetObjectPropertyValue(inst, foreignColumnNames[i], value);
                }
                //3.3 查询
                DataTable result = Select(session, inst as OrmBaseModel);
                //3.4 通过反射调用LoadOrmModelListFromDataTable<T>
                MethodInfo Load_Method  = typeof(FwUtilFunc).GetMethod("LoadOrmModelListFromDataTable", new Type[] { typeof(DataTable) });
                MethodInfo Cur_Load_Method = Load_Method.MakeGenericMethod(infoType);
                //Final_Result is List<OrmBaseModel>
                object Final_Result = Cur_Load_Method.Invoke(null, new object[] { result });

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
        public DataTable Select(DbConnection conn, DbTransaction dbTransaction, String sql, DbParameter[] parameters)
        {
            logger.Debug(sql);
            DbDataAdapter adapter = DbConnectionFactory.GetDataAdapter(conn, sql);
            if (dbTransaction != null)
            {
                adapter.SelectCommand.Transaction = dbTransaction;
            }
            adapter.SelectCommand.Parameters.AddRange(parameters);
            DataTable resultTable = new DataTable();
            adapter.Fill(resultTable);
            return resultTable;
        }

        /// <summary>
        /// 默认插入,全列插入
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public int Insert(DBSession session, OrmBaseModel model)
        {
            DbConnection conn = session.Connection;
            String dbType = FwUtilFunc.GetDBTypeByConnection(conn);
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" INSERT INTO ").Append(GetTableName(session, model)).Append("(").Append(GetAllColumnNameStr(session, model)).Append(" ) ").Append(" VALUES ( ");
            List<DbParameter> ValueList = new List<DbParameter>();

            foreach (OrmColumnBean bean in model.OrmList)
            {

                DbParameter param = null;
                OrmColumnAttribute attr = bean.OrmColumnAttributeDic[session.DbAlias];
                //oracle
                if (conn is OracleConnection)
                {

                    sqlText.Append(" :").Append(attr.ColName).Append(",");
                    if (attr.ColSize == 0)
                    {
                        param = new OracleParameter(":" + attr.ColName, (OracleDbType)attr.ColType);
                        param.Value = getProperDbParameterValue( bean.Value, attr.ColType);
                        ValueList.Add(param);
                    }
                    else
                    {
                        param = new OracleParameter(":" + attr.ColName, (OracleDbType)attr.ColType, attr.ColSize);
                        param.Value = getProperDbParameterValue( bean.Value, attr.ColType);
                        ValueList.Add(param);
                    }

                }
                //sqlserver
                if (conn is SqlConnection)
                {
                    sqlText.Append(" @").Append(attr.ColName).Append(",");
                    if (attr.ColSize == 0)
                    {
                        param = new SqlParameter("@" + attr.ColName, (SqlDbType)attr.ColType);
                        param.Value = getProperDbParameterValue( bean.Value, attr.ColType);
                        ValueList.Add(param);
                    }
                    else
                    {
                        param = new SqlParameter("@" + attr.ColName, (SqlDbType)attr.ColType, attr.ColSize);
                        param.Value = getProperDbParameterValue( bean.Value, attr.ColType);
                        ValueList.Add(param);
                    }
                }
                //mysql未实现



            }
            sqlText.Remove(sqlText.Length - 1, 1);
            sqlText.Append(")");

            //清除缓存
            if (CacheFactory.IsCached(session,model)) {
                CacheFactory.RemoveAllTableCache(GetTableName(session, model));
            }
            //执行
            return Insert(conn, session.Transaction, sqlText.ToString(), ValueList.ToArray());
        }

        /// <summary>
        /// 自定义插入
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Insert(DbConnection conn, DbTransaction dbTransaction, String sql, DbParameter[] parameters)
        {
            return executeSqlNotQuery(conn, dbTransaction, sql, parameters);
        }


        /// <summary>
        /// 默认删除,根据有值列
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public int Delete(DBSession session, OrmBaseModel model)
        {
            DbConnection conn = session.Connection;
            String dbType = FwUtilFunc.GetDBTypeByConnection(conn);
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" DELETE FROM ").Append(GetTableName(session, model)).Append(" WHERE 1=1 ");
            List<DbParameter> ValueList = new List<DbParameter>();
            SetModelWhereSqlTextAndValues(session, model, sqlText, ValueList, false);
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
        /// 默认修改,通过主键修改其他列
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public int Update(DBSession session, OrmBaseModel model)
        {
            DbConnection conn = session.Connection;
            String dbType = FwUtilFunc.GetDBTypeByConnection(conn);
            StringBuilder sqlText = new StringBuilder();
            sqlText.Append(" UPDATE ").Append(GetTableName(session, model)).Append(" SET ").Append(GetUpdateSetColumnStr(session, model));
            List<DbParameter> ValueList = new List<DbParameter>();
            GetUpdateSetColumnValues(session, model, ValueList);
            sqlText.Append(" WHERE 1=1 ");
            SetModelWhereSqlTextAndValues(session, model, sqlText, ValueList, true);
            //清除缓存
            if (CacheFactory.IsCached(session, model))
            {
                CacheFactory.RemoveAllTableCache(GetTableName(session, model));
            }
            return Update(conn, session.Transaction, sqlText.ToString(), ValueList.ToArray());
        }
        /// <summary>
        /// 自定义修改
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Update(DbConnection conn, DbTransaction dbTransaction, String sql, DbParameter[] parameters)
        {
            return executeSqlNotQuery( conn,  dbTransaction,  sql,  parameters);
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
        public void executeSqlNotQuery(DBSession session, String sql)
        {
            logger.Debug(sql);
            DbCommand cmd = session.Connection.CreateCommand();
            if (session.Transaction != null)
            {
                cmd.Transaction = session.Transaction;
            }
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
