using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class OrmForeignAttribute : Attribute
    {
        /// <summary>
        /// 标识与本表列名对应的关联表列名，中间用“,”分隔
        /// </summary>
        public string ForeignColumnName { get; set; }
        /// <summary>
        /// 标识查询所需要的本表列名，中间用“,”分隔
        /// </summary>
        public string LocalColumnName { get; set; }
        /// <summary>
        /// LAZY或EAGER查询，默认为LZAY
        /// </summary>
        public LZModelEnum LZModel { get; set; } = LZModelEnum.LAZY;
    }
}
