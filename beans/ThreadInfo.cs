using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LTCingFW.beans
{
    /// <summary>
    /// 绑定线程运行环境和线程
    /// </summary>
    public class ThreadInfo
    {
        public Thread Thread { get; set; }
        public Object ContextObject { get; set; }
        public string ErrorMsg { get; set; }

        public ThreadInfo(Thread thread)
        {
            Thread = thread;
        }
        public ThreadInfo(Thread thread, Object context)
        {
            ContextObject = context;
        }


    }
}
