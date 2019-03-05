using OPCAutomation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace LTCingFW.opc
{
    public static class StaticUtils
    {
        private static int _safe_id = 1;
        private static String  _safe_id_lock = "_safe_id_lock";
        /// <summary>
        /// 获取事务号
        /// </summary>
        public static int TransActionID
        {
            get {
                return GetSafeIntID();
            }
        }

        /// <summary>
        /// ClientHandleID
        /// </summary>
        public static int ClientHandleID = 10000;

        /// <summary>
        /// 获取一个不重复的GroupName
        /// </summary>
        /// <returns></returns>
        public static String GetRandomGroupName() {
            return System.Guid.NewGuid().ToString("N");
        }


        /// <summary>
        /// 获取一个安全的Int型ID
        /// </summary>
        /// <returns></returns>
        private static int GetSafeIntID() {

            lock (_safe_id_lock)
            {
                _safe_id++;//2^31 = 2147483648
                if (_safe_id > 1000000000)
                {
                    _safe_id = 1;
                }
                return _safe_id;
            }
        }

        /// <summary>
        /// 通过OPCGroup和OPCCommand的Item比较查看是否一致
        /// </summary>
        /// <param name="command"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static bool IsOPCGroupBelongToOPCCommand(OPCCommand command,OPCGroup group) {
            if (command.ItemGroupList.Count != group.OPCItems.Count) {
                return false;
            }
            foreach (OPCItem item in group.OPCItems)
            {
                bool flag = false;
                foreach (ItemInfo info in command.ItemGroupList) {
                    if (item.ItemID == info.OPCItemID) {
                        flag = true;
                        break;
                    }
                }
                if (!flag) {
                    return false;
                }
            }
            return true;
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

    }
}
