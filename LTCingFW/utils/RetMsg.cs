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

        public string detail { get; set; } = "";

        public object result { get; set; }

        //分页使用
        public int totalItemCount { get; set; }

        public string startTime { get; set; } = FwUtilFunc.GetNowTimeStr13();

        public string endTime { get; set; } = FwUtilFunc.GetNowTimeStr13();

        //其他附加信息
        public Dictionary<string, object> others = new Dictionary<string, object>();
    }
}
