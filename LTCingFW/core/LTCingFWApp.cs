
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.CodeDom.Compiler;
using System.IO;
using LTCingFW.utils;
using System.Threading;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;

namespace LTCingFW
{
    public class LTCingFWApp
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(LTCingFWApp));


        public static void start()
        {
            try
            {
                logger.Info("------------------------ LTCingFrameWork Starting --------------------------");
                ReadXmlConfigs();
                lookForConfigInstance_();
                lookForAttributeInstance();
                lookForConfigAspect_();
                LTCingFWProxy.createProxyForInstance();
                injectPropertytoInstance();
                logger.Info("------------------------ LTCingFrameWork Started --------------------------");
            }
            catch (Exception e)
            {
                logger.Warn("LTCingFrameWork Start Failed:" + e.Message + ":\r\n" +e.StackTrace);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        //读取配置文件
        public static void ReadXmlConfigs()
        {
            if (!System.IO.File.Exists(FWConfigs.XmlFilePath))//判断文件是否存在
            {
                throw new LTCingFWException(FWConfigs.XmlFilePath+"配置文件不存在!");
            }
            //读取配置文件
            LTCingFWSet.XmlConfigs = FwUtilFunc.DESerializerXmlFile<ConfigStructRoot>(FWConfigs.XmlFilePath);
            //载入文档
            LTCingFWSet.XmlDoc.Load(FWConfigs.XmlFilePath);
            foreach (DB_Leaf node in LTCingFWSet.XmlConfigs.DbBranch.DbLeafs)
            {
                String dbAlias = node.DbAlias.Trim();
                if (!LTCingFWSet.dbDic.Keys.Contains(dbAlias))
                {
                    DbConnectionStringBuilder dbConnectionStringBuilder = null;
                    String dbtype = node.DbType.Trim().ToLower();
                    if (dbtype == "oracle")
                    {
                        OracleConnectionStringBuilder info = new OracleConnectionStringBuilder();
                        info.DataSource = node.DataSource.Trim();
                        info.UserID = node.UserID.Trim();
                        info.Password = node.Password.Trim();
                        info.ConnectionTimeout = int.Parse(node.ConnectionTimeout.Trim());
                        info.Pooling = node.Pooling.Trim().ToLower() == "true" ? true : false;
                        info.MaxPoolSize = int.Parse(node.MaxPoolSize.Trim());
                        info.MinPoolSize = int.Parse(node.MinPoolSize.Trim());
                        info.IncrPoolSize = int.Parse(node.IncrPoolSize.Trim());
                        info.DecrPoolSize = int.Parse(node.DecrPoolSize.Trim());
                        dbConnectionStringBuilder = info;
                    }
                    else if (dbtype == "sqlserver")
                    {
                        SqlConnectionStringBuilder info = new SqlConnectionStringBuilder();
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
                        dbConnectionStringBuilder = info;
                    }
                    else
                    {
                        throw new LTCingFWException(dbtype+"为不支持的数据库类型。");

                    }
                    LTCingFWSet.dbDic.Add(dbAlias, dbConnectionStringBuilder);
                }
            }
        }

        /// <summary>
        /// 查找所有程序[特性]类
        /// </summary>
        private static void lookForAttributeInstance()
        {
            try
            {
                Assembly assembly = Assembly.GetEntryAssembly();//当前程序集
                foreach (Type type in assembly.GetTypes())
                {

                    IEnumerable<Attribute> attributes = type.GetCustomAttributes(typeof(BaseInstanceAttribute));
                    foreach (System.Attribute a in attributes)
                    {
                        //如果是实例（含有BaseInstanceAttribute），创建实例并加入到FrameWorkSet
                        if (a is BaseInstanceAttribute)
                        {
                            BaseInstanceAttribute attr = a as BaseInstanceAttribute;
                            if (FwUtilFunc.StringIsNotEmpty(attr.Name)) {
                                LTCingFWSet.Beans.Add(new FwInstanceBean(attr.Name, type.FullName));
                                logger.Info(String.Format("FrameWork find instance[{0}] with name[{1}]", type.FullName, attr.Name));
                            }
                            else if(!LTCingFWSet.Instance.ContainsKey(type.Name))
                            {
                                LTCingFWSet.Beans.Add(new FwInstanceBean(type.Name, type.FullName));
                                logger.Info(String.Format("FrameWork find instance[{0}] with name[{1}]", type.FullName, type.Name));
                            }

                        }
                    }

                }
            }
            catch (Exception e)
            {
                throw new LTCingFWException("查找特性实例出错", e);
            }
        }

        /// <summary>
        /// 依赖注入，为实例的字段自动添加依赖
        /// </summary>
        private static void injectPropertytoInstance()
        {
            try
            {
                foreach (String key in LTCingFWSet.Instance.Keys)
                {
                    Object obj = LTCingFWSet.Instance[key];
                    Type objt = obj.GetType();
                    //字段注入
                    FieldInfo[] fis = objt.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public );
                    //FieldInfo[] fis2 = objt.BaseType.GetFields(BindingFlags.NonPublic| BindingFlags.Public | BindingFlags.Instance);
                    foreach (FieldInfo info in fis)
                    {
                        var att = (InjectedAttribute[])info.GetCustomAttributes(typeof(InjectedAttribute), false);
                        InjectedAttribute attr = att.FirstOrDefault();
                        if (attr != null)
                        {
                            String instName;
                            if (attr.Name != String.Empty)
                            {
                                instName = attr.Name;
                            }
                            else
                            {
                                instName = info.FieldType.Name;
                            }
                            //检查字段依赖的对象是否存在，不存在抛出异常
                            if (!LTCingFWSet.Instance.ContainsKey(instName))
                            {
                                throw new Exception( "There is no instance of type " + instName + " for injecting " + objt.FullName + " to " + objt.FullName);
                            }
                            Object fieldobj = LTCingFWSet.Instance[instName];
                            //若字段没有值，则添加依赖
                            if (info.GetValue(obj) == null)
                            {
                                info.SetValue(obj, fieldobj);
                            }
                            logger.Info(String.Format("FrameWork inject instance[{0}] to field[{1}] of instance[{2}]", instName, info.Name, key));
                        }
                    }
                    //属性注入
                    PropertyInfo[] props = objt.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (PropertyInfo info in props)
                    {
                        var att = (InjectedAttribute[])info.GetCustomAttributes(typeof(InjectedAttribute), false);
                        InjectedAttribute attr = att.FirstOrDefault();
                        if (attr != null)
                        {
                            String instName;
                            if (attr.Name != String.Empty)
                            {
                                instName = attr.Name;
                            }
                            else
                            {
                                instName = info.PropertyType.Name;
                            }
                            //检查字段依赖的对象是否存在，不存在抛出异常
                            if (!LTCingFWSet.Instance.ContainsKey(instName))
                            {
                                throw new Exception( "There is no instance of type " + instName + " for injecting " + objt.FullName + " to " + objt.FullName);
                            }
                            Object propobj = LTCingFWSet.Instance[instName];
                            //若字段没有值，则添加依赖
                            if (info.GetValue(obj) == null)
                            {
                                info.SetValue(obj, propobj);
                            }
                            logger.Info(String.Format("FrameWork inject instance[{0}] to field[{1}] of instance[{2}]", instName, info.Name, key));
                        }
                    }
                }


            }
            catch (Exception e)
            {
                throw new LTCingFWException("实例注入出错！", e);
            }
        }


        /// <summary>
        /// 创建所有配置文件类
        /// </summary>
        private static void lookForConfigInstance_()
        {
            try
            {
                String bean_name;
                String bean_type;
                foreach (Bean_Leaf bean in LTCingFWSet.XmlConfigs.BeanBranch.BeanLeafs)
                {
                    bean_name = bean.Name;
                    bean_type = bean.Type;
                    if (LTCingFWSet.Instance.ContainsKey(bean_name))
                    {

                    }
                    else if (Assembly.GetEntryAssembly().GetType(bean_type) != null)
                    {
                        LTCingFWSet.Beans.Add(new FwInstanceBean(bean_name, bean_type));
                        logger.Info(String.Format("FrameWork find instance[{0}] with name[{1}] from config file", bean_type, bean_name));
                    }
                    else
                    {
                        throw new Exception("framework can not create bean instance of type[" + bean_type + "] from  config file.");
                    }
                }
            }
            catch (Exception e)
            {
                throw new LTCingFWException( "查找配置文件标注实例出错！", e);
            }
        }


        //}
        /// <summary>
        /// 查找AOP配置信息
        /// </summary>
        private static void lookForConfigAspect_()
        {
            try
            {
                String aspect_scope;
                String aspect_beforemethod;
                String aspect_aftermethod;
                foreach (Aspect_Leaf bean in LTCingFWSet.XmlConfigs.AspectBranch.AspectLeafs)
                {
                    aspect_scope = bean.Scope;
                    aspect_beforemethod = bean.BeforeMethod;
                    aspect_aftermethod = bean.AfterMethod;
                    FwAopBean aspect = new FwAopBean(aspect_scope, aspect_beforemethod, aspect_aftermethod);
                    LTCingFWSet.Aspects.Add(aspect);
                }
            }
            catch (Exception ex)
            {
                throw new LTCingFWException("查找配置文件标注切面出错！", ex);
            }


        }


    }
}
