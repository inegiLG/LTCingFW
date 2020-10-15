using LTCingFW.beans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public delegate void ThreadCallBackDelegate(object param);
    public abstract class BaseThread
    {
        
        public bool IsOpen { get; set; } = false;
        
        public int LoopRate { set; get; } = 1000;//循环时间,默认1000ms

        public StateLight ThreadLight { get; set; } = new StateLight();

        public abstract void run(object param);

    }
}
