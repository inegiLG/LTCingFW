using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class FwInstanceBean
    {
        public String Name { get; set; }
        public String Type { get; set; }//即FullName
        public Object Instance { get; set; }
        public String ProxyType { get; set; }
        //public BaseInstanceAttribute Attribute { get; set; }

        //构造函数
        public FwInstanceBean() { }
        public FwInstanceBean(String name) {
            Name = name;
        }
        public FwInstanceBean(String name,String type)
        {
            Name = name;
            Type = type;
        }
    }
}
