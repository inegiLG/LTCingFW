using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public enum LZModelEnum
    {
        /// <summary>
        /// 使用时查询
        /// </summary>
        LAZY = 0,
        /// <summary>
        /// 有关联即查询
        /// </summary>
        EAGER = 1,
    }
}
