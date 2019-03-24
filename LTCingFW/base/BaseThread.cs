using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public abstract class BaseThread
    {
        //执行
        public bool IsOpen { get; set; } = false;
        //循环时间,默认1000ms
        public int LoopRate { set; get; } = 1000;

        public abstract void run();

    }
}
