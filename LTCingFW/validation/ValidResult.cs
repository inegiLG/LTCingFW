using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class ValidResult
    {
        /// <summary>
        /// 结果
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 被验证的值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 被验证的属性名称
        /// </summary>
        public string ProPertyName { get; set; }
    }
}
