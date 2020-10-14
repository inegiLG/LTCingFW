using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public class LTCingFWException : Exception
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        private string _error_msg;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="error_code">错误代码</param>
        /// <param name="sxrj_msg">错误信息</param>
        /// <param name="innerException">内部错误类</param>

        public LTCingFWException(String error_msg, Exception innerException) : base(error_msg, innerException)
        {
            this._error_msg = error_msg + innerException.Message;
        }
        public LTCingFWException(String error_msg) : base(error_msg)
        {
        }
        public LTCingFWException(): base () { }
        /// <summary>
        /// 获取信息
        /// </summary>
        public override string Message
        {
            get {
                if (this._error_msg == "" || this._error_msg == null)
                {
                    return base.Message;
                }
                return _error_msg + base.Message;
            }
        }

        public override string StackTrace => base.StackTrace;


    }


}
