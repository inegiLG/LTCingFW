using LTCingFW.beans;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace LTCingFW
{
    public class LTCingFWSet
    {
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

    }
}
