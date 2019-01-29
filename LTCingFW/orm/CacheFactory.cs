using LTCingFW;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class CacheFactory
    {
        private const string CACHE_TYPE_MEMORY = "memory";
        private const string CACHE_TYPE_LOCAL = "local";
        private const string CACHE_TYPE_NET = "net";

        #region 配置
        public static string CacheSwitch { get; set; } = "off";//默认关闭

        public static string CacheType { get; set; }

        public static string LocalPosition { get; set; }

        public static string NetPosition { get; set; }
        #endregion
        //memory cache
        public static Dictionary<string,DataTable> CacheMemoryData { get;  } = new Dictionary<string, DataTable>();
        

        public static DataTable GetTabelCache(string sql) {
            lock (CacheMemoryData)
            {
                switch (CacheType)
                {
                    case CACHE_TYPE_MEMORY:
                        if (CacheMemoryData.ContainsKey(sql))
                        {
                            return CacheMemoryData[sql].Copy();
                        }
                        return null;
                    case CACHE_TYPE_LOCAL:
                        break;
                    case CACHE_TYPE_NET:
                        break;
                    default:
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// 设置CACHE
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="cache">内容</param>
        public static void SetTableCache(string sql , DataTable table) {
            lock (CacheMemoryData)
            {
                switch (CacheType)
                {
                    case CACHE_TYPE_MEMORY:
                        if (CacheMemoryData.ContainsKey(sql))
                        {
                            CacheMemoryData.Remove(sql);
                        }
                        CacheMemoryData.Add(sql, table);
                        break;
                    case CACHE_TYPE_LOCAL:
                        break;
                    case CACHE_TYPE_NET:
                        break;
                    default:
                        break;
                }
            }
            
        }

        /// <summary>
        /// 清除所有缓存，若其他系统对数据库作了增删改操作，执行清除以更新缓存
        /// </summary>
        public static void ClearAllCache() {
            CacheMemoryData.Clear();
        }


        /// <summary>
        /// 移除所有某表的CACHE数据,增删改时使用
        /// </summary>
        /// <param name="tableName">表名</param>
        public static void RemoveAllTableCache(string tableName) {
            List<string> removes = new List<string>();
            lock (CacheMemoryData) {
                switch (CacheType)
                {
                    case CACHE_TYPE_MEMORY:
                        foreach (string sql in CacheMemoryData.Keys) {
                            if (CacheMemoryData[sql].TableName.Trim().ToUpper() == tableName.Trim().ToUpper()) {
                                removes.Add(sql);
                            }
                        }
                        foreach (string item in removes)
                        {
                            CacheMemoryData.Remove(item);
                        }
                        break;
                    case CACHE_TYPE_LOCAL:
                        break;
                    case CACHE_TYPE_NET:
                        break;
                    default:
                        break;
                }

            }
        }

        /// <summary>
        /// 判断某OrmModel表是否可以缓存
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool IsCached(DBSession session , OrmBaseModel model) {

            if (CacheSwitch == "off")
            {
                return false;
            }

            object[] attrs = model.GetType().GetCustomAttributes(typeof(OrmTableAttribute), true);
            foreach (object attr in attrs) {
                OrmTableAttribute attribute = attr as OrmTableAttribute;
                if (attribute.DbAlias == session.DbAlias && attribute.Cached) {
                    return true;
                }
            }
            if (attrs.Length == 1 ) {
                OrmTableAttribute attribute = (OrmTableAttribute)attrs[0];
                if (attribute.Cached) {
                    return true;
                }
            }
            return false;

        }


    }
}
