using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.exceptions
{
    public class NoUpdateColumnException :LTCingFWException
    {

        public override string Message {
            get {
                return "非主键的列全部为空值，无法更新！";
            }

        }
    }
}
