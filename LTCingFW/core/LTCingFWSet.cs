using LTCingFW.beans;
using LTCingFW.thread;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace LTCingFW
{
    public class LTCingFWSet
    {

        /// <summary>
        /// 线程池
        /// </summary>
        public static Dictionary<String, ThreadInfo> ThreadPool { get; } = new Dictionary<String, ThreadInfo>();


        /// <summary>
        /// Timer线程池
        /// </summary>
        public static Dictionary<String, ITimer> DataReadTimerDic = new Dictionary<string, ITimer>();

        /// <summary>
        /// 所有的FwInstanceBean
        /// </summary>
        internal static List<FwInstanceBean> Beans { get; set; } = new List<FwInstanceBean>();

        /// <summary>
        /// 所有的FwAopBean
        /// </summary>
        internal static List<FwAopBean> Aspects { get; set; } = new List<FwAopBean>();

        /// <summary>
        /// 所有的DB连接
        /// </summary>
        internal static Dictionary<String, DB_Leaf> dbDic { get; } = new Dictionary<String, DB_Leaf>();

        /// <summary>
        /// 错误列表
        /// </summary>
        private static List<Exception> _errList { get; set; } = new List<Exception>();

        /// <summary>
        /// 线程上下文
        /// </summary>
        private static Dictionary<int, ThreadContext> _threadContextDic = new Dictionary<int, ThreadContext>();

        /// <summary>
        /// 框架所有实例集合
        /// </summary>
        private static readonly Dictionary<string, object> instanceMap = new Dictionary<string, object>();

        /// <summary>
        /// 获取实例集合
        /// </summary>
        internal static Dictionary<string, object> Instance
        {
            get
            {
                return instanceMap;
            }
        }

        /// <summary>
        /// XML配置
        /// </summary>
        internal static ConfigStructRoot XmlConfigs { get; set; }

        /// <summary>
        /// 私有化构造函数
        /// </summary>
        private LTCingFWSet(){}

        /// <summary>
        /// 外部接口，获取已经注册的实例
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        public static object GetInstanceBean(string instanceName)
        {
            if (instanceMap.ContainsKey(instanceName))
            {
                return instanceMap[instanceName];
            }
            return null;
        }

        /// <summary>
        /// 外部接口，获取线程上下文字典
        /// </summary>
        public static Dictionary<int, ThreadContext> ThreadContextDic
        {
            get
            {
                return _threadContextDic;
            }
        }

        /// <summary>
        /// 外部接口，获取错误列表
        /// </summary>
        public static List<Exception> ErrList
        {
            get {
                return _errList;
            }
        }

        /// <summary>
        /// 外部接口，获取线程上下文
        /// </summary>
        /// <param name="ThreadManagedId"></param>
        /// <returns></returns>
        public static ThreadContext GetThreadContext()
        {
            int ThreadManagedId = Thread.CurrentThread.ManagedThreadId;
            if (_threadContextDic.ContainsKey(ThreadManagedId))
            {
                return _threadContextDic[ThreadManagedId];
            }
            return null;
        }

        /// <summary>
        /// XML配置文件的对应文档
        /// </summary>
        internal static readonly XmlDocument XmlDoc = new XmlDocument();
        /// <summary>
        /// 外部接口，获取用户自定义的配置信息
        /// </summary>
        /// <param name="configPath">例：//LTCingFW/configs/sqlserverdatalocation</param>
        /// <returns></returns>
        public static string GetUserDefinedConfig(string configPath) {
            return XmlDoc.SelectSingleNode(configPath).InnerText.Trim();
        }

        public static void SaveConfigToXml(string configPath,string value) {
            XmlNode node = XmlDoc.SelectSingleNode(configPath);
            if (node != null)
            {
                node.InnerText = value;
                XmlDoc.Save(FWConfigs.XmlFilePath);
            }
        }


        public static void AddDB(string dbAlias, string dbProvider, string dbConnectionStr )
        {
            DB_Leaf leaf = new DB_Leaf();
            leaf.ProviderName = dbProvider;
            leaf.ConnectionString = dbConnectionStr;
            if (dbDic.Keys.Contains(dbAlias))
            {
                dbDic.Remove(dbAlias);
            }
            dbDic.Add(dbAlias, leaf);
        }
        public static Boolean FindDbByAlias(string alias)
        {
            if (dbDic.Keys.Contains(alias))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void AddBean(string bean_name, string bean_type) 
        {
            Assembly[] AllAssembly = AppDomain.CurrentDomain.GetAssemblies();
            bool findFlg = false;
            foreach (Assembly assem in AllAssembly)
            {
                if (assem.GetType(bean_type) != null)
                {
                    LTCingFWSet.Beans.Add(new FwInstanceBean(bean_name, bean_type, assem));
                    //logger.Info(String.Format("FrameWork find instance[{0}] with name[{1}] from config file", bean_type, bean_name));
                    findFlg = true;
                    break;
                }
            }
            if (!findFlg)
            {
                throw new Exception("framework can not create bean instance of type[" + bean_type + "] from  config file.");
            }
        }

    }
}
