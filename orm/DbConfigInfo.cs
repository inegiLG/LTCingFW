using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class DbConfigInfo
    {
        public String DataBaseType { set; get; }
        public String DataSource { set; get; }
        public String UserId { set; get; }
        public String Password { set; get; }
        public String TimeOut { set; get; }
        public String Pooling { set; get; }
        public String MaxSize { set; get; }
        public String MinSize { set; get; }
        public String IncrSize { set; get; }
        public String DecrSize { set; get; }
    }
}
