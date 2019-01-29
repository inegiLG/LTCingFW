using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class OrmColumnAttribute : Attribute
    {
        public string ColName { get; set; }
        public int ColType { get; set; }
        public int ColSize { get; set; }
        public string DbAlias { get; set; }
        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool PrimaryKey { get; set; }

        public OrmColumnAttribute(String colName,int colType) {
            ColName = colName;
            ColType = colType;
        }
    }
}
