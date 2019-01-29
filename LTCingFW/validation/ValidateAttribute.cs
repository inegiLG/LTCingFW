using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class ValidateAttribute : Attribute
    {
        //默认的验证项目
        private ValidateEnum items;
        public ValidateEnum Items {
            get { return items; }
            set { items = value; }
        }
        //自定义正则验证
        private string regx = string.Empty;
        public string Regx {
            get { return regx; }
            set { regx = value; }
        }
        //自定义方法验证
        private string function = string.Empty;
        /// <summary>
        /// 需要 类的全路径.方法名
        /// </summary>
        public string Function_Path {
            get { return function; }
            set { function = value; }
        }
        //最大长度
        private int max_length;
        public int MaxLength {
            get { return max_length; }
            set { max_length = value; }
        }
        //最小长度
        private int min_length;
        public int MinLength
        {
            get { return min_length; }
            set { min_length = value; }
        }
    }
}
