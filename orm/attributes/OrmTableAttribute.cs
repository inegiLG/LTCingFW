using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class OrmTableAttribute : Attribute
    {
        public string DbAlias { get; set; }
        public string TableName { get; set; }
        public bool Cached {get;set;}

        public OrmTableAttribute(String tb_name) {
            TableName = tb_name;
        }
    }
}
