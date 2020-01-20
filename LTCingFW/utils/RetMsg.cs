using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.utils
{
    public class RetMsg
    {

        public string code { get; set; } = "0";

        public string message { get; set; } = "操作成功";

        public string detail { get; set; }

        public object result { get; set; }


    }
}
