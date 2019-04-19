using log4net;
using LTCingFW.opc;
using Microsoft.Win32;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using static LTCingFW.OrmDataType;
using OracleDbType = LTCingFW.OrmDataType.OracleDbType;
using SqlDbType = LTCingFW.OrmDataType.SqlDbType;

namespace LTCingFW.utils
{
    public static class FwUtilFunc
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(LTCingFWApp));
        /// <summary>
        /// 字符串不为空
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool StringIsNotEmpty(String r)
        {

            return !StringIsEmpty(r);
        }
        /// <summary>
        /// 字符串为空
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool StringIsEmpty(String r)
        {

            if (r == null || r.Trim().Equals(""))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// 根据路径删除文件
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFileOrDirectory(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            if (attr == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
            }
            else
            {
                File.Delete(path);
            }
        }




        /// <summary>
        /// 通过Connection获取DB类型
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static String GetDBTypeByConnection(DbConnection conn) {
            //oracle
            if (conn is OracleConnection) {
                return DB_Leaf.OracleDBType;
            }
            //sqlserver
            if (conn is SqlConnection) {
                return DB_Leaf.SqlServerDBType;
            }
            //mysql未实现
            return null;
        }
        /// <summary>
        /// 获取OrmModel的属性值
        /// </summary>
        /// <param name="model"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static object GetObjectPropertyValue(object model,string propertyName)
        {
            PropertyInfo info = model.GetType().GetProperty(propertyName);
            object value = info.GetValue(model);
            return value;
        }
        /// <summary>
        /// 设置OrmModel的属性值
        /// </summary>
        /// <param name="model"></param>
        /// <param name="propertyName"></param>
        public static void SetObjectPropertyValue(object model, string propertyName,object value)
        {
            PropertyInfo info = model.GetType().GetProperty(propertyName, BindingFlags.Instance| BindingFlags.Public);
            info.SetValue(model,value);
        }
        /// <summary>
        /// 为OrmBaseModel的所有同名属性赋值
        /// </summary>
        /// <param name="from">源实例</param>
        /// <param name="model">OrmBaseModel实例</param>
        public static void AutoSetOrmModelProperty(object from ,OrmBaseModel to) {

            PropertyInfo[] infos = to.GetType().GetProperties();
            foreach (PropertyInfo t_info in infos)
            {
                object[] attrs = t_info.GetCustomAttributes(typeof(OrmColumnAttribute), true);
                if (attrs.Length == 0 )
                {
                    if (t_info.Name != "OrderBy") {
                        continue;
                    }
                    
                }

                PropertyInfo f_info = from.GetType().GetProperty(t_info.Name);
                if (f_info != null)
                {
                    if (f_info.Name == "OrderBy") {
                        t_info.SetValue(to, f_info.GetValue(from));
                    }

                    t_info.SetValue(to, f_info.GetValue(from));
                }

            }
            FieldInfo[] finfos = to.GetType().GetFields(BindingFlags.NonPublic|BindingFlags.Instance);
            foreach (FieldInfo t_info in finfos) {
                FieldInfo f_info = from.GetType().GetField(t_info.Name, BindingFlags.NonPublic| BindingFlags.Instance);
                if (f_info != null)
                {
                    t_info.SetValue(to, f_info.GetValue(from));
                }
            }

        }
        /// <summary>
        /// 根据列名传输行数据
        /// </summary>
        /// <param name="data_from"></param>
        /// <param name="data_to"></param>
        public static void TransferDataTable(DataTable data_from, DataTable data_to)
        {

            if (data_from != null)
            {
                for (int i = 0; i < data_from.Rows.Count; i++)
                {
                    DataRow row = data_to.Rows.Add();
                    for (int j = 0; j < data_from.Columns.Count; j++)
                    {
                        String colName = data_from.Columns[j].ColumnName;
                        if (!data_to.Columns.Contains(colName))
                        {
                            logger.Warn("接收端DataTable[" + data_to.TableName + "]中不包含列" + colName);
                        }
                        else
                        {
                            row[colName] = data_from.Rows[i][colName];
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 将DataGridViewRow的一行赋值给OrmModel
        /// </summary>
        /// <param name="row"></param>
        /// <param name="to"></param>
        public static void AutoSetOrmModelFromDataGridViewRow(DataGridViewRow row, OrmBaseModel to)
        {
            PropertyInfo[] infos = to.GetType().GetProperties();
            foreach (PropertyInfo t_info in infos)
            {
                object[] attrs = t_info.GetCustomAttributes(typeof(OrmColumnAttribute), true);
                if (attrs.Length == 0)
                {
                    continue;
                }
                try
                {
                    DataGridViewCell cell = row.Cells["col_" + t_info.Name];
                    object value = row.Cells["col_" + t_info.Name].Value.ToString();
                    t_info.SetValue(to, value);
                }
                catch (System.ArgumentException ex)
                {
                    logger.Warn(ex.Message+":"+ex.StackTrace);
                }
                
                

            }
        }

        /// <summary>
        /// 根据同名属性或字段转移数据
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void TransferDataBetweenObject(object f_obj ,object t_obj) {
            //全部属性赋值
            PropertyInfo[] f_p_infos = f_obj.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            PropertyInfo[] t_p_infos = t_obj.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (PropertyInfo t_info in t_p_infos)
            {
                string t_info_name = t_info.Name;
                Type t_info_type = t_info.PropertyType;

                foreach (PropertyInfo f_info in f_p_infos)
                {
                    if (f_info.Name == t_info_name && f_info.PropertyType == t_info_type)
                    {
                        object value = f_info.GetValue(f_obj);
                        if (t_info.GetSetMethod() != null)
                        {
                            t_info.SetValue(t_obj, value);
                        }

                    }
                }
            }
            //全部字段赋值
            FieldInfo[] f_f_infos = f_obj.GetType().GetFields();
            FieldInfo[] t_f_infos = t_obj.GetType().GetFields();
            foreach (FieldInfo t_info in t_f_infos)
            {
                string t_info_name = t_info.Name;
                Type t_info_type = t_info.FieldType;

                foreach (FieldInfo f_info in f_f_infos)
                {
                    if (f_info.Name == t_info_name && f_info.FieldType == t_info_type)
                    {
                        object value = f_info.GetValue(f_obj);
                        t_info.SetValue(t_obj, value);
                    }
                }
            }


        }


        /// <summary>
        /// 将DataTable的数据导入成为某OrmModel的集合
        /// </summary>
        /// <typeparam name="MT">OrmModel类型</typeparam>
        /// <param name="table">DataTable结果集</param>
        /// <returns></returns>
        public  static List<MT> LoadOrmModelListFromDataTable<MT>(DataTable table) {
            List<MT> modelList = new List<MT>();
            PropertyInfo[] infos = typeof(MT).GetProperties();

            try
            {
                foreach (DataRow row in table.Rows)
                {

                    MT model = (MT)typeof(MT).Assembly.CreateInstance(typeof(MT).FullName);

                    foreach (PropertyInfo info in infos)
                    {
                        object[] attrs = info.GetCustomAttributes(typeof(OrmColumnAttribute), true);
                        if (attrs.Length == 0)
                        {
                            continue;
                        }
                        object value = row[info.Name];
                        if (value is System.DBNull)
                        {
                            info.SetValue(model, null); //NULL
                        }
                        else if (info.PropertyType == typeof(string))
                        {
                            if (value is System.DateTime)
                            {
                                //Date
                                info.SetValue(model, (Convert.ToDateTime(value)).ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                            else if (value is System.Boolean)
                            {
                                //Boolean
                                if ((bool)value)
                                {
                                    info.SetValue(model, "1");
                                }
                                else
                                {
                                    info.SetValue(model, "0");
                                }
                            }
                            else
                            {
                                //Decimal和String
                                info.SetValue(model, Convert.ToString(value));
                            }
                        }
                        else if (info.PropertyType == typeof(int))
                        {
                            info.SetValue(model, Convert.ToInt32(value));
                        }
                        else if (info.PropertyType == typeof(float))
                        {
                            info.SetValue(model, Convert.ToSingle(value));
                        }
                        else if (info.PropertyType == typeof(double))
                        {
                            info.SetValue(model, Convert.ToDouble(value));
                        }
                        else if (info.PropertyType == typeof(decimal))
                        {
                            info.SetValue(model, Convert.ToDecimal(value));
                        }
                        else if (info.PropertyType == typeof(short))
                        {
                            info.SetValue(model, Convert.ToInt16(value));
                        }
                        else if (info.PropertyType == typeof(long))
                        {
                            info.SetValue(model, Convert.ToInt64(value));
                        }
                        else if (info.PropertyType == typeof(char))
                        {
                            info.SetValue(model, Convert.ToChar(value));
                        }
                        else if (info.PropertyType == typeof(bool))
                        {
                            info.SetValue(model, Convert.ToBoolean(value));
                        }
                        else if (info.PropertyType == typeof(DateTime))
                        {
                            info.SetValue(model, Convert.ToDateTime(value));
                        }
                        else if (info.PropertyType == typeof(Byte))
                        {
                            info.SetValue(model, Convert.ToByte(value));
                        }
                        else if (info.PropertyType == typeof(SByte))
                        {
                            info.SetValue(model, Convert.ToSByte(value));
                        }
                        else
                        {
                            info.SetValue(model, value);
                        }
                    }
                    modelList.Add(model);
                }
            }
            catch (Exception ex )
            {
                throw new LTCingFWException("DataTable->OrmModel出错："+ex.Message+ex.StackTrace);
            }
            

            return modelList;
        }

        /// <summary>
        /// 比较两个DataTable内容是否相等
        /// </summary>
        /// <param name="table1"></param>
        /// <param name="table2"></param>
        /// <returns></returns>
        public static bool EqualsForDataTableContext(object table1 , object table2) {

            if (table1 == null && table2 == null)
            {
                return true;
            }
            else if (table1 == null && table2 != null)
            {
                return false;
            }
            else if (table1 != null && table2 == null)
            {
                return false;
            }
            else {
                try
                {
                    DataTable tb1 = (DataTable)table1 ;
                    DataTable tb2 = (DataTable)table2 ;
                    //1.比较列结构
                    if (tb1.Columns.Count != tb2.Columns.Count)
                    {
                        return false;
                    }
                    for (int i = 0; i < tb1.Columns.Count; i++)
                    {
                        if (tb1.Columns[i].ColumnName != tb2.Columns[i].ColumnName)
                        {
                            return false;
                        }
                        if (!tb1.Columns[i].DataType.Equals(tb2.Columns[i].DataType))
                        {
                            return false;
                        }
                    }
                    //2.比较行内容
                    if (tb1.Rows.Count != tb2.Rows.Count)
                    {
                        return false;
                    }
                    for (int i = 0; i < tb1.Rows.Count; i++)
                    {
                        for (int j = 0; j < tb1.Columns.Count; j++)
                        {
                            if (!tb1.Rows[i][j].Equals(tb2.Rows[i][j]))
                            {
                                return false;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
               
            }
            return true;
        }



        #region valid验证使用
        /// <summary>
        /// 验证Form的ViewModel
        /// </summary>
        /// <param name="form">BaseForm</param>
        /// <returns></returns>
        public static bool ValidForm(BaseForm form)
        {

            bool validFlag = true;
            List<ValidResult> validList = ValidationFunc.ValidAll(form.Model);
            if (validList == null)
            {
                MessageBox.Show("验证内部错误！");
                validFlag = false;
            }
            else
            {
                foreach (ValidResult ret in validList)
                {
                    if (ret.Result)
                    {
                        //消除验证结果信息
                        RemoveValidLabel(form, ret);
                    }
                    else
                    {
                        //添加验证结果信息
                        CreateValidLabel(form, ret);
                        validFlag = false;
                    }
                }
            }
            return validFlag;
        }
        /// <summary>
        /// 创建验证的ValidLabel
        /// </summary>
        /// <param name="formName">Form名</param>
        /// <param name="ret"></param>
        public static void CreateValidLabel(string formName, ValidResult ret) {
            CreateValidLabel((Form)FWAppContainer.getProperty(formName),ret);
        }
        /// <summary>
        /// 创建验证的ValidLabel
        /// </summary>
        /// <param name="form"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static void CreateValidLabel(Form form, ValidResult ret)
        {
            try
            {

                Label l = (Label)GetFormControl(form, "valid_label_" + ret.ProPertyName);
                if (l == null)
                {
                    l = new Label();
                }
                l.Name = "valid_label_" + ret.ProPertyName;
                l.Text = ret.ErrorMessage;
                l.Font = new Font("宋体", 9);
                l.Size = new Size(200, 12);
                l.ForeColor = System.Drawing.Color.Red;
                Control tb = (Control)GetFormControl(form, "tb_" + ret.ProPertyName);
                l.Location = new Point(tb.Location.X - 2, tb.Location.Y + 24);
                Panel panel = (Panel)GetFormControl(form, "tb_panel");
                if (panel == null)
                {
                    form.Controls.Add(l);
                }
                else
                {
                    panel.Controls.Add(l);
                }

            }
            catch (Exception e)
            {
                logger.Warn(e.Message);
            }
        }
        /// <summary>
        /// 移除Panel中的所有ValidLabel
        /// </summary>
        /// <param name="formName">Form名</param>
        public static void RemoveAllValidLabel(string formName) {
            RemoveAllValidLabel((Form)FWAppContainer.getProperty(formName));
        }
        /// <summary>
        /// 移除Panel中的所有ValidLabel
        /// </summary>
        /// <param name="panel"></param>
        public static void RemoveAllValidLabel(Form form)
        {
            List<Control> removeList = new List<Control>();
            foreach (Control control in form.Controls)
            {
                if (control.Name.StartsWith("valid_label_"))
                {
                    removeList.Add(control);
                }
            }
            foreach (Control control in removeList)
            {
                form.Controls.Remove(control);
                control.Dispose();
            }
            removeList.Clear();
            Panel panel = (Panel)GetFormControl(form, "tb_panel");
            if (panel == null)
            {
                return;
            }
            foreach (Control control in panel.Controls)
            {
                if (control.Name.StartsWith("valid_label_"))
                {
                    removeList.Add(control);
                }
            }
            foreach (Control control in removeList)
            {
                panel.Controls.Remove(control);
                control.Dispose();
            }
        }
        /// <summary>
        /// 移除某个ValidLabel
        /// </summary>
        /// <param name="formName">Form名</param>
        /// <param name="ret"></param>
        public static void RemoveValidLabel(string formName, ValidResult ret) {
            Form f = (Form)FWAppContainer.getProperty(formName);
            if (f != null)
            {
                RemoveValidLabel(f, ret);
            }
        }
        /// <summary>
        /// 移除某个ValidLabel
        /// </summary>
        /// <param name="form">winForm</param>
        /// <param name="ret">ValidResult</param>
        public static void RemoveValidLabel(Form form, ValidResult ret)
        {
            Label l = (Label)GetFormControl(form, "valid_label_" + ret.ProPertyName);

            if (l != null)
            {
                Panel panel = (Panel)GetFormControl(form, "tb_panel");
                if (panel == null)
                {
                    form.Controls.Remove(l);
                }
                else
                {
                    panel.Controls.Remove(l);
                }
                l.Dispose();
            }
        }

        /// <summary>
        /// 获取控件
        /// </summary>
        /// <param name="form">页面</param>
        /// <param name="name">控件名</param>
        /// <returns></returns>
        public static object GetControlByName(Form form ,String name)
        {
            FieldInfo info = form.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return info.GetValue(form);
        }

        /// <summary>
        /// 从Form中获取某控件
        /// </summary>
        /// <param name="form">winForm</param>
        /// <param name="controlName">控件名</param>
        /// <returns></returns>
        public static object GetFormControl(Form form, string controlName)
        {
            Control selectedControl = null;
            Panel panel = null;
            foreach (Control control in form.Controls)
            {
                if (control.Name == controlName)
                {
                    selectedControl = control;
                }
                if (control.Name == "tb_panel") {
                    panel = (Panel)control;
                }
            }
            if (selectedControl == null)
            {
                
                if (panel == null)
                {
                    return null;
                }
                foreach (Control control in panel.Controls)
                {
                    if (control.Name == controlName)
                    {
                        selectedControl = control;
                    }
                }
            }
            return selectedControl;
        }
        #endregion




        public static bool oracleTypeIsString(int typeInt)
        {
            if ((OracleDbType)typeInt == OracleDbType.Varchar2 || (OracleDbType)typeInt == OracleDbType.NVarchar2
                || (OracleDbType)typeInt == OracleDbType.Char || (OracleDbType)typeInt == OracleDbType.NChar)
            {
                return true;
            }
            return false;
        }
        public static bool oracleTypeIsDate(int typeInt)
        {
            if ((OracleDbType)typeInt == OracleDbType.Date || (OracleDbType)typeInt == OracleDbType.TimeStamp
                || (OracleDbType)typeInt == OracleDbType.TimeStampLTZ || (OracleDbType)typeInt == OracleDbType.TimeStampTZ)
            {
                return true;
            }
            return false;
        }
        public static bool oracleTypeIsNumber(int typeInt)
        {
            if ((OracleDbType)typeInt == OracleDbType.Int16 || (OracleDbType)typeInt == OracleDbType.Int32
                || (OracleDbType)typeInt == OracleDbType.Int64 || (OracleDbType)typeInt == OracleDbType.Double
                || (OracleDbType)typeInt == OracleDbType.Decimal || (OracleDbType)typeInt == OracleDbType.Long
                || (OracleDbType)typeInt == OracleDbType.LongRaw || (OracleDbType)typeInt == OracleDbType.Raw
                || (OracleDbType)typeInt == OracleDbType.Single)
            {
                return true;
            }
            return false;
        }
        public static bool sqlserverTypeIsString(int typeInt)
        {
            if ((SqlDbType)typeInt == SqlDbType.VarChar || (SqlDbType)typeInt == SqlDbType.NVarChar
                || (SqlDbType)typeInt == SqlDbType.Char || (SqlDbType)typeInt == SqlDbType.NChar
                || (SqlDbType)typeInt == SqlDbType.Text || (SqlDbType)typeInt == SqlDbType.NText)
            {
                return true;
            }
            return false;
        }
        public static bool sqlserverTypeIsDate(int typeInt)
        {
            if ((SqlDbType)typeInt == SqlDbType.Date || (SqlDbType)typeInt == SqlDbType.DateTime
                || (SqlDbType)typeInt == SqlDbType.DateTime2 || (SqlDbType)typeInt == SqlDbType.SmallDateTime
                || (SqlDbType)typeInt == SqlDbType.Timestamp || (SqlDbType)typeInt == SqlDbType.DateTimeOffset)
            {
                return true;
            }
            return false;
        }
        public static bool sqlserverTypeIsNumber(int typeInt)
        {
            if ((SqlDbType)typeInt == SqlDbType.BigInt || (SqlDbType)typeInt == SqlDbType.Decimal
                                || (SqlDbType)typeInt == SqlDbType.Float || (SqlDbType)typeInt == SqlDbType.Real
                                || (SqlDbType)typeInt == SqlDbType.SmallInt || (SqlDbType)typeInt == SqlDbType.TinyInt
                                || (SqlDbType)typeInt == SqlDbType.Int || (SqlDbType)typeInt == SqlDbType.Bit)
            {
                return true;
            }
            return false;
        }
        public static bool mysqlTypeIsString(int typeInt)
        {
            if ((MySqlDbType)typeInt == MySqlDbType.CHAR || (MySqlDbType)typeInt == MySqlDbType.VARCHAR
                || (MySqlDbType)typeInt == MySqlDbType.TINYTEXT || (MySqlDbType)typeInt == MySqlDbType.TEXT
                || (MySqlDbType)typeInt == MySqlDbType.MEDIUMTEXT || (MySqlDbType)typeInt == MySqlDbType.LONGTEXT)
            {
                return true;
            }
            return false;
        }
        public static bool mysqlTypeIsDate(int typeInt)
        {
            if ((MySqlDbType)typeInt == MySqlDbType.DATE || (MySqlDbType)typeInt == MySqlDbType.DATETIME
                || (MySqlDbType)typeInt == MySqlDbType.TIMESTAMP || (MySqlDbType)typeInt == MySqlDbType.TIME
                || (MySqlDbType)typeInt == MySqlDbType.YEAR )
            {
                return true;
            }
            return false;
        }
        public static bool mysqlTypeIsNumber(int typeInt)
        {
            if ((MySqlDbType)typeInt == MySqlDbType.BIT || (MySqlDbType)typeInt == MySqlDbType.BOOL
                                || (MySqlDbType)typeInt == MySqlDbType.TINYINT || (MySqlDbType)typeInt == MySqlDbType.SMALLINT
                                || (MySqlDbType)typeInt == MySqlDbType.MEDIUMINT || (MySqlDbType)typeInt == MySqlDbType.INT
                                || (MySqlDbType)typeInt == MySqlDbType.BIGINT || (MySqlDbType)typeInt == MySqlDbType.FLOAT
                                || (MySqlDbType)typeInt == MySqlDbType.DOUBLE || (MySqlDbType)typeInt == MySqlDbType.DECIMAL)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// XML序列化，将类或List或DataTable转为XML
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string XmlSerialize(object obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                Type t = obj.GetType();
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(sw, obj);
                sw.Close();
                return sw.ToString();
            }
        }

        /// <summary>
        /// 按照某编码格式序列化输出到文件
        /// </summary>
        /// <param name="pth"></param>
        /// <param name="xmlobj"></param>
        public static void SerializeXMLData(string pth, object xmlobj)
        {
            XmlSerializer serializer = new XmlSerializer(xmlobj.GetType());
            using (XmlTextWriter tw = new XmlTextWriter(pth, Encoding.UTF8))
            {
                tw.Formatting = Formatting.Indented;
                serializer.Serialize(tw, xmlobj);
            }
        }

        /// <summary>
        /// XML反序列化，将xml文件转为类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T DESerializerXmlFile<T>(string filePath) where T : class
        {
            try
            {
                string str = File.ReadAllText(filePath);
                return DESerializer<T>(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// XML反序列化，将xml字符串转为类
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="strXML">xml文件字符串</param>
        /// <returns></returns>
        public static T DESerializer<T>(string strXML) where T : class
        {
            try
            {
                using (StringReader sr = new StringReader(strXML))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(sr) as T;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 根据excle的路径把第一个sheel中的内容放入datatable
        /// 注意：此方法使用需要安装AccessDatabaseEngine.exe或安装office2007以上版本
        /// </summary>
        /// <param name="path">excel存放的路径</param>
        /// <returns></returns>
        public static DataSet ReadExcelToDataSet(string path)
        {
            try
            {
                //连接字符串
                string connstring = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties='Excel 8.0;HDR=NO;IMEX=1';"; // Office 07及以上版本 不能出现多余的空格 而且分号注意
                DataSet set = new DataSet();                                                                                                                             //string connstring = Provider=Microsoft.JET.OLEDB.4.0;Data Source=" + path + ";Extended Properties='Excel 8.0;HDR=NO;IMEX=1';"; //Office 07以下版本 
                using (OleDbConnection conn = new OleDbConnection(connstring))
                {
                    conn.Open();
                    DataTable sheetsName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null); //得到所有sheet的名字

                    for (int i = 0; i < sheetsName.Rows.Count; i++)
                    {
                        string sheetName = sheetsName.Rows[i]["Table_Name"].ToString(); //得到第一个sheet的名字
                        string sql = string.Format("SELECT * FROM [{0}]", sheetName); //string sql = string.Format("SELECT * FROM [{0}] WHERE [日期] is not null", firstSheetName); //查询字符串
                        OleDbDataAdapter ada = new OleDbDataAdapter(sql, connstring);
                        ada.Fill(set, sheetName);
                    }

                    return set;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public static string GetRegularReturnType(Type ReturnType)
        {
            string returnStr = ReturnType.FullName;
            //Void
            if (ReturnType.Name == "Void") {
                returnStr = "void";
            }
            //List<T>
            if (ReturnType.Name == "List`1") {
                Type[] gs = ReturnType.GetGenericArguments();
                StringBuilder sb = new StringBuilder();
                sb.Append("System.Collections.Generic.List<");
                foreach (Type t in gs) {
                    if (t.Name == "List`1")
                    {
                        sb.Append(GetRegularReturnType(t)).Append(",");
                    }
                    else {
                        sb.Append(t.FullName).Append(",");
                    }
                }
                sb.Remove(sb.Length-1 ,1);
                sb.Append(">");
                returnStr = sb.ToString();
            }
            //Array
            if (ReturnType.Name.Contains("Array&"))
            {
                returnStr = "System.Array";
            }
            //ref 前缀
            if (ReturnType.IsByRef)
            {
                returnStr = "ref " + returnStr;
            }
            return returnStr;
        }

        public static string GetCommonTypeByPlcType(string plcType)
        {

            if (plcType == PLCTypeEnum.BOOL.ToString())
            {
                return OrmDataType.CommonType.BOOL.ToString();
            }
            else if (plcType == PLCTypeEnum.BYTE.ToString())
            {
                return OrmDataType.CommonType.INT.ToString();
            }
            else if (plcType == PLCTypeEnum.CHAR.ToString())
            {
                return OrmDataType.CommonType.INT.ToString();
            }
            else if (plcType == PLCTypeEnum.DATETIME.ToString())
            {
                return OrmDataType.CommonType.DATE.ToString();
            }
            else if (plcType == PLCTypeEnum.DINT.ToString())
            {
                return OrmDataType.CommonType.INT.ToString();
            }
            else if (plcType == PLCTypeEnum.DWORD.ToString())
            {
                return OrmDataType.CommonType.INT.ToString();
            }
            else if (plcType == PLCTypeEnum.INT.ToString())
            {
                return OrmDataType.CommonType.INT.ToString();
            }
            else if (plcType == PLCTypeEnum.REAL.ToString())
            {
                return OrmDataType.CommonType.DECIMAL.ToString();
            }
            else if (plcType == PLCTypeEnum.WORD.ToString())
            {
                return OrmDataType.CommonType.INT.ToString();
            }
           
            else
            {
                throw new LTCingFWException(String.Format("无法将{0}PlcType转换为CommonType", plcType));
            }
            
            
        }
        public static object GetCommonTypeValue(object value,string commonType)
        {
            if (commonType == OrmDataType.CommonType.STRING.ToString().ToUpper()) { return Convert.ToString(value); }
            else if (commonType == OrmDataType.CommonType.INT.ToString().ToUpper()) { return  Convert.ToInt32(value); }
            else if (commonType == OrmDataType.CommonType.DECIMAL.ToString().ToUpper()) { return  Convert.ToDecimal(value); }
            else if (commonType == OrmDataType.CommonType.DATE.ToString().ToUpper()) { return Convert.ToDateTime(value); }
            else if (commonType == OrmDataType.CommonType.BOOL.ToString().ToUpper()) { return Convert.ToBoolean(value); }
            else if (commonType == OrmDataType.CommonType.BINARY.ToString().ToUpper()) {
                if (value is byte[])
                {
                    return value;
                }
                else
                {
                    throw new LTCingFWException("值不是byte[]类型！");
                }
                
            }
            else { throw new LTCingFWException(commonType + "不是六种类型之一！"); }
        }



    }
}
