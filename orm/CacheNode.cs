using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class CacheNode
    {
        //Cache名
        public String CacheName { get; set; }
        //级别，table级别或者sql级别
        public String CacheLevel { get; set; }
        //类型，memory，以后可能增加redis，file等
        public String CacheType { get; set; }


        /// <summary>
        /// 获取缓存内容
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <returns></returns>
        public object GetCacheContext(string cacheKey,object cacheValue) {

            if (CacheType == "memory") {

            }



            return null;
        }

        /// <summary>
        /// 保存缓存内容
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        public void SetCacheContext(string cacheKey, object cacheValue) {

            if (CacheType == "memory")
            {

            }

        }
    }
}
