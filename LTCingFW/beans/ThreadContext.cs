using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.beans
{
    /// <summary>
    /// 线程上下文
    /// </summary>
    public class ThreadContext
    {
        /// <summary>
        /// DBSession
        /// </summary>
        public DBSession DBSession { get; set; }

        
        /// <summary>
        /// 线程上下文中的错误
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// 当前进入的方法名
        /// </summary>

        public String MethodName { get; set; }
    }
}
