using LTCingFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class OrmColumnBean
    {
        public String ColumnName { get; set; }
        public object Value { get; set; }

        /// <summary>
        /// String是dbAliase，针对某个数据库连接存储OrmColumn信息
        /// </summary>
        public Dictionary<String, OrmColumnAttribute> OrmColumnAttributeDic { get; } = new Dictionary<string, OrmColumnAttribute>();

        
    }
}
