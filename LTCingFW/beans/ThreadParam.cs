using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.beans
{
    public class ThreadParam
    {

        public String ControllerName { get; set; }

        public String MethodName { get; set; }

        public object[] MethodParams { get; set; }

        public ThreadCallBackDelegate CallBack { get; set; }

    }
}
